using NHulk.DynamicCache;
using Natasha;
using System.Collections.Generic;
using System.Data;

namespace NHulk
{
    public static class DbConnectionByObjectExtension
    {

        public static IEnumerable<T> QueryByObjects<T>(this IDbConnection connection, string commandText, params object[] values)
        {
            List<T> resultCollection = null;
            SqlDelegate<T>.GetReaderInstance instance_func = null;
            SqlDelegate<T>.GetCommandByObject command_func = SqlDynamicCache.GetObjectsCommandDelegate<T>(commandText, values);
            IDbCommand command = connection.CreateCommand();
            command_func(ref command, values);
            bool CloseFlag = (connection.State == ConnectionState.Closed);
            IDataReader reader = null;
            try
            {
                if (CloseFlag) { connection.Open(); }
                reader = command.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess | CommandBehavior.SingleResult);
                CloseFlag = false;
                instance_func = SqlDynamicCache.GetReaderDelegate<T>(reader, commandText);
                resultCollection = new List<T>(reader.FieldCount);
                while (reader.Read())
                {
                    T tNode = instance_func(reader);
                    resultCollection.Add(tNode);
                }
                while (reader.NextResult()) { }
                reader.Dispose();
                reader = null;
                return resultCollection;
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                    {
                        try { command.Cancel(); }
                        catch { }
                    }
                    reader.Dispose();
                }

                if (CloseFlag) connection.Close();
                command?.Dispose();
            }
        }



        public static int ExecuteNonQueryByObject(this IDbConnection connection, string commandText, params object[] values)
        {
            SqlDelegate<ENull>.GetCommandByObject command_func = SqlDynamicCache.GetObjectsCommandDelegate<ENull>(commandText, values);
            IDbCommand command = connection.CreateCommand();
            bool CloseFlag = (connection.State == ConnectionState.Closed);
            try
            {
                if (CloseFlag) { connection.Open(); }
                command_func(ref command, values);
                return command.ExecuteNonQuery();
            }
            finally
            {
                connection.Close();
                command?.Dispose();
            }
        }


        public static object ExecuteScalarByObject<T>(this IDbConnection connection, string commandText, params object[] values)
        {
            SqlDelegate<ENull>.GetCommandByObject command_func = SqlDynamicCache.GetObjectsCommandDelegate<ENull>(commandText, values);
            IDbCommand command = connection.CreateCommand();
            bool CloseFlag = (connection.State == ConnectionState.Closed);
            try
            {
                if (CloseFlag) { connection.Open(); }
                command_func(ref command, values);
                return command.ExecuteScalar();
            }
            finally
            {
                connection.Close();
                command?.Dispose();
            }
        }

    }
}
