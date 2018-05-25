namespace Extract.FileActionManager.Forms
{
    partial class OCRParametersConfigure
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
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._languagesTabPage = new System.Windows.Forms.TabPage();
            this._specifyRecognitionLanguagesCheckBox = new System.Windows.Forms.CheckBox();
            this._recognitionLanguagesGroupBox = new System.Windows.Forms.GroupBox();
            this._singleLanguageDetectionCheckBox = new System.Windows.Forms.CheckBox();
            this._language5ComboBox = new System.Windows.Forms.ComboBox();
            this._language4ComboBox = new System.Windows.Forms.ComboBox();
            this._language3ComboBox = new System.Windows.Forms.ComboBox();
            this._language2ComboBox = new System.Windows.Forms.ComboBox();
            this._language1ComboBox = new System.Windows.Forms.ComboBox();
            this._recognitionOptionsTabPage = new System.Windows.Forms.TabPage();
            this._zoneOrderingCheckBox = new System.Windows.Forms.CheckBox();
            this._limitToBasicLatinCharactersCheckBox = new System.Windows.Forms.CheckBox();
            this._imageOptionsTabPage = new System.Windows.Forms.TabPage();
            this._forceDespeckleGroupBox = new System.Windows.Forms.GroupBox();
            this._autoDespeckleCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._forceDespeckleLevelNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this._forceDespeckleMethodComboBox = new System.Windows.Forms.ComboBox();
            this._forceDespeckleWhenBitonalRadioButton = new System.Windows.Forms.RadioButton();
            this._neverForceDespeckleRadioButton = new System.Windows.Forms.RadioButton();
            this._alwaysForceDespeckleRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._maxYNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this._maxXNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this._tabControl = new System.Windows.Forms.TabControl();
            this._languagesTabPage.SuspendLayout();
            this._recognitionLanguagesGroupBox.SuspendLayout();
            this._recognitionOptionsTabPage.SuspendLayout();
            this._imageOptionsTabPage.SuspendLayout();
            this._forceDespeckleGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._forceDespeckleLevelNumericUpDown)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._maxYNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._maxXNumericUpDown)).BeginInit();
            this._tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.CausesValidation = false;
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(224, 437);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(143, 437);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 1;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _languagesTabPage
            // 
            this._languagesTabPage.Controls.Add(this._specifyRecognitionLanguagesCheckBox);
            this._languagesTabPage.Controls.Add(this._recognitionLanguagesGroupBox);
            this._languagesTabPage.Location = new System.Drawing.Point(4, 22);
            this._languagesTabPage.Name = "_languagesTabPage";
            this._languagesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._languagesTabPage.Size = new System.Drawing.Size(279, 388);
            this._languagesTabPage.TabIndex = 2;
            this._languagesTabPage.Text = "Languages";
            this._languagesTabPage.UseVisualStyleBackColor = true;
            // 
            // _specifyRecognitionLanguagesCheckBox
            // 
            this._specifyRecognitionLanguagesCheckBox.AutoSize = true;
            this._specifyRecognitionLanguagesCheckBox.Location = new System.Drawing.Point(6, 6);
            this._specifyRecognitionLanguagesCheckBox.Name = "_specifyRecognitionLanguagesCheckBox";
            this._specifyRecognitionLanguagesCheckBox.Size = new System.Drawing.Size(168, 17);
            this._specifyRecognitionLanguagesCheckBox.TabIndex = 0;
            this._specifyRecognitionLanguagesCheckBox.Text = "Specify recognition languages";
            this._specifyRecognitionLanguagesCheckBox.UseVisualStyleBackColor = true;
            this._specifyRecognitionLanguagesCheckBox.CheckedChanged += new System.EventHandler(this.Handle_ValueChanged);
            // 
            // _recognitionLanguagesGroupBox
            // 
            this._recognitionLanguagesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._recognitionLanguagesGroupBox.Controls.Add(this._singleLanguageDetectionCheckBox);
            this._recognitionLanguagesGroupBox.Controls.Add(this._language5ComboBox);
            this._recognitionLanguagesGroupBox.Controls.Add(this._language4ComboBox);
            this._recognitionLanguagesGroupBox.Controls.Add(this._language3ComboBox);
            this._recognitionLanguagesGroupBox.Controls.Add(this._language2ComboBox);
            this._recognitionLanguagesGroupBox.Controls.Add(this._language1ComboBox);
            this._recognitionLanguagesGroupBox.Location = new System.Drawing.Point(6, 29);
            this._recognitionLanguagesGroupBox.Name = "_recognitionLanguagesGroupBox";
            this._recognitionLanguagesGroupBox.Size = new System.Drawing.Size(255, 184);
            this._recognitionLanguagesGroupBox.TabIndex = 1;
            this._recognitionLanguagesGroupBox.TabStop = false;
            this._recognitionLanguagesGroupBox.Text = "Recognition languages";
            // 
            // _singleLanguageDetectionCheckBox
            // 
            this._singleLanguageDetectionCheckBox.AutoSize = true;
            this._singleLanguageDetectionCheckBox.Location = new System.Drawing.Point(8, 19);
            this._singleLanguageDetectionCheckBox.Name = "_singleLanguageDetectionCheckBox";
            this._singleLanguageDetectionCheckBox.Size = new System.Drawing.Size(183, 17);
            this._singleLanguageDetectionCheckBox.TabIndex = 0;
            this._singleLanguageDetectionCheckBox.Text = "Enable single language detection";
            this._singleLanguageDetectionCheckBox.UseVisualStyleBackColor = true;
            // 
            // _language5ComboBox
            // 
            this._language5ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._language5ComboBox.FormattingEnabled = true;
            this._language5ComboBox.Location = new System.Drawing.Point(8, 150);
            this._language5ComboBox.Name = "_language5ComboBox";
            this._language5ComboBox.Size = new System.Drawing.Size(241, 21);
            this._language5ComboBox.Sorted = true;
            this._language5ComboBox.TabIndex = 5;
            // 
            // _language4ComboBox
            // 
            this._language4ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._language4ComboBox.FormattingEnabled = true;
            this._language4ComboBox.Location = new System.Drawing.Point(8, 123);
            this._language4ComboBox.Name = "_language4ComboBox";
            this._language4ComboBox.Size = new System.Drawing.Size(241, 21);
            this._language4ComboBox.Sorted = true;
            this._language4ComboBox.TabIndex = 4;
            // 
            // _language3ComboBox
            // 
            this._language3ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._language3ComboBox.FormattingEnabled = true;
            this._language3ComboBox.Location = new System.Drawing.Point(8, 96);
            this._language3ComboBox.Name = "_language3ComboBox";
            this._language3ComboBox.Size = new System.Drawing.Size(241, 21);
            this._language3ComboBox.Sorted = true;
            this._language3ComboBox.TabIndex = 3;
            // 
            // _language2ComboBox
            // 
            this._language2ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._language2ComboBox.FormattingEnabled = true;
            this._language2ComboBox.Location = new System.Drawing.Point(8, 69);
            this._language2ComboBox.Name = "_language2ComboBox";
            this._language2ComboBox.Size = new System.Drawing.Size(241, 21);
            this._language2ComboBox.Sorted = true;
            this._language2ComboBox.TabIndex = 2;
            // 
            // _language1ComboBox
            // 
            this._language1ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._language1ComboBox.FormattingEnabled = true;
            this._language1ComboBox.Location = new System.Drawing.Point(8, 42);
            this._language1ComboBox.Name = "_language1ComboBox";
            this._language1ComboBox.Size = new System.Drawing.Size(241, 21);
            this._language1ComboBox.Sorted = true;
            this._language1ComboBox.TabIndex = 1;
            // 
            // _recognitionOptionsTabPage
            // 
            this._recognitionOptionsTabPage.Controls.Add(this._zoneOrderingCheckBox);
            this._recognitionOptionsTabPage.Controls.Add(this._limitToBasicLatinCharactersCheckBox);
            this._recognitionOptionsTabPage.Location = new System.Drawing.Point(4, 22);
            this._recognitionOptionsTabPage.Name = "_recognitionOptionsTabPage";
            this._recognitionOptionsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._recognitionOptionsTabPage.Size = new System.Drawing.Size(279, 388);
            this._recognitionOptionsTabPage.TabIndex = 1;
            this._recognitionOptionsTabPage.Text = "Recognition options";
            this._recognitionOptionsTabPage.UseVisualStyleBackColor = true;
            // 
            // _zoneOrderingCheckBox
            // 
            this._zoneOrderingCheckBox.AutoSize = true;
            this._zoneOrderingCheckBox.Location = new System.Drawing.Point(6, 6);
            this._zoneOrderingCheckBox.Name = "_zoneOrderingCheckBox";
            this._zoneOrderingCheckBox.Size = new System.Drawing.Size(129, 17);
            this._zoneOrderingCheckBox.TabIndex = 0;
            this._zoneOrderingCheckBox.Text = "Perform zone ordering";
            this._zoneOrderingCheckBox.UseVisualStyleBackColor = true;
            // 
            // _limitToBasicLatinCharactersCheckBox
            // 
            this._limitToBasicLatinCharactersCheckBox.AutoSize = true;
            this._limitToBasicLatinCharactersCheckBox.Location = new System.Drawing.Point(6, 29);
            this._limitToBasicLatinCharactersCheckBox.Name = "_limitToBasicLatinCharactersCheckBox";
            this._limitToBasicLatinCharactersCheckBox.Size = new System.Drawing.Size(166, 17);
            this._limitToBasicLatinCharactersCheckBox.TabIndex = 1;
            this._limitToBasicLatinCharactersCheckBox.Text = "Limit to basic Latin characters";
            this._limitToBasicLatinCharactersCheckBox.UseVisualStyleBackColor = true;
            // 
            // _imageOptionsTabPage
            // 
            this._imageOptionsTabPage.Controls.Add(this._forceDespeckleGroupBox);
            this._imageOptionsTabPage.Controls.Add(this.groupBox1);
            this._imageOptionsTabPage.Location = new System.Drawing.Point(4, 22);
            this._imageOptionsTabPage.Name = "_imageOptionsTabPage";
            this._imageOptionsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._imageOptionsTabPage.Size = new System.Drawing.Size(279, 388);
            this._imageOptionsTabPage.TabIndex = 0;
            this._imageOptionsTabPage.Text = "Image options";
            this._imageOptionsTabPage.UseVisualStyleBackColor = true;
            // 
            // _forceDespeckleGroupBox
            // 
            this._forceDespeckleGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._forceDespeckleGroupBox.Controls.Add(this._autoDespeckleCheckBox);
            this._forceDespeckleGroupBox.Controls.Add(this.label2);
            this._forceDespeckleGroupBox.Controls.Add(this.label1);
            this._forceDespeckleGroupBox.Controls.Add(this._forceDespeckleLevelNumericUpDown);
            this._forceDespeckleGroupBox.Controls.Add(this._forceDespeckleMethodComboBox);
            this._forceDespeckleGroupBox.Controls.Add(this._forceDespeckleWhenBitonalRadioButton);
            this._forceDespeckleGroupBox.Controls.Add(this._neverForceDespeckleRadioButton);
            this._forceDespeckleGroupBox.Controls.Add(this._alwaysForceDespeckleRadioButton);
            this._forceDespeckleGroupBox.Location = new System.Drawing.Point(6, 91);
            this._forceDespeckleGroupBox.Name = "_forceDespeckleGroupBox";
            this._forceDespeckleGroupBox.Size = new System.Drawing.Size(267, 224);
            this._forceDespeckleGroupBox.TabIndex = 1;
            this._forceDespeckleGroupBox.TabStop = false;
            this._forceDespeckleGroupBox.Text = "Despeckle";
            // 
            // _autoDespeckleCheckBox
            // 
            this._autoDespeckleCheckBox.AutoSize = true;
            this._autoDespeckleCheckBox.Checked = true;
            this._autoDespeckleCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._autoDespeckleCheckBox.Location = new System.Drawing.Point(13, 19);
            this._autoDespeckleCheckBox.Name = "_autoDespeckleCheckBox";
            this._autoDespeckleCheckBox.Size = new System.Drawing.Size(133, 17);
            this._autoDespeckleCheckBox.TabIndex = 0;
            this._autoDespeckleCheckBox.Text = "Automatic despeckling";
            this._autoDespeckleCheckBox.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 131);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Force despeckle method";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 184);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Force despeckle level";
            // 
            // _forceDespeckleLevelNumericUpDown
            // 
            this._forceDespeckleLevelNumericUpDown.Location = new System.Drawing.Point(155, 182);
            this._forceDespeckleLevelNumericUpDown.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this._forceDespeckleLevelNumericUpDown.Name = "_forceDespeckleLevelNumericUpDown";
            this._forceDespeckleLevelNumericUpDown.Size = new System.Drawing.Size(54, 20);
            this._forceDespeckleLevelNumericUpDown.TabIndex = 7;
            // 
            // _forceDespeckleMethodComboBox
            // 
            this._forceDespeckleMethodComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._forceDespeckleMethodComboBox.FormattingEnabled = true;
            this._forceDespeckleMethodComboBox.Location = new System.Drawing.Point(13, 153);
            this._forceDespeckleMethodComboBox.Name = "_forceDespeckleMethodComboBox";
            this._forceDespeckleMethodComboBox.Size = new System.Drawing.Size(196, 21);
            this._forceDespeckleMethodComboBox.TabIndex = 5;
            this._forceDespeckleMethodComboBox.SelectedIndexChanged += new System.EventHandler(this.Handle_ValueChanged);
            // 
            // _forceDespeckleWhenBitonalRadioButton
            // 
            this._forceDespeckleWhenBitonalRadioButton.AutoSize = true;
            this._forceDespeckleWhenBitonalRadioButton.Location = new System.Drawing.Point(13, 75);
            this._forceDespeckleWhenBitonalRadioButton.Name = "_forceDespeckleWhenBitonalRadioButton";
            this._forceDespeckleWhenBitonalRadioButton.Size = new System.Drawing.Size(156, 17);
            this._forceDespeckleWhenBitonalRadioButton.TabIndex = 2;
            this._forceDespeckleWhenBitonalRadioButton.Text = "Force when image is bitonal";
            this._forceDespeckleWhenBitonalRadioButton.UseVisualStyleBackColor = true;
            this._forceDespeckleWhenBitonalRadioButton.CheckedChanged += new System.EventHandler(this.Handle_ValueChanged);
            // 
            // _neverForceDespeckleRadioButton
            // 
            this._neverForceDespeckleRadioButton.AutoSize = true;
            this._neverForceDespeckleRadioButton.Checked = true;
            this._neverForceDespeckleRadioButton.Location = new System.Drawing.Point(13, 52);
            this._neverForceDespeckleRadioButton.Name = "_neverForceDespeckleRadioButton";
            this._neverForceDespeckleRadioButton.Size = new System.Drawing.Size(81, 17);
            this._neverForceDespeckleRadioButton.TabIndex = 1;
            this._neverForceDespeckleRadioButton.TabStop = true;
            this._neverForceDespeckleRadioButton.Text = "Never force";
            this._neverForceDespeckleRadioButton.UseVisualStyleBackColor = true;
            this._neverForceDespeckleRadioButton.CheckedChanged += new System.EventHandler(this.Handle_ValueChanged);
            // 
            // _alwaysForceDespeckleRadioButton
            // 
            this._alwaysForceDespeckleRadioButton.AutoSize = true;
            this._alwaysForceDespeckleRadioButton.Location = new System.Drawing.Point(13, 98);
            this._alwaysForceDespeckleRadioButton.Name = "_alwaysForceDespeckleRadioButton";
            this._alwaysForceDespeckleRadioButton.Size = new System.Drawing.Size(223, 17);
            this._alwaysForceDespeckleRadioButton.TabIndex = 3;
            this._alwaysForceDespeckleRadioButton.Text = "Always force (convert to bitonal if needed)";
            this._alwaysForceDespeckleRadioButton.UseVisualStyleBackColor = true;
            this._alwaysForceDespeckleRadioButton.CheckedChanged += new System.EventHandler(this.Handle_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this._maxYNumericUpDown);
            this.groupBox1.Controls.Add(this._maxXNumericUpDown);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(267, 79);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Maximum image size";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(90, 51);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(33, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "pixels";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(90, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(33, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "pixels";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 49);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Y";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "X";
            // 
            // _maxYNumericUpDown
            // 
            this._maxYNumericUpDown.Location = new System.Drawing.Point(30, 47);
            this._maxYNumericUpDown.Maximum = new decimal(new int[] {
            32000,
            0,
            0,
            0});
            this._maxYNumericUpDown.Name = "_maxYNumericUpDown";
            this._maxYNumericUpDown.Size = new System.Drawing.Size(54, 20);
            this._maxYNumericUpDown.TabIndex = 4;
            this._maxYNumericUpDown.Value = new decimal(new int[] {
            32000,
            0,
            0,
            0});
            // 
            // _maxXNumericUpDown
            // 
            this._maxXNumericUpDown.Location = new System.Drawing.Point(30, 21);
            this._maxXNumericUpDown.Maximum = new decimal(new int[] {
            32000,
            0,
            0,
            0});
            this._maxXNumericUpDown.Name = "_maxXNumericUpDown";
            this._maxXNumericUpDown.Size = new System.Drawing.Size(54, 20);
            this._maxXNumericUpDown.TabIndex = 1;
            this._maxXNumericUpDown.Value = new decimal(new int[] {
            32000,
            0,
            0,
            0});
            // 
            // _tabControl
            // 
            this._tabControl.Controls.Add(this._imageOptionsTabPage);
            this._tabControl.Controls.Add(this._recognitionOptionsTabPage);
            this._tabControl.Controls.Add(this._languagesTabPage);
            this._tabControl.Location = new System.Drawing.Point(12, 12);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(287, 414);
            this._tabControl.TabIndex = 0;
            // 
            // OCRParametersConfigure
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(311, 472);
            this.Controls.Add(this._tabControl);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OCRParametersConfigure";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure OCR properties";
            this._languagesTabPage.ResumeLayout(false);
            this._languagesTabPage.PerformLayout();
            this._recognitionLanguagesGroupBox.ResumeLayout(false);
            this._recognitionLanguagesGroupBox.PerformLayout();
            this._recognitionOptionsTabPage.ResumeLayout(false);
            this._recognitionOptionsTabPage.PerformLayout();
            this._imageOptionsTabPage.ResumeLayout(false);
            this._forceDespeckleGroupBox.ResumeLayout(false);
            this._forceDespeckleGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._forceDespeckleLevelNumericUpDown)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._maxYNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._maxXNumericUpDown)).EndInit();
            this._tabControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TabPage _languagesTabPage;
        private System.Windows.Forms.CheckBox _specifyRecognitionLanguagesCheckBox;
        private System.Windows.Forms.GroupBox _recognitionLanguagesGroupBox;
        private System.Windows.Forms.CheckBox _singleLanguageDetectionCheckBox;
        private System.Windows.Forms.ComboBox _language5ComboBox;
        private System.Windows.Forms.ComboBox _language4ComboBox;
        private System.Windows.Forms.ComboBox _language3ComboBox;
        private System.Windows.Forms.ComboBox _language2ComboBox;
        private System.Windows.Forms.ComboBox _language1ComboBox;
        private System.Windows.Forms.TabPage _recognitionOptionsTabPage;
        private System.Windows.Forms.CheckBox _zoneOrderingCheckBox;
        private System.Windows.Forms.CheckBox _limitToBasicLatinCharactersCheckBox;
        private System.Windows.Forms.TabPage _imageOptionsTabPage;
        private System.Windows.Forms.GroupBox _forceDespeckleGroupBox;
        private System.Windows.Forms.CheckBox _autoDespeckleCheckBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown _forceDespeckleLevelNumericUpDown;
        private System.Windows.Forms.ComboBox _forceDespeckleMethodComboBox;
        private System.Windows.Forms.RadioButton _forceDespeckleWhenBitonalRadioButton;
        private System.Windows.Forms.RadioButton _neverForceDespeckleRadioButton;
        private System.Windows.Forms.RadioButton _alwaysForceDespeckleRadioButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown _maxYNumericUpDown;
        private System.Windows.Forms.NumericUpDown _maxXNumericUpDown;
        private System.Windows.Forms.TabControl _tabControl;
    }
}