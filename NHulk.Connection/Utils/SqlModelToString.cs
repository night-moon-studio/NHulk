using Natasha.Method;
using NHulk.Connection.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NHulk.Connection
{
    public class SqlModelToString
    {
        private readonly static ConcurrentDictionary<SqlEnum, Func<SqlConnectionModel, string>> ActionMapping;
        private readonly static ConcurrentDictionary<string, SqlEnum> PathEnumMapping;
        private readonly static HashSet<string> WatchPath;
        private static FileSystemWatcher _watcher;
        public static bool IsWatchFile
        {
            set
            {
                if (value)
                {
                    _watcher = new FileSystemWatcher(SqlConfig.ConfigDirectory);
                    _watcher.EnableRaisingEvents = true;
                    _watcher.Changed += Watcher_Changed;
                }
                else
                {

                    _watcher.Changed -= Watcher_Changed;
                    _watcher.Dispose();
                }

            }
        }
        static SqlModelToString()
        {

            WatchPath = new HashSet<string>();
            PathEnumMapping = new ConcurrentDictionary<string, SqlEnum>();
            ActionMapping = new ConcurrentDictionary<SqlEnum, Func<SqlConnectionModel, string>>();

            var items = Enum.GetNames(typeof(SqlEnum));
            for (int i = 0; i < items.Length; i += 1)
            {
                string file = Path.Combine(SqlConfig.ConfigDirectory, items[i] + "Convert");
                ActionMapping[(SqlEnum)i] = GetFunc(file);
                PathEnumMapping[file] = (SqlEnum)i;
                WatchPath.Add(file);
            }
            IsWatchFile = true;
        }



        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name != "ConnectionConfig.json")
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Changed:
                        ActionMapping[PathEnumMapping[e.FullPath]] = GetFunc(e.FullPath);
                        break;
                    case WatcherChangeTypes.Deleted:
                        throw new Exception("文件为项目文件，不能删除！");
                    case WatcherChangeTypes.Renamed:
                        throw new Exception("文件为项目文件，不能重命名！");
                    default:
                        break;
                }
            }
        }

        private static Func<SqlConnectionModel, string> GetFunc(string file)
        {
            string body;
            try
            {
                body = File.ReadAllText(file).Replace("\r\n","");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine($"\r\nFile had changed: {body}");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                throw new Exception("请检查文件状态，错误：" + e.Message);
            }
            return body.Create<Func<SqlConnectionModel, string>>(typeof(StringBuilder));
        }

        public static string GetConnectionString(SqlConnectionModel model)
        {
            return ActionMapping[model.Type](model);
        }
    }
}
