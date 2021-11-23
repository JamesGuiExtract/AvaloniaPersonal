﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using UCLID_FILEPROCESSINGLib;
using AttributeDbMgrComponentsLib;
using UCLID_COMUTILSLib;

namespace Extract.FileActionManager.Database.Test
{
    /// Wrapper for a FileProcessingDB that takes care of initialization and cleanup
    [CLSCompliant(false)]
    public interface IDisposableDatabase<T> : IDisposable
    {
        FileProcessingDB FileProcessingDB { get; }
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        FileProcessingDB[] Workflows { get; }
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        string[] Actions { get; }
        [SuppressMessage("Microsoft.Naming", "CA1709")]
        int addFakeFile(int fileNumber, bool setAsSkipped, EFilePriority priority = EFilePriority.kPriorityNormal);
        void AddFakeVOA(int fileNumber, string attributeSetName);
        void CreateAttributeSet(string attributeSetName);
    }

    /// Base class for IDisposableDatabase implementations
    [CLSCompliant(false)]
    public abstract class DisposableDatabaseBase<T> : IDisposableDatabase<T>
    {
        private bool disposedValue;

        public abstract FileProcessingDB FileProcessingDB { get; }
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public abstract FileProcessingDB[] Workflows { get; }
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public abstract FileProcessingDB[] Sessions { get; }
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public abstract string[] Actions { get; }

        protected private FAMTestDBManager<T> _testDBManager { get; set; }
        protected private string _dbName { get; set; }
        private AttributeDBMgrClass _attributeDatabase; 

        public int addFakeFile(int fileNumber, bool setAsSkipped, EFilePriority priority = EFilePriority.kPriorityNormal)
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

        public void AddFakeVOA(int fileNumber, string attributeSetName)
        {
            createAttributeDatabase();
            int sessionID = FileProcessingDB.StartFileTaskSession("B25D64C0-6FF6-4E0B-83D4-0D5DFEB68006", fileNumber, 1);
            try
            {
                _attributeDatabase.CreateNewAttributeSetForFile(sessionID, attributeSetName, new IUnknownVectorClass(), false, false, false, false);
            }
            finally
            {
                FileProcessingDB.EndFileTaskSession(sessionID, 0, 0, false);
            }
        }
        public void CreateAttributeSet(string attributeSetName)
        {
            createAttributeDatabase();
            _attributeDatabase.CreateNewAttributeSetName(attributeSetName);

        }

        private void createAttributeDatabase()
        {
            if (_attributeDatabase == null)
            {
                _attributeDatabase = new();
                _attributeDatabase.FAMDB = FileProcessingDB;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
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

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}