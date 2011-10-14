using Extract;
using Extract.DataEntry.Utilities.DataEntryApplication;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.Utilities.Forms;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Extract.DataEntry.DEP.StandardLabDE.Test
{
    /// <summary>
    /// Class for testing the StandardLabDE data entry panel
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("StandardLabDE")]
    // Since this class owns a disposable object, FXCop wants this class to implement IDisposable.
    // This class is an NUnit testing object and takes care of the creation and disposing in
    // the test fixture setup and tear down and therefore there is no need to implement IDisposable
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    partial class TestStandardLabDE
    {
        #region Constants

        // Basic image and VOA file with Hematology Test
        private static readonly string _BASIC_HEMATOLOGY = "Resources.Image221.tif";
        private static readonly string _BASIC_HEMATOLOGY_VOA = "Resources.Image221.tif.voa";

        // Basic image and VOA file with Metabolic Panel Test
        private static readonly string _BASIC_METABOLIC = "Resources.Image151.tif";
        private static readonly string _BASIC_METABOLIC_VOA = "Resources.Image151.tif.voa";

        // Image and VOA file with Blood Chemistry Test
        private static readonly string _BLOOD_CHEMISTRY = "Resources.Image003.tif";
        private static readonly string _BLOOD_CHEMISTRY_VOA = "Resources.Image003.tif.voa";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The Data Entry Application form being tested.
        /// </summary>
        private DataEntryApplicationForm _dataEntryApplicationForm;

        /// <summary>
        /// The Image Viewer being tested.
        /// </summary>
        private ImageViewer _imageViewer;

        /// <summary>
        /// Manages the testing files.
        /// </summary>
        TestFileManager<TestStandardLabDE> _testFiles = new TestFileManager<TestStandardLabDE>();

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
            Assembly assembly = Assembly.GetAssembly(typeof(TestStandardLabDE));
            CopyRulesFiles(assembly);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
        public void FinalCleanup()
        {
            // Clean up the Data Entry Application form
            CleanUpDataEntryApplicationForm();

            // Dispose of the testing images
            if (_testFiles != null)
            {
                _testFiles.Dispose();
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
        /// <see cref="TestStandardLabDE"/> for each test case.</remarks>
        [SetUp]
        public void Setup()
        {
            // Ensure that all licensed components are enabled and the cache is reset
            GeneralMethods.ResetLicenseState();

            // Clean up the Data Entry Application form
            CleanUpDataEntryApplicationForm();

            // Find the config file
//            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//            string configFileName = Path.Combine(path, "Demo Solution\\StandardLabDE.config");
            string configFileName = 
                "D:\\Engineering\\binaries\\debug\\Demo Solution\\StandardLabDE.config";

            // Create new form
            _dataEntryApplicationForm = new DataEntryApplicationForm(configFileName);

            // Get the Image Viewer for this form
            _imageViewer = FormMethods.GetFormComponent<ImageViewer>(_dataEntryApplicationForm);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Cleans up the instantiated Data Entry Application form.
        /// </summary>
        private void CleanUpDataEntryApplicationForm()
        {
            // Dispose of previous form
            if (_dataEntryApplicationForm != null)
            {
                _dataEntryApplicationForm.Hide();

                _dataEntryApplicationForm.Dispose();
                _dataEntryApplicationForm = null;
            }
        }

        /// <summary>
        /// Opens the test image in the specified image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer that will own the image.</param>
        /// <param name="imageResource">The name of the embedded resource image to open.</param>
        private void OpenTestImage(ImageViewer imageViewer, string imageResource)
        {
            try
            {
                // Open the test image without updating the MRU list
                imageViewer.OpenImage(_testFiles.GetFile(imageResource), false);
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI25437", ex);
                throw;
            }
        }

        /// <summary>
        /// Opens the test image in the specified image viewer and writes the associated VOA 
        /// file next to the image file.
        /// </summary>
        /// <param name="imageViewer">The image viewer that will own the image.</param>
        /// <param name="imageResource">The name of the embedded resource image to open.</param>
        /// <param name="voaResource">The name of the embedded VOA resource to open.</param>
        private void OpenTestImageAndVOA(ImageViewer imageViewer, string imageResource, 
            string voaResource)
        {
            try
            {
                // Get the filename of the image
                string imageFile = _testFiles.GetFile(imageResource);

                // Append ".voa" and open the associated VOA file
                string voaFile = imageFile + ".voa";
                _testFiles.GetFile(voaResource, voaFile);
                
                // Open the test image without updating the MRU list
                imageViewer.OpenImage(_testFiles.GetFile(imageResource), false);
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI25577", ex);
                throw;
            }
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
                throw ExtractException.AsExtractException("ELI25438", ex);
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
                throw ExtractException.AsExtractException("ELI25439", ex);
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
