namespace IDShieldOffice
{
    partial class BatesNumberManagerAppearancePropertyPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="BatesNumberManagerAppearancePropertyPage"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="BatesNumberManagerAppearancePropertyPage"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources
                if (components != null)
                {
                    components.Dispose();
                }
                if (_font != null)
                {
                    _font.Dispose();
                }
                if (_fontDialog != null)
                {
                    _fontDialog.Dispose();
                }
            }

            // Release unmanaged resources

            // Call base dispose method
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._changeFontButton = new System.Windows.Forms.Button();
            this._fontTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._selectedAlignmentLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._anchorAlignmentControl = new Extract.Utilities.Forms.AnchorAlignmentControl();
            this._horizontalPositionComboBox = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this._horizontalInchesTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._verticalPositionComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this._verticalInchesTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(172, 48);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(61, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "of the page";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._changeFontButton);
            this.groupBox1.Controls.Add(this._fontTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(413, 67);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Appearance";
            // 
            // _changeFontButton
            // 
            this._changeFontButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._changeFontButton.Location = new System.Drawing.Point(382, 35);
            this._changeFontButton.Name = "_changeFontButton";
            this._changeFontButton.Size = new System.Drawing.Size(25, 23);
            this._changeFontButton.TabIndex = 2;
            this._changeFontButton.Text = "...";
            this._changeFontButton.UseVisualStyleBackColor = true;
            this._changeFontButton.Click += new System.EventHandler(this.HandleChangeFontButtonClick);
            // 
            // _fontTextBox
            // 
            this._fontTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._fontTextBox.Enabled = false;
            this._fontTextBox.Location = new System.Drawing.Point(10, 37);
            this._fontTextBox.Name = "_fontTextBox";
            this._fontTextBox.Size = new System.Drawing.Size(366, 20);
            this._fontTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Font";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this._selectedAlignmentLabel);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this._anchorAlignmentControl);
            this.groupBox3.Location = new System.Drawing.Point(3, 158);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(413, 82);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Select alignment of Bates number relative to above anchor position";
            // 
            // _selectedAlignmentLabel
            // 
            this._selectedAlignmentLabel.AutoSize = true;
            this._selectedAlignmentLabel.Location = new System.Drawing.Point(6, 61);
            this._selectedAlignmentLabel.Name = "_selectedAlignmentLabel";
            this._selectedAlignmentLabel.Size = new System.Drawing.Size(100, 13);
            this._selectedAlignmentLabel.TabIndex = 3;
            this._selectedAlignmentLabel.Text = "Selected alignment:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(162, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(133, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "from the picture on the left.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(144, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(178, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Select one of the nine anchor points";
            // 
            // _anchorAlignmentControl
            // 
            this._anchorAlignmentControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 23F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._anchorAlignmentControl.Location = new System.Drawing.Point(8, 19);
            this._anchorAlignmentControl.Name = "_anchorAlignmentControl";
            this._anchorAlignmentControl.Size = new System.Drawing.Size(100, 39);
            this._anchorAlignmentControl.TabIndex = 0;
            this._anchorAlignmentControl.Text = "TEXT";
            this._anchorAlignmentControl.AnchorAlignmentChanged += new System.EventHandler<Extract.Utilities.Forms.AnchorAlignmentChangedEventArgs>(this.HandleAnchorAlignmentControlAnchorAlignmentChanged);
            // 
            // _horizontalPositionComboBox
            // 
            this._horizontalPositionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._horizontalPositionComboBox.FormattingEnabled = true;
            this._horizontalPositionComboBox.Items.AddRange(new object[] {
            "left",
            "right"});
            this._horizontalPositionComboBox.Location = new System.Drawing.Point(110, 19);
            this._horizontalPositionComboBox.Name = "_horizontalPositionComboBox";
            this._horizontalPositionComboBox.Size = new System.Drawing.Size(60, 21);
            this._horizontalPositionComboBox.TabIndex = 2;
            this._horizontalPositionComboBox.TextChanged += new System.EventHandler(this.HandleHorizontalPositionComboBoxTextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(47, 48);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "inches from";
            // 
            // _horizontalInchesTextBox
            // 
            this._horizontalInchesTextBox.Location = new System.Drawing.Point(10, 19);
            this._horizontalInchesTextBox.Name = "_horizontalInchesTextBox";
            this._horizontalInchesTextBox.Size = new System.Drawing.Size(35, 20);
            this._horizontalInchesTextBox.TabIndex = 0;
            this._horizontalInchesTextBox.TextChanged += new System.EventHandler(this.HandleHorizontalInchesTextBoxTextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(172, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(61, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "of the page";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this._horizontalPositionComboBox);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this._horizontalInchesTextBox);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this._verticalPositionComboBox);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this._verticalInchesTextBox);
            this.groupBox2.Location = new System.Drawing.Point(3, 76);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(413, 76);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Default anchor position for Bates number";
            // 
            // _verticalPositionComboBox
            // 
            this._verticalPositionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._verticalPositionComboBox.FormattingEnabled = true;
            this._verticalPositionComboBox.Items.AddRange(new object[] {
            "top",
            "bottom"});
            this._verticalPositionComboBox.Location = new System.Drawing.Point(110, 45);
            this._verticalPositionComboBox.Name = "_verticalPositionComboBox";
            this._verticalPositionComboBox.Size = new System.Drawing.Size(60, 21);
            this._verticalPositionComboBox.TabIndex = 6;
            this._verticalPositionComboBox.TextChanged += new System.EventHandler(this.HandleVerticalPositionComboBoxTextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(47, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "inches from";
            // 
            // _verticalInchesTextBox
            // 
            this._verticalInchesTextBox.Location = new System.Drawing.Point(10, 45);
            this._verticalInchesTextBox.Name = "_verticalInchesTextBox";
            this._verticalInchesTextBox.Size = new System.Drawing.Size(35, 20);
            this._verticalInchesTextBox.TabIndex = 4;
            this._verticalInchesTextBox.TextChanged += new System.EventHandler(this.HandleVerticalInchesTextBoxTextChanged);
            // 
            // BatesNumberManagerAppearancePropertyPage
            // 
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Name = "BatesNumberManagerAppearancePropertyPage";
            this.Size = new System.Drawing.Size(417, 242);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button _changeFontButton;
        private System.Windows.Forms.TextBox _fontTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label _selectedAlignmentLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private Extract.Utilities.Forms.AnchorAlignmentControl _anchorAlignmentControl;
        private System.Windows.Forms.ComboBox _horizontalPositionComboBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox _horizontalInchesTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox _verticalPositionComboBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox _verticalInchesTextBox;
    }
}
