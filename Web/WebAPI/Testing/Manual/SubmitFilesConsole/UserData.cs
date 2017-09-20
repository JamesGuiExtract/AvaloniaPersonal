namespace SubmitFilesConsole
{
    /// <summary>
    /// User model
    /// </summary>
    public class UserData
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
        /// returns this object as a JSON formatted string
        /// </summary>
        /// <returns>json formatted string</returns>
        public string ToJsonString()
        {
            return $"{{\n\"username\": \"{Username}\",\n\"password\": \"{Password}\",\n\"workflowname\": \"{WorkflowName}\"\n}}";
        }
    }
}
