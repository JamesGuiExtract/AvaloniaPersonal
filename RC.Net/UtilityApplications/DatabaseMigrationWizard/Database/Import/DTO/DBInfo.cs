using System;
using System.Collections.Generic;
using System.Globalization;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class DBInfo
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is DBInfo tocompare)
            {
                // The values for these two do not matter. We are not updating these records.
                if (tocompare.Name.ToUpper(CultureInfo.InvariantCulture).Contains("VERSION") && this.Name.ToUpper(CultureInfo.InvariantCulture).Contains("VERSION")
                    || tocompare.Name.Equals("DatabaseID") && this.Name.Equals("DatabaseID"))
                {
                    return true;
                }

                return this.Name == tocompare.Name &&
                   this.Value == tocompare.Value;
            }

            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 864523660;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($"('{Name}', '{Value}')");
        }
    }
}
