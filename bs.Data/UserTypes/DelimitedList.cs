using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bs.Data.UserTypes
{
    public class DelimitedList : IUserType
    {
        private const string delimiter = "|@@|";
        public SqlType[] SqlTypes => new SqlType[] { new StringSqlType() };

        public Type ReturnedType => typeof(IList<string>);

        public bool IsMutable => false;

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public new bool Equals(object x, object y)
        {
            return object.Equals(x, y);
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            var r = rs[names[0]];
            object result = null;

            if (r == DBNull.Value || string.IsNullOrWhiteSpace((string)r))
            {
                result = new List<string>();
            }
            else
            {
                result = ((string)r).Split(delimiter, StringSplitOptions.None);
            }

            return result;
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            object paramVal = DBNull.Value;
            if (value != null && value is string[])
            {
                paramVal = string.Join(delimiter, (IEnumerable<string>)value);
            }
            var parameter = (IDataParameter)cmd.Parameters[index];
            parameter.Value = paramVal;
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }
    }
}