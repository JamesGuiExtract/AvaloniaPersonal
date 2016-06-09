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
            this.trainTestButton = new System.Windows.Forms.Button();
            this.testButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.recomputeFeaturesRadioButton = new System.Windows.Forms.RadioButton();
            this.useCurrentFeaturesRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // trainTestButton
            // 
            this.trainTestButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.trainTestButton.Location = new System.Drawing.Point(12, 569);
            this.trainTestButton.Name = "trainTestButton";
            this.trainTestButton.Size = new System.Drawing.Size(75, 23);
            this.trainTestButton.TabIndex = 0;
            this.trainTestButton.Text = "Train/Test";
            this.trainTestButton.UseVisualStyleBackColor = true;
            this.trainTestButton.Click += new System.EventHandler(this.HandleTrainAndTestButton_Click);
            // 
            // testButton
            // 
            this.testButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.testButton.Location = new System.Drawing.Point(93, 569);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(75, 23);
            this.testButton.TabIndex = 1;
            this.testButton.Text = "Test";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.HandleTestButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Location = new System.Drawing.Point(469, 569);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 2;
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
            this.groupBox1.Size = new System.Drawing.Size(532, 481);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Training/testing results";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(6, 19);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(520, 456);
            this.textBox1.TabIndex = 0;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cancelButton.Enabled = false;
            this.cancelButton.Location = new System.Drawing.Point(174, 569);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.HandleCancelButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.recomputeFeaturesRadioButton);
            this.groupBox2.Controls.Add(this.useCurrentFeaturesRadioButton);
            this.groupBox2.Location = new System.Drawing.Point(13, 493);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(531, 70);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            // 
            // recomputeFeaturesRadioButton
            // 
            this.recomputeFeaturesRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
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
            this.useCurrentFeaturesRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.useCurrentFeaturesRadioButton.AutoSize = true;
            this.useCurrentFeaturesRadioButton.Location = new System.Drawing.Point(6, 42);
            this.useCurrentFeaturesRadioButton.Name = "useCurrentFeaturesRadioButton";
            this.useCurrentFeaturesRadioButton.Size = new System.Drawing.Size(296, 17);
            this.useCurrentFeaturesRadioButton.TabIndex = 10;
            this.useCurrentFeaturesRadioButton.Text = "Use currently configured feature vectorizers when training";
            this.useCurrentFeaturesRadioButton.UseVisualStyleBackColor = true;
            // 
            // TrainingTesting
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(556, 604);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.trainTestButton);
            this.Name = "TrainingTesting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Training/Testing";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HandleTrainingTesting_FormClosing);
            this.Shown += new System.EventHandler(this.HandleTrainingTesting_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button trainTestButton;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton recomputeFeaturesRadioButton;
        private System.Windows.Forms.RadioButton useCurrentFeaturesRadioButton;
    }
}