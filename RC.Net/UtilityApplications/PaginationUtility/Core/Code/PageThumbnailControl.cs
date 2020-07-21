using Extract.Imaging.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        /// The panel that contains all elements to be displayed to users. To improve on user object
        /// usage when displaying a lot of documents or pages, only those page controls which are
        /// currently visible will have _contentsPanel instantiated.
        /// </summary>
        PageThumbnailControlContents _contentsPanel;

        /// <summary>
        /// The <see cref="Page"/> represented by this instance.
        /// </summary>
        Page _page;

        /// <summary>
        /// The <see cref="PageStylist"/>s being used to apply styles to this instance.
        /// </summary>
        HashSet<PageStylist> _pageStylists = new HashSet<PageStylist>();

        /// <summary>
        /// Indicates whether the page should be considered deleted.
        /// </summary>
        bool _deleted;

        /// <summary>
        /// Indicates whether the page associated with this control is currently displayed in the
        /// <see cref="ImageViewer"/>.
        /// </summary>
        bool _pageIsDisplayed;

        /// <summary>
        /// Prevents recursion in SetContents()
        /// </summary>
        bool _updatingContents;

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
                AddStylist(new RotatedPageStylist(this), replaceExistingTypeInstances: true);
                AddStylist(new ProcessedPageStylist(this), replaceExistingTypeInstances: true);
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

                if (Document != null)
                {
                    Document.AddPage(this);
                }

                // Add a reference to the Page so that the source document is not deleted before this
                // instance is.
                _page.AddReference(this);
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
                            _uniformSize = pageControl.Size;
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
        /// Raised to indicate the state of <see cref="Deleted"/> has changed or the page
        /// orientation has changed.
        /// </summary>
        public event EventHandler<EventArgs> PageStateChanged;

        /// <summary>
        /// Raised when the <see cref="Selected"/> property changes.
        /// </summary>
        public event EventHandler<EventArgs> SelectedStateChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="OutputDocument"/> to which this instance belongs.
        /// NOTE: This replaces the base class member to allow the property to be writable.
        /// </summary>
        /// <value>
        /// The <see cref="OutputDocument"/> to which this instance belongs.
        /// </value>
        public new OutputDocument Document
        {
            get
            {
                return base.Document;
            }

            set
            {
                base.Document = value;
            }
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
                return _pageIsDisplayed;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this page has been displayed in the ImageViewer (via the
        /// pagination tab)
        public bool Viewed
        {
            get;
            set;
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

                _contentsPanel?.OnHighlightedStateChanged();
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

                        OnPageStateChanged();

                        _contentsPanel?.OnDeletedStateChanged();
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
                _contentsPanel?.SetToolTip(toolTip);
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
                if (PageLayoutControl == null || !PageLayoutControl.EnablePageDisplay)
                {
                    return;
                }

                _pageIsDisplayed = display;

                if (display && !Viewed)
                {
                    Viewed = true;
                    OnPageStateChanged();
                }

                if (_contentsPanel == null)
                {
                    // https://extract.atlassian.net/browse/ISSUE-14808
                    // Contents panel needs to be created to display a page if requested. It is not
                    // okay to simply not show the page as had been the case after the initial 10.6
                    // re-factor.
                    if (display)
                    {
                        SetContents(forceCreation: true);
                    }
                    else
                    {
                        return;
                    }
                }

                // Show the image only if this instance is currently in a PageLayoutControl.
                bool imagePageChanged = _contentsPanel.DisplayPage(imageViewer, display && ParentForm != null);

                // Expensive refreshes of the entire form should only be performed if the page was
                // changed.
                if (imagePageChanged)
                {
                    ParentForm.Refresh();
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
            _contentsPanel?.DeactivateImageViewer();
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
        /// Raises the <see cref="E:System.Windows.Forms.Control.Layout" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.LayoutEventArgs" /> that contains the
        /// event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            try
            {
                // Create or dispose of _contentsPanel depending on current visibility.
                SetContents();

                base.OnLayout(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI43372");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the
        /// event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                // Create or dispose of _contentsPanel depending on current visibility.
                SetContents();

                base.OnPaint(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI43373");
            }
        }

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

                    OnSelectedStateChanged();
                }
            }
        }

        /// <summary>
        /// Registers to receive key events from child controls that should be raised as if
        /// they are coming from this control.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> whose children's events should be
        /// forwarded.</param>
        protected override void RegisterForEvents(Control control)
        {
            try
            {
                // https://extract.atlassian.net/browse/ISSUE-14874
                // If _contentsPanel is defined, it's events will have already been registered.
                if (control != _contentsPanel)
                {
                    base.RegisterForEvents(control);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44706");
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
                    if (_page != null)
                    {
                        _page.RemoveReference(this);
                        _page = null;
                    }

                    if (_contentsPanel != null)
                    {
                        var contentsPanel = _contentsPanel;
                        UnRegisterForEvents(contentsPanel);
                        Controls.Remove(contentsPanel);
                        contentsPanel.Dispose();
                        _contentsPanel = null;
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
        /// Handles the ImageClosed event of the HandleContentsPanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleContentsPanel_ImageClosed(object sender, EventArgs e)
        {
            _pageIsDisplayed = false;
        }

        #endregion Event Handlers

        /// <summary>
        /// Gets the page stylists.
        /// </summary>
        /// <value>
        /// The page stylists.
        /// </value>
        internal HashSet<PageStylist> PageStylists
        {
            get
            {
                return _pageStylists;
            }
        }

        #region Private Members

        /// <summary>
        /// Creates or disposes of <see cref="_contentsPanel"/> depending on current visibility of
        /// this control.
        /// </summary>
        /// <param name="forceCreation"><c>true</c> if the contents should be created even if the
        /// page is not currently visible.</param>
        void SetContents(bool forceCreation = false)
        {
            if (_updatingContents)
            {
                return;
            }

            try
            {
                _updatingContents = true;

                bool visible = forceCreation ||
                    (Parent != null &&
                     Visible &&
                     Parent.ClientRectangle.IntersectsWith(Bounds));

                if (visible && _contentsPanel == null)
                {
                    var contentsPanel = new PageThumbnailControlContents(this, _page);

                    Controls.Add(contentsPanel);
                    contentsPanel.Invalidate(true);

                    RegisterForEvents(contentsPanel);
                    contentsPanel.ImageClosed += HandleContentsPanel_ImageClosed;

                    _contentsPanel = contentsPanel;
                }
                // Don't dispose of the ContentsPanel when it is no longer visible because
                // it is responsible for painting the deleted page overlay
                // https://extract.atlassian.net/browse/ISSUE-17104
            }
            finally
            {
                _updatingContents = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="PageStateChanged"/> event.
        /// </summary>
        internal void OnPageStateChanged()
        {
            var eventHandler = PageStateChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        internal void OnSelectedStateChanged()
        {
            _contentsPanel?.OnSelectedStateChanged();

            var eventHandler = SelectedStateChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}