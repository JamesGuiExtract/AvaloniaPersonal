using Extract.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extract.ErrorHandling
{
    /// <summary>
    /// Provides ability to serialize objects of various data types into an array of bytes.
    /// </summary>
    internal class ByteArrayManipulator
    {
        // Since the c++ side is compiled in 32 bit - kLong size is Int32 and kUnsignedLong is UInt32
        enum EType : int { kString, kOctets, kInt, kLong, kUnsignedLong, kDouble, kBoolean, kNone, kInt64, kInt16, kDateTime, kGuid };

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

                if (value < 0 || value >= Length)
                {
                    var ee = new ExtractException("ELI53830", "Invalid read position.");
                    ee.AddDebugData("Position", value);
                    ee.AddDebugData("Length", Length);
                    throw ee;
                }

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
            _bytes.AddRange(value);
        }

        /// <summary>
        /// Writes the given byteStream to the stream
        /// </summary>
        /// <param name="byteStream"></param>
        public void Write(ByteArrayManipulator byteStream)
        {
            var byteStreamBytes = byteStream.GetBytes();
            Write((UInt32)byteStreamBytes.Length);
            Write(byteStreamBytes);
        }

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
        /// <param name="value">The <see langword="bool"/> to write to the stream.</param>
        public void Write(bool value)
        {
            _bytes.AddRange(BitConverter.GetBytes(value));
        }


        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see cref="Int16"/> to write to the stream.</param>
        public void Write(Int16 value)
        {
            _bytes.AddRange(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> to write to the stream.</param>
        public void Write(Int32 value)
        {
            _bytes.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(UInt32 value)
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
        public void Write(DateTime value)
        {
            _bytes.AddRange(BitConverter.GetBytes(value.ToBinary()));
        }

        /// <summary>
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> to write to the stream.</param>
        public void WriteAsCTime(DateTime value)
        {
            if (value == new DateTime(0))
            {
                Write((Int64)0);
            }
            else
            {
                TimeSpan span = value - new DateTime(1970, 1, 1);
                Write((Int64)(span.Ticks / TimeSpan.TicksPerSecond));
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
        /// Writes the specified <see paramref="value"/> to the stream.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> to write to the stream.</param>
        public void Write(double value)
        {
            _bytes.AddRange(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Determines the type and uses the appropriate write method
        /// </summary>
        /// <param name="value">the object to be saved</param>
        /// <exception cref="ExtractException">Throws if the type has no appropriate write method</exception>
        public void Write(Object value)
        {
            var type = value.GetType();
            // only types allowed are the ones that can be saved
            switch (type.Name)
            {
                case "String":
                    Write((UInt32)EType.kString);
                    Write((string)value);
                    break;
                case "Boolean":
                    Write((UInt32)EType.kBoolean);
                    Write((bool)value);
                    break;
                case "Int16":
                    Write((UInt32)EType.kInt16);
                    Write((Int16)value);
                    break;
                case "Int32":
                    Write((UInt32)EType.kLong);
                    Write((Int32)value);
                    break;
                case "Int64":
                    Write((UInt32)EType.kInt64);
                    Write((Int64)value);
                    break;
                case "UInt32":
                    Write((UInt32)EType.kUnsignedLong);
                    Write((UInt32)value);
                    break;
                case "DateTime":
                    Write((UInt32)EType.kDateTime);
                    Write((DateTime)value);
                    break;
                case "Guid":
                    Write((UInt32)EType.kGuid);
                    Write((Guid)value);
                    break;
                case "Double":
                    Write((UInt32)EType.kDouble);
                    Write((Double)value);
                    break;
                default:
                    throw new ExtractException("ELI53525",  $"Unable to write {value.GetType().Name} to ByteStream");
            }
        }

        /// <summary>
        /// Reads the specified <see paramref="count"/> of bytes from the stream, starting at
        /// <see cref="ReadPosition"/>.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The <see paramref="count"/> bytes starting at <see cref="ReadPosition"/>.
        /// </returns>
        public byte[] ReadBytes(UInt32 count)
        {
            if (ReadPosition + count > Length)
            {
                var ee = new ExtractException("ELI53831", "Cannot read past end of stream.");
                ee.AddDebugData("ReadPosition", ReadPosition);
                ee.AddDebugData("Count", count);
                ee.AddDebugData("Length", Length);
                throw ee;
            }

            var bytes = _bytes.GetRange(_readPosition, (int)count).ToArray();
            _readPosition += (int)count;

            return bytes;
        }

        /// <summary>
        /// Reads a Byte stream from the bytes starting at the current read position
        /// </summary>
        /// <returns>Returns ByteArrayManipulator for the bytes read</returns>
        public ByteArrayManipulator ReadByteStream()
        {
            UInt32 size = ReadUInt32();
            return new ByteArrayManipulator(ReadBytes(size));
        }

        /// <summary>
        /// Reads a <see langword="string"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see langword="string"/> value read from the stream.</returns>
        public string ReadString()
        {
            UInt32 length = BitConverter.ToUInt32(ReadBytes(sizeof(UInt32)), 0);
            return Encoding.ASCII.GetString(ReadBytes(length));
        }

        /// <summary>
        /// Reads a <see cref="DateTime"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> value read from the stream.</returns>
        public bool ReadBoolean()
        {
            return BitConverter.ToBoolean(ReadBytes(sizeof(bool)), 0);
        }

        /// <summary>
        /// Reads a <see cref="Int16"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="Int16"/> value read from the stream.</returns>
        public Int16 ReadInt16()
        {
            return BitConverter.ToInt16(ReadBytes(sizeof(Int16)), 0);
        }

        /// <summary>
        /// Reads a <see cref="Int32"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="Int32"/> value read from the stream.</returns>
        public Int32 ReadInt32()
        {
            return BitConverter.ToInt32(ReadBytes(sizeof(Int32)), 0);
        }

        /// <summary>
        /// Reads a <see cref="Int64"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="Int64"/> value read from the stream.</returns>
        public Int64 ReadInt64()
        {
            return BitConverter.ToInt64(ReadBytes(sizeof(Int64)), 0);
        }

        /// <summary>
        /// Reads a <see cref="UInt16"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="UInt16"/> value read from the stream.</returns>
        public UInt16 ReadUInt16()
        {
            return BitConverter.ToUInt16(ReadBytes(sizeof(UInt16)), 0);
        }

        /// <summary>
        /// Reads a <see cref="UInt32"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="UInt32"/> value read from the stream.</returns>
        public UInt32 ReadUInt32()
        {
            return BitConverter.ToUInt32(ReadBytes(sizeof(UInt32)), 0);
        }

        /// <summary>
        /// Reads a <see cref="UInt64"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="UInt64"/> value read from the stream.</returns>
        public UInt64 ReadUInt64()
        {
            return BitConverter.ToUInt32(ReadBytes(sizeof(UInt64)), 0);
        }

        /// <summary>
        /// Reads a <see cref="DateTime"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> value read from the stream.</returns>
        public DateTime ReadDateTime()
        {
            long binary = BitConverter.ToInt64(ReadBytes(sizeof(Int64)), 0);

            return DateTime.FromBinary(binary);
        }

        /// <summary>
        /// Reads a <see cref="DateTime"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> value read from the stream.</returns>
        public DateTime ReadCTimeAsDateTime()
        {
            Int64 seconds = BitConverter.ToInt64(ReadBytes(sizeof(Int64)), 0);

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

        /// <summary>
        /// Reads a <see cref="Guid"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="Guid"/> value read from the stream.</returns>
        public Guid ReadGuid()
        {
            return new Guid(ReadBytes(16));
        }

        /// <summary>
        /// Reads a <see cref="Double"/> value from the stream at <see cref="ReadPosition"/>.
        /// </summary>
        /// <returns>The <see cref="Double"/> value read from the stream.</returns>
        public Double ReadDouble()
        {
            return BitConverter.ToDouble(ReadBytes(sizeof(Double)), 0);
        }

        /// <summary>
        /// Reads an object from the strem
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ExtractException"></exception>
        public object ReadObject()
        {
            EType type = (EType)ReadInt32();
            object value;
            // only types allowed are the ones that can be saved
            switch (type)
            {
                case EType.kString:
                    value = ReadString();
                    break;
                case EType.kBoolean:
                    value = ReadBoolean();
                    break;
                case EType.kInt16:
                    value = ReadInt16();
                    break;
                case EType.kInt:
                case EType.kLong:
                    value = ReadInt32();
                    break;
                case EType.kUnsignedLong:
                    value = ReadUInt32();
                    break;
                case EType.kInt64:
                    value = ReadInt64();
                    break;
                case EType.kDateTime:
                    value = ReadDateTime();
                    break;
                case EType.kGuid:
                    value = ReadGuid();
                    break;
                case EType.kDouble:
                    value = ReadDouble();
                    break;
                default:
                    throw new ExtractException("ELI53526",  $"Unable to read type {type} from ByteStream");
            }
            return value;
        }


        /// <summary>
        /// Gets an array of the <see langword="byte"/>s in the stream with 0 bytes padded to the
        /// end such that the number of bytes returned is a multiple of
        /// <see paramref="padToMultiple"/>.
        /// </summary>
        /// <param name="padToMultiple">The number of bytes returned should be a multiple of this
        /// number.</param>
        /// <returns>The <see cref="byte"/>s in the stream.</returns>
        public byte[] GetBytes(int padToMultiple = 1)
        {
            int toPad = Length % padToMultiple;
            if (toPad > 0)
            {
                toPad = padToMultiple - toPad;
            }
            return _bytes.ToArray(Length + toPad);
        }

        #endregion Methods
    }
}
