namespace PrinterManager.Models
{
    /// <summary>
    /// 打印机驱动信息模型
    /// </summary>
    public class DriverInfo
    {
        public string Name { get; set; }
        public string Environment { get; set; }
        public string DriverPath { get; set; }
        public string DataFile { get; set; }
        public string ConfigFile { get; set; }
        public uint Version { get; set; }

        public string VersionText
        {
            get
            {
                switch (Version)
                {
                    case 3: return "Version 3 (XP/Vista/7/8/10/11)";
                    case 4: return "Version 4 (Win8+)";
                    default: return $"Version {Version}";
                }
            }
        }

        public override string ToString() => Name;
    }
}
