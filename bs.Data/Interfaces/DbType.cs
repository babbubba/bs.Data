using System.ComponentModel;

namespace bs.Data.Interfaces
{
    public enum DbType
    {
        [Description("MySql Server")]
        MySQL = 10,

        [Description("SQlite")]
        SQLite = 20,

        [Description("MS SQL Serve 2012 or higher")]
        MsSql2012 = 30,

        [Description("MS SQL Serve 2008")]
        MsSql2008 = 40,

        [Description("PostgreSQL")]
        PostgreSQL = 50
    }
}