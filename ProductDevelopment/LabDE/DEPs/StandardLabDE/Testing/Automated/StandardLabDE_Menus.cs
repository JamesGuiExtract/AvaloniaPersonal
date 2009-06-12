using Extract;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.Utilities.Forms;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Extract.LabDE.StandardLabDE.Test
{
    public partial class TestStandardLabDE
    {
        #region File Open

        /// <summary>
        /// Test that the <see cref="OpenImageToolStripMenuItem"/> is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FileOpenEnabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the File - Open menu item
            OpenImageToolStripMenuItem fileOpen =
                FormMethods.GetFormComponent<OpenImageToolStripMenuItem>(_dataEntryApplicationForm);

            // Check that the menu item is enabled
            Assert.That(fileOpen.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="OpenImageToolStripMenuItem"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FileOpenEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the File - Open menu item
            OpenImageToolStripMenuItem fileOpen =
                FormMethods.GetFormComponent<OpenImageToolStripMenuItem>(_dataEntryApplicationForm);

            // Check that the menu item is enabled
            Assert.That(fileOpen.Enabled);
        }

        #endregion File Open

        #region File Close

        /// <summary>
        /// Test that the File - Close menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FileCloseDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the File - Close menu item
            ToolStripMenuItem fileClose =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Close");

            // Check that the menu item is disabled
            Assert.That(!fileClose.Enabled);
        }

        /// <summary>
        /// Test that the File - Close menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FileCloseEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the File - Close menu item
            ToolStripMenuItem fileClose =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Close");

            // Check that the menu item is enabled
            Assert.That(fileClose.Enabled);
        }

        #endregion File Close
        
        #region File Save

        /// <summary>
        /// Test that the File - Save menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FileSaveDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the File Save menu item
            ToolStripMenuItem fileSave =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Save");

            // Check that the menu item is disabled
            Assert.That(!fileSave.Enabled);
        }

        /// <summary>
        /// Test that the File - Save menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FileSaveEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the File - Save menu item
            ToolStripMenuItem fileSave =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Save");

            // Check that the menu item is enabled
            Assert.That(fileSave.Enabled);
        }

        #endregion File Save

        #region File Print

        /// <summary>
        /// Test that the <see cref="PrintImageToolStripMenuItem"/> is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FilePrintDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the File - Print menu item
            ToolStripMenuItem filePrint =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Print...");

            // Check that the menu item is disabled
            Assert.That(!filePrint.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="PrintImageToolStripMenuItem"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FilePrintEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the File - Print menu item
            ToolStripMenuItem filePrint =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Print...");

            // Check that the menu item is enabled
            Assert.That(filePrint.Enabled);
        }

        #endregion File Print

        #region File Exit

        /// <summary>
        /// Test that the File - Exit menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FileExitEnabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the File - Exit menu item
            ToolStripMenuItem fileExit =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "E&xit");

            // Check that the menu item is enabled
            Assert.That(fileExit.Enabled);
        }

        /// <summary>
        /// Test that the File - Exit menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FileExitEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the File - Exit menu item
            ToolStripMenuItem fileExit =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "E&xit");

            // Check that the menu item is enabled
            Assert.That(fileExit.Enabled);
        }

        #endregion File Exit

        #region View Zoom Fit To Page

        /// <summary>
        /// Test that the View - Zoom - Fit To Page menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomFitToPageEnabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Zoom - Fit To Page menu item
            ToolStripMenuItem viewZoomFitToPage =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &page");

            // Check that the menu item is enabled
            Assert.That(viewZoomFitToPage.Enabled);
        }

        /// <summary>
        /// Test that the View - Zoom - Fit To Page menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomFitToPageEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the View - Zoom - Fit To Page menu item
            ToolStripMenuItem viewZoomFitToPage =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &page");

            // Check that the menu item is enabled
            Assert.That(viewZoomFitToPage.Enabled);
        }

        /// <summary>
        /// Test that the View - Zoom - Fit To Page menu item works 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripMenuItemNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Set the fit mode to none
            _imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI25557", "Could not change fit mode to none!",
                _imageViewer.FitMode == FitMode.None);

            // Get the fit to page menu item
            ToolStripMenuItem viewZoomFitToPage =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &page");

            // Click the menu item
            viewZoomFitToPage.PerformClick();

            Assert.That(_imageViewer.FitMode == FitMode.FitToPage);
        }

        /// <summary>
        /// Test that the View - Zoom - Fit To Page menu item works 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripMenuItemWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Set the fit mode to none
            _imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI25558", "Could not change fit mode to none!",
                _imageViewer.FitMode == FitMode.None);

            // Get the fit to page menu item
            ToolStripMenuItem viewZoomFitToPage =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &page");

            // Click the menu item
            viewZoomFitToPage.PerformClick();

            Assert.That(_imageViewer.FitMode == FitMode.FitToPage);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Fit To Page menu item is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripMenuItemToggledOnWhenSelectedTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the fit to width menu item
            ToolStripMenuItem viewZoomFitToWidth =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &width");

            // Select the FitToWidth tool
            viewZoomFitToWidth.PerformClick();

            // Get the fit to page menu item
            ToolStripMenuItem viewZoomFitToPage =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &page");

            // Select the FitToPage tool
            viewZoomFitToPage.PerformClick();

            // Check that the menu item is checked
            Assert.That(viewZoomFitToPage.Checked);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Fit To Page menu item is toggled off 
        /// when selected again.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripMenuItemTogglesOffWhenSelectedTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the FitToWidth menu item
            ToolStripMenuItem viewZoomFitToWidth =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &width");

            // Select the FitToWidth menu item
            viewZoomFitToWidth.PerformClick();

            // Get the FitToPage menu item
            ToolStripMenuItem fitToPage =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &page");

            // Select the FitToPage tool twice
            fitToPage.PerformClick();
            fitToPage.PerformClick();

            // Check that the menu item is unchecked
            Assert.That(!fitToPage.Checked);
        }

        #endregion View Zoom Fit To Page

        #region View Zoom Fit To Width

        /// <summary>
        /// Test that the View - Zoom - Fit To Width menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomFitToWidthEnabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Zoom - Fit To Width menu item
            ToolStripMenuItem viewZoomFitToWidth =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &width");

            // Check that the menu item is enabled
            Assert.That(viewZoomFitToWidth.Enabled);
        }

        /// <summary>
        /// Test that the View - Zoom - Fit To Width menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomFitToWidthEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the View - Zoom - Fit To Width menu item
            ToolStripMenuItem viewZoomFitToWidth =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &width");

            // Check that the menu item is enabled
            Assert.That(viewZoomFitToWidth.Enabled);
        }

        /// <summary>
        /// Test that the View - Zoom - Fit To Width menu item works 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripMenuItemNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Set the fit mode to none
            _imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI25560", "Could not change fit mode to none!",
                _imageViewer.FitMode == FitMode.None);

            // Get the fit to width menu item
            ToolStripMenuItem fitToWidth =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &width");

            // Click the menu item
            fitToWidth.PerformClick();

            Assert.That(_imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Test that the View - Zoom - Fit To Width menu item works 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripMenuItemWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Set the fit mode to none
            _imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI25561", "Could not change fit mode to none!",
                _imageViewer.FitMode == FitMode.None);

            // Get the fit to width menu item
            ToolStripMenuItem fitToWidth = 
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &width");

            // Click the menu item
            fitToWidth.PerformClick();

            Assert.That(_imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Fit To Width menu item is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripMenuItemToggledOnWhenSelectedTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the FitToPage menu item
            ToolStripMenuItem fitToPage = 
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &page");

            // Select the FitToPage tool
            fitToPage.PerformClick();

            // Get the FitToWidth menu item
            ToolStripMenuItem fitToWidth =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &width");

            // Select the FitToWidth tool
            fitToWidth.PerformClick();

            // Check that the menu item is checked
            Assert.That(fitToWidth.Checked);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Fit To Width menu item is toggled off 
        /// when selected again.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripMenuItemTogglesOffWhenSelectedTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the FitToPage menu item
            ToolStripMenuItem fitToPage = 
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &page");

            // Select the FitToPage tool
            fitToPage.PerformClick();

            // Get the FitToWidth menu item
            ToolStripMenuItem fitToWidth =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Fit to &width");

            // Select the FitToWidth tool twice
            fitToWidth.PerformClick();
            fitToWidth.PerformClick();

            // Check that the menu item is unchecked
            Assert.That(!fitToWidth.Checked);
        }

        #endregion View Zoom Fit To Width

        #region View Zoom Zoom In

        /// <summary>
        /// Test that the View - Zoom - Zoom In menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomZoomInDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Zoom - Zoom In menu item
            ToolStripMenuItem viewZoomIn =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom in");

            // Check that the menu item is disabled
            Assert.That(!viewZoomIn.Enabled);
        }

        /// <summary>
        /// Test that the View - Zoom - Zoom In menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomZoomInEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the View - Zoom - Zoom In menu item
            ToolStripMenuItem viewZoomIn =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom in");

            // Check that the menu item is enabled
            Assert.That(viewZoomIn.Enabled);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Zoom In menu item zooms in  
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripMenuItemZoomTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the zoom level
            double zoomLevel = _imageViewer.ZoomInfo.ScaleFactor;

            // Get the Zoom In menu item
            ToolStripMenuItem zoomIn =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom in");

            // Zoom in
            zoomIn.PerformClick();

            // Check that the image zoomed in
            Assert.That(_imageViewer.ZoomInfo.ScaleFactor > zoomLevel);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Zoom In menu item adds a zoom history entry.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripMenuItemAddsZoomHistoryTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the current Zoom history count
            int zoomHistoryCount = _imageViewer.ZoomHistoryCount;

            // Click the Zoom In menu item
            ToolStripMenuItem clickMe = 
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom in");
            clickMe.PerformClick();

            // Check that exactly one zoom history record has been added
            Assert.That((zoomHistoryCount + 1) == _imageViewer.ZoomHistoryCount);
        }

        #endregion View Zoom Zoom In

        #region View Zoom Zoom Out

        /// <summary>
        /// Test that the View - Zoom - Zoom Out menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomZoomOutDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Zoom - Zoom Out menu item
            ToolStripMenuItem viewZoomOut =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom out");

            // Check that the menu item is disabled
            Assert.That(!viewZoomOut.Enabled);
        }

        /// <summary>
        /// Test that the View - Zoom - Zoom Out menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomZoomOutEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the View - Zoom - Zoom Out menu item
            ToolStripMenuItem viewZoomOut =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom out");

            // Check that the menu item is enabled
            Assert.That(viewZoomOut.Enabled);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Zoom Out menu item zooms out 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripMenuItemZoomTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the zoom level
            double zoomLevel = _imageViewer.ZoomInfo.ScaleFactor;

            // Get the Zoom Out menu item
            ToolStripMenuItem zoomOut =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom out");

            // Zoom out
            zoomOut.PerformClick();

            // Check that the image zoomed out
            Assert.That(_imageViewer.ZoomInfo.ScaleFactor < zoomLevel);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Zoom Out menu item adds a zoom history entry.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripMenuItemAddsZoomHistoryTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the current Zoom history count
            int zoomHistoryCount = _imageViewer.ZoomHistoryCount;

            // Click the ZoomOutToolStripMenuItem
            ToolStripMenuItem zoomOut = 
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom out");
            zoomOut.PerformClick();

            // Check that exactly one zoom history record has been added
            Assert.That((zoomHistoryCount + 1) == _imageViewer.ZoomHistoryCount);
        }

        #endregion View Zoom Zoom Out

        #region View Zoom Zoom Previous

        /// <summary>
        /// Test that the View - Zoom - Zoom Previous menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomZoomPreviousDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Zoom - Zoom Previous menu item
            ToolStripMenuItem viewZoomPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom previous");

            // Check that the menu item is disabled
            Assert.That(!viewZoomPrevious.Enabled);
        }

        /// <summary>
        /// Test that the View - Zoom - Zoom Previous menu item is disabled 
        /// without a previous zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomZoomPreviousDisabledWithoutHistoryTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the View - Zoom - Zoom Previous menu item
            ToolStripMenuItem viewZoomPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom previous");

            // Check that the menu item is disabled
            Assert.That(!viewZoomPrevious.Enabled);
        }

        /// <summary>
        /// Tests that the View - Zoom - Zoom Previous menu item is enabled 
        /// with a previous zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomZoomPreviousEnabledWithHistoryTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Create a previous zoom history entry
            _imageViewer.ZoomIn();

            // Get the View - Zoom - Zoom Previous menu item
            ToolStripMenuItem viewZoomPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom previous");

            // Check that the menu item is enabled
            Assert.That(viewZoomPrevious.Enabled);
        }

        /// <summary>
        /// Tests that the View - Zoom - Zoom Previous menu item zooms to 
        /// a previous zoom item.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomZoomPreviousZoomsToPreviousTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the zoom level
            double zoomLevelOriginal = _imageViewer.ZoomInfo.ScaleFactor;

            // Create a previous zoom history entry
            _imageViewer.ZoomIn();

            // Get the new zoom level
            double zoomLevelNew = _imageViewer.ZoomInfo.ScaleFactor;

            // Get the View - Zoom - Zoom Previous menu item
            ToolStripMenuItem viewZoomPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom previous");

            // Zoom to previous history item
            viewZoomPrevious.PerformClick();

            // Check that the zoom level changed back to original AND
            // that the original zoom level is different than new zoom level
            Assert.That(_imageViewer.ZoomInfo.ScaleFactor == zoomLevelOriginal &&
                zoomLevelOriginal != zoomLevelNew);
        }

        #endregion View Zoom Zoom Previous

        #region View Zoom Zoom Next

        /// <summary>
        /// Test that the View - Zoom - Zoom Next menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomZoomNextDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Zoom - Zoom Next menu item
            ToolStripMenuItem viewZoomNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom next");

            // Check that the menu item is disabled
            Assert.That(!viewZoomNext.Enabled);
        }

        /// <summary>
        /// Test that the View - Zoom - Zoom Next menu item is disabled 
        /// without a next zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomZoomNextDisabledWithoutHistoryTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the View - Zoom - Zoom Next menu item
            ToolStripMenuItem viewZoomNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom next");

            // Check that the menu item is disabled
            Assert.That(!viewZoomNext.Enabled);
        }

        /// <summary>
        /// Tests that the View - Zoom - Zoom Next menu item is enabled 
        /// with a next zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomZoomNextEnabledWithHistoryTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Create a next zoom history entry
            _imageViewer.ZoomIn();
            _imageViewer.ZoomPrevious();

            // Get the View - Zoom - Zoom Next menu item
            ToolStripMenuItem viewZoomNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom next");

            // Check that the menu item is enabled
            Assert.That(viewZoomNext.Enabled);
        }

        /// <summary>
        /// Tests that the View - Zoom - Zoom Next menu item zooms to 
        /// a next zoom item.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomZoomNextZoomsToNextTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Zoom in and store the zoom level
            _imageViewer.ZoomIn();
            double zoomLevelIn = _imageViewer.ZoomInfo.ScaleFactor;

            // Zoom previous and get the previous zoom level
            _imageViewer.ZoomPrevious();
            double zoomLevelPrevious = _imageViewer.ZoomInfo.ScaleFactor;

            // Get the View - Zoom - Zoom Next menu item
            ToolStripMenuItem viewZoomNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Zoom next");

            // Zoom to next history item
            viewZoomNext.PerformClick();

            // Check that the zoom level changed back to the zoomed in value AND
            // that the previous zoom level is different than new zoom level
            Assert.That(_imageViewer.ZoomInfo.ScaleFactor == zoomLevelIn &&
                zoomLevelIn != zoomLevelPrevious);
        }

        #endregion View Zoom Zoom Next

        #region View Rotate Counterclockwise

        /// <summary>
        /// Test that the View - Rotate - Counterclockwise menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewRotateCounterclockwiseDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Rotate - Counterclockwise menu item
            ToolStripMenuItem viewRotateCounterclockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Rotate counterclockwise");

            // Check that the menu item is disabled
            Assert.That(!viewRotateCounterclockwise.Enabled);
        }

        /// <summary>
        /// Test that the View - Rotate - Counterclockwise menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewRotateCounterclockwiseEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the View - Rotate - Counterclockwise menu item
            ToolStripMenuItem viewRotateCounterclockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Rotate counterclockwise");

            // Check that the menu item is enabled
            Assert.That(viewRotateCounterclockwise.Enabled);
        }

        /// <summary>
        /// Test that the View - Rotate - Counterclockwise menu item rotates  
        /// properly without visible items.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRotateCounterclockwiseRotateWithoutItemsTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the View - Rotate - Counterclockwise menu item
            ToolStripMenuItem viewRotateCounterclockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Rotate counterclockwise");

            // Rotate the image
            viewRotateCounterclockwise.PerformClick();

            // Check that the image rotated properly
            Assert.That(MessageBox.Show("Did the image rotate counterclockwise?",
                "Proper image rotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the View - Rotate - Counterclockwise menu item rotates  
        /// only the active page.
        /// </summary>
        [Test, Category("Interactive")]
        public void Automated_ViewRotateCounterclockwiseRotatesActivePageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Get the View - Rotate - Counterclockwise menu item
            ToolStripMenuItem viewRotateCounterclockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Rotate counterclockwise");

            // Rotate the image
            viewRotateCounterclockwise.PerformClick();

            // Move to page 2 and refresh the page
            _imageViewer.PageNumber = 2;
            _dataEntryApplicationForm.Refresh();

            // Check that the image rotated properly
            Assert.That(MessageBox.Show("Is the active page rotated?",
                "Proper image rotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2, 0) == DialogResult.No);
        }

        #endregion View Rotate Counterclockwise

        #region View Rotate Clockwise

        /// <summary>
        /// Test that the View - Rotate - Clockwise menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewRotateClockwiseDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Rotate - Clockwise menu item
            ToolStripMenuItem viewRotateClockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Rotate clockwise");

            // Check that the menu item is disabled
            Assert.That(!viewRotateClockwise.Enabled);
        }

        /// <summary>
        /// Test that the View - Rotate - Clockwise menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewRotateClockwiseEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the View - Rotate - Clockwise menu item
            ToolStripMenuItem viewRotateClockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Rotate clockwise");

            // Check that the menu item is enabled
            Assert.That(viewRotateClockwise.Enabled);
        }

        /// <summary>
        /// Test that the View - Rotate - Clockwise menu item rotates  
        /// properly without visible items.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRotateClockwiseRotateWithoutItemsTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the View - Rotate - Clockwise menu item
            ToolStripMenuItem viewRotateClockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Rotate clockwise");

            // Rotate the image
            viewRotateClockwise.PerformClick();

            // Check that the image rotated properly
            Assert.That(MessageBox.Show("Did the image rotate clockwise?",
                "Proper image rotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the View - Rotate - Clockwise menu item rotates  
        /// only the active page.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRotateClockwiseRotatesActivePageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Get the View - Rotate - Clockwise menu item
            ToolStripMenuItem viewRotateClockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Rotate clockwise");

            // Rotate the image
            viewRotateClockwise.PerformClick();

            // Move to page 2 and refresh the page
            _imageViewer.PageNumber = 2;
            _dataEntryApplicationForm.Refresh();

            // Check that the image rotated properly
            Assert.That(MessageBox.Show("Is the active page rotated?",
                "Proper image rotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2, 0) == DialogResult.No);
        }

        #endregion View Rotate Clockwise

        #region View Go To Page First Page

        /// <summary>
        /// Test that the View - Goto Page - First Page menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageFirstPageDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Goto Page - First Page menu item
            ToolStripMenuItem viewGoToFirst =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "First page");

            // Check that the menu item is disabled
            Assert.That(!viewGoToFirst.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - First Page menu item is disabled 
        /// on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageFirstPageDisabledOnFirstPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the View - Go To Page - First Page menu item
            ToolStripMenuItem viewGoToFirst =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "First page");

            // Check that the menu item is disabled
            Assert.That(!viewGoToFirst.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - First Page menu item is enabled 
        /// when not on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageFirstPageEnabledNotOnFirstPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Move to page 2
            _imageViewer.PageNumber = 2;

            // Get the View - Go To Page - First Page menu item
            ToolStripMenuItem viewGoToFirst =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "First page");

            // Check that the menu item is enabled
            Assert.That(viewGoToFirst.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - First Page menu item navigates 
        /// to the first page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageFirstPageNavigationTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Move to page 2
            _imageViewer.PageNumber = 2;

            // Get the View - Go To Page - First Page menu item
            ToolStripMenuItem viewGoToFirst =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "First page");

            // Move back to the first page
            viewGoToFirst.PerformClick();

            // Check that the first page is active
            Assert.That(_imageViewer.PageNumber == 1);
        }

        #endregion View Go To Page First Page

        #region View Go To Page Previous Page

        /// <summary>
        /// Test that the View - Goto Page - Previous Page menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPagePreviousPageDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Goto Page - Previous Page menu item
            ToolStripMenuItem viewGoToPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Previous page");

            // Check that the menu item is disabled
            Assert.That(!viewGoToPrevious.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - Previous Page menu item is disabled 
        /// when on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPagePreviousPageDisabledOnFirstPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Get the View - Go To Page - Previous Page menu item
            ToolStripMenuItem viewGoToPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Previous page");

            // Check that the menu item is disabled
            Assert.That(!viewGoToPrevious.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - Previous Page menu item is enabled 
        /// when not on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPagePreviousPageEnabledNotOnFirstPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Move to page 2
            _imageViewer.PageNumber = 2;

            // Get the View - Go To Page - Previous Page menu item
            ToolStripMenuItem viewGoToPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Previous page");

            // Check that the menu item is enabled
            Assert.That(viewGoToPrevious.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - Previous Page menu item navigates 
        /// to the previous page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPagePreviousPageNavigationTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Move to page 2
            _imageViewer.PageNumber = 2;

            // Get the View - Go To Page - Previous Page menu item
            ToolStripMenuItem viewGoToPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Previous page");

            // Move to the previous page
            viewGoToPrevious.PerformClick();

            // Check that the proper page is active
            Assert.That(_imageViewer.PageNumber == 1);
        }

        #endregion View Go To Page Previous Page

        #region View Go To Page Number

        /// <summary>
        /// Test that the View - Goto Page - Page Number menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageNumberDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Goto Page - Page Number menu item
            ToolStripMenuItem viewGoToNumber =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Page n&umber...");

            // Check that the menu item is disabled
            Assert.That(!viewGoToNumber.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - Page Number menu item is disabled 
        /// with a single page image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageNumberDisabledWithSinglePageImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the View - Go To Page - Page Number menu item
            ToolStripMenuItem viewGoToNumber =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Page n&umber...");

            // Check that the menu item is disabled
            Assert.That(!viewGoToNumber.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - Page Number menu item is enabled 
        /// with a multiple page image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageNumberEnabledWithMultiplePageImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Get the View - Go To Page - Page Number menu item
            ToolStripMenuItem viewGoToNumber =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Page n&umber...");

            // Check that the menu item is enabled
            Assert.That(viewGoToNumber.Enabled);
        }

        #endregion View Go To Page Number

        #region View Go To Page Next Page

        /// <summary>
        /// Test that the View - Goto Page - Next Page menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageNextPageDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Goto Page - Next Page menu item
            ToolStripMenuItem viewGoToNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Next page");

            // Check that the menu item is disabled
            Assert.That(!viewGoToNext.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - Next Page menu item is disabled 
        /// when on the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageNextPageDisabledOnLastPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Move to the last page
            _imageViewer.PageNumber = _imageViewer.PageCount;

            // Get the View - Go To Page - Next Page menu item
            ToolStripMenuItem viewGoToNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Next page");

            // Check that the menu item is disabled
            Assert.That(!viewGoToNext.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - Next Page menu item is enabled 
        /// when not on the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageNextPageEnabledNotOnLastPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Get the View - Go To Page - Next Page menu item
            ToolStripMenuItem viewGoToNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Next page");

            // Check that the menu item is enabled
            Assert.That(viewGoToNext.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - Next Page menu item navigates 
        /// to the next page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageNextPageNavigationTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image - starts on page 1
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Get the View - Go To Page - Next Page menu item
            ToolStripMenuItem viewGoToNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Next page");

            // Move to the next page
            viewGoToNext.PerformClick();

            // Check that page 2 is active
            Assert.That(_imageViewer.PageNumber == 2);
        }

        #endregion View Go To Page Next Page

        #region View Go To Page Last Page

        /// <summary>
        /// Test that the View - Goto Page - Last Page menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageLastPageDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Goto Page - Last Page menu item
            ToolStripMenuItem viewGoToLast =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Last page");

            // Check that the menu item is disabled
            Assert.That(!viewGoToLast.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - Last Page menu item is disabled 
        /// when on the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageLastPageDisabledOnLastPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Move to the last page
            _imageViewer.PageNumber = _imageViewer.PageCount;

            // Get the View - Go To Page - Last Page menu item
            ToolStripMenuItem viewGoToLast =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Last page");

            // Check that the menu item is disabled
            Assert.That(!viewGoToLast.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - Last Page menu item is enabled 
        /// when not on the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageLastPageEnabledNotOnLastPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Get the View - Go To Page - Last Page menu item
            ToolStripMenuItem viewGoToLast =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Last page");

            // Check that the menu item is enabled
            Assert.That(viewGoToLast.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - Last Page menu item navigates 
        /// to the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageLastPageNavigationTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Get the View - Go To Page - Last Page menu item
            ToolStripMenuItem viewGoToLast =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Last page");

            // Move to the last page
            viewGoToLast.PerformClick();

            // Check that the last page is active
            Assert.That(_imageViewer.PageNumber == _imageViewer.PageCount);
        }

        #endregion View Go To Page Last Page

        #region View Go To Item Next Invalid

        /// <summary>
        /// Test that the View - Goto Item - Next Invalid menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToItemNextInvalidDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Goto Item - Next Invalid menu item
            ToolStripMenuItem viewGoToItemNextInvalid =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Next invalid");

            // Check that the menu item is disabled
            Assert.That(!viewGoToItemNextInvalid.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Item - Next Invalid menu item is disabled 
        /// with an image open and no invalid items.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToItemNextInvalidDisabledWithNoInvalidTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file - all items are valid
            OpenTestImageAndVOA(_imageViewer, _BLOOD_CHEMISTRY, _BLOOD_CHEMISTRY_VOA);

            // Get the View - Goto Item - Next Invalid menu item
            ToolStripMenuItem viewGoToItemNextInvalid =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Next invalid");

            // Check that the menu item is disabled
            Assert.That(!viewGoToItemNextInvalid.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Item - Next Invalid menu item is enabled 
        /// with an image open and at least one invalid item.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToItemNextInvalidEnabledWithInvalidTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the View - Goto Item - Next Invalid menu item
            ToolStripMenuItem viewGoToItemNextInvalid =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Next invalid");

            // Check that the menu item is enabled
            Assert.That(viewGoToItemNextInvalid.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Item - Next Invalid menu item is disabled 
        /// when last invalid item is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToItemNextInvalidDisabledOnLastInvalidTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_METABOLIC, _BASIC_METABOLIC_VOA);

            // Get the View - Goto Item - Next Invalid menu item
            ToolStripMenuItem viewGoToItemNextInvalid =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Next invalid");

            // Move to next (and last) invalid item
            viewGoToItemNextInvalid.PerformClick();

            // Provide manual text for a valid date
            SendKeys.SendWait("01/01/2001");

            // Check that the menu item is disabled
            Assert.That(!viewGoToItemNextInvalid.Enabled);
        }
        
        #endregion View Go To Item Next Invalid

        #region View Go To Item Next Unviewed

        /// <summary>
        /// Test that the View - Goto Item - Next Unviewed menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToItemNextUnviewedDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Goto Item - Next Unviewed menu item
            ToolStripMenuItem viewGoToItemNextUnviewed =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Next unviewed");

            // Check that the menu item is disabled
            Assert.That(!viewGoToItemNextUnviewed.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Item - Next Unviewed menu item is disabled 
        /// with an image open and no unviewed items.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToItemNextUnviewedDisabledWithNoUnviewedTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BLOOD_CHEMISTRY, _BLOOD_CHEMISTRY_VOA);

            // Get the View - Goto Item - Next Unviewed menu item
            ToolStripMenuItem viewGoToItemNextUnviewed =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Next unviewed");

            // Move to next unviewed item - approximately 24 times so all will be viewed
            int i = 0;
            while (viewGoToItemNextUnviewed.Enabled && i < 50)
            {
                viewGoToItemNextUnviewed.PerformClick();
                Application.DoEvents();
                _dataEntryApplicationForm.Refresh();
                i++;
            }
            
            // Check that the menu item is disabled
            Assert.That(!viewGoToItemNextUnviewed.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Item - Next Unviewed menu item is enabled 
        /// with an image open and at least one unviewed item.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToItemNextUnviewedEnabledWithUnviewedTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImageAndVOA(_imageViewer, _BLOOD_CHEMISTRY, _BLOOD_CHEMISTRY_VOA);

            // Get the View - Goto Item - Next Unviewed menu item
            ToolStripMenuItem viewGoToItemNextUnviewed =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "Next unviewed");

            // Check that the menu item is enabled
            Assert.That(viewGoToItemNextUnviewed.Enabled);
        }

        #endregion View Go To Item Next Unviewed

        #region View Temporarily Hide Tooltips

        /// <summary>
        /// Test that the View - Temporarily Hide Tooltips menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTemporarilyHideTooltipsDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Temporarily Hide Tooltips menu item
            ToolStripMenuItem viewTemporarilyHideTooltips =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, 
                "Temporarily hide tooltips");

            // Check that the menu item is disabled
            Assert.That(!viewTemporarilyHideTooltips.Enabled);
        }

        /// <summary>
        /// Test that the View - Temporarily Hide Tooltips menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTemporarilyHideTooltipsEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BLOOD_CHEMISTRY, _BLOOD_CHEMISTRY_VOA);

            // Get the View - Temporarily Hide Tooltips menu item
            ToolStripMenuItem viewTemporarilyHideTooltips =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "Temporarily hide tooltips");

            // Check that the menu item is enabled
            Assert.That(viewTemporarilyHideTooltips.Enabled);
        }

        #endregion View Temporarily Hide Tooltips

        #region View Highlight All Data In Image

        /// <summary>
        /// Test that the View - Highlight All Data In Image menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewHighlightAllDataInImageDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Highlight All Data In Image menu item
            ToolStripMenuItem viewHighlightAllDataInImage =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "Highlight all data in image");

            // Check that the menu item is disabled
            Assert.That(!viewHighlightAllDataInImage.Enabled);
        }

        /// <summary>
        /// Test that the View - Highlight All Data In Image menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewHighlightAllDataInImageEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BLOOD_CHEMISTRY, _BLOOD_CHEMISTRY_VOA);

            // Get the View - Highlight All Data In Image menu item
            ToolStripMenuItem viewHighlightAllDataInImage =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "Highlight all data in image");

            // Check that the menu item is enabled
            Assert.That(viewHighlightAllDataInImage.Enabled);
        }

        #endregion View Highlight All Data In Image

        #region View Accept Highlight

        /// <summary>
        /// Test that the View - Accept Highlight menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewAcceptHighlightDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Accept Highlight menu item
            ToolStripMenuItem viewAcceptHighlight =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "Accept highlight");

            // Check that the menu item is disabled
            Assert.That(!viewAcceptHighlight.Enabled);
        }

        /// <summary>
        /// Test that the View - Accept Highlight menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewAcceptHighlightEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BLOOD_CHEMISTRY, _BLOOD_CHEMISTRY_VOA);

            // Send F4 to advance to the next unviewed item
            SendKeys.SendWait("{F4}");

            // Get the View - Accept Highlight menu item
            ToolStripMenuItem viewAcceptHighlight =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "Accept highlight");

            // Check that the menu item is enabled
            Assert.That(viewAcceptHighlight.Enabled);
        }

        #endregion View Accept Highlight

        #region View Remove Highlight

        /// <summary>
        /// Test that the View - Remove Highlight menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewRemoveHighlightDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the View - Remove Highlight menu item
            ToolStripMenuItem viewRemoveHighlight =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "Remove highlight");

            // Check that the menu item is disabled
            Assert.That(!viewRemoveHighlight.Enabled);
        }

        /// <summary>
        /// Test that the View - Remove Highlight menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewRemoveHighlightEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BLOOD_CHEMISTRY, _BLOOD_CHEMISTRY_VOA);

            // Send F4 to advance to the next unviewed item
            SendKeys.SendWait("{F4}");

            // Get the View - Remove Highlight menu item
            ToolStripMenuItem viewRemoveHighlight =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "Remove highlight");

            // Check that the menu item is enabled
            Assert.That(viewRemoveHighlight.Enabled);
        }

        #endregion View Remove Highlight

        #region Tools Zoom

        /// <summary>
        /// Test that the Tools - Zoom menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsZoomDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Zoom window");

            // Check that the menu item is disabled
            Assert.That(!toolsZoom.Enabled);
        }

        /// <summary>
        /// Test that the Tools - Zoom menu item is enabled 
        /// with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsZoomEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Zoom window");

            // Check that the menu item is enabled
            Assert.That(toolsZoom.Enabled);
        }

        /// <summary>
        /// Tests whether the Tools - Zoom menu item depresses the  
        /// toolbar button when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsZoomSetsToolBarButtonTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Zoom window");

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Get the Zoom button
            ZoomWindowToolStripButton zoom =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_dataEntryApplicationForm);

            // Check that the Zoom button is checked
            Assert.That(zoom.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Zoom menu item is checked when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsZoomSetsMenuItemTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Zoom window");

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Check that the menu item is checked
            Assert.That(toolsZoom.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Zoom menu item is unchecked when 
        /// Pan Tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsZoomUncheckedWithPanToolTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Zoom window");

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Get the Tools - Pan menu item
            ToolStripMenuItem toolsPan =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "P&an");

            // Select the Pan tool
            toolsPan.PerformClick();

            // Check that the Zoom menu item is unchecked
            Assert.That(!toolsZoom.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Zoom menu item is unchecked when 
        /// Pan toolbar button is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsZoomUncheckedWithPanToolBarTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Zoom window");

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Get the Pan toolbar button
            PanToolStripButton toolsPan =
                FormMethods.GetFormComponent<PanToolStripButton>(_dataEntryApplicationForm);

            // Select the Pan tool
            toolsPan.PerformClick();

            // Check that the Zoom menu item is unchecked
            Assert.That(!toolsZoom.Checked);
        }

        #endregion Tools Zoom

        #region Tools Pan

        /// <summary>
        /// Test that the Tools - Pan menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsPanDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Tools - Pan menu item
            ToolStripMenuItem toolsPan =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "P&an");

            // Check that the menu item is disabled
            Assert.That(!toolsPan.Enabled);
        }

        /// <summary>
        /// Test that the Tools - Pan menu item is enabled 
        /// with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsPanEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Pan menu item
            ToolStripMenuItem toolsPan =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "P&an");

            // Check that the menu item is enabled
            Assert.That(toolsPan.Enabled);
        }

        /// <summary>
        /// Tests whether the Tools - Pan menu item depresses the  
        /// toolbar button when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsPanSetsToolBarButtonTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Pan menu item
            ToolStripMenuItem toolsPan =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "P&an");

            // Select the Pan tool
            toolsPan.PerformClick();

            // Get the Pan button
            PanToolStripButton pan =
                FormMethods.GetFormComponent<PanToolStripButton>(_dataEntryApplicationForm);

            // Check that the Pan button is checked
            Assert.That(pan.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Pan menu item is checked when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsPanSetsMenuItemTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Pan menu item
            ToolStripMenuItem toolsPan =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "P&an");

            // Select the Pan tool
            toolsPan.PerformClick();

            // Check that the menu item is checked
            Assert.That(toolsPan.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Pan menu item is unchecked when 
        /// Zoom Tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsPanUncheckedWithZoomToolTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Pan menu item
            ToolStripMenuItem toolsPan =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "P&an");

            // Select the Pan tool
            toolsPan.PerformClick();

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Zoom window");

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Check that the Pan menu item is unchecked
            Assert.That(!toolsPan.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Pan menu item is unchecked when 
        /// Zoom toolbar button is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsPanUncheckedWithZoomToolBarTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Pan menu item
            ToolStripMenuItem toolsPan =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "P&an");

            // Select the Pan tool
            toolsPan.PerformClick();

            // Get the Zoom window toolbar button
            ZoomWindowToolStripButton toolsZoom =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_dataEntryApplicationForm);

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Check that the Pan menu item is unchecked
            Assert.That(!toolsPan.Checked);
        }

        #endregion Tools Pan

        #region Tools Review

        /// <summary>
        /// Test that the Tools - Review And Select menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsReviewAndSelectDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Tools - Review And Select menu item
            ToolStripMenuItem toolsReview =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, 
                "&Review and select");

            // Check that the menu item is disabled
            Assert.That(!toolsReview.Enabled);
        }

        /// <summary>
        /// Test that the Tools - Review And Select menu item is enabled 
        /// with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsReviewAndSelectEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Review And Select menu item
            ToolStripMenuItem toolsReview =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "&Review and select");

            // Check that the menu item is enabled
            Assert.That(toolsReview.Enabled);
        }

        /// <summary>
        /// Tests whether the Tools - Review And Select menu item depresses the  
        /// toolbar button when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsReviewAndSelectSetsToolBarButtonTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Review And Select menu item
            ToolStripMenuItem toolsReview =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "&Review and select");

            // Select the Review tool
            toolsReview.PerformClick();

            // Get the Review button
            ToolStripButton review =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm, 
                "Select redactions and other objects");

            // Check that the Review button is checked
            Assert.That(review.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Review And Select menu item is checked when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsReviewAndSelectSetsMenuItemTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Review And Select menu item
            ToolStripMenuItem toolsReview =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, 
                "&Review and select");

            // Select the Review tool
            toolsReview.PerformClick();

            // Check that the menu item is checked
            Assert.That(toolsReview.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Review And Select menu item is unchecked when 
        /// Zoom Tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsReviewAndSelectUncheckedWithZoomToolTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Review And Select menu item
            ToolStripMenuItem toolsReview =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "&Review and select");

            // Select the Review tool
            toolsReview.PerformClick();

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Zoom window");

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Check that the Review menu item is unchecked
            Assert.That(!toolsReview.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Review And Select menu item is unchecked when 
        /// Zoom toolbar button is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsReviewAndSelectUncheckedWithZoomToolBarTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Tools - Review menu item
            ToolStripMenuItem toolsReview =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "&Review and select");

            // Select the Review tool
            toolsReview.PerformClick();

            // Get the Zoom window toolbar button
            ZoomWindowToolStripButton toolsZoom =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_dataEntryApplicationForm);

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Check that the Review menu item is unchecked
            Assert.That(!toolsReview.Checked);
        }

        #endregion Tools Review

        #region Tools Angular Zone

        /// <summary>
        /// Test that the Tools - Angular Zone menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsAngularZoneDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Tools - Angular Zone menu item
            ToolStripMenuItem toolsAngular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "Swipe text in a&ngular zone");

            // Check that the menu item is disabled
            Assert.That(!toolsAngular.Enabled);
        }

        /// <summary>
        /// Test that the Tools - Angular Zone menu item is enabled 
        /// with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsAngularZoneEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_HEMATOLOGY, _BASIC_HEMATOLOGY_VOA);

            // Send F4 to advance to the next unviewed item
            SendKeys.SendWait("{F4}");

            // Get the Tools - Angular Zone menu item
            ToolStripMenuItem toolsAngular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "Swipe text in a&ngular zone");

            // Check that the menu item is enabled
            Assert.That(toolsAngular.Enabled);
        }

        /// <summary>
        /// Tests whether the Tools - Angular Zone menu item depresses the  
        /// toolbar button when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsAngularZoneSetsToolBarButtonTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_HEMATOLOGY, _BASIC_HEMATOLOGY_VOA);

            // Send F4 to advance to the next unviewed item
            SendKeys.SendWait("{F4}");

            // Get the Tools - Angular Zone menu item
            ToolStripMenuItem toolsAngular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "Swipe text in a&ngular zone");

            // Select the Angular Zone tool
            toolsAngular.PerformClick();

            // Get the Angular Zone button
            AngularHighlightToolStripButton angular =
                FormMethods.GetFormComponent<AngularHighlightToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the Angular Zone button is checked
            Assert.That(angular.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Angular Zone menu item is checked when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsAngularZoneSetsMenuItemTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_HEMATOLOGY, _BASIC_HEMATOLOGY_VOA);

            // Send F4 to advance to the next unviewed item
            SendKeys.SendWait("{F4}");

            // Get the Tools - Angular Zone menu item
            ToolStripMenuItem toolsAngular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "Swipe text in a&ngular zone");

            // Select the Angular Zone tool
            toolsAngular.PerformClick();

            // Check that the menu item is checked
            Assert.That(toolsAngular.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Angular Zone menu item is unchecked when 
        /// Zoom Tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsAngularZoneUncheckedWithZoomToolTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_HEMATOLOGY, _BASIC_HEMATOLOGY_VOA);

            // Send F4 to advance to the next unviewed item
            SendKeys.SendWait("{F4}");

            // Get the Tools - Angular Zone menu item
            ToolStripMenuItem toolsAngular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "Swipe text in a&ngular zone");

            // Select the Angular Zone tool
            toolsAngular.PerformClick();

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Zoom window");

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Check that the Angular Zone menu item is unchecked
            Assert.That(!toolsAngular.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Angular Zone menu item is unchecked when 
        /// Zoom toolbar button is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsAngularZoneUncheckedWithZoomToolBarTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_HEMATOLOGY, _BASIC_HEMATOLOGY_VOA);

            // Send F4 to advance to the next unviewed item
            SendKeys.SendWait("{F4}");

            // Get the Tools - Angular Zone menu item
            ToolStripMenuItem toolsAngular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "Swipe text in a&ngular zone");

            // Select the Angular Zone tool
            toolsAngular.PerformClick();

            // Get the Zoom window toolbar button
            ZoomWindowToolStripButton toolsZoom =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_dataEntryApplicationForm);

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Check that the Angular Zone menu item is unchecked
            Assert.That(!toolsAngular.Checked);
        }

        #endregion Tools Angular Zone

        #region Tools Rectangular Zone

        /// <summary>
        /// Test that the Tools - Rectangular Zone menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsRectangularZoneDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Tools - Rectangular Zone menu item
            ToolStripMenuItem toolsRectangular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "&Swipe text in rectangular zone");

            // Check that the menu item is disabled
            Assert.That(!toolsRectangular.Enabled);
        }

        /// <summary>
        /// Test that the Tools - Rectangular Zone menu item is enabled 
        /// with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsRectangularZoneEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_HEMATOLOGY, _BASIC_HEMATOLOGY_VOA);

            // Send F4 to advance to the next unviewed item
            SendKeys.SendWait("{F4}");

            // Get the Tools - Rectangular Zone menu item
            ToolStripMenuItem toolsRectangular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "&Swipe text in rectangular zone");

            // Check that the menu item is enabled
            Assert.That(toolsRectangular.Enabled);
        }

        /// <summary>
        /// Tests whether the Tools - Rectangular Zone menu item depresses the  
        /// toolbar button when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsRectangularZoneSetsToolBarButtonTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_HEMATOLOGY, _BASIC_HEMATOLOGY_VOA);

            // Send F4 to advance to the next unviewed item
            SendKeys.SendWait("{F4}");

            // Get the Tools - Rectangular Zone menu item
            ToolStripMenuItem toolsRectangular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "&Swipe text in rectangular zone");

            // Select the Rectangular Zone tool
            toolsRectangular.PerformClick();

            // Get the Rectangular Zone button
            RectangularHighlightToolStripButton rectangular =
                FormMethods.GetFormComponent<RectangularHighlightToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the Rectangular Zone button is checked
            Assert.That(rectangular.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Rectangular Zone menu item is checked when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsRectangularZoneSetsMenuItemTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_HEMATOLOGY, _BASIC_HEMATOLOGY_VOA);

            // Send F4 to advance to the next unviewed item
            SendKeys.SendWait("{F4}");

            // Get the Tools - Rectangular Zone menu item
            ToolStripMenuItem toolsRectangular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "&Swipe text in rectangular zone");

            // Select the Rectangular Zone tool
            toolsRectangular.PerformClick();

            // Check that the menu item is checked
            Assert.That(toolsRectangular.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Rectangular Zone menu item is unchecked when 
        /// Zoom Tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsRectangularZoneUncheckedWithZoomToolTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_HEMATOLOGY, _BASIC_HEMATOLOGY_VOA);

            // Send F4 to advance to the next unviewed item
            SendKeys.SendWait("{F4}");

            // Get the Tools - Rectangular Zone menu item
            ToolStripMenuItem toolsRectangular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "&Swipe text in rectangular zone");

            // Select the Rectangular Zone tool
            toolsRectangular.PerformClick();

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm, "&Zoom window");

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Check that the Rectangular Zone menu item is unchecked
            Assert.That(!toolsRectangular.Checked);
        }

        /// <summary>
        /// Tests whether the Tools - Rectangular Zone menu item is unchecked when 
        /// Zoom toolbar button is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsRectangularZoneUncheckedWithZoomToolBarTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_HEMATOLOGY, _BASIC_HEMATOLOGY_VOA);

            // Send F4 to advance to the next unviewed item
            SendKeys.SendWait("{F4}");

            // Get the Tools - Rectangular Zone menu item
            ToolStripMenuItem toolsRectangular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_dataEntryApplicationForm,
                "&Swipe text in rectangular zone");

            // Select the Rectangular Zone tool
            toolsRectangular.PerformClick();

            // Get the Zoom window toolbar button
            ZoomWindowToolStripButton toolsZoom =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_dataEntryApplicationForm);

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Check that the Rectangular Zone menu item is unchecked
            Assert.That(!toolsRectangular.Checked);
        }

        #endregion Tools Rectangular Zone

    }
}
