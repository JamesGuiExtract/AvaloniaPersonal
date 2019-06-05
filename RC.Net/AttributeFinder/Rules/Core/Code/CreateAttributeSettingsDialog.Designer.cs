namespace Extract.AttributeFinder.Rules
{
    partial class CreateAttributeSettingsDialog
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateAttributeSettingsDialog));
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._attributeValueTextBox = new System.Windows.Forms.TextBox();
            this._rootTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._attributeTypeTextBox = new System.Windows.Forms.TextBox();
            this._removeButton = new System.Windows.Forms.Button();
            this._AddButton = new System.Windows.Forms.Button();
            this._nameDataGridView = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._attributeNameTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this._nameCheckBox = new System.Windows.Forms.CheckBox();
            this._valueCheckBox = new System.Windows.Forms.CheckBox();
            this._typeCheckBox = new System.Windows.Forms.CheckBox();
            this._nameDoNotCreateIfEmptyCheckBox = new System.Windows.Forms.CheckBox();
            this._valueDoNotCreateIfEmptyCheckBox = new System.Windows.Forms.CheckBox();
            this._typeDoNotCreateIfEmptyCheckBox = new System.Windows.Forms.CheckBox();
            this._errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this._duplicateButton = new System.Windows.Forms.Button();
            this._downButton = new Extract.Utilities.Forms.ExtractDownButton();
            this._upButton = new Extract.Utilities.Forms.ExtractUpButton();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._exportButton = new System.Windows.Forms.Button();
            this._importButton = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._nameDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 311);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(35, 13);
            label1.TabIndex = 11;
            label1.Text = "Name";
            // 
            // label2
            // 
            label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 380);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(34, 13);
            label2.TabIndex = 13;
            label2.Text = "Value";
            // 
            // _okButton
            // 
            this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(450, 527);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(87, 23);
            this._okButton.TabIndex = 15;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.HandleOkButtonClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(552, 527);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(87, 23);
            this._cancelButton.TabIndex = 16;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _attributeValueTextBox
            // 
            this._attributeValueTextBox.AcceptsReturn = true;
            this._attributeValueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeValueTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._attributeValueTextBox.Location = new System.Drawing.Point(15, 402);
            this._attributeValueTextBox.Multiline = true;
            this._attributeValueTextBox.Name = "_attributeValueTextBox";
            this._attributeValueTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._attributeValueTextBox.Size = new System.Drawing.Size(624, 40);
            this._attributeValueTextBox.TabIndex = 11;
            this._attributeValueTextBox.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            this._attributeValueTextBox.Enter += new System.EventHandler(this.HandleFocusEnter);
            this._attributeValueTextBox.Leave += new System.EventHandler(this.HandleFocusLeave);
            // 
            // _rootTextBox
            // 
            this._rootTextBox.AcceptsReturn = true;
            this._rootTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._rootTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._rootTextBox.Location = new System.Drawing.Point(16, 27);
            this._rootTextBox.Multiline = true;
            this._rootTextBox.Name = "_rootTextBox";
            this._rootTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._rootTextBox.Size = new System.Drawing.Size(623, 81);
            this._rootTextBox.TabIndex = 0;
            this._rootTextBox.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            this._rootTextBox.Enter += new System.EventHandler(this.HandleFocusEnter);
            this._rootTextBox.Leave += new System.EventHandler(this.HandleFocusLeave);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(298, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Root (for each attribute, node, selected by this XPath query...)";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 449);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Type";
            // 
            // _attributeTypeTextBox
            // 
            this._attributeTypeTextBox.AcceptsReturn = true;
            this._attributeTypeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeTypeTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._attributeTypeTextBox.Location = new System.Drawing.Point(15, 471);
            this._attributeTypeTextBox.Multiline = true;
            this._attributeTypeTextBox.Name = "_attributeTypeTextBox";
            this._attributeTypeTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._attributeTypeTextBox.Size = new System.Drawing.Size(624, 40);
            this._attributeTypeTextBox.TabIndex = 14;
            this._attributeTypeTextBox.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            this._attributeTypeTextBox.Enter += new System.EventHandler(this.HandleFocusEnter);
            this._attributeTypeTextBox.Leave += new System.EventHandler(this.HandleFocusLeave);
            // 
            // _removeButton
            // 
            this._removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._removeButton.Location = new System.Drawing.Point(552, 204);
            this._removeButton.Name = "_removeButton";
            this._removeButton.Size = new System.Drawing.Size(87, 23);
            this._removeButton.TabIndex = 3;
            this._removeButton.Text = "Remove";
            this._removeButton.UseVisualStyleBackColor = true;
            this._removeButton.Click += new System.EventHandler(this._removeButton_Click);
            // 
            // _AddButton
            // 
            this._AddButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._AddButton.Location = new System.Drawing.Point(552, 146);
            this._AddButton.Name = "_AddButton";
            this._AddButton.Size = new System.Drawing.Size(87, 23);
            this._AddButton.TabIndex = 1;
            this._AddButton.Text = "Add";
            this._AddButton.UseVisualStyleBackColor = true;
            this._AddButton.Click += new System.EventHandler(this._AddButton_Click);
            // 
            // _nameDataGridView
            // 
            this._nameDataGridView.AllowUserToAddRows = false;
            this._nameDataGridView.AllowUserToDeleteRows = false;
            this._nameDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._nameDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._nameDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this._nameDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._nameDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._nameDataGridView.ColumnHeadersVisible = false;
            this._nameDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._nameDataGridView.DefaultCellStyle = dataGridViewCellStyle1;
            this._nameDataGridView.Location = new System.Drawing.Point(16, 137);
            this._nameDataGridView.MinimumSize = new System.Drawing.Size(521, 148);
            this._nameDataGridView.Name = "_nameDataGridView";
            this._nameDataGridView.ReadOnly = true;
            this._nameDataGridView.RowHeadersVisible = false;
            this._nameDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this._nameDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._nameDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._nameDataGridView.Size = new System.Drawing.Size(521, 167);
            this._nameDataGridView.TabIndex = 22;
            this._nameDataGridView.TabStop = false;
            this._nameDataGridView.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this._nameDataGridView_RowEnter);
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Name";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // _attributeNameTextBox
            // 
            this._attributeNameTextBox.AcceptsReturn = true;
            this._attributeNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeNameTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._attributeNameTextBox.Location = new System.Drawing.Point(15, 333);
            this._attributeNameTextBox.Multiline = true;
            this._attributeNameTextBox.Name = "_attributeNameTextBox";
            this._attributeNameTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._attributeNameTextBox.Size = new System.Drawing.Size(624, 40);
            this._attributeNameTextBox.TabIndex = 8;
            this._attributeNameTextBox.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            this._attributeNameTextBox.Enter += new System.EventHandler(this.HandleFocusEnter);
            this._attributeNameTextBox.Leave += new System.EventHandler(this.HandleFocusLeave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 119);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(163, 13);
            this.label5.TabIndex = 24;
            this.label5.Text = "Create the following subattributes";
            // 
            // _nameCheckBox
            // 
            this._nameCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._nameCheckBox.AutoSize = true;
            this._nameCheckBox.Location = new System.Drawing.Point(105, 310);
            this._nameCheckBox.Name = "_nameCheckBox";
            this._nameCheckBox.Size = new System.Drawing.Size(75, 17);
            this._nameCheckBox.TabIndex = 6;
            this._nameCheckBox.Text = "use XPath";
            this._nameCheckBox.UseVisualStyleBackColor = true;
            this._nameCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxChanged);
            this._nameCheckBox.CheckStateChanged += new System.EventHandler(this.CheckBox_CheckStateChanged);
            // 
            // _valueCheckBox
            // 
            this._valueCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._valueCheckBox.AutoSize = true;
            this._valueCheckBox.Checked = true;
            this._valueCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._valueCheckBox.Location = new System.Drawing.Point(105, 379);
            this._valueCheckBox.Name = "_valueCheckBox";
            this._valueCheckBox.Size = new System.Drawing.Size(75, 17);
            this._valueCheckBox.TabIndex = 9;
            this._valueCheckBox.Text = "use XPath";
            this._valueCheckBox.UseVisualStyleBackColor = true;
            this._valueCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxChanged);
            this._valueCheckBox.CheckStateChanged += new System.EventHandler(this.CheckBox_CheckStateChanged);
            // 
            // _typeCheckBox
            // 
            this._typeCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._typeCheckBox.AutoSize = true;
            this._typeCheckBox.Location = new System.Drawing.Point(105, 448);
            this._typeCheckBox.Name = "_typeCheckBox";
            this._typeCheckBox.Size = new System.Drawing.Size(75, 17);
            this._typeCheckBox.TabIndex = 12;
            this._typeCheckBox.Text = "use XPath";
            this._typeCheckBox.UseVisualStyleBackColor = true;
            this._typeCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxChanged);
            this._typeCheckBox.CheckStateChanged += new System.EventHandler(this.CheckBox_CheckStateChanged);
            // 
            // _nameDoNotCreateIfEmptyCheckBox
            // 
            this._nameDoNotCreateIfEmptyCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._nameDoNotCreateIfEmptyCheckBox.AutoSize = true;
            this._nameDoNotCreateIfEmptyCheckBox.Location = new System.Drawing.Point(308, 311);
            this._nameDoNotCreateIfEmptyCheckBox.Name = "_nameDoNotCreateIfEmptyCheckBox";
            this._nameDoNotCreateIfEmptyCheckBox.Size = new System.Drawing.Size(229, 17);
            this._nameDoNotCreateIfEmptyCheckBox.TabIndex = 7;
            this._nameDoNotCreateIfEmptyCheckBox.Text = "Do not create attribute if this value is empty";
            this._nameDoNotCreateIfEmptyCheckBox.UseVisualStyleBackColor = true;
            this._nameDoNotCreateIfEmptyCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxChanged);
            // 
            // _valueDoNotCreateIfEmptyCheckBox
            // 
            this._valueDoNotCreateIfEmptyCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._valueDoNotCreateIfEmptyCheckBox.AutoSize = true;
            this._valueDoNotCreateIfEmptyCheckBox.Location = new System.Drawing.Point(308, 379);
            this._valueDoNotCreateIfEmptyCheckBox.Name = "_valueDoNotCreateIfEmptyCheckBox";
            this._valueDoNotCreateIfEmptyCheckBox.Size = new System.Drawing.Size(229, 17);
            this._valueDoNotCreateIfEmptyCheckBox.TabIndex = 10;
            this._valueDoNotCreateIfEmptyCheckBox.Text = "Do not create attribute if this value is empty";
            this._valueDoNotCreateIfEmptyCheckBox.UseVisualStyleBackColor = true;
            this._valueDoNotCreateIfEmptyCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxChanged);
            // 
            // _typeDoNotCreateIfEmptyCheckBox
            // 
            this._typeDoNotCreateIfEmptyCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._typeDoNotCreateIfEmptyCheckBox.AutoSize = true;
            this._typeDoNotCreateIfEmptyCheckBox.Location = new System.Drawing.Point(308, 448);
            this._typeDoNotCreateIfEmptyCheckBox.Name = "_typeDoNotCreateIfEmptyCheckBox";
            this._typeDoNotCreateIfEmptyCheckBox.Size = new System.Drawing.Size(229, 17);
            this._typeDoNotCreateIfEmptyCheckBox.TabIndex = 13;
            this._typeDoNotCreateIfEmptyCheckBox.Text = "Do not create attribute if this value is empty";
            this._typeDoNotCreateIfEmptyCheckBox.UseVisualStyleBackColor = true;
            this._typeDoNotCreateIfEmptyCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxChanged);
            // 
            // _errorProvider
            // 
            this._errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this._errorProvider.ContainerControl = this;
            // 
            // _duplicateButton
            // 
            this._duplicateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._duplicateButton.Location = new System.Drawing.Point(552, 175);
            this._duplicateButton.Name = "_duplicateButton";
            this._duplicateButton.Size = new System.Drawing.Size(87, 23);
            this._duplicateButton.TabIndex = 2;
            this._duplicateButton.Text = "Duplicate";
            this._duplicateButton.UseVisualStyleBackColor = true;
            this._duplicateButton.Click += new System.EventHandler(this._duplicateButton_Click);
            // 
            // _downButton
            // 
            this._downButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._downButton.Image = ((System.Drawing.Image)(resources.GetObject("_downButton.Image")));
            this._downButton.Location = new System.Drawing.Point(604, 233);
            this._downButton.Name = "_downButton";
            this._downButton.Size = new System.Drawing.Size(35, 35);
            this._downButton.TabIndex = 5;
            this._downButton.UseVisualStyleBackColor = true;
            this._downButton.Click += new System.EventHandler(this._downButton_Click);
            // 
            // _upButton
            // 
            this._upButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._upButton.Image = ((System.Drawing.Image)(resources.GetObject("_upButton.Image")));
            this._upButton.Location = new System.Drawing.Point(552, 233);
            this._upButton.Name = "_upButton";
            this._upButton.Size = new System.Drawing.Size(35, 35);
            this._upButton.TabIndex = 4;
            this._upButton.UseVisualStyleBackColor = true;
            this._upButton.Click += new System.EventHandler(this._upButton_Click);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "Name";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Width = 480;
            // 
            // _exportButton
            // 
            this._exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._exportButton.Location = new System.Drawing.Point(16, 527);
            this._exportButton.Name = "_exportButton";
            this._exportButton.Size = new System.Drawing.Size(75, 23);
            this._exportButton.TabIndex = 25;
            this._exportButton.Text = "Export";
            this._exportButton.UseVisualStyleBackColor = true;
            this._exportButton.Click += new System.EventHandler(this._exportButton_Click);
            // 
            // _importButton
            // 
            this._importButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._importButton.Location = new System.Drawing.Point(105, 527);
            this._importButton.Name = "_importButton";
            this._importButton.Size = new System.Drawing.Size(75, 23);
            this._importButton.TabIndex = 26;
            this._importButton.Text = "Import";
            this._importButton.UseVisualStyleBackColor = true;
            this._importButton.Click += new System.EventHandler(this._importButton_Click);
            // 
            // CreateAttributeSettingsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(654, 566);
            this.ControlBox = false;
            this.Controls.Add(this._importButton);
            this.Controls.Add(this._exportButton);
            this.Controls.Add(this._duplicateButton);
            this.Controls.Add(this._typeDoNotCreateIfEmptyCheckBox);
            this.Controls.Add(this._valueDoNotCreateIfEmptyCheckBox);
            this.Controls.Add(this._nameDoNotCreateIfEmptyCheckBox);
            this.Controls.Add(this._downButton);
            this.Controls.Add(this._upButton);
            this.Controls.Add(this._typeCheckBox);
            this.Controls.Add(this._valueCheckBox);
            this.Controls.Add(this._nameCheckBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this._attributeNameTextBox);
            this.Controls.Add(this._nameDataGridView);
            this.Controls.Add(this._removeButton);
            this.Controls.Add(this._AddButton);
            this.Controls.Add(this._attributeTypeTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._rootTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._attributeValueTextBox);
            this.Controls.Add(label2);
            this.Controls.Add(label1);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(670, 520);
            this.Name = "CreateAttributeSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create attribute settings";
            ((System.ComponentModel.ISupportInitialize)(this._nameDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.TextBox _attributeValueTextBox;
        private System.Windows.Forms.TextBox _rootTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox _attributeTypeTextBox;
        private System.Windows.Forms.Button _removeButton;
        private System.Windows.Forms.Button _AddButton;
        private System.Windows.Forms.DataGridView _nameDataGridView;
        private System.Windows.Forms.TextBox _attributeNameTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.CheckBox _nameCheckBox;
        private System.Windows.Forms.CheckBox _valueCheckBox;
        private System.Windows.Forms.CheckBox _typeCheckBox;
        private Utilities.Forms.ExtractUpButton _upButton;
        private Utilities.Forms.ExtractDownButton _downButton;
        private System.Windows.Forms.CheckBox _nameDoNotCreateIfEmptyCheckBox;
        private System.Windows.Forms.CheckBox _valueDoNotCreateIfEmptyCheckBox;
        private System.Windows.Forms.CheckBox _typeDoNotCreateIfEmptyCheckBox;
        private System.Windows.Forms.ErrorProvider _errorProvider;
        private System.Windows.Forms.Button _duplicateButton;
        private System.Windows.Forms.Button _importButton;
        private System.Windows.Forms.Button _exportButton;
    }
}