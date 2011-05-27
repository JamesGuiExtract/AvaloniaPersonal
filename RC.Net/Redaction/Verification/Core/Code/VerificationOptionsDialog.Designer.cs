namespace Extract.Redaction.Verification
{
    partial class VerificationOptionsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="VerificationOptionsDialog"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="VerificationOptionsDialog"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources
                if (components != null)
                {
                    components.Dispose();
                }
            }

            // Release unmanaged resources

            // Call base dispose method
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label3;
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.TextBox textBox1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VerificationOptionsDialog));
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label1;
            this._slideshowRunKeyComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this._autoZoomScaleTrackBar = new System.Windows.Forms.TrackBar();
            this._autoZoomScaleTextBox = new System.Windows.Forms.TextBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._tabControl = new System.Windows.Forms.TabControl();
            this._generalTabPage = new System.Windows.Forms.TabPage();
            this._OCRCheckBox = new System.Windows.Forms.CheckBox();
            this._autoToolComboBox = new System.Windows.Forms.ComboBox();
            this._autoToolCheckBox = new System.Windows.Forms.CheckBox();
            this._slideshowTabPage = new System.Windows.Forms.TabPage();
            this._slideshowAutoStartCheckBox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this._slideshowIntervalUpDown = new Extract.Utilities.Forms.BetterNumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this._zoomTabPage = new System.Windows.Forms.TabPage();
            this._autoZoomCheckBox = new System.Windows.Forms.CheckBox();
            this._onDemandTabPage = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._doNotCreateVOAFileRadioButton = new System.Windows.Forms.RadioButton();
            this._promptVOAFileRadioButton = new System.Windows.Forms.RadioButton();
            this._createVOAFileRadioButton = new System.Windows.Forms.RadioButton();
            label3 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            textBox1 = new System.Windows.Forms.TextBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._autoZoomScaleTrackBar)).BeginInit();
            this._tabControl.SuspendLayout();
            this._generalTabPage.SuspendLayout();
            this._slideshowTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._slideshowIntervalUpDown)).BeginInit();
            this._zoomTabPage.SuspendLayout();
            this._onDemandTabPage.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(188, 16);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(90, 13);
            label3.TabIndex = 9;
            label3.Text = "tool after highlight";
            // 
            // groupBox2
            // 
            groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox2.Controls.Add(this._slideshowRunKeyComboBox);
            groupBox2.Controls.Add(this.label6);
            groupBox2.Controls.Add(textBox1);
            groupBox2.Location = new System.Drawing.Point(6, 60);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(376, 102);
            groupBox2.TabIndex = 23;
            groupBox2.TabStop = false;
            groupBox2.Text = "Run key";
            // 
            // _slideshowRunKeyComboBox
            // 
            this._slideshowRunKeyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._slideshowRunKeyComboBox.FormattingEnabled = true;
            this._slideshowRunKeyComboBox.Location = new System.Drawing.Point(110, 68);
            this._slideshowRunKeyComboBox.Name = "_slideshowRunKeyComboBox";
            this._slideshowRunKeyComboBox.Size = new System.Drawing.Size(253, 21);
            this._slideshowRunKeyComboBox.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 71);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(96, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Slideshow run key:";
            // 
            // textBox1
            // 
            textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            textBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            textBox1.Location = new System.Drawing.Point(6, 19);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(357, 45);
            textBox1.TabIndex = 0;
            textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(this._autoZoomScaleTrackBar);
            groupBox1.Controls.Add(this._autoZoomScaleTextBox);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new System.Drawing.Point(17, 38);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(356, 83);
            groupBox1.TabIndex = 8;
            groupBox1.TabStop = false;
            groupBox1.Text = "Zoom level";
            // 
            // _autoZoomScaleTrackBar
            // 
            this._autoZoomScaleTrackBar.Enabled = false;
            this._autoZoomScaleTrackBar.LargeChange = 3;
            this._autoZoomScaleTrackBar.Location = new System.Drawing.Point(10, 36);
            this._autoZoomScaleTrackBar.Minimum = 1;
            this._autoZoomScaleTrackBar.Name = "_autoZoomScaleTrackBar";
            this._autoZoomScaleTrackBar.Size = new System.Drawing.Size(308, 45);
            this._autoZoomScaleTrackBar.TabIndex = 2;
            this._autoZoomScaleTrackBar.Value = 1;
            this._autoZoomScaleTrackBar.ValueChanged += new System.EventHandler(this.HandleAutoZoomScaleTrackBarValueChanged);
            // 
            // _autoZoomScaleTextBox
            // 
            this._autoZoomScaleTextBox.Enabled = false;
            this._autoZoomScaleTextBox.Location = new System.Drawing.Point(324, 36);
            this._autoZoomScaleTextBox.Name = "_autoZoomScaleTextBox";
            this._autoZoomScaleTextBox.ReadOnly = true;
            this._autoZoomScaleTextBox.Size = new System.Drawing.Size(20, 20);
            this._autoZoomScaleTextBox.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(266, 20);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(52, 13);
            label2.TabIndex = 1;
            label2.Text = "Zoom out";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(7, 20);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(45, 13);
            label1.TabIndex = 0;
            label1.Text = "Zoom in";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(333, 216);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 6;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(252, 216);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 5;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _tabControl
            // 
            this._tabControl.Controls.Add(this._generalTabPage);
            this._tabControl.Controls.Add(this._slideshowTabPage);
            this._tabControl.Controls.Add(this._zoomTabPage);
            this._tabControl.Controls.Add(this._onDemandTabPage);
            this._tabControl.Location = new System.Drawing.Point(13, 13);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(396, 194);
            this._tabControl.TabIndex = 7;
            // 
            // _generalTabPage
            // 
            this._generalTabPage.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this._generalTabPage.Controls.Add(this._OCRCheckBox);
            this._generalTabPage.Controls.Add(this._autoToolComboBox);
            this._generalTabPage.Controls.Add(this._autoToolCheckBox);
            this._generalTabPage.Controls.Add(label3);
            this._generalTabPage.Location = new System.Drawing.Point(4, 22);
            this._generalTabPage.Name = "_generalTabPage";
            this._generalTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._generalTabPage.Size = new System.Drawing.Size(388, 168);
            this._generalTabPage.TabIndex = 0;
            this._generalTabPage.Text = "General";
            // 
            // _OCRCheckBox
            // 
            this._OCRCheckBox.AutoSize = true;
            this._OCRCheckBox.Location = new System.Drawing.Point(17, 38);
            this._OCRCheckBox.Name = "_OCRCheckBox";
            this._OCRCheckBox.Size = new System.Drawing.Size(235, 17);
            this._OCRCheckBox.TabIndex = 10;
            this._OCRCheckBox.Text = "OCR text if there is no pre-existing OCR data";
            this._OCRCheckBox.UseVisualStyleBackColor = true;
            // 
            // _autoToolComboBox
            // 
            this._autoToolComboBox.DisplayMember = "selection";
            this._autoToolComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._autoToolComboBox.Enabled = false;
            this._autoToolComboBox.FormattingEnabled = true;
            this._autoToolComboBox.Location = new System.Drawing.Point(82, 13);
            this._autoToolComboBox.Name = "_autoToolComboBox";
            this._autoToolComboBox.Size = new System.Drawing.Size(100, 21);
            this._autoToolComboBox.TabIndex = 8;
            // 
            // _autoToolCheckBox
            // 
            this._autoToolCheckBox.AutoSize = true;
            this._autoToolCheckBox.Location = new System.Drawing.Point(17, 15);
            this._autoToolCheckBox.Name = "_autoToolCheckBox";
            this._autoToolCheckBox.Size = new System.Drawing.Size(59, 17);
            this._autoToolCheckBox.TabIndex = 7;
            this._autoToolCheckBox.Text = "Enable";
            this._autoToolCheckBox.UseVisualStyleBackColor = true;
            this._autoToolCheckBox.CheckedChanged += new System.EventHandler(this.HandleAutoToolCheckBoxCheckedChanged);
            // 
            // _slideshowTabPage
            // 
            this._slideshowTabPage.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this._slideshowTabPage.Controls.Add(groupBox2);
            this._slideshowTabPage.Controls.Add(this._slideshowAutoStartCheckBox);
            this._slideshowTabPage.Controls.Add(this.label4);
            this._slideshowTabPage.Controls.Add(this._slideshowIntervalUpDown);
            this._slideshowTabPage.Controls.Add(this.label5);
            this._slideshowTabPage.Location = new System.Drawing.Point(4, 22);
            this._slideshowTabPage.Name = "_slideshowTabPage";
            this._slideshowTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._slideshowTabPage.Size = new System.Drawing.Size(388, 168);
            this._slideshowTabPage.TabIndex = 1;
            this._slideshowTabPage.Text = "Slideshow";
            // 
            // _slideshowAutoStartCheckBox
            // 
            this._slideshowAutoStartCheckBox.AutoSize = true;
            this._slideshowAutoStartCheckBox.Checked = true;
            this._slideshowAutoStartCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._slideshowAutoStartCheckBox.Location = new System.Drawing.Point(6, 37);
            this._slideshowAutoStartCheckBox.Name = "_slideshowAutoStartCheckBox";
            this._slideshowAutoStartCheckBox.Size = new System.Drawing.Size(309, 17);
            this._slideshowAutoStartCheckBox.TabIndex = 22;
            this._slideshowAutoStartCheckBox.Text = "Automatically start slideshow when verification session starts";
            this._slideshowAutoStartCheckBox.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(336, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "seconds.";
            // 
            // _slideshowIntervalUpDown
            // 
            this._slideshowIntervalUpDown.IntegersOnly = true;
            this._slideshowIntervalUpDown.Location = new System.Drawing.Point(289, 11);
            this._slideshowIntervalUpDown.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this._slideshowIntervalUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._slideshowIntervalUpDown.Name = "_slideshowIntervalUpDown";
            this._slideshowIntervalUpDown.Size = new System.Drawing.Size(41, 20);
            this._slideshowIntervalUpDown.TabIndex = 20;
            this._slideshowIntervalUpDown.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(280, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "Automatically advance to the next page or document after";
            // 
            // _zoomTabPage
            // 
            this._zoomTabPage.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this._zoomTabPage.Controls.Add(this._autoZoomCheckBox);
            this._zoomTabPage.Controls.Add(groupBox1);
            this._zoomTabPage.Location = new System.Drawing.Point(4, 22);
            this._zoomTabPage.Name = "_zoomTabPage";
            this._zoomTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._zoomTabPage.Size = new System.Drawing.Size(388, 168);
            this._zoomTabPage.TabIndex = 2;
            this._zoomTabPage.Text = "Zoom";
            // 
            // _autoZoomCheckBox
            // 
            this._autoZoomCheckBox.AutoSize = true;
            this._autoZoomCheckBox.Location = new System.Drawing.Point(17, 15);
            this._autoZoomCheckBox.Name = "_autoZoomCheckBox";
            this._autoZoomCheckBox.Size = new System.Drawing.Size(205, 17);
            this._autoZoomCheckBox.TabIndex = 7;
            this._autoZoomCheckBox.Text = "Automatically zoom to redactable data";
            this._autoZoomCheckBox.UseVisualStyleBackColor = true;
            this._autoZoomCheckBox.CheckedChanged += new System.EventHandler(this.HandleAutoZoomCheckBoxCheckedChanged);
            // 
            // _onDemandTabPage
            // 
            this._onDemandTabPage.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this._onDemandTabPage.Controls.Add(this.groupBox3);
            this._onDemandTabPage.Location = new System.Drawing.Point(4, 22);
            this._onDemandTabPage.Name = "_onDemandTabPage";
            this._onDemandTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._onDemandTabPage.Size = new System.Drawing.Size(388, 168);
            this._onDemandTabPage.TabIndex = 3;
            this._onDemandTabPage.Text = "ID Shield On Demand";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this._doNotCreateVOAFileRadioButton);
            this.groupBox3.Controls.Add(this._promptVOAFileRadioButton);
            this.groupBox3.Controls.Add(this._createVOAFileRadioButton);
            this.groupBox3.Location = new System.Drawing.Point(17, 15);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(352, 93);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "If ID Shield data file does not exist, when saving:";
            // 
            // _doNotCreateVOAFileRadioButton
            // 
            this._doNotCreateVOAFileRadioButton.AutoSize = true;
            this._doNotCreateVOAFileRadioButton.Location = new System.Drawing.Point(7, 19);
            this._doNotCreateVOAFileRadioButton.Name = "_doNotCreateVOAFileRadioButton";
            this._doNotCreateVOAFileRadioButton.Size = new System.Drawing.Size(139, 17);
            this._doNotCreateVOAFileRadioButton.TabIndex = 2;
            this._doNotCreateVOAFileRadioButton.TabStop = true;
            this._doNotCreateVOAFileRadioButton.Text = "Do not create a data file";
            this._doNotCreateVOAFileRadioButton.UseVisualStyleBackColor = true;
            // 
            // _promptVOAFileRadioButton
            // 
            this._promptVOAFileRadioButton.AutoSize = true;
            this._promptVOAFileRadioButton.Location = new System.Drawing.Point(7, 44);
            this._promptVOAFileRadioButton.Name = "_promptVOAFileRadioButton";
            this._promptVOAFileRadioButton.Size = new System.Drawing.Size(208, 17);
            this._promptVOAFileRadioButton.TabIndex = 1;
            this._promptVOAFileRadioButton.TabStop = true;
            this._promptVOAFileRadioButton.Text = "Prompt for whether to create a data file";
            this._promptVOAFileRadioButton.UseVisualStyleBackColor = true;
            // 
            // _createVOAFileRadioButton
            // 
            this._createVOAFileRadioButton.AutoSize = true;
            this._createVOAFileRadioButton.Location = new System.Drawing.Point(7, 67);
            this._createVOAFileRadioButton.Name = "_createVOAFileRadioButton";
            this._createVOAFileRadioButton.Size = new System.Drawing.Size(105, 17);
            this._createVOAFileRadioButton.TabIndex = 0;
            this._createVOAFileRadioButton.TabStop = true;
            this._createVOAFileRadioButton.Text = "Create a data file";
            this._createVOAFileRadioButton.UseVisualStyleBackColor = true;
            // 
            // VerificationOptionsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(420, 251);
            this.Controls.Add(this._tabControl);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VerificationOptionsDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Verification options";
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._autoZoomScaleTrackBar)).EndInit();
            this._tabControl.ResumeLayout(false);
            this._generalTabPage.ResumeLayout(false);
            this._generalTabPage.PerformLayout();
            this._slideshowTabPage.ResumeLayout(false);
            this._slideshowTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._slideshowIntervalUpDown)).EndInit();
            this._zoomTabPage.ResumeLayout(false);
            this._zoomTabPage.PerformLayout();
            this._onDemandTabPage.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TabControl _tabControl;
        private System.Windows.Forms.TabPage _generalTabPage;
        private System.Windows.Forms.TabPage _slideshowTabPage;
        private System.Windows.Forms.ComboBox _autoToolComboBox;
        private System.Windows.Forms.CheckBox _autoToolCheckBox;
        private System.Windows.Forms.CheckBox _slideshowAutoStartCheckBox;
        private System.Windows.Forms.Label label4;
        private Extract.Utilities.Forms.BetterNumericUpDown _slideshowIntervalUpDown;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox _slideshowRunKeyComboBox;
        private System.Windows.Forms.TabPage _zoomTabPage;
        private System.Windows.Forms.CheckBox _autoZoomCheckBox;
        private System.Windows.Forms.TrackBar _autoZoomScaleTrackBar;
        private System.Windows.Forms.TextBox _autoZoomScaleTextBox;
        private System.Windows.Forms.CheckBox _OCRCheckBox;
        private System.Windows.Forms.TabPage _onDemandTabPage;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton _createVOAFileRadioButton;
        private System.Windows.Forms.RadioButton _promptVOAFileRadioButton;
        private System.Windows.Forms.RadioButton _doNotCreateVOAFileRadioButton;
    }
}