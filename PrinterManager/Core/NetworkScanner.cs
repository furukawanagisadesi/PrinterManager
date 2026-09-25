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
        public string HostName { get; set; } // 计算机名，解析失败时为空
        public string Comment { get; set; }

        // 默认（IP）路径，保持向后兼容
        public string UncPath => $@"\\{Host}\{ShareName}";

        /// <summary>
        /// 生成连接路径：useHostName 且已解析到计算机名时用计算机名，否则回退到 IP。
        /// </summary>
        public string GetUncPath(bool useHostName)
        {
            string server = (useHostName && !string.IsNullOrEmpty(HostName)) ? HostName : Host;
            return $@"\\{server}\{ShareName}";
        }

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

        [StructLayout(LayoutKind.Sequential)]
        private struct SERVER_INFO_100
        {
            public uint sv100_platform_id;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string sv100_name;
        }

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int NetServerGetInfo(string servername, int level, out IntPtr bufptr);

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

        /// <summary>
        /// 为已枚举的条目补全计算机名（按主机去重，每台主机解析一次）。
        /// 刻意放在枚举超时之外调用，避免解析耗时导致已发现的共享被丢弃。
        /// </summary>
        public static void ResolveHostNames(IList<SharedPrinterEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return;

            var cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrEmpty(e.Host))
                    continue;

                if (!cache.TryGetValue(e.Host, out string name))
                {
                    name = GetServerName(e.Host);
                    cache[e.Host] = name;
                }
                e.HostName = name;
            }
        }

        /// <summary>
        /// 解析主机的计算机名：优先 NetServerGetInfo（复用 SMB 会话），失败回退反向 DNS。
        /// 均失败返回空字符串（调用方回退为 IP）。
        /// 注意：用 IP 连接时 NetServerGetInfo 常原样回显输入，因此过滤掉等于 host 的结果。
        /// </summary>
        public static string GetServerName(string host)
        {
            if (string.IsNullOrEmpty(host))
                return "";

            // 1) NetServerGetInfo：部分环境返回真实计算机名
            IntPtr buf = IntPtr.Zero;
            try
            {
                int ret = NetServerGetInfo(host, 100, out buf);
                if (ret == NERR_Success && buf != IntPtr.Zero)
                {
                    var info = (SERVER_INFO_100)
                        Marshal.PtrToStructure(buf, typeof(SERVER_INFO_100));
                    string apiName = info.sv100_name?.TrimStart('\\');
                    if (
                        !string.IsNullOrEmpty(apiName)
                        && !string.Equals(apiName, host, StringComparison.OrdinalIgnoreCase)
                    )
                        return apiName;
                }
            }
            catch { }
            finally
            {
                if (buf != IntPtr.Zero)
                    NetApiBufferFree(buf);
            }

            // 2) 反向 DNS：取第一个标签作为计算机名（如 FLEISCH.lan → FLEISCH）
            try
            {
                string dnsName = Dns.GetHostEntry(host).HostName;
                if (!string.IsNullOrEmpty(dnsName))
                {
                    int dot = dnsName.IndexOf('.');
                    string shortName = dot > 0 ? dnsName.Substring(0, dot) : dnsName;
                    if (!string.Equals(shortName, host, StringComparison.OrdinalIgnoreCase))
                        return shortName;
                }
            }
            catch { }

            return "";
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

            // 枚举完成后再解析计算机名，避免解析耗时影响上面按主机的超时控制
            ResolveHostNames(list);

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
