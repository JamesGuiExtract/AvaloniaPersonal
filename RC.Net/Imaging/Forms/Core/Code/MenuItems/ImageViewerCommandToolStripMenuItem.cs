using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Provides the <see langword="abstract"/> base class for a <see cref="ToolStripMenuItem"/> 
    /// that interacts with the <see cref="ImageViewer"/> control. 
    /// </summary>
    public abstract partial class ImageViewerCommandToolStripMenuItem : ToolStripMenuItem, 
        IImageViewerControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(ImageViewerCommandToolStripMenuItem).ToString();

        #endregion Constants

        #region ImageViewerCommandToolStripMenuItem Fields

        /// <summary>
        /// Image viewer with which this menu item connects.
        /// </summary>
        private ImageViewer _imageViewer;

        #endregion

        #region ImageViewerCommandToolStripMenuItem Constructors

        /// <summary>
        /// Initializes a new <see cref="ImageViewerCommandToolStripMenuItem"/>
        /// class that interacts with the currently loaded image.
        /// </summary>
        /// <param name="menuItemText">The text to display in the 
        /// <see cref="ToolStripMenuItem"/>. Must not be empty string or
        /// <see langword="null"/>.</param>
        /// <param name="menuItemImage">The embedded resource name of the image
        /// to display in the <see cref="ToolStripMenuItem"/></param>
        /// <param name="menuItemType"><see cref="System.Type"/> used to resolve namespace
        /// for loading the embedded resources (can be obtained via <see langword="typeof"/>
        /// (ClassName)). May be <see langword="null"/> iff 
        /// <paramref name="menuItemImage"/> is empty string or
        /// <see langword="null"/>.</param>
        /// <example>
        /// <code lang="C#">
        /// public partial class ZoomInToolStripMenuItem : ImageViewerCommandToolStripMenuItem
        /// {
        ///     // Contruct a new ZoomInToolStripMenuItem
        ///     public ZoomInToolStripMenuItem()
        ///         : base(ToolStripButtonConstants._ZOOM_IN_BUTTON_TOOL_TIP,
        ///         ToolStripButtonConstants._ZOOM_IN_BUTTON_IMAGE,
        ///         typeof(ZoomInToolStripMenuItem))
        ///     {
        ///         InitializeComponent();
        ///     }
        /// 
        ///     // Need to handle the Control.Click event
        ///     protected override void OnClick(EventArgs e)
        ///     {
        ///         try
        ///         {
        ///             // Ensure there is an image viewer with an open image before zooming
        ///             if (base.ImageViewer != null &amp;&amp; base.ImageViewer.IsImageAvailable)
        ///             {
        ///                 // Zoom in
        ///                 base.ImageViewer.ZoomIn();
        ///             }
        ///         }
        ///         // Handle any exception that may be thrown
        ///         catch (Exception ex)
        ///         {
        ///             ExtractException ee = ExtractException.AsExtractException("ELI00000", ex);
        ///             ee.AddDebugData("Event arguments", e, false);
        ///             ee.Display();
        ///         }
        ///         finally
        ///         {
        ///             // Ensure we call the base class OnClick
        ///             base.OnClick(e);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <exception cref="ExtractException"><paramref name="menuItemText"/> is
        /// emptry string or <see langword="null"/>.</exception>
        /// <exception cref="ExtractException"><paramref name="menuItemImage"/> is
        /// <see langword="not"/><see cref="string.IsNullOrEmpty(string)"/>
        /// <see langword="and"/> <paramref name="menuItemType"/> is
        /// <see langword="null"/>.</exception>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        protected ImageViewerCommandToolStripMenuItem(string menuItemText, string menuItemImage, 
            Type menuItemType) 
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23116",
					_OBJECT_NAME);

                // Ensure a type has been passed in for the menu item
                ExtractException.Assert("ELI21382", 
                    "You must specify a valid type for the menu item!", 
                    string.IsNullOrEmpty(menuItemImage) || menuItemType != null);

                // Ensure that text has been specified for the menu item
                ExtractException.Assert("ELI21383", "menuItemText cannot be null or empty!",
                    !string.IsNullOrEmpty(menuItemText));

                InitializeComponent();

                // Set the text for the menu item
                base.Text = menuItemText;

                if (!string.IsNullOrEmpty(menuItemImage))
                {
                    base.Image = (Image)new Bitmap(menuItemType, menuItemImage);

                    // Set the display style to image and text
                    base.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                }
                else
                {
                    // No image specified set the display style to just text
                    base.DisplayStyle = ToolStripItemDisplayStyle.Text;
                }

                // Disable the menu item by default
                base.Enabled = false;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI21384",
                    "Unable to initialize ImageViewerToolStripCommandMenuItem base class!", ex);
                
                ee.AddDebugData("Text", menuItemText, false);
                ee.AddDebugData("Image name", menuItemImage, false);
                ee.AddDebugData("Type", menuItemType == null ? "null" : menuItemType.ToString(), 
                    false);

                throw ee;
            }
        }

        #endregion

        #region ImageViewerCommandToolStripMenuItem Methods

        /// <summary>
        /// Sets the enabled state of the <see cref="ImageViewerCommandToolStripMenuItem"/>
        /// </summary>
        protected virtual void SetEnabledState()
        {
            base.Enabled = _imageViewer != null && _imageViewer.IsImageAvailable;
        }

        /// <summary>
        /// Sets the shortcut key text for the menu item.
        /// </summary>
        private void SetShortcutKeys()
        {
            // Set the shortcut key text
            base.ShortcutKeyDisplayString = 
                _imageViewer == null ? "" : ShortcutsManager.GetDisplayString(GetKeys()); 
        }

        /// <summary>
        /// Retrieves an array of the shortcut keys associated with the menu item.
        /// </summary>
        /// <returns>Retrieves an array of the shortcut keys associated with the menu item. 
        /// May be <see langword="null"/> if no keys are associated with the menu item.</returns>
        protected abstract Keys[] GetKeys();

        #endregion

        #region ImageViewerCommandToolStripMenuItem Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        private void HandleImageChanged(object sender, ImageFileChangedEventArgs e)
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
        private void HandleShortcutKeyChanged(object sender, ShortcutKeyChangedEventArgs e)
        {
            try
            {
                // Set the shortcut keys
                SetShortcutKeys();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21912", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer with which to establish a connection.
        /// If you need to handle more events than just the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event then
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
        ///                 ee.AddDebugData("Image viewer", value);
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
        public virtual ImageViewer ImageViewer
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
                        _imageViewer.ImageFileChanged -= HandleImageChanged;
                        _imageViewer.Shortcuts.ShortcutKeyChanged -= HandleShortcutKeyChanged;
                    }

                    // Store the new image viewer internally
                    _imageViewer = value;
                    
                    // Check if an image viewer was specified
                    if (_imageViewer != null)
                    {
                        _imageViewer.ImageFileChanged += HandleImageChanged;
                        _imageViewer.Shortcuts.ShortcutKeyChanged += HandleShortcutKeyChanged;
                    }

                    // Set the button state
                    SetEnabledState();

                    // Set the shortcut key
                    SetShortcutKeys();
                }
                catch (Exception e)
                {
                    ExtractException ee = new ExtractException("ELI21385",
                        "Unable to establish connection to image viewer.", e);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion
    }
}
