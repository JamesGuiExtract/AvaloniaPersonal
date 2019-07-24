namespace Extract.AttributeFinder.Rules
{
    partial class DuplicateAndSeparateTreesSettingsDialog
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.GroupBox groupBox1;
            this._runOutputHandlerCheckBox = new System.Windows.Forms.CheckBox();
            this._outputHandlerControl = new Utilities.Forms.ConfigurableObjectControl();
            this._dividingAttributeTextBox = new System.Windows.Forms.TextBox();
            this._attributeSelectorControl = new Utilities.Forms.ConfigurableObjectControl();
            this.label2 = new System.Windows.Forms.Label();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(10, 16);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(254, 13);
            label1.TabIndex = 0;
            label1.Text = "Select the attributes to be duplicated and separated:";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(this._runOutputHandlerCheckBox);
            groupBox1.Controls.Add(this._outputHandlerControl);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(this._dividingAttributeTextBox);
            groupBox1.Controls.Add(this._attributeSelectorControl);
            groupBox1.Controls.Add(this.label2);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(356, 212);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            // 
            // _runOutputHandlerCheckBox
            // 
            this._runOutputHandlerCheckBox.AutoSize = true;
            this._runOutputHandlerCheckBox.Location = new System.Drawing.Point(11, 142);
            this._runOutputHandlerCheckBox.Name = "_runOutputHandlerCheckBox";
            this._runOutputHandlerCheckBox.Size = new System.Drawing.Size(261, 17);
            this._runOutputHandlerCheckBox.TabIndex = 4;
            this._runOutputHandlerCheckBox.Text = "Run output handler on each tree after it is created";
            this._runOutputHandlerCheckBox.UseVisualStyleBackColor = true;
            this._runOutputHandlerCheckBox.CheckedChanged += new System.EventHandler(this.HandleRunOutputHandlerCheckedChanged);
            // 
            // _outputHandlerControl
            // 
            this._outputHandlerControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._outputHandlerControl.CategoryName = "UCLID AF-API Output Handlers";
            this._outputHandlerControl.ConfigurableObject = null;
            this._outputHandlerControl.Location = new System.Drawing.Point(6, 160);
            this._outputHandlerControl.Name = "_outputHandlerControl";
            this._outputHandlerControl.Size = new System.Drawing.Size(344, 49);
            this._outputHandlerControl.TabIndex = 5;
            // 
            // _dividingAttributeTextBox
            // 
            this._dividingAttributeTextBox.Location = new System.Drawing.Point(11, 104);
            this._dividingAttributeTextBox.Name = "_dividingAttributeTextBox";
            this._dividingAttributeTextBox.Size = new System.Drawing.Size(255, 20);
            this._dividingAttributeTextBox.TabIndex = 3;
            // 
            // _attributeSelectorControl
            // 
            this._attributeSelectorControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeSelectorControl.CategoryName = "UCLID AF-API Selectors";
            this._attributeSelectorControl.ConfigurableObject = null;
            this._attributeSelectorControl.Location = new System.Drawing.Point(5, 32);
            this._attributeSelectorControl.Name = "_attributeSelectorControl";
            this._attributeSelectorControl.Size = new System.Drawing.Size(345, 48);
            this._attributeSelectorControl.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(202, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Specify the name of the dividing attribute:";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(293, 233);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(212, 233);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 1;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // DuplicateAndSeparateTreesSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(380, 268);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DuplicateAndSeparateTreesSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Duplicate and separate attribute trees settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private Utilities.Forms.ConfigurableObjectControl _attributeSelectorControl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _dividingAttributeTextBox;
        private Utilities.Forms.ConfigurableObjectControl _outputHandlerControl;
        private System.Windows.Forms.CheckBox _runOutputHandlerCheckBox;
    }
}