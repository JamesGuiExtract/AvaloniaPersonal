namespace Extract.UtilityApplications.PaginationUtility
{
    partial class PaginationUtilityForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PaginationUtilityForm));
            this._splitContainer = new System.Windows.Forms.SplitContainer();
            this._pageLayoutToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this._toolStrip = new System.Windows.Forms.ToolStrip();
            this._outputDocumentToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._outputFileNameToolStripTextBox = new Extract.Utilities.Forms.ToolStripSpringTextBox();
            this._outputFileNameBrowseToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._pagesToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            this._imageToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this._imageViewer = new Extract.Imaging.Forms.ImageViewer();
            this._basicTools = new System.Windows.Forms.ToolStrip();
            this._zoomWindowToolStripButton = new Extract.Imaging.Forms.ZoomWindowToolStripButton();
            this._panToolStripButton = new Extract.Imaging.Forms.PanToolStripButton();
            this._viewCommands = new System.Windows.Forms.ToolStrip();
            this._zoomInToolStripButton = new Extract.Imaging.Forms.ZoomInToolStripButton();
            this._zoomOutToolStripButton = new Extract.Imaging.Forms.ZoomOutToolStripButton();
            this._zoomPreviousToolStripButton = new Extract.Imaging.Forms.ZoomPreviousToolStripButton();
            this._zoomNextToolStripButton = new Extract.Imaging.Forms.ZoomNextToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this._fitToPageToolStripButton = new Extract.Imaging.Forms.FitToPageToolStripButton();
            this._fitToWidthToolStripButton = new Extract.Imaging.Forms.FitToWidthToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this._rotateCounterclockwiseToolStripButton = new Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton();
            this._rotateClockwiseToolStripButton = new Extract.Imaging.Forms.RotateClockwiseToolStripButton();
            this._sandDockManager = new TD.SandDock.SandDockManager();
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this._fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._loadNextDocumentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._outputSelectedDocumentsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._restartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._editMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._cutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._copyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._deleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._insertCopiedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._insertDocumentSeparatorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._pageLayoutToolStripContainer.TopToolStripPanel.SuspendLayout();
            this._pageLayoutToolStripContainer.SuspendLayout();
            this._toolStrip.SuspendLayout();
            this._imageToolStripContainer.ContentPanel.SuspendLayout();
            this._imageToolStripContainer.TopToolStripPanel.SuspendLayout();
            this._imageToolStripContainer.SuspendLayout();
            this._basicTools.SuspendLayout();
            this._viewCommands.SuspendLayout();
            this._menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _splitContainer
            // 
            this._splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._splitContainer.BackColor = System.Drawing.SystemColors.Control;
            this._splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this._splitContainer.Location = new System.Drawing.Point(0, 27);
            this._splitContainer.Margin = new System.Windows.Forms.Padding(0);
            this._splitContainer.Name = "_splitContainer";
            // 
            // _splitContainer.Panel1
            // 
            this._splitContainer.Panel1.Controls.Add(this._pageLayoutToolStripContainer);
            // 
            // _splitContainer.Panel2
            // 
            this._splitContainer.Panel2.Controls.Add(this._imageToolStripContainer);
            this._splitContainer.Size = new System.Drawing.Size(1088, 492);
            this._splitContainer.SplitterDistance = 555;
            this._splitContainer.TabIndex = 0;
            this._splitContainer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HandleSplitContainer_KeyDown);
            // 
            // _pageLayoutToolStripContainer
            // 
            this._pageLayoutToolStripContainer.BottomToolStripPanelVisible = false;
            // 
            // _pageLayoutToolStripContainer.ContentPanel
            // 
            this._pageLayoutToolStripContainer.ContentPanel.BackColor = System.Drawing.SystemColors.Control;
            this._pageLayoutToolStripContainer.ContentPanel.Size = new System.Drawing.Size(555, 453);
            this._pageLayoutToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pageLayoutToolStripContainer.LeftToolStripPanelVisible = false;
            this._pageLayoutToolStripContainer.Location = new System.Drawing.Point(0, 0);
            this._pageLayoutToolStripContainer.Name = "_pageLayoutToolStripContainer";
            this._pageLayoutToolStripContainer.RightToolStripPanelVisible = false;
            this._pageLayoutToolStripContainer.Size = new System.Drawing.Size(555, 492);
            this._pageLayoutToolStripContainer.TabIndex = 0;
            this._pageLayoutToolStripContainer.Text = "toolStripContainer1";
            // 
            // _pageLayoutToolStripContainer.TopToolStripPanel
            // 
            this._pageLayoutToolStripContainer.TopToolStripPanel.Controls.Add(this._toolStrip);
            // 
            // _toolStrip
            // 
            this._toolStrip.AutoSize = false;
            this._toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._outputDocumentToolStripButton,
            this._outputFileNameToolStripTextBox,
            this._outputFileNameBrowseToolStripButton,
            this._pagesToolStripLabel});
            this._toolStrip.Location = new System.Drawing.Point(0, 0);
            this._toolStrip.Name = "_toolStrip";
            this._toolStrip.Size = new System.Drawing.Size(555, 39);
            this._toolStrip.Stretch = true;
            this._toolStrip.TabIndex = 0;
            // 
            // _outputDocumentToolStripButton
            // 
            this._outputDocumentToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._outputDocumentToolStripButton.Image = global::Extract.UtilityApplications.PaginationUtility.Properties.Resources.SaveImageButton;
            this._outputDocumentToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._outputDocumentToolStripButton.Name = "_outputDocumentToolStripButton";
            this._outputDocumentToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._outputDocumentToolStripButton.Text = "Output document(s)";
            this._outputDocumentToolStripButton.ToolTipText = "Output selected document(s)  (Ctrl + S)";
            this._outputDocumentToolStripButton.Click += new System.EventHandler(this.HandleOutputDocuments_Click);
            // 
            // _outputFileNameToolStripTextBox
            // 
            this._outputFileNameToolStripTextBox.Name = "_outputFileNameToolStripTextBox";
            this._outputFileNameToolStripTextBox.Size = new System.Drawing.Size(392, 39);
            this._outputFileNameToolStripTextBox.Leave += new System.EventHandler(this.HandleOutputFileNameToolStripTextBox_Leave);
            this._outputFileNameToolStripTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.HandleOutputFileNameToolStripTextBox_Validating);
            this._outputFileNameToolStripTextBox.EnabledChanged += new System.EventHandler(this.HandleOutputFileNameToolStripTextBox_EnabledChanged);
            this._outputFileNameToolStripTextBox.TextChanged += new System.EventHandler(this.HandleOutputFileNameToolStripTextBox_TextChanged);
            // 
            // _outputFileNameBrowseToolStripButton
            // 
            this._outputFileNameBrowseToolStripButton.AutoSize = false;
            this._outputFileNameBrowseToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._outputFileNameBrowseToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_outputFileNameBrowseToolStripButton.Image")));
            this._outputFileNameBrowseToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._outputFileNameBrowseToolStripButton.Name = "_outputFileNameBrowseToolStripButton";
            this._outputFileNameBrowseToolStripButton.Size = new System.Drawing.Size(23, 36);
            this._outputFileNameBrowseToolStripButton.Text = "...";
            this._outputFileNameBrowseToolStripButton.Click += new System.EventHandler(this.HandleOutputFileNameBrowseToolStripButton_Click);
            // 
            // _pagesToolStripLabel
            // 
            this._pagesToolStripLabel.AutoSize = false;
            this._pagesToolStripLabel.Name = "_pagesToolStripLabel";
            this._pagesToolStripLabel.Size = new System.Drawing.Size(70, 36);
            this._pagesToolStripLabel.Text = "XXX pages";
            // 
            // _imageToolStripContainer
            // 
            this._imageToolStripContainer.BottomToolStripPanelVisible = false;
            // 
            // _imageToolStripContainer.ContentPanel
            // 
            this._imageToolStripContainer.ContentPanel.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this._imageToolStripContainer.ContentPanel.Controls.Add(this._imageViewer);
            this._imageToolStripContainer.ContentPanel.Size = new System.Drawing.Size(529, 453);
            this._imageToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._imageToolStripContainer.LeftToolStripPanelVisible = false;
            this._imageToolStripContainer.Location = new System.Drawing.Point(0, 0);
            this._imageToolStripContainer.Name = "_imageToolStripContainer";
            this._imageToolStripContainer.RightToolStripPanelVisible = false;
            this._imageToolStripContainer.Size = new System.Drawing.Size(529, 492);
            this._imageToolStripContainer.TabIndex = 0;
            this._imageToolStripContainer.Text = "toolStripContainer1";
            // 
            // _imageToolStripContainer.TopToolStripPanel
            // 
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._basicTools);
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._viewCommands);
            // 
            // _imageViewer
            // 
            this._imageViewer.AutoOcr = false;
            this._imageViewer.AutoZoomScale = 0;
            this._imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._imageViewer.FrameColor = System.Drawing.Color.Transparent;
            this._imageViewer.Location = new System.Drawing.Point(0, 0);
            this._imageViewer.MinimumAngularHighlightHeight = 16;
            this._imageViewer.MinimumSize = new System.Drawing.Size(5, 5);
            this._imageViewer.Name = "_imageViewer";
            this._imageViewer.OcrTradeoff = Extract.Imaging.OcrTradeoff.Accurate;
            this._imageViewer.RedactionMode = false;
            this._imageViewer.Size = new System.Drawing.Size(529, 453);
            this._imageViewer.TabIndex = 0;
            this._imageViewer.Text = "imageViewer1";
            this._imageViewer.UseDefaultShortcuts = true;
            // 
            // _basicTools
            // 
            this._basicTools.Dock = System.Windows.Forms.DockStyle.None;
            this._basicTools.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._basicTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomWindowToolStripButton,
            this._panToolStripButton});
            this._basicTools.Location = new System.Drawing.Point(3, 0);
            this._basicTools.Name = "_basicTools";
            this._basicTools.Size = new System.Drawing.Size(84, 39);
            this._basicTools.TabIndex = 14;
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
            this.toolStripSeparator9,
            this._rotateCounterclockwiseToolStripButton,
            this._rotateClockwiseToolStripButton});
            this._viewCommands.Location = new System.Drawing.Point(87, 0);
            this._viewCommands.Name = "_viewCommands";
            this._viewCommands.Size = new System.Drawing.Size(312, 39);
            this._viewCommands.TabIndex = 13;
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
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 39);
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
            // _sandDockManager
            // 
            this._sandDockManager.AllowMiddleButtonClosure = false;
            this._sandDockManager.AutoSaveLayout = true;
            this._sandDockManager.BorderStyle = TD.SandDock.Rendering.BorderStyle.None;
            this._sandDockManager.DockSystemContainer = this._splitContainer.Panel1;
            this._sandDockManager.OwnerForm = this;
            // 
            // _menuStrip
            // 
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileToolStripMenuItem,
            this._editMenuItem,
            this._toolsToolStripMenuItem,
            this._helpToolStripMenuItem});
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.Size = new System.Drawing.Size(1088, 24);
            this._menuStrip.TabIndex = 3;
            this._menuStrip.Text = "menuStrip1";
            // 
            // _fileToolStripMenuItem
            // 
            this._fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._loadNextDocumentMenuItem,
            this._outputSelectedDocumentsMenuItem,
            this.toolStripSeparator3,
            this._restartToolStripMenuItem,
            this.toolStripSeparator1,
            this._exitToolStripMenuItem});
            this._fileToolStripMenuItem.Name = "_fileToolStripMenuItem";
            this._fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this._fileToolStripMenuItem.Text = "&File";
            // 
            // _loadNextDocumentMenuItem
            // 
            this._loadNextDocumentMenuItem.Name = "_loadNextDocumentMenuItem";
            this._loadNextDocumentMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this._loadNextDocumentMenuItem.Size = new System.Drawing.Size(269, 22);
            this._loadNextDocumentMenuItem.Text = "&Load next document";
            this._loadNextDocumentMenuItem.Click += new System.EventHandler(this.HandleLoadNextDocumentMenuItem_Click);
            // 
            // _outputSelectedDocumentsMenuItem
            // 
            this._outputSelectedDocumentsMenuItem.Name = "_outputSelectedDocumentsMenuItem";
            this._outputSelectedDocumentsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this._outputSelectedDocumentsMenuItem.Size = new System.Drawing.Size(269, 22);
            this._outputSelectedDocumentsMenuItem.Text = "Output selected document(s)";
            this._outputSelectedDocumentsMenuItem.Click += new System.EventHandler(this.HandleOutputDocuments_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(266, 6);
            // 
            // _restartToolStripMenuItem
            // 
            this._restartToolStripMenuItem.Name = "_restartToolStripMenuItem";
            this._restartToolStripMenuItem.Size = new System.Drawing.Size(269, 22);
            this._restartToolStripMenuItem.Text = "&Restart";
            this._restartToolStripMenuItem.Click += new System.EventHandler(this.HandleRestartToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(266, 6);
            // 
            // _exitToolStripMenuItem
            // 
            this._exitToolStripMenuItem.Name = "_exitToolStripMenuItem";
            this._exitToolStripMenuItem.Size = new System.Drawing.Size(269, 22);
            this._exitToolStripMenuItem.Text = "E&xit";
            // 
            // _editMenuItem
            // 
            this._editMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._cutMenuItem,
            this._copyMenuItem,
            this._deleteMenuItem,
            this.toolStripSeparator2,
            this._insertCopiedMenuItem,
            this._insertDocumentSeparatorMenuItem});
            this._editMenuItem.Name = "_editMenuItem";
            this._editMenuItem.Size = new System.Drawing.Size(39, 20);
            this._editMenuItem.Text = "Edit";
            this._editMenuItem.DropDownOpening += new System.EventHandler(this.HandleEditMenuItem_DropDownOpening);
            // 
            // _cutMenuItem
            // 
            this._cutMenuItem.Name = "_cutMenuItem";
            this._cutMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this._cutMenuItem.Size = new System.Drawing.Size(264, 22);
            this._cutMenuItem.Text = "&Cut";
            this._cutMenuItem.Click += new System.EventHandler(this.HandleCutMenuItem_Click);
            // 
            // _copyMenuItem
            // 
            this._copyMenuItem.Name = "_copyMenuItem";
            this._copyMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this._copyMenuItem.Size = new System.Drawing.Size(264, 22);
            this._copyMenuItem.Text = "Cop&y";
            this._copyMenuItem.Click += new System.EventHandler(this.HandleCopyMenuItem_Click);
            // 
            // _deleteMenuItem
            // 
            this._deleteMenuItem.Name = "_deleteMenuItem";
            this._deleteMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this._deleteMenuItem.Size = new System.Drawing.Size(264, 22);
            this._deleteMenuItem.Text = "&Delete";
            this._deleteMenuItem.Click += new System.EventHandler(this.HandleDeleteMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(261, 6);
            // 
            // _insertCopiedMenuItem
            // 
            this._insertCopiedMenuItem.Name = "_insertCopiedMenuItem";
            this._insertCopiedMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this._insertCopiedMenuItem.Size = new System.Drawing.Size(264, 22);
            this._insertCopiedMenuItem.Text = "&Insert copied item(s)";
            this._insertCopiedMenuItem.Click += new System.EventHandler(this.HandleInsertCopiedMenuItem_Click);
            // 
            // _insertDocumentSeparatorMenuItem
            // 
            this._insertDocumentSeparatorMenuItem.Name = "_insertDocumentSeparatorMenuItem";
            this._insertDocumentSeparatorMenuItem.Size = new System.Drawing.Size(264, 22);
            this._insertDocumentSeparatorMenuItem.Text = "&Toggle document separator    Space";
            this._insertDocumentSeparatorMenuItem.Click += new System.EventHandler(this.HandleInsertDocumentSeparator_Click);
            // 
            // _toolsToolStripMenuItem
            // 
            this._toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._settingsToolStripMenuItem});
            this._toolsToolStripMenuItem.Name = "_toolsToolStripMenuItem";
            this._toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this._toolsToolStripMenuItem.Text = "&Tools";
            // 
            // _settingsToolStripMenuItem
            // 
            this._settingsToolStripMenuItem.Name = "_settingsToolStripMenuItem";
            this._settingsToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this._settingsToolStripMenuItem.Text = "&Settings...";
            this._settingsToolStripMenuItem.Click += new System.EventHandler(this.HandleSettingsMenuItem_Click);
            // 
            // _helpToolStripMenuItem
            // 
            this._helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._aboutToolStripMenuItem});
            this._helpToolStripMenuItem.Name = "_helpToolStripMenuItem";
            this._helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this._helpToolStripMenuItem.Text = "&Help";
            // 
            // _aboutToolStripMenuItem
            // 
            this._aboutToolStripMenuItem.Name = "_aboutToolStripMenuItem";
            this._aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this._aboutToolStripMenuItem.Text = "&About...";
            this._aboutToolStripMenuItem.Click += new System.EventHandler(this.HandleAboutToolStripMenuItem_Click);
            // 
            // PaginationUtilityForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.ClientSize = new System.Drawing.Size(1088, 519);
            this.Controls.Add(this._menuStrip);
            this.Controls.Add(this._splitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(575, 300);
            this.Name = "PaginationUtilityForm";
            this.Text = "Pagination Utility";
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this._pageLayoutToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this._pageLayoutToolStripContainer.ResumeLayout(false);
            this._pageLayoutToolStripContainer.PerformLayout();
            this._toolStrip.ResumeLayout(false);
            this._toolStrip.PerformLayout();
            this._imageToolStripContainer.ContentPanel.ResumeLayout(false);
            this._imageToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this._imageToolStripContainer.TopToolStripPanel.PerformLayout();
            this._imageToolStripContainer.ResumeLayout(false);
            this._imageToolStripContainer.PerformLayout();
            this._basicTools.ResumeLayout(false);
            this._basicTools.PerformLayout();
            this._viewCommands.ResumeLayout(false);
            this._viewCommands.PerformLayout();
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer _splitContainer;
        private System.Windows.Forms.ToolStripContainer _imageToolStripContainer;
        private System.Windows.Forms.ToolStrip _basicTools;
        private Imaging.Forms.ZoomWindowToolStripButton _zoomWindowToolStripButton;
        private Imaging.Forms.PanToolStripButton _panToolStripButton;
        private System.Windows.Forms.ToolStrip _viewCommands;
        private Imaging.Forms.ZoomInToolStripButton _zoomInToolStripButton;
        private Imaging.Forms.ZoomOutToolStripButton _zoomOutToolStripButton;
        private Imaging.Forms.ZoomPreviousToolStripButton _zoomPreviousToolStripButton;
        private Imaging.Forms.ZoomNextToolStripButton _zoomNextToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private Imaging.Forms.FitToPageToolStripButton _fitToPageToolStripButton;
        private Imaging.Forms.FitToWidthToolStripButton _fitToWidthToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private Imaging.Forms.RotateCounterclockwiseToolStripButton _rotateCounterclockwiseToolStripButton;
        private Imaging.Forms.RotateClockwiseToolStripButton _rotateClockwiseToolStripButton;
        private TD.SandDock.SandDockManager _sandDockManager;
        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.ToolStripMenuItem _fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripContainer _pageLayoutToolStripContainer;
        private System.Windows.Forms.ToolStrip _toolStrip;
        internal System.Windows.Forms.ToolStripButton _outputDocumentToolStripButton;
        private Extract.Imaging.Forms.ImageViewer _imageViewer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton _outputFileNameBrowseToolStripButton;
        private Utilities.Forms.ToolStripSpringTextBox _outputFileNameToolStripTextBox;
        private System.Windows.Forms.ToolStripLabel _pagesToolStripLabel;
        private System.Windows.Forms.ToolStripMenuItem _restartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _loadNextDocumentMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _editMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem _cutMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem _copyMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem _deleteMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        internal System.Windows.Forms.ToolStripMenuItem _insertCopiedMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem _insertDocumentSeparatorMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem _outputSelectedDocumentsMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}

