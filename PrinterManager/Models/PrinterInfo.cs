using System;

namespace PrinterManager.Models
{
    /// <summary>
    /// 打印机信息模型
    /// </summary>
    public class PrinterInfo
    {
        public string Name { get; set; }
        public string ServerName { get; set; }
        public string ShareName { get; set; }
        public string PortName { get; set; }
        public string DriverName { get; set; }
        public string Comment { get; set; }
        public string Location { get; set; }
        public uint Attributes { get; set; }
        public uint Status { get; set; }
        public uint JobCount { get; set; }
        public bool IsDefault { get; set; }

        public bool IsShared => (Attributes & Core.PrinterApiWrapper.PRINTER_ATTRIBUTE_SHARED) != 0;
        public bool IsNetwork => (Attributes & Core.PrinterApiWrapper.PRINTER_ATTRIBUTE_NETWORK) != 0;
        public bool IsLocal => (Attributes & Core.PrinterApiWrapper.PRINTER_ATTRIBUTE_LOCAL) != 0;

        public string StatusText
        {
            get
            {
                if (Status == 0) return "就绪";
                if ((Status & 0x00000001) != 0) return "暂停";
                if ((Status & 0x00000002) != 0) return "出错";
                if ((Status & 0x00000004) != 0) return "等待删除";
                if ((Status & 0x00000008) != 0) return "纸卡住";
                if ((Status & 0x00000010) != 0) return "缺纸";
                if ((Status & 0x00000020) != 0) return "手动进纸";
                if ((Status & 0x00000040) != 0) return "离线";
                if ((Status & 0x00000100) != 0) return "忙碌";
                if ((Status & 0x00000200) != 0) return "打印中";
                return "未知(" + Status + ")";
            }
        }

        public string TypeText
        {
            get
            {
                if (IsNetwork) return "网络";
                if (IsLocal && IsShared) return "本地(共享)";
                if (IsLocal) return "本地";
                return "未知";
            }
        }

        public override string ToString() => Name;
    }
}
