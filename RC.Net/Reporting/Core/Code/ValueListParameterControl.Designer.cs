namespace Extract.Reporting
{
    partial class ValueListParameterControl
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
            this._parameterValue = new System.Windows.Forms.ComboBox();
            this._parameterName = new System.Windows.Forms.GroupBox();
            this._parameterName.SuspendLayout();
            this.SuspendLayout();
            // 
            // _parameterValue
            // 
            this._parameterValue.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this._parameterValue.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this._parameterValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._parameterValue.FormattingEnabled = true;
            this._parameterValue.Location = new System.Drawing.Point(6, 19);
            this._parameterValue.Name = "_parameterValue";
            this._parameterValue.Size = new System.Drawing.Size(341, 21);
            this._parameterValue.TabIndex = 1;
            // 
            // _parameterName
            // 
            this._parameterName.Controls.Add(this._parameterValue);
            this._parameterName.Location = new System.Drawing.Point(3, 3);
            this._parameterName.Name = "_parameterName";
            this._parameterName.Size = new System.Drawing.Size(353, 46);
            this._parameterName.TabIndex = 2;
            this._parameterName.TabStop = false;
            this._parameterName.Text = "ParameterName";
            // 
            // ValueListParameterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._parameterName);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "ValueListParameterControl";
            this.Size = new System.Drawing.Size(361, 54);
            this._parameterName.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox _parameterValue;
        private System.Windows.Forms.GroupBox _parameterName;
    }
}
