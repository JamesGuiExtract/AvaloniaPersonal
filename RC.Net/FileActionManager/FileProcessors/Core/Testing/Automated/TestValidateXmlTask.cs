using Extract.Testing.Utilities;
using NUnit.Framework;
using System.IO;

namespace Extract.FileActionManager.FileProcessors.Test
{
    /// <summary>
    /// Provides unit test cases for the <see cref="ValidateXmlTask"/>.
    /// </summary>
    [TestFixture]
    [Category("ValidateXmlTask")]
    public class TestValidateXmlTask
    {
        #region Constants

        /// <summary>
        /// A typical LabDE XML schema definition file.
        /// </summary>
        static readonly string _LABDE_XSD = "Resources.LabDE.xsd";

        /// <summary>
        /// A typical FLEX Index XML schema definition file.
        /// </summary>
        static readonly string _FLEX_INDEX_XSD = "Resources.FlexIndex.xsd";

        /// <summary>
        /// A FLEX Index XML file using the schema _FLEX_INDEX_XSD
        /// </summary>
        static readonly string _FLEX_INDEX_XML = "Resources.FlexIndex.xml";

        /// <summary>
        /// A malformed FLEX Index XML file (missing element close).
        /// </summary>
        static readonly string _FLEX_INDEX_BAD_SYNTAX_XML = "Resources.FlexIndex-BadSyntax.xml";

        /// <summary>
        /// A LabDE XML file using the schema _LABDE_XSD that is specified inline.
        /// </summary>
        static readonly string _LABDE_INLINE = "Resources.LabDE-Inline.xml";

        /// <summary>
        /// A LabDE XML file that does not conform to schema _LABDE_XSD, yet is specified inline.
        /// </summary>
        static readonly string _LABDE_INLINE_BAD_SCHEMA = "Resources.LabDE-Inline-BadSchema.xml";

        /// <summary>
        /// A FLEX Index XML file using the wrong schema _LABDE_XSD that is specified inline.
        /// </summary>
        static readonly string _FLEX_INDEX_INLINE_WRONG_SCHEMA = "Resources.FlexIndex-Inline-WrongSchema.xml";

        /// <summary>
        /// A file with a byte with decimal value of 149 in it
        /// https://extract.atlassian.net/browse/ISSUE-17552
        /// </summary>
        static readonly string _EXTENDED_ASCII_XML = "Resources.ExtendedASCII.xml";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages the test files needed for testing.
        /// </summary>
        static TestFileManager<TestValidateXmlTask> _testFiles;

        #endregion Fields

        #region Overhead Methods

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestValidateXmlTask>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            // Dispose of the test image manager
            if (_testFiles != null)
            {
                _testFiles.Dispose();
            }
        }

        #endregion Overhead Methods

        #region Unit Tests

        /// <summary>
        /// Tests XML syntax validation.
        /// </summary>
        [Test, Category("XML Syntax")]
        public static void TestWellFormedXMLSyntax()
        {
            ValidateXmlTask validateXmlTask = new ValidateXmlTask();
            validateXmlTask.XmlFileName = "<SourceDocName>";
            validateXmlTask.TreatWarningsAsErrors = false;
            validateXmlTask.XmlSchemaValidation = XmlSchemaValidation.None;

            Assert.That(TestTaskForFailure(validateXmlTask,
                _testFiles.GetFile(_FLEX_INDEX_XML), null));
        }

        /// <summary>
        /// Tests XML syntax validation.
        /// </summary>
        [Test, Category("XML Syntax")]
        public static void TestMalformedXMLSyntax()
        {
            ValidateXmlTask validateXmlTask = new ValidateXmlTask();
            validateXmlTask.XmlFileName = "<SourceDocName>";
            validateXmlTask.TreatWarningsAsErrors = false;
            validateXmlTask.XmlSchemaValidation = XmlSchemaValidation.None;

            Assert.That(TestTaskForFailure(validateXmlTask,
                _testFiles.GetFile(_FLEX_INDEX_BAD_SYNTAX_XML), "ELI38394"));
        }

        /// <summary>
        /// Tests that XML with non-ascii code point doesn't cause an error
        /// https://extract.atlassian.net/browse/ISSUE-17552
        /// </summary>
        [Test, Category("XML Syntax")]
        public static void TestExtendedASCIIInValue()
        {
            ValidateXmlTask validateXmlTask = new ValidateXmlTask();
            validateXmlTask.XmlFileName = "<SourceDocName>";
            validateXmlTask.TreatWarningsAsErrors = false;
            validateXmlTask.XmlSchemaValidation = XmlSchemaValidation.None;

            Assert.That(TestTaskForFailure(validateXmlTask,
                _testFiles.GetFile(_EXTENDED_ASCII_XML), null));
        }

        /// <summary>
        /// Tests validation against an in-line schema.
        /// </summary>
        [Test, Category("XML Schema")]
        public static void TestValidInlineSchema()
        {
            ValidateXmlTask validateXmlTask = new ValidateXmlTask();
            validateXmlTask.XmlFileName = "<SourceDocName>";
            validateXmlTask.TreatWarningsAsErrors = false;
            validateXmlTask.XmlSchemaValidation = XmlSchemaValidation.InlineSchema;
            validateXmlTask.RequireInlineSchema = false;

            _testFiles.GetFile(_LABDE_XSD);

            Assert.That(TestTaskForFailure(validateXmlTask,
                _testFiles.GetFile(_LABDE_INLINE), null));
        }

        /// <summary>
        /// Tests validation against an in-line schema.
        /// </summary>
        [Test, Category("XML Schema")]
        public static void TestInvalidInlineSchema()
        {
            // Use a subdir to put the xml files so that this tests that relative paths are resolved correctly
            // https://extract.atlassian.net/browse/ISSUE-17562
            var tempDir = Utilities.FileSystemMethods.GetTemporaryFolder();
            try
            {
                ValidateXmlTask validateXmlTask = new ValidateXmlTask();
                validateXmlTask.XmlFileName = "<SourceDocName>";
                validateXmlTask.TreatWarningsAsErrors = false;
                validateXmlTask.XmlSchemaValidation = XmlSchemaValidation.InlineSchema;
                validateXmlTask.RequireInlineSchema = false;

                _testFiles.GetFile(_LABDE_XSD, Path.Combine(tempDir.FullName, "LabDE.xsd"));
                string xmlFile = _testFiles.GetFile(_LABDE_INLINE_BAD_SCHEMA, Path.Combine(tempDir.FullName, "SchemaViolation.xml"));

                Assert.That(TestTaskForFailure(validateXmlTask, xmlFile, "ELI38394"));
            }
            finally
            {
                // Kill cached files to avoid polluting other tests
                _testFiles.RemoveFile(_LABDE_XSD);
                _testFiles.RemoveFile(_LABDE_INLINE_BAD_SCHEMA);
                tempDir.Delete(true);
            }
        }

        /// <summary>
        /// Tests validation against an in-line schema.
        /// </summary>
        [Test, Category("XML Schema")]
        public static void TestWrongInlineSchema()
        {
            ValidateXmlTask validateXmlTask = new ValidateXmlTask();
            validateXmlTask.XmlFileName = "<SourceDocName>";
            validateXmlTask.TreatWarningsAsErrors = false;
            validateXmlTask.XmlSchemaValidation = XmlSchemaValidation.InlineSchema;
            validateXmlTask.RequireInlineSchema = true;

            _testFiles.GetFile(_LABDE_XSD);

            Assert.That(TestTaskForFailure(validateXmlTask,
                _testFiles.GetFile(_FLEX_INDEX_INLINE_WRONG_SCHEMA), "ELI38394"));
        }

        /// <summary>
        /// Tests that in-line schema is not required when not configured to be.
        /// </summary>
        [Test, Category("XML Schema")]
        public static void TestMissingInlineSchema()
        {
            ValidateXmlTask validateXmlTask = new ValidateXmlTask();
            validateXmlTask.XmlFileName = "<SourceDocName>";
            validateXmlTask.TreatWarningsAsErrors = false;
            validateXmlTask.XmlSchemaValidation = XmlSchemaValidation.InlineSchema;
            validateXmlTask.RequireInlineSchema = false;

            _testFiles.GetFile(_FLEX_INDEX_XSD);

            Assert.That(TestTaskForFailure(validateXmlTask,
                _testFiles.GetFile(_FLEX_INDEX_XML), null));
        }

        /// <summary>
        /// Tests that in-line schema is required when configured to be.
        /// </summary>
        [Test, Category("XML Schema")]
        public static void TestMissingRequiredInlineSchema()
        {
            ValidateXmlTask validateXmlTask = new ValidateXmlTask();
            validateXmlTask.XmlFileName = "<SourceDocName>";
            validateXmlTask.TreatWarningsAsErrors = false;
            validateXmlTask.XmlSchemaValidation = XmlSchemaValidation.InlineSchema;
            validateXmlTask.RequireInlineSchema = true;

            _testFiles.GetFile(_FLEX_INDEX_XSD);

            Assert.That(TestTaskForFailure(validateXmlTask,
                _testFiles.GetFile(_FLEX_INDEX_XML), "ELI38394"));
        }

        /// <summary>
        /// Tests validation against a specified schema.
        /// </summary>
        [Test, Category("XML Schema")]
        public static void TestSpecifiedSchema()
        {
            ValidateXmlTask validateXmlTask = new ValidateXmlTask();
            validateXmlTask.XmlFileName = "<SourceDocName>";
            validateXmlTask.TreatWarningsAsErrors = false;
            validateXmlTask.XmlSchemaValidation = XmlSchemaValidation.SpecifiedSchema;
            validateXmlTask.SchemaFileName = _testFiles.GetFile(_FLEX_INDEX_XSD);

            Assert.That(TestTaskForFailure(validateXmlTask,
                _testFiles.GetFile(_FLEX_INDEX_XML), null));
        }

        /// <summary>
        /// Tests validation against a specified schema.
        /// </summary>
        [Test, Category("XML Schema")]
        public static void TestSpecifiedWrongSchema()
        {
            ValidateXmlTask validateXmlTask = new ValidateXmlTask();
            validateXmlTask.XmlFileName = "<SourceDocName>";
            validateXmlTask.TreatWarningsAsErrors = false;
            validateXmlTask.XmlSchemaValidation = XmlSchemaValidation.SpecifiedSchema;
            validateXmlTask.SchemaFileName = _testFiles.GetFile(_LABDE_XSD);

            Assert.That(TestTaskForFailure(validateXmlTask,
                _testFiles.GetFile(_FLEX_INDEX_XML), "ELI38394"));
        }

        /// <summary>
        /// Tests that validation against a specified schema overrides any in-line schema.
        /// </summary>
        [Test, Category("XML Schema")]
        public static void TestSpecifiedOverridesWrongInlineSchema()
        {
            ValidateXmlTask validateXmlTask = new ValidateXmlTask();
            validateXmlTask.XmlFileName = "<SourceDocName>";
            validateXmlTask.TreatWarningsAsErrors = false;
            validateXmlTask.XmlSchemaValidation = XmlSchemaValidation.SpecifiedSchema;
            validateXmlTask.SchemaFileName = _testFiles.GetFile(_FLEX_INDEX_XSD);

            _testFiles.GetFile(_LABDE_XSD);

            Assert.That(TestTaskForFailure(validateXmlTask,
                _testFiles.GetFile(_FLEX_INDEX_INLINE_WRONG_SCHEMA), null));
        }

        /// <summary>
        /// Tests that validation fails for warnings when specified to do so. (In this case, no
        /// schema available to validate XML against).
        /// </summary>
        [Test, Category("Warnings")]
        public static void TestWarningsAsErrors()
        {
            ValidateXmlTask validateXmlTask = new ValidateXmlTask();
            validateXmlTask.XmlFileName = "<SourceDocName>";
            validateXmlTask.TreatWarningsAsErrors = true;
            validateXmlTask.XmlSchemaValidation = XmlSchemaValidation.InlineSchema;
            validateXmlTask.RequireInlineSchema = false;

            _testFiles.GetFile(_FLEX_INDEX_XSD);

            Assert.That(TestTaskForFailure(validateXmlTask,
                _testFiles.GetFile(_FLEX_INDEX_XML), "ELI38394"));
        }

        #endregion Unit Tests

        #region Private Members

        /// <summary>
        /// Tests to see whether or not the specified <see paramref="sourceDocName"/> fails
        /// validation based on the configuration of <see paramref="validateTask"/>.
        /// </summary>
        /// <param name="validateTask">The <see cref="ValidateXmlTask"/> to use to validate
        /// <see paramref="sourceDocName"/>.</param>
        /// <param name="sourceDocName">The filename <see paramref="validateTask"/> should be run
        /// against.</param>
        /// <param name="expectedFailureELI">An ELI code expected to be in the exception stack the
        /// test will fail with or <see langword="null"/> if the test is expected to succeed.
        /// </param>
        /// <returns><see langword="true"/> if the execution of <see paramref="validateTask"/>
        /// failed/succeeded as expected.</returns>
        static bool TestTaskForFailure(ValidateXmlTask validateTask, string sourceDocName,
            string expectedFailureELI)
        {
            try
            {
                validateTask.Execute(sourceDocName);
            }
            catch (ExtractException ee)
            {
                if (string.IsNullOrWhiteSpace(expectedFailureELI))
                {
                    return false;
                }
                else
                {
                    for (ExtractException eeIterator = ee; eeIterator != null;
                        eeIterator = eeIterator.InnerException as ExtractException)
                    {
                        if (eeIterator.EliCode == expectedFailureELI)
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return string.IsNullOrWhiteSpace(expectedFailureELI);
        }

        #endregion Private Members
    }
}
