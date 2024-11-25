using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace bs.Data.UserTypes
{
    public class DelimitedList : IUserType
    {
        private const string delimiter = "|@@|";
        public bool IsMutable => false;
        public Type ReturnedType => typeof(ICollection<string>);
        public SqlType[] SqlTypes => new SqlType[] { new StringSqlType() };

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
            if (x == null)
            {
                return 0;
            }

            return x.GetHashCode();
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            var r = rs[names[0]];

            object result;
            if (r == DBNull.Value || string.IsNullOrWhiteSpace((string)r))
            {
                result = Array.Empty<string>();
            }
            else
            {
                result = ((string)r).Split(delimiter, StringSplitOptions.None);
            }

            return result;
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            object paramVal;

            if (value is string[] array)
            {
                value = array.ToList();
            }
            if (value != null)
            {
                paramVal = string.Join(delimiter, (IEnumerable<string>)value);
            }
            else
            {
                paramVal = DBNull.Value;
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