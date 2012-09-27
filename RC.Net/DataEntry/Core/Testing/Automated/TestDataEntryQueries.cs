using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

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
        /// The name of an embedded resource test LabDE data entry database file.
        /// </summary>
        static readonly string _LABDE_DATABASE = "Resources.OrderMappingDB.sdf";

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

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

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

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

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

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

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
                DataEntryQuery.Create(xml, considerationAmountAttribute, null);

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

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "Bob and Mary");
        }

        /// <summary>
        /// Tests the SpatialField attribute.
        /// </summary>
        [Test, Category("AttributeQueryNode")]
        public static void TestAttributeSpatialField()
        {
            LoadDataFile(_testImages.GetFile(_FLEX_INDEX_DATA_FILE), null);

            string xml = "<Attribute Name='ConsiderationAmount'>/ConsiderationAmount</Attribute>\r\n" +
                         "<ConsiderationAmount SpatialField='Page'/>\r\n" +
                         "<ConsiderationAmount SpatialField='Left'/>\r\n" +
                         "<ConsiderationAmount SpatialField='Top'/>\r\n" +
                         "<ConsiderationAmount SpatialField='Right'/>\r\n" +
                         "<ConsiderationAmount SpatialField='Bottom'/>\r\n" +
                         "<ConsiderationAmount SpatialField='StartX'/>\r\n" +
                         "<ConsiderationAmount SpatialField='StartY'/>\r\n" +
                         "<ConsiderationAmount SpatialField='EndX'/>\r\n" +
                         "<ConsiderationAmount SpatialField='EndY'/>\r\n" +
                         "<ConsiderationAmount SpatialField='Height'/>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            string[] results = query.Evaluate().ToStringArray();

            Assert.That(results.Length == 11);
            Assert.That(results[0].ToString() == "192800.00");
            Assert.That(results[1].ToString() == "2");
            Assert.That(results[2].ToString() == "559");
            Assert.That(results[3].ToString() == "1259");
            Assert.That(results[4].ToString() == "741");
            Assert.That(results[5].ToString() == "1292");
            Assert.That(results[6].ToString() == "559");
            Assert.That(results[7].ToString() == "1276");
            Assert.That(results[8].ToString() == "741");
            Assert.That(results[9].ToString() == "1276");
            Assert.That(results[10].ToString() == "32");
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

                DataEntryQuery query = DataEntryQuery.Create(xml, null, dbConnection);

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

                DataEntryQuery query = DataEntryQuery.Create(xml, null, dbConnection);

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
                "    <Composite Parameterize='False'>WHERE [Name] = 'Grant Deed'</Composite></SQL>";

            using (DbConnection dbConnection =
                    GetDatabaseConnection(_testImages.GetFile(_FLEX_INDEX_DATABASE)))
            {
                dbConnection.Open();

                DataEntryQuery query = DataEntryQuery.Create(xml, null, dbConnection);

                QueryResult result = query.Evaluate();

                Assert.That(result.ToString() == "5");
            }
        }

        /// <summary>
        /// Tests the <see cref="ResultQueryNode"/> using the simple syntax.
        /// </summary>
        [Test, Category("ResultQueryNode")]
        public static void TestResultQueryBasic()
        {
            LoadDataFile(_testImages.GetFile(_FLEX_INDEX_DATA_FILE), null);

            string xml = @"<Attribute Name='DocType' Exclude='True'>DocumentType</Attribute>" +
                "    <DocType/>";

            using (DbConnection dbConnection =
                    GetDatabaseConnection(_testImages.GetFile(_FLEX_INDEX_DATABASE)))
            {
                dbConnection.Open();

                DataEntryQuery query = DataEntryQuery.Create(xml, null, dbConnection);

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

                DataEntryQuery query = DataEntryQuery.Create(xml, null, dbConnection);

                QueryResult result = query.Evaluate();

                Assert.That(result.ToString() == "Deed of Trust");
            }
        }

        /// <summary>
        /// Tests the <see cref="RegexQueryNode"/> with SelectionMode == First.
        /// </summary>
        [Test, Category("RegexQueryNode")]
        public static void TestRegexQueryFirstMatch()
        {
            LoadDataFile(_testImages.GetFile(_LABDE_DATA_FILE), null);

            string xml = @"<Regex Pattern='\&lt;[\d\.]+' SelectionMode='First'>" +
                "<Attribute StringList=','>/Test/Component/Range</Attribute></Regex>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "<1.4");
        }

        /// <summary>
        /// Tests the <see cref="RegexQueryNode"/> without SelectionMode == List.
        /// </summary>
        [Test, Category("RegexQueryNode")]
        public static void TestRegexAllMatches()
        {
            LoadDataFile(_testImages.GetFile(_LABDE_DATA_FILE), null);

            string xml = @"<Regex Pattern='\&lt;[\d\.]+' StringList=' '>" +
                "<Attribute StringList=','>/Test/Component/Range</Attribute></Regex>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

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

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "Mickey Mouse");
        }

        /// <summary>
        /// Tests the <see cref="ExpressionQueryNode"/> with strongly typed parameters.
        /// </summary>
        [Test, Category("ExpressionQueryNode")]
        public static void TestExpressionType()
        {
            string xml = @"<Expression>2.0 + <Composite Type='Double'>2</Composite></Expression>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "4");
        }

        /// <summary>
        /// Tests the <see cref="ExpressionQueryNode"/> with default values for strongly typed parameters.
        /// </summary>
        [Test, Category("ExpressionQueryNode")]
        public static void TestExpressionTypeDefault()
        {
            string xml = @"<Expression>2.0 + <Composite Type='Double' Default='2.0'>One</Composite></Expression>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "4");
        }

        /// <summary>
        /// Tests the <see cref="ExpressionQueryNode"/> where a parameterized value us cast as an array.
        /// </summary>
        [Test, Category("ExpressionQueryNode")]
        public static void TestExpressionArrays()
        {
            LoadDataFile(_testImages.GetFile(_LABDE_DATA_FILE), null);

            string xml = @"<Expression>" +
                            @"<Attribute Type='double[]'>/Test/Component/Value</Attribute>" +
                            @".?{#this > 100.0}" +
                        @"</Expression>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            string[] results = query.Evaluate().ToStringArray();

            Assert.That(results.Length == 4);
            Assert.That(results[0].ToString() == "136");
            Assert.That(results[1].ToString() == "101");
            Assert.That(results[2].ToString() == "250");
            Assert.That(results[3].ToString() == "159");
        }

        /// <summary>
        /// Tests combining lists with <see cref="ExpressionQueryNode"/>s. 
        /// </summary>
        [Test, Category("ExpressionQueryNode")]
        public static void TestExpressionCombineLists()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = @"<Declarations>" +
                "<Attribute SelectionMode='First' Name='First'>/Wagon/Sale/Customer</Attribute>" +
                "<Attribute SelectionMode='List' Name='All'>/Wagon/Sale/Customer</Attribute>" +
                "</Declarations>" +
                "<Query><Expression><First Type='string[]'/> + <All Type='string[]'/></Expression></Query>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "BobMary");
        }

        /// <summary>
        /// Tests subtracting lists with <see cref="ExpressionQueryNode"/>s. 
        /// </summary>
        [Test, Category("ExpressionQueryNode")]
        public static void TestExpressionSubtractLists()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = @"<Declarations>" +
                "<Attribute SelectionMode='First' Name='First'>/Wagon/Sale/Customer</Attribute>" +
                "<Attribute SelectionMode='List' Name='All'>/Wagon/Sale/Customer</Attribute>" +
                "</Declarations>" +
                "<Query><Expression><All Type='string[]'/> - <First Type='string[]'/></Expression></Query>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "Mary");
        }

        /// <summary>
        /// Tests comparing lists with <see cref="ExpressionQueryNode"/>s. 
        /// <para><b>Tests</b></para>
        /// DataEntry:1084
        /// </summary>
        [Test, Category("ExpressionQueryNode")]
        public static void TestExpressionCompareLists()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = @"<Declarations>" +
                "<Attribute Name='A'>/Wagon/Sale/Customer</Attribute>" +
                "<Composite Name='B'><Composite>Mary</Composite><Composite>Bob</Composite></Composite>" +
                "</Declarations>" +
                "<Query><Expression><A Type='string[]'/>.sort() == <B Type='string[]'/>.sort()</Expression></Query>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "True");
        }

        /// <summary>
        /// A test of the AbortIfEmpty attribute.
        /// </summary>
        [Test, Category("ExpressionQueryNode")]
        public static void TestExpressionAbortIfEmpty()
        {
            LoadDataFile(_testImages.GetFile(_LABDE_DATA_FILE), null);

            string xml = @"Empty:<Expression> " +
	                        @"0 + <Attribute StringList=' + ' Parameterize='False' AbortIfEmpty='True'>/Test/Component/Garbage</Attribute>" +
                         @"</Expression>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "Empty:");
        }

        /// <summary>
        /// Tests the distinct selection mode where the distinct value is referenced by another
        /// node.
        /// </summary>
        [Test, Category("Composite")]
        public static void TestDistinctNodeReference()
        {
            LoadDataFile(_testImages.GetFile(_LABDE_DATA_FILE), null);

            string xml = @"<Expression>" +
                            @"<Composite Type='string[]'>" +
                                @"The value of " +
                                @"<Attribute SelectionMode='Distinct' Name='Component'>/Test/Component</Attribute>" +
                                @" is " +
                                @"<Attribute Root='Component'>Value</Attribute>" +
                            @"</Composite>[3]" +
                        @"</Expression>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "The value of ECO2 is 27");
        }

        /// <summary>
        /// Tests the distinct selection mode where the distinct value is referenced by another
        /// node.
        /// </summary>
        [Test, Category("Composite")]
        public static void TestDistinctNodeReference2()
        {
            IUnknownVector attributes = LoadDataFile(_testImages.GetFile(_LABDE_DATA_FILE), null);

            attributes = _utility.QueryAttributes(attributes, "PhysicianInfo/OrderingPhysicianName/Last", false);
            IAttribute physicianLastNameAttribute = (IAttribute)attributes.At(0);

            string xml = "First name is \r\n" +
                            "<SQL SelectionMode='Distinct'>\r\n" +
                                "SELECT FirstName FROM Physician WHERE LastName = <Attribute Name='LastName'>.</Attribute>\r\n" +
                            // [DataEntry:1124]: This bug prevented the LastName reference from working correctly unless a newline
                            // was added after </SQL> and before the comma.
                            "</SQL>, last name is <LastName/>.\r\n";

            using (DbConnection dbConnection =
                    GetDatabaseConnection(_testImages.GetFile(_LABDE_DATABASE)))
            {
                dbConnection.Open();

                DataEntryQuery query = DataEntryQuery.Create(xml, physicianLastNameAttribute, dbConnection);

                string[] results = query.Evaluate().ToStringArray();

                Assert.That(results.Length == 3);
                Assert.That(results[0].ToString() == "First name is JEFFREY, last name is ROGERS.");
                Assert.That(results[1].ToString() == "First name is DAVID, last name is ROGERS.");
                Assert.That(results[2].ToString() == "First name is JOHN, last name is ROGERS.");
            }
        }

        /// <summary>
        /// Tests the distinct selection mode where the distinct value is referenced by another
        /// node.
        /// </summary>
        [Test, Category("Composite")]
        public static void TestDistinctNodeReference3()
        {
            LoadDataFile(_testImages.GetFile(_LABDE_DATA_FILE), null);

            // [DataEntry:1124]: This bug prevented anything beyond the first reference from being
            // populated, so OrderCode would be blank.
            string xml = "<Declarations>\r\n" +
                            "<Attribute Name='Components'>Test/Component</Attribute>\r\n" +
                        "</Declarations>\r\n" +
                        "<Query>\r\n" +
                            "Component=<Components SelectionMode='Distinct' Name='Component'/>," +
                            "TestCode=<Attribute Root='Component'>TestCode</Attribute>," +
                            "OrderCode=<Attribute Root='Component' >../OrderCode</Attribute>" +
                        "</Query>\r\n";

            using (DbConnection dbConnection =
                    GetDatabaseConnection(_testImages.GetFile(_LABDE_DATABASE)))
            {
                dbConnection.Open();

                DataEntryQuery query = DataEntryQuery.Create(xml, null, dbConnection);

                string[] results = query.Evaluate().ToStringArray();

                Assert.That(results.Length == 45);
                Assert.That(results[0].ToString() == "Component=SODIUM,TestCode=NA,OrderCode=CMP1");
                Assert.That(results[20].ToString() == "Component=RBC,TestCode=RBC,OrderCode=CBC");
                Assert.That(results[44].ToString() == "Component=TSH,TestCode=TSH,OrderCode=TSH");
            }
        }

        /// <summary>
        /// A test to sum values from a VOA file using nested nodes of different types.
        /// </summary>
        [Test, Category("Composite")]
        public static void TestCompositeSumColumnValues()
        {
            LoadDataFile(_testImages.GetFile(_LABDE_DATA_FILE), null);

            string xml = @"<Expression>" +
                            @"<Attribute StringList=' + ' Parameterize='False'>/Test/Component/Value</Attribute>" +
                         @"</Expression>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "1668.16");
        }

        /// <summary>
        /// A test to sum values from a VOA file using nested nodes of various types.
        /// </summary>
        [Test, Category("Composite")]
        public static void TestCompositeSumColumnValues2()
        {
            LoadDataFile(_testImages.GetFile(_LABDE_DATA_FILE), null);

            string xml = @"<Expression>" +
                            @"<Regex Pattern='[\d\.]+' StringList=' + ' Parameterize='False'>" +
                                @"<Attribute StringList=','>/Test/Component/Value</Attribute>" +
                            @"</Regex>" +
                        @"</Expression>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "1668.16");
        }

        /// <summary>
        /// A test to sum values by casting them to a double array in an expression.
        /// </summary>
        [Test, Category("Composite")]
        public static void TestCompositeSumColumnValues3()
        {
            LoadDataFile(_testImages.GetFile(_LABDE_DATA_FILE), null);

            string xml = @"<Expression>" +
                            @"<Attribute Type='double[]'>/Test/Component/Value</Attribute>" +
                            @".sum()" +
                         @"</Expression>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "1668.16");
        }

        /// <summary>
        /// A test to sum values from a VOA file using the distinct selection mode.
        /// </summary>
        [Test, Category("Composite")]
        public static void TestCompositeSumColumnValues4()
        {
            LoadDataFile(_testImages.GetFile(_LABDE_DATA_FILE), null);

            string xml = @"<Expression>" +
                            @"<Composite Parameterize='False'>" +
                                @"<Attribute SelectionMode='Distinct'>/Test/Component/Value</Attribute> + " +
                            @"</Composite>" +
                            @"0.0" +
                        @"</Expression>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            QueryResult result = query.Evaluate();

            Assert.That(result.ToString() == "1668.16");
        }

        /// <summary>
        /// A test to find missing AKA values using a variety of query features.
        /// </summary>
        [Test, Category("Composite")]
        public static void TestCompositeFindMissingAlternateTestNames()
        {
            string sourceDocName = _testImages.GetFile(_LABDE_DATA_FILE);
            LoadDataFile(sourceDocName, null);

            string xml = @"<SQL>" +
                        @"SELECT VOAFileData.* FROM " +
                        @"(" +
                            @"<Composite Parameterize='false' StringList=' UNION '> " +
                                @"SELECT " +
                                    @"'<Attribute SelectionMode='Distinct' Name='OriginalName'>Test/Component/OriginalName</Attribute>' AS OriginalName, " +
                                    @"'<Attribute Root='OriginalName'>../TestCode</Attribute>' AS TestCode, " +
                                    @"'<SourceDocName/>' AS SourceDocName, " +
                                    @"'<OriginalName SpatialField='Page'/>' AS Page, " +
                                    @"'<OriginalName SpatialField='StartX'/>' AS StartX, " +
                                    @"'<OriginalName SpatialField='StartY'/>' AS StartY, " +
                                    @"'<OriginalName SpatialField='EndX'/>' AS EndX, " +
                                    @"'<OriginalName SpatialField='EndY'/>' AS EndY, " +
                                    @"'<OriginalName SpatialField='Height'/>' AS Height " +
                            @"</Composite> " +
                        @") AS VOAFileData " +
                        @"INNER JOIN LabOrderTest ON LabOrderTest.TestCode = VOAFileData.TestCode " +
                        @"INNER JOIN LabTest ON LabTest.TestCode = VOAFileData.TestCode " +
                        @"WHERE LabTest.OfficialName &lt;&gt; VOAFileData.OriginalName " +
                        @"AND LabOrderTest.TestCode &lt;&gt; VOAFileData.OriginalName " +
                        @"AND NOT VOAFileData.OriginalName IN " +
                        @"( " +
                            @"SELECT [Name] FROM AlternateTestName WHERE TestCode = VOAFileData.TestCode " +
                        @") " +
                        @"</SQL>";

            using (DbConnection dbConnection =
                    GetDatabaseConnection(_testImages.GetFile(_LABDE_DATABASE)))
            {
                dbConnection.Open();

                DataEntryQuery query = DataEntryQuery.Create(xml, null, dbConnection);

                string[] results = query.Evaluate().ToStringArray();

                Assert.That(results.Length == 2);
                Assert.That(results[0].ToString() == "LYM%, JLYM, " + sourceDocName + ", 1, 173, 1757, 314, 1757, 46");
                Assert.That(results[1].ToString() == "MON%, KMONO, " + sourceDocName + ", 1, 170, 1808, 317, 1808, 48");
            }
        }

        /// <summary>
        /// A test of how results combine based on the selection mode.
        /// </summary>
        [Test, Category("SelectionMode")]
        public static void TestSelectionModeFirstFirst()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = "<Composite SelectionMode='First'>\r\n" +
                        "A\r\n" +
                        "B\r\n" +
                        "</Composite>\r\n" +
                        "<Composite SelectionMode='First'>\r\n" +
                        "1\r\n" +
                        "2\r\n" +
                        "</Composite>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            string[] results = query.Evaluate().ToStringArray();

            Assert.That(results.Length == 2);
            Assert.That(results[0].ToString() == "A");
            Assert.That(results[1].ToString() == "1");
        }

        /// <summary>
        /// A test of how results combine based on the selection mode.
        /// </summary>
        [Test, Category("SelectionMode")]
        public static void TestSelectionModeFirstList()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = "<Composite SelectionMode='First'>\r\n" +
                        "A\r\n" +
                        "B\r\n" +
                        "</Composite>\r\n" +
                        "<Composite SelectionMode='List'>\r\n" +
                        "1\r\n" +
                        "2\r\n" +
                        "</Composite>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            string[] results = query.Evaluate().ToStringArray();

            Assert.That(results.Length == 3);
            Assert.That(results[0].ToString() == "A");
            Assert.That(results[1].ToString() == "1");
            Assert.That(results[2].ToString() == "2");
        }

        /// <summary>
        /// A test of how results combine based on the selection mode.
        /// </summary>
        [Test, Category("SelectionMode")]
        public static void TestSelectionModeFirstDistinct()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = "<Composite SelectionMode='First'>\r\n" +
                        "A\r\n" +
                        "B\r\n" +
                        "</Composite>\r\n" +
                        "<Composite SelectionMode='Distinct'>\r\n" +
                        "1\r\n" +
                        "2\r\n" +
                        "</Composite>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            string[] results = query.Evaluate().ToStringArray();

            Assert.That(results.Length == 2);
            Assert.That(results[0].ToString() == "A1");
            Assert.That(results[1].ToString() == "A2");
        }

        /// <summary>
        /// A test of how results combine based on the selection mode.
        /// </summary>
        [Test, Category("SelectionMode")]
        public static void TestSelectionModeListFirst()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = "<Composite SelectionMode='List'>\r\n" +
                        "A\r\n" +
                        "B\r\n" +
                        "</Composite>\r\n" +
                        "<Composite SelectionMode='First'>\r\n" +
                        "1\r\n" +
                        "2\r\n" +
                        "</Composite>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            string[] results = query.Evaluate().ToStringArray();

            Assert.That(results.Length == 3);
            Assert.That(results[0].ToString() == "A");
            Assert.That(results[1].ToString() == "B");
            Assert.That(results[2].ToString() == "1");
        }

        /// <summary>
        /// A test of how results combine based on the selection mode.
        /// </summary>
        [Test, Category("SelectionMode")]
        public static void TestSelectionModeListList()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = "<Composite SelectionMode='List'>\r\n" +
                        "A\r\n" +
                        "B\r\n" +
                        "</Composite>\r\n" +
                        "<Composite SelectionMode='List'>\r\n" +
                        "1\r\n" +
                        "2\r\n" +
                        "</Composite>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            string[] results = query.Evaluate().ToStringArray();

            Assert.That(results.Length == 4);
            Assert.That(results[0].ToString() == "A");
            Assert.That(results[1].ToString() == "B");
            Assert.That(results[2].ToString() == "1");
            Assert.That(results[3].ToString() == "2");
        }

        /// <summary>
        /// A test of how results combine based on the selection mode.
        /// </summary>
        [Test, Category("SelectionMode")]
        public static void TestSelectionModeListDistinct()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = "<Composite SelectionMode='List'>\r\n" +
                        "A\r\n" +
                        "B\r\n" +
                        "</Composite>\r\n" +
                        "<Composite SelectionMode='Distinct'>\r\n" +
                        "1\r\n" +
                        "2\r\n" +
                        "</Composite>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            string[] results = query.Evaluate().ToStringArray();

            Assert.That(results.Length == 2);
            Assert.That(results[0].ToString() == "AB1");
            Assert.That(results[1].ToString() == "AB2");
        }

        /// <summary>
        /// A test of how results combine based on the selection mode.
        /// </summary>
        [Test, Category("SelectionMode")]
        public static void TestSelectionModeDistinctFirst()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = "<Composite SelectionMode='Distinct'>\r\n" +
                        "A\r\n" +
                        "B\r\n" +
                        "</Composite>\r\n" +
                        "<Composite SelectionMode='First'>\r\n" +
                        "1\r\n" +
                        "2\r\n" +
                        "</Composite>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            string[] results = query.Evaluate().ToStringArray();

            Assert.That(results.Length == 2);
            Assert.That(results[0].ToString() == "A1");
            Assert.That(results[1].ToString() == "B1");
        }

        /// <summary>
        /// A test of how results combine based on the selection mode.
        /// </summary>
        [Test, Category("SelectionMode")]
        public static void TestSelectionModeDistinctList()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = "<Composite SelectionMode='Distinct'>\r\n" +
                        "A\r\n" +
                        "B\r\n" +
                        "</Composite>\r\n" +
                        "<Composite SelectionMode='List'>\r\n" +
                        "1\r\n" +
                        "2\r\n" +
                        "</Composite>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            string[] results = query.Evaluate().ToStringArray();

            Assert.That(results.Length == 2);
            Assert.That(results[0].ToString() == "A12");
            Assert.That(results[1].ToString() == "B12");
        }

        /// <summary>
        /// A test of how results combine based on the selection mode.
        /// </summary>
        [Test, Category("SelectionMode")]
        public static void TestSelectionModeDistinctDistinct()
        {
            LoadDataFile(_testImages.GetFile(_TEST_DATA_FILE), null);

            string xml = "<Composite SelectionMode='Distinct'>\r\n" +
                        "A\r\n" +
                        "B\r\n" +
                        "</Composite>\r\n" +
                        "<Composite SelectionMode='Distinct'>\r\n" +
                        "1\r\n" +
                        "2\r\n" +
                        "</Composite>";

            DataEntryQuery query = DataEntryQuery.Create(xml, null, null);

            string[] results = query.Evaluate().ToStringArray();

            Assert.That(results.Length == 4);
            Assert.That(results[0].ToString() == "A1");
            Assert.That(results[1].ToString() == "A2");
            Assert.That(results[2].ToString() == "B1");
            Assert.That(results[3].ToString() == "B2");
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