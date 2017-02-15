namespace DocumentAPI.Models
{
    /// <summary>
    /// This class represents a Claim - a name and value pair used for authorization
    /// </summary>
    public class Claim
    {
        /// <summary>
        /// error information
        /// </summary>
        public ErrorInfo Error { get; set; }

        /// <summary>
        /// The name of the claim
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the claim
        /// </summary>
        public string Value { get; set; }
    }
}
