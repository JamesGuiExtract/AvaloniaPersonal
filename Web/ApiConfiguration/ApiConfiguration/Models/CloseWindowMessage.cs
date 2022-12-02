namespace Extract.Web.ApiConfiguration.Models
{
    public sealed class CloseWindowMessage
    {
        public static CloseWindowMessage Instance { get; } = new();

        private CloseWindowMessage()
        {

        }
    }
}
