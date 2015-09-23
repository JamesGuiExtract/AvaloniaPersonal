using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Extract.Interfaces
{
    /// <summary>
    /// Used for tracking counts decremented by rule execution.
    /// </summary>
    [ComVisible(true)]
    [Guid("CBDB5B5D-2097-4644-9516-D952CECD6066")]
    public interface IRuleExecutionCounter
    {
        /// <summary>
        /// Gets the ID of the counter.
        /// </summary>
        int CounterID
        {
            get;
        }

        /// <summary>
        /// Gets the name of the counter.
        /// </summary>
        string CounterName
        {
            get;
        }

        /// <summary>
        /// Decrements the counter by the specified <see paramref="count"/> assuming enough counts
        /// are available.
        /// </summary>
        /// <param name="count">The number of counts to decrement.</param>
        /// <returns>The new number of counts left or -1 if there were not enough counts to be able
        /// to decrement.</returns>
        int DecrementCounter(int count);
    }
}
