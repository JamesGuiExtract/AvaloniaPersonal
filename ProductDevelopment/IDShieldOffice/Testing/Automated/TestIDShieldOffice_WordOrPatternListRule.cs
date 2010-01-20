using Extract.Rules;
using NUnit.Framework;
using System;

namespace IDShieldOffice.Test
{
    public partial class TestIDShieldOffice
    {
        /// <summary>
        /// Paramaterized test that tests that the word finder does not throw an exception when
        /// using each of the regular expressions specified in [IDSD #363].
        /// </summary>
        [Test, Category("Automated"), Category("Word Or Pattern List")]
        [CLSCompliant(false)]
        public void Automated_WordOrPatternListWithRegexGetMatchesDoesNotThrowException([Values(
            @".",
            @".*",
            @"[\s\S]*",
            @"[\s\S]",
            @"[\s\S]*$",
            @" ",
            @"\s",
            @"\s*",
            @"\s+"
            )] string regex)
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Create a new word or pattern list rule to search for the regular expression
            WordOrPatternListRule wordRule = new WordOrPatternListRule(false,
                true, regex);

            // Wait for OCR to complete
            while (!_idShieldOfficeForm.OcrManager.OcrFinished)
            {
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(100);
            }

            // Get matches for the ocr result
            wordRule.GetMatches(_idShieldOfficeForm.OcrManager.GetOcrSpatialString());

            // If code reached this point just assert true since no exception has been thrown
            Assert.That(true);
        }
    }
}
