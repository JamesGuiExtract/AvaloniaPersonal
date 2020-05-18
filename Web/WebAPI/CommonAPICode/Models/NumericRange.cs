using Extract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebAPI.Models
{
    /// <summary>
    /// Represents a range of numeric values
    /// </summary>
    public class NumericRange
    {
        /// <summary>Initializes a new instance of the <see cref="NumericRange"/> class.
        /// </summary>
        /// <param name="startNumber">Gets the first number in the range.</param>
        /// <param name="endNumber">Gets the last number in the range.</param>
        public NumericRange(int startNumber, int? endNumber = null)
        {
            StartNumber = startNumber;
            EndNumber = endNumber ?? startNumber;
        }

        /// <summary>Initializes a new instance of the <see cref="NumericRange"/> class.
        /// </summary>
        /// <param name="range">The text value that represents the range.</param>
        public NumericRange(string range)
        {
            try
            {
                range = range.Trim();

                var values = range.Split(new[] { '-' }, StringSplitOptions.None);
                ExtractException.Assert("ELI46607", "Too many hyphens", values.Length <= 2);

                if (!string.IsNullOrWhiteSpace(values[0]))
                {
                    StartNumber = int.Parse(values[0]);
                    if (values.Length == 1)
                    {
                        EndNumber = StartNumber;
                    }
                }

                if (values.Length > 1 && !string.IsNullOrWhiteSpace(values[1]))
                {
                    EndNumber = int.Parse(values[1]);
                }

                ExtractException.Assert("ELI46609", "Missing value", StartNumber != null || EndNumber != null);
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI46611", "Unable to parse range.", ex);
                ee.AddDebugData("Range", range, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the first number in the range.
        /// </summary>
        public int? StartNumber
        {
            get;
        }

        /// <summary>
        /// Gets the last number in the range.
        /// </summary>
        public int? EndNumber
        {
            get;
        }

        /// <summary>
        /// Parses the specified text into a series of ranges.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static IEnumerable<NumericRange> Parse(string text)
        {
            try
            {
                return text.Split(',')
                    .Where(range => !string.IsNullOrWhiteSpace(range))
                    .Select(range => new NumericRange(range));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46602");
            }
        }

        /// <summary>
        /// Determines whether otherNumber is contained in the range.
        /// </summary>
        public bool Contains(int otherNumber)
        {
            return ((StartNumber == null || otherNumber >= StartNumber.Value) && 
                    (EndNumber == null || otherNumber <= EndNumber.Value));
        }

        /// <summary>
        /// Returns an enumeration of the integers represented by this instance.
        /// </summary>
        public IEnumerable<int> ToEnumerable()
        {
            return Enumerable.Range(StartNumber.Value, 
                (EndNumber ?? StartNumber).Value - StartNumber.Value + 1);
        }
    }
}
