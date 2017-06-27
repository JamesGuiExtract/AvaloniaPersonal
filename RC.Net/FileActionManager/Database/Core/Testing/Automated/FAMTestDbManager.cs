using Extract.Database;
using Extract.Utilities;
using Extract.Testing.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database.Test
{
    /// <summary>
    /// Manages FAM databases for unit tests.
    /// </summary>
    /// <typeparam name="T">The unit test class for which this manager is needed.</typeparam>
    /// <seealso cref="System.IDisposable" />
    [CLSCompliant(false)]
    public class FAMTestDBManager<T> : IDisposable
    {
        #region Fields

        /// <summary>
        /// The <see cref="TestFileManager"/> that manages DB backup files.
        /// </summary>
        TestFileManager<T> _backupFileManager = new TestFileManager<T>();

        /// <summary>
        /// The FAM databases being actively managed by this instance.
        /// </summary>
        Dictionary<string, FileProcessingDB> _activeDatabases = new Dictionary<string, FileProcessingDB>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestFileManager{T}"/> class.
        /// </summary>
        public FAMTestDBManager()
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Gets an IFileProcessingDB connection to a new FAM database created on the local database
        /// instance. 
        /// </summary>
        /// <param name="databaseName">The name to give to the database.</param>
        /// <returns>A <see cref="IFileProcessingDB"/> for the database.</returns>
        public FileProcessingDB GetNewDatabase(string databaseName)
        {
            try
            {
                FileProcessingDB fileProcessingDb = null;
                if (_activeDatabases.TryGetValue(databaseName, out fileProcessingDb))
                {
                    var ee = new ExtractException("ELI42028", "Database already exists");
                    ee.AddDebugData("Database name", databaseName, false);
                    throw ee;
                }
                else
                {
                    fileProcessingDb = new FileProcessingDB();
                    _activeDatabases[databaseName] = fileProcessingDb;
                    fileProcessingDb.DatabaseServer = "(local)";
                    fileProcessingDb.CreateNewDB(databaseName, bstrInitWithPassword: "a");

                    return fileProcessingDb;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    RemoveDatabase(databaseName);
                }
                catch { }

                var ee = ex.AsExtract("ELI42034");
                ee.AddDebugData("Database", databaseName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets an IFileProcessingDB connection to local database <see paramref="destinationDbName"/>
        /// as restored from the backup <see paramref="dbBackupResourceName"/>.
        /// </summary>
        /// <param name="dbBackupResourceName">The database backup as an embedded resource.</param>
        /// <param name="destinationDBName">The name the database should be restored to.</param>
        public FileProcessingDB GetDatabase(string dbBackupResourceName, string destinationDBName)
        {
            string backupDbFile = null;

            try
            {
                FileProcessingDB fileProcessingDb = null;
                if (_activeDatabases.TryGetValue(destinationDBName, out fileProcessingDb))
                {
                    return fileProcessingDb;
                }
                else
                {
                    backupDbFile = _backupFileManager.GetFile(dbBackupResourceName);

                    // In most cases SQL server will not have access to the file; giving access to
                    // all users will allow it access.
                    FileSecurity fSecurity = File.GetAccessControl(backupDbFile);
                    fSecurity.AddAccessRule(new FileSystemAccessRule(
                        @".\users", FileSystemRights.FullControl, AccessControlType.Allow));
                    File.SetAccessControl(backupDbFile, fSecurity);

                    DBMethods.RestoreDatabaseToLocalServer(backupDbFile, destinationDBName);
                    _backupFileManager.RemoveFile(dbBackupResourceName);

                    // Get a FileProcessingDB instance to the DB, and use it to upgrade the database schema.
                    fileProcessingDb = new FileProcessingDB();
                    _activeDatabases[destinationDBName] = fileProcessingDb;
                    fileProcessingDb.DatabaseServer = "(local)";
                    fileProcessingDb.DatabaseName = destinationDBName;
                    fileProcessingDb.UpgradeToCurrentSchema(null);

                    return fileProcessingDb;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    RemoveDatabase(destinationDBName);
                }
                catch { }

                var ee = ex.AsExtract("ELI41908");
                ee.AddDebugData("Database", destinationDBName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Removes the specified <see paramref="databaseName"/> from the local SQL instance.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        public void RemoveDatabase(string databaseName)
        {
            FileProcessingDB fileProcessingDb = null;
            ExtractException ee = null;

            // First try to close all DB connections for the FileProcessingDb instance
            try
            {
                if (_activeDatabases.TryGetValue(databaseName, out fileProcessingDb))
                {
                    fileProcessingDb.CloseAllDBConnections();
                }
            }
            catch (Exception ex)
            {
                ee = ex.AsExtract("ELI41909");
            }

            // Then try to drop the DB regardless of whether there were errors trying to close the
            // connections.
            try
            {
                if (fileProcessingDb != null)
                {
                    DBMethods.DropLocalDB(databaseName);
                    _activeDatabases.Remove(databaseName);
                }
            }
            catch (Exception ex)
            {
                if (ee == null)
                {
                    ex.AsExtract("ELI41910").ExtractLog("ELI43552");
                }
                else
                {
                    ex.ExtractLog("ELI41912");
                }
            }
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="FAMTestDBManager{T}"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="FAMTestDBManager{T}"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="FAMTestDBManager{T}"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_activeDatabases != null)
                {
                    var activeDbs = _activeDatabases.Keys.ToArray();
                    foreach (var databaseName in activeDbs)
                    {
                        try
                        {
                            RemoveDatabase(databaseName);
                        }
                        catch { }
                    }

                    _activeDatabases = null;
                }

                // Dispose of each of the temporary files (this will delete the files)
                if (_backupFileManager != null)
                {
                    _backupFileManager.Dispose();
                    _backupFileManager = null;
                }
            }

            // No unmanaged resources
        }

        #endregion IDisposable Members
    }

    /// <summary>
    /// Creates a FAM processing session for use in unit tests.
    /// <para><b>WARNING</b></para>
    /// When there are problems that occur within one of these sessions, the symptoms have at times
    /// been unintuitive. Rather than seeing a relevant exception, I have seen the session become
    /// deadlocked or license related errors.
    /// If a test fails or hangs that is using FAMProcessingSession, check the extract exception log,
    /// run the same setup with and actual FAM or attach a debugger to find the root cause.
    /// </summary>
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
        public FAMProcessingSession(IFileProcessingDB fileProcessingDB, string actionName, string workflowName,
            IFileProcessingTask fileProcessingTask, int threadCount = 1, int filesToGrab = 1,
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
                descriptor.Object = fileProcessingTask;

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
