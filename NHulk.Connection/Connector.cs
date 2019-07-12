using Natasha;
using Natasha.Method;
using System;
using System.Collections.Concurrent;
using System.Data;

namespace NHulk.Connection
{
    public delegate IDbConnection DbCreator();

    public class Connector
    {

        internal static readonly ConcurrentDictionary<string, (string Read, string Write)> _info_cache;
        internal static readonly ConcurrentDictionary<string, (DbCreator Read, DbCreator Write)> _func_cache;

        private static readonly Connector _link;
        static Connector()
        {
            _info_cache = new ConcurrentDictionary<string, (string Read, string Write)>();
            _func_cache = new ConcurrentDictionary<string, (DbCreator Read, DbCreator Write)>();
            _link = new Connector();
        }

        /// <summary>
        /// 获取对应key的读取数据库的IDbConnection初始化委托
        /// </summary>
        /// <param name="key">字典的key</param>
        /// <returns>初始化委托</returns>
        public static DbCreator ReadInitor(string key)
        {
            if (_func_cache.ContainsKey(key))
            {
                return _func_cache[key].Read;
            }
            else
            {
                throw new NullReferenceException($"{ key }并没有添加到字典中，请检查初始化的代码！");
            }
        }


        /// <summary>
        /// 获取对应key的写入数据库的IDbConnection初始化委托
        /// </summary>
        /// <param name="key">字典的key</param>
        /// <returns>初始化委托</returns>
        public static DbCreator WriteInitor(string key)
        {
            if (_func_cache.ContainsKey(key))
            {
                return _func_cache[key].Write;
            }
            else
            {
                throw new NullReferenceException($"{ key }并没有添加到字典中，请检查初始化的代码！");
            }
        }


        /// <summary>
        /// 获取对应key的读取和写入数据库的IDbConnection初始化委托
        /// </summary>
        /// <param name="key">字典的key</param>
        /// <returns>初始化读写委托元组</returns>
        public static (DbCreator Read, DbCreator Write) Initor(string key)
        {
            if (_func_cache.ContainsKey(key))
            {
                return _func_cache[key];
            }
            else
            {
                throw new NullReferenceException($"{ key }并没有添加到字典中，请检查初始化的代码！");
            }
        }


        /// <summary>
        /// 添加一个读连接
        /// </summary>
        /// <typeparam name="T">数据库连接类型</typeparam>
        /// <param name="key">字典的Key</param>
        /// <param name="read">读连接字符串</param>
        /// <returns>本身</returns>
        public static Connector AddRead<T>(string key, string read)
        {
            //Connector<T>.FuncCache[key] = _func_cache[key];
            return AddRead(key, read, typeof(T));
        }
        public static Connector AddRead(string key, string read, Type type)
        {
            if (!_info_cache.ContainsKey(key))
            {
                _info_cache[key] = (Read: read, Write: null);
                _func_cache[key] = (Read: DynamicCreateor(type, read), Write: null);
            }
            else
            {
                _info_cache[key] = (Read: read, _info_cache[key].Write);
                _func_cache[key] = (Read: DynamicCreateor(type, read), _func_cache[key].Write);
            }

            return _link;
        }

        /// <summary>
        /// 添加一个写连接
        /// </summary>
        /// <typeparam name="T">数据库连接类型</typeparam>
        /// <param name="key">字典的Key</param>
        /// <param name="write">写连接字符串</param>
        /// <returns>本身</returns>
        public static Connector AddWrite<T>(string key, string write)
        {
            //Connector<T>.FuncCache[key] = _func_cache[key];
            return AddWrite(key, write, typeof(T));
        }
        public static Connector AddWrite(string key, string write, Type type)
        {
            if (!_info_cache.ContainsKey(key))
            {
                _info_cache[key] = (Read: null, Write: write);
                _func_cache[key] = (Read: null, Write: DynamicCreateor(type, write));
            }
            else
            {
                _info_cache[key] = (_info_cache[key].Read, Write: write);
                _func_cache[key] = (Read: _func_cache[key].Write, Write: DynamicCreateor(type, write));
            }

            return _link;
        }

        /// <summary>
        /// 不区分读写添加初始化方案
        /// </summary>
        /// <typeparam name="T">数据库连接类型</typeparam>
        /// <param name="key">字典的Key</param>
        /// <param name="value">连接字符串</param>
        /// <returns></returns>
        public static Connector Add<T>(string key, string value)
        {
            //Connector<T>.FuncCache[key] = _func_cache[key];
            return Add<T>(key, value, value);
        }

        /// <summary>
        /// 添加初始化方案
        /// </summary>
        /// <param name="key">字典的key</param>
        /// <param name="read">读-数据库链接字符串</param>
        /// <param name="write">写-数据库链接字符串</param>
        public static Connector Add<R, W>(string key, string read, string write)
        {
            _info_cache[key] = (Read: read, Write: write);
            _func_cache[key] = (Read: DynamicCreateor<R>(read), Write: DynamicCreateor<W>(write));
            return _link;
        }

        /// <summary>
        /// 添加初始化方案
        /// </summary>
        /// <param name="key">字典的key</param>
        /// <param name="read">读-数据库链接字符串</param>
        /// <param name="write">写-数据库链接字符串</param>
        public static Connector Add<T>(string key, string read, string write)
        {
            _info_cache[key] = (Read: read, Write: write);
            _func_cache[key] = (Read: DynamicCreateor<T>(read), Write: DynamicCreateor<T>(write));
            return _link;
        }


        internal static DbCreator DynamicCreateor(Type type, string connection)
        {
            return $@"return new {type.GetDevelopName()}({connection});".Create<DbCreator>(type);
        }

        internal static DbCreator DynamicCreateor<T>(string connection)
        {
            return DynamicCreateor(typeof(T), connection);
        }
    }
}
