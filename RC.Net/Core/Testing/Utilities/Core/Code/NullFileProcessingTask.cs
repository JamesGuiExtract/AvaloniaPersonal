using Extract.Interop;
using Extract.Utilities;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Testing.Utilities
{
    /// <summary>
    /// A IFileProcessingTask that can be used for <see cref="FAMProcessingSession"/> when a FAM session
    /// needs to be started, but it is irrelevant what happens in that session. Every file that starts
    /// processing in this task will simply wait for the file processing session to stop (wait for 
    /// Cancel to be called).
    /// </summary>
    [ComVisible(true)]
    [Guid("0B9C13D8-C8F8-48DD-9908-0C3C033EA251")]
    [ProgId("Extract.Testing.Utilities.NullFileProcessingTask")]
    [CLSCompliant(false)]
    public class NullFileProcessingTask : IFileProcessingTask, ICopyableObject, IPersistStream, ILicensedComponent, IDisposable
    {
        #region Fields

        string _id = "NullTask_" + Guid.NewGuid().ToString().Replace('-', '_');

        /// <summary>
        /// Enables ProcessFiles call to be cancelled when Cancel is called.
        /// </summary>
        CancellationTokenSource _cancelSource = new CancellationTokenSource();

        static object _lock = new();

        #endregion Fields

        #region Methods

        /// Triggers the specified file ID to be completed (if currently processing)
        public void CompleteFile(FileProcessingDB famDb, int fileId)
        {
            try
            {
                // ProcessFiles will monitor the value of this metadata field as a flag that it should
                // complete processing.
                famDb.SetMetadataFieldValue(fileId, _id, "C");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53374");
            }
        }

        #endregion Methods

        #region IFileProcessingTask

        /// <summary>
        /// Gets the minimum stack size needed for the thread in which this task is to be run.
        /// </summary>
        public uint MinStackSize => 0;

        /// <summary>
        /// Returns a value indicating that the task displays a ui
        /// </summary>
        public bool DisplaysUI => false;

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>  
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        /// <param name="pFileRequestHandler">The <see cref="IFileRequestHandler"/> that can be used
        /// by the task to carry out requests for files to be checked out, released or re-ordered
        /// in the queue.</param>
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB, IFileRequestHandler pFileRequestHandler)
        {
            try
            {
                lock (_lock)
                {
                    if (!pDB.GetMetadataFieldNames().ToIEnumerable<string>()
                        .Any(name => name == _id))
                    {
                        pDB.AddMetadataField(_id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI53375", "NullFileProcessingTask Init failed");
            }
        }

        /// <summary>
        /// Processes the specified file.
        /// </summary>
		/// <param name="pFileRecord">The file record that contains the info of the file being 
		/// processed.</param>
		/// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">A File Action Manager Tag Manager for expanding tags.</param>
        /// <param name="pDB">The File Action Manager database.</param>
        /// <param name="pProgressStatus">Object to provide progress status updates to caller.
        /// </param>
        /// <param name="bCancelRequested"><see langword="true"/> if cancel was requested; 
        /// <see langword="false"/> otherwise.</param>
        /// <returns><see langword="true"/> if processing should continue; <see langword="false"/> 
        /// if all file processing should be cancelled.</returns>
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord, int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB, ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Wait for the metadata field to be set to "C" or the the active FAM session to be stopped.
                string status;
                do
                {
                    status = pDB.GetMetadataFieldValue(pFileRecord.FileID, _id);
                    if (!string.IsNullOrEmpty(status))
                    {
                        break;
                    }

                    if (_cancelSource.Token.WaitHandle.WaitOne(100))
                    {
                        return EFileProcessingResult.kProcessingCancelled;
                    }
                }
                while(true);

                
                if (status == "C")
                {
                    return EFileProcessingResult.kProcessingSuccessful;
                }
                else
                {
                    throw new ExtractException("ELI53378", "Testing task failed condition");
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI49770", "NullFileProcessingTask processing failed");
            }
        }

        /// <summary>
        /// Stops processing the current file.
        /// </summary>
        public void Cancel()
        {
            try
            {
                _cancelSource?.Cancel();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI49771", "Failed to cancel NullFileProcessingTask");
            }
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            try
            {
                _cancelSource?.Dispose();
                _cancelSource = null;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI49773", "Failed to close NullFileProcessingTask");
            }
        }

        /// <summary>
        /// Called notify to the file processor that the pending document queue is empty, but
        ///	the processing tasks have been configured to remain running until the next document
        ///	has been supplied. If the processor will standby until the next file is supplied it
        ///	should return <see langword="true"/>. If the processor wants to cancel processing,
        ///	it should return <see langword="false"/>. If the processor does not immediately know
        ///	whether processing should be cancelled right away, it may block until it does know,
        ///	and return at that time.
        /// <para><b>Note</b></para>
        /// This call will be made on a different thread than the other calls, so the Standby call
        /// must be thread-safe. This allows the file processor to block on the Standby call, but
        /// it also means that call to <see cref="ProcessFile"/> or <see cref="Close"/> may come
        /// while the Standby call is still ocurring. If this happens, the return value of Standby
        /// will be ignored; however, Standby should promptly return in this case to avoid
        /// needlessly keeping a thread alive.
        /// </summary>
        /// <returns><see langword="true"/> to standby until the next file is supplied;
        /// <see langword="false"/> to cancel processing.</returns>
        public bool Standby()
        {
            return false;
        }

        #endregion IFileProcessingTask

        #region IAccessRequired Members

        /// <summary>
        /// Returns bool value indicating if the task requires admin access
        /// </summary>
        /// <returns><see langword="true"/> if the task requires admin access
        /// <see langword="false"/> if task does not require admin access</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion IAccessRequired Members

        #region ICopyableObject

        public object Clone()
        {
            var clone = new NullFileProcessingTask();
            clone.CopyFrom(this);

            return clone;
        }

        public void CopyFrom(object pObject)
        {
            _id = ((NullFileProcessingTask)pObject)._id;
        }

        #endregion ICopyableObject

        #region IPersistStream

        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        public int IsDirty()
        {
            return HResult.FromBoolean(false);
        }

        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, 1))
                {
                    _id = reader.ReadString();
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI53371", "Failed to load NullFileProcessingTask");
            }
        }

        public void Save(IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(1))
                {
                    writer.Write(_id);
                    
                    writer.WriteTo(stream);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI53373", "Failed to save NullFileProcessingTask");
            }
        }

        public void GetSizeMax(out long size)
        {
            size = HResult.NotImplemented;
        }

        #endregion IPersistStream

        #region ILicensedComponent

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return true;
        }

        #endregion ILicensedComponent

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="BetterLock"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="NullFileProcessingTask"/>. Also
        /// deletes the temporary file being managed by this class.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                _cancelSource?.Dispose();
                _cancelSource = null;
            }

            // Dispose of ummanaged resources
        }

        #endregion IDisposable Members
    }
}
