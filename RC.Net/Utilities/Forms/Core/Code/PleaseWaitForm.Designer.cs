namespace Extract.Utilities.Forms
{
    partial class PleaseWaitForm
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
            this._labelMessage = new System.Windows.Forms.Label();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // _labelMessage
            // 
            this._labelMessage.AutoSize = true;
            this._labelMessage.Location = new System.Drawing.Point(12, 9);
            this._labelMessage.Name = "_labelMessage";
            this._labelMessage.Size = new System.Drawing.Size(35, 13);
            this._labelMessage.TabIndex = 0;
            this._labelMessage.Text = "label1";
            // 
            // _progressBar
            // 
            this._progressBar.Location = new System.Drawing.Point(12, 25);
            this._progressBar.Name = "_progressBar";
            this._progressBar.MarqueeAnimationSpeed = 50;
            this._progressBar.Size = new System.Drawing.Size(370, 25);
            this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this._progressBar.TabIndex = 1;
            // 
            // PleaseWaitForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 62);
            this.ControlBox = false;
            this.Controls.Add(this._progressBar);
            this.Controls.Add(this._labelMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PleaseWaitForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Please Wait";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _labelMessage;
        private System.Windows.Forms.ProgressBar _progressBar;
    }
}