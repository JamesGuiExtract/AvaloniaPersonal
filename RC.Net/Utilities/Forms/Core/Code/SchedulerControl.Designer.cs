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
            this._durationDaysNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this._recurrenceUnitComboBox = new System.Windows.Forms.ComboBox();
            this._recurGroup = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this._durationMinutesNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this._durationHoursNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this._scheduleDurationCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this._durationDaysNumericUpDown)).BeginInit();
            this._recurGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._durationMinutesNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._durationHoursNumericUpDown)).BeginInit();
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
            // 
            // _startTimePicker
            // 
            this._startTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this._startTimePicker.Location = new System.Drawing.Point(220, 15);
            this._startTimePicker.Name = "_startTimePicker";
            this._startTimePicker.ShowUpDown = true;
            this._startTimePicker.Size = new System.Drawing.Size(118, 20);
            this._startTimePicker.TabIndex = 2;
            // 
            // _endDatePicker
            // 
            this._endDatePicker.CustomFormat = "ddd MM/dd/yyyy";
            this._endDatePicker.Enabled = false;
            this._endDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._endDatePicker.Location = new System.Drawing.Point(85, 44);
            this._endDatePicker.Name = "_endDatePicker";
            this._endDatePicker.Size = new System.Drawing.Size(129, 20);
            this._endDatePicker.TabIndex = 4;
            // 
            // _endTimePicker
            // 
            this._endTimePicker.Enabled = false;
            this._endTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this._endTimePicker.Location = new System.Drawing.Point(220, 44);
            this._endTimePicker.Name = "_endTimePicker";
            this._endTimePicker.ShowUpDown = true;
            this._endTimePicker.Size = new System.Drawing.Size(118, 20);
            this._endTimePicker.TabIndex = 5;
            // 
            // _untilCheckBox
            // 
            this._untilCheckBox.AutoSize = true;
            this._untilCheckBox.Location = new System.Drawing.Point(16, 46);
            this._untilCheckBox.Name = "_untilCheckBox";
            this._untilCheckBox.Size = new System.Drawing.Size(47, 17);
            this._untilCheckBox.TabIndex = 3;
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
            this._recurEveryRadioButton.TabIndex = 8;
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
            this._specifiedTimeRadioButton.TabIndex = 7;
            this._specifiedTimeRadioButton.TabStop = true;
            this._specifiedTimeRadioButton.Text = "Specified time";
            this._specifiedTimeRadioButton.UseVisualStyleBackColor = true;
            // 
            // _durationDaysNumericUpDown
            // 
            this._durationDaysNumericUpDown.Enabled = false;
            this._durationDaysNumericUpDown.Location = new System.Drawing.Point(184, 149);
            this._durationDaysNumericUpDown.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this._durationDaysNumericUpDown.Name = "_durationDaysNumericUpDown";
            this._durationDaysNumericUpDown.Size = new System.Drawing.Size(75, 20);
            this._durationDaysNumericUpDown.TabIndex = 11;
            // 
            // _recuranceUnitComboBox
            // 
            this._recurrenceUnitComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._recurrenceUnitComboBox.Enabled = false;
            this._recurrenceUnitComboBox.FormattingEnabled = true;
            this._recurrenceUnitComboBox.Location = new System.Drawing.Point(95, 39);
            this._recurrenceUnitComboBox.Name = "_recuranceUnitComboBox";
            this._recurrenceUnitComboBox.Size = new System.Drawing.Size(121, 21);
            this._recurrenceUnitComboBox.TabIndex = 9;
            // 
            // _recurGroup
            // 
            this._recurGroup.Controls.Add(this._specifiedTimeRadioButton);
            this._recurGroup.Controls.Add(this._recurrenceUnitComboBox);
            this._recurGroup.Controls.Add(this._recurEveryRadioButton);
            this._recurGroup.Location = new System.Drawing.Point(16, 70);
            this._recurGroup.Name = "_recurGroup";
            this._recurGroup.Size = new System.Drawing.Size(322, 73);
            this._recurGroup.TabIndex = 6;
            this._recurGroup.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(137, 153);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Days";
            // 
            // _durationMinutesNumericUpDown
            // 
            this._durationMinutesNumericUpDown.Enabled = false;
            this._durationMinutesNumericUpDown.Location = new System.Drawing.Point(184, 204);
            this._durationMinutesNumericUpDown.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this._durationMinutesNumericUpDown.MaximumSize = new System.Drawing.Size(75, 0);
            this._durationMinutesNumericUpDown.Name = "_durationMinutesNumericUpDown";
            this._durationMinutesNumericUpDown.Size = new System.Drawing.Size(75, 20);
            this._durationMinutesNumericUpDown.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(137, 180);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Hours";
            // 
            // _durationHoursNumericUpDown
            // 
            this._durationHoursNumericUpDown.Enabled = false;
            this._durationHoursNumericUpDown.Location = new System.Drawing.Point(184, 178);
            this._durationHoursNumericUpDown.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this._durationHoursNumericUpDown.Name = "_durationHoursNumericUpDown";
            this._durationHoursNumericUpDown.Size = new System.Drawing.Size(75, 20);
            this._durationHoursNumericUpDown.TabIndex = 12;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(137, 206);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Minutes";
            // 
            // _scheduleDurationCheckBox
            // 
            this._scheduleDurationCheckBox.AutoSize = true;
            this._scheduleDurationCheckBox.Location = new System.Drawing.Point(16, 151);
            this._scheduleDurationCheckBox.Name = "_scheduleDurationCheckBox";
            this._scheduleDurationCheckBox.Size = new System.Drawing.Size(114, 17);
            this._scheduleDurationCheckBox.TabIndex = 10;
            this._scheduleDurationCheckBox.Text = "Schedule valid for:";
            this._scheduleDurationCheckBox.UseVisualStyleBackColor = true;
            this._scheduleDurationCheckBox.CheckedChanged += new System.EventHandler(this.HandleScheduleDurationCheckBoxCheckedChanged);
            // 
            // SchedulerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this._scheduleDurationCheckBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._recurGroup);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._untilCheckBox);
            this.Controls.Add(this._durationHoursNumericUpDown);
            this.Controls.Add(this._endDatePicker);
            this.Controls.Add(this._durationMinutesNumericUpDown);
            this.Controls.Add(this._startTimePicker);
            this.Controls.Add(this._durationDaysNumericUpDown);
            this.Controls.Add(this._startDatePicker);
            this.Controls.Add(this._endTimePicker);
            this.Controls.Add(this.label1);
            this.Name = "SchedulerControl";
            this.Size = new System.Drawing.Size(351, 239);
            ((System.ComponentModel.ISupportInitialize)(this._durationDaysNumericUpDown)).EndInit();
            this._recurGroup.ResumeLayout(false);
            this._recurGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._durationMinutesNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._durationHoursNumericUpDown)).EndInit();
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
        private System.Windows.Forms.NumericUpDown _durationDaysNumericUpDown;
        private System.Windows.Forms.ComboBox _recurrenceUnitComboBox;
        private System.Windows.Forms.GroupBox _recurGroup;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown _durationMinutesNumericUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown _durationHoursNumericUpDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox _scheduleDurationCheckBox;
    }
}
