namespace Extract.AttributeFinder.Rules
{
    partial class DuplicateAndSeparateTreesSettingsDialog
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
            System.Windows.Forms.Label label1;
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this._dividingAttributeTextBox = new System.Windows.Forms.TextBox();
            this._attributeSelectorControl = new AttributeSelectorControl();
            label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(254, 13);
            label1.TabIndex = 0;
            label1.Text = "Select the attributes to be duplicated and separated:";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(269, 114);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 5;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(188, 114);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 4;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(202, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Specify the name of the dividing attribute:";
            // 
            // _dividingAttributeTextBox
            // 
            this._dividingAttributeTextBox.Location = new System.Drawing.Point(16, 78);
            this._dividingAttributeTextBox.Name = "_dividingAttributeTextBox";
            this._dividingAttributeTextBox.Size = new System.Drawing.Size(247, 20);
            this._dividingAttributeTextBox.TabIndex = 3;
            // 
            // _attributeSelectorControl
            // 
            this._attributeSelectorControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeSelectorControl.AttributeSelector = null;
            this._attributeSelectorControl.Location = new System.Drawing.Point(12, 25);
            this._attributeSelectorControl.Name = "_attributeSelectorControl";
            this._attributeSelectorControl.Size = new System.Drawing.Size(333, 30);
            this._attributeSelectorControl.TabIndex = 1;
            // 
            // DuplicateAndSeparateTreesSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(357, 149);
            this.Controls.Add(this._dividingAttributeTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(label1);
            this.Controls.Add(this._attributeSelectorControl);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DuplicateAndSeparateTreesSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Duplicate and separate attribute trees settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private AttributeSelectorControl _attributeSelectorControl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _dividingAttributeTextBox;
    }
}