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
    /// that interacts with the <see cref="ImageViewer"/> control's 
    /// <see cref="P:Extract.Imaging.Forms.ImageViewer.CursorTool"/> property.
    /// </summary>
    public abstract partial class ImageViewerCursorToolStripButton : ToolStripButtonBase, 
        IImageViewerControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ImageViewerCursorToolStripButton).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Cursor tool type that this button controls.
        /// </summary>
        readonly CursorTool _cursorTool;

        /// <summary>
        /// Image viewer with which this button connects.
        /// </summary>
        IDocumentViewer _imageViewer;

        /// <summary>
        /// Tool tip text without shortcut keys text
        /// </summary>
        string _baseToolTipText;

        /// <summary>
        /// Flag that allows child classes to disallow the cursor tool change
        /// in the <see cref="Control.Click"/> handler.
        /// </summary>
        bool _allowCursorToolChangeOnClick = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ImageViewerCursorToolStripButton"/>
        /// class that interacts with the specified 
        /// <see cref="CursorTool"/> enum value type.
        /// </summary>
        /// <param name="cursorTool">Cursor tool value type with which the tool strip button 
        /// interacts.</param>
        /// <param name="buttonText">The text that will be used to set the
        /// <see cref="ToolStripItem.Text"/> property. Must not be empty string or 
        /// <see langword="null"/>.</param>
        /// <param name="buttonImage">The embedded resource name of the image to set the 
        /// <see cref="ToolStripItem.Image"/> property. Must not be empty string or 
        /// <see langword="null"/>.</param>
        /// <param name="toolTipText">The text to display in the tool tip for this 
        /// <see cref="ToolStripButton"/>.</param>
        /// <param name="buttonType"><see cref="System.Type"/> used to resolve namespace
        /// for loading the embedded resources (can be obtained via <see langword="typeof"/>
        /// (ClassName)).</param>
        /// <example>
        /// <code lang="C#">
        /// // Represents a ToolStripButton that enables the Pan CusorTool
        /// [ToolboxBitmap(typeof(PanToolStripButton), "Resources.PanButton.bmp")]
        /// public partial class PanToolStripButton : ImageViewerCursorToolStripButton
        /// {
        ///     // Initializes a new ToolStripButton class.
        ///     public PanToolStripButton()
        ///         : base(CursorTool.Pan, "Pan window", "Resources.PanButton.bmp",
        ///         "Pan", typeof(PanToolStripButton))
        ///     {
        ///         // Initialize the component
        ///         InitializeComponent();
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <exception cref="ExtractException"><paramref name="buttonText"/> is empty or 
        /// <see langword="null"/>.</exception>
        /// <exception cref="ExtractException"><paramref name="buttonImage"/> is empty or 
        /// <see langword="null"/>.</exception>
        /// <exception cref="ExtractException"><paramref name="buttonImage"/> is not a valid
        /// embedded resource name.</exception>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        protected ImageViewerCursorToolStripButton(CursorTool cursorTool, string buttonText,
            string buttonImage, string toolTipText, Type buttonType)
            : base(buttonType, buttonImage)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23102",
					_OBJECT_NAME);

                // Ensure button text and image have been specified
                ExtractException.Assert("ELI21227", "buttonText must not be empty!",
                    !string.IsNullOrEmpty(buttonText));

                // Store the cursor tool that this button controls
                _cursorTool = cursorTool;

                // Call auto-generated code to initialize the component
                InitializeComponent();

                // Set the text for the button
                base.Text = buttonText;

                // Set the tool tip text for the button
                _baseToolTipText = toolTipText;
                base.ToolTipText = toolTipText;

                // Button is not enabled until an image is open on the associated image viewer 
                // control
                base.Enabled = false;
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI21176",
                    "Unable to initialize ImageViewerCursorToolStripButton base class.", e);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        /// <seealso cref="Control.OnClick"/>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                if (_allowCursorToolChangeOnClick
                    && _imageViewer != null && _imageViewer.IsImageAvailable)
                {
                    _imageViewer.CursorTool = _cursorTool;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21177", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
            finally
            {
                base.OnClick(e);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.CursorToolChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.CursorToolChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.CursorToolChanged"/> event.</param>
        void HandleCursorToolChanged(object sender, CursorToolChangedEventArgs e)
        {
            try
            {
                // Update only the checked state of the button.
                SetButtonState(false);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21417", ex);
                ee.AddDebugData("Cursor tool", e.CursorTool, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.</param>
        void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                SetButtonState(true);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21337", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
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
                ExtractException ee = ExtractException.AsExtractException("ELI21574", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion

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
        /// <see cref="ImageViewerCursorToolStripButton"/>s.
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

        /// <summary>
        /// Gets the text that is displayed in this <see cref="ToolStripButton"/>.
        /// </summary>
        /// <value>The set property has been obsoleted and disabled.</value>
        /// <return>The text currently displayed in this <see cref="ToolStripButton"/>.</return>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                // Do nothing
            }
        }

        /// <summary>
        /// Gets/sets whether cursor tool changes are allowed when the OnClick is handled.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected bool AllowCursorToolChangeOnClick
        {
            get
            {
                return _allowCursorToolChangeOnClick;
            }
            set
            {
                _allowCursorToolChangeOnClick = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the <see cref="ToolStripItem.Enabled"/> and <see cref="ToolStripButton.Checked"/> 
        /// properties depending on the state of the associated image viewer control.
        /// </summary>
        /// <param name="updateEnabledStatus"><see langword="true"/> to update the enabled status
        /// of the button based on whether an image is open; <see langword="false"/> to update only
        /// the checked state of the button.</param>
        protected virtual void SetButtonState(bool updateEnabledStatus)
        {
            // Check whether an image is open on an associated image viewer control.
            bool imageIsOpen = _imageViewer != null && _imageViewer.IsImageAvailable;

            // Update the enabled state if requested.
            if (updateEnabledStatus)
            {
                // Enable the button iff the image is open
                base.Enabled = imageIsOpen;
            }

            // Check the button iff the image is open and the current cursor tool is selected
            Checked = imageIsOpen && _imageViewer.CursorTool == _cursorTool;
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

        #endregion

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer with which to establish a connection.
        /// </summary>
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
                        _imageViewer.CursorToolChanged -= HandleCursorToolChanged;
                        _imageViewer.ImageFileChanged -= HandleImageFileChanged;
                        _imageViewer.Shortcuts.ShortcutKeyChanged -= HandleShortcutKeyChanged;
                    }

                    // Store the new image viewer internally
                    _imageViewer = value;
                    
                    // Check if an image viewer was specified
                    if (_imageViewer != null)
                    {
                        _imageViewer.CursorToolChanged += HandleCursorToolChanged;
                        _imageViewer.ImageFileChanged += HandleImageFileChanged;
                        _imageViewer.Shortcuts.ShortcutKeyChanged += HandleShortcutKeyChanged;
                    }

                    // Set the button state
                    SetButtonState(true);

                    // Set tool tip text
                    SetToolTipText();
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21179",
                        "Unable to establish connection to image viewer.", e);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}
