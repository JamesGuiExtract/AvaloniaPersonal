using Extract.FileActionManager.Forms;

namespace Extract.UtilityApplications.NERDataCollector
{
    partial class NERDataCollectorConfigurationDialog
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._lastIDProcessedNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._annotatorSettingsPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._annotatorSettingsPathTextBox = new System.Windows.Forms.TextBox();
            this._attributeSetNameTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._modelNameTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._lastIDProcessedNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._lastIDProcessedNumericUpDown);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._annotatorSettingsPathBrowseButton);
            this.groupBox1.Controls.Add(this._attributeSetNameTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this._modelNameTextBox);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this._annotatorSettingsPathTextBox);
            this.groupBox1.Location = new System.Drawing.Point(7, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(602, 129);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // _lastIDProcessedNumericUpDown
            // 
            this._lastIDProcessedNumericUpDown.Location = new System.Drawing.Point(120, 98);
            this._lastIDProcessedNumericUpDown.Name = "_lastIDProcessedNumericUpDown";
            this._lastIDProcessedNumericUpDown.Size = new System.Drawing.Size(120, 20);
            this._lastIDProcessedNumericUpDown.TabIndex = 4;
            this._lastIDProcessedNumericUpDown.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Last ID processed";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Attribute set name";
            // 
            // _annotatorSettingsPathBrowseButton
            // 
            this._annotatorSettingsPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._annotatorSettingsPathBrowseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._annotatorSettingsPathBrowseButton.EnsureFileExists = false;
            this._annotatorSettingsPathBrowseButton.EnsurePathExists = false;
            this._annotatorSettingsPathBrowseButton.Location = new System.Drawing.Point(567, 70);
            this._annotatorSettingsPathBrowseButton.Name = "_annotatorSettingsPathBrowseButton";
            this._annotatorSettingsPathBrowseButton.Size = new System.Drawing.Size(29, 20);
            this._annotatorSettingsPathBrowseButton.TabIndex = 3;
            this._annotatorSettingsPathBrowseButton.Text = "...";
            this._annotatorSettingsPathBrowseButton.TextControl = this._annotatorSettingsPathTextBox;
            this._annotatorSettingsPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _annotatorSettingsPathTextBox
            // 
            this._annotatorSettingsPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._annotatorSettingsPathTextBox.Location = new System.Drawing.Point(120, 70);
            this._annotatorSettingsPathTextBox.Name = "_annotatorSettingsPathTextBox";
            this._annotatorSettingsPathTextBox.Size = new System.Drawing.Size(441, 20);
            this._annotatorSettingsPathTextBox.TabIndex = 2;
            this._annotatorSettingsPathTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _attributeSetNameTextBox
            // 
            this._attributeSetNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeSetNameTextBox.Location = new System.Drawing.Point(120, 18);
            this._attributeSetNameTextBox.Name = "_attributeSetNameTextBox";
            this._attributeSetNameTextBox.Size = new System.Drawing.Size(441, 20);
            this._attributeSetNameTextBox.TabIndex = 0;
            this._attributeSetNameTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Model name";
            // 
            // _modelNameTextBox
            // 
            this._modelNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._modelNameTextBox.Location = new System.Drawing.Point(120, 44);
            this._modelNameTextBox.Name = "_modelNameTextBox";
            this._modelNameTextBox.Size = new System.Drawing.Size(441, 20);
            this._modelNameTextBox.TabIndex = 1;
            this._modelNameTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(5, 73);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(108, 13);
            this.label9.TabIndex = 1;
            this.label9.Text = "Annotator settings file";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(483, 147);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(60, 23);
            this._okButton.TabIndex = 1;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(549, 147);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(60, 23);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            this._cancelButton.Click += new System.EventHandler(this.HandleCancelButton_Click);
            // 
            // NERDataCollectorConfigurationDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(616, 177);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(632, 216);
            this.Name = "NERDataCollectorConfigurationDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NER training data collector";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._lastIDProcessedNumericUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private Utilities.Forms.BrowseButton _annotatorSettingsPathBrowseButton;
        private System.Windows.Forms.TextBox _annotatorSettingsPathTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.TextBox _modelNameTextBox;
        private System.Windows.Forms.NumericUpDown _lastIDProcessedNumericUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _attributeSetNameTextBox;
    }
}