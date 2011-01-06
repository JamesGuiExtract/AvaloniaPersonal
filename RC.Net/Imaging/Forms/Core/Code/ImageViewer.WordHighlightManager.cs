using System;
using System.Collections.Generic;
using System.Drawing;
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
            /// The most recently started load task.
            /// </summary>
            volatile CancellationTokenSource _currentLoadTaskCanceler;

            /// <summary>
            /// A collection of all load tasks that have not yet been disposed of along with their
            /// associated <see cref="CancellationTokenSource"/> objects.
            /// </summary>
            volatile Dictionary<Task, CancellationTokenSource> _wordLoaderTasks =
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

            #region Methods

            /// <summary>
            /// Starts a tracking operation for word highlights (highlights become colored).
            /// </summary>
            public void StartTrackingOperation()
            {
                try
                {
                    foreach (Highlight highlight in _activeWordHighlights)
                    {
                        highlight.SetColor(_imageViewer.GetHighlightDrawColor(), false);
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

                        foreach (Highlight highlight in _activeWordHighlights)
                        {
                            highlight.SetColor(Color.White, false);
                            highlight.Visible = false;

                            // Attempt to adjust the fit of each raster zone so that they don't
                            // allow for any "leaked" pixels. This will also eliminate zones that
                            // aren't of the minimum height.
                            RasterZone zone = highlight.ToRasterZone();
                            zone.ExpandRasterZone(2, 2);
                            zone = _imageViewer.GetBlockFittedZone(zone);
                            if (zone != null)
                            {
                                rasterZones.Add(zone);
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
                        // After changing pages, call Activate. Even if the loader task is already
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
                        // Before modifying any of the data structures stop any executing load tasks.
                        Deactivate(false, false);
                        
                        lock (_lock)
                        {
                            WaitForEndedTasks();

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
                        if (_currentLoadTaskCanceler != null)
                        {
                            _currentLoadTaskCanceler.Dispose();
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
                        if (_currentLoadTaskCanceler != null &&
                            !_currentLoadTaskCanceler.IsCancellationRequested)
                        {
                            _currentLoadTaskCanceler.Cancel();
                        }

                        // Create and start a new loading task.
                        _currentLoadTaskCanceler = new CancellationTokenSource();
                        CancellationToken token = _currentLoadTaskCanceler.Token;

                        Task task = Task.Factory.StartNew(() => LoaderTask(token), token);

                        // Keep track of the task and caneler so they can be disposed of later.
                        _wordLoaderTasks[task] = _currentLoadTaskCanceler;
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
            /// <param name="clearData"></param>
            /// <param name="waitForTasks"></param>
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

                        // Cancel any currently running load task.
                        if (_currentLoadTaskCanceler != null &&
                            !_currentLoadTaskCanceler.IsCancellationRequested)
                        {
                            _currentLoadTaskCanceler.Cancel();
                        }

                        // Any load tasks in the collection at this point are to be disposed of.
                        _tasksToDispose = _wordLoaderTasks.Keys.ToArray();

                        _clearData |= clearData;

                        // Wait until the running tasks have stopped if requested.
                        if (waitForTasks)
                        {
                            WaitForEndedTasks();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31292", ex);
                }
            }

            /// <summary>
            /// Blocks until all cancelled load tasks have stopped.
            /// <para><b>Note</b></para>
            /// This method must be called within a lock.
            /// </summary>
            void WaitForEndedTasks()
            {
                if (_tasksToDispose != null)
                {
                    try
                    {
                        // Wait for any loaded tasks that are still running to complete.
                        Task.WaitAll(_tasksToDispose);
                    }
                    catch (Exception)
                    {
                        // Exceptions will have already been displayed by the tasks themselves.
                    }
                    finally
                    {
                        try
                        {
                            // Dispose of the loaded tasks and their CancelTokenSource's.
                            foreach (Task task in _tasksToDispose)
                            {
                                try
                                {
                                    _wordLoaderTasks[task].Dispose();
                                    task.Dispose();
                                    _wordLoaderTasks.Remove(task);
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
                foreach (HashSet<LayerObject> pageOfHighlights in _wordHighlights.Values)
                {
                    foreach (Highlight highlight in pageOfHighlights)
                    {
                        highlight.Dispose();
                    }
                }

                _pagesOfAddedWordHighlights.Clear();
                _wordHighlights.Clear();
                _ocrData = null;

                _clearData = false;
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
                    // Wait for previous tasks to complete for thread saftey of the data.
                    // Do so in a lock after checking the cancel token to ensure this task
                    // doesn't wait on itself if it was cancelled between the time it started
                    // and when it starts waiting on the running tasks.
                    lock (_lock)
                    {
                        cancelToken.ThrowIfCancellationRequested();
                        WaitForEndedTasks();
                    }

                    // Attempt to load
                    if (_ocrData == null)
                    {
                        string ocrFileName = _imageViewer._imageFile + ".uss";
                        if (!File.Exists(ocrFileName))
                        {
                            return;
                        }
                        else
                        {
                            _ocrData = (ISpatialString)new SpatialStringClass();
                            _ocrData.LoadFrom(ocrFileName, false);
                        }
                    }

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

                        _imageViewer.Invoke((MethodInvoker)(() =>
                            ExtractException.Display("ELI31299", ee)));

                        // Even though this will be eaten in the long run, still throw so that the task
                        // status will be set to faulted.
                        throw ee;
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
            /// <returns></returns>
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
