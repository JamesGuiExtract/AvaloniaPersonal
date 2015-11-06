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
                Thread uiThread = new Thread(() =>
                {
                    using (var form = new ManageSecureCountersForm(pDB))
                    {
                        form.ShowDialog(new WindowWrapper(owner));
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
