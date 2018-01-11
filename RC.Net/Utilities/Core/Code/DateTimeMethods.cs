using System;
using System.Globalization;

namespace Extract.Utilities
{
    /// <summary>
    /// Units of time used by the <see cref="DateTimeMethods"/>.
    /// </summary>
    public enum DateTimeUnit
    {
        /// <summary>
        /// A millisecond.
        /// </summary>
        Millisecond = 0,

        /// <summary>
        /// A second.
        /// </summary>
        Second = 1,

        /// <summary>
        /// A minute.
        /// </summary>
        Minute = 2,

        /// <summary>
        /// An hour.
        /// </summary>
        Hour = 3,

        /// <summary>
        /// A day.
        /// </summary>
        Day = 4,

        /// <summary>
        /// A week.
        /// </summary>
        Week = 5,

        /// <summary>
        /// A month.
        /// </summary>
        Month = 6,

        /// <summary>
        /// A year.
        /// </summary>
        Year = 7,

        /// <summary>
        /// A Quarter
        /// </summary>
        Quarter = 8
    }

    /// <summary>
    /// Colloquial time periods which can be used for creating time range based
    /// </summary>
    public enum TimePeriodRange
    {
        /// <summary>
        /// Any time from the start of the current minute up to the start of the next minute.
        /// </summary>
        ThisMinute = 0,

        /// <summary>
        /// Any time from the start of the last minute up to the start of this minute.
        /// </summary>
        LastMinute = 1,

        /// <summary>
        /// Any time from the start of this hour up to the start of next hour.
        /// </summary>
        ThisHour = 2,

        /// <summary>
        /// Any time from the start of previous hour up to the start of this hour.
        /// </summary>
        LastHour = 3,

        /// <summary>
        /// Any time from midnight today up to midnight tomorrow.
        /// </summary>
        Today = 4,

        /// <summary>
        /// Any time from midnight yesterday up to midnight today.
        /// </summary>
        Yesterday = 5,

        /// <summary>
        /// Any time this week where the start and end of the week is dictated by
        /// CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek.
        /// </summary>
        ThisWeek = 6,

        /// <summary>
        /// Any time last week where the start and end of the week is dictated by
        /// CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek.
        /// </summary>
        LastWeek = 7,

        /// <summary>
        /// Any time this month.
        /// </summary>
        ThisMonth = 8,

        /// <summary>
        /// Any time last month.
        /// </summary>
        LastMonth = 9,

        /// <summary>
        /// Any time this quarter
        /// </summary>
        ThisQuarter = 10,

        /// <summary>
        /// Any time last quarter
        /// </summary>
        LastQuarter = 11,

        /// <summary>
        /// Any time this year.
        /// </summary>
        ThisYear = 12,

        /// <summary>
        /// Any time last year.
        /// </summary>
        LastYear = 13
    }


    /// <summary>
    /// Methods for performing operations on <see cref="DateTime"/> instances.
    /// </summary>
    public static class DateTimeMethods
    {
        #region Extension Methods

        /// <summary>
        /// Rounds the specified <see paramref="dateTime"/> to the nearest <see paramref="timeSpan"/>
        /// boundary.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to round.</param>
        /// <param name="timeSpan">The <see cref="TimeSpan"/> to which the <see paramref="dateTime"/>
        /// should be rounded.</param>
        /// <param name="boundaryDateTime">The <see paramref="timeSpan"/> boundaries will be normalized
        /// against January 1, 0001 midnight unless a different <see cref="DateTime"/> is specified
        /// here.</param>
        /// <returns>The <see cref="DateTime"/> of the nearest boundary.</returns>
        public static DateTime Round(this DateTime dateTime, TimeSpan timeSpan,
            DateTime boundaryDateTime = new DateTime())
        {
            try
            {
                // Calculate an offset in ticks such that 0 will lie on a timespan boundary where the
                // timespans are aligned with boundaryDateTime.
                long offsetTicks = boundaryDateTime.Ticks % timeSpan.Ticks;
                ExtractException.Assert("ELI32773", "Invalid datetime rounding boundary.",
                    dateTime.Ticks >= offsetTicks);

                // Normalize dateTime.Ticks against the boundaryDateTime.
                long dateTimeTicks = dateTime.Ticks - offsetTicks;

                // Round to the nearest boundary.
                long ticks = ((dateTimeTicks + (timeSpan.Ticks / 2)) / timeSpan.Ticks) * timeSpan.Ticks;
                
                // Normalize the result back to the standard DateTime timescale.
                ticks += offsetTicks;

                return new DateTime(ticks);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32768");
            }
        }

        /// <summary>
        /// Rounds the specified <see paramref="dateTime"/> to the nearest
        /// <see paramref="dateTimeUnit"/> boundary.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to round.</param>
        /// <param name="dateTimeUnit">The <see cref="DateTimeUnit"/> to which the
        /// <see paramref="dateTime"/> should be rounded.</param>
        /// <returns>The <see cref="DateTime"/> of the nearest boundary.</returns>
        public static DateTime Round(this DateTime dateTime, DateTimeUnit dateTimeUnit)
        {
            try
            {
                switch (dateTimeUnit)
                {
                    case DateTimeUnit.Quarter:
                        {
                            int quarterNumber = (dateTime.Month - 1) / 3 + 1;
                            DateTime firstDayOfQuarter = new DateTime(dateTime.Year, (quarterNumber - 1) * 3 + 1, 1);
                            DateTime lastDayOfQuarter = firstDayOfQuarter.AddMonths(3);
                            long midPoint = (lastDayOfQuarter.Ticks + firstDayOfQuarter.Ticks) / 2;
                            return dateTime.Ticks < midPoint ? firstDayOfQuarter : lastDayOfQuarter;
                        }
                    case DateTimeUnit.Year:
                        {
                            // Special case; years are not a constant length of time do to leap years.
                            DateTime yearStart = new DateTime(dateTime.Year, 1, 1);
                            DateTime yearEnd = yearStart.AddYears(1);
                            long midPoint = (yearEnd.Ticks + yearStart.Ticks) / 2;

                            return (dateTime.Ticks < midPoint) ? yearStart : yearEnd;
                        }

                    case DateTimeUnit.Month:
                        {
                            // Special case; months are not a constant number of days.
                            DateTime monthStart = new DateTime(dateTime.Year, dateTime.Month, 1);
                            DateTime monthEnd = monthStart.AddMonths(1);
                            long midPoint = (monthEnd.Ticks + monthStart.Ticks) / 2;

                            return (dateTime.Ticks < midPoint) ? monthStart : monthEnd;
                        }

                    case DateTimeUnit.Week:
                        {
                            // To normalize the week start and end use a Sunday (7/3/2011) plus the
                            // current culture's FirstDayOfWeek.
                            // (In the DayOfWeek enum 0 = Sunday ... 6 = Saturday).
                            DateTime firstDayOfWeek = new DateTime(2011, 7, 3) + new TimeSpan(
                                (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 0, 0, 0);
                            return dateTime.Round(new TimeSpan(7, 0, 0, 0), firstDayOfWeek);
                        }

                    case DateTimeUnit.Day:
                        return dateTime.Round(new TimeSpan(1, 0, 0, 0));

                    case DateTimeUnit.Hour:
                        return dateTime.Round(new TimeSpan(0, 1, 0, 0));

                    case DateTimeUnit.Minute:
                        return dateTime.Round(new TimeSpan(0, 0, 1, 0));

                    case DateTimeUnit.Second:
                        return dateTime.Round(new TimeSpan(0, 0, 0, 1));

                    case DateTimeUnit.Millisecond:
                        return dateTime.Round(new TimeSpan(0, 0, 0, 0, 1));

                    default:
                        throw new ExtractException("ELI32769", "Unexpected date/time part.");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32778");
            }
        }

        /// <summary>
        /// Returns <see cref="DateTime"/> of the first <see paramref="timeSpan"/> boundary
        /// previous to or equal to the specified <see paramref="dateTime"/>.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to use.</param>
        /// <param name="timeSpan">The <see cref="TimeSpan"/> to use.</param>
        /// <param name="boundaryDateTime">The <see paramref="timeSpan"/> boundaries will be normalized
        /// against January 1, 0001 midnight unless a different <see cref="DateTime"/> is specified
        /// here.</param>
        /// <returns>The <see cref="DateTime"/> of the previous (or equal) boundary.</returns>
        public static DateTime Floor(this DateTime dateTime, TimeSpan timeSpan,
            DateTime boundaryDateTime = new DateTime())
        {
            try
            {
                // Calculate an offset in ticks such that 0 will lie on a timespan boundary where the
                // timespans are aligned with boundaryDateTime.
                long offsetTicks = boundaryDateTime.Ticks % timeSpan.Ticks;
                ExtractException.Assert("ELI32774", "Invalid datetime rounding boundary.",
                    dateTime.Ticks >= offsetTicks);

                // Normalize dateTime.Ticks against the boundaryDateTime.
                long dateTimeTicks = dateTime.Ticks - offsetTicks;

                // Find the previous boundary.
                long ticks = (dateTimeTicks / timeSpan.Ticks) * timeSpan.Ticks;

                // Normalize the result back to the standard DateTime timescale.
                ticks += offsetTicks;

                return new DateTime(ticks);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32770");
            }
        }

        /// <summary>
        /// Returns <see cref="DateTime"/> of the first <see paramref="dateTimeUnit"/> boundary
        /// previous to or equal to the specified <see paramref="dateTime"/>.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to use.</param>
        /// <param name="dateTimeUnit">The <see cref="DateTimeUnit"/> to use.</param>
        /// <returns>The <see cref="DateTime"/> of the previous (or equal) boundary.</returns>
        public static DateTime Floor(this DateTime dateTime, DateTimeUnit dateTimeUnit)
        {
            try
            {
                switch (dateTimeUnit)
                {
                    case DateTimeUnit.Quarter:
                        int quarterNumber = (dateTime.Month - 1) / 3 + 1;
                        return new DateTime(dateTime.Year, (quarterNumber - 1) * 3 + 1, 1);
                    case DateTimeUnit.Year:
                        // Special case; years are not a constant length of time do to leap years.
                        return new DateTime(dateTime.Year, 1, 1);

                    case DateTimeUnit.Month:
                        // Special case; months are not a constant number of days.
                        return new DateTime(dateTime.Year, dateTime.Month, 1);

                    case DateTimeUnit.Week:
                        {
                            // To normalize the week start and end use a Sunday (7/3/2011) plus the
                            // current culture's FirstDayOfWeek.
                            // (In the DayOfWeek enum 0 = Sunday ... 6 = Saturday).
                            DateTime firstDayOfWeek = new DateTime(2011, 7, 3) + new TimeSpan(
                                (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 0, 0, 0);
                            return dateTime.Floor(new TimeSpan(7, 0, 0, 0), firstDayOfWeek);
                        }

                    case DateTimeUnit.Day:
                        return dateTime.Floor(new TimeSpan(1, 0, 0, 0));

                    case DateTimeUnit.Hour:
                        return dateTime.Floor(new TimeSpan(0, 1, 0, 0));

                    case DateTimeUnit.Minute:
                        return dateTime.Floor(new TimeSpan(0, 0, 1, 0));

                    case DateTimeUnit.Second:
                        return dateTime.Floor(new TimeSpan(0, 0, 0, 1));

                    case DateTimeUnit.Millisecond:
                        return dateTime.Floor(new TimeSpan(0, 0, 0, 0, 1));

                    default:
                        throw new ExtractException("ELI32771", "Unexpected date/time part.");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32779");
            }
        }

        /// <summary>
        /// Returns <see cref="DateTime"/> of the first <see paramref="timeSpan"/> boundary
        /// after or equal to the specified <see paramref="dateTime"/>.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to use.</param>
        /// <param name="timeSpan">The <see cref="TimeSpan"/> to use.</param>
        /// <param name="boundaryDateTime">The <see paramref="timeSpan"/> boundaries will be normalized
        /// against January 1, 0001 midnight unless a different <see cref="DateTime"/> is specified
        /// here.</param>
        /// <returns>The <see cref="DateTime"/> of the next (or equal) boundary.</returns>
        public static DateTime Ceiling(this DateTime dateTime, TimeSpan timeSpan,
            DateTime boundaryDateTime = new DateTime())
        {
            try
            {
                // Calculate an offset in ticks such that 0 will lie on a timespan boundary where the
                // timespans are aligned with boundaryDateTime.
                long offsetTicks = boundaryDateTime.Ticks % timeSpan.Ticks;
                ExtractException.Assert("ELI32775", "Invalid datetime rounding boundary.",
                    dateTime.Ticks >= offsetTicks);

                // Normalize dateTime.Ticks against the boundaryDateTime.
                long dateTimeTicks = dateTime.Ticks - offsetTicks;

                // Find the next boundary.
                long ticks = ((dateTimeTicks + timeSpan.Ticks - 1) / timeSpan.Ticks) * timeSpan.Ticks;

                // Normalize the result back to the standard DateTime timescale.
                ticks += offsetTicks;

                return new DateTime(ticks);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32772");
            }
        }

        /// <summary>
        /// Returns <see cref="DateTime"/> of the first <see paramref="dateTimeUnit"/> boundary
        /// after or equal to the specified <see paramref="dateTime"/>.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to use.</param>
        /// <param name="dateTimeUnit">The <see cref="DateTimeUnit"/> to use.</param>
        /// <returns>The <see cref="DateTime"/> of the next (or equal) boundary.</returns>
        public static DateTime Ceiling(this DateTime dateTime, DateTimeUnit dateTimeUnit)
        {
            try
            {
                switch (dateTimeUnit)
                {
                    case DateTimeUnit.Quarter:
                        {
                            int quarterNumber = (dateTime.Month - 1) / 3 + 1;
                            DateTime firstDayOfQuarter = new DateTime(dateTime.Year, (quarterNumber - 1) * 3 + 1, 1);
                            // If the dateTime is already on a year boundary, it does not need to be modified.
                            return (dateTime == firstDayOfQuarter)
                                ? dateTime
                                : firstDayOfQuarter.AddMonths(3);
                        }
                    case DateTimeUnit.Year:
                        {
                            // Special case; years are not a constant length of time do to leap years.
                            DateTime yearStart = new DateTime(dateTime.Year, 1, 1);
                            // If the dateTime is already on a year boundary, it does not need to be modified.
                            return (dateTime == yearStart)
                                ? dateTime
                                : yearStart.AddYears(1);
                        }

                    case DateTimeUnit.Month:
                        {
                            // Special case; months are not a constant number of days.
                            DateTime monthStart = new DateTime(dateTime.Year, dateTime.Month, 1);
                            // If the dateTime is already on a month boundary, it does not need to be modified.
                            return (dateTime == monthStart)
                                ? dateTime
                                : monthStart.AddMonths(1);
                        }

                    case DateTimeUnit.Week:
                        {
                            // To normalize the week start and end use a Sunday (7/3/2011) plus the
                            // current culture's FirstDayOfWeek.
                            // (In the DayOfWeek enum 0 = Sunday ... 6 = Saturday).
                            DateTime firstDayOfWeek = new DateTime(2011, 7, 3) + new TimeSpan(
                                (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 0, 0, 0);
                            return dateTime.Ceiling(new TimeSpan(7, 0, 0, 0), firstDayOfWeek);
                        }

                    case DateTimeUnit.Day:
                        return dateTime.Ceiling(new TimeSpan(1, 0, 0, 0));

                    case DateTimeUnit.Hour:
                        return dateTime.Ceiling(new TimeSpan(0, 1, 0, 0));

                    case DateTimeUnit.Minute:
                        return dateTime.Ceiling(new TimeSpan(0, 0, 1, 0));

                    case DateTimeUnit.Second:
                        return dateTime.Ceiling(new TimeSpan(0, 0, 0, 1));

                    case DateTimeUnit.Millisecond:
                        return dateTime.Ceiling(new TimeSpan(0, 0, 0, 0, 1));

                    default:
                        throw new ExtractException("ELI32767", "Unexpected date/time part.");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32780");
            }
        }

        #endregion Extension Methods

        #region Utility Methods

        /// <summary>
        /// Gets the Date range for the given timePeriod
        /// Converts as follows
        ///     ThisMinute  : Start Date/Time will be the start of the current minute and the end date will be the end of the current minute
        ///     LastMinute  : Start Date/Time will be the start of the previous minute and the end date will be the end of the previous minute
        ///     ThisHour    : Start Date/Time will be the start of the current hour and the end date will be the end of the current hour
        ///     LastHour    : Start Date/Time will be the start of the previous hour and the end date will be the end of the previous hour
        ///     Today       : Start Date/time will be Todays date  0:00  end date will be Today 23:59:59
        ///     YesterDay   : Start Date/Time will be Yesterday 0:00 end date will be yesterday 23:59:59
        ///     ThisWeek    : Start Date/Time will be Sunday of current week at 0:00 end date will be Saturday 23:59:59 of the current week
        ///     LastWeek    : Start Date/Time will be Sunday of previous week at 0:00 end date will be Saturday 23:59:59 of previous week
        ///     ThisMonth   : Start Date/Time will be the first day of current month at 0:00 end date will be last day of current month 23:59:59
        ///     LastMonth   : Start Date/Time will be the first day of previous month at 0:00 end date will be last day of previous month 23:59:59
        ///     ThisYear    : Start Date/Time will be Jan 1 of current year at 0:00 end date will be Dec 31 of current year at 23:59:59
        ///     LastYear    : Start Date/Time will be Jan 1 of previous year at 0:00 end date will be Dec 31 of previous year at 23:59:59
        /// </summary>
        /// <param name="timePeriod">The timePeriod to return a range of dates for</param>
        /// <returns>Tuple with two <see cref="DateTime"/> values, the start date as Item1 and end date as Item2</returns>
        public static Tuple<DateTime, DateTime> GetDateRangeForTimePeriodRange(TimePeriodRange timePeriod)
        {
            try
            {
                DateTime startDate;
                DateTime endDate;
                DateTime referenceDate = DateTime.Now;
                DateTimeUnit dateTimePart;

                switch (timePeriod)
                {
                    case TimePeriodRange.ThisMinute:
                        dateTimePart = DateTimeUnit.Minute;
                        break;
                    case TimePeriodRange.LastMinute:
                        referenceDate = referenceDate.AddMinutes(-1);
                        dateTimePart = DateTimeUnit.Minute;
                        break;
                    case TimePeriodRange.ThisHour:
                        dateTimePart = DateTimeUnit.Hour;
                        break;
                    case TimePeriodRange.LastHour:
                        referenceDate = referenceDate.AddHours(-1);
                        dateTimePart = DateTimeUnit.Hour;
                        break;
                    case TimePeriodRange.Today:
                        dateTimePart = DateTimeUnit.Day;
                        break;
                    case TimePeriodRange.Yesterday:
                        referenceDate = referenceDate.AddDays(-1);
                        dateTimePart = DateTimeUnit.Day;
                        break;
                    case TimePeriodRange.ThisWeek:
                        dateTimePart = DateTimeUnit.Week;
                        break;
                    case TimePeriodRange.LastWeek:
                        referenceDate = referenceDate.AddDays(-7);
                        dateTimePart = DateTimeUnit.Week;
                        break;
                    case TimePeriodRange.ThisMonth:
                        dateTimePart = DateTimeUnit.Month;
                        break;
                    case TimePeriodRange.LastMonth:
                        referenceDate = referenceDate.AddMonths(-1);
                        dateTimePart = DateTimeUnit.Month;
                        break;
                    case TimePeriodRange.ThisYear:
                        dateTimePart = DateTimeUnit.Year;
                        break;
                    case TimePeriodRange.LastYear:
                        dateTimePart = DateTimeUnit.Year;
                        referenceDate = referenceDate.AddYears(-1);
                        break;
                    case TimePeriodRange.ThisQuarter:
                        dateTimePart = DateTimeUnit.Quarter;
                        break;
                    case TimePeriodRange.LastQuarter:
                        dateTimePart = DateTimeUnit.Quarter;
                        referenceDate = referenceDate.AddMonths(-3);
                        break;

                    default:
                        throw new ExtractException("ELI41580", "Unexpected relative time period.");
                }
                startDate = referenceDate.Floor(dateTimePart);
                // Ensure a reference date that falls directly on a timeframe boundary does not
                // yield an instantaneous range.
                endDate = referenceDate.Add(new TimeSpan(ticks: 1)).Ceiling(dateTimePart);
                return new Tuple<DateTime, DateTime>(startDate, endDate);
            }
            catch (Exception ex)
            {

                throw ex.AsExtract("ELI41584");
            }
        }

        #endregion Utility Methods
    }
}
