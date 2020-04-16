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

        public string DocumentFolder { get; set; }

        public string OutputFilePathInitializationFunction { get; set; }

        public int? LoadBalanceWeight { get; set; }

        public Guid? EditActionGuid { get; set; }

        public Guid? EndActionGuid { get; set; }

        public Guid? PostEditActionGuid { get; set; }

        public Guid? PostWorkflowActionGuid { get; set; }

        public Guid? StartActionGuid { get; set; }

        public Guid? AttributeSetNameGuid { get; set; }

        public Guid? MetadataFieldGuid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Workflow workflow &&
                   WorkflowGuid.Equals(workflow.WorkflowGuid) &&
                   Name == workflow.Name &&
                   WorkflowTypeCode == workflow.WorkflowTypeCode &&
                   Description == workflow.Description &&
                   DocumentFolder == workflow.DocumentFolder &&
                   OutputFilePathInitializationFunction == workflow.OutputFilePathInitializationFunction &&
                   LoadBalanceWeight == workflow.LoadBalanceWeight &&
                   EqualityComparer<Guid?>.Default.Equals(EditActionGuid, workflow.EditActionGuid) &&
                   EqualityComparer<Guid?>.Default.Equals(EndActionGuid, workflow.EndActionGuid) &&
                   EqualityComparer<Guid?>.Default.Equals(PostEditActionGuid, workflow.PostEditActionGuid) &&
                   EqualityComparer<Guid?>.Default.Equals(PostWorkflowActionGuid, workflow.PostWorkflowActionGuid) &&
                   EqualityComparer<Guid?>.Default.Equals(StartActionGuid, workflow.StartActionGuid) &&
                   EqualityComparer<Guid?>.Default.Equals(AttributeSetNameGuid, workflow.AttributeSetNameGuid) &&
                   EqualityComparer<Guid?>.Default.Equals(MetadataFieldGuid, workflow.MetadataFieldGuid);
        }

        public override int GetHashCode()
        {
            var hashCode = 2056396789;
            hashCode = hashCode * -1521134295 + WorkflowGuid.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(WorkflowTypeCode);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DocumentFolder);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(OutputFilePathInitializationFunction);
            hashCode = hashCode * -1521134295 + LoadBalanceWeight.GetHashCode();
            hashCode = hashCode * -1521134295 + EditActionGuid.GetHashCode();
            hashCode = hashCode * -1521134295 + EndActionGuid.GetHashCode();
            hashCode = hashCode * -1521134295 + PostEditActionGuid.GetHashCode();
            hashCode = hashCode * -1521134295 + PostWorkflowActionGuid.GetHashCode();
            hashCode = hashCode * -1521134295 + StartActionGuid.GetHashCode();
            hashCode = hashCode * -1521134295 + AttributeSetNameGuid.GetHashCode();
            hashCode = hashCode * -1521134295 + MetadataFieldGuid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($@"(
                    '{WorkflowGuid}'
                    , '{(Name ?? "NULL")}'
                    , {(WorkflowTypeCode == null ? "NULL" : "'" + WorkflowTypeCode.Replace("'", "''") + "'")}
                    , {(Description == null ? "NULL" : "'" + Description.Replace("'", "''") + "'")}
                    , {(DocumentFolder == null ? "NULL" : "'" + DocumentFolder.Replace("'", "''") + "'")}
                    , {(OutputFilePathInitializationFunction == null ? "NULL" : "'" + OutputFilePathInitializationFunction.Replace("'", "''") + "'")}
                    , {(LoadBalanceWeight == null ? "NULL" : LoadBalanceWeight.ToString())}
                    , {(EditActionGuid == null ? "NULL" : "'" + EditActionGuid + "'")}
                    , {(EndActionGuid == null ? "NULL" : "'" + EndActionGuid + "'")}
                    , {(PostEditActionGuid == null ? "NULL" : "'" + PostEditActionGuid + "'")}
                    , {(PostWorkflowActionGuid == null ? "NULL" : "'" + PostWorkflowActionGuid + "'")}
                    , {(StartActionGuid == null ? "NULL" : "'" + StartActionGuid + "'")}
                    , {(AttributeSetNameGuid == null ? "NULL" : "'" + AttributeSetNameGuid + "'")}
                    , {(MetadataFieldGuid == null ? "NULL" : "'" + MetadataFieldGuid + "'")}
                    )");
        }
    }
}
