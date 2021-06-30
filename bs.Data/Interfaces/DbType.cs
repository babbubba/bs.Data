using System.ComponentModel;

namespace bs.Data.Interfaces
{
    public enum DbType
    {
        [Description("Undefined")]
        Undefined = 0,

        [Description("MySql Server 5.5")]
        MySQL = 10,

        [Description("MySql Server 5.7")]
        MySQL57 = 12,

        [Description("SQlite")]
        SQLite = 20,

        [Description("MS SQL Serve 2012 or higher")]
        MsSql2012 = 30,

        [Description("MS SQL Serve 2008")]
        MsSql2008 = 40,

        [Description("PostgreSQL")]
        PostgreSQL = 50,

        [Description("PostgreSQL 8.3")]
        PostgreSQL83 = 52
    }
}