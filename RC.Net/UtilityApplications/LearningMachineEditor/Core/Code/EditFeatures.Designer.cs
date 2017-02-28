using System.Diagnostics.CodeAnalysis;
namespace Extract.UtilityApplications.LearningMachineEditor
{
    /// <summary>
    /// Allows viewing/editing of computed feature vectorizers
    /// </summary>
    partial class EditFeatures
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
            if (disposing && _numberSelector != null)
            {
                _numberSelector.Dispose();
                _numberSelector = null;
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.summaryStatusStrip = new System.Windows.Forms.StatusStrip();
            this.featureListDataGridView = new System.Windows.Forms.DataGridView();
            this.featureVectorizersContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.limitToTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exportToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToFileDistinctToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.featureDetailsTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.distinctValuesSeenListBox = new System.Windows.Forms.ListBox();
            this.distinctValuesSeenContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.featureListDataGridView)).BeginInit();
            this.featureVectorizersContextMenuStrip.SuspendLayout();
            this.distinctValuesSeenContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.summaryStatusStrip);
            this.splitContainer1.Panel1.Controls.Add(this.featureListDataGridView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.featureDetailsTextBox);
            this.splitContainer1.Panel2.Controls.Add(this.cancelButton);
            this.splitContainer1.Panel2.Controls.Add(this.okButton);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.distinctValuesSeenListBox);
            this.splitContainer1.Size = new System.Drawing.Size(982, 574);
            this.splitContainer1.SplitterDistance = 470;
            this.splitContainer1.TabIndex = 0;
            // 
            // summaryStatusStrip
            // 
            this.summaryStatusStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.summaryStatusStrip.AutoSize = false;
            this.summaryStatusStrip.BackColor = System.Drawing.SystemColors.Control;
            this.summaryStatusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.summaryStatusStrip.Location = new System.Drawing.Point(3, 3);
            this.summaryStatusStrip.Name = "summaryStatusStrip";
            this.summaryStatusStrip.Size = new System.Drawing.Size(464, 22);
            this.summaryStatusStrip.SizingGrip = false;
            this.summaryStatusStrip.TabIndex = 0;
            this.summaryStatusStrip.Text = "statusStrip1";
            // 
            // featureListDataGridView
            // 
            this.featureListDataGridView.AllowUserToAddRows = false;
            this.featureListDataGridView.AllowUserToDeleteRows = false;
            this.featureListDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.featureListDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.featureListDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.featureListDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.featureListDataGridView.ContextMenuStrip = this.featureVectorizersContextMenuStrip;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.featureListDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.featureListDataGridView.Location = new System.Drawing.Point(3, 28);
            this.featureListDataGridView.Name = "featureListDataGridView";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.featureListDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.featureListDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.featureListDataGridView.Size = new System.Drawing.Size(464, 542);
            this.featureListDataGridView.TabIndex = 1;
            this.featureListDataGridView.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleFeatureListDataGridView_RowEnter);
            this.featureListDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HandleFeatureListDataGridView_MouseDown);
            // 
            // featureVectorizersContextMenuStrip
            // 
            this.featureVectorizersContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.limitToTopToolStripMenuItem,
            this.toolStripSeparator2,
            this.exportToFileToolStripMenuItem,
            this.exportToFileDistinctToolStripMenuItem});
            this.featureVectorizersContextMenuStrip.Name = "featureVectorizersContextMenuStrip";
            this.featureVectorizersContextMenuStrip.Size = new System.Drawing.Size(288, 76);
            this.featureVectorizersContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.HandleFeatureVectorizersContextMenuStrip_Opening);
            // 
            // limitToTopToolStripMenuItem
            // 
            this.limitToTopToolStripMenuItem.Name = "limitToTopToolStripMenuItem";
            this.limitToTopToolStripMenuItem.Size = new System.Drawing.Size(287, 22);
            this.limitToTopToolStripMenuItem.Text = "Limit selected to top terms...";
            this.limitToTopToolStripMenuItem.Click += new System.EventHandler(this.HandleLimitToTopToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(284, 6);
            // 
            // exportToFileToolStripMenuItem
            // 
            this.exportToFileToolStripMenuItem.Name = "exportToFileToolStripMenuItem";
            this.exportToFileToolStripMenuItem.Size = new System.Drawing.Size(287, 22);
            this.exportToFileToolStripMenuItem.Text = "Export values of selected to file...";
            this.exportToFileToolStripMenuItem.Click += new System.EventHandler(this.HandleExportToFileToolStripMenuItem_Click);
            // 
            // exportToFileDistinctToolStripMenuItem
            // 
            this.exportToFileDistinctToolStripMenuItem.Name = "exportToFileDistinctToolStripMenuItem";
            this.exportToFileDistinctToolStripMenuItem.Size = new System.Drawing.Size(287, 22);
            this.exportToFileDistinctToolStripMenuItem.Text = "Export distinct values of selected to file...";
            this.exportToFileDistinctToolStripMenuItem.Click += new System.EventHandler(this.HandleExportDistinctToFileToolStripMenuItem_Click);
            // 
            // featureDetailsTextBox
            // 
            this.featureDetailsTextBox.Location = new System.Drawing.Point(6, 19);
            this.featureDetailsTextBox.Name = "featureDetailsTextBox";
            this.featureDetailsTextBox.ReadOnly = true;
            this.featureDetailsTextBox.Size = new System.Drawing.Size(499, 20);
            this.featureDetailsTextBox.TabIndex = 1;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(421, 539);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(340, 539);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Recognized values";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Feature details";
            // 
            // distinctValuesSeenListBox
            // 
            this.distinctValuesSeenListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.distinctValuesSeenListBox.ContextMenuStrip = this.distinctValuesSeenContextMenuStrip;
            this.distinctValuesSeenListBox.FormattingEnabled = true;
            this.distinctValuesSeenListBox.Location = new System.Drawing.Point(6, 69);
            this.distinctValuesSeenListBox.Name = "distinctValuesSeenListBox";
            this.distinctValuesSeenListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.distinctValuesSeenListBox.Size = new System.Drawing.Size(499, 459);
            this.distinctValuesSeenListBox.TabIndex = 3;
            // 
            // distinctValuesSeenContextMenuStrip
            // 
            this.distinctValuesSeenContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.toolStripSeparator1,
            this.selectAllToolStripMenuItem});
            this.distinctValuesSeenContextMenuStrip.Name = "distinctValuesSeenContextMenuStrip";
            this.distinctValuesSeenContextMenuStrip.Size = new System.Drawing.Size(165, 54);
            this.distinctValuesSeenContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.HandleDistinctValuesSeenContextMenuStrip_Opening);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.HandleCopyToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(161, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.HandleSelectAllToolStripMenuItem_Click);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(116, 17);
            this.toolStripStatusLabel1.Text = "Total input features: ";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(136, 17);
            this.toolStripStatusLabel2.Text = "Total output categories: ";
            // 
            // EditFeatures
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(982, 574);
            this.Controls.Add(this.splitContainer1);
            this.MinimumSize = new System.Drawing.Size(700, 300);
            this.Name = "EditFeatures";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit features";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.featureListDataGridView)).EndInit();
            this.featureVectorizersContextMenuStrip.ResumeLayout(false);
            this.distinctValuesSeenContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox distinctValuesSeenListBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TextBox featureDetailsTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.StatusStrip summaryStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.DataGridView featureListDataGridView;
        private System.Windows.Forms.ContextMenuStrip distinctValuesSeenContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip featureVectorizersContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem limitToTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exportToFileDistinctToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToFileToolStripMenuItem;
    }
}