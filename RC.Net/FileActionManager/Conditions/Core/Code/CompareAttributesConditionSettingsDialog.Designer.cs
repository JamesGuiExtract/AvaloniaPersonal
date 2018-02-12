using System.Windows.Forms;

namespace Extract.FileActionManager.Conditions
{
    partial class CompareAttributesConditionSettingsDialog : Form
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
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._xpathToIgnoreScintillaBox = new ScintillaNET.Scintilla();
            this._firstAttributeSetNameComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this._secondAttributeSetNameComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this._differentSameComboBox = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._ignoreAttributesSelectedByXPathRadioButton = new System.Windows.Forms.RadioButton();
            this._ignoreEmptyAttributesRadioButton = new System.Windows.Forms.RadioButton();
            this._ignoreNoAttributesRadioButton = new System.Windows.Forms.RadioButton();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._xpathToIgnoreScintillaBox)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label5
            // 
            label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(91, 28);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(126, 13);
            label5.TabIndex = 2;
            label5.Text = "for the following two sets:";
            // 
            // label6
            // 
            label6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(12, 9);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(303, 13);
            label6.TabIndex = 0;
            label6.Text = "Consider condition as met if the attributes for the current file are";
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOk.Location = new System.Drawing.Point(236, 389);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 8;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(317, 389);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 9;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _xpathToIgnoreScintillaBox
            // 
            this._xpathToIgnoreScintillaBox.AcceptsTab = false;
            this._xpathToIgnoreScintillaBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._xpathToIgnoreScintillaBox.Annotations.Visibility = ScintillaNET.AnnotationsVisibility.Standard;
            this._xpathToIgnoreScintillaBox.ConfigurationManager.Language = "xml";
            this._xpathToIgnoreScintillaBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._xpathToIgnoreScintillaBox.Indentation.ShowGuides = true;
            this._xpathToIgnoreScintillaBox.Indentation.SmartIndentType = ScintillaNET.SmartIndent.Simple;
            this._xpathToIgnoreScintillaBox.Indentation.TabWidth = 4;
            this._xpathToIgnoreScintillaBox.IsBraceMatching = true;
            this._xpathToIgnoreScintillaBox.LineWrap.Mode = ScintillaNET.WrapMode.Word;
            this._xpathToIgnoreScintillaBox.LineWrap.VisualFlags = ScintillaNET.WrapVisualFlag.Start;
            this._xpathToIgnoreScintillaBox.LineWrap.VisualFlagsLocation = ScintillaNET.WrapVisualLocation.StartByText;
            this._xpathToIgnoreScintillaBox.Location = new System.Drawing.Point(6, 93);
            this._xpathToIgnoreScintillaBox.Margins.Margin1.Width = 0;
            this._xpathToIgnoreScintillaBox.Name = "_xpathToIgnoreScintillaBox";
            this._xpathToIgnoreScintillaBox.Size = new System.Drawing.Size(362, 123);
            this._xpathToIgnoreScintillaBox.TabIndex = 3;
            // 
            // _firstAttributeSetNameComboBox
            // 
            this._firstAttributeSetNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._firstAttributeSetNameComboBox.FormattingEnabled = true;
            this._firstAttributeSetNameComboBox.Location = new System.Drawing.Point(15, 68);
            this._firstAttributeSetNameComboBox.Name = "_firstAttributeSetNameComboBox";
            this._firstAttributeSetNameComboBox.Size = new System.Drawing.Size(374, 21);
            this._firstAttributeSetNameComboBox.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "First attribute set name";
            // 
            // _secondAttributeSetNameComboBox
            // 
            this._secondAttributeSetNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._secondAttributeSetNameComboBox.FormattingEnabled = true;
            this._secondAttributeSetNameComboBox.Location = new System.Drawing.Point(15, 116);
            this._secondAttributeSetNameComboBox.Name = "_secondAttributeSetNameComboBox";
            this._secondAttributeSetNameComboBox.Size = new System.Drawing.Size(374, 21);
            this._secondAttributeSetNameComboBox.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Second attribute set name";
            // 
            // _differentSameComboBox
            // 
            this._differentSameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._differentSameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._differentSameComboBox.FormattingEnabled = true;
            this._differentSameComboBox.Items.AddRange(new object[] {
            "different",
            "the same"});
            this._differentSameComboBox.Location = new System.Drawing.Point(15, 25);
            this._differentSameComboBox.Name = "_differentSameComboBox";
            this._differentSameComboBox.Size = new System.Drawing.Size(70, 21);
            this._differentSameComboBox.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._ignoreAttributesSelectedByXPathRadioButton);
            this.groupBox1.Controls.Add(this._ignoreEmptyAttributesRadioButton);
            this.groupBox1.Controls.Add(this._ignoreNoAttributesRadioButton);
            this.groupBox1.Controls.Add(this._xpathToIgnoreScintillaBox);
            this.groupBox1.Location = new System.Drawing.Point(15, 152);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(374, 222);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Attribute filtering";
            // 
            // _ignoreAttributesSelectedByXPathRadioButton
            // 
            this._ignoreAttributesSelectedByXPathRadioButton.AutoSize = true;
            this._ignoreAttributesSelectedByXPathRadioButton.Location = new System.Drawing.Point(6, 65);
            this._ignoreAttributesSelectedByXPathRadioButton.Name = "_ignoreAttributesSelectedByXPathRadioButton";
            this._ignoreAttributesSelectedByXPathRadioButton.Size = new System.Drawing.Size(209, 17);
            this._ignoreAttributesSelectedByXPathRadioButton.TabIndex = 2;
            this._ignoreAttributesSelectedByXPathRadioButton.TabStop = true;
            this._ignoreAttributesSelectedByXPathRadioButton.Text = "Ignore attributes selected by this XPath";
            this._ignoreAttributesSelectedByXPathRadioButton.UseVisualStyleBackColor = true;
            this._ignoreAttributesSelectedByXPathRadioButton.CheckedChanged += new System.EventHandler(this.HandleIgnoreAttributesRadioButton_CheckedChanged);
            // 
            // _ignoreEmptyAttributesRadioButton
            // 
            this._ignoreEmptyAttributesRadioButton.AutoSize = true;
            this._ignoreEmptyAttributesRadioButton.Location = new System.Drawing.Point(6, 42);
            this._ignoreEmptyAttributesRadioButton.Name = "_ignoreEmptyAttributesRadioButton";
            this._ignoreEmptyAttributesRadioButton.Size = new System.Drawing.Size(132, 17);
            this._ignoreEmptyAttributesRadioButton.TabIndex = 1;
            this._ignoreEmptyAttributesRadioButton.TabStop = true;
            this._ignoreEmptyAttributesRadioButton.Text = "Ignore empty attributes";
            this._ignoreEmptyAttributesRadioButton.UseVisualStyleBackColor = true;
            this._ignoreEmptyAttributesRadioButton.CheckedChanged += new System.EventHandler(this.HandleIgnoreAttributesRadioButton_CheckedChanged);
            // 
            // _ignoreNoAttributesRadioButton
            // 
            this._ignoreNoAttributesRadioButton.AutoSize = true;
            this._ignoreNoAttributesRadioButton.Location = new System.Drawing.Point(6, 19);
            this._ignoreNoAttributesRadioButton.Name = "_ignoreNoAttributesRadioButton";
            this._ignoreNoAttributesRadioButton.Size = new System.Drawing.Size(148, 17);
            this._ignoreNoAttributesRadioButton.TabIndex = 0;
            this._ignoreNoAttributesRadioButton.TabStop = true;
            this._ignoreNoAttributesRadioButton.Text = "Don\'t ignore any attributes";
            this._ignoreNoAttributesRadioButton.UseVisualStyleBackColor = true;
            this._ignoreNoAttributesRadioButton.CheckedChanged += new System.EventHandler(this.HandleIgnoreAttributesRadioButton_CheckedChanged);
            // 
            // CompareAttributesConditionSettingsDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(402, 424);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(label5);
            this.Controls.Add(this._differentSameComboBox);
            this.Controls.Add(label6);
            this.Controls.Add(this._secondAttributeSetNameComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._firstAttributeSetNameComboBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(this._buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1024, 768);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(406, 400);
            this.Name = "CompareAttributesConditionSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Compare attributes condition settings";
            ((System.ComponentModel.ISupportInitialize)(this._xpathToIgnoreScintillaBox)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _buttonCancel;
        private ScintillaNET.Scintilla _xpathToIgnoreScintillaBox;
        private ComboBox _firstAttributeSetNameComboBox;
        private Label label1;
        private ComboBox _secondAttributeSetNameComboBox;
        private Label label2;
        private ComboBox _differentSameComboBox;
        private GroupBox groupBox1;
        private RadioButton _ignoreAttributesSelectedByXPathRadioButton;
        private RadioButton _ignoreEmptyAttributesRadioButton;
        private RadioButton _ignoreNoAttributesRadioButton;
    }
}
