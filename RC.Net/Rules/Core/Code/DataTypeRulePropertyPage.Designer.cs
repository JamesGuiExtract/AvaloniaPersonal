namespace Extract.Rules
{
    partial class DataTypeRulePropertyPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="DataTypeRulePropertyPage"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="DataTypeRulePropertyPage"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources
                if (components != null)
                {
                    components.Dispose();
                }
            }

            // Release unmanaged resources

            // Call base dispose method
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._dataTypesGroupBox = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // _dataTypesGroupBox
            // 
            this._dataTypesGroupBox.AutoSize = true;
            this._dataTypesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dataTypesGroupBox.Location = new System.Drawing.Point(0, 0);
            this._dataTypesGroupBox.Name = "_dataTypesGroupBox";
            this._dataTypesGroupBox.Size = new System.Drawing.Size(200, 20);
            this._dataTypesGroupBox.TabIndex = 0;
            this._dataTypesGroupBox.TabStop = false;
            this._dataTypesGroupBox.Text = "Select data types to redact";
            // 
            // DataTypeRulePropertyPage
            // 
            this.AutoSize = true;
            this.Controls.Add(this._dataTypesGroupBox);
            this.MinimumSize = new System.Drawing.Size(200, 20);
            this.Name = "DataTypeRulePropertyPage";
            this.Size = new System.Drawing.Size(200, 20);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox _dataTypesGroupBox;


    }
}
