

using System;

namespace Extract.Interfaces
{
    /// <summary>
    /// Defines the interface for processes that will be performed by a service
    /// </summary>
    public interface IDatabaseService : IDisposable
    {
        /// <summary>
        /// Description of the database service item
        /// </summary>
        string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the database. This value is not included in the settings
        /// </summary>
        string DatabaseName
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the Server. This value is not included in the settings
        /// </summary>
        string DatabaseServer
        {
            get;
            set;
        }

        /// <summary>
        /// This is the id from the DatabaseService table.  This value is not included in the settings
        /// </summary>
        int DatabaseServiceID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        bool Enabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="IScheduledEvent"/> instance that determines when Process will be run.
        /// </summary>
        IScheduledEvent Schedule
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is processing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if processing; otherwise, <c>false</c>.
        /// </value>
        bool Processing
        {
            get;
        }

        /// <summary>
        /// Performs the processing defined the database service record
        /// </summary>
        void Process();

        /// <summary>
        /// Returns the settings in a json string
        /// </summary>
        string GetSettings();
    }
}
