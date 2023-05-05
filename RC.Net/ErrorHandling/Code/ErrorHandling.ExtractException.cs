using Newtonsoft.Json;
using NLog;
using NLog.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using static System.Environment;

namespace Extract.ErrorHandling
{
    [Serializable]
    public class ExtractException : Exception, IExtractException
    {
        static readonly string k = "D01CD545CA432267312CCF12726A7E40";
        static readonly string s = "0F36A22E0F36A22E0F36A22E0F36A22E";

        static readonly string Default_UEX_FileName = "ExtractException.uex";

        internal static JsonSerializerSettings _serializeSettings =
           new JsonSerializerSettings
           {
               Formatting = Newtonsoft.Json.Formatting.None,
               Converters = new List<JsonConverter>()
                {
                    new ExceptionData.ExceptionDataJsonConverter()
                }
           };

        internal static JsonSerializerSettings _deserializeSettings =
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                Converters = new List<JsonConverter>()
                {
                    new ExceptionData.ExceptionDataJsonConverter()
                }
            };

        public LogLevel LoggingLevel { get; set; } = LogLevel.Error;

        private static readonly ILogger _defaultLogger = InitializeLoggerFromConfig();
        
        public ILogger Logger
        {
            get;
            internal set;
        } = _defaultLogger;

        static ExceptionSettings _settings = new ExceptionSettings();

        public static bool UseNetLogging
        {
            get =>
            _settings.UseNetLogging;
        }

        static private void VerifyCaller(AssemblyName callingAssemblyName)
        {
            string callerPublicKey = callingAssemblyName.GetPublicKey().ToHexString(true);

            if (!callerPublicKey.Equals(Constants.ExtractPublicKey))
            {
                throw new ExtractException("ELI53609", "Invalid Caller");
            }
        }

        static internal byte[] CreateK()
        {
            VerifyCaller(Assembly.GetCallingAssembly().GetName());
            var sb = s.FromHexString();
            var kb = k.FromHexString();

            ExtractException.Assert("ELI53607", "String invalid length.", sb.Length == kb.Length);

            for (int i = 0; i < sb.Length; i++)
            {
                kb[i] = (byte)(kb[i] ^ sb[i]);
            }
            return kb;
        }

        // Our code depends on these values being in this order, if adding a new value, add
        // it to the end of the list
        public enum EType : UInt32
        {
            kString,
            kOctets,
            kInt,
            kLong,
            kUnsignedLong,
            kDouble,
            kBoolean,
            kNone,
            kInt64,
            kInt16,
            kDateTime,
            kGuid
        };

        const string ExceptionSignature = "1f000000";
        const string SignatureString = "UCLIDException Object Version 2";
        private const int DefaultMaxFileSize = 2000000;

        public static readonly string _ENCRYPTED_PREFIX = "Extract_Encrypted: ";

        public override IDictionary Data { get; }

        /// <summary>
        /// The lock used to synchronize multi-threaded access to this object.
        /// </summary>
        [NonSerialized]
        readonly object _thisLock = new object();

        static readonly string LOG_FILE_MUTEX = "Global\\0A7EF4EA-E618-4A07-9D77-7F4E48D6B224";

        [NonSerialized]
        Mutex LogFileMutex = new(false, LOG_FILE_MUTEX);

        class LockMutex : IDisposable
        {
            Mutex _mutex;
            public LockMutex(Mutex mutex)
            {
                _mutex = mutex;
                _mutex.WaitOne();
            }

            public void Dispose()
            {
                _mutex.ReleaseMutex();
            }
        }

        static public UInt32 CurrentVersion { get; } = 3;

        public string EliCode { get; set; }

        public List<string> Resolutions { get; } = new List<string>();

        /// <summary>
        /// The multi-level encrypted stack trace associated with this ExtractException object
        /// and all the inner exceptions associated with this object.
        /// </summary>
        public Stack<string> StackTraceValues { get; } = new Stack<string>();

        /// <summary>
        /// Whether or not the encrypted stack trace has already been updated with the stack
        /// trace information associated with this exception object.
        /// </summary>
        // bool defaults to a value of false
        bool stackTraceRecorded;

        public string LogPath { get; set; } = Path.Combine(GetFolderPath(SpecialFolder.CommonApplicationData), @"Extract Systems\LogFiles");

        public ContextInfo ApplicationState { get; } = new();

        public Guid ExceptionIdentifier { get; private set; }

        public DateTime ExceptionTime { get; set; }

        public Int32 FileID
        {
            get => ApplicationState.FileID; 
            set 
            { 
                ApplicationState.FileID = value; 
            }
        }
        public Int32 ActionID
        {
            get => ApplicationState.ActionID;
            set { 
                ApplicationState.ActionID = value; 
            }
        }
        public string DatabaseServer {
            get => ApplicationState.DatabaseServer;
            set
            {
                ApplicationState.DatabaseServer = value;
            }
        }
        public string DatabaseName 
        { 
            get => ApplicationState.DatabaseName;
            set
            {
                ApplicationState.DatabaseName = value;
            }
        }
        public string FpsContext 
        { 
            get => ApplicationState.FpsContext;
            set
            {
                ApplicationState.FpsContext = value;
            }
        }

        private void SetupIdentifierAndTime()
        {
            ExceptionIdentifier = Guid.NewGuid();
            ExceptionTime = DateTime.UtcNow;
            ExceptionTime = new(ExceptionTime.Year,
                                ExceptionTime.Month,
                                ExceptionTime.Day,
                                ExceptionTime.Hour,
                                ExceptionTime.Minute,
                                ExceptionTime.Second,
                                DateTimeKind.Utc);
        }

        public class JsonExceptionConverter : IJsonConverter
        {
            public bool SerializeObject(object value, StringBuilder builder)
            {
                var serializedValue = JsonConvert.SerializeObject(value, ExtractException._serializeSettings);
                builder.Append(serializedValue);
                return true;
            }
        }

        static ExtractException()
        {
            LogManager.Setup().SetupSerialization(s =>
                s.RegisterJsonConverter(new JsonExceptionConverter()));
        }

        public ExtractException() : base()
        {
            Data = new ExceptionData();
            EliCode = "";
            SetupIdentifierAndTime();
        }

        public ExtractException(LogLevel loggingLevel) : this()
        {
            LoggingLevel = loggingLevel;
        }

        public ExtractException(string eliCode, string message) : base(message)
        {
            Data = new ExceptionData();
            EliCode = eliCode;
            SetupIdentifierAndTime();
        }

        public ExtractException(LogLevel loggingLevel, string eliCode, string message) : this(eliCode, message)
        {
            LoggingLevel = loggingLevel;
        }

        public ExtractException(string eliCode, string message, Exception innerException) : base(
            message,
            innerException?.AsExtractException("ELI53553"))
        {
            Data = new ExceptionData();
            EliCode = eliCode;
            if (innerException != null && !string.IsNullOrEmpty(innerException.StackTrace))
            {
                RecordStackTrace(innerException.StackTrace);
            }
            SetupIdentifierAndTime();
            ExtractException inner = (ExtractException) InnerException;
            
            // Update the root values from inner exceptions if they are not already set
            if (inner != null)
            {
                ActionID = (ActionID < 1) ? inner.ActionID : ActionID;
                FileID = (FileID < 1) ? inner.FileID : FileID;
                DatabaseServer = (string.IsNullOrWhiteSpace(DatabaseServer) ? inner.DatabaseServer : string.Empty);
                DatabaseName = (string.IsNullOrWhiteSpace(DatabaseName) ? inner.DatabaseName : string.Empty);
            }
        }

        public ExtractException(LogLevel loggingLevel, string eliCode, string message, Exception innerException)
            : this(eliCode, message, innerException)
        {
            LoggingLevel = loggingLevel;
        }

        internal ExtractException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            (Dictionary<string, object> infoDictionary, ExceptionData exceptionData) = CreateInfoDictionary(info);

            Data = exceptionData;

            EliCode = info.GetString("EliCode");
            uint version = info.GetUInt32("Version");

            VerifyVersion(version);

            stackTraceRecorded = info.GetBoolean("StackTraceRecorded");
            Stack<string> encryptedStackTrace = (Stack<string>)info.GetValue("StackTraceValues", typeof(Stack<string>));
            
            StackTraceValues.Clear();
            foreach(string value in encryptedStackTrace)
            {
                StackTraceValues.Push(DebugDataHelper.ValueAsType<string>(value));
            }
            RecordStackTrace();

            // These values may not exist 
            // Initialize with current values
            SetupIdentifierAndTime();

            ApplicationState = infoDictionary.ContainsKey("ApplicationState")
                ? (ContextInfo)info.GetValue("ApplicationState", typeof(ContextInfo)) : ApplicationState;

            if (infoDictionary.ContainsKey("LoggingLevel"))
            {
                // LogLevel type is not binary serializable - need to convert to int
                LogLevelTypeConverter logLevelTypeConverter = new();
                LoggingLevel = (LogLevel)logLevelTypeConverter.ConvertFrom(info.GetInt32("LoggingLevel"));
            }

            if (infoDictionary.ContainsKey("ExceptionTime"))
            {
                ExceptionTime = info.GetDateTime("ExceptionTime");
            }
            else
            {
                ExceptionTime = DateTime.UtcNow;
                ExceptionTime = new(ExceptionTime.Year,
                    ExceptionTime.Month,
                    ExceptionTime.Day,
                    ExceptionTime.Hour,
                    ExceptionTime.Minute,
                    ExceptionTime.Second,
                    DateTimeKind.Utc);
            }

            ExceptionIdentifier =
                infoDictionary.ContainsKey("ExceptionIdentifier") ?
                    (Guid)info.GetValue("ExceptionIdentifier", typeof(Guid)) : Guid.NewGuid();

            ActionID = infoDictionary.ContainsKey("ExceptionActionID") ?
                info.GetInt32("ExceptionActionID") : 0;
            FileID = infoDictionary.ContainsKey("ExceptionFileID") ?
                info.GetInt32("ExceptionFileID") : 0;
            DatabaseServer = infoDictionary.ContainsKey("ExceptionDatabaseServer") ?
                info.GetString("ExceptionDatabaseServer") : String.Empty;
            DatabaseName = infoDictionary.ContainsKey("ExceptionDatabaseName") ?
                info.GetString("ExceptionDatabaseName") : String.Empty;
            FpsContext = infoDictionary.ContainsKey("ExceptionFpsContext") ?
                info.GetString("ExceptionFpsContext") : String.Empty;
        }

        private static void VerifyVersion(uint version)
        {
            if (version > CurrentVersion)
            {
                ExtractException ee = new ExtractException("ELI53544", "Unknown exception version");
                ee.AddDebugData("Serialized Version", version);
                ee.AddDebugData("Current Version", CurrentVersion);
                throw ee;
            }
        }

        private (Dictionary<string, object>, ExceptionData) CreateInfoDictionary(SerializationInfo info)
        {
            var infoDictionary = info.ToDictionary();
            ExceptionData returnData = null;
            if (infoDictionary.ContainsKey("ExceptionData"))
            {
                returnData = (ExceptionData)info.GetValue("ExceptionData", typeof(ExceptionData));
            }
            else
            {
                returnData = new ExceptionData();
                if (Data != null)
                {
                    foreach (var item in Data.OfType<DictionaryEntry>())
                        Data.Add(item.Key, item.Value);
                }
            }
            return (infoDictionary, returnData);
        }

        // GetObjectData performs a custom serialization.
        public override void GetObjectData(SerializationInfo info,
            StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Version", CurrentVersion);
            info.AddValue("EliCode", EliCode);
            info.AddValue("StackTraceRecorded", stackTraceRecorded);
            info.AddValue("StackTraceValues", StackTraceValues);
            info.AddValue("ApplicationState", ApplicationState);
            info.AddValue("ExceptionIdentifier", ExceptionIdentifier);
            info.AddValue("ExceptionTime", ExceptionTime);
            info.AddValue("ExceptionData", Data);
            info.AddValue("ExceptionActionID", ActionID);
            info.AddValue("ExceptionFileID", FileID);
            info.AddValue("ExceptionDatabaseServer", DatabaseServer);
            info.AddValue("ExceptionDatabaseName", DatabaseName);
            info.AddValue("ExceptionFpsContex", FpsContext);

            // LogLevel type is not binary serializable - need to convert to int
            LogLevelTypeConverter logLevelTypeConverter = new();
            info.AddValue("LoggingLevel", logLevelTypeConverter.ConvertTo(LoggingLevel, typeof(Int32)));
        }


        /// <summary>
        /// Returns the settings in a JSON string
        /// </summary>
        public string ToJson()
        {
            try
            {
                return JsonConvert.SerializeObject(this, _serializeSettings);
            }
            catch (Exception ex)
            {
                throw ex.AsExtractException("ELI53638");
            }
        }

        /// <summary>
        /// Deserializes a <see cref="ExtractException"/> instance from a JSON string
        /// </summary>
        /// <param name="settings">The JSON string to which a <see cref="ExtractException"/> was previously saved</param>
        public static ExtractException FromJson(string settings)
        {
            try
            {
                return JsonConvert.DeserializeObject<ExtractException>(settings, _deserializeSettings);
            }
            catch (Exception ex)
            {
                throw ex.AsExtractException("ELI53639");
            }
        }

        public void AddDebugData(string debugDataName, EventArgs debugDataValue, bool encrypt = false)
        {
            if (debugDataName == null) return;
            if (debugDataValue == null)
            {
                AddDebugData(debugDataName, "<null>", encrypt);
                return;
            }
            Type type = debugDataValue.GetType();
            foreach (var property in type.GetProperties())
            {
                if (!property.CanRead)
                    continue;
                if (property.GetIndexParameters().Length > 0)
                    continue;

                try
                {
                    object value = property.GetValue(debugDataValue, null);
                    AddDebugData($"{debugDataName}.{property.Name}", value, encrypt);
                }
                catch
                {
                    AddDebugData($"{debugDataName}.{property.Name}", "Unable to add value", false);
                }
            }
        }

        public void AddDebugData(string debugDataName, string debugDataValue, bool encrypt = false)
        {
            if (debugDataName == null)
                return;

            AddDebugData(debugDataName, (object)debugDataValue ?? "<null>", encrypt);
        }

        public void AddDebugData(string debugDataName, ValueType debugDataValue, bool encrypt = false)
        {
            if (debugDataName == null)
                return;

            AddDebugData(debugDataName, (object)debugDataValue ?? "<null>", encrypt);
        }

        public string AsStringizedByteStream()
        {
            // Make sure the stacktrace is recorded otherwise it will not be kept
            if (!stackTraceRecorded)
                RecordStackTrace();

            var byteArray = AsByteStream();
            return byteArray.GetBytes().ToHexString();
        }

        private ByteArrayManipulator AsByteStream()
        {
            // Make sure the stacktrace is recorded otherwise it will not be kept
            if (!stackTraceRecorded)
                RecordStackTrace();
            ByteArrayManipulator byteArray = new ByteArrayManipulator();
            byteArray.Write(SignatureString);
            byteArray.Write(CurrentVersion);
            byteArray.Write(EliCode);
            byteArray.Write(Message);

            byteArray.Write(InnerException != null);

            if (InnerException != null)
            {
                var ee = (InnerException as ExtractException) ?? InnerException.AsExtractException("ELI53527");
                if (ee != null)
                {
                    byteArray.Write(ee.AsByteStream());
                }
            }

            byteArray.Write(Resolutions.Count);
            foreach (var r in Resolutions)
            {
                byteArray.Write(r);
            }
            var exceptionData = GetExceptionData();

            byteArray.Write(Data.Count + exceptionData.Count);
            foreach (var value in ((ExceptionData)Data).GetFlattenedData())
            {
                byteArray.Write((string)value.Key);
                byteArray.Write(value.Value);
            }
            foreach(var value in exceptionData)
            {
                byteArray.Write(value.Key);
                byteArray.Write(value.Value);
            }

            byteArray.Write(StackTraceValues.Count);

            foreach (var st in StackTraceValues.Reverse())
            {
                byteArray.Write(st);
            }

            byteArray.Write(ApplicationState.PID);
            byteArray.Write(ApplicationState.MachineName);
            byteArray.Write(ApplicationState.ApplicationName);
            byteArray.Write(ApplicationState.UserName);
            byteArray.Write(ApplicationState.ApplicationVersion);
            byteArray.Write(ExceptionIdentifier);
            byteArray.WriteAsCTime(ExceptionTime);

            byteArray.Write(FileID);
            byteArray.Write(ActionID);
            byteArray.Write(DatabaseServer);
            byteArray.Write(DatabaseName);
            byteArray.Write(FpsContext);

            return byteArray;
        }

        private Dictionary<string,object> GetExceptionData()
        {
            Dictionary<string, object> exceptionData = new();
            exceptionData.Add("ExceptionData_m_ProcessData.m_PID", ApplicationState.PID);
            exceptionData.Add("ExceptionData_m_ProcessData.m_strComputerName", ApplicationState.MachineName);
            exceptionData.Add("ExceptionData_m_ProcessData.m_strProcessName", ApplicationState.ApplicationName);
            exceptionData.Add("ExceptionData_m_ProcessData.m_strUserName", ApplicationState.UserName);
            exceptionData.Add("ExceptionData_m_ProcessData.m_strVersion", ApplicationState.ApplicationVersion);
            exceptionData.Add("ExceptionData_m_guidExceptionIdentifier",ExceptionIdentifier);
            exceptionData.Add("ExceptionData_m_unixExceptionTime", ExceptionTime.ToUnixTime());
            exceptionData.Add("ExceptionData_m_lActionID", ActionID );
            exceptionData.Add("ExceptionData_m_lFileID", FileID);
            exceptionData.Add("ExceptionData_m_strDatabaseName", DatabaseName);
            exceptionData.Add("ExceptionData_m_strDatabaseServer", DatabaseServer);
            exceptionData.Add("ExceptionData_m_strFpsContext", FpsContext);
            return exceptionData;
        }

        public string CreateLogString()
        {
            return CreateLogString(DateTime.Now);
        }

        public string CreateLogString(DateTime time)
        {
            string strSerial = String.Empty; // No longer used
            string strPID = ApplicationState.PID.ToString();

            return $"{strSerial},{ApplicationState.ApplicationName} - {ApplicationState.ApplicationVersion},{ApplicationState.MachineName},{ApplicationState.UserName},{strPID},{time.ToUnixTime()},{AsStringizedByteStream()}";
        }

        /// <summary>
        /// Display the ExtractException as a WinForms modal
        /// </summary>
        public void Display()
        {
            try
            {
                using (ErrorDisplay errorDisplay = new ErrorDisplay(this))
                {
                    errorDisplay.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee =
                       new ExtractException("ELI53635", "Exception failed to display", ex);
                ee.AddDebugData("Error Message", this.Message, false);
                ee.Log();
            }
        }

        public void LogTrace()
        {
            LoggingLevel = LogLevel.Trace;
            Logger.Trace(this);
        }

        public void LogInfo()
        {
            LoggingLevel = LogLevel.Info;
            Logger.Info(this);
        }

        public void LogWarn()
        {
            LoggingLevel = LogLevel.Warn;
            Logger.Warn(this);
        }

        public void LogError()
        {
            LoggingLevel = LogLevel.Error;
            Logger.Error(this);
        }

        public void LogDebug()
        {
            LoggingLevel = LogLevel.Debug;
            Logger.Debug(this);
        }

        public void Log()
        {
            Log(Path.Combine(LogPath, Default_UEX_FileName));
        }

        public void Log(string fileName)
        {
            try
            {
                if (Message.StartsWith("Application trace:", StringComparison.OrdinalIgnoreCase))
                {
                    LoggingLevel = LogLevel.Info;
                }
                
                LogExceptionToLogger();

                // Log to uex file
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = Path.Combine(LogPath, Default_UEX_FileName);
                }
                if (ShouldLogFileBeRenamed(fileName))
                {
                    RenameLogFile(fileName, false, string.Empty, false);
                }

                SaveLineToLog(fileName, CreateLogString());
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.Warn(ex, "Exception thrown logging to {fileName}", fileName);
                    Logger.Debug(ex.AsExtract("ELI53663"));
                }
                catch
                {
                    // don't throw from log
                }
            }
        }


        public void Log(string machineName, string userName, Int64 dateTimeUtc, int processId, string applicationName, bool noRemote)
        {
            string fileName = String.Empty;
            try
            {
                fileName = Path.Combine(LogPath, Default_UEX_FileName);
                Log(fileName, machineName, userName, dateTimeUtc, processId, applicationName, noRemote);
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.Warn(ex, "Exception thrown logging to {fileName}", fileName);
                    Logger.Debug(ex.AsExtract("ELI53664"));
                }
                catch
                {
                    // Don't throw from log
                }
            }
        }

        public void Log(string fileName, string machineName, string userName, Int64 dateTimeUtc, int processId, string applicationName, bool noRemote)
        {
            try
            {

                if (Message.StartsWith("Application trace:", StringComparison.OrdinalIgnoreCase))
                {
                    LoggingLevel = LogLevel.Info;
                }

                LogExceptionToLogger();

                // Log to UEX file
                // Convert any , in the applicationName to .
                applicationName = applicationName.Replace(" ,", ".").Replace(',', '.');
                string logString = $",{applicationName},{machineName},{userName},{processId},{dateTimeUtc},{AsStringizedByteStream()}";

                if (ShouldLogFileBeRenamed(fileName))
                {
                    RenameLogFile(fileName, false, string.Empty, false);
                }

                SaveLineToLog(fileName, logString);
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.Warn(ex, "Exception thrown logging to {fileName}", fileName);
                    Logger.Debug(ex.AsExtract("ELI53641"));
                }
                catch
                {
                    // don't throw from log
                }
            }
        }

        public static ExtractException LoadFromByteStream(string stringizedByteStream)
        {
            ExtractException.Assert("ELI53619", "Argument is not stringizedByteStream",
                stringizedByteStream.StartsWith(ExceptionSignature));

            ByteArrayManipulator byteArray = new ByteArrayManipulator(stringizedByteStream.FromHexString());
            return LoadFromByteStream(byteArray);
        }

        private static ExtractException LoadFromByteStream(ByteArrayManipulator byteArray)
        {
            try
            {
                string temp = byteArray.ReadString();
                if (temp != SignatureString)
                {
                    var e = new ExtractException("ELI53612", "Unrecognized Exception signature.");
                    e.AddDebugData("Signature Found", temp);
                    e.AddDebugData("Expected Signature", SignatureString);
                    throw e;
                }

                UInt32 versionNumber = byteArray.ReadUInt32();

                if (versionNumber != CurrentVersion)
                {
                    var e = new ExtractException("ELI53613", "Unrecognized Exception version number.");
                    e.AddDebugData("Expected version", CurrentVersion);
                    e.AddDebugData("Exception version", versionNumber);
                    throw e;
                }

                ExtractException returnException;
                string eliCode = byteArray.ReadString();
                string message = byteArray.ReadString();
                bool isInnerException = byteArray.ReadBoolean();

                if (isInnerException)
                {
                    var inner = byteArray.ReadByteStream();
                    returnException = new ExtractException(eliCode, message, LoadFromByteStream(inner));
                }
                else
                {
                    returnException = new ExtractException(eliCode, message);
                }

                UInt32 numberOfResolutions = byteArray.ReadUInt32();
                for (int i = 0; i < numberOfResolutions; i++)
                {
                    returnException.Resolutions.Add(byteArray.ReadString());
                }

                UInt32 numberOfDataPairs = byteArray.ReadUInt32();

                for (int i = 0; i < numberOfDataPairs; i++)
                {
                    string name = byteArray.ReadString();
                    var value = byteArray.ReadObject();

                    if (!SetRootValues(returnException, name, value))
                    {
                        returnException.Data.Add(name, value);
                    }
                }

                UInt32 numberOfStackTraces = byteArray.ReadUInt32();

                List<string> listOfstackTraceEncrypted = new List<string>();

                for (UInt32 i = 0; i < numberOfStackTraces; i++)
                {
                    listOfstackTraceEncrypted.Add(byteArray.ReadString());
                }

                foreach (var stackTrace in listOfstackTraceEncrypted)
                {
                    returnException.StackTraceValues.Push(DebugDataHelper.ValueAsType<string>(stackTrace));
                }

                if (!byteArray.EOF)
                {
                    returnException.ApplicationState.PID = byteArray.ReadUInt32();
                    returnException.ApplicationState.MachineName = byteArray.ReadString();
                    returnException.ApplicationState.ApplicationName = byteArray.ReadString();
                    returnException.ApplicationState.UserName = byteArray.ReadString();
                    returnException.ApplicationState.ApplicationVersion = byteArray.ReadString();
                    returnException.ExceptionIdentifier = byteArray.ReadGuid();
                    returnException.ExceptionTime = byteArray.ReadCTimeAsDateTime();
                }
                if (!byteArray.EOF)
                {
                    returnException.FileID = byteArray.ReadInt32();
                    returnException.ActionID = byteArray.ReadInt32();
                    returnException.DatabaseServer = byteArray.ReadString();
                    returnException.DatabaseName = byteArray.ReadString();
                }
                
                if (!byteArray.EOF)
                {
                    returnException.FpsContext = byteArray.ReadString();
                }

                return returnException;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private static bool SetRootValues(ExtractException extractException, string name, object value)
        {
            if (name == "ExceptionData_m_ProcessData.m_PID")
            {
                extractException.ApplicationState.PID = (UInt32)value;
                return true;
            }
            if (name == "ExceptionData_m_ProcessData.m_strComputerName")
            {
                extractException.ApplicationState.MachineName = (string)value;
                return true;
            }
            if (name == "ExceptionData_m_ProcessData.m_strProcessName")
            {
                extractException.ApplicationState.ApplicationName = (string)value;
                return true;
            }
            if (name == "ExceptionData_m_ProcessData.m_strUserName")
            {
                extractException.ApplicationState.UserName = (string)value;
                return true;
            }
            if (name == "ExceptionData_m_ProcessData.m_strVersion")
            {
                extractException.ApplicationState.ApplicationVersion = (string)value;
                return true;
            }
            if (name == "ExceptionData_m_guidExceptionIdentifier")
            {
                extractException.ExceptionIdentifier = (Guid)value;
                return true;
            }
            if (name == "ExceptionData_m_unixExceptionTime")
            {
                extractException.ExceptionTime = new DateTime(1970, 1, 1) +
                    TimeSpan.FromTicks((Int64)value * TimeSpan.TicksPerSecond);
                return true;
            }
            if (name == "ExceptionData_m_lActionID")
            {
                extractException.ActionID = (int)value;
                return true;
            }
            if (name == "ExceptionData_m_lFileID")
            {
                extractException.FileID = (int)value;
                return true;
            }
            if (name == "ExceptionData_m_strDatabaseName")
            {
                extractException.DatabaseName = (string)value;
                return true;
            }
            if (name == "ExceptionData_m_strDatabaseServer")
            {
                extractException.DatabaseServer = (string)value;
                return true;
            }
            if (name == "ExceptionData_m_strFpsContext")
            {
                extractException.FpsContext = (string)value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Throws an ExtractException built from the provided EliCode containing four
        /// debug data pairs if the condition provided is false, otherwise does
        /// nothing.
        /// </summary>
        /// <param name="eliCode">A unique Extract Systems ELI Code</param>
        /// <param name="message">The message to associate with this exception</param>
        /// <param name="condition">An expression that evaluates to a boolean</param>
        /// <param name="debugData">array that contains the debug data</param>
        /// <exception cref="ExtractException">Thrown if <paramref name="condition"/>
        /// evaluates to false.
        /// </exception>
        // This method throws an ExtractException if the condition is false. It has been
        // checked and should not throw any other exceptions.
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public static void Assert(string eliCode, string message, bool condition,
            params (string debugDataName, object debugDataValue)[] debugData)
        {
            if (!condition)
            {
                // Create the new ExtractException
                ExtractException ee;
                if (string.IsNullOrEmpty(message))
                {
                    ee = new ExtractException(eliCode, "Condition not met.");
                }
                else
                {
                    ee = new ExtractException(eliCode, message);
                }

                // Add the debug data (check for null or empty before adding the data)
                foreach (var value in debugData)
                {
                    ee.AddDebugData(value.debugDataName, value.debugDataValue, false);
                }

                // Throw the new exception
                throw ee;
            }
        }

        internal string RenameLogFile(string fileName, bool userRenamed, string comment, bool throwExceptionOnFailure)
        {
            try
            {
                Assert("ELI53572", $"File '{fileName} must exist.", File.Exists(fileName), ("FileName", fileName));
                string fileNameTo = String.Empty;
                try
                {
                    DateTime dateTime = DateTime.Now;
                    string dateTimePrefix = dateTime.ToString("yyyy-MM-dd HH'h'mm'm'ss.fff's' ");
                    fileNameTo = Path.Combine(Path.GetDirectoryName(fileName),
                        dateTimePrefix + Path.GetFileNameWithoutExtension(fileName) + Path.GetExtension(fileName));

                    using var l = new LockMutex(LogFileMutex);
                    File.Move(fileName, fileNameTo);

                    string strELICode = "ELI53578";
                    string strMessage =
                        "Application trace: Current log file was time stamped and renamed.";
                    if (userRenamed)
                    {
                        strELICode = "ELI53579";
                        strMessage = "User renamed log file.";
                    }

                    // log an entry in the new log file indicating the file has been renamed.
                    ExtractException ue = new(strELICode, strMessage);
                    ue.LoggingLevel = LogLevel.Info;
                    ue.AddDebugData("RenamedLogFile", fileNameTo);
                    if (!string.IsNullOrWhiteSpace(comment))
                    {
                        ue.AddDebugData("User Comment", comment);
                    }
                    ue.Log(fileName);
                }
                catch (Exception ex)
                {
                    if (throwExceptionOnFailure)
                    {
                        var renameEx = new ExtractException("ELI53575", "Unable to rename log file.", ex);
                        renameEx.AddDebugData("Log File Name", fileName);
                        renameEx.AddDebugData("New Log File Name", fileNameTo);
                        throw renameEx;
                    }
                    fileNameTo = String.Empty;
                }
                return fileNameTo;
            }
            catch when (!throwExceptionOnFailure) { }


            return String.Empty;
        }

        #region Private methods

        /// <summary>
        /// Checks the size of the file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool ShouldLogFileBeRenamed(string fileName, UInt32 maxSize = DefaultMaxFileSize)
        {
            FileInfo fi = new FileInfo(fileName);
            return fi.Exists && fi.Length >= maxSize;
        }

        /// <summary>
        /// This method will move the encrypted stack trace data from the inner exception
        /// to this exception object.
        /// <para>NOTE: Before calling this method, the caller must guarantee that an 
        /// inner exception object of type ExtractException is associated with this object.</para>
        /// </summary>
        /// <exception cref="ExtractException">
        /// Thrown if an inner exception of type ExtractException is not associated with
        /// this exception.
        /// </exception>
        void TakeOverInnerExceptionsStackTrace()
        {
            // The AsExtractException() call in the parameter initialization code above
            // should guarantee that by the time the code below is executing, the 
            // inner exception object associated with the base class is of type ExtractException.
            // If that's not the case, that's a logic problem in our code.
            ExtractException eex = InnerException as ExtractException;
            if (eex == null)
            {
                throw new ExtractException("ELI21062", "Internal logic error.");
            }

            // Update the encrypted stack trace information associated with eex with its
            // current stack trace information.
            eex.RecordStackTrace();
        }

        /// <summary>
        /// Update the encrypted stack trace information with this exception's stack trace if
        /// this hasn't already been done.
        /// </summary>
        internal void RecordStackTrace()
        {
            lock (_thisLock)
            {
                // Update the encrypted stack trace if it has not yet been updated with this 
                // exception object's stack trace.
                if (!stackTraceRecorded)
                {
                    RecordStackTrace(base.StackTrace);
                    stackTraceRecorded = true;
                }
            }
        }

        /// <summary>
        /// Update and decrypt stack trace information with the provided stack trace.
        /// </summary>
        /// <param name="stackTrace">The stack trace to add to the encrypted stack trace
        /// information associated with this exception.</param>
        internal void RecordStackTrace(string stackTrace)
        {
            // If there is no new stack trace information to add, then just exit.
            if (string.IsNullOrEmpty(stackTrace))
            {
                return;
            }

            // Lock this object for the rest of the scope as the member variable
            // may be updated.
            lock (_thisLock)
            {
                // Parse the stack trace and update the internal stack trace variable.
                var stackTraceEntries = stackTrace.Split(new char[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
                foreach (string s in stackTraceEntries)
                {
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        StackTraceValues.Push(s);
                    }
                }
            }
        }

        /// <summary>
        /// Copy information (such as data, source, helplink, etc) from the .NET exception object 
        /// into this ExtractException object.
        /// </summary>
        /// <param name="e">The .NET exception object to copy information from.</param>
        void CopyInformationFrom(Exception e)
        {
            // Copy basic information from the specified exception
            // to the ExtractException class members.
            lock (_thisLock)
            {
                Source = e.Source;
                HelpLink = e.HelpLink;

                // Copy e.Data and update this.Data
                foreach (DictionaryEntry de in e.Data)
                {
                    AddDebugData(de.Key.ToString(), de.Value, false);
                }
            }

            // Update the encrypted stack trace information with the stack trace information
            // associated with the specified exception
            RecordStackTrace(e.StackTrace);
        }

        /// <summary>
        /// Write this ExtractException object into a string "stream" with the
        /// appropriate level of indentation, where the indentation is used for 
        /// representing the inner exceptions and their associated data.
        /// </summary>
        /// <param name="sb">The StringBuilder object to write this ExtractException
        /// object's data to.</param>
        /// <param name="indentLevel">The indentation level to use to write this ExtractException
        /// object into the string "stream".  The top most exception should use an indentation
        /// level of zero, its inner exception should use an indentation level of 1, and so on.
        /// </param>
        void WriteToStringBuilder(StringBuilder sb, int indentLevel)
        {
            string indent = indentLevel > 1 ? new string(' ', (indentLevel - 1) * 3) : "";
            const string nodeChars = "+--";

            if (indentLevel == 0)
            {
                sb.Append("[Exception hierarchy and Data]");
                sb.Append(Environment.NewLine);
            }

            lock (_thisLock)
            {
                // Write line with ELI code + message
                sb.Append(indent);
                if (indentLevel > 0)
                {
                    sb.Append(nodeChars);
                }
                sb.Append(EliCode);
                sb.Append(": ");
                sb.Append(Message);
                sb.Append(Environment.NewLine);

                // Write the data elements
                if (Data.Count > 0)
                {
                    // Write each of the data items
                    foreach (DictionaryEntry de in Data.Values)
                    {
                        sb.Append(indent);
                        sb.Append(indent);
                        sb.Append(nodeChars);
                        sb.Append(de.Key.ToString().Replace("\r\n", @"\r\n"));
                        sb.Append("=");
                        if (de.Value != null)
                        {
                            sb.Append(de.Value.ToString().Replace("\r\n", @"\r\n"));
                        }
                        else
                        {
                            sb.Append("null");
                        }
                        sb.Append(Environment.NewLine);
                    }
                }


                // Write the stack trace if available
                if (StackTraceValues != null && StackTraceValues.Count > 0)
                {
                    if (indentLevel > 0)
                    {
                        sb.Append("   ");
                    }

                    sb.Append(indent);
                    sb.Append("[Stacktrace]");
                    sb.Append(Environment.NewLine);
                    foreach (var item in StackTraceValues)
                    {
                        if (indentLevel > 0)
                        {
                            sb.Append("   ");
                        }

                        sb.Append(indent);
                        sb.Append(item);
                        sb.Append(Environment.NewLine);
                    }
                }

            }

            // Write the inner exception if available
            if (InnerException != null)
            {
                ExtractException ex = InnerException as ExtractException;
                if (ex != null)
                {
                    ex.WriteToStringBuilder(sb, indentLevel + 1);
                }
                else
                {
                    Debug.Assert(true, "This condition not handled.");
                }
            }
        }

        /// <summary>
        /// Adds a key and object pair of debug data to the <see cref="ExtractException"/> and 
        /// optionally encrypts.
        /// </summary>
        /// <param name="debugDataName">The key to associate with the data value. If 
        /// <see langword="null"/>, no data will be added to the debug data collection.</param>
        /// <param name="debugDataValue">The value to be added to the data collection.</param>
        /// <param name="encrypt">Specifies whether to encrypt the data value or not.</param>
        void AddDebugData(string debugDataName, object debugDataValue, bool encrypt)
        {
            // Wrap in a try catch block to ensure no exceptions are thrown
            try
            {
                // Ensure debugDataName is not null or empty 
                if (!String.IsNullOrEmpty(debugDataName))
                {
                    // Ensure thread safety while adding the debug data
                    lock (_thisLock)
                    {
                        // Ensure the data value is serializable
                        object dataValue = debugDataValue.GetType().IsSerializable
                                ? debugDataValue
                                : debugDataValue.ToString();

                        if (encrypt)
                        {
                            dataValue = EncryptedValue(dataValue);
                        }

                        // Add the debug data
                        Data.Add(debugDataName, dataValue);
                    }
                }
                // debugDataName is null or empty, do nothing
            }
            catch (Exception ex)
            {
                new ExtractException("ELI53815", "Failed to add debug data.", ex.AsExtractException("ELI53816")).Log();
            }
        }

        private void SaveLineToLog(string fileName, string lineToSave)
        {
            bool fileAlreadyExists = File.Exists(fileName);
            using LockMutex lockMutex = new(LogFileMutex);
            File.AppendAllText(fileName, lineToSave + NewLine);

            // File has just been created
            if (!fileAlreadyExists && File.Exists(fileName))
            {
                GiveUsersAccess(fileName);
            }
        }

        private void GiveUsersAccess(string fileName)
        {
            try
            {
                var fileInfo = new FileInfo(fileName);
                var security = fileInfo.GetAccessControl();
                var userSecurityIdentifier = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                var modifyAccessRule = new FileSystemAccessRule(userSecurityIdentifier, FileSystemRights.Modify, AccessControlType.Allow);

                security.AddAccessRule(modifyAccessRule);
                fileInfo.SetAccessControl(security);
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI53599", $"Unable to set permissions on {fileName}", ex);
                ee.AddDebugData("FileName", fileName);
                // Try to log

                try
                {
                    ee.Log();
                }
                catch (Exception exLog)
                {
                    Logger.Warn(exLog, "Error logging exception");
                    Logger.Debug(exLog.AsExtract("ELI53642"));
                }
            }
        }

        private string EncryptedValue(object obj)
        {
            string dataString = obj?.ToString() ?? "<null>";
            var inputStream = new ByteArrayManipulator();
            inputStream.Write(dataString);

            var input = inputStream.GetBytes(8);
            var output = new byte[input.Length];

            Encryption.EncryptionEngine.Encrypt(input, CreateK(), output);
            return _ENCRYPTED_PREFIX + output.ToHexString();
        }

        private static ILogger InitializeLoggerFromConfig()
        {
            var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string configPath = Path.Combine(commonAppData, "Extract Systems\\Configuration\\NLog.config");
            if (!File.Exists(configPath))
            {
                // TODO: Add a default configuration
            }

            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(configPath);

            return LogManager.GetLogger(NLogTargetConstants.EventsTarget);
        }

        private void LogExceptionToLogger()
        {
            if (LoggingLevel == LogLevel.Trace)
            {
                Logger.Trace(this);
            }
            if (LoggingLevel == LogLevel.Debug)
            {
                Logger.Debug(this);
            }
            if (LoggingLevel == LogLevel.Info)
            {
                Logger.Info(this);
            }
            if (LoggingLevel == LogLevel.Warn)
            {
                Logger.Warn(this);
            }
            if (LoggingLevel == LogLevel.Error)
            {
                Logger.Error(this);
            }
            if (LoggingLevel == LogLevel.Fatal)
            {
                Logger.Fatal(this);
            }
        }

        #endregion Private methods


    }
}
