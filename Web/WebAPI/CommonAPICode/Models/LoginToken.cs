using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class LoginToken
    {
        /// <summary>
        /// 
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// The <see cref="ErrorInfo"/>.
        /// </summary>
        public int expires_in { get; set; }
    }
}
