namespace Extract.Utilities.Forms
{
    partial class SchedulerControl
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
            this.label1 = new System.Windows.Forms.Label();
            this._startDatePicker = new System.Windows.Forms.DateTimePicker();
            this._startTimePicker = new System.Windows.Forms.DateTimePicker();
            this._endDatePicker = new System.Windows.Forms.DateTimePicker();
            this._endTimePicker = new System.Windows.Forms.DateTimePicker();
            this._untilCheckBox = new System.Windows.Forms.CheckBox();
            this._recurEveryRadioButton = new System.Windows.Forms.RadioButton();
            this._specifiedTimeRadioButton = new System.Windows.Forms.RadioButton();
            this._recurrenceUnitComboBox = new System.Windows.Forms.ComboBox();
            this._recurGroup = new System.Windows.Forms.GroupBox();
            this._recurGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Start date";
            // 
            // _startDatePicker
            // 
            this._startDatePicker.CustomFormat = "ddd MM/dd/yyyy";
            this._startDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._startDatePicker.Location = new System.Drawing.Point(85, 15);
            this._startDatePicker.Name = "_startDatePicker";
            this._startDatePicker.Size = new System.Drawing.Size(129, 20);
            this._startDatePicker.TabIndex = 1;
            this._startDatePicker.ValueChanged += new System.EventHandler(this.Handle_ValueChanged);
            // 
            // _startTimePicker
            // 
            this._startTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this._startTimePicker.Location = new System.Drawing.Point(220, 15);
            this._startTimePicker.Name = "_startTimePicker";
            this._startTimePicker.ShowUpDown = true;
            this._startTimePicker.Size = new System.Drawing.Size(118, 20);
            this._startTimePicker.TabIndex = 2;
            this._startTimePicker.ValueChanged += new System.EventHandler(this.Handle_ValueChanged);
            // 
            // _endDatePicker
            // 
            this._endDatePicker.CustomFormat = "ddd MM/dd/yyyy";
            this._endDatePicker.Enabled = false;
            this._endDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._endDatePicker.Location = new System.Drawing.Point(94, 67);
            this._endDatePicker.Name = "_endDatePicker";
            this._endDatePicker.Size = new System.Drawing.Size(129, 20);
            this._endDatePicker.TabIndex = 7;
            this._endDatePicker.ValueChanged += new System.EventHandler(this.Handle_ValueChanged);
            // 
            // _endTimePicker
            // 
            this._endTimePicker.Enabled = false;
            this._endTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this._endTimePicker.Location = new System.Drawing.Point(229, 67);
            this._endTimePicker.Name = "_endTimePicker";
            this._endTimePicker.ShowUpDown = true;
            this._endTimePicker.Size = new System.Drawing.Size(118, 20);
            this._endTimePicker.TabIndex = 8;
            this._endTimePicker.ValueChanged += new System.EventHandler(this.Handle_ValueChanged);
            // 
            // _untilCheckBox
            // 
            this._untilCheckBox.AutoSize = true;
            this._untilCheckBox.Enabled = false;
            this._untilCheckBox.Location = new System.Drawing.Point(25, 69);
            this._untilCheckBox.Name = "_untilCheckBox";
            this._untilCheckBox.Size = new System.Drawing.Size(47, 17);
            this._untilCheckBox.TabIndex = 6;
            this._untilCheckBox.Text = "Until";
            this._untilCheckBox.UseVisualStyleBackColor = true;
            this._untilCheckBox.CheckedChanged += new System.EventHandler(this.HandleUntilCheckBoxCheckedChanged);
            // 
            // _recurEveryRadioButton
            // 
            this._recurEveryRadioButton.AutoSize = true;
            this._recurEveryRadioButton.Location = new System.Drawing.Point(6, 40);
            this._recurEveryRadioButton.Name = "_recurEveryRadioButton";
            this._recurEveryRadioButton.Size = new System.Drawing.Size(83, 17);
            this._recurEveryRadioButton.TabIndex = 4;
            this._recurEveryRadioButton.TabStop = true;
            this._recurEveryRadioButton.Text = "Recur every";
            this._recurEveryRadioButton.UseVisualStyleBackColor = true;
            this._recurEveryRadioButton.CheckedChanged += new System.EventHandler(this.HandleRecurEveryRadioButtonCheckedChanged);
            // 
            // _specifiedTimeRadioButton
            // 
            this._specifiedTimeRadioButton.AutoSize = true;
            this._specifiedTimeRadioButton.Location = new System.Drawing.Point(6, 16);
            this._specifiedTimeRadioButton.Name = "_specifiedTimeRadioButton";
            this._specifiedTimeRadioButton.Size = new System.Drawing.Size(91, 17);
            this._specifiedTimeRadioButton.TabIndex = 3;
            this._specifiedTimeRadioButton.TabStop = true;
            this._specifiedTimeRadioButton.Text = "Specified time";
            this._specifiedTimeRadioButton.UseVisualStyleBackColor = true;
            this._specifiedTimeRadioButton.CheckedChanged += new System.EventHandler(this.Handle_ValueChanged);
            // 
            // _recurrenceUnitComboBox
            // 
            this._recurrenceUnitComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._recurrenceUnitComboBox.Enabled = false;
            this._recurrenceUnitComboBox.FormattingEnabled = true;
            this._recurrenceUnitComboBox.Location = new System.Drawing.Point(95, 39);
            this._recurrenceUnitComboBox.Name = "_recurrenceUnitComboBox";
            this._recurrenceUnitComboBox.Size = new System.Drawing.Size(121, 21);
            this._recurrenceUnitComboBox.TabIndex = 5;
            this._recurrenceUnitComboBox.TextChanged += new System.EventHandler(this.Handle_ValueChanged);
            // 
            // _recurGroup
            // 
            this._recurGroup.Controls.Add(this._specifiedTimeRadioButton);
            this._recurGroup.Controls.Add(this._untilCheckBox);
            this._recurGroup.Controls.Add(this._recurrenceUnitComboBox);
            this._recurGroup.Controls.Add(this._endDatePicker);
            this._recurGroup.Controls.Add(this._recurEveryRadioButton);
            this._recurGroup.Controls.Add(this._endTimePicker);
            this._recurGroup.Location = new System.Drawing.Point(16, 41);
            this._recurGroup.Name = "_recurGroup";
            this._recurGroup.Size = new System.Drawing.Size(353, 102);
            this._recurGroup.TabIndex = 6;
            this._recurGroup.TabStop = false;
            // 
            // SchedulerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this._recurGroup);
            this.Controls.Add(this._startTimePicker);
            this.Controls.Add(this._startDatePicker);
            this.Controls.Add(this.label1);
            this.Name = "SchedulerControl";
            this.Size = new System.Drawing.Size(383, 153);
            this._recurGroup.ResumeLayout(false);
            this._recurGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker _startDatePicker;
        private System.Windows.Forms.DateTimePicker _startTimePicker;
        private System.Windows.Forms.DateTimePicker _endDatePicker;
        private System.Windows.Forms.DateTimePicker _endTimePicker;
        private System.Windows.Forms.CheckBox _untilCheckBox;
        private System.Windows.Forms.RadioButton _recurEveryRadioButton;
        private System.Windows.Forms.RadioButton _specifiedTimeRadioButton;
        private System.Windows.Forms.ComboBox _recurrenceUnitComboBox;
        private System.Windows.Forms.GroupBox _recurGroup;
    }
}
