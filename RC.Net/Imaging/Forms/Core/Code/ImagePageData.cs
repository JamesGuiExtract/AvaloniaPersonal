using Extract.Licensing;
using Leadtools;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents data associated with a single page of an image.
    /// </summary>
    public class ImagePageData
    {
        #region ImagePageData Constants

        /// <summary>
        /// The maximum number of entries in the zoom history.
        /// </summary>
        private static readonly int _MAX_ZOOM_HISTORY_COUNT = 20;

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(ImagePageData).ToString();

        #endregion ImagePageData Constants

        #region ImagePageData Fields

        /// <summary>
        /// The history of zoom operations for the current page.
        /// </summary>
        /// <seealso cref="ZoomInfo"/>
        /// <seealso cref="_currentZoom"/>
        LinkedList<ZoomInfo> _zoomHistory = new LinkedList<ZoomInfo>();

        /// <summary>
        /// The current zoom for the current page.
        /// </summary>
        /// <seealso cref="ZoomInfo"/>
        /// <seealso cref="_zoomHistory"/>
        LinkedListNode<ZoomInfo> _currentZoom;

        /// <summary>
        /// The orientation for the current page. Is always either 0, 90, 180, or 270.
        /// </summary>
        /// <seealso cref="Orientation"/>
        int _orientation;

        #endregion ImagePageData Fields

        #region ImagePageData Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePageData"/> class.
        /// </summary>
        public ImagePageData()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23107",
					_OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23108", ex);
            }
        }

        #endregion ImagePageData Constructors

        #region ImagePageData Properties

        /// <summary>
        /// Gets the visible rotation in degrees from the original image of the current page.
        /// </summary>
        /// <returns>The visible rotation in degrees from the original image of the current page.
        /// </returns>
        public int Orientation
        {
            get
            {
                return _orientation;
            }
        }

        /// <summary>
        /// Gets or sets the current zoom setting.
        /// </summary>
        /// <value>The current zoom setting.</value>
        /// <returns>The current zoom setting.</returns>
        public ZoomInfo ZoomInfo
        {
            get
            {
                return _currentZoom.Value;
            }
            internal set
            {
                // Don't add a zoom history if current zoom is the same as the zoom value to be
                // added.
                if (_currentZoom == null || _currentZoom.Value != value)
                {
                    // Remove any subsequent zooms in the history
                    while (_currentZoom != _zoomHistory.Last)
                    {
                        _zoomHistory.RemoveLast();
                    }

                    // Check if the zoom history is at maximum capacity
                    if (_zoomHistory.Count == _MAX_ZOOM_HISTORY_COUNT)
                    {
                        // Remove the oldest zoom history item
                        _zoomHistory.RemoveFirst();
                    }

                    // Add the zoom to the zoom history
                    _zoomHistory.AddLast(value);

                    // Store the current zoom
                    _currentZoom = _zoomHistory.Last;
                }
            }
        }

        /// <summary>
        /// Gets whether there is a previous entry in the zoom history.
        /// </summary>
        /// <returns><see langword="true"/> if a previous entry exists; <see langword="false"/> if 
        /// a previous entry does not exist.</returns>
        public bool CanZoomPrevious
        {
            get
            {
                return _currentZoom != _zoomHistory.First;
            }
        }

        /// <summary>
        /// Gets whether there is a subsequent entry in the zoom history.
        /// </summary>
        /// <returns><see langword="true"/> if a subsequent entry exists; <see langword="false"/> 
        /// if a subsequent entry does not exist.</returns>
        public bool CanZoomNext
        {
            get
            {
                return _currentZoom != _zoomHistory.Last;
            }
        }

        /// <summary>
        /// Gets the number of zoom history entries.
        /// </summary>
        /// <value>The number of zoom history entries.</value>
        public int ZoomHistoryCount
        {
            get
            {
                return _zoomHistory.Count;
            }
        }

        #endregion ImagePageData Properties

        #region ImagePageData Methods

        /// <summary>
        /// Rotates the page orientation clockwise by the specified number of degrees.
        /// </summary>
        /// <param name="angle">The number of degrees to rotate the page orientation clockwise.
        /// </param>
        internal void RotateOrientation(int angle)
        {
            _orientation += angle;
            _orientation %= 360;

            if (_orientation < 0)
            {
                _orientation += 360;
            }
        }

        /// <summary>
        /// Retreats to the previous entry in the zoom history.
        /// </summary>
        /// <returns>The previous entry in the zoom history.</returns>
        internal ZoomInfo ZoomPrevious()
        {
            _currentZoom = _currentZoom.Previous;
            return _currentZoom.Value;
        }

        /// <summary>
        /// Advance to the next entry in the zoom history.
        /// </summary>
        /// <returns>The next entry in the zoom history.</returns>
        internal ZoomInfo ZoomNext()
        {
            _currentZoom = _currentZoom.Next;
            return _currentZoom.Value;
        }

        #endregion ImagePageData Methods
    }
}
