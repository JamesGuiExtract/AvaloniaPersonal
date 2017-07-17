﻿using Extract.Testing.Utilities;
using Extract.Utilities;
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
        const string _TEST_DATA_FILE = "Resources.A418.tif.voa";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages the test images needed for testing.
        /// </summary>
        static TestFileManager<TestXPathQueries> _testImages;

        static IUnknownVector _attributes;

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

        [Test, Category("XPath")]
        public static void Test15_RemovingAttributeTree()
        {
            var xpathContext = GetXPathContext();
            var result = xpathContext.FindAllOfType<IAttribute>("/*/LabInfo");
            var enumeration = result.Single().EnumerateDepthFirst();
            Assert.AreEqual(7, enumeration.Count());

            xpathContext.RemoveMatchingAttributes("/*/LabInfo/Address");
            enumeration = result.Single().EnumerateDepthFirst();
            Assert.AreEqual(2, enumeration.Count());

            result = xpathContext.FindAllOfType<IAttribute>("/*/LabInfo/Address");
            CollectionAssert.IsEmpty(result);

            result = xpathContext.FindAllOfType<IAttribute>("/*/LabInfo/Address/Address1");
            CollectionAssert.IsEmpty(result);
        }

        [Test, Category("XPath")]
        public static void Test16_RemovingMultipleAttributes()
        {
            var xpathContext = GetXPathContext();

            var enumeration = _attributes.ToIEnumerable<IAttribute>()
                .SelectMany(AttributeMethods.EnumerateDepthFirst);
            Assert.AreEqual(177, enumeration.Count());

            var result = xpathContext.FindAllOfType<IAttribute>("/*/Test");
            Assert.AreEqual(3, result.Count());

            // Including all subattributes the Test attributes total 157
            enumeration = result.SelectMany(AttributeMethods.EnumerateDepthFirst);
            Assert.AreEqual(157, enumeration.Count());

            // Remove most of the contents
            xpathContext.RemoveMatchingAttributes("/*/Test/Component");

            // Check that there are still 3 Test attribute
            result = xpathContext.FindAllOfType<IAttribute>("/*/Test");
            Assert.AreEqual(3, result.Count());

            // Now only 15 attributes in the recursive enumeration instead of 157
            enumeration = result.SelectMany(AttributeMethods.EnumerateDepthFirst);
            Assert.AreEqual(15, enumeration.Count());

            // No Components left
            result = xpathContext.FindAllOfType<IAttribute>("/*/Test/Component");
            CollectionAssert.IsEmpty(result);

            // Ensure original hierarchy has been suitably modified
            enumeration = _attributes.ToIEnumerable<IAttribute>()
                .SelectMany(AttributeMethods.EnumerateDepthFirst);
            Assert.AreEqual(35, enumeration.Count());
        }

        [Test, Category("XPath")]
        public static void Test17_RemovingAllAttributes()
        {
            var xpathContext = GetXPathContext();

            // Make a shallow copy of _attributes
            var topLevelAttributes = _attributes.ToIEnumerable<IAttribute>().ToList();
            Assert.AreEqual(6, topLevelAttributes.Count());

            var enumeration = topLevelAttributes.SelectMany(AttributeMethods.EnumerateDepthFirst);
            Assert.AreEqual(177, enumeration.Count());

            xpathContext.RemoveMatchingAttributes("//*");

            var result = xpathContext.FindAllOfType<IAttribute>("//*");
            CollectionAssert.IsEmpty(result);

            Assert.AreEqual(0, _attributes.Size());

            // Top-level attributes saved in copy will have had all subattributes removed
            Assert.AreEqual(6, topLevelAttributes.Count());
            enumeration = topLevelAttributes.SelectMany(AttributeMethods.EnumerateDepthFirst);
            CollectionAssert.AreEqual(topLevelAttributes, enumeration);

            // After removing all attributes the context will only contain the root node,
            // returned as an empty string from Evaluate
            var resultObject = xpathContext.Evaluate("//*");
            Assert.IsEmpty((string)((IEnumerable<object>)resultObject).First());
        }

        // Empty attributes should not have a text node
        // https://extract.atlassian.net/browse/ISSUE-14210
        [Test, Category("XPath")]
        public static void Test18_EmptyAttribute()
        {
            var xpathContext = GetXPathContext(path: "Resources.EmptyAttribute.voa");

            var result = xpathContext.FindAllOfType<IAttribute>("//*[not(text())]");
            Assert.AreEqual(1, result.Count());
        }

        [Test, Category("XPath")]
        public static void Test19_Bitmap()
        {
            var imagePath = _testImages.GetFile("Resources.A418.tif");
            var voaPath = _testImages.GetFile("Resources.A418.tif.voa");
            var voa = new IUnknownVector();
            voa.LoadFrom(voaPath, false);
            voa.UpdateSourceDocNameOfAttributes(imagePath);
            var xpathContext = new XPathContext(voa);

            var result = xpathContext.Evaluate(
                "es:Bitmap(25, 25, /*/Test/Component/Flag)");

            Assert.IsInstanceOf<string>(result);
            Assert.AreEqual("Bitmap: 25 x 25 = " +
                "220,33,26,223,219,52,198,39,18,18,20,54,240,255,247,35,18,18,18,18,37,198,52,217,234," +
                "28,28,1,18,18,17,16,16,1,0,0,43,245,255,245,26,0,0,0,0,2,16,16,11,53," +
                "53,18,0,0,0,0,0,0,34,37,37,52,244,255,241,54,29,30,4,0,0,0,0,30,49," +
                "229,60,45,46,54,4,3,42,59,239,254,254,255,255,255,236,79,221,75,2,42,45,45,59,245," +
                "255,255,255,255,254,19,4,63,244,255,255,255,255,255,255,255,255,255,255,12,57,244,255,255,255," +
                "255,255,255,255,255,19,4,70,244,255,255,255,255,255,255,255,255,255,186,13,65,244,255,255,255," +
                "255,255,255,255,253,18,5,78,246,255,255,255,255,255,255,255,255,253,10,4,78,246,255,255,255," +
                "255,255,255,255,254,18,5,86,249,255,255,255,255,255,255,255,255,254,18,5,86,249,255,255,255," +
                "255,255,255,255,255,19,6,94,251,255,255,255,255,255,255,255,255,255,19,6,94,251,255,255,255," +
                "255,255,255,255,255,19,8,121,255,255,255,255,255,255,255,255,255,255,19,7,102,253,255,255,255," +
                "255,255,255,255,255,19,0,11,153,148,146,146,146,146,146,146,146,137,10,7,111,254,255,255,255," +
                "255,255,255,255,255,27,0,0,0,0,0,0,0,0,0,0,0,0,0,8,119,255,255,255,255," +
                "255,255,255,255,255,27,0,0,0,0,0,0,0,0,0,0,0,0,0,8,126,255,255,255,255," +
                "255,255,255,255,255,27,9,127,136,136,136,136,136,136,136,136,138,145,11,9,135,255,255,255,255," +
                "255,255,255,255,255,18,10,145,255,255,255,255,255,255,255,255,255,255,21,10,143,255,255,255,255," +
                "255,255,255,255,255,18,10,152,255,255,255,255,255,255,255,255,255,254,19,10,152,255,255,255,255," +
                "255,255,255,255,255,18,18,160,254,255,255,255,255,255,255,255,255,252,19,11,160,254,255,255,255," +
                "255,255,255,255,255,18,18,168,253,255,255,255,255,255,255,255,255,250,19,12,168,253,255,255,255," +
                "255,255,255,255,250,36,30,176,252,255,255,255,255,255,255,255,255,247,19,12,176,252,255,255,255," +
                "255,255,255,255,247,36,31,186,254,255,255,255,255,255,255,255,255,244,19,13,186,254,255,255,255," +
                "255,255,255,255,245,36,32,194,254,255,255,255,255,255,255,255,255,241,18,14,194,254,255,255,245," +
                "68,53,53,53,50,21,17,22,54,46,59,240,255,255,245,60,45,42,3,0,4,54,64,225,217," +
                "7,0,0,0,0,0,17,18,18,0,0,193,251,255,237,2,0,0,0,0,0,0,0,0,42," +
                "16,12,12,12,12,12,28,29,29,18,33,218,255,255,245,36,18,18,18,18,18,18,18,30,190," +
                "212,213,213,213,213,213,213,210,233,232,232,234,255,255,254,234,232,232,232,232,232,232,232,232,220"
                , result);
        }

        #endregion Tests

        #region Helper Functions

        static XPathContext GetXPathContext(string path=_TEST_DATA_FILE)
        {
            _attributes = new IUnknownVector();
            _attributes.LoadFrom(_testImages.GetFile(path), false);
            return new XPathContext(_attributes);
        }

        #endregion Helper Functions
    }
}