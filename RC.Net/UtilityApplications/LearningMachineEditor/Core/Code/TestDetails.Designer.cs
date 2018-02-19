namespace Extract.UtilityApplications.LearningMachineEditor
{
    partial class TestDetails
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
            this.testDetailsDataGridView = new System.Windows.Forms.DataGridView();
            this.closeButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.normalizeByColumnsRadioButton = new System.Windows.Forms.RadioButton();
            this.normalizeByRowRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.showAccuracyForTrainingSetRadioButton = new System.Windows.Forms.RadioButton();
            this.showAccuracyForTestingSetRadioButton = new System.Windows.Forms.RadioButton();
            this.negativeClassesTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.scoreTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.testDetailsDataGridView)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // testDetailsDataGridView
            // 
            this.testDetailsDataGridView.AllowUserToAddRows = false;
            this.testDetailsDataGridView.AllowUserToDeleteRows = false;
            this.testDetailsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.testDetailsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader;
            this.testDetailsDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            this.testDetailsDataGridView.ColumnHeadersHeight = 150;
            this.testDetailsDataGridView.Location = new System.Drawing.Point(12, 76);
            this.testDetailsDataGridView.Name = "testDetailsDataGridView";
            this.testDetailsDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.testDetailsDataGridView.Size = new System.Drawing.Size(535, 535);
            this.testDetailsDataGridView.TabIndex = 0;
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(472, 659);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 1;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.normalizeByColumnsRadioButton);
            this.groupBox2.Controls.Add(this.normalizeByRowRadioButton);
            this.groupBox2.Location = new System.Drawing.Point(179, 617);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(161, 65);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Normalize colors by:";
            // 
            // normalizeByColumnsRadioButton
            // 
            this.normalizeByColumnsRadioButton.AutoSize = true;
            this.normalizeByColumnsRadioButton.Checked = true;
            this.normalizeByColumnsRadioButton.Location = new System.Drawing.Point(9, 19);
            this.normalizeByColumnsRadioButton.Name = "normalizeByColumnsRadioButton";
            this.normalizeByColumnsRadioButton.Size = new System.Drawing.Size(104, 17);
            this.normalizeByColumnsRadioButton.TabIndex = 1;
            this.normalizeByColumnsRadioButton.TabStop = true;
            this.normalizeByColumnsRadioButton.Text = "Columns (Found)";
            this.normalizeByColumnsRadioButton.UseVisualStyleBackColor = true;
            this.normalizeByColumnsRadioButton.CheckedChanged += new System.EventHandler(this.HandleNormalizeByColumnsRadioButton_CheckedChanged);
            // 
            // normalizeByRowRadioButton
            // 
            this.normalizeByRowRadioButton.AutoSize = true;
            this.normalizeByRowRadioButton.Location = new System.Drawing.Point(9, 42);
            this.normalizeByRowRadioButton.Name = "normalizeByRowRadioButton";
            this.normalizeByRowRadioButton.Size = new System.Drawing.Size(106, 17);
            this.normalizeByRowRadioButton.TabIndex = 0;
            this.normalizeByRowRadioButton.Text = "Rows (Expected)";
            this.normalizeByRowRadioButton.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox3.Controls.Add(this.showAccuracyForTrainingSetRadioButton);
            this.groupBox3.Controls.Add(this.showAccuracyForTestingSetRadioButton);
            this.groupBox3.Location = new System.Drawing.Point(12, 617);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(161, 65);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Show accuracy for:";
            // 
            // showAccuracyForTrainingSetRadioButton
            // 
            this.showAccuracyForTrainingSetRadioButton.AutoSize = true;
            this.showAccuracyForTrainingSetRadioButton.Location = new System.Drawing.Point(9, 19);
            this.showAccuracyForTrainingSetRadioButton.Name = "showAccuracyForTrainingSetRadioButton";
            this.showAccuracyForTrainingSetRadioButton.Size = new System.Drawing.Size(80, 17);
            this.showAccuracyForTrainingSetRadioButton.TabIndex = 1;
            this.showAccuracyForTrainingSetRadioButton.Text = "Training set";
            this.showAccuracyForTrainingSetRadioButton.UseVisualStyleBackColor = true;
            this.showAccuracyForTrainingSetRadioButton.CheckedChanged += new System.EventHandler(this.HandleShowAccuracyForTrainingSetRadioButton_CheckedChanged);
            // 
            // showAccuracyForTestingSetRadioButton
            // 
            this.showAccuracyForTestingSetRadioButton.AutoSize = true;
            this.showAccuracyForTestingSetRadioButton.Checked = true;
            this.showAccuracyForTestingSetRadioButton.Location = new System.Drawing.Point(9, 42);
            this.showAccuracyForTestingSetRadioButton.Name = "showAccuracyForTestingSetRadioButton";
            this.showAccuracyForTestingSetRadioButton.Size = new System.Drawing.Size(77, 17);
            this.showAccuracyForTestingSetRadioButton.TabIndex = 0;
            this.showAccuracyForTestingSetRadioButton.TabStop = true;
            this.showAccuracyForTestingSetRadioButton.Text = "Testing set";
            this.showAccuracyForTestingSetRadioButton.UseVisualStyleBackColor = true;
            // 
            // negativeClassesTextBox
            // 
            this.negativeClassesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.negativeClassesTextBox.Location = new System.Drawing.Point(12, 26);
            this.negativeClassesTextBox.Name = "negativeClassesTextBox";
            this.negativeClassesTextBox.Size = new System.Drawing.Size(535, 20);
            this.negativeClassesTextBox.TabIndex = 4;
            this.negativeClassesTextBox.Leave += new System.EventHandler(this.HandleNegativeClassesTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Negative class(es) (CSV):";
            // 
            // scoreTextBox
            // 
            this.scoreTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scoreTextBox.Location = new System.Drawing.Point(12, 52);
            this.scoreTextBox.Name = "scoreTextBox";
            this.scoreTextBox.ReadOnly = true;
            this.scoreTextBox.Size = new System.Drawing.Size(535, 20);
            this.scoreTextBox.TabIndex = 6;
            // 
            // TestDetails
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(559, 700);
            this.Controls.Add(this.scoreTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.negativeClassesTextBox);
            this.Controls.Add(this.testDetailsDataGridView);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.closeButton);
            this.MinimumSize = new System.Drawing.Size(451, 551);
            this.Name = "TestDetails";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Test Details";
            ((System.ComponentModel.ISupportInitialize)(this.testDetailsDataGridView)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView testDetailsDataGridView;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton normalizeByColumnsRadioButton;
        private System.Windows.Forms.RadioButton normalizeByRowRadioButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton showAccuracyForTrainingSetRadioButton;
        private System.Windows.Forms.RadioButton showAccuracyForTestingSetRadioButton;
        private System.Windows.Forms.TextBox negativeClassesTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox scoreTextBox;
    }
}