using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Provides data validation for a specified <see cref="IAttribute"/>.
    /// </summary>
    public interface IDataEntryValidator
    {
        #region Events

        /// <summary>
        /// Raised to indicate the validation list has been updated.
        /// </summary>
        event EventHandler<EventArgs> ValidationListChanged;

        /// <summary>
        /// Raised to indicate the auto-complete values have changed.
        /// </summary>
        event EventHandler<EventArgs> AutoCompleteValuesChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets the error message that should be displayed upon validation failure. 
        /// </summary>
        /// <returns>The error that is to be displayed upon validation failure.</returns>
        string ValidationErrorMessage
        {
            get;
        }

        /// <summary>
        /// Get the map of autocomplete values to akas
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IEnumerable<KeyValuePair<string, List<string>>> AutoCompleteValuesWithSynonyms { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Tests to see if the provided <see cref="IAttribute"/> meets all specified 
        /// validation requirements the implementing class has.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose data is to be validated.
        /// </param>
        /// <param name="throwException">If <see langword="true"/> if the method will throw an
        /// exception if the provided value does not meet validation requirements.</param>
        /// <returns>A <see cref="DataValidity"/>value indicating whether 
        /// <see paramref="attribute"/>'s value is currently valid.
        /// </returns>
        /// <throws><see cref="DataEntryValidationException"/> if the <see cref="IAttribute"/>'s 
        /// data fails to match any validation requirements it has and
        /// <see paramref="throwException"/> is <see langword="true"/></throws>
        DataValidity Validate(IAttribute attribute, bool throwException);

        /// <summary>
        /// Tests to see if the provided <see cref="IAttribute"/> meets all specified 
        /// validation requirements the implementing class has.
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
        DataValidity Validate(IAttribute attribute, bool throwException, out string correctedValue);

        /// <summary>
        /// Gets an array of <see langword="string"/> values that should be provided to the user in
        /// an auto-complete list.
        /// </summary>
        /// <returns>
        /// An array of <see langword="string"/> values that should be provided to the user in an
        /// auto-complete list.
        /// </returns>
        string[] GetAutoCompleteValues();

        /// <summary>
        /// Retrieves a validator instance that is either not specific to any given attribute or
        /// is a new copy of the validator to use for a specific attribute.
        /// </summary>
        /// <returns>A <see cref="IDataEntryValidator"/> instance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        IDataEntryValidator GetPerAttributeInstance();

        #endregion Methods
    }

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
}
