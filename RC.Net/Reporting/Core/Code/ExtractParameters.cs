using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Extract.Reporting
{
    /// <summary>
    /// An interface that all extract parameter controls must implement
    /// </summary>
    public interface IExtractParameterControl
    {
        /// <summary>
        /// Applies the current control values to the underlying Extract parameter.
        /// </summary>
        void Apply();
    }

    /// <summary>
    /// An interface that all extract report parameters must implement.
    /// </summary>
    public interface IExtractReportParameter
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <returns>The parameter name.</returns>
        string ParameterName
        {
            get;
        }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <value>The value of the parameter.</value>
        object Value
        {
            get;
            set;
        }

        /// <summary>
        /// Return <see langword="true"/> if there is a current value set.
        /// </summary>
        /// <returns><see langword="true"/> if there is a current value and
        /// <see langword="false"/> otherwise.</returns>
        bool HasValueSet();

        /// <summary>
        /// Sets the value of the parameter with the <see langword="string"/>
        /// <see paramref="value"/>.
        /// </summary>
        /// <param name="value">The <see langword="string"/> that specifies the new value of the
        /// parameter. Must be a value that can be converted to the type of the current parameter.
        /// </param>
        void SetValueFromString(string value);

        /// <summary>
        /// Writes the report parameter object to the <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to write the object to.</param>
        void WriteToXml(XmlWriter writer);
    }

    /// <summary>
    /// Abstract base class for all report parameters
    /// </summary>
    /// <typeparam name="T">The type of data the report parameter represents.</typeparam>
    public abstract class ExtractReportParameter<T> : IExtractReportParameter
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(ExtractReportParameter<T>).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The current value of the parameter.
        /// </summary>
        private T _currentValue;

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        private string _parameterName;

        /// <summary>
        /// Stores whether there is a currently set value for the parameter.
        /// </summary>
        private bool _hasValueSet;

        #endregion Fields

        /// <overloads>Initializes a new <see cref="ExtractReportParameter{T}"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="ExtractReportParameter{T}"/> class.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        protected ExtractReportParameter(string parameterName)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23809",
					_OBJECT_NAME);

                _parameterName = parameterName;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23758", ex);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="ExtractReportParameter{T}"/> class.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="defaultValue">Sets the default value for the
        /// report parameter.</param>
        protected ExtractReportParameter(string parameterName, T defaultValue)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23810",
					_OBJECT_NAME);

                _parameterName = parameterName;
                _currentValue = defaultValue;
                _hasValueSet = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23703", ex);
            }
        }

        /// <summary>
        /// Gets or sets the current report parameter's value.
        /// </summary>
        virtual public T ParameterValue
        {
            get
            {
                return _currentValue;
            }
            set
            {
                try
                {
                    _currentValue = value;
                    _hasValueSet = true;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI23704", ex);
                }
            }
        }

        #region IReportParameter Members

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <returns>The name of the parameter.</returns>
        public string ParameterName
        {
            get
            {
                return _parameterName;
            }
        }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <value>The value of the parameter.</value>
        public virtual object Value
        {
            get
            {
                return ParameterValue;
            }

            set
            {
                ParameterValue = (T)value;
            }
        }

        /// <summary>
        /// Writes the <see cref="ExtractReportParameter{T}"/> to the specified
        /// <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to write the
        /// parameter to.</param>
        public abstract void WriteToXml(XmlWriter writer);

        /// <summary>
        /// Indicates whether the <see cref="ExtractReportParameter{T}"/> has had
        /// its value set yet.  <see langword="true"/> if the value has been set;
        /// <see langword="false"/> if it has not been set.
        /// </summary>
        /// <returns><see langword="true"/> if the parameter value has been set;
        /// <see langword="false"/> if it has not been set.</returns>
        public virtual bool HasValueSet()
        {
            return _hasValueSet;
        }

        /// <summary>
        /// Sets the value of the parameter with the <see langword="string"/>
        /// <see paramref="value"/>.
        /// </summary>
        /// <param name="value">The <see langword="string"/> that specifies the new value of the
        /// parameter. Must be a value that can be converted to type <see typeref="T"/>.</param>
        public abstract void SetValueFromString(string value);

        #endregion IReportParameter Members
    }

    /// <summary>
    /// Represents a text report parameter. 
    /// </summary>
    public class TextParameter : ExtractReportParameter<string>
    {
        /// <summary>
        /// Stores whether there is a currently set value for the parameter.
        /// </summary>
        private bool _hasValueSet;

        /// <overloads>Initializes a new <see cref="TextParameter"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="TextParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        public TextParameter(string name)
            : base(name)
        {
            _hasValueSet = base.HasValueSet();
        }

        /// <summary>
        /// Initializes a new <see cref="TextParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="defaultValue">Sets the default value for
        /// the report parameter.</param>
        public TextParameter(string name, string defaultValue)
            : base(name, defaultValue)
        {
            _hasValueSet = !string.IsNullOrEmpty(defaultValue);
        }

        /// <summary>
        /// Gets or sets the current report parameter's value.
        /// </summary>
        /// <returns>The current report parameter's value.</returns>
        /// <value>The current report parameter's value.</value>
        public override string ParameterValue
        {
            get
            {
                return base.ParameterValue;
            }
            set
            {
                base.ParameterValue = value;
                _hasValueSet = !string.IsNullOrEmpty(value);
            }
        }

        /// <summary>
        /// Indicates whether the <see cref="TextParameter"/> has had
        /// its value set yet.  <see langword="true"/> if the value has been set;
        /// <see langword="false"/> if it has not been set.
        /// </summary>
        /// <returns><see langword="true"/> if the parameter value has been set;
        /// <see langword="false"/> if it has not been set.</returns>
        public override bool HasValueSet()
        {
            return _hasValueSet;
        }

        /// <summary>
        /// Sets the value of the parameter with the <see langword="string"/>
        /// <see paramref="value"/>.
        /// </summary>
        /// <param name="value">The <see langword="string"/> that specifies the new value of the
        /// parameter.</param>
        public override void SetValueFromString(string value)
        {
            try
            {
                ParameterValue = value;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36061");
            }
        }

        /// <summary>
        /// Writes the <see cref="TextParameter"/> to the specified
        /// <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to write the
        /// parameter to.</param>
        public override void WriteToXml(XmlWriter writer)
        {
            try
            {
                writer.WriteStartElement("TextParameter");
                writer.WriteAttributeString("Name", this.ParameterName);
                writer.WriteAttributeString("Default", this.ParameterValue);
                writer.WriteEndElement();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23705", ex);
            }
        }
    }

    /// <summary>
    /// Represents a numeric report parameter.
    /// </summary>
    public class NumberParameter : ExtractReportParameter<double>
    {
        /// <overloads>Initializes a new <see cref="NumberParameter"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="NumberParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        public NumberParameter(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="NumberParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="defaultValue">Sets the default value for
        /// the report parameter.</param>
        public NumberParameter(string name, double defaultValue)
            : base(name, defaultValue)
        {
        }

        /// <summary>
        /// Sets the value of the parameter with the <see langword="string"/>
        /// <see paramref="value"/>.
        /// </summary>
        /// <param name="value">The <see langword="string"/> that specifies the new value of the
        /// parameter. Must be a value that can be converted to type <see langword="double"/>.
        /// </param>
        public override void SetValueFromString(string value)
        {
            try
            {
                ParameterValue = Convert.ToDouble(value, CultureInfo.CurrentCulture);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36062");
            }
        }

        /// <summary>
        /// Writes the <see cref="NumberParameter"/> to the specified
        /// <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to write the
        /// parameter to.</param>
        public override void WriteToXml(XmlWriter writer)
        {
            try
            {
                writer.WriteStartElement("NumberParameter");
                writer.WriteAttributeString("Name", this.ParameterName);
                writer.WriteAttributeString("Default",
                    this.ParameterValue.ToString(CultureInfo.CurrentCulture));
                writer.WriteEndElement();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23706", ex);
            }
        }
    }

    /// <summary>
    /// Represents a date report parameter.
    /// </summary>
    public class DateParameter : ExtractReportParameter<DateTime>
    {
        /// <overloads>Initializes a new <see cref="DateParameter"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="DateParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        public DateParameter(string name)
            : this(name, DateTime.Now, true)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="DateParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="defaultValue">Sets the default value for
        /// the report parameter.</param>
        /// <param name="showTime"><see langword="true"/> to show the date and time;
        /// <see langword="false"/> to show only the date.</param>
        public DateParameter(string name, DateTime defaultValue, bool showTime)
            : base(name, defaultValue)
        {
            ShowTime = showTime;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the time.
        /// </summary>
        /// <value><see langword="true"/> to show the date and time; <see langword="false"/> to show
        /// only the date.
        /// </value>
        public bool ShowTime
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the value of the parameter with the <see langword="string"/>
        /// <see paramref="value"/>.
        /// </summary>
        /// <param name="value">The <see langword="string"/> that specifies the new value of the
        /// parameter. Must be a value that can be converted to type <see cref="DateTime"/>.</param>
        public override void SetValueFromString(string value)
        {
            try
            {
                ParameterValue = Convert.ToDateTime(value, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36063");
            }
        }

        /// <summary>
        /// Writes the <see cref="DateParameter"/> to the specified
        /// <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to write the
        /// parameter to.</param>
        public override void WriteToXml(XmlWriter writer)
        {
            try
            {
                writer.WriteStartElement("DateParameter");
                writer.WriteAttributeString("Name", this.ParameterName);
                writer.WriteAttributeString("Default",
                    this.ParameterValue.ToString("g", CultureInfo.CurrentCulture));
                writer.WriteEndElement();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23707", ex);
            }
        }
    }

    /// <summary>
    /// Specifies a range to use when computing a <see cref="DateRangeParameter"/> value.
    /// </summary>
    public enum DateRangeValue
    {
        /// <summary>
        /// Include all dates in the database (no WHERE clause in the query).
        /// </summary>
        All = 0,

        /// <summary>
        /// Include all dates in the previous week (if today = Wed. 12/24/2008 then
        /// the range will be 12/14/2008 - 12/20/2008).
        /// </summary>
        PreviousWeek = 1,

        /// <summary>
        /// Include all dates in the previous month (if today = Wed. 12/24/2008 then
        /// the range will be 11/01/2008 - 11/30/2008).
        /// </summary>
        PreviousMonth = 2,

        /// <summary>
        /// Include all dates in the previous year (if tody = Wed. 12/24/2008 then
        /// the range will be 01/01/2007 - 12/31/2007)
        /// </summary>
        PreviousYear = 3,

        /// <summary>
        /// Include all dates in the last 30 days (if today = Wed. 12/24/2008 then
        /// the range will be 11/25/2008 - 12/24/2008).
        /// </summary>
        Last30Days = 4,

        /// <summary>
        /// Include all dates in the last 7 days (if today = Wed. 12/24/2008 then
        /// the range will be 12/18/2008 - 12/24/2008).
        /// </summary>
        Last7Days = 5,

        /// <summary>
        /// Include all records in the last hour (if now = 12/24/2008 15:31:00 then
        /// the range will be 12/24/2008 14:31:00 - 12/24/2008 15:31:00)
        /// </summary>
        LastHour = 6,

        /// <summary>
        /// Include all records in the last two hours (if now = 12/24/2008 15:31:00 then
        /// the range will be 12/24/2008 13:31:00 - 12/24/2008 15:31:00)
        /// </summary>
        Last2Hours = 7,

        /// <summary>
        /// Include all records in the last five hours (if now = 12/24/2008 15:31:00 then
        /// the range will be 12/24/2008 10:31:00 - 12/24/2008 15:31:00)
        /// </summary>
        Last5Hours = 8,

        /// <summary>
        /// Include all records in the last ten hours (if now = 12/24/2008 15:31:00 then
        /// the range will be 12/24/2008 05:31:00 - 12/24/2008 15:31:00)
        /// </summary>
        Last10Hours = 9,

        /// <summary>
        /// Include all records in the last 24 hours
        /// </summary>
        Last24Hours = 10,

        /// <summary>
        /// Include all records in the last 1 minute
        /// </summary>
        Last1Minute = 11,

        /// <summary>
        /// Include all records in the last ten hours
        /// </summary>
        Last3Minutes = 12,

        /// <summary>
        /// Include all records in the last ten hours
        /// </summary>
        Last10Minutes = 13,

        /// <summary>
        /// Include all records in the last ten hours
        /// </summary>
        Last30Minutes = 14,
        
        /// <summary>
        /// Sun. through Sat. of the current week (if today = Wed. 12/24/2008 then
        /// the range will be 12/21/2008 - 12/27/2008).
        /// </summary>
        CurrentWeek = 15,

        /// <summary>
        /// Include all dates in the current month (if today = Wed. 12/24/2008 then
        /// the range will be 12/01/2008 - 12/31/2008).
        /// </summary>
        CurrentMonth = 16,

        /// <summary>
        /// Include all dates in the current year (if today = Wed. 12/24/2008 then
        /// the range will be 01/01/2008 - 12/31/2008).
        /// </summary>
        CurrentYear = 17,

        /// <summary>
        /// Includes only records for today (if today = Wed. 12/24/2008 then
        /// the range will be 12/24/2008 - 12/24/2008).
        /// </summary>
        Today = 18,

        /// <summary>
        /// Includes only records for yesterday (if today = Wed. 12/24/2008 then
        /// the range will be 12/23/2008 - 12/23/2008).
        /// </summary>
        Yesterday = 19,

        /// <summary>
        /// A custom date range, user must specify min and max values for the range.
        /// </summary>
        Custom = 20,
    }

    /// <summary>
    /// A helper class for parsing and returning a human readable string version of
    /// the <see cref="DateRangeValue"/> enumeration.
    /// </summary>
    public static class DateRangeValueTypeHelper
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(DateRangeValueTypeHelper).ToString();

        #endregion Constants

        #region Fields

        private static Dictionary<DateRangeValue, string> _dateRangeAsString =
            new Dictionary<DateRangeValue, string>(Enum.GetValues(typeof(DateRangeValue)).Length);

        private static List<string> _enumStrings;

        #endregion Fields

        /// <summary>
        /// Returns a <see cref="string"/> array containing all of the
        /// <see cref="DateRangeValue"/> enums in a more human readable format.
        /// </summary>
        /// <returns>A <see cref="string"/> array of human readable
        /// strings associated with the <see cref="DateRangeValue"/> enum.</returns>
        public static string[] GetHumanReadableStrings()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23811",
					_OBJECT_NAME);

                if (_enumStrings == null)
                {
                    // Get the array of values
                    Array values = Enum.GetValues(typeof(DateRangeValue));

                    // Create the list to hold the values
                    _enumStrings = new List<string>(values.Length);
                    foreach (DateRangeValue value in values)
                    {
                        _enumStrings.Add(GetHumanReadableString(value));
                    }
                }

                return _enumStrings.ToArray();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23812", ex);
            }
        }

        /// <summary>
        /// Returns a human readable version of the specified enum value.
        /// </summary>
        /// <param name="dateRangeValue">The value to get a string for.</param>
        /// <returns>A human readable string version of the specified enum value.</returns>
        public static string GetHumanReadableString(DateRangeValue dateRangeValue)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23813",
					_OBJECT_NAME);

                // First check if the string has been computed yet, if not then
                // compute the string
                string value;
                if (!_dateRangeAsString.TryGetValue(dateRangeValue, out value))
                {
                    // Get the string from the enum (ensure that the string is at least 1 character)
                    value = dateRangeValue.ToString();
                    ExtractException.Assert("ELI23814", "Enum string is too short", value.Length > 0);

                    // Ensure the first character is upper case and then split the string
                    // based on case change and digits in the remaining string
                    StringBuilder sb = new StringBuilder();
                    sb.Append(char.ToUpper(value[0], CultureInfo.CurrentCulture));

                    // Now split it at the case changes or digit changes (starting at the second
                    // character since the first should always be upper case)
                    for (int i = 1; i < value.Length; i++)
                    {
                        char temp = value[i];
                        if (char.IsLower(temp))
                        {
                            sb.Append(temp);
                        }
                        else if (char.IsUpper(temp))
                        {
                            sb.Append(" ");
                            sb.Append(char.ToLower(temp, CultureInfo.CurrentCulture));
                        }
                        else if (char.IsDigit(temp))
                        {
                            sb.Append(" ");

                            // Keep checking for more digits and adding the current digit
                            // to the list
                            do
                            {
                                sb.Append(value[i++]);
                            }
                            while (i < value.Length && char.IsDigit(value[i]));

                            // Need to decrement i by 1 since it will be incremented in the
                            // loop iteration
                            i--;
                        }
                    }

                    // Get the string from the string builder and store it in the collection
                    value = sb.ToString();
                    _dateRangeAsString.Add(dateRangeValue, value);
                }

                // Return the value
                return value;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23815", ex);
            }
        }

        /// <summary>
        /// Parses the enum string (either human readable or regular enum value) and returns the
        /// associated <see cref="DateRangeValue"/>.
        /// </summary>
        /// <param name="enumValue">The string to parse.</param>
        /// <returns>The associated <see cref="DateRangeValue"/>.</returns>
        public static DateRangeValue ParseString(string enumValue)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23816",
					_OBJECT_NAME);

                // Remove the spaces from the string
                string value = enumValue.Replace(" ", "");

                return (DateRangeValue)Enum.Parse(typeof(DateRangeValue), value, true);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23817", ex);
            }
        }
    }

    /// <summary>
    /// Represents a date range report parameter.
    /// </summary>
    public class DateRangeParameter : ExtractReportParameter<DateRangeValue>
    {
        /// <summary>
        /// The minimum (beginning) date for the date range.
        /// </summary>
        DateTime _min = DateTime.Now;

        /// <summary>
        /// The maximum (ending) date for the date range.
        /// </summary>
        DateTime _max = DateTime.Now;

        /// <overloads>Initializes a new <see cref="DateRangeParameter"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="DateRangeParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        public DateRangeParameter(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="DateRangeParameter"/> class.
        /// <para><b>Note:</b></para>
        /// This overload will automatically set the <see cref="DateRangeParameter.ParameterValue"/>
        /// property to "Custom".
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="min">The minimum (beginning) date for the range.</param>
        /// <param name="max">The maximum (ending) date for the range.</param>
        public DateRangeParameter(string name, DateTime min, DateTime max)
            : base(name, DateRangeValue.Custom)
        {
            try
            {
                if (min > max)
                {
                    _max = min;
                    _min = max;
                }
                else
                {
                    _min = min;
                    _max = max;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23759", ex);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="DateRangeParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="defaultValue">Sets the default value for
        /// the report parameter.</param>
        public DateRangeParameter(string name, DateRangeValue defaultValue)
            : base(name, defaultValue)
        {
            try
            {
                // Update the min and max range based on the default value
                UpdateMinAndMax();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23760", ex);
            }
        }

        /// <summary>
        /// Gets or sets the minimum (beginning) date.
        /// <returns>Minimum (beginning) date.</returns>
        /// <value>Set the minimum (beginning) date.  Note: This will also
        /// set the <see cref="DateRangeParameter.ParameterValue"/> to "Custom"</value>
        /// </summary>
        public DateTime Minimum
        {
            get
            {
                return _min;
            }
            set
            {
                base.ParameterValue = DateRangeValue.Custom;
                _min = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum (ending) date.
        /// <returns>Maximum (ending) date.</returns>
        /// <value>Set the maximum (ending) date.  Note: This will also
        /// set the <see cref="DateRangeParameter.ParameterValue"/> to "Custom"</value>
        /// </summary>
        public DateTime Maximum
        {
            get
            {
                return _max;
            }
            set
            {
                base.ParameterValue = DateRangeValue.Custom;
                _max = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DateRangeValue"/>.
        /// </summary>
        /// <returns>The current <see cref="DateRangeValue"/>.</returns>
        /// <value>The current <see cref="DateRangeValue"/>.</value>
        public override DateRangeValue ParameterValue
        {
            get
            {
                return base.ParameterValue;
            }
            set
            {
                // Set the value and then update the min and max values
                // Note: If value = Custom then min and max will not be changed.
                base.ParameterValue = value;
                UpdateMinAndMax();
            }
        }

        /// <summary>
        /// Updates the min and max values based on the current <see cref="DateRangeValue"/>
        /// setting.
        /// </summary>
        private void UpdateMinAndMax()
        {
            // Get the current time
            DateTime currentDateTime = DateTime.Now;

            switch (base.ParameterValue)
            {
                case DateRangeValue.All:
                    {
                        _min = new DateTime(1900, 1, 1, 0, 0, 0);
                        _max = new DateTime(2100, 12, 31, 23, 59, 59);
                    }
                    break;

                case DateRangeValue.PreviousWeek:
                    {
                        // Set the min to the first day of last week (Sunday)
                        _min = currentDateTime.Subtract(
                            new TimeSpan(((int)(currentDateTime.DayOfWeek)) + 7, 0, 0, 0));
                        _min = new DateTime(_min.Year, _min.Month, _min.Day, 0, 0, 0);

                        // Set the maximum to the last day of the week
                        _max = _min.AddDays(6);
                        _max = new DateTime(_max.Year, _max.Month, _max.Day, 23, 59, 59);
                    }
                    break;

                case DateRangeValue.PreviousMonth:
                    {
                        // Compute the previous month (if current month is January
                        // also need to subtract one from the year)
                        int year = (currentDateTime.Month == 1)
                            ? currentDateTime.Year - 1 : currentDateTime.Year;
                        int month = (currentDateTime.Month == 1) ? 12 : currentDateTime.Month - 1;

                        // Set min to the first day of previous month and
                        // max to last day of previous month
                        _min = new DateTime(year, month, 1, 0, 0, 0);
                        _max = new DateTime(year, month,
                            DateTime.DaysInMonth(year, month), 23, 59, 59);
                    }
                    break;

                case DateRangeValue.PreviousYear:
                    {
                        // Set min to first day of previous year and max to last
                        // day of last year
                        int year = currentDateTime.Year - 1;
                        _min = new DateTime(year, 1, 1, 0, 0, 0);
                        _max = new DateTime(year, 12, 31, 23, 59, 59);
                    }
                    break;

                case DateRangeValue.Last30Days:
                    {
                        // Set max to current time and min to 30 days prior
                        _min = currentDateTime.Subtract(new TimeSpan(30, 0, 0, 0));
                        _max = currentDateTime;
                    }
                    break;

                case DateRangeValue.Last7Days:
                    {
                        // Set max to current time and min to 7 days prior
                        _min = currentDateTime.Subtract(new TimeSpan(7, 0, 0, 0));
                        _max = currentDateTime;
                    }
                    break;

                case DateRangeValue.LastHour:
                    {
                        // Set max to current time and min to 1 hour previous
                        _min = currentDateTime.Subtract(new TimeSpan(1, 0, 0));
                        _max = currentDateTime;
                    }
                    break;

                case DateRangeValue.Last30Minutes:
                    {
                        // Set max to current time and min to 30 minutes previous
                        _min = currentDateTime.Subtract(new TimeSpan(0, 30, 0));
                        _max = currentDateTime;
                    }
                    break;

                case DateRangeValue.Last10Minutes:
                    {
                        // Set max to current time and min to 10 minutes previous
                        _min = currentDateTime.Subtract(new TimeSpan(0, 10, 0));
                        _max = currentDateTime;
                    }
                    break;

                case DateRangeValue.Last3Minutes:
                    {
                        // Set max to current time and min to 3 minutes previous
                        _min = currentDateTime.Subtract(new TimeSpan(0, 3, 0));
                        _max = currentDateTime;
                    }
                    break;

                case DateRangeValue.Last1Minute:
                    {
                        // Set max to current time and min to 1 minute previous
                        _min = currentDateTime.Subtract(new TimeSpan(0, 1, 0));
                        _max = currentDateTime;
                    }
                    break;

                case DateRangeValue.Last2Hours:
                    {
                        // Set max to current time and min to 2 hour previous
                        _min = currentDateTime.Subtract(new TimeSpan(2, 0, 0));
                        _max = currentDateTime;
                    }
                    break;

                case DateRangeValue.Last5Hours:
                    {
                        // Set max to current time and min to 5 hour previous
                        _min = currentDateTime.Subtract(new TimeSpan(5, 0, 0));
                        _max = currentDateTime;
                    }
                    break;

                case DateRangeValue.Last10Hours:
                    {
                        // Set max to current time and min to 10 hour previous
                        _min = currentDateTime.Subtract(new TimeSpan(10, 0, 0));
                        _max = currentDateTime;
                    }
                    break;

                case DateRangeValue.Last24Hours:
                    {
                        // Set max to current time and min to 24 hour previous
                        _min = currentDateTime.Subtract(new TimeSpan(24, 0, 0));
                        _max = currentDateTime;
                    }
                    break;

                case DateRangeValue.CurrentWeek:
                    {
                        // Set min to first day of the current week and max
                        // to last day of current week
                        _min = currentDateTime.Subtract(
                            new TimeSpan((int)(currentDateTime.DayOfWeek), 0, 0, 0));
                        _min = new DateTime(_min.Year, _min.Month, _min.Day, 0, 0, 0);
                        _max = _min.AddDays(6);
                        _max = new DateTime(_max.Year, _max.Month, _max.Day, 23, 59, 59);
                    }
                    break;

                case DateRangeValue.CurrentMonth:
                    {
                        // Set min to the first day of the current month and
                        // max to the last day of the current month
                        int year = currentDateTime.Year;
                        int month = currentDateTime.Month;
                        _min = new DateTime(year, month, 1, 0, 0, 0);
                        _max = new DateTime(year, month, DateTime.DaysInMonth(year, month),
                            23, 59, 59);
                    }
                    break;

                case DateRangeValue.CurrentYear:
                    {
                        // Set min to first day of the current year and max to last
                        // day of the current year
                        int year = currentDateTime.Year;
                        _min = new DateTime(year, 1, 1, 0, 0, 0);
                        _max = new DateTime(year, 12, 31, 23, 59, 59);
                    }
                    break;

                case DateRangeValue.Today:
                    {
                        // Set min and max to current time
                        _min = new DateTime(currentDateTime.Year, currentDateTime.Month,
                            currentDateTime.Day, 0, 0, 0);
                        _max = new DateTime(_min.Year, _min.Month, _min.Day, 23, 59, 59);
                    }
                    break;

                case DateRangeValue.Yesterday:
                    {
                        // Set min and max to yesterday
                        _min = currentDateTime.Subtract(new TimeSpan(1, 0, 0, 0));
                        _min = new DateTime(_min.Year, _min.Month, _min.Day, 0, 0, 0);
                        _max = new DateTime(_min.Year, _min.Month, _min.Day, 23, 59, 59);
                    }
                    break;

                case DateRangeValue.Custom:
                    // Do nothing
                    break;

                default:
                    // Shouldn't get here
                    ExtractException.ThrowLogicException("ELI23708");
                    break;
            }
        }

        /// <summary>
        /// Sets the value of the parameter with the <see langword="string"/>
        /// <see paramref="value"/>.
        /// </summary>
        /// <param name="value">The <see langword="string"/> that specifies the new value of the
        /// parameter. Must be a value that can be converted to type <see cref="DateRangeValue"/>.
        /// </param>
        public override void SetValueFromString(string value)
        {
            try
            {
                ParameterValue = DateRangeValueTypeHelper.ParseString(value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36064");
            }
        }

        /// <summary>
        /// Writes the <see cref="DateRangeParameter"/> to the specified
        /// <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to write the
        /// parameter to.</param>
        public override void WriteToXml(XmlWriter writer)
        {
            try
            {
                writer.WriteStartElement("DateRangeParameter");
                writer.WriteAttributeString("Name", this.ParameterName);
                writer.WriteAttributeString("Default", this.ParameterValue.ToString());
                if (this.ParameterValue == DateRangeValue.Custom)
                {
                    writer.WriteAttributeString("Min", _min.ToString("g",
                        CultureInfo.CurrentCulture));
                    writer.WriteAttributeString("Max", _max.ToString("g",
                        CultureInfo.CurrentCulture));
                }
                writer.WriteEndElement();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23709", ex);
            }
        }
    }

    /// <summary>
    /// Represents a value list report parameter.
    /// </summary>
    public class ValueListParameter : ExtractReportParameter<string>
    {
        /// <summary>
        /// The collection of available values for this parameter.
        /// </summary>
        List<string> _valueList = new List<string>();

        /// <summary>
        /// Whether values other than the ones in the list can be specified.
        /// </summary>
        bool _allowOtherValues;

        /// <overloads>Initializes a new <see cref="ValueListParameter"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="ValueListParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of strings that
        /// define the possible values for this <see cref="ValueListParameter"/>.</param>
        public ValueListParameter(string name, IEnumerable<string> values)
            : this(name, values, null, false)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ValueListParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of strings that
        /// define the possible values for this <see cref="ValueListParameter"/>.</param>
        /// <param name="allowOtherValues">Indicates whether the <see cref="ValueListParameter"/>
        /// will allow values other than the ones specified in the list.</param>
        public ValueListParameter(string name, IEnumerable<string> values, bool allowOtherValues)
            : this(name, values, null, allowOtherValues)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ValueListParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of strings that
        /// define the possible values for this <see cref="ValueListParameter"/>.</param>
        /// <param name="defaultValue">The default value for this <see cref="ValueListParameter"/>
        /// .  Note: This value muse be included in the <paramref name="values"/> list.</param>
        public ValueListParameter(string name, IEnumerable<string> values, string defaultValue)
            : this(name, values, defaultValue, false)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ValueListParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of strings that
        /// define the possible values for this <see cref="ValueListParameter"/>.</param>
        /// <param name="defaultValue">The default value for this <see cref="ValueListParameter"/>
        /// .  Note: This value must be included in the <paramref name="values"/> list.</param>
        /// <param name="allowOtherValues">Indicates whether the <see cref="ValueListParameter"/>
        /// will allow values other than the ones specified in the list.</param>
        public ValueListParameter(string name, IEnumerable<string> values, string defaultValue,
             bool allowOtherValues)
            : base(name)
        {
            try
            {
                // Ensure that the values collection is not null.
                ExtractException.Assert("ELI23710", "Value collection cannot be null!",
                    values != null);

                // Add the list of values to the value list
                _valueList.AddRange(values);

                // Set the allow other values value
                _allowOtherValues = allowOtherValues;

                // Ensure that there was at least one item added to the list
                ExtractException.Assert("ELI23711",
                    "Value collection must contain at least one item!", _valueList.Count > 0);

                // Check if default value has been specified
                if (!string.IsNullOrEmpty(defaultValue))
                {
                    if (!_valueList.Contains(defaultValue))
                    {
                        if (!allowOtherValues)
                        {
                            ExtractException ee = new ExtractException("ELI23712",
                                "Default value must be contained in the value list!");
                            ee.AddDebugData("Default Value", defaultValue, false);
                            throw ee;
                        }
                        else
                        {
                            _valueList.Add(defaultValue);
                        }
                    }

                    base.ParameterValue = defaultValue;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23713", ex);
            }
        }

        /// <summary>
        /// Gets the collection of possible values for this <see cref="ValueListParameter"/>.
        /// </summary>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> containing the possible
        /// values for this <see cref="ValueListParameter"/>.</returns>
        public ReadOnlyCollection<string> Values
        {
            get
            {
                return _valueList.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="ValueListParameter"/> allows values other
        /// than the values contained in the list.
        /// </summary>
        /// <returns><see langword="true"/> if the <see cref="ValueListParameter"/>
        /// will allow values other than the values in the list; <see langword="false"/>
        /// otherwise.</returns>
        /// <value><see langword="true"/> if the <see cref="ValueListParameter"/>
        /// will allow values other than the values in the list; <see langword="false"/>
        /// otherwise.</value>
        public bool AllowOtherValues
        {
            get
            {
                return _allowOtherValues;
            }
            set
            {
                _allowOtherValues = value;
            }
        }

        /// <summary>
        /// Sets the value of the parameter with the <see langword="string"/>
        /// <see paramref="value"/>.
        /// </summary>
        /// <param name="value">The <see langword="string"/> that specifies the new value of the
        /// parameter.</param>
        public override void SetValueFromString(string value)
        {
            try
            {
                ParameterValue = value;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36065");
            }
        }

        /// <summary>
        /// Gets or sets the current value of this <see cref="ValueListParameter"/>.
        /// </summary>
        /// <returns>The current value of this <see cref="ValueListParameter"/>.</returns>
        /// <value>The current value of this <see cref="ValueListParameter"/>.</value>
        public override string ParameterValue
        {
            get
            {
                return base.ParameterValue;
            }
            set
            {
                try
                {
                    // Check if the specified value is contained in the list
                    if (!_valueList.Contains(value))
                    {
                        // If values are restricted to the list then throw exception
                        if (!_allowOtherValues)
                        {
                            ExtractException ee = new ExtractException("ELI23714",
                                "Specified value is not contained in the list!");
                            ee.AddDebugData("Value Specified", value, false);
                            throw ee;
                        }
                        else
                        {
                            // Values are not restricted, update the list with new value
                            _valueList.Add(value);
                        }
                    }

                    base.ParameterValue = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26506", ex);
                }
            }
        }

        /// <summary>
        /// Writes the <see cref="ValueListParameter"/> to the specified
        /// <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> to write the
        /// parameter to.</param>
        public override void WriteToXml(XmlWriter writer)
        {
            try
            {
                writer.WriteStartElement("ValueListParameter");
                writer.WriteAttributeString("Name", this.ParameterName);

                // Build a comma separated list of values
                StringBuilder sb = new StringBuilder(_valueList[0]);
                for (int i = 1; i < _valueList.Count; i++)
                {
                    sb.Append(",");
                    sb.Append(_valueList[i]);
                }
                // Write the list of values
                writer.WriteAttributeString("Values", sb.ToString());

                // Write the remaining values
                writer.WriteAttributeString("Editable", _allowOtherValues.ToString());
                writer.WriteAttributeString("Default", this.ParameterValue);
                writer.WriteEndElement();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23715", ex);
            }
        }
    }
}
