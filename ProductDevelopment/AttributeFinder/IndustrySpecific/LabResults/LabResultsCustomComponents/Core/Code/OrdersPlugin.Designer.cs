namespace Extract.LabResultsCustomComponents
{
    partial class OrdersPlugin
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
            this._gridPanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this._LinkedComponentsTextBox = new Extract.LabResultsCustomComponents.LinkedItemsTextBox();
            ((System.ComponentModel.ISupportInitialize)(this._LinkedComponentsTextBox)).BeginInit();
            this.SuspendLayout();
            // 
            // _gridPanel
            // 
            this._gridPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._gridPanel.Location = new System.Drawing.Point(0, 86);
            this._gridPanel.Name = "_gridPanel";
            this._gridPanel.Size = new System.Drawing.Size(755, 246);
            this._gridPanel.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 339);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Included components:";
            // 
            // _LinkedComponentsTextBox
            // 
            this._LinkedComponentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._LinkedComponentsTextBox.LineWrap.Mode = ScintillaNET.WrapMode.Word;
            this._LinkedComponentsTextBox.Location = new System.Drawing.Point(3, 361);
            this._LinkedComponentsTextBox.Margins.Margin1.Width = 0;
            this._LinkedComponentsTextBox.Name = "_LinkedComponentsTextBox";
            this._LinkedComponentsTextBox.Size = new System.Drawing.Size(748, 86);
            this._LinkedComponentsTextBox.Styles.LastPredefined.ForeColor = System.Drawing.Color.Blue;
            this._LinkedComponentsTextBox.Styles.LastPredefined.IsHotspot = true;
            this._LinkedComponentsTextBox.Styles.LastPredefined.Underline = true;
            this._LinkedComponentsTextBox.TabIndex = 1;
            this._LinkedComponentsTextBox.ItemClicked += new System.EventHandler<Extract.LabResultsCustomComponents.ItemClickedEventArgs>(this._HandleLinkedComponentsTextBox_ItemClicked);
            // 
            // OrdersPlugin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this._LinkedComponentsTextBox);
            this.Controls.Add(this._gridPanel);
            this.Name = "OrdersPlugin";
            this.Size = new System.Drawing.Size(755, 450);
            ((System.ComponentModel.ISupportInitialize)(this._LinkedComponentsTextBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel _gridPanel;
        private LinkedItemsTextBox _LinkedComponentsTextBox;
        private System.Windows.Forms.Label label1;


    }
}
