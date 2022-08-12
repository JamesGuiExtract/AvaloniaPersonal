﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using static System.Environment;

namespace Extract.ErrorHandling
{
    [Serializable]
    public class ExtractException : Exception, IExtractException
    {
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
        // TODO: Fix encrypt
        //https://extract.atlassian.net/browse/ISSUE-18431
        //static readonly string _ENCRYPTED_PREFIX = "Extract_Encrypted: ";

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

        /// <summary>
        /// Raised to notify listeners that an exception is about to be displayed.
        /// </summary>
        public static event EventHandler<ExtractExceptionEventArgs> DisplayingException;

        public ExtractException() : base()
        {
            EliCode = "";
        }

        public ExtractException(string eliCode, string message) : base(message)
        {
            EliCode = eliCode;
        }

        public ExtractException(string eliCode, string message, Exception innerException) : base(
            message,
            innerException?.AsExtractException("ELI53553"))
        {
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
                    AddDebugData($"{debugDataName}.{property.Name}", "exception", encrypt);
                }
            }
        }

        public void AddDebugData(string debugDataName, string debugDataValue, bool encrypt = false)
        {
            if (debugDataName == null) return;
            if (debugDataValue == null)
            {
                AddDebugData(debugDataName, "<null>", encrypt);
                return;
            }
            Data.Add(debugDataName, debugDataValue);
        }

        public void AddDebugData(string debugDataName, ValueType debugDataValue, bool encrypt = false)
        {
            if (debugDataName == null) return;
            if (debugDataValue == null)
            {
                AddDebugData(debugDataName, "<null>", encrypt);
                return;
            }
            Data.Add(debugDataName, debugDataValue);
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
            foreach (var key in Data.Keys)
            {
                byteArray.Write((string)key);
                byteArray.Write(Data[key]);
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
                    string logPath = GetFolderPath(SpecialFolder.CommonApplicationData);
                    fileName = Path.Combine(logPath, "Extract Systems\\LogFiles\\ExtractException.uex");
                }
                using LockMutex lockMutex = new(LogFileMutex);
                File.AppendAllText(fileName, CreateLogString() + NewLine);
            }
            catch(Exception )
            {
                // don't want to throw from log
            }
        }

        public void Log(string machineName, string userName, int dateTimeUtc, int processId, string applicationName, bool noRemote)
        {
            // Convert any , in the applicationName to .
            applicationName = applicationName.Replace(" ,", ".").Replace(',', '.');
            string logString = $",{applicationName},{machineName},{userName},{processId},{dateTimeUtc},{AsStringizedByteStream()}";

            string logPath = GetFolderPath(SpecialFolder.CommonApplicationData);
            string fileName = Path.Combine(logPath, "Extract Systems\\LogFiles\\ExtractException.uex");

            using LockMutex lockMutex = new(LogFileMutex);
            File.AppendAllText(fileName, logString + NewLine);
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
                    var e = new Exception("Unrecognized Exception signature.");
                    e.Data.Add("Signature Found", temp);
                    e.Data.Add("Expected Signature", SignatureString);
                    throw e;
                }

                UInt32 versionNumber = byteArray.ReadUInt32();

                if (versionNumber != CurrentVersion)
                {
                    var e = new Exception("Unrecognized Exception version number.");
                    e.Data.Add("Expected version", CurrentVersion);
                    e.Data.Add("Exception version", versionNumber);
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


        #region Private methods

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
                        // TODO: Fix encrypt
                        //https://extract.atlassian.net/browse/ISSUE-18431
                        // Check if the value is already encrypted, encrypt if needed
                        //if (!s.StartsWith(_ENCRYPTED_PREFIX, StringComparison.Ordinal))
                        //{

                        //    StackTraceValues.Push(_ENCRYPTED_PREFIX + NativeMethods.EncryptString(s));
                        //}
                        //else
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
                    foreach (DictionaryEntry de in Data)
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
                        object dataValue = debugDataValue;
                        if (dataValue == null)
                        {
                            dataValue = "null";
                        }
                        else
                        {
                            // TODO: Fix encrypt
                            // https://extract.atlassian.net/browse/ISSUE-18431
                            //if (encrypt)
                            //{
                            //    dataValue = dataValue.ToString();

                            //    // Can't encrypt an empty string
                            //    if (!dataValue.Equals(""))
                            //    {
                            //        //dataValue = _ENCRYPTED_PREFIX +
                            //        //    NativeMethods.EncryptString((string)dataValue);
                            //    }
                            //}
                            //else
                            {
                                // If the value is not serializable get its string representation
                                dataValue = debugDataValue.GetType().IsSerializable
                                    ? debugDataValue
                                    : debugDataValue.ToString();
                            }
                        }

                        // Ensure the debug data name is unique
                        // [DotNetRCAndUtils #166]
                        int i = 1;
                        string uniqueDebugDataName = debugDataName;
                        while (Data.Contains(uniqueDebugDataName))
                        {
                            uniqueDebugDataName =
                                debugDataName + i.ToString(CultureInfo.CurrentCulture);
                            i++;
                        }

                        // Add the debug data
                        Data.Add(uniqueDebugDataName, dataValue);
                    }
                }
                // debugDataName is null or empty, do nothing
            }
            catch (Exception ex)
            {
                new ExtractException("ELI21253", "Failed to add debug data.", ex.AsExtractException("ELI53543")).Log();
            }
        }

        #endregion Private methods


    }
}
