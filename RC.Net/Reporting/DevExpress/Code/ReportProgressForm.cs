using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting.Native;
using System.IO;
using System.Threading;

namespace Extract.ReportingDevExpress
{
    public partial class ReportProgressForm : DevExpress.XtraEditors.XtraForm
    {
        public bool CanClose = false;

        public ReportProgressForm(string reportFileName)
        {
            InitializeComponent();

            textReportName.Text = Path.GetFileNameWithoutExtension(reportFileName);
        }


        private const int CP_DISABLE_CLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle = cp.ClassStyle | CP_DISABLE_CLOSE_BUTTON;
                return cp;
            }
        }

        private void ReportProgressForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (!CanClose)
                {
                    MessageBox.Show("Report generation is not complete.");
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI50346");
            }
        }
    
    }
}