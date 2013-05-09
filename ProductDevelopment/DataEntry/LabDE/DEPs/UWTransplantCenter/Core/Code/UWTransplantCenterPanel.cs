using Extract.DataEntry;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Extract.DataEntry.DEP.UWTransplantCenter
{
    /// <summary>
    /// A sample <see cref="DataEntryControlHost"/> intended to demonstrate functionality.
    /// </summary>
    public partial class UWTransplantCenterPanel : DataEntryControlHost
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(UWTransplantCenterPanel).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UWTransplantCenterPanel"/> class.
        /// </summary>
        public UWTransplantCenterPanel() 
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
                    LicenseIdName.LabDEVerificationUIObject, "ELI35805", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI35806", ex);
            }
        }

        #endregion Constructors
    }
}
