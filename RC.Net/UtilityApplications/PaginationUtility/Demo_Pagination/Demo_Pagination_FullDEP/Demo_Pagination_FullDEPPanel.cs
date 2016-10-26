using Extract.DataEntry.LabDE;
using Extract.Utilities.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility.Demo_Pagination_FullDEP
{
    /// <summary>
    /// Implements <see cref="Extract.UtilityApplications.PaginationUtility.DataEntryDocumentDataPanel"/>
    /// so that this DEP can be used to test full DEP support as part of the paginate files task:
    /// https://extract.atlassian.net/browse/ISSUE-14103
    /// </summary>
    public partial class Demo_Pagination_FullDEPPanel : Extract.UtilityApplications.PaginationUtility.DataEntryDocumentDataPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Demo_Pagination_FullDEPanel"/> class.
        /// </summary>
        public Demo_Pagination_FullDEPPanel()
            : base()
        {
            try
            {
                InitializeComponent();

                base.SummaryQuery =
                    "<Expression>" + 
                    "<Attribute>/PatientInfo/Name/First</Attribute> + ' ' + " +
                    "<Attribute>/PatientInfo/Name/Last</Attribute>";

                LabDEQueryUtilities.Register();
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI41569");
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_refreshButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRefreshButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                ClearCache();
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI41570");
            }
        }
    }
}
