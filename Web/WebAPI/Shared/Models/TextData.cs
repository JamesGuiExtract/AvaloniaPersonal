namespace WebAPI
{
    /// <summary>
    /// Represents text of a request argument or return value.
    /// </summary>
    public class TextData
    {
        /// <summary>
        /// The text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Implicit conversion from string
        /// </summary>
        public static implicit operator TextData(string text)
        {
            var args = new TextData();
            args.Text = text;
            return args;
        }
    }
}
