namespace Extract.FileActionManager.Forms
{
    partial class PaginationSettingsDialog
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            Extract.Utilities.Forms.InfoTip infoTip4;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PaginationSettingsDialog));
            Extract.Utilities.Forms.InfoTip infoTip3;
            Extract.Utilities.Forms.InfoTip infoTip2;
            Extract.Utilities.Forms.InfoTip infoTip1;
            this._sourceActionComboBox = new System.Windows.Forms.ComboBox();
            this._outputActionComboBox = new System.Windows.Forms.ComboBox();
            this._outputPriorityComboBox = new System.Windows.Forms.ComboBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._outputPathPathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._outputPathTextBox = new Extract.Utilities.Forms.BetterTextBox();
            this._browsePdfFile = new Extract.Utilities.Forms.BrowseButton();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            infoTip4 = new Extract.Utilities.Forms.InfoTip();
            infoTip3 = new Extract.Utilities.Forms.InfoTip();
            infoTip2 = new Extract.Utilities.Forms.InfoTip();
            infoTip1 = new Extract.Utilities.Forms.InfoTip();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 13);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(162, 13);
            label1.TabIndex = 0;
            label1.Text = "Paginated document output path";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(13, 56);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(258, 13);
            label2.TabIndex = 4;
            label2.Text = "Set unverified source documents to pending in action";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(13, 102);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(346, 13);
            label3.TabIndex = 6;
            label3.Text = "Set manually specified paginated output documents to pending in action";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(13, 152);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(484, 13);
            label4.TabIndex = 8;
            label4.Text = "Set priority of manually paginated outputs document to the greater of the source " +
    "documents priority or:";
            // 
            // label5
            // 
            label5.Location = new System.Drawing.Point(13, 204);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(432, 39);
            label5.TabIndex = 10;
            label5.Text = "NOTE: OCR data (uss files) will automatically be generated for all paginated outp" +
    "ut documents using the OCR data of the source document(s)\r\n";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(13, 118);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(0, 13);
            label6.TabIndex = 7;
            // 
            // infoTip4
            // 
            infoTip4.BackColor = System.Drawing.Color.Transparent;
            infoTip4.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip4.BackgroundImage")));
            infoTip4.Location = new System.Drawing.Point(282, 56);
            infoTip4.Name = "infoTip4";
            infoTip4.Size = new System.Drawing.Size(16, 16);
            infoTip4.TabIndex = 14;
            infoTip4.TipText = resources.GetString("infoTip4.TipText");
            // 
            // infoTip3
            // 
            infoTip3.BackColor = System.Drawing.Color.Transparent;
            infoTip3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip3.BackgroundImage")));
            infoTip3.Location = new System.Drawing.Point(527, 152);
            infoTip3.Name = "infoTip3";
            infoTip3.Size = new System.Drawing.Size(16, 16);
            infoTip3.TabIndex = 16;
            infoTip3.TipText = resources.GetString("infoTip3.TipText");
            // 
            // infoTip2
            // 
            infoTip2.BackColor = System.Drawing.Color.Transparent;
            infoTip2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip2.BackgroundImage")));
            infoTip2.Location = new System.Drawing.Point(373, 102);
            infoTip2.Name = "infoTip2";
            infoTip2.Size = new System.Drawing.Size(16, 16);
            infoTip2.TabIndex = 15;
            infoTip2.TipText = resources.GetString("infoTip2.TipText");
            // 
            // infoTip1
            // 
            infoTip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            infoTip1.BackColor = System.Drawing.Color.Transparent;
            infoTip1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("infoTip1.BackgroundImage")));
            infoTip1.Location = new System.Drawing.Point(492, 6);
            infoTip1.Name = "infoTip1";
            infoTip1.Size = new System.Drawing.Size(16, 16);
            infoTip1.TabIndex = 13;
            infoTip1.TipText = resources.GetString("infoTip1.TipText");
            // 
            // _sourceActionComboBox
            // 
            this._sourceActionComboBox.FormattingEnabled = true;
            this._sourceActionComboBox.Location = new System.Drawing.Point(16, 73);
            this._sourceActionComboBox.Name = "_sourceActionComboBox";
            this._sourceActionComboBox.Size = new System.Drawing.Size(233, 21);
            this._sourceActionComboBox.TabIndex = 5;
            this._sourceActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _outputActionComboBox
            // 
            this._outputActionComboBox.FormattingEnabled = true;
            this._outputActionComboBox.Location = new System.Drawing.Point(16, 118);
            this._outputActionComboBox.Name = "_outputActionComboBox";
            this._outputActionComboBox.Size = new System.Drawing.Size(233, 21);
            this._outputActionComboBox.TabIndex = 5;
            this._outputActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _outputPriorityComboBox
            // 
            this._outputPriorityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._outputPriorityComboBox.FormattingEnabled = true;
            this._outputPriorityComboBox.Location = new System.Drawing.Point(16, 168);
            this._outputPriorityComboBox.Name = "_outputPriorityComboBox";
            this._outputPriorityComboBox.Size = new System.Drawing.Size(159, 21);
            this._outputPriorityComboBox.TabIndex = 9;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(468, 246);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 12;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(387, 246);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 11;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _outputPathPathTags
            // 
            this._outputPathPathTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathPathTags.Image = ((System.Drawing.Image)(resources.GetObject("_outputPathPathTags.Image")));
            this._outputPathPathTags.Location = new System.Drawing.Point(492, 28);
            this._outputPathPathTags.Name = "_outputPathPathTags";
            this._outputPathPathTags.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._outputPathPathTags.Size = new System.Drawing.Size(18, 21);
            this._outputPathPathTags.TabIndex = 2;
            this._outputPathPathTags.TextControl = this._outputPathTextBox;
            this._outputPathPathTags.UseVisualStyleBackColor = true;
            // 
            // _outputPathTextBox
            // 
            this._outputPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputPathTextBox.Location = new System.Drawing.Point(16, 29);
            this._outputPathTextBox.Name = "_outputPathTextBox";
            this._outputPathTextBox.Required = true;
            this._outputPathTextBox.Size = new System.Drawing.Size(470, 20);
            this._outputPathTextBox.TabIndex = 1;
            // 
            // _browsePdfFile
            // 
            this._browsePdfFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._browsePdfFile.Location = new System.Drawing.Point(516, 28);
            this._browsePdfFile.Name = "_browsePdfFile";
            this._browsePdfFile.Size = new System.Drawing.Size(27, 21);
            this._browsePdfFile.TabIndex = 3;
            this._browsePdfFile.Text = "...";
            this._browsePdfFile.TextControl = this._outputPathTextBox;
            this._browsePdfFile.UseVisualStyleBackColor = true;
            // 
            // PaginationSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(555, 281);
            this.Controls.Add(infoTip4);
            this.Controls.Add(infoTip3);
            this.Controls.Add(infoTip2);
            this.Controls.Add(infoTip1);
            this.Controls.Add(label6);
            this.Controls.Add(this._outputPathPathTags);
            this.Controls.Add(this._browsePdfFile);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(label5);
            this.Controls.Add(this._outputPriorityComboBox);
            this.Controls.Add(label4);
            this.Controls.Add(this._outputActionComboBox);
            this.Controls.Add(label3);
            this.Controls.Add(this._sourceActionComboBox);
            this.Controls.Add(label2);
            this.Controls.Add(this._outputPathTextBox);
            this.Controls.Add(label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(571, 319);
            this.Name = "PaginationSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pagination Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Extract.Utilities.Forms.BetterTextBox _outputPathTextBox;
        private System.Windows.Forms.ComboBox _sourceActionComboBox;
        private System.Windows.Forms.ComboBox _outputActionComboBox;
        private System.Windows.Forms.ComboBox _outputPriorityComboBox;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private FileActionManagerPathTagButton _outputPathPathTags;
        private Utilities.Forms.BrowseButton _browsePdfFile;
    }
}