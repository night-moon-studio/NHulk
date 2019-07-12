namespace NHulk.PreGeneration.Model
{
    public class SqlRequestModel
    {
        public readonly string TableGetterSql;
        public readonly string FieldGetterSql;

        public SqlRequestModel(string table_sql,string field_sql)
        {
            TableGetterSql = table_sql;
            FieldGetterSql = field_sql;
        }

    }
}
