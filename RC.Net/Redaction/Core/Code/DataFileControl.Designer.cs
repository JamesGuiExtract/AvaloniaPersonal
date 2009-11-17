namespace Extract.Redaction
{
    sealed partial class DataFileControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="DataFileControl"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="DataFileControl"/>.
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
            System.Windows.Forms.GroupBox groupBox1;
            this._customizeButton = new System.Windows.Forms.Button();
            this._label = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._customizeButton);
            groupBox1.Controls.Add(this._label);
            groupBox1.Location = new System.Drawing.Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(420, 60);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "ID Shield data file location";
            // 
            // _customizeButton
            // 
            this._customizeButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this._customizeButton.Location = new System.Drawing.Point(339, 22);
            this._customizeButton.Name = "_customizeButton";
            this._customizeButton.Size = new System.Drawing.Size(75, 23);
            this._customizeButton.TabIndex = 1;
            this._customizeButton.Text = "Customize...";
            this._customizeButton.UseVisualStyleBackColor = true;
            this._customizeButton.Click += new System.EventHandler(this.HandleCustomizeButtonClick);
            // 
            // _label
            // 
            this._label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._label.AutoSize = true;
            this._label.Location = new System.Drawing.Point(7, 27);
            this._label.Name = "_label";
            this._label.Size = new System.Drawing.Size(146, 13);
            this._label.TabIndex = 0;
            this._label.Text = "You are using default settings";
            // 
            // DataFileControl
            // 
            this.Controls.Add(groupBox1);
            this.Name = "DataFileControl";
            this.Size = new System.Drawing.Size(420, 60);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _customizeButton;
        private System.Windows.Forms.Label _label;
    }
}
