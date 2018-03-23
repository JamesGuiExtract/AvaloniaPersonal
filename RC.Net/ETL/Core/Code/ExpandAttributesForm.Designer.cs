namespace Extract.ETL
{
    partial class ExpandAttributesForm
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
            Extract.Utilities.ScheduledEvent scheduledEvent1 = new Extract.Utilities.ScheduledEvent();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this._descriptionTextBox = new System.Windows.Forms.TextBox();
            this._stroreEmptyAttributesCheckBox = new System.Windows.Forms.CheckBox();
            this._storeSpatialInfoCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this._schedulerControl = new Extract.Utilities.Forms.SchedulerControl();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(404, 276);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(323, 276);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Description";
            // 
            // _descriptionTextBox
            // 
            this._descriptionTextBox.Location = new System.Drawing.Point(80, 11);
            this._descriptionTextBox.Name = "_descriptionTextBox";
            this._descriptionTextBox.Size = new System.Drawing.Size(400, 20);
            this._descriptionTextBox.TabIndex = 0;
            // 
            // _stroreEmptyAttributesCheckBox
            // 
            this._stroreEmptyAttributesCheckBox.AutoSize = true;
            this._stroreEmptyAttributesCheckBox.Location = new System.Drawing.Point(13, 62);
            this._stroreEmptyAttributesCheckBox.Name = "_stroreEmptyAttributesCheckBox";
            this._stroreEmptyAttributesCheckBox.Size = new System.Drawing.Size(128, 17);
            this._stroreEmptyAttributesCheckBox.TabIndex = 2;
            this._stroreEmptyAttributesCheckBox.Text = "Store empty attributes";
            this._stroreEmptyAttributesCheckBox.UseVisualStyleBackColor = true;
            // 
            // _storeSpatialInfoCheckBox
            // 
            this._storeSpatialInfoCheckBox.AutoSize = true;
            this._storeSpatialInfoCheckBox.Location = new System.Drawing.Point(13, 38);
            this._storeSpatialInfoCheckBox.Name = "_storeSpatialInfoCheckBox";
            this._storeSpatialInfoCheckBox.Size = new System.Drawing.Size(209, 17);
            this._storeSpatialInfoCheckBox.TabIndex = 1;
            this._storeSpatialInfoCheckBox.Text = "Store spatial information (Raster zones)";
            this._storeSpatialInfoCheckBox.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Schedule";
            // 
            // _schedulerControl
            // 
            this._schedulerControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._schedulerControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._schedulerControl.Location = new System.Drawing.Point(13, 108);
            this._schedulerControl.MinimumSize = new System.Drawing.Size(351, 153);
            this._schedulerControl.Name = "_schedulerControl";
            this._schedulerControl.Size = new System.Drawing.Size(385, 157);
            this._schedulerControl.TabIndex = 3;
            scheduledEvent1.Duration = null;
            scheduledEvent1.Enabled = true;
            scheduledEvent1.End = null;
            scheduledEvent1.Exclusions = new Extract.Utilities.ScheduledEvent[0];
            scheduledEvent1.RecurrenceUnit = null;
            scheduledEvent1.Start = new System.DateTime(2018, 3, 16, 9, 38, 23, 0);
            this._schedulerControl.Value = scheduledEvent1;
            // 
            // ExpandAttributesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(487, 308);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._schedulerControl);
            this.Controls.Add(this._storeSpatialInfoCheckBox);
            this.Controls.Add(this._stroreEmptyAttributesCheckBox);
            this.Controls.Add(this._descriptionTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(503, 340);
            this.Name = "ExpandAttributesForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Expand attributes";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _descriptionTextBox;
        private System.Windows.Forms.CheckBox _stroreEmptyAttributesCheckBox;
        private System.Windows.Forms.CheckBox _storeSpatialInfoCheckBox;
        private Utilities.Forms.SchedulerControl _schedulerControl;
        private System.Windows.Forms.Label label2;
    }
}