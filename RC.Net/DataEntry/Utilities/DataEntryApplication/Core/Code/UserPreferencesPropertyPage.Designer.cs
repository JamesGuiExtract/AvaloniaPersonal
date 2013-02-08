namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    partial class UserPreferencesPropertyPage
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
            this._tabControl = new System.Windows.Forms.TabControl();
            this._generalTab = new System.Windows.Forms.TabPage();
            this._ocrTradeOffLabel2 = new System.Windows.Forms.Label();
            this._ocrTradeOffLabel = new System.Windows.Forms.Label();
            this._ocrTradeOffComboBox = new System.Windows.Forms.ComboBox();
            this._autoOcrCheckBox = new System.Windows.Forms.CheckBox();
            this._autoZoomTab = new System.Windows.Forms.TabPage();
            this._mostContextLabel = new System.Windows.Forms.Label();
            this._leastContextLabel = new System.Windows.Forms.Label();
            this._zoomContextTrackBar = new System.Windows.Forms.TrackBar();
            this._zoomContextLabel = new System.Windows.Forms.Label();
            this._autoZoomRadioButton = new System.Windows.Forms.RadioButton();
            this._zoomOutIfNecessaryRadioButton = new System.Windows.Forms.RadioButton();
            this._noZoomRadioButton = new System.Windows.Forms.RadioButton();
            this._autoZoomSettingLabel = new System.Windows.Forms.Label();
            this._tabControl.SuspendLayout();
            this._generalTab.SuspendLayout();
            this._autoZoomTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._zoomContextTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // _tabControl
            // 
            this._tabControl.Controls.Add(this._generalTab);
            this._tabControl.Controls.Add(this._autoZoomTab);
            this._tabControl.Location = new System.Drawing.Point(3, 3);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(446, 195);
            this._tabControl.TabIndex = 1;
            // 
            // _generalTab
            // 
            this._generalTab.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this._generalTab.Controls.Add(this._ocrTradeOffLabel2);
            this._generalTab.Controls.Add(this._ocrTradeOffLabel);
            this._generalTab.Controls.Add(this._ocrTradeOffComboBox);
            this._generalTab.Controls.Add(this._autoOcrCheckBox);
            this._generalTab.Location = new System.Drawing.Point(4, 22);
            this._generalTab.Name = "_generalTab";
            this._generalTab.Padding = new System.Windows.Forms.Padding(3);
            this._generalTab.Size = new System.Drawing.Size(438, 169);
            this._generalTab.TabIndex = 1;
            this._generalTab.Text = "General";
            // 
            // _ocrTradeOffLabel2
            // 
            this._ocrTradeOffLabel2.AutoSize = true;
            this._ocrTradeOffLabel2.Location = new System.Drawing.Point(259, 38);
            this._ocrTradeOffLabel2.Name = "_ocrTradeOffLabel2";
            this._ocrTradeOffLabel2.Size = new System.Drawing.Size(42, 13);
            this._ocrTradeOffLabel2.TabIndex = 17;
            this._ocrTradeOffLabel2.Text = "method";
            // 
            // _ocrTradeOffLabel
            // 
            this._ocrTradeOffLabel.AutoSize = true;
            this._ocrTradeOffLabel.Location = new System.Drawing.Point(48, 38);
            this._ocrTradeOffLabel.Name = "_ocrTradeOffLabel";
            this._ocrTradeOffLabel.Size = new System.Drawing.Size(78, 13);
            this._ocrTradeOffLabel.TabIndex = 16;
            this._ocrTradeOffLabel.Text = "OCR text using";
            // 
            // _ocrTradeOffComboBox
            // 
            this._ocrTradeOffComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._ocrTradeOffComboBox.Enabled = false;
            this._ocrTradeOffComboBox.FormattingEnabled = true;
            this._ocrTradeOffComboBox.Items.AddRange(new object[] {
            "Accurate",
            "Balanced",
            "Fast"});
            this._ocrTradeOffComboBox.Location = new System.Drawing.Point(132, 35);
            this._ocrTradeOffComboBox.Name = "_ocrTradeOffComboBox";
            this._ocrTradeOffComboBox.Size = new System.Drawing.Size(121, 21);
            this._ocrTradeOffComboBox.TabIndex = 15;
            this._ocrTradeOffComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleOcrTradeOffSelectedIndexChanged);
            // 
            // _autoOcrCheckBox
            // 
            this._autoOcrCheckBox.AutoSize = true;
            this._autoOcrCheckBox.Location = new System.Drawing.Point(12, 12);
            this._autoOcrCheckBox.Name = "_autoOcrCheckBox";
            this._autoOcrCheckBox.Size = new System.Drawing.Size(306, 17);
            this._autoOcrCheckBox.TabIndex = 14;
            this._autoOcrCheckBox.Text = "OCR text in background if there is no pre-existing OCR data";
            this._autoOcrCheckBox.UseVisualStyleBackColor = false;
            this._autoOcrCheckBox.CheckedChanged += new System.EventHandler(this.HandleAutoOcrCheckChanged);
            // 
            // _autoZoomTab
            // 
            this._autoZoomTab.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this._autoZoomTab.Controls.Add(this._mostContextLabel);
            this._autoZoomTab.Controls.Add(this._leastContextLabel);
            this._autoZoomTab.Controls.Add(this._zoomContextTrackBar);
            this._autoZoomTab.Controls.Add(this._zoomContextLabel);
            this._autoZoomTab.Controls.Add(this._autoZoomRadioButton);
            this._autoZoomTab.Controls.Add(this._zoomOutIfNecessaryRadioButton);
            this._autoZoomTab.Controls.Add(this._noZoomRadioButton);
            this._autoZoomTab.Controls.Add(this._autoZoomSettingLabel);
            this._autoZoomTab.Location = new System.Drawing.Point(4, 22);
            this._autoZoomTab.Name = "_autoZoomTab";
            this._autoZoomTab.Padding = new System.Windows.Forms.Padding(3);
            this._autoZoomTab.Size = new System.Drawing.Size(438, 169);
            this._autoZoomTab.TabIndex = 0;
            this._autoZoomTab.Text = "Auto zoom";
            // 
            // _mostContextLabel
            // 
            this._mostContextLabel.AutoSize = true;
            this._mostContextLabel.Location = new System.Drawing.Point(362, 146);
            this._mostContextLabel.Name = "_mostContextLabel";
            this._mostContextLabel.Size = new System.Drawing.Size(68, 13);
            this._mostContextLabel.TabIndex = 15;
            this._mostContextLabel.Text = "Most context";
            // 
            // _leastContextLabel
            // 
            this._leastContextLabel.AutoSize = true;
            this._leastContextLabel.Location = new System.Drawing.Point(9, 146);
            this._leastContextLabel.Name = "_leastContextLabel";
            this._leastContextLabel.Size = new System.Drawing.Size(71, 13);
            this._leastContextLabel.TabIndex = 14;
            this._leastContextLabel.Text = "Least context";
            // 
            // _zoomContextTrackBar
            // 
            this._zoomContextTrackBar.Location = new System.Drawing.Point(12, 114);
            this._zoomContextTrackBar.Name = "_zoomContextTrackBar";
            this._zoomContextTrackBar.Size = new System.Drawing.Size(418, 45);
            this._zoomContextTrackBar.TabIndex = 13;
            this._zoomContextTrackBar.Value = 5;
            // 
            // _zoomContextLabel
            // 
            this._zoomContextLabel.AutoSize = true;
            this._zoomContextLabel.Location = new System.Drawing.Point(9, 98);
            this._zoomContextLabel.Name = "_zoomContextLabel";
            this._zoomContextLabel.Size = new System.Drawing.Size(421, 13);
            this._zoomContextLabel.TabIndex = 12;
            this._zoomContextLabel.Text = "When changing zoom level, how much context should be shown around selected data?";
            // 
            // _autoZoomRadioButton
            // 
            this._autoZoomRadioButton.AutoSize = true;
            this._autoZoomRadioButton.Location = new System.Drawing.Point(30, 69);
            this._autoZoomRadioButton.Name = "_autoZoomRadioButton";
            this._autoZoomRadioButton.Size = new System.Drawing.Size(299, 17);
            this._autoZoomRadioButton.TabIndex = 11;
            this._autoZoomRadioButton.Text = "Automatically adjust zoom to area around current selection";
            this._autoZoomRadioButton.UseVisualStyleBackColor = false;
            // 
            // _zoomOutIfNecessaryRadioButton
            // 
            this._zoomOutIfNecessaryRadioButton.AutoSize = true;
            this._zoomOutIfNecessaryRadioButton.Location = new System.Drawing.Point(30, 46);
            this._zoomOutIfNecessaryRadioButton.Name = "_zoomOutIfNecessaryRadioButton";
            this._zoomOutIfNecessaryRadioButton.Size = new System.Drawing.Size(233, 17);
            this._zoomOutIfNecessaryRadioButton.TabIndex = 10;
            this._zoomOutIfNecessaryRadioButton.Text = "Zoom out if necessary to fit current selection";
            this._zoomOutIfNecessaryRadioButton.UseVisualStyleBackColor = false;
            // 
            // _noZoomRadioButton
            // 
            this._noZoomRadioButton.AutoSize = true;
            this._noZoomRadioButton.Checked = true;
            this._noZoomRadioButton.Location = new System.Drawing.Point(30, 23);
            this._noZoomRadioButton.Name = "_noZoomRadioButton";
            this._noZoomRadioButton.Size = new System.Drawing.Size(142, 17);
            this._noZoomRadioButton.TabIndex = 9;
            this._noZoomRadioButton.TabStop = true;
            this._noZoomRadioButton.Text = "Don\'t change zoom level";
            this._noZoomRadioButton.UseVisualStyleBackColor = false;
            // 
            // _autoZoomSettingLabel
            // 
            this._autoZoomSettingLabel.AutoSize = true;
            this._autoZoomSettingLabel.Location = new System.Drawing.Point(6, 6);
            this._autoZoomSettingLabel.Name = "_autoZoomSettingLabel";
            this._autoZoomSettingLabel.Size = new System.Drawing.Size(137, 13);
            this._autoZoomSettingLabel.TabIndex = 8;
            this._autoZoomSettingLabel.Text = "Center on selected text and";
            // 
            // UserPreferencesPropertyPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._tabControl);
            this.Name = "UserPreferencesPropertyPage";
            this.Size = new System.Drawing.Size(450, 200);
            this._tabControl.ResumeLayout(false);
            this._generalTab.ResumeLayout(false);
            this._generalTab.PerformLayout();
            this._autoZoomTab.ResumeLayout(false);
            this._autoZoomTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._zoomContextTrackBar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl _tabControl;
        private System.Windows.Forms.TabPage _generalTab;
        private System.Windows.Forms.TabPage _autoZoomTab;
        private System.Windows.Forms.Label _mostContextLabel;
        private System.Windows.Forms.Label _leastContextLabel;
        private System.Windows.Forms.TrackBar _zoomContextTrackBar;
        private System.Windows.Forms.Label _zoomContextLabel;
        private System.Windows.Forms.RadioButton _autoZoomRadioButton;
        private System.Windows.Forms.RadioButton _zoomOutIfNecessaryRadioButton;
        private System.Windows.Forms.RadioButton _noZoomRadioButton;
        private System.Windows.Forms.Label _autoZoomSettingLabel;
        private System.Windows.Forms.Label _ocrTradeOffLabel2;
        private System.Windows.Forms.Label _ocrTradeOffLabel;
        private System.Windows.Forms.ComboBox _ocrTradeOffComboBox;
        private System.Windows.Forms.CheckBox _autoOcrCheckBox;

    }
}
