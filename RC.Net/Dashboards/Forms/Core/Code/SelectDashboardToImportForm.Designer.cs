namespace Extract.Dashboard.Forms
{
    partial class SelectDashboardToImportForm
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
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this._okButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this._dashboardNameTextBox = new System.Windows.Forms.TextBox();
            this._browseButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this._dashboardFileTextBox = new System.Windows.Forms.TextBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "esdx";
            this.openFileDialog.Filter = "ESDX|*.esdx|All|*.*";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(390, 68);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 4;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOKButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Dashboard name";
            // 
            // _dashboardNameTextBox
            // 
            this._dashboardNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._dashboardNameTextBox.Location = new System.Drawing.Point(107, 38);
            this._dashboardNameTextBox.Name = "_dashboardNameTextBox";
            this._dashboardNameTextBox.Size = new System.Drawing.Size(402, 20);
            this._dashboardNameTextBox.TabIndex = 2;
            // 
            // _browseButton
            // 
            this._browseButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this._browseButton.Location = new System.Drawing.Point(515, 10);
            this._browseButton.Name = "_browseButton";
            this._browseButton.Size = new System.Drawing.Size(32, 23);
            this._browseButton.TabIndex = 1;
            this._browseButton.Text = "...";
            this._browseButton.UseVisualStyleBackColor = true;
            this._browseButton.Click += new System.EventHandler(this.HandleBrowseButtonClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Dashboard file";
            // 
            // _dashboardFileTextBox
            // 
            this._dashboardFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._dashboardFileTextBox.Location = new System.Drawing.Point(107, 12);
            this._dashboardFileTextBox.Name = "_dashboardFileTextBox";
            this._dashboardFileTextBox.Size = new System.Drawing.Size(402, 20);
            this._dashboardFileTextBox.TabIndex = 0;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(472, 68);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 5;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // SelectDashboardToImportForm
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(559, 103);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._browseButton);
            this.Controls.Add(this._dashboardFileTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._dashboardNameTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._okButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(575, 142);
            this.Name = "SelectDashboardToImportForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Dashboard to import";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _dashboardNameTextBox;
        private System.Windows.Forms.Button _browseButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _dashboardFileTextBox;
        private System.Windows.Forms.Button _cancelButton;
    }
}