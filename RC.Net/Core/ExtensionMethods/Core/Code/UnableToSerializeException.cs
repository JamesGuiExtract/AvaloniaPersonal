using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Extract
{
    /// <summary>
    /// <see cref="Exception"/> thrown when a <see cref="Exception"/> is not serializable.
    /// This class will try to extract some data from the non-serializable <see cref="Exception"/>
    /// and add it as its own exception data. All inner exceptions will be lost, but the stack
    /// trace will be preserved.
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public sealed class UnableToSerializeException : Exception
    {
        /// <summary>
        /// The current version of this exception class.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// Holds the non-serializable exceptions stack trace
        /// </summary>
        string _stackTrace = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnableToSerializeException"/> class.
        /// </summary>
        /// <param name="eliCode">The ELICode.</param>
        /// <param name="notSerializable">The exception that was not serializable.</param>
        public UnableToSerializeException(string eliCode, Exception notSerializable)
            : this(eliCode, notSerializable, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnableToSerializeException"/> class.
        /// </summary>
        /// <param name="eliCode">The eli code.</param>
        /// <param name="notSerializable">The exception that was not serializable.</param>
        /// <param name="innerSerializationException">The serialization exception that occurred. This will be added as
        /// the inner exception to this exception.</param>
        [SuppressMessage("ExtractRules", "ES0002:MethodsShouldContainValidEliCodes")]
        public UnableToSerializeException(string eliCode, Exception notSerializable,
            SerializationException innerSerializationException)
            : base(notSerializable.Message, innerSerializationException)
        {
            _stackTrace = notSerializable.StackTrace;
            Data.Add("ELICode", eliCode);
            Data.Add("Not Serializable Type", notSerializable.GetType().ToString());
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="UnableToSerializeException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">
        /// The class name is <see langword="null"/> or <see cref="P:System.Exception.HResult"/> is zero (0).
        /// </exception>
        UnableToSerializeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Load the version number information first
            int version = info.GetInt32("_CURRENT_VERSION");
            if (version > _CURRENT_VERSION)
            {
                var se = new SerializationException("Attempted to load newer version of serialized object.");
                se.Data["CurrentVersion"] = _CURRENT_VERSION;
                se.Data["VersionToLoad"] = version;
                se.Data["TypeToLoad"] = typeof(UnableToSerializeException).ToString();
                throw se;
            }

            _stackTrace = info.GetString("_stackTrace");
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic).
        /// </exception>
        /// <PermissionSet>
        /// <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/>
        /// <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/>
        /// </PermissionSet>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("_CURRENT_VERSION", _CURRENT_VERSION);
            info.AddValue("_stackTrace", _stackTrace);
        }

        /// <summary>
        /// Gets a string representation of the frames on the call stack at the time the current exception was thrown.
        /// </summary>
        /// <returns>
        /// A string that describes the contents of the call stack, with the most recent method call appearing first.
        /// </returns>
        /// <PermissionSet>
        /// <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*"/>
        /// </PermissionSet>
        public override string StackTrace
        {
            get
            {
                return _stackTrace;
            }
        }
    }
}
