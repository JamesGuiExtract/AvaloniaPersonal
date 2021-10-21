namespace Extract.SQLCDBEditor
{
    partial class SQLCDBEditorForm
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
            System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
            TD.SandDock.DockingRules dockingRules4 = new TD.SandDock.DockingRules();
            TD.SandDock.DockingRules dockingRules5 = new TD.SandDock.DockingRules();
            TD.SandDock.DockingRules dockingRules6 = new TD.SandDock.DockingRules();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SQLCDBEditorForm));
            this._toolStrip = new System.Windows.Forms.ToolStrip();
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this._fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._updateToCurrentSchemaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._newQueryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._statusStrip = new System.Windows.Forms.StatusStrip();
            this._databaseStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._dataStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._sandDockManager = new TD.SandDock.SandDockManager();
            this._tableDockWindow = new TD.SandDock.DockableWindow();
            this._tablesListBox = new System.Windows.Forms.ListBox();
            this._contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._openInSeparateTabMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._copyToNewQueryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._renameQueryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._navigationDockContainer = new TD.SandDock.DockContainer();
            this._queryDockWindow = new TD.SandDock.DockableWindow();
            this._queriesListBox = new System.Windows.Forms.ListBox();
            this._pluginDockableWindow = new TD.SandDock.DockableWindow();
            this._pluginsListBox = new System.Windows.Forms.ListBox();
            this._importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._saveToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._newQueryToolStripButton = new System.Windows.Forms.ToolStripButton();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this._toolStrip.SuspendLayout();
            this._menuStrip.SuspendLayout();
            this._statusStrip.SuspendLayout();
            this._tableDockWindow.SuspendLayout();
            this._contextMenuStrip.SuspendLayout();
            this._navigationDockContainer.SuspendLayout();
            this._queryDockWindow.SuspendLayout();
            this._pluginDockableWindow.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(100, 6);
            // 
            // _toolStrip
            // 
            this._toolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._openToolStripButton,
            this._saveToolStripButton,
            this._newQueryToolStripButton});
            this._toolStrip.Location = new System.Drawing.Point(0, 24);
            this._toolStrip.Name = "_toolStrip";
            this._toolStrip.Size = new System.Drawing.Size(847, 39);
            this._toolStrip.TabIndex = 0;
            this._toolStrip.Text = "toolStrip1";
            // 
            // _menuStrip
            // 
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileToolStripMenuItem,
            this._toolsToolStripMenuItem,
            this._helpToolStripMenuItem});
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.Size = new System.Drawing.Size(847, 24);
            this._menuStrip.TabIndex = 2;
            this._menuStrip.Text = "menuStrip1";
            // 
            // _fileToolStripMenuItem
            // 
            this._fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._openToolStripMenuItem,
            this._saveToolStripMenuItem,
            this._closeToolStripMenuItem,
            toolStripMenuItem1,
            this._exitToolStripMenuItem});
            this._fileToolStripMenuItem.Name = "_fileToolStripMenuItem";
            this._fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this._fileToolStripMenuItem.Text = "&File";
            // 
            // _openToolStripMenuItem
            // 
            this._openToolStripMenuItem.Name = "_openToolStripMenuItem";
            this._openToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this._openToolStripMenuItem.Text = "&Open";
            this._openToolStripMenuItem.Click += new System.EventHandler(this.HandleOpenClick);
            // 
            // _saveToolStripMenuItem
            // 
            this._saveToolStripMenuItem.Name = "_saveToolStripMenuItem";
            this._saveToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this._saveToolStripMenuItem.Text = "&Save";
            this._saveToolStripMenuItem.Click += new System.EventHandler(this.HandleSaveClick);
            // 
            // _closeToolStripMenuItem
            // 
            this._closeToolStripMenuItem.Name = "_closeToolStripMenuItem";
            this._closeToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this._closeToolStripMenuItem.Text = "&Close";
            this._closeToolStripMenuItem.Click += new System.EventHandler(this.CloseToolStripMenuItem_Click);
            // 
            // _exitToolStripMenuItem
            // 
            this._exitToolStripMenuItem.Name = "_exitToolStripMenuItem";
            this._exitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this._exitToolStripMenuItem.Text = "E&xit";
            this._exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // _toolsToolStripMenuItem
            // 
            this._toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._updateToCurrentSchemaToolStripMenuItem,
            this._newQueryToolStripMenuItem,
            this._importToolStripMenuItem,
            this._exportToolStripMenuItem});
            this._toolsToolStripMenuItem.Name = "_toolsToolStripMenuItem";
            this._toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this._toolsToolStripMenuItem.Text = "&Tools";
            // 
            // _updateToCurrentSchemaToolStripMenuItem
            // 
            this._updateToCurrentSchemaToolStripMenuItem.Name = "_updateToCurrentSchemaToolStripMenuItem";
            this._updateToCurrentSchemaToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this._updateToCurrentSchemaToolStripMenuItem.Text = "&Update to current schema...";
            this._updateToCurrentSchemaToolStripMenuItem.Click += new System.EventHandler(this.HandleUpdateToCurrentSchemaClick);
            // 
            // _newQueryToolStripMenuItem
            // 
            this._newQueryToolStripMenuItem.Enabled = false;
            this._newQueryToolStripMenuItem.Name = "_newQueryToolStripMenuItem";
            this._newQueryToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this._newQueryToolStripMenuItem.Text = "&Create new query";
            this._newQueryToolStripMenuItem.Click += new System.EventHandler(this.HandleNewQueryClick);
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
            this._aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this._aboutToolStripMenuItem.Text = "About";
            this._aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // _statusStrip
            // 
            this._statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._databaseStatusLabel,
            this._dataStatusLabel});
            this._statusStrip.Location = new System.Drawing.Point(0, 476);
            this._statusStrip.Name = "_statusStrip";
            this._statusStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this._statusStrip.Size = new System.Drawing.Size(847, 22);
            this._statusStrip.TabIndex = 3;
            this._statusStrip.Text = "statusStrip1";
            // 
            // _databaseStatusLabel
            // 
            this._databaseStatusLabel.Name = "_databaseStatusLabel";
            this._databaseStatusLabel.Size = new System.Drawing.Size(416, 17);
            this._databaseStatusLabel.Spring = true;
            this._databaseStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _dataStatusLabel
            // 
            this._dataStatusLabel.Name = "_dataStatusLabel";
            this._dataStatusLabel.Size = new System.Drawing.Size(416, 17);
            this._dataStatusLabel.Spring = true;
            this._dataStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _sandDockManager
            // 
            this._sandDockManager.AutoSaveLayout = true;
            this._sandDockManager.BorderStyle = TD.SandDock.Rendering.BorderStyle.None;
            this._sandDockManager.DockSystemContainer = this;
            this._sandDockManager.OwnerForm = this;
            this._sandDockManager.DockControlActivated += new TD.SandDock.DockControlEventHandler(this.HandleDockControlActivated);
            // 
            // _tableDockWindow
            // 
            this._tableDockWindow.AllowCollapse = false;
            this._tableDockWindow.Controls.Add(this._tablesListBox);
            dockingRules4.AllowDockBottom = true;
            dockingRules4.AllowDockLeft = true;
            dockingRules4.AllowDockRight = true;
            dockingRules4.AllowDockTop = true;
            dockingRules4.AllowFloat = true;
            dockingRules4.AllowTab = true;
            this._tableDockWindow.DockingRules = dockingRules4;
            this._tableDockWindow.Guid = new System.Guid("85a6af86-de66-4810-9629-cf367ab421f1");
            this._tableDockWindow.Location = new System.Drawing.Point(0, 18);
            this._tableDockWindow.Name = "_tableDockWindow";
            this._tableDockWindow.ShowOptions = false;
            this._tableDockWindow.Size = new System.Drawing.Size(233, 93);
            this._tableDockWindow.TabIndex = 0;
            this._tableDockWindow.Text = "Tables";
            this._tableDockWindow.Closing += new TD.SandDock.DockControlClosingEventHandler(this.HandleDockWindowClosing);
            // 
            // _tablesListBox
            // 
            this._tablesListBox.ContextMenuStrip = this._contextMenuStrip;
            this._tablesListBox.DisplayMember = "DisplayName";
            this._tablesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tablesListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this._tablesListBox.FormattingEnabled = true;
            this._tablesListBox.IntegralHeight = false;
            this._tablesListBox.Location = new System.Drawing.Point(0, 0);
            this._tablesListBox.Name = "_tablesListBox";
            this._tablesListBox.Size = new System.Drawing.Size(233, 93);
            this._tablesListBox.TabIndex = 0;
            this._tablesListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.HandleListBoxDrawItem);
            this._tablesListBox.SelectedIndexChanged += new System.EventHandler(this.HandleListSelectionChanged);
            this._tablesListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HandleListMouseDown);
            // 
            // _contextMenuStrip
            // 
            this._contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._openInSeparateTabMenuItem,
            this._copyToNewQueryToolStripMenuItem,
            this._renameQueryMenuItem,
            this._deleteToolStripMenuItem});
            this._contextMenuStrip.Name = "_contextMenuStrip";
            this._contextMenuStrip.Size = new System.Drawing.Size(184, 92);
            this._contextMenuStrip.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.HandleContextMenuStripClosed);
            this._contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.HandleContextMenuStripOpening);
            // 
            // _openInSeparateTabMenuItem
            // 
            this._openInSeparateTabMenuItem.Name = "_openInSeparateTabMenuItem";
            this._openInSeparateTabMenuItem.Size = new System.Drawing.Size(183, 22);
            this._openInSeparateTabMenuItem.Text = "Open in separate tab";
            this._openInSeparateTabMenuItem.Click += new System.EventHandler(this.HandleOpenInNewTabMenuItemClick);
            // 
            // _copyToNewQueryToolStripMenuItem
            // 
            this._copyToNewQueryToolStripMenuItem.Name = "_copyToNewQueryToolStripMenuItem";
            this._copyToNewQueryToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this._copyToNewQueryToolStripMenuItem.Text = "Copy to new query";
            this._copyToNewQueryToolStripMenuItem.Click += new System.EventHandler(this.HandleCopyToNewQueryMenuItemClick);
            // 
            // _renameQueryMenuItem
            // 
            this._renameQueryMenuItem.Name = "_renameQueryMenuItem";
            this._renameQueryMenuItem.Size = new System.Drawing.Size(183, 22);
            this._renameQueryMenuItem.Text = "Rename query";
            this._renameQueryMenuItem.Click += new System.EventHandler(this.HandleRenameQueryMenuItemClick);
            // 
            // _deleteToolStripMenuItem
            // 
            this._deleteToolStripMenuItem.Name = "_deleteToolStripMenuItem";
            this._deleteToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this._deleteToolStripMenuItem.Text = "Delete";
            this._deleteToolStripMenuItem.Click += new System.EventHandler(this.HandleDeleteMenuItemClick);
            // 
            // _navigationDockContainer
            // 
            this._navigationDockContainer.ContentSize = 233;
            this._navigationDockContainer.Controls.Add(this._tableDockWindow);
            this._navigationDockContainer.Controls.Add(this._queryDockWindow);
            this._navigationDockContainer.Controls.Add(this._pluginDockableWindow);
            this._navigationDockContainer.Dock = System.Windows.Forms.DockStyle.Left;
            this._navigationDockContainer.LayoutSystem = new TD.SandDock.SplitLayoutSystem(new System.Drawing.SizeF(250F, 400F), System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 400F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._tableDockWindow))}, this._tableDockWindow))),
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 400F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._queryDockWindow))}, this._queryDockWindow))),
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 400F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._pluginDockableWindow))}, this._pluginDockableWindow)))});
            this._navigationDockContainer.Location = new System.Drawing.Point(0, 63);
            this._navigationDockContainer.Manager = this._sandDockManager;
            this._navigationDockContainer.Name = "_navigationDockContainer";
            this._navigationDockContainer.Size = new System.Drawing.Size(237, 413);
            this._navigationDockContainer.TabIndex = 4;
            // 
            // _queryDockWindow
            // 
            this._queryDockWindow.AllowCollapse = false;
            this._queryDockWindow.Controls.Add(this._queriesListBox);
            dockingRules5.AllowDockBottom = true;
            dockingRules5.AllowDockLeft = true;
            dockingRules5.AllowDockRight = true;
            dockingRules5.AllowDockTop = true;
            dockingRules5.AllowFloat = true;
            dockingRules5.AllowTab = true;
            this._queryDockWindow.DockingRules = dockingRules5;
            this._queryDockWindow.Guid = new System.Guid("77c6358a-bf4e-4e01-b533-508c8c53771b");
            this._queryDockWindow.Location = new System.Drawing.Point(0, 157);
            this._queryDockWindow.Name = "_queryDockWindow";
            this._queryDockWindow.ShowOptions = false;
            this._queryDockWindow.Size = new System.Drawing.Size(233, 93);
            this._queryDockWindow.TabIndex = 0;
            this._queryDockWindow.Text = "Queries";
            this._queryDockWindow.Closing += new TD.SandDock.DockControlClosingEventHandler(this.HandleDockWindowClosing);
            // 
            // _queriesListBox
            // 
            this._queriesListBox.ContextMenuStrip = this._contextMenuStrip;
            this._queriesListBox.DisplayMember = "DisplayName";
            this._queriesListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._queriesListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this._queriesListBox.FormattingEnabled = true;
            this._queriesListBox.IntegralHeight = false;
            this._queriesListBox.Location = new System.Drawing.Point(0, 0);
            this._queriesListBox.Name = "_queriesListBox";
            this._queriesListBox.Size = new System.Drawing.Size(233, 93);
            this._queriesListBox.TabIndex = 0;
            this._queriesListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.HandleListBoxDrawItem);
            this._queriesListBox.SelectedIndexChanged += new System.EventHandler(this.HandleListSelectionChanged);
            this._queriesListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HandleListMouseDown);
            this._queriesListBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.HandleQueryListPreviewKeyDown);
            // 
            // _pluginDockableWindow
            // 
            this._pluginDockableWindow.AllowCollapse = false;
            this._pluginDockableWindow.Controls.Add(this._pluginsListBox);
            dockingRules6.AllowDockBottom = true;
            dockingRules6.AllowDockLeft = true;
            dockingRules6.AllowDockRight = true;
            dockingRules6.AllowDockTop = true;
            dockingRules6.AllowFloat = true;
            dockingRules6.AllowTab = true;
            this._pluginDockableWindow.DockingRules = dockingRules6;
            this._pluginDockableWindow.Guid = new System.Guid("7ed945d9-f840-4970-8a04-7de7b125ba79");
            this._pluginDockableWindow.Location = new System.Drawing.Point(0, 296);
            this._pluginDockableWindow.Name = "_pluginDockableWindow";
            this._pluginDockableWindow.ShowOptions = false;
            this._pluginDockableWindow.Size = new System.Drawing.Size(233, 93);
            this._pluginDockableWindow.TabIndex = 0;
            this._pluginDockableWindow.Text = "Plugins";
            // 
            // _pluginsListBox
            // 
            this._pluginsListBox.ContextMenuStrip = this._contextMenuStrip;
            this._pluginsListBox.DisplayMember = "DisplayName";
            this._pluginsListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pluginsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this._pluginsListBox.FormattingEnabled = true;
            this._pluginsListBox.IntegralHeight = false;
            this._pluginsListBox.Location = new System.Drawing.Point(0, 0);
            this._pluginsListBox.Name = "_pluginsListBox";
            this._pluginsListBox.Size = new System.Drawing.Size(233, 93);
            this._pluginsListBox.TabIndex = 1;
            this._pluginsListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.HandleListBoxDrawItem);
            this._pluginsListBox.SelectedIndexChanged += new System.EventHandler(this.HandleListSelectionChanged);
            this._pluginsListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HandleListMouseDown);
            // 
            // _importToolStripMenuItem
            // 
            this._importToolStripMenuItem.Name = "_importToolStripMenuItem";
            this._importToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this._importToolStripMenuItem.Text = "Import...";
            this._importToolStripMenuItem.Click += new System.EventHandler(this.HandleImportClick);
            // 
            // _exportToolStripMenuItem
            // 
            this._exportToolStripMenuItem.Name = "_exportToolStripMenuItem";
            this._exportToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this._exportToolStripMenuItem.Text = "Export...";
            this._exportToolStripMenuItem.Click += new System.EventHandler(this.HandleExportClick);
            // 
            // _openToolStripButton
            // 
            this._openToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._openToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_openToolStripButton.Image")));
            this._openToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._openToolStripButton.Name = "_openToolStripButton";
            this._openToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._openToolStripButton.Text = "Open Database";
            this._openToolStripButton.Click += new System.EventHandler(this.HandleOpenClick);
            // 
            // _saveToolStripButton
            // 
            this._saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._saveToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_saveToolStripButton.Image")));
            this._saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._saveToolStripButton.Name = "_saveToolStripButton";
            this._saveToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._saveToolStripButton.Text = "Save Database";
            this._saveToolStripButton.Click += new System.EventHandler(this.HandleSaveClick);
            // 
            // _newQueryToolStripButton
            // 
            this._newQueryToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._newQueryToolStripButton.Enabled = false;
            this._newQueryToolStripButton.Image = global::Extract.SQLCDBEditor.Properties.Resources.DbQueryLarge;
            this._newQueryToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._newQueryToolStripButton.Name = "_newQueryToolStripButton";
            this._newQueryToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._newQueryToolStripButton.Text = "Create New Query";
            this._newQueryToolStripButton.Click += new System.EventHandler(this.HandleNewQueryClick);
            // 
            // SQLCDBEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.ClientSize = new System.Drawing.Size(847, 498);
            this.Controls.Add(this._navigationDockContainer);
            this.Controls.Add(this._statusStrip);
            this.Controls.Add(this._toolStrip);
            this.Controls.Add(this._menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this._menuStrip;
            this.MinimumSize = new System.Drawing.Size(575, 300);
            this.Name = "SQLCDBEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ExtractSQLiteEditor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SQLCDBEditorForm_FormClosing);
            this._toolStrip.ResumeLayout(false);
            this._toolStrip.PerformLayout();
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this._statusStrip.ResumeLayout(false);
            this._statusStrip.PerformLayout();
            this._tableDockWindow.ResumeLayout(false);
            this._contextMenuStrip.ResumeLayout(false);
            this._navigationDockContainer.ResumeLayout(false);
            this._queryDockWindow.ResumeLayout(false);
            this._pluginDockableWindow.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip _toolStrip;
        private System.Windows.Forms.ToolStripButton _openToolStripButton;
        private System.Windows.Forms.ToolStripButton _saveToolStripButton;
        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.ToolStripMenuItem _fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _updateToCurrentSchemaToolStripMenuItem;
        private System.Windows.Forms.StatusStrip _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _databaseStatusLabel;
        private TD.SandDock.SandDockManager _sandDockManager;
        private TD.SandDock.DockableWindow _tableDockWindow;
        private TD.SandDock.DockContainer _navigationDockContainer;
        private TD.SandDock.DockableWindow _queryDockWindow;
        private System.Windows.Forms.ContextMenuStrip _contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem _openInSeparateTabMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _renameQueryMenuItem;
        private System.Windows.Forms.ListBox _tablesListBox;
        private System.Windows.Forms.ListBox _queriesListBox;
        private System.Windows.Forms.ToolStripButton _newQueryToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem _newQueryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _copyToNewQueryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _deleteToolStripMenuItem;
        private TD.SandDock.DockableWindow _pluginDockableWindow;
        private System.Windows.Forms.ListBox _pluginsListBox;
        private System.Windows.Forms.ToolStripStatusLabel _dataStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem _importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exportToolStripMenuItem;

    }
}

