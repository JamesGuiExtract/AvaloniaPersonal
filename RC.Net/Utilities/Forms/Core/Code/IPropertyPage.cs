using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Defines a method for applying changes to a configurable object's property page and
    /// a method to check if the settings are dirty.
    /// </summary>
    public interface IPropertyPage
    {
        /// <summary>
        /// Event that should be raised any time the dirty flag is set.
        /// </summary>
        event EventHandler PropertyPageModified;

        /// <summary>
        /// Applies the changes from the property page to the configurable object.
        /// </summary>
        void Apply();

        /// <summary>
        /// Gets whether the current property page is dirty or not.
        /// </summary>
        /// <return><see langword="true"/> if the property page is dirty and return
        /// <see langword="false"/> if not.</return>
        bool IsDirty
        {
            get;
        }

        /// <summary>
        /// Gets whether the user-specified settings on the property page are valid.
        /// </summary>
        /// <value><see langword="true"/> if the user-specified settings are valid; 
        /// <see langword="false"/> if the settings are not valid.</value>
        bool IsValid
        {
            get;
        }

        /// <summary>
        /// Sets the focus to the first control in the property page.
        /// </summary>
        void SetFocusToFirstControl();
    }
}
