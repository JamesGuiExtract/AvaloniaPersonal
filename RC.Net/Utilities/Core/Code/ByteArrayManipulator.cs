using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extract.Utilities
{
    /// <summary>
    /// Provides ability to serialize objects of various data types into an array of bytes.
    /// </summary>
    public class ByteArrayManipulator
    {
        #region Constants

        /// <summary>
        /// The object name used in licensing calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ByteArrayManipulator).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// A list of bytes in the stream.
        /// </summary>
        List<byte> _bytes;

        /// <summary>
        /// The index of <see cref="_bytes"/> from which the next value will be read.
        /// </summary>
        int _readPosition;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new empty instance of the <see cref="ByteArrayManipulator"/> class.
        /// </summary>
        public ByteArrayManipulator()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayManipulator"/> class.
        /// </summary>
        /// <param name="data">The initial content of the stream or <see langword="null"/> if the
        /// stream should be initialized empty.</param>
        public ByteArrayManipulator(byte[] data)
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI38827", _OBJECT_NAME);

                if (data == null)
                {
                    _bytes = new List<byte>();
                }
                else
                {
                    _bytes = new List<byte>(data);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38828");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the read position.
        /// </summary>
        /// <value>
        /// The read position.
        /// </value>
        public int ReadPosition
        {
            get
            {
                return _readPosition;
            }

            set
            {
                try
                {
                    ExtractException.Assert("ELI38829", "Invalid read position.",
                        value > 0 && value < Length, "Position", value, "Length", Length);

                    _readPosition = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38851");
                }
            }
        }

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int Length
        {
            get
            {
                return _bytes.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ByteArrayManipulator"/> is EOF.
        /// </summary>
        /// <value><see langword="true"/> if EOF; otherwise, <see langword="false"/>.</value>
        public bool EOF
        {
            get
            {
                return ReadPosition >= Length;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see langword="byte"/>s to write to the stream.</param>
        public void Write(params byte[] value)
        {
            try
            {
                _bytes.AddRange(value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38830");
            }
        }

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see langword="string"/> to write to the stream.</param>
        public void Write(string value)
        {
            try
            {
                _bytes.AddRange(BitConverter.GetBytes(value.Length));
                _bytes.AddRange(Encoding.ASCII.GetBytes(value));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38831");
            }
        }

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see langword="bool"/> to write to the stream.</param>
        public void Write(bool value)
        {
            try
            {
                _bytes.AddRange(BitConverter.GetBytes(value));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38832");
            }
        }
 

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see cref="Int16"/> to write to the stream.</param>
        public void Write(Int16 value)
        {
            try
            {
                _bytes.AddRange(BitConverter.GetBytes(value));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38833");
            }
        }

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> to write to the stream.</param>
        public void Write(Int32 value)
        {
            try
            {
                _bytes.AddRange(BitConverter.GetBytes(value));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38834");
            }
        }

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see cref="Int64"/> to write to the stream.</param>
        public void Write(Int64 value)
        {
            try
            {
                _bytes.AddRange(BitConverter.GetBytes(value));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38835");
            }
        }

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> to write to the stream.</param>
        public void Write(DateTime value)
        {
            try
            {
                _bytes.AddRange(BitConverter.GetBytes(value.ToBinary()));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38836");
            }
        }

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> to write to the stream.</param>
        public void WriteAsCTime(DateTime value)
        {
            try
            {
                if (value == new DateTime(0))
                {
                    Write((int)0);
                }
                else
                {
                    TimeSpan span = value - new DateTime(1970, 1, 1);
                    Write((int)(span.Ticks / TimeSpan.TicksPerSecond));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38836");
            }
        }

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see cref="Guid"/> to write to the stream.</param>
        public void Write(Guid value)
        {
            try
            {
                _bytes.AddRange(value.ToByteArray());
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38837");
            }
        }

        /// <summary>
        /// Reads the specified <see paramref="count"/> of bytes from the stream, starting at
        /// <see cref="ReadPosition"/>.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The <see paramref="count"/> bytes starting at <see cref="ReadPosition"/>.
        /// </returns>
        public byte[] ReadBytes(int count)
        {
            try
            {
                ExtractException.Assert("ELI38838", "Cannot read past end of stream.",
                    ReadPosition + count <= Length, "ReadPosition", ReadPosition, "Count", count,
                        "Length", Length);

                var bytes = _bytes.GetRange(_readPosition, count).ToArray();
                _readPosition += count;

                return bytes;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38839");
            }
        }

        /// <summary>
        /// Reads a <see langword="string"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see langword="string"/> value read from the stream.</returns>
        public string ReadString()
        {
            try
            {
                int length = BitConverter.ToInt32(ReadBytes(sizeof(int)), 0);
                return Encoding.ASCII.GetString(ReadBytes(length));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38840");
            }
        }

        /// <summary>
        /// Reads a <see cref="DateTime"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> value read from the stream.</returns>
        public bool ReadBoolean()
        {
            try
            {
                return BitConverter.ToBoolean(ReadBytes(sizeof(bool)), 0);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38841");
            }
        }

        /// <summary>
        /// Reads a <see cref="Int16"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="Int16"/> value read from the stream.</returns>
        public int ReadInt16()
        {
            try
            {
                return BitConverter.ToInt16(ReadBytes(sizeof(Int16)), 0);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38842");
            }
        }

        /// <summary>
        /// Reads a <see cref="Int32"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="Int32"/> value read from the stream.</returns>
        public int ReadInt32()
        {
            try
            {
                return BitConverter.ToInt32(ReadBytes(sizeof(Int32)), 0);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38843");
            }
        }

        /// <summary>
        /// Reads a <see cref="Int64"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="Int64"/> value read from the stream.</returns>
        public int ReadInt64()
        {
            try
            {
                return BitConverter.ToInt32(ReadBytes(sizeof(Int64)), 0);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38844");
            }
        }

        /// <summary>
        /// Reads a <see cref="DateTime"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> value read from the stream.</returns>
        public DateTime ReadDateTime()
        {
            try
            {
                long binary = BitConverter.ToInt64(ReadBytes(sizeof(Int64)), 0);

                return DateTime.FromBinary(binary);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38845");
            }
        }

        /// <summary>
        /// Reads a <see cref="DateTime"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> value read from the stream.</returns>
        public DateTime ReadCTimeAsDateTime()
        {
            try
            {
                int seconds = BitConverter.ToInt32(ReadBytes(sizeof(Int32)), 0);

                if (seconds == 0)
                {
                    return new DateTime(0);
                }
                else
                {
                    return new DateTime(1970, 1, 1) +
                        TimeSpan.FromTicks(seconds * TimeSpan.TicksPerSecond);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38845");
            }
        }

        /// <summary>
        /// Reads a <see cref="Guid"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="Guid"/> value read from the stream.</returns>
        public Guid ReadGuid()
        {
            try
            {
                return new Guid(ReadBytes(16));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38847");
            }
        }

        /// <summary>
        /// Gets an array of the <see langword="byte"/>s in the stream with 0 bytes padded to the
        /// end such that the number of bytes returned is a multiple of
        /// <see paramref="padToMultiple"/>.
        /// </summary>
        /// <param name="padToMultiple">The number of bytes returned should be a multiple of this
        /// number.</param>
        /// <returns>The <see cref="byte"/>s in the stream.</returns>
        public byte[] GetBytes(int padToMultiple)
        {
            try
            {
                int toPad = (padToMultiple > 0)
                    ? padToMultiple - (Length % padToMultiple)
                    : 0;
                return _bytes.Concat(Enumerable.Repeat((byte)0, toPad)).ToArray();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38846");
            }
        }

        #endregion Methods
    }
}
