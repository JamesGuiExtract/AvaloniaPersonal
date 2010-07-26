using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// COM class for launching the ser processing schedule dialog.
    /// </summary>
    [ComVisible(true)]
    [ProgId("Extract.FileActionManager.Forms.SetProcessingSchedule")]
    [Guid("9A613C2C-54A3-4503-95CC-864152C29A91")]
    public class SetProcessingSchedule : ISetProcessingSchedule
    {
        #region Constants

        /// <summary>
        /// The number of hours in a week, used to initialize the schedule
        /// collection as well as ensure the passed in schedule is of the 
        /// appropriate size.
        /// </summary>
        internal const int _NUMBER_OF_HOURS_IN_WEEK = 168;

        /// <summary>
        /// Object name used in license validation calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(SetProcessingSchedule).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="SetProcessingSchedule"/> class.
        /// </summary>
        public SetProcessingSchedule()
        {
        }

        #endregion Constructors

        #region ISetProcessingSchedule

        /// <summary>
        /// Prompts the user to set the processing schedule.
        /// </summary>
        /// <param name="pSchedule">The current processing schedule.</param>
        /// <returns>The updated processing schedule.</returns>
        [CLSCompliant(false)]
        public VariantVector PromptForSchedule(VariantVector pSchedule)
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI30418", _OBJECT_NAME);

                int size = pSchedule.Size;
                ExtractException.Assert("ELI30419", "Schedule has invalid size.",
                    size == _NUMBER_OF_HOURS_IN_WEEK, "Size", size);

                // Initialize the schedule list from the variant vector
                List<bool> schedule = new List<bool>(size);
                for (int i = 0; i < size; i++)
                {
                    schedule.Add((bool)pSchedule[i]);
                }

                VariantVector newSchedule = null;
                using (var scheduler = new SetProcessingScheduleForm(schedule))
                {
                    // If the dialog was closed with OK then insert the new schedule
                    // into the variant vector
                    if (scheduler.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var updatedSchedule = scheduler.Schedule;
                        newSchedule = new VariantVector();
                        for (int i = 0; i < updatedSchedule.Count; i++)
                        {
                            newSchedule.Insert(i, updatedSchedule[i]);
                        }
                    }
                }

                return newSchedule;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI30420",
                    "Unable to set schedule.", ex);
            }
        }

        #endregion ISetProcessingSchedule
    }
}
