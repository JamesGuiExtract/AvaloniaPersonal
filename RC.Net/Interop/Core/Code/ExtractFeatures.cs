
namespace Extract.Interop
{
    /// <summary>
    /// The names of features that can be enabled, disabled or restricted via the FAM DB.
    /// </summary>
    public static class ExtractFeatures
    {
        /// <summary>
        /// Allows filenames to be copied as text from a file list.
        /// </summary>
        public static readonly string FileHandlerCopyNames = "Files: Copy filenames";

        /// <summary>
        /// Allows documents to be copied or dragged as files from a file list.
        /// </summary>
        public static readonly string FileHandlerCopyFiles = "Files: Copy files";

        /// <summary>
        /// Allows documents and associated data to be copied or dragged as files from a file list.
        /// </summary>
        public static readonly string FileHandlerCopyFilesAndData = "Files: Copy files and data";

        /// <summary>
        /// Allows the containing folder of document to be opened in Windows file explorer.
        /// </summary>
        public static readonly string FileHandlerOpenFileLocation = "Files: Open file location";

        /// <summary>
        /// Allows document specific reports to be run.
        /// </summary>
        public static readonly string RunDocumentSpecificReports = "Reports: Run document specific";
    }
}
