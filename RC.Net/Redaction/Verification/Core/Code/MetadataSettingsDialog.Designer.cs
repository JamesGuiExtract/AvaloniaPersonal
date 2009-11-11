namespace Extract.Redaction.Verification
{
	partial class MetadataSettingsDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <overloads>Releases resources used by the <see cref="MetadataSettingsDialog"/>.</overloads>
		/// <summary>
		/// Releases all unmanaged resources used by the <see cref="MetadataSettingsDialog"/>.
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.Label label1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MetadataSettingsDialog));
            this._metadataPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._metadataBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._metadataFileTextBox = new System.Windows.Forms.TextBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(this._metadataPathTagsButton);
            groupBox1.Controls.Add(this._metadataBrowseButton);
            groupBox1.Controls.Add(this._metadataFileTextBox);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(368, 72);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "Output";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(7, 20);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(130, 13);
            label1.TabIndex = 5;
            label1.Text = "Metadata output file name";
            // 
            // _metadataPathTagsButton
            // 
            this._metadataPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_metadataPathTagsButton.Image")));
            this._metadataPathTagsButton.Location = new System.Drawing.Point(310, 40);
            this._metadataPathTagsButton.Name = "_metadataPathTagsButton";
            this._metadataPathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._metadataPathTagsButton.TabIndex = 4;
            this._metadataPathTagsButton.UseVisualStyleBackColor = true;
            this._metadataPathTagsButton.TagSelected += new System.EventHandler<Extract.Utilities.Forms.TagSelectedEventArgs>(this.HandleMetadataPathTagsButtonTagSelected);
            // 
            // _metadataBrowseButton
            // 
            this._metadataBrowseButton.Location = new System.Drawing.Point(334, 40);
            this._metadataBrowseButton.Name = "_metadataBrowseButton";
            this._metadataBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._metadataBrowseButton.TabIndex = 3;
            this._metadataBrowseButton.Text = "...";
            this._metadataBrowseButton.UseVisualStyleBackColor = true;
            this._metadataBrowseButton.PathSelected += new System.EventHandler<Extract.Utilities.Forms.PathSelectedEventArgs>(this.HandleMetadataBrowseButtonPathSelected);
            // 
            // _metadataFileTextBox
            // 
            this._metadataFileTextBox.Location = new System.Drawing.Point(10, 40);
            this._metadataFileTextBox.Name = "_metadataFileTextBox";
            this._metadataFileTextBox.Size = new System.Drawing.Size(294, 20);
            this._metadataFileTextBox.TabIndex = 2;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(307, 96);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 1;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(226, 96);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 0;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // MetadataSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(395, 131);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MetadataSettingsDialog";
            this.ShowIcon = false;
            this.Text = "Create redaction metadata";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private Extract.Utilities.Forms.PathTagsButton _metadataPathTagsButton;
        private Extract.Utilities.Forms.BrowseButton _metadataBrowseButton;
        private System.Windows.Forms.TextBox _metadataFileTextBox;
	}
}