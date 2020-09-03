namespace Extract.ReportingDevExpress
{
    partial class ReportProgressForm
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
                components?.Dispose();
                components = null;
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
            this.progressBarControl = new DevExpress.XtraEditors.MarqueeProgressBarControl();
            this.textReportName = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBarControl
            // 
            this.progressBarControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarControl.CausesValidation = false;
            this.progressBarControl.EditValue = 0;
            this.progressBarControl.Location = new System.Drawing.Point(13, 29);
            this.progressBarControl.Name = "progressBarControl";
            this.progressBarControl.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Style3D;
            this.progressBarControl.Properties.ProgressViewStyle = DevExpress.XtraEditors.Controls.ProgressViewStyle.Solid;
            this.progressBarControl.Size = new System.Drawing.Size(542, 34);
            this.progressBarControl.TabIndex = 0;
            // 
            // textReportName
            // 
            this.textReportName.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Style3D;
            this.textReportName.Location = new System.Drawing.Point(13, 6);
            this.textReportName.MinimumSize = new System.Drawing.Size(542, 0);
            this.textReportName.Name = "textReportName";
            this.textReportName.Size = new System.Drawing.Size(542, 17);
            this.textReportName.TabIndex = 2;
            this.textReportName.Text = "labelControl1";
            // 
            // ReportProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(566, 76);
            this.Controls.Add(this.textReportName);
            this.Controls.Add(this.progressBarControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.IconOptions.ShowIcon = false;
            this.MaximizeBox = false;
            this.Name = "ReportProgressForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generating report";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReportProgressForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.MarqueeProgressBarControl progressBarControl;
        private DevExpress.XtraEditors.LabelControl textReportName;
    }
}