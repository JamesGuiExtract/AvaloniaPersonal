using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Provides the <see langword="abstract"/> base class for a <see cref="ToolStripButton"/> 
    /// that interacts with the <see cref="ImageViewer"/> control. 
    /// </summary>
    public abstract partial class ImageViewerCommandToolStripButton : ToolStripButtonBase, 
        IImageViewerControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ImageViewerCommandToolStripButton).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Image viewer with which this button connects.
        /// </summary>
        IDocumentViewer _imageViewer;

        /// <summary>
        /// Tool tip text without shortcut keys text
        /// </summary>
        string _baseToolTipText;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ImageViewerCommandToolStripButton"/>
        /// class that interacts with the currently loaded immage.
        /// </summary>
        /// <param name="buttonImage">The embedded resource name of the image to set the 
        /// <see cref="ToolStripItem.Image"/> property. Must not be empty string or 
        /// <see langword="null"/>.</param>
        /// <param name="toolTipText">The text to display in the tool tip for this 
        /// <see cref="ToolStripButton"/>.</param>
        /// <param name="buttonType"><see cref="System.Type"/> used to resolve namespace
        /// for loading the embedded resources (can be obtained via <see langword="typeof"/>
        /// (ClassName)).</param>
        /// <param name="buttonText">The text that will be used to set the
        /// <see cref="ToolStripItem.Text"/> property.</param>
        /// <example>
        /// <code lang="C#">
        /// // Represents a ToolStripButton that enables the ZoomIn Command
        /// [ToolboxBitmap(typeof(ZoomInToolStripButton), "Resources.ZoomIn.png")]
        /// public partial class ZoomInToolStripButton : ImageViewerCommandToolStripButton
        /// {
        ///     // Initializes a new ToolStripButton class.
        ///     public ZoomInToolStripButton()
        ///         : base("Resources.ZoomIn.png", "Zoom in", 
        ///         "Zoom in", typeof(ZoomInToolStripButton))
        ///     {
        ///         // Initialize the component
        ///         InitializeComponent();
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <exception cref="ExtractException"><paramref name="buttonImage"/> is empty or 
        /// <see langword="null"/>.</exception>
        /// <exception cref="ExtractException"><paramref name="buttonImage"/> is not a valid
        /// embedded resource name.</exception>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        protected ImageViewerCommandToolStripButton(string buttonImage, string toolTipText,
            Type buttonType, string buttonText)
            : base(buttonType, buttonImage)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23101",
					_OBJECT_NAME);

                // Call auto-generated code to initialize the component
                InitializeComponent();

                // Set the tool tip text for the button
                _baseToolTipText = toolTipText;
                base.ToolTipText = toolTipText;

                // Set the text for the button if it has been specified
                if (!string.IsNullOrEmpty(buttonText))
                {
                    base.Text = buttonText;
                }

                // Disable the button by default
                base.Enabled = false;
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI21330",
                    "Unable to initialize ImageViewerCommandToolStripButton base class.", e);
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Set the <see cref="ImageViewerCommandToolStripButton"/> to enabled if the
        /// control is associated with an <see cref="ImageViewer"/> and there is
        /// an image opened in the associated <see cref="ImageViewer"/>.
        /// </summary>
        protected virtual void SetEnabledState()
        {
            base.Enabled = (_imageViewer != null && _imageViewer.IsImageAvailable);
        }

        /// <summary>
        /// Sets the tool tip text for the button including the keyboard shortcut.
        /// </summary>
        void SetToolTipText()
        {
            // Get the original tool tip text
            string toolTipText = _baseToolTipText;

            // Check if an image viewer is attached
            if (_imageViewer != null)
            {
                // Get the shortcut keys associated with this tool
                string shortcuts = ShortcutsManager.GetDisplayString(GetKeys());
                if (shortcuts.Length != 0)
                {
                    toolTipText += " (" + shortcuts + ")";
                }
            }

            // Set the tool tip text
            base.ToolTipText = toolTipText;
        }

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the button.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the button. 
        /// May be <see langword="null"/> if no keys are associated with the button.</returns>
        protected abstract Keys[] GetKeys();

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.</param>
        void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            // Set the enabled state
            SetEnabledState();
        }

        /// <summary>
        /// Handles the <see cref="ShortcutsManager.ShortcutKeyChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ShortcutsManager.ShortcutKeyChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ShortcutsManager.ShortcutKeyChanged"/> event.</param>
        void HandleShortcutKeyChanged(object sender, ShortcutKeyChangedEventArgs e)
        {
            try
            {
                SetToolTipText();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21575", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region Properties

        /// <summary>
        /// The tooltip text to display in the <see cref="ToolStripButton"/>. (minus any shortcut
        /// keys assigned for this command)
        /// </summary>
        /// <value>The tooltip text to display.</value>
        /// <returns>The tooltip text to display.</returns>
        public string BaseToolTipText
        {
            get
            {
                return _baseToolTipText;
            }

            set
            {
                _baseToolTipText = value;
            }
        }

        /// <summary>
        /// Masks ToolTipText property of <see cref="ToolStripButton"/> which shouldn't be used.
        /// Rather the <see cref="BaseToolTipText"/> property should be used for
        /// <see cref="ImageViewerCommandToolStripButton"/>s.
        /// </summary>
        /// <return>The ToolTipText currently displayed in this <see cref="ToolStripButton"/>.
        /// </return>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string ToolTipText
        {
            get
            {
                return base.ToolTipText;
            }
        }

        #endregion Properties

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer with which to establish a connection.
        /// If you need to handle more events than just the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event then
        /// you will need to override this method in your derived class.
        /// <para><b>NOTE:</b></para>
        /// The set will also call <see cref="SetEnabledState"/>.
        /// </summary>
        /// <example>
        /// <code lang="C#">
        /// // Example of overridden ImageViewer property in derived class
        /// public ImageViewer ImageViewer
        /// {
        ///     get
        ///     {
        ///         return base.ImageViewer;
        ///     }
        ///     set
        ///     {
        ///         try
        ///         {
        ///             // Unregister from previously subscribed-to events
        ///             if (base.ImageViewer != null)
        ///             {
        ///                 base.ImageViewer.PageChanged -= HandlePageChanged;
        ///             }
        /// 
        ///             // Call the base class set property
        ///             // (Note - this will call the SetEnabledState())
        ///             base.ImageViewer = value;
        /// 
        ///             // Register for events
        ///             if (base.ImageViewer != null)
        ///             {
        ///                 base.ImageViewer.PageChanged += HandlePageChanged;
        ///             }
        ///         }
        ///         catch (Exception ex)
        ///         {
        ///             ExtractException ee = new ExtractException("ELI00000",
        ///                 "Unable to establish connection to image viewer.", ex);
        ///                 ee.AddDebugData("Image viewer", value.ToString(), false);
        ///                 throw ee;
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <value>The image viewer with which to establish a connection. <see langword="null"/> 
        /// indicates the connection should be disconnected from the current image viewer.</value>
        /// <returns>The image viewer with which a connection is established. 
        /// <see langword="null"/> if no image viewer is connected.</returns>
        /// <seealso cref="IImageViewerControl"/>
        [CLSCompliant(false)]
        public virtual IDocumentViewer ImageViewer
        {
            get
            {
                return _imageViewer;
            }
            set
            {
                try
                {
                    // Unregister from previously subscribed-to events
                    if (_imageViewer != null)
                    {
                        _imageViewer.ImageFileChanged -= HandleImageFileChanged;
                        _imageViewer.Shortcuts.ShortcutKeyChanged -= HandleShortcutKeyChanged;
                    }

                    // Store the new image viewer internally
                    _imageViewer = value;

                    // Register for events
                    if (_imageViewer != null)
                    {
                        _imageViewer.ImageFileChanged += HandleImageFileChanged;
                        _imageViewer.Shortcuts.ShortcutKeyChanged += HandleShortcutKeyChanged;
                    }

                    // Update buttons enabled state
                    SetEnabledState();

                    // Set the tool tip text
                    SetToolTipText();
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21332",
                        "Unable to establish connection to image viewer.", e);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members
    }
}
