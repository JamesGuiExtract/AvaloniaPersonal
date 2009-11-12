using System;
using System.Collections.Generic;
using System.Text;
using Extract.Utilities;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents a list of exemption codes.
    /// </summary>
    public class ExemptionCodeList : IEquatable<ExemptionCodeList>
    {
        #region ExemptionCodeList Fields

        /// <summary>
        /// The exemption code category.
        /// </summary>
        readonly string _category;

        /// <summary>
        /// The exemption codes applied.
        /// </summary>
        readonly string[] _codes;

        /// <summary>
        /// Other text associated with the list of exemption codes.
        /// </summary>
        readonly string _text;

        #endregion ExemptionCodeList Fields

        #region ExemptionCodeList Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExemptionCodeList"/> class.
        /// </summary>
        public ExemptionCodeList() : this(null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExemptionCodeList"/> class.
        /// </summary>
        public ExemptionCodeList(string category, IEnumerable<string> codes, string text)
        {
            _category = category ?? "";

            _codes = codes == null ? new string[0] : CollectionMethods.ToArray(codes);

            _text = text ?? "";
        }

        #endregion ExemptionCodeList Constructors

        #region ExemptionCodeList Properties

        /// <summary>
        /// Gets the category associated with the exemption codes.
        /// </summary>
        /// <returns>The category associated with the exemption codes.</returns>
        public string Category
        {
            get
            {
                return _category;
            }
        }

        /// <summary>
        /// Gets the exemption codes.
        /// </summary>
        /// <returns>The exemption codes.</returns>
        public IEnumerable<string> Codes
        {
            get
            {
                return _codes;
            }
        }

        /// <summary>
        /// Gets the additional text associated with the exemption codes.
        /// </summary>
        /// <returns>The additional text associated with the exemption codes.</returns>
        public string OtherText
        {
            get
            {
                return _text;
            }
        }

        /// <summary>
        /// Gets whether the <see cref="ExemptionCodeList"/> is empty.
        /// </summary>
        /// <returns><see langword="true"/> if the exemption code list does not have exemption 
        /// codes or other text; <see langword="false"/> if the exemption code list has at least 
        /// one exemption code or some other text.</returns>
        public bool IsEmpty
        {
            get
            {
                return _codes.Length <= 0 && string.IsNullOrEmpty(_text);
            }
        }

        /// <summary>
        /// Gets whether the <see cref="ExemptionCodeList"/> has a category.
        /// </summary>
        /// <returns><see langword="true"/> if the <see cref="ExemptionCodeList"/> has a category;
        /// <see langword="false"/> if no category is specified.</returns>
        public bool HasCategory
        {
            get
            {
                return !string.IsNullOrEmpty(_category);
            }
        }

        #endregion ExemptionCodeList Properties

        #region ExemptionCodeList Overrides

        /// <summary>
        /// Returns a string that represents the <see cref="ExemptionCodeList"/>.
        /// </summary>
        /// <returns>A string that represents the <see cref="ExemptionCodeList"/>.</returns>
        public override string ToString()
        {
            // Append the exemption codes together
	        string result = string.Join(", ", _codes);

	        // Add the additional text
	        if (!string.IsNullOrEmpty(_text))
	        {
		        if (string.IsNullOrEmpty(result))
		        {
                    result = _text;
		        }
		        else
		        {
                    result += ", " + _text;
		        }
	        }

	        // Return the result
            return result;
        }

        #endregion ExemptionCodeList Overrides

        #region ExemptionCodeList Methods

        /// <summary>
        /// Determines whether the specified code is contained in the 
        /// <see cref="ExemptionCodeList"/>.
        /// </summary>
        /// <param name="code">The code to check for containment.</param>
        /// <returns><see langword="true"/> if <paramref name="code"/> is in the list; 
        /// <see langword="false"/> if <paramref name="code"/> is not in the list.</returns>
        public bool HasCode(string code)
        {
            return Array.IndexOf(_codes, code) >= 0;
        }

        /// <summary>
        /// Creates an exemption code list from the specified category and codes.
        /// </summary>
        /// <param name="category">The exemption code category.</param>
        /// <param name="codes">A comma separated list of exemption codes and other text.</param>
        /// <param name="masterCodes">The master list of valid exemption categories and codes.
        /// </param>
        /// <returns>An exemption code list of <paramref name="codes"/> in 
        /// <paramref name="category"/>.</returns>
        public static ExemptionCodeList Parse(string category, string codes, 
            MasterExemptionCodeList masterCodes)
        {
            try
            {
                List<string> exemptions = new List<string>(codes.Split(
                        new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries));

                StringBuilder otherText = new StringBuilder();
                for (int i = 0; i < exemptions.Count; i++)
                {
                    string code = exemptions[i];
                    if (!masterCodes.HasCode(category, code))
                    {
                        if (otherText.Length > 0)
                        {
                            otherText.Append(", ");
                        }
                        otherText.Append(code);

                        exemptions.RemoveAt(i);
                        i--;
                    }
                }

                return new ExemptionCodeList(category, exemptions.ToArray(), otherText.ToString());
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26935",
                    "Unable to parse exemption code list.", ex);
                ee.AddDebugData("Category", category, false);
                ee.AddDebugData("Codes", codes, false);
                throw ee;
            }
        }

        /// <summary>
        /// Determines whether the two arrays contain the same strings irrespective of order.
        /// </summary>
        /// <param name="left">An array of strings.</param>
        /// <param name="right">Another array of strings.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> contains exactly the same 
        /// elements as <paramref name="right"/> in any order; <see langword="false"/> if 
        /// <paramref name="left"/> or <paramref name="right"/> contain any element that is not in 
        /// the other.</returns>
        static bool AreEqual(string[] left, string[] right)
        {
            // If the lengths are different, they are not the same
            if (left.Length != right.Length)
            {
                return false;
            }

            // If the left contains a code that is not in right, they are not the same
            foreach (string code in left)
            {
                if (Array.IndexOf(right, code) < 0)
                {
                    return false;
                }
            }

            // They are the same
            return true;
        }

        /// <summary>
        /// Generates a hash code for the array of exemption codes.
        /// </summary>
        /// <param name="codes">An array of exemption codes.</param>
        /// <returns>A hash code for <paramref name="codes"/>.</returns>
        static int GetHashCode(IEnumerable<string> codes)
        {
            int hashCode = 0;
            foreach (string code in codes)
            {
                hashCode ^= code.GetHashCode();
            }

            return hashCode;
        }

        #endregion ExemptionCodeList Methods

        #region IEquatable<ExemptionCodeList> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type. 
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true"/> if the current object is equal to the other parameter; 
        /// otherwise, <see langword="false"/>.</returns>
        public bool Equals(ExemptionCodeList other)
        {
            // If the other list is null, they are not equal.
            if (other == null)
            {
                return false;
            }

            // Empty exemption lists are considered equal if only their categories differ.
            if (_codes.Length == 0 && other._codes.Length == 0)
            {
                return _text == other._text;
            }

            // These are equal if and only if:
            // 1) The categories are the same
            // 2) The exemption codes are the same
            // 3) The other text is the same
            return _category == other._category && AreEqual(_codes, other._codes) &&
                _text == other._text;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current 
        /// <see cref="Object"/>. 
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current 
        /// <see cref="Object"/>.</param>
        /// <returns><see langword="true"/> if the specified <see cref="Object"/> is equal to the 
        /// current <see cref="Object"/>; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ExemptionCodeList);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="ExemptionCodeList"/>. 
        /// </summary>
        /// <returns>A hash code for the current <see cref="ExemptionCodeList"/>.</returns>
        public override int GetHashCode()
        {
            // The hash code for empty exemption lists differ only by the additional text.
            if (_codes.Length == 0)
            {
                return _text.GetHashCode();
            }

            return _category.GetHashCode() ^ GetHashCode(_codes) ^ _text.GetHashCode();
        }

        #endregion IEquatable<ExemptionCodeList> Members
    }
}
