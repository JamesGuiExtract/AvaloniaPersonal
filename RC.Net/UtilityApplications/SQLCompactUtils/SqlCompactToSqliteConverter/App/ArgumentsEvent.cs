using System.Collections.Generic;

namespace Extract.Utilities.SqlCompactToSqliteConverter
{
    /// Event data representing the commandline arguments passed to the application
    public class ArgumentsEvent
    {
        /// The commandline arguments passed to the application
        public IList<string> Args { get; }

        /// <summary>
        /// Create an instance of the event data
        /// </summary>
        /// <param name="args">The arguments passed to the application</param>
        public ArgumentsEvent(IList<string> args) { Args = args; }
    }
}
