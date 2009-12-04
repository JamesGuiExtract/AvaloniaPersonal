using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a base class for <see cref="ToolStripButton"/> with a non-default image.
    /// </summary>
    [ToolboxItem(false)]
    public partial class ToolStripButtonBase : ToolStripButton
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ToolStripButtonBase).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ToolStripButtonBase"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public ToolStripButtonBase(Type resourceType, string resourceName)
        {
            // Load licenses in design mode
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                // Load the license files from folder
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            }

            // Validate license
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28737",
                _OBJECT_NAME);


            // Ensure button image has been specified
            ExtractException.Assert("ELI21329", "Resource name expected.",
                !string.IsNullOrEmpty(resourceName));

            InitializeComponent();

            // Load and set the image for this compononent from the embedded resource
            base.Image = new Bitmap(resourceType, resourceName);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the image that is displayed in this <see cref="ToolStripButton"/>.
        /// </summary>
        /// <value>The set property has been obsoleted and disabled.</value>
        /// <return>The image that is displayed in this <see cref="ToolStripButton"/>.</return>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Image Image
        {
            get
            {
                return base.Image;
            }
            set
            {
                // Prevent the image from being modified
            }
        }

        #endregion Properties
    }
}
