using Extract;
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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Forms.Test
{
    /// <summary>
    /// Class for testing the Image Viewer control
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("ImageViewer")]
    // Since this class owns a disposable object, FXCop wants this class to implement IDisposable.
    // This class is an NUnit testing object and takes care of the creation and disposing in
    // the test fixture setup and tear down and therefore there is no need to implement IDisposable
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public partial class TestImageViewer
    {
        #region Fields

        /// <summary>
        /// The name of the embedded resource test image file.
        /// </summary>
        private static readonly string _TEST_IMAGE_FILE = "Resources.TestImage.tif";

        /// <summary>
        /// The name of the embedded resource test image uss file.
        /// </summary>
        private static readonly string _TEST_IMAGE_USS_FILE = "Resources.TestImage.tif.uss";

        /// <summary>
        /// The name of the embedded resource annotation test image file.
        /// </summary>
        private static readonly string _TEST_ANNOTATION_IMAGE_FILE =
            "Resources.TestAnnotationTypes.tif";

        /// <summary>
        /// The name of the embedded resource rotated view perspective test image file.
        /// </summary>
        private static readonly string _TEST_ROTATED_VIEW_PERSPECTIVE_FILE =
            "Resources.TestRotatedViewPerspective.tif";

        /// <summary>
        /// The name of the embedded resource rotated view perspective 
        /// with annotations test image file.
        /// </summary>
        private static readonly string _TEST_ROTATED_VIEW_PERSPECTIVE_ANNOTATION_FILE =
            "Resources.TestRotatedVPWithAnnotations.tif";

        /// <summary>
        /// The image viewer application form
        /// </summary>
        private TestImageViewerForm _imageViewerForm;

        /// <summary>
        /// Manages the test images needed for testing the <see cref="ImageViewer"/>.
        /// </summary>
        private TestFileManager<TestImageViewer> _testImages =
            new TestFileManager<TestImageViewer>();

        #endregion

        #region Fixture Setup and Teardown

        /// <summary>
        /// Performs initialization needed for the entire test run.
        /// </summary>
        [OneTimeSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public void FinalCleanup()
        {
            if (_imageViewerForm != null)
            {
                _imageViewerForm.Dispose();
                _imageViewerForm = null;
            }

            // Dispose of the test image manager
            if (_testImages != null)
            {
                _testImages.Dispose();
            }
        }

        #endregion

        #region Test Setup and Teardown

        /// <summary>
        /// Performs initialization needed before each test.
        /// </summary>
        /// <remarks>In order to ensure accurate testing we dispose of and create a new
        /// <see cref="TestImageViewerForm"/> for each test case.</remarks>
        [SetUp]
        public void Setup()
        {
            // Ensure that all licensed components are enabled and the cache is reset
            GeneralMethods.ResetLicenseState();

            if (_imageViewerForm != null)
            {
                _imageViewerForm.Hide();
                _imageViewerForm.Dispose();
                _imageViewerForm = null;
            }

            _imageViewerForm = new TestImageViewerForm();

            // Reset the default fit mode
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            imageViewer.FitMode = FitMode.None;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Opens the test image in the specified image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer that will own the image.</param>
        private void OpenTestImage(ImageViewer imageViewer)
        {
            try
            {
                // Open the test image without updating the MRU list
                imageViewer.OpenImage(_testImages.GetFile(_TEST_IMAGE_FILE), false);
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI21658", ex);
                throw;
            }
        }

        /// <summary>
        /// Opens the annotation test image in the specified image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer that will own the image.</param>
        private void OpenAnnotationTestImage(ImageViewer imageViewer)
        {
            try
            {
                // Open the test image without updating the MRU list
                imageViewer.OpenImage(_testImages.GetFile(_TEST_ANNOTATION_IMAGE_FILE), false);
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI21918", ex);
                throw;
            }
        }

        /// <summary>
        /// Opens the rotated view perspective test image in the specified image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer that will own the image.</param>
        private void OpenRotatedViewPerspectiveTestImage(ImageViewer imageViewer)
        {
            try
            {
                // Open the test image without updating the MRU list
                imageViewer.OpenImage(
                    _testImages.GetFile(_TEST_ROTATED_VIEW_PERSPECTIVE_FILE), false);
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI21919", ex);
                throw;
            }
        }

        /// <summary>
        /// Opens the rotated view perspective with annotations test image
        /// in the specified image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer that will own the image.</param>
        private void OpenRotatedViewPerspectiveAnnotationTestImage(ImageViewer imageViewer)
        {
            try
            {
                // Open the test image without updating the MRU list
                imageViewer.OpenImage(
                    _testImages.GetFile(_TEST_ROTATED_VIEW_PERSPECTIVE_ANNOTATION_FILE), false);
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI21963", ex);
                throw;
            }

        }

        #endregion
    }
}
