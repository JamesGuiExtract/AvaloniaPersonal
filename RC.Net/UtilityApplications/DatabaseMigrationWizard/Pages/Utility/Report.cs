using System;

namespace DatabaseMigrationWizard.Pages.Utility
{
    public class Report
    {
        public string Classification { get; set; }

        public string Message { get; set; }

        public string TableName { get; set; }

        public DateTime DateTime { get; set; }
    }
}
