using Extract.Licensing;
using Extract.Redaction;
using Extract.Utilities.Forms;
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
    /// Enables the location of found and expected data to be specified.
    /// </summary>
    public partial class FeedbackAdvancedOptionsForm : Form
    {
        #region Fields

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FeedbackAdvancedOptionsForm).ToString();

        /// <summary>
        /// The IDShieldTesterFolder representing the folder to be tested.
        /// </summary>
        IDShieldTesterFolder _testFolder;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="FeedbackAdvancedOptionsForm"/> instance.
        /// </summary>
        /// <param name="testFolder">The <see cref="IDShieldTesterFolder"/> instance for storing the
        /// test folder settings.</param>
        public FeedbackAdvancedOptionsForm(IDShieldTesterFolder testFolder)
        {
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects, "ELI28652",
					_OBJECT_NAME);

                InitializeComponent();

                _testFolder = testFolder;
                _foundDataPathTextBox.Text = _testFolder.FoundDataLocation;
                _expectedDataPathTextBox.Text = _testFolder.ExpectedDataLocation;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28653", ex);
            }
        }

        #endregion Constructors

        #region Event handlers

        /// <summary>
        /// Handles the OK button <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void OnOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                _testFolder.FoundDataLocation = _foundDataPathTextBox.Text;
                _testFolder.ExpectedDataLocation = _expectedDataPathTextBox.Text;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28673", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion Event handlers
    }
}