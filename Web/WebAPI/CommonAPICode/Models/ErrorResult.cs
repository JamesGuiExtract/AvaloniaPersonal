
namespace WebAPI.Models
{
    /// <summary>
    /// An API return value with <see cref="ErrorInfo"/> regarding and error that occurred.
    /// </summary>
    public class ErrorResult
    {
        /// <summary>
        /// The <see cref="ErrorInfo"/> describing the error.
        /// </summary>
        public ErrorInfo Error { get; set; } = new ErrorInfo();
    }
}
