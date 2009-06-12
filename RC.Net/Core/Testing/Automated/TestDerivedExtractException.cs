using Extract;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Text;

[module: SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", 
    Scope="type", Target="Extract.DerivedExtractException")]
namespace Extract.Test
{
    /// <summary>
    /// Sample for testing deriving from ExtractException.  Unless specificially overridden
    /// this class maintains the behavior of the underlying ExtractException class 
    /// (e.g. all inner exceptions will be wrapped as ExtractExceptions, 
    /// not DerivedExtractExceptions)
    /// </summary>
    // Standard constructors are unnecessary for testing purposes
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public sealed class DerivedExtractException : ExtractException 
    {
        private string _DerivedExtractExceptionCode;

        /// <summary>
        /// Constructor for the derived ExtractException
        /// </summary>
        /// <param name="derivedCode">The derived code value</param>
        /// <param name="eliCode">The eliCode associated with this exception</param>
        /// <param name="message">The message associated with this exception</param>
        public DerivedExtractException(string derivedCode, string eliCode, string message)
            : base(eliCode, message)
        {
            _DerivedExtractExceptionCode = derivedCode;
        }

        /// <summary>
        /// Constructor for the derived ExtractException
        /// </summary>
        /// <param name="derivedCode">The derived code value</param>
        /// <param name="eliCode">The eliCode associated with this exception</param>
        /// <param name="message">The message associated with this exception</param>
        /// <param name="ex">The inner exception to be added to this exception</param>
        public DerivedExtractException(string derivedCode, string eliCode, string message,
            Exception ex)
            : base(eliCode, message, ex)
        {
            _DerivedExtractExceptionCode = derivedCode;
        }

        /// <summary>
        /// The private serialization constructor
        /// </summary>
        /// <param name="info">The serialization info object</param>
        /// <param name="context">The streaming context</param>
        private DerivedExtractException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _DerivedExtractExceptionCode = info.GetString("_DerivedExtractExceptionCode");
        }

        /// <summary>
        /// The overridden GetObjectData method
        /// </summary>
        /// <param name="info">The serialization info object</param>
        /// <param name="context">The streaming context</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Order is important, call the base GetObjectData first
            base.GetObjectData(info, context);

            // Now add additional data
            info.AddValue("_DerivedExtractExceptionCode", _DerivedExtractExceptionCode);
        }

        /// <summary>
        /// Returns this exception object as a string
        /// </summary>
        /// <returns>A stringized representation of this exception object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append(Environment.NewLine);
            sb.Append("_DerivedExceptionCode: ");
            sb.Append(_DerivedExtractExceptionCode);

            return sb.ToString();
        }
    }

    /// <summary>
    /// Class for testing classes derived from ExtractException
    /// </summary>
    [TestFixture]
    [Category("ExtractException")]
    [Category("DerivedExtractException")]
    public class TestDerivedExtractException
    {
        /// <summary>
        /// Test to verify that accessing the StackTrace property on a DerivedExtractException
        /// returns an empty string
        /// </summary>
        [Test]
        public static void StackTraceIsEmpty()
        {
            // Create a new ExtractException
            DerivedExtractException dee = new DerivedExtractException("DLI00000", "ELI00000", 
                "Fake test exception!");

            // Get the stack trace from the ExtractException
            string stackTrace = dee.StackTrace;

            // Check that the stack trace is empty
            Assert.That(stackTrace.Length == 0);
        }

        /// <summary>
        /// Test to verify that accessing the StackTrace property of an inner exception
        /// on a DerivedExtractException returns an empty string
        /// </summary>
        [Test]
        public static void InnerExceptionStackTraceIsEmpty()
        {
            // Variable to hold the inner stack trace, initialize it to non-empty
            string innerStackTrace = "Has a stack trace!";

            try
            {
                // Throw a fake exception
                throw new ArithmeticException(
                    "This exception will be wrapped as the inner exception.");
            }
            catch (Exception ex)
            {
                // Catch the exception and add it to a new DerivedExtractException
                // as an inner exception
                DerivedExtractException dex = new DerivedExtractException("DLI00000", "ELI00000", 
                    "Fake testing exception!", ex);

                // Get the stack trace from the inner exception
                innerStackTrace = dex.InnerException.StackTrace;
            }

            // Check that the stack trace is empty
            Assert.That(innerStackTrace.Length == 0);
        }

        /// <summary>
        /// Test to verify that the inner exception of an DerivedExtractException is a 
        /// ExtractException.
        /// </summary>
        [Test]
        public static void InnerExceptionIsExtractException()
        {
            bool innerExceptionIsExtract = false;

            try
            {
                // Throw a fake exception
                throw new ArithmeticException(
                    "This exception will be wrapped as the inner exception.");
            }
            catch (Exception ex)
            {
                // Catch the exception and add it to a new DerivedExtractException
                // as an inner exception
                DerivedExtractException dex = new DerivedExtractException("DLI00000", "ELI00000", 
                    "Fake testing exception!", ex);

                // Check that the type of the inner exception is now ExtractException
                innerExceptionIsExtract = dex.InnerException is ExtractException;
            }

            Assert.That(innerExceptionIsExtract);
        }

        /// <summary>
        /// Tests if the derived exception can be serialized and deserialized successfully
        /// </summary>
        [Test]
        public static void SerializeDerivedExtractException()
        {
            string tempFile = "";
            FileStream streamOut = null;
            FileStream streamIn = null;

            try
            {
                // Create the derived exception
                DerivedExtractException dee = new DerivedExtractException("DLI00000", "ELI00000",
                    "Fake testing exception!");

                // Create a temporary file and open it for writing
                tempFile = Path.GetTempFileName();
                streamOut = new FileStream(tempFile, FileMode.Create);

                // Create a new binary format serializer
                BinaryFormatter formatter = new BinaryFormatter();

                // Serialize the derived exception to the stream
                formatter.Serialize(streamOut, dee);
                streamOut.Flush();

                // Close the stream
                streamOut.Close();
                streamOut = null;

                // Open the temporary file for reading
                streamIn = new FileStream(tempFile, FileMode.Open);
                formatter = new BinaryFormatter();

                // Now read the derived exception from the stream
                DerivedExtractException streamedDex =
                    (DerivedExtractException)formatter.Deserialize(streamIn);

                // Close the input stream
                streamIn.Close();
                streamIn = null;

                // Delete the temporary file
                File.Delete(tempFile);

                // Assert that the exception and the streamed exception are the same
                Assert.That(dee.ToString() == streamedDex.ToString());
            }
            catch
            {
                // Ensure the streams are closed
                if (streamOut != null)
                {
                    streamOut.Close();
                }
                if (streamIn != null)
                {
                    streamIn.Close();
                }

                // Ensure the temporary file is deleted
                if (tempFile.Length != 0)
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            }
        }
    }
}
