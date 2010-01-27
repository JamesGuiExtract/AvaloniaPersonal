using Extract.Imaging.Forms;
using Extract.Licensing;
using System;

namespace Extract.Rules
{
    internal class FindResult : IComparable<FindResult>
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FindResult).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="CompositeHighlightLayerObject"/> associated
        /// with this <see cref="FindResult"/>.
        /// </summary>
        readonly CompositeHighlightLayerObject _compositeMatch;

        /// <summary>
        /// The <see cref="MatchResult"/> that produced this <see cref="FindResult"/>.
        /// </summary>
        readonly MatchResult _matchResult;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FindResult"/> class.
        /// </summary>
        /// <param name="compositeMatch">The <see cref="CompositeHighlightLayerObject"/>
        /// to display for this find result.</param>
        /// <param name="matchResult">The <see cref="MatchResult"/> this <see cref="FindResult"/>
        /// is associated with (was created from).</param>
        internal FindResult(CompositeHighlightLayerObject compositeMatch, MatchResult matchResult)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RedactionCoreObjects, "ELI23196",
                    _OBJECT_NAME);

                _compositeMatch = compositeMatch;
                _matchResult = matchResult;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23197", ex);
            }
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets the <see cref="CompositeHighlightLayerObject"/> for this
        /// <see cref="FindResult"/>.
        /// </summary>
        /// <returns>The<see cref="CompositeHighlightLayerObject"/> for this
        /// <see cref="FindResult"/>.</returns>
        internal CompositeHighlightLayerObject CompositeMatch
        {
            get
            {
                return _compositeMatch;
            }
        }

        /// <summary>
        /// Gets the <see cref="MatchResult"/> associated with this <see cref="FindResult"/>.
        /// </summary>
        /// <returns>The <see cref="MatchResult"/> associated with this <see cref="FindResult"/>.
        /// </returns>
        internal MatchResult MatchResult
        {
            get
            {
                return _matchResult;
            }
        }

        #endregion Properties


        #region IComparable<FindResult> Members

        /// <summary>
        /// Compares this <see cref="FindResult"/> with another <see cref="FindResult"/>.
        /// </summary>
        /// <param name="other">A <see cref="FindResult"/> to compare with this
        /// <see cref="FindResult"/>.</param>
        /// <returns>An <see cref="int"/> that indicates the relative order of the
        /// <see cref="FindResult"/> objects being compared.</returns>
        public int CompareTo(FindResult other)
        {
            // Just compare the composite matches
            return _compositeMatch.CompareTo(other._compositeMatch);
        }

        /// <summary>
        /// Checks whether the specified <see cref="object"/> is equal to this
        /// <see cref="FindResult"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with.</param>
        /// <returns><see langword="true"/> if the objects are equal and
        /// <see langword="false"/> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // Check if this is a find result
            FindResult findResult = obj as FindResult;
            if (findResult == null)
            {
                return false;
            }

            // Check if they are equal
            return this == findResult;
        }

        /// <summary>
        /// Checks whether the specified <see cref="FindResult"/> is equal to this
        /// <see cref="FindResult"/>.
        /// </summary>
        /// <param name="findResult">The <see cref="FindResult"/> to compare with.</param>
        /// <returns><see langword="true"/> if the objects are equal and
        /// <see langword="false"/> otherwise.</returns>
        public bool Equals(FindResult findResult)
        {
            return this == findResult;
        }

        /// <summary>
        /// Returns a hashcode for this <see cref="FindResult"/>.
        /// </summary>
        /// <returns>The hashcode for this <see cref="FindResult"/>.</returns>
        public override int GetHashCode()
        {
            return _compositeMatch.Id.GetHashCode() ^ _matchResult.GetHashCode();
        }

        /// <summary>
        /// Checks whether two specified <see cref="FindResult"/> objects are equal.
        /// </summary>
        /// <param name="result1">A <see cref="FindResult"/> to compare.</param>
        /// <param name="result2">A <see cref="FindResult"/> to compare.</param>
        /// <returns><see langword="true"/> if the objects are equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator ==(FindResult result1, FindResult result2)
        {
            if (ReferenceEquals(result1, result2))
            {
                return true;
            }

            if (((object)result1 == null) || ((object)result2 == null))
            {
                return false;
            }

            return (result1.CompositeMatch.Id == result2.CompositeMatch.Id)
                && (result1.MatchResult == result2.MatchResult);
        }

        /// <summary>
        /// Checks whether two specified <see cref="FindResult"/> objects are not equal.
        /// </summary>
        /// <param name="result1">A <see cref="FindResult"/> to compare.</param>
        /// <param name="result2">A <see cref="FindResult"/> to compare.</param>
        /// <returns><see langword="true"/> if the objects are not equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator !=(FindResult result1, FindResult result2)
        {
            return !(result1 == result2);
        }

        /// <summary>
        /// Checks whether the first specified <see cref="FindResult"/> is less than
        /// the second specified <see cref="FindResult"/>.
        /// </summary>
        /// <param name="result1">A <see cref="FindResult"/> to compare.</param>
        /// <param name="result2">A <see cref="FindResult"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="result1"/> is less
        /// than <paramref name="result2"/> and <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator <(FindResult result1, FindResult result2)
        {
            return result1.CompareTo(result2) < 0;
        }

        /// <summary>
        /// Checks whether the first specified <see cref="FindResult"/> is greater than
        /// the second specified <see cref="FindResult"/>.
        /// </summary>
        /// <param name="result1">A <see cref="FindResult"/> to compare.</param>
        /// <param name="result2">A <see cref="FindResult"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="result1"/> is greater
        /// than <paramref name="result2"/> and <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator >(FindResult result1, FindResult result2)
        {
            return result1.CompareTo(result2) > 0;
        }

        #endregion
    }
}
