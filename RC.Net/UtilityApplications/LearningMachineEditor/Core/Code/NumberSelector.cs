using System;
using System.Windows.Forms;

namespace Extract.UtilityApplications.LearningMachineEditor
{
    /// <summary>
    /// Form used to select the number of terms to which to limit the selected feature vectorizers
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class NumberSelector : Form
    {
        /// <summary>
        /// Gets the value of the <see cref="NumericUpDown"/> control
        /// </summary>
        public int Value
        {
            get
            {
                // The control is restricted to representing integer values
                return (int)numericUpDown.Value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberSelector"/> class.
        /// </summary>
        public NumberSelector()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41840");
            }
        }
    }
}