using System;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class Workflow
    {
        public string Name { get; set; }

        public string WorkflowTypeCode { get; set; }

        public string Description { get; set; }

        public int? StartActionID { get; set; }

        public int? EndActionID { get; set; }

        public int? PostWorkflowActionID { get; set; }

        public string DocumentFolder { get; set; }

        public Int64? OutputAttributeSetID { get; set; }

        public int? OutputFileMetadataFieldID { get; set; }

        public string OutputFilePathInitializationFunction { get; set; }

        public int? LoadBalanceWeight { get; set; }

        public int? EditActionID { get; set; }

        public int? PostEditActionID { get; set; }

        public string StartAction { get; set; }

        public string EditAction { get; set; }

        public string EndAction { get; set; }

        public string PostEditAction { get; set; }

        public string PostWorkflowAction { get; set; }

        public string AttributeSetName { get; set; }

        public string MetadataFieldName { get; set; }

        public override string ToString()
        {
            return $@"(
                    '{(Name ?? "NULL")}'
                    , {(WorkflowTypeCode == null ? "NULL" : "'" + WorkflowTypeCode.Replace("'", "''") + "'")}
                    , {(Description == null ? "NULL" : "'" + Description.Replace("'", "''") + "'")}
                    , {(StartActionID == null ? "NULL" : StartActionID.ToString() )}
                    , {(EndActionID == null ? "NULL" : EndActionID.ToString())}
                    , {(PostWorkflowActionID == null ? "NULL" : PostWorkflowActionID.ToString())}
                    , {(DocumentFolder == null ? "NULL" : "'" + DocumentFolder.Replace("'", "''") + "'")}
                    , {(OutputAttributeSetID == null ? "NULL" : OutputAttributeSetID.ToString())}
                    , {(OutputFileMetadataFieldID == null ? "NULL" : OutputFileMetadataFieldID.ToString())}
                    , {(OutputFilePathInitializationFunction == null ? "NULL" : "'" + OutputFilePathInitializationFunction.Replace("'", "''") + "'")}
                    , {(LoadBalanceWeight == null ? "NULL" : LoadBalanceWeight.ToString())}
                    , {(EditActionID == null ? "NULL" : EditActionID.ToString())}
                    , {(PostEditActionID == null ? "NULL" : PostEditActionID.ToString())}
                    , {(StartAction == null ? "NULL" : "'" + StartAction.Replace("'", "''") + "'")}
                    , {(EditAction == null ? "NULL" : "'" + EditAction.Replace("'", "''") + "'")}
                    , {(EndAction == null ? "NULL" : "'" + EndAction.Replace("'", "''") + "'")}
                    , {(PostEditAction == null ? "NULL" : "'" + PostEditAction.Replace("'", "''") + "'")}
                    , {(PostWorkflowAction == null ? "NULL" : "'" + PostWorkflowAction.Replace("'", "''") + "'")}
                    , {(AttributeSetName == null ? "NULL" : "'" + AttributeSetName.Replace("'", "''") + "'")}
                    , {(MetadataFieldName == null ? "NULL" : "'" + MetadataFieldName.Replace("'", "''") + "'")}
                    )";
        }
    }
}
