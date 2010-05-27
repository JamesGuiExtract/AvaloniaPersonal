using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace Extract
{
    /// <summary>
    /// A special exception class used to pass <see cref="ExtractException"/> back
    /// from .Net remoting calls.
    /// <para><b>Note:</b></para>
    /// This class should only be used to throw exceptions out of .Net remoting
    /// classes (objects which inherit from <see cref="MarshalByRefObject"/>
    /// </summary>
    // Added so that FXCop will not complain that we have not implemented the standard
    // exception constructors.  We have intentionally not implemented them so that you
    // cannot create an RemoteExtractException without providing an exception.  If it is
    // discovered in later testing that we need to implement the default constructors
    // due to some issue in the framework we can then remove this suppress message.
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public sealed class RemoteExtractException : RemotingException, ISerializable
    {
        #region Constants

        /// <summary>
        /// The current version number for the <see cref="RemoteExtractException"/>
        /// class.
        /// </summary>
        int _CURRENT_VERSION = 1;

        /// <summary>
        /// Default string for the exception byte stream if no exception is provided.
        /// </summary>
        static readonly string _DEFAULT_EXCEPTION_STRING = GetDefaultExceptionValue();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The exception byte stream from an <see cref="ExtractException"/> object.
        /// </summary>
        string _extractExceptionByteStream;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteExtractException"/> class.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> to build the remote exception from.</param>
        public RemoteExtractException(Exception ex)
        {
            try
            {
                if (ex != null)
                {
                    ExtractException ee = ex as ExtractException;
                    if (ee != null)
                    {
                        _extractExceptionByteStream = ee.AsStringizedByteStream();
                    }
                    else
                    {
                        _extractExceptionByteStream =
                            ExtractException.AsExtractException("ELI30139", ex).AsStringizedByteStream();
                    }
                }
                else
                {
                    _extractExceptionByteStream = _DEFAULT_EXCEPTION_STRING;
                }

            }
            catch (Exception ex2)
            {
                // Log the exception since you should not throw exceptions from an exception
                // class constructor
                ExtractException.Log("ELI30140", ex2);
            }
        }

        #endregion Constructors

        #region ISerializable Members

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteExtractException"/> class.
        /// </summary>
        /// <param name="info">The info object to read the data from.</param>
        /// <param name="context">The serialization context for the object.</param>
        private RemoteExtractException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            try
            {
                int version = (int)info.GetValue("VersionNumber", typeof(int));

                if (version > _CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI30141",
                        "Unable to load newer remote exception version.");
                    ee.AddDebugData("Version To Load", version, false);
                    ee.AddDebugData("Current Version", _CURRENT_VERSION, false);
                    throw ee;
                }

                _extractExceptionByteStream =
                    (string) info.GetValue("ExceptionByteStream", typeof(string));
            }
            catch (Exception ex)
            {
                // Log the exception since you should not throw exceptions from an exception
                // class constructor
                ExtractException.Log("ELI30142", ex);
            }
        }

        /// <summary>
        /// Gets the object data from this object into the serialization info.
        /// </summary>
        /// <param name="info">The info object to read the data from.</param>
        /// <param name="context">The serialization context for the object.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Call the base class first
            base.GetObjectData(info, context);

            info.AddValue("VersionNumber", _CURRENT_VERSION);
            info.AddValue("ExceptionByteStream", _extractExceptionByteStream);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the exception information.
        /// </summary>
        public override string Message
        {
            get
            {
                return _extractExceptionByteStream;
            }
        }

        /// <summary>
        /// The stack trace for the exception.
        /// </summary>
        public override string StackTrace
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets an <see cref="ExtractException"/> for the stringized byte stream
        /// contained within this object.
        /// </summary>
        /// <returns>An <see cref="ExtractException"/> created from the stringized
        /// byte stream contained within this object.</returns>
        // This property is not raising an exception, it is creating one from the internal
        // stringized bytestream.
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public ExtractException Exception
        {
            get
            {
                return new ExtractException("ELI30143", _extractExceptionByteStream);
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the default exception stringized bytestream for this class.
        /// </summary>
        /// <returns>A stringized bytestream of the default exception value.</returns>
        static string GetDefaultExceptionValue()
        {
            ExtractException ee = new ExtractException(
                "ELI30138", "No exception data available.");
            return ee.AsStringizedByteStream();
        }

        #endregion Methods
    }
}
