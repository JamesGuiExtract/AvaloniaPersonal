using Extract.Drawing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
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
            /// Controls how lenient the algorighm to find auto-fit zones is when a hard edge of
            /// pixel content cannot be found.
            /// </summary>
            const float _AUTO_FIT_FUZZY_FACTOR = 0.3F;

            #endregion Constants

            #region Fields

            /// <summary>
            /// A lock to synchronize access to the <see cref="WordHighlightManager"/>'s fields.
            /// </summary>
            object _lock = new object();

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
            volatile int _executeInUIReferenceCount = 0;

            /// <summary>
            /// The <see cref="ImageViewer"/> for which word highlights are being managed.
            /// </summary>
            volatile ImageViewer _imageViewer;

            /// <summary>
            /// The OCR data from the current <see cref="ImageViewer"/> document.
            /// </summary>
            volatile ISpatialString _ocrData;

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
            /// Indicates whether the WordHighlightManager is currently removing word layer objects
            /// from the image viewer.
            /// </summary>
            volatile bool _removingWordHighlights;

            /// <summary>
            /// Indicates whether word highlights are currently being loaded.
            /// </summary>
            volatile bool _loadingWordHighlights;

            /// <summary>
            /// The maximum height of an automatically sized zone for each page.
            /// </summary>
            Dictionary<int, int> _autoZoneHeightLimit = new Dictionary<int, int>();

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
            bool _active;

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

            #region Properties

            /// <summary>
            /// Gets a value indicating whether auto zones are being calculated for the current
            /// tracking operation.
            /// </summary>
            /// <value>
            /// <see langword="true"/> auto zones are being calculated for the current tracking
            /// operation; otherwise, <see langword="false"/>.
            /// </value>
            public bool InAutoFitOperation
            {
                get
                {
                    return _trackingStartLocation.HasValue;
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
                    // If the shift key is down this tracking operation will attempt to
                    // automatically generate a zone based on pixel content.
                    if (Control.ModifierKeys == Keys.Shift)
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
                            _imageViewer.MouseMove += HandleImageViewerMouseMove;

                            // Hide any active word highlights.
                            foreach (Highlight highlight in _activeWordHighlights)
                            {
                                highlight.SetColor(Color.White, false);
                                highlight.Visible = false;
                            }
                            _activeWordHighlights.Clear();
                        }
                    }
                    // Otherwise, this tracking operation will select word highlights to redact.
                    else
                    {
                        Color highlightColor = _imageViewer.GetHighlightDrawColor();

                        foreach (Highlight highlight in _activeWordHighlights)
                        {
                            highlight.SetColor(highlightColor, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31295", ex);
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
                bool restartWordLoading = false;

                try
                {
                    // Before accessing fields that may be modifed by the background worker, stop
                    // any currently running operation on the background worker.
                    lock (_lock)
                    {
                        restartWordLoading = _loadingWordHighlights || 
                            !_pagesOfAddedWordHighlights.Contains(_imageViewer.PageNumber);
                        CancelRunningOperation();
                    }

                    if (!_backgroundWorkerIdle.WaitOne(1000))
                    {
                        new ExtractException("ELI31374",
                            "Application trace: Word highlight background operation aborted.").Log();
                    }

                    if (!cancel)
                    {
                        CreateOutputHighlight();
                    }

                    // Hide all word highlights until the tool is used/moved again.
                    HideWordHighlights();

                    // If this was an auto-fit operation, reset the auto-fit data.
                    if (_trackingStartLocation != null)
                    {
                        _imageViewer.MouseMove -= HandleImageViewerMouseMove;
                        _trackingStartLocation = null;

                        RemoveAutoFitHighlight();
                    }

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
                    if (restartWordLoading)
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
                    // If the word highlighter or redactor is active when a new page is loaded,
                    // activate.
                    if (_imageViewer.IsImageAvailable &&
                        (_imageViewer.CursorTool == CursorTool.WordHighlight ||
                        _imageViewer.CursorTool == CursorTool.WordRedaction))
                    {
                        Activate();
                    }
                    // Otherwise, deactivate and reset all loaded data
                    else if (_active)
                    {
                        Deactivate(true);
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
                    if (_imageViewer.CursorTool == CursorTool.WordHighlight ||
                        _imageViewer.CursorTool == CursorTool.WordRedaction)
                    {
                        // After changing pages, call Activate. Even if a loader task is already
                        // active, this will force highlights to be loaded for the current page first.
                        Activate();
                    }
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
                    // Only run the _wordHighlightManager if the word highlighter/redactor is active.
                    if (_imageViewer.IsImageAvailable &&
                        (e.CursorTool == CursorTool.WordHighlight ||
                         e.CursorTool == CursorTool.WordRedaction))
                    {
                        Activate();
                    }
                    else
                    {
                        Deactivate(false);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31317", ex);
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
                    // During an auto zone tracking operation or if a previously open image has
                    // closed ignore this event.
                    if (_trackingStartLocation != null || !_imageViewer.IsImageAvailable)
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
                            highlight.Visible = true;

                            // If a tracking operation is active, display the redaction/highlight
                            // fill color as well.
                            if (_imageViewer._trackingData != null)
                            {
                                highlight.SetColor(_imageViewer.GetHighlightDrawColor(), false);
                            }

                            _activeWordHighlights.Add(e.LayerObject);

                            // If not in a tracking operation, invalidate the image viewer to
                            // force the highlight to be drawn. (If in a tracking operation, the
                            // image viewer will call invalidate after all highlights and tracking
                            // indications have been prepared.)
                            if (_imageViewer._trackingData == null)
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
                    // During an auto zone tracking operation or if a previously open image has
                    // closed ignore this event.
                    if (_trackingStartLocation != null || !_imageViewer.IsImageAvailable)
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
                        highlight.SetColor(Color.White, false);

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
                bool restartWordLoading = false;

                try
                {
                    // If the WordHighlight manager is the one deleting ignore the event.
                    if (_removingWordHighlights)
                    {
                        return;
                    }

                    // Before accessing fields that may be modifed by the background worker, stop
                    // any currently running operation on the background worker.
                    lock (_lock)
                    {
                        restartWordLoading = _loadingWordHighlights;
                        CancelRunningOperation();
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
                        restartWordLoading |= true;

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
                            try
                            {
                                _removingWordHighlights = true;
                                _imageViewer.LayerObjects.Remove(remainingHighlights, true);
                            }
                            finally
                            {
                                _removingWordHighlights = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Display("ELI31320", ex);
                }
                finally
                {
                    // Call activate to ensure we pick up loading word highlights again if
                    // a loading task was canceled.
                    if (restartWordLoading)
                    {
                        Activate();
                    }
                }
            }

            /// <summary>
            /// Handles the image viewer mouse move.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance
            /// containing the event data.</param>
            void HandleImageViewerMouseMove(object sender, MouseEventArgs e)
            {
                try
                {
                    // If the mouse has moved less that 2 client pixels since the last time we
                    // started calculating an auto zone, allow the previous calculation to complete
                    // so that we don't excessively restart calculations for small mouse movements.
                    if (_currentAutoFitLocation.HasValue)
                    {
                        // Compute the distance
                        int dX = e.Location.X - _currentAutoFitLocation.Value.X;
                        int dY = e.Location.Y - _currentAutoFitLocation.Value.Y;
                        double distance = Math.Sqrt(dX * dX + dY * dY);

                        if (distance < 2)
                        {
                            return;
                        }
                    }

                    _currentAutoFitLocation = e.Location;

                    // Get the points to be used by the AutoFitOperation
                    Point[] points = new Point[] { _trackingStartLocation.Value, e.Location };

                    // Convert the points from client to image coordinates.
                    GeometryMethods.InvertPoints(_imageViewer._transform, points);

                    // Cancel any currently running task and start calculating an auto-fit zone
                    // based on the current mouse location.
                    StartOperation(() => AutoFitOperation(points[0], points[1]));
                }
                catch (Exception ex)
                {
                    ExtractException.Display("ELI31351", ex);
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
                    _backgroundWorker.RunWorkerCompleted -= BackgroundWorkerCompleted;

                    // If we are handling this event, it is in order to kick off the worker again
                    // as soon as the previous worker thread completes.
                    Activate();
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
                        if (_backgroundWorker != null)
                        {
                            Deactivate(false);
                            _backgroundWorkerIdle.WaitOne(5000);

                            _backgroundWorker.Dispose();
                            _backgroundWorker = null;
                        }

                        ClearData();

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
                        if (!_active)
                        {
                            _active = true;

                            // If active, watch for when the cursor enters/leaves word highlights
                            _imageViewer.CursorEnteredLayerObject +=
                                HandleCursorEnteredLayerObject;
                            _imageViewer.CursorLeftLayerObject +=
                                HandleCursorLeftLayerObject;
                            _imageViewer.LayerObjects.DeletingLayerObjects +=
                                HandleDeletingLayerObjects;
                        }

                        // Create the background worker if it has not yet been created.
                        if (_backgroundWorker == null)
                        {
                            _backgroundWorker = new BackgroundWorker();
                            _backgroundWorker.WorkerSupportsCancellation = true;
                            _backgroundWorker.DoWork += WorkerThread;
                        }

                        // If the background worker is not currently running, start it.
                        if (_backgroundWorker.IsBusy)
                        {
                            // If the runnning working thread has been canceled, schedule a new
                            // worker to start once the current one has completed.
                            if (_backgroundWorker.CancellationPending)
                            {
                                _backgroundWorker.RunWorkerCompleted += BackgroundWorkerCompleted;
                                return;
                            }
                        }
                        else
                        {
                            _backgroundWorker.RunWorkerAsync();
                        }

                        // In case a running operation is cancelled by a message handler, indicate that a
                        // loading operation has been started so it can be resumed after the handler
                        // is complete.
                        _loadingWordHighlights = true;

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
                        if (_active)
                        {
                            _active = false;

                            // Stop watching for when the cursor enters/leaves word highlights
                            _imageViewer.CursorEnteredLayerObject -=
                                HandleCursorEnteredLayerObject;

                            _imageViewer.CursorLeftLayerObject -=
                                HandleCursorLeftLayerObject;
                            _imageViewer.LayerObjects.DeletingLayerObjects -=
                                HandleDeletingLayerObjects;
                        }

                        HideWordHighlights();

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
                            CancelRunningOperation();

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
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31292", ex);
                }
            }

            /// <summary>
            /// Cancels any currently running operation and starts the specified
            /// <see paramref="operation"/> instead.
            /// </summary>
            /// <param name="operation">The <see cref="Action"/> to perform.</param>
            void StartOperation(Action operation)
            {
                // Cancel any currently running operation.
                CancelRunningOperation();

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
            void CancelRunningOperation()
            {
                // Cancel any currently running operation.
                if (_canceler != null && !_canceler.IsCancellationRequested)
                {
                    _canceler.Cancel();
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
                            for (int i = 0; _executeInUIReferenceCount > 0; i++)
                            {
                                Thread.Sleep(50);

                                // Give up after a second.
                                if (i > 20)
                                {
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

                            // Attempt to load OCR data for the current document for the operations
                            // to use.
                            if (_ocrData == null)
                            {
                                string ocrFileName = _imageViewer._imageFile + ".uss";
                                if (File.Exists(ocrFileName))
                                {
                                    _ocrData = (ISpatialString)new SpatialStringClass();
                                    _ocrData.LoadFrom(ocrFileName, false);
                                }
                            }

                            _cancelToken.ThrowIfCancellationRequested();

                            // Perform the operation.
                            currentOperation();
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
                                string message = (_imageViewer.CursorTool == CursorTool.WordHighlight)
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
                finally
                {
                    // If the background worker has been stopped, consider the background worker idle.
                    _backgroundWorkerIdle.Set();
                }
            }

            /// <summary>
            /// Clear and disposes of all loaded data.
            /// <para><b>Note</b></para>
            /// This method must be called from within a lock.
            /// </summary>
            void ClearData()
            {
                _imageViewer.MouseMove -= HandleImageViewerMouseMove;

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
                _wordLineMapping.Clear();
                _autoZoneHeightLimit.Clear();
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
                try
                {
                    // Attempt to find an already specified auto zone height limit for the current page.
                    int page = _imageViewer.PageNumber;
                    int zoneHeightLimit = 0;
                    if (!_autoZoneHeightLimit.TryGetValue(page, out zoneHeightLimit))
                    {
                        if (_ocrData != null)
                        {
                            // Retrieve an IUnknownVector of SpatialStrings representing the words on the page.
                            ISpatialString pageData = (ISpatialString)_ocrData.GetSpecifiedPages(page, page);

                            // Base the height limit on the average line height for the page.
                            zoneHeightLimit = pageData.GetAverageLineHeight() * 5;
                        }

                        // Ensure the height limit is at least 100 pixels.
                        if (zoneHeightLimit < 100)
                        {
                            zoneHeightLimit = 100;
                        }

                        _autoZoneHeightLimit[page] = zoneHeightLimit;
                    }

                    // Create a probe to read image pixels.
                    using (PixelProbe probe = _imageViewer._reader.CreatePixelProbe(page))
                    {
                        _cancelToken.ThrowIfCancellationRequested();

                        Color outlineColor = LayerObject.SelectionPen.Color;

                        // Generate a 1 pixel hight raster zone as the basis of the fitting
                        // operation.
                        RasterZone rasterZone = new RasterZone(startPoint, endPoint, 1, page);

                        // Create a new FittingData instance and try to find the top edge of pixel
                        // content. Allow for an edge to be found using "fuzzy" logic.
                        FittingData data = new FittingData(rasterZone, _cancelToken);
                        if (data.FitEdge(Side.Top, probe, false, false, _AUTO_FIT_FUZZY_FACTOR, 0, 0,
                            zoneHeightLimit))
                        {
                            int remainingHeight = zoneHeightLimit - (int)data.Height;

                            // If a top edge was found, search downward for an opposing edge.
                            if (remainingHeight > 0 &&
                                data.FitEdge(Side.Bottom, probe, false, false,
                                    _AUTO_FIT_FUZZY_FACTOR, 0, 0, remainingHeight) &&
                                data.Height >= _MIN_SPLIT_HEIGHT)
                            {
                                // Shrink the left and right side to fit pixel content.
                                data.FitEdge(Side.Left, probe);
                                data.FitEdge(Side.Right, probe);

                                // Generate the new auto-fitted highlight.
                                Highlight highlight = new Highlight(_imageViewer, "",
                                    data.ToRasterZone(), "", _imageViewer.GetHighlightDrawColor());
                                highlight.Selectable = false;
                                highlight.CanRender = false;
                                highlight.OutlineColor = outlineColor;

                                // Add the new auto-fit highlight
                                ExecuteInUI((MethodInvoker)(() =>
                                {
                                    RemoveAutoFitHighlight();
                                    AddAutoFitHighlight(highlight);
                                }));

                                return;
                            }
                        }
                    }

                    // If a new auto-fit highlight wasn't able to be created, ensure the start and
                    // end point pass though an already existing auto-fit highlight. If they don't,
                    // remove it.
                    if (_autoFitHighlight != null)
                    {
                        FittingData autoZoneFittingData =
                            new FittingData(_autoFitHighlight.ToRasterZone());
                        if (!autoZoneFittingData.LinePassesThrough(startPoint, endPoint))
                        {
                            ExecuteInUI((MethodInvoker)(() => RemoveAutoFitHighlight()));
                        }
                    }
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31352", ex);
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
                    // If there is no OCR data, there is nothing more to be done.
                    if (_ocrData == null)
                    {
                        return;
                    }

                    // Loop through all pages starting at the current page to ensure highlights for
                    // the current page are loaded first.
                    int page = _imageViewer.PageNumber;
                    do
                    {
                        _cancelToken.ThrowIfCancellationRequested();

                        // Create highlights for each word on the page (if they have not already
                        // been created).
                        HashSet<LayerObject> wordHighlights = LoadWordHighlightsForPage(page);

                        if (page == _imageViewer.PageNumber)
                        {
                            // If this is the current page, add the word highlights to the page if
                            // they have not already been added.
                            ExecuteInUI((MethodInvoker)(() => AddWordLayerObjects(page, wordHighlights)));
                        }
                        else
                        {
                            // If this is not the current page, but highlights were previously
                            // added to the page, remove them to prevent bogging down the image
                            // viewer on documents with lots of pages.
                            ExecuteInUI((MethodInvoker)(() => RemoveWordLayerObjects(page)));
                        }

                        // Increment the page number (looping back to the first page if necessary).
                        page = (page < _imageViewer.PageCount) ? page + 1 : 1;
                    }
                    while (page != _imageViewer.PageNumber);
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31298", ex);
                }
                finally
                {
                    if (!_cancelToken.IsCancellationRequested)
                    {
                        _loadingWordHighlights = false;
                    }
                }
            }

            /// <summary>
            /// Creates <see cref="Highlight"/>s for each word on the specified document page. If
            /// cancelled, this method will allow for the load process to resume at the same point
            /// it was previously canceled.
            /// </summary>
            /// <param name="page">The page to load.</param>
            /// <returns>A <see cref="HashSet{T}"/> of <see cref="LayerObject"/>s representing the
            /// words on the specified page.</returns>
            HashSet<LayerObject> LoadWordHighlightsForPage(int page)
            {
                // Retrieve an IUnknownVector of SpatialStrings representing the words on the page.
                ISpatialString pageData = (ISpatialString)_ocrData.GetSpecifiedPages(page, page);

                // Get the data grouped by lines.
                IUnknownVector pageLines = pageData.GetLines();
                int lineCount = pageLines.Size();

                // If there is no OCR data on the page, return null;
                if (lineCount == 0)
                {
                    return null;
                }

                // Create or retrieve an existing HashSet to hold the words for the current page.
                HashSet<LayerObject> wordHighlights;
                if (!_wordHighlights.TryGetValue(page, out wordHighlights))
                {
                    wordHighlights = new HashSet<LayerObject>();
                    _wordHighlights[page] = wordHighlights;
                }

                // Keeps track of the starting point of each line in terms of the number of words
                // on the page before it.
                int lineStartIndex = 0;

                // Loop through the lines on the page
                for (int i = 0; i < lineCount; i++)
                {
                    // Get the words from this line.
                    ISpatialString line = (ISpatialString)pageLines.At(i);
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
                        ISpatialString word = (ISpatialString)words.At(j);
                        IUnknownVector comRasterZones = word.GetOriginalImageRasterZones();

                        ExtractException.Assert("ELI31304", "Unexpected raster zone in OCR data.",
                            comRasterZones.Size() == 1);

                        ComRasterZone comRasterZone = comRasterZones.At(0) as ComRasterZone;

                        ExtractException.Assert("ELI31305", "Invalid raster zone in OCR data.",
                            comRasterZone != null);

                        RasterZone rasterZone = new RasterZone(comRasterZone);

                        // Use the raster zone to create a highlight.
                        Highlight highlight =
                            new Highlight(_imageViewer, "", rasterZone, "", Color.White);
                        highlight.Selectable = false;
                        highlight.CanRender = false;
                        highlight.OutlineColor = LayerObject.SelectionPen.Color;
                        highlight.Visible = false;

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
            /// <param name="method">The <see cref="Delegate"/> to execute in the UI thread.</param>
            void ExecuteInUI(Delegate method)
            {
                // To avoid scheduling to the UI unnecessarilly, check cancelation token first.
                _cancelToken.ThrowIfCancellationRequested();

                // Keep track of the fact that the method was scheduled to execute.
                _executeInUIReferenceCount++;

                // Invoke to avoid modifying the imageViewer from outside the UI thread. Use begin
                // invoke so the operation isn't executed in the middle of another UI event.
                _imageViewer.BeginInvoke(method);

                // By scheduling a decrement of _UIOperationReferenceCount, we'll have record of
                // whether the method was called, even if the blocking here was cancelled.
                IAsyncResult result =
                    _imageViewer.BeginInvoke((MethodInvoker)(() => _executeInUIReferenceCount--));

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
                    // If the image is still available and the highlights have not already been
                    // added to the page, add them now.
                    if (_imageViewer.IsImageAvailable && wordHighlights != null &&
                        !_pagesOfAddedWordHighlights.Contains(page))
                    {
                        foreach (LayerObject layerObject in wordHighlights)
                        {
                            _imageViewer._layerObjects.Add(layerObject);
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

                        if (_imageViewer.IsImageAvailable)
                        {
                            _removingWordHighlights = true;
                            _imageViewer._layerObjects.Remove(_wordHighlights[page], false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Log("ELI31368", ex);
                }
                finally
                {
                    _removingWordHighlights = false;
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
                    if (_imageViewer.IsImageAvailable && InAutoFitOperation)
                    {
                        _imageViewer._layerObjects.Add(highlight);
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
                        if (_imageViewer.IsImageAvailable &&
                            _imageViewer.LayerObjects.Contains(_autoFitHighlight))
                        {
                            _removingWordHighlights = true;
                            _imageViewer._layerObjects.Remove(_autoFitHighlight, true);
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
                finally
                {
                    _removingWordHighlights = false;
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
            /// Adds a <see cref="Highlight"/> or <see cref="Redaction"/> to the image viewer based
            /// on the current tracking data.
            /// </summary>
            void CreateOutputHighlight()
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
                if (InAutoFitOperation)
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
                    zonesToHighlight.AddRange(GetWordOutputZones());
                }

                foreach (RasterZone zone in zonesToHighlight)
                {
                    // Attempt to adjust the fit of each raster zone so that they don't
                    // allow for any "leaked" pixels. This will also eliminate zones that
                    // aren't of the minimum height.
                    using (PixelProbe probe =
                        _imageViewer._reader.CreatePixelProbe(_imageViewer.PageNumber))
                    {
                        FittingData data = new FittingData(zone);

                        // Expand out up to 2 pixel in each direction looking for an all
                        // white row to ensure the zone has encapsulated all pixel content.
                        data.FitEdge(Side.Left, probe, false, false, null, 0, 0,
                            horizontalExpandLimit);
                        data.FitEdge(Side.Top, probe, false, false, null, 0, 0,
                            verticalExpandLimit);
                        data.FitEdge(Side.Right, probe, false, false, null, 0, 0,
                            horizontalExpandLimit);
                        data.FitEdge(Side.Bottom, probe, false, false, null, 0, 0,
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
                    // In order to check if the potential output is a duplicate of something that
                    // already exists, get a list of all raster zones of existing
                    // highlights/redactions.
                    List<RasterZone> existingRasterZones = new List<RasterZone>();
                    if (_imageViewer._cursorTool == CursorTool.WordHighlight)
                    {
                        existingRasterZones.AddRange(_imageViewer.LayerObjects
                            .OfType<CompositeHighlightLayerObject>()
                            .Where(lo => !IsWordHighlight(lo))
                            .SelectMany(h => h.GetRasterZones())
                            .Where(z => z.PageNumber == _imageViewer.PageNumber));
                    }
                    else
                    {
                        existingRasterZones.AddRange(_imageViewer.LayerObjects
                            .OfType<Redaction>()
                            .Where(lo => !IsWordHighlight(lo))
                            .SelectMany(h => h.GetRasterZones())
                            .Where(z => z.PageNumber == _imageViewer.PageNumber));
                    }

                    // If the raster zones aren't all duplicates of existing redaction/highlights
                    // create the output redaction/highlight.
                    if (!rasterZones.All(z => existingRasterZones
                            .Any(e => e.CompareTo(z) == 0)))
                    {
                        if (_imageViewer._cursorTool == CursorTool.WordHighlight)
                        {
                            CompositeHighlightLayerObject highlight =
                                new CompositeHighlightLayerObject(_imageViewer, _imageViewer.PageNumber,
                                    LayerObject.ManualComment, rasterZones,
                                    _imageViewer._defaultHighlightColor);
                            _imageViewer._layerObjects.Add(highlight);
                        }
                        else
                        {
                            Redaction redaction = new Redaction(_imageViewer,
                                _imageViewer.PageNumber, LayerObject.ManualComment, rasterZones,
                                _imageViewer._defaultRedactionFillColor);
                            _imageViewer._layerObjects.Add(redaction);
                        }
                    }
                }
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
                    highlight.SetColor(Color.White, false);
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
            /// Hides any currently visible word highlights.
            /// </summary>
            void HideWordHighlights()
            {
                // If cancelling, simply reset the highlight color and hide all active
                // highlights.
                foreach (Highlight highlight in _activeWordHighlights)
                {
                    highlight.SetColor(Color.White, false);
                    highlight.Visible = false;
                }

                _activeWordHighlights.Clear();
            }

            #endregion Private Members
        }
    }
}
