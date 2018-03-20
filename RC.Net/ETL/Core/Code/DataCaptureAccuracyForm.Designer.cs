namespace Extract.ETL
{
    partial class DataCaptureAccuracyForm
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
            this.label1 = new System.Windows.Forms.Label();
            this._expectedAttributeSetComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this._foundAttributeSetComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this._xpathToIgnoreTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this._xpathContainerOnlyTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this._descriptionTextBox = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this._schedulerControl = new Extract.Utilities.Forms.SchedulerControl();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Expected attribute set";
            // 
            // _expectedAttributeSetComboBox
            // 
            this._expectedAttributeSetComboBox.FormattingEnabled = true;
            this._expectedAttributeSetComboBox.Location = new System.Drawing.Point(166, 41);
            this._expectedAttributeSetComboBox.Name = "_expectedAttributeSetComboBox";
            this._expectedAttributeSetComboBox.Size = new System.Drawing.Size(319, 21);
            this._expectedAttributeSetComboBox.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Found attribute set";
            // 
            // _foundAttributeSetComboBox
            // 
            this._foundAttributeSetComboBox.FormattingEnabled = true;
            this._foundAttributeSetComboBox.Location = new System.Drawing.Point(166, 70);
            this._foundAttributeSetComboBox.Name = "_foundAttributeSetComboBox";
            this._foundAttributeSetComboBox.Size = new System.Drawing.Size(319, 21);
            this._foundAttributeSetComboBox.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(138, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "XPath of attributes to ignore";
            // 
            // _xpathToIgnoreTextBox
            // 
            this._xpathToIgnoreTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._xpathToIgnoreTextBox.Location = new System.Drawing.Point(6, 44);
            this._xpathToIgnoreTextBox.Multiline = true;
            this._xpathToIgnoreTextBox.Name = "_xpathToIgnoreTextBox";
            this._xpathToIgnoreTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._xpathToIgnoreTextBox.Size = new System.Drawing.Size(743, 84);
            this._xpathToIgnoreTextBox.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 138);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(163, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "XPath of container only attributes";
            // 
            // _xpathContainerOnlyTextBox
            // 
            this._xpathContainerOnlyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._xpathContainerOnlyTextBox.Location = new System.Drawing.Point(6, 154);
            this._xpathContainerOnlyTextBox.Multiline = true;
            this._xpathContainerOnlyTextBox.Name = "_xpathContainerOnlyTextBox";
            this._xpathContainerOnlyTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._xpathContainerOnlyTextBox.Size = new System.Drawing.Size(743, 84);
            this._xpathContainerOnlyTextBox.TabIndex = 6;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(700, 402);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(619, 402);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Description";
            // 
            // _descriptionTextBox
            // 
            this._descriptionTextBox.Location = new System.Drawing.Point(166, 13);
            this._descriptionTextBox.Name = "_descriptionTextBox";
            this._descriptionTextBox.Size = new System.Drawing.Size(319, 20);
            this._descriptionTextBox.TabIndex = 1;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 97);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(763, 297);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this._xpathContainerOnlyTextBox);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this._xpathToIgnoreTextBox);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(755, 271);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "XPath Settings";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this._schedulerControl);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(755, 271);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Schedule";
            // 
            // _schedulerControl
            // 
            this._schedulerControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._schedulerControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._schedulerControl.Location = new System.Drawing.Point(6, 16);
            this._schedulerControl.Name = "_schedulerControl";
            this._schedulerControl.Size = new System.Drawing.Size(351, 239);
            this._schedulerControl.TabIndex = 7;
            scheduledEvent1.Duration = null;
            scheduledEvent1.Enabled = true;
            scheduledEvent1.End = null;
            scheduledEvent1.Exclusions = new Extract.Utilities.ScheduledEvent[0];
            scheduledEvent1.RecurrenceUnit = null;
            scheduledEvent1.Start = new System.DateTime(2018, 3, 20, 12, 45, 14, 0);
            this._schedulerControl.Value = scheduledEvent1;
            // 
            // DataCaptureAccuracyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(787, 437);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this._descriptionTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this._foundAttributeSetComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._expectedAttributeSetComboBox);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(726, 397);
            this.Name = "DataCaptureAccuracyForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Data capture accuracy";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox _expectedAttributeSetComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox _foundAttributeSetComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox _xpathToIgnoreTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox _xpathContainerOnlyTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox _descriptionTextBox;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private Utilities.Forms.SchedulerControl _schedulerControl;
    }
}