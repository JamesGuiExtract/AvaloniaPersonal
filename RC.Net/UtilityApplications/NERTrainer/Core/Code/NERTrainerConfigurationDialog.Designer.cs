using Extract.FileActionManager.Forms;

namespace Extract.UtilityApplications.NERTrainer
{
    partial class NERTrainerConfigurationDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NERTrainerConfigurationDialog));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._addModelButton = new System.Windows.Forms.Button();
            this._modelNameComboBox = new System.Windows.Forms.ComboBox();
            this._modelDestinationPathBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._modelDestinationPathTextBox = new System.Windows.Forms.TextBox();
            this._modelDestinationPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this.label3 = new System.Windows.Forms.Label();
            this._trainingCommandPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._trainingCommandTextBox = new System.Windows.Forms.TextBox();
            this._testingCommandPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._testingCommandTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._updateCommandTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._addModelButton);
            this.groupBox1.Controls.Add(this._modelNameComboBox);
            this.groupBox1.Controls.Add(this._modelDestinationPathBrowseButton);
            this.groupBox1.Controls.Add(this._modelDestinationPathTagsButton);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this._modelDestinationPathTextBox);
            this.groupBox1.Controls.Add(this._trainingCommandPathTagsButton);
            this.groupBox1.Controls.Add(this._testingCommandPathTagsButton);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._trainingCommandTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this._testingCommandTextBox);
            this.groupBox1.Location = new System.Drawing.Point(7, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(631, 129);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // _addModelButton
            // 
            this._addModelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._addModelButton.Location = new System.Drawing.Point(493, 18);
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
            this._modelNameComboBox.Size = new System.Drawing.Size(367, 21);
            this._modelNameComboBox.TabIndex = 0;
            // 
            // _modelDestinationPathBrowseButton
            // 
            this._modelDestinationPathBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._modelDestinationPathBrowseButton.EnsureFileExists = false;
            this._modelDestinationPathBrowseButton.EnsurePathExists = false;
            this._modelDestinationPathBrowseButton.Location = new System.Drawing.Point(597, 98);
            this._modelDestinationPathBrowseButton.Name = "_modelDestinationPathBrowseButton";
            this._modelDestinationPathBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._modelDestinationPathBrowseButton.TabIndex = 8;
            this._modelDestinationPathBrowseButton.Text = "...";
            this._modelDestinationPathBrowseButton.TextControl = this._modelDestinationPathTextBox;
            this._modelDestinationPathBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _modelDestinationPathTextBox
            // 
            this._modelDestinationPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._modelDestinationPathTextBox.Location = new System.Drawing.Point(120, 98);
            this._modelDestinationPathTextBox.Name = "_modelDestinationPathTextBox";
            this._modelDestinationPathTextBox.Size = new System.Drawing.Size(448, 20);
            this._modelDestinationPathTextBox.TabIndex = 6;
            // 
            // _modelDestinationPathTagsButton
            // 
            this._modelDestinationPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._modelDestinationPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_modelDestinationPathTagsButton.Image")));
            this._modelDestinationPathTagsButton.Location = new System.Drawing.Point(575, 98);
            this._modelDestinationPathTagsButton.Name = "_modelDestinationPathTagsButton";
            this._modelDestinationPathTagsButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this._modelDestinationPathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._modelDestinationPathTagsButton.TabIndex = 7;
            this._modelDestinationPathTagsButton.TextControl = this._modelDestinationPathTextBox;
            this._modelDestinationPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Destination path";
            // 
            // _trainingCommandPathTagsButton
            // 
            this._trainingCommandPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._trainingCommandPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_trainingCommandPathTagsButton.Image")));
            this._trainingCommandPathTagsButton.Location = new System.Drawing.Point(575, 46);
            this._trainingCommandPathTagsButton.Name = "_trainingCommandPathTagsButton";
            this._trainingCommandPathTagsButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this._trainingCommandPathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._trainingCommandPathTagsButton.TabIndex = 3;
            this._trainingCommandPathTagsButton.TextControl = this._trainingCommandTextBox;
            this._trainingCommandPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _trainingCommandTextBox
            // 
            this._trainingCommandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._trainingCommandTextBox.Location = new System.Drawing.Point(120, 46);
            this._trainingCommandTextBox.Name = "_trainingCommandTextBox";
            this._trainingCommandTextBox.Size = new System.Drawing.Size(448, 20);
            this._trainingCommandTextBox.TabIndex = 2;
            this._trainingCommandTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // _testingCommandPathTagsButton
            // 
            this._testingCommandPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._testingCommandPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_testingCommandPathTagsButton.Image")));
            this._testingCommandPathTagsButton.Location = new System.Drawing.Point(575, 72);
            this._testingCommandPathTagsButton.Name = "_testingCommandPathTagsButton";
            this._testingCommandPathTagsButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this._testingCommandPathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._testingCommandPathTagsButton.TabIndex = 5;
            this._testingCommandPathTagsButton.TextControl = this._testingCommandTextBox;
            this._testingCommandPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _testingCommandTextBox
            // 
            this._testingCommandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._testingCommandTextBox.Location = new System.Drawing.Point(120, 72);
            this._testingCommandTextBox.Name = "_testingCommandTextBox";
            this._testingCommandTextBox.Size = new System.Drawing.Size(448, 20);
            this._testingCommandTextBox.TabIndex = 4;
            this._testingCommandTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Model name";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Training command";
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(5, 74);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(91, 13);
            this.label9.TabIndex = 1;
            this.label9.Text = "Testing command";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(512, 147);
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
            this._cancelButton.Location = new System.Drawing.Point(578, 147);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(60, 23);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            this._cancelButton.Click += new System.EventHandler(this.HandleCancelButton_Click);
            // 
            // _updateCommandTextBox
            // 
            this._updateCommandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._updateCommandTextBox.Location = new System.Drawing.Point(120, 97);
            this._updateCommandTextBox.Name = "_updateCommandTextBox";
            this._updateCommandTextBox.Size = new System.Drawing.Size(448, 20);
            this._updateCommandTextBox.TabIndex = 7;
            // 
            // NERTrainerConfigurationDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(645, 177);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(632, 216);
            this.Name = "NERTrainerConfigurationDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NER trainer";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _testingCommandTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _trainingCommandTextBox;
        private Utilities.Forms.PathTagsButton _trainingCommandPathTagsButton;
        private Utilities.Forms.PathTagsButton _testingCommandPathTagsButton;
        private Utilities.Forms.PathTagsButton _modelDestinationPathTagsButton;
        private System.Windows.Forms.TextBox _modelDestinationPathTextBox;
        private System.Windows.Forms.Label label3;
        private Utilities.Forms.BrowseButton _modelDestinationPathBrowseButton;
        private System.Windows.Forms.TextBox _updateCommandTextBox;
        private System.Windows.Forms.ComboBox _modelNameComboBox;
        private System.Windows.Forms.Button _addModelButton;
    }
}