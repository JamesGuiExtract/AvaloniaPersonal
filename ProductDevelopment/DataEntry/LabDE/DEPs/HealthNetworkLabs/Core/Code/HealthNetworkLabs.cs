using Extract.DataEntry;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Extract.DataEntry.DEP.HealthNetworkLabs
{
    /// <summary>
    /// A LabDE <see cref="DataEntryControlHost"/> customized for Health Network Labs.
    /// </summary>
    public partial class HealthNetworkLabsPanel : DataEntryControlHost
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(HealthNetworkLabsPanel).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthNetworkLabsPanel"/> class.
        /// </summary>
        public HealthNetworkLabsPanel() 
            : base()
        {
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDEVerificationUIObject, "ELI34403", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI34404", ex);
            }
        }

        #endregion Constructors
    }
}
