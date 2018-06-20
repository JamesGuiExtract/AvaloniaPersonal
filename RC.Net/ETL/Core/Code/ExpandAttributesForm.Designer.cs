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
            this._storeEmptyAttributesCheckBox = new System.Windows.Forms.CheckBox();
            this._storeSpatialInfoCheckBox = new System.Windows.Forms.CheckBox();
            this._schedulerControl = new Extract.Utilities.Forms.SchedulerControl();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageSchedule = new System.Windows.Forms.TabPage();
            this.tabPageDashboardFields = new System.Windows.Forms.TabPage();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.tabControl.SuspendLayout();
            this.tabPageSchedule.SuspendLayout();
            this.tabPageDashboardFields.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(710, 403);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(629, 403);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 5;
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
            this._descriptionTextBox.Size = new System.Drawing.Size(395, 20);
            this._descriptionTextBox.TabIndex = 0;
            // 
            // _storeEmptyAttributesCheckBox
            // 
            this._storeEmptyAttributesCheckBox.AutoSize = true;
            this._storeEmptyAttributesCheckBox.Location = new System.Drawing.Point(13, 62);
            this._storeEmptyAttributesCheckBox.Name = "_storeEmptyAttributesCheckBox";
            this._storeEmptyAttributesCheckBox.Size = new System.Drawing.Size(128, 17);
            this._storeEmptyAttributesCheckBox.TabIndex = 2;
            this._storeEmptyAttributesCheckBox.Text = "Store empty attributes";
            this._storeEmptyAttributesCheckBox.UseVisualStyleBackColor = true;
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
            // _schedulerControl
            // 
            this._schedulerControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._schedulerControl.Location = new System.Drawing.Point(6, 6);
            this._schedulerControl.MinimumSize = new System.Drawing.Size(351, 153);
            this._schedulerControl.Name = "_schedulerControl";
            this._schedulerControl.Size = new System.Drawing.Size(385, 157);
            this._schedulerControl.TabIndex = 4;
            scheduledEvent1.Duration = null;
            scheduledEvent1.Enabled = true;
            scheduledEvent1.End = null;
            scheduledEvent1.Exclusions = new Extract.Utilities.ScheduledEvent[0];
            scheduledEvent1.RecurrenceUnit = null;
            scheduledEvent1.Start = new System.DateTime(2018, 3, 16, 9, 38, 23, 0);
            this._schedulerControl.Value = scheduledEvent1;
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPageSchedule);
            this.tabControl.Controls.Add(this.tabPageDashboardFields);
            this.tabControl.Location = new System.Drawing.Point(16, 85);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(769, 304);
            this.tabControl.TabIndex = 7;
            // 
            // tabPageSchedule
            // 
            this.tabPageSchedule.Controls.Add(this._schedulerControl);
            this.tabPageSchedule.Location = new System.Drawing.Point(4, 22);
            this.tabPageSchedule.Name = "tabPageSchedule";
            this.tabPageSchedule.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSchedule.Size = new System.Drawing.Size(761, 278);
            this.tabPageSchedule.TabIndex = 0;
            this.tabPageSchedule.Text = "Schedule";
            this.tabPageSchedule.UseVisualStyleBackColor = true;
            // 
            // tabPageDashboardFields
            // 
            this.tabPageDashboardFields.Controls.Add(this.dataGridView);
            this.tabPageDashboardFields.Location = new System.Drawing.Point(4, 22);
            this.tabPageDashboardFields.Name = "tabPageDashboardFields";
            this.tabPageDashboardFields.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDashboardFields.Size = new System.Drawing.Size(761, 278);
            this.tabPageDashboardFields.TabIndex = 1;
            this.tabPageDashboardFields.Text = "Dashboard fields";
            this.tabPageDashboardFields.UseVisualStyleBackColor = true;
            // 
            // dataGridView
            // 
            this.dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView.Location = new System.Drawing.Point(3, 3);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.Size = new System.Drawing.Size(755, 272);
            this.dataGridView.TabIndex = 0;
            this.dataGridView.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.HandleDataGridView_DefaultValuesNeeded);
            // 
            // ExpandAttributesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(797, 435);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this._storeSpatialInfoCheckBox);
            this.Controls.Add(this._storeEmptyAttributesCheckBox);
            this.Controls.Add(this._descriptionTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(503, 347);
            this.Name = "ExpandAttributesForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Expand attributes";
            this.tabControl.ResumeLayout(false);
            this.tabPageSchedule.ResumeLayout(false);
            this.tabPageDashboardFields.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _descriptionTextBox;
        private System.Windows.Forms.CheckBox _storeEmptyAttributesCheckBox;
        private System.Windows.Forms.CheckBox _storeSpatialInfoCheckBox;
        private Utilities.Forms.SchedulerControl _schedulerControl;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageSchedule;
        private System.Windows.Forms.TabPage tabPageDashboardFields;
        private System.Windows.Forms.DataGridView dataGridView;
    }
}