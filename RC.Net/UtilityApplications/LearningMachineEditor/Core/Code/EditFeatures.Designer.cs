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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.fEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.fName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Features = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.statusStrip1.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.cancelButton);
            this.splitContainer1.Panel1.Controls.Add(this.okButton);
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.listBox1);
            this.splitContainer1.Size = new System.Drawing.Size(982, 574);
            this.splitContainer1.SplitterDistance = 470;
            this.splitContainer1.TabIndex = 0;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(381, 539);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(300, 539);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.fEnabled,
            this.fName,
            this.fType,
            this.Features});
            this.dataGridView1.Location = new System.Drawing.Point(3, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(464, 526);
            this.dataGridView1.TabIndex = 1;
            // 
            // fEnabled
            // 
            this.fEnabled.HeaderText = "Enabled";
            this.fEnabled.Name = "fEnabled";
            // 
            // fName
            // 
            this.fName.HeaderText = "Name";
            this.fName.Name = "fName";
            this.fName.ReadOnly = true;
            // 
            // fType
            // 
            this.fType.HeaderText = "Type";
            this.fType.Items.AddRange(new object[] {
            "DiscreteTerms",
            "Exists",
            "Numeric"});
            this.fType.Name = "fType";
            // 
            // Features
            // 
            this.Features.HeaderText = "# Features";
            this.Features.Name = "Features";
            this.Features.ReadOnly = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 552);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(508, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
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
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(142, 17);
            this.toolStripStatusLabel2.Text = "Total output categories: 8";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Feature details";
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Items.AddRange(new object[] {
            "OFFICE CONTACT",
            "FAX NUMBER FOR",
            "YOU OR YOUR OFFICE",
            "WAY WE CAN",
            "WAY WE",
            "THERE IS ANY WAY WE",
            "THERE IS ANY WAY",
            "THERE IS ANY",
            "THE TABLE",
            "QUESTIONS OR IF THERE IS",
            "QUESTIONS OR IF THERE",
            "QUESTIONS OR IF",
            "PROVIDER IN YOUR",
            "OR YOUR OFFICE",
            "OR IF THERE IS ANY",
            "OR IF THERE IS",
            "OR IF THERE",
            "IS ANY WAY WE CAN",
            "IS ANY WAY WE",
            "IS ANY WAY",
            "IS ANY",
            "INDIVIDUAL PROVIDER",
            "IF THERE IS ANY WAY",
            "IF THERE IS ANY",
            "HAVE ANY QUESTIONS OR IF",
            "EACH PROVIDER IN YOUR",
            "EACH PROVIDER IN",
            "EACH PROVIDER",
            "ANY WAY WE CAN",
            "ANY WAY WE",
            "ANY QUESTIONS OR IF THERE",
            "ANY QUESTIONS OR IF",
            "YOUR PATIENT YOUR",
            "WOULD LIKE TO COMPLETE THE",
            "WOULD LIKE TO COMPLETE",
            "WORLD CLASS SERVICE YOU CAN",
            "WORLD CLASS SERVICE YOU",
            "WORLD CLASS SERVICE",
            "WORLD CLASS",
            "WORLD",
            "WILL BE CANCELLED IN",
            "WILL BE CANCELLED",
            "US DIRECTION ON",
            "US DIRECTION",
            "TO COMPLETE THE SCREENING THANK",
            "THE SCREENING THANK YOU FOR",
            "THE SCREENING THANK YOU",
            "THE SCREENING THANK",
            "THE ORDER WILL BE CANCELLED",
            "THE ORDER OPEN PLEASE CALL",
            "THE ORDER OPEN PLEASE",
            "THE ORDER OPEN",
            "THE INITIAL ORDER DATE IF",
            "SIGN AND FAX",
            "SERVICE YOU CAN",
            "SERVICE YOU",
            "SCREENING THANK YOU FOR YOUR",
            "SCREENING THANK YOU FOR",
            "SCREENING THANK YOU",
            "SCREENING THANK",
            "PROVIDER CANCELLATION NOTIFICATION",
            "PROVIDER CANCELLATION",
            "PROVIDE US DIRECTION ON",
            "PROVIDE US DIRECTION",
            "PLEASE PROVIDE US DIRECTION ON",
            "PLEASE PROVIDE US DIRECTION",
            "PATIENT YOUR",
            "ORDER WILL BE CANCELLED IN",
            "ORDER WILL BE CANCELLED",
            "ORDER TO PROVIDE YOU",
            "ORDER TO PROVIDE",
            "ORDER OPEN PLEASE CALL",
            "ORDER OPEN PLEASE",
            "ORDER OPEN",
            "ORDER FOR YOUR PATIENT YOUR",
            "ORDER DATE IF THE",
            "ORDER DATE IF",
            "OR THE ORDER WILL BE",
            "OR THE ORDER WILL",
            "OR THE ORDER",
            "OPEN PLEASE CALL",
            "OPEN PLEASE",
            "OF THE INITIAL ORDER DATE",
            "OF THE INITIAL ORDER",
            "OF THE INITIAL",
            "LIKE TO COMPLETE THE SCREENING",
            "LIKE TO COMPLETE THE",
            "LIKE TO COMPLETE",
            "INITIAL ORDER DATE IF THE",
            "INITIAL ORDER DATE IF",
            "IN ORDER TO PROVIDE YOU",
            "IN ORDER TO PROVIDE",
            "HOURS IF",
            "FOR YOUR PATIENT YOUR",
            "FOR REQUEST",
            "DIRECTION ON",
            "DIRECTION",
            "DAYS OF THE INITIAL ORDER",
            "DAYS OF THE INITIAL",
            "DATE IF THE"});
            this.listBox1.Location = new System.Drawing.Point(3, 25);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(502, 524);
            this.listBox1.TabIndex = 0;
            // 
            // EditFeatures
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(982, 574);
            this.Controls.Add(this.splitContainer1);
            this.Name = "EditFeatures";
            this.Text = "Edit features";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dataGridView1;
        //private System.Windows.Forms.DataGridViewCheckBoxColumn featureEnabled;
        //private System.Windows.Forms.DataGridViewTextBoxColumn featureName;
        //private System.Windows.Forms.DataGridViewComboBoxColumn featureType;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn fEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn fName;
        private System.Windows.Forms.DataGridViewComboBoxColumn fType;
        private System.Windows.Forms.DataGridViewTextBoxColumn Features;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
    }
}