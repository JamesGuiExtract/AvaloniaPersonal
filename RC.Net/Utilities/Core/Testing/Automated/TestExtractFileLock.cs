using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Extract.Testing.Utilities;
using System.IO;

namespace Extract.Utilities.Test
{
    /// <summary>
    /// Class for testing the <see cref="ExtractFileLock"/> class.
    /// </summary>
    [TestFixture]
    [Category("ExtractSettings")]
    public class TestExtractFileLock
    {
        #region Overhead Methods

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        #endregion Overhead Methods

        #region Tests

        /// <summary>
        /// Tests that a second lock can't be taken if a file is already locked.
        /// </summary>
        [Test]
        public static void TestFileLock1()
        {
            Exception exception = null;

            using (var fileLock1 = new ExtractFileLock())
            using (var fileLock2 = new ExtractFileLock())
            {
                using (var tempFile1 = new TemporaryFile(".txt", false))
                {
                    fileLock1.GetLock(tempFile1.FileName, "TestContext");

                    try
                    {
                        fileLock2.GetLock(tempFile1.FileName);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                }
            }

            Assert.IsNotNull(exception);
            Assert.Contains("ELI39245", exception.Data.Values.OfType<string>().ToList());
            Assert.AreEqual(exception.Data["Context"].AsString(), "TestContext");
        }

        /// <summary>
        /// Tests that a file lock can be switched.
        /// </summary>
        [Test]
        public static void TestFileLock2()
        {
            using (var fileLock1 = new ExtractFileLock())
            using (var fileLock2 = new ExtractFileLock())
            {
                using (var tempFile1 = new TemporaryFile(".txt", false))
                using (var tempFile2 = new TemporaryFile(".txt", false))
                {
                    fileLock1.GetLock(tempFile1.FileName);
                    // Switch target file
                    fileLock1.GetLock(tempFile2.FileName);

                    fileLock2.GetLock(tempFile1.FileName);

                    // Test repeated calls to lock a file don't case error.
                    fileLock1.GetLock(tempFile2.FileName);

                    fileLock1.ReleaseLock();
                    fileLock2.GetLock(tempFile2.FileName);

                    Assert.That(!File.Exists(tempFile1.FileName + ".ExtractLock"));
                    Assert.That(File.Exists(tempFile2.FileName + ".ExtractLock"));
                }
            }
        }

        /// <summary>
        /// Tests that a file lock can be taken before file exists.
        /// </summary>
        [Test]
        public static void TestFileLock3()
        {
            Exception exception = null;

            using (var fileLock1 = new ExtractFileLock(@"C:\Non-existent"))
            using (var fileLock2 = new ExtractFileLock())
            {
                try
                {
                    fileLock2.GetLock(@"C:\Non-existent");
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            Assert.IsNotNull(exception);
            Assert.Contains("ELI39245", exception.Data.Values.OfType<string>().ToList());
        }

        #endregion Tests
    }
}
