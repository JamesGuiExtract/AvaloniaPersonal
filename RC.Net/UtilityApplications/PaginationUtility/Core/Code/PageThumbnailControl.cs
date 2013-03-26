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
    /// 
    /// </summary>
    internal partial class PageThumbnailControl : PaginationControl
    {
        #region Constants

        /// <summary>
        /// Licensing key to unlock document (anti-aliasing) support
        /// </summary>
        static readonly string _DOCUMENT_SUPPORT_KEY = "vhG42tyuh9";
        
        /// <summary>
        /// 
        /// </summary>
        static readonly Padding _NORMAL_PADDING = new Padding(0, 1, 0, 1);

        /// <summary>
        /// 
        /// </summary>
        static readonly Padding _SEPARATOR_ALLOWANCE_PADDING =
            new Padding(PaginationSeparator._SEPARATOR_WIDTH, 1, 0, 1);

        #endregion Constants

        #region Fields

        /// <summary>
        /// 
        /// </summary>
        static Size _thumbnailSize = new Size(128, 128);

        /// <summary>
        /// 
        /// </summary>
        Page _page;

        /// <summary>
        /// 
        /// </summary>
        ImageViewer _activeImageViewer;

        /// <summary>
        /// 
        /// </summary>
        ToolTip _toolTip = new ToolTip();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PageThumbnailControl"/> class.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="page"></param>
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
                _fileNameLabel.Text = Path.GetFileNameWithoutExtension(_page.OriginalDocumentName);
                _pageNumberLabel.Text = string.Format(CultureInfo.CurrentCulture, "Page {0:D}",
                    page.OriginalPageNumber);

                if (Document != null)
                {
                    Document.AddPage(this);
                }
                page.AddReference(this);
                _page.ThumbnailChanged += HandlePage_ThumbnailChanged;

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
        /// Gets or sets the output document.
        /// </summary>
        /// <value>
        /// The output document.
        /// </value>
        public OutputDocument Document
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the size of the thumbnail.
        /// </summary>
        /// <value>
        /// The size of the thumbnail.
        /// </value>
        internal static Size ThumbnailSize
        {
            get
            {
                return _thumbnailSize;
            }
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        public Page Page
        {
            get
            {
                return _page;
            }
        }

        /// <summary>
        /// 
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
        /// Gets a value indicating whether [page is displayed].
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if [page is displayed]; otherwise, <see langword="false"/>.
        /// </value>
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
        /// 
        /// </summary>
        public void DisplayPage(ImageViewer imageViewer, bool display)
        {
            try
            {
                if (display && ParentForm != null)
                {
                    using (new LockControlUpdates(ParentForm))
                    {
                        if (Page.OriginalDocumentName != imageViewer.ImageFile)
                        {
                            imageViewer.OpenImage(Page.OriginalDocumentName, false);
                        }
                        imageViewer.PageNumber = Page.OriginalPageNumber;

                        imageViewer.ImageChanged += HandleImageViewer_ImageChanged;
                        imageViewer.PageChanged += HandleImageViewer_PageChanged;
                    }
                    _activeImageViewer = imageViewer;

                    ParentForm.Refresh();
                }
                else if (_activeImageViewer != null)
                {
                    _activeImageViewer.CloseImage();
                    _activeImageViewer = null;

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
        /// Gets or sets the selection area control.
        /// </summary>
        /// <value>
        /// The selection area control.
        /// </value>
        public override Control SelectionAreaControl
        {
            get
            {
                return _outerPanel;
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
        /// <param name="e">A <see cref="T:System.Windows.Forms.LayoutEventArgs"/> that contains the event data.</param>
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
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
        /// Handles the ThumbnailChanged event of the HandlePage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePage_ThumbnailChanged(object sender, EventArgs e)
        {
            try
            {
                if (IsDisposed)
                {
                    return;
                }

                FormsMethods.ExecuteInUIThread(this, () =>
                {
                    if (!IsDisposed)
                    {
                        _rasterPictureBox.Image = _page.ThumbnailImage.Clone();
                    }
                });
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35481");
            }
        }

        /// <summary>
        /// Handles the PageChanged event of the HandleImageViewer control.
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
                    _activeImageViewer = null;
                    _outerPanel.Invalidate();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35482");
            }
        }

        /// <summary>
        /// Handles the ImageChanged event of the HandleImageViewer control.
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
                    _activeImageViewer = null;
                    _outerPanel.Invalidate();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35483");
            }
        }

        /// <summary>
        /// Handles the Paint event of the HandleOuterPanel control.
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

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Registers for events.
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
        /// Checks the padding.
        /// </summary>
        void CheckPadding()
        {
            bool hasPaddingAllowance = Padding.Equals(_SEPARATOR_ALLOWANCE_PADDING);
            bool shouldHavePaddingAllowance =
                PreceedingControl == null ||
                PreceedingControl.Left > Left ||
                PreceedingControl is PageThumbnailControl;

            if (hasPaddingAllowance != shouldHavePaddingAllowance)
            {
                Padding = shouldHavePaddingAllowance
                    ? _SEPARATOR_ALLOWANCE_PADDING
                    : _NORMAL_PADDING;
            }
        }

        #endregion Private Members
    }
}
