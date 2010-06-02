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
        /// Sets whether OCR text results should be sent to the clipboard.
        /// </summary>
        /// <param name="sendToClipboard">If <see langword="true"/> then OCR
        /// results will be sent to the clipboard.</param>
        public void SendOcrTextToClipboard(bool sendToClipboard)
        {
            try
            {
                _extractImageForm.SendOcrTextToClipboard = sendToClipboard;
            }
            catch (Exception ex)
            {
                throw new RemoteExtractException(
                    ExtractException.AsExtractException("ELI30156", ex));
            }
        }

        /// <summary>
        /// Sets the name of the file to send OCR results to. If <paramref name="fileName"/>
        /// is <see langword="null"/> or <see cref="String.Empty"/> then results will
        /// be sent to a messagebox.
        /// </summary>
        /// <param name="fileName">The file to send OCR results to.</param>
        public void SendOcrTextToFile(string fileName)
        {
            try
            {
                _extractImageForm.OcrTextFile = fileName;
            }
            catch (Exception ex)
            {
                throw new RemoteExtractException(
                    ExtractException.AsExtractException("ELI30157", ex));
            }
        }

        /// <summary>
        /// Executes the specified script file in the <see cref="ExtractImageViewerForm"/>.
        /// </summary>
        /// <param name="scriptFile">The script file to execute.</param>
        public void ExecuteScriptFile(string scriptFile)
        {
            try
            {
                _extractImageForm.ProcessScriptFile(scriptFile);
            }
            catch (Exception ex)
            {
                throw new RemoteExtractException(
                    ExtractException.AsExtractException("ELI30158", ex));
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
