namespace Extract.AttributeFinder.Rules
{
    partial class AttributeSelectorControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._selectorComboBox = new System.Windows.Forms.ComboBox();
            this._configureButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _selectorComboBox
            // 
            this._selectorComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._selectorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._selectorComboBox.FormattingEnabled = true;
            this._selectorComboBox.Location = new System.Drawing.Point(4, 4);
            this._selectorComboBox.Name = "_selectorComboBox";
            this._selectorComboBox.Size = new System.Drawing.Size(238, 21);
            this._selectorComboBox.TabIndex = 0;
            this._selectorComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleAttributeSelectorSelectedIndexChanged);
            // 
            // _configureButton
            // 
            this._configureButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._configureButton.Enabled = false;
            this._configureButton.Location = new System.Drawing.Point(248, 3);
            this._configureButton.Name = "_configureButton";
            this._configureButton.Size = new System.Drawing.Size(75, 23);
            this._configureButton.TabIndex = 1;
            this._configureButton.Text = "Configure";
            this._configureButton.UseVisualStyleBackColor = true;
            this._configureButton.Click += new System.EventHandler(this.HandleConfigureButtonClick);
            // 
            // AttributeSelectorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._configureButton);
            this.Controls.Add(this._selectorComboBox);
            this.Name = "AttributeSelectorControl";
            this.Size = new System.Drawing.Size(326, 30);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox _selectorComboBox;
        private System.Windows.Forms.Button _configureButton;
    }
}
