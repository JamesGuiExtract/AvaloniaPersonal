using System;
using System.Globalization;
using System.IO;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database.Test
{
    /// Wrapper for a FileProcessingDB that takes care of initialization and cleanup
    [CLSCompliant(false)]
    public interface IDisposableDatabase<T> : IDisposable
    {
        FileProcessingDB FileProcessingDB { get; }
        FileProcessingDB[] Workflows { get; }
        int addFakeFile(int fileNumber, bool setAsSkipped, EFilePriority priority = EFilePriority.kPriorityNormal);
        string[] Actions { get; }
    }

    /// Base class for IDisposableDatabase implementations
    [CLSCompliant(false)]
    public abstract class DisposableDatabaseBase<T> : IDisposableDatabase<T>
    {
        public abstract FileProcessingDB FileProcessingDB { get; }
        public abstract FileProcessingDB[] Workflows { get; }
        public abstract FileProcessingDB[] Sessions { get; }
        public abstract string[] Actions { get; }

        protected private FAMTestDBManager<T> _testDBManager { get; set; }
        protected private string _dbName { get; set; }

        public virtual int addFakeFile(int fileNumber, bool setAsSkipped, EFilePriority priority = EFilePriority.kPriorityNormal)
        {
            var fileName = Path.Combine(Path.GetTempPath(), fileNumber.ToString("N3", CultureInfo.InvariantCulture) + ".tif");
            int fileID = FileProcessingDB.AddFileNoQueue(fileName, 0, 0, priority, -1);
            foreach (var wf in Workflows)
                foreach (var action in Actions)
                    if (setAsSkipped)
                        wf.SetFileStatusToSkipped(fileID, action, false, false);
                    else
                        wf.SetFileStatusToPending(fileID, action, false);

            return fileID;
        }

        public void Dispose()
        {
            foreach (var fpDB in Sessions)
            {
                if (fpDB != null)
                {
                    try
                    {
                        // Prevent 'files were reverted' log
                        foreach (string action in Actions)
                        {
                            fpDB.SetStatusForAllFiles(action, EActionStatus.kActionUnattempted);
                        }
                        fpDB.UnregisterActiveFAM();
                        fpDB.RecordFAMSessionStop();
                    }
                    catch { }
                }
            }
            _testDBManager.RemoveDatabase(_dbName);
        }
    }
}