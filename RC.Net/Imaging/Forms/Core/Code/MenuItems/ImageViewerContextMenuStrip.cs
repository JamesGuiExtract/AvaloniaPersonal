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
    /// with the <see cref="ImageViewer"/>.
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

        #region Fields

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _OBJECT_NAME);

        #endregion Fields

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
                _licenseCache.Validate("ELI23117");

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
