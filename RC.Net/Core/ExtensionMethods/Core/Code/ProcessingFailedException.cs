using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Extract
{
    /// <summary>
    /// Helper class, derived from exception to indicate that processing failed.
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public sealed class ProcessingFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingFailedException"/> class.
        /// </summary>
        /// <param name="eliCode">The ELICode.</param>
        public ProcessingFailedException(string eliCode)
            : this(eliCode, "Processing failed.", null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingFailedException"/> class.
        /// </summary>
        /// <param name="eliCode">The ELICode.</param>
        /// <param name="message">The message.</param>
        public ProcessingFailedException(string eliCode, string message)
            : this(eliCode, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingFailedException"/> class.
        /// </summary>
        /// <param name="eliCode">The ELICode.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        [SuppressMessage("ExtractRules", "ES0002:MethodsShouldContainValidEliCodes")]
        public ProcessingFailedException(string eliCode, string message, Exception innerException) :
            base(message, innerException)
        {
            Data.Add("ELICode", eliCode);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingFailedException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">
        /// The class name is <see langword="null"/> or <see cref="P:System.Exception.HResult"/> is zero (0).
        /// </exception>
        ProcessingFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
