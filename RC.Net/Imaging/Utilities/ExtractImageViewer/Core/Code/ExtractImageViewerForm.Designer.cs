using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System;

namespace Extract.Imaging.Utilities.ExtractImageViewer
{
    partial class ExtractImageViewerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (_ocrManager != null)
            {
                _ocrManager.Dispose();
                _ocrManager = null;
            }
            if (_imageSearchForm != null)
            {
                _imageSearchForm.Dispose();
                _imageSearchForm = null;
            }
            if (_remoteHandler != null)
            {
                _remoteHandler.Dispose();
                _remoteHandler = null;
            }
            if (_ipcChannel != null)
            {
                ChannelServices.UnregisterChannel(_ipcChannel);
                _ipcChannel = null;
            }
            if (_formStateManager != null)
            {
                _formStateManager.Dispose();
                _formStateManager = null;
            }

            // Collapsed or hidden dockable windows must be disposed explicitly [FIDSC #4246]
            // TODO: Can be removed when Divelements corrects this in the next release (3.0.7+)
            if (_thumbnailDockableWindow != null)
            {
                _thumbnailDockableWindow.Dispose();
                _thumbnailDockableWindow = null;
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtractImageViewerForm));
            this._toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this._contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._zoomWindowContextMenuItem = new Extract.Imaging.Forms.ZoomWindowToolStripMenuItem();
            this._panContextMenuItem = new Extract.Imaging.Forms.PanToolStripMenuItem();
            this._zoomPreviousContextMenuItem = new Extract.Imaging.Forms.ZoomPreviousToolStripMenuItem();
            this._zoomNextContextMenuItem = new Extract.Imaging.Forms.ZoomNextToolStripMenuItem();
            this._angularHighlightContextMenuItem = new Extract.Imaging.Forms.AngularHighlightToolStripMenuItem();
            this._rectangularHighlightContextMenuItem = new Extract.Imaging.Forms.RectangularHighlightToolStripMenuItem();
            this._imageViewer = new Extract.Imaging.Forms.ImageViewer();
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openImageToolStripMenuItem = new Extract.Imaging.Forms.OpenImageToolStripMenuItem();
            this._searchForImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._printImageToolStripMenuItem = new Extract.Imaging.Forms.PrintImageToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this._openImageMruToolStripMenuItem = new Extract.Imaging.Forms.OpenImageMruToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this._exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._zoomWindowToolStripMenuItem = new Extract.Imaging.Forms.ZoomWindowToolStripMenuItem();
            this._panToolStripMenuItem = new Extract.Imaging.Forms.PanToolStripMenuItem();
            this._angularHighlightToolStripMenuItem = new Extract.Imaging.Forms.AngularHighlightToolStripMenuItem();
            this._rectangularHighlightToolStripMenuItem = new Extract.Imaging.Forms.RectangularHighlightToolStripMenuItem();
            this._deleteLayerObjectsToolStripMenuItem = new Extract.Imaging.Forms.DeleteLayerObjectsToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._zoomInToolStripMenuItem = new Extract.Imaging.Forms.ZoomInToolStripMenuItem();
            this._zoomOutToolStripMenuItem = new Extract.Imaging.Forms.ZoomOutToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomPreviousToolStripMenuItem = new Extract.Imaging.Forms.ZoomPreviousToolStripMenuItem();
            this._zoomNextToolStripMenuItem = new Extract.Imaging.Forms.ZoomNextToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.previousTileToolStripMenuItem1 = new Extract.Imaging.Forms.PreviousTileToolStripMenuItem();
            this.nextTileToolStripMenuItem1 = new Extract.Imaging.Forms.NextTileToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this._fitToWidthToolStripMenuItem = new Extract.Imaging.Forms.FitToWidthToolStripMenuItem();
            this._fitToPageToolStripMenuItem = new Extract.Imaging.Forms.FitToPageToolStripMenuItem();
            this._oneToOneZoomToolStripMenuItem = new Extract.Imaging.Forms.OneToOneZoomToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this._rotateCounterclockwiseToolStripMenuItem = new Extract.Imaging.Forms.RotateCounterclockwiseToolStripMenuItem();
            this._rotateClockwiseToolStripMenuItem = new Extract.Imaging.Forms.RotateClockwiseToolStripMenuItem();
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem = new Extract.Imaging.Forms.RotateAllDocumentPagesCounterclockwiseToolStripMenuItem();
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem = new Extract.Imaging.Forms.RotateAllDocumentPagesClockwiseToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this._firstPageToolStripMenuItem = new Extract.Imaging.Forms.FirstPageToolStripMenuItem();
            this._previousPageToolStripMenuItem = new Extract.Imaging.Forms.PreviousPageToolStripMenuItem();
            this._nextPageToolStripMenuItem = new Extract.Imaging.Forms.NextPageToolStripMenuItem();
            this._lastPageToolStripMenuItem = new Extract.Imaging.Forms.LastPageToolStripMenuItem();
            this._fileCommands = new System.Windows.Forms.ToolStrip();
            this._openImageToolStripSplitButton = new Extract.Imaging.Forms.OpenImageToolStripSplitButton();
            this._printImageToolStripButton = new Extract.Imaging.Forms.PrintImageToolStripButton();
            this._basicTools = new System.Windows.Forms.ToolStrip();
            this._zoomWindowToolStripButton = new Extract.Imaging.Forms.ZoomWindowToolStripButton();
            this._panToolStripButton = new Extract.Imaging.Forms.PanToolStripButton();
            this._highlightToolStripSplitButton = new Extract.Imaging.Forms.HighlightToolStripSplitButton();
            this._deleteLayerObjectsToolStripButton = new Extract.Imaging.Forms.DeleteLayerObjectsToolStripButton();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this._extractImageToolStripButton = new Extract.Imaging.Forms.ExtractImageToolStripButton();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this._thumbnailsToolStripButton = new Extract.Imaging.Forms.ThumbnailViewerToolStripButton();
            this._viewCommands = new System.Windows.Forms.ToolStrip();
            this._zoomInToolStripButton = new Extract.Imaging.Forms.ZoomInToolStripButton();
            this._zoomOutToolStripButton = new Extract.Imaging.Forms.ZoomOutToolStripButton();
            this._zoomPreviousToolStripButton = new Extract.Imaging.Forms.ZoomPreviousToolStripButton();
            this._zoomNextToolStripButton = new Extract.Imaging.Forms.ZoomNextToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this._fitToPageToolStripButton = new Extract.Imaging.Forms.FitToPageToolStripButton();
            this._fitToWidthToolStripButton = new Extract.Imaging.Forms.FitToWidthToolStripButton();
            this._oneToOneZoomToolStripButton = new Extract.Imaging.Forms.OneToOneZoomToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this._previousTileToolStripButton = new Extract.Imaging.Forms.PreviousTileToolStripButton();
            this._nextTileToolStripButton = new Extract.Imaging.Forms.NextTileToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this._rotateCounterclockwiseToolStripButton = new Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton();
            this._rotateClockwiseToolStripButton = new Extract.Imaging.Forms.RotateClockwiseToolStripButton();
            this._navigationTools = new System.Windows.Forms.ToolStrip();
            this._firstPageToolStripButton = new Extract.Imaging.Forms.FirstPageToolStripButton();
            this._previousPageToolStripButton = new Extract.Imaging.Forms.PreviousPageToolStripButton();
            this._pageNavigationToolStripTextBox = new Extract.Imaging.Forms.PageNavigationToolStripTextBox();
            this._nextPageToolStripButton = new Extract.Imaging.Forms.NextPageToolStripButton();
            this._lastPageToolStripButton = new Extract.Imaging.Forms.LastPageToolStripButton();
            this._imageViewerStatusStrip = new Extract.Imaging.Forms.ImageViewerStatusStrip();
            this._sandDockManager = new TD.SandDock.SandDockManager();
            this._thumbnailDockableWindow = new TD.SandDock.DockableWindow();
            this._thumbnailViewer = new Extract.Imaging.Forms.ThumbnailViewer();
            this.dockContainer1 = new TD.SandDock.DockContainer();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this._invertColorsToolStripButton = new Extract.Imaging.Forms.InvertColorsToolStripButton();
            toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this._toolStripContainer.ContentPanel.SuspendLayout();
            this._toolStripContainer.TopToolStripPanel.SuspendLayout();
            this._toolStripContainer.SuspendLayout();
            this._contextMenu.SuspendLayout();
            this._menuStrip.SuspendLayout();
            this._fileCommands.SuspendLayout();
            this._basicTools.SuspendLayout();
            this._viewCommands.SuspendLayout();
            this._navigationTools.SuspendLayout();
            this._thumbnailDockableWindow.SuspendLayout();
            this.dockContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripSeparator14
            // 
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new System.Drawing.Size(185, 6);
            // 
            // toolStripSeparator15
            // 
            toolStripSeparator15.Name = "toolStripSeparator15";
            toolStripSeparator15.Size = new System.Drawing.Size(185, 6);
            // 
            // _toolStripContainer
            // 
            // 
            // _toolStripContainer.ContentPanel
            // 
            this._toolStripContainer.ContentPanel.ContextMenuStrip = this._contextMenu;
            this._toolStripContainer.ContentPanel.Controls.Add(this._imageViewer);
            this._toolStripContainer.ContentPanel.Size = new System.Drawing.Size(748, 440);
            this._toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._toolStripContainer.LeftToolStripPanelVisible = false;
            this._toolStripContainer.Location = new System.Drawing.Point(0, 0);
            this._toolStripContainer.Name = "_toolStripContainer";
            this._toolStripContainer.RightToolStripPanelVisible = false;
            this._toolStripContainer.Size = new System.Drawing.Size(748, 542);
            this._toolStripContainer.TabIndex = 0;
            this._toolStripContainer.TabStop = false;
            this._toolStripContainer.Text = "toolStripContainer1";
            // 
            // _toolStripContainer.TopToolStripPanel
            // 
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._menuStrip);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._fileCommands);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._navigationTools);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._basicTools);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._viewCommands);
            // 
            // _contextMenu
            // 
            this._contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomWindowContextMenuItem,
            this._panContextMenuItem,
            toolStripSeparator14,
            this._zoomPreviousContextMenuItem,
            this._zoomNextContextMenuItem,
            toolStripSeparator15,
            this._angularHighlightContextMenuItem,
            this._rectangularHighlightContextMenuItem});
            this._contextMenu.Name = "_contextMenu";
            this._contextMenu.Size = new System.Drawing.Size(189, 148);
            // 
            // _zoomWindowContextMenuItem
            // 
            this._zoomWindowContextMenuItem.Enabled = false;
            this._zoomWindowContextMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomWindowContextMenuItem.Image")));
            this._zoomWindowContextMenuItem.ImageViewer = null;
            this._zoomWindowContextMenuItem.Name = "_zoomWindowContextMenuItem";
            this._zoomWindowContextMenuItem.ShortcutKeyDisplayString = "";
            this._zoomWindowContextMenuItem.Size = new System.Drawing.Size(188, 22);
            this._zoomWindowContextMenuItem.Text = "&Zoom window";
            // 
            // _panContextMenuItem
            // 
            this._panContextMenuItem.Enabled = false;
            this._panContextMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_panContextMenuItem.Image")));
            this._panContextMenuItem.ImageViewer = null;
            this._panContextMenuItem.Name = "_panContextMenuItem";
            this._panContextMenuItem.ShortcutKeyDisplayString = "";
            this._panContextMenuItem.Size = new System.Drawing.Size(188, 22);
            this._panContextMenuItem.Text = "P&an";
            // 
            // _zoomPreviousContextMenuItem
            // 
            this._zoomPreviousContextMenuItem.Enabled = false;
            this._zoomPreviousContextMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomPreviousContextMenuItem.Image")));
            this._zoomPreviousContextMenuItem.ImageViewer = null;
            this._zoomPreviousContextMenuItem.Name = "_zoomPreviousContextMenuItem";
            this._zoomPreviousContextMenuItem.ShortcutKeyDisplayString = "";
            this._zoomPreviousContextMenuItem.Size = new System.Drawing.Size(188, 22);
            this._zoomPreviousContextMenuItem.Text = "Zoom previous";
            // 
            // _zoomNextContextMenuItem
            // 
            this._zoomNextContextMenuItem.Enabled = false;
            this._zoomNextContextMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomNextContextMenuItem.Image")));
            this._zoomNextContextMenuItem.ImageViewer = null;
            this._zoomNextContextMenuItem.Name = "_zoomNextContextMenuItem";
            this._zoomNextContextMenuItem.ShortcutKeyDisplayString = "";
            this._zoomNextContextMenuItem.Size = new System.Drawing.Size(188, 22);
            this._zoomNextContextMenuItem.Text = "Zoom next";
            // 
            // _angularHighlightContextMenuItem
            // 
            this._angularHighlightContextMenuItem.Enabled = false;
            this._angularHighlightContextMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_angularHighlightContextMenuItem.Image")));
            this._angularHighlightContextMenuItem.ImageViewer = null;
            this._angularHighlightContextMenuItem.Name = "_angularHighlightContextMenuItem";
            this._angularHighlightContextMenuItem.ShortcutKeyDisplayString = "";
            this._angularHighlightContextMenuItem.Size = new System.Drawing.Size(188, 22);
            this._angularHighlightContextMenuItem.Text = "A&ngular highlight";
            // 
            // _rectangularHighlightContextMenuItem
            // 
            this._rectangularHighlightContextMenuItem.Enabled = false;
            this._rectangularHighlightContextMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rectangularHighlightContextMenuItem.Image")));
            this._rectangularHighlightContextMenuItem.ImageViewer = null;
            this._rectangularHighlightContextMenuItem.Name = "_rectangularHighlightContextMenuItem";
            this._rectangularHighlightContextMenuItem.ShortcutKeyDisplayString = "";
            this._rectangularHighlightContextMenuItem.Size = new System.Drawing.Size(188, 22);
            this._rectangularHighlightContextMenuItem.Text = "&Rectangular highlight";
            // 
            // _imageViewer
            // 
            this._imageViewer.AllowDrop = true;
            this._imageViewer.AutoOcr = false;
            this._imageViewer.AutoZoomScale = 0;
            this._imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._imageViewer.FrameColor = System.Drawing.Color.Transparent;
            this._imageViewer.InvertColors = false;
            this._imageViewer.Location = new System.Drawing.Point(0, 0);
            this._imageViewer.MinimumAngularHighlightHeight = 16;
            this._imageViewer.MinimumSize = new System.Drawing.Size(5, 5);
            this._imageViewer.Name = "_imageViewer";
            this._imageViewer.OcrTradeoff = Extract.Imaging.OcrTradeoff.Accurate;
            this._imageViewer.RedactionMode = false;
            this._imageViewer.Size = new System.Drawing.Size(748, 440);
            this._imageViewer.TabIndex = 0;
            this._imageViewer.Text = "imageViewer1";
            this._imageViewer.UseDefaultShortcuts = true;
            this._imageViewer.ImageFileChanged += new System.EventHandler<Extract.Imaging.Forms.ImageFileChangedEventArgs>(this.HandleImageViewerImageFileChanged);
            // 
            // _menuStrip
            // 
            this._menuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.viewToolStripMenuItem});
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.Size = new System.Drawing.Size(748, 24);
            this._menuStrip.TabIndex = 0;
            this._menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._openImageToolStripMenuItem,
            this._searchForImagesToolStripMenuItem,
            this.toolStripSeparator1,
            this._printImageToolStripMenuItem,
            this.toolStripSeparator6,
            this._openImageMruToolStripMenuItem,
            this.toolStripSeparator12,
            this._exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // _openImageToolStripMenuItem
            // 
            this._openImageToolStripMenuItem.Enabled = false;
            this._openImageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_openImageToolStripMenuItem.Image")));
            this._openImageToolStripMenuItem.ImageViewer = null;
            this._openImageToolStripMenuItem.Name = "_openImageToolStripMenuItem";
            this._openImageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._openImageToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this._openImageToolStripMenuItem.Text = "&Open image";
            // 
            // _searchForImagesToolStripMenuItem
            // 
            this._searchForImagesToolStripMenuItem.Name = "_searchForImagesToolStripMenuItem";
            this._searchForImagesToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this._searchForImagesToolStripMenuItem.Text = "Search for images...";
            this._searchForImagesToolStripMenuItem.Click += new System.EventHandler(this.HandleSearchForImageFilesClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(174, 6);
            // 
            // _printImageToolStripMenuItem
            // 
            this._printImageToolStripMenuItem.Enabled = false;
            this._printImageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_printImageToolStripMenuItem.Image")));
            this._printImageToolStripMenuItem.ImageViewer = null;
            this._printImageToolStripMenuItem.Name = "_printImageToolStripMenuItem";
            this._printImageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._printImageToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this._printImageToolStripMenuItem.Text = "&Print";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(174, 6);
            // 
            // _openImageMruToolStripMenuItem
            // 
            this._openImageMruToolStripMenuItem.Enabled = false;
            this._openImageMruToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_openImageMruToolStripMenuItem.Image")));
            this._openImageMruToolStripMenuItem.ImageViewer = null;
            this._openImageMruToolStripMenuItem.Name = "_openImageMruToolStripMenuItem";
            this._openImageMruToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._openImageMruToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this._openImageMruToolStripMenuItem.Text = "Recent images";
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(174, 6);
            // 
            // _exitToolStripMenuItem
            // 
            this._exitToolStripMenuItem.Name = "_exitToolStripMenuItem";
            this._exitToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this._exitToolStripMenuItem.Text = "E&xit";
            this._exitToolStripMenuItem.Click += new System.EventHandler(this.HandleFileMenuExitClick);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomWindowToolStripMenuItem,
            this._panToolStripMenuItem,
            this._angularHighlightToolStripMenuItem,
            this._rectangularHighlightToolStripMenuItem,
            this._deleteLayerObjectsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // _zoomWindowToolStripMenuItem
            // 
            this._zoomWindowToolStripMenuItem.Enabled = false;
            this._zoomWindowToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomWindowToolStripMenuItem.Image")));
            this._zoomWindowToolStripMenuItem.ImageViewer = null;
            this._zoomWindowToolStripMenuItem.Name = "_zoomWindowToolStripMenuItem";
            this._zoomWindowToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomWindowToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this._zoomWindowToolStripMenuItem.Text = "&Zoom window";
            // 
            // _panToolStripMenuItem
            // 
            this._panToolStripMenuItem.Enabled = false;
            this._panToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_panToolStripMenuItem.Image")));
            this._panToolStripMenuItem.ImageViewer = null;
            this._panToolStripMenuItem.Name = "_panToolStripMenuItem";
            this._panToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._panToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this._panToolStripMenuItem.Text = "P&an";
            // 
            // _angularHighlightToolStripMenuItem
            // 
            this._angularHighlightToolStripMenuItem.Enabled = false;
            this._angularHighlightToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_angularHighlightToolStripMenuItem.Image")));
            this._angularHighlightToolStripMenuItem.ImageViewer = null;
            this._angularHighlightToolStripMenuItem.Name = "_angularHighlightToolStripMenuItem";
            this._angularHighlightToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._angularHighlightToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this._angularHighlightToolStripMenuItem.Text = "A&ngular highlight";
            // 
            // _rectangularHighlightToolStripMenuItem
            // 
            this._rectangularHighlightToolStripMenuItem.Enabled = false;
            this._rectangularHighlightToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rectangularHighlightToolStripMenuItem.Image")));
            this._rectangularHighlightToolStripMenuItem.ImageViewer = null;
            this._rectangularHighlightToolStripMenuItem.Name = "_rectangularHighlightToolStripMenuItem";
            this._rectangularHighlightToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._rectangularHighlightToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this._rectangularHighlightToolStripMenuItem.Text = "&Rectangular highlight";
            // 
            // _deleteLayerObjectsToolStripMenuItem
            // 
            this._deleteLayerObjectsToolStripMenuItem.Enabled = false;
            this._deleteLayerObjectsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_deleteLayerObjectsToolStripMenuItem.Image")));
            this._deleteLayerObjectsToolStripMenuItem.ImageViewer = null;
            this._deleteLayerObjectsToolStripMenuItem.Name = "_deleteLayerObjectsToolStripMenuItem";
            this._deleteLayerObjectsToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._deleteLayerObjectsToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this._deleteLayerObjectsToolStripMenuItem.Text = "&Delete highlights";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomInToolStripMenuItem,
            this._zoomOutToolStripMenuItem,
            this.toolStripSeparator2,
            this._zoomPreviousToolStripMenuItem,
            this._zoomNextToolStripMenuItem,
            this.toolStripSeparator3,
            this.previousTileToolStripMenuItem1,
            this.nextTileToolStripMenuItem1,
            this.toolStripSeparator7,
            this._fitToWidthToolStripMenuItem,
            this._fitToPageToolStripMenuItem,
            this._oneToOneZoomToolStripMenuItem,
            this.toolStripSeparator4,
            this._rotateCounterclockwiseToolStripMenuItem,
            this._rotateClockwiseToolStripMenuItem,
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem,
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem,
            this.toolStripSeparator5,
            this._firstPageToolStripMenuItem,
            this._previousPageToolStripMenuItem,
            this._nextPageToolStripMenuItem,
            this._lastPageToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // _zoomInToolStripMenuItem
            // 
            this._zoomInToolStripMenuItem.Enabled = false;
            this._zoomInToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomInToolStripMenuItem.Image")));
            this._zoomInToolStripMenuItem.ImageViewer = null;
            this._zoomInToolStripMenuItem.Name = "_zoomInToolStripMenuItem";
            this._zoomInToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomInToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this._zoomInToolStripMenuItem.Text = "Zoom in";
            // 
            // _zoomOutToolStripMenuItem
            // 
            this._zoomOutToolStripMenuItem.Enabled = false;
            this._zoomOutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomOutToolStripMenuItem.Image")));
            this._zoomOutToolStripMenuItem.ImageViewer = null;
            this._zoomOutToolStripMenuItem.Name = "_zoomOutToolStripMenuItem";
            this._zoomOutToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomOutToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this._zoomOutToolStripMenuItem.Text = "Zoom out";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(200, 6);
            // 
            // _zoomPreviousToolStripMenuItem
            // 
            this._zoomPreviousToolStripMenuItem.Enabled = false;
            this._zoomPreviousToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomPreviousToolStripMenuItem.Image")));
            this._zoomPreviousToolStripMenuItem.ImageViewer = null;
            this._zoomPreviousToolStripMenuItem.Name = "_zoomPreviousToolStripMenuItem";
            this._zoomPreviousToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomPreviousToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this._zoomPreviousToolStripMenuItem.Text = "Zoom previous";
            // 
            // _zoomNextToolStripMenuItem
            // 
            this._zoomNextToolStripMenuItem.Enabled = false;
            this._zoomNextToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomNextToolStripMenuItem.Image")));
            this._zoomNextToolStripMenuItem.ImageViewer = null;
            this._zoomNextToolStripMenuItem.Name = "_zoomNextToolStripMenuItem";
            this._zoomNextToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomNextToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this._zoomNextToolStripMenuItem.Text = "Zoom next";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(200, 6);
            // 
            // previousTileToolStripMenuItem1
            // 
            this.previousTileToolStripMenuItem1.Enabled = false;
            this.previousTileToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("previousTileToolStripMenuItem1.Image")));
            this.previousTileToolStripMenuItem1.ImageViewer = null;
            this.previousTileToolStripMenuItem1.Name = "previousTileToolStripMenuItem1";
            this.previousTileToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this.previousTileToolStripMenuItem1.Size = new System.Drawing.Size(203, 22);
            this.previousTileToolStripMenuItem1.Text = "Previous tile";
            // 
            // nextTileToolStripMenuItem1
            // 
            this.nextTileToolStripMenuItem1.Enabled = false;
            this.nextTileToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("nextTileToolStripMenuItem1.Image")));
            this.nextTileToolStripMenuItem1.ImageViewer = null;
            this.nextTileToolStripMenuItem1.Name = "nextTileToolStripMenuItem1";
            this.nextTileToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this.nextTileToolStripMenuItem1.Size = new System.Drawing.Size(203, 22);
            this.nextTileToolStripMenuItem1.Text = "Next tile";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(200, 6);
            // 
            // _fitToWidthToolStripMenuItem
            // 
            this._fitToWidthToolStripMenuItem.Enabled = false;
            this._fitToWidthToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_fitToWidthToolStripMenuItem.Image")));
            this._fitToWidthToolStripMenuItem.ImageViewer = null;
            this._fitToWidthToolStripMenuItem.Name = "_fitToWidthToolStripMenuItem";
            this._fitToWidthToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._fitToWidthToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this._fitToWidthToolStripMenuItem.Text = "Fit to &width";
            // 
            // _fitToPageToolStripMenuItem
            // 
            this._fitToPageToolStripMenuItem.Enabled = false;
            this._fitToPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_fitToPageToolStripMenuItem.Image")));
            this._fitToPageToolStripMenuItem.ImageViewer = null;
            this._fitToPageToolStripMenuItem.Name = "_fitToPageToolStripMenuItem";
            this._fitToPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._fitToPageToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this._fitToPageToolStripMenuItem.Text = "Fit to &page";
            // 
            // _oneToOneZoomToolStripMenuItem
            // 
            this._oneToOneZoomToolStripMenuItem.Enabled = false;
            this._oneToOneZoomToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_oneToOneZoomToolStripMenuItem.Image")));
            this._oneToOneZoomToolStripMenuItem.ImageViewer = null;
            this._oneToOneZoomToolStripMenuItem.Name = "_oneToOneZoomToolStripMenuItem";
            this._oneToOneZoomToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._oneToOneZoomToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this._oneToOneZoomToolStripMenuItem.Text = "&One-to-one zoom";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(200, 6);
            // 
            // _rotateCounterclockwiseToolStripMenuItem
            // 
            this._rotateCounterclockwiseToolStripMenuItem.Enabled = false;
            this._rotateCounterclockwiseToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rotateCounterclockwiseToolStripMenuItem.Image")));
            this._rotateCounterclockwiseToolStripMenuItem.ImageViewer = null;
            this._rotateCounterclockwiseToolStripMenuItem.Name = "_rotateCounterclockwiseToolStripMenuItem";
            this._rotateCounterclockwiseToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._rotateCounterclockwiseToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this._rotateCounterclockwiseToolStripMenuItem.Text = "Rotate counterclockwise";
            // 
            // _rotateClockwiseToolStripMenuItem
            // 
            this._rotateClockwiseToolStripMenuItem.Enabled = false;
            this._rotateClockwiseToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rotateClockwiseToolStripMenuItem.Image")));
            this._rotateClockwiseToolStripMenuItem.ImageViewer = null;
            this._rotateClockwiseToolStripMenuItem.Name = "_rotateClockwiseToolStripMenuItem";
            this._rotateClockwiseToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._rotateClockwiseToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this._rotateClockwiseToolStripMenuItem.Text = "Rotate clockwise";
            // 
            // _rotateAllDocumentPagesCounterclockwiseToolStripMenuItem
            // 
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem.Enabled = false;
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rotateCounterclockwiseToolStripMenuItem.Image")));
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem.ImageViewer = null;
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem.Name = "_rotateAllDocumentPagesCounterclockwiseToolStripMenuItem";
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this._rotateAllDocumentPagesCounterclockwiseToolStripMenuItem.Text = "Rotate all pages counterclockwise";
            // 
            // _rotateAllDocumentPagesClockwiseToolStripMenuItem
            // 
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem.Enabled = false;
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rotateClockwiseToolStripMenuItem.Image")));
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem.ImageViewer = null;
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem.Name = "_rotateAllDocumentPagesClockwiseToolStripMenuItem";
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this._rotateAllDocumentPagesClockwiseToolStripMenuItem.Text = "Rotate all pages clockwise";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(200, 6);
            // 
            // _firstPageToolStripMenuItem
            // 
            this._firstPageToolStripMenuItem.Enabled = false;
            this._firstPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_firstPageToolStripMenuItem.Image")));
            this._firstPageToolStripMenuItem.ImageViewer = null;
            this._firstPageToolStripMenuItem.Name = "_firstPageToolStripMenuItem";
            this._firstPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._firstPageToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this._firstPageToolStripMenuItem.Text = "First page";
            // 
            // _previousPageToolStripMenuItem
            // 
            this._previousPageToolStripMenuItem.Enabled = false;
            this._previousPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_previousPageToolStripMenuItem.Image")));
            this._previousPageToolStripMenuItem.ImageViewer = null;
            this._previousPageToolStripMenuItem.Name = "_previousPageToolStripMenuItem";
            this._previousPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._previousPageToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this._previousPageToolStripMenuItem.Text = "Previous page";
            // 
            // _nextPageToolStripMenuItem
            // 
            this._nextPageToolStripMenuItem.Enabled = false;
            this._nextPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_nextPageToolStripMenuItem.Image")));
            this._nextPageToolStripMenuItem.ImageViewer = null;
            this._nextPageToolStripMenuItem.Name = "_nextPageToolStripMenuItem";
            this._nextPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._nextPageToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this._nextPageToolStripMenuItem.Text = "Next page";
            // 
            // _lastPageToolStripMenuItem
            // 
            this._lastPageToolStripMenuItem.Enabled = false;
            this._lastPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_lastPageToolStripMenuItem.Image")));
            this._lastPageToolStripMenuItem.ImageViewer = null;
            this._lastPageToolStripMenuItem.Name = "_lastPageToolStripMenuItem";
            this._lastPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._lastPageToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this._lastPageToolStripMenuItem.Text = "Last page";
            // 
            // _fileCommands
            // 
            this._fileCommands.Dock = System.Windows.Forms.DockStyle.None;
            this._fileCommands.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._fileCommands.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._openImageToolStripSplitButton,
            this._printImageToolStripButton});
            this._fileCommands.Location = new System.Drawing.Point(3, 24);
            this._fileCommands.Name = "_fileCommands";
            this._fileCommands.Size = new System.Drawing.Size(96, 39);
            this._fileCommands.TabIndex = 6;
            // 
            // _openImageToolStripSplitButton
            // 
            this._openImageToolStripSplitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._openImageToolStripSplitButton.Enabled = false;
            this._openImageToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._openImageToolStripSplitButton.ImageViewer = null;
            this._openImageToolStripSplitButton.Name = "_openImageToolStripSplitButton";
            this._openImageToolStripSplitButton.Size = new System.Drawing.Size(48, 36);
            this._openImageToolStripSplitButton.Text = "openImageToolStripSplitButton1";
            this._openImageToolStripSplitButton.ToolTipText = "Open image";
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
            // _basicTools
            // 
            this._basicTools.Dock = System.Windows.Forms.DockStyle.None;
            this._basicTools.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._basicTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomWindowToolStripButton,
            this._panToolStripButton,
            this._highlightToolStripSplitButton,
            this._deleteLayerObjectsToolStripButton,
            this.toolStripSeparator11,
            this._extractImageToolStripButton,
            this.toolStripSeparator16,
            this._invertColorsToolStripButton,
            this.toolStripSeparator13,
            this._thumbnailsToolStripButton});
            this._basicTools.Location = new System.Drawing.Point(332, 24);
            this._basicTools.Name = "_basicTools";
            this._basicTools.Size = new System.Drawing.Size(325, 39);
            this._basicTools.TabIndex = 5;
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
            // _highlightToolStripSplitButton
            // 
            this._highlightToolStripSplitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._highlightToolStripSplitButton.Enabled = false;
            this._highlightToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._highlightToolStripSplitButton.ImageViewer = null;
            this._highlightToolStripSplitButton.Name = "_highlightToolStripSplitButton";
            this._highlightToolStripSplitButton.Size = new System.Drawing.Size(48, 36);
            this._highlightToolStripSplitButton.Text = "Highlighter";
            this._highlightToolStripSplitButton.ToolTipText = "Highlight new text";
            // 
            // _deleteLayerObjectsToolStripButton
            // 
            this._deleteLayerObjectsToolStripButton.BaseToolTipText = "Delete objects";
            this._deleteLayerObjectsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._deleteLayerObjectsToolStripButton.Enabled = false;
            this._deleteLayerObjectsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._deleteLayerObjectsToolStripButton.ImageViewer = null;
            this._deleteLayerObjectsToolStripButton.Name = "_deleteLayerObjectsToolStripButton";
            this._deleteLayerObjectsToolStripButton.Size = new System.Drawing.Size(36, 36);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(6, 39);
            // 
            // _extractImageToolStripButton
            // 
            this._extractImageToolStripButton.BaseToolTipText = "Open subimage in new ImageViewer";
            this._extractImageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._extractImageToolStripButton.Enabled = false;
            this._extractImageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._extractImageToolStripButton.ImageViewer = null;
            this._extractImageToolStripButton.Name = "_extractImageToolStripButton";
            this._extractImageToolStripButton.Size = new System.Drawing.Size(36, 36);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(6, 39);
            // 
            // _thumbnailsToolStripButton
            // 
            this._thumbnailsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._thumbnailsToolStripButton.DockableWindow = null;
            this._thumbnailsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._thumbnailsToolStripButton.Name = "_thumbnailsToolStripButton";
            this._thumbnailsToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._thumbnailsToolStripButton.Text = "Show/Hide thumbnails";
            // 
            // _viewCommands
            // 
            this._viewCommands.Dock = System.Windows.Forms.DockStyle.None;
            this._viewCommands.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._viewCommands.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomInToolStripButton,
            this._zoomOutToolStripButton,
            this._zoomPreviousToolStripButton,
            this._zoomNextToolStripButton,
            this.toolStripSeparator8,
            this._fitToPageToolStripButton,
            this._fitToWidthToolStripButton,
            this._oneToOneZoomToolStripButton,
            this.toolStripSeparator9,
            this._previousTileToolStripButton,
            this._nextTileToolStripButton,
            this.toolStripSeparator10,
            this._rotateCounterclockwiseToolStripButton,
            this._rotateClockwiseToolStripButton});
            this._viewCommands.Location = new System.Drawing.Point(3, 63);
            this._viewCommands.Name = "_viewCommands";
            this._viewCommands.Size = new System.Drawing.Size(426, 39);
            this._viewCommands.TabIndex = 8;
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
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 39);
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
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 39);
            // 
            // _previousTileToolStripButton
            // 
            this._previousTileToolStripButton.BaseToolTipText = "Previous tile";
            this._previousTileToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._previousTileToolStripButton.Enabled = false;
            this._previousTileToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._previousTileToolStripButton.ImageViewer = null;
            this._previousTileToolStripButton.Name = "_previousTileToolStripButton";
            this._previousTileToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._previousTileToolStripButton.Text = "previousTileToolStripButton1";
            // 
            // _nextTileToolStripButton
            // 
            this._nextTileToolStripButton.BaseToolTipText = "Next tile";
            this._nextTileToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextTileToolStripButton.Enabled = false;
            this._nextTileToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextTileToolStripButton.ImageViewer = null;
            this._nextTileToolStripButton.Name = "_nextTileToolStripButton";
            this._nextTileToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextTileToolStripButton.Text = "nextTileToolStripButton1";
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(6, 39);
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
            // _navigationTools
            // 
            this._navigationTools.Dock = System.Windows.Forms.DockStyle.None;
            this._navigationTools.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._navigationTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._firstPageToolStripButton,
            this._previousPageToolStripButton,
            this._pageNavigationToolStripTextBox,
            this._nextPageToolStripButton,
            this._lastPageToolStripButton});
            this._navigationTools.Location = new System.Drawing.Point(99, 24);
            this._navigationTools.Name = "_navigationTools";
            this._navigationTools.Size = new System.Drawing.Size(233, 39);
            this._navigationTools.TabIndex = 7;
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
            // _imageViewerStatusStrip
            // 
            this._imageViewerStatusStrip.Location = new System.Drawing.Point(0, 542);
            this._imageViewerStatusStrip.Name = "_imageViewerStatusStrip";
            this._imageViewerStatusStrip.ShowBackgroundProcessStatus = false;
            this._imageViewerStatusStrip.Size = new System.Drawing.Size(974, 24);
            this._imageViewerStatusStrip.TabIndex = 2;
            this._imageViewerStatusStrip.Text = "_imageViewerStatusStrip";
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
            // _thumbnailDockableWindow
            // 
            this._thumbnailDockableWindow.Controls.Add(this._thumbnailViewer);
            this._thumbnailDockableWindow.Guid = new System.Guid("821a2840-0871-42ed-94a7-22df189299bc");
            this._thumbnailDockableWindow.Location = new System.Drawing.Point(4, 25);
            this._thumbnailDockableWindow.Name = "_thumbnailDockableWindow";
            this._thumbnailDockableWindow.Size = new System.Drawing.Size(222, 494);
            this._thumbnailDockableWindow.TabIndex = 0;
            this._thumbnailDockableWindow.Text = "Page thumbnails";
            // 
            // _thumbnailViewer
            // 
            this._thumbnailViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._thumbnailViewer.ImageViewer = null;
            this._thumbnailViewer.Location = new System.Drawing.Point(0, 0);
            this._thumbnailViewer.Name = "_thumbnailViewer";
            this._thumbnailViewer.Size = new System.Drawing.Size(222, 494);
            this._thumbnailViewer.TabIndex = 0;
            // 
            // dockContainer1
            // 
            this.dockContainer1.ContentSize = 222;
            this.dockContainer1.Controls.Add(this._thumbnailDockableWindow);
            this.dockContainer1.Dock = System.Windows.Forms.DockStyle.Right;
            this.dockContainer1.LayoutSystem = new TD.SandDock.SplitLayoutSystem(new System.Drawing.SizeF(250F, 400F), System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 400F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._thumbnailDockableWindow))}, this._thumbnailDockableWindow)))});
            this.dockContainer1.Location = new System.Drawing.Point(748, 0);
            this.dockContainer1.Manager = this._sandDockManager;
            this.dockContainer1.Name = "dockContainer1";
            this.dockContainer1.Size = new System.Drawing.Size(226, 542);
            this.dockContainer1.TabIndex = 3;
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(6, 39);
            // 
            // _invertColorsToolStripButton
            // 
            this._invertColorsToolStripButton.BaseToolTipText = "Invert image colors";
            this._invertColorsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._invertColorsToolStripButton.Enabled = false;
            this._invertColorsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._invertColorsToolStripButton.ImageViewer = null;
            this._invertColorsToolStripButton.Name = "_invertColorsToolStripButton";
            this._invertColorsToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._invertColorsToolStripButton.Text = "Invert image colors";
            // 
            // ExtractImageViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(974, 566);
            this.Controls.Add(this._toolStripContainer);
            this.Controls.Add(this.dockContainer1);
            this.Controls.Add(this._imageViewerStatusStrip);
            this.MainMenuStrip = this._menuStrip;
            this.Name = "ExtractImageViewerForm";
            this.Text = "Extract Image Viewer";
            this._toolStripContainer.ContentPanel.ResumeLayout(false);
            this._toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this._toolStripContainer.TopToolStripPanel.PerformLayout();
            this._toolStripContainer.ResumeLayout(false);
            this._toolStripContainer.PerformLayout();
            this._contextMenu.ResumeLayout(false);
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this._fileCommands.ResumeLayout(false);
            this._fileCommands.PerformLayout();
            this._basicTools.ResumeLayout(false);
            this._basicTools.PerformLayout();
            this._viewCommands.ResumeLayout(false);
            this._viewCommands.PerformLayout();
            this._navigationTools.ResumeLayout(false);
            this._navigationTools.PerformLayout();
            this._thumbnailDockableWindow.ResumeLayout(false);
            this.dockContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer _toolStripContainer;
        private Extract.Imaging.Forms.ImageViewer _imageViewer;
        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private Extract.Imaging.Forms.ImageViewerStatusStrip _imageViewerStatusStrip;
        private Extract.Imaging.Forms.OpenImageToolStripMenuItem _openImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private Extract.Imaging.Forms.PrintImageToolStripMenuItem _printImageToolStripMenuItem;
        private Extract.Imaging.Forms.ZoomWindowToolStripMenuItem _zoomWindowToolStripMenuItem;
        private Extract.Imaging.Forms.PanToolStripMenuItem _panToolStripMenuItem;
        private Extract.Imaging.Forms.ZoomInToolStripMenuItem _zoomInToolStripMenuItem;
        private Extract.Imaging.Forms.ZoomOutToolStripMenuItem _zoomOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private Extract.Imaging.Forms.ZoomPreviousToolStripMenuItem _zoomPreviousToolStripMenuItem;
        private Extract.Imaging.Forms.ZoomNextToolStripMenuItem _zoomNextToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private Extract.Imaging.Forms.FitToWidthToolStripMenuItem _fitToWidthToolStripMenuItem;
        private Extract.Imaging.Forms.FitToPageToolStripMenuItem _fitToPageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private Extract.Imaging.Forms.RotateCounterclockwiseToolStripMenuItem _rotateCounterclockwiseToolStripMenuItem;
        private Extract.Imaging.Forms.RotateClockwiseToolStripMenuItem _rotateClockwiseToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private Extract.Imaging.Forms.FirstPageToolStripMenuItem _firstPageToolStripMenuItem;
        private Extract.Imaging.Forms.LastPageToolStripMenuItem _lastPageToolStripMenuItem;
        private Extract.Imaging.Forms.NextPageToolStripMenuItem _nextPageToolStripMenuItem;
        private Extract.Imaging.Forms.PreviousPageToolStripMenuItem _previousPageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private Extract.Imaging.Forms.OpenImageMruToolStripMenuItem _openImageMruToolStripMenuItem;
        private Extract.Imaging.Forms.PreviousTileToolStripMenuItem previousTileToolStripMenuItem1;
        private Extract.Imaging.Forms.NextTileToolStripMenuItem nextTileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStrip _basicTools;
        private Extract.Imaging.Forms.ZoomWindowToolStripButton _zoomWindowToolStripButton;
        private Extract.Imaging.Forms.PanToolStripButton _panToolStripButton;
        private Extract.Imaging.Forms.HighlightToolStripSplitButton _highlightToolStripSplitButton;
        private System.Windows.Forms.ToolStrip _fileCommands;
        private Extract.Imaging.Forms.OpenImageToolStripSplitButton _openImageToolStripSplitButton;
        private Extract.Imaging.Forms.PrintImageToolStripButton _printImageToolStripButton;
        private System.Windows.Forms.ToolStrip _navigationTools;
        private Extract.Imaging.Forms.FirstPageToolStripButton _firstPageToolStripButton;
        private Extract.Imaging.Forms.PreviousPageToolStripButton _previousPageToolStripButton;
        private System.Windows.Forms.ToolStrip _viewCommands;
        private Extract.Imaging.Forms.PageNavigationToolStripTextBox _pageNavigationToolStripTextBox;
        private Extract.Imaging.Forms.NextPageToolStripButton _nextPageToolStripButton;
        private Extract.Imaging.Forms.LastPageToolStripButton _lastPageToolStripButton;
        private Extract.Imaging.Forms.ZoomInToolStripButton _zoomInToolStripButton;
        private Extract.Imaging.Forms.ZoomOutToolStripButton _zoomOutToolStripButton;
        private Extract.Imaging.Forms.ZoomPreviousToolStripButton _zoomPreviousToolStripButton;
        private Extract.Imaging.Forms.ZoomNextToolStripButton _zoomNextToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private Extract.Imaging.Forms.FitToPageToolStripButton _fitToPageToolStripButton;
        private Extract.Imaging.Forms.FitToWidthToolStripButton _fitToWidthToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private Extract.Imaging.Forms.PreviousTileToolStripButton _previousTileToolStripButton;
        private Extract.Imaging.Forms.NextTileToolStripButton _nextTileToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton _rotateCounterclockwiseToolStripButton;
        private Extract.Imaging.Forms.RotateClockwiseToolStripButton _rotateClockwiseToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem _searchForImagesToolStripMenuItem;
        private TD.SandDock.SandDockManager _sandDockManager;
        private TD.SandDock.DockableWindow _thumbnailDockableWindow;
        private TD.SandDock.DockContainer dockContainer1;
        private Extract.Imaging.Forms.ThumbnailViewer _thumbnailViewer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private Extract.Imaging.Forms.ThumbnailViewerToolStripButton _thumbnailsToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripMenuItem _exitToolStripMenuItem;
        private Extract.Imaging.Forms.DeleteLayerObjectsToolStripMenuItem _deleteLayerObjectsToolStripMenuItem;
        private Extract.Imaging.Forms.DeleteLayerObjectsToolStripButton _deleteLayerObjectsToolStripButton;
        private Extract.Imaging.Forms.ExtractImageToolStripButton _extractImageToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private Forms.AngularHighlightToolStripMenuItem _angularHighlightToolStripMenuItem;
        private Forms.RectangularHighlightToolStripMenuItem _rectangularHighlightToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip _contextMenu;
        private Forms.ZoomWindowToolStripMenuItem _zoomWindowContextMenuItem;
        private Forms.PanToolStripMenuItem _panContextMenuItem;
        private Forms.ZoomPreviousToolStripMenuItem _zoomPreviousContextMenuItem;
        private Forms.ZoomNextToolStripMenuItem _zoomNextContextMenuItem;
        private Forms.AngularHighlightToolStripMenuItem _angularHighlightContextMenuItem;
        private Forms.RectangularHighlightToolStripMenuItem _rectangularHighlightContextMenuItem;
        private Forms.OneToOneZoomToolStripButton _oneToOneZoomToolStripButton;
        private Forms.OneToOneZoomToolStripMenuItem _oneToOneZoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator16;
        private Forms.InvertColorsToolStripButton _invertColorsToolStripButton;
        private Imaging.Forms.RotateAllDocumentPagesClockwiseToolStripMenuItem _rotateAllDocumentPagesClockwiseToolStripMenuItem;
        private Imaging.Forms.RotateAllDocumentPagesCounterclockwiseToolStripMenuItem _rotateAllDocumentPagesCounterclockwiseToolStripMenuItem;
    }
}

