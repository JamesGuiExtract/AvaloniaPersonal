using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Form used to display all the class types that have the specified category.
    /// 
    /// A category is added to a class by using the ExtractCategoryAttribute defined in Extract.Code.Attributes
    /// </summary>
    public partial class SelectTypeByExtractCategoryForm : Form
    {
        #region Fields

        /// <summary>
        /// Set of strings that are used to create the types for the given catalog
        /// </summary>
        HashSet<string> _typesAvailable;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the selection dialog with the types for extractCategory
        /// </summary>
        /// <param name="extractCategory">The category of types that will be in the combo box for selection</param>
        public SelectTypeByExtractCategoryForm(string extractCategory)
        {
            InitializeComponent();

            // Get the types
            _typesAvailable = UtilityMethods.GetExtractCategoriesJson()?[extractCategory];

            // Update the window title
            Text = string.Format(CultureInfo.InvariantCulture,
                "Select type of {0}", extractCategory);

            _comboBoxOfTypes.DataSource = _typesAvailable?.ToList();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns an instance of the currently selected type in the combo box
        /// if nothing is selected the return value will be null
        /// </summary>
        public object TypeSelected
        {
            get
            {
                // Check for a selection
                if (_comboBoxOfTypes.SelectedIndex >= 0)
                {
                    var t = Type.GetType(_comboBoxOfTypes.SelectedValue as string);
                    return Activator.CreateInstance(t);
                }
                return null;
            }
        }

        #endregion
    }
}
