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
    /// Represents a writer that writes data to an <see cref="IStream"/> object.
    /// </summary>
    public class IStreamWriter : IDisposable
    {
        #region IStreamWriter Constants

        static readonly string _OBJECT_NAME = typeof(IStreamWriter).ToString();

        #endregion IStreamWriter Constants

        #region IStreamWriter Fields

        /// <summary>
        /// A <see cref="MemoryStream"/> to which all data is written before it is committed to
        /// the <see cref="IStream"/>
        /// </summary>
        MemoryStream _stream = new MemoryStream();

        /// <summary>
        /// The formatter that is used to serialize data into <see cref="_stream"/>.
        /// </summary>
        readonly BinaryFormatter _formatter = new BinaryFormatter(); 

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _OBJECT_NAME);

        #endregion IStreamWriter Fields

        #region IStreamWriter Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IStreamWriter"/> class.
        /// </summary>
        public IStreamWriter(int version)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI26483");

                _formatter.Serialize(_stream, version);
            }
            catch (Exception ex)
            {
                if (_stream != null)
                {
                    _stream.Dispose();
                }

                throw new ExtractException("ELI26597", "Unable to create stream writer.", ex);
            }
        }

        #endregion IStreamWriter Constructors

        #region IStreamWriter Methods

        /// <summary>
        /// Writes a string to the <see cref="IStream"/> object.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public void Write(string value)
        {
            try
            {
                // First stream whether the value is null
                bool hasValue = value != null;
                _formatter.Serialize(_stream, hasValue);
                if (hasValue)
                {
                    // The value is not null, stream the value itself
                    _formatter.Serialize(_stream, value);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26484", 
                    "Unable to write string.", ex);
                ee.AddDebugData("Value", value, false);
                throw ee;
            }
        }

        /// <summary>
        /// Writes a <see cref="Boolean"/> to the <see cref="IStream"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Boolean"/> to write.</param>
        public void Write(bool value)
        {
            try
            {
                _formatter.Serialize(_stream, value);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26485",
                    "Unable to write boolean value.", ex);
                ee.AddDebugData("Value", value, false);
                throw ee;
            }
        }

        /// <summary>
        /// Writes a <see cref="Int32"/> to the <see cref="IStream"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> to write.</param>
        public void Write(int value)
        {
            try
            {
                _formatter.Serialize(_stream, value);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26486",
                    "Unable to write integer.", ex);
                ee.AddDebugData("Value", value, false);
                throw ee;
            }
        }

        /// <summary>
        /// Writes the contents of the <see cref="IStreamWriter"/> to the specified 
        /// <see cref="IStream"/>.
        /// </summary>
        /// <param name="stream">The stream into which the data should be written.</param>
        public void WriteTo(IStream stream)
        {
            try
            {
                byte[] dataBuffer = _stream.ToArray();
                byte[] dataLengthBuffer = BitConverter.GetBytes(dataBuffer.Length);
                stream.Write(dataLengthBuffer, dataLengthBuffer.Length, IntPtr.Zero);
                stream.Write(dataBuffer, dataBuffer.Length, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26487",
                    "Unable to write to IStream object.", ex);
            }
        }

        #endregion IStreamWriter Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="IStreamWriter"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="IStreamWriter"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="IStreamWriter"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
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
