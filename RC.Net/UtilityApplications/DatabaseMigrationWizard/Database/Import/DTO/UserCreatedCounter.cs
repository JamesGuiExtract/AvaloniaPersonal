using System;
using System.Collections.Generic;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class UserCreatedCounter
    {
        public string CounterName { get; set; }

        public string Value { get; set; }

        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is UserCreatedCounter counter &&
                   CounterName == counter.CounterName &&
                   Value == counter.Value &&
                   Guid.Equals(counter.Guid);
        }

        public override int GetHashCode()
        {
            var hashCode = -1779228722;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CounterName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($"('{CounterName}', '{Value}', '{Guid}')");
        }
    }
}
