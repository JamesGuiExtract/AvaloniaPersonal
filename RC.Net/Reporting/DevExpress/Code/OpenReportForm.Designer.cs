namespace Extract.ReportingDevExpress
{
    partial class OpenReportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenReportForm));
            this._tabReportsList = new System.Windows.Forms.TabControl();
            this._tabStandardReports = new System.Windows.Forms.TabPage();
            this._standardReportList = new System.Windows.Forms.ListBox();
            this._tabSavedReports = new System.Windows.Forms.TabPage();
            this._savedReportList = new System.Windows.Forms.ListBox();
            this._groupReportPreview = new System.Windows.Forms.GroupBox();
            this._labelNoPreview = new System.Windows.Forms.Label();
            this._reportPreview = new System.Windows.Forms.PictureBox();
            this._topPanelSplitContainer = new System.Windows.Forms.SplitContainer();
            this._formSplitPanel = new System.Windows.Forms.SplitContainer();
            this._btnOk = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this._btnDeleteReport = new System.Windows.Forms.Button();
            this._tabReportsList.SuspendLayout();
            this._tabStandardReports.SuspendLayout();
            this._tabSavedReports.SuspendLayout();
            this._groupReportPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._reportPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._topPanelSplitContainer)).BeginInit();
            this._topPanelSplitContainer.Panel1.SuspendLayout();
            this._topPanelSplitContainer.Panel2.SuspendLayout();
            this._topPanelSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._formSplitPanel)).BeginInit();
            this._formSplitPanel.Panel1.SuspendLayout();
            this._formSplitPanel.Panel2.SuspendLayout();
            this._formSplitPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _tabReportsList
            // 
            this._tabReportsList.Controls.Add(this._tabStandardReports);
            this._tabReportsList.Controls.Add(this._tabSavedReports);
            this._tabReportsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tabReportsList.Location = new System.Drawing.Point(0, 0);
            this._tabReportsList.Name = "_tabReportsList";
            this._tabReportsList.SelectedIndex = 0;
            this._tabReportsList.Size = new System.Drawing.Size(373, 404);
            this._tabReportsList.TabIndex = 0;
            this._tabReportsList.Selected += new System.Windows.Forms.TabControlEventHandler(this.HandleTabPageChanged);
            // 
            // _tabStandardReports
            // 
            this._tabStandardReports.Controls.Add(this._standardReportList);
            this._tabStandardReports.Location = new System.Drawing.Point(4, 22);
            this._tabStandardReports.Name = "_tabStandardReports";
            this._tabStandardReports.Padding = new System.Windows.Forms.Padding(3);
            this._tabStandardReports.Size = new System.Drawing.Size(365, 378);
            this._tabStandardReports.TabIndex = 0;
            this._tabStandardReports.Text = "Standard reports";
            this._tabStandardReports.UseVisualStyleBackColor = true;
            // 
            // _standardReportList
            // 
            this._standardReportList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._standardReportList.FormattingEnabled = true;
            this._standardReportList.Location = new System.Drawing.Point(3, 3);
            this._standardReportList.Name = "_standardReportList";
            this._standardReportList.Size = new System.Drawing.Size(359, 372);
            this._standardReportList.TabIndex = 0;
            this._standardReportList.SelectedIndexChanged += new System.EventHandler(this.HandleStandardReportSelectedIndexChanged);
            this._standardReportList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.HandleReportDoubleClick);
            // 
            // _tabSavedReports
            // 
            this._tabSavedReports.Controls.Add(this._savedReportList);
            this._tabSavedReports.Location = new System.Drawing.Point(4, 22);
            this._tabSavedReports.Name = "_tabSavedReports";
            this._tabSavedReports.Padding = new System.Windows.Forms.Padding(3);
            this._tabSavedReports.Size = new System.Drawing.Size(365, 378);
            this._tabSavedReports.TabIndex = 1;
            this._tabSavedReports.Text = "Saved reports";
            this._tabSavedReports.UseVisualStyleBackColor = true;
            // 
            // _savedReportList
            // 
            this._savedReportList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._savedReportList.FormattingEnabled = true;
            this._savedReportList.Location = new System.Drawing.Point(3, 3);
            this._savedReportList.Name = "_savedReportList";
            this._savedReportList.Size = new System.Drawing.Size(359, 372);
            this._savedReportList.TabIndex = 0;
            this._savedReportList.SelectedIndexChanged += new System.EventHandler(this.HandleSavedReportSelectedIndexChanged);
            this._savedReportList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.HandleReportDoubleClick);
            // 
            // _groupReportPreview
            // 
            this._groupReportPreview.Controls.Add(this._labelNoPreview);
            this._groupReportPreview.Controls.Add(this._reportPreview);
            this._groupReportPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this._groupReportPreview.Location = new System.Drawing.Point(0, 0);
            this._groupReportPreview.Name = "_groupReportPreview";
            this._groupReportPreview.Size = new System.Drawing.Size(302, 404);
            this._groupReportPreview.TabIndex = 0;
            this._groupReportPreview.TabStop = false;
            this._groupReportPreview.Text = "Report preview";
            // 
            // _labelNoPreview
            // 
            this._labelNoPreview.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._labelNoPreview.AutoSize = true;
            this._labelNoPreview.Location = new System.Drawing.Point(59, 186);
            this._labelNoPreview.Name = "_labelNoPreview";
            this._labelNoPreview.Size = new System.Drawing.Size(203, 13);
            this._labelNoPreview.TabIndex = 1;
            this._labelNoPreview.Text = "No preview available for selected report.";
            this._labelNoPreview.Visible = false;
            // 
            // _reportPreview
            // 
            this._reportPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this._reportPreview.Location = new System.Drawing.Point(3, 17);
            this._reportPreview.Name = "_reportPreview";
            this._reportPreview.Size = new System.Drawing.Size(296, 384);
            this._reportPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._reportPreview.TabIndex = 0;
            this._reportPreview.TabStop = false;
            this._reportPreview.Resize += new System.EventHandler(this.HandlePictureBoxResize);
            // 
            // _topPanelSplitContainer
            // 
            this._topPanelSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._topPanelSplitContainer.Location = new System.Drawing.Point(0, 0);
            this._topPanelSplitContainer.Margin = new System.Windows.Forms.Padding(5);
            this._topPanelSplitContainer.Name = "_topPanelSplitContainer";
            // 
            // _topPanelSplitContainer.Panel1
            // 
            this._topPanelSplitContainer.Panel1.Controls.Add(this._tabReportsList);
            // 
            // _topPanelSplitContainer.Panel2
            // 
            this._topPanelSplitContainer.Panel2.Controls.Add(this._groupReportPreview);
            this._topPanelSplitContainer.Size = new System.Drawing.Size(679, 404);
            this._topPanelSplitContainer.SplitterDistance = 373;
            this._topPanelSplitContainer.TabIndex = 4;
            // 
            // _formSplitPanel
            // 
            this._formSplitPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._formSplitPanel.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this._formSplitPanel.IsSplitterFixed = true;
            this._formSplitPanel.Location = new System.Drawing.Point(14, 14);
            this._formSplitPanel.Margin = new System.Windows.Forms.Padding(5);
            this._formSplitPanel.Name = "_formSplitPanel";
            this._formSplitPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _formSplitPanel.Panel1
            // 
            this._formSplitPanel.Panel1.Controls.Add(this._topPanelSplitContainer);
            this._formSplitPanel.Panel1.Margin = new System.Windows.Forms.Padding(5);
            // 
            // _formSplitPanel.Panel2
            // 
            this._formSplitPanel.Panel2.Controls.Add(this._btnOk);
            this._formSplitPanel.Panel2.Controls.Add(this._btnCancel);
            this._formSplitPanel.Panel2.Controls.Add(this._btnDeleteReport);
            this._formSplitPanel.Size = new System.Drawing.Size(679, 438);
            this._formSplitPanel.SplitterDistance = 404;
            this._formSplitPanel.TabIndex = 5;
            // 
            // _btnOk
            // 
            this._btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._btnOk.Enabled = false;
            this._btnOk.Location = new System.Drawing.Point(523, 4);
            this._btnOk.Name = "_btnOk";
            this._btnOk.Size = new System.Drawing.Size(75, 23);
            this._btnOk.TabIndex = 2;
            this._btnOk.Text = "OK";
            this._btnOk.UseVisualStyleBackColor = true;
            // 
            // _btnCancel
            // 
            this._btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Location = new System.Drawing.Point(604, 4);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 23);
            this._btnCancel.TabIndex = 1;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _btnDeleteReport
            // 
            this._btnDeleteReport.Enabled = false;
            this._btnDeleteReport.Location = new System.Drawing.Point(0, 4);
            this._btnDeleteReport.Name = "_btnDeleteReport";
            this._btnDeleteReport.Size = new System.Drawing.Size(135, 23);
            this._btnDeleteReport.TabIndex = 0;
            this._btnDeleteReport.Text = "Delete selected report";
            this._btnDeleteReport.UseVisualStyleBackColor = true;
            this._btnDeleteReport.Click += new System.EventHandler(this.HandleDeleteReportClicked);
            // 
            // OpenReportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(707, 466);
            this.Controls.Add(this._formSplitPanel);
            this.IconOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("OpenReportForm.IconOptions.SvgImage")));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(709, 498);
            this.Name = "OpenReportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Open Report";
            this._tabReportsList.ResumeLayout(false);
            this._tabStandardReports.ResumeLayout(false);
            this._tabSavedReports.ResumeLayout(false);
            this._groupReportPreview.ResumeLayout(false);
            this._groupReportPreview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._reportPreview)).EndInit();
            this._topPanelSplitContainer.Panel1.ResumeLayout(false);
            this._topPanelSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._topPanelSplitContainer)).EndInit();
            this._topPanelSplitContainer.ResumeLayout(false);
            this._formSplitPanel.Panel1.ResumeLayout(false);
            this._formSplitPanel.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._formSplitPanel)).EndInit();
            this._formSplitPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl _tabReportsList;
        private System.Windows.Forms.TabPage _tabStandardReports;
        private System.Windows.Forms.ListBox _standardReportList;
        private System.Windows.Forms.TabPage _tabSavedReports;
        private System.Windows.Forms.ListBox _savedReportList;
        private System.Windows.Forms.GroupBox _groupReportPreview;
        private System.Windows.Forms.PictureBox _reportPreview;
        private System.Windows.Forms.SplitContainer _topPanelSplitContainer;
        private System.Windows.Forms.SplitContainer _formSplitPanel;
        private System.Windows.Forms.Button _btnDeleteReport;
        private System.Windows.Forms.Button _btnOk;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Label _labelNoPreview;
    }
}