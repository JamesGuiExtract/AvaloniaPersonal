using System;
using System.Collections.Generic;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class DataEntryCounterDefinition
    {
        public string Name { get; set; }

        public string AttributeQuery { get; set; }

        public bool RecordOnLoad { get; set; }

        public bool RecordOnSave { get; set; }

        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is DataEntryCounterDefinition definition &&
                   Name == definition.Name &&
                   AttributeQuery == definition.AttributeQuery &&
                   RecordOnLoad == definition.RecordOnLoad &&
                   RecordOnSave == definition.RecordOnSave &&
                   Guid.Equals(definition.Guid);
        }

        public override int GetHashCode()
        {
            var hashCode = 98731872;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AttributeQuery);
            hashCode = hashCode * -1521134295 + RecordOnLoad.GetHashCode();
            hashCode = hashCode * -1521134295 + RecordOnSave.GetHashCode();
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($@"(
                        '{Name}'
                        , '{AttributeQuery}'
                        , {(RecordOnLoad == true ? "1" : "0")}
                        , {(RecordOnSave == true ? "1" : "0")}
                        , '{Guid}'
                    )");
        }
    }
}
