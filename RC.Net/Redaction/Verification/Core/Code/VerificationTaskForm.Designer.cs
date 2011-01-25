using System;
using TD.SandDock;

namespace Extract.Redaction.Verification
{
    sealed partial class VerificationTaskForm
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
                if (_processingStream != null)
                {
                    _processingStream.Dispose();
                    _processingStream = null;
                }
                // Log that the lock was released if necessary
                if (RegistryManager.LogFileLocking)
                {
                    ExtractException ee = new ExtractException("ELI29943",
                        "Application trace: Processing document unlocked");
                    ee.Log();
                }
                if (_inputEventTracker != null)
                {
                    _inputEventTracker.Dispose();
                    _inputEventTracker = null;
                }
                if (_findOrRedactForm != null)
                {
                    _findOrRedactForm.Dispose();
                    _findOrRedactForm = null;
                }
                if (_formStateManager != null)
                {
                    _formStateManager.Dispose();
                    _formStateManager = null;
                }
                if (_slideshowTimer != null)
                {
                    _slideshowTimer.Dispose();
                    _slideshowTimer = null;
                }

                // Collapsed or hidden dockable windows must be disposed explicitly [FIDSC #4246]
                // TODO: Can be removed when Divelements corrects this in the next release (3.0.7+)
                if (_dataWindowDockableWindow != null)
                {
                    _dataWindowDockableWindow.Dispose();
                    _dataWindowDockableWindow = null;
                }
                if (_thumbnailDockableWindow != null)
                {
                    _thumbnailDockableWindow.Dispose();
                    _thumbnailDockableWindow = null;
                }
                if (_magnifierDockableWindow != null)
                {
                    _magnifierDockableWindow.Dispose();
                    _magnifierDockableWindow = null;
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripContainer dataGridToolStripContainer;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VerificationTaskForm));
            System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
            TD.SandDock.DockContainer dockContainer;
            System.Windows.Forms.ToolStripContainer imageViewerToolStripContainer;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
            TD.SandDock.DockContainer dockContainer1;
            this._dataWindowSplitContainer = new System.Windows.Forms.SplitContainer();
            this._commentsTextBox = new System.Windows.Forms.TextBox();
            this._currentDocumentTextBox = new System.Windows.Forms.TextBox();
            this._documentTypeTextBox = new System.Windows.Forms.TextBox();
            this._redactionGridView = new Extract.Redaction.Verification.RedactionGridView();
            this._pageSummaryView = new Extract.Imaging.Forms.PageSummaryView();
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this._fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveAndCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._printImageToolStripMenuItem = new Extract.Imaging.Forms.PrintImageToolStripMenuItem();
            this._printViewToolStripMenuItem = new Extract.Imaging.Forms.PrintViewToolStripMenuItem();
            this._skipProcessingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._stopProcessingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._discardChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._findOrRedactToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._zoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._fitToPageToolStripMenuItem = new Extract.Imaging.Forms.FitToPageToolStripMenuItem();
            this._fitToWidthToolStripMenuItem = new Extract.Imaging.Forms.FitToWidthToolStripMenuItem();
            this._toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomInToolStripMenuItem = new Extract.Imaging.Forms.ZoomInToolStripMenuItem();
            this._zoomOutToolStripMenuItem = new Extract.Imaging.Forms.ZoomOutToolStripMenuItem();
            this._toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this._zoomPreviousToolStripMenuItem = new Extract.Imaging.Forms.ZoomPreviousToolStripMenuItem();
            this._zoomNextToolStripMenuItem = new Extract.Imaging.Forms.ZoomNextToolStripMenuItem();
            this._rotateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._rotateCounterclockwiseToolStripMenuItem = new Extract.Imaging.Forms.RotateCounterclockwiseToolStripMenuItem();
            this._rotateClockwiseToolStripMenuItem = new Extract.Imaging.Forms.RotateClockwiseToolStripMenuItem();
            this._gotoPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._firstPageToolStripMenuItem = new Extract.Imaging.Forms.FirstPageToolStripMenuItem();
            this._previousPageToolStripMenuItem = new Extract.Imaging.Forms.PreviousPageToolStripMenuItem();
            this._toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this._pageNavigationToolStripMenuItem = new Extract.Imaging.Forms.PageNavigationToolStripMenuItem();
            this._toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this._nextPageToolStripMenuItem = new Extract.Imaging.Forms.NextPageToolStripMenuItem();
            this._lastPageToolStripMenuItem = new Extract.Imaging.Forms.LastPageToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this._fullScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._zoomWindowToolStripMenuItem = new Extract.Imaging.Forms.ZoomWindowToolStripMenuItem();
            this._panToolStripMenuItem = new Extract.Imaging.Forms.PanToolStripMenuItem();
            this._selectLayerObjectToolStripMenuItem = new Extract.Imaging.Forms.SelectLayerObjectToolStripMenuItem();
            this._angularRedactionToolStripMenuItem = new Extract.Imaging.Forms.AngularRedactionToolStripMenuItem();
            this._rectangularRedactionToolStripMenuItem = new Extract.Imaging.Forms.RectangularRedactionToolStripMenuItem();
            this._wordRedactionToolStripMenuItem = new Extract.Imaging.Forms.WordRedactionToolStripMenuItem();
            this._slideshowToolStripMenuItemSeparator = new System.Windows.Forms.ToolStripSeparator();
            this._slideshowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._slideshowConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._slideshowPlayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._slideshowPauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._slideshowStopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this._optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._idShieldHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._aboutIDShieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._basicDataGridToolStrip = new System.Windows.Forms.ToolStrip();
            this._saveAndCommitToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._previousDocumentToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._nextDocumentToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._previousRedactionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._nextRedactionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._exemptionsToolStrip = new System.Windows.Forms.ToolStrip();
            this._applyExemptionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._lastExemptionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._tagFileToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this._tagFileToolStripButton = new Extract.FileActionManager.Forms.TagFileToolStripButton();
            this._slideShowToolStrip = new System.Windows.Forms.ToolStrip();
            this._slideshowConfigToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._slideshowPlayToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._slideshowPauseToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._slideshowStopToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._dataWindowDockableWindow = new TD.SandDock.DockableWindow();
            this._sandDockManager = new TD.SandDock.SandDockManager();
            this._imageViewerStatusStrip = new Extract.Imaging.Forms.ImageViewerStatusStrip();
            this._imageViewer = new Extract.Imaging.Forms.ImageViewer();
            this._imageViewerContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._selectLayerObjectToolStripMenuItem1 = new Extract.Imaging.Forms.SelectLayerObjectToolStripMenuItem();
            this._zoomWindowToolStripMenuItem1 = new Extract.Imaging.Forms.ZoomWindowToolStripMenuItem();
            this._panToolStripMenuItem1 = new Extract.Imaging.Forms.PanToolStripMenuItem();
            this._angularRedactionToolStripMenuItem1 = new Extract.Imaging.Forms.AngularRedactionToolStripMenuItem();
            this._rectangularRedactionToolStripMenuItem1 = new Extract.Imaging.Forms.RectangularRedactionToolStripMenuItem();
            this._wordRedactionToolStripMenuItem1 = new Extract.Imaging.Forms.WordRedactionToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this._blockFitSelectionToolStripMenuItem1 = new Extract.Imaging.Forms.BlockFitSelectionToolStripMenuItem();
            this._viewCommandsToolStrip = new System.Windows.Forms.ToolStrip();
            this._zoomInToolStripButton = new Extract.Imaging.Forms.ZoomInToolStripButton();
            this._zoomOutToolStripButton = new Extract.Imaging.Forms.ZoomOutToolStripButton();
            this._zoomPreviousToolStripButton = new Extract.Imaging.Forms.ZoomPreviousToolStripButton();
            this._zoomNextToolStripButton = new Extract.Imaging.Forms.ZoomNextToolStripButton();
            this._fitToPageToolStripButton = new Extract.Imaging.Forms.FitToPageToolStripButton();
            this._fitToWidthToolStripButton = new Extract.Imaging.Forms.FitToWidthToolStripButton();
            this._previousTileToolStripButton = new Extract.Imaging.Forms.PreviousTileToolStripButton();
            this._nextTileToolStripButton = new Extract.Imaging.Forms.NextTileToolStripButton();
            this._rotateCounterclockwiseToolStripButton = new Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton();
            this._rotateClockwiseToolStripButton = new Extract.Imaging.Forms.RotateClockwiseToolStripButton();
            this._thumbnailsToolStripButton = new Extract.Imaging.Forms.ThumbnailViewerToolStripButton();
            this._pageNavigationToolStrip = new System.Windows.Forms.ToolStrip();
            this._firstPageToolStripButton = new Extract.Imaging.Forms.FirstPageToolStripButton();
            this._previousPageToolStripButton = new Extract.Imaging.Forms.PreviousPageToolStripButton();
            this._pageNavigationToolStripTextBox = new Extract.Imaging.Forms.PageNavigationToolStripTextBox();
            this._nextPageToolStripButton = new Extract.Imaging.Forms.NextPageToolStripButton();
            this._lastPageToolStripButton = new Extract.Imaging.Forms.LastPageToolStripButton();
            this._basicImageViewerToolStrip = new System.Windows.Forms.ToolStrip();
            this._printImageToolStripButton = new Extract.Imaging.Forms.PrintImageToolStripButton();
            this._findOrRedactToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._zoomWindowToolStripButton = new Extract.Imaging.Forms.ZoomWindowToolStripButton();
            this._panToolStripButton = new Extract.Imaging.Forms.PanToolStripButton();
            this._selectLayerObjectToolStripButton = new Extract.Imaging.Forms.SelectLayerObjectToolStripButton();
            this._angularRedactionToolStripButton = new Extract.Imaging.Forms.AngularRedactionToolStripButton();
            this._rectangularRedactionToolStripButton = new Extract.Imaging.Forms.RectangularRedactionToolStripButton();
            this._wordRedactionToolStripButton = new Extract.Imaging.Forms.WordRedactionToolStripButton();
            this._thumbnailDockableWindow = new TD.SandDock.DockableWindow();
            this._thumbnailViewer = new Extract.Imaging.Forms.ThumbnailViewer();
            this._magnifierDockableWindow = new TD.SandDock.DockableWindow();
            this._magnifierControl = new Imaging.Forms.MagnifierControl();
            this._magnifierToolStripButton = new Extract.Imaging.Forms.MagnifierWindowToolStripButton();
            dataGridToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            dockContainer = new TD.SandDock.DockContainer();
            imageViewerToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            dockContainer1 = new TD.SandDock.DockContainer();
            dataGridToolStripContainer.ContentPanel.SuspendLayout();
            dataGridToolStripContainer.TopToolStripPanel.SuspendLayout();
            dataGridToolStripContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dataWindowSplitContainer)).BeginInit();
            this._dataWindowSplitContainer.Panel1.SuspendLayout();
            this._dataWindowSplitContainer.Panel2.SuspendLayout();
            this._dataWindowSplitContainer.SuspendLayout();
            this._menuStrip.SuspendLayout();
            this._basicDataGridToolStrip.SuspendLayout();
            this._exemptionsToolStrip.SuspendLayout();
            this._slideShowToolStrip.SuspendLayout();
            dockContainer.SuspendLayout();
            this._dataWindowDockableWindow.SuspendLayout();
            imageViewerToolStripContainer.BottomToolStripPanel.SuspendLayout();
            imageViewerToolStripContainer.ContentPanel.SuspendLayout();
            imageViewerToolStripContainer.TopToolStripPanel.SuspendLayout();
            imageViewerToolStripContainer.SuspendLayout();
            this._imageViewerContextMenu.SuspendLayout();
            this._viewCommandsToolStrip.SuspendLayout();
            this._pageNavigationToolStrip.SuspendLayout();
            this._basicImageViewerToolStrip.SuspendLayout();
            dockContainer1.SuspendLayout();
            this._thumbnailDockableWindow.SuspendLayout();
            this._magnifierDockableWindow.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridToolStripContainer
            // 
            // 
            // dataGridToolStripContainer.ContentPanel
            // 
            dataGridToolStripContainer.ContentPanel.Controls.Add(this._dataWindowSplitContainer);
            dataGridToolStripContainer.ContentPanel.Size = new System.Drawing.Size(527, 815);
            dataGridToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridToolStripContainer.Location = new System.Drawing.Point(0, 0);
            dataGridToolStripContainer.Name = "dataGridToolStripContainer";
            dataGridToolStripContainer.Size = new System.Drawing.Size(527, 878);
            dataGridToolStripContainer.TabIndex = 0;
            dataGridToolStripContainer.Text = "toolStripContainer2";
            // 
            // dataGridToolStripContainer.TopToolStripPanel
            // 
            dataGridToolStripContainer.TopToolStripPanel.Controls.Add(this._menuStrip);
            dataGridToolStripContainer.TopToolStripPanel.Controls.Add(this._basicDataGridToolStrip);
            dataGridToolStripContainer.TopToolStripPanel.Controls.Add(this._exemptionsToolStrip);
            dataGridToolStripContainer.TopToolStripPanel.Controls.Add(this._slideShowToolStrip);
            // 
            // _dataWindowSplitContainer
            // 
            this._dataWindowSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dataWindowSplitContainer.Location = new System.Drawing.Point(0, 0);
            this._dataWindowSplitContainer.Name = "_dataWindowSplitContainer";
            this._dataWindowSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _dataWindowSplitContainer.Panel1
            // 
            this._dataWindowSplitContainer.Panel1.Controls.Add(this._commentsTextBox);
            this._dataWindowSplitContainer.Panel1.Controls.Add(label3);
            this._dataWindowSplitContainer.Panel1.Controls.Add(this._currentDocumentTextBox);
            this._dataWindowSplitContainer.Panel1.Controls.Add(label2);
            this._dataWindowSplitContainer.Panel1.Controls.Add(this._documentTypeTextBox);
            this._dataWindowSplitContainer.Panel1.Controls.Add(label1);
            this._dataWindowSplitContainer.Panel1.Controls.Add(this._redactionGridView);
            this._dataWindowSplitContainer.Panel1MinSize = 225;
            // 
            // _dataWindowSplitContainer.Panel2
            // 
            this._dataWindowSplitContainer.Panel2.Controls.Add(label4);
            this._dataWindowSplitContainer.Panel2.Controls.Add(this._pageSummaryView);
            this._dataWindowSplitContainer.Panel2MinSize = 150;
            this._dataWindowSplitContainer.Size = new System.Drawing.Size(527, 815);
            this._dataWindowSplitContainer.SplitterDistance = 498;
            this._dataWindowSplitContainer.TabIndex = 0;
            // 
            // _commentsTextBox
            // 
            this._commentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._commentsTextBox.Location = new System.Drawing.Point(7, 106);
            this._commentsTextBox.Multiline = true;
            this._commentsTextBox.Name = "_commentsTextBox";
            this._commentsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._commentsTextBox.Size = new System.Drawing.Size(513, 58);
            this._commentsTextBox.TabIndex = 6;
            this._commentsTextBox.TextChanged += new System.EventHandler(this.HandleCommentsTextBoxTextChanged);
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(4, 89);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(56, 13);
            label3.TabIndex = 5;
            label3.Text = "Comments";
            // 
            // _currentDocumentTextBox
            // 
            this._currentDocumentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._currentDocumentTextBox.Location = new System.Drawing.Point(7, 66);
            this._currentDocumentTextBox.Name = "_currentDocumentTextBox";
            this._currentDocumentTextBox.ReadOnly = true;
            this._currentDocumentTextBox.Size = new System.Drawing.Size(513, 20);
            this._currentDocumentTextBox.TabIndex = 4;
            this._currentDocumentTextBox.TabStop = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(4, 49);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(91, 13);
            label2.TabIndex = 3;
            label2.Text = "Current document";
            // 
            // _documentTypeTextBox
            // 
            this._documentTypeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._documentTypeTextBox.Location = new System.Drawing.Point(7, 26);
            this._documentTypeTextBox.Name = "_documentTypeTextBox";
            this._documentTypeTextBox.ReadOnly = true;
            this._documentTypeTextBox.Size = new System.Drawing.Size(513, 20);
            this._documentTypeTextBox.TabIndex = 2;
            this._documentTypeTextBox.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(4, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(79, 13);
            label1.TabIndex = 1;
            label1.Text = "Document type";
            // 
            // _redactionGridView
            // 
            this._redactionGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._redactionGridView.ImageViewer = null;
            this._redactionGridView.Location = new System.Drawing.Point(7, 170);
            this._redactionGridView.Name = "_redactionGridView";
            this._redactionGridView.Size = new System.Drawing.Size(513, 327);
            this._redactionGridView.TabIndex = 0;
            this._redactionGridView.ExemptionsApplied += new System.EventHandler<Extract.Redaction.Verification.ExemptionsAppliedEventArgs>(this.HandleRedactionGridViewExemptionsApplied);
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(4, 3);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(76, 13);
            label4.TabIndex = 1;
            label4.Text = "Page summary";
            // 
            // _pageSummaryView
            // 
            this._pageSummaryView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._pageSummaryView.ImageViewer = null;
            this._pageSummaryView.Location = new System.Drawing.Point(7, 19);
            this._pageSummaryView.MinimumSize = new System.Drawing.Size(100, 100);
            this._pageSummaryView.Name = "_pageSummaryView";
            this._pageSummaryView.Size = new System.Drawing.Size(513, 284);
            this._pageSummaryView.TabIndex = 0;
            this._pageSummaryView.TabStop = false;
            // 
            // _menuStrip
            // 
            this._menuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileToolStripMenuItem,
            this._editToolStripMenuItem,
            this._viewToolStripMenuItem,
            this._toolsToolStripMenuItem,
            this._helpToolStripMenuItem});
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.Size = new System.Drawing.Size(527, 24);
            this._menuStrip.TabIndex = 2;
            this._menuStrip.Text = "menuStrip1";
            // 
            // _fileToolStripMenuItem
            // 
            this._fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._saveToolStripMenuItem,
            this._saveAndCommitToolStripMenuItem,
            toolStripSeparator7,
            this._printImageToolStripMenuItem,
            this._printViewToolStripMenuItem,
            toolStripSeparator8,
            this._skipProcessingToolStripMenuItem,
            this._stopProcessingToolStripMenuItem});
            this._fileToolStripMenuItem.Name = "_fileToolStripMenuItem";
            this._fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this._fileToolStripMenuItem.Text = "File";
            // 
            // _saveToolStripMenuItem
            // 
            this._saveToolStripMenuItem.Name = "_saveToolStripMenuItem";
            this._saveToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this._saveToolStripMenuItem.Text = "Save";
            this._saveToolStripMenuItem.Click += new System.EventHandler(this.HandleSaveToolStripMenuItemClick);
            // 
            // _saveAndCommitToolStripMenuItem
            // 
            this._saveAndCommitToolStripMenuItem.Name = "_saveAndCommitToolStripMenuItem";
            this._saveAndCommitToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+S";
            this._saveAndCommitToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this._saveAndCommitToolStripMenuItem.Text = "Save and commit";
            this._saveAndCommitToolStripMenuItem.Click += new System.EventHandler(this.HandleSaveAndCommitToolStripMenuItemClick);
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(201, 6);
            // 
            // _printImageToolStripMenuItem
            // 
            this._printImageToolStripMenuItem.Enabled = false;
            this._printImageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_printImageToolStripMenuItem.Image")));
            this._printImageToolStripMenuItem.ImageViewer = null;
            this._printImageToolStripMenuItem.Name = "_printImageToolStripMenuItem";
            this._printImageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._printImageToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this._printImageToolStripMenuItem.Text = "&Print...";
            // 
            // _printViewToolStripMenuItem
            // 
            this._printViewToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._printViewToolStripMenuItem.Enabled = false;
            this._printViewToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_printViewToolStripMenuItem.Image")));
            this._printViewToolStripMenuItem.ImageViewer = null;
            this._printViewToolStripMenuItem.Name = "_printViewToolStripMenuItem";
            this._printViewToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._printViewToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this._printViewToolStripMenuItem.Text = "Print view...";
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new System.Drawing.Size(201, 6);
            // 
            // _skipProcessingToolStripMenuItem
            // 
            this._skipProcessingToolStripMenuItem.Name = "_skipProcessingToolStripMenuItem";
            this._skipProcessingToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this._skipProcessingToolStripMenuItem.Text = "Skip processing";
            this._skipProcessingToolStripMenuItem.Click += new System.EventHandler(this.HandleSkipProcessingToolStripMenuItemClick);
            // 
            // _stopProcessingToolStripMenuItem
            // 
            this._stopProcessingToolStripMenuItem.Name = "_stopProcessingToolStripMenuItem";
            this._stopProcessingToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this._stopProcessingToolStripMenuItem.Text = "Stop processing";
            this._stopProcessingToolStripMenuItem.Click += new System.EventHandler(this.HandleStopProcessingToolStripMenuItemClick);
            // 
            // _editToolStripMenuItem
            // 
            this._editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._discardChangesToolStripMenuItem,
            toolStripSeparator3,
            this._findOrRedactToolStripMenuItem});
            this._editToolStripMenuItem.Name = "_editToolStripMenuItem";
            this._editToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this._editToolStripMenuItem.Text = "Edit";
            // 
            // _discardChangesToolStripMenuItem
            // 
            this._discardChangesToolStripMenuItem.Name = "_discardChangesToolStripMenuItem";
            this._discardChangesToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._discardChangesToolStripMenuItem.Text = "Discard changes";
            this._discardChangesToolStripMenuItem.Click += new System.EventHandler(this.HandleDiscardChangesToolStripMenuItemClick);
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(199, 6);
            // 
            // _findOrRedactToolStripMenuItem
            // 
            this._findOrRedactToolStripMenuItem.Image = global::Extract.Redaction.Verification.Properties.Resources.FindWordsSmall;
            this._findOrRedactToolStripMenuItem.Name = "_findOrRedactToolStripMenuItem";
            this._findOrRedactToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+F";
            this._findOrRedactToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._findOrRedactToolStripMenuItem.Text = "Find or redact...";
            this._findOrRedactToolStripMenuItem.Click += new System.EventHandler(this.HandleFindOrRedactToolStripMenuItemClick);
            // 
            // _viewToolStripMenuItem
            // 
            this._viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomToolStripMenuItem,
            this._rotateToolStripMenuItem,
            this._gotoPageToolStripMenuItem,
            this.toolStripSeparator13,
            this._fullScreenToolStripMenuItem});
            this._viewToolStripMenuItem.Name = "_viewToolStripMenuItem";
            this._viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this._viewToolStripMenuItem.Text = "&View";
            // 
            // _zoomToolStripMenuItem
            // 
            this._zoomToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fitToPageToolStripMenuItem,
            this._fitToWidthToolStripMenuItem,
            this._toolStripSeparator3,
            this._zoomInToolStripMenuItem,
            this._zoomOutToolStripMenuItem,
            this._toolStripSeparator4,
            this._zoomPreviousToolStripMenuItem,
            this._zoomNextToolStripMenuItem});
            this._zoomToolStripMenuItem.Name = "_zoomToolStripMenuItem";
            this._zoomToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this._zoomToolStripMenuItem.Text = "&Zoom";
            // 
            // _fitToPageToolStripMenuItem
            // 
            this._fitToPageToolStripMenuItem.Enabled = false;
            this._fitToPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_fitToPageToolStripMenuItem.Image")));
            this._fitToPageToolStripMenuItem.ImageViewer = null;
            this._fitToPageToolStripMenuItem.Name = "_fitToPageToolStripMenuItem";
            this._fitToPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._fitToPageToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._fitToPageToolStripMenuItem.Text = "Fit to &page";
            // 
            // _fitToWidthToolStripMenuItem
            // 
            this._fitToWidthToolStripMenuItem.Enabled = false;
            this._fitToWidthToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_fitToWidthToolStripMenuItem.Image")));
            this._fitToWidthToolStripMenuItem.ImageViewer = null;
            this._fitToWidthToolStripMenuItem.Name = "_fitToWidthToolStripMenuItem";
            this._fitToWidthToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._fitToWidthToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._fitToWidthToolStripMenuItem.Text = "Fit to &width";
            // 
            // _toolStripSeparator3
            // 
            this._toolStripSeparator3.Name = "_toolStripSeparator3";
            this._toolStripSeparator3.Size = new System.Drawing.Size(152, 6);
            // 
            // _zoomInToolStripMenuItem
            // 
            this._zoomInToolStripMenuItem.Enabled = false;
            this._zoomInToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomInToolStripMenuItem.Image")));
            this._zoomInToolStripMenuItem.ImageViewer = null;
            this._zoomInToolStripMenuItem.Name = "_zoomInToolStripMenuItem";
            this._zoomInToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomInToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._zoomInToolStripMenuItem.Text = "Zoom in";
            // 
            // _zoomOutToolStripMenuItem
            // 
            this._zoomOutToolStripMenuItem.Enabled = false;
            this._zoomOutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomOutToolStripMenuItem.Image")));
            this._zoomOutToolStripMenuItem.ImageViewer = null;
            this._zoomOutToolStripMenuItem.Name = "_zoomOutToolStripMenuItem";
            this._zoomOutToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomOutToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._zoomOutToolStripMenuItem.Text = "Zoom out";
            // 
            // _toolStripSeparator4
            // 
            this._toolStripSeparator4.Name = "_toolStripSeparator4";
            this._toolStripSeparator4.Size = new System.Drawing.Size(152, 6);
            // 
            // _zoomPreviousToolStripMenuItem
            // 
            this._zoomPreviousToolStripMenuItem.Enabled = false;
            this._zoomPreviousToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomPreviousToolStripMenuItem.Image")));
            this._zoomPreviousToolStripMenuItem.ImageViewer = null;
            this._zoomPreviousToolStripMenuItem.Name = "_zoomPreviousToolStripMenuItem";
            this._zoomPreviousToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomPreviousToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._zoomPreviousToolStripMenuItem.Text = "Zoom previous";
            // 
            // _zoomNextToolStripMenuItem
            // 
            this._zoomNextToolStripMenuItem.Enabled = false;
            this._zoomNextToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomNextToolStripMenuItem.Image")));
            this._zoomNextToolStripMenuItem.ImageViewer = null;
            this._zoomNextToolStripMenuItem.Name = "_zoomNextToolStripMenuItem";
            this._zoomNextToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomNextToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this._zoomNextToolStripMenuItem.Text = "Zoom next";
            // 
            // _rotateToolStripMenuItem
            // 
            this._rotateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._rotateCounterclockwiseToolStripMenuItem,
            this._rotateClockwiseToolStripMenuItem});
            this._rotateToolStripMenuItem.Name = "_rotateToolStripMenuItem";
            this._rotateToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this._rotateToolStripMenuItem.Text = "&Rotate";
            // 
            // _rotateCounterclockwiseToolStripMenuItem
            // 
            this._rotateCounterclockwiseToolStripMenuItem.Enabled = false;
            this._rotateCounterclockwiseToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rotateCounterclockwiseToolStripMenuItem.Image")));
            this._rotateCounterclockwiseToolStripMenuItem.ImageViewer = null;
            this._rotateCounterclockwiseToolStripMenuItem.Name = "_rotateCounterclockwiseToolStripMenuItem";
            this._rotateCounterclockwiseToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._rotateCounterclockwiseToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._rotateCounterclockwiseToolStripMenuItem.Text = "Rotate counterclockwise";
            // 
            // _rotateClockwiseToolStripMenuItem
            // 
            this._rotateClockwiseToolStripMenuItem.Enabled = false;
            this._rotateClockwiseToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rotateClockwiseToolStripMenuItem.Image")));
            this._rotateClockwiseToolStripMenuItem.ImageViewer = null;
            this._rotateClockwiseToolStripMenuItem.Name = "_rotateClockwiseToolStripMenuItem";
            this._rotateClockwiseToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._rotateClockwiseToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this._rotateClockwiseToolStripMenuItem.Text = "Rotate clockwise";
            // 
            // _gotoPageToolStripMenuItem
            // 
            this._gotoPageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._firstPageToolStripMenuItem,
            this._previousPageToolStripMenuItem,
            this._toolStripSeparator5,
            this._pageNavigationToolStripMenuItem,
            this._toolStripSeparator6,
            this._nextPageToolStripMenuItem,
            this._lastPageToolStripMenuItem});
            this._gotoPageToolStripMenuItem.Name = "_gotoPageToolStripMenuItem";
            this._gotoPageToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this._gotoPageToolStripMenuItem.Text = "&Goto page";
            // 
            // _firstPageToolStripMenuItem
            // 
            this._firstPageToolStripMenuItem.Enabled = false;
            this._firstPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_firstPageToolStripMenuItem.Image")));
            this._firstPageToolStripMenuItem.ImageViewer = null;
            this._firstPageToolStripMenuItem.Name = "_firstPageToolStripMenuItem";
            this._firstPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._firstPageToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this._firstPageToolStripMenuItem.Text = "First page";
            // 
            // _previousPageToolStripMenuItem
            // 
            this._previousPageToolStripMenuItem.Enabled = false;
            this._previousPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_previousPageToolStripMenuItem.Image")));
            this._previousPageToolStripMenuItem.ImageViewer = null;
            this._previousPageToolStripMenuItem.Name = "_previousPageToolStripMenuItem";
            this._previousPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._previousPageToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this._previousPageToolStripMenuItem.Text = "Previous page";
            // 
            // _toolStripSeparator5
            // 
            this._toolStripSeparator5.Name = "_toolStripSeparator5";
            this._toolStripSeparator5.Size = new System.Drawing.Size(157, 6);
            // 
            // _pageNavigationToolStripMenuItem
            // 
            this._pageNavigationToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._pageNavigationToolStripMenuItem.Enabled = false;
            this._pageNavigationToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_pageNavigationToolStripMenuItem.Image")));
            this._pageNavigationToolStripMenuItem.ImageViewer = null;
            this._pageNavigationToolStripMenuItem.Name = "_pageNavigationToolStripMenuItem";
            this._pageNavigationToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._pageNavigationToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this._pageNavigationToolStripMenuItem.Text = "Page n&umber...";
            // 
            // _toolStripSeparator6
            // 
            this._toolStripSeparator6.Name = "_toolStripSeparator6";
            this._toolStripSeparator6.Size = new System.Drawing.Size(157, 6);
            // 
            // _nextPageToolStripMenuItem
            // 
            this._nextPageToolStripMenuItem.Enabled = false;
            this._nextPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_nextPageToolStripMenuItem.Image")));
            this._nextPageToolStripMenuItem.ImageViewer = null;
            this._nextPageToolStripMenuItem.Name = "_nextPageToolStripMenuItem";
            this._nextPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._nextPageToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this._nextPageToolStripMenuItem.Text = "Next page";
            // 
            // _lastPageToolStripMenuItem
            // 
            this._lastPageToolStripMenuItem.Enabled = false;
            this._lastPageToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_lastPageToolStripMenuItem.Image")));
            this._lastPageToolStripMenuItem.ImageViewer = null;
            this._lastPageToolStripMenuItem.Name = "_lastPageToolStripMenuItem";
            this._lastPageToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._lastPageToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this._lastPageToolStripMenuItem.Text = "Last page";
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(158, 6);
            // 
            // _fullScreenToolStripMenuItem
            // 
            this._fullScreenToolStripMenuItem.Name = "_fullScreenToolStripMenuItem";
            this._fullScreenToolStripMenuItem.ShortcutKeyDisplayString = "F11";
            this._fullScreenToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this._fullScreenToolStripMenuItem.Text = "&Full screen";
            this._fullScreenToolStripMenuItem.Click += new System.EventHandler(this.HandleFullScreenToolStripMenuItemClick);
            // 
            // _toolsToolStripMenuItem
            // 
            this._toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._zoomWindowToolStripMenuItem,
            this._panToolStripMenuItem,
            this._selectLayerObjectToolStripMenuItem,
            this._angularRedactionToolStripMenuItem,
            this._rectangularRedactionToolStripMenuItem,
            this._wordRedactionToolStripMenuItem,
            this._slideshowToolStripMenuItemSeparator,
            this._slideshowToolStripMenuItem,
            this._toolStripSeparator8,
            this._optionsToolStripMenuItem});
            this._toolsToolStripMenuItem.Name = "_toolsToolStripMenuItem";
            this._toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this._toolsToolStripMenuItem.Text = "Tools";
            // 
            // _zoomWindowToolStripMenuItem
            // 
            this._zoomWindowToolStripMenuItem.Enabled = false;
            this._zoomWindowToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_zoomWindowToolStripMenuItem.Image")));
            this._zoomWindowToolStripMenuItem.ImageViewer = null;
            this._zoomWindowToolStripMenuItem.Name = "_zoomWindowToolStripMenuItem";
            this._zoomWindowToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._zoomWindowToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._zoomWindowToolStripMenuItem.Text = "&Zoom window";
            // 
            // _panToolStripMenuItem
            // 
            this._panToolStripMenuItem.Enabled = false;
            this._panToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_panToolStripMenuItem.Image")));
            this._panToolStripMenuItem.ImageViewer = null;
            this._panToolStripMenuItem.Name = "_panToolStripMenuItem";
            this._panToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._panToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._panToolStripMenuItem.Text = "P&an";
            // 
            // _selectLayerObjectToolStripMenuItem
            // 
            this._selectLayerObjectToolStripMenuItem.Enabled = false;
            this._selectLayerObjectToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_selectLayerObjectToolStripMenuItem.Image")));
            this._selectLayerObjectToolStripMenuItem.ImageViewer = null;
            this._selectLayerObjectToolStripMenuItem.Name = "_selectLayerObjectToolStripMenuItem";
            this._selectLayerObjectToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._selectLayerObjectToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._selectLayerObjectToolStripMenuItem.Text = "Select redactions and other objects";
            // 
            // _angularRedactionToolStripMenuItem
            // 
            this._angularRedactionToolStripMenuItem.Enabled = false;
            this._angularRedactionToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_angularRedactionToolStripMenuItem.Image")));
            this._angularRedactionToolStripMenuItem.ImageViewer = null;
            this._angularRedactionToolStripMenuItem.Name = "_angularRedactionToolStripMenuItem";
            this._angularRedactionToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._angularRedactionToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._angularRedactionToolStripMenuItem.Text = "A&ngular redaction";
            // 
            // _rectangularRedactionToolStripMenuItem
            // 
            this._rectangularRedactionToolStripMenuItem.Enabled = false;
            this._rectangularRedactionToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_rectangularRedactionToolStripMenuItem.Image")));
            this._rectangularRedactionToolStripMenuItem.ImageViewer = null;
            this._rectangularRedactionToolStripMenuItem.Name = "_rectangularRedactionToolStripMenuItem";
            this._rectangularRedactionToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._rectangularRedactionToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._rectangularRedactionToolStripMenuItem.Text = "&Rectangular redaction";
            // 
            // _wordRedactionToolStripMenuItem
            // 
            this._wordRedactionToolStripMenuItem.Enabled = false;
            this._wordRedactionToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_wordRedactionToolStripMenuItem.Image")));
            this._wordRedactionToolStripMenuItem.ImageViewer = null;
            this._wordRedactionToolStripMenuItem.Name = "_wordRedactionToolStripMenuItem";
            this._wordRedactionToolStripMenuItem.ShortcutKeyDisplayString = "";
            this._wordRedactionToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._wordRedactionToolStripMenuItem.Text = "&Word redaction";
            // 
            // _slideshowToolStripMenuItemSeparator
            // 
            this._slideshowToolStripMenuItemSeparator.Name = "_slideshowToolStripMenuItemSeparator";
            this._slideshowToolStripMenuItemSeparator.Size = new System.Drawing.Size(252, 6);
            // 
            // _slideshowToolStripMenuItem
            // 
            this._slideshowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._slideshowConfigToolStripMenuItem,
            this._slideshowPlayToolStripMenuItem,
            this._slideshowPauseToolStripMenuItem,
            this._slideshowStopToolStripMenuItem});
            this._slideshowToolStripMenuItem.Name = "_slideshowToolStripMenuItem";
            this._slideshowToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._slideshowToolStripMenuItem.Text = "&Slideshow";
            // 
            // _slideshowConfigToolStripMenuItem
            // 
            this._slideshowConfigToolStripMenuItem.Name = "_slideshowConfigToolStripMenuItem";
            this._slideshowConfigToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this._slideshowConfigToolStripMenuItem.Text = "&Options...";
            this._slideshowConfigToolStripMenuItem.Click += new System.EventHandler(this.HandleSlideshowConfigUIClick);
            // 
            // _slideshowPlayToolStripMenuItem
            // 
            this._slideshowPlayToolStripMenuItem.Name = "_slideshowPlayToolStripMenuItem";
            this._slideshowPlayToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this._slideshowPlayToolStripMenuItem.Text = "&Play";
            this._slideshowPlayToolStripMenuItem.Click += new System.EventHandler(this.HandleSlideshowPlayUIClick);
            // 
            // _slideshowPauseToolStripMenuItem
            // 
            this._slideshowPauseToolStripMenuItem.Name = "_slideshowPauseToolStripMenuItem";
            this._slideshowPauseToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this._slideshowPauseToolStripMenuItem.Text = "P&ause";
            this._slideshowPauseToolStripMenuItem.Click += new System.EventHandler(this.HandleSlideshowPauseUIClick);
            // 
            // _slideshowStopToolStripMenuItem
            // 
            this._slideshowStopToolStripMenuItem.Name = "_slideshowStopToolStripMenuItem";
            this._slideshowStopToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this._slideshowStopToolStripMenuItem.Text = "&Stop";
            this._slideshowStopToolStripMenuItem.Click += new System.EventHandler(this.HandleSlideshowStopUIClick);
            // 
            // _toolStripSeparator8
            // 
            this._toolStripSeparator8.Name = "_toolStripSeparator8";
            this._toolStripSeparator8.Size = new System.Drawing.Size(252, 6);
            // 
            // _optionsToolStripMenuItem
            // 
            this._optionsToolStripMenuItem.Name = "_optionsToolStripMenuItem";
            this._optionsToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this._optionsToolStripMenuItem.Text = "Options...";
            this._optionsToolStripMenuItem.Click += new System.EventHandler(this.HandleOptionsToolStripMenuItemClick);
            // 
            // _helpToolStripMenuItem
            // 
            this._helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._idShieldHelpToolStripMenuItem,
            this._aboutIDShieldToolStripMenuItem});
            this._helpToolStripMenuItem.Name = "_helpToolStripMenuItem";
            this._helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this._helpToolStripMenuItem.Text = "Help";
            // 
            // _idShieldHelpToolStripMenuItem
            // 
            this._idShieldHelpToolStripMenuItem.Name = "_idShieldHelpToolStripMenuItem";
            this._idShieldHelpToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this._idShieldHelpToolStripMenuItem.Text = "ID Shield help";
            this._idShieldHelpToolStripMenuItem.Click += new System.EventHandler(this.HandleIDShieldHelpToolStripMenuItemClick);
            // 
            // _aboutIDShieldToolStripMenuItem
            // 
            this._aboutIDShieldToolStripMenuItem.Name = "_aboutIDShieldToolStripMenuItem";
            this._aboutIDShieldToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this._aboutIDShieldToolStripMenuItem.Text = "About ID Shield";
            this._aboutIDShieldToolStripMenuItem.Click += new System.EventHandler(this.HandleAboutIDShieldToolStripMenuItemClick);
            // 
            // _basicDataGridToolStrip
            // 
            this._basicDataGridToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._basicDataGridToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._basicDataGridToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._saveAndCommitToolStripButton,
            toolStripSeparator1,
            this._previousDocumentToolStripButton,
            this._nextDocumentToolStripButton,
            toolStripSeparator2,
            this._previousRedactionToolStripButton,
            this._nextRedactionToolStripButton});
            this._basicDataGridToolStrip.Location = new System.Drawing.Point(3, 24);
            this._basicDataGridToolStrip.Name = "_basicDataGridToolStrip";
            this._basicDataGridToolStrip.Size = new System.Drawing.Size(204, 39);
            this._basicDataGridToolStrip.TabIndex = 0;
            // 
            // _saveAndCommitToolStripButton
            // 
            this._saveAndCommitToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._saveAndCommitToolStripButton.Enabled = false;
            this._saveAndCommitToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_saveAndCommitToolStripButton.Image")));
            this._saveAndCommitToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._saveAndCommitToolStripButton.Name = "_saveAndCommitToolStripButton";
            this._saveAndCommitToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._saveAndCommitToolStripButton.Text = "Save and Commit (Ctrl+S)";
            this._saveAndCommitToolStripButton.Click += new System.EventHandler(this.HandleSaveToolStripButtonClick);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 39);
            // 
            // _previousDocumentToolStripButton
            // 
            this._previousDocumentToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._previousDocumentToolStripButton.Enabled = false;
            this._previousDocumentToolStripButton.Image = global::Extract.Redaction.Verification.Properties.Resources.PreviousDocument;
            this._previousDocumentToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._previousDocumentToolStripButton.Name = "_previousDocumentToolStripButton";
            this._previousDocumentToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._previousDocumentToolStripButton.Text = "Previous document (Ctrl+Shift+Tab)";
            this._previousDocumentToolStripButton.Click += new System.EventHandler(this.HandlePreviousDocumentToolStripButtonClick);
            // 
            // _nextDocumentToolStripButton
            // 
            this._nextDocumentToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextDocumentToolStripButton.Enabled = false;
            this._nextDocumentToolStripButton.Image = global::Extract.Redaction.Verification.Properties.Resources.NextDocument;
            this._nextDocumentToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextDocumentToolStripButton.Name = "_nextDocumentToolStripButton";
            this._nextDocumentToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextDocumentToolStripButton.Text = "Next document (Ctrl+Tab)";
            this._nextDocumentToolStripButton.Click += new System.EventHandler(this.HandleNextDocumentToolStripButtonClick);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 39);
            // 
            // _previousRedactionToolStripButton
            // 
            this._previousRedactionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._previousRedactionToolStripButton.Enabled = false;
            this._previousRedactionToolStripButton.Image = global::Extract.Redaction.Verification.Properties.Resources.PreviousRedaction;
            this._previousRedactionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._previousRedactionToolStripButton.Name = "_previousRedactionToolStripButton";
            this._previousRedactionToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._previousRedactionToolStripButton.Text = "Previous redaction (Shift+Tab)";
            this._previousRedactionToolStripButton.Click += new System.EventHandler(this.HandlePreviousRedactionToolStripButtonClick);
            // 
            // _nextRedactionToolStripButton
            // 
            this._nextRedactionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextRedactionToolStripButton.Enabled = false;
            this._nextRedactionToolStripButton.Image = global::Extract.Redaction.Verification.Properties.Resources.NextRedaction;
            this._nextRedactionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextRedactionToolStripButton.Name = "_nextRedactionToolStripButton";
            this._nextRedactionToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextRedactionToolStripButton.Text = "Next redaction (Tab)";
            this._nextRedactionToolStripButton.Click += new System.EventHandler(this.HandleNextRedactionToolStripButtonClick);
            // 
            // _exemptionsToolStrip
            // 
            this._exemptionsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._exemptionsToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._exemptionsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._applyExemptionToolStripButton,
            this._lastExemptionToolStripButton,
            this._tagFileToolStripSeparator,
            this._tagFileToolStripButton});
            this._exemptionsToolStrip.Location = new System.Drawing.Point(207, 24);
            this._exemptionsToolStrip.Name = "_exemptionsToolStrip";
            this._exemptionsToolStrip.Size = new System.Drawing.Size(84, 39);
            this._exemptionsToolStrip.TabIndex = 1;
            // 
            // _applyExemptionToolStripButton
            // 
            this._applyExemptionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._applyExemptionToolStripButton.Enabled = false;
            this._applyExemptionToolStripButton.Image = global::Extract.Redaction.Verification.Properties.Resources.Exemption;
            this._applyExemptionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._applyExemptionToolStripButton.Name = "_applyExemptionToolStripButton";
            this._applyExemptionToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._applyExemptionToolStripButton.Text = "Apply exemption codes (E)";
            this._applyExemptionToolStripButton.Click += new System.EventHandler(this.HandleApplyExemptionToolStripButtonClick);
            // 
            // _lastExemptionToolStripButton
            // 
            this._lastExemptionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._lastExemptionToolStripButton.Enabled = false;
            this._lastExemptionToolStripButton.Image = global::Extract.Redaction.Verification.Properties.Resources.LastExemption;
            this._lastExemptionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._lastExemptionToolStripButton.Name = "_lastExemptionToolStripButton";
            this._lastExemptionToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._lastExemptionToolStripButton.Text = "Apply last exemption codes (Ctrl+E)";
            this._lastExemptionToolStripButton.Click += new System.EventHandler(this.HandleLastExemptionToolStripButtonClick);
            // 
            // _tagFileToolStripSeparator
            // 
            this._tagFileToolStripSeparator.Name = "_tagFileToolStripSeparator";
            this._tagFileToolStripSeparator.Size = new System.Drawing.Size(6, 39);
            this._tagFileToolStripSeparator.Visible = false;
            // 
            // _tagFileToolStripButton
            // 
            this._tagFileToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._tagFileToolStripButton.Enabled = false;
            this._tagFileToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._tagFileToolStripButton.Name = "_tagFileToolStripButton";
            this._tagFileToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._tagFileToolStripButton.Text = "Apply tags";
            this._tagFileToolStripButton.Visible = false;
            // 
            // _slideShowToolStrip
            // 
            this._slideShowToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._slideShowToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._slideShowToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._slideshowConfigToolStripButton,
            this._slideshowPlayToolStripButton,
            this._slideshowPauseToolStripButton,
            this._slideshowStopToolStripButton});
            this._slideShowToolStrip.Location = new System.Drawing.Point(291, 24);
            this._slideShowToolStrip.Name = "_slideShowToolStrip";
            this._slideShowToolStrip.Size = new System.Drawing.Size(156, 39);
            this._slideShowToolStrip.TabIndex = 2;
            // 
            // _slideshowConfigToolStripButton
            // 
            this._slideshowConfigToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._slideshowConfigToolStripButton.Image = global::Extract.Redaction.Verification.Properties.Resources.SlideshowConfig;
            this._slideshowConfigToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._slideshowConfigToolStripButton.Name = "_slideshowConfigToolStripButton";
            this._slideshowConfigToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._slideshowConfigToolStripButton.Text = "Slideshow Options";
            this._slideshowConfigToolStripButton.Click += new System.EventHandler(this.HandleSlideshowConfigUIClick);
            // 
            // _slideshowPlayToolStripButton
            // 
            this._slideshowPlayToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._slideshowPlayToolStripButton.Enabled = false;
            this._slideshowPlayToolStripButton.Image = global::Extract.Redaction.Verification.Properties.Resources.SlideshowPlay;
            this._slideshowPlayToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._slideshowPlayToolStripButton.Name = "_slideshowPlayToolStripButton";
            this._slideshowPlayToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._slideshowPlayToolStripButton.Text = "Slideshow Play (F5)";
            this._slideshowPlayToolStripButton.ToolTipText = "Slideshow Play (F5)";
            this._slideshowPlayToolStripButton.Click += new System.EventHandler(this.HandleSlideshowPlayUIClick);
            // 
            // _slideshowPauseToolStripButton
            // 
            this._slideshowPauseToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._slideshowPauseToolStripButton.Enabled = false;
            this._slideshowPauseToolStripButton.Image = global::Extract.Redaction.Verification.Properties.Resources.SlideshowPause;
            this._slideshowPauseToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._slideshowPauseToolStripButton.Name = "_slideshowPauseToolStripButton";
            this._slideshowPauseToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._slideshowPauseToolStripButton.Text = "Slideshow Pause";
            this._slideshowPauseToolStripButton.Click += new System.EventHandler(this.HandleSlideshowPauseUIClick);
            // 
            // _slideshowStopToolStripButton
            // 
            this._slideshowStopToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._slideshowStopToolStripButton.Enabled = false;
            this._slideshowStopToolStripButton.Image = global::Extract.Redaction.Verification.Properties.Resources.SlideshowStop;
            this._slideshowStopToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._slideshowStopToolStripButton.Name = "_slideshowStopToolStripButton";
            this._slideshowStopToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._slideshowStopToolStripButton.Text = "Slideshow Stop";
            this._slideshowStopToolStripButton.ToolTipText = "Slideshow Pausetop";
            this._slideshowStopToolStripButton.Click += new System.EventHandler(this.HandleSlideshowStopUIClick);
            // 
            // dockContainer
            // 
            dockContainer.ContentSize = 527;
            dockContainer.Controls.Add(this._dataWindowDockableWindow);
            dockContainer.Controls.Add(this._magnifierDockableWindow);
            dockContainer.Dock = System.Windows.Forms.DockStyle.Left;
            dockContainer.LayoutSystem = new TD.SandDock.SplitLayoutSystem(new System.Drawing.SizeF(250F, 400F), System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 585.9155F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._dataWindowDockableWindow))}, this._dataWindowDockableWindow))),
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 214.0845F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._magnifierDockableWindow))}, this._magnifierDockableWindow)))});
            dockContainer.Location = new System.Drawing.Point(0, 0);
            dockContainer.Manager = this._sandDockManager;
            dockContainer.Name = "dockContainer";
            dockContainer.Size = new System.Drawing.Size(531, 926);
            dockContainer.TabIndex = 0;
            // 
            // _dataWindowDockableWindow
            // 
            this._dataWindowDockableWindow.AllowClose = false;
            this._dataWindowDockableWindow.Controls.Add(dataGridToolStripContainer);
            this._dataWindowDockableWindow.Guid = new System.Guid("9a0fd258-12fb-4a21-9076-d00f8ce8b1c6");
            this._dataWindowDockableWindow.Location = new System.Drawing.Point(0, 25);
            this._dataWindowDockableWindow.Name = "_dataWindowDockableWindow";
            this._dataWindowDockableWindow.PrimaryControl = this;
            this._dataWindowDockableWindow.Size = new System.Drawing.Size(527, 878);
            this._dataWindowDockableWindow.TabIndex = 0;
            this._dataWindowDockableWindow.Text = "Data window";
            // 
            // _sandDockManager
            // 
            this._sandDockManager.AllowKeyboardNavigation = false;
            this._sandDockManager.DockSystemContainer = this;
            this._sandDockManager.MaximumDockContainerSize = 2000;
            this._sandDockManager.MinimumDockContainerSize = 220;
            this._sandDockManager.OwnerForm = this;
            this._sandDockManager.Renderer = new TD.SandDock.Rendering.Office2003Renderer();
            // 
            // imageViewerToolStripContainer
            // 
            // 
            // imageViewerToolStripContainer.BottomToolStripPanel
            // 
            imageViewerToolStripContainer.BottomToolStripPanel.Controls.Add(this._imageViewerStatusStrip);
            // 
            // imageViewerToolStripContainer.ContentPanel
            // 
            imageViewerToolStripContainer.ContentPanel.Controls.Add(this._imageViewer);
            imageViewerToolStripContainer.ContentPanel.Size = new System.Drawing.Size(857, 787);
            imageViewerToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            imageViewerToolStripContainer.Location = new System.Drawing.Point(531, 0);
            imageViewerToolStripContainer.Name = "imageViewerToolStripContainer";
            imageViewerToolStripContainer.Size = new System.Drawing.Size(857, 926);
            imageViewerToolStripContainer.TabIndex = 1;
            imageViewerToolStripContainer.Text = "toolStripContainer1";
            // 
            // imageViewerToolStripContainer.TopToolStripPanel
            // 
            imageViewerToolStripContainer.TopToolStripPanel.Controls.Add(this._viewCommandsToolStrip);
            imageViewerToolStripContainer.TopToolStripPanel.Controls.Add(this._pageNavigationToolStrip);
            imageViewerToolStripContainer.TopToolStripPanel.Controls.Add(this._basicImageViewerToolStrip);
            // 
            // _imageViewerStatusStrip
            // 
            this._imageViewerStatusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._imageViewerStatusStrip.Location = new System.Drawing.Point(0, 0);
            this._imageViewerStatusStrip.Name = "_imageViewerStatusStrip";
            this._imageViewerStatusStrip.Size = new System.Drawing.Size(857, 22);
            this._imageViewerStatusStrip.TabIndex = 0;
            // 
            // _imageViewer
            // 
            this._imageViewer.ContextMenuStrip = this._imageViewerContextMenu;
            this._imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._imageViewer.Location = new System.Drawing.Point(0, 0);
            this._imageViewer.MinimumAngularHighlightHeight = 1;
            this._imageViewer.Name = "_imageViewer";
            this._imageViewer.Size = new System.Drawing.Size(857, 787);
            this._imageViewer.TabIndex = 0;
            this._imageViewer.TabStop = false;
            this._imageViewer.UseDefaultShortcuts = true;
            // 
            // _imageViewerContextMenu
            // 
            this._imageViewerContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._selectLayerObjectToolStripMenuItem1,
            this._zoomWindowToolStripMenuItem1,
            this._panToolStripMenuItem1,
            toolStripSeparator11,
            this._angularRedactionToolStripMenuItem1,
            this._rectangularRedactionToolStripMenuItem1,
            this._wordRedactionToolStripMenuItem1,
            this.toolStripSeparator12,
            this._blockFitSelectionToolStripMenuItem1});
            this._imageViewerContextMenu.Name = "_imageViewerContextMenu";
            this._imageViewerContextMenu.Size = new System.Drawing.Size(256, 170);
            // 
            // _selectLayerObjectToolStripMenuItem1
            // 
            this._selectLayerObjectToolStripMenuItem1.Enabled = false;
            this._selectLayerObjectToolStripMenuItem1.ImageViewer = null;
            this._selectLayerObjectToolStripMenuItem1.Name = "_selectLayerObjectToolStripMenuItem1";
            this._selectLayerObjectToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._selectLayerObjectToolStripMenuItem1.Size = new System.Drawing.Size(255, 22);
            this._selectLayerObjectToolStripMenuItem1.Text = "Select redactions and other objects";
            // 
            // _zoomWindowToolStripMenuItem1
            // 
            this._zoomWindowToolStripMenuItem1.Enabled = false;
            this._zoomWindowToolStripMenuItem1.ImageViewer = null;
            this._zoomWindowToolStripMenuItem1.Name = "_zoomWindowToolStripMenuItem1";
            this._zoomWindowToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._zoomWindowToolStripMenuItem1.Size = new System.Drawing.Size(255, 22);
            this._zoomWindowToolStripMenuItem1.Text = "&Zoom window";
            // 
            // _panToolStripMenuItem1
            // 
            this._panToolStripMenuItem1.Enabled = false;
            this._panToolStripMenuItem1.ImageViewer = null;
            this._panToolStripMenuItem1.Name = "_panToolStripMenuItem1";
            this._panToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._panToolStripMenuItem1.Size = new System.Drawing.Size(255, 22);
            this._panToolStripMenuItem1.Text = "P&an";
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new System.Drawing.Size(252, 6);
            // 
            // _angularRedactionToolStripMenuItem1
            // 
            this._angularRedactionToolStripMenuItem1.Enabled = false;
            this._angularRedactionToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("_angularRedactionToolStripMenuItem1.Image")));
            this._angularRedactionToolStripMenuItem1.ImageViewer = null;
            this._angularRedactionToolStripMenuItem1.Name = "_angularRedactionToolStripMenuItem1";
            this._angularRedactionToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._angularRedactionToolStripMenuItem1.Size = new System.Drawing.Size(255, 22);
            this._angularRedactionToolStripMenuItem1.Text = "A&ngular redaction";
            // 
            // _rectangularRedactionToolStripMenuItem1
            // 
            this._rectangularRedactionToolStripMenuItem1.Enabled = false;
            this._rectangularRedactionToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("_rectangularRedactionToolStripMenuItem1.Image")));
            this._rectangularRedactionToolStripMenuItem1.ImageViewer = null;
            this._rectangularRedactionToolStripMenuItem1.Name = "_rectangularRedactionToolStripMenuItem1";
            this._rectangularRedactionToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._rectangularRedactionToolStripMenuItem1.Size = new System.Drawing.Size(255, 22);
            this._rectangularRedactionToolStripMenuItem1.Text = "&Rectangular redaction";
            // 
            // _wordRedactionToolStripMenuItem1
            // 
            this._wordRedactionToolStripMenuItem1.Enabled = false;
            this._wordRedactionToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("_wordRedactionToolStripMenuItem1.Image")));
            this._wordRedactionToolStripMenuItem1.ImageViewer = null;
            this._wordRedactionToolStripMenuItem1.Name = "_wordRedactionToolStripMenuItem1";
            this._wordRedactionToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._wordRedactionToolStripMenuItem1.Size = new System.Drawing.Size(255, 22);
            this._wordRedactionToolStripMenuItem1.Text = "&Word redaction";
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(252, 6);
            // 
            // _blockFitSelectionToolStripMenuItem1
            // 
            this._blockFitSelectionToolStripMenuItem1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._blockFitSelectionToolStripMenuItem1.Enabled = false;
            this._blockFitSelectionToolStripMenuItem1.ImageViewer = null;
            this._blockFitSelectionToolStripMenuItem1.Name = "_blockFitSelectionToolStripMenuItem1";
            this._blockFitSelectionToolStripMenuItem1.ShortcutKeyDisplayString = "";
            this._blockFitSelectionToolStripMenuItem1.Size = new System.Drawing.Size(255, 22);
            this._blockFitSelectionToolStripMenuItem1.Text = "&Block fit selection";
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
            toolStripSeparator5,
            this._fitToPageToolStripButton,
            this._fitToWidthToolStripButton,
            toolStripSeparator6,
            this._previousTileToolStripButton,
            this._nextTileToolStripButton,
            toolStripSeparator9,
            this._rotateCounterclockwiseToolStripButton,
            this._rotateClockwiseToolStripButton,
            toolStripSeparator10,
            this._thumbnailsToolStripButton,
            this._magnifierToolStripButton});
            this._viewCommandsToolStrip.Location = new System.Drawing.Point(3, 0);
            this._viewCommandsToolStrip.Name = "_viewCommandsToolStrip";
            this._viewCommandsToolStrip.Size = new System.Drawing.Size(499, 39);
            this._viewCommandsToolStrip.TabIndex = 2;
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
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(6, 39);
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
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(6, 39);
            // 
            // _previousTileToolStripButton
            // 
            this._previousTileToolStripButton.BaseToolTipText = "Previous tile";
            this._previousTileToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._previousTileToolStripButton.Enabled = false;
            this._previousTileToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._previousTileToolStripButton.ImageViewer = null;
            this._previousTileToolStripButton.Name = "_previousTileToolStripButton";
            this._previousTileToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._previousTileToolStripButton.Text = "previousTileToolStripButton1";
            // 
            // _nextTileToolStripButton
            // 
            this._nextTileToolStripButton.BaseToolTipText = "Next tile";
            this._nextTileToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._nextTileToolStripButton.Enabled = false;
            this._nextTileToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._nextTileToolStripButton.ImageViewer = null;
            this._nextTileToolStripButton.Name = "_nextTileToolStripButton";
            this._nextTileToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._nextTileToolStripButton.Text = "nextTileToolStripButton1";
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new System.Drawing.Size(6, 39);
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
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new System.Drawing.Size(6, 39);
            // 
            // _thumbnailsToolStripButton
            // 
            this._thumbnailsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._thumbnailsToolStripButton.DockableWindow = null;
            this._thumbnailsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._thumbnailsToolStripButton.Name = "_thumbnailsToolStripButton";
            this._thumbnailsToolStripButton.Size = new System.Drawing.Size(36, 36);
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
            this._pageNavigationToolStrip.Location = new System.Drawing.Point(3, 39);
            this._pageNavigationToolStrip.Name = "_pageNavigationToolStrip";
            this._pageNavigationToolStrip.Size = new System.Drawing.Size(233, 39);
            this._pageNavigationToolStrip.TabIndex = 1;
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
            // _basicImageViewerToolStrip
            // 
            this._basicImageViewerToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._basicImageViewerToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._basicImageViewerToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._printImageToolStripButton,
            this._findOrRedactToolStripButton,
            toolStripSeparator4,
            this._zoomWindowToolStripButton,
            this._panToolStripButton,
            this._selectLayerObjectToolStripButton,
            this._angularRedactionToolStripButton,
            this._rectangularRedactionToolStripButton,
            this._wordRedactionToolStripButton});
            this._basicImageViewerToolStrip.Location = new System.Drawing.Point(3, 78);
            this._basicImageViewerToolStrip.Name = "_basicImageViewerToolStrip";
            this._basicImageViewerToolStrip.Size = new System.Drawing.Size(306, 39);
            this._basicImageViewerToolStrip.TabIndex = 0;
            // 
            // _printImageToolStripButton
            // 
            this._printImageToolStripButton.BaseToolTipText = "Print image";
            this._printImageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._printImageToolStripButton.Enabled = false;
            this._printImageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._printImageToolStripButton.ImageViewer = null;
            this._printImageToolStripButton.Name = "_printImageToolStripButton";
            this._printImageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._printImageToolStripButton.Text = "Print image";
            // 
            // _findOrRedactToolStripButton
            // 
            this._findOrRedactToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._findOrRedactToolStripButton.Enabled = false;
            this._findOrRedactToolStripButton.Image = global::Extract.Redaction.Verification.Properties.Resources.FindWords;
            this._findOrRedactToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._findOrRedactToolStripButton.Name = "_findOrRedactToolStripButton";
            this._findOrRedactToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._findOrRedactToolStripButton.Text = "Find or redact";
            this._findOrRedactToolStripButton.ToolTipText = "Find or redact (Ctrl+F)";
            this._findOrRedactToolStripButton.Click += new System.EventHandler(this.HandleFindOrRedactToolStripButtonClick);
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 39);
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
            // _selectLayerObjectToolStripButton
            // 
            this._selectLayerObjectToolStripButton.BaseToolTipText = "Select redactions and other objects";
            this._selectLayerObjectToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._selectLayerObjectToolStripButton.Enabled = false;
            this._selectLayerObjectToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._selectLayerObjectToolStripButton.ImageViewer = null;
            this._selectLayerObjectToolStripButton.Name = "_selectLayerObjectToolStripButton";
            this._selectLayerObjectToolStripButton.Size = new System.Drawing.Size(36, 36);
            // 
            // _angularRedactionToolStripButton
            // 
            this._angularRedactionToolStripButton.BaseToolTipText = "Create angular redaction";
            this._angularRedactionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._angularRedactionToolStripButton.Enabled = false;
            this._angularRedactionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._angularRedactionToolStripButton.ImageViewer = null;
            this._angularRedactionToolStripButton.Name = "_angularRedactionToolStripButton";
            this._angularRedactionToolStripButton.Size = new System.Drawing.Size(36, 36);
            // 
            // _rectangularRedactionToolStripButton
            // 
            this._rectangularRedactionToolStripButton.BaseToolTipText = "Create rectangular redaction";
            this._rectangularRedactionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._rectangularRedactionToolStripButton.Enabled = false;
            this._rectangularRedactionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._rectangularRedactionToolStripButton.ImageViewer = null;
            this._rectangularRedactionToolStripButton.Name = "_rectangularRedactionToolStripButton";
            this._rectangularRedactionToolStripButton.Size = new System.Drawing.Size(36, 36);
            // 
            // _wordRedactionToolStripButton
            // 
            this._wordRedactionToolStripButton.BaseToolTipText = "Create word redaction";
            this._wordRedactionToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._wordRedactionToolStripButton.Enabled = false;
            this._wordRedactionToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._wordRedactionToolStripButton.ImageViewer = null;
            this._wordRedactionToolStripButton.Name = "_wordRedactionToolStripButton";
            this._wordRedactionToolStripButton.Size = new System.Drawing.Size(36, 36);
            // 
            // dockContainer1
            // 
            dockContainer1.ContentSize = 200;
            dockContainer1.Controls.Add(this._thumbnailDockableWindow);
            dockContainer1.Dock = System.Windows.Forms.DockStyle.Right;
            dockContainer1.LayoutSystem = new TD.SandDock.SplitLayoutSystem(new System.Drawing.SizeF(250F, 400F), System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 400F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._thumbnailDockableWindow))}, this._thumbnailDockableWindow)))});
            dockContainer1.Location = new System.Drawing.Point(1388, 0);
            dockContainer1.Manager = this._sandDockManager;
            dockContainer1.Name = "dockContainer1";
            dockContainer1.Size = new System.Drawing.Size(204, 926);
            dockContainer1.TabIndex = 2;
            // 
            // _thumbnailDockableWindow
            // 
            this._thumbnailDockableWindow.Controls.Add(this._thumbnailViewer);
            this._thumbnailDockableWindow.Guid = new System.Guid("ae627741-717d-48f0-8e85-071b39098d21");
            this._thumbnailDockableWindow.Location = new System.Drawing.Point(4, 25);
            this._thumbnailDockableWindow.Name = "_thumbnailDockableWindow";
            this._thumbnailDockableWindow.PrimaryControl = this;
            this._thumbnailDockableWindow.Size = new System.Drawing.Size(200, 878);
            this._thumbnailDockableWindow.TabIndex = 0;
            this._thumbnailDockableWindow.Text = "Page thumbnails";
            this._thumbnailDockableWindow.DockSituationChanged += new System.EventHandler(this.HandleThumbnailDockableWindowDockSituationChanged);
            // 
            // _thumbnailViewer
            // 
            this._thumbnailViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._thumbnailViewer.ImageViewer = null;
            this._thumbnailViewer.Location = new System.Drawing.Point(0, 0);
            this._thumbnailViewer.Name = "_thumbnailViewer";
            this._thumbnailViewer.Size = new System.Drawing.Size(200, 878);
            this._thumbnailViewer.TabIndex = 0;
            // 
            // _magnifierDockableWindow
            // 
            this._magnifierDockableWindow.Controls.Add(this._magnifierControl);
            this._magnifierDockableWindow.Guid = new System.Guid("ae627741-717d-48f0-8e85-071b39098d21");
            this._magnifierDockableWindow.Location = new System.Drawing.Point(0, 601);
            this._magnifierDockableWindow.Name = "_magnifierDockableWindow";
            this._magnifierDockableWindow.PrimaryControl = this;
            this._magnifierDockableWindow.Size = new System.Drawing.Size(527, 161);
            this._magnifierDockableWindow.TabIndex = 0;
            this._magnifierDockableWindow.Text = "Magnifier";
            // 
            // _magnifierControl
            // 
            this._magnifierControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._magnifierControl.Location = new System.Drawing.Point(0, 0);
            this._magnifierControl.Name = "_magnifierControl";
            this._magnifierControl.Size = new System.Drawing.Size(527, 161);
            this._magnifierControl.TabIndex = 0;
            // 
            // _magnifierToolStripButton
            // 
            this._magnifierToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._magnifierToolStripButton.DockableWindow = null;
            this._magnifierToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._magnifierToolStripButton.Name = "_magnifierToolStripButton";
            this._magnifierToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._magnifierToolStripButton.Text = "Show/Hide magnifier";
            // 
            // VerificationTaskForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1592, 926);
            this.Controls.Add(imageViewerToolStripContainer);
            this.Controls.Add(dockContainer1);
            this.Controls.Add(dockContainer);
            this.Icon = global::Extract.Redaction.Verification.Properties.Resources.IDShieldIcon;
            this.MainMenuStrip = this._menuStrip;
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "VerificationTaskForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ID Shield Verification (Waiting for file)";
            dataGridToolStripContainer.ContentPanel.ResumeLayout(false);
            dataGridToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            dataGridToolStripContainer.TopToolStripPanel.PerformLayout();
            dataGridToolStripContainer.ResumeLayout(false);
            dataGridToolStripContainer.PerformLayout();
            this._dataWindowSplitContainer.Panel1.ResumeLayout(false);
            this._dataWindowSplitContainer.Panel1.PerformLayout();
            this._dataWindowSplitContainer.Panel2.ResumeLayout(false);
            this._dataWindowSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dataWindowSplitContainer)).EndInit();
            this._dataWindowSplitContainer.ResumeLayout(false);
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this._basicDataGridToolStrip.ResumeLayout(false);
            this._basicDataGridToolStrip.PerformLayout();
            this._exemptionsToolStrip.ResumeLayout(false);
            this._exemptionsToolStrip.PerformLayout();
            this._slideShowToolStrip.ResumeLayout(false);
            this._slideShowToolStrip.PerformLayout();
            dockContainer.ResumeLayout(false);
            this._dataWindowDockableWindow.ResumeLayout(false);
            imageViewerToolStripContainer.BottomToolStripPanel.ResumeLayout(false);
            imageViewerToolStripContainer.BottomToolStripPanel.PerformLayout();
            imageViewerToolStripContainer.ContentPanel.ResumeLayout(false);
            imageViewerToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            imageViewerToolStripContainer.TopToolStripPanel.PerformLayout();
            imageViewerToolStripContainer.ResumeLayout(false);
            imageViewerToolStripContainer.PerformLayout();
            this._imageViewerContextMenu.ResumeLayout(false);
            this._viewCommandsToolStrip.ResumeLayout(false);
            this._viewCommandsToolStrip.PerformLayout();
            this._pageNavigationToolStrip.ResumeLayout(false);
            this._pageNavigationToolStrip.PerformLayout();
            this._basicImageViewerToolStrip.ResumeLayout(false);
            this._basicImageViewerToolStrip.PerformLayout();
            dockContainer1.ResumeLayout(false);
            this._thumbnailDockableWindow.ResumeLayout(false);
            this._magnifierDockableWindow.ResumeLayout(false);
            this.ResumeLayout(false);

        }
     
        #endregion

        private TD.SandDock.SandDockManager _sandDockManager;
        private TD.SandDock.DockableWindow _dataWindowDockableWindow;
        private Extract.Imaging.Forms.ImageViewer _imageViewer;
        private System.Windows.Forms.ToolStrip _basicImageViewerToolStrip;
        private Extract.Imaging.Forms.PrintImageToolStripButton _printImageToolStripButton;
        private Extract.Imaging.Forms.ZoomWindowToolStripButton _zoomWindowToolStripButton;
        private Extract.Imaging.Forms.PanToolStripButton _panToolStripButton;
        private Extract.Imaging.Forms.SelectLayerObjectToolStripButton _selectLayerObjectToolStripButton;
        private Extract.Imaging.Forms.AngularRedactionToolStripButton _angularRedactionToolStripButton;
        private Extract.Imaging.Forms.RectangularRedactionToolStripButton _rectangularRedactionToolStripButton;
        private Extract.Imaging.Forms.WordRedactionToolStripButton _wordRedactionToolStripButton;
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
        private Extract.Imaging.Forms.FitToPageToolStripButton _fitToPageToolStripButton;
        private Extract.Imaging.Forms.FitToWidthToolStripButton _fitToWidthToolStripButton;
        private Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton _rotateCounterclockwiseToolStripButton;
        private Extract.Imaging.Forms.RotateClockwiseToolStripButton _rotateClockwiseToolStripButton;
        private System.Windows.Forms.ToolStrip _basicDataGridToolStrip;
        private System.Windows.Forms.ToolStripButton _saveAndCommitToolStripButton;
        private System.Windows.Forms.ToolStripButton _previousDocumentToolStripButton;
        private System.Windows.Forms.ToolStripButton _nextDocumentToolStripButton;
        private System.Windows.Forms.ToolStripButton _previousRedactionToolStripButton;
        private System.Windows.Forms.ToolStripButton _nextRedactionToolStripButton;
        private System.Windows.Forms.ToolStrip _exemptionsToolStrip;
        private System.Windows.Forms.ToolStripButton _applyExemptionToolStripButton;
        private System.Windows.Forms.ToolStripButton _lastExemptionToolStripButton;
        private RedactionGridView _redactionGridView;
        private System.Windows.Forms.TextBox _currentDocumentTextBox;
        private System.Windows.Forms.TextBox _documentTypeTextBox;
        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.ToolStripMenuItem _fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _helpToolStripMenuItem;
        private System.Windows.Forms.TextBox _commentsTextBox;
        private System.Windows.Forms.ToolStripMenuItem _saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _saveAndCommitToolStripMenuItem;
        private Extract.Imaging.Forms.PrintImageToolStripMenuItem _printImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _skipProcessingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _stopProcessingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _discardChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _idShieldHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _aboutIDShieldToolStripMenuItem;
        private Extract.Imaging.Forms.PageSummaryView _pageSummaryView;
        private TD.SandDock.DockableWindow _thumbnailDockableWindow;
        private Extract.Imaging.Forms.ThumbnailViewer _thumbnailViewer;
        private Extract.Imaging.Forms.ImageViewerStatusStrip _imageViewerStatusStrip;
        private System.Windows.Forms.ToolStripMenuItem _toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _optionsToolStripMenuItem;
        private Extract.Imaging.Forms.ThumbnailViewerToolStripButton _thumbnailsToolStripButton;
        private Extract.FileActionManager.Forms.TagFileToolStripButton _tagFileToolStripButton;
        private Extract.Imaging.Forms.PreviousTileToolStripButton _previousTileToolStripButton;
        private Extract.Imaging.Forms.NextTileToolStripButton _nextTileToolStripButton;
        private System.Windows.Forms.ToolStripSeparator _tagFileToolStripSeparator;
        private System.Windows.Forms.SplitContainer _dataWindowSplitContainer;
        private System.Windows.Forms.ToolStripMenuItem _findOrRedactToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton _findOrRedactToolStripButton;
        private Extract.Imaging.Forms.PrintViewToolStripMenuItem _printViewToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip _imageViewerContextMenu;
        private Extract.Imaging.Forms.SelectLayerObjectToolStripMenuItem _selectLayerObjectToolStripMenuItem1;
        private Extract.Imaging.Forms.ZoomWindowToolStripMenuItem _zoomWindowToolStripMenuItem1;
        private Extract.Imaging.Forms.PanToolStripMenuItem _panToolStripMenuItem1;
        private Extract.Imaging.Forms.AngularRedactionToolStripMenuItem _angularRedactionToolStripMenuItem1;
        private Extract.Imaging.Forms.RectangularRedactionToolStripMenuItem _rectangularRedactionToolStripMenuItem1;
        private Extract.Imaging.Forms.WordRedactionToolStripMenuItem _wordRedactionToolStripMenuItem1;
        private Extract.Imaging.Forms.BlockFitSelectionToolStripMenuItem _blockFitSelectionToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem _viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _zoomToolStripMenuItem;
        private Imaging.Forms.FitToPageToolStripMenuItem _fitToPageToolStripMenuItem;
        private Imaging.Forms.FitToWidthToolStripMenuItem _fitToWidthToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator3;
        private Imaging.Forms.ZoomInToolStripMenuItem _zoomInToolStripMenuItem;
        private Imaging.Forms.ZoomOutToolStripMenuItem _zoomOutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator4;
        private Imaging.Forms.ZoomPreviousToolStripMenuItem _zoomPreviousToolStripMenuItem;
        private Imaging.Forms.ZoomNextToolStripMenuItem _zoomNextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _rotateToolStripMenuItem;
        private Imaging.Forms.RotateCounterclockwiseToolStripMenuItem _rotateCounterclockwiseToolStripMenuItem;
        private Imaging.Forms.RotateClockwiseToolStripMenuItem _rotateClockwiseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _gotoPageToolStripMenuItem;
        private Imaging.Forms.FirstPageToolStripMenuItem _firstPageToolStripMenuItem;
        private Imaging.Forms.PreviousPageToolStripMenuItem _previousPageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator5;
        private Imaging.Forms.PageNavigationToolStripMenuItem _pageNavigationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator6;
        private Imaging.Forms.NextPageToolStripMenuItem _nextPageToolStripMenuItem;
        private Imaging.Forms.LastPageToolStripMenuItem _lastPageToolStripMenuItem;
        private Imaging.Forms.ZoomWindowToolStripMenuItem _zoomWindowToolStripMenuItem;
        private Imaging.Forms.PanToolStripMenuItem _panToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolStripSeparator8;
        private Imaging.Forms.SelectLayerObjectToolStripMenuItem _selectLayerObjectToolStripMenuItem;
        private Imaging.Forms.AngularRedactionToolStripMenuItem _angularRedactionToolStripMenuItem;
        private Imaging.Forms.RectangularRedactionToolStripMenuItem _rectangularRedactionToolStripMenuItem;
        private Imaging.Forms.WordRedactionToolStripMenuItem _wordRedactionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripMenuItem _fullScreenToolStripMenuItem;
        private System.Windows.Forms.ToolStrip _slideShowToolStrip;
        private System.Windows.Forms.ToolStripButton _slideshowConfigToolStripButton;
        private System.Windows.Forms.ToolStripButton _slideshowPlayToolStripButton;
        private System.Windows.Forms.ToolStripButton _slideshowPauseToolStripButton;
        private System.Windows.Forms.ToolStripButton _slideshowStopToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripSeparator _slideshowToolStripMenuItemSeparator;
        private System.Windows.Forms.ToolStripMenuItem _slideshowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _slideshowConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _slideshowPlayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _slideshowPauseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _slideshowStopToolStripMenuItem;
        private DockableWindow _magnifierDockableWindow;
        private Imaging.Forms.MagnifierControl _magnifierControl;
        private Imaging.Forms.MagnifierWindowToolStripButton _magnifierToolStripButton;
    }
}
