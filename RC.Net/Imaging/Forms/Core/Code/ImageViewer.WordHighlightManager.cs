﻿using Extract.Drawing;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;

namespace Extract.Imaging.Forms
{
    sealed partial class ImageViewer
    {
        /// <summary>
        /// Helper class that creates and displays <see cref="Highlight"/>s for the word
        /// redaction/highlighter tools by using image OCR data.
        /// </summary>
        class WordHighlightManager : IDisposable
        {
            #region Constants

            /// <summary>
            /// The maximum height of an auto-fit zone in terms of the average line height of the page.
            /// </summary>
            const float _AUTO_FIT_MAX_HEIGHT = 5F;

            /// <summary>
            /// Controls how lenient the algorighm to find auto-fit zones is when a hard edge of
            /// pixel content cannot be found.
            /// </summary>
            const float _AUTO_FIT_FUZZY_FACTOR = 0.25F;

            /// <summary>
            /// After finding qualifying a "fuzzy" edge to an auto-fit zone, continue searching
            /// this much further for a true edge.
            /// </summary>
            const float _AUTO_FIT_FUZZY_BUFFER = 0.5F;

            #endregion Constants

            #region Fields

            /// <summary>
            /// The registry settings for the image viewer.
            /// </summary>
            readonly RegistrySettings<Properties.Settings> _registry;

            /// <summary>
            /// A lock to synchronize access to the <see cref="WordHighlightManager"/>'s fields.
            /// </summary>
            object _lock = new object();

            /// <summary>
            /// Synchronizes access to OCR resources
            /// </summary>
            object _ocrLock = new object();

            /// <summary>
            /// A <see cref="BackgroundWorker"/> object that, when the word highlight/redaction tool
            /// is active, loads word spatial info from OCR data and displays dynamic auto-fit
            /// highlights during tracking events.
            /// </summary>
            BackgroundWorker _backgroundWorker;

            /// <summary>
            /// Set when the background worker is inactive or doesn't currently have operation to
            /// perform.
            /// </summary>
            ManualResetEvent _backgroundWorkerIdle = new ManualResetEvent(true); 

            /// <summary>
            /// An event to signal the background worker that a new operation is available to
            /// perform (or to stop waiting since the backgroung worker is being deactivated).
            /// </summary>
            EventWaitHandle _operationAvailable = new ManualResetEvent(false);

            /// <summary>
            /// Allows the most recently started operation to be canceled.
            /// </summary>
            volatile CancellationTokenSource _canceler;

            /// <summary>
            /// Allows any currently running OCR operation to be canceled.
            /// </summary>
            volatile CancellationTokenSource _ocrCanceler;

            /// <summary>
            /// The <see cref="CancellationToken"/> assosicated with _canceler that operations
            /// should periodically check to see if they have been canceled.
            /// </summary>
            CancellationToken _cancelToken;

            /// <summary>
            /// An operation that has been scheduled to run as soon as any currently running
            /// operation ends.
            /// </summary>
            volatile Action _pendingOperation;

            /// <summary>
            /// Indicates the number of outstanding calls scheduled to run via the UI message queue.
            /// </summary>
            long _executeInUIReferenceCount = 0;

            /// <summary>
            /// The <see cref="ImageViewer"/> for which word highlights are being managed.
            /// </summary>
            volatile ImageViewer _imageViewer;

            /// <summary>
            /// The OCR data from the current <see cref="ImageViewer"/> document.
            /// </summary>
            volatile ThreadSafeSpatialString _ocrData;

            /// <summary>
            /// Maintains the OCR data for each individual page.
            /// </summary>
            ConcurrentDictionary<int, ThreadSafeSpatialString> _ocrPageData =
                new ConcurrentDictionary<int, ThreadSafeSpatialString>();

            /// <summary>
            /// Indicates whether a call to OCRPage has completed and that results are now available
            /// (if the operation completed successfully).
            /// </summary>
            ManualResetEvent _ocrPageComplete = new ManualResetEvent(true); 

            /// <summary>
            /// The background process status message for each document page.
            /// -1 indicates a status that should be used regardless of the current image page.
            /// </summary>
            ConcurrentDictionary<int, string> _pageStatusMessages =
                new ConcurrentDictionary<int, string>();

            /// <summary>
            /// The collection of word highlights that have been created based on OCR data, grouped
            /// by page. Concurrent because in the case of a cancelled background LoaderOperation,
            /// it may have scheduled highlights to be added/removed via the UI message queue and
            /// this could conceivably end up running after the next background operation has begun.
            /// </summary>
            ConcurrentDictionary<int, HashSet<LayerObject>> _wordHighlights =
                new ConcurrentDictionary<int, HashSet<LayerObject>>();

            /// <summary>
            /// Maps each word to the id of the line on the page it belongs to so that when multiple
            /// words on the same line are selected, they can be combined into one highlight.
            /// </summary>
            volatile Dictionary<LayerObject, int> _wordLineMapping =
                new Dictionary<LayerObject, int>();

            /// <summary>
            /// A set of all pages for which word highlights have been added.
            /// </summary>
            volatile HashSet<int> _pagesOfAddedWordHighlights = new HashSet<int>();

            /// <summary>
            /// The set of highlights that are under the word highlight/redaction cursor or that are
            /// contained within its selection box.
            /// </summary>
            HashSet<LayerObject> _activeWordHighlights = new HashSet<LayerObject>();

            /// <summary>
            /// The OCR content associated with each word highlight. This is only populated when
            /// <see cref="ImageViewer.RedactionMode"/> is <see langword="false"/>.
            /// </summary>
            ConcurrentDictionary<LayerObject, ThreadSafeSpatialString> _highlightOcr =
                new ConcurrentDictionary<LayerObject, ThreadSafeSpatialString>();

            /// <summary>
            /// The OCR content of the word highlights loaded for the current document. This
            /// dictionary is used to sort the results of and word highlight operation since the
            /// words in this lists are in the same order as the OCR on the page. This is only
            /// populated when <see cref="ImageViewer.RedactionMode"/> is <see langword="false"/>.
            /// </summary>
            ConcurrentDictionary<int, List<ThreadSafeSpatialString>> _loadedOcrWords =
                new ConcurrentDictionary<int, List<ThreadSafeSpatialString>>();

            /// <summary>
            /// Indicates for which image page OCR or word highlights are currently being loaded.
            /// -1 indicates no background loading is currently occurring.
            /// </summary>
            volatile int _loadingPage = -1;

            /// <summary>
            /// The average line height for each page.
            /// </summary>
            Dictionary<int, int> _averageLineHeight = new Dictionary<int, int>();

            /// <summary>
            /// The starting point in client coordinates of the current tracking operation. Used
            /// when generating auto zones.
            /// </summary>
            Point? _trackingStartLocation;

            /// <summary>
            /// The last point in client coordinates from which an auto zone was attempted to be
            /// found.
            /// </summary>
            Point? _currentAutoFitLocation;

            /// <summary>
            /// The current auto fit highlight (if any).
            /// </summary>
            volatile Highlight _autoFitHighlight;

            /// <summary>
            /// Indicates whether the loaded data should be cleared and disposed of before
            /// starting the next load task.
            /// </summary>
            volatile bool _clearData;

            /// <summary>
            /// <see langword="true"/> if currently loading/managing word highlights.
            /// </summary>
            volatile bool _highlightsEnabled;

            /// <summary>
            /// The <see cref="AsynchronousOcrManager"/> to be used for background OCRing.
            /// </summary>
            volatile AsynchronousOcrManager _ocrManager;

            /// <summary>
            /// The number of pages in the currently loaded document.
            /// </summary>
            volatile int _pageCount;

            /// <summary>
            /// Indicates whether the background worker thread should be restarted after completion
            /// because a new operation was scheduled after the currently running instance was
            /// canceled.
            /// </summary>
            volatile bool _restartBackgroundWorker;

            /// <summary>
            /// Indicates whether the word highlight tool is in auto-fit mode.
            /// </summary>
            volatile bool _inAutoFitMode;

            /// <summary>
            /// Indicates whether this instance has been disposed.
            /// </summary>
            volatile bool _disposed;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="WordHighlightManager"/> class.
            /// </summary>
            /// <param name="imageViewer">The image viewer.</param>
            public WordHighlightManager(ImageViewer imageViewer)
            {
                try
                {
                    _registry = new RegistrySettings<Properties.Settings>(
                        @"Software\Extract Systems\Imaging");

                    _imageViewer = imageViewer;

                    _imageViewer.PageChanged += HandlePageChanged;
                    _imageViewer.ImageFileChanged += HandleImageFileChanged;
                    _imageViewer.ImageFileClosing += HandleImageFileClosing;
                    _imageViewer.CursorToolChanged += HandleCursorToolChanged;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31290", ex);
                }
            }

            #endregion Constructors

            #region Events

            /// <summary>
            /// Raised when a new status message is available regarding the state of background operations.
            /// </summary>
            public event EventHandler<BackgroundProcessStatusUpdateEventArgs> BackgroundProcessStatusUpdate;

            /// <summary>
            /// Raised when the automatic OCR processing has successfully completed on the currently
            /// loaded document.
            /// </summary>
            public event EventHandler<OcrTextEventArgs> OcrLoaded;

            #endregion Events

            #region Properties

            /// <summary>
            /// Gets a value indicating whether the word highlight tool is in auto-fit mode.
            /// </summary>
            /// <value>
            /// <see langword="true"/> if the word highlight tool is in auto-fit mode. otherwise,
            /// <see langword="false"/>.
            /// </value>
            public bool InAutoFitMode
            {
                get
                {
                    return _inAutoFitMode;
                }

                private set
                {
                    if (value != _inAutoFitMode)
                    {
                        // Swap auto-fit highlights and word highlights, and re-activate word
                        // loading if auto-fit mode is no longer active.
                        if (value)
                        {
                            HideWordHighlights(false);
                        }
                        else
                        {
                            Activate();
                            RemoveAutoFitHighlight();
                            ShowWordHighlights();
                        }

                        _imageViewer.Invalidate();

                        _inAutoFitMode = value;
                    }
                }
            }

            /// <summary>
            /// Gets a value indicating whether the word highlight tool is in a valid auto-fit
            /// operation.
            /// </summary>
            /// <value>
            /// <see langword="true"/> if the word highlight tool is in a valid auto-fit operation.
            /// otherwise, <see langword="false"/>.
            /// </value>
            public bool IsValidAutoFitOperation
            {
                get
                {
                    return InAutoFitMode && _trackingStartLocation != null;
                }
            }

            /// <summary>
            /// Gets a value indicating whether OCR or word zone data is currently being loaded in
            /// the background.
            /// </summary>
            /// <value><see langword="true"/> if OCR or word zone data is currently being loaded in
            /// the background; otherwise, <see langword="false"/>.
            /// </value>
            public bool IsLoadingData
            {
                get
                {
                    return _loadingPage >= 0;
                }
            }

            /// <summary>
            /// Gets OCR associated with the currently loaded document (if available).
            /// </summary>
            public ThreadSafeSpatialString OcrData
            {
                get
                {
                    // If a clear data operation has been requested any value in _ocrData is no
                    // longer valid.
                    return _clearData ? null : _ocrData;
                }
            }

            #endregion Properties

            #region Methods

            /// <summary>
            /// Starts a tracking operation for word highlights (highlights become colored).
            /// </summary>
            /// <param name="x">The client X coordinate from which the tracking operation was
            /// started.</param>
            /// <param name="y">The client Y coordinate from which the tracking operation was
            /// started.</param>
            public void StartTrackingOperation(int x, int y)
            {
                try
                {
                    // Get the starting point of the tracking operation
                    Point[] startPoint = new Point[] { new Point(x, y) };

                    // Convert the points from client to image coordinates.
                    GeometryMethods.InvertPoints(_imageViewer._transform, startPoint);

                    // Ensure the starting point is onpage before starting an auto-fit
                    // operation.
                    if (startPoint[0].X >= 0 && startPoint[0].X < _imageViewer.ImageWidth &&
                        startPoint[0].Y >= 0 && startPoint[0].Y < _imageViewer.ImageHeight)
                    {
                        _trackingStartLocation = new Point(x, y);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31295", ex);
                }
            }

            /// <summary>
            /// Updates an active tracking operation using the mouse position specified.
            /// </summary>
            /// <param name="mouseX">The physical (client) x coordinate of the mouse.</param>
            /// <param name="mouseY">The physical (client) y coordinate of the mouse.</param>
            public void UpdateTracking(int mouseX, int mouseY)
            {
                try
                {
                    if (IsValidAutoFitOperation)
                    {
                        // If the mouse has moved less that 2 client pixels since the last time we
                        // started calculating an auto zone, allow the previous calculation to complete
                        // so that we don't excessively restart calculations for small mouse movements.
                        if (_currentAutoFitLocation.HasValue)
                        {
                            // Compute the distance
                            int dX = mouseX - _currentAutoFitLocation.Value.X;
                            int dY = mouseY - _currentAutoFitLocation.Value.Y;
                            double distance = Math.Sqrt(dX * dX + dY * dY);

                            if (distance < 2)
                            {
                                return;
                            }
                        }

                        _currentAutoFitLocation = new Point(mouseX, mouseY);

                        // Get the points to be used by the AutoFitOperation
                        Point[] points = new Point[] 
                            { 
                                _trackingStartLocation.Value,
                                _currentAutoFitLocation.Value
                            };

                        // Convert the points from client to image coordinates.
                        GeometryMethods.InvertPoints(_imageViewer._transform, points);

                        // Cancel any currently running task and start calculating an auto-fit zone
                        // based on the current mouse location.
                        StartOperation(() => AutoFitOperation(points[0], points[1]));
                    }
                    else if (!InAutoFitMode)
                    {
                        // Ensure that if auto-fit mode is re-activated, it will start
                        // re-calculating a zone even if the mouse has not moved.
                        _currentAutoFitLocation = null;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33370");
                }
            }

            /// <summary>
            /// Ends a tracking operation for word highlights resulting in a new
            /// <see cref="Redaction"/> or <see cref="Highlight"/>.
            /// </summary>
            /// <param name="cancel"><see langword="true"/> if the tracking operation is ending
            /// because it was cancelled, <see langword="false"/> if it completed normally and a
            /// <see cref="Redaction"/>/<see cref="Highlight"/> is to be created.</param>
            public void EndTrackingOperation(bool cancel)
            {
                bool restartLoading = false;

                try
                {
                    // Before accessing fields that may be modifed by the background worker, stop
                    // any currently running operation on the background worker.
                    lock (_lock)
                    {
                        restartLoading = IsLoadingData || 
                            !_pagesOfAddedWordHighlights.Contains(_imageViewer.PageNumber);
                        
                        // false to allow OCR operations to continue.
                        CancelRunningOperation(false);
                    }

                    if (!_backgroundWorkerIdle.WaitOne(1000))
                    {
                        new ExtractException("ELI31374",
                            "Application trace: Word highlight background operation aborted.").Log();
                    }

                    if (!cancel)
                    {
                        if (_imageViewer.RedactionMode)
                        {
                            CreateOutput<Redaction>();
                        }
                        else
                        {
                            CreateOutput<CompositeHighlightLayerObject>();
                        }
                    }

                    // If this was an auto-fit operation, reset the auto-fit data.
                    if (InAutoFitMode)
                    {
                        RemoveAutoFitHighlight();
                    }
                    else
                    {
                        // Otherwise, hide all word highlights until the tool is used/moved again.
                        HideWordHighlights(true);
                    }

                    _trackingStartLocation = null;

                    _imageViewer.Invalidate();
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31296", ex);
                }
                finally
                {
                    // Call activate to ensure we pick up loading word highlights again if
                    // a loading task was canceled.
                    if (restartLoading)
                    {
                        Activate();
                    }
                }
            }

            #endregion Methods

            #region Event Handlers

            /// <summary>
            /// Handles the image file changed event.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="Extract.Imaging.Forms.ImageFileChangedEventArgs"/>
            /// instance containing the event data.</param>
            void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
            {
                try
                {
                    // When a new page is loaded, activate.
                    if (_imageViewer.IsImageAvailable)
                    {
                        Activate();
                    }
                    // Otherwise, deactivate and reset all loaded data
                    else
                    {
                        Deactivate(true);

                        // Ensure the background progress status is cleared.
                        OnBackgroundProcessStatusUpdate("", 0);
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Display("ELI31316", ex);
                }
            }

            /// <summary>
            /// Handles the image file closing event.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="Extract.Imaging.Forms.ImageFileClosingEventArgs"/>
            /// instance containing the event data.</param>
            void HandleImageFileClosing(object sender, ImageFileClosingEventArgs e)
            {
                try
                {
                    // Deactivate and reset all loaded data
                    Deactivate(true);
                }
                catch (Exception ex)
                {
                    ExtractException.Display("ELI31319", ex);
                }
            }

            /// <summary>
            /// Handles the image viewer page changed event.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="Extract.Imaging.Forms.PageChangedEventArgs"/>
            /// instance containing the event data.</param>
            void HandlePageChanged(object sender, PageChangedEventArgs e)
            {
                try
                {
                    // After changing pages, call Activate. Even if a loader task is already
                    // active, this will force highlights to be loaded for the current page first.
                    Activate();
                }
                catch (Exception ex)
                {
                    ExtractException.Display("ELI31315", ex);
                }
            }

            /// <summary>
            /// Handles the cursor tool changed event.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="Extract.Imaging.Forms.CursorToolChangedEventArgs"/> instance containing the event data.</param>
            void HandleCursorToolChanged(object sender, CursorToolChangedEventArgs e)
            {
                try
                {
                    if (_imageViewer.IsImageAvailable)
                    {
                        // If a word highlighter/redactor tool was selected, call Activate to ensure
                        // word highlights are loaded for the current page.
                        if (e.CursorTool == CursorTool.WordHighlight ||
                            e.CursorTool == CursorTool.WordRedaction)
                        {
                            Activate();
                        }
                        else
                        {
                            DisableWordHighlights();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31317", ex);
                }
            }

            /// <summary>
            /// Handles the case that the image viewer cursor changed.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
            void HandleCursorChanged(object sender, EventArgs e)
            {
                try
                {
                    // Auto-fit mode is allowed to change in the middle of a tracking operation.
                    InAutoFitMode = (_imageViewer.Cursor == ExtractCursors.ShiftWordRedaction ||
                                     _imageViewer.Cursor == ExtractCursors.ShiftWordHighlight);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33372");
                }
            }

            /// <summary>
            /// Handles the case that the cursor entered a layer object so that word highlights
            /// are displayed if the <see cref="WordHighlightManager"/> is active and the layer
            /// object is a word highlight.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="Extract.Imaging.Forms.LayerObjectEventArgs"/>
            /// instance containing the event data.</param>
            void HandleCursorEnteredLayerObject(object sender, LayerObjectEventArgs e)
            {
                try
                {
                    // If a previously open image has closed ignore this event.
                    if (!_imageViewer.IsImageAvailable)
                    {
                        return;
                    }

                    // Check to see if the specified layer object is a word highlight
                    if (IsWordHighlight(e.LayerObject))
                    {
                        // Display the highlight (border) if the highlight is of the miniumum
                        // height.
                        Highlight highlight = (Highlight)e.LayerObject;
                        if (highlight.Height >= _MIN_SPLIT_HEIGHT)
                        {
                            // If in an auto-fit operation, keep track of the layer object added to
                            // the selection in case the user switches back to non-auto-fit mode,
                            // but do not show it at this time.
                            highlight.Visible = !InAutoFitMode;

                            _activeWordHighlights.Add(e.LayerObject);

                            // If not in a tracking operation, invalidate the image viewer to
                            // force the highlight to be drawn. (If in a tracking operation, the
                            // image viewer will call invalidate after all highlights and tracking
                            // indications have been prepared.)
                            if (!InAutoFitMode && !_imageViewer.IsTracking)
                            {
                                _imageViewer.Invalidate();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Display("ELI31293", ex);
                }
            }

            /// <summary>
            /// Handles the case that the cursor left a layer object so that, if it is a
            /// word highlight it can be hidden.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="Extract.Imaging.Forms.LayerObjectEventArgs"/>
            /// instance containing the event data.</param>
            void HandleCursorLeftLayerObject(object sender, LayerObjectEventArgs e)
            {
                try
                {
                    // If a previously open image has closed ignore this event.
                    if (!_imageViewer.IsImageAvailable)
                    {
                        return;
                    }

                    // Check if the layer object is a currently displayed word highlight.
                    if (_activeWordHighlights.Contains(e.LayerObject))
                    {
                        _activeWordHighlights.Remove(e.LayerObject);

                        // Hide the word highlight and ensure the color is set back to white (rather
                        // than the redaction/highlight color).
                        Highlight highlight = (Highlight)e.LayerObject;
                        highlight.Visible = false;

                        _imageViewer.Invalidate();
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Display("ELI31294", ex);
                }
            }

            /// <summary>
            /// Handles the deleting layer objects event.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="Extract.Imaging.Forms.DeletingLayerObjectsEventArgs"/>
            /// instance containing the event data.</param>
            void HandleDeletingLayerObjects(object sender, DeletingLayerObjectsEventArgs e)
            {
                bool restartLoading = false;

                try
                {
                    // Before accessing fields that may be modifed by the background worker, stop
                    // any currently running operation on the background worker.
                    lock (_lock)
                    {
                        restartLoading = IsLoadingData;

                        // false to allow OCR operations to continue.
                        CancelRunningOperation(false);
                    }

                    if (!_backgroundWorkerIdle.WaitOne(1000))
                    {
                        new ExtractException("ELI31375",
                            "Application trace: Word highlight background operation aborted.").Log();
                    }

                    // Get a set of the layer objects being deleted that are word highlights.
                    HashSet<LayerObject> deletedWordHighlights =
                        new HashSet<LayerObject>(e.LayerObjects
                            .Where(o => IsWordHighlight(o)));

                    // If any word highlights are deleted by anything but the WordHighlightManager,
                    // reload the highlights for the affected pages.
                    if (deletedWordHighlights.Count > 0)
                    {
                        restartLoading |= true;

                        // Collect a list of all pages from which highlights are being deleted and all
                        // word highlights from those pages.
                        IEnumerable<int> affectedPages = deletedWordHighlights
                            .Select(h => h.PageNumber)
                            .Distinct();
                        List<LayerObject> highlightsFromAffectedPages = new List<LayerObject>();
                        foreach (int page in affectedPages)
                        {
                            HashSet<LayerObject> pageHighlights;
                            ExtractException.Assert("ELI31363", "Internal error.",
                                _wordHighlights.TryRemove(page, out pageHighlights));
                            highlightsFromAffectedPages.AddRange(pageHighlights);

                            // No longer consider any of these highlights as loaded.
                            _pagesOfAddedWordHighlights.Remove(page);
                            foreach (Highlight highlight in pageHighlights)
                            {
                                _wordLineMapping.Remove(highlight);

                                if (!_imageViewer.RedactionMode)
                                {
                                    // _highlightOcr will only be populated when not in RedactionMode.
                                    ThreadSafeSpatialString temp;
                                    _highlightOcr.TryRemove(highlight, out temp);
                                }
                            }

                            if (!_imageViewer.RedactionMode)
                            {
                                // _loadedOcrWords will only be populated when not in RedactionMode.
                                List<ThreadSafeSpatialString> ocrWords;
                                ExtractException.Assert("ELI34064", "Internal error.",
                                    _loadedOcrWords.TryRemove(page, out ocrWords));
                            }
                        }

                        // Look for any highlights on the affected pages that aren't already
                        // being deleted and delete them so that word highlights aren't duplicated
                        // when they are re-loaded.
                        List<LayerObject> remainingHighlights =
                            new List<LayerObject>(highlightsFromAffectedPages
                                .Where(h => !deletedWordHighlights.Contains(h)));

                        if (remainingHighlights.Count > 0)
                        {
                            _imageViewer.LayerObjects.Remove(remainingHighlights, true, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Display("ELI31320", ex);
                }
                finally
                {
                    // Call activate to ensure we pick up loading word highlights again if a loading
                    // task was canceled or auto-fit mode was active (which would have blocked word
                    // loading.
                    if (restartLoading || InAutoFitMode)
                    {
                        Activate();
                    }
                }
            }

            /// <summary>
            /// Handles the case that the background worker thread completed.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="System.ComponentModel.RunWorkerCompletedEventArgs"/>
            /// instance containing the event data.</param>
            void BackgroundWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                try
                {
                    if (_restartBackgroundWorker)
                    {
                        // Start a new worker thread if a restart was requested.
                        _restartBackgroundWorker = false;
                        StartWorkerThread();
                    }
                    else
                    {
                        // Otherwise indicate that there is no longer any background loading going
                        // on.
                        _loadingPage = -1;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Display("ELI31377", ex);
                }
            }

            #endregion Event Handlers

            #region IDisposable Members

            /// <summary>
            /// Releases all resources used by the <see cref="WordHighlightManager"/>.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <overloads>Releases resources used by the <see cref="WordHighlightManager"/>.
            /// </overloads>
            /// <summary>
            /// Releases all unmanaged resources used by the <see cref="WordHighlightManager"/>. 
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
            /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
            void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // Dispose of managed resources
                    try
                    {
                        _disposed = true;

                        if (_backgroundWorker != null)
                        {
                            Deactivate(false);
                            _backgroundWorkerIdle.WaitOne(5000);

                            _backgroundWorker.Dispose();
                            _backgroundWorker = null;
                        }

                        ClearData();

                        if (_ocrManager != null)
                        {
                            _ocrManager.Dispose();
                            _ocrManager = null;
                        }

                        if (_backgroundWorkerIdle != null)
                        {
                            _backgroundWorkerIdle.Dispose();
                            _backgroundWorkerIdle = null;
                        }

                        if (_operationAvailable != null)
                        {
                            _operationAvailable.Dispose();
                            _operationAvailable = null;
                        }

                        if (_canceler != null)
                        {
                            _canceler.Dispose();
                            _canceler = null;
                        }

                        if (_ocrCanceler != null)
                        {
                            _ocrCanceler.Dispose();
                            _ocrCanceler = null;
                        }

                        if (_ocrPageComplete != null)
                        {
                            _ocrPageComplete.Dispose();
                            _ocrPageComplete = null;
                        }

                        // Not necessary since this happens in ClearData, but it appeases FXCop.
                        if (_autoFitHighlight != null)
                        {
                            _autoFitHighlight.Dispose();
                            _autoFitHighlight = null;
                        }
                    }
                    catch { }
                }

                // Dispose of ummanaged resources
            }

            #endregion IDisposable Members

            #region Private Members

            /// <summary>
            /// Starts loading and tracking word highlights.
            /// </summary>
            void Activate()
            {
                try
                {
                    lock (_lock)
                    {
                        // Word highlights need be enabled only if the word highlight/redaction tool
                        // is active.
                        bool enableHighlights =
                            (_imageViewer.CursorTool == CursorTool.WordHighlight ||
                             _imageViewer.CursorTool == CursorTool.WordRedaction);

                        if (enableHighlights)
                        {
                            EnableWordHighlights();
                        }
                        else
                        {
                            DisableWordHighlights();
                        }

                        // Ensure there is a background worker thread active to run the loading operation.
                        StartWorkerThread();

                        // In case a running operation is cancelled by a message handler, set
                        // _loadingPage = 0 indicate that a loading operation has been started so it
                        // can be resumed after the handler is complete.
                        _loadingPage = 0;

                        // Start a new loading task for the current image viewer page (canceling any
                        // other operation currently in progress.
                        StartOperation(() => LoaderOperation());
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31291", ex);
                }
            }

            /// <summary>
            /// Stops loading and managing word highlights.
            /// </summary>
            /// <param name="clearData"><see langword="true"/> to clear and dispose of all loaded
            /// data after deactivating, <see langword="false"/> otherwise.</param>
            void Deactivate(bool clearData)
            {
                try
                {
                    lock (_lock)
                    {
                        DisableWordHighlights();

                        _clearData |= clearData;

                        if (clearData)
                        {
                            // Ensure _trackingStartLocation is reset in the UI thread since a
                            // nullable cannot be volatile.
                            _trackingStartLocation = null;
                        }

                        if (_backgroundWorker != null && _backgroundWorker.IsBusy)
                        {
                            // Call cancel on the background worker so the background thread will end
                            // after any currently running operation is complete.
                            _backgroundWorker.CancelAsync();

                            // Signal any currently running operation to cancel.
                            // (including OCR operations).
                            CancelRunningOperation(true);

                            // In case the background thread is currently waiting on the next
                            // operation, set _operationAvailable to end the wait.
                            _operationAvailable.Set();
                        }
                        else if (_clearData)
                        {
                            // If the worker thread is not running, but the data is to be cleared,
                            // do it here.
                            ClearData();
                        }
                    }

                    // If deactivating, clear any background status message.
                    if (clearData)
                    {
                        UpdateBackgroundProgressStatus(-1, "", 0);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31292", ex);
                }
            }

            /// <summary>
            /// Enables the word highlights.
            /// </summary>
            void EnableWordHighlights()
            {
                if (!_highlightsEnabled)
                {
                    _highlightsEnabled = true;

                    InAutoFitMode = (_imageViewer.Cursor == ExtractCursors.ShiftWordRedaction ||
                                     _imageViewer.Cursor == ExtractCursors.ShiftWordHighlight);

                    // If word highlights are active, watch for when the cursor
                    // enters/leaves word highlights.
                    _imageViewer.CursorChanged += HandleCursorChanged;
                    _imageViewer.CursorEnteredLayerObject +=
                        HandleCursorEnteredLayerObject;
                    _imageViewer.CursorLeftLayerObject +=
                        HandleCursorLeftLayerObject;
                    _imageViewer.LayerObjects.DeletingLayerObjects +=
                        HandleDeletingLayerObjects;
                }
            }

            /// <summary>
            /// Disables the word highlights.
            /// </summary>
            void DisableWordHighlights()
            {
                if (_highlightsEnabled)
                {
                    _highlightsEnabled = false;

                    // Stop watching for when the cursor enters/leaves word highlights
                    _imageViewer.CursorChanged -= HandleCursorChanged;
                    _imageViewer.CursorEnteredLayerObject -=
                        HandleCursorEnteredLayerObject;
                    _imageViewer.CursorLeftLayerObject -=
                        HandleCursorLeftLayerObject;
                    _imageViewer.LayerObjects.DeletingLayerObjects -=
                        HandleDeletingLayerObjects;

                    HideWordHighlights(true);
                }
            }

            /// <summary>
            /// Starts the worker thread if it is not already running.
            /// </summary>
            void StartWorkerThread()
            {
                // Create the background worker if it has not yet been created.
                if (_backgroundWorker == null)
                {
                    _backgroundWorker = new BackgroundWorker();
                    _backgroundWorker.WorkerSupportsCancellation = true;
                    _backgroundWorker.DoWork += WorkerThread;
                    _backgroundWorker.RunWorkerCompleted += BackgroundWorkerCompleted;
                }

                // If the background worker is not currently running, start it.
                if (_backgroundWorker.IsBusy)
                {
                    // If the runnning working thread has been canceled, request a new worker to
                    // start once the current one has completed.
                    if (_backgroundWorker.CancellationPending)
                    {
                        _restartBackgroundWorker = true;
                    }
                }
                else
                {
                    _backgroundWorker.RunWorkerAsync();
                }
            }

            /// <summary>
            /// Cancels any currently running operation and starts the specified
            /// <see paramref="operation"/> instead.
            /// </summary>
            /// <param name="operation">The <see cref="Action"/> to perform.</param>
            void StartOperation(Action operation)
            {
                // Cancel any currently running operation except for OCR
                CancelRunningOperation(false);

                // Schedule the new operation.
                _pendingOperation = operation;

                // Signal the _backgroundWorker that an operation is available.
                _operationAvailable.Set();
            }

            /// <summary>
            /// Cancels the actively running task (if there is one).
            /// <para><b>Note</b></para>
            /// This method must be called within a lock.
            /// </summary>
            /// <param name="cancelOcr"><see langword="true"/> if OCR operations should be canceled
            /// as well; <see langword="false"/> if they are allowed to continue.</param>
            void CancelRunningOperation(bool cancelOcr)
            {
                // Cancel any currently running operation.
                if (_canceler != null && !_canceler.IsCancellationRequested)
                {
                    _canceler.Cancel();
                }

                if (cancelOcr && _ocrCanceler != null && !_ocrCanceler.IsCancellationRequested)
                {
                    _ocrCanceler.Cancel();
                }

                _pendingOperation = null;
            }

            /// <summary>
            /// Encapsulates the lifespan of the _backgroundWorker thread. This thread will
            /// continually loop to perform any scheduled background operation for the word
            /// highlight/redaction tool. If no operation is available, as long as the tool is
            /// active the thread will wait in an idle state for an operation to be requested.
            /// </summary>
            void WorkerThread(object sender, DoWorkEventArgs e)
            {
                try
                {
                    // The only case in which the thread should not continue to loop is if the worker
                    // itself has been canceled.
                    while (_backgroundWorker != null && !_backgroundWorker.CancellationPending)
                    {
                        try
                        {
                            // Check for an available operation.
                            if (!_operationAvailable.WaitOne(0))
                            {
                                // If there is no pending operation, the background worker is idle.
                                _backgroundWorkerIdle.Set();

                                _operationAvailable.WaitOne();
                            }

                            // In case of a cancelled operation, give any pending UI operations every
                            // opportunity possible to cancel. (We cannot call DoEvents or wait
                            // indefinetly since that could cause a deadlock).
                            for (int i = 0; Interlocked.Read(ref _executeInUIReferenceCount) > 0; i++)
                            {
                                Thread.Sleep(50);

                                // Give up after a second.
                                if (i > 20)
                                {
                                    // In this case, reset the canceler so that it is not disposed of
                                    // so that its cancel token can still be checked if and when the
                                    // UI operation is finally run.
                                    _canceler = null;
                                    new ExtractException("ELI31371",
                                        "Application trace: Word highlight background operation aborted.").Log();
                                    break;
                                }
                            }

                            // Initialize the next operation to perform.
                            Action currentOperation;
                            lock (_lock)
                            {
                                _operationAvailable.Reset();

                                if (_clearData)
                                {
                                    ClearData();
                                }

                                if (_pendingOperation == null || _backgroundWorker.CancellationPending)
                                {
                                    continue;
                                }

                                _backgroundWorkerIdle.Reset();

                                currentOperation = _pendingOperation;
                                _pendingOperation = null;

                                if (_canceler != null)
                                {
                                    _canceler.Dispose();
                                }
                                _canceler = new CancellationTokenSource();
                                _cancelToken = _canceler.Token;
                            }

                            // Perform the operation.
                            if (currentOperation != null)
                            {
                                currentOperation();
                            }
                        }
                        catch (OperationCanceledException)
                        { }
                        catch (Exception ex)
                        {
                            // If the task threw an exception but a cancel has been requested, assume
                            // the exception resulted from attempting an operation that should not have
                            // occured after cancelation (for example, accessing the current page number
                            if (!_cancelToken.IsCancellationRequested)
                            {
                                string message = _imageViewer.RedactionMode
                                    ? "Error loading data for word highlight tool."
                                    : "Error loading data for word redaction tool.";

                                // Display any non-cancelation exception so the user is notified right away
                                // instead of when the page is changed or the document is closed.
                                ExtractException ee = new ExtractException("ELI31365", message, ex);

                                _imageViewer.Invoke((MethodInvoker)(() => ee.Display()));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Log("ELI32628", ex);
                }
                finally
                {
                    // If the background worker has been stopped, consider the background worker idle.
                    _backgroundWorkerIdle.Set();
                }
            }

            /// <summary>
            /// Handles the case that _ocrManager has updated the progress of an on-going OCR
            /// operation.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="Extract.Imaging.OcrProgressUpdateEventArgs"/> instance containing the event data.</param>
            void HandleOcrProgressUpdate(object sender, OcrProgressUpdateEventArgs e)
            {
                try
                {
                    string status = string.Format(CultureInfo.CurrentCulture, "OCR: {0:P0}",
                        e.ProgressPercent);

                    UpdateBackgroundProgressStatus(_loadingPage, status, e.ProgressPercent);
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI32622");
                }
            }

            /// <summary>
            /// Gets the number of pages for which data has been loaded by the background operation.
            /// This value depends on whether word highlights are enabled.
            /// </summary>
            int PagesLoaded
            {
                get
                {
                    if (_imageViewer.IsImageAvailable)
                    {
                        return _highlightsEnabled
                            ? _wordHighlights.Count
                            : _ocrPageData
                                .Where(data => data.Value != null)
                                .Count();
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            /// <summary>
            /// Clear and disposes of all loaded data.
            /// <para><b>Note</b></para>
            /// This method must be called from within a lock.
            /// </summary>
            void ClearData()
            {
                if (_autoFitHighlight != null)
                {
                    _autoFitHighlight.Dispose();
                    _autoFitHighlight = null;
                }

                foreach (HashSet<LayerObject> pageOfHighlights in _wordHighlights.Values)
                {
                    foreach (Highlight highlight in pageOfHighlights)
                    {
                        highlight.Dispose();
                    }
                }

                _pagesOfAddedWordHighlights.Clear();
                _wordHighlights.Clear();
                _loadedOcrWords.Clear();
                _highlightOcr.Clear();
                _pageStatusMessages.Clear();
                _wordLineMapping.Clear();
                _averageLineHeight.Clear();
                _pageCount = 0;
                _ocrPageData.Clear();
                _ocrData = null;

                _clearData = false;
            }

            /// <summary>
            /// Encapsulates a task to calculate an angular highlight automatically sized to pixel
            /// content using the start and end point of the tracking operation as the start and
            /// end point of the raster zone.
            /// </summary>
            /// <param name="startPoint">The start <see cref="Point"/> of the auto-fit calculation.
            /// </param>
            /// <param name="endPoint">The end <see cref="Point"/> of the auto-fit calculation.
            /// </param>
            void AutoFitOperation(Point startPoint, Point endPoint)
            {
                PixelProbe probe = null;

                try
                {
                    // NOTE:
                    // This method executes on a background thread. While it is guaranted that only
                    // one instance of this method will run at any given time, any operations that
                    // occur in this method should be thread-safe with respect to the UI thread and
                    // should be tolerant to document/page changes that could occur in the UI thread.
                    // Code that needs to be run on the UI thread can be run using ExecuteInUIThread.

                    // Attempt to retrieve the image page currently being displayed and a PixelProbe
                    // for it. If unable to, an image isn't currently available and the operation
                    // should be aborted.
                    int page = -1;
                    ExecuteInUIThread(() =>
                    {
                        page = _imageViewer.PageNumber;
                        probe = _imageViewer._reader.CreatePixelProbe(page);
                    });
                    if (page == -1 || probe == null)
                    {
                        return;
                    }

                    // Attempt to find an already calculated average line height for the current page.
                    int averageLineHeight = 0;
                    if (!_averageLineHeight.TryGetValue(page, out averageLineHeight))
                    {
                        if (_ocrData != null && _ocrData.SpatialString.HasSpatialInfo())
                        {
                            // Retrieve an IUnknownVector of SpatialStrings representing the words on the page.
                            SpatialString pageData =
                                _ocrData.SpatialString.GetSpecifiedPages(page, page);

                            // Base the height limit on the average line height for the page.
                            averageLineHeight = pageData.GetAverageLineHeight() * 5;
                        }

                        // Ensure the average line height is at least 30 pixels.
                        if (averageLineHeight < 30)
                        {
                            averageLineHeight = 30;
                        }

                        _averageLineHeight[page] = averageLineHeight;
                    }

                    // Base the auto-fit zone max height and fuzzy edge buffer on the line height.
                    int zoneHeightLimit = (int)(averageLineHeight * _AUTO_FIT_MAX_HEIGHT);
                    int fuzzyEdgeBuffer = (int)(averageLineHeight * _AUTO_FIT_FUZZY_BUFFER);

                    _cancelToken.ThrowIfCancellationRequested();

                    // Generate a 1 pixel height raster zone as the basis of the fitting
                    // operation.
                    RasterZone rasterZone = new RasterZone(startPoint, endPoint, 1, page);

                    // Create a new FittingData instance and try to find the top edge of pixel
                    // content. Allow for an edge to be found using "fuzzy" logic.
                    ZoneGeometry data = new ZoneGeometry(rasterZone, _cancelToken);
                    if (data.FitEdge(Side.Top, probe, false, false, _AUTO_FIT_FUZZY_FACTOR,
                        fuzzyEdgeBuffer, 0, 0, zoneHeightLimit))
                    {
                        int remainingHeight = zoneHeightLimit - (int)data.Height;

                        // If a top edge was found, search downward for an opposing edge.
                        if (remainingHeight > 0 &&
                            data.FitEdge(Side.Bottom, probe, false, false, _AUTO_FIT_FUZZY_FACTOR,
                                fuzzyEdgeBuffer, 0, 0, remainingHeight) &&
                            data.Height >= _MIN_SPLIT_HEIGHT)
                        {
                            // Shrink the left and right side to fit pixel content.
                            data.FitEdge(Side.Left, probe, buffer: 0F);
                            data.FitEdge(Side.Right, probe, buffer: 0F);

                            // Generate the new auto-fitted highlight.
                            Highlight highlight = new Highlight(_imageViewer, "",
                                data.ToRasterZone(RoundingMode.Safe), "",
                                _imageViewer.GetHighlightDrawColor());
                            highlight.Selectable = false;
                            highlight.CanRender = false;
                            highlight.Inflate((float)_registry.Settings.AutoFitZonePadding + 1,
                                RoundingMode.Safe, false);

                            // Add the new auto-fit highlight
                            ExecuteInUIThread(() =>
                            {
                                RemoveAutoFitHighlight();
                                AddAutoFitHighlight(highlight);
                            });

                            return;
                        }
                    }

                    // If a new auto-fit highlight wasn't able to be created, ensure the start and
                    // end point pass though an already existing auto-fit highlight. If they don't,
                    // remove it.
                    if (_autoFitHighlight != null)
                    {
                        ZoneGeometry autoZoneFittingData =
                            new ZoneGeometry(_autoFitHighlight.ToRasterZone());
                        if (!autoZoneFittingData.LinePassesThrough(startPoint, endPoint))
                        {
                            ExecuteInUIThread(() => RemoveAutoFitHighlight());
                        }
                    }
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31352", ex);
                }
                finally
                {
                    if (probe != null)
                    {
                        probe.Dispose();
                    }
                }
            }

            /// <summary>
            /// Encapsulates a task to load and add highlights for all words on a document from OCR
            /// data. While the tasks will load and create highlights for all words in the document,
            /// in order to reduce demand on the image viewer for documents with many pages,
            /// it will only add the highlights to the current page. To add highights to other
            /// pages, another tasks instance should be run when the image viewer page is changed
            /// (this task will also remove highlights from all pages except the current page).
            /// </summary>
            void LoaderOperation()
            {
                try
                {
                    // NOTE:
                    // This method executes on a background thread. While it is guaranted that only
                    // one instance of this method will run at any given time, any operations that
                    // occur in this method should be thread-safe with respect to the UI thread and
                    // should be tolerant to document/page changes that could occur in the UI thread.
                    // Code that needs to be run on the UI thread can be run using ExecuteInUIThread.

                    // Attempt to retrieve the image page currently being displayed and the total
                    // page count. If unable to, an image isn't currently available and the
                    // operation should be aborted.
                    int startingPage = -1;
                    bool autoOcr = false;

                    ExecuteInUIThread(() =>
                    {
                        startingPage = _imageViewer.PageNumber;
                        autoOcr = _imageViewer.AutoOcr;
                        _pageCount = _imageViewer.PageCount;

                        // Retrieve any OCR data the image viewer has for the current document.
                        _ocrData = _ocrData ?? _imageViewer.OcrData;
                    });

                    // If there is no page loaded or no OCR data with autoOcr turned off, there is
                    // nothing more to be done.
                    if (startingPage == -1 || _pageCount == -1 ||
                        (_ocrData == null && !autoOcr))
                    {
                        UpdateBackgroundProgressStatus(-1, "OCR unavailable", 0.0);

                        return;
                    }

                    // Loop through all pages starting at the current page to ensure highlights for
                    // the current page are loaded first.
                    _loadingPage = startingPage;
                    do
                    {
                        _cancelToken.ThrowIfCancellationRequested();

                        // Retrieve OCR data for the current page, or OCR if appropriate.
                        SpatialString pageOcr = LoadOcrDataForPage(_loadingPage);

                        _cancelToken.ThrowIfCancellationRequested();

                        // If word highlights are enabled and we have OCR data, load word highlights.
                        if (_highlightsEnabled && pageOcr != null)
                        {
                            UpdateBackgroundProgressStatus(_loadingPage, "Loading word zones...", 1.0);

                            // Create highlights for each word on the page (if they have not already
                            // been created).
                            HashSet<LayerObject> wordHighlights =
                                LoadWordHighlightsForPage(_loadingPage, pageOcr);

                            ExecuteInUIThread(() =>
                            {
                                if (_loadingPage == startingPage)
                                {
                                    // If this is the current page, add the word highlights to the
                                    // page if they have not already been added.
                                    AddWordLayerObjects(_loadingPage, wordHighlights);
                                }
                                else
                                {
                                    // If this is not the current page, but highlights were
                                    // previously added to the page, remove them to prevent bogging
                                    // down the image viewer on documents with lots of pages.
                                    RemoveWordLayerObjects(_loadingPage);
                                }
                            });
                        }

                        UpdateBackgroundProgressStatus(_loadingPage,
                            (pageOcr == null) ? "OCR unavailable" : "OCR available", 0.0);

                        // Increment the page number (looping back to the first page if necessary).
                        _loadingPage = (_loadingPage < _pageCount) ? _loadingPage + 1 : 1;
                    }
                    while (_loadingPage != startingPage);

                    // If OCR data was not initially available, now that data for all pages has been
                    // loaded, store the complete document OCR for access by the _imageViewer.
                    if (_ocrData == null)
                    {
                        _ocrData = new ThreadSafeSpatialString(_imageViewer,
                            _ocrPageData
                                .OrderBy(entry => entry.Key)
                                .Select(entry => entry.Value));

                        OnOcrLoaded(_ocrData);
                    }
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                {
                    // If processing was not cancelled, loading failed.
                    if (!_cancelToken.IsCancellationRequested)
                    {
                        UpdateBackgroundProgressStatus(_loadingPage, "OCR unavailable", 0.0);
                    }
                    throw ExtractException.AsExtractException("ELI31298", ex);
                }
                finally
                {
                    if (!_cancelToken.IsCancellationRequested)
                    {
                        _loadingPage = -1;
                    }
                }
            }

            /// <summary>
            /// Gets OCR data for the current page (if available).
            /// </summary>
            /// <param name="page"></param>
            /// <returns>A <see cref="SpatialString"/> representing the OCR data for the specified
            /// <see paramref="page"/>.</returns>
            SpatialString LoadOcrDataForPage(int page)
            {
                // NOTE:
                // This method executes on a background thread. While it is guaranted that only
                // one instance of this method will run at any given time, any operations that
                // occur in this method should be thread-safe with respect to the UI thread and
                // should be tolerant to document/page changes that could occur in the UI thread.
                // Code that needs to be run on the UI thread can be run using ExecuteInUIThread.

                SpatialString pageOcr = null;
                ThreadSafeSpatialString ocrData;
                if (_ocrPageData.TryGetValue(page, out ocrData) && ocrData != null)
                {
                    pageOcr = ocrData.SpatialString;
                }
                else
                {   
                    // If OCR data is available for the document as a whole, simply grab the page
                    // needed.
                    if (_ocrData != null && _ocrData.SpatialString.HasSpatialInfo())
                    {
                        pageOcr = _ocrData.SpatialString.GetSpecifiedPages(page, page);

                        if (pageOcr != null)
                        {
                            // Cache any valid data for the page.
                            _ocrPageData[page] = new ThreadSafeSpatialString(_imageViewer, pageOcr);
                        }
                    }
                    // Otherwise attempt to OCR data if _imageViewer.AutoOcr is on.
                    else if (_ocrData == null)
                    {
                        bool autoOcr = false;
                        OcrTradeoff ocrTradeoff = OcrTradeoff.Balanced;
                        string imageFile = string.Empty;

                        if (LicenseUtilities.IsLicensed(LicenseIdName.OcrOnClientFeature))
                        {
                            ExecuteInUIThread(() =>
                            {
                                autoOcr = _imageViewer.AutoOcr;
                                ocrTradeoff = _imageViewer.OcrTradeoff;
                                imageFile = _imageViewer.ImageFile;
                            });
                        }

                        if (autoOcr)
                        {
                            // Launch an asynchronous task to schedule the OCR operation. This
                            // allows us to instantly move on if cancelled and potentially also
                            // allows the OCR operation to continue so that the results can be
                            // retrieved later.
                            Task ocrTask = Task.Factory.StartNew(() =>
                                pageOcr = OCRPage(imageFile, page, ocrTradeoff));

                            try
                            {
                                // Wait for completion as long as we don't receive a cancel request.
                                ocrTask.Wait(_cancelToken);
                            }
                            catch (OperationCanceledException)
                            {
                                // If canceled, use ContinueWith to ensure any exceptions are
                                // handled (which would otherwise crash the app).
                                ocrTask.ContinueWith((canceledTask) =>
                                    {
                                        if (!canceledTask.Exception.InnerExceptions
                                                .Where(ex => ex is OperationCanceledException ||
                                                    ex.Message.Contains("cancel"))
                                                .Any())
                                        {
                                            ExtractException ee = new ExtractException("ELI33373",
                                                "OCR Operation failed.", canceledTask.Exception);
                                            ee.AddDebugData("Document", imageFile, false);
                                            ee.AddDebugData("Page", page, false);
                                            ee.Log();
                                        }

                                        canceledTask.Dispose();
                                    }, TaskContinuationOptions.OnlyOnFaulted);
                            }
                            catch (Exception ex)
                            {
                                ExtractException ee = new ExtractException("ELI33374",
                                                "OCR Operation failed.", ex);
                                ee.AddDebugData("Document", imageFile, false);
                                ee.AddDebugData("Page", page, false);
                                throw  ee;
                            }
                        }
                    }
                }

                if (pageOcr != null && pageOcr.HasSpatialInfo())
                {
                    return pageOcr;
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Performs OCR on the specified <see paramref="imageFile"/> and
            /// <see paramref="pageNumber"/>.
            /// </summary>
            /// <param name="imageFile">The image file to be OCR'd.</param>
            /// <param name="pageNumber">The page number to be OCR'd.</param>
            /// <param name="ocrTradeoff">The <see cref="OcrTradeoff"/> indicating the quality/speed
            /// tradeoff to use.</param>
            /// <returns>
            /// A <see cref="SpatialString"/> instance representing the OCR data for
            /// the specified <see paramref="imageFile"/> and <see paramref="pageNumber"/>.
            /// </returns>
            // Throwing an OperationCanceledException with an explicit but unreferenced argument.
            // If throw is used with no exception, it is not thrown out as an
            // OperationCanceledException and catch statements intended to catch it won't work.
            SpatialString OCRPage(string imageFile, int pageNumber, OcrTradeoff ocrTradeoff)
            {
                // NOTE:
                // This method executes on a background thread. While it is guaranted that only
                // one instance of this method will run at any given time, any operations that
                // occur in this method should be thread-safe with respect to the UI thread and
                // should be tolerant to document/page changes that could occur in the UI thread.
                // Code that needs to be run on the UI thread can be run using ExecuteInUIThread.

                CancellationTokenSource cancelTokenSource = null;
                CancellationToken cancelToken;
                bool pageAlreadyLoading = false;

                try
                {
                    // Set up a new OCR operation.
                    lock (_ocrLock)
                    {
                        // First check to be sure the OCR results don't already exist or that the
                        // page isn't currently OCR'ing in another thread.
                        ThreadSafeSpatialString ocrData;
                        if (_ocrPageData.TryGetValue(pageNumber, out ocrData))
                        {
                            if (ocrData != null)
                            {
                                return ocrData.SpatialString;
                            }
                            else if (_ocrManager != null && _ocrCanceler != null)
                            {
                                // A null entry in _ocrPageData indicates an OCR operation is in
                                // progress for this page.
                                pageAlreadyLoading = true;
                            }
                        }

                        if (pageAlreadyLoading)
                        {
                            // If the page is already being OCR'd we'll just wait for that operation
                            // to complete.
                            cancelToken = _ocrCanceler.Token;
                        }
                        else
                        {
                            // Otherwise launch a new operation.
                            cancelTokenSource = new CancellationTokenSource();
                            _ocrCanceler = cancelTokenSource;
                            cancelToken = _ocrCanceler.Token;

                            if (_ocrManager == null)
                            {
                                _ocrManager = new AsynchronousOcrManager();
                                _ocrManager.OcrProgressUpdate += HandleOcrProgressUpdate;
                            }

                            _ocrManager.Tradeoff = ocrTradeoff;
                            _ocrManager.OcrFile(imageFile, pageNumber, pageNumber, cancelToken);

                            // Indicate that data is being loaded for this page.
                            _ocrPageData[pageNumber] = null;
                            _ocrPageComplete.Reset();
                        }
                    }

                    // If waiting for results from another thread, wait for _ocrPageComplete which
                    // signals that the results are available (if OCR succeeded).
                    if (pageAlreadyLoading)
                    {
                        _ocrPageComplete.WaitOne();

                        cancelToken.ThrowIfCancellationRequested();

                        // Retrieve the results, or re-start OCRPage if there are no results.
                        ThreadSafeSpatialString ocrData;
                        if (_ocrPageData.TryGetValue(pageNumber, out ocrData) && ocrData != null)
                        {
                            return ocrData.SpatialString;
                        }
                        else
                        {
                            // [FlexIDSCore:4944]
                            // If the page wasn't correctly OCR'd, the initial processing was likely
                            // cancelled which would have set _ocrPageComplete. Before retrying,
                            // ensure _ocrPageComplete is reset.
                            _ocrPageComplete.Reset();

                            // The risk of infinite resursion is small, and in the end would be
                            // stopped by a document change that would trip _ocrCanceler.
                            return OCRPage(imageFile, pageNumber, ocrTradeoff);
                        }
                    }
                    // Otherwise run the OCR operation outside of _ocrLock so that it can be
                    // interrupted by a new OCR operation.
                    else
                    {
                        _ocrManager.WaitForOcrCompletion();

                        lock (_ocrLock)
                        {
                            cancelToken.ThrowIfCancellationRequested();

                            // If the output is null but an exception wasn't thrown, the OCR operation
                            // was likely cancelled by a previous operation. Worst case, this will force
                            // another OCR attempt on a page without text.
                            if (string.IsNullOrEmpty(_ocrManager.OcrOutput))
                            {
                                throw new OperationCanceledException();
                            }

                            // We have valid OCR results. Cache them.
                            _ocrPageData[pageNumber] =
                                new ThreadSafeSpatialString(_imageViewer, _ocrManager.OcrOutput);
                        }

                        return _ocrPageData[pageNumber].SpatialString;
                    }
                }
                catch (OperationCanceledException)
                {
                    lock (_ocrLock)
                    {
                        try
                        {
                            // Ensure the flag that indicates OCR was being loaded for this page is
                            // cleared.
                            ThreadSafeSpatialString ocrData;
                            _ocrPageData.TryRemove(pageNumber, out ocrData);
                        }
                        catch (Exception ex2)
                        {
                            ex2.ExtractLog("ELI34140");
                        }
                    }

                    // [FlexIDSCore:4944]
                    // I am getting weird results when attempting throw either the caught exception
                    // or a new OperationCanceledException from here. At times, the outer scope was
                    // catching it as a base Exception instead and in other cases it never seemed
                    // to be caught at all. Simply returning null should produce the same effect as
                    // throwing an OperationCanceledException from here.
                    return null;
                }
                catch (Exception ex)
                {
                    lock (_ocrLock)
                    {
                        try
                        {
                            // Ensure the flag that indicates OCR was being loaded for this page is
                            // cleared.
                            ThreadSafeSpatialString ocrData;
                            _ocrPageData.TryRemove(pageNumber, out ocrData);
                        }
                        catch (Exception ex2)
                        {
                            ex2.ExtractLog("ELI34139");
                        }
                    }

                    throw ex.AsExtract("ELI32614");
                }
                finally
                {
                    lock (_ocrLock)
                    {
                        if (cancelTokenSource != null)
                        {
                            _ocrPageComplete.Set();

                            if (_ocrCanceler == cancelTokenSource)
                            {
                                _ocrCanceler = null;
                            }

                            cancelTokenSource.Dispose();
                        }
                    }
                }
            }

            /// <summary>
            /// Creates <see cref="Highlight"/>s for each word on the specified document page. If
            /// cancelled, this method will allow for the load process to resume at the same point
            /// it was previously canceled.
            /// </summary>
            /// <param name="page">The page to load.</param>
            /// <param name="pageOcr">A <see cref="SpatialString"/> representing the OCR data for
            /// the specified <see paramref="page"/>.</param>
            /// <returns>A <see cref="HashSet{T}"/> of <see cref="LayerObject"/>s representing the
            /// words on the specified page.</returns>
            HashSet<LayerObject> LoadWordHighlightsForPage(int page, SpatialString pageOcr)
            {
                // NOTE:
                // This method executes on a background thread. While it is guaranted that only
                // one instance of this method will run at any given time, any operations that
                // occur in this method should be thread-safe with respect to the UI thread and
                // should be tolerant to document/page changes that could occur in the UI thread.
                // Code that needs to be run on the UI thread can be run using ExecuteInUIThread.

                // Get the data grouped by lines.
                IUnknownVector pageLines = pageOcr.GetLines();
                int lineCount = pageLines.Size();

                // If there is no OCR data on the page, return null;
                if (lineCount == 0)
                {
                    return null;
                }

                Color highlightColor = _imageViewer.GetHighlightDrawColor();

                // Create or retrieve an existing HashSet to hold the words for the current page.
                HashSet<LayerObject> wordHighlights;
                if (!_wordHighlights.TryGetValue(page, out wordHighlights))
                {
                    wordHighlights = new HashSet<LayerObject>();
                    _wordHighlights[page] = wordHighlights;
                }

                // If not in RedactionMode, create or retrieve an existing list to hold the OCR for
                // words on the current page.
                List<ThreadSafeSpatialString> loadedOcrWords = null;
                if (!_imageViewer.RedactionMode)
                {
                    if (!_loadedOcrWords.TryGetValue(page, out loadedOcrWords))
                    {
                        loadedOcrWords = new List<ThreadSafeSpatialString>();
                        _loadedOcrWords[page] = loadedOcrWords;
                    }
                }

                // Keeps track of the starting point of each line in terms of the number of words
                // on the page before it.
                int lineStartIndex = 0;

                // Loop through the lines on the page
                for (int i = 0; i < lineCount; i++)
                {
                    // Get the words from this line.
                    SpatialString line = (SpatialString)pageLines.At(i);
                    IUnknownVector words = line.GetWords();
                    int wordCount = words.Size();

                    // This line has already been entirely loaded. Move onto the next.
                    if (wordHighlights.Count >= lineStartIndex + wordCount)
                    {
                        lineStartIndex += wordCount;
                        continue;
                    }

                    // In case some, but not all words were loaded for this line, determine the word
                    // to load next.
                    int nextWordToLoad = wordHighlights.Count - lineStartIndex;
                    
                    // Generate an int to uniquely identify this line on the document.
                    int lineIdentifier = (page << 16) | i;

                    // Loop through the words on the line
                    for (int j = nextWordToLoad; j < wordCount; j++)
                    {
                        _cancelToken.ThrowIfCancellationRequested();

                        // Get a raster zone for the word.
                        SpatialString word = (SpatialString)words.At(j);
                        IUnknownVector comRasterZones = word.GetOriginalImageRasterZones();

                        ExtractException.Assert("ELI31304", "Unexpected raster zone in OCR data.",
                            comRasterZones.Size() == 1);

                        ComRasterZone comRasterZone = comRasterZones.At(0) as ComRasterZone;

                        ExtractException.Assert("ELI31305", "Invalid raster zone in OCR data.",
                            comRasterZone != null);

                        RasterZone rasterZone = new RasterZone(comRasterZone);

                        // Use the raster zone to create a highlight.
                        Highlight highlight =
                            new Highlight(_imageViewer, "", rasterZone, "", highlightColor);
                        highlight.Selectable = false;
                        highlight.CanRender = false;
                        highlight.Visible = false;

                        if (_imageViewer.RedactionMode)
                        {
                            // [FlexIDSCore:4601]
                            // Until pixels are scanned to refine the borders of OCR coordinates when
                            // adding a redaction, the OCR coordinates may be a pixel too small in some
                            // cases. Therefore, pad the preview highlights by AutoFitZonePadding in
                            // each direction to give the user confidence that when the redaction is
                            // added it will properly cover the entire word.
                            highlight.Inflate((float)_registry.Settings.AutoFitZonePadding + 1,
                                RoundingMode.Safe, false);
                        }
                        else
                        {
                            // In highlighting mode, collect the OCR text associated with the
                            // highlight.
                            var threadSafeWord = new ThreadSafeSpatialString(_imageViewer, word);
                            loadedOcrWords.Add(threadSafeWord);
                            _highlightOcr[highlight] = threadSafeWord;
                        }

                        _wordLineMapping[highlight] = lineIdentifier;
                        wordHighlights.Add(highlight);
                    }

                    lineStartIndex += wordCount;
                }

                return wordHighlights;
            }

            /// <summary>
            /// Adds the specified <see paramref="method"/> to the UI's message queue so that it is
            /// processed by the UI without interrupting any active message handler. This method
            /// blocks until the method has been executed or until the current background operation
            /// has been canceled.
            /// <para><b>Note</b></para>
            /// This method cannot block indefinetly because the UI may be waiting on the background
            /// thread (ie, it must adhere to _cancelToken).
            /// </summary>
            /// <param name="method">The <see cref="Action"/> to execute in the UI thread.</param>
            void ExecuteInUIThread(Action method)
            {
                // To avoid scheduling to the UI unnecessarilly, check cancelation token first.
                _cancelToken.ThrowIfCancellationRequested();

                // Keep track of the fact that the method was scheduled to execute.
                Interlocked.Increment(ref _executeInUIReferenceCount);

                // Assign a local copy of the _cancelToken to check inside the invoke call so that
                // even if the worker thread is running a different operation by the time occurs
                // and, therefore, _cancelToken is now set to a different token, the Invoke can stil
                // know if the operation that launched it has been cancelled.
                CancellationToken cancelToken = _cancelToken;

                // Invoke to avoid modifying the imageViewer from outside the UI thread. Use begin
                // invoke so the operation isn't executed in the middle of another UI event.
                IAsyncResult result = _imageViewer.BeginInvoke((MethodInvoker)(() => 
                    {
                        try
                        {
                            // If the task was cancelled before invoked or no image is loaded do not
                            // execute the method.
                            if (!cancelToken.IsCancellationRequested &&
                                _imageViewer.IsImageAvailable)
                            {
                                method();
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractDisplay("ELI32209");
                        }
                        finally
                        {
                            // By decrementing _UIOperationReferenceCount as part of the invoke, we'll
                            // have record of whether the method was called, even if the blocking here
                            // was cancelled.
                            Interlocked.Decrement(ref _executeInUIReferenceCount);
                        }
                    }));

                WaitHandle[] waitHandles = new WaitHandle[] 
                {
                    result.AsyncWaitHandle,
                    _cancelToken.WaitHandle
                };

                // For thread safety of fields modified in the background thread don't return until
                // the method has been executed (or the operation has been canceled.
                WaitHandle.WaitAny(waitHandles);

                // If cancelled, end task immediately so it doesn't touch any fields used in the
                // invoke.
                _cancelToken.ThrowIfCancellationRequested();
            }

            /// <summary>
            /// Adds the specified <see cref="LayerObjects"/> to the <see cref="ImageViewer"/>.
            /// </summary>
            /// <param name="page">The page to which the highlights should be added.</param>
            /// <param name="wordHighlights">The highlights to add.</param>
            void AddWordLayerObjects(int page, IEnumerable<LayerObject> wordHighlights)
            {
                try
                {
                    // If the highlights have not already been added to the page, add them now.
                    if (wordHighlights != null && !_pagesOfAddedWordHighlights.Contains(page))
                    {
                        foreach (LayerObject layerObject in wordHighlights)
                        {
                            _imageViewer._layerObjects.Add(layerObject, false);
                        }

                        _pagesOfAddedWordHighlights.Add(page);
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Log("ELI31367", ex);
                }
            }

            /// <summary>
            /// Removes all word highlights from the specified page.
            /// </summary>
            /// <param name="page">The page for which word highlights should be removed from the
            /// image viewer.</param>
            void RemoveWordLayerObjects(int page)
            {
                try
                {
                    if (_pagesOfAddedWordHighlights.Contains(page))
                    {
                        _pagesOfAddedWordHighlights.Remove(page);

                        _imageViewer._layerObjects.Remove(_wordHighlights[page], false, false);
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Log("ELI31368", ex);
                }
            }

            /// <summary>
            /// Removes and disposes of any existing auto-fit highlight.
            /// </summary>
            /// <param name="highlight">The newly calculated auto-fit highlight to be added to the
            /// image viewer.</param>
            void AddAutoFitHighlight(Highlight highlight)
            {
                try
                {
                    // Ensure an auto-event tracking event is still active before adding the auto-fit
                    // highlight.
                    if (_imageViewer.IsTracking && InAutoFitMode)
                    {
                        _imageViewer._layerObjects.Add(highlight, false);
                        _autoFitHighlight = highlight;
                        _imageViewer.Invalidate();
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Log("ELI31369", ex);
                }
            }

            /// <summary>
            /// Removes and disposes of any existing auto-fit highlight.
            /// </summary>
            void RemoveAutoFitHighlight()
            {
                try
                {
                    _currentAutoFitLocation = null;

                    if (_autoFitHighlight != null)
                    {
                        if (_imageViewer.LayerObjects.Contains(_autoFitHighlight))
                        {
                            _imageViewer._layerObjects.Remove(_autoFitHighlight, true, false);
                        }
                        else
                        {
                            _autoFitHighlight.Dispose();
                        }

                        _autoFitHighlight = null;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Log("ELI31370", ex);
                }
            }

            /// <summary>
            /// Determines whether the specified layer object is a word highlight.
            /// </summary>
            /// <param name="layerObject">The layer object.</param>
            /// <returns>
            /// <see langword="true"/> if the specified layer object is a word highlight;
            /// otherwise, <see langword="false"/>.
            /// </returns>
            bool IsWordHighlight(LayerObject layerObject)
            {
                HashSet<LayerObject> wordHighlights;
                if (_wordHighlights.TryGetValue(layerObject.PageNumber, out wordHighlights))
                {
                    if (wordHighlights.Contains(layerObject))
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Adds a <see cref="Highlight"/>, <see cref="Redaction"/> or highlights OCR text in
            /// the image viewer based on the current tracking data.
            /// </summary>
            void CreateOutput<T>() where T : CompositeHighlightLayerObject
            {
                // Collect all raster zones from the active highlights.
                List<RasterZone> rasterZones = new List<RasterZone>();

                // The amount to potentially expand the raster zones to reach the edge of pixel
                // content.
                int horizontalExpandLimit = 2;
                int verticalExpandLimit = 2;

                // The highlights that are to be turned in highlights may be either 1 or
                // more word highlights or an auto fit highlight.
                List<RasterZone> zonesToHighlight = new List<RasterZone>();
                if (InAutoFitMode)
                {
                    RasterZone autoFitZone = GetAutoFitOutputZone();
                    if (autoFitZone != null)
                    {
                        if (_autoFitHighlight == null)
                        {
                            // If an auto-fit zone was not able to be found, we're simply using a
                            // zone of the default height from the start to the end point; no
                            // pixel-fitting is necessary.
                            rasterZones.Add(autoFitZone);
                        }
                        else
                        {
                            // For an auto-fit zone, search a little ways left and right for the
                            // edge of content to correct for the start/end tracking event not
                            // encapulating pixel content.
                            horizontalExpandLimit = 10;

                            zonesToHighlight.Add(autoFitZone);
                        }
                    }
                }
                else
                {
                    if (_imageViewer.RedactionMode)
                    {
                        zonesToHighlight.AddRange(GetWordOutputZones());
                    }
                    else
                    {
                        // Rather than create spatial zones, report the OCR content itelf to be
                        // highlighted via the OcrTextHighlighted event.
                        HighlightActiveOCRText();
                        return;
                    }
                }

                foreach (RasterZone zone in zonesToHighlight)
                {
                    // Attempt to adjust the fit of each raster zone so that they don't
                    // allow for any "leaked" pixels. This will also eliminate zones that
                    // aren't of the minimum height.
                    using (PixelProbe probe =
                        _imageViewer._reader.CreatePixelProbe(_imageViewer.PageNumber))
                    {
                        ZoneGeometry data = new ZoneGeometry(zone);

                        // Shrink the box such that there are [AutoFitZonePadding] rows of white
                        // pixels between the edge of the zone and the contained pixel content.
                        // (buffer is the distance from the first row of pixels, not the number of
                        // rows of pixels in between as with AutoFitZonePadding).
                        int buffer = _registry.Settings.AutoFitZonePadding + 1;

                        // Expand out up to 2 pixel in each direction looking for an all
                        // white row to ensure the zone has encapsulated all pixel content.
                        data.FitEdge(Side.Left, probe, false, false, null, 0, buffer, 0,
                            horizontalExpandLimit);
                        data.FitEdge(Side.Top, probe, false, false, null, 0, buffer, 0,
                            verticalExpandLimit);
                        data.FitEdge(Side.Right, probe, false, false, null, 0, buffer, 0,
                            horizontalExpandLimit);
                        data.FitEdge(Side.Bottom, probe, false, false, null, 0, buffer, 0,
                            verticalExpandLimit);

                        // Shrink any sides with excess space and eliminate zones that
                        // are too small.
                        RasterZone fittedZone = GetBlockFittedZone(data, probe);

                        if (fittedZone != null)
                        {
                            rasterZones.Add(fittedZone);
                        }
                    }
                }

                if (rasterZones.Count > 0)
                {
                    // Create the candidate layer object and get the candidate rasterZones to check
                    // against the current doc for duplicates.
                    CompositeHighlightLayerObject outputLayerObject = null;

                    try
                    {
                        if (_imageViewer.RedactionMode)
                        {
                            outputLayerObject = new Redaction(_imageViewer,
                                _imageViewer.PageNumber, LayerObject.ManualComment, rasterZones,
                                _imageViewer._defaultRedactionFillColor);
                        }
                        else
                        {
                            outputLayerObject = new CompositeHighlightLayerObject(_imageViewer,
                                    _imageViewer.PageNumber, LayerObject.ManualComment, rasterZones,
                                    _imageViewer._defaultHighlightColor);
                        }

                        rasterZones = new List<RasterZone>(outputLayerObject.GetRasterZones());

                        // In order to check if the potential output is a duplicate of something
                        // that already exists, get a list of all raster zones of existing
                        // highlights/redactions.
                        List<RasterZone> existingRasterZones = new List<RasterZone>(
                            _imageViewer.LayerObjects
                                .OfType<T>()
                                .Where(lo => lo.GetType() == typeof(T) && !IsWordHighlight(lo))
                                .SelectMany(h => h.GetRasterZones())
                                .Where(z => z.PageNumber == _imageViewer.PageNumber));

                        // If the candidate raster zones aren't all duplicates of existing
                        // redaction/highlights add it to the image viewer.
                        if (!rasterZones.All(z => existingRasterZones
                                .Any(e => e.Equals(z))))
                        {
                            _imageViewer._layerObjects.Add(outputLayerObject);
                            
                            // Set to null so it doesn't get disposed of.
                            outputLayerObject = null;
                        }
                    }
                    finally
                    {
                        if (outputLayerObject != null)
                        {
                            outputLayerObject.Dispose();
                        }
                    }
                }
            }

            /// <summary>
            /// Reports the OCR text associated with the active highlight(s) to be highlighted via
            /// the <see cref="OcrTextHighlighted"/> event.
            /// </summary>
            void HighlightActiveOCRText()
            {
                if (_activeWordHighlights.Count == 0)
                {
                    // If there are no active word highlights, there is nothing to do.
                    return;
                }

                // Populate pageOcrWords with the OCR content on the page in order to sort the OCR
                // content of the active highlights correctly in the output.
                int pageNumber = _activeWordHighlights.First().PageNumber;
                List<ThreadSafeSpatialString> pageOcrWords;
                ExtractException.Assert("ELI34065", "Internal error.",
                    _loadedOcrWords.TryGetValue(pageNumber, out pageOcrWords));
                int? lastWordLineNumber = null;

                // Generate a SpatialString by merging the OCR content of all active word highlights.
                SpatialString outputSpatialString = _activeWordHighlights
                    .Select(highlight => new Tuple<int, ThreadSafeSpatialString>(
                        _wordLineMapping[highlight], _highlightOcr[highlight]))
                    .OrderBy(tuple => pageOcrWords.IndexOf(tuple.Item2))
                    .Aggregate(new SpatialString(), (spatialString, next) =>
                    {
                        // For each active word, we have the line number of the word and a
                        // SpatialString representing its OCR content.
                        int nextLineNumber = next.Item1;
                        SpatialString nextSpatialString = next.Item2.SpatialString;

                        // If this is not the first word in the output, separate it from the
                        // previous word using either a space or a carriage return depending on
                        // whether it is on the same line as the previous word.
                        if (lastWordLineNumber.HasValue)
                        {
                            if (lastWordLineNumber.Value == nextLineNumber)
                            {
                                spatialString.AppendString(" ");
                            }
                            else
                            {
                                spatialString.AppendString("\r\n");
                            }
                        }

                        lastWordLineNumber = nextLineNumber;
                        spatialString.Append(nextSpatialString);

                        return spatialString;
                    });

                // Raise OcrTextHighlighted with the resulting SpatialString.
                var eventArgs = new OcrTextEventArgs(
                    new ThreadSafeSpatialString(_imageViewer, outputSpatialString));

                _imageViewer.OnOcrTextHighlighted(eventArgs);
            }

            /// <summary>
            /// Creates a auto-fit <see cref="RasterZone"/> based on the current tracking data if
            /// possible.
            /// </summary>
            /// <returns>The <see cref="RasterZone"/> if possible, otherwise <see langword="null"/>.
            /// </returns>
            RasterZone GetAutoFitOutputZone()
            {
                RasterZone autoFitZone = null;

                if (_autoFitHighlight != null)
                {
                    autoFitZone = _autoFitHighlight.ToRasterZone();
                }
                else if (_currentAutoFitLocation.HasValue)
                {
                    // Get the points of the tracking event's bisecting line
                    Point[] points = new Point[]
                    {
                        _trackingStartLocation.Value,
                        _currentAutoFitLocation.Value
                    };

                    // Convert the points from client to image coordinates.
                    GeometryMethods.InvertPoints(_imageViewer._transform, points);

                    // Compute the distance betwen the points.
                    int dX = points[0].X - points[1].X;
                    int dY = points[0].Y - points[1].Y;
                    double distance = Math.Sqrt(dX * dX + dY * dY);

                    // If the mouse hasn't moved at least 5 image pixels, disregard the
                    // operation.
                    if (distance > 5)
                    {
                        // Create a raster zone of the default height.
                        RasterZone defaultZone = new RasterZone(points[0], points[1],
                            _imageViewer.DefaultHighlightHeight, _imageViewer.PageNumber);

                        autoFitZone = defaultZone;
                    }
                }

                return autoFitZone;
            }

            /// <summary>
            /// Creates a collection of <see cref="RasterZone"/>s based on the words indicated by
            /// the current tracking data. Words on the same line will be combined into a single
            /// <see cref="RasterZone"/>
            /// </summary>
            /// <returns>The <see cref="RasterZone"/>s.</returns>
            IEnumerable<RasterZone> GetWordOutputZones()
            {
                List<RasterZone> wordOutputZones = new List<RasterZone>();

                // Group all selected words by line.
                Dictionary<int, List<RasterZone>> rasterZonesByLine =
                    new Dictionary<int, List<RasterZone>>();

                foreach (Highlight highlight in _activeWordHighlights)
                {
                    // In the process, make the highlights invisible again.
                    highlight.Visible = false;

                    // Retrieve the line ID for the word
                    int lineIdentifier = _wordLineMapping[highlight];

                    // Creates new set for this line if necessary.
                    List<RasterZone> lineRasterZones;
                    if (!rasterZonesByLine.TryGetValue(lineIdentifier, out lineRasterZones))
                    {
                        lineRasterZones = new List<RasterZone>();
                        rasterZonesByLine[lineIdentifier] = lineRasterZones;
                    }

                    lineRasterZones.Add(highlight.ToRasterZone());
                }

                // For each set of raster zones on a line, create a bounding raster zone that shares
                // the average angle of all the zones.
                foreach (List<RasterZone> lineRasterZones in rasterZonesByLine.Values)
                {
                    if (lineRasterZones.Count == 1)
                    {
                        // If there's only one zone on the line, simply use it.
                        wordOutputZones.Add(lineRasterZones[0]);
                    }
                    else
                    {
                        // Otherwise, find a bounding rectangle for the zones in a coordinate
                        // system aligned with the zones' average angle.
                        double angle;
                        RectangleF bounds =
                            RasterZone.GetAngledBoundingRectangle(lineRasterZones, out angle);

                        // Create a start and end point for the zone in the raster zones' coordinate
                        // system.
                        float verticalMidPoint = bounds.Top + bounds.Height / 2F;
                        PointF[] points = new PointF[]
                                    {
                                        new PointF(bounds.Left, verticalMidPoint),
                                        new PointF(bounds.Right, verticalMidPoint)
                                    };

                        // Translate these points into the image coordinate system.
                        using (Matrix transform = new Matrix())
                        {
                            transform.Rotate((float)angle);
                            transform.TransformPoints(points);
                        }

                        // Adjust coordinates to ensure the raster zone doesn't shrink from points that are
                        // rounded off in the wrong direction.
                        int startX = (int)((points[0].X < points[1].X) 
                            ? points[0].X
                            :Math.Ceiling(points[0].X));
                        int startY = (int)((points[0].Y < points[1].Y) 
                            ? points[0].Y
                            : Math.Ceiling(points[0].Y));
                        int endX = (int)((points[1].X < points[0].X) 
                            ? points[1].X
                            : Math.Ceiling(points[1].X));
                        int endY = (int)((points[1].Y < points[0].Y) 
                            ? points[1].Y
                            : Math.Ceiling(points[1].Y));
                        int height = (int)Math.Ceiling(bounds.Height);

                        // If the height is odd, the top and bottom of the zone will be a .5 value. When such a
                        // zone is displayed, those values will be rounded off, potentially exposing pixel
                        // content. Expand the zone by a pixel to prevent this.
                        if (height % 2 == 1)
                        {
                            height++;
                        }

                        wordOutputZones.Add(new RasterZone(startX, startY, endX, endY, height,
                            _imageViewer.PageNumber));
                    }
                }

                return wordOutputZones;
            }

            /// <summary>
            /// Makes the word highlights visible.
            /// </summary>
            void ShowWordHighlights()
            {
                foreach (Highlight highlight in _activeWordHighlights)
                {
                    highlight.Visible = true;
                }
            }

            /// <summary>
            /// Hides any currently visible word highlights.
            /// </summary>
            /// <param name="clear"><see langword="true"/> to clear _activeWordHighlights after the
            /// highlights have been hidden. Otherwise, <see langword="false"/>.</param>
            void HideWordHighlights(bool clear)
            {
                // If cancelling, simply reset the highlight color and hide all active
                // highlights.
                foreach (Highlight highlight in _activeWordHighlights)
                {
                    highlight.Visible = false;
                }

                if (clear)
                {
                    _activeWordHighlights.Clear();
                }
            }

            /// <summary>
            /// Sends a status message to the image viewer if appropriate for the specified
            /// <see paramref="page"/>.
            /// </summary>
            /// <param name="page">The page for which the specified <see paramref="status"/> and
            /// <see paramref="progressPercent"/> apply. -1 if they should apply for any loaded
            /// page.</param>
            /// <param name="status">A message describing the current state of the background
            /// loading operation.</param>
            /// <param name="progressPercent">The percent loading is complete for the specified
            /// <see paramref="page"/>.</param>
            void UpdateBackgroundProgressStatus(int page, string status, double progressPercent)
            {
                // Be sure to disregard any calls that occur after Dispose is called.
                if (_disposed)
                {
                    return;
                }

                // Cache the current status for later use.
                if (page != -1 && status != null)
                {
                    _pageStatusMessages[page] = status;
                }

                // Calculate the overall progress of the background operation.
                double overallProgress = progressPercent;
                if (page != -1)
                {
                    overallProgress = progressPercent / (double)_pageCount;
                    overallProgress += (double)PagesLoaded / (double)_pageCount;
                    if (overallProgress > 1.0)
                    {
                        overallProgress = 1.0;
                    }
                }

                // If appropriate, raise the BackgroundProcessStatusUpdate event in the UI thread.
                _imageViewer.BeginInvoke((MethodInvoker) (() =>
                    {
                        if (_imageViewer.IsImageAvailable)
                        {
                            if (page == -1 || page == _imageViewer.PageNumber)
                            {
                                OnBackgroundProcessStatusUpdate(status, overallProgress);
                            }
                            else if (_pageStatusMessages.TryGetValue(_imageViewer.PageNumber, out status))
                            {
                                OnBackgroundProcessStatusUpdate(status, overallProgress);
                            }
                        }
                    }));
            }

            /// <summary>
            /// Raises the <see cref="BackgroundProcessStatusUpdate"/> event.
            /// </summary>
            /// <param name="status">A message describing the current state of the background
            /// loading operation.</param>
            /// <param name="progressPercent">The percent loading is complete for the document as a
            /// whole.</param>
            void OnBackgroundProcessStatusUpdate(string status, double progressPercent)
            {
                if (BackgroundProcessStatusUpdate != null)
                {
                    BackgroundProcessStatusUpdate(this,
                        new BackgroundProcessStatusUpdateEventArgs(status, progressPercent));
                }
            }

            /// <summary>
            /// Raises the <see cref="OcrLoaded"/> event.
            /// </summary>
            /// <param name="ocrData">A <see cref="ThreadSafeSpatialString"/> instance representing the
            /// data from the completed OCR operation.</param>
            void OnOcrLoaded(ThreadSafeSpatialString ocrData)
            {
                if (OcrLoaded != null)
                {
                    OcrLoaded(this, new OcrTextEventArgs(ocrData));
                }
            }

            #endregion Private Members
        }
    }
}
