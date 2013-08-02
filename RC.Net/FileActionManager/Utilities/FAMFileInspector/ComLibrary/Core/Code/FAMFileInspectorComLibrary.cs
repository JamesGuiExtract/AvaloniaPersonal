using Extract.Utilities.Forms;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Interface for the <see cref="FAMFileInspectorComLibrary"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("C9F9EA07-1B0C-4A31-B8F1-EF0112D1B176")]
    [CLSCompliant(false)]
    public interface IFAMFileInspector
    {
        /// <summary>
        /// Opens the FAM file inspector.
        /// <para><b>Note</b></para>
        /// The FAM file inspector form will use and modify the settings of
        /// <see paramref="fileSelector"/>. The caller should not continue to use this
        /// <see cref="IFAMFileSelector"/> instance if it does not wish to share settings changes
        /// with the <see cref="FAMFileInspectorForm"/>.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> to be inspected.
        /// </param>
        /// <param name="fileSelector">The <see cref="IFAMFileSelector"/> specifying the set of
        /// files to be inspected or <see langword="null"/> to inspect all files in the database.
        /// </param>
        void OpenFAMFileInspector(FileProcessingDB fileProcessingDB, IFAMFileSelector fileSelector);
    }

    /// <summary>
    /// Exposes use of a <see cref="FAMFileInspectorForm"/> via COM.
    /// </summary>
    [Guid("4C6B10BE-EEF6-4723-9D62-E773ADC8ABED")]
    [ProgId("Extract.FileActionManager.FAMFileInspector")]
    [ComVisible(true)]
    [CLSCompliant(false)]
    public class FAMFileInspectorComLibrary : IFAMFileInspector
    {
        #region Fields

        /// <summary>
        /// Synchronizes access to <see cref="_fileInspectorForm"/>.
        /// </summary>
        static object _lock = new object();

        /// <summary>
        /// The <see cref="FAMFileInspectorForm"/> instance currently in use by this class.
        /// </summary>
        static volatile FAMFileInspectorForm _fileInspectorForm;

        #endregion Fields

        #region IFAMFileInspector

        /// <summary>
        /// Opens the FAM file inspector.
        /// <para><b>Note</b></para>
        /// The FAM file inspector form will use and modify the settings of
        /// <see paramref="fileSelector"/>. The caller should not continue to use this
        /// <see cref="IFAMFileSelector"/> instance if it does not wish to share settings changes
        /// with the <see cref="FAMFileInspectorForm"/>.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> to be inspected.
        /// </param>
        /// <param name="fileSelector">The <see cref="IFAMFileSelector"/> specifying the set of
        /// files to be inspected or <see langword="null"/> to inspect all files in the database.
        /// </param>
        public void OpenFAMFileInspector(FileProcessingDB fileProcessingDB, IFAMFileSelector fileSelector)
        {
            try
            {
                lock (_lock)
                {
                    // If the file inspector has been created and has not been disposed (closed).
                    if (_fileInspectorForm != null && !_fileInspectorForm.IsDisposed)
                    {
                        try
                        {
                            _fileInspectorForm.Invoke((MethodInvoker)(() =>
                            {
                                // Uses fileProcessingDB.LoggedInAsAdmin to determine whether
                                // _fileInspectorForm should run in admin mode.
                                _fileInspectorForm.SetLogOnMode(fileProcessingDB);

                                // Because the FileProcessingDB may be re-configured to connect to
                                // a new DB from within this app and because we don't want that
                                // affecting an outside caller still using it (DBAdmin), just copy
                                // the connection settings rather that using the passed in
                                // FileProcessingDB.
                                CopyDbConnectionSettings(fileProcessingDB,
                                    _fileInspectorForm.FileProcessingDB);
                                _fileInspectorForm.InitializeContextMenu();
                                if (fileSelector == null)
                                {
                                    // No provided file selection settings should be interpreted as
                                    // to bring the form up with no file selection conditions.
                                    _fileInspectorForm.ResetFileSelectionSettings();
                                }
                                else
                                {
                                    _fileInspectorForm.FileSelector = fileSelector;
                                    _fileInspectorForm.FileSelector.LimitToSubset(false, false,
                                        FAMFileInspectorForm.MaxFilesToDisplay);
                                }

                                _fileInspectorForm.Restore();
                                _fileInspectorForm.Activate();
                                _fileInspectorForm.GenerateFileList(false);
                            }));
                        }
                        catch (Exception ex)
                        {
                            // Allow for the possibility that the form may have been in the process
                            // of closing during this call despite the above IsDisposed check. If
                            // after a second, _fileInspectorForm is now found to have been disposed,
                            // ignore the error the error and spawn a new form.
                            Thread.Sleep(1000);
                            if (!_fileInspectorForm.IsDisposed)
                            {
                                throw ex.AsExtract("ELI35798");
                            }
                        }

                        // An existing form was used successfully.
                        return;
                    }

                    // [DotNetRCAndUtils:1009]
                    // The form needs to be launched into it's own STA thread. Otherwise there are
                    // message handling issue that can intefere with tab order and print
                    // functionality among other things.
                    Thread uiThread = new Thread(() =>
                    {
                        try
                        {
                            // A new form is needed.
                            _fileInspectorForm = new FAMFileInspectorForm();

                            // Uses fileProcessingDB.LoggedInAsAdmin to determine whether
                            // _fileInspectorForm should run in admin mode.
                            _fileInspectorForm.SetLogOnMode(fileProcessingDB);

                            // Because the FileProcessingDB may be re-configured to connect to
                            // a new DB from within this app and because we don't want that
                            // affecting an outside caller still using it (DBAdmin), just copy
                            // the connection settings rather that using the passed in
                            // FileProcessingDB.
                            CopyDbConnectionSettings(fileProcessingDB,
                                _fileInspectorForm.FileProcessingDB);
                            if (fileSelector == null)
                            {
                                // No provided file selection settings should be interpreted as
                                // to bring the form up with no file selection conditions.
                                _fileInspectorForm.ResetFileSelectionSettings();
                            }
                            else
                            {
                                _fileInspectorForm.FileSelector = fileSelector;
                                _fileInspectorForm.FileSelector.LimitToSubset(false, false,
                                    FAMFileInspectorForm.MaxFilesToDisplay);
                            }

                            Application.Run(_fileInspectorForm);
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractDisplay("ELI35851");
                        }
                    });
                    uiThread.SetApartmentState(ApartmentState.STA);
                    uiThread.Start();
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35793", "Error opening FAMFileInspector");
            }
        }

        #endregion IFAMFileInspector

        #region Private Members

        /// <summary>
        /// Copies the databases connection settings from <see paramref="sourceDb"/> to
        /// <see paramref="targetDb"/>.
        /// </summary>
        /// <param name="sourceDb">The source <see cref="FileProcessingDB"/>.</param>
        /// <param name="targetDb">The target <see cref="FileProcessingDB"/>.</param>
        static void CopyDbConnectionSettings(FileProcessingDB sourceDb,  FileProcessingDB targetDb)
        {
            targetDb.CloseAllDBConnections();
            targetDb.DatabaseServer = sourceDb.DatabaseServer;
            targetDb.DatabaseName = sourceDb.DatabaseName;
            targetDb.AdvancedConnectionStringProperties
                = sourceDb.AdvancedConnectionStringProperties;
        }

        #endregion Private Members
    }
}
