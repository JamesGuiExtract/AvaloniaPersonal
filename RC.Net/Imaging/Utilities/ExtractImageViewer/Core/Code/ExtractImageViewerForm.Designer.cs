using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;

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

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtractImageViewerForm));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this._imageViewer = new Extract.Imaging.Forms.ImageViewer();
            this._imageViewerContextMenuStrip = new Extract.Imaging.Forms.ImageViewerContextMenuStrip();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openImageToolStripMenuItem = new Extract.Imaging.Forms.OpenImageToolStripMenuItem();
            this._searchForImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.previousTileToolStripMenuItem1 = new Extract.Imaging.Forms.PreviousTileToolStripMenuItem();
            this.nextTileToolStripMenuItem1 = new Extract.Imaging.Forms.NextTileToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
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
            this._basicTools = new System.Windows.Forms.ToolStrip();
            this.zoomWindowToolStripButton1 = new Extract.Imaging.Forms.ZoomWindowToolStripButton();
            this.panToolStripButton1 = new Extract.Imaging.Forms.PanToolStripButton();
            this.highlightToolStripSplitButton1 = new Extract.Imaging.Forms.HighlightToolStripSplitButton();
            this.selectLayerObjectToolStripButton1 = new Extract.Imaging.Forms.SelectLayerObjectToolStripButton();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.editHighlightTextToolStripButton1 = new Extract.Imaging.Forms.EditHighlightTextToolStripButton();
            this._fileCommands = new System.Windows.Forms.ToolStrip();
            this.openImageToolStripSplitButton1 = new Extract.Imaging.Forms.OpenImageToolStripSplitButton();
            this._saveGddImageButton = new System.Windows.Forms.ToolStripButton();
            this.printImageToolStripButton1 = new Extract.Imaging.Forms.PrintImageToolStripButton();
            this._navigationTools = new System.Windows.Forms.ToolStrip();
            this.firstPageToolStripButton1 = new Extract.Imaging.Forms.FirstPageToolStripButton();
            this.previousPageToolStripButton1 = new Extract.Imaging.Forms.PreviousPageToolStripButton();
            this.pageNavigationToolStripTextBox1 = new Extract.Imaging.Forms.PageNavigationToolStripTextBox();
            this.nextPageToolStripButton1 = new Extract.Imaging.Forms.NextPageToolStripButton();
            this.lastPageToolStripButton1 = new Extract.Imaging.Forms.LastPageToolStripButton();
            this._viewCommands = new System.Windows.Forms.ToolStrip();
            this.zoomInToolStripButton1 = new Extract.Imaging.Forms.ZoomInToolStripButton();
            this.zoomOutToolStripButton1 = new Extract.Imaging.Forms.ZoomOutToolStripButton();
            this.zoomPreviousToolStripButton1 = new Extract.Imaging.Forms.ZoomPreviousToolStripButton();
            this.zoomNextToolStripButton1 = new Extract.Imaging.Forms.ZoomNextToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.fitToPageToolStripButton1 = new Extract.Imaging.Forms.FitToPageToolStripButton();
            this.fitToWidthToolStripButton1 = new Extract.Imaging.Forms.FitToWidthToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.previousTileToolStripButton1 = new Extract.Imaging.Forms.PreviousTileToolStripButton();
            this.nextTileToolStripButton1 = new Extract.Imaging.Forms.NextTileToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.rotateCounterclockwiseToolStripButton1 = new Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton();
            this.rotateClockwiseToolStripButton1 = new Extract.Imaging.Forms.RotateClockwiseToolStripButton();
            this._imageViewerStatusStrip = new Extract.Imaging.Forms.ImageViewerStatusStrip();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this._basicTools.SuspendLayout();
            this._fileCommands.SuspendLayout();
            this._navigationTools.SuspendLayout();
            this._viewCommands.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this._imageViewer);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(742, 264);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.LeftToolStripPanelVisible = false;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.RightToolStripPanelVisible = false;
            this.toolStripContainer1.Size = new System.Drawing.Size(742, 444);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.TabStop = false;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this._basicTools);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this._fileCommands);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this._navigationTools);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this._viewCommands);
            // 
            // _imageViewer
            // 
            this._imageViewer.AllowDrop = true;
            this._imageViewer.ContextMenuStrip = this._imageViewerContextMenuStrip;
            this._imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._imageViewer.FrameColor = System.Drawing.Color.Transparent;
            this._imageViewer.Location = new System.Drawing.Point(0, 0);
            this._imageViewer.Name = "_imageViewer";
            this._imageViewer.Size = new System.Drawing.Size(742, 264);
            this._imageViewer.TabIndex = 0;
            this._imageViewer.Text = "imageViewer1";
            this._imageViewer.UseDefaultShortcuts = true;
            this._imageViewer.ImageFileChanged += new System.EventHandler<Extract.Imaging.Forms.ImageFileChangedEventArgs>(this.HandleImageViewerImageFileChanged);
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
            this._searchForImagesToolStripMenuItem,
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
            this._openImageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._openImageToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this._openImageToolStripMenuItem.Text = "&Open image";
            // 
            // _searchForImagesToolStripMenuItem
            // 
            this._searchForImagesToolStripMenuItem.Name = "_searchForImagesToolStripMenuItem";
            this._searchForImagesToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this._searchForImagesToolStripMenuItem.Text = "Search for images...";
            this._searchForImagesToolStripMenuItem.Click += new System.EventHandler(this.HandleSearchForImageFilesClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(180, 6);
            // 
            // _printImageToolStripMenuItem
            // 
            this._printImageToolStripMenuItem.Enabled = false;
            this._printImageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_printImageToolStripMenuItem.Image")));
            this._printImageToolStripMenuItem.ImageViewer = null;
            this._printImageToolStripMenuItem.Name = "_printImageToolStripMenuItem";
            this._printImageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._printImageToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this._printImageToolStripMenuItem.Text = "&Print";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(180, 6);
            // 
            // _openImageMruToolStripMenuItem
            // 
            this._openImageMruToolStripMenuItem.Enabled = false;
            this._openImageMruToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_openImageMruToolStripMenuItem.Image")));
            this._openImageMruToolStripMenuItem.ImageViewer = null;
            this._openImageMruToolStripMenuItem.Name = "_openImageMruToolStripMenuItem";
            this._openImageMruToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._openImageMruToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
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
            this._zoomWindowToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomWindowToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this._zoomWindowToolStripMenuItem.Text = "&Zoom window";
            // 
            // _panToolStripMenuItem
            // 
            this._panToolStripMenuItem.Enabled = false;
            this._panToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_panToolStripMenuItem.Image")));
            this._panToolStripMenuItem.ImageViewer = null;
            this._panToolStripMenuItem.Name = "_panToolStripMenuItem";
            this._panToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._panToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this._panToolStripMenuItem.Text = "P&an";
            // 
            // _highlightToolStripMenuItem
            // 
            this._highlightToolStripMenuItem.Enabled = false;
            this._highlightToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_highlightToolStripMenuItem.Image")));
            this._highlightToolStripMenuItem.ImageViewer = null;
            this._highlightToolStripMenuItem.Name = "_highlightToolStripMenuItem";
            this._highlightToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._highlightToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this._highlightToolStripMenuItem.Text = "Highlighter";
            this._highlightToolStripMenuItem.ToolTipText = "Highlight new text or select pre-highlighted text";
            // 
            // _selectLayerObjectToolStripMenuItem
            // 
            this._selectLayerObjectToolStripMenuItem.Enabled = false;
            this._selectLayerObjectToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_selectLayerObjectToolStripMenuItem.Image")));
            this._selectLayerObjectToolStripMenuItem.ImageViewer = null;
            this._selectLayerObjectToolStripMenuItem.Name = "_selectLayerObjectToolStripMenuItem";
            this._selectLayerObjectToolStripMenuItem.ShortcutKeyDisplayString = "";
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
            this._zoomInToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomInToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._zoomInToolStripMenuItem.Text = "Zoom in";
            // 
            // _zoomOutToolStripMenuItem
            // 
            this._zoomOutToolStripMenuItem.Enabled = false;
            this._zoomOutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomOutToolStripMenuItem.Image")));
            this._zoomOutToolStripMenuItem.ImageViewer = null;
            this._zoomOutToolStripMenuItem.Name = "_zoomOutToolStripMenuItem";
            this._zoomOutToolStripMenuItem.ShortcutKeyDisplayString = "";
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
            this._zoomPreviousToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomPreviousToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._zoomPreviousToolStripMenuItem.Text = "Zoom previous";
            // 
            // _zoomNextToolStripMenuItem
            // 
            this._zoomNextToolStripMenuItem.Enabled = false;
            this._zoomNextToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomNextToolStripMenuItem.Image")));
            this._zoomNextToolStripMenuItem.ImageViewer = null;
            this._zoomNextToolStripMenuItem.Name = "_zoomNextToolStripMenuItem";
            this._zoomNextToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomNextToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._zoomNextToolStripMenuItem.Text = "Zoom next";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(199, 6);
            // 
            // previousTileToolStripMenuItem1
            // 
            this.previousTileToolStripMenuItem1.Enabled = false;
            this.previousTileToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("previousTileToolStripMenuItem1.Image")));
            this.previousTileToolStripMenuItem1.ImageViewer = null;
            this.previousTileToolStripMenuItem1.Name = "previousTileToolStripMenuItem1";
            this.previousTileToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this.previousTileToolStripMenuItem1.Size = new System.Drawing.Size(202, 22);
            this.previousTileToolStripMenuItem1.Text = "Previous tile";
            // 
            // nextTileToolStripMenuItem1
            // 
            this.nextTileToolStripMenuItem1.Enabled = false;
            this.nextTileToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("nextTileToolStripMenuItem1.Image")));
            this.nextTileToolStripMenuItem1.ImageViewer = null;
            this.nextTileToolStripMenuItem1.Name = "nextTileToolStripMenuItem1";
            this.nextTileToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this.nextTileToolStripMenuItem1.Size = new System.Drawing.Size(202, 22);
            this.nextTileToolStripMenuItem1.Text = "Next tile";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(199, 6);
            // 
            // _fitToWidthToolStripMenuItem
            // 
            this._fitToWidthToolStripMenuItem.Enabled = false;
            this._fitToWidthToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_fitToWidthToolStripMenuItem.Image")));
            this._fitToWidthToolStripMenuItem.ImageViewer = null;
            this._fitToWidthToolStripMenuItem.Name = "_fitToWidthToolStripMenuItem";
            this._fitToWidthToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._fitToWidthToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._fitToWidthToolStripMenuItem.Text = "Fit to &width";
            // 
            // _fitToPageToolStripMenuItem
            // 
            this._fitToPageToolStripMenuItem.Enabled = false;
            this._fitToPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_fitToPageToolStripMenuItem.Image")));
            this._fitToPageToolStripMenuItem.ImageViewer = null;
            this._fitToPageToolStripMenuItem.Name = "_fitToPageToolStripMenuItem";
            this._fitToPageToolStripMenuItem.ShortcutKeyDisplayString = "";
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
            this._rotateCounterclockwiseToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._rotateCounterclockwiseToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._rotateCounterclockwiseToolStripMenuItem.Text = "Rotate counterclockwise";
            // 
            // _rotateClockwiseToolStripMenuItem
            // 
            this._rotateClockwiseToolStripMenuItem.Enabled = false;
            this._rotateClockwiseToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rotateClockwiseToolStripMenuItem.Image")));
            this._rotateClockwiseToolStripMenuItem.ImageViewer = null;
            this._rotateClockwiseToolStripMenuItem.Name = "_rotateClockwiseToolStripMenuItem";
            this._rotateClockwiseToolStripMenuItem.ShortcutKeyDisplayString = "";
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
            this._firstPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._firstPageToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._firstPageToolStripMenuItem.Text = "First page";
            // 
            // _previousPageToolStripMenuItem
            // 
            this._previousPageToolStripMenuItem.Enabled = false;
            this._previousPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_previousPageToolStripMenuItem.Image")));
            this._previousPageToolStripMenuItem.ImageViewer = null;
            this._previousPageToolStripMenuItem.Name = "_previousPageToolStripMenuItem";
            this._previousPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._previousPageToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._previousPageToolStripMenuItem.Text = "Previous page";
            // 
            // _nextPageToolStripMenuItem
            // 
            this._nextPageToolStripMenuItem.Enabled = false;
            this._nextPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_nextPageToolStripMenuItem.Image")));
            this._nextPageToolStripMenuItem.ImageViewer = null;
            this._nextPageToolStripMenuItem.Name = "_nextPageToolStripMenuItem";
            this._nextPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._nextPageToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._nextPageToolStripMenuItem.Text = "Next page";
            // 
            // _lastPageToolStripMenuItem
            // 
            this._lastPageToolStripMenuItem.Enabled = false;
            this._lastPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_lastPageToolStripMenuItem.Image")));
            this._lastPageToolStripMenuItem.ImageViewer = null;
            this._lastPageToolStripMenuItem.Name = "_lastPageToolStripMenuItem";
            this._lastPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._lastPageToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._lastPageToolStripMenuItem.Text = "Last page";
            // 
            // _basicTools
            // 
            this._basicTools.Dock = System.Windows.Forms.DockStyle.None;
            this._basicTools.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._basicTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoomWindowToolStripButton1,
            this.panToolStripButton1,
            this.highlightToolStripSplitButton1,
            this.selectLayerObjectToolStripButton1,
            this.toolStripSeparator11,
            this.editHighlightTextToolStripButton1});
            this._basicTools.Location = new System.Drawing.Point(3, 24);
            this._basicTools.Name = "_basicTools";
            this._basicTools.Size = new System.Drawing.Size(210, 39);
            this._basicTools.TabIndex = 5;
            // 
            // zoomWindowToolStripButton1
            // 
            this.zoomWindowToolStripButton1.BaseToolTipText = "Zoom window";
            this.zoomWindowToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomWindowToolStripButton1.Enabled = false;
            this.zoomWindowToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomWindowToolStripButton1.ImageViewer = null;
            this.zoomWindowToolStripButton1.Name = "zoomWindowToolStripButton1";
            this.zoomWindowToolStripButton1.Size = new System.Drawing.Size(36, 36);
            // 
            // panToolStripButton1
            // 
            this.panToolStripButton1.BaseToolTipText = "Pan";
            this.panToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.panToolStripButton1.Enabled = false;
            this.panToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.panToolStripButton1.ImageViewer = null;
            this.panToolStripButton1.Name = "panToolStripButton1";
            this.panToolStripButton1.Size = new System.Drawing.Size(36, 36);
            // 
            // highlightToolStripSplitButton1
            // 
            this.highlightToolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.highlightToolStripSplitButton1.Enabled = false;
            this.highlightToolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.highlightToolStripSplitButton1.ImageViewer = null;
            this.highlightToolStripSplitButton1.Name = "highlightToolStripSplitButton1";
            this.highlightToolStripSplitButton1.Size = new System.Drawing.Size(48, 36);
            this.highlightToolStripSplitButton1.Text = "Highlighter";
            this.highlightToolStripSplitButton1.ToolTipText = "Highlight new text";
            // 
            // selectLayerObjectToolStripButton1
            // 
            this.selectLayerObjectToolStripButton1.BaseToolTipText = "Select redactions and other objects";
            this.selectLayerObjectToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.selectLayerObjectToolStripButton1.Enabled = false;
            this.selectLayerObjectToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.selectLayerObjectToolStripButton1.ImageViewer = null;
            this.selectLayerObjectToolStripButton1.Name = "selectLayerObjectToolStripButton1";
            this.selectLayerObjectToolStripButton1.Size = new System.Drawing.Size(36, 36);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(6, 39);
            // 
            // editHighlightTextToolStripButton1
            // 
            this.editHighlightTextToolStripButton1.BaseToolTipText = "Edit highlight text";
            this.editHighlightTextToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.editHighlightTextToolStripButton1.Enabled = false;
            this.editHighlightTextToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.editHighlightTextToolStripButton1.ImageViewer = null;
            this.editHighlightTextToolStripButton1.Name = "editHighlightTextToolStripButton1";
            this.editHighlightTextToolStripButton1.Size = new System.Drawing.Size(36, 36);
            // 
            // _fileCommands
            // 
            this._fileCommands.Dock = System.Windows.Forms.DockStyle.None;
            this._fileCommands.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._fileCommands.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openImageToolStripSplitButton1,
            this._saveGddImageButton,
            this.printImageToolStripButton1});
            this._fileCommands.Location = new System.Drawing.Point(3, 63);
            this._fileCommands.Name = "_fileCommands";
            this._fileCommands.Size = new System.Drawing.Size(132, 39);
            this._fileCommands.TabIndex = 6;
            // 
            // openImageToolStripSplitButton1
            // 
            this.openImageToolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openImageToolStripSplitButton1.Enabled = false;
            this.openImageToolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openImageToolStripSplitButton1.ImageViewer = null;
            this.openImageToolStripSplitButton1.Name = "openImageToolStripSplitButton1";
            this.openImageToolStripSplitButton1.Size = new System.Drawing.Size(48, 36);
            this.openImageToolStripSplitButton1.Text = "openImageToolStripSplitButton1";
            this.openImageToolStripSplitButton1.ToolTipText = "Open image";
            // 
            // _saveGddImageButton
            // 
            this._saveGddImageButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._saveGddImageButton.Enabled = false;
            this._saveGddImageButton.Image = global::Extract.Imaging.Utilities.ExtractImageViewer.Properties.Resources.SaveAsGddFile;
            this._saveGddImageButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._saveGddImageButton.Name = "_saveGddImageButton";
            this._saveGddImageButton.Size = new System.Drawing.Size(36, 36);
            this._saveGddImageButton.Text = "Save Gdd Image";
            this._saveGddImageButton.ToolTipText = "Save highlights in a new image file";
            // 
            // printImageToolStripButton1
            // 
            this.printImageToolStripButton1.BaseToolTipText = "Print image";
            this.printImageToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.printImageToolStripButton1.Enabled = false;
            this.printImageToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.printImageToolStripButton1.ImageViewer = null;
            this.printImageToolStripButton1.Name = "printImageToolStripButton1";
            this.printImageToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.printImageToolStripButton1.Text = "Print image";
            // 
            // _navigationTools
            // 
            this._navigationTools.Dock = System.Windows.Forms.DockStyle.None;
            this._navigationTools.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._navigationTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.firstPageToolStripButton1,
            this.previousPageToolStripButton1,
            this.pageNavigationToolStripTextBox1,
            this.nextPageToolStripButton1,
            this.lastPageToolStripButton1});
            this._navigationTools.Location = new System.Drawing.Point(3, 102);
            this._navigationTools.Name = "_navigationTools";
            this._navigationTools.Size = new System.Drawing.Size(233, 39);
            this._navigationTools.TabIndex = 7;
            // 
            // firstPageToolStripButton1
            // 
            this.firstPageToolStripButton1.BaseToolTipText = "Go to first page";
            this.firstPageToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.firstPageToolStripButton1.Enabled = false;
            this.firstPageToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.firstPageToolStripButton1.ImageViewer = null;
            this.firstPageToolStripButton1.Name = "firstPageToolStripButton1";
            this.firstPageToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.firstPageToolStripButton1.Text = "firstPageToolStripButton1";
            // 
            // previousPageToolStripButton1
            // 
            this.previousPageToolStripButton1.BaseToolTipText = "Go to previous page";
            this.previousPageToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.previousPageToolStripButton1.Enabled = false;
            this.previousPageToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.previousPageToolStripButton1.ImageViewer = null;
            this.previousPageToolStripButton1.Name = "previousPageToolStripButton1";
            this.previousPageToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.previousPageToolStripButton1.Text = "previousPageToolStripButton1";
            // 
            // pageNavigationToolStripTextBox1
            // 
            this.pageNavigationToolStripTextBox1.Enabled = false;
            this.pageNavigationToolStripTextBox1.ImageViewer = null;
            this.pageNavigationToolStripTextBox1.Name = "pageNavigationToolStripTextBox1";
            this.pageNavigationToolStripTextBox1.Size = new System.Drawing.Size(75, 39);
            this.pageNavigationToolStripTextBox1.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // nextPageToolStripButton1
            // 
            this.nextPageToolStripButton1.BaseToolTipText = "Go to next page";
            this.nextPageToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.nextPageToolStripButton1.Enabled = false;
            this.nextPageToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.nextPageToolStripButton1.ImageViewer = null;
            this.nextPageToolStripButton1.Name = "nextPageToolStripButton1";
            this.nextPageToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.nextPageToolStripButton1.Text = "nextPageToolStripButton1";
            // 
            // lastPageToolStripButton1
            // 
            this.lastPageToolStripButton1.BaseToolTipText = "Go to last page";
            this.lastPageToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.lastPageToolStripButton1.Enabled = false;
            this.lastPageToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.lastPageToolStripButton1.ImageViewer = null;
            this.lastPageToolStripButton1.Name = "lastPageToolStripButton1";
            this.lastPageToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.lastPageToolStripButton1.Text = "lastPageToolStripButton1";
            // 
            // _viewCommands
            // 
            this._viewCommands.Dock = System.Windows.Forms.DockStyle.None;
            this._viewCommands.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._viewCommands.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoomInToolStripButton1,
            this.zoomOutToolStripButton1,
            this.zoomPreviousToolStripButton1,
            this.zoomNextToolStripButton1,
            this.toolStripSeparator8,
            this.fitToPageToolStripButton1,
            this.fitToWidthToolStripButton1,
            this.toolStripSeparator9,
            this.previousTileToolStripButton1,
            this.nextTileToolStripButton1,
            this.toolStripSeparator10,
            this.rotateCounterclockwiseToolStripButton1,
            this.rotateClockwiseToolStripButton1});
            this._viewCommands.Location = new System.Drawing.Point(3, 141);
            this._viewCommands.Name = "_viewCommands";
            this._viewCommands.Size = new System.Drawing.Size(390, 39);
            this._viewCommands.TabIndex = 8;
            // 
            // zoomInToolStripButton1
            // 
            this.zoomInToolStripButton1.BaseToolTipText = "Zoom in";
            this.zoomInToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomInToolStripButton1.Enabled = false;
            this.zoomInToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomInToolStripButton1.ImageViewer = null;
            this.zoomInToolStripButton1.Name = "zoomInToolStripButton1";
            this.zoomInToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.zoomInToolStripButton1.Text = "Zoom in";
            // 
            // zoomOutToolStripButton1
            // 
            this.zoomOutToolStripButton1.BaseToolTipText = "Zoom out";
            this.zoomOutToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomOutToolStripButton1.Enabled = false;
            this.zoomOutToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomOutToolStripButton1.ImageViewer = null;
            this.zoomOutToolStripButton1.Name = "zoomOutToolStripButton1";
            this.zoomOutToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.zoomOutToolStripButton1.Text = "Zoom out";
            // 
            // zoomPreviousToolStripButton1
            // 
            this.zoomPreviousToolStripButton1.BaseToolTipText = "Zoom previous";
            this.zoomPreviousToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomPreviousToolStripButton1.Enabled = false;
            this.zoomPreviousToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomPreviousToolStripButton1.ImageViewer = null;
            this.zoomPreviousToolStripButton1.Name = "zoomPreviousToolStripButton1";
            this.zoomPreviousToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.zoomPreviousToolStripButton1.Text = "Zoom previous";
            // 
            // zoomNextToolStripButton1
            // 
            this.zoomNextToolStripButton1.BaseToolTipText = "Zoom next";
            this.zoomNextToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.zoomNextToolStripButton1.Enabled = false;
            this.zoomNextToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.zoomNextToolStripButton1.ImageViewer = null;
            this.zoomNextToolStripButton1.Name = "zoomNextToolStripButton1";
            this.zoomNextToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.zoomNextToolStripButton1.Text = "Zoom next";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 39);
            // 
            // fitToPageToolStripButton1
            // 
            this.fitToPageToolStripButton1.BaseToolTipText = "Fit to page";
            this.fitToPageToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.fitToPageToolStripButton1.Enabled = false;
            this.fitToPageToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fitToPageToolStripButton1.ImageViewer = null;
            this.fitToPageToolStripButton1.Name = "fitToPageToolStripButton1";
            this.fitToPageToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.fitToPageToolStripButton1.Text = "Fit to page";
            // 
            // fitToWidthToolStripButton1
            // 
            this.fitToWidthToolStripButton1.BaseToolTipText = "Fit to width";
            this.fitToWidthToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.fitToWidthToolStripButton1.Enabled = false;
            this.fitToWidthToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fitToWidthToolStripButton1.ImageViewer = null;
            this.fitToWidthToolStripButton1.Name = "fitToWidthToolStripButton1";
            this.fitToWidthToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.fitToWidthToolStripButton1.Text = "Fit to width";
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 39);
            // 
            // previousTileToolStripButton1
            // 
            this.previousTileToolStripButton1.BaseToolTipText = "Previous tile";
            this.previousTileToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.previousTileToolStripButton1.Enabled = false;
            this.previousTileToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.previousTileToolStripButton1.ImageViewer = null;
            this.previousTileToolStripButton1.Name = "previousTileToolStripButton1";
            this.previousTileToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.previousTileToolStripButton1.Text = "previousTileToolStripButton1";
            // 
            // nextTileToolStripButton1
            // 
            this.nextTileToolStripButton1.BaseToolTipText = "Next tile";
            this.nextTileToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.nextTileToolStripButton1.Enabled = false;
            this.nextTileToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.nextTileToolStripButton1.ImageViewer = null;
            this.nextTileToolStripButton1.Name = "nextTileToolStripButton1";
            this.nextTileToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.nextTileToolStripButton1.Text = "nextTileToolStripButton1";
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(6, 39);
            // 
            // rotateCounterclockwiseToolStripButton1
            // 
            this.rotateCounterclockwiseToolStripButton1.BaseToolTipText = "Rotate counterclockwise";
            this.rotateCounterclockwiseToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rotateCounterclockwiseToolStripButton1.Enabled = false;
            this.rotateCounterclockwiseToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.rotateCounterclockwiseToolStripButton1.ImageViewer = null;
            this.rotateCounterclockwiseToolStripButton1.Name = "rotateCounterclockwiseToolStripButton1";
            this.rotateCounterclockwiseToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.rotateCounterclockwiseToolStripButton1.Text = "Rotate counterclockwise";
            // 
            // rotateClockwiseToolStripButton1
            // 
            this.rotateClockwiseToolStripButton1.BaseToolTipText = "Rotate clockwise";
            this.rotateClockwiseToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rotateClockwiseToolStripButton1.Enabled = false;
            this.rotateClockwiseToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.rotateClockwiseToolStripButton1.ImageViewer = null;
            this.rotateClockwiseToolStripButton1.Name = "rotateClockwiseToolStripButton1";
            this.rotateClockwiseToolStripButton1.Size = new System.Drawing.Size(36, 36);
            this.rotateClockwiseToolStripButton1.Text = "Rotate clockwise";
            // 
            // _imageViewerStatusStrip
            // 
            this._imageViewerStatusStrip.Location = new System.Drawing.Point(0, 444);
            this._imageViewerStatusStrip.Name = "_imageViewerStatusStrip";
            this._imageViewerStatusStrip.Size = new System.Drawing.Size(742, 22);
            this._imageViewerStatusStrip.TabIndex = 2;
            this._imageViewerStatusStrip.Text = "_imageViewerStatusStrip";
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
            this._basicTools.ResumeLayout(false);
            this._basicTools.PerformLayout();
            this._fileCommands.ResumeLayout(false);
            this._fileCommands.PerformLayout();
            this._navigationTools.ResumeLayout(false);
            this._navigationTools.PerformLayout();
            this._viewCommands.ResumeLayout(false);
            this._viewCommands.PerformLayout();
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
        private Extract.Imaging.Forms.ImageViewerContextMenuStrip _imageViewerContextMenuStrip;
        private Extract.Imaging.Forms.ImageViewerStatusStrip _imageViewerStatusStrip;
        private Extract.Imaging.Forms.OpenImageToolStripMenuItem _openImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private Extract.Imaging.Forms.PrintImageToolStripMenuItem _printImageToolStripMenuItem;
        private Extract.Imaging.Forms.ZoomWindowToolStripMenuItem _zoomWindowToolStripMenuItem;
        private Extract.Imaging.Forms.PanToolStripMenuItem _panToolStripMenuItem;
        private Extract.Imaging.Forms.HighlightToolStripMenuItem _highlightToolStripMenuItem;
        private Extract.Imaging.Forms.SelectLayerObjectToolStripMenuItem _selectLayerObjectToolStripMenuItem;
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
        private Extract.Imaging.Forms.ZoomWindowToolStripButton zoomWindowToolStripButton1;
        private Extract.Imaging.Forms.PanToolStripButton panToolStripButton1;
        private Extract.Imaging.Forms.HighlightToolStripSplitButton highlightToolStripSplitButton1;
        private Extract.Imaging.Forms.SelectLayerObjectToolStripButton selectLayerObjectToolStripButton1;
        private System.Windows.Forms.ToolStrip _fileCommands;
        private Extract.Imaging.Forms.OpenImageToolStripSplitButton openImageToolStripSplitButton1;
        private System.Windows.Forms.ToolStripButton _saveGddImageButton;
        private Extract.Imaging.Forms.PrintImageToolStripButton printImageToolStripButton1;
        private System.Windows.Forms.ToolStrip _navigationTools;
        private Extract.Imaging.Forms.FirstPageToolStripButton firstPageToolStripButton1;
        private Extract.Imaging.Forms.PreviousPageToolStripButton previousPageToolStripButton1;
        private System.Windows.Forms.ToolStrip _viewCommands;
        private Extract.Imaging.Forms.PageNavigationToolStripTextBox pageNavigationToolStripTextBox1;
        private Extract.Imaging.Forms.NextPageToolStripButton nextPageToolStripButton1;
        private Extract.Imaging.Forms.LastPageToolStripButton lastPageToolStripButton1;
        private Extract.Imaging.Forms.ZoomInToolStripButton zoomInToolStripButton1;
        private Extract.Imaging.Forms.ZoomOutToolStripButton zoomOutToolStripButton1;
        private Extract.Imaging.Forms.ZoomPreviousToolStripButton zoomPreviousToolStripButton1;
        private Extract.Imaging.Forms.ZoomNextToolStripButton zoomNextToolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private Extract.Imaging.Forms.FitToPageToolStripButton fitToPageToolStripButton1;
        private Extract.Imaging.Forms.FitToWidthToolStripButton fitToWidthToolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private Extract.Imaging.Forms.PreviousTileToolStripButton previousTileToolStripButton1;
        private Extract.Imaging.Forms.NextTileToolStripButton nextTileToolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton rotateCounterclockwiseToolStripButton1;
        private Extract.Imaging.Forms.RotateClockwiseToolStripButton rotateClockwiseToolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private Extract.Imaging.Forms.EditHighlightTextToolStripButton editHighlightTextToolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem _searchForImagesToolStripMenuItem;
    }
}

