namespace Extract.FileActionManager.Database
{
    partial class ApplyCounterUpdateForm
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
            this._cancelButton = new System.Windows.Forms.Button();
            this._updateCodeLabel = new System.Windows.Forms.Label();
            this._counterUpdateCodeTextBox = new System.Windows.Forms.TextBox();
            this._pasteCodeButton = new System.Windows.Forms.Button();
            this._applyButton = new System.Windows.Forms.Button();
            this._loadCodeFromFileButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(640, 140);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 5;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _updateCodeLabel
            // 
            this._updateCodeLabel.AutoSize = true;
            this._updateCodeLabel.Location = new System.Drawing.Point(12, 9);
            this._updateCodeLabel.Name = "_updateCodeLabel";
            this._updateCodeLabel.Size = new System.Drawing.Size(110, 13);
            this._updateCodeLabel.TabIndex = 0;
            this._updateCodeLabel.Text = "Counter update code:";
            // 
            // _counterUpdateCodeTextBox
            // 
            this._counterUpdateCodeTextBox.AllowDrop = true;
            this._counterUpdateCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._counterUpdateCodeTextBox.Location = new System.Drawing.Point(15, 27);
            this._counterUpdateCodeTextBox.Multiline = true;
            this._counterUpdateCodeTextBox.Name = "_counterUpdateCodeTextBox";
            this._counterUpdateCodeTextBox.ReadOnly = true;
            this._counterUpdateCodeTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._counterUpdateCodeTextBox.Size = new System.Drawing.Size(700, 107);
            this._counterUpdateCodeTextBox.TabIndex = 1;
            this._counterUpdateCodeTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.HandleCounterUpdateCodeTextBox_DragDrop);
            this._counterUpdateCodeTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.HandleCounterUpdateCodeTextBox_DragEnter);
            // 
            // _pasteCodeButton
            // 
            this._pasteCodeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._pasteCodeButton.Location = new System.Drawing.Point(15, 140);
            this._pasteCodeButton.Name = "_pasteCodeButton";
            this._pasteCodeButton.Size = new System.Drawing.Size(154, 23);
            this._pasteCodeButton.TabIndex = 2;
            this._pasteCodeButton.Text = "Paste code from clipboard";
            this._pasteCodeButton.UseVisualStyleBackColor = true;
            this._pasteCodeButton.Click += new System.EventHandler(this.HandlePasteCodeButton_Click);
            // 
            // _applyButton
            // 
            this._applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._applyButton.Enabled = false;
            this._applyButton.Location = new System.Drawing.Point(559, 140);
            this._applyButton.Name = "_applyButton";
            this._applyButton.Size = new System.Drawing.Size(75, 23);
            this._applyButton.TabIndex = 4;
            this._applyButton.Text = "Apply";
            this._applyButton.UseVisualStyleBackColor = true;
            this._applyButton.Click += new System.EventHandler(this.HandleApplyButton_Click);
            // 
            // _loadCodeFromFileButton
            // 
            this._loadCodeFromFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._loadCodeFromFileButton.Location = new System.Drawing.Point(175, 140);
            this._loadCodeFromFileButton.Name = "_loadCodeFromFileButton";
            this._loadCodeFromFileButton.Size = new System.Drawing.Size(154, 23);
            this._loadCodeFromFileButton.TabIndex = 3;
            this._loadCodeFromFileButton.Text = "Load code from file...";
            this._loadCodeFromFileButton.UseVisualStyleBackColor = true;
            this._loadCodeFromFileButton.Click += new System.EventHandler(this.HandleLoadCodeFromFileButton_Click);
            // 
            // ApplyCounterUpdateForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(727, 172);
            this.Controls.Add(this._loadCodeFromFileButton);
            this.Controls.Add(this._applyButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._updateCodeLabel);
            this.Controls.Add(this._counterUpdateCodeTextBox);
            this.Controls.Add(this._pasteCodeButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(550, 150);
            this.Name = "ApplyCounterUpdateForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Apply Counter Update Code";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label _updateCodeLabel;
        private System.Windows.Forms.TextBox _counterUpdateCodeTextBox;
        private System.Windows.Forms.Button _pasteCodeButton;
        private System.Windows.Forms.Button _applyButton;
        private System.Windows.Forms.Button _loadCodeFromFileButton;
    }
}