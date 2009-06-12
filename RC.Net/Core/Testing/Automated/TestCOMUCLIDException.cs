using Extract;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using UCLID_EXCEPTIONMGMTLib;

namespace Extract.Test
{
    /// <summary>
    /// Class for testing the COMUCLIDException Object
    /// </summary>
    // Using C++ casing for consistency
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", 
        MessageId="COMUCLID")]
    [TestFixture]
    [Category("COMUCLIDException")]
    public class TestCOMUCLIDException
    {
        // This is the same as gstrSTRINGIZED_BYTE_STREAM_OF_SIGNATURE that is
        // defined in the UCLIDException.cpp file. 
        private static readonly string _STRINGIZED_BYTE_STREAM_OF_SIGNATURE = "1f000000";
        /// <summary>
        /// Test to verify that the ELICode and Description are set after calling CreateFromString.
        /// </summary>
        [Test]
        public static void CreateFromStringIsCopied()
        {
            COMUCLIDException cex = new COMUCLIDException();
            cex.CreateFromString("TESTELI", "TestText");
            Assert.That(cex.GetTopELICode() == "TESTELI" && cex.GetTopText() == "TestText");
        }
        /// <summary>
        /// Test to verify that the key value is set properly after a call to AddDebugInfo.
        /// </summary>
        [Test]
        public static void DebugInfoIsAdded()
        {
            COMUCLIDException cex = new COMUCLIDException();
            cex.CreateFromString("TESTELI", "TestText");
            cex.AddDebugInfo("TestKey", "TestValue");

            // Get debug info.
            string keyValue;
            string debugValue;
            cex.GetDebugInfo(0, out keyValue, out debugValue);

            // Test to make sure debug info was set
            Assert.That(cex.GetDebugInfoCount() == 1 && 
                keyValue == "TestKey" && debugValue == "TestValue");
        }
        /// <summary>
        /// Test to verify the stack trace count and entries after adding 3 entries.
        /// </summary>
        [Test]
        public static void StackTraceEntryIsAdded()
        {
            COMUCLIDException cex = new COMUCLIDException();
            cex.CreateFromString("TESTELI", "TestText");

            // Add Stack trace entries
            cex.AddStackTraceEntry("Test stack trace entry 1.");
            cex.AddStackTraceEntry("Test stack trace entry 2.");
            cex.AddStackTraceEntry("Test stack trace entry 3.");

            // Get the stack trace count
            int stackTraceCount = cex.GetStackTraceCount();

            // Get stack trace entry
            string stackTraceEntry1 = cex.GetStackTraceEntry(0);
            string stackTraceEntry2 = cex.GetStackTraceEntry(1);
            string stackTraceEntry3 = cex.GetStackTraceEntry(2);

            // Verify that the count and stack trace entry are the as expected
            Assert.That(stackTraceCount == 3 
                && stackTraceEntry1 == "Test stack trace entry 1."
                && stackTraceEntry2 == "Test stack trace entry 2."
                && stackTraceEntry3 == "Test stack trace entry 3.");
        }
        /// <summary>
        /// Test to verify the first 8 bytes of StringizedByteStream indicates an 
        /// Extract Exception.
        /// </summary>
        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", 
            MessageId = "Stringized")]
        public static void AsStringizedByteStreamIsValid()
        {
            COMUCLIDException cex = new COMUCLIDException();
            cex.CreateFromString("TESTELI", "TestText");

            // Get exception as a string
            string ExceptionAsString = cex.AsStringizedByteStream();

            // Get the signature at the beginning of the string
            int signatureLength = _STRINGIZED_BYTE_STREAM_OF_SIGNATURE.Length;
            string ExceptionSignature = ExceptionAsString.Substring(0, signatureLength);
            
            // String should begin with exception signature
            Assert.That(ExceptionSignature == _STRINGIZED_BYTE_STREAM_OF_SIGNATURE);
        }
        /// <summary>
        /// Test to verify the ELI Code and Text of exception created from a call to 
        /// AsStringizedByteStream is the same as the original exception.
        /// </summary>
        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", 
            MessageId = "Stringized")]
        public static void CreateWithStringFromStringizedIsValid()
        {
            COMUCLIDException cex = new COMUCLIDException();
            cex.CreateFromString("TESTELI", "TestText");

            // Get exception as string
            string ExceptionAsString = cex.AsStringizedByteStream();

            // Creat an exception from the string
            COMUCLIDException cexFromString = new COMUCLIDException();
            cexFromString.CreateFromString("TESTFROMSTRING", ExceptionAsString);

            // Exception should have eli Code of TESTELI and Text TestText
            Assert.That(cexFromString.GetTopELICode() == "TESTELI" &&
                cexFromString.GetTopText() == "TestText");
        }
        /// <summary>
        /// Test to verify that an exception with an inner exception works correctly.
        /// </summary>
        [Test]
        public static void CreateWithInnerExceptionIsValid()
        {
            // Create an inner exception
            COMUCLIDException cexInnerException = new COMUCLIDException();
            cexInnerException.CreateFromString("INNER", "InnerExceptionText");

            // Create the outer exception with the inner exception
            COMUCLIDException cex = new COMUCLIDException();
            cex.CreateWithInnerException("TESTELI", "TestText", cexInnerException);

            // Need to test all parts
            bool valid = cex.GetTopELICode() == "TESTELI";
            valid = valid && cex.GetTopText() == "TestText";

            // Get the inner exception
            COMUCLIDException cexReturnedInner = cex.GetInnerException();

            // Make sure the inner exception values are correct
            valid = valid && cexReturnedInner.GetTopELICode() == "INNER";
            valid = valid && cexReturnedInner.GetTopText() == "InnerExceptionText";
            valid = valid && cexReturnedInner.GetInnerException() == null;
            Assert.That(valid);
        }
        /// <summary>
        /// Test to verify that an exception with an inner exception works when converted to
        /// a StringizedException and is created properly using CreateFromString.
        /// </summary>
        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", 
            MessageId = "Stringized")]
        public static void CreateWithInnerExceptionAsStringizedIsValid()
        {
            // Create inner exception
            COMUCLIDException cexInnerException = new COMUCLIDException();
            cexInnerException.CreateFromString("INNER", "InnerExceptionText");

            // Create outer exception with inner exception
            COMUCLIDException cex = new COMUCLIDException();
            cex.CreateWithInnerException("TESTELI", "TestText", cexInnerException);

            // Get the stringized exception
            string StringizedException = cex.AsStringizedByteStream();

            // Create exception from the StringizedException
            COMUCLIDException cse = new COMUCLIDException();
            cse.CreateFromString("IGNOREELI", StringizedException);

            // Need to test all parts
            bool valid = cse.GetTopELICode() == "TESTELI";
            valid = valid && cse.GetTopText() == "TestText";

            // Get the inner exception
            COMUCLIDException cseReturnedInner = cse.GetInnerException();

            // Make sure the inner exception values are correct
            valid = valid && cseReturnedInner.GetTopELICode() == "INNER";
            valid = valid && cseReturnedInner.GetTopText() == "InnerExceptionText";
            valid = valid && cseReturnedInner.GetInnerException() == null;
            Assert.That(valid);
        }
    }
}
