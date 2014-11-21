namespace Extract.SQLCDBEditor.Plugins
{
    partial class AlternateTestNameManager
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
            this._imageViewer = new Extract.Imaging.Forms.ImageViewer();
            this._toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this._basicDataGridToolStrip = new System.Windows.Forms.ToolStrip();
            this._previousAKAToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._nextAKAToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._basicTools = new System.Windows.Forms.ToolStrip();
            this._zoomWindowToolStripButton = new Extract.Imaging.Forms.ZoomWindowToolStripButton();
            this._panToolStripButton = new Extract.Imaging.Forms.PanToolStripButton();
            this._viewCommands = new System.Windows.Forms.ToolStrip();
            this._zoomInToolStripButton = new Extract.Imaging.Forms.ZoomInToolStripButton();
            this._zoomOutToolStripButton = new Extract.Imaging.Forms.ZoomOutToolStripButton();
            this._zoomPreviousToolStripButton = new Extract.Imaging.Forms.ZoomPreviousToolStripButton();
            this._zoomNextToolStripButton = new Extract.Imaging.Forms.ZoomNextToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this._fitToPageToolStripButton = new Extract.Imaging.Forms.FitToPageToolStripButton();
            this._fitToWidthToolStripButton = new Extract.Imaging.Forms.FitToWidthToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this._rotateCounterclockwiseToolStripButton = new Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton();
            this._rotateClockwiseToolStripButton = new Extract.Imaging.Forms.RotateClockwiseToolStripButton();
            this._navigationTools = new System.Windows.Forms.ToolStrip();
            this._firstPageToolStripButton = new Extract.Imaging.Forms.FirstPageToolStripButton();
            this._previousPageToolStripButton = new Extract.Imaging.Forms.PreviousPageToolStripButton();
            this._pageNavigationToolStripTextBox = new Extract.Imaging.Forms.PageNavigationToolStripTextBox();
            this._nextPageToolStripButton = new Extract.Imaging.Forms.NextPageToolStripButton();
            this._lastPageToolStripButton = new Extract.Imaging.Forms.LastPageToolStripButton();
            this._toolStripContainer.ContentPanel.SuspendLayout();
            this._toolStripContainer.TopToolStripPanel.SuspendLayout();
            this._toolStripContainer.SuspendLayout();
            this._basicDataGridToolStrip.SuspendLayout();
            this._basicTools.SuspendLayout();
            this._viewCommands.SuspendLayout();
            this._navigationTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // _imageViewer
            // 
            this._imageViewer.AutoOcr = false;
            this._imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._imageViewer.Location = new System.Drawing.Point(0, 0);
            this._imageViewer.MinimumAngularHighlightHeight = 4;
            this._imageViewer.Name = "_imageViewer";
            this._imageViewer.OcrTradeoff = Extract.Imaging.OcrTradeoff.Accurate;
            this._imageViewer.RedactionMode = false;
            this._imageViewer.Size = new System.Drawing.Size(929, 378);
            this._imageViewer.TabIndex = 0;
            this._imageViewer.TabStop = false;
            // 
            // _toolStripContainer
            // 
            this._toolStripContainer.BottomToolStripPanelVisible = false;
            // 
            // _toolStripContainer.ContentPanel
            // 
            this._toolStripContainer.ContentPanel.Controls.Add(this._imageViewer);
            this._toolStripContainer.ContentPanel.Size = new System.Drawing.Size(929, 378);
            this._toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._toolStripContainer.LeftToolStripPanelVisible = false;
            this._toolStripContainer.Location = new System.Drawing.Point(0, 0);
            this._toolStripContainer.Name = "_toolStripContainer";
            this._toolStripContainer.RightToolStripPanelVisible = false;
            this._toolStripContainer.Size = new System.Drawing.Size(929, 417);
            this._toolStripContainer.TabIndex = 1;
            this._toolStripContainer.Text = "toolStripContainer1";
            // 
            // _toolStripContainer.TopToolStripPanel
            // 
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._basicDataGridToolStrip);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._basicTools);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._viewCommands);
            this._toolStripContainer.TopToolStripPanel.Controls.Add(this._navigationTools);
            // 
            // _basicDataGridToolStrip
            // 
            this._basicDataGridToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._basicDataGridToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._basicDataGridToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._previousAKAToolStripButton,
            this._nextAKAToolStripButton});
            this._basicDataGridToolStrip.Location = new System.Drawing.Point(3, 0);
            this._basicDataGridToolStrip.Name = "_basicDataGridToolStrip";
            this._basicDataGridToolStrip.Size = new System.Drawing.Size(84, 39);
            this._basicDataGridToolStrip.TabIndex = 12;
            // 
            // _previousAKAToolStripButton
            // 
            this._previousAKAToolStripButton.ToolTipText = "Go to previous AKA";
            this._previousAKAToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._previousAKAToolStripButton.Enabled = false;
            this._previousAKAToolStripButton.Image = global::Extract.SQLCDBEditor.Plugins.Properties.Resources.PreviousAKAExample;
            this._previousAKAToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._previousAKAToolStripButton.Name = "_previousAKAToolStripButton";
            this._previousAKAToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._previousAKAToolStripButton.Click += new System.EventHandler(this.HandlePreviousExampleClick);
            // 
            // _nextAKAToolStripButton
            // 
            this._nextAKAToolStripButton.ToolTipText = "Go to next AKA";
            this._nextAKAToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextAKAToolStripButton.Enabled = false;
            this._nextAKAToolStripButton.Image = global::Extract.SQLCDBEditor.Plugins.Properties.Resources.NextAKAExample;
            this._nextAKAToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextAKAToolStripButton.Name = "_nextAKAToolStripButton";
            this._nextAKAToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextAKAToolStripButton.Click += new System.EventHandler(this.HandleNextExampleClick);
            // 
            // _basicTools
            // 
            this._basicTools.Dock = System.Windows.Forms.DockStyle.None;
            this._basicTools.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._basicTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomWindowToolStripButton,
            this._panToolStripButton});
            this._basicTools.Location = new System.Drawing.Point(87, 0);
            this._basicTools.Name = "_basicTools";
            this._basicTools.Size = new System.Drawing.Size(84, 39);
            this._basicTools.TabIndex = 10;
            // 
            // _zoomWindowToolStripButton
            // 
            this._zoomWindowToolStripButton.BaseToolTipText = "Zoom window";
            this._zoomWindowToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomWindowToolStripButton.Enabled = false;
            this._zoomWindowToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomWindowToolStripButton.ImageViewer = null;
            this._zoomWindowToolStripButton.Name = "_zoomWindowToolStripButton";
            this._zoomWindowToolStripButton.Size = new System.Drawing.Size(36, 36);
            // 
            // _panToolStripButton
            // 
            this._panToolStripButton.BaseToolTipText = "Pan";
            this._panToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._panToolStripButton.Enabled = false;
            this._panToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._panToolStripButton.ImageViewer = null;
            this._panToolStripButton.Name = "_panToolStripButton";
            this._panToolStripButton.Size = new System.Drawing.Size(36, 36);
            // 
            // _viewCommands
            // 
            this._viewCommands.Dock = System.Windows.Forms.DockStyle.None;
            this._viewCommands.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._viewCommands.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomInToolStripButton,
            this._zoomOutToolStripButton,
            this._zoomPreviousToolStripButton,
            this._zoomNextToolStripButton,
            this.toolStripSeparator8,
            this._fitToPageToolStripButton,
            this._fitToWidthToolStripButton,
            this.toolStripSeparator9,
            this._rotateCounterclockwiseToolStripButton,
            this._rotateClockwiseToolStripButton});
            this._viewCommands.Location = new System.Drawing.Point(172, 0);
            this._viewCommands.Name = "_viewCommands";
            this._viewCommands.Size = new System.Drawing.Size(312, 39);
            this._viewCommands.TabIndex = 9;
            // 
            // _zoomInToolStripButton
            // 
            this._zoomInToolStripButton.BaseToolTipText = "Zoom in";
            this._zoomInToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomInToolStripButton.Enabled = false;
            this._zoomInToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomInToolStripButton.ImageViewer = null;
            this._zoomInToolStripButton.Name = "_zoomInToolStripButton";
            this._zoomInToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomInToolStripButton.Text = "Zoom in";
            // 
            // _zoomOutToolStripButton
            // 
            this._zoomOutToolStripButton.BaseToolTipText = "Zoom out";
            this._zoomOutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomOutToolStripButton.Enabled = false;
            this._zoomOutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomOutToolStripButton.ImageViewer = null;
            this._zoomOutToolStripButton.Name = "_zoomOutToolStripButton";
            this._zoomOutToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomOutToolStripButton.Text = "Zoom out";
            // 
            // _zoomPreviousToolStripButton
            // 
            this._zoomPreviousToolStripButton.BaseToolTipText = "Zoom previous";
            this._zoomPreviousToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomPreviousToolStripButton.Enabled = false;
            this._zoomPreviousToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomPreviousToolStripButton.ImageViewer = null;
            this._zoomPreviousToolStripButton.Name = "_zoomPreviousToolStripButton";
            this._zoomPreviousToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomPreviousToolStripButton.Text = "Zoom previous";
            // 
            // _zoomNextToolStripButton
            // 
            this._zoomNextToolStripButton.BaseToolTipText = "Zoom next";
            this._zoomNextToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomNextToolStripButton.Enabled = false;
            this._zoomNextToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomNextToolStripButton.ImageViewer = null;
            this._zoomNextToolStripButton.Name = "_zoomNextToolStripButton";
            this._zoomNextToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomNextToolStripButton.Text = "Zoom next";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 39);
            // 
            // _fitToPageToolStripButton
            // 
            this._fitToPageToolStripButton.BaseToolTipText = "Fit to page";
            this._fitToPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._fitToPageToolStripButton.Enabled = false;
            this._fitToPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._fitToPageToolStripButton.ImageViewer = null;
            this._fitToPageToolStripButton.Name = "_fitToPageToolStripButton";
            this._fitToPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._fitToPageToolStripButton.Text = "Fit to page";
            // 
            // _fitToWidthToolStripButton
            // 
            this._fitToWidthToolStripButton.BaseToolTipText = "Fit to width";
            this._fitToWidthToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._fitToWidthToolStripButton.Enabled = false;
            this._fitToWidthToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._fitToWidthToolStripButton.ImageViewer = null;
            this._fitToWidthToolStripButton.Name = "_fitToWidthToolStripButton";
            this._fitToWidthToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._fitToWidthToolStripButton.Text = "Fit to width";
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 39);
            // 
            // _rotateCounterclockwiseToolStripButton
            // 
            this._rotateCounterclockwiseToolStripButton.BaseToolTipText = "Rotate counterclockwise";
            this._rotateCounterclockwiseToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._rotateCounterclockwiseToolStripButton.Enabled = false;
            this._rotateCounterclockwiseToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._rotateCounterclockwiseToolStripButton.ImageViewer = null;
            this._rotateCounterclockwiseToolStripButton.Name = "_rotateCounterclockwiseToolStripButton";
            this._rotateCounterclockwiseToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._rotateCounterclockwiseToolStripButton.Text = "Rotate counterclockwise";
            // 
            // _rotateClockwiseToolStripButton
            // 
            this._rotateClockwiseToolStripButton.BaseToolTipText = "Rotate clockwise";
            this._rotateClockwiseToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._rotateClockwiseToolStripButton.Enabled = false;
            this._rotateClockwiseToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._rotateClockwiseToolStripButton.ImageViewer = null;
            this._rotateClockwiseToolStripButton.Name = "_rotateClockwiseToolStripButton";
            this._rotateClockwiseToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._rotateClockwiseToolStripButton.Text = "Rotate clockwise";
            // 
            // _navigationTools
            // 
            this._navigationTools.Dock = System.Windows.Forms.DockStyle.None;
            this._navigationTools.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._navigationTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._firstPageToolStripButton,
            this._previousPageToolStripButton,
            this._pageNavigationToolStripTextBox,
            this._nextPageToolStripButton,
            this._lastPageToolStripButton});
            this._navigationTools.Location = new System.Drawing.Point(484, 0);
            this._navigationTools.Name = "_navigationTools";
            this._navigationTools.Size = new System.Drawing.Size(233, 39);
            this._navigationTools.TabIndex = 11;
            // 
            // _firstPageToolStripButton
            // 
            this._firstPageToolStripButton.BaseToolTipText = "Go to first page";
            this._firstPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._firstPageToolStripButton.Enabled = false;
            this._firstPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._firstPageToolStripButton.ImageViewer = null;
            this._firstPageToolStripButton.Name = "_firstPageToolStripButton";
            this._firstPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._firstPageToolStripButton.Text = "firstPageToolStripButton1";
            // 
            // _previousPageToolStripButton
            // 
            this._previousPageToolStripButton.BaseToolTipText = "Go to previous page";
            this._previousPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._previousPageToolStripButton.Enabled = false;
            this._previousPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._previousPageToolStripButton.ImageViewer = null;
            this._previousPageToolStripButton.Name = "_previousPageToolStripButton";
            this._previousPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._previousPageToolStripButton.Text = "previousPageToolStripButton1";
            // 
            // _pageNavigationToolStripTextBox
            // 
            this._pageNavigationToolStripTextBox.Enabled = false;
            this._pageNavigationToolStripTextBox.ImageViewer = null;
            this._pageNavigationToolStripTextBox.Name = "_pageNavigationToolStripTextBox";
            this._pageNavigationToolStripTextBox.Size = new System.Drawing.Size(75, 39);
            this._pageNavigationToolStripTextBox.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // _nextPageToolStripButton
            // 
            this._nextPageToolStripButton.BaseToolTipText = "Go to next page";
            this._nextPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextPageToolStripButton.Enabled = false;
            this._nextPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextPageToolStripButton.ImageViewer = null;
            this._nextPageToolStripButton.Name = "_nextPageToolStripButton";
            this._nextPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextPageToolStripButton.Text = "nextPageToolStripButton1";
            // 
            // _lastPageToolStripButton
            // 
            this._lastPageToolStripButton.BaseToolTipText = "Go to last page";
            this._lastPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._lastPageToolStripButton.Enabled = false;
            this._lastPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._lastPageToolStripButton.ImageViewer = null;
            this._lastPageToolStripButton.Name = "_lastPageToolStripButton";
            this._lastPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._lastPageToolStripButton.Text = "lastPageToolStripButton1";
            // 
            // AlternateTestNameManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._toolStripContainer);
            this.Name = "AlternateTestNameManager";
            this.Size = new System.Drawing.Size(929, 417);
            this._toolStripContainer.ContentPanel.ResumeLayout(false);
            this._toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this._toolStripContainer.TopToolStripPanel.PerformLayout();
            this._toolStripContainer.ResumeLayout(false);
            this._toolStripContainer.PerformLayout();
            this._basicDataGridToolStrip.ResumeLayout(false);
            this._basicDataGridToolStrip.PerformLayout();
            this._basicTools.ResumeLayout(false);
            this._basicTools.PerformLayout();
            this._viewCommands.ResumeLayout(false);
            this._viewCommands.PerformLayout();
            this._navigationTools.ResumeLayout(false);
            this._navigationTools.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Extract.Imaging.Forms.ImageViewer _imageViewer;
        private System.Windows.Forms.ToolStripContainer _toolStripContainer;
        private System.Windows.Forms.ToolStrip _viewCommands;
        private Imaging.Forms.ZoomInToolStripButton _zoomInToolStripButton;
        private Imaging.Forms.ZoomOutToolStripButton _zoomOutToolStripButton;
        private Imaging.Forms.ZoomPreviousToolStripButton _zoomPreviousToolStripButton;
        private Imaging.Forms.ZoomNextToolStripButton _zoomNextToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private Imaging.Forms.FitToPageToolStripButton _fitToPageToolStripButton;
        private Imaging.Forms.FitToWidthToolStripButton _fitToWidthToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private Imaging.Forms.RotateCounterclockwiseToolStripButton _rotateCounterclockwiseToolStripButton;
        private Imaging.Forms.RotateClockwiseToolStripButton _rotateClockwiseToolStripButton;
        private System.Windows.Forms.ToolStrip _basicTools;
        private Imaging.Forms.ZoomWindowToolStripButton _zoomWindowToolStripButton;
        private Imaging.Forms.PanToolStripButton _panToolStripButton;
        private System.Windows.Forms.ToolStrip _navigationTools;
        private Imaging.Forms.FirstPageToolStripButton _firstPageToolStripButton;
        private Imaging.Forms.PreviousPageToolStripButton _previousPageToolStripButton;
        private Imaging.Forms.PageNavigationToolStripTextBox _pageNavigationToolStripTextBox;
        private Imaging.Forms.NextPageToolStripButton _nextPageToolStripButton;
        private Imaging.Forms.LastPageToolStripButton _lastPageToolStripButton;
        private System.Windows.Forms.ToolStrip _basicDataGridToolStrip;
        private System.Windows.Forms.ToolStripButton _previousAKAToolStripButton;
        private System.Windows.Forms.ToolStripButton _nextAKAToolStripButton;
    }
}
