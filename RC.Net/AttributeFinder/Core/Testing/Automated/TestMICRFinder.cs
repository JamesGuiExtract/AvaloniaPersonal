using Extract.AttributeFinder.Rules;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Linq;
using UCLID_AFCORELib;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Test cases for AFUtils methods. This is not meant to be comprehensive at this time but to include
    /// test cases for new features.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("MICRFinder")]
    public class TestMicrFinder
    {
        /// <summary>
        /// A USS file representation of a check tiff.
        /// </summary>
        private static readonly string MICRFinderTest1UssFile = "Resources.MICRFinderTest1.tiff.uss";

        /// <summary>
        /// A Tiff file for a check .
        /// </summary>
        private static readonly string MICRFinderTest1TIFFFile = "Resources.MICRFinderTest1.tiff";

        /// <summary>
        /// Manages the test files used by this test
        /// </summary>
        private static TestFileManager<TestMicrFinder> _testFiles;

        /// <summary>
        /// A static representation of the MICRFinderSplitter.dat file.
        /// </summary>
        private static readonly string MicrSplitterRegex =
@"
\s*
(?'CheckNumber'(U\s?)?([?\d]\s?){3,}U)?
\s*
(?'Routing'(T\s?)?([?\d]\s?(D\s?)?){4,}T)
\s*
(?'Account'([?\d]\s?(D\s?)?){4,}U)
";

        /// <summary>
        /// A static representation of the MICRFinderFilter.dat file.
        /// </summary>
        private static readonly string FilterRegex = @"(\d\s?){3,}";

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestMicrFinder>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            if (_testFiles != null)
            {
                _testFiles.Dispose();
            }
        }

        /// <summary>
        /// Checks to see if the MICRFinder is licensed.
        /// </summary>
        [Test, Category("MICRFinder")]
        public static void ConfirmLicensed()
        {
            var micrFinder = new MicrFinder();
            Assert.IsTrue(micrFinder.IsLicensed(), "MICRFinder is not licensed");
        }

        /// <summary>
        /// Tests the copy constructor of the MICRFinder with all of its properties.
        /// </summary>
        [Test, Category("MICRFinder")]
        public static void MicrFinderCopy()
        {
            var micrFinder = new MicrFinder()
            {
                FilterCharsWhenSplitting = true,
                FilterRegex = string.Empty,
                HighConfidenceThreshold = 20,
                LowConfidenceThreshold = 10,
                MicrSplitterRegex = string.Empty,
                SplitAccountNumber = false,
                SplitAmount = true,
                SplitCheckNumber = true,
                SplitRoutingNumber = false,
                UseLowConfidenceThreshold = false
            };

            var micrFinderCopy = new MicrFinder(micrFinder);
            Assert.AreEqual(micrFinder.FilterCharsWhenSplitting, micrFinderCopy.FilterCharsWhenSplitting);
            Assert.AreEqual(micrFinder.FilterRegex, micrFinderCopy.FilterRegex);
            Assert.AreEqual(micrFinder.HighConfidenceThreshold, micrFinderCopy.HighConfidenceThreshold);
            Assert.AreEqual(micrFinder.LowConfidenceThreshold, micrFinderCopy.LowConfidenceThreshold);
            Assert.AreEqual(micrFinder.MicrSplitterRegex, micrFinderCopy.MicrSplitterRegex);
            Assert.AreEqual(micrFinder.SplitAccountNumber, micrFinderCopy.SplitAccountNumber);
            Assert.AreEqual(micrFinder.SplitAmount, micrFinderCopy.SplitAmount);
            Assert.AreEqual(micrFinder.SplitCheckNumber, micrFinderCopy.SplitCheckNumber);
            Assert.AreEqual(micrFinder.SplitRoutingNumber, micrFinderCopy.SplitRoutingNumber);
            Assert.AreEqual(micrFinder.UseLowConfidenceThreshold, micrFinderCopy.UseLowConfidenceThreshold);
        }

        /// <summary>
        /// Calls the parse text method on a check to see if a MICR exists.
        /// If one does not it will return an empty IUnknownVector.
        /// </summary>
        [Test, Category("MICRFinder")]
        public static void MicrFinderParseText()
        {
            var parsedText = new MicrFinder() { FilterRegex = FilterRegex, MicrSplitterRegex = MicrSplitterRegex }.ParseText(SetupAFDocument(), null);
            Assert.IsTrue(parsedText is UCLID_COMUTILSLib.IUnknownVectorClass);
        }

        /// <summary>
        /// Splits out the account number.
        /// </summary>
        [Test, Category("MICRFinder")]
        public static void MicrFinderSplitAccountNumber()
        {
            var parsedText = new MicrFinder() { FilterRegex = FilterRegex, MicrSplitterRegex = MicrSplitterRegex, SplitAccountNumber = true }.ParseText(SetupAFDocument(), null);
            var subAttributes = parsedText.ToIEnumerable<IAttribute>().First().SubAttributes.ToIEnumerable<IAttribute>().ToList();
            Assert.IsTrue(subAttributes.Count.Equals(1));
            Assert.IsTrue(subAttributes.First().Type.Equals("Account"));
        }

        /// <summary>
        /// Splits out the Routing number.
        /// </summary>
        [Test, Category("MICRFinder")]
        public static void MicrFinderSplitRoutingNumber()
        {
            var parsedText = new MicrFinder() { FilterRegex = FilterRegex, MicrSplitterRegex = MicrSplitterRegex, SplitRoutingNumber = true }.ParseText(SetupAFDocument(), null);
            var subAttributes = parsedText.ToIEnumerable<IAttribute>().First().SubAttributes.ToIEnumerable<IAttribute>().ToList();
            Assert.IsTrue(subAttributes.Count.Equals(1));
            Assert.IsTrue(subAttributes.First().Type.Equals("Routing"));
        }

        /// <summary>
        /// Tries to split out the check number, since this document does not have
        /// one it returns zero.
        /// </summary>
        [Test, Category("MICRFinder")]
        public static void MicrFinderSplitCheckNumber()
        {
            var parsedText = new MicrFinder() { FilterRegex = FilterRegex, MicrSplitterRegex = MicrSplitterRegex, SplitCheckNumber = true }.ParseText(SetupAFDocument(), null);
            var subAttributes = parsedText.ToIEnumerable<IAttribute>().First().SubAttributes.ToIEnumerable<IAttribute>().ToList();
            Assert.IsTrue(subAttributes.Count.Equals(0));
        }

        /// <summary>
        /// Since Amount does not exist in the MicrSplitterRegex(Defined at the top of this class)
        /// The call to MICRFinder.ParseText should fail because it's regular expression does not exist.
        /// </summary>
        [Test, Category("MICRFinder")]
        public static void MicrFinderSplitAmount()
        {
            try
            {
                new MicrFinder() { FilterRegex = FilterRegex, MicrSplitterRegex = MicrSplitterRegex, SplitAmount = true }.ParseText(SetupAFDocument(), null);
                Assert.Fail("Amount should not exist!");
            }
            catch(ExtractException ex)
            {
                var readableExn = ExtractException.FromStringizedByteStream(ex.EliCode, ex.Message);
                var innerMessage = readableExn.InnerException?.Message;
                Assert.AreEqual("Failed to get amount because regex group is missing", innerMessage);
            }
        }

        /// <summary>
        /// Loads a Tiff file along with its uss file to create an AFDocument.
        /// </summary>
        /// <returns>An AFDocument</returns>
        private static AFDocument SetupAFDocument()
        {
            string tiffFilePath = _testFiles.GetFile(MICRFinderTest1TIFFFile);
            string ussFilePath = _testFiles.GetFile(MICRFinderTest1UssFile);
            SpatialString spatialString = new SpatialString();
            spatialString.LoadFrom(ussFilePath, false);
            spatialString.SourceDocName = tiffFilePath;
            return new AFDocument() { Text = spatialString };
        }
    }
}
