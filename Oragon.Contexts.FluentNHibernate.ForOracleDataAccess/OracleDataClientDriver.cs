﻿using NHibernate.Driver;

namespace Oragon.Contexts.FluentNHibernate.ForOracleDataAccess
{
    /// <summary>
    /// A NHibernate Driver for using the Oracle.DataAccess (unmanaged) DataProvider
    /// </summary>
    public class OracleDataClientDriver : OracleDataClientDriverBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="OracleDataClientDriver"/>.
        /// </summary>
        /// <exception cref="HibernateException">
        /// Thrown when the <c>Oracle.DataAccess</c> assembly can not be loaded.
        /// </exception>
        public OracleDataClientDriver()
          : base("Oracle.ManagedDataAccess")
        {
        }

        public override bool HasDelayedDistributedTransactionCompletion => true;
    }
}