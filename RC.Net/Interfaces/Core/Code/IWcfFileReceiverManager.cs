using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;

// This assembly is reserved for the definition of interfaces and helper classes for those
// interfaces. To ensure these interfaces are accessible from all projects without circular
// dependency issues and to allow the assemblies definitions to be used in both 32 and 64 bit code,
// This assembly should have no dependencies on any other Extract projects.
namespace Extract.Interfaces
{
    /// <summary>
    /// WCF Service contract interface for the exception logging service.
    /// </summary>
    [ServiceContract]
    public interface IWcfFileReceiverManager
    {
        /// <summary>
        /// Adds a new <see cref="FileReceiver"/>.
        /// </summary>
        /// <param name="menuDefinition">A <see cref="MenuDefinition"/> that defines a context menu
        /// item that is to supply files for this receiver.</param>
        /// <param name="fileFilter">A <see cref="FileFilter"/> to define which files are eligible
        /// to be received.</param>
        /// <returns>The id that has been assigned to the <see cref="FileReceiver"/> or -1 if the
        /// receiver was not successfully added.</returns>
        [OperationContract]
        int AddFileReceiver(MenuDefinition menuDefinition, FileFilter fileFilter);

        /// <summary>
        /// Removes the specified <see cref="FileReceiver"/>.
        /// </summary>
        /// <param name="fileReceiverId">The id of the <see cref="FileReceiver"/> to remove.</param>
        /// <returns>Any files received that have not yet been popped via
        /// <see cref="PopSuppliedFiles"/>.</returns>
        [OperationContract]
        IEnumerable<string> RemoveFileReceiver(int fileReceiverId);

        /// <summary>
        /// Gets the IDs of all active <see cref="FileReceiver"/>s.
        /// </summary>
        /// <returns>The IDs of all active <see cref="FileReceiver"/>s.</returns>
        [OperationContract]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        IEnumerable<int> GetFileReceiverIds();

        /// <summary>
        /// Gets the <see cref="MenuDefinition"/> for the specified <see cref="FileReceiver"/>.
        /// </summary>
        /// <param name="fileReceiverId">The ID of the <see cref="FileReceiver"/>.</param>
        /// <returns>The <see cref="MenuDefinition"/>.</returns>
        [OperationContract]
        MenuDefinition GetMenuDefinition(int fileReceiverId);

        /// <summary>
        /// Gets the <see cref="FileFilter"/> for the specified <see cref="FileReceiver"/>.
        /// </summary>
        /// <param name="fileReceiverId">The ID of the <see cref="FileReceiver"/>.</param>
        /// <returns>The <see cref="FileFilter"/>.</returns>
        [OperationContract]
        FileFilter GetFileFilter(int fileReceiverId);

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
        [OperationContract]
        bool SupplyFiles(int fileReceiverId, params string[] fileNames);

        /// <summary>
        /// Gets the files that have been supplied to the <see cref="FileReceiver"/> since the last
        /// call to this method.
        /// </summary>
        /// <param name="fileReceiverId">The ID of the <see cref="FileReceiver"/>.</param>
        /// <returns>The files that have been supplied since the last call to this method.</returns>
        [OperationContract]
        IEnumerable<string> PopSuppliedFiles(int fileReceiverId);
    }
}
