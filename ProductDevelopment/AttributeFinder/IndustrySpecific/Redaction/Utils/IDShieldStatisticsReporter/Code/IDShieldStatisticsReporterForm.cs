using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.IDShieldStatisticsReporter
{
    /// <summary>
    /// Allow recaction accuracy statistics to be generated from feedback data sets.
    /// </summary>
    public partial class IDShieldStatisticsReporterForm : Form
    {
        #region Fields

        static string _OBJECT_NAME = typeof(IDShieldStatisticsReporterForm).ToString();

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.FlexIndexIDShieldCoreObjects, _OBJECT_NAME);

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="IDShieldStatisticsReporterForm"/> instance.
        /// </summary>
        public IDShieldStatisticsReporterForm()
        {
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                _licenseCache.Validate("ELI28534");

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28535", ex);
            }
        }

        #endregion Constructors
    }
}