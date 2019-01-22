using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Extract.Utilities.Forms;
using Leadtools;
using Leadtools.WinForms;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.Imaging.Forms
{
    [CLSCompliant(false)]
    public interface IDocumentViewer: IDisposable
    {
        bool AllowBandedSelection { get; set; }

        bool AllowHighlight { get; set; }

        bool AutoOcr { get; set; }

        bool AutoZoomed { get; }

        int AutoZoomScale { get; set; }

        bool CacheImages { get; set; }

        bool CanGoToNextLayerObject { get; }

        bool CanGoToPreviousLayerObject { get; }

        bool CanZoomNext { get; }

        bool CanZoomPrevious { get; }

        ContextMenuStrip ContextMenuStrip { get; set; }

        CursorTool CursorTool { get; set; }

        Color DefaultHighlightColor { get; set; }

        int DefaultHighlightHeight { get; set; }

        RedactionColor DefaultRedactionFillColor { get; set; }

        string DefaultStatusMessage { get; set; }

        bool DisplayAnnotations { get; set; }

        FitMode FitMode { get; set; }

        RasterImage Image { get; set; }

        string ImageFile { get; }

        int ImageHeight { get; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        ReadOnlyCollection<ImagePageData> ImagePageData { get; set; }

        int ImageWidth { get; }

        bool InvertColors { get; set; }

        bool IsFirstDocumentTile { get; }

        void EnlargeSelectedZones();

        void ShrinkSelectedZones();


        bool IsFirstPage { get; }

        bool IsFirstTile { get; }


        void BlockFitSelectedZones();


        bool IsLastDocumentTile { get; }

        bool IsLastPage { get; }

        bool IsLastTile { get; }

        bool IsLoadingData { get; }

        bool IsImageAvailable { get; }

        bool IsSelectionInView { get; }

        bool IsTracking { get; }

        LayerObjectsCollection LayerObjects { get; }

        bool MaintainZoomLevelForNewPages { get; set; }

        int MinimumAngularHighlightHeight { get; set; }

        ThreadSafeSpatialString OcrData { get; set; }

        OcrTradeoff OcrTradeoff { get; set; }

        int OpenImageFileTypeFilterIndex { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        List<string> OpenImageFileTypeFilterList { get; }

        int Orientation { get; set; }

        int PageCount { get; }

        int PageNumber { get; set; }

        bool RecognizeHighlightText { get; set; }

        bool RedactionMode { get; set; }

        IEnumerable<LayerObject> SelectedLayerObjectsOnVisiblePage { get; }

        ShortcutsManager Shortcuts { get; }

        //RasterPaintSizeMode SizeMode { get; set; }

        LongToObjectMap SpatialPageInfos { get; }

        Matrix Transform { get; }

        bool UseAntiAliasing { get; set; }

        bool UseDefaultShortcuts { get; set; }

        string Watermark { get; set; }

        bool WordHighlightToolEnabled { get; set; }

        int ZoomHistoryCount { get; }

        ZoomInfo ZoomInfo { get; set; }


        event EventHandler<EventArgs> AllowHighlightStatusChanged;
        event EventHandler<BackgroundProcessStatusUpdateEventArgs> BackgroundProcessStatusUpdate;
        event EventHandler<LayerObjectEventArgs> CursorEnteredLayerObject;
        event EventHandler<LayerObjectEventArgs> CursorLeftLayerObject;
        event EventHandler<CursorToolChangedEventArgs> CursorToolChanged;
        event EventHandler<DisplayingPrintDialogEventArgs> DisplayingPrintDialog;
        event EventHandler<ExtendedNavigationEventArgs> ExtendedNavigation;
        event EventHandler<ExtendedNavigationCheckEventArgs> ExtendedNavigationCheck;
        event EventHandler<FileOpenErrorEventArgs> FileOpenError;
        event EventHandler<FitModeChangedEventArgs> FitModeChanged;
        event EventHandler<ImageExtractedEventArgs> ImageExtracted;
        event EventHandler<ImageFileChangedEventArgs> ImageFileChanged;
        event EventHandler<ImageFileClosingEventArgs> ImageFileClosing;
        event EventHandler<EventArgs> InvertColorsStatusChanged;
        event EventHandler<LoadingNewImageEventArgs> LoadingNewImage;
        event EventHandler<OrientationChangedEventArgs> NonDisplayedPageOrientationChanged;
        event EventHandler<OcrTextEventArgs> OcrLoaded;
        event EventHandler<OcrTextEventArgs> OcrTextHighlighted;
        event EventHandler<OpeningImageEventArgs> OpeningImage;
        event EventHandler<OrientationChangedEventArgs> OrientationChanged;
        event EventHandler<PageChangedEventArgs> PageChanged;
        event EventHandler<ZoomChangedEventArgs> ZoomChanged;
        event EventHandler ScrollPositionChanged;
        event EventHandler HandleDestroyed;
        event PaintEventHandler PostImagePaint;
        event EventHandler ImageChanged;
        event PaintEventHandler Paint;

        void AddDisallowedPrinter(string printerName);

        void BeginUpdate();

        void BringSelectionIntoView(bool autoZoom);

        void CacheImage(string fileName);

        void CenterAtPoint(Point pt);

        void CenterOnLayerObjects(params LayerObject[] layerObjects);

        void CloseImage();

        void CloseImage(bool unloadImage);

        bool Contains(LayerObject layerObject);

        bool Contains(RectangleF rectangle);

        IEnumerable<CompositeHighlightLayerObject> CreateHighlights(SpatialString spatialString, Color color);

        void DecreaseHighlightHeight();

        void DisplayRasterImage(RasterImage image, int orientation, string imageFileName);

        void EndUpdate();

        void EstablishConnections(Control control);

        void EstablishConnections(ToolStripItem toolStripItem);

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        List<LayerObject> GetLayeredObjects(IEnumerable<string> requiredTags, ArgumentRequirement tagsArgumentRequirement);

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        List<LayerObject> GetLayeredObjects(IEnumerable<string> requiredTags, ArgumentRequirement tagsArgumentRequirement, IEnumerable<Type> requiredTypes, ArgumentRequirement requiredTypesArgumentRequirement, IEnumerable<Type> excludeTypes, ArgumentRequirement excludeTypesArgumentRequirement);

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        List<LayerObject> GetLayeredObjectsOnPage(int pageNumber, IEnumerable<string> requiredTags, ArgumentRequirement tagsArgumentRequirement);

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        List<LayerObject> GetLayeredObjectsOnPage(int pageNumber, IEnumerable<string> requiredTags, ArgumentRequirement tagsArgumentRequirement, IEnumerable<Type> requiredTypes, ArgumentRequirement requiredTypesArgumentRequirement, IEnumerable<Type> excludeTypes, ArgumentRequirement excludeTypesArgumentRequirement);

        SpatialString GetOcrTextFromZone(RasterZone rasterZone);

        ImagePageProperties GetPageProperties(int pageNumber);

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        double GetScaleFactorY();

        Rectangle GetTransformedRectangle(Rectangle rectangle, bool clientToImage);

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        Rectangle GetVisibleImageArea();

        void GoToFirstPage();

        void GoToLastPage();

        void GoToNextLayerObject();

        void GoToNextPage();

        void GoToNextVisibleLayerObject(bool selectObject);

        void GoToPreviousLayerObject();

        void GoToPreviousPage();

        void GoToPreviousVisibleLayerObject(bool selectObject);

        void IncreaseHighlightHeight();

        bool Intersects(LayerObject layerObject);

        bool Intersects(RectangleF rectangle);

        void OpenImage(string fileName, bool updateMruList);

        void OpenImage(string fileName, bool updateMruList, bool refreshBeforeLoad);

        Rectangle PadViewingRectangle(Rectangle viewRectangle, int horizontalPadding, int verticalPadding, bool transferPaddingIfOffPage);

        void PaintToGraphics(Graphics graphics, Rectangle clip, Point centerPoint, double scaleFactor);

        void Print();

        void PrintPreview();

        void PrintView();

        void RemoveDisallowedPrinter(string printerName);

        void RestoreNonSelectionZoom();

        void RestoreScrollPosition();

        void Rotate(int angle, bool updateZoomHistory, bool raiseZoomChanged);

        void RotateAllDocumentPages(int angle, bool updateZoomHistory, bool raiseZoomChanged);

        void SaveImage(string fileName, RasterImageFormat format);

        void SelectAngularHighlightTool();

        void SelectDeleteLayerObjectsTool();

        void SelectEditHighlightTextTool();

        void SelectFirstDocumentTile(double scaleFactor, FitMode fitMode);

        void SelectLastDocumentTile(double scaleFactor, FitMode fitMode);

        void SelectNextTile();

        void SelectOpenImage();

        void SelectPanTool();

        void SelectPreviousTile();

        void SelectPrint();

        void SelectPrintView();

        void SelectRectangularHighlightTool();

        void SelectRemoveSelectedLayerObjects();

        void SelectRotateAllDocumentPagesClockwise();

        void SelectRotateAllDocumentPagesCounterclockwise();

        void SelectRotateClockwise();

        void SelectRotateCounterclockwise();

        void SelectSelectAllLayerObjects();

        void SelectSelectLayerObjectsTool();

        void SelectWordHighlightTool();

        void SelectZoomIn();

        void SelectZoomNext();

        void SelectZoomOut();

        void SelectZoomPrevious();

        void SelectZoomWindowTool();

        void SetVisibleStateForSpecifiedLayerObjects(IEnumerable<string> tags, IEnumerable<Type> includeTypes, bool visibleState);

        void ShowPrintPageSetupDialog();

        void ToggleFitToPageMode();

        void ToggleFitToWidthMode();

        void ToggleHighlightTool();

        void ToggleRedactionTool();

        void UnloadImage(string fileName);

        void UpdateCursor();

        bool WaitForOcrData();

        void ZoomIn();

        void ZoomNext();

        void ZoomOut();

        void ZoomPrevious();

        void ZoomToRectangle(Rectangle rc);

        void Invalidate();

        Cursor Cursor { get; set; }

        Cursor GetSelectionCursor(int mouseX, int mouseY);

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y")]
        LayerObject GetLayerObjectAtPoint<T>(IEnumerable<LayerObject> layerObjects, int x, int y, bool onlySelectableObjects) where T : LayerObject;

        Rectangle PhysicalViewRectangle { get; }

        float ImageDpiY { get; }

        float ImageDpiX { get; }

        double ScaleFactor { get; }

        bool IsDisposed { get; }

        Color FrameColor { get; set; }

        IAsyncResult BeginInvoke(Delegate method);

    }

}
