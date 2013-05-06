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
        /// designer. Move the initialization of _searchSplitContainer and it's too panels to just
        /// above the FAMFileInspectorForm itself below after making changes in the designer.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.Label label1;
            TD.SandDock.DockingRules dockingRules1 = new TD.SandDock.DockingRules();
            this._selectFilesSummaryLabel = new System.Windows.Forms.Label();
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._splitContainer = new System.Windows.Forms.SplitContainer();
            this._mainToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this._statusToolStrip = new System.Windows.Forms.ToolStrip();
            this._statusToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            this._searchSplitContainer = new System.Windows.Forms.SplitContainer();
            this._selectFilesButton = new System.Windows.Forms.Button();
            this._fileListPanel = new System.Windows.Forms.Panel();
            this._fileListDataGridView = new System.Windows.Forms.DataGridView();
            this._fileListNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._fileListPagesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._fileListMatchesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._fileListFolderColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._showOnlyMatchesCheckBox = new System.Windows.Forms.CheckBox();
            this.dockContainer1 = new TD.SandDock.DockContainer();
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
            this._imageViewer = new Extract.Imaging.Forms.ImageViewer();
            this._navigationToolsImageViewerToolStrip = new Extract.Imaging.Forms.NavigationToolsImageViewerToolStrip();
            this._imageViewerToolsToolStrip = new System.Windows.Forms.ToolStrip();
            this.zoomWindowToolStripButton1 = new Extract.Imaging.Forms.ZoomWindowToolStripButton();
            this.panToolStripButton1 = new Extract.Imaging.Forms.PanToolStripButton();
            this._viewCommandsImageViewerToolStrip = new Extract.Imaging.Forms.ViewCommandsImageViewerToolStrip();
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
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
            this._statusToolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._searchSplitContainer)).BeginInit();
            this._searchSplitContainer.Panel1.SuspendLayout();
            this._searchSplitContainer.Panel2.SuspendLayout();
            this._searchSplitContainer.SuspendLayout();
            this._fileListPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._fileListDataGridView)).BeginInit();
            this.dockContainer1.SuspendLayout();
            this._searchDockableWindow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dataSearchTermsDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._textSearchTermsDataGridView)).BeginInit();
            this._imageToolStripContainer.ContentPanel.SuspendLayout();
            this._imageToolStripContainer.TopToolStripPanel.SuspendLayout();
            this._imageToolStripContainer.SuspendLayout();
            this._imageViewerToolsToolStrip.SuspendLayout();
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
            this._selectFilesSummaryLabel.Text = "One\r\nTwo\r\nThree\r\nFour\r\nFive\r\nSix\r\nSeven\r\n";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(188, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(78, 13);
            label1.TabIndex = 3;
            label1.Text = "of the following";
            // 
            // _menuStrip
            // 
            this._menuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.Size = new System.Drawing.Size(500, 24);
            this._menuStrip.TabIndex = 0;
            this._menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
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
            this._splitContainer.Size = new System.Drawing.Size(1136, 637);
            this._splitContainer.SplitterDistance = 500;
            this._splitContainer.TabIndex = 1;
            this._splitContainer.TabStop = false;
            // 
            // _mainToolStripContainer
            // 
            // 
            // _mainToolStripContainer.BottomToolStripPanel
            // 
            this._mainToolStripContainer.BottomToolStripPanel.Controls.Add(this._statusToolStrip);
            // 
            // _mainToolStripContainer.ContentPanel
            // 
            this._mainToolStripContainer.ContentPanel.Controls.Add(this._searchSplitContainer);
            this._mainToolStripContainer.ContentPanel.Size = new System.Drawing.Size(500, 588);
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
            // _statusToolStrip
            // 
            this._statusToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._statusToolStrip.AutoSize = false;
            this._statusToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._statusToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._statusToolStripLabel});
            this._statusToolStrip.Location = new System.Drawing.Point(3, 0);
            this._statusToolStrip.Name = "_statusToolStrip";
            this._statusToolStrip.Size = new System.Drawing.Size(391, 25);
            this._statusToolStrip.TabIndex = 2;
            this._statusToolStrip.Text = "Ready";
            // 
            // _statusToolStripLabel
            // 
            this._statusToolStripLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._statusToolStripLabel.Name = "_statusToolStripLabel";
            this._statusToolStripLabel.Size = new System.Drawing.Size(39, 22);
            this._statusToolStripLabel.Text = "Ready";
            this._statusToolStripLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this._fileListPanel.Size = new System.Drawing.Size(500, 326);
            this._fileListPanel.TabIndex = 7;
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
            this._fileListDataGridView.TabIndex = 3;
            this._fileListDataGridView.CurrentCellChanged += new System.EventHandler(this.HandleResultsDataGridView_CurrentCellChanged);
            this._fileListDataGridView.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(this.HandleFileListDataGridView_SortCompare);
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
            this._showOnlyMatchesCheckBox.TabIndex = 7;
            this._showOnlyMatchesCheckBox.Text = "Show only matching search results";
            this._showOnlyMatchesCheckBox.UseVisualStyleBackColor = true;
            this._showOnlyMatchesCheckBox.CheckedChanged += new System.EventHandler(this.HandleShowOnlyMatchesCheckBox_CheckedChanged);
            // 
            // dockContainer1
            // 
            this.dockContainer1.ContentSize = 181;
            this.dockContainer1.Controls.Add(this._searchDockableWindow);
            this.dockContainer1.Dock = System.Windows.Forms.DockStyle.Top;
            this.dockContainer1.LayoutSystem = new TD.SandDock.SplitLayoutSystem(new System.Drawing.SizeF(250F, 400F), System.Windows.Forms.Orientation.Vertical, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 497.2955F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._searchDockableWindow))}, this._searchDockableWindow)))});
            this.dockContainer1.Location = new System.Drawing.Point(0, 0);
            this.dockContainer1.Manager = this._sandDockManager;
            this.dockContainer1.Name = "dockContainer1";
            this.dockContainer1.Size = new System.Drawing.Size(500, 185);
            this.dockContainer1.TabIndex = 6;
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
            this._dataSearchTermsDataGridView.TabIndex = 11;
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
            this._clearButton.TabIndex = 10;
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
            this._searchButton.TabIndex = 9;
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
            this._textSearchTermsDataGridView.TabIndex = 8;
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
            this.label3.TabIndex = 7;
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
            this._searchTypeComboBox.TabIndex = 5;
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
            this._searchModifierComboBox.TabIndex = 2;
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
            this._imageToolStripContainer.BottomToolStripPanel.Enabled = false;
            // 
            // _imageToolStripContainer.ContentPanel
            // 
            this._imageToolStripContainer.ContentPanel.Controls.Add(this._imageViewer);
            this._imageToolStripContainer.ContentPanel.Size = new System.Drawing.Size(632, 559);
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
            this._imageToolStripContainer.Size = new System.Drawing.Size(632, 637);
            this._imageToolStripContainer.TabIndex = 1;
            this._imageToolStripContainer.Text = "toolStripContainer1";
            // 
            // _imageToolStripContainer.TopToolStripPanel
            // 
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._navigationToolsImageViewerToolStrip);
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._imageViewerToolsToolStrip);
            this._imageToolStripContainer.TopToolStripPanel.Controls.Add(this._viewCommandsImageViewerToolStrip);
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
            this._imageViewer.Size = new System.Drawing.Size(632, 559);
            this._imageViewer.TabIndex = 0;
            // 
            // _navigationToolsImageViewerToolStrip
            // 
            this._navigationToolsImageViewerToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._navigationToolsImageViewerToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._navigationToolsImageViewerToolStrip.Location = new System.Drawing.Point(3, 0);
            this._navigationToolsImageViewerToolStrip.Name = "_navigationToolsImageViewerToolStrip";
            this._navigationToolsImageViewerToolStrip.Size = new System.Drawing.Size(233, 39);
            this._navigationToolsImageViewerToolStrip.TabIndex = 2;
            // 
            // _imageViewerToolsToolStrip
            // 
            this._imageViewerToolsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._imageViewerToolsToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._imageViewerToolsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoomWindowToolStripButton1,
            this.panToolStripButton1});
            this._imageViewerToolsToolStrip.Location = new System.Drawing.Point(3, 39);
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
            // _viewCommandsImageViewerToolStrip
            // 
            this._viewCommandsImageViewerToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._viewCommandsImageViewerToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._viewCommandsImageViewerToolStrip.Location = new System.Drawing.Point(87, 39);
            this._viewCommandsImageViewerToolStrip.Name = "_viewCommandsImageViewerToolStrip";
            this._viewCommandsImageViewerToolStrip.Size = new System.Drawing.Size(312, 39);
            this._viewCommandsImageViewerToolStrip.TabIndex = 1;
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
            this._searchSplitContainer.Panel2.Controls.Add(this.dockContainer1);
            this._searchSplitContainer.Size = new System.Drawing.Size(500, 588);
            this._searchSplitContainer.SplitterDistance = 73;
            this._searchSplitContainer.TabIndex = 2;
            // 
            // FAMFileInspectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1136, 637);
            this.Controls.Add(this._splitContainer);
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
            this._mainToolStripContainer.ContentPanel.ResumeLayout(false);
            this._mainToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this._mainToolStripContainer.TopToolStripPanel.PerformLayout();
            this._mainToolStripContainer.ResumeLayout(false);
            this._mainToolStripContainer.PerformLayout();
            this._statusToolStrip.ResumeLayout(false);
            this._statusToolStrip.PerformLayout();
            this._searchSplitContainer.Panel1.ResumeLayout(false);
            this._searchSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._searchSplitContainer)).EndInit();
            this._searchSplitContainer.ResumeLayout(false);
            this._fileListPanel.ResumeLayout(false);
            this._fileListPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._fileListDataGridView)).EndInit();
            this.dockContainer1.ResumeLayout(false);
            this._searchDockableWindow.ResumeLayout(false);
            this._searchDockableWindow.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dataSearchTermsDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._textSearchTermsDataGridView)).EndInit();
            this._imageToolStripContainer.ContentPanel.ResumeLayout(false);
            this._imageToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this._imageToolStripContainer.TopToolStripPanel.PerformLayout();
            this._imageToolStripContainer.ResumeLayout(false);
            this._imageToolStripContainer.PerformLayout();
            this._imageViewerToolsToolStrip.ResumeLayout(false);
            this._imageViewerToolsToolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.SplitContainer _splitContainer;
        private System.Windows.Forms.ToolStripContainer _imageToolStripContainer;
        private TD.SandDock.SandDockManager _sandDockManager;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
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
        private System.Windows.Forms.ToolStrip _statusToolStrip;
        private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        private System.Windows.Forms.ToolStripContentPanel ContentPanel;
        private System.Windows.Forms.ToolStripLabel _statusToolStripLabel;
        private System.Windows.Forms.ComboBox _searchModifierComboBox;
        private System.Windows.Forms.ComboBox _searchTypeComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView _textSearchTermsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn _textSearchTermsColumn;
        private System.Windows.Forms.Button _searchButton;
        private System.Windows.Forms.Button _clearButton;
        private System.Windows.Forms.DataGridView _fileListDataGridView;
        private Extract.Imaging.Forms.ImageViewer _imageViewer;
        private TD.SandDock.DockContainer dockContainer1;
        private System.Windows.Forms.DataGridViewTextBoxColumn _fileListNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _fileListPagesColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _fileListMatchesColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _fileListFolderColumn;
        private System.Windows.Forms.CheckBox _showOnlyMatchesCheckBox;
        private System.Windows.Forms.Panel _fileListPanel;
        private System.Windows.Forms.DataGridView _dataSearchTermsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn _dataSearchFieldColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn _dataSearchValueColumn;

    }
}

