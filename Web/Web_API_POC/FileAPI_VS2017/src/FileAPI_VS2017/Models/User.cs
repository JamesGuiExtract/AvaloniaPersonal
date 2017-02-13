using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileAPI_VS2017.Models
{
    /// <summary>
    /// User model
    /// </summary>
    public class User
    {
        /// <summary>
        /// user name
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// user password
        /// </summary>
        public string Password { get; set; }

        /*
        /// <summary>
        /// Identifier value used by Get by Id
        /// </summary>
        public int Id { get; set; }
        */

        /// <summary>
        /// the claims associated with this user
        /// </summary>
        public List<Claim> Claims = new List<Claim>();

        /// <summary>
        /// tracks whether this user is logged in or not.
        /// </summary>
        public bool LoggedIn { get; set; }
    }
}
