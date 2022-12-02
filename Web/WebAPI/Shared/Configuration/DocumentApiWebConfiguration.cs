using System;
using System.Text.Json;
using Extract;
using UCLID_FILEPROCESSINGLib;

namespace WebAPI.Configuration
{
    public class DocumentApiWebConfiguration : IDocumentApiWebConfiguration
    {
        public string DocumentFolder { get; set; }
        public string StartWorkflowAction { get; set; }
        public string EndWorkflowAction { get; set; }
        public string FileNameMetadataInitialValueFunction { get; set; }
        public string PostWorkflowAction { get; set; }
        public string OutputFileNameMetadataField { get; set; }
        public string ConfigurationName { get; set; }
        public string WorkflowName { get; set; }
        public string AttributeSet { get; set; }
        public string ProcessingAction { get; set; }
        public string PostProcessingAction { get; set; }

        public IWebConfiguration Copy()
        {
            return new DocumentApiWebConfiguration()
            {
                DocumentFolder = DocumentFolder,
                StartWorkflowAction = StartWorkflowAction,
                EndWorkflowAction = EndWorkflowAction,
                FileNameMetadataInitialValueFunction = FileNameMetadataInitialValueFunction,
                PostWorkflowAction = PostWorkflowAction,
                OutputFileNameMetadataField = OutputFileNameMetadataField,
                ConfigurationName = ConfigurationName,
                WorkflowName = WorkflowName,
                AttributeSet = AttributeSet,
                ProcessingAction = ProcessingAction,
                PostProcessingAction = PostProcessingAction,
            };
        }

        public void LoadConfigurationData(FileProcessingDB fileProcessingDB, string configurationName)
        {
            try
            {
                var configuration = ConfigurationUtilities.GetWebConfigurationForConfigurationName(fileProcessingDB, configurationName);

                DocumentApiWebConfiguration? apiWebConfiguration =
                    JsonSerializer.Deserialize<DocumentApiWebConfiguration>(configuration);
                if (apiWebConfiguration != null
                    && apiWebConfiguration.ConfigurationName != null
                    && apiWebConfiguration.ConfigurationName.Equals(configurationName))
                {
                    DocumentFolder = apiWebConfiguration.DocumentFolder;
                    StartWorkflowAction = apiWebConfiguration.StartWorkflowAction;
                    EndWorkflowAction = apiWebConfiguration.EndWorkflowAction;
                    FileNameMetadataInitialValueFunction = apiWebConfiguration.FileNameMetadataInitialValueFunction;
                    PostWorkflowAction = apiWebConfiguration.PostWorkflowAction;
                    OutputFileNameMetadataField = apiWebConfiguration.OutputFileNameMetadataField;
                    ConfigurationName = apiWebConfiguration.ConfigurationName;
                    WorkflowName = apiWebConfiguration.WorkflowName;
                    AttributeSet = apiWebConfiguration.AttributeSet;
                    ProcessingAction = apiWebConfiguration.ProcessingAction;
                    PostProcessingAction = apiWebConfiguration.PostProcessingAction;
                }
                else
                {
                    throw new ExtractException("ELI53706", $"Could not deserialize {configurationName} into a redaction web configuration.");
                }
            }
            catch (Exception e)
            {
                var ee = e.AsExtract("ELI53690");
                ee.AddDebugData("Info", "Error parsing web configuration from database.");
                throw ee;
            }
        }
    }
}
