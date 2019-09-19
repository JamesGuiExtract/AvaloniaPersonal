namespace Extract.AttributeFinder.Rules
{
    partial class MoveCopyAttributesSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MoveCopyAttributesSettingsDialog));
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this._sourceXPathTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._destinationXPathtextBox = new System.Windows.Forms.TextBox();
            this._moveRadioButton = new System.Windows.Forms.RadioButton();
            this._copyRadioButton = new System.Windows.Forms.RadioButton();
            this._moveCopyGroupBox = new System.Windows.Forms.GroupBox();
            this.infoTip1 = new Extract.Utilities.Forms.InfoTip();
            this._moveCopyGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(377, 238);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(87, 23);
            this._okButton.TabIndex = 5;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(479, 238);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(87, 23);
            this._cancelButton.TabIndex = 6;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "XPath for source attributes";
            // 
            // _sourceXPathTextBox
            // 
            this._sourceXPathTextBox.Location = new System.Drawing.Point(16, 30);
            this._sourceXPathTextBox.Multiline = true;
            this._sourceXPathTextBox.Name = "_sourceXPathTextBox";
            this._sourceXPathTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._sourceXPathTextBox.Size = new System.Drawing.Size(550, 64);
            this._sourceXPathTextBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 106);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(297, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "XPath for destination (copy/move attributes to children of this)";
            // 
            // _destinationXPathtextBox
            // 
            this._destinationXPathtextBox.Location = new System.Drawing.Point(16, 123);
            this._destinationXPathtextBox.Multiline = true;
            this._destinationXPathtextBox.Name = "_destinationXPathtextBox";
            this._destinationXPathtextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._destinationXPathtextBox.Size = new System.Drawing.Size(550, 64);
            this._destinationXPathtextBox.TabIndex = 2;
            // 
            // _moveRadioButton
            // 
            this._moveRadioButton.AutoSize = true;
            this._moveRadioButton.Location = new System.Drawing.Point(6, 16);
            this._moveRadioButton.Name = "_moveRadioButton";
            this._moveRadioButton.Size = new System.Drawing.Size(99, 17);
            this._moveRadioButton.TabIndex = 3;
            this._moveRadioButton.TabStop = true;
            this._moveRadioButton.Text = "Move Attributes";
            this._moveRadioButton.UseVisualStyleBackColor = true;
            // 
            // _copyRadioButton
            // 
            this._copyRadioButton.AutoSize = true;
            this._copyRadioButton.Location = new System.Drawing.Point(6, 39);
            this._copyRadioButton.Name = "_copyRadioButton";
            this._copyRadioButton.Size = new System.Drawing.Size(95, 17);
            this._copyRadioButton.TabIndex = 4;
            this._copyRadioButton.TabStop = true;
            this._copyRadioButton.Text = "Copy attributes";
            this._copyRadioButton.UseVisualStyleBackColor = true;
            // 
            // _moveCopyGroupBox
            // 
            this._moveCopyGroupBox.Controls.Add(this._moveRadioButton);
            this._moveCopyGroupBox.Controls.Add(this._copyRadioButton);
            this._moveCopyGroupBox.Location = new System.Drawing.Point(16, 193);
            this._moveCopyGroupBox.Name = "_moveCopyGroupBox";
            this._moveCopyGroupBox.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this._moveCopyGroupBox.Size = new System.Drawing.Size(200, 68);
            this._moveCopyGroupBox.TabIndex = 23;
            this._moveCopyGroupBox.TabStop = false;
            // 
            // infoTip1
            // 
            this.infoTip1.BackColor = System.Drawing.Color.Transparent;
            this.infoTip1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip1.BackgroundImage")));
            this.infoTip1.Location = new System.Drawing.Point(331, 100);
            this.infoTip1.Name = "infoTip1";
            this.infoTip1.Size = new System.Drawing.Size(16, 16);
            this.infoTip1.TabIndex = 24;
            this.infoTip1.TipText = "This XPath can be absolute (start with /) or based on the source node.\n" +
                "E.g., could use \'Sub\' to copy into a child named \'Sub\' or \'../Other\' to copy into a sibling named \'Other\'";
            // 
            // MoveCopyAttributesSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(584, 273);
            this.Controls.Add(this.infoTip1);
            this.Controls.Add(this._moveCopyGroupBox);
            this.Controls.Add(this._destinationXPathtextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._sourceXPathTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(600, 312);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 312);
            this.Name = "MoveCopyAttributesSettingsDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Move/Copy attributes settings";
            this._moveCopyGroupBox.ResumeLayout(false);
            this._moveCopyGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _sourceXPathTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _destinationXPathtextBox;
        private System.Windows.Forms.RadioButton _moveRadioButton;
        private System.Windows.Forms.RadioButton _copyRadioButton;
        private System.Windows.Forms.GroupBox _moveCopyGroupBox;
        private Utilities.Forms.InfoTip infoTip1;
    }
}