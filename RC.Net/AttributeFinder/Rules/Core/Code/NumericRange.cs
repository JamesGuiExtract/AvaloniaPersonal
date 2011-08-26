using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Extract.AttributeFinder.Rules
{
    public partial class NumericSequencer
    {
        /// <summary>
        /// Represents in individual element in a sequence to be expanded/contracted by the
        /// <see cref="NumericSequencer"/> class. Each element can be a consecutive range of
        /// positive numbers, a single positive number, or an element that could not be parsed as a
        /// number or range of numbers.
        /// </summary>
        class NumericRange : IComparable<NumericRange>, IComparer<NumericRange>, IEquatable<NumericRange>
        {
            #region Fields

            /// <summary>
            /// Used to identify non-numeric instances that can still be sorted according to a
            /// numberic value.
            /// </summary>
            static Regex _suffixedNumberRegex = new Regex(@"[\d]+(?=[\w]+)");

            /// <summary>
            /// A value used to determine this instances position in a sorted list of
            /// <see cref="NumericRange"/>s.
            /// </summary>
            int? _numericSortValue;

            #endregion Fields

            #region Constructors

            /// <overloads>Initializes a new instance of the <see cref="NumericRange"/> class.
            /// </overloads>
            /// <summary>Initializes a new instance of the <see cref="NumericRange"/> class.
            /// </summary>
            /// <param name="text">The text value that represents the range.</param>
            public NumericRange(string text)
            {
                try
                {
                    Text = text.Trim();

                    string[] values = Text.Split('-');
                    ExtractException.Assert("ELI33434", "Too many hypens", values.Length <= 2);

                    uint number1;
                    if (UInt32.TryParse(values[0].Trim(), out number1))
                    {
                        Numeric = true;

                        uint number2 = 0;
                        if (values.Length == 1 ||
                            !UInt32.TryParse(values[1].Trim(), out number2))
                        {
                            if (Text.Contains('-'))
                            {
                                Numeric = false;
                                StartNumber = 0;
                            }
                            else
                            {
                                number2 = number1;
                            }
                        }

                        if (Numeric)
                        {
                            StartNumber = Math.Min(number1, number2);
                            EndNumber = Math.Max(number1, number2);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee =
                        new ExtractException("ELI33435", "Failed to parse numeric range.", ex);
                    ee.AddDebugData("Text", text, true);
                    throw ee;
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="NumericRange"/> class.
            /// </summary>
            /// <param name="number">The one and only number to be in this instance.</param>
            public NumericRange(uint number)
            {
                try
                {
                    Numeric = true;
                    StartNumber = number;
                    EndNumber = number;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33442");
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="NumericRange"/> class.
            /// </summary>
            /// <param name="number1">The first number in the new range.</param>
            /// <param name="number2">The last number in the new range.</param>
            public NumericRange(uint number1, uint number2)
            {
                try
                {
                    Numeric = true;
                    StartNumber = Math.Min(number1, number2);
                    EndNumber = Math.Max(number1, number2);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33443");
                }
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="NumericRange"/> is able to
            /// be interpreted as a numeric range.
            /// </summary>
            /// <value><see langword="true"/> if this <see cref="NumericRange"/> is able to be
            /// interpreted as a numeric range.; otherwise, <see langword="false"/>.
            /// </value>
            public bool Numeric
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the text that was used to create this instance.
            /// </summary>
            /// <value>
            /// The text that was used to create this instance or <see langword="null"/> if this
            /// instance was initialized by numeric range.
            /// </value>
            public string Text
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the first number in the range.
            /// </summary>
            /// <value>
            /// Gets the first number in the range or 0 if <see cref="Numeric"/> is
            /// <see langword="false"/>.
            /// </value>
            public uint StartNumber
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the last number in the range.
            /// </summary>
            /// <value>
            /// Gets the last number in the range or 0 if <see cref="Numeric"/> is
            /// <see langword="false"/>.
            /// </value>
            public uint EndNumber
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the count of numbers in the range.
            /// </summary>
            /// <value>Gets the count of numbers in the range.</value>
            /// <throws><see cref="ExtractException"/> if <see cref="Numeric"/> is
            /// <see langword="false"/>.</throws>
            public uint Count
            {
                get
                {
                    ExtractException.Assert("ELI33436", "Cannot get count from non-numeric range.",
                        Numeric);

                    return EndNumber - StartNumber + 1;
                }
            }

            #endregion Properties

            #region Public Methods

            /// <summary>
            /// Merges <see paramref="range1"/> and <see paramref="range2"/> if possible.
            /// <para><b>Note</b></para>
            /// If <see paramref="eliminateDuplicates"/> is <see paramref="false"/>, a merge may
            /// succeed in producing a larger sequence of numbers than previously existed, but
            /// still return two ranges; the smaller range will be the numbers that needed to be
            /// maintained separately in order to not eliminate duplicates.
            /// </summary>
            /// <param name="range1">The first <see cref="NumericRange"/> to merge.</param>
            /// <param name="range2">The second <see cref="NumericRange"/> to merge.</param>
            /// <param name="eliminateDuplicates"><see langword="true"/> to eliminate duplicates as
            /// part of the merge, <see langword="false"/> to preserve them.</param>
            /// <returns>The <see cref="NumericRange"/> instances that are the result of the merge,
            /// or <see langword="null"/> if the ranges could not be merged.</returns>
            public static IEnumerable<NumericRange> Merge(NumericRange range1, NumericRange range2,
                bool eliminateDuplicates)
            {
                try
                {
                    ExtractException.Assert("ELI33437", "Cannot merge non-numeric ranges.",
                        range1.Numeric && range2.Numeric);

                    ExtractException.Assert("ELI33438", "Cannot merge unsorted numeric ranges",
                        range1.CompareTo(range2) <= 0);

                    // If the ranges don't intersect, they can't be merged.
                    if (range1.EndNumber < range2.StartNumber - 1 ||
                        range1.StartNumber > range2.EndNumber + 1)
                    {
                        return null;
                    }
                    // If eliminating duplicates or the ranges form a larger range without any
                    // overlap return the combined range.
                    else if (eliminateDuplicates || range1.EndNumber < range2.StartNumber)
                    {
                        return new NumericRange[]
                        {
                            new NumericRange(Math.Min(range1.StartNumber, range2.StartNumber),
                                             Math.Max(range1.EndNumber, range2.EndNumber))
                        };
                    }
                    // If range1 starts before range2, but does not extend past the end of range2,
                    // return the overall range, plus the overlapping numbers that need to be
                    // preserved due to the !eliminateDuplicates setting.
                    else if (range1.StartNumber < range2.StartNumber &&
                             range1.EndNumber < range2.EndNumber)
                    {
                        return new NumericRange[]
                        {
                            new NumericRange(Math.Min(range1.StartNumber, range2.StartNumber),
                                             Math.Max(range1.EndNumber, range2.EndNumber)),
                            new NumericRange(range2.StartNumber, range1.EndNumber)
                        };
                    }
                    else
                    {
                        // If we got here we know that either
                        // A) Range1 and range2 have the same starting point or
                        // B) Range1 includes all of the elements of range2
                        // In either case, one range is a subset of the other. Simply leave both
                        // ranges as-is since there is no un-duplicated numbers to consolidate.
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33439");
                }
            }

            /// <summary>
            /// Expands this instance so that there is one <see cref="NumericRange"/> for every
            /// number in the range.
            /// </summary>
            /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="NumericRange"/> that represent
            /// all numbers in the original range and each instance is either a non-numeric instance
            /// or represents a single number.</returns>
            public IEnumerable<NumericRange> Expand()
            {
                try
                {
                    if (Numeric && Count > 1)
                    {
                        // If there are more than one numbers in this range, expand them.
                        return Enumerable.Range((int)StartNumber, (int)EndNumber - (int)StartNumber + 1)
                            .Select(number => new NumericRange(Convert.ToUInt32(number)));
                    }
                    else
                    {
                        // Otherwise, just return this instance.
                        return new NumericRange[] { this };
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33444");
                }
            }

            #endregion Public Methods

            #region Overrides

            /// <summary>
            /// Returns a <see cref="System.String"/> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String"/> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                try
                {
                    if (!Numeric)
                    {
                        return Text;
                    }
                    else if (Count == 1)
                    {
                        return StartNumber.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        return StartNumber.ToString(CultureInfo.InvariantCulture) + "-" +
                            EndNumber.ToString(CultureInfo.InvariantCulture);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33440");
                }
            }

            #endregion Overrides

            #region IComparable Members

            /// <summary>
            /// Compares the current object with another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// A 32-bit signed integer that indicates the relative order of the objects being
            /// compared. The return value has the following meanings: Value Meaning Less than
            /// zero This object is less than the other parameter.Zero This object is equal to
            /// other. Greater than zero This object is greater than other.
            /// </returns>
            public int CompareTo(NumericRange other)
            {
                try
                {
                    // Compare the NumericSortValue of the two instances.
                    int compareResult = NumericSortValue.CompareTo(other.NumericSortValue);

                    if (compareResult == 0)
                    {
                        if (Numeric && other.Numeric)
                        {
                            // For numeric tiebreaker, use the end number
                            return EndNumber.CompareTo(other.EndNumber);
                        }
                        else
                        {
                            // For non-numeric tiebreaker, use a string comparison of the text
                            // values.
                            return string.Compare(Text, other.Text,
                                StringComparison.OrdinalIgnoreCase);
                        }
                    }

                    return compareResult;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33441");
                }
            }

            #endregion IComparable Members

            #region IComparer Members

            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            /// Value Condition Less than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.
            /// </returns>
            public int Compare(NumericRange x, NumericRange y)
            {
                try
                {
                    return x.CompareTo(y);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33445");
                }
            }

            #endregion IComparer Members

            #region IEquatable Members

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data
            /// structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                try
                {
                    if (Numeric)
                    {
                        return StartNumber.GetHashCode() ^ EndNumber.GetHashCode();
                    }
                    else
                    {
                        return (Text == null) ? 0 : Text.GetHashCode();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33446");
                }
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// true if the current object is equal to the other parameter; otherwise, false.
            /// </returns>
            /// 
            public bool Equals(NumericRange other)
            {
                try
                {
                    // Check whether the compared object is null.
                    if (Object.ReferenceEquals(other, null))
                    {
                        return false;
                    }
                    // Check whether the compared object references the same data.
                    else if (Object.ReferenceEquals(this, other))
                    {
                        return true;
                    }
                    // If numeric, compare the start and end numbers.
                    else if (Numeric)
                    {
                        return StartNumber == other.StartNumber && EndNumber == other.EndNumber;
                    }
                    // If non-numeric, compare the text values.
                    else if ((Text == null) != (other.Text == null))
                    {
                        return false;
                    }
                    // If the text of both instances is null, the two instances are equal.
                    else if (Text == null)
                    {
                        return true;
                    }
                    else
                    {
                        return Text.Equals(other.Text, StringComparison.Ordinal);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33447");
                }
            }

            #endregion IEquatable Members

            #region Private members

            /// <summary>
            /// Gets a value used to determine this instances position in a sorted list of
            /// <see cref="NumericRange"/>s.
            /// </summary>
            /// <returns>A value used to determine this instances position in a sorted list of
            /// <see cref="NumericRange"/>s.</returns>
            int NumericSortValue
            {
                get
                {
                    if (!_numericSortValue.HasValue)
                    {
                        if (Numeric)
                        {
                            _numericSortValue = checked((int)StartNumber);
                        }
                        else
                        {
                            int parsedNumber;
                            Match match = _suffixedNumberRegex.Match(Text);
                            if (match != null &&
                                Int32.TryParse(match.Value, out parsedNumber))
                            {
                                _numericSortValue = parsedNumber;
                            }
                            else
                            {
                                _numericSortValue = Int32.MaxValue;
                            }
                        }
                    }

                    return _numericSortValue.Value;
                }
            }

            #endregion Private members
        }
    }
}
