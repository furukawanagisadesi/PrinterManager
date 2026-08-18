using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using PrinterManager.UI;

namespace PrinterManager.Helpers
{
    /// <summary>
    /// 进度消息回调：后台线程中调用，会安全地切换到 UI 线程更新等待框文字。
    /// </summary>
    public delegate void ReportProgress(string message);

    /// <summary>
    /// 在后台线程执行耗时操作，同时在 UI 线程显示模态转圈等待框。
    /// 完成后自动关闭等待框，并通过回调回到 UI 线程处理结果。
    /// </summary>
    public static class OperationRunner
    {
        /// <summary>
        /// 执行无进度回调的耗时操作。
        /// </summary>
        public static void Run(
            Form owner,
            string message,
            Action work,
            Action onSuccess = null,
            Action<Exception> onError = null
        )
        {
            Run(owner, message, progress => work(), onSuccess, onError);
        }

        /// <summary>
        /// 执行耗时操作，可实时更新等待框提示文字。
        /// </summary>
        public static void Run(
            Form owner,
            string message,
            Action<ReportProgress> work,
            Action onSuccess = null,
            Action<Exception> onError = null
        )
        {
            Exception error = null;

            using (var dlg = new WaitForm(message))
            {
                Task task = null;

                // 窗体显示后启动后台任务，避免任务过快结束导致对话框闪退
                dlg.Shown += (s, e) =>
                {
                    task = Task.Factory.StartNew(
                        () =>
                        {
                            try
                            {
                                work(
                                    report =>
                                    {
                                        try
                                        {
                                            if (dlg.IsHandleCreated)
                                                dlg.BeginInvoke(
                                                    (Action)(() => dlg.SetMessage(report))
                                                );
                                        }
                                        catch (InvalidOperationException)
                                        {
                                            // 窗体已关闭，忽略
                                        }
                                    }
                                );
                            }
                            catch (Exception ex)
                            {
                                error = ex;
                            }
                            finally
                            {
                                try
                                {
                                    if (dlg.IsHandleCreated)
                                        dlg.BeginInvoke((Action)dlg.Close);
                                }
                                catch (InvalidOperationException)
                                {
                                    // 窗体已关闭，忽略
                                }
                            }
                        },
                        TaskCreationOptions.LongRunning
                    );
                };

                dlg.ShowDialog(owner);

                if (task != null)
                    task.Wait();

                if (error != null)
                {
                    if (onError != null)
                        onError(error);
                    else
                        MessageBox.Show(
                            owner,
                            error.Message,
                            "错误",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                }
                else if (onSuccess != null)
                {
                    onSuccess();
                }
            }
        }
    }
}