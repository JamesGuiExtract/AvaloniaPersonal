using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Extract.Dashboard.Utilities
{
    public partial class DisplayUnderlyingDataForm : Form
    {
        public DisplayUnderlyingDataForm()
        {
            InitializeComponent();
        }

        public object DataSource
        {
            set
            {
                dataGridView1.DataSource = value;
            }

        }
    }
}
