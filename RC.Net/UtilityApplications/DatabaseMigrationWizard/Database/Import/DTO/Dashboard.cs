using System;
using System.Collections.Generic;
using System.Globalization;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class Dashboard
    {
        public string DashboardName { get; set; }

        public string Definition { get; set; }

        public string LastImportedDate { get; set; }

        public bool UseExtractedData { get; set; }

        public string ExtractedDataDefinition { get; set; }

        public Guid DashboardGuid { get; set; }

        public Guid FAMUserGuid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Dashboard dashboard &&
                   DashboardName == dashboard.DashboardName &&
                   Definition == dashboard.Definition &&
                   DateTime.Parse(LastImportedDate, CultureInfo.InvariantCulture) == DateTime.Parse(dashboard.LastImportedDate, CultureInfo.InvariantCulture) &&
                   UseExtractedData == dashboard.UseExtractedData &&
                   ExtractedDataDefinition == dashboard.ExtractedDataDefinition &&
                   DashboardGuid.Equals(dashboard.DashboardGuid) &&
                   FAMUserGuid.Equals(dashboard.FAMUserGuid);
        }

        public override int GetHashCode()
        {
            var hashCode = 848307306;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DashboardName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Definition);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LastImportedDate);
            hashCode = hashCode * -1521134295 + UseExtractedData.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ExtractedDataDefinition);
            hashCode = hashCode * -1521134295 + DashboardGuid.GetHashCode();
            hashCode = hashCode * -1521134295 + FAMUserGuid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($@"('{DashboardName}'
                        , CONVERT(XML, N'{Definition.Replace("'", "''")}')
                        , CAST('{LastImportedDate}' AS DATETIME)
                        , {(UseExtractedData == true ? "1" : "0")}
                        , {(ExtractedDataDefinition == null ? "NULL" : "CONVERT(XML, N'" + ExtractedDataDefinition.Replace("'", "''") + "')")}
                        , '{DashboardGuid}'
                        , '{FAMUserGuid}'
                    )");
        }
    }
}
