namespace Extract.UtilityApplications.PaginationUtility
{
    partial class PaginationSettingsDialog
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label5;
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._inputFolderTextBox = new System.Windows.Forms.TextBox();
            this._fileFilterComboBox = new System.Windows.Forms.ComboBox();
            this._includeSubfoldersCheckBox = new System.Windows.Forms.CheckBox();
            this._processedDocumentFolderTextBox = new System.Windows.Forms.TextBox();
            this._outputFolderTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._deleteInputDocumentRadioButton = new System.Windows.Forms.RadioButton();
            this._moveInputDocumentRadioButton = new System.Windows.Forms.RadioButton();
            this._processedDocumentFolderBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._inputPageCountUpDown = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._outputFolderBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._inputFolderBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._preserveOutputSubFoldersCheckBox = new System.Windows.Forms.CheckBox();
            this._exportSettingsButton = new System.Windows.Forms.Button();
            this._importSettingsButton = new System.Windows.Forms.Button();
            this._randomizeOutputFileNameCheckBox = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._inputPageCountUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(9, 13);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(111, 13);
            label1.TabIndex = 0;
            label1.Text = "Specify input directory";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(9, 58);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(125, 13);
            label2.TabIndex = 3;
            label2.Text = "Limit input to files of type:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(9, 131);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(118, 13);
            label4.TabIndex = 9;
            label4.Text = "Specify output directory";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(9, 107);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(69, 13);
            label3.TabIndex = 6;
            label3.Text = "Keep at least";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(136, 107);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(177, 13);
            label5.TabIndex = 8;
            label5.Text = "pages in the input pane (if available)";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(337, 322);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 17;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(418, 322);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 18;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _inputFolderTextBox
            // 
            this._inputFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._inputFolderTextBox.Location = new System.Drawing.Point(12, 29);
            this._inputFolderTextBox.Name = "_inputFolderTextBox";
            this._inputFolderTextBox.Size = new System.Drawing.Size(448, 20);
            this._inputFolderTextBox.TabIndex = 1;
            // 
            // _fileFilterComboBox
            // 
            this._fileFilterComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._fileFilterComboBox.FormattingEnabled = true;
            this._fileFilterComboBox.Items.AddRange(new object[] {
            "*.tif;*.tiff;*.pdf",
            "*.pdf",
            "*.tif;*.tiff",
            "*.*"});
            this._fileFilterComboBox.Location = new System.Drawing.Point(140, 55);
            this._fileFilterComboBox.Name = "_fileFilterComboBox";
            this._fileFilterComboBox.Size = new System.Drawing.Size(320, 21);
            this._fileFilterComboBox.TabIndex = 4;
            // 
            // _includeSubfoldersCheckBox
            // 
            this._includeSubfoldersCheckBox.AutoSize = true;
            this._includeSubfoldersCheckBox.Location = new System.Drawing.Point(12, 82);
            this._includeSubfoldersCheckBox.Name = "_includeSubfoldersCheckBox";
            this._includeSubfoldersCheckBox.Size = new System.Drawing.Size(189, 17);
            this._includeSubfoldersCheckBox.TabIndex = 5;
            this._includeSubfoldersCheckBox.Text = "Include files from all sub-directories";
            this._includeSubfoldersCheckBox.UseVisualStyleBackColor = true;
            this._includeSubfoldersCheckBox.CheckedChanged += new System.EventHandler(this.HandleIncludeSubfoldersCheckBox_CheckedChanged);
            // 
            // _processedDocumentFolderTextBox
            // 
            this._processedDocumentFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._processedDocumentFolderTextBox.Location = new System.Drawing.Point(6, 42);
            this._processedDocumentFolderTextBox.Name = "_processedDocumentFolderTextBox";
            this._processedDocumentFolderTextBox.Size = new System.Drawing.Size(433, 20);
            this._processedDocumentFolderTextBox.TabIndex = 1;
            // 
            // _outputFolderTextBox
            // 
            this._outputFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputFolderTextBox.Location = new System.Drawing.Point(12, 147);
            this._outputFolderTextBox.Name = "_outputFolderTextBox";
            this._outputFolderTextBox.Size = new System.Drawing.Size(448, 20);
            this._outputFolderTextBox.TabIndex = 10;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._deleteInputDocumentRadioButton);
            this.groupBox1.Controls.Add(this._moveInputDocumentRadioButton);
            this.groupBox1.Controls.Add(this._processedDocumentFolderTextBox);
            this.groupBox1.Controls.Add(this._processedDocumentFolderBrowseButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 219);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(481, 94);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "After handling all pages of an input document";
            // 
            // _deleteInputDocumentRadioButton
            // 
            this._deleteInputDocumentRadioButton.AutoSize = true;
            this._deleteInputDocumentRadioButton.Location = new System.Drawing.Point(6, 68);
            this._deleteInputDocumentRadioButton.Name = "_deleteInputDocumentRadioButton";
            this._deleteInputDocumentRadioButton.Size = new System.Drawing.Size(124, 17);
            this._deleteInputDocumentRadioButton.TabIndex = 3;
            this._deleteInputDocumentRadioButton.TabStop = true;
            this._deleteInputDocumentRadioButton.Text = "Delete the document";
            this._deleteInputDocumentRadioButton.UseVisualStyleBackColor = true;
            // 
            // _moveInputDocumentRadioButton
            // 
            this._moveInputDocumentRadioButton.AutoSize = true;
            this._moveInputDocumentRadioButton.Location = new System.Drawing.Point(7, 19);
            this._moveInputDocumentRadioButton.Name = "_moveInputDocumentRadioButton";
            this._moveInputDocumentRadioButton.Size = new System.Drawing.Size(196, 17);
            this._moveInputDocumentRadioButton.TabIndex = 0;
            this._moveInputDocumentRadioButton.TabStop = true;
            this._moveInputDocumentRadioButton.Text = "Move the document to the directory:";
            this._moveInputDocumentRadioButton.UseVisualStyleBackColor = true;
            this._moveInputDocumentRadioButton.CheckedChanged += new System.EventHandler(this.HandleMoveInputDocumentRadioButton_CheckedChanged);
            // 
            // _processedDocumentFolderBrowseButton
            // 
            this._processedDocumentFolderBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._processedDocumentFolderBrowseButton.FolderBrowser = true;
            this._processedDocumentFolderBrowseButton.Location = new System.Drawing.Point(445, 41);
            this._processedDocumentFolderBrowseButton.Name = "_processedDocumentFolderBrowseButton";
            this._processedDocumentFolderBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._processedDocumentFolderBrowseButton.TabIndex = 2;
            this._processedDocumentFolderBrowseButton.Text = "...";
            this._processedDocumentFolderBrowseButton.TextControl = this._processedDocumentFolderTextBox;
            this._processedDocumentFolderBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _inputPageCountUpDown
            // 
            this._inputPageCountUpDown.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this._inputPageCountUpDown.IntegersOnly = true;
            this._inputPageCountUpDown.Location = new System.Drawing.Point(84, 105);
            this._inputPageCountUpDown.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this._inputPageCountUpDown.Name = "_inputPageCountUpDown";
            this._inputPageCountUpDown.Size = new System.Drawing.Size(46, 20);
            this._inputPageCountUpDown.TabIndex = 7;
            this._inputPageCountUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // _outputFolderBrowseButton
            // 
            this._outputFolderBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputFolderBrowseButton.FolderBrowser = true;
            this._outputFolderBrowseButton.Location = new System.Drawing.Point(466, 147);
            this._outputFolderBrowseButton.Name = "_outputFolderBrowseButton";
            this._outputFolderBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._outputFolderBrowseButton.TabIndex = 11;
            this._outputFolderBrowseButton.Text = "...";
            this._outputFolderBrowseButton.TextControl = this._outputFolderTextBox;
            this._outputFolderBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _inputFolderBrowseButton
            // 
            this._inputFolderBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._inputFolderBrowseButton.FolderBrowser = true;
            this._inputFolderBrowseButton.Location = new System.Drawing.Point(466, 28);
            this._inputFolderBrowseButton.Name = "_inputFolderBrowseButton";
            this._inputFolderBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._inputFolderBrowseButton.TabIndex = 2;
            this._inputFolderBrowseButton.Text = "...";
            this._inputFolderBrowseButton.TextControl = this._inputFolderTextBox;
            this._inputFolderBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _preserveOutputSubFoldersCheckBox
            // 
            this._preserveOutputSubFoldersCheckBox.AutoSize = true;
            this._preserveOutputSubFoldersCheckBox.Location = new System.Drawing.Point(12, 196);
            this._preserveOutputSubFoldersCheckBox.Name = "_preserveOutputSubFoldersCheckBox";
            this._preserveOutputSubFoldersCheckBox.Size = new System.Drawing.Size(248, 17);
            this._preserveOutputSubFoldersCheckBox.TabIndex = 13;
            this._preserveOutputSubFoldersCheckBox.Text = "Preserve sub-folder structure in output directory";
            this._preserveOutputSubFoldersCheckBox.UseVisualStyleBackColor = true;
            // 
            // _exportSettingsButton
            // 
            this._exportSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._exportSettingsButton.Location = new System.Drawing.Point(12, 322);
            this._exportSettingsButton.Name = "_exportSettingsButton";
            this._exportSettingsButton.Size = new System.Drawing.Size(108, 23);
            this._exportSettingsButton.TabIndex = 15;
            this._exportSettingsButton.Text = "Export Settings...";
            this._exportSettingsButton.UseVisualStyleBackColor = true;
            this._exportSettingsButton.Click += new System.EventHandler(this.HandleExportSettingsButton_Click);
            // 
            // _importSettingsButton
            // 
            this._importSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._importSettingsButton.Location = new System.Drawing.Point(126, 322);
            this._importSettingsButton.Name = "_importSettingsButton";
            this._importSettingsButton.Size = new System.Drawing.Size(108, 23);
            this._importSettingsButton.TabIndex = 16;
            this._importSettingsButton.Text = "Import Settings...";
            this._importSettingsButton.UseVisualStyleBackColor = true;
            this._importSettingsButton.Click += new System.EventHandler(this.HandleImportSettingsButton_Click);
            // 
            // _randomizeOutputFileNameCheckBox
            // 
            this._randomizeOutputFileNameCheckBox.AutoSize = true;
            this._randomizeOutputFileNameCheckBox.Location = new System.Drawing.Point(12, 173);
            this._randomizeOutputFileNameCheckBox.Name = "_randomizeOutputFileNameCheckBox";
            this._randomizeOutputFileNameCheckBox.Size = new System.Drawing.Size(154, 17);
            this._randomizeOutputFileNameCheckBox.TabIndex = 12;
            this._randomizeOutputFileNameCheckBox.Text = "Randomize output filename";
            this._randomizeOutputFileNameCheckBox.UseVisualStyleBackColor = true;
            // 
            // PaginationSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(505, 355);
            this.Controls.Add(this._randomizeOutputFileNameCheckBox);
            this.Controls.Add(this._importSettingsButton);
            this.Controls.Add(this._exportSettingsButton);
            this.Controls.Add(this._preserveOutputSubFoldersCheckBox);
            this.Controls.Add(this._inputPageCountUpDown);
            this.Controls.Add(label5);
            this.Controls.Add(label3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(label4);
            this.Controls.Add(this._outputFolderBrowseButton);
            this.Controls.Add(this._outputFolderTextBox);
            this.Controls.Add(this._includeSubfoldersCheckBox);
            this.Controls.Add(label2);
            this.Controls.Add(this._fileFilterComboBox);
            this.Controls.Add(this._inputFolderBrowseButton);
            this.Controls.Add(this._inputFolderTextBox);
            this.Controls.Add(label1);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PaginationSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pagination Utility Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._inputPageCountUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private Utilities.Forms.BrowseButton _inputFolderBrowseButton;
        private System.Windows.Forms.TextBox _inputFolderTextBox;
        private System.Windows.Forms.ComboBox _fileFilterComboBox;
        private System.Windows.Forms.CheckBox _includeSubfoldersCheckBox;
        private Utilities.Forms.BrowseButton _processedDocumentFolderBrowseButton;
        private System.Windows.Forms.TextBox _processedDocumentFolderTextBox;
        private Utilities.Forms.BrowseButton _outputFolderBrowseButton;
        private System.Windows.Forms.TextBox _outputFolderTextBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton _moveInputDocumentRadioButton;
        private System.Windows.Forms.RadioButton _deleteInputDocumentRadioButton;
        private Utilities.Forms.BetterNumericUpDown _inputPageCountUpDown;
        private System.Windows.Forms.CheckBox _preserveOutputSubFoldersCheckBox;
        private System.Windows.Forms.Button _exportSettingsButton;
        private System.Windows.Forms.Button _importSettingsButton;
        private System.Windows.Forms.CheckBox _randomizeOutputFileNameCheckBox;
    }
}