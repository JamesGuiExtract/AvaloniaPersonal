using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Extract.DataEntry
{
    /// <summary>
    /// Implemented by a control that also implements <see cref="IDataEntryControl"/> if the
    /// control intends to use the standard error provider supplied by its   
    /// <see cref="DataEntryControlHost"/>. <see cref="IDataEntryControl"/>s do not need to 
    /// implement this interface, but to allow the <see cref="DataEntryControlHost"/> to control 
    /// and standardize behavior as much as possible it is recommended that they do unless they 
    /// have a specific need to display data validation problems in a non-standard way. 
    /// </summary>
    public interface IRequiresErrorProvider
    {
        /// <summary>
        /// Specifies the standard <see cref="ErrorProvider"/>s that should be used to 
        /// display data validation errors.
        /// </summary>
        /// <param name="validationErrorProvider">The standard <see cref="ErrorProvider"/> that
        /// should be used to display data validation errors.</param>
        /// <param name="validationWarningErrorProvider">The <see cref="ErrorProvider"/> that should
        /// be used to display data validation warnings.</param>
        void SetErrorProviders(ErrorProvider validationErrorProvider,
            ErrorProvider validationWarningErrorProvider);
    }
}
