namespace Extract.Reporting
{
    partial class MultipleSelectValueListParameterControl
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
            this._parameterName = new System.Windows.Forms.GroupBox();
            this.parameterValuesListBox = new System.Windows.Forms.CheckedListBox();
            this._parameterName.SuspendLayout();
            this.SuspendLayout();
            // 
            // _parameterName
            // 
            this._parameterName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._parameterName.Controls.Add(this.parameterValuesListBox);
            this._parameterName.Location = new System.Drawing.Point(3, 3);
            this._parameterName.Name = "_parameterName";
            this._parameterName.Size = new System.Drawing.Size(322, 209);
            this._parameterName.TabIndex = 2;
            this._parameterName.TabStop = false;
            this._parameterName.Text = "ParameterName";
            // 
            // parameterValuesListBox
            // 
            this.parameterValuesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.parameterValuesListBox.CheckOnClick = true;
            this.parameterValuesListBox.FormattingEnabled = true;
            this.parameterValuesListBox.Location = new System.Drawing.Point(7, 20);
            this.parameterValuesListBox.Name = "parameterValuesListBox";
            this.parameterValuesListBox.Size = new System.Drawing.Size(309, 184);
            this.parameterValuesListBox.TabIndex = 0;
            // 
            // MultipleSelectValueListParameterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._parameterName);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "MultipleSelectValueListParameterControl";
            this.Size = new System.Drawing.Size(330, 215);
            this._parameterName.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox _parameterName;
        private System.Windows.Forms.CheckedListBox parameterValuesListBox;
    }
}
