namespace Extract.FileActionManager.Database
{
    partial class FAMDatabaseOptionsDialog
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
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label8;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FAMDatabaseOptionsDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this._tabControlSettings = new System.Windows.Forms.TabControl();
            this._tabGeneral = new System.Windows.Forms.TabPage();
            this._alternateComponentDataDirectoryTextBox = new System.Windows.Forms.TextBox();
            this._numberMaxTimeBetweenChecks = new Extract.Utilities.Forms.NumericEntryTextBox();
            this._numberMinTimeBetweenChecks = new Extract.Utilities.Forms.NumericEntryTextBox();
            this._upDownRevertMinutes = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._buttonRemoveEmail = new System.Windows.Forms.Button();
            this._buttonModifyEmail = new System.Windows.Forms.Button();
            this._buttonAddEmail = new System.Windows.Forms.Button();
            this._listAutoRevertEmailList = new System.Windows.Forms.ListBox();
            this._checkAutoDeleteFileActionComments = new System.Windows.Forms.CheckBox();
            this._checkAutoCreateActions = new System.Windows.Forms.CheckBox();
            this._checkAllowdynamicTagCreation = new System.Windows.Forms.CheckBox();
            this._tabHistory = new System.Windows.Forms.TabPage();
            this._checkStoreFTPEventHistory = new System.Windows.Forms.CheckBox();
            this._checkStoreDBSettingsChangeHistory = new System.Windows.Forms.CheckBox();
            this._checkStoreDocTagHistory = new System.Windows.Forms.CheckBox();
            this._checkStoreQueueEventHistory = new System.Windows.Forms.CheckBox();
            this._checkStoreFASTHistory = new System.Windows.Forms.CheckBox();
            this._checkStoreSourceDocChangeHistory = new System.Windows.Forms.CheckBox();
            this._tabSecurity = new System.Windows.Forms.TabPage();
            this._noteAboutAzureTabLabel = new System.Windows.Forms.Label();
            this._configurePasswordRequirementsButton = new System.Windows.Forms.Button();
            this._buttonRemoveMachine = new System.Windows.Forms.Button();
            this._buttonModifyMachine = new System.Windows.Forms.Button();
            this._buttonAddMachine = new System.Windows.Forms.Button();
            this._listMachinesToAuthenticate = new System.Windows.Forms.ListBox();
            this._checkRequireAuthenticationToRun = new System.Windows.Forms.CheckBox();
            this._checkRequirePasswordForSkipped = new System.Windows.Forms.CheckBox();
            this._tabVerification = new System.Windows.Forms.TabPage();
            this._verificationLayout = new System.Windows.Forms.FlowLayoutPanel();
            this._groupVerificationAll = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this._checkSessionTimeout = new System.Windows.Forms.CheckBox();
            this._numberSessionTimeout = new System.Windows.Forms.NumericUpDown();
            this._groupDataEntry = new System.Windows.Forms.GroupBox();
            this._checkDataEntryEnableCounters = new System.Windows.Forms.CheckBox();
            this._tabEmail = new System.Windows.Forms.TabPage();
            this._emailSettingsControl = new Extract.Utilities.Email.EmailSettingsControl();
            this.tabDashboard = new System.Windows.Forms.TabPage();
            this.textBoxDashboardExcludeFilter = new System.Windows.Forms.TextBox();
            this.textBoxDashboardIncludeFilter = new System.Windows.Forms.TextBox();
            this._tabPageAzure = new System.Windows.Forms.TabPage();
            this._azureApplicationGroupBox = new System.Windows.Forms.GroupBox();
            this._azureApplicationTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._azureClientIDLabel = new System.Windows.Forms.Label();
            this._azureInstance = new System.Windows.Forms.TextBox();
            this._azureTenantLabel = new System.Windows.Forms.Label();
            this._azureTenant = new System.Windows.Forms.TextBox();
            this._azureInstanceLabel = new System.Windows.Forms.Label();
            this._azureClientID = new System.Windows.Forms.TextBox();
            this._azureEmailFileSupplierGroupBox = new System.Windows.Forms.GroupBox();
            this._azureEmailFileSupplierTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._usernameInfoTip = new Extract.Utilities.Forms.InfoTip();
            this._azureEmailFileSupplierValidateCredentialsButton = new System.Windows.Forms.Button();
            this._azureEmailFileSupplierPasswordTextBox = new System.Windows.Forms.TextBox();
            this._azureEmailFileSupplierUserNameLabel = new System.Windows.Forms.Label();
            this._azureEmailFileSupplierPasswordLabel = new System.Windows.Forms.Label();
            this._azureEmailFileSupplierUserNameTextBox = new System.Windows.Forms.TextBox();
            this._azureEmailFileSupplierShowHidePasswordButton = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOK = new System.Windows.Forms.Button();
            this._buttonRefresh = new System.Windows.Forms.Button();
            this._emailTestButton = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            this._tabControlSettings.SuspendLayout();
            this._tabGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._upDownRevertMinutes)).BeginInit();
            this._tabHistory.SuspendLayout();
            this._tabSecurity.SuspendLayout();
            this._tabVerification.SuspendLayout();
            this._verificationLayout.SuspendLayout();
            this._groupVerificationAll.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numberSessionTimeout)).BeginInit();
            this._groupDataEntry.SuspendLayout();
            this._tabEmail.SuspendLayout();
            this.tabDashboard.SuspendLayout();
            this._tabPageAzure.SuspendLayout();
            this._azureApplicationGroupBox.SuspendLayout();
            this._azureApplicationTableLayoutPanel.SuspendLayout();
            this._azureEmailFileSupplierGroupBox.SuspendLayout();
            this._azureEmailFileSupplierTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(4, 84);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(353, 13);
            label2.TabIndex = 12;
            label2.Text = "Skip authentication when running as a service on the following machines:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(22, 98);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(305, 13);
            label4.TabIndex = 12;
            label4.Text = "After reverting file status, notify the following recipients by email:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(22, 75);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(170, 13);
            label3.TabIndex = 4;
            label3.Text = "Automatically revert file status after";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(260, 75);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(105, 13);
            label5.TabIndex = 21;
            label5.Text = "minute(s) of inactivity";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(3, 219);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(271, 13);
            label6.TabIndex = 22;
            label6.Text = "Minimum time between checking for files to process (ms)";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(3, 245);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(274, 13);
            label7.TabIndex = 23;
            label7.Text = "Maximum time between checking for files to process (ms)";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(3, 268);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(201, 13);
            label8.TabIndex = 24;
            label8.Text = "Alternate component data (FKB) directory";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Dashboard Include Filter";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 153);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(125, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Dashboard Exclude Filter";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label10.AutoSize = true;
            this.label10.ForeColor = System.Drawing.Color.Red;
            this.label10.Location = new System.Drawing.Point(14, 373);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(367, 13);
            this.label10.TabIndex = 5;
            this.label10.Text = "Services and APIs need to be restarted for settings changes to go into effect";
            // 
            // _tabControlSettings
            // 
            this._tabControlSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tabControlSettings.Controls.Add(this._tabGeneral);
            this._tabControlSettings.Controls.Add(this._tabHistory);
            this._tabControlSettings.Controls.Add(this._tabSecurity);
            this._tabControlSettings.Controls.Add(this._tabVerification);
            this._tabControlSettings.Controls.Add(this._tabEmail);
            this._tabControlSettings.Controls.Add(this.tabDashboard);
            this._tabControlSettings.Controls.Add(this._tabPageAzure);
            this._tabControlSettings.Location = new System.Drawing.Point(13, 13);
            this._tabControlSettings.Name = "_tabControlSettings";
            this._tabControlSettings.SelectedIndex = 0;
            this._tabControlSettings.Size = new System.Drawing.Size(466, 357);
            this._tabControlSettings.TabIndex = 0;
            this._tabControlSettings.SelectedIndexChanged += new System.EventHandler(this.HandleTabControl_SelectedIndexChanged);
            // 
            // _tabGeneral
            // 
            this._tabGeneral.Controls.Add(this._alternateComponentDataDirectoryTextBox);
            this._tabGeneral.Controls.Add(label8);
            this._tabGeneral.Controls.Add(this._numberMaxTimeBetweenChecks);
            this._tabGeneral.Controls.Add(this._numberMinTimeBetweenChecks);
            this._tabGeneral.Controls.Add(label7);
            this._tabGeneral.Controls.Add(label6);
            this._tabGeneral.Controls.Add(this._upDownRevertMinutes);
            this._tabGeneral.Controls.Add(label5);
            this._tabGeneral.Controls.Add(this._buttonRemoveEmail);
            this._tabGeneral.Controls.Add(this._buttonModifyEmail);
            this._tabGeneral.Controls.Add(this._buttonAddEmail);
            this._tabGeneral.Controls.Add(this._listAutoRevertEmailList);
            this._tabGeneral.Controls.Add(label4);
            this._tabGeneral.Controls.Add(label3);
            this._tabGeneral.Controls.Add(this._checkAutoDeleteFileActionComments);
            this._tabGeneral.Controls.Add(this._checkAutoCreateActions);
            this._tabGeneral.Controls.Add(this._checkAllowdynamicTagCreation);
            this._tabGeneral.Location = new System.Drawing.Point(4, 22);
            this._tabGeneral.Name = "_tabGeneral";
            this._tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this._tabGeneral.Size = new System.Drawing.Size(458, 331);
            this._tabGeneral.TabIndex = 0;
            this._tabGeneral.Text = "General";
            this._tabGeneral.ToolTipText = "General FAM database settings";
            this._tabGeneral.UseVisualStyleBackColor = true;
            // 
            // _alternateComponentDataDirectoryTextBox
            // 
            this._alternateComponentDataDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._alternateComponentDataDirectoryTextBox.Location = new System.Drawing.Point(6, 284);
            this._alternateComponentDataDirectoryTextBox.Name = "_alternateComponentDataDirectoryTextBox";
            this._alternateComponentDataDirectoryTextBox.Size = new System.Drawing.Size(446, 20);
            this._alternateComponentDataDirectoryTextBox.TabIndex = 25;
            // 
            // _numberMaxTimeBetweenChecks
            // 
            this._numberMaxTimeBetweenChecks.Location = new System.Drawing.Point(297, 242);
            this._numberMaxTimeBetweenChecks.MaximumValue = 1.7976931348623157E+308D;
            this._numberMaxTimeBetweenChecks.MinimumValue = -1.7976931348623157E+308D;
            this._numberMaxTimeBetweenChecks.Name = "_numberMaxTimeBetweenChecks";
            this._numberMaxTimeBetweenChecks.Size = new System.Drawing.Size(55, 20);
            this._numberMaxTimeBetweenChecks.TabIndex = 11;
            // 
            // _numberMinTimeBetweenChecks
            // 
            this._numberMinTimeBetweenChecks.AllowNegative = false;
            this._numberMinTimeBetweenChecks.Location = new System.Drawing.Point(297, 216);
            this._numberMinTimeBetweenChecks.MaximumValue = 1.7976931348623157E+308D;
            this._numberMinTimeBetweenChecks.MinimumValue = -1.7976931348623157E+308D;
            this._numberMinTimeBetweenChecks.Name = "_numberMinTimeBetweenChecks";
            this._numberMinTimeBetweenChecks.Size = new System.Drawing.Size(55, 20);
            this._numberMinTimeBetweenChecks.TabIndex = 10;
            // 
            // _upDownRevertMinutes
            // 
            this._upDownRevertMinutes.Location = new System.Drawing.Point(205, 73);
            this._upDownRevertMinutes.Maximum = new decimal(new int[] {
            1440,
            0,
            0,
            0});
            this._upDownRevertMinutes.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this._upDownRevertMinutes.Name = "_upDownRevertMinutes";
            this._upDownRevertMinutes.Size = new System.Drawing.Size(49, 20);
            this._upDownRevertMinutes.TabIndex = 5;
            this._upDownRevertMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this._upDownRevertMinutes.Value = new decimal(new int[] {
            1440,
            0,
            0,
            0});
            this._upDownRevertMinutes.UserTextCorrected += new System.EventHandler<System.EventArgs>(this.HandleAutoRevertValueCorrectedEvent);
            // 
            // _buttonRemoveEmail
            // 
            this._buttonRemoveEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonRemoveEmail.Location = new System.Drawing.Point(377, 173);
            this._buttonRemoveEmail.Name = "_buttonRemoveEmail";
            this._buttonRemoveEmail.Size = new System.Drawing.Size(75, 23);
            this._buttonRemoveEmail.TabIndex = 9;
            this._buttonRemoveEmail.Text = "Remove";
            this._buttonRemoveEmail.UseVisualStyleBackColor = true;
            this._buttonRemoveEmail.Click += new System.EventHandler(this.HandleAutoRevertEmailRemoveClicked);
            // 
            // _buttonModifyEmail
            // 
            this._buttonModifyEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonModifyEmail.Location = new System.Drawing.Point(377, 144);
            this._buttonModifyEmail.Name = "_buttonModifyEmail";
            this._buttonModifyEmail.Size = new System.Drawing.Size(75, 23);
            this._buttonModifyEmail.TabIndex = 8;
            this._buttonModifyEmail.Text = "Modify...";
            this._buttonModifyEmail.UseVisualStyleBackColor = true;
            this._buttonModifyEmail.Click += new System.EventHandler(this.HandleModifyAutoRevertEmailClicked);
            // 
            // _buttonAddEmail
            // 
            this._buttonAddEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonAddEmail.Location = new System.Drawing.Point(377, 115);
            this._buttonAddEmail.Name = "_buttonAddEmail";
            this._buttonAddEmail.Size = new System.Drawing.Size(75, 23);
            this._buttonAddEmail.TabIndex = 7;
            this._buttonAddEmail.Text = "Add...";
            this._buttonAddEmail.UseVisualStyleBackColor = true;
            this._buttonAddEmail.Click += new System.EventHandler(this.HandleAddAutoRevertEmailClicked);
            // 
            // _listAutoRevertEmailList
            // 
            this._listAutoRevertEmailList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._listAutoRevertEmailList.FormattingEnabled = true;
            this._listAutoRevertEmailList.Location = new System.Drawing.Point(25, 115);
            this._listAutoRevertEmailList.Name = "_listAutoRevertEmailList";
            this._listAutoRevertEmailList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this._listAutoRevertEmailList.Size = new System.Drawing.Size(345, 95);
            this._listAutoRevertEmailList.TabIndex = 6;
            this._listAutoRevertEmailList.SelectedIndexChanged += new System.EventHandler(this.HandleAutoRevertSelectionChanged);
            // 
            // _checkAutoDeleteFileActionComments
            // 
            this._checkAutoDeleteFileActionComments.AutoSize = true;
            this._checkAutoDeleteFileActionComments.Location = new System.Drawing.Point(6, 52);
            this._checkAutoDeleteFileActionComments.Name = "_checkAutoDeleteFileActionComments";
            this._checkAutoDeleteFileActionComments.Size = new System.Drawing.Size(288, 17);
            this._checkAutoDeleteFileActionComments.TabIndex = 2;
            this._checkAutoDeleteFileActionComments.Text = "Automatically delete file action comments on completion";
            this._checkAutoDeleteFileActionComments.UseVisualStyleBackColor = true;
            // 
            // _checkAutoCreateActions
            // 
            this._checkAutoCreateActions.AutoSize = true;
            this._checkAutoCreateActions.Location = new System.Drawing.Point(6, 29);
            this._checkAutoCreateActions.Name = "_checkAutoCreateActions";
            this._checkAutoCreateActions.Size = new System.Drawing.Size(258, 17);
            this._checkAutoCreateActions.TabIndex = 1;
            this._checkAutoCreateActions.Text = "Automatically create actions specified in FPS files";
            this._checkAutoCreateActions.UseVisualStyleBackColor = true;
            // 
            // _checkAllowdynamicTagCreation
            // 
            this._checkAllowdynamicTagCreation.AutoSize = true;
            this._checkAllowdynamicTagCreation.Location = new System.Drawing.Point(6, 6);
            this._checkAllowdynamicTagCreation.Name = "_checkAllowdynamicTagCreation";
            this._checkAllowdynamicTagCreation.Size = new System.Drawing.Size(152, 17);
            this._checkAllowdynamicTagCreation.TabIndex = 0;
            this._checkAllowdynamicTagCreation.Text = "Allow dynamic tag creation";
            this._checkAllowdynamicTagCreation.UseVisualStyleBackColor = true;
            // 
            // _tabHistory
            // 
            this._tabHistory.Controls.Add(this._checkStoreFTPEventHistory);
            this._tabHistory.Controls.Add(this._checkStoreDBSettingsChangeHistory);
            this._tabHistory.Controls.Add(this._checkStoreDocTagHistory);
            this._tabHistory.Controls.Add(this._checkStoreQueueEventHistory);
            this._tabHistory.Controls.Add(this._checkStoreFASTHistory);
            this._tabHistory.Controls.Add(this._checkStoreSourceDocChangeHistory);
            this._tabHistory.Location = new System.Drawing.Point(4, 22);
            this._tabHistory.Name = "_tabHistory";
            this._tabHistory.Padding = new System.Windows.Forms.Padding(3);
            this._tabHistory.Size = new System.Drawing.Size(458, 331);
            this._tabHistory.TabIndex = 4;
            this._tabHistory.Text = "History";
            this._tabHistory.ToolTipText = "FAM history settings";
            this._tabHistory.UseVisualStyleBackColor = true;
            // 
            // _checkStoreFTPEventHistory
            // 
            this._checkStoreFTPEventHistory.AutoSize = true;
            this._checkStoreFTPEventHistory.Location = new System.Drawing.Point(6, 121);
            this._checkStoreFTPEventHistory.Name = "_checkStoreFTPEventHistory";
            this._checkStoreFTPEventHistory.Size = new System.Drawing.Size(137, 17);
            this._checkStoreFTPEventHistory.TabIndex = 21;
            this._checkStoreFTPEventHistory.Text = "Store FTP event history";
            this._checkStoreFTPEventHistory.UseVisualStyleBackColor = true;
            // 
            // _checkStoreDBSettingsChangeHistory
            // 
            this._checkStoreDBSettingsChangeHistory.AutoSize = true;
            this._checkStoreDBSettingsChangeHistory.Location = new System.Drawing.Point(6, 98);
            this._checkStoreDBSettingsChangeHistory.Name = "_checkStoreDBSettingsChangeHistory";
            this._checkStoreDBSettingsChangeHistory.Size = new System.Drawing.Size(209, 17);
            this._checkStoreDBSettingsChangeHistory.TabIndex = 7;
            this._checkStoreDBSettingsChangeHistory.Text = "Store database settings change history";
            this._checkStoreDBSettingsChangeHistory.UseVisualStyleBackColor = true;
            // 
            // _checkStoreDocTagHistory
            // 
            this._checkStoreDocTagHistory.AutoSize = true;
            this._checkStoreDocTagHistory.Location = new System.Drawing.Point(6, 75);
            this._checkStoreDocTagHistory.Name = "_checkStoreDocTagHistory";
            this._checkStoreDocTagHistory.Size = new System.Drawing.Size(152, 17);
            this._checkStoreDocTagHistory.TabIndex = 3;
            this._checkStoreDocTagHistory.Text = "Store document tag history";
            this._checkStoreDocTagHistory.UseVisualStyleBackColor = true;
            // 
            // _checkStoreQueueEventHistory
            // 
            this._checkStoreQueueEventHistory.AutoSize = true;
            this._checkStoreQueueEventHistory.Location = new System.Drawing.Point(6, 29);
            this._checkStoreQueueEventHistory.Name = "_checkStoreQueueEventHistory";
            this._checkStoreQueueEventHistory.Size = new System.Drawing.Size(147, 17);
            this._checkStoreQueueEventHistory.TabIndex = 1;
            this._checkStoreQueueEventHistory.Text = "Store queue event history";
            this._checkStoreQueueEventHistory.UseVisualStyleBackColor = true;
            // 
            // _checkStoreFASTHistory
            // 
            this._checkStoreFASTHistory.AutoSize = true;
            this._checkStoreFASTHistory.Location = new System.Drawing.Point(6, 6);
            this._checkStoreFASTHistory.Name = "_checkStoreFASTHistory";
            this._checkStoreFASTHistory.Size = new System.Drawing.Size(203, 17);
            this._checkStoreFASTHistory.TabIndex = 0;
            this._checkStoreFASTHistory.Text = "Store file action state transition history";
            this._checkStoreFASTHistory.UseVisualStyleBackColor = true;
            // 
            // _checkStoreSourceDocChangeHistory
            // 
            this._checkStoreSourceDocChangeHistory.AutoSize = true;
            this._checkStoreSourceDocChangeHistory.Location = new System.Drawing.Point(6, 52);
            this._checkStoreSourceDocChangeHistory.Name = "_checkStoreSourceDocChangeHistory";
            this._checkStoreSourceDocChangeHistory.Size = new System.Drawing.Size(237, 17);
            this._checkStoreSourceDocChangeHistory.TabIndex = 2;
            this._checkStoreSourceDocChangeHistory.Text = "Store source document name change history";
            this._checkStoreSourceDocChangeHistory.UseVisualStyleBackColor = true;
            // 
            // _tabSecurity
            // 
            this._tabSecurity.Controls.Add(this._noteAboutAzureTabLabel);
            this._tabSecurity.Controls.Add(this._configurePasswordRequirementsButton);
            this._tabSecurity.Controls.Add(this._buttonRemoveMachine);
            this._tabSecurity.Controls.Add(this._buttonModifyMachine);
            this._tabSecurity.Controls.Add(this._buttonAddMachine);
            this._tabSecurity.Controls.Add(this._listMachinesToAuthenticate);
            this._tabSecurity.Controls.Add(label2);
            this._tabSecurity.Controls.Add(this._checkRequireAuthenticationToRun);
            this._tabSecurity.Controls.Add(this._checkRequirePasswordForSkipped);
            this._tabSecurity.Location = new System.Drawing.Point(4, 22);
            this._tabSecurity.Name = "_tabSecurity";
            this._tabSecurity.Padding = new System.Windows.Forms.Padding(3);
            this._tabSecurity.Size = new System.Drawing.Size(458, 331);
            this._tabSecurity.TabIndex = 1;
            this._tabSecurity.Text = "Security";
            this._tabSecurity.ToolTipText = "FAM security settings";
            this._tabSecurity.UseVisualStyleBackColor = true;
            // 
            // _noteAboutAzureTabLabel
            // 
            this._noteAboutAzureTabLabel.AutoSize = true;
            this._noteAboutAzureTabLabel.Location = new System.Drawing.Point(6, 198);
            this._noteAboutAzureTabLabel.Name = "_noteAboutAzureTabLabel";
            this._noteAboutAzureTabLabel.Size = new System.Drawing.Size(230, 13);
            this._noteAboutAzureTabLabel.TabIndex = 17;
            this._noteAboutAzureTabLabel.Text = "(See Azure tab for more authentication settings)";
            // 
            // _configurePasswordRequirementsButton
            // 
            this._configurePasswordRequirementsButton.AutoSize = true;
            this._configurePasswordRequirementsButton.Location = new System.Drawing.Point(6, 6);
            this._configurePasswordRequirementsButton.Name = "_configurePasswordRequirementsButton";
            this._configurePasswordRequirementsButton.Size = new System.Drawing.Size(182, 23);
            this._configurePasswordRequirementsButton.TabIndex = 0;
            this._configurePasswordRequirementsButton.Text = "Configure password requirements...";
            this._configurePasswordRequirementsButton.UseVisualStyleBackColor = true;
            this._configurePasswordRequirementsButton.Click += new System.EventHandler(this.HandleConfigurePasswordRequirementsButton_Click);
            // 
            // _buttonRemoveMachine
            // 
            this._buttonRemoveMachine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonRemoveMachine.Location = new System.Drawing.Point(375, 158);
            this._buttonRemoveMachine.Name = "_buttonRemoveMachine";
            this._buttonRemoveMachine.Size = new System.Drawing.Size(75, 23);
            this._buttonRemoveMachine.TabIndex = 16;
            this._buttonRemoveMachine.Text = "Remove";
            this._buttonRemoveMachine.UseVisualStyleBackColor = true;
            this._buttonRemoveMachine.Click += new System.EventHandler(this.HandleRemoveMachineNamesClicked);
            // 
            // _buttonModifyMachine
            // 
            this._buttonModifyMachine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonModifyMachine.Location = new System.Drawing.Point(375, 129);
            this._buttonModifyMachine.Name = "_buttonModifyMachine";
            this._buttonModifyMachine.Size = new System.Drawing.Size(75, 23);
            this._buttonModifyMachine.TabIndex = 15;
            this._buttonModifyMachine.Text = "Modify...";
            this._buttonModifyMachine.UseVisualStyleBackColor = true;
            this._buttonModifyMachine.Click += new System.EventHandler(this.HandleModifyMachineNameClick);
            // 
            // _buttonAddMachine
            // 
            this._buttonAddMachine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonAddMachine.Location = new System.Drawing.Point(375, 100);
            this._buttonAddMachine.Name = "_buttonAddMachine";
            this._buttonAddMachine.Size = new System.Drawing.Size(75, 23);
            this._buttonAddMachine.TabIndex = 14;
            this._buttonAddMachine.Text = "Add...";
            this._buttonAddMachine.UseVisualStyleBackColor = true;
            this._buttonAddMachine.Click += new System.EventHandler(this.HandleAddMachineNameButtonClicked);
            // 
            // _listMachinesToAuthenticate
            // 
            this._listMachinesToAuthenticate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._listMachinesToAuthenticate.FormattingEnabled = true;
            this._listMachinesToAuthenticate.Location = new System.Drawing.Point(7, 100);
            this._listMachinesToAuthenticate.Name = "_listMachinesToAuthenticate";
            this._listMachinesToAuthenticate.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this._listMachinesToAuthenticate.Size = new System.Drawing.Size(362, 95);
            this._listMachinesToAuthenticate.TabIndex = 13;
            this._listMachinesToAuthenticate.SelectedIndexChanged += new System.EventHandler(this.HandleMachineToSkipSelectionChanged);
            // 
            // _checkRequireAuthenticationToRun
            // 
            this._checkRequireAuthenticationToRun.AutoSize = true;
            this._checkRequireAuthenticationToRun.Location = new System.Drawing.Point(7, 61);
            this._checkRequireAuthenticationToRun.Name = "_checkRequireAuthenticationToRun";
            this._checkRequireAuthenticationToRun.Size = new System.Drawing.Size(163, 17);
            this._checkRequireAuthenticationToRun.TabIndex = 5;
            this._checkRequireAuthenticationToRun.Text = "Require authentication to run";
            this._checkRequireAuthenticationToRun.UseVisualStyleBackColor = true;
            // 
            // _checkRequirePasswordForSkipped
            // 
            this._checkRequirePasswordForSkipped.AutoSize = true;
            this._checkRequirePasswordForSkipped.Location = new System.Drawing.Point(7, 38);
            this._checkRequirePasswordForSkipped.Name = "_checkRequirePasswordForSkipped";
            this._checkRequirePasswordForSkipped.Size = new System.Drawing.Size(371, 17);
            this._checkRequirePasswordForSkipped.TabIndex = 1;
            this._checkRequirePasswordForSkipped.Text = "Require FAM database administrator password to process all skipped files";
            this._checkRequirePasswordForSkipped.UseVisualStyleBackColor = true;
            // 
            // _tabVerification
            // 
            this._tabVerification.Controls.Add(this._verificationLayout);
            this._tabVerification.Location = new System.Drawing.Point(4, 22);
            this._tabVerification.Name = "_tabVerification";
            this._tabVerification.Size = new System.Drawing.Size(458, 331);
            this._tabVerification.TabIndex = 5;
            this._tabVerification.Text = "Verification";
            this._tabVerification.ToolTipText = "Verification settings";
            this._tabVerification.UseVisualStyleBackColor = true;
            // 
            // _verificationLayout
            // 
            this._verificationLayout.Controls.Add(this._groupVerificationAll);
            this._verificationLayout.Controls.Add(this._groupDataEntry);
            this._verificationLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._verificationLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this._verificationLayout.Location = new System.Drawing.Point(0, 0);
            this._verificationLayout.Name = "_verificationLayout";
            this._verificationLayout.Size = new System.Drawing.Size(458, 331);
            this._verificationLayout.TabIndex = 1;
            // 
            // _groupVerificationAll
            // 
            this._groupVerificationAll.Controls.Add(this.label11);
            this._groupVerificationAll.Controls.Add(this._checkSessionTimeout);
            this._groupVerificationAll.Controls.Add(this._numberSessionTimeout);
            this._groupVerificationAll.Location = new System.Drawing.Point(3, 3);
            this._groupVerificationAll.Name = "_groupVerificationAll";
            this._groupVerificationAll.Size = new System.Drawing.Size(451, 61);
            this._groupVerificationAll.TabIndex = 1;
            this._groupVerificationAll.TabStop = false;
            this._groupVerificationAll.Text = "All";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(319, 20);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(105, 13);
            this.label11.TabIndex = 7;
            this.label11.Text = "minute(s) of inactivity";
            // 
            // _checkSessionTimeout
            // 
            this._checkSessionTimeout.AutoSize = true;
            this._checkSessionTimeout.Location = new System.Drawing.Point(6, 19);
            this._checkSessionTimeout.Name = "_checkSessionTimeout";
            this._checkSessionTimeout.Size = new System.Drawing.Size(237, 17);
            this._checkSessionTimeout.TabIndex = 5;
            this._checkSessionTimeout.Text = "Automatically close verification sessions after";
            this._checkSessionTimeout.UseVisualStyleBackColor = true;
            // 
            // _numberSessionTimeout
            // 
            this._numberSessionTimeout.DecimalPlaces = 1;
            this._numberSessionTimeout.Enabled = false;
            this._numberSessionTimeout.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this._numberSessionTimeout.Location = new System.Drawing.Point(258, 18);
            this._numberSessionTimeout.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this._numberSessionTimeout.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this._numberSessionTimeout.Name = "_numberSessionTimeout";
            this._numberSessionTimeout.Size = new System.Drawing.Size(55, 20);
            this._numberSessionTimeout.TabIndex = 6;
            this._numberSessionTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this._numberSessionTimeout.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // _groupDataEntry
            // 
            this._groupDataEntry.Controls.Add(this._checkDataEntryEnableCounters);
            this._groupDataEntry.Location = new System.Drawing.Point(3, 70);
            this._groupDataEntry.Name = "_groupDataEntry";
            this._groupDataEntry.Size = new System.Drawing.Size(451, 56);
            this._groupDataEntry.TabIndex = 2;
            this._groupDataEntry.TabStop = false;
            this._groupDataEntry.Text = "Data Entry";
            // 
            // _checkDataEntryEnableCounters
            // 
            this._checkDataEntryEnableCounters.AutoSize = true;
            this._checkDataEntryEnableCounters.Location = new System.Drawing.Point(6, 19);
            this._checkDataEntryEnableCounters.Name = "_checkDataEntryEnableCounters";
            this._checkDataEntryEnableCounters.Size = new System.Drawing.Size(153, 17);
            this._checkDataEntryEnableCounters.TabIndex = 1;
            this._checkDataEntryEnableCounters.Text = "Enable data entry counters";
            this._checkDataEntryEnableCounters.UseVisualStyleBackColor = true;
            // 
            // _tabEmail
            // 
            this._tabEmail.Controls.Add(this._emailSettingsControl);
            this._tabEmail.Location = new System.Drawing.Point(4, 22);
            this._tabEmail.Name = "_tabEmail";
            this._tabEmail.Padding = new System.Windows.Forms.Padding(3);
            this._tabEmail.Size = new System.Drawing.Size(458, 331);
            this._tabEmail.TabIndex = 6;
            this._tabEmail.Text = "Email";
            this._tabEmail.UseVisualStyleBackColor = true;
            // 
            // _emailSettingsControl
            // 
            this._emailSettingsControl.Location = new System.Drawing.Point(3, 3);
            this._emailSettingsControl.Name = "_emailSettingsControl";
            this._emailSettingsControl.Size = new System.Drawing.Size(452, 319);
            this._emailSettingsControl.TabIndex = 0;
            this._emailSettingsControl.SettingsChanged += new System.EventHandler<System.EventArgs>(this.HandleEmailSettingsControl_SettingsChanged);
            // 
            // tabDashboard
            // 
            this.tabDashboard.Controls.Add(this.textBoxDashboardExcludeFilter);
            this.tabDashboard.Controls.Add(this.label9);
            this.tabDashboard.Controls.Add(this.textBoxDashboardIncludeFilter);
            this.tabDashboard.Controls.Add(this.label1);
            this.tabDashboard.Location = new System.Drawing.Point(4, 22);
            this.tabDashboard.Name = "tabDashboard";
            this.tabDashboard.Padding = new System.Windows.Forms.Padding(3);
            this.tabDashboard.Size = new System.Drawing.Size(458, 331);
            this.tabDashboard.TabIndex = 7;
            this.tabDashboard.Text = "Dashboard";
            this.tabDashboard.UseVisualStyleBackColor = true;
            // 
            // textBoxDashboardExcludeFilter
            // 
            this.textBoxDashboardExcludeFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDashboardExcludeFilter.Location = new System.Drawing.Point(10, 170);
            this.textBoxDashboardExcludeFilter.Multiline = true;
            this.textBoxDashboardExcludeFilter.Name = "textBoxDashboardExcludeFilter";
            this.textBoxDashboardExcludeFilter.Size = new System.Drawing.Size(442, 121);
            this.textBoxDashboardExcludeFilter.TabIndex = 1;
            // 
            // textBoxDashboardIncludeFilter
            // 
            this.textBoxDashboardIncludeFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDashboardIncludeFilter.Location = new System.Drawing.Point(10, 24);
            this.textBoxDashboardIncludeFilter.Multiline = true;
            this.textBoxDashboardIncludeFilter.Name = "textBoxDashboardIncludeFilter";
            this.textBoxDashboardIncludeFilter.Size = new System.Drawing.Size(442, 121);
            this.textBoxDashboardIncludeFilter.TabIndex = 1;
            // 
            // _tabPageAzure
            // 
            this._tabPageAzure.Controls.Add(this._azureApplicationGroupBox);
            this._tabPageAzure.Controls.Add(this._azureEmailFileSupplierGroupBox);
            this._tabPageAzure.Location = new System.Drawing.Point(4, 22);
            this._tabPageAzure.Name = "_tabPageAzure";
            this._tabPageAzure.Size = new System.Drawing.Size(458, 331);
            this._tabPageAzure.TabIndex = 8;
            this._tabPageAzure.Text = "Azure";
            this._tabPageAzure.UseVisualStyleBackColor = true;
            // 
            // _azureApplicationGroupBox
            // 
            this._azureApplicationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._azureApplicationGroupBox.AutoSize = true;
            this._azureApplicationGroupBox.Controls.Add(this._azureApplicationTableLayoutPanel);
            this._azureApplicationGroupBox.Location = new System.Drawing.Point(3, 3);
            this._azureApplicationGroupBox.Name = "_azureApplicationGroupBox";
            this._azureApplicationGroupBox.Size = new System.Drawing.Size(452, 106);
            this._azureApplicationGroupBox.TabIndex = 0;
            this._azureApplicationGroupBox.TabStop = false;
            this._azureApplicationGroupBox.Text = "Azure application";
            // 
            // _azureApplicationTableLayoutPanel
            // 
            this._azureApplicationTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._azureApplicationTableLayoutPanel.ColumnCount = 2;
            this._azureApplicationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._azureApplicationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._azureApplicationTableLayoutPanel.Controls.Add(this._azureClientIDLabel, 0, 0);
            this._azureApplicationTableLayoutPanel.Controls.Add(this._azureInstance, 1, 2);
            this._azureApplicationTableLayoutPanel.Controls.Add(this._azureTenantLabel, 0, 1);
            this._azureApplicationTableLayoutPanel.Controls.Add(this._azureTenant, 1, 1);
            this._azureApplicationTableLayoutPanel.Controls.Add(this._azureInstanceLabel, 0, 2);
            this._azureApplicationTableLayoutPanel.Controls.Add(this._azureClientID, 1, 0);
            this._azureApplicationTableLayoutPanel.Location = new System.Drawing.Point(6, 19);
            this._azureApplicationTableLayoutPanel.Name = "_azureApplicationTableLayoutPanel";
            this._azureApplicationTableLayoutPanel.RowCount = 3;
            this._azureApplicationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._azureApplicationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._azureApplicationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._azureApplicationTableLayoutPanel.Size = new System.Drawing.Size(440, 80);
            this._azureApplicationTableLayoutPanel.TabIndex = 18;
            // 
            // _azureClientIDLabel
            // 
            this._azureClientIDLabel.AutoSize = true;
            this._azureClientIDLabel.Location = new System.Drawing.Point(3, 3);
            this._azureClientIDLabel.Margin = new System.Windows.Forms.Padding(3);
            this._azureClientIDLabel.Name = "_azureClientIDLabel";
            this._azureClientIDLabel.Size = new System.Drawing.Size(47, 13);
            this._azureClientIDLabel.TabIndex = 0;
            this._azureClientIDLabel.Text = "Client ID";
            // 
            // _azureInstance
            // 
            this._azureInstance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._azureInstance.Location = new System.Drawing.Point(57, 56);
            this._azureInstance.Name = "_azureInstance";
            this._azureInstance.Size = new System.Drawing.Size(380, 20);
            this._azureInstance.TabIndex = 5;
            // 
            // _azureTenantLabel
            // 
            this._azureTenantLabel.AutoSize = true;
            this._azureTenantLabel.Location = new System.Drawing.Point(3, 29);
            this._azureTenantLabel.Margin = new System.Windows.Forms.Padding(3);
            this._azureTenantLabel.Name = "_azureTenantLabel";
            this._azureTenantLabel.Size = new System.Drawing.Size(41, 13);
            this._azureTenantLabel.TabIndex = 2;
            this._azureTenantLabel.Text = "Tenant";
            // 
            // _azureTenant
            // 
            this._azureTenant.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._azureTenant.Location = new System.Drawing.Point(57, 29);
            this._azureTenant.Name = "_azureTenant";
            this._azureTenant.Size = new System.Drawing.Size(380, 20);
            this._azureTenant.TabIndex = 3;
            // 
            // _azureInstanceLabel
            // 
            this._azureInstanceLabel.AutoSize = true;
            this._azureInstanceLabel.Location = new System.Drawing.Point(3, 55);
            this._azureInstanceLabel.Margin = new System.Windows.Forms.Padding(3);
            this._azureInstanceLabel.Name = "_azureInstanceLabel";
            this._azureInstanceLabel.Size = new System.Drawing.Size(48, 13);
            this._azureInstanceLabel.TabIndex = 4;
            this._azureInstanceLabel.Text = "Instance";
            // 
            // _azureClientID
            // 
            this._azureClientID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._azureClientID.Location = new System.Drawing.Point(57, 3);
            this._azureClientID.Name = "_azureClientID";
            this._azureClientID.Size = new System.Drawing.Size(380, 20);
            this._azureClientID.TabIndex = 1;
            // 
            // _azureEmailFileSupplierGroupBox
            // 
            this._azureEmailFileSupplierGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._azureEmailFileSupplierGroupBox.AutoSize = true;
            this._azureEmailFileSupplierGroupBox.Controls.Add(this._azureEmailFileSupplierTableLayoutPanel);
            this._azureEmailFileSupplierGroupBox.Location = new System.Drawing.Point(3, 115);
            this._azureEmailFileSupplierGroupBox.Name = "_azureEmailFileSupplierGroupBox";
            this._azureEmailFileSupplierGroupBox.Size = new System.Drawing.Size(452, 110);
            this._azureEmailFileSupplierGroupBox.TabIndex = 1;
            this._azureEmailFileSupplierGroupBox.TabStop = false;
            this._azureEmailFileSupplierGroupBox.Text = "Email file supplier login";
            // 
            // _azureEmailFileSupplierTableLayoutPanel
            // 
            this._azureEmailFileSupplierTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._azureEmailFileSupplierTableLayoutPanel.ColumnCount = 3;
            this._azureEmailFileSupplierTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._azureEmailFileSupplierTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._azureEmailFileSupplierTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._azureEmailFileSupplierTableLayoutPanel.Controls.Add(this._usernameInfoTip, 2, 0);
            this._azureEmailFileSupplierTableLayoutPanel.Controls.Add(this._azureEmailFileSupplierValidateCredentialsButton, 1, 2);
            this._azureEmailFileSupplierTableLayoutPanel.Controls.Add(this._azureEmailFileSupplierPasswordTextBox, 1, 1);
            this._azureEmailFileSupplierTableLayoutPanel.Controls.Add(this._azureEmailFileSupplierUserNameLabel, 0, 0);
            this._azureEmailFileSupplierTableLayoutPanel.Controls.Add(this._azureEmailFileSupplierPasswordLabel, 0, 1);
            this._azureEmailFileSupplierTableLayoutPanel.Controls.Add(this._azureEmailFileSupplierUserNameTextBox, 1, 0);
            this._azureEmailFileSupplierTableLayoutPanel.Controls.Add(this._azureEmailFileSupplierShowHidePasswordButton, 2, 1);
            this._azureEmailFileSupplierTableLayoutPanel.Location = new System.Drawing.Point(6, 19);
            this._azureEmailFileSupplierTableLayoutPanel.Name = "_azureEmailFileSupplierTableLayoutPanel";
            this._azureEmailFileSupplierTableLayoutPanel.RowCount = 3;
            this._azureEmailFileSupplierTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._azureEmailFileSupplierTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._azureEmailFileSupplierTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this._azureEmailFileSupplierTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this._azureEmailFileSupplierTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this._azureEmailFileSupplierTableLayoutPanel.Size = new System.Drawing.Size(440, 85);
            this._azureEmailFileSupplierTableLayoutPanel.TabIndex = 0;
            // 
            // _usernameInfoTip
            // 
            this._usernameInfoTip.BackColor = System.Drawing.Color.Transparent;
            this._usernameInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_usernameInfoTip.BackgroundImage")));
            this._usernameInfoTip.Location = new System.Drawing.Point(393, 3);
            this._usernameInfoTip.Name = "_usernameInfoTip";
            this._usernameInfoTip.Size = new System.Drawing.Size(16, 16);
            this._usernameInfoTip.TabIndex = 2;
            this._usernameInfoTip.TabStop = false;
            this._usernameInfoTip.TipText = "The name used to connect to the email account. Must include the domain, e.g., joh" +
    "n.doe@company.com";
            // 
            // _azureEmailFileSupplierValidateCredentialsButton
            // 
            this._azureEmailFileSupplierValidateCredentialsButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this._azureEmailFileSupplierValidateCredentialsButton.AutoSize = true;
            this._azureEmailFileSupplierTableLayoutPanel.SetColumnSpan(this._azureEmailFileSupplierValidateCredentialsButton, 2);
            this._azureEmailFileSupplierValidateCredentialsButton.Location = new System.Drawing.Point(328, 58);
            this._azureEmailFileSupplierValidateCredentialsButton.Name = "_azureEmailFileSupplierValidateCredentialsButton";
            this._azureEmailFileSupplierValidateCredentialsButton.Size = new System.Drawing.Size(109, 23);
            this._azureEmailFileSupplierValidateCredentialsButton.TabIndex = 6;
            this._azureEmailFileSupplierValidateCredentialsButton.Text = "Validate credentials";
            this._azureEmailFileSupplierValidateCredentialsButton.UseVisualStyleBackColor = true;
            this._azureEmailFileSupplierValidateCredentialsButton.Click += new System.EventHandler(this.HandleAzureEmailFileSupplierValidateCredentialsButton_Click);
            // 
            // _azureEmailFileSupplierPasswordTextBox
            // 
            this._azureEmailFileSupplierPasswordTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._azureEmailFileSupplierPasswordTextBox.Location = new System.Drawing.Point(67, 30);
            this._azureEmailFileSupplierPasswordTextBox.Name = "_azureEmailFileSupplierPasswordTextBox";
            this._azureEmailFileSupplierPasswordTextBox.Size = new System.Drawing.Size(320, 20);
            this._azureEmailFileSupplierPasswordTextBox.TabIndex = 4;
            this._azureEmailFileSupplierPasswordTextBox.UseSystemPasswordChar = true;
            this._azureEmailFileSupplierPasswordTextBox.TextChanged += new System.EventHandler(this.HandleAzureEmailFileSupplierCredentials_TextChanged);
            // 
            // _azureEmailFileSupplierUserNameLabel
            // 
            this._azureEmailFileSupplierUserNameLabel.AutoSize = true;
            this._azureEmailFileSupplierUserNameLabel.Location = new System.Drawing.Point(3, 3);
            this._azureEmailFileSupplierUserNameLabel.Margin = new System.Windows.Forms.Padding(3);
            this._azureEmailFileSupplierUserNameLabel.Name = "_azureEmailFileSupplierUserNameLabel";
            this._azureEmailFileSupplierUserNameLabel.Size = new System.Drawing.Size(58, 13);
            this._azureEmailFileSupplierUserNameLabel.TabIndex = 0;
            this._azureEmailFileSupplierUserNameLabel.Text = "User name";
            // 
            // _azureEmailFileSupplierPasswordLabel
            // 
            this._azureEmailFileSupplierPasswordLabel.AutoSize = true;
            this._azureEmailFileSupplierPasswordLabel.Location = new System.Drawing.Point(3, 29);
            this._azureEmailFileSupplierPasswordLabel.Margin = new System.Windows.Forms.Padding(3);
            this._azureEmailFileSupplierPasswordLabel.Name = "_azureEmailFileSupplierPasswordLabel";
            this._azureEmailFileSupplierPasswordLabel.Size = new System.Drawing.Size(53, 13);
            this._azureEmailFileSupplierPasswordLabel.TabIndex = 3;
            this._azureEmailFileSupplierPasswordLabel.Text = "Password";
            // 
            // _azureEmailFileSupplierUserNameTextBox
            // 
            this._azureEmailFileSupplierUserNameTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._azureEmailFileSupplierUserNameTextBox.Location = new System.Drawing.Point(67, 3);
            this._azureEmailFileSupplierUserNameTextBox.Name = "_azureEmailFileSupplierUserNameTextBox";
            this._azureEmailFileSupplierUserNameTextBox.Size = new System.Drawing.Size(320, 20);
            this._azureEmailFileSupplierUserNameTextBox.TabIndex = 1;
            this._azureEmailFileSupplierUserNameTextBox.TextChanged += new System.EventHandler(this.HandleAzureEmailFileSupplierCredentials_TextChanged);
            // 
            // _azureEmailFileSupplierShowHidePasswordButton
            // 
            this._azureEmailFileSupplierShowHidePasswordButton.AutoSize = true;
            this._azureEmailFileSupplierShowHidePasswordButton.Location = new System.Drawing.Point(393, 29);
            this._azureEmailFileSupplierShowHidePasswordButton.Name = "_azureEmailFileSupplierShowHidePasswordButton";
            this._azureEmailFileSupplierShowHidePasswordButton.Size = new System.Drawing.Size(44, 23);
            this._azureEmailFileSupplierShowHidePasswordButton.TabIndex = 5;
            this._azureEmailFileSupplierShowHidePasswordButton.Text = "Show";
            this._azureEmailFileSupplierShowHidePasswordButton.UseVisualStyleBackColor = true;
            this._azureEmailFileSupplierShowHidePasswordButton.Click += new System.EventHandler(this.HandleAzureEmailFileSupplierShowHidePasswordButton_Click);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(404, 393);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 3;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _buttonOK
            // 
            this._buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOK.Location = new System.Drawing.Point(323, 393);
            this._buttonOK.Name = "_buttonOK";
            this._buttonOK.Size = new System.Drawing.Size(75, 23);
            this._buttonOK.TabIndex = 2;
            this._buttonOK.Text = "OK";
            this._buttonOK.UseVisualStyleBackColor = true;
            this._buttonOK.Click += new System.EventHandler(this.HandleOkClicked);
            // 
            // _buttonRefresh
            // 
            this._buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonRefresh.Location = new System.Drawing.Point(242, 393);
            this._buttonRefresh.Name = "_buttonRefresh";
            this._buttonRefresh.Size = new System.Drawing.Size(75, 23);
            this._buttonRefresh.TabIndex = 1;
            this._buttonRefresh.Text = "Refresh";
            this._buttonRefresh.UseVisualStyleBackColor = true;
            this._buttonRefresh.Click += new System.EventHandler(this.HandleRefreshDialog);
            // 
            // _emailTestButton
            // 
            this._emailTestButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._emailTestButton.Location = new System.Drawing.Point(132, 393);
            this._emailTestButton.Name = "_emailTestButton";
            this._emailTestButton.Size = new System.Drawing.Size(104, 23);
            this._emailTestButton.TabIndex = 4;
            this._emailTestButton.Text = "Send test email";
            this._emailTestButton.UseVisualStyleBackColor = true;
            this._emailTestButton.Visible = false;
            this._emailTestButton.Click += new System.EventHandler(this.HandleEmailTestButton_Click);
            // 
            // FAMDatabaseOptionsDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(491, 428);
            this.Controls.Add(this.label10);
            this.Controls.Add(this._emailTestButton);
            this.Controls.Add(this._buttonRefresh);
            this.Controls.Add(this._buttonOK);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(this._tabControlSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(497, 354);
            this.Name = "FAMDatabaseOptionsDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Database Options";
            this._tabControlSettings.ResumeLayout(false);
            this._tabGeneral.ResumeLayout(false);
            this._tabGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._upDownRevertMinutes)).EndInit();
            this._tabHistory.ResumeLayout(false);
            this._tabHistory.PerformLayout();
            this._tabSecurity.ResumeLayout(false);
            this._tabSecurity.PerformLayout();
            this._tabVerification.ResumeLayout(false);
            this._verificationLayout.ResumeLayout(false);
            this._groupVerificationAll.ResumeLayout(false);
            this._groupVerificationAll.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numberSessionTimeout)).EndInit();
            this._groupDataEntry.ResumeLayout(false);
            this._groupDataEntry.PerformLayout();
            this._tabEmail.ResumeLayout(false);
            this.tabDashboard.ResumeLayout(false);
            this.tabDashboard.PerformLayout();
            this._tabPageAzure.ResumeLayout(false);
            this._tabPageAzure.PerformLayout();
            this._azureApplicationGroupBox.ResumeLayout(false);
            this._azureApplicationTableLayoutPanel.ResumeLayout(false);
            this._azureApplicationTableLayoutPanel.PerformLayout();
            this._azureEmailFileSupplierGroupBox.ResumeLayout(false);
            this._azureEmailFileSupplierTableLayoutPanel.ResumeLayout(false);
            this._azureEmailFileSupplierTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl _tabControlSettings;
        private System.Windows.Forms.TabPage _tabGeneral;
        private System.Windows.Forms.TabPage _tabSecurity;
        private System.Windows.Forms.ListBox _listMachinesToAuthenticate;
        private System.Windows.Forms.CheckBox _checkRequireAuthenticationToRun;
        private System.Windows.Forms.CheckBox _checkRequirePasswordForSkipped;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOK;
        private System.Windows.Forms.TabPage _tabHistory;
        private System.Windows.Forms.CheckBox _checkAutoDeleteFileActionComments;
        private System.Windows.Forms.CheckBox _checkAutoCreateActions;
        private System.Windows.Forms.CheckBox _checkAllowdynamicTagCreation;
        private System.Windows.Forms.ListBox _listAutoRevertEmailList;
        private System.Windows.Forms.CheckBox _checkStoreQueueEventHistory;
        private System.Windows.Forms.CheckBox _checkStoreFASTHistory;
        private System.Windows.Forms.CheckBox _checkStoreSourceDocChangeHistory;
        private System.Windows.Forms.Button _buttonRemoveEmail;
        private System.Windows.Forms.Button _buttonModifyEmail;
        private System.Windows.Forms.Button _buttonAddEmail;
        private System.Windows.Forms.Button _buttonRemoveMachine;
        private System.Windows.Forms.Button _buttonModifyMachine;
        private System.Windows.Forms.Button _buttonAddMachine;
        private System.Windows.Forms.Button _buttonRefresh;
        private System.Windows.Forms.TabPage _tabVerification;
        private System.Windows.Forms.GroupBox _groupDataEntry;
        private System.Windows.Forms.CheckBox _checkDataEntryEnableCounters;
        private System.Windows.Forms.FlowLayoutPanel _verificationLayout;
        private Utilities.Forms.BetterNumericUpDown _upDownRevertMinutes;
        private System.Windows.Forms.CheckBox _checkStoreDocTagHistory;
        private System.Windows.Forms.CheckBox _checkStoreDBSettingsChangeHistory;
        private Utilities.Forms.NumericEntryTextBox _numberMaxTimeBetweenChecks;
        private Utilities.Forms.NumericEntryTextBox _numberMinTimeBetweenChecks;
        private System.Windows.Forms.CheckBox _checkStoreFTPEventHistory;
        private System.Windows.Forms.TextBox _alternateComponentDataDirectoryTextBox;
        private System.Windows.Forms.TabPage _tabEmail;
        private Utilities.Email.EmailSettingsControl _emailSettingsControl;
        private System.Windows.Forms.Button _emailTestButton;
        private System.Windows.Forms.TabPage tabDashboard;
        private System.Windows.Forms.TextBox textBoxDashboardExcludeFilter;
        private System.Windows.Forms.TextBox textBoxDashboardIncludeFilter;
        private System.Windows.Forms.Button _configurePasswordRequirementsButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox _groupVerificationAll;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox _checkSessionTimeout;
        private System.Windows.Forms.NumericUpDown _numberSessionTimeout;
        private System.Windows.Forms.TabPage _tabPageAzure;
        private System.Windows.Forms.GroupBox _azureEmailFileSupplierGroupBox;
        private System.Windows.Forms.TableLayoutPanel _azureEmailFileSupplierTableLayoutPanel;
        private System.Windows.Forms.TextBox _azureEmailFileSupplierPasswordTextBox;
        private System.Windows.Forms.Label _azureEmailFileSupplierUserNameLabel;
        private System.Windows.Forms.Label _azureEmailFileSupplierPasswordLabel;
        private System.Windows.Forms.TextBox _azureEmailFileSupplierUserNameTextBox;
        private System.Windows.Forms.TableLayoutPanel _azureApplicationTableLayoutPanel;
        private System.Windows.Forms.TextBox _azureInstance;
        private System.Windows.Forms.TextBox _azureTenant;
        private System.Windows.Forms.TextBox _azureClientID;
        private System.Windows.Forms.GroupBox _azureApplicationGroupBox;
        private System.Windows.Forms.Label _azureClientIDLabel;
        private System.Windows.Forms.Label _azureTenantLabel;
        private System.Windows.Forms.Label _azureInstanceLabel;
        private System.Windows.Forms.Label _noteAboutAzureTabLabel;
        private Utilities.Forms.InfoTip _usernameInfoTip;
        private System.Windows.Forms.Button _azureEmailFileSupplierValidateCredentialsButton;
        private System.Windows.Forms.Button _azureEmailFileSupplierShowHidePasswordButton;
    }
}

