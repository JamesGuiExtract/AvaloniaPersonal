using Extract;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace IDShieldOffice.Test
{
    public partial class TestIDShieldOffice
    {
        #region Open Image

        /// <summary>
        /// Test that the <see cref="OpenImageToolStripSplitButton"/> is 
        /// enabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_OpenImageEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the open image button
            OpenImageToolStripSplitButton openImage =
                FormMethods.GetFormComponent<OpenImageToolStripSplitButton>(_idShieldOfficeForm);

            // Check that the button is enabled
            Assert.That(openImage.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="OpenImageToolStripSplitButton"/> is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_OpenImageEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the open image button
            OpenImageToolStripSplitButton openImage =
                FormMethods.GetFormComponent<OpenImageToolStripSplitButton>(_idShieldOfficeForm);

            // Check that the button is enabled
            Assert.That(openImage.Enabled);
        }

        #endregion Open Image

        #region Save

        /// <summary>
        /// Test that the Save button is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SaveDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Save button
            ToolStripButton saveButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm, "Save");

            // Check that the button is disabled
            Assert.That(!saveButton.Enabled);
        }

        /// <summary>
        /// Test that the Save button is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SaveEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Save button
            ToolStripButton saveButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm, "Save");

            // Check that the button is enabled
            Assert.That(saveButton.Enabled);
        }

        #endregion Save

        #region Print Image

        /// <summary>
        /// Test that the <see cref="PrintImageToolStripSplitButton"/> is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PrintImageDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Print image button
            PrintImageToolStripButton printImage =
                FormMethods.GetFormComponent<PrintImageToolStripButton>(_idShieldOfficeForm);

            // Check that the button is disabled
            Assert.That(!printImage.Enabled);
        }

        /// <summary>
        /// Test that the <see cref="PrintImageToolStripSplitButton"/> is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PrintImageEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Print image button
            PrintImageToolStripButton printImage =
                FormMethods.GetFormComponent<PrintImageToolStripButton>(_idShieldOfficeForm);

            // Check that the button is enabled
            Assert.That(printImage.Enabled);
        }

        /// <summary>
        /// Test that the Print image button displays the Print dialog.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_PrintDisplaysDialogTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Print image button
            PrintImageToolStripButton printImage =
                FormMethods.GetFormComponent<PrintImageToolStripButton>(_idShieldOfficeForm);

            // Click the button
            printImage.PerformClick();

            // Check that the expected dialog is visible
            Assert.That(MessageBox.Show("Did the \"Print\" dialog appear?",
                "Check dialog visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Print Image

        #region Find Data Types

        /// <summary>
        /// Test that the Find Data Types button is 
        /// enabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FindDataTypesEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Find Data Types button
            ToolStripButton findDataTypesButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Data type finder");

            // Check that the button is enabled
            Assert.That(findDataTypesButton.Enabled);
        }

        /// <summary>
        /// Test that the Find Data Types button is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FindDataTypesEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Find Data Types button
            ToolStripButton findDataTypesButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Data type finder");

            // Check that the button is enabled
            Assert.That(findDataTypesButton.Enabled);
        }

        /// <summary>
        /// Test that the Find Data Types button displays the Find and 
        /// redact Data Types dialog.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_FindDataTypesDisplaysDialogTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Get the Find Data Types button
            ToolStripButton findDataTypesButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Data type finder");

            // Click the button
            findDataTypesButton.PerformClick();

            // Check that the expected dialog is visible
            Assert.That(MessageBox.Show("Is the \"Find or redact - Data types\" dialog visible?",
                "Check dialog visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Find Data Types

        #region Find Bracketed Text

        /// <summary>
        /// Test that the Find Bracketed Text button is 
        /// enabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FindBracketedTextEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Find Bracketed Text button
            ToolStripButton findBracketedTextButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Bracketed text finder");

            // Check that the button is enabled
            Assert.That(findBracketedTextButton.Enabled);
        }

        /// <summary>
        /// Test that the Find Bracketed Text button is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FindBracketedTextEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Find Bracketed Text button
            ToolStripButton findBracketedTextButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Bracketed text finder");

            // Check that the button is enabled
            Assert.That(findBracketedTextButton.Enabled);
        }

        /// <summary>
        /// Test that the Find Bracketed Text button displays the Find and 
        /// redact Bracketed Text dialog.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_FindBracketedTextDisplaysDialogTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Get the Find Bracketed Text button
            ToolStripButton findBracketedTextButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Bracketed text finder");

            // Click the button
            findBracketedTextButton.PerformClick();

            // Check that the expected dialog is visible
            Assert.That(MessageBox.Show("Is the \"Find or redact - Bracketed Text\" dialog visible?",
                "Check dialog visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Find Bracketed Text

        #region Find Word List

        /// <summary>
        /// Test that the Find Word List button is 
        /// enabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FindWordsEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Find Word List button
            ToolStripButton findWordListButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Word list finder");

            // Check that the button is enabled
            Assert.That(findWordListButton.Enabled);
        }

        /// <summary>
        /// Test that the Find Word List button is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FindWordsEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Find Word List button
            ToolStripButton findWordListButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Word list finder");

            // Check that the button is enabled
            Assert.That(findWordListButton.Enabled);
        }

        /// <summary>
        /// Test that the Find Word List button displays the Find and 
        /// redact Words / patterns dialog.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_FindWordsDisplaysDialogTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Get the Find Word List button
            ToolStripButton findWordListButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Word list finder");

            // Click the button
            findWordListButton.PerformClick();

            // Check that the expected dialog is visible
            Assert.That(MessageBox.Show("Is the \"Find or redact - Words/patterns\" dialog visible?",
                "Check dialog visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Find Word List

        #region Apply Bates Number

        /// <summary>
        /// Test that the Apply Bates Number button is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ApplyBatesNumberDisabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Apply Bates Number button
            ToolStripButton applyBatesNumberButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Apply Bates number");

            // Check that the button is disabled
            Assert.That(!applyBatesNumberButton.Enabled);
        }

        /// <summary>
        /// Test that the Apply Bates Number button is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ApplyBatesNumberEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Apply Bates Number button
            ToolStripButton applyBatesNumberButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Apply Bates number");

            // Check that the button is enabled
            Assert.That(applyBatesNumberButton.Enabled);
        }

        /// <summary>
        /// Test that the Apply Bates Number button applies a Bates  
        /// number to the open document.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_ApplyBatesNumberAppliesNumberTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Apply Bates Number button
            ToolStripButton applyBatesNumberButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Apply Bates number");

            // Click the button
            applyBatesNumberButton.PerformClick();

            // Check that the Bates number was applied
            Assert.That(MessageBox.Show("Is a Bates number present on the image?",
                "Check for Bates number", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Apply Bates Number

        #region Properties Window

        /// <summary>
        /// Test that the Properties Window button is 
        /// enabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PropertiesWindowEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Properties Window button
            ToolStripButton propertiesWindowButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Show or hide object properties");

            // Check that the button is enabled
            Assert.That(propertiesWindowButton.Enabled);
        }

        /// <summary>
        /// Test that the Properties Window button is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PropertiesWindowEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Properties Window button
            ToolStripButton propertiesWindowButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Show or hide object properties");

            // Check that the button is enabled
            Assert.That(propertiesWindowButton.Enabled);
        }

        /// <summary>
        /// Test that the Properties Window button shows and hides the 
        /// Properties Window.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_PropertiesWindowButtonShowsAndHidesWindowTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Get the Properties Window button
            ToolStripButton propertiesWindowButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Show or hide object properties");

            // Click the button, pause, click the button again
            _idShieldOfficeForm.Refresh();
            System.Threading.Thread.Sleep(1000);
            propertiesWindowButton.PerformClick();

            _idShieldOfficeForm.Refresh();
            System.Threading.Thread.Sleep(1000);
            propertiesWindowButton.PerformClick();
            _idShieldOfficeForm.Refresh();

            // Check that the window visibility changed twice
            Assert.That(
                MessageBox.Show("Did the Properties Window change visibility and then change back?", 
                "Check Properties Window visibility",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Properties Window

        #region Layers Window

        /// <summary>
        /// Test that the Layers Window button is 
        /// enabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_LayersWindowEnabledWithNoImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Get the Layers Window button
            ToolStripButton layersWindowButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Show or hide layers");

            // Check that the button is enabled
            Assert.That(layersWindowButton.Enabled);
        }

        /// <summary>
        /// Test that the Layers Window button is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_LayersWindowEnabledWithImageTest()
        {
            // Load the form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the Layers Window button
            ToolStripButton layersWindowButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Show or hide layers");

            // Check that the button is enabled
            Assert.That(layersWindowButton.Enabled);
        }

        /// <summary>
        /// Test that the Layers Window button shows and hides the Layers Window.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_LayersWindowButtonShowsAndHidesWindowTest()
        {
            // Show the image viewer form
            _idShieldOfficeForm.Show();
            _idShieldOfficeForm.BringToFront();

            // Get the Layers Window button
            ToolStripButton layersWindowButton =
                FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Show or hide layers");

            // Click the button, pause, click the button again
            _idShieldOfficeForm.Refresh();
            System.Threading.Thread.Sleep(1000);
            layersWindowButton.PerformClick();

            _idShieldOfficeForm.Refresh();
            System.Threading.Thread.Sleep(1000);
            layersWindowButton.PerformClick();
            _idShieldOfficeForm.Refresh();

            // Check that the window visibility changed twice
            Assert.That(
                MessageBox.Show("Did the Layers Window change visibility and then change back?",
                "Check Layers Window visibility",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Layers Window

    }
}
