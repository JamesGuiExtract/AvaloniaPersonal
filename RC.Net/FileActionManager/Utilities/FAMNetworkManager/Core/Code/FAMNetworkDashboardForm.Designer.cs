namespace Extract.FileActionManager.Utilities
{
    partial class FAMNetworkDashboardForm
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
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
                if (_refreshThreadEnded != null)
                {
                    _refreshData = false;
                    _endThreads.Set();
                    _refreshThreadEnded.WaitOne(30000);
                    _refreshThreadEnded.Dispose();
                    _refreshThreadEnded = null;
                }
                if (_cleanupThreadEnded != null)
                {
                    _endThreads.Set();
                    _cleanupThreadEnded.WaitOne(30000);
                    _cleanupThreadEnded.Dispose();
                    _cleanupThreadEnded = null;
                }
                if (_endThreads != null)
                {
                    _endThreads.Dispose();
                    _endThreads = null;
                }
                if (_refreshTimeChanged != null)
                {
                    _refreshTimeChanged.Dispose();
                    _refreshTimeChanged = null;
                }
                if (_deletedRows != null)
                {
                    foreach (var row in _deletedRows)
                    {
                        row.Dispose();
                    }
                    _deletedRows = null;
                }
                if (_formStateManager != null)
                {
                    _formStateManager.Dispose();
                    _formStateManager = null;
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
            System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this._toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this._machineListGridView = new System.Windows.Forms.DataGridView();
            this.MachineColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GroupColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FamServiceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FdrsServiceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CpuColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this._fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._aboutFamNetworkManagerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._manageGridToolStrip = new System.Windows.Forms.ToolStrip();
            this._openFileToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._saveFileToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._addMachineToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._removeMachineToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._editMachineGroupAndNameToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._groupFilterComboBox = new System.Windows.Forms.ToolStripComboBox();
            this._updateToolStrip = new System.Windows.Forms.ToolStrip();
            this._startServiceToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._stopServiceToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._startStopTargetComboBox = new System.Windows.Forms.ToolStripComboBox();
            this._modifyServiceDatabaseToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._autoRefreshDataToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._refreshDataToolStripButton = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._toolStripContainer.ContentPanel.SuspendLayout();
            this._toolStripContainer.TopToolStripPanel.SuspendLayout();
            this._toolStripContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._machineListGridView)).BeginInit();
            this._menuStrip.SuspendLayout();
            this._manageGridToolStrip.SuspendLayout();
            this._updateToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 39);
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 39);
            // 
            // _toolStripContainer
            // 
            // 
            // _toolStripContainer.ContentPanel
            // 
            this._toolStripContainer.ContentPanel.Controls.Add(this._machineListGridView);
            this._toolStripContainer.ContentPanel.Size = new System.Drawing.Size(742, 303);
            this._toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._toolStripContainer.Location = new System.Drawing.Point(0, 0);
            this._toolStripContainer.Name = "_toolStripContainer";
            this._toolStripContainer.Size = new System.Drawing.Size(742, 366);
            this._toolStripContainer.TabIndex = 0;
            this._toolStripContainer.Text = "toolStripContainer1";
            // 
            // _toolStripContainer.TopToolStripPanel
            // 
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._menuStrip);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._manageGridToolStrip);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._updateToolStrip);
            // 
            // _machineListGridView
            // 
            this._machineListGridView.AllowUserToAddRows = false;
            this._machineListGridView.AllowUserToDeleteRows = false;
            this._machineListGridView.AllowUserToResizeRows = false;
            this._machineListGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._machineListGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this._machineListGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._machineListGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.MachineColumn,
            this.GroupColumn,
            this.FamServiceColumn,
            this.FdrsServiceColumn,
            this.CpuColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._machineListGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this._machineListGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._machineListGridView.Location = new System.Drawing.Point(0, 0);
            this._machineListGridView.Name = "_machineListGridView";
            this._machineListGridView.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this._machineListGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this._machineListGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this._machineListGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._machineListGridView.Size = new System.Drawing.Size(742, 303);
            this._machineListGridView.TabIndex = 0;
            this._machineListGridView.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.HandleMachineGridViewMouseDoubleClick);
            this._machineListGridView.RowHeaderMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.HandleMachineGridViewMouseDoubleClick);
            this._machineListGridView.SelectionChanged += new System.EventHandler(this.HandleMachineGridViewSelectionChanged);
            // 
            // MachineColumn
            // 
            this.MachineColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.MachineColumn.FillWeight = 300F;
            this.MachineColumn.HeaderText = "Machine";
            this.MachineColumn.MinimumWidth = 50;
            this.MachineColumn.Name = "MachineColumn";
            this.MachineColumn.ReadOnly = true;
            // 
            // GroupColumn
            // 
            this.GroupColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.GroupColumn.FillWeight = 300F;
            this.GroupColumn.HeaderText = "Group";
            this.GroupColumn.MinimumWidth = 50;
            this.GroupColumn.Name = "GroupColumn";
            this.GroupColumn.ReadOnly = true;
            // 
            // FamServiceColumn
            // 
            this.FamServiceColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FamServiceColumn.FillWeight = 200F;
            this.FamServiceColumn.HeaderText = "FAM Status";
            this.FamServiceColumn.MinimumWidth = 50;
            this.FamServiceColumn.Name = "FamServiceColumn";
            this.FamServiceColumn.ReadOnly = true;
            // 
            // FdrsServiceColumn
            // 
            this.FdrsServiceColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FdrsServiceColumn.FillWeight = 200F;
            this.FdrsServiceColumn.HeaderText = "FDRS Status";
            this.FdrsServiceColumn.MinimumWidth = 50;
            this.FdrsServiceColumn.Name = "FdrsServiceColumn";
            this.FdrsServiceColumn.ReadOnly = true;
            // 
            // CpuColumn
            // 
            this.CpuColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.CpuColumn.HeaderText = "CPU";
            this.CpuColumn.MinimumWidth = 50;
            this.CpuColumn.Name = "CpuColumn";
            this.CpuColumn.ReadOnly = true;
            // 
            // _menuStrip
            // 
            this._menuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileToolStripMenuItem,
            this._toolsToolStripMenuItem,
            this._helpToolStripMenuItem});
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.Size = new System.Drawing.Size(742, 24);
            this._menuStrip.TabIndex = 0;
            this._menuStrip.Text = "menuStrip1";
            // 
            // _fileToolStripMenuItem
            // 
            this._fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._newToolStripMenuItem,
            this._openToolStripMenuItem,
            this._saveToolStripMenuItem,
            this._saveAsToolStripMenuItem,
            this.toolStripSeparator2,
            this._exitToolStripMenuItem});
            this._fileToolStripMenuItem.Name = "_fileToolStripMenuItem";
            this._fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this._fileToolStripMenuItem.Text = "&File";
            // 
            // _newToolStripMenuItem
            // 
            this._newToolStripMenuItem.Name = "_newToolStripMenuItem";
            this._newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this._newToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this._newToolStripMenuItem.Text = "&New";
            this._newToolStripMenuItem.Click += new System.EventHandler(this.HandleNewToolStripMenuItemClick);
            // 
            // _openToolStripMenuItem
            // 
            this._openToolStripMenuItem.Name = "_openToolStripMenuItem";
            this._openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this._openToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this._openToolStripMenuItem.Text = "&Open...";
            this._openToolStripMenuItem.Click += new System.EventHandler(this.HandleOpenFileClick);
            // 
            // _saveToolStripMenuItem
            // 
            this._saveToolStripMenuItem.Name = "_saveToolStripMenuItem";
            this._saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this._saveToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this._saveToolStripMenuItem.Text = "&Save...";
            this._saveToolStripMenuItem.Click += new System.EventHandler(this.HandleSaveFileClick);
            // 
            // _saveAsToolStripMenuItem
            // 
            this._saveAsToolStripMenuItem.Name = "_saveAsToolStripMenuItem";
            this._saveAsToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this._saveAsToolStripMenuItem.Text = "Save &As...";
            this._saveAsToolStripMenuItem.Click += new System.EventHandler(this.HandleSaveAsClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(160, 6);
            // 
            // _exitToolStripMenuItem
            // 
            this._exitToolStripMenuItem.Name = "_exitToolStripMenuItem";
            this._exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this._exitToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this._exitToolStripMenuItem.Text = "E&xit";
            this._exitToolStripMenuItem.Click += new System.EventHandler(this.HandleExitMenuItemClick);
            // 
            // _toolsToolStripMenuItem
            // 
            this._toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._optionsToolStripMenuItem});
            this._toolsToolStripMenuItem.Name = "_toolsToolStripMenuItem";
            this._toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this._toolsToolStripMenuItem.Text = "&Tools";
            // 
            // _optionsToolStripMenuItem
            // 
            this._optionsToolStripMenuItem.Name = "_optionsToolStripMenuItem";
            this._optionsToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this._optionsToolStripMenuItem.Text = "&Options...";
            this._optionsToolStripMenuItem.Click += new System.EventHandler(this.HandleSetAutoRefreshThreadSleepTime);
            // 
            // _helpToolStripMenuItem
            // 
            this._helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._aboutFamNetworkManagerMenuItem});
            this._helpToolStripMenuItem.Name = "_helpToolStripMenuItem";
            this._helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this._helpToolStripMenuItem.Text = "&Help";
            // 
            // _aboutFamNetworkManagerMenuItem
            // 
            this._aboutFamNetworkManagerMenuItem.Name = "_aboutFamNetworkManagerMenuItem";
            this._aboutFamNetworkManagerMenuItem.Size = new System.Drawing.Size(126, 22);
            this._aboutFamNetworkManagerMenuItem.Text = "&About...";
            this._aboutFamNetworkManagerMenuItem.Click += new System.EventHandler(this.HandleAboutFamNetworkManagerMenuItem);
            // 
            // _manageGridToolStrip
            // 
            this._manageGridToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._manageGridToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._manageGridToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._openFileToolStripButton,
            this._saveFileToolStripButton,
            toolStripSeparator1,
            this._addMachineToolStripButton,
            this._removeMachineToolStripButton,
            this._editMachineGroupAndNameToolStripButton,
            this._groupFilterComboBox});
            this._manageGridToolStrip.Location = new System.Drawing.Point(3, 24);
            this._manageGridToolStrip.Name = "_manageGridToolStrip";
            this._manageGridToolStrip.Size = new System.Drawing.Size(400, 39);
            this._manageGridToolStrip.TabIndex = 1;
            // 
            // _openFileToolStripButton
            // 
            this._openFileToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._openFileToolStripButton.Image = global::Extract.FileActionManager.Utilities.Properties.Resources.OpenFileButton;
            this._openFileToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._openFileToolStripButton.Name = "_openFileToolStripButton";
            this._openFileToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._openFileToolStripButton.Text = "Open file";
            this._openFileToolStripButton.ToolTipText = "Open file (Ctrl+O)";
            this._openFileToolStripButton.Click += new System.EventHandler(this.HandleOpenFileClick);
            // 
            // _saveFileToolStripButton
            // 
            this._saveFileToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._saveFileToolStripButton.Image = global::Extract.FileActionManager.Utilities.Properties.Resources.SaveFileButton;
            this._saveFileToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._saveFileToolStripButton.Name = "_saveFileToolStripButton";
            this._saveFileToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._saveFileToolStripButton.Text = "Save file";
            this._saveFileToolStripButton.ToolTipText = "Save file (Ctrl+S)";
            this._saveFileToolStripButton.Click += new System.EventHandler(this.HandleSaveFileClick);
            // 
            // _addMachineToolStripButton
            // 
            this._addMachineToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._addMachineToolStripButton.Image = global::Extract.FileActionManager.Utilities.Properties.Resources.AddServerButton;
            this._addMachineToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._addMachineToolStripButton.Name = "_addMachineToolStripButton";
            this._addMachineToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._addMachineToolStripButton.Text = "Add machine";
            this._addMachineToolStripButton.ToolTipText = "Add machine (F8)";
            this._addMachineToolStripButton.Click += new System.EventHandler(this.HandleAddMachineButtonClick);
            // 
            // _removeMachineToolStripButton
            // 
            this._removeMachineToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._removeMachineToolStripButton.Image = global::Extract.FileActionManager.Utilities.Properties.Resources.RemoveServerButton;
            this._removeMachineToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._removeMachineToolStripButton.Name = "_removeMachineToolStripButton";
            this._removeMachineToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._removeMachineToolStripButton.Text = "Remove machine";
            this._removeMachineToolStripButton.ToolTipText = "Remove machine (Del)";
            this._removeMachineToolStripButton.Click += new System.EventHandler(this.HandleRemoveMachineButtonClick);
            // 
            // _editMachineGroupAndNameToolStripButton
            // 
            this._editMachineGroupAndNameToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._editMachineGroupAndNameToolStripButton.Image = global::Extract.FileActionManager.Utilities.Properties.Resources.EditGroupButton;
            this._editMachineGroupAndNameToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._editMachineGroupAndNameToolStripButton.Name = "_editMachineGroupAndNameToolStripButton";
            this._editMachineGroupAndNameToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._editMachineGroupAndNameToolStripButton.Text = "Edit machine(s) group and name";
            this._editMachineGroupAndNameToolStripButton.ToolTipText = "Edit machine(s) group and name (F2)";
            this._editMachineGroupAndNameToolStripButton.Click += new System.EventHandler(this.HandleEditMachineOrGroupButtonClick);
            // 
            // _groupFilterComboBox
            // 
            this._groupFilterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._groupFilterComboBox.Name = "_groupFilterComboBox";
            this._groupFilterComboBox.Size = new System.Drawing.Size(200, 39);
            this._groupFilterComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleFilterGroupsSelectedIndexChanged);
            // 
            // _updateToolStrip
            // 
            this._updateToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._updateToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._updateToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._startServiceToolStripButton,
            this._stopServiceToolStripButton,
            this._startStopTargetComboBox,
            toolStripSeparator3,
            this._modifyServiceDatabaseToolStripButton,
            this._autoRefreshDataToolStripButton,
            this._refreshDataToolStripButton});
            this._updateToolStrip.Location = new System.Drawing.Point(403, 24);
            this._updateToolStrip.Name = "_updateToolStrip";
            this._updateToolStrip.Size = new System.Drawing.Size(300, 39);
            this._updateToolStrip.TabIndex = 2;
            // 
            // _startServiceToolStripButton
            // 
            this._startServiceToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._startServiceToolStripButton.Image = global::Extract.FileActionManager.Utilities.Properties.Resources.StartServiceButton;
            this._startServiceToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._startServiceToolStripButton.Name = "_startServiceToolStripButton";
            this._startServiceToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._startServiceToolStripButton.Text = "Start service";
            this._startServiceToolStripButton.Click += new System.EventHandler(this.HandleStartServiceButtonClick);
            // 
            // _stopServiceToolStripButton
            // 
            this._stopServiceToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._stopServiceToolStripButton.Image = global::Extract.FileActionManager.Utilities.Properties.Resources.StopServiceButton;
            this._stopServiceToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._stopServiceToolStripButton.Name = "_stopServiceToolStripButton";
            this._stopServiceToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._stopServiceToolStripButton.Text = "Stop service";
            this._stopServiceToolStripButton.Click += new System.EventHandler(this.HandleStopServiceButtonClick);
            // 
            // _startStopTargetComboBox
            // 
            this._startStopTargetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._startStopTargetComboBox.Items.AddRange(new object[] {
            "Both",
            "FAM Service",
            "FDRS Service"});
            this._startStopTargetComboBox.Name = "_startStopTargetComboBox";
            this._startStopTargetComboBox.Size = new System.Drawing.Size(100, 39);
            this._startStopTargetComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleStartStopComboSelectedIndexChanged);
            // 
            // _modifyServiceDatabaseToolStripButton
            // 
            this._modifyServiceDatabaseToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._modifyServiceDatabaseToolStripButton.Image = global::Extract.FileActionManager.Utilities.Properties.Resources.ModifyDatabaseButton;
            this._modifyServiceDatabaseToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._modifyServiceDatabaseToolStripButton.Name = "_modifyServiceDatabaseToolStripButton";
            this._modifyServiceDatabaseToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._modifyServiceDatabaseToolStripButton.Text = "Modify service database";
            this._modifyServiceDatabaseToolStripButton.Click += new System.EventHandler(this.HandleEditServiceDatabaseButtonClick);
            // 
            // _autoRefreshDataToolStripButton
            // 
            this._autoRefreshDataToolStripButton.CheckOnClick = true;
            this._autoRefreshDataToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._autoRefreshDataToolStripButton.Image = global::Extract.FileActionManager.Utilities.Properties.Resources.AutoRefreshButton;
            this._autoRefreshDataToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._autoRefreshDataToolStripButton.Name = "_autoRefreshDataToolStripButton";
            this._autoRefreshDataToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._autoRefreshDataToolStripButton.Text = "Auto refresh status";
            this._autoRefreshDataToolStripButton.Click += new System.EventHandler(this.HandleAutoRefreshDataButtonClick);
            // 
            // _refreshDataToolStripButton
            // 
            this._refreshDataToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._refreshDataToolStripButton.Image = global::Extract.FileActionManager.Utilities.Properties.Resources.RefreshButton;
            this._refreshDataToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._refreshDataToolStripButton.Name = "_refreshDataToolStripButton";
            this._refreshDataToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._refreshDataToolStripButton.Text = "Refresh status";
            this._refreshDataToolStripButton.ToolTipText = "Refresh status (F5)";
            this._refreshDataToolStripButton.Click += new System.EventHandler(this.HandleRefreshDataButtonClick);
            // 
            // FAMNetworkDashboardForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(742, 366);
            this.Controls.Add(this._toolStripContainer);
            this.MainMenuStrip = this._menuStrip;
            this.MinimumSize = new System.Drawing.Size(500, 200);
            this.Name = "FAMNetworkDashboardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FAM Network Dashboard";
            this._toolStripContainer.ContentPanel.ResumeLayout(false);
            this._toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this._toolStripContainer.TopToolStripPanel.PerformLayout();
            this._toolStripContainer.ResumeLayout(false);
            this._toolStripContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._machineListGridView)).EndInit();
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this._manageGridToolStrip.ResumeLayout(false);
            this._manageGridToolStrip.PerformLayout();
            this._updateToolStrip.ResumeLayout(false);
            this._updateToolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer _toolStripContainer;
        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.ToolStripMenuItem _fileToolStripMenuItem;
        private System.Windows.Forms.DataGridView _machineListGridView;
        private System.Windows.Forms.ToolStrip _manageGridToolStrip;
        private System.Windows.Forms.ToolStripButton _openFileToolStripButton;
        private System.Windows.Forms.ToolStripButton _saveFileToolStripButton;
        private System.Windows.Forms.ToolStripButton _addMachineToolStripButton;
        private System.Windows.Forms.ToolStripButton _removeMachineToolStripButton;
        private System.Windows.Forms.ToolStripButton _editMachineGroupAndNameToolStripButton;
        private System.Windows.Forms.ToolStripButton _startServiceToolStripButton;
        private System.Windows.Forms.ToolStripButton _stopServiceToolStripButton;
        private System.Windows.Forms.ToolStripButton _modifyServiceDatabaseToolStripButton;
        private System.Windows.Forms.ToolStripButton _autoRefreshDataToolStripButton;
        private System.Windows.Forms.ToolStripButton _refreshDataToolStripButton;
        private System.Windows.Forms.ToolStripComboBox _groupFilterComboBox;
        private System.Windows.Forms.ToolStripMenuItem _helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _aboutFamNetworkManagerMenuItem;
        private System.Windows.Forms.ToolStripComboBox _startStopTargetComboBox;
        private System.Windows.Forms.ToolStrip _updateToolStrip;
        private System.Windows.Forms.ToolStripMenuItem _exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem _saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _optionsToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn MachineColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn GroupColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn FamServiceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn FdrsServiceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn CpuColumn;
        private System.Windows.Forms.ToolStripMenuItem _newToolStripMenuItem;
    }
}

