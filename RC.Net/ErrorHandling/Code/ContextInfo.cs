using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;

namespace Extract.ErrorHandling
{
    [Serializable]
    public class ContextInfo : ISerializable
    {

        public ContextInfo()
        {
            var currentProcess = Process.GetCurrentProcess();
            ApplicationVersion = currentProcess.MainModule.FileVersionInfo.FileVersion
                .Replace(" ,", ",")
                .Replace(',', '.');
            ApplicationName = currentProcess.MainModule.ModuleName;
            MachineName = Environment.MachineName;
            UserName = WindowsIdentity.GetCurrent().Name.Split('\\').Last();
            PID = (UInt32)currentProcess.Id;
            FileID = 0;
            ActionID = 0;
            DatabaseServer = "";
            DatabaseName = "";
            FpsContext = "";
        }

        public ContextInfo(Int32 fileID, Int32 actionID, string databaseServer, string databaseName, string fpsContext)
            : this()
        {
            FileID = fileID;
            ActionID = actionID;
            DatabaseServer = databaseServer;
            DatabaseName = databaseName;
            FpsContext = fpsContext;
        }

        internal ContextInfo(SerializationInfo info, StreamingContext context) : this()
        {
            var infoDictionary = info.ToDictionary();
            PID = infoDictionary.ContainsKey("PID") ? info.GetUInt32("PID") : PID;

            MachineName =
                infoDictionary.ContainsKey("ComputerName") ? info.GetString("ComputerName") : 
                infoDictionary.ContainsKey("MachineName") ? info.GetString("MachineName") : MachineName;

            ApplicationName =
                infoDictionary.ContainsKey("ApplicationName") ? info.GetString("ApplicationName") : ApplicationName;

            ApplicationVersion =
                infoDictionary.ContainsKey("ApplicationVersion") ? info.GetString("ApplicationVersion") : ApplicationVersion;

            UserName = infoDictionary.ContainsKey("UserName") ? info.GetString("UserName") : UserName;
            
            FileID = infoDictionary.ContainsKey("FileID") ? info.GetInt32("FileID") : FileID;
            
            ActionID = infoDictionary.ContainsKey("ActionID") ? info.GetInt32("ActionID") : ActionID;

            DatabaseServer = infoDictionary.ContainsKey("DatabaseServer") ? info.GetString("DatabaseServer") : DatabaseServer;

            DatabaseName = infoDictionary.ContainsKey("DatabaseName") ? info.GetString("DatabaseName") : DatabaseName;

            FpsContext = infoDictionary.ContainsKey("FpsContext") ? info.GetString("FpsContext") : FpsContext;
        }
        
        public string ApplicationName { get; set; }
        public string ApplicationVersion { get; set; }
        public string MachineName { get; set; }
        public string UserName { get; set; }
        public UInt32 PID { get; set; }
        public Int32 FileID { get; set; }
        public Int32 ActionID { get; set; }
        public string DatabaseServer { get; set; }
        public string DatabaseName { get; set; }
        public string FpsContext { get; set; }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("PID", PID);
            info.AddValue("ApplicationName", ApplicationName);
            info.AddValue("ApplicationVersion", ApplicationVersion);
            info.AddValue("MachineName", MachineName);
            info.AddValue("UserName", UserName);
            info.AddValue("FileID", FileID);
            info.AddValue("ActionID", ActionID);
            info.AddValue("DatabaseServer", DatabaseServer);
            info.AddValue("DatabaseName", DatabaseName);
            info.AddValue("FpsContext", FpsContext);
        }
    }
}
