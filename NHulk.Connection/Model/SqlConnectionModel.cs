
namespace NHulk.Connection.Model
{
    /// <summary>
    /// 序列化信息 -- 数据库链接
    /// </summary>
    public class SqlConnectionModel: SqlConfigModel
    {
        public string Address;
        public string Port;
        public string Username;
        public string Password;
        public string CharacterSet;
        public string DbName;
    }
}
