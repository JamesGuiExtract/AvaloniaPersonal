using System;

namespace Extract.Utilities
{
    /// <summary>
    /// A wrapper for a string that makes it case-insensitive wrt equality and comparison
    /// </summary>
    public class NoCaseString : IComparable<NoCaseString>, IEquatable<NoCaseString>
    {
        static readonly StringComparer _STRING_COMPARER = StringComparer.InvariantCultureIgnoreCase;

        /// <summary>
        /// Create an instance
        /// </summary>
        public NoCaseString(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Implicitly convert a string to a NoCaseString
        /// </summary>
        public static implicit operator NoCaseString(string value)
        {
            return new NoCaseString(value);
        }

        /// <summary>
        /// The underlying string value
        /// </summary>
        public string Value { get; }

        /// <inheritdoc/>
        public int CompareTo(NoCaseString other)
        {
            return _STRING_COMPARER.Compare(Value, other.Value);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as NoCaseString);
        }

        /// <inheritdoc/>
        public bool Equals(NoCaseString other)
        {
            return other is not null &&
                _STRING_COMPARER.Equals(Value, other.Value);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _STRING_COMPARER.GetHashCode(Value);
        }

        /// <inheritdoc/>
        public static bool operator ==(NoCaseString left, NoCaseString right)
        {
            return _STRING_COMPARER.Equals(left.Value, right.Value);
        }

        /// <inheritdoc/>
        public static bool operator !=(NoCaseString left, NoCaseString right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Value;
        }
    }
}
