using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PrinterManager.Core;
using PrinterManager.Models;

namespace PrinterManager.UI
{
    public partial class MainForm : Form
    {
        private List<PrinterInfo> _printers = new List<PrinterInfo>();
        private List<DriverInfo> _drivers = new List<DriverInfo>();

        public MainForm()
        {
            InitializeComponent();
            SetupListViews();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RefreshAll();
        }

        // ─── 初始化 ListView 列 ───────────────────────────────────────────────────

        private void SetupListViews()
        {
            // 打印机列表列
            lvPrinters.Columns.Add("打印机名称", 220);
            lvPrinters.Columns.Add("类型", 80);
            lvPrinters.Columns.Add("状态", 80);
            lvPrinters.Columns.Add("驱动程序", 180);
            lvPrinters.Columns.Add("端口", 90);
            lvPrinters.Columns.Add("共享名", 120);
            lvPrinters.Columns.Add("作业数", 60);
            lvPrinters.Columns.Add("备注", 150);

            // 驱动列表列
            lvDrivers.Columns.Add("驱动名称", 260);
            lvDrivers.Columns.Add("版本", 160);
            lvDrivers.Columns.Add("环境", 130);
            lvDrivers.Columns.Add("驱动文件路径", 280);
        }

        // ─── 刷新 ─────────────────────────────────────────────────────────────────

        private void RefreshAll()
        {
            RefreshPrinters();
            RefreshDrivers();
        }

        private void RefreshPrinters()
        {
            lvPrinters.Items.Clear();
            try
            {
                _printers = PrinterOperations.EnumeratePrinters();
                foreach (var p in _printers)
                {
                    var item = new ListViewItem(p.Name);
                    item.SubItems.Add(p.TypeText);
                    item.SubItems.Add(p.StatusText);
                    item.SubItems.Add(p.DriverName);
                    item.SubItems.Add(p.PortName);
                    item.SubItems.Add(p.ShareName ?? "");
                    item.SubItems.Add(p.JobCount.ToString());
                    item.SubItems.Add(p.Comment ?? "");
                    item.Tag = p;

                    if (p.IsDefault)
                    {
                        item.Font = new Font(lvPrinters.Font, FontStyle.Bold);
                        item.ForeColor = Color.FromArgb(0, 100, 200);
                    }

                    if (p.IsNetwork)
                        item.ImageIndex = 1;
                    else if (p.IsShared)
                        item.ImageIndex = 2;
                    else
                        item.ImageIndex = 0;

                    lvPrinters.Items.Add(item);
                }
                lblPrinterCount.Text = $"共 {_printers.Count} 台打印机";
            }
            catch (Exception ex)
            {
                ShowError("刷新打印机列表失败", ex);
            }
        }

        private void RefreshDrivers()
        {
            lvDrivers.Items.Clear();
            try
            {
                _drivers = DriverOperations.EnumerateDrivers();
                foreach (var d in _drivers)
                {
                    var item = new ListViewItem(d.Name);
                    item.SubItems.Add(d.VersionText);
                    item.SubItems.Add(d.Environment);
                    item.SubItems.Add(d.DriverPath ?? "");
                    item.Tag = d;
                    lvDrivers.Items.Add(item);
                }
                lblDriverCount.Text = $"共 {_drivers.Count} 个驱动";
            }
            catch (Exception ex)
            {
                ShowError("刷新驱动列表失败", ex);
            }
        }

        // ─── 打印机操作 ───────────────────────────────────────────────────────────

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshAll();
            LogInfo("已刷新打印机和驱动列表。");
        }

        private void btnDeletePrinter_Click(object sender, EventArgs e)
        {
            if (lvPrinters.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "请先选择要删除的打印机。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            var item = lvPrinters.SelectedItems[0];
            var printer = (PrinterInfo)item.Tag;

            var result = MessageBox.Show(
                $"确定要删除打印机 \"{printer.Name}\" 吗？\n\n（仅删除打印机队列，不删除驱动程序）",
                "确认删除打印机",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                SetBusy(true);
                if (printer.IsNetwork)
                    PrinterOperations.RemoveNetworkPrinterConnection(printer.Name);
                else
                    PrinterOperations.DeletePrinter(printer.Name);

                LogSuccess($"打印机 \"{printer.Name}\" 已成功删除。");
                RefreshPrinters();
            }
            catch (Exception ex)
            {
                ShowError($"删除打印机 \"{printer.Name}\" 失败", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void btnSetDefault_Click(object sender, EventArgs e)
        {
            if (lvPrinters.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "请先选择要设置为默认的打印机。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            var printer = (PrinterInfo)lvPrinters.SelectedItems[0].Tag;
            try
            {
                PrinterOperations.SetDefaultPrinter(printer.Name);
                LogSuccess($"已将 \"{printer.Name}\" 设置为默认打印机。");
                RefreshPrinters();
            }
            catch (Exception ex)
            {
                ShowError("设置默认打印机失败", ex);
            }
        }

        private void btnAddNetwork_Click(object sender, EventArgs e)
        {
            using (var dlg = new AddNetworkPrinterForm())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    // dlg.UncPaths 是一个 List<string>，包含所有要安装的路径
                    var paths = dlg.UncPaths;
                    if (paths == null || paths.Count == 0)
                        return;

                    int successCount = 0;
                    int failCount = 0;

                    try
                    {
                        SetBusy(true);
                        foreach (string uncPath in paths)
                        {
                            try
                            {
                                PrinterOperations.AddNetworkPrinter(uncPath);
                                LogSuccess(
                                    string.Format("已成功连接网络打印机 \"{0}\"。", uncPath)
                                );
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                LogError(
                                    string.Format(
                                        "连接打印机 \"{0}\" 失败：{1}",
                                        uncPath,
                                        ex.Message
                                    )
                                );
                                failCount++;
                            }
                        }

                        if (successCount > 0)
                            RefreshPrinters();

                        if (failCount > 0)
                            MessageBox.Show(
                                string.Format(
                                    "共 {0} 台成功，{1} 台失败，详情请查看操作日志。",
                                    successCount,
                                    failCount
                                ),
                                "安装完成",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                    }
                    finally
                    {
                        SetBusy(false);
                    }
                }
            }
        }

        // ─── 驱动操作 ─────────────────────────────────────────────────────────────

        private void btnDeleteDriver_Click(object sender, EventArgs e)
        {
            if (lvDrivers.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "请先选择要删除的驱动程序。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            var driver = (DriverInfo)lvDrivers.SelectedItems[0].Tag;

            // 检查是否有打印机正在使用此驱动
            var usingPrinters = _printers.FindAll(p =>
                string.Equals(p.DriverName, driver.Name, StringComparison.OrdinalIgnoreCase)
            );

            string warnMsg =
                usingPrinters.Count > 0
                    ? $"警告：以下打印机正在使用此驱动，删除驱动将导致这些打印机无法使用：\n  {string.Join("\n  ", usingPrinters.ConvertAll(p => p.Name))}\n\n"
                    : "";

            var result = MessageBox.Show(
                $"{warnMsg}确定要删除驱动程序 \"{driver.Name}\" 吗？\n\n"
                    + $"版本：{driver.VersionText}\n环境：{driver.Environment}\n\n"
                    + "是否同时删除驱动关联文件（.dll/.inf等）？\n\n"
                    + "  [是] = 删除驱动 + 文件\n  [否] = 仅删除驱动记录\n  [取消] = 放弃",
                "确认删除驱动",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Cancel)
                return;
            bool deleteFiles = (result == DialogResult.Yes);

            try
            {
                SetBusy(true);
                var errors = DriverOperations.DeleteDriverAllVersions(
                    driver.Name,
                    driver.Environment,
                    deleteFiles
                );

                if (errors.Count == 0)
                    LogSuccess(
                        $"驱动 \"{driver.Name}\" 已成功删除{(deleteFiles ? "（含关联文件）" : "")}。"
                    );
                else
                    LogWarning(
                        $"驱动 \"{driver.Name}\" 删除完成，部分版本有警告：\n"
                            + string.Join("\n", errors)
                    );

                RefreshDrivers();
            }
            catch (Exception ex)
            {
                ShowError($"删除驱动 \"{driver.Name}\" 失败", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // ─── 辅助方法 ─────────────────────────────────────────────────────────────

        private void SetBusy(bool busy)
        {
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            btnRefresh.Enabled = !busy;
            btnDeletePrinter.Enabled = !busy;
            btnAddNetwork.Enabled = !busy;
            btnSetDefault.Enabled = !busy;
            btnDeleteDriver.Enabled = !busy;
            btnRestartSpooler.Enabled = !busy;
            progressBar.Visible = busy;
        }

        private void LogInfo(string msg) => AppendLog(msg, Color.FromArgb(200, 205, 215));

        private void LogSuccess(string msg) => AppendLog("✔ " + msg, Color.FromArgb(0, 130, 60));

        private void LogWarning(string msg) => AppendLog("⚠ " + msg, Color.FromArgb(180, 100, 0));

        private void LogError(string msg) => AppendLog("✖ " + msg, Color.FromArgb(180, 0, 0));

        private void AppendLog(string msg, Color color)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = Color.FromArgb(100, 120, 150); // ← 原来是 Color.Gray
            rtbLog.AppendText($"[{timestamp}] ");
            rtbLog.SelectionColor = color;
            rtbLog.AppendText(msg + Environment.NewLine);
            rtbLog.ScrollToCaret();
        }

        private void ShowError(string title, Exception ex)
        {
            LogError($"{title}: {ex.Message}");
            MessageBox.Show(
                $"{title}：\n\n{ex.Message}",
                "错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            rtbLog.Clear();
        }

        private void lvPrinters_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool selected = lvPrinters.SelectedItems.Count > 0;
            btnDeletePrinter.Enabled = selected;
            btnSetDefault.Enabled = selected;

            if (selected)
            {
                var p = (PrinterInfo)lvPrinters.SelectedItems[0].Tag;
                UpdateStatusBar(
                    $"打印机: {p.Name}  |  驱动: {p.DriverName}  |  端口: {p.PortName}  |  状态: {p.StatusText}  |  {(p.IsDefault ? "★ 默认打印机" : "")}"
                );
            }
        }

        private void lvDrivers_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool selected = lvDrivers.SelectedItems.Count > 0;
            btnDeleteDriver.Enabled = selected;

            if (selected)
            {
                var d = (DriverInfo)lvDrivers.SelectedItems[0].Tag;
                int usedBy = _printers
                    .FindAll(p =>
                        string.Equals(p.DriverName, d.Name, StringComparison.OrdinalIgnoreCase)
                    )
                    .Count;
                UpdateStatusBar(
                    $"驱动: {d.Name}  |  {d.VersionText}  |  {d.Environment}  |  被 {usedBy} 台打印机使用"
                );
            }
        }

        private void UpdateStatusBar(string text)
        {
            lblStatus.Text = text;
        }

        private void lvPrinters_DoubleClick(object sender, EventArgs e)
        {
            if (lvPrinters.SelectedItems.Count == 0)
                return;
            var printer = (PrinterInfo)lvPrinters.SelectedItems[0].Tag;

            string info =
                $"打印机名称：{printer.Name}\n"
                + $"类型：{printer.TypeText}\n"
                + $"驱动程序：{printer.DriverName}\n"
                + $"端口：{printer.PortName}\n"
                + $"共享名：{printer.ShareName ?? "(无)"}\n"
                + $"服务器：{printer.ServerName ?? "(本地)"}\n"
                + $"状态：{printer.StatusText}\n"
                + $"待打印任务：{printer.JobCount}\n"
                + $"备注：{printer.Comment ?? "(无)"}\n"
                + $"位置：{printer.Location ?? "(无)"}\n"
                + $"默认打印机：{(printer.IsDefault ? "是" : "否")}";

            MessageBox.Show(
                info,
                $"打印机详情 - {printer.Name}",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void btnRestartSpooler_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "确定要重启 Print Spooler 打印服务吗？\n\n重启期间所有打印任务将暂停。",
                "确认重启打印服务",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            try
            {
                SetBusy(true);
                LogInfo("正在重启 Print Spooler 服务...");

                using (var svc = new System.ServiceProcess.ServiceController("Spooler"))
                {
                    if (svc.Status != System.ServiceProcess.ServiceControllerStatus.Stopped)
                    {
                        svc.Stop();
                        svc.WaitForStatus(
                            System.ServiceProcess.ServiceControllerStatus.Stopped,
                            TimeSpan.FromSeconds(15)
                        );
                    }

                    svc.Start();
                    svc.WaitForStatus(
                        System.ServiceProcess.ServiceControllerStatus.Running,
                        TimeSpan.FromSeconds(20)
                    );
                }

                LogSuccess("Print Spooler 服务已成功重启。");
                RefreshPrinters();
            }
            catch (Exception ex)
            {
                ShowError("重启 Print Spooler 失败", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void btnRefresh2_Click(object sender, EventArgs e)
        {
            RefreshAll();
            LogInfo("已刷新打印机和驱动列表。");
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabPrinters)
                RefreshPrinters();
            else if (tabControl.SelectedTab == tabDrivers)
                RefreshDrivers();
            // 切到日志页不刷新
        }
    }
}
