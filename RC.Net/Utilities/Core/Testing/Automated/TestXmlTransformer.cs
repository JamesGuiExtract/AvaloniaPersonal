using Extract.Testing.Utilities;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Extract.Utilities.Test
{
    [TestFixture]
    [Category("XmlTransformer")]
    public class TestXmlTransformer
    {
        const int _NUM_INPUTS = 15;
        static readonly string[] _inputs = new string[]
        {
            "Resources.XML.FullText.xml",
            "Resources.XML.FullTextAttribute.xml",
            "Resources.XML.FullTextAttributeNoSpatial.xml",
            "Resources.XML.FullTextAttributeSchema.xml",
            "Resources.XML.FullTextAttributeSchemaNoSpatial.xml",
            "Resources.XML.FullTextAttributeSchemaRemoveEmpty.xml",
            "Resources.XML.FullTextAttributeSchemaRemoveEmptyNoSpatial.xml",
            "Resources.XML.FullTextNoSpatial.xml",
            "Resources.XML.FullTextSchema.xml",
            "Resources.XML.FullTextSchemaNoSpatial.xml",
            "Resources.XML.Version1NoSpatial.xml",
            "Resources.XML.Version1Spatial.xml",
            "Resources.XML.FullTextSchemaRemoveEmpty.xml",
            "Resources.XML.FullTextSchemaRemoveEmptyNoSpatial.xml",
            "Resources.XML.NoFullTextWithMultilineTextNode.xml",
        };

        static readonly string[] _expectedAlphaByName = new string[]
        {
            "Resources.XML.AlphaByName.FullText.sorted.xml",
            "Resources.XML.AlphaByName.FullTextAttribute.sorted.xml",
            "Resources.XML.AlphaByName.FullTextAttributeNoSpatial.sorted.xml",
            "Resources.XML.AlphaByName.FullTextAttributeSchema.sorted.xml",
            "Resources.XML.AlphaByName.FullTextAttributeSchemaNoSpatial.sorted.xml",
            "Resources.XML.AlphaByName.FullTextAttributeSchemaRemoveEmpty.sorted.xml",
            "Resources.XML.AlphaByName.FullTextAttributeSchemaRemoveEmptyNoSpatial.sorted.xml",
            "Resources.XML.AlphaByName.FullTextNoSpatial.sorted.xml",
            "Resources.XML.AlphaByName.FullTextSchema.sorted.xml",
            "Resources.XML.AlphaByName.FullTextSchemaNoSpatial.sorted.xml",
            "Resources.XML.AlphaByName.Version1NoSpatial.sorted.xml",
            "Resources.XML.AlphaByName.Version1Spatial.sorted.xml",
            "Resources.XML.AlphaByName.FullTextSchemaRemoveEmpty.sorted.xml",
            "Resources.XML.AlphaByName.FullTextSchemaRemoveEmptyNoSpatial.sorted.xml",
            "Resources.XML.AlphaByName.NoFullTextWithMultilineTextNode.sorted.xml",
        };

        static readonly string[] _expectedAlphaByNameFTF = new string[]
        {
            "Resources.XML.AlphaByNameFullTextFirst.FullText.sorted.xml",
            "Resources.XML.AlphaByNameFullTextFirst.FullTextAttribute.sorted.xml",
            "Resources.XML.AlphaByNameFullTextFirst.FullTextAttributeNoSpatial.sorted.xml",
            "Resources.XML.AlphaByNameFullTextFirst.FullTextAttributeSchema.sorted.xml",
            "Resources.XML.AlphaByNameFullTextFirst.FullTextAttributeSchemaNoSpatial.sorted.xml",
            "Resources.XML.AlphaByNameFullTextFirst.FullTextAttributeSchemaRemoveEmpty.sorted.xml",
            "Resources.XML.AlphaByNameFullTextFirst.FullTextAttributeSchemaRemoveEmptyNoSpatial.sorted.xml",
            "Resources.XML.AlphaByNameFullTextFirst.FullTextNoSpatial.sorted.xml",
            "Resources.XML.AlphaByNameFullTextFirst.FullTextSchema.sorted.xml",
            "Resources.XML.AlphaByNameFullTextFirst.FullTextSchemaNoSpatial.sorted.xml",
            "Resources.XML.AlphaByNameFullTextFirst.Version1NoSpatial.sorted.xml",
            "Resources.XML.AlphaByNameFullTextFirst.Version1Spatial.sorted.xml",
            "Resources.XML.AlphaByNameFullTextFirst.FullTextSchemaRemoveEmpty.sorted.xml",
            "Resources.XML.AlphaByNameFullTextFirst.FullTextSchemaRemoveEmptyNoSpatial.sorted.xml",
            "Resources.XML.AlphaByNameFullTextFirst.NoFullTextWithMultilineTextNode.sorted.xml",
        };

        static TestFileManager<TestXmlTransformer> _testFiles;
        static XmlTransformer _alphaSortTransformer = new XmlTransformer(XmlTransformer.StyleSheets.AlphaSortName);
        static XmlTransformer _alphaSortFTFTransformer = new XmlTransformer(XmlTransformer.StyleSheets.AlphaSortNameFullTextFirst);

        /// <summary>
        /// Performs initialization needed for the entire test run.
        /// </summary>
        [OneTimeSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestXmlTransformer>("A3A4ED53-06B1-4A4E-A288-A451227CA08F");
        }

        /// <summary>
        /// Performs tear down needed after entire test run.
        /// </summary>
        [OneTimeTearDown]
        public static void Teardown()
        {
            _testFiles?.Dispose();
        }

        // Helper method to test a single transform on a single example
        static void TestTransformer(XmlTransformer transformer, string expected, string input)
        {
            // Run the test
            var x = GetResult(input, transformer, expected);

            // Validate the expected value:
            //   This transform formats the XML in addition to sorting it
            //   but the length of the input and output should be the same with whitespace removed
            Assert.AreEqual(Regex.Replace(x.Input, @"\s+", "").Length, Regex.Replace(x.Output, @"\s+", "").Length);

            // Check against the expected value
            Assert.AreEqual(x.Expected, x.Output);
        }

        /// <summary>
        /// Test alphabetic sort with various XML formats
        /// </summary>
        [Test, Category("Automated")]
        public static void AlphabeticSort([Range(0, _NUM_INPUTS - 1)] int index)
        {
            var transformer = _alphaSortTransformer;
            var expected = _expectedAlphaByName;

            // Validate the inputs/outputs/range constant
            Assert.AreEqual(_inputs.Length, _NUM_INPUTS);
            Assert.AreEqual(_inputs.Length, expected.Length);

            TestTransformer(transformer, expected[index], _inputs[index]);
        }

        /// <summary>
        /// Test alphabetic sort, fulltext-first, with various XML formats
        /// </summary>
        [Test, Category("Automated")]
        public static void AlphabeticSortFullTextFirst([Range(0, _NUM_INPUTS - 1)] int index)
        {
            var transformer = _alphaSortFTFTransformer;
            var expected = _expectedAlphaByNameFTF;

            // Validate the inputs/outputs/range constant
            Assert.AreEqual(_inputs.Length, _NUM_INPUTS);
            Assert.AreEqual(_inputs.Length, expected.Length);

            TestTransformer(transformer, expected[index], _inputs[index]);
        }

        // Helper method to test a double transform on a single example
        static void TestTransformTwice(XmlTransformer transformer, string expected, string input)
        {
            // Run the first test
            var x = GetResult(input, transformer, expected);
            Assert.AreEqual(x.Expected, x.Output);

            // Transform the output of the first test again
            using var inputStream = GetStreamFromString(x.Output, transformer.OutputEncoding);
            x = GetResult(inputStream, transformer, expected);
            Assert.AreEqual(x.Expected, x.Output);
        }

        /// <summary>
        /// Test running the transform twice to make sure the result is stable (no extra whitespace added)
        /// </summary>
        [Test, Category("Automated")]
        public static void TransformTwice([Range(0, _NUM_INPUTS - 1)] int index)
        {
            var transformer = _alphaSortTransformer;
            var expected = _expectedAlphaByName;

            // Validate the inputs/outputs/range constant
            Assert.AreEqual(_inputs.Length, _NUM_INPUTS);
            Assert.AreEqual(_inputs.Length, expected.Length);

            TestTransformTwice(transformer, expected[index], _inputs[index]);
        }

        /// <summary>
        /// Test running the FTF transform twice to make sure the result is stable (no extra whitespace added)
        /// </summary>
        [Test, Category("Automated")]
        public static void TransformTwiceFTF([Range(0, _NUM_INPUTS - 1)] int index)
        {
            var transformer = _alphaSortFTFTransformer;
            var expected = _expectedAlphaByNameFTF;

            // Validate the inputs/outputs/range constant
            Assert.AreEqual(_inputs.Length, _NUM_INPUTS);
            Assert.AreEqual(_inputs.Length, expected.Length);

            TestTransformTwice(transformer, expected[index], _inputs[index]);
        }

        /// <summary>
        /// Confirm that text output method works correctly
        /// </summary>
        [Test, Category("Automated")]
        public static void TextOutput()
        {
            var stylesheet = @"
                <xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
                    <xsl:output method=""text""/>
                    <xsl:template match=""node()"">
                        <xsl:value-of select=""text()""/>
                        <xsl:apply-templates select=""node()""/>
                    </xsl:template>
                </xsl:stylesheet>";

            var transformer = new XmlTransformer(stylesheet);

            var input = "<root>Unescape:\r\n&lt;Quoted&gt;&apos;&amp;&apos;&lt;/Quoted&gt;</root>";
            var expected = "Unescape:\r\n<Quoted>'&'</Quoted>";
            var output = new MemoryStream();
            transformer.TransformXml(GetStreamFromString(input, transformer.OutputEncoding), output);

            var actual = GetStringFromStream(output, transformer.OutputEncoding);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Container for a transformer and an example output XML that matches the output properties of the transform
        /// </summary>
        class TransformWithExampleOutputXml
        {
            public string Description { get; }
            public XmlTransformer Transformer { get; }
            public string XmlResource { get; }
            public TransformWithExampleOutputXml(string description, string transform, string xmlResource)
            {
                Description = description;
                Transformer = new XmlTransformer(transform);
                XmlResource = xmlResource;
            }
        }

        /// <summary>
        /// Create an 'identity transform' using the supplied output settings node
        /// </summary>
        /// <param name="outputNode">The xsl:output node to use in the transform</param>
        static string BuildIdentityTransform(string outputNode)
        {
            return UtilityMethods.FormatInvariant($@"
                <xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
                    {outputNode}
                    <xsl:strip-space elements=""*""/>
                    <xsl:template match=""node()|@*"">
                        <xsl:copy>
                            <xsl:apply-templates select=""node()|@*""/>
                        </xsl:copy>
                    </xsl:template>
                </xsl:stylesheet>");
        }

        /// <summary>
        /// Possible inputs/outputs to test that character encodings are properly handled
        /// </summary>
        static readonly TransformWithExampleOutputXml[] _identityTransformers =
        {
            new TransformWithExampleOutputXml(
                description: "windows-1252 with declaration",
                transform: BuildIdentityTransform(@"<xsl:output method=""xml"" indent=""yes"" omit-xml-declaration=""no"" encoding=""windows-1252""/>"),
                xmlResource: "Resources.XML.Encoding.Windows-1252_WithDeclaration.xml"),

            new TransformWithExampleOutputXml(
                description: "windows-1252 no declaration",
                transform: BuildIdentityTransform(@"<xsl:output method=""xml"" indent=""yes"" omit-xml-declaration=""yes"" encoding=""windows-1252""/>"),
                xmlResource: "Resources.XML.Encoding.Windows-1252_NoDeclaration.xml"),

            new TransformWithExampleOutputXml(
                description: "utf-8 with declaration",
                transform: BuildIdentityTransform(@"<xsl:output method=""xml"" indent=""yes"" omit-xml-declaration=""no"" encoding=""utf-8""/>"),
                xmlResource: "Resources.XML.Encoding.UTF-8_WithDeclaration.xml"),

            new TransformWithExampleOutputXml(
                description: "utf-8 no declaration",
                transform: BuildIdentityTransform(@"<xsl:output method=""xml"" indent=""yes"" omit-xml-declaration=""yes"" encoding=""utf-8""/>"),
                xmlResource: "Resources.XML.Encoding.UTF-8_NoDeclaration.xml"),

            new TransformWithExampleOutputXml(
                description: "utf-8 no bom with declaration",
                transform: BuildIdentityTransform(@"<xsl:output method=""xml"" indent=""yes"" omit-xml-declaration=""no"" encoding=""utf-8""/>"),
                xmlResource: "Resources.XML.Encoding.UTF-8_NoBOM_WithDeclaration.xml"),

            new TransformWithExampleOutputXml(
                description: "utf-8 no bom no declaration",
                transform: BuildIdentityTransform(@"<xsl:output method=""xml"" indent=""yes"" omit-xml-declaration=""yes"" encoding=""utf-8""/>"),
                xmlResource: "Resources.XML.Encoding.UTF-8_NoBOM_NoDeclaration.xml"),

            new TransformWithExampleOutputXml(
                description: "utf-16 big endian with declaration",
                transform: BuildIdentityTransform(@"<xsl:output method=""xml"" indent=""yes"" omit-xml-declaration=""no"" encoding=""utf-16be""/>"),
                xmlResource: "Resources.XML.Encoding.UTF-16_BE_WithDeclaration.xml"),

            new TransformWithExampleOutputXml(
                description: "utf-16 big endian no declaration",
                transform: BuildIdentityTransform(@"<xsl:output method=""xml"" indent=""yes"" omit-xml-declaration=""yes"" encoding=""utf-16be""/>"),
                xmlResource: "Resources.XML.Encoding.UTF-16_BE_NoDeclaration.xml"),
            
            new TransformWithExampleOutputXml(
                description: "utf-16 little endian with declaration",
                transform: BuildIdentityTransform(@"<xsl:output method=""xml"" indent=""yes"" omit-xml-declaration=""no"" encoding=""utf-16le""/>"),
                xmlResource: "Resources.XML.Encoding.UTF-16_LE_WithDeclaration.xml"),

            new TransformWithExampleOutputXml(
                description: "utf-16 little endian no declaration",
                transform: BuildIdentityTransform(@"<xsl:output method=""xml"" indent=""yes"" omit-xml-declaration=""yes"" encoding=""utf-16le""/>"),
                xmlResource: "Resources.XML.Encoding.UTF-16_LE_NoDeclaration.xml"),
        };
        const int _NUM_ENCODING_TRANSFORMS = 10;

        /// <summary>
        /// Test that every input encoding can be recognized correctly (with some exceptions)
        /// </summary>
        /// <param name="fromIdx">The example index from which to get the input XML</param>
        /// <param name="toIdx">The example index from which to get the transformer and expected output XML</param>
        [Test, Category("Automated")]
        public static void ExtendedASCIIXml([Range(0, _NUM_ENCODING_TRANSFORMS - 1)] int fromIdx, [Range(0, _NUM_ENCODING_TRANSFORMS - 1)] int toIdx)
        {
            // Validate the range constant
            Assert.AreEqual(_identityTransformers.Length, _NUM_ENCODING_TRANSFORMS);

            var from = _identityTransformers[fromIdx];
            var to = _identityTransformers[toIdx];
            var desc = UtilityMethods.FormatInvariant($"Convert from {from.Description} to {to.Description}");

            var inputRes = from.XmlResource;
            var expectedRes = to.XmlResource;
            var transformer = to.Transformer;
            var expectedEncoding = transformer.OutputEncoding;

            using var inputStream = GetStreamFromResource(inputRes);
            var output = new MemoryStream();
            transformer.TransformXml(inputStream, output);

            var shouldHaveUTF8ByteOrderMark = expectedEncoding.EncodingName == "Unicode (UTF-8)";
            Assert.AreEqual(shouldHaveUTF8ByteOrderMark, HasUTF8ByteOrderMark(output), message: desc);

            var shouldHaveUTF16ByteOrderMark = expectedEncoding.EncodingName == "Unicode";
            Assert.AreEqual(shouldHaveUTF16ByteOrderMark, HasUTF16LittleEndianByteOrderMark(output), message: desc);

            var shouldHaveUTF16BEByteOrderMark = expectedEncoding.EncodingName == "Unicode (Big-Endian)";
            Assert.AreEqual(shouldHaveUTF16BEByteOrderMark, HasUTF16BigEndianByteOrderMark(output), message: desc);

            var expected = GetStringFromStream(GetStreamFromResource(expectedRes), expectedEncoding);
            var actual = GetStringFromStream(output, expectedEncoding);

            if (desc.StartsWith("Convert from utf-8 no bom no declaration"))
            {
                // Currently the encoding is assumed to be windows-1252 if there is no BOM and no encoding attribute in the XML.
                // If the encoding is utf-8 but doesn't have a BOM or encoding attribute then the input will not be read correctly.
                Assert.AreNotEqual(expected, actual, message: UtilityMethods.FormatCurrent($"Formerly failed but succeeded! Update this test: {desc}"));
            }
            else
            {
                Assert.AreEqual(expected, actual, message: desc);
            }
        }

        static TestData GetResult(string inputResource, XmlTransformer transformer, string expectedResource)
        {
            var inputPath = _testFiles.GetFile(inputResource);
            using var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return GetResult(inputStream, transformer, expectedResource);
        }

        static TestData GetResult(Stream inputStream, XmlTransformer transformer, string expectedResource)
        {
            var expectedPath = _testFiles.GetFile(expectedResource);
            var expected = File.ReadAllText(expectedPath);

            var input = GetStringFromStream(inputStream, transformer.OutputEncoding);

            using var outputStream = new MemoryStream();
            transformer.TransformXml(inputStream, outputStream);

            // https://extract.atlassian.net/browse/ISSUE-17371
            Assert.IsFalse(HasUTF8ByteOrderMark(outputStream));
            Assert.IsFalse(HasUTF16LittleEndianByteOrderMark(outputStream));
            Assert.IsFalse(HasUTF16BigEndianByteOrderMark(outputStream));

            var output = GetStringFromStream(outputStream, transformer.OutputEncoding);

            return new TestData(input, expected, output);
        }

        static string GetStringFromStream(Stream stream, Encoding encoding)
        {
            try
            {
                stream.Position = 0;
                using var reader = new StreamReader(stream, encoding, true, 1024, true);
                return reader.ReadToEnd();
            }
            finally
            {
                stream.Position = 0;
            }
        }

        static Stream GetStreamFromString(string str, Encoding encoding)
        {
            return new MemoryStream(encoding.GetBytes(str));
        }

        static Stream GetStreamFromResource(string resName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(TestXmlTransformer), resName);
        }

        static bool HasUTF8ByteOrderMark(Stream stream)
        {
            try
            {
                stream.Position = 0;
                var bom = new byte[3];
                return stream.Read(bom, 0, 3) == 3
                    && bom[0] == 0xEF
                    && bom[1] == 0xBB
                    && bom[2] == 0xBF;
            }
            finally
            {
                stream.Position = 0;
            }
        }

        static bool HasUTF16LittleEndianByteOrderMark(Stream stream)
        {
            try
            {
                stream.Position = 0;
                var bom = new byte[2];
                var read = stream.Read(bom, 0, 2);
                return read == 2
                    && bom[0] == 0xFF
                    && bom[1] == 0xFE;
            }
            finally
            {
                stream.Position = 0;
            }
        }

        static bool HasUTF16BigEndianByteOrderMark(Stream stream)
        {
            try
            {
                stream.Position = 0;
                var bom = new byte[2];
                var read = stream.Read(bom, 0, 2);
                return read == 2
                    && bom[0] == 0xFE
                    && bom[1] == 0xFF;
            }
            finally
            {
                stream.Position = 0;
            }
        }

        class TestData
        {
            public string Input { get; }
            public string Expected { get; }
            public string Output { get; }
            public TestData(string input, string expected, string actual)
            {
                Input = input;
                Expected = expected;
                Output = actual;
            }
        }
    }
}
