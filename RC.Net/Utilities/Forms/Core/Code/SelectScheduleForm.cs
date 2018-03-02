using Extract.Licensing;
using System;
using System.Linq;
using System.Windows.Forms;



namespace Extract.Utilities.Forms
{
    public partial class SelectScheduleForm : Form
    {
        #region Constants

        /// <summary>
        /// Object name used in licensing calls
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(SelectScheduleForm).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes the SelectScheduleForm with the given schedule
        /// </summary>
        /// <param name="scheduled">Schedule to edit</param>
        public SelectScheduleForm(ScheduledEvent scheduled)
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI45625", _OBJECT_NAME);

                InitializeComponent();
                schedulerControl1.Value = scheduled ?? new ScheduledEvent();
            }
            catch (Exception ex)
            {

                throw ex.AsExtract("ELI45626");
            }
        }

        #endregion

        #region Public properties

        public ScheduledEvent Schedule
        {
            get
            {
                try
                {
                    return schedulerControl1.Value;
                }
                catch (Exception ex)
                {

                    throw ex.AsExtract("ELI45627");
                }
            }
        }

        #endregion
    }
}
