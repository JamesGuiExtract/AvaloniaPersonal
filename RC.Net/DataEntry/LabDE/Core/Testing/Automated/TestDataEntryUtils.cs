using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Globalization;
using static System.FormattableString;

using Util = Extract.DataEntry.LabDE.LabDEQueryUtilities;

namespace Extract.DataEntry.Test
{
    /// <summary>
    /// Provides test cases for various data entry utility functions.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("DataEntryUtilities")]
    public class TestDataEntryUtilities
    {
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
        }


        static void CheckResult(string expected, string converted)
        {
            Assert.AreEqual(expected, converted);
        }

        /// <summary>
        /// Tests the format date utility function.
        /// Anything in the past 100 years should resolve to the most recent date
        /// IE: if today is 1/1/2019, any short date ending in 19 should resolve to 2019
        /// However if todays date is still 1/1/2019, ad we are given a short date of 20
        /// It should resolve to 1/1/1920
        /// </summary>
        [Test, Category("FormatDateTest")]
        public static void TestFormatDate()
        {
            // single digit month
            CheckResult(expected: "02/14/2004", converted: Util.FormatDate("2/14/2004"));
            
            var currentTwoDigitYear = DateTime.Now.Year % 100;
            var nextTwoDigitYear = currentTwoDigitYear + 1;
            // If the year is currently 1/1/2019, this will return 1/1/1920, so add one to the year, then subtract 100.
            string previousCenturyYear = Invariant($"{(DateTime.Now.Year / 100) - 1}{nextTwoDigitYear}");

            // single digit month and day, 2 digit year
            CheckResult(expected: "02/04/" + previousCenturyYear, converted: Util.FormatDate("2/4/" + nextTwoDigitYear));
            CheckResult(expected: "02/01/" + DateTime.Now.Year.ToString(CultureInfo.InvariantCulture), converted: Util.FormatDate("2/1/" + currentTwoDigitYear)); 

            CheckResult(expected: "02/01/2012", converted: Util.FormatDate("2/1/2012"));
            
            // invalid dates
            CheckResult(expected: "", converted: Util.FormatDate("13/14/2004"));  // invalid month
            CheckResult(expected: "", converted: Util.FormatDate("02/31/2004"));  // invalid day

            // 6 digit date test
            CheckResult(expected: "12/15/" + previousCenturyYear, converted: Util.FormatDate("1215" + nextTwoDigitYear));

            // 8 digit date test
            CheckResult(expected: "12/15/1958", converted: Util.FormatDate("12151958"));

            // hyphen separators
            CheckResult(expected: "02/04/" + previousCenturyYear, converted: Util.FormatDate("2-4-" + nextTwoDigitYear));
            CheckResult(expected: "02/14/2004", converted: Util.FormatDate("2-14-2004"));

            // dot separators
            CheckResult(expected: "02/14/2004", converted: Util.FormatDate("02.14.2004"));
            CheckResult(expected: "02/14/2004", converted: Util.FormatDate("2.14.2004"));
            CheckResult(expected: "02/14/" + DateTime.Now.Year.ToString(CultureInfo.InvariantCulture), converted: Util.FormatDate("2.14." + currentTwoDigitYear));

            // Dates with time components
            CheckResult(expected: "02/02/" + DateTime.Now.Year.ToString(CultureInfo.InvariantCulture), converted: Util.FormatDateWithOptionalTime("2.2." + currentTwoDigitYear + " 1:45 AM"));
            CheckResult(expected: "02/02/2016", converted: Util.FormatDateWithOptionalTime("2.2.2016 1:45 AM"));
        }

        /// <summary>
        /// Tests the format date utility function.
        /// </summary>
        [Test, Category("FormatDateTest")]
        public static void TestFormatDateTryParseMethod()
        {
            var currentTwoDigitYear = DateTime.Now.Year % 100;
            // https://extract.atlassian.net/browse/ISSUE-14370
            CheckResult(expected: "08/07/2014", converted: Util.FormatDate("2014/08/07"));
            CheckResult(expected: "08/07/" + DateTime.Now.Year, converted: Util.FormatDate("08-07-" + currentTwoDigitYear));
            CheckResult(expected: "08/07/" + DateTime.Now.Year, converted: Util.FormatDate("08 07 " + currentTwoDigitYear));
            CheckResult(expected: "08/07/" + DateTime.Now.Year, converted: Util.FormatDate("07 Aug " + currentTwoDigitYear));
            CheckResult(expected: "08/07/" + DateTime.Now.Year, converted: Util.FormatDate("07 August " + currentTwoDigitYear));
            CheckResult(expected: "08/07/2014", converted: Util.FormatDate("Aug 7, 2014"));
            CheckResult(expected: "08/07/2014", converted: Util.FormatDate("August 7, 2014"));

            // Test that future years are not output
            var toParse = DateTime.Now.AddYears(1).ToString("MM dd yy", CultureInfo.CurrentCulture);
            var expected = DateTime.Now.AddYears(-99).ToString("MM/dd/yyyy", CultureInfo.CurrentCulture);
            CheckResult(expected: expected, converted: Util.FormatDate(toParse));
        }

        /// <summary>
        /// Tests the format time utility fuction.
        /// </summary>
        [Test, Category("FormatTimeTest")]
        public static void TestFormatTime()
        {
            CheckResult(expected: "09:15 AM", converted: Util.FormatTime("08/08/2008 09:15 AM"));

            CheckResult(expected: "09:15 AM", converted: Util.FormatTime("9:15 AM"));

            CheckResult(expected: "03:01 AM", converted: Util.FormatTime("3:01 AM"));

            CheckResult(expected: "06:35", converted: Util.FormatTime("06:35:00"));

            CheckResult(expected: "05:33", converted: Util.FormatTime("0533"));

            CheckResult(expected: "09:09", converted: Util.FormatTime("09:09"));

            CheckResult(expected: "03:01 AM", converted: Util.FormatTime("Time: 3:01 AM"));

            CheckResult(expected: "07:54", converted: Util.FormatTime("10/13/2008 07:54:00"));

            CheckResult(expected: "09:00 AM", converted: Util.FormatTime("10/13/2008 9:00 AM"));

            CheckResult(expected: "20:40", converted: Util.FormatTime("Oct 31 20:40:05 2000"));

            CheckResult(expected: "09:18", converted: Util.FormatTime("08/08/2008 9:18"));

            CheckResult(expected: "09:00 AM", converted: Util.FormatTime("9:00AM"));

            CheckResult(expected: "21:00", converted: Util.FormatTime("9:00 PM"));
        }
    }
}
