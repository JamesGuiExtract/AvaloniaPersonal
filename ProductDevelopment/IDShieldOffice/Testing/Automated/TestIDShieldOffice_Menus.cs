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

namespace IDShieldOffice.Test
{
    public partial class TestIDShieldOffice
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
            _idShieldOfficeForm.Show();

            // Get the File - Open menu item
            OpenImageToolStripMenuItem fileOpen =
                FormMethods.GetFormComponent<OpenImageToolStripMenuItem>(_idShieldOfficeForm);

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the File - Open menu item
            OpenImageToolStripMenuItem fileOpen =
                FormMethods.GetFormComponent<OpenImageToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is enabled
            Assert.That(fileOpen.Enabled);
        }

        /// <summary>
        /// Tests whether <see cref="OpenImageToolStripMenuItem"/> raises the
        /// <see cref="ImageFileChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_FileOpenRaisesEventTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Prompt the user to select a valid image file
            MessageBox.Show("Please select a valid image file.", "", MessageBoxButtons.OK,
                MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ImageFileChanged events
            imageViewer.ImageFileChanged += eventCounters.CountEvent<ImageFileChangedEventArgs>;

            // Get the File Open menu item
            OpenImageToolStripMenuItem fileOpen =
                FormMethods.GetFormComponent<OpenImageToolStripMenuItem>(_idShieldOfficeForm);
            fileOpen.PerformClick();

            // Check that exactly one ImageFileChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
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
            _idShieldOfficeForm.Show();

            // Get the File - Close menu item
            ToolStripMenuItem fileClose =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Close");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the File - Close menu item
            ToolStripMenuItem fileClose =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Close");

            // Check that the menu item is enabled
            Assert.That(fileClose.Enabled);
        }

        /// <summary>
        /// Tests whether File - Close menu item raises the
        /// <see cref="ImageFileChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FileCloseRaisesEventTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ImageFileChanged events
            imageViewer.ImageFileChanged += eventCounters.CountEvent<ImageFileChangedEventArgs>;

            // Get the File - Close menu item
            ToolStripMenuItem fileClose =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Close");

            // Close the file
            fileClose.PerformClick();

            // Check that exactly one ImageFileChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
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
            _idShieldOfficeForm.Show();

            // Get the File Save menu item
            ToolStripMenuItem fileSave =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Save");

            // Check that the menu item is disabled
            Assert.That(!fileSave.Enabled);
        }

        #endregion File Save

        #region File Save As

        /// <summary>
        /// Test that the File - Save As menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FileSaveAsDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the File - Save As menu item
            ToolStripMenuItem fileSaveAs =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm,
                "Save &As...");

            // Check that the menu item is disabled
            Assert.That(!fileSaveAs.Enabled);
        }

        /// <summary>
        /// Test that the File - Save As menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FileSaveAsEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the File - Save As menu item
            ToolStripMenuItem fileSaveAs =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm,
                "Save &As...");

            // Check that the menu item is enabled
            Assert.That(fileSaveAs.Enabled);
        }

        #endregion File Save As

        #region File Page Setup

        /// <summary>
        /// Test that the File - Page Setup menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FilePageSetupDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the File - Page Setup menu item
            ToolStripMenuItem filePageSetup =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm,
                "Pa&ge setup...");

            // Check that the menu item is disabled
            Assert.That(!filePageSetup.Enabled);
        }

        /// <summary>
        /// Test that the File - Page Setup menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FilePageSetupEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the File - Page Setup menu item
            ToolStripMenuItem filePageSetup =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm,
                "Pa&ge setup...");

            // Check that the menu item is enabled
            Assert.That(filePageSetup.Enabled);
        }

        #endregion File Page Setup

        #region File Print Preview

        /// <summary>
        /// Test that the File - Print Preview menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FilePrintPreviewDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the File - Print Preview menu item
            ToolStripMenuItem filePreview =
                FormMethods.GetFormComponent<PrintPreviewToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is disabled
            Assert.That(!filePreview.Enabled);
        }

        /// <summary>
        /// Test that the File - Print Preview menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FilePrintPreviewEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the File - Print Preview menu item
            ToolStripMenuItem filePreview =
                FormMethods.GetFormComponent<PrintPreviewToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is enabled
            Assert.That(filePreview.Enabled);
        }

        #endregion File Print Preview

        #region File Print

        /// <summary>
        /// Test that the <see cref="PrintImageToolStripMenuItem"/> is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FilePrintDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the File - Print menu item
            PrintImageToolStripMenuItem filePrint =
                FormMethods.GetFormComponent<PrintImageToolStripMenuItem>(_idShieldOfficeForm);

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the File - Print menu item
            PrintImageToolStripMenuItem filePrint =
                FormMethods.GetFormComponent<PrintImageToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is enabled
            Assert.That(filePrint.Enabled);
        }

        #endregion File Print

        #region File Properties

        /// <summary>
        /// Test that the File - Properties menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FilePropertiesDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the File Properties menu item
            ToolStripMenuItem fileProperties =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm,
                "P&roperties");

            // Check that the menu item is disabled
            Assert.That(!fileProperties.Enabled);
        }

        /// <summary>
        /// Test that the File - Properties menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FilePropertiesEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the File - Properties menu item
            ToolStripMenuItem fileProperties =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm,
                "P&roperties");

            // Check that the menu item is enabled
            Assert.That(fileProperties.Enabled);
        }

        #endregion File Properties

        #region File Exit

        /// <summary>
        /// Test that the File - Exit menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FileExitEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the File - Exit menu item
            ToolStripMenuItem fileExit =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "E&xit");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the File - Exit menu item
            ToolStripMenuItem fileExit =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "E&xit");

            // Check that the menu item is enabled
            Assert.That(fileExit.Enabled);
        }

        #endregion File Exit

        #region Edit Select All

        /// <summary>
        /// Test that the Edit - Select All menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_EditSelectAllDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Edit - Select All menu item
            ToolStripMenuItem editSelectAll =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Select &all");

            // Check that the menu item is disabled
            Assert.That(!editSelectAll.Enabled);
        }

        /// <summary>
        /// Test that the Edit - Select All menu item is disabled 
        /// with image open and no layer objects.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_EditSelectAllDisabledWithImageNoLayerObjectsTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Edit - Select All menu item
            ToolStripMenuItem editSelectAll =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Select &all");

            // Check that the menu item is disabled
            Assert.That(!editSelectAll.Enabled);
        }

        /// <summary>
        /// Test that the Edit - Select All menu item is enabled 
        /// with image open and visible layer objects.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_EditSelectAllEnabledWithImageAndLayerObjectsTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Tools - Apply Bates Number menu item
            ToolStripMenuItem toolsBates =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Apply Bates number");

            // Apply the Bates Number - to provide a visible layer object
            toolsBates.PerformClick();

            // Get the Edit - Select All menu item
            ToolStripMenuItem editSelectAll =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Select &all");

            // Check that the menu item is enabled
            Assert.That(editSelectAll.Enabled);
        }

        #endregion Edit Select All

        #region Edit Delete Selection

        /// <summary>
        /// Test that the Edit - Delete Selection menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_EditDeleteSelectionDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Edit - Delete Selection menu item
            ToolStripMenuItem editDeleteSelection =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Delete selection");

            // Check that the menu item is disabled
            Assert.That(!editDeleteSelection.Enabled);
        }

        /// <summary>
        /// Test that the Edit - Delete Selection menu item is disabled 
        /// with image open and no layer objects.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_EditDeleteSelectionDisabledWithImageNoLayerObjectsTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Edit - Delete Selection menu item
            ToolStripMenuItem editDeleteSelection =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Delete selection");

            // Check that the menu item is disabled
            Assert.That(!editDeleteSelection.Enabled);
        }

        /// <summary>
        /// Test that the Edit - Delete Selection menu item is disabled 
        /// with image open and visible but unselected layer objects.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_EditDeleteSelectionDisabledWithImageSelectedObjectsTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Tools - Apply Bates Number menu item
            ToolStripMenuItem toolsBates =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Apply Bates number");

            // Apply the Bates Number - to provide a visible layer object
            toolsBates.PerformClick();

            // Get the Edit - Select All menu item
            ToolStripMenuItem editSelectAll =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Select &all");

            // Select the layer objects
            editSelectAll.PerformClick();

            // Get the Edit - Delete Selection menu item
            ToolStripMenuItem editDeleteSelection =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Delete selection");

            // Check that the menu item is enabled
            Assert.That(editDeleteSelection.Enabled);
        }

        /// <summary>
        /// Test that the Edit - Delete Selection menu item is enabled 
        /// with image open and selected layer objects.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_EditDeleteSelectionEnabledWithImageSelectedObjectsTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Tools - Apply Bates Number menu item
            ToolStripMenuItem toolsBates =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Apply Bates number");

            // Apply the Bates Number - to provide a visible layer object
            toolsBates.PerformClick();

            // Get the Edit - Select All menu item
            ToolStripMenuItem editSelectAll =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Select &all");

            // Select the layer objects
            editSelectAll.PerformClick();

            // Get the Edit - Delete Selection menu item
            ToolStripMenuItem editDeleteSelection =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Delete selection");

            // Check that the menu item is enabled
            Assert.That(editDeleteSelection.Enabled);
        }

        #endregion Edit Delete Selection

        #region Edit Find

        /// <summary>
        /// Test that the Edit - Find menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_EditFindEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Edit - Find menu item
            ToolStripMenuItem editFind =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Find...");

            // Check that the menu item is enabled
            Assert.That(editFind.Enabled);
        }

        /// <summary>
        /// Test that the Edit - Find menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_EditFindEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Edit - Find menu item
            ToolStripMenuItem editFind =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Find...");

            // Check that the menu item is enabled
            Assert.That(editFind.Enabled);
        }

        /// <summary>
        /// Test that the Edit - Find menu item displays the Find and 
        /// redact Words / patterns dialog.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_EditFindDisplaysDialogTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Get the Edit - Find menu item
            ToolStripMenuItem editFind =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Find...");

            // Select the menu item
            editFind.PerformClick();

            // Check that the expected dialog is visible
            Assert.That(MessageBox.Show("Is the \"Find or redact - Words/patterns\" dialog visible?",
                "Check dialog visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Edit Find

        #region Edit Redact Entire Page

        /// <summary>
        /// Test that the Edit - Redact Entire Page menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_EditRedactEntirePageDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Edit - Redact Entire Page menu item
            ToolStripMenuItem editRedact =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Redact entire page");

            // Check that the menu item is disabled
            Assert.That(!editRedact.Enabled);
        }

        /// <summary>
        /// Test that the Edit - Redact Entire Page menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_EditRedactEntirePageEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Edit - Redact Entire Page menu item
            ToolStripMenuItem editRedact =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Redact entire page");

            // Check that the menu item is enabled
            Assert.That(editRedact.Enabled);
        }

        /// <summary>
        /// Test that the Edit - Redact Entire Page menu item redacts the 
        /// entire page.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_EditRedactEntirePageRedactionTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Edit - Redact Entire Page menu item
            ToolStripMenuItem editRedact =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Redact entire page");

            // Select the menu item
            editRedact.PerformClick();

            // Check that the page was redacted
            Assert.That(MessageBox.Show("Is page 1 completely redacted?",
                "Check redaction of page", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Edit Redact Entire Page

        #region Edit Preferences

        /// <summary>
        /// Test that the Edit - Preferences menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_EditPreferencesEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Edit - Preferences menu item
            ToolStripMenuItem editPreferences =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Preferences...");

            // Check that the menu item is enabled
            Assert.That(editPreferences.Enabled);
        }

        /// <summary>
        /// Test that the Edit - Preferences menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_EditPreferencesEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Edit - Preferences menu item
            ToolStripMenuItem editPreferences =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Preferences...");

            // Check that the menu item is enabled
            Assert.That(editPreferences.Enabled);
        }

        /// <summary>
        /// Test that the Edit - Preferences menu item displays the User
        /// Preferences dialog.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_EditPreferencesDisplaysDialogTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Get the Edit - Preferences menu item
            ToolStripMenuItem editPreferences =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Preferences...");

            // Select the menu item
            editPreferences.PerformClick();

            // Check that the expected dialog is visible
            Assert.That(MessageBox.Show("Did the \"User Preferences\" dialog appear?",
                "Check dialog visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Edit Preferences

        #region View Zoom Fit To Page

        /// <summary>
        /// Test that the View - Zoom - Fit To Page menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomFitToPageEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the View - Zoom - Fit To Page menu item
            ToolStripMenuItem viewZoomFitToPage =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Fit to &page");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Zoom - Fit To Page menu item
            ToolStripMenuItem viewZoomFitToPage =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Fit to &page");

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
            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Get the imageviewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Set the fit mode to none
            imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI22726", "Could not change fit mode to none!",
                imageViewer.FitMode == FitMode.None);

            // Get the fit to page menu item
            FitToPageToolStripMenuItem fitToPage = FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_idShieldOfficeForm);

            // Click the menu item
            fitToPage.PerformClick();

            Assert.That(imageViewer.FitMode == FitMode.FitToPage);
        }

        /// <summary>
        /// Test that the View - Zoom - Fit To Page menu item works 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripMenuItemWithImageTest()
        {
            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Get the imageviewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Set the fit mode to none
            imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI22727", "Could not change fit mode to none!",
                imageViewer.FitMode == FitMode.None);

            // Get the fit to page menu item
            FitToPageToolStripMenuItem fitToPage = FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_idShieldOfficeForm);

            // Click the menu item
            fitToPage.PerformClick();

            Assert.That(imageViewer.FitMode == FitMode.FitToPage);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Fit To Page menu item is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripMenuItemToggledOnWhenSelectedTest()
        {
            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the FitToWidth menu item
            FitToWidthToolStripMenuItem fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_idShieldOfficeForm);

            // Select the FitToWidth tool
            fitToWidth.PerformClick();

            // Get the FitToPage menu item
            FitToPageToolStripMenuItem fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_idShieldOfficeForm);

            // Select the FitToPage tool
            fitToPage.PerformClick();

            // Check that the menu item is checked
            Assert.That(fitToPage.Checked);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Fit To Page menu item is toggled off 
        /// when selected again.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripMenuItemTogglesOffWhenSelectedTest()
        {
            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the FitToWidth menu item
            FitToWidthToolStripMenuItem fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_idShieldOfficeForm);

            // Select the FitToWidth tool
            fitToWidth.PerformClick();

            // Get the FitToPage menu item
            FitToPageToolStripMenuItem fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_idShieldOfficeForm);

            // Select the FitToPage tool twice
            fitToPage.PerformClick();
            fitToPage.PerformClick();

            // Check that the menu item is unchecked
            Assert.That(!fitToPage.Checked);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Fit To Page menu item raises the
        /// <see cref="ImageViewer.FitModeChanged"/> and 
        /// <see cref="ImageViewer.ZoomChanged"/> events.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageToolStripMenuItemEventTest()
        {
            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open an image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of FitModeChanged and ZoomChanged events
            imageViewer.FitModeChanged += eventCounters.CountEvent<FitModeChangedEventArgs>;
            imageViewer.ZoomChanged += eventCounters.CountEvent2<ZoomChangedEventArgs>;

            // Click the FitToPageToolStripMenuItem
            FitToPageToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_idShieldOfficeForm);
            clickMe.PerformClick();

            // Check that exactly one FitModeChanged and one ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter == 1 && eventCounters.EventCounter2 == 1);

            // Click the FitToPageToolStripMenuItem again
            clickMe.PerformClick();

            // Check that exactly two FitModeChanged and two ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter == 2 && eventCounters.EventCounter2 == 2);
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
            _idShieldOfficeForm.Show();

            // Get the View - Zoom - Fit To Width menu item
            ToolStripMenuItem viewZoomFitToWidth =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Fit to &width");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Zoom - Fit To Width menu item
            ToolStripMenuItem viewZoomFitToWidth =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Fit to &width");

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
            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Get the imageviewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Set the fit mode to none
            imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI23073", "Could not change fit mode to none!",
                imageViewer.FitMode == FitMode.None);

            // Get the fit to width menu item
            FitToWidthToolStripMenuItem fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_idShieldOfficeForm);

            // Click the menu item
            fitToWidth.PerformClick();

            Assert.That(imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Test that the View - Zoom - Fit To Width menu item works 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripMenuItemWithImageTest()
        {
            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Get the imageviewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Set the fit mode to none
            imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI23072", "Could not change fit mode to none!",
                imageViewer.FitMode == FitMode.None);

            // Get the fit to width menu item
            FitToWidthToolStripMenuItem fitToWidth = 
                FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_idShieldOfficeForm);

            // Click the menu item
            fitToWidth.PerformClick();

            Assert.That(imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Fit To Width menu item is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripMenuItemToggledOnWhenSelectedTest()
        {
            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the FitToPage menu item
            FitToPageToolStripMenuItem fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_idShieldOfficeForm);

            // Select the FitToPage tool
            fitToPage.PerformClick();

            // Get the FitToWidth menu item
            FitToWidthToolStripMenuItem fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_idShieldOfficeForm);

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
            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the FitToPage menu item
            FitToPageToolStripMenuItem fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripMenuItem>(_idShieldOfficeForm);

            // Select the FitToPage tool
            fitToPage.PerformClick();

            // Get the FitToWidth menu item
            FitToWidthToolStripMenuItem fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_idShieldOfficeForm);

            // Select the FitToWidth tool twice
            fitToWidth.PerformClick();
            fitToWidth.PerformClick();

            // Check that the menu item is unchecked
            Assert.That(!fitToWidth.Checked);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Fit To Width menu item raises the
        /// <see cref="ImageViewer.FitModeChanged"/> and 
        /// <see cref="ImageViewer.ZoomChanged"/> events.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthToolStripMenuItemEventTest()
        {
            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open an image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of FitModeChanged and ZoomChanged events
            imageViewer.FitModeChanged += eventCounters.CountEvent<FitModeChangedEventArgs>;
            imageViewer.ZoomChanged += eventCounters.CountEvent2<ZoomChangedEventArgs>;

            // Click the FitToWidthToolStripMenuItem
            FitToWidthToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<FitToWidthToolStripMenuItem>(_idShieldOfficeForm);
            clickMe.PerformClick();

            // Check that exactly one FitModeChanged and one ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter == 1 && eventCounters.EventCounter2 == 1);

            // Click the FitToWidthToolStripMenuItem again
            clickMe.PerformClick();

            // Check that exactly two FitModeChanged and two ZoomChanged events were raised
            Assert.That(eventCounters.EventCounter == 2 && eventCounters.EventCounter2 == 2);
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
            _idShieldOfficeForm.Show();

            // Get the View - Zoom - Zoom In menu item
            ToolStripMenuItem viewZoomIn =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Zoom in");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Zoom - Zoom In menu item
            ToolStripMenuItem viewZoomIn =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Zoom in");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the zoom level
            double zoomLevel = imageViewer.ZoomInfo.ScaleFactor;

            // Get the Zoom In menu item
            ZoomInToolStripMenuItem zoomIn =
                FormMethods.GetFormComponent<ZoomInToolStripMenuItem>(_idShieldOfficeForm);

            // Zoom in
            zoomIn.PerformClick();

            // Check that the image zoomed in
            Assert.That(imageViewer.ZoomInfo.ScaleFactor > zoomLevel);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Zoom In menu item adds a zoom history entry.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripMenuItemAddsZoomHistoryTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the current Zoom history count
            int zoomHistoryCount = imageViewer.ZoomHistoryCount;

            // Click the ZoomInToolStripMenuItem
            ZoomInToolStripMenuItem clickMe = FormMethods.GetFormComponent<ZoomInToolStripMenuItem>(_idShieldOfficeForm);
            clickMe.PerformClick();

            // Check that exactly one zoom history record has been added
            Assert.That((zoomHistoryCount + 1) == imageViewer.ZoomHistoryCount);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Zoom In menu item raises the
        /// <see cref="ImageViewer.ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolStripMenuItemEventTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ZoomChanged events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;

            // Click the ZoomInToolStripMenuItem
            ZoomInToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<ZoomInToolStripMenuItem>(_idShieldOfficeForm);
            clickMe.PerformClick();

            // Check that exactly one ZoomChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
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
            _idShieldOfficeForm.Show();

            // Get the View - Zoom - Zoom Out menu item
            ToolStripMenuItem viewZoomOut =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Zoom out");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Zoom - Zoom Out menu item
            ToolStripMenuItem viewZoomOut =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Zoom out");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the zoom level
            double zoomLevel = imageViewer.ZoomInfo.ScaleFactor;

            // Get the Zoom Out menu item
            ZoomOutToolStripMenuItem zoomOut =
                FormMethods.GetFormComponent<ZoomOutToolStripMenuItem>(_idShieldOfficeForm);

            // Zoom out
            zoomOut.PerformClick();

            // Check that the image zoomed out
            Assert.That(imageViewer.ZoomInfo.ScaleFactor < zoomLevel);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Zoom Out menu item adds a zoom history entry.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripMenuItemAddsZoomHistoryTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the current Zoom history count
            int zoomHistoryCount = imageViewer.ZoomHistoryCount;

            // Click the ZoomOutToolStripMenuItem
            ZoomOutToolStripMenuItem clickMe = FormMethods.GetFormComponent<ZoomOutToolStripMenuItem>(_idShieldOfficeForm);
            clickMe.PerformClick();

            // Check that exactly one zoom history record has been added
            Assert.That((zoomHistoryCount + 1) == imageViewer.ZoomHistoryCount);
        }

        /// <summary>
        /// Tests whether the View - Zoom - Zoom Out menu item raises the
        /// <see cref="ImageViewer.ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolStripMenuItemEventTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ZoomChanged events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;

            // Click the ZoomOutToolStripMenuItem
            ZoomOutToolStripMenuItem clickMe =
                FormMethods.GetFormComponent<ZoomOutToolStripMenuItem>(_idShieldOfficeForm);
            clickMe.PerformClick();

            // Check that exactly one ZoomChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
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
            _idShieldOfficeForm.Show();

            // Get the View - Zoom - Zoom Previous menu item
            ToolStripMenuItem viewZoomPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Zoom previous");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Zoom - Zoom Previous menu item
            ToolStripMenuItem viewZoomPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Zoom previous");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Create a previous zoom history entry
            _idShieldOfficeForm.ImageViewer.ZoomIn();

            // Get the View - Zoom - Zoom Previous menu item
            ToolStripMenuItem viewZoomPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Zoom previous");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the zoom level
            double zoomLevelOriginal = imageViewer.ZoomInfo.ScaleFactor;

            // Create a previous zoom history entry
            imageViewer.ZoomIn();

            // Get the new zoom level
            double zoomLevelNew = imageViewer.ZoomInfo.ScaleFactor;

            // Get the View - Zoom - Zoom Previous menu item
            ToolStripMenuItem viewZoomPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Zoom previous");

            // Zoom to previous history item
            viewZoomPrevious.PerformClick();

            // Check that the zoom level changed back to original AND
            // that the original zoom level is different than new zoom level
            Assert.That(imageViewer.ZoomInfo.ScaleFactor == zoomLevelOriginal &&
                zoomLevelOriginal != zoomLevelNew);
        }

        /// <summary>
        /// Tests that the View - Zoom - Zoom Previous menu item raises the
        /// ZoomChanged event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomZoomPreviousEventTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Create a previous zoom history entry
            imageViewer.ZoomIn();

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ZoomChanged events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;

            // Click the View - Zoom - Zoom Previous menu item
            ToolStripMenuItem viewZoomPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Zoom previous");
            viewZoomPrevious.PerformClick();

            // Check that exactly one ZoomChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
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
            _idShieldOfficeForm.Show();

            // Get the View - Zoom - Zoom Next menu item
            ToolStripMenuItem viewZoomNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Zoom next");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Zoom - Zoom Next menu item
            ToolStripMenuItem viewZoomNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Zoom next");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Create a next zoom history entry
            imageViewer.ZoomIn();
            imageViewer.ZoomPrevious();

            // Get the View - Zoom - Zoom Next menu item
            ToolStripMenuItem viewZoomNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Zoom next");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Zoom in and store the zoom level
            imageViewer.ZoomIn();
            double zoomLevelIn = imageViewer.ZoomInfo.ScaleFactor;

            // Zoom previous and get the previous zoom level
            imageViewer.ZoomPrevious();
            double zoomLevelPrevious = imageViewer.ZoomInfo.ScaleFactor;

            // Get the View - Zoom - Zoom Next menu item
            ToolStripMenuItem viewZoomNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Zoom next");

            // Zoom to next history item
            viewZoomNext.PerformClick();

            // Check that the zoom level changed back to the zoomed in value AND
            // that the previous zoom level is different than new zoom level
            Assert.That(imageViewer.ZoomInfo.ScaleFactor == zoomLevelIn &&
                zoomLevelIn != zoomLevelPrevious);
        }

        /// <summary>
        /// Tests that the View - Zoom - Zoom Next menu item raises the
        /// ZoomChanged event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewZoomZoomNextEventTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Create a next zoom history entry
            imageViewer.ZoomIn();
            imageViewer.ZoomPrevious();

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of ZoomChanged events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;

            // Click the View - Zoom - Zoom Next menu item
            ToolStripMenuItem viewZoomNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Zoom next");
            viewZoomNext.PerformClick();

            // Check that exactly one ZoomChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
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
            _idShieldOfficeForm.Show();

            // Get the View - Rotate - Counterclockwise menu item
            ToolStripMenuItem viewRotateCounterclockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Rotate counterclockwise");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Rotate - Counterclockwise menu item
            ToolStripMenuItem viewRotateCounterclockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Rotate counterclockwise");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Rotate - Counterclockwise menu item
            ToolStripMenuItem viewRotateCounterclockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Rotate counterclockwise");

            // Rotate the image
            viewRotateCounterclockwise.PerformClick();

            // Check that the image rotated properly
            Assert.That(MessageBox.Show("Did the image rotate counterclockwise?",
                "Proper image rotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the View - Rotate - Counterclockwise menu item rotates  
        /// properly with visible Bates Number.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRotateCounterclockwiseRotateWithBatesNumberTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Tools - Apply Bates Number menu item
            ToolStripMenuItem toolsBates =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Apply Bates number");

            // Apply the Bates Number
            toolsBates.PerformClick();

            // Get the View - Rotate - Counterclockwise menu item
            ToolStripMenuItem viewRotateCounterclockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Rotate counterclockwise");

            // Rotate the image
            viewRotateCounterclockwise.PerformClick();

            // Check that the image rotated properly
            Assert.That(MessageBox.Show("Did the image and Bates Number rotate counterclockwise?",
                "Proper image rotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the View - Rotate - Counterclockwise menu item rotates  
        /// properly with visible redaction.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRotateCounterclockwiseRotateWithRedactionTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Edit - Redact Entire Page menu item
            ToolStripMenuItem editRedact =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Redact entire page");

            // Redact the page
            editRedact.PerformClick();

            // Get the View - Rotate - Counterclockwise menu item
            ToolStripMenuItem viewRotateCounterclockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Rotate counterclockwise");

            // Rotate the image
            viewRotateCounterclockwise.PerformClick();

            // Check that the image rotated properly
            Assert.That(MessageBox.Show("Did the image and full-page redaction rotate counterclockwise?",
                "Proper image rotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Tests whether View - Rotate - Counterclockwise menu item raises the
        /// <see cref="OrientationChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewRotateCounterclockwiseRaisesEventTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of OrientationChanged events
            imageViewer.OrientationChanged += eventCounters.CountEvent<OrientationChangedEventArgs>;

            // Get the View - Rotate - Counterclockwise menu item
            ToolStripMenuItem viewRotateCounterclockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Rotate counterclockwise");

            // Rotate the page
            viewRotateCounterclockwise.PerformClick();

            // Check that exactly one OrientationChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Test that the View - Rotate - Counterclockwise menu item rotates  
        /// only the active page.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRotateCounterclockwiseRotatesActivePageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the View - Rotate - Counterclockwise menu item
            ToolStripMenuItem viewRotateCounterclockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Rotate counterclockwise");

            // Rotate the image
            viewRotateCounterclockwise.PerformClick();

            // Move to page 2 and refresh the page
            imageViewer.PageNumber = 2;
            _idShieldOfficeForm.Refresh();

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
            _idShieldOfficeForm.Show();

            // Get the View - Rotate - Clockwise menu item
            ToolStripMenuItem viewRotateClockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Rotate clockwise");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Rotate - Clockwise menu item
            ToolStripMenuItem viewRotateClockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Rotate clockwise");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Rotate - Clockwise menu item
            ToolStripMenuItem viewRotateClockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Rotate clockwise");

            // Rotate the image
            viewRotateClockwise.PerformClick();

            // Check that the image rotated properly
            Assert.That(MessageBox.Show("Did the image rotate clockwise?",
                "Proper image rotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the View - Rotate - Clockwise menu item rotates  
        /// properly with visible Bates Number.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRotateClockwiseRotateWithBatesNumberTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Tools - Apply Bates Number menu item
            ToolStripMenuItem toolsBates =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Apply Bates number");

            // Apply the Bates Number
            toolsBates.PerformClick();

            // Get the View - Rotate - Clockwise menu item
            ToolStripMenuItem viewRotateClockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Rotate clockwise");

            // Rotate the image
            viewRotateClockwise.PerformClick();

            // Check that the image rotated properly
            Assert.That(MessageBox.Show("Did the image and Bates Number rotate clockwise?",
                "Proper image rotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the View - Rotate - Clockwise menu item rotates  
        /// properly with visible redaction.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRotateClockwiseRotateWithRedactionTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Edit - Redact Entire Page menu item
            ToolStripMenuItem editRedact =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Redact entire page");

            // Redact the page
            editRedact.PerformClick();

            // Get the View - Rotate - Clockwise menu item
            ToolStripMenuItem viewRotateClockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Rotate clockwise");

            // Rotate the image
            viewRotateClockwise.PerformClick();

            // Check that the image rotated properly
            Assert.That(MessageBox.Show("Did the image and full-page redaction rotate clockwise?",
                "Proper image rotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Tests whether View - Rotate - Clockwise menu item raises the
        /// <see cref="OrientationChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRotateClockwiseRaisesEventTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of OrientationChanged events
            imageViewer.OrientationChanged += eventCounters.CountEvent<OrientationChangedEventArgs>;

            // Get the View - Rotate - Clockwise menu item
            ToolStripMenuItem viewRotateClockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Rotate clockwise");

            // Rotate the page
            viewRotateClockwise.PerformClick();

            // Check that exactly one OrientationChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Test that the View - Rotate - Clockwise menu item rotates  
        /// only the active page.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRotateClockwiseRotatesActivePageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the View - Rotate - Clockwise menu item
            ToolStripMenuItem viewRotateClockwise =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Rotate clockwise");

            // Rotate the image
            viewRotateClockwise.PerformClick();

            // Move to page 2 and refresh the page
            imageViewer.PageNumber = 2;
            _idShieldOfficeForm.Refresh();

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
            _idShieldOfficeForm.Show();

            // Get the View - Goto Page - First Page menu item
            ToolStripMenuItem viewGoToFirst =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "First page");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Go To Page - First Page menu item
            ToolStripMenuItem viewGoToFirst =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "First page");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Move to page 2
            imageViewer.PageNumber = 2;

            // Get the View - Go To Page - First Page menu item
            ToolStripMenuItem viewGoToFirst =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "First page");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Move to page 2
            imageViewer.PageNumber = 2;

            // Get the View - Go To Page - First Page menu item
            ToolStripMenuItem viewGoToFirst =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "First page");

            // Move back to the first page
            viewGoToFirst.PerformClick();

            // Check that the first page is active
            Assert.That(imageViewer.PageNumber == 1);
        }

        /// <summary>
        /// Tests whether View - Go To Page - First Page menu item raises the
        /// <see cref="PageChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageFirstPageRaisesEventTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Move to page 2
            imageViewer.PageNumber = 2;

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of PageChanged events
            imageViewer.PageChanged += eventCounters.CountEvent<PageChangedEventArgs>;

            // Get the View - Go To Page - First Page menu item
            ToolStripMenuItem viewGoToFirst =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "First page");

            // Move back to page 1
            viewGoToFirst.PerformClick();

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
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
            _idShieldOfficeForm.Show();

            // Get the View - Goto Page - Previous Page menu item
            ToolStripMenuItem viewGoToPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Previous page");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Go To Page - Previous Page menu item
            ToolStripMenuItem viewGoToPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Previous page");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Move to page 2
            imageViewer.PageNumber = 2;

            // Get the View - Go To Page - Previous Page menu item
            ToolStripMenuItem viewGoToPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Previous page");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Move to page 2
            imageViewer.PageNumber = 2;

            // Get the View - Go To Page - Previous Page menu item
            ToolStripMenuItem viewGoToPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Previous page");

            // Move to the previous page
            viewGoToPrevious.PerformClick();

            // Check that the proper page is active
            Assert.That(imageViewer.PageNumber == 1);
        }

        /// <summary>
        /// Tests whether View - Go To Page - Previous Page menu item raises the
        /// <see cref="PageChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPagePreviousPageRaisesEventTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Move to page 2
            imageViewer.PageNumber = 2;

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of PageChanged events
            imageViewer.PageChanged += eventCounters.CountEvent<PageChangedEventArgs>;

            // Get the View - Go To Page - Previous Page menu item
            ToolStripMenuItem viewGoToPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Previous page");

            // Move back to page 1
            viewGoToPrevious.PerformClick();

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
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
            _idShieldOfficeForm.Show();

            // Get the View - Goto Page - Page Number menu item
            ToolStripMenuItem viewGoToNumber =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Page n&umber...");

            // Check that the menu item is disabled
            Assert.That(!viewGoToNumber.Enabled);
        }

        /// <summary>
        /// Test that the View - Goto Page - Page Number menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageNumberEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Go To Page - Page Number menu item
            ToolStripMenuItem viewGoToNumber =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Page n&umber...");

            // Check that the menu item is enabled
            Assert.That(viewGoToNumber.Enabled);
        }

        /// <summary>
        /// Tests whether View - Goto Page - Page Number menu item raises the
        /// <see cref="PageChanged"/> event.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewGoToPagePageNumberRaisesEventTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of PageChanged events
            imageViewer.PageChanged += eventCounters.CountEvent<PageChangedEventArgs>;

            // Prompt the user to change the page
            MessageBox.Show("Close this message box and select a new page number.",
                "Select new page", MessageBoxButtons.OK, MessageBoxIcon.None,
                MessageBoxDefaultButton.Button1, 0);

            // Get the View - Goto Page - Page Number menu item
            ToolStripMenuItem viewPageNumber =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm,
                "Page n&umber...");

            // Display the Page Number dialog
            viewPageNumber.PerformClick();

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
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
            _idShieldOfficeForm.Show();

            // Get the View - Goto Page - Next Page menu item
            ToolStripMenuItem viewGoToNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Next page");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Move to the last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Get the View - Go To Page - Next Page menu item
            ToolStripMenuItem viewGoToNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Next page");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Go To Page - Next Page menu item
            ToolStripMenuItem viewGoToNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Next page");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image - starts on page 1
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Go To Page - Next Page menu item
            ToolStripMenuItem viewGoToNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Next page");

            // Move to the next page
            viewGoToNext.PerformClick();

            // Check that page 2 is active
            Assert.That(imageViewer.PageNumber == 2);
        }

        /// <summary>
        /// Tests whether View - Go To Page - Next Page menu item raises the
        /// <see cref="PageChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageNextPageRaisesEventTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of PageChanged events
            imageViewer.PageChanged += eventCounters.CountEvent<PageChangedEventArgs>;

            // Get the View - Go To Page - Next Page menu item
            ToolStripMenuItem viewGoToNext =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Next page");

            // Move to page 2
            viewGoToNext.PerformClick();

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
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
            _idShieldOfficeForm.Show();

            // Get the View - Goto Page - Last Page menu item
            ToolStripMenuItem viewGoToLast =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Last page");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Move to the last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Get the View - Go To Page - Next Page menu item
            ToolStripMenuItem viewGoToLast =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Last page");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Go To Page - Next Page menu item
            ToolStripMenuItem viewGoToLast =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Last page");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Go To Page - Next Page menu item
            ToolStripMenuItem viewGoToLast =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Last page");

            // Move to the last page
            viewGoToLast.PerformClick();

            // Check that the last page is active
            Assert.That(imageViewer.PageNumber == imageViewer.PageCount);
        }

        /// <summary>
        /// Tests whether View - Go To Page - Last Page menu item raises the
        /// <see cref="PageChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewGoToPageLastPageRaisesEventTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Count the number of PageChanged events
            imageViewer.PageChanged += eventCounters.CountEvent<PageChangedEventArgs>;

            // Get the View - Go To Page - Last Page menu item
            ToolStripMenuItem viewGoToLast =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Last page");

            // Move to the last page
            viewGoToLast.PerformClick();

            // Check that exactly one PageChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion View Go To Page Last Page

        #region View Tiles Previous Tile

        /// <summary>
        /// Test that the View - Tiles - Previous Tile menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesPreviousTileDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the View - Tiles - Previous Tile menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<PreviousTileToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is disabled
            Assert.That(!viewPrevious.Enabled);
        }

        /// <summary>
        /// Test that the View - Tiles - Previous Tile menu item is disabled 
        /// when on the first tile - at the beginning of the first page of an 
        /// open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesPreviousTileDisabledOnFirstTileTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Tiles - Previous Tile menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<PreviousTileToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is disabled
            Assert.That(!viewPrevious.Enabled);
        }

        /// <summary>
        /// Test that the View - Tiles - Previous Tile menu item is enabled 
        /// when not on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesPreviousTileEnabledNotOnFirstPageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Move to page 2
            imageViewer.PageNumber = 2;

            // Get the View - Tiles - Previous Tile menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<PreviousTileToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is enabled
            Assert.That(viewPrevious.Enabled);
        }

        /// <summary>
        /// Test that the View - Tiles - Previous Tile menu item enabled state 
        /// is refreshed after rotation.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesPreviousTileRefreshEnabledAfterRotationTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Fit the top of the page in view
            imageViewer.FitMode = FitMode.FitToWidth;
            imageViewer.FitMode = FitMode.None;

            // Get the View - Tiles - Previous Tile menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "Previous tile");

            // Get initial enabled state
            bool initialState = viewPrevious.Enabled;

            // Rotate the image clockwise
            imageViewer.Rotate(90);

            // Check that the menu item was not enabled and now is enabled
            Assert.That(!initialState && viewPrevious.Enabled);
        }

        /// <summary>
        /// Test that the View - Tiles - Previous Tile menu item preserves 
        /// Fit To Page mode.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesPreviousTilePreservesFitToPageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Set Fit To Page mode
            imageViewer.FitMode = FitMode.FitToPage;

            // Move to page 2
            imageViewer.PageNumber = 2;

            // Get the View - Tiles - Previous Tile menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<PreviousTileToolStripMenuItem>(_idShieldOfficeForm);

            // Move to previous tile
            viewPrevious.PerformClick();

            // Check that Fit mode is unchanged
            Assert.That(imageViewer.FitMode == FitMode.FitToPage);
        }

        /// <summary>
        /// Test that the View - Tiles - Previous Tile menu item preserves 
        /// Fit To Width mode.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesPreviousTilePreservesFitToWidthTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Set Fit To Width mode
            imageViewer.FitMode = FitMode.FitToWidth;

            // Move to page 2
            imageViewer.PageNumber = 2;

            // Get the View - Tiles - Previous Tile menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<PreviousTileToolStripMenuItem>(_idShieldOfficeForm);

            // Move to previous tile
            viewPrevious.PerformClick();

            // Check that Fit mode is unchanged
            Assert.That(imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Tests whether View - Tiles - Previous Tile menu item raises the
        /// <see cref="ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesPreviousTileRaisesZoomEventTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Zoom in several steps
            ZoomInToolStripButton zoomIn = 
                FormMethods.GetFormComponent<ZoomInToolStripButton>(_idShieldOfficeForm);
            zoomIn.PerformClick();
            zoomIn.PerformClick();
            zoomIn.PerformClick();

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Start counting the number of ZoomChanged events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;

            // Get the View - Tiles - Previous Tile menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<PreviousTileToolStripMenuItem>(_idShieldOfficeForm);

            // Move to the previous tile
            viewPrevious.PerformClick();

            // Check that exactly one ZoomChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether View - Tiles - Previous Tile menu item raises the
        /// <see cref="PageChanged"/> event when the page changes.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesPreviousTileRaisesPageEventTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Move to page 2
            imageViewer.PageNumber = 2;

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Start counting the number of PageChanged events
            imageViewer.PageChanged += eventCounters.CountEvent<PageChangedEventArgs>;

            // Get the View - Tiles - Previous Tile menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<PreviousTileToolStripMenuItem>(_idShieldOfficeForm);

            // Move to the previous tile
            viewPrevious.PerformClick();

            // Check that exactly one ZoomChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion View Tiles Previous Tile

        #region View Tiles Next Tile

        /// <summary>
        /// Test that the View - Tiles - Next Tile menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesNextTileDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the View - Tiles - Next Tile menu item
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextTileToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is disabled
            Assert.That(!viewNext.Enabled);
        }

        /// <summary>
        /// Test that the View - Tiles - Next Tile menu item is disabled 
        /// when on the last tile - at the end of the last page of an 
        /// open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesNextTileDisabledOnLastTileTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Set Fit To Page mode
            imageViewer.FitMode = FitMode.FitToPage;

            // Move to the last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Get the View - Tiles - Next Tile menu item
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextTileToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is disabled
            Assert.That(!viewNext.Enabled);
        }

        /// <summary>
        /// Test that the View - Tiles - Next Tile menu item is enabled 
        /// when not on the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesNextTileEnabledNotOnLastPageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Tiles - Next Tile menu item
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextTileToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is enabled
            Assert.That(viewNext.Enabled);
        }

        /// <summary>
        /// Test that the View - Tiles - Next Tile menu item enabled state 
        /// is refreshed after rotation.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesNextTileRefreshEnabledAfterRotationTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Set Fit To Width mode
            imageViewer.FitMode = FitMode.FitToWidth;

            // Move to the last page
            imageViewer.PageNumber = imageViewer.PageCount;

            // Get the View - Tiles - Next Tile menu item
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextTileToolStripMenuItem>(_idShieldOfficeForm);

            // Move to the last tile
            viewNext.PerformClick();
            viewNext.PerformClick();
            viewNext.PerformClick();
            viewNext.PerformClick();

            // Get initial enabled state
            bool initialState = viewNext.Enabled;

            // Rotate the image clockwise
            imageViewer.Rotate(90);

            // Check that the menu item was not enabled and now is enabled
            Assert.That(!initialState && viewNext.Enabled);
        }

        /// <summary>
        /// Test that the View - Tiles - Next Tile menu item preserves 
        /// Fit To Page mode.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesNextTilePreservesFitToPageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Set Fit To Page mode
            imageViewer.FitMode = FitMode.FitToPage;

            // Get the View - Tiles - Next Tile menu item
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextTileToolStripMenuItem>(_idShieldOfficeForm);

            // Move to Next tile
            viewNext.PerformClick();

            // Check that Fit mode is unchanged
            Assert.That(imageViewer.FitMode == FitMode.FitToPage);
        }

        /// <summary>
        /// Test that the View - Tiles - Next Tile menu item preserves 
        /// Fit To Width mode.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesNextTilePreservesFitToWidthTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Set Fit To Width mode
            imageViewer.FitMode = FitMode.FitToWidth;

            // Get the View - Tiles - Next Tile menu item
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextTileToolStripMenuItem>(_idShieldOfficeForm);

            // Move to next tile
            viewNext.PerformClick();

            // Check that Fit mode is unchanged
            Assert.That(imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Tests whether View - Tiles - Next Tile menu item raises the
        /// <see cref="ZoomChanged"/> event.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesNextTileRaisesZoomEventTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Zoom in several steps
            ZoomInToolStripButton zoomIn =
                FormMethods.GetFormComponent<ZoomInToolStripButton>(_idShieldOfficeForm);
            zoomIn.PerformClick();
            zoomIn.PerformClick();
            zoomIn.PerformClick();

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Start counting the number of ZoomChanged events
            imageViewer.ZoomChanged += eventCounters.CountEvent<ZoomChangedEventArgs>;

            // Get the View - Tiles - Next Tile menu item
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextTileToolStripMenuItem>(_idShieldOfficeForm);

            // Move to the next tile
            viewNext.PerformClick();

            // Check that exactly one ZoomChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        /// <summary>
        /// Tests whether View - Tiles - Next Tile menu item raises the
        /// <see cref="PageChanged"/> event when the page changes.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewTilesNextTileRaisesPageEventTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Declare event counters class for counting the events
            EventCounters eventCounters = new EventCounters();

            // Start counting the number of PageChanged events
            imageViewer.PageChanged += eventCounters.CountEvent<PageChangedEventArgs>;

            // Get the View - Tiles - Next Tile menu item
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextTileToolStripMenuItem>(_idShieldOfficeForm);

            // Move to the next tile
            viewNext.PerformClick();

            // Check that exactly one ZoomChanged event was raised
            Assert.That(eventCounters.EventCounter == 1);
        }

        #endregion View Tiles Next Tile

        #region View Previous Redaction

        /// <summary>
        /// Test that the View - Redaction/Object - Previous Layer Object menu item 
        /// is disabled with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewRedactionPreviousDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the View - Redaction/Object - Previous Layer Object menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<PreviousLayerObjectToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is disabled
            Assert.That(!viewPrevious.Enabled);
        }

        /// <summary>
        /// Test that the View - Redaction/Object - Previous Layer Object menu item 
        /// is disabled when the first object is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewRedactionPreviousDisabledOnFirstObjectTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Apply Bates Number button and apply the number
            ToolStripButton applyBatesNumberButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm, "Apply Bates number");
            applyBatesNumberButton.PerformClick();

            // Get the View - Redaction/Object - Next Layer Object menu item
            // and move to the Next object
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextLayerObjectToolStripMenuItem>(_idShieldOfficeForm);
            viewNext.PerformClick();

            // Get the View - Redaction/Object - Previous Layer Object menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<PreviousLayerObjectToolStripMenuItem>(_idShieldOfficeForm);

            // Move back to the first layer object
            viewPrevious.PerformClick();

            // Check that the menu item is now disabled
            Assert.That(!viewPrevious.Enabled);
        }

        /// <summary>
        /// Test that the View - Redaction/Object - Previous Layer Object menu item 
        /// is disabled when all Layers are unchecked.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRedactionPreviousDisabledWithoutLayersTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Apply Bates Number button and apply the number
            ToolStripButton applyBatesNumberButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm, "Apply Bates number");
            applyBatesNumberButton.PerformClick();

            // Get the View - Redaction/Object - Next Layer Object menu item
            // and move to the Next object
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextLayerObjectToolStripMenuItem>(_idShieldOfficeForm);
            viewNext.PerformClick();

            // Get the View - Redaction/Object - Previous Layer Object menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<PreviousLayerObjectToolStripMenuItem>(_idShieldOfficeForm);

            // Ask user to turn off all layers
            ShowModelessInstructionsAndWait(new string[] {
                "Click on the ID Shield Office title bar to give it focus.",
                "Uncheck each of the types of Layer objects.",
                "Click okay to close this dialog."});

            // Check that the menu item is now disabled
            Assert.That(!viewPrevious.Enabled);
        }

        /// <summary>
        /// Test that the View - Redaction/Object - Previous Layer Object menu item 
        /// is enabled with active Clues layer and a previous clue.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRedactionPreviousEnabledWithClueTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ask user to Find SSNs and select the second clue
            ShowModelessInstructionsAndWait(new string[] {
                "Click on the ID Shield Office title bar to give it focus.",
                "Confirm that the Clues layer is checked.",
                "Use the Data Types finder to find Social Security Numbers.",
                "Close the dialog and select the second Clue object.",
                "Click okay to close this dialog."});

            // Get the View - Redaction/Object - Previous Layer Object menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<PreviousLayerObjectToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is now enabled
            Assert.That(viewPrevious.Enabled);
        }

        /// <summary>
        /// Test that the View - Redaction/Object - Previous Layer Object menu item 
        /// is disabled with inactive Clues layer and a previous clue.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRedactionPreviousDisabledWithClueTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ask user to Find SSNs, select the second clue and uncheck the Clues layer
            ShowModelessInstructionsAndWait(new string[] {
                "Click on the ID Shield Office title bar to give it focus.",
                "Use the Data Types finder to find Social Security Numbers.",
                "Close the dialog and select the second Clue object.",
                "Uncheck the Clues layer.",
                "Click okay to close this dialog."});

            // Get the View - Redaction/Object - Previous Layer Object menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<PreviousLayerObjectToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is now disabled
            Assert.That(!viewPrevious.Enabled);
        }

        /// <summary>
        /// Test that the View - Redaction/Object - Previous Layer Object menu item 
        /// is enabled with active Redaction layer and a previous redaction.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRedactionPreviousEnabledWithRedactionTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ask user to Find SSNs, uncheck the Clues layer and select the second redaction
            ShowModelessInstructionsAndWait(new string[] {
                "Click on the ID Shield Office title bar to give it focus.",
                "Uncheck the Clues layer.",
                "Use the Data Types finder to find Social Security Numbers.",
                "Click Redact All.",
                "Close the dialog and select the second Redaction object.",
                "Click okay to close this dialog."});

            // Get the View - Redaction/Object - Previous Layer Object menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<PreviousLayerObjectToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is now enabled
            Assert.That(viewPrevious.Enabled);
        }

        /// <summary>
        /// Test that the View - Redaction/Object - Previous Layer Object menu item 
        /// is disabled with inactive Redaction layer and a previous redaction.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRedactionPreviousDisabledWithRedactionTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ask user to Find SSNs, uncheck the Clues layer, select the 
            // second redaction object and uncheck the Redactions layer
            ShowModelessInstructionsAndWait(new string[] {
                "Click on the ID Shield Office title bar to give it focus.",
                "Uncheck the Clues layer.",
                "Use the Data Types finder to find Social Security Numbers.",
                "Click Redact All.",
                "Close the dialog and select the second Redaction object.",
                "Uncheck the Redactions layer.",
                "Click okay to close this dialog."});

            // Get the View - Redaction/Object - Previous Layer Object menu item
            ToolStripMenuItem viewPrevious =
                FormMethods.GetFormComponent<PreviousLayerObjectToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is now disabled
            Assert.That(!viewPrevious.Enabled);
        }

        #endregion View Previous Redaction

        #region View Next Redaction

        /// <summary>
        /// Test that the View - Redaction/Object - Next Layer Object menu item 
        /// is disabled with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewRedactionNextDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the View - Redaction/Object - Next Layer Object menu item
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextLayerObjectToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is disabled
            Assert.That(!viewNext.Enabled);
        }

        /// <summary>
        /// Test that the View - Redaction/Object - Next Layer Object menu item 
        /// is disabled when the last object is selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewRedactionNextDisabledOnLastObjectTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Apply Bates Number button and apply the number
            ToolStripButton applyBatesNumberButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm, "Apply Bates number");
            applyBatesNumberButton.PerformClick();

            // Get the View - Redaction/Object - Next Layer Object menu item
            // and move to the Next object
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextLayerObjectToolStripMenuItem>(_idShieldOfficeForm);
            viewNext.PerformClick();

            // Check that the menu item is now disabled
            Assert.That(!viewNext.Enabled);
        }

        /// <summary>
        /// Test that the View - Redaction/Object - Next Layer Object menu item 
        /// is disabled when all Layers are unchecked.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRedactionNextDisabledWithoutLayersTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Apply Bates Number button and apply the number
            ToolStripButton applyBatesNumberButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm, "Apply Bates number");
            applyBatesNumberButton.PerformClick();

            // Get the View - Redaction/Object - Next Layer Object menu item
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextLayerObjectToolStripMenuItem>(_idShieldOfficeForm);

            // Ask user to turn off all layers
            ShowModelessInstructionsAndWait(new string[] {
                "Click on the ID Shield Office title bar to give it focus.",
                "Uncheck each of the types of Layer objects.",
                "Click okay to close this dialog."});

            // Check that the menu item is now disabled
            Assert.That(!viewNext.Enabled);
        }

        /// <summary>
        /// Test that the View - Redaction/Object - Next Layer Object menu item 
        /// is enabled with active Clues layer and a next clue.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRedactionNextEnabledWithClueTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ask user to Find SSNs and select the first clue
            ShowModelessInstructionsAndWait(new string[] {
                "Click on the ID Shield Office title bar to give it focus.",
                "Confirm that the Clues layer is checked.",
                "Use the Data Types finder to find Social Security Numbers.",
                "Close the dialog and select the first Clue object.",
                "Click okay to close this dialog."});

            // Get the View - Redaction/Object - Next Layer Object menu item
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextLayerObjectToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is now enabled
            Assert.That(viewNext.Enabled);
        }

        /// <summary>
        /// Test that the View - Redaction/Object - Next Layer Object menu item 
        /// is disabled with inactive Clues layer and a next clue.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRedactionNextDisabledWithClueTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ask user to Find SSNs, select the first clue and uncheck the Clues layer
            ShowModelessInstructionsAndWait(new string[] {
                "Click on the ID Shield Office title bar to give it focus.",
                "Use the Data Types finder to find Social Security Numbers.",
                "Close the dialog and select the first Clue object.",
                "Uncheck the Clues layer.",
                "Click okay to close this dialog."});

            // Get the View - Redaction/Object - Next Layer Object menu item
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextLayerObjectToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is now disabled
            Assert.That(!viewNext.Enabled);
        }

        /// <summary>
        /// Test that the View - Redaction/Object - Next Layer Object menu item 
        /// is enabled with active Redaction layer and a next redaction.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRedactionNextEnabledWithRedactionTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ask user to Find SSNs, uncheck the Clues layer, Redact All and 
            // select the first redaction
            ShowModelessInstructionsAndWait(new string[] {
                "Click on the ID Shield Office title bar to give it focus.",
                "Uncheck the Clues layer.",
                "Use the Data Types finder to find Social Security Numbers.",
                "Click Redact All.",
                "Close the dialog and select the first Redaction object.",
                "Click okay to close this dialog."});

            // Get the View - Redaction/Object - Next Layer Object menu item
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextLayerObjectToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is now enabled
            Assert.That(viewNext.Enabled);
        }

        /// <summary>
        /// Test that the View - Redaction/Object - Next Layer Object menu item 
        /// is disabled with inactive Redaction layer and a next redaction.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ViewRedactionNextDisabledWithRedactionTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ask user to Find SSNs, uncheck the Clues layer, select the 
            // first redaction object and uncheck the Redactions layer
            ShowModelessInstructionsAndWait(new string[] {
                "Click on the ID Shield Office title bar to give it focus.",
                "Uncheck the Clues layer.",
                "Use the Data Types finder to find Social Security Numbers.",
                "Click Redact All.",
                "Close the dialog and select the first Redaction object.",
                "Uncheck the Redactions layer.",
                "Click okay to close this dialog."});

            // Get the View - Redaction/Object - Next Layer Object menu item
            ToolStripMenuItem viewNext =
                FormMethods.GetFormComponent<NextLayerObjectToolStripMenuItem>(_idShieldOfficeForm);

            // Check that the menu item is now disabled
            Assert.That(!viewNext.Enabled);
        }

        #endregion View Next Redaction

        #region View Layers Window

        /// <summary>
        /// Test that the View - Layers Window menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewLayersWindowEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the View - Layers Window menu item
            ToolStripMenuItem viewLayers =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Layers window");

            // Check that the menu item is enabled
            Assert.That(viewLayers.Enabled);
        }

        /// <summary>
        /// Test that the View - Layers Window menu item is enabled 
        /// with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewLayersWindowEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Layers Window menu item
            ToolStripMenuItem viewLayers =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Layers window");

            // Check that the menu item is enabled
            Assert.That(viewLayers.Enabled);
        }

        /// <summary>
        /// Test that the View - Layers Window menu item shows and hides the 
        /// Layers Window.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_LayersWindowShowsAndHidesWindowTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Get the View - Layers Window menu item
            ToolStripMenuItem viewLayers =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Layers window");

            // Click the menu item, pause, click the menu item again
            _idShieldOfficeForm.Refresh();
            System.Threading.Thread.Sleep(1000);
            viewLayers.PerformClick();

            _idShieldOfficeForm.Refresh();
            System.Threading.Thread.Sleep(1000);
            viewLayers.PerformClick();
            _idShieldOfficeForm.Refresh();

            // Check that the window visibility changed twice
            Assert.That(
                MessageBox.Show("Did the Layers Window change visibility and then change back?",
                "Check Layers Window visibility",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the View - Layers Window menu item is checked and unchecked 
        /// when window is displayed and hidden.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_LayersWindowMenuItemCheckedUncheckedTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Get the View - Layers Window menu item
            ToolStripMenuItem viewLayers =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm,
                "&Layers window");

            // Click the menu item, pause, click the menu item again
            _idShieldOfficeForm.Refresh();
            System.Threading.Thread.Sleep(1000);
            viewLayers.PerformClick();
            bool windowVisible1 = viewLayers.Checked;

            _idShieldOfficeForm.Refresh();
            System.Threading.Thread.Sleep(1000);
            viewLayers.PerformClick();
            _idShieldOfficeForm.Refresh();
            bool windowVisible2 = viewLayers.Checked;

            // Get the Layers Window button
            ToolStripButton layersWindowButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm, "Show or hide layers");
            bool buttonState = layersWindowButton.Checked;

            // Check that the menu item checked state changed and that 
            // the toolbar button matches menu item state
            Assert.That(windowVisible1 != windowVisible2 &&
                windowVisible2 == buttonState);
        }

        #endregion View Layers Window

        #region View Object Properties Window

        /// <summary>
        /// Test that the View - Object Properties Window menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewObjectPropertiesWindowEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the View - Object Properties Window menu item
            ToolStripMenuItem viewProperties =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Object properties window");

            // Check that the menu item is enabled
            Assert.That(viewProperties.Enabled);
        }

        /// <summary>
        /// Test that the View - Object Properties Window menu item is enabled 
        /// with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ViewObjectPropertiesWindowEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the View - Object Properties Window menu item
            ToolStripMenuItem viewProperties =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Object properties window");

            // Check that the menu item is enabled
            Assert.That(viewProperties.Enabled);
        }

        /// <summary>
        /// Test that the View - Object Properties Window menu item shows and hides the 
        /// Properties Window.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_PropertiesWindowShowsAndHidesWindowTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Get the View - Object Properties Window menu item
            ToolStripMenuItem viewProperties =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Object properties window");

            // Click the menu item, pause, click the menu item again
            _idShieldOfficeForm.Refresh();
            System.Threading.Thread.Sleep(1000);
            viewProperties.PerformClick();

            _idShieldOfficeForm.Refresh();
            System.Threading.Thread.Sleep(1000);
            viewProperties.PerformClick();
            _idShieldOfficeForm.Refresh();

            // Check that the window visibility changed twice
            Assert.That(
                MessageBox.Show("Did the Properties Window change visibility and then change back?",
                "Check Properties Window visibility",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the View - Object Properties Window menu item is checked and unchecked 
        /// when window is displayed and hidden.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PropertiesWindowMenuItemCheckedUncheckedTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Get the View - Object Properties Window menu item
            ToolStripMenuItem viewProperties =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Object properties window");

            // Click the menu item, pause, click the menu item again
            _idShieldOfficeForm.Refresh();
            System.Threading.Thread.Sleep(1000);
            viewProperties.PerformClick();
            bool windowVisible1 = viewProperties.Checked;

            _idShieldOfficeForm.Refresh();
            System.Threading.Thread.Sleep(1000);
            viewProperties.PerformClick();
            _idShieldOfficeForm.Refresh();
            bool windowVisible2 = viewProperties.Checked;

            // Get the Properties Window button
            ToolStripButton propertiesWindowButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm, "Show or hide object properties");
            bool buttonState = propertiesWindowButton.Checked;

            // Check that the menu item checked state changed and that 
            // the toolbar button matches menu item state
            Assert.That(windowVisible1 != windowVisible2 && 
                windowVisible2 == buttonState);
        }

        #endregion View Object Properties Window

        #region Tools Pan

        /// <summary>
        /// Test that the Tools - Pan menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsPanDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Tools - Pan menu item
            ToolStripMenuItem toolsPan =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "P&an");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Tools - Pan menu item
            ToolStripMenuItem toolsPan =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "P&an");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Pan menu item
            ToolStripMenuItem toolsPan =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "P&an");

            // Select the Pan tool
            toolsPan.PerformClick();

            // Get the Pan button
            PanToolStripButton pan =
                FormMethods.GetFormComponent<PanToolStripButton>(_idShieldOfficeForm);

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Pan menu item
            ToolStripMenuItem toolsPan =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "P&an");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Pan menu item
            ToolStripMenuItem toolsPan =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "P&an");

            // Select the Pan tool
            toolsPan.PerformClick();

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Zoom window");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Pan menu item
            ToolStripMenuItem toolsPan =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "P&an");

            // Select the Pan tool
            toolsPan.PerformClick();

            // Get the Zoom window toolbar button
            ZoomWindowToolStripButton toolsZoom =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_idShieldOfficeForm);

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Check that the Pan menu item is unchecked
            Assert.That(!toolsPan.Checked);
        }

        #endregion Tools Pan

        #region Tools Zoom

        /// <summary>
        /// Test that the Tools - Zoom menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsZoomDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Zoom window");

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
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Zoom window");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Zoom window");

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Get the Pan button
            ZoomWindowToolStripButton zoom =
                FormMethods.GetFormComponent<ZoomWindowToolStripButton>(_idShieldOfficeForm);

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Zoom window");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Zoom window");

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Get the Tools - Pan menu item
            ToolStripMenuItem toolsPan =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "P&an");

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
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Zoom menu item
            ToolStripMenuItem toolsZoom =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Zoom window");

            // Select the Zoom tool
            toolsZoom.PerformClick();

            // Get the Pan toolbar button
            PanToolStripButton toolsPan =
                FormMethods.GetFormComponent<PanToolStripButton>(_idShieldOfficeForm);

            // Select the Pan tool
            toolsPan.PerformClick();

            // Check that the Zoom menu item is unchecked
            Assert.That(!toolsZoom.Checked);
        }

        #endregion Tools Zoom

        #region Tools Angular Redaction

        /// <summary>
        /// Test that the Tools - Angular Redaction menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsAngularRedactionDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Tools - Angular Redaction menu item
            ToolStripMenuItem toolsAngular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "A&ngular redaction");

            // Check that the menu item is disabled
            Assert.That(!toolsAngular.Enabled);
        }

        /// <summary>
        /// Test that the Tools - Angular Redaction menu item is enabled 
        /// with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsAngularRedactionEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Tools - Angular Redaction menu item
            ToolStripMenuItem toolsAngular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "A&ngular redaction");

            // Check that the menu item is enabled
            Assert.That(toolsAngular.Enabled);
        }

        /// <summary>
        /// Tests whether the Tools - Angular Redaction menu item selects the  
        /// Angular Redaction toolbar button when selected.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ToolsAngularRedactionSelectsToolBarButtonTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Angular Redaction menu item
            ToolStripMenuItem toolsAngular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "A&ngular redaction");

            // Select the Angular Redaction tool
            toolsAngular.PerformClick();

            // Check that the proper toolbar button is visible
            Assert.That(MessageBox.Show("Is the Angular Redaction toolbar button visible?",
                "Check button visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Tests whether the Tools - Angular Redaction menu item is checked when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsAngularRedactionSetsMenuItemTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Angular Redaction menu item
            ToolStripMenuItem toolsAngular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "A&ngular redaction");

            // Select the Angular Redaction tool
            toolsAngular.PerformClick();

            // Check that the menu item is checked
            Assert.That(toolsAngular.Checked);
        }

        #endregion Tools Angular Redaction

        #region Tools Rectangular Redaction

        /// <summary>
        /// Test that the Tools - Rectangular Redaction menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsRectangularRedactionDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Tools - Rectangular Redaction menu item
            ToolStripMenuItem toolsRectangular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Rectangular redaction");

            // Check that the menu item is disabled
            Assert.That(!toolsRectangular.Enabled);
        }

        /// <summary>
        /// Test that the Tools - Rectangular Redaction menu item is enabled 
        /// with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsRectangularRedactionEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Tools - Rectangular Redaction menu item
            ToolStripMenuItem toolsRectangular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Rectangular redaction");

            // Check that the menu item is enabled
            Assert.That(toolsRectangular.Enabled);
        }

        /// <summary>
        /// Tests whether the Tools - Rectangular Redaction menu item selects the  
        /// Rectangular Redaction toolbar button when selected.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ToolsRectangularRedactionSelectsToolBarButtonTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Rectangular Redaction menu item
            ToolStripMenuItem toolsRectangular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Rectangular redaction");

            // Select the Rectangular Redaction tool
            toolsRectangular.PerformClick();

            // Check that the proper toolbar button is visible
            Assert.That(MessageBox.Show("Is the Rectangular Redaction toolbar button visible?",
                "Check button visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Tests whether the Tools - Rectangular Redaction menu item is checked 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsRectangularRedactionSetsMenuItemTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Rectangular Redaction menu item
            ToolStripMenuItem toolsRectangular =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Rectangular redaction");

            // Select the Rectangular Redaction tool
            toolsRectangular.PerformClick();

            // Check that the menu item is checked
            Assert.That(toolsRectangular.Checked);
        }

        #endregion Tools Rectangular Redaction

        #region Tools Find Bracketed Text

        /// <summary>
        /// Test that the Tools - Find Bracketed Text menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsFindBracketedTextEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Tools - Find Bracketed Text menu item
            ToolStripMenuItem toolsBracketed =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Bracketed text...");

            // Check that the menu item is enabled
            Assert.That(toolsBracketed.Enabled);
        }

        /// <summary>
        /// Test that the Tools - Find Bracketed Text menu item is enabled 
        /// with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsFindBracketedTextEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Tools - Find Bracketed Text menu item
            ToolStripMenuItem toolsBracketed =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Bracketed text...");

            // Check that the menu item is enabled
            Assert.That(toolsBracketed.Enabled);
        }

        /// <summary>
        /// Tests whether the Tools - Find Bracketed Text menu item displays the  
        /// Find Bracketed Text dialog when selected.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ToolsFindBracketedTextDisplaysDialogTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Find Bracketed Text menu item
            ToolStripMenuItem toolsFind =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Bracketed text...");

            // Select the Find Bracketed Text tool
            toolsFind.PerformClick();

            // Check that the dialog is visible
            Assert.That(MessageBox.Show("Is the \"Find or redact - Bracketed Text\" dialog visible?",
                "Check dialog visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Tools Find Bracketed Text

        #region Tools Find Data Types

        /// <summary>
        /// Test that the Tools - Find Data Types menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsFindDataTypesEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Tools - Find Data Types menu item
            ToolStripMenuItem toolsData =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Data types...");

            // Check that the menu item is enabled
            Assert.That(toolsData.Enabled);
        }

        /// <summary>
        /// Test that the Tools - Find Data Types menu item is enabled 
        /// with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsFindDataTypesEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Tools - Find Data Types menu item
            ToolStripMenuItem toolsData =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Data types...");

            // Check that the menu item is enabled
            Assert.That(toolsData.Enabled);
        }

        /// <summary>
        /// Tests whether the Tools - Find Data Types menu item displays the  
        /// Find Data Types dialog when selected.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ToolsFindDataTypesDisplaysDialogTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Find Data Types menu item
            ToolStripMenuItem toolsFind =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Data types...");

            // Select the Find Data Types tool
            toolsFind.PerformClick();

            // Check that the dialog is visible
            Assert.That(MessageBox.Show("Is the \"Find or redact - Data types\" dialog visible?",
                "Check dialog visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Tools Find Data Types

        #region Tools Find Words / Patterns

        /// <summary>
        /// Test that the Tools - Find Words / Patterns menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsFindWordsPatternsEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Tools - Find Words / Patterns menu item
            ToolStripMenuItem toolsWords =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Words / patterns...");

            // Check that the menu item is enabled
            Assert.That(toolsWords.Enabled);
        }

        /// <summary>
        /// Test that the Tools - Find Words / Patterns menu item is enabled 
        /// with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsFindWordsPatternsEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Tools - Find Words / Patterns menu item
            ToolStripMenuItem toolsWords =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Words / patterns...");

            // Check that the menu item is enabled
            Assert.That(toolsWords.Enabled);
        }

        /// <summary>
        /// Tests whether the Tools - Find Words/Patterns menu item displays the  
        /// Find Words/Patterns dialog when selected.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ToolsFindWordsPatternsDisplaysDialogTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Find Words/Patterns menu item
            ToolStripMenuItem toolsFind =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Words / patterns...");

            // Select the Find Words/Patterns tool
            toolsFind.PerformClick();

            // Check that the dialog is visible
            Assert.That(MessageBox.Show("Is the \"Find or redact - Words/patterns\" dialog visible?",
                "Check dialog visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Tools Find Words / Patterns

        #region Tools Apply Bates Number

        /// <summary>
        /// Test that the Tools - Apply Bates Number menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsApplyBatesNumberDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Tools - Apply Bates Number menu item
            ToolStripMenuItem toolsBates =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Apply Bates number");

            // Check that the menu item is disabled
            Assert.That(!toolsBates.Enabled);
        }

        /// <summary>
        /// Test that the Tools - Apply Bates Number menu item is enabled 
        /// with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsApplyBatesNumberEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Tools - Apply Bates Number menu item
            ToolStripMenuItem toolsBates =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Apply Bates number");

            // Check that the menu item is enabled
            Assert.That(toolsBates.Enabled);
        }

        /// <summary>
        /// Tests whether the Tools - Apply Bates Number menu item adds a 
        /// Bates number to the open image.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ToolsApplyBatesNumberAddsNumberTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the image viewer
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Open the test image
            OpenTestImage(imageViewer);

            // Get the Tools - Apply Bates Number menu item
            ToolStripMenuItem toolsBates =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Apply Bates number");

            // Select the Apply Bates Number tool
            toolsBates.PerformClick();

            // Check that the Bates Number has been applied
            Assert.That(MessageBox.Show("Is the Bates Number visible?",
                "Check Bates Number visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the Tools - Apply Bates Number menu item is enabled 
        /// with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ToolsApplyBatesNumberDisabledAfterApplyTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Tools - Apply Bates Number menu item
            ToolStripMenuItem toolsBates =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Apply Bates number");

            // Apply the Bates Number
            toolsBates.PerformClick();

            // Check that the menu item is disabled
            Assert.That(!toolsBates.Enabled);
        }

        #endregion Tools Apply Bates Number

        #region Help ID Shield Office

        /// <summary>
        /// Test that the Help - ID Shield Office menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HelpIDShieldOfficeEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Help - ID Shield Office menu item
            ToolStripMenuItem helpIDShield =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&ID Shield Office help...");

            // Check that the menu item is enabled
            Assert.That(helpIDShield.Enabled);
        }

        /// <summary>
        /// Test that the Help - ID Shield Office menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HelpIDShieldOfficeEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Help - ID Shield Office menu item
            ToolStripMenuItem helpIDShield =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&ID Shield Office help...");

            // Check that the menu item is enabled
            Assert.That(helpIDShield.Enabled);
        }

        #endregion Help ID Shield Office

        #region Help Regular Expression

        /// <summary>
        /// Test that the Help - Regular Expression menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HelpRegularExpressionEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Help - Regular Expression menu item
            ToolStripMenuItem helpRegularExpression =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Regular expression help...");

            // Check that the menu item is enabled
            Assert.That(helpRegularExpression.Enabled);
        }

        /// <summary>
        /// Test that the Help - Regular Expression menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HelpRegularExpressionEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Help - Regular Expression menu item
            ToolStripMenuItem helpRegularExpression =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Regular expression help...");

            // Check that the menu item is enabled
            Assert.That(helpRegularExpression.Enabled);
        }

        #endregion Help Regular Expression

        #region Help About

        /// <summary>
        /// Test that the Help - About menu item is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HelpAboutEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Help - About menu item
            ToolStripMenuItem helpAbout =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&About ID Shield Office...");

            // Check that the menu item is enabled
            Assert.That(helpAbout.Enabled);
        }

        /// <summary>
        /// Test that the Help - About menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HelpAboutEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Help - About menu item
            ToolStripMenuItem helpAbout =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&About ID Shield Office...");

            // Check that the menu item is enabled
            Assert.That(helpAbout.Enabled);
        }

        #endregion Help About

        #region Context Menu

        /// <summary>
        /// Tests that the appropriate context menu items are available on the image viewer.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuContainsAppropriateItems()
        {
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

            // Get the context menu
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Check that all the expected items are there
            bool containsAllItems = items.Count == 8
                && (items[0] as SelectLayerObjectToolStripMenuItem) != null
                && (items[1] as ZoomWindowToolStripMenuItem) != null
                && (items[2] as PanToolStripMenuItem) != null
                && (items[3] as ToolStripSeparator) != null
                && (items[4] as ZoomPreviousToolStripMenuItem) != null
                && (items[5] as ZoomNextToolStripMenuItem) != null
                && (items[6] as ToolStripSeparator) != null
                && (items[7] as RedactionToolStripMenuItem) != null;

            Assert.That(containsAllItems);
        }

        #region Context Menu - Select Redactions

        /// <summary>
        /// Tests that the Select Redactions context menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuSelectRedactionsDisabledWithoutImage()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the context menu
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Select Redactions menu item
            SelectLayerObjectToolStripMenuItem menu = 
                (items[0] as SelectLayerObjectToolStripMenuItem);

            // Check that the menu item is disabled
            Assert.That(!menu.Enabled);
        }

        /// <summary>
        /// Tests that the Select Redactions context menu item is enabled 
        /// with an image open but no selectable item.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuSelectRedactionsEnabledWithImageNoItem()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            OpenTestImage(imageViewer);

            // Get the context menu
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Select Redactions menu item
            SelectLayerObjectToolStripMenuItem menu =
                (items[0] as SelectLayerObjectToolStripMenuItem);

            // Check that the menu item is enabled
            Assert.That(menu.Enabled);
        }

        /// <summary>
        /// Tests that the Select Redactions context menu item is enabled 
        /// with an image open and a selectable item.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuSelectRedactionsEnabledWithImageAndItem()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            OpenTestImage(imageViewer);

            // Get the Tools - Apply Bates Number menu item
            // and apply a Bates Number
            ToolStripMenuItem toolsBates =
                FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm, "&Apply Bates number");
            toolsBates.PerformClick();

            // Get the context menu
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Select Redactions menu item
            SelectLayerObjectToolStripMenuItem menu =
                (items[0] as SelectLayerObjectToolStripMenuItem);

            // Check that the menu item is enabled
            Assert.That(menu.Enabled);
        }

        #endregion Context Menu - Select Redactions

        #region Context Menu - Zoom Window

        /// <summary>
        /// Tests that the Zoom Window context menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuZoomWindowDisabledWithoutImage()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the context menu
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Zoom Window menu item
            ZoomWindowToolStripMenuItem menu =
                (items[1] as ZoomWindowToolStripMenuItem);

            // Check that the menu item is disabled
            Assert.That(!menu.Enabled);
        }

        /// <summary>
        /// Tests that the Zoom Window context menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuZoomWindowEnabledWithImage()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            OpenTestImage(imageViewer);

            // Get the context menu
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Zoom Window menu item
            ZoomWindowToolStripMenuItem menu =
                (items[1] as ZoomWindowToolStripMenuItem);

            // Check that the menu item is enabled
            Assert.That(menu.Enabled);
        }

        #endregion Context Menu - Zoom Window

        #region Context Menu - Pan

        /// <summary>
        /// Tests that the Pan context menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuPanDisabledWithoutImage()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the context menu
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Pan menu item
            PanToolStripMenuItem menu =
                (items[2] as PanToolStripMenuItem);

            // Check that the menu item is disabled
            Assert.That(!menu.Enabled);
        }

        /// <summary>
        /// Tests that the Pan context menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuPanEnabledWithImage()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            OpenTestImage(imageViewer);

            // Get the context menu
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Pan menu item
            PanToolStripMenuItem menu =
                (items[2] as PanToolStripMenuItem);

            // Check that the menu item is enabled
            Assert.That(menu.Enabled);
        }

        #endregion Context Menu - Pan

        #region Context Menu - Zoom Previous

        /// <summary>
        /// Tests that the Zoom Previous context menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuZoomPreviousDisabledWithoutImage()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the context menu
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Zoom Previous menu item
            ZoomPreviousToolStripMenuItem menu =
                (items[4] as ZoomPreviousToolStripMenuItem);

            // Check that the menu item is disabled
            Assert.That(!menu.Enabled);
        }

        /// <summary>
        /// Tests that the Zoom Previous context menu item is disabled 
        /// with an image open but no previous item.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuZoomPreviousDisabledWithImageNoPrevious()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            OpenTestImage(imageViewer);

            // Get the context menu
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Zoom Previous menu item
            ZoomPreviousToolStripMenuItem menu =
                (items[4] as ZoomPreviousToolStripMenuItem);

            // Check that the menu item is disabled
            Assert.That(!menu.Enabled);
        }

        /// <summary>
        /// Tests that the Zoom Previous context menu item is enabled 
        /// with an image open and a previous item.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuZoomPreviousEnabledWithImageAndPrevious()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            OpenTestImage(imageViewer);

            // Create a previous zoom history item by zooming in
            ZoomInToolStripMenuItem zoomIn =
                FormMethods.GetFormComponent<ZoomInToolStripMenuItem>(_idShieldOfficeForm);
            zoomIn.PerformClick();

            // Get the context menu
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Zoom Previous menu item
            ZoomPreviousToolStripMenuItem menu =
                (items[4] as ZoomPreviousToolStripMenuItem);

            // Check that the menu item is enabled
            Assert.That(menu.Enabled);
        }

        #endregion Context Menu - Zoom Previous

        #region Context Menu - Zoom Next

        /// <summary>
        /// Tests that the Zoom Next context menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuZoomNextDisabledWithoutImage()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the context menu
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Zoom Next menu item
            ZoomNextToolStripMenuItem menu =
                (items[5] as ZoomNextToolStripMenuItem);

            // Check that the menu item is disabled
            Assert.That(!menu.Enabled);
        }

        /// <summary>
        /// Tests that the Zoom Next context menu item is disabled 
        /// with an image open but no next item.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuZoomNextDisabledWithImageNoNext()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            OpenTestImage(imageViewer);

            // Get the context menu
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Zoom Next menu item
            ZoomNextToolStripMenuItem menu =
                (items[5] as ZoomNextToolStripMenuItem);

            // Check that the menu item is disabled
            Assert.That(!menu.Enabled);
        }

        /// <summary>
        /// Tests that the Zoom Next context menu item is enabled 
        /// with an image open and a next item.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuZoomNextEnabledWithImageAndNext()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            OpenTestImage(imageViewer);

            // Create a previous zoom history item by zooming in
            ZoomInToolStripMenuItem zoomIn =
                FormMethods.GetFormComponent<ZoomInToolStripMenuItem>(_idShieldOfficeForm);
            zoomIn.PerformClick();

            // Zoom to the previous item to create a Next item
            ZoomPreviousToolStripMenuItem zoomPrevious =
                FormMethods.GetFormComponent<ZoomPreviousToolStripMenuItem>(_idShieldOfficeForm);
            zoomPrevious.PerformClick();

            // Get the context menu
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Zoom Next menu item
            ZoomNextToolStripMenuItem menu =
                (items[5] as ZoomNextToolStripMenuItem);

            // Check that the menu item is enabled
            Assert.That(menu.Enabled);
        }

        #endregion Context Menu - Zoom Next

        #region Context Menu - Pan

        /// <summary>
        /// Tests that the Redaction context menu item is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuRedactionDisabledWithoutImage()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the context menu
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Redaction menu item
            RedactionToolStripMenuItem menu =
                (items[7] as RedactionToolStripMenuItem);

            // Check that the menu item is disabled
            Assert.That(!menu.Enabled);
        }

        /// <summary>
        /// Tests that the Redaction context menu item is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ContextMenuRedactionEnabledWithImage()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            OpenTestImage(imageViewer);

            // Get the context menu
            ContextMenuStrip contextMenu = imageViewer.ContextMenuStrip;

            // Get the collection of items from the context menu
            ToolStripItemCollection items = contextMenu.Items;

            // Retrieve Redaction menu item
            RedactionToolStripMenuItem menu =
                (items[7] as RedactionToolStripMenuItem);

            // Check that the menu item is enabled
            Assert.That(menu.Enabled);
        }

        #endregion Context Menu - Redaction

        #endregion Context Menu
    }
}
