namespace Extract.FileActionManager.Utilities
{
    partial class FAMFileInspectorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// <para><b>NOTE:</b></para>
        /// For some reason with this form, if _searchSplitContainer is not initialized last, the
        /// components on the form get re-arranged when the form is displayed outside of the
        /// designer. Move the initialization of _searchSplitContainer and it's two panels to just
        /// above the FAMFileInspectorForm itself below after making changes in the designer.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            TD.SandDock.DockingRules dockingRules1 = new TD.SandDock.DockingRules();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FAMFileInspectorForm));
            this._selectFilesSummaryLabel = new System.Windows.Forms.Label();
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this._databaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._adminModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._logoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._databaseMenuToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this._exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._ffiHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._splitContainer = new Extract.Utilities.Forms.BetterSplitContainer();
            this._mainToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this._searchStatusStrip = new System.Windows.Forms.StatusStrip();
            this._searchStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._searchProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this._searchErrorStatusStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._mainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._okCancelPanel = new System.Windows.Forms.Panel();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._dataSplitContainer = new Extract.Utilities.Forms.BetterSplitContainer();
            this._refreshFileListButton = new System.Windows.Forms.Button();
            this._selectFilesButton = new System.Windows.Forms.Button();
            this._resultsSplitContainer = new Extract.Utilities.Forms.BetterSplitContainer();
            this._searchPanel = new System.Windows.Forms.Panel();
            this._closeSearchPaneButton = new System.Windows.Forms.Button();
            this._caseSensitiveSearchCheckBox = new System.Windows.Forms.CheckBox();
            this._regexSearchCheckBox = new System.Windows.Forms.CheckBox();
            this._searchModifierComboBox = new System.Windows.Forms.ComboBox();
            this._fuzzySearchCheckBox = new System.Windows.Forms.CheckBox();
            this._searchTypeComboBox = new System.Windows.Forms.ComboBox();
            this._dataSearchTermsDataGridView = new System.Windows.Forms.DataGridView();
            this._dataSearchFieldColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._dataSearchValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label3 = new System.Windows.Forms.Label();
            this._clearButton = new System.Windows.Forms.Button();
            this._textSearchTermsDataGridView = new System.Windows.Forms.DataGridView();
            this._textSearchTermsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._searchButton = new System.Windows.Forms.Button();
            this._resultsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._collapsedSearchPanel = new System.Windows.Forms.Panel();
            this._openSearchPaneButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this._fileListPanel = new System.Windows.Forms.Panel();
            this._fileListDataGridView = new System.Windows.Forms.DataGridView();
            this._fileListFlagColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this._fileListIDColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._fileListNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._fileListPagesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._fileListMatchesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._fileListFolderColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._showOnlyMatchesCheckBox = new System.Windows.Forms.CheckBox();
            this._imageToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this._imageViewerStatusStrip = new System.Windows.Forms.StatusStrip();
            this._layerObjectSelectionStatusLabel = new Extract.Imaging.Forms.StatusStripItems.LayerObjectSelectionStatusLabel();
            this._imageViewerErrorStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._zoomLevelToolStripStatusLabel = new Extract.Imaging.Forms.ZoomLevelToolStripStatusLabel();
            this._resolutionToolStripStatusLabel = new Extract.Imaging.Forms.ResolutionToolStripStatusLabel();
            this._mousePositionToolStripStatusLabel = new Extract.Imaging.Forms.MousePositionToolStripStatusLabel();
            this._imageViewer = new Extract.Imaging.Forms.ImageViewer();
            this._navigationToolsImageViewerToolStrip = new Extract.Imaging.Forms.NavigationToolsImageViewerToolStrip();
            this._viewCommandsImageViewerToolStrip = new Extract.Imaging.Forms.ViewCommandsImageViewerToolStrip();
            this._highlightNavigationToolStrip = new System.Windows.Forms.ToolStrip();
            this._previousLayerObjectToolStripButton = new Extract.Imaging.Forms.PreviousLayerObjectToolStripButton();
            this._nextLayerObjectToolStripButton = new Extract.Imaging.Forms.NextLayerObjectToolStripButton();
            this._imageViewerToolsToolStrip = new System.Windows.Forms.ToolStrip();
            this.zoomWindowToolStripButton1 = new Extract.Imaging.Forms.ZoomWindowToolStripButton();
            this.panToolStripButton1 = new Extract.Imaging.Forms.PanToolStripButton();
            this._generalImageToolStrip = new System.Windows.Forms.ToolStrip();
            this._printImageToolStripButton = new Extract.Imaging.Forms.PrintImageToolStripButton();
            this._thumbnailViewerToolStripButton = new Extract.Imaging.Forms.ThumbnailViewerToolStripButton();
            this._thumbnailDockableWindow = new TD.SandDock.DockableWindow();
            this._thumbnailViewer = new Extract.Imaging.Forms.ThumbnailViewer();
            this._invertColorsToolStripButton = new Extract.Imaging.Forms.InvertColorsToolStripButton();
            this._sandDockManager = new TD.SandDock.SandDockManager();
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this._dockContainer = new TD.SandDock.DockContainer();
            this._formToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this._customSearchTopDockContainer = new TD.SandDock.DockContainer();
            this._customSearchTopDockableWindow = new TD.SandDock.DockableWindow();
            this._voaPathExpressionLabel = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            this._menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._mainToolStripContainer.BottomToolStripPanel.SuspendLayout();
            this._mainToolStripContainer.ContentPanel.SuspendLayout();
            this._mainToolStripContainer.TopToolStripPanel.SuspendLayout();
            this._mainToolStripContainer.SuspendLayout();
            this._searchStatusStrip.SuspendLayout();
            this._mainLayoutPanel.SuspendLayout();
            this._okCancelPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dataSplitContainer)).BeginInit();
            this._dataSplitContainer.Panel1.SuspendLayout();
            this._dataSplitContainer.Panel2.SuspendLayout();
            this._dataSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._resultsSplitContainer)).BeginInit();
            this._resultsSplitContainer.Panel1.SuspendLayout();
            this._resultsSplitContainer.Panel2.SuspendLayout();
            this._resultsSplitContainer.SuspendLayout();
            this._searchPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dataSearchTermsDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._textSearchTermsDataGridView)).BeginInit();
            this._resultsTableLayoutPanel.SuspendLayout();
            this._collapsedSearchPanel.SuspendLayout();
            this._fileListPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._fileListDataGridView)).BeginInit();
            this._imageToolStripContainer.BottomToolStripPanel.SuspendLayout();
            this._imageToolStripContainer.ContentPanel.SuspendLayout();
            this._imageToolStripContainer.TopToolStripPanel.SuspendLayout();
            this._imageToolStripContainer.SuspendLayout();
            this._imageViewerStatusStrip.SuspendLayout();
            this._highlightNavigationToolStrip.SuspendLayout();
            this._imageViewerToolsToolStrip.SuspendLayout();
            this._generalImageToolStrip.SuspendLayout();
            this._thumbnailDockableWindow.SuspendLayout();
            this._dockContainer.SuspendLayout();
            this._formToolStripContainer.ContentPanel.SuspendLayout();
            this._formToolStripContainer.SuspendLayout();
            this._customSearchTopDockContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._selectFilesSummaryLabel);
            groupBox1.Location = new System.Drawing.Point(12, 10);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(372, 60);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            // 
            // _selectFilesSummaryLabel
            // 
            this._selectFilesSummaryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._selectFilesSummaryLabel.AutoEllipsis = true;
            this._selectFilesSummaryLabel.Location = new System.Drawing.Point(6, 12);
            this._selectFilesSummaryLabel.Name = "_selectFilesSummaryLabel";
            this._selectFilesSummaryLabel.Size = new System.Drawing.Size(360, 41);
            this._selectFilesSummaryLabel.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(188, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(78, 13);
            label1.TabIndex = 2;
            label1.Text = "of the following";
            // 
            // _menuStrip
            // 
            this._menuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._databaseToolStripMenuItem,
            this._helpToolStripMenuItem});
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.Size = new System.Drawing.Size(468, 24);
            this._menuStrip.TabIndex = 0;
            this._menuStrip.Text = "menuStrip1";
            // 
            // _databaseToolStripMenuItem
            // 
            this._databaseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._adminModeToolStripMenuItem,
            this._logoutToolStripMenuItem,
            this._databaseMenuToolStripSeparator,
            this._exitToolStripMenuItem});
            this._databaseToolStripMenuItem.Name = "_databaseToolStripMenuItem";
            this._databaseToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this._databaseToolStripMenuItem.Text = "&Database";
            // 
            // _adminModeToolStripMenuItem
            // 
            this._adminModeToolStripMenuItem.Name = "_adminModeToolStripMenuItem";
            this._adminModeToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this._adminModeToolStripMenuItem.Text = "&Admin mode";
            this._adminModeToolStripMenuItem.Click += new System.EventHandler(this.HandleAdminModeToolStripMenuItem_Click);
            // 
            // _logoutToolStripMenuItem
            // 
            this._logoutToolStripMenuItem.Name = "_logoutToolStripMenuItem";
            this._logoutToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this._logoutToolStripMenuItem.Text = "&Logout";
            this._logoutToolStripMenuItem.Click += new System.EventHandler(this.HandleLogoutToolStripMenuItem_Click);
            // 
            // _databaseMenuToolStripSeparator
            // 
            this._databaseMenuToolStripSeparator.Name = "_databaseMenuToolStripSeparator";
            this._databaseMenuToolStripSeparator.Size = new System.Drawing.Size(141, 6);
            // 
            // _exitToolStripMenuItem
            // 
            this._exitToolStripMenuItem.Name = "_exitToolStripMenuItem";
            this._exitToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this._exitToolStripMenuItem.Text = "E&xit";
            this._exitToolStripMenuItem.Click += new System.EventHandler(this.HandleExitToolStripMenuItem_Click);
            // 
            // _helpToolStripMenuItem
            // 
            this._helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._ffiHelpMenuItem,
            this._aboutToolStripMenuItem});
            this._helpToolStripMenuItem.Name = "_helpToolStripMenuItem";
            this._helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this._helpToolStripMenuItem.Text = "&Help";
            // 
            // _aboutToolStripMenuItem
            // 
            this._aboutToolStripMenuItem.Name = "_aboutToolStripMenuItem";
            this._aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this._aboutToolStripMenuItem.Text = "&About";
            this._aboutToolStripMenuItem.Click += new System.EventHandler(this.HandleAboutToolStripMenuItem_Click);
            //
            // _ffiHelpMenuItem
            //
            this._ffiHelpMenuItem.Name = "_ffiHelpMenuItem";
            this._ffiHelpMenuItem.Size = new System.Drawing.Size(107, 22);
            this._ffiHelpMenuItem.Text = "&FAM File Inspector Help";
            this._ffiHelpMenuItem.Click += new System.EventHandler(this.HandleFfiHelpMenuItem_Click);
            // 
            // _splitContainer
            // 
            this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this._splitContainer.Location = new System.Drawing.Point(0, 219);
            this._splitContainer.Margin = new System.Windows.Forms.Padding(0);
            this._splitContainer.Name = "_splitContainer";
            // 
            // _splitContainer.Panel1
            // 
            this._splitContainer.Panel1.Controls.Add(this._mainToolStripContainer);
            this._splitContainer.Panel1MinSize = 468;
            // 
            // _splitContainer.Panel2
            // 
            this._splitContainer.Panel2.Controls.Add(this._imageToolStripContainer);
            this._splitContainer.Panel2MinSize = 0;
            this._splitContainer.Size = new System.Drawing.Size(992, 416);
            this._splitContainer.SplitterDistance = 468;
            this._splitContainer.TabIndex = 1;
            this._splitContainer.TabStop = false;
            // 
            // _mainToolStripContainer
            // 
            // 
            // _mainToolStripContainer.BottomToolStripPanel
            // 
            this._mainToolStripContainer.BottomToolStripPanel.Controls.Add(this._searchStatusStrip);
            // 
            // _mainToolStripContainer.ContentPanel
            // 
            this._mainToolStripContainer.ContentPanel.Controls.Add(this._mainLayoutPanel);
            this._mainToolStripContainer.ContentPanel.Size = new System.Drawing.Size(468, 370);
            this._mainToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // _mainToolStripContainer.LeftToolStripPanel
            // 
            this._mainToolStripContainer.LeftToolStripPanel.Enabled = true;
            this._mainToolStripContainer.Location = new System.Drawing.Point(0, 0);
            this._mainToolStripContainer.Name = "_mainToolStripContainer";
            // 
            // _mainToolStripContainer.RightToolStripPanel
            // 
            this._mainToolStripContainer.RightToolStripPanel.Enabled = true;
            this._mainToolStripContainer.Size = new System.Drawing.Size(468, 416);
            this._mainToolStripContainer.TabIndex = 0;
            this._mainToolStripContainer.Text = "toolStripContainer1";
            // 
            // _mainToolStripContainer.TopToolStripPanel
            // 
            this._mainToolStripContainer.TopToolStripPanel.Controls.Add(this._menuStrip);
            // 
            // _searchStatusStrip
            // 
            this._searchStatusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._searchStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._searchStatusLabel,
            this._searchProgressBar,
            this._searchErrorStatusStripLabel});
            this._searchStatusStrip.Location = new System.Drawing.Point(0, 0);
            this._searchStatusStrip.Name = "_searchStatusStrip";
            this._searchStatusStrip.Size = new System.Drawing.Size(468, 22);
            this._searchStatusStrip.TabIndex = 0;
            // 
            // _searchStatusLabel
            // 
            this._searchStatusLabel.AutoSize = false;
            this._searchStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._searchStatusLabel.Name = "_searchStatusLabel";
            this._searchStatusLabel.Size = new System.Drawing.Size(1, 17);
            this._searchStatusLabel.Spring = true;
            this._searchStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _searchProgressBar
            // 
            this._searchProgressBar.AutoSize = false;
            this._searchProgressBar.Name = "_searchProgressBar";
            this._searchProgressBar.Size = new System.Drawing.Size(233, 16);
            this._searchProgressBar.Step = 1;
            this._searchProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // _searchErrorStatusStripLabel
            // 
            this._searchErrorStatusStripLabel.AutoSize = false;
            this._searchErrorStatusStripLabel.ForeColor = System.Drawing.Color.Red;
            this._searchErrorStatusStripLabel.Name = "_searchErrorStatusStripLabel";
            this._searchErrorStatusStripLabel.Size = new System.Drawing.Size(233, 17);
            // 
            // _mainLayoutPanel
            // 
            this._mainLayoutPanel.ColumnCount = 1;
            this._mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._mainLayoutPanel.Controls.Add(this._okCancelPanel, 0, 1);
            this._mainLayoutPanel.Controls.Add(this._dataSplitContainer, 0, 0);
            this._mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._mainLayoutPanel.Name = "_mainLayoutPanel";
            this._mainLayoutPanel.RowCount = 2;
            this._mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this._mainLayoutPanel.Size = new System.Drawing.Size(468, 370);
            this._mainLayoutPanel.TabIndex = 3;
            // 
            // _okCancelPanel
            // 
            this._okCancelPanel.Controls.Add(this._cancelButton);
            this._okCancelPanel.Controls.Add(this._okButton);
            this._okCancelPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._okCancelPanel.Location = new System.Drawing.Point(0, 342);
            this._okCancelPanel.Margin = new System.Windows.Forms.Padding(0);
            this._okCancelPanel.Name = "_okCancelPanel";
            this._okCancelPanel.Size = new System.Drawing.Size(468, 28);
            this._okCancelPanel.TabIndex = 0;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(390, 2);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 1;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            this._cancelButton.Click += new System.EventHandler(this.HandleCancelButton_Click);
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(309, 2);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 0;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _dataSplitContainer
            // 
            this._dataSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dataSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this._dataSplitContainer.Location = new System.Drawing.Point(0, 0);
            this._dataSplitContainer.Margin = new System.Windows.Forms.Padding(0);
            this._dataSplitContainer.Name = "_dataSplitContainer";
            this._dataSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _dataSplitContainer.Panel1
            // 
            this._dataSplitContainer.Panel1.Controls.Add(this._refreshFileListButton);
            this._dataSplitContainer.Panel1.Controls.Add(groupBox1);
            this._dataSplitContainer.Panel1.Controls.Add(this._selectFilesButton);
            this._dataSplitContainer.Panel1MinSize = 78;
            // 
            // _dataSplitContainer.Panel2
            // 
            this._dataSplitContainer.Panel2.Controls.Add(this._resultsSplitContainer);
            this._dataSplitContainer.Panel2MinSize = 0;
            this._dataSplitContainer.Size = new System.Drawing.Size(468, 342);
            this._dataSplitContainer.SplitterDistance = 78;
            this._dataSplitContainer.TabIndex = 0;
            this._dataSplitContainer.TabStop = false;
            // 
            // _refreshFileListButton
            // 
            this._refreshFileListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._refreshFileListButton.Location = new System.Drawing.Point(390, 46);
            this._refreshFileListButton.Name = "_refreshFileListButton";
            this._refreshFileListButton.Size = new System.Drawing.Size(75, 23);
            this._refreshFileListButton.TabIndex = 2;
            this._refreshFileListButton.Text = "Refresh";
            this._refreshFileListButton.UseVisualStyleBackColor = true;
            this._refreshFileListButton.Click += new System.EventHandler(this.HandleRefreshFileListButton_Click);
            // 
            // _selectFilesButton
            // 
            this._selectFilesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._selectFilesButton.Location = new System.Drawing.Point(390, 17);
            this._selectFilesButton.Name = "_selectFilesButton";
            this._selectFilesButton.Size = new System.Drawing.Size(75, 23);
            this._selectFilesButton.TabIndex = 1;
            this._selectFilesButton.Text = "Change...";
            this._selectFilesButton.UseVisualStyleBackColor = true;
            this._selectFilesButton.Click += new System.EventHandler(this.HandleSelectFilesButton_Click);
            // 
            // _resultsSplitContainer
            // 
            this._resultsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._resultsSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this._resultsSplitContainer.Location = new System.Drawing.Point(0, 0);
            this._resultsSplitContainer.Name = "_resultsSplitContainer";
            this._resultsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _resultsSplitContainer.Panel1
            // 
            this._resultsSplitContainer.Panel1.Controls.Add(this._searchPanel);
            this._resultsSplitContainer.Panel1MinSize = 111;
            // 
            // _resultsSplitContainer.Panel2
            // 
            this._resultsSplitContainer.Panel2.Controls.Add(this._resultsTableLayoutPanel);
            this._resultsSplitContainer.Size = new System.Drawing.Size(468, 260);
            this._resultsSplitContainer.SplitterDistance = 117;
            this._resultsSplitContainer.TabIndex = 3;
            // 
            // _searchPanel
            // 
            this._searchPanel.Controls.Add(this._voaPathExpressionLabel);
            this._searchPanel.Controls.Add(this._closeSearchPaneButton);
            this._searchPanel.Controls.Add(this._caseSensitiveSearchCheckBox);
            this._searchPanel.Controls.Add(label1);
            this._searchPanel.Controls.Add(this._regexSearchCheckBox);
            this._searchPanel.Controls.Add(this._searchModifierComboBox);
            this._searchPanel.Controls.Add(this._fuzzySearchCheckBox);
            this._searchPanel.Controls.Add(this._searchTypeComboBox);
            this._searchPanel.Controls.Add(this._dataSearchTermsDataGridView);
            this._searchPanel.Controls.Add(this.label3);
            this._searchPanel.Controls.Add(this._clearButton);
            this._searchPanel.Controls.Add(this._textSearchTermsDataGridView);
            this._searchPanel.Controls.Add(this._searchButton);
            this._searchPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._searchPanel.Location = new System.Drawing.Point(0, 0);
            this._searchPanel.Name = "_searchPanel";
            this._searchPanel.Size = new System.Drawing.Size(468, 117);
            this._searchPanel.TabIndex = 2;
            // 
            // _closeSearchPaneButton
            // 
            this._closeSearchPaneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._closeSearchPaneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(154)))), ((int)(((byte)(144)))));
            this._closeSearchPaneButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this._closeSearchPaneButton.FlatAppearance.BorderSize = 0;
            this._closeSearchPaneButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(121)))), ((int)(((byte)(100)))));
            this._closeSearchPaneButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(121)))), ((int)(((byte)(100)))));
            this._closeSearchPaneButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this._closeSearchPaneButton.Image = global::Extract.FileActionManager.Utilities.Properties.Resources.Close;
            this._closeSearchPaneButton.Location = new System.Drawing.Point(450, 6);
            this._closeSearchPaneButton.Name = "_closeSearchPaneButton";
            this._closeSearchPaneButton.Size = new System.Drawing.Size(14, 14);
            this._closeSearchPaneButton.TabIndex = 10;
            this._closeSearchPaneButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this._closeSearchPaneButton.UseVisualStyleBackColor = false;
            this._closeSearchPaneButton.Click += new System.EventHandler(this.HandleCloseSearchPaneButton_Click);
            // 
            // _caseSensitiveSearchCheckBox
            // 
            this._caseSensitiveSearchCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._caseSensitiveSearchCheckBox.AutoSize = true;
            this._caseSensitiveSearchCheckBox.Location = new System.Drawing.Point(7, 94);
            this._caseSensitiveSearchCheckBox.Name = "_caseSensitiveSearchCheckBox";
            this._caseSensitiveSearchCheckBox.Size = new System.Drawing.Size(82, 17);
            this._caseSensitiveSearchCheckBox.TabIndex = 5;
            this._caseSensitiveSearchCheckBox.Text = "Match case";
            this._caseSensitiveSearchCheckBox.UseVisualStyleBackColor = true;
            // 
            // _regexSearchCheckBox
            // 
            this._regexSearchCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._regexSearchCheckBox.AutoSize = true;
            this._regexSearchCheckBox.Location = new System.Drawing.Point(188, 94);
            this._regexSearchCheckBox.Name = "_regexSearchCheckBox";
            this._regexSearchCheckBox.Size = new System.Drawing.Size(116, 17);
            this._regexSearchCheckBox.TabIndex = 7;
            this._regexSearchCheckBox.Text = "Regular expression";
            this._regexSearchCheckBox.UseVisualStyleBackColor = true;
            // 
            // _searchModifierComboBox
            // 
            this._searchModifierComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._searchModifierComboBox.FormattingEnabled = true;
            this._searchModifierComboBox.Items.AddRange(new object[] {
            "any",
            "all",
            "none"});
            this._searchModifierComboBox.Location = new System.Drawing.Point(125, 6);
            this._searchModifierComboBox.Name = "_searchModifierComboBox";
            this._searchModifierComboBox.Size = new System.Drawing.Size(57, 21);
            this._searchModifierComboBox.TabIndex = 1;
            // 
            // _fuzzySearchCheckBox
            // 
            this._fuzzySearchCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._fuzzySearchCheckBox.AutoSize = true;
            this._fuzzySearchCheckBox.Location = new System.Drawing.Point(94, 94);
            this._fuzzySearchCheckBox.Name = "_fuzzySearchCheckBox";
            this._fuzzySearchCheckBox.Size = new System.Drawing.Size(88, 17);
            this._fuzzySearchCheckBox.TabIndex = 6;
            this._fuzzySearchCheckBox.Text = "Fuzzy search";
            this._fuzzySearchCheckBox.UseVisualStyleBackColor = true;
            this._fuzzySearchCheckBox.CheckedChanged += new System.EventHandler(this.HandleFuzzySearchCheckBox_CheckedChanged);
            // 
            // _searchTypeComboBox
            // 
            this._searchTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._searchTypeComboBox.FormattingEnabled = true;
            this._searchTypeComboBox.Items.AddRange(new object[] {
            "words",
            "extracted data"});
            this._searchTypeComboBox.Location = new System.Drawing.Point(272, 6);
            this._searchTypeComboBox.Name = "_searchTypeComboBox";
            this._searchTypeComboBox.Size = new System.Drawing.Size(96, 21);
            this._searchTypeComboBox.TabIndex = 3;
            this._searchTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleSearchTypeComboBox_SelectedIndexChanged);
            // 
            // _dataSearchTermsDataGridView
            // 
            this._dataSearchTermsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._dataSearchTermsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this._dataSearchTermsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dataSearchTermsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._dataSearchFieldColumn,
            this._dataSearchValueColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._dataSearchTermsDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this._dataSearchTermsDataGridView.Location = new System.Drawing.Point(3, 33);
            this._dataSearchTermsDataGridView.Name = "_dataSearchTermsDataGridView";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._dataSearchTermsDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this._dataSearchTermsDataGridView.Size = new System.Drawing.Size(462, 51);
            this._dataSearchTermsDataGridView.TabIndex = 4;
            this._dataSearchTermsDataGridView.Visible = false;
            // 
            // _dataSearchFieldColumn
            // 
            this._dataSearchFieldColumn.HeaderText = "Field";
            this._dataSearchFieldColumn.Name = "_dataSearchFieldColumn";
            this._dataSearchFieldColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this._dataSearchFieldColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this._dataSearchFieldColumn.Width = 200;
            // 
            // _dataSearchValueColumn
            // 
            this._dataSearchValueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._dataSearchValueColumn.HeaderText = "Value";
            this._dataSearchValueColumn.Name = "_dataSearchValueColumn";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Search above files for";
            // 
            // _clearButton
            // 
            this._clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._clearButton.Location = new System.Drawing.Point(390, 90);
            this._clearButton.Name = "_clearButton";
            this._clearButton.Size = new System.Drawing.Size(75, 23);
            this._clearButton.TabIndex = 9;
            this._clearButton.Text = "Clear";
            this._clearButton.UseVisualStyleBackColor = true;
            this._clearButton.Click += new System.EventHandler(this.HandleClearButton_Click);
            // 
            // _textSearchTermsDataGridView
            // 
            this._textSearchTermsDataGridView.AllowUserToResizeRows = false;
            this._textSearchTermsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._textSearchTermsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this._textSearchTermsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._textSearchTermsDataGridView.ColumnHeadersVisible = false;
            this._textSearchTermsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._textSearchTermsColumn});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._textSearchTermsDataGridView.DefaultCellStyle = dataGridViewCellStyle5;
            this._textSearchTermsDataGridView.Location = new System.Drawing.Point(3, 33);
            this._textSearchTermsDataGridView.Name = "_textSearchTermsDataGridView";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._textSearchTermsDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this._textSearchTermsDataGridView.Size = new System.Drawing.Size(462, 51);
            this._textSearchTermsDataGridView.TabIndex = 4;
            // 
            // _textSearchTermsColumn
            // 
            this._textSearchTermsColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._textSearchTermsColumn.Name = "_textSearchTermsColumn";
            // 
            // _searchButton
            // 
            this._searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._searchButton.Location = new System.Drawing.Point(309, 90);
            this._searchButton.Name = "_searchButton";
            this._searchButton.Size = new System.Drawing.Size(75, 23);
            this._searchButton.TabIndex = 8;
            this._searchButton.Text = "Search";
            this._searchButton.UseVisualStyleBackColor = true;
            this._searchButton.Click += new System.EventHandler(this.HandleSearchButton_Click);
            // 
            // _resultsTableLayoutPanel
            // 
            this._resultsTableLayoutPanel.ColumnCount = 1;
            this._resultsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._resultsTableLayoutPanel.Controls.Add(this._collapsedSearchPanel, 0, 0);
            this._resultsTableLayoutPanel.Controls.Add(this._fileListPanel, 0, 1);
            this._resultsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._resultsTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._resultsTableLayoutPanel.Name = "_resultsTableLayoutPanel";
            this._resultsTableLayoutPanel.RowCount = 2;
            this._resultsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this._resultsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._resultsTableLayoutPanel.Size = new System.Drawing.Size(468, 139);
            this._resultsTableLayoutPanel.TabIndex = 2;
            // 
            // _collapsedSearchPanel
            // 
            this._collapsedSearchPanel.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this._collapsedSearchPanel.Controls.Add(this._openSearchPaneButton);
            this._collapsedSearchPanel.Controls.Add(this.label2);
            this._collapsedSearchPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._collapsedSearchPanel.Location = new System.Drawing.Point(0, 0);
            this._collapsedSearchPanel.Margin = new System.Windows.Forms.Padding(0);
            this._collapsedSearchPanel.Name = "_collapsedSearchPanel";
            this._collapsedSearchPanel.Size = new System.Drawing.Size(468, 21);
            this._collapsedSearchPanel.TabIndex = 11;
            // 
            // _openSearchPaneButton
            // 
            this._openSearchPaneButton.BackColor = System.Drawing.SystemColors.Control;
            this._openSearchPaneButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this._openSearchPaneButton.FlatAppearance.BorderSize = 0;
            this._openSearchPaneButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.Control;
            this._openSearchPaneButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this._openSearchPaneButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this._openSearchPaneButton.Image = global::Extract.FileActionManager.Utilities.Properties.Resources.Expand;
            this._openSearchPaneButton.Location = new System.Drawing.Point(7, 5);
            this._openSearchPaneButton.Name = "_openSearchPaneButton";
            this._openSearchPaneButton.Size = new System.Drawing.Size(12, 12);
            this._openSearchPaneButton.TabIndex = 11;
            this._openSearchPaneButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this._openSearchPaneButton.UseVisualStyleBackColor = false;
            this._openSearchPaneButton.Click += new System.EventHandler(this.HandleOpenSearchPaneButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(22, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Search";
            // 
            // _fileListPanel
            // 
            this._fileListPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._fileListPanel.Controls.Add(this._fileListDataGridView);
            this._fileListPanel.Controls.Add(this._showOnlyMatchesCheckBox);
            this._fileListPanel.Location = new System.Drawing.Point(0, 21);
            this._fileListPanel.Margin = new System.Windows.Forms.Padding(0);
            this._fileListPanel.Name = "_fileListPanel";
            this._fileListPanel.Size = new System.Drawing.Size(468, 337);
            this._fileListPanel.TabIndex = 1;
            // 
            // _fileListDataGridView
            // 
            this._fileListDataGridView.AllowUserToAddRows = false;
            this._fileListDataGridView.AllowUserToDeleteRows = false;
            this._fileListDataGridView.AllowUserToResizeRows = false;
            this._fileListDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._fileListDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this._fileListDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._fileListDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._fileListFlagColumn,
            this._fileListIDColumn,
            this._fileListNameColumn,
            this._fileListPagesColumn,
            this._fileListMatchesColumn,
            this._fileListFolderColumn});
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._fileListDataGridView.DefaultCellStyle = dataGridViewCellStyle8;
            this._fileListDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._fileListDataGridView.Location = new System.Drawing.Point(3, 26);
            this._fileListDataGridView.Name = "_fileListDataGridView";
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._fileListDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this._fileListDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._fileListDataGridView.Size = new System.Drawing.Size(461, 308);
            this._fileListDataGridView.TabIndex = 1;
            this._fileListDataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleFileListDataGridView_CellDoubleClick);
            this._fileListDataGridView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.HandleResultsDataGridView_CellPainting);
            this._fileListDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleFileListDataGridView_CellValueChanged);
            this._fileListDataGridView.CurrentCellChanged += new System.EventHandler(this.HandleResultsDataGridView_CurrentCellChanged);
            this._fileListDataGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.HandleFileListDataGridView_CurrentCellDirtyStateChanged);
            this._fileListDataGridView.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.HandleFileListDataGridView_EditingControlShowing);
            this._fileListDataGridView.SelectionChanged += new System.EventHandler(this.HandleFileListDataGridView_SelectionChanged);
            this._fileListDataGridView.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(this.HandleFileListDataGridView_SortCompare);
            this._fileListDataGridView.SizeChanged += new System.EventHandler(this.HandleFileListDataGridView_SizeChanged);
            this._fileListDataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HandleFileListDataGridView_KeyDown);
            this._fileListDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HandleFileListDataGridView_MouseDown);
            this._fileListDataGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleFileListDataGridView_MouseMove);
            this._fileListDataGridView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.HandleFileListDataGridView_MouseUp);
            // 
            // _fileListFlagColumn
            // 
            this._fileListFlagColumn.HeaderText = "";
            this._fileListFlagColumn.Name = "_fileListFlagColumn";
            this._fileListFlagColumn.ReadOnly = true;
            this._fileListFlagColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this._fileListFlagColumn.Width = 20;
            // 
            // _fileListIDColumn
            // 
            this._fileListIDColumn.FillWeight = 50F;
            this._fileListIDColumn.HeaderText = "ID";
            this._fileListIDColumn.Name = "_fileListIDColumn";
            this._fileListIDColumn.ReadOnly = true;
            this._fileListIDColumn.Width = 50;
            // 
            // _fileListNameColumn
            // 
            this._fileListNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._fileListNameColumn.HeaderText = "Filename";
            this._fileListNameColumn.Name = "_fileListNameColumn";
            this._fileListNameColumn.ReadOnly = true;
            this._fileListNameColumn.Width = 225;
            // 
            // _fileListPagesColumn
            // 
            this._fileListPagesColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._fileListPagesColumn.HeaderText = "Pages";
            this._fileListPagesColumn.Name = "_fileListPagesColumn";
            this._fileListPagesColumn.ReadOnly = true;
            this._fileListPagesColumn.Width = 50;
            // 
            // _fileListMatchesColumn
            // 
            this._fileListMatchesColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._fileListMatchesColumn.HeaderText = "Matches";
            this._fileListMatchesColumn.Name = "_fileListMatchesColumn";
            this._fileListMatchesColumn.ReadOnly = true;
            this._fileListMatchesColumn.Width = 60;
            // 
            // _fileListFolderColumn
            // 
            this._fileListFolderColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._fileListFolderColumn.HeaderText = "Folder";
            this._fileListFolderColumn.Name = "_fileListFolderColumn";
            this._fileListFolderColumn.ReadOnly = true;
            // 
            // _showOnlyMatchesCheckBox
            // 
            this._showOnlyMatchesCheckBox.AutoSize = true;
            this._showOnlyMatchesCheckBox.Enabled = false;
            this._showOnlyMatchesCheckBox.Location = new System.Drawing.Point(7, 3);
            this._showOnlyMatchesCheckBox.Name = "_showOnlyMatchesCheckBox";
            this._showOnlyMatchesCheckBox.Size = new System.Drawing.Size(189, 17);
            this._showOnlyMatchesCheckBox.TabIndex = 0;
            this._showOnlyMatchesCheckBox.Text = "Show only matching search results";
            this._showOnlyMatchesCheckBox.UseVisualStyleBackColor = true;
            this._showOnlyMatchesCheckBox.CheckedChanged += new System.EventHandler(this.HandleShowOnlyMatchesCheckBox_CheckedChanged);
            // 
            // _imageToolStripContainer
            // 
            // 
            // _imageToolStripContainer.BottomToolStripPanel
            // 
            this._imageToolStripContainer.BottomToolStripPanel.Controls.Add(this._imageViewerStatusStrip);
            // 
            // _imageToolStripContainer.ContentPanel
            // 
            this._imageToolStripContainer.ContentPanel.Controls.Add(this._imageViewer);
            this._imageToolStripContainer.ContentPanel.Size = new System.Drawing.Size(520, 316);
            this._imageToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // _imageToolStripContainer.LeftToolStripPanel
            // 
            this._imageToolStripContainer.LeftToolStripPanel.Enabled = true;
            this._imageToolStripContainer.Location = new System.Drawing.Point(0, 0);
            this._imageToolStripContainer.Name = "_imageToolStripContainer";
            // 
            // _imageToolStripContainer.RightToolStripPanel
            // 
            this._imageToolStripContainer.RightToolStripPanel.Enabled = true;
            this._imageToolStripContainer.Size = new System.Drawing.Size(520, 416);
            this._imageToolStripContainer.TabIndex = 0;
            this._imageToolStripContainer.Text = "toolStripContainer1";
            // 
            // _imageToolStripContainer.TopToolStripPanel
            // 
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._highlightNavigationToolStrip);
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._imageViewerToolsToolStrip);
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._navigationToolsImageViewerToolStrip);
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._viewCommandsImageViewerToolStrip);
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._generalImageToolStrip);
            // 
            // _imageViewerStatusStrip
            // 
            this._imageViewerStatusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._imageViewerStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._layerObjectSelectionStatusLabel,
            this._imageViewerErrorStripStatusLabel,
            this._zoomLevelToolStripStatusLabel,
            this._resolutionToolStripStatusLabel,
            this._mousePositionToolStripStatusLabel});
            this._imageViewerStatusStrip.Location = new System.Drawing.Point(0, 0);
            this._imageViewerStatusStrip.Name = "_imageViewerStatusStrip";
            this._imageViewerStatusStrip.Size = new System.Drawing.Size(520, 22);
            this._imageViewerStatusStrip.TabIndex = 1;
            this._imageViewerStatusStrip.Text = "statusStrip1";
            // 
            // _layerObjectSelectionStatusLabel
            // 
            this._layerObjectSelectionStatusLabel.ImageViewer = null;
            this._layerObjectSelectionStatusLabel.LayerObjectName = "Match";
            this._layerObjectSelectionStatusLabel.Name = "_layerObjectSelectionStatusLabel";
            this._layerObjectSelectionStatusLabel.Size = new System.Drawing.Size(129, 17);
            this._layerObjectSelectionStatusLabel.Text = "(Layer object selection)";
            // 
            // _imageViewerErrorStripStatusLabel
            // 
            this._imageViewerErrorStripStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this._imageViewerErrorStripStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this._imageViewerErrorStripStatusLabel.ForeColor = System.Drawing.Color.Red;
            this._imageViewerErrorStripStatusLabel.Name = "_imageViewerErrorStripStatusLabel";
            this._imageViewerErrorStripStatusLabel.Size = new System.Drawing.Size(136, 17);
            this._imageViewerErrorStripStatusLabel.Spring = true;
            this._imageViewerErrorStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _zoomLevelToolStripStatusLabel
            // 
            this._zoomLevelToolStripStatusLabel.AutoSize = false;
            this._zoomLevelToolStripStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this._zoomLevelToolStripStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this._zoomLevelToolStripStatusLabel.ImageViewer = null;
            this._zoomLevelToolStripStatusLabel.Name = "_zoomLevelToolStripStatusLabel";
            this._zoomLevelToolStripStatusLabel.Size = new System.Drawing.Size(80, 17);
            this._zoomLevelToolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _resolutionToolStripStatusLabel
            // 
            this._resolutionToolStripStatusLabel.AutoSize = false;
            this._resolutionToolStripStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this._resolutionToolStripStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this._resolutionToolStripStatusLabel.ImageViewer = null;
            this._resolutionToolStripStatusLabel.Name = "_resolutionToolStripStatusLabel";
            this._resolutionToolStripStatusLabel.Size = new System.Drawing.Size(60, 17);
            // 
            // _mousePositionToolStripStatusLabel
            // 
            this._mousePositionToolStripStatusLabel.AutoSize = false;
            this._mousePositionToolStripStatusLabel.DisplayOption = Extract.Imaging.Forms.MousePositionDisplayOption.Registry;
            this._mousePositionToolStripStatusLabel.ImageViewer = null;
            this._mousePositionToolStripStatusLabel.Name = "_mousePositionToolStripStatusLabel";
            this._mousePositionToolStripStatusLabel.Size = new System.Drawing.Size(100, 17);
            this._mousePositionToolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _imageViewer
            // 
            this._imageViewer.AutoOcr = false;
            this._imageViewer.AutoZoomScale = 0;
            this._imageViewer.DisplayAnnotations = false;
            this._imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._imageViewer.InvertColors = false;
            this._imageViewer.Location = new System.Drawing.Point(0, 0);
            this._imageViewer.MinimumAngularHighlightHeight = 4;
            this._imageViewer.Name = "_imageViewer";
            this._imageViewer.OcrTradeoff = Extract.Imaging.OcrTradeoff.Accurate;
            this._imageViewer.RedactionMode = false;
            this._imageViewer.Size = new System.Drawing.Size(520, 316);
            this._imageViewer.TabIndex = 0;
            this._imageViewer.TabStop = false;
            // 
            // _navigationToolsImageViewerToolStrip
            // 
            this._navigationToolsImageViewerToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._navigationToolsImageViewerToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._navigationToolsImageViewerToolStrip.Location = new System.Drawing.Point(171, 0);
            this._navigationToolsImageViewerToolStrip.Name = "_navigationToolsImageViewerToolStrip";
            this._navigationToolsImageViewerToolStrip.Size = new System.Drawing.Size(233, 39);
            this._navigationToolsImageViewerToolStrip.TabIndex = 2;
            // 
            // _viewCommandsImageViewerToolStrip
            // 
            this._viewCommandsImageViewerToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._viewCommandsImageViewerToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._viewCommandsImageViewerToolStrip.Location = new System.Drawing.Point(3, 39);
            this._viewCommandsImageViewerToolStrip.Name = "_viewCommandsImageViewerToolStrip";
            this._viewCommandsImageViewerToolStrip.Size = new System.Drawing.Size(348, 39);
            this._viewCommandsImageViewerToolStrip.TabIndex = 1;
            // 
            // _highlightNavigationToolStrip
            // 
            this._highlightNavigationToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._highlightNavigationToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._highlightNavigationToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._previousLayerObjectToolStripButton,
            this._nextLayerObjectToolStripButton});
            this._highlightNavigationToolStrip.Location = new System.Drawing.Point(3, 0);
            this._highlightNavigationToolStrip.Name = "_highlightNavigationToolStrip";
            this._highlightNavigationToolStrip.Size = new System.Drawing.Size(84, 39);
            this._highlightNavigationToolStrip.TabIndex = 4;
            // 
            // _previousLayerObjectToolStripButton
            // 
            this._previousLayerObjectToolStripButton.BaseToolTipText = "Go to previous match";
            this._previousLayerObjectToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._previousLayerObjectToolStripButton.Enabled = false;
            this._previousLayerObjectToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._previousLayerObjectToolStripButton.ImageViewer = null;
            this._previousLayerObjectToolStripButton.Name = "_previousLayerObjectToolStripButton";
            this._previousLayerObjectToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._previousLayerObjectToolStripButton.Text = "Go to previous match";
            // 
            // _nextLayerObjectToolStripButton
            // 
            this._nextLayerObjectToolStripButton.BaseToolTipText = "Go to next match";
            this._nextLayerObjectToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextLayerObjectToolStripButton.Enabled = false;
            this._nextLayerObjectToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextLayerObjectToolStripButton.ImageViewer = null;
            this._nextLayerObjectToolStripButton.Name = "_nextLayerObjectToolStripButton";
            this._nextLayerObjectToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextLayerObjectToolStripButton.Text = "Go to next match";
            // 
            // _imageViewerToolsToolStrip
            // 
            this._imageViewerToolsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._imageViewerToolsToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._imageViewerToolsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoomWindowToolStripButton1,
            this.panToolStripButton1});
            this._imageViewerToolsToolStrip.Location = new System.Drawing.Point(87, 0);
            this._imageViewerToolsToolStrip.Name = "_imageViewerToolsToolStrip";
            this._imageViewerToolsToolStrip.Size = new System.Drawing.Size(84, 39);
            this._imageViewerToolsToolStrip.TabIndex = 3;
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
            // _generalImageToolStrip
            // 
            this._generalImageToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._generalImageToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._generalImageToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._printImageToolStripButton,
            this._thumbnailViewerToolStripButton,
            this._invertColorsToolStripButton});
            this._generalImageToolStrip.Location = new System.Drawing.Point(351, 39);
            this._generalImageToolStrip.Name = "_generalImageToolStrip";
            this._generalImageToolStrip.Size = new System.Drawing.Size(120, 39);
            this._generalImageToolStrip.TabIndex = 0;
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
            // _thumbnailViewerToolStripButton
            // 
            this._thumbnailViewerToolStripButton.Checked = true;
            this._thumbnailViewerToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this._thumbnailViewerToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._thumbnailViewerToolStripButton.DockableWindow = this._thumbnailDockableWindow;
            this._thumbnailViewerToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._thumbnailViewerToolStripButton.Name = "_thumbnailViewerToolStripButton";
            this._thumbnailViewerToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._thumbnailViewerToolStripButton.Text = "Show/Hide thumbnails";
            // 
            // _thumbnailDockableWindow
            // 
            this._thumbnailDockableWindow.Collapsed = true;
            this._thumbnailDockableWindow.Controls.Add(this._thumbnailViewer);
            this._thumbnailDockableWindow.Guid = new System.Guid("821a2840-0871-42ed-94a7-22df189299bc");
            this._thumbnailDockableWindow.Location = new System.Drawing.Point(4, 18);
            this._thumbnailDockableWindow.Name = "_thumbnailDockableWindow";
            this._thumbnailDockableWindow.Size = new System.Drawing.Size(215, 374);
            this._thumbnailDockableWindow.TabIndex = 0;
            this._thumbnailDockableWindow.Text = "Page thumbnails";
            this._thumbnailDockableWindow.DockSituationChanged += new System.EventHandler(this.HandleThumbnailDockableWindowDockSituationChanged);
            // 
            // _thumbnailViewer
            // 
            this._thumbnailViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._thumbnailViewer.ImageViewer = this._imageViewer;
            this._thumbnailViewer.Location = new System.Drawing.Point(0, 0);
            this._thumbnailViewer.Name = "_thumbnailViewer";
            this._thumbnailViewer.Size = new System.Drawing.Size(215, 374);
            this._thumbnailViewer.TabIndex = 0;
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
            // _sandDockManager
            // 
            this._sandDockManager.AllowMiddleButtonClosure = false;
            this._sandDockManager.DockSystemContainer = this._mainToolStripContainer.ContentPanel;
            this._sandDockManager.DocumentOverflow = TD.SandDock.DocumentOverflowMode.None;
            this._sandDockManager.OwnerForm = this;
            // 
            // BottomToolStripPanel
            // 
            this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // TopToolStripPanel
            // 
            this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // RightToolStripPanel
            // 
            this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // LeftToolStripPanel
            // 
            this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // ContentPanel
            // 
            this.ContentPanel.Size = new System.Drawing.Size(515, 582);
            // 
            // _dockContainer
            // 
            this._dockContainer.ContentSize = 215;
            this._dockContainer.Controls.Add(this._thumbnailDockableWindow);
            this._dockContainer.Dock = System.Windows.Forms.DockStyle.Right;
            this._dockContainer.LayoutSystem = new TD.SandDock.SplitLayoutSystem(new System.Drawing.SizeF(250F, 400F), System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 400F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._thumbnailDockableWindow))}, this._thumbnailDockableWindow)))});
            this._dockContainer.Location = new System.Drawing.Point(992, 219);
            this._dockContainer.Manager = this._sandDockManager;
            this._dockContainer.Name = "_dockContainer";
            this._dockContainer.Size = new System.Drawing.Size(219, 416);
            this._dockContainer.TabIndex = 4;
            // 
            // _formToolStripContainer
            // 
            // 
            // _formToolStripContainer.ContentPanel
            // 
            this._formToolStripContainer.ContentPanel.Controls.Add(this._splitContainer);
            this._formToolStripContainer.ContentPanel.Controls.Add(this._dockContainer);
            this._formToolStripContainer.ContentPanel.Controls.Add(this._customSearchTopDockContainer);
            this._formToolStripContainer.ContentPanel.Size = new System.Drawing.Size(1211, 635);
            this._formToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._formToolStripContainer.Location = new System.Drawing.Point(0, 0);
            this._formToolStripContainer.Name = "_formToolStripContainer";
            this._formToolStripContainer.Size = new System.Drawing.Size(1211, 635);
            this._formToolStripContainer.TabIndex = 5;
            this._formToolStripContainer.Text = "toolStripContainer1";
            this._formToolStripContainer.TopToolStripPanelVisible = false;
            // 
            // _customSearchTopDockContainer
            // 
            this._customSearchTopDockContainer.ContentSize = 215;
            this._customSearchTopDockContainer.Controls.Add(this._customSearchTopDockableWindow);
            this._customSearchTopDockContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this._customSearchTopDockContainer.LayoutSystem = new TD.SandDock.SplitLayoutSystem(new System.Drawing.SizeF(250F, 400F), System.Windows.Forms.Orientation.Vertical, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 400F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._customSearchTopDockableWindow))}, this._customSearchTopDockableWindow)))});
            this._customSearchTopDockContainer.Location = new System.Drawing.Point(0, 0);
            this._customSearchTopDockContainer.Manager = this._sandDockManager;
            this._customSearchTopDockContainer.Name = "_customSearchTopDockContainer";
            this._customSearchTopDockContainer.Size = new System.Drawing.Size(1211, 219);
            this._customSearchTopDockContainer.TabIndex = 5;
            // 
            // _customSearchTopDockableWindow
            // 
            this._customSearchTopDockableWindow.AllowClose = false;
            this._customSearchTopDockableWindow.AllowCollapse = false;
            dockingRules1.AllowDockBottom = false;
            dockingRules1.AllowDockLeft = false;
            dockingRules1.AllowDockRight = false;
            dockingRules1.AllowDockTop = true;
            dockingRules1.AllowFloat = false;
            dockingRules1.AllowTab = false;
            this._customSearchTopDockableWindow.DockingRules = dockingRules1;
            this._customSearchTopDockableWindow.Guid = new System.Guid("cfdc9bf0-340c-42a0-89c9-f2a857ad9954");
            this._customSearchTopDockableWindow.Location = new System.Drawing.Point(0, 18);
            this._customSearchTopDockableWindow.Name = "_customSearchTopDockableWindow";
            this._customSearchTopDockableWindow.ShowOptions = false;
            this._customSearchTopDockableWindow.Size = new System.Drawing.Size(1211, 173);
            this._customSearchTopDockableWindow.TabIndex = 1;
            // 
            // _voaPathExpressionLabel
            // 
            this._voaPathExpressionLabel.AutoSize = true;
            this._voaPathExpressionLabel.Location = new System.Drawing.Point(374, 9);
            this._voaPathExpressionLabel.Name = "_voaPathExpressionLabel";
            this._voaPathExpressionLabel.Size = new System.Drawing.Size(122, 13);
            this._voaPathExpressionLabel.TabIndex = 11;
            this._voaPathExpressionLabel.Text = "<SourceDocName>.voa";
            this._voaPathExpressionLabel.Visible = false;
            // 
            // 
            // FAMFileInspectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1211, 635);
            this.Controls.Add(this._formToolStripContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this._menuStrip;
            this.MinimumSize = new System.Drawing.Size(700, 400);
            this.Name = "FAMFileInspectorForm";
            this.Text = "FAM File Inspector";
            groupBox1.ResumeLayout(false);
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this._mainToolStripContainer.BottomToolStripPanel.ResumeLayout(false);
            this._mainToolStripContainer.BottomToolStripPanel.PerformLayout();
            this._mainToolStripContainer.ContentPanel.ResumeLayout(false);
            this._mainToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this._mainToolStripContainer.TopToolStripPanel.PerformLayout();
            this._mainToolStripContainer.ResumeLayout(false);
            this._mainToolStripContainer.PerformLayout();
            this._searchStatusStrip.ResumeLayout(false);
            this._searchStatusStrip.PerformLayout();
            this._mainLayoutPanel.ResumeLayout(false);
            this._okCancelPanel.ResumeLayout(false);
            this._dataSplitContainer.Panel1.ResumeLayout(false);
            this._dataSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._dataSplitContainer)).EndInit();
            this._dataSplitContainer.ResumeLayout(false);
            this._resultsSplitContainer.Panel1.ResumeLayout(false);
            this._resultsSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._resultsSplitContainer)).EndInit();
            this._resultsSplitContainer.ResumeLayout(false);
            this._searchPanel.ResumeLayout(false);
            this._searchPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dataSearchTermsDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._textSearchTermsDataGridView)).EndInit();
            this._resultsTableLayoutPanel.ResumeLayout(false);
            this._collapsedSearchPanel.ResumeLayout(false);
            this._collapsedSearchPanel.PerformLayout();
            this._fileListPanel.ResumeLayout(false);
            this._fileListPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._fileListDataGridView)).EndInit();
            this._imageToolStripContainer.BottomToolStripPanel.ResumeLayout(false);
            this._imageToolStripContainer.BottomToolStripPanel.PerformLayout();
            this._imageToolStripContainer.ContentPanel.ResumeLayout(false);
            this._imageToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this._imageToolStripContainer.TopToolStripPanel.PerformLayout();
            this._imageToolStripContainer.ResumeLayout(false);
            this._imageToolStripContainer.PerformLayout();
            this._imageViewerStatusStrip.ResumeLayout(false);
            this._imageViewerStatusStrip.PerformLayout();
            this._highlightNavigationToolStrip.ResumeLayout(false);
            this._highlightNavigationToolStrip.PerformLayout();
            this._imageViewerToolsToolStrip.ResumeLayout(false);
            this._imageViewerToolsToolStrip.PerformLayout();
            this._generalImageToolStrip.ResumeLayout(false);
            this._generalImageToolStrip.PerformLayout();
            this._thumbnailDockableWindow.ResumeLayout(false);
            this._dockContainer.ResumeLayout(false);
            this._formToolStripContainer.ContentPanel.ResumeLayout(false);
            this._formToolStripContainer.ResumeLayout(false);
            this._formToolStripContainer.PerformLayout();
            this._customSearchTopDockContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuStrip _menuStrip;
        private Extract.Utilities.Forms.BetterSplitContainer _splitContainer;
        private System.Windows.Forms.ToolStripContainer _imageToolStripContainer;
        private TD.SandDock.SandDockManager _sandDockManager;
        private System.Windows.Forms.ToolStripMenuItem _databaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripContainer _mainToolStripContainer;
        private System.Windows.Forms.Button _selectFilesButton;
        private System.Windows.Forms.Label _selectFilesSummaryLabel;
        private Imaging.Forms.ViewCommandsImageViewerToolStrip _viewCommandsImageViewerToolStrip;
        private Imaging.Forms.NavigationToolsImageViewerToolStrip _navigationToolsImageViewerToolStrip;
        private System.Windows.Forms.ToolStrip _imageViewerToolsToolStrip;
        private Imaging.Forms.ZoomWindowToolStripButton zoomWindowToolStripButton1;
        private Imaging.Forms.PanToolStripButton panToolStripButton1;
        private Extract.Utilities.Forms.BetterSplitContainer _dataSplitContainer;
        private System.Windows.Forms.ComboBox _searchModifierComboBox;
        private System.Windows.Forms.ComboBox _searchTypeComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView _textSearchTermsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn _textSearchTermsColumn;
        private System.Windows.Forms.Button _searchButton;
        private System.Windows.Forms.Button _clearButton;
        private Extract.Imaging.Forms.ImageViewer _imageViewer;
        private System.Windows.Forms.DataGridView _dataSearchTermsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn _dataSearchFieldColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _dataSearchValueColumn;
        private System.Windows.Forms.ToolStrip _highlightNavigationToolStrip;
        private Imaging.Forms.PreviousLayerObjectToolStripButton _previousLayerObjectToolStripButton;
        private Imaging.Forms.NextLayerObjectToolStripButton _nextLayerObjectToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem _logoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator _databaseMenuToolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem _exitToolStripMenuItem;
        private System.Windows.Forms.StatusStrip _searchStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _searchStatusLabel;
        private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        private System.Windows.Forms.ToolStripContentPanel ContentPanel;
        private System.Windows.Forms.ToolStripProgressBar _searchProgressBar;
        private System.Windows.Forms.StatusStrip _imageViewerStatusStrip;
        private Imaging.Forms.StatusStripItems.LayerObjectSelectionStatusLabel _layerObjectSelectionStatusLabel;
        private System.Windows.Forms.ToolStrip _generalImageToolStrip;
        private Imaging.Forms.PrintImageToolStripButton _printImageToolStripButton;
        private System.Windows.Forms.ToolStripStatusLabel _searchErrorStatusStripLabel;
        private System.Windows.Forms.ToolStripStatusLabel _imageViewerErrorStripStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem _helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _aboutToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem _ffiHelpMenuItem;

        private System.Windows.Forms.Button _refreshFileListButton;
        private System.Windows.Forms.ToolStripMenuItem _adminModeToolStripMenuItem;
        private System.Windows.Forms.CheckBox _caseSensitiveSearchCheckBox;
        private System.Windows.Forms.CheckBox _regexSearchCheckBox;
        private System.Windows.Forms.CheckBox _fuzzySearchCheckBox;
        private Imaging.Forms.ZoomLevelToolStripStatusLabel _zoomLevelToolStripStatusLabel;
        private Imaging.Forms.ResolutionToolStripStatusLabel _resolutionToolStripStatusLabel;
        private Imaging.Forms.MousePositionToolStripStatusLabel _mousePositionToolStripStatusLabel;
        private System.Windows.Forms.Panel _searchPanel;
        private Extract.Utilities.Forms.BetterSplitContainer _resultsSplitContainer;
        private System.Windows.Forms.Button _closeSearchPaneButton;
        private System.Windows.Forms.Panel _collapsedSearchPanel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button _openSearchPaneButton;
        private System.Windows.Forms.Panel _fileListPanel;
        private System.Windows.Forms.DataGridView _fileListDataGridView;
        private System.Windows.Forms.CheckBox _showOnlyMatchesCheckBox;
        private System.Windows.Forms.TableLayoutPanel _resultsTableLayoutPanel;
        private TD.SandDock.DockContainer _dockContainer;
        private TD.SandDock.DockableWindow _thumbnailDockableWindow;
        private Imaging.Forms.ThumbnailViewer _thumbnailViewer;
        private Imaging.Forms.ThumbnailViewerToolStripButton _thumbnailViewerToolStripButton;
        private System.Windows.Forms.DataGridViewImageColumn _fileListFlagColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _fileListIDColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _fileListNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _fileListPagesColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _fileListMatchesColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _fileListFolderColumn;
        private Imaging.Forms.InvertColorsToolStripButton _invertColorsToolStripButton;
        private System.Windows.Forms.TableLayoutPanel _mainLayoutPanel;
        private System.Windows.Forms.Panel _okCancelPanel;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.ToolStripContainer _formToolStripContainer;
        private TD.SandDock.DockContainer _customSearchTopDockContainer;
        private TD.SandDock.DockableWindow _customSearchTopDockableWindow;
        private System.Windows.Forms.Label _voaPathExpressionLabel;

    }
}

