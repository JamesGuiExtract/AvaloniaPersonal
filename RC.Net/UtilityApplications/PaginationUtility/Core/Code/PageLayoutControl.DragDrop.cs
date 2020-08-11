using Extract.Utilities.Forms;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// This file contains class members used to process drag and drop operations
    /// </summary>
    partial class PageLayoutControl
    {
        /// <summary>
        /// The name of the data format for drag and drop operations.
        /// </summary>
        static readonly string _DRAG_DROP_DATA_FORMAT = "ExtractPaginationDragDropDataFormat";

        /// <summary>
        /// The number of pixels from the top or bottom of the control where scrolling will be
        /// triggered during a drag/drop operation.
        /// </summary>
        static readonly int _DRAG_DROP_SCROLL_AREA = 50;

        #region Fields

        /// <summary>
        /// Indicates where <see cref="PaginationControl"/>s will be dropped during a
        /// drag-and-drop operation.
        /// </summary>
        DropLocationIndicator _dropLocationIndicator = new DropLocationIndicator();

        PaginationControl _activeDragTarget;

        /// <summary>
        /// Indicates the control index at which controls should be dropped during a
        /// drag-and-drop operation.
        /// </summary>
        int _dropLocationIndex = -1;

        /// <summary>
        /// The location of any ongoing drag operation; null if no drag operation is active.
        /// </summary>
        Point? _dragLocation;

        /// <summary>
        /// The time of the last mouse movement for an ongoing drag operation; null if no drag operation is active.
        /// </summary>
        DateTime? _lastDragMove;

        /// <summary>
        /// A <see cref="Timer"/> which fires to trigger scrolling during drag/drop operation while
        /// the cursor is close to either the top or bottom of the control.
        /// </summary>
        Timer _dragDropScrollTimer;

        /// <summary>
        /// The number of pixels per <see cref="_dragDropScrollTimer"/> fire the control should
        /// scroll while drag/drop scrolling is active.
        /// </summary>
        int _scrollSpeed;

        /// <summary>
        /// The last scroll position that was set programmatically during drag/drop scrolling. There
        /// are situations outside of the code here that cause the scroll position to be adjusted
        /// after we have set it. By keeping track of what we last wanted it to be we can prevent
        /// the scroll position from jumping around in an unexpected fashion.
        /// </summary>
        int _dragDropScrollPos;

        /// <summary>
        /// Indicates whether a drag operation is in progress
        /// </summary>
        bool _dragActive;

        #endregion Fields

        #region Methods

        // We do not need to worry about preventing sleep mode with the _dragDropScrollTimer as it
        // will be active only during a drag/drop operation.
        [SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
        void DragDrop_Init()
        {
            // When dragging files in from the Windows shell, _dropLocationIndicator receives
            // drag/drop events if the mouse is over the indicator.
            _dropLocationIndicator.DragDrop += Handle_DragDrop;
            _dropLocationIndicator.DragEnter += Handle_DragEnter;
            _dropLocationIndicator.DragOver += Handle_DragOver;
            _dropLocationIndicator.DragLeave += Handle_DragLeave;

            // The following two events are registered to allow drag events to be tracked
            // before dragging over a potential target control (such as dragging down below
            // the last page control).
            _flowLayoutPanel.MouseMove += (o, e) => DragDrop_HandleMouseMove(null, e, displayException: true);
            _flowLayoutPanel.DragOver += (o, e) => Handle_DragOver(null, e);

            // Set scrolling during a drag/drop scroll event to occur 20 times / sec.
            _dragDropScrollTimer = new Timer();
            _dragDropScrollTimer.Interval = 50;
            _dragDropScrollTimer.Tick += HandleDragDropScrollTimer_Tick;
        }

        /// <summary>
        /// Starts/stops scrolling during a drag event based upon the location of the mouse.
        /// </summary>
        /// <param name="mouseLocation">The current mouse location in screen coordinates.</param>
        void EnableDragScrolling(Point mouseLocation)
        {
            // Determine if scrolling should occur based upon the mouse location being close to
            // the top/bottom of the screen
            Point screenLocation = PointToScreen(Location);
            int topScrollZone = screenLocation.Y + _DRAG_DROP_SCROLL_AREA;
            int bottomScrollZone =
                screenLocation.Y + DisplayRectangle.Height - _DRAG_DROP_SCROLL_AREA;

            // If the control should scroll up
            if (mouseLocation.Y <= topScrollZone)
            {
                _scrollSpeed = -Math.Min(topScrollZone - mouseLocation.Y, _DRAG_DROP_SCROLL_AREA);

                if (!_dragDropScrollTimer.Enabled)
                {
                    _dragDropScrollPos = -1;
                    _dragDropScrollTimer.Start();
                }
            }
            // If the control should scroll down
            else if (mouseLocation.Y >= bottomScrollZone)
            {
                _scrollSpeed = Math.Min(mouseLocation.Y - bottomScrollZone, _DRAG_DROP_SCROLL_AREA);

                if (!_dragDropScrollTimer.Enabled)
                {
                    _dragDropScrollPos = -1;
                    _dragDropScrollTimer.Start();
                }
            }
            // If scrolling should be stopped
            else if (_dragDropScrollTimer.Enabled)
            {
                _dragDropScrollTimer.Stop();
            }
        }

        /// <summary>
        /// Shows an indicator at the specified <see cref="_dragLocation"/> to indicate where pages
        /// currently being dragged would be dropped.
        /// </summary>
        /// <returns><c>true</c> if a valid drag target was found and the indicator was displayed;
        /// <c>false</c> if no valid drag target exists at the specified location.</returns>
        bool ShowDropLocationIndicator(Point dragLocation)
        {
            var paginationControl = GetControlAtPoint<PaginationControl>(dragLocation);
            if (paginationControl != _activeDragTarget && _activeDragTarget != null)
            {
                ResetActiveDragTarget();
            }

            if (paginationControl != null)
            {
                if (_dragLocation != dragLocation)
                {
                    _lastDragMove = DateTime.Now;
                }
                _dragLocation = dragLocation;
            }

            if (paginationControl is PageThumbnailControl pageThumbnailControl
                && pageThumbnailControl.Visible
                && pageThumbnailControl.Document.OutputProcessed != true)
            {
                InitializePageThumbnailDragTarget(dragLocation, pageThumbnailControl);
                return true;
            }
            else if (paginationControl is PaginationSeparator separator
                && !SelectedControls.Any(control => control.Document == separator.Document))
            {
                InitializeDocumentSeparatorDragTarget(separator);
                return true;
            }
            else
            {
                Controls.Remove(_dropLocationIndicator);
                return false;
            }
        }

        void InitializePageThumbnailDragTarget(Point dragLocation, PageThumbnailControl pageThumbnailControl)
        {
            Point location;
            _dropLocationIndex = _flowLayoutPanel.Controls.IndexOf(pageThumbnailControl);
            _activeDragTarget = pageThumbnailControl;

            // Because a lot of padding may be added to extend a page control out to the end
            // of a row, take the padding into account when deciding whether a drop should
            // occur before or after the page.
            int left = pageThumbnailControl.Left + pageThumbnailControl.Padding.Left;
            int right = pageThumbnailControl.Right - pageThumbnailControl.Padding.Right;
            int center = (right - left) / 2;

            if ((dragLocation.X - pageThumbnailControl.Left) > center)
            {
                _dropLocationIndex++;
                location = pageThumbnailControl.TrailingInsertionPoint;
            }
            else
            {
                location = pageThumbnailControl.PreceedingInsertionPoint;
            }

            ShowDropLocationIndicator(location, _activeDragTarget.Height);
        }

        void InitializeDocumentSeparatorDragTarget(PaginationSeparator separator)
        {
            if (separator.Collapsed)
            {
                if (!separator.ShowDragHints)
                {
                    separator.DragOver += Handle_DragOver;
                    separator.ShowDragHints = true;
                }
                Controls.Remove(_dropLocationIndicator);
                _activeDragTarget = separator;

                if (_lastDragMove.HasValue
                    && (DateTime.Now - _lastDragMove.Value) > new TimeSpan(0, 0, 0, 0, milliseconds: 500))
                {
                    using (new UIUpdateLock(this))
                    {
                        separator.Collapsed = false;
                    }
                }
            }
            else
            {
                var pageThumbnailControl = separator.Document.PageControls.Last();
                _activeDragTarget = pageThumbnailControl;

                // Because a lot of padding may be added to extend a page control out to the end
                // of a row, take the padding into account when deciding whether a drop should
                // occur before or after the page.
                //_dropLocationIndex++;
                ShowDropLocationIndicator(
                    pageThumbnailControl.TrailingInsertionPoint,
                    pageThumbnailControl.Height);
            }

            _dropLocationIndex = _flowLayoutPanel.Controls.IndexOf(separator.Document.PageControls.Last()) + 1;
        }

        /// <summary>
        /// Shows the <see cref="_dropLocationIndicator"/> at the specified
        /// <see paramref="location"/>.
        /// </summary>
        /// <param name="location">The <see cref="Point"/> where the location indicator should be
        /// drawn.</param>
        /// <param name="height">The height the location indicator should be</param>
        void ShowDropLocationIndicator(Point location, int height)
        {
            location.Offset(-_dropLocationIndicator.Width / 2, 0);

            // If the _dropLocationIndicator is already visible, but needs to be moved, remove it
            // completely, otherwise the background may retain some artifacts from the controls it
            // was previously over.
            if (Controls.Contains(_dropLocationIndicator) &&
                _dropLocationIndicator.Location != location)
            {
                Controls.Remove(_dropLocationIndicator);
            }

            if (!Controls.Contains(_dropLocationIndicator))
            {
                Controls.Add(_dropLocationIndicator);
                _dropLocationIndicator.BringToFront();
            }
            _dropLocationIndicator.Location = location;
            _dropLocationIndicator.Height = height;

            // Make sure the rectangle we are updating is large enough to intersect with bordering
            // controls.
            Rectangle updateRect = _dropLocationIndicator.Bounds;
            updateRect.Inflate(5, 5);

            // To make the background of _dropLocationIndicator "transparent", update the region of
            // this control under the _dropLocationIndicator.
            Invalidate(updateRect, true);
            Update();

            // Now, the pagination controls under it should be refreshed as well.
            foreach (Control paginationControl in _flowLayoutPanel.Controls)
            {
                if (updateRect.IntersectsWith(paginationControl.Bounds))
                {
                    paginationControl.Refresh();
                }
            }

            // Finally, trigger the _dropLocationIndicator itself to paint on top of the
            // "background" that has just been drawn.
            _dropLocationIndicator.Invalidate();
        }

        /// <summary>
        /// Call when a drag event is ending or a new control may be about to be assigned as the
        /// active drag target.
        /// </summary>
        void ResetActiveDragTarget()
        {
            _dragLocation = null;
            _lastDragMove = null;

            if (_activeDragTarget is PaginationSeparator activeDragSeparatorTarget)
            {
                activeDragSeparatorTarget.DragOver -= Handle_DragOver;
                activeDragSeparatorTarget.ShowDragHints = false;
            }
            _activeDragTarget = null;
        }

        /// <summary>
        /// Handling for drag/drop implementation that should occur when the mouse is moved within a
        /// <see cref="PaginationControl"/>.
        /// </summary>
        /// <param name="originControl">The <see cref="PaginationControl"/> that processed the MouseMove
        /// event.</param>
        /// <param name="e">The event arguments provided by the mouse move event.</param>
        /// <param name="displayException"><c>true</c> to display any exception; <c>false</c> to throw
        /// the exception.</param>
        void DragDrop_HandleMouseMove(PaginationControl originControl, MouseEventArgs e, bool displayException)
        {
            try
            {
                // Do not allow modification of documents that have already been output.
                if (SelectedControls
                        .OfType<PageThumbnailControl>()
                        .Any(pageControl => pageControl.Document.OutputProcessed))
                {
                    return;
                }

                //If the mouse button is down and the sending control is already selected, start a
                // drag/drop operation.
                if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
                {
                    if (originControl != _loadNextDocumentButtonControl)
                    {
                        // Don't start a drag and drop operation unless the user has dragged out of
                        // the origin control to help prevent accidental drag/drops.
                        Point mouseLocation = PointToClient(Control.MousePosition);
                        var targetControl = GetControlAtPoint<PaginationControl>(mouseLocation);

                        if (targetControl != originControl
                            && originControl?.Document?.OutputProcessed != true
                            && targetControl?.Document?.OutputProcessed != true)
                        {
                            // [DotNetRCAndUtils:968]
                            // If the control where the drag originated is not selected, imply
                            // selection of that control when starting the drag operation.
                            if (originControl?.Selected == false && !(originControl is PaginationSeparator))
                            {
                                ProcessControlSelection(originControl);
                            }

                            var dataObject = new DataObject(_DRAG_DROP_DATA_FORMAT, this);

                            try
                            {
                                _dragActive = true;

                                // Don't allow the document split indicator to be active once a drag
                                // operation begins.
                                DeactivateSplitIndicator();

                                DoDragDrop(dataObject, DragDropEffects.Move);
                            }
                            finally
                            {
                                _dragActive = false;
                                _dropLocationIndex = -1;

                                if (Controls.Contains(_dropLocationIndicator))
                                {
                                    Controls.Remove(_dropLocationIndicator);
                                }

                                // If drag/drop scrolling was active when the drag/drop event ends,
                                // stop the scrolling now.
                                if (_dragDropScrollTimer.Enabled)
                                {
                                    _dragDropScrollTimer.Stop();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI49929");
                if (displayException)
                {
                    ee.Display();
                }
                else
                {
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Handling for drag/drop implementation that should occur when a
        /// <see cref="PaginationSeparator"/> is collapsed/expanded.
        /// </summary>
        /// <param name="separator">The separator that was collapsed or expanded.</param>
        /// <returns><c>true</c> if this event should be considered handled by drag/drop purposes;
        /// <c>false</c> if the event should be processed outside the context of drag/drop.</returns>
        bool DragDrop_HandleDocumentCollapsedChanged(PaginationSeparator separator)
        {
            if (!separator.Collapsed && separator == _activeDragTarget)
            {
                // Drag hover over separator has caused it to expand; show page drop indicator after
                // last existing page in the document.
                separator.ShowDragHints = false;
                _activeDragTarget = separator.Document.PageControls.Last();
                ShowDropLocationIndicator(_activeDragTarget.TrailingInsertionPoint, _activeDragTarget.Height);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Disposes resources used for drag/drop operations.
        /// </summary>
        void DragDrop_Dispose()
        {
            if (_dragDropScrollTimer != null)
            {
                _dragDropScrollTimer.Dispose();
                _dragDropScrollTimer = null;
            }

            if (_dropLocationIndicator != null)
            {
                _dropLocationIndicator.Dispose();
                _dropLocationIndicator = null;
            }
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.DragEnter"/> event of a child control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing
        /// the event data.</param>
        void Handle_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(_DRAG_DROP_DATA_FORMAT))
                {
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    // No data or unsupported data type.
                    e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35447");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.DragLeave"/> event of a child control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Handle_DragLeave(object sender, EventArgs e)
        {
            try
            {
                EnableDragScrolling(Control.MousePosition);

                _dropLocationIndex = -1;

                if (Controls.Contains(_dropLocationIndicator))
                {
                    Controls.Remove(_dropLocationIndicator);
                }

                ResetActiveDragTarget();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35448");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.DragOver"/> event of a child control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance
        /// containing the event data.</param>
        void Handle_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                EnableDragScrolling(new Point(e.X, e.Y));
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI35449");
            }

            try
            {
                Point dragLocation = PointToClient(new Point(e.X, e.Y));
                if (ShowDropLocationIndicator(dragLocation))
                {
                    e.Effect = e.AllowedEffect;
                }
                else
                {
                    _dropLocationIndex = -1;
                    e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35450");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.DragDrop"/> event of a child control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing
        /// the event data.</param>
        void Handle_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (_dropLocationIndex >= 0)
                {
                    var sourceLayoutControl =
                        e.Data.GetData(_DRAG_DROP_DATA_FORMAT) as PageLayoutControl;
                    if (sourceLayoutControl != null)
                    {
                        MoveSelectedControls(sourceLayoutControl, _dropLocationIndex);

                        if (_activeDragTarget is PaginationSeparator separator)
                        {
                            if (separator.Collapsed)
                            {
                                separator.Collapsed = false;
                            }

                            // Reset selection so that the first dropped page (rather than the separator)
                            // is now the primary selection
                            ProcessControlSelection(
                                activeControl: SelectedControls.First(),
                                additionalControls: SelectedControls.ToArray(),
                                select: true,
                                modifierKeys: Keys.None);
                        }

                        // Whenever pages are dropped, ensure after the drop, the first dropped page is in view.
                        _flowLayoutPanel.RequestScrollToControl(
                            control: SelectedControls.First(),
                            topAlignmentOffset: null,
                            activateScrollToControlForEvent: true);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35451");
            }
            finally
            {
                try
                {
                    _dropLocationIndex = -1;

                    if (Controls.Contains(_dropLocationIndicator))
                    {
                        Controls.Remove(_dropLocationIndicator);
                    }

                    ResetActiveDragTarget();
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI35619");
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="Timer.Tick"/> event of the <see cref="_dragDropScrollTimer"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDragDropScrollTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var vScroll = _flowLayoutPanel.VerticalScroll;
                bool scrolledToTop = _scrollSpeed < 0 && vScroll.Value <= vScroll.Minimum;
                bool scrolledToBottom = _scrollSpeed > 0
                    && (vScroll.Value + _flowLayoutPanel.Height) >= vScroll.Maximum;

                if (DocumentInDataEdit != null
                    && !_allDocumentsShowing
                    && (scrolledToTop || scrolledToBottom))
                {
                    var scrollPosition = DocumentInDataEdit.PaginationSeparator.Location.Y;

                    // If dragging to top/bottom of panel while a single document is displayed via
                    // SnapDataPanelToTop, redisplay all other documents to allow the pages being
                    // dragged to be dropped into other documents.
                    RedisplayAllDocuments();

                    // After displaying all documents, the panel will be scrolled to the top; restore
                    // scroll position relative to the top of the document from which we are dragging.
                    _flowLayoutPanel.ScrollControlIntoViewManual(DocumentInDataEdit.PaginationSeparator, scrollPosition);

                    return;
                }

                // Determine the existing scroll position (or what it should be as a result of the
                // last tick event).
                int lastScrollPos = vScroll.Value;
                if (_dragDropScrollPos >= 0)
                {
                    lastScrollPos = (_scrollSpeed > 0)
                        ? Math.Max(_dragDropScrollPos, lastScrollPos)
                        : Math.Min(_dragDropScrollPos, lastScrollPos);
                }

                _dragDropScrollPos = lastScrollPos + _scrollSpeed;

                // Ensure the scroll position stays within range.
                if (_dragDropScrollPos < vScroll.Minimum)
                {
                    _dragDropScrollPos = vScroll.Minimum;
                }
                else if (_dragDropScrollPos > vScroll.Maximum)
                {
                    _dragDropScrollPos = vScroll.Maximum;
                }

                vScroll.Value = _dragDropScrollPos;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35581");
            }
        }

        #endregion Event Handlers
    }
}