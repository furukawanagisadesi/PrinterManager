using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using PrinterManager.Models;

namespace PrinterManager.Core
{
    /// <summary>
    /// 打印机驱动操作：枚举、删除驱动（含关联文件）
    /// </summary>
    public static class DriverOperations
    {
        /// <summary>
        /// 枚举本机已安装的所有打印机驱动（版本3和版本4）
        /// </summary>
        public static List<DriverInfo> EnumerateDrivers()
        {
            var list = new List<DriverInfo>();

            uint needed = 0,
                returned = 0;
            // 先获取缓冲区大小
            PrinterApiWrapper.EnumPrinterDrivers(
                null,
                null,
                3,
                IntPtr.Zero,
                0,
                ref needed,
                ref returned
            );

            if (needed == 0)
                return list;

            IntPtr buf = Marshal.AllocHGlobal((int)needed);
            try
            {
                if (
                    !PrinterApiWrapper.EnumPrinterDrivers(
                        null,
                        null,
                        3,
                        buf,
                        needed,
                        ref needed,
                        ref returned
                    )
                )
                {
                    int err = Marshal.GetLastWin32Error();
                    // ERROR_INSUFFICIENT_BUFFER = 122，重新尝试
                    if (err == 122)
                    {
                        Marshal.FreeHGlobal(buf);
                        buf = Marshal.AllocHGlobal((int)needed);
                        if (
                            !PrinterApiWrapper.EnumPrinterDrivers(
                                null,
                                null,
                                3,
                                buf,
                                needed,
                                ref needed,
                                ref returned
                            )
                        )
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    else
                    {
                        throw new Win32Exception(err);
                    }
                }

                int structSize = Marshal.SizeOf(typeof(PrinterApiWrapper.DRIVER_INFO_3));
                for (int i = 0; i < returned; i++)
                {
                    IntPtr ptr = new IntPtr(buf.ToInt64() + i * structSize);
                    var info = (PrinterApiWrapper.DRIVER_INFO_3)
                        Marshal.PtrToStructure(ptr, typeof(PrinterApiWrapper.DRIVER_INFO_3));

                    // 避免重复（同驱动多版本）
                    if (
                        !list.Exists(d =>
                            string.Equals(d.Name, info.pName, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(
                                d.Environment,
                                info.pEnvironment,
                                StringComparison.OrdinalIgnoreCase
                            )
                            && d.Version == info.cVersion
                        )
                    )
                    {
                        list.Add(
                            new DriverInfo
                            {
                                Name = info.pName,
                                Environment = info.pEnvironment,
                                DriverPath = info.pDriverPath,
                                DataFile = info.pDataFile,
                                ConfigFile = info.pConfigFile,
                                Version = info.cVersion,
                            }
                        );
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }

            list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            return list;
        }

        /// <summary>
        /// 删除打印机驱动（同时删除关联文件）
        /// </summary>
        /// <param name="driverName">驱动名称</param>
        /// <param name="environment">环境，如 "Windows x64"，null 表示当前系统</param>
        /// <param name="deleteFiles">是否同时删除驱动文件</param>
        public static void DeleteDriver(
            string driverName,
            string environment = null,
            bool deleteFiles = true
        )
        {
            uint deleteFlag = deleteFiles
                ? PrinterApiWrapper.DPD_DELETE_ALL_FILES | PrinterApiWrapper.DPD_DELETE_UNUSED_FILES
                : 0;

            // 尝试用 DeletePrinterDriverEx（支持删文件）
            bool ok = PrinterApiWrapper.DeletePrinterDriverEx(
                null,
                environment,
                driverName,
                deleteFlag,
                0
            );

            if (!ok)
            {
                int err = Marshal.GetLastWin32Error();
                // 某些情况下 DeletePrinterDriverEx 不可用，回退到 DeletePrinterDriver
                if (err == 50 || err == 1 || err == 87)
                {
                    ok = PrinterApiWrapper.DeletePrinterDriver(null, environment, driverName);
                    if (!ok)
                        throw new Win32Exception(
                            Marshal.GetLastWin32Error(),
                            $"删除驱动 \"{driverName}\" 失败"
                        );
                }
                else
                {
                    throw new Win32Exception(
                        err,
                        $"删除驱动 \"{driverName}\" 失败（错误码 {err}）"
                    );
                }
            }
        }

        /// <summary>
        /// 删除驱动的所有版本（version 3 和 version 4）
        /// </summary>
        public static List<string> DeleteDriverAllVersions(
            string driverName,
            string environment = null,
            bool deleteFiles = true
        )
        {
            var errors = new List<string>();

            // 先在 Spooler 运行时收集驱动文件路径（停了之后就查不到了）
            List<string> driverFilePaths = new List<string>();
            if (deleteFiles)
                driverFilePaths = CollectDriverFilePaths(driverName);

            // 1. 停止 Spooler
            ServiceControllerStatus originalStatus;
            try
            {
                originalStatus = StopSpooler();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "停止 Print Spooler 服务失败，无法删除驱动。\n" + ex.Message,
                    ex
                );
            }

            try
            {
                // 2. 直接删注册表中的驱动记录
                DeleteDriverFromRegistry(driverName, errors);

                // 3. 删除驱动文件
                if (deleteFiles)
                    DeleteDriverFiles(driverFilePaths, errors);
            }
            finally
            {
                // 4. 重启 Spooler
                if (originalStatus != ServiceControllerStatus.Stopped)
                {
                    try
                    {
                        StartSpooler();
                    }
                    catch (Exception ex)
                    {
                        errors.Add("⚠ Print Spooler 重启失败，请手动启动：" + ex.Message);
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Spooler 运行时，通过注册表读取驱动文件路径列表
        /// </summary>
        private static List<string> CollectDriverFilePaths(string driverName)
        {
            var paths = new List<string>();
            // 驱动文件记录在两个位置（Version 3 / Version 4）
            string[] regRoots =
            {
                @"SYSTEM\CurrentControlSet\Control\Print\Environments\Windows x64\Drivers\Version-3",
                @"SYSTEM\CurrentControlSet\Control\Print\Environments\Windows x64\Drivers\Version-4",
                @"SYSTEM\CurrentControlSet\Control\Print\Environments\Windows NT x86\Drivers\Version-3",
            };

            foreach (string root in regRoots)
            {
                try
                {
                    using (
                        var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                            root + @"\" + driverName
                        )
                    )
                    {
                        if (key == null)
                            continue;

                        // 收集所有文件路径值
                        string[] valueNames =
                        {
                            "Driver",
                            "ConfigFile",
                            "DataFile",
                            "HelpFile",
                            "DependentFiles",
                        };
                        foreach (string vn in valueNames)
                        {
                            object val = key.GetValue(vn);
                            if (val == null)
                                continue;

                            if (val is string s && !string.IsNullOrEmpty(s))
                                paths.Add(ExpandDriverPath(s));
                            else if (val is string[] arr)
                                foreach (string f in arr)
                                    if (!string.IsNullOrEmpty(f))
                                        paths.Add(ExpandDriverPath(f));
                        }
                    }
                }
                catch { }
            }

            return paths;
        }

        /// <summary>
        /// 驱动路径可能是相对路径（如 EPSONL8.DLL），展开为完整路径
        /// </summary>
        private static string ExpandDriverPath(string path)
        {
            if (System.IO.Path.IsPathRooted(path))
                return path;

            // 默认驱动目录
            string driverDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "spool",
                "drivers",
                "x64",
                "3"
            );

            return System.IO.Path.Combine(driverDir, path);
        }

        /// <summary>
        /// 直接从注册表删除驱动记录（Spooler 停止后使用）
        /// </summary>
        private static void DeleteDriverFromRegistry(string driverName, List<string> errors)
        {
            string[] regRoots =
            {
                @"SYSTEM\CurrentControlSet\Control\Print\Environments\Windows x64\Drivers\Version-3",
                @"SYSTEM\CurrentControlSet\Control\Print\Environments\Windows x64\Drivers\Version-4",
                @"SYSTEM\CurrentControlSet\Control\Print\Environments\Windows NT x86\Drivers\Version-3",
            };

            foreach (string root in regRoots)
            {
                try
                {
                    using (
                        var parentKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                            root,
                            writable: true
                        )
                    )
                    {
                        if (parentKey == null)
                            continue;

                        // 检查子键是否存在
                        bool exists = Array.Exists(
                            parentKey.GetSubKeyNames(),
                            n => string.Equals(n, driverName, StringComparison.OrdinalIgnoreCase)
                        );

                        if (!exists)
                            continue;

                        parentKey.DeleteSubKeyTree(driverName, throwOnMissingSubKey: false);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"注册表删除失败 [{root}\\{driverName}]: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 删除驱动相关文件（忽略系统共用文件的删除失败）
        /// </summary>
        private static void DeleteDriverFiles(List<string> paths, List<string> errors)
        {
            foreach (string path in paths)
            {
                if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
                    continue;

                try
                {
                    System.IO.File.Delete(path);
                }
                catch (Exception ex)
                {
                    // 文件被其他驱动共用时删除会失败，记录警告但不中断
                    errors.Add(
                        $"文件删除失败（可能被其他驱动共用）: {System.IO.Path.GetFileName(path)} - {ex.Message}"
                    );
                }
            }
        }

        private const string SpoolerServiceName = "Spooler";
        private const int SpoolerStopTimeoutMs = 15000;
        private const int SpoolerStartTimeoutMs = 20000;

        private static ServiceControllerStatus StopSpooler()
        {
            using (var svc = new ServiceController(SpoolerServiceName))
            {
                ServiceControllerStatus originalStatus = svc.Status;

                if (svc.Status == ServiceControllerStatus.Stopped)
                    return originalStatus;

                if (svc.Status == ServiceControllerStatus.StartPending)
                    svc.WaitForStatus(
                        ServiceControllerStatus.Running,
                        TimeSpan.FromMilliseconds(SpoolerStartTimeoutMs)
                    );

                if (svc.Status != ServiceControllerStatus.Stopped)
                {
                    svc.Stop();
                    svc.WaitForStatus(
                        ServiceControllerStatus.Stopped,
                        TimeSpan.FromMilliseconds(SpoolerStopTimeoutMs)
                    );
                }

                if (svc.Status != ServiceControllerStatus.Stopped)
                    throw new InvalidOperationException(
                        "Print Spooler 服务无法在规定时间内停止，操作已取消。"
                    );

                return originalStatus;
            }
        }

        private static void StartSpooler()
        {
            using (var svc = new ServiceController(SpoolerServiceName))
            {
                if (svc.Status == ServiceControllerStatus.Running)
                    return;

                svc.Start();
                svc.WaitForStatus(
                    ServiceControllerStatus.Running,
                    TimeSpan.FromMilliseconds(SpoolerStartTimeoutMs)
                );
            }
        }
    }
}
