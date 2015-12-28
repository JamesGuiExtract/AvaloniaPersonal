using Extract.Utilities.Forms;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// Methods for managing the secure counters in a <see cref="FileProcessingDB"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("D22193BD-EC71-4494-B185-F673EEA548D2")]
    [ProgId("Extract.FileActionManager.Database.SecureCounterManagement")]
    [CLSCompliant(false)]
    public class SecureCounterManagement : ISecureCounterManagement
    {
        /// <summary>
        /// Used to indicate that the UI is active or soon will be
        /// If this has a value of 1 the UI is active
        /// If this has a value of 0 the UI is not active
        /// </summary>
        static int _UICount;

        #region ISecureCounterManagement

        /// <summary>
        /// Displays a UI for managing the secure counters in <see paramref="pDB"/>.
        /// </summary>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> for which secure counters are being
        /// managed.</param>
        /// <param name="owner">The handle of the window to which the displayed UI should be modal.
        /// </param>
        public void ShowUI(FileProcessingDB pDB, IntPtr owner)
        {
            try 
	        {
                // Set the _UICount to 1 if it isn't already
                if (1 == Interlocked.CompareExchange(ref _UICount, 1, 0))
                {
                    // UI is currently active since the previous value was 1
                    return;
                }

                Thread uiThread = new Thread(() =>
                {
                    try
                    {
                        using (var form = new ManageSecureCountersForm(pDB))
                        {
                            form.ShowDialog(new WindowWrapper(owner));
                        }
                    }
                    catch (Exception ex)
                    {
                        ExtractException ee = new ExtractException("ELI39118", "Unable to display Manage Counter Dialog.", ex);
                        ee.Display();
                    }
                    finally
                    {
                        // set the value of the _UICount to 0
                        Interlocked.Exchange(ref _UICount, 0);
                    }
                });
                // Single-threaded apartment state is needed for copy/paste or drag/drop to work.
                uiThread.SetApartmentState(ApartmentState.STA);
                uiThread.Start();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI39089", ex.Message);
            }
        }

        #endregion ISecureCounterManagement
    }
}
