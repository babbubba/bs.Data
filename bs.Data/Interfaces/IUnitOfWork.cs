using NHibernate;
using System;
using System.Collections.Generic;
using System.Text;

namespace bs.Data.Interfaces
{
    public interface IUnitOfWork
    {
        ISession Session { get; set; }

        void BeginTransaction();
        void Commit();
        void Rollback();
        void Dispose();
    }
}
