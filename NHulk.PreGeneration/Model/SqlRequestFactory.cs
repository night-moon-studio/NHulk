using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace NHulk.PreGeneration.Model
{
    public class SqlRequestFactory
    {
        public static readonly ConcurrentDictionary<string, SqlRequestModel> SqlModelCache;

        static SqlRequestFactory()
        {
            SqlModelCache = new ConcurrentDictionary<string, SqlRequestModel>();
        }


    }
}
