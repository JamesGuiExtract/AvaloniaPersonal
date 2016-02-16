namespace Extract.LabResultsCustomComponents
{
    partial class ComponentsPlugin
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._componentsGridView = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._orderPickerButton = new System.Windows.Forms.Button();
            this._containedInTheOrdersTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this._clearFilterButton = new System.Windows.Forms.Button();
            this._applyFilterButton = new System.Windows.Forms.Button();
            this._mappingStatusComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.CodeTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._addNewComponentsButton = new System.Windows.Forms.Button();
            this._deleteSelectedComponentsButton = new System.Windows.Forms.Button();
            this._editViewComponentDetailsButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this._ordersThatContainComponentTextBox = new System.Windows.Forms.TextBox();
            this._ordersThatContainComponentLinkLabel = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this._componentsGridView)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _componentsGridView
            // 
            this._componentsGridView.AllowUserToAddRows = false;
            this._componentsGridView.AllowUserToDeleteRows = false;
            this._componentsGridView.AllowUserToResizeRows = false;
            this._componentsGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._componentsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._componentsGridView.Location = new System.Drawing.Point(3, 91);
            this._componentsGridView.Name = "_componentsGridView";
            this._componentsGridView.ReadOnly = true;
            this._componentsGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._componentsGridView.Size = new System.Drawing.Size(749, 223);
            this._componentsGridView.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._orderPickerButton);
            this.groupBox1.Controls.Add(this._containedInTheOrdersTextBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this._clearFilterButton);
            this.groupBox1.Controls.Add(this._applyFilterButton);
            this.groupBox1.Controls.Add(this._mappingStatusComboBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.NameTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.CodeTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(749, 82);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Only show components matching";
            // 
            // _orderPickerButton
            // 
            this._orderPickerButton.Location = new System.Drawing.Point(617, 51);
            this._orderPickerButton.Name = "_orderPickerButton";
            this._orderPickerButton.Size = new System.Drawing.Size(29, 23);
            this._orderPickerButton.TabIndex = 8;
            this._orderPickerButton.Text = "...";
            this._orderPickerButton.UseVisualStyleBackColor = true;
            // 
            // _containedInTheOrdersTextBox
            // 
            this._containedInTheOrdersTextBox.Location = new System.Drawing.Point(134, 53);
            this._containedInTheOrdersTextBox.Name = "_containedInTheOrdersTextBox";
            this._containedInTheOrdersTextBox.Size = new System.Drawing.Size(477, 20);
            this._containedInTheOrdersTextBox.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(122, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Contained in the order(s)";
            // 
            // _clearFilterButton
            // 
            this._clearFilterButton.Location = new System.Drawing.Point(652, 50);
            this._clearFilterButton.Name = "_clearFilterButton";
            this._clearFilterButton.Size = new System.Drawing.Size(91, 23);
            this._clearFilterButton.TabIndex = 10;
            this._clearFilterButton.Text = "Clear filter";
            this._clearFilterButton.UseVisualStyleBackColor = true;
            // 
            // _applyFilterButton
            // 
            this._applyFilterButton.Location = new System.Drawing.Point(652, 17);
            this._applyFilterButton.Name = "_applyFilterButton";
            this._applyFilterButton.Size = new System.Drawing.Size(91, 23);
            this._applyFilterButton.TabIndex = 9;
            this._applyFilterButton.Text = "Apply filter";
            this._applyFilterButton.UseVisualStyleBackColor = true;
            // 
            // _mappingStatusComboBox
            // 
            this._mappingStatusComboBox.FormattingEnabled = true;
            this._mappingStatusComboBox.Items.AddRange(new object[] {
            "All",
            "Mapped",
            "Unmapped"});
            this._mappingStatusComboBox.Location = new System.Drawing.Point(512, 19);
            this._mappingStatusComboBox.Name = "_mappingStatusComboBox";
            this._mappingStatusComboBox.Size = new System.Drawing.Size(134, 21);
            this._mappingStatusComboBox.TabIndex = 5;
            this._mappingStatusComboBox.Text = "All";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(427, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Mapping status";
            // 
            // NameTextBox
            // 
            this.NameTextBox.Location = new System.Drawing.Point(256, 19);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(165, 20);
            this.NameTextBox.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(215, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Name";
            // 
            // CodeTextBox
            // 
            this.CodeTextBox.Location = new System.Drawing.Point(44, 19);
            this.CodeTextBox.Name = "CodeTextBox";
            this.CodeTextBox.Size = new System.Drawing.Size(165, 20);
            this.CodeTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Code";
            // 
            // _addNewComponentsButton
            // 
            this._addNewComponentsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._addNewComponentsButton.Location = new System.Drawing.Point(3, 320);
            this._addNewComponentsButton.Name = "_addNewComponentsButton";
            this._addNewComponentsButton.Size = new System.Drawing.Size(170, 23);
            this._addNewComponentsButton.TabIndex = 2;
            this._addNewComponentsButton.Text = "Add new component...";
            this._addNewComponentsButton.UseVisualStyleBackColor = true;
            // 
            // _deleteSelectedComponentsButton
            // 
            this._deleteSelectedComponentsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._deleteSelectedComponentsButton.Location = new System.Drawing.Point(179, 320);
            this._deleteSelectedComponentsButton.Name = "_deleteSelectedComponentsButton";
            this._deleteSelectedComponentsButton.Size = new System.Drawing.Size(170, 23);
            this._deleteSelectedComponentsButton.TabIndex = 3;
            this._deleteSelectedComponentsButton.Text = "Delete selected component(s)";
            this._deleteSelectedComponentsButton.UseVisualStyleBackColor = true;
            // 
            // _editViewComponentDetailsButton
            // 
            this._editViewComponentDetailsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._editViewComponentDetailsButton.Location = new System.Drawing.Point(355, 320);
            this._editViewComponentDetailsButton.Name = "_editViewComponentDetailsButton";
            this._editViewComponentDetailsButton.Size = new System.Drawing.Size(170, 23);
            this._editViewComponentDetailsButton.TabIndex = 4;
            this._editViewComponentDetailsButton.Text = "Edit/View component details...";
            this._editViewComponentDetailsButton.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label5.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.label5.Location = new System.Drawing.Point(0, 350);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(760, 2);
            this.label5.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label6.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.label6.Location = new System.Drawing.Point(0, 346);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(760, 2);
            this.label6.TabIndex = 5;
            // 
            // _ordersThatContainComponentTextBox
            // 
            this._ordersThatContainComponentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._ordersThatContainComponentTextBox.BackColor = System.Drawing.SystemColors.Control;
            this._ordersThatContainComponentTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._ordersThatContainComponentTextBox.Location = new System.Drawing.Point(3, 358);
            this._ordersThatContainComponentTextBox.Name = "_ordersThatContainComponentTextBox";
            this._ordersThatContainComponentTextBox.ReadOnly = true;
            this._ordersThatContainComponentTextBox.Size = new System.Drawing.Size(749, 13);
            this._ordersThatContainComponentTextBox.TabIndex = 6;
            this._ordersThatContainComponentTextBox.Text = "Orders that contain the component:";
            // 
            // _ordersThatContainComponentLinkLabel
            // 
            this._ordersThatContainComponentLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._ordersThatContainComponentLinkLabel.BackColor = System.Drawing.SystemColors.Window;
            this._ordersThatContainComponentLinkLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ordersThatContainComponentLinkLabel.Location = new System.Drawing.Point(3, 383);
            this._ordersThatContainComponentLinkLabel.Name = "_ordersThatContainComponentLinkLabel";
            this._ordersThatContainComponentLinkLabel.Size = new System.Drawing.Size(749, 56);
            this._ordersThatContainComponentLinkLabel.TabIndex = 7;
            // 
            // ComponentsPlugin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._ordersThatContainComponentLinkLabel);
            this.Controls.Add(this._ordersThatContainComponentTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this._editViewComponentDetailsButton);
            this.Controls.Add(this._deleteSelectedComponentsButton);
            this.Controls.Add(this._addNewComponentsButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._componentsGridView);
            this.Name = "ComponentsPlugin";
            this.Size = new System.Drawing.Size(755, 450);
            ((System.ComponentModel.ISupportInitialize)(this._componentsGridView)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView _componentsGridView;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button _orderPickerButton;
        private System.Windows.Forms.TextBox _containedInTheOrdersTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button _clearFilterButton;
        private System.Windows.Forms.Button _applyFilterButton;
        private System.Windows.Forms.ComboBox _mappingStatusComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox CodeTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _addNewComponentsButton;
        private System.Windows.Forms.Button _deleteSelectedComponentsButton;
        private System.Windows.Forms.Button _editViewComponentDetailsButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox _ordersThatContainComponentTextBox;
        private System.Windows.Forms.LinkLabel _ordersThatContainComponentLinkLabel;


    }
}
