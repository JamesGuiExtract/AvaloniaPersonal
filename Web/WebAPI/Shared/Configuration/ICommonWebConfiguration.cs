using UCLID_FILEPROCESSINGLib;

namespace WebAPI.Configuration
{
    public interface IWebConfiguration
    {
        /// <summary>
        /// Gets or sets the unique configuration name.
        /// </summary>
        public string ConfigurationName { get; set; }

        /// <summary>
        /// Gets or sets the workflow this configuration is assigned to.
        /// </summary>
        public string WorkflowName { get; set; }

        public string AttributeSet { get; set; }

        public string ProcessingAction { get; set; }

        public string PostProcessingAction { get; set; }

        public IWebConfiguration Copy();
        public void LoadConfigurationData(FileProcessingDB fileProcessingDB, string configurationName);
    }
}
