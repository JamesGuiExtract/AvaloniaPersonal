using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.IO;
using UCLID_AFCORELib;
using UCLID_AFVALUEFINDERSLib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Test
{
    [TestFixture]
    [Category("RuleSet")]
    public class TestRuleSet
    {
        /// <summary>
        /// Performs initialization needed for the entire test run.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Performs tear down needed after entire test run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            // This helps prevent the test host from crashing when running only these tests
            // (I think the crash is due to our COM memory reporting hacks)
            GC.Collect(0);
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Confirm that rules can run with empty or non-existent source document name
        /// https://extract.atlassian.net/browse/ISSUE-18477
        /// </summary>
        [TestCase("", ExpectedResult = "Success")]
        [TestCase(@"C:\One\Two\Buckle\My\Shoe.pdf", ExpectedResult = "Success")]
        [TestCase(@"C:\One\Two\Buckle\My\Shoe.tif", ExpectedResult = "Success")]
        [RequiresThread]
        public static string ConfirmThatSourceDocNameCanBeMissingOrEmpty(string sourceDocName)
        {
            Assume.That(string.IsNullOrEmpty(sourceDocName) || !File.Exists(sourceDocName));

            try
            {
                // Arrange
                // Create a non-spatial input document with the supplied source doc name
                AFDocumentClass input = new();
                input.Text.CreateNonSpatialString("Will you find Success?", sourceDocName);

                // Make a ruleset with a regex rule to test ExecuteRulesOnText, which is run by the RuleTester
                // and also, via AttributeFinderEngine, by the ExecuteRules task 
                AttributeFindInfoClass attributeFindInfo = new();
                attributeFindInfo.AttributeRules.PushBack(
                    new AttributeRuleClass
                    {
                        AttributeFindingRule = new RegExprRuleClass { Pattern = "Success" }
                    });

                attributeFindInfo.InputValidator = new();
                RuleSetClass ruleset = new() { FileName = @"C:\placeholder.rsd" };
                ruleset.AttributeNameToInfoMap.Set("_", attributeFindInfo);

                // Act
                IUnknownVector results = ruleset.ExecuteRulesOnText(input, null, null, null);

                // Assert
                Assert.AreEqual(1, results.Size());

                // Return the string value for comparison by nunit
                return ((IAttribute)results.At(0)).Value.String;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                // Convert COM exceptions into a readable form
                throw ex.AsExtract("ELI53600");
            }
        }
    }
}
