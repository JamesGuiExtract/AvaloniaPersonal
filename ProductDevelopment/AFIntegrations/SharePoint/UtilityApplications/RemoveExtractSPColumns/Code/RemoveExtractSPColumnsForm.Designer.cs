namespace RemoveExtractSPColumns
{
    partial class RemoveExtractSPColumnsForm
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
            this.label1 = new System.Windows.Forms.Label();
            this._siteURLTextBox = new System.Windows.Forms.TextBox();
            this._removeSPColumnsButton = new System.Windows.Forms.Button();
            this._redactedFileColumnCheckBox = new System.Windows.Forms.CheckBox();
            this._unredactedColumnCheckBox = new System.Windows.Forms.CheckBox();
            this._idshieldStatusColumn = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._idshieldSensitiveItemCount = new System.Windows.Forms.CheckBox();
            this._idshieldIDSReference = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._DataCaptureStatus = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(165, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Site URL to remove columns from";
            // 
            // _siteURLTextBox
            // 
            this._siteURLTextBox.Location = new System.Drawing.Point(15, 25);
            this._siteURLTextBox.Name = "_siteURLTextBox";
            this._siteURLTextBox.Size = new System.Drawing.Size(636, 20);
            this._siteURLTextBox.TabIndex = 1;
            // 
            // _removeSPColumnsButton
            // 
            this._removeSPColumnsButton.Location = new System.Drawing.Point(9, 271);
            this._removeSPColumnsButton.Name = "_removeSPColumnsButton";
            this._removeSPColumnsButton.Size = new System.Drawing.Size(75, 23);
            this._removeSPColumnsButton.TabIndex = 2;
            this._removeSPColumnsButton.Text = "Remove";
            this._removeSPColumnsButton.UseVisualStyleBackColor = true;
            this._removeSPColumnsButton.Click += new System.EventHandler(this.handleRemoveSPColumnsButton_Click);
            // 
            // _redactedFileColumnCheckBox
            // 
            this._redactedFileColumnCheckBox.AutoSize = true;
            this._redactedFileColumnCheckBox.Checked = true;
            this._redactedFileColumnCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._redactedFileColumnCheckBox.Location = new System.Drawing.Point(23, 27);
            this._redactedFileColumnCheckBox.Name = "_redactedFileColumnCheckBox";
            this._redactedFileColumnCheckBox.Size = new System.Drawing.Size(138, 17);
            this._redactedFileColumnCheckBox.TabIndex = 3;
            this._redactedFileColumnCheckBox.Text = "ID Shield Redacted File";
            this._redactedFileColumnCheckBox.UseVisualStyleBackColor = true;
            // 
            // _unredactedColumnCheckBox
            // 
            this._unredactedColumnCheckBox.AutoSize = true;
            this._unredactedColumnCheckBox.Checked = true;
            this._unredactedColumnCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._unredactedColumnCheckBox.Location = new System.Drawing.Point(23, 50);
            this._unredactedColumnCheckBox.Name = "_unredactedColumnCheckBox";
            this._unredactedColumnCheckBox.Size = new System.Drawing.Size(147, 17);
            this._unredactedColumnCheckBox.TabIndex = 3;
            this._unredactedColumnCheckBox.Text = "ID Shield Unredacted File";
            this._unredactedColumnCheckBox.UseVisualStyleBackColor = true;
            // 
            // _idshieldStatusColumn
            // 
            this._idshieldStatusColumn.AutoSize = true;
            this._idshieldStatusColumn.Checked = true;
            this._idshieldStatusColumn.CheckState = System.Windows.Forms.CheckState.Checked;
            this._idshieldStatusColumn.Location = new System.Drawing.Point(23, 73);
            this._idshieldStatusColumn.Name = "_idshieldStatusColumn";
            this._idshieldStatusColumn.Size = new System.Drawing.Size(102, 17);
            this._idshieldStatusColumn.TabIndex = 3;
            this._idshieldStatusColumn.Text = "ID Shield Status";
            this._idshieldStatusColumn.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._idshieldStatusColumn);
            this.groupBox1.Controls.Add(this._idshieldIDSReference);
            this.groupBox1.Controls.Add(this._idshieldSensitiveItemCount);
            this.groupBox1.Controls.Add(this._unredactedColumnCheckBox);
            this.groupBox1.Controls.Add(this._redactedFileColumnCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(9, 55);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(642, 102);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ID Shield Columns to Remove";
            // 
            // _idshieldSensitiveItemCount
            // 
            this._idshieldSensitiveItemCount.AutoSize = true;
            this._idshieldSensitiveItemCount.Checked = true;
            this._idshieldSensitiveItemCount.CheckState = System.Windows.Forms.CheckState.Checked;
            this._idshieldSensitiveItemCount.Location = new System.Drawing.Point(215, 27);
            this._idshieldSensitiveItemCount.Name = "_idshieldSensitiveItemCount";
            this._idshieldSensitiveItemCount.Size = new System.Drawing.Size(123, 17);
            this._idshieldSensitiveItemCount.TabIndex = 3;
            this._idshieldSensitiveItemCount.Text = "Sensitive Item Count";
            this._idshieldSensitiveItemCount.UseVisualStyleBackColor = true;
            // 
            // _idshieldIDSReference
            // 
            this._idshieldIDSReference.AutoSize = true;
            this._idshieldIDSReference.Checked = true;
            this._idshieldIDSReference.CheckState = System.Windows.Forms.CheckState.Checked;
            this._idshieldIDSReference.Location = new System.Drawing.Point(215, 50);
            this._idshieldIDSReference.Name = "_idshieldIDSReference";
            this._idshieldIDSReference.Size = new System.Drawing.Size(97, 17);
            this._idshieldIDSReference.TabIndex = 3;
            this._idshieldIDSReference.Text = "IDS Reference";
            this._idshieldIDSReference.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this._DataCaptureStatus);
            this.groupBox2.Location = new System.Drawing.Point(9, 163);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(642, 102);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Data Capture Columns to Remove";
            // 
            // _DataCaptureStatus
            // 
            this._DataCaptureStatus.AutoSize = true;
            this._DataCaptureStatus.Checked = true;
            this._DataCaptureStatus.CheckState = System.Windows.Forms.CheckState.Checked;
            this._DataCaptureStatus.Location = new System.Drawing.Point(23, 27);
            this._DataCaptureStatus.Name = "_DataCaptureStatus";
            this._DataCaptureStatus.Size = new System.Drawing.Size(158, 17);
            this._DataCaptureStatus.TabIndex = 3;
            this._DataCaptureStatus.Text = "Extract Data Capture Status";
            this._DataCaptureStatus.UseVisualStyleBackColor = true;
            // 
            // RemoveExtractSPColumnsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(663, 302);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._removeSPColumnsButton);
            this.Controls.Add(this._siteURLTextBox);
            this.Controls.Add(this.label1);
            this.Name = "RemoveExtractSPColumnsForm";
            this.Text = "Remove Extract Sharepoint Columns from Site";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _siteURLTextBox;
        private System.Windows.Forms.Button _removeSPColumnsButton;
        private System.Windows.Forms.CheckBox _redactedFileColumnCheckBox;
        private System.Windows.Forms.CheckBox _unredactedColumnCheckBox;
        private System.Windows.Forms.CheckBox _idshieldStatusColumn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox _idshieldIDSReference;
        private System.Windows.Forms.CheckBox _idshieldSensitiveItemCount;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox _DataCaptureStatus;
    }
}