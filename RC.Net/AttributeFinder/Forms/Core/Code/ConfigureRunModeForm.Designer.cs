namespace Extract.AttributeFinder.Forms
{
    partial class ConfigureRunModeForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigureRunModeForm));
            this._runModegroupBox = new System.Windows.Forms.GroupBox();
            this._runOnPaginationDocumentsRadioButton = new System.Windows.Forms.RadioButton();
            this._runByPageRadioButton = new System.Windows.Forms.RadioButton();
            this._runByDocumentRadioButton = new System.Windows.Forms.RadioButton();
            this._passVOAtoOutputRadioButton = new System.Windows.Forms.RadioButton();
            this._insertUnderParentCheckBox = new System.Windows.Forms.CheckBox();
            this._parentAttributeTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._parentValueTextBox = new System.Windows.Forms.TextBox();
            this._deepCopyInputAttributesCheckBox = new System.Windows.Forms.CheckBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._parentValuePathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._runModegroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _runModegroupBox
            // 
            this._runModegroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._runModegroupBox.Controls.Add(this._runOnPaginationDocumentsRadioButton);
            this._runModegroupBox.Controls.Add(this._runByPageRadioButton);
            this._runModegroupBox.Controls.Add(this._runByDocumentRadioButton);
            this._runModegroupBox.Controls.Add(this._passVOAtoOutputRadioButton);
            this._runModegroupBox.Location = new System.Drawing.Point(13, 13);
            this._runModegroupBox.Name = "_runModegroupBox";
            this._runModegroupBox.Size = new System.Drawing.Size(489, 116);
            this._runModegroupBox.TabIndex = 0;
            this._runModegroupBox.TabStop = false;
            this._runModegroupBox.Text = "Run mode";
            // 
            // _runOnPaginationDocumentsRadioButton
            // 
            this._runOnPaginationDocumentsRadioButton.AutoSize = true;
            this._runOnPaginationDocumentsRadioButton.Location = new System.Drawing.Point(13, 65);
            this._runOnPaginationDocumentsRadioButton.Name = "_runOnPaginationDocumentsRadioButton";
            this._runOnPaginationDocumentsRadioButton.Size = new System.Drawing.Size(255, 17);
            this._runOnPaginationDocumentsRadioButton.TabIndex = 3;
            this._runOnPaginationDocumentsRadioButton.TabStop = true;
            this._runOnPaginationDocumentsRadioButton.Text = "Run attribute rules on Document/DocumentData";
            this._runOnPaginationDocumentsRadioButton.UseVisualStyleBackColor = true;
            this._runOnPaginationDocumentsRadioButton.CheckedChanged += new System.EventHandler(this.HandleRunOnPaginationDocumentsRadioButton_CheckedChanged);
            // 
            // _runByPageRadioButton
            // 
            this._runByPageRadioButton.AutoSize = true;
            this._runByPageRadioButton.Location = new System.Drawing.Point(13, 42);
            this._runByPageRadioButton.Name = "_runByPageRadioButton";
            this._runByPageRadioButton.Size = new System.Drawing.Size(180, 17);
            this._runByPageRadioButton.TabIndex = 1;
            this._runByPageRadioButton.Text = "Run attribute rules on each page";
            this._runByPageRadioButton.UseVisualStyleBackColor = true;
            this._runByPageRadioButton.CheckedChanged += new System.EventHandler(this.HandleByPage_CheckedChanged);
            // 
            // _runByDocumentRadioButton
            // 
            this._runByDocumentRadioButton.AutoSize = true;
            this._runByDocumentRadioButton.Checked = true;
            this._runByDocumentRadioButton.Location = new System.Drawing.Point(13, 19);
            this._runByDocumentRadioButton.Name = "_runByDocumentRadioButton";
            this._runByDocumentRadioButton.Size = new System.Drawing.Size(205, 17);
            this._runByDocumentRadioButton.TabIndex = 0;
            this._runByDocumentRadioButton.TabStop = true;
            this._runByDocumentRadioButton.Text = "Run attribute rules on entire document";
            this._runByDocumentRadioButton.UseVisualStyleBackColor = true;
            // 
            // _passVOAtoOutputRadioButton
            // 
            this._passVOAtoOutputRadioButton.AutoSize = true;
            this._passVOAtoOutputRadioButton.Location = new System.Drawing.Point(13, 88);
            this._passVOAtoOutputRadioButton.Name = "_passVOAtoOutputRadioButton";
            this._passVOAtoOutputRadioButton.Size = new System.Drawing.Size(144, 17);
            this._passVOAtoOutputRadioButton.TabIndex = 2;
            this._passVOAtoOutputRadioButton.Text = "Pass input VOA to output";
            this._passVOAtoOutputRadioButton.UseVisualStyleBackColor = true;
            this._passVOAtoOutputRadioButton.CheckedChanged += new System.EventHandler(this.HandleVOAtoOutput_CheckedChanged);
            // 
            // _insertUnderParentCheckBox
            // 
            this._insertUnderParentCheckBox.AutoSize = true;
            this._insertUnderParentCheckBox.Location = new System.Drawing.Point(6, 14);
            this._insertUnderParentCheckBox.Name = "_insertUnderParentCheckBox";
            this._insertUnderParentCheckBox.Size = new System.Drawing.Size(237, 17);
            this._insertUnderParentCheckBox.TabIndex = 1;
            this._insertUnderParentCheckBox.Text = "Insert attributes under parent attribute named";
            this._insertUnderParentCheckBox.UseVisualStyleBackColor = true;
            this._insertUnderParentCheckBox.CheckedChanged += new System.EventHandler(this.HandleInsertUnderParent_CheckedChanged);
            // 
            // _parentAttributeTextBox
            // 
            this._parentAttributeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._parentAttributeTextBox.Location = new System.Drawing.Point(269, 14);
            this._parentAttributeTextBox.Name = "_parentAttributeTextBox";
            this._parentAttributeTextBox.Size = new System.Drawing.Size(188, 20);
            this._parentAttributeTextBox.TabIndex = 2;
            this._parentAttributeTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.HandleParentAttributeTextBox_Validating);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(188, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "with value";
            // 
            // _parentValueTextBox
            // 
            this._parentValueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._parentValueTextBox.Location = new System.Drawing.Point(269, 40);
            this._parentValueTextBox.Name = "_parentValueTextBox";
            this._parentValueTextBox.Size = new System.Drawing.Size(188, 20);
            this._parentValueTextBox.TabIndex = 4;
            // 
            // _deepCopyInputAttributesCheckBox
            // 
            this._deepCopyInputAttributesCheckBox.AutoSize = true;
            this._deepCopyInputAttributesCheckBox.Location = new System.Drawing.Point(6, 66);
            this._deepCopyInputAttributesCheckBox.Name = "_deepCopyInputAttributesCheckBox";
            this._deepCopyInputAttributesCheckBox.Size = new System.Drawing.Size(150, 17);
            this._deepCopyInputAttributesCheckBox.TabIndex = 5;
            this._deepCopyInputAttributesCheckBox.Text = "Deep copy input attributes";
            this._deepCopyInputAttributesCheckBox.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.CausesValidation = false;
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this._cancelButton.Location = new System.Drawing.Point(427, 247);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 6;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(346, 247);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 6;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _parentValuePathTagsButton
            // 
            this._parentValuePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._parentValuePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_parentValuePathTagsButton.Image")));
            this._parentValuePathTagsButton.Location = new System.Drawing.Point(463, 40);
            this._parentValuePathTagsButton.Name = "_parentValuePathTagsButton";
            this._parentValuePathTagsButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this._parentValuePathTagsButton.Size = new System.Drawing.Size(20, 20);
            this._parentValuePathTagsButton.TabIndex = 7;
            this._parentValuePathTagsButton.TextControl = this._parentValueTextBox;
            this._parentValuePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _errorProvider
            // 
            this._errorProvider.ContainerControl = this;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._deepCopyInputAttributesCheckBox);
            this.groupBox1.Controls.Add(this._parentValuePathTagsButton);
            this.groupBox1.Controls.Add(this._insertUnderParentCheckBox);
            this.groupBox1.Controls.Add(this._parentAttributeTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this._parentValueTextBox);
            this.groupBox1.Location = new System.Drawing.Point(13, 135);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(489, 92);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            // 
            // ConfigureRunModeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.ClientSize = new System.Drawing.Size(514, 282);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._runModegroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigureRunModeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure rule running mode";
            this._runModegroupBox.ResumeLayout(false);
            this._runModegroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox _runModegroupBox;
        private System.Windows.Forms.RadioButton _runByPageRadioButton;
        private System.Windows.Forms.RadioButton _runByDocumentRadioButton;
        private System.Windows.Forms.RadioButton _passVOAtoOutputRadioButton;
        private System.Windows.Forms.CheckBox _insertUnderParentCheckBox;
        private System.Windows.Forms.TextBox _parentAttributeTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _parentValueTextBox;
        private System.Windows.Forms.CheckBox _deepCopyInputAttributesCheckBox;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private Utilities.Forms.PathTagsButton _parentValuePathTagsButton;
        private System.Windows.Forms.ErrorProvider _errorProvider;
        private System.Windows.Forms.RadioButton _runOnPaginationDocumentsRadioButton;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}