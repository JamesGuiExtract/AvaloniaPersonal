using Extract;
using Extract.DataEntry;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Extract.DataEntry.DEP.TEReceipts
{
    /// <summary>
    /// A sample <see cref="DataEntryControlHost"/> intended to demonstrate receipt functionality.
    /// </summary>
    public partial class TEReceiptsPanel : DataEntryControlHost
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(TEReceiptsPanel).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="TEReceiptsPanel"/> instance.
        /// </summary>
        public TEReceiptsPanel()
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
                    LicenseIdName.DataEntryCoreComponents, "ELI30017", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30018", ex);
            }
        }

        #endregion Constructors
    }
}