namespace Extract.ReportViewer
{
    partial class ReportViewerForm
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

                if (_report != null)
                {
                    _report.Dispose();
                    _report = null;
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
            System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportViewerForm));
            this._crystalReportViewer = new CrystalDecisions.Windows.Forms.CrystalReportViewer();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this._fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveReportTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exportReportToPDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exportReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._aboutReportViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._pleaseWaitLabel = new System.Windows.Forms.Label();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(229, 6);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(229, 6);
            // 
            // _crystalReportViewer
            // 
            this._crystalReportViewer.ActiveViewIndex = -1;
            this._crystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._crystalReportViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._crystalReportViewer.Location = new System.Drawing.Point(0, 24);
            this._crystalReportViewer.Name = "_crystalReportViewer";
            this._crystalReportViewer.ReuseParameterValuesOnRefresh = true;
            this._crystalReportViewer.ShowCloseButton = false;
            this._crystalReportViewer.ShowGroupTreeButton = false;
            this._crystalReportViewer.ShowParameterPanelButton = false;
            this._crystalReportViewer.ShowRefreshButton = false;
            this._crystalReportViewer.Size = new System.Drawing.Size(792, 542);
            this._crystalReportViewer.TabIndex = 0;
            this._crystalReportViewer.ToolPanelView = CrystalDecisions.Windows.Forms.ToolPanelViewType.None;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileToolStripMenuItem,
            this._helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(792, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // _fileToolStripMenuItem
            // 
            this._fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._openReportToolStripMenuItem,
            this._refreshToolStripMenuItem,
            toolStripSeparator1,
            this._saveReportTemplateToolStripMenuItem,
            this._exportReportToPDFToolStripMenuItem,
            this._exportReportToolStripMenuItem,
            toolStripSeparator2,
            this._exitToolStripMenuItem});
            this._fileToolStripMenuItem.Name = "_fileToolStripMenuItem";
            this._fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this._fileToolStripMenuItem.Text = "&File";
            // 
            // _openReportToolStripMenuItem
            // 
            this._openReportToolStripMenuItem.Name = "_openReportToolStripMenuItem";
            this._openReportToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this._openReportToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this._openReportToolStripMenuItem.Text = "&Open report...";
            this._openReportToolStripMenuItem.Click += new System.EventHandler(this.HandleFileOpenReportClick);
            // 
            // _refreshToolStripMenuItem
            // 
            this._refreshToolStripMenuItem.Enabled = false;
            this._refreshToolStripMenuItem.Name = "_refreshToolStripMenuItem";
            this._refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this._refreshToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this._refreshToolStripMenuItem.Text = "&Refresh";
            this._refreshToolStripMenuItem.Click += new System.EventHandler(this.HandleRefreshOpenDocument);
            // 
            // _saveReportTemplateToolStripMenuItem
            // 
            this._saveReportTemplateToolStripMenuItem.Enabled = false;
            this._saveReportTemplateToolStripMenuItem.Name = "_saveReportTemplateToolStripMenuItem";
            this._saveReportTemplateToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this._saveReportTemplateToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this._saveReportTemplateToolStripMenuItem.Text = "&Save report template...";
            this._saveReportTemplateToolStripMenuItem.Click += new System.EventHandler(this.HandleFileSaveReportTemplateClick);
            // 
            // _exportReportToPDFToolStripMenuItem
            // 
            this._exportReportToPDFToolStripMenuItem.Enabled = false;
            this._exportReportToPDFToolStripMenuItem.Name = "_exportReportToPDFToolStripMenuItem";
            this._exportReportToPDFToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this._exportReportToPDFToolStripMenuItem.Text = "Export to &PDF...";
            this._exportReportToPDFToolStripMenuItem.Click += new System.EventHandler(this.HandleFileExportReportToPdfClick);
            // 
            // _exportReportToolStripMenuItem
            // 
            this._exportReportToolStripMenuItem.Enabled = false;
            this._exportReportToolStripMenuItem.Name = "_exportReportToolStripMenuItem";
            this._exportReportToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this._exportReportToolStripMenuItem.Text = "Expo&rt...";
            this._exportReportToolStripMenuItem.Click += new System.EventHandler(this.HandleFileExportReportClick);
            // 
            // _exitToolStripMenuItem
            // 
            this._exitToolStripMenuItem.Name = "_exitToolStripMenuItem";
            this._exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this._exitToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this._exitToolStripMenuItem.Text = "E&xit";
            this._exitToolStripMenuItem.Click += new System.EventHandler(this.HandleFileExitClick);
            // 
            // _helpToolStripMenuItem
            // 
            this._helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._aboutReportViewerToolStripMenuItem});
            this._helpToolStripMenuItem.Name = "_helpToolStripMenuItem";
            this._helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this._helpToolStripMenuItem.Text = "Help";
            // 
            // _aboutReportViewerToolStripMenuItem
            // 
            this._aboutReportViewerToolStripMenuItem.Name = "_aboutReportViewerToolStripMenuItem";
            this._aboutReportViewerToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this._aboutReportViewerToolStripMenuItem.Text = "About Report Viewer...";
            this._aboutReportViewerToolStripMenuItem.Click += new System.EventHandler(this.HandleHelpAboutClick);
            // 
            // _pleaseWaitLabel
            // 
            this._pleaseWaitLabel.AutoSize = true;
            this._pleaseWaitLabel.Location = new System.Drawing.Point(111, 244);
            this._pleaseWaitLabel.Name = "_pleaseWaitLabel";
            this._pleaseWaitLabel.Size = new System.Drawing.Size(560, 13);
            this._pleaseWaitLabel.TabIndex = 2;
            this._pleaseWaitLabel.Text = "Please wait while the specified report is loaded (this may take several minutes d" +
    "epending on the size of the database).";
            this._pleaseWaitLabel.Visible = false;
            // 
            // _progressBar
            // 
            this._progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._progressBar.Location = new System.Drawing.Point(114, 277);
            this._progressBar.MarqueeAnimationSpeed = 40;
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(557, 28);
            this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this._progressBar.TabIndex = 3;
            this._progressBar.Visible = false;
            // 
            // ReportViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 566);
            this.Controls.Add(this._progressBar);
            this.Controls.Add(this._pleaseWaitLabel);
            this.Controls.Add(this._crystalReportViewer);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "ReportViewerForm";
            this.Text = "Report Viewer";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CrystalDecisions.Windows.Forms.CrystalReportViewer _crystalReportViewer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem _fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exitToolStripMenuItem;
        private System.Windows.Forms.Label _pleaseWaitLabel;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.ToolStripMenuItem _openReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _saveReportTemplateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exportReportToPDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _aboutReportViewerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exportReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _refreshToolStripMenuItem;

    }
}

