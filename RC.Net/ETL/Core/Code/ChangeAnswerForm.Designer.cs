namespace Extract.ETL
{
    partial class ChangeAnswerForm
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
            this._changeButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._oldAnswerTextBox = new System.Windows.Forms.TextBox();
            this._newAnswerTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _changeButton
            // 
            this._changeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._changeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._changeButton.Location = new System.Drawing.Point(197, 106);
            this._changeButton.Name = "_changeButton";
            this._changeButton.Size = new System.Drawing.Size(75, 23);
            this._changeButton.TabIndex = 2;
            this._changeButton.Text = "Change";
            this._changeButton.UseVisualStyleBackColor = true;
            this._changeButton.Click += new System.EventHandler(this.HandleChangeButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(278, 106);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 3;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _oldAnswerTextBox
            // 
            this._oldAnswerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._oldAnswerTextBox.Location = new System.Drawing.Point(12, 29);
            this._oldAnswerTextBox.Name = "_oldAnswerTextBox";
            this._oldAnswerTextBox.Size = new System.Drawing.Size(341, 20);
            this._oldAnswerTextBox.TabIndex = 0;
            // 
            // _newAnswerTextBox
            // 
            this._newAnswerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._newAnswerTextBox.Location = new System.Drawing.Point(12, 78);
            this._newAnswerTextBox.Name = "_newAnswerTextBox";
            this._newAnswerTextBox.Size = new System.Drawing.Size(341, 20);
            this._newAnswerTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Old answer";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "New answer";
            // 
            // ChangeAnswerDialog
            // 
            this.AcceptButton = this._changeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(365, 141);
            this.ControlBox = false;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._newAnswerTextBox);
            this.Controls.Add(this._oldAnswerTextBox);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._changeButton);
            this.MinimumSize = new System.Drawing.Size(197, 180);
            this.Name = "ChangeAnswerDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Change answer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _changeButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.TextBox _oldAnswerTextBox;
        private System.Windows.Forms.TextBox _newAnswerTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}