using Extract.DataEntry;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Extract.DataEntry.DEP.StandardLabDE
{
    /// <summary>
    /// A sample <see cref="DataEntryControlHost"/> intended to demonstrate functionality.
    /// </summary>
    public partial class StandardLabDEPanel : DataEntryControlHost
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(StandardLabDEPanel).ToString();

        #endregion Constants

        #region Constructors

        public StandardLabDEPanel() 
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
                    LicenseIdName.LabDEVerificationUIObject, "ELI26970", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25408", ex);
            }
        }

        #endregion Constructors
    }
}
