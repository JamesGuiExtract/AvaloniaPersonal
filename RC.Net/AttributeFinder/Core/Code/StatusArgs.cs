using Accord.Math; using Accord.Statistics.Analysis;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using ComAttribute = UCLID_AFCORELib.Attribute;
using System.Threading;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Class to pass progress status info
    /// </summary>
    public class StatusArgs
    {
        /// <summary>
        /// Report number of files found for this update
        /// </summary>
        public int Int32Value { get; set; }

        /// <summary>
        /// Report current task name
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// Status message
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// Double values
        /// </summary>
        public IEnumerable<double> DoubleValues
        {
            get
            {
                return _doubleValues;
            }
            set
            {
                _doubleValues = value ?? Enumerable.Empty<double>();
            }
        }
        private IEnumerable<double> _doubleValues = Enumerable.Empty<double>();

        /// <summary>
        /// Whether to replace the last task status if the same name
        /// </summary>
        public bool ReplaceLastStatus { get; set; }

        /// <summary>
        /// Gets or sets the indent level
        /// </summary>
        public int Indent { get; set; }

        /// <summary>
        /// Gets or sets the size of an indent level
        /// </summary>
        public static int IndentSize
        {
            get
            {
                return _indentSize;
            }
            set
            {
                _indentSize = value;
            }
        }
        private static int _indentSize = 2;

        /// <summary>
        /// Combine values from other <see cref="StatusArgs"/> with this one
        /// </summary>
        /// <param name="other">The other instance to combine values from</param>
        public void Combine(StatusArgs other)
        {
            try
            {
                Int32Value += other.Int32Value;
                if (DoubleValues.Any())
                {
                    // Evaluate immediately to prevent SO exception
                    // https://extract.atlassian.net/browse/ISSUE-15268
                    DoubleValues = DoubleValues
                        .Zip(other.DoubleValues, (a, b) => a + b)
                        .ToList();
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39875");
            }
        }

        /// <summary>
        /// Combines data values with message to produce formatted string
        /// </summary>
        /// <returns>The message formatted with data values</returns>
        public string GetFormattedValue(bool indent = true)
        {
            string statusMessage = null;
            try
            {
                statusMessage = indent
                    ? new string(' ', Indent * IndentSize) + StatusMessage
                    : StatusMessage;
                if (DoubleValues.Count() > 1)
                {
                    object [] args = DoubleValues.Cast<object>().ToArray();
                    return string.Format(CultureInfo.CurrentCulture, statusMessage, args);
                }
                else if (DoubleValues.Count() == 1)
                {
                    return string.Format(CultureInfo.CurrentCulture, statusMessage, DoubleValues.First());
                }
                else
                {
                    return string.Format(CultureInfo.CurrentCulture, statusMessage, Int32Value);
                }
            }
            catch (System.FormatException)
            {
                return statusMessage ?? "";
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39876");
            }
        }
    }
}
