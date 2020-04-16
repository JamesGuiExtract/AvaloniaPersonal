using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class Action
    {
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Naming violations are a result of acronyms in the database.")]
        public string ASCName { get; set; }

        public string Description { get; set; }

        public bool? MainSequence { get; set; }

        public Guid ActionGuid { get; set; }

        public Guid? WorkflowGuid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Action action &&
                   ASCName == action.ASCName &&
                   Description == action.Description &&
                   MainSequence == action.MainSequence &&
                   ActionGuid.Equals(action.ActionGuid) &&
                   EqualityComparer<Guid?>.Default.Equals(WorkflowGuid, action.WorkflowGuid);
        }

        public override int GetHashCode()
        {
            var hashCode = 197701942;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ASCName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
            hashCode = hashCode * -1521134295 + MainSequence.GetHashCode();
            hashCode = hashCode * -1521134295 + ActionGuid.GetHashCode();
            hashCode = hashCode * -1521134295 + WorkflowGuid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($@"(
                '{ASCName}'
                , {(Description == null ? "NULL" : "'" + Description + "'")}
                , {(MainSequence == null ? "NULL" : (MainSequence == true ? "1" : "0" ))}
                , '{ActionGuid}'
                , {(WorkflowGuid == null ? "NULL" : "'" + WorkflowGuid + "'")}
                )");
        }
    }
}
