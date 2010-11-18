using System;
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
        /// Gets a value indicating whether a database schema update is required.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if an update is required; otherwise, <see langword="false"/>.
        /// </value>
        bool IsRequired { get; }

        /// <summary>
        /// Begins the update to latest schema.
        /// </summary>
        /// <param name="progressStatus">The progress status object to update, if
        /// <see langword="null"/> then no progress status will be given. Otherwise it
        /// will be reinitialized to the appropriate number of steps and updated by the
        /// update task as it runs.</param>
        /// <returns>
        /// A handle to the task that is updating the schema.
        /// </returns>
        Task BeginUpdateToLatestSchema(IProgressStatus progressStatus);
    }
}
