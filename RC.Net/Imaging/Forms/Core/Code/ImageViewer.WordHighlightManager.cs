using Extract.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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
            #region Fields

            /// <summary>
            /// A lock to synchronize access to the <see cref="WordHighlightManager"/>'s fields.
            /// </summary>
            object _lock = new object();

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
            /// by page.
            /// </summary>
            volatile Dictionary<int, HashSet<LayerObject>> _wordHighlights =
                new Dictionary<int, HashSet<LayerObject>>();

            /// <summary>
            /// The most recently started task.
            /// </summary>
            volatile CancellationTokenSource _currentTaskCanceler;

            /// <summary>
            /// A collection of all tasks that have not yet been disposed of along with their
            /// associated <see cref="CancellationTokenSource"/> objects.
            /// </summary>
            volatile Dictionary<Task, CancellationTokenSource> _tasks =
                new Dictionary<Task, CancellationTokenSource>();

            /// <summary>
            /// A list of tasks that have either ended or have been canceled.
            /// </summary>
            volatile Task[] _tasksToDispose;

            /// <summary>
            /// Indicates whether the loaded data should be cleared and disposed of before
            /// starting the next load task.
            /// </summary>
            volatile bool _clearData;

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
            bool _removingWordHighlights;

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
            Highlight _autoFitHighlight;

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
                    // If there is any active word highlight, this tracking operation will
                    // select word highlights to redact.
                    if (_activeWordHighlights.Count > 0)
                    {
                        foreach (Highlight highlight in _activeWordHighlights)
                        {
                            highlight.SetColor(_imageViewer.GetHighlightDrawColor(), false);
                        }
                    }
                    // Otherwise this tracking operation will attempt to automatically generate
                    // a zone based on pixel content.
                    else
                    {
                        _trackingStartLocation = new Point(x, y);
                        _imageViewer.MouseMove += HandleImageViewerMouseMove;
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
                try
                {
                    if (cancel)
                    {
                        // If cancelling, simply reset the highlight color and hide all active
                        // highlights.
                        foreach (Highlight highlight in _activeWordHighlights)
                        {
                            highlight.SetColor(Color.White, false);
                            highlight.Visible = false;
                        }
                    }
                    else
                    {
                        // Collect all raster zones from the active highlights.
                        List<RasterZone> rasterZones = new List<RasterZone>();

                        // The highlights that are to be turned in highlights may be either 1 or
                        // more word highlights or an auto fit highlight.
                        IEnumerable<RasterZone> zonesToHighlight = new List<RasterZone>();
                        if (_trackingStartLocation.HasValue)
                        {
                            if (_autoFitHighlight != null)
                            {
                                zonesToHighlight =
                                    new RasterZone[] { _autoFitHighlight.ToRasterZone() };
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

                                // Create a raster zone of the default height.
                                RasterZone defaultZone = new RasterZone(points[0], points[1],
                                    _imageViewer.DefaultHighlightHeight, _imageViewer.PageNumber);

                                zonesToHighlight = new RasterZone[] { defaultZone };
                            }
                        }
                        else
                        {
                            foreach (Highlight highlight in _activeWordHighlights)
                            {
                                highlight.SetColor(Color.White, false);
                                highlight.Visible = false;
                            }

                            zonesToHighlight =
                                _activeWordHighlights.Select(h => ((Highlight)h).ToRasterZone());
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
                                data.FitEdge(Side.Left, probe, false, false, null, 0, 0, 2);
                                data.FitEdge(Side.Top, probe, false, false, null, 0, 0, 2);
                                data.FitEdge(Side.Right, probe, false, false, null, 0, 0, 2);
                                data.FitEdge(Side.Bottom, probe, false, false, null, 0, 0, 2);

                                // Shrink any sides with excess space and eliminate zones that
                                // are too small.
                                RasterZone fittedZone = GetBlockFittedZone(data, probe);

                                if (fittedZone != null)
                                {
                                    rasterZones.Add(fittedZone);
                                }
                            }
                        }

                        // Create a highlight or redaction using these zones as long as there is at
                        // least one qualifying zone.
                        if (rasterZones.Count > 0)
                        {
                            if (_imageViewer._cursorTool == CursorTool.WordHighlight)
                            {
                                Highlight highlight =
                                    new Highlight(_imageViewer, LayerObject.ManualComment,
                                        rasterZones[0]);
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

                    // De-activate all word highlights until the tool is used/moved again.
                    _activeWordHighlights.Clear();

                    // If this was an auto-fit operation, reset the auto-fit data.
                    if (_trackingStartLocation != null)
                    {
                        _imageViewer.MouseMove -= HandleImageViewerMouseMove;
                        _trackingStartLocation = null;

                        RemoveAutoFitHighlight();

                        // Call activate to ensure we pick up loading word highlights again if
                        // a loading task was canceled to perform the auto tracking operation.
                        if (_active)
                        {
                            Activate();
                        }
                    }

                    _imageViewer.Invalidate();
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31296", ex);
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
                        Deactivate(true, false);
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
                    Deactivate(true, false);
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
                        Deactivate(false, false);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31317", ex);
                }
            }

            /// <summary>
            /// Handles the case that selection tool entered a layer object so that word highlights
            /// are displayed if the <see cref="WordHighlightManager"/> is active and the layer
            /// object is a word highlight.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="Extract.Imaging.Forms.LayerObjectEventArgs"/>
            /// instance containing the event data.</param>
            void HandleSelectionToolEnteredLayerObject(object sender, LayerObjectEventArgs e)
            {
                try
                {
                    // During an auto zone tracking operation, ignore this event.
                    if (_trackingStartLocation != null)
                    {
                        return;
                    }

                    // Check to see if the specified layer object is a word highlight
                    if (IsWordHighlight(e.LayerObject))
                    {
                        // Display the highlight (border)
                        Highlight highlight = (Highlight)e.LayerObject;
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
                catch (Exception ex)
                {
                    ExtractException.Display("ELI31293", ex);
                }
            }

            /// <summary>
            /// Handles the case that the selection tool left a layer object so that, if it is a
            /// word highlight it can be hidden.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="Extract.Imaging.Forms.LayerObjectEventArgs"/>
            /// instance containing the event data.</param>
            void HandleSelectionToolLeftLayerObject(object sender, LayerObjectEventArgs e)
            {
                try
                {
                    // During an auto zone tracking operation, ignore this event.
                    if (_trackingStartLocation != null)
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
                try
                {
                    // If the WordHighlight manager is the one deleting ignore the event.
                    if (_removingWordHighlights)
                    {
                        return;
                    }

                    // Get a set of the layer objects being deleted that are word highlights.
                    HashSet<LayerObject> deletedWordHighlights =
                        new HashSet<LayerObject>(e.LayerObjects
                            .Where(o => IsWordHighlight(o)));

                    // If any word highlights are deleted by anything but the WordHighlightManager,
                    // reload the highlights for the affected pages.
                    if (deletedWordHighlights.Count > 0)
                    {
                        // Before modifying any of the data structures stop any executing tasks.
                        Deactivate(false, false);

                        lock (_lock)
                        {
                            WaitForEndedTasks(null);

                            // Collect a list of all pages highlights are being deleted from and all
                            // word highlights from those pages.
                            IEnumerable<int> affectedPages = deletedWordHighlights
                                .Select(h => h.PageNumber)
                                .Distinct();
                            List<LayerObject> highlightsFromAffectedPages = new List<LayerObject>();
                            foreach (int page in affectedPages)
                            {
                                HashSet<LayerObject> pageHighlights = _wordHighlights[page];
                                highlightsFromAffectedPages.AddRange(pageHighlights);

                                // No longer consider any of these highlights as loaded.
                                _pagesOfAddedWordHighlights.Remove(page);
                                _wordHighlights.Remove(page);
                            }

                            // Look for any highlights on the affected pages that aren't already
                            // being deleted and delete them so that word highlights are duplicated
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

                        // As long as an image is still available, re-activate to start re-loading
                        // the highlight data.
                        if (_imageViewer.IsImageAvailable)
                        {
                            Activate();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.Display("ELI31320", ex);
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

                    // Otherwise, cancel any currently running task and start calculating an
                    // auto-fit zone based on the current location.
                    StartAutoFitTask(_trackingStartLocation.Value, e.Location);
                }
                catch (Exception ex)
                {
                    ExtractException.Display("ELI31351", ex);
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
                        Deactivate(true, true);

                        // Not necessary, but appeases FXCop.
                        if (_currentTaskCanceler != null)
                        {
                            _currentTaskCanceler.Dispose();
                            _currentTaskCanceler = null;
                        }
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
                            _imageViewer.SelectionToolEnteredLayerObject +=
                                HandleSelectionToolEnteredLayerObject;
                            _imageViewer.SelectionToolLeftLayerObject +=
                                HandleSelectionToolLeftLayerObject;
                            _imageViewer.LayerObjects.DeletingLayerObjects +=
                                HandleDeletingLayerObjects;
                        }

                        // Cancel the most recently started loading task. A new task will be
                        // restarted on the current image viewer page.
                        CancelRunningTask();

                        // Create and start a new loading task.
                        _currentTaskCanceler = new CancellationTokenSource();
                        CancellationToken token = _currentTaskCanceler.Token;

                        Task task = Task.Factory.StartNew(() => LoaderTask(token), token);

                        // Keep track of the task and canceler so they can be disposed of later.
                        _tasks[task] = _currentTaskCanceler;
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
            /// <param name="waitForTasks"><see langword="true"/> to block until all tasks all
            /// tasks have completed.</param>
            void Deactivate(bool clearData, bool waitForTasks)
            {
                try
                {
                    lock (_lock)
                    {
                        if (_active)
                        {
                            _active = false;

                            // Stop watching for when the cursor enters/leaves word highlights
                            _imageViewer.SelectionToolEnteredLayerObject -=
                                HandleSelectionToolEnteredLayerObject;
                            _imageViewer.SelectionToolLeftLayerObject -=
                                HandleSelectionToolLeftLayerObject;
                            _imageViewer.LayerObjects.DeletingLayerObjects -=
                                HandleDeletingLayerObjects;
                        }

                        CancelRunningTask();

                        _clearData |= clearData;

                        // Wait until the running tasks have stopped if requested.
                        if (waitForTasks)
                        {
                            WaitForEndedTasks(null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31292", ex);
                }
            }

            /// <summary>
            /// Cancels the actively running task (if there is one).
            /// <para><b>Note</b></para>
            /// This method must be called within a lock.
            /// </summary>
            void CancelRunningTask()
            {
                // Cancel any currently running task.
                if (_currentTaskCanceler != null &&
                    !_currentTaskCanceler.IsCancellationRequested)
                {
                    _currentTaskCanceler.Cancel();
                }

                // Any load tasks in the collection at this point are to be disposed of.
                if (_tasks.Count > 0)
                {
                    _tasksToDispose = _tasks.Keys.ToArray();
                }
            }

            /// <summary>
            /// Blocks until all cancelled load tasks have stopped.
            /// <para><b>Note</b></para>
            /// This method must be called within a lock.
            /// </summary>
            /// <param name="cancelToken">A <see cref="CancellationToken"/> to abort the wait.
            /// </param>
            void WaitForEndedTasks(CancellationToken? cancelToken)
            {
                if (_tasksToDispose != null)
                {
                    try
                    {
                        // Wait for any tasks that are still running to complete.
                        if (cancelToken.HasValue)
                        {
                            // While it would seem passing cancelToken.Value to the overload of
                            // WaitAll that takes a CancellationToken would ensure the wait is
                            // aborted when the token is canceled, that does not seem to be the
                            // case. Check the cancelToken manually every 200 ms to ensure we don't
                            // continue to wait if canceled.
                            do
                            {
                                cancelToken.Value.ThrowIfCancellationRequested();
                            }
                            while (!Task.WaitAll(_tasksToDispose, 200, cancelToken.Value));
                        }
                        else
                        {
                            Task.WaitAll(_tasksToDispose);
                        }
                    }
                    catch (Exception)
                    {
                        // Exceptions will have already been displayed by the tasks themselves.
                    }
                    finally
                    {
                        // If this task is canceled, the _tasksToDispose may still be running
                        // and should not be disposed of.
                        if (cancelToken.HasValue)
                        {
                            cancelToken.Value.ThrowIfCancellationRequested();
                        }

                        try
                        {
                            // Dispose of the tasks and their CancelTokenSource's.
                            foreach (Task task in _tasksToDispose)
                            {
                                try
                                {
                                    _tasks[task].Dispose();
                                    task.Dispose();
                                    _tasks.Remove(task);
                                }
                                catch (Exception ex)
                                {
                                    ExtractException.Log("ELI31302", ex);
                                }
                            }

                            _tasksToDispose = null;

                            // If requested, clear and dispose of all loaded data.
                            if (_clearData)
                            {
                                ClearData();
                            }
                        }
                        catch (Exception ex)
                        {
                            ExtractException.Log("ELI31303", ex);
                        }
                    }
                }
            }

            /// <summary>
            /// Clear and disposes of all loaded data.
            /// </summary>
            void ClearData()
            {
                if (_trackingStartLocation != null)
                {
                    _trackingStartLocation = null;
                    _imageViewer.MouseMove -= HandleImageViewerMouseMove;
                }

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
                _autoZoneHeightLimit.Clear();
                _ocrData = null;

                _clearData = false;
            }

            /// <summary>
            /// Cancels any current running task as starts a new task to create an auto-fit
            /// highlight based on the current tracking data.
            /// </summary>
            /// <param name="startPoint">The start <see cref="Point"/> of the auto-fit calculation.
            /// </param>
            /// <param name="endPoint">The end <see cref="Point"/> of the auto-fit calculation.
            /// </param>
            void StartAutoFitTask(Point startPoint, Point endPoint)
            {
                lock (_lock)
                {
                    if (_active)
                    {
                        _currentAutoFitLocation = endPoint;

                        CancelRunningTask();

                        // Create and start a new loading task.
                        _currentTaskCanceler = new CancellationTokenSource();
                        CancellationToken token = _currentTaskCanceler.Token;

                        Task task = Task.Factory.StartNew(() => 
                            AutoFitTask(token, startPoint,endPoint), token);

                        // Keep track of the task and canceler so they can be disposed of later.
                        _tasks[task] = _currentTaskCanceler;
                    }
                }
            }

            /// <summary>
            /// Encapsulates a task to calculate an angular highlight automatically sized to pixel
            /// content using the start and end point of the tracking operation as the start and
            /// end point of the raster zone.
            /// </summary>
            /// <param name="cancelToken">A <see cref="CancellationToken"/> to halt execution of the
            /// task.</param>
            /// <param name="startPoint">The start <see cref="Point"/> of the auto-fit calculation.
            /// </param>
            /// <param name="endPoint">The end <see cref="Point"/> of the auto-fit calculation.
            /// </param>
            void AutoFitTask(CancellationToken cancelToken, Point startPoint, Point endPoint)
            {
                try
                {
                    // Wait for any cancelled task and load OCR data.
                    CommonTaskStart(cancelToken);

                    cancelToken.ThrowIfCancellationRequested();

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
                    using (Matrix transform = (Matrix)_imageViewer._transform.Clone())
                    {
                        Point[] points;

                        cancelToken.ThrowIfCancellationRequested();

                        points = new Point[] { startPoint, endPoint };
                    
                        // Convert the points from client to image coordinates.
                        GeometryMethods.InvertPoints(transform, points);

                        // Generate a 1 pixel hight raster zone as the basis of the fitting
                        // operation.
                        RasterZone rasterZone = new RasterZone(points[0], points[1], 1, page);

                        // Create a new FittingData instance and try to find the top edge of pixel
                        // content. Allow for an edge to be found using "fuzzy" logic.
                        FittingData data = new FittingData(rasterZone, cancelToken);
                        if (data.FitEdge(Side.Top, probe, false, false, 0.2F, 0, 0, zoneHeightLimit))
                        {
                            int remainingHeight = zoneHeightLimit - (int)data.Height;

                            // If a top edge was found, search downward for an opposing edge.
                            if (remainingHeight > 0 &&
                                data.FitEdge(Side.Bottom, probe, false, false, 0.1F, 0, 0, remainingHeight) &&
                                data.Height >= _MIN_SPLIT_HEIGHT)
                            {
                                // Shrink the left and right side to fit pixel content.
                                data.FitEdge(Side.Left, probe);
                                data.FitEdge(Side.Right, probe);

                                // Generate a raster zone based on the fitting data.
                                rasterZone = data.ToRasterZone();
                                Highlight highlight = null;

                                try
                                {
                                    // Generate the auto-fitted highlight.
                                    highlight = new Highlight(_imageViewer, "", rasterZone, "",
                                        _imageViewer.GetHighlightDrawColor());
                                    highlight.Selectable = false;
                                    highlight.CanRender = false;
                                    highlight.OutlineColor = LayerObject.SelectionPen.Color;

                                    // Remove any previous auto-fit highlight.
                                    RemoveAutoFitHighlight();

                                    // Add the new one.
                                    AddAutoFitHighlight(highlight);
                                }
                                catch
                                {
                                    // Dispose of the highlight if it was not added successfully.
                                    if (highlight != null && highlight != _autoFitHighlight)
                                    {
                                        highlight.Dispose();
                                    }

                                    throw;
                                }
                            }
                        }
                    }

                    return;
                }
                catch (OperationCanceledException)
                {
                    // If canceled, re-throw the exception to allow the task status to be set to
                    // canceled, but don't display the exception as will happen with any other type
                    // of exception.
                    throw;
                }
                catch (Exception ex)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        // If the task threw an exception but a cancel has been requested, assume
                        // the exception resulted from attempting an operation that should not have
                        // occured after cancelation (for example, accessing the current page number
                        // after a document has been closed).
                        throw new OperationCanceledException();
                    }
                    else
                    {
                        string message = (_imageViewer.CursorTool == CursorTool.WordHighlight)
                            ? "Error creating word highlight."
                            : "Error creating word redaction.";

                        // Display any non-cancelation exception so the user is notified right away
                        // instead of when the page is changed or the document is closed.
                        ExtractException ee = new ExtractException("ELI31352", message, ex);

                        ExtractException.Display("ELI31353", ee);

                        // Even though this will be eaten in the long run, still throw so that the task
                        // status will be set to faulted.
                        throw ee;
                    }
                }
            }

            /// <summary>
            /// Removes and disposes of any existing auto-fit highlight.
            /// </summary>
            /// <param name="highlight">The newly calculated auto-fit highlight to be added to the
            /// image viewer.</param>
            void AddAutoFitHighlight(Highlight highlight)
            {
                // Ensure an auto-event tracking event is still active before adding the auto-fit
                // highlight.
                if (InAutoFitOperation)
                {
                    _imageViewer.BeginInvoke((MethodInvoker)(() =>
                    {
                        InsertWordLayerObjects(highlight.PageNumber, new LayerObject[] { highlight });
                        _autoFitHighlight = highlight;
                        _imageViewer.Invalidate();
                    }));
                }
            }

            /// <summary>
            /// Removes and disposes of any existing auto-fit highlight.
            /// </summary>
            void RemoveAutoFitHighlight()
            {
                _currentAutoFitLocation = null;
                _imageViewer.BeginInvoke((MethodInvoker)(() =>
                {
                    if (_autoFitHighlight != null)
                    {
                        if (_imageViewer.IsImageAvailable &&
                            _imageViewer.LayerObjects.Contains(_autoFitHighlight))
                        {
                            _imageViewer._layerObjects.Remove(_autoFitHighlight, true);
                        }
                        else
                        {
                            _autoFitHighlight.Dispose();
                        }

                        _autoFitHighlight = null;
                    }
                }));
            }

            /// <summary>
            /// Encapsulates a task to load and add highlights for all words on a document from OCR
            /// data. While the tasks will load and create highlights for all words in the document,
            /// in order to reduce demand on the image viewer for documents with many pages,
            /// it will only add the highlights to the current page. To add highights to other
            /// pages, another tasks instance should be run when the image viewer page is changed
            /// (this task will also remove highlights from all pages except the current page).
            /// </summary>
            /// <param name="cancelToken">A <see cref="CancellationToken"/> to halt execution of the
            /// task.</param>
            void LoaderTask(CancellationToken cancelToken)
            {
                try
                {
                    // Wait for any cancelled task and load OCR data.
                    CommonTaskStart(cancelToken);

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
                        cancelToken.ThrowIfCancellationRequested();

                        // Create highlights for each word on the page (if they have not already
                        // been created).
                        HashSet<LayerObject> wordHighlights = LoadWordHighlightsForPage(page, cancelToken);

                        if (page == _imageViewer.PageNumber)
                        {
                            // If this is the current page and the highlights have not already been
                            // added to the page, add them now.
                            if (wordHighlights != null && !_pagesOfAddedWordHighlights.Contains(page))
                            {
                                // Invoke to avoid modifying the imageViewer from outside the UI thread.
                                _imageViewer.Invoke((MethodInvoker)(() =>
                                    InsertWordLayerObjects(page, wordHighlights)));
                            }
                        }
                        else
                        {
                            // If this is not the current page, but highlights were previously
                            // added to the page, remove them to prevent bogging down the image
                            // viewer on documents with lots of pages.
                            if (_pagesOfAddedWordHighlights.Contains(page))
                            {
                                // Invoke to avoid modifying the imageViewer from outside the UI thread.
                                _imageViewer.Invoke((MethodInvoker)(() =>
                                    RemoveWordLayerObjects(page)));
                            }
                        }

                        // Increment the page number (looping back to the first page if necessary).
                        page = (page < _imageViewer.PageCount) ? page + 1 : 1;
                    }
                    while (page != _imageViewer.PageNumber);
                }
                catch (OperationCanceledException)
                {
                    // If canceled, re-throw the exception to allow the task status to be set to
                    // canceled, but don't display the exception as will happen with any other type
                    // of exception.
                    throw;
                }
                catch (Exception ex)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        // If the task threw an exception but a cancel has been requested, assume
                        // the exception resulted from attempting an operation that should not have
                        // occured after cancelation (for example, accessing the current page number
                        // after a document has been closed).
                        throw new OperationCanceledException();
                    }
                    else
                    {
                        string message = (_imageViewer.CursorTool == CursorTool.WordHighlight)
                            ? "Failed to load data for word highlight tool."
                            : "Failed to load data for word redation tool.";

                        // Display any non-cancelation exception so the user is notified right away
                        // instead of when the page is changed or the document is closed.
                        ExtractException ee = new ExtractException("ELI31298", message, ex);

                        ExtractException.Display("ELI31299", ee);

                        // Even though this will be eaten in the long run, still throw so that the task
                        // status will be set to faulted.
                        throw ee;
                    }
                }
            }

            /// <summary>
            /// Common code to newly starting tasks-- wait for any cancelled tasks and loads OCR
            /// data.
            /// </summary>
            /// <param name="cancelToken">A <see cref="CancellationToken"/> to halt execution of the
            /// task.</param>
            void CommonTaskStart(CancellationToken cancelToken)
            {
                // Wait for previous tasks to complete for thread saftey of the data.
                // Do so in a lock with a wait that watches the cancel token to ensure this task
                // doesn't wait on itself if it was cancelled between the time it started
                // and when it starts waiting on the running tasks.
                lock (_lock)
                {
                    WaitForEndedTasks(cancelToken);
                }

                // Attempt to load OCR data for the current document.
                if (_ocrData == null)
                {
                    string ocrFileName = _imageViewer._imageFile + ".uss";
                    if (File.Exists(ocrFileName))
                    {
                        _ocrData = (ISpatialString)new SpatialStringClass();
                        _ocrData.LoadFrom(ocrFileName, false);
                    }
                }
            }

            /// <summary>
            /// Creates <see cref="Highlight"/>s for each word on the specified document page. If
            /// cancelled, this method will allow for the load process to resume at the same point
            /// it was previously canceled.
            /// </summary>
            /// <param name="page">The page to load.</param>
            /// <param name="cancelToken">A <see cref="CancellationToken"/> to halt execution of the
            /// task.</param>
            /// <returns>A <see cref="HashSet{T}"/> of <see cref="LayerObject"/>s representing the
            /// words on the specified page.</returns>
            HashSet<LayerObject> LoadWordHighlightsForPage(int page, CancellationToken cancelToken)
            {
                // Retrieve an IUnknownVector of SpatialStrings representing the words on the page.
                ISpatialString pageData = (ISpatialString)_ocrData.GetSpecifiedPages(page, page);
                IUnknownVector words = pageData.GetWords();
                int wordCount = words.Size();

                // If there a no words on the page, return null;
                if (wordCount == 0)
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

                // Loop through the words on the page, starting where left off if appropriate.
                for (int i = wordHighlights.Count; i < wordCount; i++)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    // Get a raster zone for the word.
                    ISpatialString word = (ISpatialString)words.At(i);
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
                    wordHighlights.Add(highlight);
                }

                return wordHighlights;
            }

            /// <summary>
            /// Adds the specified <see cref="LayerObjects"/> to the <see cref="ImageViewer"/>.
            /// </summary>
            /// <param name="page"></param>
            /// <param name="wordHighlights"></param>
            void InsertWordLayerObjects(int page, IEnumerable<LayerObject> wordHighlights)
            {
                if (_imageViewer.IsImageAvailable)
                {
                    foreach (LayerObject layerObject in wordHighlights)
                    {
                        _imageViewer._layerObjects.Add(layerObject);
                    }

                    _pagesOfAddedWordHighlights.Add(page);
                }
            }

            /// <summary>
            /// Removes all word highlights from the specified page.
            /// </summary>
            /// <param name="page">The page for which word highlights should be removed from the
            /// image viewer.</param>
            void RemoveWordLayerObjects(int page)
            {
                if (_imageViewer.IsImageAvailable)
                {
                    try
                    {
                        _removingWordHighlights = true;
                        _imageViewer._layerObjects.Remove(_wordHighlights[page], false);
                        _pagesOfAddedWordHighlights.Remove(page);
                    }
                    finally
                    {
                        _removingWordHighlights = false;
                    }
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

            #endregion Private Members
        }
    }
}
