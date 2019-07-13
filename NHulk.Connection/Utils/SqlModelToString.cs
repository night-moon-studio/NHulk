using Natasha.Method;
using NHulk.Connection.Model;
using NHulk.Connection.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

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
                    _watcher = new FileSystemWatcher(FileManagement.SqlConfigFolder)
                    {
                        EnableRaisingEvents = true,
                        NotifyFilter = NotifyFilters.LastWrite
                    };
                    _watcher.Changed += Watcher_Changed;
                }
                else
                {

                    _watcher.Changed -= Watcher_Changed;
                    _watcher.Dispose();
                }

            }
        }
        /// <summary>
        /// 初始化所有配置信息
        /// </summary>
        static SqlModelToString()
        {

            WatchPath = new HashSet<string>();
            PathEnumMapping = new ConcurrentDictionary<string, SqlEnum>();
            ActionMapping = new ConcurrentDictionary<SqlEnum, Func<SqlConnectionModel, string>>();
            //获取所有枚举类型
            var items = Enum.GetNames(typeof(SqlEnum));
            for (int i = 0; i < items.Length; i += 1)
            {
                //根据不同的枚举类型生成不同的文件路径
                string file = Path.Combine(FileManagement.SqlConfigFolder, items[i] + "Convert");
                ActionMapping[(SqlEnum)i] = GetFunc(File.ReadAllText(file));
                PathEnumMapping[file] = (SqlEnum)i;
                WatchPath.Add(file);
            }
            IsWatchFile = true;
        }


        /// <summary>
        /// 数据库配置文件监控回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                _watcher.EnableRaisingEvents = false;
                if (e.Name != "ConnectionConfig.json")
                {
                    //更新配置文件内容
                    FileInfo info = new FileInfo(e.FullPath);
                    var temp = Path.Combine(FileManagement.SqlConfigTempFolder, Path.GetFileName(e.FullPath));
                    info.CopyTo(temp, true);
                    var body = File.ReadAllText(temp);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine();
                    Console.WriteLine($"\r\nFile had changed: {body}");
                    Console.ResetColor();
                    ActionMapping[PathEnumMapping[e.FullPath]] = GetFunc(body);
                }
                _watcher.EnableRaisingEvents = true;
            }
            catch (Exception)
            {
                Thread.Sleep(FileManagement.Delay);
                Watcher_Changed(sender, e);
            }
        }


        /// <summary>
        /// 根据配置文件物理路径名获取配置数据
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static Func<SqlConnectionModel, string> GetFunc(string body)
        {
            try
            {
                body = body.Replace("\r\n","");
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
