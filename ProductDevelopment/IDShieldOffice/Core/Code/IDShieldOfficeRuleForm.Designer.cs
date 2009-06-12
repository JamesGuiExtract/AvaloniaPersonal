namespace IDShieldOffice
{
    partial class IDShieldOfficeRuleForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IDShieldOfficeRuleForm));
            this._resetButton = new System.Windows.Forms.Button();
            this._findNextButton = new System.Windows.Forms.Button();
            this._redactButton = new System.Windows.Forms.Button();
            this._redactAllButton = new System.Windows.Forms.Button();
            this._closeButton = new System.Windows.Forms.Button();
            this._splitContainer = new System.Windows.Forms.SplitContainer();
            this._resultsList = new System.Windows.Forms.DataGridView();
            this._statusStrip = new System.Windows.Forms.StatusStrip();
            this._toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._resultsList)).BeginInit();
            this._statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _resetButton
            // 
            this._resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._resetButton.Location = new System.Drawing.Point(12, 7);
            this._resetButton.Name = "_resetButton";
            this._resetButton.Size = new System.Drawing.Size(80, 23);
            this._resetButton.TabIndex = 0;
            this._resetButton.Text = "Reset search";
            this._resetButton.UseVisualStyleBackColor = true;
            this._resetButton.Click += new System.EventHandler(this.HandleResetButton);
            // 
            // _findNextButton
            // 
            this._findNextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._findNextButton.Location = new System.Drawing.Point(98, 7);
            this._findNextButton.Name = "_findNextButton";
            this._findNextButton.Size = new System.Drawing.Size(80, 23);
            this._findNextButton.TabIndex = 1;
            this._findNextButton.Text = "Find next";
            this._findNextButton.UseVisualStyleBackColor = true;
            this._findNextButton.Click += new System.EventHandler(this.HandleFindNextButton);
            // 
            // _redactButton
            // 
            this._redactButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._redactButton.Location = new System.Drawing.Point(184, 7);
            this._redactButton.Name = "_redactButton";
            this._redactButton.Size = new System.Drawing.Size(80, 23);
            this._redactButton.TabIndex = 2;
            this._redactButton.Text = "Redact";
            this._redactButton.UseVisualStyleBackColor = true;
            this._redactButton.Click += new System.EventHandler(this.HandleRedactButton);
            // 
            // _redactAllButton
            // 
            this._redactAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._redactAllButton.Location = new System.Drawing.Point(270, 7);
            this._redactAllButton.Name = "_redactAllButton";
            this._redactAllButton.Size = new System.Drawing.Size(80, 23);
            this._redactAllButton.TabIndex = 3;
            this._redactAllButton.Text = "Redact all";
            this._redactAllButton.UseVisualStyleBackColor = true;
            this._redactAllButton.Click += new System.EventHandler(this.HandleRedactAllButton);
            // 
            // _closeButton
            // 
            this._closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._closeButton.Location = new System.Drawing.Point(356, 7);
            this._closeButton.Name = "_closeButton";
            this._closeButton.Size = new System.Drawing.Size(80, 23);
            this._closeButton.TabIndex = 4;
            this._closeButton.Text = "Close";
            this._closeButton.UseVisualStyleBackColor = true;
            this._closeButton.Click += new System.EventHandler(this.HandleCloseButton);
            // 
            // _splitContainer
            // 
            this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this._splitContainer.IsSplitterFixed = true;
            this._splitContainer.Location = new System.Drawing.Point(0, 0);
            this._splitContainer.Margin = new System.Windows.Forms.Padding(0);
            this._splitContainer.Name = "_splitContainer";
            this._splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _splitContainer.Panel1
            // 
            this._splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(12);
            this._splitContainer.Panel1MinSize = 0;
            // 
            // _splitContainer.Panel2
            // 
            this._splitContainer.Panel2.Controls.Add(this._resultsList);
            this._splitContainer.Panel2.Controls.Add(this._redactButton);
            this._splitContainer.Panel2.Controls.Add(this._closeButton);
            this._splitContainer.Panel2.Controls.Add(this._resetButton);
            this._splitContainer.Panel2.Controls.Add(this._redactAllButton);
            this._splitContainer.Panel2.Controls.Add(this._findNextButton);
            this._splitContainer.Panel2.Controls.Add(this._statusStrip);
            this._splitContainer.Panel2MinSize = 0;
            this._splitContainer.Size = new System.Drawing.Size(448, 133);
            this._splitContainer.SplitterDistance = 0;
            this._splitContainer.SplitterWidth = 1;
            this._splitContainer.TabIndex = 5;
            // 
            // _resultsList
            // 
            this._resultsList.AllowUserToAddRows = false;
            this._resultsList.AllowUserToDeleteRows = false;
            this._resultsList.AllowUserToResizeColumns = false;
            this._resultsList.AllowUserToResizeRows = false;
            this._resultsList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._resultsList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._resultsList.BackgroundColor = System.Drawing.SystemColors.Control;
            this._resultsList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this._resultsList.DefaultCellStyle = dataGridViewCellStyle1;
            this._resultsList.Enabled = false;
            this._resultsList.EnableHeadersVisualStyles = false;
            this._resultsList.Location = new System.Drawing.Point(-1, 44);
            this._resultsList.MultiSelect = false;
            this._resultsList.Name = "_resultsList";
            this._resultsList.ReadOnly = true;
            this._resultsList.RowHeadersVisible = false;
            this._resultsList.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this._resultsList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._resultsList.Size = new System.Drawing.Size(450, 69);
            this._resultsList.TabIndex = 6;
            this._resultsList.TabStop = false;
            // 
            // _statusStrip
            // 
            this._statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._toolStripStatusLabel});
            this._statusStrip.Location = new System.Drawing.Point(0, 112);
            this._statusStrip.Name = "_statusStrip";
            this._statusStrip.Size = new System.Drawing.Size(448, 22);
            this._statusStrip.TabIndex = 5;
            this._statusStrip.Text = "_statusStrip";
            // 
            // _toolStripStatusLabel
            // 
            this._toolStripStatusLabel.Name = "_toolStripStatusLabel";
            this._toolStripStatusLabel.Size = new System.Drawing.Size(402, 17);
            this._toolStripStatusLabel.Spring = true;
            this._toolStripStatusLabel.Text = "_toolStripStatusLabel";
            this._toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // IDShieldOfficeRuleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 133);
            this.Controls.Add(this._splitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "IDShieldOfficeRuleForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "IDShieldOfficeRuleForm";
            this._splitContainer.Panel2.ResumeLayout(false);
            this._splitContainer.Panel2.PerformLayout();
            this._splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._resultsList)).EndInit();
            this._statusStrip.ResumeLayout(false);
            this._statusStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _resetButton;
        private System.Windows.Forms.Button _findNextButton;
        private System.Windows.Forms.Button _redactButton;
        private System.Windows.Forms.Button _redactAllButton;
        private System.Windows.Forms.Button _closeButton;
        private System.Windows.Forms.SplitContainer _splitContainer;
        private System.Windows.Forms.StatusStrip _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _toolStripStatusLabel;
        private System.Windows.Forms.DataGridView _resultsList;
    }
}