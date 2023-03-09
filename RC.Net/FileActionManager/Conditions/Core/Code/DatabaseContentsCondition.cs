using Extract.AttributeFinder;
using Extract.Database;
using Extract.DataEntry;
using Extract.FileActionManager.Forms;
using Extract.Interop;
using Extract.Licensing;
using Extract.SqlDatabase;
using Extract.Utilities;
using Extract.Utilities.Parsers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// Represents the way in which the specified search fields will be used to determine
    /// matching files.
    /// </summary>
    [ComVisible(true)]
    [Guid("D426D557-6B58-4247-B045-EA0F722E86C2")]
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum DatabaseContentsConditionSearchModifier
    {
        /// <summary>
        /// If any specified field value matches the returned value for a row, the condition will be
        /// considered as met.
        /// </summary>
        Any = 0,

        /// <summary>
        /// If all specified field values match the returned values for a row, the condition will be
        /// considered as met.
        /// </summary>
        All = 1,

        /// <summary>
        /// If none of the specified field values match the returned values for a row, the condition
        /// will be considered as met.
        /// </summary>
        None = 2
    }

    /// <summary>
    /// Specifies possibilities for qualifying a <see cref="DatabaseContentsCondition"/> as
    /// satisfied based on the number of rows returned.
    /// </summary>
    [ComVisible(true)]
    [Guid("C63B0BCC-6993-4B0B-87A2-E97A524193D3")]
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum DatabaseContentsConditionRowCount
    {
        /// <summary>
        /// The condition is satisfied if zero rows are returned.
        /// </summary>
        Zero = 0,

        /// <summary>
        /// The condition is satisfied if exactly one row is returned.
        /// </summary>
        ExactlyOne = 1,

        /// <summary>
        /// The condition is satisfied if at least one row is returned.
        /// </summary>
        AtLeastOne = 2
    }

    /// <summary>
    /// The possibilities for how the <see cref="DatabaseContentsCondition"/> can deal with errors
    /// evaluating the condition.
    /// </summary>
    [ComVisible(true)]
    [Guid("C46AB6CA-49F8-419B-B499-5B5291DAAD6F")]
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum DatabaseContentsConditionErrorBehavior
    {
        /// <summary>
        /// The error will be ignored and the condition considered not satisfied
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// The error will be logged and the condition considered not satisfied
        /// </summary>
        Log = 1,

        /// <summary>
        /// The error will be logged and processing will be aborted.
        /// </summary>
        Abort = 2
    }

    /// <summary>
    /// A file processing task condition based on database contents.
    /// </summary>
    [ComVisible(true)]
    [Guid("A79E1B0F-BF34-4F90-98B8-CA2D52551342")]
    public interface IDatabaseContentsCondition : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFAMCondition, IPaginationCondition,
        ILicensedComponent,
        IPersistStream
    {
        /// <summary>
        /// Gets or sets whether the database used for the condition should be the current
        /// <see cref="IFileProcessingDB"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the database used for the condition should be the current
        /// <see cref="IFileProcessingDB"/>; <see langword="false"/> if the specified database
        /// should be used.
        /// </value>
        bool UseFAMDBConnection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or set the name of the data source for the database connection if
        /// <see cref="UseFAMDBConnection"/> is <see langword="false"/>.
        /// </summary>
        /// <value>
        /// The name of the data source for the database connection/
        /// </value>
        string DataSourceName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or set the name of the data provider for the database connection if
        /// <see cref="UseFAMDBConnection"/> is <see langword="false"/>.
        /// </summary>
        /// <value>
        /// The name of the data provider for the database connection.
        /// </value>
        string DataProviderName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the database connection string to be used for the database connection if
        /// <see cref="UseFAMDBConnection"/> is <see langword="false"/>.
        /// </summary>
        /// <value>
        /// The database connection string to be used for the database connection.
        /// </value>
        string DataConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the condition will be evaluated using a query.
        /// </summary>
        /// <value><see langword="true"/> if the condition will be evaluated using a query;
        /// <see langword="false"/> if will be evaluated using a table.</value>
        bool UseQuery
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the query to evaluate.
        /// </summary>
        /// <value>
        /// The query to evaluate.
        /// </value>
        string Query
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of table to evaluate.
        /// </summary>
        /// <value>
        /// The name of table to evaluate.
        /// </value>
        string Table
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the table or query results should be checked against specified
        /// field values.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the table or query results should be checked against specified
        /// field values; <see langword="false"/> if results should be considered as met as long
        /// as at least one row is returned.
        /// </value>
        bool CheckFields
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseContentsConditionRowCount"/> specifying the number
        /// of rows that constitute a satisfied condition.
        /// </summary>
        /// <value>
        /// The <see cref="DatabaseContentsConditionRowCount"/> specifying the number of rows that
        /// constitute a satisfied condition.
        /// </value>
        DatabaseContentsConditionRowCount RowCountCondition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseContentsConditionSearchModifier"/> specifying the
        /// way in which the specified search fields will be used to determine matching files.
        /// </summary>
        /// <value>
        /// The <see cref="DatabaseContentsConditionSearchModifier"/> specifying the way in which
        /// the specified search fields will be used to determine matching files.
        /// </value>
        DatabaseContentsConditionSearchModifier SearchModifier
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the values the fields returned from the query should be compared to in
        /// order to determine if the returned row satisfies the condition.
        /// </summary>
        /// <value>
        /// The values the fields returned from the query should be compared to in order to
        /// determine if the returned row satisfies the condition.
        /// </value>
        IUnknownVector SearchFields
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the VOA file containing any attribute values needed by the
        /// query.
        /// </summary>
        /// <value>
        /// The name of the VOA file containing any attribute values needed by the
        /// query.
        /// </value>
        string DataFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets how the to deal with errors evaluating the condition.
        /// </summary>
        /// <value>
        /// The <see cref="DatabaseContentsConditionErrorBehavior"/> indicating how to deal with
        /// errors evaluating the condition.
        /// </value>
        DatabaseContentsConditionErrorBehavior ErrorBehavior
        {
            get;
            set;
        }
    }

    /// <summary>
    /// A <see cref="IFAMCondition"/> based on the attributes in a VOA data file.
    /// </summary>
    [ComVisible(true)]
    [Guid("92C31019-CD7F-4ECC-AF1C-215D3FD3699D")]
    [ProgId("Extract.FileActionManager.Conditions.DatabaseContentsCondition")]
    public class DatabaseContentsCondition : IDatabaseContentsCondition, IDisposable
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Database contents condition";

        /// <summary>
        /// Current task version.
        /// Version 2: 
        /// Updated tag names: ActionName -> DatabaseAction and DatabaseServerName -> DatabaseServer
        /// </summary>
        internal const int _CURRENT_VERSION = 2;

        #endregion Constants

        #region FieldDefinition

        /// <summary>
        /// Defines a field returned from a query and allows comparison of the field for a given
        /// row returned.
        /// </summary>
        class FieldDefinition
        {
            /// <summary>
            /// The <see cref="DatabaseContentsCondition"/> this instance is being used by.
            /// </summary>
            DatabaseContentsCondition _databaseContentsCondition;

            /// <summary>
            /// The name of the field (may be a digit to indicate column ordinal).
            /// </summary>
            string _name;

            /// <summary>
            /// If <see cref="_name"/> is a digit representing column ordinal, this is the ordinal.
            /// </summary>
            int _ordinal;

            /// <summary>
            /// The specified (unexpanded) value to compare against in query result rows.
            /// </summary>
            string _value;

            /// <summary>
            /// <see cref="_value"/> converted to the type of the field.
            /// </summary>
            object _convertedValue;

            /// <summary>
            /// When a fuzzy comparison is to be performed, the <see cref="DotNetRegexParser"/> that
            /// will be used to perform the comparison.
            /// </summary>
            DotNetRegexParser _valueRegex;

            /// <summary>
            /// Indicates whether string or fuzzy comparisons should be sensitive to casing.
            /// </summary>
            bool _caseSensitive;

            /// <summary>
            /// Indicates that the comparison should be done as as a string comparison (regardless
            /// of field type) and that a certain number of different characters should be tolerated
            /// based on the overall length of <see cref="_value"/>.
            /// </summary>
            bool _fuzzy;

            /// <summary>
            /// Indicates whether the comparison should be checking for null rather than a value.
            /// </summary>
            bool _matchNull;

            /// <summary>
            /// Initializes a new instance of the <see cref="FieldDefinition"/> class.
            /// </summary>
            /// <param name="databaseContentsCondition">he <see cref="DatabaseContentsCondition"/>
            /// this instance is being used by.</param>
            /// <param name="fieldDataVector">An <see cref="IVariantVector"/> representing the field
            /// configuration: Name, Value, Case-sensitive, and Fuzzy</param>
            public FieldDefinition(DatabaseContentsCondition databaseContentsCondition, 
                IVariantVector fieldDataVector)
            {
                try
                {
                    _databaseContentsCondition = databaseContentsCondition;

                    _name = (string)fieldDataVector[0];
                    if (!Int32.TryParse(_name, out _ordinal) || _ordinal <= 0)
                    {
                        _ordinal = -1;
                    }
                    _value = (string)fieldDataVector[1];
                    _caseSensitive = (bool)fieldDataVector[2];
                    _fuzzy = (bool)fieldDataVector[3];
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36926");
                }
            }

            /// <summary>
            /// Uses the run-time parameters to initialize this instance to be compared against rows
            /// in <see paramref="queryResults"/>.
            /// </summary>
            /// <param name="queryResults">A <see cref="DataTable"/> representing the query results
            /// to be compared.</param>
            /// <param name="fileRecord">A <see cref="FileRecord"/> representing the file for which
            /// the condition is being executed.</param>
            /// <param name="pathTags">A <see cref="FileActionManagerPathTags"/> instance to use to
            /// expand path tags and functions in the specified value.</param>
            /// <param name="fileProcessingDB">The active <see cref="IFileProcessingDB"/> instance.
            /// </param>
            public void PrepareValue(DataTable queryResults, FileRecord fileRecord,
                FileActionManagerPathTags pathTags, IFileProcessingDB fileProcessingDB)
            {
                try
                {
                    // Treat "NULL" as a special case.
                    if (_value.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                    {
                        _matchNull = true;
                    }
                    else
                    {
                        // Get the column to be compared from the query results based on name or
                        // ordinal.
                        DataColumn column = (_ordinal > 0)
                            ? queryResults.Columns[_ordinal - 1]  // _ordinal is 1-based.
                            : queryResults.Columns[_name];

                        ExtractException.Assert("ELI36927", "Column not found in table/query output",
                            column != null, "Column name", _name);

                        // Expand any attributes, queries, path tags or functions in the specified
                        // value.
                        string expandedValue = _databaseContentsCondition.UseFAMDBConnection
                            ? _databaseContentsCondition.ExpandText(
                                _value, fileRecord, pathTags, null, fileProcessingDB)
                            : _databaseContentsCondition.ExpandText(
                                _value, fileRecord, pathTags, _databaseContentsCondition.DatabaseConnectionInfo, null);

                        if (_fuzzy)
                        {
                            // https://extract.atlassian.net/browse/ISSUE-12321
                            // The value to be searched needs to be escaped for regex, otherwise the
                            // value will be treated as a regex. The expansion should happen after
                            // determining the error allowance so that escape chars aren't factored
                            // into the error allowance.
                            expandedValue = Regex.Escape(expandedValue);

                            _valueRegex = new DotNetRegexParser();
                            _valueRegex.IgnoreCase = !_caseSensitive;

                            if (expandedValue.Length <= 1)
                            {
                                // https://extract.atlassian.net/browse/ISSUE-12463
                                // If the value to be compared is 1 char or less in length, the
                                // fuzzy syntax will not work and would otherwise be inappropriate
                                // to use. Just use a literal expression in this case (but don't
                                // drop into the logic that would use type conversion as we don't
                                // want an entirely different type of comparison occuring depending
                                // on the specific data at hand).
                                _valueRegex.Pattern = @"\A" + expandedValue + @"\z";
                            }
                            else
                            {
                                // Intialize _valueRegex as with a fuzzy regex pattern that allows one
                                // wrong char per 3 chars in the search term and 1 extra whitespace
                                // char per 2 chars in the search term.
                                int errorAllowance = expandedValue.Length / 3;
                                int whiteSpaceAllowance = expandedValue.Length / 2;

                                _valueRegex.Pattern = string.Format(CultureInfo.InvariantCulture,
                                    @"\A(?~<error={0:D},xtra_ws={1:D}>{2})\z",
                                    errorAllowance, whiteSpaceAllowance, expandedValue);
                            }
                        }
                        else
                        {
                            // If not using a fuzzy comparison, create an object of the type
                            // associated with the field. This allows, for instance, that
                            // a date represented as "January 1 2001" to be successfully compared
                            // with a value of "1/1/2001" in a DateTime column.
                            _convertedValue = expandedValue.ConvertToType(column.DataType);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36928");
                }
            }

            /// <summary>
            /// Returns whether the specified <see paramref="row"/> is a match based on this field's
            /// value.
            /// </summary>
            /// <param name="row">The <see cref="DataRow"/> to be compared.</param>
            /// <returns><see langword="true"/> if the row is a match based on this field;
            /// otherwise, <see langword="false"/>.</returns>
            public bool RowIsMatch(DataRow row)
            {
                try
                {
                    // Get the value for this field from the supplied row.
                    object fieldValue = (_ordinal > 0)
                        ? row[_ordinal - 1]  // _ordinal is 1-based.
                        : row[_name];

                    if (_matchNull)
                    {
                        // NULL DB values won't result in fieldValue == null, but with an object of
                        // type DBNull.
                        return fieldValue.GetType() == typeof(DBNull);
                    }
                    else if (fieldValue == null)
                    {
                        // If not checking for null, not a match if the field has a value.
                        return false;
                    }
                    else if (_fuzzy)
                    {
                        // If using a fuzzy match, compare against the field value as a string.
                        return _valueRegex.Regex.IsMatch(fieldValue.ToString());
                    }
                    else if (_convertedValue.GetType() == typeof(string) || _caseSensitive)
                    {
                        // If using a case-sensitive search, compare against the field value as a
                        // string.
                        return ((string)_convertedValue).Equals(fieldValue.ToString(),
                            _caseSensitive
                                ? StringComparison.Ordinal
                                : StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        // Otherwise use a type-specific comparison
                        // (e.g. '1/1/2001' == 'Jan 1, 2001', '1' == 'True')
                        return _convertedValue.Equals(fieldValue);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36929");
                }
            }
        }

        #endregion FieldDefinition

        #region Fields

        /// <summary>
        /// Regex that parses text to find "matches" where each match is a section of the source
        /// text that alternates between recognized data entry queries and literal text. The sum of
        /// all matches = the original source text.
        /// </summary>
        static readonly Regex _queryParserRegex =
            new Regex(@"((?!<Query>[\s\S]+?</Query>)[\S\s])+|<Query>[\s\S]+?</Query>",
                RegexOptions.Compiled);

        /// <summary>
        /// Regex that finds all shorthand attribute queries in text.
        /// </summary>
        static readonly Regex _attributeQueryFinderRegex =
            new Regex(@"</[\s\S]+?>", RegexOptions.Compiled);

        /// <summary>
        /// Indicates whether the database used for the condition should be the current
        /// <see cref="IFileProcessingDB"/>.
        /// </summary>
        bool _useFAMDBConnection = true;

        /// <summary>
        /// The <see cref="DatabaseConnectionInfo"/> describing the data source to be used for SQL
        /// query elements.
        /// </summary>
        DatabaseConnectionInfo _databaseConnectionInfo = new DatabaseConnectionInfo();

        /// <summary>
        /// Indicates whether the condition will be evaluated using a query.
        /// </summary>
        bool _useQuery;

        /// <summary>
        /// The query to evaluate.
        /// </summary>
        string _query;

        /// <summary>
        /// The name of table to evaluate.
        /// </summary>
        string _table;

        /// <summary>
        /// Indicates whether the table or query results should be checked against specified field
        /// values.
        /// </summary>
        bool _checkFields;

        /// <summary>
        /// Indicates the number of rows that constitute a satisfied condition.
        /// </summary>
        DatabaseContentsConditionRowCount _rowCountCondition =
            DatabaseContentsConditionRowCount.AtLeastOne;

        /// <summary>
        /// The <see cref="DatabaseContentsConditionSearchModifier"/> specifying the way in which
        /// the specified search fields will be used to determine matching files.
        /// </summary>
        DatabaseContentsConditionSearchModifier _searchModifier =
            DatabaseContentsConditionSearchModifier.All;

        /// <summary>
        /// Specifies the values the fields returned from the query should be compared to in order
        /// to determine if the returned row satisfies the condition.
        /// </summary>
        IUnknownVector _searchFields = new IUnknownVector();

        /// <summary>
        /// Indicates the name of the VOA file containing any attribute values needed by the query.
        /// </summary>
        string _dataFileName = "<SourceDocName>.voa";

        /// <summary>
        /// Indicates how the to deal with errors evaluating the condition.
        /// </summary>
        DatabaseContentsConditionErrorBehavior _errorBehavior =
            DatabaseContentsConditionErrorBehavior.Abort;

        /// <summary>
        /// <see langword="true"/> if changes have been made to
        /// <see cref="DatabaseContentsCondition"/> since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Indicates whether data to run data entry queries has been initialized for the current
        /// file.
        /// </summary>
        bool _queryDataInitialized;

        /// <summary>
        /// Indicates whether the VOA file was loaded.
        /// </summary>
        bool _dataFileLoaded;

        /// <summary>
        /// The <see cref="DbConnection"/> to use to resolve data queries.
        /// </summary>
        DbConnection _dbConnection;

        /// <summary>
        /// The fields to be compared in the rows returned from the query.
        /// </summary>
        List<FieldDefinition> _fieldDefinitions;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContentsCondition"/> class.
        /// </summary>
        public DatabaseContentsCondition()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36930");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContentsCondition"/> class as a copy
        /// of the specified <see paramref="task"/>.
        /// </summary>
        /// <param name="task">The <see cref="DatabaseContentsCondition"/> from which settings should be
        /// copied.</param>
        public DatabaseContentsCondition(DatabaseContentsCondition task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36931");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets whether the database used for the condition should be the current
        /// <see cref="IFileProcessingDB"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the  database used for the condition should be the current
        /// <see cref="IFileProcessingDB"/>; <see langword="false"/> if the specified database
        /// should be used.
        /// </value>
        public bool UseFAMDBConnection
        {
            get
            {
                return _useFAMDBConnection;
            }

            set
            {
                try
                {
                    if (value != _useFAMDBConnection)
                    {
                        _useFAMDBConnection = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI36932", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or set the name of the data source for the database connection if
        /// <see cref="UseFAMDBConnection"/> is <see langword="false"/>.
        /// </summary>
        /// <value>
        /// The name of the data source for the database connection.
        /// </value>
        public string DataSourceName
        {
            get
            {
                return _databaseConnectionInfo.DataSourceName;
            }

            set
            {
                try
                {
                    if (value != _databaseConnectionInfo.DataSourceName)
                    {
                        _databaseConnectionInfo.DataSourceName = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI36933", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or set the name of the data provider for the database connection if
        /// <see cref="UseFAMDBConnection"/> is <see langword="false"/>.
        /// </summary>
        /// <value>
        /// The name of the data provider for the database connection.
        /// </value>
        public string DataProviderName
        {
            get
            {
                return _databaseConnectionInfo.DataProviderName;
            }

            set
            {
                try
                {
                    if (value != _databaseConnectionInfo.DataProviderName)
                    {
                        _databaseConnectionInfo.DataProviderName = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI36934", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the database connection string to be used for the database connection if
        /// <see cref="UseFAMDBConnection"/> is <see langword="false"/>.
        /// </summary>
        /// <value>
        /// The database connection string to be used for the database connection.
        /// </value>
        public string DataConnectionString
        {
            get
            {
                return _databaseConnectionInfo.ConnectionString;
            }

            set
            {
                try
                {
                    if (value != _databaseConnectionInfo.ConnectionString)
                    {
                        _databaseConnectionInfo.ConnectionString = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI36935", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseConnectionInfo"/> describing the data source to be
        /// used for SQL query elements.
        /// </summary>
        /// <value>
        /// The <see cref="DatabaseConnectionInfo"/> describing the data source to be used.
        /// </value>
        internal DatabaseConnectionInfo DatabaseConnectionInfo
        {
            get
            {
                return _databaseConnectionInfo;
            }

            set
            {
                _databaseConnectionInfo = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the condition will be evaluated using a query.
        /// </summary>
        /// <value><see langword="true"/> if the condition will be evaluated using a query;
        /// <see langword="false"/> if will be evaluated using a table.</value>
        public bool UseQuery
        {
            get
            {
                return _useQuery;
            }

            set
            {
                try
                {
                    if (value != _useQuery)
                    {
                        _useQuery = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI36936", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the query to evaluate.
        /// </summary>
        /// <value>
        /// The query to evaluate.
        /// </value>
        public string Query
        {
            get
            {
                return _query;
            }

            set
            {
                try
                {
                    if (value != _query)
                    {
                        _query = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI36937", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of table to evaluate.
        /// </summary>
        /// <value>
        /// The name of table to evaluate.
        /// </value>
        public string Table
        {
            get
            {
                return _table;
            }

            set
            {
                try
                {
                    if (value != _table)
                    {
                        _table = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI36938", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the table or query results should be checked against specified
        /// field values.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the table or query results should be checked against specified
        /// field values; <see langword="false"/> if results should be considered as met as long
        /// as at least one row is returned from the query regardless of the field values.
        /// </value>
        public bool CheckFields
        {
            get
            {
                return _checkFields;
            }

            set
            {
                try
                {
                    if (value != _checkFields)
                    {
                        _checkFields = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI36939", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseContentsConditionRowCount"/> specifying the number
        /// of rows that constitute a satisfied condition.
        /// </summary>
        /// <value>
        /// The <see cref="DatabaseContentsConditionRowCount"/> specifying the number of rows that
        /// constitute a satisfied condition.
        /// </value>
        public DatabaseContentsConditionRowCount RowCountCondition
        {
            get
            {
                return _rowCountCondition;
            }

            set
            {
                try
                {
                    if (value != _rowCountCondition)
                    {
                        _rowCountCondition = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI36940", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseContentsConditionSearchModifier"/> specifying the
        /// way in which the specified search fields will be used to determine matching files.
        /// </summary>
        /// <value>
        /// The <see cref="DatabaseContentsConditionSearchModifier"/> specifying the way in which
        /// the specified search fields will be used to determine matching files.
        /// </value>
        public DatabaseContentsConditionSearchModifier SearchModifier
        {
            get
            {
                return _searchModifier;
            }

            set
            {
                try
                {
                    if (value != _searchModifier)
                    {
                        _searchModifier = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI36941", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the values the fields returned from the query should be compared to in
        /// order to determine if the returned row satisfies the condition.
        /// </summary>
        /// <value>
        /// The values the fields returned from the query should be compared to in order to
        /// determine if the returned row satisfies the condition.
        /// </value>
        public IUnknownVector SearchFields
        {
            get
            {
                return _searchFields;
            }

            set
            {
                try
                {
                    if (value != _searchFields)
                    {
                        _searchFields = value ?? new IUnknownVector();
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI36942", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the VOA file containing any attribute values needed by the
        /// query.
        /// </summary>
        /// <value>
        /// The name of the VOA file containing any attribute values needed by the
        /// query.
        /// </value>
        public string DataFileName
        {
            get
            {
                return _dataFileName;
            }

            set
            {
                try
                {
                    if (value != _dataFileName)
                    {
                        _dataFileName = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI36943", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets how the to deal with errors evaluating the condition.
        /// </summary>
        /// <value>
        /// The <see cref="DatabaseContentsConditionErrorBehavior"/> indicating how to deal with
        /// errors evaluating the condition.
        /// </value>
        public DatabaseContentsConditionErrorBehavior ErrorBehavior
        {
            get
            {
                return _errorBehavior;
            }

            set
            {
                try
                {
                    if (value != _errorBehavior)
                    {
                        _errorBehavior = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI36944", ex.Message);
                }
            }
        }

        #endregion Properties

        #region IFAMCondition Members

        /// <summary>
        /// Evaluates the <see cref="Query"/> to determine whether the condition is met.
        /// </summary>
        /// <param name="pFileRecord">A <see cref="FileRecord"/> specifing the file to be tested.
        /// </param>
        /// <param name="pFPDB">The <see cref="FileProcessingDB"/> currently in use.</param>
        /// <param name="lActionID">The ID of the database action in use.</param>
        /// <param name="pFAMTagManager">A <see cref="FAMTagManager"/> to be used to evaluate any
        /// FAM tags used by the condition.</param>
        /// <returns><see langword="true"/> if the condition was met, <see langword="false"/> if it
        /// was not.</returns>
        public bool FileMatchesFAMCondition(FileRecord pFileRecord, FileProcessingDB pFPDB,
            int lActionID, FAMTagManager pFAMTagManager)
        {
            try
            {
                return FileMatchesCondition(pFileRecord, pFPDB, pFAMTagManager);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47079", "Error occured in '" + _COMPONENT_DESCRIPTION + "'");
            }
        }

        /// <summary>
        /// Returns bool value indicating if the condition requires admin access
        /// </summary>
        /// <returns><see langword="true"/> if the condition requires admin access
        /// <see langword="false"/> if condition does not require admin access</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion IFAMCondition Members

        #region IPaginationCondition Members

        /// <summary>
        /// Used to allow PaginationTask to inform IPaginationCondition implementers when they are
        /// being used in the context of the IPaginationCondition interface.
        /// NOTE: While it is not necessary for implementers to persist this setting, this setting
        /// does need to be copied in the context of the ICopyableObject interface (CopyFrom)
        /// </summary>
        public bool IsPaginationCondition { get; set; }

        /// <summary>
        /// Tests proposed pagination output <see paramref="pFileRecord"/> to determine if it is
        /// qualified to be automatically generated.
        /// </summary>
        /// <param name="pSourceFileRecord">A <see cref="FileRecord"/> specifing the source document
        /// for the proposed output document tested here.
        /// </param>
        /// <param name="bstrProposedFileName">The filename planned to be assigned to the document.</param>
        /// <param name="bstrDocumentStatus">A json representation of the pagination DocumentStatus
        /// for the proposed output document.</param>
        /// <param name="bstrSerializedDocumentAttributes">A searialized copy of all attributes that fall
        /// under a root-level "Document" attribute including Pages, DeletedPages and DocumentData (the
        /// content of which would become the output document's voa)</param>
        /// <param name="pFPDB">The <see cref="FileProcessingDB"/> currently in use.</param>
        /// <param name="lActionID">The ID of the database action in use.</param>
        /// <param name="pFAMTagManager">A <see cref="FAMTagManager"/> to be used to evaluate any
        /// FAM tags used by the condition.</param>
        /// <returns><see langword="true"/> if the condition was met, <see langword="false"/> if it
        /// was not.</returns>
        public bool FileMatchesPaginationCondition(FileRecord pSourceFileRecord, string bstrProposedFileName,
            string bstrDocumentStatus, string bstrSerializedDocumentAttributes,
            FileProcessingDB pFPDB, int lActionID, FAMTagManager pFAMTagManager)
        {
            try
            {
                return FileMatchesCondition(pSourceFileRecord, pFPDB, pFAMTagManager);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI47080", "Error occured in '" + _COMPONENT_DESCRIPTION + "'");
            }
        }

        #endregion IPaginationCondition Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="DatabaseContentsCondition"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI36947",
                    _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                DatabaseContentsCondition cloneOfThis = (DatabaseContentsCondition)Clone();

                using (DatabaseContentsConditionSettingsDialog dlg
                    = new DatabaseContentsConditionSettingsDialog(cloneOfThis))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dlg.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI36948", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object has been configured and <see langword="false"/>
        /// otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                // DatabaseContentsConditionSettingsDialog will provide more detailed validation
                // of the settings. This call only ensures the basic settings are in place.
                if (!UseFAMDBConnection &&
                    (DatabaseConnectionInfo.DataProvider == null ||
                    string.IsNullOrWhiteSpace(DatabaseConnectionInfo.ConnectionString)))
                {
                    return false;
                }

                if ((UseQuery && string.IsNullOrWhiteSpace(Query)) ||
                    (!UseQuery && string.IsNullOrWhiteSpace(Table)))
                {
                    return false;
                }

                if (CheckFields && SearchFields.Size() == 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI36949",
                    "Failed to check '" + _COMPONENT_DESCRIPTION + "' configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="DatabaseContentsCondition"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="DatabaseContentsCondition"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new DatabaseContentsCondition(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI36950",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="DatabaseContentsCondition"/> instance into
        /// this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as DatabaseContentsCondition;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to DatabaseContentsCondition");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI36951",
                    "Failed to copy '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        #endregion ICopyableObject Members

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        /// <returns>The name of the COM object.</returns>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/>
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(LicenseIdName.ExtractCoreObjects);
        }

        #endregion ILicensedComponent Members

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        /// <summary>
        /// Checks the object for changes since it was last saved.
        /// </summary>
        /// <returns>
        ///   <see cref="HResult.Ok"/> if changes have been made;
        /// <see cref="HResult.False"/> if changes have not been made.
        /// </returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the object should be loaded.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                _fieldDefinitions = null;

                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    UseFAMDBConnection = reader.ReadBoolean();
                    DataSourceName = reader.ReadString();
                    DataProviderName = reader.ReadString();
                    DataConnectionString = reader.ReadString();
                    DataFileName = reader.ReadString();
                    UseQuery = reader.ReadBoolean();
                    Query = reader.ReadString();
                    Table = reader.ReadString();
                    CheckFields = reader.ReadBoolean();
                    RowCountCondition = (DatabaseContentsConditionRowCount)reader.ReadInt32();
                    SearchModifier = (DatabaseContentsConditionSearchModifier)reader.ReadInt32();
                    SearchFields = (IUnknownVector)reader.ReadIPersistStream();
                    ErrorBehavior = (DatabaseContentsConditionErrorBehavior)reader.ReadInt32();

                    if (reader.Version < 2)
                    {
                        // Update tag name of <DatabaseServerName> to <DatabaseServer>
                        DataConnectionString = DataConnectionString.Replace("<DatabaseServerName>",
                            FileActionManagerPathTags.DatabaseServerTag);
                        Query = Query.Replace("<DatabaseServerName>",
                            FileActionManagerPathTags.DatabaseServerTag);
                        for (int i = 0; i < SearchFields.Size(); i++)
                        {
                            var fieldConfiguration = (VariantVector)SearchFields.At(i);
                            string fieldValue = (string)fieldConfiguration[1];
                            fieldValue = fieldValue.Replace("<DatabaseServerName>",
                                FileActionManagerPathTags.DatabaseServerTag);
                            fieldConfiguration.Set(1, fieldValue);
                        }
                    }
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI36952",
                    "Failed to load '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Saves the instance into the specified IStream and clears the dirty flag (if specified).
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.</param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(UseFAMDBConnection);
                    writer.Write(DataSourceName);
                    writer.Write(DataProviderName);
                    writer.Write(DataConnectionString);
                    writer.Write(DataFileName);
                    writer.Write(UseQuery);
                    writer.Write(Query);
                    writer.Write(Table);
                    writer.Write(CheckFields);
                    writer.Write((int)RowCountCondition);
                    writer.Write((int)SearchModifier);
                    writer.Write((IPersistStream)SearchFields, clearDirty);
                    writer.Write((int)ErrorBehavior);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI36953",
                    "Failed to save '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            throw new NotImplementedException();
        }

        #endregion IPersistStream Members

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="DatabaseContentsCondition"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="DatabaseContentsCondition"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="DatabaseContentsCondition"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_dbConnection is DbConnection conn)
                {
                    conn.Dispose();
                }
                _dbConnection = null;

                if (_databaseConnectionInfo != null)
                {
                    _databaseConnectionInfo.Dispose();
                    _databaseConnectionInfo = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "Extract FAM Conditions" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.FileActionManagerConditionsGuid);
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.PaginationConditionsGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "Extract FAM Conditions" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.FileActionManagerConditionsGuid);
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.PaginationConditionsGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="DatabaseContentsCondition"/> instance into this one.
        /// </summary>
        /// <param name="source">The <see cref="DatabaseContentsCondition"/> from which to copy.
        /// </param>
        void CopyFrom(DatabaseContentsCondition source)
        {
            _fieldDefinitions = null;

            UseFAMDBConnection = source.UseFAMDBConnection;
            DataSourceName = source.DataSourceName;
            DataProviderName = source.DataProviderName;
            DataConnectionString = source.DataConnectionString;
            DataFileName = source.DataFileName;
            UseQuery = source.UseQuery;
            Query = source.Query;
            Table = source.Table;
            CheckFields = source.CheckFields;
            RowCountCondition = source.RowCountCondition;
            SearchModifier = source.SearchModifier;
            if (source.SearchFields != null)
            {
                ICopyableObject copyFrom = source.SearchFields;
                SearchFields = (IUnknownVector)copyFrom.Clone();
            }
            else
            {
                SearchFields = new IUnknownVector();
            }
            ErrorBehavior = source.ErrorBehavior;
            IsPaginationCondition = source.IsPaginationCondition;
        }

        /// <summary>
        /// Gets the <see cref="FieldDefinition"/>s for the fields whose values are to be compared.
        /// </summary>
        IEnumerable<FieldDefinition> FieldDefinitions
        {
            get
            {
                if (_fieldDefinitions == null)
                {
                    _fieldDefinitions = new List<FieldDefinition>();

                    foreach (IVariantVector fieldVector in
                        SearchFields.ToIEnumerable<IVariantVector>())
                    {
                        _fieldDefinitions.Add(new FieldDefinition(this, fieldVector));
                    }
                }

                return _fieldDefinitions;
            }
        }

        /// <summary>
        /// Tests whether the configured condition is met for the specified <paramref name="fileRecord"/>.
        /// </summary>
        bool FileMatchesCondition(FileRecord fileRecord, FileProcessingDB fileProcessingDB, FAMTagManager famTagManager)
        {
            DataTable queryResults = null;

            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI36945",
                    _COMPONENT_DESCRIPTION);

                // Resets values relating to caching for DataEntryQueries that may be used in the
                // query.
                _queryDataInitialized = false;
                _dataFileLoaded = false;

                // Create the pathTags instance to be used to expand any path tags/functions.
                FileActionManagerPathTags pathTags =
                    new FileActionManagerPathTags(famTagManager, fileRecord.Name);
                pathTags.AlwaysShowDatabaseTags = true;

                queryResults = GetTableOrQueryResults(fileRecord, pathTags, fileProcessingDB);

                if (CheckFields)
                {
                    return CheckQueryResultFields(queryResults, fileRecord, pathTags, fileProcessingDB);
                }
                else
                {
                    // If not checking field values, the condition's result depends only upon
                    // queryResults.Rows.Count. 
                    switch (RowCountCondition)
                    {
                        case DatabaseContentsConditionRowCount.Zero:
                            return queryResults.Rows.Count == 0;

                        case DatabaseContentsConditionRowCount.AtLeastOne:
                            return queryResults.Rows.Count > 0;

                        case DatabaseContentsConditionRowCount.ExactlyOne:
                            return queryResults.Rows.Count == 1;

                        default:
                            throw new ExtractException("ELI37085", "Internal logic error.");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ErrorBehavior == DatabaseContentsConditionErrorBehavior.Ignore)
                {
                    return false;
                }
                else
                {
                    var ee = ExtractException.CreateComVisible("ELI36946",
                        "Error occurred in '" + _COMPONENT_DESCRIPTION + "'", ex);
                    if (ErrorBehavior == DatabaseContentsConditionErrorBehavior.Log)
                    {
                        ee.Log();
                        return false;
                    }

                    // DatabaseContentsConditionErrorBehavior.Abort
                    throw ee;
                }
            }
            finally
            {
                if (queryResults != null)
                {
                    queryResults.Dispose();
                }

                // https://extract.atlassian.net/browse/ISSUE-12198
                // For the time being, use new connections for every file to avoid the risk of
                // keeping connections open indefinitely since there is no way for the condition to
                // know if processing is stopped.
                _databaseConnectionInfo.CloseManagedDbConnection();
                if (_dbConnection != null)
                {
                    _dbConnection?.Dispose();
                    _dbConnection = null;
                }
            }
        }

        /// <summary>
        /// Expands all path tags/functions and data queries in the specified <see paramref="text"/>.
        /// <para><b>Note</b></para>
        /// Either <see paramref="databaseConnectionInfo"/> or <see paramref="fileProcessingDB"/>
        /// must be provided depending upon which is to be used as the source of the database to use
        /// for expanding the text, but not both.
        /// This expansion supports shorthand attribute queries in the form &lt;/AttributeName&gt;
        /// </summary>
        /// <param name="text">The text to be expanded.</param>
        /// <param name="fileRecord">The <see cref="FileRecord"/> relating to the text to be
        /// expanded.</param>
        /// <param name="pathTags">The <see cref="FileActionManagerPathTags"/> instance to use to
        /// expand path tags and functions in the <see paramref="text"/>.</param>
        /// <param name="databaseConnnectionInfo">A <see cref="DatabaseConnectionInfo"/> instance
        /// defining the database to use for expansion. Must be <see langworkd="null"/> if
        /// <see paramref="fileProcessingDB"/> is specified.</param>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> to use for expansion.
        /// Must be <see langworkd="null"/> if <see paramref="databaseConnnectionInfo"/> is
        /// specified.</param>
        /// <returns>The expanded <see paramref="text"/> as the one an only entry in the array.
        /// </returns>
        string ExpandText(string text, FileRecord fileRecord, FileActionManagerPathTags pathTags,
            DatabaseConnectionInfo databaseConnnectionInfo, IFileProcessingDB fileProcessingDB)
        {
            bool temp;
            string[] expandedText =
                ExpandText(text, fileRecord, pathTags, databaseConnnectionInfo, fileProcessingDB,
                    null, out temp);
            return (expandedText.Length == 0) ? "" : expandedText.FirstOrDefault();
        }

        /// <summary>
        /// Expands all path tags/functions and data queries in the specified <see paramref="text"/>.
        /// <para><b>Note</b></para>
        /// Either <see paramref="databaseConnectionInfo"/> or <see paramref="fileProcessingDB"/>
        /// must be provided depending upon which is to be used as the source of the database to use
        /// for expanding the text, but not both.
        /// This expansion supports shorthand attribute queries in the form &lt;/AttributeName&gt;
        /// </summary>
        /// <param name="text">The text to be expanded.</param>
        /// <param name="fileRecord">The <see cref="FileRecord"/> relating to the text to be
        /// expanded.</param>
        /// <param name="pathTags">The <see cref="FileActionManagerPathTags"/> instance to use to
        /// expand path tags and functions in the <see paramref="text"/>.</param>
        /// <param name="databaseConnectionInfo">A <see cref="DatabaseConnectionInfo"/> instance
        /// defining the database to use for expansion. Must be <see langworkd="null"/> if
        /// <see paramref="fileProcessingDB"/> is specified.</param>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> to use for expansion.
        /// Must be <see langworkd="null"/> if <see paramref="databaseConnectionInfo"/> is
        /// specified.</param>
        /// <param name="failedQueryReplacement">Text that should be inserted in place of queries
        /// that cannot be expanded or <see langword="null"/> to throw an exception when queries
        /// cannot be expanded.</param>
        /// <param name="isDataEntryQueryOnly">Returns <see langword="true"/> if the entire
        /// <see paramref="text"/> was a DataEntryQuery; otherwise, <see langword="false"/>.</param>
        /// <returns>If <see paramref="isDataEntryQueryOnly"/> is <see langword="true"/>, the
        /// results of the query as a string array; if <see paramref="isDataEntryQueryOnly"/> is
        /// <see langword="false"/>, the expanded <see paramref="text"/> as the one an only entry in
        /// the array.</returns>
        internal string[] ExpandText(string text, FileRecord fileRecord, FileActionManagerPathTags pathTags,
            DatabaseConnectionInfo databaseConnectionInfo, IFileProcessingDB fileProcessingDB,
            string failedQueryReplacement, out bool isDataEntryQueryOnly)
        {
            try
            {
                string expandedOutput = "";
                isDataEntryQueryOnly = false;

                _databaseConnectionInfo.PathTags = pathTags;

                ExtractException.Assert("ELI36984", "Invalid text expansion parameters.",
                    (databaseConnectionInfo == null) != (fileProcessingDB == null));

                // Don't attempt to expand a blank string.
                if (string.IsNullOrWhiteSpace(text))
                {
                    return new[] { "" };
                }

                text = text.Trim();

                // Parse the source text into alternating "matches" where every other "match" is a
                // query and the "matches" in-between are non-query text.
                var matches = _queryParserRegex.Matches(text)
                    .OfType<Match>()
                    .ToList();

                // Iterate all non-query text to see if it contains any shorthand query syntax that
                // needs to be expanded.
                // (</AttributeName> for <Query><Attribute>AttributeName</Attribute></Query>)
                foreach (Match match in matches
                    .Where(match => !IsQuery(match))
                    .ToArray())
                {
                    // Substitute any attribute query shorthand with the full query syntax.
                    string matchText =
                        _attributeQueryFinderRegex.Replace(match.Value, SubstituteAttributeQuery);

                    // If after substitutions the _queryParserRegex finds more than one partition, or
                    // the one and only partition is a query, one or more shorthand queries were
                    // expanded. Insert the expanded partitions in place of the original one.
                    var subMatches = _queryParserRegex.Matches(matchText);
                    if (subMatches.Count > 1 || IsQuery(subMatches[0]))
                    {
                        int index = matches.IndexOf(match);
                        matches.RemoveAt(index);
                        matches.InsertRange(index, subMatches.OfType<Match>());
                    }
                }

                // Iterate all partitions of the source text, evaluating any queires as we go.
                foreach (Match match in matches)
                {
                    if (IsQuery(match))
                    {
                        // If the only partition of the overall query is a data entry query,
                        // indicate this.
                        isDataEntryQueryOnly = (matches.Count == 1);

                        // The first time a query in encountered, load the database and data for all
                        // subsequent queries for this files to use.
                        if (!_queryDataInitialized)
                        {
                            IUnknownVector sourceAttributes = new IUnknownVector();
                            string dataFileName = pathTags.Expand(DataFileName);
                            if (File.Exists(dataFileName))
                            {
                                // If data file exists, load it.
                                sourceAttributes.LoadFrom(dataFileName, false);

                                // So that the garbage collector knows of and properly manages the associated
                                // memory.
                                sourceAttributes.ReportMemoryUsage();

                                _dataFileLoaded = true;
                            }

                            DbConnection dbConnection = (fileProcessingDB == null)
                                ? GetDbConnection(databaseConnectionInfo)
                                : GetDbConnection(fileProcessingDB);

                            AttributeStatusInfo.InitializeForQuery(sourceAttributes,
                                fileRecord.Name, dbConnection, pathTags);

                            _queryDataInitialized = true;
                        }

                        try
                        {
                            // If data file does not exist and query appears to contain an attribute
                            // query, throw an error.
                            if (!_dataFileLoaded && match.Value.IndexOf(
                                    "<Attribute", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                 var ee = new ExtractException("ELI36954",
                                    "Data file is unavailable to expand query for query condition.");
                                ee.AddDebugData("Query", match.Value, false);
                                ee.AddDebugData("DataFilename", pathTags.Expand(DataFileName), false);
                                ee.AddDebugData("SourceDocName", fileRecord.Name, false);
                                ee.AddDebugData("FPS",
                                    pathTags.Expand(FileActionManagerPathTags.FpsFileNameTag), false);
                                throw ee;
                            }

                            // Append the query result to the expanded output in place of the query.
                            using (var dataQuery = DataEntryQuery.Create(match.Value, null, _dbConnection))
                            {
                                QueryResult queryResult = dataQuery.Evaluate();
                                if (isDataEntryQueryOnly)
                                {
                                    return queryResult.ToStringArray();
                                }
                                else
                                {
                                    string[] expandResults = queryResult.ToStringArray();
                                    // Per discussion with Arvind, until shorthand attribute syntax
                                    // is expanded to be able specify which of multiple items to
                                    // select or how to join multiple items, multiple results from
                                    // embedded items are not to be supported.
                                    if (expandResults.Length > 1)
                                    {
                                        var ee = new ExtractException("ELI36955",
                                            "Embedded element expanded into multiple results");
                                        ee.AddDebugData("Element", match.Value, false);
                                        foreach (string result in expandResults)
                                        {
                                            ee.AddDebugData("Results", result, false);
                                        }
                                        throw ee;
                                    }
                                    expandedOutput += string.Join("\r\n", queryResult.ToStringArray());
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (string.IsNullOrEmpty(failedQueryReplacement))
                            {
                                var ee = new ExtractException("ELI36956",
                                    "Unable to expand data query.", ex);
                                ee.AddDebugData("Query", match.Value, false);
                                ee.AddDebugData("SourceDocName", fileRecord.Name, false);
                                ee.AddDebugData("FPS",
                                    pathTags.Expand(FileActionManagerPathTags.FpsFileNameTag), false);
                                throw ee;
                            }
                            else
                            {
                                expandedOutput += failedQueryReplacement;
                            }
                        }
                    }
                    else
                    {
                        // Append any non-query text as is.
                        expandedOutput += match.Value;
                    }
                }

                // Once all queries have been expanded, expand any path tags and functions as well.
                expandedOutput = pathTags.Expand(expandedOutput);

                // If he text appears to contain database related tags that could not be expanded,
                // throw an error.
                if ((expandedOutput.IndexOf(FileActionManagerPathTags.DatabaseActionTag, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     expandedOutput.IndexOf(FileActionManagerPathTags.DatabaseNameTag, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     expandedOutput.IndexOf(FileActionManagerPathTags.DatabaseServerTag, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    var ee = new ExtractException("ELI36957",
                        "FAM DB is unavailable to expand data query for query condition.");
                    ee.AddDebugData("Query", expandedOutput, false);
                    ee.AddDebugData("SourceDocName", fileRecord.Name, false);
                    ee.AddDebugData("FPS",
                        pathTags.Expand(FileActionManagerPathTags.FpsFileNameTag), false);
                    throw ee;
                }

                return new[] { expandedOutput };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36958");
            }
        }

        /// <summary>
        /// Determines whether the specified <see paramref="match"/> is a data query.
        /// </summary>
        /// <param name="match">The <see cref="Match"/> to check.</param>
        /// <returns><see langword="true"/> if the match is a data query; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        static bool IsQuery(Match match)
        {
            return match.Value.StartsWith("<Query>", StringComparison.OrdinalIgnoreCase) &&
                match.Value.EndsWith("</Query>", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Substitutes full data query syntax for any shorthand attribute queries within the
        /// specified <see paramref="match"/>.
        /// </summary>
        /// <param name="match">The <see cref="Match"/> for which substitution should be done.
        /// </param>
        /// <returns>The text of the match with full data query syntax substituted for any shorthand
        /// attribute queries </returns>
        static string SubstituteAttributeQuery(Match match)
        {
            string result = "<Query><Attribute>" +
                match.Value.Substring(1, match.Length - 2) +
                "</Attribute></Query>";

            return result;
        }

        /// <summary>
        /// Gets the <see cref="DbConnection"/> to use for evaluating the query.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> instance currently
        /// being used by the FAM process.</param>
        /// <returns>The <see cref="DbConnection"/> to use for evaluating the query.</returns>
        DbConnection GetDbConnection(IFileProcessingDB fileProcessingDB)
        {
            try
            {
                if (_dbConnection == null)
                {
                    _dbConnection = new ExtractRoleConnection(
                        SqlUtil.CreateConnectionString(
                            fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName));
                    _dbConnection.Open();
                }

                return _dbConnection;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37131");
            }
        }

        /// <summary>
        /// Gets the <see cref="DbConnection"/> to use for evaluating the query.
        /// </summary>
        /// <param name="databaseConnectionInfo"></param>
        /// <returns>The <see cref="DbConnection"/> to use for evaluating the query.</returns>
        DbConnection GetDbConnection(DatabaseConnectionInfo databaseConnectionInfo)
        {
            if (_dbConnection == null)
            {
                _dbConnection = databaseConnectionInfo.ManagedDbConnection;
            }

            return _dbConnection;
        }

        /// <summary>
        /// Gets a <see cref="DataTable"/> representing the contents of the target database table or
        /// the results of the specified query.
        /// </summary>
        /// <param name="fileRecord">The <see cref="FileRecord"/> representing the file currently
        /// being processed.</param>
        /// <param name="pathTags">The <see cref="FileActionManagerPathTags"/> to expand any path
        /// tags or functions.</param>
        /// <param name="fileProcessingDB">The active <see cref="FileProcessingDB"/>.</param>
        /// <returns>A <see cref="DataTable"/> representing the contents of the target database
        /// table or the results of the specified query.</returns>
        DataTable GetTableOrQueryResults(FileRecord fileRecord, FileActionManagerPathTags pathTags,
            FileProcessingDB fileProcessingDB)
        {
            DataTable queryResults = null;
            string sqlQuery = "";
            bool isDataEntryQueryOnly = false;

            _databaseConnectionInfo.PathTags = pathTags;

            if (UseQuery)
            {
                // Expand any embedded DataEntryQueries.
                string[] expandedQuery = UseFAMDBConnection
                    ? ExpandText(Query, fileRecord, pathTags, null, fileProcessingDB, null, out isDataEntryQueryOnly)
                    : ExpandText(Query, fileRecord, pathTags, _databaseConnectionInfo, null, null, out isDataEntryQueryOnly);

                // If the entire query is a data entry query, expandedQuery is actually the query
                // results.
                if (isDataEntryQueryOnly)
                {
                    queryResults = expandedQuery.ToDataTable();
                }
                // Otherwise, expandedQuery is the SQL query to evaluate.
                else
                {
                    sqlQuery = expandedQuery[0];
                }
            }
            else
            {
                // If using a table instead of a query, build a query to select all data from the
                // table.
                sqlQuery = "SELECT * FROM [" + Table + "]";
            }

            if (!isDataEntryQueryOnly)
            {
                DbConnection dbConnection = UseFAMDBConnection
                    ? GetDbConnection(fileProcessingDB)
                    : GetDbConnection(_databaseConnectionInfo);

                using (DbTransaction transaction = dbConnection.BeginTransaction())
                using (var dbCommand = DBMethods.CreateDBCommand(dbConnection, sqlQuery, null))
                {
                    dbCommand.Transaction = transaction;

                    try
                    {
                        queryResults = DBMethods.ExecuteDBQuery(dbCommand);
                    }
                    finally
                    {
                        // Ensure the query does not result in the database being modified.
                        transaction.Rollback();
                    }
                }
            }

            return queryResults;
        }

        /// <summary>
        /// Checks the fields in the specified <see paramref="queryResults"/> against the configured
        /// <see cref="SearchFields"/>.
        /// </summary>
        /// <param name="queryResults">A <see cref="DataTable"/> representing the query results to
        /// check.</param>
        /// <param name="fileRecord">A <see cref="FileRecord"/> instance indicating the FAM file
        /// this check relates to.</param>
        /// <param name="pathTags">A <see cref="FileActionManagerPathTags"/> to expand any path
        /// tags/functions used in the <see cref="SearchFields"/> values.</param>
        /// <param name="fileProcessingDB">The active <see cref="IFileProcessingDB"/> instance.
        /// </param>
        /// <returns><see paramref="true"/> if the data in <see paramref="queryResults"/> satisfies
        /// the conditions specified in <see cref="SearchFields"/>; otherwise,
        /// <see langword="false"/>.</returns>
        bool CheckQueryResultFields(DataTable queryResults, FileRecord fileRecord,
            FileActionManagerPathTags pathTags, IFileProcessingDB fileProcessingDB)
        {
            foreach (var fieldDefinition in FieldDefinitions)
            {
                fieldDefinition.PrepareValue(queryResults, fileRecord, pathTags, fileProcessingDB);
            }

            bool conditionMet = false;
            switch (RowCountCondition)
            {
                case DatabaseContentsConditionRowCount.Zero:
                {
                    conditionMet = !queryResults.Rows
                        .OfType<DataRow>()
                        .Any(row => RowIsMatch(row));
                }
                break;

                case DatabaseContentsConditionRowCount.AtLeastOne:
                {
                    conditionMet = queryResults.Rows
                        .OfType<DataRow>()
                        .Any(row => RowIsMatch(row));
                }
                break;

                case DatabaseContentsConditionRowCount.ExactlyOne:
                {
                    conditionMet = queryResults.Rows
                        .OfType<DataRow>()
                        .Count(row => RowIsMatch(row)) == 1;
                }
                break;
            }

            return conditionMet;
        }

        /// <summary>
        /// Indicates if the specified <see paramref="row"/>'s fields are a match for the configured
        /// <see cref="SearchFields"/>.
        /// </summary>
        /// <param name="row">A <see cref="DataRow"/> to check against the
        /// <see cref="SearchFields"/>.</param>
        /// <returns><see paramref="true"/> if the data in <see paramref="row"/> satisfies
        /// the conditions specified in <see cref="SearchFields"/>; otherwise,
        /// <see langword="false"/>.</returns>
        bool RowIsMatch(DataRow row)
        {
            bool rowIsMatch = false;

            switch (SearchModifier)
            {
                case DatabaseContentsConditionSearchModifier.All:
                    {
                        rowIsMatch = FieldDefinitions
                            .All(field => field.RowIsMatch(row));
                    }
                    break;

                case DatabaseContentsConditionSearchModifier.Any:
                    {
                        rowIsMatch = FieldDefinitions
                            .Any(field => field.RowIsMatch(row));
                    }
                    break;

                case DatabaseContentsConditionSearchModifier.None:
                    {
                        rowIsMatch = !FieldDefinitions
                            .Any(field => field.RowIsMatch(row));
                    }
                    break;
            }

            return rowIsMatch;
        }

        #endregion Private Members
    }
}
