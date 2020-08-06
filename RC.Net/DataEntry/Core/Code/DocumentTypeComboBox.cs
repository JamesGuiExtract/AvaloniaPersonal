using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    /// <summary>
    /// A <see cref="ComboBox"/> that implements <see cref="IDataEntryAutoCompleteControl"/> to provide suggested
    /// values for selecting DocumentTypes
    /// </summary>
    public partial class DocumentTypeComboBox : LuceneComboBox
    {
        // Reusable object to support using DataEntryValidator
        readonly IAttribute _attribute;

        DataValidity _dataValidity;

        /// <summary>
        /// Initializes a new <see cref="DocumentTypeComboBox"/> instance.
        /// </summary>
        public DocumentTypeComboBox() : base()
        {
            try
            {
                InitializeComponent();

                _attribute = new AttributeClass { Name = "DocumentType" };
                _attribute.Value.CreateNonSpatialString("", "None");
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI50238", ex);
            }
        }

        /// <summary>
        /// Sets the valid values for this control
        /// </summary>
        public void SetAutoCompleteValues(IEnumerable<string> validValues)
        {
            try
            {
                var validValueMatrix = validValues
                    .Select(docType => new[] { docType })
                    .ToArray();
                if (validValueMatrix.Length == 0)
                {
                    ActiveValidator = null;
                }
                else if (ActiveValidator == null)
                {
                    var validator = new DataEntryValidator();
                    validator.SetAutoCompleteValues(validValueMatrix);
                    ActiveValidator = validator;
                    UpdateComboBoxItems();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50142");
            }
        }

        /// <summary>
        /// Corrects case of the text and updates the _dataValidity field
        /// </summary>
        /// <param name="text">The unformatted text to be formatted for display</param>
        /// <returns>The formatted text to be applied to the control</returns>
        protected override string FormatForTextSetter(string text)
        {
            _dataValidity = Validate(text, out var correctedValue);

            if (!string.IsNullOrEmpty(correctedValue))
            {
                return correctedValue;
            }

            return text;
        }

        /// <summary>
        /// Raises the <see cref="Control.TextChanged"/> event
        /// </summary>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            UpdateValidation(_dataValidity);
        }

        // Compute the validity of the input and correct case
        DataValidity Validate(string inputValue, out string correctedValue)
        {
            if (ActiveValidator == null)
            {
                correctedValue = null;
                return DataValidity.Valid;
            }
            _attribute.Value.ReplaceAndDowngradeToNonSpatial(inputValue);

            return ActiveValidator.Validate(_attribute, false, out correctedValue);
        }
    }
}
