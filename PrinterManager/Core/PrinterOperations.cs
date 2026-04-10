using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using PrinterManager.Models;

namespace PrinterManager.Core
{
    /// <summary>
    /// 打印机操作：枚举、删除、连接网络打印机、设置默认打印机
    /// </summary>
    public static class PrinterOperations
    {
        /// <summary>
        /// 枚举本地及连接的打印机
        /// </summary>
        public static List<PrinterInfo> EnumeratePrinters()
        {
            var list = new List<PrinterInfo>();
            string defaultPrinter = GetDefaultPrinterName();

            uint flags =
                PrinterApiWrapper.PRINTER_ENUM_LOCAL | PrinterApiWrapper.PRINTER_ENUM_CONNECTIONS;
            uint needed = 0,
                returned = 0;

            // 第一次调用获取缓冲区大小
            PrinterApiWrapper.EnumPrinters(
                flags,
                null,
                2,
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
                    !PrinterApiWrapper.EnumPrinters(
                        flags,
                        null,
                        2,
                        buf,
                        needed,
                        ref needed,
                        ref returned
                    )
                )
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                int structSize = Marshal.SizeOf(typeof(PrinterApiWrapper.PRINTER_INFO_2));
                for (int i = 0; i < returned; i++)
                {
                    IntPtr ptr = new IntPtr(buf.ToInt64() + i * structSize);
                    var info = (PrinterApiWrapper.PRINTER_INFO_2)
                        Marshal.PtrToStructure(ptr, typeof(PrinterApiWrapper.PRINTER_INFO_2));

                    list.Add(
                        new PrinterInfo
                        {
                            Name = info.pPrinterName,
                            ServerName = info.pServerName,
                            ShareName = info.pShareName,
                            PortName = info.pPortName,
                            DriverName = info.pDriverName,
                            Comment = info.pComment,
                            Location = info.pLocation,
                            Attributes = info.Attributes,
                            Status = info.Status,
                            JobCount = info.cJobs,
                            IsDefault = string.Equals(
                                info.pPrinterName,
                                defaultPrinter,
                                StringComparison.OrdinalIgnoreCase
                            ),
                        }
                    );
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }

            return list;
        }

        /// <summary>
        /// 删除打印机（先清除队列）
        /// </summary>
        public static void DeletePrinter(string printerName)
        {
            var pd = new PrinterApiWrapper.PRINTER_DEFAULTS
            {
                pDatatype = null,
                pDevMode = IntPtr.Zero,
                DesiredAccess = PrinterApiWrapper.PRINTER_ALL_ACCESS,
            };

            if (!PrinterApiWrapper.OpenPrinter(printerName, out IntPtr hPrinter, ref pd))
                throw new Win32Exception(
                    Marshal.GetLastWin32Error(),
                    $"无法打开打印机 \"{printerName}\""
                );

            try
            {
                // 清除所有打印任务
                PrinterApiWrapper.SetPrinter(
                    hPrinter,
                    0,
                    IntPtr.Zero,
                    PrinterApiWrapper.PRINTER_CONTROL_PURGE
                );

                if (!PrinterApiWrapper.DeletePrinter(hPrinter))
                    throw new Win32Exception(
                        Marshal.GetLastWin32Error(),
                        $"删除打印机 \"{printerName}\" 失败"
                    );
            }
            finally
            {
                PrinterApiWrapper.ClosePrinter(hPrinter);
            }
        }

        /// <summary>
        /// 添加网络共享打印机连接（\\server\share 格式）
        /// </summary>
        public static void AddNetworkPrinter(string uncPath)
        {
            if (!uncPath.StartsWith(@"\\"))
                throw new ArgumentException(@"网络打印机路径必须是 \\服务器\共享名 格式");

            // 连接前先设置 Point and Print 相关注册表，避免驱动安装被拦截
            SetPointAndPrintRegistry();

            if (!PrinterApiWrapper.AddPrinterConnection(uncPath))
                throw new Win32Exception(
                    Marshal.GetLastWin32Error(),
                    string.Format("连接打印机 \"{0}\" 失败", uncPath)
                );
        }

        private static void SetPointAndPrintRegistry()
        {
            try
            {
                // HKCU\Printers\LegacyPointAndPrint
                using (
                    var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                        @"Printers\LegacyPointAndPrint"
                    )
                )
                {
                    if (key != null)
                        key.SetValue(
                            "DisableLegacyPointAndPrintAdminSecurityWarning",
                            1,
                            Microsoft.Win32.RegistryValueKind.DWord
                        );
                }

                // HKLM\Software\Policies\Microsoft\Windows NT\Printers\PointAndPrint
                using (
                    var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(
                        @"Software\Policies\Microsoft\Windows NT\Printers\PointAndPrint"
                    )
                )
                {
                    if (key != null)
                    {
                        key.SetValue(
                            "RestrictDriverInstallationToAdministrators",
                            0,
                            Microsoft.Win32.RegistryValueKind.DWord
                        );
                        key.SetValue("InForest", 0, Microsoft.Win32.RegistryValueKind.DWord);
                        key.SetValue("Restricted", 0, Microsoft.Win32.RegistryValueKind.DWord);
                        key.SetValue("TrustedServers", 0, Microsoft.Win32.RegistryValueKind.DWord);
                    }
                }
            }
            catch (Exception ex)
            {
                // 注册表写入失败不阻断连接流程，但抛出警告
                throw new InvalidOperationException(
                    "写入 Point and Print 注册表失败，可能需要管理员权限：\n" + ex.Message,
                    ex
                );
            }
        }

        /// <summary>
        /// 移除网络打印机连接
        /// </summary>
        public static void RemoveNetworkPrinterConnection(string uncPath)
        {
            if (!PrinterApiWrapper.DeletePrinterConnection(uncPath))
                throw new Win32Exception(
                    Marshal.GetLastWin32Error(),
                    $"删除网络打印机连接 \"{uncPath}\" 失败"
                );
        }

        /// <summary>
        /// 设置默认打印机
        /// </summary>
        public static void SetDefaultPrinter(string printerName)
        {
            if (!PrinterApiWrapper.SetDefaultPrinter(printerName))
                throw new Win32Exception(
                    Marshal.GetLastWin32Error(),
                    $"设置默认打印机 \"{printerName}\" 失败"
                );
        }

        /// <summary>
        /// 获取当前默认打印机名称
        /// </summary>
        public static string GetDefaultPrinterName()
        {
            uint size = 256;
            var sb = new StringBuilder((int)size);
            if (PrinterApiWrapper.GetDefaultPrinter(sb, ref size))
                return sb.ToString();
            return string.Empty;
        }
    }
}
