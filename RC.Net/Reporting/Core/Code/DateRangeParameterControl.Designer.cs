namespace Extract.Reporting
{
    partial class DateRangeParameterControl
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._parameterValueBegin = new System.Windows.Forms.DateTimePicker();
            this._parameterValueEnd = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this._rangeValues = new System.Windows.Forms.ComboBox();
            this._parameterName = new System.Windows.Forms.GroupBox();
            this._parameterName.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Begin date";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(179, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "End date";
            // 
            // _parameterValueBegin
            // 
            this._parameterValueBegin.CustomFormat = "MM/dd/yyyy HH:mm";
            this._parameterValueBegin.Enabled = false;
            this._parameterValueBegin.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._parameterValueBegin.Location = new System.Drawing.Point(9, 72);
            this._parameterValueBegin.Name = "_parameterValueBegin";
            this._parameterValueBegin.Size = new System.Drawing.Size(156, 20);
            this._parameterValueBegin.TabIndex = 1;
            this._parameterValueBegin.Value = new System.DateTime(2009, 1, 6, 0, 0, 0, 0);
            this._parameterValueBegin.ValueChanged += new System.EventHandler(this.HandleBeginDateValueChanged);
            // 
            // _parameterValueEnd
            // 
            this._parameterValueEnd.CustomFormat = "MM/dd/yyyy HH:mm";
            this._parameterValueEnd.Enabled = false;
            this._parameterValueEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._parameterValueEnd.Location = new System.Drawing.Point(182, 72);
            this._parameterValueEnd.Name = "_parameterValueEnd";
            this._parameterValueEnd.Size = new System.Drawing.Size(156, 20);
            this._parameterValueEnd.TabIndex = 2;
            this._parameterValueEnd.Value = new System.DateTime(2009, 1, 6, 0, 0, 0, 0);
            this._parameterValueEnd.ValueChanged += new System.EventHandler(this.HandleEndDateValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Range";
            // 
            // _rangeValues
            // 
            this._rangeValues.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._rangeValues.FormattingEnabled = true;
            this._rangeValues.Location = new System.Drawing.Point(9, 32);
            this._rangeValues.Name = "_rangeValues";
            this._rangeValues.Size = new System.Drawing.Size(156, 21);
            this._rangeValues.TabIndex = 0;
            this._rangeValues.SelectedIndexChanged += new System.EventHandler(this.HandleRangeComboChanged);
            // 
            // _parameterName
            // 
            this._parameterName.Controls.Add(this.label1);
            this._parameterName.Controls.Add(this._rangeValues);
            this._parameterName.Controls.Add(this.label2);
            this._parameterName.Controls.Add(this.label3);
            this._parameterName.Controls.Add(this._parameterValueEnd);
            this._parameterName.Controls.Add(this._parameterValueBegin);
            this._parameterName.Location = new System.Drawing.Point(3, 3);
            this._parameterName.Name = "_parameterName";
            this._parameterName.Size = new System.Drawing.Size(353, 100);
            this._parameterName.TabIndex = 6;
            this._parameterName.TabStop = false;
            this._parameterName.Text = "ParameterName";
            // 
            // DateRangeParameterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._parameterName);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "DateRangeParameterControl";
            this.Size = new System.Drawing.Size(359, 106);
            this._parameterName.ResumeLayout(false);
            this._parameterName.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker _parameterValueBegin;
        private System.Windows.Forms.DateTimePicker _parameterValueEnd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox _rangeValues;
        private System.Windows.Forms.GroupBox _parameterName;
    }
}
