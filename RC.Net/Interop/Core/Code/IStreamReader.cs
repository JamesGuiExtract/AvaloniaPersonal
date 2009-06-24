using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Extract.Interop
{
    /// <summary>
    /// Represents a reader that reads data from an <see cref="IStream"/> object.
    /// </summary>
    public class IStreamReader : IDisposable
    {
        #region IStreamReader Constants

        static readonly string _OBJECT_NAME = typeof(IStreamReader).ToString();

        #endregion IStreamReader Constants

        #region IStreamReader Fields

        /// <summary>
        /// A <see cref="MemoryStream"/> into which all data is read.
        /// </summary>
        MemoryStream _stream;

        /// <summary>
        /// The formatter that is used from serialize data infrom <see cref="_stream"/>.
        /// </summary>
        readonly BinaryFormatter _formatter = new BinaryFormatter();

        /// <summary>
        /// The version of the <see cref="IStream"/> object being read.
        /// </summary>
        readonly int _version;

        #endregion IStreamReader Fields

        #region IStreamReader Constructors

        /// <summary>
	    /// Initializes a new instance of the <see cref="IStreamReader"/> class.
	    /// </summary>
        /// <param name="stream">The <see cref="IStream"/> from which data should be read.</param>
        /// <param name="version">The highest recognized version number.</param>
	    public IStreamReader(IStream stream, int version)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI26479",
                    _OBJECT_NAME);

                // Get the size of data stream to load
                byte[] dataLengthBuffer = new Byte[4];
                stream.Read(dataLengthBuffer, dataLengthBuffer.Length, IntPtr.Zero);
                int dataLength = BitConverter.ToInt32(dataLengthBuffer, 0);

                // Read the data from the provided stream into a buffer
                byte[] dataBuffer = new byte[dataLength];
                stream.Read(dataBuffer, dataLength, IntPtr.Zero);

                // Create a memory stream to deserialize the settings
                _stream = new MemoryStream(dataBuffer);
            
                // Read the version of the object being loaded.
                _version = (int)_formatter.Deserialize(_stream);

                // Validate the version
                ExtractException.Assert("ELI26185", "Unrecognized version number.",
                    _version <= version);
            }
            catch (Exception ex)
            {
                if (_stream != null)
                {
                    _stream.Dispose();
                }

                throw new ExtractException("ELI26488", "Unable to create stream reader.", ex);
            }
        }

        #endregion IStreamReader Constructors

        #region IStreamReader Properties

        /// <summary>
        /// Gets the version number of the <see cref="IStream"/> object being read.
        /// </summary>
        /// <returns>The version number of the <see cref="IStream"/> object being read.</returns>
        public int Version
        {
            get
            {
                return _version;
            }
        }

        #endregion IStreamReader Properties

        #region IStreamReader Methods

        /// <summary>
        /// Reads a string from the <see cref="IStream"/> object.
        /// </summary>
        public string ReadString()
        {
            try
            {
                // Check if the value is null
                bool hasValue = (bool)_formatter.Deserialize(_stream);
                if (!hasValue)
                {
                    return null;
                }

                return (string)_formatter.Deserialize(_stream);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26480",
                    "Unable to read string.", ex);
            }
        }

        /// <summary>
        /// Reads a <see cref="Boolean"/> from the <see cref="IStream"/> object.
        /// </summary>
        public bool ReadBoolean()
        {
            try
            {
                return (bool)_formatter.Deserialize(_stream);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26481",
                    "Unable to read boolean value.", ex);
            }
        }

        /// <summary>
        /// Reads a <see cref="Int32"/> from the <see cref="IStream"/> object.
        /// </summary>
        public int ReadInt32()
        {
            try
            {
                return (int)_formatter.Deserialize(_stream);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26482",
                    "Unable to read integer.", ex);
            }
        }

        #endregion IStreamReader Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="IStreamReader"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="IStreamReader"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> from release both managed and unmanaged 
        /// resources; <see langword="false"/> from release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}
