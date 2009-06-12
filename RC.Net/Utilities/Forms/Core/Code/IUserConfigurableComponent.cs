using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Defines a method for getting and setting a property page for configuring an object.
    /// </summary>
    public interface IUserConfigurableComponent
    {
        /// <summary>
        /// Gets or sets the <see cref="System.Windows.Forms.UserControl"/> that contains
        /// the configurable objects property page.
        /// </summary>
        /// <return>The <see cref="System.Windows.Forms.UserControl"/> that contains
        /// the configurable objects property page.</return>
        System.Windows.Forms.UserControl PropertyPage
        {
            get;
        }
    }
}
