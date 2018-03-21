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
    public partial class SelectTypeByExtractCategoryForm<T> : Form 
    {
        #region Fields

        /// <summary>
        /// Set of strings that are used to create the types for the given catalog
        /// </summary>
        HashSet<ExtractCategoryType> _typesAvailable;

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
            _comboBoxOfTypes.DisplayMember = "DescriptionOfType";
            _comboBoxOfTypes.ValueMember = "CreateTypeString";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns an instance of the currently selected type in the combo box
        /// if nothing is selected the return value will be null
        /// </summary>
        public T TypeSelected
        {
            get
            {
                // Check for a selection
                if (_comboBoxOfTypes.SelectedIndex >= 0)
                {
                    var t = Type.GetType(_comboBoxOfTypes.SelectedValue as string);
                    var returnObject = (T)Activator.CreateInstance(t);
                    if (returnObject is T)
                    {
                        return returnObject;
                    }
                    ExtractException ee = new ExtractException("ELI45655", "Selected object is not the correct type.");

                    ee.AddDebugData("Expected Type", typeof(T).FullName, false);
                    ee.AddDebugData("Generated Type", typeof(T).FullName, false);
                    throw ee;
                }
                return default(T);
            }
        }

        #endregion
    }
}
