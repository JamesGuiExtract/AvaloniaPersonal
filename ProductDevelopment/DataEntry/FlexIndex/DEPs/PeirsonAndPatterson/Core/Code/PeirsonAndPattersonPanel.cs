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
    /// A <see cref="DataEntryControlHost"/> for the Peirson and Patterson FLEX project.
    /// </summary>
    public partial class PeirsonAndPattersonPanel : DataEntryControlHost
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(PeirsonAndPattersonPanel).ToString();

        #endregion Constants

        /// <summary>
        /// Initializes a new <see cref="PeirsonAndPattersonPanel"/> instance.
        /// </summary>
        public PeirsonAndPattersonPanel()
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
                    LicenseIdName.FlexIndexIDShieldCoreObjects, "ELI35329", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI35330", ex);
            }
        }
    }
}
