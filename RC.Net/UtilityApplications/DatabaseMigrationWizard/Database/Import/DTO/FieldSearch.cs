using System;
using System.Collections.Generic;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class FieldSearch
    {
        public bool Enabled { get; set; }

        public string FieldName { get; set; }
        
        public string AttributeQuery { get; set; }

        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is FieldSearch search &&
                   Enabled == search.Enabled &&
                   FieldName == search.FieldName &&
                   AttributeQuery == search.AttributeQuery &&
                   Guid.Equals(search.Guid);
        }

        public override int GetHashCode()
        {
            var hashCode = 1874857819;
            hashCode = hashCode * -1521134295 + Enabled.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FieldName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AttributeQuery);
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($@"(
                '{(Enabled == true ? "1" : "0")}'
                , '{FieldName}'
                , '{AttributeQuery}'
                , '{Guid}'
                )");
        }
    }
}
