using Extract.Licensing;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.Utilities
{
    /// <summary>
    /// Manages readable values associated with the values of an enum.
    /// </summary>
    /// <typeparam name="T">The enum for which readable values are to be maintained.</typeparam>
    internal class ReadableEnumData<T> where T : struct
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ReadableEnumData<T>).ToString();

        #endregion Constants

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
        /// Initializes a new instance of the <see cref="ReadableEnumData&lt;T&gt;"/> class.
        /// </summary>
        public ReadableEnumData()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI32716", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32717");
            }
        }

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
            try
            {
                string existingReadableValue;
                if (_valueStringDictionary.TryGetValue(value, out existingReadableValue))
                {
                    if (existingReadableValue != readableValue)
                    {
                        ExtractException ee = new ExtractException("ELI32718",
                            "Enum has already been assinged a different readable value.");
                        ee.AddDebugData("Existing string", existingReadableValue, false);
                        ee.AddDebugData("New string", readableValue, false);
                        throw ee;
                    }
                }

                T existingValue;
                if (_stringValueDictionary.TryGetValue(readableValue, out existingValue))
                {
                    if (!existingValue.Equals(value))
                    {
                        ExtractException ee = new ExtractException("ELI32719",
                            "String value has already been assinged to a different enum.");
                        ee.AddDebugData("Existing string", existingReadableValue, false);
                        ee.AddDebugData("New string", readableValue, false);
                        throw ee;
                    }
                }

                _valueStringDictionary[value] = readableValue;
                _stringValueDictionary[readableValue] = value;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32720");
            }
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
            try
            {
                string readableValue;
                if (_valueStringDictionary.TryGetValue(value, out readableValue))
                {
                    return readableValue;
                }

                ExtractException ee = new ExtractException("ELI32721",
                    "Enum has not been assigned a readable value.");
                ee.AddDebugData("Type", typeof(T).ToString(), false);
                ee.AddDebugData("Value", value.ToString(), false);
                throw ee;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32722");
            }
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

            ExtractException ee = new ExtractException("ELI32723", "Failed to look up enum value.");
            ee.AddDebugData("Type", typeof(T).ToString(), false);
            ee.AddDebugData("Description", readableValue, false);
            throw ee;
        }
    }

    /// <summary>
    /// Defines static helper methods and extension methods for assigning and using enum readable
    /// strings.
    /// </summary>
    public static class ReadableEnumMethods
    {
        #region Fields

        /// <summary>
        /// Maps enum types to the <see cref="ReadableEnumData{T}"/> instances they have been
        /// assigned.
        /// </summary>
        static ConcurrentDictionary<Type, object> RegisteredClasses =
            new ConcurrentDictionary<Type, object>();

        #endregion Fields

        #region Methods

        /// <summary>
        /// Obtains the enum value associated with the specified <see paramref="readableValue"/>.
        /// </summary>
        /// <typeparam name="T">The enum type for which the readable value is assigned.</typeparam>
        /// <param name="readableValue">The readable value.</param>
        /// <returns>The enum value.</returns>
        public static T FromReadableValue<T>(string readableValue) where T : struct
        {
            try
            {
                return GetReadableEnum<T>().GetValueFromReadableValue(readableValue);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32726");
            }
        }

        #endregion Methods

        #region Extension Methods

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
            try
            {
                GetReadableEnum<T>().SetReadableValue(value, readableValue);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32724");
            }
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
            try
            {
                return GetReadableEnum<T>().GetReadableValue(value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32725");
            }
        }

        /// <summary>
        /// Populates the combo box with the readable values for the enum type.
        /// </summary>
        /// <typeparam name="T">The enum type for which the combo box is to be populated.</typeparam>
        /// <param name="comboBox">The combo box.</param>
        // FXCop suggests that explicity providing the type for generic method is too difficult to
        // understand, but there is no need for a parameter of type T here.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static void InitializeWithReadableEnum<T>(this ComboBox comboBox) where T : struct
        {
            try
            {
                comboBox.Items.Clear();

                foreach (T value in typeof(T).GetEnumValues())
                {
                    comboBox.Items.Add(value.ToReadableValue());
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32727");
            }
        }

        /// <summary>
        /// Gets the enum value associated with the readable value currently selected in the
        /// <see cref="ComboBox"/>.
        /// </summary>
        /// <typeparam name="T">The enum type for which the readable value is assigned.</typeparam>
        /// <param name="comboBox">The <see cref="ComboBox"/>.</param>
        /// <returns>The readable value.</returns>
        /// <throws><see cref="ExtractException"/> if the <see cref="ComboBox"/> is not a readable
        /// string assigned to one of the enums values.</throws>
        // Since this method acts explicity on a ComboBox, it is appropriate to name the parameter
        // comboBox rather than control.
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static T ToReadableEnumValue<T>(this ComboBox comboBox) where T : struct
        {
            try
            {
                return FromReadableValue<T>(comboBox.Text);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32728");
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
