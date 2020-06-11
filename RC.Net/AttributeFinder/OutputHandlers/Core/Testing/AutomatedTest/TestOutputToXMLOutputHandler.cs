using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.IO;
using UCLID_AFCORELib;
using UCLID_AFOUTPUTHANDLERSLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.AttributeFinder.OutputHandlers.Test
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    [Category("OutputToXMLOutputHandler")]
    public class TestOutputToXMLOutputHandler
    {
        // Name of file containing a list of attributes that are all spatial except the one blank node
        static readonly string _TEST_VOA = "Resources.Test.voa";

        // Names of the expected output XML files that are included as resources
        static readonly string _ATTRIBUTE = "Resources.Attribute.xml";
        static readonly string _ATTRIBUTENOSPATIAL = "Resources.AttributeNoSpatial.xml";
        static readonly string _ATTRIBUTESCHEMA = "Resources.AttributeSchema.xml";
        static readonly string _ATTRIBUTESCHEMANOSPATIAL = "Resources.AttributeSchemaNoSpatial.xml";
        static readonly string _ATTRIBUTESCHEMAREMOVEEMPTY = "Resources.AttributeSchemaRemoveEmpty.xml";
        static readonly string _ATTRIBUTESCHEMAREMOVEEMPTYNOSPATIAL = "Resources.AttributeSchemaRemoveEmptyNoSpatial.xml";
        static readonly string _FULLTEXT = "Resources.FullText.xml";
        static readonly string _FULLTEXTATTRIBUTE = "Resources.FullTextAttribute.xml";
        static readonly string _FULLTEXTATTRIBUTENOSPATIAL = "Resources.FullTextAttributeNoSpatial.xml";
        static readonly string _FULLTEXTATTRIBUTESCHEMA = "Resources.FullTextAttributeSchema.xml";
        static readonly string _FULLTEXTATTRIBUTESCHEMANOSPATIAL = "Resources.FullTextAttributeSchemaNoSpatial.xml";
        static readonly string _FULLTEXTATTRIBUTESCHEMAREMOVEEMPTY = "Resources.FullTextAttributeSchemaRemoveEmpty.xml";
        static readonly string _FULLTEXTATTRIBUTESCHEMAREMOVEEMPTYNOSPATIAL = "Resources.FullTextAttributeSchemaRemoveEmptyNoSpatial.xml";
        static readonly string _FULLTEXTNOSPATIAL = "Resources.FullTextNoSpatial.xml";
        static readonly string _FULLTEXTSCHEMA = "Resources.FullTextSchema.xml";
        static readonly string _FULLTEXTSCHEMANOSPATIAL = "Resources.FullTextSchemaNoSpatial.xml";
        static readonly string _FULLTEXTSCHEMAREMOVEEMPTY = "Resources.FullTextSchemaRemoveEmpty.xml";
        static readonly string _FULLTEXTSCHEMAREMOVEEMPTYNOSPATIAL = "Resources.FullTextSchemaRemoveEmptyNoSpatial.xml";
        static readonly string _NONE = "Resources.None.xml";
        static readonly string _NONENOSPATIAL = "Resources.NoneNoSpatial.xml";
        static readonly string _REMOVEEMPTY = "Resources.RemoveEmpty.xml";
        static readonly string _REMOVEEMPTYNOSPATIAL = "Resources.RemoveEmptyNoSpatial.xml";
        static readonly string _SCHEMA = "Resources.Schema.xml";
        static readonly string _SCHEMANOSPATIAL = "Resources.SchemaNoSpatial.xml";
        static readonly string _SCHEMAREMOVEEMPTY = "Resources.SchemaRemoveEmpty.xml";
        static readonly string _SCHEMAREMOVEEMPTYNOSPATIAL = "Resources.SchemaRemoveEmptyNoSpatial.xml";
        static readonly string _VERSION1NOSPATIAL = "Resources.Version1NoSpatial.xml";
        static readonly string _VERSION1SPATIAL = "Resources.Version1Spatial.xml";

        /// <summary>
        /// Manages the test files needed for testing.
        /// </summary>
        static TestFileManager<TestOutputToXMLOutputHandler> _testFiles;

        /// <summary>
        /// Initializes the test fixture
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestOutputToXMLOutputHandler>();
        }
        
        #region Tests

        /// <summary>
        /// Tests the output of Version 1 XML with spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void Version1Spatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLOriginal;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = false;
            outputToXML.UseSchemaName = false;
            outputToXML.ValueAsFullText = true;
            outputToXML.SchemaName = "";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_VERSION1SPATIAL)));
        }

        /// <summary>
        /// Tests the output of Version 1 XML with spatial without spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void Version1NoSpatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLOriginal;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = true;
            outputToXML.UseSchemaName = false;
            outputToXML.ValueAsFullText = true;
            outputToXML.SchemaName = "";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_VERSION1NOSPATIAL)));
        }

        /// <summary>
        /// Tests the output for Version 2 with Attributes as node names and spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void AttributeNames()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = true;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = false;
            outputToXML.UseSchemaName = false;
            outputToXML.ValueAsFullText = false;
            outputToXML.SchemaName = "";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_ATTRIBUTE)));
        }

        /// <summary>
        /// Tests the output for Version 2 with Attributes as node names and no spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void AttributeNamesNoSpatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = true;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = true;
            outputToXML.UseSchemaName = false;
            outputToXML.ValueAsFullText = false;
            outputToXML.SchemaName = "";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_ATTRIBUTENOSPATIAL)));
        }

        /// <summary>
        /// Tests the output for Version 2 with Attributes as node names and a schema
        /// </summary>
        [Test, Category("Automated")]
        public static void AttributeNamesAndSchema()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = true;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = false;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = false;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_ATTRIBUTESCHEMA)));
        }

        /// <summary>
        /// Tests the output for Version 2 with Attributes as node names, schema and no spatial
        /// info
        /// </summary>
        [Test, Category("Automated")]
        public static void AttributeNamesAndSchemaNoSpatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = true;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = true;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = false;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_ATTRIBUTESCHEMANOSPATIAL)));
        }

        /// <summary>
        /// Tests the output for Version 2 with Attributes as node names, schema and remove empty
        /// nodes
        /// </summary>
        [Test, Category("Automated")]
        public static void AttributeSchemaRemoveEmpty()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = true;
            outputToXML.RemoveEmptyNodes = true;
            outputToXML.RemoveSpatialInfo = false;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = false;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_ATTRIBUTESCHEMAREMOVEEMPTY)));
        }

        /// <summary>
        /// Tests the output for Version 2 with Attributes as node names, schema, remove empty nodes
        /// and no spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void AttributeSchemaRemoveEmptyNoSpatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = true;
            outputToXML.RemoveEmptyNodes = true;
            outputToXML.RemoveSpatialInfo = true;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = false;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_ATTRIBUTESCHEMAREMOVEEMPTYNOSPATIAL)));
        }

        /// <summary>
        /// Tests the output for Version 2 with full text node
        /// </summary>
        [Test, Category("Automated")]
        public static void FullText()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = false;
            outputToXML.UseSchemaName = false;
            outputToXML.ValueAsFullText = true;
            outputToXML.SchemaName = "";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_FULLTEXT)));
        }

        /// <summary>
        /// Tests the output for Version 2 with full text node and attributes as node names
        /// </summary>
        [Test, Category("Automated")]
        public static void FullTextAttribute()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = true;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = false;
            outputToXML.UseSchemaName = false;
            outputToXML.ValueAsFullText = true;
            outputToXML.SchemaName = "";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_FULLTEXTATTRIBUTE)));
        }

        /// <summary>
        /// Tests the output for Version 2 with full text node, attributes as node names and no 
        /// spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void FullTextAttributeNoSpatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = true;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = true;
            outputToXML.UseSchemaName = false;
            outputToXML.ValueAsFullText = true;
            outputToXML.SchemaName = "";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_FULLTEXTATTRIBUTENOSPATIAL)));
        }

        /// <summary>
        /// Tests the output for Version 2 with full text node, attributes as node names and schema
        /// </summary>
        [Test, Category("Automated")]
        public static void FullTextAttributeSchema()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = true;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = false;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = true;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_FULLTEXTATTRIBUTESCHEMA)));
        }

        /// <summary>
        /// Tests the output for Version 2 with full text node, attributes as node names, schema
        /// and no spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void FullTextAttributeSchemaNoSpatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = true;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = true;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = true;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_FULLTEXTATTRIBUTESCHEMANOSPATIAL)));
        }

        /// <summary>
        /// Tests the output for Version 2 with full text node, attributes as node names, schema and 
        /// remove empty nodes
        /// </summary>
        [Test, Category("Automated")]
        public static void FullTextAttributeSchemaRemoveEmpty()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = true;
            outputToXML.RemoveEmptyNodes = true;
            outputToXML.RemoveSpatialInfo = false;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = true;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_FULLTEXTATTRIBUTESCHEMAREMOVEEMPTY)));
        }

        /// <summary>
        /// Tests the output for Version 2 with full text node, attributes as node names, schema,
        /// remove empty nodes and no spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void FullTextAttributeSchemaRemoveEmptyNoSpatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = true;
            outputToXML.RemoveEmptyNodes = true;
            outputToXML.RemoveSpatialInfo = true;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = true;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_FULLTEXTATTRIBUTESCHEMAREMOVEEMPTYNOSPATIAL)));
        }

        /// <summary>
        /// Tests the output for Version 2 with full text node and no spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void FullTextNoSpatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = true;
            outputToXML.UseSchemaName = false;
            outputToXML.ValueAsFullText = true;
            outputToXML.SchemaName = "";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_FULLTEXTNOSPATIAL)));
        }

        /// <summary>
        /// Tests the output for Version 2 with full text node and schema
        /// </summary>
        [Test, Category("Automated")]
        public static void FullTextSchema()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = false;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = true;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_FULLTEXTSCHEMA)));
        }

        /// <summary>
        /// Tests the output for Version 2 with full text node, schema and no spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void FullTextSchemaNoSpatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = true;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = true;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_FULLTEXTSCHEMANOSPATIAL)));
        }

        /// <summary>
        /// Tests the output for Version 2 with full text node, schema and remove empty nodes
        /// </summary>
        [Test, Category("Automated")]
        public static void FullTextSchemaRemoveEmpty()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = true;
            outputToXML.RemoveSpatialInfo = false;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = true;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_FULLTEXTSCHEMAREMOVEEMPTY)));
        }

        /// <summary>
        /// Tests the output for Version 2 with full text node, schema, remove empty nodes and
        /// no spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void FullTextSchemaRemoveEmptyNoSpatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = true;
            outputToXML.RemoveSpatialInfo = true;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = true;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_FULLTEXTSCHEMAREMOVEEMPTYNOSPATIAL)));
        }

        /// <summary>
        /// Tests the output for Version 2 no options selected
        /// </summary>
        [Test, Category("Automated")]
        public static void None()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = false;
            outputToXML.UseSchemaName = false;
            outputToXML.ValueAsFullText = false;
            outputToXML.SchemaName = "";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_NONE)));
        }

        /// <summary>
        /// Tests the output for Version 2 with no options selected and no spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void NoneNoSpatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = true;
            outputToXML.UseSchemaName = false;
            outputToXML.ValueAsFullText = false;
            outputToXML.SchemaName = "";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_NONENOSPATIAL)));
        }

        /// <summary>
        /// Tests the output for Version 2 with remove empty nodes
        /// </summary>
        [Test, Category("Automated")]
        public static void RemoveEmpty()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = true;
            outputToXML.RemoveSpatialInfo = false;
            outputToXML.UseSchemaName = false;
            outputToXML.ValueAsFullText = false;
            outputToXML.SchemaName = "";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_REMOVEEMPTY)));
        }

        /// <summary>
        /// Tests the output for Version 2 with remove empty nodes and no spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void RemoveEmptyNoSpatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = true;
            outputToXML.RemoveSpatialInfo = true;
            outputToXML.UseSchemaName = false;
            outputToXML.ValueAsFullText = false;
            outputToXML.SchemaName = "";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_REMOVEEMPTYNOSPATIAL)));
        }

        /// <summary>
        /// Tests the output for Version 2 with schema
        /// </summary>
        [Test, Category("Automated")]
        public static void Schema()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = false;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = false;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_SCHEMA)));
        }

        /// <summary>
        /// Tests the output for Version 2 with schema and no spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void SchemaNoSpatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = false;
            outputToXML.RemoveSpatialInfo = true;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = false;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_SCHEMANOSPATIAL)));
        }

        /// <summary>
        /// Tests the output for Version 2 with schema and remove empty nodes
        /// </summary>
        [Test, Category("Automated")]
        public static void SchemaRemoveEmpty()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = true;
            outputToXML.RemoveSpatialInfo = false;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = false;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_SCHEMAREMOVEEMPTY)));
        }


        /// <summary>
        /// Tests the output for Version 2 with schema, remove empty nodes and no spatial info
        /// </summary>
        [Test, Category("Automated")]
        public static void SchemaRemoveEmptyNoSpatial()
        {
            OutputToXML outputToXML = new OutputToXML();
            string tempOutputFile = FileSystemMethods.GetTemporaryFileName(".xml");
            outputToXML.FileName = tempOutputFile;
            outputToXML.Format = EXMLOutputFormat.kXMLSchema;
            outputToXML.NamedAttributes = false;
            outputToXML.RemoveEmptyNodes = true;
            outputToXML.RemoveSpatialInfo = true;
            outputToXML.UseSchemaName = true;
            outputToXML.ValueAsFullText = false;
            outputToXML.SchemaName = "test";

            Assert.That(PerformTest(outputToXML, _testFiles.GetFile(_SCHEMAREMOVEEMPTYNOSPATIAL)));
        }
        
        #endregion

        #region Helper methods

        /// <summary>
        /// Performs the test using the <see paramref="outputToXML"/> and comparing the output to
        /// the contents of the file named <see paramref="fileNameOfExpected"/>
        /// </summary>
        /// <param name="outputToXML">OutputToXML output handler configured for the test.</param>
        /// <param name="fileNameOfExpected">Name of the file containing the expected results
        /// for the test</param>
        /// <returns><see langword="true"/> if the output of the <see paramref="outputToXML"/>
        /// output handler matches the expected output in file <see paramref="fileNameOfExpected"/>
        /// otherwise returns <see langword="false"/></returns>
        static bool PerformTest(OutputToXML outputToXML, string fileNameOfExpected)
        {
            // Load the voa
            IUnknownVector attributes = new IUnknownVector();
            attributes.LoadFrom(_testFiles.GetFile(_TEST_VOA), false);

            // Create a fake AFDocument object for the output handler
            SpatialString emptySpatialString = new SpatialString();

            AFDocument emtpyAFDocument = new AFDocument();
            emtpyAFDocument.Text = emptySpatialString;

            IOutputHandler outputHandler = (IOutputHandler)outputToXML;
            outputHandler.ProcessOutput(attributes, emtpyAFDocument, null);

            return CompareFiles(outputToXML.FileName, fileNameOfExpected);
        }

        /// <summary>
        /// Compares the 2 files with the names <see paramref="file1" /> and <see paramref="file2" />
        /// </summary>
        /// <param name="file1">Name of first file to compare</param>
        /// <param name="file2">Name of second file to compare</param>
        /// <returns><see langword="true"/> if the files match, <see langword="false"/> if they
        /// do not match</returns>
        static bool CompareFiles(string file1, string file2)
        {
            // Load the files and compare
            string file1Contents = File.ReadAllText(file1);
            string file2Contents = File.ReadAllText(file2);

            return file1Contents == file2Contents;
        } 
        #endregion
    }

}
