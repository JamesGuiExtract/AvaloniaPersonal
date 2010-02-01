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
    /// Represents a reader that reads data from an <see cref="IStream"/> object.
    /// </summary>
    public class IStreamReader : IDisposable
    {
        #region Constants

        static readonly string _OBJECT_NAME = typeof(IStreamReader).ToString();

        #endregion Constants

        #region Fields

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

        #endregion Fields

        #region Constructors

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

        #endregion Constructors

        #region Properties

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

        #endregion Properties

        #region Methods

        /// <summary>
        /// Reads a {T} from the <see cref="IStream"/> object.
        /// </summary>
        /// <typeparam name="T">The type of object to read from the stream.</typeparam>
        /// <returns>The serialized object or <see langword="null"/> if the original
        /// streamed object was <see langword="null"/>.</returns>
        // In order to ensure type safety it is acceptable to require the user to declare the
        // type for this method.  An alternative design would be to add an out parameter and
        // have this method not return any objects, but this would generate another
        // FxCop warning about avoiding out parameters. This syntax is cleaner and easier
        // to use than a method with out parameters and so this message is being suppressed.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public T ReadObject<T>() where T : class, ISerializable
        {
            try
            {
                bool hasValue = (bool)_formatter.Deserialize(_stream);
                if (!hasValue)
                {
                    return null;
                }
                return (T)_formatter.Deserialize(_stream);
            }
            catch (Exception ex)
            {
                ExtractException ee =  new ExtractException("ELI27871", "Unable to read object.", ex);
                ee.AddDebugData("Object Type", typeof(T).ToString(), false);
                throw ee;
            }
        }

        /// <summary>
        /// Reads a <see cref="String"/> from the <see cref="IStream"/> object.
        /// </summary>
        /// <returns>A <see cref="String"/>.</returns>
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
        /// Reads an array of strings from the <see cref="IStream"/> object.
        /// </summary>
        /// <returns>An array of strings.</returns>
        public string[] ReadStringArray()
        {
            try
            {
                // Check if the value is null
                bool hasValue = (bool)_formatter.Deserialize(_stream);
                if (!hasValue)
                {
                    return null;
                }

                // Get the number of items in the array
                int count = (int)_formatter.Deserialize(_stream);
                
                // Read each string
                string[] result = new string[count];
                for (int i = 0; i < count; i++)
                {
                    result[i] = ReadString();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29520",
                    "Unable to read string array.", ex);
            }
        }

        /// <summary>
        /// Reads a <see cref="Boolean"/> from the <see cref="IStream"/> object.
        /// </summary>
        /// <returns>A <see cref="Boolean"/>.</returns>
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
        /// <returns>A <see cref="Int32"/>.</returns>
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

        /// <summary>
        /// Reads a <see cref="Int64"/> from the <see cref="IStream"/> object.
        /// </summary>
        /// <returns>A <see cref="Int64"/>.</returns>
        public long ReadInt64()
        {
            try
            {
                return (long)_formatter.Deserialize(_stream);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27872", "Unable to read long integer.", ex);
            }
        }

        /// <summary>
        /// Reads a <see cref="Single"/> from the <see cref="IStream"/> object.
        /// </summary>
        /// <returns>A <see cref="Single"/>.</returns>
        public float ReadSingle()
        {
            try
            {
                return (float)_formatter.Deserialize(_stream);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27873", "Unable to read float.", ex);
            }
        }

        /// <summary>
        /// Reads a <see cref="Double"/> from the <see cref="IStream"/> object.
        /// </summary>
        /// <returns>A <see cref="Double"/>.</returns>
        public double ReadDouble()
        {
            try
            {
                return (double)_formatter.Deserialize(_stream);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27874", "Unable to read double.", ex);
            }
        }

        #endregion Methods

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
