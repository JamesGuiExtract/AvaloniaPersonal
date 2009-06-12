using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Provides the <see langword="abstract"/> base class for a <see cref="ToolStripButton"/> 
    /// that interacts with the <see cref="ImageViewer"/> control's 
    /// <see cref="P:Extract.Imaging.Forms.ImageViewer.CursorTool"/> property.
    /// </summary>
    public abstract partial class ImageViewerCursorToolStripButton : ToolStripButton, 
        IImageViewerControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(ImageViewerCursorToolStripButton).ToString();

        #endregion Constants

        #region ImageViewerCursorToolStripButton Fields

        /// <summary>
        /// Cursor tool type that this button controls.
        /// </summary>
        private readonly CursorTool _cursorTool;

        /// <summary>
        /// Image viewer with which this button connects.
        /// </summary>
        private ImageViewer _imageViewer;

        /// <summary>
        /// Tool tip text without shortcut keys text
        /// </summary>
        private string _baseToolTipText;

        #endregion

        #region ImageViewerCursorToolStripButton Constructors

        /// <summary>
        /// Initializes a new <see cref="ImageViewerCursorToolStripButton"/>
        /// class that interacts with the specified 
        /// <see cref="T:ImageViewer.CursorTool"/> enum value type.
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23102",
                    _OBJECT_NAME);

                // Ensure button text and image have been specified
                ExtractException.Assert("ELI21227", "buttonText must not be empty!",
                    !string.IsNullOrEmpty(buttonText));
                ExtractException.Assert("ELI21228", "buttonImage must not be empty!",
                    !string.IsNullOrEmpty(buttonImage));

                // Store the cursor tool that this button controls
                _cursorTool = cursorTool;

                // Call auto-generated code to initialize the component
                InitializeComponent();

                // Set the text for the button
                base.Text = buttonText;

                // Load and set the image for this compononent from the embedded resource
                base.Image = (Image)new Bitmap(buttonType, buttonImage); 

                // Set the tool tip text for the button
                _baseToolTipText = toolTipText;
                base.ToolTipText = toolTipText;

                // Set the display style to image
                base.DisplayStyle = ToolStripItemDisplayStyle.Image;

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

        #region ImageViewerCursorToolStripButton Events

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
                if (_imageViewer != null && _imageViewer.IsImageAvailable)
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

        #region ImageViewerCursorToolStripButton Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.CursorToolChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.CursorToolChanged"/> event.</param>
        private void HandleCursorToolChanged(object sender, CursorToolChangedEventArgs e)
        {
            try
            {
                SetButtonState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21417", ex);
                ee.AddDebugData("Cursor tool", e.CursorTool, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        private void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                SetButtonState();
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
        private void HandleShortcutKeyChanged(object sender, ShortcutKeyChangedEventArgs e)
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

        #region ImageViewerCursorToolStripButton Properties

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
                // Do nothing
            }
        }

        #endregion

        #region ImageViewerCursorToolStripButton Methods

        /// <summary>
        /// Sets the <see cref="ToolStripItem.Enabled"/> and <see cref="ToolStripButton.Checked"/> 
        /// properties depending on the state of the associated image viewer control.
        /// </summary>
        private void SetButtonState()
        {
            // Check whether an image is open on an associated image viewer control.
            bool imageIsOpen = _imageViewer != null && _imageViewer.IsImageAvailable;

            // Enable the button iff the image is open
            base.Enabled = imageIsOpen;

            // Check the button iff the image is open and the current cursor tool is selected
            base.Checked = imageIsOpen && _imageViewer.CursorTool == _cursorTool;
        }

        /// <summary>
        /// Sets the tool tip text for the button including the keyboard shortcut.
        /// </summary>
        private void SetToolTipText()
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
        public ImageViewer ImageViewer
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
                    SetButtonState();

                    // Set tool tip text
                    SetToolTipText();
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21179",
                        "Unable to establish connection to image viewer.", e);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}
