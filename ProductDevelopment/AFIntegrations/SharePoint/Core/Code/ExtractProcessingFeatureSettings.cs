using Microsoft.SharePoint.Administration;

namespace Extract.SharePoint
{
    /// <summary>
    /// Persisted settings class for use with SharePoint
    /// </summary>
    [System.Runtime.InteropServices.GuidAttribute("CE4EF627-58BE-45D0-8F4C-9BCBE3BB81FC")]
    internal class ExtractProcessingFeatureSettings : SPPersistedObject
    {
        #region Fields

        /// <summary>
        /// The local working folder for the extract feature.
        /// </summary>
        [Persisted]
        string _localWorkingFolder;

        /// <summary>
        /// The ip address for the Extract exception service.
        /// </summary>
        [Persisted]
        string _exceptionServiceIpAddress;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractProcessingFeatureSettings"/> class.
        /// </summary>
        public ExtractProcessingFeatureSettings()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractProcessingFeatureSettings"/> class.
        /// </summary>
        /// <param name="name">The name for the persisted object.</param>
        /// <param name="parent">The parent for the persisted object.</param>
        public ExtractProcessingFeatureSettings(string name, SPPersistedObject parent)
            : base(name, parent)
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Indicates whether this persisted class has additional update access
        /// </summary>
        /// <returns><see langword="true"/></returns>
        protected override bool HasAdditionalUpdateAccess()
        {
            return true;
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets or sets the local working folder.
        /// </summary>
        /// <value>The local working folder.</value>
        public string LocalWorkingFolder
        {
            get
            {
                return _localWorkingFolder ?? string.Empty;
            }
            set
            {
                _localWorkingFolder = value ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the exception service ip address.
        /// </summary>
        /// <value>The exception service ip address.</value>
        public string ExceptionServiceIpAddress
        {
            get
            {
                return _exceptionServiceIpAddress ?? string.Empty;
            }
            set
            {
                _exceptionServiceIpAddress = value ?? string.Empty;
            }
        }

        #endregion Properties
    }
}
