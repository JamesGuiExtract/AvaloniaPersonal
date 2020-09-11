using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Provides ability to configure and execute data validation on <see langword="string"/> 
    /// values.
    /// </summary>
    public class DataEntryValidator : ICloneable, IDataEntryValidator
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataEntryValidator).ToString();

        /// <summary>
        /// The value that should be specified in a validation list to consider a blank value as
        /// valid.
        /// </summary>
        static readonly string _BLANK_VALUE = "[BLANK]";

        /// <summary>
        /// The default error message
        /// </summary>
        static readonly string _DEFAULT_ERROR_MESSAGE = "Invalid value";

        #endregion Constants

        #region Struct

        /// <summary>
        /// Represents info about a <see cref="DataEntryQuery"/> to be used for validation.
        /// </summary>
        struct ValidationQuery
        {
            /// <summary>
            /// The <see cref="DataEntryQuery"/> to be used to validate <see cref="IAttribute"/>
            /// values.
            /// </summary>
            public DataEntryQuery DataEntryQuery;

            /// <summary>
            /// The set of values to be considered values (the key being an uppercase version of
            /// the correctly cased value).
            /// </summary>
            public Dictionary<string, string> ValidationValues;

            /// <summary>
            /// <see langword="true"/> if the query represents a validation warning,
            /// <see langword="false"/> if the query identifies data that is truly invalid.
            /// </summary>
            public bool IsWarning;

            /// <summary>
            /// The <see cref="QueryResult"/> that was produced the last time
            /// <see cref="DataEntryQuery"/> was evaluated.
            /// </summary>
            public QueryResult QueryResult;
        }

        #endregion Struct

        #region Fields

        /// <summary>
        /// A regular expression a string value must match prior to being saved.
        /// </summary>
        Regex _validationRegex;

        /// <summary>
        /// Keeps track of the <see cref="DataEntryQuery"/>s that are used to validate data.
        /// </summary>
        Dictionary<DataEntryQuery, ValidationQuery> _validationQueries =
            new Dictionary<DataEntryQuery, ValidationQuery>();

        /// <summary>
        /// The list of values that comprise the auto-complete options that should be available.
        /// </summary>
        List<KeyValuePair<string, List<string>>> _autoCompleteValues;

        /// <summary>
        /// Specifies whether validation lists will be checked for matches case-sensitively.
        /// </summary>
        bool _caseSensitive;

        /// <summary>
        /// Specifies whether a value that matches a validation list item case-insensitively but
        /// not case-sensitively will be changed to match the validation list value.
        /// </summary>
        bool _correctCase = true;

        /// <summary>
        /// The error message that should be displayed upon validation failure.
        /// </summary>
        string _validationErrorMessage = _DEFAULT_ERROR_MESSAGE;

        /// <summary>
        /// The error message that should be displayed instead of _validationErrorMessage based on
        /// the <see cref="DataEntryQuery"/> used to mark the data as invalid.
        /// </summary>
        string _overrideErrorMessage;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataEntryValidator"/> instance.
        /// </summary>
        public DataEntryValidator()
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI24492", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24482", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a regular expression data match prior to being saved.
        /// </summary>
        /// <value>A regular expression a <see langword="string"/> value must match prior to being 
        /// saved. <see langword="null"/> to remove any existing validation pattern requirement.
        /// </value>
        /// <returns>A regular expression a <see langword="string"/> value prior to being saved.
        /// <see langword="null"/> if there is no validation pattern set.</returns>
        public string ValidationPattern
        {
            get
            {
                try
                {
                    return (_validationRegex != null ? _validationRegex.ToString() : null);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24293", ex);
                }
            }

            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _validationRegex = new Regex(value);
                    }
                    else
                    {
                        _validationRegex = null;
                    }
                }
                catch (ExtractException ex)
                {
                    throw ExtractException.AsExtractException("ELI24295", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether a value that matches a validation list item case-insensitively but
        /// not case-sensitively will be changed to match the validation list value.
        /// </summary>
        /// <value><see langword="true"/> if values should be modified to match the case of list items,
        /// <see langword="false"/> if case-insensitive matches should be left as-is.</value>
        /// <returns><see langword="true"/> if values will be modified to match the case of list items,
        /// <see langword="false"/> if case-insensitive matches will be left as-is.</returns>
        public bool CorrectCase
        {
            get
            {
                return _correctCase;
            }

            set
            {
                _correctCase = value;
            }
        }

        /// <summary>
        /// Gets or sets whether validation lists will be checked for matches case-sensitively.
        /// </summary>
        /// <value><see langword="true"/> to validate against a validation list case-sensitively;
        /// <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if validating against a validation list
        /// case-sensitively; <see langword="false"/> otherwise.</returns>
        public bool CaseSensitive
        {
            get
            {
                return _caseSensitive;
            }

            set
            {
                _caseSensitive = value;
            }
        }

        /// <summary>
        /// Gets or set the error message that should be displayed upon validation failure. 
        /// If unspecified, a default of "Invalid value" will be used.
        /// </summary>
        /// <value>The error that should be displayed upon validation failure.</value>
        /// <returns>The error that is to be displayed upon validation failure.</returns>
        public string ValidationErrorMessage
        {
            get
            {
                return _overrideErrorMessage ?? _validationErrorMessage;
            }

            set
            {
                _validationErrorMessage = value ?? _DEFAULT_ERROR_MESSAGE;
            }
        }

        /// <summary>
        /// Indicates whether validation is enabled for the control. If <c>false</c>, validation
        /// queries will continue to provide auto-complete lists and alter case if
        /// ValidationCorrectsCase is set for any field, but it will not show any data errors or
        /// warnings or prevent saving of the document.
        /// </summary>
        public bool ValidationEnabled { get; set; } = true;

        /// <summary>
        /// Get the dictionary of autocomplete values to akas
        /// </summary>
        public IEnumerable<KeyValuePair<string, List<string>>> AutoCompleteValuesWithSynonyms => _autoCompleteValues;

        #endregion Properties

        #region Methods

        /// <overrides>
        /// Tests to see if the provided value meets any validation requirements the validator has
        /// </overrides>
        /// <summary>
        /// Tests to see if the provided value meets any validation requirements the validator has
        /// <para><b>Note</b></para>
        /// If a <see cref="DataEntryQuery"/> provides a list of valid values and the provided value
        /// matches a value in the supplied list case-insensitively but not case-sensitively, the 
        /// value will be modified to match the casing in the supplied list.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> mapped to the value to be
        /// validated. If throwException is <see langword="false"/>, the <see cref="IAttribute"/>'s
        /// associated <see cref="AttributeStatusInfo"/> will be updated to reflect the result of 
        /// this call.
        /// <para><b>Requirements:</b></para>
        /// The <see cref="IAttribute"/> must contain a valid <see cref="AttributeStatusInfo"/> 
        /// instance in IAttribute.DataObject</param>
        /// <param name="throwException">If <see langword="true"/> if the method will throw an
        /// exception if the provided value does not meet validation requirements.</param>
        /// <param name="correctValue"><see langword="true"/> if the value is valid, to remove any
        /// extra whitespace and to match the value's casing used by the validator.</param>
        /// <param name="correctedValue">If the value is valid but has extra whitespace or has
        /// different casing, this parameter will be populated with a corrected version of the
        /// value that has already been applied to the attribute. Unused if
        /// <see paramref="correctValue"/> is <see langword="false"/>.</param>
        /// <returns>A <see cref="DataValidity"/>value indicating whether 
        /// <see paramref="attribute"/>'s value is now valid.
        /// </returns>
        /// <throws><see cref="DataEntryValidationException"/> if the <see cref="IAttribute"/>'s 
        /// data fails to match any validation requirements it has and
        /// <see paramref="throwException"/> is <see langword="true"/></throws>
        DataValidity Validate(IAttribute attribute, bool throwException, bool correctValue,
            out string correctedValue)
        {
            string originalValue = null;

            try
            {
                ExtractException.Assert("ELI29217", "Null argument exception!", attribute != null);

                // Reset the override error message prior to evaluating the value again.
                _overrideErrorMessage = null;
                correctedValue = null;

                DataValidity dataValidity = DataValidity.Valid;
                originalValue = attribute.Value == null ? "" : attribute.Value.String;
                string value = originalValue;

                // NOTE: Validate may be used only to update auto-complete lists without actually
                // ever validating based on the list items; in the case that validation is not
                // enabled, DataValidity.Valid will be returned. 
                bool validationEnabled = AttributeStatusInfo.IsValidationEnabled(attribute);

                // Preventing an item from being marked invalid without _validationErrorMessage
                // enforces consistency; .Net won't display validation error icons without error
                // text, so this prevents the situation where the DataEntry framework considers
                // a value invalid, yet no error icon is displayed.
                if (validationEnabled && string.IsNullOrEmpty(_validationErrorMessage))
                {
                    ValidationEnabled = false;  // Validation status for this validator
                    validationEnabled = false;  // Overall calculated validation status accounting
                                                // for control visibility, etc.
                }

                // If there is a specified validation pattern, check it.
                if (validationEnabled &&  _validationRegex != null &&
                         !_validationRegex.IsMatch(value))
                {
                    if (throwException)
                    {
                        DataEntryValidationException ee = new DataEntryValidationException("ELI24281",
                            "Invalid value: " + _validationErrorMessage,
                            AttributeStatusInfo.GetStatusInfo(attribute).OwningControl);
                        ee.AddDebugData("Validation Pattern", this.ValidationPattern, false);
                        throw ee;
                    }
                    else
                    {
                        dataValidity = DataValidity.Invalid;
                    }
                }
                // If there are any validation queries, evaluate them. Do this even if validation is
                // disabled to take advantage of trimming and case-correction.
                else if (_validationQueries != null)
                {
                    ValidationQuery? validationFailure = null;

                    // [DataEntry:1131]
                    // Make a copy of this collection before iterating since it is possible for
                    // _validationQueries to be updated with new results while iterating.
                    var validationQueries = new List<ValidationQuery>(_validationQueries.Values);

                    // Evaluate each validation query looking for one that does not return valid.
                    foreach (ValidationQuery validationQuery in validationQueries)
                    {
                        if (!Validate(ref value, validationQuery))
                        {
                            // validationFailure should be set to the first non-valid query or
                            // to any query that returns invalid.
                            if (validationFailure == null || !validationQuery.IsWarning)
                            {
                                validationFailure = validationQuery;

                                // Keep searching until a query is found that returns invalid.
                                if (!validationQuery.IsWarning)
                                {
                                    break;
                                }
                            }
                        }
                        else if (correctValue && value != originalValue)
                        {
                            // If the data is valid, correctValue is true, and the validator updated
                            // the value with new casing, apply the updated value to both the
                            // control and underlying attribute.
                            AttributeStatusInfo.SetValue(attribute, value, false, false);
                            correctedValue = value;
                            originalValue = value;
                        }
                    }

                    // If validation is enabled and one of the validation queries failed, set the
                    // dataValidity and associated validation message from the query.
                    if (validationEnabled && validationFailure != null)
                    {
                        // Override the default error message if the query provides one.
                        _overrideErrorMessage =
                            validationFailure.Value.DataEntryQuery.GetValidationMessage();

                        if (throwException)
                        {
                            DataEntryValidationException ee = new DataEntryValidationException(
                                "ELI24282", "Invalid value: " + ValidationErrorMessage,
                                AttributeStatusInfo.GetStatusInfo(attribute).OwningControl);
                            throw ee;
                        }
                        else
                        {
                            dataValidity = validationFailure.Value.IsWarning
                                ? DataValidity.ValidationWarning : DataValidity.Invalid;
                        }
                    }
                }

                AttributeStatusInfo.SetDataValidity(attribute, dataValidity, validationEnabled);

                return dataValidity;
            }
            catch (DataEntryValidationException validationException)
            {
                validationException.AddDebugData("Value", originalValue, false);
                throw;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24463", ex);
            }
        }

        /// <summary>
        /// Validates the specified value using the specified <see cref="ValidationQuery"/>.
        /// </summary>
        /// <param name="value">The <see langword="string"/> value to be validated (and corrected
        /// for casing differences if so configured).</param>
        /// <param name="validationQuery">The <see cref="ValidationQuery"/> that is to be used to
        /// validate the value.</param>
        /// <returns><see langword="true"/> if the specified <see paramref="value"/> passes
        /// validation using <see paramref="validationQuery"/>, <see langword="false"/> otherwise.
        /// </returns>
        bool Validate(ref string value, ValidationQuery validationQuery)
        {
            try
            {
                bool dataIsValid = true;

                DataEntryQuery dataEntryQuery = validationQuery.DataEntryQuery;
                ExtractException.Assert("ELI28976", "Missing validation query!",
                    dataEntryQuery != null);

                // The validation query needs to be evaluated every time if a ValidValue is being
                // used or if the validation values have not yet been calculated.
                if (dataEntryQuery.ValidValue != null || validationQuery.ValidationValues == null)
                {
                    QueryResult queryResult = dataEntryQuery.Evaluate();

                    if (dataEntryQuery.ValidValue != null)
                    {
                        dataIsValid =
                            dataEntryQuery.ValidValue.Equals(queryResult.ToString(),
                                dataEntryQuery.CaseSensitive ?
                                    StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                    }
                    // If using a validation value list, update the values associated with this query.
                    else
                    {
                        SetValidationQuery(dataEntryQuery, queryResult);
                    }
                }

                if (dataEntryQuery.ValidValue == null)
                {
                    string valueTrimmed = value.Trim();
                    string listValue;
                    if (_caseSensitive)
                    {
                        // If case-sensitive, keys are assigned with the original casing.
                        dataIsValid =
                            validationQuery.ValidationValues.TryGetValue(valueTrimmed, out listValue);
                    }
                    else
                    {
                        // If checking case-insensitively, keys have been assigned using all caps.
                        string valueUpper = valueTrimmed.ToUpper(CultureInfo.CurrentCulture);
                        dataIsValid =
                            validationQuery.ValidationValues.TryGetValue(valueUpper, out listValue);
                    }

                    if (dataIsValid)
                    {
                        if (_correctCase && listValue != value)
                        {
                            // If there is a validation list configured and the text box's data
                            // matches an item in the list, but not case-sensitively, change the
                            // casing to match the list item.
                            value = listValue;
                        }
                        else if (value != valueTrimmed)
                        {
                            // Always trim the entries whether or not CorrectCase is enabled.  This
                            // ensures that if controls add copies of the list entries with leading
                            // spaces to enable all auto-complete entries to be displayed that the
                            // spaces are removed in the final value.
                            value = valueTrimmed;
                        }
                    }
                }

                return dataIsValid;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28930", ex);
            }
        }

        /// <summary>
        /// Specifies that the provided <see cref="DataEntryQuery"/> to be used to validate data or
        /// generate auto-complete values in association with this <see cref="DataEntryValidator"/>.
        /// </summary>
        /// <param name="dataEntryQuery">The <see cref="DataEntryQuery"/> to be used to validate
        /// data or generate auto-complete values.</param>
        /// <param name="queryResults">The <see cref="QueryResult"/>s that were generated the last
        /// time <see paramref="dataEntryQuery"/> was evaluated.</param>
        internal void SetValidationQuery(DataEntryQuery dataEntryQuery, QueryResult queryResults)
        {
            try
            {
                // Initialize a ValidationQuery instance.
                ValidationQuery validationQuery = new ValidationQuery();
                validationQuery.DataEntryQuery = dataEntryQuery;
                validationQuery.QueryResult = queryResults;
                validationQuery.ValidationValues = new Dictionary<string, string>();
                validationQuery.IsWarning = dataEntryQuery.IsValidationWarning;

                // Initialize the value(s) of ValidValue or ValidationValues as appropriate.
                if (!queryResults.IsEmpty || dataEntryQuery.ValidValue != null)
                {
                    // If ValidValue is not null, rather than provide a list, the query result
                    // will be compared against the specified ValidValue.
                    // If ValidValue is null, generate a list of values to be considered valid.
                    if (dataEntryQuery.ValidValue == null)
                    {
                        string[] values = queryResults.ToStringArray();

                        foreach (string value in values)
                        {
                            string trimmedValue = value.Trim();

                            // [DataEntry:188] Add support of "blank" as a valid value.
                            if (string.Compare(trimmedValue, _BLANK_VALUE,
                                    StringComparison.CurrentCultureIgnoreCase) == 0)
                            {
                                trimmedValue = "";
                            }

                            if (_caseSensitive)
                            {
                                // If using case-sensitive validation, the key should use the
                                // original casing.
                                validationQuery.ValidationValues[trimmedValue] = trimmedValue;
                            }
                            else
                            {
                                // If using case-insensitive validation, the key should use all
                                // caps.
                                string key = trimmedValue.ToUpper(CultureInfo.CurrentCulture);
                                validationQuery.ValidationValues[key] = trimmedValue;
                            }
                        }
                    }
                }

                _validationQueries[dataEntryQuery] = validationQuery;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28975", ex);
            }
        }

        /// <summary>
        /// Gets an array of <see langword="string"/> values that should be provided to the user in
        /// an auto-complete list.
        /// </summary>
        /// <returns>
        /// An array of <see langword="string"/> values that should be provided to the user in an
        /// auto-complete list.
        /// </returns>
        public string[] GetAutoCompleteValues()
        {
            try
            {
                if (_autoCompleteValues == null || _autoCompleteValues.Count == 0)
                {
                    return null;
                }
                else
                {
                    return _autoCompleteValues.Select(kv => kv.Key).ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28929", ex);
            }
        }

        /// <summary>
        /// Sets the values that should be provided to the user in an auto-complete list.
        /// </summary>
        /// <param name="values">An array of values that should be provided to the user in an
        /// auto-complete list associated with the data's control. The first item in each row should
        /// be the valid value. Subsequent items are other fields to be queried, e.g., AKAs</param>
        public void SetAutoCompleteValues(string[][] values)
        {
            try
            {
                _autoCompleteValues = new List<KeyValuePair<string, List<string>>>();
                if (values != null)
                {
                    // Build the validation list to AKA map
                    var map = new Dictionary<string, List<string>>();
                    if (values != null)
                    {
                        foreach (string[] items in values)
                        {
                            string trimmedItem = items[0].Trim();

                            // [DataEntry:188] Add support of "blank" as a valid value.
                            if (string.Compare(trimmedItem, _BLANK_VALUE,
                                    StringComparison.CurrentCultureIgnoreCase) == 0)
                            {
                                trimmedItem = "";
                            }
                            if (map.TryGetValue(trimmedItem, out var akas))
                            {
                                akas.AddRange(items.Skip(1));
                            }
                            else
                            {
                                akas = items.Skip(1).ToList();
                                map.Add(trimmedItem, akas);
                                _autoCompleteValues.Add(new KeyValuePair<string, List<string>>(trimmedItem, akas));
                            }
                        }
                    }
                }

                OnAutoCompleteValuesChanged();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26153", ex);
            }
        }

        #region IDataEntryValidator Members

        /// <summary>
        /// Tests to see if the provided <see cref="IAttribute"/> meets any specified 
        /// validation requirements the <see cref="DataEntryValidator"/> has.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose data is to be validated.
        /// </param>
        /// <param name="throwException">If <see langword="true"/> if the method will throw an
        /// exception if the provided value does not meet validation requirements.</param>
        /// <returns></returns>
        /// <throws><see cref="DataEntryValidationException"/> if the <see cref="IAttribute"/>'s 
        /// data fails to match any validation requirements it has and
        /// <see paramref="throwException"/> is <see langword="true"/></throws>
        public DataValidity Validate(IAttribute attribute, bool throwException)
        {
            try
            {
                string correctedValue;
                
                return Validate(attribute, throwException, false, out correctedValue);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24631", ex);
            }
        }

        /// <summary>
        /// Tests to see if the provided <see cref="IAttribute"/> meets any specified 
        /// validation requirements the <see cref="DataEntryValidator"/> has.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose data is to be validated.
        /// </param>
        /// <param name="throwException">If <see langword="true"/> if the method will throw an
        /// exception if the provided value does not meet validation requirements.</param>
        /// <param name="correctedValue">If the value is valid but has extra whitespace or has
        /// different casing, this parameter will be populated with a corrected version of the
        /// value that has already been applied to the attribute.</param>
        /// <returns>A <see cref="DataValidity"/>value indicating whether 
        /// <see paramref="attribute"/>'s value is currently valid.
        /// </returns>
        /// <throws><see cref="DataEntryValidationException"/> if the <see cref="IAttribute"/>'s 
        /// data fails to match any validation requirements it has and
        /// <see paramref="throwException"/> is <see langword="true"/></throws>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        public DataValidity Validate(IAttribute attribute, bool throwException, out string correctedValue)
        {
            try
            {
                return Validate(attribute, throwException, true, out correctedValue);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29218", ex);
            }
        }

        /// <summary>
        /// Retrieves a validator instance that is either not specific to any given attribute or
        /// is a new copy of the validator to use for a specific attribute.
        /// </summary>
        /// <returns>A <see cref="IDataEntryValidator"/> instance.</returns>
        public IDataEntryValidator GetPerAttributeInstance()
        {
            try
            {
                return (IDataEntryValidator)Clone();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29175", ex);
            }
        }

        #endregion IDataEntryValidator Members

        #endregion Methods

        #region ICloneable Members

        /// <summary>
        /// Creates a copy of the current <see cref="DataEntryValidator"/> instance.
        /// </summary>
        /// <returns>A copy of the current <see cref="DataEntryValidator"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                DataEntryValidator clone = new DataEntryValidator();

                clone.CopyFrom(this);

                return clone;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26203", ex);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised to indicate the validation list has been updated.
        /// </summary>
        public event EventHandler<EventArgs> ValidationListChanged;

        /// <summary>
        /// Raised to indicate the auto-complete values have changed.
        /// </summary>
        public event EventHandler<EventArgs> AutoCompleteValuesChanged;

        #endregion Events

        #region Private Members

        /// <summary>
        /// Raises the <see cref="ValidationListChanged"/> event.
        /// </summary>
        void OnValidationListChanged()
        {
            if (this.ValidationListChanged != null)
            {
                ValidationListChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="AutoCompleteValuesChanged"/> event.
        /// </summary>
        void OnAutoCompleteValuesChanged()
        {
            if (this.AutoCompleteValuesChanged != null)
            {
                AutoCompleteValuesChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Copies the value of the provided <see cref="DataEntryValidator"/> instance into the 
        /// current one.
        /// </summary>
        /// <param name="sourceInstance">The object to copy from.</param>
        /// <exception cref="ExtractException">If the supplied object is not of type
        /// <see cref="DataEntryValidator"/>.</exception>
        void CopyFrom(object sourceInstance)
        {
            try
            {
                ExtractException.Assert("ELI26205", "Cannot copy from an object of a different type!",
                    sourceInstance.GetType() == GetType());

                DataEntryValidator sourceValidator = (DataEntryValidator)sourceInstance;

                _validationRegex = sourceValidator._validationRegex;
                _validationErrorMessage = sourceValidator._validationErrorMessage;
                // The validation queries will be re-populated when attributes are mapped.
                _validationQueries.Clear();
                _correctCase = sourceValidator._correctCase;
                _caseSensitive = sourceValidator._caseSensitive;
                ValidationEnabled = sourceValidator.ValidationEnabled;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26204", ex);
            }
        }

        #endregion Private Members
    }
}
