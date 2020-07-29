using Extract.Licensing.Internal;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.FAMDBCounterManager
{
    /// <summary>
    /// Manages readable values associated with the values of an enum.
    /// <para><b>Note</b></para>
    /// This class is a modified copy of Extract.Utilities.ReadableEnumData. This project is not
    /// linked to Extract.Utilities to avoid COM dependencies.
    /// </summary>
    /// <typeparam name="T">The enum for which readable values are to be maintained.</typeparam>
    internal class ReadableEnumData<T> where T : struct
    {
        #region Fields

        /// <summary>
        /// Maps the enum values to readable values.
        /// </summary>
        ConcurrentDictionary<T, string> _valueStringDictionary
            = new ConcurrentDictionary<T, string>();

        /// <summary>
        /// Maps the readable values to enum values.
        /// </summary>
        ConcurrentDictionary<string, T> _stringValueDictionary
            = new ConcurrentDictionary<string, T>();

        #endregion Fields

        /// <summary>
        /// Assigns a readable value to the specified enum <see paramref="value"/>.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <param name="readableValue">The readable value to associate with the enum value.
        /// </param>
        /// <throws><see cref="ExtractException"/> if either the <see paramref="value"/> or the
        /// <see paramref="readableValue"/> has been otherwise assigned.</throws>
        public void SetReadableValue(T value, string readableValue)
        {
            string existingReadableValue;
            if (_valueStringDictionary.TryGetValue(value, out existingReadableValue))
            {
                UtilityMethods.Assert(existingReadableValue == readableValue,
                    "Enum has already been assigned a different readable value");
            }

            T existingValue;
            if (_stringValueDictionary.TryGetValue(readableValue, out existingValue))
            {
                UtilityMethods.Assert(existingValue.Equals(value),
                    "String value has already been assigned to a different enum");
            }

            _valueStringDictionary[value] = readableValue;
            _stringValueDictionary[readableValue] = value;
        }

        /// <summary>
        /// Gets the readable value associated with the specified enum <see paramref="value"/>.
        /// </summary>
        /// <param name="value">The enum value for which the associated readable value is to be
        /// returned.</param>
        /// <returns>The readable value.</returns>
        /// <throws><see cref="ExtractException"/> if the <see paramref="value"/> has not been
        /// assigned a readable value.</throws>
        public string GetReadableValue(T value)
        {
            string readableValue;
            if (_valueStringDictionary.TryGetValue(value, out readableValue))
            {
                return readableValue;
            }

            throw new Exception("Enum has not been assigned a readable value");
        }

        /// <summary>
        /// Gets the readable value associated with the specified enum <see paramref="value"/>.
        /// </summary>
        /// <param name="value">The enum value for which the associated readable value is to be
        /// returned.</param>
        /// <param name="readableValue">The readable value.</param>
        /// <returns><see langword="true"/> if a readable value was found for the specific enum
        /// <see paramref="value"/>; otherwise, <see langword="false"/>.</returns>
        public bool TryGetReadableValue(T value, out string readableValue)
        {
            return _valueStringDictionary.TryGetValue(value, out readableValue);
        }

        /// <summary>
        /// Gets the enum value associated with the specified <see paramref="readableValue"/>.
        /// </summary>
        /// <param name="readableValue">The readable value for which the enum value is to be
        /// returned.</param>
        /// <returns>The enum value.</returns>
        /// <throws><see cref="ExtractException"/> if the <see paramref="readableValue"/> has not
        /// been assigned to an enum value.</throws>
        public T GetValueFromReadableValue(string readableValue)
        {
            T value;
            if (_stringValueDictionary.TryGetValue(readableValue, out value))
            {
                return value;
            }

            throw new Exception("Failed to look up enum value");
        }
    }

    /// <summary>
    /// Defines static helper methods and extension methods for assigning and using enum readable
    /// strings.
    /// <para><b>Note</b></para>
    /// This class is a modified copy of Extract.Utilities.ReadableEnumMethods. This project is not
    /// linked to Extract.Utilities to avoid COM dependencies.
    /// </summary>
    internal static class ReadableEnumMethods
    {
        #region Fields

        /// <summary>
        /// Maps enum types to the <see cref="ReadableEnumData{T}"/> instances they have been
        /// assigned.
        /// </summary>
        static ConcurrentDictionary<Type, object> RegisteredClasses =
            new ConcurrentDictionary<Type, object>();

        #endregion Fields

        #region Extension Methods

        /// <summary>
        /// Obtains the enum value associated with the specified <see paramref="readableValue"/>.
        /// </summary>
        /// <typeparam name="T">The enum type for which the readable value is assigned.</typeparam>
        /// <param name="readableValue">The readable value.</param>
        /// <returns>The associated enum value.</returns>
        public static T ToEnumValue<T>(this string readableValue) where T : struct
        {
            return GetReadableEnum<T>().GetValueFromReadableValue(readableValue);
        }

        /// <summary>
        /// Assigns a readable value to the specified enum <see paramref="value"/>.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <param name="readableValue">The readable value to associate with the enum value.
        /// </param>
        /// <throws><see cref="ExtractException"/> if either the <see paramref="value"/> or the
        /// <see paramref="readableValue"/> has been otherwise assigned.</throws>
        public static void SetReadableValue<T>(this T value, string readableValue) where T : struct
        {
            GetReadableEnum<T>().SetReadableValue(value, readableValue);
        }

        /// <summary>
        /// Gets the readable value associated with the specified enum <see paramref="value"/>.
        /// </summary>
        /// <typeparam name="T">The enum type for which the readable value is assigned.</typeparam>
        /// <param name="value">The enum value for which the associated readable value is to be
        /// returned.</param>
        /// <returns>The readable value.</returns>
        /// <throws><see cref="ExtractException"/> if the <see paramref="value"/> has not been
        /// assigned a readable value.</throws>
        public static string ToReadableValue<T>(this T value) where T : struct
        {
            return GetReadableEnum<T>().GetReadableValue(value);
        }

        /// <summary>
        /// Gets the readable value associated with the specified enum <see paramref="value"/>.
        /// </summary>
        /// <typeparam name="T">The enum type for which the readable value is assigned.</typeparam>
        /// <param name="value">The enum value for which the associated readable value is to be
        /// returned.</param>
        /// <param name="readableValue">The readable value.</param>
        /// <returns><see langword="true"/> if a readable value was found for the specific enum
        /// <see paramref="value"/>; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetReadableValue<T>(this T value, out string readableValue)
            where T : struct
        {
            return GetReadableEnum<T>().TryGetReadableValue(value, out readableValue);
        }

        /// <summary>
        /// Populates the combo box with the readable values for the enum type.
        /// </summary>
        /// <typeparam name="T">The enum type for which the combo box is to be populated.</typeparam>
        /// <param name="comboBox">The combo box.</param>
        /// <param name="ignoreUnassignedValues"><see langword="true"/> to leave any enum values
        /// without readable values assigned out of the combo box, <see langword="false"/> to throw
        /// an exception if any values are missing readable values.</param>
        // FXCop suggests that explicitly providing the type for generic method is too difficult to
        // understand, but there is no need for a parameter of type T here.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static void InitializeWithReadableEnum<T>(this ComboBox comboBox,
            bool ignoreUnassignedValues) where T : struct
        {
            // Keep track of the currently selected item.
            string selectedText = comboBox.Text;

            comboBox.Items.Clear();

            foreach (T value in typeof(T).GetEnumValues())
            {
                string readableValue;
                if (ignoreUnassignedValues && !value.TryGetReadableValue(out readableValue))
                {
                    continue;
                }
                else
                {
                    readableValue = value.ToReadableValue();
                }

                comboBox.Items.Add(value.ToReadableValue());
            }

            // Attempt to re-select the previously selected item if it is still present in the
            // combo box.
            if (!string.IsNullOrWhiteSpace(selectedText) &&
                comboBox.Items.Contains(selectedText))
            {
                comboBox.Text = selectedText;
            }
        }

        #endregion Extension Methods

        #region Private Members

        /// <summary>
        /// Gets the better enum.
        /// </summary>
        /// <typeparam name="T">The enum type for which a <see cref="ReadableEnumData{T}"/> instance
        /// is needed.</typeparam>
        /// <returns></returns>
        static ReadableEnumData<T> GetReadableEnum<T>() where T : struct
        {
            object registeredObject;
            if (!RegisteredClasses.TryGetValue(typeof(T), out registeredObject))
            {
                registeredObject = new ReadableEnumData<T>();
                RegisteredClasses[typeof(T)] = registeredObject;
            }

            return (ReadableEnumData<T>)registeredObject;
        }

        #endregion Private Members
    }
}
