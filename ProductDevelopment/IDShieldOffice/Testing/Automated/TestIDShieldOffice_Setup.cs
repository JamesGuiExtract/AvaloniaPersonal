using Extract;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Rules;
using Extract.Testing.Utilities;
using Extract.Utilities.Forms;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace IDShieldOffice.Test
{
    /// <summary>
    /// Class for testing the ID Shield Office application
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("IDShieldOffice")]
    // Since this class owns a disposable object, FXCop wants this class to implement IDisposable.
    // This class is an NUnit testing object and takes care of the creation and disposing in
    // the test fixture setup and tear down and therefore there is no need to implement IDisposable
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    partial class TestIDShieldOffice
    {
        #region Constants

        /// <summary>
        /// Constant string that indicates a line break in the rule test text string.
        /// </summary>
        private static readonly string _LINE_BREAK = "/LINE/";

        /// <summary>
        /// Constant string that indicates a page break in the rule test text string.
        /// </summary>
        private static readonly string _PAGE_BREAK = "/PAGE/";

        /// <summary>
        /// The license string for the SandDock manager
        /// </summary>
        private static readonly string _SANDOCK_LICENSE_STRING =
            @"1970|siE7SnF/jzINQg1AOTIaCXLlouA=";

        // All other account number types
        private static readonly string _ACCOUNT_NUMBER = "Resources.AccountNumber.tif";

        // General testing
        private static readonly string _FIND_TEXT_TEST = "Resources.FindTextTest.tif";

        // Debit and credit card account numbers
        private static readonly string _DEBIT_CREDIT = "Resources.DebitCredit.tif";

        // Drivers license numbers
        private static readonly string _DRIVERS_LICENSE = "Resources.DriversLicenseNumber.tif";

        // Email addresses
        private static readonly string _EMAIL = "Resources.Email.tif";

        // Savings and checking account numbers
        private static readonly string _SAVINGS_CHECKING = "Resources.SavingsChecking.tif";

        // Social security numbers
        private static readonly string _SOCIAL_SECURITY_NUMBER =
            "Resources.SocialSecurityNumber.tif";

        // Federal tax identification numbers
        private static readonly string _TAX_ID = "Resources.TaxID.tif";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The IDSO form being tested.
        /// </summary>
        private IDShieldOfficeForm _idShieldOfficeForm;

        /// <summary>
        /// Manages the testing images.
        /// </summary>
        TestFileManager<TestIDShieldOffice> _testImages =
            new TestFileManager<TestIDShieldOffice>();

        #endregion

        #region Fixture Setup and Teardown

        /// <summary>
        /// Performs initialization needed for the entire test run.
        /// </summary>
        [TestFixtureSetUp]
        static public void Initialize()
        {
            // Perform basic test setup (loads license files)
            GeneralMethods.TestSetup();

            // Copy the rules files
            Assembly assembly = Assembly.GetAssembly(typeof(TestIDShieldOffice));
            CopyRulesFiles(assembly);

            // License SandDock
            TD.SandDock.SandDockManager.ActivateProduct(_SANDOCK_LICENSE_STRING);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
        public void FinalCleanup()
        {
            // Clean up the IDSO form
            CleanUpIdsoForm();

            // Dispose of the testing images
            if (_testImages != null)
            {
                _testImages.Dispose();
            }

            // Delete the copied rules files
            DeleteRulesFiles();
        }

        #endregion

        #region Test Setup and Teardown

        /// <summary>
        /// Performs initialization needed before each test.
        /// </summary>
        /// <remarks>In order to ensure accurate testing we dispose of and create a new
        /// <see cref="TestIDShieldOffice"/> for each test case.</remarks>
        [SetUp]
        public void Setup()
        {
            // Ensure that all licensed components are enabled and the cache is reset
            GeneralMethods.ResetLicenseState();

            // Clean up the IDSO form
            CleanUpIdsoForm();

            // Create new form
            _idShieldOfficeForm = new IDShieldOfficeForm();
            _idShieldOfficeForm.UserPreferences.ResetToDefaults();

            // Reset the fit mode
            _idShieldOfficeForm.ImageViewer.FitMode = FitMode.None;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Cleans up the instantiated IDSO form.
        /// </summary>
        private void CleanUpIdsoForm()
        {
            // Dispose of previous form
            if (_idShieldOfficeForm != null)
            {
                _idShieldOfficeForm.Hide();

                _idShieldOfficeForm.Dispose();
                _idShieldOfficeForm = null;
            }
        }

        /// <summary>
        /// Opens the test image in the specified image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer that will own the image.</param>
        private void OpenTestImage(ImageViewer imageViewer)
        {
            try
            {
                // Open the test image without updating the MRU list
                imageViewer.OpenImage(_testImages.GetFile(_FIND_TEXT_TEST), false);
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI22201", ex);
                throw;
            }
        }

        /// <summary>
        /// Performs the search based upon the specified rule on the specified text and
        /// compares the results to the expected results.
        /// </summary>
        /// <param name="rule">The rule to perform the find with.</param>
        /// <param name="textToSearch">The text to be searched.
        /// <para><b>Note:</b></para>
        /// Use the constant _LINE_BREAK to indicate a line break 
        /// and _PAGE_BREAK to indicate a page break.
        /// <param name="expectedResults">The array of expected strings.
        /// <para><b>Note:</b></para>
        /// These should be listed in the order they are expected to be found.</param>
        /// <returns>Whether the found results match the expected results.</returns>
        private static bool TestFindingRuleOnText(IRule rule,
            string textToSearch, string[] expectedResults)
        {
            int lineMultiplier = 100;
            int lineHeight = lineMultiplier / 2;

            // Write the test string to the console for debugging purposes
            Console.WriteLine("String: " + textToSearch);

            // Write the expected results to the console for debugging purposes
            for (int i = 0; i < expectedResults.Length; i++)
            {
                Console.WriteLine("Expected "
                    + i.ToString(System.Globalization.CultureInfo.CurrentCulture)
                    + ": " + expectedResults[i]);
            }

            // Build an array of pages
            string[] pagesToSearch = textToSearch.Split(new string[] { _PAGE_BREAK },
                StringSplitOptions.RemoveEmptyEntries);

            // Holds the maximum number of lines on any page
            int maxLinesOnPage = 0;

            // Build a collection of pages and associated strings
            Dictionary<int, string[]> dataToSearch = new Dictionary<int, string[]>();
            for (int i = 0; i < pagesToSearch.Length; i++)
            {
                string[] linesToSearch = pagesToSearch[i].Split(new string[] { _LINE_BREAK },
                    StringSplitOptions.RemoveEmptyEntries);

                // Append a new line to each line
                for (int j = 0; j < linesToSearch.Length; j++)
                {
                    linesToSearch[j] += Environment.NewLine;
                }

                // Set the maximum number of lines
                maxLinesOnPage = Math.Max(maxLinesOnPage, linesToSearch.Length);

                // Add the collection of lines to the data collection
                dataToSearch.Add(i, linesToSearch);
            }

            // Create spatial page info object
            UCLID_RASTERANDOCRMGMTLib.SpatialPageInfo spatialPageInfo =
                new UCLID_RASTERANDOCRMGMTLib.SpatialPageInfo();
            spatialPageInfo.Deskew = 0.0;
            spatialPageInfo.Height = maxLinesOnPage * 2 * lineMultiplier;
            spatialPageInfo.Width = 1500;
            spatialPageInfo.Orientation = UCLID_RASTERANDOCRMGMTLib.EOrientation.kRotNone;

            // Create fake ocr text
            UCLID_RASTERANDOCRMGMTLib.SpatialString fakeOcrText =
                new UCLID_RASTERANDOCRMGMTLib.SpatialString();

            // Create spatial page info map
            UCLID_COMUTILSLib.LongToObjectMap spatialPageInfoMap =
                new UCLID_COMUTILSLib.LongToObjectMap();
            for (int i = 0; i < pagesToSearch.Length; i++)
            {
                // Set the spatial page info map for the current page
                spatialPageInfoMap.Set(i + 1, spatialPageInfo);

                // Get the strings for this page
                string[] linesToSearch;
                if (!dataToSearch.TryGetValue(i, out linesToSearch))
                {
                    ExtractException ee = new ExtractException("ELI22236",
                        "No line data for the specified page!");
                    ee.AddDebugData("Page number", i + 1, false);
                    throw ee;
                }

                // For each line on the page, build a spatial string and append
                // it to the fake ocr text spatial string
                for (int j = 0; j < linesToSearch.Length; j++)
                {
                    // Compute the y value for this line
                    int yValue = (j + 1) * lineMultiplier;

                    // Build the temp spatial string
                    UCLID_RASTERANDOCRMGMTLib.SpatialString tempString =
                        new UCLID_RASTERANDOCRMGMTLib.SpatialString();
                    tempString.CreatePseudoSpatialString(
                        (new RasterZone(0, yValue, linesToSearch[j].Length,
                        yValue, lineHeight, i + 1)).ToComRasterZone(),
                        linesToSearch[j], "FakeDoc.tif", spatialPageInfoMap);

                    // Append it to the master spatial string
                    fakeOcrText.Append(tempString);
                }
            }

            // Get the matches
            MatchResultCollection matches = rule.GetMatches(fakeOcrText);

            // Build the results collection
            string[] results = new string[matches.Count];
            for (int j = 0; j < matches.Count; j++)
            {
                // Get a match from the match results
                MatchResult match = matches[j];

                // Build the result string
                StringBuilder resultString = new StringBuilder();
                foreach (RasterZone rasterZone in match.RasterZones)
                {
                    // Get the page number
                    int pageNumber = rasterZone.PageNumber;

                    // Get the lines for the page
                    string[] linesToSearch;
                    if (!dataToSearch.TryGetValue(pageNumber - 1, out linesToSearch))
                    {
                        ExtractException ee = new ExtractException("ELI22237",
                            "No line data for the specified page!");
                        ee.AddDebugData("Page number", pageNumber, false);
                        throw ee;
                    }

                    // Get the line from the raster zone
                    int line = rasterZone.StartY / lineMultiplier;

                    // Get the substring from that line
                    resultString.Append(linesToSearch[line - 1].Substring(rasterZone.StartX,
                        (rasterZone.EndX - rasterZone.StartX)));
                }

                // Get the result string
                results[j] = resultString.ToString();

                // Write the result to the console for debugging purposes
                Console.WriteLine("Result "
                    + j.ToString(System.Globalization.CultureInfo.CurrentCulture) +
                    ": " + results[j]);
            }

            // Compare the match results
            bool expectedResultsMatch = results.Length == expectedResults.Length;
            if (expectedResultsMatch)
            {
                // Compare each result with the expected results
                for (int j = 0; j < results.Length; j++)
                {
                    if (results[j] != expectedResults[j])
                    {
                        expectedResultsMatch = false;
                        break;
                    }
                }
            }

            // Return the results
            return expectedResultsMatch;
        }

        /// <summary>
        /// Copies the rules files from their default location to the shadow-copied location.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to use to compute paths.</param>
        private static void CopyRulesFiles(Assembly assembly)
        {
            try
            {
                // Get the directory paths
                string source = Path.GetDirectoryName(assembly.CodeBase) + @"\..\Rules";
                source = Path.GetFullPath(source.Substring(6));
                string destination = Path.GetDirectoryName(Application.ExecutablePath) + @"\..\Rules";
                destination = Path.GetFullPath(destination);

                // Copy the files
                Extract.Utilities.FileSystemMethods.CopyDirectory(source, destination, true);

                // Ensure the files are writeable
                Extract.Utilities.FileSystemMethods.MakeWritable(destination, true);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23085", ex);
            }
        }

        /// <summary>
        /// Deletes the rules files that were copied to the shadow-copy location.
        /// </summary>
        private static void DeleteRulesFiles()
        {
            try
            {
                // Get the path where the files were copied to
                string path = Path.GetDirectoryName(Application.ExecutablePath) + @"\..\Rules";
                path = Path.GetFullPath(path);

                // Ensure that it still exists
                if (Directory.Exists(path))
                {
                    // Recursively delete the rules files
                    Directory.Delete(path, true);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23086", ex);
            }
        }

        /// <summary>
        /// Will show a modeless message box containing a list of instructions for performing
        /// the current interactive test.
        /// <para><b>Note:</b></para>
        /// This method will not return until the dialog box has closed.
        /// </summary>
        /// <param name="instructions">An array of <see cref="string"/> objects containing
        /// the instructions for performing the current test.</param>
        private static void ShowModelessInstructionsAndWait(string[] instructions)
        {
            // Create a StringBuilder to hold the message to display
            StringBuilder sb = new StringBuilder("Here are your instructions\n\n");

            // Build the instruction list
            int i = 1;
            foreach (string instruction in instructions)
            {
                sb.Append(i.ToString(CultureInfo.CurrentCulture));
                sb.Append(". ");
                sb.Append(instruction);
                sb.Append("\n");
                i++;
            }

            // Add a polite ending
            sb.Append("\nThank you.");

            // Build and display the modeless dialog box
            using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
            {
                // Build the modeless dialog
                messageBox.AddStandardButtons(MessageBoxButtons.OK);
                messageBox.Caption = "Test instructions";
                messageBox.StandardIcon = MessageBoxIcon.Information;
                messageBox.Text = sb.ToString();

                // Show the modeless dialog
                messageBox.ShowModeless();

                // Loop until the dialog has closed
                while (messageBox.IsVisible)
                {
                    // Process all messages in the queue
                    Application.DoEvents();
                }
            }
        }

        #endregion
    }
}
