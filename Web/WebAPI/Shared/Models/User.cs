namespace WebAPI
{
    /// <summary>
    /// Information needed to authenticate an API user
    /// </summary>
    public class User
    {
        /// <summary>
        /// User name
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// User password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Optional user-specified workflow name that overrides the default workflow
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// Optional user-specified configuration name that overrides the default configuration
        /// (Takes priority over workflow name).
        /// </summary>
        public string ConfigurationName { get; set; }
    }
}
