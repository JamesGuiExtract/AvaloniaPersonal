using Extract.ErrorHandling;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Extract.ErrorHandling.Test
{
    /// <summary>
    /// Class for testing the ExtractException object
    /// </summary>
    [TestFixture]
    [Category("ExtractException")]
    public class LegacyTestExtractException
    {
        /// <summary>
        /// Test to verify that accessing the StackTrace property on an ExtractException
        /// returns an empty string
        /// </summary>
        [Test]
        public static void StackTraceIsEmpty()
        {
            // Create a new ExtractException
            ExtractException ee = new ExtractException("ELI00000", "Fake test exception!");

            // Get the stack trace from the ExtractException
            string stackTrace = ee.StackTrace;

            // Check that the stack trace is empty
            Assert.That(string.IsNullOrEmpty(stackTrace));
        }

        /// <summary>
        /// Test to verify that accessing the StackTrace property of an inner exception
        /// on an ExtractException returns an empty string
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
                // Catch the exception and add it to a new ExtractException
                // as an inner exception
                ExtractException uex = new ExtractException("ELI00000", "Fake testing exception!",
                    ex);

                // Get the stack trace from the inner exception
                innerStackTrace = uex.InnerException.StackTrace;
            }

            // Check that the stack trace is empty
            Assert.That(string.IsNullOrEmpty(innerStackTrace));
        }

        /// <summary>
        /// Test to verify that the inner exception of an ExtractException is an ExtractException.
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
                // Catch the exception and add it to a new ExtractException
                // as an inner exception
                ExtractException uex = new ExtractException("ELI00000", "Fake testing exception!",
                    ex);

                // Check that the type of the inner exception is now ExtractException
                innerExceptionIsExtract = uex.InnerException is ExtractException;
            }

            Assert.That(innerExceptionIsExtract);
        }

        /// <summary>
        /// Test to verify that the inner exception of an ExtractException can be created from 
        /// stringized exception.
        /// </summary>
        // StringizedByteStream is from the COM side and so does not conform to .Net conventions.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", 
            MessageId="Stringized")]
        // This class is testing the exception class and so uses fake ELI codes
        // while testing different scenarios
        [SuppressMessage("ExtractRules", "ES0002:MethodsShouldContainValidEliCodes")]
        [Test]
        public static void InnerExceptionFromStringizedException()
        {
            bool innerExceptionIsCorrect = false;
            
            ExtractException cex = new ("ELI00000", "Fake inner exception!");

            // Create ExtractException with inner exception specified as a string.
            ExtractException ee = new ExtractException("ELITEST", "Fake testing exception!",
                cex);

            // Test that the inner exception has the correct eli code and message.
            innerExceptionIsCorrect = ee.InnerException.Message == "Fake inner exception!" &&
                ((ExtractException)ee.InnerException).EliCode == "ELI00000";

            Assert.That(innerExceptionIsCorrect);
        }

        /// <summary>
        /// Test to verify that 
        /// <see cref="ExtractException.AddDebugData(string, string, bool)"/> where value is a 
        /// <see cref="System.String"/> and where encrypt is 
        /// <see langword="true"/> will encrypt the associated value before adding it to the data 
        /// collection.
        /// </summary>
        [Test]
        public static void EncryptedStringAddDebugData()
        {
            // Create a test string and test exception
            string testValue = "This is sample test data!";
            ExtractException ee = new ExtractException("ELI00000", "Fake testing exception!");

            // Add the debug data setting the encrypt flag to true
            ee.AddDebugData("Test Data", testValue, true);

            // Get the encrypted data value back from the collection
            string value = ee.Data["Test Data"].ToString();

            // Write out the values to the console to help verification in the Testing UI
            Console.WriteLine("Test value: " + testValue);
            Console.WriteLine(value);

            // Assert that the post encryption string is different
            // NOTE: This does not check that the value was necessarily encrypted and recoverable
            // it simply checks that the data is not the same.
            Assert.That(!value.Equals(testValue));
        }

        /// <summary>
        /// Test to verify that 
        /// <see cref="ExtractException.AddDebugData(string, System.ValueType, bool)"/>
        /// where value is a <see cref="System.Int64"/> and where encrypt
        /// is <see langword="true"/> will encrypt the associated value before adding it to
        /// the data collection.
        /// </summary>
        [Test]
        public static void EncryptedNumberAddDebugData()
        {
            // Create a test value and test exception
            long testValue = 42;
            ExtractException ee = new ExtractException("ELI00000", "Fake testing exception!");

            // Add the debug data and set the encrypt flag to true
            ee.AddDebugData("Test Data", testValue, true);

            // Get the encrypted data value back from the collection
            string value = ee.Data["Test Data"].ToString();

            // Write out the values to the console to help verification in the Testing UI
            Console.WriteLine("Test value: " + testValue);
            Console.WriteLine(value);

            // Assert that the post encryption value is different
            // NOTE: This does not check that the value was necessarily encrypted and recoverable
            // it simply checks that the data is not the same.
            Assert.That(!value.Equals(
                testValue.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Test to verify that <see cref="ExtractException.AddDebugData(string, string, bool)"/>
        /// where value is a <see cref="System.String"/> and where encrypt
        /// is <see langword="false"/> will not encrypt the associated value before adding it to
        /// the data collection.
        /// </summary>
        [Test]
        public static void UnencryptedStringAddDebugData()
        {
            // Create a test string and test exception
            string testValue = "This is sample test data!";
            ExtractException ee = new ExtractException("ELI00000", "Fake testing exception!");

            // Add the debug data setting the encrypt flag to false
            ee.AddDebugData("Test Data", testValue, false);

            // Get the data value back from the collection
            string value = ee.Data["Test Data"].ToString();

            // Write out the values to the console to help verification in the Testing UI
            Console.WriteLine("Test value: " + testValue);
            Console.WriteLine(value);

            // Assert that the post adding string is the same
            Assert.That(value.Equals(testValue));
        }

        /// <summary>
        /// Test to verify that 
        /// <see cref="ExtractException.AddDebugData(string, System.ValueType, bool)"/>
        /// where value is a <see cref="System.Int64"/> and where encrypt
        /// is <see langword="false"/> will not encrypt the associated value before adding it to
        /// the data collection.
        /// </summary>
        [Test]
        public static void UnencryptedNumberAddDebugData()
        {
            // Create a test value and test exception
            long testValue = 42;
            ExtractException ee = new ExtractException("ELI00000", "Fake testing exception!");

            // Add the debug data and set the encrypt flag to false
            ee.AddDebugData("Test Data", testValue, false);

            // Get the data value back from the collection
            string value = ee.Data["Test Data"].ToString();

            // Write out the values to the console to help verification in the Testing UI
            Console.WriteLine("Test value: " + testValue);
            Console.WriteLine(value);

            // Assert that the post adding value is the same
            Assert.That(value.Equals(
                testValue.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Test to verify that calling the ExtractException.Assert with no debug data 
        /// with a false condition will throw an ExtractException
        /// which has "Test Assert!" as part of its message.
        /// </summary>
        [Test]
        public static void ExtractAssert()
        {
            var ex = Assert.Throws<ExtractException>(() =>
            {
                ExtractException.Assert("ELI00000", "Test Assert!", false,
                ("FakeDebugDataName", "FakeDebugDataValue"));
            });
            StringAssert.Contains("Test Assert!", ex.Message);
        }

        /// <summary>
        /// Test to verify that calling the single pair of debug arguments version of
        /// ExtractException.Assert with a false condition will throw an ExtractException
        /// which has "Test Assert!" as part of its message.
        /// </summary>
        [Test]
        public static void ExtractAssertOnePair()
        {
            var ex = Assert.Throws<ExtractException>(() =>
            {
                ExtractException.Assert("ELI00000", "Test Assert!", false,
                ("FakeDebugDataName", "FakeDebugDataValue"));
            });
            StringAssert.Contains("Test Assert!", ex.Message);
        }

        /// <summary>
        /// Test to verify that calling the two pairs of debug arguments version of
        /// ExtractException.Assert with a false condition will throw an ExtractException
        /// which has "Test Assert!" as part of its message.
        /// </summary>
        [Test]
        public static void ExtractAssertTwoPairs()
        {
            var ex = Assert.Throws<ExtractException>(() =>
            {
                ExtractException.Assert("ELI00000", "Test Assert!", false,
                                        ("FakeDebugDataName1", "FakeDebugDataValue1"),
                                        ("FakeDebugDataName2", "FakeDebugDataValue2"));
            });
            StringAssert.Contains("Test Assert!", ex.Message);
        }

        /// <summary>
        /// Test to verify that calling the three pairs of debug arguments version of
        /// ExtractException.Assert with a false condition will throw an ExtractException
        /// which has "Test Assert!" as part of its message.
        /// </summary>
        [Test]
        public static void ExtractAssertThreePairs()
        {
            var ex = Assert.Throws<ExtractException>(() =>
            {
                ExtractException.Assert("ELI00000", "Test Assert!", false,
                ("FakeDebugDataName1", "FakeDebugDataValue1"),
                ("FakeDebugDataName2", "FakeDebugDataValue2"), 
                ("FakeDebugDataName3", "FakeDebugDataValue3"));
            });
            StringAssert.Contains("Test Assert!", ex.Message);
        }

        /// <summary>
        /// Test to verify that calling the four pairs of debug arguments version of
        /// ExtractException.Assert with a false condition will throw an ExtractException
        /// which has "Test Assert!" as part of its message.
        /// </summary>
        [Test]
        public static void ExtractAssertFourPairs()
        {
            var ex = Assert.Throws<ExtractException>(() =>
            {
                ExtractException.Assert("ELI00000", "Test Assert!", false,
                ("FakeDebugDataName1", "FakeDebugDataValue1"),
                ("FakeDebugDataName2", "FakeDebugDataValue2"), 
                ("FakeDebugDataName3", "FakeDebugDataValue3"), 
                ("FakeDebugDataName4", "FakeDebugDataValue4"));
            });
            StringAssert.Contains("Test Assert!", ex.Message);
        }

        /// <summary>
        /// Tests if the exception can be serialized and deserialized successfully
        /// </summary>
        [Test]
        public static void SerializeExtractException()
        {
            string tempFile = "";
            FileStream streamOut = null;
            FileStream streamIn = null;

            try
            {
                // Create the extract exception
                ExtractException ee = new ExtractException("ELI00000", "Fake testing exception!");

                // Create a temporary file and open it for writing
                tempFile = Path.GetTempFileName();
                streamOut = new FileStream(tempFile, FileMode.Create);

                // Create a new binary format serializer
                BinaryFormatter formatter = new BinaryFormatter();

                // Serialize the exception to the stream
                formatter.Serialize(streamOut, ee);
                streamOut.Flush();

                // Close the stream
                streamOut.Close();
                streamOut = null;

                // Open the temporary file for reading
                streamIn = new FileStream(tempFile, FileMode.Open);
                formatter = new BinaryFormatter();

                // Now read the exception from the stream
                ExtractException streamedEx =
                    (ExtractException)formatter.Deserialize(streamIn);

                // Close the input stream
                streamIn.Close();
                streamIn = null;

                // Delete the temporary file
                File.Delete(tempFile);

                // Assert that the exception and the streamed exception are the same
                Assert.That(ee.ToString() == streamedEx.ToString());
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
