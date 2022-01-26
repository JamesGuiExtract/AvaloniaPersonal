using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Testing.Utilities
{
    /// <summary>
    /// Creates a FAM processing session for use in unit tests.
    /// <para><b>WARNING</b></para>
    /// When there are problems that occur within one of these sessions, the symptoms have at times
    /// been unintuitive. Rather than seeing a relevant exception, I have seen the session become
    /// deadlocked or license related errors.
    /// If a test fails or hangs that is using FAMProcessingSession, check the extract exception log,
    /// run the same setup with and actual FAM or attach a debugger to find the root cause.
    /// </summary>
    /// <remarks>If you get a test failure and debugging shows a license failure due to failure to load Extract
    /// then you might need to add this assembly attribute to the test project: [assembly: TestAssemblyDirectoryResolve]</remarks>
    /// <seealso cref="System.IDisposable" />
    [CLSCompliant(false)]
    public class FAMProcessingSession : IDisposable
    {
        FileProcessingManager _fileProcessingManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMProcessingSession"/> class.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/>.
        /// NOTE: This DB is used only for connection info; the FileProcessingManager create it's
        /// own instance.</param>
        /// <param name="actionName">Name of the action to process.</param>
        /// <param name="workflowName">Name of the workflow to process within or empty for all workflows.</param>
        /// <param name="fileProcessingTask">The <see cref="IFileProcessingTask"/> to be executed.</param>
        /// <param name="threadCount">The number threads to use.</param>
        /// <param name="filesToGrab">The number files to grab at a time from the DB.</param>
        /// <param name="keepProcessing"><c>true</c> if processing should continue as files are added, or
        /// <c>false</c> to stop processing as soon as the queue is empty.</param>
        /// <param name="docsToProcess">The number of files to process before the processing session should
        /// automatically stop or zero if the process should not automatically stop after processing a set
        /// number of files.</param>
        public FAMProcessingSession(IFileProcessingDB fileProcessingDB, string actionName, string workflowName = "",
            IFileProcessingTask fileProcessingTask = null, int threadCount = 1, int filesToGrab = 1,
            bool keepProcessing = false, int docsToProcess = 0)
        {
            try
            {
                _fileProcessingManager = new FileProcessingManager();
                _fileProcessingManager.DatabaseServer = fileProcessingDB.DatabaseServer;
                _fileProcessingManager.DatabaseName = fileProcessingDB.DatabaseName;
                _fileProcessingManager.ActionName = actionName;
                _fileProcessingManager.ActiveWorkflow = workflowName;
                _fileProcessingManager.MaxFilesFromDB = filesToGrab;
                _fileProcessingManager.FileProcessingMgmtRole.NumThreads = threadCount;
                _fileProcessingManager.FileProcessingMgmtRole.KeepProcessingAsAdded = keepProcessing;
                _fileProcessingManager.NumberOfDocsToProcess = docsToProcess;

                var descriptor = new ObjectWithDescription();
                descriptor.Object = fileProcessingTask ?? new NullFileProcessingTask();

                var tasksVector = new[] { descriptor }.ToIUnknownVector<ObjectWithDescription>();

                _fileProcessingManager.FileProcessingMgmtRole.FileProcessors = tasksVector;
                var fileActionMgmtRole = (IFileActionMgmtRole)_fileProcessingManager.FileProcessingMgmtRole;
                fileActionMgmtRole.Enabled = true;

                _fileProcessingManager.StartProcessing();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI42132");
            }
        }

        public FileProcessingManager FileProcessingManager
        {
            get
            {
                return _fileProcessingManager;
            }
        }

        /// <summary>
        /// Blocks until processing has completed (queue is empty).
        /// <para><b>NOTE</b></para>
        /// This method assumes files are to be queued. If the session is configured to keep
        /// processing as files are queued, it will stop as soon as a files are queued then process.
        /// However, if no files are ever queued, the call will block forever.
        /// <para><b>NOTE2</b></para>
        /// There seems to be a bug with this method that allows it to return before processing is
        /// actually complete when running a multi-threaded instance.
        /// </summary>
        /// <returns>The number of files that were successfully processed.</returns>
        public int WaitForProcessingToComplete()
        {
            try
            {
                while (_fileProcessingManager.ProcessingStarted &&
                        _fileProcessingManager.FileProcessingMgmtRole != null &&
                        !_fileProcessingManager.FileProcessingMgmtRole.HasProcessingCompleted)
                {
                    System.Threading.Thread.Sleep(200);
                }

                _fileProcessingManager.GetCounts(out int succeeded, out int errors, out int supplied, out int supplyErrors);
                return succeeded;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI42131");
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="FAMProcessingSession"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="FAMProcessingSession"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="FAMProcessingSession"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (_fileProcessingManager != null &&
                        _fileProcessingManager.ProcessingStarted)
                    {
                        _fileProcessingManager.StopProcessing();
                        while (_fileProcessingManager.ProcessingStarted)
                        {
                            System.Threading.Thread.Sleep(200);
                        }
                    }

                    // For reasons that aren't entirely clear to me, without this call to clear
                    // subsequent FAM sessions fail.
                    _fileProcessingManager.Clear();
                    _fileProcessingManager = null;
                }
                catch { }
            }
        }

        #endregion IDisposable Members
    }
}
