using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    /// <summary>
    /// Data model for a login response.
    /// </summary>
    public class LoginToken
    {
        /// <summary>
        /// The JWT Bearer token to identify a caller in subsequent API calls
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// The number of seconds before this token expires
        /// </summary>
        public int expires_in { get; set; }
    }
}
