using Extract.Licensing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Extract.Interop
{
    /// <summary>
    /// Represents a writer that writes data to an <see cref="IStream"/> object.
    /// </summary>
    public class IStreamWriter : IDisposable
    {
        #region Constants

        static readonly string _OBJECT_NAME = typeof(IStreamWriter).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// A <see cref="MemoryStream"/> to which all data is written before it is committed to
        /// the <see cref="IStream"/>
        /// </summary>
        MemoryStream _stream = new MemoryStream();

        /// <summary>
        /// The formatter that is used to serialize data into <see cref="_stream"/>.
        /// </summary>
        readonly BinaryFormatter _formatter = new BinaryFormatter(); 

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IStreamWriter"/> class.
        /// </summary>
        public IStreamWriter(int version)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI26483",
					_OBJECT_NAME);

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

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Writes a {T} to the stream.
        /// </summary>
        /// <typeparam name="T">The type of object to write to the stream.</typeparam>
        /// <param name="value">The object to write to the stream.</param>
        public void WriteObject<T>(T value) where T : class, ISerializable
        {
            try
            {
                bool hasValue = value != null;
                _formatter.Serialize(_stream, hasValue);

                if (hasValue)
                {
                    _formatter.Serialize(_stream, value);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27875",
                    "Unable to write object.", ex);
                ee.AddDebugData("Value", value == null ? "Null" : value.ToString(), false);
                throw ee;
            }
        }

        /// <summary>
        /// Writes a {T} to the stream.
        /// </summary>
        /// <typeparam name="T">The type of struct to write to the stream.</typeparam>
        /// <param name="value">The struct to write to the stream.</param>
        public void WriteStruct<T>(T value) where T : struct, ISerializable
        {
            try
            {
                _formatter.Serialize(_stream, value);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI32776", "Unable to write struct.", ex);
                ee.AddDebugData("Value", value.ToString(), false);
                throw ee;
            }
        }

        /// <summary>
        /// Writes a <see cref="String"/> to the <see cref="IStream"/> object.
        /// </summary>
        /// <param name="value">The <see cref="String"/> to write.</param>
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
        /// Writes an array of strings to the <see cref="IStream"/> object.
        /// </summary>
        /// <param name="value">The array of strings to write.</param>
        public void Write(string[] value)
        {
            try
            {
                // First stream whether the value is null
                bool hasValue = value != null;
                _formatter.Serialize(_stream, hasValue);
                if (hasValue)
                {
                    // Stream the number of strings in the array
                    _formatter.Serialize(_stream, value.Length);

                    // Stream each string
                    for (int i = 0; i < value.Length; i++)
                    {
                        Write(value[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29521",
                    "Unable to write string array.", ex);
            }
        }

        /// <summary>
        /// Writes an array of boolean to the <see cref="IStream"/> object.
        /// </summary>
        /// <param name="value">The array of booleans to write.</param>
        public void Write(bool[] value)
        {
            try
            {
                // First stream whether the value is null
                bool hasValue = value != null;
                _formatter.Serialize(_stream, hasValue);
                if (hasValue)
                {
                    // Stream the number of booleans in the array
                    _formatter.Serialize(_stream, value.Length);

                    // Stream each boolean
                    for (int i = 0; i < value.Length; i++)
                    {
                        Write(value[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI33874",
                    "Unable to write boolean array.", ex);
            }
        }

        /// <summary>
        /// Writes an array of <typeparamref name="T"/> to the <see cref="IStream"/> object.
        /// </summary>
        /// <param name="value">The array of <typeparamref name="T"/> to write.</param>
        public void Write<T>(T[] value) where T : struct, ISerializable
        {
            try
            {
                // First stream whether the value is null
                bool hasValue = value != null;
                _formatter.Serialize(_stream, hasValue);
                if (hasValue)
                {
                    // Stream the number of structs in the array
                    _formatter.Serialize(_stream, value.Length);

                    // Stream each struct
                    for (int i = 0; i < value.Length; i++)
                    {
                        WriteStruct(value[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI33991",
                    "Unable to write struct array.", ex);
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
        /// Writes a <see cref="Int64"/> to the <see cref="IStream"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Int64"/> to write.</param>
        public void Write(long value)
        {
            try
            {
                _formatter.Serialize(_stream, value);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27876",
                    "Unable to write long integer.", ex);
                ee.AddDebugData("Value", value, false);
                throw ee;
            }
        }

        /// <summary>
        /// Writes a <see cref="Single"/> to the <see cref="IStream"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Single"/> to write.</param>
        public void Write(float value)
        {
            try
            {
                _formatter.Serialize(_stream, value);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27877",
                    "Unable to write float.", ex);
                ee.AddDebugData("Value", value, false);
                throw ee;
            }
        }

        /// <summary>
        /// Writes a <see cref="Double"/> to the <see cref="IStream"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> to write.</param>
        public void Write(double value)
        {
            try
            {
                _formatter.Serialize(_stream, value);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27878",
                    "Unable to write double.", ex);
                ee.AddDebugData("Value", value, false);
                throw ee;
            }
        }

        /// <summary>
        /// Writes the specified persist <see cref="IPersistStream"/> instance to the stream.
        /// </summary>
        /// <param name="persistStreamObject">The <see cref="IPersistStream"/> to write.</param>
        /// <param name="clearDirtyFlag"><see langword="true"/> to the clear dirty flag, 
        /// <see langword="false"/> otherwise.</param>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
        public void Write(IPersistStream persistStreamObject, bool clearDirtyFlag)
        {
            try
            {
                Guid classID;
                persistStreamObject.GetClassID(out classID);
                _formatter.Serialize(_stream, classID);
                persistStreamObject.Save(new IStreamWrapper(_stream), clearDirtyFlag);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31051", ex);
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

        #endregion Methods

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
