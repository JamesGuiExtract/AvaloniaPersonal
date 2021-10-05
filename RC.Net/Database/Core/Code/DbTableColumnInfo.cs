using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Extract.Database
{

    /// <summary>
    /// This class contains basic column information for a database column
    /// </summary>
    public class ColumnInfo
    {
        #region Fields

        /// <summary>
        /// ColumnName - unused in code, provided for debugging only.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:NoUpstreamCallers")]
        public string ColumnName { get; private set; }

        /// <summary>
        /// The text description of the column type, as retrieved by SQL query
        /// </summary>
        public string ColumnType { get; private set; }

        /// <summary>
        /// size (or width) of the column in bytes
        /// </summary>
        public int ColumnSize { get; private set; }

        /// <summary>
        /// Ordinal, or position of column
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:NoUpstreamCallers")]
        public int ColumnOrdinal { get; private set; }

        /// <summary>
        /// is this an identity column
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:NoUpstreamCallers")]
        public bool IsIdentity { get; private set; }

        /// <summary>
        /// Is this an auto-increment column
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:NoUpstreamCallers")]
        public bool IsAutoIncrement { get; private set; }

        /// <summary>
        /// Is the column read-only
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:NoUpstreamCallers")]
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Does the column except null
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:NoUpstreamCallers")]
        public bool IsNullable { get; private set; }

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor - default, as the information has to be set iteratively using Update()
        /// </summary>
        public ColumnInfo()
        {
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// This method is used to set the information
        /// </summary>
        /// <param name="name">name of the column property</param>
        /// <param name="value">associated value of the column property</param>
        [SuppressMessage("ExtractRules", "ES0001: MapExceptionToExtractException")]
        public void Update(string name, object value)
        {
            if (name == "ColumnName")
            {
                ColumnName = value.ToString();
            }
            else if (name == "ColumnOrdinal")
            {
                ColumnOrdinal = (int)value;
            }
            else if (name == "ColumnSize")
            {
                ColumnSize = (int)value;
            }
            else if (name == "AllowDBNull")
            {
                IsNullable = (bool)value;
            }
            else if (name == "ProviderType")
            {
                ColumnType = value.ToString();
            }
            else if (name == "IsIdentity")
            {
                IsIdentity = (bool)value;
            }
            else if (name == "IsAutoIncrement")
            {
                IsAutoIncrement = (bool)value;
            }
            else if (name == "IsReadOnly")
            {
                IsReadOnly = (bool)value;
            }

        }

        /// <summary>
        /// Is this column type a text/string type?
        /// </summary>
        /// <returns>true if the column type is a text/string type</returns>
        /// NOTE: There are a set of text types: nvarchar, nchar, national character 
        /// varying, and national character that all contain "char", so they are
        /// all captured in one Contains().
        public bool IsTextColumn()
        {
            try
            {
                string cType = this.ColumnType.ToUpperInvariant();

                if (true == cType.Contains("CHAR") ||
                    true == cType.Contains("NTEXT") ||  // not supported in SQLCE v3.5, might occur in older DB
                    true == cType.Contains("VARBINARY"))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39171");
            }
        }
    }

    #endregion Public Methods


    /// <summary>
    /// This class captures and provides column information for a SQL CE table.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1709: CorrectCasingInTypeName")]
    [SuppressMessage("Microsoft.Naming", "CA1710: RenameToEndInCollection")]
    public class DbTableColumnInfo : IEnumerable<ColumnInfo>
    {
        #region Fields

        /// <summary>
        /// The name of the table.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// list of column info
        /// </summary>
        List<ColumnInfo> _columns = null;

        #endregion Fields

        #region Constants

        const int columnNameIndex = 0;
        const int dataTypeIndex = 1;
        const int columnSizeIndex = 2;

        #endregion Constants

        #region Constructors

        /// <summary>
        /// CTOR retrieves table column info
        /// </summary>
        /// <param name="tableName">name of the table to get column information for</param>
        /// <param name="connection">open connection to database</param>
        public DbTableColumnInfo(string tableName, DbConnection connection)
        {
            TableName = tableName;
            _columns = new List<ColumnInfo>();

            var query = String.Format(CultureInfo.InvariantCulture, "SELECT TOP (1) * FROM [{0}];", TableName);
            using (DbCommand cmd = DBMethods.CreateDBCommand(connection, query, null))
            {
                using var reader = cmd.ExecuteReader();
                using (DataTable dt = reader.GetSchemaTable())
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        ColumnInfo ci = new ColumnInfo();
                        foreach (DataColumn dc in dt.Columns)
                        {
                            ci.Update(name: dc.ColumnName, value: dr[dc]);
                        }

                        _columns.Add(ci);
                    }
                }
            }
        }

        #endregion Constructors

        #region Public Methods
        /// <summary>
        /// This method supports use of this class in foreach loops
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ColumnInfo> GetEnumerator()
        {
            return _columns.GetEnumerator();
        }

        /// <summary>
        /// indexer that retrieves specified column info
        /// </summary>
        /// <param name="index">index value must be less than Count of internal 
        /// collection of columns(enforced). Use DbTableColumnInfo.Count to determine 
        /// maximum extent of the collection.</param>
        /// <returns>The specified ColumnInfo</returns>
        public ColumnInfo this[int index]
        {
            get
            {
                return _columns[index];
            }
        }

        /// <summary>
        /// The Count of the internal collection of columns
        /// </summary>
        public int Count
        {
            get
            {
                return _columns.Count;
            }
        }

        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// This method is also required to support foreach use.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion Private Methods
    }
}