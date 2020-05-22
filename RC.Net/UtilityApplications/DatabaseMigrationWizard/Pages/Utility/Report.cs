using System;
using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Pages.Utility
{
    /// <summary>
    /// Is a representation of the ReportingDatabaseMigrationWizard table
    /// </summary>
    public class Report
    {
        /// <summary>
        /// Classification is an Error, warning or info
        /// </summary>
        public string Classification { get; set; }

        /// <summary>
        /// Information about the info/warning/error
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The table that the event occured in
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// The time the event occured.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// The command that executed. Likely Insert/Update/ "N/A"
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// The old value being updated
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public string Old_Value { get; set; }

        /// <summary>
        /// The new value being updated.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        public string New_Value { get; set; }
    }
}
