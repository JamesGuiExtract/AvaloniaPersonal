namespace Extract.Utilities.ReactiveUI
{
    public sealed class OkWindowMessage
    {
        public static OkWindowMessage Instance { get; } = new();

        private OkWindowMessage()
        {

        }
    }

    public sealed class CloseWindowMessage
    {
        public static CloseWindowMessage Instance { get; } = new();

        private CloseWindowMessage()
        {

        }
    }
}
