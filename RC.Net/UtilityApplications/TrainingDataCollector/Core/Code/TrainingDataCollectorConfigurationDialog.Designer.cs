using Extract.FileActionManager.Forms;

namespace Extract.UtilityApplications.TrainingDataCollector
{
    partial class TrainingDataCollectorConfigurationDialog
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

            try
            {
                _database?.CloseAllDBConnections();
            }
            catch { }
            _database = null;

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
            this._manageMLModelsButton = new System.Windows.Forms.Button();
            this._attributeSetNameComboBox = new System.Windows.Forms.ComboBox();
            this._addModelButton = new System.Windows.Forms.Button();
            this._modelNameComboBox = new System.Windows.Forms.ComboBox();
            this._lastIDProcessedNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._lmModelTypeRadioButton = new System.Windows.Forms.RadioButton();
            this._nerModelTypeRadioButton = new System.Windows.Forms.RadioButton();
            this._dataGeneratorPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._dataGeneratorPathTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._lastIDProcessedNumericUpDown)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._manageMLModelsButton);
            this.groupBox1.Controls.Add(this._attributeSetNameComboBox);
            this.groupBox1.Controls.Add(this._addModelButton);
            this.groupBox1.Controls.Add(this._modelNameComboBox);
            this.groupBox1.Controls.Add(this._lastIDProcessedNumericUpDown);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(7, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(470, 109);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // _manageMLModelsButton
            // 
            this._manageMLModelsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._manageMLModelsButton.Location = new System.Drawing.Point(401, 18);
            this._manageMLModelsButton.Name = "_manageMLModelsButton";
            this._manageMLModelsButton.Size = new System.Drawing.Size(63, 23);
            this._manageMLModelsButton.TabIndex = 2;
            this._manageMLModelsButton.Text = "Manage...";
            this._manageMLModelsButton.UseVisualStyleBackColor = true;
            this._manageMLModelsButton.Click += new System.EventHandler(this.Handle_ManageMLModelsButton_Click);
            // 
            // _attributeSetNameComboBox
            // 
            this._attributeSetNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeSetNameComboBox.FormattingEnabled = true;
            this._attributeSetNameComboBox.Location = new System.Drawing.Point(120, 46);
            this._attributeSetNameComboBox.Name = "_attributeSetNameComboBox";
            this._attributeSetNameComboBox.Size = new System.Drawing.Size(274, 21);
            this._attributeSetNameComboBox.TabIndex = 3;
            // 
            // _addModelButton
            // 
            this._addModelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._addModelButton.Location = new System.Drawing.Point(319, 18);
            this._addModelButton.Name = "_addModelButton";
            this._addModelButton.Size = new System.Drawing.Size(75, 23);
            this._addModelButton.TabIndex = 1;
            this._addModelButton.Text = "Add new...";
            this._addModelButton.UseVisualStyleBackColor = true;
            this._addModelButton.Click += new System.EventHandler(this.HandleAddModelButton_Click);
            // 
            // _modelNameComboBox
            // 
            this._modelNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._modelNameComboBox.FormattingEnabled = true;
            this._modelNameComboBox.Location = new System.Drawing.Point(120, 19);
            this._modelNameComboBox.Name = "_modelNameComboBox";
            this._modelNameComboBox.Size = new System.Drawing.Size(193, 21);
            this._modelNameComboBox.TabIndex = 0;
            // 
            // _lastIDProcessedNumericUpDown
            // 
            this._lastIDProcessedNumericUpDown.Location = new System.Drawing.Point(120, 74);
            this._lastIDProcessedNumericUpDown.Name = "_lastIDProcessedNumericUpDown";
            this._lastIDProcessedNumericUpDown.Size = new System.Drawing.Size(120, 20);
            this._lastIDProcessedNumericUpDown.TabIndex = 6;
            this._lastIDProcessedNumericUpDown.ValueChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 76);
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
            this.label1.Location = new System.Drawing.Point(5, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Attribute set name";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Model name";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(351, 214);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(60, 23);
            this._okButton.TabIndex = 2;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(417, 214);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(60, 23);
            this._cancelButton.TabIndex = 3;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            this._cancelButton.Click += new System.EventHandler(this.HandleCancelButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this._lmModelTypeRadioButton);
            this.groupBox2.Controls.Add(this._nerModelTypeRadioButton);
            this.groupBox2.Controls.Add(this._dataGeneratorPathBrowseButton);
            this.groupBox2.Controls.Add(this._dataGeneratorPathTextBox);
            this.groupBox2.Location = new System.Drawing.Point(7, 127);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(470, 73);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            // 
            // _lmModelTypeRadioButton
            // 
            this._lmModelTypeRadioButton.AutoSize = true;
            this._lmModelTypeRadioButton.Location = new System.Drawing.Point(218, 19);
            this._lmModelTypeRadioButton.Name = "_lmModelTypeRadioButton";
            this._lmModelTypeRadioButton.Size = new System.Drawing.Size(144, 17);
            this._lmModelTypeRadioButton.TabIndex = 1;
            this._lmModelTypeRadioButton.TabStop = true;
            this._lmModelTypeRadioButton.Text = "LM data encoder (.lm file)";
            this._lmModelTypeRadioButton.UseVisualStyleBackColor = true;
            // 
            // _nerModelTypeRadioButton
            // 
            this._nerModelTypeRadioButton.AutoSize = true;
            this._nerModelTypeRadioButton.Location = new System.Drawing.Point(11, 19);
            this._nerModelTypeRadioButton.Name = "_nerModelTypeRadioButton";
            this._nerModelTypeRadioButton.Size = new System.Drawing.Size(173, 17);
            this._nerModelTypeRadioButton.TabIndex = 0;
            this._nerModelTypeRadioButton.TabStop = true;
            this._nerModelTypeRadioButton.Text = "NER annotator (*.annotator file)";
            this._nerModelTypeRadioButton.UseVisualStyleBackColor = true;
            // 
            // _dataGeneratorPathBrowseButton
            // 
            this._dataGeneratorPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._dataGeneratorPathBrowseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._dataGeneratorPathBrowseButton.EnsureFileExists = false;
            this._dataGeneratorPathBrowseButton.EnsurePathExists = false;
            this._dataGeneratorPathBrowseButton.Location = new System.Drawing.Point(401, 42);
            this._dataGeneratorPathBrowseButton.Name = "_dataGeneratorPathBrowseButton";
            this._dataGeneratorPathBrowseButton.Size = new System.Drawing.Size(63, 20);
            this._dataGeneratorPathBrowseButton.TabIndex = 3;
            this._dataGeneratorPathBrowseButton.Text = "...";
            this._dataGeneratorPathBrowseButton.TextControl = this._dataGeneratorPathTextBox;
            this._dataGeneratorPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _dataGeneratorPathTextBox
            // 
            this._dataGeneratorPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataGeneratorPathTextBox.Location = new System.Drawing.Point(7, 42);
            this._dataGeneratorPathTextBox.Name = "_dataGeneratorPathTextBox";
            this._dataGeneratorPathTextBox.Size = new System.Drawing.Size(388, 20);
            this._dataGeneratorPathTextBox.TabIndex = 2;
            // 
            // TrainingDataCollectorConfigurationDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(484, 244);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1024, 283);
            this.MinimumSize = new System.Drawing.Size(500, 283);
            this.Name = "TrainingDataCollectorConfigurationDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Training data collector";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._lastIDProcessedNumericUpDown)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.NumericUpDown _lastIDProcessedNumericUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _addModelButton;
        private System.Windows.Forms.ComboBox _modelNameComboBox;
        private System.Windows.Forms.ComboBox _attributeSetNameComboBox;
        private System.Windows.Forms.Button _manageMLModelsButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton _lmModelTypeRadioButton;
        private System.Windows.Forms.RadioButton _nerModelTypeRadioButton;
        private Utilities.Forms.BrowseButton _dataGeneratorPathBrowseButton;
        private System.Windows.Forms.TextBox _dataGeneratorPathTextBox;
    }
}