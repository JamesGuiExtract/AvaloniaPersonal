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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label5;
            this._tabControlSettings = new System.Windows.Forms.TabControl();
            this._tabGeneral = new System.Windows.Forms.TabPage();
            this._upDownRevertMinutes = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._buttonRemoveEmail = new System.Windows.Forms.Button();
            this._buttonModifyEmail = new System.Windows.Forms.Button();
            this._buttonAddEmail = new System.Windows.Forms.Button();
            this._listAutoRevertEmailList = new System.Windows.Forms.ListBox();
            this._checkAutoRevertFiles = new System.Windows.Forms.CheckBox();
            this._checkAutoDeleteFileActionComments = new System.Windows.Forms.CheckBox();
            this._checkAutoCreateActions = new System.Windows.Forms.CheckBox();
            this._checkAllowdynamicTagCreation = new System.Windows.Forms.CheckBox();
            this._tabHistory = new System.Windows.Forms.TabPage();
            this._checkStoreDBSettingsChangeHistory = new System.Windows.Forms.CheckBox();
            this._checkStoreDocTagHistory = new System.Windows.Forms.CheckBox();
            this._upDownInputEventHistory = new Extract.Utilities.Forms.BetterNumericUpDown();
            this._checkStoreInputEventTracking = new System.Windows.Forms.CheckBox();
            this._checkStoreQueueEventHistory = new System.Windows.Forms.CheckBox();
            this._checkStoreFASTHistory = new System.Windows.Forms.CheckBox();
            this._checkStoreFAMSessionHistory = new System.Windows.Forms.CheckBox();
            this._checkStoreSourceDocChangeHistory = new System.Windows.Forms.CheckBox();
            this._tabSecurity = new System.Windows.Forms.TabPage();
            this._buttonRemoveMachine = new System.Windows.Forms.Button();
            this._buttonModifyMachine = new System.Windows.Forms.Button();
            this._buttonAddMachine = new System.Windows.Forms.Button();
            this._listMachinesToAuthenticate = new System.Windows.Forms.ListBox();
            this._checkRequireAuthenticationToRun = new System.Windows.Forms.CheckBox();
            this._checkRequirePasswordForSkipped = new System.Windows.Forms.CheckBox();
            this._tabProductSpecific = new System.Windows.Forms.TabPage();
            this._productSpecificLayout = new System.Windows.Forms.FlowLayoutPanel();
            this._groupIDShield = new System.Windows.Forms.GroupBox();
            this._checkIdShieldHistory = new System.Windows.Forms.CheckBox();
            this._groupDataEntry = new System.Windows.Forms.GroupBox();
            this._checkDataEntryEnableCounters = new System.Windows.Forms.CheckBox();
            this._checkDataEntryHistory = new System.Windows.Forms.CheckBox();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOK = new System.Windows.Forms.Button();
            this._buttonRefresh = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            this._tabControlSettings.SuspendLayout();
            this._tabGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._upDownRevertMinutes)).BeginInit();
            this._tabHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._upDownInputEventHistory)).BeginInit();
            this._tabSecurity.SuspendLayout();
            this._tabProductSpecific.SuspendLayout();
            this._productSpecificLayout.SuspendLayout();
            this._groupIDShield.SuspendLayout();
            this._groupDataEntry.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(6, 50);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(353, 13);
            label2.TabIndex = 2;
            label2.Text = "Skip authentication when running as a service on the following machines:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(23, 115);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(305, 13);
            label4.TabIndex = 12;
            label4.Text = "After reverting file status, notify the following recipients by email:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(23, 95);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(110, 13);
            label3.TabIndex = 10;
            label3.Text = "Revert file status after";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(254, 122);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(35, 13);
            label1.TabIndex = 20;
            label1.Text = "day(s)";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(194, 95);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(105, 13);
            label5.TabIndex = 21;
            label5.Text = "minute(s) of inactivity";
            // 
            // _tabControlSettings
            // 
            this._tabControlSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tabControlSettings.Controls.Add(this._tabGeneral);
            this._tabControlSettings.Controls.Add(this._tabHistory);
            this._tabControlSettings.Controls.Add(this._tabSecurity);
            this._tabControlSettings.Controls.Add(this._tabProductSpecific);
            this._tabControlSettings.Location = new System.Drawing.Point(13, 13);
            this._tabControlSettings.Name = "_tabControlSettings";
            this._tabControlSettings.SelectedIndex = 0;
            this._tabControlSettings.Size = new System.Drawing.Size(466, 272);
            this._tabControlSettings.TabIndex = 0;
            // 
            // _tabGeneral
            // 
            this._tabGeneral.Controls.Add(this._upDownRevertMinutes);
            this._tabGeneral.Controls.Add(label5);
            this._tabGeneral.Controls.Add(this._buttonRemoveEmail);
            this._tabGeneral.Controls.Add(this._buttonModifyEmail);
            this._tabGeneral.Controls.Add(this._buttonAddEmail);
            this._tabGeneral.Controls.Add(this._listAutoRevertEmailList);
            this._tabGeneral.Controls.Add(label4);
            this._tabGeneral.Controls.Add(label3);
            this._tabGeneral.Controls.Add(this._checkAutoRevertFiles);
            this._tabGeneral.Controls.Add(this._checkAutoDeleteFileActionComments);
            this._tabGeneral.Controls.Add(this._checkAutoCreateActions);
            this._tabGeneral.Controls.Add(this._checkAllowdynamicTagCreation);
            this._tabGeneral.Location = new System.Drawing.Point(4, 22);
            this._tabGeneral.Name = "_tabGeneral";
            this._tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this._tabGeneral.Size = new System.Drawing.Size(458, 246);
            this._tabGeneral.TabIndex = 0;
            this._tabGeneral.Text = "General";
            this._tabGeneral.ToolTipText = "General FAM database settings";
            this._tabGeneral.UseVisualStyleBackColor = true;
            // 
            // _upDownRevertMinutes
            // 
            this._upDownRevertMinutes.Location = new System.Drawing.Point(139, 93);
            this._upDownRevertMinutes.Maximum = new decimal(new int[] {
            1440,
            0,
            0,
            0});
            this._upDownRevertMinutes.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this._upDownRevertMinutes.Name = "_upDownRevertMinutes";
            this._upDownRevertMinutes.Size = new System.Drawing.Size(49, 20);
            this._upDownRevertMinutes.TabIndex = 4;
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
            this._buttonRemoveEmail.Location = new System.Drawing.Point(377, 190);
            this._buttonRemoveEmail.Name = "_buttonRemoveEmail";
            this._buttonRemoveEmail.Size = new System.Drawing.Size(75, 23);
            this._buttonRemoveEmail.TabIndex = 8;
            this._buttonRemoveEmail.Text = "Remove";
            this._buttonRemoveEmail.UseVisualStyleBackColor = true;
            this._buttonRemoveEmail.Click += new System.EventHandler(this.HandleAutoRevertEmailRemoveClicked);
            // 
            // _buttonModifyEmail
            // 
            this._buttonModifyEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonModifyEmail.Location = new System.Drawing.Point(377, 161);
            this._buttonModifyEmail.Name = "_buttonModifyEmail";
            this._buttonModifyEmail.Size = new System.Drawing.Size(75, 23);
            this._buttonModifyEmail.TabIndex = 7;
            this._buttonModifyEmail.Text = "Modify...";
            this._buttonModifyEmail.UseVisualStyleBackColor = true;
            this._buttonModifyEmail.Click += new System.EventHandler(this.HandleModifyAutoRevertEmailClicked);
            // 
            // _buttonAddEmail
            // 
            this._buttonAddEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonAddEmail.Location = new System.Drawing.Point(377, 132);
            this._buttonAddEmail.Name = "_buttonAddEmail";
            this._buttonAddEmail.Size = new System.Drawing.Size(75, 23);
            this._buttonAddEmail.TabIndex = 6;
            this._buttonAddEmail.Text = "Add...";
            this._buttonAddEmail.UseVisualStyleBackColor = true;
            this._buttonAddEmail.Click += new System.EventHandler(this.HandleAddAutoRevertEmailClicked);
            // 
            // _listAutoRevertEmailList
            // 
            this._listAutoRevertEmailList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._listAutoRevertEmailList.FormattingEnabled = true;
            this._listAutoRevertEmailList.Location = new System.Drawing.Point(26, 132);
            this._listAutoRevertEmailList.Name = "_listAutoRevertEmailList";
            this._listAutoRevertEmailList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this._listAutoRevertEmailList.Size = new System.Drawing.Size(345, 95);
            this._listAutoRevertEmailList.TabIndex = 5;
            this._listAutoRevertEmailList.SelectedIndexChanged += new System.EventHandler(this.HandleAutoRevertSelectionChanged);
            // 
            // _checkAutoRevertFiles
            // 
            this._checkAutoRevertFiles.AutoSize = true;
            this._checkAutoRevertFiles.Location = new System.Drawing.Point(6, 75);
            this._checkAutoRevertFiles.Name = "_checkAutoRevertFiles";
            this._checkAutoRevertFiles.Size = new System.Drawing.Size(174, 17);
            this._checkAutoRevertFiles.TabIndex = 3;
            this._checkAutoRevertFiles.Text = "Automatically revert locked files";
            this._checkAutoRevertFiles.UseVisualStyleBackColor = true;
            this._checkAutoRevertFiles.CheckedChanged += new System.EventHandler(this.HandleAutoRevertCheckChangedEvent);
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
            this._checkAutoCreateActions.Size = new System.Drawing.Size(264, 17);
            this._checkAutoCreateActions.TabIndex = 1;
            this._checkAutoCreateActions.Text = "Automactically create actions specified in FPS files";
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
            this._tabHistory.Controls.Add(this._checkStoreDBSettingsChangeHistory);
            this._tabHistory.Controls.Add(this._checkStoreDocTagHistory);
            this._tabHistory.Controls.Add(this._upDownInputEventHistory);
            this._tabHistory.Controls.Add(label1);
            this._tabHistory.Controls.Add(this._checkStoreInputEventTracking);
            this._tabHistory.Controls.Add(this._checkStoreQueueEventHistory);
            this._tabHistory.Controls.Add(this._checkStoreFASTHistory);
            this._tabHistory.Controls.Add(this._checkStoreFAMSessionHistory);
            this._tabHistory.Controls.Add(this._checkStoreSourceDocChangeHistory);
            this._tabHistory.Location = new System.Drawing.Point(4, 22);
            this._tabHistory.Name = "_tabHistory";
            this._tabHistory.Padding = new System.Windows.Forms.Padding(3);
            this._tabHistory.Size = new System.Drawing.Size(458, 246);
            this._tabHistory.TabIndex = 4;
            this._tabHistory.Text = "History";
            this._tabHistory.ToolTipText = "FAM history settings";
            this._tabHistory.UseVisualStyleBackColor = true;
            // 
            // _checkStoreDBSettingsChangeHistory
            // 
            this._checkStoreDBSettingsChangeHistory.AutoSize = true;
            this._checkStoreDBSettingsChangeHistory.Location = new System.Drawing.Point(6, 144);
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
            // _upDownInputEventHistory
            // 
            this._upDownInputEventHistory.Location = new System.Drawing.Point(208, 120);
            this._upDownInputEventHistory.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this._upDownInputEventHistory.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._upDownInputEventHistory.Name = "_upDownInputEventHistory";
            this._upDownInputEventHistory.Size = new System.Drawing.Size(40, 20);
            this._upDownInputEventHistory.TabIndex = 6;
            this._upDownInputEventHistory.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this._upDownInputEventHistory.UserTextCorrected += new System.EventHandler<System.EventArgs>(this.HandleInputEventHistoryValueCorrectedEvent);
            // 
            // _checkStoreInputEventTracking
            // 
            this._checkStoreInputEventTracking.AutoSize = true;
            this._checkStoreInputEventTracking.Location = new System.Drawing.Point(6, 121);
            this._checkStoreInputEventTracking.Name = "_checkStoreInputEventTracking";
            this._checkStoreInputEventTracking.Size = new System.Drawing.Size(196, 17);
            this._checkStoreInputEventTracking.TabIndex = 5;
            this._checkStoreInputEventTracking.Text = "Store input event tracking history for";
            this._checkStoreInputEventTracking.UseVisualStyleBackColor = true;
            this._checkStoreInputEventTracking.CheckedChanged += new System.EventHandler(this.HandleInputEventHistoryCheckChangedEvent);
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
            // _checkStoreFAMSessionHistory
            // 
            this._checkStoreFAMSessionHistory.AutoSize = true;
            this._checkStoreFAMSessionHistory.Location = new System.Drawing.Point(6, 98);
            this._checkStoreFAMSessionHistory.Name = "_checkStoreFAMSessionHistory";
            this._checkStoreFAMSessionHistory.Size = new System.Drawing.Size(250, 17);
            this._checkStoreFAMSessionHistory.TabIndex = 4;
            this._checkStoreFAMSessionHistory.Text = "Store File Action Manager (FAM) session history";
            this._checkStoreFAMSessionHistory.UseVisualStyleBackColor = true;
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
            this._tabSecurity.Size = new System.Drawing.Size(458, 246);
            this._tabSecurity.TabIndex = 1;
            this._tabSecurity.Text = "Security";
            this._tabSecurity.ToolTipText = "FAM security settings";
            this._tabSecurity.UseVisualStyleBackColor = true;
            // 
            // _buttonRemoveMachine
            // 
            this._buttonRemoveMachine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonRemoveMachine.Location = new System.Drawing.Point(377, 124);
            this._buttonRemoveMachine.Name = "_buttonRemoveMachine";
            this._buttonRemoveMachine.Size = new System.Drawing.Size(75, 23);
            this._buttonRemoveMachine.TabIndex = 5;
            this._buttonRemoveMachine.Text = "Remove";
            this._buttonRemoveMachine.UseVisualStyleBackColor = true;
            this._buttonRemoveMachine.Click += new System.EventHandler(this.HandleRemoveMachineNamesClicked);
            // 
            // _buttonModifyMachine
            // 
            this._buttonModifyMachine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonModifyMachine.Location = new System.Drawing.Point(377, 95);
            this._buttonModifyMachine.Name = "_buttonModifyMachine";
            this._buttonModifyMachine.Size = new System.Drawing.Size(75, 23);
            this._buttonModifyMachine.TabIndex = 4;
            this._buttonModifyMachine.Text = "Modify...";
            this._buttonModifyMachine.UseVisualStyleBackColor = true;
            this._buttonModifyMachine.Click += new System.EventHandler(this.HandleModifyMachineNameClick);
            // 
            // _buttonAddMachine
            // 
            this._buttonAddMachine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonAddMachine.Location = new System.Drawing.Point(377, 66);
            this._buttonAddMachine.Name = "_buttonAddMachine";
            this._buttonAddMachine.Size = new System.Drawing.Size(75, 23);
            this._buttonAddMachine.TabIndex = 3;
            this._buttonAddMachine.Text = "Add...";
            this._buttonAddMachine.UseVisualStyleBackColor = true;
            this._buttonAddMachine.Click += new System.EventHandler(this.HandleAddMachineNameButtonClicked);
            // 
            // _listMachinesToAuthenticate
            // 
            this._listMachinesToAuthenticate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._listMachinesToAuthenticate.FormattingEnabled = true;
            this._listMachinesToAuthenticate.Location = new System.Drawing.Point(9, 66);
            this._listMachinesToAuthenticate.Name = "_listMachinesToAuthenticate";
            this._listMachinesToAuthenticate.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this._listMachinesToAuthenticate.Size = new System.Drawing.Size(362, 95);
            this._listMachinesToAuthenticate.TabIndex = 2;
            this._listMachinesToAuthenticate.SelectedIndexChanged += new System.EventHandler(this.HandleMachineToSkipSelectionChanged);
            // 
            // _checkRequireAuthenticationToRun
            // 
            this._checkRequireAuthenticationToRun.AutoSize = true;
            this._checkRequireAuthenticationToRun.Location = new System.Drawing.Point(7, 30);
            this._checkRequireAuthenticationToRun.Name = "_checkRequireAuthenticationToRun";
            this._checkRequireAuthenticationToRun.Size = new System.Drawing.Size(163, 17);
            this._checkRequireAuthenticationToRun.TabIndex = 1;
            this._checkRequireAuthenticationToRun.Text = "Require authentication to run";
            this._checkRequireAuthenticationToRun.UseVisualStyleBackColor = true;
            // 
            // _checkRequirePasswordForSkipped
            // 
            this._checkRequirePasswordForSkipped.AutoSize = true;
            this._checkRequirePasswordForSkipped.Location = new System.Drawing.Point(7, 7);
            this._checkRequirePasswordForSkipped.Name = "_checkRequirePasswordForSkipped";
            this._checkRequirePasswordForSkipped.Size = new System.Drawing.Size(371, 17);
            this._checkRequirePasswordForSkipped.TabIndex = 0;
            this._checkRequirePasswordForSkipped.Text = "Require FAM database administrator password to process all skipped files";
            this._checkRequirePasswordForSkipped.UseVisualStyleBackColor = true;
            // 
            // _tabProductSpecific
            // 
            this._tabProductSpecific.Controls.Add(this._productSpecificLayout);
            this._tabProductSpecific.Location = new System.Drawing.Point(4, 22);
            this._tabProductSpecific.Name = "_tabProductSpecific";
            this._tabProductSpecific.Size = new System.Drawing.Size(458, 246);
            this._tabProductSpecific.TabIndex = 5;
            this._tabProductSpecific.Text = "Product Specific";
            this._tabProductSpecific.ToolTipText = "Product specific settings";
            this._tabProductSpecific.UseVisualStyleBackColor = true;
            // 
            // _productSpecificLayout
            // 
            this._productSpecificLayout.Controls.Add(this._groupIDShield);
            this._productSpecificLayout.Controls.Add(this._groupDataEntry);
            this._productSpecificLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._productSpecificLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this._productSpecificLayout.Location = new System.Drawing.Point(0, 0);
            this._productSpecificLayout.Name = "_productSpecificLayout";
            this._productSpecificLayout.Size = new System.Drawing.Size(458, 246);
            this._productSpecificLayout.TabIndex = 1;
            // 
            // _groupIDShield
            // 
            this._groupIDShield.Controls.Add(this._checkIdShieldHistory);
            this._groupIDShield.Location = new System.Drawing.Point(3, 3);
            this._groupIDShield.Name = "_groupIDShield";
            this._groupIDShield.Size = new System.Drawing.Size(452, 65);
            this._groupIDShield.TabIndex = 0;
            this._groupIDShield.TabStop = false;
            this._groupIDShield.Text = "ID Shield";
            // 
            // _checkIdShieldHistory
            // 
            this._checkIdShieldHistory.AutoSize = true;
            this._checkIdShieldHistory.Location = new System.Drawing.Point(6, 19);
            this._checkIdShieldHistory.Name = "_checkIdShieldHistory";
            this._checkIdShieldHistory.Size = new System.Drawing.Size(184, 17);
            this._checkIdShieldHistory.TabIndex = 0;
            this._checkIdShieldHistory.Text = "Store ID Shield processing history";
            this._checkIdShieldHistory.UseVisualStyleBackColor = true;
            // 
            // _groupDataEntry
            // 
            this._groupDataEntry.Controls.Add(this._checkDataEntryEnableCounters);
            this._groupDataEntry.Controls.Add(this._checkDataEntryHistory);
            this._groupDataEntry.Location = new System.Drawing.Point(3, 74);
            this._groupDataEntry.Name = "_groupDataEntry";
            this._groupDataEntry.Size = new System.Drawing.Size(451, 65);
            this._groupDataEntry.TabIndex = 1;
            this._groupDataEntry.TabStop = false;
            this._groupDataEntry.Text = "Data Entry";
            // 
            // _checkDataEntryEnableCounters
            // 
            this._checkDataEntryEnableCounters.AutoSize = true;
            this._checkDataEntryEnableCounters.Location = new System.Drawing.Point(6, 42);
            this._checkDataEntryEnableCounters.Name = "_checkDataEntryEnableCounters";
            this._checkDataEntryEnableCounters.Size = new System.Drawing.Size(153, 17);
            this._checkDataEntryEnableCounters.TabIndex = 1;
            this._checkDataEntryEnableCounters.Text = "Enable data entry counters";
            this._checkDataEntryEnableCounters.UseVisualStyleBackColor = true;
            // 
            // _checkDataEntryHistory
            // 
            this._checkDataEntryHistory.AutoSize = true;
            this._checkDataEntryHistory.Location = new System.Drawing.Point(6, 19);
            this._checkDataEntryHistory.Name = "_checkDataEntryHistory";
            this._checkDataEntryHistory.Size = new System.Drawing.Size(188, 17);
            this._checkDataEntryHistory.TabIndex = 0;
            this._checkDataEntryHistory.Text = "Store data entry processing history";
            this._checkDataEntryHistory.UseVisualStyleBackColor = true;
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(404, 291);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 3;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _buttonOK
            // 
            this._buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._buttonOK.Location = new System.Drawing.Point(323, 291);
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
            this._buttonRefresh.Location = new System.Drawing.Point(242, 291);
            this._buttonRefresh.Name = "_buttonRefresh";
            this._buttonRefresh.Size = new System.Drawing.Size(75, 23);
            this._buttonRefresh.TabIndex = 1;
            this._buttonRefresh.Text = "Refresh";
            this._buttonRefresh.UseVisualStyleBackColor = true;
            this._buttonRefresh.Click += new System.EventHandler(this.HandleRefreshDialog);
            // 
            // FAMDatabaseOptionsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(491, 326);
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
            ((System.ComponentModel.ISupportInitialize)(this._upDownInputEventHistory)).EndInit();
            this._tabSecurity.ResumeLayout(false);
            this._tabSecurity.PerformLayout();
            this._tabProductSpecific.ResumeLayout(false);
            this._productSpecificLayout.ResumeLayout(false);
            this._groupIDShield.ResumeLayout(false);
            this._groupIDShield.PerformLayout();
            this._groupDataEntry.ResumeLayout(false);
            this._groupDataEntry.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.CheckBox _checkAutoRevertFiles;
        private System.Windows.Forms.CheckBox _checkStoreInputEventTracking;
        private System.Windows.Forms.CheckBox _checkStoreQueueEventHistory;
        private System.Windows.Forms.CheckBox _checkStoreFASTHistory;
        private System.Windows.Forms.CheckBox _checkStoreFAMSessionHistory;
        private System.Windows.Forms.CheckBox _checkStoreSourceDocChangeHistory;
        private System.Windows.Forms.Button _buttonRemoveEmail;
        private System.Windows.Forms.Button _buttonModifyEmail;
        private System.Windows.Forms.Button _buttonAddEmail;
        private System.Windows.Forms.Button _buttonRemoveMachine;
        private System.Windows.Forms.Button _buttonModifyMachine;
        private System.Windows.Forms.Button _buttonAddMachine;
        private System.Windows.Forms.Button _buttonRefresh;
        private System.Windows.Forms.TabPage _tabProductSpecific;
        private System.Windows.Forms.GroupBox _groupDataEntry;
        private System.Windows.Forms.CheckBox _checkDataEntryEnableCounters;
        private System.Windows.Forms.CheckBox _checkDataEntryHistory;
        private System.Windows.Forms.GroupBox _groupIDShield;
        private System.Windows.Forms.CheckBox _checkIdShieldHistory;
        private System.Windows.Forms.FlowLayoutPanel _productSpecificLayout;
        private Utilities.Forms.BetterNumericUpDown _upDownRevertMinutes;
        private Utilities.Forms.BetterNumericUpDown _upDownInputEventHistory;
        private System.Windows.Forms.CheckBox _checkStoreDocTagHistory;
        private System.Windows.Forms.CheckBox _checkStoreDBSettingsChangeHistory;
    }
}

