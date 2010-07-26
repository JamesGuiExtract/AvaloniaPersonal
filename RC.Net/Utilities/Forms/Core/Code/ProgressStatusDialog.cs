using Extract.Interop;
using Extract.Licensing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Class for showing a progress status dialog.
    /// </summary>
    [ComVisible(true)]
    [ProgId("Extract.Utilties.Forms.ProgressStatusDialog"),
    GuidAttribute("07BC6AE1-B039-472A-A22D-BAD9BBBA5721")]
    public class ProgressStatusDialog : IProgressStatusDialog, ILicensedComponent, IDisposable
    {
        #region Fields

        /// <summary>
        /// Class name used in license validation calls
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ProgressStatusDialog).ToString();

        /// <summary>
        /// The progress status dialog that will be displayed.
        /// </summary>
        ProgressStatusDialogForm _statusForm;

        #endregion Fields

        #region IProgressStatusDialog Members

        /// <summary>
        /// Closes (hides) the <see cref="ProgressStatusDialogForm"/>.
        /// </summary>
        public void Close()
        {
            try
            {
                if (_statusForm == null)
                {
                    throw new ExtractException("ELI30340",
                        "Cannot close progress dialog if it has not been shown.");
                }

                _statusForm.Hide();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI30341",
                    "Failed to close progress dialog.", ex);
            }
        }

        /// <summary>
        /// Gets/sets the progress status object that is used to update the progress bars
        /// in the dialog.
        /// </summary>
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public ProgressStatus ProgressStatusObject
        {
            get
            {
                try
                {
                    if (_statusForm == null)
                    {
                        throw new ExtractException("ELI30342",
                            "Cannot get progress status object until dialog has been shown.");
                    }

                    return _statusForm.ProgressStatus;
                }
                catch (Exception ex)
                {
                    throw ExtractException.CreateComVisible("ELI30430",
                        "Cannot get progress status object.", ex);
                }
            }
            set
            {
                try
                {
                    if (_statusForm == null)
                    {
                        throw new ExtractException("ELI30343",
                            "Cannot set progress status object until dialog has been shown.");
                    }

                    _statusForm.ProgressStatus = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.CreateComVisible("ELI30431",
                        "Cannot set progress status object.", ex);
                }
            }
        }

        /// <summary>
        /// Displays the <see cref="ProgressStatusDialogForm"/> as a modeless dialog
        /// box.
        /// </summary>
        /// <param name="hWndParent">The parent window for the dialog.</param>
        /// <param name="strWindowTitle">The title to display in the progress dialog.</param>
        /// <param name="pProgressStatus">The progress status object to use to update the
        /// progress bars.</param>
        /// <param name="nNumProgressLevels">The number of levels of progress to display.</param>
        /// <param name="nDelayBetweenRefreshes">The amount of delay between refreshes of the
        /// progress information.</param>
        /// <param name="bShowCloseButton">Whether the close button should be displayed or not.</param>
        /// <param name="hStopEvent">The event handle to signal when the stop button is pressed.
        /// If <paramref name="hStopEvent"/> is <see cref="IntPtr.Zero"/> then no stop
        /// button will be displayed.</param>
        /// <returns>The Hresult for the call.</returns>
        [CLSCompliant(false)]
        public int ShowModelessDialog(IntPtr hWndParent, string strWindowTitle,
            ProgressStatus pProgressStatus, int nNumProgressLevels, int nDelayBetweenRefreshes,
            bool bShowCloseButton, IntPtr hStopEvent)
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30344", _OBJECT_NAME);

                if (_statusForm == null)
                {
                    _statusForm = new ProgressStatusDialogForm(hWndParent, nNumProgressLevels,
                        nDelayBetweenRefreshes, bShowCloseButton, hStopEvent);
                }

                _statusForm.UpdateTitle(strWindowTitle);
                _statusForm.ProgressStatus = pProgressStatus;

                _statusForm.Show();

                return HResult.Ok;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI30345",
                    "Failed to show progress dialog.", ex);
            }
        }

        /// <summary>
        /// Gets/sets the title to be displayed in the progress dialog.
        /// </summary>
        /// <exception cref="ExtractException">If the dialog has not been shown yet.</exception>
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public string Title
        {
            get
            {
                try
                {
                    if (_statusForm == null)
                    {
                        throw new ExtractException("ELI30346",
                            "Cannot get title until dialog has been shown.");
                    }

                    return _statusForm.Text;
                }
                catch (Exception ex)
                {
                    throw ExtractException.CreateComVisible("ELI30433",
                        "Cannot get title.", ex);
                }
            }
            set
            {
                try
                {
                    if (_statusForm == null)
                    {
                        throw new ExtractException("ELI30347",
                            "Cannot set title until dialog has been shown.");
                    }

                    _statusForm.UpdateTitle(value);
                }
                catch (Exception ex)
                {
                    throw ExtractException.CreateComVisible("ELI30432",
                        "Cannot set title.", ex);
                }
            }
        }

        #endregion

        #region ILicensedComponent Members

        /// <summary>
        /// Indicates whether this object is licensed or not.
        /// </summary>
        /// <returns><see langword="true"/> if this object is licensed and
        /// <see langword="false"/> if it is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(LicenseIdName.ExtractCoreObjects);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="TemporaryFile"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="TemporaryFile"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="TemporaryFile"/>. Also
        /// deletes the temporary file being managed by this class.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_statusForm != null)
                {
                    _statusForm.Dispose();
                    _statusForm = null;
                }
            }
        }

        #endregion IDisposable
    }
}
