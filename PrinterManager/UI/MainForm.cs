using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using PrinterManager.Core;
using PrinterManager.Helpers;
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
                _drivers = DriverEnumerator.EnumerateDrivers();
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

            OperationRunner.Run(
                this,
                "正在删除打印机…",
                () =>
                {
                    if (printer.IsNetwork)
                        PrinterOperations.RemoveNetworkPrinterConnection(printer.Name);
                    else
                        PrinterOperations.DeletePrinter(printer.Name);
                },
                onSuccess: () =>
                {
                    LogSuccess($"打印机 \"{printer.Name}\" 已成功删除。");
                    RefreshPrinters();
                },
                onError: ex => ShowError($"删除打印机 \"{printer.Name}\" 失败", ex)
            );
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
            OperationRunner.Run(
                this,
                "正在设置默认打印机…",
                () => PrinterOperations.SetDefaultPrinter(printer.Name),
                onSuccess: () =>
                {
                    LogSuccess($"已将 \"{printer.Name}\" 设置为默认打印机。");
                    RefreshPrinters();
                },
                onError: ex => ShowError("设置默认打印机失败", ex)
            );
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

                    OperationRunner.Run(
                        this,
                        "正在添加网络打印机…",
                        () =>
                        {
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
                        },
                        onSuccess: () =>
                        {
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
                    );
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

            // 检测是否有打印机正在使用此驱动，自动先删除打印机
            var usingPrinters = _printers.FindAll(p =>
                string.Equals(p.DriverName, driver.Name, StringComparison.OrdinalIgnoreCase)
            );
            if (usingPrinters.Count > 0)
            {
                string printerList = string.Join("\n  ", usingPrinters.ConvertAll(p => p.Name));
                var result = MessageBox.Show(
                    $"以下打印机正在使用此驱动 \"{driver.Name}\"：\n  {printerList}\n\n"
                        + "将先自动删除这些打印机，再删除驱动程序。\n是否继续？",
                    "自动删除打印机",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );
                if (result == DialogResult.No)
                    return;

                OperationRunner.Run(
                    this,
                    "正在自动删除使用该驱动的打印机…",
                    () =>
                    {
                        foreach (var printer in usingPrinters)
                        {
                            try
                            {
                                if (printer.IsNetwork)
                                    PrinterOperations.RemoveNetworkPrinterConnection(
                                        printer.Name
                                    );
                                else
                                    PrinterOperations.DeletePrinter(printer.Name);
                                LogInfo($"已自动删除打印机 \"{printer.Name}\"。");
                            }
                            catch (Exception ex)
                            {
                                LogWarning(
                                    $"删除打印机 \"{printer.Name}\" 失败：{ex.Message}，跳过继续卸载驱动。"
                                );
                            }
                        }
                    },
                    onSuccess: () => ConfirmAndDeleteDriver(driver)
                );
                return;
            }

            ConfirmAndDeleteDriver(driver);
        }

        private void ConfirmAndDeleteDriver(DriverInfo driver)
        {
            var result2 = MessageBox.Show(
                $"确定要删除驱动程序 \"{driver.Name}\" 吗？\n\n"
                    + $"版本：{driver.VersionText}\n环境：{driver.Environment}\n\n"
                    + "操作说明：\n"
                    + "  · 通过 pnputil /delete-driver 删除驱动包（.inf/.cat）\n"
                    + "  · Win32 API 清理驱动注册表记录及关联文件",
                "确认删除驱动",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result2 == DialogResult.No)
                return;

            OperationRunner.Run(
                this,
                "正在卸载驱动程序…",
                () =>
                {
                    var errors = DriverOperations.UninstallDriverEnhanced(driver.Name);

                    if (errors.Count == 0)
                        LogSuccess(
                            $"驱动 \"{driver.Name}\" 已成功删除（含关联文件 + 驱动包清理）"
                        );
                    else
                        LogWarning(
                            $"驱动 \"{driver.Name}\" 删除完成，部分操作有警告：\n"
                                + string.Join("\n", errors)
                        );
                },
                onSuccess: () => RefreshDrivers(),
                onError: ex => ShowError($"删除驱动 \"{driver.Name}\" 失败", ex)
            );
        }

        // ─── 辅助方法 ─────────────────────────────────────────────────────────────

        private void LogInfo(string msg) =>
            LogThreadSafe(msg, Color.FromArgb(200, 205, 215));

        private void LogSuccess(string msg) =>
            LogThreadSafe("[成功] " + msg, Color.FromArgb(0, 130, 60));

        private void LogWarning(string msg) =>
            LogThreadSafe("[告警] " + msg, Color.FromArgb(180, 100, 0));

        private void LogError(string msg) =>
            LogThreadSafe("[失败] " + msg, Color.FromArgb(180, 0, 0));

        /// <summary>
        /// 线程安全的日志输出：后台线程调用时自动切回 UI 线程。
        /// </summary>
        private void LogThreadSafe(string msg, Color color)
        {
            if (InvokeRequired)
                BeginInvoke((Action)(() => AppendLog(msg, color)));
            else
                AppendLog(msg, color);
        }

        private void AppendLog(string msg, Color color)
        {
            AppendToLog(rtbLog, msg, color);
            AppendToLog(rtbPrinterLog, msg, color);
            AppendToLog(rtbDriverLog, msg, color);
        }

        private static void AppendToLog(RichTextBox box, string msg, Color color)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = Color.FromArgb(100, 120, 150);
            box.AppendText($"[{timestamp}] ");
            box.SelectionColor = color;
            box.AppendText(msg + Environment.NewLine);
            box.ScrollToCaret();
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
            rtbPrinterLog.Clear();
            rtbDriverLog.Clear();
        }

        private void lvPrinters_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool selected = lvPrinters.SelectedItems.Count > 0;
            btnDeletePrinter.Enabled = selected;
            btnSetDefault.Enabled = selected;
            btnToggleShare.Enabled = selected;

            if (selected)
            {
                var p = (PrinterInfo)lvPrinters.SelectedItems[0].Tag;
                UpdateStatusBar(
                    $"打印机: {p.Name}  |  驱动: {p.DriverName}  |  端口: {p.PortName}  |  状态: {p.StatusText}  |  {(p.IsDefault ? "★ 默认打印机" : "")}  |  {(p.IsShared ? "共享: " + p.ShareName : "未共享")}"
                );
            }
        }

        private void btnInstallDriver_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "选择驱动程序 INF 文件";
                ofd.Filter = "INF 文件 (*.inf)|*.inf|所有文件 (*.*)|*.*";
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                string infPath = ofd.FileName;

                // 检查 INF 是否存在
                if (!File.Exists(infPath))
                {
                    ShowError("选中的 INF 文件不存在。", null);
                    return;
                }

                try
                {
                    OperationRunner.Run(
                        this,
                        "正在安装驱动程序…",
                        () =>
                        {
                            LogInfo($"开始安装驱动程序: {Path.GetFileName(infPath)}");

                            // 解析 INF 中的真实驱动名（从 [Manufacturer] → [Strings] 解析）
                            string driverName = InfParser.ParseDriverNameFromInf(infPath);
                            LogInfo($"解析驱动名称: {driverName}");

                            DriverOperations.InstallDriver(infPath, driverName);

                            LogSuccess($"驱动程序安装成功: {driverName}");
                        },
                        onSuccess: () => RefreshDrivers(),
                        onError: ex => ShowError("安装驱动程序时发生错误", ex)
                    );
                }
                catch (Exception ex)
                {
                    ShowError("安装驱动程序时发生错误", ex);
                }
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

            OperationRunner.Run(
                this,
                "正在重启打印服务…",
                () =>
                {
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
                },
                onSuccess: () => RefreshPrinters(),
                onError: ex => ShowError("重启 Print Spooler 失败", ex)
            );
        }

        private void btnToggleShare_Click(object sender, EventArgs e)
        {
            if (lvPrinters.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "请先选择要设置共享的打印机。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            var printer = (PrinterInfo)lvPrinters.SelectedItems[0].Tag;

            if (printer.IsShared)
            {
                // 取消共享
                var result = MessageBox.Show(
                    $"确定要取消打印机 \"{printer.Name}\" 的共享吗？\n\n共享名：{printer.ShareName}",
                    "确认取消共享",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (result != DialogResult.Yes)
                    return;

                OperationRunner.Run(
                    this,
                    "正在取消打印机共享…",
                    () => PrinterOperations.UnsetPrinterShare(printer.Name),
                    onSuccess: () =>
                    {
                        LogSuccess($"已取消打印机 \"{printer.Name}\" 的共享。");
                        RefreshPrinters();
                    },
                    onError: ex => ShowError("取消打印机共享失败", ex)
                );
            }
            else
            {
                // 设置共享 - 弹窗输入共享名
                string shareName = Microsoft.VisualBasic.Interaction.InputBox(
                    "请输入共享名称：",
                    "设置打印机共享",
                    printer.Name,
                    -1,
                    -1
                );

                if (string.IsNullOrWhiteSpace(shareName))
                {
                    LogInfo("已取消共享设置操作。");
                    return;
                }

                string trimmedShare = shareName.Trim();
                OperationRunner.Run(
                    this,
                    "正在设置打印机共享…",
                    () => PrinterOperations.SetPrinterShare(printer.Name, trimmedShare),
                    onSuccess: () =>
                    {
                        LogSuccess(
                            $"已将打印机 \"{printer.Name}\" 设置为共享，共享名：{trimmedShare}。"
                        );
                        RefreshPrinters();
                    },
                    onError: ex => ShowError("设置打印机共享失败", ex)
                );
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

        private void btnCleanPrinters_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "确定要清空打印任务吗？\n\n清理将重启 Print Spooler 打印服务。",
                "确认清空打印任务",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            bool isCleaned = false;
            int remainingCount = 0;

            OperationRunner.Run(
                this,
                "正在清空打印任务…",
                progress =>
                {
                    const string PrintersPath = @"C:\Windows\System32\spool\PRINTERS";
                    const int MaxAttempts = 3;

                    // 停止服务
                    progress("正在停止 Print Spooler 服务…");
                    LogInfo("正在清空打印缓冲文件...");
                    using (var svc = new System.ServiceProcess.ServiceController("Spooler"))
                    {
                        if (svc.Status != System.ServiceProcess.ServiceControllerStatus.Stopped)
                        {
                            LogInfo("正在停止 Print Spooler 服务...");
                            svc.Stop();
                            svc.WaitForStatus(
                                System.ServiceProcess.ServiceControllerStatus.Stopped,
                                TimeSpan.FromSeconds(15)
                            );
                        }
                    }

                    // 循环清理，直到干净或达到最大次数
                    for (int i = 1; i <= MaxAttempts && !isCleaned; i++)
                    {
                        progress($"正在清理缓冲文件（第 {i} 次）…");
                        LogInfo($"第 {i} 次清理缓冲文件...");

                        if (Directory.Exists(PrintersPath))
                        {
                            // 删除所有文件
                            foreach (
                                var file in Directory.GetFiles(
                                    PrintersPath,
                                    "*.*",
                                    SearchOption.AllDirectories
                                )
                            )
                            {
                                try
                                {
                                    File.SetAttributes(file, FileAttributes.Normal);
                                    File.Delete(file);
                                }
                                catch (Exception fileEx)
                                {
                                    LogWarning(
                                        $"无法删除文件: {Path.GetFileName(file)} - {fileEx.Message}"
                                    );
                                }
                            }

                            // 删除子目录
                            foreach (var dir in Directory.GetDirectories(PrintersPath))
                            {
                                try
                                {
                                    Directory.Delete(dir, true);
                                }
                                catch (Exception dirEx)
                                {
                                    LogWarning(
                                        $"无法删除目录: {Path.GetFileName(dir)} - {dirEx.Message}"
                                    );
                                }
                            }
                        }

                        // 等待系统释放句柄
                        Thread.Sleep(500);

                        // 验证是否清空完成
                        if (Directory.Exists(PrintersPath))
                        {
                            var remainingFiles = Directory.GetFiles(
                                PrintersPath,
                                "*.*",
                                SearchOption.AllDirectories
                            );
                            var remainingDirs = Directory.GetDirectories(PrintersPath);

                            if (remainingFiles.Length == 0 && remainingDirs.Length == 0)
                            {
                                isCleaned = true;
                                LogSuccess("打印缓冲文件已清空完成。");
                            }
                            else
                            {
                                LogWarning(
                                    $"仍有 {remainingFiles.Length} 个文件、{remainingDirs.Length} 个目录未清理"
                                );
                            }
                        }
                        else
                        {
                            isCleaned = true;
                            LogSuccess("打印缓冲目录已清空。");
                        }
                    }

                    // 重新启动服务
                    progress("正在启动 Print Spooler 服务…");
                    LogInfo("正在启动 Print Spooler 服务...");
                    using (var svc = new System.ServiceProcess.ServiceController("Spooler"))
                    {
                        svc.Start();
                        svc.WaitForStatus(
                            System.ServiceProcess.ServiceControllerStatus.Running,
                            TimeSpan.FromSeconds(20)
                        );
                    }

                    // 最终结果
                    if (isCleaned)
                    {
                        LogSuccess("打印任务清空完成，服务已恢复。");
                    }
                    else
                    {
                        remainingCount = Directory.Exists(PrintersPath)
                            ? Directory
                                .GetFiles(PrintersPath, "*.*", SearchOption.AllDirectories)
                                .Length
                            : 0;

                        LogError($"清空未完成，仍有 {remainingCount} 个文件无法删除");
                    }
                },
                onSuccess: () =>
                {
                    RefreshPrinters();
                    if (!isCleaned)
                    {
                        MessageBox.Show(
                            $"清空未完成！\n\n仍有 {remainingCount} 个文件无法删除。\n建议重启电脑后再试，或检查是否有杀毒软件拦截。",
                            "清理失败",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                },
                onError: ex => ShowError("清空打印任务失败", ex)
            );
        }
    }
}
