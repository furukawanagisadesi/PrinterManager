using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using PrinterManager.Models;

namespace PrinterManager.Core
{
    /// <summary>
    /// 打印机驱动操作：枚举、删除驱动（含关联文件）
    /// </summary>
    public static class DriverOperations
    {
        /// <summary>
        /// 进程执行结果（替代 ValueTuple）
        /// </summary>
        public class ProcessResult
        {
            public bool Success { get; }
            public string Output { get; }

            public ProcessResult(bool success, string output)
            {
                Success = success;
                Output = output;
            }

            public void Deconstruct(out bool success, out string output)
            {
                success = Success;
                output = Output;
            }
        }

        // ══════════════════════════════════════════════════════════════
        // 枚举驱动
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 枚举本机已安装的所有打印机驱动（版本3和版本4）
        /// </summary>
        public static List<DriverInfo> EnumerateDrivers()
        {
            var list = new List<DriverInfo>();
            EnumerateDriversLevel3(list);
            EnumerateDriversLevel4(list);
            list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            return list;
        }

        private static void EnumerateDriversLevel3(List<DriverInfo> list)
        {
            uint needed = 0,
                returned = 0;
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
                return;

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
                    if (err == 122) // ERROR_INSUFFICIENT_BUFFER
                    {
                        // 先分配新缓冲区，再释放旧缓冲区，避免分配失败导致 double-free
                        IntPtr newBuf = Marshal.AllocHGlobal((int)needed);
                        Marshal.FreeHGlobal(buf);
                        buf = newBuf;
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
        }

        private static void EnumerateDriversLevel4(List<DriverInfo> list)
        {
            // V4 驱动（Win8.1+），旧系统不支持时静默忽略
            uint needed = 0,
                returned = 0;
            PrinterApiWrapper.EnumPrinterDrivers(
                null,
                null,
                4,
                IntPtr.Zero,
                0,
                ref needed,
                ref returned
            );
            if (needed == 0)
                return;

            IntPtr buf = Marshal.AllocHGlobal((int)needed);
            try
            {
                if (
                    !PrinterApiWrapper.EnumPrinterDrivers(
                        null,
                        null,
                        4,
                        buf,
                        needed,
                        ref needed,
                        ref returned
                    )
                )
                {
                    int err = Marshal.GetLastWin32Error();
                    if (err == 122)
                    {
                        // 先分配新缓冲区，再释放旧缓冲区，避免分配失败导致 double-free
                        IntPtr newBuf = Marshal.AllocHGlobal((int)needed);
                        Marshal.FreeHGlobal(buf);
                        buf = newBuf;
                        if (
                            !PrinterApiWrapper.EnumPrinterDrivers(
                                null,
                                null,
                                4,
                                buf,
                                needed,
                                ref needed,
                                ref returned
                            )
                        )
                            return;
                    }
                    else
                    {
                        return;
                    }
                }

                int structSize = Marshal.SizeOf(typeof(PrinterApiWrapper.DRIVER_INFO_4));
                for (int i = 0; i < returned; i++)
                {
                    IntPtr ptr = new IntPtr(buf.ToInt64() + i * structSize);
                    var info = (PrinterApiWrapper.DRIVER_INFO_4)
                        Marshal.PtrToStructure(ptr, typeof(PrinterApiWrapper.DRIVER_INFO_4));

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
        }

        // ══════════════════════════════════════════════════════════════
        // 删除驱动（Win32 API）
        // ══════════════════════════════════════════════════════════════

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
            // DPD_DELETE_ALL_FILES 与 DPD_DELETE_UNUSED_FILES 语义互斥，只用其中一个。
            // DPD_DELETE_UNUSED_FILES 更安全：只删没有被其他驱动引用的文件
            uint deleteFlag = deleteFiles ? PrinterApiWrapper.DPD_DELETE_UNUSED_FILES : 0;

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
                // 仅在 ERROR_NOT_SUPPORTED (50) 时回退到旧 API：
                // err==1 (ERROR_INVALID_FUNCTION) 和 err==87 (ERROR_INVALID_PARAMETER)
                // 表示参数有误，回退旧 API 同样会失败，应直接抛出原始错误。
                if (err == 50)
                {
                    ok = PrinterApiWrapper.DeletePrinterDriver(null, environment, driverName);
                    if (!ok)
                        throw new Win32Exception(
                            Marshal.GetLastWin32Error(),
                            string.Format("删除驱动 \"{0}\" 失败", driverName)
                        );
                }
                else
                {
                    throw new Win32Exception(
                        err,
                        string.Format("删除驱动 \"{0}\" 失败（错误码 {1}）", driverName, err)
                    );
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        // pnputil 包装
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 检测指定驱动名称是否已安装
        /// </summary>
        public static bool CheckDriverInstalled(string driverName)
        {
            var drivers = EnumerateDrivers();
            return drivers.Exists(d =>
                string.Equals(d.Name, driverName, StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <summary>
        /// 通过 pnputil 安装驱动包（.inf）
        /// </summary>
        public static ProcessResult RunPnputilAddDriver(string infPath)
        {
            if (!File.Exists(infPath))
                throw new FileNotFoundException(string.Format("驱动包文件不存在: {0}", infPath));

            return RunProcess(
                "pnputil.exe",
                string.Format("/add-driver \"{0}\" /install", infPath)
            );
        }

        /// <summary>
        /// 通过 pnputil 删除驱动包
        /// </summary>
        public static ProcessResult RunPnputilDeleteDriver(string publishedName)
        {
            return RunProcess(
                "pnputil.exe",
                string.Format("/delete-driver \"{0}\" /force", publishedName)
            );
        }

        /// <summary>
        /// 通用进程执行辅助方法。
        /// 用两个并行 Task 读取 stdout/stderr，彻底避免缓冲区死锁。
        /// </summary>
        private static ProcessResult RunProcess(string fileName, string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(psi))
            {
                var outputTask = Task.Factory.StartNew(() => process.StandardOutput.ReadToEnd());
                var errorTask = Task.Factory.StartNew(() => process.StandardError.ReadToEnd());

                bool exited = process.WaitForExit(60000);
                if (!exited)
                {
                    process.Kill();
                    throw new System.TimeoutException(
                        string.Format("进程 '{0}' 执行超过 60 秒，已强制终止。", fileName)
                    );
                }

                // 在 .NET Framework 上，WaitForExit(timeout) 不保证异步重定向管道已刷新完毕；
                // 必须再调用一次无参 WaitForExit() 以确保 stdout/stderr 全部读取完成。
                process.WaitForExit();

                string output = outputTask.Result;
                string error = errorTask.Result;

                string fullOutput = output;
                if (!string.IsNullOrEmpty(error))
                    fullOutput += "\n[Error]\n" + error;

                return new ProcessResult(process.ExitCode == 0, fullOutput);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // INF 解析
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 从 INF 文件解析真实的打印机驱动名称
        /// </summary>
        public static string ParseDriverNameFromInf(string infPath)
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(infPath);
            }
            catch
            {
                return Path.GetFileNameWithoutExtension(infPath);
            }

            var strings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var manufacturerSections = new List<string>();
            string currentSection = null;
            bool inStrings = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith(";"))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.TrimStart('[').TrimEnd(']').Trim();
                    inStrings = currentSection.Equals(
                        "Strings",
                        StringComparison.OrdinalIgnoreCase
                    );
                    continue;
                }

                if (inStrings && line.Contains("="))
                {
                    int eq = line.IndexOf("=");
                    string k = line.Substring(0, eq).Trim().Trim('%');
                    string v = line.Substring(eq + 1).Trim().Trim('"');
                    if (!string.IsNullOrEmpty(k) && !string.IsNullOrEmpty(v))
                        strings[k] = v;
                }

                if (
                    currentSection != null
                    && currentSection.StartsWith("Manufacturer", StringComparison.OrdinalIgnoreCase)
                    && !inStrings
                    && line.Contains("=")
                )
                {
                    int eq = line.IndexOf("=");
                    string right = line.Substring(eq + 1).Trim().Trim('"');
                    if (!string.IsNullOrEmpty(right))
                        manufacturerSections.Add(right);
                }
            }

            foreach (var mfgSecBase in manufacturerSections)
            {
                currentSection = null;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith(";"))
                        continue;
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        currentSection = line.TrimStart('[').TrimEnd(']').Trim();
                        continue;
                    }
                    bool inSection =
                        currentSection != null
                        && (
                            currentSection.Equals(mfgSecBase, StringComparison.OrdinalIgnoreCase)
                            || currentSection.StartsWith(
                                mfgSecBase + ".",
                                StringComparison.OrdinalIgnoreCase
                            )
                        );
                    if (inSection && line.Contains("="))
                    {
                        int eq = line.IndexOf("=");
                        string left = line.Substring(0, eq).Trim();
                        if (left.StartsWith("\"") && left.EndsWith("\""))
                            return left.Trim('"');
                        if (left.StartsWith("%") && left.EndsWith("%"))
                        {
                            string key = left.Trim('%');
                            string r;
                            if (strings.TryGetValue(key, out r))
                                return r;
                        }
                    }
                }
            }

            return Path.GetFileNameWithoutExtension(infPath);
        }

        // ══════════════════════════════════════════════════════════════
        // 安装驱动
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 完整安装驱动流程
        /// </summary>
        /// <param name="infPath">驱动 .inf 文件路径</param>
        /// <param name="driverName">驱动名称</param>
        public static void InstallDriver(string infPath, string driverName)
        {
            if (CheckDriverInstalled(driverName))
                throw new InvalidOperationException(
                    string.Format("驱动 \"{0}\" 已安装，无需重复安装。", driverName)
                );

            var result = RunPnputilAddDriver(infPath);
            if (!result.Success)
                throw new InvalidOperationException(
                    string.Format("pnputil 安装驱动包失败:\n{0}", result.Output)
                );

            AddDriverViaApi(driverName);
        }

        /// <summary>
        /// 通过 Windows API AddPrinterDriver 注册驱动
        /// </summary>
        private static void AddDriverViaApi(string driverName)
        {
            var drivers = EnumerateDrivers();
            var driver = drivers.Find(d =>
                string.Equals(d.Name, driverName, StringComparison.OrdinalIgnoreCase)
            );

            if (driver == null)
                throw new InvalidOperationException(
                    string.Format("驱动 \"{0}\" 未通过 pnputil 成功安装到系统中。", driverName)
                );

            var drvInfo = new PrinterApiWrapper.DRIVER_INFO_3
            {
                cVersion = driver.Version,
                pName = driver.Name,
                pEnvironment = driver.Environment,
                pDriverPath = driver.DriverPath,
                pDataFile = driver.DataFile,
                pConfigFile = driver.ConfigFile,
            };

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(drvInfo));
            try
            {
                Marshal.StructureToPtr(drvInfo, ptr, false);
                if (!PrinterApiWrapper.AddPrinterDriver(null, 3, ptr))
                {
                    int err = Marshal.GetLastWin32Error();
                    // ERROR_PRINTER_DRIVER_ALREADY_INSTALLED = 1795，可忽略
                    if (err != 1795)
                        throw new Win32Exception(
                            err,
                            string.Format("AddPrinterDriver 注册驱动 \"{0}\" 失败", driverName)
                        );
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // 注册表查找 / Published Name 查找
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 通过 PowerShell 查找驱动对应的 Published Name（oemNN.inf）。
        /// 先解析 pnputil /enum-drivers 输出构建原始 INF 名 → 发布名映射，
        /// 再通过 Get-PrinterDriver 获取 INF 文件名进行匹配。
        /// </summary>
        public static string FindPublishedName(string driverName)
        {
            string psScript = string.Format(
                @"
$ProgressPreference = 'SilentlyContinue'
$WarningPreference  = 'SilentlyContinue'
$driverMap = @{{}}
$current = @{{}}
pnputil /enum-drivers | ForEach-Object {{
    $line = $_.Trim()
    if ($line -match '^Published Name\s*:\s*(.+)') {{
        $current['Published'] = $Matches[1].Trim()
    }} elseif ($line -match '^Original Name\s*:\s*(.+)') {{
        $current['Original'] = $Matches[1].Trim()
    }} elseif ($line -eq '') {{
        if ($current['Original'] -and $current['Published']) {{
            $driverMap[$current['Original']] = $current['Published']
        }}
        $current = @{{}}
    }}
}}
# 处理最后一条记录（文件末尾无空行时）
if ($current['Original'] -and $current['Published']) {{
    $driverMap[$current['Original']] = $current['Published']
}}

$driver = Get-PrinterDriver -Name '{0}' -ErrorAction SilentlyContinue
if ($driver -and $driver.InfPath) {{
    $infFileName = Split-Path $driver.InfPath -Leaf
    $pub = $driverMap[$infFileName]
    if ($pub) {{
        # 使用明确前缀，让 C# 侧精确提取，避免混入其他输出
        Write-Output ""PUBLISHED_NAME:$pub""
    }}
}}
",
                driverName.Replace("'", "''")
            );

            var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(psScript));

            var result = RunProcess(
                "powershell.exe",
                string.Format("-NoProfile -ExecutionPolicy Bypass -EncodedCommand {0}", encoded)
            );

            if (result.Output != null)
            {
                foreach (string line in result.Output.Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("PUBLISHED_NAME:", StringComparison.Ordinal))
                    {
                        string name = trimmed.Substring("PUBLISHED_NAME:".Length).Trim();
                        // 校验格式：必须是 oem<数字>.inf，防止脏数据传入 pnputil
                        if (!string.IsNullOrEmpty(name)
                            && name.StartsWith("oem", StringComparison.OrdinalIgnoreCase)
                            && name.EndsWith(".inf", StringComparison.OrdinalIgnoreCase))
                            return name;
                    }
                }
            }

            return null;
        }

        // ══════════════════════════════════════════════════════════════
        // Spooler 服务管理
        // ══════════════════════════════════════════════════════════════

        private const string SpoolerServiceName = "Spooler";
        private const int SpoolerTimeoutMs = 15000;

        /// <summary>
        /// 停止后台打印服务，返回是否原本正在运行（用于后续恢复）
        /// </summary>
        private static bool StopSpoolerIfRunning()
        {
            using (var svc = new ServiceController(SpoolerServiceName))
            {
                bool wasRunning =
                    svc.Status == ServiceControllerStatus.Running
                    || svc.Status == ServiceControllerStatus.StartPending;

                if (wasRunning)
                {
                    svc.Stop();
                    svc.WaitForStatus(
                        ServiceControllerStatus.Stopped,
                        TimeSpan.FromMilliseconds(SpoolerTimeoutMs)
                    );
                }
                return wasRunning;
            }
        }

        /// <summary>
        /// 恢复后台打印服务，失败时抛出异常（由调用方决定如何处理）
        /// </summary>
        private static void StartSpooler()
        {
            using (var svc = new ServiceController(SpoolerServiceName))
            {
                if (
                    svc.Status != ServiceControllerStatus.Running
                    && svc.Status != ServiceControllerStatus.StartPending
                )
                {
                    svc.Start();
                    svc.WaitForStatus(
                        ServiceControllerStatus.Running,
                        TimeSpan.FromMilliseconds(SpoolerTimeoutMs)
                    );
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        // 终止占用驱动文件的进程
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 枚举所有加载了 spool\drivers 下 DLL 的进程（除 Spooler 外），尝试终止。
        /// </summary>
        private static void KillProcessesHoldingSpoolDriverFiles()
        {
            string spoolDriversDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                    @"spool\drivers"
                )
                .ToLowerInvariant();

            int currentPid = Process.GetCurrentProcess().Id;

            var targetProcs = new List<Process>();

            foreach (var proc in Process.GetProcesses())
            {
                if (proc.Id == currentPid)
                    continue;

                try
                {
                    bool matched = false;
                    foreach (ProcessModule module in proc.Modules)
                    {
                        string filePath = module.FileName.ToLowerInvariant();
                        if (filePath.StartsWith(spoolDriversDir, StringComparison.Ordinal))
                        {
                            matched = true;
                            break;
                        }
                    }

                    if (matched)
                        targetProcs.Add(proc);
                }
                catch
                {
                    // 访问被拒绝（如 SYSTEM 进程）或进程已退出，跳过
                }
            }

            foreach (var proc in targetProcs)
            {
                if (proc.HasExited)
                    continue;

                string procInfo = string.Format("{0}(PID={1})", proc.ProcessName, proc.Id);
                try
                {
                    proc.Kill();
                    if (!proc.WaitForExit(5000))
                        Debug.WriteLine(string.Format("进程 {0} 未能在5秒内终止", procInfo));
                    else
                        Debug.WriteLine(string.Format("进程 {0} 已终止", procInfo));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("终止进程 {0} 失败: {1}", procInfo, ex.Message));
                }
                finally
                {
                    proc.Dispose();
                }
            }
        }

        /// <summary>
        /// 清空 spool\PRINTERS 目录下的残留队列文件（.SHD / .SPL）。
        ///
        /// 必须在 Spooler 停止后调用。
        /// 残留的队列文件会在 Spooler 重启时被重新加载，Spooler 会重新持有驱动文件句柄，
        /// 导致随后的 DeletePrinterDriverEx 仍然返回 3001。
        /// </summary>
        private static void ClearSpoolPrinterFiles()
        {
            string spoolPrintersDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                @"spool\PRINTERS"
            );

            if (!Directory.Exists(spoolPrintersDir))
                return;

            foreach (string file in Directory.GetFiles(spoolPrintersDir))
            {
                // 只删除打印队列相关文件，避免意外删除其他文件
                string ext = Path.GetExtension(file);
                if (!string.Equals(ext, ".SPL", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(ext, ".SHD", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    File.Delete(file);
                    Debug.WriteLine(string.Format("已删除队列文件: {0}", file));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("删除队列文件失败 {0}: {1}", file, ex.Message));
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        // 增强版卸载入口（混合模式）
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 混合卸载驱动：PowerShell 获取 INF 路径 + C# 读取 INF +
        /// Win32 API 卸载 + pnputil 清理 + 手动删除驱动文件。
        ///
        /// 完整流程：
        ///   1.  PowerShell：删除打印机对象 + 获取驱动 INF 路径
        ///   2.  C#：读取 INF 文件（解析 Version 信息）
        ///   3.  停止 Spooler
        ///   4.  清空 spool\PRINTERS 队列残留文件
        ///   5.  终止占用驱动文件的进程
        ///   6.  临时启动 Spooler → Win32 API（DeletePrinterDriverEx）卸载驱动
        ///   7.  pnputil /delete-driver 清理 PnP 驱动包数据库
        ///   8.  手动删除 Driver Store 目录
        ///   9.  清理注册表键
        ///  10.  恢复 Spooler
        /// </summary>
        /// <param name="driverName">驱动名称</param>
        /// <param name="publishedName">已废弃，保留仅为兼容</param>
        /// <param name="catalogDir">已废弃，保留仅为兼容</param>
        public static List<string> UninstallDriverEnhanced(
            string driverName,
            string publishedName = null,
            string catalogDir = null
        )
        {
            var errors = new List<string>();

            // ──────────────────────────────────────────────────────────
            // Phase 1: PowerShell — 删除打印机 + 获取 INF 路径
            // ──────────────────────────────────────────────────────────
            string psScript = string.Format(
                @"
$ProgressPreference = 'SilentlyContinue'
$driverName = '{0}'

# 1. 删除所有使用该驱动的打印机对象
Get-Printer | Where-Object {{ $_.DriverName -eq $driverName }} | ForEach-Object {{
    Remove-Printer -Name $_.Name -Confirm:$false -ErrorAction SilentlyContinue
}}

# 2. 获取驱动 INF 路径（供 C# 后续处理）
$driver = Get-PrinterDriver -Name $driverName -ErrorAction SilentlyContinue
if ($driver -and $driver.InfPath) {{
    Write-Output ""INF_PATH:$($driver.InfPath)""
}}
",
                driverName.Replace("'", "''")
            );

            var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(psScript));

            var psResult = RunProcess(
                "powershell.exe",
                string.Format("-NoProfile -ExecutionPolicy Bypass -EncodedCommand {0}", encoded)
            );

            // 解析 INF 路径
            string infPath = null;
            if (psResult.Output != null)
            {
                foreach (
                    string line in psResult.Output.Split(
                        new[] { '\r', '\n' },
                        StringSplitOptions.RemoveEmptyEntries
                    )
                )
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("INF_PATH:"))
                    {
                        infPath = trimmed.Substring("INF_PATH:".Length).Trim();
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(infPath))
            {
                errors.Add(
                    string.Format(
                        "未找到驱动 \"{0}\" 的 INF 文件路径。PowerShell 输出:\n{1}",
                        driverName,
                        psResult.Output ?? "(null)"
                    )
                );
                // 即使无 INF 路径，仍继续尝试 Win32 API 卸载
            }

            // ──────────────────────────────────────────────────────────
            // Phase 2: C# 读取 INF 文件 → 确定驱动版本 (V3/V4)
            // ──────────────────────────────────────────────────────────
            string driverVersion = null; // "3" 或 "4"
            if (!string.IsNullOrEmpty(infPath) && File.Exists(infPath))
            {
                try
                {
                    var lines = File.ReadAllLines(infPath);
                    bool inVersion = false;
                    bool hasPrinterPackageSection = false;
                    foreach (string line in lines)
                    {
                        string trim = line.Trim();
                        // 检测 [PrinterPackageInstallation] section 是 V4 驱动的标志
                        if (trim.StartsWith("[PrinterPackageInstallation", StringComparison.OrdinalIgnoreCase))
                        {
                            hasPrinterPackageSection = true;
                        }
                        if (trim.Equals("[Version]", StringComparison.OrdinalIgnoreCase))
                        {
                            inVersion = true;
                            continue;
                        }
                        if (inVersion)
                        {
                            if (trim.StartsWith("["))
                                break; // 下一段落
                            if (trim.StartsWith("Signature", StringComparison.OrdinalIgnoreCase))
                            {
                                string val = trim.Substring(trim.IndexOf('=') + 1).Trim();
                                // "$CHICAGO$" 是 Win9x/早期驱动，"$Windows NT$" 是现代驱动（V3/V4 均使用）
                                // V4 特征应通过 [PrinterPackageInstallation] section 或 ClassVer 字段判断
                                if (val.IndexOf("CHICAGO", StringComparison.OrdinalIgnoreCase) >= 0)
                                    driverVersion = "3"; // Win9x 签名，必为 V3
                                // "$Windows NT$" 不足以区分 V3/V4，继续扫描后续 section
                            }
                        }
                    }
                    // 如果有 [PrinterPackageInstallation] section，确定为 V4
                    if (hasPrinterPackageSection)
                        driverVersion = "4";
                    else if (driverVersion == null)
                        driverVersion = "3"; // 默认 V3（最常见）
                }
                catch (Exception ex)
                {
                    errors.Add(string.Format("读取 INF 文件失败: {0}", ex.Message));
                }
            }

            // ──────────────────────────────────────────────────────────
            // Phase 3: Win32 API 卸载 + 文件/注册表清理
            // ──────────────────────────────────────────────────────────
            bool spoolerWasRunning = false;

            try
            {
                // 3a. 停止 Spooler（以便清空 PRINTERS 目录和释放文件锁定）
                spoolerWasRunning = StopSpoolerIfRunning();

                // 3b. 清空 spool\PRINTERS
                ClearSpoolPrinterFiles();

                // 3c. 终止占用驱动文件的进程
                KillProcessesHoldingSpoolDriverFiles();

                // 3d. Win32 API 卸载驱动（DeletePrinterDriverEx 需要 Spooler 运行）
                //     临时启动 Spooler 以便 API 调用成功
                try
                {
                    StartSpooler();
                }
                catch (Exception ex)
                {
                    errors.Add(string.Format("临时启动 Spooler 失败: {0}", ex.Message));
                }

                // 3d.1 查找 pnputil 驱动包 Published Name（必须在 DeletePrinterDriverEx 之前获取，
                //      因为 Get-PrinterDriver 依赖 Spooler 数据库中的驱动记录）
                string pnputilPublishedName = null;
                try
                {
                    pnputilPublishedName = FindPublishedName(driverName);
                }
                catch (Exception ex)
                {
                    errors.Add(string.Format("查找驱动包 Published Name 失败: {0}", ex.Message));
                }

                try
                {
                    DeleteDriver(driverName, deleteFiles: true);
                }
                catch (Exception ex)
                {
                    errors.Add(string.Format("Win32 API 卸载驱动失败: {0}", ex.Message));
                }

                // 3d.2 pnputil 删除驱动包（清理 PnP 驱动数据库）
                //      Win32 DeletePrinterDriverEx 仅清理 Spooler 层注册记录，
                //      pnputil 维护独立的 PnP 驱动包数据库（HKLM\SOFTWARE\Microsoft\
                //      Windows\CurrentVersion\Setup\PnpDriverDatabase）。
                //      必须同步清理，否则重装时 pnputil /add-driver 可能因数据库残留
                //      而静默跳过或部分安装，导致 spool\drivers 中有文件但 Spooler
                //      枚举不到驱动（"打印服务器属性"中不显示）。
                try
                {
                    if (!string.IsNullOrEmpty(pnputilPublishedName))
                    {
                        var pnpResult = RunPnputilDeleteDriver(pnputilPublishedName);
                        if (!pnpResult.Success)
                            errors.Add(
                                string.Format("pnputil 删除驱动包失败: {0}", pnpResult.Output)
                            );
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(string.Format("pnputil 清理驱动包异常: {0}", ex.Message));
                }

                // 再次停止 Spooler 以进行文件/注册表清理
                try
                {
                    StopSpoolerIfRunning();
                }
                catch (Exception ex)
                {
                    errors.Add(string.Format("再次停止 Spooler 失败: {0}", ex.Message));
                }

                // 3e. 手动删除 Driver Store 目录
                if (!string.IsNullOrEmpty(infPath))
                {
                    string driverStoreDir = Path.GetDirectoryName(infPath);
                    DeleteDriverStoreDirectory(driverStoreDir, errors);
                }

                // 3f. 清理注册表
                CleanDriverRegistry(driverName, driverVersion, errors);
            }
            finally
            {
                // 3g. 恢复 Spooler（无论前面是否出错）
                if (spoolerWasRunning)
                {
                    try
                    {
                        StartSpooler();
                    }
                    catch (Exception ex)
                    {
                        errors.Add(string.Format("恢复 Spooler 失败: {0}", ex.Message));
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// 删除 Driver Store 目录（先尝试普通删除，失败后 takeown 提权）
        /// </summary>
        private static void DeleteDriverStoreDirectory(string dirPath, List<string> errors)
        {
            if (string.IsNullOrEmpty(dirPath) || !Directory.Exists(dirPath))
                return;

            try
            {
                Directory.Delete(dirPath, recursive: true);
                Debug.WriteLine(string.Format("已删除 Driver Store 目录: {0}", dirPath));
            }
            catch
            {
                // 提权后重试
                try
                {
                    RunProcess("takeown.exe", string.Format("/f \"{0}\" /r /d y", dirPath));
                    RunProcess(
                        "icacls.exe",
                        string.Format("\"{0}\" /grant administrators:F /t", dirPath)
                    );
                    Directory.Delete(dirPath, recursive: true);
                    Debug.WriteLine(string.Format("提权后已删除 Driver Store 目录: {0}", dirPath));
                }
                catch (Exception ex)
                {
                    errors.Add(string.Format("删除 Driver Store 目录失败: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// 清理驱动注册表键（根据 INF 判断的版本精准删除，否则全删）
        /// 注：DeletePrinterDriverEx 已在 Spooler 端清理，此方法作为冗余兜底
        /// </summary>
        private static void CleanDriverRegistry(
            string driverName,
            string driverVersion,
            List<string> errors
        )
        {
            // V3 / V4 都尝试，若已通过 INF 确定版本则只清理对应版本
            string[][] versionArchSets = new[]
            {
                new[] { "Version-3", "3" },
                new[] { "Version-4", "4" },
            };

            string[][] archPaths = new[]
            {
                new[] { "Windows x64", "x64" },
                new[] { "Windows NT x86", "x86" },
            };

            foreach (var ver in versionArchSets)
            {
                string kv = ver[1]; // "3" or "4"
                // 如果 INF 已确认版本，跳过不匹配的
                if (driverVersion != null && driverVersion != kv)
                    continue;

                foreach (var arch in archPaths)
                {
                    string keyPath = string.Format(
                        @"SYSTEM\CurrentControlSet\Control\Print\Environments\{0}\Drivers\{1}\{2}",
                        arch[0],
                        ver[0],
                        driverName
                    );

                    try
                    {
                        using (var key = Registry.LocalMachine.OpenSubKey(keyPath, writable: true))
                        {
                            if (key != null)
                            {
                                Registry.LocalMachine.DeleteSubKeyTree(keyPath);
                                Debug.WriteLine(
                                    string.Format("已清除注册表键: HKLM\\{0}", keyPath)
                                );
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(
                            string.Format("清除注册表键失败 HKLM\\{0}: {1}", keyPath, ex.Message)
                        );
                    }
                }
            }
        }
    }
}
