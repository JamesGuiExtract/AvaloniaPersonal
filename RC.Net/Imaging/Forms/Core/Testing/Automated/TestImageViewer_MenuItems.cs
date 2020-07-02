using Extract;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Extract.Imaging.Forms.Test
{
    public partial class TestImageViewer
    {

        #region FitToPage

        /// <summary>
        /// Test the <see cref="FitToPageToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripMenuItemEnabledNoImageTest()
        {
            // Show image viewer so the menu items enabled state can be checked
            _imageViewerForm.Show();

            // Get the fit to page menu item
            FitToPageToolStripMenuItem fitToPage = FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_imageViewerForm);

            Assert.That(fitToPage.Enabled);
        }

        /// <summary>
        /// Test the <see cref="FitToPageToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripMenuItemNoImageTest()
        {
            // Show image viewer so the menu items enabled state can be checked
            _imageViewerForm.Show();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Set the fit mode to none
            imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI21700", "Could not change fit mode to none!",
                imageViewer.FitMode == FitMode.None);

            // Get the fit to page menu item
            FitToPageToolStripMenuItem fitToPage = FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_imageViewerForm);

            // Click the menu item
            fitToPage.PerformClick();

            Assert.That(imageViewer.FitMode == FitMode.FitToPage);
        }

        /// <summary>
        /// Test the <see cref="FitToPageToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripMenuItemWithImageTest()
        {
            // Show image viewer so the menu items enabled state can be checked
            _imageViewerForm.Show();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Set the fit mode to none
            imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI21701", "Could not change fit mode to none!",
                imageViewer.FitMode == FitMode.None);

            // Get the fit to page menu item
            FitToPageToolStripMenuItem fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_imageViewerForm);

            // Click the menu item
            fitToPage.PerformClick();

            Assert.That(imageViewer.FitMode == FitMode.FitToPage);
        }

        /// <summary>
        /// Tests whether <see cref="FitToPageToolStripMenuItem"/> is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripMenuItemToggledOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the FitToWidth menu item
            FitToWidthToolStripMenuItem fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_imageViewerForm);

            // Select the FitToWidth tool
            fitToWidth.PerformClick();

            // Get the FitToPage menu item
            FitToPageToolStripMenuItem fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_imageViewerForm);

            // Select the FitToPage tool
            fitToPage.PerformClick();

            // Check that the menu item is checked
            Assert.That(fitToPage.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="FitToPageToolStripMenuItem"/> is toggled off 
        /// when selected again.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripMenuItemTogglesOffWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the FitToWidth menu item
            FitToWidthToolStripMenuItem fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_imageViewerForm);

            // Select the FitToWidth tool
            fitToWidth.PerformClick();

            // Get the FitToPage menu item
            FitToPageToolStripMenuItem fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_imageViewerForm);

            // Select the FitToPage tool twice
            fitToPage.PerformClick();
            fitToPage.PerformClick();

            // Check that the menu item is unchecked
            Assert.That(!fitToPage.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="FitToPageToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.FitModeChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open an image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of FitModeChanged and ZoomChanged events
            imageViewer.FitModeChanged += eventCounters.CountEvent<FitModeChangedEventArgs>;
            imageViewer.ZoomChanged += eventCounters.CountEvent2<ZoomChangedEventArgs>;

            // Click the FitToPageToolStripMenuItem
            FitToPageToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one FitModeChanged and one ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter == 1 && eventCounters.EventCounter2 == 1);

            // Click the FitToPageToolStripMenuItem again
            clickMe.PerformClick();

            // Check that exactly two FitModeChanged and two ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter == 2 && eventCounters.EventCounter2 == 2);
        }

        #endregion FitToPage

        #region FitToWidth

        /// <summary>
        /// Test the <see cref="FitToWidthToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripMenuItemEnabledNoImageTest()
        {
            // Show image viewer so the menu items enabled state can be checked
            _imageViewerForm.Show();

            // Get the fit to width menu item
            FitToWidthToolStripMenuItem fitToWidth = 
                FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_imageViewerForm);

            Assert.That(fitToWidth.Enabled);
        }

        /// <summary>
        /// Test the <see cref="FitToWidthToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripMenuItemNoImageTest()
        {
            // Show image viewer so the menu items enabled state can be checked
            _imageViewerForm.Show();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Set the fit mode to none
            imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI21702", "Could not change fit mode to none!",
                imageViewer.FitMode == FitMode.None);

            // Get the fit to width menu item
            FitToWidthToolStripMenuItem fitToWidth = FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_imageViewerForm);

            // Click the menu item
            fitToWidth.PerformClick();

            Assert.That(imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Test the <see cref="FitToWidthToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripMenuItemWithImageTest()
        {
            // Show image viewer so the menu items enabled state can be checked
            _imageViewerForm.Show();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Set the fit mode to none
            imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI21703", "Could not change fit mode to none!",
                imageViewer.FitMode == FitMode.None);

            // Get the fit to width menu item
            FitToWidthToolStripMenuItem fitToWidth = FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_imageViewerForm);

            // Click the menu item
            fitToWidth.PerformClick();

            Assert.That(imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Tests whether <see cref="FitToWidthToolStripMenuItem"/> is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripMenuItemToggledOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the FitToPage menu item
            FitToPageToolStripMenuItem fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_imageViewerForm);

            // Select the FitToPage tool
            fitToPage.PerformClick();

            // Get the FitToWidth menu item
            FitToWidthToolStripMenuItem fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_imageViewerForm);

            // Select the FitToWidth tool
            fitToWidth.PerformClick();

            // Check that the menu item is checked
            Assert.That(fitToWidth.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="FitToWidthToolStripMenuItem"/> is toggled off 
        /// when selected again.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripMenuItemTogglesOffWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the FitToPage menu item
            FitToPageToolStripMenuItem fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_imageViewerForm);

            // Select the FitToPage tool
            fitToPage.PerformClick();

            // Get the FitToWidth menu item
            FitToWidthToolStripMenuItem fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_imageViewerForm);

            // Select the FitToWidth tool twice
            fitToWidth.PerformClick();
            fitToWidth.PerformClick();

            // Check that the menu item is unchecked
            Assert.That(!fitToWidth.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="FitToWidthToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.FitModeChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open an image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of FitModeChanged and ZoomChanged events
            imageViewer.FitModeChanged += eventCounters.CountEvent<FitModeChangedEventArgs>;
            imageViewer.ZoomChanged += eventCounters.CountEvent2<ZoomChangedEventArgs>;

            // Click the FitToWidthToolStripMenuItem
            FitToWidthToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one FitModeChanged and one or more ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter == 1 && eventCounters.EventCounter2 >= 1);
            var previousZoomEventCount = eventCounters.EventCounter2;

            // Click the FitToWidthToolStripMenuItem again
            clickMe.PerformClick();

            // Check that exactly two FitModeChanged and more ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter == 2 && eventCounters.EventCounter2 > previousZoomEventCount);
        }

        #endregion FitToWidth

        #region Open Image

        /// <summary>
        /// Test that the <see cref="OpenImageToolStripMenuItem"/> is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_OpenImageToolStripMenuItemEnabledWithNoImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Get the open image menu item
            OpenImageToolStripMenuItem openImage =
                FormMethods.GetFormComponent<OpenImageToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(openImage.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="OpenImageToolStripMenuItem"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_OpenImageToolStripMenuItemEnabledWithImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the open image menu item
            OpenImageToolStripMenuItem openImage =
                FormMethods.GetFormComponent<OpenImageToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(openImage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="OpenImageToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_OpenImageToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Prompt the user to select a valid image file
            MessageBox.Show("Please select a valid image file.", "", MessageBoxButtons.OK, 
                MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ImageFileChanged events
            imageViewer.ImageFileChanged += eventCounters.CountEvent<ImageFileChangedEventArgs>;

            // Click the OpenImageToolStripMenuItem
            OpenImageToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<OpenImageToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one ImageFileChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion Open Image

        #region Zoom In

        /// <summary>
        /// Tests whether <see cref="ZoomInToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripMenuItemDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the Zoom In menu item
            ZoomInToolStripMenuItem zoomIn =
                FormMethods.GetFormComponent<ZoomInToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!zoomIn.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomInToolStripMenuItem"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripMenuItemEnabledWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Zoom In menu item
            ZoomInToolStripMenuItem zoomIn =
                FormMethods.GetFormComponent<ZoomInToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(zoomIn.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomInToolStripMenuItem"/> zooms in  
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripMenuItemZoomTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the zoom level
            double zoomLevel = imageViewer.ZoomInfo.ScaleFactor;

            // Get the Zoom In menu item
            ZoomInToolStripMenuItem zoomIn =
                FormMethods.GetFormComponent<ZoomInToolStripMenuItem>(_imageViewerForm);

            // Zoom in
            zoomIn.PerformClick();

            // Check that the image zoomed in
            Assert.That(imageViewer.ZoomInfo.ScaleFactor > zoomLevel);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomInToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ZoomChanged events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;

            // Click the ZoomInToolStripMenuItem
            ZoomInToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<ZoomInToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that one or more ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter >= 1);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomInToolStripMenuItem"/> adds a zoom history entry.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripMenuItemAddsZoomHistoryTest()
        {
            // Show the image viewer
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the current Zoom history count
            int zoomHistoryCount = imageViewer.ZoomHistoryCount;

            // Click the ZoomInToolStripMenuItem
            ZoomInToolStripMenuItem clickMe = FormMethods.GetFormComponent<ZoomInToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one zoom history record has been added
            Assert.That((zoomHistoryCount + 1) == imageViewer.ZoomHistoryCount);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomInToolStripMenuItem"/> deselects 
        /// active FitToPage.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripMenuItemDeselectsFitToPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Set FitToPage
            imageViewer.FitMode = FitMode.FitToPage;

            // Get the Zoom In menu item
            ZoomInToolStripMenuItem zoomIn =
                FormMethods.GetFormComponent<ZoomInToolStripMenuItem>(_imageViewerForm);

            // Zoom in
            zoomIn.PerformClick();

            // Check that Fit Mode is no longer FitToPage
            Assert.That(imageViewer.FitMode != FitMode.FitToPage);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomInToolStripMenuItem"/> deselects 
        /// active FitToWidth.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripMenuItemDeselectsFitToWidthTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Set FitToWidth
            imageViewer.FitMode = FitMode.FitToWidth;

            // Get the Zoom In menu item
            ZoomInToolStripMenuItem zoomIn =
                FormMethods.GetFormComponent<ZoomInToolStripMenuItem>(_imageViewerForm);

            // Zoom in
            zoomIn.PerformClick();

            // Check that Fit Mode is no longer FitToWidth
            Assert.That(imageViewer.FitMode != FitMode.FitToWidth);
        }

        #endregion Zoom In

        #region Zoom Out

        /// <summary>
        /// Tests whether <see cref="ZoomOutToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripMenuItemDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the Zoom Out menu item
            ZoomOutToolStripMenuItem zoomOut =
                FormMethods.GetFormComponent<ZoomOutToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!zoomOut.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomOutToolStripMenuItem"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripMenuItemEnabledWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Zoom Out menu item
            ZoomOutToolStripMenuItem zoomOut =
                FormMethods.GetFormComponent<ZoomOutToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(zoomOut.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomOutToolStripMenuItem"/> zooms out 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripMenuItemZoomTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Zoom Out menu item
            ZoomOutToolStripMenuItem zoomOut =
                FormMethods.GetFormComponent<ZoomOutToolStripMenuItem>(_imageViewerForm);

            // Due to changes in the zooming code, you can no longer zoom out
            // Past the size of the page. I believe this is the JIRA that orignally broke this: https://extract.atlassian.net/browse/ISSUE-14420
            // Added a zoom in, so it can zoom out.
            ZoomInToolStripMenuItem zoomIn =
                FormMethods.GetFormComponent<ZoomInToolStripMenuItem>(_imageViewerForm);
            zoomIn.PerformClick();

            // Get the zoom level
            double zoomLevel = imageViewer.ZoomInfo.ScaleFactor;

            // Zoom out
            zoomOut.PerformClick();

            // Check that the zoom level has changed
            Assert.That(imageViewer.ZoomInfo.ScaleFactor < zoomLevel);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomOutToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ZoomChanged events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;

            // Click the ZoomOutToolStripMenuItem
            ZoomOutToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<ZoomOutToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one ZoomChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomOutToolStripMenuItem"/> adds a zoom history entry.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripMenuItemAddsZoomHistoryTest()
        {
            // Show the image viewer
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Due to changes in the zooming code, you can no longer zoom out
            // Past the size of the page. I believe this is the JIRA that orignally broke this: https://extract.atlassian.net/browse/ISSUE-14420
            // Added a zoom in, so it can zoom out.
            ZoomInToolStripMenuItem zoomIn =
                FormMethods.GetFormComponent<ZoomInToolStripMenuItem>(_imageViewerForm);
            zoomIn.PerformClick();

            // Get the current Zoom history count
            int zoomHistoryCount = imageViewer.ZoomHistoryCount;

            // Click the ZoomOutToolStripMenuItem
            ZoomOutToolStripMenuItem clickMe = FormMethods.GetFormComponent<ZoomOutToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one zoom history record has been added
            Assert.That((zoomHistoryCount + 1) == imageViewer.ZoomHistoryCount);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomOutToolStripMenuItem"/> deselects 
        /// active FitToPage.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripMenuItemDeselectsFitToPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Set FitToPage
            imageViewer.FitMode = FitMode.FitToPage;

            // Get the Zoom Out menu item
            ZoomOutToolStripMenuItem zoomOut =
                FormMethods.GetFormComponent<ZoomOutToolStripMenuItem>(_imageViewerForm);

            // Zoom out
            zoomOut.PerformClick();

            // Check that Fit Mode is no longer FitToPage
            Assert.That(imageViewer.FitMode != FitMode.FitToPage);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomOutToolStripMenuItem"/> deselects 
        /// active FitToWidth.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripMenuItemDeselectsFitToWidthTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Set FitToWidth
            imageViewer.FitMode = FitMode.FitToWidth;

            // Get the Zoom Out menu item
            ZoomOutToolStripMenuItem zoomOut =
                FormMethods.GetFormComponent<ZoomOutToolStripMenuItem>(_imageViewerForm);

            // Zoom out
            zoomOut.PerformClick();

            // Check that Fit Mode is no longer FitToWidth
            Assert.That(imageViewer.FitMode != FitMode.FitToWidth);
        }

        #endregion Zoom Out

        #region Zoom Previous

        /// <summary>
        /// Tests whether <see cref="ZoomPreviousToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousToolStripMenuItemDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the Zoom Previous menu item
            ZoomPreviousToolStripMenuItem zoomPrevious =
                FormMethods.GetFormComponent<ZoomPreviousToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!zoomPrevious.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomPreviousToolStripMenuItem"/> is disabled 
        /// without a previous zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousToolStripMenuItemDisabledWithoutHistoryTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image - no zoom history
            OpenTestImage(imageViewer);

            // Get the Zoom Previous menu item
            ZoomPreviousToolStripMenuItem zoomPrevious =
                FormMethods.GetFormComponent<ZoomPreviousToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!zoomPrevious.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomPreviousToolStripMenuItem"/> is enabled 
        /// with a previous zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousToolStripMenuItemEnabledWithHistoryTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Create a previous zoom history entry
            imageViewer.ZoomIn();

            // Get the Zoom Previous menu item
            ZoomPreviousToolStripMenuItem zoomPrevious =
                FormMethods.GetFormComponent<ZoomPreviousToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(zoomPrevious.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomPreviousToolStripMenuItem"/> zooms to 
        /// a previous zoom item.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousToolStripMenuItemZoomsToPreviousTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the zoom level
            double zoomLevelOriginal = imageViewer.ZoomInfo.ScaleFactor;

            // Create a previous zoom history entry
            imageViewer.ZoomIn();

            // Get the new zoom level
            double zoomLevelNew = imageViewer.ZoomInfo.ScaleFactor;

            // Get the Zoom Previous menu item
            ZoomPreviousToolStripMenuItem zoomPrevious =
                FormMethods.GetFormComponent<ZoomPreviousToolStripMenuItem>(_imageViewerForm);

            // Zoom to previous history item
            zoomPrevious.PerformClick();

            // Check that the zoom level changed back to original AND
            // that the original zoom level is different than new zoom level
            Assert.That(imageViewer.ZoomInfo.ScaleFactor == zoomLevelOriginal && 
                zoomLevelOriginal != zoomLevelNew);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomPreviousToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Create a previous zoom history entry
            imageViewer.ZoomIn();

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ZoomChanged events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;

            // Click the ZoomPreviousToolStripMenuItem
            ZoomPreviousToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<ZoomPreviousToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that one or more zoom events were raised.
            Assert.That(eventCounters.EventCounter >= 1);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomPreviousToolStripMenuItem"/> is limited to 
        /// twenty history entries.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousToolStripMenuItemSizeLimitedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Create more than twenty zoom history entries
            for (int i = 0; i < 12; i++)
            {
                imageViewer.ZoomIn();
                imageViewer.ZoomOut();
            }

            // Get the Zoom Previous menu item
            ZoomPreviousToolStripMenuItem zoomPrevious =
                FormMethods.GetFormComponent<ZoomPreviousToolStripMenuItem>(_imageViewerForm);

            // Go to the 20 previous zoom history entries
            for (int j = 0; j < 20; j++)
            {
                zoomPrevious.PerformClick();
            }

            // Check that the zoom history is back to the beginning
            Assert.That(!imageViewer.CanZoomPrevious);
        }

        #endregion Zoom Previous

        #region Zoom Next

        /// <summary>
        /// Tests whether <see cref="ZoomNextToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomNextToolStripMenuItemDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the Zoom Next menu item
            ZoomNextToolStripMenuItem zoomNext =
                FormMethods.GetFormComponent<ZoomNextToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!zoomNext.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomNextToolStripMenuItem"/> is disabled 
        /// without a next zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomNextToolStripMenuItemDisabledWithoutHistoryTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image - no zoom history
            OpenTestImage(imageViewer);

            // Get the Zoom Next menu item
            ZoomNextToolStripMenuItem zoomNext =
                FormMethods.GetFormComponent<ZoomNextToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!zoomNext.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomNextToolStripMenuItem"/> is enabled 
        /// with a next zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomNextToolStripMenuItemEnabledWithZoomHistoryTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Create a subsequent zoom history entry
            imageViewer.ZoomIn();
            imageViewer.ZoomPrevious();

            // Get the Zoom Next menu item
            ZoomNextToolStripMenuItem zoomNext =
                FormMethods.GetFormComponent<ZoomNextToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(zoomNext.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomNextToolStripMenuItem"/> zooms to 
        /// the next history entry.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomNextToolStripMenuItemZoomsNextTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Zoom in and store the zoom level
            imageViewer.ZoomIn();
            double zoomLevelIn = imageViewer.ZoomInfo.ScaleFactor;

            // Zoom previous and get the previous zoom level
            imageViewer.ZoomPrevious();
            double zoomLevelPrevious = imageViewer.ZoomInfo.ScaleFactor;

            // Get the Zoom Next menu item
            ZoomNextToolStripMenuItem zoomNext =
                FormMethods.GetFormComponent<ZoomNextToolStripMenuItem>(_imageViewerForm);

            // Go to the next zoom history entry
            zoomNext.PerformClick();

            // Check that the zoom level changed back to the zoomed in value AND
            // that the previous zoom level is different than new zoom level
            Assert.That(imageViewer.ZoomInfo.ScaleFactor == zoomLevelIn &&
                zoomLevelIn != zoomLevelPrevious);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomNextToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomNextToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Create a subsequent zoom history entry
            imageViewer.ZoomIn();
            imageViewer.ZoomPrevious();
            
            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ZoomChanged events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;

            // Click the ZoomNextToolStripMenuItem
            ZoomNextToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<ZoomNextToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Zoom events are fired based on any changes now.
            // It is safest to assume 1 or more zoom events will fire.
            Assert.That(eventCounters.EventCounter >= 1);
        }

        #endregion Zoom Next

        #region Previous Tile

        /// <summary>
        /// Tests the enabled state of the <see cref="PreviousTileToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousTileToolStripMenuItemEnableDisableTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Check that the previous tile toolstrip menu item is disabled without an image open
            PreviousTileToolStripMenuItem PreviousTile =
                FormMethods.GetFormComponent<PreviousTileToolStripMenuItem>(_imageViewerForm);
            Assert.That(!PreviousTile.Enabled);

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Check that the previous tile menu item is disabled now that the image is open
            Assert.That(!PreviousTile.Enabled);

            // Go to the next tile
            imageViewer.SelectNextTile();

            // Check that the Previous tile menu item is enabled
            Assert.That(PreviousTile.Enabled);
        }

        /// <summary>
        /// Tests whether the <see cref="PreviousTileToolStripMenuItem"/> updates the zoom history 
        /// properly.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousTileToolStripMenuItemZoomHistoryTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Go to the third page
            imageViewer.PageNumber = 3;

            // Go to the previous tile (on page 2)
            PreviousTileToolStripMenuItem previousTile =
                FormMethods.GetFormComponent<PreviousTileToolStripMenuItem>(_imageViewerForm);
            imageViewer.PerformClick(previousTile);

            // Ensure that there is only one zoom history entry for this page
            Assert.That(!imageViewer.CanZoomPrevious && !imageViewer.CanZoomNext);
        }

        /// <summary>
        /// Tests whether the <see cref="PreviousTileToolStripMenuItem"/> preserves the fit mode.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousTileToolStripMenuItemPreserveFitModeTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Go to the fourth page
            imageViewer.PageNumber = 4;

            // Set the fit mode to fit to page
            imageViewer.FitMode = FitMode.FitToPage;

            // Go to the previous tile
            PreviousTileToolStripMenuItem previousTile =
                FormMethods.GetFormComponent<PreviousTileToolStripMenuItem>(_imageViewerForm);
            imageViewer.PerformClick(previousTile);

            // This should be the entire previous page
            Assert.That(imageViewer.PageNumber == 3);

            // Ensure that the fit mode was preserved
            Assert.That(imageViewer.FitMode == FitMode.FitToPage);

            // Set the fit mode to fit to width (the top tile of page 3)
            imageViewer.FitMode = FitMode.FitToWidth;

            // Go to the previous tile
            imageViewer.PerformClick(previousTile);

            // This should be the bottom tile of the second page
            Assert.That(imageViewer.PageNumber == 2);

            // Ensure that the fit mode was preserved
            Assert.That(imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Tests whether the <see cref="PreviousTileToolStripMenuItem"/> raises events properly.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousTileToolStripMenuItemEventsTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Go to the fourth page
            imageViewer.PageNumber = 4;

            // Set the fit mode to fit to width
            imageViewer.FitMode = FitMode.FitToWidth;

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ZoomChanged and PageChanged events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;
            imageViewer.PageChanged += eventCounters.CountEvent2<PageChangedEventArgs>;

            // Go to the previous tile
            PreviousTileToolStripMenuItem previousTile =
                FormMethods.GetFormComponent<PreviousTileToolStripMenuItem>(_imageViewerForm);
            imageViewer.PerformClick(previousTile);

            // Ensure that one or more ZoomChanged events fire and one PageChanged event was raised
            Assert.That(eventCounters.EventCounter >= 1 && eventCounters.EventCounter2 == 1);
            var previousZoomEventCount = eventCounters.EventCounter;

            // Go to the previous tile
            imageViewer.PerformClick(previousTile);

            // Ensure that more ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter > previousZoomEventCount && eventCounters.EventCounter2 == 1);
        }

        /// <summary>
        /// Tests whether the <see cref="PreviousTileToolStripMenuItem"/> properly updates the 
        /// enabled state during rotation. [DotNetRCAndUtils #127]
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousTileToolStripMenuItemEnableRotationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Set the fit mode to fit to width
            imageViewer.FitMode = FitMode.FitToWidth;

            // The previous tile menu item should be disabled
            PreviousTileToolStripMenuItem previousTile =
                FormMethods.GetFormComponent<PreviousTileToolStripMenuItem>(_imageViewerForm);
            Assert.That(!previousTile.Enabled);

            // Rotate the image 90 degrees
            imageViewer.Rotate(90, true, true);

            // The previous tile menu item should be enabled
            Assert.That(previousTile.Enabled);
        }

        #endregion Previous Tile

        #region Next Tile

        /// <summary>
        /// Tests the enabled state of the <see cref="NextTileToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextTileToolStripMenuItemEnableDisableTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Check that the next tile toolstrip menu item is disabled without an image open
            NextTileToolStripMenuItem nextTile = FormMethods.GetFormComponent<NextTileToolStripMenuItem>(_imageViewerForm);
            Assert.That(!nextTile.Enabled);

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Check that the next tile menu item is enabled now that the image is open
            Assert.That(nextTile.Enabled);

            // Go to the last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Check that the next tile menu item is disabled
            Assert.That(!nextTile.Enabled);
        }

        /// <summary>
        /// Tests whether the <see cref="NextTileToolStripMenuItem"/> updates the zoom history 
        /// properly.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextTileToolStripMenuItemZoomHistoryTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Go to the next tile
            NextTileToolStripMenuItem nextTile = FormMethods.GetFormComponent<NextTileToolStripMenuItem>(_imageViewerForm);
            imageViewer.PerformClick(nextTile);

            // Ensure that there is only one zoom history entry for this page
            Assert.That(!imageViewer.CanZoomPrevious && !imageViewer.CanZoomNext);
        }

        /// <summary>
        /// Tests whether the <see cref="NextTileToolStripMenuItem"/> preserves the fit mode.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextTileToolStripMenuItemPreserveFitModeTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Set the fit mode to fit to page
            imageViewer.FitMode = FitMode.FitToPage;

            // Go to the next three tile
            NextTileToolStripMenuItem nextTile = FormMethods.GetFormComponent<NextTileToolStripMenuItem>(_imageViewerForm);
            imageViewer.PerformClick(nextTile);

            // This should be the next page
            Assert.That(imageViewer.PageNumber == 2);

            // Ensure that the fit mode was preserved
            Assert.That(imageViewer.FitMode == FitMode.FitToPage);

            // Set the fit mode to fit to width
            imageViewer.FitMode = FitMode.FitToWidth;

            // Go to the next four tiles
            imageViewer.PerformClick(nextTile);
            imageViewer.PerformClick(nextTile);
            imageViewer.PerformClick(nextTile);
            imageViewer.PerformClick(nextTile);

            // This should be the top tile of the third page
            Assert.That(imageViewer.PageNumber == 3);

            // Ensure that the fit mode was preserved
            Assert.That(imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Tests whether the <see cref="NextTileToolStripMenuItem"/> raises events properly.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextTileToolStripMenuItemEventsTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Set the fit mode to fit to width
            imageViewer.FitMode = FitMode.FitToWidth;

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ZoomChanged and PageChanged events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;
            imageViewer.PageChanged += eventCounters.CountEvent2<PageChangedEventArgs>;

            // Go to the next tile
            NextTileToolStripMenuItem nextTile = FormMethods.GetFormComponent<NextTileToolStripMenuItem>(_imageViewerForm);
            imageViewer.PerformClick(nextTile);

            // Ensure that ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter >= 1 && eventCounters.EventCounter2 == 0);
            var previousZoomEventCount = eventCounters.EventCounter;

            // Go to the next tile
            imageViewer.PerformClick(nextTile);

            // Ensure that more ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter > previousZoomEventCount && eventCounters.EventCounter2 == 0);
            previousZoomEventCount = eventCounters.EventCounter;

            // Go to the next tile
            imageViewer.PerformClick(nextTile);

            // Ensure that more ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter > previousZoomEventCount && eventCounters.EventCounter2 == 0);
            previousZoomEventCount = eventCounters.EventCounter;

            // Go to the next tile
            imageViewer.PerformClick(nextTile);

            // Ensure that more ZoomChanged events were fired and one more PageChanged event was 
            // raised.
            Assert.That(eventCounters.EventCounter > previousZoomEventCount && eventCounters.EventCounter2 == 1);
        }

        /// <summary>
        /// Tests whether the <see cref="NextTileToolStripMenuItem"/> properly updates the 
        /// enabled state during rotation. [DotNetRCAndUtils #127]
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextTileToolStripMenuItemEnableRotationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Set the fit mode to fit to width
            imageViewer.FitMode = FitMode.FitToWidth;

            // Go to the last tile
            imageViewer.GoToLastPage();
            imageViewer.SelectNextTile();
            imageViewer.SelectNextTile();
            imageViewer.SelectNextTile();

            // The next tile menu item should be disabled
            NextTileToolStripMenuItem nextTile = FormMethods.GetFormComponent<NextTileToolStripMenuItem>(_imageViewerForm);
            Assert.That(!nextTile.Enabled);

            // Rotate the image 90 degrees
            imageViewer.FitMode = FitMode.None;
            imageViewer.Rotate(90, true, true);

            // The next tile menu item should be enabled
            Assert.That(nextTile.Enabled);
        }

        #endregion Next Tile

        #region First Page

        /// <summary>
        /// Tests whether <see cref="FirstPageToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FirstPageToolStripMenuItemDisabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the First Page menu item
            FirstPageToolStripMenuItem firstPage =
                FormMethods.GetFormComponent<FirstPageToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!firstPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="FirstPageToolStripMenuItem"/> is disabled 
        /// on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FirstPageToolStripMenuItemDisabledOnFirstPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the First Page menu item
            FirstPageToolStripMenuItem firstPage =
                FormMethods.GetFormComponent<FirstPageToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!firstPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="FirstPageToolStripMenuItem"/> is enabled 
        /// when not on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FirstPageToolStripMenuItemEnabledNotOnFirstPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Go to the last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Get the First Page menu item
            FirstPageToolStripMenuItem firstPage =
                FormMethods.GetFormComponent<FirstPageToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(firstPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="FirstPageToolStripMenuItem"/> navigates 
        /// to the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FirstPageToolStripMenuItemNavigationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Go to the last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Get the First Page menu item
            FirstPageToolStripMenuItem firstPage =
                FormMethods.GetFormComponent<FirstPageToolStripMenuItem>(_imageViewerForm);

            // Go to the first page
            firstPage.PerformClick();

            // Check that the first page is active
            Assert.That(imageViewer.PageNumber == 1);
        }

        /// <summary>
        /// Tests whether <see cref="FirstPageToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.PageChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FirstPageToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Go to fourth page
            imageViewer.PageNumber = 4;

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of PageChanged events
            imageViewer.PageChanged += eventCounters.CountEvent<PageChangedEventArgs>;

            // Click the FirstPageToolStripMenuItem
            FirstPageToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<FirstPageToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion First Page

        #region Previous Page

        /// <summary>
        /// Tests whether <see cref="PreviousPageToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousPageToolStripMenuItemDisabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the Previous Page menu item
            PreviousPageToolStripMenuItem previousPage =
                FormMethods.GetFormComponent<PreviousPageToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!previousPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="PreviousPageToolStripMenuItem"/> is disabled 
        /// on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousPageToolStripMenuItemDisabledOnFirstPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Previous Page menu item
            PreviousPageToolStripMenuItem previousPage =
                FormMethods.GetFormComponent<PreviousPageToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!previousPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="PreviousPageToolStripMenuItem"/> is enabled 
        /// when not on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousPageToolStripMenuItemEnabledNotOnFirstPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Go to the last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Get the Previous Page menu item
            PreviousPageToolStripMenuItem previousPage =
                FormMethods.GetFormComponent<PreviousPageToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(previousPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="PreviousPageToolStripMenuItem"/> navigates 
        /// to the previous page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousPageToolStripMenuItemNavigationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Go to the last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Get the Previous Page menu item
            PreviousPageToolStripMenuItem previousPage =
                FormMethods.GetFormComponent<PreviousPageToolStripMenuItem>(_imageViewerForm);

            // Go to the previous page
            previousPage.PerformClick();

            // Check that the second-to-last page is active
            Assert.That(imageViewer.PageNumber == imageViewer.PageCount - 1);
        }

        /// <summary>
        /// Tests whether <see cref="PreviousPageToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.PageChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousPageToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Go to fourth page
            imageViewer.PageNumber = 4;
            
            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of PageChanged events
            imageViewer.PageChanged += eventCounters.CountEvent<PageChangedEventArgs>;

            // Click the PreviousPageToolStripMenuItem
            PreviousPageToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<PreviousPageToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion Previous Page

        #region Goto Page

        /// <summary>
        /// Tests whether <see cref="PageNavigationToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToPageToolStripMenuItemDisabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the Page Number menu item
            PageNavigationToolStripMenuItem gotoPage = 
                FormMethods.GetFormComponent<PageNavigationToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!gotoPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="PageNavigationToolStripMenuItem"/> is disabled 
        /// with a single page image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToPageToolStripMenuItemDisabledWithSinglePageImage()
        {
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the annotation test image
            OpenAnnotationTestImage(imageViewer);

            // Get the Page Number menu item
            PageNavigationToolStripMenuItem gotoPage =
                FormMethods.GetFormComponent<PageNavigationToolStripMenuItem>(_imageViewerForm);

            ExtractException.Assert("ELI21989",
                "Could not find Page Number menu item!", gotoPage != null);

            // Check that the menu item is disabled
            Assert.That(!gotoPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="PageNavigationToolStripMenuItem"/> is enabled 
        /// with a multipage image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToPageToolStripMenuItemEnabledWithMultiplePageImage()
        {
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Page Number menu item
            PageNavigationToolStripMenuItem gotoPage =
                FormMethods.GetFormComponent<PageNavigationToolStripMenuItem>(_imageViewerForm);

            ExtractException.Assert("ELI21987",
                "Could not find Page Number menu item!", gotoPage != null);

            // Check that the menu item is enabled
            Assert.That(gotoPage.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="PageNavigationToolStripMenuItem"/> opens the 
        /// Goto Page dialog.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_GoToPageToolStripMenuItemDialogTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Page Number menu item
            PageNavigationToolStripMenuItem gotoPage =
                FormMethods.GetFormComponent<PageNavigationToolStripMenuItem>(_imageViewerForm);

            ExtractException.Assert("ELI21988",
                "Could not find Page Number menu item!", gotoPage != null);

            // Click the Page Number menu item
            gotoPage.PerformClick();

            // Ask user if Goto Page dialog appeared
            Assert.That(
                MessageBox.Show("Did the Goto Page dialog appear?", "Did dialog appear?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the <see cref="PageNavigationToolStripMenuItem"/> navigates to the 
        /// specified page.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_GoToPageToolStripMenuItemNavigationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Page Number menu item
            PageNavigationToolStripMenuItem gotoPage =
                FormMethods.GetFormComponent<PageNavigationToolStripMenuItem>(_imageViewerForm);

            ExtractException.Assert("ELI22223",
                "Could not find Page Number menu item!", gotoPage != null);

            FormMethods.ShowModelessInstructionsAndWait(new string[] {
                "Click on the image viewer title bar to give it focus.",
                "Select Page number... from the View - Page Navigation menu.",
                "Type in a valid page number to change the page.",
                "Click okay to close this dialog and end this test if the proper page is displayed."});

            // Ask user if Goto Page dialog appeared
            Assert.That(
                MessageBox.Show("Did the expected page appear?", "Did page change?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Tests whether <see cref="PageNavigationToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.PageChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_GoToPageToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of PageChanged events
            imageViewer.PageChanged += eventCounters.CountEvent<PageChangedEventArgs>;

            FormMethods.ShowModelessInstructionsAndWait(new string[] {
                "Click on the image viewer title bar to give it focus.",
                "Select Page number... from the View - Page Navigation menu.",
                "Type in a valid page number to change the page.",
                "Click okay to close this dialog and end this test."});

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion Goto Page

        #region Next Page

        /// <summary>
        /// Tests whether <see cref="NextPageToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextPageToolStripMenuItemDisabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the Next Page menu item
            NextPageToolStripMenuItem nextPage = 
                FormMethods.GetFormComponent<NextPageToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!nextPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="NextPageToolStripMenuItem"/> is disabled 
        /// on the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextPageToolStripMenuItemDisabledOnLastPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Go to the last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Get the Next Page menu item
            NextPageToolStripMenuItem nextPage =
                FormMethods.GetFormComponent<NextPageToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!nextPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="NextPageToolStripMenuItem"/> is enabled 
        /// when not on the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextPageToolStripMenuItemEnabledOnNotLastPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Next Page menu item
            NextPageToolStripMenuItem nextPage =
                FormMethods.GetFormComponent<NextPageToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(nextPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="NextPageToolStripMenuItem"/> is enabled 
        /// when not on the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextPageToolStripMenuItemNavigatesToNextPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Next Page menu item
            NextPageToolStripMenuItem nextPage =
                FormMethods.GetFormComponent<NextPageToolStripMenuItem>(_imageViewerForm);

            // Click the menu item
            nextPage.PerformClick();

            // Check that the image is on second page
            Assert.That(imageViewer.PageNumber == 2);
        }

        /// <summary>
        /// Tests whether <see cref="NextPageToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.PageChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextPageToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of PageChanged events
            imageViewer.PageChanged += eventCounters.CountEvent<PageChangedEventArgs>;

            // Click the NextPageToolStripMenuItem
            NextPageToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<NextPageToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion Next Page

        #region Last Page

        /// <summary>
        /// Tests whether <see cref="LastPageToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_LastPageToolStripMenuItemDisabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the Last Page menu item
            LastPageToolStripMenuItem lastPage =
                FormMethods.GetFormComponent<LastPageToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!lastPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="LastPageToolStripMenuItem"/> is disabled 
        /// on the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_LastPageToolStripMenuItemDisabledOnLastPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Go to the last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Get the Last Page menu item
            LastPageToolStripMenuItem lastPage =
                FormMethods.GetFormComponent<LastPageToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!lastPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="LastPageToolStripMenuItem"/> is enabled 
        /// when not on the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_LastPageToolStripMenuItemEnabledNotOnLastPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Last Page menu item
            LastPageToolStripMenuItem lastPage =
                FormMethods.GetFormComponent<LastPageToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(lastPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="LastPageToolStripMenuItem"/> navigates 
        /// to the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_LastPageToolStripMenuItemNavigationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Last Page menu item
            LastPageToolStripMenuItem lastPage =
                FormMethods.GetFormComponent<LastPageToolStripMenuItem>(_imageViewerForm);

            // Go to the last page
            lastPage.PerformClick();

            // Check that the last page is active
            Assert.That(imageViewer.PageNumber == imageViewer.PageCount);
        }

        /// <summary>
        /// Tests whether <see cref="LastPageToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.PageChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_LastPageToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of PageChanged events
            imageViewer.PageChanged += eventCounters.CountEvent<PageChangedEventArgs>;

            // Click the LastPageToolStripMenuItem
            LastPageToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<LastPageToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion Last Page

        #region Rotate Clockwise

        /// <summary>
        /// Test that the <see cref="RotateClockwiseToolStripMenuItem"/> is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateClockwiseToolStripMenuItemDisabledWithNoImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Get the menu item
            RotateClockwiseToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<RotateClockwiseToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!clickMe.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="RotateClockwiseToolStripMenuItem"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateClockwiseToolStripMenuItemEnabledWithImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the menu item
            RotateClockwiseToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<RotateClockwiseToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(clickMe.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="RotateClockwiseToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.OrientationChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateClockwiseToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of OrientationChanged events
            imageViewer.OrientationChanged += eventCounters.CountEvent<OrientationChangedEventArgs>;

            // Click the RotateClockwiseToolStripMenuItem
            RotateClockwiseToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<RotateClockwiseToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one OrientationChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripMenuItemTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Fit to page so that rotation appears clearly
            imageViewer.FitMode = FitMode.FitToPage;

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateClockwiseToolStripMenuItem rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see the image before rotation
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image rotate to the right?", "Did image rotate?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripMenuItemWithHighlightTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight =
                new Highlight(imageViewer, "Test", new Point(220, 200), new Point(400, 200), 120, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateClockwiseToolStripMenuItem rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and highlight rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripMenuItemViewPerspectiveTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the annotation test image
            OpenRotatedViewPerspectiveTestImage(imageViewer);

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateClockwiseToolStripMenuItem rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripMenuItemViewPerspectiveUnlicensedTest()
        {
            // Disable the view perspective license
            LicenseUtilities.DisableId(LicenseIdName.AnnotationFeature);
            
            // Ensure that Annotation feature is unlicensed
            ExtractException.Assert("ELI21927", "Annotation feature is licensed!",
                !LicenseUtilities.IsLicensed(LicenseIdName.AnnotationFeature));

            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the annotation test image
            OpenRotatedViewPerspectiveTestImage(imageViewer);

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateClockwiseToolStripMenuItem rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripMenuItemViewPerspectiveWithHighlightTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the annotation test image
            OpenRotatedViewPerspectiveTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(120, 400), new Point(300, 400), 300, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Select the highlight to make it clear to the user
            imageViewer.LayerObjects.SelectAll();

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateClockwiseToolStripMenuItem rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and highlight rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripMenuItemViewPerspectiveUnlicensedWithHighlightTest()
        {
            // Disable the view perspective license
            LicenseUtilities.DisableId(LicenseIdName.AnnotationFeature);
                        
            // Ensure that Annotation feature is unlicensed
            ExtractException.Assert("ELI21928", "Annotation feature is licensed!",
                !LicenseUtilities.IsLicensed(LicenseIdName.AnnotationFeature));

            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the annotation test image
            OpenRotatedViewPerspectiveTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(120, 400), new Point(300, 400), 300, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Select the highlight to make it clear to the user
            imageViewer.LayerObjects.SelectAll();

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateClockwiseToolStripMenuItem rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and highlight rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripMenuItemAnnotationsTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the annotation test image
            OpenAnnotationTestImage(imageViewer);

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateClockwiseToolStripMenuItem rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and annotations rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripMenuItemAnnotationsWithHighlightTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the annotation test image
            OpenAnnotationTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(120, 400), new Point(300, 400), 300, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Select the highlight to make it clear to the user
            imageViewer.LayerObjects.SelectAll();

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateClockwiseToolStripMenuItem rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image, annotations and  highlight rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripMenuItemViewPerspectiveWithAnnotationsTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the rotated view perspective with annotations image
            OpenRotatedViewPerspectiveAnnotationTestImage(imageViewer);

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Sleep for a second so the user can see the image and annotations
            System.Threading.Thread.Sleep(1000);

            // Get the rotate clockwise menu item
            RotateClockwiseToolStripMenuItem rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripMenuItem>(_imageViewerForm);

            // Click the menu item
            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and annotations rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the <see cref="RotateClockwiseToolStripMenuItem"/> rotates 
        /// only the active page.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripMenuItemRotatesActivePageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Fit to page so that rotation appears clearly
            imageViewer.FitMode = FitMode.FitToPage;

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateClockwiseToolStripMenuItem rotateClockwise =
                FormMethods.GetFormComponent<RotateClockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see the image before rotation
            System.Threading.Thread.Sleep(1000);
            rotateClockwise.PerformClick();
            _imageViewerForm.Refresh();

            // Sleep for a second so the user can see page 1 rotated before changing 
            // to page 2
            System.Threading.Thread.Sleep(1000);
            imageViewer.PageNumber = 2;
            _imageViewerForm.Refresh();

            Assert.That(
                MessageBox.Show("Did page 1 rotate to the right while page 2 did not rotate?", 
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Rotate Clockwise

        #region Rotate Counterclockwise

        /// <summary>
        /// Test that the <see cref="RotateCounterclockwiseToolStripMenuItem"/> is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateCounterclockwiseToolStripMenuItemDisabledWithNoImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Get the menu item
            RotateCounterclockwiseToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!clickMe.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="RotateCounterclockwiseToolStripMenuItem"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateCounterclockwiseToolStripMenuItemEnabledWithImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the menu item
            RotateCounterclockwiseToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(clickMe.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="RotateCounterclockwiseToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.OrientationChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateCounterclockwiseToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of OrientationChanged events
            imageViewer.OrientationChanged += eventCounters.CountEvent<OrientationChangedEventArgs>;

            // Click the RotateCounterclockwiseToolStripMenuItem
            RotateCounterclockwiseToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one OrientationChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripMenuItemTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Fit to page so that rotation appears clearly
            imageViewer.FitMode = FitMode.FitToPage;

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateCounterclockwiseToolStripMenuItem rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see the image before rotation
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image rotate to the left?", "Did image rotate?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripMenuItemWithHighlightTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(220, 200), new Point(400, 200), 120, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateCounterclockwiseToolStripMenuItem rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and highlight rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripMenuItemViewPerspectiveTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the annotation test image
            OpenRotatedViewPerspectiveTestImage(imageViewer);

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateCounterclockwiseToolStripMenuItem rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripMenuItemViewPerspectiveUnlicensedTest()
        {
            // Disable the view perspective license
            LicenseUtilities.DisableId(LicenseIdName.AnnotationFeature);

            // Ensure that Annotation feature is unlicensed
            ExtractException.Assert("ELI21925", "Annotation feature is licensed!",
                !LicenseUtilities.IsLicensed(LicenseIdName.AnnotationFeature));

            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the annotation test image
            OpenRotatedViewPerspectiveTestImage(imageViewer);

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateCounterclockwiseToolStripMenuItem rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripMenuItemViewPerspectiveWithHighlightTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the annotation test image
            OpenRotatedViewPerspectiveTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(120, 400), new Point(300, 400), 300, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Select the highlight to make it clear to the user
            imageViewer.LayerObjects.SelectAll();

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateCounterclockwiseToolStripMenuItem rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and highlight rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripMenuItemViewPerspectiveUnlicensedWithHighlightTest()
        {
            // Disable the view perspective license
            LicenseUtilities.DisableId(LicenseIdName.AnnotationFeature);

            // Ensure that Annotation feature is unlicensed
            ExtractException.Assert("ELI21926", "Annotation feature is licensed!",
                !LicenseUtilities.IsLicensed(LicenseIdName.AnnotationFeature));

            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the annotation test image
            OpenRotatedViewPerspectiveTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(120, 400), new Point(300, 400), 300, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Select the highlight to make it clear to the user
            imageViewer.LayerObjects.SelectAll();

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateCounterclockwiseToolStripMenuItem rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and highlight rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripMenuItemAnnotationsTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the annotation test image
            OpenAnnotationTestImage(imageViewer);

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateCounterclockwiseToolStripMenuItem rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and annotations rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripMenuItemAnnotationsWithHighlightTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the annotation test image
            OpenAnnotationTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(120, 400), new Point(300, 400), 300, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Select the highlight to make it clear to the user
            imageViewer.LayerObjects.SelectAll();

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateCounterclockwiseToolStripMenuItem rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image, annotations and  highlight rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripMenuItemViewPerspectiveWithAnnotationsTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the rotated view perspective with annotations image
            OpenRotatedViewPerspectiveAnnotationTestImage(imageViewer);

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Sleep for a second so the user can see the image and annotations
            System.Threading.Thread.Sleep(1000);

            // Get the rotate counter-clockwise menu item
            RotateCounterclockwiseToolStripMenuItem rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripMenuItem>(_imageViewerForm);

            // Click the menu item
            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and annotations rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the <see cref="RotateCounterclockwiseToolStripMenuItem"/> 
        /// only rotates the active page.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripMenuItemActivePageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Fit to page so that rotation appears clearly
            imageViewer.FitMode = FitMode.FitToPage;

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Get the rotate clockwise menu item
            RotateCounterclockwiseToolStripMenuItem rotateCounterclockwise =
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripMenuItem>(_imageViewerForm);

            // Sleep for a second so the user can see the image before rotation
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();
            _imageViewerForm.Refresh();

            // Sleep for a second so the user can see the image after rotation
            System.Threading.Thread.Sleep(1000);

            // Change to page 2 and pause again
            imageViewer.PageNumber = 2;
            System.Threading.Thread.Sleep(1000);

            Assert.That(
                MessageBox.Show("Did page 1 rotate to the left while page 2 did not rotate?", 
                "Did only page 1 rotate?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Rotate Counterclockwise

        #region Pan 

        /// <summary>
        /// Tests whether <see cref="PanToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PanToolStripMenuItemDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the Pan menu item
            PanToolStripMenuItem pan =
                FormMethods.GetFormComponent<PanToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!pan.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="PanToolStripMenuItem"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PanToolStripMenuItemEnabledWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan menu item
            PanToolStripMenuItem pan =
                FormMethods.GetFormComponent<PanToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(pan.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="PanToolStripMenuItem"/> is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PanToolStripMenuItemToggledOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan menu item
            PanToolStripMenuItem pan =
                FormMethods.GetFormComponent<PanToolStripMenuItem>(_imageViewerForm);

            // Select the Pan tool
            pan.PerformClick();

            // Check that the menu item is checked
            Assert.That(pan.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="PanToolStripMenuItem"/> is toggled off 
        /// when different cursor tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PanToolStripMenuItemToggledOffWithDifferentSelectionTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan menu item
            PanToolStripMenuItem pan =
                FormMethods.GetFormComponent<PanToolStripMenuItem>(_imageViewerForm);

            // Select the Pan tool
            pan.PerformClick();

            // Get the ZoomWindow menu item
            ZoomWindowToolStripMenuItem zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripMenuItem>(_imageViewerForm);

            // Select the Zoom Window tool
            zoomWindow.PerformClick();

            // Check that the Pan menu item is unchecked
            Assert.That(!pan.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="PanToolStripMenuItem"/> sets the CursorTool 
        /// property when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PanToolStripMenuItemSetsCursorPropertyWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan menu item
            PanToolStripMenuItem pan =
                FormMethods.GetFormComponent<PanToolStripMenuItem>(_imageViewerForm);

            // Select the Pan tool
            pan.PerformClick();

            // Check that the CursorTool property has been set
            Assert.That(imageViewer.CursorTool == CursorTool.Pan);
        }

        /// <summary>
        /// Tests whether <see cref="PanToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PanToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of CursorToolChanged events
            imageViewer.CursorToolChanged += eventCounters.CountEvent<CursorToolChangedEventArgs>;

            // Click the PanToolStripMenuItem
            PanToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<PanToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion Pan 

        #region Zoom Window

        /// <summary>
        /// Tests whether <see cref="ZoomWindowToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowToolStripMenuItemDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the Zoom Window menu item
            ZoomWindowToolStripMenuItem zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!zoomWindow.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomWindowToolStripMenuItem"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowToolStripMenuItemEnabledWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Zoom Window menu item
            ZoomWindowToolStripMenuItem zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(zoomWindow.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomWindowToolStripMenuItem"/> is toggled 
        /// on by default with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowToolStripMenuItemOnByDefaultWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Zoom Window menu item
            ZoomWindowToolStripMenuItem zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is toggled on
            Assert.That(zoomWindow.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomWindowToolStripMenuItem"/> is toggled 
        /// on when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowToolStripMenuItemOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan menu item
            PanToolStripMenuItem pan =
                FormMethods.GetFormComponent<PanToolStripMenuItem>(_imageViewerForm);

            // Set Pan mode
            pan.PerformClick();

            // Get the Zoom Window menu item
            ZoomWindowToolStripMenuItem zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripMenuItem>(_imageViewerForm);

            // Set Zoom Window mode
            zoomWindow.PerformClick();

            // Check that the menu item is toggled on
            Assert.That(zoomWindow.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomWindowToolStripMenuItem"/> is toggled 
        /// off when different tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowToolStripMenuItemOffWhenNotSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan menu item
            PanToolStripMenuItem pan =
                FormMethods.GetFormComponent<PanToolStripMenuItem>(_imageViewerForm);

            // Set Pan mode
            pan.PerformClick();

            // Get the Zoom Window menu item
            ZoomWindowToolStripMenuItem zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is toggled off
            Assert.That(!zoomWindow.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomWindowToolStripMenuItem"/> sets CursorTool 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowToolStripMenuItemSetsCursorToolWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan menu item
            PanToolStripMenuItem pan =
                FormMethods.GetFormComponent<PanToolStripMenuItem>(_imageViewerForm);

            // Set Pan mode
            pan.PerformClick();

            // Get the Zoom Window menu item
            ZoomWindowToolStripMenuItem zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripMenuItem>(_imageViewerForm);

            // Set Zoom Window mode
            zoomWindow.PerformClick();

            // Check that the CursorTool is set properly
            Assert.That(imageViewer.CursorTool == CursorTool.ZoomWindow);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomWindowToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Set a different cursor tool
            imageViewer.CursorTool = CursorTool.SelectLayerObject;
            
            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of CursorToolChanged events
            imageViewer.CursorToolChanged += eventCounters.CountEvent<CursorToolChangedEventArgs>;

            // Click the ZoomWindowToolStripMenuItem
            ZoomWindowToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<ZoomWindowToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking the <see cref="ZoomWindowToolStripMenuItem"/> a drag
        /// event on the <see cref="ImageViewer"/> raises a
        /// <see cref="ImageViewer.ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ZoomWindowToolStripMenuItemRaisesZoomChangedEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ZoomChanged events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;

            // Click the ZoomWindowToolStripMenuItem
            ZoomWindowToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<ZoomWindowToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Please click and drag to draw a rectangle on the image.",
                "Click okay to close this dialog and end this test."};

            // Prompt the user to click and drag to zoom in on image
            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that exactly one ZoomChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion Zoom Window

        #region Highlighter 

        /// <summary>
        /// Tests whether <see cref="HighlightToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HighlightToolStripMenuItemDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the Highlight menu item
            HighlightToolStripMenuItem highlight =
                FormMethods.GetFormComponent<HighlightToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!highlight.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="HighlightToolStripMenuItem"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HighlightToolStripMenuItemEnabledWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Highlight menu item
            HighlightToolStripMenuItem highlight =
                FormMethods.GetFormComponent<HighlightToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(highlight.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="HighlightToolStripMenuItem"/> is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HighlightToolStripMenuItemToggledOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Highlight menu item
            HighlightToolStripMenuItem highlight =
                FormMethods.GetFormComponent<HighlightToolStripMenuItem>(_imageViewerForm);

            // Select the Highlight tool
            highlight.PerformClick();

            // Check that the menu item is checked
            Assert.That(highlight.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="HighlightToolStripMenuItem"/> is toggled off 
        /// when different cursor tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HighlightToolStripMenuItemToggledOffWithDifferentSelectionTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Highlight menu item
            HighlightToolStripMenuItem highlight =
                FormMethods.GetFormComponent<HighlightToolStripMenuItem>(_imageViewerForm);

            // Select the Highlight tool
            highlight.PerformClick();

            // Get the ZoomWindow menu item
            ZoomWindowToolStripMenuItem zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripMenuItem>(_imageViewerForm);

            // Select the Zoom Window tool
            zoomWindow.PerformClick();

            // Check that the Highlight menu item is unchecked
            Assert.That(!highlight.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="HighlightToolStripMenuItem"/> toggles between 
        /// the two different highlight tools when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HighlightToolStripMenuItemTogglesBetweenToolsTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Highlight menu item
            HighlightToolStripMenuItem highlight =
                FormMethods.GetFormComponent<HighlightToolStripMenuItem>(_imageViewerForm);

            // Select the Highlight tool
            highlight.PerformClick();

            // Store whether or not the Any-angle highlight tool is selected
            bool anyAngleToolIsSelected =
                imageViewer.CursorTool == CursorTool.AngularHighlight;

            // Store whether or not the Rectangular highlight tool is selected
            bool rectangularToolIsSelected =
                imageViewer.CursorTool == CursorTool.RectangularHighlight;

            // Select the Highlight tool again
            highlight.PerformClick();

            // Check that the tool selection states have both changed
            Assert.That((imageViewer.CursorTool == CursorTool.AngularHighlight) !=
                anyAngleToolIsSelected && 
                (imageViewer.CursorTool == CursorTool.RectangularHighlight) !=
                rectangularToolIsSelected);
        }

        /// <summary>
        /// Tests whether <see cref="HighlightToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HighlightToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of CursorToolChanged events
            imageViewer.CursorToolChanged += eventCounters.CountEvent<CursorToolChangedEventArgs>;

            // Click the HighlightToolStripMenuItem
            HighlightToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<HighlightToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);

            // Click the HighlightToolStripMenuItem again
            clickMe.PerformClick();

            // Check that exactly one more CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 2);
        }

        #endregion Highlighter 

        #region Angular Highlight

        /// <summary>
        /// Tests whether <see cref="AngularHighlightToolStripMenuItem"/> sets CursorTool 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_AngularHighlightToolStripMenuItemSetsCursorToolWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan menu item
            PanToolStripMenuItem pan =
                FormMethods.GetFormComponent<PanToolStripMenuItem>(_imageViewerForm);

            // Set Pan mode
            pan.PerformClick();
            ExtractException.Assert("ELI22555", "Did not change to Pan cursor tool!",
                imageViewer.CursorTool == CursorTool.Pan);

            // Get the Angular Highlight menu item
            AngularHighlightToolStripMenuItem angle =
                FormMethods.GetFormComponent<AngularHighlightToolStripMenuItem>(_imageViewerForm);

            // Set Angular Highlight mode
            angle.PerformClick();

            // Check that the CursorTool is set properly
            Assert.That(imageViewer.CursorTool == CursorTool.AngularHighlight);
        }

        /// <summary>
        /// Tests whether <see cref="AngularHighlightToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_AngularHighlightToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of CursorToolChanged events
            imageViewer.CursorToolChanged += eventCounters.CountEvent<CursorToolChangedEventArgs>;

            // Click the AngularHighlightToolStripMenuItem
            AngularHighlightToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<AngularHighlightToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking the <see cref="AngularHighlightToolStripMenuItem"/> a drag
        /// event on the <see cref="ImageViewer"/> raises a
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_AngularHighlightToolStripMenuItemRaisesLayerObjectAddedEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ZoomChanged events
            imageViewer.LayerObjects.LayerObjectAdded += eventCounters.CountEvent<LayerObjectAddedEventArgs>;

            // Click the ZoomWindowToolStripMenuItem
            AngularHighlightToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<AngularHighlightToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Please click and drag to draw a highlight on the image.",
                "Click okay to close this dialog and end this test."};

            // Prompt the user to click and drag to zoom in on image
            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that exactly one LayerObjectAdded event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking the <see cref="AngularHighlightToolStripMenuItem"/> a drag
        /// event on the <see cref="ImageViewer"/> allows a user to create a highlight.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_AngularHighlightToolStripMenuItemAllowsHighlightCreationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Click the ZoomWindowToolStripMenuItem
            AngularHighlightToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<AngularHighlightToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Please click and drag to draw a highlight on the image.",
                "Click okay to close this dialog and end this test."};

            // Prompt the user to click and drag to zoom in on image
            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that a highlight was added
            Assert.That(imageViewer.LayerObjects.Count == 1);
        }

        #endregion Angular Highlight

        #region Rectangular Highlight

        /// <summary>
        /// Tests whether <see cref="RectangularHighlightToolStripMenuItem"/> sets CursorTool 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RectangularHighlightToolStripMenuItemSetsCursorToolWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan menu item
            PanToolStripMenuItem pan =
                FormMethods.GetFormComponent<PanToolStripMenuItem>(_imageViewerForm);

            // Set Pan mode
            pan.PerformClick();
            ExtractException.Assert("ELI22556", "Did not change to Pan cursor tool!",
                imageViewer.CursorTool == CursorTool.Pan);

            // Get the Rectangular Highlight menu item
            RectangularHighlightToolStripMenuItem rectangle =
                FormMethods.GetFormComponent<RectangularHighlightToolStripMenuItem>(_imageViewerForm);

            // Set Rectangular Highlight mode
            rectangle.PerformClick();

            // Check that the CursorTool is set properly
            Assert.That(imageViewer.CursorTool == CursorTool.RectangularHighlight);
        }

        /// <summary>
        /// Tests whether <see cref="RectangularHighlightToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RectangularHighlightToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of CursorToolChanged events
            imageViewer.CursorToolChanged += eventCounters.CountEvent<CursorToolChangedEventArgs>;

            // Click the RectangularHighlightToolStripMenuItem
            RectangularHighlightToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<RectangularHighlightToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking the <see cref="RectangularHighlightToolStripMenuItem"/> a drag
        /// event on the <see cref="ImageViewer"/> raises a
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RectangularHighlightToolStripMenuItemRaisesLayerObjectAddedEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ZoomChanged events
            imageViewer.LayerObjects.LayerObjectAdded += eventCounters.CountEvent<LayerObjectAddedEventArgs>;

            // Click the ZoomWindowToolStripMenuItem
            RectangularHighlightToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<RectangularHighlightToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Please click and drag to draw a highlight on the image.",
                "Click okay to close this dialog and end this test."};

            // Prompt the user to click and drag to zoom in on image
            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that exactly one LayerObjectAdded event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking the <see cref="RectangularHighlightToolStripMenuItem"/> a drag
        /// event on the <see cref="ImageViewer"/> allows a user to create a highlight.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RectangularHighlightToolStripMenuItemAllowsHighlightCreationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Click the ZoomWindowToolStripMenuItem
            RectangularHighlightToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<RectangularHighlightToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Please click and drag to draw a highlight on the image.",
                "Click okay to close this dialog and end this test."};

            // Prompt the user to click and drag to zoom in on image
            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that a highlight was added
            Assert.That(imageViewer.LayerObjects.Count == 1);
        }

        #endregion Rectangular Highlight

        #region Set Highlight Height

        /// <summary>
        /// Tests whether <see cref="SetHighlightHeightToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SetHighlightHeightToolStripMenuItemDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the SetHighlightHeight menu item
            SetHighlightHeightToolStripMenuItem setHighlightHeight =
                FormMethods.GetFormComponent<SetHighlightHeightToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!setHighlightHeight.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="SetHighlightHeightToolStripMenuItem"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SetHighlightHeightToolStripMenuItemEnabledWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SetHighlightHeight menu item
            SetHighlightHeightToolStripMenuItem setHighlightHeight =
                FormMethods.GetFormComponent<SetHighlightHeightToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(setHighlightHeight.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="SetHighlightHeightToolStripMenuItem"/> is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SetHighlightHeightToolStripMenuItemToggledOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SetHighlightHeight menu item
            SetHighlightHeightToolStripMenuItem setHighlightHeight =
                FormMethods.GetFormComponent<SetHighlightHeightToolStripMenuItem>(_imageViewerForm);

            // Select the tool
            setHighlightHeight.PerformClick();

            // Check that the menu item is checked
            Assert.That(setHighlightHeight.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="SetHighlightHeightToolStripMenuItem"/> is toggled off 
        /// when different cursor tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SetHighlightHeightToolStripMenuItemToggledOffWithDifferentSelectionTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SetHighlightHeight menu item
            SetHighlightHeightToolStripMenuItem setHighlightHeight =
                FormMethods.GetFormComponent<SetHighlightHeightToolStripMenuItem>(_imageViewerForm);

            // Select the tool
            setHighlightHeight.PerformClick();

            // Get the ZoomWindow menu item
            ZoomWindowToolStripMenuItem zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripMenuItem>(_imageViewerForm);

            // Select the Zoom Window tool
            zoomWindow.PerformClick();

            // Check that the SetHighlightHeight menu item is unchecked
            Assert.That(!setHighlightHeight.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="SetHighlightHeightToolStripMenuItem"/> sets the CursorTool 
        /// property when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SetHighlightHeightToolStripMenuItemSetsCursorPropertyWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SetHighlightHeight menu item
            SetHighlightHeightToolStripMenuItem setHighlightHeight =
                FormMethods.GetFormComponent<SetHighlightHeightToolStripMenuItem>(_imageViewerForm);

            // Select the tool
            setHighlightHeight.PerformClick();

            // Check that the CursorTool property has been set
            Assert.That(imageViewer.CursorTool == CursorTool.SetHighlightHeight);
        }

        /// <summary>
        /// Tests whether <see cref="SetHighlightHeightToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SetHighlightHeightToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of CursorToolChanged events
            imageViewer.CursorToolChanged += eventCounters.CountEvent<CursorToolChangedEventArgs>;

            // Click the SetHighlightHeightToolStripMenuItem
            SetHighlightHeightToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<SetHighlightHeightToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion Set Highlight Height

        #region Delete Highlights

        /// <summary>
        /// Tests whether <see cref="DeleteLayerObjectsToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_DeleteLayerObjectsToolStripMenuItemDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the DeleteLayerObjects menu item
            DeleteLayerObjectsToolStripMenuItem deleteLayerObjects =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!deleteLayerObjects.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="DeleteLayerObjectsToolStripMenuItem"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_DeleteLayerObjectsToolStripMenuItemEnabledWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the DeleteLayerObjects menu item
            DeleteLayerObjectsToolStripMenuItem deleteLayerObjects =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(deleteLayerObjects.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="DeleteLayerObjectsToolStripMenuItem"/> is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_DeleteLayerObjectsToolStripMenuItemToggledOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the DeleteLayerObjects menu item
            DeleteLayerObjectsToolStripMenuItem deleteLayerObjects =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripMenuItem>(_imageViewerForm);

            // Select the tool
            deleteLayerObjects.PerformClick();

            // Check that the menu item is checked
            Assert.That(deleteLayerObjects.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="DeleteLayerObjectsToolStripMenuItem"/> is toggled off 
        /// when different cursor tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_DeleteLayerObjectsToolStripMenuItemToggledOffWithDifferentSelectionTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the DeleteLayerObjects menu item
            DeleteLayerObjectsToolStripMenuItem deleteLayerObjects =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripMenuItem>(_imageViewerForm);

            // Select the tool
            deleteLayerObjects.PerformClick();

            // Get the ZoomWindow menu item
            ZoomWindowToolStripMenuItem zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripMenuItem>(_imageViewerForm);

            // Select the Zoom Window tool
            zoomWindow.PerformClick();

            // Check that the DeleteLayerObjects menu item is unchecked
            Assert.That(!deleteLayerObjects.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="DeleteLayerObjectsToolStripMenuItem"/> sets the CursorTool 
        /// property when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_DeleteLayerObjectsToolStripMenuItemSetsCursorPropertyWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the DeleteLayerObjects menu item
            DeleteLayerObjectsToolStripMenuItem deleteLayerObjects =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripMenuItem>(_imageViewerForm);

            // Select the tool
            deleteLayerObjects.PerformClick();

            // Check that the CursorTool property has been set
            Assert.That(imageViewer.CursorTool == CursorTool.DeleteLayerObjects);
        }

        /// <summary>
        /// Tests whether <see cref="DeleteLayerObjectsToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_DeleteHighlightsToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of CursorToolChanged events
            imageViewer.CursorToolChanged += eventCounters.CountEvent<CursorToolChangedEventArgs>;

            // Click the DeleteLayerObjectsToolStripMenuItem
            DeleteLayerObjectsToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="DeleteLayerObjectsToolStripMenuItem"/>
        /// dragging to select a group of highlights raises one
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event for each highlight selected.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_DeleteHighlightsToolStripMenuItemRaisesLayerObjectDeletedEventForEachDeletedHighlightTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add 3 highlights to page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(220, 400), new Point(700, 400), 400, 1);
            imageViewer.LayerObjects.Add(highlight);
            highlight = 
                new Highlight(imageViewer, "Test", new Point(220, 900), new Point(700, 900), 400, 1);
            imageViewer.LayerObjects.Add(highlight);
            highlight =
                new Highlight(imageViewer, "Test", new Point(750, 500), new Point(1200, 700), 300, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Add a listener to the highlight deleted event
            imageViewer.LayerObjects.LayerObjectDeleted += eventCounters.CountEvent<LayerObjectDeletedEventArgs>;

            // Click the DeleteLayerObjectsToolStripMenuItem
            DeleteLayerObjectsToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            FormMethods.ShowModelessInstructionsAndWait(new string[] {
                "Click and drag a box around all three highlights.",
                "Click okay to close this dialog and end this test."});

            // Check that there were 3 delete highlight events raised
            Assert.That(eventCounters.EventCounter == 3);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="DeleteLayerObjectsToolStripMenuItem"/>
        /// dragging to select a group of highlights deletes all selected highlights.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_DeleteHighlightsToolStripMenuItemAllowsDragSelectionForDeleteTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add 3 highlights to page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(220, 400), new Point(700, 400), 400, 1);
            imageViewer.LayerObjects.Add(highlight);
            highlight =
                new Highlight(imageViewer, "Test", new Point(220, 900), new Point(700, 900), 400, 1);
            imageViewer.LayerObjects.Add(highlight);
            highlight =
                new Highlight(imageViewer, "Test", new Point(750, 500), new Point(1200, 700), 300, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Click the DeleteLayerObjectsToolStripMenuItem
            DeleteLayerObjectsToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            FormMethods.ShowModelessInstructionsAndWait(new string[] {
                "Click and drag a box around all three highlights.",
                "Click okay to close this dialog and end this test."});

            // Check that all highlights were deleted
            Assert.That(imageViewer.LayerObjects.Count == 0);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="DeleteLayerObjectsToolStripMenuItem"/>
        /// and dragging to select a group of highlights the cursor tool reverts to the
        /// last used cursor tool.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_DeleteHighlightsToolStripMenuItemSwitchesToLastUsedCursorToolTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Set the the current cursor tool to ZoomWindow
            imageViewer.CursorTool = CursorTool.ZoomWindow;

            // Add 3 highlights to page 1
            Highlight highlight =
                new Highlight(imageViewer, "Test", new Point(220, 400), new Point(700, 400), 400, 1);
            imageViewer.LayerObjects.Add(highlight);
            highlight =
                new Highlight(imageViewer, "Test", new Point(220, 900), new Point(700, 900), 400, 1);
            imageViewer.LayerObjects.Add(highlight);
            highlight =
                new Highlight(imageViewer, "Test", new Point(750, 500), new Point(1200, 700), 300, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Click the DeleteLayerObjectsToolStripMenuItem
            DeleteLayerObjectsToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that the cursor tool changed
            ExtractException.Assert("ELI21936", "Did not change to delete highlight cursor tool!",
                imageViewer.CursorTool == CursorTool.DeleteLayerObjects);

            FormMethods.ShowModelessInstructionsAndWait(new string[] {
                "Click and drag a box around all three highlights.",
                "Click okay to close this dialog and end this test."});

            // Check that the image viewer switched back to the ZoomWindow tool
            Assert.That(imageViewer.CursorTool == CursorTool.ZoomWindow);
        }

        #endregion Delete Highlights

        #region Select Highlight

        /// <summary>
        /// Tests whether <see cref="SelectLayerObjectToolStripMenuItem"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SelectLayerObjectToolStripMenuItemDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the SelectLayerObject menu item
            SelectLayerObjectToolStripMenuItem selectLayerObject =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is disabled
            Assert.That(!selectLayerObject.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="SelectLayerObjectToolStripMenuItem"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SelectLayerObjectToolStripMenuItemEnabledWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SelectLayerObject menu item
            SelectLayerObjectToolStripMenuItem selectLayerObject =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripMenuItem>(_imageViewerForm);

            // Check that the menu item is enabled
            Assert.That(selectLayerObject.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="SelectLayerObjectToolStripMenuItem"/> is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SelectLayerObjectToolStripMenuItemToggledOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SelectLayerObject menu item
            SelectLayerObjectToolStripMenuItem selectLayerObject =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripMenuItem>(_imageViewerForm);

            // Select the tool
            selectLayerObject.PerformClick();

            // Check that the menu item is checked
            Assert.That(selectLayerObject.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="SelectLayerObjectToolStripMenuItem"/> is toggled off 
        /// when different cursor tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SelectLayerObjectToolStripMenuItemToggledOffWithDifferentSelectionTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SelectLayerObject menu item
            SelectLayerObjectToolStripMenuItem selectLayerObject =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripMenuItem>(_imageViewerForm);

            // Select the tool
            selectLayerObject.PerformClick();

            // Get the ZoomWindow menu item
            ZoomWindowToolStripMenuItem zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripMenuItem>(_imageViewerForm);

            // Select the Zoom Window tool
            zoomWindow.PerformClick();

            // Check that the SelectLayerObject menu item is unchecked
            Assert.That(!selectLayerObject.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="SelectLayerObjectToolStripMenuItem"/> sets the CursorTool 
        /// property when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SelectLayerObjectToolStripMenuItemSetsCursorPropertyWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SelectLayerObject menu item
            SelectLayerObjectToolStripMenuItem selectLayerObject =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripMenuItem>(_imageViewerForm);

            // Select the tool
            selectLayerObject.PerformClick();

            // Check that the CursorTool property has been set
            Assert.That(imageViewer.CursorTool == CursorTool.SelectLayerObject);
        }

        /// <summary>
        /// Tests whether <see cref="SelectLayerObjectToolStripMenuItem"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SelectHighlightToolStripMenuItemEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of CursorToolChanged events
            imageViewer.CursorToolChanged += eventCounters.CountEvent<CursorToolChangedEventArgs>;

            // Click the SelectLayerObjectToolStripMenuItem
            SelectLayerObjectToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="SelectLayerObjectToolStripMenuItem"/>
        /// selecting and resizing a highlight raises the
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_SelectHighlightToolStripMenuItemResizeAngularHighlightRaisesLayerObjectChangedEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(220, 200), new Point(400, 300), 120, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Select the highlight
            imageViewer.LayerObjects.SelectAll();

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of LayerObjectChanged events
            imageViewer.LayerObjects.LayerObjectChanged += eventCounters.CountEvent<LayerObjectChangedEventArgs>;

            // Click the SelectLayerObjectToolStripMenuItem
            SelectLayerObjectToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Click and drag one of the handles on the highlight to resize it.",
                "Click okay to close this dialog and end this test."};

            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that exactly one LayerObjectChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="SelectLayerObjectToolStripMenuItem"/>
        /// selecting and resizing a highlight raises the
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_SelectHighlightToolStripMenuItemResizeRectangularHighlightSideRaisesLayerObjectChangedEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(220, 200), new Point(400, 200), 120, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Select the highlight
            imageViewer.LayerObjects.SelectAll();

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of LayerObjectChanged events
            imageViewer.LayerObjects.LayerObjectChanged += eventCounters.CountEvent<LayerObjectChangedEventArgs>;

            // Click the SelectLayerObjectToolStripMenuItem
            SelectLayerObjectToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Click and drag one of the side handles on the highlight to resize it.",
                "Click okay to close this dialog and end this test."};

            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that exactly one LayerObjectChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="SelectLayerObjectToolStripMenuItem"/>
        /// selecting and resizing a highlight raises the
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_SelectHighlightToolStripMenuItemResizeRectangularHighlightCornerRaisesLayerObjectChangedEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight =
                new Highlight(imageViewer, "Test", new Point(220, 200), new Point(400, 200), 120, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Select the highlight
            imageViewer.LayerObjects.SelectAll();

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of LayerObjectChanged events
            imageViewer.LayerObjects.LayerObjectChanged += eventCounters.CountEvent<LayerObjectChangedEventArgs>;

            // Click the SelectLayerObjectToolStripMenuItem
            SelectLayerObjectToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Click and drag one of the corner handles on the highlight to resize it.",
                "Click okay to close this dialog and end this test."};

            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that exactly one LayerObjectChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="SelectLayerObjectToolStripMenuItem"/>
        /// selecting and changing a highlights angle raises the
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_SelectHighlightToolStripMenuItemChangeHighlightAngleRaisesLayerObjectChangedEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(220, 200), new Point(400, 300), 120, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Select the highlight
            imageViewer.LayerObjects.SelectAll();

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of LayerObjectChanged events
            imageViewer.LayerObjects.LayerObjectChanged += eventCounters.CountEvent<LayerObjectChangedEventArgs>;

            // Click the SelectLayerObjectToolStripMenuItem
            SelectLayerObjectToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Press and hold the Ctrl key.",
                "Click and drag one of the side handles on the highlight to change its angle.",
                "Click okay to close this dialog and end this test."};

            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that exactly one LayerObjectChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="SelectLayerObjectToolStripMenuItem"/>
        /// selecting and resizing a highlight out of the image area does not raises the
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_SelectHighlightToolStripMenuItemResizeHighlightOutsideImageAreaDoesNotRaisesLayerObjectChangedEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight =
                new Highlight(imageViewer, "Test", new Point(imageViewer.ImageWidth - 400, 400),
                    new Point(imageViewer.ImageWidth, 400), 400, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Select the highlight
            imageViewer.LayerObjects.SelectAll();

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of LayerObjectChanged events
            imageViewer.LayerObjects.LayerObjectChanged += eventCounters.CountEvent<LayerObjectChangedEventArgs>;

            // Click the SelectLayerObjectToolStripMenuItem
            SelectLayerObjectToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Click on the left side handle of the highlight and drag it outside the image area.",
                "Click okay to close this dialog and end this test."};

            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that no LayerObjectChanged event was raised
            Assert.That(eventCounters.EventCounter == 0);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="SelectLayerObjectToolStripMenuItem"/>
        /// selecting and attempting to move a highlight outside of the image area
        /// doea not raise the <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_SelectHighlightToolStripMenuItemMoveHighlightOutOfImageAreaDoesNotRaisesLayerObjectChangedEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(220, 200), new Point(400, 200), 120, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Select the highlight
            imageViewer.LayerObjects.SelectAll();

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of LayerObjectChanged events
            imageViewer.LayerObjects.LayerObjectChanged += eventCounters.CountEvent<LayerObjectChangedEventArgs>;

            // Click the SelectLayerObjectToolStripMenuItem
            SelectLayerObjectToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Click and drag the highlight to move it out of the image area.",
                "Click okay to close this dialog and end this test."};

            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that exactly one LayerObjectChanged event was raised
            Assert.That(eventCounters.EventCounter == 0);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="SelectLayerObjectToolStripMenuItem"/>
        /// selecting and resizing a highlight out of the image area does not raises the
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_SelectHighlightToolStripMenuItemChangeHighlightAngleOutsideImageAreaDoesNotRaisesLayerObjectChangedEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add a highlight to page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(imageViewer.ImageWidth - 400, 400), 
                    new Point(imageViewer.ImageWidth, 400), 400, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Select the highlight
            imageViewer.LayerObjects.SelectAll();

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of LayerObjectChanged events
            imageViewer.LayerObjects.LayerObjectChanged += eventCounters.CountEvent<LayerObjectChangedEventArgs>;

            // Click the SelectLayerObjectToolStripMenuItem
            SelectLayerObjectToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripMenuItem>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Press and hold the Ctrl key.",
                "Click on the left side handle of the highlight and rotate it outside the image area.",
                "Click okay to close this dialog and end this test."};

            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that no LayerObjectChanged event was raised
            Assert.That(eventCounters.EventCounter == 0);
        }

        #endregion Select Highlight
    }
}
