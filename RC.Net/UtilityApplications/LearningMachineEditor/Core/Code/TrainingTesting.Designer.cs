namespace Extract.UtilityApplications.LearningMachineEditor
{
    partial class TrainingTesting
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
                if (_cancellationTokenSource != null)
                {
                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Dispose();
                    }
                    if (_mainTask != null)
                    {
                        _mainTask.Dispose();
                    }
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
            this.trainTestButton = new System.Windows.Forms.Button();
            this.testButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.trainingLogContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.clearLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cancelButton = new System.Windows.Forms.Button();
            this.computeFeaturesGroupBox = new System.Windows.Forms.GroupBox();
            this.updateCsvsWithPredictionsCheckBox = new System.Windows.Forms.CheckBox();
            this.saveFeatureVectorsToCsvsCheckBox = new System.Windows.Forms.CheckBox();
            this.loadDataFromCsvRadioButton = new System.Windows.Forms.RadioButton();
            this.recomputeFeaturesRadioButton = new System.Windows.Forms.RadioButton();
            this.useCurrentFeaturesRadioButton = new System.Windows.Forms.RadioButton();
            this.detailsButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.trainingLogContextMenuStrip.SuspendLayout();
            this.computeFeaturesGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // trainTestButton
            // 
            this.trainTestButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.trainTestButton.Location = new System.Drawing.Point(12, 619);
            this.trainTestButton.Name = "trainTestButton";
            this.trainTestButton.Size = new System.Drawing.Size(75, 23);
            this.trainTestButton.TabIndex = 2;
            this.trainTestButton.Text = "Train/Test";
            this.trainTestButton.UseVisualStyleBackColor = true;
            this.trainTestButton.Click += new System.EventHandler(this.HandleTrainAndTestButton_Click);
            // 
            // testButton
            // 
            this.testButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.testButton.Location = new System.Drawing.Point(93, 619);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(75, 23);
            this.testButton.TabIndex = 3;
            this.testButton.Text = "Test";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.HandleTestButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Location = new System.Drawing.Point(469, 619);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 6;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(532, 479);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Training/testing results";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.BackColor = System.Drawing.SystemColors.Window;
            this.textBox1.ContextMenuStrip = this.trainingLogContextMenuStrip;
            this.textBox1.Location = new System.Drawing.Point(6, 19);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(520, 454);
            this.textBox1.TabIndex = 0;
            // 
            // trainingLogContextMenuStrip
            // 
            this.trainingLogContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.toolStripSeparator1,
            this.selectAllToolStripMenuItem,
            this.toolStripSeparator2,
            this.clearLogToolStripMenuItem});
            this.trainingLogContextMenuStrip.Name = "trainingLogContextMenuStrip";
            this.trainingLogContextMenuStrip.Size = new System.Drawing.Size(165, 82);
            this.trainingLogContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.HandleTrainingLogContextMenuStrip_Opening);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.HandleCopyToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(161, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.HandleSelectAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(161, 6);
            // 
            // clearLogToolStripMenuItem
            // 
            this.clearLogToolStripMenuItem.Name = "clearLogToolStripMenuItem";
            this.clearLogToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.clearLogToolStripMenuItem.Text = "Clear";
            this.clearLogToolStripMenuItem.Click += new System.EventHandler(this.HandleClearLogToolStripMenuItem_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cancelButton.Enabled = false;
            this.cancelButton.Location = new System.Drawing.Point(174, 619);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.HandleCancelButton_Click);
            // 
            // computeFeaturesGroupBox
            // 
            this.computeFeaturesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.computeFeaturesGroupBox.Controls.Add(this.updateCsvsWithPredictionsCheckBox);
            this.computeFeaturesGroupBox.Controls.Add(this.saveFeatureVectorsToCsvsCheckBox);
            this.computeFeaturesGroupBox.Controls.Add(this.loadDataFromCsvRadioButton);
            this.computeFeaturesGroupBox.Controls.Add(this.recomputeFeaturesRadioButton);
            this.computeFeaturesGroupBox.Controls.Add(this.useCurrentFeaturesRadioButton);
            this.computeFeaturesGroupBox.Location = new System.Drawing.Point(13, 491);
            this.computeFeaturesGroupBox.Name = "computeFeaturesGroupBox";
            this.computeFeaturesGroupBox.Size = new System.Drawing.Size(531, 114);
            this.computeFeaturesGroupBox.TabIndex = 1;
            this.computeFeaturesGroupBox.TabStop = false;
            // 
            // updateCsvsWithPredictionsCheckBox
            // 
            this.updateCsvsWithPredictionsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.updateCsvsWithPredictionsCheckBox.AutoSize = true;
            this.updateCsvsWithPredictionsCheckBox.Checked = true;
            this.updateCsvsWithPredictionsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.updateCsvsWithPredictionsCheckBox.Location = new System.Drawing.Point(299, 88);
            this.updateCsvsWithPredictionsCheckBox.Name = "updateCsvsWithPredictionsCheckBox";
            this.updateCsvsWithPredictionsCheckBox.Size = new System.Drawing.Size(226, 17);
            this.updateCsvsWithPredictionsCheckBox.TabIndex = 13;
            this.updateCsvsWithPredictionsCheckBox.Text = "Update CSVs with predictions/probabilities";
            this.updateCsvsWithPredictionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // saveFeatureVectorsToCsvsCheckBox
            // 
            this.saveFeatureVectorsToCsvsCheckBox.AutoSize = true;
            this.saveFeatureVectorsToCsvsCheckBox.Location = new System.Drawing.Point(6, 88);
            this.saveFeatureVectorsToCsvsCheckBox.Name = "saveFeatureVectorsToCsvsCheckBox";
            this.saveFeatureVectorsToCsvsCheckBox.Size = new System.Drawing.Size(166, 17);
            this.saveFeatureVectorsToCsvsCheckBox.TabIndex = 12;
            this.saveFeatureVectorsToCsvsCheckBox.Text = "Save feature vectors to CSVs";
            this.saveFeatureVectorsToCsvsCheckBox.UseVisualStyleBackColor = true;
            this.saveFeatureVectorsToCsvsCheckBox.CheckedChanged += new System.EventHandler(this.HandleSaveFeatureVectorsToCsvsCheckBox_CheckedChanged);
            // 
            // loadDataFromCsvRadioButton
            // 
            this.loadDataFromCsvRadioButton.AutoSize = true;
            this.loadDataFromCsvRadioButton.Location = new System.Drawing.Point(6, 65);
            this.loadDataFromCsvRadioButton.Name = "loadDataFromCsvRadioButton";
            this.loadDataFromCsvRadioButton.Size = new System.Drawing.Size(193, 17);
            this.loadDataFromCsvRadioButton.TabIndex = 11;
            this.loadDataFromCsvRadioButton.Text = "Load training/testing data from CSV";
            this.loadDataFromCsvRadioButton.UseVisualStyleBackColor = true;
            this.loadDataFromCsvRadioButton.CheckedChanged += new System.EventHandler(this.HandleLoadDataFromCsvRadioButton_CheckedChanged);
            // 
            // recomputeFeaturesRadioButton
            // 
            this.recomputeFeaturesRadioButton.AutoSize = true;
            this.recomputeFeaturesRadioButton.Checked = true;
            this.recomputeFeaturesRadioButton.Location = new System.Drawing.Point(6, 19);
            this.recomputeFeaturesRadioButton.Name = "recomputeFeaturesRadioButton";
            this.recomputeFeaturesRadioButton.Size = new System.Drawing.Size(309, 17);
            this.recomputeFeaturesRadioButton.TabIndex = 9;
            this.recomputeFeaturesRadioButton.TabStop = true;
            this.recomputeFeaturesRadioButton.Text = "Recompute feature vectorizers from input data when training";
            this.recomputeFeaturesRadioButton.UseVisualStyleBackColor = true;
            // 
            // useCurrentFeaturesRadioButton
            // 
            this.useCurrentFeaturesRadioButton.AutoSize = true;
            this.useCurrentFeaturesRadioButton.Location = new System.Drawing.Point(6, 42);
            this.useCurrentFeaturesRadioButton.Name = "useCurrentFeaturesRadioButton";
            this.useCurrentFeaturesRadioButton.Size = new System.Drawing.Size(296, 17);
            this.useCurrentFeaturesRadioButton.TabIndex = 10;
            this.useCurrentFeaturesRadioButton.Text = "Use currently configured feature vectorizers when training";
            this.useCurrentFeaturesRadioButton.UseVisualStyleBackColor = true;
            // 
            // detailsButton
            // 
            this.detailsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.detailsButton.Location = new System.Drawing.Point(318, 619);
            this.detailsButton.Name = "detailsButton";
            this.detailsButton.Size = new System.Drawing.Size(75, 23);
            this.detailsButton.TabIndex = 5;
            this.detailsButton.Text = "Details...";
            this.detailsButton.UseVisualStyleBackColor = true;
            this.detailsButton.Click += new System.EventHandler(this.HandleDetailsButton_Click);
            // 
            // TrainingTesting
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(556, 654);
            this.Controls.Add(this.detailsButton);
            this.Controls.Add(this.computeFeaturesGroupBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.trainTestButton);
            this.MinimumSize = new System.Drawing.Size(450, 300);
            this.Name = "TrainingTesting";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Training/Testing";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HandleTrainingTesting_FormClosing);
            this.Shown += new System.EventHandler(this.HandleTrainingTesting_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.trainingLogContextMenuStrip.ResumeLayout(false);
            this.computeFeaturesGroupBox.ResumeLayout(false);
            this.computeFeaturesGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button trainTestButton;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox computeFeaturesGroupBox;
        private System.Windows.Forms.RadioButton recomputeFeaturesRadioButton;
        private System.Windows.Forms.RadioButton useCurrentFeaturesRadioButton;
        private System.Windows.Forms.ContextMenuStrip trainingLogContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem clearLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.Button detailsButton;
        private System.Windows.Forms.RadioButton loadDataFromCsvRadioButton;
        private System.Windows.Forms.CheckBox saveFeatureVectorsToCsvsCheckBox;
        private System.Windows.Forms.CheckBox updateCsvsWithPredictionsCheckBox;
    }
}