using System;
using TD.SandDock;

namespace Extract.FileActionManager.FileProcessors
{
    sealed partial class ViewImageTaskForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripContainer imageViewerToolStripContainer;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewImageTaskForm));
            System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
            TD.SandDock.DockContainer dockContainer1;
            this._imageViewerStatusStrip = new Extract.Imaging.Forms.ImageViewerStatusStrip();
            this._imageViewer = new Extract.Imaging.Forms.ImageViewer();
            this._imageViewerContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._zoomPreviousToolStripMenuItem1 = new Extract.Imaging.Forms.ZoomPreviousToolStripMenuItem();
            this._zoomWindowToolStripMenuItem1 = new Extract.Imaging.Forms.ZoomWindowToolStripMenuItem();
            this._panToolStripMenuItem1 = new Extract.Imaging.Forms.PanToolStripMenuItem();
            this._selectLayerObjectToolStripMenuItem1 = new Extract.Imaging.Forms.SelectLayerObjectToolStripMenuItem();
            this._angularRedactionToolStripMenuItem1 = new Extract.Imaging.Forms.AngularRedactionToolStripMenuItem();
            this._rectangularRedactionToolStripMenuItem1 = new Extract.Imaging.Forms.RectangularRedactionToolStripMenuItem();
            this._wordRedactionToolStripMenuItem1 = new Extract.Imaging.Forms.WordRedactionToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this._blockFitSelectionToolStripMenuItem1 = new Extract.Imaging.Forms.BlockFitSelectionToolStripMenuItem();
            this._enlargeRedactionToolStripMenuItem = new Extract.Imaging.Forms.EnlargeSelectionToolStripMenuItem();
            this._shrinkRedactionToolStripMenuItem = new Extract.Imaging.Forms.ShrinkSelectionToolStripMenuItem();
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this._fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._nextDocumentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._printImageToolStripMenuItem = new Extract.Imaging.Forms.PrintImageToolStripMenuItem();
            this._printViewToolStripMenuItem = new Extract.Imaging.Forms.PrintViewToolStripMenuItem();
            this._skipProcessingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._stopProcessingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._zoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._fitToPageToolStripMenuItem = new Extract.Imaging.Forms.FitToPageToolStripMenuItem();
            this._fitToWidthToolStripMenuItem = new Extract.Imaging.Forms.FitToWidthToolStripMenuItem();
            this._oneToOneZoomToolStripMenuItem = new Extract.Imaging.Forms.OneToOneZoomToolStripMenuItem();
            this._toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomInToolStripMenuItem = new Extract.Imaging.Forms.ZoomInToolStripMenuItem();
            this._zoomOutToolStripMenuItem = new Extract.Imaging.Forms.ZoomOutToolStripMenuItem();
            this._toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomPreviousToolStripMenuItem = new Extract.Imaging.Forms.ZoomPreviousToolStripMenuItem();
            this._zoomNextToolStripMenuItem = new Extract.Imaging.Forms.ZoomNextToolStripMenuItem();
            this._rotateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._rotateCounterclockwiseToolStripMenuItem = new Extract.Imaging.Forms.RotateCounterclockwiseToolStripMenuItem();
            this._rotateClockwiseToolStripMenuItem = new Extract.Imaging.Forms.RotateClockwiseToolStripMenuItem();
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem = new Extract.Imaging.Forms.RotateAllDocumentPagesCounterclockwiseToolStripMenuItem();
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem = new Extract.Imaging.Forms.RotateAllDocumentPagesClockwiseToolStripMenuItem();
            this._gotoPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._firstPageToolStripMenuItem = new Extract.Imaging.Forms.FirstPageToolStripMenuItem();
            this._previousPageToolStripMenuItem = new Extract.Imaging.Forms.PreviousPageToolStripMenuItem();
            this._toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this._pageNavigationToolStripMenuItem = new Extract.Imaging.Forms.PageNavigationToolStripMenuItem();
            this._toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this._nextPageToolStripMenuItem = new Extract.Imaging.Forms.NextPageToolStripMenuItem();
            this._lastPageToolStripMenuItem = new Extract.Imaging.Forms.LastPageToolStripMenuItem();
            this._toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._zoomWindowToolStripMenuItem = new Extract.Imaging.Forms.ZoomWindowToolStripMenuItem();
            this._panToolStripMenuItem = new Extract.Imaging.Forms.PanToolStripMenuItem();
            this._basicDataGridToolStrip = new System.Windows.Forms.ToolStrip();
            this._nextDocumentToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._tagFileToolStripButton = new Extract.FileActionManager.Forms.TagFileToolStripButton();
            this._basicImageViewerToolStrip = new System.Windows.Forms.ToolStrip();
            this._printImageToolStripButton = new Extract.Imaging.Forms.PrintImageToolStripButton();
            this._zoomWindowToolStripButton = new Extract.Imaging.Forms.ZoomWindowToolStripButton();
            this._panToolStripButton = new Extract.Imaging.Forms.PanToolStripButton();
            this._pageNavigationToolStrip = new System.Windows.Forms.ToolStrip();
            this._firstPageToolStripButton = new Extract.Imaging.Forms.FirstPageToolStripButton();
            this._previousPageToolStripButton = new Extract.Imaging.Forms.PreviousPageToolStripButton();
            this._pageNavigationToolStripTextBox = new Extract.Imaging.Forms.PageNavigationToolStripTextBox();
            this._nextPageToolStripButton = new Extract.Imaging.Forms.NextPageToolStripButton();
            this._lastPageToolStripButton = new Extract.Imaging.Forms.LastPageToolStripButton();
            this._viewCommandsToolStrip = new System.Windows.Forms.ToolStrip();
            this._zoomInToolStripButton = new Extract.Imaging.Forms.ZoomInToolStripButton();
            this._zoomOutToolStripButton = new Extract.Imaging.Forms.ZoomOutToolStripButton();
            this._zoomPreviousToolStripButton = new Extract.Imaging.Forms.ZoomPreviousToolStripButton();
            this._zoomNextToolStripButton = new Extract.Imaging.Forms.ZoomNextToolStripButton();
            this._fitToPageToolStripButton = new Extract.Imaging.Forms.FitToPageToolStripButton();
            this._fitToWidthToolStripButton = new Extract.Imaging.Forms.FitToWidthToolStripButton();
            this._oneToOneZoomToolStripButton = new Extract.Imaging.Forms.OneToOneZoomToolStripButton();
            this._rotateCounterclockwiseToolStripButton = new Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton();
            this._rotateClockwiseToolStripButton = new Extract.Imaging.Forms.RotateClockwiseToolStripButton();
            this._thumbnailsToolStripButton = new Extract.Imaging.Forms.ThumbnailViewerToolStripButton();
            this._magnifierToolStripButton = new Extract.Imaging.Forms.MagnifierWindowToolStripButton();
            this._thumbnailDockableWindow = new TD.SandDock.DockableWindow();
            this._thumbnailViewer = new Extract.Imaging.Forms.ThumbnailViewer();
            this._sandDockManager = new TD.SandDock.SandDockManager();
            this._magnifierDockableWindow = new TD.SandDock.DockableWindow();
            this._magnifierControl = new Extract.Imaging.Forms.MagnifierControl();
            this.dockContainer2 = new TD.SandDock.DockContainer();
            imageViewerToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            dockContainer1 = new TD.SandDock.DockContainer();
            imageViewerToolStripContainer.BottomToolStripPanel.SuspendLayout();
            imageViewerToolStripContainer.ContentPanel.SuspendLayout();
            imageViewerToolStripContainer.TopToolStripPanel.SuspendLayout();
            imageViewerToolStripContainer.SuspendLayout();
            this._imageViewerContextMenu.SuspendLayout();
            this._menuStrip.SuspendLayout();
            this._basicDataGridToolStrip.SuspendLayout();
            this._basicImageViewerToolStrip.SuspendLayout();
            this._pageNavigationToolStrip.SuspendLayout();
            this._viewCommandsToolStrip.SuspendLayout();
            dockContainer1.SuspendLayout();
            this._thumbnailDockableWindow.SuspendLayout();
            this._magnifierDockableWindow.SuspendLayout();
            this.dockContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageViewerToolStripContainer
            // 
            // 
            // imageViewerToolStripContainer.BottomToolStripPanel
            // 
            imageViewerToolStripContainer.BottomToolStripPanel.Controls.Add(this._imageViewerStatusStrip);
            // 
            // imageViewerToolStripContainer.ContentPanel
            // 
            imageViewerToolStripContainer.ContentPanel.Controls.Add(this._imageViewer);
            imageViewerToolStripContainer.ContentPanel.Size = new System.Drawing.Size(1388, 614);
            imageViewerToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            imageViewerToolStripContainer.Location = new System.Drawing.Point(0, 0);
            imageViewerToolStripContainer.Name = "imageViewerToolStripContainer";
            imageViewerToolStripContainer.Size = new System.Drawing.Size(1388, 701);
            imageViewerToolStripContainer.TabIndex = 1;
            imageViewerToolStripContainer.Text = "toolStripContainer1";
            // 
            // imageViewerToolStripContainer.TopToolStripPanel
            // 
            imageViewerToolStripContainer.TopToolStripPanel.Controls.Add(this._menuStrip);
            imageViewerToolStripContainer.TopToolStripPanel.Controls.Add(this._basicDataGridToolStrip);
            imageViewerToolStripContainer.TopToolStripPanel.Controls.Add(this._basicImageViewerToolStrip);
            imageViewerToolStripContainer.TopToolStripPanel.Controls.Add(this._pageNavigationToolStrip);
            imageViewerToolStripContainer.TopToolStripPanel.Controls.Add(this._viewCommandsToolStrip);
            // 
            // _imageViewerStatusStrip
            // 
            this._imageViewerStatusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._imageViewerStatusStrip.Location = new System.Drawing.Point(0, 0);
            this._imageViewerStatusStrip.Name = "_imageViewerStatusStrip";
            this._imageViewerStatusStrip.ShowBackgroundProcessStatus = false;
            this._imageViewerStatusStrip.Size = new System.Drawing.Size(1388, 24);
            this._imageViewerStatusStrip.TabIndex = 0;
            // 
            // _imageViewer
            // 
            this._imageViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._imageViewer.AutoOcr = false;
            this._imageViewer.AutoZoomScale = 0;
            this._imageViewer.ContextMenuStrip = this._imageViewerContextMenu;
            this._imageViewer.InvertColors = false;
            this._imageViewer.Location = new System.Drawing.Point(0, 0);
            this._imageViewer.MinimumAngularHighlightHeight = 4;
            this._imageViewer.Name = "_imageViewer";
            this._imageViewer.OcrTradeoff = Extract.Imaging.OcrTradeoff.Accurate;
            this._imageViewer.RedactionMode = true;
            this._imageViewer.Size = new System.Drawing.Size(1388, 612);
            this._imageViewer.TabIndex = 0;
            this._imageViewer.TabStop = false;
            this._imageViewer.UseDefaultShortcuts = true;
            this._imageViewer.WordHighlightToolEnabled = true;
            // 
            // _imageViewerContextMenu
            // 
            this._imageViewerContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomPreviousToolStripMenuItem1,
            toolStripSeparator14,
            this._zoomWindowToolStripMenuItem1,
            this._panToolStripMenuItem1,
            this._selectLayerObjectToolStripMenuItem1,
            toolStripSeparator11,
            this._angularRedactionToolStripMenuItem1,
            this._rectangularRedactionToolStripMenuItem1,
            this._wordRedactionToolStripMenuItem1,
            this.toolStripSeparator12,
            this._blockFitSelectionToolStripMenuItem1,
            this._enlargeRedactionToolStripMenuItem,
            this._shrinkRedactionToolStripMenuItem});
            this._imageViewerContextMenu.Name = "_imageViewerContextMenu";
            this._imageViewerContextMenu.Size = new System.Drawing.Size(259, 242);
            // 
            // _zoomPreviousToolStripMenuItem1
            // 
            this._zoomPreviousToolStripMenuItem1.Enabled = false;
            this._zoomPreviousToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("_zoomPreviousToolStripMenuItem1.Image")));
            this._zoomPreviousToolStripMenuItem1.ImageViewer = null;
            this._zoomPreviousToolStripMenuItem1.Name = "_zoomPreviousToolStripMenuItem1";
            this._zoomPreviousToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._zoomPreviousToolStripMenuItem1.Size = new System.Drawing.Size(258, 22);
            this._zoomPreviousToolStripMenuItem1.Text = "Zoom previous";
            // 
            // toolStripSeparator14
            // 
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new System.Drawing.Size(255, 6);
            // 
            // _zoomWindowToolStripMenuItem1
            // 
            this._zoomWindowToolStripMenuItem1.Enabled = false;
            this._zoomWindowToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("_zoomWindowToolStripMenuItem1.Image")));
            this._zoomWindowToolStripMenuItem1.ImageViewer = null;
            this._zoomWindowToolStripMenuItem1.Name = "_zoomWindowToolStripMenuItem1";
            this._zoomWindowToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._zoomWindowToolStripMenuItem1.Size = new System.Drawing.Size(258, 22);
            this._zoomWindowToolStripMenuItem1.Text = "&Zoom window";
            // 
            // _panToolStripMenuItem1
            // 
            this._panToolStripMenuItem1.Enabled = false;
            this._panToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("_panToolStripMenuItem1.Image")));
            this._panToolStripMenuItem1.ImageViewer = null;
            this._panToolStripMenuItem1.Name = "_panToolStripMenuItem1";
            this._panToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._panToolStripMenuItem1.Size = new System.Drawing.Size(258, 22);
            this._panToolStripMenuItem1.Text = "P&an";
            // 
            // _selectLayerObjectToolStripMenuItem1
            // 
            this._selectLayerObjectToolStripMenuItem1.Enabled = false;
            this._selectLayerObjectToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("_selectLayerObjectToolStripMenuItem1.Image")));
            this._selectLayerObjectToolStripMenuItem1.ImageViewer = null;
            this._selectLayerObjectToolStripMenuItem1.Name = "_selectLayerObjectToolStripMenuItem1";
            this._selectLayerObjectToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._selectLayerObjectToolStripMenuItem1.Size = new System.Drawing.Size(258, 22);
            this._selectLayerObjectToolStripMenuItem1.Text = "Select redactions and other objects";
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new System.Drawing.Size(255, 6);
            // 
            // _angularRedactionToolStripMenuItem1
            // 
            this._angularRedactionToolStripMenuItem1.Enabled = false;
            this._angularRedactionToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("_angularRedactionToolStripMenuItem1.Image")));
            this._angularRedactionToolStripMenuItem1.ImageViewer = null;
            this._angularRedactionToolStripMenuItem1.Name = "_angularRedactionToolStripMenuItem1";
            this._angularRedactionToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._angularRedactionToolStripMenuItem1.Size = new System.Drawing.Size(258, 22);
            this._angularRedactionToolStripMenuItem1.Text = "A&ngular redaction";
            // 
            // _rectangularRedactionToolStripMenuItem1
            // 
            this._rectangularRedactionToolStripMenuItem1.Enabled = false;
            this._rectangularRedactionToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("_rectangularRedactionToolStripMenuItem1.Image")));
            this._rectangularRedactionToolStripMenuItem1.ImageViewer = null;
            this._rectangularRedactionToolStripMenuItem1.Name = "_rectangularRedactionToolStripMenuItem1";
            this._rectangularRedactionToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._rectangularRedactionToolStripMenuItem1.Size = new System.Drawing.Size(258, 22);
            this._rectangularRedactionToolStripMenuItem1.Text = "&Rectangular redaction";
            // 
            // _wordRedactionToolStripMenuItem1
            // 
            this._wordRedactionToolStripMenuItem1.Enabled = false;
            this._wordRedactionToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("_wordRedactionToolStripMenuItem1.Image")));
            this._wordRedactionToolStripMenuItem1.ImageViewer = null;
            this._wordRedactionToolStripMenuItem1.Name = "_wordRedactionToolStripMenuItem1";
            this._wordRedactionToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._wordRedactionToolStripMenuItem1.Size = new System.Drawing.Size(258, 22);
            this._wordRedactionToolStripMenuItem1.Text = "&Word redaction";
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(255, 6);
            // 
            // _blockFitSelectionToolStripMenuItem1
            // 
            this._blockFitSelectionToolStripMenuItem1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._blockFitSelectionToolStripMenuItem1.Enabled = false;
            this._blockFitSelectionToolStripMenuItem1.ImageViewer = null;
            this._blockFitSelectionToolStripMenuItem1.Name = "_blockFitSelectionToolStripMenuItem1";
            this._blockFitSelectionToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._blockFitSelectionToolStripMenuItem1.Size = new System.Drawing.Size(258, 22);
            this._blockFitSelectionToolStripMenuItem1.Text = "Auto-shrink selection";
            // 
            // _enlargeRedactionToolStripMenuItem
            // 
            this._enlargeRedactionToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._enlargeRedactionToolStripMenuItem.Enabled = false;
            this._enlargeRedactionToolStripMenuItem.ImageViewer = null;
            this._enlargeRedactionToolStripMenuItem.Name = "_enlargeRedactionToolStripMenuItem";
            this._enlargeRedactionToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._enlargeRedactionToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this._enlargeRedactionToolStripMenuItem.Text = "Enlarge selection by 1 pixel";
            // 
            // _shrinkRedactionToolStripMenuItem
            // 
            this._shrinkRedactionToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._shrinkRedactionToolStripMenuItem.Enabled = false;
            this._shrinkRedactionToolStripMenuItem.ImageViewer = null;
            this._shrinkRedactionToolStripMenuItem.Name = "_shrinkRedactionToolStripMenuItem";
            this._shrinkRedactionToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._shrinkRedactionToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this._shrinkRedactionToolStripMenuItem.Text = "Shrink selection by 1 pixel";
            // 
            // _menuStrip
            // 
            this._menuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileToolStripMenuItem,
            this._viewToolStripMenuItem,
            this._toolsToolStripMenuItem});
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.Size = new System.Drawing.Size(1388, 24);
            this._menuStrip.TabIndex = 3;
            this._menuStrip.Text = "menuStrip1";
            // 
            // _fileToolStripMenuItem
            // 
            this._fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._nextDocumentToolStripMenuItem,
            toolStripSeparator7,
            this._printImageToolStripMenuItem,
            this._printViewToolStripMenuItem,
            toolStripSeparator8,
            this._skipProcessingToolStripMenuItem,
            this._stopProcessingToolStripMenuItem});
            this._fileToolStripMenuItem.Name = "_fileToolStripMenuItem";
            this._fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this._fileToolStripMenuItem.Text = "File";
            // 
            // _nextDocumentToolStripMenuItem
            // 
            this._nextDocumentToolStripMenuItem.Name = "_nextDocumentToolStripMenuItem";
            this._nextDocumentToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Tab";
            this._nextDocumentToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this._nextDocumentToolStripMenuItem.Text = "Next document";
            this._nextDocumentToolStripMenuItem.Click += new System.EventHandler(this.HandleNextDocumentToolStripMenuItemClick);
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(193, 6);
            // 
            // _printImageToolStripMenuItem
            // 
            this._printImageToolStripMenuItem.Enabled = false;
            this._printImageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_printImageToolStripMenuItem.Image")));
            this._printImageToolStripMenuItem.ImageViewer = null;
            this._printImageToolStripMenuItem.Name = "_printImageToolStripMenuItem";
            this._printImageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._printImageToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this._printImageToolStripMenuItem.Text = "&Print...";
            // 
            // _printViewToolStripMenuItem
            // 
            this._printViewToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._printViewToolStripMenuItem.Enabled = false;
            this._printViewToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_printViewToolStripMenuItem.Image")));
            this._printViewToolStripMenuItem.ImageViewer = null;
            this._printViewToolStripMenuItem.Name = "_printViewToolStripMenuItem";
            this._printViewToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._printViewToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this._printViewToolStripMenuItem.Text = "Print view...";
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new System.Drawing.Size(193, 6);
            // 
            // _skipProcessingToolStripMenuItem
            // 
            this._skipProcessingToolStripMenuItem.Name = "_skipProcessingToolStripMenuItem";
            this._skipProcessingToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this._skipProcessingToolStripMenuItem.Text = "Skip processing";
            this._skipProcessingToolStripMenuItem.Click += new System.EventHandler(this.HandleSkipProcessingToolStripMenuItemClick);
            // 
            // _stopProcessingToolStripMenuItem
            // 
            this._stopProcessingToolStripMenuItem.Name = "_stopProcessingToolStripMenuItem";
            this._stopProcessingToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this._stopProcessingToolStripMenuItem.Text = "Stop processing";
            this._stopProcessingToolStripMenuItem.Click += new System.EventHandler(this.HandleStopProcessingToolStripMenuItemClick);
            // 
            // _viewToolStripMenuItem
            // 
            this._viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomToolStripMenuItem,
            this._rotateToolStripMenuItem,
            this._gotoPageToolStripMenuItem});
            this._viewToolStripMenuItem.Name = "_viewToolStripMenuItem";
            this._viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this._viewToolStripMenuItem.Text = "&View";
            // 
            // _zoomToolStripMenuItem
            // 
            this._zoomToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fitToPageToolStripMenuItem,
            this._fitToWidthToolStripMenuItem,
            this._oneToOneZoomToolStripMenuItem,
            this._toolStripSeparator3,
            this._zoomInToolStripMenuItem,
            this._zoomOutToolStripMenuItem,
            this._toolStripSeparator4,
            this._zoomPreviousToolStripMenuItem,
            this._zoomNextToolStripMenuItem});
            this._zoomToolStripMenuItem.Name = "_zoomToolStripMenuItem";
            this._zoomToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._zoomToolStripMenuItem.Text = "&Zoom";
            // 
            // _fitToPageToolStripMenuItem
            // 
            this._fitToPageToolStripMenuItem.Enabled = false;
            this._fitToPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_fitToPageToolStripMenuItem.Image")));
            this._fitToPageToolStripMenuItem.ImageViewer = null;
            this._fitToPageToolStripMenuItem.Name = "_fitToPageToolStripMenuItem";
            this._fitToPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._fitToPageToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this._fitToPageToolStripMenuItem.Text = "Fit to &page";
            // 
            // _fitToWidthToolStripMenuItem
            // 
            this._fitToWidthToolStripMenuItem.Enabled = false;
            this._fitToWidthToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_fitToWidthToolStripMenuItem.Image")));
            this._fitToWidthToolStripMenuItem.ImageViewer = null;
            this._fitToWidthToolStripMenuItem.Name = "_fitToWidthToolStripMenuItem";
            this._fitToWidthToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._fitToWidthToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this._fitToWidthToolStripMenuItem.Text = "Fit to &width";
            // 
            // _oneToOneZoomToolStripMenuItem
            // 
            this._oneToOneZoomToolStripMenuItem.Enabled = false;
            this._oneToOneZoomToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_oneToOneZoomToolStripMenuItem.Image")));
            this._oneToOneZoomToolStripMenuItem.ImageViewer = null;
            this._oneToOneZoomToolStripMenuItem.Name = "_oneToOneZoomToolStripMenuItem";
            this._oneToOneZoomToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._oneToOneZoomToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this._oneToOneZoomToolStripMenuItem.Text = "&One-to-one zoom";
            // 
            // _toolStripSeparator3
            // 
            this._toolStripSeparator3.Name = "_toolStripSeparator3";
            this._toolStripSeparator3.Size = new System.Drawing.Size(167, 6);
            // 
            // _zoomInToolStripMenuItem
            // 
            this._zoomInToolStripMenuItem.Enabled = false;
            this._zoomInToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomInToolStripMenuItem.Image")));
            this._zoomInToolStripMenuItem.ImageViewer = null;
            this._zoomInToolStripMenuItem.Name = "_zoomInToolStripMenuItem";
            this._zoomInToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomInToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this._zoomInToolStripMenuItem.Text = "Zoom in";
            // 
            // _zoomOutToolStripMenuItem
            // 
            this._zoomOutToolStripMenuItem.Enabled = false;
            this._zoomOutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomOutToolStripMenuItem.Image")));
            this._zoomOutToolStripMenuItem.ImageViewer = null;
            this._zoomOutToolStripMenuItem.Name = "_zoomOutToolStripMenuItem";
            this._zoomOutToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomOutToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this._zoomOutToolStripMenuItem.Text = "Zoom out";
            // 
            // _toolStripSeparator4
            // 
            this._toolStripSeparator4.Name = "_toolStripSeparator4";
            this._toolStripSeparator4.Size = new System.Drawing.Size(167, 6);
            // 
            // _zoomPreviousToolStripMenuItem
            // 
            this._zoomPreviousToolStripMenuItem.Enabled = false;
            this._zoomPreviousToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomPreviousToolStripMenuItem.Image")));
            this._zoomPreviousToolStripMenuItem.ImageViewer = null;
            this._zoomPreviousToolStripMenuItem.Name = "_zoomPreviousToolStripMenuItem";
            this._zoomPreviousToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomPreviousToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this._zoomPreviousToolStripMenuItem.Text = "Zoom previous";
            // 
            // _zoomNextToolStripMenuItem
            // 
            this._zoomNextToolStripMenuItem.Enabled = false;
            this._zoomNextToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomNextToolStripMenuItem.Image")));
            this._zoomNextToolStripMenuItem.ImageViewer = null;
            this._zoomNextToolStripMenuItem.Name = "_zoomNextToolStripMenuItem";
            this._zoomNextToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomNextToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this._zoomNextToolStripMenuItem.Text = "Zoom next";
            // 
            // _rotateToolStripMenuItem
            // 
            this._rotateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._rotateCounterclockwiseToolStripMenuItem,
            this._rotateClockwiseToolStripMenuItem,
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem,
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem});
            this._rotateToolStripMenuItem.Name = "_rotateToolStripMenuItem";
            this._rotateToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._rotateToolStripMenuItem.Text = "&Rotate";
            // 
            // _rotateCounterclockwiseToolStripMenuItem
            // 
            this._rotateCounterclockwiseToolStripMenuItem.Enabled = false;
            this._rotateCounterclockwiseToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rotateCounterclockwiseToolStripMenuItem.Image")));
            this._rotateCounterclockwiseToolStripMenuItem.ImageViewer = null;
            this._rotateCounterclockwiseToolStripMenuItem.Name = "_rotateCounterclockwiseToolStripMenuItem";
            this._rotateCounterclockwiseToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._rotateCounterclockwiseToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this._rotateCounterclockwiseToolStripMenuItem.Text = "Rotate counterclockwise";
            // 
            // _rotateClockwiseToolStripMenuItem
            // 
            this._rotateClockwiseToolStripMenuItem.Enabled = false;
            this._rotateClockwiseToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rotateClockwiseToolStripMenuItem.Image")));
            this._rotateClockwiseToolStripMenuItem.ImageViewer = null;
            this._rotateClockwiseToolStripMenuItem.Name = "_rotateClockwiseToolStripMenuItem";
            this._rotateClockwiseToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._rotateClockwiseToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this._rotateClockwiseToolStripMenuItem.Text = "Rotate clockwise";
            // 
            // _rotateAllDocumentPagesCounterclockwiseToolStripMenuItem
            // 
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem.Enabled = false;
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rotateAllDocumentPagesCounterclockwiseToolStripMenuItem.Image")));
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem.ImageViewer = null;
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem.Name = "_rotateAllDocumentPagesCounterclockwiseToolStripMenuItem";
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem.Text = "Rotate all pages counterclockwise";
            // 
            // _rotateAllDocumentPagesClockwiseToolStripMenuItem
            // 
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem.Enabled = false;
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rotateAllDocumentPagesClockwiseToolStripMenuItem.Image")));
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem.ImageViewer = null;
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem.Name = "_rotateAllDocumentPagesClockwiseToolStripMenuItem";
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem.Text = "Rotate all pages clockwise";
            // 
            // _gotoPageToolStripMenuItem
            // 
            this._gotoPageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._firstPageToolStripMenuItem,
            this._previousPageToolStripMenuItem,
            this._toolStripSeparator5,
            this._pageNavigationToolStripMenuItem,
            this._toolStripSeparator6,
            this._nextPageToolStripMenuItem,
            this._lastPageToolStripMenuItem});
            this._gotoPageToolStripMenuItem.Name = "_gotoPageToolStripMenuItem";
            this._gotoPageToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._gotoPageToolStripMenuItem.Text = "&Goto page";
            // 
            // _firstPageToolStripMenuItem
            // 
            this._firstPageToolStripMenuItem.Enabled = false;
            this._firstPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_firstPageToolStripMenuItem.Image")));
            this._firstPageToolStripMenuItem.ImageViewer = null;
            this._firstPageToolStripMenuItem.Name = "_firstPageToolStripMenuItem";
            this._firstPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._firstPageToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._firstPageToolStripMenuItem.Text = "First page";
            // 
            // _previousPageToolStripMenuItem
            // 
            this._previousPageToolStripMenuItem.Enabled = false;
            this._previousPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_previousPageToolStripMenuItem.Image")));
            this._previousPageToolStripMenuItem.ImageViewer = null;
            this._previousPageToolStripMenuItem.Name = "_previousPageToolStripMenuItem";
            this._previousPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._previousPageToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._previousPageToolStripMenuItem.Text = "Previous page";
            // 
            // _toolStripSeparator5
            // 
            this._toolStripSeparator5.Name = "_toolStripSeparator5";
            this._toolStripSeparator5.Size = new System.Drawing.Size(151, 6);
            // 
            // _pageNavigationToolStripMenuItem
            // 
            this._pageNavigationToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._pageNavigationToolStripMenuItem.Enabled = false;
            this._pageNavigationToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_pageNavigationToolStripMenuItem.Image")));
            this._pageNavigationToolStripMenuItem.ImageViewer = null;
            this._pageNavigationToolStripMenuItem.Name = "_pageNavigationToolStripMenuItem";
            this._pageNavigationToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._pageNavigationToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._pageNavigationToolStripMenuItem.Text = "Page n&umber...";
            // 
            // _toolStripSeparator6
            // 
            this._toolStripSeparator6.Name = "_toolStripSeparator6";
            this._toolStripSeparator6.Size = new System.Drawing.Size(151, 6);
            // 
            // _nextPageToolStripMenuItem
            // 
            this._nextPageToolStripMenuItem.Enabled = false;
            this._nextPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_nextPageToolStripMenuItem.Image")));
            this._nextPageToolStripMenuItem.ImageViewer = null;
            this._nextPageToolStripMenuItem.Name = "_nextPageToolStripMenuItem";
            this._nextPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._nextPageToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._nextPageToolStripMenuItem.Text = "Next page";
            // 
            // _lastPageToolStripMenuItem
            // 
            this._lastPageToolStripMenuItem.Enabled = false;
            this._lastPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_lastPageToolStripMenuItem.Image")));
            this._lastPageToolStripMenuItem.ImageViewer = null;
            this._lastPageToolStripMenuItem.Name = "_lastPageToolStripMenuItem";
            this._lastPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._lastPageToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._lastPageToolStripMenuItem.Text = "Last page";
            // 
            // _toolsToolStripMenuItem
            // 
            this._toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomWindowToolStripMenuItem,
            this._panToolStripMenuItem});
            this._toolsToolStripMenuItem.Name = "_toolsToolStripMenuItem";
            this._toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this._toolsToolStripMenuItem.Text = "Tools";
            // 
            // _zoomWindowToolStripMenuItem
            // 
            this._zoomWindowToolStripMenuItem.Enabled = false;
            this._zoomWindowToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomWindowToolStripMenuItem.Image")));
            this._zoomWindowToolStripMenuItem.ImageViewer = null;
            this._zoomWindowToolStripMenuItem.Name = "_zoomWindowToolStripMenuItem";
            this._zoomWindowToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomWindowToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._zoomWindowToolStripMenuItem.Text = "&Zoom window";
            // 
            // _panToolStripMenuItem
            // 
            this._panToolStripMenuItem.Enabled = false;
            this._panToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_panToolStripMenuItem.Image")));
            this._panToolStripMenuItem.ImageViewer = null;
            this._panToolStripMenuItem.Name = "_panToolStripMenuItem";
            this._panToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._panToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._panToolStripMenuItem.Text = "P&an";
            // 
            // _basicDataGridToolStrip
            // 
            this._basicDataGridToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._basicDataGridToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._basicDataGridToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._nextDocumentToolStripButton,
            this._tagFileToolStripButton});
            this._basicDataGridToolStrip.Location = new System.Drawing.Point(3, 24);
            this._basicDataGridToolStrip.Name = "_basicDataGridToolStrip";
            this._basicDataGridToolStrip.Size = new System.Drawing.Size(84, 39);
            this._basicDataGridToolStrip.TabIndex = 0;
            // 
            // _nextDocumentToolStripButton
            // 
            this._nextDocumentToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextDocumentToolStripButton.Enabled = false;
            this._nextDocumentToolStripButton.Image = global::Extract.FileActionManager.FileProcessors.Properties.Resources.NextDocument;
            this._nextDocumentToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextDocumentToolStripButton.Name = "_nextDocumentToolStripButton";
            this._nextDocumentToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextDocumentToolStripButton.Text = "Next document (Ctrl+Tab)";
            this._nextDocumentToolStripButton.ToolTipText = "Next document (Ctrl+Tab)";
            this._nextDocumentToolStripButton.Click += new System.EventHandler(this.HandleNextDocumentToolStripButtonClick);
            // 
            // _tagFileToolStripButton
            // 
            this._tagFileToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._tagFileToolStripButton.Enabled = false;
            this._tagFileToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._tagFileToolStripButton.Name = "_tagFileToolStripButton";
            this._tagFileToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._tagFileToolStripButton.Text = "Apply tags";
            // 
            // _basicImageViewerToolStrip
            // 
            this._basicImageViewerToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._basicImageViewerToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._basicImageViewerToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._printImageToolStripButton,
            this._zoomWindowToolStripButton,
            this._panToolStripButton});
            this._basicImageViewerToolStrip.Location = new System.Drawing.Point(87, 24);
            this._basicImageViewerToolStrip.Name = "_basicImageViewerToolStrip";
            this._basicImageViewerToolStrip.Size = new System.Drawing.Size(120, 39);
            this._basicImageViewerToolStrip.TabIndex = 0;
            // 
            // _printImageToolStripButton
            // 
            this._printImageToolStripButton.BaseToolTipText = "Print image";
            this._printImageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._printImageToolStripButton.Enabled = false;
            this._printImageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._printImageToolStripButton.ImageViewer = null;
            this._printImageToolStripButton.Name = "_printImageToolStripButton";
            this._printImageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._printImageToolStripButton.Text = "Print image";
            // 
            // _zoomWindowToolStripButton
            // 
            this._zoomWindowToolStripButton.BaseToolTipText = "Zoom window";
            this._zoomWindowToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomWindowToolStripButton.Enabled = false;
            this._zoomWindowToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomWindowToolStripButton.ImageViewer = null;
            this._zoomWindowToolStripButton.Name = "_zoomWindowToolStripButton";
            this._zoomWindowToolStripButton.Size = new System.Drawing.Size(36, 36);
            // 
            // _panToolStripButton
            // 
            this._panToolStripButton.BaseToolTipText = "Pan";
            this._panToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._panToolStripButton.Enabled = false;
            this._panToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._panToolStripButton.ImageViewer = null;
            this._panToolStripButton.Name = "_panToolStripButton";
            this._panToolStripButton.Size = new System.Drawing.Size(36, 36);
            // 
            // _pageNavigationToolStrip
            // 
            this._pageNavigationToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._pageNavigationToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._pageNavigationToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._firstPageToolStripButton,
            this._previousPageToolStripButton,
            this._pageNavigationToolStripTextBox,
            this._nextPageToolStripButton,
            this._lastPageToolStripButton});
            this._pageNavigationToolStrip.Location = new System.Drawing.Point(207, 24);
            this._pageNavigationToolStrip.Name = "_pageNavigationToolStrip";
            this._pageNavigationToolStrip.Size = new System.Drawing.Size(233, 39);
            this._pageNavigationToolStrip.TabIndex = 1;
            // 
            // _firstPageToolStripButton
            // 
            this._firstPageToolStripButton.BaseToolTipText = "Go to first page";
            this._firstPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._firstPageToolStripButton.Enabled = false;
            this._firstPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._firstPageToolStripButton.ImageViewer = null;
            this._firstPageToolStripButton.Name = "_firstPageToolStripButton";
            this._firstPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._firstPageToolStripButton.Text = "firstPageToolStripButton1";
            // 
            // _previousPageToolStripButton
            // 
            this._previousPageToolStripButton.BaseToolTipText = "Go to previous page";
            this._previousPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._previousPageToolStripButton.Enabled = false;
            this._previousPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._previousPageToolStripButton.ImageViewer = null;
            this._previousPageToolStripButton.Name = "_previousPageToolStripButton";
            this._previousPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._previousPageToolStripButton.Text = "previousPageToolStripButton1";
            // 
            // _pageNavigationToolStripTextBox
            // 
            this._pageNavigationToolStripTextBox.Enabled = false;
            this._pageNavigationToolStripTextBox.ImageViewer = null;
            this._pageNavigationToolStripTextBox.Name = "_pageNavigationToolStripTextBox";
            this._pageNavigationToolStripTextBox.Size = new System.Drawing.Size(75, 39);
            this._pageNavigationToolStripTextBox.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // _nextPageToolStripButton
            // 
            this._nextPageToolStripButton.BaseToolTipText = "Go to next page";
            this._nextPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextPageToolStripButton.Enabled = false;
            this._nextPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextPageToolStripButton.ImageViewer = null;
            this._nextPageToolStripButton.Name = "_nextPageToolStripButton";
            this._nextPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextPageToolStripButton.Text = "nextPageToolStripButton1";
            // 
            // _lastPageToolStripButton
            // 
            this._lastPageToolStripButton.BaseToolTipText = "Go to last page";
            this._lastPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._lastPageToolStripButton.Enabled = false;
            this._lastPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._lastPageToolStripButton.ImageViewer = null;
            this._lastPageToolStripButton.Name = "_lastPageToolStripButton";
            this._lastPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._lastPageToolStripButton.Text = "lastPageToolStripButton1";
            // 
            // _viewCommandsToolStrip
            // 
            this._viewCommandsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._viewCommandsToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._viewCommandsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomInToolStripButton,
            this._zoomOutToolStripButton,
            this._zoomPreviousToolStripButton,
            this._zoomNextToolStripButton,
            toolStripSeparator5,
            this._fitToPageToolStripButton,
            this._fitToWidthToolStripButton,
            this._oneToOneZoomToolStripButton,
            toolStripSeparator6,
            this._rotateCounterclockwiseToolStripButton,
            this._rotateClockwiseToolStripButton,
            toolStripSeparator10,
            this._thumbnailsToolStripButton,
            this._magnifierToolStripButton});
            this._viewCommandsToolStrip.Location = new System.Drawing.Point(440, 24);
            this._viewCommandsToolStrip.Name = "_viewCommandsToolStrip";
            this._viewCommandsToolStrip.Size = new System.Drawing.Size(426, 39);
            this._viewCommandsToolStrip.TabIndex = 2;
            // 
            // _zoomInToolStripButton
            // 
            this._zoomInToolStripButton.BaseToolTipText = "Zoom in";
            this._zoomInToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomInToolStripButton.Enabled = false;
            this._zoomInToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomInToolStripButton.ImageViewer = null;
            this._zoomInToolStripButton.Name = "_zoomInToolStripButton";
            this._zoomInToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomInToolStripButton.Text = "Zoom in";
            // 
            // _zoomOutToolStripButton
            // 
            this._zoomOutToolStripButton.BaseToolTipText = "Zoom out";
            this._zoomOutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomOutToolStripButton.Enabled = false;
            this._zoomOutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomOutToolStripButton.ImageViewer = null;
            this._zoomOutToolStripButton.Name = "_zoomOutToolStripButton";
            this._zoomOutToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomOutToolStripButton.Text = "Zoom out";
            // 
            // _zoomPreviousToolStripButton
            // 
            this._zoomPreviousToolStripButton.BaseToolTipText = "Zoom previous";
            this._zoomPreviousToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomPreviousToolStripButton.Enabled = false;
            this._zoomPreviousToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomPreviousToolStripButton.ImageViewer = null;
            this._zoomPreviousToolStripButton.Name = "_zoomPreviousToolStripButton";
            this._zoomPreviousToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomPreviousToolStripButton.Text = "Zoom previous";
            // 
            // _zoomNextToolStripButton
            // 
            this._zoomNextToolStripButton.BaseToolTipText = "Zoom next";
            this._zoomNextToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomNextToolStripButton.Enabled = false;
            this._zoomNextToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomNextToolStripButton.ImageViewer = null;
            this._zoomNextToolStripButton.Name = "_zoomNextToolStripButton";
            this._zoomNextToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomNextToolStripButton.Text = "Zoom next";
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(6, 39);
            // 
            // _fitToPageToolStripButton
            // 
            this._fitToPageToolStripButton.BaseToolTipText = "Fit to page";
            this._fitToPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._fitToPageToolStripButton.Enabled = false;
            this._fitToPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._fitToPageToolStripButton.ImageViewer = null;
            this._fitToPageToolStripButton.Name = "_fitToPageToolStripButton";
            this._fitToPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._fitToPageToolStripButton.Text = "Fit to page";
            // 
            // _fitToWidthToolStripButton
            // 
            this._fitToWidthToolStripButton.BaseToolTipText = "Fit to width";
            this._fitToWidthToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._fitToWidthToolStripButton.Enabled = false;
            this._fitToWidthToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._fitToWidthToolStripButton.ImageViewer = null;
            this._fitToWidthToolStripButton.Name = "_fitToWidthToolStripButton";
            this._fitToWidthToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._fitToWidthToolStripButton.Text = "Fit to width";
            // 
            // _oneToOneZoomToolStripButton
            // 
            this._oneToOneZoomToolStripButton.BaseToolTipText = "One-to-one zoom";
            this._oneToOneZoomToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._oneToOneZoomToolStripButton.Enabled = false;
            this._oneToOneZoomToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._oneToOneZoomToolStripButton.ImageViewer = null;
            this._oneToOneZoomToolStripButton.Name = "_oneToOneZoomToolStripButton";
            this._oneToOneZoomToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._oneToOneZoomToolStripButton.Text = "One-to-one zoom";
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(6, 39);
            // 
            // _rotateCounterclockwiseToolStripButton
            // 
            this._rotateCounterclockwiseToolStripButton.BaseToolTipText = "Rotate counterclockwise";
            this._rotateCounterclockwiseToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._rotateCounterclockwiseToolStripButton.Enabled = false;
            this._rotateCounterclockwiseToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._rotateCounterclockwiseToolStripButton.ImageViewer = null;
            this._rotateCounterclockwiseToolStripButton.Name = "_rotateCounterclockwiseToolStripButton";
            this._rotateCounterclockwiseToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._rotateCounterclockwiseToolStripButton.Text = "Rotate counterclockwise";
            // 
            // _rotateClockwiseToolStripButton
            // 
            this._rotateClockwiseToolStripButton.BaseToolTipText = "Rotate clockwise";
            this._rotateClockwiseToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._rotateClockwiseToolStripButton.Enabled = false;
            this._rotateClockwiseToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._rotateClockwiseToolStripButton.ImageViewer = null;
            this._rotateClockwiseToolStripButton.Name = "_rotateClockwiseToolStripButton";
            this._rotateClockwiseToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._rotateClockwiseToolStripButton.Text = "Rotate clockwise";
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new System.Drawing.Size(6, 39);
            // 
            // _thumbnailsToolStripButton
            // 
            this._thumbnailsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._thumbnailsToolStripButton.DockableWindow = null;
            this._thumbnailsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._thumbnailsToolStripButton.Name = "_thumbnailsToolStripButton";
            this._thumbnailsToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._thumbnailsToolStripButton.Text = "Show/Hide thumbnails (F10)";
            // 
            // _magnifierToolStripButton
            // 
            this._magnifierToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._magnifierToolStripButton.DockableWindow = null;
            this._magnifierToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._magnifierToolStripButton.Name = "_magnifierToolStripButton";
            this._magnifierToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._magnifierToolStripButton.Text = "Show/Hide magnifier (F12)";
            // 
            // dockContainer1
            // 
            dockContainer1.ContentSize = 200;
            dockContainer1.Controls.Add(this._thumbnailDockableWindow);
            dockContainer1.Dock = System.Windows.Forms.DockStyle.Right;
            dockContainer1.LayoutSystem = new TD.SandDock.SplitLayoutSystem(new System.Drawing.SizeF(250F, 400F), System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 400F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._thumbnailDockableWindow))}, this._thumbnailDockableWindow)))});
            dockContainer1.Location = new System.Drawing.Point(1388, 0);
            dockContainer1.Manager = this._sandDockManager;
            dockContainer1.Name = "dockContainer1";
            dockContainer1.Size = new System.Drawing.Size(204, 701);
            dockContainer1.TabIndex = 2;
            // 
            // _thumbnailDockableWindow
            // 
            this._thumbnailDockableWindow.Controls.Add(this._thumbnailViewer);
            this._thumbnailDockableWindow.Guid = new System.Guid("ae627741-717d-48f0-8e85-071b39098d21");
            this._thumbnailDockableWindow.Location = new System.Drawing.Point(4, 25);
            this._thumbnailDockableWindow.Name = "_thumbnailDockableWindow";
            this._thumbnailDockableWindow.PrimaryControl = this;
            this._thumbnailDockableWindow.Size = new System.Drawing.Size(200, 653);
            this._thumbnailDockableWindow.TabIndex = 0;
            this._thumbnailDockableWindow.Text = "Page thumbnails";
            this._thumbnailDockableWindow.DockSituationChanged += new System.EventHandler(this.HandleThumbnailDockableWindowDockSituationChanged);
            // 
            // _thumbnailViewer
            // 
            this._thumbnailViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._thumbnailViewer.ImageViewer = null;
            this._thumbnailViewer.Location = new System.Drawing.Point(0, 0);
            this._thumbnailViewer.Name = "_thumbnailViewer";
            this._thumbnailViewer.Size = new System.Drawing.Size(200, 653);
            this._thumbnailViewer.TabIndex = 0;
            // 
            // _sandDockManager
            // 
            this._sandDockManager.AllowKeyboardNavigation = false;
            this._sandDockManager.DockSystemContainer = this;
            this._sandDockManager.MaximumDockContainerSize = 2000;
            this._sandDockManager.MinimumDockContainerSize = 220;
            this._sandDockManager.OwnerForm = this;
            this._sandDockManager.Renderer = new TD.SandDock.Rendering.Office2003Renderer();
            // 
            // _magnifierDockableWindow
            // 
            this._magnifierDockableWindow.Controls.Add(this._magnifierControl);
            this._magnifierDockableWindow.Guid = new System.Guid("3dc14a68-1717-4ee9-b9c3-a28e90c98124");
            this._magnifierDockableWindow.Location = new System.Drawing.Point(0, 29);
            this._magnifierDockableWindow.Name = "_magnifierDockableWindow";
            this._magnifierDockableWindow.PrimaryControl = this;
            this._magnifierDockableWindow.Size = new System.Drawing.Size(1592, 173);
            this._magnifierDockableWindow.TabIndex = 0;
            this._magnifierDockableWindow.Text = "Magnifier";
            // 
            // _magnifierControl
            // 
            this._magnifierControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._magnifierControl.ImageViewer = null;
            this._magnifierControl.Location = new System.Drawing.Point(0, 0);
            this._magnifierControl.Name = "_magnifierControl";
            this._magnifierControl.Size = new System.Drawing.Size(1592, 173);
            this._magnifierControl.TabIndex = 0;
            // 
            // dockContainer2
            // 
            this.dockContainer2.ContentSize = 221;
            this.dockContainer2.Controls.Add(this._magnifierDockableWindow);
            this.dockContainer2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dockContainer2.LayoutSystem = new TD.SandDock.SplitLayoutSystem(new System.Drawing.SizeF(250F, 400F), System.Windows.Forms.Orientation.Vertical, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 214.0845F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._magnifierDockableWindow))}, this._magnifierDockableWindow)))});
            this.dockContainer2.Location = new System.Drawing.Point(0, 701);
            this.dockContainer2.Manager = this._sandDockManager;
            this.dockContainer2.Name = "dockContainer2";
            this.dockContainer2.Size = new System.Drawing.Size(1592, 225);
            this.dockContainer2.TabIndex = 3;
            // 
            // ViewImageTaskForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1592, 926);
            this.Controls.Add(imageViewerToolStripContainer);
            this.Controls.Add(dockContainer1);
            this.Controls.Add(this.dockContainer2);
            this.Icon = global::Extract.FileActionManager.FileProcessors.Properties.Resources.ExtractImageViewerIcon;
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "ViewImageTaskForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "View Image";
            imageViewerToolStripContainer.BottomToolStripPanel.ResumeLayout(false);
            imageViewerToolStripContainer.BottomToolStripPanel.PerformLayout();
            imageViewerToolStripContainer.ContentPanel.ResumeLayout(false);
            imageViewerToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            imageViewerToolStripContainer.TopToolStripPanel.PerformLayout();
            imageViewerToolStripContainer.ResumeLayout(false);
            imageViewerToolStripContainer.PerformLayout();
            this._imageViewerContextMenu.ResumeLayout(false);
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this._basicDataGridToolStrip.ResumeLayout(false);
            this._basicDataGridToolStrip.PerformLayout();
            this._basicImageViewerToolStrip.ResumeLayout(false);
            this._basicImageViewerToolStrip.PerformLayout();
            this._pageNavigationToolStrip.ResumeLayout(false);
            this._pageNavigationToolStrip.PerformLayout();
            this._viewCommandsToolStrip.ResumeLayout(false);
            this._viewCommandsToolStrip.PerformLayout();
            dockContainer1.ResumeLayout(false);
            this._thumbnailDockableWindow.ResumeLayout(false);
            this._magnifierDockableWindow.ResumeLayout(false);
            this.dockContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }
     
        #endregion

        private TD.SandDock.SandDockManager _sandDockManager;
        private Extract.Imaging.Forms.ImageViewer _imageViewer;
        private System.Windows.Forms.ToolStrip _basicImageViewerToolStrip;
        private Extract.Imaging.Forms.PrintImageToolStripButton _printImageToolStripButton;
        private Extract.Imaging.Forms.ZoomWindowToolStripButton _zoomWindowToolStripButton;
        private Extract.Imaging.Forms.PanToolStripButton _panToolStripButton;
        private System.Windows.Forms.ToolStrip _pageNavigationToolStrip;
        private Extract.Imaging.Forms.FirstPageToolStripButton _firstPageToolStripButton;
        private Extract.Imaging.Forms.PreviousPageToolStripButton _previousPageToolStripButton;
        private Extract.Imaging.Forms.PageNavigationToolStripTextBox _pageNavigationToolStripTextBox;
        private Extract.Imaging.Forms.NextPageToolStripButton _nextPageToolStripButton;
        private Extract.Imaging.Forms.LastPageToolStripButton _lastPageToolStripButton;
        private System.Windows.Forms.ToolStrip _viewCommandsToolStrip;
        private Extract.Imaging.Forms.ZoomInToolStripButton _zoomInToolStripButton;
        private Extract.Imaging.Forms.ZoomOutToolStripButton _zoomOutToolStripButton;
        private Extract.Imaging.Forms.ZoomPreviousToolStripButton _zoomPreviousToolStripButton;
        private Extract.Imaging.Forms.ZoomNextToolStripButton _zoomNextToolStripButton;
        private Extract.Imaging.Forms.FitToPageToolStripButton _fitToPageToolStripButton;
        private Extract.Imaging.Forms.FitToWidthToolStripButton _fitToWidthToolStripButton;
        private Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton _rotateCounterclockwiseToolStripButton;
        private Extract.Imaging.Forms.RotateClockwiseToolStripButton _rotateClockwiseToolStripButton;
        private System.Windows.Forms.ToolStrip _basicDataGridToolStrip;
        private System.Windows.Forms.ToolStripButton _nextDocumentToolStripButton;
        private TD.SandDock.DockableWindow _thumbnailDockableWindow;
        private Extract.Imaging.Forms.ThumbnailViewer _thumbnailViewer;
        private Extract.Imaging.Forms.ImageViewerStatusStrip _imageViewerStatusStrip;
        private Extract.Imaging.Forms.ThumbnailViewerToolStripButton _thumbnailsToolStripButton;
        private System.Windows.Forms.ContextMenuStrip _imageViewerContextMenu;
        private Extract.Imaging.Forms.SelectLayerObjectToolStripMenuItem _selectLayerObjectToolStripMenuItem1;
        private Extract.Imaging.Forms.ZoomWindowToolStripMenuItem _zoomWindowToolStripMenuItem1;
        private Extract.Imaging.Forms.PanToolStripMenuItem _panToolStripMenuItem1;
        private Extract.Imaging.Forms.AngularRedactionToolStripMenuItem _angularRedactionToolStripMenuItem1;
        private Extract.Imaging.Forms.RectangularRedactionToolStripMenuItem _rectangularRedactionToolStripMenuItem1;
        private Extract.Imaging.Forms.WordRedactionToolStripMenuItem _wordRedactionToolStripMenuItem1;
        private Extract.Imaging.Forms.BlockFitSelectionToolStripMenuItem _blockFitSelectionToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private DockableWindow _magnifierDockableWindow;
        private Imaging.Forms.MagnifierControl _magnifierControl;
        private Imaging.Forms.MagnifierWindowToolStripButton _magnifierToolStripButton;
        private Extract.Imaging.Forms.EnlargeSelectionToolStripMenuItem _enlargeRedactionToolStripMenuItem;
        private Extract.Imaging.Forms.ShrinkSelectionToolStripMenuItem _shrinkRedactionToolStripMenuItem;
        private Imaging.Forms.ZoomPreviousToolStripMenuItem _zoomPreviousToolStripMenuItem1;
        private Imaging.Forms.OneToOneZoomToolStripButton _oneToOneZoomToolStripButton;
        private DockContainer dockContainer2;
        private Forms.TagFileToolStripButton _tagFileToolStripButton;
        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.ToolStripMenuItem _fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _nextDocumentToolStripMenuItem;
        private Imaging.Forms.PrintImageToolStripMenuItem _printImageToolStripMenuItem;
        private Imaging.Forms.PrintViewToolStripMenuItem _printViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _skipProcessingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _stopProcessingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _zoomToolStripMenuItem;
        private Imaging.Forms.FitToPageToolStripMenuItem _fitToPageToolStripMenuItem;
        private Imaging.Forms.FitToWidthToolStripMenuItem _fitToWidthToolStripMenuItem;
        private Imaging.Forms.OneToOneZoomToolStripMenuItem _oneToOneZoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator3;
        private Imaging.Forms.ZoomInToolStripMenuItem _zoomInToolStripMenuItem;
        private Imaging.Forms.ZoomOutToolStripMenuItem _zoomOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator4;
        private Imaging.Forms.ZoomPreviousToolStripMenuItem _zoomPreviousToolStripMenuItem;
        private Imaging.Forms.ZoomNextToolStripMenuItem _zoomNextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _rotateToolStripMenuItem;
        private Imaging.Forms.RotateCounterclockwiseToolStripMenuItem _rotateCounterclockwiseToolStripMenuItem;
        private Imaging.Forms.RotateClockwiseToolStripMenuItem _rotateClockwiseToolStripMenuItem;
        private Imaging.Forms.RotateAllDocumentPagesCounterclockwiseToolStripMenuItem _rotateAllDocumentPagesCounterclockwiseToolStripMenuItem;
        private Imaging.Forms.RotateAllDocumentPagesClockwiseToolStripMenuItem _rotateAllDocumentPagesClockwiseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _gotoPageToolStripMenuItem;
        private Imaging.Forms.FirstPageToolStripMenuItem _firstPageToolStripMenuItem;
        private Imaging.Forms.PreviousPageToolStripMenuItem _previousPageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator5;
        private Imaging.Forms.PageNavigationToolStripMenuItem _pageNavigationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator6;
        private Imaging.Forms.NextPageToolStripMenuItem _nextPageToolStripMenuItem;
        private Imaging.Forms.LastPageToolStripMenuItem _lastPageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _toolsToolStripMenuItem;
        private Imaging.Forms.ZoomWindowToolStripMenuItem _zoomWindowToolStripMenuItem;
        private Imaging.Forms.PanToolStripMenuItem _panToolStripMenuItem;
    }
}
