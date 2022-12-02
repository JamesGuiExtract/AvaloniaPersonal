using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Extract;
using UCLID_FILEPROCESSINGLib;

namespace WebAPI.Configuration
{
    public class RedactionWebConfiguration : IRedactionWebConfiguration
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string ActiveDirectoryGroup { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IList<string> RedactionTypes { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool EnableAllUserPendingQueue { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string DocumentTypeFileLocation { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string ConfigurationName { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string WorkflowName { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string AttributeSet { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string ProcessingAction { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string PostProcessingAction { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public IWebConfiguration Copy()
        {
            return new RedactionWebConfiguration()
            {
                AttributeSet = AttributeSet,
                ProcessingAction = ProcessingAction,
                PostProcessingAction = PostProcessingAction,
                ActiveDirectoryGroup = ActiveDirectoryGroup,
                ConfigurationName = ConfigurationName,
                DocumentTypeFileLocation = DocumentTypeFileLocation,
                EnableAllUserPendingQueue = EnableAllUserPendingQueue,
                WorkflowName = WorkflowName,
                RedactionTypes = RedactionTypes?.ToList(),
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="fileProcessingDB"></param>
        /// <param name="configurationName"></param>
        public void LoadConfigurationData(FileProcessingDB fileProcessingDB, string configurationName)
        {
            try
            {
                var configuration = ConfigurationUtilities.GetWebConfigurationForConfigurationName(fileProcessingDB, configurationName);

                RedactionWebConfiguration? backendWebConfiguration =
                    JsonSerializer.Deserialize<RedactionWebConfiguration>(configuration);

                if (backendWebConfiguration != null
                    && backendWebConfiguration.ConfigurationName != null)
                {
                    AttributeSet = backendWebConfiguration.AttributeSet;
                    ProcessingAction = backendWebConfiguration.ProcessingAction;
                    PostProcessingAction = backendWebConfiguration.PostProcessingAction;
                    ActiveDirectoryGroup = backendWebConfiguration.ActiveDirectoryGroup;
                    ConfigurationName = backendWebConfiguration.ConfigurationName;
                    DocumentTypeFileLocation = backendWebConfiguration.DocumentTypeFileLocation;
                    EnableAllUserPendingQueue = backendWebConfiguration.EnableAllUserPendingQueue;
                    WorkflowName = backendWebConfiguration.WorkflowName;
                    RedactionTypes = backendWebConfiguration.RedactionTypes?.ToList();
                }
                else
                {
                    throw new ExtractException("ELI53705", $"Could not deserialize {configurationName} into a redaction web configuration.");
                }
                
            }
            catch (Exception e)
            {
                var ee = e.AsExtract("ELI53685");
                ee.AddDebugData("Info", "Error parsing web configuration from database.");
                throw ee;
            }
        }
    }
}
