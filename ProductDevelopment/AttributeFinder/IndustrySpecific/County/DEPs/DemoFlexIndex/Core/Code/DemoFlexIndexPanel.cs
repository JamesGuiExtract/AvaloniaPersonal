using Extract.DataEntry;
using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Extract.DataEntry.DEP.DemoFlexIndex
{
    /// <summary>
    /// A sample <see cref="DataEntryControlHost"/> intended to demonstrate data entry functionality
    /// in the county document industry.
    /// </summary>
    public partial class DemoFlexIndexPanel : DataEntryControlHost
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DemoFlexIndexPanel).ToString();

        #endregion Constants

        /// <summary>
        /// Initializes a new <see cref="DemoFlexIndexPanel"/> instance.
        /// </summary>
        public DemoFlexIndexPanel()
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
                    LicenseIdName.FlexIndexIDShieldCoreObjects, "ELI29164", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29165", ex);
            }
        }
    }
}
