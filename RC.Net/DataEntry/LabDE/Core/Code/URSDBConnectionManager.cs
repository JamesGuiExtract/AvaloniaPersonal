using Extract.Database;
using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Provides a <see cref="DbConnection"/> for the URS order mapping DB that corresponds to the
    /// provided customer order mapping <see cref="DbConnection"/>.
    /// </summary>
    public class URSDBConnectionManager : IDisposable
    {
        #region Constants

        /// <summary>
        /// The path to the URS order-mapping DB relative to the active component data directory.
        /// </summary>
        static readonly string _URS_DB_RELATIVE_LOCATION =
            @"LabDE\TestResults\OrderMapper\OrderMappingDB.sdf";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The folder path of URS database. Note that the database is several sub-folders deeper in
        /// the component data hierarchy.
        /// </summary>
        string _ursDBPath;

        /// <summary>
        /// An alternate ComponentDataDir that may be used (may be supplied by a FileProcessingDB,
        /// for instance).
        /// </summary>
        string _alternateComponentDataDir;

        /// <summary>
        /// The customer order mapping DB for which the corresponding URS database is needed.
        /// </summary>
        DbConnection _customerDBConnection;

        /// <summary>
        /// A <see cref="DatabaseConnectionInfo"/> for the URS database that corresponds to
        /// <see cref="CustomerDBConnection"/>.
        /// </summary>
        DatabaseConnectionInfo _ursDbConnectionInfo;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="URSDBConnectionManager"/> class.
        /// </summary>
        /// <param name="customerDBConnection">The customer order mapping DB for which the
        /// corresponding URS database is needed.</param>
        /// <param name="alternateComponentDataDir">An alternate ComponentData dir that may be used
        /// (may be supplied by a FileProcessingDB, for instance).</param>
        public URSDBConnectionManager(DbConnection customerDBConnection,
            string alternateComponentDataDir = null)
        {
            try
            {
                _customerDBConnection = customerDBConnection;
                _alternateComponentDataDir = alternateComponentDataDir;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39313");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the connection information for the associated order mapping database.
        /// </summary>
        /// <value>
        /// The order mapping database connection information.
        /// </value>
        public DbConnection CustomerDBConnection
        {
            get
            {
                return _customerDBConnection;
            }
        }

        #endregion Properties

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="URSDBConnectionManager"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the <see cref="URSDBConnectionManager"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param> 
        protected virtual void Dispose(bool disposing)
        {
            // Dispose of managed objects.
            if (disposing)
            {
                if (_ursDbConnectionInfo != null)
                {
                    _ursDbConnectionInfo.Dispose();
                    _ursDbConnectionInfo = null;
                }
            }
        }

        #endregion IDisposable Members

        #region Private Methods

        /// <summary>
        /// Gets the complete path and name of URS database.
        /// </summary>
        /// <value>
        /// The complete path and name of URS database.
        /// </value>
        string UrsDBPath
        {
            get
            {
                try
                {
                    if (String.IsNullOrWhiteSpace(_ursDBPath))
                    {
                        _ursDBPath = Path.Combine(GetComponentDataDir(), _URS_DB_RELATIVE_LOCATION);
                    }

                    return _ursDBPath;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39311");
                }
            }
        }

        /// <summary>
        /// Gets path of the URS Folder that matches the FKBVersion from the customer OrderMappingDB.
        /// </summary>
        /// <returns>The path of the URS Folder for.</returns>
        string GetComponentDataDir()
        {
            if (DBMethods.GetTableNames(_customerDBConnection).Count(name => name == "Settings") == 1)
            {
                ExtractException ee = new ExtractException("ELI39307", "Settings table not found");
                ee.AddDebugData("Database", _customerDBConnection.ConnectionString, false);
                throw ee;
            }

            string FKBVersion = DBMethods.GetQueryResultsAsStringArray(_customerDBConnection,
                "SELECT [Value] FROM [Settings] WHERE [Name] = 'FKBVersion'")
                .SingleOrDefault();
            if (String.IsNullOrWhiteSpace(FKBVersion))
            {
                ExtractException ee = new ExtractException("ELI39308", 
                    "Could not retrieve valid (non-empty) FKBVersion from Settings table");
                ee.AddDebugData("Database", _customerDBConnection.ConnectionString, false);
                throw ee;
            }

            try
            {
                var afUtility = new AFUtility();
                return afUtility.GetComponentDataFolder2(FKBVersion, _alternateComponentDataDir);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39309");
            }
        }

        #endregion Private Methods
    }
}
