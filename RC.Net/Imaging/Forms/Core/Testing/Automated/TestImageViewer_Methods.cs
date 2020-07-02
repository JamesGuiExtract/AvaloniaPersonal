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
        #region FitMode

        /// <summary>
        /// Test that the <see cref="FitToPageToolStripMenuItem"/> allows the entire 
        /// first page of an image to be visible.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_FitToPageVisibilityTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Set FitMode to FitToPage
            imageViewer.FitMode = FitMode.FitToPage;

            // Ensure that FitMode was set
            ExtractException.Assert("ELI21790", "Unable to set FitMode!",
                imageViewer.FitMode == FitMode.FitToPage);

            // Open the test image
            OpenTestImage(imageViewer);

            // Invalidate and update the control so that the user sees the image viewer
            _imageViewerForm.Invalidate();
            _imageViewerForm.Update();

            // Check that entire page is visible
            Assert.That(MessageBox.Show("Is the entire page visible?",
                "Check page visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the <see cref="FitToPageToolStripMenuItem"/> allows the entirety 
        /// of another page to be visible.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_FitToPageAnotherPageVisibilityTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Set FitMode to FitToPage
            imageViewer.FitMode = FitMode.FitToPage;

            // Ensure that FitMode was set
            ExtractException.Assert("ELI22536", "Unable to set FitMode!",
                imageViewer.FitMode == FitMode.FitToPage);

            // Open the test image
            OpenTestImage(imageViewer);

            // Move to page 2
            imageViewer.PageNumber = 2;

            // Invalidate and update the control so that the user sees the image viewer
            _imageViewerForm.Invalidate();
            _imageViewerForm.Update();

            // Check that entire page is visible
            Assert.That(MessageBox.Show("Is the entire second page visible?",
                "Check page visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test the <see cref="FitToWidthToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_OpenedImageFitToWidthVisibilityTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.Size = new Size(800, 300);

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Set FitMode to FitToWidth
            imageViewer.FitMode = FitMode.FitToWidth;

            // Ensure that FitMode was set
            ExtractException.Assert("ELI21791", "Unable to set FitMode!",
                imageViewer.FitMode == FitMode.FitToWidth);

            // Open the test image
            OpenTestImage(imageViewer);

            // Refresh the image viewer
            _imageViewerForm.Refresh();

            // Check that just page width is visible
            Assert.That(!imageViewer.HScroll && imageViewer.VScroll);
        }

        /// <summary>
        /// Test the <see cref="FitToWidthToolStripMenuItem"/>.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthAcrossPagesVisibilityTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.Size = new Size(800, 300);

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Set FitMode to FitToWidth
            imageViewer.FitMode = FitMode.FitToWidth;

            // Ensure that FitMode was set
            ExtractException.Assert("ELI21956", "Unable to set FitMode!",
                imageViewer.FitMode == FitMode.FitToWidth);

            // Open the test image
            OpenTestImage(imageViewer);

            // Navigate to page 2
            imageViewer.PageNumber = 2;

            // Refresh the image viewer
            _imageViewerForm.Refresh();

            // Check that just page width is visible
            Assert.That(!imageViewer.HScroll && imageViewer.VScroll);
        }

        /// <summary>
        /// Test switching between fit modes without temporary display of scrollbars.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_NoScrollBarsBetweenNoFitAndFitToPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Set FitMode to FitToPage to remove scroll bars
            imageViewer.FitMode = FitMode.FitToPage;

            // Ensure that FitMode was reset
            ExtractException.Assert("ELI21788", "Unable to set FitMode!",
                imageViewer.FitMode == FitMode.FitToPage);

            // Open the test image
            OpenTestImage(imageViewer);

            // Reset FitMode to None
            imageViewer.FitMode = FitMode.None;

            // Ensure that FitMode was reset
            ExtractException.Assert("ELI21789", "Unable to reset FitMode!",
                imageViewer.FitMode == FitMode.None);

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Tell user to watch for flickering scrollbar
            MessageBox.Show("Please watch for any flickering scrollbars as the fit mode is changed.", 
                "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information, 
                MessageBoxDefaultButton.Button1, 0);

            // Set FitMode to FitToPage
            imageViewer.FitMode = FitMode.FitToPage;

            // Check that no scrollbars appeared [DotNetRCAndUtils #50]
            Assert.That(MessageBox.Show("Did scroll bars flicker before FitMode changed to FitToPage?",
                "Did scrollbars display?", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2, 0) == DialogResult.No);
        }

        /// <summary>
        /// Test switching between fit modes without temporary display of scrollbars.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_NoScrollBarsBetweenNoFitAndFitToWidthTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Set FitMode to FitToPage to remove scroll bars
            imageViewer.FitMode = FitMode.FitToPage;

            // Ensure that FitMode was reset
            ExtractException.Assert("ELI22553", "Unable to set FitMode!",
                imageViewer.FitMode == FitMode.FitToPage);

            // Open the test image
            OpenTestImage(imageViewer);

            // Reset FitMode to None
            imageViewer.FitMode = FitMode.None;

            // Ensure that FitMode was reset
            ExtractException.Assert("ELI22554", "Unable to reset FitMode!",
                imageViewer.FitMode == FitMode.None);

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Tell user to watch for flickering scrollbar
            MessageBox.Show("Please watch for any flickering scrollbars as the fit mode is changed.",
                "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, 0);

            // Set FitMode to FitToWidth
            imageViewer.FitMode = FitMode.FitToWidth;

            // Check that no flicker appeared with the vertical scrollbar
            Assert.That(MessageBox.Show("Did the vertical scroll bar flicker before FitMode changed to FitToWidth?",
                "Did the scrollbar flicker?", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2, 0) == DialogResult.No);
        }

        /// <summary>
        /// Tests that a revisted page maintains its previous zoom setting (DotNetRCAndUtils #107)
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RevisitedPageShowsLastZoomSettingTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Zoom in on the image
            imageViewer.ZoomIn();
            imageViewer.ZoomIn();

            // Get the zoom info for this page
            ZoomInfo zoomInfo = imageViewer.ZoomInfo;

            // Move to the next page
            imageViewer.PageNumber = 2;

            // Ensure that the zoom has changed for the new page
            ExtractException.Assert("ELI21951", "Page should be in fit to page!",
                zoomInfo != imageViewer.ZoomInfo);

            // Go back to the first page
            imageViewer.PageNumber = 1;

            // Check that it returned to the previous zoom level
            Assert.That(zoomInfo == imageViewer.ZoomInfo);
        }

        /// <summary>
        /// Tests that an open image has its first page visible.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NoFitOpenImageShowsEntireFirstPageTest()
        {
            // Show the image viewer
            _imageViewerForm.Show();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Refresh the image viewer
            _imageViewerForm.Refresh();

            // Check that neither scroll bar is visible
            Assert.That(!imageViewer.HScroll && !imageViewer.VScroll);
        }

        /// <summary>
        /// Tests that an open image has its first page visible.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_NoFitNewVisitedPageShowsEntirePageTest()
        {
            // Show the image viewer
            _imageViewerForm.Show();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Navigate to page 2
            imageViewer.PageNumber = 2;

            // Refresh the image viewer
            _imageViewerForm.Refresh();

            // Check that neither scroll bar is visible
            Assert.That(!imageViewer.HScroll && !imageViewer.VScroll);
        }

        /// <summary>
        /// Test DotNetRCAndUtils #24 - Highlight scaled improperly
        /// when in FitToPage or FitToWidth mode
        /// </summary>
        [Test, Category("Interactive")]
        [SuppressMessage("Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Pvcs")]
        public void Interactive_HighlightSizeCorrectWhenDrawnInFitToPageModePvcs24Test()
        {
            // Show the image viewer
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.Width = 1000;
            _imageViewerForm.Height = 600;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Perform steps to reproduce error from PVCS entry

            // Set image viewer to fit to width
            imageViewer.FitMode = FitMode.FitToWidth;

            // Open the test image
            OpenTestImage(imageViewer);

            // Refresh the image viewer
            _imageViewerForm.Refresh();

            // Get and click the rectangular highlight button
            RectangularHighlightToolStripButton clickMe =
                FormMethods.GetFormComponent<RectangularHighlightToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            FormMethods.ShowModelessInstructionsAndWait(new string[] {
                "Please draw a highlight near the top of the image",
                "Click okay to close this dialog and continue the test" });

            // Set the fit mode to fit to page
            imageViewer.FitMode = FitMode.FitToPage;

            // Refresh the image viewer
            _imageViewerForm.Refresh();

            // Check that the highlight resized properly
            Assert.That(MessageBox.Show("Did the highlight scale properly when the image resized?",
                "Did highlight size properly?", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test DotNetRCAndUtils #24 - Highlight scaled improperly
        /// when in FitToPage or FitToWidth mode
        /// </summary>
        [Test, Category("Interactive")]
        [SuppressMessage("Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pvcs")]
        public void Interactive_HighlightSizeCorrectWhenDrawnInFitToWidthModePvcs24Test()
        {
            // Show the image viewer
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.Width = 1000;
            _imageViewerForm.Height = 600;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Perform steps to reproduce error from PVCS entry

            // Set image viewer to fit to page
            imageViewer.FitMode = FitMode.FitToPage;

            // Open the test image
            OpenTestImage(imageViewer);

            // Refresh the image viewer
            _imageViewerForm.Refresh();

            // Get and click the rectangular highlight button
            RectangularHighlightToolStripButton clickMe =
                FormMethods.GetFormComponent<RectangularHighlightToolStripButton>(_imageViewerForm);
            clickMe.PerformClick();

            FormMethods.ShowModelessInstructionsAndWait(new string[] {
                "Please draw a highlight near the top of the image",
                "Click okay to close this dialog and continue the test" });

            // Set the fit mode to fit to width
            imageViewer.FitMode = FitMode.FitToWidth;

            // Refresh the image viewer
            _imageViewerForm.Refresh();

            // Check that the highlight resized properly
            Assert.That(MessageBox.Show("Did the highlight scale properly when the image resized?",
                "Did highlight size properly?", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion FitMode

        #region DeleteLayerObjects

        /// <summary>
        /// Test that a prompt is displayed when deleting layer objects on multiple pages.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_PromptWhenDeletingLayerObjectsOnDifferentPageTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add highlights to page 2
            Highlight highlight =
                new Highlight(imageViewer, "Test", new Point(220, 200), new Point(400, 200), 120, 2);
            imageViewer.LayerObjects.Add(highlight);
            highlight =
                new Highlight(imageViewer, "Test", new Point(30, 100), new Point(50, 100), 20, 2);
            imageViewer.LayerObjects.Add(highlight);

            // Ensure there are two highlights on the image now
            ExtractException.Assert("ELI21668", "Layer objects did not get added to the image viewer!",
                imageViewer.LayerObjects.Count == 2);

            // Select all the highlights
            imageViewer.LayerObjects.SelectAll();

            // Invalidate and update the control so that the user sees the image viewer
            _imageViewerForm.Invalidate();
            _imageViewerForm.Update();

            // Sleep for a second so the user can see the form before the message box
            System.Threading.Thread.Sleep(1000);

            // Remove all the selected layer objects
            imageViewer.SelectRemoveSelectedLayerObjects();

            // Check that the user was prompted
            Assert.That(MessageBox.Show("Were you prompted before the layer objects were deleted?",
                "Did prompt display?", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that a prompt is displayed when deleting layer objects on multiple pages.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_PromptWhenDeletingLayerObjectsOnMultiplePagesTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add a highlight to page 1 and page 2
            Highlight highlight =
                new Highlight(imageViewer, "Test", new Point(220, 200), new Point(400, 200), 120, 1);
            imageViewer.LayerObjects.Add(highlight);
            highlight =
                new Highlight(imageViewer, "Test", new Point(30, 100), new Point(50, 100), 20, 2);
            imageViewer.LayerObjects.Add(highlight);

            // Ensure there are two layer objects on the image now
            ExtractException.Assert("ELI21670", "Layer objects did not get added to the image viewer!",
                imageViewer.LayerObjects.Count == 2);

            // Select all the highlights
            imageViewer.LayerObjects.SelectAll();

            // Invalidate and update the control so that the user sees the selected layer objects
            _imageViewerForm.Invalidate();
            _imageViewerForm.Update();

            // Sleep for a second so the user can see the form before the message box
            System.Threading.Thread.Sleep(1000);

            // Remove all the selected layer objects
            imageViewer.SelectRemoveSelectedLayerObjects();

            // Check that the user was prompted
            Assert.That(MessageBox.Show("Were you prompted before the layer objects were deleted?",
                "Did prompt display?", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion DeleteLayerObjects

        #region Tiles

        /// <summary>
        /// Tests whether the navigating tiles preserves the zoom history when in fit to width 
        /// mode [DotNetRCAndUtils #126].
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_TileNavigationFitToWidthZoomHistoryTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Open the test image
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);
            OpenTestImage(imageViewer);

            // Set the fit mode to fit to width
            imageViewer.FitMode = FitMode.FitToWidth;

            // Go to the next tile
            imageViewer.SelectNextTile();

            // Get the current scroll position
            int scrollPosition = imageViewer.ScrollPosition.Y;

            // Retrieve this zoom history entry
            imageViewer.ZoomPrevious();
            imageViewer.ZoomNext();

            // Ensure that the zoom history was preserved
            Assert.That(Math.Abs(imageViewer.ScrollPosition.Y - scrollPosition) < 5);
        }

        #endregion Tiles

        #region OpenImage

        /// <summary>
        /// Tests whether the <see cref="ImageViewer"/> can open a set of unique customer
        /// supplied image types.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_UniqueCustomerFileTypesDisplayProperly()
        {
            // Get the image viewer from the form
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            string uniqueFilesDir = @"I:\Common\Engineering\ProductTesting\UniqueImageTypes";
            string[] fileNames = System.IO.Directory.GetFiles(
                uniqueFilesDir, "*.*", SearchOption.TopDirectoryOnly);

            int failedCount = 0;

            foreach (string fileName in fileNames)
            {
                // Ignore the .csv files and .db files in this directory
                if (fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                    || fileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Set the caption of the image viewer form
                _imageViewerForm.Text = "File: " + fileName;

                try
                {
                    // Open the test image and refresh the form
                    imageViewer.OpenImage(fileName, false);
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI21915",
                        "Error opening image!", ex);
                    ee.AddDebugData("Failed Image File", fileName, false);
                    ee.Log();
                    failedCount++;
                    continue;
                }

                _imageViewerForm.Refresh();

                if (MessageBox.Show("Is the image: \"" + fileName + "\" displayed?",
                    "Image displayed", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 0) ==
                    DialogResult.No)
                {
                    ExtractException ee = new ExtractException("ELI21842",
                        "Failed opening unique file types!");
                    ee.AddDebugData("Failed Image File", fileName, false);
                    ee.Log();
                    failedCount++;
                }
            }

            if (failedCount != 0)
            {
                Console.WriteLine("Number failed: {0}", failedCount);
            }

            Assert.That(failedCount == 0);
        }

        /// <summary>
        /// Tests whether the <see cref="ImageViewer"/> can open a set of unique
        /// image types.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_UniqueImageFileTypesDisplayProperly()
        {
            // Get the image viewer from the form
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            string uniqueFilesDir = @"I:\Common\Engineering\ProductTesting\ImageViewer\ImageTypes";
            string[] fileNames = System.IO.Directory.GetFiles(
                uniqueFilesDir, "*.*", SearchOption.TopDirectoryOnly);

            int failedCount = 0;

            foreach (string fileName in fileNames)
            {
                // Ignore the .db files in this directory
                if (fileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Set the caption of the image viewer form
                _imageViewerForm.Text = "File: " + fileName;

                try
                {
                    // Open the test image and refresh the form
                    imageViewer.OpenImage(fileName, false);
                }
                catch (Exception ex)
                {
                    ExtractException ee = new ExtractException("ELI22363",
                        "Error opening image!", ex);
                    ee.AddDebugData("Failed Image File", fileName, false);
                    ee.Log();
                    failedCount++;
                    continue;
                }

                _imageViewerForm.Refresh();

                if (MessageBox.Show("Is the image: \"" + fileName + "\" displayed?",
                    "Image displayed", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 0) ==
                    DialogResult.No)
                {
                    ExtractException ee = new ExtractException("ELI22362",
                        "Failed opening unique file types!");
                    ee.AddDebugData("Failed Image File", fileName, false);
                    ee.Log();
                    failedCount++;
                }
            }

            if (failedCount != 0)
            {
                Console.WriteLine("Number failed: {0}", failedCount);
            }

            Assert.That(failedCount == 0);
        }


        /// <summary>
        /// Tests whether the <see cref="ImageViewer"/> will open the associated image
        /// file when supplied with a uss file.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ImageFileOpensIfUssSpecified()
        {
            // Get the image viewer from the form
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Get the image file name
            string imageFile = _testImages.GetFile(_TEST_IMAGE_FILE);

            // Get the uss file
            string ussFile = _testImages.GetFile(_TEST_IMAGE_USS_FILE,
                imageFile + ".uss");

            // Show the image viewer form
            _imageViewerForm.Show();

            // Open the uss file
            imageViewer.OpenImage(ussFile, false);

            // Assert that an image is open
            Assert.That(imageViewer.IsImageAvailable);
        }

        #endregion OpenImage

        #region Rotate

        /// <summary>
        /// Tests that the <see cref="ImageViewer.Rotate"/> sets the
        /// <see cref="ImageViewer.Orientation"/> property properly.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateChangesOrientationPropertyProperly()
        {
            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Rotate the image 90 degrees
            imageViewer.Rotate(90, true, true);

            // Check that the orientation is 90 degrees
            Assert.That(imageViewer.Orientation == 90);
        }

        #endregion Rotate

        #region AntiAliasing

        /// <summary>
        /// Tests that images are not anti-aliased if <see cref="ImageViewer.UseAntiAliasing"/>
        /// is <see langword="false"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ImageIsNotAntiAliasedWhenAntiAliasingIsFalseTest()
        {
            // Show the image viewer
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            
            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Turn anti-aliasing off
            imageViewer.UseAntiAliasing = false;

            // Open the test image
            OpenTestImage(imageViewer);

            // Set the zoom window cursor tool
            imageViewer.CursorTool = CursorTool.ZoomWindow;

            FormMethods.ShowModelessInstructionsAndWait(new string[] {
                "Use the mouse to draw zoom areas on the image until you are viewing 1 letter.",
                "Click okay to close this dialog and end this test." });

            // Ask the user if the text appears anti-aliased
            Assert.That(MessageBox.Show("Does the text appear anti-aliased?",
                "Is text anti-aliased?", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2, 0) == DialogResult.No);
        }

        /// <summary>
        /// Tests that images are anti-aliased if <see cref="ImageViewer.UseAntiAliasing"/>
        /// is <see langword="true"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ImageIsAntiAliasedWhenAntiAliasingIsTrueTest()
        {
            // Show the image viewer
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            
            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Turn anti-aliasing on
            imageViewer.UseAntiAliasing = true;

            // Open the test image
            OpenTestImage(imageViewer);

            // Set the zoom window cursor tool
            imageViewer.CursorTool = CursorTool.ZoomWindow;

            FormMethods.ShowModelessInstructionsAndWait(new string[] {
                "Use the mouse to draw zoom areas on the image until you are viewing 1 letter.",
                "Click okay to close this dialog and end this test." });

            // Ask the user if the text appears anti-aliased
            Assert.That(MessageBox.Show("Does the text appear anti-aliased?",
                "Is text anti-aliased?", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Tests that images are anti-aliased if <see cref="ImageViewer.UseAntiAliasing"/>
        /// is <see langword="true"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_PdfImageIsAntiAliasedWhenAntiAliasingIsTrueTest()
        {
            // Show the image viewer
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Turn anti-aliasing on
            imageViewer.UseAntiAliasing = true;

            FormMethods.ShowModelessInstructionsAndWait(new string[] {
                "Open a PDF image.",
                "Use the mouse to draw zoom areas on the image until you are viewing 1 letter.",
                "Click okay to close this dialog and end this test." });

            // Ask the user if the text appears anti-aliased
            Assert.That(MessageBox.Show("Does the text appear anti-aliased?",
                "Is text anti-aliased?", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion AntiAliasing

        #region Annotations

        /// <summary>
        /// Tests that annotations are not displayed if <see cref="ImageViewer.DisplayAnnotations"/>
        /// is <see langword="false"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_AnnotationsNotDisplayedIfDisplayAnnotationsFalseTest()
        {
            // Show the image viewer
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // get the ImageViewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Set the annotation display to true
            imageViewer.DisplayAnnotations = true;

            // Open the annotation image
            OpenAnnotationTestImage(imageViewer);

            // Refresh the screen
            imageViewer.Refresh();

            // Sleep for a second so the user sees the annotation
            System.Threading.Thread.Sleep(1000);

            // Turn annotation display off
            imageViewer.DisplayAnnotations = false;

            // Refresh the screen
            imageViewer.Refresh();

            Assert.That(MessageBox.Show("Are the annotations currently displayed?",
                "Are annotations displayed?", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, 0) == DialogResult.No);
        }

        /// <summary>
        /// Tests that annotations are displayed if <see cref="ImageViewer.DisplayAnnotations"/>
        /// is <see langword="true"/>.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_AnnotationsDisplayedIfDisplayAnnotationsTrueTest()
        {
            // Show the image viewer
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // get the ImageViewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Set the annotation display to false
            imageViewer.DisplayAnnotations = false;

            // Open the annotation image
            OpenAnnotationTestImage(imageViewer);

            // Refresh the screen
            imageViewer.Refresh();

            // Sleep for a second so the user sees the annotation
            System.Threading.Thread.Sleep(1000);

            // Turn annotation display on
            imageViewer.DisplayAnnotations = true;

            // Refresh the screen
            imageViewer.Refresh();

            Assert.That(MessageBox.Show("Are the annotations currently displayed?",
                "Are annotations displayed?", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Tests DotNetRCAndUtils #54 - Annotation disappears on rotated page.
        /// </summary>
        [Test, Category("Interactive")]
        [SuppressMessage("Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Pvcs")]
        public void Interactive_AnnotationRotatesProperlyPvcs54Test()
        {
            // Path to the image file associated with [DotNetRCAndUtils #54]
            string imageFile = @"I:\Common\Testing\PVCS_Testing\DotNetRCAndUtils\54\test.tif";

            // Show the image viewer
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Set the fit mode to Fit to page so the whole image is visible
            imageViewer.FitMode = FitMode.FitToPage;

            // Open the test image file
            imageViewer.OpenImage(imageFile, false);

            // Perform steps to reproduce error from PVCS entry

            // Move to page 3 and rotate it 90 degrees
            imageViewer.PageNumber = 3;
            imageViewer.Rotate(90, true, true);

            // Navigate to next page
            imageViewer.PageNumber = 4;

            // Navigate back to page 3
            imageViewer.PageNumber = 3;

            // Refresh the image viewer
            imageViewer.Refresh();

            // Check that the annotation is still visible
            Assert.That(MessageBox.Show("Is there a black bar on the left side of this page?",
                "Annotation visible?", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Tests DotNetRCAndUtils #53 - Annotation incorrectly scaled in fit mode
        /// </summary>
        [Test, Category("Interactive")]
        [SuppressMessage("Microsoft.Naming",
            "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Pvcs")]
        public void Interactive_AnnotationScaleProperlyInFitModePvcs53Test()
        {
            // Show the image viewer
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.Width = 1000;
            _imageViewerForm.Height = 500;

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Perform steps to reproduce error from PVCS entry

            // Set the fit mode to Fit to page so the whole image is visible
            imageViewer.FitMode = FitMode.FitToPage;

            // Refresh the image viewer
            imageViewer.Refresh();

            // Open the annotation test file
            OpenAnnotationTestImage(imageViewer);

            // Check that the annotations are scaled properly
            Assert.That(MessageBox.Show("Are all of the annotations contained within the image",
                "Annotations scaled properly?", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Annotations

        #region MouseWheel

        /// <summary>
        /// Tests that Ctrl+MouseWheel up raises a <see cref="ImageViewer.ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            MessageId="UpRaises")]
        public void Interactive_CtrlMouseWheelUpRaisesZoomChangedEvent()
        {
            // Show the image viewer
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of zoom changed events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;

            FormMethods.ShowModelessInstructionsAndWait(new string[] {
                "Click on the image viewer title bar to give it focus.",
                "Press and hold the Ctrl key.",
                "Slowly scroll the mouse wheel up 3 times.",
                "Click okay to close this dialog and end this test."});

            // Check that 3 ZoomChanged events occurred
            Assert.That(eventCounters.EventCounter == 3);
        }

        /// <summary>
        /// Tests that Ctrl+MouseWheel down raises a <see cref="ImageViewer.ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_CtrlMouseWheelDownRaisesZoomChangedEvent()
        {
            // Show the image viewer
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of zoom changed events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;

            FormMethods.ShowModelessInstructionsAndWait(new string[] {
                "Click on the image viewer title bar to give it focus.",
                "Press and hold the Ctrl key.",
                "Slowly scroll the mouse wheel down 3 times.",
                "Click okay to close this dialog and end this test."});

            // Check that 3 ZoomChanged events occurred
            Assert.That(eventCounters.EventCounter == 3);
        }

        #endregion MouseWheel
    }
}
