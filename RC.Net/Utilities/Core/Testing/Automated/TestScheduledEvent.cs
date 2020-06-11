using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.Utilities.Test
{
    /// <summary>
    /// Unit tests for ScheduledEvent class
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("Automatic")]
    class TestScheduledEvent
    {
        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]

        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        #endregion Overhead

        #region Tests

        [Test]
        public static void TestNonRecurring()
        {
            var schedule = new ScheduledEvent();
            schedule.Start = new DateTime(2000, 1, 1);

            Assert.AreEqual(null, schedule.GetNextOccurrence(new DateTime(2000, 1, 2)));
            Assert.AreEqual(schedule.Start, schedule.GetNextOccurrence(new DateTime(1999, 12, 31)));
            Assert.AreEqual(schedule.Start, schedule.GetLastOccurrence(new DateTime(2000, 1, 2)));
            Assert.AreEqual(null, schedule.GetLastOccurrence(new DateTime(1999, 12, 31)));

            Assert.AreEqual(false, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 1)));
            schedule.Duration = new TimeSpan(1, 0, 0, 0);
            Assert.AreEqual(true, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 1)));
            Assert.AreEqual(false, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 2)));
            Assert.AreEqual(false, schedule.GetIsInScheduledEvent(new DateTime(1999, 12, 31)));
        }

        /// <summary>
        /// Database service with no future scheduled occurrence cause errors on service start
        /// https://extract.atlassian.net/browse/ISSUE-15382
        /// </summary>
        [Test]
        public static void TestNonRecurringOnStart()
        {
            var schedule = new ScheduledEvent();
            schedule.Start = new DateTime(2000, 1, 1);
            schedule.EventStarted += delegate { };
        }

        [Test]
        public static void TestRecurringSecondly()
        {
            var schedule = new ScheduledEvent();
            schedule.Start = new DateTime(2000, 1, 1);
            schedule.RecurrenceUnit = DateTimeUnit.Second;
            schedule.End = new DateTime(2000, 1, 1, 0, 0, 2);

            Assert.AreEqual(schedule.Start, schedule.GetNextOccurrence(new DateTime(1999, 12, 31, 23, 0, 0)));
            Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 1), schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 0, 0, 1)));
            Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 2), schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 0, 0, 1, 30)));
            Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 2), schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 0, 0, 2)));
            Assert.AreEqual(null, schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 0, 0, 2, 30)));

            schedule.End = null;
            Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 3), schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 0, 0, 2, 30)));
        }

        [Test]
        public static void TestRecurringMinutely()
        {
            var schedule = new ScheduledEvent();
            schedule.Start = new DateTime(2000, 1, 1);
            schedule.RecurrenceUnit = DateTimeUnit.Minute;
            schedule.End = new DateTime(2000, 1, 1, 0, 2, 0);

            Assert.AreEqual(schedule.Start, schedule.GetNextOccurrence(new DateTime(1999, 12, 31, 23, 0, 0)));
            Assert.AreEqual(new DateTime(2000, 1, 1, 0, 1, 0), schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 0, 1, 0)));
            Assert.AreEqual(new DateTime(2000, 1, 1, 0, 2, 0), schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 0, 1, 30)));
            Assert.AreEqual(new DateTime(2000, 1, 1, 0, 2, 0), schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 0, 2, 0)));
            Assert.AreEqual(null, schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 0, 2, 30)));

            schedule.End = null;
            Assert.AreEqual(new DateTime(2000, 1, 1, 0, 3, 0), schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 0, 2, 30)));
        }

        [Test]
        public static void TestRecurringHourly()
        {
            var schedule = new ScheduledEvent();
            schedule.Start = new DateTime(2000, 1, 1);
            schedule.RecurrenceUnit = DateTimeUnit.Hour;
            schedule.End = new DateTime(2000, 1, 1, 2, 0, 0);

            Assert.AreEqual(schedule.Start, schedule.GetNextOccurrence(new DateTime(1999, 12, 31, 23, 0, 0)));
            Assert.AreEqual(new DateTime(2000, 1, 1, 1, 0, 0), schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 1, 0, 0)));
            Assert.AreEqual(new DateTime(2000, 1, 1, 2, 0, 0), schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 1, 30, 0)));
            Assert.AreEqual(new DateTime(2000, 1, 1, 2, 0, 0), schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 2, 0, 0)));
            Assert.AreEqual(null, schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 2, 30, 0)));

            schedule.End = null;
            Assert.AreEqual(new DateTime(2000, 1, 1, 3, 0, 0), schedule.GetNextOccurrence(new DateTime(2000, 1, 1, 2, 30, 0)));
        }

        [Test]
        public static void TestRecurringDaily()
        {
            var schedule = new ScheduledEvent();
            schedule.Start = new DateTime(2000, 1, 1);
            schedule.RecurrenceUnit = DateTimeUnit.Day;
            schedule.End = new DateTime(2000, 1, 3);

            Assert.AreEqual(schedule.Start, schedule.GetNextOccurrence(new DateTime(1999, 12, 31)));
            Assert.AreEqual(new DateTime(2000, 1, 2), schedule.GetNextOccurrence(new DateTime(2000, 1, 2)));
            Assert.AreEqual(new DateTime(2000, 1, 3), schedule.GetNextOccurrence(new DateTime(2000, 1, 2, 1, 0, 0)));
            Assert.AreEqual(new DateTime(2000, 1, 3), schedule.GetNextOccurrence(new DateTime(2000, 1, 3)));
            Assert.AreEqual(null, schedule.GetNextOccurrence(new DateTime(2000, 1, 3, 1, 0, 0)));

            schedule.End = null;
            Assert.AreEqual(new DateTime(2000, 1, 4), schedule.GetNextOccurrence(new DateTime(2000, 1, 3, 1, 0, 0)));
        }

        [Test]
        public static void TestRecurringWeekly()
        {
            var schedule = new ScheduledEvent();
            schedule.Start = new DateTime(2000, 1, 1);
            schedule.RecurrenceUnit = DateTimeUnit.Week;
            schedule.End = new DateTime(2000, 1, 22);

            Assert.AreEqual(schedule.Start, schedule.GetNextOccurrence(new DateTime(1999, 12, 31)));
            Assert.AreEqual(new DateTime(2000, 1, 8), schedule.GetNextOccurrence(new DateTime(2000, 1, 8)));
            Assert.AreEqual(new DateTime(2000, 1, 15), schedule.GetNextOccurrence(new DateTime(2000, 1, 9)));
            Assert.AreEqual(new DateTime(2000, 1, 22), schedule.GetNextOccurrence(new DateTime(2000, 1, 22)));
            Assert.AreEqual(null, schedule.GetNextOccurrence(new DateTime(2000, 1, 23)));

            schedule.End = null;
            Assert.AreEqual(new DateTime(2000, 1, 29), schedule.GetNextOccurrence(new DateTime(2000, 1, 23)));
        }

        [Test]
        public static void TestRecurringMonthly()
        {
            var schedule = new ScheduledEvent();
            schedule.Start = new DateTime(2000, 1, 1);
            schedule.RecurrenceUnit = DateTimeUnit.Month;
            schedule.End = new DateTime(2000, 3, 1);

            Assert.AreEqual(schedule.Start, schedule.GetNextOccurrence(new DateTime(1999, 12, 31)));
            Assert.AreEqual(new DateTime(2000, 2, 1), schedule.GetNextOccurrence(new DateTime(2000, 2, 1)));
            Assert.AreEqual(new DateTime(2000, 3, 1), schedule.GetNextOccurrence(new DateTime(2000, 2, 2)));
            Assert.AreEqual(new DateTime(2000, 3, 1), schedule.GetNextOccurrence(new DateTime(2000, 3, 1)));
            Assert.AreEqual(null, schedule.GetNextOccurrence(new DateTime(2000, 3, 2)));

            schedule.End = null;
            Assert.AreEqual(new DateTime(2000, 4, 1), schedule.GetNextOccurrence(new DateTime(2000, 3, 2)));
        }

        [Test]
        public static void TestRecurringDuration()
        {
            var schedule = new ScheduledEvent();
            schedule.Start = new DateTime(2000, 1, 1);
            schedule.RecurrenceUnit = DateTimeUnit.Day;
            schedule.End = new DateTime(2000, 1, 3);

            Assert.AreEqual(false, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 1)));
            schedule.Duration = new TimeSpan(0, 1, 0, 0);
            Assert.AreEqual(true, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 1)));
            Assert.AreEqual(true, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 1, 0, 30, 0)));
            Assert.AreEqual(true, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 2, 0, 30, 0)));
            Assert.AreEqual(false, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 2, 1, 0, 0)));
            Assert.AreEqual(true, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 3, 0, 30, 0)));
            Assert.AreEqual(false, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 4, 0, 30, 0)));
        }

        [Test]
        public static void TestRecurringTimeOffset()
        {
            var schedule = new ScheduledEvent();
            schedule.Start = new DateTime(2000, 1, 1, 12, 0, 0);
            schedule.RecurrenceUnit = DateTimeUnit.Day;
            schedule.Duration = new TimeSpan(0, 1, 0, 0);
            schedule.End = new DateTime(2000, 1, 4);

            Assert.AreEqual(new DateTime(2000, 1, 1, 12, 0, 0), schedule.GetNextOccurrence(new DateTime(2000, 1, 1)));
            Assert.AreEqual(new DateTime(2000, 1, 3, 12, 0, 0), schedule.GetNextOccurrence(new DateTime(2000, 1, 3)));
            Assert.AreEqual(null, schedule.GetNextOccurrence(new DateTime(2000, 1, 4)));
            Assert.AreEqual(false, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 1, 11, 0, 0)));
            Assert.AreEqual(true, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 1, 12, 0, 0)));
            Assert.AreEqual(false, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 1, 13, 0, 0)));
            Assert.AreEqual(false, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 3, 11, 0, 0)));
            Assert.AreEqual(true, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 3, 12, 0, 0)));
            Assert.AreEqual(true, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 3, 12, 59, 0)));
            Assert.AreEqual(false, schedule.GetIsInScheduledEvent(new DateTime(2000, 1, 4, 12, 59, 0)));
        }

        [Test]
        public static void TestRecurringExclusions()
        {
            var schedule = new ScheduledEvent();
            schedule.Start = new DateTime(2000, 1, 1, 0, 0, 0); // Saturday
            schedule.RecurrenceUnit = DateTimeUnit.Day;
            schedule.Duration = new TimeSpan(1, 0, 0, 0);

            var weekendExclusion = new ScheduledEvent();
            weekendExclusion.Start = new DateTime(2000, 1, 1, 0, 0, 0); // Saturday
            weekendExclusion.RecurrenceUnit = DateTimeUnit.Week;

            // Exclusions must have a duration.
            Assert.Throws<ExtractException>(() => schedule.Exclusions = new[] { weekendExclusion });

            weekendExclusion.Duration = new TimeSpan(2, 0, 0, 0);
            schedule.Exclusions = new[] { weekendExclusion };

            Assert.AreEqual(new DateTime(2000, 1, 3), schedule.GetNextOccurrence(schedule.Start));
            Assert.AreEqual(null, schedule.GetLastOccurrence(schedule.Start.AddTicks(-1)));
            Assert.AreEqual(new DateTime(2000, 1, 4), schedule.GetNextOccurrence(new DateTime(2000, 1, 3, 1, 0, 0)));
            Assert.AreEqual(new DateTime(2000, 1, 7), schedule.GetLastOccurrence(new DateTime(2000, 1, 9)));

            var testEnd = new DateTime(2000, 3, 1);
            for (var dateTime = new DateTime(2000, 1, 1, 0, 0, 0);
                 dateTime < testEnd;
                 dateTime = dateTime.AddDays(1))
            {
                Assert.AreNotEqual(
                    dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday,
                    schedule.GetIsInScheduledEvent(dateTime));
            }
        }

        #endregion Tests
    }
}
