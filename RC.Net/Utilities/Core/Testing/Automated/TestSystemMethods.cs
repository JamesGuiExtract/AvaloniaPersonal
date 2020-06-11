using Extract.Testing.Utilities;
using Extract.Utilities.Forms;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        static string pathToUIExe = Path.Combine(FileSystemMethods.CommonComponentsPath, "LearningMachineEditor.exe");

        #region TestSetup

        /// <summary>
        /// Initializes the test fixture for testing these methods
        /// </summary>
        [OneTimeSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Performs post test execution cleanup.
        /// </summary>
        [OneTimeTearDown]
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

        [Test, Category("Automated")]
        public static void TestRunExecutableWithTimeoutAndNoWindow()
        {
            List<string> argList = new List<string>() { "/SleepTime", "30000" };
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int returnValue = SystemMethods.RunExecutable(pathToExe, argList, 5000, createNoWindow: true);
            long elapsed = sw.ElapsedMilliseconds;

            Assert.AreEqual(SystemMethods.OperationTimeoutExitCode, returnValue,
               string.Format("return code was {0} expected {1}", returnValue, SystemMethods.OperationCanceledExitCode));

            // Executable should not have been allowed to finish
            Assert.Less(elapsed, 30000);
        }

        [Test, Category("Automated")]
        public static void TestRunExecutableWithCancelAndNoWindow()
        {
            string nameForTokenSource = Guid.NewGuid().AsString();
            NamedTokenSource tokenSource = new NamedTokenSource(nameForTokenSource);
            Thread thread = new Thread(() =>
            {
                Thread.Sleep(5000);
                tokenSource.Cancel();
            });

            List<string> argList = new List<string>() { "/SleepTime", "30000" };
            Stopwatch sw = new Stopwatch();
            thread.Start();
            sw.Start();
            int returnValue = SystemMethods.RunExecutable(pathToExe, argList, int.MaxValue, createNoWindow: true, cancelToken: tokenSource.Token);
            long elapsed = sw.ElapsedMilliseconds;

            Assert.AreEqual(SystemMethods.OperationCanceledExitCode, returnValue,
               string.Format("return code was {0} expected {1}", returnValue, SystemMethods.OperationCanceledExitCode));

            // Executable should not have been allowed to finish
            Assert.Less(elapsed, 30000);
        }

        [Test, Category("Automated")]
        public static void TestRunExecutableWithCancelAndRedirectOutput()
        {
            string nameForTokenSource = Guid.NewGuid().AsString();
            NamedTokenSource tokenSource = new NamedTokenSource(nameForTokenSource);
            Thread thread = new Thread(() =>
            {
                Thread.Sleep(5000);
                tokenSource.Cancel();
            });

            List<string> argList = new List<string>() { "/SleepTime", "30000" };
            Stopwatch sw = new Stopwatch();
            thread.Start();
            sw.Start();
            int returnValue = SystemMethods.RunExecutable(pathToExe, argList, out string _, out string _, cancelToken: tokenSource.Token);
            long elapsed = sw.ElapsedMilliseconds;

            Assert.AreEqual(SystemMethods.OperationCanceledExitCode, returnValue,
               string.Format("return code was {0} expected {1}", returnValue, SystemMethods.OperationCanceledExitCode));

            // Executable should not have been allowed to finish
            Assert.Less(elapsed, 30000);
        }

        #region TestForDeadlock

        /// <summary>
        /// Helper class to test for deadlock
        /// Before fixing the issue by using ConfigureAwait(false) this class would consistently deadlock on load
        /// https://extract.atlassian.net/browse/ISSUE-16555
        /// </summary>
        private class LaunchFromUI : InvisibleForm
        {
            private readonly string _pathToExe;
            private readonly List<string> _argList;
            private CancellationToken _token;

            public LaunchFromUI(string pathToExe, List<string> argList, CancellationToken token) : base()
            {
                _pathToExe = pathToExe;
                _argList = argList;
                _token = token;
            }

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);
                SystemMethods.RunExecutable(_pathToExe, _argList, out string _, out string _, cancelToken: _token);
                Application.Exit();
            }
        }

        private static async Task<bool> RunAndCheckForTimeout(int timeout, CancellationToken token)
        {
            List<string> argList = new List<string>() { "/SleepTime", "30000" };

            LaunchFromUI form = null;
            var task = Task.Run(() =>
            {
                form = new LaunchFromUI(pathToExe, argList, token);
                Application.Run(form);
            });

            if (await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false) == task)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        [Test, Category("Automated")]
        public static void TestRunExecutableWithCancelAndRedirectOutputFromUIThread()
        {
            string nameForTokenSource = Guid.NewGuid().AsString();
            NamedTokenSource tokenSource = new NamedTokenSource(nameForTokenSource);
            Task.Run(() =>
            {
                Task.Delay(5000);
                tokenSource.Cancel();
            });

            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool timedOut = RunAndCheckForTimeout(10000, tokenSource.Token).GetAwaiter().GetResult();

            long elapsed = sw.ElapsedMilliseconds;

            Assert.IsFalse(timedOut, "Wait for task exceeded timeout. Deadlock likely occurred!");

            // Executable should not have been allowed to finish
            Assert.Less(elapsed, 30000);
        }

        #endregion TestForDeadlock
    }
}
