using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UCLID_AFCORELib;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// A configuration to be used to represent order record types.
    /// </summary>
    /// <seealso cref="Extract.DataEntry.LabDE.IFAMDataConfiguration" />
    public class OrderDataConfiguration : IFAMDataConfiguration
    {
        #region Constants

        /// <summary>
        /// The base select query
        /// </summary>
        const string _BASE_SELECT_QUERY =
            "SELECT [OrderNumber] FROM [LabDEOrder]\r\n" +
            "   INNER JOIN[LabDEPatient] p ON[LabDEOrder].[PatientMRN] = p.[MRN] \r\n" +
            "   INNER JOIN [LabDEPatient] ON p.[CurrentMRN] = [LabDEPatient].[MRN]";

        /// <summary>
        /// The display name of the ID field name for orders
        /// </summary>
        const string _ID_FIELD_DISPLAY_NAME = "Order Number";

        /// <summary>
        /// The name of the column representing the order number field in the database.
        /// </summary>
        const string ID_FIELD_DATABASE_NAME = "OrderNumber";

        /// <summary>
        /// The attribute path, relative to the main record attribute, of the attribute that
        /// represents the order number.
        /// </summary>
        const string _DEFAULT_ID_FIELD_ATTRIBUTE_PATH = "OrderNumber";

        /// <summary>
        /// An SQL column specification for the order name.
        /// </summary>
        const string _ORDER_NAME_XPATH =
            "[ORMMessage].value('(/ORM_O01/ORM_O01.ORDER/ORM_O01.ORDER_DETAIL/ORM_O01.OBRRQDRQ1RXOODSODT_SUPPGRP/OBR/OBR.4/CE.2)[1]','NVARCHAR(MAX)')";

        /// <summary>
        /// An SQL column specification for the ordering provider first name using an x-path query
        /// for the XML version of an ORM-O01 HL7 message.
        /// </summary>
        const string _ORDER_PROVIDER_FIRST_NAME_XPATH =
            "[ORMMessage].value('(/ORM_O01/ORM_O01.ORDER/ORM_O01.ORDER_DETAIL/ORM_O01.OBRRQDRQ1RXOODSODT_SUPPGRP/OBR/OBR.16/XCN.2/FN.1)[1]','NVARCHAR(MAX)')";

        /// <summary>
        /// An SQL column specification for the ordering provider last name using an x-path query
        /// for the XML version of an ORM-O01 HL7 message.
        /// </summary>
        const string _ORDER_PROVIDER_LAST_NAME_XPATH =
            "[ORMMessage].value('(/ORM_O01/ORM_O01.ORDER/ORM_O01.ORDER_DETAIL/ORM_O01.OBRRQDRQ1RXOODSODT_SUPPGRP/OBR/OBR.16/XCN.3)[1]','NVARCHAR(MAX)')";

        /// <summary>
        /// The default attribute path for the attribute containing the collection date.
        /// </summary>
        internal const string _DEFAULT_COLLECTION_DATE_ATTRIBUTE_PATH = "CollectionDate";

        /// <summary>
        /// The default attribute path for the attribute containing the collection time.
        /// </summary>
        internal const string _DEFAULT_COLLECTION_TIME_ATTRIBUTE_PATH = "CollectionTime";

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderDataConfiguration"/> class.
        /// </summary>
        public OrderDataConfiguration()
        {
            try
            {
                // Initialize defaults
                IdFieldAttributePath = _DEFAULT_ID_FIELD_ATTRIBUTE_PATH;
                CollectionDateAttributePath = _DEFAULT_COLLECTION_DATE_ATTRIBUTE_PATH;
                CollectionTimeAttributePath = _DEFAULT_COLLECTION_TIME_ATTRIBUTE_PATH;

                RecordMatchCriteria = new List<string>();
                RecordMatchCriteria.Add("[PatientMRN] = {/PatientInfo/MR_Number}");
                RecordMatchCriteria.Add("[OrderCode] = {OrderCode}");
                RecordMatchCriteria.Add("[OrderStatus] = 'A'");
                RecordMatchCriteria.Add("COALESCE([ReferenceDateTime],[ReceivedDateTime]) > DATEADD(MONTH, -3, GETDATE())");

                RecordQueryColumns = new OrderedDictionary();
                RecordQueryColumns.Add(IdFieldDatabaseName, "[LabDEOrder].[OrderNumber]");
                RecordQueryColumns.Add("Order Name", _ORDER_NAME_XPATH);
                RecordQueryColumns.Add("Patient", "[LabDEPatient].[LastName] + ', ' + [LabDEPatient].[FirstName]");
                RecordQueryColumns.Add("Ordered By",
                    "(" + _ORDER_PROVIDER_LAST_NAME_XPATH + " + ', ' + " + _ORDER_PROVIDER_FIRST_NAME_XPATH + ")");
                RecordQueryColumns.Add("Request Date/Time DESC", "[LabDEOrder].[ReferenceDateTime]");
                RecordQueryColumns.Add("Collection Date/Time", "[LabDEOrderFile].[CollectionDate]");

                ColorQueryConditions = new OrderedDictionary();
                ColorQueryConditions.Add("Red", "COUNT(CASE WHEN ([OrderStatus] = 'A') THEN 1 END) = 0");
                ColorQueryConditions.Add("Yellow", "COUNT(CASE WHEN ([OrderStatus] = '*') THEN 1 END) >= " +
                    "COUNT(CASE WHEN ([OrderStatus] = 'A' AND [FileCount] = 0) THEN 1 END)");
                ColorQueryConditions.Add("Lime", "COUNT(CASE WHEN ([OrderStatus] = 'A' AND [FileCount] = 0) THEN 1 END) > 0");
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI41521");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the root clause of an SQL query that selects and joins any relevant tables to be
        /// used to query potentially matching records.
        /// </summary>
        /// <value>
        /// The root clause of an SQL query that is to be used to query potentially matching records.
        /// </value>
        public string BaseSelectQuery
        {
            get
            {
                return _BASE_SELECT_QUERY;
            }
        }

        /// <summary>
        /// Gets the name of the ID field for the record type.
        /// <para><b>Note</b></para>
        /// Must correspond with one of the columns returned by <see cref="GetRecordInfoQuery"/>.
        /// </summary>
        /// <value>
        /// The name of the ID field for the record type.
        /// </value>
        public string IdFieldDisplayName
        {
            get
            {
                return _ID_FIELD_DISPLAY_NAME;
            }
        }

        /// <summary>
        /// Gets the name of the column representing the record number field in the database.
        /// </summary>
        /// <value>
        /// The name of the column representing the record number field in the database.
        /// </value>
        public string IdFieldDatabaseName
        {
            get
            {
                return ID_FIELD_DATABASE_NAME;
            }
        }

        /// <summary>
        /// Gets or set the attribute path, relative to the main record attribute, of the attribute
        /// that represents the record ID.
        /// </summary>
        /// <value>
        /// The attribute path, relative to the main record attribute, of the attribute that
        /// represents the record ID. 
        /// </value>
        public string IdFieldAttributePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the collection date. The
        /// path should either be rooted or be relative to the LabDE Order attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the collection date.
        /// </value>
        public string CollectionDateAttributePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the collection time. The
        /// path should either be rooted or be relative to the LabDE Order attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the collection time.
        /// </value>
        public string CollectionTimeAttributePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an SQL query that selects record IDs based on additional custom criteria
        /// above having basic matches of key fields.
        /// </summary>
        /// <value>
        /// An SQL query that selects record IDs based on additional custom criteria
        /// </value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<string> RecordMatchCriteria
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the definitions of the columns for the record selection grid. The keys are
        /// the column names and the values are  an SQL query part that selects the appropriate data
        /// from the FAM DB table. 
        /// <para><b>Note</b></para>
        /// The IdFieldDatabaseName must be selected as the first column.
        /// </summary>
        /// <value>
        /// The definitions of the columns for the order selection grid.
        /// </value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public OrderedDictionary RecordQueryColumns
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the different possible status colors for the buttons and their SQL query
        /// conditions. The keys are the color name (from <see cref="System.Drawing.Color"/>) while
        /// the values are SQL query expression that evaluates to true if the color should be used
        /// based to indicate available order status. If multiple expressions evaluate to true, the
        /// first of the matching rows will be used.
        /// <para><b>Note</b></para>
        /// The fields available to query against for each order are:
        /// OrderNumber:        The order number.
        /// Available:          1 if the order's status in the DB is Available, 0 if it is cancelled.
        /// FileCount:          The number of files that have been filed against this order.
        /// ReceivedDateTime:   The time the ORM-O01 HL7 message defining the order was received.
        /// ReferenceDateTime:  A configurable date/time extracted from the message (requested
        ///                     date/time is typical).
        /// </summary>
        /// <value>
        /// The different possible status colors for the buttons and their SQL query conditions.
        /// </value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public OrderedDictionary ColorQueryConditions
        {
            get;
            set;
        }

        #endregion Properties

        /// <summary>
        /// Creates a new <see cref="DocumentDataRecord"/>.
        /// </summary>
        /// <param name="famData">The <see cref="FAMData"/> instance that is managing this instance.
        /// </param>
        /// <param name="dataEntryTableRow">The <see cref="DataEntryTableRow"/> representing the
        /// order in the LabDE DEP to which this instance pertains.</param>
        /// <param name="attribute">The <see cref="IAttribute"/> that represents this record.</param>
        /// <returns>
        /// A new <see cref="DocumentDataRecord"/>.
        /// </returns>
        public DocumentDataRecord CreateDocumentDataRecord(
            FAMData famData, DataEntryTableRow dataEntryTableRow, IAttribute attribute)
        {
            try
            {
                return new DocumentDataOrderRecord(famData, dataEntryTableRow, attribute);
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI41517");
            }
        }

        /// <summary>
        /// Generates an SQL query to retrieve data from a FAM DB related to the
        /// <see paramref="selectedRecordNumbers"/>.
        /// <para><b>Note</b></para>
        /// The first column returned by the query must be the record ID. Also, in addition to the
        /// relevant record data, the query must return a "FileID" column that is used to report any
        /// documents that have been filed against the order. Multiple rows may be return per record
        /// for records for which multiple files have been submitted.
        /// </summary>
        /// <param name="recordIds">The record IDs for which info is needed.</param>
        /// <param name="selectViaQuery"><c>true</c> if <see paramref="recordIDs"/> represents an SQL
        /// query that will return the record IDs, <c>false</c> if <see paramref="recordIDs"/>
        /// is a literal comma delimited list of record IDs.</param>
        /// <returns>An SQL query to retrieve data from a FAM DB related to the
        /// <see paramref="selectedRecordNumbers"/>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "1#")]
        public virtual string GetRecordInfoQuery(string recordIds, bool selectViaQuery)
        {
            try
            {
                ExtractException.Assert("ELI38151",
                        "Order query columns have not been properly defined.",
                        RecordQueryColumns.Count > 0 &&
                        RecordQueryColumns[0].ToString().IndexOf(
                            IdFieldDatabaseName, StringComparison.OrdinalIgnoreCase) >= 0);

                // If selectedOrderNumbers is a query, select the results into a table that can be
                // joined with LabDEOrderFile.
                string declarationsClause = "";
                if (selectViaQuery)
                {
                    declarationsClause = "DECLARE @OrderNumbers TABLE ([OrderNumber] NVARCHAR(20)) \r\n" +
                        "INSERT INTO @OrderNumbers\r\n" + recordIds;
                }

                string columnsClause = string.Join(", \r\n",
                    RecordQueryColumns
                        .OfType<DictionaryEntry>()
                        .Select(column => column.Value + " AS [" + column.Key + "]"));

                // Add a query against [LabDEOrderFile].[FileID] behind the scenes here to be able to collect
                // and return correspondingFileIds.
                string query = declarationsClause + "\r\n\r\n" +
                    "SELECT " + columnsClause + "\r\n, [LabDEOrderFile].[FileID]\r\n " +
                    "FROM [LabDEOrder] \r\n" +
                    "INNER JOIN [LabDEPatient] p ON [LabDEOrder].[PatientMRN] = p.[MRN] \r\n" +
                    "INNER JOIN [LabDEPatient] ON p.[CurrentMRN] = [LabDEPatient].[MRN] \r\n" +
                    "FULL JOIN [LabDEOrderFile] ON [LabDEOrderFile].[OrderNumber] = [LabDEOrder].[OrderNumber] \r\n" +
                    (selectViaQuery
                        ? "INNER JOIN @OrderNumbers ON [LabDEOrder].[OrderNumber] = [@OrderNumbers].[OrderNumber]"
                        : "WHERE [LabDEOrder].[OrderNumber] IN (" + recordIds + ")");

                return query;
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI41518");
            }
        }
    }
}
