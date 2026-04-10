using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using PrinterManager.Core;
using PrinterManager.Helpers;
using PrinterManager.Models;

namespace PrinterManager.UI
{
    public partial class ScanPrinterForm : Form
    {
        private CancellationTokenSource _cts;
        private List<SharedPrinterEntry> _results = new List<SharedPrinterEntry>();

        /// <summary>用户最终选中要安装的打印机列表</summary>
        public List<SharedPrinterEntry> SelectedPrinters { get; private set; } =
            new List<SharedPrinterEntry>();

        public ScanPrinterForm()
        {
            InitializeComponent();
        }

        private void ScanPrinterForm_Load(object sender, EventArgs e)
        {
            string localIp = NetworkScanner.GetLocalIp();
            if (!string.IsNullOrEmpty(localIp))
            {
                // 默认填网段（尾号改为 0）
                string prefix = localIp.Substring(0, localIp.LastIndexOf('.'));
                txtTarget.Text = prefix + ".0";
                lblSubnet.Text = $"本机 IP：{localIp}";
            }
            else
            {
                lblSubnet.Text = "本机 IP：获取失败";
            }

            SetPlaceholder(txtTarget, "如 10.220.2.0 或 10.220.2.71");
        }

        private string GetPrefix(string ip) => ip.Substring(0, ip.LastIndexOf('.'));

        // ── 开始扫描 ──────────────────────────────────────────────────────────

        private void btnScan_Click(object sender, EventArgs e)
        {
            string input = txtTarget.Text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show(
                    "请输入目标 IP 或网段。\n\n" + "单机扫描：10.220.2.71\n网段扫描：10.220.2.0",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                txtTarget.Focus();
                return;
            }

            // 解析输入
            bool isSingleHost;
            string prefix;
            int fromSuffix,
                toSuffix;

            if (!TryParseTarget(input, out isSingleHost, out prefix, out fromSuffix, out toSuffix))
            {
                MessageBox.Show(
                    "IP 格式不正确。\n\n"
                        + "单机示例：10.220.2.71\n网段示例：10.220.2.0 或 10.220.2",
                    "格式错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                txtTarget.Focus();
                return;
            }

            _cts = new CancellationTokenSource();
            lvResults.Items.Clear();
            _results.Clear();
            btnInstall.Enabled = false;
            btnScan.Enabled = false;
            btnStop.Enabled = true;
            progressBar.Value = 0;
            progressBar.Maximum = isSingleHost ? 1 : (toSuffix - fromSuffix + 1);
            lblProgress.Text = "正在扫描...";

            IProgress<ScanProgress> progress = new Progress<ScanProgress>(p =>
            {
                if (!IsHandleCreated)
                    return;
                progressBar.Value = Math.Min(p.Done, progressBar.Maximum);
                lblProgress.Text = string.Format(
                    "进度：{0}/{1}  当前：{2}",
                    p.Done,
                    p.Total,
                    p.Host
                );
            });

            var ct = _cts.Token;
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    List<SharedPrinterEntry> found;
                    if (isSingleHost)
                    {
                        found = NetworkScanner.GetSharedPrinters(input);
                        progress.Report(
                            new ScanProgress
                            {
                                Done = 1,
                                Total = 1,
                                Host = input,
                            }
                        );
                    }
                    else
                    {
                        found = NetworkScanner.ScanSubnet(
                            prefix,
                            fromSuffix,
                            toSuffix,
                            progress,
                            ct
                        );
                    }

                    if (IsHandleCreated)
                        BeginInvoke(new Action(() => OnScanComplete(found)));
                }
                catch (OperationCanceledException)
                {
                    if (IsHandleCreated)
                        BeginInvoke(new Action(() => OnScanCancelled()));
                }
                catch (Exception ex)
                {
                    if (IsHandleCreated)
                        BeginInvoke(
                            new Action(() =>
                            {
                                MessageBox.Show(
                                    "扫描出错：" + ex.Message,
                                    "错误",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                                ResetScanButtons();
                            })
                        );
                }
                finally
                {
                    // 无论正常完成、取消、报错，都保证按钮恢复
                    if (IsHandleCreated)
                        BeginInvoke(new Action(() => ResetScanButtons()));
                }
            });
        }

        /// <summary>
        /// 解析用户输入，判断是单机还是网段
        /// 单机：10.220.2.71  → isSingleHost=true
        /// 网段：10.220.2.0 或 10.220.2 → isSingleHost=false, prefix=10.220.2, from=1, to=254
        /// </summary>
        private bool TryParseTarget(
            string input,
            out bool isSingleHost,
            out string prefix,
            out int fromSuffix,
            out int toSuffix
        )
        {
            isSingleHost = false;
            prefix = "";
            fromSuffix = 1;
            toSuffix = 254;

            string[] parts = input.Split('.');

            // 三段：10.220.2 → 视为网段
            if (parts.Length == 3)
            {
                // 验证前三段都是合法数字
                foreach (string p in parts)
                    if (!int.TryParse(p, out int n) || n < 0 || n > 255)
                        return false;

                isSingleHost = false;
                prefix = input;
                return true;
            }

            // 四段：10.220.2.x
            if (parts.Length == 4)
            {
                foreach (string p in parts)
                    if (!int.TryParse(p, out int n) || n < 0 || n > 255)
                        return false;

                int lastOctet = int.Parse(parts[3]);
                prefix = $"{parts[0]}.{parts[1]}.{parts[2]}";

                if (lastOctet == 0)
                {
                    // x.x.x.0 → 扫整个网段 1-254
                    isSingleHost = false;
                    fromSuffix = 1;
                    toSuffix = 254;
                }
                else
                {
                    // 具体 IP → 单机
                    isSingleHost = true;
                }
                return true;
            }

            return false;
        }

        private void OnScanComplete(List<SharedPrinterEntry> found)
        {
            _results = found;
            foreach (var p in found)
            {
                var item = new ListViewItem(p.Host);
                item.SubItems.Add(p.ShareName);
                item.SubItems.Add(p.UncPath);
                item.SubItems.Add(p.Comment);
                item.Tag = p;
                lvResults.Items.Add(item);
            }

            lblProgress.Text =
                found.Count > 0
                    ? string.Format("扫描完成，共发现 {0} 台共享打印机。", found.Count)
                    : "扫描完成，未发现共享打印机。";

            progressBar.Value = progressBar.Maximum;
            // 不在这里调用 ResetScanButtons，由 finally 统一处理
        }

        private void OnScanCancelled()
        {
            lblProgress.Text = "扫描已取消。";
            // 不在这里调用 ResetScanButtons，由 finally 统一处理
        }

        private void ResetScanButtons()
        {
            btnScan.Enabled = true;
            btnStop.Enabled = false;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            btnStop.Enabled = false;
            lblProgress.Text = "正在取消...";
        }

        // ── 安装选中项 ────────────────────────────────────────────────────────

        private void btnInstall_Click(object sender, EventArgs e)
        {
            SelectedPrinters.Clear();
            foreach (ListViewItem item in lvResults.CheckedItems)
                SelectedPrinters.Add((SharedPrinterEntry)item.Tag);

            if (SelectedPrinters.Count == 0)
            {
                MessageBox.Show(
                    "请先勾选要安装的打印机。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void lvResults_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            btnInstall.Enabled = lvResults.CheckedItems.Count > 0;
        }

        private void txtTarget_Enter(object sender, EventArgs e)
        {
            if (txtTarget.ForeColor == System.Drawing.Color.Gray)
            {
                txtTarget.Text = "";
                txtTarget.ForeColor = System.Drawing.Color.Black;
            }
        }

        // 在 ScanPrinterForm.cs 或单独的工具类里加
        [System.Runtime.InteropServices.DllImport(
            "user32.dll",
            CharSet = System.Runtime.InteropServices.CharSet.Auto
        )]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, string lParam);

        private const int EM_SETCUEBANNER = 0x1501;

        private void SetPlaceholder(System.Windows.Forms.TextBox tb, string text)
        {
            SendMessage(tb.Handle, EM_SETCUEBANNER, 0, text);
        }
    }
}
