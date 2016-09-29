using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

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
            for (int i = 0; i < _ussFiles.Length; i++)
            {
                var baseName = "Resources.LearningMachine.DocumentCategorization.Example{0:D2}.tif{1}";
                _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ""));
                _ussFiles[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".uss"));
                _voaFiles[i] = _testFiles.GetFile(string.Format(CultureInfo.CurrentCulture, baseName, i+1, ".voa"));
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
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, attributeFilter:"DocumentType");
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

        // Test exception handling
        // One input VOA is non-existent, one is empty string
        [Test, Category("LearningMachineDataEncoder")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "VOA")]
        public static void DocumentCategorizationWithAutoBowAndBadVOA()
        {
            SetDocumentCategorizationFiles();
            var autoBoW = new SpatialStringFeatureVectorizer("", 5, 2000);
            LearningMachineDataEncoder encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW,
                attributeFilter:"DocumentType");

            _voaFiles[1] = "";
            var ex = Assert.Throws<ExtractException>(() => encoder.ComputeEncodings(_ussFiles, _voaFiles, _categories));
            Assert.That( ex.Message, Is.EqualTo("Cannot get attributes from unsupported file type.") );
            Assert.That(!encoder.AreEncodingsComputed);

            _voaFiles[1] = "blah.voa";
            ex = Assert.Throws<ExtractException>(() => encoder.ComputeEncodings(_ussFiles, _voaFiles, _categories));
            Assert.That( ex.Message, Is.EqualTo("Specified file or folder can't be found.") );
            Assert.That(!encoder.AreEncodingsComputed);
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
            var firstPageValues = autoBoW.DistinctValuesSeen;
            Assert.AreEqual(2000, firstPageValues.Count());

            // Compare to all pages
            autoBoW.PagesToProcess = "";
            encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW);
            encoder.ComputeEncodings(_ussFiles, null, _categories);
            var allPageValues = autoBoW.DistinctValuesSeen;
            Assert.AreEqual(2000, allPageValues.Count());
            CollectionAssert.AreNotEquivalent(firstPageValues, allPageValues);

            // Using page number that is not in any image works, but results in no features
            autoBoW.PagesToProcess = "100";
            encoder = new LearningMachineDataEncoder(LearningMachineUsage.DocumentCategorization, autoBoW);
            encoder.ComputeEncodings(_ussFiles, null, _categories);
            var pageOneHundredValues = autoBoW.DistinctValuesSeen;
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
            Assert.AreEqual(199, encoder.FeatureVectorLength);

            // There are three categories available
            Assert.AreEqual(3, encoder.AnswerNameToCode.Count);

            // All three types (DOB, CollectionDate, and 'nothing') are represented in these VOA files
            var featuresAndAnswers = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles, null);
            var answers = featuresAndAnswers.Item2;
            Assert.AreEqual(3, answers.Distinct().Count());
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
            Assert.AreEqual(73, encoder.FeatureVectorLength);

            // There are still three categories available because the 'nothing' category is always available
            Assert.AreEqual(3, encoder.AnswerNameToCode.Count);

            var featuresAndAnswers = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles2, null);
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

            var featuresAndAnswers = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles2, null);
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

            var featuresAndAnswers = encoder.GetFeatureVectorAndAnswerCollections(_ussFiles, _voaFiles2, null);
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

        #endregion Tests
    }
}