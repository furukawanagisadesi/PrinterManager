using System;
using System.Drawing;
using System.Windows.Forms;

namespace PrinterManager.UI
{
    /// <summary>
    /// 模态等待窗体：显示转圈进度条 + 可实时更新的文字提示。
    /// </summary>
    public class WaitForm : Form
    {
        private readonly Label lblMessage;
        private readonly ProgressBar progressBar;

        public WaitForm(string message)
        {
            lblMessage = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 9.5F),
                ForeColor = Color.FromArgb(60, 70, 85),
                Text = message ?? "",
                TextAlign = ContentAlignment.MiddleLeft,
            };

            progressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                MarqueeAnimationSpeed = 30,
                Style = ProgressBarStyle.Marquee,
            };

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 14, 16, 10),
            };
            panel.Controls.Add(lblMessage);
            panel.Controls.Add(progressBar);

            Controls.Add(panel);

            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(380, 92);
            ControlBox = false;
            Font = new Font("微软雅黑", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "请稍候";
        }

        /// <summary>
        /// 更新提示文字（必须在 UI 线程调用）。
        /// </summary>
        public void SetMessage(string message)
        {
            lblMessage.Text = message;
        }
    }
}