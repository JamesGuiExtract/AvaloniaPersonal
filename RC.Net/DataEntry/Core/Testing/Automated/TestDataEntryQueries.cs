using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using System.Reflection;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using Extract.Utilities;

namespace Extract.DataEntry.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="DataEntryQuery"/> class.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("DataEntryQueries")]
    public class TestDataEntryQueries
    {
        /// <summary>
        /// The name of an embedded resource test VOA file.
        /// </summary>
        static readonly string _TEST_DATA_FILE = "Resources.Test.tif.voa";

        /// <summary>
        /// The name of an embedded resource test LabDE VOA file.
        /// </summary>
        static readonly string _LABDE_DATA_FILE = "Resources.005.tif.voa";

        /// <summary>
        /// The name of an embedded resource test FLEX Index VOA file.
        /// </summary>
        static readonly string _FLEX_INDEX_DATA_FILE = "Resources.Example01.tif.voa";

        /// <summary>
        /// The name of an embedded resource test FLEX Index data entry database file.
        /// </summary>
        static readonly string _FLEX_INDEX_DATABASE = "Resources.DemoFlexIndex.sdf";

        /// <summary>
        /// Manages the test images needed for testing.
        /// </summary>
        static TestFileManager<TestDataEntryQueries> _testImages;

        /// <summary>
        /// Attribute Finder utility for expanding tags.
        /// </summary>
        static AFUtility _utility = new AFUtility();

        /// <summary>
        /// Setup method to initalize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testImages = new TestFileManager<TestDataEntryQueries>();
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

        /// <summary>
        /// Tests the <see cref="SourceDocNameQueryNode"/>.
        /// </summary>
        [Test, Category("SourceDocNameQueryNode")]
        public static void TestSourceDocName()
        {
            string sourceDocName = _testImages.GetFile(_FLEX_INDEX_DATA_FILE);
            LoadDataFile(sourceDocName, null);

            string xml = @"<SourceDocName/>";

            DataEntryQuery query =
                DataEntryQuery.Create(xml, null, null, MultipleQueryResultSelectionMode.First, true);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == sourceDocName);
        }

        /// <summary>
        /// Tests the <see cref="SolutionDirectoryQueryNode"/>.
        /// </summary>
        [Test, Category("SolutionDirectoryQueryNode")]
        public static void TestSolutionDirectory()
        {
            DataEntryMethods.SolutionRootDirectory = Environment.CurrentDirectory;

            LoadDataFile(_testImages.GetFile(_FLEX_INDEX_DATA_FILE), null);

            string xml = @"<SolutionDirectory/>";

            DataEntryQuery query =
                DataEntryQuery.Create(xml, null, null, MultipleQueryResultSelectionMode.First, true);

            QueryResult result = query.Evaluate();

            string workingPathUNC = Environment.CurrentDirectory;
            FileSystemMethods.ConvertToNetworkPath(ref workingPathUNC, true);

            Assert.That(result.ToString() == workingPathUNC);
        }

        /// <summary>
        /// Tests the <see cref="AttributeQueryNode"/> based on the root of the attribute hierarchy.
        /// </summary>
        [Test, Category("AttributeQueryNode")]
        public static void TestAttributeQueryNodeRootAttribute()
        {
            LoadDataFile(_testImages.GetFile(_FLEX_INDEX_DATA_FILE), null);

            string xml = @"<Attribute>/ConsiderationAmount</Attribute>";

            DataEntryQuery query =
                DataEntryQuery.Create(xml, null, null, MultipleQueryResultSelectionMode.First, true);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "192800.00");
        }

        /// <summary>
        /// Tests the <see cref="AttributeQueryNode"/> relative to a particular attribute.
        /// </summary>
        [Test, Category("AttributeQueryNode")]
        public static void TestAttributeQueryNodeRelativeAttribute()
        {
            IUnknownVector attributes = LoadDataFile(_testImages.GetFile(_FLEX_INDEX_DATA_FILE), null);

            string xml = @"<Attribute>../LegalDescription/Lot</Attribute>";

            attributes = _utility.QueryAttributes(attributes, "ConsiderationAmount", false);
            IAttribute considerationAmountAttribute = (IAttribute)attributes.At(0);

            DataEntryQuery query =
                DataEntryQuery.Create(xml, considerationAmountAttribute, null, MultipleQueryResultSelectionMode.First, true);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "123");
        }

        /// <summary>
        /// Tests that the <see cref="AttributeQueryNode"/> can return a delimited list.
        /// </summary>
        [Test, Category("AttributeQueryNode")]
        public static void TestAttributeQueryList()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = @"<Attribute StringList=' and '>/Wagon/Sale/Customer</Attribute>";

            DataEntryQuery query =
                DataEntryQuery.Create(xml, null, null, MultipleQueryResultSelectionMode.List, true);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "Bob and Mary");
        }

        /// <summary>
        /// Tests the <see cref="SqlQueryNode"/>
        /// </summary>
        [Test, Category("TestSqlQuery")]
        public static void TestSqlQuery()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = @"<SQL>SELECT [Abbreviation] FROM [State] WHERE [Name] = 'Wisconsin'</SQL>";

            using (DbConnection dbConnection =
                    GetDatabaseConnection(_testImages.GetFile(_FLEX_INDEX_DATABASE)))
            {
                dbConnection.Open();

                DataEntryQuery query =
                    DataEntryQuery.Create(xml, null, dbConnection, MultipleQueryResultSelectionMode.First, true);

                QueryResult result = query.Evaluate();

                Assert.That(result.ToString() == "WI");
            }
        }

        /// <summary>
        /// Tests the <see cref="SqlQueryNode"/> with parameterization.
        /// </summary>
        [Test, Category("SqlQueryNode")]
        public static void TestSqlQueryParameterize()
        {
            LoadDataFile(_testImages.GetFile(_FLEX_INDEX_DATA_FILE), null);

            string xml = @"<SQL>SELECT CAST([ID] AS NVARCHAR(8)) FROM [DocumentType] " +
                "    WHERE [Name] = <Attribute Parameterize='True'>DocumentType</Attribute></SQL>";

            using (DbConnection dbConnection =
                    GetDatabaseConnection(_testImages.GetFile(_FLEX_INDEX_DATABASE)))
            {
                dbConnection.Open();

                DataEntryQuery query =
                    DataEntryQuery.Create(xml, null, dbConnection, MultipleQueryResultSelectionMode.First, true);

                QueryResult result = query.Evaluate();

                Assert.That(result.ToString() == "3");
            }
        }

        /// <summary>
        /// Tests the <see cref="SqlQueryNode"/> without parameterization.
        /// </summary>
        [Test, Category("SqlQueryNode")]
        public static void TestSqlQueryDoNotParameterize()
        {
            LoadDataFile(_testImages.GetFile(_FLEX_INDEX_DATA_FILE), null);

            string xml = @"<SQL>SELECT CAST([ID] AS NVARCHAR(8)) FROM [DocumentType] " +
                "    <Complex Parameterize='False'>WHERE [Name] = 'Grant Deed'</Complex></SQL>";

            using (DbConnection dbConnection =
                    GetDatabaseConnection(_testImages.GetFile(_FLEX_INDEX_DATABASE)))
            {
                dbConnection.Open();

                DataEntryQuery query =
                    DataEntryQuery.Create(xml, null, dbConnection, MultipleQueryResultSelectionMode.First, true);

                QueryResult result = query.Evaluate();

                Assert.That(result.ToString() == "5");
            }
        }

        /// <summary>
        /// Tests the <see cref="ResultQueryNode"/>.
        /// </summary>
        [Test, Category("ResultQueryNode")]
        public static void TestResultQueryBasic()
        {
            LoadDataFile(_testImages.GetFile(_FLEX_INDEX_DATA_FILE), null);

            string xml = @"<Attribute Name='DocType' Exclude='True'>DocumentType</Attribute>" +
                "    <Result Arg1='DocType'/>";

            using (DbConnection dbConnection =
                    GetDatabaseConnection(_testImages.GetFile(_FLEX_INDEX_DATABASE)))
            {
                dbConnection.Open();

                DataEntryQuery query =
                    DataEntryQuery.Create(xml, null, dbConnection, MultipleQueryResultSelectionMode.First, true);

                QueryResult result = query.Evaluate();

                Assert.That(result.ToString() == "Deed of Trust");
            }
        }

        /// <summary>
        /// Tests the <see cref="ResultQueryNode"/> using the simple syntax.
        /// </summary>
        [Test, Category("ResultQueryNode")]
        public static void TestResultQuerySimpleSyntax()
        {
            LoadDataFile(_testImages.GetFile(_FLEX_INDEX_DATA_FILE), null);

            string xml = @"<Attribute Name='DocType' Exclude='True'>DocumentType</Attribute>" +
                "    <DocType/>";

            using (DbConnection dbConnection =
                    GetDatabaseConnection(_testImages.GetFile(_FLEX_INDEX_DATABASE)))
            {
                dbConnection.Open();

                DataEntryQuery query =
                    DataEntryQuery.Create(xml, null, dbConnection, MultipleQueryResultSelectionMode.First, true);

                QueryResult result = query.Evaluate();

                Assert.That(result.ToString() == "Deed of Trust");
            }
        }

        /// <summary>
        /// Tests the <see cref="ResultQueryNode"/> using declarations.
        /// </summary>
        [Test, Category("ResultQueryNode")]
        public static void TestResultQueryDeclarations()
        {
            LoadDataFile(_testImages.GetFile(_FLEX_INDEX_DATA_FILE), null);

            string xml = @"<Declarations><Attribute Name='DocType'>DocumentType</Attribute></Declarations>" +
                "    <Query><DocType/></Query>";

            using (DbConnection dbConnection =
                    GetDatabaseConnection(_testImages.GetFile(_FLEX_INDEX_DATABASE)))
            {
                dbConnection.Open();

                DataEntryQuery query =
                    DataEntryQuery.Create(xml, null, dbConnection, MultipleQueryResultSelectionMode.First, true);

                QueryResult result = query.Evaluate();

                Assert.That(result.ToString() == "Deed of Trust");
            }
        }

        /// <summary>
        /// Tests the <see cref="ResultQueryNode"/> CombineLists operation. 
        /// </summary>
        [Test, Category("ResultQueryNode")]
        public static void TestResultQueryCombineOperation()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = @"<Declarations>" +
                "<Attribute SelectionMode='First' Name='First'>/Wagon/Sale/Customer</Attribute>" +
                "<Attribute SelectionMode='List' Name='All'>/Wagon/Sale/Customer</Attribute>" +
                "</Declarations>" +
                "<Query><Result Arg1='First' Arg2='All' Operation='CombineLists'/></Query>";

            DataEntryQuery query =
                DataEntryQuery.Create(xml, null, null, MultipleQueryResultSelectionMode.List, true);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "BobMary");
        }

        /// <summary>
        /// Tests the <see cref="ResultQueryNode"/> SubtractList operation.
        /// </summary>
        [Test, Category("ResultQueryNode")]
        public static void TestResultQuerySubtractOperation()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = @"<Declarations>" +
                "<Attribute SelectionMode='First' Name='First'>/Wagon/Sale/Customer</Attribute>" +
                "<Attribute SelectionMode='List' Name='All'>/Wagon/Sale/Customer</Attribute>" +
                "</Declarations>" +
                "<Query><Result Arg1='All' Arg2='First' Operation='SubtractList'/></Query>";

            DataEntryQuery query =
                DataEntryQuery.Create(xml, null, null, MultipleQueryResultSelectionMode.List, true);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "Mary");
        }

        /// <summary>
        /// Tests the <see cref="ResultQueryNode"/> CompareLists operation.
        /// </summary>
        [Test, Category("ResultQueryNode")]
        public static void TestResultQueryCompareOperation()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = @"<Declarations>" +
                "<Attribute SelectionMode='List' Name='A'>/Wagon/Sale/Customer</Attribute>" +
                "<Complex SelectionMode='List' Name='B'><Complex>Mary</Complex><Complex>Bob</Complex></Complex>" +
                "</Declarations>" +
                "<Query><Result Arg1='A' Arg2='B' Operation='CompareLists'/></Query>";

            DataEntryQuery query =
                DataEntryQuery.Create(xml, null, null, MultipleQueryResultSelectionMode.List, true);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "1");
        }

        /// <summary>
        /// Tests the <see cref="RegexQueryNode"/> with FirstMatchOnly.
        /// </summary>
        [Test, Category("RegexQueryNode")]
        public static void TestRegexQueryBasic()
        {
            LoadDataFile(_testImages.GetFile(_LABDE_DATA_FILE), null);

            string xml = @"<Regex Pattern='\&lt;[\d\.]+'>" +
                "<Attribute StringList=','>/Test/Component/Range</Attribute></Regex>";

            DataEntryQuery query =
                DataEntryQuery.Create(xml, null, null, MultipleQueryResultSelectionMode.List, true);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "<1.4");
        }

        /// <summary>
        /// Tests the <see cref="RegexQueryNode"/> without FirstMatchOnly.
        /// </summary>
        [Test, Category("RegexQueryNode")]
        public static void TestRegexAllMatches()
        {
            LoadDataFile(_testImages.GetFile(_LABDE_DATA_FILE), null);

            string xml = @"<Regex Pattern='\&lt;[\d\.]+' FirstMatchOnly='False' StringList=' '>" +
                "<Attribute StringList=','>/Test/Component/Range</Attribute></Regex>";

            DataEntryQuery query =
                DataEntryQuery.Create(xml, null, null, MultipleQueryResultSelectionMode.List, true);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "<1.4 <150 <130 <4.5");
        }

        /// <summary>
        /// Tests the <see cref="ExpressionQueryNode"/>.
        /// </summary>
        [Test, Category("ExpressionQueryNode")]
        public static void TestExpressionBasic()
        {
            string xml = @"<Expression>'Mickey' + ' ' + 'Mouse'</Expression>";

            DataEntryQuery query =
                DataEntryQuery.Create(xml, null, null, MultipleQueryResultSelectionMode.List, true);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "Mickey Mouse");
        }

        /// <summary>
        /// Tests the <see cref="ExpressionQueryNode"/> with strongly typed parameters.
        /// </summary>
        [Test, Category("ExpressionQueryNode")]
        public static void TestExpressionType()
        {
            string xml = @"<Expression>2.0 + <Complex Type='Double'>2</Complex></Expression>";

            DataEntryQuery query =
                DataEntryQuery.Create(xml, null, null, MultipleQueryResultSelectionMode.List, true);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "4");
        }

        /// <summary>
        /// Tests the <see cref="ExpressionQueryNode"/> with default values for strongly typed parameters.
        /// </summary>
        [Test, Category("ExpressionQueryNode")]
        public static void TestExpressionTypeDefault()
        {
            string xml = @"<Expression>2.0 + <Complex Type='Double' Default='2.0'>One</Complex></Expression>";

            DataEntryQuery query =
                DataEntryQuery.Create(xml, null, null, MultipleQueryResultSelectionMode.List, true);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "4");
        }

        /// <summary>
        /// A complex test to sum values from a VOA file using nested nodes of various types.
        /// </summary>
        [Test, Category("Composite")]
        public static void TestCompositeSumColumnValues()
        {
            LoadDataFile(_testImages.GetFile(_LABDE_DATA_FILE), null);

            string xml = @"<Expression Type='Double'>" +
                            @"<Complex Parameterize='False'>" +
                                @"<Regex Pattern='[\d\.]+' FirstMatchOnly='False' SelectionMode='List' StringList=' + '>" +
                                @"<Attribute SelectionMode='List' StringList=','>/Test/Component/Value</Attribute>" +
                                @"</Regex>" +
                            @"</Complex>" +
                        @"</Expression>";

            DataEntryQuery query =
                DataEntryQuery.Create(xml, null, null, MultipleQueryResultSelectionMode.First, true);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "1678.96");
        }

        /// <summary>
        /// Gets the database connection.
        /// </summary>
        /// <param name="databaseFileName">Name of the database file.</param>
        /// <returns></returns>
        static DbConnection GetDatabaseConnection(string databaseFileName)
        {
            string connectionString = "Data Source='" + databaseFileName + "';";

            return new SqlCeConnection(connectionString);
        }

        /// <summary>
        /// Loads the data file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="dbConnection"></param>
        static IUnknownVector LoadDataFile(string fileName, DbConnection dbConnection)
        {
            IUnknownVector attributes = (IUnknownVector)new IUnknownVector();
            attributes.LoadFrom(fileName, false);

            AttributeStatusInfo.ResetData(fileName, attributes, dbConnection);

            InitializeAttributes(attributes);

            return attributes;
        }

        /// <summary>
        /// Initializes the attributes.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        static void InitializeAttributes(IUnknownVector attributes)
        {
            int attributeCount = attributes.Size();
            for (int i = 0; i < attributeCount; i++)
            {
                IAttribute attribute = (IAttribute)attributes.At(i);
                AttributeStatusInfo.Initialize(attribute, attributes, null);

                InitializeAttributes(attribute.SubAttributes);
            }
        }
    }
}
