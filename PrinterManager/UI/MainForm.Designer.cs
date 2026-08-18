namespace PrinterManager.UI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPrinters = new System.Windows.Forms.TabPage();
            this.splitPrinters = new System.Windows.Forms.SplitContainer();
            this.lvPrinters = new System.Windows.Forms.ListView();
            this.panelPrinterToolbar = new System.Windows.Forms.Panel();
            this.btnCleanPrinters = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnAddNetwork = new System.Windows.Forms.Button();
            this.btnDeletePrinter = new System.Windows.Forms.Button();
            this.btnSetDefault = new System.Windows.Forms.Button();
            this.btnToggleShare = new System.Windows.Forms.Button();
            this.btnRestartSpooler = new System.Windows.Forms.Button();
            this.lblPrinterCount = new System.Windows.Forms.Label();
            this.panelPrinterLogToolbar = new System.Windows.Forms.Panel();
            this.btnClearPrinterLog = new System.Windows.Forms.Button();
            this.lblPrinterLogTitle = new System.Windows.Forms.Label();
            this.rtbPrinterLog = new System.Windows.Forms.RichTextBox();
            this.tabDrivers = new System.Windows.Forms.TabPage();
            this.splitDrivers = new System.Windows.Forms.SplitContainer();
            this.lvDrivers = new System.Windows.Forms.ListView();
            this.panelDriverToolbar = new System.Windows.Forms.Panel();
            this.btnRefresh2 = new System.Windows.Forms.Button();
            this.btnInstallDriver = new System.Windows.Forms.Button();
            this.btnDeleteDriver = new System.Windows.Forms.Button();
            this.lblDriverCount = new System.Windows.Forms.Label();
            this.panelDriverLogToolbar = new System.Windows.Forms.Panel();
            this.btnClearDriverLog = new System.Windows.Forms.Button();
            this.lblDriverLogTitle = new System.Windows.Forms.Label();
            this.rtbDriverLog = new System.Windows.Forms.RichTextBox();
            this.tabLog = new System.Windows.Forms.TabPage();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.panelLogToolbar = new System.Windows.Forms.Panel();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.splitContainerPrinters = new System.Windows.Forms.SplitContainer();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.tabControl.SuspendLayout();
            this.tabPrinters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitPrinters)).BeginInit();
            this.splitPrinters.Panel1.SuspendLayout();
            this.splitPrinters.Panel2.SuspendLayout();
            this.splitPrinters.SuspendLayout();
            this.panelPrinterToolbar.SuspendLayout();
            this.panelPrinterLogToolbar.SuspendLayout();
            this.tabDrivers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitDrivers)).BeginInit();
            this.splitDrivers.Panel1.SuspendLayout();
            this.splitDrivers.Panel2.SuspendLayout();
            this.splitDrivers.SuspendLayout();
            this.panelDriverToolbar.SuspendLayout();
            this.panelDriverLogToolbar.SuspendLayout();
            this.tabLog.SuspendLayout();
            this.panelLogToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPrinters)).BeginInit();
            this.splitContainerPrinters.SuspendLayout();
            this.panelStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPrinters);
            this.tabControl.Controls.Add(this.tabDrivers);
            this.tabControl.Controls.Add(this.tabLog);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1100, 674);
            this.tabControl.TabIndex = 0;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // tabPrinters
            // 
            this.tabPrinters.Controls.Add(this.splitPrinters);
            this.tabPrinters.Location = new System.Drawing.Point(4, 28);
            this.tabPrinters.Name = "tabPrinters";
            this.tabPrinters.Padding = new System.Windows.Forms.Padding(4);
            this.tabPrinters.Size = new System.Drawing.Size(1092, 642);
            this.tabPrinters.TabIndex = 0;
            this.tabPrinters.Text = "打印机列表  ";
            // 
            // splitPrinters
            // 
            this.splitPrinters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitPrinters.Location = new System.Drawing.Point(4, 4);
            this.splitPrinters.Name = "splitPrinters";
            this.splitPrinters.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitPrinters.Panel1.Controls.Add(this.lvPrinters);
            this.splitPrinters.Panel1.Controls.Add(this.panelPrinterToolbar);
            this.splitPrinters.Panel1MinSize = 200;
            this.splitPrinters.Panel2.Controls.Add(this.rtbPrinterLog);
            this.splitPrinters.Panel2.Controls.Add(this.panelPrinterLogToolbar);
            this.splitPrinters.Panel2MinSize = 100;
            this.splitPrinters.Size = new System.Drawing.Size(1084, 634);
            this.splitPrinters.SplitterDistance = 450;
            this.splitPrinters.SplitterWidth = 6;
            this.splitPrinters.TabIndex = 0;
            // 
            // lvPrinters
            // 
            this.lvPrinters.BackColor = System.Drawing.Color.White;
            this.lvPrinters.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvPrinters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvPrinters.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lvPrinters.FullRowSelect = true;
            this.lvPrinters.GridLines = true;
            this.lvPrinters.HideSelection = false;
            this.lvPrinters.Location = new System.Drawing.Point(0, 44);
            this.lvPrinters.MultiSelect = false;
            this.lvPrinters.Name = "lvPrinters";
            this.lvPrinters.Size = new System.Drawing.Size(1084, 406);
            this.lvPrinters.TabIndex = 0;
            this.lvPrinters.UseCompatibleStateImageBehavior = false;
            this.lvPrinters.View = System.Windows.Forms.View.Details;
            this.lvPrinters.SelectedIndexChanged += new System.EventHandler(this.lvPrinters_SelectedIndexChanged);
            this.lvPrinters.DoubleClick += new System.EventHandler(this.lvPrinters_DoubleClick);
            // 
            // panelPrinterToolbar
            // 
            this.panelPrinterToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(255)))));
            this.panelPrinterToolbar.Controls.Add(this.btnCleanPrinters);
            this.panelPrinterToolbar.Controls.Add(this.btnRefresh);
            this.panelPrinterToolbar.Controls.Add(this.btnAddNetwork);
            this.panelPrinterToolbar.Controls.Add(this.btnDeletePrinter);
            this.panelPrinterToolbar.Controls.Add(this.btnSetDefault);
            this.panelPrinterToolbar.Controls.Add(this.btnToggleShare);
            this.panelPrinterToolbar.Controls.Add(this.btnRestartSpooler);
            this.panelPrinterToolbar.Controls.Add(this.lblPrinterCount);
            this.panelPrinterToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelPrinterToolbar.Location = new System.Drawing.Point(0, 0);
            this.panelPrinterToolbar.Name = "panelPrinterToolbar";
            this.panelPrinterToolbar.Padding = new System.Windows.Forms.Padding(4, 6, 4, 4);
            this.panelPrinterToolbar.Size = new System.Drawing.Size(1084, 44);
            this.panelPrinterToolbar.TabIndex = 1;
            // 
            // btnRefresh
            // 
            this.btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(195)))), ((int)(((byte)(215)))));
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnRefresh.Location = new System.Drawing.Point(4, 7);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(80, 30);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnAddNetwork
            // 
            this.btnAddNetwork.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.btnAddNetwork.FlatAppearance.BorderSize = 0;
            this.btnAddNetwork.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddNetwork.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnAddNetwork.ForeColor = System.Drawing.Color.White;
            this.btnAddNetwork.Location = new System.Drawing.Point(94, 7);
            this.btnAddNetwork.Name = "btnAddNetwork";
            this.btnAddNetwork.Size = new System.Drawing.Size(140, 30);
            this.btnAddNetwork.TabIndex = 1;
            this.btnAddNetwork.Text = "添加网络打印机";
            this.btnAddNetwork.UseVisualStyleBackColor = false;
            this.btnAddNetwork.Click += new System.EventHandler(this.btnAddNetwork_Click);
            // 
            // btnDeletePrinter
            // 
            this.btnDeletePrinter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(43)))), ((int)(((byte)(28)))));
            this.btnDeletePrinter.Enabled = false;
            this.btnDeletePrinter.FlatAppearance.BorderSize = 0;
            this.btnDeletePrinter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeletePrinter.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnDeletePrinter.ForeColor = System.Drawing.Color.White;
            this.btnDeletePrinter.Location = new System.Drawing.Point(244, 7);
            this.btnDeletePrinter.Name = "btnDeletePrinter";
            this.btnDeletePrinter.Size = new System.Drawing.Size(110, 30);
            this.btnDeletePrinter.TabIndex = 2;
            this.btnDeletePrinter.Text = "删除打印机";
            this.btnDeletePrinter.UseVisualStyleBackColor = false;
            this.btnDeletePrinter.Click += new System.EventHandler(this.btnDeletePrinter_Click);
            // 
            // btnSetDefault
            // 
            this.btnSetDefault.Enabled = false;
            this.btnSetDefault.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(195)))), ((int)(((byte)(215)))));
            this.btnSetDefault.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetDefault.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnSetDefault.Location = new System.Drawing.Point(364, 7);
            this.btnSetDefault.Name = "btnSetDefault";
            this.btnSetDefault.Size = new System.Drawing.Size(100, 30);
            this.btnSetDefault.TabIndex = 3;
            this.btnSetDefault.Text = "设为默认";
            this.btnSetDefault.Click += new System.EventHandler(this.btnSetDefault_Click);
            // 
            // btnToggleShare
            // 
            this.btnToggleShare.Enabled = false;
            this.btnToggleShare.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(195)))), ((int)(((byte)(215)))));
            this.btnToggleShare.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToggleShare.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnToggleShare.Location = new System.Drawing.Point(474, 7);
            this.btnToggleShare.Name = "btnToggleShare";
            this.btnToggleShare.Size = new System.Drawing.Size(100, 30);
            this.btnToggleShare.TabIndex = 7;
            this.btnToggleShare.Text = "共享设置";
            this.btnToggleShare.Click += new System.EventHandler(this.btnToggleShare_Click);
            // 
            // btnRestartSpooler
            // 
            this.btnRestartSpooler.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(195)))), ((int)(((byte)(215)))));
            this.btnRestartSpooler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestartSpooler.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnRestartSpooler.Location = new System.Drawing.Point(584, 7);
            this.btnRestartSpooler.Name = "btnRestartSpooler";
            this.btnRestartSpooler.Size = new System.Drawing.Size(120, 30);
            this.btnRestartSpooler.TabIndex = 4;
            this.btnRestartSpooler.Text = "重启打印服务";
            this.btnRestartSpooler.Click += new System.EventHandler(this.btnRestartSpooler_Click);
            // 
            // lblPrinterCount
            // 
            this.lblPrinterCount.AutoSize = true;
            this.lblPrinterCount.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblPrinterCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(110)))), ((int)(((byte)(130)))));
            this.lblPrinterCount.Location = new System.Drawing.Point(844, 13);
            this.lblPrinterCount.Name = "lblPrinterCount";
            this.lblPrinterCount.Size = new System.Drawing.Size(83, 17);
            this.lblPrinterCount.TabIndex = 5;
            this.lblPrinterCount.Text = "共 0 台打印机";
            // 
            // panelPrinterLogToolbar
            // 
            this.panelPrinterLogToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(255)))));
            this.panelPrinterLogToolbar.Controls.Add(this.btnClearPrinterLog);
            this.panelPrinterLogToolbar.Controls.Add(this.lblPrinterLogTitle);
            this.panelPrinterLogToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelPrinterLogToolbar.Location = new System.Drawing.Point(0, 0);
            this.panelPrinterLogToolbar.Name = "panelPrinterLogToolbar";
            this.panelPrinterLogToolbar.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelPrinterLogToolbar.Size = new System.Drawing.Size(1084, 32);
            this.panelPrinterLogToolbar.TabIndex = 1;
            // 
            // btnClearPrinterLog
            // 
            this.btnClearPrinterLog.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(195)))), ((int)(((byte)(215)))));
            this.btnClearPrinterLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearPrinterLog.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnClearPrinterLog.Location = new System.Drawing.Point(4, 2);
            this.btnClearPrinterLog.Name = "btnClearPrinterLog";
            this.btnClearPrinterLog.Size = new System.Drawing.Size(80, 26);
            this.btnClearPrinterLog.TabIndex = 0;
            this.btnClearPrinterLog.Text = "清空日志";
            this.btnClearPrinterLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // lblPrinterLogTitle
            // 
            this.lblPrinterLogTitle.AutoSize = true;
            this.lblPrinterLogTitle.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblPrinterLogTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(110)))), ((int)(((byte)(130)))));
            this.lblPrinterLogTitle.Location = new System.Drawing.Point(94, 7);
            this.lblPrinterLogTitle.Name = "lblPrinterLogTitle";
            this.lblPrinterLogTitle.Size = new System.Drawing.Size(55, 17);
            this.lblPrinterLogTitle.TabIndex = 1;
            this.lblPrinterLogTitle.Text = "操作日志";
            // 
            // rtbPrinterLog
            // 
            this.rtbPrinterLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(28)))));
            this.rtbPrinterLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbPrinterLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbPrinterLog.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.rtbPrinterLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(225)))), ((int)(((byte)(235)))));
            this.rtbPrinterLog.Location = new System.Drawing.Point(0, 32);
            this.rtbPrinterLog.Name = "rtbPrinterLog";
            this.rtbPrinterLog.ReadOnly = true;
            this.rtbPrinterLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbPrinterLog.Size = new System.Drawing.Size(1084, 146);
            this.rtbPrinterLog.TabIndex = 0;
            this.rtbPrinterLog.Text = "";
            // 
            // tabDrivers
            // 
            this.tabDrivers.Controls.Add(this.splitDrivers);
            this.tabDrivers.Location = new System.Drawing.Point(4, 28);
            this.tabDrivers.Name = "tabDrivers";
            this.tabDrivers.Padding = new System.Windows.Forms.Padding(4);
            this.tabDrivers.Size = new System.Drawing.Size(1092, 642);
            this.tabDrivers.TabIndex = 1;
            this.tabDrivers.Text = "驱动程序  ";
            // 
            // splitDrivers
            // 
            this.splitDrivers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitDrivers.Location = new System.Drawing.Point(4, 4);
            this.splitDrivers.Name = "splitDrivers";
            this.splitDrivers.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitDrivers.Panel1.Controls.Add(this.lvDrivers);
            this.splitDrivers.Panel1.Controls.Add(this.panelDriverToolbar);
            this.splitDrivers.Panel1MinSize = 200;
            this.splitDrivers.Panel2.Controls.Add(this.rtbDriverLog);
            this.splitDrivers.Panel2.Controls.Add(this.panelDriverLogToolbar);
            this.splitDrivers.Panel2MinSize = 100;
            this.splitDrivers.Size = new System.Drawing.Size(1084, 634);
            this.splitDrivers.SplitterDistance = 450;
            this.splitDrivers.SplitterWidth = 6;
            this.splitDrivers.TabIndex = 0;
            // 
            // lvDrivers
            // 
            this.lvDrivers.BackColor = System.Drawing.Color.White;
            this.lvDrivers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvDrivers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvDrivers.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lvDrivers.FullRowSelect = true;
            this.lvDrivers.GridLines = true;
            this.lvDrivers.HideSelection = false;
            this.lvDrivers.Location = new System.Drawing.Point(0, 44);
            this.lvDrivers.MultiSelect = false;
            this.lvDrivers.Name = "lvDrivers";
            this.lvDrivers.Size = new System.Drawing.Size(1084, 406);
            this.lvDrivers.TabIndex = 0;
            this.lvDrivers.UseCompatibleStateImageBehavior = false;
            this.lvDrivers.View = System.Windows.Forms.View.Details;
            this.lvDrivers.SelectedIndexChanged += new System.EventHandler(this.lvDrivers_SelectedIndexChanged);
            // 
            // panelDriverToolbar
            // 
            this.panelDriverToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(255)))));
            this.panelDriverToolbar.Controls.Add(this.btnRefresh2);
            this.panelDriverToolbar.Controls.Add(this.btnInstallDriver);
            this.panelDriverToolbar.Controls.Add(this.btnDeleteDriver);
            this.panelDriverToolbar.Controls.Add(this.lblDriverCount);
            this.panelDriverToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelDriverToolbar.Location = new System.Drawing.Point(0, 0);
            this.panelDriverToolbar.Name = "panelDriverToolbar";
            this.panelDriverToolbar.Padding = new System.Windows.Forms.Padding(4, 6, 4, 4);
            this.panelDriverToolbar.Size = new System.Drawing.Size(1084, 44);
            this.panelDriverToolbar.TabIndex = 1;
            // 
            // btnRefresh2
            // 
            this.btnRefresh2.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(195)))), ((int)(((byte)(215)))));
            this.btnRefresh2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh2.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnRefresh2.Location = new System.Drawing.Point(4, 7);
            this.btnRefresh2.Name = "btnRefresh2";
            this.btnRefresh2.Size = new System.Drawing.Size(80, 30);
            this.btnRefresh2.TabIndex = 2;
            this.btnRefresh2.Text = "刷新";
            this.btnRefresh2.Click += new System.EventHandler(this.btnRefresh2_Click);
            // 
            // btnInstallDriver
            // 
            this.btnInstallDriver.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnInstallDriver.FlatAppearance.BorderSize = 0;
            this.btnInstallDriver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInstallDriver.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnInstallDriver.ForeColor = System.Drawing.Color.White;
            this.btnInstallDriver.Location = new System.Drawing.Point(93, 7);
            this.btnInstallDriver.Name = "btnInstallDriver";
            this.btnInstallDriver.Size = new System.Drawing.Size(130, 30);
            this.btnInstallDriver.TabIndex = 3;
            this.btnInstallDriver.Text = "安装驱动程序";
            this.btnInstallDriver.UseVisualStyleBackColor = false;
            this.btnInstallDriver.Click += new System.EventHandler(this.btnInstallDriver_Click);
            // 
            // btnDeleteDriver
            // 
            this.btnDeleteDriver.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(43)))), ((int)(((byte)(28)))));
            this.btnDeleteDriver.Enabled = false;
            this.btnDeleteDriver.FlatAppearance.BorderSize = 0;
            this.btnDeleteDriver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteDriver.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnDeleteDriver.ForeColor = System.Drawing.Color.White;
            this.btnDeleteDriver.Location = new System.Drawing.Point(229, 7);
            this.btnDeleteDriver.Name = "btnDeleteDriver";
            this.btnDeleteDriver.Size = new System.Drawing.Size(130, 30);
            this.btnDeleteDriver.TabIndex = 0;
            this.btnDeleteDriver.Text = "删除驱动程序";
            this.btnDeleteDriver.UseVisualStyleBackColor = false;
            this.btnDeleteDriver.Click += new System.EventHandler(this.btnDeleteDriver_Click);
            // 
            // lblDriverCount
            // 
            this.lblDriverCount.AutoSize = true;
            this.lblDriverCount.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblDriverCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(110)))), ((int)(((byte)(130)))));
            this.lblDriverCount.Location = new System.Drawing.Point(365, 14);
            this.lblDriverCount.Name = "lblDriverCount";
            this.lblDriverCount.Size = new System.Drawing.Size(71, 17);
            this.lblDriverCount.TabIndex = 1;
            this.lblDriverCount.Text = "共 0 个驱动";
            // 
            // panelDriverLogToolbar
            // 
            this.panelDriverLogToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(255)))));
            this.panelDriverLogToolbar.Controls.Add(this.btnClearDriverLog);
            this.panelDriverLogToolbar.Controls.Add(this.lblDriverLogTitle);
            this.panelDriverLogToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelDriverLogToolbar.Location = new System.Drawing.Point(0, 0);
            this.panelDriverLogToolbar.Name = "panelDriverLogToolbar";
            this.panelDriverLogToolbar.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelDriverLogToolbar.Size = new System.Drawing.Size(1084, 32);
            this.panelDriverLogToolbar.TabIndex = 1;
            // 
            // btnClearDriverLog
            // 
            this.btnClearDriverLog.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(195)))), ((int)(((byte)(215)))));
            this.btnClearDriverLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearDriverLog.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnClearDriverLog.Location = new System.Drawing.Point(4, 2);
            this.btnClearDriverLog.Name = "btnClearDriverLog";
            this.btnClearDriverLog.Size = new System.Drawing.Size(80, 26);
            this.btnClearDriverLog.TabIndex = 0;
            this.btnClearDriverLog.Text = "清空日志";
            this.btnClearDriverLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // lblDriverLogTitle
            // 
            this.lblDriverLogTitle.AutoSize = true;
            this.lblDriverLogTitle.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblDriverLogTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(110)))), ((int)(((byte)(130)))));
            this.lblDriverLogTitle.Location = new System.Drawing.Point(94, 7);
            this.lblDriverLogTitle.Name = "lblDriverLogTitle";
            this.lblDriverLogTitle.Size = new System.Drawing.Size(55, 17);
            this.lblDriverLogTitle.TabIndex = 1;
            this.lblDriverLogTitle.Text = "操作日志";
            // 
            // rtbDriverLog
            // 
            this.rtbDriverLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(28)))));
            this.rtbDriverLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbDriverLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbDriverLog.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.rtbDriverLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(225)))), ((int)(((byte)(235)))));
            this.rtbDriverLog.Location = new System.Drawing.Point(0, 32);
            this.rtbDriverLog.Name = "rtbDriverLog";
            this.rtbDriverLog.ReadOnly = true;
            this.rtbDriverLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbDriverLog.Size = new System.Drawing.Size(1084, 146);
            this.rtbDriverLog.TabIndex = 0;
            this.rtbDriverLog.Text = "";
            // 
            // tabLog
            // 
            this.tabLog.Controls.Add(this.rtbLog);
            this.tabLog.Controls.Add(this.panelLogToolbar);
            this.tabLog.Location = new System.Drawing.Point(4, 28);
            this.tabLog.Name = "tabLog";
            this.tabLog.Padding = new System.Windows.Forms.Padding(4);
            this.tabLog.Size = new System.Drawing.Size(1092, 642);
            this.tabLog.TabIndex = 2;
            this.tabLog.Text = "  操作日志  ";
            // 
            // rtbLog
            // 
            this.rtbLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(28)))));
            this.rtbLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLog.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.rtbLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(225)))), ((int)(((byte)(235)))));
            this.rtbLog.Location = new System.Drawing.Point(4, 48);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbLog.Size = new System.Drawing.Size(1084, 590);
            this.rtbLog.TabIndex = 0;
            this.rtbLog.Text = "";
            // 
            // panelLogToolbar
            // 
            this.panelLogToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(255)))));
            this.panelLogToolbar.Controls.Add(this.btnClearLog);
            this.panelLogToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLogToolbar.Location = new System.Drawing.Point(4, 4);
            this.panelLogToolbar.Name = "panelLogToolbar";
            this.panelLogToolbar.Padding = new System.Windows.Forms.Padding(4, 6, 4, 4);
            this.panelLogToolbar.Size = new System.Drawing.Size(1084, 44);
            this.panelLogToolbar.TabIndex = 1;
            // 
            // btnClearLog
            // 
            this.btnClearLog.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(195)))), ((int)(((byte)(215)))));
            this.btnClearLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearLog.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnClearLog.Location = new System.Drawing.Point(4, 7);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(100, 30);
            this.btnClearLog.TabIndex = 0;
            this.btnClearLog.Text = "清空日志";
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // splitContainerPrinters
            // 
            this.splitContainerPrinters.Location = new System.Drawing.Point(0, 0);
            this.splitContainerPrinters.Name = "splitContainerPrinters";
            this.splitContainerPrinters.Size = new System.Drawing.Size(150, 100);
            this.splitContainerPrinters.TabIndex = 0;
            // 
            // panelStatus
            // 
            this.panelStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(95)))), ((int)(((byte)(184)))));
            this.panelStatus.Controls.Add(this.lblStatus);
            this.panelStatus.Controls.Add(this.progressBar);
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStatus.Location = new System.Drawing.Point(0, 674);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(1100, 26);
            this.panelStatus.TabIndex = 1;
            // 
            // lblStatus
            // 
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = new System.Drawing.Font("微软雅黑", 8.5F);
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.Location = new System.Drawing.Point(0, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblStatus.Size = new System.Drawing.Size(980, 26);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "就绪";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.progressBar.Location = new System.Drawing.Point(980, 0);
            this.progressBar.MarqueeAnimationSpeed = 30;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(120, 26);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 1;
            this.progressBar.Visible = false;
            // 
            // btnCleanPrinters
            // 
            this.btnCleanPrinters.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(195)))), ((int)(((byte)(215)))));
            this.btnCleanPrinters.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCleanPrinters.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnCleanPrinters.Location = new System.Drawing.Point(714, 7);
            this.btnCleanPrinters.Name = "btnCleanPrinters";
            this.btnCleanPrinters.Size = new System.Drawing.Size(120, 30);
            this.btnCleanPrinters.TabIndex = 6;
            this.btnCleanPrinters.Text = "清空打印任务";
            this.btnCleanPrinters.Click += new System.EventHandler(this.btnCleanPrinters_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.panelStatus);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.MinimumSize = new System.Drawing.Size(800, 520);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "打印机管理工具 - Printer Manager";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl.ResumeLayout(false);
            this.tabPrinters.ResumeLayout(false);
            this.splitPrinters.Panel1.ResumeLayout(false);
            this.splitPrinters.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPrinters)).EndInit();
            this.splitPrinters.ResumeLayout(false);
            this.panelPrinterToolbar.ResumeLayout(false);
            this.panelPrinterToolbar.PerformLayout();
            this.panelPrinterLogToolbar.ResumeLayout(false);
            this.panelPrinterLogToolbar.PerformLayout();
            this.tabDrivers.ResumeLayout(false);
            this.splitDrivers.Panel1.ResumeLayout(false);
            this.splitDrivers.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitDrivers)).EndInit();
            this.splitDrivers.ResumeLayout(false);
            this.panelDriverToolbar.ResumeLayout(false);
            this.panelDriverToolbar.PerformLayout();
            this.panelDriverLogToolbar.ResumeLayout(false);
            this.panelDriverLogToolbar.PerformLayout();
            this.tabLog.ResumeLayout(false);
            this.panelLogToolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPrinters)).EndInit();
            this.splitContainerPrinters.ResumeLayout(false);
            this.panelStatus.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPrinters;
        private System.Windows.Forms.TabPage tabDrivers;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.SplitContainer splitContainerPrinters;
        private System.Windows.Forms.Panel panelPrinterToolbar;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnAddNetwork;
        private System.Windows.Forms.Button btnDeletePrinter;
        private System.Windows.Forms.Button btnSetDefault;
        private System.Windows.Forms.Button btnToggleShare;
        private System.Windows.Forms.Button btnRestartSpooler;
        private System.Windows.Forms.Label lblPrinterCount;
        private System.Windows.Forms.ListView lvPrinters;
        private System.Windows.Forms.Panel panelDriverToolbar;
        private System.Windows.Forms.Button btnDeleteDriver;
        private System.Windows.Forms.Button btnInstallDriver;
        private System.Windows.Forms.Label lblDriverCount;
        private System.Windows.Forms.ListView lvDrivers;
        private System.Windows.Forms.Panel panelLogToolbar;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnRefresh2;
        private System.Windows.Forms.Button btnCleanPrinters;
        private System.Windows.Forms.SplitContainer splitPrinters;
        private System.Windows.Forms.RichTextBox rtbPrinterLog;
        private System.Windows.Forms.Panel panelPrinterLogToolbar;
        private System.Windows.Forms.Button btnClearPrinterLog;
        private System.Windows.Forms.Label lblPrinterLogTitle;
        private System.Windows.Forms.SplitContainer splitDrivers;
        private System.Windows.Forms.RichTextBox rtbDriverLog;
        private System.Windows.Forms.Panel panelDriverLogToolbar;
        private System.Windows.Forms.Button btnClearDriverLog;
        private System.Windows.Forms.Label lblDriverLogTitle;
    }
}