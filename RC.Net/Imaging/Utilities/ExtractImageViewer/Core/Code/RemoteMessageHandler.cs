using Extract.Imaging.Forms;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Security.Permissions;
using System.Text;

namespace Extract.Imaging.Utilities.ExtractImageViewer
{
    /// <summary>
    /// A .Net remoting class used to allow other objects to interact with the
    /// <see cref="ExtractImageViewerForm"/>.
    /// </summary>
    internal class RemoteMessageHandler : MarshalByRefObject, IDisposable
    {
        #region Constants

        /// <summary>
        /// The base object uri used to access this remote object.
        /// </summary>
        static readonly string _REMOTING_NAME = "63778670-AA87-4D31-A441-CA1005ABACD3";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The image viewer that this 
        /// </summary>
        ExtractImageViewerForm _extractImageForm;

        /// <summary>
        /// Whether this object has been disposed or not.
        /// </summary>
        bool _disposed;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteMessageHandler"/> class.
        /// </summary>
        public RemoteMessageHandler(ExtractImageViewerForm form)
        {
            try
            {
                _extractImageForm = form;
                RemotingServices.Marshal(this, _REMOTING_NAME);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30145", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the <see cref="ExtractImageViewerForm"/> associated with this object.
        /// </summary>
        internal ExtractImageViewerForm ExtractImageViewer
        {
            get
            {
                return _extractImageForm;
            }
            set
            {
                _extractImageForm = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Overridden to indicate that this remote object does not expire.
        /// </summary>
        /// <returns><see langword="null"/>.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)] 
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Opens the specified image file in the <see cref="ExtractImageViewer"/>.
        /// </summary>
        /// <param name="fileName">The image file to open.</param>
        public void OpenImage(string fileName)
        {
            try
            {
                _extractImageForm.ImageViewer.OpenImage(fileName, true);
            }
            catch (Exception ex)
            {
                throw new RemoteExtractException(
                    ExtractException.AsExtractException("ELI30137", ex));
            }
        }

        /// <summary>
        /// Builds the remote object uri for the <see cref="RemoteMessageHandler"/> for the
        /// specified process ID.
        /// </summary>
        /// <param name="processId">The process ID to build the remote name for.</param>
        /// <returns>The remote object uri.</returns>
        static internal string BuildRemoteObjectUri(int processId)
        {
            StringBuilder sb = new StringBuilder("ipc://");
            sb.Append(ExtractImageViewerForm.BuildExtractImageViewerUri(processId));
            sb.Append("/");
            sb.Append(_REMOTING_NAME);
            return sb.ToString();
        }

        #endregion Methods

        #region IDisposable Members

        /// <overloads>
        /// Releases resources used by the <see cref="RemoteMessageHandler"/>
        /// </overloads>
        /// <summary>
        /// Releases resources used by the <see cref="RemoteMessageHandler"/>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by the <see cref="RemoteMessageHandler"/>
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Set the image form back to null
                _extractImageForm = null;

                // If not disposed yet, then disconnect this object from the
                // remoting service
                if (!_disposed)
                {
                    RemotingServices.Disconnect(this);
                    _disposed = true;
                }
            }
        }

        #endregion
    }
}
