using System.Collections.Generic;

namespace DocumentAPI.Models
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

        /// <summary>
        /// user-specified workflow name that overrides the default workflow
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// the claims associated with this user
        /// </summary>
        public List<Claim> Claims = new List<Claim>();
    }
}
