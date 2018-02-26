using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Extract.Utilities.Test
{
    /// <summary>
    /// Test SystemMethods class
    /// </summary>
    [TestFixture]
    [Category("System Methods")]
    class TestSystemMethods
    {
        static string pathToExe = Path.Combine(FileSystemMethods.CommonComponentsPath, "TestAppForSystemMethods.exe");

        #region TestSetup

        /// <summary>
        /// Initializes the test fixture for testing these methods
        /// </summary>
        [TestFixtureSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Performs post test execution cleanup.
        /// </summary>
        [TestFixtureTearDown]
        public static void Cleanup()
        {
        }

        #endregion TestSetup

        [Test, Category("Automated")]
        public static void TestRunExecutableEnumerableNoCancelNoTimeout()
        {
            List<string> arglist = new List<string>() { "arg1", "arg2" };
            int returnValue = SystemMethods.RunExecutable(pathToExe, arglist);
            Assert.AreEqual(arglist.Count(), returnValue,
                string.Format("return code was {0} expected {1}", returnValue, 0));
        }

        [Test, Category("Automated")]
        public static void TestRunExecutableEnumerableCancelNoTimeout()
        {
            List<string> arglist = new List<string>() { "/SleepTime", "30000" };
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            Thread thread = new Thread(() =>
            {
                Thread.Sleep(5000);
                tokenSource.Cancel();
            });

            thread.Start();
            int returnValue = SystemMethods.RunExecutable(pathToExe, arglist, cancelToken: tokenSource.Token);

            Assert.AreEqual(SystemMethods.OperationCanceledExitCode, returnValue,
               string.Format("return code was {0} expected {1}", returnValue, SystemMethods.OperationCanceledExitCode));
        }

        [Test, Category("Automated")]
        public static void TestRunExecutableWithTimeout()
        {

            int returnValue = SystemMethods.RunExecutable(pathToExe, "/SleepTime 30000", 10000);
            Assert.AreEqual(SystemMethods.OperationTimeoutExitCode,
                returnValue,
                string.Format("return code was {0} expected {1}", returnValue, SystemMethods.OperationTimeoutExitCode));
        }

        [Test, Category("Automated")]
        public static void TestRunExecutableRegularExit()
        {
            int returnValue = SystemMethods.RunExecutable(pathToExe, string.Empty, int.MaxValue);
            Assert.AreEqual(0, returnValue,
                string.Format("return code was {0} expected {1}", returnValue, 0));
        }

        [Test, Category("Automated")]
        public static void TestRunExtractExecutableWithNamedTokenSource()
        {
            string nameForTokenSource = Guid.NewGuid().AsString();
            NamedTokenSource tokenSource = new NamedTokenSource(nameForTokenSource);
            Thread thread = new Thread(() =>
            {
                Thread.Sleep(5000);
                tokenSource.Cancel();
            });

            thread.Start();
            int returnValue = SystemMethods.RunExtractExecutable(pathToExe, "/SleepTime 30000", tokenSource.Token, true);

            Assert.AreEqual(SystemMethods.OperationCanceledExitCode, returnValue,
               string.Format("return code was {0} expected {1}", returnValue, SystemMethods.OperationCanceledExitCode));
        }

        [Test, Category("Automated")]
        public static void TestRunExtractExecutableWithEnumerableArgsAndNamedTokenSource()
        {
            string nameForTokenSource = Guid.NewGuid().AsString();
            NamedTokenSource tokenSource = new NamedTokenSource(nameForTokenSource);
            Thread thread = new Thread(() =>
            {
                Thread.Sleep(5000);
                tokenSource.Cancel();
            });

            List<string> argList = new List<string>() { "/SleepTime", "30000" };
            thread.Start();
            int returnValue = SystemMethods.RunExtractExecutable(pathToExe, argList, tokenSource.Token, true);

            Assert.AreEqual(SystemMethods.OperationCanceledExitCode, returnValue,
               string.Format("return code was {0} expected {1}", returnValue, SystemMethods.OperationCanceledExitCode));
        }
    }
}
