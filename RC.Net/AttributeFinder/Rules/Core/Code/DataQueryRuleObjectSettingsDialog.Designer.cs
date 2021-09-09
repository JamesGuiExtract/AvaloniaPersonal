namespace Extract.AttributeFinder.Rules
{
    partial class DataQueryRuleObjectSettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Panel panel1;
            this._queryScintillaBox = new ScintillaNET.Scintilla();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._testQueryButton = new System.Windows.Forms.Button();
            this._databaseConnectionControl = new Extract.Database.DatabaseConnectionControl();
            this._useFAMDBCheckBox = new System.Windows.Forms.CheckBox();
            this._useSpecifiedDBCheckBox = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._queryScintillaBox)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(10, 11);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(59, 13);
            label1.TabIndex = 0;
            label1.Text = "Data query";
            // 
            // panel1
            // 
            panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            panel1.Controls.Add(this._queryScintillaBox);
            panel1.Location = new System.Drawing.Point(13, 28);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(825, 481);
            panel1.TabIndex = 19;
            // 
            // _queryScintillaBox
            // 
            this._queryScintillaBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._queryScintillaBox.Annotations.Visibility = ScintillaNET.AnnotationsVisibility.Standard;
            this._queryScintillaBox.ConfigurationManager.Language = "xml";
            this._queryScintillaBox.Indentation.ShowGuides = true;
            this._queryScintillaBox.Indentation.SmartIndentType = ScintillaNET.SmartIndent.Simple;
            this._queryScintillaBox.Indentation.TabWidth = 4;
            this._queryScintillaBox.IsBraceMatching = true;
            this._queryScintillaBox.LineWrap.Mode = ScintillaNET.WrapMode.Word;
            this._queryScintillaBox.LineWrap.VisualFlags = ScintillaNET.WrapVisualFlag.Start;
            this._queryScintillaBox.LineWrap.VisualFlagsLocation = ScintillaNET.WrapVisualLocation.StartByText;
            this._queryScintillaBox.Location = new System.Drawing.Point(0, 0);
            this._queryScintillaBox.Margins.Margin1.Width = 0;
            this._queryScintillaBox.Name = "_queryScintillaBox";
            this._queryScintillaBox.Size = new System.Drawing.Size(828, 479);
            this._queryScintillaBox.TabIndex = 0;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(682, 662);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 5;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(763, 662);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 6;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _testQueryButton
            // 
            this._testQueryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._testQueryButton.Location = new System.Drawing.Point(601, 662);
            this._testQueryButton.Name = "_testQueryButton";
            this._testQueryButton.Size = new System.Drawing.Size(75, 23);
            this._testQueryButton.TabIndex = 4;
            this._testQueryButton.Text = "Test query";
            this._testQueryButton.UseVisualStyleBackColor = true;
            this._testQueryButton.Click += new System.EventHandler(this.HandleTestQueryClick);
            // 
            // _databaseConnectionControl
            // 
            this._databaseConnectionControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._databaseConnectionControl.Enabled = false;
            this._databaseConnectionControl.Location = new System.Drawing.Point(13, 561);
            this._databaseConnectionControl.Name = "_databaseConnectionControl";
            this._databaseConnectionControl.PathTags = null;
            this._databaseConnectionControl.ShowCopyConnectionTypeMenuOption = true;
            this._databaseConnectionControl.Size = new System.Drawing.Size(825, 92);
            this._databaseConnectionControl.TabIndex = 3;
            // 
            // _useFAMDBCheckBox
            // 
            this._useFAMDBCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._useFAMDBCheckBox.AutoSize = true;
            this._useFAMDBCheckBox.Location = new System.Drawing.Point(12, 515);
            this._useFAMDBCheckBox.Name = "_useFAMDBCheckBox";
            this._useFAMDBCheckBox.Size = new System.Drawing.Size(180, 17);
            this._useFAMDBCheckBox.TabIndex = 1;
            this._useFAMDBCheckBox.Text = "Use current FAM DB connection";
            this._useFAMDBCheckBox.UseVisualStyleBackColor = true;
            // 
            // _useSpecifiedDBCheckBox
            // 
            this._useSpecifiedDBCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._useSpecifiedDBCheckBox.AutoSize = true;
            this._useSpecifiedDBCheckBox.Location = new System.Drawing.Point(12, 538);
            this._useSpecifiedDBCheckBox.Name = "_useSpecifiedDBCheckBox";
            this._useSpecifiedDBCheckBox.Size = new System.Drawing.Size(167, 17);
            this._useSpecifiedDBCheckBox.TabIndex = 2;
            this._useSpecifiedDBCheckBox.Text = "Use specified DB connection:";
            this._useSpecifiedDBCheckBox.UseVisualStyleBackColor = true;
            // 
            // DataQueryRuleObjectSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(850, 694);
            this.Controls.Add(this._useSpecifiedDBCheckBox);
            this.Controls.Add(this._useFAMDBCheckBox);
            this.Controls.Add(panel1);
            this.Controls.Add(this._databaseConnectionControl);
            this.Controls.Add(this._testQueryButton);
            this.Controls.Add(label1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(470, 350);
            this.Name = "DataQueryRuleObjectSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Data query settings";
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._queryScintillaBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private ScintillaNET.Scintilla _queryScintillaBox;
        private System.Windows.Forms.Button _testQueryButton;
        private Database.DatabaseConnectionControl _databaseConnectionControl;
        private System.Windows.Forms.CheckBox _useFAMDBCheckBox;
        private System.Windows.Forms.CheckBox _useSpecifiedDBCheckBox;
    }
}
