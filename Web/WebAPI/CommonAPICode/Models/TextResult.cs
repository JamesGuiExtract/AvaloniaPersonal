using System;

namespace WebAPI.Models
{
    /// <summary>
    /// DTO for result of GetTextResult
    /// </summary>
    public class TextResult : IResultData
    {
        /// <summary>
        /// the Text of the result
        /// </summary>
        public String Text { get; set; }

        /// <summary>
        /// error info
        /// </summary>
        public ErrorInfo Error { get; set; }
    }
}
