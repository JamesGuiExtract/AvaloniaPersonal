using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TD.SandDock;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a dialog that allows the user to verify redactions.
    /// </summary>
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
        // Temporarily suppress this warning. Verification settings will be used in the future.
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        readonly VerificationSettings _settings;

        #endregion VerificationTaskForm Fields

        #region VerificationTaskForm Events

        /// <summary>
        /// Occurs when a file has completed verification.
        /// </summary>
        public event EventHandler<EventArgs> FileVerified;

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
        }

        #endregion VerificationTaskForm Constructors

        #region VerificationTaskForm Methods

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

            _imageViewer.EstablishConnections(this);
        }

        #endregion VerificationTaskForm Overrides

        #region VerificationTaskForm OnEvents

        /// <summary>
        /// Raises the <see cref="FileVerified"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="FileVerified"/> 
        /// event.</param>
        protected virtual void OnFileVerified(EventArgs e)
        {
            if (FileVerified != null)
            {
                FileVerified(this, e);
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
                OnFileVerified(new EventArgs());
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26628", ex);
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
        public void Open(string fileName)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new StringParameter(Open), new object[] { fileName });
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