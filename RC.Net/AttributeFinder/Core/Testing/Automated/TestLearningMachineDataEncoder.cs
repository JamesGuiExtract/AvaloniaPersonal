using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Unit tests for learning machine data encoder class
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("LearningMachineDataEncoder")]
    public class TestLearningMachineDataEncoder
    {
        #region Constants

        /// <summary>
        /// The name of an embedded resource folder
        /// </summary>

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages the test images needed for testing.
        /// </summary>
        static TestFileManager<TestLearningMachineDataEncoder> _testFiles;
        static string[] _ussFiles;
        static string[] _voaFiles;
        static string[] _voaFiles2;
        static string[] _voaFiles3;
        static string[] _eavFiles;
        static string[] _categories;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestLearningMachineDataEncoder>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
            // Dispose of the test image manager
            if (_testFiles != null)
            {
                _testFiles.Dispose();
            }
        }

        #endregion Overhead

        #region Tests

        // Helper function to build file lists for pagination testing
        // These images are stapled together from Demo_LabDE images
        private static void SetPaginationFiles()
        {
            _ussFiles = new string[7];
            _voaFiles = new string[7];
            _eavFiles = new string[7];
            for (int i = 0; i < _ussFiles.Length; i++)
            {
                var baseName = "Resources.LearningMachine.Pagination.Pagination_{0:D3}.tif{1}";
                _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ""));
                _ussFiles[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".uss"));
                _voaFiles[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".protofeatures.voa"));
                _eavFiles[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".eav"));
            }
        }

        // Test where there is no attribute filter and no auto-bag-of-words feature
        [Test, Category("LearningMachineDataEncoder")]
        public static void PaginationWithNoAttributeFilter()
        {
            int expectedFeatureVectorLength = 57;
            SetPaginationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination);
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _eavFiles);
            Assert.AreEqual(expectedFeatureVectorLength, encoder.FeatureVectorLength);

            Assert.AreEqual(21, encoder.AttributeFeatureVectorizers.Count());
            Assert.AreEqual(18, encoder.AttributeFeatureVectorizers
                .Where(fv => fv.FeatureType == FeatureVectorizerType.Numeric).Count());
            Assert.AreEqual(3, encoder.AttributeFeatureVectorizers
                .Where(fv => fv.FeatureType == FeatureVectorizerType.DiscreteTerms).Count());

            var t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles, _eavFiles);
            var features = t.Item1;
            var answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);
        }

        // Test where there is an attribute filter and no auto-bag-of-words feature
        [Test, Category("LearningMachineDataEncoder")]
        public static void PaginationWithAttributeFilter()
        {
            int expectedFeatureVectorLength = 32;
            SetPaginationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, attributeFilter: "*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _eavFiles);
            Assert.AreEqual(expectedFeatureVectorLength, encoder.FeatureVectorLength);

            Assert.AreEqual(16, encoder.AttributeFeatureVectorizers.Count());
            Assert.That(encoder.AttributeFeatureVectorizers.All(fv => fv.FeatureType == FeatureVectorizerType.Numeric));

            var t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles, _eavFiles);
            var features = t.Item1;
            var answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);
        }

        // Test using a filter that matches the same attribute by type and by name
        // https://extract.atlassian.net/browse/ISSUE-14761
        [Test, Category("LearningMachineDataEncoder")]
        public static void PaginationWithMultipleMatchFilter()
        {
            int expectedFeatureVectorLength = 32;
            SetPaginationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, attributeFilter: "DOBDiff02|*@Feature|*@EnsureAFQuery{*}");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _eavFiles);
            Assert.AreEqual(expectedFeatureVectorLength, encoder.FeatureVectorLength);

            Assert.AreEqual(16, encoder.AttributeFeatureVectorizers.Count());
            Assert.That(encoder.AttributeFeatureVectorizers.All(fv => fv.FeatureType == FeatureVectorizerType.Numeric));
            Assert.That(encoder.AttributeFeatureVectorizers.Any(fv => fv.Name == "DOBDiff02"));
        }

        // Test where there is a negated attribute filter used to get the same results as a positive filter
        [Test, Category("LearningMachineDataEncoder")]
        public static void PaginationWithNegatedAttributeFilter()
        {
            int expectedFeatureVectorLength = 32;
            SetPaginationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination,
                attributeFilter:"*@", negateFilter:true);
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _eavFiles);
            Assert.AreEqual(expectedFeatureVectorLength, encoder.FeatureVectorLength);

            Assert.AreEqual(16, encoder.AttributeFeatureVectorizers.Count());
            Assert.That(encoder.AttributeFeatureVectorizers.All(fv => fv.FeatureType == FeatureVectorizerType.Numeric));

            var t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles, _eavFiles);
            var features = t.Item1;
            var answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);
        }

        // Use auto-bag-of-words feature along with attribute features
        [Test, Category("LearningMachineDataEncoder")]
        public static void PaginationWithAutoBow()
        {
            int expectedFeatureVectorLength = 2032;
            SetPaginationFiles();
            var autoBoW = new SpatialStringFeatureVectorizer("", 5, 2000);
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, autoBoW, attributeFilter:"*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _eavFiles);
            Assert.AreEqual(expectedFeatureVectorLength, encoder.FeatureVectorLength);

            Assert.AreEqual(2000, encoder.AutoBagOfWords.FeatureVectorLength);

            Assert.AreEqual(16, encoder.AttributeFeatureVectorizers.Count());

            Assert.That(encoder.AttributeFeatureVectorizers.All(fv => fv.FeatureType == FeatureVectorizerType.Numeric));

            var t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles, _eavFiles);
            var features = t.Item1;
            var answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);

            var uss = new UCLID_RASTERANDOCRMGMTLib.SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);
            var voa = new UCLID_COMUTILSLib.IUnknownVectorClass();
            voa.LoadFrom(_voaFiles[0], false);

            int pages = uss.GetPages(true, " ").Size();
            Assert.AreEqual(4, pages);
            var featureVectorCollection = encoder.GetFeatureVectors(uss, voa).ToArray();
            Assert.AreEqual(pages-1, featureVectorCollection.Length);
            Assert.AreEqual(expectedFeatureVectorLength, featureVectorCollection[0].Length);

            uss.LoadFrom(_ussFiles[2], false);
            voa.LoadFrom(_voaFiles[2], false);
            pages = uss.GetPages(true, " ").Size();
            Assert.AreEqual(1, pages);
            featureVectorCollection = encoder.GetFeatureVectors(uss, voa).ToArray();
            Assert.AreEqual(pages-1, featureVectorCollection.Length);
        }

        // Use auto-bag-of-words feature without attribute features
        [Test, Category("LearningMachineDataEncoder")]
        public static void PaginationWithAutoBowNoAttributeFiles()
        {
            int expectedFeatureVectorLength = 2000;
            SetPaginationFiles();

            var autoBoW = new SpatialStringFeatureVectorizer("", 5, 2000);
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, autoBoW, attributeFilter:"*@Feature");
            encoder.ComputeEncodings(_ussFiles, null, _eavFiles);
            Assert.AreEqual(expectedFeatureVectorLength, encoder.FeatureVectorLength);

            Assert.AreEqual(2000, encoder.AutoBagOfWords.FeatureVectorLength);

            Assert.AreEqual(0, encoder.AttributeFeatureVectorizers.Count());

            Assert.That(encoder.AttributeFeatureVectorizers.All(fv => fv.FeatureType == FeatureVectorizerType.Numeric));

            var t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, null, _eavFiles);
            var features = t.Item1;
            var answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);
            Assert.AreEqual(expectedFeatureVectorLength, features[0].Length);

            var uss = new UCLID_RASTERANDOCRMGMTLib.SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);
            var voa = new UCLID_COMUTILSLib.IUnknownVectorClass();
            voa.LoadFrom(_voaFiles[0], false);

            int pages = uss.GetPages(true, " ").Size();
            Assert.AreEqual(4, pages);
            var featureVectorCollection = encoder.GetFeatureVectors(uss, voa).ToArray();
            Assert.AreEqual(pages-1, featureVectorCollection.Length);
            Assert.AreEqual(expectedFeatureVectorLength, featureVectorCollection[0].Length);

            uss.LoadFrom(_ussFiles[2], false);
            voa.LoadFrom(_voaFiles[2], false);
            pages = uss.GetPages(true, " ").Size();
            Assert.AreEqual(1, pages);
            featureVectorCollection = encoder.GetFeatureVectors(uss, voa).ToArray();
            Assert.AreEqual(pages-1, featureVectorCollection.Length);
        }

        // Test exception handling
        // One input VOA is non-existent, one is empty string
        [Test, Category("LearningMachineDataEncoder")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "VOA")]
        public static void PaginationWithAutoBowAndBadVOA()
        {
            SetDocumentCategorizationFiles();
            var autoBoW = new SpatialStringFeatureVectorizer("", 5, 2000);
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, autoBoW, attributeFilter:"*@Feature");

            _voaFiles[1] = "";
            var ex = Assert.Throws<ExtractException>(() => encoder.ComputeEncodings(_ussFiles, _voaFiles, _categories));
            Assert.That( ex.Message, Is.EqualTo("One or more errors occurred.") );
            Assert.That(!encoder.AreEncodingsComputed);

            _voaFiles[1] = "blah.voa";
            ex = Assert.Throws<ExtractException>(() => encoder.ComputeEncodings(_ussFiles, _voaFiles, _categories));
            Assert.That( ex.Message, Is.EqualTo("One or more errors occurred.") );
            Assert.That(!encoder.AreEncodingsComputed);
        }

        // Test exception handling: neither auto-bag-of-words nor attribute features is invalid
        [Test, Category("LearningMachineDataEncoder")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "VOA")]
        public static void PaginationWithNoVOANoBow()
        {
            SetDocumentCategorizationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination);
            var ex = Assert.Throws<ExtractException>(() => encoder.ComputeEncodings(_ussFiles, null, _categories));
            Assert.That( ex.Message, Is.EqualTo("Unable to successfully compute encodings") );

            // Can't use if not configured successfully
            ex = Assert.Throws<ExtractException>(() => encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, null, _categories));
            Assert.That( ex.Message, Is.EqualTo("Encodings have not been computed") );

            // Can't use if not configured successfully
            var uss = new UCLID_RASTERANDOCRMGMTLib.SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);
            var voa = new UCLID_COMUTILSLib.IUnknownVectorClass();
            voa.LoadFrom(_voaFiles[0], false);
            ex = Assert.Throws<ExtractException>(() => encoder.GetFeatureVectors(uss, voa));
            Assert.That( ex.Message, Is.EqualTo("Encodings have not been computed") );
        }

        // Helper function to create file lists for document categorization testing
        // These files are from Demo_FlexIndex
        private static void SetDocumentCategorizationFiles()
        {
            _ussFiles = new string[10];
            _voaFiles = new string[10];
            _voaFiles2 = new string[10];
            for (int i = 0; i < _ussFiles.Length; i++)
            {
                var baseName = "Resources.LearningMachine.DocumentCategorization.Example{0:D2}.tif{1}";
                _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ""));
                _ussFiles[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".uss"));
                _voaFiles[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".voa"));
                _voaFiles2[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".protofeatures.voa"));
            }
            _categories = new string[]
            {
                "Deed of Trust",
                "Mortgage",
                "Satisfaction of Mortgage",
                "Reconveyance",
                "Grant Deed",
                "Warranty Deed",
                "Quit Claim Deed",
                "Assignment of Deed of Trust",
                "Assignment of Mortgage",
                "Notice of Federal Tax Lien"
            };
        }

        // Test using DocumentType attributes for features
        [Test, Category("LearningMachineDataEncoder")]
        public static void DocumentCategorization()
        {
            int expectedFeatureVectorLength = 11;
            SetDocumentCategorizationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(
                LearningMachineUsage.DocumentCategorization, attributeFilter:"DocumentType");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _categories);
            Assert.AreEqual(expectedFeatureVectorLength, encoder.FeatureVectorLength);

            Assert.AreEqual(1, encoder.AttributeFeatureVectorizers.Count());
            Assert.That(encoder.AttributeFeatureVectorizers.All(fv => fv.FeatureType == FeatureVectorizerType.DiscreteTerms));

            var t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles, _categories);
            var features = t.Item1;
            var answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);

            var uss = new UCLID_RASTERANDOCRMGMTLib.SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);
            var voa = new UCLID_COMUTILSLib.IUnknownVectorClass();
            voa.LoadFrom(_voaFiles[0], false);

            var featureVector = encoder.GetFeatureVectors(uss, voa).First();
            Assert.AreEqual(expectedFeatureVectorLength, featureVector.Length);
        }

        // Test using auto-bag-of-words and DocumentType attributes for features
        [Test, Category("LearningMachineDataEncoder")]
        public static void DocumentCategorizationWithAutoBow()
        {
            int expectedFeatureVectorLength = 2011;
            SetDocumentCategorizationFiles();
            var autoBoW = new SpatialStringFeatureVectorizer("", 5, 2000);
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW,
                attributeFilter:"DocumentType");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _categories);
            Assert.AreEqual(expectedFeatureVectorLength, encoder.FeatureVectorLength);

            Assert.AreEqual(1, encoder.AttributeFeatureVectorizers.Count());
            Assert.That(encoder.AttributeFeatureVectorizers.All(fv => fv.FeatureType == FeatureVectorizerType.DiscreteTerms));

            var t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles, _categories);
            var features = t.Item1;
            var answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);

            var uss = new UCLID_RASTERANDOCRMGMTLib.SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);
            var voa = new UCLID_COMUTILSLib.IUnknownVectorClass();
            voa.LoadFrom(_voaFiles[0], false);

            var featureVector = encoder.GetFeatureVectors(uss, voa).First();
            Assert.AreEqual(expectedFeatureVectorLength, featureVector.Length);
        }

        // Test using auto-bag-of-words and DocumentType attributes for features
        [Test, Category("LearningMachineDataEncoder")]
        public static void DocumentCategorizationWithFeatureHashing()
        {
            int expectedFeatureVectorLength = 8000;
            SetDocumentCategorizationFiles();
            var autoBoW = new SpatialStringFeatureVectorizer("", 2, 8000) { UseFeatureHashing = true };
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW);
            encoder.ComputeEncodings(_ussFiles, null, _categories);

            Assert.That(encoder.AreEncodingsComputed);
            Assert.That(encoder.AnswerCodeToName.Any());
            Assert.AreEqual(expectedFeatureVectorLength, encoder.FeatureVectorLength);

            var t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, null, _categories);
            var features = t.Item1;
            var answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);

            var uss = new UCLID_RASTERANDOCRMGMTLib.SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);

            var featureVector = encoder.GetFeatureVectors(uss, null).First();
            Assert.AreEqual(expectedFeatureVectorLength, featureVector.Length);
        }

        // Test using only auto-bag-of-words feature
        [Test, Category("LearningMachineDataEncoder")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "VOA")]
        public static void DocumentCategorizationWithNoVOA()
        {
            int expectedFeatureVectorLength = 2000;
            SetDocumentCategorizationFiles();
            var autoBoW = new SpatialStringFeatureVectorizer("", 5, 2000);
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW);
            encoder.ComputeEncodings(_ussFiles, null, _categories);
            Assert.AreEqual(expectedFeatureVectorLength, encoder.FeatureVectorLength);

            Assert.AreEqual(0, encoder.AttributeFeatureVectorizers.Count());
            Assert.That(encoder.AttributeFeatureVectorizers.All(fv => fv.FeatureType == FeatureVectorizerType.DiscreteTerms));

            // Try passing VOA files that will be ignored
            var t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles, _categories);
            var features = t.Item1;
            var answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);

            // Try passing null for VOA files
            t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, null, _categories);
            features = t.Item1;
            answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);

            // Try passing incorrect VOA files (also ignored)
            t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _ussFiles, _categories);
            features = t.Item1;
            answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);

            var uss = new UCLID_RASTERANDOCRMGMTLib.SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);
            var voa = new UCLID_COMUTILSLib.IUnknownVectorClass();
            voa.LoadFrom(_voaFiles[0], false);

            var featureVector = encoder.GetFeatureVectors(uss, voa).First();
            Assert.AreEqual(expectedFeatureVectorLength, featureVector.Length);
        }

        // Test using different page filters
        [Test, Category("LearningMachineDataEncoder")]
        public static void DocumentCategorizationBowPageFilters()
        {
            SetDocumentCategorizationFiles();
            // Use first pages only
            var autoBoW = new SpatialStringFeatureVectorizer("1", 5, 2000);
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW);
            encoder.ComputeEncodings(_ussFiles, null, _categories);
            var firstPageValues = autoBoW.RecognizedValues;
            Assert.AreEqual(2000, firstPageValues.Count());

            // Compare to all pages
            autoBoW.PagesToProcess = "";
            encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW);
            encoder.ComputeEncodings(_ussFiles, null, _categories);
            var allPageValues = autoBoW.RecognizedValues;
            Assert.AreEqual(2000, allPageValues.Count());
            CollectionAssert.AreNotEquivalent(firstPageValues, allPageValues);

            // Using page number that is not in any image works, but results in no features
            autoBoW.PagesToProcess = "100";
            encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW);
            encoder.ComputeEncodings(_ussFiles, null, _categories);
            var pageOneHundredValues = autoBoW.RecognizedValues;
            Assert.AreEqual(0, pageOneHundredValues.Count());
            Assert.AreEqual(0, encoder.FeatureVectorLength);
        }

        // Test exception handling: neither auto-bag-of-words nor attribute features is invalid
        [Test, Category("LearningMachineDataEncoder")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "VOA")]
        public static void DocumentCategorizationWithNoVOANoBow()
        {
            SetDocumentCategorizationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization);
            var ex = Assert.Throws<ExtractException>(() => encoder.ComputeEncodings(_ussFiles, null, _categories));
            Assert.That( ex.Message, Is.EqualTo("Unable to successfully compute encodings") );

            // Can't use if not configured successfully
            ex = Assert.Throws<ExtractException>(() => encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, null, _categories));
            Assert.That( ex.Message, Is.EqualTo("Encodings have not been computed") );

            // Can't use if not configured successfully
            var uss = new UCLID_RASTERANDOCRMGMTLib.SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);
            var voa = new UCLID_COMUTILSLib.IUnknownVectorClass();
            voa.LoadFrom(_voaFiles[0], false);
            ex = Assert.Throws<ExtractException>(() => encoder.GetFeatureVectors(uss, voa));
            Assert.That( ex.Message, Is.EqualTo("Encodings have not been computed") );
        }

        // Use auto-bag-of-words with two pages per case
        [Test, Category("LearningMachineDataEncoder")]
        public static void PaginationWithAutoBowTwoPages()
        {
            int expectedFeatureVectorLength = 4032;
            SetPaginationFiles();
            var autoBoW = new SpatialStringFeatureVectorizer("2", 5, 2000);
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, autoBoW, attributeFilter:"*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _eavFiles);

            // This number reflects the fact that there will be two pages of features for each case
            Assert.AreEqual(expectedFeatureVectorLength, encoder.FeatureVectorLength);

            // This is the per-page number, not the actual feature vector length
            Assert.AreEqual(2000, encoder.AutoBagOfWords.FeatureVectorLength);

            // This is the true number of BoW features
            Assert.AreEqual(4000, encoder.AutoBagOfWords.FeatureVectorLengthForPagination);

            var t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles, _eavFiles);
            var features = t.Item1;
            var answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);

            var uss = new SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);
            var voa = new IUnknownVectorClass();
            voa.LoadFrom(_voaFiles[0], false);

            int pages = uss.GetPages(true, " ").Size();
            Assert.AreEqual(4, pages);
            var featureVectorCollection = encoder.GetFeatureVectors(uss, voa).ToArray();
            Assert.AreEqual(pages-1, featureVectorCollection.Length);

            var case1Features = featureVectorCollection[0];
            Assert.AreEqual(expectedFeatureVectorLength, case1Features.Length);

            // First 2k features of the second case are the same as second 2k features of first case,
            // since they are from the same page
            var case2Features = featureVectorCollection[1];
            CollectionAssert.AreEqual(
                case1Features.Skip(2000).Take(2000).ToArray(),
                case2Features.Take(2000).ToArray());
        }

        // Use auto-bag-of-words with three pages per case
        [Test, Category("LearningMachineDataEncoder")]
        public static void PaginationWithAutoBowThreePages()
        {
            // With three pages per case there is the possibility for a page to not be available (last case in a document)
            // so the feature vector is 6033 instead of 6032 (32 are attribute features)
            int expectedFeatureVectorLength = 6033;
            SetPaginationFiles();
            var autoBoW = new SpatialStringFeatureVectorizer("3", 5, 2000);
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, autoBoW, attributeFilter:"*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _eavFiles);

            // This number reflects the fact that there will be three pages of features for each case
            Assert.AreEqual(expectedFeatureVectorLength, encoder.FeatureVectorLength);

            // This is the per-page number, not the actual feature vector length
            Assert.AreEqual(2000, encoder.AutoBagOfWords.FeatureVectorLength);

            // This is the true number of BoW features
            Assert.AreEqual(6001, encoder.AutoBagOfWords.FeatureVectorLengthForPagination);

            var t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles, _eavFiles);
            var features = t.Item1;
            var answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);

            var uss = new SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);
            var voa = new IUnknownVectorClass();
            voa.LoadFrom(_voaFiles[0], false);

            int pages = uss.GetPages(true, " ").Size();
            Assert.AreEqual(4, pages);
            var featureVectorCollection = encoder.GetFeatureVectors(uss, voa).ToArray();
            Assert.AreEqual(pages-1, featureVectorCollection.Length);

            var case1Features = featureVectorCollection[0];
            Assert.AreEqual(expectedFeatureVectorLength, case1Features.Length);

            // First 2k features of the second case are the same as second 2k features of first case,
            // since they are from the same page
            var case2Features = featureVectorCollection[1];
            CollectionAssert.AreEqual(
                case1Features.Skip(2000).Take(2000).ToArray(),
                case2Features.Take(2000).ToArray());

            // The features from the third page may be all zeros, if the page doesn't exist,
            // so there is an extra, present/missing flag, feature for that page with value of 1 when
            // the page is present and zero if missing
            Assert.AreEqual(1, case1Features[4000]);
            var case3Features = featureVectorCollection[2];
            Assert.AreEqual(0, case3Features[4000]);
            Assert.That(case3Features.Skip(4001).Take(2000).All(f => f == 0));

            // First 2k features of the third case are the same as third 2k features (shifted one place
            // for the present/missing flag) of first case, since they are from the same page
            CollectionAssert.AreEqual(
                case1Features.Skip(4001).Take(2000).ToArray(),
                case3Features.Take(2000).ToArray());
        }

        // Use auto-bag-of-words with four pages per case
        [Test, Category("LearningMachineDataEncoder")]
        public static void PaginationWithAutoBowFourPages()
        {
            // With four pages per case there is the possibility for two pages to not be available
            // (first case in a document could be missing two and last one) so the feature vector
            // is 8034 instead of 8032 (32 are attribute features)
            int expectedFeatureVectorLength = 8034;
            SetPaginationFiles();
            var autoBoW = new SpatialStringFeatureVectorizer("4", 5, 2000);
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, autoBoW, attributeFilter:"*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _eavFiles);

            // This number reflects the fact that there will be three pages of features for each case
            Assert.AreEqual(expectedFeatureVectorLength, encoder.FeatureVectorLength);

            // This is the per-page number, not the actual feature vector length
            Assert.AreEqual(2000, encoder.AutoBagOfWords.FeatureVectorLength);

            // This is the true number of BoW features
            Assert.AreEqual(8002, encoder.AutoBagOfWords.FeatureVectorLengthForPagination);

            var t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles, _eavFiles);
            var features = t.Item1;
            var answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);

            var uss = new SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);
            var voa = new IUnknownVectorClass();
            voa.LoadFrom(_voaFiles[0], false);

            int pages = uss.GetPages(true, " ").Size();
            Assert.AreEqual(4, pages);
            var featureVectorCollection = encoder.GetFeatureVectors(uss, voa).ToArray();
            Assert.AreEqual(pages-1, featureVectorCollection.Length);

            var case1Features = featureVectorCollection[0];
            Assert.AreEqual(expectedFeatureVectorLength, case1Features.Length);

            // First 2k features, shifted one place, of the third case are the same as the
            // third 2k features, shifted one place, of the first case, since they are from the same page
            var case3Features = featureVectorCollection[2];
            CollectionAssert.AreEqual(
                case1Features.Skip(4001).Take(2000).ToArray(),
                case3Features.Skip(1).Take(2000).ToArray());

            // Second 2k features, shifted one place, of the third case are the same as the
            // fourth 2k features, shifted two places, of first case, since they are from the same page
            CollectionAssert.AreEqual(
                case1Features.Skip(6002).Take(2000).ToArray(),
                case3Features.Skip(2001).Take(2000).ToArray());

            // The features from the first page may be all zeros, if the page doesn't exist,
            // so there is an extra, present/missing flag, feature for that page with value of 1 when
            // the page is present and zero if missing
            Assert.That(case1Features.Take(2001).All(f => f == 0));
            Assert.AreEqual(1, case3Features[0]);

            // Check on two page document (one case with two missing pages)
            uss.LoadFrom(_ussFiles[6], false);
            voa.LoadFrom(_voaFiles[6], false);

            pages = uss.GetPages(true, " ").Size();
            Assert.AreEqual(2, pages);
            featureVectorCollection = encoder.GetFeatureVectors(uss, voa).ToArray();
            Assert.AreEqual(pages-1, featureVectorCollection.Length);

            case1Features = featureVectorCollection[0];
            Assert.That(case1Features.Take(2001).All(f => f == 0));
            Assert.That(case1Features.Skip(6001).Take(2001).All(f => f == 0));
        }

        // Use feature-hashing-auto-bag-of-words with four pages per case
        [Test, Category("LearningMachineDataEncoder")]
        public static void PaginationWithHashingAutoBowFourPages()
        {
            // With four pages per case there is the possibility for two pages to not be available
            // (first case in a document could be missing two and last one) so the feature vector
            // is 8034 instead of 8032 (32 are attribute features)
            int expectedFeatureVectorLength = 8034;
            SetPaginationFiles();
            var autoBoW = new SpatialStringFeatureVectorizer("4", 5, 2000) { UseFeatureHashing = true };
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, autoBoW, attributeFilter:"*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _eavFiles);

            // This number reflects the fact that there will be three pages of features for each case
            Assert.AreEqual(expectedFeatureVectorLength, encoder.FeatureVectorLength);

            // This is the per-page number, not the actual feature vector length
            Assert.AreEqual(2000, encoder.AutoBagOfWords.FeatureVectorLength);

            // This is the true number of BoW features
            Assert.AreEqual(8002, encoder.AutoBagOfWords.FeatureVectorLengthForPagination);

            var t = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles, _eavFiles);
            var features = t.Item1;
            var answers = t.Item2;
            Assert.AreEqual(features.Length, answers.Length);

            var uss = new SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);
            var voa = new IUnknownVectorClass();
            voa.LoadFrom(_voaFiles[0], false);

            int pages = uss.GetPages(true, " ").Size();
            Assert.AreEqual(4, pages);
            var featureVectorCollection = encoder.GetFeatureVectors(uss, voa).ToArray();
            Assert.AreEqual(pages-1, featureVectorCollection.Length);

            var case1Features = featureVectorCollection[0];
            Assert.AreEqual(expectedFeatureVectorLength, case1Features.Length);

            // First 2k features, shifted one place, of the third case are the same as the
            // third 2k features, shifted one place, of the first case, since they are from the same page
            var case3Features = featureVectorCollection[2];
            CollectionAssert.AreEqual(
                case1Features.Skip(4001).Take(2000).ToArray(),
                case3Features.Skip(1).Take(2000).ToArray());

            // Second 2k features, shifted one place, of the third case are the same as the
            // fourth 2k features, shifted two places, of first case, since they are from the same page
            CollectionAssert.AreEqual(
                case1Features.Skip(6002).Take(2000).ToArray(),
                case3Features.Skip(2001).Take(2000).ToArray());

            // The features from the first page may be all zeros, if the page doesn't exist,
            // so there is an extra, present/missing flag, feature for that page with value of 1 when
            // the page is present and zero if missing
            Assert.That(case1Features.Take(2001).All(f => f == 0));
            Assert.AreEqual(1, case3Features[0]);

            // Check on two page document (one case with two missing pages)
            uss.LoadFrom(_ussFiles[6], false);
            voa.LoadFrom(_voaFiles[6], false);

            pages = uss.GetPages(true, " ").Size();
            Assert.AreEqual(2, pages);
            featureVectorCollection = encoder.GetFeatureVectors(uss, voa).ToArray();
            Assert.AreEqual(pages-1, featureVectorCollection.Length);

            case1Features = featureVectorCollection[0];
            Assert.That(case1Features.Take(2001).All(f => f == 0));
            Assert.That(case1Features.Skip(6001).Take(2001).All(f => f == 0));
        }

        // Tests changing feature types
        [Test, Category("LearningMachineDataEncoder")]
        public static void ChangeFeatureVectorizerTypes()
        {
            SetPaginationFiles();
            var autoBoW = new SpatialStringFeatureVectorizer("", 5, 2000);
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, autoBoW);
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _eavFiles);

            Assert.AreEqual(21, encoder.AttributeFeatureVectorizers.Count());
            Assert.AreEqual(18, encoder.AttributeFeatureVectorizers
                .Where(fv => fv.FeatureType == FeatureVectorizerType.Numeric).Count());
            Assert.AreEqual(3, encoder.AttributeFeatureVectorizers
                .Where(fv => fv.FeatureType == FeatureVectorizerType.DiscreteTerms).Count());

            Assert.AreEqual(2057, encoder.FeatureVectorLength);

            var uss = new UCLID_RASTERANDOCRMGMTLib.SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);
            var voa = new UCLID_COMUTILSLib.IUnknownVectorClass();
            voa.LoadFrom(_voaFiles[0], false);

            foreach (var vectorizer in encoder.AttributeFeatureVectorizers)
            {
                Assert.That(vectorizer.IsFeatureTypeChangeable);
                vectorizer.FeatureType = FeatureVectorizerType.Exists;
            }
            Assert.AreEqual(2021, encoder.FeatureVectorLength);
            encoder.GetFeatureVectors(uss, voa);

            foreach (var vectorizer in encoder.AttributeFeatureVectorizers)
            {
                vectorizer.FeatureType = FeatureVectorizerType.Numeric;
            }
            Assert.AreEqual(2042, encoder.FeatureVectorLength);
            encoder.GetFeatureVectors(uss, voa);

            foreach (var vectorizer in encoder.AttributeFeatureVectorizers)
            {
                vectorizer.FeatureType = FeatureVectorizerType.DiscreteTerms;
            }
            Assert.That(encoder.FeatureVectorLength > 2100);
            encoder.GetFeatureVectors(uss, voa);

            // Spatial string vectorizer type cannot be changed
            Assert.That(!encoder.AutoBagOfWords.IsFeatureTypeChangeable);
            var ex = Assert.Throws<ExtractException>(() => encoder.AutoBagOfWords.FeatureType = FeatureVectorizerType.Exists);
            Assert.That( ex.Message, Is.EqualTo("Cannot change type of SpatialStringFeatureVectorizer") );
        }

        // Test comparison methods
        [Test, Category("LearningMachineDataEncoder")]
        public static void TestConfigurationEqualTo()
        {
            var encoder1 = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization);
            var encoder2 = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization);
            Assert.That(encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(encoder2.IsConfigurationEqualTo(encoder1));

            encoder2 = new LearningMachineDataEncoder(LearningMachineUsage.Pagination);
            Assert.That(!encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(!encoder2.IsConfigurationEqualTo(encoder1));

            encoder2 = null;
            Assert.That(!encoder1.IsConfigurationEqualTo(encoder2));

            var autoBoW1 = new SpatialStringFeatureVectorizer("", 5, 2000);
            var autoBoW2 = new SpatialStringFeatureVectorizer("", 5, 2000);
            encoder1 = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW1);
            encoder2 = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW2);

            Assert.That(encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(encoder2.IsConfigurationEqualTo(encoder1));

            encoder2.AutoBagOfWords.Enabled = false;
            Assert.That(!encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(!encoder2.IsConfigurationEqualTo(encoder1));

            encoder2 = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization);
            Assert.That(!encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(!encoder2.IsConfigurationEqualTo(encoder1));

            autoBoW2.Enabled = true;
            encoder1 = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW1, "*@Feature");
            encoder2 = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW2);
            Assert.That(!encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(!encoder2.IsConfigurationEqualTo(encoder1));

            encoder2 = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW2, "*@Feature");
            Assert.That(encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(encoder2.IsConfigurationEqualTo(encoder1));

            encoder1 = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW1, "*@Feature", true);
            Assert.That(!encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(!encoder2.IsConfigurationEqualTo(encoder1));

            encoder2 = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW2, "*@Feature", true);
            Assert.That(encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(encoder2.IsConfigurationEqualTo(encoder1));

            // Considered configurations equal if features only computed for one
            SetDocumentCategorizationFiles();
            encoder1.ComputeEncodings(_ussFiles, _voaFiles, _categories);
            Assert.That(encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(encoder2.IsConfigurationEqualTo(encoder1));

            // Considered equal if features computed for both and results are the same
            encoder2.ComputeEncodings(_ussFiles, _voaFiles, _categories);
            Assert.That(encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(encoder2.IsConfigurationEqualTo(encoder1));

            // If encodings are computed for both, then changes to an attribute feature vectorizer will affect comparison
            encoder1.AttributeFeatureVectorizers.First().Enabled = false;
            Assert.That(!encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(!encoder2.IsConfigurationEqualTo(encoder1));

            // Make them both match
            encoder2.AttributeFeatureVectorizers.First().Enabled = false;
            Assert.That(encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(encoder2.IsConfigurationEqualTo(encoder1));

            // Changing a feature vectorizer that is not enabled also makes the encoders not equal
            encoder1.AttributeFeatureVectorizers.First().FeatureType = FeatureVectorizerType.DiscreteTerms;
            encoder2.AttributeFeatureVectorizers.First().FeatureType = FeatureVectorizerType.Exists;
            Assert.That(!encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(!encoder2.IsConfigurationEqualTo(encoder1));
        }

        // Test shallow clone method
        [Test, Category("LearningMachineDataEncoder")]
        public static void TestShallowClone()
        {
            var autoBoW1 = new SpatialStringFeatureVectorizer("", 5, 2000);
            var encoder1 = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW1);
            var encoder2 = encoder1.ShallowClone();

            // Changing settings of a member changes for both original and clone
            encoder1.AutoBagOfWords.Enabled = false;
            Assert.That(encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(encoder2.IsConfigurationEqualTo(encoder1));

            // Compute encodings and clone again
            SetDocumentCategorizationFiles();
            encoder1.ComputeEncodings(_ussFiles, _voaFiles, _categories);
            encoder2 = encoder1.ShallowClone();

            // Changing settings of a feature vectorizer changes for both original and clone
            encoder1.AttributeFeatureVectorizers.First().Enabled = false;
            Assert.That(encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(encoder2.IsConfigurationEqualTo(encoder1));

            // Changing a setting on the original won't affect the clone
            encoder1.AttributeFilter = "*@Feature";
            Assert.That(!encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(!encoder2.IsConfigurationEqualTo(encoder1));
        }

        // Test deep clone method
        [Test, Category("LearningMachineDataEncoder")]
        public static void TestDeepClone()
        {
            var autoBoW1 = new SpatialStringFeatureVectorizer("", 5, 2000);
            var encoder1 = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW1);
            var encoder2 = encoder1.DeepClone();

            // Changing settings of a member of original won't affect the clone
            encoder1.AutoBagOfWords.Enabled = false;
            Assert.That(!encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(!encoder2.IsConfigurationEqualTo(encoder1));

            // Compute encodings and clone again
            SetDocumentCategorizationFiles();
            encoder1.ComputeEncodings(_ussFiles, _voaFiles, _categories);
            encoder2 = encoder1.DeepClone();

            // Changing settings of a feature vectorizer of the original won't affect the clone
            encoder1.AttributeFeatureVectorizers.First().Enabled = false;
            Assert.That(!encoder1.IsConfigurationEqualTo(encoder2));
            Assert.That(!encoder2.IsConfigurationEqualTo(encoder1));
        }

        // Helper function to build file lists for attribute categorization testing
        // These images are stapled together from Demo_LabDE images
        private static void SetAttributeCategorizationFiles()
        {
            _ussFiles = new string[7];
            _voaFiles = new string[7];
            _voaFiles2 = new string[7];
            _voaFiles3 = new string[7];
            for (int i = 0; i < _ussFiles.Length; i++)
            {
                var baseName = "Resources.LearningMachine.Pagination.Pagination_{0:D3}.tif{1}";
                _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ""));
                _ussFiles[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".uss"));
                _voaFiles[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".labeled_3_types.voa"));
                _voaFiles2[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".labeled_2_types.voa"));
                _voaFiles3[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".labeled.voa"));
            }
        }

        // Test attribute categorization when every attribute is a candidate
        // This test uses date attributes that are marked DOB, CollectionDate, or 'nothing' (empty AttributeType value)
        [Test, Category("LearningMachineDataEncoder")]
        public static void AttributeCategorizationAllCandidates()
        {
            SetAttributeCategorizationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, null, "*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, null);
            // More features than previous because vectorizers are now fully case-sensitive
            // https://extract.atlassian.net/browse/ISSUE-14483
            Assert.AreEqual(216, encoder.FeatureVectorLength);

            // There are three categories available
            Assert.AreEqual(3, encoder.AnswerNameToCode.Count);

            // All three types (DOB, CollectionDate, and 'nothing') are represented in these VOA files
            var featuresAndAnswers = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles, (string[])null);
            var answers = featuresAndAnswers.Item2;
            Assert.AreEqual(3, answers.Distinct().Count());
        }

        // This test took almost 3 minutes prior to fixing the code
        // and now it takes under 3 _seconds_ on my dev machine so fail if
        // it takes more than 10 seconds.
        // https://extract.atlassian.net/browse/ISSUE-15605
        [Test, Category("LearningMachineDataEncoder"), Timeout(10000)]
        public static void AttributeCategorizationManyAttributes()
        {
            SetAttributeCategorizationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(
                LearningMachineUsage.AttributeCategorization, null, "*@Feature")
            {
                AttributesToTokenizeFilter = "*"
            };
            encoder.ComputeEncodings(_ussFiles, _voaFiles, null);

            var text = new SpatialString();
            var attributes = new IUnknownVector();
            text.LoadFrom(_ussFiles[3], false);
            attributes.LoadFrom(_voaFiles[3], false);
            attributes = Enumerable.Repeat(attributes.ToIEnumerable<IAttribute>(), 10)
                .SelectMany(a => a)
                .ToIUnknownVector();

            var (features, _) = encoder.GetFeatureVectorAndAnswerCollections(new[] { text }, new[] { attributes }, null, false);

            Assert.AreEqual(570, features.Length);
        }

        // Test attribute categorization when not every attribute is a candidate
        // This test uses date attributes that are marked DOB, CollectionDate or not marked (don't have an AttributeType subattribute)
        [Test, Category("LearningMachineDataEncoder")]
        public static void AttributeCategorizationNotAllCandidates()
        {
            SetAttributeCategorizationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, null, "*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles2, null);
            // Less features result because there are less candidates
            // More features than previous because vectorizers are now fully case-sensitive
            // https://extract.atlassian.net/browse/ISSUE-14483
            Assert.AreEqual(79, encoder.FeatureVectorLength);

            // There are still three categories available because the 'nothing' category is always available
            Assert.AreEqual(3, encoder.AnswerNameToCode.Count);

            var featuresAndAnswers = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles2, (string[])null);
            var answers = featuresAndAnswers.Item2;
            // Only two types (DOB, CollectionDate) are represented in these VOA files
            Assert.AreEqual(2, answers.Distinct().Count());
        }

        // Test attribute categorization with categories that were not present during training (will be treated as unknown)
        // This test uses training data that only have DOB and 'nothing' labels but then computes feature vectors from
        // VOAs with CollectionDate labels too
        [Test, Category("LearningMachineDataEncoder")]
        public static void AttributeCategorizationUnknownLabels()
        {
            SetAttributeCategorizationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, null, "*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles3, null);
            // There are two categories available, DOB and 'nothing'
            Assert.AreEqual(2, encoder.AnswerNameToCode.Count);

            var featuresAndAnswers = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles2, (string[])null);
            var answers = featuresAndAnswers.Item2;
            // Three types (DOB, CollectionDate, and 'nothing') are represented in these VOA files but since the encoder
            // does not know about CollectionDate those labels will be treated like 'nothing'
            Assert.AreEqual(2, answers.Distinct().Count());
        }

        // Test attribute categorization encoding for prediction when not every attribute is labeled
        // Labels should have no effect when predicting, only when configuring/training, so all attributes
        // will be treated as candidates
        [Test, Category("LearningMachineDataEncoder")]
        public static void AttributeCategorizationPrediction()
        {
            SetAttributeCategorizationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, null, "*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles2, null);

            var featuresAndAnswers = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles2, (string[])null);
            // Only a subset of attributes are considered for training because not all are labeled (not all have an AttributeType subattribute)
            Assert.AreEqual(41, featuresAndAnswers.Item1.Length);

            var afutil = new UCLID_AFUTILSLib.AFUtility();
            var featureVectors =
                _ussFiles
                .Zip(_voaFiles2, Tuple.Create)
                .SelectMany(t =>
                {
                    var uss = new UCLID_RASTERANDOCRMGMTLib.SpatialString();
                    uss.LoadFrom(t.Item1, false);
                    var voa = afutil.GetAttributesFromFile(t.Item2);
                    return encoder.GetFeatureVectors(uss, voa);
                });
            // All attributes are considered for predicting because labels don't matter in this context
            Assert.AreEqual(134, featureVectors.Count());
        }

        // Specifically test that attribute feature vectorizers are fully case-sensitive
        // https://extract.atlassian.net/browse/ISSUE-14483
        [Test, Category("LearningMachineDataEncoder")]
        public static void AttributeFeatureVectorizersRespectCase()
        {
            SetAttributeCategorizationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, null, "*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, null);
            Assert.Less(
                encoder.AttributeFeatureVectorizers.Sum(v => v.RecognizedValues
                                                              .Distinct(StringComparer.OrdinalIgnoreCase)
                                                              .Count()),
                encoder.AttributeFeatureVectorizers.Sum(v => v.RecognizedValues.Count()));
        }

        // Test that attribute feature vectorizers sort terms by tf*idf score
        [Test, Category("LearningMachineDataEncoder")]
        public static void AttributeFeatureVectorizersOrderTermsByRelevance1()
        {
            SetAttributeCategorizationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, null, "*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, null);
            var left = encoder.AttributeFeatureVectorizers.First(v => v.Name == "Left");
            var terms = left.RecognizedValues.ToList();
            var mostRelevant = terms.Take(10).ToList();
            var leastRelevant = terms.Reverse<string>().Take(10).ToList();
            CollectionAssert.AreEqual(
                new[] { "Date", "DOB", "AM", "Collected", "R", "DOE", "COLLECTED", "DATE", "PRINTED", "ID" },
                mostRelevant);

            CollectionAssert.AreEqual(
                new[] { "U", "Time", "thru", "Requested", "REPRINT", "REPORTED", "Refill", "r", "Processed", "PM" },
                leastRelevant);
        }

        // Test that attribute feature vectorizers order of terms not dependant on input order
        [Test, Category("LearningMachineDataEncoder")]
        public static void AttributeFeatureVectorizersOrderTermsByRelevance2()
        {
            SetAttributeCategorizationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, null, "*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, null);
            var left = encoder.AttributeFeatureVectorizers.First(v => v.Name == "Left");
            var terms = left.RecognizedValues.ToList();

            var reversedUSSFiles = _ussFiles.Reverse().ToArray();
            var reversedVOAFiles = _voaFiles.Reverse().ToArray();
            encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, null, "*@Feature");
            encoder.ComputeEncodings(reversedUSSFiles, reversedVOAFiles, null);
            var terms2 = left.RecognizedValues.ToList();
            CollectionAssert.AreEqual(terms, terms2);
        }

        // Pagination: Test that attribute feature vectorizers sort terms by tf*idf score
        [Test, Category("LearningMachineDataEncoder")]
        public static void AttributeFeatureVectorizersOrderTermsByRelevance3()
        {
            SetPaginationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, null, "*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _eavFiles);
            var nameDiff01 = encoder.AttributeFeatureVectorizers.First(v => v.Name == "NameDiff01");
            Assert.That(nameDiff01.FeatureType == FeatureVectorizerType.Numeric);
            // Change to discrete terms type so that the recognized values ordering can be checked
            nameDiff01.FeatureType = FeatureVectorizerType.DiscreteTerms;
            var terms = nameDiff01.RecognizedValues.ToList();
            CollectionAssert.AreEqual(new[] { "3", "4", "8", "0", "10", "5" }, terms);
        }

        // Test that attribute feature vectorizers sort terms by tf*idf score after serialization/deserialization
        [Test, Category("LearningMachineDataEncoder")]
        public static void AttributeFeatureVectorizersOrderTermsByRelevance4()
        {
            SetPaginationFiles();
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.Pagination, null, "*@Feature");
            encoder.ComputeEncodings(_ussFiles, _voaFiles, _eavFiles);
            var nameDiff01 = encoder.AttributeFeatureVectorizers.First(v => v.Name == "NameDiff01");
            Assert.That(nameDiff01.FeatureType == FeatureVectorizerType.Numeric);
            // Change to discrete terms type so that the recognized values ordering can be checked
            nameDiff01.FeatureType = FeatureVectorizerType.DiscreteTerms;
            var terms = nameDiff01.RecognizedValues.ToList();
            CollectionAssert.AreEqual(new[] { "3", "4", "8", "0", "10", "5" }, terms);
        }

        // DocumentCategorization: Test that attribute feature vectorizers sort terms by tf*idf score
        // Also tests new attributeVectorizerMaxFeatures property
        [Test, Category("LearningMachineDataEncoder")]
        public static void AttributeFeatureVectorizersOrderTermsByRelevance5()
        {
            SetDocumentCategorizationFiles();
            var autoBoW = new SpatialStringFeatureVectorizer("1", 5, 2000);
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(
                LearningMachineUsage.DocumentCategorization, autoBoW,
                attributeFilter: null, negateFilter: false,
                attributeVectorizerMaxFeatures: 2000);
            encoder.ComputeEncodings(_ussFiles, _voaFiles2, _categories);

            // Compare top terms from the spatial string vectorizer with attribute vectorizer to check that the
            // algorithms sort the same
            var spatialStringVectorizer = encoder.AutoBagOfWords;
            var attributeVectorizer = encoder.AttributeFeatureVectorizers.First();
            var spatialRecognizedValues = spatialStringVectorizer.RecognizedValues.ToArray();
            var attributeRecognizedValues = attributeVectorizer.RecognizedValues.ToArray();
            CollectionAssert.AreEqual(spatialRecognizedValues, attributeRecognizedValues);
        }

        // DocumentCategorization: Test that attribute feature vectorizers sort terms by tf*idf score
        // Test using less categories so that there are multiple documents per category
        [Test, Category("LearningMachineDataEncoder")]
        public static void AttributeFeatureVectorizersOrderTermsByRelevance6()
        {
            SetDocumentCategorizationFiles();
            _categories = new string[]
            {
                "Deed of Trust",
                "Deed of Trust",
                "Deed of Trust",
                "Reconveyance",
                "Deed",
                "Deed",
                "Deed",
                "Deed of Trust",
                "Deed of Trust",
                "Notice of Federal Tax Lien"
            };
            var autoBoW = new SpatialStringFeatureVectorizer("1", 5, 2000);
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(
                LearningMachineUsage.DocumentCategorization, autoBoW,
                attributeFilter: null, negateFilter: false,
                attributeVectorizerMaxFeatures: 2000);
            encoder.ComputeEncodings(_ussFiles, _voaFiles2, _categories);

            // Compare top terms from the spatial string vectorizer with attribute vectorizer to check that the
            // algorithms sort the same
            var spatialStringVectorizer = encoder.AutoBagOfWords;

            var attributeVectorizer = encoder.AttributeFeatureVectorizers.First();
            var spatialStringFeatures = spatialStringVectorizer.RecognizedValues.ToArray();
            var attributeFeatures = attributeVectorizer.RecognizedValues.Take(2000).ToArray();

            CollectionAssert.AreEqual(spatialStringFeatures, attributeFeatures);
        }

        // Test that tokenizing works the same as auto-b-o-w
        [Test, Category("LearningMachineDataEncoder")]
        public static void AttributeFeatureVectorizerTokenization()
        {
            SetDocumentCategorizationFiles();
            var autoBoW = new SpatialStringFeatureVectorizer("", 5, 2000);
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(
                LearningMachineUsage.DocumentCategorization, autoBoW,
                attributeFilter: null, negateFilter: false,
                attributeVectorizerMaxFeatures: 2000,
                attributesToTokenize: "*",
                attributeVectorizerShingleSize: 5);

            var voaFiles = _ussFiles.Select(ussFile =>
            {
                var voa = new IUnknownVector();
                var ss = new SpatialString();
                ss.LoadFrom(ussFile, false);
                var attr = new AttributeClass { Name = "DocText", Value = ss };
                voa.PushBack(attr);
                var voaName = ussFile + ".voa";
                voa.SaveTo(voaName, false, null);
                return voaName;
            }).ToArray();
            encoder.ComputeEncodings(_ussFiles, voaFiles, _categories);

            foreach(var voaFile in voaFiles)
            {
                FileSystemMethods.DeleteFile(voaFile);
            }

            // Compare top terms from the spatial string vectorizer with attribute vectorizer to
            // confirm that the tokinizer works
            var spatialStringVectorizer = encoder.AutoBagOfWords;
            var attributeVectorizer = encoder.AttributeFeatureVectorizers.First();
            CollectionAssert.AreEqual(
                spatialStringVectorizer.RecognizedValues.ToArray(),
                attributeVectorizer.RecognizedValues.ToArray());
        }

        // Test that attribute features can have zero values seen without exception
        // https://extract.atlassian.net/browse/ISSUE-14611
        [Test, Category("LearningMachineDataEncoder")]
        public static void Issue14611()
        {
            SetDocumentCategorizationFiles();
            var voaFiles = _ussFiles.Select((ussFile, i) =>
            {
                var voa = new IUnknownVector();
                var ss = new SpatialString();
                ss.LoadFrom(ussFile, false);
                var attr = new AttributeClass { Name = "DocText", Value = ss };
                voa.PushBack(attr);
                if (i % 2 == 0)
                {
                    ss = new SpatialString();
                    attr = new AttributeClass { Name = "Even", Value = ss };
                    voa.PushBack(attr);
                }
                var voaName = ussFile + ".voa";
                voa.SaveTo(voaName, false, null);
                return voaName;
            }).ToArray();

            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(
                LearningMachineUsage.DocumentCategorization, null,
                attributeFilter: null, negateFilter: false,
                attributeVectorizerMaxFeatures: 2000,
                attributesToTokenize: "*",
                attributeVectorizerShingleSize: 5);

            encoder.ComputeEncodings(_ussFiles, voaFiles, _categories);

            using (var stream = new System.IO.MemoryStream())
            {
                var serializer = new System.Runtime.Serialization.NetDataContractSerializer()
                {
                    AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
                };
                serializer.Serialize(stream, encoder);
                stream.Flush();
                stream.Position = 0;
                encoder = (LearningMachineDataEncoder)serializer.Deserialize(stream);
            }
        }

        #endregion Tests
    }
}