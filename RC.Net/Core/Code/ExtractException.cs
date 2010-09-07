using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace Extract
{
    /// <summary>
    /// This class is the base class for all exceptions thrown by .NET components developed
    /// by Extract Systems. Some components may throw exceptions derived from this class.
    /// See the documentation of each component for details on what exceptions may be thrown
    /// by the various methods of the component.
    /// </summary>
    /// <example>Deriving a new Exception class from ExtractException:<para></para>
    /// <code>
    /// using System;
    /// using System.Text;
    /// using System.Runtime.Serialization;
    /// using System.Security.Permissions;
    /// using Extract;
    /// 
    /// // If you want to be able to derive exceptions from your derived exception class
    /// // then remove the sealed keyword from the class definition
    /// [Serializable]
    /// public sealed class DerivedExtractException : ExtractException
    /// {
    ///     string _DerivedExtractExceptionCode;
    /// 
    ///     public DerivedExtractException(string derivedCode, string eliCode, string message)
    ///         : base (eliCode, message)
    ///     {
    ///         _DerivedExtractExceptionCode = derivedCode;
    ///     }
    /// 
    ///     public DerivedExtractException(string derivedCode, string eliCode, string message,
    ///         Exception ex)
    ///         : base (eliCode, message, ex)
    ///     {
    ///         _DerivedExtractExceptionCode = derivedCode;
    ///     }
    /// 
    ///     DerivedExtractException(SerializationInfo info, StreamingContext context)
    ///         : base(info, context)
    ///     {
    ///         _DerivedExtractExceptionCode = info.GetString("_DerivedExtractExceptionCode");
    ///     }
    /// 
    ///     [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    ///     public override void GetObjectData(SerializationInfo info,
    ///         StreamingContext context)
    ///     {
    ///         // Must call base.GetObjectData first - Order is very important
    ///         base.GetObjectData(info, context);
    /// 
    ///         // Now add class specific data
    ///         info.AddValue("_DerivedExtractExceptionCode", _DerivedExtractExceptionCode);
    ///     }
    /// 
    ///     public override string ToString()
    ///     {
    ///         StringBuilder sb = new StringBuilder(base.ToString());
    ///         sb.Append(Environment.NewLine);
    ///         sb.Append("_DerivedExceptionCode: ");
    ///         sb.Append(_DerivedExtractExceptionCode);
    ///
    ///         return sb.ToString();
    ///     }
    /// }
    /// </code>
    /// </example>
    [Serializable]
    // Added so that FXCop will not complain that we have not implemented the standard
    // exception constructors.  We have intentionally not implemented them so that you
    // cannot create an ExtractException without specifying an ELI code.  If it is
    // discovered in later testing that we need to implement the default constructors
    // due to some issue in the framework we can then remove this suppress message.
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class ExtractException : Exception
    {
        #region Constants

        const int _CURRENT_VERSION = 1;

        static readonly string _ENCRYPTED_PREFIX = "Extract_Encrypted: ";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The lock used to synchronize multi-threaded access to this object.
        /// </summary>
        [NonSerialized] 
        readonly object _thisLock = new object();

        /// <summary>
        /// The ELI code associated with this ExtractException object.
        /// </summary>
        readonly string _eliCode;

        /// <summary>
        /// The multi-level encrypted stack trace associated with this ExtractException object
        /// and all the inner exceptions associated with this object.
        /// </summary>
        Stack<string> _stackTrace = new Stack<string>();

        /// <summary>
        /// Whether or not the encrypted stack trace has already been updated with the stack
        /// trace information associated with this exception object.
        /// </summary>
        // bool defaults to a value of false
        bool _stackTraceRecorded;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Create a simple ExtractException object from an ELI code and a message string,
        /// and no inner exceptions.
        /// </summary>
        /// <param name="eliCode">The ELI code associated with this exception.</param>
        /// <param name="message">The message string associated with this exception.</param>
        public ExtractException(string eliCode, string message)
            : base(message)
        {
            // Update member variables with provided args
            _eliCode = eliCode;
        }

        /// <summary>
        /// Create an ExtractException object from an ELI code, a message string, and an
        /// inner exception.
        /// </summary>
        /// <param name="eliCode">The ELI code associated with this exception.</param>
        /// <param name="message">The message string associated with this exception.</param>
        /// <param name="innerException">The inner exception associated with this exception.
        /// The inner exception can be of type Exception representing a standard .NET exception, of
        /// type Exception representing a C++/COM exception that has propagated into the .NET 
        /// framework, or an ExtractException object.
        /// </param>
        /// <exception cref="ExtractException">
        /// Thrown if the provided inner exception is not an ExtractException and cannot be 
        /// converted into an ExtractException.
        /// </exception>
        public ExtractException(string eliCode, string message, Exception innerException)
            : base(message, AsExtractException("ELI21061", innerException))
        {
            // Update member variables with provided args
            _eliCode = eliCode;

            // Move the stack trace of inner exception into the new exception and remove the 
            // stack trace associated with the inner exception.
            TakeOverInnerExceptionsStackTrace();
        }

        /// <summary>
        /// Create an ExtractException object from an ELI code, a message string, and an
        /// inner exception.
        /// </summary>
        /// <param name="eliCode">The ELI code associated with this exception.</param>
        /// <param name="message">The message string associated with this exception.</param>
        /// <param name="stringizedInnerException">The inner exception associated with this 
        /// exception. The inner exception should be a string produced by the asStringizedByteStream 
        /// method of UCLIDException or COMUCLIDException, if it is a message string an inner
        /// exception will be created with the message.
        /// </param>
        /// <exception cref="ExtractException">
        /// Thrown if the provided stringizedInnerException can not be converted to an 
        /// ExtractException.
        /// </exception>
        // Suppress the message for generated for creating an Exception object
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "stringized")]
        public ExtractException(string eliCode, string message, string stringizedInnerException)
            : base(message, FromCppException("ELI21952", new Exception(stringizedInnerException)))
        {
            // Update member variables with provided args
            _eliCode = eliCode;

            // Move the stack trace of inner exception into the new exception and remove the 
            // stack trace associated with the inner exception.
            TakeOverInnerExceptionsStackTrace();
        }

        #region ISerializable Members

        /// <summary>
        /// Constructor for serialization
        /// </summary>
        /// <param name="info">The serialization info object</param>
        /// <param name="context">The streaming context</param>
        protected ExtractException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Assert("ELI21182", "Serialization info cannot be null.", info != null);

            // Order is important in writing and retrieving the stream data
            // Load the version number information first
            int version = info.GetInt32("_CURRENT_VERSION");

            // Check the version number
            if (version > _CURRENT_VERSION)
            {
                // Unknown version number, log and exception and then
                // return leaving the exception data members with their default
                // empty/null initializations
                ExtractException ee = new ExtractException("ELI21135",
                    "Unknown exception version.");
                ee.AddDebugData("Serialized Version", version, false);
                ee.AddDebugData("Current Version", _CURRENT_VERSION, false);
                ee.Log();
                return;
            }

            // Load the remaining data elements in the same order they were stored
            _eliCode = info.GetString("_eliCode");
            _stackTraceRecorded = info.GetBoolean("_stackTraceRecorded");
            _stackTrace = (Stack<string>)info.GetValue("_stackTrace", typeof(Stack<string>));

            // Lastly ensure the _thisLock object is initialized.
            // This piece was added based on the recommendations in:
            // _Effective C#: 50 Specific Ways To Improve Your C#_ - Item 25
            // Item 25: Prefer Serializable types states that Non-Serialized member
            // items will not be initialized automatically by the DeSerialization process
            // since non of the standard constructors will be called.  This statement
            // has been added to ensure that the _thisLock object will be initialized.
            if (_thisLock == null)
            {
                _thisLock = new Object();
            }
        }

        /// <summary>
        /// Writes the ExtractException data to the serialization stream.
        /// </summary>
        /// <param name="info">The serialization info object</param>
        /// <param name="context">The streaming context</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info,
            StreamingContext context)
        {
            try
            {
                Assert("ELI21183", "Serialization info cannot be null.", info != null);

                // Order is important in writing and retrieving the stream data
                // Call the base GetObjectData first
                base.GetObjectData(info, context);

                // Add the local data values second
                info.AddValue("_CURRENT_VERSION", _CURRENT_VERSION);
                info.AddValue("_eliCode", _eliCode);
                info.AddValue("_stackTraceRecorded", _stackTraceRecorded);
                info.AddValue("_stackTrace", _stackTrace);
            }
            catch (Exception ex)
            {
                throw AsExtractException("ELI26490", ex);
            }
        }

        #endregion ISerializeable

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to return the ELICode for the exception
        /// </summary>
        public string EliCode
        {
            get
            {
                return _eliCode;
            }
        }

        #endregion Properties

        #region Public methods

        /// <summary>
        /// Log this exception to the standard exception log used by all of
        /// Extract Systems' products.
        /// <para>NOTE: This method should only be called once on a given ExtractException
        /// object.  If this method is called more than once on a given ExtractException
        /// object, the encrypted stack trace may contain duplicate entries.</para>
        /// </summary>
        public void Log()
        {
            Log(null);
        }

        /// <summary>
        /// Log this exception to the specified exception log file.
        /// <para>NOTE: This method should only be called once on a given ExtractException
        /// object.  If this method is called more than once on a given ExtractException
        /// object, the encrypted stack trace may contain duplicate entries.</para>
        /// </summary>
        public void Log(string fileName)
        {
            // Ensure the log function does not throw any exceptions
            try
            {
                // Update the encrypted stack trace information with the stack trace associated with
                // this ExtractException object, if this hasn't already been done.
                RecordStackTrace();

                // Create a UCLIDException COM object and populate it with all the information
                // contained in this ExtractException object.
                UCLID_EXCEPTIONMGMTLib.COMUCLIDException uex = AsCppException();

                if (string.IsNullOrEmpty(fileName))
                {
                    // Call the Log() method on the UCLIDException object to cause the exception to get
                    // logged to the standard log file used by all Extract Systems products.
                    uex.Log();
                }
                else
                {
                    // Ensure the filename is an absolute path
                    if (!Path.IsPathRooted(fileName))
                    {
                        fileName = Path.GetDirectoryName(Application.ExecutablePath) + @"\" + fileName;
                        fileName = Path.GetFullPath(fileName);
                    }

                    // Call the saveto method on the UCLIDException object to cause the exception
                    // to get logged to the specified log file.
                    uex.SaveTo(fileName, true);
                }
            }
            catch (Exception e)
            {
                // Log exception that was caught.
                NativeMethods.LogException("ELI21761", e.Message);

                // Log the exception that was supposed to be logged.
                NativeMethods.LogException(EliCode, Message);
            }
        }

        /// <summary>
        /// Display this exception using the standard exception viewer window used by all
        /// of Extract Systems' products.
        /// </summary>
        // This code will not be localized for a culture that uses right to left reading order.
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        public void Display()
        {
            // Ensure the display function does not throw any exceptions
            try
            {
                // Update the encrypted stack trace information with the stack trace associated with
                // this ExtractException object, if this hasn't already been done.
                RecordStackTrace();

                // Create a UCLIDException COM object and populate it with all the information
                // contained in this ExtractException object.
                UCLID_EXCEPTIONMGMTLib.COMUCLIDException uex = AsCppException();

                // Ensure the COM dialog handles the user's first mouse click [DotNetRCAndUtils #58]
                UncaptureToolStrips();

                // Call the Display() method on the UCLIDException object to cause the 
                // exception to get displayed using the standard exception displaying mechanism used 
                // by all Extract Systems products.
                uex.Display();
            }
            catch (Exception e)
            {
                // Display exception that should be displayed.
                MessageBox.Show(ToString(), "Exception");

                // Log the exception that caused a problem displaying with COMUCLIDException object.
                NativeMethods.LogException("ELI22361", e.Message);

                // Log the ELI code and message of exception that was displayed.
                NativeMethods.LogException(EliCode, Message);
            }
        }

        /// <overloads>Prevents tool strips from capturing mouse events.</overloads>
        /// <summary>
        /// Prevents all tool strips in the application's main form from capturing mouse events.
        /// </summary>
        static void UncaptureToolStrips()
        {
            try
            {
                // If this is not a GUI application, there are no toolstrips and we are done
                if (!Environment.UserInteractive)
                {
                    return;
                }

                // Get the main form
                Form mainForm = null;
                foreach (Form form in Application.OpenForms)
                {
                    if (form.TopLevel)
                    {
                        mainForm = form;
                        break;
                    }
                }

                // Set the Capture property of all child toolstrips and status strips to false
                if (mainForm != null)
                {
                    UncaptureToolStrips(mainForm);
                }
            }
            catch (Exception ex)
            {
                Log("ELI23322", ex);
            }
        }

        /// <summary>
        /// Prevents all child tool strips of the specified control from capturing mouse events.
        /// </summary>
        /// <param name="parent"></param>
        static void UncaptureToolStrips(Control parent)
        {
            // Check whether this is a toolstrip control
            ToolStrip toolStrip = parent as ToolStrip;
            if (toolStrip != null)
            {
                // Don't capture mouse events
                toolStrip.Capture = false;
                return;
            }

            // Get the collection of subcontrols
            Control.ControlCollection children = parent.Controls;
            if (children == null)
            {
                // Done.
                return;
            }

            // Check each sub control of this control
            foreach (Control child in children)
            {
                // Recursively uncapture the sub control and its children
                if (child != null)
                {
                    UncaptureToolStrips(child);
                }
            }
        }

        /// <summary>
        /// Returns the stack trace associated with this exception.
        /// <para>NOTE: The stack trace is encrypted, and therefore this property will
        /// always return an empty string.</para>
        /// </summary>
        public override string StackTrace
        {
            get
            {
                return "";
            }
        }

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        /// <returns>This ExtractException object represented as a multi-line string, including
        /// all information, such as ELI codes, inner exceptions, debug data, and encrypted stack
        /// trace.</returns>
        public override string ToString()
        {
            // Update the encrypted stack trace information with the stack trace associated with
            // this exception object, if this hasn't already been done so.
            RecordStackTrace();

            // Create a StringBuilder object and "stream" this exception object and its 
            // inner exceptions into a string.
            StringBuilder sb = new StringBuilder();
            WriteToStringBuilder(sb, 0);

            // Return the computed string
            return sb.ToString();
        }

        /// <overloads>Adds debug data to the exception.</overloads>
        /// <summary>
        /// Adds a key and value type pair of debug data to the <see cref="ExtractException"/> and
        /// optionally encrypts.
        /// </summary>
        /// <param name="debugDataName">The key to associate with the data value. If 
        /// <see langword="null"/>, no data will be added to the debug data collection.</param>
        /// <param name="debugDataValue">The value to be added to the data collection.</param>
        /// <param name="encrypt">Specifies whether to encrypt the data value or not.</param>
        public void AddDebugData(string debugDataName, ValueType debugDataValue, bool encrypt)
        {
            try
            {
                AddDebugData(debugDataName, (object)debugDataValue, encrypt);
            }
            catch
            {
                // Ignore all exceptions
            }
        }

        /// <summary>
        /// Adds a key and string pair of debug data to the <see cref="ExtractException"/> and 
        /// optionally encrypts.
        /// </summary>
        /// <param name="debugDataName">The key to associate with the data value. If 
        /// <see langword="null"/>, no data will be added to the debug data collection.</param>
        /// <param name="debugDataValue">The string to be added to the data collection.</param>
        /// <param name="encrypt">Specifies whether to encrypt the data value or not.</param>
        public void AddDebugData(string debugDataName, string debugDataValue, bool encrypt)
        {
            try
            {
                AddDebugData(debugDataName, (object)debugDataValue, encrypt);
            }
            catch
            {
                // Ignore all exceptions
            }
        }

        /// <summary>
        /// Adds a key and <see cref="EventArgs"/> pair of debug data to the 
        /// <see cref="ExtractException"/> and optionally encrypts.
        /// </summary>
        /// <param name="debugDataName">The key to associate with the data value. If 
        /// <see langword="null"/>, no data will be added to the debug data collection.</param>
        /// <param name="debugDataValue">The string to be added to the data collection.</param>
        /// <param name="encrypt">Specifies whether to encrypt the data value or not.</param>
        public void AddDebugData(string debugDataName, EventArgs debugDataValue, bool encrypt)
        {
            try
            {
                // Ensure the debug data is not null
                if (debugDataValue == null)
                {
                    AddDebugData(debugDataName, (object)debugDataValue, encrypt);
                    return;
                }

                // Get the type of this class
                Type type = debugDataValue.GetType();

                // NOTE: Code could be added here to specially handle certain EventArgs-derived 
                // classes.

                // Iterate through the public properties of the debug data
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    // Skip this property if it is set-only
                    if (!property.CanRead)
                    {
                        continue;
                    }

                    // Skip this property if it is indexed
                    ParameterInfo[] parameters = property.GetIndexParameters();
                    if (parameters.Length > 0)
                    {
                        continue;
                    }

                    try
                    {
                        // Add the value of this property
                        object value = property.GetValue(debugDataValue, null);
                        AddDebugData(property.Name, value, encrypt);
                    }
                    catch
                    {
                        // Add that this property threw an exception
                        AddDebugData(property.Name, (object)"exception", encrypt);
                    }
                }
            }
            catch
            {
                // Ignore all exceptions
            }
        }

        /// <summary>
        /// Adds a key and <see cref="Control"/> pair of debug data to the 
        /// <see cref="ExtractException"/> and optionally encrypts.
        /// </summary>
        /// <param name="debugDataName">The key to associate with the data value. If 
        /// <see langword="null"/>, no data will be added to the debug data collection.</param>
        /// <param name="debugDataValue">The string to be added to the data collection.</param>
        /// <param name="encrypt">Specifies whether to encrypt the data value or not.</param>
        public void AddDebugData(string debugDataName, Control debugDataValue, bool encrypt)
        {
            try
            {
                // NOTE: Code could be added here to specially handle certain Control-derived 
                // classes.

                // Add the name of this control
                AddDebugData(debugDataName,
                    debugDataValue == null ? "null" : debugDataValue.Name, encrypt);

                // Add the control type if possible
                if (debugDataValue != null)
                {
                    AddDebugData("Control Type", debugDataValue.GetType().ToString(), encrypt);
                }
            }
            catch
            {
                // Ignore all exceptions
            }
        }

        /// <summary>
        /// Returns the exception as a stringized byte stram
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Stringized")]
        public string AsStringizedByteStream()
        {
            // Create a UCLIDException COM object and populate it with all the information
            // contained in this ExtractException object.
            UCLID_EXCEPTIONMGMTLib.COMUCLIDException uex = AsCppException();
            return uex.AsStringizedByteStream();
        }

        #endregion Public methods

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
        void RecordStackTrace()
        {
            lock (_thisLock)
            {
                // Update the encrypted stack trace if it has not yet been updated with this 
                // exception object's stack trace.
                if (!_stackTraceRecorded)
                {
                    RecordStackTrace(base.StackTrace);
                    _stackTraceRecorded = true;
                }
            }
        }

        /// <summary>
        /// Update the encrypted stack trace information with the provided stack trace.
        /// </summary>
        /// <param name="stackTrace">The stack trace to add to the encrypted stack trace
        /// information associated with this exception.</param>
        void RecordStackTrace(string stackTrace)
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
                // If the stack trace object has not yet been initialized, do so
                if (_stackTrace == null)
                {
                    _stackTrace = new Stack<string>();
                }

                // Parse the stack trace and update the internal stack trace variable.
                string[] stackTraceEntries = stackTrace.Split(new char[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in stackTraceEntries)
                {
                    if (s.Length != 0)
                    {
                        // Trim leading and trailing spaces, encrypt and push the
                        // stack trace entry onto our stack
                        _stackTrace.Push(_ENCRYPTED_PREFIX + NativeMethods.EncryptString(s.Trim()));
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
        /// Return a COMUCLIDException object containing information/data equivalent
        /// to the information/data stored in this exception object.
        /// </summary>
        /// <returns>A COMUCLIDException object containing information/data equivalent
        /// to the information/data stored in this exception object.</returns>
        UCLID_EXCEPTIONMGMTLib.COMUCLIDException AsCppException()
        {
            // Create a new COMUCLIDException object.
            UCLID_EXCEPTIONMGMTLib.COMUCLIDException ue =
                new UCLID_EXCEPTIONMGMTLib.COMUCLIDException();

            // The COMUCLIDException should be created differently based on the existence of 
            // inner exception
            if (InnerException == null)
            {
                // Create the COMUCLIDException with create from string
                ue.CreateFromString(_eliCode, Message);
            }
            else if (InnerException.GetType() == typeof(ExtractException))
            {
                // Create COMUCLIDException with an inner exception
                ue.CreateWithInnerException(_eliCode, Message,
                    ((ExtractException)InnerException).AsCppException());
            }
            else
            {
                // This should not happen because the inner exception should already be 
                // an ExtractException, but if not should convert the inner exception
                // to ExtractException
                ExtractException exInner = AsExtractException("ELI21296", InnerException);
                ue.CreateWithInnerException(_eliCode, Message, exInner.AsCppException());
            }

            lock (_thisLock)
            {
                // Add all of the debug information
                foreach (DictionaryEntry de in base.Data)
                {
                    ue.AddDebugInfo(de.Key.ToString(), de.Value.ToString());
                }

                // Add stack trace data
                if (_stackTrace != null)
                {
                    foreach (string s in _stackTrace)
                    {
                        ue.AddStackTraceEntry(s);
                    }
                }
            }

            // Return the equivalent COMUCLIDException object.
            return ue;
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
                sb.Append(_eliCode);
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
                if (_stackTrace != null && _stackTrace.Count > 0)
                {
                    if (indentLevel > 0)
                    {
                        sb.Append("   ");
                    }

                    sb.Append(indent);
                    sb.Append("[Stacktrace]");
                    sb.Append(Environment.NewLine);
                    while (_stackTrace.Count > 0)
                    {
                        if (indentLevel > 0)
                        {
                            sb.Append("   ");
                        }

                        sb.Append(indent);
                        sb.Append(_stackTrace.Pop());
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
                        object dataValue = "null";
                        if (debugDataValue != null)
                        {
                            // If the value is not serializable get its string representation
                            dataValue = debugDataValue.GetType().IsSerializable ?
                                debugDataValue : debugDataValue.ToString();
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

                        // Add the debug data (encrypting if encrypt is true and debugDataValue
                        // is not null)
                        Data.Add(uniqueDebugDataName,
                            ((encrypt && debugDataValue != null) ?
                            _ENCRYPTED_PREFIX +
                            NativeMethods.EncryptString(dataValue.ToString())
                            : dataValue));
                    }
                }
                // debugDataName is null or empty, do nothing
            }
            catch (Exception ex)
            {
                new ExtractException("ELI21253", "Failed to add debug data.", ex).Log();
            }
        }

        #endregion Private methods

        #region Static methods

        /// <summary>
        /// Log the specified exception to the specified exception log.
        /// </summary>
        /// <param name="fileName">The file to log exceptions to.</param>
        /// <param name="eliCode">Unique identifier for the location of the exception.</param>
        /// <param name="ex">The exception to log.</param>
        public static void Log(string fileName, string eliCode, Exception ex)
        {
            ExtractException ee = AsExtractException(eliCode, ex);
            ee.Log(fileName);
        }

        /// <summary>
        /// Log the specified exception to the standard exception log used by all of
        /// Extract Systems' products.
        /// </summary>
        /// <param name="eliCode">Unique identifier for the location of the exception.</param>
        /// <param name="ex">The exception to log.</param>
        public static void Log(string eliCode, Exception ex)
        {
            // Convert the specified exception into an ExtractException and log it.
            ExtractException ee = AsExtractException(eliCode, ex);
            ee.Log();
        }

        /// <summary>
        /// Display the specified exception using the standard exception viewer window
        /// used by all of Extract Systems' products.
        /// </summary>
        /// <param name="eliCode">Unique identifier for the location of the exception.</param>
        /// <param name="ex">The exception to display.</param>
        public static void Display(string eliCode, Exception ex)
        {
            // Convert the specified exception into an ExtractException and display it.
            ExtractException ee = AsExtractException(eliCode, ex);
            ee.Display();
        }

        /// <summary>
        /// Return an ExtractException object equivalent to the specified 
        /// stringized C++/COM exception.
        /// </summary>
        /// <param name="eliCode">The ELI code to associate with this 
        /// exception transformation.</param>
        /// <param name="e">The C++ exception that has propagated into the 
        /// .NET framework via COM.</param>
        /// <returns>An ExtractException object equivalent to the 
        /// specified C++/COM exception.</returns>
        // This method does not raise a reserved exception. It wraps a stringized c++ exception in 
        // an Exception class for the purpose of converting it to an ExtractException.
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        static ExtractException FromCppException(string eliCode, Exception e)
        {
            UCLID_EXCEPTIONMGMTLib.COMUCLIDException uc =
                new UCLID_EXCEPTIONMGMTLib.COMUCLIDException();
            uc.CreateFromString(eliCode, e.Message);

            // Get the inner exception if there is one
            UCLID_EXCEPTIONMGMTLib.COMUCLIDException ucInner = uc.GetInnerException();

            // Create the ExtractException
            ExtractException ee;
            if (ucInner != null)
            {
                // Create with inner exception
                ee = new ExtractException(uc.GetTopELICode(), uc.GetTopText(),
                    FromCppException("ELI21463", new Exception(ucInner.AsStringizedByteStream())));
            }
            else
            {
                // Create without inner exception
                ee = new ExtractException(uc.GetTopELICode(), uc.GetTopText());
            }

            // Add data from the C++ exception
            int debugCount = uc.GetDebugInfoCount();
            for (int i = 0; i < debugCount; i++)
            {
                string debugKey, debugValue;
                uc.GetDebugInfo(i, out debugKey, out debugValue);
                ee.AddDebugData(debugKey, debugValue, false);
            }

            // Record the stack trace from the C++ exception
            int stackTraceCount = uc.GetStackTraceCount();
            for (int i = 0; i < stackTraceCount; i++)
            {
                ee.RecordStackTrace(uc.GetStackTraceEntry(i));
            }
            ee.RecordStackTrace(e.StackTrace);
            return ee;
        }

        /// <summary>
        /// Convert the specified exception into an ExtractException if the specified exception
        /// is not already an ExtractException.  If the specified exception is not already an
        /// ExtractException, it should be a standard .NET exception (which may or may not have 
        /// inner exceptions), or a C++/COM exception that was propagated into the .NET framework
        /// and wrapped as a standard .NET exception.
        /// </summary>
        /// <param name="eliCode">The ELI code to associate with 
        /// this exception transformation.</param>
        /// <param name="ex">The Exception object to return as an equivalent 
        /// ExtractException object.</param>
        /// <returns>An ExtractException object equivalent to the specified 
        /// Exception object.</returns>
        public static ExtractException AsExtractException(string eliCode, Exception ex)
        {
            try
            {
                // Check if e is already an ExtractException object
                ExtractException ee = ex as ExtractException;
                if (ee != null)
                {
                    ee.AddDebugData("CatchELI", eliCode, false);
                    return ee;
                }

                // Check if e is a RemoteExtractException
                RemoteExtractException re = ex as RemoteExtractException;
                if (re != null)
                {
                    // Return the remote extract exception as an extract exception
                    ee = re.Exception;
                    ee.AddDebugData("CatchELI", eliCode, false);
                    return ee;
                }

                // Check if e is an Exception object that was created on the .NET side because
                // a UCLIDException object was thrown from one of our C++/COM objects
                if (ex.Message.StartsWith("15000000", StringComparison.Ordinal) ||
                    ex.Message.StartsWith("1f000000", StringComparison.Ordinal))
                {
                    return FromCppException(eliCode, ex);
                }

                // At this time, we know that e is just a standard .NET Exception object.  We just
                // need to check whether e has inner exceptions and return a ExtractException hierarchy
                // representing the same hierarchy and data as e and its inner exceptions.
                if (ex.InnerException == null)
                {
                    ExtractException ex2 = new ExtractException(eliCode, ex.Message);
                    ex2.CopyInformationFrom(ex);
                    return ex2;
                }
                else
                {
                    ExtractException exInner = AsExtractException("ELI21073", ex.InnerException);
                    ExtractException ex2 = new ExtractException(eliCode, ex.Message, exInner);
                    ex2.CopyInformationFrom(ex);
                    return ex2;
                }
            }
            catch
            {
                ExtractException ee = new ExtractException("ELI26489",
                    "Failed converting to ExtractException");
                ee.AddDebugData("Exception Message", ex.Message, true);

                return ee;
            }
        }

        /// <summary>
        /// Create an <see cref="ExtractException"/> from a previously stringized exception.
        /// </summary>
        /// <param name="eliCode">The ELI code for a new history entry associated with the
        /// conversion.</param>
        /// <param name="stringizedException">The stringized excpetion.</param>
        /// <returns>An <see cref="ExtractException"/> that results from re-instantiating the
        /// stringized exception.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "stringized")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Stringized")]
        public static ExtractException FromStringizedByteStream(string eliCode, 
            string stringizedException)
        {
            // Re-create an exception instance from a stringized version.
            return FromCppException(eliCode, new Exception(stringizedException));
        }

        /// <summary>
        /// Create an <see cref="ExtractException"/> that can be thrown from a COM visible method.
        /// </summary>
        /// <param name="eliCode">The ELI code for the new exception.</param>
        /// <param name="message">The message string associated with the exception.</param>
        /// <param name="ex">The exception from which the <see cref="ExtractException"/> will be 
        /// created.</param>
        /// <returns>An <see cref="ExtractException"/> that can be thrown from a COM visible 
        /// method.</returns>
        public static ExtractException CreateComVisible(string eliCode, string message, Exception ex)
        {
            ExtractException ee = new ExtractException(eliCode, message, ex);

            // Stringize ee so that COM can get the data from it. If the ee exception is thrown 
            // across a COM boundary the line position of the original exception will be lost.
            return new ExtractException("ELI26464", ee.AsStringizedByteStream());
        }

        /// <summary>
        /// Throws an ExtractException built from the provided ELICode containing no debug data
        /// if the condition provided is false, otherwise does nothing.
        /// </summary>
        /// <overloads>This method has five overloads</overloads>
        /// <param name="eliCode">A unique Extract Systems ELI Code</param>
        /// <param name="message">The message to associate with this exception</param>
        /// <param name="condition">An expression that evaluates to a boolean</param>
        // Added as per [DotNetRCAndUtils #4] - JDS - 05/19/2008
        public static void Assert(string eliCode, string message, bool condition)
        {
            Assert(eliCode, message, condition, null, null, null, null, null, null, null, null);
        }

        /// <summary>
        /// Throws an ExtractException built from the provided ELICode containing a single
        /// debug data pair if the condition provided is false, otherwise does
        /// nothing.
        /// </summary>
        /// <param name="eliCode">A unique Extract Systems ELI Code</param>
        /// <param name="message">The message to associate with this exception</param>
        /// <param name="condition">An expression that evaluates to a boolean</param>
        /// <param name="debugDataName">A string describing the debugDataValue.  If this
        /// value is null or empty string then the data will not be added.</param>
        /// <param name="debugDataValue">Debug data to be added to the exception</param>
        /// <exception cref="ExtractException">Thrown if <paramref name="condition"/>
        /// evaluates to false.
        /// </exception>
        public static void Assert(string eliCode, string message, bool condition,
            string debugDataName, object debugDataValue)
        {
            Assert(eliCode, message, condition, debugDataName, debugDataValue, null, null,
                null, null, null, null);
        }

        /// <summary>
        /// Throws an ExtractException built from the provided ELICode containing two
        /// debug data pairs if the condition provided is false, otherwise does
        /// nothing.
        /// </summary>
        /// <param name="eliCode">A unique Extract Systems ELI Code</param>
        /// <param name="message">The message to associate with this exception</param>
        /// <param name="condition">An expression that evaluates to a boolean</param>
        /// <param name="debugDataName1">A string describing the debugDataValue.  If this
        /// value is null or empty string then this data and all subsequent debug data
        /// will not be added.</param>
        /// <param name="debugDataValue1">Debug data to be added to the exception</param>
        /// <param name="debugDataName2">A string describing the debugDataValue.  If this
        /// value is null or empty string then this data and all subsequent debug data
        /// will not be added.</param>
        /// <param name="debugDataValue2">Debug data to be added to the exception</param>
        /// <exception cref="ExtractException">Thrown if <paramref name="condition"/>
        /// evaluates to false.
        /// </exception>
        public static void Assert(string eliCode, string message, bool condition,
            string debugDataName1, object debugDataValue1,
            string debugDataName2, object debugDataValue2)
        {
            Assert(eliCode, message, condition, debugDataName1, debugDataValue1,
                debugDataName2, debugDataValue2, null, null, null, null);
        }

        /// <summary>
        /// Throws an ExtractException built from the provided ELICode containing three
        /// debug data pairs if the condition provided is false, otherwise does
        /// nothing.
        /// </summary>
        /// <param name="eliCode">A unique Extract Systems ELI Code</param>
        /// <param name="message">The message to associate with this exception</param>
        /// <param name="condition">An expression that evaluates to a boolean</param>
        /// <param name="debugDataName1">A string describing the debugDataValue.  If this
        /// value is null or empty string then this data and all subsequent debug data
        /// will not be added.</param>
        /// <param name="debugDataValue1">Debug data to be added to the exception</param>
        /// <param name="debugDataName2">A string describing the debugDataValue.  If this
        /// value is null or empty string then this data and all subsequent debug data
        /// will not be added.</param>
        /// <param name="debugDataValue2">Debug data to be added to the exception</param>
        /// <param name="debugDataName3">A string describing the debugDataValue.  If this
        /// value is null or empty string then this data and all subsequent debug data
        /// will not be added.</param>
        /// <param name="debugDataValue3">Debug data to be added to the exception</param>
        /// <exception cref="ExtractException">Thrown if <paramref name="condition"/>
        /// evaluates to false.
        /// </exception>
        public static void Assert(string eliCode, string message, bool condition,
            string debugDataName1, object debugDataValue1,
            string debugDataName2, object debugDataValue2,
            string debugDataName3, object debugDataValue3)
        {
            Assert(eliCode, message, condition, debugDataName1, debugDataValue1,
                debugDataName2, debugDataValue2, debugDataName3, debugDataValue3, null, null);
        }

        /// <summary>
        /// Throws an ExtractException built from the provided ELICode containing four
        /// debug data pairs if the condition provided is false, otherwise does
        /// nothing.
        /// </summary>
        /// <param name="eliCode">A unique Extract Systems ELI Code</param>
        /// <param name="message">The message to associate with this exception</param>
        /// <param name="condition">An expression that evaluates to a boolean</param>
        /// <param name="debugDataName1">A string describing the debugDataValue.  If this
        /// value is null or empty string then this data and all subsequent debug data
        /// will not be added.</param>
        /// <param name="debugDataValue1">Debug data to be added to the exception</param>
        /// <param name="debugDataName2">A string describing the debugDataValue.  If this
        /// value is null or empty string then this data and all subsequent debug data
        /// will not be added.</param>
        /// <param name="debugDataValue2">Debug data to be added to the exception</param>
        /// <param name="debugDataName3">A string describing the debugDataValue.  If this
        /// value is null or empty string then this data and all subsequent debug data
        /// will not be added.</param>
        /// <param name="debugDataValue3">Debug data to be added to the exception</param>
        /// <param name="debugDataName4">A string describing the debugDataValue.  If this
        /// value is null or empty string then this data and all subsequent debug data
        /// will not be added.</param>
        /// <param name="debugDataValue4">Debug data to be added to the exception</param>
        /// <exception cref="ExtractException">Thrown if <paramref name="condition"/>
        /// evaluates to false.
        /// </exception>
        // This method throws an ExtractException if the condition is false. It has been
        // checked and should not throw any other exceptions.
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public static void Assert(string eliCode, string message, bool condition,
            string debugDataName1, object debugDataValue1,
            string debugDataName2, object debugDataValue2,
            string debugDataName3, object debugDataValue3,
            string debugDataName4, object debugDataValue4)
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
                if (!string.IsNullOrEmpty(debugDataName1))
                {
                    ee.AddDebugData(debugDataName1, debugDataValue1, false);

                    if (!string.IsNullOrEmpty(debugDataName2))
                    {
                        ee.AddDebugData(debugDataName2, debugDataValue2, false);

                        if (!string.IsNullOrEmpty(debugDataName3))
                        {
                            ee.AddDebugData(debugDataName3, debugDataValue3, false);

                            if (!string.IsNullOrEmpty(debugDataName4))
                            {
                                ee.AddDebugData(debugDataName4, debugDataValue4, false);
                            }
                        }
                    }
                }

                // Throw the new exception
                throw ee;
            }
        }

        /// <summary>
        /// Throws an <see cref="ExtractException"/>indicating an internal
        /// logic error has occurred.
        /// </summary>
        /// <param name="eliCode">A unique Extract Systems ELI code.</param>
        /// <exception cref="ExtractException">Always thrown.</exception>
        public static void ThrowLogicException(string eliCode)
        {
            throw new ExtractException(eliCode, "Internal logic error.");
        }

        /// <overloads>
        /// Loads an extract exception from an exception log file.
        /// </overloads>
        /// <summary>
        /// Loads the first extract exception from an exception log file.
        /// </summary>
        /// <param name="eliCode">The ELI code to associate with the loaded exception.</param>
        /// <param name="fileName">The name of the exception file
        /// to load the exception from.</param>
        /// <returns>The exception loaded from the file.</returns>
        /// <exception cref="ExtractException">If <paramref name="fileName"/>
        /// is <see langword="null"/> or <see cref="String.Empty"/>, or if
        /// <paramref name="fileName"/> does not exist.</exception> 
        public static ExtractException LoadFromFile(string eliCode, string fileName)
        {
            return LoadFromFile(eliCode, fileName, 1);
        }

        /// <summary>
        /// Loads the specified extract exception from an exception log file.
        /// </summary>
        /// <param name="eliCode">The ELI code to associate with the loaded exception.</param>
        /// <param name="fileName">The name of the exception file
        /// to load the exception from.</param>
        /// <param name="line">The 1-based line number in the file to load.</param>
        /// <exception cref="ExtractException">If <paramref name="fileName"/>
        /// is <see langword="null"/> or <see cref="String.Empty"/>, or if
        /// <paramref name="fileName"/> does not exist, or if <paramref name="line"/>
        /// is &lt; 1 or &gt; the number of lines in <paramref name="fileName"/>.
        /// </exception>
        public static ExtractException LoadFromFile(string eliCode, string fileName, int line)
        {
            try
            {
                Assert("ELI30275", "Filename cannot be null or empty and must exist.",
                    !string.IsNullOrEmpty(fileName) && File.Exists(fileName));

                string[] lines = File.ReadAllLines(fileName);
                return LoadFromFile(eliCode, lines, line, fileName);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30279", ex);
            }
        }

        /// <overloads>
        /// Loads all exceptions from an exception log file.
        /// </overloads>
        /// <summary>
        /// Loads all exceptions from an exception log file.
        /// </summary>
        /// <param name="eliCode">The ELI code to associate with the loaded exceptions.</param>
        /// <param name="fileName">The name of the exception file
        /// to load the exception from.</param>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of <see cref="ExtractException"/>s from the
        /// file.</returns>
        public static IEnumerable<ExtractException> LoadAllFromFile(string eliCode, string fileName)
        {
            return LoadAllFromFile(eliCode, fileName, false);
        }

        /// <summary>
        /// Loads all exceptions from an exception log file.
        /// </summary>
        /// <param name="eliCode">The ELI code to associate with the loaded exceptions.</param>
        /// <param name="fileName">The name of the exception file
        /// to load the exception from.</param>
        /// <param name="includeErrorsInOutput">If <see langword="true"/>, when an exception is
        /// generated due to an error while trying to parse an exception, the generated exception
        /// (which will have the ELI code "ELI30603") will be returned as if it were one of the
        /// exceptions that was read from the file. If <see langword="false"/>, the exception will
        /// be thrown.</param>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of <see cref="ExtractException"/>s from the
        /// file.</returns>
        public static IEnumerable<ExtractException> LoadAllFromFile(string eliCode, string fileName,
            bool includeErrorsInOutput)
        {
            string[] lines;

            try
            {
                Assert("ELI30594", "Filename cannot be null or empty and must exist.",
                    !string.IsNullOrEmpty(fileName) && File.Exists(fileName));

                lines = File.ReadAllLines(fileName);
            }
            catch (Exception ex)
            {
                ExtractException ee = AsExtractException("ELI30593", ex);
                ee.AddDebugData("File Name", fileName, false);
                throw ee;
            }

            for (int i = 1; i <= lines.Length; i++)
            {
                ExtractException ee;

                try
                {
                    ee = LoadFromFile(eliCode, lines, i, fileName);
                }
                catch (Exception ex)
                {
                    if (includeErrorsInOutput)
                    {
                        ee = new ExtractException("ELI30603", "Error reading exception from file.", ex);
                        ee.Log();
                    }
                    else
                    {
                        throw ExtractException.AsExtractException("ELI30604", ex);
                    }
                }

                yield return ee;
            }
        }

        /// <summary>
        /// Loads the specified extract exception from an exception log file.
        /// </summary>
        /// <param name="eliCode">The ELI code to associate with the loaded exception.</param>
        /// <param name="lines">The array of strings defining the exceptions in a file.</param>
        /// <param name="lineNumber">The 1-based line number in the file to load.</param>
        /// <param name="fileName">The name of the exception file the lines were loaded from
        /// (for debug purposes).</param>
        /// <exception cref="ExtractException">If <paramref name="lineNumber"/> is &lt; 1 or
        /// &gt; the number of lines in <paramref name="lines"/>.
        /// </exception>
        static ExtractException LoadFromFile(string eliCode, string[] lines, int lineNumber,
            string fileName)
        {
            try
            {
                if (lineNumber < 0 || lineNumber > lines.Length)
                {
                    ExtractException ee = new ExtractException("ELI30276",
                        "Invalid line specification.");
                    ee.AddDebugData("Line", lineNumber, false);
                    ee.AddDebugData("Total Lines In File", lines.Length, false);
                    throw ee;
                }

                string[] tokens = lines[lineNumber - 1].Split(',');
                if (tokens.Length != 7)
                {
                    ExtractException ee = new ExtractException("ELI30277",
                        "Invalid number of tokens in the exception file.");
                    ee.AddDebugData("Number of tokens", tokens.Length, false);
                    ee.AddDebugData("Line Parsed", lines[lineNumber - 1], false);
                    throw ee;
                }

                // The exception is the last token
                string exception = tokens[tokens.Length - 1];
                return ExtractException.FromStringizedByteStream(eliCode, exception);
            }
            catch (Exception ex)
            {
                ExtractException ee = AsExtractException("ELI30595", ex);
                ee.AddDebugData("File Name", fileName, false);
                throw ee;
            }
        }

        #endregion Static methods
    }
}
