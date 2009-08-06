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
            this._zoomSettingGroupBox = new System.Windows.Forms.GroupBox();
            this._autoZoomSettingLabel = new System.Windows.Forms.Label();
            this._noZoomRadioButton = new System.Windows.Forms.RadioButton();
            this._zoomOutIfNecessaryRadioButton = new System.Windows.Forms.RadioButton();
            this._autoZoomRadioButton = new System.Windows.Forms.RadioButton();
            this._zoomContextLabel = new System.Windows.Forms.Label();
            this._zoomContextTrackBar = new System.Windows.Forms.TrackBar();
            this._leastContextLabel = new System.Windows.Forms.Label();
            this._mostContextLabel = new System.Windows.Forms.Label();
            this._zoomSettingGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._zoomContextTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // _zoomSettingGroupBox
            // 
            this._zoomSettingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._zoomSettingGroupBox.Controls.Add(this._mostContextLabel);
            this._zoomSettingGroupBox.Controls.Add(this._leastContextLabel);
            this._zoomSettingGroupBox.Controls.Add(this._zoomContextTrackBar);
            this._zoomSettingGroupBox.Controls.Add(this._zoomContextLabel);
            this._zoomSettingGroupBox.Controls.Add(this._autoZoomRadioButton);
            this._zoomSettingGroupBox.Controls.Add(this._zoomOutIfNecessaryRadioButton);
            this._zoomSettingGroupBox.Controls.Add(this._noZoomRadioButton);
            this._zoomSettingGroupBox.Controls.Add(this._autoZoomSettingLabel);
            this._zoomSettingGroupBox.Location = new System.Drawing.Point(4, 4);
            this._zoomSettingGroupBox.Name = "_zoomSettingGroupBox";
            this._zoomSettingGroupBox.Size = new System.Drawing.Size(451, 185);
            this._zoomSettingGroupBox.TabIndex = 0;
            this._zoomSettingGroupBox.TabStop = false;
            this._zoomSettingGroupBox.Text = "Auto zoom";
            // 
            // _autoZoomSettingLabel
            // 
            this._autoZoomSettingLabel.AutoSize = true;
            this._autoZoomSettingLabel.Location = new System.Drawing.Point(7, 20);
            this._autoZoomSettingLabel.Name = "_autoZoomSettingLabel";
            this._autoZoomSettingLabel.Size = new System.Drawing.Size(137, 13);
            this._autoZoomSettingLabel.TabIndex = 0;
            this._autoZoomSettingLabel.Text = "Center on selected text and";
            // 
            // _noZoomRadioButton
            // 
            this._noZoomRadioButton.AutoSize = true;
            this._noZoomRadioButton.Location = new System.Drawing.Point(31, 37);
            this._noZoomRadioButton.Name = "_noZoomRadioButton";
            this._noZoomRadioButton.Size = new System.Drawing.Size(142, 17);
            this._noZoomRadioButton.TabIndex = 1;
            this._noZoomRadioButton.TabStop = true;
            this._noZoomRadioButton.Text = "Don\'t change zoom level";
            this._noZoomRadioButton.UseVisualStyleBackColor = true;
            // 
            // _zoomOutIfNecessaryRadioButton
            // 
            this._zoomOutIfNecessaryRadioButton.AutoSize = true;
            this._zoomOutIfNecessaryRadioButton.Location = new System.Drawing.Point(31, 60);
            this._zoomOutIfNecessaryRadioButton.Name = "_zoomOutIfNecessaryRadioButton";
            this._zoomOutIfNecessaryRadioButton.Size = new System.Drawing.Size(319, 17);
            this._zoomOutIfNecessaryRadioButton.TabIndex = 2;
            this._zoomOutIfNecessaryRadioButton.TabStop = true;
            this._zoomOutIfNecessaryRadioButton.Text = "Zoom out if necessary to fit current selection";
            this._zoomOutIfNecessaryRadioButton.UseVisualStyleBackColor = true;
            // 
            // _autoZoomRadioButton
            // 
            this._autoZoomRadioButton.AutoSize = true;
            this._autoZoomRadioButton.Location = new System.Drawing.Point(31, 83);
            this._autoZoomRadioButton.Name = "_autoZoomRadioButton";
            this._autoZoomRadioButton.Size = new System.Drawing.Size(197, 17);
            this._autoZoomRadioButton.TabIndex = 3;
            this._autoZoomRadioButton.TabStop = true;
            this._autoZoomRadioButton.Text = "Automatically adjust zoom to area around current selection";
            this._autoZoomRadioButton.UseVisualStyleBackColor = true;
            // 
            // _zoomContextLabel
            // 
            this._zoomContextLabel.AutoSize = true;
            this._zoomContextLabel.Location = new System.Drawing.Point(10, 112);
            this._zoomContextLabel.Name = "_zoomContextLabel";
            this._zoomContextLabel.Size = new System.Drawing.Size(421, 13);
            this._zoomContextLabel.TabIndex = 4;
            this._zoomContextLabel.Text = "When changing zoom level, how much context should be shown around selected data?";
            // 
            // _zoomContextTrackBar
            // 
            this._zoomContextTrackBar.Location = new System.Drawing.Point(13, 128);
            this._zoomContextTrackBar.Name = "_zoomContextTrackBar";
            this._zoomContextTrackBar.Size = new System.Drawing.Size(418, 45);
            this._zoomContextTrackBar.TabIndex = 5;
            this._zoomContextTrackBar.Value = 5;
            // 
            // _leastContextLabel
            // 
            this._leastContextLabel.AutoSize = true;
            this._leastContextLabel.Location = new System.Drawing.Point(10, 160);
            this._leastContextLabel.Name = "_leastContextLabel";
            this._leastContextLabel.Size = new System.Drawing.Size(71, 13);
            this._leastContextLabel.TabIndex = 6;
            this._leastContextLabel.Text = "Least context";
            // 
            // _mostContextLabel
            // 
            this._mostContextLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._mostContextLabel.AutoSize = true;
            this._mostContextLabel.Location = new System.Drawing.Point(362, 160);
            this._mostContextLabel.Name = "_mostContextLabel";
            this._mostContextLabel.Size = new System.Drawing.Size(68, 13);
            this._mostContextLabel.TabIndex = 7;
            this._mostContextLabel.Text = "Most context";
            // 
            // UserPreferencesPropertyPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._zoomSettingGroupBox);
            this.Name = "UserPreferencesPropertyPage";
            this.Size = new System.Drawing.Size(458, 192);
            this._zoomSettingGroupBox.ResumeLayout(false);
            this._zoomSettingGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._zoomContextTrackBar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox _zoomSettingGroupBox;
        private System.Windows.Forms.RadioButton _autoZoomRadioButton;
        private System.Windows.Forms.RadioButton _zoomOutIfNecessaryRadioButton;
        private System.Windows.Forms.RadioButton _noZoomRadioButton;
        private System.Windows.Forms.Label _autoZoomSettingLabel;
        private System.Windows.Forms.Label _zoomContextLabel;
        private System.Windows.Forms.TrackBar _zoomContextTrackBar;
        private System.Windows.Forms.Label _mostContextLabel;
        private System.Windows.Forms.Label _leastContextLabel;
    }
}
