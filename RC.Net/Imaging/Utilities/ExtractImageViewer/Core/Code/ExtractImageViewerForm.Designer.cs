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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Leadtools.WinForms.RasterMagnifyGlass rasterMagnifyGlass1 = new Leadtools.WinForms.RasterMagnifyGlass();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtractImageViewerForm));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this._imageViewer = new Extract.Imaging.Forms.ImageViewer();
            this._imageViewerContextMenuStrip = new Extract.Imaging.Forms.ImageViewerContextMenuStrip();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openImageToolStripMenuItem = new Extract.Imaging.Forms.OpenImageToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._printImageToolStripMenuItem = new Extract.Imaging.Forms.PrintImageToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this._openImageMruToolStripMenuItem = new Extract.Imaging.Forms.OpenImageMruToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._zoomWindowToolStripMenuItem = new Extract.Imaging.Forms.ZoomWindowToolStripMenuItem();
            this._panToolStripMenuItem = new Extract.Imaging.Forms.PanToolStripMenuItem();
            this._highlightToolStripMenuItem = new Extract.Imaging.Forms.HighlightToolStripMenuItem();
            this._selectLayerObjectToolStripMenuItem = new Extract.Imaging.Forms.SelectLayerObjectToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._zoomInToolStripMenuItem = new Extract.Imaging.Forms.ZoomInToolStripMenuItem();
            this._zoomOutToolStripMenuItem = new Extract.Imaging.Forms.ZoomOutToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomPreviousToolStripMenuItem = new Extract.Imaging.Forms.ZoomPreviousToolStripMenuItem();
            this._zoomNextToolStripMenuItem = new Extract.Imaging.Forms.ZoomNextToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._fitToWidthToolStripMenuItem = new Extract.Imaging.Forms.FitToWidthToolStripMenuItem();
            this._fitToPageToolStripMenuItem = new Extract.Imaging.Forms.FitToPageToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this._rotateCounterclockwiseToolStripMenuItem = new Extract.Imaging.Forms.RotateCounterclockwiseToolStripMenuItem();
            this._rotateClockwiseToolStripMenuItem = new Extract.Imaging.Forms.RotateClockwiseToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this._firstPageToolStripMenuItem = new Extract.Imaging.Forms.FirstPageToolStripMenuItem();
            this._previousPageToolStripMenuItem = new Extract.Imaging.Forms.PreviousPageToolStripMenuItem();
            this._nextPageToolStripMenuItem = new Extract.Imaging.Forms.NextPageToolStripMenuItem();
            this._lastPageToolStripMenuItem = new Extract.Imaging.Forms.LastPageToolStripMenuItem();
            this._fileCommands = new Extract.Imaging.Forms.FileCommandsImageViewerToolStrip();
            this._viewCommands = new Extract.Imaging.Forms.ViewCommandsImageViewerToolStrip();
            this._navigationTools = new Extract.Imaging.Forms.NavigationToolsImageViewerToolStrip();
            this._basicTools = new Extract.Imaging.Forms.BasicToolsImageViewerToolStrip();
            this._imageViewerStatusStrip = new Extract.Imaging.Forms.ImageViewerStatusStrip();
            this.previousTileToolStripMenuItem1 = new Extract.Imaging.Forms.PreviousTileToolStripMenuItem();
            this.nextTileToolStripMenuItem1 = new Extract.Imaging.Forms.NextTileToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this._imageViewer);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(742, 395);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.LeftToolStripPanelVisible = false;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.RightToolStripPanelVisible = false;
            this.toolStripContainer1.Size = new System.Drawing.Size(742, 444);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this._fileCommands);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this._viewCommands);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this._navigationTools);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this._basicTools);
            // 
            // _imageViewer
            // 
            this._imageViewer.AllowDrop = true;
            this._imageViewer.AnimateFloater = true;
            this._imageViewer.AnimateRegion = true;
            this._imageViewer.AutoDisposeImages = true;
            this._imageViewer.AutoResetScaleFactor = true;
            this._imageViewer.AutoResetScrollPosition = true;
            this._imageViewer.AutoScroll = true;
            this._imageViewer.BindingData = null;
            this._imageViewer.BindingLoadBitsPerPixel = 24;
            this._imageViewer.BindingRasterCodecs = null;
            this._imageViewer.BindingSaveBitsPerPixel = 24;
            this._imageViewer.BindingSaveImageFormat = Leadtools.RasterImageFormat.Jpeg;
            this._imageViewer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._imageViewer.ContextMenuStrip = this._imageViewerContextMenuStrip;
            this._imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._imageViewer.DoubleBuffer = true;
            this._imageViewer.EnableScrollingInterface = false;
            this._imageViewer.EnableTimer = false;
            this._imageViewer.FloaterImage = null;
            this._imageViewer.FloaterPosition = new System.Drawing.Point(0, 0);
            this._imageViewer.FloaterVisible = true;
            this._imageViewer.FrameColor = System.Drawing.Color.Transparent;
            this._imageViewer.FrameShadowColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this._imageViewer.FrameShadowSize = new System.Drawing.SizeF(0F, 0F);
            this._imageViewer.FramesIsPartOfImage = true;
            this._imageViewer.FrameSize = new System.Drawing.SizeF(0F, 0F);
            this._imageViewer.HorizontalAlignMode = Leadtools.RasterPaintAlignMode.Near;
            this._imageViewer.Location = new System.Drawing.Point(0, 0);
            rasterMagnifyGlass1.Border3DStyle = System.Windows.Forms.Border3DStyle.Raised;
            rasterMagnifyGlass1.BorderColor = System.Drawing.Color.Black;
            rasterMagnifyGlass1.BorderWidth = 1;
            rasterMagnifyGlass1.Crosshair = Leadtools.WinForms.RasterMagnifyGlassCrosshair.Fine;
            rasterMagnifyGlass1.CrosshairColor = System.Drawing.Color.Black;
            rasterMagnifyGlass1.CrosshairWidth = 1;
            rasterMagnifyGlass1.RoundRectangleEllipseSize = new System.Drawing.Size(20, 20);
            rasterMagnifyGlass1.ScaleFactor = 2F;
            rasterMagnifyGlass1.Shape = Leadtools.WinForms.RasterMagnifyGlassShape.Rectangle;
            rasterMagnifyGlass1.Size = new System.Drawing.Size(150, 150);
            this._imageViewer.MagnifyGlass = rasterMagnifyGlass1;
            this._imageViewer.Name = "_imageViewer";
            this._imageViewer.ScaleFactor = 1;
            this._imageViewer.ScrollPosition = new System.Drawing.Point(0, 0);
            this._imageViewer.Size = new System.Drawing.Size(742, 395);
            this._imageViewer.SmallScrollChangeRatio = 20;
            this._imageViewer.SourceRectangle = new System.Drawing.Rectangle(0, 0, 0, 0);
            this._imageViewer.TabIndex = 0;
            this._imageViewer.Text = "imageViewer1";
            this._imageViewer.UseDefaultShortcuts = true;
            this._imageViewer.UseDpi = false;
            this._imageViewer.VerticalAlignMode = Leadtools.RasterPaintAlignMode.Near;
            // 
            // _imageViewerContextMenuStrip
            // 
            this._imageViewerContextMenuStrip.Name = "_imageViewerContextMenuStrip";
            this._imageViewerContextMenuStrip.Size = new System.Drawing.Size(156, 126);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(742, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._openImageToolStripMenuItem,
            this.toolStripSeparator1,
            this._printImageToolStripMenuItem,
            this.toolStripSeparator6,
            this._openImageMruToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // _openImageToolStripMenuItem
            // 
            this._openImageToolStripMenuItem.Enabled = false;
            this._openImageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_openImageToolStripMenuItem.Image")));
            this._openImageToolStripMenuItem.ImageViewer = null;
            this._openImageToolStripMenuItem.Name = "_openImageToolStripMenuItem";
            this._openImageToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._openImageToolStripMenuItem.Text = "&Open image";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(152, 6);
            // 
            // _printImageToolStripMenuItem
            // 
            this._printImageToolStripMenuItem.Enabled = false;
            this._printImageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_printImageToolStripMenuItem.Image")));
            this._printImageToolStripMenuItem.ImageViewer = null;
            this._printImageToolStripMenuItem.Name = "_printImageToolStripMenuItem";
            this._printImageToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._printImageToolStripMenuItem.Text = "&Print";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(152, 6);
            // 
            // _openImageMruToolStripMenuItem
            // 
            this._openImageMruToolStripMenuItem.Enabled = false;
            this._openImageMruToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_openImageMruToolStripMenuItem.Image")));
            this._openImageMruToolStripMenuItem.ImageViewer = null;
            this._openImageMruToolStripMenuItem.Name = "_openImageMruToolStripMenuItem";
            this._openImageMruToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._openImageMruToolStripMenuItem.Text = "Recent images";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomWindowToolStripMenuItem,
            this._panToolStripMenuItem,
            this._highlightToolStripMenuItem,
            this._selectLayerObjectToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // _zoomWindowToolStripMenuItem
            // 
            this._zoomWindowToolStripMenuItem.Enabled = false;
            this._zoomWindowToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomWindowToolStripMenuItem.Image")));
            this._zoomWindowToolStripMenuItem.ImageViewer = null;
            this._zoomWindowToolStripMenuItem.Name = "_zoomWindowToolStripMenuItem";
            this._zoomWindowToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this._zoomWindowToolStripMenuItem.Text = "&Zoom window";
            // 
            // _panToolStripMenuItem
            // 
            this._panToolStripMenuItem.Enabled = false;
            this._panToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_panToolStripMenuItem.Image")));
            this._panToolStripMenuItem.ImageViewer = null;
            this._panToolStripMenuItem.Name = "_panToolStripMenuItem";
            this._panToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this._panToolStripMenuItem.Text = "P&an";
            // 
            // _highlightToolStripMenuItem
            // 
            this._highlightToolStripMenuItem.Enabled = false;
            this._highlightToolStripMenuItem.ImageViewer = null;
            this._highlightToolStripMenuItem.Name = "_highlightToolStripMenuItem";
            this._highlightToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this._highlightToolStripMenuItem.ToolTipText = "Highlight new text or select pre-highlighted text";
            // 
            // _selectLayerObjectToolStripMenuItem
            // 
            this._selectLayerObjectToolStripMenuItem.Enabled = false;
            this._selectLayerObjectToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_selectHighlightToolStripMenuItem.Image")));
            this._selectLayerObjectToolStripMenuItem.ImageViewer = null;
            this._selectLayerObjectToolStripMenuItem.Name = "_selectHighlightToolStripMenuItem";
            this._selectLayerObjectToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this._selectLayerObjectToolStripMenuItem.Text = "Select highlight";
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
            this.toolStripSeparator4,
            this._rotateCounterclockwiseToolStripMenuItem,
            this._rotateClockwiseToolStripMenuItem,
            this.toolStripSeparator5,
            this._firstPageToolStripMenuItem,
            this._previousPageToolStripMenuItem,
            this._nextPageToolStripMenuItem,
            this._lastPageToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // _zoomInToolStripMenuItem
            // 
            this._zoomInToolStripMenuItem.Enabled = false;
            this._zoomInToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomInToolStripMenuItem.Image")));
            this._zoomInToolStripMenuItem.ImageViewer = null;
            this._zoomInToolStripMenuItem.Name = "_zoomInToolStripMenuItem";
            this._zoomInToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._zoomInToolStripMenuItem.Text = "Zoom in";
            // 
            // _zoomOutToolStripMenuItem
            // 
            this._zoomOutToolStripMenuItem.Enabled = false;
            this._zoomOutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomOutToolStripMenuItem.Image")));
            this._zoomOutToolStripMenuItem.ImageViewer = null;
            this._zoomOutToolStripMenuItem.Name = "_zoomOutToolStripMenuItem";
            this._zoomOutToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._zoomOutToolStripMenuItem.Text = "Zoom out";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(199, 6);
            // 
            // _zoomPreviousToolStripMenuItem
            // 
            this._zoomPreviousToolStripMenuItem.Enabled = false;
            this._zoomPreviousToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomPreviousToolStripMenuItem.Image")));
            this._zoomPreviousToolStripMenuItem.ImageViewer = null;
            this._zoomPreviousToolStripMenuItem.Name = "_zoomPreviousToolStripMenuItem";
            this._zoomPreviousToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._zoomPreviousToolStripMenuItem.Text = "Zoom previous";
            // 
            // _zoomNextToolStripMenuItem
            // 
            this._zoomNextToolStripMenuItem.Enabled = false;
            this._zoomNextToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomNextToolStripMenuItem.Image")));
            this._zoomNextToolStripMenuItem.ImageViewer = null;
            this._zoomNextToolStripMenuItem.Name = "_zoomNextToolStripMenuItem";
            this._zoomNextToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._zoomNextToolStripMenuItem.Text = "Zoom next";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(199, 6);
            // 
            // _fitToWidthToolStripMenuItem
            // 
            this._fitToWidthToolStripMenuItem.Enabled = false;
            this._fitToWidthToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_fitToWidthToolStripMenuItem.Image")));
            this._fitToWidthToolStripMenuItem.ImageViewer = null;
            this._fitToWidthToolStripMenuItem.Name = "_fitToWidthToolStripMenuItem";
            this._fitToWidthToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._fitToWidthToolStripMenuItem.Text = "Fit to &width";
            // 
            // _fitToPageToolStripMenuItem
            // 
            this._fitToPageToolStripMenuItem.Enabled = false;
            this._fitToPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_fitToPageToolStripMenuItem.Image")));
            this._fitToPageToolStripMenuItem.ImageViewer = null;
            this._fitToPageToolStripMenuItem.Name = "_fitToPageToolStripMenuItem";
            this._fitToPageToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._fitToPageToolStripMenuItem.Text = "Fit to &page";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(199, 6);
            // 
            // _rotateCounterclockwiseToolStripMenuItem
            // 
            this._rotateCounterclockwiseToolStripMenuItem.Enabled = false;
            this._rotateCounterclockwiseToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rotateCounterclockwiseToolStripMenuItem.Image")));
            this._rotateCounterclockwiseToolStripMenuItem.ImageViewer = null;
            this._rotateCounterclockwiseToolStripMenuItem.Name = "_rotateCounterclockwiseToolStripMenuItem";
            this._rotateCounterclockwiseToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._rotateCounterclockwiseToolStripMenuItem.Text = "Rotate counterclockwise";
            // 
            // _rotateClockwiseToolStripMenuItem
            // 
            this._rotateClockwiseToolStripMenuItem.Enabled = false;
            this._rotateClockwiseToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rotateClockwiseToolStripMenuItem.Image")));
            this._rotateClockwiseToolStripMenuItem.ImageViewer = null;
            this._rotateClockwiseToolStripMenuItem.Name = "_rotateClockwiseToolStripMenuItem";
            this._rotateClockwiseToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._rotateClockwiseToolStripMenuItem.Text = "Rotate clockwise";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(199, 6);
            // 
            // _firstPageToolStripMenuItem
            // 
            this._firstPageToolStripMenuItem.Enabled = false;
            this._firstPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_firstPageToolStripMenuItem.Image")));
            this._firstPageToolStripMenuItem.ImageViewer = null;
            this._firstPageToolStripMenuItem.Name = "_firstPageToolStripMenuItem";
            this._firstPageToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._firstPageToolStripMenuItem.Text = "First page";
            // 
            // _previousPageToolStripMenuItem
            // 
            this._previousPageToolStripMenuItem.Enabled = false;
            this._previousPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_previousPageToolStripMenuItem.Image")));
            this._previousPageToolStripMenuItem.ImageViewer = null;
            this._previousPageToolStripMenuItem.Name = "_previousPageToolStripMenuItem";
            this._previousPageToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._previousPageToolStripMenuItem.Text = "Previous page";
            // 
            // _nextPageToolStripMenuItem
            // 
            this._nextPageToolStripMenuItem.Enabled = false;
            this._nextPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_nextPageToolStripMenuItem.Image")));
            this._nextPageToolStripMenuItem.ImageViewer = null;
            this._nextPageToolStripMenuItem.Name = "_nextPageToolStripMenuItem";
            this._nextPageToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._nextPageToolStripMenuItem.Text = "Next page";
            // 
            // _lastPageToolStripMenuItem
            // 
            this._lastPageToolStripMenuItem.Enabled = false;
            this._lastPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_lastPageToolStripMenuItem.Image")));
            this._lastPageToolStripMenuItem.ImageViewer = null;
            this._lastPageToolStripMenuItem.Name = "_lastPageToolStripMenuItem";
            this._lastPageToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._lastPageToolStripMenuItem.Text = "Last page";
            // 
            // _fileCommands
            // 
            this._fileCommands.Dock = System.Windows.Forms.DockStyle.None;
            this._fileCommands.Location = new System.Drawing.Point(3, 24);
            this._fileCommands.Name = "_fileCommands";
            this._fileCommands.Size = new System.Drawing.Size(67, 25);
            this._fileCommands.TabIndex = 1;
            // 
            // _viewCommands
            // 
            this._viewCommands.Dock = System.Windows.Forms.DockStyle.None;
            this._viewCommands.Location = new System.Drawing.Point(70, 24);
            this._viewCommands.Name = "_viewCommands";
            this._viewCommands.Size = new System.Drawing.Size(208, 25);
            this._viewCommands.TabIndex = 4;
            // 
            // _navigationTools
            // 
            this._navigationTools.Dock = System.Windows.Forms.DockStyle.None;
            this._navigationTools.Location = new System.Drawing.Point(278, 24);
            this._navigationTools.Name = "_navigationTools";
            this._navigationTools.Size = new System.Drawing.Size(181, 25);
            this._navigationTools.TabIndex = 2;
            // 
            // _basicTools
            // 
            this._basicTools.Dock = System.Windows.Forms.DockStyle.None;
            this._basicTools.Location = new System.Drawing.Point(459, 24);
            this._basicTools.Name = "_basicTools";
            this._basicTools.Size = new System.Drawing.Size(171, 25);
            this._basicTools.TabIndex = 3;
            // 
            // _imageViewerStatusStrip
            // 
            this._imageViewerStatusStrip.Location = new System.Drawing.Point(0, 444);
            this._imageViewerStatusStrip.Name = "_imageViewerStatusStrip";
            this._imageViewerStatusStrip.Size = new System.Drawing.Size(742, 22);
            this._imageViewerStatusStrip.TabIndex = 2;
            this._imageViewerStatusStrip.Text = "_imageViewerStatusStrip";
            // 
            // previousTileToolStripMenuItem1
            // 
            this.previousTileToolStripMenuItem1.Enabled = false;
            this.previousTileToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("previousTileToolStripMenuItem1.Image")));
            this.previousTileToolStripMenuItem1.ImageViewer = null;
            this.previousTileToolStripMenuItem1.Name = "previousTileToolStripMenuItem1";
            this.previousTileToolStripMenuItem1.Size = new System.Drawing.Size(202, 22);
            this.previousTileToolStripMenuItem1.Text = "Previous tile";
            // 
            // nextTileToolStripMenuItem1
            // 
            this.nextTileToolStripMenuItem1.Enabled = false;
            this.nextTileToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("nextTileToolStripMenuItem1.Image")));
            this.nextTileToolStripMenuItem1.ImageViewer = null;
            this.nextTileToolStripMenuItem1.Name = "nextTileToolStripMenuItem1";
            this.nextTileToolStripMenuItem1.Size = new System.Drawing.Size(202, 22);
            this.nextTileToolStripMenuItem1.Text = "Next tile";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(199, 6);
            // 
            // ExtractImageViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(742, 466);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this._imageViewerStatusStrip);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ExtractImageViewerForm";
            this.Text = "Extract Image Viewer";
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private Extract.Imaging.Forms.ImageViewer _imageViewer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private Extract.Imaging.Forms.FileCommandsImageViewerToolStrip _fileCommands;
        private Extract.Imaging.Forms.ImageViewerContextMenuStrip _imageViewerContextMenuStrip;
        private Extract.Imaging.Forms.ImageViewerStatusStrip _imageViewerStatusStrip;
        private Extract.Imaging.Forms.OpenImageToolStripMenuItem _openImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private Extract.Imaging.Forms.PrintImageToolStripMenuItem _printImageToolStripMenuItem;
        private Extract.Imaging.Forms.ZoomWindowToolStripMenuItem _zoomWindowToolStripMenuItem;
        private Extract.Imaging.Forms.PanToolStripMenuItem _panToolStripMenuItem;
        private Extract.Imaging.Forms.HighlightToolStripMenuItem _highlightToolStripMenuItem;
        private Extract.Imaging.Forms.SelectLayerObjectToolStripMenuItem _selectLayerObjectToolStripMenuItem;
        private Extract.Imaging.Forms.NavigationToolsImageViewerToolStrip _navigationTools;
        private Extract.Imaging.Forms.BasicToolsImageViewerToolStrip _basicTools;
        private Extract.Imaging.Forms.ViewCommandsImageViewerToolStrip _viewCommands;
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
    }
}

