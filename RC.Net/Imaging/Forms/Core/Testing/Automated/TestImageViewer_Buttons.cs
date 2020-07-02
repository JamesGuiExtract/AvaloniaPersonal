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

namespace Extract.Imaging.Forms.Test
{
    public partial class TestImageViewer
    {
        #region OpenImage

        /// <summary>
        /// Test the <see cref="OpenImageToolStripSplitButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_OpenImageEnabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the open image button
            OpenImageToolStripSplitButton openImage = 
                FormMethods.GetFormComponent<OpenImageToolStripSplitButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(openImage.Enabled);
        }

        /// <summary>
        /// Test the <see cref="OpenImageToolStripSplitButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_OpenImageEnabledWithImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the open image button
            OpenImageToolStripSplitButton openImage = 
                FormMethods.GetFormComponent<OpenImageToolStripSplitButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(openImage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="OpenImageToolStripSplitButton"/> raises the
        /// <see cref="ImageViewer.ImageFileChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_OpenImageToolStripSplitButtonEventTest()
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

            // Click the OpenImageToolStripSplitButton
            OpenImageToolStripSplitButton clickMe =
                FormMethods.GetFormComponent<OpenImageToolStripSplitButton>(_imageViewerForm);
            clickMe.PerformButtonClick();

            // Check that exactly one ImageFileChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion

        #region PrintImage

        /// <summary>
        /// Test the <see cref="PrintImageToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PrintImageDisabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the print image button
            PrintImageToolStripButton printImage = FormMethods.GetFormComponent<PrintImageToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!printImage.Enabled);
        }

        /// <summary>
        /// Test the <see cref="PrintImageToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PrintImageEnabledWithImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the print image button
            PrintImageToolStripButton printImage = FormMethods.GetFormComponent<PrintImageToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(printImage.Enabled);
        }

        #endregion

        #region FirstPage

        /// <summary>
        /// Test the <see cref="FirstPageToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FirstPageDisabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the first page button
            FirstPageToolStripButton firstPage = FormMethods.GetFormComponent<FirstPageToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!firstPage.Enabled);
        }

        /// <summary>
        /// Test the <see cref="FirstPageToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FirstPageDisabledWithImageOnFirstPageTest()
        {
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the first page button
            FirstPageToolStripButton firstPage = FormMethods.GetFormComponent<FirstPageToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!firstPage.Enabled);
        }

        /// <summary>
        /// Test the <see cref="FirstPageToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FirstPageEnabledWithImageNotOnFirstPageTest()
        {
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Move to page 2
            imageViewer.PageNumber = 2;

            // Get the first page button
            FirstPageToolStripButton firstPage = FormMethods.GetFormComponent<FirstPageToolStripButton>(_imageViewerForm);

            ExtractException.Assert("ELI21805",
                "Could not find First page button in navigation commands toolstrip!",
                firstPage != null);

            // Check that the button is disabled
            Assert.That(firstPage.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="FirstPageToolStripButton"/> navigates 
        /// to the first page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FirstPageToolStripButtonNavigationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Move to page 2
            imageViewer.PageNumber = 2;

            // Get the first page button
            FirstPageToolStripButton firstPage = FormMethods.GetFormComponent<FirstPageToolStripButton>(_imageViewerForm);

            ExtractException.Assert("ELI22234",
                "Could not find First page button in navigation commands toolstrip!",
                firstPage != null);

            // Go to the first page
            firstPage.PerformClick();

            // Check that the first page is active
            Assert.That(imageViewer.PageNumber == 1);
        }

        /// <summary>
        /// Tests whether <see cref="FirstPageToolStripButton"/> raises the
        /// <see cref="ImageViewer.PageChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FirstPageToolStripButtonEventTest()
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

            // Click the FirstPageToolStripButton
            FirstPageToolStripButton clickMe =
                FormMethods.GetFormComponent<FirstPageToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion

        #region Previous Page

        /// <summary>
        /// Tests whether <see cref="PreviousPageToolStripButton"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousPageToolStripButtonDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the Previous Page button
            PreviousPageToolStripButton previousPage =
                FormMethods.GetFormComponent<PreviousPageToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!previousPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="PreviousPageToolStripButton"/> is disabled 
        /// on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousPageToolStripButtonDisabledOnFirstPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Previous Page button
            PreviousPageToolStripButton previousPage =
                FormMethods.GetFormComponent<PreviousPageToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!previousPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="PreviousPageToolStripButton"/> is enabled 
        /// when not on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousPageToolStripButtonEnabledNotOnFirstPageTest()
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

            // Get the Previous Page button
            PreviousPageToolStripButton previousPage =
                FormMethods.GetFormComponent<PreviousPageToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(previousPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="PreviousPageToolStripButton"/> navigates 
        /// to the previous page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousPageToolStripButtonNavigationTest()
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

            // Get the Previous Page button
            PreviousPageToolStripButton previousPage =
                FormMethods.GetFormComponent<PreviousPageToolStripButton>(_imageViewerForm);

            // Go to the previous page
            previousPage.PerformClick();

            // Check that the previous page is active
            Assert.That(imageViewer.PageNumber == imageViewer.PageCount - 1);
        }

        /// <summary>
        /// Tests whether <see cref="PreviousPageToolStripButton"/> raises the
        /// <see cref="ImageViewer.PageChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousPageToolStripButtonEventTest()
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

            // Click the PreviousPageToolStripButton
            PreviousPageToolStripButton clickMe =
                FormMethods.GetFormComponent<PreviousPageToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion Previous Page

        #region Next Page

        /// <summary>
        /// Tests whether <see cref="NextPageToolStripButton"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextPageToolStripButtonDisabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the Next Page button
            NextPageToolStripButton nextPage =
                FormMethods.GetFormComponent<NextPageToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!nextPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="NextPageToolStripButton"/> is disabled 
        /// on the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextPageToolStripButtonDisabledOnLastPageTest()
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

            // Get the Next Page button
            NextPageToolStripButton nextPage =
                FormMethods.GetFormComponent<NextPageToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!nextPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="NextPageToolStripButton"/> is enabled 
        /// when not on the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextPageToolStripButtonEnabledNotOnLastPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Next Page button
            NextPageToolStripButton nextPage =
                FormMethods.GetFormComponent<NextPageToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(nextPage.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="NextPageToolStripButton"/> navigates 
        /// to the next page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextPageToolStripButtonNavigationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Next Page button
            NextPageToolStripButton nextPage =
                FormMethods.GetFormComponent<NextPageToolStripButton>(_imageViewerForm);

            // Go to the next page
            nextPage.PerformClick();

            // Check that the next page is active
            Assert.That(imageViewer.PageNumber == 2);
        }

        /// <summary>
        /// Tests whether <see cref="NextPageToolStripButton"/> raises the
        /// <see cref="ImageViewer.PageChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextPageToolStripButtonEventTest()
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

            // Click the NextPageToolStripButton
            NextPageToolStripButton clickMe =
                FormMethods.GetFormComponent<NextPageToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion Next Page

        #region LastPage

        /// <summary>
        /// Test the <see cref="LastPageToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_LastPageDisabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the last page button
            LastPageToolStripButton lastPage = FormMethods.GetFormComponent<LastPageToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!lastPage.Enabled);
        }

        /// <summary>
        /// Test the <see cref="LastPageToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_LastPageDisabledWithImageOnLastPageTest()
        {
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Move to last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Get the last page button
            LastPageToolStripButton lastPage = FormMethods.GetFormComponent<LastPageToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!lastPage.Enabled);
        }

        /// <summary>
        /// Test the <see cref="LastPageToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_LastPageEnabledWithImageNotOnLastPageTest()
        {
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the last page button
            LastPageToolStripButton lastPage = FormMethods.GetFormComponent<LastPageToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(lastPage.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="LastPageToolStripButton"/> navigates to 
        /// the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_LastPageNavigationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the last page button
            LastPageToolStripButton lastPage = FormMethods.GetFormComponent<LastPageToolStripButton>(_imageViewerForm);

            // Go to the last page
            lastPage.PerformClick();

            // Check that the last page is active
            Assert.That(imageViewer.PageNumber == imageViewer.PageCount);
        }

        /// <summary>
        /// Tests whether <see cref="LastPageToolStripButton"/> raises the
        /// <see cref="ImageViewer.PageChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_LastPageToolStripButtonEventTest()
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

            // Click the LastPageToolStripButton
            LastPageToolStripButton clickMe =
                FormMethods.GetFormComponent<LastPageToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion

        #region Page Navigation Text Box

        /// <summary>
        /// Test that the <see cref="PageNavigationToolStripTextBox"/> is 
        /// disabled without an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToPageDisabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the Goto Page text box
            PageNavigationToolStripTextBox gotoPage = 
                FormMethods.GetFormComponent<PageNavigationToolStripTextBox>(_imageViewerForm);

            // Check that the text box is disabled
            Assert.That(!gotoPage.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="PageNavigationToolStripTextBox"/> is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToPageEnabledWithOpenImageTest()
        {
            _imageViewerForm.Show();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Goto Page text box
            PageNavigationToolStripTextBox gotoPage =
                FormMethods.GetFormComponent<PageNavigationToolStripTextBox>(_imageViewerForm);

            // Check that the text box is enabled
            Assert.That(gotoPage.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="PageNavigationToolStripTextBox"/> navigates to the 
        /// specified page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PageNavigationToolStripTextBoxDisplayTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Type a page number into the PageNavigationToolStripTextBox
            PageNavigationToolStripTextBox textBox =
                FormMethods.GetFormComponent<PageNavigationToolStripTextBox>(_imageViewerForm);
            textBox.Focus();
            textBox.Text = "3";

            // Leave focus of the text box
            imageViewer.Focus();

            // Check that text box contents are as expected
            Assert.That(textBox.Text == "3 of 4");
        }

        /// <summary>
        /// Tests whether <see cref="PageNavigationToolStripTextBox"/> raises the
        /// <see cref="ImageViewer.PageChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PageNavigationToolStripTextBoxEventTest()
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

            // Type a page number into the PageNavigationToolStripTextBox
            PageNavigationToolStripTextBox textBox = 
                FormMethods.GetFormComponent<PageNavigationToolStripTextBox>(_imageViewerForm);
            textBox.Focus();
            textBox.Text = "3";

            // Leave focus of the text box
            imageViewer.Focus();

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether <see cref="PageNavigationToolStripTextBox"/> navigates to
        /// the specified page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PageNavigationToolStripTextBoxNavigatesToPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Ensure we are on page 1
            imageViewer.PageNumber = 1;

            // Type a page number into the PageNavigationToolStripTextBox
            PageNavigationToolStripTextBox textBox = 
                FormMethods.GetFormComponent<PageNavigationToolStripTextBox>(_imageViewerForm);
            textBox.Focus();
            textBox.Text = "3";

            // Leave focus of the text box
            imageViewer.Focus();

            // Check that the current page is page 3
            Assert.That(imageViewer.PageNumber == 3);
        }

        /// <summary>
        /// Tests that the <see cref="PageNavigationToolStripTextBox"/> does not 
        /// throw an exception if an invalid page is specified.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PageNavigationToolStripTextBoxNoExceptionTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Ensure we are on page 1
            imageViewer.PageNumber = 1;

            // Type an invalid page number into the PageNavigationToolStripTextBox
            PageNavigationToolStripTextBox textBox =
                FormMethods.GetFormComponent<PageNavigationToolStripTextBox>(_imageViewerForm);
            textBox.Focus();
            textBox.Text = "33";

            // Leave focus of the text box
            imageViewer.Focus();

            // No exception means the test passes
        }

        #endregion Page Navigation Text Box

        #region FitToPage

        /// <summary>
        /// Test the <see cref="FitToPageToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripButtonEnabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the fit to page button
            FitToPageToolStripButton fitToPage = FormMethods.GetFormComponent<FitToPageToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(fitToPage.Enabled);
        }

        /// <summary>
        /// Test the <see cref="FitToPageToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripButtonEnabledWithImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the fit to page button
            FitToPageToolStripButton fitToPage = FormMethods.GetFormComponent<FitToPageToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(fitToPage.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="FitToPageToolStripButton"/> works 
        /// without an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripButtonNoImageTest()
        {
            // Show image viewer so the menu items enabled state can be checked
            _imageViewerForm.Show();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Set the fit mode to none
            imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI22537", "Could not change fit mode to none!",
                imageViewer.FitMode == FitMode.None);

            // Get the fit to page menu item
            FitToPageToolStripButton fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_imageViewerForm);

            // Click the button
            fitToPage.PerformClick();

            Assert.That(imageViewer.FitMode == FitMode.FitToPage);
        }

        /// <summary>
        /// Test that the <see cref="FitToPageToolStripButton"/> works with 
        /// an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripButtonWithImageTest()
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
            ExtractException.Assert("ELI22538", "Could not change fit mode to none!",
                imageViewer.FitMode == FitMode.None);

            // Get the fit to page button
            FitToPageToolStripButton fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_imageViewerForm);

            // Click the button
            fitToPage.PerformClick();

            Assert.That(imageViewer.FitMode == FitMode.FitToPage);
        }

        /// <summary>
        /// Tests whether <see cref="FitToPageToolStripButton"/> is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripButtonToggledOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the FitToWidth button
            FitToWidthToolStripButton fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_imageViewerForm);

            // Select the FitToWidth tool
            fitToWidth.PerformClick();

            // Get the FitToPage button
            FitToPageToolStripButton fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_imageViewerForm);

            // Select the FitToPage tool
            fitToPage.PerformClick();

            // Check that the button is checked
            Assert.That(fitToPage.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="FitToPageToolStripButton"/> is toggled off 
        /// when selected again.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripButtonTogglesOffWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the FitToWidth button
            FitToWidthToolStripButton fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_imageViewerForm);

            // Select the FitToWidth tool
            fitToWidth.PerformClick();

            // Get the FitToPage button
            FitToPageToolStripButton fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_imageViewerForm);

            // Select the FitToPage tool twice
            fitToPage.PerformClick();
            fitToPage.PerformClick();

            // Check that the button is unchecked
            Assert.That(!fitToPage.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="FitToPageToolStripButton"/> raises the
        /// <see cref="ImageViewer.FitModeChanged"/> and <see cref="ImageViewer.ZoomChanged"/> 
        /// events.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripButtonEventTest()
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

            // Click the FitToPageToolStripButton
            FitToPageToolStripButton clickMe =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one FitModeChanged and ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter == 1 && eventCounters.EventCounter2 == 1);

            // Click the FitToPageToolStripButton again
            clickMe.PerformClick();

            // Check that exactly two FitModeChanged and two ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter == 2 && eventCounters.EventCounter2 == 2);
        }

        #endregion

        #region FitToWidth

        /// <summary>
        /// Test the <see cref="FitToWidthToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripButtonEnabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the fit to width button
            FitToWidthToolStripButton fitToWidth = FormMethods.GetFormComponent<FitToWidthToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(fitToWidth.Enabled);
        }

        /// <summary>
        /// Test the <see cref="FitToWidthToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripButtonEnabledWithImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the fit to width button
            FitToWidthToolStripButton fitToWidth = FormMethods.GetFormComponent<FitToWidthToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(fitToWidth.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="FitToWidthToolStripButton"/> works 
        /// without an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripButtonNoImageTest()
        {
            // Show image viewer so the menu items enabled state can be checked
            _imageViewerForm.Show();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Set the fit mode to none
            imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI22534", "Could not change fit mode to none!",
                imageViewer.FitMode == FitMode.None);

            // Get the fit to width menu item
            FitToWidthToolStripButton fitToWidth = 
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_imageViewerForm);

            // Click the button
            fitToWidth.PerformClick();

            Assert.That(imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Test that the <see cref="FitToWidthToolStripButton"/> works with 
        /// an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripButtonWithImageTest()
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
            ExtractException.Assert("ELI22535", "Could not change fit mode to none!",
                imageViewer.FitMode == FitMode.None);

            // Get the fit to width button
            FitToWidthToolStripButton fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_imageViewerForm);

            // Click the button
            fitToWidth.PerformClick();

            Assert.That(imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Tests whether <see cref="FitToWidthToolStripButton"/> is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripButtonToggledOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the FitToPage button
            FitToPageToolStripButton fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_imageViewerForm);

            // Select the FitToPage tool
            fitToPage.PerformClick();

            // Get the FitToWidth button
            FitToWidthToolStripButton fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_imageViewerForm);

            // Select the FitToWidth tool
            fitToWidth.PerformClick();

            // Check that the button is checked
            Assert.That(fitToWidth.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="FitToWidthToolStripButton"/> is toggled off 
        /// when selected again.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripButtonTogglesOffWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the FitToPage button
            FitToPageToolStripButton fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_imageViewerForm);

            // Select the FitToPage tool
            fitToPage.PerformClick();

            // Get the FitToWidth button
            FitToWidthToolStripButton fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_imageViewerForm);

            // Select the FitToWidth tool twice
            fitToWidth.PerformClick();
            fitToWidth.PerformClick();

            // Check that the button is unchecked
            Assert.That(!fitToWidth.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="FitToWidthToolStripButton"/> raises the
        /// <see cref="ImageViewer.FitModeChanged"/> and <see cref="ImageViewer.ZoomChanged"/> 
        /// events.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripButtonEventTest()
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

            // Click the FitToWidthToolStripButton
            FitToWidthToolStripButton clickMe =
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one FitModeChanged and one or more ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter == 1 && eventCounters.EventCounter2 >= 1);
            var previousZoomEventCount = eventCounters.EventCounter2;

            // Click the FitToWidthToolStripButton again
            clickMe.PerformClick();

            // Check that exactly two FitModeChanged and more ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter == 2 && eventCounters.EventCounter2 > previousZoomEventCount);
        }

        #endregion

        #region ZoomIn

        /// <summary>
        /// Test the <see cref="ZoomInToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripButtonDisabledWithNoImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Get the zoom in button
            ZoomInToolStripButton zoomIn = FormMethods.GetFormComponent<ZoomInToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!zoomIn.Enabled);
        }

        /// <summary>
        /// Test the <see cref="ZoomInToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripButtonEnabledWithImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the zoom in button
            ZoomInToolStripButton zoomIn = FormMethods.GetFormComponent<ZoomInToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(zoomIn.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="ZoomInToolStripButton"/> zooms in.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripButtonZoomsWithImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the zoom in button
            ZoomInToolStripButton zoomIn = FormMethods.GetFormComponent<ZoomInToolStripButton>(_imageViewerForm);

            // Get the zoom level
            double zoomLevel = imageViewer.ZoomInfo.ScaleFactor;

            // Zoom in
            zoomIn.PerformClick();

            // Check that the image zoomed in
            Assert.That(imageViewer.ZoomInfo.ScaleFactor > zoomLevel);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomInToolStripButton"/> raises the
        /// <see cref="ImageViewer.ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripButtonEventTest()
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

            // Click the ZoomInToolStripButton
            ZoomInToolStripButton clickMe =
                FormMethods.GetFormComponent<ZoomInToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that one or more ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter >= 1);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomInToolStripButton"/> adds a zoom history entry.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripButtonAddsZoomHistoryTest()
        {
            // Show the image viewer
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the current Zoom history count
            int zoomHistoryCount = imageViewer.ZoomHistoryCount;

            // Click the ZoomInToolStripButton
            ZoomInToolStripButton clickMe = FormMethods.GetFormComponent<ZoomInToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one zoom history record has been added
            Assert.That((zoomHistoryCount + 1) == imageViewer.ZoomHistoryCount);
        }

        #endregion

        #region ZoomOut

        /// <summary>
        /// Test the <see cref="ZoomOutToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripButtonDisabledWithNoImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Get the zoom out button
            ZoomOutToolStripButton zoomOut = FormMethods.GetFormComponent<ZoomOutToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!zoomOut.Enabled);
        }

        /// <summary>
        /// Test the <see cref="ZoomOutToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripButtonEnabledWithImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the zoom out button
            ZoomOutToolStripButton zoomOut = FormMethods.GetFormComponent<ZoomOutToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(zoomOut.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="ZoomOutToolStripButton"/> zooms out.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripButtonZoomsWithImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the zoom out button
            ZoomOutToolStripButton zoomOut = FormMethods.GetFormComponent<ZoomOutToolStripButton>(_imageViewerForm);

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

            // Check that the image zoomed out
            Assert.That(imageViewer.ZoomInfo.ScaleFactor < zoomLevel);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomOutToolStripButton"/> raises the
        /// <see cref="ImageViewer.ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripButtonEventTest()
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

            // Click the ZoomOutToolStripButton
            ZoomOutToolStripButton clickMe =
                FormMethods.GetFormComponent<ZoomOutToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // The zoom code fires two events for this action, one for centering around the users cursor,
            // and the actual zoom out. Since events are fired based on any change
            // the safest way to test this is to ensure more than one event fired.
            Assert.That(eventCounters.EventCounter >= 1);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomOutToolStripButton"/> adds a zoom history entry.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripButtonAddsZoomHistoryTest()
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

            // Click the ZoomOutToolStripButton
            ZoomOutToolStripButton clickMe = FormMethods.GetFormComponent<ZoomOutToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one zoom history record has been added
            Assert.That((zoomHistoryCount + 1) == imageViewer.ZoomHistoryCount);
        }

        #endregion

        #region ZoomPrevious

        /// <summary>
        /// Test the <see cref="ZoomPreviousToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousDisabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the zoom previous button
            ZoomPreviousToolStripButton zoomPrevious = 
                FormMethods.GetFormComponent<ZoomPreviousToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!zoomPrevious.Enabled);
        }

        /// <summary>
        /// Test the <see cref="ZoomPreviousToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousDisabledWithImageAndNoZoomHistoryTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the zoom previous button
            ZoomPreviousToolStripButton zoomPrevious = 
                FormMethods.GetFormComponent<ZoomPreviousToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!zoomPrevious.Enabled);
        }

        /// <summary>
        /// Test the <see cref="ZoomPreviousToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousEnabledWithImageAndZoomHistoryTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the zoom previous button
            ZoomPreviousToolStripButton zoomPrevious = 
                FormMethods.GetFormComponent<ZoomPreviousToolStripButton>(_imageViewerForm);

            // Add a zoom history record
            imageViewer.ZoomIn();

            ExtractException.Assert("ELI21824",
                "Cannot zoom previous!", imageViewer.CanZoomPrevious);

            // Check that the button is enabled
            Assert.That(zoomPrevious.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomPreviousToolStripButton"/> zooms to 
        /// a previous zoom item.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousButtonZoomsToPreviousTest()
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

            // Get the Zoom Previous button
            ZoomPreviousToolStripButton zoomPrevious =
                FormMethods.GetFormComponent<ZoomPreviousToolStripButton>(_imageViewerForm);

            // Zoom to previous history item
            zoomPrevious.PerformClick();

            // Check that the zoom level changed back to original AND
            // that the original zoom level is different than new zoom level
            Assert.That(imageViewer.ZoomInfo.ScaleFactor == zoomLevelOriginal &&
                zoomLevelOriginal != zoomLevelNew);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomPreviousToolStripButton"/> raises the
        /// <see cref="ImageViewer.ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousToolStripButtonEventTest()
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

            // Click the ZoomPreviousToolStripButton
            ZoomPreviousToolStripButton clickMe =
                FormMethods.GetFormComponent<ZoomPreviousToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // The zoom code fires two events for this action, one for centering around the users cursor,
            // and the actual zoom previous. Since events are fired based on any change
            // the safest way to test this is to ensure more than one event fired.
            Assert.That(eventCounters.EventCounter >= 1);
        }

        #endregion

        #region ZoomNext

        /// <summary>
        /// Test the <see cref="ZoomNextToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomNextDisabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the zoom next button
            ZoomNextToolStripButton zoomNext = FormMethods.GetFormComponent<ZoomNextToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!zoomNext.Enabled);
        }

        /// <summary>
        /// Test the <see cref="ZoomNextToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomNextDisabledWithImageAndNoZoomHistoryTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the zoom next button
            ZoomNextToolStripButton zoomNext = FormMethods.GetFormComponent<ZoomNextToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!zoomNext.Enabled);
        }

        /// <summary>
        /// Test the <see cref="ZoomNextToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomNextEnabledWithImageAndCanZoomNextTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the zoom next button
            ZoomNextToolStripButton zoomNext = FormMethods.GetFormComponent<ZoomNextToolStripButton>(_imageViewerForm);

            // Add a zoom history record and then zoom previous
            // which should set CanZoomNext to true
            imageViewer.ZoomIn();
            imageViewer.ZoomPrevious();
            ExtractException.Assert("ELI21823",
                "Cannot zoom next!", imageViewer.CanZoomNext);

            // Check that the button is enabled
            Assert.That(zoomNext.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomNextToolStripButton"/> zooms to 
        /// the next history entry.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomNextToolStripButtonZoomsNextTest()
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

            // Get the Zoom Next button
            ZoomNextToolStripButton zoomNext =
                FormMethods.GetFormComponent<ZoomNextToolStripButton>(_imageViewerForm);

            // Go to the next zoom history entry
            zoomNext.PerformClick();

            // Check that the zoom level changed back to the zoomed in value AND
            // that the previous zoom level is different than new zoom level
            Assert.That(imageViewer.ZoomInfo.ScaleFactor == zoomLevelIn &&
                zoomLevelIn != zoomLevelPrevious);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomNextToolStripButton"/> raises the
        /// <see cref="ImageViewer.ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomNextToolStripButtonEventTest()
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

            // Click the ZoomNextToolStripButton
            ZoomNextToolStripButton clickMe =
                FormMethods.GetFormComponent<ZoomNextToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that one or more ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter >= 1);
        }

        #endregion

        #region Previous Tile

        /// <summary>
        /// Tests the enabled state of the <see cref="PreviousTileToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousTileToolStripButtonEnableDisableTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Check that the previous tile toolstrip button is disabled without an image open
            PreviousTileToolStripButton PreviousTile = 
                FormMethods.GetFormComponent<PreviousTileToolStripButton>(_imageViewerForm);
            Assert.That(!PreviousTile.Enabled);

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Check that the previous tile button is disabled now that the image is open
            Assert.That(!PreviousTile.Enabled);

            // Go to the next tile
            imageViewer.SelectNextTile();

            // Check that the Previous tile button is enabled
            Assert.That(PreviousTile.Enabled);
        }

        /// <summary>
        /// Tests whether the <see cref="PreviousTileToolStripButton"/> updates the zoom history 
        /// properly.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousTileToolStripButtonZoomHistoryTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Go to the third page
            imageViewer.PageNumber = 3;

            // Go to the previous tile
            PreviousTileToolStripButton previousTile = FormMethods.GetFormComponent<PreviousTileToolStripButton>(_imageViewerForm);
            imageViewer.PerformClick(previousTile);

            // Ensure that there is only one zoom history entry for this page
            Assert.That(!imageViewer.CanZoomPrevious && !imageViewer.CanZoomNext);
        }

        /// <summary>
        /// Tests whether the <see cref="PreviousTileToolStripButton"/> preserves the fit mode.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousTileToolStripButtonPreserveFitModeTest()
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
            PreviousTileToolStripButton previousTile = FormMethods.GetFormComponent<PreviousTileToolStripButton>(_imageViewerForm);
            imageViewer.PerformClick(previousTile);

            // This should be the previous page
            Assert.That(imageViewer.PageNumber == 3);

            // Ensure that the fit mode was preserved
            Assert.That(imageViewer.FitMode == FitMode.FitToPage);

            // Set the fit mode to fit to width
            imageViewer.FitMode = FitMode.FitToWidth;

            // Go to the previous tile
            imageViewer.PerformClick(previousTile);

            // This should be the second page
            Assert.That(imageViewer.PageNumber == 2);

            // Ensure that the fit mode was preserved
            Assert.That(imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Tests whether the <see cref="PreviousTileToolStripButton"/> raises events properly.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousTileToolStripButtonEventsTest()
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
            PreviousTileToolStripButton previousTile = 
                FormMethods.GetFormComponent<PreviousTileToolStripButton>(_imageViewerForm);
            imageViewer.PerformClick(previousTile);

            // Ensure that ZoomChanged and PageChanged event were raised
            Assert.That(eventCounters.EventCounter >= 1 && eventCounters.EventCounter2 == 1);
            var previousZoomEventCount = eventCounters.EventCounter;

            // Go to the previous tile
            imageViewer.PerformClick(previousTile);

            // Ensure that more ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter > previousZoomEventCount && eventCounters.EventCounter2 == 1);
        }

        /// <summary>
        /// Tests whether the <see cref="PreviousTileToolStripButton"/> properly updates the 
        /// enabled state during rotation. [DotNetRCAndUtils #127]
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PreviousTileToolStripButtonEnableRotationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Set the fit mode to fit to width
            imageViewer.FitMode = FitMode.FitToWidth;

            // The previous tile button should be disabled
            PreviousTileToolStripButton previousTile =
                FormMethods.GetFormComponent<PreviousTileToolStripButton>(_imageViewerForm);
            Assert.That(!previousTile.Enabled);

            // Rotate the image 90 degrees
            imageViewer.Rotate(90, true, true);

            // The previous tile button should be enabled
            Assert.That(previousTile.Enabled);
        }

        #endregion Previous Tile

        #region Next Tile

        /// <summary>
        /// Tests the enabled state of the <see cref="NextTileToolStripButton"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextTileToolStripButtonEnableDisableTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Check that the next tile toolstrip button is disabled without an image open
            NextTileToolStripButton nextTile = FormMethods.GetFormComponent<NextTileToolStripButton>(_imageViewerForm);
            Assert.That(!nextTile.Enabled);

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Check that the next tile button is enabled now that the image is open
            Assert.That(nextTile.Enabled);

            // Go to the last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Check that the next tile button is disabled
            Assert.That(!nextTile.Enabled);
        }

        /// <summary>
        /// Tests whether the <see cref="NextTileToolStripButton"/> updates the zoom history 
        /// properly.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextTileToolStripButtonZoomHistoryTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Go to the next tile
            NextTileToolStripButton nextTile = FormMethods.GetFormComponent<NextTileToolStripButton>(_imageViewerForm);
            imageViewer.PerformClick(nextTile);

            // Ensure that there is only one zoom history entry for this page
            Assert.That(!imageViewer.CanZoomPrevious && !imageViewer.CanZoomNext);
        }

        /// <summary>
        /// Tests whether the <see cref="NextTileToolStripButton"/> preserves the fit mode.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextTileToolStripButtonPreserveFitModeTest()
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
            NextTileToolStripButton nextTile = FormMethods.GetFormComponent<NextTileToolStripButton>(_imageViewerForm);
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

            // This should be the third page
            Assert.That(imageViewer.PageNumber == 3);

            // Ensure that the fit mode was preserved
            Assert.That(imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Tests whether the <see cref="NextTileToolStripButton"/> raises events properly.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextTileToolStripButtonEventsTest()
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
            NextTileToolStripButton nextTile = FormMethods.GetFormComponent<NextTileToolStripButton>(_imageViewerForm);
            imageViewer.PerformClick(nextTile);

            // Ensure that one or more ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter >= 1 && eventCounters.EventCounter2 == 0);
            var previousZoomEventCount = eventCounters.EventCounter;

            // Go to the next tile
            imageViewer.PerformClick(nextTile);

            // Ensure that one or more ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter > previousZoomEventCount && eventCounters.EventCounter2 == 0);
            previousZoomEventCount = eventCounters.EventCounter;

            // Go to the next tile
            imageViewer.PerformClick(nextTile);

            // Ensure that one or more ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter > previousZoomEventCount && eventCounters.EventCounter2 == 0);
            previousZoomEventCount = eventCounters.EventCounter;

            // Go to the next tile
            imageViewer.PerformClick(nextTile);

            // Ensure that one or more ZoomChanged events were raised and one more PageChanged event was 
            // raised.
            Assert.That(eventCounters.EventCounter > previousZoomEventCount && eventCounters.EventCounter2 == 1);
        }

        /// <summary>
        /// Tests whether the <see cref="NextTileToolStripButton"/> properly updates the 
        /// enabled state during rotation. [DotNetRCAndUtils #127]
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NextTileToolStripButtonEnableRotationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Fit the top of the page into view
            imageViewer.FitMode = FitMode.FitToWidth;

            // Go to the last tile
            imageViewer.GoToLastPage();
            imageViewer.SelectNextTile();
            imageViewer.SelectNextTile();
            imageViewer.SelectNextTile();

            // The next tile button should be disabled
            NextTileToolStripButton nextTile = FormMethods.GetFormComponent<NextTileToolStripButton>(_imageViewerForm);
            Assert.That(!nextTile.Enabled);

            // Rotate the image 90 degrees
            imageViewer.FitMode = FitMode.None;
            imageViewer.Rotate(90, true, true);

            // The next tile button should be enabled
            Assert.That(nextTile.Enabled);
        }

        #endregion Next Tile

        #region RotateClockwise

        /// <summary>
        /// Test that the <see cref="RotateClockwiseToolStripButton"/> is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateClockwiseToolStripButtonDisabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the toolbar button
            RotateClockwiseToolStripButton rotateClockwise =
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!rotateClockwise.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="RotateClockwiseToolStripButton"/> is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateClockwiseToolStripButtonEnabledWithImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the toolbar button
            RotateClockwiseToolStripButton rotateClockwise =
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(rotateClockwise.Enabled);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripButtonTest()
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

            // Get the rotate clockwise button
            RotateClockwiseToolStripButton rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see the image before rotation
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image rotate to the right?", "Did image rotate?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripButtonWithHighlightTest()
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

            // Get the rotate clockwise button
            RotateClockwiseToolStripButton rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and highlight rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripButtonOnlyRotatesCurrentPageTest()
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

            // Get the rotate clockwise button
            RotateClockwiseToolStripButton rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(_imageViewerForm);

            rotateClockwise.PerformClick();

            // Change the page to page 2
            imageViewer.PageNumber = 2;

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Sleep for a second so the user can see the image before the prompt
            System.Threading.Thread.Sleep(1000);

            Assert.That(
                MessageBox.Show("Is this page rotated?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button2, 0) == DialogResult.No);
        }

        /// <summary>
        /// Tests whether <see cref="RotateClockwiseToolStripButton"/> raises the
        /// <see cref="ImageViewer.OrientationChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateClockwiseToolStripButtonEventTest()
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

            // Click the RotateClockwiseToolStripButton
            RotateClockwiseToolStripButton clickMe =
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one OrientationChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripButtonViewPerspectiveTest()
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

            // Get the rotate clockwise button
            RotateClockwiseToolStripButton rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripButtonViewPerspectiveUnlicensedTest()
        {
            // Disable the view perspective license
            LicenseUtilities.DisableId(LicenseIdName.AnnotationFeature);
                        
            // Ensure that Annotation feature is unlicensed
            ExtractException.Assert("ELI21929", "Annotation feature is licensed!",
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

            // Get the rotate clockwise button
            RotateClockwiseToolStripButton rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripButtonViewPerspectiveWithHighlightTest()
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

            // Get the rotate clockwise button
            RotateClockwiseToolStripButton rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and highlight rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripButtonViewPerspectiveUnlicensedWithHighlightTest()
        {
            // Disable the view perspective license
            LicenseUtilities.DisableId(LicenseIdName.AnnotationFeature);
                        
            // Ensure that Annotation feature is unlicensed
            ExtractException.Assert("ELI21930", "Annotation feature is licensed!",
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

            // Get the rotate clockwise button
            RotateClockwiseToolStripButton rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and highlight rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripButtonAnnotationsTest()
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

            // Get the rotate clockwise button
            RotateClockwiseToolStripButton rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and annotations rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripButtonAnnotationsWithHighlightTest()
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

            // Get the rotate clockwise button
            RotateClockwiseToolStripButton rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image, annotations and  highlight rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateClockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseToolStripButtonViewPerspectiveWithAnnotationsTest()
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

            // Get the rotate clockwise button
            RotateClockwiseToolStripButton rotateClockwise = 
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(_imageViewerForm);

            // Click the menu item
            rotateClockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and annotations rotate to the right?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion

        #region RotateCounterclockwise

        /// <summary>
        /// Test that the <see cref="RotateCounterclockwiseToolStripButton"/> is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateCounterclockwiseToolStripButtonDisabledWithNoImageTest()
        {
            _imageViewerForm.Show();

            // Get the toolbar button
            RotateCounterclockwiseToolStripButton rotateCounterclockwise =
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!rotateCounterclockwise.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="RotateCounterclockwiseToolStripButton"/> is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateCounterclockwiseToolStripButtonEnabledWithImageTest()
        {
            // Load the form
            _imageViewerForm.Show();

            // Open the test image
            OpenTestImage(FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm));

            // Get the toolbar button
            RotateCounterclockwiseToolStripButton rotateCounterclockwise =
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(rotateCounterclockwise.Enabled);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripButtonTest()
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

            // Get the rotate Counterclockwise button
            RotateCounterclockwiseToolStripButton rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see that the image before rotation
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image rotate to the left?", "Did image rotate?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripButtonWithHighlightTest()
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

            // Get the rotate Counterclockwise button
            RotateCounterclockwiseToolStripButton rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see the image
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and highlight rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripButtonOnlyRotatesCurrentPageTest()
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

            // Get the rotate Counterclockwise button
            RotateCounterclockwiseToolStripButton rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(_imageViewerForm);

            rotateCounterclockwise.PerformClick();

            // Change the page to page 2
            imageViewer.PageNumber = 2;

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Sleep for a second so the user can see the image before the prompt
            System.Threading.Thread.Sleep(1000);

            Assert.That(
                MessageBox.Show("Is this page rotated?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button2, 0) == DialogResult.No);
        }

        /// <summary>
        /// Tests whether <see cref="RotateCounterclockwiseToolStripButton"/> raises the
        /// <see cref="ImageViewer.OrientationChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateCounterclockwiseToolStripButtonEventTest()
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

            // Click the RotateCounterclockwiseToolStripButton
            RotateCounterclockwiseToolStripButton clickMe =
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one OrientationChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripButtonViewPerspectiveTest()
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

            // Get the rotate clockwise button
            RotateCounterclockwiseToolStripButton rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripButtonViewPerspectiveUnlicensedTest()
        {
            // Disable the view perspective license
            LicenseUtilities.DisableId(LicenseIdName.AnnotationFeature);
                        
            // Ensure that Annotation feature is unlicensed
            ExtractException.Assert("ELI21931", "Annotation feature is licensed!",
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

            // Get the rotate clockwise button
            RotateCounterclockwiseToolStripButton rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripButtonViewPerspectiveWithHighlightTest()
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

            // Get the rotate clockwise button
            RotateCounterclockwiseToolStripButton rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and highlight rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripButtonViewPerspectiveUnlicensedWithHighlightTest()
        {
            // Disable the view perspective license
            LicenseUtilities.DisableId(LicenseIdName.AnnotationFeature);
                        
            // Ensure that Annotation feature is unlicensed
            ExtractException.Assert("ELI21932", "Annotation feature is licensed!",
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

            // Get the rotate clockwise button
            RotateCounterclockwiseToolStripButton rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and highlight rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripButtonAnnotationsTest()
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

            // Get the rotate clockwise button
            RotateCounterclockwiseToolStripButton rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and annotations rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripButtonAnnotationsWithHighlightTest()
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

            // Get the rotate clockwise button
            RotateCounterclockwiseToolStripButton rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(_imageViewerForm);

            // Sleep for a second so the user can see that the image and highlight
            System.Threading.Thread.Sleep(1000);

            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image, annotations and  highlight rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="RotateCounterclockwiseToolStripButton"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseToolStripButtonViewPerspectiveWithAnnotationsTest()
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

            // Get the rotate counter-clockwise button
            RotateCounterclockwiseToolStripButton rotateCounterclockwise = 
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(_imageViewerForm);

            // Click the menu item
            rotateCounterclockwise.PerformClick();

            Assert.That(
                MessageBox.Show("Did the image and annotations rotate to the left?",
                "Did image rotate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion

        #region Pan 

        /// <summary>
        /// Tests whether <see cref="PanToolStripButton"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PanToolStripButtonDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the Pan button
            PanToolStripButton pan = 
                FormMethods.GetFormComponent<PanToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!pan.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="PanToolStripButton"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PanToolStripButtonEnabledWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan button
            PanToolStripButton pan =
                FormMethods.GetFormComponent<PanToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(pan.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="PanToolStripButton"/> is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PanToolStripButtonToggledOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan button
            PanToolStripButton pan =
                FormMethods.GetFormComponent<PanToolStripButton>(_imageViewerForm);

            // Select the Pan button
            pan.PerformClick();

            // Check that the button is checked
            Assert.That(pan.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="PanToolStripButton"/> is toggled off 
        /// when different cursor tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PanToolStripButtonToggledOffWithDifferentSelectionTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan button
            PanToolStripButton pan =
                FormMethods.GetFormComponent<PanToolStripButton>(_imageViewerForm);

            // Select the Pan tool
            pan.PerformClick();

            // Get the ZoomWindow menu item
            ZoomWindowToolStripButton zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_imageViewerForm);

            // Select the Zoom Window tool
            zoomWindow.PerformClick();

            // Check that the Pan button is unchecked
            Assert.That(!pan.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="PanToolStripButton"/> sets the CursorTool 
        /// property when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PanToolStripButtonSetsCursorPropertyWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan button
            PanToolStripButton pan =
                FormMethods.GetFormComponent<PanToolStripButton>(_imageViewerForm);

            // Select the Pan tool
            pan.PerformClick();

            // Check that the CursorTool property has been set
            Assert.That(imageViewer.CursorTool == CursorTool.Pan);
        }

        /// <summary>
        /// Tests whether <see cref="PanToolStripButton"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PanToolStripButtonEventTest()
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

            // Click the PanToolStripButton
            PanToolStripButton clickMe =
                FormMethods.GetFormComponent<PanToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion Pan 

        #region Zoom Window

        /// <summary>
        /// Tests whether <see cref="ZoomWindowToolStripButton"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowToolStripButtonDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the Zoom Window button
            ZoomWindowToolStripButton zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!zoomWindow.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomWindowToolStripButton"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowToolStripButtonEnabledWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Zoom Window button
            ZoomWindowToolStripButton zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(zoomWindow.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomWindowToolStripButton"/> is toggled 
        /// on by default with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowToolStripButtonOnByDefaultWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Zoom Window button
            ZoomWindowToolStripButton zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_imageViewerForm);

            // Check that the button is toggled on
            Assert.That(zoomWindow.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomWindowToolStripButton"/> is toggled 
        /// on when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowToolStripButtonOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan button
            PanToolStripButton pan =
                FormMethods.GetFormComponent<PanToolStripButton>(_imageViewerForm);

            // Set Pan mode
            pan.PerformClick();

            // Get the Zoom Window button
            ZoomWindowToolStripButton zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_imageViewerForm);

            // Set Zoom Window mode
            zoomWindow.PerformClick();

            // Check that the button is toggled on
            Assert.That(zoomWindow.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomWindowToolStripButton"/> is toggled 
        /// off when different tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowToolStripButtonOffWhenNotSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan button
            PanToolStripButton pan =
                FormMethods.GetFormComponent<PanToolStripButton>(_imageViewerForm);

            // Set Pan mode
            pan.PerformClick();

            // Get the Zoom Window button
            ZoomWindowToolStripButton zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_imageViewerForm);

            // Check that the button is toggled off
            Assert.That(!zoomWindow.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomWindowToolStripButton"/> sets CursorTool 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowToolStripButtonSetsCursorToolWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Pan button
            PanToolStripButton pan =
                FormMethods.GetFormComponent<PanToolStripButton>(_imageViewerForm);

            // Set Pan mode
            pan.PerformClick();

            // Get the Zoom Window button
            ZoomWindowToolStripButton zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_imageViewerForm);

            // Set Zoom Window mode
            zoomWindow.PerformClick();

            // Check that the CursorTool property is set correctly
            Assert.That(imageViewer.CursorTool == CursorTool.ZoomWindow);
        }

        /// <summary>
        /// Tests whether <see cref="ZoomWindowToolStripButton"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowToolStripButtonEventTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Set a different cursor tool
            imageViewer.CursorTool = CursorTool.Pan;
            
            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of CursorToolChanged events
            imageViewer.CursorToolChanged += eventCounters.CountEvent<CursorToolChangedEventArgs>;

            // Click the ZoomWindowToolStripButton
            ZoomWindowToolStripButton zoomWindowToolButton =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_imageViewerForm);
            imageViewer.PerformClick(zoomWindowToolButton);

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking the <see cref="ZoomWindowToolStripButton"/> a drag
        /// event on the <see cref="ImageViewer"/> raises a
        /// <see cref="ImageViewer.ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ZoomWindowToolStripButtonRaisesZoomChangedEventTest()
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

            // Click the ZoomWindowToolStripButton
            ZoomWindowToolStripButton clickMe =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_imageViewerForm);
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
        /// Tests whether <see cref="HighlightToolStripSplitButton"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HighlightToolStripSplitButtonDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the Highlight button
            HighlightToolStripSplitButton highlight =
                FormMethods.GetFormComponent<HighlightToolStripSplitButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!highlight.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="HighlightToolStripSplitButton"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HighlightToolStripSplitButtonEnabledWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Highlight button
            HighlightToolStripSplitButton highlight =
                FormMethods.GetFormComponent<HighlightToolStripSplitButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(highlight.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="HighlightToolStripSplitButton"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HighlightToolStripSplitButtonEventTest()
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

            // Click the HighlightToolStripSplitButton
            HighlightToolStripSplitButton clickMe =
                FormMethods.GetFormComponent<HighlightToolStripSplitButton>(_imageViewerForm);
            clickMe.PerformButtonClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion Highlighter 

        #region Angular Highlight

        /// <summary>
        /// Tests whether <see cref="AngularHighlightToolStripButton"/> sets CursorTool 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_AngularHighlightToolStripButtonSetsCursorToolWhenSelectedTest()
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

            // Get the Angular Highlight button
            AngularHighlightToolStripButton angle =
                FormMethods.GetFormComponent<AngularHighlightToolStripButton>(_imageViewerForm);

            // Set Angular Highlight mode
            angle.PerformClick();

            // Check that the CursorTool is set properly
            Assert.That(imageViewer.CursorTool == CursorTool.AngularHighlight);
        }

        /// <summary>
        /// Tests whether <see cref="AngularHighlightToolStripButton"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_AngularHighlightToolStripButtonEventTest()
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

            // Click the AngularHighlightToolStripButton
            AngularHighlightToolStripButton clickMe =
                FormMethods.GetFormComponent<AngularHighlightToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking the <see cref="AngularHighlightToolStripButton"/> a drag
        /// event on the <see cref="ImageViewer"/> raises a
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_AngularHighlightToolStripButtonRaisesLayerObjectAddedEventTest()
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

            // Click the ZoomWindowToolStripButton
            AngularHighlightToolStripButton clickMe =
                FormMethods.GetFormComponent<AngularHighlightToolStripButton>(_imageViewerForm);
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
        /// Tests whether after clicking the <see cref="AngularHighlightToolStripButton"/> a drag
        /// event on the <see cref="ImageViewer"/> allows a user to create a highlight.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_AngularHighlightToolStripButtonAllowsHighlightCreationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Click the ZoomWindowToolStripButton
            AngularHighlightToolStripButton clickMe =
                FormMethods.GetFormComponent<AngularHighlightToolStripButton>(_imageViewerForm);
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
        /// Tests whether <see cref="RectangularHighlightToolStripButton"/> sets CursorTool 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RectangularHighlightToolStripButtonSetsCursorToolWhenSelectedTest()
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

            // Get the Rectangular Highlight button
            RectangularHighlightToolStripButton rectangle =
                FormMethods.GetFormComponent<RectangularHighlightToolStripButton>(_imageViewerForm);

            // Set Rectangular Highlight mode
            rectangle.PerformClick();

            // Check that the CursorTool is set properly
            Assert.That(imageViewer.CursorTool == CursorTool.RectangularHighlight);
        }

        /// <summary>
        /// Tests whether <see cref="RectangularHighlightToolStripButton"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RectangularHighlightToolStripButtonEventTest()
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

            // Click the RectangularHighlightToolStripButton
            RectangularHighlightToolStripButton clickMe =
                FormMethods.GetFormComponent<RectangularHighlightToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking the <see cref="RectangularHighlightToolStripButton"/> a drag
        /// event on the <see cref="ImageViewer"/> raises a
        /// <see cref="LayerObjectsCollection.LayerObjectAdded"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RectangularHighlightToolStripButtonRaisesLayerObjectAddedEventTest()
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

            // Click the ZoomWindowToolStripButton
            RectangularHighlightToolStripButton clickMe =
                FormMethods.GetFormComponent<RectangularHighlightToolStripButton>(_imageViewerForm);
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
        /// Tests whether after clicking the <see cref="RectangularHighlightToolStripButton"/> a drag
        /// event on the <see cref="ImageViewer"/> allows a user to create a highlight.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RectangularHighlightToolStripButtonAllowsHighlightCreationTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Click the ZoomWindowToolStripButton
            RectangularHighlightToolStripButton clickMe =
                FormMethods.GetFormComponent<RectangularHighlightToolStripButton>(_imageViewerForm);
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
        /// Tests whether <see cref="SetHighlightHeightToolStripButton"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SetHighlightHeightToolStripButtonDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the SetHighlightHeight button
            SetHighlightHeightToolStripButton setHighlightHeight =
                FormMethods.GetFormComponent<SetHighlightHeightToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!setHighlightHeight.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="SetHighlightHeightToolStripButton"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SetHighlightHeightToolStripButtonEnabledWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SetHighlightHeight button
            SetHighlightHeightToolStripButton setHighlightHeight =
                FormMethods.GetFormComponent<SetHighlightHeightToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(setHighlightHeight.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="SetHighlightHeightToolStripButton"/> is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SetHighlightHeightToolStripButtonToggledOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SetHighlightHeight button
            SetHighlightHeightToolStripButton setHighlightHeight =
                FormMethods.GetFormComponent<SetHighlightHeightToolStripButton>(_imageViewerForm);

            // Select the SetHighlightHeight button
            setHighlightHeight.PerformClick();

            // Check that the button is checked
            Assert.That(setHighlightHeight.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="SetHighlightHeightToolStripButton"/> is toggled off 
        /// when different cursor tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SetHighlightHeightToolStripButtonToggledOffWithDifferentSelectionTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SetHighlightHeight button
            SetHighlightHeightToolStripButton setHighlightHeight =
                FormMethods.GetFormComponent<SetHighlightHeightToolStripButton>(_imageViewerForm);

            // Select the SetHighlightHeight tool
            setHighlightHeight.PerformClick();

            // Get the ZoomWindow menu item
            ZoomWindowToolStripButton zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_imageViewerForm);

            // Select the Zoom Window tool
            zoomWindow.PerformClick();

            // Check that the SetHighlightHeight button is unchecked
            Assert.That(!setHighlightHeight.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="SetHighlightHeightToolStripButton"/> sets the CursorTool 
        /// property when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SetHighlightHeightToolStripButtonSetsCursorPropertyWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SetHighlightHeight button
            SetHighlightHeightToolStripButton setHighlightHeight =
                FormMethods.GetFormComponent<SetHighlightHeightToolStripButton>(_imageViewerForm);

            // Select the tool
            setHighlightHeight.PerformClick();

            // Check that the CursorTool property has been set
            Assert.That(imageViewer.CursorTool == CursorTool.SetHighlightHeight);
        }

        /// <summary>
        /// Tests whether <see cref="SetHighlightHeightToolStripButton"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SetHighlightHeightToolStripButtonEventTest()
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

            // Click the SetHighlightHeightToolStripButton
            SetHighlightHeightToolStripButton clickMe =
                FormMethods.GetFormComponent<SetHighlightHeightToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion Set Highlight Height

        #region Delete Highlights

        /// <summary>
        /// Tests whether <see cref="DeleteLayerObjectsToolStripButton"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_DeleteLayerObjectsToolStripButtonDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the DeleteLayerObjects button
            DeleteLayerObjectsToolStripButton deleteLayerObjects =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!deleteLayerObjects.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="DeleteLayerObjectsToolStripButton"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_DeleteLayerObjectsToolStripButtonEnabledWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the DeleteLayerObjects button
            DeleteLayerObjectsToolStripButton deleteLayerObjects =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(deleteLayerObjects.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="DeleteLayerObjectsToolStripButton"/> is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_DeleteLayerObjectsToolStripButtonToggledOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the DeleteLayerObjects button
            DeleteLayerObjectsToolStripButton deleteLayerObjects =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripButton>(_imageViewerForm);

            // Select the DeleteLayerObjects button
            deleteLayerObjects.PerformClick();

            // Check that the button is checked
            Assert.That(deleteLayerObjects.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="DeleteLayerObjectsToolStripButton"/> is toggled off 
        /// when different cursor tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_DeleteLayerObjectsToolStripButtonToggledOffWithDifferentSelectionTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the DeleteLayerObjects button
            DeleteLayerObjectsToolStripButton deleteLayerObjects =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripButton>(_imageViewerForm);

            // Select the DeleteLayerObjects tool
            deleteLayerObjects.PerformClick();

            // Get the ZoomWindow menu item
            ZoomWindowToolStripButton zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_imageViewerForm);

            // Select the Zoom Window tool
            zoomWindow.PerformClick();

            // Check that the DeleteLayerObjects button is unchecked
            Assert.That(!deleteLayerObjects.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="DeleteLayerObjectsToolStripButton"/> sets the CursorTool 
        /// property when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_DeleteLayerObjectsToolStripButtonSetsCursorPropertyWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the DeleteLayerObjects button
            DeleteLayerObjectsToolStripButton deleteLayerObjects =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripButton>(_imageViewerForm);

            // Select the tool
            deleteLayerObjects.PerformClick();

            // Check that the CursorTool property has been set
            Assert.That(imageViewer.CursorTool == CursorTool.DeleteLayerObjects);
        }

        /// <summary>
        /// Tests whether <see cref="DeleteLayerObjectsToolStripButton"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_DeleteHighlightsToolStripButtonEventTest()
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

            // Click the DeleteLayerObjectsToolStripButton
            DeleteLayerObjectsToolStripButton clickMe =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="DeleteLayerObjectsToolStripButton"/>
        /// dragging to select a group of highlights raises one
        /// <see cref="LayerObjectsCollection.LayerObjectDeleted"/> event for each highlight selected.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_DeleteHighlightsToolStripButtonRaisesLayerObjectDeletedEventForEachDeletedHighlightTest()
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

            // Click the DeleteLayerObjectsToolStripButton
            DeleteLayerObjectsToolStripButton clickMe =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            FormMethods.ShowModelessInstructionsAndWait(new string[] {
                "Click and drag a box around all three highlights.",
                "Click okay to close this dialog and end this test."});

            // Check that there were 3 delete highlight events raised
            Assert.That(eventCounters.EventCounter == 3);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="DeleteLayerObjectsToolStripButton"/>
        /// dragging to select a group of highlights deletes all selected highlights.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_DeleteHighlightsToolStripButtonAllowsDragSelectionForDeleteTest()
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

            // Click the DeleteLayerObjectsToolStripButton
            DeleteLayerObjectsToolStripButton clickMe =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            FormMethods.ShowModelessInstructionsAndWait(new string[] {
                "Click and drag a box around all three highlights.",
                "Click okay to close this dialog and end this test."});

            // Check that all highlights were deleted
            Assert.That(imageViewer.LayerObjects.Count == 0);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="DeleteLayerObjectsToolStripButton"/>
        /// and dragging to select a group of highlights the cursor tool reverts to the
        /// last used cursor tool.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_DeleteHighlightsToolStripButtonSwitchesToLastUsedCursorToolTest()
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

            // Click the DeleteLayerObjectsToolStripButton
            DeleteLayerObjectsToolStripButton clickMe =
                FormMethods.GetFormComponent<DeleteLayerObjectsToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that the cursor tool changed
            ExtractException.Assert("ELI21935", "Did not change to delete highlight cursor tool!",
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
        /// Tests whether <see cref="SelectLayerObjectToolStripButton"/> is disabled 
        /// without an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SelectLayerObjectToolStripButtonDisabledWithNoImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the SelectLayerObject button
            SelectLayerObjectToolStripButton selectLayerObject =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(_imageViewerForm);

            // Check that the button is disabled
            Assert.That(!selectLayerObject.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="SelectLayerObjectToolStripButton"/> is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SelectLayerObjectToolStripButtonEnabledWithImageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SelectLayerObject button
            SelectLayerObjectToolStripButton selectLayerObject =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(_imageViewerForm);

            // Check that the button is enabled
            Assert.That(selectLayerObject.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="SelectLayerObjectToolStripButton"/> is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SelectLayerObjectToolStripButtonToggledOnWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SelectLayerObject button
            SelectLayerObjectToolStripButton selectLayerObject =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(_imageViewerForm);

            // Select the SelectLayerObject button
            selectLayerObject.PerformClick();

            // Check that the button is checked
            Assert.That(selectLayerObject.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="SelectLayerObjectToolStripButton"/> is toggled off 
        /// when different cursor tool is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SelectLayerObjectToolStripButtonToggledOffWithDifferentSelectionTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SelectLayerObject button
            SelectLayerObjectToolStripButton selectLayerObject =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(_imageViewerForm);

            // Select the SelectLayerObject tool
            selectLayerObject.PerformClick();

            // Get the ZoomWindow menu item
            ZoomWindowToolStripButton zoomWindow =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_imageViewerForm);

            // Select the Zoom Window tool
            zoomWindow.PerformClick();

            // Check that the SelectLayerObject button is unchecked
            Assert.That(!selectLayerObject.Checked);
        }

        /// <summary>
        /// Tests whether <see cref="SelectLayerObjectToolStripButton"/> sets the CursorTool 
        /// property when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SelectLayerObjectToolStripButtonSetsCursorPropertyWhenSelectedTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the SelectLayerObject button
            SelectLayerObjectToolStripButton selectLayerObject =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(_imageViewerForm);

            // Select the tool
            selectLayerObject.PerformClick();

            // Check that the CursorTool property has been set
            Assert.That(imageViewer.CursorTool == CursorTool.SelectLayerObject);
        }

        /// <summary>
        /// Tests whether <see cref="SelectLayerObjectToolStripButton"/> raises the
        /// <see cref="ImageViewer.CursorToolChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SelectHighlightToolStripButtonEventTest()
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

            // Click the SelectLayerObjectToolStripButton
            SelectLayerObjectToolStripButton clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            // Check that exactly one CursorToolChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="SelectLayerObjectToolStripButton"/>
        /// and then selecting and resizing a highlight raises the
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_SelectHighlightToolStripButtonResizeAngularHighlightRaisesLayerObjectChangedEventTest()
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

            // Click the SelectLayerObjectToolStripButton
            SelectLayerObjectToolStripButton clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Click and drag one of the handles on the highlight to resize it.",
                "Click okay to close this dialog and end this test."};

            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that exactly one LayerObjectChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="SelectLayerObjectToolStripButton"/>
        /// selecting and resizing a highlight raises the
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_SelectHighlightToolStripButtonResizeRectangularHighlightSideRaisesLayerObjectChangedEventTest()
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

            // Click the SelectLayerObjectToolStripButton
            SelectLayerObjectToolStripButton clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Click and drag one of the side handles on the highlight to resize it.",
                "Click okay to close this dialog and end this test."};

            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that exactly one LayerObjectChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="SelectLayerObjectToolStripButton"/>
        /// selecting and resizing a highlight raises the
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_SelectHighlightToolStripButtonResizeRectangularHighlightCornerRaisesLayerObjectChangedEventTest()
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

            // Click the SelectLayerObjectToolStripButton
            SelectLayerObjectToolStripButton clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Click and drag one of the corner handles on the highlight to resize it.",
                "Click okay to close this dialog and end this test."};

            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that exactly one LayerObjectChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="SelectLayerObjectToolStripButton"/>
        /// selecting and changing a highlights angle raises the
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_SelectHighlightToolStripButtonChangeHighlightAngleRaisesLayerObjectChangedEventTest()
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

            // Click the SelectLayerObjectToolStripButton
            SelectLayerObjectToolStripButton clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(_imageViewerForm);
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
        /// Tests whether after clicking <see cref="SelectLayerObjectToolStripButton"/>
        /// selecting and resizing a highlight out of the image area does not raises the
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_SelectHighlightToolStripButtonResizeHighlightOutsideImageAreaDoesNotRaisesLayerObjectChangedEventTest()
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

            // Click the SelectLayerObjectToolStripButton
            SelectLayerObjectToolStripButton clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Click on the left side handle of the highlight and drag it outside the image area.",
                "Click okay to close this dialog and end this test."};

            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that no LayerObjectChanged event was raised
            Assert.That(eventCounters.EventCounter == 0);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="SelectLayerObjectToolStripButton"/>
        /// selecting and attempting to move a highlight outside of the image area
        /// does not raise the <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_SelectHighlightToolStripButtonMoveHighlightOutOfImageAreaDoesNotRaisesLayerObjectChangedEventTest()
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

            // Click the SelectLayerObjectToolStripButton
            SelectLayerObjectToolStripButton clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            string[] instructions = new string[] {
                "Click and drag the highlight to move it out of the image area.",
                "Click okay to close this dialog and end this test."};

            FormMethods.ShowModelessInstructionsAndWait(instructions);

            // Check that exactly one LayerObjectChanged event was raised
            Assert.That(eventCounters.EventCounter == 0);
        }

        /// <summary>
        /// Tests whether after clicking <see cref="SelectLayerObjectToolStripButton"/>
        /// selecting and resizing a highlight out of the image area does not raises the
        /// <see cref="LayerObjectsCollection.LayerObjectChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_SelectHighlightToolStripButtonChangeHighlightAngleOutsideImageAreaDoesNotRaisesLayerObjectChangedEventTest()
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

            // Click the SelectLayerObjectToolStripButton
            SelectLayerObjectToolStripButton clickMe =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(_imageViewerForm);
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
