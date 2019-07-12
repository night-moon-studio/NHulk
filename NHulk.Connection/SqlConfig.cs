using Newtonsoft.Json;
using NHulk.Connection.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NHulk.Connection
{
    public class SqlConfig
    {
        public static readonly ConcurrentDictionary<string, SqlConnectionModel> ConfigMapping;
        public static readonly ConcurrentDictionary<string, string> ConnectionStringMapping;
        private static FileSystemWatcher _watcher;
        public static readonly string ConfigDirectory;
        private static readonly string _config_path;
        private static StreamReader _stream;
        public static bool IsWatchFile
        {
            set
            {
                if (value)
                {

                    _watcher = new FileSystemWatcher(ConfigDirectory);
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

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name == "ConnectionConfig.json")
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Changed:
                        Init();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine();
                        Console.WriteLine(GetConnectionString("XXSystem"));
                        Console.ResetColor();
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

        static SqlConfig()
        {
            ConfigMapping = new ConcurrentDictionary<string, SqlConnectionModel>();
            ConnectionStringMapping = new ConcurrentDictionary<string, string>();
            ConfigDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");
            _config_path = Path.Combine(ConfigDirectory, "ConnectionConfig.json");
            Init();
            IsWatchFile = true;
        }


        private static void Init()
        {
            //string body;
            //using (StreamReader stream = new StreamReader(_config_path, Encoding.UTF8))
            //{
            //    body = await stream.ReadToEndAsync();
            //}
            if (_stream==null)
            {
                _stream = new StreamReader(_config_path, Encoding.UTF8);
            }
           
            //var body = File.ReadAllText(_config_path);
            var body = _stream.ReadToEndAsync().Result;
            var result = JsonConvert.DeserializeObject<List<SqlConnectionModel>>(body);
            foreach (var item in result)
            {
                ConfigMapping[item.Name] = item;
            }
        }

        public void Add(string key, Action<SqlConnectionModel> action)
        {
            var model = new SqlConnectionModel();
            action(model);
            ConfigMapping[key] = model;
        }

        public static SqlConnectionModel GetModel(string key)
        {
            if (ConfigMapping.ContainsKey(key))
            {
                return ConfigMapping[key];
            }
            return default;
        }
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
