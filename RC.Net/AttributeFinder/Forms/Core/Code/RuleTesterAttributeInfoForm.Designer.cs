namespace Extract.AttributeFinder.Forms
{
    partial class RuleTesterAttributeInfoForm
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
            try
            {
                Cleanup?.Invoke(this, System.EventArgs.Empty);
            }
            catch { }

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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this._closeButton = new System.Windows.Forms.Button();
            this._groupBox = new System.Windows.Forms.GroupBox();
            this._attributeValueComboBox = new System.Windows.Forms.ComboBox();
            this._showValueInUSSViewerButton = new System.Windows.Forms.Button();
            this._characterConfidenceTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this._spatialContentChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._attributeTypeTextBox = new System.Windows.Forms.TextBox();
            this._attributeNameTextBox = new System.Windows.Forms.TextBox();
            this._attributeValueTextBox = new System.Windows.Forms.TextBox();
            this._groupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._spatialContentChart)).BeginInit();
            this.SuspendLayout();
            // 
            // _closeButton
            // 
            this._closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._closeButton.CausesValidation = false;
            this._closeButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this._closeButton.Location = new System.Drawing.Point(497, 526);
            this._closeButton.Name = "_closeButton";
            this._closeButton.Size = new System.Drawing.Size(75, 23);
            this._closeButton.TabIndex = 1;
            this._closeButton.Text = "Close";
            this._closeButton.UseVisualStyleBackColor = true;
            // 
            // _groupBox
            // 
            this._groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._groupBox.Controls.Add(this._attributeValueComboBox);
            this._groupBox.Controls.Add(this._showValueInUSSViewerButton);
            this._groupBox.Controls.Add(this._characterConfidenceTextBox);
            this._groupBox.Controls.Add(this.label5);
            this._groupBox.Controls.Add(this._spatialContentChart);
            this._groupBox.Controls.Add(this.label4);
            this._groupBox.Controls.Add(this.label2);
            this._groupBox.Controls.Add(this.label1);
            this._groupBox.Controls.Add(this._attributeTypeTextBox);
            this._groupBox.Controls.Add(this._attributeNameTextBox);
            this._groupBox.Controls.Add(this._attributeValueTextBox);
            this._groupBox.Location = new System.Drawing.Point(13, 13);
            this._groupBox.Name = "_groupBox";
            this._groupBox.Size = new System.Drawing.Size(559, 507);
            this._groupBox.TabIndex = 0;
            this._groupBox.TabStop = false;
            // 
            // _attributeValueComboBox
            // 
            this._attributeValueComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeValueComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._attributeValueComboBox.FormattingEnabled = true;
            this._attributeValueComboBox.Location = new System.Drawing.Point(6, 122);
            this._attributeValueComboBox.Name = "_attributeValueComboBox";
            this._attributeValueComboBox.Size = new System.Drawing.Size(323, 21);
            this._attributeValueComboBox.TabIndex = 4;
            this._attributeValueComboBox.SelectedIndexChanged += new System.EventHandler(this.HandleAttributeValueComboBox_SelectedIndexChanged);
            // 
            // _showValueInUSSViewerButton
            // 
            this._showValueInUSSViewerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._showValueInUSSViewerButton.Location = new System.Drawing.Point(6, 478);
            this._showValueInUSSViewerButton.Name = "_showValueInUSSViewerButton";
            this._showValueInUSSViewerButton.Size = new System.Drawing.Size(151, 23);
            this._showValueInUSSViewerButton.TabIndex = 8;
            this._showValueInUSSViewerButton.Text = "Show value in USS Viewer";
            this._showValueInUSSViewerButton.UseVisualStyleBackColor = true;
            this._showValueInUSSViewerButton.Click += new System.EventHandler(this.HandleShowValueInUSSViewerButton_Click);
            // 
            // _characterConfidenceTextBox
            // 
            this._characterConfidenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._characterConfidenceTextBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._characterConfidenceTextBox.Location = new System.Drawing.Point(6, 451);
            this._characterConfidenceTextBox.Name = "_characterConfidenceTextBox";
            this._characterConfidenceTextBox.ReadOnly = true;
            this._characterConfidenceTextBox.Size = new System.Drawing.Size(180, 21);
            this._characterConfidenceTextBox.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 435);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(109, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Character confidence";
            // 
            // _spatialContentChart
            // 
            this._spatialContentChart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.AxisX.Interval = 1D;
            chartArea1.AxisX.IsReversed = true;
            chartArea1.AxisX.IsStartedFromZero = false;
            chartArea1.AxisX.LabelAutoFitStyle = ((System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles)((((System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.IncreaseFont | System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.DecreaseFont) 
            | System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.LabelsAngleStep30) 
            | System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.WordWrap)));
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisY.Interval = 10D;
            chartArea1.AxisY.LabelAutoFitStyle = ((System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles)((((System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.IncreaseFont | System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.DecreaseFont) 
            | System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.LabelsAngleStep30) 
            | System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.WordWrap)));
            chartArea1.AxisY.Maximum = 100D;
            chartArea1.AxisY.Minimum = 0D;
            chartArea1.AxisY2.Interval = 10D;
            chartArea1.AxisY2.LabelAutoFitStyle = ((System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles)((((System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.IncreaseFont | System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.DecreaseFont) 
            | System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.LabelsAngleStep30) 
            | System.Windows.Forms.DataVisualization.Charting.LabelAutoFitStyles.WordWrap)));
            chartArea1.AxisY2.MajorGrid.Enabled = false;
            chartArea1.AxisY2.Maximum = 100D;
            chartArea1.AxisY2.Minimum = 0D;
            chartArea1.CursorX.IsUserSelectionEnabled = true;
            chartArea1.CursorY.IsUserSelectionEnabled = true;
            chartArea1.Name = "ChartArea1";
            this._spatialContentChart.ChartAreas.Add(chartArea1);
            this._spatialContentChart.Location = new System.Drawing.Point(335, 34);
            this._spatialContentChart.Name = "_spatialContentChart";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;
            series1.IsValueShownAsLabel = true;
            series1.IsXValueIndexed = true;
            series1.Name = "Series1";
            series1.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            this._spatialContentChart.Series.Add(series1);
            this._spatialContentChart.Size = new System.Drawing.Size(219, 396);
            this._spatialContentChart.TabIndex = 10;
            this._spatialContentChart.Text = "chart1";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(335, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(191, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Spatial content (% black pixels per row)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Type";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            // 
            // _attributeTypeTextBox
            // 
            this._attributeTypeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeTypeTextBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._attributeTypeTextBox.Location = new System.Drawing.Point(6, 78);
            this._attributeTypeTextBox.Name = "_attributeTypeTextBox";
            this._attributeTypeTextBox.ReadOnly = true;
            this._attributeTypeTextBox.Size = new System.Drawing.Size(323, 21);
            this._attributeTypeTextBox.TabIndex = 3;
            // 
            // _attributeNameTextBox
            // 
            this._attributeNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeNameTextBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._attributeNameTextBox.Location = new System.Drawing.Point(6, 34);
            this._attributeNameTextBox.Name = "_attributeNameTextBox";
            this._attributeNameTextBox.ReadOnly = true;
            this._attributeNameTextBox.Size = new System.Drawing.Size(323, 21);
            this._attributeNameTextBox.TabIndex = 1;
            // 
            // _attributeValueTextBox
            // 
            this._attributeValueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._attributeValueTextBox.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._attributeValueTextBox.Location = new System.Drawing.Point(6, 149);
            this._attributeValueTextBox.Multiline = true;
            this._attributeValueTextBox.Name = "_attributeValueTextBox";
            this._attributeValueTextBox.ReadOnly = true;
            this._attributeValueTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._attributeValueTextBox.Size = new System.Drawing.Size(323, 281);
            this._attributeValueTextBox.TabIndex = 5;
            // 
            // RuleTesterAttributeInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.CancelButton = this._closeButton;
            this.ClientSize = new System.Drawing.Size(584, 561);
            this.Controls.Add(this._closeButton);
            this.Controls.Add(this._groupBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "RuleTesterAttributeInfoForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Attribute Information";
            this._groupBox.ResumeLayout(false);
            this._groupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._spatialContentChart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button _closeButton;
        private System.Windows.Forms.GroupBox _groupBox;
        private System.Windows.Forms.DataVisualization.Charting.Chart _spatialContentChart;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox _attributeTypeTextBox;
        private System.Windows.Forms.TextBox _attributeNameTextBox;
        private System.Windows.Forms.TextBox _attributeValueTextBox;
        private System.Windows.Forms.TextBox _characterConfidenceTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button _showValueInUSSViewerButton;
        private System.Windows.Forms.ComboBox _attributeValueComboBox;
    }
}