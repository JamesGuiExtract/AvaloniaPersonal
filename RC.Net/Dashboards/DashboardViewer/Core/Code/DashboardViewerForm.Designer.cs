namespace Extract.DashboardViewer
{
    partial class DashboardViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DashboardViewerForm));
            this.dashboardViewerMain = new DevExpress.DashboardWin.DashboardViewer(this.components);
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.dashboardFlyoutPanel = new DevExpress.Utils.FlyoutPanel();
            this.flyoutPanelControl1 = new DevExpress.Utils.FlyoutPanelControl();
            this._dashboardsInDBListBoxControl = new DevExpress.XtraEditors.ListBoxControl();
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dashboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._toolStripButtonRefresh = new System.Windows.Forms.ToolStripButton();
            this._toolStripTextBoxlastRefresh = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButtonClearMasterFilter = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.dashboardViewerMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            this.splitContainerControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dashboardFlyoutPanel)).BeginInit();
            this.dashboardFlyoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.flyoutPanelControl1)).BeginInit();
            this.flyoutPanelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dashboardsInDBListBoxControl)).BeginInit();
            this.menuStripMain.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dashboardViewerMain
            // 
            this.dashboardViewerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dashboardViewerMain.Location = new System.Drawing.Point(0, 0);
            this.dashboardViewerMain.Name = "dashboardViewerMain";
            this.dashboardViewerMain.Size = new System.Drawing.Size(944, 418);
            this.dashboardViewerMain.TabIndex = 0;
            this.dashboardViewerMain.DashboardChanged += new System.EventHandler(this.HandleDashboardViewerMainDashboardChanged);
            this.dashboardViewerMain.ConfigureDataConnection += new DevExpress.DashboardCommon.DashboardConfigureDataConnectionEventHandler(this.HandleDashboardViewerMainConfigureDataConnection);
            this.dashboardViewerMain.MasterFilterSet += new DevExpress.DashboardCommon.MasterFilterSetEventHandler(this.HandleDashboardViewerMainMasterFilterSet);
            this.dashboardViewerMain.MasterFilterCleared += new DevExpress.DashboardCommon.MasterFilterClearedEventHandler(this.HandleDashboardViewerMainMasterFilterCleared);
            this.dashboardViewerMain.DrillDownPerformed += new DevExpress.DashboardCommon.DrillActionEventHandler(this.HandleDashboardViewerMainDrillDownPerformed);
            this.dashboardViewerMain.DrillUpPerformed += new DevExpress.DashboardCommon.DrillActionEventHandler(this.HandleDashboardViewerMainDrillUpPerformed);
            this.dashboardViewerMain.DashboardItemDoubleClick += new DevExpress.DashboardWin.DashboardItemMouseActionEventHandler(this.HandleDashboardViewerMainDashboardItemDoubleClick);
            this.dashboardViewerMain.PopupMenuShowing += new DevExpress.DashboardWin.DashboardPopupMenuShowingEventHandler(this.HandlePopupMenuShowing);
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 49);
            this.splitContainerControl1.Name = "splitContainerControl1";
            this.splitContainerControl1.Panel1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
            this.splitContainerControl1.Panel1.Text = "Panel1";
            this.splitContainerControl1.Panel2.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
            this.splitContainerControl1.Panel2.Controls.Add(this.dashboardFlyoutPanel);
            this.splitContainerControl1.Panel2.Controls.Add(this.dashboardViewerMain);
            this.splitContainerControl1.Panel2.MinSize = 1000;
            this.splitContainerControl1.Panel2.Text = "Panel2";
            this.splitContainerControl1.Size = new System.Drawing.Size(953, 422);
            this.splitContainerControl1.SplitterPosition = 0;
            this.splitContainerControl1.TabIndex = 1;
            this.splitContainerControl1.Text = "splitContainerControl1";
            // 
            // dashboardFlyoutPanel
            // 
            this.dashboardFlyoutPanel.Controls.Add(this.flyoutPanelControl1);
            this.dashboardFlyoutPanel.Location = new System.Drawing.Point(5, 1);
            this.dashboardFlyoutPanel.Name = "dashboardFlyoutPanel";
            this.dashboardFlyoutPanel.Options.AnchorType = DevExpress.Utils.Win.PopupToolWindowAnchor.Left;
            this.dashboardFlyoutPanel.Options.AnimationType = DevExpress.Utils.Win.PopupToolWindowAnimation.Fade;
            this.dashboardFlyoutPanel.Options.CloseOnOuterClick = true;
            this.dashboardFlyoutPanel.OwnerControl = this.splitContainerControl1.Panel2;
            this.dashboardFlyoutPanel.Size = new System.Drawing.Size(189, 428);
            this.dashboardFlyoutPanel.TabIndex = 1;
            // 
            // flyoutPanelControl1
            // 
            this.flyoutPanelControl1.Controls.Add(this._dashboardsInDBListBoxControl);
            this.flyoutPanelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flyoutPanelControl1.FlyoutPanel = this.dashboardFlyoutPanel;
            this.flyoutPanelControl1.Location = new System.Drawing.Point(0, 0);
            this.flyoutPanelControl1.Name = "flyoutPanelControl1";
            this.flyoutPanelControl1.Size = new System.Drawing.Size(189, 428);
            this.flyoutPanelControl1.TabIndex = 0;
            // 
            // _dashboardsInDBListBoxControl
            // 
            this._dashboardsInDBListBoxControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dashboardsInDBListBoxControl.Location = new System.Drawing.Point(2, 2);
            this._dashboardsInDBListBoxControl.Name = "_dashboardsInDBListBoxControl";
            this._dashboardsInDBListBoxControl.Size = new System.Drawing.Size(185, 424);
            this._dashboardsInDBListBoxControl.TabIndex = 1;
            this._dashboardsInDBListBoxControl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.HandleDashboardsInDBListBoxControlMouseDoubleClick);
            // 
            // menuStripMain
            // 
            this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.dashboardToolStripMenuItem});
            this.menuStripMain.Location = new System.Drawing.Point(0, 0);
            this.menuStripMain.Name = "menuStripMain";
            this.menuStripMain.Size = new System.Drawing.Size(953, 24);
            this.menuStripMain.TabIndex = 2;
            this.menuStripMain.Text = "Main Menu";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.HandleOpenToolStripMenuItemClick);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.closeToolStripMenuItem.Text = "&Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.HandleCloseToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(100, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.HandleExitToolStripMenuItemClick);
            // 
            // dashboardToolStripMenuItem
            // 
            this.dashboardToolStripMenuItem.Name = "dashboardToolStripMenuItem";
            this.dashboardToolStripMenuItem.Size = new System.Drawing.Size(76, 20);
            this.dashboardToolStripMenuItem.Text = "Dashboard";
            this.dashboardToolStripMenuItem.Click += new System.EventHandler(this.HandleDashboardToolStripMenuItemClick);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._toolStripButtonRefresh,
            this._toolStripTextBoxlastRefresh,
            this.toolStripButtonClearMasterFilter});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(953, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // _toolStripButtonRefresh
            // 
            this._toolStripButtonRefresh.Image = ((System.Drawing.Image)(resources.GetObject("_toolStripButtonRefresh.Image")));
            this._toolStripButtonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripButtonRefresh.Margin = new System.Windows.Forms.Padding(5, 1, 0, 2);
            this._toolStripButtonRefresh.Name = "_toolStripButtonRefresh";
            this._toolStripButtonRefresh.Size = new System.Drawing.Size(66, 22);
            this._toolStripButtonRefresh.Text = "Refresh";
            this._toolStripButtonRefresh.Click += new System.EventHandler(this.HandleToolStripButtonRefresh_Click);
            // 
            // _toolStripTextBoxlastRefresh
            // 
            this._toolStripTextBoxlastRefresh.BackColor = System.Drawing.SystemColors.Info;
            this._toolStripTextBoxlastRefresh.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._toolStripTextBoxlastRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._toolStripTextBoxlastRefresh.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this._toolStripTextBoxlastRefresh.Name = "_toolStripTextBoxlastRefresh";
            this._toolStripTextBoxlastRefresh.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this._toolStripTextBoxlastRefresh.ReadOnly = true;
            this._toolStripTextBoxlastRefresh.Size = new System.Drawing.Size(130, 25);
            // 
            // toolStripButtonClearMasterFilter
            // 
            this.toolStripButtonClearMasterFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonClearMasterFilter.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonClearMasterFilter.Image")));
            this.toolStripButtonClearMasterFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonClearMasterFilter.Name = "toolStripButtonClearMasterFilter";
            this.toolStripButtonClearMasterFilter.Size = new System.Drawing.Size(85, 22);
            this.toolStripButtonClearMasterFilter.Text = "Clear all filters";
            this.toolStripButtonClearMasterFilter.Click += new System.EventHandler(this.HandleToolStripButtonClearMasterFilterClick);
            // 
            // DashboardViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(953, 471);
            this.Controls.Add(this.splitContainerControl1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStripMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStripMain;
            this.Name = "DashboardViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dashboard";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DashboardViewerForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dashboardViewerMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            this.splitContainerControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dashboardFlyoutPanel)).EndInit();
            this.dashboardFlyoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.flyoutPanelControl1)).EndInit();
            this.flyoutPanelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._dashboardsInDBListBoxControl)).EndInit();
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.DashboardWin.DashboardViewer dashboardViewerMain;
        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private DevExpress.Utils.FlyoutPanel dashboardFlyoutPanel;
        private DevExpress.Utils.FlyoutPanelControl flyoutPanelControl1;
        private DevExpress.XtraEditors.ListBoxControl _dashboardsInDBListBoxControl;
        private System.Windows.Forms.ToolStripMenuItem dashboardToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton _toolStripButtonRefresh;
        private System.Windows.Forms.ToolStripTextBox _toolStripTextBoxlastRefresh;
        private System.Windows.Forms.ToolStripButton toolStripButtonClearMasterFilter;
        //private Extract.Imaging.Forms.ImageViewer _imageViewer;
    }
}

