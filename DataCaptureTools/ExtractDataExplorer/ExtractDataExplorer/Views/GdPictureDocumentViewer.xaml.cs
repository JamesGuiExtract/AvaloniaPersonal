using ExtractDataExplorer.Models;
using ExtractDataExplorer.ViewModels;
using GdPicture14;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using ReactiveMarbles.ObservableEvents;
using Extract;
using Extract.GdPicture;

namespace ExtractDataExplorer.Views
{
    /// <summary>
    /// Base class to hide the generic from WPF
    /// </summary>
    public class GdPictureDocumentViewerBase : ReactiveUserControl<DocumentViewModel> { }

    /// <summary>
    /// Interaction logic for GdPictureDocumentViewer.xaml
    /// </summary>
    public sealed partial class GdPictureDocumentViewer : GdPictureDocumentViewerBase
    {
        static readonly Color _highlightBorderColor = Color.LightSeaGreen;
        static readonly Color _highlightFillColor = Color.FromArgb(alpha: 100, baseColor: Color.Blue);

        readonly AnnotationManager _annotationMgr;

        public GdPictureDocumentViewer()
        {
            InitializeComponent();

            _annotationMgr = _documentViewer.GetAnnotationManager();

            // Disable native handling so that we can support ctrl+wheel = zoom, etc
            _documentViewer.EnableMouseWheel = false;

            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel, vm => vm.DocumentPath, v => v._documentPath.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.LoadDocumentText, v => v._loadDocumentButton.Content)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.LoadDocumentCommand, v => v._loadDocumentButton.Command)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.SelectDocumentCommand, v => v._selectDocumentButton.Command)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, vm => vm.CurrentPageNumber, v => v._pageNumber.Text,
                        signalViewUpdate: _pageNumber.Events().LostFocus)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.TotalPages, v => v._totalPages.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.NextPageCommand, v => v._nextPageButton.Command)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.PrevPageCommand, v => v._prevPageButton.Command)
                    .DisposeWith(disposables);

                // Update the VM's CurrentPageNumber in response to document viewer events
                Observable.CombineLatest(_documentViewer.Events().PageDisplayed, _documentViewer.Events().DocumentClosed, (_, _) => Unit.Default)
                    .Select(_ => GetCurrentPageNumber())
                    .BindTo(ViewModel, vm => vm.CurrentPageNumber)
                    .DisposeWith(disposables);

                // Handle zoom/scroll with mouse wheel
                _documentViewer.Events().MouseWheel
                    .Subscribe(HandleMouseWheel)
                    .DisposeWith(disposables);

                // Handle keyboard zoom
                _documentViewer.Events().KeyUp
                    .Subscribe(HandleKeyUp)
                    .DisposeWith(disposables);

                // Open/close the document in response to the VM
                ViewModel.WhenAnyValue(x => x.CurrentlyOpenDocumentPath)
                    .Subscribe(path =>
                    {
                        if (string.IsNullOrEmpty(path))
                        {
                            _documentViewer.CloseDocument();
                        }
                        else
                        {
                            GdPictureUtility.DisplayIfStatusNotOK(_documentViewer.DisplayFromFile(path),
                                "ELI56306", "Could not display document", new(filePath: path));
                        }

                        // Update the VM so that it shows the correct document path and page count
                        if (ViewModel is DocumentViewModel vm)
                        {
                            // Only set the current path if it's not empty
                            // (the load/reload button that takes care of indicating that the current path isn't loaded)
                            if (!string.IsNullOrEmpty(path))
                            {
                                vm.DocumentPath = path;
                            }

                            vm.TotalPages = _documentViewer.PageCount is int totalPages && totalPages > 0
                                ? totalPages
                                : null;
                        }

                    })
                    .DisposeWith(disposables);

                // Change page/highlights in response to the VM
                ViewModel.WhenAnyValue(x => x.CurrentPageNumber, x => x.HighlightedAreas,
                    (page, zones) => (page, zones))
                    .Subscribe(values =>
                    {
                        if (values.page is int pageNumber && pageNumber > 0 && pageNumber <= _documentViewer.PageCount)
                        {
                            GdPictureUtility.DisplayIfStatusNotOK(_documentViewer.DisplayPage(pageNumber),
                                "ELI56307", "Could not display page", new(pageNumber: pageNumber));
                        }
                        else
                        {
                            // Update the VM if the page number is out of range so that it will not show a bad value
                            if (ViewModel is DocumentViewModel vm)
                            {
                                vm.CurrentPageNumber = GetCurrentPageNumber();
                            }

                        }

                        if (values.zones is not null)
                        {
                            HighlightAreas(values.zones);
                        }
                    })
                    .DisposeWith(disposables);
            });
        }

        private int? GetCurrentPageNumber()
        {
            return _documentViewer.CurrentPage is int page && page > 0
                ? page
                : null;
        }

        // Replace any annotations with annotations derived from the specified polygons that are on the current page
        private void HighlightAreas(IList<Polygon> polygons)
        {
            _ = polygons ?? throw new ArgumentNullException(nameof(polygons));

            int? currentPage = GetCurrentPageNumber();

            polygons = polygons
                .Where(poly => poly.PageNumber == currentPage)
                .ToList();

            RemoveAllAnnotations();

            // Extract uses 300x300 dpi for PDF rendering but the GdViewer uses native PDF coordinates adjusted by the zoom level
            double pdfDpiAdjustmentFactor = _documentViewer.GetDocumentType() == DocumentType.DocumentTypePDF
                ? 300.0 / 96.0 / _documentViewer.Zoom
                : 1.0;

            float dpiX = (float)(_documentViewer.PageHorizontalResolution * pdfDpiAdjustmentFactor);
            float dpiY = (float)(_documentViewer.PageVerticalResolution * pdfDpiAdjustmentFactor);

            RectangleF bounds = AddPolygonHighlights(polygons, dpiX, dpiY);

            // Ensure the highlights are visible, with a little extra space around if possible
            float padding = 0.02f;
            RectangleF paddedBounds = new(
                x: Math.Max(0, bounds.Left - padding),
                y: Math.Max(0, bounds.Top - padding),
                width: (float)Math.Min(bounds.Right + padding, _documentViewer.PageInchWidth) - bounds.Left,
                height: (float)Math.Min(bounds.Bottom + padding, _documentViewer.PageInchHeight) - bounds.Top);

            EnsureVisible(paddedBounds);
        }

        // Add each polygon as a highlight and return the combined bounding rectangle
        private RectangleF AddPolygonHighlights(IList<Polygon> polygons, float dpiX, float dpiY)
        {
            RectangleF bounds = RectangleF.Empty;
            foreach (var polygon in polygons)
            {
                // Add an annotation for this polygon
                var annot = _annotationMgr.AddPolygonAnnot(
                    BorderColor: _highlightBorderColor,
                    BackColor: _highlightFillColor,
                    Points: ConvertPolygonToInches(polygon.Points, dpiX, dpiY));

                // Aggregate the bounds of the polygon
                var annotBounds = new RectangleF(annot.Left - annot.Width / 2, annot.Top - annot.Height / 2, annot.Width, annot.Height);
                if (bounds.IsEmpty)
                {
                    bounds = annotBounds;
                }
                else
                {
                    bounds = RectangleF.Union(bounds, annotBounds);
                }
            }

            return bounds;
        }

        // Convert an array of points from native units to inches
        private static PointF[] ConvertPolygonToInches(PointF[] polygon, float dpiX, float dpiY)
        {
            PointF[] converted = new PointF[polygon.Length];
            for (int i = 0; i < polygon.Length; i++)
            {
                converted[i] = new PointF(x: polygon[i].X / dpiX, y: polygon[i].Y / dpiY);
            }

            return converted;
        }

        private void RemoveAllAnnotations()
        {
            int annotationCount = _annotationMgr.GetAnnotationCount();
            for (int _ = 0; _ < annotationCount; _++)
            {
                _annotationMgr.DeleteAnnotation(0);
            }
        }

        // Make at least the top-left corner of the region visible by adjusting the scroll position
        private void EnsureVisible(RectangleF region)
        {
            GetBoundsInCanvasPixels(region, out double left, out double top, out double right, out double bottom);
            double canvasWidth = GetCanvasWidth();
            double canvasHeight = GetCanvasHeight();
            double horizontalDelta = 0.0;
            double verticalDelta = 0.0;
            if (left < 0.0)
            {
                horizontalDelta = left;
            }
            else if (right > canvasWidth && right - canvasWidth < left)
            {
                horizontalDelta = right - canvasWidth;
            }
            else if (left > canvasWidth)
            {
                horizontalDelta = left;
            }

            if (top < 0.0)
            {
                verticalDelta = top;
            }
            else if (bottom > canvasHeight && bottom - canvasHeight < top)
            {
                verticalDelta = bottom - canvasHeight;
            }
            else if (top > canvasHeight)
            {
                verticalDelta = top;
            }

            if (horizontalDelta != 0.0 || verticalDelta != 0.0)
            {
                _documentViewer.SetScrollBarsPosition(
                    NewHPos: _documentViewer.GetHorizontalScrollBarPosition() + horizontalDelta,
                    NewVPos: _documentViewer.GetVerticalScrollBarPosition() + verticalDelta);
            }
        }

        // Translate a region defined with inches into canvas pixels
        private void GetBoundsInCanvasPixels(RectangleF region, out double left, out double top, out double right, out double bottom)
        {
            double viewerLeft = 0, viewerRight = 0, viewerTop = 0, viewerBottom = 0;
            _documentViewer.CoordDocumentInchToViewerPixel(region.Left, region.Top, ref viewerLeft, ref viewerTop);
            _documentViewer.CoordDocumentInchToViewerPixel(region.Right, region.Top, ref viewerRight, ref viewerTop);
            _documentViewer.CoordDocumentInchToViewerPixel(region.Left, region.Bottom, ref viewerLeft, ref viewerBottom);
            left = Math.Min(viewerLeft, viewerRight);
            right = Math.Max(viewerLeft, viewerRight);
            top = Math.Min(viewerTop, viewerBottom);
            bottom = Math.Max(viewerTop, viewerBottom);
        }

        private double GetCanvasHeight()
        {
            double scrollBarHeight = _documentViewer.HorizontalScrollBarVisible ? SystemParameters.HorizontalScrollBarHeight : 0;
            return _documentViewer.ActualHeight - scrollBarHeight;
        }

        private double GetCanvasWidth()
        {
            double scrollBarWidth = _documentViewer.VerticalScrollBarVisible ? SystemParameters.VerticalScrollBarWidth : 0;
            return _documentViewer.ActualWidth - scrollBarWidth;
        }

        private void HandleMouseWheel(MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                HandleMouseZoom(e);
            }
            else
            {
                HandleMouseScroll(e);
            }
        }

        private void HandleMouseZoom(MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                _documentViewer.ZoomIN();
            }
            else
            {
                _documentViewer.ZoomOUT();
            }
        }

        private void HandleMouseScroll(MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                _documentViewer.SetHorizontalScrollBarPosition(_documentViewer.GetHorizontalScrollBarPosition() - e.Delta);
            }
            else
            {
                _documentViewer.SetVerticalScrollBarPosition(_documentViewer.GetVerticalScrollBarPosition() - e.Delta);
            }
        }

        private void HandleKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Add || e.Key == Key.OemPlus)
            {
                _documentViewer.ZoomIN();
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                _documentViewer.ZoomOUT();
            }
        }
    }
}
