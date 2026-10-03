using System;
using System.Drawing;
using System.Windows.Forms;

namespace PrinterManager.Helpers
{
    /// <summary>
    /// DPI 换算辅助。
    /// 刻意不使用 .NET Framework 4.7+ 才有的 Control.LogicalToDeviceUnits / Control.DeviceDpi，
    /// 以便在较低版本运行时（如 Win7 自带的 4.5.2）上也能运行；96 DPI 时返回原值。
    /// </summary>
    internal static class DpiHelper
    {
        /// <summary>
        /// 把逻辑单位按控件当前所在显示器的 DPI 换算为设备像素。
        /// </summary>
        public static int LogicalToDevice(Control control, int value)
        {
            if (control == null || value == 0)
                return value;

            try
            {
                using (Graphics g = control.CreateGraphics())
                {
                    float scale = g.DpiX / 96f;
                    if (scale <= 0f || Math.Abs(scale - 1f) < 0.001f)
                        return value;
                    return (int)Math.Round(value * scale);
                }
            }
            catch
            {
                // 取不到 DPI 时按原值返回，保证不影响功能
                return value;
            }
        }
    }
}
