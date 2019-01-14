using Extract.Testing.Utilities;
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
    [TestFixture, Category("Sequencer")]
    public class TestSequencer
    {
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
        
        #region Tests

        /// <summary>
        /// General test of the Sequencer class.
        /// <para><b>NOTE</b></para>
        /// This test requires at least 4 cores to run correctly.
        /// </summary>
        [Test]
        public static void Test01()
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

            Assert.That(sequencedOutput.SequenceEqual(new[] { 2, 1, 4, 3 }));
            // Expected output intervals are a combination of how long each thread was sleeping and
            // whether the 500ms interval between items being let through.
            string message = "Failure is a timing issue that may be impacted by other active processes. " +
                "Try again after ensuring no other processes are using CPU and that there are at least " +
                "4 physical cores available on this machine.";
            Assert.That(outputIntervals[0] >= 2000 && outputIntervals[0] < 2100, message);
            Assert.That(outputIntervals[1] >= 2500 && outputIntervals[1] < 2600, message);
            Assert.That(outputIntervals[2] >= 4000 && outputIntervals[2] < 4100, message);
            Assert.That(outputIntervals[3] >= 4500 && outputIntervals[3] < 4600, message);
        }

        #endregion Tests
    }
}
