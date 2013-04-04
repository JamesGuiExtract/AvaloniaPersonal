using Extract.Drawing;
using Extract.Imaging.Forms;
using Extract.Utilities.Forms;
using Leadtools;
using Leadtools.Drawing;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A <see cref="PaginationControl"/> that represents a page in a potential output document with
    /// a thumbnail image of the page.
    /// </summary>
    internal partial class PageThumbnailControl : PaginationControl
    {
        #region Constants

        /// <summary>
        /// Licensing key to unlock document (anti-aliasing) support
        /// </summary>
        static readonly string _DOCUMENT_SUPPORT_KEY = "vhG42tyuh9";
        
        /// <summary>
        /// The padding that should be used for an instance that is preceeded by a separator control.
        /// </summary>
        static readonly Padding _NORMAL_PADDING = new Padding(0, 1, 0, 1);

        /// <summary>
        /// The padding that should be used for an instance that is not preceeded by a separator
        /// control such that one can be added later without shifting the position of this control.
        /// </summary>
        static readonly Padding _SEPARATOR_ALLOWANCE_PADDING =
            new Padding(PaginationSeparator._SEPARATOR_WIDTH, 1, 0, 1);

        /// <summary>
        /// The font to use when drawing an asterix to indicate a page is one of multiple copies.
        /// </summary>
        static readonly Font _COPY_INDICATOR_FONT = new Font("Sans Serif", 20);

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="Size"/> the thumbnail image should be.
        /// </summary>
        static Size _thumbnailSize = new Size(128, 128);

        /// <summary>
        /// The <see cref="Page"/> represented by this instance.
        /// </summary>
        Page _page;

        /// <summary>
        /// The <see cref="ImageViewer"/> being used to display the active page or
        /// <see langword="null"/> if the page is not currently being displayed.
        /// </summary>
        ImageViewer _activeImageViewer;

        /// <summary>
        /// The <see cref="ToolTip"/> to display for this instance.
        /// </summary>
        ToolTip _toolTip = new ToolTip();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PageThumbnailControl"/> class.
        /// </summary>
        /// <param name="document">The <see cref="OutputDocument"/> to which this instance should be
        /// added.</param>
        /// <param name="page">The <see cref="Page"/> represented by this instance.</param>
        public PageThumbnailControl(OutputDocument document, Page page)
            : base()
        {
            try
            {
                ExtractException.Assert("ELI35473", "Null argument exception.", page != null);

                InitializeComponent();

                Padding = _NORMAL_PADDING;

                Document = document;
                _page = page;

                // Set the labels based on the source document name and page number, not the output
                // Filename.
                _fileNameLabel.Text = Path.GetFileNameWithoutExtension(_page.OriginalDocumentName);
                _pageNumberLabel.Text = string.Format(CultureInfo.CurrentCulture, "Page {0:D}",
                    page.OriginalPageNumber);

                if (Document != null)
                {
                    Document.AddPage(this);
                }

                // Add a reference to the Page so that the source document is not deleted before this
                // instance is.
                _page.AddReference(this);
                _page.ThumbnailChanged += HandlePage_ThumbnailChanged;
                _page.OrientationChanged += HandlePage_OrientationChanged;

                // Turn on anti-aliasing
                RasterSupport.Unlock(RasterSupportType.Document, _DOCUMENT_SUPPORT_KEY);
                RasterPaintProperties properties = _rasterPictureBox.PaintProperties;
                properties.PaintDisplayMode |= RasterPaintDisplayModeFlags.ScaleToGray;
                _rasterPictureBox.PaintProperties = properties;

                _toolTip.AutoPopDelay = 0;
                _toolTip.InitialDelay = 500;
                _toolTip.ReshowDelay = 500;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35474");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="OutputDocument"/> to which this instance belongs.
        /// </summary>
        /// <value>
        /// The <see cref="OutputDocument"/> to which this instance belongs.
        /// </value>
        public OutputDocument Document
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="Page"/> represented by this instance.
        /// </summary>
        public Page Page
        {
            get
            {
                return _page;
            }
        }

        /// <summary>
        /// The page number of this instance in the <see cref="Document"/> or zero if it does not
        /// currently part of any document.
        /// </summary>
        public int PageNumber
        {
            get
            {
                try
                {
                    return (Document == null) ? 0 : Document.PageControls.IndexOf(this) + 1;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35475");
                }
            }
        }

        /// <summary>
        /// Gets the size the thumbnail image should be.
        /// </summary>
        /// <value>
        /// The size the thumbnail image should be.
        /// </value>
        public static Size ThumbnailSize
        {
            get
            {
                return _thumbnailSize;
            }
        }

  
        /// <summary>
        /// Gets a value indicating whether this page is currently being displayed in an
        /// <see cref="ImageViewer"/>.
        /// </summary>
        /// <value><see langword="true"/> if page is currently being displayed; otherwise,
        /// <see langword="false"/>.</value>
        public bool PageIsDisplayed
        {
            get
            {
                return _activeImageViewer != null;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Displays or closed the <see cref="Page"/> in the specified <see paramref="ImageViewer"/>.
        /// </summary>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> that should diplay or close the
        /// page.</param>
        /// <param name="display"><see langword="true"/> to display the image;
        /// <see langword="false"/> to close it.</param>
        public void DisplayPage(ImageViewer imageViewer, bool display)
        {
            try
            {
                // Show the image only if this instance is currently in a PageLayoutControl.
                if (display && ParentForm != null)
                {
                    // Lock updating of this form while changing images to prevent flicker as the
                    // last image closes before this one is displayed.
                    using (new LockControlUpdates(ParentForm))
                    {
                        if (Page.OriginalDocumentName != imageViewer.ImageFile)
                        {
                            imageViewer.OpenImage(Page.OriginalDocumentName, false);
                        }
                        imageViewer.PageNumber = Page.OriginalPageNumber;
                        imageViewer.Orientation = -Page.ImageOrientation;

                        if (_activeImageViewer == null)
                        {
                            imageViewer.ImageChanged += HandleImageViewer_ImageChanged;
                            imageViewer.PageChanged += HandleImageViewer_PageChanged;
                            imageViewer.OrientationChanged += HandleActiveImageViewer_OrientationChanged;

                            _activeImageViewer = imageViewer;
                        }
                    }

                    ParentForm.Refresh();
                }
                // Close the image if specified.
                else if (!display && _activeImageViewer != null)
                {
                    _activeImageViewer.CloseImage();
                    _activeImageViewer.ImageChanged -= HandleImageViewer_ImageChanged;
                    _activeImageViewer.PageChanged -= HandleImageViewer_PageChanged;
                    _activeImageViewer.OrientationChanged -= HandleActiveImageViewer_OrientationChanged;
                    _activeImageViewer = null;

                    // Refresh _outerPanel to remove the indication that it is currently displayed.
                    _outerPanel.Invalidate();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35476");
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Gets or sets whether this control is selected.
        /// </summary>
        /// <value><see langword="true"/> if selected; otherwise, <see langword="false"/>.
        /// </value>
        public override bool Selected
        {
            get
            {
                return base.Selected;
            }

            set
            {
                if (value != base.Selected)
                {
                    base.Selected = value;

                    // Indicate selection with the BackColor of _outerPanel
                    _outerPanel.BackColor = value
                        ? SystemColors.ControlDark
                        : SystemColors.Control;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _rasterPictureBox.Image = _page.ThumbnailImage.Clone();

                SetToolTip(this);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35477");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.LocationChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLocationChanged(EventArgs e)
        {
            try
            {
                base.OnLocationChanged(e);

                CheckPadding();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35478");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Layout"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.LayoutEventArgs"/> that contains the
        /// event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            try
            {
                CheckPadding();

                base.OnLayout(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35479");
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if managed resources should be disposed;
        /// otherwise, <see langword="false"/>.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (_rasterPictureBox != null)
                    {
                        _rasterPictureBox.Dispose();
                        _rasterPictureBox = null;
                    }

                    if (_page != null)
                    {
                        _page.ThumbnailChanged -= HandlePage_ThumbnailChanged;
                        _page.OrientationChanged -= HandlePage_OrientationChanged;
                        _page.RemoveReference(this);
                        _page = null;
                    }

                    if (components != null)
                    {
                        components.Dispose();
                    }
                }
                catch (System.Exception ex)
                {
                    ex.ExtractLog("ELI35480");
                }
            }

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="T:Page.ThumbnailChanged"/> event of the <see cref="_page"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePage_ThumbnailChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateThumbnail();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35481");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Page.OrientationChanged"/> event of the <see cref="Page"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePage_OrientationChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateThumbnail();

                if (Document != null)
                {
                    Document.InOriginalForm = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35566");
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer.PageChanged"/> event of the <see cref="ImageViewer"/>
        /// control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.PageChangedEventArgs"/> instance
        /// containing the event data.</param>
        void HandleImageViewer_PageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                if (_activeImageViewer != null && e.PageNumber != _page.OriginalPageNumber)
                {
                    _activeImageViewer.ImageChanged -= HandleImageViewer_ImageChanged;
                    _activeImageViewer.PageChanged -= HandleImageViewer_PageChanged;
                    _activeImageViewer.OrientationChanged -= HandleActiveImageViewer_OrientationChanged;
                    _activeImageViewer = null;

                    // Refresh _outerPanel to remove the indication that it is currently displayed.
                    _outerPanel.Invalidate();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35482");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:ImageViewer.ImageChanged"/> event of the <see cref="ImageViewer"/>
        /// control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleImageViewer_ImageChanged(object sender, EventArgs e)
        {
            try
            {
                if (_activeImageViewer != null &&
                        !_activeImageViewer.ImageFile.Equals(
                            _page.OriginalDocumentName, StringComparison.OrdinalIgnoreCase))
                {
                    _activeImageViewer.ImageChanged -= HandleImageViewer_ImageChanged;
                    _activeImageViewer.PageChanged -= HandleImageViewer_PageChanged;
                    _activeImageViewer.OrientationChanged -= HandleActiveImageViewer_OrientationChanged;
                    _activeImageViewer = null;

                    // Refresh _outerPanel to remove the indication that it is currently displayed.
                    _outerPanel.Invalidate();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35483");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Paint"/> event of the <see cref="_rasterPictureBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance
        /// containing the event data.</param>
        void HandleRasterPictureBox_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (Page.MultipleCopiesExist)
                {
                    var brush = ExtractBrushes.GetSolidBrush(Color.SeaGreen);
                    e.Graphics.DrawString("*", _COPY_INDICATOR_FONT, brush, new Point (0, 0));
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35562");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Paint"/> event of the <see cref="_outerPanel"/> control
        /// in order to indicate the selection state.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance
        /// containing the event data.</param>
        void HandleOuterPanel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (_activeImageViewer != null)
                {
                    var highlightBrush = ExtractBrushes.GetSolidBrush(SystemColors.Highlight);
                    e.Graphics.FillRectangle(highlightBrush, _outerPanel.ClientRectangle);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35484");
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer.OrientationChanged"/> event of the
        /// <see cref="_activeImageViewer"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.OrientationChangedEventArgs"/>
        /// instance containing the event data.</param>
        void HandleActiveImageViewer_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            try
            {
                ExtractException.Assert("ELI35587", "Unexpected image viewer event registration",
                    _activeImageViewer != null);

                Page.ImageOrientation = _activeImageViewer.Orientation;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35564");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Sets the <see cref="_toolTip"/> to be active for all child controls.
        /// </summary>
        /// <param name="control"></param>
        void SetToolTip(Control control)
        {
            foreach (Control childControl in control.Controls)
            {
                _toolTip.SetToolTip(childControl, Page.OriginalDocumentName);

                SetToolTip(childControl);
            }
        }

        /// <summary>
        /// Ensures that the padding is such that it will cause all
        /// <see cref="PageThumbnailControl"/>s in the <see cref="PageLayoutControl"/> to align
        /// vertically whether or not they are preceeded by a <see cref="PaginationSeparator"/>
        /// control.
        /// </summary>
        void CheckPadding()
        {
            bool hasPaddingAllowance = Padding.Equals(_SEPARATOR_ALLOWANCE_PADDING);
            bool shouldHavePaddingAllowance =
                PreviousControl == null ||
                PreviousControl.Left > Left ||
                PreviousControl is PageThumbnailControl;

            if (hasPaddingAllowance != shouldHavePaddingAllowance)
            {
                Padding = shouldHavePaddingAllowance
                    ? _SEPARATOR_ALLOWANCE_PADDING
                    : _NORMAL_PADDING;
            }
        }

        /// <summary>
        /// Updates the thumbnail image.
        /// </summary>
        void UpdateThumbnail()
        {
            if (IsDisposed)
            {
                return;
            }

            // Since the thumbnail may be changed by a background thread and we don't want the work
            // of the background worker to be held up waiting on messages currently being
            // handled in the UI thread, invoke the image change to occur on the UI thread.
            this.SafeBeginInvoke("ELI35559", () =>
            {
                if (!IsDisposed)
                {
                    _rasterPictureBox.Image = _page.ThumbnailImage.Clone();
                    _rasterPictureBox.Invalidate();
                }
            });
        }

        #endregion Private Members
    }
}
