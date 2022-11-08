using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;

namespace Extract.ErrorHandling
{
    [Serializable]
    public class ApplicationStateInfo : ISerializable
    {

        public ApplicationStateInfo()
        {
            var currentProcess = Process.GetCurrentProcess();
            ApplicationVersion = currentProcess.MainModule.FileVersionInfo.FileVersion
                .Replace(" ,", ",")
                .Replace(',', '.');
            ApplicationName = currentProcess.MainModule.ModuleName;
            ComputerName = Environment.MachineName;
            UserName = WindowsIdentity.GetCurrent().Name.Split('\\').Last();
            PID = currentProcess.Id;
        }

        internal ApplicationStateInfo(SerializationInfo info, StreamingContext context) : this()
        {
            var infoDictionary = info.ToDictionary();
            PID = infoDictionary.ContainsKey("PID") ? info.GetInt32("PID") : PID;

            ComputerName =
                infoDictionary.ContainsKey("ComputerName") ? info.GetString("ComputerName") : ComputerName;

            ApplicationName =
                infoDictionary.ContainsKey("ApplicationName") ? info.GetString("ApplicationName") : ApplicationName;

            ApplicationVersion =
                infoDictionary.ContainsKey("ApplicationVersion") ? info.GetString("ApplicationVersion") : ApplicationVersion;

            UserName = infoDictionary.ContainsKey("UserName") ? info.GetString("UserName") : UserName;
        }
        
        public string ApplicationName { get; set; }
        public string ApplicationVersion { get; set; }
        public string ComputerName { get; set; }
        public string UserName { get; set; }
        public Int32 PID { get; set; }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("PID", PID);
            info.AddValue("ApplicationName", ApplicationName);
            info.AddValue("ApplicationVersion", ApplicationVersion);
            info.AddValue("ComputerName", ComputerName);
            info.AddValue("UserName", UserName);
        }
    }
}
