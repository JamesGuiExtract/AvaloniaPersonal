using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;

namespace Extract.Database
{
    /// <summary>
    /// Interface definition for implementing a DatabaseSchemaUpdater.
    /// </summary>
    [CLSCompliant(false)]
    public interface IDatabaseSchemaUpdater
    {
        /// <summary>
        /// Sets the database connection to be used by the schema updater.
        /// </summary>
        /// <value>The database connection.</value>
        void SetDatabaseConnection(DbConnection connection);

        /// <summary>
        /// Gets a value indicating whether a database schema update is required.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if an update is required; otherwise, <see langword="false"/>.
        /// </value>
        bool IsUpdateRequired { get; }

        /// <summary>
        /// Gets a value indicating whether the current database schema is of a newer version.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the schema is a newer version.
        /// </value>
        bool IsNewerVersion { get; }

        /// <summary>
        /// Begins the update to latest schema.
        /// </summary>
        /// <param name="progressStatus">The progress status object to update, if
        /// <see langword="null"/> then no progress status will be given. Otherwise it
        /// will be reinitialized to the appropriate number of steps and updated by the
        /// update task as it runs.</param>
        /// <param name="cancelTokenSource">The cancel token that can be used to cancel
        /// the update task. Must not be <see langword="null"/>. Cancellation
        /// is up to the implementer, there is no guarantee that the update task is
        /// cancellable.</param>
        /// <returns>
        /// A handle to the task that is updating the schema. The task will have a result
        /// <see cref="string"/>. This result should contain the path to the backed up copy
        /// of the database before it was updated to the latest schema.
        /// </returns>
        Task<string> BeginUpdateToLatestSchema(IProgressStatus progressStatus,
            CancellationTokenSource cancelTokenSource);
    }
}
