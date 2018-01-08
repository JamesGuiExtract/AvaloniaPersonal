using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.Interfaces
{
    /// <summary>
    /// Describes a scheduled event that may recur.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IScheduledEvent : IDisposable
    {
        /// <summary>
        /// Raised when any instance of the event starts.
        /// </summary>
        event EventHandler<EventArgs> EventStarted;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IDatabaseService"/> is enabled.
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
        /// Gets a value indicating whether the current time is within a time frame where the event
        /// is excluded from occurring.
        /// </summary>
        bool InExcludedTime
        {
            get;
        }
    }
}
