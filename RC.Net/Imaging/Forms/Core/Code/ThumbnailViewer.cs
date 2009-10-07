using Leadtools;
using Leadtools.WinForms;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a control that displays thumbnails for the pages of an image.
    /// </summary>
    public partial class ThumbnailViewer : UserControl, IImageViewerControl
    {
        #region ThumbnailViewer Constants

        /// <summary>
        /// Licensing key to unlock document (anti-aliasing) support
        /// </summary>
        static readonly string _DOCUMENT_SUPPORT_KEY = "vhG42tyuh9";

        /// <summary>
        /// The default image to use for an image that is not yet loaded.
        /// </summary>
        static readonly RasterImage _LOADING_IMAGE = GetLoadingImage();

        #endregion ThumbnailViewer Constants

        #region ThumbnailViewer Fields

        /// <summary>
        /// The image viewer associated with the <see cref="ThumbnailViewer"/>.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// Loads the pages of an image in a separate thread.
        /// </summary>
        ThumbnailWorker _worker;

        #endregion ThumbnailViewer Fields

        #region ThumbnailViewer Constructors

        /// <summary>
        /// Initializes a new <see cref="ThumbnailViewer"/> class.
        /// </summary>
        public ThumbnailViewer()
        {
            InitializeComponent();

            // Turn on anti-aliasing
            RasterSupport.Unlock(RasterSupportType.Document, _DOCUMENT_SUPPORT_KEY);
            RasterPaintProperties properties = _imageList.PaintProperties;
            properties.PaintDisplayMode |= RasterPaintDisplayModeFlags.ScaleToGray;
            _imageList.PaintProperties = properties;
        }

        #endregion ThumbnailViewer Constructors

        #region ThumbnailViewer Methods

        /// <summary>
        /// Gets the default image to use for thumbnails that have not yet been loaded.
        /// </summary>
        /// <returns>The default image to use for thumbnails that have not yet been loaded.
        /// </returns>
        static RasterImage GetLoadingImage()
        {
            Image image = new Bitmap(typeof(ThumbnailViewer), "Resources.Loading.png");
            return new RasterImage(image);
        }

        /// <summary>
        /// Populates the image list with default loading image for each page and triggers the 
        /// worker thread to load page thumnails for the currently open image.
        /// </summary>
        void PopulateImageList()
        {
            _imageList.BeginUpdate();

            Clear();

            if (_imageViewer.IsImageAvailable)
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
                            if (image != null && image != _LOADING_IMAGE)
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
            _worker = new ThumbnailWorker(_imageViewer.ImageFile, _imageViewer.PageCount, 
                                          _imageList.ItemImageSize);

            _worker.BeginLoading();

            _timer.Start();
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
                        item.Invalidate();
                    }
                }
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

        #endregion ThumbnailViewer Methods

        #region ThumbnailViewer Event Handlers

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.ImageFileChanged"/> event.</param>
        void HandleImageViewerImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                PopulateImageList();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27918", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Forms.ImageViewer.PageChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Forms.ImageViewer.PageChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Forms.ImageViewer.PageChanged"/> event.</param>
        void HandleImageViewerPageChanged(object sender, PageChangedEventArgs e)
        {
            try
            {
                // Select the thumbnail corresponding to this page
                RasterImageListItem item = _imageList.Items[e.PageNumber - 1];
                if (!item.Selected)
                {
                    // Ensure the item to select is visible
                    _imageList.EnsureVisible(e.PageNumber - 1);

                    // Clear the current selection
                    ClearSelection();

                    // Select this item
                    item.Selected = true;
                    item.Invalidate();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27919", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                if (_imageViewer.IsImageAvailable)
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
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27920", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
                ExtractException ee = ExtractException.AsExtractException("ELI27965", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion ThumbnailViewer Event Handlers

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
                        _imageViewer.ImageFileChanged -= HandleImageViewerImageFileChanged;
                        _imageViewer.PageChanged -= HandleImageViewerPageChanged;
                    }

                    // Store the new image viewer
                    _imageViewer = value;

                    // Register for events
                    if (_imageViewer != null)
                    {
                        _imageViewer.ImageFileChanged += HandleImageViewerImageFileChanged;
                        _imageViewer.PageChanged += HandleImageViewerPageChanged;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI27922",
                        "Unable to establish connection to image viewer.", ex);
                    ee.AddDebugData("Image viewer", value, false);
                    throw ee;
                }
            }
        }

        #endregion IImageViewerControl Members
    }
}
