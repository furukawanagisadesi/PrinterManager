namespace PrinterManager.UI
{
    partial class AddNetworkPrinterForm
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblDesc = new System.Windows.Forms.Label();
            this.lblUncPath = new System.Windows.Forms.Label();
            this.txtUncPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblHint = new System.Windows.Forms.Label();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnScanLan = new System.Windows.Forms.Button();
            this.panelButtons.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(16, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(154, 22);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "添加网络共享打印机";
            // 
            // lblDesc
            // 
            this.lblDesc.AutoSize = true;
            this.lblDesc.Font = new System.Drawing.Font("微软雅黑", 8.5F);
            this.lblDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(220)))), ((int)(((byte)(240)))));
            this.lblDesc.Location = new System.Drawing.Point(18, 36);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(231, 17);
            this.lblDesc.TabIndex = 1;
            this.lblDesc.Text = "输入共享打印机的网络路径（UNC 格式）";
            // 
            // lblUncPath
            // 
            this.lblUncPath.AutoSize = true;
            this.lblUncPath.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.lblUncPath.Location = new System.Drawing.Point(20, 82);
            this.lblUncPath.Name = "lblUncPath";
            this.lblUncPath.Size = new System.Drawing.Size(143, 19);
            this.lblUncPath.TabIndex = 0;
            this.lblUncPath.Text = "打印机路径（UNC）：";
            // 
            // txtUncPath
            // 
            this.txtUncPath.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtUncPath.Location = new System.Drawing.Point(20, 106);
            this.txtUncPath.Name = "txtUncPath";
            this.txtUncPath.Size = new System.Drawing.Size(340, 23);
            this.txtUncPath.TabIndex = 1;
            this.txtUncPath.Text = "\\\\";
            this.txtUncPath.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtUncPath_KeyDown);
            // 
            // btnBrowse
            // 
            this.btnBrowse.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(195)))), ((int)(((byte)(215)))));
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowse.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnBrowse.Location = new System.Drawing.Point(368, 105);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(72, 26);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "浏览...";
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblHint
            // 
            this.lblHint.Font = new System.Drawing.Font("微软雅黑", 8.5F);
            this.lblHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(110)))), ((int)(((byte)(130)))));
            this.lblHint.Location = new System.Drawing.Point(21, 169);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(420, 62);
            this.lblHint.TabIndex = 3;
            this.lblHint.Text = "示例格式：\r\n  \\\\192.168.1.100\\HP-LaserJet\r\n  \\\\OFFICE-SERVER\\Canon-MF\r\n\r\n提示：确保您有访问该共享打" +
    "印机的网络权限。";
            // 
            // panelButtons
            // 
            this.panelButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.panelButtons.Controls.Add(this.btnOK);
            this.panelButtons.Controls.Add(this.btnCancel);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 236);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(460, 48);
            this.panelButtons.TabIndex = 5;
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.btnOK.FlatAppearance.BorderSize = 0;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOK.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnOK.ForeColor = System.Drawing.Color.White;
            this.btnOK.Location = new System.Drawing.Point(230, 10);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 30);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "连接打印机";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(195)))), ((int)(((byte)(215)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnCancel.Location = new System.Drawing.Point(340, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "取消";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(95)))), ((int)(((byte)(184)))));
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Controls.Add(this.lblDesc);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(460, 60);
            this.panelTop.TabIndex = 4;
            // 
            // btnScanLan
            // 
            this.btnScanLan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.btnScanLan.FlatAppearance.BorderSize = 0;
            this.btnScanLan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScanLan.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.btnScanLan.ForeColor = System.Drawing.Color.White;
            this.btnScanLan.Location = new System.Drawing.Point(20, 138);
            this.btnScanLan.Name = "btnScanLan";
            this.btnScanLan.Size = new System.Drawing.Size(120, 28);
            this.btnScanLan.TabIndex = 6;
            this.btnScanLan.Text = "扫描局域网";
            this.btnScanLan.UseVisualStyleBackColor = false;
            this.btnScanLan.Click += new System.EventHandler(this.btnScanLan_Click);
            // 
            // AddNetworkPrinterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(460, 284);
            this.Controls.Add(this.lblUncPath);
            this.Controls.Add(this.txtUncPath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.btnScanLan);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddNetworkPrinterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "添加网络打印机";
            this.panelButtons.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblDesc;
        private System.Windows.Forms.Label lblUncPath;
        private System.Windows.Forms.TextBox txtUncPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnScanLan;
    }
}
