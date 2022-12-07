using System;
using System.Collections.Generic;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class Workflow
    {
        public Guid WorkflowGuid { get; set; }

        public string Name { get; set; }

        public string WorkflowTypeCode { get; set; }

        public string Description { get; set; }

        public int? LoadBalanceWeight { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Workflow workflow &&
                   WorkflowGuid.Equals(workflow.WorkflowGuid) &&
                   Name == workflow.Name &&
                   WorkflowTypeCode == workflow.WorkflowTypeCode &&
                   Description == workflow.Description &&
                   LoadBalanceWeight == workflow.LoadBalanceWeight;
        }

        public override int GetHashCode()
        {
            var hashCode = 2056396789;
            hashCode = hashCode * -1521134295 + WorkflowGuid.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(WorkflowTypeCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
            hashCode = hashCode * -1521134295 + LoadBalanceWeight.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($@"(
                    '{WorkflowGuid}'
                    , '{(Name ?? "NULL")}'
                    , {(WorkflowTypeCode == null ? "NULL" : "'" + WorkflowTypeCode.Replace("'", "''") + "'")}
                    , {(Description == null ? "NULL" : "'" + Description.Replace("'", "''") + "'")}
                    , {(LoadBalanceWeight == null ? "NULL" : LoadBalanceWeight.ToString())}
                    )");
        }
    }
}
