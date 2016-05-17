using System;
using System.Collections.Generic;

namespace Extract.Utilities
{
    /// <summary>
    /// Utility to simplify creating hash codes for custom objects
    /// http://stackoverflow.com/a/18613926
    /// </summary>
    public struct HashCode : IEquatable<HashCode>
    {
        private readonly int _hashCode;

        /// <summary>
        /// Constructs a <see cref="HashCode"/> from an integer
        /// </summary>
        /// <param name="hashCode">The integer hash code value</param>
        public HashCode(int hashCode)
        {
            _hashCode = hashCode;
        }

        /// <summary>
        /// Constructs a <see cref="HashCode"/> with a default initial value
        /// </summary>
        public static HashCode Start
        {
            get
            {
                return new HashCode(17);
            }
        }

        /// <summary>
        /// Converts from <see cref="HashCode"/> to the underlying <see langword="int"/>
        /// </summary>
        /// <param name="hashCode">The <see cref="HashCode"/> to convert</param>
        /// <returns>The underlying <see cref="int"/> value</returns>
        public static int ToInt32(HashCode hashCode)
        {
            return hashCode.GetHashCode();
        }

        /// <summary>
        /// Converts from an <see langword="int"/> to a <see cref="HashCode"/>
        /// </summary>
        /// <param name="value">The <see langword="int"/> to convert</param>
        /// <returns>A <see cref="HashCode"/> enclosing the <see langword="int"/></returns>
        public static HashCode ToHashCode(int value)
        {
            return new HashCode(value);
        }

        /// <summary>
        /// Converts from <see cref="HashCode"/> to the underlying <see langword="int"/>
        /// </summary>
        /// <param name="hashCode">The <see cref="HashCode"/> to convert</param>
        /// <returns>The underlying <see cref="int"/> value</returns>
        public static implicit operator int(HashCode hashCode)
        {
            return hashCode.GetHashCode();
        }

        /// <summary>
        /// Computes the hash code for <see paramref="obj"/> and combines it with this instance
        /// </summary>
        /// <typeparam name="T">The type of <see paramref="obj"/></typeparam>
        /// <param name="obj">The <see paramref="T"/> the hash code of which to combine with this instance</param>
        /// <returns>The combined hash codes</returns>
        public HashCode Hash<T>(T obj)
        {
            try
            {
                var c = EqualityComparer<T>.Default;
                var h = c.Equals(obj, default(T)) ? 0 : obj.GetHashCode();
                unchecked
                {
                    h += _hashCode * 31;
                }
                return new HashCode(h);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39828");                
            }
        }

        /// <summary>
        /// Gets the underlying <see langword="int"/>
        /// </summary>
        /// <returns>The underlying <see langword="int"/></returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        /// Whether this instance's underlying value is equal to another
        /// </summary>
        /// <param name="other">The other instance to compare</param>
        /// <returns><see langword="true"/> if the instances are the same, else <see langword="false"/></returns>
        public bool Equals(HashCode other)
        {
            return _hashCode == other.GetHashCode();
        }

        /// <summary>
        /// Whether this instance's underlying value is equal to another
        /// </summary>
        /// <param name="obj">The other instance to compare</param>
        /// <returns><see langword="true"/> if the instances are the same, else <see langword="false"/></returns>
        public override bool Equals(object obj)
        {
            return obj is HashCode && this.Equals((HashCode)obj);
        }

        /// <summary>
        /// Whether two <see cref="HashCode"/>s are equal
        /// </summary>
        /// <param name="leftHandSide">LHS of operator</param>
        /// <param name="rightHandSide">RHS of operator</param>
        /// <returns><see langword="true"/> if the instances are the same, else <see langword="false"/></returns>
        public static bool operator ==(HashCode leftHandSide, HashCode rightHandSide)
        {
            return leftHandSide.Equals(rightHandSide);
        }

        /// <summary>
        /// Whether two <see cref="HashCode"/>s are not equal
        /// </summary>
        /// <param name="leftHandSide">LHS of operator</param>
        /// <param name="rightHandSide">RHS of operator</param>
        /// <returns><see langword="true"/> if the instances are not the same, else <see langword="false"/></returns>
        public static bool operator !=(HashCode leftHandSide, HashCode rightHandSide)
        {
            return !leftHandSide.Equals(rightHandSide);
        }
    }
}
