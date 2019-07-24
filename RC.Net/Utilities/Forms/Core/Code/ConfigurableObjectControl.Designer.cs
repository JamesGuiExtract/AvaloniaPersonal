namespace Extract.Utilities.Forms
{
    partial class ConfigurableObjectControl
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
            this._objectSelectionComboBox = new System.Windows.Forms.ComboBox();
            this._configureButton = new System.Windows.Forms.Button();
            this._configurationReminderLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _objectSelectionComboBox
            // 
            this._objectSelectionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._objectSelectionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._objectSelectionComboBox.FormattingEnabled = true;
            this._objectSelectionComboBox.Location = new System.Drawing.Point(4, 4);
            this._objectSelectionComboBox.Name = "_objectSelectionComboBox";
            this._objectSelectionComboBox.Size = new System.Drawing.Size(238, 21);
            this._objectSelectionComboBox.TabIndex = 0;
            this._objectSelectionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleObjectSelectedIndexChanged);
            // 
            // _configureButton
            // 
            this._configureButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._configureButton.Enabled = false;
            this._configureButton.Location = new System.Drawing.Point(248, 3);
            this._configureButton.Name = "_configureButton";
            this._configureButton.Size = new System.Drawing.Size(75, 23);
            this._configureButton.TabIndex = 1;
            this._configureButton.Text = "Configure";
            this._configureButton.UseVisualStyleBackColor = true;
            this._configureButton.Click += new System.EventHandler(this.HandleConfigureButtonClick);
            // 
            // _configurationReminderLabel
            // 
            this._configurationReminderLabel.AutoSize = true;
            this._configurationReminderLabel.Location = new System.Drawing.Point(4, 32);
            this._configurationReminderLabel.Name = "_configurationReminderLabel";
            this._configurationReminderLabel.Size = new System.Drawing.Size(151, 13);
            this._configurationReminderLabel.TabIndex = 2;
            this._configurationReminderLabel.Text = "The object must be configured";
            // 
            // ConfigurableObjectControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._configurationReminderLabel);
            this.Controls.Add(this._configureButton);
            this.Controls.Add(this._objectSelectionComboBox);
            this.Name = "ConfigurableObjectControl";
            this.Size = new System.Drawing.Size(326, 49);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox _objectSelectionComboBox;
        private System.Windows.Forms.Button _configureButton;
        private System.Windows.Forms.Label _configurationReminderLabel;
    }
}
