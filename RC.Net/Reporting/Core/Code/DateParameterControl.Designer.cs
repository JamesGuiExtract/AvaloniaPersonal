namespace Extract.Reporting
{
    partial class DateParameterControl
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
            this._parameterValue = new System.Windows.Forms.DateTimePicker();
            this._parameterName = new System.Windows.Forms.GroupBox();
            this._parameterName.SuspendLayout();
            this.SuspendLayout();
            // 
            // _parameterValue
            // 
            this._parameterValue.CustomFormat = "MM/dd/yyyy HH:mm";
            this._parameterValue.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._parameterValue.Location = new System.Drawing.Point(6, 19);
            this._parameterValue.Name = "_parameterValue";
            this._parameterValue.Size = new System.Drawing.Size(156, 20);
            this._parameterValue.TabIndex = 1;
            this._parameterValue.Value = new System.DateTime(2009, 1, 6, 0, 0, 0, 0);
            this._parameterValue.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _parameterName
            // 
            this._parameterName.Controls.Add(this._parameterValue);
            this._parameterName.Location = new System.Drawing.Point(3, 3);
            this._parameterName.Name = "_parameterName";
            this._parameterName.Size = new System.Drawing.Size(353, 45);
            this._parameterName.TabIndex = 2;
            this._parameterName.TabStop = false;
            this._parameterName.Text = "ParameterName";
            // 
            // DateParameterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._parameterName);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "DateParameterControl";
            this.Size = new System.Drawing.Size(359, 51);
            this._parameterName.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DateTimePicker _parameterValue;
        private System.Windows.Forms.GroupBox _parameterName;
    }
}
