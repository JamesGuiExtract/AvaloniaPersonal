using Extract.Utilities.Forms;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Extract.UtilityApplications.PaginationUtility
{
    partial class PageLayoutControl
    {
        /// <summary>
        /// Used to temporarily suspend input, drawing, or layout of controls to prevent flashing,
        /// artifacts or undesired input when the UI is in the midst of operations that involve a
        /// lot of shuffling of controls or data loading.
        /// NOTE: While the UI is locked via individual class instances in order to keep track of
        /// separate reasons the UI is locked, the implementation is implemented via thread static
        /// fields, thus only one <see cref="PageLayoutControl"/> is supported per thread.
        /// </summary>
        class UIUpdateLock : IDisposable
        {
            // The number of instances that currently exist.
            [ThreadStatic]
            static int _referenceCount;
            
            /// <summary>
            /// Prevents painting of the control during the operation.
            /// </summary>
            [ThreadStatic]
            static LockControlUpdates _controlUpdateLock;

            /// <summary>
            /// The wait cursor that is displayed during the operation.
            /// </summary>
            [ThreadStatic]
            static TemporaryWaitCursor _waitCursor;

            /// <summary>
            /// Indicates whether a full layout should be performed when this instance is disposed.
            /// </summary>
            [ThreadStatic]
            static bool _forceFullLayout;

            /// <summary>
            /// The <see cref="PageLayoutControl"/> for which the update is taking place.
            /// </summary>
            [ThreadStatic]
            static PageLayoutControl _pageLayoutControl;

            /// <summary>
            /// Initializes a new instance of the <see cref="UIUpdateLock"/> class.
            /// NOTE: While the UI is locked via individual class instances in order to keep track of
            /// separate reasons the UI is locked, the implementation is implemented via thread static
            /// fields, thus only one <see cref="PageLayoutControl"/> is supported per thread.
            /// </summary>
            /// <param name="pageLayoutControl">The <see cref="PageLayoutControl"/> for which the
            /// update is taking place.</param>
            /// <param name="forceLayoutOnResume"><c>true</c> to force a full layout when layout is
            /// resumed.</param>
            public UIUpdateLock(PageLayoutControl pageLayoutControl, bool forceLayoutOnResume = false)
            {
                Extract.ExtractException.Assert("ELI50184", "Invalid control state", _referenceCount >= 0);

                if (_referenceCount == 0)
                {
                    _pageLayoutControl = pageLayoutControl;
                    _forceFullLayout = forceLayoutOnResume;

                    LockUI();
                }
                else
                {
                    Extract.ExtractException.Assert("ELI50185", "Invalid control state",
                        pageLayoutControl == _pageLayoutControl);

                    _forceFullLayout |= forceLayoutOnResume;
                }

                _referenceCount++;
            }

            /// <summary>
            /// <c>true</c> if at least on instance of this class exists
            /// NOTE: <c>true</c> will be returned during <see cref="RefreshUI"/>, even though the UI will
            /// be briefly enabled.
            /// </summary>
            public static bool IsLocked
            {
                get
                {
                    return _referenceCount > 0;
                }
            }

            /// <summary>
            /// Refreshes the UI on demand without removing any existing lock instances.
            /// </summary>
            public static void RefreshUI()
            {
                try
                {
                    if (_referenceCount > 0)
                    {
                        UnlockUI(temporary: true);
                        LockUI();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI50197");
                }
            }

            /// <summary>
            /// Releases all resources used by the <see cref="UIUpdateLock"/>.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases all unmanaged resources used by the <see cref="UIUpdateLock"/> (unlocks the UI).
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
            /// resources; <see langword="false"/> to release only unmanaged resources.</param>
            // These fields are disposed of, just not directly.

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
            void Dispose(bool disposing)
            {
                if (disposing)
                {
                    try
                    {
                        // Dispose of managed resources
                        _referenceCount--;

                        if (_referenceCount == 0)
                        {
                            UnlockUI(temporary: false);
                        }
                    }
                    catch { }
                }
                // Dispose of unmanaged resources
            }

            /// <summary>
            /// Locks the <see cref="_pageLayoutControl"/> UI.
            /// </summary>
            static void LockUI()
            {
                if (_controlUpdateLock == null)
                {
                    _pageLayoutControl.SuspendingUIUpdates?.Invoke(_pageLayoutControl, new EventArgs());

                    _controlUpdateLock =
                        new LockControlUpdates(_pageLayoutControl._flowLayoutPanel, true, true);

                    // Wait cursor may be persisted thru temporary unlocks
                    if (_waitCursor == null)
                    {
                        _waitCursor = new TemporaryWaitCursor();
                    }

                    _pageLayoutControl.SuspendLayout();
                    _pageLayoutControl._flowLayoutPanel.SuspendLayout();

                    if (_pageLayoutControl.LoadNextDocumentVisible)
                    {
                        _pageLayoutControl.RemovePaginationControl(_pageLayoutControl._loadNextDocumentButtonControl, false);
                    }
                }
            }

            /// <summary>
            /// Unlocks the <see cref="_pageLayoutControl"/> UI.
            /// </summary>
            /// <param name="temporary"><c>true</c> if the UI is only being temporarily unlocked to refresh the UI
            /// or <c>false</c> if the UI is intented to be usable at this point.</param>
            static void UnlockUI(bool temporary)
            {
                if (_pageLayoutControl != null && _controlUpdateLock != null)
                {
                    if (_pageLayoutControl.LoadNextDocumentVisible)
                    {
                        _pageLayoutControl.AddPaginationControl(_pageLayoutControl._loadNextDocumentButtonControl);
                    }

                    _controlUpdateLock.Dispose();
                    _controlUpdateLock = null;

                    if (!temporary)
                    {
                        _waitCursor.Dispose();
                        _waitCursor = null;
                    }

                    // The lock will have taken focus away from the pageLayoutControl
                    _pageLayoutControl.Focus();

                    if (_forceFullLayout)
                    {
                        ((PaginationLayoutEngine)_pageLayoutControl._flowLayoutPanel.LayoutEngine).ForceNextLayout = true;
                        _forceFullLayout = false;
                    }
                    _pageLayoutControl._flowLayoutPanel.ResumeLayout(true);
                    _pageLayoutControl.ResumeLayout(true);
                    _pageLayoutControl.UpdateCommandStates();

                    _pageLayoutControl.ResumingUIUpdates?.Invoke(_pageLayoutControl, new EventArgs());
                }
            }
        }
    }
}
