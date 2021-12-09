
namespace Extract.ETL
{
    partial class DatabaseCleanupForm
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
            Extract.Utilities.ScheduledEvent scheduledEvent1 = new Extract.Utilities.ScheduledEvent();
            this.OK_Button = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this._schedulerControl = new Extract.Utilities.Forms.SchedulerControl();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.betterLabel1 = new Extract.Utilities.Forms.BetterLabel();
            this.label1 = new System.Windows.Forms.Label();
            this._purgeRecordsOlderThanDays = new System.Windows.Forms.NumericUpDown();
            this._descriptionTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.betterLabel2 = new Extract.Utilities.Forms.BetterLabel();
            this._maximumDaysToProcessPerRun = new System.Windows.Forms.NumericUpDown();
            this.CalculateNumberOfRowsToBeDeletedButton = new System.Windows.Forms.Button();
            this.tabPage2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._purgeRecordsOlderThanDays)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._maximumDaysToProcessPerRun)).BeginInit();
            this.SuspendLayout();
            // 
            // OK_Button
            // 
            this.OK_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OK_Button.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK_Button.Location = new System.Drawing.Point(332, 386);
            this.OK_Button.Name = "OK_Button";
            this.OK_Button.Size = new System.Drawing.Size(75, 23);
            this.OK_Button.TabIndex = 70;
            this.OK_Button.Text = "Ok";
            this.OK_Button.UseVisualStyleBackColor = true;
            this.OK_Button.Click += new System.EventHandler(this.OK_Button_Click);
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(413, 386);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 80;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this._schedulerControl);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(405, 183);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Schedule";
            // 
            // _schedulerControl
            // 
            this._schedulerControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._schedulerControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._schedulerControl.Location = new System.Drawing.Point(6, 16);
            this._schedulerControl.Name = "_schedulerControl";
            this._schedulerControl.Size = new System.Drawing.Size(385, 163);
            this._schedulerControl.TabIndex = 51;
            scheduledEvent1.Duration = null;
            scheduledEvent1.Enabled = true;
            scheduledEvent1.End = null;
            scheduledEvent1.Exclusions = new Extract.Utilities.ScheduledEvent[0];
            scheduledEvent1.RecurrenceUnit = null;
            scheduledEvent1.Start = new System.DateTime(2018, 3, 20, 12, 45, 14, 0);
            this._schedulerControl.Value = scheduledEvent1;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(47, 171);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(413, 209);
            this.tabControl1.TabIndex = 50;
            // 
            // betterLabel1
            // 
            this.betterLabel1.AutoSize = true;
            this.betterLabel1.Location = new System.Drawing.Point(44, 44);
            this.betterLabel1.Name = "betterLabel1";
            this.betterLabel1.Size = new System.Drawing.Size(126, 13);
            this.betterLabel1.TabIndex = 6;
            this.betterLabel1.Text = "Purge records older than:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(295, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "days.";
            // 
            // _purgeRecordsOlderThanDays
            // 
            this._purgeRecordsOlderThanDays.Location = new System.Drawing.Point(172, 42);
            this._purgeRecordsOlderThanDays.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this._purgeRecordsOlderThanDays.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this._purgeRecordsOlderThanDays.Name = "_purgeRecordsOlderThanDays";
            this._purgeRecordsOlderThanDays.Size = new System.Drawing.Size(120, 20);
            this._purgeRecordsOlderThanDays.TabIndex = 11;
            this._purgeRecordsOlderThanDays.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
            // 
            // _descriptionTextBox
            // 
            this._descriptionTextBox.Location = new System.Drawing.Point(113, 7);
            this._descriptionTextBox.Name = "_descriptionTextBox";
            this._descriptionTextBox.Size = new System.Drawing.Size(319, 20);
            this._descriptionTextBox.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(44, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Description";
            // 
            // betterLabel2
            // 
            this.betterLabel2.AutoSize = true;
            this.betterLabel2.Location = new System.Drawing.Point(44, 78);
            this.betterLabel2.Name = "betterLabel2";
            this.betterLabel2.Size = new System.Drawing.Size(203, 13);
            this.betterLabel2.TabIndex = 12;
            this.betterLabel2.Text = "Days of records to cleanup per run: ";
            // 
            // _maximumNumberOfRecordsToProcessFromFileTaskSession
            // 
            this._maximumDaysToProcessPerRun.Location = new System.Drawing.Point(223, 76);
            this._maximumDaysToProcessPerRun.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this._maximumDaysToProcessPerRun.Name = "_maximumNumberOfRecordsToProcessFromFileTaskSession";
            this._maximumDaysToProcessPerRun.Size = new System.Drawing.Size(120, 20);
            this._maximumDaysToProcessPerRun.TabIndex = 12;
            // 
            // CalculateNumberOfRowsToBeDeletedButton
            // 
            this.CalculateNumberOfRowsToBeDeletedButton.Location = new System.Drawing.Point(47, 142);
            this.CalculateNumberOfRowsToBeDeletedButton.Name = "CalculateNumberOfRowsToBeDeletedButton";
            this.CalculateNumberOfRowsToBeDeletedButton.Size = new System.Drawing.Size(228, 23);
            this.CalculateNumberOfRowsToBeDeletedButton.TabIndex = 31;
            this.CalculateNumberOfRowsToBeDeletedButton.Text = "Calculate number of rows to be deleted";
            this.CalculateNumberOfRowsToBeDeletedButton.UseVisualStyleBackColor = true;
            this.CalculateNumberOfRowsToBeDeletedButton.Click += new System.EventHandler(this.CalculateNumberOfRowsToBeDeletedButton_Click);
            // 
            // DatabaseCleanupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(500, 421);
            this.Controls.Add(this.CalculateNumberOfRowsToBeDeletedButton);
            this.Controls.Add(this._maximumDaysToProcessPerRun);
            this.Controls.Add(this.betterLabel2);
            this.Controls.Add(this._descriptionTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this._purgeRecordsOlderThanDays);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.betterLabel1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.OK_Button);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(516, 460);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(516, 460);
            this.Name = "DatabaseCleanupForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Database Cleanup Form";
            this.tabPage2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._purgeRecordsOlderThanDays)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._maximumDaysToProcessPerRun)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OK_Button;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.TabPage tabPage2;
        private Utilities.Forms.SchedulerControl _schedulerControl;
        private System.Windows.Forms.TabControl tabControl1;
        private Utilities.Forms.BetterLabel betterLabel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown _purgeRecordsOlderThanDays;
        private System.Windows.Forms.TextBox _descriptionTextBox;
        private System.Windows.Forms.Label label5;
        private Utilities.Forms.BetterLabel betterLabel2;
        private System.Windows.Forms.NumericUpDown _maximumDaysToProcessPerRun;
        private System.Windows.Forms.Button CalculateNumberOfRowsToBeDeletedButton;
    }
}