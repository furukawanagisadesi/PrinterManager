using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PrinterManager.UI
{
    /// <summary>
    /// 添加网络共享打印机对话框
    /// </summary>
    public partial class AddNetworkPrinterForm : Form
    {
        /// <summary>
        /// 用户输入的 UNC 路径，如 \\Server\PrinterShare
        /// </summary>
        public List<string> UncPaths { get; private set; } = new List<string>();

        public AddNetworkPrinterForm()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                var thread = new System.Threading.Thread(() =>
                {
                    System.Diagnostics.Process.Start("explorer.exe", @"\\");
                });
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();

                MessageBox.Show(
                    "已打开网络资源浏览器。\n找到打印机后，请复制其 UNC 路径（如 \\\\服务器名\\共享名）粘贴到上方输入框。",
                    "浏览网络打印机",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch { }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string path = txtUncPath.Text.Trim();

            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show(
                    "请输入打印机 UNC 路径。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                txtUncPath.Focus();
                return;
            }

            if (!path.StartsWith(@"\\"))
            {
                MessageBox.Show(
                    "UNC 路径格式不正确。\n\n" + @"正确格式示例：\\服务器名\共享打印机名",
                    "格式错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                txtUncPath.Focus();
                return;
            }

            UncPaths.Add(path);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void txtUncPath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnOK_Click(sender, e);
        }

        private void btnScanLan_Click(object sender, EventArgs e)
        {
            using (var dlg = new ScanPrinterForm())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedPrinters.Count > 0)
                {
                    // 多台全部加入 UncPaths，同时把第一台显示在输入框里
                    UncPaths.Clear();
                    foreach (var p in dlg.SelectedPrinters)
                        UncPaths.Add(p.UncPath);

                    // 输入框显示选中的数量或第一台路径
                    if (dlg.SelectedPrinters.Count == 1)
                    {
                        txtUncPath.Text = dlg.SelectedPrinters[0].UncPath;
                        txtUncPath.ForeColor = System.Drawing.Color.Black;
                    }
                    else
                    {
                        txtUncPath.Text = string.Format(
                            "已选择 {0} 台打印机",
                            dlg.SelectedPrinters.Count
                        );
                        txtUncPath.ForeColor = System.Drawing.Color.FromArgb(0, 120, 212);
                    }

                    // 直接确认，不需要再点"连接打印机"按钮
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }
    }
}
