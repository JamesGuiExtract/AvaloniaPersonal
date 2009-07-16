namespace Extract.Redaction.Verification
{
    partial class VerificationTaskForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="VerificationTaskForm"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="VerificationTaskForm"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources
                if (components != null)
                {
                    components.Dispose();
                }
            }

            // Release unmanaged resources

            // Call base dispose method
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VerificationTaskForm));
            this._dataGridToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._redactionGridView = new Extract.Redaction.Verification.RedactionGridView();
            this._basicDataGridToolStrip = new System.Windows.Forms.ToolStrip();
            this._saveToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._previousDocumentToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._nextDocumentToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._previousRedactionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._nextRedactionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._optionsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._exemptionsToolStrip = new System.Windows.Forms.ToolStrip();
            this._applyExemptionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._lastExemptionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._sandDockManager = new TD.SandDock.SandDockManager();
            this._dataGridDockableWindow = new TD.SandDock.DockableWindow();
            this._dockContainer = new TD.SandDock.DockContainer();
            this._imageViewerToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this._imageViewer = new Extract.Imaging.Forms.ImageViewer();
            this._basicImageViewerToolStrip = new System.Windows.Forms.ToolStrip();
            this._printImageToolStripButton = new Extract.Imaging.Forms.PrintImageToolStripButton();
            this._toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomWindowToolStripButton = new Extract.Imaging.Forms.ZoomWindowToolStripButton();
            this._panToolStripButton = new Extract.Imaging.Forms.PanToolStripButton();
            this._selectLayerObjectToolStripButton = new Extract.Imaging.Forms.SelectLayerObjectToolStripButton();
            this._angularRedactionToolStripButton = new Extract.Imaging.Forms.AngularRedactionToolStripButton();
            this._rectangularRedactionToolStripButton = new Extract.Imaging.Forms.RectangularRedactionToolStripButton();
            this._viewCommandsToolStrip = new System.Windows.Forms.ToolStrip();
            this._zoomInToolStripButton = new Extract.Imaging.Forms.ZoomInToolStripButton();
            this._zoomOutToolStripButton = new Extract.Imaging.Forms.ZoomOutToolStripButton();
            this._zoomPreviousToolStripButton = new Extract.Imaging.Forms.ZoomPreviousToolStripButton();
            this._zoomNextToolStripButton = new Extract.Imaging.Forms.ZoomNextToolStripButton();
            this._toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._fitToPageToolStripButton = new Extract.Imaging.Forms.FitToPageToolStripButton();
            this._fitToWidthToolStripButton = new Extract.Imaging.Forms.FitToWidthToolStripButton();
            this._toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._rotateCounterclockwiseToolStripButton = new Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton();
            this._rotateClockwiseToolStripButton = new Extract.Imaging.Forms.RotateClockwiseToolStripButton();
            this._pageNavigationToolStrip = new System.Windows.Forms.ToolStrip();
            this._firstPageToolStripButton = new Extract.Imaging.Forms.FirstPageToolStripButton();
            this._previousPageToolStripButton = new Extract.Imaging.Forms.PreviousPageToolStripButton();
            this._pageNavigationToolStripTextBox = new Extract.Imaging.Forms.PageNavigationToolStripTextBox();
            this._nextPageToolStripButton = new Extract.Imaging.Forms.NextPageToolStripButton();
            this._lastPageToolStripButton = new Extract.Imaging.Forms.LastPageToolStripButton();
            this._dataGridToolStripContainer.ContentPanel.SuspendLayout();
            this._dataGridToolStripContainer.TopToolStripPanel.SuspendLayout();
            this._dataGridToolStripContainer.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this._basicDataGridToolStrip.SuspendLayout();
            this._exemptionsToolStrip.SuspendLayout();
            this._dataGridDockableWindow.SuspendLayout();
            this._dockContainer.SuspendLayout();
            this._imageViewerToolStripContainer.ContentPanel.SuspendLayout();
            this._imageViewerToolStripContainer.TopToolStripPanel.SuspendLayout();
            this._imageViewerToolStripContainer.SuspendLayout();
            this._basicImageViewerToolStrip.SuspendLayout();
            this._viewCommandsToolStrip.SuspendLayout();
            this._pageNavigationToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _dataGridToolStripContainer
            // 
            // 
            // _dataGridToolStripContainer.ContentPanel
            // 
            this._dataGridToolStripContainer.ContentPanel.Controls.Add(this.splitContainer1);
            this._dataGridToolStripContainer.ContentPanel.Size = new System.Drawing.Size(578, 845);
            this._dataGridToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dataGridToolStripContainer.Location = new System.Drawing.Point(0, 0);
            this._dataGridToolStripContainer.Name = "_dataGridToolStripContainer";
            this._dataGridToolStripContainer.Size = new System.Drawing.Size(578, 884);
            this._dataGridToolStripContainer.TabIndex = 0;
            this._dataGridToolStripContainer.Text = "toolStripContainer2";
            // 
            // _dataGridToolStripContainer.TopToolStripPanel
            // 
            this._dataGridToolStripContainer.TopToolStripPanel.Controls.Add(this._basicDataGridToolStrip);
            this._dataGridToolStripContainer.TopToolStripPanel.Controls.Add(this._exemptionsToolStrip);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._redactionGridView);
            this.splitContainer1.Size = new System.Drawing.Size(578, 845);
            this.splitContainer1.SplitterDistance = 166;
            this.splitContainer1.TabIndex = 0;
            // 
            // _redactionGridView
            // 
            this._redactionGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._redactionGridView.ImageViewer = null;
            this._redactionGridView.Location = new System.Drawing.Point(0, 0);
            this._redactionGridView.Name = "_redactionGridView";
            this._redactionGridView.Size = new System.Drawing.Size(578, 166);
            this._redactionGridView.TabIndex = 0;
            // 
            // _basicDataGridToolStrip
            // 
            this._basicDataGridToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._basicDataGridToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._basicDataGridToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._saveToolStripButton,
            this.toolStripSeparator1,
            this._previousDocumentToolStripButton,
            this._nextDocumentToolStripButton,
            this.toolStripSeparator2,
            this._previousRedactionToolStripButton,
            this._nextRedactionToolStripButton,
            this.toolStripSeparator3,
            this._optionsToolStripButton});
            this._basicDataGridToolStrip.Location = new System.Drawing.Point(3, 0);
            this._basicDataGridToolStrip.Name = "_basicDataGridToolStrip";
            this._basicDataGridToolStrip.Size = new System.Drawing.Size(246, 39);
            this._basicDataGridToolStrip.TabIndex = 0;
            // 
            // _saveToolStripButton
            // 
            this._saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._saveToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_saveToolStripButton.Image")));
            this._saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._saveToolStripButton.Name = "_saveToolStripButton";
            this._saveToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._saveToolStripButton.Text = "Save";
            this._saveToolStripButton.Click += new System.EventHandler(this.HandleSaveToolStripButtonClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 39);
            // 
            // _previousDocumentToolStripButton
            // 
            this._previousDocumentToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._previousDocumentToolStripButton.Enabled = false;
            this._previousDocumentToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_previousDocumentToolStripButton.Image")));
            this._previousDocumentToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._previousDocumentToolStripButton.Name = "_previousDocumentToolStripButton";
            this._previousDocumentToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._previousDocumentToolStripButton.Text = "Previous document";
            // 
            // _nextDocumentToolStripButton
            // 
            this._nextDocumentToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextDocumentToolStripButton.Enabled = false;
            this._nextDocumentToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_nextDocumentToolStripButton.Image")));
            this._nextDocumentToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextDocumentToolStripButton.Name = "_nextDocumentToolStripButton";
            this._nextDocumentToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextDocumentToolStripButton.Text = "Next document";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 39);
            // 
            // _previousRedactionToolStripButton
            // 
            this._previousRedactionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._previousRedactionToolStripButton.Enabled = false;
            this._previousRedactionToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_previousRedactionToolStripButton.Image")));
            this._previousRedactionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._previousRedactionToolStripButton.Name = "_previousRedactionToolStripButton";
            this._previousRedactionToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._previousRedactionToolStripButton.Text = "Previous redaction";
            // 
            // _nextRedactionToolStripButton
            // 
            this._nextRedactionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextRedactionToolStripButton.Enabled = false;
            this._nextRedactionToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_nextRedactionToolStripButton.Image")));
            this._nextRedactionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextRedactionToolStripButton.Name = "_nextRedactionToolStripButton";
            this._nextRedactionToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextRedactionToolStripButton.Text = "Next redaction";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 39);
            // 
            // _optionsToolStripButton
            // 
            this._optionsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._optionsToolStripButton.Enabled = false;
            this._optionsToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_optionsToolStripButton.Image")));
            this._optionsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._optionsToolStripButton.Name = "_optionsToolStripButton";
            this._optionsToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._optionsToolStripButton.Text = "Options";
            // 
            // _exemptionsToolStrip
            // 
            this._exemptionsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._exemptionsToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._exemptionsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._applyExemptionToolStripButton,
            this._lastExemptionToolStripButton});
            this._exemptionsToolStrip.Location = new System.Drawing.Point(249, 0);
            this._exemptionsToolStrip.Name = "_exemptionsToolStrip";
            this._exemptionsToolStrip.Size = new System.Drawing.Size(84, 39);
            this._exemptionsToolStrip.TabIndex = 1;
            // 
            // _applyExemptionToolStripButton
            // 
            this._applyExemptionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._applyExemptionToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_applyExemptionToolStripButton.Image")));
            this._applyExemptionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._applyExemptionToolStripButton.Name = "_applyExemptionToolStripButton";
            this._applyExemptionToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._applyExemptionToolStripButton.Text = "Apply exemption codes";
            this._applyExemptionToolStripButton.Click += new System.EventHandler(this.HandleApplyExemptionToolStripButtonClick);
            // 
            // _lastExemptionToolStripButton
            // 
            this._lastExemptionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._lastExemptionToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_lastExemptionToolStripButton.Image")));
            this._lastExemptionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._lastExemptionToolStripButton.Name = "_lastExemptionToolStripButton";
            this._lastExemptionToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._lastExemptionToolStripButton.Text = "Apply last exemption codes";
            this._lastExemptionToolStripButton.Click += new System.EventHandler(this.HandleLastExemptionToolStripButtonClick);
            // 
            // _sandDockManager
            // 
            this._sandDockManager.DockSystemContainer = this;
            this._sandDockManager.MaximumDockContainerSize = 2000;
            this._sandDockManager.MinimumDockContainerSize = 575;
            this._sandDockManager.OwnerForm = this;
            // 
            // _dataGridDockableWindow
            // 
            this._dataGridDockableWindow.Controls.Add(this._dataGridToolStripContainer);
            this._dataGridDockableWindow.Guid = new System.Guid("9a0fd258-12fb-4a21-9076-d00f8ce8b1c6");
            this._dataGridDockableWindow.Location = new System.Drawing.Point(0, 18);
            this._dataGridDockableWindow.Name = "_dataGridDockableWindow";
            this._dataGridDockableWindow.Size = new System.Drawing.Size(578, 884);
            this._dataGridDockableWindow.TabIndex = 0;
            this._dataGridDockableWindow.Text = "Data grid";
            // 
            // _dockContainer
            // 
            this._dockContainer.ContentSize = 578;
            this._dockContainer.Controls.Add(this._dataGridDockableWindow);
            this._dockContainer.Dock = System.Windows.Forms.DockStyle.Left;
            this._dockContainer.LayoutSystem = new TD.SandDock.SplitLayoutSystem(new System.Drawing.SizeF(250F, 400F), System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 400F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._dataGridDockableWindow))}, this._dataGridDockableWindow)))});
            this._dockContainer.Location = new System.Drawing.Point(0, 0);
            this._dockContainer.Manager = this._sandDockManager;
            this._dockContainer.Name = "_dockContainer";
            this._dockContainer.Size = new System.Drawing.Size(582, 926);
            this._dockContainer.TabIndex = 0;
            // 
            // _imageViewerToolStripContainer
            // 
            // 
            // _imageViewerToolStripContainer.ContentPanel
            // 
            this._imageViewerToolStripContainer.ContentPanel.Controls.Add(this._imageViewer);
            this._imageViewerToolStripContainer.ContentPanel.Size = new System.Drawing.Size(1010, 887);
            this._imageViewerToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._imageViewerToolStripContainer.Location = new System.Drawing.Point(582, 0);
            this._imageViewerToolStripContainer.Name = "_imageViewerToolStripContainer";
            this._imageViewerToolStripContainer.Size = new System.Drawing.Size(1010, 926);
            this._imageViewerToolStripContainer.TabIndex = 1;
            this._imageViewerToolStripContainer.Text = "toolStripContainer1";
            // 
            // _imageViewerToolStripContainer.TopToolStripPanel
            // 
            this._imageViewerToolStripContainer.TopToolStripPanel.Controls.Add(this._basicImageViewerToolStrip);
            this._imageViewerToolStripContainer.TopToolStripPanel.Controls.Add(this._viewCommandsToolStrip);
            this._imageViewerToolStripContainer.TopToolStripPanel.Controls.Add(this._pageNavigationToolStrip);
            // 
            // _imageViewer
            // 
            this._imageViewer.DefaultStatusMessage = null;
            this._imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._imageViewer.Location = new System.Drawing.Point(0, 0);
            this._imageViewer.Name = "_imageViewer";
            this._imageViewer.Size = new System.Drawing.Size(1089, 887);
            this._imageViewer.TabIndex = 0;
            this._imageViewer.TabStop = false;
            this._imageViewer.UseDefaultShortcuts = true;
            this._imageViewer.Watermark = null;
            // 
            // _basicImageViewerToolStrip
            // 
            this._basicImageViewerToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._basicImageViewerToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._basicImageViewerToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._printImageToolStripButton,
            this._toolStripSeparator1,
            this._zoomWindowToolStripButton,
            this._panToolStripButton,
            this._selectLayerObjectToolStripButton,
            this._angularRedactionToolStripButton,
            this._rectangularRedactionToolStripButton});
            this._basicImageViewerToolStrip.Location = new System.Drawing.Point(3, 0);
            this._basicImageViewerToolStrip.Name = "_basicImageViewerToolStrip";
            this._basicImageViewerToolStrip.Size = new System.Drawing.Size(234, 39);
            this._basicImageViewerToolStrip.TabIndex = 0;
            // 
            // _printImageToolStripButton
            // 
            this._printImageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._printImageToolStripButton.Enabled = false;
            this._printImageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._printImageToolStripButton.ImageViewer = null;
            this._printImageToolStripButton.Name = "_printImageToolStripButton";
            this._printImageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._printImageToolStripButton.Text = "Print image";
            this._printImageToolStripButton.ToolTipText = "Print image";
            // 
            // _toolStripSeparator1
            // 
            this._toolStripSeparator1.Name = "_toolStripSeparator1";
            this._toolStripSeparator1.Size = new System.Drawing.Size(6, 39);
            // 
            // _zoomWindowToolStripButton
            // 
            this._zoomWindowToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomWindowToolStripButton.Enabled = false;
            this._zoomWindowToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomWindowToolStripButton.ImageViewer = null;
            this._zoomWindowToolStripButton.Name = "_zoomWindowToolStripButton";
            this._zoomWindowToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomWindowToolStripButton.ToolTipText = "Zoom window";
            // 
            // _panToolStripButton
            // 
            this._panToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._panToolStripButton.Enabled = false;
            this._panToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._panToolStripButton.ImageViewer = null;
            this._panToolStripButton.Name = "_panToolStripButton";
            this._panToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._panToolStripButton.ToolTipText = "Pan";
            // 
            // _selectLayerObjectToolStripButton
            // 
            this._selectLayerObjectToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._selectLayerObjectToolStripButton.Enabled = false;
            this._selectLayerObjectToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._selectLayerObjectToolStripButton.ImageViewer = null;
            this._selectLayerObjectToolStripButton.Name = "_selectLayerObjectToolStripButton";
            this._selectLayerObjectToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._selectLayerObjectToolStripButton.ToolTipText = "Select redactions and other objects";
            // 
            // _angularRedactionToolStripButton
            // 
            this._angularRedactionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._angularRedactionToolStripButton.Enabled = false;
            this._angularRedactionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._angularRedactionToolStripButton.ImageViewer = null;
            this._angularRedactionToolStripButton.Name = "_angularRedactionToolStripButton";
            this._angularRedactionToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._angularRedactionToolStripButton.ToolTipText = "Create angular redaction";
            // 
            // _rectangularRedactionToolStripButton
            // 
            this._rectangularRedactionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._rectangularRedactionToolStripButton.Enabled = false;
            this._rectangularRedactionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._rectangularRedactionToolStripButton.ImageViewer = null;
            this._rectangularRedactionToolStripButton.Name = "_rectangularRedactionToolStripButton";
            this._rectangularRedactionToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._rectangularRedactionToolStripButton.ToolTipText = "Create rectangular redaction";
            // 
            // _viewCommandsToolStrip
            // 
            this._viewCommandsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._viewCommandsToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._viewCommandsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomInToolStripButton,
            this._zoomOutToolStripButton,
            this._zoomPreviousToolStripButton,
            this._zoomNextToolStripButton,
            this._toolStripSeparator2,
            this._fitToPageToolStripButton,
            this._fitToWidthToolStripButton,
            this._toolStripSeparator3,
            this._rotateCounterclockwiseToolStripButton,
            this._rotateClockwiseToolStripButton});
            this._viewCommandsToolStrip.Location = new System.Drawing.Point(470, 0);
            this._viewCommandsToolStrip.Name = "_viewCommandsToolStrip";
            this._viewCommandsToolStrip.Size = new System.Drawing.Size(312, 39);
            this._viewCommandsToolStrip.TabIndex = 2;
            // 
            // _zoomInToolStripButton
            // 
            this._zoomInToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomInToolStripButton.Enabled = false;
            this._zoomInToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomInToolStripButton.ImageViewer = null;
            this._zoomInToolStripButton.Name = "_zoomInToolStripButton";
            this._zoomInToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomInToolStripButton.Text = "Zoom in";
            this._zoomInToolStripButton.ToolTipText = "Zoom in";
            // 
            // _zoomOutToolStripButton
            // 
            this._zoomOutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomOutToolStripButton.Enabled = false;
            this._zoomOutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomOutToolStripButton.ImageViewer = null;
            this._zoomOutToolStripButton.Name = "_zoomOutToolStripButton";
            this._zoomOutToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomOutToolStripButton.Text = "Zoom out";
            this._zoomOutToolStripButton.ToolTipText = "Zoom out";
            // 
            // _zoomPreviousToolStripButton
            // 
            this._zoomPreviousToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomPreviousToolStripButton.Enabled = false;
            this._zoomPreviousToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomPreviousToolStripButton.ImageViewer = null;
            this._zoomPreviousToolStripButton.Name = "_zoomPreviousToolStripButton";
            this._zoomPreviousToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomPreviousToolStripButton.Text = "Zoom previous";
            this._zoomPreviousToolStripButton.ToolTipText = "Zoom previous";
            // 
            // _zoomNextToolStripButton
            // 
            this._zoomNextToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._zoomNextToolStripButton.Enabled = false;
            this._zoomNextToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._zoomNextToolStripButton.ImageViewer = null;
            this._zoomNextToolStripButton.Name = "_zoomNextToolStripButton";
            this._zoomNextToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._zoomNextToolStripButton.Text = "Zoom next";
            this._zoomNextToolStripButton.ToolTipText = "Zoom next";
            // 
            // _toolStripSeparator2
            // 
            this._toolStripSeparator2.Name = "_toolStripSeparator2";
            this._toolStripSeparator2.Size = new System.Drawing.Size(6, 39);
            // 
            // _fitToPageToolStripButton
            // 
            this._fitToPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._fitToPageToolStripButton.Enabled = false;
            this._fitToPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._fitToPageToolStripButton.ImageViewer = null;
            this._fitToPageToolStripButton.Name = "_fitToPageToolStripButton";
            this._fitToPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._fitToPageToolStripButton.Text = "Fit to page";
            this._fitToPageToolStripButton.ToolTipText = "Fit to page";
            // 
            // _fitToWidthToolStripButton
            // 
            this._fitToWidthToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._fitToWidthToolStripButton.Enabled = false;
            this._fitToWidthToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._fitToWidthToolStripButton.ImageViewer = null;
            this._fitToWidthToolStripButton.Name = "_fitToWidthToolStripButton";
            this._fitToWidthToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._fitToWidthToolStripButton.Text = "Fit to width";
            this._fitToWidthToolStripButton.ToolTipText = "Fit to width";
            // 
            // _toolStripSeparator3
            // 
            this._toolStripSeparator3.Name = "_toolStripSeparator3";
            this._toolStripSeparator3.Size = new System.Drawing.Size(6, 39);
            // 
            // _rotateCounterclockwiseToolStripButton
            // 
            this._rotateCounterclockwiseToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._rotateCounterclockwiseToolStripButton.Enabled = false;
            this._rotateCounterclockwiseToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._rotateCounterclockwiseToolStripButton.ImageViewer = null;
            this._rotateCounterclockwiseToolStripButton.Name = "_rotateCounterclockwiseToolStripButton";
            this._rotateCounterclockwiseToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._rotateCounterclockwiseToolStripButton.Text = "Rotate counterclockwise";
            this._rotateCounterclockwiseToolStripButton.ToolTipText = "Rotate counterclockwise";
            // 
            // _rotateClockwiseToolStripButton
            // 
            this._rotateClockwiseToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._rotateClockwiseToolStripButton.Enabled = false;
            this._rotateClockwiseToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._rotateClockwiseToolStripButton.ImageViewer = null;
            this._rotateClockwiseToolStripButton.Name = "_rotateClockwiseToolStripButton";
            this._rotateClockwiseToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._rotateClockwiseToolStripButton.Text = "Rotate clockwise";
            this._rotateClockwiseToolStripButton.ToolTipText = "Rotate clockwise";
            // 
            // _pageNavigationToolStrip
            // 
            this._pageNavigationToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._pageNavigationToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._pageNavigationToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._firstPageToolStripButton,
            this._previousPageToolStripButton,
            this._pageNavigationToolStripTextBox,
            this._nextPageToolStripButton,
            this._lastPageToolStripButton});
            this._pageNavigationToolStrip.Location = new System.Drawing.Point(237, 0);
            this._pageNavigationToolStrip.Name = "_pageNavigationToolStrip";
            this._pageNavigationToolStrip.Size = new System.Drawing.Size(233, 39);
            this._pageNavigationToolStrip.TabIndex = 1;
            // 
            // _firstPageToolStripButton
            // 
            this._firstPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._firstPageToolStripButton.Enabled = false;
            this._firstPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._firstPageToolStripButton.ImageViewer = null;
            this._firstPageToolStripButton.Name = "_firstPageToolStripButton";
            this._firstPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._firstPageToolStripButton.Text = "firstPageToolStripButton1";
            this._firstPageToolStripButton.ToolTipText = "Go to first page";
            // 
            // _previousPageToolStripButton
            // 
            this._previousPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._previousPageToolStripButton.Enabled = false;
            this._previousPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._previousPageToolStripButton.ImageViewer = null;
            this._previousPageToolStripButton.Name = "_previousPageToolStripButton";
            this._previousPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._previousPageToolStripButton.Text = "previousPageToolStripButton1";
            this._previousPageToolStripButton.ToolTipText = "Go to previous page";
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
            this._nextPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextPageToolStripButton.Enabled = false;
            this._nextPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextPageToolStripButton.ImageViewer = null;
            this._nextPageToolStripButton.Name = "_nextPageToolStripButton";
            this._nextPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextPageToolStripButton.Text = "nextPageToolStripButton1";
            this._nextPageToolStripButton.ToolTipText = "Go to next page";
            // 
            // _lastPageToolStripButton
            // 
            this._lastPageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._lastPageToolStripButton.Enabled = false;
            this._lastPageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._lastPageToolStripButton.ImageViewer = null;
            this._lastPageToolStripButton.Name = "_lastPageToolStripButton";
            this._lastPageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._lastPageToolStripButton.Text = "lastPageToolStripButton1";
            this._lastPageToolStripButton.ToolTipText = "Go to last page";
            // 
            // VerificationTaskForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1592, 926);
            this.Controls.Add(this._imageViewerToolStripContainer);
            this.Controls.Add(this._dockContainer);
            this.Name = "VerificationTaskForm";
            this.ShowIcon = false;
            this.Text = "ID Shield Verification";
            this._dataGridToolStripContainer.ContentPanel.ResumeLayout(false);
            this._dataGridToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this._dataGridToolStripContainer.TopToolStripPanel.PerformLayout();
            this._dataGridToolStripContainer.ResumeLayout(false);
            this._dataGridToolStripContainer.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this._basicDataGridToolStrip.ResumeLayout(false);
            this._basicDataGridToolStrip.PerformLayout();
            this._exemptionsToolStrip.ResumeLayout(false);
            this._exemptionsToolStrip.PerformLayout();
            this._dataGridDockableWindow.ResumeLayout(false);
            this._dockContainer.ResumeLayout(false);
            this._imageViewerToolStripContainer.ContentPanel.ResumeLayout(false);
            this._imageViewerToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this._imageViewerToolStripContainer.TopToolStripPanel.PerformLayout();
            this._imageViewerToolStripContainer.ResumeLayout(false);
            this._imageViewerToolStripContainer.PerformLayout();
            this._basicImageViewerToolStrip.ResumeLayout(false);
            this._basicImageViewerToolStrip.PerformLayout();
            this._viewCommandsToolStrip.ResumeLayout(false);
            this._viewCommandsToolStrip.PerformLayout();
            this._pageNavigationToolStrip.ResumeLayout(false);
            this._pageNavigationToolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer _dataGridToolStripContainer;
        private TD.SandDock.SandDockManager _sandDockManager;
        private TD.SandDock.DockableWindow _dataGridDockableWindow;
        private TD.SandDock.DockContainer _dockContainer;
        private System.Windows.Forms.ToolStripContainer _imageViewerToolStripContainer;
        private Extract.Imaging.Forms.ImageViewer _imageViewer;
        private System.Windows.Forms.ToolStrip _basicImageViewerToolStrip;
        private Extract.Imaging.Forms.PrintImageToolStripButton _printImageToolStripButton;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator1;
        private Extract.Imaging.Forms.ZoomWindowToolStripButton _zoomWindowToolStripButton;
        private Extract.Imaging.Forms.PanToolStripButton _panToolStripButton;
        private Extract.Imaging.Forms.SelectLayerObjectToolStripButton _selectLayerObjectToolStripButton;
        private Extract.Imaging.Forms.AngularRedactionToolStripButton _angularRedactionToolStripButton;
        private Extract.Imaging.Forms.RectangularRedactionToolStripButton _rectangularRedactionToolStripButton;
        private System.Windows.Forms.ToolStrip _pageNavigationToolStrip;
        private Extract.Imaging.Forms.FirstPageToolStripButton _firstPageToolStripButton;
        private Extract.Imaging.Forms.PreviousPageToolStripButton _previousPageToolStripButton;
        private Extract.Imaging.Forms.PageNavigationToolStripTextBox _pageNavigationToolStripTextBox;
        private Extract.Imaging.Forms.NextPageToolStripButton _nextPageToolStripButton;
        private Extract.Imaging.Forms.LastPageToolStripButton _lastPageToolStripButton;
        private System.Windows.Forms.ToolStrip _viewCommandsToolStrip;
        private Extract.Imaging.Forms.ZoomInToolStripButton _zoomInToolStripButton;
        private Extract.Imaging.Forms.ZoomOutToolStripButton _zoomOutToolStripButton;
        private Extract.Imaging.Forms.ZoomPreviousToolStripButton _zoomPreviousToolStripButton;
        private Extract.Imaging.Forms.ZoomNextToolStripButton _zoomNextToolStripButton;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator2;
        private Extract.Imaging.Forms.FitToPageToolStripButton _fitToPageToolStripButton;
        private Extract.Imaging.Forms.FitToWidthToolStripButton _fitToWidthToolStripButton;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator3;
        private Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton _rotateCounterclockwiseToolStripButton;
        private Extract.Imaging.Forms.RotateClockwiseToolStripButton _rotateClockwiseToolStripButton;
        private System.Windows.Forms.ToolStrip _basicDataGridToolStrip;
        private System.Windows.Forms.ToolStripButton _saveToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton _previousDocumentToolStripButton;
        private System.Windows.Forms.ToolStripButton _nextDocumentToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton _previousRedactionToolStripButton;
        private System.Windows.Forms.ToolStripButton _nextRedactionToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton _optionsToolStripButton;
        private System.Windows.Forms.ToolStrip _exemptionsToolStrip;
        private System.Windows.Forms.ToolStripButton _applyExemptionToolStripButton;
        private System.Windows.Forms.ToolStripButton _lastExemptionToolStripButton;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private RedactionGridView _redactionGridView;
    }
}