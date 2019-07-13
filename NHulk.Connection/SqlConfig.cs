using Newtonsoft.Json;
using NHulk.Connection.Model;
using NHulk.Connection.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace NHulk.Connection
{
    public class SqlConfig
    {
        public static readonly ConcurrentDictionary<string, SqlConnectionModel> ConfigMapping;
        public static readonly ConcurrentDictionary<string, string> ConnectionStringMapping;
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
                        Filter = "ConnectionConfig.json",
                        NotifyFilter = NotifyFilters.Size
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
        /// 数据库配置文件监控回调事件用于处理数据库配置文件更新后即时重新初始化配置文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            _watcher.EnableRaisingEvents = false;
            //数据库配置文件文件发生更改触发Changed事件重新初始化读取配置文件
            Init();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine(GetConnectionString("XXSystem"));
            Console.ResetColor();
            _watcher.EnableRaisingEvents = true;
            
        }


        /// <summary>
        /// **函数逻辑***
        /// 1.初始化配置文件
        /// 2.注册文件监控委托
        /// </summary>
        static SqlConfig()
        {
            ConfigMapping = new ConcurrentDictionary<string, SqlConnectionModel>();
            ConnectionStringMapping = new ConcurrentDictionary<string, string>();
            IsWatchFile = true;
        }


        private static void Init()
        {
            try
            {
                FileInfo info = new FileInfo(FileManagement.SqlConfigFile);
                info.CopyTo(FileManagement.SqlConfigTempFile, true);

                var body = File.ReadAllText(FileManagement.SqlConfigTempFile);
                var result = JsonConvert.DeserializeObject<List<SqlConnectionModel>>(body);


                foreach (var item in result)
                {
                    ConfigMapping[item.Name] = item;
                }
            }
            catch (Exception)
            {
                Thread.Sleep(FileManagement.Delay);
                Init();
            }
           
        }


        /// <summary>
        /// 添加数据库配置信息
        /// </summary>
        /// <param name="key">配置信息key键</param>
        /// <param name="action"></param>
        public void Add(string key, Action<SqlConnectionModel> action)
        {
            var model = new SqlConnectionModel();
            action(model);
            ConfigMapping[key] = model;
        }


        /// <summary>
        /// 根据K获取配置信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static SqlConnectionModel GetModel(string key)
        {
            if (ConfigMapping.ContainsKey(key))
            {
                return ConfigMapping[key];
            }
            return default;
        }
        /// <summary>
        /// 根据K获取数据库配置文件完整字符串
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConnectionString(string key)
        {
            if (ConfigMapping.ContainsKey(key))
            {
               return SqlModelToString.GetConnectionString(ConfigMapping[key]).Replace("\r\n","");
            }
            return default;
        }
    }
}
