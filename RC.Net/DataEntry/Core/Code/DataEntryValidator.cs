using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
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
    /// An <see cref="ExtractException"/> override that is thrown from 
    /// <see cref="DataEntryValidator"/> to indicate data validation errors.
    /// </summary>
    [Serializable]
    // Not overriding GetObjectData because it is not currently necessary to 
    // serialize the associated IDataEntryControl.
    [SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly")]
    // Added so that FXCop will not complain that we have not implemented the standard
    // exception constructors.  We have intentionally not implemented them so that you
    // cannot create an DataEntryValidationException without specifying an ELI code.  
    // If it is discovered in later testing that we need to implement the default constructors
    // due to some issue in the framework we can then remove this suppress message.
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class DataEntryValidationException : ExtractException
    {
        #region Fields

        /// <summary>
        /// The control associated with the invalid data.
        /// </summary>
        private IDataEntryControl _control;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Create a simple DataEntryValidationException object from an ELI code, message string,
        /// and control associated with the exception (no inner exceptions).
        /// </summary>
        /// <param name="eliCode">The ELI code associated with this exception.</param>
        /// <param name="message">The message string associated with this exception.</param>
        /// <param name="control">The <see cref="IDataEntryControl"/> containing the invalid data.
        /// </param>
        public DataEntryValidationException(string eliCode, string message, IDataEntryControl control)
            : base(eliCode, message)
        {
            _control = control;
        }

        /// <summary>
        /// Create an ExtractException object from an ELI code, a message string, an
        /// inner exception, and a control associated with the exception.
        /// </summary>
        /// <param name="eliCode">The ELI code associated with this exception.</param>
        /// <param name="message">The message string associated with this exception.</param>
        /// <param name="innerException">The inner exception associated with this exception.
        /// The inner exception can be of type <see cref="Exception"/> representing a standard 
        /// .NET exception, of type <see cref="Exception"/> representing a C++/COM exception 
        /// that has propagated into the .NET framework, an <see cref="ExtractException"/> object.
        /// </param>
        /// <param name="control">The <see cref="IDataEntryControl"/> containing the invalid data.
        /// </param>
        public DataEntryValidationException(string eliCode, string message, Exception innerException,
            IDataEntryControl control)
            : base(eliCode, message, innerException)
        {
            _control = control;
        }

        /// <summary>
        /// Create an ExtractException object from an ELI code, a message string, an inner exception
        /// and a control associated with the exception.
        /// </summary>
        /// <param name="eliCode">The ELI code associated with this exception.</param>
        /// <param name="message">The message string associated with this exception.</param>
        /// <param name="stringizedInnerException">The inner exception associated with this 
        /// exception. The inner exception should be a string produced by the asStringizedByteStream 
        /// method of <see cref="UCLIDException"/>, if it is a message string an inner exception will 
        /// be created with the message.</param>
        /// <param name="control">The <see cref="IDataEntryControl"/> containing the invalid data.
        /// </param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "stringized")]
        public DataEntryValidationException(string eliCode, string message, string stringizedInnerException,
            IDataEntryControl control)
            : base(eliCode, message, stringizedInnerException)
        {
            _control = control;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The <see cref="IDataEntryControl"/> associated with the invalid data.
        /// </summary>
        public IDataEntryControl Control
        {
            get
            {
                return _control;
            }
        }

        #endregion Properties
    }

    /// <summary>
    /// Provides ability to configure and execute data validation on <see langword="string"/> 
    /// values.
    /// </summary>
    public class DataEntryValidator : ICopyableObject
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(DataEntryValidator).ToString();

        /// <summary>
        /// The value that should be specified in a validation list to consider a blank value as
        /// valid.
        /// </summary>
        private static readonly string _BLANK_VALUE = "[BLANK]";

        #endregion Constants

        #region Fields

        /// <summary>
        /// A regular expression a string value must match prior to being saved.
        /// </summary>
        private Regex _validationRegex;

        /// <summary>
        /// The name of a text file that contains a list of values a string value is required
        /// to match.
        /// </summary>
        private string _validationListFileName;

        /// <summary>
        /// A dictionary of values a qualifying string value is required to match.  The keys are 
        /// the values in upper-case to facilitate case-insensitive lookups.  The value for each 
        /// key represents the original casing specified in the validation list file.
        /// </summary>
        private Dictionary<string, string> _validationListValues;

        /// <summary>
        /// Specifies whether a value that matches a validation list item case-insensitively but
        /// not case-sensitively will be changed to match the validation list value.
        /// </summary>
        private bool _correctCase = true;

        /// <summary>
        /// The error message that should be displayed upon validation failure.
        /// </summary>
        private string _validationErrorMessage = "Invalid value";

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
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI24492",
                    _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24482", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or set a regular expression data match prior to being saved.
        /// <para><b>Requirements</b></para>
        /// Cannot be specified at the same time <see cref="ValidationListFileName"/> is specified.
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
                        ExtractException.Assert("ELI24294",
                            "A validation pattern cannot be specified at the same time as a validation list!",
                            _validationListValues == null);

                        _validationRegex = new Regex(value, RegexOptions.Compiled);
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
        /// Gets or sets the name of a file containing a list of possible values a 
        /// <see langword="string"/> value must match prior to being saved. 
        /// <para><b>Requirements</b></para>
        /// Cannot be specified at the same time <see cref="ValidationPattern"/> is specified.
        /// <para><b>Note</b></para>
        /// If a <see langword="string"/> value matches a value in the supplied list 
        /// case-insensitively but not case-sensitively, the value will be modified to match the 
        /// casing in the list. If a value is specified in the list multiple times, the casing of 
        /// the last entry will be used.
        /// </summary>
        /// <value>The name of a file containing list of values. <see langword="null"/> to remove
        /// any existing validation list requirement.</value>
        /// <returns>The name of a file containing list of values. <see langword="null"/> if there 
        /// is no validation list set.</returns>
        public string ValidationListFileName
        {
            get
            {
                return _validationListFileName;
            }

            set
            {
                try
                { 
                    // Use a ProcessName check for design mode because LicenseUsageMode.UsageMode 
                    // isn't always accruate.
                    if (!Process.GetCurrentProcess().ProcessName.Equals(
                            "devenv", StringComparison.CurrentCultureIgnoreCase) && 
                        !string.IsNullOrEmpty(value))
                    {
                        // Read the file contents into a string value.
                        string fileContents = File.ReadAllText(DataEntryMethods.ResolvePath(value));

                        // Parse the file contents into individual list items.
                        string[] listItems = fileContents.Split(new string[] { Environment.NewLine },
                            StringSplitOptions.RemoveEmptyEntries);

                        SetValidationListValues(listItems);
                    }
                    else
                    {
                        // Either in design mode or the filename was cleared; set the validation
                        // list to null.
                        _validationListValues = null;
                    }

                    // Save the provided file name.
                    _validationListFileName = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24298", ex);
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
        /// Gets or set the error message that should be displayed upon validation failure. 
        /// If unspecified, a default of "Bad value" will be used.
        /// </summary>
        /// <value>The error that should be displayed upon validation failure.</value>
        /// <returns>The error that to be displayed upon validation failure.</returns>
        public string ValidationErrorMessage
        {
            get
            {
                return _validationErrorMessage;
            }

            set
            {
                ExtractException.Assert("ELI24323", "ValidationErrorMessage cannot be null!", 
                    value != null);

                _validationErrorMessage = value;
            }
        }

        #endregion Properties

        #region Methods
        
        /// <summary>
        /// Tests to see if the provided value meets any validation requirements the validator has
        /// </summary>
        /// <param name="value">The <see langword="string"/> value is to be validated.
        /// <para><b>Note</b></para>
        /// If a <see cref="ValidationListFileName"/> has been configured and the provided value
        /// matches a value in the supplied list case-insensitively but not case-sensitively, the 
        /// value will be modified to match the casing in the supplied list.</param>
        /// <param name="attribute">The <see cref="IAttribute"/> mapped to the value to be
        /// validated. If throwException is <see langword="false"/>, the <see cref="IAttribute"/>'s
        /// associated <see cref="AttributeStatusInfo"/> will be updated to reflect the result of 
        /// this call.
        /// <para><b>Requirements:</b></para>
        /// The <see cref="IAttribute"/> must contain a valid <see cref="AttributeStatusInfo"/> 
        /// instance in <see cref="IAttribute.DataObject"/></param>
        /// <param name="throwException">If <see langword="true"/> if the method will throw an
        /// exception if the provided value does not meet validation requirements.</param>
        /// <returns><see langword="true"/> if the value met all validation requirements,
        /// <see langword="false"/> if it did not and throwException is also <see langword="false"/>.
        /// </returns>
        /// <throws><see cref="DataEntryValidationException"/> if the value fails to match any 
        /// validation requirements it has.</throws>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        public bool Validate(ref string value, IAttribute attribute, bool throwException)
        {
            try
            {
                // Allow validation lists and queries to be used only to update auto-complete lists
                // without actually ever validating based on the list items.  Also, since .Net won't
                // display validation error icons without error text, this enforces consistency by
                // preventing cases where the DataEntry framework considers a value invalid, yet
                // no error icon is displayed.
                if (string.IsNullOrEmpty(_validationErrorMessage))
                {
                    return true;
                }

                // Ensure the control's attribute value matches the value of the text it contains.
                bool dataIsValid = (attribute.Value == null && string.IsNullOrEmpty(value)) ||
                                   (attribute.Value != null && attribute.Value.String == value);

                if (!dataIsValid)
                {
                    if (throwException)
                    {
                        DataEntryValidationException ee = new DataEntryValidationException("ELI24182",
                            "Unexpected data!",
                            AttributeStatusInfo.GetStatusInfo(attribute).OwningControl);
                        throw ee;
                    }
                }
                // If there is a specified validation pattern, check it.
                else if (_validationRegex != null && !_validationRegex.IsMatch(value))
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
                        dataIsValid = false;
                    }
                }
                // If there is a specified validation list, check it.
                else if (_validationListValues != null)
                {
                    string valueUpper = value.Trim().ToUpper(CultureInfo.CurrentCulture);
                    string listValue;
                    bool valueIsInList = _validationListValues.TryGetValue(valueUpper, out listValue);

                    if (!valueIsInList)
                    {
                        if (throwException)
                        {
                            DataEntryValidationException ee = new DataEntryValidationException("ELI24282",
                                "Invalid value: " + _validationErrorMessage,
                                AttributeStatusInfo.GetStatusInfo(attribute).OwningControl);
                            ee.AddDebugData("Validation List", this.ValidationListFileName, false);
                            throw ee;
                        }
                        else
                        {
                            dataIsValid = false;
                        }
                    }
                    else if (_correctCase && listValue != value)
                    {
                        // If there is a validation list configured and the text box's data matches an
                        // item in the list, but not case-sensitively, change the casing to match
                        // the list item.
                        value = listValue;
                    }
                }

                // Update the statusInfo for this attribute.
                AttributeStatusInfo.MarkDataAsValid(attribute, dataIsValid);

                return dataIsValid;
            }
            catch (DataEntryValidationException validationException)
            {
                validationException.AddDebugData("Value", value, false);
                throw;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24463", ex);
            }
        }

        /// <summary>
        /// Retrieves the values from the file specified by <see cref="ValidationListFileName"/>.
        /// </summary>
        /// <returns>The values from the file specified by <see cref="ValidationListFileName"/>.
        /// </returns>
        public string[] GetValidationListValues()
        {
            try
            {
                if (_validationListValues == null)
                {
                    return null;
                }
                else
                {
                    // Copy the values from the _validationListValues to an array and return them.
                    string[] values = new string[_validationListValues.Values.Count];
                    _validationListValues.Values.CopyTo(values, 0);
                    return values;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25518", ex);
            }
        }

        /// <summary>
        /// Sets the possible values the validator will accept as valid. This overrides any existing
        /// validation list.
        /// </summary>
        /// <param name="values">The values to be considered valid.</param>
        public void SetValidationListValues(string[] values)
        {
            try
            {
                ExtractException.Assert("ELI24297",
                    "A validation list cannot be specified at the same time as a validation pattern!",
                    this.ValidationPattern == null);

                // Populate the validation list dictionary.
                _validationListValues = new Dictionary<string, string>();
                if (values != null)
                {
                    foreach (string item in values)
                    {
                        string trimmedItem = item.Trim();

                        // [DataEntry:188] Add support of "blank" as a valid value.
                        if (string.Compare(trimmedItem, _BLANK_VALUE,
                                StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            trimmedItem = "";
                        }

                        _validationListValues[trimmedItem.ToUpper(CultureInfo.CurrentCulture)] =
                            trimmedItem;
                    }
                }

                OnValidationListChanged();
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI26153", ex);
            }
        }        

        /// <summary>
        /// Tests to see if the provided <see cref="IAttribute"/> meets any specified 
        /// validation requirements the <see cref="DataEntryValidator"/> has.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose data is to be validated.
        /// </param>
        /// <throws><see cref="DataEntryValidationException"/> if the <see cref="IAttribute"/>'s 
        /// data fails to match any validation requirements it has.</throws>
        public void Validate(IAttribute attribute)
        {
            try
            {
                string value = attribute.Value == null ? "" : attribute.Value.String;

                Validate(ref value, attribute, true);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24631", ex);
            }
        }

        #endregion Methods

        #region ICopyableObject Members

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

        /// <summary>
        /// Copies the value of the provided <see cref="DataEntryValidator"/> instance into the 
        /// current one.
        /// </summary>
        /// <param name="pObject">The object to copy from.</param>
        /// <exception cref="ExtractException">If the supplied object is not of type
        /// <see cref="DataEntryValidator"/>.</exception>
        public void CopyFrom(object pObject)
        {
            try
            {
                ExtractException.Assert("ELI26205", "Cannot copy from an object of a different type!",
                    pObject.GetType() == this.GetType());

                DataEntryValidator source = (DataEntryValidator)pObject;

                _validationRegex = source._validationRegex;
                _validationListFileName = source._validationListFileName;
                _validationErrorMessage = source._validationErrorMessage;
                if (source._validationListValues != null)
                {
                    SetValidationListValues(source.GetValidationListValues());
                }
                _correctCase = source._correctCase;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26204", ex);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised to indicate the validation list has been updated.
        /// </summary>
        public event EventHandler<EventArgs> ValidationListChanged;

        #endregion Events

        #region Private Members

        /// <summary>
        /// Raises the <see cref="ValidationListChanged"/> event.
        /// </summary>
        private void OnValidationListChanged()
        {
            if (this.ValidationListChanged != null)
            {
                ValidationListChanged(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
