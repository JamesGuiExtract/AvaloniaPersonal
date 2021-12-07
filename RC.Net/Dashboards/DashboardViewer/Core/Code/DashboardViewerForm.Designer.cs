using Extract.Dashboard.Utilities;

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
            DevExpress.XtraEditors.TableLayout.ItemTemplateBase itemTemplateBase1 = new DevExpress.XtraEditors.TableLayout.ItemTemplateBase();
            DevExpress.XtraEditors.TableLayout.TableColumnDefinition tableColumnDefinition1 = new DevExpress.XtraEditors.TableLayout.TableColumnDefinition();
            DevExpress.XtraEditors.TableLayout.TableColumnDefinition tableColumnDefinition2 = new DevExpress.XtraEditors.TableLayout.TableColumnDefinition();
            DevExpress.XtraEditors.TableLayout.TemplatedItemElement templatedItemElement1 = new DevExpress.XtraEditors.TableLayout.TemplatedItemElement();
            DevExpress.XtraEditors.TableLayout.TemplatedItemElement templatedItemElement2 = new DevExpress.XtraEditors.TableLayout.TemplatedItemElement();
            DevExpress.XtraEditors.TableLayout.TableRowDefinition tableRowDefinition1 = new DevExpress.XtraEditors.TableLayout.TableRowDefinition();
            DevExpress.XtraEditors.TableLayout.ItemTemplateBase itemTemplateBase2 = new DevExpress.XtraEditors.TableLayout.ItemTemplateBase();
            DevExpress.XtraEditors.TableLayout.TableColumnDefinition tableColumnDefinition3 = new DevExpress.XtraEditors.TableLayout.TableColumnDefinition();
            DevExpress.XtraEditors.TableLayout.TableColumnDefinition tableColumnDefinition4 = new DevExpress.XtraEditors.TableLayout.TableColumnDefinition();
            DevExpress.XtraEditors.TableLayout.TableColumnDefinition tableColumnDefinition5 = new DevExpress.XtraEditors.TableLayout.TableColumnDefinition();
            DevExpress.XtraEditors.TableLayout.TemplatedItemElement templatedItemElement3 = new DevExpress.XtraEditors.TableLayout.TemplatedItemElement();
            DevExpress.XtraEditors.TableLayout.TemplatedItemElement templatedItemElement4 = new DevExpress.XtraEditors.TableLayout.TemplatedItemElement();
            DevExpress.XtraEditors.TableLayout.TableRowDefinition tableRowDefinition2 = new DevExpress.XtraEditors.TableLayout.TableRowDefinition();
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
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel2)).BeginInit();
            this.splitContainerControl1.Panel2.SuspendLayout();
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
            this.dashboardViewerMain.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.dashboardViewerMain.Appearance.Options.UseBackColor = true;
            this.dashboardViewerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dashboardViewerMain.Location = new System.Drawing.Point(0, 0);
            this.dashboardViewerMain.Name = "dashboardViewerMain";
            this.dashboardViewerMain.Size = new System.Drawing.Size(939, 416);
            this.dashboardViewerMain.TabIndex = 0;
            this.dashboardViewerMain.DashboardChanged += new System.EventHandler(this.HandleDashboardViewerMainDashboardChanged);
            this.dashboardViewerMain.ConfigureDataConnection += new DevExpress.DashboardCommon.DashboardConfigureDataConnectionEventHandler(this.HandleDashboardViewerMainConfigureDataConnection);
            this.dashboardViewerMain.CustomParameters += new DevExpress.DashboardCommon.CustomParametersEventHandler(this.HandleDashboardView_CustomParameters);
            this.dashboardViewerMain.DataLoadingError += new DevExpress.DashboardCommon.DataLoadingErrorEventHandler(this.HandleDashboardViewerMain_DataLoadingError);
            this.dashboardViewerMain.MasterFilterSet += new DevExpress.DashboardCommon.MasterFilterSetEventHandler(this.HandleDashboardViewerMainMasterFilterSet);
            this.dashboardViewerMain.MasterFilterCleared += new DevExpress.DashboardCommon.MasterFilterClearedEventHandler(this.HandleDashboardViewerMainMasterFilterCleared);
            this.dashboardViewerMain.ValidateCustomSqlQuery += new DevExpress.DashboardCommon.ValidateDashboardCustomSqlQueryEventHandler(DashboardHelpers.HandleDashboardCustomSqlQuery);
            this.dashboardViewerMain.DrillDownPerformed += new DevExpress.DashboardCommon.DrillActionEventHandler(this.HandleDashboardViewerMainDrillDownPerformed);
            this.dashboardViewerMain.DrillUpPerformed += new DevExpress.DashboardCommon.DrillActionEventHandler(this.HandleDashboardViewerMainDrillUpPerformed);
            this.dashboardViewerMain.DashboardItemDoubleClick += new DevExpress.DashboardWin.DashboardItemMouseActionEventHandler(this.HandleDashboardViewerMainDashboardItemDoubleClick);
            this.dashboardViewerMain.PopupMenuShowing += new DevExpress.DashboardWin.DashboardPopupMenuShowingEventHandler(this.HandlePopupMenuShowing);
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 51);
            this.splitContainerControl1.Name = "splitContainerControl1";
            // 
            // splitContainerControl1.Panel1
            // 
            this.splitContainerControl1.Panel1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
            this.splitContainerControl1.Panel1.Text = "Panel1";
            // 
            // splitContainerControl1.Panel2
            // 
            this.splitContainerControl1.Panel2.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
            this.splitContainerControl1.Panel2.Controls.Add(this.dashboardFlyoutPanel);
            this.splitContainerControl1.Panel2.Controls.Add(this.dashboardViewerMain);
            this.splitContainerControl1.Panel2.MinSize = 1000;
            this.splitContainerControl1.Panel2.Text = "Panel2";
            this.splitContainerControl1.Size = new System.Drawing.Size(953, 420);
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
            this._dashboardsInDBListBoxControl.ItemHeight = 34;
            this._dashboardsInDBListBoxControl.Location = new System.Drawing.Point(2, 2);
            this._dashboardsInDBListBoxControl.Name = "_dashboardsInDBListBoxControl";
            this._dashboardsInDBListBoxControl.Size = new System.Drawing.Size(185, 424);
            this._dashboardsInDBListBoxControl.TabIndex = 1;
            tableColumnDefinition1.Length.Value = 148D;
            tableColumnDefinition2.Length.Value = 48D;
            itemTemplateBase1.Columns.Add(tableColumnDefinition1);
            itemTemplateBase1.Columns.Add(tableColumnDefinition2);
            templatedItemElement1.FieldName = "DisplayMember";
            templatedItemElement1.ImageOptions.ImageAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
            templatedItemElement1.ImageOptions.ImageScaleMode = DevExpress.XtraEditors.TileItemImageScaleMode.ZoomInside;
            templatedItemElement1.Text = "DisplayMember";
            templatedItemElement1.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleLeft;
            templatedItemElement2.ColumnIndex = 1;
            templatedItemElement2.FieldName = null;
            templatedItemElement2.ImageOptions.ImageAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
            templatedItemElement2.ImageOptions.ImageScaleMode = DevExpress.XtraEditors.TileItemImageScaleMode.ZoomInside;
            templatedItemElement2.Text = "";
            templatedItemElement2.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
            itemTemplateBase1.Elements.Add(templatedItemElement1);
            itemTemplateBase1.Elements.Add(templatedItemElement2);
            itemTemplateBase1.Name = "DatabaseDashboardTemplate";
            itemTemplateBase1.Rows.Add(tableRowDefinition1);
            tableColumnDefinition3.Length.Value = 163D;
            tableColumnDefinition4.Length.Value = 33D;
            tableColumnDefinition5.Length.Value = 0D;
            itemTemplateBase2.Columns.Add(tableColumnDefinition3);
            itemTemplateBase2.Columns.Add(tableColumnDefinition4);
            itemTemplateBase2.Columns.Add(tableColumnDefinition5);
            templatedItemElement3.FieldName = "DisplayMember";
            templatedItemElement3.ImageOptions.ImageAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
            templatedItemElement3.ImageOptions.ImageScaleMode = DevExpress.XtraEditors.TileItemImageScaleMode.ZoomInside;
            templatedItemElement3.Text = "DisplayMember";
            templatedItemElement3.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleLeft;
            templatedItemElement4.ColumnIndex = 1;
            templatedItemElement4.FieldName = null;
            templatedItemElement4.ImageOptions.ImageAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
            templatedItemElement4.ImageOptions.ImageScaleMode = DevExpress.XtraEditors.TileItemImageScaleMode.ZoomInside;
            templatedItemElement4.Text = "Core";
            templatedItemElement4.TextAlignment = DevExpress.XtraEditors.TileItemContentAlignment.MiddleCenter;
            itemTemplateBase2.Elements.Add(templatedItemElement3);
            itemTemplateBase2.Elements.Add(templatedItemElement4);
            itemTemplateBase2.Name = "CoreDashboardTemplate";
            itemTemplateBase2.Rows.Add(tableRowDefinition2);
            this._dashboardsInDBListBoxControl.Templates.Add(itemTemplateBase1);
            this._dashboardsInDBListBoxControl.Templates.Add(itemTemplateBase2);
            this._dashboardsInDBListBoxControl.CustomItemTemplate += new DevExpress.XtraEditors.CustomItemTemplateEventHandler(this.HandleDashboardsInDBListBoxControl_CustomItemTemplate);
            this._dashboardsInDBListBoxControl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.HandleDashboardsInDBListBoxControlMouseDoubleClick);
            // 
            // menuStripMain
            // 
            this.menuStripMain.ImageScalingSize = new System.Drawing.Size(20, 20);
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
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._toolStripButtonRefresh,
            this._toolStripTextBoxlastRefresh,
            this.toolStripButtonClearMasterFilter});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(953, 27);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // _toolStripButtonRefresh
            // 
            this._toolStripButtonRefresh.Image = ((System.Drawing.Image)(resources.GetObject("_toolStripButtonRefresh.Image")));
            this._toolStripButtonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripButtonRefresh.Margin = new System.Windows.Forms.Padding(5, 1, 0, 2);
            this._toolStripButtonRefresh.Name = "_toolStripButtonRefresh";
            this._toolStripButtonRefresh.Size = new System.Drawing.Size(70, 24);
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
            this._toolStripTextBoxlastRefresh.Size = new System.Drawing.Size(63, 27);
            // 
            // toolStripButtonClearMasterFilter
            // 
            this.toolStripButtonClearMasterFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonClearMasterFilter.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonClearMasterFilter.Image")));
            this.toolStripButtonClearMasterFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonClearMasterFilter.Name = "toolStripButtonClearMasterFilter";
            this.toolStripButtonClearMasterFilter.Size = new System.Drawing.Size(85, 24);
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
            this.IconOptions.Icon = ((System.Drawing.Icon)(resources.GetObject("DashboardViewerForm.IconOptions.Icon")));
            this.MainMenuStrip = this.menuStripMain;
            this.Name = "DashboardViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dashboard";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DashboardViewerForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dashboardViewerMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel2)).EndInit();
            this.splitContainerControl1.Panel2.ResumeLayout(false);
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

