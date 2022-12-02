namespace WebAPI
{
    /// <summary>
    /// Returned in response to a successful login
    /// </summary>
    public class LoginToken
    {
        /// <summary>
        /// The JSON web (bearer) token to identify a caller in subsequent API calls
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// The number of seconds before this token expires
        /// </summary>
        public int expires_in { get; set; }
    }
}
