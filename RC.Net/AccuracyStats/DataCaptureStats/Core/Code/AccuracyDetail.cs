using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.DataCaptureStats
{
    public enum AccuracyDetailLabel
    {
        ContainerOnly = 0,
        Expected = 1,
        Correct = 2,
        Incorrect = 3
    }
    /// <summary>
    /// Holds the label, path and numeric value of an accuracy detail
    /// </summary>
    public class AccuracyDetail
    {
        public AccuracyDetail(AccuracyDetailLabel label, string path, int value)
        {
            Label = label;
            Path = path;
            Value = value;
        }

        /// <summary>
        /// Gets the <see cref="AccuracyDetailLabel"/> of this instance
        /// </summary>
        public AccuracyDetailLabel Label { get; private set; }

        /// <summary>
        /// Gets the path of the attribute this detail is based on
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Gets or sets the numeric value of this detail
        /// </summary>
        public int Value { get; set; }

        #region Overrides

        /// <summary>
        /// Whether this instance is equal to another.
        /// </summary>
        /// <param name="obj">The instance to compare with</param>
        /// <returns><see langword="true"/> if this instance has equal property values, else <see langword="false"/></returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as AccuracyDetail;
            if (other == null
                || other.Label != Label
                || other.Path != Path
                || other.Value != Value)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the hash code for this object
        /// </summary>
        /// <returns>The hash code for this object</returns>
        public override int GetHashCode()
        {
            return HashCode.Start
                .Hash(Label)
                .Hash(Path)
                .Hash(Value);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "{0}|{1}|{2}", Label, Path, Value);
        }

        #endregion Overrides
    }
}
