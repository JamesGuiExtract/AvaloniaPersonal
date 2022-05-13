using Extract.SqlDatabase;
using System;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// Reimplementation of some FileProcessingDB functionality
    /// to enable FAMFile operations to take place within a .NET transaction
    /// </summary>
    [CLSCompliant(false)]
    public partial class FAMFileUtility
    {
        const int COMMAND_TIMEOUT = 300;
        private readonly ExtractRoleConnection _databaseConnection;

        /// <summary>
        /// Create an instance using the provided, open, database connection
        /// </summary>
        public FAMFileUtility(ExtractRoleConnection fileProcessingDatabaseConnection)
        {
            _databaseConnection = fileProcessingDatabaseConnection;
        }

        /// <summary>
        /// Whether the specified file exists in the FAMFile table
        /// </summary>
        public bool IsFileInDatabase(string fileName)
        {
            try
            {
                using var command = _databaseConnection.CreateCommand();
                command.CommandText = "SELECT ID FROM dbo.FAMFile WHERE [FileName] = @FileName";
                command.Parameters.AddWithValue("@FileName", fileName);
                command.CommandTimeout = COMMAND_TIMEOUT;

                return command.ExecuteScalar() is not null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53410");
            }
        }

        /// <summary>
        /// Add a file to the database but do not set any action to pending
        /// </summary>
        /// <param name="fileInfo">The details about the file to add</param>
        /// <returns>The FAMFile.ID of the record that was added</returns>
        /// <remarks>
        /// This is mostly a port of FileProcessingDB.AddFileNoQueue but this version does not
        /// add QueueEvent records.
        /// </remarks>
        public int AddFileNoQueue(FAMFileInfo fileInfo)
        {
            try
            {
                int fileID = -1;

                using (var command = _databaseConnection.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO dbo.FAMFile ([FileName], [FileSize], [Pages])" +
                        " OUTPUT INSERTED.[ID] VALUES (@FileName, @FileSize, @PageCount)";
                    command.Parameters.AddWithValue("@FileName", fileInfo.FilePath);
                    command.Parameters.AddWithValue("@FileSize", fileInfo.FileSize);
                    command.Parameters.AddWithValue("@PageCount", fileInfo.PageCount);
                    command.CommandTimeout = COMMAND_TIMEOUT;

                    fileID = (int)command.ExecuteScalar();
                }

                if (fileInfo.WorkflowID.HasValue)
                {
                    using var command = _databaseConnection.CreateCommand();
                    command.CommandText =
                        "IF NOT EXISTS dbo.WorkflowFile WHERE WorkflowID = @WorkflowID AND FileID = @FileID" +
                        " BEGIN INSERT INTO dbo.WorkflowFile ([WorkflowID], [FileID]) VALUES (@WorkflowID, @FileID) END";
                    command.Parameters.AddWithValue("@WorkflowID", fileInfo.WorkflowID);
                    command.Parameters.AddWithValue("@FileID", fileID);
                    command.CommandTimeout = COMMAND_TIMEOUT;

                    command.ExecuteNonQuery();
                }

                return fileID;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53411");
            }
        }
    }
}
