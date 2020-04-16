using System;
using System.Collections.Generic;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class AttributeSetName
    {
        public string Description { get; set; }

        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is AttributeSetName name &&
                   Description == name.Description &&
                   Guid.Equals(name.Guid);
        }

        public override int GetHashCode()
        {
            var hashCode = 1458262941;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($"('{Description}', '{Guid}')");
        }
    }
}
