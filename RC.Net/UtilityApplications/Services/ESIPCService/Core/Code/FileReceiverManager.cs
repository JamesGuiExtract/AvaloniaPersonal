using Extract.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

namespace Extract.UtilityApplications.Services
{
    /// <summary>
    /// Manages <see cref="FileReceiver"/> instances that allow files to be received via WCF by one
    /// process from another process.
    /// </summary>
    // InstanceContextMode needs to be Single, otherwise a process won't get the same instance of
    // this manager that another used to register a FileReceiver.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class FileReceiverManager : IWcfFileReceiverManager
    {
        #region Fields

        /// <summary>
        /// Maps IDs to each active <see cref="FileReceiver"/>.
        /// </summary>
        ConcurrentDictionary<int, FileReceiver> _fileReceivers =
            new ConcurrentDictionary<int, FileReceiver>();

        /// <summary>
        /// The ID to assign to the next FileReceiver that is added.
        /// </summary>
        int _nextID = 0;

        #endregion Fields

        /// <summary>
        /// Initializes a new instance of the <see cref="FileReceiverManager"/> class.
        /// </summary>
        public FileReceiverManager()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33133");
            }
        }

        /// <summary>
        /// Adds a new <see cref="FileReceiver"/>.
        /// </summary>
        /// <param name="menuDefinition">A <see cref="MenuDefinition"/> that defines a context menu
        /// item that is to supply files for this receiver.</param>
        /// <param name="fileFilter">A <see cref="FileFilter"/> to define which files are eligible
        /// to be received.</param>
        /// <returns>The id that has been assigned to the <see cref="FileReceiver"/> or -1 if the
        /// receiver was not successfully added.</returns>
        public int AddFileReceiver(MenuDefinition menuDefinition, FileFilter fileFilter)
        {
            try
            {
                int id = Interlocked.Increment(ref _nextID);
                menuDefinition.FileReceiverId = id;
                FileReceiver fileReceiver =
                    new FileReceiver(menuDefinition, fileFilter, OperationContext.Current.Channel);
                _fileReceivers[id] = fileReceiver;

                return id;
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI33134");
                return -1;
            }
        }

        /// <summary>
        /// Removes the specified <see cref="FileReceiver"/>.
        /// </summary>
        /// <param name="fileReceiverId">The id of the <see cref="FileReceiver"/> to remove.</param>
        /// <returns>Any files received that have not yet been popped via
        /// <see cref="PopSuppliedFiles"/>.</returns>
        public IEnumerable<string> RemoveFileReceiver(int fileReceiverId)
        {
            try
            {
                FileReceiver fileReceiver;
                if (_fileReceivers.TryRemove(fileReceiverId, out fileReceiver))
                {
                    IEnumerable<string> poppedFiles = fileReceiver.PopSuppliedFiles();

                    return poppedFiles;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI33135");
            }

            return null;
        }

        /// <summary>
        /// Gets the IDs of all active <see cref="FileReceiver"/>s.
        /// </summary>
        /// <returns>The IDs of all active <see cref="FileReceiver"/>s.</returns>
        public IEnumerable<int> GetFileReceiverIds()
        {
            try
            {
                return _fileReceivers.Keys;
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI33136");
                return null;
            }
        }

        /// <summary>
        /// Gets the <see cref="MenuDefinition"/> for the specified <see cref="FileReceiver"/>.
        /// </summary>
        /// <param name="fileReceiverId">The ID of the <see cref="FileReceiver"/>.</param>
        /// <returns>The <see cref="MenuDefinition"/>.</returns>
        public MenuDefinition GetMenuDefinition(int fileReceiverId)
        {
            try
            {
                FileReceiver fileReceiver = GetFileReceiver(fileReceiverId);

                return (fileReceiver == null) ? null : fileReceiver.MenuDefinition;
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI33138");
                return null;
            }
        }

        /// <summary>
        /// Gets the <see cref="FileFilter"/> for the specified <see cref="FileReceiver"/>.
        /// </summary>
        /// <param name="fileReceiverId">The ID of the <see cref="FileReceiver"/>.</param>
        /// <returns>The <see cref="FileFilter"/>.</returns>
        public FileFilter GetFileFilter(int fileReceiverId)
        {
            try
            {
                FileReceiver fileReceiver = GetFileReceiver(fileReceiverId);

                return (fileReceiver == null) ? null : fileReceiver.FileFilter;
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI33137");
                return null;
            }
        }

        /// <summary>
        /// Supplies files to the specified <see cref="FileReceiver"/>.
        /// <para><b>Note</b></para>
        /// Since the ESIPCService may not have the same network access the process supplying the
        /// files, it is up to the supplying process to use FileFilter.FileMatchesFilter to
        /// eliminate files that should not be supplied prior to calling this method. The method
        /// will not filter the supplied list (though the creator or the <see cref="FileReceiver"/>
        /// may).
        /// </summary>
        /// <param name="fileReceiverId">The ID of the <see cref="FileReceiver"/>.</param>
        /// <param name="fileNames">The files to supply.</param>
        /// <returns><see langword="true"/> if the files were supplied, <see langword="false"/>
        /// otherwise.</returns>
        public bool SupplyFiles(int fileReceiverId, params string[] fileNames)
        {
            try
            {
                FileReceiver fileReceiver = GetFileReceiver(fileReceiverId);

                if (fileReceiver != null)
                {
                    fileReceiver.SupplyFiles(fileNames);
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI33139");
            }

            return false;
        }

        /// <summary>
        /// Gets the files that have been supplied to the <see cref="FileReceiver"/> since the last
        /// call to this method.
        /// </summary>
        /// <param name="fileReceiverId">The ID of the <see cref="FileReceiver"/>.</param>
        /// <returns>The files that have been supplied since the last call to this method.</returns>
        public IEnumerable<string> PopSuppliedFiles(int fileReceiverId)
        {
            try
            {
                FileReceiver fileReceiver = GetFileReceiver(fileReceiverId);

                return (fileReceiver == null) ? null : fileReceiver.PopSuppliedFiles();
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI33140");
                return null;
            }
        }

        #region Private Members

        /// <summary>
        /// Gets the <see cref="FileReceiver"/> associated with the specified
        /// <see paramref="fileReceiverId"/> or <see langword="null"/> if there is no active
        /// <see cref="FileReceiver"/> with that ID. If there is one, but the
        /// <see cref="IContextChannel"/> it was created on is no longer open, the
        /// <see cref="FileReceiver"/> is removed and <see langword="null"/> is returned.
        /// </summary>
        /// <param name="fileReceiverId">The ID of the <see cref="FileReceiver"/>.</param>
        /// <returns>The <see cref="FileReceiver"/> associated with the specified
        /// <see paramref="fileReceiverId"/> or <see langword="null"/> if there is no active
        /// <see cref="FileReceiver"/> with that ID.</returns>
        FileReceiver GetFileReceiver(int fileReceiverId)
        {
            FileReceiver fileReceiver;

            if (_fileReceivers.TryGetValue(fileReceiverId, out fileReceiver))
            {
                if (fileReceiver.HostChannel.State == CommunicationState.Opened)
                {
                    return fileReceiver;
                }

                RemoveFileReceiver(fileReceiverId);
            }

            return null;
        }

        #endregion Private Members
    }
}
