using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// A custom FFI component that implements this interface manages custom data that requires an
    /// explicit click of an "OK" button to be applied or "Cancel" to be reverted.
    /// The <see cref="FAMFileInspectorForm"/> will only display OK and Cancel buttons if this
    /// interface is implemented for any custom FFI component.
    /// </summary>
    public interface IFFIDataManager
    {
        /// <summary>
        /// Gets if there is any changes that need to be applied.
        /// </summary>
        bool Dirty
        {
            get;
        }

        /// <summary>
        /// Gets a description of changes that should be displayed to the user in a prompt when
        /// applying changes. If <see langword="null"/>, no prompt will be displayed when applying
        /// changed.
        /// </summary>
        string ApplyPrompt
        {
            get;
        }

        /// <summary>
        /// Gets a description of changes that should be displayed to the user in a prompt when
        /// the user is canceling changes. If <see langword="null"/>, no prompt will be displayed
        /// when canceling except if the FFI is closed via the form's cancel button (red X).
        /// </summary>
        string CancelPrompt
        {
            get;
        }

        /// <summary>
        /// Applies all uncommitted values specified via SetValue.
        /// </summary>
        /// <returns><see langword="true"/> if the changes were successfully applied; otherwise,
        /// <see langword="false"/>.</returns>
        bool Apply();

        /// <summary>
        /// Cancels all uncommitted data changes specified via SetValue.
        /// </summary>
        void Cancel();
    }
}
