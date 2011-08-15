namespace Extract.Redaction.CustomComponentsHelper
{
    partial class CreateRedactedImageAdvanceLabelingDialog
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
            System.Windows.Forms.GroupBox groupBox1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateRedactedImageAdvanceLabelingDialog));
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.Button _buttonCancel;
            this._buttonUp = new Extract.Utilities.Forms.ExtractUpButton();
            this._listReplacements = new System.Windows.Forms.ListView();
            this._columnToBeReplaced = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._columnReplacement = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._buttonAdd = new System.Windows.Forms.Button();
            this._buttonRemove = new System.Windows.Forms.Button();
            this._buttonModify = new System.Windows.Forms.Button();
            this._buttonDown = new Extract.Utilities.Forms.ExtractDownButton();
            this._buttonSaveList = new System.Windows.Forms.Button();
            this._buttonLoadList = new System.Windows.Forms.Button();
            this._checkAutoCase = new System.Windows.Forms.CheckBox();
            this._checkPrefixText = new System.Windows.Forms.CheckBox();
            this._suffixTags = new Extract.Utilities.Forms.PathTagsButton();
            this._textSuffix = new System.Windows.Forms.TextBox();
            this._textPrefix = new System.Windows.Forms.TextBox();
            this._prefixTags = new Extract.Utilities.Forms.PathTagsButton();
            this._checkSuffixText = new System.Windows.Forms.CheckBox();
            this._buttonOk = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            _buttonCancel = new System.Windows.Forms.Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this._buttonUp);
            groupBox1.Controls.Add(this._listReplacements);
            groupBox1.Controls.Add(this._buttonAdd);
            groupBox1.Controls.Add(this._buttonRemove);
            groupBox1.Controls.Add(this._buttonModify);
            groupBox1.Controls.Add(this._buttonDown);
            groupBox1.Controls.Add(this._buttonSaveList);
            groupBox1.Controls.Add(this._buttonLoadList);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(484, 234);
            groupBox1.TabIndex = 19;
            groupBox1.TabStop = false;
            groupBox1.Text = "Perform the following search and replace operations on the redaction text";
            // 
            // _buttonUp
            // 
            this._buttonUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonUp.Image = ((System.Drawing.Image)(resources.GetObject("_buttonUp.Image")));
            this._buttonUp.Location = new System.Drawing.Point(402, 106);
            this._buttonUp.Name = "_buttonUp";
            this._buttonUp.Size = new System.Drawing.Size(35, 35);
            this._buttonUp.TabIndex = 4;
            this._buttonUp.UseVisualStyleBackColor = true;
            this._buttonUp.Click += new System.EventHandler(this.HandleUpClicked);
            // 
            // _listReplacements
            // 
            this._listReplacements.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._listReplacements.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._columnToBeReplaced,
            this._columnReplacement});
            this._listReplacements.FullRowSelect = true;
            this._listReplacements.GridLines = true;
            this._listReplacements.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._listReplacements.HideSelection = false;
            this._listReplacements.Location = new System.Drawing.Point(8, 19);
            this._listReplacements.Name = "_listReplacements";
            this._listReplacements.Size = new System.Drawing.Size(388, 206);
            this._listReplacements.TabIndex = 0;
            this._listReplacements.UseCompatibleStateImageBehavior = false;
            this._listReplacements.View = System.Windows.Forms.View.Details;
            this._listReplacements.SelectedIndexChanged += new System.EventHandler(this.HandleReplacementListSelectionChanged);
            // 
            // _columnToBeReplaced
            // 
            this._columnToBeReplaced.Text = "To Be Replaced";
            this._columnToBeReplaced.Width = 192;
            // 
            // _columnReplacement
            // 
            this._columnReplacement.Text = "Replacement";
            this._columnReplacement.Width = 192;
            // 
            // _buttonAdd
            // 
            this._buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonAdd.Location = new System.Drawing.Point(403, 19);
            this._buttonAdd.Name = "_buttonAdd";
            this._buttonAdd.Size = new System.Drawing.Size(75, 23);
            this._buttonAdd.TabIndex = 1;
            this._buttonAdd.Text = "Add...";
            this._buttonAdd.UseVisualStyleBackColor = true;
            this._buttonAdd.Click += new System.EventHandler(this.HandleAddReplacementClicked);
            // 
            // _buttonRemove
            // 
            this._buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonRemove.Location = new System.Drawing.Point(402, 48);
            this._buttonRemove.Name = "_buttonRemove";
            this._buttonRemove.Size = new System.Drawing.Size(75, 23);
            this._buttonRemove.TabIndex = 2;
            this._buttonRemove.Text = "Remove";
            this._buttonRemove.UseVisualStyleBackColor = true;
            this._buttonRemove.Click += new System.EventHandler(this.HandleRemoveReplacementClicked);
            // 
            // _buttonModify
            // 
            this._buttonModify.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonModify.Location = new System.Drawing.Point(402, 77);
            this._buttonModify.Name = "_buttonModify";
            this._buttonModify.Size = new System.Drawing.Size(75, 23);
            this._buttonModify.TabIndex = 3;
            this._buttonModify.Text = "Modify...";
            this._buttonModify.UseVisualStyleBackColor = true;
            this._buttonModify.Click += new System.EventHandler(this.HandleModifyReplacementClicked);
            // 
            // _buttonDown
            // 
            this._buttonDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonDown.Image = ((System.Drawing.Image)(resources.GetObject("_buttonDown.Image")));
            this._buttonDown.Location = new System.Drawing.Point(442, 106);
            this._buttonDown.Name = "_buttonDown";
            this._buttonDown.Size = new System.Drawing.Size(35, 35);
            this._buttonDown.TabIndex = 5;
            this._buttonDown.UseVisualStyleBackColor = true;
            this._buttonDown.Click += new System.EventHandler(this.HandleDownClicked);
            // 
            // _buttonSaveList
            // 
            this._buttonSaveList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonSaveList.Location = new System.Drawing.Point(402, 202);
            this._buttonSaveList.Name = "_buttonSaveList";
            this._buttonSaveList.Size = new System.Drawing.Size(75, 23);
            this._buttonSaveList.TabIndex = 7;
            this._buttonSaveList.Text = "Save List...";
            this._buttonSaveList.UseVisualStyleBackColor = true;
            this._buttonSaveList.Click += new System.EventHandler(this.HandleSaveListClicked);
            // 
            // _buttonLoadList
            // 
            this._buttonLoadList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonLoadList.Location = new System.Drawing.Point(402, 173);
            this._buttonLoadList.Name = "_buttonLoadList";
            this._buttonLoadList.Size = new System.Drawing.Size(75, 23);
            this._buttonLoadList.TabIndex = 6;
            this._buttonLoadList.Text = "Load List...";
            this._buttonLoadList.UseVisualStyleBackColor = true;
            this._buttonLoadList.Click += new System.EventHandler(this.HandleLoadListClicked);
            // 
            // groupBox2
            // 
            groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            groupBox2.Controls.Add(this._checkAutoCase);
            groupBox2.Controls.Add(this._checkPrefixText);
            groupBox2.Controls.Add(this._suffixTags);
            groupBox2.Controls.Add(this._textPrefix);
            groupBox2.Controls.Add(this._textSuffix);
            groupBox2.Controls.Add(this._prefixTags);
            groupBox2.Controls.Add(this._checkSuffixText);
            groupBox2.Location = new System.Drawing.Point(12, 252);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(484, 142);
            groupBox2.TabIndex = 20;
            groupBox2.TabStop = false;
            groupBox2.Text = "Additional redaction text settings";
            // 
            // _checkAutoCase
            // 
            this._checkAutoCase.AutoSize = true;
            this._checkAutoCase.Location = new System.Drawing.Point(8, 19);
            this._checkAutoCase.Name = "_checkAutoCase";
            this._checkAutoCase.Size = new System.Drawing.Size(187, 17);
            this._checkAutoCase.TabIndex = 0;
            this._checkAutoCase.Text = "Auto adjust case for redaction text";
            this._checkAutoCase.UseVisualStyleBackColor = true;
            // 
            // _checkPrefixText
            // 
            this._checkPrefixText.AutoSize = true;
            this._checkPrefixText.Location = new System.Drawing.Point(8, 42);
            this._checkPrefixText.Name = "_checkPrefixText";
            this._checkPrefixText.Size = new System.Drawing.Size(174, 17);
            this._checkPrefixText.TabIndex = 1;
            this._checkPrefixText.Text = "Prefix first instance of type with:";
            this._checkPrefixText.UseVisualStyleBackColor = true;
            this._checkPrefixText.CheckedChanged += new System.EventHandler(this.HandlePrefixCheckChanged);
            // 
            // _suffixTags
            // 
            this._suffixTags.DisplayFunctionTags = false;
            this._suffixTags.Image = ((System.Drawing.Image)(resources.GetObject("_suffixTags.Image")));
            this._suffixTags.Location = new System.Drawing.Point(347, 114);
            this._suffixTags.Name = "_suffixTags";
            this._suffixTags.Size = new System.Drawing.Size(18, 20);
            this._suffixTags.TabIndex = 6;
            this._suffixTags.TextControl = this._textSuffix;
            this._suffixTags.UseVisualStyleBackColor = true;
            // 
            // _textSuffix
            // 
            this._textSuffix.HideSelection = false;
            this._textSuffix.Location = new System.Drawing.Point(29, 114);
            this._textSuffix.Name = "_textSuffix";
            this._textSuffix.Size = new System.Drawing.Size(312, 20);
            this._textSuffix.TabIndex = 5;
            // 
            // _textPrefix
            // 
            this._textPrefix.HideSelection = false;
            this._textPrefix.Location = new System.Drawing.Point(29, 65);
            this._textPrefix.Name = "_textPrefix";
            this._textPrefix.Size = new System.Drawing.Size(312, 20);
            this._textPrefix.TabIndex = 2;
            // 
            // _prefixTags
            // 
            this._prefixTags.DisplayFunctionTags = false;
            this._prefixTags.Image = ((System.Drawing.Image)(resources.GetObject("_prefixTags.Image")));
            this._prefixTags.Location = new System.Drawing.Point(347, 64);
            this._prefixTags.Name = "_prefixTags";
            this._prefixTags.Size = new System.Drawing.Size(18, 20);
            this._prefixTags.TabIndex = 3;
            this._prefixTags.TextControl = this._textPrefix;
            this._prefixTags.UseVisualStyleBackColor = true;
            // 
            // _checkSuffixText
            // 
            this._checkSuffixText.AutoSize = true;
            this._checkSuffixText.Location = new System.Drawing.Point(8, 91);
            this._checkSuffixText.Name = "_checkSuffixText";
            this._checkSuffixText.Size = new System.Drawing.Size(174, 17);
            this._checkSuffixText.TabIndex = 4;
            this._checkSuffixText.Text = "Suffix first instance of type with:";
            this._checkSuffixText.UseVisualStyleBackColor = true;
            this._checkSuffixText.CheckedChanged += new System.EventHandler(this.HandleSuffixCheckChanged);
            // 
            // _buttonCancel
            // 
            _buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            _buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            _buttonCancel.Location = new System.Drawing.Point(421, 400);
            _buttonCancel.Name = "_buttonCancel";
            _buttonCancel.Size = new System.Drawing.Size(75, 23);
            _buttonCancel.TabIndex = 1;
            _buttonCancel.Text = "Cancel";
            _buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOk.Location = new System.Drawing.Point(340, 400);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 0;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.HandleOkClicked);
            // 
            // CreateRedactedImageAdvanceLabelingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = _buttonCancel;
            this.ClientSize = new System.Drawing.Size(508, 435);
            this.Controls.Add(this._buttonOk);
            this.Controls.Add(_buttonCancel);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(514, 463);
            this.Name = "CreateRedactedImageAdvanceLabelingDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Advanced label settings";
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView _listReplacements;
        private System.Windows.Forms.Button _buttonAdd;
        private System.Windows.Forms.Button _buttonRemove;
        private System.Windows.Forms.Button _buttonModify;
        private Utilities.Forms.ExtractDownButton _buttonDown;
        private System.Windows.Forms.Button _buttonSaveList;
        private System.Windows.Forms.Button _buttonLoadList;
        private System.Windows.Forms.CheckBox _checkPrefixText;
        private System.Windows.Forms.TextBox _textPrefix;
        private Utilities.Forms.PathTagsButton _prefixTags;
        private Utilities.Forms.PathTagsButton _suffixTags;
        private System.Windows.Forms.TextBox _textSuffix;
        private System.Windows.Forms.CheckBox _checkSuffixText;
        private System.Windows.Forms.CheckBox _checkAutoCase;
        private Utilities.Forms.ExtractUpButton _buttonUp;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.ColumnHeader _columnToBeReplaced;
        private System.Windows.Forms.ColumnHeader _columnReplacement;
    }
}