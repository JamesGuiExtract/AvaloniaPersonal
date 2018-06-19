using Extract.Code.Attributes;
using Extract.Utilities;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Transactions;
using System.Windows.Forms;

namespace Extract.ETL
{
    [DataContract]
    [KnownType(typeof(ScheduledEvent))]
    [ExtractCategory("DatabaseService", "HIM stats service")]
    public class HIMStats : DatabaseService, IConfigSettings
    {
        #region Constants

        readonly string _UpdateQuery = @"
            DELETE FROM ReportingHIMStats
            FROM ReportingHIMStats AS rpt LEFT JOIN
            (
            	SELECT DISTINCT PaginationID
            
            	FROM vPaginationDataWithRank inner join vPaginatedDestFiles ON vPaginatedDestFiles.DestFileID = vPaginationDataWithRank.DestFileID
            	WHERE RankDesc = 1
            ) AS newData ON rpt.PaginationID = newData.PaginationID
            WHERE newData.PaginationID  is null
            
            
            INSERT INTO ReportingHIMStats
            SELECT DISTINCT vPaginationDataWithRank.PaginationID
            	, vPaginationDataWithRank.FAMUserID
            	, vPaginationDataWithRank.SourceFileID
            	, vPaginationDataWithRank.DestFileID
            	, vPaginationDataWithRank.OriginalFileID
            	, CAST([DateTimeStamp] AS DATE) AS [DateProcessed]
            	, vPaginationDataWithRank.ActionID
            	, vPaginationDataWithRank.FileTaskSessionID
            	, vPaginationDataWithRank.ASCName 
            FROM vPaginationDataWithRank inner join vPaginatedDestFiles ON vPaginatedDestFiles.DestFileID = vPaginationDataWithRank.DestFileID
            	LEFT join ReportingHIMStats ON vPaginationDataWithRank.PaginationID = ReportingHIMStats.PaginationID
            WHERE RankDesc = 1 AND ReportingHIMStats.PaginationID IS NULL";

        #endregion

        #region Fields

        /// <summary>
        /// Current version
        /// </summary>
        const int CURRENT_VERSION = 1;

        /// <summary>
        /// Indicates whether the Process method is currently executing.
        /// </summary>
        bool _processing;

        #endregion Fields

        #region DatabaseService Properties

        /// <summary>
        /// Gets a value indicating whether this instance is processing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if processing; otherwise, <c>false</c>.
        /// </value>
        public override bool Processing
        {
            get
            {
                return _processing;
            }
        }

        #endregion DatabaseService Properties

        #region HIMStats Properties

        [DataMember]
        public override int Version { get; protected set; } = CURRENT_VERSION;

        #endregion

        #region IConfigSettings implementation

        /// <summary>
        /// Method returns the state of the configuration
        /// </summary>
        /// <returns>Returns <see langword="true"/> if configuration is valid, otherwise false</returns>

        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(Description);
        }

        /// <summary>
        /// Displays a form to Configures the HIMStats service
        /// </summary>
        /// <returns><see langword="true"/> if configuration was ok'd. if configuration was canceled returns 
        /// <see langword="false"/></returns>
        public bool Configure()
        {
            try
            {
                HIMStatsForm form = new HIMStatsForm(this);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46056");
                return false;
            }
        }

        #endregion

        #region DatabaseService Methods

        /// <summary>
        /// Performs the process of populating the ReportingHIMStats table in the database
        /// </summary>
        /// <param name="cancelToken">Token that can cancel the processing</param>
        public override void Process(CancellationToken cancelToken)
        {
            try
            {
                _processing = true;

                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    using (var scope = new TransactionScope())
                    {
                        cmd.CommandText = _UpdateQuery;
                        cmd.CommandTimeout = 600;
                        var task = cmd.ExecuteNonQueryAsync(cancelToken);
                        var result = task.Result;

                        // if records were affected then complete the transaction 
                        if (result > 0)
                        {
                            scope.Complete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46059");
            }
            finally
            {
                _processing = false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Called after this instance is deserialized.
        /// </summary>
        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (Version > CURRENT_VERSION)
            {
                ExtractException ee = new ExtractException("ELI46058", "Settings were saved with a newer version.");
                ee.AddDebugData("SavedVersion", Version, false);
                ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                throw ee;
            }

            Version = CURRENT_VERSION;
        }

        #endregion

    }
}
