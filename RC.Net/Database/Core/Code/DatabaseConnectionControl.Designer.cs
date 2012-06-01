namespace Extract.Database
{
    partial class DatabaseConnectionControl
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseConnectionControl));
            this._dataSourceTextBox = new System.Windows.Forms.TextBox();
            this._configureConnectionButton = new System.Windows.Forms.Button();
            this._connectionStringTextBox = new System.Windows.Forms.TextBox();
            this._pathTagsButton = new Extract.Utilities.Forms.PathTagsButton();
            this._contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(-3, -1);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(65, 13);
            label3.TabIndex = 0;
            label3.Text = "Data source";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(-3, 38);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(89, 13);
            label2.TabIndex = 2;
            label2.Text = "Connection string";
            // 
            // _dataSourceTextBox
            // 
            this._dataSourceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._dataSourceTextBox.Location = new System.Drawing.Point(0, 15);
            this._dataSourceTextBox.Name = "_dataSourceTextBox";
            this._dataSourceTextBox.ReadOnly = true;
            this._dataSourceTextBox.Size = new System.Drawing.Size(434, 20);
            this._dataSourceTextBox.TabIndex = 1;
            // 
            // _configureConnectionButton
            // 
            this._configureConnectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._configureConnectionButton.Location = new System.Drawing.Point(468, 53);
            this._configureConnectionButton.Name = "_configureConnectionButton";
            this._configureConnectionButton.Size = new System.Drawing.Size(74, 38);
            this._configureConnectionButton.TabIndex = 5;
            this._configureConnectionButton.Text = "Configure connection";
            this._configureConnectionButton.UseVisualStyleBackColor = true;
            this._configureConnectionButton.Click += new System.EventHandler(this.HandleConfigureConnectionClicked);
            // 
            // _connectionStringTextBox
            // 
            this._connectionStringTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._connectionStringTextBox.Location = new System.Drawing.Point(0, 55);
            this._connectionStringTextBox.Multiline = true;
            this._connectionStringTextBox.Name = "_connectionStringTextBox";
            this._connectionStringTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._connectionStringTextBox.Size = new System.Drawing.Size(434, 35);
            this._connectionStringTextBox.TabIndex = 3;
            this._connectionStringTextBox.TextChanged += new System.EventHandler(this.HandleConnectionStringTextChanged);
            // 
            // _pathTagsButton
            // 
            this._pathTagsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._pathTagsButton.DisplayFunctionTags = false;
            this._pathTagsButton.DisplayPathTags = false;
            this._pathTagsButton.Image = ((System.Drawing.Image)(resources.GetObject("_pathTagsButton.Image")));
            this._pathTagsButton.Location = new System.Drawing.Point(442, 53);
            this._pathTagsButton.Name = "_pathTagsButton";
            this._pathTagsButton.Size = new System.Drawing.Size(18, 38);
            this._pathTagsButton.TabIndex = 4;
            this._pathTagsButton.TextControl = this._connectionStringTextBox;
            this._pathTagsButton.UseVisualStyleBackColor = true;
            this._pathTagsButton.MenuOpening += new System.EventHandler<Extract.Utilities.Forms.PathTagsMenuOpeningEventArgs>(this.HandlePathTagsMenuOpening);
            // 
            // _contextMenuStrip
            // 
            this._contextMenuStrip.Name = "_contextMenuStrip";
            this._contextMenuStrip.Size = new System.Drawing.Size(61, 4);
            // 
            // DatabaseConnectionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._pathTagsButton);
            this.Controls.Add(this._dataSourceTextBox);
            this.Controls.Add(label3);
            this.Controls.Add(this._configureConnectionButton);
            this.Controls.Add(label2);
            this.Controls.Add(this._connectionStringTextBox);
            this.Name = "DatabaseConnectionControl";
            this.Size = new System.Drawing.Size(542, 90);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _dataSourceTextBox;
        private System.Windows.Forms.Button _configureConnectionButton;
        private System.Windows.Forms.TextBox _connectionStringTextBox;
        private Utilities.Forms.PathTagsButton _pathTagsButton;
        private System.Windows.Forms.ContextMenuStrip _contextMenuStrip;
    }
}
