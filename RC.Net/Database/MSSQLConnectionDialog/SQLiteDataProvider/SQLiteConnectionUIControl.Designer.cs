namespace Microsoft.Data.ConnectionUI
{
	partial class SQLiteConnectionUIControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SQLiteConnectionUIControl));
            this.propertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.databaseButtonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.browseButton = new System.Windows.Forms.Button();
            this.createButton = new System.Windows.Forms.Button();
            this.databaseTextBox = new System.Windows.Forms.TextBox();
            this.databaseLabel = new System.Windows.Forms.Label();
            this.propertiesGroupBox.SuspendLayout();
            this.databaseButtonsTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // propertiesGroupBox
            // 
            resources.ApplyResources(this.propertiesGroupBox, "propertiesGroupBox");
            this.propertiesGroupBox.Controls.Add(this.databaseButtonsTableLayoutPanel);
            this.propertiesGroupBox.Controls.Add(this.databaseTextBox);
            this.propertiesGroupBox.Controls.Add(this.databaseLabel);
            this.propertiesGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.propertiesGroupBox.Name = "propertiesGroupBox";
            this.propertiesGroupBox.TabStop = false;
            // 
            // databaseButtonsTableLayoutPanel
            // 
            resources.ApplyResources(this.databaseButtonsTableLayoutPanel, "databaseButtonsTableLayoutPanel");
            this.databaseButtonsTableLayoutPanel.Controls.Add(this.browseButton, 1, 0);
            this.databaseButtonsTableLayoutPanel.Controls.Add(this.createButton, 0, 0);
            this.databaseButtonsTableLayoutPanel.Name = "databaseButtonsTableLayoutPanel";
            // 
            // browseButton
            // 
            resources.ApplyResources(this.browseButton, "browseButton");
            this.browseButton.Name = "browseButton";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // createButton
            // 
            resources.ApplyResources(this.createButton, "createButton");
            this.createButton.Name = "createButton";
            this.createButton.UseVisualStyleBackColor = true;
            // 
            // databaseTextBox
            // 
            resources.ApplyResources(this.databaseTextBox, "databaseTextBox");
            this.databaseTextBox.Name = "databaseTextBox";
            this.databaseTextBox.TextChanged += new System.EventHandler(this.databaseTextBox_TextChanged);
            this.databaseTextBox.Leave += new System.EventHandler(this.TrimControlText);
            // 
            // databaseLabel
            // 
            resources.ApplyResources(this.databaseLabel, "databaseLabel");
            this.databaseLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.databaseLabel.Name = "databaseLabel";
            // 
            // SQLiteConnectionUIControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.propertiesGroupBox);
            this.Name = "SQLiteConnectionUIControl";
            this.propertiesGroupBox.ResumeLayout(false);
            this.propertiesGroupBox.PerformLayout();
            this.databaseButtonsTableLayoutPanel.ResumeLayout(false);
            this.databaseButtonsTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.GroupBox propertiesGroupBox;
		private System.Windows.Forms.Label databaseLabel;
		private System.Windows.Forms.TextBox databaseTextBox;
		private System.Windows.Forms.TableLayoutPanel databaseButtonsTableLayoutPanel;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.Button createButton;
	}
}
