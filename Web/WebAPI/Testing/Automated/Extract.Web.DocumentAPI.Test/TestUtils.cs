using DynamicData.Kernel;
using Extract.Testing.Utilities;
using Extract.Web.ApiConfiguration.Models;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebAPI;

namespace Extract.Web.WebAPI.Test
{
    /// <summary>
    /// Tests the Sequencer class.
    /// </summary>
    [TestFixture, Category("Automated")]
    public class TestUtils
    {
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

        #region Tests

        /// <summary>
        /// Tests that <see cref="Test_ApiContext.CreateCopy"/> sets property values appropriately.
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_ApiContext()
        {
            Utils.SetCurrentApiContext(
                "42.2"
                , "TestServerName"
                , "TestDatabaseName"
                , new DocumentApiConfiguration(
                    "NA",
                    isDefault: true,
                    workflowName: "TestWorkflowName",
                    attributeSet: "",
                    processingAction: "",
                    postProcessingAction: "",
                    documentFolder: "",
                    startAction: "",
                    endAction: "",
                    postWorkflowAction: "",
                    outputFileNameMetadataField: "",
                    outputFileNameMetadataInitialValueFunction: "")
                , dbNumberOfConnectionRetries: "42"
                , dbConnectionRetryTimeout: "43"
                , maxInterfaces: "44"
                , requestWaitTimeout: "45"
                , exceptionLogFilter: "401, 404");

            Assert.AreEqual(new ApiVersion(42, 2), Utils.CurrentApiContext.ApiVersion);
            Assert.AreEqual("TestServerName", Utils.CurrentApiContext.DatabaseServerName);
            Assert.AreEqual("TestDatabaseName", Utils.CurrentApiContext.DatabaseName);
            Assert.AreEqual("TestWorkflowName", Utils.CurrentApiContext.WebConfiguration.ValueOrDefault()?.WorkflowName);
            Assert.AreEqual(42, Utils.CurrentApiContext.NumberOfConnectionRetries);
            Assert.AreEqual(43, Utils.CurrentApiContext.ConnectionRetryTimeout);
            Assert.AreEqual(44, Utils.CurrentApiContext.MaxInterfaces);
            Assert.AreEqual(45, Utils.CurrentApiContext.RequestWaitTimeout);
            Assert.True(
                new int[] { 401, 404 }
                .SequenceEqual(
                    Utils.CurrentApiContext.ExceptionLogFilter
                        .SelectMany(num => num.ToEnumerable())
                        .OrderBy(num => num)));

            var cloneContext = Utils.CurrentApiContext.CreateCopy();
            Assert.AreEqual(new ApiVersion(42, 2), cloneContext.ApiVersion);
            Assert.AreEqual("TestServerName", cloneContext.DatabaseServerName);
            Assert.AreEqual("TestDatabaseName", cloneContext.DatabaseName);
            Assert.AreEqual("TestWorkflowName", cloneContext.WebConfiguration.ValueOrDefault()?.WorkflowName);
            Assert.AreEqual(42, cloneContext.NumberOfConnectionRetries);
            Assert.AreEqual(43, cloneContext.ConnectionRetryTimeout);
            Assert.AreEqual(44, cloneContext.MaxInterfaces);
            Assert.AreEqual(45, cloneContext.RequestWaitTimeout);
            Assert.True(
                new int[] { 401, 404 }
                .SequenceEqual(
                    cloneContext.ExceptionLogFilter
                        .SelectMany(num => num.ToEnumerable())
                        .OrderBy(num => num)));
            cloneContext.SessionId = "46";
            cloneContext.FAMSessionId = 46;

            string newWorkflowName = "NewWorkflow";
            Utils.CurrentApiContext.WebConfiguration = new DocumentApiConfiguration(
                configurationName: "NA",
                isDefault: true,
                workflowName: "NewWorkflow",
                attributeSet: "",
                processingAction: "",
                postProcessingAction: "",
                documentFolder: "",
                startAction: "",
                endAction: "",
                postWorkflowAction: "",
                outputFileNameMetadataField: "",
                outputFileNameMetadataInitialValueFunction: "");
            var cloneContext2 = Utils.CurrentApiContext.CreateCopy();
            Assert.AreEqual(cloneContext.ApiVersion, cloneContext2.ApiVersion);
            Assert.AreEqual(cloneContext.DatabaseServerName, cloneContext2.DatabaseServerName);
            Assert.AreEqual(cloneContext.DatabaseName, cloneContext2.DatabaseName);
            Assert.AreEqual(newWorkflowName, cloneContext2.WebConfiguration.ValueOrDefault()?.WorkflowName);
            Assert.AreEqual(cloneContext.NumberOfConnectionRetries, cloneContext2.NumberOfConnectionRetries);
            Assert.AreEqual(cloneContext.ConnectionRetryTimeout, cloneContext2.ConnectionRetryTimeout);
            Assert.AreEqual(cloneContext.MaxInterfaces, cloneContext2.MaxInterfaces);
            Assert.AreEqual(cloneContext.RequestWaitTimeout, cloneContext2.RequestWaitTimeout);
            Assert.True(
                new int[] { 401, 404 }
                .SequenceEqual(
                    cloneContext2.ExceptionLogFilter
                        .SelectMany(num => num.ToEnumerable())
                        .OrderBy(num => num)));
            Assert.AreNotEqual(cloneContext.SessionId, cloneContext2.SessionId);
            Assert.AreNotEqual(cloneContext.FAMSessionId, cloneContext2.FAMSessionId);
        }

        /// <summary>
        /// General test of the Sequencer class.
        /// <para><b>NOTE</b></para>
        /// This test requires at least 4 cores to run correctly.
        /// </summary>
        [Test]
        public static void Test_Sequencer()
        {
            // 500ms spacing after each item is allowed its turn.
            var sequencer = new Sequencer<int>(500, false);

            // Queue 4 integers out of order
            sequencer.AddToQueue(2);
            sequencer.AddToQueue(1);
            sequencer.AddToQueue(4);
            sequencer.AddToQueue(3);  
            
            var sequencedOutput = new ConcurrentQueue<int>();
            var outputIntervals = new List<long>();

            using (var threadsFinishedEvent = new CountdownEvent(4))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // Kick off 5 threads to wait for the turn for integers 0-4. Since 0 isn't
                // queued, it should immediately return false. All other threads will sleep 1
                // second * the integer that is waiting to ensure they are calling WaitForTurn in
                // the same order they were queued.
                for (int i = 0; i < 5; i++)
                {
                    int item = i;

                    Task.Factory.StartNew(() =>
                        {
                            // Sleep is limited by the resolution of the system clock and therefore
                            // may sleep for slightly less than the requested time. It looks like
                            // 15ms is generally the resolution that can be expected; 20 extra ms
                            // of sleep should guarantee the full time is elapsed.
                            Thread.Sleep(1000 * item + 20);
                            if (sequencer.WaitForTurn(item))
                            {
                                sequencedOutput.Enqueue(item);
                                outputIntervals.Add(stopwatch.ElapsedMilliseconds);
                                threadsFinishedEvent.Signal();
                            }
                        });
                }

                Assert.That(threadsFinishedEvent.Wait(10000),
                    "Failed to start all threads; 4 cores are required for this test to run.");
            }

            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(new[] { 2, 1, 4, 3 }, sequencedOutput);

                // Expected output intervals are a combination of how long each thread was sleeping and
                // whether the 500ms interval between items being let through.
                string message = "Failure is a timing issue that may be impacted by other active processes. " +
                    "Try again after ensuring no other processes are using CPU and that there are at least " +
                    "4 physical cores available on this machine.";

                Assert.That(outputIntervals[0], Is.GreaterThanOrEqualTo(2000), message);
                Assert.That(outputIntervals[0], Is.LessThan(2200), message);

                Assert.That(outputIntervals[1], Is.GreaterThanOrEqualTo(2500), message);
                Assert.That(outputIntervals[1], Is.LessThan(2900), message);

                Assert.That(outputIntervals[2], Is.GreaterThanOrEqualTo(4000), message);
                Assert.That(outputIntervals[2], Is.LessThan(4700), message);

                Assert.That(outputIntervals[3], Is.GreaterThanOrEqualTo(4500), message);
                Assert.That(outputIntervals[3], Is.LessThan(5300), message);
            });
        }

        #endregion Tests
    }
}
