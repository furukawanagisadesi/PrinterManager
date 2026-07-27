using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using PrinterManager.Models;

namespace PrinterManager.Core
{
    /// <summary>
    /// 打印机驱动枚举，泛型统一 Level 3 / Level 4 的重复逻辑
    /// </summary>
    public static class DriverEnumerator
    {
        public static List<DriverInfo> EnumerateDrivers()
        {
            var list = new List<DriverInfo>();
            EnumDrivers<PrinterApiWrapper.DRIVER_INFO_3>(3, list, info => new DriverInfo
            {
                Name = info.pName,
                Environment = info.pEnvironment,
                DriverPath = info.pDriverPath,
                DataFile = info.pDataFile,
                ConfigFile = info.pConfigFile,
                Version = info.cVersion,
            });
            EnumDrivers<PrinterApiWrapper.DRIVER_INFO_4>(4, list, info => new DriverInfo
            {
                Name = info.pName,
                Environment = info.pEnvironment,
                DriverPath = info.pDriverPath,
                DataFile = info.pDataFile,
                ConfigFile = info.pConfigFile,
                Version = info.cVersion,
            });
            list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            return list;
        }

        private static void EnumDrivers<T>(
            uint level,
            List<DriverInfo> list,
            Func<T, DriverInfo> mapper)
        {
            uint needed = 0, returned = 0;

            PrinterApiWrapper.EnumPrinterDrivers(
                null, null, level,
                IntPtr.Zero, 0,
                ref needed, ref returned);

            if (needed == 0)
                return;

            IntPtr buf = Marshal.AllocHGlobal((int)needed);
            try
            {
                if (!PrinterApiWrapper.EnumPrinterDrivers(
                    null, null, level,
                    buf, needed,
                    ref needed, ref returned))
                {
                    int err = Marshal.GetLastWin32Error();
                    if (err == 122) // ERROR_INSUFFICIENT_BUFFER
                    {
                        IntPtr newBuf = Marshal.AllocHGlobal((int)needed);
                        Marshal.FreeHGlobal(buf);
                        buf = newBuf;
                        if (!PrinterApiWrapper.EnumPrinterDrivers(
                            null, null, level,
                            buf, needed,
                            ref needed, ref returned))
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    else
                    {
                        // Level 4 在旧系统上不支持，静默忽略
                        if (level == 4) return;
                        throw new Win32Exception(err);
                    }
                }

                int structSize = Marshal.SizeOf(typeof(T));
                for (int i = 0; i < returned; i++)
                {
                    IntPtr ptr = new IntPtr(buf.ToInt64() + i * structSize);
                    var info = (T)Marshal.PtrToStructure(ptr, typeof(T));
                    var driver = mapper(info);

                    if (!list.Exists(d =>
                        string.Equals(d.Name, driver.Name, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(d.Environment, driver.Environment,
                            StringComparison.OrdinalIgnoreCase)
                        && d.Version == driver.Version))
                    {
                        list.Add(driver);
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }
        }
    }
}
