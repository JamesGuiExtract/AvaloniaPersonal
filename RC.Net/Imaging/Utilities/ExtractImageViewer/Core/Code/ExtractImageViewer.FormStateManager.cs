using System;
using System.Windows.Forms;
using TD.SandDock;

namespace Extract.Imaging.Utilities.ExtractImageViewer
{
    partial class ExtractImageViewerForm
    {
        /// <summary>
        /// Manages the state of a <see cref="ExtractImageViewerForm"/>'s user interface properties.
        /// <para><b>Note</b></para>
        /// Though the methods SaveState and RestoreSavedState allow for explicitly loading and
        /// saving the UI state information, this class will automatically save
        /// the form's state when the managed <see cref="Form"/>'s <see cref="Form.Closing"/> event is
        /// raised and restore the saved state when the managed <see cref="Form"/>'s
        /// <see cref="Form.Load"/> event is raised.
        /// </summary>
        class FormStateManager : Extract.Utilities.Forms.FormStateManager
        {
            #region Fields

            /// <summary>
            /// The <see cref="ExtractImageViewerForm"/> for which state information is being
            /// managed.
            /// </summary>
            ExtractImageViewerForm _imageViewerForm;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="FormStateManager"/> class.
            /// </summary>
            /// <param name="imageViewerForm">The <see cref="ExtractImageViewerForm"/> whose state
            /// is to be managed.</param>
            /// <param name="persistenceFileName">The name of the file to which form properties will
            /// be maintained.</param>
            /// <param name="mutexString">Name for the mutex used to serialize persistance of the
            /// control and form layout.</param>
            /// <param name="sandDockManager">If specified, this <see cref="SandDockManager"/>'s state
            /// info will be persisted.</param>
            public FormStateManager(ExtractImageViewerForm imageViewerForm,
                string persistenceFileName, string mutexString, SandDockManager sandDockManager)
                : base(imageViewerForm, persistenceFileName, mutexString, sandDockManager, true)
            {
                try
                {
                    _imageViewerForm = imageViewerForm;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30799", ex);
                }
            }

            #endregion Constructors

            #region Overrides

            /// <summary>
            /// Restores the <see cref="ExtractImageViewer"/>'s UI state from disk.
            /// </summary>
            /// <retuns><see langword="true"/> if the UI state was restored from disk,
            /// <see langword="false"/> otherwise.</retuns>
            public override bool RestoreSavedState()
            {
                try
                {
                    if (!_imageViewerForm._resetLayout)
                    {
                        return base.RestoreSavedState();
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30800", ex);
                }
            }

            #endregion Overrides
        }
    }
}
