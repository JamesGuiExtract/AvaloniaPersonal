namespace IDShieldOffice
{
    partial class IDShieldOfficeForm
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
            if (disposing)
            {
                if(components != null)
                {
                    components.Dispose();
                }

                // Dispose of the OCR manager
                if (_ocrManager != null)
                {
                    _ocrManager.Dispose();
                }

                // Dispose of the bracketed text rule form
                if (_bracketedTextRuleForm != null)
                {
                    _bracketedTextRuleForm.Dispose();
                }

                // Dispose of the word or pattern list rule form
                if (_wordOrPatternListRuleForm != null)
                {
                    _wordOrPatternListRuleForm.Dispose();
                }

                // Dispose of the data type rule form
                if (_dataTypeRuleForm != null)
                {
                    _dataTypeRuleForm.Dispose();
                }

                // Dispose of the user preferences
                if (_userPreferences != null)
                {
                    _userPreferences.Dispose();

                }

                // Dispose of the user preferences dialog
                if (_userPreferencesDialog != null)
                {
                    _userPreferencesDialog.Dispose();
                }
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
            Leadtools.WinForms.RasterMagnifyGlass rasterMagnifyGlass1 = new Leadtools.WinForms.RasterMagnifyGlass();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IDShieldOfficeForm));
            TD.SandDock.DockingRules dockingRules1 = new TD.SandDock.DockingRules();
            TD.SandDock.DockingRules dockingRules2 = new TD.SandDock.DockingRules();
            this._statusStrip = new System.Windows.Forms.StatusStrip();
            this._userActionToolStripStatusLabel = new Extract.Imaging.Forms.UserActionToolStripStatusLabel();
            this._ocrProgressStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._resolutionToolStripStatusLabel = new Extract.Imaging.Forms.ResolutionToolStripStatusLabel();
            this._mousePositionToolStripStatusLabel = new Extract.Imaging.Forms.MousePositionToolStripStatusLabel();
            this._toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this._imageViewer = new Extract.Imaging.Forms.ImageViewer();
            this._idsoContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectLayerObjectToolStripMenuItem1 = new Extract.Imaging.Forms.SelectLayerObjectToolStripMenuItem();
            this.zoomWindowToolStripMenuItem1 = new Extract.Imaging.Forms.ZoomWindowToolStripMenuItem();
            this.panToolStripMenuItem1 = new Extract.Imaging.Forms.PanToolStripMenuItem();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.zoomPreviousToolStripMenuItem1 = new Extract.Imaging.Forms.ZoomPreviousToolStripMenuItem();
            this.zoomNextToolStripMenuItem1 = new Extract.Imaging.Forms.ZoomNextToolStripMenuItem();
            this.toolStripSeparator21 = new System.Windows.Forms.ToolStripSeparator();
            this.redactionToolStripMenuItem1 = new Extract.Imaging.Forms.RedactionToolStripMenuItem();
            this._dockContainer1 = new TD.SandDock.DockContainer();
            this._objectPropertyGridDockableWindow = new TD.SandDock.DockableWindow();
            this._objectPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this._layersDockableWindow = new TD.SandDock.DockableWindow();
            this._layersViewPanel = new System.Windows.Forms.Panel();
            this._showRedactionsCheckBox = new System.Windows.Forms.CheckBox();
            this._showCluesCheckBox = new System.Windows.Forms.CheckBox();
            this._showBatesNumberCheckBox = new System.Windows.Forms.CheckBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._checkAllLayersToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._uncheckAllLayersToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._sandDockManager1 = new TD.SandDock.SandDockManager();
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this._fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openImageToolStripMenuItem1 = new Extract.Imaging.Forms.OpenImageToolStripMenuItem();
            this._closeImageToolStripMenuItem = new Extract.Imaging.Forms.CloseImageToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this._saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this._pageSetupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._previewToolStripMenuItem = new Extract.Imaging.Forms.PrintPreviewToolStripMenuItem();
            this._printImageToolStripMenuItem = new Extract.Imaging.Forms.PrintImageToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this._propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this._exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._selectAllToolStripMenuItem = new Extract.Imaging.Forms.SelectAllLayerObjectsToolStripMenuItem();
            this._deleteSelectionToolStripMenuItem = new Extract.Imaging.Forms.DeleteSelectionToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this._findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this._redactEntirePageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this._preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._fitToPageToolStripMenuItem = new Extract.Imaging.Forms.FitToPageToolStripMenuItem();
            this._fitToWidthToolStripMenuItem = new Extract.Imaging.Forms.FitToWidthToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomInToolStripMenuItem = new Extract.Imaging.Forms.ZoomInToolStripMenuItem();
            this._zoomOutToolStripMenuItem = new Extract.Imaging.Forms.ZoomOutToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomPreviousToolStripMenuItem = new Extract.Imaging.Forms.ZoomPreviousToolStripMenuItem();
            this._zoomNextToolStripMenuItem = new Extract.Imaging.Forms.ZoomNextToolStripMenuItem();
            this._rotateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._rotateCounterclockwiseToolStripMenuItem = new Extract.Imaging.Forms.RotateCounterclockwiseToolStripMenuItem();
            this._rotateClockwiseToolStripMenuItem = new Extract.Imaging.Forms.RotateClockwiseToolStripMenuItem();
            this._gotoPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._firstPageToolStripMenuItem = new Extract.Imaging.Forms.FirstPageToolStripMenuItem();
            this._previousPageToolStripMenuItem = new Extract.Imaging.Forms.PreviousPageToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this._pageNavigationToolStripMenuItem = new Extract.Imaging.Forms.PageNavigationToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this._nextPageToolStripMenuItem = new Extract.Imaging.Forms.NextPageToolStripMenuItem();
            this._lastPageToolStripMenuItem = new Extract.Imaging.Forms.LastPageToolStripMenuItem();
            this._tilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._previousTileToolStripMenuItem = new Extract.Imaging.Forms.PreviousTileToolStripMenuItem();
            this._nextTileToolStripMenuItem = new Extract.Imaging.Forms.NextTileToolStripMenuItem();
            this._redactionObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._previousLayerObjectToolStripMenuItem = new Extract.Imaging.Forms.PreviousLayerObjectToolStripMenuItem();
            this._nextLayerObjectToolStripMenuItem = new Extract.Imaging.Forms.NextLayerObjectToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this._showLayersWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._showObjectPropertiesGridWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._selectLayerObjectToolStripMenuItem = new Extract.Imaging.Forms.SelectLayerObjectToolStripMenuItem();
            this._panToolStripMenuItem = new Extract.Imaging.Forms.PanToolStripMenuItem();
            this._zoomWindowToolStripMenuItem = new Extract.Imaging.Forms.ZoomWindowToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this._angularRedactionToolStripMenuItem = new Extract.Imaging.Forms.AngularRedactionToolStripMenuItem();
            this._rectangularRedactionToolStripMenuItem = new Extract.Imaging.Forms.RectangularRedactionToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this._findOrRedactToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._bracketedTextFinderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._dataTypesFinderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._wordListFinderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this._applyBatesNumberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._idShieldOfficeHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._regularExpressionHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._aboutIDShieldOfficeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._fileCommandsToolStrip = new System.Windows.Forms.ToolStrip();
            this._openImageToolStripSplitButton = new Extract.Imaging.Forms.OpenImageToolStripSplitButton();
            this._saveToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._printImageToolStripButton = new Extract.Imaging.Forms.PrintImageToolStripButton();
            this._basicToolsToolStrip = new System.Windows.Forms.ToolStrip();
            this._selectLayerObjectToolStripButton = new Extract.Imaging.Forms.SelectLayerObjectToolStripButton();
            this._dataTypesFinderToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._bracketedTextFinderToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._wordListFinderToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._applyBatesNumberToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._deleteSelectionToolStripButton = new Extract.Imaging.Forms.DeleteSelectionToolStripButton();
            this._viewCommandsToolStrip = new System.Windows.Forms.ToolStrip();
            this._fitToPageToolStripButton = new Extract.Imaging.Forms.FitToPageToolStripButton();
            this._fitToWidthToolStripButton = new Extract.Imaging.Forms.FitToWidthToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomWindowToolStripButton = new Extract.Imaging.Forms.ZoomWindowToolStripButton();
            this._panToolStripButton = new Extract.Imaging.Forms.PanToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomInToolStripButton = new Extract.Imaging.Forms.ZoomInToolStripButton();
            this._zoomOutToolStripButton = new Extract.Imaging.Forms.ZoomOutToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomPreviousToolStripButton = new Extract.Imaging.Forms.ZoomPreviousToolStripButton();
            this._zoomNextToolStripButton = new Extract.Imaging.Forms.ZoomNextToolStripButton();
            this._layerNavigationToolStrip = new System.Windows.Forms.ToolStrip();
            this._previousTileToolStripButton = new Extract.Imaging.Forms.PreviousTileToolStripButton();
            this._nextTileToolStripButton = new Extract.Imaging.Forms.NextTileToolStripButton();
            this._previousLayerObjectToolStripButton = new Extract.Imaging.Forms.PreviousLayerObjectToolStripButton();
            this._nextLayerObjectToolStripButton = new Extract.Imaging.Forms.NextLayerObjectToolStripButton();
            this._pageNavigationToolStrip = new System.Windows.Forms.ToolStrip();
            this._firstPageToolStripButton = new Extract.Imaging.Forms.FirstPageToolStripButton();
            this._previousPageToolStripButton = new Extract.Imaging.Forms.PreviousPageToolStripButton();
            this._pageNavigationToolStripTextBox = new Extract.Imaging.Forms.PageNavigationToolStripTextBox();
            this._nextPageToolStripButton = new Extract.Imaging.Forms.NextPageToolStripButton();
            this._lastPageToolStripButton = new Extract.Imaging.Forms.LastPageToolStripButton();
            this._dockableWindowsToolStrip = new System.Windows.Forms.ToolStrip();
            this._showObjectPropertyGridWindowToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._showLayersWindowToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._rotateCommandsToolStrip = new System.Windows.Forms.ToolStrip();
            this._rotateCounterclockwiseToolStripButton = new Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton();
            this._rotateClockwiseToolStripButton = new Extract.Imaging.Forms.RotateClockwiseToolStripButton();
            this._rectangularRedactionToolStripButton = new Extract.Imaging.Forms.RectangularRedactionToolStripButton();
            this._angularRedactionToolStripButton = new Extract.Imaging.Forms.AngularRedactionToolStripButton();
            this._statusStrip.SuspendLayout();
            this._toolStripContainer.BottomToolStripPanel.SuspendLayout();
            this._toolStripContainer.ContentPanel.SuspendLayout();
            this._toolStripContainer.TopToolStripPanel.SuspendLayout();
            this._toolStripContainer.SuspendLayout();
            this._idsoContextMenu.SuspendLayout();
            this._dockContainer1.SuspendLayout();
            this._objectPropertyGridDockableWindow.SuspendLayout();
            this._layersDockableWindow.SuspendLayout();
            this._layersViewPanel.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this._menuStrip.SuspendLayout();
            this._fileCommandsToolStrip.SuspendLayout();
            this._basicToolsToolStrip.SuspendLayout();
            this._viewCommandsToolStrip.SuspendLayout();
            this._layerNavigationToolStrip.SuspendLayout();
            this._pageNavigationToolStrip.SuspendLayout();
            this._dockableWindowsToolStrip.SuspendLayout();
            this._rotateCommandsToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _statusStrip
            // 
            this._statusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._userActionToolStripStatusLabel,
            this._ocrProgressStatusLabel,
            this._resolutionToolStripStatusLabel,
            this._mousePositionToolStripStatusLabel});
            this._statusStrip.Location = new System.Drawing.Point(0, 0);
            this._statusStrip.Name = "_statusStrip";
            this._statusStrip.ShowItemToolTips = true;
            this._statusStrip.Size = new System.Drawing.Size(751, 22);
            this._statusStrip.TabIndex = 0;
            this._statusStrip.Text = "statusStrip1";
            // 
            // _userActionToolStripStatusLabel
            // 
            this._userActionToolStripStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this._userActionToolStripStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this._userActionToolStripStatusLabel.ImageViewer = null;
            this._userActionToolStripStatusLabel.Name = "_userActionToolStripStatusLabel";
            this._userActionToolStripStatusLabel.Size = new System.Drawing.Size(326, 17);
            this._userActionToolStripStatusLabel.Spring = true;
            this._userActionToolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _ocrProgressStatusLabel
            // 
            this._ocrProgressStatusLabel.AutoSize = false;
            this._ocrProgressStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this._ocrProgressStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this._ocrProgressStatusLabel.DoubleClickEnabled = true;
            this._ocrProgressStatusLabel.Name = "_ocrProgressStatusLabel";
            this._ocrProgressStatusLabel.Size = new System.Drawing.Size(115, 17);
            this._ocrProgressStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._ocrProgressStatusLabel.DoubleClick += new System.EventHandler(this.ocrProgressStatusLabel_DoubleClick);
            // 
            // _resolutionToolStripStatusLabel
            // 
            this._resolutionToolStripStatusLabel.AutoSize = false;
            this._resolutionToolStripStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this._resolutionToolStripStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this._resolutionToolStripStatusLabel.ImageViewer = null;
            this._resolutionToolStripStatusLabel.Name = "_resolutionToolStripStatusLabel";
            this._resolutionToolStripStatusLabel.Size = new System.Drawing.Size(120, 17);
            this._resolutionToolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _mousePositionToolStripStatusLabel
            // 
            this._mousePositionToolStripStatusLabel.AutoSize = false;
            this._mousePositionToolStripStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this._mousePositionToolStripStatusLabel.DisplayOption = Extract.Imaging.Forms.MousePositionDisplayOption.Registry;
            this._mousePositionToolStripStatusLabel.ImageViewer = null;
            this._mousePositionToolStripStatusLabel.Name = "_mousePositionToolStripStatusLabel";
            this._mousePositionToolStripStatusLabel.Size = new System.Drawing.Size(175, 17);
            this._mousePositionToolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _toolStripContainer
            // 
            // 
            // _toolStripContainer.BottomToolStripPanel
            // 
            this._toolStripContainer.BottomToolStripPanel.Controls.Add(this._statusStrip);
            // 
            // _toolStripContainer.ContentPanel
            // 
            this._toolStripContainer.ContentPanel.Controls.Add(this._imageViewer);
            this._toolStripContainer.ContentPanel.Controls.Add(this._dockContainer1);
            this._toolStripContainer.ContentPanel.Size = new System.Drawing.Size(751, 442);
            this._toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._toolStripContainer.Location = new System.Drawing.Point(0, 0);
            this._toolStripContainer.Name = "_toolStripContainer";
            this._toolStripContainer.Size = new System.Drawing.Size(751, 566);
            this._toolStripContainer.TabIndex = 1;
            this._toolStripContainer.Text = "toolStripContainer1";
            // 
            // _toolStripContainer.TopToolStripPanel
            // 
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._menuStrip);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._fileCommandsToolStrip);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._basicToolsToolStrip);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._viewCommandsToolStrip);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._layerNavigationToolStrip);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._pageNavigationToolStrip);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._dockableWindowsToolStrip);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._rotateCommandsToolStrip);
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
            this._imageViewer.ContextMenuStrip = this._idsoContextMenu;
            this._imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._imageViewer.DoubleBuffer = true;
            this._imageViewer.EnableScrollingInterface = false;
            this._imageViewer.EnableTimer = false;
            this._imageViewer.FloaterImage = null;
            this._imageViewer.FloaterPosition = new System.Drawing.Point(0, 0);
            this._imageViewer.FloaterVisible = true;
            this._imageViewer.FrameColor = System.Drawing.Color.Black;
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
            this._imageViewer.Size = new System.Drawing.Size(547, 442);
            this._imageViewer.SmallScrollChangeRatio = 20;
            this._imageViewer.SourceRectangle = new System.Drawing.Rectangle(0, 0, 0, 0);
            this._imageViewer.TabIndex = 1;
            this._imageViewer.Text = "imageViewer1";
            this._imageViewer.UseDefaultShortcuts = true;
            this._imageViewer.UseDpi = false;
            this._imageViewer.VerticalAlignMode = Leadtools.RasterPaintAlignMode.Near;
            this._imageViewer.Watermark = null;
            this._imageViewer.ImageFileClosing += new System.EventHandler<Extract.Imaging.Forms.ImageFileClosingEventArgs>(this.HandleImageViewerImageFileClosing);
            this._imageViewer.LoadingNewImage += new System.EventHandler<Extract.Imaging.Forms.LoadingNewImageEventArgs>(this.HandleImageViewerLoadingNewImage);
            this._imageViewer.DisplayingPrintDialog += new System.EventHandler<Extract.Imaging.Forms.DisplayingPrintDialogEventArgs>(this.HandleImageViewerDisplayingPrintDialog);
            this._imageViewer.ImageFileChanged += new System.EventHandler<Extract.Imaging.Forms.ImageFileChangedEventArgs>(this.HandleImageViewerImageFileChanged);
            this._imageViewer.CursorToolChanged += new System.EventHandler<Extract.Imaging.Forms.CursorToolChangedEventArgs>(this.HandleImageViewerCursorToolChanged);
            this._imageViewer.PageChanged += new System.EventHandler<Extract.Imaging.Forms.PageChangedEventArgs>(this.HandleImageViewerPageChanged);
            this._imageViewer.OpeningImage += new System.EventHandler<Extract.Imaging.Forms.OpeningImageEventArgs>(this.HandleImageViewerOpeningImage);
            this._imageViewer.FileOpenError += new System.EventHandler<Extract.Imaging.Forms.FileOpenErrorEventArgs>(this.HandleFileOpenError);
            // 
            // _idsoContextMenu
            // 
            this._idsoContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectLayerObjectToolStripMenuItem1,
            this.zoomWindowToolStripMenuItem1,
            this.panToolStripMenuItem1,
            this.toolStripSeparator20,
            this.zoomPreviousToolStripMenuItem1,
            this.zoomNextToolStripMenuItem1,
            this.toolStripSeparator21,
            this.redactionToolStripMenuItem1});
            this._idsoContextMenu.Name = "_idsoContextMenu";
            this._idsoContextMenu.Size = new System.Drawing.Size(256, 148);
            // 
            // selectLayerObjectToolStripMenuItem1
            // 
            this.selectLayerObjectToolStripMenuItem1.Enabled = false;
            this.selectLayerObjectToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("selectLayerObjectToolStripMenuItem1.Image")));
            this.selectLayerObjectToolStripMenuItem1.ImageViewer = null;
            this.selectLayerObjectToolStripMenuItem1.Name = "selectLayerObjectToolStripMenuItem1";
            this.selectLayerObjectToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this.selectLayerObjectToolStripMenuItem1.Size = new System.Drawing.Size(255, 22);
            this.selectLayerObjectToolStripMenuItem1.Text = "Select redactions and other objects";
            // 
            // zoomWindowToolStripMenuItem1
            // 
            this.zoomWindowToolStripMenuItem1.Enabled = false;
            this.zoomWindowToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("zoomWindowToolStripMenuItem1.Image")));
            this.zoomWindowToolStripMenuItem1.ImageViewer = null;
            this.zoomWindowToolStripMenuItem1.Name = "zoomWindowToolStripMenuItem1";
            this.zoomWindowToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this.zoomWindowToolStripMenuItem1.Size = new System.Drawing.Size(255, 22);
            this.zoomWindowToolStripMenuItem1.Text = "&Zoom window";
            // 
            // panToolStripMenuItem1
            // 
            this.panToolStripMenuItem1.Enabled = false;
            this.panToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("panToolStripMenuItem1.Image")));
            this.panToolStripMenuItem1.ImageViewer = null;
            this.panToolStripMenuItem1.Name = "panToolStripMenuItem1";
            this.panToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this.panToolStripMenuItem1.Size = new System.Drawing.Size(255, 22);
            this.panToolStripMenuItem1.Text = "P&an";
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(252, 6);
            // 
            // zoomPreviousToolStripMenuItem1
            // 
            this.zoomPreviousToolStripMenuItem1.Enabled = false;
            this.zoomPreviousToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("zoomPreviousToolStripMenuItem1.Image")));
            this.zoomPreviousToolStripMenuItem1.ImageViewer = null;
            this.zoomPreviousToolStripMenuItem1.Name = "zoomPreviousToolStripMenuItem1";
            this.zoomPreviousToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this.zoomPreviousToolStripMenuItem1.Size = new System.Drawing.Size(255, 22);
            this.zoomPreviousToolStripMenuItem1.Text = "Zoom previous";
            // 
            // zoomNextToolStripMenuItem1
            // 
            this.zoomNextToolStripMenuItem1.Enabled = false;
            this.zoomNextToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("zoomNextToolStripMenuItem1.Image")));
            this.zoomNextToolStripMenuItem1.ImageViewer = null;
            this.zoomNextToolStripMenuItem1.Name = "zoomNextToolStripMenuItem1";
            this.zoomNextToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this.zoomNextToolStripMenuItem1.Size = new System.Drawing.Size(255, 22);
            this.zoomNextToolStripMenuItem1.Text = "Zoom next";
            // 
            // toolStripSeparator21
            // 
            this.toolStripSeparator21.Name = "toolStripSeparator21";
            this.toolStripSeparator21.Size = new System.Drawing.Size(252, 6);
            // 
            // redactionToolStripMenuItem1
            // 
            this.redactionToolStripMenuItem1.Enabled = false;
            this.redactionToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("redactionToolStripMenuItem1.Image")));
            this.redactionToolStripMenuItem1.ImageViewer = null;
            this.redactionToolStripMenuItem1.Name = "redactionToolStripMenuItem1";
            this.redactionToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this.redactionToolStripMenuItem1.Size = new System.Drawing.Size(255, 22);
            this.redactionToolStripMenuItem1.Text = "Redaction";
            this.redactionToolStripMenuItem1.ToolTipText = "Redact new text";
            // 
            // _dockContainer1
            // 
            this._dockContainer1.ContentSize = 200;
            this._dockContainer1.Controls.Add(this._objectPropertyGridDockableWindow);
            this._dockContainer1.Controls.Add(this._layersDockableWindow);
            this._dockContainer1.Dock = System.Windows.Forms.DockStyle.Right;
            this._dockContainer1.LayoutSystem = new TD.SandDock.SplitLayoutSystem(new System.Drawing.SizeF(250F, 400F), System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 400F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._objectPropertyGridDockableWindow))}, this._objectPropertyGridDockableWindow))),
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 400F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._layersDockableWindow))}, this._layersDockableWindow)))});
            this._dockContainer1.Location = new System.Drawing.Point(547, 0);
            this._dockContainer1.Manager = this._sandDockManager1;
            this._dockContainer1.Name = "_dockContainer1";
            this._dockContainer1.Size = new System.Drawing.Size(204, 442);
            this._dockContainer1.TabIndex = 0;
            // 
            // _objectPropertyGridDockableWindow
            // 
            this._objectPropertyGridDockableWindow.Controls.Add(this._objectPropertyGrid);
            dockingRules1.AllowDockBottom = true;
            dockingRules1.AllowDockLeft = true;
            dockingRules1.AllowDockRight = true;
            dockingRules1.AllowDockTop = true;
            dockingRules1.AllowFloat = false;
            dockingRules1.AllowTab = false;
            this._objectPropertyGridDockableWindow.DockingRules = dockingRules1;
            this._objectPropertyGridDockableWindow.Guid = new System.Guid("e8b225b0-6f54-414c-b0b9-c14f0d082a58");
            this._objectPropertyGridDockableWindow.Location = new System.Drawing.Point(4, 25);
            this._objectPropertyGridDockableWindow.Name = "_objectPropertyGridDockableWindow";
            this._objectPropertyGridDockableWindow.Size = new System.Drawing.Size(200, 171);
            this._objectPropertyGridDockableWindow.TabIndex = 0;
            this._objectPropertyGridDockableWindow.Text = "Properties";
            this._objectPropertyGridDockableWindow.Closing += new TD.SandDock.DockControlClosingEventHandler(this.objectPropertyGridDockableWindow_Closing);
            // 
            // _objectPropertyGrid
            // 
            this._objectPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._objectPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this._objectPropertyGrid.Name = "_objectPropertyGrid";
            this._objectPropertyGrid.Size = new System.Drawing.Size(200, 171);
            this._objectPropertyGrid.TabIndex = 0;
            this._objectPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.HandleObjectPropertyGridPropertyValueChanged);
            // 
            // _layersDockableWindow
            // 
            this._layersDockableWindow.Controls.Add(this._layersViewPanel);
            this._layersDockableWindow.Controls.Add(this.toolStrip1);
            dockingRules2.AllowDockBottom = true;
            dockingRules2.AllowDockLeft = true;
            dockingRules2.AllowDockRight = true;
            dockingRules2.AllowDockTop = true;
            dockingRules2.AllowFloat = false;
            dockingRules2.AllowTab = false;
            this._layersDockableWindow.DockingRules = dockingRules2;
            this._layersDockableWindow.Guid = new System.Guid("4ddc5e84-047c-4194-acb9-73bdbfbdd21f");
            this._layersDockableWindow.Location = new System.Drawing.Point(4, 248);
            this._layersDockableWindow.Name = "_layersDockableWindow";
            this._layersDockableWindow.Size = new System.Drawing.Size(200, 171);
            this._layersDockableWindow.TabIndex = 1;
            this._layersDockableWindow.Text = "Layers";
            this._layersDockableWindow.Closing += new TD.SandDock.DockControlClosingEventHandler(this.layersDockableWindow_Closing);
            // 
            // _layersViewPanel
            // 
            this._layersViewPanel.Controls.Add(this._showRedactionsCheckBox);
            this._layersViewPanel.Controls.Add(this._showCluesCheckBox);
            this._layersViewPanel.Controls.Add(this._showBatesNumberCheckBox);
            this._layersViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._layersViewPanel.Location = new System.Drawing.Point(0, 39);
            this._layersViewPanel.Name = "_layersViewPanel";
            this._layersViewPanel.Padding = new System.Windows.Forms.Padding(5);
            this._layersViewPanel.Size = new System.Drawing.Size(200, 132);
            this._layersViewPanel.TabIndex = 1;
            // 
            // _showRedactionsCheckBox
            // 
            this._showRedactionsCheckBox.AutoSize = true;
            this._showRedactionsCheckBox.Checked = true;
            this._showRedactionsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._showRedactionsCheckBox.Location = new System.Drawing.Point(8, 54);
            this._showRedactionsCheckBox.Name = "_showRedactionsCheckBox";
            this._showRedactionsCheckBox.Size = new System.Drawing.Size(80, 17);
            this._showRedactionsCheckBox.TabIndex = 2;
            this._showRedactionsCheckBox.Text = "Redactions";
            this._showRedactionsCheckBox.UseVisualStyleBackColor = true;
            this._showRedactionsCheckBox.CheckedChanged += new System.EventHandler(this.HandleShowRedactionsCheckboxChanged);
            // 
            // _showCluesCheckBox
            // 
            this._showCluesCheckBox.AutoSize = true;
            this._showCluesCheckBox.Checked = true;
            this._showCluesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._showCluesCheckBox.Location = new System.Drawing.Point(8, 31);
            this._showCluesCheckBox.Name = "_showCluesCheckBox";
            this._showCluesCheckBox.Size = new System.Drawing.Size(52, 17);
            this._showCluesCheckBox.TabIndex = 1;
            this._showCluesCheckBox.Text = "Clues";
            this._showCluesCheckBox.UseVisualStyleBackColor = true;
            this._showCluesCheckBox.CheckedChanged += new System.EventHandler(this.HandleShowCluesCheckboxChanged);
            // 
            // _showBatesNumberCheckBox
            // 
            this._showBatesNumberCheckBox.AutoSize = true;
            this._showBatesNumberCheckBox.Checked = true;
            this._showBatesNumberCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._showBatesNumberCheckBox.Location = new System.Drawing.Point(8, 8);
            this._showBatesNumberCheckBox.Name = "_showBatesNumberCheckBox";
            this._showBatesNumberCheckBox.Size = new System.Drawing.Size(96, 17);
            this._showBatesNumberCheckBox.TabIndex = 0;
            this._showBatesNumberCheckBox.Text = "Bates numbers";
            this._showBatesNumberCheckBox.UseVisualStyleBackColor = true;
            this._showBatesNumberCheckBox.CheckedChanged += new System.EventHandler(this.HandleShowBatesNumbersCheckboxChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._checkAllLayersToolStripButton,
            this._uncheckAllLayersToolStripButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(200, 39);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // _checkAllLayersToolStripButton
            // 
            this._checkAllLayersToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._checkAllLayersToolStripButton.Image = global::IDShieldOffice.Properties.Resources.CheckAllButton;
            this._checkAllLayersToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._checkAllLayersToolStripButton.Name = "_checkAllLayersToolStripButton";
            this._checkAllLayersToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._checkAllLayersToolStripButton.Text = "Check all";
            this._checkAllLayersToolStripButton.Click += new System.EventHandler(this.HandleCheckOrUncheckAllLayerObjects);
            // 
            // _uncheckAllLayersToolStripButton
            // 
            this._uncheckAllLayersToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._uncheckAllLayersToolStripButton.Image = global::IDShieldOffice.Properties.Resources.UncheckAllButton;
            this._uncheckAllLayersToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._uncheckAllLayersToolStripButton.Name = "_uncheckAllLayersToolStripButton";
            this._uncheckAllLayersToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._uncheckAllLayersToolStripButton.Text = "Uncheck all";
            this._uncheckAllLayersToolStripButton.Click += new System.EventHandler(this.HandleCheckOrUncheckAllLayerObjects);
            // 
            // _sandDockManager1
            // 
            this._sandDockManager1.AutoSaveLayout = true;
            this._sandDockManager1.DockSystemContainer = this._toolStripContainer.ContentPanel;
            this._sandDockManager1.OwnerForm = this;
            this._sandDockManager1.Renderer = new TD.SandDock.Rendering.Office2003Renderer();
            this._sandDockManager1.SerializeTabbedDocuments = true;
            // 
            // _menuStrip
            // 
            this._menuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileToolStripMenuItem,
            this._editToolStripMenuItem,
            this._viewToolStripMenuItem,
            this._toolsToolStripMenuItem,
            this._helpToolStripMenuItem});
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.Size = new System.Drawing.Size(751, 24);
            this._menuStrip.TabIndex = 0;
            this._menuStrip.Text = "menuStrip1";
            // 
            // _fileToolStripMenuItem
            // 
            this._fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._openImageToolStripMenuItem1,
            this._closeImageToolStripMenuItem,
            this.toolStripSeparator5,
            this._saveToolStripMenuItem,
            this._saveAsToolStripMenuItem,
            this.toolStripSeparator19,
            this._pageSetupToolStripMenuItem,
            this._previewToolStripMenuItem,
            this._printImageToolStripMenuItem,
            this.toolStripSeparator6,
            this._propertiesToolStripMenuItem,
            this.toolStripSeparator7,
            this._exitToolStripMenuItem});
            this._fileToolStripMenuItem.Name = "_fileToolStripMenuItem";
            this._fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this._fileToolStripMenuItem.Text = "&File";
            // 
            // _openImageToolStripMenuItem1
            // 
            this._openImageToolStripMenuItem1.Enabled = false;
            this._openImageToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("_openImageToolStripMenuItem1.Image")));
            this._openImageToolStripMenuItem1.ImageViewer = null;
            this._openImageToolStripMenuItem1.Name = "_openImageToolStripMenuItem1";
            this._openImageToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._openImageToolStripMenuItem1.Size = new System.Drawing.Size(155, 22);
            this._openImageToolStripMenuItem1.Text = "&Open...";
            // 
            // _closeImageToolStripMenuItem
            // 
            this._closeImageToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._closeImageToolStripMenuItem.Enabled = false;
            this._closeImageToolStripMenuItem.ImageViewer = null;
            this._closeImageToolStripMenuItem.Name = "_closeImageToolStripMenuItem";
            this._closeImageToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+F4";
            this._closeImageToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._closeImageToolStripMenuItem.Text = "&Close";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(152, 6);
            // 
            // _saveToolStripMenuItem
            // 
            this._saveToolStripMenuItem.Enabled = false;
            this._saveToolStripMenuItem.Image = global::IDShieldOffice.Properties.Resources.SaveImageButtonSmall;
            this._saveToolStripMenuItem.Name = "_saveToolStripMenuItem";
            this._saveToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+S";
            this._saveToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._saveToolStripMenuItem.Text = "&Save";
            this._saveToolStripMenuItem.Click += new System.EventHandler(this.HandleSaveToolStripMenuItemClick);
            // 
            // _saveAsToolStripMenuItem
            // 
            this._saveAsToolStripMenuItem.Enabled = false;
            this._saveAsToolStripMenuItem.Name = "_saveAsToolStripMenuItem";
            this._saveAsToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._saveAsToolStripMenuItem.Text = "Save &As...";
            this._saveAsToolStripMenuItem.Click += new System.EventHandler(this.HandleSaveAsToolStripMenuItemClick);
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(152, 6);
            // 
            // _pageSetupToolStripMenuItem
            // 
            this._pageSetupToolStripMenuItem.Enabled = false;
            this._pageSetupToolStripMenuItem.Name = "_pageSetupToolStripMenuItem";
            this._pageSetupToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._pageSetupToolStripMenuItem.Text = "Pa&ge setup...";
            this._pageSetupToolStripMenuItem.Click += new System.EventHandler(this.HandlePageSetupMenuItemClick);
            // 
            // _previewToolStripMenuItem
            // 
            this._previewToolStripMenuItem.Enabled = false;
            this._previewToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_previewToolStripMenuItem.Image")));
            this._previewToolStripMenuItem.ImageViewer = null;
            this._previewToolStripMenuItem.Name = "_previewToolStripMenuItem";
            this._previewToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._previewToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._previewToolStripMenuItem.Text = "Print pre&view";
            // 
            // _printImageToolStripMenuItem
            // 
            this._printImageToolStripMenuItem.Enabled = false;
            this._printImageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_printImageToolStripMenuItem.Image")));
            this._printImageToolStripMenuItem.ImageViewer = null;
            this._printImageToolStripMenuItem.Name = "_printImageToolStripMenuItem";
            this._printImageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._printImageToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._printImageToolStripMenuItem.Text = "&Print...";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(152, 6);
            // 
            // _propertiesToolStripMenuItem
            // 
            this._propertiesToolStripMenuItem.Enabled = false;
            this._propertiesToolStripMenuItem.Name = "_propertiesToolStripMenuItem";
            this._propertiesToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._propertiesToolStripMenuItem.Text = "P&roperties";
            this._propertiesToolStripMenuItem.Click += new System.EventHandler(this.HandlePropertiesToolStripMenuItemClick);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(152, 6);
            // 
            // _exitToolStripMenuItem
            // 
            this._exitToolStripMenuItem.Name = "_exitToolStripMenuItem";
            this._exitToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._exitToolStripMenuItem.Text = "E&xit";
            this._exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // _editToolStripMenuItem
            // 
            this._editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._selectAllToolStripMenuItem,
            this._deleteSelectionToolStripMenuItem,
            this.toolStripSeparator8,
            this._findToolStripMenuItem,
            this.toolStripSeparator18,
            this._redactEntirePageToolStripMenuItem,
            this.toolStripSeparator9,
            this._preferencesToolStripMenuItem});
            this._editToolStripMenuItem.Name = "_editToolStripMenuItem";
            this._editToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this._editToolStripMenuItem.Text = "&Edit";
            // 
            // _selectAllToolStripMenuItem
            // 
            this._selectAllToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._selectAllToolStripMenuItem.Enabled = false;
            this._selectAllToolStripMenuItem.ImageViewer = null;
            this._selectAllToolStripMenuItem.Name = "_selectAllToolStripMenuItem";
            this._selectAllToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._selectAllToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._selectAllToolStripMenuItem.Text = "Select &all";
            // 
            // _deleteSelectionToolStripMenuItem
            // 
            this._deleteSelectionToolStripMenuItem.Enabled = false;
            this._deleteSelectionToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_deleteSelectionToolStripMenuItem.Image")));
            this._deleteSelectionToolStripMenuItem.ImageViewer = null;
            this._deleteSelectionToolStripMenuItem.Name = "_deleteSelectionToolStripMenuItem";
            this._deleteSelectionToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._deleteSelectionToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._deleteSelectionToolStripMenuItem.Text = "&Delete selection";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(177, 6);
            // 
            // _findToolStripMenuItem
            // 
            this._findToolStripMenuItem.Image = global::IDShieldOffice.Properties.Resources.FindWordsSmall;
            this._findToolStripMenuItem.Name = "_findToolStripMenuItem";
            this._findToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+F";
            this._findToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._findToolStripMenuItem.Text = "&Find...";
            this._findToolStripMenuItem.Click += new System.EventHandler(this.HandleShowWordOrPatternListRuleForm);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(177, 6);
            // 
            // _redactEntirePageToolStripMenuItem
            // 
            this._redactEntirePageToolStripMenuItem.Enabled = false;
            this._redactEntirePageToolStripMenuItem.Name = "_redactEntirePageToolStripMenuItem";
            this._redactEntirePageToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._redactEntirePageToolStripMenuItem.Text = "&Redact entire page";
            this._redactEntirePageToolStripMenuItem.Click += new System.EventHandler(this.HandleRedactEntirePageMenuItemClick);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(177, 6);
            // 
            // _preferencesToolStripMenuItem
            // 
            this._preferencesToolStripMenuItem.Name = "_preferencesToolStripMenuItem";
            this._preferencesToolStripMenuItem.ShortcutKeyDisplayString = "F12";
            this._preferencesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._preferencesToolStripMenuItem.Text = "&Preferences...";
            this._preferencesToolStripMenuItem.Click += new System.EventHandler(this.HandlePreferencesToolStripMenuItemClick);
            // 
            // _viewToolStripMenuItem
            // 
            this._viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoomToolStripMenuItem,
            this._rotateToolStripMenuItem,
            this._gotoPageToolStripMenuItem,
            this._tilesToolStripMenuItem,
            this._redactionObjectToolStripMenuItem,
            this.toolStripSeparator14,
            this._showLayersWindowToolStripMenuItem,
            this._showObjectPropertiesGridWindowToolStripMenuItem});
            this._viewToolStripMenuItem.Name = "_viewToolStripMenuItem";
            this._viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this._viewToolStripMenuItem.Text = "&View";
            // 
            // zoomToolStripMenuItem
            // 
            this.zoomToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fitToPageToolStripMenuItem,
            this._fitToWidthToolStripMenuItem,
            this.toolStripSeparator10,
            this._zoomInToolStripMenuItem,
            this._zoomOutToolStripMenuItem,
            this.toolStripSeparator11,
            this._zoomPreviousToolStripMenuItem,
            this._zoomNextToolStripMenuItem});
            this.zoomToolStripMenuItem.Name = "zoomToolStripMenuItem";
            this.zoomToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.zoomToolStripMenuItem.Text = "&Zoom";
            // 
            // _fitToPageToolStripMenuItem
            // 
            this._fitToPageToolStripMenuItem.Enabled = false;
            this._fitToPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_fitToPageToolStripMenuItem.Image")));
            this._fitToPageToolStripMenuItem.ImageViewer = null;
            this._fitToPageToolStripMenuItem.Name = "_fitToPageToolStripMenuItem";
            this._fitToPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._fitToPageToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._fitToPageToolStripMenuItem.Text = "Fit to &page";
            // 
            // _fitToWidthToolStripMenuItem
            // 
            this._fitToWidthToolStripMenuItem.Enabled = false;
            this._fitToWidthToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_fitToWidthToolStripMenuItem.Image")));
            this._fitToWidthToolStripMenuItem.ImageViewer = null;
            this._fitToWidthToolStripMenuItem.Name = "_fitToWidthToolStripMenuItem";
            this._fitToWidthToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._fitToWidthToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._fitToWidthToolStripMenuItem.Text = "Fit to &width";
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(152, 6);
            // 
            // _zoomInToolStripMenuItem
            // 
            this._zoomInToolStripMenuItem.Enabled = false;
            this._zoomInToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomInToolStripMenuItem.Image")));
            this._zoomInToolStripMenuItem.ImageViewer = null;
            this._zoomInToolStripMenuItem.Name = "_zoomInToolStripMenuItem";
            this._zoomInToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomInToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._zoomInToolStripMenuItem.Text = "Zoom in";
            // 
            // _zoomOutToolStripMenuItem
            // 
            this._zoomOutToolStripMenuItem.Enabled = false;
            this._zoomOutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomOutToolStripMenuItem.Image")));
            this._zoomOutToolStripMenuItem.ImageViewer = null;
            this._zoomOutToolStripMenuItem.Name = "_zoomOutToolStripMenuItem";
            this._zoomOutToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomOutToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._zoomOutToolStripMenuItem.Text = "Zoom out";
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(152, 6);
            // 
            // _zoomPreviousToolStripMenuItem
            // 
            this._zoomPreviousToolStripMenuItem.Enabled = false;
            this._zoomPreviousToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomPreviousToolStripMenuItem.Image")));
            this._zoomPreviousToolStripMenuItem.ImageViewer = null;
            this._zoomPreviousToolStripMenuItem.Name = "_zoomPreviousToolStripMenuItem";
            this._zoomPreviousToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomPreviousToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._zoomPreviousToolStripMenuItem.Text = "Zoom previous";
            // 
            // _zoomNextToolStripMenuItem
            // 
            this._zoomNextToolStripMenuItem.Enabled = false;
            this._zoomNextToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomNextToolStripMenuItem.Image")));
            this._zoomNextToolStripMenuItem.ImageViewer = null;
            this._zoomNextToolStripMenuItem.Name = "_zoomNextToolStripMenuItem";
            this._zoomNextToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomNextToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._zoomNextToolStripMenuItem.Text = "Zoom next";
            // 
            // _rotateToolStripMenuItem
            // 
            this._rotateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._rotateCounterclockwiseToolStripMenuItem,
            this._rotateClockwiseToolStripMenuItem});
            this._rotateToolStripMenuItem.Name = "_rotateToolStripMenuItem";
            this._rotateToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this._rotateToolStripMenuItem.Text = "&Rotate";
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
            // _gotoPageToolStripMenuItem
            // 
            this._gotoPageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._firstPageToolStripMenuItem,
            this._previousPageToolStripMenuItem,
            this.toolStripSeparator12,
            this._pageNavigationToolStripMenuItem,
            this.toolStripSeparator13,
            this._nextPageToolStripMenuItem,
            this._lastPageToolStripMenuItem});
            this._gotoPageToolStripMenuItem.Name = "_gotoPageToolStripMenuItem";
            this._gotoPageToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this._gotoPageToolStripMenuItem.Text = "&Goto page";
            // 
            // _firstPageToolStripMenuItem
            // 
            this._firstPageToolStripMenuItem.Enabled = false;
            this._firstPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_firstPageToolStripMenuItem.Image")));
            this._firstPageToolStripMenuItem.ImageViewer = null;
            this._firstPageToolStripMenuItem.Name = "_firstPageToolStripMenuItem";
            this._firstPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._firstPageToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this._firstPageToolStripMenuItem.Text = "First page";
            // 
            // _previousPageToolStripMenuItem
            // 
            this._previousPageToolStripMenuItem.Enabled = false;
            this._previousPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_previousPageToolStripMenuItem.Image")));
            this._previousPageToolStripMenuItem.ImageViewer = null;
            this._previousPageToolStripMenuItem.Name = "_previousPageToolStripMenuItem";
            this._previousPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._previousPageToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this._previousPageToolStripMenuItem.Text = "Previous page";
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(157, 6);
            // 
            // _pageNavigationToolStripMenuItem
            // 
            this._pageNavigationToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._pageNavigationToolStripMenuItem.Enabled = false;
            this._pageNavigationToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_pageNavigationToolStripMenuItem.Image")));
            this._pageNavigationToolStripMenuItem.ImageViewer = null;
            this._pageNavigationToolStripMenuItem.Name = "_pageNavigationToolStripMenuItem";
            this._pageNavigationToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._pageNavigationToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this._pageNavigationToolStripMenuItem.Text = "Page n&umber...";
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(157, 6);
            // 
            // _nextPageToolStripMenuItem
            // 
            this._nextPageToolStripMenuItem.Enabled = false;
            this._nextPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_nextPageToolStripMenuItem.Image")));
            this._nextPageToolStripMenuItem.ImageViewer = null;
            this._nextPageToolStripMenuItem.Name = "_nextPageToolStripMenuItem";
            this._nextPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._nextPageToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this._nextPageToolStripMenuItem.Text = "Next page";
            // 
            // _lastPageToolStripMenuItem
            // 
            this._lastPageToolStripMenuItem.Enabled = false;
            this._lastPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_lastPageToolStripMenuItem.Image")));
            this._lastPageToolStripMenuItem.ImageViewer = null;
            this._lastPageToolStripMenuItem.Name = "_lastPageToolStripMenuItem";
            this._lastPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._lastPageToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this._lastPageToolStripMenuItem.Text = "Last page";
            // 
            // _tilesToolStripMenuItem
            // 
            this._tilesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._previousTileToolStripMenuItem,
            this._nextTileToolStripMenuItem});
            this._tilesToolStripMenuItem.Name = "_tilesToolStripMenuItem";
            this._tilesToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this._tilesToolStripMenuItem.Text = "&Tiles";
            // 
            // _previousTileToolStripMenuItem
            // 
            this._previousTileToolStripMenuItem.Enabled = false;
            this._previousTileToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_previousTileToolStripMenuItem.Image")));
            this._previousTileToolStripMenuItem.ImageViewer = null;
            this._previousTileToolStripMenuItem.Name = "_previousTileToolStripMenuItem";
            this._previousTileToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._previousTileToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this._previousTileToolStripMenuItem.Text = "Previous tile";
            // 
            // _nextTileToolStripMenuItem
            // 
            this._nextTileToolStripMenuItem.Enabled = false;
            this._nextTileToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_nextTileToolStripMenuItem.Image")));
            this._nextTileToolStripMenuItem.ImageViewer = null;
            this._nextTileToolStripMenuItem.Name = "_nextTileToolStripMenuItem";
            this._nextTileToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._nextTileToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this._nextTileToolStripMenuItem.Text = "Next tile";
            // 
            // _redactionObjectToolStripMenuItem
            // 
            this._redactionObjectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._previousLayerObjectToolStripMenuItem,
            this._nextLayerObjectToolStripMenuItem});
            this._redactionObjectToolStripMenuItem.Name = "_redactionObjectToolStripMenuItem";
            this._redactionObjectToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this._redactionObjectToolStripMenuItem.Text = "Re&daction / object";
            // 
            // _previousLayerObjectToolStripMenuItem
            // 
            this._previousLayerObjectToolStripMenuItem.Enabled = false;
            this._previousLayerObjectToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_previousLayerObjectToolStripMenuItem.Image")));
            this._previousLayerObjectToolStripMenuItem.ImageViewer = null;
            this._previousLayerObjectToolStripMenuItem.Name = "_previousLayerObjectToolStripMenuItem";
            this._previousLayerObjectToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._previousLayerObjectToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this._previousLayerObjectToolStripMenuItem.Text = "Previous object";
            // 
            // _nextLayerObjectToolStripMenuItem
            // 
            this._nextLayerObjectToolStripMenuItem.Enabled = false;
            this._nextLayerObjectToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_nextLayerObjectToolStripMenuItem.Image")));
            this._nextLayerObjectToolStripMenuItem.ImageViewer = null;
            this._nextLayerObjectToolStripMenuItem.Name = "_nextLayerObjectToolStripMenuItem";
            this._nextLayerObjectToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._nextLayerObjectToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this._nextLayerObjectToolStripMenuItem.Text = "Next object";
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(230, 6);
            // 
            // _showLayersWindowToolStripMenuItem
            // 
            this._showLayersWindowToolStripMenuItem.Image = global::IDShieldOffice.Properties.Resources.ShowHideLayersSmall;
            this._showLayersWindowToolStripMenuItem.Name = "_showLayersWindowToolStripMenuItem";
            this._showLayersWindowToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+L";
            this._showLayersWindowToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this._showLayersWindowToolStripMenuItem.Text = "&Layers window";
            this._showLayersWindowToolStripMenuItem.Click += new System.EventHandler(this.HandleShowLayersWindow);
            // 
            // _showObjectPropertiesGridWindowToolStripMenuItem
            // 
            this._showObjectPropertiesGridWindowToolStripMenuItem.Image = global::IDShieldOffice.Properties.Resources.LayerObjectPropertiesSmall;
            this._showObjectPropertiesGridWindowToolStripMenuItem.Name = "_showObjectPropertiesGridWindowToolStripMenuItem";
            this._showObjectPropertiesGridWindowToolStripMenuItem.ShortcutKeyDisplayString = "F10";
            this._showObjectPropertiesGridWindowToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this._showObjectPropertiesGridWindowToolStripMenuItem.Text = "&Object properties window";
            this._showObjectPropertiesGridWindowToolStripMenuItem.Click += new System.EventHandler(this.HandleShowObjectPropertyGridWindow);
            // 
            // _toolsToolStripMenuItem
            // 
            this._toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._selectLayerObjectToolStripMenuItem,
            this._panToolStripMenuItem,
            this._zoomWindowToolStripMenuItem,
            this.toolStripSeparator15,
            this._angularRedactionToolStripMenuItem,
            this._rectangularRedactionToolStripMenuItem,
            this.toolStripSeparator16,
            this._findOrRedactToolStripMenuItem,
            this.toolStripSeparator17,
            this._applyBatesNumberToolStripMenuItem});
            this._toolsToolStripMenuItem.Name = "_toolsToolStripMenuItem";
            this._toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this._toolsToolStripMenuItem.Text = "&Tools";
            // 
            // _selectLayerObjectToolStripMenuItem
            // 
            this._selectLayerObjectToolStripMenuItem.Enabled = false;
            this._selectLayerObjectToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_selectLayerObjectToolStripMenuItem.Image")));
            this._selectLayerObjectToolStripMenuItem.ImageViewer = null;
            this._selectLayerObjectToolStripMenuItem.Name = "_selectLayerObjectToolStripMenuItem";
            this._selectLayerObjectToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._selectLayerObjectToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._selectLayerObjectToolStripMenuItem.Text = "Select redactions and other objects";
            // 
            // _panToolStripMenuItem
            // 
            this._panToolStripMenuItem.Enabled = false;
            this._panToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_panToolStripMenuItem.Image")));
            this._panToolStripMenuItem.ImageViewer = null;
            this._panToolStripMenuItem.Name = "_panToolStripMenuItem";
            this._panToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._panToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._panToolStripMenuItem.Text = "P&an";
            // 
            // _zoomWindowToolStripMenuItem
            // 
            this._zoomWindowToolStripMenuItem.Enabled = false;
            this._zoomWindowToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomWindowToolStripMenuItem.Image")));
            this._zoomWindowToolStripMenuItem.ImageViewer = null;
            this._zoomWindowToolStripMenuItem.Name = "_zoomWindowToolStripMenuItem";
            this._zoomWindowToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomWindowToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._zoomWindowToolStripMenuItem.Text = "&Zoom window";
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(252, 6);
            // 
            // _angularRedactionToolStripMenuItem
            // 
            this._angularRedactionToolStripMenuItem.Enabled = false;
            this._angularRedactionToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_angularRedactionToolStripMenuItem.Image")));
            this._angularRedactionToolStripMenuItem.ImageViewer = null;
            this._angularRedactionToolStripMenuItem.Name = "_angularRedactionToolStripMenuItem";
            this._angularRedactionToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._angularRedactionToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._angularRedactionToolStripMenuItem.Text = "A&ngular redaction";
            // 
            // _rectangularRedactionToolStripMenuItem
            // 
            this._rectangularRedactionToolStripMenuItem.Enabled = false;
            this._rectangularRedactionToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rectangularRedactionToolStripMenuItem.Image")));
            this._rectangularRedactionToolStripMenuItem.ImageViewer = null;
            this._rectangularRedactionToolStripMenuItem.Name = "_rectangularRedactionToolStripMenuItem";
            this._rectangularRedactionToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._rectangularRedactionToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._rectangularRedactionToolStripMenuItem.Text = "&Rectangular redaction";
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(252, 6);
            // 
            // _findOrRedactToolStripMenuItem
            // 
            this._findOrRedactToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._bracketedTextFinderToolStripMenuItem,
            this._dataTypesFinderToolStripMenuItem,
            this._wordListFinderToolStripMenuItem});
            this._findOrRedactToolStripMenuItem.Name = "_findOrRedactToolStripMenuItem";
            this._findOrRedactToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._findOrRedactToolStripMenuItem.Text = "&Find or redact";
            // 
            // _bracketedTextFinderToolStripMenuItem
            // 
            this._bracketedTextFinderToolStripMenuItem.Image = global::IDShieldOffice.Properties.Resources.FindBracketedTextSmall;
            this._bracketedTextFinderToolStripMenuItem.Name = "_bracketedTextFinderToolStripMenuItem";
            this._bracketedTextFinderToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+K";
            this._bracketedTextFinderToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this._bracketedTextFinderToolStripMenuItem.Text = "&Bracketed text...";
            this._bracketedTextFinderToolStripMenuItem.Click += new System.EventHandler(this.HandleShowBracketedTextFinderWindow);
            // 
            // _dataTypesFinderToolStripMenuItem
            // 
            this._dataTypesFinderToolStripMenuItem.Image = global::IDShieldOffice.Properties.Resources.FindDataTypesSmall;
            this._dataTypesFinderToolStripMenuItem.Name = "_dataTypesFinderToolStripMenuItem";
            this._dataTypesFinderToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+D";
            this._dataTypesFinderToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this._dataTypesFinderToolStripMenuItem.Text = "&Data types...";
            this._dataTypesFinderToolStripMenuItem.Click += new System.EventHandler(this.HandleShowDataTypeRuleForm);
            // 
            // _wordListFinderToolStripMenuItem
            // 
            this._wordListFinderToolStripMenuItem.Image = global::IDShieldOffice.Properties.Resources.FindWordsSmall;
            this._wordListFinderToolStripMenuItem.Name = "_wordListFinderToolStripMenuItem";
            this._wordListFinderToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+F";
            this._wordListFinderToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this._wordListFinderToolStripMenuItem.Text = "&Words / patterns...";
            this._wordListFinderToolStripMenuItem.Click += new System.EventHandler(this.HandleShowWordOrPatternListRuleForm);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(252, 6);
            // 
            // _applyBatesNumberToolStripMenuItem
            // 
            this._applyBatesNumberToolStripMenuItem.Enabled = false;
            this._applyBatesNumberToolStripMenuItem.Image = global::IDShieldOffice.Properties.Resources.AddBatesNumberSmall;
            this._applyBatesNumberToolStripMenuItem.Name = "_applyBatesNumberToolStripMenuItem";
            this._applyBatesNumberToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+B";
            this._applyBatesNumberToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._applyBatesNumberToolStripMenuItem.Text = "&Apply Bates number";
            this._applyBatesNumberToolStripMenuItem.Click += new System.EventHandler(this.HandleApplyBatesNumberToolStripMenuItemClick);
            // 
            // _helpToolStripMenuItem
            // 
            this._helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._idShieldOfficeHelpToolStripMenuItem,
            this._regularExpressionHelpToolStripMenuItem,
            this._aboutIDShieldOfficeToolStripMenuItem});
            this._helpToolStripMenuItem.Name = "_helpToolStripMenuItem";
            this._helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this._helpToolStripMenuItem.Text = "&Help";
            // 
            // _idShieldOfficeHelpToolStripMenuItem
            // 
            this._idShieldOfficeHelpToolStripMenuItem.Name = "_idShieldOfficeHelpToolStripMenuItem";
            this._idShieldOfficeHelpToolStripMenuItem.ShortcutKeyDisplayString = "F1";
            this._idShieldOfficeHelpToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this._idShieldOfficeHelpToolStripMenuItem.Text = "&ID Shield Office help...";
            this._idShieldOfficeHelpToolStripMenuItem.Click += new System.EventHandler(this.HandleIdShieldOfficeHelpToolStripMenuItemClick);
            // 
            // _regularExpressionHelpToolStripMenuItem
            // 
            this._regularExpressionHelpToolStripMenuItem.Name = "_regularExpressionHelpToolStripMenuItem";
            this._regularExpressionHelpToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this._regularExpressionHelpToolStripMenuItem.Text = "&Regular expression help...";
            this._regularExpressionHelpToolStripMenuItem.Click += new System.EventHandler(this.HandleRegularExpressionHelpToolStripMenuItemClick);
            // 
            // _aboutIDShieldOfficeToolStripMenuItem
            // 
            this._aboutIDShieldOfficeToolStripMenuItem.Name = "_aboutIDShieldOfficeToolStripMenuItem";
            this._aboutIDShieldOfficeToolStripMenuItem.Size = new System.Drawing.Size(213, 22);
            this._aboutIDShieldOfficeToolStripMenuItem.Text = "&About ID Shield Office...";
            this._aboutIDShieldOfficeToolStripMenuItem.Click += new System.EventHandler(this.HandleAboutIDShieldOfficeMenuItemClick);
            // 
            // _fileCommandsToolStrip
            // 
            this._fileCommandsToolStrip.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._fileCommandsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._fileCommandsToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._fileCommandsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._openImageToolStripSplitButton,
            this._saveToolStripButton,
            this._printImageToolStripButton});
            this._fileCommandsToolStrip.Location = new System.Drawing.Point(3, 24);
            this._fileCommandsToolStrip.Name = "_fileCommandsToolStrip";
            this._fileCommandsToolStrip.Size = new System.Drawing.Size(132, 39);
            this._fileCommandsToolStrip.TabIndex = 1;
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
            this._openImageToolStripSplitButton.ToolTipText = "Open Image";
            // 
            // _saveToolStripButton
            // 
            this._saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._saveToolStripButton.Enabled = false;
            this._saveToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_saveToolStripButton.Image")));
            this._saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._saveToolStripButton.Name = "_saveToolStripButton";
            this._saveToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._saveToolStripButton.Text = "Save";
            this._saveToolStripButton.ToolTipText = "Save (Ctrl+S)";
            this._saveToolStripButton.Click += new System.EventHandler(this.HandleSaveToolStripButtonClick);
            // 
            // _printImageToolStripButton
            // 
            this._printImageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._printImageToolStripButton.Enabled = false;
            this._printImageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._printImageToolStripButton.ImageViewer = null;
            this._printImageToolStripButton.Name = "_printImageToolStripButton";
            this._printImageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._printImageToolStripButton.Text = "Print image";
            this._printImageToolStripButton.ToolTipText = "Print image";
            // 
            // _basicToolsToolStrip
            // 
            this._basicToolsToolStrip.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._basicToolsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._basicToolsToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._basicToolsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._selectLayerObjectToolStripButton,
            this._angularRedactionToolStripButton,
            this._rectangularRedactionToolStripButton,
            this._dataTypesFinderToolStripButton,
            this._bracketedTextFinderToolStripButton,
            this._wordListFinderToolStripButton,
            this._applyBatesNumberToolStripButton,
            this._deleteSelectionToolStripButton});
            this._basicToolsToolStrip.Location = new System.Drawing.Point(135, 24);
            this._basicToolsToolStrip.Name = "_basicToolsToolStrip";
            this._basicToolsToolStrip.Size = new System.Drawing.Size(300, 39);
            this._basicToolsToolStrip.TabIndex = 2;
            // 
            // _selectLayerObjectToolStripButton
            // 
            this._selectLayerObjectToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._selectLayerObjectToolStripButton.Enabled = false;
            this._selectLayerObjectToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._selectLayerObjectToolStripButton.ImageViewer = null;
            this._selectLayerObjectToolStripButton.Name = "_selectLayerObjectToolStripButton";
            this._selectLayerObjectToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._selectLayerObjectToolStripButton.ToolTipText = "Select redactions and other objects";
            // 
            // _dataTypesFinderToolStripButton
            // 
            this._dataTypesFinderToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._dataTypesFinderToolStripButton.Image = global::IDShieldOffice.Properties.Resources.FindDataTypes;
            this._dataTypesFinderToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._dataTypesFinderToolStripButton.Name = "_dataTypesFinderToolStripButton";
            this._dataTypesFinderToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._dataTypesFinderToolStripButton.Text = "Data type finder";
            this._dataTypesFinderToolStripButton.ToolTipText = "Data type finder (Ctrl+D)";
            this._dataTypesFinderToolStripButton.Click += new System.EventHandler(this.HandleShowDataTypeRuleForm);
            // 
            // _bracketedTextFinderToolStripButton
            // 
            this._bracketedTextFinderToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._bracketedTextFinderToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_bracketedTextFinderToolStripButton.Image")));
            this._bracketedTextFinderToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._bracketedTextFinderToolStripButton.Name = "_bracketedTextFinderToolStripButton";
            this._bracketedTextFinderToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._bracketedTextFinderToolStripButton.Text = "Bracketed text finder";
            this._bracketedTextFinderToolStripButton.ToolTipText = "Bracketed text finder (Ctrl+K)";
            this._bracketedTextFinderToolStripButton.Click += new System.EventHandler(this.HandleShowBracketedTextFinderWindow);
            // 
            // _wordListFinderToolStripButton
            // 
            this._wordListFinderToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._wordListFinderToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_wordListFinderToolStripButton.Image")));
            this._wordListFinderToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._wordListFinderToolStripButton.Name = "_wordListFinderToolStripButton";
            this._wordListFinderToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._wordListFinderToolStripButton.Text = "Word list finder";
            this._wordListFinderToolStripButton.ToolTipText = "Word list finder (Ctrl+F)";
            this._wordListFinderToolStripButton.Click += new System.EventHandler(this.HandleShowWordOrPatternListRuleForm);
            // 
            // _applyBatesNumberToolStripButton
            // 
            this._applyBatesNumberToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._applyBatesNumberToolStripButton.Enabled = false;
            this._applyBatesNumberToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_applyBatesNumberToolStripButton.Image")));
            this._applyBatesNumberToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._applyBatesNumberToolStripButton.Name = "_applyBatesNumberToolStripButton";
            this._applyBatesNumberToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._applyBatesNumberToolStripButton.Text = "Apply Bates number";
            this._applyBatesNumberToolStripButton.ToolTipText = "Apply Bates number (Ctrl+B)";
            this._applyBatesNumberToolStripButton.Click += new System.EventHandler(this.HandleApplyBatesNumberToolStripButtonClick);
            // 
            // _deleteSelectionToolStripButton
            // 
            this._deleteSelectionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._deleteSelectionToolStripButton.Enabled = false;
            this._deleteSelectionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._deleteSelectionToolStripButton.ImageViewer = null;
            this._deleteSelectionToolStripButton.Name = "_deleteSelectionToolStripButton";
            this._deleteSelectionToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._deleteSelectionToolStripButton.Text = "Delete selection";
            this._deleteSelectionToolStripButton.ToolTipText = "Delete Selection";
            // 
            // _viewCommandsToolStrip
            // 
            this._viewCommandsToolStrip.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._viewCommandsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._viewCommandsToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._viewCommandsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fitToPageToolStripButton,
            this._fitToWidthToolStripButton,
            this.toolStripSeparator1,
            this._zoomWindowToolStripButton,
            this._panToolStripButton,
            this.toolStripSeparator2,
            this._zoomInToolStripButton,
            this._zoomOutToolStripButton,
            this.toolStripSeparator3,
            this._zoomPreviousToolStripButton,
            this._zoomNextToolStripButton});
            this._viewCommandsToolStrip.Location = new System.Drawing.Point(435, 24);
            this._viewCommandsToolStrip.Name = "_viewCommandsToolStrip";
            this._viewCommandsToolStrip.Size = new System.Drawing.Size(316, 39);
            this._viewCommandsToolStrip.TabIndex = 4;
            // 
            // _fitToPageToolStripButton
            // 
            this._fitToPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._fitToPageToolStripButton.Enabled = false;
            this._fitToPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._fitToPageToolStripButton.ImageViewer = null;
            this._fitToPageToolStripButton.Name = "_fitToPageToolStripButton";
            this._fitToPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._fitToPageToolStripButton.Text = "Fit to page";
            this._fitToPageToolStripButton.ToolTipText = "Fit to page";
            // 
            // _fitToWidthToolStripButton
            // 
            this._fitToWidthToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._fitToWidthToolStripButton.Enabled = false;
            this._fitToWidthToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._fitToWidthToolStripButton.ImageViewer = null;
            this._fitToWidthToolStripButton.Name = "_fitToWidthToolStripButton";
            this._fitToWidthToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._fitToWidthToolStripButton.Text = "Fit to width";
            this._fitToWidthToolStripButton.ToolTipText = "Fit to width";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 39);
            // 
            // _zoomWindowToolStripButton
            // 
            this._zoomWindowToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomWindowToolStripButton.Enabled = false;
            this._zoomWindowToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomWindowToolStripButton.ImageViewer = null;
            this._zoomWindowToolStripButton.Name = "_zoomWindowToolStripButton";
            this._zoomWindowToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomWindowToolStripButton.ToolTipText = "Zoom window";
            // 
            // _panToolStripButton
            // 
            this._panToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._panToolStripButton.Enabled = false;
            this._panToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._panToolStripButton.ImageViewer = null;
            this._panToolStripButton.Name = "_panToolStripButton";
            this._panToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._panToolStripButton.ToolTipText = "Pan";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 39);
            // 
            // _zoomInToolStripButton
            // 
            this._zoomInToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomInToolStripButton.Enabled = false;
            this._zoomInToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomInToolStripButton.ImageViewer = null;
            this._zoomInToolStripButton.Name = "_zoomInToolStripButton";
            this._zoomInToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomInToolStripButton.Text = "Zoom in";
            this._zoomInToolStripButton.ToolTipText = "Zoom in";
            // 
            // _zoomOutToolStripButton
            // 
            this._zoomOutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomOutToolStripButton.Enabled = false;
            this._zoomOutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomOutToolStripButton.ImageViewer = null;
            this._zoomOutToolStripButton.Name = "_zoomOutToolStripButton";
            this._zoomOutToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomOutToolStripButton.Text = "Zoom out";
            this._zoomOutToolStripButton.ToolTipText = "Zoom out";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 39);
            // 
            // _zoomPreviousToolStripButton
            // 
            this._zoomPreviousToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomPreviousToolStripButton.Enabled = false;
            this._zoomPreviousToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomPreviousToolStripButton.ImageViewer = null;
            this._zoomPreviousToolStripButton.Name = "_zoomPreviousToolStripButton";
            this._zoomPreviousToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomPreviousToolStripButton.Text = "Zoom previous";
            this._zoomPreviousToolStripButton.ToolTipText = "Zoom previous";
            // 
            // _zoomNextToolStripButton
            // 
            this._zoomNextToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomNextToolStripButton.Enabled = false;
            this._zoomNextToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomNextToolStripButton.ImageViewer = null;
            this._zoomNextToolStripButton.Name = "_zoomNextToolStripButton";
            this._zoomNextToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomNextToolStripButton.Text = "Zoom next";
            this._zoomNextToolStripButton.ToolTipText = "Zoom next";
            // 
            // _layerNavigationToolStrip
            // 
            this._layerNavigationToolStrip.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._layerNavigationToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._layerNavigationToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._layerNavigationToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._previousTileToolStripButton,
            this._nextTileToolStripButton,
            this._previousLayerObjectToolStripButton,
            this._nextLayerObjectToolStripButton});
            this._layerNavigationToolStrip.Location = new System.Drawing.Point(3, 63);
            this._layerNavigationToolStrip.Name = "_layerNavigationToolStrip";
            this._layerNavigationToolStrip.Size = new System.Drawing.Size(156, 39);
            this._layerNavigationToolStrip.TabIndex = 6;
            // 
            // _previousTileToolStripButton
            // 
            this._previousTileToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._previousTileToolStripButton.Enabled = false;
            this._previousTileToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._previousTileToolStripButton.ImageViewer = null;
            this._previousTileToolStripButton.Name = "_previousTileToolStripButton";
            this._previousTileToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._previousTileToolStripButton.Text = "previousTileToolStripButton1";
            this._previousTileToolStripButton.ToolTipText = "Previous tile";
            // 
            // _nextTileToolStripButton
            // 
            this._nextTileToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextTileToolStripButton.Enabled = false;
            this._nextTileToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextTileToolStripButton.ImageViewer = null;
            this._nextTileToolStripButton.Name = "_nextTileToolStripButton";
            this._nextTileToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextTileToolStripButton.Text = "nextTileToolStripButton1";
            this._nextTileToolStripButton.ToolTipText = "Next tile";
            // 
            // _previousLayerObjectToolStripButton
            // 
            this._previousLayerObjectToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._previousLayerObjectToolStripButton.Enabled = false;
            this._previousLayerObjectToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._previousLayerObjectToolStripButton.ImageViewer = null;
            this._previousLayerObjectToolStripButton.Name = "_previousLayerObjectToolStripButton";
            this._previousLayerObjectToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._previousLayerObjectToolStripButton.Text = "previousLayerObjectToolStripButton1";
            this._previousLayerObjectToolStripButton.ToolTipText = "Go to previous object";
            // 
            // _nextLayerObjectToolStripButton
            // 
            this._nextLayerObjectToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextLayerObjectToolStripButton.Enabled = false;
            this._nextLayerObjectToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextLayerObjectToolStripButton.ImageViewer = null;
            this._nextLayerObjectToolStripButton.Name = "_nextLayerObjectToolStripButton";
            this._nextLayerObjectToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextLayerObjectToolStripButton.Text = "nextLayerObjectToolStripButton1";
            this._nextLayerObjectToolStripButton.ToolTipText = "Go to next object";
            // 
            // _pageNavigationToolStrip
            // 
            this._pageNavigationToolStrip.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._pageNavigationToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._pageNavigationToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._pageNavigationToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._firstPageToolStripButton,
            this._previousPageToolStripButton,
            this._pageNavigationToolStripTextBox,
            this._nextPageToolStripButton,
            this._lastPageToolStripButton});
            this._pageNavigationToolStrip.Location = new System.Drawing.Point(159, 63);
            this._pageNavigationToolStrip.Name = "_pageNavigationToolStrip";
            this._pageNavigationToolStrip.Size = new System.Drawing.Size(233, 39);
            this._pageNavigationToolStrip.TabIndex = 5;
            // 
            // _firstPageToolStripButton
            // 
            this._firstPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._firstPageToolStripButton.Enabled = false;
            this._firstPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._firstPageToolStripButton.ImageViewer = null;
            this._firstPageToolStripButton.Name = "_firstPageToolStripButton";
            this._firstPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._firstPageToolStripButton.Text = "firstPageToolStripButton1";
            this._firstPageToolStripButton.ToolTipText = "Go To First Page";
            // 
            // _previousPageToolStripButton
            // 
            this._previousPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._previousPageToolStripButton.Enabled = false;
            this._previousPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._previousPageToolStripButton.ImageViewer = null;
            this._previousPageToolStripButton.Name = "_previousPageToolStripButton";
            this._previousPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._previousPageToolStripButton.Text = "previousPageToolStripButton1";
            this._previousPageToolStripButton.ToolTipText = "Go To Previous Page";
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
            this._nextPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextPageToolStripButton.Enabled = false;
            this._nextPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextPageToolStripButton.ImageViewer = null;
            this._nextPageToolStripButton.Name = "_nextPageToolStripButton";
            this._nextPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextPageToolStripButton.Text = "nextPageToolStripButton1";
            this._nextPageToolStripButton.ToolTipText = "Go To Next Page";
            // 
            // _lastPageToolStripButton
            // 
            this._lastPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._lastPageToolStripButton.Enabled = false;
            this._lastPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._lastPageToolStripButton.ImageViewer = null;
            this._lastPageToolStripButton.Name = "_lastPageToolStripButton";
            this._lastPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._lastPageToolStripButton.Text = "lastPageToolStripButton1";
            this._lastPageToolStripButton.ToolTipText = "Go To Last Page";
            // 
            // _dockableWindowsToolStrip
            // 
            this._dockableWindowsToolStrip.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._dockableWindowsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._dockableWindowsToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._dockableWindowsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._showObjectPropertyGridWindowToolStripButton,
            this._showLayersWindowToolStripButton});
            this._dockableWindowsToolStrip.Location = new System.Drawing.Point(476, 63);
            this._dockableWindowsToolStrip.Name = "_dockableWindowsToolStrip";
            this._dockableWindowsToolStrip.Size = new System.Drawing.Size(84, 39);
            this._dockableWindowsToolStrip.TabIndex = 3;
            // 
            // _showObjectPropertyGridWindowToolStripButton
            // 
            this._showObjectPropertyGridWindowToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._showObjectPropertyGridWindowToolStripButton.Image = global::IDShieldOffice.Properties.Resources.LayerObjectProperties;
            this._showObjectPropertyGridWindowToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._showObjectPropertyGridWindowToolStripButton.Name = "_showObjectPropertyGridWindowToolStripButton";
            this._showObjectPropertyGridWindowToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._showObjectPropertyGridWindowToolStripButton.Text = "Show or hide object properties";
            this._showObjectPropertyGridWindowToolStripButton.ToolTipText = "Show or hide object properties (F10)";
            this._showObjectPropertyGridWindowToolStripButton.Click += new System.EventHandler(this.HandleShowObjectPropertyGridWindow);
            // 
            // _showLayersWindowToolStripButton
            // 
            this._showLayersWindowToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._showLayersWindowToolStripButton.Image = global::IDShieldOffice.Properties.Resources.ShowHideLayers;
            this._showLayersWindowToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._showLayersWindowToolStripButton.Name = "_showLayersWindowToolStripButton";
            this._showLayersWindowToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._showLayersWindowToolStripButton.Text = "Show or hide layers";
            this._showLayersWindowToolStripButton.ToolTipText = "Show or hide layers (Ctrl+L)";
            this._showLayersWindowToolStripButton.Click += new System.EventHandler(this.HandleShowLayersWindow);
            // 
            // _rotateCommandsToolStrip
            // 
            this._rotateCommandsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._rotateCommandsToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._rotateCommandsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._rotateCounterclockwiseToolStripButton,
            this._rotateClockwiseToolStripButton});
            this._rotateCommandsToolStrip.Location = new System.Drawing.Point(392, 63);
            this._rotateCommandsToolStrip.Name = "_rotateCommandsToolStrip";
            this._rotateCommandsToolStrip.Size = new System.Drawing.Size(84, 39);
            this._rotateCommandsToolStrip.TabIndex = 7;
            // 
            // _rotateCounterclockwiseToolStripButton
            // 
            this._rotateCounterclockwiseToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._rotateCounterclockwiseToolStripButton.Enabled = false;
            this._rotateCounterclockwiseToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._rotateCounterclockwiseToolStripButton.ImageViewer = null;
            this._rotateCounterclockwiseToolStripButton.Name = "_rotateCounterclockwiseToolStripButton";
            this._rotateCounterclockwiseToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._rotateCounterclockwiseToolStripButton.Text = "Rotate counterclockwise";
            this._rotateCounterclockwiseToolStripButton.ToolTipText = "Rotate counterclockwise";
            // 
            // _rotateClockwiseToolStripButton
            // 
            this._rotateClockwiseToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._rotateClockwiseToolStripButton.Enabled = false;
            this._rotateClockwiseToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._rotateClockwiseToolStripButton.ImageViewer = null;
            this._rotateClockwiseToolStripButton.Name = "_rotateClockwiseToolStripButton";
            this._rotateClockwiseToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._rotateClockwiseToolStripButton.Text = "Rotate clockwise";
            this._rotateClockwiseToolStripButton.ToolTipText = "Rotate clockwise";
            // 
            // _rectangularRedactionToolStripButton
            // 
            this._rectangularRedactionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._rectangularRedactionToolStripButton.Enabled = false;
            this._rectangularRedactionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._rectangularRedactionToolStripButton.ImageViewer = null;
            this._rectangularRedactionToolStripButton.Name = "_rectangularRedactionToolStripButton";
            this._rectangularRedactionToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._rectangularRedactionToolStripButton.ToolTipText = "Create rectangular redaction";
            // 
            // _angularRedactionToolStripButton
            // 
            this._angularRedactionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._angularRedactionToolStripButton.Enabled = false;
            this._angularRedactionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._angularRedactionToolStripButton.ImageViewer = null;
            this._angularRedactionToolStripButton.Name = "_angularRedactionToolStripButton";
            this._angularRedactionToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._angularRedactionToolStripButton.ToolTipText = "Create angular redaction";
            // 
            // IDShieldOfficeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(751, 566);
            this.Controls.Add(this._toolStripContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this._menuStrip;
            this.Name = "IDShieldOfficeForm";
            this.Text = "ID Shield Office";
            this.Click += new System.EventHandler(this.HandleShowLayersWindow);
            this._statusStrip.ResumeLayout(false);
            this._statusStrip.PerformLayout();
            this._toolStripContainer.BottomToolStripPanel.ResumeLayout(false);
            this._toolStripContainer.BottomToolStripPanel.PerformLayout();
            this._toolStripContainer.ContentPanel.ResumeLayout(false);
            this._toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this._toolStripContainer.TopToolStripPanel.PerformLayout();
            this._toolStripContainer.ResumeLayout(false);
            this._toolStripContainer.PerformLayout();
            this._idsoContextMenu.ResumeLayout(false);
            this._dockContainer1.ResumeLayout(false);
            this._objectPropertyGridDockableWindow.ResumeLayout(false);
            this._layersDockableWindow.ResumeLayout(false);
            this._layersDockableWindow.PerformLayout();
            this._layersViewPanel.ResumeLayout(false);
            this._layersViewPanel.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this._fileCommandsToolStrip.ResumeLayout(false);
            this._fileCommandsToolStrip.PerformLayout();
            this._basicToolsToolStrip.ResumeLayout(false);
            this._basicToolsToolStrip.PerformLayout();
            this._viewCommandsToolStrip.ResumeLayout(false);
            this._viewCommandsToolStrip.PerformLayout();
            this._layerNavigationToolStrip.ResumeLayout(false);
            this._layerNavigationToolStrip.PerformLayout();
            this._pageNavigationToolStrip.ResumeLayout(false);
            this._pageNavigationToolStrip.PerformLayout();
            this._dockableWindowsToolStrip.ResumeLayout(false);
            this._dockableWindowsToolStrip.PerformLayout();
            this._rotateCommandsToolStrip.ResumeLayout(false);
            this._rotateCommandsToolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.StatusStrip _statusStrip;
        private Extract.Imaging.Forms.UserActionToolStripStatusLabel _userActionToolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel _ocrProgressStatusLabel;
        private Extract.Imaging.Forms.ResolutionToolStripStatusLabel _resolutionToolStripStatusLabel;
        private Extract.Imaging.Forms.MousePositionToolStripStatusLabel _mousePositionToolStripStatusLabel;
        private System.Windows.Forms.ToolStripContainer _toolStripContainer;
        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.ToolStripMenuItem _fileToolStripMenuItem;
        private TD.SandDock.DockContainer _dockContainer1;
        private TD.SandDock.DockableWindow _objectPropertyGridDockableWindow;
        private TD.SandDock.SandDockManager _sandDockManager1;
        private TD.SandDock.DockableWindow _layersDockableWindow;
        private System.Windows.Forms.ToolStrip _fileCommandsToolStrip;
        private System.Windows.Forms.ToolStrip _basicToolsToolStrip;
        private System.Windows.Forms.ToolStrip _dockableWindowsToolStrip;
        private System.Windows.Forms.ToolStrip _viewCommandsToolStrip;
        private System.Windows.Forms.ToolStrip _pageNavigationToolStrip;
        private System.Windows.Forms.ToolStrip _layerNavigationToolStrip;
        private Extract.Imaging.Forms.ImageViewer _imageViewer;
        private Extract.Imaging.Forms.OpenImageToolStripSplitButton _openImageToolStripSplitButton;
        private System.Windows.Forms.ToolStripButton _saveToolStripButton;
        private Extract.Imaging.Forms.PrintImageToolStripButton _printImageToolStripButton;
        private System.Windows.Forms.ToolStripButton _dataTypesFinderToolStripButton;
        private System.Windows.Forms.ToolStripButton _bracketedTextFinderToolStripButton;
        private System.Windows.Forms.ToolStripButton _wordListFinderToolStripButton;
        private System.Windows.Forms.ToolStripButton _applyBatesNumberToolStripButton;
        private System.Windows.Forms.ToolStripButton _showObjectPropertyGridWindowToolStripButton;
        private System.Windows.Forms.ToolStripButton _showLayersWindowToolStripButton;
        private Extract.Imaging.Forms.FitToPageToolStripButton _fitToPageToolStripButton;
        private Extract.Imaging.Forms.FitToWidthToolStripButton _fitToWidthToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private Extract.Imaging.Forms.ZoomWindowToolStripButton _zoomWindowToolStripButton;
        private Extract.Imaging.Forms.PanToolStripButton _panToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private Extract.Imaging.Forms.ZoomInToolStripButton _zoomInToolStripButton;
        private Extract.Imaging.Forms.ZoomOutToolStripButton _zoomOutToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private Extract.Imaging.Forms.ZoomPreviousToolStripButton _zoomPreviousToolStripButton;
        private Extract.Imaging.Forms.ZoomNextToolStripButton _zoomNextToolStripButton;
        private Extract.Imaging.Forms.FirstPageToolStripButton _firstPageToolStripButton;
        private Extract.Imaging.Forms.PreviousPageToolStripButton _previousPageToolStripButton;
        private Extract.Imaging.Forms.PageNavigationToolStripTextBox _pageNavigationToolStripTextBox;
        private Extract.Imaging.Forms.NextPageToolStripButton _nextPageToolStripButton;
        private Extract.Imaging.Forms.LastPageToolStripButton _lastPageToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem _editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _idShieldOfficeHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _regularExpressionHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _aboutIDShieldOfficeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem _saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _propertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem _exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem _redactEntirePageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem _preferencesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem _rotateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _gotoPageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private Extract.Imaging.Forms.PageNavigationToolStripMenuItem _pageNavigationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripMenuItem _tilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _redactionObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _bracketedTextFinderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _dataTypesFinderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _wordListFinderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripMenuItem _showLayersWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _showObjectPropertiesGridWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator16;
        private System.Windows.Forms.ToolStripMenuItem _findOrRedactToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator17;
        private System.Windows.Forms.ToolStripMenuItem _applyBatesNumberToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton _checkAllLayersToolStripButton;
        private System.Windows.Forms.ToolStripButton _uncheckAllLayersToolStripButton;
        private System.Windows.Forms.PropertyGrid _objectPropertyGrid;
        private Extract.Imaging.Forms.PreviousTileToolStripButton _previousTileToolStripButton;
        private Extract.Imaging.Forms.NextTileToolStripButton _nextTileToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem _findToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator18;
        private Extract.Imaging.Forms.SelectLayerObjectToolStripButton _selectLayerObjectToolStripButton;
        private System.Windows.Forms.Panel _layersViewPanel;
        private System.Windows.Forms.CheckBox _showRedactionsCheckBox;
        private System.Windows.Forms.CheckBox _showCluesCheckBox;
        private System.Windows.Forms.CheckBox _showBatesNumberCheckBox;
        private Extract.Imaging.Forms.CloseImageToolStripMenuItem _closeImageToolStripMenuItem;
        private Extract.Imaging.Forms.SelectAllLayerObjectsToolStripMenuItem _selectAllToolStripMenuItem;
        private Extract.Imaging.Forms.PreviousLayerObjectToolStripButton _previousLayerObjectToolStripButton;
        private Extract.Imaging.Forms.NextLayerObjectToolStripButton _nextLayerObjectToolStripButton;
        private Extract.Imaging.Forms.DeleteSelectionToolStripButton _deleteSelectionToolStripButton;
        private Extract.Imaging.Forms.DeleteSelectionToolStripMenuItem _deleteSelectionToolStripMenuItem;
        private Extract.Imaging.Forms.PanToolStripMenuItem _panToolStripMenuItem;
        private Extract.Imaging.Forms.OpenImageToolStripMenuItem _openImageToolStripMenuItem1;
        private Extract.Imaging.Forms.FitToPageToolStripMenuItem _fitToPageToolStripMenuItem;
        private Extract.Imaging.Forms.FitToWidthToolStripMenuItem _fitToWidthToolStripMenuItem;
        private Extract.Imaging.Forms.ZoomInToolStripMenuItem _zoomInToolStripMenuItem;
        private Extract.Imaging.Forms.ZoomOutToolStripMenuItem _zoomOutToolStripMenuItem;
        private Extract.Imaging.Forms.ZoomPreviousToolStripMenuItem _zoomPreviousToolStripMenuItem;
        private Extract.Imaging.Forms.ZoomNextToolStripMenuItem _zoomNextToolStripMenuItem;
        private Extract.Imaging.Forms.RotateCounterclockwiseToolStripMenuItem _rotateCounterclockwiseToolStripMenuItem;
        private Extract.Imaging.Forms.RotateClockwiseToolStripMenuItem _rotateClockwiseToolStripMenuItem;
        private Extract.Imaging.Forms.FirstPageToolStripMenuItem _firstPageToolStripMenuItem;
        private Extract.Imaging.Forms.PreviousPageToolStripMenuItem _previousPageToolStripMenuItem;
        private Extract.Imaging.Forms.NextPageToolStripMenuItem _nextPageToolStripMenuItem;
        private Extract.Imaging.Forms.LastPageToolStripMenuItem _lastPageToolStripMenuItem;
        private Extract.Imaging.Forms.PreviousTileToolStripMenuItem _previousTileToolStripMenuItem;
        private Extract.Imaging.Forms.NextTileToolStripMenuItem _nextTileToolStripMenuItem;
        private Extract.Imaging.Forms.PreviousLayerObjectToolStripMenuItem _previousLayerObjectToolStripMenuItem;
        private Extract.Imaging.Forms.NextLayerObjectToolStripMenuItem _nextLayerObjectToolStripMenuItem;
        private Extract.Imaging.Forms.SelectLayerObjectToolStripMenuItem _selectLayerObjectToolStripMenuItem;
        private Extract.Imaging.Forms.ZoomWindowToolStripMenuItem _zoomWindowToolStripMenuItem;
        private Extract.Imaging.Forms.AngularRedactionToolStripMenuItem _angularRedactionToolStripMenuItem;
        private Extract.Imaging.Forms.RectangularRedactionToolStripMenuItem _rectangularRedactionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _pageSetupToolStripMenuItem;
        private Extract.Imaging.Forms.PrintPreviewToolStripMenuItem _previewToolStripMenuItem;
        private Extract.Imaging.Forms.PrintImageToolStripMenuItem _printImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator19;
        private System.Windows.Forms.ContextMenuStrip _idsoContextMenu;
        private Extract.Imaging.Forms.SelectLayerObjectToolStripMenuItem selectLayerObjectToolStripMenuItem1;
        private Extract.Imaging.Forms.ZoomWindowToolStripMenuItem zoomWindowToolStripMenuItem1;
        private Extract.Imaging.Forms.PanToolStripMenuItem panToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator20;
        private Extract.Imaging.Forms.ZoomPreviousToolStripMenuItem zoomPreviousToolStripMenuItem1;
        private Extract.Imaging.Forms.ZoomNextToolStripMenuItem zoomNextToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator21;
        private Extract.Imaging.Forms.RedactionToolStripMenuItem redactionToolStripMenuItem1;
        private System.Windows.Forms.ToolStrip _rotateCommandsToolStrip;
        private Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton _rotateCounterclockwiseToolStripButton;
        private Extract.Imaging.Forms.RotateClockwiseToolStripButton _rotateClockwiseToolStripButton;
        private Extract.Imaging.Forms.AngularRedactionToolStripButton _angularRedactionToolStripButton;
        private Extract.Imaging.Forms.RectangularRedactionToolStripButton _rectangularRedactionToolStripButton;
    }
}

