using System.Collections.Concurrent;
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
    /// 
    /// </summary>
    public class FileReceiver
    {
        /// <summary>
        /// Constant for the endpoint of the TCP/IP channel for the service.
        /// </summary>
        public static readonly string WcfAddress = "net.pipe://localhost/ESFileReceiver";

        /// <summary>
        /// A <see cref="MenuDefinition"/> that defines a context menu item that is to supply files
        /// for this receiver.
        /// </summary>
        MenuDefinition _menuDefinition;

        /// <summary>
        /// A <see cref="FileFilter"/> to define which files are eligible to be received.
        /// </summary>
        FileFilter _fileFilter;

        /// <summary>
        /// The <see cref="IContextChannel"/> from which this receiver was added.
        /// </summary>
        IContextChannel _hostChannel;

        /// <summary>
        /// The files that have been received and are waiting to be popped.
        /// </summary>
        ConcurrentQueue<string> _suppliedFiles = new ConcurrentQueue<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileReceiver"/> class.
        /// </summary>
        /// <param name="menuDefinition">A <see cref="MenuDefinition"/> that defines a context menu
        /// item that is to supply files for this receiver.</param>
        /// <param name="fileFilter">A <see cref="FileFilter"/> to define which files are eligible
        /// to be received.</param>
        /// <param name="hostChannel">The <see cref="IContextChannel"/> from which this receiver was
        /// added.</param>
        public FileReceiver(MenuDefinition menuDefinition, FileFilter fileFilter,
            IContextChannel hostChannel)
        {
            _menuDefinition = menuDefinition;
            _fileFilter = fileFilter;
            _hostChannel = hostChannel;
        }

        /// <summary>
        /// A <see cref="MenuDefinition"/> that defines a context menu item that is to supply files
        /// for this receiver.
        /// </summary>
        public MenuDefinition MenuDefinition
        {
            get
            {
                return _menuDefinition;
            }
        }

        /// <summary>
        /// A <see cref="FileFilter"/> to define which files are eligible to be received.
        /// </summary>
        public FileFilter FileFilter
        {
            get
            {
                return _fileFilter;
            }
        }

        /// <summary>
        /// The <see cref="IContextChannel"/> from which this receiver was added.
        /// </summary>
        public IContextChannel HostChannel
        {
            get
            {
                return _hostChannel;
            }
        }

        /// <summary>
        /// Supplies files to the this instance.
        /// <para><b>Note</b></para>
        /// Since the ESIPCService may not have the same network access the process supplying the
        /// files, it is up to the supplying process to use FileFilter.FileMatchesFilter to
        /// eliminate files that should not be supplied prior to calling this method. The method
        /// will not filter the supplied list (though the creator or the <see cref="FileReceiver"/>
        /// may).
        /// </summary>
        /// <param name="fileNames">The files to supply.</param>
        // This assembly and class do not have access to the ExtractException class and therefore
        // we can't really do anything about the exception if we caught it.
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public void SupplyFiles(params string[] fileNames)
        {
            foreach(string fileName in fileNames)
            {
                _suppliedFiles.Enqueue(fileName);
            }
        }

        /// <summary>
        /// Gets the files that have been supplied to this instance since the last call to this
        /// method.
        /// </summary>
        /// <returns>The files that have been supplied since the last call to this method.</returns>
        // This assembly and class do not have access to the ExtractException class and therefore
        // we can't really do anything about the exception if we caught it.
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public IEnumerable<string> PopSuppliedFiles()
        {
            string suppliedFile;
            while (_suppliedFiles.TryDequeue(out suppliedFile))
            {
                yield return suppliedFile;
            }
        }
    }
}
