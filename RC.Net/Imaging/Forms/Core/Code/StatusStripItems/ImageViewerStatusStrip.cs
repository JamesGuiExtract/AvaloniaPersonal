using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="StatusStrip"/> pre-populated with the following labels:
    /// <para/>
    /// <see cref="UserActionToolStripStatusLabel"/>,
    /// <see cref="BackgroundProcessStatusLabel"/>,
    /// <see cref="ResolutionToolStripStatusLabel"/>, and
    /// <see cref="MousePositionToolStripStatusLabel"/>.
    /// </summary>
    [ToolboxBitmap(typeof(StatusStrip))]
    public partial class ImageViewerStatusStrip : StatusStrip
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(ImageViewerStatusStrip).ToString();

        #endregion Constants

        /// <summary>
        /// Initializes a new <see cref="ImageViewerStatusStrip"/> class.
        /// </summary>
        public ImageViewerStatusStrip()
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23124",
					_OBJECT_NAME);

                InitializeComponent();

                ToolStripStatusLabel[] statusStripItems = new ToolStripStatusLabel[]{
                                                    new UserActionToolStripStatusLabel(),
                                                    new BackgroundProcessStatusLabel(),
                                                    new ResolutionToolStripStatusLabel(),
                                                    new MousePositionToolStripStatusLabel()};

                // Set first label to fill in any extra space
                statusStripItems[0].Spring = true;

                // Loop through all but the last label and set the border style
                // (note: not setting last label because its right border is at
                // the right edge of the dialog).
                for (int i = 0; i < statusStripItems.Length - 1; i++)
                {
                    statusStripItems[i].BorderSides = ToolStripStatusLabelBorderSides.Right;
                    statusStripItems[i].BorderStyle = Border3DStyle.Etched;
                }

                // Add the items to the status strip
                base.Items.AddRange(statusStripItems);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23125", ex);
            }
        }
    }
}
