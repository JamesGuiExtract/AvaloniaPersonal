using System;

namespace Extract.Utilities
{
    /// <summary>
    /// A simple class that holds a pair of <see cref="System.String"/>
    /// objects. This class was created because FxCop complains about passing around
    /// <see cref="System.Tuple{T,K}" /> of <see cref="System.String"/> in a nested collection./>
    /// </summary>
    public struct StringPair
    {
        #region Fields

        /// <summary>
        /// Gets or sets the first <see cref="String"/>.
        /// </summary>
        /// <value>
        /// The first <see cref="String"/>.
        /// </value>
        public string First { get; set; }

        /// <summary>
        /// Gets or sets the second <see cref="String"/>.
        /// </summary>
        /// <value>
        /// The second <see cref="String"/>.
        /// </value>
        public string Second { get; set; }

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the pair of strings.
        /// </summary>
        public Tuple<string, string> Pair
        {
            get
            {
                return new Tuple<string, string>(First, Second);
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Concat("First: ", First, " Second: ", Second);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return First.GetHashCode() ^ Second.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is StringPair))
            {
                return false;
            }

            return Equals((StringPair)obj);
        }

        /// <summary>
        /// Determines whether the specified <see cref="StringPair"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="StringPair"/> is equal to this instance; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(StringPair obj)
        {
            return this == obj;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(StringPair left, StringPair right)
        {
            return left.First == right.First
                && left.Second == right.Second;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(StringPair left, StringPair right)
        {
            return !(left == right);
        }

        #endregion Overrides
    }
}
