using System;
using System.Runtime.InteropServices;

namespace PrinterManager.Core
{
    /// <summary>
    /// 封装所有打印机相关的 Windows API P/Invoke 声明
    /// </summary>
    internal static class PrinterApiWrapper
    {
        // ─── 常量 ────────────────────────────────────────────────────────────────

        // EnumPrinters flags
        public const uint PRINTER_ENUM_LOCAL = 0x00000002;
        public const uint PRINTER_ENUM_CONNECTIONS = 0x00000004;
        public const uint PRINTER_ENUM_NETWORK = 0x00000040;
        public const uint PRINTER_ENUM_SHARED = 0x00000020;

        // OpenPrinter access rights
        public const uint PRINTER_ACCESS_ADMINISTER = 0x00000004;
        public const uint PRINTER_ACCESS_USE = 0x00000008;
        public const uint PRINTER_ALL_ACCESS = 0x000F000C;
        public const uint SERVER_ALL_ACCESS = 0x000F0003;

        // SetPrinter command
        public const uint PRINTER_CONTROL_PURGE = 3;
        public const uint PRINTER_CONTROL_SET_STATUS = 4;

        // Printer attributes
        public const uint PRINTER_ATTRIBUTE_SHARED = 0x00000008;
        public const uint PRINTER_ATTRIBUTE_NETWORK = 0x00000010;
        public const uint PRINTER_ATTRIBUTE_LOCAL = 0x00000040;
        public const uint PRINTER_ATTRIBUTE_DEFAULT = 0x00000004;

        // DeletePrinterDriver flags
        public const uint DPD_DELETE_UNUSED_FILES = 0x00000001;
        public const uint DPD_DELETE_SPECIFIC_VERSION = 0x00000002;
        public const uint DPD_DELETE_ALL_FILES = 0x00000004;

        // AddPrinterConnection2 flags
        public const uint PRINTER_CONNECTION_MISMATCH = 0x00000020;
        public const uint PRINTER_CONNECTION_NO_UI = 0x00000040;

        // ─── 结构体 ──────────────────────────────────────────────────────────────

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PRINTER_INFO_2
        {
            [MarshalAs(UnmanagedType.LPTStr)] public string pServerName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pPrinterName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pShareName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pPortName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pDriverName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pComment;
            [MarshalAs(UnmanagedType.LPTStr)] public string pLocation;
            public IntPtr pDevMode;
            [MarshalAs(UnmanagedType.LPTStr)] public string pSepFile;
            [MarshalAs(UnmanagedType.LPTStr)] public string pPrintProcessor;
            [MarshalAs(UnmanagedType.LPTStr)] public string pDatatype;
            [MarshalAs(UnmanagedType.LPTStr)] public string pParameters;
            public IntPtr pSecurityDescriptor;
            public uint Attributes;
            public uint Priority;
            public uint DefaultPriority;
            public uint StartTime;
            public uint UntilTime;
            public uint Status;
            public uint cJobs;
            public uint AveragePPM;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PRINTER_INFO_4
        {
            [MarshalAs(UnmanagedType.LPTStr)] public string pPrinterName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pServerName;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DRIVER_INFO_3
        {
            public uint cVersion;
            [MarshalAs(UnmanagedType.LPTStr)] public string pName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pEnvironment;
            [MarshalAs(UnmanagedType.LPTStr)] public string pDriverPath;
            [MarshalAs(UnmanagedType.LPTStr)] public string pDataFile;
            [MarshalAs(UnmanagedType.LPTStr)] public string pConfigFile;
            [MarshalAs(UnmanagedType.LPTStr)] public string pHelpFile;
            [MarshalAs(UnmanagedType.LPTStr)] public string pDependentFiles;
            [MarshalAs(UnmanagedType.LPTStr)] public string pMonitorName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pDefaultDataType;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DRIVER_INFO_6
        {
            public uint cVersion;
            [MarshalAs(UnmanagedType.LPTStr)] public string pName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pEnvironment;
            [MarshalAs(UnmanagedType.LPTStr)] public string pDriverPath;
            [MarshalAs(UnmanagedType.LPTStr)] public string pDataFile;
            [MarshalAs(UnmanagedType.LPTStr)] public string pConfigFile;
            [MarshalAs(UnmanagedType.LPTStr)] public string pHelpFile;
            [MarshalAs(UnmanagedType.LPTStr)] public string pDependentFiles;
            [MarshalAs(UnmanagedType.LPTStr)] public string pMonitorName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pDefaultDataType;
            [MarshalAs(UnmanagedType.LPTStr)] public string pszzPreviousNames;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftDriverDate;
            public ulong dwlDriverVersion;
            [MarshalAs(UnmanagedType.LPTStr)] public string pszMfgName;
            [MarshalAs(UnmanagedType.LPTStr)] public string pszOEMUrl;
            [MarshalAs(UnmanagedType.LPTStr)] public string pszHardwareID;
            [MarshalAs(UnmanagedType.LPTStr)] public string pszProvider;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PRINTER_DEFAULTS
        {
            [MarshalAs(UnmanagedType.LPTStr)] public string pDatatype;
            public IntPtr pDevMode;
            public uint DesiredAccess;
        }

        // ─── 打印机枚举 & 操作 API ────────────────────────────────────────────────

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumPrinters(
            uint Flags,
            string Name,
            uint Level,
            IntPtr pPrinterEnum,
            uint cbBuf,
            ref uint pcbNeeded,
            ref uint pcReturned);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool OpenPrinter(
            string pPrinterName,
            out IntPtr phPrinter,
            ref PRINTER_DEFAULTS pDefault);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool OpenPrinter(
            string pPrinterName,
            out IntPtr phPrinter,
            IntPtr pDefault);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool DeletePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetPrinter(
            IntPtr hPrinter,
            uint Level,
            IntPtr pPrinter,
            uint cbBuf,
            ref uint pcbNeeded);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetPrinter(
            IntPtr hPrinter,
            uint Level,
            IntPtr pPrinter,
            uint Command);

        // ─── 网络打印机连接 API ───────────────────────────────────────────────────

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool AddPrinterConnection(string pName);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool AddPrinterConnection2(
            IntPtr hWnd,
            string pszName,
            uint dwLevel,
            IntPtr pConnectionInfo);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeletePrinterConnection(string pName);

        // ─── 驱动相关 API ─────────────────────────────────────────────────────────

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumPrinterDrivers(
            string pName,
            string pEnvironment,
            uint Level,
            IntPtr pDriverInfo,
            uint cbBuf,
            ref uint pcbNeeded,
            ref uint pcReturned);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeletePrinterDriver(
            string pName,
            string pEnvironment,
            string pDriverName);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeletePrinterDriverEx(
            string pName,
            string pEnvironment,
            string pDriverName,
            uint dwDeleteFlag,
            uint dwVersionFlag);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool AddPrinterDriver(
            string pName,
            uint Level,
            IntPtr pDriverInfo);

        // ─── 打印作业 API ─────────────────────────────────────────────────────────

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool SetJob(
            IntPtr hPrinter,
            uint JobId,
            uint Level,
            IntPtr pJob,
            uint Command);

        // ─── 默认打印机 API ───────────────────────────────────────────────────────

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetDefaultPrinter(
            System.Text.StringBuilder pszBuffer,
            ref uint pcchBuffer);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDefaultPrinter(string pszPrinter);
    }
}
