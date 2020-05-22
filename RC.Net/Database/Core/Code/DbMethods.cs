using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.FormattableString;

namespace Extract.Database
{
    /// <summary>
    /// A collection of database utility methods.
    /// </summary>
    public static class DBMethods
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(DBMethods).ToString();

        /// <summary>
        /// Provides access to settings in the config file.
        /// </summary>
        static ConfigSettings<Properties.Settings> _config = new ConfigSettings<Properties.Settings>(true);

        #endregion Constants

        #region Fields

        /// <summary>
        /// Cache the <see cref="DbProviderFactory"/> looked up by <see cref="GetDBProvider"/>
        /// per thread so that subsequent calls don't have to go through the full lookup process.
        /// </summary>
        [ThreadStatic]
        static KeyValuePair<DbConnection, DbProviderFactory> _lastProvider;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Generates a <see cref="DbCommand"/> based on the specified query, parameters and database
        /// connection.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/> for which the command is
        /// to apply.</param>
        /// <param name="query">The <see cref="DbCommand"/>'s <see cref="DbCommand.CommandText"/>
        /// value.</param>
        /// <param name="parameters">A <see cref="Dictionary{T, T}"/> of parameter names and values
        /// that need to be parameterized for the command if specified, <see langword="null"/> if
        /// parameters are not being used. Note that if parameters are being used, the parameter
        /// names must have already been inserted into <see paramref="query"/>.</param>
        /// <returns>The generated <see cref="DbCommand"/>.</returns>
        public static DbCommand CreateDBCommand(DbConnection dbConnection, string query,
            Dictionary<string, string> parameters)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.ExtractCoreObjects, "ELI40295", _OBJECT_NAME);

                ExtractException.Assert("ELI40296", "Null argument exception!",
                    dbConnection != null);
                ExtractException.Assert("ELI40297", "Null argument exception!",
                    !string.IsNullOrEmpty(query));

                DbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = query;

                // If parameters are being used, specify them.
                if (parameters != null)
                {
                    // We need a DbProviderFactory to create the parameters.
                    DbProviderFactory providerFactory = GetDBProvider(dbConnection);

                    // [DataEntry:1273]
                    // In case the parameters are not named, order will be important, so ensure the
                    // parameters are added in order of their key (interpreted as a number, if possible).
                    int intValue = 0;
                    foreach (KeyValuePair<string, string> parameter in parameters
                        .OrderBy(parameter =>
                            (parameter.Key.Length > 1 && 
                             int.TryParse(parameter.Key.Substring(1), out intValue))
                                ? (IComparable)intValue
                                : (IComparable)parameter.Key))
                    {
                        DbParameter dbParameter = providerFactory.CreateParameter();
                        dbParameter.Direction = ParameterDirection.Input;
                        dbParameter.ParameterName = parameter.Key;
                        dbParameter.Value = parameter.Value;
                        dbCommand.Parameters.Add(dbParameter);
                    }
                }

                return dbCommand;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI40298", ex);
                ee.AddDebugData("Query", query, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the <see cref="DbProviderFactory"/> that corresponds with the specified
        /// <see cref="DbConnection"/>.
        /// <para><b>Note</b></para>
        /// MSDN doc claims the availability of a DbProviderFactories.GetFactory() override
        /// in .Net 4.0 that takes a DbConnection... but that doesn't seem to be the case.
        /// http://msdn.microsoft.com/en-us/library/hh323136(v=vs.100).aspx
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/> for which the provider is
        /// needed.</param>
        /// <returns>The <see cref="DbProviderFactory"/> that corresponds with the specified
        /// <see cref="DbConnection"/>.</returns>
        public static DbProviderFactory GetDBProvider(DbConnection dbConnection)
        {
            try
            {
                if (_lastProvider.Key == dbConnection)
                {
                    return _lastProvider.Value;
                }

                // Use GetProviderMatchScore to select the provider that has the closest version to
                // dbConnection.ServerVersion from the providers that correspond with the connection
                // type.
                var providerRow = DbProviderFactories
                    .GetFactoryClasses()
                    .Rows.Cast<DataRow>()
                    .Select(row => new Tuple<DataRow, int>(
                        row, GetProviderMatchScore(row, dbConnection)))
                    .Where(item => item.Item2 > 0)
                    .OrderByDescending(item => item.Item2)
                    .Select(item => item.Item1)
                    .FirstOrDefault();

                DbProviderFactory providerFactory = (providerRow == null)
                    ? DbProviderFactories.GetFactory(_config.Settings.DefaultDBProviderFactoryName)
                    : DbProviderFactories.GetFactory(providerRow);

                // Cache the provider per-thread so that subsequent calls don't have to go through
                // the full lookup process.
                _lastProvider = new KeyValuePair<DbConnection, DbProviderFactory>(
                    dbConnection, providerFactory);

                return providerFactory;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36825");
            }
        }

        /// <summary>
        /// Gets the query results as a string array.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <param name="query">The query to execute.</param>
        /// <returns>A string array representing the results of the query where each
        /// value is a separate row and where the values are delimited by tabs.</returns>
        public static string[] GetQueryResultsAsStringArray(DbConnection dbConnection, string query)
        {
            try
            {
                using (DataTable resultsTable = ExecuteDBQuery(dbConnection, query))
                {
                    return resultsTable.ToStringArray("\t");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36989");
            }
        }

        /// <summary>
        /// Executes the supplied <see paramref="query"/> on the specified
        /// <see paramref="dbConnection"/>.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <param name="query">The query to execute.</param>
        /// <param name="parameters">Parameters to be used in the query. They key for each parameter
        /// must begin with the appropriate symbol ("@" for T-SQL and SQL CE, ":" for Oracle) and
        /// that key should appear in the <see paramref="query"/>.</param>
        /// <param name="transaction">If not <c>null</c>, the transaction in which the query
        /// should run.</param>
        /// <returns>A <see cref="DataTable"/> representing the results of the query.</returns>
        public static DataTable ExecuteDBQuery(DbConnection dbConnection, string query,
            Dictionary<string, string> parameters = null, DbTransaction transaction = null)
        {
            try
            {
                using (var command = DBMethods.CreateDBCommand(dbConnection, query, parameters))
                {
                    if (transaction != null)
                    {
                        command.Transaction = transaction;
                    }
                    return ExecuteDBQuery(command);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34572");
            }
        }

        /// <summary>
        /// Gets the query results as a string array.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <param name="query">The query to execute.</param>
        /// <param name="parameters">Parameters to be used in the query. They key for each parameter
        /// must begin with the appropriate symbol ("@" for T-SQL and SQL CE, ":" for Oracle) and
        /// that key should appear in the <see paramref="query"/>.</param>
        /// <param name="columnSeparator">The string used to separate multiple column results.
        /// (Will not be included in any result with less than 2 columns)</param>
        /// <returns>A string array representing the results of the query where each
        /// value is a separate row and where the values are delimited by
        /// <see paramref="columnSeparator"/>.</returns>
        /// <param name="checkForExistingSeparators">Whether to check for existing separators,
        /// and if found, to quote values</param>
        public static string[] GetQueryResultsAsStringArray(DbConnection dbConnection, string query,
            Dictionary<string, string> parameters, string columnSeparator, bool checkForExistingSeparators = false)
        {
            try
            {
                using (DataTable resultsTable = ExecuteDBQuery(dbConnection, query, parameters))
                {
                    return resultsTable.ToStringArray(columnSeparator, checkForExistingSeparators);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36978");
            }
        }


        /// <summary>
        /// Executes a query against the specified database connection and returns the
        /// result as a string array.
        /// </summary>
        /// <param name="dbCommand">The <see cref="DbCommand"/> defining the query to be applied.
        /// </param>
        /// <returns>A <see cref="DataTable"/> representing the results of the query.</returns>
        public static DataTable ExecuteDBQuery(DbCommand dbCommand)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI40299", _OBJECT_NAME);

                ExtractException.Assert("ELI26151", "Null argument exception!", dbCommand != null);

                using (DbDataReader sqlReader = dbCommand.ExecuteReader())
                using (DataSet dataSet = new DataSet())
                {
                    dataSet.Locale = CultureInfo.CurrentCulture;
                    // Use a DataSet to turn off enforcement of constaints primarily for backward
                    // compatibility-- the old ExecuteDBQuery method that returned a string array
                    // did not enforce constraints.
                    DataTable dataTable = new DataTable();
                    dataTable.Locale = CultureInfo.CurrentCulture;
                    dataSet.Tables.Add(dataTable);
                    dataSet.EnforceConstraints = false;

                    dataTable.Load(sqlReader);

                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee =
                    new ExtractException("ELI26150", "Database query failed.", ex);

                if (dbCommand != null)
                {
                    ee.AddDebugData("Query", dbCommand.CommandText, false);

                    try
                    {
                        foreach (DbParameter parameter in dbCommand.Parameters)
                        {
                            ee.AddDebugData("Parameter " + parameter.ParameterName,
                                parameter.Value.ToString(), false);
                        }
                    }
                    catch (Exception ex2)
                    {
                        ExtractException.Log("ELI40300", ex2);
                    }
                }

                throw ee;
            }
        }

        /// <summary>
        /// Returns the data in <see paramref="dataTable"/> as a string array.
        /// </summary>
        /// <param name="dataTable">The <see cref="DataTable"/> containing the data to return as a
        /// string array.</param>
        /// <param name="columnSeparator">The string used to separate multiple column results.
        /// (Will not be included in any result with less than 2 columns)</param>
        /// <remarks>Any values that contain a column separator will be quoted with double quotes. Any
        /// double quote characters in a quoted value will be escaped by doubling them.</remarks>
        /// <param name="checkForExistingSeparators">Whether to check for existing separators,
        /// and if found, to quote values</param>
        public static string[] ToStringArray(this DataTable dataTable,
            string columnSeparator, bool checkForExistingSeparators = false)
        {
            try
            {
                List<string> results = new List<string>();
                bool quoteWhenNeeded = checkForExistingSeparators && !string.IsNullOrEmpty(columnSeparator);

                // Loop throw each row of the results.
                for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
                {
                    StringBuilder result = new StringBuilder();

                    // Keep track of all column delimiters that are appended. They are only added
                    // once it is confirmed that there is more data in the row.
                    StringBuilder pendingColumnDelimiters = new StringBuilder();

                    for (int columnIndex = 0; columnIndex < dataTable.Columns.Count; columnIndex++)
                    {
                        // If not the first column result, a column separator may be needed.
                        if (columnIndex > 0)
                        {
                            pendingColumnDelimiters.Append(columnSeparator);
                        }

                        string columnValue = dataTable.Rows[rowIndex][columnIndex].ToString();

                        if (!string.IsNullOrEmpty(columnValue))
                        {
                            // If there is data to write, go ahead and commit all pending
                            // column delimiters.
                            result.Append(pendingColumnDelimiters.ToString());

                            // Reset the pending column delimiters
                            pendingColumnDelimiters = new StringBuilder();

                            // Quote string if it contains a column separator or a quote character
                            if (quoteWhenNeeded)
                            {
                                columnValue = columnValue.QuoteIfNeeded("\"", columnSeparator);
                            }
                            result.Append(columnValue);
                        }
                    }

                    results.Add(result.ToString());
                }

                return results.ToArray();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36979");
            }
        }

        /// <summary>
        /// Adds the specified <see paramref="data"/> to a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="data">A string array representing the rows of data to add to the table. The
        /// column delimiters are assumed to be tab characters.
        /// </param>
        /// <returns>A <see cref="DataTable"/> with the data from <see paramref="data"/>.</returns>
        public static DataTable ToDataTable(this string[] data)
        {
            try 
	        {	        
		        return data.ToDataTable("\t");
	        }
	        catch (Exception ex)
	        {
		        throw ex.AsExtract("ELI36980");
	        }
        }

        /// <summary>
        /// Adds the specified <see paramref="data"/> to a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="data">A string array representing the rows of data to add to the table. The
        /// column delimiters are assumed to be tab characters.
        /// </param>
        /// <param name="columnSeparator">The string used to separate multiple columns in the
        /// <see paramref="data"/>.</param>
        /// <returns>A <see cref="DataTable"/> with the data from <see paramref="data"/>.</returns>
        public static DataTable ToDataTable(this string[] data, string columnSeparator)
        {
            try
            {
                DataTable dataTable = new DataTable();
                dataTable.Locale = CultureInfo.CurrentCulture;
                if (data.Length == 0)
                {
                    return dataTable;
                }

                int columnCount = 
                    data.Max(row =>
                        Enumerable.Range(0, row.Length - 1)
                        .Count(index => row.IndexOf(columnSeparator, index, StringComparison.Ordinal) >= 0) + 1);
                for (int i = 0; i < columnCount; i++)
                {
                    dataTable.Columns.Add();
                }
                foreach (string row in data)
                {
                    dataTable.Rows.Add(row.Split('\t'));
                }

                return dataTable;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36981");
            }
        }

        /// <overloads>
        /// Formats the data from the specified <see paramref="query"/> into the provided
        /// <see paramref="dataGridView"/> using the formatting options specified. The columns
        /// (along with the column names) will be generated via the schema of the query result;
        /// columns are assumed not to be pre-created.
        /// </overloads>
        /// <summary>
        /// <para><b>Note</b></para>
        /// The <see paramref="dataGridView"/> will be bound to a <see cref="DataTable"/> which will
        /// be automatically disposed upon any subsequent call into this method; Final disposal of
        /// the <see cref="DataTable"/>, however, is the responsibility of the caller.
        /// </summary>
        /// <param name="connection">The an open <see cref="DbConnection"/> be used to execute the
        /// query.</param>
        /// <param name="query">The query that is to generate the data for the grid.</param>
        /// <param name="parameters">A <see cref="Dictionary{T, T}"/> of parameter names and values
        /// that need to be parameterized for the command if specified, <see langword="null"/> if
        /// parameters are not being used. Note that if parameters are being used, the parameter
        /// names must have already been inserted into <see paramref="query"/>.</param>
        /// <param name="dataGridView">The <see cref="DataGridView"/> to be populated with the
        /// query results.</param>
        /// <param name="resetLayout"><see langword="true"/> if columns widths and sorting is
        /// is to be reset; <see langword="false"/> to maintain existing column sizes and sorting.
        /// </param>
        /// <param name="resetScrollPos"><see langword="true"/> if the scroll position of the grid
        /// is to be reset; <see langword="false"/> to maintain existing scroll position if possible.
        /// </param>
        /// <param name="fitHeaderText"><see langword="true"/> if, when <see paramref="resetLayout"/>
        /// is <see langword="true"/>, column widths should be sized to the width of the header text
        /// (regardless of any width or fill weights specified); <see langword="false"/> if the
        /// columns initial sizing can be narrower than the header text.
        /// </param>
        /// <param name="columnLayouts">An array of <see cref="ColumnLayout"/> instances specifying
        /// layout and formatting of the column of the same ordinal. Can be <see langword="null"/> to
        /// use default formatting for all columns, can include less than the number columns to use
        /// default formatting for the remaining columns or can have <see langword="null"/> at any
        /// index to indicate the column of the matching ordinal should use default formatting.
        /// </param>
        public static void FormatDataIntoGrid(DbConnection connection, string query,
            Dictionary<string, string> parameters, DataGridView dataGridView, bool resetLayout,
            bool resetScrollPos, bool fitHeaderText, params ColumnLayout[] columnLayouts)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.ExtractCoreObjects, "ELI39340", _OBJECT_NAME);

                DataTable queryResults = ExecuteDBQuery(connection, query, parameters);

                var oldData = dataGridView.DataSource as IDisposable;

                FormatDataIntoGrid(queryResults, dataGridView, resetLayout, resetScrollPos,
                    fitHeaderText, columnLayouts);

                if (oldData != null)
                {
                    oldData.Dispose();
                }

            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39367");
            }
        }

        /// <summary>
        /// Formats the data from the specified <see paramref="DataTable"/> into the provided
        /// <see paramref="dataGridView"/> using the formatting options specified. The columns
        /// (along with the column names) will be generated via the schema of the query result;
        /// columns are assumed not to be pre-created.
        /// </summary>
        /// <param name="data">The <see cref="DataTable"/> to be used as the data source.</param>
        /// <param name="dataGridView">The <see cref="DataGridView"/> to be populated with the
        /// <see cref="DataTable"/> data.</param>
        /// <param name="resetLayout"><see langword="true"/> if columns widths and sorting is
        /// is to be reset; <see langword="false"/> to maintain existing column sizes and sorting.
        /// </param>
        /// <param name="resetScrollPos"><see langword="true"/> if the scroll position of the grid
        /// is to be reset; <see langword="false"/> to maintain existing scroll position if possible.
        /// </param>
        /// <param name="fitHeaderText"><see langword="true"/> if, when <see paramref="resetLayout"/>
        /// is <see langword="true"/>, column widths should be sized to the width of the header text
        /// (regardless of any width or fill weights specified); <see langword="false"/> if the
        /// columns initial sizing can be narrower than the header text.
        /// </param>
        /// <param name="columnLayouts">An array of <see cref="ColumnLayout"/> instances specifying
        /// layout and formatting of the column of the same ordinal. Can be <see langword="null"/> to
        /// use default formatting for all columns, can include less than the number columns to use
        /// default formatting for the remaining columns or can have <see langword="null"/> at any
        /// index to indicate the column of the matching ordinal should use default formatting.
        /// </param>
        public static void FormatDataIntoGrid(DataTable data, DataGridView dataGridView, bool resetLayout,
            bool resetScrollPos, bool fitHeaderText, params ColumnLayout[] columnLayouts)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.ExtractCoreObjects, "ELI40366", _OBJECT_NAME);

                int sortedColumnIndex = -1;
                ListSortDirection sortOrder = ListSortDirection.Ascending;
                int scrollPos = 0;
                int[] columnWidths = null;
                string[] columnNames = null;

                if (!resetLayout)
                {
                    // If updating the results, keep track of the sort order, last scroll position,
                    // and column sized so that we can keep the same data visible.
                    sortedColumnIndex = (dataGridView.SortedColumn == null)
                        ? -1
                        : dataGridView.SortedColumn.Index;
                    sortOrder = (dataGridView.SortOrder == System.Windows.Forms.SortOrder.Descending)
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                    columnWidths = dataGridView.Columns
                        .OfType<DataGridViewColumn>()
                        .Select(column => column.Width)
                        .ToArray();
                    columnNames = dataGridView.Columns
                        .OfType<DataGridViewColumn>()
                        .Select(column => column.Name)
                        .ToArray();
                }

                if (!resetScrollPos)
                {
                    scrollPos = dataGridView.FirstDisplayedScrollingRowIndex;
                }

                dataGridView.DataSource = data;

                // If the new columns differ from the previous columns, the layout should be reset
                // despite the passed-in resetLayout value.
                if (!resetLayout &&
                    !columnNames.SequenceEqual(
                        dataGridView.Columns
                        .OfType<DataGridViewColumn>()
                        .Select(column => column.Name)))
                {
                    resetLayout = true;
                }

                if (resetLayout)
                {
                    LayoutGridColumns(dataGridView, data, fitHeaderText, columnLayouts);
                }
                else
                {
                    // Re-apply the previous column widths.
                    foreach (DataGridViewColumn column in dataGridView.Columns)
                    {
                        column.Width = columnWidths[column.Index];
                    }

                    // Restore the previous sort order.
                    if (sortedColumnIndex >= 0)
                    {
                        dataGridView.Sort(dataGridView.Columns[sortedColumnIndex], sortOrder);
                    }
                }

                // Re-apply the previous vertical scroll position for tables to try to make it
                // appear that the table was updated in-place.
                if (!resetScrollPos && scrollPos >= 0 && scrollPos < dataGridView.RowCount)
                {
                    dataGridView.FirstDisplayedScrollingRowIndex = scrollPos;
                }

                dataGridView.Refresh();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39338");
            }
        }

        /// <summary>
        /// Restores the specified <see paramref name="backupFileName"/> to the specified database
        /// name on the local SQL instance.
        /// </summary>
        /// <param name="backupFileName">Name of the database backup file.</param>
        /// <param name="databaseName">Name of the database to restore to.</param>
        public static void RestoreDatabaseToLocalServer(string backupFileName, string databaseName)
        {
            try
            {
                // NOTE: If the specified backup file contains multiple backup sets, this method
                // will restore the earliest version, not the latest. To use the latest version, 
                // a separate "RESTORE HEADERONLY" needs to be run to find the latest version and
                // then that needs to be specified instead of in the "WITH FILE=x" clause below.

                using (var dbConnection = new SqlConnection(
                        "Server=(local);Integrated Security=SSPI"))
                {
                    dbConnection.Open();

                    string dataFolder = GetSqlFolder(dbConnection, dataFolder: true);
                    string logFolder = GetSqlFolder(dbConnection, dataFolder: false);

                    var parameters = new Dictionary<string, string>()
                    {
                        { "@BackupFile", backupFileName },
                        { "@NewDatabaseName", databaseName },
                        { "@DataFolder", dataFolder },
                        { "@LogFolder", logFolder }
                    };

                    // This query is derived from: http://weblogs.sqlteam.com/dang/archive/2009/06/13.aspx
                    string query = @"
                        SET NOCOUNT ON;

                        DECLARE @LogicalName nvarchar(128),
                            @PhysicalName nvarchar(260),
                            @PhysicalFolderName nvarchar(260),
                            @PhysicalFileName nvarchar(260),
                            @NewPhysicalName nvarchar(260),
                            @NewLogicalName nvarchar(128),
                            @RestoreStatement nvarchar(MAX),
                            @FileType char(1),
                            @ChangeLogicalNamesSql nvarchar(MAX),
                            @Error int;

                        DECLARE @FileList TABLE
                        (
                            LogicalName nvarchar(128) NOT NULL,
                            PhysicalName nvarchar(260) NOT NULL,
                            Type char(1) NOT NULL,
                            FileGroupName nvarchar(120) NULL,
                            Size numeric(20, 0) NOT NULL,
                            MaxSize numeric(20, 0) NOT NULL,
                            FileID bigint NULL,
                            CreateLSN numeric(25, 0) NULL,
                            DropLSN numeric(25, 0) NULL,
                            UniqueID uniqueidentifier NULL,
                            ReadOnlyLSN numeric(25, 0) NULL,
                            ReadWriteLSN numeric(25, 0) NULL,
                            BackupSizeInBytes bigint NULL,
                            SourceBlockSize int NULL,
                            FileGroupID int NULL,
                            LogGroupGUID uniqueidentifier NULL,
                            DifferentialBaseLSN numeric(25, 0)NULL,
                            DifferentialBaseGUID uniqueidentifier NULL,
                            IsReadOnly bit NULL,
                            IsPresent bit NULL,
                            TDEThumbprint varbinary(32) NULL
                        );

                        DECLARE @FileList2016 TABLE
                        (
                            LogicalName nvarchar(128) NOT NULL,
                            PhysicalName nvarchar(260) NOT NULL,
                            Type char(1) NOT NULL,
                            FileGroupName nvarchar(120) NULL,
                            Size numeric(20, 0) NOT NULL,
                            MaxSize numeric(20, 0) NOT NULL,
                            FileID bigint NULL,
                            CreateLSN numeric(25, 0) NULL,
                            DropLSN numeric(25, 0) NULL,
                            UniqueID uniqueidentifier NULL,
                            ReadOnlyLSN numeric(25, 0) NULL,
                            ReadWriteLSN numeric(25, 0) NULL,
                            BackupSizeInBytes bigint NULL,
                            SourceBlockSize int NULL,
                            FileGroupID int NULL,
                            LogGroupGUID uniqueidentifier NULL,
                            DifferentialBaseLSN numeric(25, 0)NULL,
                            DifferentialBaseGUID uniqueidentifier NULL,
                            IsReadOnly bit NULL,
                            IsPresent bit NULL,
                            TDEThumbprint varbinary(32) NULL,
	                        SnapshotUrl nvarchar(max) NULL
                        );

                        SET @Error = 0;

                        --add trailing backslash to folder names if not already specified
                        IF LEFT(REVERSE(@DataFolder), 1) <> '\' SET @DataFolder = @DataFolder + '\';
                        IF LEFT(REVERSE(@LogFolder), 1) <> '\' SET @LogFolder = @LogFolder + '\';

                        DECLARE @CompatabilityLevel INT
                        SELECT @CompatabilityLevel = MAX(compatibility_level) FROM sys.databases

                        -- get info about the database files
                        SET @RestoreStatement = N'RESTORE FILELISTONLY FROM DISK = ''' + @BackupFile + ''''
                        IF @CompatabilityLevel < 130 -- SQL version less than 2016
                        BEGIN
	                        INSERT INTO @FileList
		                        EXEC(@RestoreStatement);
			                        SET @Error = @@ERROR;
			                        IF @Error <> 0 GOTO Done;
			                        IF NOT EXISTS(SELECT * FROM @FileList) GOTO Done;
                        END
                        ELSE
                        BEGIN
	                        INSERT INTO @FileList2016
		                        EXEC(@RestoreStatement);
			                        SET @Error = @@ERROR;
			                        IF @Error <> 0 GOTO Done;
			                        IF NOT EXISTS(SELECT * FROM @FileList2016) GOTO Done;
	
	                        -- Transfer data into FileList minus the SnapshotUrl column that did not exist prior to 2016
	                        INSERT INTO @FileList 
		                        SELECT LogicalName, PhysicalName, Type, FileGroupName, Size, MaxSize, FileID, CreateLSN, DropLSN, UniqueID,
			                        ReadOnlyLSN, ReadWriteLSN, BackupSizeInBytes, SourceBlockSize, FileGroupID, LogGroupGUID,
			                        DifferentialBaseLSN, DifferentialBaseGUID, IsReadOnly, IsPresent, TDEThumbprint
		                        FROM @FileList2016
                        END

                        --generate RESTORE DATABASE statement and ALTER DATABASE statements
                        SET @ChangeLogicalNamesSql = '';
                        SET @RestoreStatement = N'RESTORE DATABASE ' + QUOTENAME(@NewDatabaseName) + N' FROM DISK=''' + @BackupFile + ''' WITH FILE=1'
                        DECLARE FileList CURSOR LOCAL STATIC READ_ONLY FOR
                            SELECT
                                Type AS FileType,
                                LogicalName,
                                --extract folder name from full path
                                LEFT(PhysicalName,
                                    LEN(LTRIM(RTRIM(PhysicalName))) -
                                    CHARINDEX('\',
                                    REVERSE(LTRIM(RTRIM(PhysicalName)))) + 1)
                                    AS PhysicalFolderName,
                                --extract file name from full path
                                LTRIM(RTRIM(RIGHT(PhysicalName,
                                    CHARINDEX('\',
                                    REVERSE(PhysicalName)) - 1))) AS PhysicalFileName
                        FROM @FileList;
                        OPEN FileList;

                        WHILE 1 = 1
                        BEGIN
                            FETCH NEXT FROM FileList INTO
                                @FileType, @LogicalName, @PhysicalFolderName, @PhysicalFileName;
                            IF @@FETCH_STATUS = -1 BREAK;

                            SET @NewPhysicalName =
                            CASE @FileType
                                WHEN 'D' THEN
                                    COALESCE(@DataFolder, @PhysicalFolderName) + '\' + @NewDatabaseName + '.mdf'
                                WHEN 'L' THEN
                                    COALESCE(@LogFolder, @PhysicalFolderName) + '\' + @NewDatabaseName + '_log.ldf'
                            END;

                            SET @NewLogicalName =
                            CASE @FileType
                                WHEN 'D' THEN @NewDatabaseName
                                WHEN 'L' THEN @NewDatabaseName + '_log'
                            END;

                            IF @NewLogicalName <> @LogicalName
                                SET @ChangeLogicalNamesSql = @ChangeLogicalNamesSql + N'ALTER DATABASE ' + QUOTENAME(@NewDatabaseName) + N'
                                    MODIFY FILE (NAME = ''' + @LogicalName + N''', NEWNAME = ''' + @NewLogicalName + N'''); '

                            -- add MOVE option as needed if folder and / or file names are changed
                            IF @PhysicalFolderName +@PhysicalFileName <> @NewPhysicalName
                            BEGIN
                                SET @RestoreStatement = @RestoreStatement +
                                      N',
                                      MOVE ''' +
                                      @LogicalName +
                                      N''' TO ''' +
                                      @NewPhysicalName +
                                      N'''';
                            END;
                        END;

                        CLOSE FileList;
                        DEALLOCATE FileList;

                        EXEC(@RestoreStatement);
                        SET @Error = @@ERROR;
                        IF @Error <> 0 GOTO Done;

                        EXEC(@ChangeLogicalNamesSql);

                        Done:";

                    ExecuteDBQuery(dbConnection, query, parameters);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41906");
            }
        }

        /// <summary>
        /// Gets the default data or log folder for the specified SQL server connection.
        /// </summary>
        /// <param name="sqlConnection">The <see cref="DbConnection"/> for which the data or log
        /// folder is needed.</param>
        /// <param name="dataFolder"><c>true</c> to get the data folder; <c>false</c> to get the
        /// log folder.</param>
        /// <returns>The data or log folder.</returns>
        public static string GetSqlFolder(DbConnection sqlConnection, bool dataFolder)
        {
            try
            {
                // This query is derived from: http://stackoverflow.com/a/12756990
                string query = @"
                    DECLARE @Default NVARCHAR(512)
                    DECLARE @Master NVARCHAR(512)
                    EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',
                        N'Software\Microsoft\MSSQLServer\MSSQLServer', "
                        + Invariant($"{(dataFolder ? "N'DefaultData', " : "N'DefaultLog', ")}")
                            + @" @Default OUTPUT
                    EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',
                        N'Software\Microsoft\MSSQLServer\MSSQLServer\Parameters', "
                        + Invariant($"{(dataFolder ? "N'SqlArg0', " : "N'SqlArg2', ")}")
                            + @" @Master OUTPUT
                    SELECT @Master = SUBSTRING(@Master, 3, 255)
                    SELECT @Master = SUBSTRING(@Master, 1, LEN(@Master) - CHARINDEX('\', REVERSE(@Master)))
                    SELECT ISNULL(@Default, @Master)";

                return GetQueryResultsAsStringArray(sqlConnection, query)[0];
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41905");
            }
        }


        /// <summary>
        /// Drops the specified <see paramref="databaseName"/> from the local SQL instance.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        public static void DropLocalDB(string databaseName)
        {
            try
            {
                using (var dbConnection = new SqlConnection(
                        "Server=(local);Integrated Security=SSPI"))
                {
                    dbConnection.Open();

                    // No matter what I do, the connection that I open and use in .NET won't release the DB.
                    // Setting SINGLE_USER fixes the problem
                    ExecuteDBQuery(dbConnection, Invariant($"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"));

                    ExecuteDBQuery(dbConnection, Invariant($"DROP DATABASE [{databaseName}]"));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41907");
            }
        }

        #endregion Methods

        #region Private Members

        /// <summary>
        /// Determines whether the <see paramref="providerDataRow"/> appears to be the one used by
        /// the <see paramref="dbConnection"/> based on the namespace and assembly and, if so,
        /// closely the <see cref="DbConnection.ServerVersion"/> and the provider's server version
        /// match.
        /// </summary>
        /// <param name="providerDataRow"><see cref="DataRow"/> representing a
        /// <see cref="DbProviderFactory"/>.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <returns>-1 if the provider doesn't correspond to the connection; 0 if the provider
        /// corresponds, but the version numbers are complete different; 1 if they correspond and
        /// only the major version matches, 2 if the minor version matches as well, 3 if all but
        /// the revision match and 4 if the version numbers are identical.
        /// </returns>
        static int GetProviderMatchScore(DataRow providerDataRow, DbConnection dbConnection)
        {
            var connectionNameParser=
                new AssemblyQualifiedNameParser(dbConnection.GetType().AssemblyQualifiedName);
            var providerNameParser =
                new AssemblyQualifiedNameParser(providerDataRow["AssemblyQualifiedName"].ToString());

            // Check that the provider and connection are from the same namespace
            if (connectionNameParser.Namespace != providerNameParser.Namespace ||
                connectionNameParser.PublicKeyToken != providerNameParser.PublicKeyToken)
            {
                // -1 indicates that this provider isn't a match for the current connection.
                return -1;
            }

            // https://extract.atlassian.net/browse/ISSUE-12161
            // At this point it appears the provider is a match. However, at least for SQLServerCE,
            // there can be multiple matching versions installed and using the wrong version will
            // lead to errors. It doesn't appear to be the case that version numbers can be matched
            // up exactly (at least in all cases), but the matching candidates can be ranked by how
            // closely the version numbers correspond; 

            // Score a point for each component of the version that matches.
            int score = 0;
            if (providerNameParser.Version.Major == connectionNameParser.Version.Major)
            {
                score++;
                if (providerNameParser.Version.Minor == connectionNameParser.Version.Minor)
                {
                    score++;
                    if (providerNameParser.Version.Build == connectionNameParser.Version.Build)
                    {
                        score++;
                        if (providerNameParser.Version.Revision == connectionNameParser.Version.Revision)
                        {
                            score++;
                        }
                    }
                }
            }

            return score;
        }

        /// <summary>
        /// Helper method for FormatDataIntoGrid. Applies layout and formatting of
        /// <see paramref="dataGridView"/>'s columns.
        /// </summary>
        /// <param name="dataGridView">The <see cref="DataGridView"/> instance whose columns are to
        /// be formatted.</param>
        /// <param name="dataTable">The <see cref="DataTable"/> to which
        /// <see paramref="dataGridView"/> is bound.</param>
        /// <param name="fitHeaderText"><see langword="true"/> if column widths should be sized to
        /// the width of the header text (regardless of any width or fill weights specified);
        /// <see langword="false"/> if the columns initial sizing can be narrower than the header
        /// text.
        /// </param>
        /// <param name="specifiedLayouts">An array of <see cref="ColumnLayout"/> instances specifying
        /// layout and formatting of the column of the same ordinal. Can be <see langword="null"/> to
        /// use default formatting for all columns, can include less than the number columns to use
        /// default formatting for the remaining columns or can have <see langword="null"/> at any
        /// index to indicate the column of the matching ordinal should use default formatting.
        /// </param>
        static void LayoutGridColumns(DataGridView dataGridView, DataTable dataTable,
            bool fitHeaderText, ColumnLayout[] specifiedLayouts)
        {
            if (dataGridView.Columns.Count == 0)
            {
                return;
            }

            // Keeps track of the effective ColumnLayout instances used for each column, even for
            // columns where not any/all layout info was supplied.
            var columnLayouts = new List<ColumnLayout>();

            // font and graphics will be used to measure header text to enforce fitHeaderText.
            var font = dataGridView.ColumnHeadersDefaultCellStyle.Font;
            using (var graphics = dataGridView.CreateGraphics())
            {
                foreach (var column in dataGridView.Columns.OfType<DataGridViewColumn>())
                {
                    ColumnLayout layout = GetColumnLayout(column, dataTable, specifiedLayouts);
                    columnLayouts.Add(layout);

                    if (layout.FillWeight != null)
                    {
                        column.FillWeight = layout.FillWeight.Value;
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                    else
                    {
                        column.Width = layout.Width.Value;
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    }

                    if (layout.Style != null)
                    {
                        column.DefaultCellStyle = layout.Style;
                    }

                    if (layout.HeaderStyle != null)
                    {
                        column.HeaderCell.Style = layout.HeaderStyle;
                    }

                    // Seems like there should be a way to get appropriate column width for header
                    // text without side-effect such as triggering other columns to resize, but I
                    // couldn't find such a way. Trial and error seems to show header text plus
                    // "X" provides appropriate width for a non-sortable column or "XXX" to allow
                    // for the sort indicator.
                    // Temporarily set MinimumWidth to this width.
                    if (fitHeaderText)
                    {
                        string padding = (column.SortMode == DataGridViewColumnSortMode.Automatic)
                            ? "XXX"
                            : "X";

                        int headerTextWidth = (int)Math.Ceiling(
                            graphics.MeasureString(column.HeaderText + padding, font).Width);
                        column.MinimumWidth = 
                            Math.Max(layout.MinimumWidth.Value, (int)(headerTextWidth));
                    }
                }

                // Perform a layout to apply the default column sizes.
                dataGridView.PerformLayout();
            }

            int lastColumnIndex = dataGridView.Columns.Count - 1;

            // Remove column auto-size (and any other temporary settings) that were needed for the
            // initial layout so that after the initial layout column sizing is manual with the
            // exception of the last column which will fill all remaining space as the columns are
            // manually resized.
            foreach (var column in dataGridView.Columns
                    .Cast<DataGridViewColumn>()
                    .Take(lastColumnIndex))
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                // Now that column is sized to allow for header text, restore intended
                // MinimumWidth.
                if (fitHeaderText)
                {
                    column.MinimumWidth = columnLayouts[column.Index].MinimumWidth.Value;
                }
            }

            dataGridView.Columns[lastColumnIndex].AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;
        }

        /// <summary>
        /// Helper method for FormatDataIntoGrid. Retrieves or initializes a 
        /// <see cref="ColumnLayout"/> instance for the specified <see paramref="ColumnLayout"/>.
        /// </summary>
        /// <param name="column">The <see cref="DataGridViewColumn"/> for which a
        /// <see cref="ColumnLayout"/> instance is needed.</param>
        /// <param name="dataTable">The <see cref="DataTable"/> serving as the data source
        /// for the <see cref="DataGridView"/> that this column belongs to.</param>
        /// <param name="specifiedLayouts">An array of <see cref="ColumnLayout"/>s dictated by the
        /// caller layouts that should be used if the array contains an instance at the
        /// <see paramref="column"/>'s ordinal.</param>
        /// <returns>An initialized <see cref="ColumnLayout"/> instance.</returns>
        static ColumnLayout GetColumnLayout(DataGridViewColumn column, DataTable dataTable, 
            ColumnLayout[] specifiedLayouts)
        {
            ColumnLayout layout = column.Index < specifiedLayouts.Length
                ? specifiedLayouts[column.Index]
                : new ColumnLayout();

            // The column index may exist in specifiedLayouts as null.
            if (layout == null)
            {
                layout = new ColumnLayout();
            }

            ExtractException.Assert("ELI39341", "Conflicting column width specification.",
                !layout.FillWeight.HasValue || !layout.Width.HasValue);

            // If neither fill weight nor width have been specified, initialize the layout with
            // default sizing appropriate for the column data type.
            if (!layout.FillWeight.HasValue && !layout.Width.HasValue)
            {
                if (column.ValueType == typeof(bool))
                {
                    // Bool fields do not need to scale larger with size as the table does and
                    // can have a smaller min width.
                    layout.Width = 25;
                    if (!layout.MinimumWidth.HasValue)
                    {
                        layout.MinimumWidth = 25;
                    }
                }
                else if (column.ValueType == typeof(string))
                {
                    // For columns containing string data, use the max length of the column as a
                    // clue for how the columns should be sized relative to other columns in the
                    // table.
                    if (dataTable != null)
                    {
                        int maxLength = dataTable.Columns[column.Index].MaxLength;

                        // For text fields that can be > 10 chars but not unlimited, scale up from
                        // a fill weight of 1.0 logarithmically.
                        // MaxLength 10	 = 1.0
                        // MaxLength 50	 = 2.6
                        // MaxLength 500 = 4.9
                        if (maxLength > 10)
                        {
                            layout.FillWeight = (float)Math.Log((double)maxLength / 3.7);

                            // Cap the max fill weight at 7 (MaxLength >= ~5000)
                            if (layout.FillWeight.Value > 7.0F)
                            {
                                layout.FillWeight = 7.0F;                               
                            }
                        }
                        else if (maxLength == -1)
                        {
                            // Fields with no limit should have max fill weight of 7.
                            layout.FillWeight = 7.0F;
                        }
                    }

                    if (!layout.FillWeight.HasValue)
                    {
                        layout.FillWeight = 1.0F;
                    }
                }
                else
                {
                    // Any other column with non-string is unlikely to benefit from greater width.
                    // Assign a static size that should fit most numbers.
                    layout.Width = 75;
                }
            }

            if (!layout.MinimumWidth.HasValue)
            {
                layout.MinimumWidth = 50;
            }

            return layout;
        }

        #endregion Private Members
    }

    /// <summary>
    /// Helper class for FormatDataIntoGrid. Encapsulates layout and
    /// formatting to be used for a given column of the grid being formated.
    /// </summary>
    public class ColumnLayout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnLayout"/> class.
        /// </summary>
        /// <param name="fillWeight">A value representing the initial width of the column relative
        /// to the widths of other columns whose size is to be calculated based on grid width. Must
        /// be <see langword="null"/> if <see cref="Width"/> is specified.</param>
        /// <param name="width">Gets or sets the width a column should be initialized to regardless
        /// of total grid width. Must be <see langword="null"/> if <see cref="FillWeight"/> is
        /// specified.</param>
        /// <param name="minimumWidth">The minimum allowed width of the column.</param>
        /// <param name="style">A <see cref="DataGridViewCellStyle"/> to use for cells in the column
        /// (except the column header cell).</param>
        /// <param name="headerStyle">A <see cref="DataGridViewCellStyle"/> to use for the column
        /// header.</param>
        public ColumnLayout(float? fillWeight = null, int? width = null, int? minimumWidth = null,
            DataGridViewCellStyle style = null, DataGridViewCellStyle headerStyle = null)
        {
            try
            {
                FillWeight = fillWeight;
                Width = width;
                MinimumWidth = minimumWidth;
                Style = style;
                HeaderStyle = headerStyle;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39342");
            }
        }

        /// <summary>
        /// Gets or sets a value representing the initial width of the column relative to the widths
        /// of other columns whose size is to be calculated based on grid width. Must be
        /// <see langword="null"/> if <see cref="Width"/> is specified.
        /// </summary>
        public float? FillWeight { get; set; }

        /// <summary>
        /// Gets or sets the width a column should be initialized to regardless of total grid width.
        /// Must be <see langword="null"/> if <see cref="FillWeight"/> is specified.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the minimum allowed width of the column.
        /// </summary>
        public int? MinimumWidth { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="DataGridViewCellStyle"/> to use for cells in the column
        /// (except the column header cell).
        /// </summary>
        public DataGridViewCellStyle Style { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="DataGridViewCellStyle"/> to use for the column header.
        /// </summary>
        public DataGridViewCellStyle HeaderStyle { get; set; }
    }
}
