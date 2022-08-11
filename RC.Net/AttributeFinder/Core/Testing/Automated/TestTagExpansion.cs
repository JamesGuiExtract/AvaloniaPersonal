using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Test
{
    public enum TagSource
    {
        TagUtility,
        AFDocument,
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

        /// <summary>
        /// Test that a custom tag that expands to functions will work when there are parenthesis in the source document name
        /// </summary>
        [Test, Category("Automated")]
        public static void TagReferencesBuiltInTag_SpecialChars([Values] TagSource tagSource)
        {
            try
            {
                // Arrange
                StrToStrMap tags = new StrToStrMapClass();
                tags.Set("<TestTag>", @"$DirOf(<SDN>)\$FileNoExtOf($FileNoExtOf($Replace{|}(<SDN>|_User_Paginated_|.))).$ExtOf(<SDN>)");
                tags.Set("<SDN>", @"<SourceDocName>");

                var tagExpander = GetTagExpander(tags, tagSource, @"\\Server\Input\Attempt (1)_User_Paginated_3.pdf");

                // Act
                string res = tagExpander("<TestTag>");

                // Assert
                Assert.AreEqual(@"\\Server\Input\Attempt (1).pdf", res);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                throw ex.AsExtract("ELI53554");
            }
        }

        /// <summary>
        /// Confirm that custom tags can be defined with functions and other custom tags
        /// </summary>
        [Test, Category("Automated")]
        public static void NestedTagsAndFunctions([Values] TagSource tagSource)
        {
            try
            {
                // Arrange
                StrToStrMap tags = new StrToStrMapClass();
                tags.Set("<ProtectPeriods>", @"$Replace(<SDN>,<PERIOD>,<PIPE>)");
                tags.Set("<RemoveStuff>", @"$FileNoExtOf($Replace(<ProtectPeriods>,<COMMA>,<PERIOD>))");
                tags.Set("<Unmangle>", @"$Replace($Replace(<RemoveStuff>,<PERIOD>,<COMMA>),<PIPE>,<PERIOD>)");
                tags.Set("<Finalize>", @"$DirOf(<SDN>)\<Unmangle>.$ExtOf(<SDN>)");
                tags.Set("<PERIOD>", ".");
                tags.Set("<PIPE>", "|");
                tags.Set("<COMMA>", ",");
                tags.Set("<SDN>", @"<SourceDocName>");

                var tagExpander = GetTagExpander(tags, tagSource, @"\\Server\Input\A man, a plan, a canal (panama).pdf");

                // Act
                string res = tagExpander("<Finalize>");

                // Assert
                Assert.AreEqual(@"\\Server\Input\A man, a plan.pdf", res);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                throw ex.AsExtract("ELI53556");
            }
        }

        /// <summary>
        /// Ensure that a helpful exception is generated when there is an accidental cycle in tag definitions
        /// </summary>
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

            var tagExpander = GetTagExpander(tags, tagSource, @"\\Server\Input\Attempt (1)_User_Paginated_3.pdf");

            const string expectedResult = "UNDEFINED";
            string result = expectedResult;
            string exceptionMessage = "";

            // Act
            try
            {
                result = tagExpander("<TestTag>");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                exceptionMessage = ExtractException.FromStringizedByteStream("", ex.Message).Message;
            }

            // Assert
            Assert.Multiple(() =>
            {
                Assert.AreEqual("Runtime error: Cycle detected while expanding path tags!", exceptionMessage);

                // An exception is best, but if that doesn't happen, what is the result?
                Assert.AreEqual(expectedResult, result);
            });
        }

        /// <summary>
        /// Ensure that a helpful exception is generated when there is an accidental cycle in tag definitions that contain functions
        /// </summary>
        [Test, Category("Automated")]
        public static void FunctionsWithCycles([Values] TagSource tagSource)
        {
            // Arrange
            StrToStrMap tags = new StrToStrMapClass();
            tags.Set("<FirstTag>", "$DirOf(<SecondTag>)");
            tags.Set("<SecondTag>", "$DirOf(<ThirdTag>)");
            tags.Set("<ThirdTag>", "$DirOf(<FourthTag>)");
            tags.Set("<FourthTag>", "$DirOf(<FirstTag>)");
            tags.Set("<TestTag>", "$DirOf(<FirstTag>)");

            var tagExpander = GetTagExpander(tags, tagSource, @"\\Server\Input\Attempt (1)_User_Paginated_3.pdf");

            const string expectedResult = "UNDEFINED";
            string result = expectedResult;
            string exceptionMessage = "";

            // Act
            try
            {
                result = tagExpander("<TestTag>");
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

        #region Helper Methods

        // Setup a FAMTagManager that uses custom tags, ether via a ContextTagProvider or programmatically added
        private static Func<string, string> GetTagExpander(StrToStrMap tags, TagSource tagSource, string sourceDocName)
        {
            AFDocumentClass doc = new();
            doc.Text.CreateNonSpatialString("", sourceDocName);

            var tagUtility = new AFUtilityClass();
            if (tagSource == TagSource.AFDocument)
            {
                foreach (var pair in tags.GetAllKeyValuePairs().ToIEnumerable<IStringPair>())
                {
                    doc.StringTags.Set(pair.StringKey.Replace("<", "").Replace(">", ""), pair.StringValue);
                }

                return (string input) => tagUtility.ExpandTagsAndFunctions(input, doc);
            }
            else
            {
                foreach (var pair in tags.GetAllKeyValuePairs().ToIEnumerable<IStringPair>())
                {
                    tagUtility.AddTag(pair.StringKey, pair.StringValue);
                }

                return (string input) => tagUtility.ExpandTagsAndFunctions(input, sourceDocName, doc);
            }
        }

        #endregion Helper Methods
    }
}