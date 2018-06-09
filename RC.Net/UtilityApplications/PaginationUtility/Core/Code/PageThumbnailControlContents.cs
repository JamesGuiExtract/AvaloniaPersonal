﻿using Extract.Drawing;
using Extract.Imaging.Forms;
using Extract.Utilities.Forms;
using Leadtools;
using Leadtools.Drawing;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// The contents of a <see cref="PageThumbnailControl"/>. Created and disposed of dynamically as
    /// the owning <see cref="PageThumbnailControl"/> needs to be displayed or is scrolled out-of-view.
    /// </summary>
    internal partial class PageThumbnailControlContents : UserControl
    {
        #region Constants

        /// <summary>
        /// Licensing key to unlock document (anti-aliasing) support
        /// </summary>
        static readonly string _DOCUMENT_SUPPORT_KEY = "vhG42tyuh9";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="PageThumbnailControl"/> to which this instance belongs.
        /// </summary>
        PageThumbnailControl _pageControl;

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
        /// Keeps track of the last tooltip message displayed for this control.
        /// </summary>
        string _toolTipText;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PageThumbnailControlContents"/> class.
        /// </summary>
        public PageThumbnailControlContents()
            : base()
        {
            try
            {
                InitializeComponent();

                // Turn on anti-aliasing
                RasterSupport.Unlock(RasterSupportType.Document, _DOCUMENT_SUPPORT_KEY);
                RasterPaintProperties properties = _rasterPictureBox.PaintProperties;
                properties.PaintDisplayMode |= RasterPaintDisplayModeFlags.ScaleToGray;
                _rasterPictureBox.PaintProperties = properties;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43391");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageThumbnailControlContents"/> class.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> to which this instance
        /// belongs.</param>
        public PageThumbnailControlContents(PageThumbnailControl pageControl, Page page)
            : this()
        {
            try
            {
                _pageControl = pageControl;
                _page = page;

                // Set the labels based on the source document name and page number, not the output
                // Filename.
                _fileNameLabel.Text = Path.GetFileNameWithoutExtension(_page.OriginalDocumentName);
                _pageNumberLabel.Text = string.Format(CultureInfo.CurrentCulture, "Page {0:D}",
                    _page.OriginalPageNumber);

                _page.ThumbnailChanged += HandlePage_ThumbnailChanged;
                _page.OrientationChanged += HandlePage_OrientationChanged;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43374");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Occurs when the image associated with this control is closed or changed in the <see cref="ImageViewer"/>.
        /// </summary>
        public event EventHandler<EventArgs> ImageClosed;

        #endregion Events

        #region Methods

        /// <summary>
        /// Displays or closed the <see cref="Page"/> in the specified <see paramref="ImageViewer"/>.
        /// </summary>
        /// <param name="imageViewer">The <see cref="ImageViewer"/> that should display or close the
        /// page.</param>
        /// <param name="display"><see langword="true"/> to display the image;
        /// <see langword="false"/> to close it.</param>
        public bool DisplayPage(ImageViewer imageViewer, bool display)
        {
            try
            {
                bool imagePageChanged = false;

                if (display)
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
                        if (_page != null)
                        {
                            if (_page.OriginalDocumentName != imageViewer.ImageFile)
                            {
                                imagePageChanged = true;

                                imageViewer.OpenImage(_page.OriginalDocumentName, false, false);
                            }
                            if (imageViewer.PageNumber != _page.OriginalPageNumber)
                            {
                                imagePageChanged = true;

                                imageViewer.PageNumber = _page.OriginalPageNumber;
                            }
                            if (imageViewer.Orientation != _page.ImageOrientation)
                            {
                                imagePageChanged = true;

                                // Set the image viewer orientation to be the same value as the control.
                                // I can't figure out why it used to be the -Page.Orientation; when it was
                                // that way a control with a 90 or 270 degree orientation would cause the
                                // image to get flipped the second time it was selected (so that the image
                                // displayed in the full-sized image viewer would not match the thumbnail)
                                // https://extract.atlassian.net/browse/ISSUE-14208
                                imageViewer.Orientation = _page.ImageOrientation;
                            }

                            if (_activeImageViewer == null)
                            {
                                imageViewer.OrientationChanged += HandleActiveImageViewer_OrientationChanged;
                                imageViewer.ImageChanged += HandleImageViewer_ImageChanged;
                                imageViewer.PageChanged += HandleImageViewer_PageChanged;

                                _activeImageViewer = imageViewer;
                            }
                        }
                    }
                    finally
                    {
                        if (toolStripLocker != null)
                        {
                            toolStripLocker.Dispose();
                        }
                    }
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

                return imagePageChanged;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43375");
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

                ImageClosed?.Invoke(this, new EventArgs());
            }
        }

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
                string newToolTipText = _pageControl.PageStylists
                    .Select(stylist => stylist.GetToolTipText(_rasterPictureBox, mouseLocation))
                    .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
                newToolTipText = newToolTipText ?? _page.OriginalDocumentName;

                if (newToolTipText != _toolTipText)
                {
                    _toolTipText = newToolTipText;
                    SetToolTip(this, toolTip, newToolTipText);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI43376");
            }
        }

        /// <summary>
        /// Called when the <see cref="PaginationControl.Selected"/> property of the owning
        /// <see cref="PageThumbnailControl"/> has changed.
        /// </summary>
        public void OnSelectedStateChanged()
        {
            _borderPanel.Invalidate();
        }

        /// <summary>
        /// Called when the <see cref="PaginationControl.Highlighted"/> property of the owning
        /// <see cref="PageThumbnailControl"/> has changed.
        /// </summary>
        public void OnHighlightedStateChanged()
        {
            Invalidate(false);
        }

        /// <summary>
        /// Called when the <see cref="PageThumbnailControl.Deleted"/> property of the owning
        /// <see cref="PageThumbnailControl"/> has changed.
        /// </summary>
        public void OnDeletedStateChanged()
        {
            Invalidate();
        }

        #endregion Methods

        #region Overrides

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
                    if (!IsDisposed && _page != null && !_page.IsDisposed)
                    {
                        // RasterPictureBox does dispose of the old image.
                        _rasterPictureBox.Image = _page.ThumbnailImage;
                    }
                }
                catch (Exception ex)
                {
                    // To prevent any exceptions from being needlessly displayed at times user
                    // wouldn't otherwise know of a problem, just log exceptions setting the
                    // thumbnail image for now. Needs to be re-visited later.
                    ex.ExtractLog("ELI43377");
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI43378");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:Paint" /> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs" /> instance containing the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                if (_pageControl != null && _pageControl.Highlighted)
                {
                    var highlightColor = _pageControl.PageLayoutControl.IndicateFocus
                        ? ExtractColors.LightBlue
                        : SystemColors.ControlDark;
                    var highlightBrush = ExtractBrushes.GetSolidBrush(highlightColor);
                    e.Graphics.FillRectangle(highlightBrush, e.ClipRectangle);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43379");
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
                        _page.ThumbnailChanged -= HandlePage_ThumbnailChanged;
                        _page.OrientationChanged -= HandlePage_OrientationChanged;
                        _page = null;
                    }

                    if (_rasterPictureBox != null)
                    {
                        _rasterPictureBox.Dispose();
                        _rasterPictureBox = null;
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
                    ex.ExtractLog("ELI43380");
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
                ex.ExtractDisplay("ELI43381");
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
                ex.ExtractDisplay("ELI43382");
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
                if (_pageControl != null)
                {
                    var brush = ExtractBrushes.GetSolidBrush(
                        _pageControl.Selected && _pageControl.PageLayoutControl.IndicateFocus
                        ? SystemColors.ControlDark
                        : SystemColors.Control);

                    e.Graphics.FillRectangle(brush, e.ClipRectangle);

                    foreach (var stylist in _pageControl.PageStylists)
                    {
                        stylist.PaintBackground(e);
                    }
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43383");

                // https://extract.atlassian.net/browse/ISSUE-14778
                // I have been unable to reproduce a GDI related exception here and unable to find
                // via research a fix I believe would prevent the error. Since painting the border
                // is not a critical operation, just log any GDI exceptions.
                if (ee.Message.Contains("GDI"))
                {
                    ee.Log();
                }
                else
                {
                    ee.Display();
                }
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
                if (_pageControl != null)
                {
                    foreach (var stylist in _pageControl.PageStylists)
                    {
                        stylist.PaintForeground(_rasterPictureBox, e);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI43384");
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
                ExtractException.Assert("ELI43385", "Unexpected image viewer event registration",
                    _activeImageViewer != null);

                _page.ImageOrientation = _activeImageViewer.Orientation;
                _pageControl.OnPageStateChanged();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI43386");
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
                ex.ExtractDisplay("ELI43387");
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
                ex.ExtractDisplay("ELI43388");
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

            var thumbnail = _page.ThumbnailImage;

            // Since the thumbnail may be changed by a background thread and we don't want the work
            // of the background worker to be held up waiting on messages currently being
            // handled in the UI thread, invoke the image change to occur on the UI thread.
            this.SafeBeginInvoke("ELI43389", () =>
            {
                // Ensure this control has not been disposed of since invoking the thumbnail change.
                if (!IsDisposed && _page != null && !_page.IsDisposed)
                {
                    // RasterPictureBox does dispose of the old image.
                    _rasterPictureBox.Image = thumbnail;
                    _rasterPictureBox.Invalidate();
                }
            });
        }

        #endregion Private Members
    }
}