using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
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

        readonly string _extractCategory;

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
        public SelectTypeByExtractCategoryForm(string extractCategory, bool refreshCache = false)
        {
            InitializeComponent();

            _extractCategory = extractCategory;

            // Get the types
            _typesAvailable = UtilityMethods.GetExtractCategoriesJson(refreshCache)?[extractCategory];

            // Update the window title
            Text = string.Format(CultureInfo.InvariantCulture,
                "Select type of {0}", extractCategory);

            _comboBoxOfTypes.DataSource = _typesAvailable?.ToList();
            _comboBoxOfTypes.DisplayMember = "DescriptionOfType";
            _comboBoxOfTypes.ValueMember = "CreateTypeString";
        }

        #endregion

        /// <summary>
        /// Returns an instance of the currently selected type in the combo box if it matches the expected type.
        /// </summary>
        /// <returns>
        /// An instance of the currently selected type, cast to <typeparamref name="T"/> or the default value if
        /// no item is selected. Throws an exception if the type cannot be created or cannot be cast to the expected type.
        /// </returns>
        public T GetTypeFromSelection()
        {
            string typeName = null;
            try
            {
                if (_comboBoxOfTypes.SelectedIndex < 0)
                {
                    return default;
                }

                typeName = _comboBoxOfTypes.SelectedValue as string;
                var t = Type.GetType(typeName);
                ExtractException.Assert("ELI53950", "Could resolve type", t is not null);

                var returnObject = Activator.CreateInstance(t);
                if (returnObject is T returnType)
                {
                    return returnType;
                }

                ExtractException ee = new("ELI45655", "Selected object is not the correct type.");
                ee.AddDebugData("Expected Type", typeof(T).FullName, false);
                ee.AddDebugData("Generated Type", returnObject?.GetType().FullName, false);
                throw ee;
            }
            catch (Exception ex)
            {
                var uex = ex.AsExtract("ELI53951");
                uex.AddDebugData("Specified Type", typeName);
                throw uex;
            }
        }
    }
}
