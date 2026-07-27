using System;
using System.Collections.Generic;
using System.IO;

namespace PrinterManager.Core
{
    /// <summary>
    /// 从 INF 文件解析打印机驱动名称及版本信息
    /// </summary>
    public static class InfParser
    {
        /// <summary>
        /// 从 INF 文件解析真实的打印机驱动名称
        /// </summary>
        public static string ParseDriverNameFromInf(string infPath)
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(infPath);
            }
            catch
            {
                return Path.GetFileNameWithoutExtension(infPath);
            }

            var strings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var manufacturerSections = new List<string>();
            string currentSection = null;
            bool inStrings = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith(";"))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.TrimStart('[').TrimEnd(']').Trim();
                    inStrings = currentSection.Equals("Strings", StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                if (inStrings && line.Contains("="))
                {
                    int eq = line.IndexOf("=");
                    string k = line.Substring(0, eq).Trim().Trim('%');
                    string v = line.Substring(eq + 1).Trim().Trim('"');
                    if (!string.IsNullOrEmpty(k) && !string.IsNullOrEmpty(v))
                        strings[k] = v;
                }

                if (currentSection != null
                    && currentSection.StartsWith("Manufacturer", StringComparison.OrdinalIgnoreCase)
                    && !inStrings
                    && line.Contains("="))
                {
                    int eq = line.IndexOf("=");
                    string right = line.Substring(eq + 1).Trim().Trim('"');
                    if (!string.IsNullOrEmpty(right))
                        manufacturerSections.Add(right);
                }
            }

            foreach (var mfgSecBase in manufacturerSections)
            {
                currentSection = null;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith(";"))
                        continue;
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        currentSection = line.TrimStart('[').TrimEnd(']').Trim();
                        continue;
                    }
                    bool inSection =
                        currentSection != null
                        && (currentSection.Equals(mfgSecBase, StringComparison.OrdinalIgnoreCase)
                            || currentSection.StartsWith(mfgSecBase + ".",
                                StringComparison.OrdinalIgnoreCase));
                    if (inSection && line.Contains("="))
                    {
                        int eq = line.IndexOf("=");
                        string left = line.Substring(0, eq).Trim();
                        if (left.StartsWith("\"") && left.EndsWith("\""))
                            return left.Trim('"');
                        if (left.StartsWith("%") && left.EndsWith("%"))
                        {
                            string key = left.Trim('%');
                            string r;
                            if (strings.TryGetValue(key, out r))
                                return r;
                        }
                    }
                }
            }

            return Path.GetFileNameWithoutExtension(infPath);
        }

        /// <summary>
        /// 从 INF 文件解析驱动版本类型（V3 或 V4）
        /// </summary>
        public static string DetectDriverVersion(string infPath)
        {
            if (string.IsNullOrEmpty(infPath) || !File.Exists(infPath))
                return null;

            try
            {
                var lines = File.ReadAllLines(infPath);
                bool inVersion = false;
                bool hasPrinterPackageSection = false;
                string driverVersion = null;

                foreach (string line in lines)
                {
                    string trim = line.Trim();
                    if (trim.StartsWith("[PrinterPackageInstallation",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        hasPrinterPackageSection = true;
                    }
                    if (trim.Equals("[Version]", StringComparison.OrdinalIgnoreCase))
                    {
                        inVersion = true;
                        continue;
                    }
                    if (inVersion)
                    {
                        if (trim.StartsWith("["))
                            break;
                        if (trim.StartsWith("Signature", StringComparison.OrdinalIgnoreCase))
                        {
                            string val = trim.Substring(trim.IndexOf('=') + 1).Trim();
                            if (val.IndexOf("CHICAGO", StringComparison.OrdinalIgnoreCase) >= 0)
                                driverVersion = "3";
                        }
                    }
                }

                if (hasPrinterPackageSection)
                    return "4";
                return driverVersion ?? "3";
            }
            catch
            {
                return null;
            }
        }
    }
}
