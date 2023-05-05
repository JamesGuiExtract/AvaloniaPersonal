using Nest;
using System;

namespace Extract.ErrorsAndAlerts.ElasticDTOs
{
    public class ContextInfoDto
    {
        [PropertyName("application_name")]
        public string ApplicationName { get; set; } = "";

        [PropertyName("application_version")]
        public string ApplicationVersion { get; set; } = "";

        [PropertyName("machine_name")]
        public string? MachineName { get; set; }

        [PropertyName("database_server")]
        public string? DatabaseServer { get; set; }

        [PropertyName("database_name")]
        public string? DatabaseName { get; set; }

        [PropertyName("fps_context")]
        public string? FpsContext { get; set; }

        [PropertyName("user_name")]
        public string UserName { get; set; } = "";

        [PropertyName("pid")]
        public UInt32 PID { get; set; }

        [PropertyName("file_id")]
        public Int32 FileID { get; set; }

        [PropertyName("action_id")]
        public Int32 ActionID { get; set; }
    }
}
