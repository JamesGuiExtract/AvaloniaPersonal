namespace Extract.Utilities.Forms
{
    partial class ProgressStatusDialogForm
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
            if (disposing)
            {
                _timer.Stop();

                if (_labels != null)
                {
                    _labels.Clear();
                    _labels = null;
                }
                if (_progressBars != null)
                {
                    _progressBars.Clear();
                    _progressBars = null;
                }
                lock (_lock)
                {
                    _progressStatus = null;
                }

                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
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
            this._topPanel = new System.Windows.Forms.Panel();
            this._timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // _topPanel
            // 
            this._topPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._topPanel.AutoScroll = true;
            this._topPanel.Location = new System.Drawing.Point(12, 12);
            this._topPanel.Name = "_topPanel";
            this._topPanel.Size = new System.Drawing.Size(353, 12);
            this._topPanel.TabIndex = 0;
            // 
            // _timer
            // 
            this._timer.Tick += new System.EventHandler(this.HandleTimerTick);
            // 
            // ProgressStatusDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(377, 36);
            this.ControlBox = false;
            this.Controls.Add(this._topPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressStatusDialogForm";
            this.ShowInTaskbar = false;
            this.Text = "Progress Status";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _topPanel;
        private System.Windows.Forms.Timer _timer;

    }
}