using System;
using System.IO;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors.Test
{
    /// <summary>
    /// Provides utility methods for testing <see cref="IFileProcessingTask"/> functionality within
    /// unit tests.
    /// </summary>
    static internal class FileProcessingTaskTester
    {
        /// <summary>
        /// Executes the specified <see paramref="task"/> against the specified
        /// <see paramref="sourceDocName"/>.
        /// <para><b>Note</b></para>
        /// The task will run without a <see cref="IFileProcessingDB"/>; If the task requires a FAM
        /// DB to run, this call will throw an exception.
        /// </summary>
        /// <param name="task">The <see cref="IFileProcessingTask"/> to execute.</param>
        /// <param name="sourceDocName">The name of the file the task should be run against.</param>
        /// <returns>An <see cref="EFileProcessingResult"/> representing the result of the execution.
        /// </returns>
        public static EFileProcessingResult Execute(this IFileProcessingTask task,
            string sourceDocName)
        {
            try
            {
                // Setup file record for call to InitProcessClose
                var fileRecord = new FileRecordClass();
                fileRecord.Name = sourceDocName;
                fileRecord.FileID = 0;

                // Push the task into a task list for task executor
                var owd = new ObjectWithDescription();
                owd.Object = task;
                var tasks = new IUnknownVector();
                tasks.PushBack(owd);

                // Generate a TagManager based on dummy "Test.fps" file name in the same directory
                // as the source document.
                var tagManager = new FAMTagManagerClass();
                tagManager.FPSFileDir = Path.GetDirectoryName(sourceDocName);
                tagManager.FPSFileName = Path.Combine(tagManager.FPSFileDir, "Test.fps");

                // Use a local task executor to directly execute the file processing
                // tasks.
                var taskExecutor = new FileProcessingTaskExecutorClass();

                return taskExecutor.InitProcessClose(fileRecord, tasks, 0, null, tagManager, null,
                    null, false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38402");
            }
        }
    }
}
