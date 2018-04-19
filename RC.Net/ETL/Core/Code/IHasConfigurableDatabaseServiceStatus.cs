using System;

namespace Extract.ETL
{
    public interface IHasConfigurableDatabaseServiceStatus
    {
        /// <summary>
        /// Refreshes the <see cref="DatabaseServiceStatus"/>
        /// by loading from the database, creating a new instance, etc
        /// </summary>
        void RefreshStatus();

        DatabaseServiceStatus Status { get; }
    }
}
