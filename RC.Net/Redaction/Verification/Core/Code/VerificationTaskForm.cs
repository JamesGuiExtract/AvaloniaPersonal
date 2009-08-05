using Extract.Imaging.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using TD.SandDock;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a dialog that allows the user to verify redactions.
    /// </summary>
    [CLSCompliant(false)]
    public partial class VerificationTaskForm : Form, IVerificationForm
    {
        #region VerificationTaskForm Constants

        /// <summary>
        /// The license string for the SandDock manager
        /// </summary>
        static readonly string _SANDDOCK_LICENSE_STRING = @"1970|siE7SnF/jzINQg1AOTIaCXLlouA=";

        #endregion VerificationTaskForm Constants

        #region VerificationTaskForm Fields

        /// <summary>
        /// The settings for verification.
        /// </summary>
        // TODO: Don't forget to remove suppress message.
        // Temporarily suppress this warning. Verification codes will be used in the future.
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        readonly VerificationSettings _settings;

        #endregion VerificationTaskForm Fields

        #region VerificationTaskForm Events

        /// <summary>
        /// Occurs when a file has completed verification.
        /// </summary>
        public event EventHandler<FileCompleteEventArgs> FileComplete;

        #endregion VerificationTaskForm Events

        #region VerificationTaskForm Constructors

        /// <summary>
        /// Initializes a new <see cref="VerificationTaskForm"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public VerificationTaskForm(VerificationSettings settings)
        {
            // License SandDock before creating the form
            SandDockManager.ActivateProduct(_SANDDOCK_LICENSE_STRING);

            _settings = settings;

            InitializeComponent();

            _imageViewer.LayerObjects.LayerObjectAdded += HandleImageViewerLayerObjectAdded;
            _imageViewer.LayerObjects.LayerObjectDeleted += HandleImageViewerLayerObjectDeleted;
        }

        #endregion VerificationTaskForm Constructors

        #region VerificationTaskForm Methods

        /// <summary>
        /// Gets the voa file for the currently open image.
        /// </summary>
        /// <returns>The voa file for the currently open image.</returns>
        string GetVoaFileName()
        {
            // TODO: Use the path tags settings to determine the voa file
            // The fps file directory is not yet bubbling down to the VerificationTaskForm.
            return _imageViewer.ImageFile + ".voa";
        }
        
        /// <summary>
        /// Saves and optionally commits the currently open image file.
        /// </summary>
        /// <param name="commit"><see langword="true"/> if the image file should be commited; 
        /// <see langword="false"/> if the image file should not be committed.</param>
        void Save(bool commit)
        {
            _redactionGridView.SaveTo(GetVoaFileName());

            if (commit)
            {
                OnFileComplete(new FileCompleteEventArgs(EFileProcessingResult.kProcessingSuccessful));
            }
        }

        #endregion VerificationTaskForm Methods

        #region VerificationTaskForm Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> 
        /// event.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                _imageViewer.EstablishConnections(this);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26715", ex);
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">The window message to process.</param>
        /// <param name="keyData">The key to process.</param>
        /// <returns><see langword="true"/> if the character was processed by the control; 
        /// otherwise, <see langword="false"/>.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Allow the image viewer to handle keyboard input for shortcuts.
            if (_imageViewer.Shortcuts.ProcessKey(keyData))
            {
                return true;
            }

            // This key was not processed, bubble it up to the base class.
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion VerificationTaskForm Overrides

        #region VerificationTaskForm OnEvents

        /// <summary>
        /// Raises the <see cref="FileComplete"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="FileComplete"/> 
        /// event.</param>
        protected virtual void OnFileComplete(FileCompleteEventArgs e)
        {
            if (FileComplete != null)
            {
                FileComplete(this, e);
            }
        }

        #endregion VerificationTaskForm OnEvents

        #region VerificationTaskForm Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        void HandleSaveToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                Save(true);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26628", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleSaveAndCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Save(true);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26785", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleSaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Save(false);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27044", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandlePrintToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // TODO: Implement me
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27045", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleSkipProcessingToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                OnFileComplete(new FileCompleteEventArgs(EFileProcessingResult.kProcessingSkipped));
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27046", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleStopProcessingToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                OnFileComplete(new FileCompleteEventArgs(EFileProcessingResult.kProcessingCancelled));
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27047", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleDiscardChangesToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // TODO: Implement me
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27048", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleIDShieldHelpToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // TODO: Implement me
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27049", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleAboutIDShieldToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // TODO: Implement me
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27050", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleApplyExemptionToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                _redactionGridView.PromptForExemptions();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26710", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleLastExemptionToolStripButtonClick(object sender, EventArgs e)
        {
            try
            {
                _redactionGridView.ApplyLastExemptions();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26711", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="RedactionGridView.ExemptionsApplied"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="RedactionGridView.ExemptionsApplied"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="RedactionGridView.ExemptionsApplied"/> event.</param>
        void HandleRedactionGridViewExemptionsApplied(object sender, ExemptionsAppliedEventArgs e)
        {
            try
            {
                // Enable the apply last exemption codes tool strip button if there are redactions
                _lastExemptionToolStripButton.Enabled = _imageViewer.LayerObjects.Count > 0;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27051", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ImageViewer.ImageFileChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ImageViewer.ImageFileChanged"/> event.</param>
        void HandleImageViewerImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                if (_imageViewer.IsImageAvailable)
                {
                    _currentDocumentTextBox.Text = _imageViewer.ImageFile;

                    string voaFile = GetVoaFileName();
                    if (File.Exists(voaFile))
                    {
                        _redactionGridView.LoadFrom(voaFile);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26760", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.</param>
        void HandleImageViewerLayerObjectAdded(object sender, LayerObjectAddedEventArgs e)
        {
            try
            {
                if (_imageViewer.LayerObjects.Count == 1)
                {
                    _applyExemptionToolStripButton.Enabled = true;
                    _lastExemptionToolStripButton.Enabled = _redactionGridView.HasAppliedExemptions;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27052", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event.</param>
        void HandleImageViewerLayerObjectDeleted(object sender, LayerObjectDeletedEventArgs e)
        {
            try
            {
                if (_imageViewer.LayerObjects.Count == 0)
                {
                    _applyExemptionToolStripButton.Enabled = false;
                    _lastExemptionToolStripButton.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27053", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion VerificationTaskForm Event Handlers

        #region IVerificationForm Members

        /// <summary>
        /// A thread-safe method that opens a document for verification.
        /// </summary>
        /// <param name="fileName">The filename of the document to open.</param>
        /// <param name="fileID">The ID of the file being processed.</param>
        /// <param name="actionID">The ID of the action being processed.</param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use.</param>
        public void Open(string fileName, int fileID, int actionID, FAMTagManager tagManager,
            FileProcessingDB fileProcessingDB)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new VerificationFormOpen(Open),
                        new object[] { fileName, fileID, actionID, tagManager, fileProcessingDB });
                    return;
                }

                _imageViewer.OpenImage(fileName, false);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26627",
                    "Unable to open file for verification.", ex);
                ee.AddDebugData("File name", fileName, false);
                throw ee;
            }
        }

        #endregion IVerificationForm Members
    }
}