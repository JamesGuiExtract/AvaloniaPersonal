using System;

namespace Extract.Utilities
{
    /// <summary>
    /// Event arguments class for a value out of range event.
    /// </summary>
    public class ValueOutOfRangeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        public double Value { get; private set; }

        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        public double MinimumValue { get; private set; }

        /// <summary>
        /// Gets the maximum value.
        /// </summary>
        public double MaximumValue { get; private set; }

        /// <summary>
        /// Gets the closest valid value to the value that is outside of the range.
        /// </summary>
        public double ClosestValidValue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueOutOfRangeEventArgs"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="maximum">The maximum value.</param>
        public ValueOutOfRangeEventArgs(double value, double minimum, double maximum)
        {
            Value = value;
            MinimumValue = minimum;
            MaximumValue = maximum;

            ClosestValidValue = value < minimum ? minimum : maximum;
        }
    }
}
