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
            TD.SandDock.DockingRules dockingRules1 = new TD.SandDock.DockingRules();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FAMFileInspectorForm));
            this._selectFilesSummaryLabel = new System.Windows.Forms.Label();
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this._databaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._logoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._splitContainer = new System.Windows.Forms.SplitContainer();
            this._mainToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this._searchStatusStrip = new System.Windows.Forms.StatusStrip();
            this._searchStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._searchProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this._searchErrorStatusStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._searchSplitContainer = new System.Windows.Forms.SplitContainer();
            this._selectFilesButton = new System.Windows.Forms.Button();
            this._fileListPanel = new System.Windows.Forms.Panel();
            this._fileListDataGridView = new System.Windows.Forms.DataGridView();
            this._fileListNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._fileListPagesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._fileListMatchesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._fileListFolderColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._showOnlyMatchesCheckBox = new System.Windows.Forms.CheckBox();
            this._dockContainer = new TD.SandDock.DockContainer();
            this._searchDockableWindow = new TD.SandDock.DockableWindow();
            this._dataSearchTermsDataGridView = new System.Windows.Forms.DataGridView();
            this._dataSearchFieldColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._dataSearchValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._clearButton = new System.Windows.Forms.Button();
            this._searchButton = new System.Windows.Forms.Button();
            this._textSearchTermsDataGridView = new System.Windows.Forms.DataGridView();
            this._textSearchTermsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label3 = new System.Windows.Forms.Label();
            this._searchTypeComboBox = new System.Windows.Forms.ComboBox();
            this._searchModifierComboBox = new System.Windows.Forms.ComboBox();
            this._sandDockManager = new TD.SandDock.SandDockManager();
            this._imageToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this._imageViewerStatusStrip = new System.Windows.Forms.StatusStrip();
            this._layerObjectSelectionStatusLabel = new Extract.Imaging.Forms.StatusStripItems.LayerObjectSelectionStatusLabel();
            this._imageViewerErrorStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._imageViewer = new Extract.Imaging.Forms.ImageViewer();
            this._imageViewerToolsToolStrip = new System.Windows.Forms.ToolStrip();
            this.zoomWindowToolStripButton1 = new Extract.Imaging.Forms.ZoomWindowToolStripButton();
            this.panToolStripButton1 = new Extract.Imaging.Forms.PanToolStripButton();
            this._highlightNavigationToolStrip = new System.Windows.Forms.ToolStrip();
            this._previousLayerObjectToolStripButton = new Extract.Imaging.Forms.PreviousLayerObjectToolStripButton();
            this._nextLayerObjectToolStripButton = new Extract.Imaging.Forms.NextLayerObjectToolStripButton();
            this._navigationToolsImageViewerToolStrip = new Extract.Imaging.Forms.NavigationToolsImageViewerToolStrip();
            this._viewCommandsImageViewerToolStrip = new Extract.Imaging.Forms.ViewCommandsImageViewerToolStrip();
            this._generalImageToolStrip = new System.Windows.Forms.ToolStrip();
            this._printImageToolStripButton = new Extract.Imaging.Forms.PrintImageToolStripButton();
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this._helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            ((System.ComponentModel.ISupportInitialize)(this._searchSplitContainer)).BeginInit();
            this._searchSplitContainer.Panel1.SuspendLayout();
            this._searchSplitContainer.Panel2.SuspendLayout();
            this._searchSplitContainer.SuspendLayout();
            this._fileListPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._fileListDataGridView)).BeginInit();
            this._dockContainer.SuspendLayout();
            this._searchDockableWindow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dataSearchTermsDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._textSearchTermsDataGridView)).BeginInit();
            this._imageToolStripContainer.BottomToolStripPanel.SuspendLayout();
            this._imageToolStripContainer.ContentPanel.SuspendLayout();
            this._imageToolStripContainer.TopToolStripPanel.SuspendLayout();
            this._imageToolStripContainer.SuspendLayout();
            this._imageViewerStatusStrip.SuspendLayout();
            this._imageViewerToolsToolStrip.SuspendLayout();
            this._highlightNavigationToolStrip.SuspendLayout();
            this._generalImageToolStrip.SuspendLayout();
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
            groupBox1.Size = new System.Drawing.Size(404, 57);
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
            this._selectFilesSummaryLabel.Size = new System.Drawing.Size(392, 38);
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
            this._menuStrip.Size = new System.Drawing.Size(500, 24);
            this._menuStrip.TabIndex = 0;
            this._menuStrip.Text = "menuStrip1";
            // 
            // _databaseToolStripMenuItem
            // 
            this._databaseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._logoutToolStripMenuItem,
            this.toolStripSeparator1,
            this._exitToolStripMenuItem});
            this._databaseToolStripMenuItem.Name = "_databaseToolStripMenuItem";
            this._databaseToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this._databaseToolStripMenuItem.Text = "&Database";
            // 
            // _logoutToolStripMenuItem
            // 
            this._logoutToolStripMenuItem.Name = "_logoutToolStripMenuItem";
            this._logoutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._logoutToolStripMenuItem.Text = "&Logout";
            this._logoutToolStripMenuItem.Click += new System.EventHandler(this.HandleLogoutToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // _exitToolStripMenuItem
            // 
            this._exitToolStripMenuItem.Name = "_exitToolStripMenuItem";
            this._exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._exitToolStripMenuItem.Text = "E&xit";
            this._exitToolStripMenuItem.Click += new System.EventHandler(this.HandleExitToolStripMenuItem_Click);
            // 
            // _splitContainer
            // 
            this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this._splitContainer.Location = new System.Drawing.Point(0, 0);
            this._splitContainer.Margin = new System.Windows.Forms.Padding(0);
            this._splitContainer.Name = "_splitContainer";
            // 
            // _splitContainer.Panel1
            // 
            this._splitContainer.Panel1.Controls.Add(this._mainToolStripContainer);
            this._splitContainer.Panel1MinSize = 396;
            // 
            // _splitContainer.Panel2
            // 
            this._splitContainer.Panel2.Controls.Add(this._imageToolStripContainer);
            this._splitContainer.Size = new System.Drawing.Size(1274, 637);
            this._splitContainer.SplitterDistance = 500;
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
            this._mainToolStripContainer.ContentPanel.Controls.Add(this._searchSplitContainer);
            this._mainToolStripContainer.ContentPanel.Size = new System.Drawing.Size(500, 591);
            this._mainToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // _mainToolStripContainer.LeftToolStripPanel
            // 
            this._mainToolStripContainer.LeftToolStripPanel.Enabled = false;
            this._mainToolStripContainer.Location = new System.Drawing.Point(0, 0);
            this._mainToolStripContainer.Name = "_mainToolStripContainer";
            // 
            // _mainToolStripContainer.RightToolStripPanel
            // 
            this._mainToolStripContainer.RightToolStripPanel.Enabled = false;
            this._mainToolStripContainer.Size = new System.Drawing.Size(500, 637);
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
            this._searchStatusStrip.Size = new System.Drawing.Size(500, 22);
            this._searchStatusStrip.TabIndex = 0;
            // 
            // _searchStatusLabel
            // 
            this._searchStatusLabel.AutoSize = false;
            this._searchStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._searchStatusLabel.Name = "_searchStatusLabel";
            this._searchStatusLabel.Size = new System.Drawing.Size(17, 17);
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
            // _selectFilesButton
            // 
            this._selectFilesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._selectFilesButton.Location = new System.Drawing.Point(422, 17);
            this._selectFilesButton.Name = "_selectFilesButton";
            this._selectFilesButton.Size = new System.Drawing.Size(75, 23);
            this._selectFilesButton.TabIndex = 1;
            this._selectFilesButton.Text = "Change...";
            this._selectFilesButton.UseVisualStyleBackColor = true;
            this._selectFilesButton.Click += new System.EventHandler(this.HandleSelectFilesButton_Click);
            // 
            // _fileListPanel
            // 
            this._fileListPanel.Controls.Add(this._fileListDataGridView);
            this._fileListPanel.Controls.Add(this._showOnlyMatchesCheckBox);
            this._fileListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._fileListPanel.Location = new System.Drawing.Point(0, 185);
            this._fileListPanel.Name = "_fileListPanel";
            this._fileListPanel.Size = new System.Drawing.Size(500, 329);
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
            this._fileListDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._fileListDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._fileListNameColumn,
            this._fileListPagesColumn,
            this._fileListMatchesColumn,
            this._fileListFolderColumn});
            this._fileListDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._fileListDataGridView.Location = new System.Drawing.Point(0, 29);
            this._fileListDataGridView.Name = "_fileListDataGridView";
            this._fileListDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._fileListDataGridView.Size = new System.Drawing.Size(500, 297);
            this._fileListDataGridView.TabIndex = 1;
            this._fileListDataGridView.CurrentCellChanged += new System.EventHandler(this.HandleResultsDataGridView_CurrentCellChanged);
            this._fileListDataGridView.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(this.HandleFileListDataGridView_SortCompare);
            this._fileListDataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HandleFileListDataGridView_KeyDown);
            // 
            // _fileListNameColumn
            // 
            this._fileListNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._fileListNameColumn.HeaderText = "Filename";
            this._fileListNameColumn.Name = "_fileListNameColumn";
            this._fileListNameColumn.Width = 225;
            // 
            // _fileListPagesColumn
            // 
            this._fileListPagesColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._fileListPagesColumn.HeaderText = "Pages";
            this._fileListPagesColumn.Name = "_fileListPagesColumn";
            this._fileListPagesColumn.Width = 50;
            // 
            // _fileListMatchesColumn
            // 
            this._fileListMatchesColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this._fileListMatchesColumn.HeaderText = "Matches";
            this._fileListMatchesColumn.Name = "_fileListMatchesColumn";
            this._fileListMatchesColumn.Width = 60;
            // 
            // _fileListFolderColumn
            // 
            this._fileListFolderColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._fileListFolderColumn.HeaderText = "Folder";
            this._fileListFolderColumn.Name = "_fileListFolderColumn";
            // 
            // _showOnlyMatchesCheckBox
            // 
            this._showOnlyMatchesCheckBox.AutoSize = true;
            this._showOnlyMatchesCheckBox.Enabled = false;
            this._showOnlyMatchesCheckBox.Location = new System.Drawing.Point(7, 6);
            this._showOnlyMatchesCheckBox.Name = "_showOnlyMatchesCheckBox";
            this._showOnlyMatchesCheckBox.Size = new System.Drawing.Size(189, 17);
            this._showOnlyMatchesCheckBox.TabIndex = 0;
            this._showOnlyMatchesCheckBox.Text = "Show only matching search results";
            this._showOnlyMatchesCheckBox.UseVisualStyleBackColor = true;
            this._showOnlyMatchesCheckBox.CheckedChanged += new System.EventHandler(this.HandleShowOnlyMatchesCheckBox_CheckedChanged);
            // 
            // _dockContainer
            // 
            this._dockContainer.ContentSize = 181;
            this._dockContainer.Controls.Add(this._searchDockableWindow);
            this._dockContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this._dockContainer.LayoutSystem = new TD.SandDock.SplitLayoutSystem(new System.Drawing.SizeF(250F, 400F), System.Windows.Forms.Orientation.Vertical, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 497.2955F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._searchDockableWindow))}, this._searchDockableWindow)))});
            this._dockContainer.Location = new System.Drawing.Point(0, 0);
            this._dockContainer.Manager = this._sandDockManager;
            this._dockContainer.Name = "_dockContainer";
            this._dockContainer.Size = new System.Drawing.Size(500, 185);
            this._dockContainer.TabIndex = 0;
            // 
            // _searchDockableWindow
            // 
            this._searchDockableWindow.AllowCollapse = false;
            this._searchDockableWindow.Controls.Add(this._dataSearchTermsDataGridView);
            this._searchDockableWindow.Controls.Add(this._clearButton);
            this._searchDockableWindow.Controls.Add(this._searchButton);
            this._searchDockableWindow.Controls.Add(this._textSearchTermsDataGridView);
            this._searchDockableWindow.Controls.Add(this.label3);
            this._searchDockableWindow.Controls.Add(this._searchTypeComboBox);
            this._searchDockableWindow.Controls.Add(label1);
            this._searchDockableWindow.Controls.Add(this._searchModifierComboBox);
            dockingRules1.AllowDockBottom = false;
            dockingRules1.AllowDockLeft = false;
            dockingRules1.AllowDockRight = false;
            dockingRules1.AllowDockTop = true;
            dockingRules1.AllowFloat = true;
            dockingRules1.AllowTab = false;
            this._searchDockableWindow.DockingRules = dockingRules1;
            this._searchDockableWindow.FloatingSize = new System.Drawing.Size(500, 163);
            this._searchDockableWindow.Guid = new System.Guid("ecd6c9fb-760e-4a1b-822c-bb83ea44d6f4");
            this._searchDockableWindow.Location = new System.Drawing.Point(0, 18);
            this._searchDockableWindow.Name = "_searchDockableWindow";
            this._searchDockableWindow.ShowOptions = false;
            this._searchDockableWindow.Size = new System.Drawing.Size(500, 139);
            this._searchDockableWindow.TabIndex = 0;
            this._searchDockableWindow.Text = "Search";
            this._searchDockableWindow.Closing += new TD.SandDock.DockControlClosingEventHandler(this.HandleDockWindow_Closing);
            this._searchDockableWindow.AutoHidePopupOpened += new System.EventHandler(this.HandleDockableWindow_AutoHidePopupOpened);
            // 
            // _dataSearchTermsDataGridView
            // 
            this._dataSearchTermsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataSearchTermsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dataSearchTermsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._dataSearchFieldColumn,
            this._dataSearchValueColumn});
            this._dataSearchTermsDataGridView.Location = new System.Drawing.Point(3, 33);
            this._dataSearchTermsDataGridView.Name = "_dataSearchTermsDataGridView";
            this._dataSearchTermsDataGridView.Size = new System.Drawing.Size(494, 74);
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
            // _clearButton
            // 
            this._clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._clearButton.Location = new System.Drawing.Point(422, 113);
            this._clearButton.Name = "_clearButton";
            this._clearButton.Size = new System.Drawing.Size(75, 23);
            this._clearButton.TabIndex = 6;
            this._clearButton.Text = "Clear";
            this._clearButton.UseVisualStyleBackColor = true;
            this._clearButton.Click += new System.EventHandler(this.HandleClearButton_Click);
            // 
            // _searchButton
            // 
            this._searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._searchButton.Location = new System.Drawing.Point(341, 113);
            this._searchButton.Name = "_searchButton";
            this._searchButton.Size = new System.Drawing.Size(75, 23);
            this._searchButton.TabIndex = 5;
            this._searchButton.Text = "Search";
            this._searchButton.UseVisualStyleBackColor = true;
            this._searchButton.Click += new System.EventHandler(this.HandleSearchButton_Click);
            // 
            // _textSearchTermsDataGridView
            // 
            this._textSearchTermsDataGridView.AllowUserToResizeRows = false;
            this._textSearchTermsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textSearchTermsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._textSearchTermsDataGridView.ColumnHeadersVisible = false;
            this._textSearchTermsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._textSearchTermsColumn});
            this._textSearchTermsDataGridView.Location = new System.Drawing.Point(3, 33);
            this._textSearchTermsDataGridView.Name = "_textSearchTermsDataGridView";
            this._textSearchTermsDataGridView.Size = new System.Drawing.Size(494, 74);
            this._textSearchTermsDataGridView.TabIndex = 4;
            // 
            // _textSearchTermsColumn
            // 
            this._textSearchTermsColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._textSearchTermsColumn.HeaderText = "";
            this._textSearchTermsColumn.Name = "_textSearchTermsColumn";
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
            // _sandDockManager
            // 
            this._sandDockManager.AllowMiddleButtonClosure = false;
            this._sandDockManager.DockSystemContainer = this._mainToolStripContainer.ContentPanel;
            this._sandDockManager.DocumentOverflow = TD.SandDock.DocumentOverflowMode.None;
            this._sandDockManager.OwnerForm = this;
            this._sandDockManager.DockControlActivated += new TD.SandDock.DockControlEventHandler(this.HandleSandDockManager_DockControlActivated);
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
            this._imageToolStripContainer.ContentPanel.Size = new System.Drawing.Size(770, 576);
            this._imageToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // _imageToolStripContainer.LeftToolStripPanel
            // 
            this._imageToolStripContainer.LeftToolStripPanel.Enabled = false;
            this._imageToolStripContainer.Location = new System.Drawing.Point(0, 0);
            this._imageToolStripContainer.Name = "_imageToolStripContainer";
            // 
            // _imageToolStripContainer.RightToolStripPanel
            // 
            this._imageToolStripContainer.RightToolStripPanel.Enabled = false;
            this._imageToolStripContainer.Size = new System.Drawing.Size(770, 637);
            this._imageToolStripContainer.TabIndex = 0;
            this._imageToolStripContainer.Text = "toolStripContainer1";
            // 
            // _imageToolStripContainer.TopToolStripPanel
            // 
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._imageViewerToolsToolStrip);
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._highlightNavigationToolStrip);
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._navigationToolsImageViewerToolStrip);
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._viewCommandsImageViewerToolStrip);
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._generalImageToolStrip);
            // 
            // _imageViewerStatusStrip
            // 
            this._imageViewerStatusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._imageViewerStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._layerObjectSelectionStatusLabel,
            this._imageViewerErrorStripStatusLabel});
            this._imageViewerStatusStrip.Location = new System.Drawing.Point(0, 0);
            this._imageViewerStatusStrip.Name = "_imageViewerStatusStrip";
            this._imageViewerStatusStrip.Size = new System.Drawing.Size(770, 22);
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
            this._imageViewerErrorStripStatusLabel.ForeColor = System.Drawing.Color.Red;
            this._imageViewerErrorStripStatusLabel.Name = "_imageViewerErrorStripStatusLabel";
            this._imageViewerErrorStripStatusLabel.Size = new System.Drawing.Size(626, 17);
            this._imageViewerErrorStripStatusLabel.Spring = true;
            this._imageViewerErrorStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _imageViewer
            // 
            this._imageViewer.AutoOcr = false;
            this._imageViewer.AutoZoomScale = 0;
            this._imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._imageViewer.Location = new System.Drawing.Point(0, 0);
            this._imageViewer.MinimumAngularHighlightHeight = 4;
            this._imageViewer.Name = "_imageViewer";
            this._imageViewer.OcrTradeoff = Extract.Imaging.OcrTradeoff.Accurate;
            this._imageViewer.RedactionMode = false;
            this._imageViewer.Size = new System.Drawing.Size(770, 576);
            this._imageViewer.TabIndex = 0;
            this._imageViewer.TabStop = false;
            // 
            // _imageViewerToolsToolStrip
            // 
            this._imageViewerToolsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._imageViewerToolsToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._imageViewerToolsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoomWindowToolStripButton1,
            this.panToolStripButton1});
            this._imageViewerToolsToolStrip.Location = new System.Drawing.Point(3, 0);
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
            // _highlightNavigationToolStrip
            // 
            this._highlightNavigationToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._highlightNavigationToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._highlightNavigationToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._previousLayerObjectToolStripButton,
            this._nextLayerObjectToolStripButton});
            this._highlightNavigationToolStrip.Location = new System.Drawing.Point(87, 0);
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
            this._viewCommandsImageViewerToolStrip.Location = new System.Drawing.Point(404, 0);
            this._viewCommandsImageViewerToolStrip.Name = "_viewCommandsImageViewerToolStrip";
            this._viewCommandsImageViewerToolStrip.Size = new System.Drawing.Size(312, 39);
            this._viewCommandsImageViewerToolStrip.TabIndex = 1;
            // 
            // _generalImageToolStrip
            // 
            this._generalImageToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._generalImageToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._generalImageToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._printImageToolStripButton});
            this._generalImageToolStrip.Location = new System.Drawing.Point(716, 0);
            this._generalImageToolStrip.Name = "_generalImageToolStrip";
            this._generalImageToolStrip.Size = new System.Drawing.Size(48, 39);
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
            this._aboutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._aboutToolStripMenuItem.Text = "&About";
            this._aboutToolStripMenuItem.Click += new System.EventHandler(this.HandleAboutToolStripMenuItem_Click);
            // 
            // _searchSplitContainer
            // 
            this._searchSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._searchSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this._searchSplitContainer.Location = new System.Drawing.Point(0, 0);
            this._searchSplitContainer.Name = "_searchSplitContainer";
            this._searchSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _searchSplitContainer.Panel1
            // 
            this._searchSplitContainer.Panel1.Controls.Add(groupBox1);
            this._searchSplitContainer.Panel1.Controls.Add(this._selectFilesButton);
            this._searchSplitContainer.Panel1MinSize = 47;
            // 
            // _searchSplitContainer.Panel2
            // 
            this._searchSplitContainer.Panel2.Controls.Add(this._fileListPanel);
            this._searchSplitContainer.Panel2.Controls.Add(this._dockContainer);
            this._searchSplitContainer.Size = new System.Drawing.Size(500, 591);
            this._searchSplitContainer.SplitterDistance = 73;
            this._searchSplitContainer.TabIndex = 0;
            this._searchSplitContainer.TabStop = false;
            // 
            // FAMFileInspectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1274, 637);
            this.Controls.Add(this._splitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this._menuStrip;
            this.MinimumSize = new System.Drawing.Size(575, 300);
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
            this._searchSplitContainer.Panel1.ResumeLayout(false);
            this._searchSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._searchSplitContainer)).EndInit();
            this._searchSplitContainer.ResumeLayout(false);
            this._fileListPanel.ResumeLayout(false);
            this._fileListPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._fileListDataGridView)).EndInit();
            this._dockContainer.ResumeLayout(false);
            this._searchDockableWindow.ResumeLayout(false);
            this._searchDockableWindow.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dataSearchTermsDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._textSearchTermsDataGridView)).EndInit();
            this._imageToolStripContainer.BottomToolStripPanel.ResumeLayout(false);
            this._imageToolStripContainer.BottomToolStripPanel.PerformLayout();
            this._imageToolStripContainer.ContentPanel.ResumeLayout(false);
            this._imageToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this._imageToolStripContainer.TopToolStripPanel.PerformLayout();
            this._imageToolStripContainer.ResumeLayout(false);
            this._imageToolStripContainer.PerformLayout();
            this._imageViewerStatusStrip.ResumeLayout(false);
            this._imageViewerStatusStrip.PerformLayout();
            this._imageViewerToolsToolStrip.ResumeLayout(false);
            this._imageViewerToolsToolStrip.PerformLayout();
            this._highlightNavigationToolStrip.ResumeLayout(false);
            this._highlightNavigationToolStrip.PerformLayout();
            this._generalImageToolStrip.ResumeLayout(false);
            this._generalImageToolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.SplitContainer _splitContainer;
        private System.Windows.Forms.ToolStripContainer _imageToolStripContainer;
        private TD.SandDock.SandDockManager _sandDockManager;
        private System.Windows.Forms.ToolStripMenuItem _databaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripContainer _mainToolStripContainer;
        private System.Windows.Forms.Button _selectFilesButton;
        private System.Windows.Forms.Label _selectFilesSummaryLabel;
        private TD.SandDock.DockableWindow _searchDockableWindow;
        private Imaging.Forms.ViewCommandsImageViewerToolStrip _viewCommandsImageViewerToolStrip;
        private Imaging.Forms.NavigationToolsImageViewerToolStrip _navigationToolsImageViewerToolStrip;
        private System.Windows.Forms.ToolStrip _imageViewerToolsToolStrip;
        private Imaging.Forms.ZoomWindowToolStripButton zoomWindowToolStripButton1;
        private Imaging.Forms.PanToolStripButton panToolStripButton1;
        private System.Windows.Forms.SplitContainer _searchSplitContainer;
        private System.Windows.Forms.ComboBox _searchModifierComboBox;
        private System.Windows.Forms.ComboBox _searchTypeComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView _textSearchTermsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn _textSearchTermsColumn;
        private System.Windows.Forms.Button _searchButton;
        private System.Windows.Forms.Button _clearButton;
        private System.Windows.Forms.DataGridView _fileListDataGridView;
        private Extract.Imaging.Forms.ImageViewer _imageViewer;
        private TD.SandDock.DockContainer _dockContainer;
        private System.Windows.Forms.DataGridViewTextBoxColumn _fileListNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _fileListPagesColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _fileListMatchesColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _fileListFolderColumn;
        private System.Windows.Forms.CheckBox _showOnlyMatchesCheckBox;
        private System.Windows.Forms.Panel _fileListPanel;
        private System.Windows.Forms.DataGridView _dataSearchTermsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn _dataSearchFieldColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _dataSearchValueColumn;
        private System.Windows.Forms.ToolStrip _highlightNavigationToolStrip;
        private Imaging.Forms.PreviousLayerObjectToolStripButton _previousLayerObjectToolStripButton;
        private Imaging.Forms.NextLayerObjectToolStripButton _nextLayerObjectToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem _logoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
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

    }
}

