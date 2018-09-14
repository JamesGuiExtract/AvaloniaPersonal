using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;


namespace Extract.ETL
{

    /// <summary>
    /// Interface to implement if DatabaseService uses LastFileTaskSessionID
    /// This interface is used to save the LastFileTaskSessionIDProcessed to the DatabaseService record 
    /// </summary>
    public interface IFileTaskSessionServiceStatus
    {
        int LastFileTaskSessionIDProcessed { get; set; }
    }

    /// <summary>
    /// Defines the base class for database service status
    /// </summary>
    [DataContract]
    public abstract class DatabaseServiceStatus
    {
        /// <summary>
        /// The version
        /// </summary>
        [DataMember]
        public abstract int Version { get; protected set; }

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
                throw ex.AsExtract("ELI45469");
            }
        }

        /// <summary>
        /// Deserializes a <see cref="DatabaseServiceStatus"/> instance from a JSON string
        /// </summary>
        /// <param name="settings">The JSON string to which a <see cref="DatabaseServiceStatus"/> was is saved</param>
        public static DatabaseServiceStatus FromJson(string status)
        {
            try
            {
                return (DatabaseServiceStatus)JsonConvert.DeserializeObject(status,
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45470");
            }
        }

        /// <summary>
        /// Saves the status to the given connection with the given transaction to the record with the given database service id
        /// </summary>
        /// <param name="connection">An open <see cref="SqlConnection"/> to the database to save the status to</param>
        /// <param name="databaseServiceId">The ID of the DatabaseService record to update</param>
        public void SaveStatus(SqlConnection connection, Int32 databaseServiceId)
        {
            try
            {
                var fileTaskSessionStatus = this as IFileTaskSessionServiceStatus;
                int? lastFileTaskSession = fileTaskSessionStatus?.LastFileTaskSessionIDProcessed;
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE [DatabaseService]
                        SET [Status] = @Status,
                            [LastFileTaskSessionIDProcessed] = @LastFileTaskSession,
                            [LastWrite] = GetDate()
                        WHERE ID = @DatabaseServiceID";
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = this.ToJson();
                    cmd.Parameters.Add("@DatabaseServiceID", SqlDbType.Int).Value = databaseServiceId;
                    if (lastFileTaskSession is null)
                    {
                        cmd.Parameters.AddWithValue("@LastFileTaskSession", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@LastFileTaskSession", lastFileTaskSession);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45492");
            }
        }
    }
}
