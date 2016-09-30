using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="XPathContext"/> class.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("XPath")]
    public class TestXPathQueries
    {
        #region Constants

        /// <summary>
        /// The name of an embedded resource test VOA file.
        /// </summary>
        static readonly string _TEST_DATA_FILE = "Resources.A418.tif.voa";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages the test images needed for testing.
        /// </summary>
        static TestFileManager<TestXPathQueries> _testImages;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testImages = new TestFileManager<TestXPathQueries>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
            // Dispose of the test image manager
            if (_testImages != null)
            {
                _testImages.Dispose();
            }
        }

        #endregion Overhead

        #region Tests

        [Test, Category("XPath")]
        public static void Test01_StringResult()
        {
            var xpathContext = GetXPathContext();

            var result = xpathContext.Evaluate("string(/*/Test[3]/Component[3]/text()[1])");
            var stringValue = result as string;

            Assert.That(stringValue != null);
            Assert.That(stringValue == "HEMOGLOBIN");
        }

        [Test, Category("XPath")]
        public static void Test02_NumericResult()
        {
            var xpathContext = GetXPathContext();

            var result = xpathContext.Evaluate("number(/*/Test[3]/Component[3]/Value)");

            Assert.IsInstanceOf<double>(result);
            Assert.That((double)result == 12.7);
        }

        [Test, Category("XPath")]
        public static void Test03_AttributeResult()
        {
            var xpathContext = GetXPathContext();

            var result = xpathContext.Evaluate("/*/Test[3]/Component[3]/Value");
            var objectList = result as List<object>;

            Assert.That(objectList != null);
            Assert.That(objectList.Count == 1);
            var attribute = objectList.First() as IAttribute;
            Assert.That(attribute != null);
            Assert.That(attribute.Name == "Value");
            Assert.That(attribute.Value.String == "12.7");
            Assert.That(attribute.Value.HasSpatialInfo());
        }

        [Test, Category("XPath")]
        public static void Test04_StringArrayResult()
        {
            var xpathContext = GetXPathContext();

            var result = xpathContext.Evaluate("/*/Test[3]/Component/text()[1]");
            var objectList = result as List<object>;

            Assert.That(objectList != null);
            Assert.That(objectList.Count == 21);
            Assert.That((string)objectList[0] == "WBC");
            Assert.That((string)objectList[1] == "RBC");
            Assert.That((string)objectList[10] == "DIFF TYPE");
            Assert.That((string)objectList[20] == "ABSOLUTE BASO");
        }

        [Test, Category("XPath")]
        public static void Test05_NumericAggregation()
        {
            var xpathContext = GetXPathContext();

            var result = xpathContext.Evaluate("sum(/*/Test[3]/Component[position() < 10]/Value)");

            Assert.IsInstanceOf<double>(result);
            Assert.That((double)result == 485.91);
        }

        [Test, Category("XPath")]
        public static void Test06_TypeCondition()
        {
            var xpathContext = GetXPathContext();

            var result = xpathContext.Evaluate("/*/Test/*[@Type='Date']");
            var objectList = result as List<object>;

            Assert.That(objectList != null);
            var attributes = objectList.OfType<IAttribute>().ToArray();
            Assert.That(attributes.Length == 3);
            Assert.That(attributes[0].Name == "CollectionDate");
            Assert.That(attributes[1].Value.String == "09/09/2009");
            Assert.That(attributes[2].Type == "Date");
        }

        [Test, Category("XPath")]
        public static void Test07_Levenshtein()
        {
            var xpathContext = GetXPathContext();

            var result = xpathContext.Evaluate(
                "es:Levenshtein(string(/*/Test[3]/Component[5]/Value), string(/*/Test[3]/Component[6]/Value))");

            Assert.IsInstanceOf<double>(result);
            Assert.That((double)result == 2);

            // Test evaluating node sets instead of strings
            result = xpathContext.Evaluate(
                "es:Levenshtein(/*/Test[3]/Component[5]/Value, /*/Test[3]/Component[6]/Value)");

            Assert.IsInstanceOf<double>(result);
            Assert.That((double)result == 2);

            // Test evaluating an empty node set
            result = xpathContext.Evaluate(
                "es:Levenshtein(/*/Test[3]/Component[5]/Zebra, /*/Test[3]/Component[6]/Value)");

            CollectionAssert.IsEmpty((IEnumerable<object>)result);
        }

        [Test, Category("XPath")]
        public static void Test08_AttributeRelative()
        {
            var xpathContext = GetXPathContext();

            var result = xpathContext.Evaluate("/*/Test[3]/Component[3]");
            var objectList = result as List<object>;

            Assert.That(objectList != null);
            var attribute = objectList.OfType<IAttribute>().SingleOrDefault();
            Assert.That(attribute != null);
            Assert.That(attribute.Value.String == "HEMOGLOBIN");

            result = xpathContext.Evaluate("string(following-sibling::Component[1]/text()[1])", attribute);

            Assert.IsInstanceOf<string>(result);
            Assert.That((string)result == "HEMATOCRIT");
        }

        [Test, Category("XPath")]
        public static void Test09_EvaluateOver()
        {
            var xpathContext = GetXPathContext();

            var result = xpathContext.EvaluateOver("/*/Test[3]/Component",
                "concat(string(preceding-sibling::Component[1]/text()[1]), ',', string(text()[1]), " +
                    "',', string(following-sibling::Component[1]/text()[1]))");
            var objectEnum = result as IEnumerable<object>;

            Assert.That(objectEnum != null);
            Assert.That(objectEnum.Count() == 21);
            Assert.That((string)objectEnum.First() == ",WBC,RBC");
            Assert.That((string)objectEnum.Skip(1).First() == "WBC,RBC,HEMOGLOBIN");
            Assert.That((string)objectEnum.Skip(10).First() == "MPV,DIFF TYPE,NEUTROPHIL");
            Assert.That((string)objectEnum.Skip(20).First() == "ABSOLUTE EOS,ABSOLUTE BASO,");
        }

        [Test, Category("XPath")]
        public static void Test10_Iterator()
        {
            var xpathContext = GetXPathContext();
            var iterator = xpathContext.GetIterator("/*/Test[3]/Component");

            for (int i = 0; iterator.MoveNext(); i++)
            {
                string comparison = (string)xpathContext.Evaluate(
                    iterator, "concat(string(preceding-sibling::Component[1]/Value), ' -> ', string(Value))");
                double result = (double)xpathContext.Evaluate(
                    iterator, "es:Levenshtein(string(preceding-sibling::Component[1]/Value), string(Value))");

                switch (i)
                {
                    case 0: Assert.That(comparison == " -> 4.85" && result == 4); break;
                    case 1: Assert.That(comparison == "4.85 -> 4.36" && result == 2); break;
                    case 2: Assert.That(comparison == "4.36 -> 12.7" && result == 4); break;
                    case 3: Assert.That(comparison == "12.7 -> 40.5" && result == 3); break;
                    case 5: Assert.That(comparison == "92.9 -> 29.1" && result == 2); break; // transpose
                    case 20: Assert.That(comparison == "0.09 -> 0.01" && result == 1); break;
                }
            }
        }

        [Test, Category("XPath")]
        public static void Test11_MixedResultTypes()
        {
            var xpathContext = GetXPathContext();
            var result = xpathContext.Evaluate("/*/PatientInfo|/*/PatientInfo/Name/*/text()|/*/Test");

            var objectList = result as List<object>;
            Assert.That(objectList != null);
            Assert.That(objectList.Count == 6);
            Assert.IsInstanceOf<IAttribute>(objectList[0]);
            Assert.IsInstanceOf<string>(objectList[1]);
            Assert.IsInstanceOf<string>(objectList[2]);
            Assert.IsInstanceOf<IAttribute>(objectList[3]);
            Assert.IsInstanceOf<IAttribute>(objectList[4]);
            Assert.IsInstanceOf<IAttribute>(objectList[5]);
        }

        [Test, Category("XPath")]
        public static void Test12_MoreLevenshtein()
        {
            var xpathContext = GetXPathContext();

            // Test evaluating node sets instead of strings
            var result = xpathContext.Evaluate(
                "es:Levenshtein(/*/Test[3]/Component[5]/Value, /*/Test[3]/Component[6]/Value)");

            Assert.IsInstanceOf<double>(result);
            Assert.That((double)result == 2);

            // Test evaluating an empty node set
            result = xpathContext.Evaluate(
                "es:Levenshtein(/*/Test[3]/Component[5]/Zebra, /*/Test[3]/Component[6]/Value)");

            CollectionAssert.IsEmpty((IEnumerable<object>)result);
        }

        [Test, Category("XPath")]
        public static void Test13_FilterMixedResultTypes()
        {
            var xpathContext = GetXPathContext();
            IEnumerable<IAttribute> attrResult =
                xpathContext.FindAllOfType<IAttribute>("/*/PatientInfo|/*/PatientInfo/Name/*/text()|/*/Test");
            Assert.AreEqual(4, attrResult.Count());

            IEnumerable<string> stringResult =
                xpathContext.FindAllOfType<string>("/*/PatientInfo|/*/PatientInfo/Name/*/text()|/*/Test");
            Assert.AreEqual(2, stringResult.Count());

            IEnumerable<double> numericResult = xpathContext.FindAllOfType<double>("number(/*/Test[3]/Component[3]/Value)");
            Assert.AreEqual(1, numericResult.Count());
            Assert.AreEqual(12.7, numericResult.First());

            double? numberResult = xpathContext.FindAllOfType<double?>("number(/*/Test[3]/Component[3]/Value)").FirstOrDefault();
            Assert.AreEqual(12.7, numberResult);

            numberResult = xpathContext.FindAllOfType<double?>("/*/Test[3]/Component[3]/Value").FirstOrDefault();
            Assert.AreEqual(null, numberResult);
        }

        [Test, Category("XPath")]
        public static void Test14_ForceMixedResultTypesToString()
        {
            var xpathContext = GetXPathContext();
            IEnumerable<string> stringResult =
                xpathContext.FindAllAsStrings("/*/PatientInfo|/*/PatientInfo/Name/*/text()|/*/Test");
            Assert.AreEqual(6, stringResult.Count());
            Assert.AreEqual("N/A", stringResult.ElementAt(0));
            Assert.AreEqual("DOE", stringResult.ElementAt(1));

            stringResult = xpathContext.FindAllAsStrings("number(/*/Test[3]/Component[3]/Value)");
            Assert.AreEqual(1, stringResult.Count());
            Assert.AreEqual("12.7", stringResult.ElementAt(0));
        }

        #endregion Tests

        #region Helper Functions

        static XPathContext GetXPathContext()
        {
            var attributes = new IUnknownVector();
            attributes.LoadFrom(_testImages.GetFile(_TEST_DATA_FILE), false);
            return new XPathContext(attributes);
        }

        #endregion Helper Functions
    }
}