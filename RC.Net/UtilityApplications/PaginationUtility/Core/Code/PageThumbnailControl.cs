using Extract.Drawing;
using Extract.Imaging.Forms;
using Extract.Utilities.Forms;
using Leadtools;
using Leadtools.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A <see cref="PaginationControl"/> that represents a page in a potential output document with
    /// a thumbnail image of the page.
    /// </summary>
    internal partial class PageThumbnailControl : NavigablePaginationControl
    {
        #region Constants

        /// <summary>
        /// Licensing key to unlock document (anti-aliasing) support
        /// </summary>
        static readonly string _DOCUMENT_SUPPORT_KEY = "vhG42tyuh9";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="Size"/> the thumbnail image should be.
        /// </summary>
        static Size _thumbnailSize = new Size(128, 128);

        /// <summary>
        /// The overall <see cref="Size"/> all <see cref="NavigablePaginationControl"/>s should be.
        /// </summary>
        static Size? _uniformSize;

        /// <summary>
        /// Indicates the normal padding that should be used for an instance of this class.
        /// </summary>
        static Padding? _normalPadding;

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
        /// The <see cref="PageStylist"/>s being used to apply styles to this instance.
        /// </summary>
        HashSet<PageStylist> _pageStylists = new HashSet<PageStylist>();

        /// <summary>
        /// Keeps track of the last tooltip message displayed for this control.
        /// </summary>
        string _toolTipText;

        /// <summary>
        /// Indicates whether the page should be considered deleted.
        /// </summary>
        bool _deleted;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="PageThumbnailControl"/> class from being
        /// created from an outside caller.
        /// </summary>
        PageThumbnailControl()
            : base()
        {
            try
            {
                InitializeComponent();

                AddStylist(new CopiedPageStylist(this), replaceExistingTypeInstances: true);
                AddStylist(new DeletedPageStylist(this), replaceExistingTypeInstances: true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35651");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageThumbnailControl"/> class.
        /// </summary>
        /// <param name="document">The <see cref="OutputDocument"/> to which this instance should be
        /// added.</param>
        /// <param name="page">The <see cref="Page"/> represented by this instance.</param>
        public PageThumbnailControl(OutputDocument document, Page page)
            : this()
        {
            try
            {
                ExtractException.Assert("ELI35473", "Null argument exception.", page != null);

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
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35474");
            }
        }

        #endregion Constructors

        #region Static Members

        /// <summary>
        /// Gets the overall <see cref="Size"/> all <see cref="NavigablePaginationControl"/>s should
        /// be.
        /// </summary>
        /// <value>
        /// The overall <see cref="Size"/> all <see cref="NavigablePaginationControl"/>s should be.
        /// </value>
        public static Size UniformSize
        {
            get
            {
                try
                {
                    if (_uniformSize == null)
                    {
                        using (var pageControl = new PageThumbnailControl())
                        {
                            _uniformSize = pageControl.GetPreferredSize(Size.Empty);
                        }
                    }

                    return _uniformSize.Value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35654");
                }
            }
        }

        #endregion Static Members

        #region Events

        /// <summary>
        /// Raised to indicate the state of <see cref="Deleted"/> has changed.
        /// </summary>
        public event EventHandler<EventArgs> DeletedStateChanged;

        #endregion Events

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
        /// Gets the zero-based page index of the page within its current document or -1 if it is
        /// not currently part of any document.
        /// <para><b>Note</b></para>
        /// The page index includes pages where <see cref="Deleted"/> is <see langword="true"/>.
        /// </summary>
        public int DocumentPageIndex
        {
            get
            {
                return (Document == null)
                    ? -1
                    : Document.PageControls.IndexOf(this);
            }
        }

        /// <summary>
        /// The page number of this instance in the <see cref="Document"/> or zero if it is not
        /// currently part of any document.
        /// <para><b>Note</b></para>
        /// The page number does not include pages where <see cref="Deleted"/> is
        /// <see langword="true"/>.
        /// </summary>
        public int PageNumber
        {
            get
            {
                try
                {
                    return (Document == null || Deleted)
                        ? 0
                        : Document.PageControls
                            .Where(c => !c.Deleted)
                            .ToList()
                            .IndexOf(this) + 1;
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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is highlighted.
        /// </summary>
        /// <value><see langword="true"/> if this instance is highlighted; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public override bool Highlighted
        {
            get
            {
                return base.Highlighted;
            }
            set
            {
                base.Highlighted = value;

                // Refresh _outerPanel to update the indication of whether it is currently the
                // primary selection.
                _outerPanel.Invalidate(false);
            }
        }

        /// <summary>
        /// Gets the normal <see cref="Padding"/> that should be used by this instance.
        /// </summary>
        /// <value>The normal <see cref="Padding"/> that should be used by this instance.
        /// </value>
        public override Padding NormalPadding
        {
            get
            {
                try
                {
                    if (_normalPadding == null)
                    {
                        using (var pageControl = new PageThumbnailControl())
                        {
                            _normalPadding = pageControl.Padding;
                        }
                    }

                    return _normalPadding.Value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35664");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PageThumbnailControl"/> should
        /// be considered deleted.
        /// </summary>
        /// <value><see langword="true"/> if deleted; otherwise, <see langword="false"/>.
        /// </value>
        public bool Deleted
        {
            get
            {
                return _deleted;
            }

            set
            {
                try
                {
                    if (value != _deleted)
                    {
                        _deleted = value;
                        Invalidate();

                        OnDeletedStateChanged();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40048");
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Associates the specified <see paramref="toolTip"/> with this control.
        /// </summary>
        /// <param name="toolTip">The <see cref="ToolTip"/> to associate with this control.</param>
        public void SetToolTip(ToolTip toolTip)
        {
            try
            {
                // Get the location of the mouse relative to the _rasterPictureBox (the surface to
                // which stylists draw).
                var mouseLocation = _rasterPictureBox.PointToClient(Control.MousePosition);

                // Check if any of the stylists drawing at this position have their own tooltip text
                // to use.
                string newToolTipText = _pageStylists
                    .Select(stylist => stylist.GetToolTipText(_rasterPictureBox, mouseLocation))
                    .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
                    newToolTipText = newToolTipText ?? Page.OriginalDocumentName;

                if (newToolTipText != _toolTipText)
                {
                    _toolTipText = newToolTipText;
                    SetToolTip(this, toolTip, newToolTipText);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35666");
            }
        }

        /// <summary>
        /// Displays or closed the <see cref="Page"/> in the specified <see paramref="ImageViewer"/>.
        /// </summary>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> that should display or close the
        /// page.</param>
        /// <param name="display"><see langword="true"/> to display the image;
        /// <see langword="false"/> to close it.</param>
        public void DisplayPage(ImageViewer imageViewer, bool display)
        {
            try
            {
                if (!PageLayoutControl.EnablePageDisplay)
                {
                    return;
                }

                // Show the image only if this instance is currently in a PageLayoutControl.
                if (display && ParentForm != null)
                {
                    // To prevent flicker of the image viewer tool strips while loading a new image,
                    // if we can find a parent ToolStripContainer, lock it until the new image is
                    // loaded.
                    // [DotNetRCAndUtils:931]
                    // This, and the addition of a parameter on OpenImage to prevent an initial
                    // refresh is in place of locking the entire form which can cause the form to
                    // fall behind other open applications when clicked.
                    LockControlUpdates toolStripLocker = null;
                    for (Control control = imageViewer; control != null; control = control.Parent)
                    {
                        var toolStripContainer = control as ToolStripContainer;
                        if (toolStripContainer != null)
                        {
                            toolStripLocker = new LockControlUpdates(toolStripContainer);
                            break;
                        }
                    }

                    try
                    {
                        if (Page.OriginalDocumentName != imageViewer.ImageFile)
                        {
                            imageViewer.OpenImage(Page.OriginalDocumentName, false, false);
                        }
                        imageViewer.PageNumber = Page.OriginalPageNumber;
                        imageViewer.Orientation = -Page.ImageOrientation;

                        if (_activeImageViewer == null)
                        {
                            imageViewer.OrientationChanged += HandleActiveImageViewer_OrientationChanged;
                            imageViewer.ImageChanged += HandleImageViewer_ImageChanged;
                            imageViewer.PageChanged += HandleImageViewer_PageChanged;

                            _activeImageViewer = imageViewer;
                        }
                    }
                    finally
                    {
                        if (toolStripLocker != null)
                        {
                            toolStripLocker.Dispose();
                        }
                    }

                    ParentForm.Refresh();
                }
                // Close the image if specified.
                else if (!display && _activeImageViewer != null)
                {
                    // [DotNetRCAndUtils:956]
                    // Do not unload the image, otherwise it may be deleted or modified by an
                    // outside application while still available in this UI.
                    _activeImageViewer.CloseImage(false);
                    DeactivateImageViewer();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35476");
            }
        }

        /// <summary>
        /// Deactivates <see cref="_activeImageViewer"/> when <see cref="Page"/> is no longer being
        /// displayed.
        /// </summary>
        public void DeactivateImageViewer()
        {
            if (_activeImageViewer != null)
            {
                _activeImageViewer.OrientationChanged -= HandleActiveImageViewer_OrientationChanged;
                _activeImageViewer.ImageChanged -= HandleImageViewer_ImageChanged;
                _activeImageViewer.PageChanged -= HandleImageViewer_PageChanged;
                _activeImageViewer = null;
            }
        }

        /// <summary>
        /// Adds <see paramref="stylist"/> to the set of <see cref="PageStylist"/>s being used by
        /// this instance.
        /// </summary>
        /// <param name="stylist">The <see cref="PageStylist"/> to be used by this instance.</param>
        /// <param name="replaceExistingTypeInstances"></param>
        public void AddStylist(PageStylist stylist, bool replaceExistingTypeInstances)
        {
            try
            {
                if (replaceExistingTypeInstances)
                {
                    _pageStylists.RemoveWhere(s => s.GetType() == stylist.GetType());
                }

                _pageStylists.Add(stylist);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40004");
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

                    _borderPanel.Invalidate();
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

                try
                {
                    // [DotNetRCAndUtils:959]
                    // For reasons I don't understand, the dispose of one PageThumbnailControl can
                    // sometimes trigger the OnLoad call of a subsequent PageThumbnailControl.
                    // Ensure the page thumbnail exists before trying to use it.
                    if (!IsDisposed && _page != null && _page.ThumbnailImage != null)
                    {
                        _rasterPictureBox.Image = _page.ThumbnailImage.Clone();
                    }
                }
                catch (Exception ex)
                {
                    // To prevent any exceptions from being needlessly displayed at times user
                    // wouldn't otherwise know of a problem, just log exceptions setting the
                    // thumbnail image for now. Needs to be re-visited later.
                    ex.ExtractLog("ELI35668");
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35477");
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

                    if (_activeImageViewer != null)
                    {
                        _activeImageViewer.OrientationChanged -= HandleActiveImageViewer_OrientationChanged;
                        _activeImageViewer = null;
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
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35566");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Paint"/> event of the <see cref="_borderPanel"/> control
        /// in order to indicate the selection state.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance
        /// containing the event data.</param>
        void HandleBorderPanel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                var brush = ExtractBrushes.GetSolidBrush(Selected
                    ? SystemColors.ControlDark
                    : SystemColors.Control);

                e.Graphics.FillRectangle(brush, e.ClipRectangle);

                foreach (var stylist in _pageStylists)
                {
                    stylist.PaintBackground(e);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39634");
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
                foreach (var stylist in _pageStylists)
                {
                    stylist.PaintForeground(_rasterPictureBox, e);
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
                if (Highlighted)
                {
                    var highlightBrush = ExtractBrushes.GetSolidBrush(SystemColors.Highlight);
                    e.Graphics.FillRectangle(highlightBrush, e.ClipRectangle);
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

        /// <summary>
        /// Handles the <see cref="T:ImageViewer.ImageChanged"/> event of the
        /// <see cref="_activeImageViewer"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleImageViewer_ImageChanged(object sender, EventArgs e)
        {
            try
            {
                // [DotNetRCAndUtils:1039]
                // If the page has changed, the displayed page is no longer the one associated with
                // this instance; don't allow rotation of the displayed page to affect the rotation
                // of this page.
                DeactivateImageViewer();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35881");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:ImageViewer.PageChanged"/> event of the
        /// <see cref="_activeImageViewer"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.PageChangedEventArgs"/> instance
        /// containing the event data.</param>
        void HandleImageViewer_PageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                // [DotNetRCAndUtils:1039]
                // If the page has changed, the displayed page is no longer the one associated with
                // this instance; don't allow rotation of the displayed page to affect the rotation
                // of this page.
                DeactivateImageViewer();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35882");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Recursively associates the specified <see paramref="toolTip"/> and
        /// <see paramref="toolTipText"/> with the specified <see paramref="control"/>.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to which the tooltip should be applied.
        /// </param>
        /// <param name="toolTip">The <see cref="ToolTip"/> to apply.</param>
        /// <param name="toolTipText">The tooltip text to apply.</param>
        void SetToolTip(Control control, ToolTip toolTip, string toolTipText)
        {
            toolTip.SetToolTip(control, toolTipText);

            foreach (Control childControl in control.Controls)
            {
                SetToolTip(childControl, toolTip, toolTipText);
            }
        }

        /// <summary>
        /// Updates the thumbnail image.
        /// </summary>
        void UpdateThumbnail()
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            // Since the thumbnail may be changed by a background thread and we don't want the work
            // of the background worker to be held up waiting on messages currently being
            // handled in the UI thread, invoke the image change to occur on the UI thread.
            this.SafeBeginInvoke("ELI35559", () =>
            {
                // Ensure this control has not been disposed of since invoking the thumbnail change.
                if (!IsDisposed && _page != null && !_page.IsDisposed)
                {
                    _rasterPictureBox.Image = _page.ThumbnailImage.Clone();
                    _rasterPictureBox.Invalidate();
                }
            });
        }

        /// <summary>
        /// Raises the <see cref="DeletedStateChanged"/> event.
        /// </summary>
        void OnDeletedStateChanged()
        {
            var eventHandler = DeletedStateChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}