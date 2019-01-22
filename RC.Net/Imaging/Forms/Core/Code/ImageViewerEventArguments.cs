using Leadtools;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Provides data for the <see cref="DocumentViewer.ImageFileChanged"/> event.
    /// </summary>
    public class ImageFileChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The file name to which the image changed.
        /// </summary>
        private readonly string _fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFileChangedEventArgs"/> class.
        /// </summary>
        /// <param name="fileName">The file name to which the image changed.</param>
        public ImageFileChangedEventArgs(string fileName)
        {
            _fileName = fileName;
        }

        /// <summary>
        /// Gets the file name to which the image changed.
        /// </summary>
        /// <returns>The file name to which the image changed.</returns>
        public string FileName
        {
            get
            {
                return _fileName;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="DocumentViewer.ImageFileClosing"/> event.
    /// </summary>
    public class ImageFileClosingEventArgs : CancelEventArgs
    {
        /// <summary>
        /// The file name which is about to close
        /// </summary>
        private readonly string _fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFileClosingEventArgs"/> class.
        /// </summary>
        /// <param name="fileName">The file name which is about to close.</param>
        public ImageFileClosingEventArgs(string fileName)
        {
            _fileName = fileName;
        }

        /// <summary>
        /// Gets the file name which is about to close.
        /// </summary>
        /// <returns>The file name which is about to close.</returns>
        public string FileName
        {
            get
            {
                return _fileName;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="DocumentViewer.ZoomChanged"/> event.
    /// </summary>
    public class ZoomChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The zoom info that was changed to.
        /// </summary>
        private readonly ZoomInfo _zoomInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoomChangedEventArgs"/> class.
        /// </summary>
        /// <param name="zoomInfo">The zoom info that was changed to.</param>
        public ZoomChangedEventArgs(ZoomInfo zoomInfo)
        {
            _zoomInfo = zoomInfo;
        }

        /// <summary>
        /// Gets the zoom info that was changed to.
        /// </summary>
        /// <returns>The zoom info that was changed to.</returns>
        public ZoomInfo ZoomInfo
        {
            get
            {
                return _zoomInfo;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="DocumentViewer.OrientationChanged"/> event.
    /// </summary>
    public class OrientationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The orientation that was changed to.
        /// </summary>
        private readonly int _orientation;

        /// <summary>
        /// The page number of the affected image.
        /// </summary>
        private readonly int _pageNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrientationChangedEventArgs"/> class.
        /// </summary>
        /// <param name="orientation">The orientation that was changed to.</param>
        /// <param name="pageNumber">The page number of the affected image.</param>
        public OrientationChangedEventArgs(int orientation, int pageNumber)
        {
            _orientation = orientation;
            _pageNumber = pageNumber;
        }

        /// <summary>
        /// Gets the orientation that was changed to.
        /// </summary>
        /// <returns>The orientation that was changed to.</returns>
        public int Orientation
        {
            get
            {
                return _orientation;
            }
        }

        /// <summary>
        /// Gets the page number of the affected image.
        /// </summary>
        public int PageNumber
        {
            get
            {
                return _pageNumber;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="DocumentViewer.CursorToolChanged"/> event.
    /// </summary>
    public class CursorToolChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The cursor tool that was changed to.
        /// </summary>
        private readonly CursorTool _cursorTool;

        /// <summary>
        /// Initializes a new instance of the <see cref="CursorToolChangedEventArgs"/> class.
        /// </summary>
        /// <param name="cursorTool">The cursor tool that was changed to.</param>
        public CursorToolChangedEventArgs(CursorTool cursorTool)
        {
            _cursorTool = cursorTool;
        }

        /// <summary>
        /// Gets the cursor tool that was changed to.
        /// </summary>
        /// <returns>The cursor tool that was changed to.</returns>
        public CursorTool CursorTool
        {
            get
            {
                return _cursorTool;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="DocumentViewer.FitModeChanged"/> event.
    /// </summary>
    public class FitModeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The fit mode that was changed to.
        /// </summary>
        private readonly FitMode _fitMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="FitModeChangedEventArgs"/> class.
        /// </summary>
        /// <param name="fitMode">The fit mode that was changed to.</param>
        public FitModeChangedEventArgs(FitMode fitMode)
        {
            _fitMode = fitMode;
        }

        /// <summary>
        /// Gets the fit mode that was changed to.
        /// </summary>
        /// <returns>The fit mode that was changed to.</returns>
        public FitMode FitMode
        {
            get
            {
                return _fitMode;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="DocumentViewer.PageChanged"/> event.
    /// </summary>
    public class PageChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The page number that was changed to.
        /// </summary>
        private readonly int _pageNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageChangedEventArgs"/> class.
        /// </summary>
        /// <param name="pageNumber">The page number that was changed to.</param>
        public PageChangedEventArgs(int pageNumber)
        {
            _pageNumber = pageNumber;
        }

        /// <summary>
        /// Gets the page number that was changed to.
        /// </summary>
        /// <returns>The page number that was changed to.</returns>
        public int PageNumber
        {
            get
            {
                return _pageNumber;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="DocumentViewer.OpeningImage"/> event.
    /// </summary>
    public class OpeningImageEventArgs : CancelEventArgs
    {
        /// <summary>
        /// The file name that is being opened.
        /// </summary>
        private readonly string _fileName;

        /// <summary>
        /// Whether to update the MRU list or not
        /// </summary>
        private readonly bool _updateMruList;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpeningImageEventArgs"/> class.
        /// </summary>
        /// <param name="fileName">The file name to be opened.</param>
        /// <param name="updateMruList">Whether to update the MRU list or not.</param>
        public OpeningImageEventArgs(string fileName, bool updateMruList)
        {
            _fileName = fileName;
            _updateMruList = updateMruList;
        }

        /// <summary>
        /// Gets the file name to be opened.
        /// </summary>
        /// <returns>The file name to be opened.</returns>
        public string FileName
        {
            get
            {
                return _fileName;
            }
        }

        /// <summary>
        /// Gets whether to update the MRU list or not.
        /// </summary>
        /// <returns>Whether to update the MRU list or not.</returns>
        public bool UpdateMruList
        {
            get
            {
                return _updateMruList;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="DocumentViewer.LoadingNewImage"/> event.
    /// </summary>
    public class LoadingNewImageEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingNewImageEventArgs"/> class.
        /// </summary>
        public LoadingNewImageEventArgs()
        {
        }
    }

    /// <summary>
    /// Provides data for the <see cref="DocumentViewer.DisplayingPrintDialog"/> event.
    /// </summary>
    public class DisplayingPrintDialogEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayingPrintDialogEventArgs"/> class.
        /// </summary>
        public DisplayingPrintDialogEventArgs()
        {
        }
    }

    /// <summary>
    /// Provides data for the <see cref="DocumentViewer.FileOpenError"/> event.
    /// </summary>
    public class FileOpenErrorEventArgs : CancelEventArgs
    {
        /// <summary>
        /// The filename that could not be opened.
        /// </summary>
        private readonly string _fileName;

        /// <summary>
        /// The exception to be thrown if this event is not cancelled.
        /// </summary>
        private readonly ExtractException _extractException;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileOpenErrorEventArgs"/> class.
        /// </summary>
        /// <param name="fileName">The filename that could not be opened.</param>
        /// <param name="extractException">The exception to be thrown if this event is not 
        /// cancelled.</param>
        public FileOpenErrorEventArgs(string fileName, ExtractException extractException)
        {
            _fileName = fileName;
            _extractException = extractException;
        }

        /// <summary>
        /// Gets the layerObject that was added.
        /// </summary>
        /// <returns>The layerObject that was added.</returns>
        public string FileName
        {
            get
            {
                return _fileName;
            }
        }

        /// <summary>
        /// Gets the exception to be thrown if this event is not cancelled.
        /// </summary>
        /// <returns>The exception to be thrown if this event is not cancelled.</returns>
        public ExtractException ExtractException
        {
            get
            {
                return _extractException;
            }
        }
    }

    /// <summary>
    /// Provides data for an <see cref="DocumentViewer"/> event dealing with a particular
    /// <see cref="LayerObject"/>.
    /// </summary>
    [CLSCompliant(false)]
    public class LayerObjectEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="LayerObject"/> involved in the event.
        /// </summary>
        private readonly LayerObject _layerObject;

        /// <summary>
        /// Initializes a new <see cref="LayerObjectEventArgs"/> instance
        /// </summary>
        /// <param name="layerObject">The <see cref="LayerObject"/> involved in the event.</param>
        public LayerObjectEventArgs(LayerObject layerObject)
        {
            _layerObject = layerObject;
        }

        /// <summary>
        /// Gets the <see cref="LayerObject"/> involved in the event.
        /// </summary>
        /// <returns>The <see cref="LayerObject"/> involved in the event.</returns>
        public LayerObject LayerObject
        {
            get
            {
                return _layerObject;
            }
        }
    }

    /// <summary>
    /// Provides data for an <see cref="DocumentViewer.ImageExtracted"/> event.
    /// </summary>
    public class ImageExtractedEventArgs : EventArgs
    {
        /// <summary>
        /// The extracted image.
        /// </summary>
        private readonly RasterImage _image;

        /// <summary>
        /// The orientation of the original source image.
        /// </summary>
        private readonly int _orientation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageExtractedEventArgs"/>
        /// class.
        /// </summary>
        /// <param name="image">The extracted image that was created.</param>
        /// <param name="orientation">The orientation of the original image.
        /// Will always be 0, 90, 180, or 270.</param>
        public ImageExtractedEventArgs(RasterImage image, int orientation)
        {
            _image = image;
            _orientation = orientation;
        }

        /// <summary>
        /// Gets a clone of the extracted image.  The caller of this method is
        /// responsible for disposing <see cref="RasterImage.Dispose(bool)"/> of
        /// the image object.
        /// </summary>
        /// <returns>A clone of the extracted image.</returns>
        // This is intentionally being listed as a method to help reinforce
        // the concept that the image coming back is a clone and that the caller
        // is repsonsible to dispose of the image.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public RasterImage GetExtractedImage()
        {
            return _image.Clone();
        }

        /// <summary>
        /// Gets the orientation that the original image is displayed with.
        /// </summary>
        public int Orientation
        {
            get
            {
                return _orientation;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="DocumentViewer.ExtendedNavigationCheck"/> event.
    /// </summary>
    public class ExtendedNavigationCheckEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedNavigationCheckEventArgs"/> class.
        /// </summary>
        /// <param name="forward"><see langword="true"/> if this instance involves forward navigation;
        /// otherwise, <see langword="false"/>.</param>
        /// <param name="tileNavigation">see langword="true"/> if this instance involves tile
        /// navigation; <see langword="false"/> if page navigation.</param>
        public ExtendedNavigationCheckEventArgs(bool forward, bool tileNavigation)
        {
            try
            {
                Forward = forward;
                TileNavigation = tileNavigation;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32382");
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ExtendedNavigationCheckEventArgs"/> involves
        /// forward navigation.
        /// </summary>
        /// <value><see langword="true"/> if forward navigation; otherwise, <see langword="false"/>.
        /// </value>
        public bool Forward
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ExtendedNavigationCheckEventArgs"/> involves
        /// tile navigation.
        /// </summary>
        /// <value><see langword="true"/> if tile navigation; <see langword="false"/> if page
        /// navigation.
        /// </value>
        public bool TileNavigation
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the specified navigation is available.
        /// </summary>
        /// <value><see langword="true"/> if the specified navigation is available;
        /// otherwise, <see langword="false"/>.
        /// <para><b>Note</b></para>
        /// Use the |= operator to set this value so as not not to override any other handler that
        /// had set this property to <see langword="true"/>.
        /// </value>
        public bool IsAvailable
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="DocumentViewer.ExtendedNavigation"/> event.
    /// </summary>
    public class ExtendedNavigationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedNavigationEventArgs"/> class.
        /// </summary>
        /// <param name="forward"><see langword="true"/> if this instance involves forward navigation;
        /// otherwise, <see langword="false"/>.</param>
        /// <param name="tileNavigation">see langword="true"/> if this instance involves tile
        /// navigation; <see langword="false"/> if page navigation.</param>
        public ExtendedNavigationEventArgs(bool forward, bool tileNavigation)
        {
            try
            {
                Forward = forward;
                TileNavigation = tileNavigation;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32390");
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ExtendedNavigationCheckEventArgs"/> involves
        /// forward navigation.
        /// </summary>
        /// <value><see langword="true"/> if forward navigation; otherwise, <see langword="false"/>.
        /// </value>
        public bool Forward
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ExtendedNavigationCheckEventArgs"/> involves
        /// tile navigation.
        /// </summary>
        /// <value><see langword="true"/> if tile navigation; <see langword="false"/> if page
        /// navigation.
        /// </value>
        public bool TileNavigation
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the event was handled.
        /// </summary>
        /// <value><see langword="true"/> if the event was handled; otherwise,
        /// <see langword="false"/>.</value>
        public bool Handled
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="DocumentViewer.BackgroundProcessStatusUpdate"/> event.
    /// </summary>
    public class BackgroundProcessStatusUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundProcessStatusUpdateEventArgs"/> class.
        /// </summary>
        /// <param name="status">A message describing the current state of the background
        /// loading operation.</param>
        /// <param name="progressPercent">The percent loading is complete for the document as a
        /// whole.</param>
        public BackgroundProcessStatusUpdateEventArgs(string status, double progressPercent)
            : base()
        {
            Status = status;
            ProgressPercent = progressPercent;
        }

        /// <summary>
        /// Gets a message describing the current state of the background loading operation.
        /// </summary>
        public string Status
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the percent loading is complete for the document as a whole.
        /// </summary>
        public double ProgressPercent
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// Provides data for events relating to OCR text.
    /// </summary>
    public class OcrTextEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OcrTextEventArgs"/>
        /// class.
        /// </summary>
        /// <param name="ocrData">A <see cref="ThreadSafeSpatialString"/> instance representing the
        /// data for the event.</param>
        public OcrTextEventArgs(ThreadSafeSpatialString ocrData)
            : base()
        {
            OcrData = ocrData;
        }

        /// <summary>
        /// Gets a <see cref="ThreadSafeSpatialString"/> instance representing the data for the
        /// event.
        /// </summary>
        public ThreadSafeSpatialString OcrData
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="ThumbnailWorker.ThumbnailLoaded"/> event.
    /// </summary>
    public class ThumbnailLoadedEventArgs : EventArgs
    {
        /// <summary>
        /// The page number of the thumbnail that was loaded.
        /// </summary>
        int _pageNumber;

        /// <summary>
        /// TThe page number of the thumbnail that was loaded.
        /// </summary>
        RasterImage _thumbnailImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbnailLoadedEventArgs"/> class.
        /// </summary>
        /// <param name="pageNumber">The page number of the thumbnail that was loaded.</param>
        /// <param name="thumbnailImage">The page number of the thumbnail that was loaded.</param>
        public ThumbnailLoadedEventArgs(int pageNumber, RasterImage thumbnailImage)
            : base()
        {
            _pageNumber = pageNumber;
            _thumbnailImage = thumbnailImage;
        }

        /// <summary>
        /// Gets the page number of the thumbnail that was loaded.
        /// </summary>
        /// <value>The page number of the thumbnail that was loaded.</value>
        public int PageNumber
        {
            get
            {
                return _pageNumber;
            }
        }

        /// <summary>
        /// Gets the thumbnail as a <see cref="RasterImage"/>.
        /// </summary>
        /// <value>The thumbnail as a <see cref="RasterImage"/>.</value>
        public RasterImage ThumbnailImage
        {
            get
            {
                return _thumbnailImage;
            }
        }
    }
}