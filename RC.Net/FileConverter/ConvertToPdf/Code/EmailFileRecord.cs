using System;
using System.Runtime.InteropServices;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileConverter.ConvertToPdf
{
    /// <summary>
    /// Data for an email that is being split/converted
    /// </summary>
    [CLSCompliant(false)]
    [ComVisible(false)]
    public class EmailFileRecord
    {
        /// <summary>
        /// Create an instance with only a FilePath
        /// </summary>
        /// <param name="filePath"></param>
        public EmailFileRecord(string filePath)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Create an instance from a <see cref="IFileRecord"/>
        /// </summary>
        public EmailFileRecord(IFileRecord fileRecord)
        {
            _ = fileRecord ?? throw new ArgumentNullException(nameof(fileRecord));

            FilePath = fileRecord.Name;
            FileID = fileRecord.FileID;
            ActionID = fileRecord.ActionID;
            Priority = fileRecord.Priority;
            WorkflowID = fileRecord.WorkflowID;
        }

        /// <summary>
        /// The database ID of the email file
        /// </summary>
        public int FileID { get; }

        /// <summary>
        /// The path to the email file
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// The ID of the database Action that is being run
        /// </summary>
        public int ActionID { get; }

        /// <summary>
        /// The priority level of the email file
        /// </summary>
        public EFilePriority Priority { get; }


        /// <summary>
        /// The ID of the workflow that is being run
        /// </summary>
        public int WorkflowID { get; }
    }
}
