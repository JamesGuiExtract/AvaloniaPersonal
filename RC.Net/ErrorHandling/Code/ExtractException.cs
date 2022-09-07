using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
                try
                {
                    _mutex.WaitOne();
                }
                catch (AbandonedMutexException) 
                { 
                    // don't want to throw an exception so ignore
                };
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

        /// <summary>
        /// Raised to notify listeners that an exception is about to be displayed.
        /// </summary>
        public static event EventHandler<ExtractExceptionEventArgs> DisplayingException;

        public string LogPath { get; set; } = Path.Combine(GetFolderPath(SpecialFolder.CommonApplicationData), @"Extract Systems\LogFiles");

        public ExtractException() : base()
        {
            Data = new ExceptionData();
            EliCode = "";
        }

        public ExtractException(string eliCode, string message) : base(message)
        {
            Data = new ExceptionData();
            EliCode = eliCode;
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
        }

        internal ExtractException(SerializationInfo info,
        StreamingContext context) :
            base(info, context)
        {
            var hold = Data;
            try
            {
                Data = (ExceptionData)info.GetValue("ExceptionData", typeof(ExceptionData));
            }
            catch(Exception ex) 
            {
                var ee = new ExtractException("ELI53618", "Unable to deserialize ExceptionData", ex);
                ee.Log();

                Data = new ExceptionData();
                if (hold != null)
                {
                    foreach (var item in hold.OfType<DictionaryEntry>())
                        Data.Add(item.Key, item.Value);
                }
            }
            EliCode = info.GetString("ELICode");
            uint version = info.GetUInt32("Version");
            if (version > CurrentVersion)
            {
                ExtractException ee = new ExtractException("ELI53544", "Unknown exception version");
                ee.AddDebugData("Serialized Version", version);
                ee.AddDebugData("Current Version", CurrentVersion);
                throw ee;
            }
            stackTraceRecorded = info.GetBoolean("StackTraceRecorded");
            StackTraceValues = (Stack<string>)info.GetValue("StackTraceValues", typeof(Stack<string>));
            RecordStackTrace();
        }

        // GetObjectData performs a custom serialization.
        public override void GetObjectData(SerializationInfo info,
            StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ExceptionData", Data);
            info.AddValue("Version", CurrentVersion);
            info.AddValue("ELICode", EliCode);
            info.AddValue("StackTraceRecorded", stackTraceRecorded);
            info.AddValue("StackTraceValues", StackTraceValues);
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
            foreach(var property in type.GetProperties())
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
            byteArray.Write(Data.Count);
            foreach (var value in ((ExceptionData)Data).GetFlattenedData())
            {
                byteArray.Write((string)value.Key);
                byteArray.Write(value.Value);
            }
            byteArray.Write(StackTraceValues.Count);

            foreach (var st in StackTraceValues)
            {
                byteArray.Write(st);
            }
            return byteArray;
        }

        public string CreateLogString()
        {
            return CreateLogString(DateTime.Now);
        }

        public string CreateLogString(DateTime time )
        {
            string strSerial = String.Empty; // No longer used
            string appName = AppDomain.CurrentDomain.FriendlyName;
            string appVersion = Process.GetCurrentProcess().MainModule.FileVersionInfo.ProductVersion;
            string strApp = $"{appName} - {appVersion}"
                .Replace(" ,", ",")
                .Replace(',','.');
            string strComputer = Environment.MachineName;
            string strUser = WindowsIdentity.GetCurrent().Name.Split('\\').Last();
            string strPID = Process.GetCurrentProcess().Id.ToString();

            return $"{strSerial},{strApp},{strComputer},{strUser},{strPID},{time.ToUnixTime()},{AsStringizedByteStream()}";
        }

        /// <summary>
        /// https://extract.atlassian.net/browse/ISSUE-18451
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Display()
        {
            DisplayingException?.Invoke(this, new ExtractExceptionEventArgs(this));
            throw new NotImplementedException();
        }

        public void Log()
        {
            Log(null, false);
        }

        public void Log(string fileName)
        {
            Log(fileName, false);
        }

        public void Log(string fileName, bool forceLocal)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = Path.Combine(LogPath, "ExtractException.uex");
                }
                if (ShouldLogFileBeRenamed(fileName))
                {
                    RenameLogFile(fileName, false, string.Empty, false);
                }

                SaveLineToLog(fileName, CreateLogString());
            }
            catch(Exception )
            {
                // don't want to throw from log
            }
        }


        public void Log(string machineName, string userName, Int64 dateTimeUtc, int processId, string applicationName, bool noRemote)
        {
            try
            {
                string fileName = Path.Combine(LogPath, "ExtractException.uex");
                Log(fileName, machineName, userName, dateTimeUtc, processId, applicationName, noRemote);

            }
            catch (Exception)
            {
                // dont' want to 
            }        
        }

        public void Log(string fileName, string machineName, string userName, Int64 dateTimeUtc, int processId, string applicationName, bool noRemote)
        {
            try
            {
                // Convert any , in the applicationName to .
                applicationName = applicationName.Replace(" ,", ".").Replace(',', '.');
                string logString = $",{applicationName},{machineName},{userName},{processId},{dateTimeUtc},{AsStringizedByteStream()}";

                if (ShouldLogFileBeRenamed(fileName))
                {
                    RenameLogFile(fileName, false, string.Empty, false);
                }

                SaveLineToLog(fileName, logString);
            }
            catch (Exception)
            {
                // don't want to throw from log
            }
        }

        public static ExtractException LoadFromByteStream(string stringizedByteStream)
        {
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

                    returnException.Data.Add(name, value);
                }

                UInt32 numberOfStackTraces = byteArray.ReadUInt32();

                for (UInt32 i = 0; i < numberOfStackTraces; i++)
                {
                    returnException.StackTraceValues.Push (byteArray.ReadString());
                }

                return returnException;
                
            }
            catch(Exception )
            {
                throw ; 
            }
        }

        /// <summary>
        /// Throws an ExtractException built from the provided ELICode containing four
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
            params(string debugDataName, object debugDataValue)[] debugData)
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
                    ExtractException ue = new (strELICode, strMessage);
                    ue.AddDebugData("RenamedLogFile", fileNameTo);
                    if (!string.IsNullOrWhiteSpace(comment))
                    {
                        ue.AddDebugData("User Comment", comment);
                    }
                    ue.Log(fileName, false);
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
        /// Update the encrypted stack trace information with the provided stack trace.
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
                    if (s.Length > 0)
                    {
                        // Check if the value is already encrypted, encrypt if needed
                        if (!s.StartsWith(_ENCRYPTED_PREFIX, StringComparison.Ordinal))
                        {
                            if (!string.IsNullOrWhiteSpace(s))
                            {
                                var inputStream = new ByteArrayManipulator();
                                inputStream.Write(s);

                                var input = inputStream.GetBytes(8);
                                var output = new byte[input.Length];

                                Encryption.EncryptionEngine.Encrypt(input, CreateK(), output);

                                StackTraceValues.Push(_ENCRYPTED_PREFIX + output.ToHexString());
                            }
                        }
                        else 
                        {
                            StackTraceValues.Push(s);
                        }
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
                new ExtractException("ELI21253", "Failed to add debug data.", ex.AsExtractException("ELI53543")).Log();
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
                catch (Exception)
                {
                    // We tried to log so just eat it
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

        #endregion Private methods


    }
}
