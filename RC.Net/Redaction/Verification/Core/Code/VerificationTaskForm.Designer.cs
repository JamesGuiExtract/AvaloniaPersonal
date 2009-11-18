using System;

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
                if (_unsavedMemento != null)
                {
                    try
                    {
                        // This method should never throw exceptions, but wrap in try anyway.
                        ResetUnsavedMemento();
                    }
                    catch (Exception ex)
                    {
                        ExtractException.Log("ELI27118", ex);
                    }
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
            System.Windows.Forms.ToolStripContainer dataGridToolStripContainer;
            System.Windows.Forms.SplitContainer splitContainer1;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VerificationTaskForm));
            System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
            TD.SandDock.DockContainer dockContainer;
            System.Windows.Forms.ToolStripContainer imageViewerToolStripContainer;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
            TD.SandDock.DockContainer dockContainer1;
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
            this._skipProcessingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._stopProcessingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._discardChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._idShieldHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._aboutIDShieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._basicDataGridToolStrip = new System.Windows.Forms.ToolStrip();
            this._saveToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._previousDocumentToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._nextDocumentToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._previousRedactionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._nextRedactionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._exemptionsToolStrip = new System.Windows.Forms.ToolStrip();
            this._applyExemptionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._lastExemptionToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._dataWindowDockableWindow = new TD.SandDock.DockableWindow();
            this._sandDockManager = new TD.SandDock.SandDockManager();
            this._imageViewerStatusStrip = new Extract.Imaging.Forms.ImageViewerStatusStrip();
            this._imageViewer = new Extract.Imaging.Forms.ImageViewer();
            this._viewCommandsToolStrip = new System.Windows.Forms.ToolStrip();
            this._zoomInToolStripButton = new Extract.Imaging.Forms.ZoomInToolStripButton();
            this._zoomOutToolStripButton = new Extract.Imaging.Forms.ZoomOutToolStripButton();
            this._zoomPreviousToolStripButton = new Extract.Imaging.Forms.ZoomPreviousToolStripButton();
            this._zoomNextToolStripButton = new Extract.Imaging.Forms.ZoomNextToolStripButton();
            this._fitToPageToolStripButton = new Extract.Imaging.Forms.FitToPageToolStripButton();
            this._fitToWidthToolStripButton = new Extract.Imaging.Forms.FitToWidthToolStripButton();
            this._rotateCounterclockwiseToolStripButton = new Extract.Imaging.Forms.RotateCounterclockwiseToolStripButton();
            this._rotateClockwiseToolStripButton = new Extract.Imaging.Forms.RotateClockwiseToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this._thumbnailsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._pageNavigationToolStrip = new System.Windows.Forms.ToolStrip();
            this._firstPageToolStripButton = new Extract.Imaging.Forms.FirstPageToolStripButton();
            this._previousPageToolStripButton = new Extract.Imaging.Forms.PreviousPageToolStripButton();
            this._pageNavigationToolStripTextBox = new Extract.Imaging.Forms.PageNavigationToolStripTextBox();
            this._nextPageToolStripButton = new Extract.Imaging.Forms.NextPageToolStripButton();
            this._lastPageToolStripButton = new Extract.Imaging.Forms.LastPageToolStripButton();
            this._basicImageViewerToolStrip = new System.Windows.Forms.ToolStrip();
            this._printImageToolStripButton = new Extract.Imaging.Forms.PrintImageToolStripButton();
            this._zoomWindowToolStripButton = new Extract.Imaging.Forms.ZoomWindowToolStripButton();
            this._panToolStripButton = new Extract.Imaging.Forms.PanToolStripButton();
            this._selectLayerObjectToolStripButton = new Extract.Imaging.Forms.SelectLayerObjectToolStripButton();
            this._angularRedactionToolStripButton = new Extract.Imaging.Forms.AngularRedactionToolStripButton();
            this._rectangularRedactionToolStripButton = new Extract.Imaging.Forms.RectangularRedactionToolStripButton();
            this._thumbnailDockableWindow = new TD.SandDock.DockableWindow();
            this._thumbnailViewer = new Extract.Imaging.Forms.ThumbnailViewer();
            dataGridToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            dockContainer = new TD.SandDock.DockContainer();
            imageViewerToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            dockContainer1 = new TD.SandDock.DockContainer();
            dataGridToolStripContainer.ContentPanel.SuspendLayout();
            dataGridToolStripContainer.TopToolStripPanel.SuspendLayout();
            dataGridToolStripContainer.SuspendLayout();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            this._menuStrip.SuspendLayout();
            this._basicDataGridToolStrip.SuspendLayout();
            this._exemptionsToolStrip.SuspendLayout();
            dockContainer.SuspendLayout();
            this._dataWindowDockableWindow.SuspendLayout();
            imageViewerToolStripContainer.BottomToolStripPanel.SuspendLayout();
            imageViewerToolStripContainer.ContentPanel.SuspendLayout();
            imageViewerToolStripContainer.TopToolStripPanel.SuspendLayout();
            imageViewerToolStripContainer.SuspendLayout();
            this._viewCommandsToolStrip.SuspendLayout();
            this._pageNavigationToolStrip.SuspendLayout();
            this._basicImageViewerToolStrip.SuspendLayout();
            dockContainer1.SuspendLayout();
            this._thumbnailDockableWindow.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridToolStripContainer
            // 
            // 
            // dataGridToolStripContainer.ContentPanel
            // 
            dataGridToolStripContainer.ContentPanel.Controls.Add(splitContainer1);
            dataGridToolStripContainer.ContentPanel.Size = new System.Drawing.Size(527, 821);
            dataGridToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridToolStripContainer.Location = new System.Drawing.Point(0, 0);
            dataGridToolStripContainer.Name = "dataGridToolStripContainer";
            dataGridToolStripContainer.Size = new System.Drawing.Size(527, 884);
            dataGridToolStripContainer.TabIndex = 0;
            dataGridToolStripContainer.Text = "toolStripContainer2";
            // 
            // dataGridToolStripContainer.TopToolStripPanel
            // 
            dataGridToolStripContainer.TopToolStripPanel.Controls.Add(this._menuStrip);
            dataGridToolStripContainer.TopToolStripPanel.Controls.Add(this._basicDataGridToolStrip);
            dataGridToolStripContainer.TopToolStripPanel.Controls.Add(this._exemptionsToolStrip);
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(this._commentsTextBox);
            splitContainer1.Panel1.Controls.Add(label3);
            splitContainer1.Panel1.Controls.Add(this._currentDocumentTextBox);
            splitContainer1.Panel1.Controls.Add(label2);
            splitContainer1.Panel1.Controls.Add(this._documentTypeTextBox);
            splitContainer1.Panel1.Controls.Add(label1);
            splitContainer1.Panel1.Controls.Add(this._redactionGridView);
            splitContainer1.Panel1MinSize = 225;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(label4);
            splitContainer1.Panel2.Controls.Add(this._pageSummaryView);
            splitContainer1.Panel2MinSize = 50;
            splitContainer1.Size = new System.Drawing.Size(527, 821);
            splitContainer1.SplitterDistance = 508;
            splitContainer1.TabIndex = 0;
            // 
            // _commentsTextBox
            // 
            this._commentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._commentsTextBox.Location = new System.Drawing.Point(7, 106);
            this._commentsTextBox.Multiline = true;
            this._commentsTextBox.Name = "_commentsTextBox";
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
            this._redactionGridView.Size = new System.Drawing.Size(513, 337);
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
            this._pageSummaryView.Size = new System.Drawing.Size(513, 280);
            this._pageSummaryView.TabIndex = 0;
            this._pageSummaryView.TabStop = false;
            // 
            // _menuStrip
            // 
            this._menuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileToolStripMenuItem,
            this._editToolStripMenuItem,
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
            this._discardChangesToolStripMenuItem});
            this._editToolStripMenuItem.Name = "_editToolStripMenuItem";
            this._editToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this._editToolStripMenuItem.Text = "Edit";
            // 
            // _discardChangesToolStripMenuItem
            // 
            this._discardChangesToolStripMenuItem.Name = "_discardChangesToolStripMenuItem";
            this._discardChangesToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this._discardChangesToolStripMenuItem.Text = "Discard changes";
            this._discardChangesToolStripMenuItem.Click += new System.EventHandler(this.HandleDiscardChangesToolStripMenuItemClick);
            // 
            // _toolsToolStripMenuItem
            // 
            this._toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._optionsToolStripMenuItem});
            this._toolsToolStripMenuItem.Name = "_toolsToolStripMenuItem";
            this._toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this._toolsToolStripMenuItem.Text = "Tools";
            // 
            // _optionsToolStripMenuItem
            // 
            this._optionsToolStripMenuItem.Name = "_optionsToolStripMenuItem";
            this._optionsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
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
            this._saveToolStripButton,
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
            this._lastExemptionToolStripButton});
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
            // dockContainer
            // 
            dockContainer.ContentSize = 527;
            dockContainer.Controls.Add(this._dataWindowDockableWindow);
            dockContainer.Dock = System.Windows.Forms.DockStyle.Left;
            dockContainer.LayoutSystem = new TD.SandDock.SplitLayoutSystem(new System.Drawing.SizeF(250F, 400F), System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] {
            ((TD.SandDock.LayoutSystemBase)(new TD.SandDock.ControlLayoutSystem(new System.Drawing.SizeF(250F, 400F), new TD.SandDock.DockControl[] {
                        ((TD.SandDock.DockControl)(this._dataWindowDockableWindow))}, this._dataWindowDockableWindow)))});
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
            this._dataWindowDockableWindow.Location = new System.Drawing.Point(0, 18);
            this._dataWindowDockableWindow.Name = "_dataWindowDockableWindow";
            this._dataWindowDockableWindow.Size = new System.Drawing.Size(527, 884);
            this._dataWindowDockableWindow.TabIndex = 0;
            this._dataWindowDockableWindow.Text = "Data window";
            // 
            // _sandDockManager
            // 
            this._sandDockManager.DockSystemContainer = this;
            this._sandDockManager.MaximumDockContainerSize = 2000;
            this._sandDockManager.MinimumDockContainerSize = 220;
            this._sandDockManager.OwnerForm = this;
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
            imageViewerToolStripContainer.ContentPanel.Size = new System.Drawing.Size(857, 865);
            imageViewerToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            imageViewerToolStripContainer.Location = new System.Drawing.Point(531, 0);
            imageViewerToolStripContainer.Name = "imageViewerToolStripContainer";
            imageViewerToolStripContainer.Size = new System.Drawing.Size(857, 926);
            imageViewerToolStripContainer.TabIndex = 1;
            imageViewerToolStripContainer.Text = "toolStripContainer1";
            // 
            // imageViewerToolStripContainer.TopToolStripPanel
            // 
            imageViewerToolStripContainer.TopToolStripPanel.Controls.Add(this._basicImageViewerToolStrip);
            imageViewerToolStripContainer.TopToolStripPanel.Controls.Add(this._pageNavigationToolStrip);
            imageViewerToolStripContainer.TopToolStripPanel.Controls.Add(this._viewCommandsToolStrip);
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
            this._imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._imageViewer.Location = new System.Drawing.Point(0, 0);
            this._imageViewer.Name = "_imageViewer";
            this._imageViewer.Size = new System.Drawing.Size(857, 787);
            this._imageViewer.TabIndex = 0;
            this._imageViewer.TabStop = false;
            this._imageViewer.UseDefaultShortcuts = true;
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
            this._rotateCounterclockwiseToolStripButton,
            this._rotateClockwiseToolStripButton,
            this.toolStripSeparator9,
            this._thumbnailsToolStripButton});
            this._viewCommandsToolStrip.Location = new System.Drawing.Point(470, 0);
            this._viewCommandsToolStrip.Name = "_viewCommandsToolStrip";
            this._viewCommandsToolStrip.Size = new System.Drawing.Size(385, 39);
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
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 39);
            // 
            // _thumbnailsToolStripButton
            // 
            this._thumbnailsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._thumbnailsToolStripButton.Image = global::Extract.Redaction.Verification.Properties.Resources.ThumbnailViewer;
            this._thumbnailsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._thumbnailsToolStripButton.Name = "_thumbnailsToolStripButton";
            this._thumbnailsToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._thumbnailsToolStripButton.Text = "Show/Hide thumbnails";
            this._thumbnailsToolStripButton.Click += new System.EventHandler(this.HandleThumbnailsToolStripButtonClick);
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
            toolStripSeparator4,
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
            this._printImageToolStripButton.BaseToolTipText = "Print image";
            this._printImageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._printImageToolStripButton.Enabled = false;
            this._printImageToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._printImageToolStripButton.ImageViewer = null;
            this._printImageToolStripButton.Name = "_printImageToolStripButton";
            this._printImageToolStripButton.Size = new System.Drawing.Size(36, 36);
            this._printImageToolStripButton.Text = "Print image";
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
            this._thumbnailDockableWindow.Location = new System.Drawing.Point(4, 18);
            this._thumbnailDockableWindow.Name = "_thumbnailDockableWindow";
            this._thumbnailDockableWindow.Size = new System.Drawing.Size(200, 884);
            this._thumbnailDockableWindow.TabIndex = 0;
            this._thumbnailDockableWindow.Text = "Page thumbnails";
            // 
            // _thumbnailViewer
            // 
            this._thumbnailViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._thumbnailViewer.ImageViewer = null;
            this._thumbnailViewer.Location = new System.Drawing.Point(0, 0);
            this._thumbnailViewer.Name = "_thumbnailViewer";
            this._thumbnailViewer.Size = new System.Drawing.Size(200, 884);
            this._thumbnailViewer.TabIndex = 0;
            // 
            // VerificationTaskForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1592, 926);
            this.Controls.Add(imageViewerToolStripContainer);
            this.Controls.Add(dockContainer1);
            this.Controls.Add(dockContainer);
            this.MainMenuStrip = this._menuStrip;
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "VerificationTaskForm";
            this.ShowIcon = false;
            this.Text = "ID Shield Verification (Waiting for file)";
            dataGridToolStripContainer.ContentPanel.ResumeLayout(false);
            dataGridToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            dataGridToolStripContainer.TopToolStripPanel.PerformLayout();
            dataGridToolStripContainer.ResumeLayout(false);
            dataGridToolStripContainer.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            splitContainer1.ResumeLayout(false);
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this._basicDataGridToolStrip.ResumeLayout(false);
            this._basicDataGridToolStrip.PerformLayout();
            this._exemptionsToolStrip.ResumeLayout(false);
            this._exemptionsToolStrip.PerformLayout();
            dockContainer.ResumeLayout(false);
            this._dataWindowDockableWindow.ResumeLayout(false);
            imageViewerToolStripContainer.BottomToolStripPanel.ResumeLayout(false);
            imageViewerToolStripContainer.BottomToolStripPanel.PerformLayout();
            imageViewerToolStripContainer.ContentPanel.ResumeLayout(false);
            imageViewerToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            imageViewerToolStripContainer.TopToolStripPanel.PerformLayout();
            imageViewerToolStripContainer.ResumeLayout(false);
            imageViewerToolStripContainer.PerformLayout();
            this._viewCommandsToolStrip.ResumeLayout(false);
            this._viewCommandsToolStrip.PerformLayout();
            this._pageNavigationToolStrip.ResumeLayout(false);
            this._pageNavigationToolStrip.PerformLayout();
            this._basicImageViewerToolStrip.ResumeLayout(false);
            this._basicImageViewerToolStrip.PerformLayout();
            dockContainer1.ResumeLayout(false);
            this._thumbnailDockableWindow.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripButton _saveToolStripButton;
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripButton _thumbnailsToolStripButton;
    }
}