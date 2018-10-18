
namespace WebAPI.Models
{
    /// <summary>
    /// A return value with ErrorInfo describing the reason for a failed call.
    /// </summary>
    public class ErrorResult
    {
        /// <summary>
        /// The ErrorInfo describing the error
        /// </summary>
        public ErrorInfo Error { get; set; } = new ErrorInfo();
    }
}
