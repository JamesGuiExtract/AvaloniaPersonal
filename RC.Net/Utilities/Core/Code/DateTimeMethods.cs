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
        Year = 7
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
                    case DateTimeUnit.Year:
                        {
                            // Special case; years are not a constant length of time do to leap years.
                            DateTime yearStart = new DateTime(dateTime.Year, 1, 1);
                            return yearStart.AddYears(1);
                        }

                    case DateTimeUnit.Month:
                        {
                            // Special case; months are not a constant number of days.
                            DateTime yearStart = new DateTime(dateTime.Year, dateTime.Month, 1);
                            return yearStart.AddMonths(1);
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
    }
}
