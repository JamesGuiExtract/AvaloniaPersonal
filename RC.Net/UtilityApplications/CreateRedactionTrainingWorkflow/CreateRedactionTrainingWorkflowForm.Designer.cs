namespace VerifierWorkflowConfig
{
    partial class CreateRedactionTrainingWorkflowForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._loginTextBox = new System.Windows.Forms.TextBox();
            this._loginLabel = new System.Windows.Forms.Label();
            this._createButton = new System.Windows.Forms.Button();
            this._workflowLocationTextBox = new System.Windows.Forms.TextBox();
            this._workflowLocationLabel = new System.Windows.Forms.Label();
            this._dbServerLabel = new System.Windows.Forms.Label();
            this._dbServerTextBox = new System.Windows.Forms.TextBox();
            this._dbNameLabel = new System.Windows.Forms.Label();
            this._dbNameTextBox = new System.Windows.Forms.TextBox();
            this._setNameLabel = new System.Windows.Forms.Label();
            this._setNameTextBox = new System.Windows.Forms.TextBox();
            this._statusLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _loginTextBox
            // 
            this._loginTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._loginTextBox.Location = new System.Drawing.Point(10, 147);
            this._loginTextBox.Name = "_loginTextBox";
            this._loginTextBox.Size = new System.Drawing.Size(293, 20);
            this._loginTextBox.TabIndex = 3;
            this._loginTextBox.TextChanged += new System.EventHandler(this.HandleTextChanged);
            // 
            // _loginLabel
            // 
            this._loginLabel.AutoSize = true;
            this._loginLabel.Location = new System.Drawing.Point(10, 132);
            this._loginLabel.Name = "_loginLabel";
            this._loginLabel.Size = new System.Drawing.Size(101, 13);
            this._loginLabel.TabIndex = 1;
            this._loginLabel.Text = "Verifier domain login";
            // 
            // _createButton
            // 
            this._createButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._createButton.Location = new System.Drawing.Point(238, 272);
            this._createButton.Name = "_createButton";
            this._createButton.Size = new System.Drawing.Size(64, 20);
            this._createButton.TabIndex = 2;
            this._createButton.Text = "Create";
            this._createButton.UseVisualStyleBackColor = true;
            this._createButton.Click += new System.EventHandler(this.HandleCreateButton_Click);
            // 
            // _workflowLocationTextBox
            // 
            this._workflowLocationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._workflowLocationTextBox.Location = new System.Drawing.Point(10, 23);
            this._workflowLocationTextBox.Name = "_workflowLocationTextBox";
            this._workflowLocationTextBox.Size = new System.Drawing.Size(293, 20);
            this._workflowLocationTextBox.TabIndex = 0;
            this._workflowLocationTextBox.TabStop = false;
            // 
            // _workflowLocationLabel
            // 
            this._workflowLocationLabel.AutoSize = true;
            this._workflowLocationLabel.Location = new System.Drawing.Point(10, 8);
            this._workflowLocationLabel.Name = "_workflowLocationLabel";
            this._workflowLocationLabel.Size = new System.Drawing.Size(137, 13);
            this._workflowLocationLabel.TabIndex = 1;
            this._workflowLocationLabel.Text = "Training Workflow Location";
            // 
            // _dbServerLabel
            // 
            this._dbServerLabel.AutoSize = true;
            this._dbServerLabel.Location = new System.Drawing.Point(11, 49);
            this._dbServerLabel.Name = "_dbServerLabel";
            this._dbServerLabel.Size = new System.Drawing.Size(87, 13);
            this._dbServerLabel.TabIndex = 3;
            this._dbServerLabel.Text = "Database Server";
            // 
            // _dbServerTextBox
            // 
            this._dbServerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dbServerTextBox.Location = new System.Drawing.Point(11, 65);
            this._dbServerTextBox.Name = "_dbServerTextBox";
            this._dbServerTextBox.Size = new System.Drawing.Size(293, 20);
            this._dbServerTextBox.TabIndex = 1;
            this._dbServerTextBox.TabStop = false;
            // 
            // _dbNameLabel
            // 
            this._dbNameLabel.AutoSize = true;
            this._dbNameLabel.Location = new System.Drawing.Point(11, 91);
            this._dbNameLabel.Name = "_dbNameLabel";
            this._dbNameLabel.Size = new System.Drawing.Size(84, 13);
            this._dbNameLabel.TabIndex = 4;
            this._dbNameLabel.Text = "Database Name";
            // 
            // _dbNameTextBox
            // 
            this._dbNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dbNameTextBox.Location = new System.Drawing.Point(11, 107);
            this._dbNameTextBox.Name = "_dbNameTextBox";
            this._dbNameTextBox.Size = new System.Drawing.Size(293, 20);
            this._dbNameTextBox.TabIndex = 2;
            this._dbNameTextBox.TabStop = false;
            // 
            // _setNameLabel
            // 
            this._setNameLabel.AutoSize = true;
            this._setNameLabel.Location = new System.Drawing.Point(11, 173);
            this._setNameLabel.Name = "_setNameLabel";
            this._setNameLabel.Size = new System.Drawing.Size(54, 13);
            this._setNameLabel.TabIndex = 5;
            this._setNameLabel.Text = "Set Name";
            // 
            // _setNameTextBox
            // 
            this._setNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._setNameTextBox.Location = new System.Drawing.Point(10, 189);
            this._setNameTextBox.Name = "_setNameTextBox";
            this._setNameTextBox.Size = new System.Drawing.Size(293, 20);
            this._setNameTextBox.TabIndex = 4;
            this._setNameTextBox.TextChanged += new System.EventHandler(this.HandleTextChanged);
            // 
            // _statusLabel
            // 
            this._statusLabel.AutoSize = true;
            this._statusLabel.Location = new System.Drawing.Point(12, 226);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new System.Drawing.Size(0, 13);
            this._statusLabel.TabIndex = 6;
            // 
            // CreateRedactionTrainingWorkflowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(313, 302);
            this.Controls.Add(this._statusLabel);
            this.Controls.Add(this._setNameTextBox);
            this.Controls.Add(this._setNameLabel);
            this.Controls.Add(this._dbNameTextBox);
            this.Controls.Add(this._dbNameLabel);
            this.Controls.Add(this._dbServerTextBox);
            this.Controls.Add(this._dbServerLabel);
            this.Controls.Add(this._workflowLocationLabel);
            this.Controls.Add(this._workflowLocationTextBox);
            this.Controls.Add(this._createButton);
            this.Controls.Add(this._loginLabel);
            this.Controls.Add(this._loginTextBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateRedactionTrainingWorkflowForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Create Verifier Workflow";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _loginTextBox;
        private System.Windows.Forms.Button _createButton;
        private System.Windows.Forms.TextBox _workflowLocationTextBox;
        private System.Windows.Forms.Label _workflowLocationLabel;
        private System.Windows.Forms.Label _loginLabel;
        private System.Windows.Forms.Label _dbServerLabel;
        private System.Windows.Forms.TextBox _dbServerTextBox;
        private System.Windows.Forms.Label _dbNameLabel;
        private System.Windows.Forms.TextBox _dbNameTextBox;
        private System.Windows.Forms.Label _setNameLabel;
        private System.Windows.Forms.TextBox _setNameTextBox;
        private System.Windows.Forms.Label _statusLabel;
    }
}

