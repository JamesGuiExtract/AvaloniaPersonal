namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    partial class ConfigurationForm
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
            this._label1 = new System.Windows.Forms.Label();
            this._configFileNameTextBox = new System.Windows.Forms.TextBox();
            this._fileBrowseButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._enableInputTrackingCheckBox = new System.Windows.Forms.CheckBox();
            this._enableCountersCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // _label1
            // 
            this._label1.AutoSize = true;
            this._label1.Location = new System.Drawing.Point(12, 9);
            this._label1.Name = "_label1";
            this._label1.Size = new System.Drawing.Size(175, 13);
            this._label1.TabIndex = 0;
            this._label1.Text = "Specify the configuration file to use:";
            // 
            // _configFileNameTextBox
            // 
            this._configFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._configFileNameTextBox.Location = new System.Drawing.Point(15, 26);
            this._configFileNameTextBox.Name = "_configFileNameTextBox";
            this._configFileNameTextBox.Size = new System.Drawing.Size(438, 20);
            this._configFileNameTextBox.TabIndex = 1;
            // 
            // _fileBrowseButton
            // 
            this._fileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._fileBrowseButton.Location = new System.Drawing.Point(459, 24);
            this._fileBrowseButton.Name = "_fileBrowseButton";
            this._fileBrowseButton.Size = new System.Drawing.Size(24, 23);
            this._fileBrowseButton.TabIndex = 2;
            this._fileBrowseButton.Text = "...";
            this._fileBrowseButton.UseVisualStyleBackColor = true;
            this._fileBrowseButton.Click += new System.EventHandler(this.HandleFileBrowseButtonClick);
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(327, 91);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 3;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(408, 91);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 4;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _enableInputTrackingCheckBox
            // 
            this._enableInputTrackingCheckBox.AutoSize = true;
            this._enableInputTrackingCheckBox.Location = new System.Drawing.Point(15, 53);
            this._enableInputTrackingCheckBox.Name = "_enableInputTrackingCheckBox";
            this._enableInputTrackingCheckBox.Size = new System.Drawing.Size(156, 17);
            this._enableInputTrackingCheckBox.TabIndex = 5;
            this._enableInputTrackingCheckBox.Text = "Enable input event tracking";
            this._enableInputTrackingCheckBox.UseVisualStyleBackColor = true;
            // 
            // _enableCountersCheckBox
            // 
            this._enableCountersCheckBox.AutoSize = true;
            this._enableCountersCheckBox.Location = new System.Drawing.Point(15, 77);
            this._enableCountersCheckBox.Name = "_enableCountersCheckBox";
            this._enableCountersCheckBox.Size = new System.Drawing.Size(153, 17);
            this._enableCountersCheckBox.TabIndex = 6;
            this._enableCountersCheckBox.Text = "Enable data entry counters";
            this._enableCountersCheckBox.UseVisualStyleBackColor = true;
            // 
            // ConfigurationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 126);
            this.ControlBox = false;
            this.Controls.Add(this._enableCountersCheckBox);
            this.Controls.Add(this._enableInputTrackingCheckBox);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._fileBrowseButton);
            this.Controls.Add(this._configFileNameTextBox);
            this.Controls.Add(this._label1);
            this.Name = "ConfigurationForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Data Entry: Verify Extracted Data";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _label1;
        private System.Windows.Forms.TextBox _configFileNameTextBox;
        private System.Windows.Forms.Button _fileBrowseButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.CheckBox _enableInputTrackingCheckBox;
        private System.Windows.Forms.CheckBox _enableCountersCheckBox;
    }
}
