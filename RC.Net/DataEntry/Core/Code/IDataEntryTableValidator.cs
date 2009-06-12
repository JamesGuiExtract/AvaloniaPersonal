using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Extract.DataEntry
{
    /// <summary>
    /// Provides data validation for an <see cref="IDataEntryTableCell"/>.
    /// </summary>
    public interface IDataEntryTableValidator
    {
        /// <summary>
        /// Specifies whether the cells being validated by this object should be edited with a 
        /// non-editable combo box.
        /// </summary>
        /// <value><see langword="true"/> if a <see cref="DataEntryComboBoxCell"/> should be used to
        /// edit values in the cells being validated by this object, <see langword="false"/> if
        /// values should be edited with a <see cref="DataEntryTextBoxCell"/>.</value>
        /// <returns><see langword="true"/> if a <see cref="DataEntryComboBoxCell"/> are used to
        /// edit values in the cells being validated by this object, <see langword="false"/> if
        /// values are edited with a <see cref="DataEntryTextBoxCell"/>.</returns>
        bool UseComboBoxCells
        {
            get;
            set;
        }

        /// <summary>
        /// Tests to see if the provided <see cref="IDataEntryTableCell"/> meets any specified 
        /// validation requirements the implementing class has and adds an error icon to the cell
        /// if appropriate.
        /// </summary>
        /// <param name="dataEntryCell">The <see cref="IDataEntryTableCell"/> whose data is to be
        /// validated.</param>
        /// <param name="throwException"></param>
        /// <throws><see cref="ExtractException"/> if the <see cref="IDataEntryTableCell"/>'s data 
        /// fails to match any validation requirements it has.</throws>
        void Validate(IDataEntryTableCell dataEntryCell, bool throwException);

        /// <summary>
        /// Retrieves the values from a validation list that are being used for validation.
        /// </summary>
        /// <returns>Retrieves the values from a validation list that are being used for validation or 
        /// <see langword="null"/> if no validation list is being used.</returns>
        string[] GetValidationListValues();
    }
}
