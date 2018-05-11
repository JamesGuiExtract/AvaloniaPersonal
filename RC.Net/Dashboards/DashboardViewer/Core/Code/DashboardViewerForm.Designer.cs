﻿namespace DashboardViewer
{
    partial class DashboardViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DashboardViewerForm));
            this.dashboardViewerMain = new DevExpress.DashboardWin.DashboardViewer(this.components);
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._dashboardsInDBListBoxControl = new DevExpress.XtraEditors.ListBoxControl();
            this.dashboardFlyoutPanel = new DevExpress.Utils.FlyoutPanel();
            this.flyoutPanelControl1 = new DevExpress.Utils.FlyoutPanelControl();
            this.dashboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dashboardViewerMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            this.splitContainerControl1.SuspendLayout();
            this.menuStripMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dashboardsInDBListBoxControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dashboardFlyoutPanel)).BeginInit();
            this.dashboardFlyoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.flyoutPanelControl1)).BeginInit();
            this.flyoutPanelControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dashboardViewerMain
            // 
            this.dashboardViewerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dashboardViewerMain.Location = new System.Drawing.Point(0, 0);
            this.dashboardViewerMain.Name = "dashboardViewerMain";
            this.dashboardViewerMain.Size = new System.Drawing.Size(944, 443);
            this.dashboardViewerMain.TabIndex = 0;
            this.dashboardViewerMain.DashboardChanged += new System.EventHandler(this.HandleDashboardViewerMainDashboardChanged);
            this.dashboardViewerMain.ConfigureDataConnection += new DevExpress.DashboardCommon.DashboardConfigureDataConnectionEventHandler(this.HandleDashboardViewerMainConfigureDataConnection);
            this.dashboardViewerMain.DrillDownPerformed += new DevExpress.DashboardCommon.DrillActionEventHandler(this.HandleDashboardViewerMainDrillDownPerformed);
            this.dashboardViewerMain.DrillUpPerformed += new DevExpress.DashboardCommon.DrillActionEventHandler(this.HandleDashboardViewerMainDrillUpPerformed);
            this.dashboardViewerMain.DashboardItemDoubleClick += new DevExpress.DashboardWin.DashboardItemMouseActionEventHandler(this.HandleDashboardViewerMainDashboardItemDoubleClick);
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 24);
            this.splitContainerControl1.Name = "splitContainerControl1";
            this.splitContainerControl1.Panel1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
            this.splitContainerControl1.Panel1.Text = "Panel1";
            this.splitContainerControl1.Panel2.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
            this.splitContainerControl1.Panel2.Controls.Add(this.dashboardFlyoutPanel);
            this.splitContainerControl1.Panel2.Controls.Add(this.dashboardViewerMain);
            this.splitContainerControl1.Panel2.MinSize = 1000;
            this.splitContainerControl1.Panel2.Text = "Panel2";
            this.splitContainerControl1.Size = new System.Drawing.Size(953, 447);
            this.splitContainerControl1.SplitterPosition = 0;
            this.splitContainerControl1.TabIndex = 1;
            this.splitContainerControl1.Text = "splitContainerControl1";
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
            this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.HandleOpenToolStripMenuItemClick);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.closeToolStripMenuItem.Text = "&Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.HandleCloseToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.HandleExitToolStripMenuItemClick);
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
            // flyoutPanel1
            // 
            this.dashboardFlyoutPanel.Controls.Add(this.flyoutPanelControl1);
            this.dashboardFlyoutPanel.Location = new System.Drawing.Point(5, 1);
            this.dashboardFlyoutPanel.Name = "flyoutPanel1";
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
            // dashboardToolStripMenuItem
            // 
            this.dashboardToolStripMenuItem.Name = "dashboardToolStripMenuItem";
            this.dashboardToolStripMenuItem.Size = new System.Drawing.Size(76, 20);
            this.dashboardToolStripMenuItem.Text = "Dashboard";
            this.dashboardToolStripMenuItem.Click += new System.EventHandler(this.HandleDashboardToolStripMenuItemClick);
            // 
            // DashboardViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(953, 471);
            this.Controls.Add(this.splitContainerControl1);
            this.Controls.Add(this.menuStripMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStripMain;
            this.Name = "DashboardViewerForm";
            this.Text = "Dashboard";
            ((System.ComponentModel.ISupportInitialize)(this.dashboardViewerMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            this.splitContainerControl1.ResumeLayout(false);
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dashboardsInDBListBoxControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dashboardFlyoutPanel)).EndInit();
            this.dashboardFlyoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.flyoutPanelControl1)).EndInit();
            this.flyoutPanelControl1.ResumeLayout(false);
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
        //private Extract.Imaging.Forms.ImageViewer _imageViewer;
    }
}

