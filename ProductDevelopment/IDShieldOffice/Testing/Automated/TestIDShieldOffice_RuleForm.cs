using Extract;
using Extract.Imaging;
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
        /// <summary>
        /// Test that the <see cref="IDShieldOfficeRuleForm"/> redact all
        /// button will not add the same redaction multiple times. [IDSD #51]
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RedactAllMultipleClicksOnlyAddsRedactionsOnce()
        {
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Create a new IDShieldOfficeRuleForm
            IDShieldOfficeRuleForm ruleForm = new IDShieldOfficeRuleForm("Bracketed text finder",
                new BracketedTextRule(true, true, true), _idShieldOfficeForm);
            
            // Show the rule form
            ruleForm.Show();

            // Get the redact all button
            Button redactAll = FormMethods.GetFormComponent<Button>(ruleForm, "Redact all");

            // Wait for OCR completion
            while (!_idShieldOfficeForm.OcrManager.OcrFinished)
            {
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(100);
            }

            // Click the redact all button
            redactAll.PerformClick();

            // Count the number of redactions
            int redactCount = _idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                null, ArgumentRequirement.Any,
                new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count;

            // Ensure that at least one redaction was created
            ExtractException.Assert("ELI22296", "No redactions created on image!", redactCount > 0);
 
            // Click the redact all button two more times
            redactAll.PerformClick();
            redactAll.PerformClick();

            // Check that there are still the same number of redactions
            Assert.That(redactCount == _idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                null, ArgumentRequirement.Any,
                new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count);
        }

        /// <summary>
        /// Test that the <see cref="IDShieldOfficeRuleForm"/> redact all
        /// button will add back a redaction that was deleted. [IDSD #51]
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RedactAllReplacesDeletedRedaction()
        {
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Create a new IDShieldOfficeRuleForm
            IDShieldOfficeRuleForm ruleForm = new IDShieldOfficeRuleForm("Bracketed text finder",
                new BracketedTextRule(true, false, false), _idShieldOfficeForm);
            
            // Show the rule form
            ruleForm.Show();

            // Get the redact all button
            Button redactAll = FormMethods.GetFormComponent<Button>(ruleForm, "Redact all");

            // Wait for OCR completion
            while (!_idShieldOfficeForm.OcrManager.OcrFinished)
            {
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(100);
            }

            // Click the redact all button
            redactAll.PerformClick();

            // Get the first redaction
            Redaction redactionToDelete = (Redaction)_idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                null, ArgumentRequirement.Any,
                new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any)[0];
            
            // Remove the first redaction from the image viewer
            _idShieldOfficeForm.ImageViewer.LayerObjects.Remove(redactionToDelete);

            // Ensure that the removed redaction does not equal any other redaction
            foreach (Redaction redaction in _idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                null, ArgumentRequirement.Any,
                new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any))
            {
                if (redactionToDelete == redaction)
                {
                    throw new ExtractException("ELI22295",
                        "Deleted redaction overlaps an existing redaction!");
                }
            }

            // Click redact all button
            redactAll.PerformClick();

            // Check that the redaction was placed back
            bool redactionReplaced = false;
            foreach (Redaction redaction in _idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                null, ArgumentRequirement.Any,
                new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any))
            {
                if (redactionToDelete == redaction)
                {
                    redactionReplaced = true;
                    break;
                }
            }

            Assert.That(redactionReplaced);
        }

        /// <summary>
        /// Tests that clicking Redact all will not add redactions in an area that is already
        /// covered by an existing redaction (i.e. a large redaction that covers two redactions
        /// that redact all would add) [IDSD #51]
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FindFormRedactAllDoesNotAddRedactionsToAreaAlreadyCovered()
        {
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the redact page menu item and click it
            FormMethods.GetFormComponent<ToolStripMenuItem>(_idShieldOfficeForm,
                "&Redact entire page").PerformClick();

            // Ensure there is only one redaction on page 1
            ExtractException.Assert("ELI23373",
                "There is more than one redaction and/or it is not on page one",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjectsOnPage(1, null,
                ArgumentRequirement.Any, new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count == 1);

            // Create a new IDShieldOfficeRuleForm
            IDShieldOfficeRuleForm ruleForm = new IDShieldOfficeRuleForm("Bracketed text finder",
                new BracketedTextRule(true, false, false), _idShieldOfficeForm);
            
            // Show the rule form
            ruleForm.Show();

            // Wait for OCR completion
            while (!_idShieldOfficeForm.OcrManager.OcrFinished)
            {
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(100);
            }

            // Redact all
            FormMethods.GetFormComponent<Button>(ruleForm, "Redact all").PerformClick();

            // Ensure there is still only one redaction on page 1
            bool onlyOnePageOneRedaction = _idShieldOfficeForm.ImageViewer.GetLayeredObjectsOnPage(1,
                null, ArgumentRequirement.Any,
                new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count == 1;

            // Check that there is only one redaction on page 1 and that there is more than
            // one redaction on the document
            Assert.That(onlyOnePageOneRedaction && _idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                null, ArgumentRequirement.Any, new Type[] { typeof(Redaction) },
                ArgumentRequirement.Any, null, ArgumentRequirement.Any).Count > 1);
        }

        /// <summary>
        /// Tests that each of the buttons on the find form are disabled when there is no
        /// image open. [IDSD #43]
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FindFormButtonsDisabledWithNoImage()
        {
            // Show the IDSO form
            _idShieldOfficeForm.Show();

            // Create a new IDShieldOfficeRuleForm
            IDShieldOfficeRuleForm ruleForm = new IDShieldOfficeRuleForm("Bracketed text finder",
                new BracketedTextRule(true, false, false), _idShieldOfficeForm);
            
            // Show the rule form
            ruleForm.Show();

            // Check that Find next, Redact, and Redact all are disabled
            bool buttonsEnabled = FormMethods.GetFormComponent<Button>(ruleForm, "Find next").Enabled;
            buttonsEnabled |= FormMethods.GetFormComponent<Button>(ruleForm, "Redact").Enabled;
            buttonsEnabled |= FormMethods.GetFormComponent<Button>(ruleForm, "Redact all").Enabled;

            Assert.That(!buttonsEnabled);
        }

        /// <summary>
        /// Tests that the redact button is not enabled when navigating through the redact all
        /// results. [IDSD #311]
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_RedactButtonNotEnabledWhenViewingRedactAllResults()
        {
            // Show the IDSO form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Create a new IDShieldOfficeRuleForm
            IDShieldOfficeRuleForm ruleForm = new IDShieldOfficeRuleForm("Bracketed text finder",
                new BracketedTextRule(true, false, false), _idShieldOfficeForm);
            
            // Show the rule form
            ruleForm.Show();

            // Get the find next and the redact button
            Button findNext = FormMethods.GetFormComponent<Button>(ruleForm, "Find next");
            Button redact = FormMethods.GetFormComponent<Button>(ruleForm, "Redact");

            // Wait for OCR completion
            while (!_idShieldOfficeForm.OcrManager.OcrFinished)
            {
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(100);
            }

            // Get the count of layer objects before the redact all
            int count = _idShieldOfficeForm.ImageViewer.LayerObjects.Count;

            // Click the redact all button
            FormMethods.GetFormComponent<Button>(ruleForm, "Redact all").PerformClick();

            // Ensure that at least one more layer object has been added
            ExtractException.Assert("ELI23381", "No results added for redact all",
                count < _idShieldOfficeForm.ImageViewer.LayerObjects.Count);

            // Loop through each of the found results and ensure that the redact button is
            // always disabled
            bool redactDisabled = !redact.Enabled;
            while (redactDisabled && findNext.Enabled)
            {
                findNext.PerformClick();
                redactDisabled = !redact.Enabled;
            }

            Assert.That(redactDisabled);
        }

        /// <summary>
        /// Tests that the find results are navigated in sorted order even if a match
        /// spans pages and has a match in between the page spanning. [IDSD #120]
        /// </summary>
        [Test, Category("Automated")]
        public void Automated_FindResultsAreNavigatedInSortedOrder()
        {
            // Show the IDSO form
            _idShieldOfficeForm.Show();

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Create a new IDShieldOfficeRuleForm
            IDShieldOfficeRuleForm ruleForm = new IDShieldOfficeRuleForm("Bracketed text finder",
                new BracketedTextRule(true, true, true), _idShieldOfficeForm);
            
            // Show the rule form
            ruleForm.Show();

            // Wait for OCR completion
            while (!_idShieldOfficeForm.OcrManager.OcrFinished)
            {
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(100);
            }

            // Get the find next button
            Button findNext = FormMethods.GetFormComponent<Button>(ruleForm, "Find next");

            List<CompositeHighlightLayerObject> results = new List<CompositeHighlightLayerObject>();
            // Build a list of layer objects from the find results
            while (findNext.Enabled)
            {
                // Find next
                findNext.PerformClick();
                
                // Get the newly added find result (there should only be one)
                results.Add((CompositeHighlightLayerObject)
                    _idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                    IDShieldOfficeForm._SEARCH_RESULT_TAGS, ArgumentRequirement.Any,
                    new Type[] { typeof(CompositeHighlightLayerObject) }, ArgumentRequirement.Any,
                    null, ArgumentRequirement.Any)[0]);
            }

            // Check that the find results are sorted
            bool sorted = true;
            for (int i = 1; i < results.Count && sorted; i++)
            {
                sorted = results[i - 1] < results[i] || results[i-1] == results[1];
            }

            // Check that the items were sorted
            Assert.That(sorted);
        }
    }
}
