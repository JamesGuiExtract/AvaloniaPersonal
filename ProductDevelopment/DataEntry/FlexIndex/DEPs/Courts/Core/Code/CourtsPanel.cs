using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Extract.Licensing;

namespace Extract.DataEntry.DEP.Courts
{
    /// <summary>
    /// A <see cref="DataEntryControlHost"/> intended for indexing of court documents.
    /// </summary>
    public partial class CourtsPanel : DataEntryControlHost
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(CourtsPanel).ToString();

        #endregion Constants

        /// <summary>
        /// Initializes a new instance of the <see cref="CourtsPanel"/> class.
        /// </summary>
        public CourtsPanel()
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
                    LicenseIdName.FlexIndexIDShieldCoreObjects, "ELI34406", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI34407", ex);
            }
        }
    }
}
