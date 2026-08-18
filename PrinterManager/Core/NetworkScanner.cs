using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PrinterManager.Helpers;
using PrinterManager.Models;

namespace PrinterManager.Core
{
    public class SharedPrinterEntry
    {
        public string Host { get; set; } // 10.220.2.5
        public string ShareName { get; set; } // HP-LaserJet
        public string UncPath => $@"\\{Host}\{ShareName}";
        public string Comment { get; set; }

        public override string ToString() => UncPath;
    }

    public static class NetworkScanner
    {
        // ── NetShareEnum P/Invoke ────────────────────────────────────────────

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHARE_INFO_1
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string shi1_netname;
            public uint shi1_type;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string shi1_remark;
        }

        // shi1_type 常量
        private const uint STYPE_PRINTQ = 1; // 打印机共享
        private const uint STYPE_SPECIAL = 0x80000000;

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int NetShareEnum(
            string servername,
            int level,
            ref IntPtr bufptr,
            int prefmaxlen,
            out int entriesread,
            out int totalentries,
            ref int resume_handle
        );

        [DllImport("Netapi32.dll")]
        private static extern int NetApiBufferFree(IntPtr buffer);

        private const int MAX_PREFERRED_LENGTH = -1;
        private const int NERR_Success = 0;

        // ── 获取单台主机的共享打印机 ─────────────────────────────────────────

        /// <summary>
        /// 枚举指定主机上的所有共享打印机，失败返回空列表
        /// </summary>
        public static List<SharedPrinterEntry> GetSharedPrinters(string host)
        {
            var result = new List<SharedPrinterEntry>();
            IntPtr buf = IntPtr.Zero;
            int resume = 0;

            try
            {
                int ret = NetShareEnum(
                    host,
                    1,
                    ref buf,
                    MAX_PREFERRED_LENGTH,
                    out int read,
                    out int _,
                    ref resume
                );
                if (ret != NERR_Success)
                    return result;

                int size = Marshal.SizeOf(typeof(SHARE_INFO_1));
                for (int i = 0; i < read; i++)
                {
                    var entry = (SHARE_INFO_1)
                        Marshal.PtrToStructure(
                            new IntPtr(buf.ToInt64() + i * size),
                            typeof(SHARE_INFO_1)
                        );

                    // 只取打印机共享（过滤 SPECIAL bit）
                    if ((entry.shi1_type & ~STYPE_SPECIAL) == STYPE_PRINTQ)
                    {
                        result.Add(
                            new SharedPrinterEntry
                            {
                                Host = host,
                                ShareName = entry.shi1_netname,
                                Comment = entry.shi1_remark ?? "",
                            }
                        );
                    }
                }
            }
            catch { }
            finally
            {
                if (buf != IntPtr.Zero)
                    NetApiBufferFree(buf);
            }

            return result;
        }

        // ── 扫描整个 /24 子网 ────────────────────────────────────────────────

        /// <summary>
        /// 根据本机 IP 推算 /24 网段，并行 Ping + 枚举共享打印机
        /// </summary>
        /// <param name="progress">进度回调 (已完成数, 总数)</param>
        /// <param name="ct">取消令牌</param>
        public static List<SharedPrinterEntry> ScanSubnet(
            string prefix,
            int fromSuffix,
            int toSuffix,
            IProgress<ScanProgress> progress,
            CancellationToken ct
        )
        {
            var hosts = new List<string>();
            for (int i = fromSuffix; i <= toSuffix; i++)
                hosts.Add($"{prefix}.{i}");

            var found = new ConcurrentBag<SharedPrinterEntry>();
            int done = 0;
            int total = hosts.Count;

            var opts = new ParallelOptions { MaxDegreeOfParallelism = 32, CancellationToken = ct };

            Parallel.ForEach(
                hosts,
                opts,
                host =>
                {
                    ct.ThrowIfCancellationRequested();
                    if (PingHost(host, 300))
                        foreach (var p in GetSharedPrintersWithTimeout(host, ct, SmbEnumTimeoutMs))
                            found.Add(p);

                    int current = Interlocked.Increment(ref done);
                    if (progress != null)
                        progress.Report(
                            new ScanProgress
                            {
                                Done = current,
                                Total = total,
                                Host = host,
                            }
                        );
                }
            );

            var list = new List<SharedPrinterEntry>(found);
            list.Sort(
                (a, b) => string.Compare(a.UncPath, b.UncPath, StringComparison.OrdinalIgnoreCase)
            );
            return list;
        }

        // ── 工具方法 ─────────────────────────────────────────────────────────

        // SMB 枚举超时：防止个别主机的 NetShareEnum 长时间阻塞，导致扫描/取消卡顿
        private const int SmbEnumTimeoutMs = 3000;

        /// <summary>
        /// 带超时和取消响应的共享打印机枚举。
        /// NetShareEnum 是阻塞式 P/Invoke 无法直接取消，这里放到后台任务并限时等待：
        /// - 超时则放弃该主机（底层任务继续在后台自然结束，不影响扫描循环）
        /// - 取消时立即抛出 OperationCanceledException，让 Parallel.ForEach 快速退出
        /// </summary>
        private static List<SharedPrinterEntry> GetSharedPrintersWithTimeout(
            string host,
            CancellationToken ct,
            int timeoutMs
        )
        {
            var task = Task.Factory.StartNew(() => GetSharedPrinters(host));
            try
            {
                if (task.Wait(timeoutMs, ct))
                    return task.Result;
                return new List<SharedPrinterEntry>();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return new List<SharedPrinterEntry>();
            }
        }

        public static string GetLocalIp()
        {
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up)
                        continue;
                    var ipProps = ni.GetIPProperties();
                    foreach (var addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == AddressFamily.InterNetwork
                            && !IPAddress.IsLoopback(addr.Address))
                        {
                            return addr.Address.ToString();
                        }
                    }
                }
            }
            catch
            {
            }
            return "";
        }

        private static bool PingHost(string host, int timeoutMs)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send(host, timeoutMs);
                    return reply?.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
