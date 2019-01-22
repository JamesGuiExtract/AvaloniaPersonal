using Extract.Imaging.Utilities;
using Extract.Licensing;
using Leadtools;
using Leadtools.Drawing;
using Leadtools.ImageProcessing.Color;
using Leadtools.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a control that displays thumbnails for the pages of an image.
    /// </summary>
    public partial class ThumbnailViewer : UserControl, IImageViewerControl
    {
        #region Constants

        /// <summary>
        /// The default image to use for an image that is not yet loaded.
        /// </summary>
        internal static readonly RasterImage _LOADING_IMAGE = GetLoadingImage();

        /// <summary>
        /// The image to use for an image that failed to load properly.
        /// </summary>
        internal static readonly RasterImage _ERROR_IMAGE = GetErrorImage();

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ThumbnailViewer).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The image viewer associated with the <see cref="ThumbnailViewer"/>.
        /// </summary>
        IDocumentViewer _imageViewer;

        /// <summary>
        /// Loads the pages of an image in a separate thread.
        /// </summary>
        ThumbnailWorker _worker;

        /// <summary>
        /// Indicates whether the thumbnail viewer should load thumbnails.
        /// </summary>
        bool _active = true;

        /// <summary>
        /// Indicates whether the colors of the thumbnails are currently inverted.
        /// </summary>
        bool _invertColors;

        /// <summary>
        /// The current page number of the <see cref="ImageViewer"/>
        /// </summary>
        int _currentPage;

        /// <summary>
        /// Maintain the original view perspectives for each thumbnail that is loaded so that
        /// orientation can be applied in an "absolute" way rather than a relative to the current
        /// thumbnail orientation.
        /// </summary>
        List<RasterViewPerspective> _originalViewPerspectives = new List<RasterViewPerspective>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ThumbnailViewer"/> class.
        /// </summary>
        public ThumbnailViewer()
        {
            InitializeComponent();

            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28488",
                _OBJECT_NAME);

            // Turn on anti-aliasing

            // Unlock the document support toolkit
            UnlockLeadtools.UnlockDocumentSupport(false);

            RasterPaintProperties properties = _imageList.PaintProperties;
            properties.PaintDisplayMode |= RasterPaintDisplayModeFlags.ScaleToGray;
            _imageList.PaintProperties = properties;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Indicates whether the thumbnail viewer should load thumbnails.
        /// </summary>
        /// <value><see langword="true"/> if the thumbnails should be loaded;
        /// <see langword="false"/> otherwise.</value>
        [DefaultValue(true)]
        public bool Active
        {
            get
            {
                return _active;
            }

            set
            {
                try
                {
                    if (value != _active)
                    {
                        _active = value;

                        if (_active && _imageViewer != null && _imageViewer.IsImageAvailable)
                        {
                            // If _imageList hasn't been initialized, do it now.
                            if (_imageList == null || _imageList.Items.Count == 0)
                            {
                                LoadDefaultThumbnails();
                            }

                            // Kick off or un-pause a worker thread.
                            StartThumbnailWorker();

                            // Ensure the proper thumbnail is selected after re-activating.
                            SelectPage(_currentPage);
                        }
                        else if (!_active && _worker != null && _worker.IsRunning)
                        {
                            // If deactivated, keep the worker thread around, but pause it.
                            PauseThumbnailWorker();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30743", ex);
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the default image to use for thumbnails that have not yet been loaded.
        /// </summary>
        /// <returns>The default image to use for thumbnails that have not yet been loaded.
        /// </returns>
        static RasterImage GetLoadingImage()
        {
            using (Image image = new Bitmap(typeof(ThumbnailViewer), "Resources.Loading.png"))
            {
                return RasterImageConverter.ConvertFromImage(image, ConvertFromImageOptions.None);
            }
        }

        /// <summary>
        /// Gets the image to use for thumbnails that failed to load properly.
        /// </summary>
        /// <returns>The default image to use for thumbnails that failed to load properly.
        /// </returns>
        static RasterImage GetErrorImage()
        {
            using (Image image = new Bitmap(typeof(ThumbnailViewer), "Resources.Error.png"))
            {
                return RasterImageConverter.ConvertFromImage(image, ConvertFromImageOptions.None);
            }
        }


        /// <summary>
        /// Populates the image list with default loading image for each page and triggers the 
        /// worker thread to load page thumnails for the currently open image.
        /// </summary>
        void UpdateImageList()
        {
            _imageList.BeginUpdate();

            Clear();

            if (Active && _imageViewer.IsImageAvailable)
            {
                LoadDefaultThumbnails();

                StartThumbnailWorker();
            }

            _imageList.EndUpdate();
        }

        /// <summary>
        /// Clears and disposes of all thumbnails.
        /// </summary>
        void Clear()
        {
            // Stop and dispose of the worker thread
            StopThumbnailWorker();

            // Dispose and clear the thumbnails
            DisposeThumbnails();
        }

        /// <summary>
        /// Disposes of all the image thumbnails.
        /// </summary>
        void DisposeThumbnails()
        {
            if (_imageList != null)
            {
                RasterImageListItemCollection items = _imageList.Items;
                if (items != null && items.Count > 0)
                {
                    foreach (RasterImageListItem item in items)
                    {
                        if (item != null)
                        {
                            RasterImage image = item.Image;
                            if (image != null && image != _LOADING_IMAGE && image != _ERROR_IMAGE)
                            {
                                image.Dispose();
                            }
                        }
                    }
                    items.Clear();
                }
            }
        }

        /// <summary>
        /// Loads the default image for all the thumbnails.
        /// </summary>
        void LoadDefaultThumbnails()
        {
            // Store the original view perspective for later reference when rotating thumbnails.
            _originalViewPerspectives =
                Enumerable.Range(0, _imageViewer.PageCount)
                .Select(i => new RasterViewPerspective())
                .ToList();

            // Create a default entry for each page
            for (int i = 1; i <= _imageViewer.PageCount; i++)
            {
                string text = "Page " + i.ToString(CultureInfo.CurrentCulture);
                RasterImageListItem listItem = new RasterImageListItem(_LOADING_IMAGE, 1, text);
                listItem.Tag = i;

                _imageList.Items.Add(listItem);
            }
        }

        /// <summary>
        /// Creates and starts a worker thread to load the images.
        /// </summary>
        void StartThumbnailWorker()
        {
            if (_worker == null)
            {
                _worker = new ThumbnailWorker(_imageViewer.ImageFile, _imageList.ItemImageSize, false);

                _worker.BeginLoading();
            }
            else
            {
                _worker.Paused = false;
            }

            _timer.Start();
        }

        /// <summary>
        /// Pauses the worker thread if it exists.
        /// </summary>
        void PauseThumbnailWorker()
        {
            _timer.Stop();

            if (_worker != null && _worker.IsRunning)
            {
                _worker.Paused = true;
            }
        }

        /// <summary>
        /// Stops and destroys the worker thread.
        /// </summary>
        void StopThumbnailWorker()
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            if (_worker != null)
            {
                _worker.Dispose();
                _worker = null;
            }
        }

        /// <summary>
        /// Updates any visible thumbnails that are displaying the default image with the actual 
        /// thumbnail image loaded from the <see cref="ThumbnailWorker"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the <see cref="ThumbnailWorker"/> has loaded all 
        /// the visible pages for the specified</returns>
        bool UpdateThumbnailsFromWorker(int startIndex, int endIndex)
        {
            bool success = true;
            bool listUpdated = false;
            for (int i = startIndex; i <= endIndex; i++)
            {
                RasterImageListItem item = _imageList.Items[i];
                if (item.Image == _LOADING_IMAGE)
                {
                    RasterImage image = _worker.GetThumbnail(i + 1);
                    if (image == null)
                    {
                        success = false;
                    }
                    else
                    {
                        item.Image = image;

                        // Don't  invert or rotate the special loading/error images.
                        if (image != _LOADING_IMAGE && image != _ERROR_IMAGE)
                        {
                            // The thumbnail viewer should respect the InvertColors property of the
                            // ImageViewer.
                            if (ImageViewer.InvertColors)
                            {
                                var invertCommand = new InvertCommand();
                                invertCommand.Run(item.Image);
                            }

                            _originalViewPerspectives[i] = item.Image.ViewPerspective;

                            if (ImageViewer.IsImageAvailable && (ImageViewer.ImagePageData?.Count ?? 0) > i)
                            {
                                var pageData = ImageViewer.ImagePageData[i];
                                item.Image.RotateViewPerspective(pageData.Orientation);
                            }
                        }
                        listUpdated = true;
                        item.Invalidate();
                    }
                }
            }
            if (listUpdated)
            {
                _imageList.Invalidate();
            }

            return success;
        }

        /// <summary>
        /// Gets index of the last visible thumbnail.
        /// </summary>
        /// <returns>The index of the last visible thumbnails.</returns>
        int GetBottomIndex()
        {
            return Math.Min(_imageList.TopIndex + GetVisibleCells(), _imageList.Items.Count) - 1;
        }

        /// <summary>
        /// Gets the number of visible cells. This includes cells that do not contain any image.
        /// </summary>
        /// <returns>The number of visible cells. This includes cells that do not contain any 
        /// image.</returns>
        int GetVisibleCells()
        {
            return _imageList.VisibleRows * _imageList.VisibleColumns;
        }

        /// <summary>
        /// Clears all selected thumbnails.
        /// </summary>
        void ClearSelection()
        {
            foreach (RasterImageListItem selectedItem in _imageList.SelectedItems)
            {
                selectedItem.Selected = false;
                selectedItem.Invalidate();
            }
        }

        /// <summary>
        /// Selects the thumbnail representing the specified image page.
        /// </summary>
        /// <param name="pageNumber">The page number to select.</param>
        void SelectPage(int pageNumber)
        {

            if (Active && _imageViewer != null && _imageViewer.IsImageAvailable)
            {
                // If _imageList hasn't been initialized, do it now.
                // https://extract.atlassian.net/browse/ISSUE-14317
                if (_imageList == null || _imageList.Items.Count == 0)
                {
                    LoadDefaultThumbnails();

                    // Kick off or un-pause a worker thread.
                    StartThumbnailWorker();
                }

                // Select the thumbnail corresponding to this page
                RasterImageListItem item = _imageList.Items[pageNumber - 1];
                if (!item.Selected)
                {
                    // Ensure the item to select is visible
                    _imageList.EnsureVisible(pageNumber - 1);

                    // Clear the current selection
                    ClearSelection();

                    // Select this item
                    item.Selected = true;
                    
                    _imageList.Invalidate();
                }
            }
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.DocumentViewer.ImageFileChanged"/> event.</param>
        void HandleImageViewerImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                UpdateImageList();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27918", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Forms.DocumentViewer.PageChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Forms.DocumentViewer.PageChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Forms.DocumentViewer.PageChanged"/> event.</param>
        void HandleImageViewerPageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                // Keep track of the current page, even if the thumbnail viewer isn't currently
                // active. That way the proper page can be selected if it is re-activated.
                _currentPage = e.PageNumber;

                if (Active)
                {
                    SelectPage(_currentPage);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27919", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="E:ImageViewer.InvertColorsStatusChanged"/> event of the
        /// <see cref="_imageViewer"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleImageViewerInvertColorsStatusChanged(object sender, EventArgs e)
        {
            try
            {
                // The thumbnail viewer should keep it's color inversion state in sync with
                // the ImageViewer InvertColors property.
                InvertColors = _imageViewer.InvertColors;
            }
            catch (Exception ex)
            {
                // This is intentionally not a displayed exception since this event is not directly
                // raised by a user action.
                throw ex.AsExtract("ELI36800");
            }
        }

        /// <summary>
        /// Handles the <see cref="E:ImageViewer.NonDisplayedPageOrientationChanged"/> and
        /// <see cref="E:ImageViewer.OrientationChanged"/> events of the <see cref="_imageViewer"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.OrientationChangedEventArgs"/>
        /// instance containing the event data.</param>
        void HandleImageViewerOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            try
            {
                int index = e.PageNumber - 1;

                // The existence of a non-zero entry in _originalViewPerspectives for the page
                // indicates that the thumbnail has been loaded. If the thumbnail has not been
                // loaded, don't apply the orientation change since it will cause the placeholder
                // "Loading" image to be rotaded.
                if (Active && _originalViewPerspectives[index] != 0)
                {
                    RasterImageListItem item = _imageList.Items[index];
                    item.Image.ViewPerspective = _originalViewPerspectives[index];
                    item.Image.RotateViewPerspective(e.Orientation);
                    item.Invalidate();
                }
            }
            catch (Exception ex)
            {
                // This is intentionally not a displayed exception since this event is not directly
                // raised by a user action.
                throw ex.AsExtract("ELI36813");
            }
        }

        /// <summary>
        /// Handles the <see cref="RasterImageList.SelectedIndexChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="RasterImageList.SelectedIndexChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="RasterImageList.SelectedIndexChanged"/> event.</param>
        void HandleImageListSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (Active && _imageViewer.IsImageAvailable)
                {
                    // Go to the page of the first selected item.
                    // Only one page can be selected at a time.
                    RasterImageListItemCollection items = _imageList.SelectedItems;
                    if (items.Count > 0)
                    {
                        int page = (int)items[0].Tag;
                        if (page != _imageViewer.PageNumber)
                        {
                            _imageViewer.PageNumber = page;
                        }
                    }
                    _imageList.Invalidate();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27920", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Timer.Tick"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Timer.Tick"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Timer.Tick"/> event.</param>
        void HandleTimerTick(object sender, EventArgs e)
        {
            try
            {
                // Determine the page range of the thumbnails to update
                int startIndex;
                int endIndex;
                if (_worker.IsRunning)
                {
                    // Update visible thumbnails only
                    startIndex = _imageList.TopIndex;
                    endIndex = GetBottomIndex();
                }
                else
                {
                    // Update all thumbnails
                    _timer.Stop();
                    startIndex = 0;
                    endIndex = _imageList.Items.Count - 1;
                }

                // Attempt to update default images
                bool success = UpdateThumbnailsFromWorker(startIndex, endIndex);
                if (!success)
                {
                    // If there are still visible default images, mark them as a priority to load
                    _worker.SetPriorityThumbnails(startIndex + 1, endIndex + 1);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27965", ex);
            }
        }

        #endregion Event Handlers

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the image viewer to which the <see cref="ThumbnailViewer"/> 
        /// is connected.
        /// </summary>
        /// <value>The image viewer to which the <see cref="ThumbnailViewer"/> is 
        /// connected. <see langword="null"/> if connection should be disconnected.</value>
        /// <returns>The image viewer to which the <see cref="ThumbnailViewer"/> is 
        /// connected. <see langword="null"/> if no connections are established.</returns>
        [Browsable(false)]
        [CLSCompliant(false)]
        public IDocumentViewer ImageViewer
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
                        _imageViewer.ImageFileChanged -= HandleImageViewerImageFileChanged;
                        _imageViewer.PageChanged -= HandleImageViewerPageChanged;
                        _imageViewer.InvertColorsStatusChanged -=
                            HandleImageViewerInvertColorsStatusChanged;
                        _imageViewer.OrientationChanged -= HandleImageViewerOrientationChanged;
                        _imageViewer.NonDisplayedPageOrientationChanged -=
                            HandleImageViewerOrientationChanged;
                    }

                    // Store the new image viewer
                    _imageViewer = value;

                    // Register for events
                    if (_imageViewer != null)
                    {
                        // The thumbnail viewer should keep it's color inversion state in sync with
                        // the ImageViewer InvertColors property.
                        InvertColors = _imageViewer.InvertColors;

                        _imageViewer.ImageFileChanged += HandleImageViewerImageFileChanged;
                        _imageViewer.PageChanged += HandleImageViewerPageChanged;
                        _imageViewer.InvertColorsStatusChanged +=
                            HandleImageViewerInvertColorsStatusChanged;
                        _imageViewer.OrientationChanged += HandleImageViewerOrientationChanged;
                        _imageViewer.NonDisplayedPageOrientationChanged +=
                            HandleImageViewerOrientationChanged;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI27922",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value.ToString(), false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members

        #region Private Members

        /// <summary>
        /// Gets or sets a value indicating whether the colors of the thumbnails are currently
        /// inverted.
        /// </summary>
        /// <value><see langword="true"/> if thumbnail colors are currently inverted; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool InvertColors
        {
            get
            {
                return _invertColors;
            }

            set
            {
                try
                {
                    if (value != _invertColors)
                    {
                        _invertColors = value;

                        InvertAllLoadedThumbnails();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36794");
                }
            }
        }

        /// <summary>
        /// Inverts all currently loaded thumbnails.
        /// </summary>
        void InvertAllLoadedThumbnails()
        {
            for (int i = 0; i < _imageList.Items.Count; i++)
            {
                RasterImageListItem item = _imageList.Items[i];
                if (item.Image != null && item.Image != _LOADING_IMAGE && item.Image != _ERROR_IMAGE)
                {
                    var invertCommand = new InvertCommand();
                    invertCommand.Run(item.Image);

                    item.Invalidate();
                }
            }
        }

        #endregion Private Members
    }
}
