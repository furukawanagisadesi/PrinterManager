namespace PrinterManager.UI
{
    partial class ScanPrinterForm
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
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblDesc = new System.Windows.Forms.Label();
            this.lblSubnet = new System.Windows.Forms.Label();
            this.panelToolbar = new System.Windows.Forms.Panel();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblTarget = new System.Windows.Forms.Label();
            this.txtTarget = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.lvResults = new System.Windows.Forms.ListView();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnInstall = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();

            this.panelTop.SuspendLayout();
            this.panelToolbar.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();

            // panelTop
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 70;
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(0, 95, 184);
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Controls.Add(this.lblDesc);
            this.panelTop.Controls.Add(this.lblSubnet);

            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(16, 10);
            this.lblTitle.Font = new System.Drawing.Font("微软雅黑", 12f, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Text = "扫描局域网共享打印机";

            this.lblDesc.AutoSize = true;
            this.lblDesc.Location = new System.Drawing.Point(18, 36);
            this.lblDesc.Font = new System.Drawing.Font("微软雅黑", 8.5f);
            this.lblDesc.ForeColor = System.Drawing.Color.FromArgb(200, 220, 240);
            this.lblDesc.Text = "输入单个 IP 扫描单台，输入 x.x.x.0 或 x.x.x 扫描整个网段";

            this.lblSubnet.AutoSize = true;
            this.lblSubnet.Location = new System.Drawing.Point(18, 52);
            this.lblSubnet.Font = new System.Drawing.Font("微软雅黑", 8f);
            this.lblSubnet.ForeColor = System.Drawing.Color.FromArgb(180, 210, 240);
            this.lblSubnet.Text = "本机 IP：检测中...";

            // panelToolbar
            this.panelToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelToolbar.Height = 80;
            this.panelToolbar.BackColor = System.Drawing.Color.FromArgb(245, 248, 252);
            this.panelToolbar.Controls.Add(this.btnScan);
            this.panelToolbar.Controls.Add(this.btnStop);
            this.panelToolbar.Controls.Add(this.lblTarget);
            this.panelToolbar.Controls.Add(this.txtTarget);
            this.panelToolbar.Controls.Add(this.progressBar);
            this.panelToolbar.Controls.Add(this.lblProgress);

            // 第一行：按钮 + 输入框
            this.btnScan.Text = "开始扫描";
            this.btnScan.Location = new System.Drawing.Point(8, 8);
            this.btnScan.Size = new System.Drawing.Size(100, 30);
            this.btnScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScan.BackColor = System.Drawing.Color.FromArgb(0, 120, 212);
            this.btnScan.ForeColor = System.Drawing.Color.White;
            this.btnScan.FlatAppearance.BorderSize = 0;
            this.btnScan.Font = new System.Drawing.Font("微软雅黑", 9f);
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);

            this.btnStop.Text = "停止";
            this.btnStop.Location = new System.Drawing.Point(116, 8);
            this.btnStop.Size = new System.Drawing.Size(80, 30);
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 195, 215);
            this.btnStop.Font = new System.Drawing.Font("微软雅黑", 9f);
            this.btnStop.Enabled = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);

            this.lblTarget.AutoSize = true;
            this.lblTarget.Location = new System.Drawing.Point(210, 14);
            this.lblTarget.Font = new System.Drawing.Font("微软雅黑", 9f);
            this.lblTarget.Text = "目标 IP / 网段：";

            this.txtTarget.Location = new System.Drawing.Point(318, 11);
            this.txtTarget.Size = new System.Drawing.Size(180, 23);
            this.txtTarget.Font = new System.Drawing.Font("Consolas", 10f);
            this.txtTarget.Text = "如 10.220.2.0 或 10.220.2.71";

            // 第二行：进度条
            this.progressBar.Location = new System.Drawing.Point(8, 52);
            this.progressBar.Size = new System.Drawing.Size(300, 16);
            this.progressBar.Minimum = 0;
            this.progressBar.Maximum = 254;

            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(318, 50);
            this.lblProgress.Font = new System.Drawing.Font("微软雅黑", 8.5f);
            this.lblProgress.ForeColor = System.Drawing.Color.FromArgb(90, 100, 120);
            this.lblProgress.Text = "输入目标后点击\"开始扫描\"";

            // lvResults
            this.lvResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvResults.View = System.Windows.Forms.View.Details;
            this.lvResults.FullRowSelect = true;
            this.lvResults.CheckBoxes = true;
            this.lvResults.GridLines = true;
            this.lvResults.Font = new System.Drawing.Font("微软雅黑", 9f);
            this.lvResults.BackColor = System.Drawing.Color.White;
            this.lvResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvResults.Columns.Add("主机 IP", 120);
            this.lvResults.Columns.Add("共享名", 150);
            this.lvResults.Columns.Add("UNC 路径", 220);
            this.lvResults.Columns.Add("备注", 160);
            this.lvResults.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvResults_ItemChecked);

            // panelBottom
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Height = 48;
            this.panelBottom.BackColor = System.Drawing.Color.FromArgb(245, 248, 252);
            this.panelBottom.Controls.Add(this.btnInstall);
            this.panelBottom.Controls.Add(this.btnClose);

            this.btnInstall.Text = "安装选中打印机";
            this.btnInstall.Location = new System.Drawing.Point(480, 10);
            this.btnInstall.Size = new System.Drawing.Size(140, 30);
            this.btnInstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInstall.BackColor = System.Drawing.Color.FromArgb(0, 150, 80);
            this.btnInstall.ForeColor = System.Drawing.Color.White;
            this.btnInstall.FlatAppearance.BorderSize = 0;
            this.btnInstall.Font = new System.Drawing.Font("微软雅黑", 9f);
            this.btnInstall.Enabled = false;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);

            this.btnClose.Text = "关闭";
            this.btnClose.Location = new System.Drawing.Point(630, 10);
            this.btnClose.Size = new System.Drawing.Size(80, 30);
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 195, 215);
            this.btnClose.Font = new System.Drawing.Font("微软雅黑", 9f);
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(730, 480);
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "扫描局域网共享打印机";
            this.Controls.Add(this.lvResults);
            this.Controls.Add(this.panelToolbar);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelBottom);
            this.Load += new System.EventHandler(this.ScanPrinterForm_Load);

            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelToolbar.ResumeLayout(false);
            this.panelToolbar.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblDesc;
        private System.Windows.Forms.Label lblSubnet;
        private System.Windows.Forms.Panel panelToolbar;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.TextBox txtTarget;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.ListView lvResults;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Button btnClose;
    }
}