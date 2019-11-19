using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a <see cref="ToolStripTextBox"/> that allows for page navigation on an 
    /// associated image viewer control.
    /// </summary>
    [ToolboxBitmap(typeof(OpenImageToolStripSplitButton),
        ToolStripButtonConstants._PAGE_NAVIGATION_TEXTBOX_IMAGE)]
    public partial class PageNavigationToolStripTextBox : ToolStripTextBox, IImageViewerControl
    {
        #region PageNavigationToolStripTextBox Fields

        /// <summary>
        /// The image viewer control with which the <see cref="PageNavigationToolStripTextBox"/> 
        /// is associated.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// Regular expression used to validate the user input.
        /// </summary>
        Regex _userInputValidator = new Regex("[0-9]+");

        #endregion PageNavigationToolStripTextBox Fields

        #region PageNavigationToolStripTextBox Constructors

        /// <summary>
        /// Initializes a new <see cref="PageNavigationToolStripTextBox"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public PageNavigationToolStripTextBox()
        {
            InitializeComponent();

            base.Enabled = false;
            base.TextBoxTextAlign = HorizontalAlignment.Center;

            // This size allows for four digit page numbers to fit within text box.
            base.Size = new Size(75, 25);
        }

        #endregion PageNavigationToolStripTextBox Constructors

        #region PageNavigationToolStripTextBox Methods

        /// <summary>
        /// Sets the enabled state of the <see cref="PageNavigationToolStripTextBox"/> based on 
        /// the state of its associated image viewer control.
        /// </summary>
        private void SetState()
        {
            // Set the enabled state
            base.Enabled = _imageViewer != null && _imageViewer.IsImageAvailable;

            // Set the text box state
            if (base.Enabled)
            {
                base.Text = _imageViewer.PageNumber.ToString(CultureInfo.CurrentCulture)
                    + " of " + _imageViewer.PageCount.ToString(CultureInfo.CurrentCulture);

                // Select the text in the text box if it currently has the focus
                if (base.Focused)
                {
                    base.SelectAll();
                }
            }
            else
            {
                base.Text = "";
            }
        }

        /// <summary>
        /// Sets the page number of an associated image viewer control based on input that a user 
        /// has provided on the <see cref="PageNavigationToolStripTextBox"/> itself.
        /// </summary>
        private void ProcessUserInput()
        {
            // Check if the text box starts with a number
            Match match = _userInputValidator.Match(base.Text);
            if (match.Success)
            {
                // Set the page number
                int pageNumber = Convert.ToInt32(match.Value, CultureInfo.CurrentCulture);
                if (pageNumber > 0 && pageNumber <= _imageViewer.PageCount)
                {
                    // NOTE: This will raise the PageChanged event, 
                    // which will update this button's state
                    _imageViewer.PageNumber = pageNumber;
                    return;
                }
            }

            // Reset the button's state
            SetState();
        }

        #endregion PageNavigationToolStripTextBox Methods

        #region PageNavigationToolStripTextBox OnEvents

        /// <summary>
        /// Raises the <see cref="Control.KeyPress"/> event.
        /// </summary>
        /// <param name="e">A <see cref="KeyPressEventArgs"/> that contains the event data.</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            try
            {
                base.OnKeyPress(e);

                // Process user input on enter
                if (e.KeyChar == '\r')
                {
                    ProcessUserInput();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21512", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.LostFocus"/> event.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            // [IDSD:257] There is a .NET problem which allows a disabled ToolStripButton to appear selected after
            // a ToolStripTextBox loses focus.  I have found that calling Invalidate or Refresh on the parent
            // toolstrip in OnLostFocus or OnLeave acts as a work-around with the caveat that you still
            // may see a flicker of the button being selected before the selection disappears again.  The flicker
            // appears least likely to occur by using Refresh in the LostFocus event.
            this.Parent.Refresh();

            base.OnLostFocus(e);
        }

        /// <summary>
        /// Raises the <see cref="Control.Leave"/> event.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLeave(EventArgs e)
        {
            try
            {
                base.OnLeave(e);

                ProcessUserInput();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21513", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion PageNavigationToolStripTextBox OnEvents

        #region PageNavigationToolStripTextBox Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.PageChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.PageChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.PageChanged"/> event.</param>
        private void HandlePageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                SetState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI21509", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.LoadingNewImage"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// <see cref="Extract.Imaging.Forms.ImageViewer.LoadingNewImage"/> event.</param>
        /// <param name="e">The event data associated with the
        /// <see cref="Extract.Imaging.Forms.ImageViewer.LoadingNewImage"/> event.</param>
        private void HandleLoadingNewImage(object sender, LoadingNewImageEventArgs e)
        {
            try
            {
                SetState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23257", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the event.</param>
        private void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                SetState();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23315", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion PageNavigationToolStripTextBox Event Handlers

        #region IImageViewerControls Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="PageNavigationToolStripTextBox"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="PageNavigationToolStripTextBox"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="PageNavigationToolStripTextBox"/> is 
        /// connected. <see langword="null"/> if no connections are established.</returns>
        [Browsable(false)]
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
                    // Check if already connected to an image viewer
                    if (_imageViewer != null)
                    {
                        // Unregister from previously subscribed-to events
                        _imageViewer.PageChanged -= HandlePageChanged;
                        _imageViewer.LoadingNewImage -= HandleLoadingNewImage;
                        _imageViewer.ImageFileChanged -= HandleImageFileChanged;

                        // Remove shortcuts
                        _imageViewer.Shortcuts[Keys.G | Keys.Control] = null;
                    }

                    // Store the new image viewer internally
                    _imageViewer = value;

                    // Check if a new image viewer has been connected
                    if (_imageViewer != null)
                    {
                        // Register for events
                        _imageViewer.PageChanged += HandlePageChanged;
                        _imageViewer.LoadingNewImage += HandleLoadingNewImage;
                        _imageViewer.ImageFileChanged += HandleImageFileChanged;

                        // Add shortcuts
                        _imageViewer.Shortcuts[Keys.G | Keys.Control] = Focus;
                    }

                    // Set the state based on the state of the image viewer
                    SetState();
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI21508",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControls Members
    }
}
