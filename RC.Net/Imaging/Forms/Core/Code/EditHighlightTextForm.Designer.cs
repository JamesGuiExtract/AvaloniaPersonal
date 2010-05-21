namespace Extract.Imaging.Forms
{
    partial class EditHighlightTextForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="EditHighlightTextForm"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="EditHighlightTextForm"/>.
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
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._label = new System.Windows.Forms.Label();
            this._highlightTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(346, 288);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 0;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(427, 288);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 1;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _label
            // 
            this._label.AutoSize = true;
            this._label.Location = new System.Drawing.Point(12, 14);
            this._label.Name = "_label";
            this._label.Size = new System.Drawing.Size(68, 13);
            this._label.TabIndex = 2;
            this._label.Text = "Highlight text";
            // 
            // _highlightTextBox
            // 
            this._highlightTextBox.Location = new System.Drawing.Point(12, 30);
            this._highlightTextBox.Multiline = true;
            this._highlightTextBox.Name = "_highlightTextBox";
            this._highlightTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._highlightTextBox.Size = new System.Drawing.Size(490, 252);
            this._highlightTextBox.TabIndex = 3;
            // 
            // EditHighlightTextForm
            // 
            this.ClientSize = new System.Drawing.Size(514, 318);
            this.Controls.Add(this._highlightTextBox);
            this.Controls.Add(this._label);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "EditHighlightTextForm";
            this.Text = "Edit Highlight Text";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label _label;
        private System.Windows.Forms.TextBox _highlightTextBox;
    }
}
