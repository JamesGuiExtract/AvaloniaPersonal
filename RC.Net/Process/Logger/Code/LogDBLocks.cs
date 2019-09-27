﻿using System;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;

namespace Extract.Process.Logger
{
    /// <summary>
    /// Com LogDBLogs class
    /// </summary>
    [ComVisible(true)]
    [Guid("0ED53D5C-F26F-465E-8E0E-A3FC33C640C3")]
    [ProgId("Extract.Process.Logger.LogDBLocks")]
    [CLSCompliant(false)]
    public class LogDBLocks : LogDBInfoBase
    {
        #region Constants

        static readonly string LockQuery = @"
SELECT 
       SessionId    = s.session_id, 
       UserProcess  = CONVERT(CHAR(1), s.is_user_process),
       LoginInfo    = s.login_name,   
       DbInstance   = ISNULL(db_name(r.database_id), N''), 
       TaskState    = ISNULL(t.task_state, N''), 
       Command      = ISNULL(r.command, N''), 
       App            = ISNULL(s.program_name, N''), 
       WaitTime_ms  = ISNULL(w.wait_duration_ms, 0),
       WaitType     = ISNULL(w.wait_type, N''),
       WaitResource = ISNULL(w.resource_description, N''), 
       BlockBy        = ISNULL(CONVERT (varchar, w.blocking_session_id), ''),
       HeadBlocker  = 
            CASE 
                -- session has active request; is blocked; blocking others
                WHEN r2.session_id IS NOT NULL AND r.blocking_session_id = 0 THEN '1' 
                -- session idle; has an open tran; blocking others
                WHEN r.session_id IS NULL THEN '1' 
                ELSE ''
            END, 
       TotalCPU_ms        = s.cpu_time, 
       TotalPhyIO_mb    = (s.reads + s.writes) * 8 / 1024, 
       MemUsage_kb        = s.memory_usage * 8192 / 1024, 
       OpenTrans        = ISNULL(r.open_transaction_count,0), 
       LoginTime        = s.login_time, 
       LastReqStartTime = s.last_request_start_time,
       HostName            = ISNULL(s.host_name, N''),
       NetworkAddr        = ISNULL(c.client_net_address, N''), 
       ExecContext        = ISNULL(t.exec_context_id, 0),
       ReqId            = ISNULL(r.request_id, 0),
       WorkLoadGrp        = N'',
       LastCommandBatch = (select text from sys.dm_exec_sql_text(c.most_recent_sql_handle)) 
    FROM sys.dm_exec_sessions s LEFT OUTER JOIN sys.dm_exec_connections c ON (s.session_id = c.session_id)
    LEFT OUTER JOIN sys.dm_exec_requests r ON (s.session_id = r.session_id)
    LEFT OUTER JOIN sys.dm_os_tasks t ON (r.session_id = t.session_id AND r.request_id = t.request_id)
    LEFT OUTER JOIN 
    (
        -- Using row_number to select longest wait for each thread, 
        -- should be representative of other wait relationships if thread has multiple involvements. 
        SELECT *, ROW_NUMBER() OVER (PARTITION BY waiting_task_address ORDER BY wait_duration_ms DESC) AS row_num
        FROM sys.dm_os_waiting_tasks 
    ) w ON (t.task_address = w.waiting_task_address) AND w.row_num = 1
    LEFT OUTER JOIN sys.dm_exec_requests r2 ON (r.session_id = r2.blocking_session_id)
    OUTER APPLY sys.dm_exec_sql_text(r.sql_handle) as st

    WHERE s.session_Id > 50                         -- ignore anything pertaining to the system spids.

    AND s.session_Id NOT IN (@@SPID)     -- let's avoid our own query! :)

	and  (s.program_name = 'ProcessFiles Application' or s.program_name = 'FAMProcess') and w.wait_type  like 'LCK%'";

        static readonly string LockCountQuery = @"WITH DBLocks AS (" + LockQuery +
            @")
SELECT GETDATE() TimeStamp, COUNT(SessionId) LockCount FROM DBLocks";

        #endregion

        #region Constructors

        /// <summary>
        /// Default contructor
        /// </summary>
        public LogDBLocks() :
            base()
        {
        }

        /// <summary>
        /// Constructor that initiallizes all properties
        /// </summary>
        /// <param name="logDirectory">The directory for the log file </param>
        /// <param name="databaseServer">The Database server containing the database to log locks from</param>
        /// <param name="databaseName">The database on the server to log the locks</param>
        /// <param name="pollingTime">Time between log entries</param>
        public LogDBLocks(string logDirectory, string logFileName, string databaseServer, string databaseName, int pollingTime) :
            base(logDirectory, logFileName, databaseServer, databaseName, pollingTime)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Query that returns the data to be logged
        /// NOTE: All columns returned should be logged
        /// </summary>
        protected override string InfoQuery
        {
            get
            {
                return LockCountQuery;
            }
        }

        #endregion

    }
}
