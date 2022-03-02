namespace Extract.FileActionManager.FileProcessors
{
    partial class ConvertEmailToPdfTaskSettingsDialog
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
            System.Windows.Forms.Label _splitModeQueueNewFilesActionLabel;
            System.Windows.Forms.Label _splitModeOutputDirLabel;
            Extract.Utilities.Forms.InfoTip _splitModeOutputDirInfoTip;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConvertEmailToPdfTaskSettingsDialog));
            Extract.Utilities.Forms.InfoTip _comboModeOutputFileNameInfoTip;
            System.Windows.Forms.Label _comboModeQueueNewFileActionLabel;
            System.Windows.Forms.Label _comboModeOutputFileNameLabel;
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._splitModeQueueSourceFileActionLabel = new System.Windows.Forms.Label();
            this._splitModeQueueSourceFileActionComboBox = new System.Windows.Forms.ComboBox();
            this._splitModeQueueNewFilesActionComboBox = new System.Windows.Forms.ComboBox();
            this._splitModeOutputDirBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._splitModeOutputDirTextBox = new System.Windows.Forms.TextBox();
            this._splitModeOutputDirPathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this._comboModeRadioButton = new System.Windows.Forms.RadioButton();
            this._splitModeRadioButton = new System.Windows.Forms.RadioButton();
            this._splitModeGroupBox = new System.Windows.Forms.GroupBox();
            this._comboModeGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this._comboModeQueueSourceFileActionComboBox = new System.Windows.Forms.ComboBox();
            this._comboModeQueueSourceFileActionLabel = new System.Windows.Forms.Label();
            this._comboModeOutputFileNameBrowseButton = new Extract.Utilities.Forms.BrowseButton();
            this._comboModeOutputFileNameTextBox = new System.Windows.Forms.TextBox();
            this._comboModeQueueNewFileActionComboBox = new System.Windows.Forms.ComboBox();
            this._comboModeOutputFileNamePathTagsButton = new Extract.FileActionManager.Forms.FileActionManagerPathTagButton();
            this._comboModeModifySourceDocNameCheckBox = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            _splitModeQueueNewFilesActionLabel = new System.Windows.Forms.Label();
            _splitModeOutputDirLabel = new System.Windows.Forms.Label();
            _splitModeOutputDirInfoTip = new Extract.Utilities.Forms.InfoTip();
            _comboModeOutputFileNameInfoTip = new Extract.Utilities.Forms.InfoTip();
            _comboModeQueueNewFileActionLabel = new System.Windows.Forms.Label();
            _comboModeOutputFileNameLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this._splitModeGroupBox.SuspendLayout();
            this._comboModeGroupBox.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _splitModeQueueNewFilesActionLabel
            // 
            _splitModeQueueNewFilesActionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _splitModeQueueNewFilesActionLabel.AutoSize = true;
            _splitModeQueueNewFilesActionLabel.Location = new System.Drawing.Point(3, 33);
            _splitModeQueueNewFilesActionLabel.Name = "_splitModeQueueNewFilesActionLabel";
            _splitModeQueueNewFilesActionLabel.Size = new System.Drawing.Size(146, 13);
            _splitModeQueueNewFilesActionLabel.TabIndex = 4;
            _splitModeQueueNewFilesActionLabel.Text = "Queue new files to this action";
            // 
            // _splitModeOutputDirLabel
            // 
            _splitModeOutputDirLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _splitModeOutputDirLabel.AutoSize = true;
            _splitModeOutputDirLabel.Location = new System.Drawing.Point(3, 6);
            _splitModeOutputDirLabel.Name = "_splitModeOutputDirLabel";
            _splitModeOutputDirLabel.Size = new System.Drawing.Size(68, 13);
            _splitModeOutputDirLabel.TabIndex = 0;
            _splitModeOutputDirLabel.Text = "Output folder";
            // 
            // _splitModeOutputDirInfoTip
            // 
            _splitModeOutputDirInfoTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            _splitModeOutputDirInfoTip.BackColor = System.Drawing.Color.Transparent;
            _splitModeOutputDirInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_splitModeOutputDirInfoTip.BackgroundImage")));
            _splitModeOutputDirInfoTip.Location = new System.Drawing.Point(642, 3);
            _splitModeOutputDirInfoTip.Name = "_splitModeOutputDirInfoTip";
            _splitModeOutputDirInfoTip.Size = new System.Drawing.Size(16, 16);
            _splitModeOutputDirInfoTip.TabIndex = 2;
            _splitModeOutputDirInfoTip.TabStop = false;
            _splitModeOutputDirInfoTip.TipText = "The output file names will be based on the input file name, e.g.,SourceDocName_bo" +
    "dy_text.html or SourceDocName_attachment_001_AttachmentName.pdf";
            // 
            // _comboModeOutputFileNameInfoTip
            // 
            _comboModeOutputFileNameInfoTip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            _comboModeOutputFileNameInfoTip.BackColor = System.Drawing.Color.Transparent;
            _comboModeOutputFileNameInfoTip.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_comboModeOutputFileNameInfoTip.BackgroundImage")));
            _comboModeOutputFileNameInfoTip.Location = new System.Drawing.Point(642, 3);
            _comboModeOutputFileNameInfoTip.Name = "_comboModeOutputFileNameInfoTip";
            _comboModeOutputFileNameInfoTip.Size = new System.Drawing.Size(16, 16);
            _comboModeOutputFileNameInfoTip.TabIndex = 2;
            _comboModeOutputFileNameInfoTip.TabStop = false;
            _comboModeOutputFileNameInfoTip.TipText = "The output file name should be based on the input file name, e.g., <SourceDocName" +
    ">.pdf";
            // 
            // _comboModeQueueNewFileActionLabel
            // 
            _comboModeQueueNewFileActionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _comboModeQueueNewFileActionLabel.AutoSize = true;
            _comboModeQueueNewFileActionLabel.Location = new System.Drawing.Point(3, 56);
            _comboModeQueueNewFileActionLabel.Name = "_comboModeQueueNewFileActionLabel";
            _comboModeQueueNewFileActionLabel.Size = new System.Drawing.Size(146, 13);
            _comboModeQueueNewFileActionLabel.TabIndex = 5;
            _comboModeQueueNewFileActionLabel.Text = "Queue new files to this action";
            // 
            // _comboModeOutputFileNameLabel
            // 
            _comboModeOutputFileNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _comboModeOutputFileNameLabel.AutoSize = true;
            _comboModeOutputFileNameLabel.Location = new System.Drawing.Point(3, 6);
            _comboModeOutputFileNameLabel.Name = "_comboModeOutputFileNameLabel";
            _comboModeOutputFileNameLabel.Size = new System.Drawing.Size(84, 13);
            _comboModeOutputFileNameLabel.TabIndex = 0;
            _comboModeOutputFileNameLabel.Text = "Output file name";
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(84, 23);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 1;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.Location = new System.Drawing.Point(3, 23);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 0;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _splitModeQueueSourceFileActionLabel
            // 
            this._splitModeQueueSourceFileActionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._splitModeQueueSourceFileActionLabel.AutoSize = true;
            this._splitModeQueueSourceFileActionLabel.Location = new System.Drawing.Point(3, 60);
            this._splitModeQueueSourceFileActionLabel.Name = "_splitModeQueueSourceFileActionLabel";
            this._splitModeQueueSourceFileActionLabel.Size = new System.Drawing.Size(158, 13);
            this._splitModeQueueSourceFileActionLabel.TabIndex = 6;
            this._splitModeQueueSourceFileActionLabel.Text = "Queue source files to this action";
            // 
            // _splitModeQueueSourceFileActionComboBox
            // 
            this._splitModeQueueSourceFileActionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._splitModeQueueSourceFileActionComboBox.FormattingEnabled = true;
            this._splitModeQueueSourceFileActionComboBox.Location = new System.Drawing.Point(167, 56);
            this._splitModeQueueSourceFileActionComboBox.Name = "_splitModeQueueSourceFileActionComboBox";
            this._splitModeQueueSourceFileActionComboBox.Size = new System.Drawing.Size(409, 21);
            this._splitModeQueueSourceFileActionComboBox.TabIndex = 7;
            this._splitModeQueueSourceFileActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _splitModeQueueNewFilesActionComboBox
            // 
            this._splitModeQueueNewFilesActionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._splitModeQueueNewFilesActionComboBox.FormattingEnabled = true;
            this._splitModeQueueNewFilesActionComboBox.Location = new System.Drawing.Point(167, 29);
            this._splitModeQueueNewFilesActionComboBox.Name = "_splitModeQueueNewFilesActionComboBox";
            this._splitModeQueueNewFilesActionComboBox.Size = new System.Drawing.Size(409, 21);
            this._splitModeQueueNewFilesActionComboBox.TabIndex = 5;
            this._splitModeQueueNewFilesActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _splitModeOutputDirBrowseButton
            // 
            this._splitModeOutputDirBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._splitModeOutputDirBrowseButton.EnsureFileExists = false;
            this._splitModeOutputDirBrowseButton.EnsurePathExists = false;
            this._splitModeOutputDirBrowseButton.FolderBrowser = true;
            this._splitModeOutputDirBrowseButton.Location = new System.Drawing.Point(612, 3);
            this._splitModeOutputDirBrowseButton.Name = "_splitModeOutputDirBrowseButton";
            this._splitModeOutputDirBrowseButton.Size = new System.Drawing.Size(24, 20);
            this._splitModeOutputDirBrowseButton.TabIndex = 3;
            this._splitModeOutputDirBrowseButton.Text = "...";
            this._splitModeOutputDirBrowseButton.TextControl = this._splitModeOutputDirTextBox;
            this._splitModeOutputDirBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _splitModeOutputDirTextBox
            // 
            this._splitModeOutputDirTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._splitModeOutputDirTextBox.Location = new System.Drawing.Point(167, 3);
            this._splitModeOutputDirTextBox.MinimumSize = new System.Drawing.Size(409, 20);
            this._splitModeOutputDirTextBox.Name = "_splitModeOutputDirTextBox";
            this._splitModeOutputDirTextBox.Size = new System.Drawing.Size(409, 20);
            this._splitModeOutputDirTextBox.TabIndex = 1;
            // 
            // _splitModeOutputDirPathTagsButton
            // 
            this._splitModeOutputDirPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._splitModeOutputDirPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_splitModeOutputDirPathTagsButton.Image")));
            this._splitModeOutputDirPathTagsButton.Location = new System.Drawing.Point(582, 3);
            this._splitModeOutputDirPathTagsButton.Name = "_splitModeOutputDirPathTagsButton";
            this._splitModeOutputDirPathTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._splitModeOutputDirPathTagsButton.Size = new System.Drawing.Size(24, 20);
            this._splitModeOutputDirPathTagsButton.TabIndex = 2;
            this._splitModeOutputDirPathTagsButton.TextControl = this._splitModeOutputDirTextBox;
            this._splitModeOutputDirPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this._splitModeQueueSourceFileActionComboBox, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this._splitModeQueueSourceFileActionLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(_splitModeOutputDirInfoTip, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this._splitModeOutputDirBrowseButton, 3, 0);
            this.tableLayoutPanel1.Controls.Add(_splitModeQueueNewFilesActionLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this._splitModeQueueNewFilesActionComboBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(_splitModeOutputDirLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._splitModeOutputDirPathTagsButton, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this._splitModeOutputDirTextBox, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(661, 80);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox3, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this._splitModeGroupBox, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this._comboModeGroupBox, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel1, 0, 3);
            this.tableLayoutPanel2.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(11, 12);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 3, 11, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(679, 416);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.AutoSize = true;
            this.groupBox3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox3.Controls.Add(this.tableLayoutPanel4);
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(668, 84);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Processing mode";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 505F));
            this.tableLayoutPanel4.Controls.Add(this._comboModeRadioButton, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this._splitModeRadioButton, 0, 1);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(656, 46);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // _comboModeRadioButton
            // 
            this._comboModeRadioButton.AutoSize = true;
            this._comboModeRadioButton.Checked = true;
            this._comboModeRadioButton.Location = new System.Drawing.Point(3, 3);
            this._comboModeRadioButton.Name = "_comboModeRadioButton";
            this._comboModeRadioButton.Size = new System.Drawing.Size(145, 17);
            this._comboModeRadioButton.TabIndex = 0;
            this._comboModeRadioButton.TabStop = true;
            this._comboModeRadioButton.Text = "Convert MIME file to PDF";
            this._comboModeRadioButton.UseVisualStyleBackColor = true;
            this._comboModeRadioButton.CheckedChanged += new System.EventHandler(this.HandleProcessingModeRadioButton_CheckedChanged);
            // 
            // _splitModeRadioButton
            // 
            this._splitModeRadioButton.AutoSize = true;
            this._splitModeRadioButton.Location = new System.Drawing.Point(3, 26);
            this._splitModeRadioButton.Name = "_splitModeRadioButton";
            this._splitModeRadioButton.Size = new System.Drawing.Size(92, 17);
            this._splitModeRadioButton.TabIndex = 1;
            this._splitModeRadioButton.Text = "Split MIME file";
            this._splitModeRadioButton.UseVisualStyleBackColor = true;
            this._splitModeRadioButton.CheckedChanged += new System.EventHandler(this.HandleProcessingModeRadioButton_CheckedChanged);
            // 
            // _splitModeGroupBox
            // 
            this._splitModeGroupBox.AutoSize = true;
            this._splitModeGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._splitModeGroupBox.Controls.Add(this.tableLayoutPanel1);
            this._splitModeGroupBox.Location = new System.Drawing.Point(3, 240);
            this._splitModeGroupBox.Name = "_splitModeGroupBox";
            this._splitModeGroupBox.Size = new System.Drawing.Size(673, 118);
            this._splitModeGroupBox.TabIndex = 1;
            this._splitModeGroupBox.TabStop = false;
            this._splitModeGroupBox.Text = "Split MIME file";
            // 
            // _comboModeGroupBox
            // 
            this._comboModeGroupBox.AutoSize = true;
            this._comboModeGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._comboModeGroupBox.Controls.Add(this.tableLayoutPanel3);
            this._comboModeGroupBox.Location = new System.Drawing.Point(3, 93);
            this._comboModeGroupBox.Name = "_comboModeGroupBox";
            this._comboModeGroupBox.Size = new System.Drawing.Size(673, 141);
            this._comboModeGroupBox.TabIndex = 1;
            this._comboModeGroupBox.TabStop = false;
            this._comboModeGroupBox.Text = "Convert MIME file to PDF";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 5;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this._comboModeQueueSourceFileActionComboBox, 1, 3);
            this.tableLayoutPanel3.Controls.Add(this._comboModeQueueSourceFileActionLabel, 0, 3);
            this.tableLayoutPanel3.Controls.Add(_comboModeOutputFileNameInfoTip, 4, 0);
            this.tableLayoutPanel3.Controls.Add(this._comboModeOutputFileNameBrowseButton, 3, 0);
            this.tableLayoutPanel3.Controls.Add(_comboModeQueueNewFileActionLabel, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this._comboModeQueueNewFileActionComboBox, 1, 2);
            this.tableLayoutPanel3.Controls.Add(_comboModeOutputFileNameLabel, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this._comboModeOutputFileNamePathTagsButton, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this._comboModeOutputFileNameTextBox, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this._comboModeModifySourceDocNameCheckBox, 1, 1);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 4;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(661, 103);
            this.tableLayoutPanel3.TabIndex = 6;
            // 
            // _comboModeQueueSourceFileActionComboBox
            // 
            this._comboModeQueueSourceFileActionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._comboModeQueueSourceFileActionComboBox.FormattingEnabled = true;
            this._comboModeQueueSourceFileActionComboBox.Location = new System.Drawing.Point(167, 79);
            this._comboModeQueueSourceFileActionComboBox.Name = "_comboModeQueueSourceFileActionComboBox";
            this._comboModeQueueSourceFileActionComboBox.Size = new System.Drawing.Size(409, 21);
            this._comboModeQueueSourceFileActionComboBox.TabIndex = 8;
            this._comboModeQueueSourceFileActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _comboModeQueueSourceFileActionLabel
            // 
            this._comboModeQueueSourceFileActionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._comboModeQueueSourceFileActionLabel.AutoSize = true;
            this._comboModeQueueSourceFileActionLabel.Location = new System.Drawing.Point(3, 83);
            this._comboModeQueueSourceFileActionLabel.Name = "_comboModeQueueSourceFileActionLabel";
            this._comboModeQueueSourceFileActionLabel.Size = new System.Drawing.Size(158, 13);
            this._comboModeQueueSourceFileActionLabel.TabIndex = 7;
            this._comboModeQueueSourceFileActionLabel.Text = "Queue source files to this action";
            // 
            // _comboModeOutputFileNameBrowseButton
            // 
            this._comboModeOutputFileNameBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._comboModeOutputFileNameBrowseButton.EnsureFileExists = false;
            this._comboModeOutputFileNameBrowseButton.EnsurePathExists = false;
            this._comboModeOutputFileNameBrowseButton.Location = new System.Drawing.Point(612, 3);
            this._comboModeOutputFileNameBrowseButton.Name = "_comboModeOutputFileNameBrowseButton";
            this._comboModeOutputFileNameBrowseButton.Size = new System.Drawing.Size(24, 20);
            this._comboModeOutputFileNameBrowseButton.TabIndex = 3;
            this._comboModeOutputFileNameBrowseButton.Text = "...";
            this._comboModeOutputFileNameBrowseButton.TextControl = this._comboModeOutputFileNameTextBox;
            this._comboModeOutputFileNameBrowseButton.UseVisualStyleBackColor = true;
            // 
            // _comboModeOutputFileNameTextBox
            // 
            this._comboModeOutputFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._comboModeOutputFileNameTextBox.Location = new System.Drawing.Point(167, 3);
            this._comboModeOutputFileNameTextBox.MinimumSize = new System.Drawing.Size(409, 20);
            this._comboModeOutputFileNameTextBox.Name = "_comboModeOutputFileNameTextBox";
            this._comboModeOutputFileNameTextBox.Size = new System.Drawing.Size(409, 20);
            this._comboModeOutputFileNameTextBox.TabIndex = 1;
            // 
            // _comboModeQueueNewFileActionComboBox
            // 
            this._comboModeQueueNewFileActionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._comboModeQueueNewFileActionComboBox.FormattingEnabled = true;
            this._comboModeQueueNewFileActionComboBox.Location = new System.Drawing.Point(167, 52);
            this._comboModeQueueNewFileActionComboBox.Name = "_comboModeQueueNewFileActionComboBox";
            this._comboModeQueueNewFileActionComboBox.Size = new System.Drawing.Size(409, 21);
            this._comboModeQueueNewFileActionComboBox.TabIndex = 6;
            this._comboModeQueueNewFileActionComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleActionComboBox_SelectedIndexChanged);
            // 
            // _comboModeOutputFileNamePathTagsButton
            // 
            this._comboModeOutputFileNamePathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._comboModeOutputFileNamePathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_comboModeOutputFileNamePathTagsButton.Image")));
            this._comboModeOutputFileNamePathTagsButton.Location = new System.Drawing.Point(582, 3);
            this._comboModeOutputFileNamePathTagsButton.Name = "_comboModeOutputFileNamePathTagsButton";
            this._comboModeOutputFileNamePathTagsButton.PathTags = new Extract.FileActionManager.Forms.FileActionManagerPathTags();
            this._comboModeOutputFileNamePathTagsButton.Size = new System.Drawing.Size(24, 20);
            this._comboModeOutputFileNamePathTagsButton.TabIndex = 2;
            this._comboModeOutputFileNamePathTagsButton.TextControl = this._comboModeOutputFileNameTextBox;
            this._comboModeOutputFileNamePathTagsButton.UseVisualStyleBackColor = true;
            // 
            // _comboModeModifySourceDocNameCheckBox
            // 
            this._comboModeModifySourceDocNameCheckBox.AutoSize = true;
            this.tableLayoutPanel3.SetColumnSpan(this._comboModeModifySourceDocNameCheckBox, 4);
            this._comboModeModifySourceDocNameCheckBox.Location = new System.Drawing.Point(167, 29);
            this._comboModeModifySourceDocNameCheckBox.Name = "_comboModeModifySourceDocNameCheckBox";
            this._comboModeModifySourceDocNameCheckBox.Size = new System.Drawing.Size(391, 17);
            this._comboModeModifySourceDocNameCheckBox.TabIndex = 4;
            this._comboModeModifySourceDocNameCheckBox.Text = "Modify SourceDocName in the database to match the above output file name";
            this._comboModeModifySourceDocNameCheckBox.UseVisualStyleBackColor = true;
            this._comboModeModifySourceDocNameCheckBox.CheckedChanged += new System.EventHandler(this.HandleComboModeModifySourceDocNameCheckBox_CheckedChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this._okButton);
            this.flowLayoutPanel1.Controls.Add(this._cancelButton);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(514, 364);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 20, 0, 0);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(162, 49);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // ConvertEmailToPdfTaskSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(697, 436);
            this.Controls.Add(this.tableLayoutPanel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConvertEmailToPdfTaskSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Convert email to PDF task settings";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this._splitModeGroupBox.ResumeLayout(false);
            this._splitModeGroupBox.PerformLayout();
            this._comboModeGroupBox.ResumeLayout(false);
            this._comboModeGroupBox.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label _splitModeQueueSourceFileActionLabel;
        private System.Windows.Forms.ComboBox _splitModeQueueSourceFileActionComboBox;
        private System.Windows.Forms.ComboBox _splitModeQueueNewFilesActionComboBox;
        private Extract.Utilities.Forms.BrowseButton _splitModeOutputDirBrowseButton;
        private System.Windows.Forms.TextBox _splitModeOutputDirTextBox;
        private Forms.FileActionManagerPathTagButton _splitModeOutputDirPathTagsButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.RadioButton _comboModeRadioButton;
        private System.Windows.Forms.RadioButton _splitModeRadioButton;
        private System.Windows.Forms.GroupBox _splitModeGroupBox;
        private System.Windows.Forms.GroupBox _comboModeGroupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.ComboBox _comboModeQueueSourceFileActionComboBox;
        private System.Windows.Forms.Label _comboModeQueueSourceFileActionLabel;
        private Extract.Utilities.Forms.BrowseButton _comboModeOutputFileNameBrowseButton;
        private System.Windows.Forms.TextBox _comboModeOutputFileNameTextBox;
        private System.Windows.Forms.ComboBox _comboModeQueueNewFileActionComboBox;
        private Forms.FileActionManagerPathTagButton _comboModeOutputFileNamePathTagsButton;
        private System.Windows.Forms.CheckBox _comboModeModifySourceDocNameCheckBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
