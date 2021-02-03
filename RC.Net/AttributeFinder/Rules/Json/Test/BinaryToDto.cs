using Extract.Interop;
using Extract.Testing.Utilities;
using Newtonsoft.Json;
using NUnit.Framework;
using System.IO;
using System.Text;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules.Json.Test
{
    [TestFixture]
    [Category("JsonRuleObjects")]
    public class BinaryToDto
    {
        #region Fields

        static TestFileManager<BinaryToDto> _testFiles;

        #endregion Fields

        #region Setup and Teardown

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<BinaryToDto>();
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            if (_testFiles != null)
            {
                _testFiles.Dispose();
            }
        }

        #endregion Setup and Teardown

        #region Public Test Functions

        /// <summary>
        /// Load legacy rule objects from binary into DTO and compare with current version loaded from JSON
        /// </summary>
        [Category("LegacyRulesJsonDeserialization")]
        [TestCase("Resources.RuleObjects.FSharpPreprocessor.hex.txt", "Resources.RuleObjects.FSharpPreprocessorV2.json", TestName = "LegacyFSharpPreprocessor")]
        [TestCase("Resources.RuleObjects.MicrFinderV2.hex.txt", "Resources.RuleObjects.MicrFinderV2.1.json", TestName = "LegacyMICRFinder")]
        public static void LegacyRuleObjects(string legacyResourceName, string currentVersionResourceName)
        {
            var legacyFile = _testFiles.GetFile(legacyResourceName);
            var currentVersionJsonFile = _testFiles.GetFile(currentVersionResourceName);
            var serializer = JsonSerializer.CreateDefault(RuleObjectJsonSerializer.Settings);

            using var currentVersionStream = new FileStream(currentVersionJsonFile, FileMode.Open);
            using var currentVersionStreamReader = new StreamReader(currentVersionStream);
            using var currentVersionJsonReader = new JsonTextReader(currentVersionStreamReader);

            var lines = File.ReadLines(legacyFile);
            var lineNum = 0;
            var objectsCompared = 0;

            // Each line in legacyFile is a hex-string-encoded ObjectWithDescription where the Object is the rule object being tested
            foreach (var line in lines)
            {
                lineNum++;

                var sourceBytes = line.ToByteArray();
                var stream = new MemoryStream(sourceBytes);
                var istream = new IStreamWrapper(stream);
                var ipersistStream = (IPersistStream)new ObjectWithDescriptionClass().Clone();
                ipersistStream.Load(istream);
                var owd = (IObjectWithDescription)ipersistStream;

                var (_, dtoFromBinary) = RuleObjectJsonSerializer.Serialize<IObjectWithDescription, Dto.ObjectWithDescription>(owd);

                // Each item in the array in currentVersionJsonFile is an ObjectWithDescription where the Object is the expected
                // current version of the object being tested
                while (currentVersionJsonReader.Read() && currentVersionJsonReader.TokenType != JsonToken.StartObject) ;
                if (currentVersionJsonReader.TokenType == JsonToken.StartObject)
                {
                    var dtoExpectedResultFromJson = serializer.Deserialize<Dto.ObjectWithDescription>(currentVersionJsonReader);
                    Assert.AreEqual(dtoExpectedResultFromJson, dtoFromBinary);
                    objectsCompared++;
                }
            }

            Assert.AreEqual(lineNum, objectsCompared, "Less than expected JSON objects were found");
        }

        /// <summary>
        /// Load current version rule objects from binary into DTO and compare with current version loaded from JSON
        /// </summary>
        [Category("RulesJsonSerialization")]
        [TestCase("Resources.RuleObjects.FSharpPreprocessorV2.hex.txt", "Resources.RuleObjects.FSharpPreprocessorV2.json", TestName = "FSharpPreprocessorV2")]
        [TestCase("Resources.RuleObjects.MicrFinderV2.1.hex.txt", "Resources.RuleObjects.MicrFinderV2.1.json", TestName = "MicrFinderV2.1")]
        public static void CurrentVersionRuleObjects(string binaryResourceName, string currentVersionResourceName)
        {
            var legacyFile = _testFiles.GetFile(binaryResourceName);
            var currentVersionJsonFile = _testFiles.GetFile(currentVersionResourceName);
            var serializer = JsonSerializer.CreateDefault(RuleObjectJsonSerializer.Settings);

            using var currentVersionStream = new FileStream(currentVersionJsonFile, FileMode.Open);
            using var currentVersionStreamReader = new StreamReader(currentVersionStream);
            using var currentVersionJsonReader = new JsonTextReader(currentVersionStreamReader);

            var lines = File.ReadLines(legacyFile);
            var lineNum = 0;
            var objectsCompared = 0;

            // Each line in legacyFile is a hex-string-encoded ObjectWithDescription where the Object is the rule object being tested
            foreach (var line in lines)
            {
                lineNum++;

                var sourceBytes = line.ToByteArray();
                var stream = new MemoryStream(sourceBytes);
                var istream = new IStreamWrapper(stream);
                var ipersistStream = (IPersistStream)new ObjectWithDescriptionClass().Clone();
                ipersistStream.Load(istream);
                var owd = (IObjectWithDescription)ipersistStream;

                var (_, dtoFromBinary) = RuleObjectJsonSerializer.Serialize<IObjectWithDescription, Dto.ObjectWithDescription>(owd);

                // Each item in the array in currentVersionJsonFile is an ObjectWithDescription where the Object is the expected
                // current version of the object being tested
                while (currentVersionJsonReader.Read() && currentVersionJsonReader.TokenType != JsonToken.StartObject) ;
                if (currentVersionJsonReader.TokenType == JsonToken.StartObject)
                {
                    var dtoExpectedResultFromJson = serializer.Deserialize<Dto.ObjectWithDescription>(currentVersionJsonReader);
                    Assert.AreEqual(dtoExpectedResultFromJson, dtoFromBinary);
                    objectsCompared++;
                }
            }

            Assert.AreEqual(lineNum, objectsCompared, "Less than expected JSON objects were found");
        }

        #endregion Public Test Functions

#if WRITING_UNIT_TESTS
        /// <summary>
        /// Load from json and save using COM storage encoded as text lines
        /// </summary>
        [TestCase("Resources.RuleObjects.MicrFinderV2.1.json", TestName = "Make_MicrFinderV2.1.hex")]
        public static void CreateTestData(string resourceName)
        {
            var jsonFile = _testFiles.GetFile(resourceName);
            var serializer = JsonSerializer.CreateDefault(RuleObjectJsonSerializer.Settings);

            using var jsonStream = new FileStream(jsonFile, FileMode.Open);
            using var jsonStreamReader = new StreamReader(jsonStream);
            using var jsonReader = new JsonTextReader(jsonStreamReader);

            var outputFileName = @"C:\Engineering\RC.Net\AttributeFinder\Rules\Json\Test\Resources\RuleObjects\MicrFinderV2.1.hex.txt";
            using var outputStream = new FileStream(outputFileName, FileMode.Create);
            using var outputWriter = new StreamWriter(outputStream, Encoding.ASCII);

            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.StartObject)
                {
                    var dtoInputFromJson = serializer.Deserialize<Dto.ObjectWithDescription>(jsonReader);
                    var domain = (IObjectWithDescription)Domain.RuleObjectConverter.ConvertFromDto(dtoInputFromJson);
                    var bytes = GetRuleObjectBytes(domain);
                    var hex = bytes.ToHexString();
                    outputWriter.WriteLine(hex);
                }
            }
        }

        public static byte[] GetRuleObjectBytes(object ruleObject)
        {
            var stream = new MemoryStream();
            var istream = new IStreamWrapper(stream);
            var ipersistStream = (IPersistStream)((ICopyableObject)ruleObject).Clone();
            ipersistStream.Save(istream, false);
            return stream.ToArray();
        }

#endif
    }
}
