using Extract;
using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a pre-populated <see cref="ContextMenuStrip"/> that interacts
    /// with the <see cref="DocumentViewer"/>.
    /// </summary>
    [ToolboxBitmap(typeof(ToolStripDropDown))]
    public partial class ImageViewerContextMenuStrip : ContextMenuStrip
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(ImageViewerContextMenuStrip).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ImageViewerContextMenuStrip"/> item.
        /// </summary>
        public ImageViewerContextMenuStrip()
            : base()
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23117",
					_OBJECT_NAME);

                InitializeComponent();

                // Load the menu items into the context menu
                base.Items.AddRange(new ToolStripItem[] {
                new ZoomWindowToolStripMenuItem(),
                new PanToolStripMenuItem(),
                new ToolStripSeparator(),
                new ZoomPreviousToolStripMenuItem(),
                new ZoomNextToolStripMenuItem(),
                new ToolStripSeparator(),
                new HighlightToolStripMenuItem()});
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23118", ex);
            }
        }

        #endregion
    }
}
