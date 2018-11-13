using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extract.FAMDBCounterManager
{
    /// <summary>
    /// Provides ability to serialize objects of various data types into an array of bytes.
    /// <para><b>Note</b></para>
    /// This class is a modified copy of Extract.Utilities.ByteArrayManipulator. This project is not
    /// linked to Extract.Utilities to avoid COM dependencies.
    /// </summary>
    internal class ByteArrayManipulator
    {
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
            if (data == null)
            {
                _bytes = new List<byte>();
            }
            else
            {
                _bytes = new List<byte>(data);
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
                UtilityMethods.Assert(value >= 0 && value < Length, "Invalid read position.");

                _readPosition = value;
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

        #endregion Properties

        #region Methods

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see langword="string"/> to write to the stream.</param>
        public void Write(string value)
        {
            _bytes.AddRange(BitConverter.GetBytes(value.Length));
            _bytes.AddRange(Encoding.ASCII.GetBytes(value));
        }

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> to write to the stream.</param>
        public void Write(Int32 value)
        {
            _bytes.AddRange(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see cref="Int64"/> to write to the stream.</param>
        public void Write(Int64 value)
        {
            _bytes.AddRange(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> to write to the stream.</param>
        public void WriteAsCTime(DateTime value)
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

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream as a filetime.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> to write to the stream.</param>
        public void WriteAsFileTime(DateTime value)
        {
            if (value == new DateTime(0))
            {
                Write((int)0);
            }
            else
            {
                Write(value.ToFileTime());
            }
        }

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see cref="Guid"/> to write to the stream.</param>
        public void Write(Guid value)
        {
            _bytes.AddRange(value.ToByteArray());
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
            UtilityMethods.Assert(ReadPosition + count <= Length, "Cannot read past end of stream");

            var bytes = _bytes.GetRange(_readPosition, count).ToArray();
            _readPosition += count;

            return bytes;
        }

        /// <summary>
        /// Reads a <see langword="string"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see langword="string"/> value read from the stream.</returns>
        public string ReadString()
        {
            int length = BitConverter.ToInt32(ReadBytes(sizeof(int)), 0);
            return Encoding.ASCII.GetString(ReadBytes(length));
        }

        /// <summary>
        /// Reads a <see cref="Int32"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="Int32"/> value read from the stream.</returns>
        public int ReadInt32()
        {
             return BitConverter.ToInt32(ReadBytes(sizeof(Int32)), 0);
        }

        /// <summary>
        /// Reads a <see cref="DateTime"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> value read from the stream.</returns>
        public DateTime ReadCTimeAsDateTime()
        {
            int seconds = BitConverter.ToInt32(ReadBytes(sizeof(Int32)), 0);

            if (seconds == 0)
            {
                return new DateTime(0);
            }
            else
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) +
                    TimeSpan.FromTicks(seconds * TimeSpan.TicksPerSecond);
            }
        }

        /// <summary>
        /// Reads a filetime from the string into a <see cref="DateTime"/> value.
        /// <para><b>Note</b></para>
        /// The filetime is interpreted literally-- it is not converted based on timezone.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> value read from the stream.</returns>
        public DateTime ReadFileTimeAsDateTime()
        {
            long fileTime = BitConverter.ToInt64(ReadBytes(sizeof(Int64)), 0);

            if (fileTime == 0)
            {
                return new DateTime(0);
            }
            else
            {
                // Read the filetime as UTC, otherwise it will be adjusted based on the 
                // current timezone.
                return DateTime.FromFileTimeUtc(fileTime);
            }
        }

        /// <summary>
        /// Reads a <see cref="Guid"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="Guid"/> value read from the stream.</returns>
        public Guid ReadGuid()
        {
            return new Guid(ReadBytes(16));
        }

        /// <summary>
        /// Gets an array of the <see langword="byte"/>s in the stream with bytes padded to the end
        /// such that the number of bytes returned is a multiple of <see paramref="padToMultiple"/>.
        /// </summary>
        /// <param name="padToMultiple">The number of bytes returned should be a multiple of this
        /// number.</param>
        /// <returns>The <see cref="byte"/>s in the stream.</returns>
        public byte[] GetBytes(int padToMultiple)
        {
            int toPad = (padToMultiple > 0)
                ? padToMultiple - (Length % padToMultiple)
                : 0;
            return _bytes.Concat(Enumerable.Repeat((byte)0, toPad)).ToArray();
        }

        #endregion Methods
    }
}
