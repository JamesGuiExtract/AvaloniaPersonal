using Extract.Utilities;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace Extract.ETL
{
    /// <summary>
    /// Defines the base class for processes that will be performed by a service
    /// </summary>
    [DataContract]
    [KnownType(typeof(ScheduledEvent))]
    public abstract class DatabaseService : IDisposable
    {
        bool _enabled = true;

        /// <summary>
        /// Description of the database service item
        /// </summary>
        [DataMember]
        public string Description { get; set; } = "";

        /// <summary>
        /// Name of the database. This value is not included in the settings
        /// </summary>
        public string DatabaseName { get; set; } = "";

        /// <summary>
        /// Name of the Server. This value is not included in the settings
        /// </summary>
        public string DatabaseServer { get; set; } = "";

        /// <summary>
        /// This is the id from the DatabaseService table.  This value is not included in the settings
        /// </summary>
        public int DatabaseServiceID { get; set; }

        [DataMember]
        public ScheduledEvent Schedule { get; set; }

        /// <summary>
        /// Whether enabled
        /// </summary>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                try
                {
                    if (value != _enabled)
                    {
                        _enabled = value;
                        if (Schedule != null)
                        {
                            Schedule.Enabled = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45418");
                }
            }
        }

        /// <summary>
        /// Whether processing
        /// </summary>
        public abstract bool Processing { get; }

        /// <summary>
        /// The version
        /// </summary>
        [DataMember]
        public abstract int Version { get; protected set; }

        /// <summary>
        /// Performs the processing defined the database service record
        /// </summary>
        public abstract void Process();

        /// <summary>
        /// Returns the settings in a JSON string
        /// </summary>
        public string ToJson()
        {
            try
            {
                return JsonConvert.SerializeObject(this,
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects, Formatting = Formatting.Indented });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45368");
            }
        }

        /// <summary>
        /// Deserializes a <see cref="DatabaseService"/> instance from a JSON string
        /// </summary>
        /// <param name="settings">The JSON string to which a <see cref="DatabaseService"/> was previously saved</param>
        public static DatabaseService FromJson(string settings)
        {
            try
            {
                return (DatabaseService)JsonConvert.DeserializeObject(settings,
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45387");
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="NERDataCollector"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="NERDataCollector"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                Schedule?.Dispose();
                Schedule = null;
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Helper members

        /// <summary>
        /// Returns a connection to the configured database. Can be overridden if needed
        /// </summary>
        /// <returns>SqlConnection that connects to the <see cref="DatabaseServer"/> and <see cref="DatabaseName"/></returns>
        protected virtual SqlConnection getNewSqlDbConnection()
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = DatabaseServer;
            sqlConnectionBuild.InitialCatalog = DatabaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            return new SqlConnection( sqlConnectionBuild.ConnectionString);
        }

        #endregion
    }
}
