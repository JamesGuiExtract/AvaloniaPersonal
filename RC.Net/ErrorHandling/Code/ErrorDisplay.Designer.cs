namespace Extract.ErrorHandling
{
    partial class ErrorDisplay
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorDisplay));
            this._okButton = new System.Windows.Forms.Button();
            this._detailsButton = new System.Windows.Forms.Button();
            this._saveButton = new System.Windows.Forms.Button();
            this._buttonGroupTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._labelErrorMessage = new System.Windows.Forms.Label();
            this._tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this._buttonGroupTableLayoutPanel.SuspendLayout();
            this._tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._okButton.Location = new System.Drawing.Point(385, 9);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 21);
            this._okButton.TabIndex = 24;
            this._okButton.Text = "Ok";
            // 
            // _detailsButton
            // 
            this._detailsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._detailsButton.Location = new System.Drawing.Point(298, 9);
            this._detailsButton.Name = "_detailsButton";
            this._detailsButton.Size = new System.Drawing.Size(75, 21);
            this._detailsButton.TabIndex = 25;
            this._detailsButton.Text = "Details...";
            this._detailsButton.Click += new System.EventHandler(this.HandleDetailsClick);
            // 
            // _saveButton
            // 
            this._saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._saveButton.Location = new System.Drawing.Point(211, 9);
            this._saveButton.Name = "_saveButton";
            this._saveButton.Size = new System.Drawing.Size(75, 21);
            this._saveButton.TabIndex = 26;
            this._saveButton.Text = "Save As";
            // 
            // _buttonGroupTableLayoutPanel
            // 
            this._buttonGroupTableLayoutPanel.ColumnCount = 4;
            this._buttonGroupTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._buttonGroupTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 87F));
            this._buttonGroupTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 87F));
            this._buttonGroupTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 87F));
            this._buttonGroupTableLayoutPanel.Controls.Add(this._okButton, 3, 0);
            this._buttonGroupTableLayoutPanel.Controls.Add(this._detailsButton, 2, 0);
            this._buttonGroupTableLayoutPanel.Controls.Add(this._saveButton, 1, 0);
            this._buttonGroupTableLayoutPanel.Location = new System.Drawing.Point(106, 98);
            this._buttonGroupTableLayoutPanel.Name = "_buttonGroupTableLayoutPanel";
            this._buttonGroupTableLayoutPanel.RowCount = 1;
            this._buttonGroupTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._buttonGroupTableLayoutPanel.Size = new System.Drawing.Size(463, 33);
            this._buttonGroupTableLayoutPanel.TabIndex = 0;
            // 
            // _labelErrorMessage
            // 
            this._labelErrorMessage.AutoSize = true;
            this._labelErrorMessage.Location = new System.Drawing.Point(106, 3);
            this._labelErrorMessage.Margin = new System.Windows.Forms.Padding(3);
            this._labelErrorMessage.MaximumSize = new System.Drawing.Size(0, 17);
            this._labelErrorMessage.Name = "_labelErrorMessage";
            this._labelErrorMessage.Size = new System.Drawing.Size(75, 13);
            this._labelErrorMessage.TabIndex = 19;
            this._labelErrorMessage.Text = "Error Message";
            this._labelErrorMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _tableLayoutPanel
            // 
            this._tableLayoutPanel.ColumnCount = 2;
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 103F));
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this._tableLayoutPanel.Controls.Add(this.pictureBox1, 0, 0);
            this._tableLayoutPanel.Controls.Add(this._labelErrorMessage, 1, 0);
            this._tableLayoutPanel.Controls.Add(this._buttonGroupTableLayoutPanel, 1, 1);
            this._tableLayoutPanel.Location = new System.Drawing.Point(9, 9);
            this._tableLayoutPanel.Name = "_tableLayoutPanel";
            this._tableLayoutPanel.RowCount = 2;
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 95F));
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 84.86842F));
            this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this._tableLayoutPanel.Size = new System.Drawing.Size(572, 134);
            this._tableLayoutPanel.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(97, 89);
            this.pictureBox1.TabIndex = 25;
            this.pictureBox1.TabStop = false;
            // 
            // ErrorDisplay
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 145);
            this.Controls.Add(this._tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ErrorDisplay";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Error";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ErrorDisplay_FormClosing);
            this._buttonGroupTableLayoutPanel.ResumeLayout(false);
            this._tableLayoutPanel.ResumeLayout(false);
            this._tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _detailsButton;
        private System.Windows.Forms.Button _saveButton;
        private System.Windows.Forms.TableLayoutPanel _buttonGroupTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel _tableLayoutPanel;
        private System.Windows.Forms.Label _labelErrorMessage;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
