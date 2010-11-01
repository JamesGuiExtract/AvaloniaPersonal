namespace Extract.FileActionManager.Utilities
{
    partial class AddModifyMachineForm
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
            System.Windows.Forms.Label labelMachineName;
            System.Windows.Forms.Label labelGroupName;
            this._textMachineName = new System.Windows.Forms.TextBox();
            this._browseMachineButton = new System.Windows.Forms.Button();
            this._groupNameCombo = new System.Windows.Forms.ComboBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            labelMachineName = new System.Windows.Forms.Label();
            labelGroupName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelMachineName
            // 
            labelMachineName.AutoSize = true;
            labelMachineName.Location = new System.Drawing.Point(12, 9);
            labelMachineName.Name = "labelMachineName";
            labelMachineName.Size = new System.Drawing.Size(77, 13);
            labelMachineName.TabIndex = 0;
            labelMachineName.Text = "Machine name";
            // 
            // labelGroupName
            // 
            labelGroupName.AutoSize = true;
            labelGroupName.Location = new System.Drawing.Point(12, 48);
            labelGroupName.Name = "labelGroupName";
            labelGroupName.Size = new System.Drawing.Size(65, 13);
            labelGroupName.TabIndex = 3;
            labelGroupName.Text = "Group name";
            // 
            // _textMachineName
            // 
            this._textMachineName.Location = new System.Drawing.Point(12, 25);
            this._textMachineName.Name = "_textMachineName";
            this._textMachineName.Size = new System.Drawing.Size(298, 20);
            this._textMachineName.TabIndex = 1;
            this._textMachineName.TextChanged += new System.EventHandler(this.HandleMachineNameTextChanged);
            // 
            // _browseMachineButton
            // 
            this._browseMachineButton.Enabled = false;
            this._browseMachineButton.Location = new System.Drawing.Point(316, 25);
            this._browseMachineButton.Name = "_browseMachineButton";
            this._browseMachineButton.Size = new System.Drawing.Size(27, 20);
            this._browseMachineButton.TabIndex = 2;
            this._browseMachineButton.Text = "...";
            this._browseMachineButton.UseVisualStyleBackColor = true;
            this._browseMachineButton.Click += new System.EventHandler(this.HandleBrowseMachineButtonClick);
            // 
            // _groupNameCombo
            // 
            this._groupNameCombo.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._groupNameCombo.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this._groupNameCombo.FormattingEnabled = true;
            this._groupNameCombo.Location = new System.Drawing.Point(12, 64);
            this._groupNameCombo.Name = "_groupNameCombo";
            this._groupNameCombo.Size = new System.Drawing.Size(331, 21);
            this._groupNameCombo.TabIndex = 4;
            this._groupNameCombo.TextChanged += new System.EventHandler(this.HandleGroupNameTextChanged);
            // 
            // _cancelButton
            // 
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(268, 91);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 6;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Location = new System.Drawing.Point(187, 92);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 5;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // AddModifyMachineForm
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(355, 124);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(labelGroupName);
            this.Controls.Add(this._groupNameCombo);
            this.Controls.Add(this._browseMachineButton);
            this.Controls.Add(this._textMachineName);
            this.Controls.Add(labelMachineName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddModifyMachineForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Machine";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _textMachineName;
        private System.Windows.Forms.Button _browseMachineButton;
        private System.Windows.Forms.ComboBox _groupNameCombo;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
    }
}