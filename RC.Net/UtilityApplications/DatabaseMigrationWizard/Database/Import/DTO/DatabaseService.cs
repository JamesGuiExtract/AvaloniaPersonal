using System;
using System.Collections.Generic;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class DatabaseService
    {
        public string Description { get; set; }

        public string Settings { get; set; }

        public bool Enabled { get; set; }

        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is DatabaseService service &&
                   Description == service.Description &&
                   Settings == service.Settings &&
                   Enabled == service.Enabled &&
                   Guid.Equals(service.Guid);
        }

        public override int GetHashCode()
        {
            var hashCode = 1489437891;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Settings);
            hashCode = hashCode * -1521134295 + Enabled.GetHashCode();
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($"('{Description}', '{Settings.Replace("'", "''")}', {(Enabled == true ? "1" : "0")}, '{Guid}')");
        }
    }
}
