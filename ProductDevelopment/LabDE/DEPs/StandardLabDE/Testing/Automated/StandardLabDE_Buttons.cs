using Extract;
using Extract.DataEntry;
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
        #region Open Image

        /// <summary>
        /// Test that the <see cref="OpenImageToolStripSplitButton"/> is 
        /// enabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_OpenImageEnabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the open image button
            OpenImageToolStripSplitButton openImage =
                FormMethods.GetFormComponent<OpenImageToolStripSplitButton>(_dataEntryApplicationForm);

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
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the open image button
            OpenImageToolStripSplitButton openImage =
                FormMethods.GetFormComponent<OpenImageToolStripSplitButton>(_dataEntryApplicationForm);

            // Check that the button is enabled
            Assert.That(openImage.Enabled);
        }

        #endregion Open Image

        #region Print Image

        /// <summary>
        /// Test that the <see cref="PrintImageToolStripSplitButton"/> is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PrintImageDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Print image button
            PrintImageToolStripButton printImage =
                FormMethods.GetFormComponent<PrintImageToolStripButton>(_dataEntryApplicationForm);

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
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Print image button
            PrintImageToolStripButton printImage =
                FormMethods.GetFormComponent<PrintImageToolStripButton>(_dataEntryApplicationForm);

            // Check that the button is enabled
            Assert.That(printImage.Enabled);
        }

        /// <summary>
        /// Test that the Print image button displays the Print dialog.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_PrintDisplaysDialogTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();
            _dataEntryApplicationForm.BringToFront();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Print image button
            PrintImageToolStripButton printImage =
                FormMethods.GetFormComponent<PrintImageToolStripButton>(_dataEntryApplicationForm);

            // Click the button
            printImage.PerformClick();

            // Check that the expected dialog is visible
            Assert.That(MessageBox.Show("Did the \"Print\" dialog appear?",
                "Check dialog visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        #endregion Print Image

        #region Save

        /// <summary>
        /// Test that the Save button is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_SaveDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Save button
            ToolStripButton saveButton =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm, 
                "Save");

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
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Save button
            ToolStripButton saveButton =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm, 
                "Save");

            // Check that the button is enabled
            Assert.That(saveButton.Enabled);
        }

        #endregion Save

        #region Go To Next Invalid Item

        /// <summary>
        /// Test that the Go To Next Invalid button is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToNextInvalidDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Go To Next Invalid button
            ToolStripButton nextInvalidButton =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm,
                "Go to next invalid item");

            // Check that the button is disabled
            Assert.That(!nextInvalidButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To Next Invalid button is 
        /// disabled with an open image but no invalid items.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToNextInvalidDisabledWithImageButNoInvalidTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file - all items are valid
            OpenTestImageAndVOA(_imageViewer, _BLOOD_CHEMISTRY, _BLOOD_CHEMISTRY_VOA);

            // Get the Go To Next Invalid button
            ToolStripButton nextInvalidButton =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm,
                "Go to next invalid item");

            // Check that the button is disabled
            Assert.That(!nextInvalidButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To Next Invalid button is 
        /// enabled with an open image and invalid items.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToNextInvalidEnabledWithImageAndInvalidTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_METABOLIC, _BASIC_METABOLIC_VOA);

            // Get the Go To Next Invalid button
            ToolStripButton nextInvalidButton =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm,
                "Go to next invalid item");

            // Check that the button is enabled
            Assert.That(nextInvalidButton.Enabled);
        }

        #endregion Go To Next Invalid Item

        #region Go To Next Unviewed Item

        /// <summary>
        /// Test that the Go To Next Unviewed button is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToNextUnviewedDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Go To Next Unviewed button
            ToolStripButton nextUnviewedButton =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm,
                "Go to next unviewed item");

            // Check that the button is disabled
            Assert.That(!nextUnviewedButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To Next Unviewed button is 
        /// disabled with an open image but no unviewed items.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToNextUnviewedDisabledWithImageButNoUnviewedTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BLOOD_CHEMISTRY, _BLOOD_CHEMISTRY_VOA);

            // Get the Go To Next Unviewed button
            ToolStripButton nextUnviewedButton =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm,
                "Go to next invalid item");

            // Move to next unviewed item - approximately 24 times so all will be viewed
            int i = 0;
            while (nextUnviewedButton.Enabled && i < 50)
            {
                nextUnviewedButton.PerformClick();
                Application.DoEvents();
                _dataEntryApplicationForm.Refresh();
                i++;
            }

            // Check that the button is disabled
            Assert.That(!nextUnviewedButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To Next Unviewed button is 
        /// enabled with an open image and unviewed items.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToNextUnviewedEnabledWithImageAndUnviewedTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_METABOLIC, _BASIC_METABOLIC_VOA);

            // Get the Go To Next Unviewed button
            ToolStripButton nextUnviewedButton =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm,
                "Go to next unviewed item");

            // Check that the button is enabled
            Assert.That(nextUnviewedButton.Enabled);
        }

        #endregion Go To Next Unviewed Item

        #region Highlight All Data

        /// <summary>
        /// Test that the Highlight All Data button is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HighlightAllDataDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Highlight All Data button
            ToolStripButton highlightAllDataButton =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm,
                "Highlight all data in image");

            // Check that the button is disabled
            Assert.That(!highlightAllDataButton.Enabled);
        }

        /// <summary>
        /// Test that the Highlight All Data button is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_HighlightAllDataEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_METABOLIC, _BASIC_METABOLIC_VOA);

            // Get the Highlight All Data button
            ToolStripButton highlightAllDataButton =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm,
                "Highlight all data in image");

            // Check that the button is enabled
            Assert.That(highlightAllDataButton.Enabled);
        }

        /// <summary>
        /// Test that the Highlight All Data button highlights all data 
        /// when toggled on.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_HighlightAllDataHighlightsWhenOnTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_METABOLIC, _BASIC_METABOLIC_VOA);

            // Get the Highlight All Data button
            ToolStripButton highlightAllDataButton =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm,
                "Highlight all data in image");

            // Click the button and make sure that it is toggled on
            highlightAllDataButton.PerformClick();
            if (!highlightAllDataButton.Checked)
            {
                // First click toggled it off, click it again
                highlightAllDataButton.PerformClick();
            }
            Assert.That(highlightAllDataButton.Checked);

            // Check that the data is highlighted
            Assert.That(MessageBox.Show("Is all data highlighted?",
                "Check highlight visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the Highlight All Data button clears all highlights 
        /// when toggled off.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_HighlightAllDataClearsHighlightsWhenOffTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_METABOLIC, _BASIC_METABOLIC_VOA);

            // Get the Highlight All Data button
            ToolStripButton highlightAllDataButton =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm,
                "Highlight all data in image");

            // Click the button and make sure that it is toggled off
            highlightAllDataButton.PerformClick();
            if (highlightAllDataButton.Checked)
            {
                // First click toggled it on, click it again
                highlightAllDataButton.PerformClick();
            }
            Assert.That(!highlightAllDataButton.Checked);

            // Check that the highlights are cleared
            Assert.That(MessageBox.Show("Is all data highlighted?",
                "Check highlight visibility", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2, 0) == DialogResult.No);
        }

        #endregion Highlight All Data

        #region Zoom Window

        /// <summary>
        /// Test that the Zoom Window button is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Zoom Window button
            ToolStripButton zoomWindowButton =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm,
                "Zoom window");

            // Check that the button is disabled
            Assert.That(!zoomWindowButton.Enabled);
        }

        /// <summary>
        /// Test that the Zoom Window button is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomWindowEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Zoom Window button
            ToolStripButton zoomWindowButton =
                FormMethods.GetFormComponent<ToolStripButton>(_dataEntryApplicationForm,
                "Zoom window");

            // Check that the button is enabled
            Assert.That(zoomWindowButton.Enabled);
        }

        #endregion Zoom Window

        #region Pan

        /// <summary>
        /// Test that the Pan button is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PanDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Pan button
            PanToolStripButton panButton =
                FormMethods.GetFormComponent<PanToolStripButton>(_dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!panButton.Enabled);
        }

        /// <summary>
        /// Test that the Pan button is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_PanEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Pan button
            PanToolStripButton panButton =
                FormMethods.GetFormComponent<PanToolStripButton>(_dataEntryApplicationForm);

            // Check that the button is enabled
            Assert.That(panButton.Enabled);
        }

        #endregion Pan

        #region Review And Select

        /// <summary>
        /// Test that the Review And Select button is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ReviewAndSelectDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Review And Select button
            SelectLayerObjectToolStripButton reviewAndSelectButton =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!reviewAndSelectButton.Enabled);
        }

        /// <summary>
        /// Test that the Review And Select button is 
        /// enabled with an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ReviewAndSelectEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Review And Select button
            SelectLayerObjectToolStripButton reviewAndSelectButton =
                FormMethods.GetFormComponent<SelectLayerObjectToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is enabled
            Assert.That(reviewAndSelectButton.Enabled);
        }

        #endregion Review And Select

        #region Create Angular Highlight

        /// <summary>
        /// Test that the Create Angular Highlight button is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_CreateAngularHighlightDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Create Angular Highlight button
            AngularHighlightToolStripButton angularButton =
                FormMethods.GetFormComponent<AngularHighlightToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!angularButton.Enabled);
        }

        /// <summary>
        /// Test that the Create Angular Highlight button is 
        /// disabled with an open image but no editable cell 
        /// selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_CreateAngularHighlightDisabledWithImageNoCellTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_HEMATOLOGY, _BASIC_HEMATOLOGY_VOA);

            // Get and select the Result Status combo box
            DataEntryComboBox resultStatus = FormMethods.GetFormComponent<DataEntryComboBox>(
                _dataEntryApplicationForm, "_resultStatus");
            resultStatus.Select();

            // Get the Create Angular Highlight button
            AngularHighlightToolStripButton angularButton =
                FormMethods.GetFormComponent<AngularHighlightToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!angularButton.Enabled);
        }

        /// <summary>
        /// Test that the Create Angular Highlight button is 
        /// enabled with an open image and an editable cell 
        /// selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_CreateAngularHighlightEnabledWithImageAndCellTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_HEMATOLOGY, _BASIC_HEMATOLOGY_VOA);

            // Send F4 to advance to the next unviewed item
            SendKeys.SendWait("{F4}");

            // Get the Create Angular Highlight button
            AngularHighlightToolStripButton angularButton =
                FormMethods.GetFormComponent<AngularHighlightToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is enabled
            Assert.That(angularButton.Enabled);
        }

        #endregion Create Angular Highlight

        #region Create Rectangular Highlight

        /// <summary>
        /// Test that the Create Rectangular Highlight button is 
        /// disabled with no open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_CreateRectangularHighlightDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Create Rectangular Highlight button
            RectangularHighlightToolStripButton rectangularButton =
                FormMethods.GetFormComponent<RectangularHighlightToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!rectangularButton.Enabled);
        }

        /// <summary>
        /// Test that the Create Rectangular Highlight button is 
        /// disabled with an open image but no editable cell 
        /// selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_CreateRectangularHighlightDisabledWithImageNoCellTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_HEMATOLOGY, _BASIC_HEMATOLOGY_VOA);

            // Get and select the Result Status combo box
            DataEntryComboBox resultStatus = FormMethods.GetFormComponent<DataEntryComboBox>(
                _dataEntryApplicationForm, "_resultStatus");
            resultStatus.Select();

            // Get the Create Rectangular Highlight button
            RectangularHighlightToolStripButton rectangularButton =
                FormMethods.GetFormComponent<RectangularHighlightToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!rectangularButton.Enabled);
        }

        /// <summary>
        /// Test that the Create Rectangular Highlight button is 
        /// enabled with an open image and an editable cell 
        /// selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_CreateRectangularHighlightEnabledWithImageAndCellTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image and VOA file
            OpenTestImageAndVOA(_imageViewer, _BASIC_HEMATOLOGY, _BASIC_HEMATOLOGY_VOA);

            // Send F4 to advance to the next unviewed item
            SendKeys.SendWait("{F4}");

            // Get the Create Rectangular Highlight button
            RectangularHighlightToolStripButton rectangularButton =
                FormMethods.GetFormComponent<RectangularHighlightToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is enabled
            Assert.That(rectangularButton.Enabled);
        }

        #endregion Create Rectangular Highlight

        #region Go To First Page

        /// <summary>
        /// Test that the Go To First Page toolbar button is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToFirstPageDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Go To First Page toolbar button
            FirstPageToolStripButton pageButton =
                FormMethods.GetFormComponent<FirstPageToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!pageButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To First Page toolbar button is disabled 
        /// on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToFirstPageDisabledOnFirstPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Go To First Page toolbar button
            FirstPageToolStripButton pageButton =
                FormMethods.GetFormComponent<FirstPageToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!pageButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To First Page toolbar button is enabled 
        /// when not on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToFirstPageEnabledNotOnFirstPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Move to page 2
            _imageViewer.PageNumber = 2;

            // Get the Go To First Page toolbar button
            FirstPageToolStripButton pageButton =
                FormMethods.GetFormComponent<FirstPageToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is enabled
            Assert.That(pageButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To First Page toolbar button navigates 
        /// to the first page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToFirstPageNavigationTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Move to page 2
            _imageViewer.PageNumber = 2;

            // Get the Go To First Page toolbar button
            FirstPageToolStripButton pageButton =
                FormMethods.GetFormComponent<FirstPageToolStripButton>(
                _dataEntryApplicationForm);

            // Move back to the first page
            pageButton.PerformClick();

            // Check that the first page is active
            Assert.That(_imageViewer.PageNumber == 1);
        }

        #endregion Go To First Page

        #region Go To Previous Page

        /// <summary>
        /// Test that the Go To Previous Page toolbar button is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToPreviousPageDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Go To Previous Page toolbar button
            PreviousPageToolStripButton pageButton =
                FormMethods.GetFormComponent<PreviousPageToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!pageButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To Previous Page toolbar button is disabled 
        /// on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToPreviousPageDisabledOnFirstPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Go To Previous Page toolbar button
            PreviousPageToolStripButton pageButton =
                FormMethods.GetFormComponent<PreviousPageToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!pageButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To Previous Page toolbar button is enabled 
        /// when not on the first page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToPreviousPageEnabledNotOnFirstPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Move to page 2
            _imageViewer.PageNumber = 2;

            // Get the Go To Previous Page toolbar button
            PreviousPageToolStripButton pageButton =
                FormMethods.GetFormComponent<PreviousPageToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is enabled
            Assert.That(pageButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To Previous Page toolbar button navigates 
        /// to the previous page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToPreviousPageNavigationTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Move to page 2
            _imageViewer.PageNumber = 2;

            // Get the Go To Previous Page toolbar button
            PreviousPageToolStripButton pageButton =
                FormMethods.GetFormComponent<PreviousPageToolStripButton>(
                _dataEntryApplicationForm);

            // Move back to the previous page
            pageButton.PerformClick();

            // Check that the previous page is active
            Assert.That(_imageViewer.PageNumber == 1);
        }

        #endregion Go To Previous Page

        #region Go To Next Page

        /// <summary>
        /// Test that the Go To Next Page toolbar button is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToNextPageDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Go To Next Page toolbar button
            NextPageToolStripButton pageButton =
                FormMethods.GetFormComponent<NextPageToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!pageButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To Next Page toolbar button is disabled 
        /// on the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToNextPageDisabledOnLastPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Move to the last page
            _imageViewer.PageNumber = _imageViewer.PageCount;

            // Get the Go To Next Page toolbar button
            NextPageToolStripButton pageButton =
                FormMethods.GetFormComponent<NextPageToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!pageButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To Next Page toolbar button is enabled 
        /// when on the first page of an open multiple-page image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToNextPageEnabledOnFirstPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Get the Go To Next Page toolbar button
            NextPageToolStripButton pageButton =
                FormMethods.GetFormComponent<NextPageToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is enabled
            Assert.That(pageButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To Next Page toolbar button navigates 
        /// to the next page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToNextPageNavigationTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Get the Go To Next Page toolbar button
            NextPageToolStripButton pageButton =
                FormMethods.GetFormComponent<NextPageToolStripButton>(
                _dataEntryApplicationForm);

            // Move to the next page
            pageButton.PerformClick();

            // Check that the next page is active
            Assert.That(_imageViewer.PageNumber == 2);
        }

        #endregion Go To Next Page

        #region Go To Last Page

        /// <summary>
        /// Test that the Go To Last Page toolbar button is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToLastPageDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Go To Last Page toolbar button
            LastPageToolStripButton pageButton =
                FormMethods.GetFormComponent<LastPageToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!pageButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To Last Page toolbar button is disabled 
        /// on the last page of an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToLastPageDisabledOnLastPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Move to the last page
            _imageViewer.PageNumber = _imageViewer.PageCount;

            // Get the Go To Last Page toolbar button
            LastPageToolStripButton pageButton =
                FormMethods.GetFormComponent<LastPageToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!pageButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To Last Page toolbar button is enabled 
        /// when on the first page of an open multiple-page image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToLastPageEnabledOnFirstPageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Get the Go To Last Page toolbar button
            LastPageToolStripButton pageButton =
                FormMethods.GetFormComponent<LastPageToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is enabled
            Assert.That(pageButton.Enabled);
        }

        /// <summary>
        /// Test that the Go To Last Page toolbar button navigates 
        /// to the next page.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_GoToLastPageNavigationTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Get the Go To Last Page toolbar button
            LastPageToolStripButton pageButton =
                FormMethods.GetFormComponent<LastPageToolStripButton>(
                _dataEntryApplicationForm);

            // Move to the last page
            pageButton.PerformClick();

            // Check that the last page is active
            Assert.That(_imageViewer.PageNumber == _imageViewer.PageCount);
        }

        #endregion Go To Last Page

        #region Zoom In

        /// <summary>
        /// Test that the Zoom In toolbar button is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Zoom In toolbar button
            ZoomInToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomInToolStripButton>(_dataEntryApplicationForm);

            // Check that the toolbar button is disabled
            Assert.That(!zoomButton.Enabled);
        }

        /// <summary>
        /// Test that the Zoom In toolbar button is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Zoom In toolbar button
            ZoomInToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomInToolStripButton>(_dataEntryApplicationForm);

            // Check that the menu item is enabled
            Assert.That(zoomButton.Enabled);
        }

        /// <summary>
        /// Tests whether the Zoom In toolbar button zooms in  
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolBarButtonZoomTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the zoom level
            double zoomLevel = _imageViewer.ZoomInfo.ScaleFactor;

            // Get the Zoom In toolbar button
            ZoomInToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomInToolStripButton>(_dataEntryApplicationForm);

            // Zoom in
            zoomButton.PerformClick();

            // Check that the image zoomed in
            Assert.That(_imageViewer.ZoomInfo.ScaleFactor > zoomLevel);
        }

        /// <summary>
        /// Tests whether the Zoom In toolbar button adds a zoom history entry.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomInToolBarButtonAddsZoomHistoryTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the current Zoom history count
            int zoomHistoryCount = _imageViewer.ZoomHistoryCount;

            // Get the Zoom In toolbar button and zoom in
            ZoomInToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomInToolStripButton>(_dataEntryApplicationForm);
            zoomButton.PerformClick();

            // Check that exactly one zoom history record has been added
            Assert.That((zoomHistoryCount + 1) == _imageViewer.ZoomHistoryCount);
        }

        #endregion Zoom In

        #region Zoom Out

        /// <summary>
        /// Test that the Zoom Out toolbar button is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Zoom Out toolbar button
            ZoomOutToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomOutToolStripButton>(_dataEntryApplicationForm);

            // Check that the toolbar button is disabled
            Assert.That(!zoomButton.Enabled);
        }

        /// <summary>
        /// Test that the Zoom Out toolbar button is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Zoom Out toolbar button
            ZoomOutToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomOutToolStripButton>(_dataEntryApplicationForm);

            // Check that the menu item is enabled
            Assert.That(zoomButton.Enabled);
        }

        /// <summary>
        /// Tests whether the Zoom Out toolbar button zooms out 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolBarButtonZoomTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the zoom level
            double zoomLevel = _imageViewer.ZoomInfo.ScaleFactor;

            // Get the Zoom Out toolbar button
            ZoomOutToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomOutToolStripButton>(_dataEntryApplicationForm);

            // Zoom out
            zoomButton.PerformClick();

            // Check that the image zoomed out
            Assert.That(_imageViewer.ZoomInfo.ScaleFactor < zoomLevel);
        }

        /// <summary>
        /// Tests whether the Zoom Out toolbar button adds a zoom history entry.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomOutToolBarButtonAddsZoomHistoryTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the current Zoom history count
            int zoomHistoryCount = _imageViewer.ZoomHistoryCount;

            // Get the Zoom Out toolbar button and zoom out
            ZoomOutToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomOutToolStripButton>(_dataEntryApplicationForm);
            zoomButton.PerformClick();

            // Check that exactly one zoom history record has been added
            Assert.That((zoomHistoryCount + 1) == _imageViewer.ZoomHistoryCount);
        }

        #endregion Zoom Out

        #region Zoom Previous

        /// <summary>
        /// Test that the Zoom Previous toolbar button is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Zoom Previous toolbar button
            ZoomPreviousToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomPreviousToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the toolbar button is disabled
            Assert.That(!zoomButton.Enabled);
        }

        /// <summary>
        /// Test that the Zoom Previous toolbar button is disabled 
        /// with an image open and no zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousDisabledWithImageNoHistoryTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Zoom Previous toolbar button
            ZoomPreviousToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomPreviousToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the menu item is disabled
            Assert.That(!zoomButton.Enabled);
        }

        /// <summary>
        /// Test that the Zoom Previous toolbar button is enabled 
        /// with an image open and a zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousEnabledWithImageAndHistoryTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Zoom In toolbar button and zoom in
            ZoomInToolStripButton zoomInButton =
                FormMethods.GetFormComponent<ZoomInToolStripButton>(_dataEntryApplicationForm);
            zoomInButton.PerformClick();

            // Get the Zoom Previous toolbar button
            ZoomPreviousToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomPreviousToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the menu item is enabled
            Assert.That(zoomButton.Enabled);
        }

        /// <summary>
        /// Tests whether the Zoom Previous toolbar button zooms to previous 
        /// with an image open and a zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomPreviousToolBarButtonZoomTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the initial zoom level
            double zoomLevelInitial = _imageViewer.ZoomInfo.ScaleFactor;

            // Get the Zoom In toolbar button and zoom in
            ZoomInToolStripButton zoomInButton =
                FormMethods.GetFormComponent<ZoomInToolStripButton>(_dataEntryApplicationForm);
            zoomInButton.PerformClick();

            // Get the new zoom level
            double zoomLevelNew = _imageViewer.ZoomInfo.ScaleFactor;

            // Get the Zoom Previous toolbar button and zoom previous
            ZoomPreviousToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomPreviousToolStripButton>(
                _dataEntryApplicationForm);
            zoomButton.PerformClick();

            // Check that the image zoomed previous
            Assert.That(_imageViewer.ZoomInfo.ScaleFactor == zoomLevelInitial &&
                zoomLevelInitial != zoomLevelNew);
        }

        #endregion Zoom Previous

        #region Zoom Next

        /// <summary>
        /// Test that the Zoom Next toolbar button is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomNextDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Zoom Next toolbar button
            ZoomNextToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomNextToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the toolbar button is disabled
            Assert.That(!zoomButton.Enabled);
        }

        /// <summary>
        /// Test that the Zoom Next toolbar button is disabled 
        /// with an image open and no zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomNextDisabledWithImageNoHistoryTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Zoom Next toolbar button
            ZoomNextToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomNextToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the menu item is disabled
            Assert.That(!zoomButton.Enabled);
        }

        /// <summary>
        /// Test that the Zoom Next toolbar button is enabled 
        /// with an image open and a zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomNextEnabledWithImageAndHistoryTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Zoom In toolbar button and zoom in
            ZoomInToolStripButton zoomInButton =
                FormMethods.GetFormComponent<ZoomInToolStripButton>(_dataEntryApplicationForm);
            zoomInButton.PerformClick();

            // Get the Zoom Previous toolbar button and zoom previous
            ZoomPreviousToolStripButton zoomPreviousButton =
                FormMethods.GetFormComponent<ZoomPreviousToolStripButton>(
                _dataEntryApplicationForm);
            zoomPreviousButton.PerformClick();

            // Get the Zoom Next toolbar button
            ZoomNextToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomNextToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the menu item is enabled
            Assert.That(zoomButton.Enabled);
        }

        /// <summary>
        /// Tests whether the Zoom Next toolbar button zooms to next 
        /// with an image open and a zoom history.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_ZoomNextToolBarButtonZoomTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the initial zoom level
            double zoomLevelInitial = _imageViewer.ZoomInfo.ScaleFactor;

            // Get the Zoom In toolbar button and zoom in
            ZoomInToolStripButton zoomInButton =
                FormMethods.GetFormComponent<ZoomInToolStripButton>(_dataEntryApplicationForm);
            zoomInButton.PerformClick();

            // Get the new zoom level
            double zoomLevelNew = _imageViewer.ZoomInfo.ScaleFactor;

            // Get the Zoom Previous toolbar button and zoom previous
            ZoomPreviousToolStripButton zoomPreviousButton =
                FormMethods.GetFormComponent<ZoomPreviousToolStripButton>(
                _dataEntryApplicationForm);
            zoomPreviousButton.PerformClick();

            // Get the Zoom Next toolbar button and zoom next
            ZoomNextToolStripButton zoomButton =
                FormMethods.GetFormComponent<ZoomNextToolStripButton>(
                _dataEntryApplicationForm);
            zoomButton.PerformClick();

            // Check that the image zoomed next
            Assert.That(_imageViewer.ZoomInfo.ScaleFactor == zoomLevelNew &&
                zoomLevelInitial != zoomLevelNew);
        }

        #endregion Zoom Next

        #region Fit To Page

        /// <summary>
        /// Test that the Fit To Page toolbar button is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageButtonEnabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the fit to page button
            FitToPageToolStripButton fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_dataEntryApplicationForm);

            // Check that the button is enabled
            Assert.That(fitToPage.Enabled);
        }

        /// <summary>
        /// Test that the Fit To Page toolbar button is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageButtonEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the fit to page button
            FitToPageToolStripButton fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_dataEntryApplicationForm);

            // Check that the button is enabled
            Assert.That(fitToPage.Enabled);
        }

        /// <summary>
        /// Test that the Fit To Page toolbar button works 
        /// without an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageButtonNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Set the fit mode to none
            _imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI26127", "Could not change fit mode to none!",
                _imageViewer.FitMode == FitMode.None);

            // Get the fit to page button
            FitToPageToolStripButton fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_dataEntryApplicationForm);

            // Click the button
            fitToPage.PerformClick();

            Assert.That(_imageViewer.FitMode == FitMode.FitToPage);
        }

        /// <summary>
        /// Test that the Fit To Page toolbar button works with 
        /// an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageButtonWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Set the fit mode to none
            _imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI26128", "Could not change fit mode to none!",
                _imageViewer.FitMode == FitMode.None);

            // Get the fit to page button
            FitToPageToolStripButton fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_dataEntryApplicationForm);

            // Click the button
            fitToPage.PerformClick();

            Assert.That(_imageViewer.FitMode == FitMode.FitToPage);
        }

        /// <summary>
        /// Tests whether the Fit To Page toolbar button is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageButtonToggledOnWhenSelectedTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the FitToWidth button
            FitToWidthToolStripButton fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_dataEntryApplicationForm);

            // Select the FitToWidth tool
            fitToWidth.PerformClick();

            // Get the FitToPage button
            FitToPageToolStripButton fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_dataEntryApplicationForm);

            // Select the FitToPage tool
            fitToPage.PerformClick();

            // Check that the button is checked
            Assert.That(fitToPage.Checked);
        }

        /// <summary>
        /// Tests whether the Fit To Page toolbar button is toggled off 
        /// when selected again.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToPageButtonTogglesOffWhenSelectedTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the FitToWidth button
            FitToWidthToolStripButton fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_dataEntryApplicationForm);

            // Select the FitToWidth tool
            fitToWidth.PerformClick();

            // Get the FitToPage button
            FitToPageToolStripButton fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_dataEntryApplicationForm);

            // Select the FitToPage tool twice
            fitToPage.PerformClick();
            fitToPage.PerformClick();

            // Check that the button is unchecked
            Assert.That(!fitToPage.Checked);
        }

        #endregion Fit To Page

        #region Fit To Width

        /// <summary>
        /// Test that the Fit To Width toolbar button is enabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthButtonEnabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the fit to width button
            FitToWidthToolStripButton fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_dataEntryApplicationForm);

            // Check that the button is enabled
            Assert.That(fitToWidth.Enabled);
        }

        /// <summary>
        /// Test that the Fit To Width toolbar button is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthButtonEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the fit to width button
            FitToWidthToolStripButton fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_dataEntryApplicationForm);

            // Check that the button is enabled
            Assert.That(fitToWidth.Enabled);
        }

        /// <summary>
        /// Test that the Fit To Width toolbar button works 
        /// without an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthButtonNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Set the fit mode to none
            _imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI26127", "Could not change fit mode to none!",
                _imageViewer.FitMode == FitMode.None);

            // Get the FitToWidth toolbar button
            FitToWidthToolStripButton fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_dataEntryApplicationForm);

            // Click the button
            fitToWidth.PerformClick();

            Assert.That(_imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Test that the Fit To Width toolbar button works with 
        /// an open image.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthButtonWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Set the fit mode to none
            _imageViewer.FitMode = FitMode.None;

            // Check that fit mode changed to none
            ExtractException.Assert("ELI26128", "Could not change fit mode to none!",
                _imageViewer.FitMode == FitMode.None);

            // Get the FitToWidth button
            FitToWidthToolStripButton fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_dataEntryApplicationForm);

            // Click the button
            fitToWidth.PerformClick();

            Assert.That(_imageViewer.FitMode == FitMode.FitToWidth);
        }

        /// <summary>
        /// Tests whether the Fit To Width toolbar button is toggled on 
        /// when selected.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthButtonToggledOnWhenSelectedTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the FitToPage button
            FitToPageToolStripButton fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_dataEntryApplicationForm);

            // Select the FitToPage tool
            fitToPage.PerformClick();

            // Get the FitToWidth button
            FitToWidthToolStripButton fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_dataEntryApplicationForm);

            // Select the FitToWidth tool
            fitToWidth.PerformClick();

            // Check that the button is checked
            Assert.That(fitToWidth.Checked);
        }

        /// <summary>
        /// Tests whether the Fit To Width toolbar button is toggled off 
        /// when selected again.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FitToWidthButtonTogglesOffWhenSelectedTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the FitToPage button
            FitToPageToolStripButton fitToPage =
                FormMethods.GetFormComponent<FitToPageToolStripButton>(_dataEntryApplicationForm);

            // Select the FitToPage tool
            fitToPage.PerformClick();

            // Get the FitToWidth button
            FitToWidthToolStripButton fitToWidth =
                FormMethods.GetFormComponent<FitToWidthToolStripButton>(_dataEntryApplicationForm);

            // Select the FitToWidth tool twice
            fitToWidth.PerformClick();
            fitToWidth.PerformClick();

            // Check that the button is unchecked
            Assert.That(!fitToWidth.Checked);
        }

        #endregion Fit To Width

        #region Rotate Counterclockwise

        /// <summary>
        /// Test that the Rotate Counterclockwise toolbar button is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateCounterclockwiseDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Rotate Counterclockwise toolbar button
            RotateCounterclockwiseToolStripButton rotateButton =
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!rotateButton.Enabled);
        }

        /// <summary>
        /// Test that the Rotate Counterclockwise toolbar button is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateCounterclockwiseEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Rotate Counterclockwise toolbar button
            RotateCounterclockwiseToolStripButton rotateButton =
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the menu item is enabled
            Assert.That(rotateButton.Enabled);
        }

        /// <summary>
        /// Test that the Rotate Counterclockwise toolbar button rotates  
        /// properly without visible items.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateCounterclockwiseRotateWithoutItemsTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Rotate Counterclockwise toolbar button
            RotateCounterclockwiseToolStripButton rotateButton =
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(
                _dataEntryApplicationForm);

            // Rotate the image
            rotateButton.PerformClick();

            // Check that the image rotated properly
            Assert.That(MessageBox.Show("Did the image rotate counterclockwise?",
                "Proper image rotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the Rotate Counterclockwise toolbar button menu item rotates  
        /// only the active page.
        /// </summary>
        [Test, Category("Interactive")]
        public void Automated_RotateCounterclockwiseRotatesActivePageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Get the Rotate Counterclockwise toolbar button
            RotateCounterclockwiseToolStripButton rotateButton =
                FormMethods.GetFormComponent<RotateCounterclockwiseToolStripButton>(
                _dataEntryApplicationForm);

            // Rotate the image
            rotateButton.PerformClick();

            // Move to page 2 and refresh the page
            _imageViewer.PageNumber = 2;
            _dataEntryApplicationForm.Refresh();

            // Check that the image rotated properly
            Assert.That(MessageBox.Show("Is the active page rotated?",
                "Proper image rotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2, 0) == DialogResult.No);
        }

        #endregion Rotate Counterclockwise

        #region Rotate Clockwise

        /// <summary>
        /// Test that the Rotate Clockwise toolbar button is disabled 
        /// with no image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateClockwiseDisabledWithNoImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Get the Rotate Clockwise toolbar button
            RotateClockwiseToolStripButton rotateButton =
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the button is disabled
            Assert.That(!rotateButton.Enabled);
        }

        /// <summary>
        /// Test that the Rotate Clockwise toolbar button is enabled 
        /// with an image open.
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RotateClockwiseEnabledWithImageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Rotate Clockwise toolbar button
            RotateClockwiseToolStripButton rotateButton =
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(
                _dataEntryApplicationForm);

            // Check that the menu item is enabled
            Assert.That(rotateButton.Enabled);
        }

        /// <summary>
        /// Test that the Rotate Clockwise toolbar button rotates  
        /// properly without visible items.
        /// </summary>
        [Test, Category("Interactive")]
        public void Interactive_RotateClockwiseRotateWithoutItemsTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_HEMATOLOGY);

            // Get the Rotate Clockwise toolbar button
            RotateClockwiseToolStripButton rotateButton =
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(
                _dataEntryApplicationForm);

            // Rotate the image
            rotateButton.PerformClick();

            // Check that the image rotated properly
            Assert.That(MessageBox.Show("Did the image rotate clockwise?",
                "Proper image rotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
        }

        /// <summary>
        /// Test that the Rotate Clockwise toolbar button rotates  
        /// only the active page.
        /// </summary>
        [Test, Category("Interactive")]
        public void Automated_RotateClockwiseRotatesActivePageTest()
        {
            // Load the form
            _dataEntryApplicationForm.Show();

            // Open the test image
            OpenTestImage(_imageViewer, _BASIC_METABOLIC);

            // Get the Rotate Clockwise toolbar button
            RotateClockwiseToolStripButton rotateButton =
                FormMethods.GetFormComponent<RotateClockwiseToolStripButton>(
                _dataEntryApplicationForm);

            // Rotate the image
            rotateButton.PerformClick();

            // Move to page 2 and refresh the page
            _imageViewer.PageNumber = 2;
            _dataEntryApplicationForm.Refresh();

            // Check that the image rotated properly
            Assert.That(MessageBox.Show("Is the active page rotated?",
                "Proper image rotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2, 0) == DialogResult.No);
        }

        #endregion Rotate Clockwise

    }
}
