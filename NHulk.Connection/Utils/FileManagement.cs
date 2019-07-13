using System;
using System.IO;

namespace NHulk.Connection.Utils
{
    public static class FileManagement
    {
        public static readonly string SqlConfigFile;
        public static readonly string SqlConfigFolder;
        public static readonly string SqlConfigTempFile;
        public static readonly string SqlConfigTempFolder;
        public static int Delay = 0;

        static FileManagement()
        {
            SqlConfigFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");
            SqlConfigFile = Path.Combine(SqlConfigFolder, "ConnectionConfig.json");
            SqlConfigTempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
            SqlConfigTempFile = Path.Combine(SqlConfigTempFolder, "ConnectionConfig.json");
        }
    }
}
