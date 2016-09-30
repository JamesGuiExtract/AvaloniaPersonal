namespace Extract.UtilityApplications.LearningMachineEditor
{
    partial class LabelAttributesConfigurationDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LabelAttributesConfigurationDialog));
            this.processButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.createEmptyLabelCheckBox = new System.Windows.Forms.CheckBox();
            this.duplicateButton = new System.Windows.Forms.Button();
            this.downButton = new Extract.Utilities.Forms.ExtractDownButton();
            this.upButton = new Extract.Utilities.Forms.ExtractUpButton();
            this.categoryAndQueryDataGridView = new System.Windows.Forms.DataGridView();
            this.removeButton = new System.Windows.Forms.Button();
            this.addButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.attributesToLabelPathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this.attributesToLabelTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.destinationTagButton = new Extract.Utilities.Forms.PathTagsButton();
            this.destinationTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.sourceOfLabelsPathTagButton = new Extract.Utilities.Forms.PathTagsButton();
            this.sourceOfLabelsTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.revertButton = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.categoryAndQueryDataGridView)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // processButton
            // 
            this.processButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.processButton.Location = new System.Drawing.Point(7, 574);
            this.processButton.Name = "processButton";
            this.processButton.Size = new System.Drawing.Size(87, 23);
            this.processButton.TabIndex = 2;
            this.processButton.Text = "Process";
            this.processButton.UseVisualStyleBackColor = true;
            this.processButton.Click += new System.EventHandler(this.HandleProcessButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Location = new System.Drawing.Point(462, 574);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(87, 23);
            this.closeButton.TabIndex = 4;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.createEmptyLabelCheckBox);
            this.groupBox2.Controls.Add(this.duplicateButton);
            this.groupBox2.Controls.Add(this.downButton);
            this.groupBox2.Controls.Add(this.upButton);
            this.groupBox2.Controls.Add(this.categoryAndQueryDataGridView);
            this.groupBox2.Controls.Add(this.removeButton);
            this.groupBox2.Controls.Add(this.addButton);
            this.groupBox2.Location = new System.Drawing.Point(7, 184);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(542, 384);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Label each attribute with the first definition where the matching source attribut" +
    "e overlaps it spatially";
            // 
            // createEmptyLabelCheckBox
            // 
            this.createEmptyLabelCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.createEmptyLabelCheckBox.AutoSize = true;
            this.createEmptyLabelCheckBox.Location = new System.Drawing.Point(10, 361);
            this.createEmptyLabelCheckBox.Name = "createEmptyLabelCheckBox";
            this.createEmptyLabelCheckBox.Size = new System.Drawing.Size(224, 17);
            this.createEmptyLabelCheckBox.TabIndex = 6;
            this.createEmptyLabelCheckBox.Text = "Create empty label if there are no matches";
            this.createEmptyLabelCheckBox.UseVisualStyleBackColor = true;
            this.createEmptyLabelCheckBox.CheckStateChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // duplicateButton
            // 
            this.duplicateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.duplicateButton.Location = new System.Drawing.Point(446, 55);
            this.duplicateButton.Name = "duplicateButton";
            this.duplicateButton.Size = new System.Drawing.Size(87, 23);
            this.duplicateButton.TabIndex = 2;
            this.duplicateButton.Text = "Duplicate";
            this.duplicateButton.UseVisualStyleBackColor = true;
            this.duplicateButton.Click += new System.EventHandler(this.HandleDuplicateButton_Click);
            // 
            // downButton
            // 
            this.downButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.downButton.Image = ((System.Drawing.Image)(resources.GetObject("downButton.Image")));
            this.downButton.Location = new System.Drawing.Point(498, 113);
            this.downButton.Name = "downButton";
            this.downButton.Size = new System.Drawing.Size(35, 35);
            this.downButton.TabIndex = 5;
            this.downButton.UseVisualStyleBackColor = true;
            this.downButton.Click += new System.EventHandler(this.HandleDownButton_Click);
            // 
            // upButton
            // 
            this.upButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.upButton.Image = ((System.Drawing.Image)(resources.GetObject("upButton.Image")));
            this.upButton.Location = new System.Drawing.Point(446, 113);
            this.upButton.Name = "upButton";
            this.upButton.Size = new System.Drawing.Size(35, 35);
            this.upButton.TabIndex = 4;
            this.upButton.UseVisualStyleBackColor = true;
            this.upButton.Click += new System.EventHandler(this.HandleUpButton_Click);
            // 
            // categoryAndQueryDataGridView
            // 
            this.categoryAndQueryDataGridView.AllowUserToAddRows = false;
            this.categoryAndQueryDataGridView.AllowUserToDeleteRows = false;
            this.categoryAndQueryDataGridView.AllowUserToOrderColumns = true;
            this.categoryAndQueryDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.categoryAndQueryDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.categoryAndQueryDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.categoryAndQueryDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.categoryAndQueryDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.categoryAndQueryDataGridView.Location = new System.Drawing.Point(9, 26);
            this.categoryAndQueryDataGridView.MultiSelect = false;
            this.categoryAndQueryDataGridView.Name = "categoryAndQueryDataGridView";
            this.categoryAndQueryDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.categoryAndQueryDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.categoryAndQueryDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.categoryAndQueryDataGridView.Size = new System.Drawing.Size(422, 329);
            this.categoryAndQueryDataGridView.TabIndex = 0;
            this.categoryAndQueryDataGridView.TabStop = false;
            this.categoryAndQueryDataGridView.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.HandleCategoryAndQueryDataGridView_RowEnter);
            // 
            // removeButton
            // 
            this.removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.removeButton.Location = new System.Drawing.Point(446, 84);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(87, 23);
            this.removeButton.TabIndex = 3;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.HandleRemoveButton_Click);
            // 
            // addButton
            // 
            this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.addButton.Location = new System.Drawing.Point(446, 26);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(87, 23);
            this.addButton.TabIndex = 1;
            this.addButton.Text = "Add";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.HandleAddButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.attributesToLabelPathTagsButton);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.destinationTagButton);
            this.groupBox1.Controls.Add(this.destinationTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.sourceOfLabelsPathTagButton);
            this.groupBox1.Controls.Add(this.sourceOfLabelsTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.attributesToLabelTextBox);
            this.groupBox1.Location = new System.Drawing.Point(7, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(542, 166);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "VOA paths, based on input image as <SourceDocName>";
            // 
            // attributesToLabelPathTagsButton
            // 
            this.attributesToLabelPathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.attributesToLabelPathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("attributesToLabelPathTagsButton.Image")));
            this.attributesToLabelPathTagsButton.Location = new System.Drawing.Point(510, 40);
            this.attributesToLabelPathTagsButton.Name = "attributesToLabelPathTagsButton";
            this.attributesToLabelPathTagsButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this.attributesToLabelPathTagsButton.Size = new System.Drawing.Size(24, 24);
            this.attributesToLabelPathTagsButton.TabIndex = 1;
            this.attributesToLabelPathTagsButton.TextControl = this.attributesToLabelTextBox;
            this.attributesToLabelPathTagsButton.UseVisualStyleBackColor = true;
            // 
            // attributesToLabelTextBox
            // 
            this.attributesToLabelTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.attributesToLabelTextBox.Location = new System.Drawing.Point(9, 42);
            this.attributesToLabelTextBox.Name = "attributesToLabelTextBox";
            this.attributesToLabelTextBox.Size = new System.Drawing.Size(495, 20);
            this.attributesToLabelTextBox.TabIndex = 0;
            this.attributesToLabelTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(158, 13);
            this.label3.TabIndex = 70;
            this.label3.Text = "Destination for labeled attributes";
            // 
            // destinationTagButton
            // 
            this.destinationTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.destinationTagButton.Image = ((System.Drawing.Image)(resources.GetObject("destinationTagButton.Image")));
            this.destinationTagButton.Location = new System.Drawing.Point(510, 131);
            this.destinationTagButton.Name = "destinationTagButton";
            this.destinationTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this.destinationTagButton.Size = new System.Drawing.Size(24, 24);
            this.destinationTagButton.TabIndex = 5;
            this.destinationTagButton.TextControl = this.destinationTextBox;
            this.destinationTagButton.UseVisualStyleBackColor = true;
            // 
            // destinationTextBox
            // 
            this.destinationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.destinationTextBox.Location = new System.Drawing.Point(9, 133);
            this.destinationTextBox.Name = "destinationTextBox";
            this.destinationTextBox.Size = new System.Drawing.Size(495, 20);
            this.destinationTextBox.TabIndex = 4;
            this.destinationTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 67;
            this.label2.Text = "Source of labels";
            // 
            // sourceOfLabelsPathTagButton
            // 
            this.sourceOfLabelsPathTagButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceOfLabelsPathTagButton.Image = ((System.Drawing.Image)(resources.GetObject("sourceOfLabelsPathTagButton.Image")));
            this.sourceOfLabelsPathTagButton.Location = new System.Drawing.Point(510, 85);
            this.sourceOfLabelsPathTagButton.Name = "sourceOfLabelsPathTagButton";
            this.sourceOfLabelsPathTagButton.PathTags = new Extract.Utilities.SourceDocumentPathTags();
            this.sourceOfLabelsPathTagButton.Size = new System.Drawing.Size(24, 24);
            this.sourceOfLabelsPathTagButton.TabIndex = 3;
            this.sourceOfLabelsPathTagButton.TextControl = this.sourceOfLabelsTextBox;
            this.sourceOfLabelsPathTagButton.UseVisualStyleBackColor = true;
            // 
            // sourceOfLabelsTextBox
            // 
            this.sourceOfLabelsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceOfLabelsTextBox.Location = new System.Drawing.Point(9, 87);
            this.sourceOfLabelsTextBox.Name = "sourceOfLabelsTextBox";
            this.sourceOfLabelsTextBox.Size = new System.Drawing.Size(495, 20);
            this.sourceOfLabelsTextBox.TabIndex = 2;
            this.sourceOfLabelsTextBox.TextChanged += new System.EventHandler(this.HandleValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 64;
            this.label1.Text = "Attributes to label";
            // 
            // revertButton
            // 
            this.revertButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.revertButton.Location = new System.Drawing.Point(369, 574);
            this.revertButton.Name = "revertButton";
            this.revertButton.Size = new System.Drawing.Size(87, 23);
            this.revertButton.TabIndex = 3;
            this.revertButton.Text = "Revert";
            this.revertButton.UseVisualStyleBackColor = true;
            this.revertButton.Click += new System.EventHandler(this.HandleRevertButton_Click);
            // 
            // LabelAttributesConfigurationDialog
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(556, 604);
            this.Controls.Add(this.revertButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.processButton);
            this.Controls.Add(this.closeButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(572, 572);
            this.Name = "LabelAttributesConfigurationDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Label attributes via spatial comparison";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.categoryAndQueryDataGridView)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button processButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button duplicateButton;
        private Utilities.Forms.ExtractDownButton downButton;
        private Utilities.Forms.ExtractUpButton upButton;
        private System.Windows.Forms.DataGridView categoryAndQueryDataGridView;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.CheckBox createEmptyLabelCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private Utilities.Forms.PathTagsButton destinationTagButton;
        private System.Windows.Forms.TextBox destinationTextBox;
        private System.Windows.Forms.Label label2;
        private Utilities.Forms.PathTagsButton sourceOfLabelsPathTagButton;
        private System.Windows.Forms.TextBox sourceOfLabelsTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox attributesToLabelTextBox;
        private Utilities.Forms.PathTagsButton attributesToLabelPathTagsButton;
        private System.Windows.Forms.Button revertButton;
    }
}