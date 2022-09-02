namespace Extract.GdPicture
{
    public class DebugData
    {
        public string? FilePath { get; }
        public int? PageNumber { get; }

        public DebugData(string? filePath = null, int? pageNumber = null)
        {
            FilePath = filePath;
            PageNumber = pageNumber;
        }
    }
}
