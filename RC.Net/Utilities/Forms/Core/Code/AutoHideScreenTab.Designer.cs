namespace Extract.Utilities.Forms
{
    partial class AutoHideScreenTab
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._linkLabel = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // _linkLabel
            // 
            this._linkLabel.ActiveLinkColor = System.Drawing.Color.White;
            this._linkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._linkLabel.BackColor = System.Drawing.Color.Transparent;
            this._linkLabel.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._linkLabel.LinkColor = System.Drawing.Color.LightSteelBlue;
            this._linkLabel.Location = new System.Drawing.Point(0, 0);
            this._linkLabel.Name = "_linkLabel";
            this._linkLabel.Size = new System.Drawing.Size(237, 50);
            this._linkLabel.TabIndex = 0;
            this._linkLabel.TabStop = true;
            this._linkLabel.Text = "Label";
            this._linkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AutoHideScreenTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.ClientSize = new System.Drawing.Size(225, 45);
            this.ControlBox = false;
            this.Controls.Add(this._linkLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "AutoHideScreenTab";
            this.Opacity = 0.8D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "AutoHideTab";
            this.TransparencyKey = System.Drawing.SystemColors.InactiveCaptionText;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.LinkLabel _linkLabel;
    }
}