﻿namespace WebAPI.Models
{
    /// <summary>
    /// type of the document submission - either file or text
    /// Note that this information gets incorporated into the DocumentSubmitResult.Id
    /// </summary>
    public enum DocumentSubmitType
    {
        /// <summary>
        /// submission was a file
        /// </summary>
        File,

        /// <summary>
        /// submission was text (converted into a file)
        /// </summary>
        Text
    }

    /// <summary>
    /// This class is used to return File or Text submission result
    /// </summary>
    public class DocumentSubmitResult : IResultData
    {
        /// <summary>
        /// The identifier for the submitted file
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// error info
        /// </summary>
        public ErrorInfo Error { get; set; }
    }
}
