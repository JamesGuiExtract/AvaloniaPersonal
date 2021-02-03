using Extract.Testing.Utilities;
using Extract.Utilities.Forms;
using NUnit.Framework;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Extract.Imaging.Forms.Test
{
    public partial class TestImageViewer
    {
        #region Ctrl+A

        /// <summary>
        /// Test that the shortcut key Ctrl+A does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ControlANoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the Ctrl+A key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.A);

            // No exception was thrown so test has passed
        }

        /// <summary>
        /// Test that the shortcut key Ctrl+A selects all highlights.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SelectAllHighlightsOnSamePageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add two highlights on page 1
            Highlight highlight =
                new Highlight(imageViewer, "Test", new Point(50, 50), new Point(100, 50), 40, 1);
            imageViewer.LayerObjects.Add(highlight);
            highlight =
                new Highlight(imageViewer, "Test", new Point(30, 100), new Point(50, 100), 20, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Ensure there are two highlights on the image now
            ExtractException.Assert("ELI21649", "Highlights did not get added to the image viewer!",
                imageViewer.LayerObjects.Count == 2);

            // Ensure no highlights are currently selected
            imageViewer.LayerObjects.Selection.Clear();

            // Send the Ctrl+A key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.A);

            // Check that all the highlights are selected (counts should be equal)
            Assert.That(imageViewer.LayerObjects.Selection.Count == imageViewer.LayerObjects.Count);
        }

        /// <summary>
        /// Test the shortcut key Ctrl+A that selectes all highlights.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SelectAllHighlightsOnMultiplePagesTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add a highlight to page 1 and page 2
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(50, 50), new Point(100, 50), 40, 1);
            imageViewer.LayerObjects.Add(highlight);
            highlight = 
                new Highlight(imageViewer, "Test", new Point(30, 100), new Point(50, 100), 20, 2);
            imageViewer.LayerObjects.Add(highlight);

            // Ensure there are two highlights on the image now
            ExtractException.Assert("ELI21651", "Highlights did not get added to the image viewer!",
                imageViewer.LayerObjects.Count == 2);

            // Ensure no highlights are currently selected
            imageViewer.LayerObjects.Selection.Clear();

            // Send the Ctrl+A key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.A);

            // Check that all the highlights are selected (counts should be equal)
            Assert.That(imageViewer.LayerObjects.Selection.Count == imageViewer.LayerObjects.Count);
        }

        #endregion

        #region Ctrl+O

        /// <summary>
        /// Test that the shortcut key Ctrl+O opens the File dialog.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ControlOOpensFileDialogTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Send the Ctrl+O key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.O);

            Assert.That(
                MessageBox.Show("Did the Open File dialog appear?", "Did dialog appear?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion

        #region Ctrl+P

        /// <summary>
        /// Test that the shortcut key Ctrl+P does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ControlPNoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the Ctrl+P key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.P);

            // No exception was thrown so test has passed
        }

        /// <summary>
        /// Test that the shortcut key Ctrl+P opens the Print dialog.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ControlPOpensPrintDialogTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.P);

            Assert.That(
                MessageBox.Show("Did the Print dialog appear?", "Did dialog appear?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion

        #region Ctrl+R

        /// <summary>
        /// Test that the shortcut key Ctrl+R does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ControlRNoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the Ctrl+R key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.R);

            // No exception was thrown so test has passed
        }

        /// <summary>
        /// Test that the shortcut key Ctrl+R rotates the current page clockwise.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ControlRRotatesClockwiseTest()
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

            // Sleep for a second so the user can see the image before rotation
            System.Threading.Thread.Sleep(1000);

            // Send the Ctrl+R key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.R);

            Assert.That(
                MessageBox.Show("Did the image rotate to the right?", "Did image rotate?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion

        #region Ctrl+Shift+R

        /// <summary>
        /// Test that the shortcut key Ctrl+Shift+R does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ControlShiftRNoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the Ctrl+Shift+R key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.Shift | Keys.R);

            // No exception was thrown so test has passed
        }

        /// <summary>
        /// Test that the shortcut key Ctrl+Shift+R rotates the current page 
        /// counterclockwise.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ControlShiftRRotatesCounterclockwiseTest()
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

            // Sleep for a second so the user can see the image before rotation
            System.Threading.Thread.Sleep(1000);

            // Send the Ctrl+Shift+R key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.Shift | Keys.R);

            Assert.That(
                MessageBox.Show("Did the image rotate to the left?", "Did image rotate?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion

        #region Ctrl+End

        /// <summary>
        /// Test that the shortcut key Ctrl+End navigates to the last page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ControlEndNavigatesToTheLastPageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Send the Ctrl+End key press to navigate to the last page
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.End);

            // Check that the last page is visible
            Assert.That(imageViewer.PageCount == imageViewer.PageNumber);
        }

        /// <summary>
        /// Test that the shortcut key Ctrl+End does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ControlEndNoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the Ctrl+End key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.End);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key Ctrl+End does not throw an exception if an 
        /// image is open to the last page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ControlEndNoExceptionWithOpenImageOnLastPageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Send the Ctrl+End key press to navigate to the last page
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.End);

            // Send the Ctrl+End key press again
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.End);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        #endregion Ctrl+End

        #region Ctrl+Home

        /// <summary>
        /// Test that the shortcut key Ctrl+Home does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ControlHomeNoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the Ctrl+Home key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.Home);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key Ctrl+Home does not throw an exception if an 
        /// image is open to the first page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ControlHomeNoExceptionWithOpenImageOnFirstPageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Send the Ctrl+Home key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.Home);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key Ctrl+Home navigates to the first page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ControlHomeNavigatesToTheFirstPageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Navigate to the last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Send the Ctrl+Home key press to navigate to the first page
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.Home);

            // Check that the first page is visible
            Assert.That(imageViewer.PageNumber == 1);
        }

        #endregion Ctrl+Home

        #region Ctrl+Plus

        /// <summary>
        /// Test that the shortcut key Ctrl+Plus does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ControlPlusNoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the Ctrl+Plus key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.Add);

            // Send the Ctrl+Plus (numpad) key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.Oemplus);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key Ctrl+Plus zooms in.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ControlPlusZoomsInTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Fit to page so that zoom appears clearly
            imageViewer.FitMode = FitMode.FitToPage;

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Pause for a second to let user see image
            System.Threading.Thread.Sleep(1000);

            // Send the Ctrl+Plus key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.Add);

            Assert.That(
                MessageBox.Show("Did the image zoom in from Fit-To-Page?", "Check zoom state",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion

        #region Ctrl+Minus

        /// <summary>
        /// Test that the shortcut key Ctrl+Minus does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ControlMinusNoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the Ctrl+Minus key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.Subtract);

            // Send the Ctrl+Minus (numpad) key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.OemMinus);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key Ctrl+Minus zooms out.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ControlMinusZoomsOutTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Fit to page so that zoom appears clearly
            imageViewer.FitMode = FitMode.FitToPage;

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            // Pause for a second to let user see image
            System.Threading.Thread.Sleep(1000);

            // Send the Ctrl+Minus key press
            imageViewer.Shortcuts.ProcessKey(Keys.Control | Keys.Subtract);

            Assert.That(
                MessageBox.Show("Did the image zoom out from Fit-To-Page?", "Check zoom state",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion

        #region DEL

        /// <summary>
        /// Test that the shortcut key DEL does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_DeleteNoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the DEL key press
            imageViewer.Shortcuts.ProcessKey(Keys.Delete);

            // No exception was thrown so test has passed
        }

        /// <summary>
        /// Test the shortcut key DEL that deletes all selected highlights.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_DeleteAllHighlightsOnImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Add two highlights on page 1
            Highlight highlight = 
                new Highlight(imageViewer, "Test", new Point(30, 30), new Point(50, 30), 20, 1);
            imageViewer.LayerObjects.Add(highlight);
            highlight = 
                new Highlight(imageViewer, "Test", new Point(30, 100), new Point(50, 100), 20, 1);
            imageViewer.LayerObjects.Add(highlight);

            // Ensure there are two highlights on the image now
            ExtractException.Assert("ELI21667", "Highlights did not get added to the image viewer!",
                imageViewer.LayerObjects.Count == 2);

            // Select all the highlights
            imageViewer.LayerObjects.SelectAll();

            // Send the DEL key press
            imageViewer.Shortcuts.ProcessKey(Keys.Delete);

            // Check that all the highlights have been deleted
            Assert.That(imageViewer.LayerObjects.Count == 0);
        }

        #endregion

        #region PageDown

        /// <summary>
        /// Test that the shortcut key PageDown does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PageDownNoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the PageDown key press
            imageViewer.Shortcuts.ProcessKey(Keys.PageDown);

            // No exception was thrown so test has passed
        }

        /// <summary>
        /// Test that the shortcut key PageDown navigates to the next page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PageDownNavigatesToTheNextPageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Navigate to the first page
            imageViewer.PageNumber = 1;

            // Send the PageDown key press to navigate to the next page
            imageViewer.Shortcuts.ProcessKey(Keys.PageDown);

            // Check that the second page is visible
            Assert.That(imageViewer.PageNumber == 2);
        }

        #endregion

        #region PageUp

        /// <summary>
        /// Test that the shortcut key PageUp does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PageUpNoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the PageUp key press
            imageViewer.Shortcuts.ProcessKey(Keys.PageUp);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key PageUp navigates to the previous page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PageUpNavigatesToThePreviousPageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Navigate to the last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Send the PageUp key press to navigate to the previous page
            imageViewer.Shortcuts.ProcessKey(Keys.PageUp);

            // Check that the second-to-last page is visible
            Assert.That(imageViewer.PageNumber == imageViewer.PageCount - 1);
        }

        #endregion

        #region Alt+Left

        /// <summary>
        /// Test that the shortcut key Alt+Left does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_AltLeftNoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the Alt+Left key press
            imageViewer.Shortcuts.ProcessKey(Keys.Alt | Keys.Left);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key Alt+Left does not throw an exception if an 
        /// image is open but has no zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_AltLeftNoExceptionIfNoZoomHistoryTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Confirm that zoom history is empty
            ExtractException.Assert("ELI21797", "Zoom history is not empty!",
                !imageViewer.CanZoomPrevious && !imageViewer.CanZoomNext);

            // Send the Alt+Left key press
            imageViewer.Shortcuts.ProcessKey(Keys.Alt | Keys.Left);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key Alt+Left changes to previous zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_AltLeftChangesToPreviousZoomHistoryTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the zoom level
            double zoomLevelOriginal = imageViewer.ZoomInfo.ScaleFactor;

            // Create a previous zoom history entry
            imageViewer.ZoomIn();

            // Get the new zoom level
            double zoomLevelNew = imageViewer.ZoomInfo.ScaleFactor;

            // Send the Alt+Left key press
            imageViewer.Shortcuts.ProcessKey(Keys.Alt | Keys.Left);

            // Check that the zoom level changed back to original AND
            // that the original zoom level is different than new zoom level
            Assert.That(imageViewer.ZoomInfo.ScaleFactor == zoomLevelOriginal &&
                zoomLevelOriginal != zoomLevelNew);
        }

        #endregion

        #region Alt+Right

        /// <summary>
        /// Test that the shortcut key Alt+Right does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_AltRightNoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the Alt+Right key press
            imageViewer.Shortcuts.ProcessKey(Keys.Alt | Keys.Right);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key Alt+Right does not throw an exception if an 
        /// image is open but has no zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_AltRightNoExceptionIfNoZoomHistoryTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Confirm that zoom history is empty
            ExtractException.Assert("ELI21798", "Zoom history is not empty!",
                !imageViewer.CanZoomPrevious && !imageViewer.CanZoomNext);

            // Send the Alt+Right key press
            imageViewer.Shortcuts.ProcessKey(Keys.Alt | Keys.Right);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Tests that the shortcut Alt+Right zooms to 
        /// the next history entry.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_AltRightZoomsNextTest()
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

            // Send the Alt+Right key press
            imageViewer.Shortcuts.ProcessKey(Keys.Alt | Keys.Right);

            // Check that the zoom level changed back to the zoomed in value AND
            // that the previous zoom level is different than new zoom level
            Assert.That(imageViewer.ZoomInfo.ScaleFactor == zoomLevelIn &&
                zoomLevelIn != zoomLevelPrevious);
        }

        #endregion

        #region F7

        /// <summary>
        /// Test that the shortcut key F7 does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_F7NoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the F7 key press
            imageViewer.Shortcuts.ProcessKey(Keys.F7);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key F7 zooms in.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_F7ZoomsInTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Fit to page so that zoom appears clearly
            imageViewer.FitMode = FitMode.FitToPage;

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            System.Threading.Thread.Sleep(1000);

            // Send the F7 key press
            imageViewer.Shortcuts.ProcessKey(Keys.F7);

            Assert.That(
                MessageBox.Show("Did the image zoom in from Fit-To-Page?", "Check zoom state",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion

        #region F8

        /// <summary>
        /// Test that the shortcut key F8 does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_F8NoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the F8 key press
            imageViewer.Shortcuts.ProcessKey(Keys.F8);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key F8 zooms out.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_F8ZoomsOutTest()
        {
            // Show the image viewer form
            _imageViewerForm.Show();
            _imageViewerForm.BringToFront();
            _imageViewerForm.WindowState = FormWindowState.Maximized;

            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Fit to page so that zoom appears clearly
            imageViewer.FitMode = FitMode.FitToPage;

            // Refresh the Image viewer
            _imageViewerForm.Refresh();

            System.Threading.Thread.Sleep(1000);

            // Send the F8 key press
            imageViewer.Shortcuts.ProcessKey(Keys.F8);

            Assert.That(
                MessageBox.Show("Did the image zoom out from Fit-To-Page?", "Check zoom state",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion

        #region A

        /// <summary>
        /// Test that the shortcut key A does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ANoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the A key press
            imageViewer.Shortcuts.ProcessKey(Keys.A);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key A selects the Pan tool if an 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ASelectsPanToolWithImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Send the A key press
            imageViewer.Shortcuts.ProcessKey(Keys.A);

            // Confirm that the Pan tool is selected
            Assert.That(imageViewer.CursorTool == CursorTool.Pan);
        }

        #endregion

        #region H

        /// <summary>
        /// Test that the shortcut key H does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HNoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the H key press
            imageViewer.Shortcuts.ProcessKey(Keys.H);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key H selects the Highlight tool if an 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HSelectsHighlightToolWithImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Send the A key press - to select something different
            imageViewer.Shortcuts.ProcessKey(Keys.A);

            // Send the H key press
            imageViewer.Shortcuts.ProcessKey(Keys.H);

            // Confirm that one of the Highlight tools is selected
            Assert.That(imageViewer.CursorTool == CursorTool.AngularHighlight || 
                imageViewer.CursorTool == CursorTool.RectangularHighlight);
        }

        /// <summary>
        /// Test that the shortcut key H toggles between the Highlight tools.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HTogglesBetweenHighlightToolsWithImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Send the H key press
            imageViewer.Shortcuts.ProcessKey(Keys.H);

            // Store selection state of each Highlight tool
            bool angularHighlight1 = imageViewer.CursorTool == CursorTool.AngularHighlight;
            bool rectangularHighlight1 = imageViewer.CursorTool == CursorTool.RectangularHighlight;

            // Send the H key press again
            imageViewer.Shortcuts.ProcessKey(Keys.H);

            // Store new selection state of each Highlight tool
            bool angularHighlight2 = imageViewer.CursorTool == CursorTool.AngularHighlight;
            bool rectangularHighlight2 = imageViewer.CursorTool == CursorTool.RectangularHighlight;

            // Confirm that the Highlight tools changed selection
            Assert.That(angularHighlight1 != angularHighlight2 && 
                rectangularHighlight1 != rectangularHighlight2);
        }

        #endregion

        #region P

        /// <summary>
        /// Test that the shortcut key P selects Fit-To-Page mode.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PSelectsFitToPageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Send the W key press - to select something else
            imageViewer.Shortcuts.ProcessKey(Keys.W);

            // Send the P key press
            imageViewer.Shortcuts.ProcessKey(Keys.P);

            // Confirm that Fit-To-Page mode is selected
            Assert.That(imageViewer.FitMode == FitMode.FitToPage);
        }

        #endregion

        #region Backspace

        /// <summary>
        /// Test that the shortcut key R changes to previous zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RChangesToPreviousZoomHistoryTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the zoom level
            double zoomLevelOriginal = imageViewer.ZoomInfo.ScaleFactor;

            // Create a previous zoom history entry
            imageViewer.ZoomIn();

            // Get the new zoom level
            double zoomLevelNew = imageViewer.ZoomInfo.ScaleFactor;

            // Send the Backspace key press - to zoom to previous history item
            imageViewer.Shortcuts.ProcessKey(Keys.Back);

            // Check that the zoom level changed back to original AND
            // that the original zoom level is different than new zoom level
            Assert.That(imageViewer.ZoomInfo.ScaleFactor == zoomLevelOriginal &&
                zoomLevelOriginal != zoomLevelNew);
        }

        #endregion

        #region W

        /// <summary>
        /// Test that the shortcut key W selects Fit-To-Width mode.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_WSelectsFitToWidthTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Send the P key press - to select something else
            imageViewer.Shortcuts.ProcessKey(Keys.P);

            // Send the W key press
            imageViewer.Shortcuts.ProcessKey(Keys.W);

            // Confirm that Fit-To-Width mode is selected
            Assert.That(imageViewer.FitMode == FitMode.FitToWidth);
        }

        #endregion

        #region Z

        /// <summary>
        /// Test that the shortcut key Z selects the Zoom Window tool if an 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZSelectsZoomWindowToolWithImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Send the A key press - to select something else
            imageViewer.Shortcuts.ProcessKey(Keys.A);

            // Send the Z key press
            imageViewer.Shortcuts.ProcessKey(Keys.Z);

            // Confirm that the Zoom Window tool is selected
            Assert.That(imageViewer.CursorTool == CursorTool.ZoomWindow);
        }

        /// <summary>
        /// Test that the shortcut key Z does not throw an exception if no 
        /// image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZNoExceptionWithNoImageTest()
        {
            // Get the imageviewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the Z key press
            imageViewer.Shortcuts.ProcessKey(Keys.Z);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        #endregion

        #region Comma (Previous Tile)

        /// <summary>
        /// Test that the shortcut key comma navigates to the previous view tile.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_CommaNavigatesToPreviousTileTest()
        {
            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Go to the fourth page
            imageViewer.PageNumber = 4;

            // Send the comma key press to navigate to the previous tile
            imageViewer.Shortcuts.ProcessKey(Keys.Oemcomma);

            // Check that the image viewer is now on page 3
            Assert.That(imageViewer.PageNumber == 3);
        }

        /// <summary>
        /// Test that the shortcut key comma does not throw an exception if no image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_CommaNoExceptionWithNoImageTest()
        {
            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the comma key press
            imageViewer.Shortcuts.ProcessKey(Keys.Oemcomma);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key comma does not throw an exception if an image is open to the 
        /// first tile.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_CommaNoExceptionWithOpenImageOnFirstTileTest()
        {
            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Send the comma key press
            imageViewer.Shortcuts.ProcessKey(Keys.Oemcomma);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        #endregion Comma (Previous Tile)

        #region Period (Next Tile)

        /// <summary>
        /// Test that the shortcut key period navigates to the next view tile.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PeriodNavigatesToNextTileTest()
        {
            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Send the period key press to navigate to the next tile
            imageViewer.Shortcuts.ProcessKey(Keys.OemPeriod);

            // Check that the image viewer is now on page 2
            Assert.That(imageViewer.PageNumber == 2);
        }

        /// <summary>
        /// Test that the shortcut key period does not throw an exception if no image is open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PeriodNoExceptionWithNoImageTest()
        {
            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Send the period key press
            imageViewer.Shortcuts.ProcessKey(Keys.OemPeriod);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        /// <summary>
        /// Test that the shortcut key period does not throw an exception if an image is open to the 
        /// first tile.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PeriodNoExceptionWithOpenImageOnLastTileTest()
        {
            // Get the image viewer
            ImageViewer imageViewer = FormMethods.GetFormComponent<ImageViewer>(_imageViewerForm);

            // Open the test image
            OpenTestImage(imageViewer);

            // Go to the last page
            imageViewer.GoToLastPage();

            // Send the period key press
            imageViewer.Shortcuts.ProcessKey(Keys.OemPeriod);

            // No exception was thrown so test has passed
            Assert.That(true);
        }

        #endregion Period (Next Tile)
    }
}
