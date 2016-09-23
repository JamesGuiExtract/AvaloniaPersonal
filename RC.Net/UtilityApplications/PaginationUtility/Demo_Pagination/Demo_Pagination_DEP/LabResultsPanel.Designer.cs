namespace Extract.Demo_Pagination
{
    partial class LabResultsPanel
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
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label1;
            this._collectionDateTextBox = new System.Windows.Forms.TextBox();
            this._collectionTimeTextBox = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = System.Drawing.Color.White;
            label5.Location = new System.Drawing.Point(-3, 6);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(82, 13);
            label5.TabIndex = 14;
            label5.Text = "Collection Date:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = System.Drawing.Color.White;
            label1.Location = new System.Drawing.Point(202, 6);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(82, 13);
            label1.TabIndex = 16;
            label1.Text = "Collection Time:";
            // 
            // _collectionDateTextBox
            // 
            this._collectionDateTextBox.Location = new System.Drawing.Point(88, 3);
            this._collectionDateTextBox.Name = "_collectionDateTextBox";
            this._collectionDateTextBox.Size = new System.Drawing.Size(105, 20);
            this._collectionDateTextBox.TabIndex = 15;
            this._collectionDateTextBox.TextChanged += new System.EventHandler(this.Handle_CollectionDateTextBox_TextChanged);
            // 
            // _collectionTimeTextBox
            // 
            this._collectionTimeTextBox.Location = new System.Drawing.Point(293, 3);
            this._collectionTimeTextBox.Name = "_collectionTimeTextBox";
            this._collectionTimeTextBox.Size = new System.Drawing.Size(130, 20);
            this._collectionTimeTextBox.TabIndex = 17;
            this._collectionTimeTextBox.TextChanged += new System.EventHandler(this.Handle_CollectionTimeTextBox_TextChanged);
            // 
            // LabResultsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Controls.Add(this._collectionTimeTextBox);
            this.Controls.Add(label1);
            this.Controls.Add(this._collectionDateTextBox);
            this.Controls.Add(label5);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "LabResultsPanel";
            this.Size = new System.Drawing.Size(424, 27);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _collectionDateTextBox;
        private System.Windows.Forms.TextBox _collectionTimeTextBox;

    }
}
