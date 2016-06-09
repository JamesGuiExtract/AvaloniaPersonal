using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Extract.AttributeFinder;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Extract.UtilityApplications.LearningMachineEditor
{
    /// <summary>
    /// Allows viewing/editing of computed feature vectorizers
    /// </summary>
    public partial class EditFeatures : Form
    {
        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="encoder">The <see cref="LearningMachineDataEncoder"/> to be viewed/edited</param>
        [SuppressMessage("Microsoft.Performance", "CA1801", Target="encoder")]
        public EditFeatures(LearningMachineDataEncoder encoder)
        {
            InitializeComponent();
            dataGridView1.Rows.Add(true, "Auto Bag of Words", "DiscreteTerms", 2000);
            dataGridView1.Rows.Add(true, "FileNameSuffix", "Exists", 1);

            var totalFeatures = dataGridView1.Rows.Cast<DataGridViewRow>().Sum(r => r.IsNewRow
                                                                                    ? 0
                                                                                    : (Int32)(r.Cells[3].Value));
            statusStrip1.Items[0].Text = statusStrip1.Items[0].Text + totalFeatures.ToString(CultureInfo.CurrentCulture);
        }
    }
}
