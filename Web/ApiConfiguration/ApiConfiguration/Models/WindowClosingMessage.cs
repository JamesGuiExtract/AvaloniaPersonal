namespace Extract.Web.ApiConfiguration.Models
{
    public sealed class WindowClosingMessage
    {
        public static WindowClosingMessage Instance { get; } = new();

        private WindowClosingMessage()
        {

        }
    }
}
