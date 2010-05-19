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

namespace Extract.DataEntry.DEP.Generic
{
    /// <summary>
    /// A sample <see cref="DataEntryControlHost"/> intended to provide a UI for a set of simple
    /// text fields.
    /// </summary>
    public partial class GenericPanel : DataEntryControlHost
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(GenericPanel).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="GenericPanel"/> instance.
        /// </summary>
        public GenericPanel()
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
                    LicenseIdName.DataEntryCoreComponents, "ELI30104", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30105", ex);
            }
        }

        #endregion Constructors
    }
}