namespace Extract.Utilities.Forms
{
    public partial class SelectFromListInputBox
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
            this.cancel = new System.Windows.Forms.Button();
            this.label = new System.Windows.Forms.Label();
            this.ok = new System.Windows.Forms.Button();
            this.comboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cancel
            // 
            this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(493, 64);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(64, 24);
            this.cancel.TabIndex = 3;
            this.cancel.Text = "Cancel";
            // 
            // label
            // 
            this.label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label.Location = new System.Drawing.Point(12, 9);
            this.label.MaximumSize = new System.Drawing.Size(545, 19);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(545, 19);
            this.label.TabIndex = 6;
            this.label.Text = "InputBox";
            // 
            // ok
            // 
            this.ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ok.Location = new System.Drawing.Point(423, 64);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(64, 24);
            this.ok.TabIndex = 2;
            this.ok.Text = "OK";
            this.ok.Click += new System.EventHandler(this.HandleOKClick);
            // 
            // comboBox
            // 
            this.comboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox.FormattingEnabled = true;
            this.comboBox.Location = new System.Drawing.Point(15, 32);
            this.comboBox.Name = "comboBox";
            this.comboBox.Size = new System.Drawing.Size(542, 21);
            this.comboBox.TabIndex = 1;
            // 
            // SelectFromListInputBox
            // 
            this.AcceptButton = this.ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(569, 100);
            this.Controls.Add(this.comboBox);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.label);
            this.Controls.Add(this.ok);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1200, 139);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(585, 139);
            this.Name = "SelectFromListInputBox";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select From List Form";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Label label;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.ComboBox comboBox;
    }
}