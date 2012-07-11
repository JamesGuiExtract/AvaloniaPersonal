using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Extract.DataEntry.DEP.UITransplantCenter
{
    /// <summary>
    /// A LabDE <see cref="DataEntryControlHost"/> customized for the University of Iowa Organ
    /// Transplant Center
    /// </summary>
    public partial class UITransplantCenterPanel : DataEntryControlHost
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(UITransplantCenterPanel).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UITransplantCenterPanel"/> class.
        /// </summary>
        public UITransplantCenterPanel()
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
                    LicenseIdName.LabDEVerificationUIObject, "ELI34847", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI34848", ex);
            }
        }

        #endregion Constructors
    }
}
