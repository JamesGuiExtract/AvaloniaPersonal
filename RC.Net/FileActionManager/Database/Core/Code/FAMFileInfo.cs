namespace Extract.FileActionManager.Database
{
    public class FAMFileInfo
    {
        public FAMFileInfo(string filePath, long fileSize, int pageCount, int? workflowID)
        {
            FilePath = filePath;
            FileSize = fileSize;
            PageCount = pageCount;
            WorkflowID = workflowID;
        }

        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public int PageCount { get; set; }
        public int? WorkflowID { get; set; }
    }
}
