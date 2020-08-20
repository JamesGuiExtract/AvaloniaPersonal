using Extract.Testing.Utilities;
using NUnit.Framework;
using System.IO;
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

        static readonly string[] _expected = new string[]
        {
            "Resources.XML.FullText.sorted.xml",
            "Resources.XML.FullTextAttribute.sorted.xml",
            "Resources.XML.FullTextAttributeNoSpatial.sorted.xml",
            "Resources.XML.FullTextAttributeSchema.sorted.xml",
            "Resources.XML.FullTextAttributeSchemaNoSpatial.sorted.xml",
            "Resources.XML.FullTextAttributeSchemaRemoveEmpty.sorted.xml",
            "Resources.XML.FullTextAttributeSchemaRemoveEmptyNoSpatial.sorted.xml",
            "Resources.XML.FullTextNoSpatial.sorted.xml",
            "Resources.XML.FullTextSchema.sorted.xml",
            "Resources.XML.FullTextSchemaNoSpatial.sorted.xml",
            "Resources.XML.Version1NoSpatial.sorted.xml",
            "Resources.XML.Version1Spatial.sorted.xml",
            "Resources.XML.FullTextSchemaRemoveEmpty.sorted.xml",
            "Resources.XML.FullTextSchemaRemoveEmptyNoSpatial.sorted.xml",
            "Resources.XML.NoFullTextWithMultilineTextNode.sorted.xml",
        };

        static TestFileManager<TestXmlTransformer> _testFiles;
        static XmlTransformer _alphaSortTransformer;

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

        /// <summary>
        /// Test alphabetic sort with various XML formats
        /// </summary>
        [Test, Category("Automated")]
        
        public static void AlphabeticSort([Range(0, _NUM_INPUTS - 1)] int index)
        {
            // Validate the inputs/outputs/range constant
            Assert.AreEqual(_inputs.Length, _NUM_INPUTS);
            Assert.AreEqual(_inputs.Length, _expected.Length);

            // Run the test
            var transformer = _alphaSortTransformer ??= new XmlTransformer(XmlTransformer.StyleSheets.AlphaSortName);
            var x = GetResult(_inputs[index], transformer, _expected[index]);

            // Validate the expected value:
            //   This transform formats the XML in addition to sorting it
            //   but the length of the input and output should be the same with whitespace removed
            Assert.AreEqual(Regex.Replace(x.Input, @"\s+", "").Length, Regex.Replace(x.Output, @"\s+", "").Length);

            // Check against the expected value
            Assert.AreEqual(x.Expected, x.Output);
        }

        /// <summary>
        /// Test running the transform twice to make sure the result is stable (no extra whitespace added)
        /// </summary>
        [Test, Category("Automated")]
        
        public static void TransformTwice([Range(0, _NUM_INPUTS - 1)] int index)
        {
            // Validate the inputs/outputs/range constant
            Assert.AreEqual(_inputs.Length, _NUM_INPUTS);
            Assert.AreEqual(_inputs.Length, _expected.Length);

            // Run the first test
            var transformer = _alphaSortTransformer ??= new XmlTransformer(XmlTransformer.StyleSheets.AlphaSortName);
            var x = GetResult(_inputs[index], transformer, _expected[index]);
            Assert.AreEqual(x.Expected, x.Output);

            // Transform the output of the first test again
            using var inputStream = GetStreamFromString(x.Output);
            x = GetResult(inputStream, transformer, _expected[index]);
            Assert.AreEqual(x.Expected, x.Output);
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

            var input = GetStringFromStream(inputStream);

            using var outputStream = new MemoryStream();
            transformer.TransformXml(inputStream, outputStream);
            var output = GetStringFromStream(outputStream);

            return new TestData(input, expected, output);
        }

        static string GetStringFromStream(Stream stream)
        {
            try
            {
                stream.Position = 0;
                using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
                return reader.ReadToEnd();
            }
            finally
            {
                stream.Position = 0;
            }
        }

        static Stream GetStreamFromString(string str)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(str));
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
