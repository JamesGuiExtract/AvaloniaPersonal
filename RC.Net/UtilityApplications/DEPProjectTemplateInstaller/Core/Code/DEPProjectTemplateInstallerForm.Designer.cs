namespace DEPProjectTemplateInstaller
{
    partial class DEPProjectTemplateInstallerForm
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
            this._templateNameTextBox = new System.Windows.Forms.TextBox();
            this._sourceProjectTextBox = new System.Windows.Forms.TextBox();
            this._blankRadioButton = new System.Windows.Forms.RadioButton();
            this._sourceRadioButton = new System.Windows.Forms.RadioButton();
            this._sourceProjectBrowseButton = new System.Windows.Forms.Button();
            this._createButton = new System.Windows.Forms.Button();
            this._labdeRadioButton = new System.Windows.Forms.RadioButton();
            this._flexIndexRadioButton = new System.Windows.Forms.RadioButton();
            label1 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 13);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(85, 13);
            label1.TabIndex = 0;
            label1.Text = "Template Name:";
            // 
            // _templateNameTextBox
            // 
            this._templateNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._templateNameTextBox.Location = new System.Drawing.Point(16, 29);
            this._templateNameTextBox.Name = "_templateNameTextBox";
            this._templateNameTextBox.Size = new System.Drawing.Size(446, 20);
            this._templateNameTextBox.TabIndex = 1;
            this._templateNameTextBox.Text = "DEP_Template";
            // 
            // _sourceProjectTextBox
            // 
            this._sourceProjectTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceProjectTextBox.Enabled = false;
            this._sourceProjectTextBox.Location = new System.Drawing.Point(16, 147);
            this._sourceProjectTextBox.Name = "_sourceProjectTextBox";
            this._sourceProjectTextBox.Size = new System.Drawing.Size(413, 20);
            this._sourceProjectTextBox.TabIndex = 5;
            // 
            // _blankRadioButton
            // 
            this._blankRadioButton.AutoSize = true;
            this._blankRadioButton.Checked = true;
            this._blankRadioButton.Location = new System.Drawing.Point(16, 101);
            this._blankRadioButton.Name = "_blankRadioButton";
            this._blankRadioButton.Size = new System.Drawing.Size(229, 17);
            this._blankRadioButton.TabIndex = 3;
            this._blankRadioButton.TabStop = true;
            this._blankRadioButton.Text = "New projects should start with a blank DEP";
            this._blankRadioButton.UseVisualStyleBackColor = true;
            // 
            // _sourceRadioButton
            // 
            this._sourceRadioButton.AutoSize = true;
            this._sourceRadioButton.Location = new System.Drawing.Point(16, 124);
            this._sourceRadioButton.Name = "_sourceRadioButton";
            this._sourceRadioButton.Size = new System.Drawing.Size(264, 17);
            this._sourceRadioButton.TabIndex = 4;
            this._sourceRadioButton.Text = "New projects should be based on the DEP project:";
            this._sourceRadioButton.UseVisualStyleBackColor = true;
            this._sourceRadioButton.CheckedChanged += new System.EventHandler(this.HandleSourceRadioButton_CheckedChanged);
            // 
            // _sourceProjectBrowseButton
            // 
            this._sourceProjectBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._sourceProjectBrowseButton.Enabled = false;
            this._sourceProjectBrowseButton.Location = new System.Drawing.Point(435, 146);
            this._sourceProjectBrowseButton.Name = "_sourceProjectBrowseButton";
            this._sourceProjectBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._sourceProjectBrowseButton.TabIndex = 6;
            this._sourceProjectBrowseButton.Text = "...";
            this._sourceProjectBrowseButton.UseVisualStyleBackColor = true;
            this._sourceProjectBrowseButton.Click += new System.EventHandler(this.HandleSourceProjectBrowseButton_Click);
            // 
            // _createButton
            // 
            this._createButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._createButton.Location = new System.Drawing.Point(340, 175);
            this._createButton.Name = "_createButton";
            this._createButton.Size = new System.Drawing.Size(122, 23);
            this._createButton.TabIndex = 7;
            this._createButton.Text = "Create Template";
            this._createButton.UseVisualStyleBackColor = true;
            this._createButton.Click += new System.EventHandler(this.HandleOkButton_Click);
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(this._flexIndexRadioButton);
            groupBox1.Controls.Add(this._labdeRadioButton);
            groupBox1.Location = new System.Drawing.Point(16, 56);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(446, 39);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Target product";
            // 
            // _labdeRadioButton
            // 
            this._labdeRadioButton.AutoSize = true;
            this._labdeRadioButton.Checked = true;
            this._labdeRadioButton.Location = new System.Drawing.Point(99, 15);
            this._labdeRadioButton.Name = "_labdeRadioButton";
            this._labdeRadioButton.Size = new System.Drawing.Size(58, 17);
            this._labdeRadioButton.TabIndex = 0;
            this._labdeRadioButton.TabStop = true;
            this._labdeRadioButton.Text = "LabDE";
            this._labdeRadioButton.UseVisualStyleBackColor = true;
            // 
            // _flexIndexRadioButton
            // 
            this._flexIndexRadioButton.AutoSize = true;
            this._flexIndexRadioButton.Location = new System.Drawing.Point(263, 15);
            this._flexIndexRadioButton.Name = "_flexIndexRadioButton";
            this._flexIndexRadioButton.Size = new System.Drawing.Size(80, 17);
            this._flexIndexRadioButton.TabIndex = 1;
            this._flexIndexRadioButton.Text = "FLEX Index";
            this._flexIndexRadioButton.UseVisualStyleBackColor = true;
            // 
            // DEPProjectTemplateInstallerForm
            // 
            this.AcceptButton = this._createButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 210);
            this.Controls.Add(groupBox1);
            this.Controls.Add(this._createButton);
            this.Controls.Add(this._sourceProjectBrowseButton);
            this.Controls.Add(this._sourceRadioButton);
            this.Controls.Add(this._blankRadioButton);
            this.Controls.Add(this._sourceProjectTextBox);
            this.Controls.Add(this._templateNameTextBox);
            this.Controls.Add(label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "DEPProjectTemplateInstallerForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DEP project template installer";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _templateNameTextBox;
        private System.Windows.Forms.TextBox _sourceProjectTextBox;
        private System.Windows.Forms.RadioButton _blankRadioButton;
        private System.Windows.Forms.RadioButton _sourceRadioButton;
        private System.Windows.Forms.Button _sourceProjectBrowseButton;
        private System.Windows.Forms.Button _createButton;
        private System.Windows.Forms.RadioButton _flexIndexRadioButton;
        private System.Windows.Forms.RadioButton _labdeRadioButton;
    }
}

