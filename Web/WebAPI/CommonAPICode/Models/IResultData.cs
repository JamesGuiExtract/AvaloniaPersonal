
namespace WebAPI.Models
{
    /// <summary>
    /// Common interface for all API call return values.
    /// </summary>
    public interface IResultData
    {
        /// <summary>
        /// Error info - Error == true if there has been an error
        /// </summary>
        ErrorInfo Error { get; set; }
    }
}
