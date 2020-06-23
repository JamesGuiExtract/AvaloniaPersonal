namespace Extract.Reporting
{
    partial class ParameterEntryForm
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
            this._btnOk = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this._topPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // _btnOk
            // 
            this._btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this._btnOk.Location = new System.Drawing.Point(238, 30);
            this._btnOk.Name = "_btnOk";
            this._btnOk.Size = new System.Drawing.Size(75, 23);
            this._btnOk.TabIndex = 0;
            this._btnOk.Text = "OK";
            this._btnOk.UseVisualStyleBackColor = true;
            this._btnOk.Click += new System.EventHandler(this.HandleOkClicked);
            // 
            // _btnCancel
            // 
            this._btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Location = new System.Drawing.Point(319, 30);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 23);
            this._btnCancel.TabIndex = 1;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _topPanel
            // 
            this._topPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._topPanel.AutoScroll = true;
            this._topPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._topPanel.Location = new System.Drawing.Point(12, 12);
            this._topPanel.Margin = new System.Windows.Forms.Padding(5);
            this._topPanel.Name = "_topPanel";
            this._topPanel.Size = new System.Drawing.Size(379, 10);
            this._topPanel.TabIndex = 2;
            // 
            // ParameterEntryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(403, 65);
            this.Controls.Add(this._topPanel);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MinimumSize = new System.Drawing.Size(409, 97);
            this.Name = "ParameterEntryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Parameter Entry Form";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Button _btnOk;
        private System.Windows.Forms.Panel _topPanel;
    }
}