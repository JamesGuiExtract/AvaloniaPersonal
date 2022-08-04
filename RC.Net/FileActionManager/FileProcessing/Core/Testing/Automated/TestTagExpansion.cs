using Extract.Testing.Utilities;
using Extract.Utilities;
using Moq;
using NUnit.Framework;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessing.Test
{
    public enum TagSource
    {
        TagManager,
        ContextTagProvider,
    }

    [TestFixture]
    [Category("TagExpansion")]
    public class TestTagExpansion
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
        }

        [Test, Category("Automated")]
        public static void TagReferencesBuiltInTag_SpecialChars([Values] TagSource tagSource)
        {
            try
            {
                // Arrange
                StrToStrMap tags = new StrToStrMapClass();
                tags.Set("<TestTag>", @"$DirOf(<SDN>)\$FileNoExtOf($FileNoExtOf($Replace{|}(<SDN>|_User_Paginated_|.))).$ExtOf(<SDN>)");
                tags.Set("<SDN>", @"<SourceDocName>");

                var tagManager = GetFAMTagManager(tags, tagSource);

                // Act
                string res = tagManager.ExpandTagsAndFunctions("<TestTag>", @"\\Server\Input\Attempt (1)_User_Paginated_3.pdf");

                // Assert
                Assert.AreEqual(@"\\Server\Input\Attempt (1).pdf", res);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                throw ex.AsExtract("ELI53545");
            }
        }

        [Test, Category("Automated")]
        public static void TagsWithCycles([Values] TagSource tagSource)
        {
            // Arrange
            StrToStrMap tags = new StrToStrMapClass();
            tags.Set("<FirstTag>", "<SecondTag>");
            tags.Set("<SecondTag>", "<ThirdTag>");
            tags.Set("<ThirdTag>", "<FourthTag>");
            tags.Set("<FourthTag>", "<FirstTag>");
            tags.Set("<TestTag>", "<FirstTag>");

            var tagManager = GetFAMTagManager(tags, tagSource);

            const string expectedResult = "UNDEFINED";
            string result = expectedResult;
            string exceptionMessage = "";

            // Act
            try
            {
                result = tagManager.ExpandTagsAndFunctions("<TestTag>", @"\\Server\Input\Attempt (1)_User_Paginated_3.pdf");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                exceptionMessage = ExtractException.FromStringizedByteStream("", ex.Message).Message;
            }

            // Assert
            Assert.Multiple(() =>
            {
                Assert.AreEqual("Runtime error: Exceeded maximum number of cycles allowed while expanding path tags!", exceptionMessage);

                // An exception is best, but if that doesn't happen, what is the result?
                Assert.AreEqual(expectedResult, result);
            });
        }

        [Test, Category("Automated")]
        [Ignore("Causes an access violation")]
        public static void FunctionsWithCycles([Values] TagSource tagSource)
        {
            // Arrange
            StrToStrMap tags = new StrToStrMapClass();
            tags.Set("<FirstTag>", "$DirOf(<SecondTag>)");
            tags.Set("<SecondTag>", "$DirOf(<ThirdTag>)");
            tags.Set("<ThirdTag>", "$DirOf(<FourthTag>)");
            tags.Set("<FourthTag>", "$DirOf(<FirstTag>)");
            tags.Set("<TestTag>", "$DirOf(<FirstTag>)");

            var tagManager = GetFAMTagManager(tags, tagSource);

            const string expectedResult = "UNDEFINED";
            string result = expectedResult;
            string exceptionMessage = "";

            // Act
            try
            {
                result = tagManager.ExpandTagsAndFunctions("<TestTag>", @"\\Server\Input\Attempt (1)_User_Paginated_3.pdf");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                exceptionMessage = ExtractException.FromStringizedByteStream("", ex.Message).Message;
            }

            // Assert
            Assert.Multiple(() =>
            {
                Assert.AreEqual("Runtime error: Exceeded maximum level of recursion allowed while expanding path tags!", exceptionMessage);

                // An exception is best, but if that doesn't happen, what is the result?
                Assert.AreEqual(expectedResult, result);
            });
        }

        /// <summary>
        /// There was extra code in MiscUtils.ExpandTagsAndFunctions to expand tags outside of functions but this
        /// wasn't actually doing anything
        /// </summary>
        [Test, Category("Automated")]
        public static void TagsOutsideOfFunctions()
        {
            // Arrange
            MiscUtilsClass miscUtils = new();

            // Act
            string result = miscUtils.ExpandTagsAndFunctions(
                "<SourceDocName>.$ExtOf(<SourceDocName>).<SourceDocName>", @"\\Server\Input\Attempt (1)_User_Paginated_3.pdf", null);

            // Assert
            Assert.AreEqual(@"\\Server\Input\Attempt (1)_User_Paginated_3.pdf.pdf.\\Server\Input\Attempt (1)_User_Paginated_3.pdf", result);
        }

        #region Helper Methods

        // Setup a FAMTagManager that uses custom tags, ether via a ContextTagProvider or programmatically added
        private static IFAMTagManager GetFAMTagManager(StrToStrMap tags, TagSource tagSource)
        {
            var tagManager = new FAMTagManager();
            if (tagSource == TagSource.ContextTagProvider)
            {

                VariantVector workflows = new VariantVectorClass();
                workflows.PushBack("WF");

                Mock<IContextTagProvider> contextTagProvider = new();
                contextTagProvider.Setup(x => x.GetTagValuePairsForWorkflow(It.IsAny<string>())).Returns(tags);
                contextTagProvider.Setup(x => x.GetWorkflowsThatHaveValues()).Returns(workflows);
                tagManager.SetContextTagProvider(contextTagProvider.Object);
                tagManager.RefreshContextTags();

                tagManager.Workflow = "WF";
            }
            else
            {
                ITagUtility tagUtility = (ITagUtility)tagManager;
                foreach (var pair in tags.GetAllKeyValuePairs().ToIEnumerable<IStringPair>())
                {
                    tagUtility.AddTag(pair.StringKey, pair.StringValue);
                }
            }

            return tagManager;
        }

        #endregion Helper Methods
    }
}
