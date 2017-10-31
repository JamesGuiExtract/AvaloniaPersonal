
namespace WebAPI.Models
{
    /// <summary>
    /// Represents an API return value with nothing except <see cref="ErrorInfo"/>.
    /// </summary>
    public class GenericResult
    {
        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public ErrorInfo Error { get; set; }
    }
}
