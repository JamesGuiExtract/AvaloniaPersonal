using Extract.Testing.Utilities;
using NUnit.Framework;
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
            Assert.That(!encoder.IsConfigured);

            _voaFiles[1] = "blah.voa";
            ex = Assert.Throws<ExtractException>(() => encoder.ComputeEncodings(_ussFiles, _voaFiles, _categories));
            Assert.That( ex.Message, Is.EqualTo("One or more errors occurred.") );
            Assert.That(!encoder.IsConfigured);
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
            Assert.That( ex.Message, Is.EqualTo("Object has not been configured") );

            // Can't use if not configured successfully
            var uss = new UCLID_RASTERANDOCRMGMTLib.SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);
            var voa = new UCLID_COMUTILSLib.IUnknownVectorClass();
            voa.LoadFrom(_voaFiles[0], false);
            ex = Assert.Throws<ExtractException>(() => encoder.GetFeatureVectors(uss, voa));
            Assert.That( ex.Message, Is.EqualTo("Object has not been configured") );
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
            Assert.That(!encoder.IsConfigured);

            _voaFiles[1] = "blah.voa";
            ex = Assert.Throws<ExtractException>(() => encoder.ComputeEncodings(_ussFiles, _voaFiles, _categories));
            Assert.That( ex.Message, Is.EqualTo("Specified file or folder can't be found.") );
            Assert.That(!encoder.IsConfigured);
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
            Assert.That( ex.Message, Is.EqualTo("Object has not been configured") );

            // Can't use if not configured successfully
            var uss = new UCLID_RASTERANDOCRMGMTLib.SpatialStringClass();
            uss.LoadFrom(_ussFiles[0], false);
            var voa = new UCLID_COMUTILSLib.IUnknownVectorClass();
            voa.LoadFrom(_voaFiles[0], false);
            ex = Assert.Throws<ExtractException>(() => encoder.GetFeatureVectors(uss, voa));
            Assert.That( ex.Message, Is.EqualTo("Object has not been configured") );
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

        #endregion Tests
    }
}