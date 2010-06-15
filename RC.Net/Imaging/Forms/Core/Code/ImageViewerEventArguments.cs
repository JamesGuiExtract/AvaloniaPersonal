using Leadtools;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Provides data for the <see cref="ImageViewer.ImageFileChanged"/> event.
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
    /// Provides data for the <see cref="ImageViewer.ImageFileClosing"/> event.
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
    /// Provides data for the <see cref="ImageViewer.ZoomChanged"/> event.
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
    /// Provides data for the <see cref="ImageViewer.OrientationChanged"/> event.
    /// </summary>
    public class OrientationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The orientation that was changed to.
        /// </summary>
        private readonly int _orientation;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrientationChangedEventArgs"/> class.
        /// </summary>
        /// <param name="orientation">The orientation that was changed to.</param>
        public OrientationChangedEventArgs(int orientation)
        {
            _orientation = orientation;
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
    }

    /// <summary>
    /// Provides data for the <see cref="ImageViewer.CursorToolChanged"/> event.
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
    /// Provides data for the <see cref="ImageViewer.FitModeChanged"/> event.
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
    /// Provides data for the <see cref="ImageViewer.PageChanged"/> event.
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
    /// Provides data for the <see cref="ImageViewer.OpeningImage"/> event.
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
    /// Provides data for the <see cref="ImageViewer.LoadingNewImage"/> event.
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
    /// Provides data for the <see cref="ImageViewer.DisplayingPrintDialog"/> event.
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
    /// Provides data for the <see cref="ImageViewer.FileOpenError"/> event.
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
    /// Provides data for an <see cref="ImageViewer"/> event dealing with a particular
    /// <see cref="LayerObject"/>.
    /// </summary>
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
    /// Provides data for an <see cref="ImageViewer.ImageExtracted"/> event.
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
        /// responsible for disposing <see cref="RasterImage.Dispose"/> of
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
}