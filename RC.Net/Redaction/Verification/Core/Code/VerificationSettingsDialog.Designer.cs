namespace Extract.Redaction.Verification
{
    partial class VerificationSettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="VerificationSettingsDialog"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="VerificationSettingsDialog"/>.
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
            }

            // Release unmanaged resources

            // Call base dispose method
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.GroupBox groupBox3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VerificationSettingsDialog));
            System.Windows.Forms.GroupBox groupBox4;
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.Label label1;
            this._backdropImagePathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._backdropImageTextBox = new System.Windows.Forms.TextBox();
            this._backdropImageBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._backdropImageCheckBox = new System.Windows.Forms.CheckBox();
            this._slideshowSettingsButton = new System.Windows.Forms.Button();
            this._enableSlideshowCheckBox = new System.Windows.Forms.CheckBox();
            this._actionNamePathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._actionStatusComboBox = new System.Windows.Forms.ComboBox();
            this._actionNameComboBox = new Extract.Utilities.Forms.BetterComboBox();
            this._fileActionCheckBox = new System.Windows.Forms.CheckBox();
            this._tagSettingsButton = new System.Windows.Forms.Button();
            this._allowTagsCheckBox = new System.Windows.Forms.CheckBox();
            this._seamlessNavigationCheckBox = new System.Windows.Forms.CheckBox();
            this._verifyAllItemsCheckBox = new System.Windows.Forms.CheckBox();
            this._launchFullScreenCheckBox = new System.Windows.Forms.CheckBox();
            this._feedbackSettingsButton = new System.Windows.Forms.Button();
            this._collectFeedbackCheckBox = new System.Windows.Forms.CheckBox();
            this._requireExemptionsCheckBox = new System.Windows.Forms.CheckBox();
            this._requireTypeCheckBox = new System.Windows.Forms.CheckBox();
            this._verifyAllPagesCheckBox = new System.Windows.Forms.CheckBox();
            this._promptForSaveUntilCommit = new System.Windows.Forms.CheckBox();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this._verifyFullPageCluesCheckBox = new System.Windows.Forms.CheckBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._redactionQaExplanatoryLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this._redactionVerifyExplanatoryTextLabel = new System.Windows.Forms.Label();
            this._redactionQaComboBox = new System.Windows.Forms.ComboBox();
            this._redactionQaRadioButton = new System.Windows.Forms.RadioButton();
            this._redactionVerificationRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this._dataFileControl = new Extract.Redaction.DataFileControl();
            groupBox3 = new System.Windows.Forms.GroupBox();
            groupBox4 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(this._backdropImagePathTagsButton);
            groupBox3.Controls.Add(this._backdropImageBrowseButton);
            groupBox3.Controls.Add(this._backdropImageTextBox);
            groupBox3.Controls.Add(this._backdropImageCheckBox);
            groupBox3.Location = new System.Drawing.Point(6, 144);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(429, 76);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Image location";
            // 
            // _backdropImagePathTagsButton
            // 
            this._backdropImagePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._backdropImagePathTagsButton.Enabled = false;
            this._backdropImagePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_backdropImagePathTagsButton.Image")));
            this._backdropImagePathTagsButton.Location = new System.Drawing.Point(372, 41);
            this._backdropImagePathTagsButton.Name = "_backdropImagePathTagsButton";
            this._backdropImagePathTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._backdropImagePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._backdropImagePathTagsButton.TabIndex = 2;
            this._backdropImagePathTagsButton.TextControl = this._backdropImageTextBox;
            this._backdropImagePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _backdropImageTextBox
            // 
            this._backdropImageTextBox.Enabled = false;
            this._backdropImageTextBox.HideSelection = false;
            this._backdropImageTextBox.Location = new System.Drawing.Point(6, 42);
            this._backdropImageTextBox.Name = "_backdropImageTextBox";
            this._backdropImageTextBox.Size = new System.Drawing.Size(360, 20);
            this._backdropImageTextBox.TabIndex = 1;
            // 
            // _backdropImageBrowseButton
            // 
            this._backdropImageBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._backdropImageBrowseButton.Enabled = false;
            this._backdropImageBrowseButton.EnsureFileExists = false;
            this._backdropImageBrowseButton.EnsurePathExists = false;
            this._backdropImageBrowseButton.Location = new System.Drawing.Point(396, 41);
            this._backdropImageBrowseButton.Name = "_backdropImageBrowseButton";
            this._backdropImageBrowseButton.Size = new System.Drawing.Size(27, 20);
            this._backdropImageBrowseButton.TabIndex = 3;
            this._backdropImageBrowseButton.Text = "...";
            this._backdropImageBrowseButton.UseVisualStyleBackColor = true;
            this._backdropImageBrowseButton.PathSelected += new System.EventHandler<Extract.Utilities.Forms.PathSelectedEventArgs>(this.HandleBackdropImageBrowseButtonPathSelected);
            // 
            // _backdropImageCheckBox
            // 
            this._backdropImageCheckBox.AutoSize = true;
            this._backdropImageCheckBox.Location = new System.Drawing.Point(7, 19);
            this._backdropImageCheckBox.Name = "_backdropImageCheckBox";
            this._backdropImageCheckBox.Size = new System.Drawing.Size(260, 17);
            this._backdropImageCheckBox.TabIndex = 0;
            this._backdropImageCheckBox.Text = "Use image as backdrop for verification if available";
            this._backdropImageCheckBox.UseVisualStyleBackColor = true;
            this._backdropImageCheckBox.CheckedChanged += new System.EventHandler(this.HandleBackdropImageCheckBoxCheckedChanged);
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(this._slideshowSettingsButton);
            groupBox4.Controls.Add(this._enableSlideshowCheckBox);
            groupBox4.Location = new System.Drawing.Point(6, 242);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new System.Drawing.Size(429, 47);
            groupBox4.TabIndex = 2;
            groupBox4.TabStop = false;
            groupBox4.Text = "Slideshow";
            // 
            // _slideshowSettingsButton
            // 
            this._slideshowSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._slideshowSettingsButton.Location = new System.Drawing.Point(348, 15);
            this._slideshowSettingsButton.Name = "_slideshowSettingsButton";
            this._slideshowSettingsButton.Size = new System.Drawing.Size(75, 23);
            this._slideshowSettingsButton.TabIndex = 1;
            this._slideshowSettingsButton.Text = "Settings...";
            this._slideshowSettingsButton.UseVisualStyleBackColor = true;
            this._slideshowSettingsButton.Click += new System.EventHandler(this.HandleSlideshowSettingsButtonClick);
            // 
            // _enableSlideshowCheckBox
            // 
            this._enableSlideshowCheckBox.AutoSize = true;
            this._enableSlideshowCheckBox.Checked = true;
            this._enableSlideshowCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._enableSlideshowCheckBox.Location = new System.Drawing.Point(6, 19);
            this._enableSlideshowCheckBox.Name = "_enableSlideshowCheckBox";
            this._enableSlideshowCheckBox.Size = new System.Drawing.Size(187, 17);
            this._enableSlideshowCheckBox.TabIndex = 0;
            this._enableSlideshowCheckBox.Text = "Enable slideshow in verification UI";
            this._enableSlideshowCheckBox.UseVisualStyleBackColor = true;
            this._enableSlideshowCheckBox.CheckedChanged += new System.EventHandler(this.HandleEnableSlideshowCheckBoxCheckedChanged);
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(this._actionNamePathTagsButton);
            groupBox2.Controls.Add(this._actionStatusComboBox);
            groupBox2.Controls.Add(this._actionNameComboBox);
            groupBox2.Controls.Add(this._fileActionCheckBox);
            groupBox2.Location = new System.Drawing.Point(6, 61);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(429, 78);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "After committing a document";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(221, 46);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(16, 13);
            label1.TabIndex = 3;
            label1.Text = "to";
            // 
            // _actionNamePathTagsButton
            // 
            this._actionNamePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_actionNamePathTagsButton.Image")));
            this._actionNamePathTagsButton.Location = new System.Drawing.Point(197, 43);
            this._actionNamePathTagsButton.Name = "_actionNamePathTagsButton";
            this._actionNamePathTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._actionNamePathTagsButton.Size = new System.Drawing.Size(18, 20);
            this._actionNamePathTagsButton.TabIndex = 2;
            this._actionNamePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _actionStatusComboBox
            // 
            this._actionStatusComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._actionStatusComboBox.FormattingEnabled = true;
            this._actionStatusComboBox.Items.AddRange(new object[] {
            "Completed",
            "Failed",
            "Pending",
            "Skipped",
            "Unattempted"});
            this._actionStatusComboBox.Location = new System.Drawing.Point(243, 43);
            this._actionStatusComboBox.Name = "_actionStatusComboBox";
            this._actionStatusComboBox.Size = new System.Drawing.Size(179, 21);
            this._actionStatusComboBox.TabIndex = 4;
            // 
            // _actionNameComboBox
            // 
            this._actionNameComboBox.FormattingEnabled = true;
            this._actionNameComboBox.HideSelection = false;
            this._actionNameComboBox.Location = new System.Drawing.Point(7, 43);
            this._actionNameComboBox.Name = "_actionNameComboBox";
            this._actionNameComboBox.Size = new System.Drawing.Size(184, 21);
            this._actionNameComboBox.TabIndex = 1;
            // 
            // _fileActionCheckBox
            // 
            this._fileActionCheckBox.AutoSize = true;
            this._fileActionCheckBox.Location = new System.Drawing.Point(7, 19);
            this._fileActionCheckBox.Name = "_fileActionCheckBox";
            this._fileActionCheckBox.Size = new System.Drawing.Size(133, 17);
            this._fileActionCheckBox.TabIndex = 0;
            this._fileActionCheckBox.Text = "Set file action status of";
            this._fileActionCheckBox.UseVisualStyleBackColor = true;
            this._fileActionCheckBox.CheckedChanged += new System.EventHandler(this.HandleFileActionCheckBoxCheckedChanged);
            // 
            // _tagSettingsButton
            // 
            this._tagSettingsButton.Location = new System.Drawing.Point(236, 15);
            this._tagSettingsButton.Name = "_tagSettingsButton";
            this._tagSettingsButton.Size = new System.Drawing.Size(75, 23);
            this._tagSettingsButton.TabIndex = 1;
            this._tagSettingsButton.Text = "Settings...";
            this._tagSettingsButton.UseVisualStyleBackColor = true;
            this._tagSettingsButton.Click += new System.EventHandler(this.HandleTagSettingsButtonClick);
            // 
            // _allowTagsCheckBox
            // 
            this._allowTagsCheckBox.AutoSize = true;
            this._allowTagsCheckBox.Location = new System.Drawing.Point(6, 19);
            this._allowTagsCheckBox.Name = "_allowTagsCheckBox";
            this._allowTagsCheckBox.Size = new System.Drawing.Size(217, 17);
            this._allowTagsCheckBox.TabIndex = 0;
            this._allowTagsCheckBox.Text = "Allow user to apply tags to the document";
            this._allowTagsCheckBox.UseVisualStyleBackColor = true;
            this._allowTagsCheckBox.CheckedChanged += new System.EventHandler(this.HandleAllowTagsCheckBox_CheckedChanged);
            // 
            // _seamlessNavigationCheckBox
            // 
            this._seamlessNavigationCheckBox.AutoSize = true;
            this._seamlessNavigationCheckBox.Location = new System.Drawing.Point(6, 19);
            this._seamlessNavigationCheckBox.Name = "_seamlessNavigationCheckBox";
            this._seamlessNavigationCheckBox.Size = new System.Drawing.Size(283, 17);
            this._seamlessNavigationCheckBox.TabIndex = 0;
            this._seamlessNavigationCheckBox.Text = "Enable seamless page navigation between documents";
            this._seamlessNavigationCheckBox.UseVisualStyleBackColor = true;
            // 
            // _verifyAllItemsCheckBox
            // 
            this._verifyAllItemsCheckBox.AutoSize = true;
            this._verifyAllItemsCheckBox.Location = new System.Drawing.Point(6, 31);
            this._verifyAllItemsCheckBox.Name = "_verifyAllItemsCheckBox";
            this._verifyAllItemsCheckBox.Size = new System.Drawing.Size(299, 17);
            this._verifyAllItemsCheckBox.TabIndex = 1;
            this._verifyAllItemsCheckBox.Text = "Require every suggested redaction or clue to be reviewed";
            this._verifyAllItemsCheckBox.UseVisualStyleBackColor = true;
            this._verifyAllItemsCheckBox.CheckedChanged += new System.EventHandler(this.HandleCheckBox_CheckedChanged);
            // 
            // _launchFullScreenCheckBox
            // 
            this._launchFullScreenCheckBox.AutoSize = true;
            this._launchFullScreenCheckBox.Location = new System.Drawing.Point(6, 42);
            this._launchFullScreenCheckBox.Name = "_launchFullScreenCheckBox";
            this._launchFullScreenCheckBox.Size = new System.Drawing.Size(236, 17);
            this._launchFullScreenCheckBox.TabIndex = 1;
            this._launchFullScreenCheckBox.Text = "Open verification window in full screen mode";
            this._launchFullScreenCheckBox.UseVisualStyleBackColor = true;
            // 
            // _feedbackSettingsButton
            // 
            this._feedbackSettingsButton.Location = new System.Drawing.Point(273, 19);
            this._feedbackSettingsButton.Name = "_feedbackSettingsButton";
            this._feedbackSettingsButton.Size = new System.Drawing.Size(150, 23);
            this._feedbackSettingsButton.TabIndex = 1;
            this._feedbackSettingsButton.Text = "Feedback settings...";
            this._feedbackSettingsButton.UseVisualStyleBackColor = true;
            this._feedbackSettingsButton.Click += new System.EventHandler(this.HandleFeedbackSettingsButtonClick);
            // 
            // _collectFeedbackCheckBox
            // 
            this._collectFeedbackCheckBox.AutoSize = true;
            this._collectFeedbackCheckBox.Location = new System.Drawing.Point(6, 23);
            this._collectFeedbackCheckBox.Name = "_collectFeedbackCheckBox";
            this._collectFeedbackCheckBox.Size = new System.Drawing.Size(261, 17);
            this._collectFeedbackCheckBox.TabIndex = 0;
            this._collectFeedbackCheckBox.Text = "Enable collection of redaction accuracy feedback";
            this._collectFeedbackCheckBox.UseVisualStyleBackColor = true;
            this._collectFeedbackCheckBox.CheckedChanged += new System.EventHandler(this.HandleCollectFeedbackCheckBoxCheckedChanged);
            // 
            // _requireExemptionsCheckBox
            // 
            this._requireExemptionsCheckBox.AutoSize = true;
            this._requireExemptionsCheckBox.Location = new System.Drawing.Point(6, 106);
            this._requireExemptionsCheckBox.Name = "_requireExemptionsCheckBox";
            this._requireExemptionsCheckBox.Size = new System.Drawing.Size(218, 17);
            this._requireExemptionsCheckBox.TabIndex = 4;
            this._requireExemptionsCheckBox.Text = "Require exemption codes to be specified";
            this._requireExemptionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // _requireTypeCheckBox
            // 
            this._requireTypeCheckBox.AutoSize = true;
            this._requireTypeCheckBox.Location = new System.Drawing.Point(6, 81);
            this._requireTypeCheckBox.Name = "_requireTypeCheckBox";
            this._requireTypeCheckBox.Size = new System.Drawing.Size(205, 17);
            this._requireTypeCheckBox.TabIndex = 3;
            this._requireTypeCheckBox.Text = "Require redaction type to be specified";
            this._requireTypeCheckBox.UseVisualStyleBackColor = true;
            // 
            // _verifyAllPagesCheckBox
            // 
            this._verifyAllPagesCheckBox.AutoSize = true;
            this._verifyAllPagesCheckBox.Location = new System.Drawing.Point(6, 6);
            this._verifyAllPagesCheckBox.Name = "_verifyAllPagesCheckBox";
            this._verifyAllPagesCheckBox.Size = new System.Drawing.Size(168, 17);
            this._verifyAllPagesCheckBox.TabIndex = 0;
            this._verifyAllPagesCheckBox.Text = "Require all pages to be visited";
            this._verifyAllPagesCheckBox.UseVisualStyleBackColor = true;
            this._verifyAllPagesCheckBox.CheckedChanged += new System.EventHandler(this.HandleCheckBox_CheckedChanged);
            // 
            // _promptForSaveUntilCommit
            // 
            this._promptForSaveUntilCommit.AutoSize = true;
            this._promptForSaveUntilCommit.Location = new System.Drawing.Point(6, 131);
            this._promptForSaveUntilCommit.Name = "_promptForSaveUntilCommit";
            this._promptForSaveUntilCommit.Size = new System.Drawing.Size(398, 17);
            this._promptForSaveUntilCommit.TabIndex = 5;
            this._promptForSaveUntilCommit.Text = "Prompt to save changes before navigating away from uncommitted documents.";
            this._promptForSaveUntilCommit.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(391, 343);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Location = new System.Drawing.Point(310, 343);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 1;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(454, 325);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox5);
            this.tabPage1.Controls.Add(groupBox2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(446, 299);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this._allowTagsCheckBox);
            this.groupBox5.Controls.Add(this._tagSettingsButton);
            this.groupBox5.Location = new System.Drawing.Point(6, 6);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(429, 49);
            this.groupBox5.TabIndex = 0;
            this.groupBox5.TabStop = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this._requireTypeCheckBox);
            this.tabPage2.Controls.Add(this._verifyFullPageCluesCheckBox);
            this.tabPage2.Controls.Add(this._verifyAllItemsCheckBox);
            this.tabPage2.Controls.Add(this._verifyAllPagesCheckBox);
            this.tabPage2.Controls.Add(this._requireExemptionsCheckBox);
            this.tabPage2.Controls.Add(this._promptForSaveUntilCommit);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(446, 299);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Validation";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // _verifyFullPageCluesCheckBox
            // 
            this._verifyFullPageCluesCheckBox.AutoSize = true;
            this._verifyFullPageCluesCheckBox.Location = new System.Drawing.Point(6, 56);
            this._verifyFullPageCluesCheckBox.Name = "_verifyFullPageCluesCheckBox";
            this._verifyFullPageCluesCheckBox.Size = new System.Drawing.Size(155, 17);
            this._verifyFullPageCluesCheckBox.TabIndex = 2;
            this._verifyFullPageCluesCheckBox.Text = "Review full page clues only";
            this._verifyFullPageCluesCheckBox.UseVisualStyleBackColor = true;
            this._verifyFullPageCluesCheckBox.CheckedChanged += new System.EventHandler(this.HandleCheckBox_CheckedChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox1);
            this.tabPage3.Controls.Add(this.groupBox6);
            this.tabPage3.Controls.Add(groupBox4);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(446, 299);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "View/Navigation";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._redactionQaExplanatoryLabel);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this._redactionVerifyExplanatoryTextLabel);
            this.groupBox1.Controls.Add(this._redactionQaComboBox);
            this.groupBox1.Controls.Add(this._redactionQaRadioButton);
            this.groupBox1.Controls.Add(this._redactionVerificationRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(6, 85);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(429, 151);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Verification mode";
            // 
            // _redactionQaExplanatoryLabel
            // 
            this._redactionQaExplanatoryLabel.AutoSize = true;
            this._redactionQaExplanatoryLabel.Location = new System.Drawing.Point(22, 114);
            this._redactionQaExplanatoryLabel.MaximumSize = new System.Drawing.Size(382, 0);
            this._redactionQaExplanatoryLabel.Name = "_redactionQaExplanatoryLabel";
            this._redactionQaExplanatoryLabel.Size = new System.Drawing.Size(360, 26);
            this._redactionQaExplanatoryLabel.TabIndex = 7;
            this._redactionQaExplanatoryLabel.Text = "(You will see viewed status of redaction items and pages as of the time the docum" +
    "entation most recently verified)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(104, 90);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(211, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "viewed status of redaction items and pages";
            // 
            // _redactionVerifyExplanatoryTextLabel
            // 
            this._redactionVerifyExplanatoryTextLabel.AutoSize = true;
            this._redactionVerifyExplanatoryTextLabel.Location = new System.Drawing.Point(20, 39);
            this._redactionVerifyExplanatoryTextLabel.Name = "_redactionVerifyExplanatoryTextLabel";
            this._redactionVerifyExplanatoryTextLabel.Size = new System.Drawing.Size(382, 13);
            this._redactionVerifyExplanatoryTextLabel.TabIndex = 1;
            this._redactionVerifyExplanatoryTextLabel.Text = "(In this mode you will automatically continue verification where it was last left" +
    " off)";
            // 
            // _redactionQaComboBox
            // 
            this._redactionQaComboBox.AutoCompleteCustomSource.AddRange(new string[] {
            "Preserve",
            "Reset"});
            this._redactionQaComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._redactionQaComboBox.Enabled = false;
            this._redactionQaComboBox.FormattingEnabled = true;
            this._redactionQaComboBox.Items.AddRange(new object[] {
            "Preserve",
            "Reset"});
            this._redactionQaComboBox.Location = new System.Drawing.Point(25, 87);
            this._redactionQaComboBox.Name = "_redactionQaComboBox";
            this._redactionQaComboBox.Size = new System.Drawing.Size(73, 21);
            this._redactionQaComboBox.TabIndex = 3;
            this._redactionQaComboBox.SelectedIndexChanged += new System.EventHandler(this._redactionQaComboBox_SelectedIndexChanged);
            // 
            // _redactionQaRadioButton
            // 
            this._redactionQaRadioButton.AutoSize = true;
            this._redactionQaRadioButton.Location = new System.Drawing.Point(6, 64);
            this._redactionQaRadioButton.Name = "_redactionQaRadioButton";
            this._redactionQaRadioButton.Size = new System.Drawing.Size(92, 17);
            this._redactionQaRadioButton.TabIndex = 2;
            this._redactionQaRadioButton.Text = "Redaction QA";
            this._redactionQaRadioButton.UseVisualStyleBackColor = true;
            this._redactionQaRadioButton.CheckedChanged += new System.EventHandler(this._redactionQaRadioButton_CheckedChanged);
            // 
            // _redactionVerificationRadioButton
            // 
            this._redactionVerificationRadioButton.AutoSize = true;
            this._redactionVerificationRadioButton.Checked = true;
            this._redactionVerificationRadioButton.Location = new System.Drawing.Point(6, 19);
            this._redactionVerificationRadioButton.Name = "_redactionVerificationRadioButton";
            this._redactionVerificationRadioButton.Size = new System.Drawing.Size(128, 17);
            this._redactionVerificationRadioButton.TabIndex = 0;
            this._redactionVerificationRadioButton.TabStop = true;
            this._redactionVerificationRadioButton.Text = "Redaction verification";
            this._redactionVerificationRadioButton.UseVisualStyleBackColor = true;
            this._redactionVerificationRadioButton.CheckedChanged += new System.EventHandler(this._redactionVerificationRadioButton_CheckedChanged);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this._seamlessNavigationCheckBox);
            this.groupBox6.Controls.Add(this._launchFullScreenCheckBox);
            this.groupBox6.Location = new System.Drawing.Point(6, 6);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(429, 73);
            this.groupBox6.TabIndex = 0;
            this.groupBox6.TabStop = false;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.groupBox7);
            this.tabPage4.Controls.Add(groupBox3);
            this.tabPage4.Controls.Add(this._dataFileControl);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(446, 299);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Advanced";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this._feedbackSettingsButton);
            this.groupBox7.Controls.Add(this._collectFeedbackCheckBox);
            this.groupBox7.Location = new System.Drawing.Point(6, 6);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(429, 60);
            this.groupBox7.TabIndex = 0;
            this.groupBox7.TabStop = false;
            // 
            // _dataFileControl
            // 
            this._dataFileControl.Location = new System.Drawing.Point(6, 78);
            this._dataFileControl.Name = "_dataFileControl";
            this._dataFileControl.Size = new System.Drawing.Size(429, 60);
            this._dataFileControl.TabIndex = 0;
            // 
            // VerificationSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(476, 376);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VerificationSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Redaction: Verify sensitive data settings";
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox _verifyAllPagesCheckBox;
        private System.Windows.Forms.Button _feedbackSettingsButton;
        private System.Windows.Forms.CheckBox _collectFeedbackCheckBox;
        private System.Windows.Forms.CheckBox _requireExemptionsCheckBox;
        private System.Windows.Forms.CheckBox _requireTypeCheckBox;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private DataFileControl _dataFileControl;
        private System.Windows.Forms.CheckBox _backdropImageCheckBox;
        private Extract.FileActionManager.Forms.FileActionManagerPathTagButton _backdropImagePathTagsButton;
        private Extract.Utilities.Forms.BrowseButton _backdropImageBrowseButton;
        private System.Windows.Forms.TextBox _backdropImageTextBox;
        private System.Windows.Forms.CheckBox _launchFullScreenCheckBox;
        private System.Windows.Forms.Button _slideshowSettingsButton;
        private System.Windows.Forms.CheckBox _enableSlideshowCheckBox;
        private System.Windows.Forms.CheckBox _seamlessNavigationCheckBox;
        private System.Windows.Forms.CheckBox _verifyAllItemsCheckBox;
        private System.Windows.Forms.CheckBox _promptForSaveUntilCommit;
        private System.Windows.Forms.CheckBox _allowTagsCheckBox;
        private System.Windows.Forms.Button _tagSettingsButton;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox5;
        private FileActionManager.Forms.FileActionManagerPathTagButton _actionNamePathTagsButton;
        private System.Windows.Forms.ComboBox _actionStatusComboBox;
        private Utilities.Forms.BetterComboBox _actionNameComboBox;
        private System.Windows.Forms.CheckBox _fileActionCheckBox;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox _redactionQaComboBox;
        private System.Windows.Forms.RadioButton _redactionQaRadioButton;
        private System.Windows.Forms.RadioButton _redactionVerificationRadioButton;
        private System.Windows.Forms.Label _redactionVerifyExplanatoryTextLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label _redactionQaExplanatoryLabel;
        private System.Windows.Forms.CheckBox _verifyFullPageCluesCheckBox;
    }
}