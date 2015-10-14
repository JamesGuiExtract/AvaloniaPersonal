namespace Extract.FAMDBCounterManager
{
    partial class EmailForm
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
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._editableBodyTextBox = new System.Windows.Forms.TextBox();
            this._subjectTextBox = new System.Windows.Forms.TextBox();
            this._carbonCopyRecipientTextBox = new System.Windows.Forms.TextBox();
            this._recipientTextBox = new System.Windows.Forms.TextBox();
            this._flexLicenseEmailTextBox = new System.Windows.Forms.TextBox();
            this._ccTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._bodyTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._readOnlyBodyTextBox = new System.Windows.Forms.TextBox();
            this._bodyPanel = new System.Windows.Forms.Panel();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            this._ccTableLayoutPanel.SuspendLayout();
            this._bodyTableLayoutPanel.SuspendLayout();
            this._bodyPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 15);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(20, 13);
            label1.TabIndex = 0;
            label1.Text = "To";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(13, 41);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(20, 13);
            label2.TabIndex = 2;
            label2.Text = "Cc";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(13, 67);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(43, 13);
            label3.TabIndex = 5;
            label3.Text = "Subject";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(495, 404);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 9;
            this._okButton.Text = "Send Email";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(576, 404);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 10;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _editableBodyTextBox
            // 
            this._editableBodyTextBox.AcceptsReturn = true;
            this._editableBodyTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._editableBodyTextBox.Location = new System.Drawing.Point(0, 0);
            this._editableBodyTextBox.Margin = new System.Windows.Forms.Padding(0);
            this._editableBodyTextBox.MinimumSize = new System.Drawing.Size(4, 45);
            this._editableBodyTextBox.Multiline = true;
            this._editableBodyTextBox.Name = "_editableBodyTextBox";
            this._editableBodyTextBox.Size = new System.Drawing.Size(635, 45);
            this._editableBodyTextBox.TabIndex = 7;
            this._editableBodyTextBox.TextChanged += new System.EventHandler(this.HandleEditableBodyTextBox_TextChanged);
            // 
            // _subjectTextBox
            // 
            this._subjectTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._subjectTextBox.Location = new System.Drawing.Point(62, 64);
            this._subjectTextBox.Name = "_subjectTextBox";
            this._subjectTextBox.Size = new System.Drawing.Size(589, 20);
            this._subjectTextBox.TabIndex = 6;
            // 
            // _carbonCopyRecipientTextBox
            // 
            this._carbonCopyRecipientTextBox.AllowDrop = true;
            this._carbonCopyRecipientTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._carbonCopyRecipientTextBox.Location = new System.Drawing.Point(60, 0);
            this._carbonCopyRecipientTextBox.Margin = new System.Windows.Forms.Padding(0);
            this._carbonCopyRecipientTextBox.Name = "_carbonCopyRecipientTextBox";
            this._carbonCopyRecipientTextBox.Size = new System.Drawing.Size(529, 20);
            this._carbonCopyRecipientTextBox.TabIndex = 4;
            this._carbonCopyRecipientTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.HandleRecipient_DragDrop);
            this._carbonCopyRecipientTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.HandleRecipient_DragEnter);
            // 
            // _recipientTextBox
            // 
            this._recipientTextBox.AllowDrop = true;
            this._recipientTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._recipientTextBox.Location = new System.Drawing.Point(62, 12);
            this._recipientTextBox.Name = "_recipientTextBox";
            this._recipientTextBox.Size = new System.Drawing.Size(589, 20);
            this._recipientTextBox.TabIndex = 1;
            this._recipientTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.HandleRecipient_DragDrop);
            this._recipientTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.HandleRecipient_DragEnter);
            // 
            // _flexLicenseEmailTextBox
            // 
            this._flexLicenseEmailTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._flexLicenseEmailTextBox.Location = new System.Drawing.Point(0, 0);
            this._flexLicenseEmailTextBox.Margin = new System.Windows.Forms.Padding(0);
            this._flexLicenseEmailTextBox.Name = "_flexLicenseEmailTextBox";
            this._flexLicenseEmailTextBox.ReadOnly = true;
            this._flexLicenseEmailTextBox.Size = new System.Drawing.Size(60, 20);
            this._flexLicenseEmailTextBox.TabIndex = 3;
            this._flexLicenseEmailTextBox.Text = "flex-license;";
            this._flexLicenseEmailTextBox.TextChanged += new System.EventHandler(this.HandleFlexLicenseEmailTextBox_TextChanged);
            // 
            // _ccTableLayoutPanel
            // 
            this._ccTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._ccTableLayoutPanel.ColumnCount = 2;
            this._ccTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this._ccTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._ccTableLayoutPanel.Controls.Add(this._flexLicenseEmailTextBox, 0, 0);
            this._ccTableLayoutPanel.Controls.Add(this._carbonCopyRecipientTextBox, 1, 0);
            this._ccTableLayoutPanel.Location = new System.Drawing.Point(62, 36);
            this._ccTableLayoutPanel.Name = "_ccTableLayoutPanel";
            this._ccTableLayoutPanel.RowCount = 1;
            this._ccTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._ccTableLayoutPanel.Size = new System.Drawing.Size(589, 20);
            this._ccTableLayoutPanel.TabIndex = 3;
            // 
            // _bodyTableLayoutPanel
            // 
            this._bodyTableLayoutPanel.AutoSize = true;
            this._bodyTableLayoutPanel.ColumnCount = 1;
            this._bodyTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._bodyTableLayoutPanel.Controls.Add(this._readOnlyBodyTextBox, 0, 1);
            this._bodyTableLayoutPanel.Controls.Add(this._editableBodyTextBox, 0, 0);
            this._bodyTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._bodyTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._bodyTableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this._bodyTableLayoutPanel.Name = "_bodyTableLayoutPanel";
            this._bodyTableLayoutPanel.RowCount = 2;
            this._bodyTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this._bodyTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this._bodyTableLayoutPanel.Size = new System.Drawing.Size(635, 245);
            this._bodyTableLayoutPanel.TabIndex = 7;
            // 
            // _readOnlyBodyTextBox
            // 
            this._readOnlyBodyTextBox.AcceptsReturn = true;
            this._readOnlyBodyTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._readOnlyBodyTextBox.Location = new System.Drawing.Point(0, 45);
            this._readOnlyBodyTextBox.Margin = new System.Windows.Forms.Padding(0);
            this._readOnlyBodyTextBox.Multiline = true;
            this._readOnlyBodyTextBox.Name = "_readOnlyBodyTextBox";
            this._readOnlyBodyTextBox.ReadOnly = true;
            this._readOnlyBodyTextBox.Size = new System.Drawing.Size(635, 200);
            this._readOnlyBodyTextBox.TabIndex = 8;
            this._readOnlyBodyTextBox.TextChanged += new System.EventHandler(this.HandleEditableBodyTextBox_TextChanged);
            // 
            // _bodyPanel
            // 
            this._bodyPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._bodyPanel.AutoScroll = true;
            this._bodyPanel.Controls.Add(this._bodyTableLayoutPanel);
            this._bodyPanel.Location = new System.Drawing.Point(16, 90);
            this._bodyPanel.Name = "_bodyPanel";
            this._bodyPanel.Size = new System.Drawing.Size(635, 308);
            this._bodyPanel.TabIndex = 18;
            this._bodyPanel.SizeChanged += new System.EventHandler(this.HandleBodyPanel_SizeChanged);
            // 
            // EmailForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(663, 439);
            this.Controls.Add(this._bodyPanel);
            this.Controls.Add(this._ccTableLayoutPanel);
            this.Controls.Add(this._recipientTextBox);
            this.Controls.Add(this._subjectTextBox);
            this.Controls.Add(label3);
            this.Controls.Add(label2);
            this.Controls.Add(label1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 350);
            this.Name = "EmailForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "License email";
            this._ccTableLayoutPanel.ResumeLayout(false);
            this._ccTableLayoutPanel.PerformLayout();
            this._bodyTableLayoutPanel.ResumeLayout(false);
            this._bodyTableLayoutPanel.PerformLayout();
            this._bodyPanel.ResumeLayout(false);
            this._bodyPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.TextBox _editableBodyTextBox;
        private System.Windows.Forms.TextBox _subjectTextBox;
        private System.Windows.Forms.TextBox _carbonCopyRecipientTextBox;
        private System.Windows.Forms.TextBox _recipientTextBox;
        private System.Windows.Forms.TextBox _flexLicenseEmailTextBox;
        private System.Windows.Forms.TableLayoutPanel _ccTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel _bodyTableLayoutPanel;
        private System.Windows.Forms.Panel _bodyPanel;
        private System.Windows.Forms.TextBox _readOnlyBodyTextBox;
    }
}