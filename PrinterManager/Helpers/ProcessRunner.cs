using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PrinterManager.Helpers
{
    /// <summary>
    /// 进程执行结果
    /// </summary>
    public class ProcessResult
    {
        public bool Success { get; }
        public string Output { get; }

        public ProcessResult(bool success, string output)
        {
            Success = success;
            Output = output;
        }

        public void Deconstruct(out bool success, out string output)
        {
            success = Success;
            output = Output;
        }
    }

    /// <summary>
    /// 通用进程执行辅助方法。
    /// 用两个并行 Task 读取 stdout/stderr，彻底避免缓冲区死锁。
    /// </summary>
    public static class ProcessRunner
    {
        public static ProcessResult Run(string fileName, string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(psi))
            {
                var outputTask = Task.Factory.StartNew(() => process.StandardOutput.ReadToEnd());
                var errorTask = Task.Factory.StartNew(() => process.StandardError.ReadToEnd());

                bool exited = process.WaitForExit(60000);
                if (!exited)
                {
                    process.Kill();
                    throw new TimeoutException(
                        string.Format("进程 '{0}' 执行超过 60 秒，已强制终止。", fileName));
                }

                // 在 .NET Framework 上，WaitForExit(timeout) 不保证异步重定向管道已刷新完毕；
                // 必须再调用一次无参 WaitForExit() 以确保 stdout/stderr 全部读取完成。
                process.WaitForExit();

                string output = outputTask.Result;
                string error = errorTask.Result;

                string fullOutput = output;
                if (!string.IsNullOrEmpty(error))
                    fullOutput += "\n[Error]\n" + error;

                return new ProcessResult(process.ExitCode == 0, fullOutput);
            }
        }
    }
}
