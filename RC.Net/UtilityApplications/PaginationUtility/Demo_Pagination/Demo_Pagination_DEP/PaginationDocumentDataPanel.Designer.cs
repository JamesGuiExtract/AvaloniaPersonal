namespace Extract.Demo_Pagination
{
    partial class PaginationDocumentDataPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            this._documentTypeComboBox = new System.Windows.Forms.ComboBox();
            this._patientFirstTextBox = new System.Windows.Forms.TextBox();
            this._patientDOBTextBox = new System.Windows.Forms.TextBox();
            this._patientLastTextBox = new System.Windows.Forms.TextBox();
            this._patientMiddleTextBox = new System.Windows.Forms.TextBox();
            this._patientMRNComboBox = new System.Windows.Forms.ComboBox();
            this._documentDateTextBox = new System.Windows.Forms.TextBox();
            this._patientSexComboBox = new System.Windows.Forms.ComboBox();
            this._tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._commentPanel = new System.Windows.Forms.Panel();
            this._documentCommentTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            this._tableLayoutPanel.SuspendLayout();
            this._commentPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = System.Drawing.Color.White;
            label1.Location = new System.Drawing.Point(1, 58);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(86, 13);
            label1.TabIndex = 11;
            label1.Text = "Document Type:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = System.Drawing.Color.White;
            label2.Location = new System.Drawing.Point(12, 6);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(74, 13);
            label2.TabIndex = 1;
            label2.Text = "Patient Name:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ForeColor = System.Drawing.Color.White;
            label3.Location = new System.Drawing.Point(53, 32);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(33, 13);
            label3.TabIndex = 5;
            label3.Text = "DOB:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = System.Drawing.Color.White;
            label4.Location = new System.Drawing.Point(311, 32);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(35, 13);
            label4.TabIndex = 9;
            label4.Text = "MRN:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = System.Drawing.Color.White;
            label5.Location = new System.Drawing.Point(261, 58);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(85, 13);
            label5.TabIndex = 11;
            label5.Text = "Document Date:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.ForeColor = System.Drawing.Color.White;
            label6.Location = new System.Drawing.Point(204, 32);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(28, 13);
            label6.TabIndex = 7;
            label6.Text = "Sex:";
            // 
            // _documentTypeComboBox
            // 
            this._documentTypeComboBox.FormattingEnabled = true;
            this._documentTypeComboBox.Items.AddRange(new object[] {
            "Cover Page",
            "History and Physical",
            "Insurance",
            "Lab Results",
            "Medications",
            "Prescription",
            "Radiology",
            "Referral Letter",
            "Regulatory",
            "Type and Screen"});
            this._documentTypeComboBox.Location = new System.Drawing.Point(93, 55);
            this._documentTypeComboBox.Name = "_documentTypeComboBox";
            this._documentTypeComboBox.Size = new System.Drawing.Size(139, 21);
            this._documentTypeComboBox.TabIndex = 12;
            this._documentTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleDocumentTypeComboBox_SelectedIndexChanged);
            // 
            // _patientFirstTextBox
            // 
            this._patientFirstTextBox.Location = new System.Drawing.Point(93, 3);
            this._patientFirstTextBox.Name = "_patientFirstTextBox";
            this._patientFirstTextBox.Size = new System.Drawing.Size(139, 20);
            this._patientFirstTextBox.TabIndex = 2;
            this._patientFirstTextBox.TextChanged += new System.EventHandler(this.HandlePatientFirstTextBox_TextChanged);
            // 
            // _patientDOBTextBox
            // 
            this._patientDOBTextBox.Location = new System.Drawing.Point(93, 29);
            this._patientDOBTextBox.Name = "_patientDOBTextBox";
            this._patientDOBTextBox.Size = new System.Drawing.Size(83, 20);
            this._patientDOBTextBox.TabIndex = 6;
            this._patientDOBTextBox.TextChanged += new System.EventHandler(this.HandlePatientDOBTextBox_TextChanged);
            // 
            // _patientLastTextBox
            // 
            this._patientLastTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._patientLastTextBox.Location = new System.Drawing.Point(352, 3);
            this._patientLastTextBox.Name = "_patientLastTextBox";
            this._patientLastTextBox.Size = new System.Drawing.Size(204, 20);
            this._patientLastTextBox.TabIndex = 4;
            this._patientLastTextBox.TextChanged += new System.EventHandler(this.HandlePatientLastTextBox_TextChanged);
            // 
            // _patientMiddleTextBox
            // 
            this._patientMiddleTextBox.Location = new System.Drawing.Point(238, 3);
            this._patientMiddleTextBox.Name = "_patientMiddleTextBox";
            this._patientMiddleTextBox.Size = new System.Drawing.Size(108, 20);
            this._patientMiddleTextBox.TabIndex = 3;
            this._patientMiddleTextBox.TextChanged += new System.EventHandler(this.HandlePatientMiddleTextBox_TextChanged);
            // 
            // _patientMRNComboBox
            // 
            this._patientMRNComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._patientMRNComboBox.Location = new System.Drawing.Point(352, 29);
            this._patientMRNComboBox.Name = "_patientMRNComboBox";
            this._patientMRNComboBox.Size = new System.Drawing.Size(204, 21);
            this._patientMRNComboBox.TabIndex = 10;
            this._patientMRNComboBox.SelectedIndexChanged += new System.EventHandler(this.HandlePatientMRNTextBox_SelectedIndexChanged);
            this._patientMRNComboBox.TextChanged += new System.EventHandler(this.HandlePatientMRNTextBox_TextChanged);
            // 
            // _documentDateTextBox
            // 
            this._documentDateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._documentDateTextBox.Location = new System.Drawing.Point(352, 55);
            this._documentDateTextBox.Name = "_documentDateTextBox";
            this._documentDateTextBox.Size = new System.Drawing.Size(204, 20);
            this._documentDateTextBox.TabIndex = 13;
            this._documentDateTextBox.TextChanged += new System.EventHandler(this.HandleDocumentDateTextBox_TextChanged);
            // 
            // _patientSexComboBox
            // 
            this._patientSexComboBox.FormattingEnabled = true;
            this._patientSexComboBox.Items.AddRange(new object[] {
            "F",
            "M"});
            this._patientSexComboBox.Location = new System.Drawing.Point(238, 28);
            this._patientSexComboBox.Name = "_patientSexComboBox";
            this._patientSexComboBox.Size = new System.Drawing.Size(53, 21);
            this._patientSexComboBox.TabIndex = 8;
            this._patientSexComboBox.SelectedIndexChanged += new System.EventHandler(this.HandlePatientSexComboBox_SelectedIndexChanged);
            // 
            // _tableLayoutPanel
            // 
            this._tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tableLayoutPanel.AutoSize = true;
            this._tableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._tableLayoutPanel.ColumnCount = 1;
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanel.Controls.Add(this._commentPanel, 0, 1);
            this._tableLayoutPanel.Location = new System.Drawing.Point(4, 82);
            this._tableLayoutPanel.Name = "_tableLayoutPanel";
            this._tableLayoutPanel.RowCount = 2;
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this._tableLayoutPanel.Size = new System.Drawing.Size(552, 40);
            this._tableLayoutPanel.TabIndex = 14;
            // 
            // _commentPanel
            // 
            this._commentPanel.Controls.Add(this._documentCommentTextBox);
            this._commentPanel.Controls.Add(this.label7);
            this._commentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._commentPanel.Location = new System.Drawing.Point(0, 0);
            this._commentPanel.Margin = new System.Windows.Forms.Padding(0);
            this._commentPanel.Name = "_commentPanel";
            this._commentPanel.Size = new System.Drawing.Size(552, 40);
            this._commentPanel.TabIndex = 1;
            // 
            // _documentCommentTextBox
            // 
            this._documentCommentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._documentCommentTextBox.Location = new System.Drawing.Point(0, 16);
            this._documentCommentTextBox.Name = "_documentCommentTextBox";
            this._documentCommentTextBox.Size = new System.Drawing.Size(551, 20);
            this._documentCommentTextBox.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(-3, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(51, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Comment";
            // 
            // PaginationDocumentDataPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Controls.Add(this._tableLayoutPanel);
            this.Controls.Add(this._patientSexComboBox);
            this.Controls.Add(label6);
            this.Controls.Add(this._documentDateTextBox);
            this.Controls.Add(label5);
            this.Controls.Add(this._patientMRNComboBox);
            this.Controls.Add(label4);
            this.Controls.Add(this._patientMiddleTextBox);
            this.Controls.Add(this._patientLastTextBox);
            this.Controls.Add(this._patientDOBTextBox);
            this.Controls.Add(label3);
            this.Controls.Add(this._patientFirstTextBox);
            this.Controls.Add(label2);
            this.Controls.Add(this._documentTypeComboBox);
            this.Controls.Add(label1);
            this.MinimumSize = new System.Drawing.Size(264, 29);
            this.Name = "PaginationDocumentDataPanel";
            this.Size = new System.Drawing.Size(558, 203);
            this._tableLayoutPanel.ResumeLayout(false);
            this._commentPanel.ResumeLayout(false);
            this._commentPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox _documentTypeComboBox;
        private System.Windows.Forms.TextBox _patientFirstTextBox;
        private System.Windows.Forms.TextBox _patientDOBTextBox;
        private System.Windows.Forms.TextBox _patientLastTextBox;
        private System.Windows.Forms.TextBox _patientMiddleTextBox;
        private System.Windows.Forms.ComboBox _patientMRNComboBox;
        private System.Windows.Forms.TextBox _documentDateTextBox;
        private System.Windows.Forms.ComboBox _patientSexComboBox;
        private System.Windows.Forms.TableLayoutPanel _tableLayoutPanel;
        private System.Windows.Forms.Panel _commentPanel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox _documentCommentTextBox;
    }
}
