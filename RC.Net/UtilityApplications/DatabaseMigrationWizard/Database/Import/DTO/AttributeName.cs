using System;
using System.Collections.Generic;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class AttributeName
    {
        public string Name { get; set; }

        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is AttributeName name &&
                   Name == name.Name &&
                   Guid.Equals(name.Guid);
        }

        public override int GetHashCode()
        {
            var hashCode = -857131742;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($@"('{Name}', '{Guid}')");
        }
    }
}
