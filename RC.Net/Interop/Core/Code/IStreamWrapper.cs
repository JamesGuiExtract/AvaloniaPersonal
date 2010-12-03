/* IStreamWrapper.cs
 * An OLE IStream wrapper implementation around native .NET Streams
 *
 * Added by Steve Kurth 11/09/2010
 * Based on:
 * --------------------------------------------------------------------
 * Based on work by Oliver Sturm (see remarks on class IStreamWrapper). I
 * basically just adapted this to work on .NET Compact Framework 2.0 and
 * added the requisite support constructs.
 *
 * Distributed with no license what-so-ever - feel free to do
 * whatever the hell you want with this :-)  I would appreciate it
 * if you dropped me a line to let me know if you've found this
 * useful though.
 *
 * Have fun!
 *
 * Tomer Gabel (http://www.tomergabel.com)
 * Monfort Software Engineering Ltd. (http://www.monfort.co.il)
 * E-mail me at tomer@tomergabel.com
 *
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices.ComTypes;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using Extract.Licensing;

namespace Extract.Interop
{
    /// <summary>
    /// Wraps <see cref="System.IO.Stream"/> with a COM IStream implementation.
    /// <para><b>Warining</b></para>
    /// This class has not been fully implemented. Only just enough has been implemented so that
    /// UCLID_COMUtils::writeObjectToStream and UCLID_COMUtils::readObjectFromStream will work.
    /// </summary>
    /// <remarks>Based on work by <a href="http://www.sturmnet.org/blog/archives/2005/03/03/cds-csharp-extractor/">
    /// Oliver Sturm</a></remarks>
    [ComVisible(true)]
    [Guid("EB6EE2AB-5FE7-4467-AD4F-52656F8E1854")]
    [ClassInterface(ClassInterfaceType.None)]
    public sealed class IStreamWrapper : IStream
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(IStreamWrapper).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="Stream"/> being wrapped as an <see cref="IStream"/> implmentation.
        /// </summary>
        Stream _stream;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IStreamWrapper"/> class.
        /// </summary>
        public IStreamWrapper()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI31108",
                    _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31109", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IStreamWrapper"/> class.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to wrap.</param>
        public IStreamWrapper(Stream stream)
            :this()
        {
            _stream = stream;
        }

        #endregion Constructors

        #region IStream members

        /// <summary>
        /// This method has not been implemented.
        /// </summary>
        /// <param name="ppstm">Unused.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "ppstm")]
        void IStream.Clone(out IStream ppstm)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method has not been implemented.
        /// </summary>
        /// <param name="grfCommitFlags">Unused.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "grfCommitFlags")]
        void IStream.Commit(int grfCommitFlags)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method has not been implemented.
        /// </summary>
        /// <param name="pstm">Unused.</param>
        /// <param name="cb">Unused.</param>
        /// <param name="pcbRead">Unused.</param>
        /// <param name="pcbWritten">Unused.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pstm")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cb")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pcbRead")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pcbWritten")]
        void IStream.CopyTo(IStream pstm, long cb, System.IntPtr pcbRead, System.IntPtr pcbWritten)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method has not been implemented.
        /// </summary>
        /// <param name="libOffset">Unused.</param>
        /// <param name="cb">Unused.</param>
        /// <param name="dwLockType">Unused.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "libOffset")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cb")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dwLockType")]
        void IStream.LockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a specified number of bytes from the stream object into memory starting at the
        /// current seek pointer.
        /// </summary>
        /// <param name="pv">When this method returns, contains the data read from the stream. This
        /// parameter is passed uninitialized.</param>
        /// <param name="cb">The number of bytes to read from the stream object.</param>
        /// <param name="pcbRead">A pointer to a ULONG variable that receives the actual number of
        /// bytes read from the stream object.</param>
        void IStream.Read(byte[] pv, int cb, System.IntPtr pcbRead)
        {
            try
            {
                int totalBytesRead;
                int bytesRead;

                for (totalBytesRead = 0; cb > 0; totalBytesRead += bytesRead)
                {
                    // Read the next chunk of bytes
                    bytesRead = _stream.Read(pv, totalBytesRead, cb);
                    if (bytesRead > 0)
                    {
                        cb -= bytesRead;
                    }
                    else
                    {
                        break;
                    }
                }

                if (pcbRead != IntPtr.Zero)
                {
                    Marshal.WriteInt32(pcbRead, totalBytesRead);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31104", ex);
            }
        }

        /// <summary>
        /// This method has not been implemented.
        /// </summary>
        void IStream.Revert()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Changes the seek pointer to a new location relative to the beginning of the stream, to
        /// the end of the stream, or to the current seek pointer.
        /// </summary>
        /// <param name="dlibMove">The displacement to add to <paramref name="dwOrigin"/>.</param>
        /// <param name="dwOrigin">The origin of the seek. The origin can be the beginning of the
        /// file, the current seek pointer, or the end of the file.</param>
        /// <param name="plibNewPosition">On successful return, contains the offset of the seek
        /// pointer from the beginning of the stream.</param>
        void IStream.Seek(long dlibMove, int dwOrigin, System.IntPtr plibNewPosition)
        {
            try
            {
                Marshal.WriteInt32(
                    plibNewPosition, (int)_stream.Seek(dlibMove, (SeekOrigin)dwOrigin));
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31105", ex);
            }
        }

        /// <summary>
        /// This method has not been implemented.
        /// </summary>
        /// <param name="libNewSize">Unused.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "libNewSize")]      
        void IStream.SetSize(long libNewSize)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the <see cref="T:System.Runtime.InteropServices.STATSTG"/> structure for this
        /// stream.
        /// </summary>
        /// <param name="pstatstg">When this method returns, contains a STATSTG structure that
        /// describes this stream object. This parameter is passed uninitialized.</param>
        /// <param name="grfStatFlag">Members in the STATSTG structure that this method does not
        /// return, thus saving some memory allocation operations.</param>
        void IStream.Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg,
            int grfStatFlag)
        {
            try
            {
                pstatstg = new System.Runtime.InteropServices.ComTypes.STATSTG();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31107", ex);
            }
        }

        /// <summary>
        /// This method has not been implemented.
        /// </summary>
        /// <param name="libOffset">Unused.</param>
        /// <param name="cb">Unused.</param>
        /// <param name="dwLockType">Unused.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "libOffset")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cb")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dwLockType")]
        void IStream.UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a specified number of bytes into the stream object starting at the current seek
        /// pointer.
        /// </summary>
        /// <param name="pv">The buffer to write this stream to.</param>
        /// <param name="cb">The number of bytes to write to the stream.</param>
        /// <param name="pcbWritten">On successful return, contains the actual number of bytes
        /// written to the stream object. If the caller sets this pointer to
        /// <see cref="F:System.IntPtr.Zero"/>, this method does not provide the actual number of
        /// bytes written.</param>
        void IStream.Write(byte[] pv, int cb, System.IntPtr pcbWritten)
        {
            try
            {
                _stream.Write(pv, 0, cb);
                _stream.Flush();

                if (pcbWritten != IntPtr.Zero)
                {
                    Marshal.WriteInt32(pcbWritten, cb);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31106", ex);
            }
        }

        #endregion IStream members
    }
}
