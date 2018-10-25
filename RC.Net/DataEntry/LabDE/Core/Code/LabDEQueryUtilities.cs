using Extract.Database;
using Extract.Licensing;
using Extract.Utilities;
using Spring.Core.TypeResolution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// https://extract.atlassian.net/browse/ISSUE-12999
    /// Helper functions for use in <see cref="DataEntryQuery"/>s in LabDE DEPs. Allows common
    /// and/or complex query logic to be handled by shared compiled code rather than repeated
    /// separately in DEP assemblies.
    /// </summary>
    public static class LabDEQueryUtilities
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(LabDEQueryUtilities).ToString();

        #endregion Constants

        #region Public Methods

        /// <summary>
        /// Registers this class for use in expression query nodes. Must be called in the DEP's
        /// constructor.
        /// </summary>
        public static void Register()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI38426", _OBJECT_NAME);

                TypeRegistry.RegisterType("LabDEUtils", typeof(LabDEQueryUtilities));

                // I suspect these will become necessary at some point.
                // TypeRegistry.RegisterType("IAttribute", typeof(IAttribute));
                // TypeRegistry.RegisterType("SpatialString", typeof(SpatialString));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38424");
            }
        }

        /// <summary>
        /// Validates whether the specified <see paramref="flag"/> is valid given the
        /// <see paramref="value"/> and <see paramref="range"/>.
        /// </summary>
        /// <param name="flag">The abnormal flag to validate.</param>
        /// <param name="value">The value the flag pertains to.</param>
        /// <param name="range">The range that should be allowed for <see paramref="value"/>.
        /// </param>
        /// <returns><see langword="true"/> if the flag is valid for the value and range or if the
        /// value and/or range cannot be parsed numerically; <see langword="false"/> if the flag
        /// does not appear to be valid.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flag")]
        public static bool ValidateFlagAgainstValueAndRange(string flag, string value, string range)
        {
            try
            {
                // If an unsupported flag value is specified, don't warn.
                if (!string.IsNullOrWhiteSpace(flag) && !Regex.IsMatch(flag, @"^(H|HH|L|LL|A|AA)$"))
                {
                    return true;
                }

                // Parse any supported numerical format from the value.
                value = ParseNumericalValue(value);

                // If we are not able to parse the value, don't warn.
                if (string.IsNullOrWhiteSpace(value))
                {
                    return true;
                }

                // Parse the minimum possible value. Null if there is no minimum value (i.e., <X).
                ComparisonBias valueMinBias;
                double? valueMin = ParseMinValue(value, out valueMinBias);

                // Parse the maximum possible value. Null if there is no maximum value. (Will be the
                // same as valueMin if the value is not a range.)
                ComparisonBias valueMaxBias;
                double? valueMax = ParseMaxValue(value, out valueMaxBias);

                // If we are not able to parse either the min or max value, don't warn.
                if (valueMinBias == ComparisonBias.Error ||
                    valueMaxBias == ComparisonBias.Error ||
                    (valueMin == null && valueMax == null))
                {
                    return true;
                }

                // Parse any supported numerical format from the range.
                range = ParseNumericalValue(range);

                // If we are not able to parse the range, don't warn.
                if (string.IsNullOrWhiteSpace(range))
                {
                    return true;
                }

                // Parse the minimum allowed value. Null if there is no minimum. (i.e., <X).
                ComparisonBias rangeMinBias;
                double? rangeMin = ParseMinValue(range, out rangeMinBias);

                // Parse the maximum allowed value. Null if there is no maximum. (i.e., >X).
                ComparisonBias rangeMaxBias;
                double? rangeMax = ParseMaxValue(range, out rangeMaxBias);

                // If we are not able to parse either the min or max of range, don't warn.
                if (rangeMinBias == ComparisonBias.Error ||
                    rangeMaxBias == ComparisonBias.Error ||
                    rangeMin == null && rangeMax == null)
                {
                    return true;
                }

                // If we don't have a range minimum to compare consider min range boundary validated
                bool inRangeLow = (rangeMin == null);

                // If there is a minimum possible value, determine if it is within the minimum
                // permitted range.
                if (!inRangeLow && valueMin != null)
                {
                    inRangeLow =
                        // Compare minimum range and value using >= if appropriate
                        (valueMinBias >= rangeMinBias && valueMin >= rangeMin) ||
                        // Compare minimum range and value using > if appropriate
                        (valueMinBias < rangeMinBias && valueMin > rangeMin);

                }

                // Determine if minimum range comparison is ambiguous because it spans the value
                // spans minimum range.
                // (e.g. value = "<4" vs range = "3-5")
                bool ambiguousValueMin = rangeMin != null &&
                    // Value min not set or less than range min
                    (valueMin == null ||
                        ((valueMinBias >= rangeMinBias && valueMin < rangeMin) ||
                         (valueMinBias < rangeMinBias && valueMin <= rangeMin)))
                    // Value max not set or within range max
                    &&
                    (valueMax == null ||
                        ((valueMaxBias >= rangeMinBias && valueMax >= rangeMin) ||
                         (valueMaxBias < rangeMinBias && valueMax > rangeMin)));

                // If we don't have a range maximum to compare consider max range boundary validated
                bool inRangeHigh = (rangeMax == null);

                // If there is a maximum possible value, determine if it is within the maximum
                // permitted range.
                if (!inRangeHigh && valueMax != null)
                {
                    inRangeHigh =
                        // Compare maximum range and value using < if appropriate
                        (valueMaxBias > rangeMaxBias && valueMax < rangeMax) ||
                        // Compare maximum range and value using <= if appropriate
                        (valueMaxBias <= rangeMaxBias && valueMax <= rangeMax);
                }
                
                // Determine if maximum range comparison is ambiguous because it spans the value
                // spans maximum range.
                // (e.g. value = "2-4" vs range = "<3")
                bool ambiguousValueMax = rangeMax != null &&
                    // Value min not set or less than range max
                    (valueMin == null ||
                        ((valueMinBias > rangeMaxBias && valueMin < rangeMax) ||
                         (valueMinBias <= rangeMaxBias && valueMin <= rangeMax)))
                    &&
                    // Value max not set or greater than range max
                    (valueMax == null ||
                        ((valueMaxBias > rangeMaxBias && valueMax >= rangeMax) ||
                         (valueMaxBias <= rangeMaxBias && valueMax > rangeMax)));

                // Don't warn if the flag is missing and the value is within the range min and max
                // either explicitly or ambiguously
                if (string.IsNullOrWhiteSpace(flag) &&
                    (inRangeLow || ambiguousValueMin) &&
                    (inRangeHigh || ambiguousValueMax))
                {
                    return true;
                }

                // Don't warn if the flag is low and the value is outside range min or is ambiguously
                // within the range min
                if (flag.StartsWith("L", StringComparison.Ordinal) && 
                    (!inRangeLow || ambiguousValueMin))
                {
                    return true;
                }

                // Don't warn if the flag is high and the value is outside range max or is ambiguously
                // within the range max
                if (flag.StartsWith("H", StringComparison.Ordinal) &&
                    (!inRangeHigh || ambiguousValueMax))
                {
                    return true;
                }

                // Don't warn if the flag is abnormal and the value outside of either the range min or
                // max or is ambiguously within either
                if (flag.StartsWith("A", StringComparison.Ordinal) &&
                    (!inRangeLow || ambiguousValueMin || !inRangeHigh || ambiguousValueMax))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI38425");
                return true;
            }
        }

        /// <summary>
        /// ****************************************************************************************
        /// Don't use this method. Instead, use GetComponentNamesFromAKA (or GetComponentCodesFromAKA
        /// for TestCodes instead of OfficialNames) so that both URS AKAs and customer-specific AKAs
        /// are used to generate the list. Using one of those two methods instead means that the
        /// separate query against the customer-specific OrderMappingDB can/should be removed from
        /// the DEP. It also means that the AKA can be mapped to a Result Component even if the AKA
        /// appears both in the customer-specific DB and the URS DB for the same Result Component.
        /// ****************************************************************************************
        /// Gets any result component official names from <see paramref="orderMappingDbConnection"/>
        /// that the specified <see paramref="componentAKA"/> may refer to using the AKAs defined in
        /// <see paramref="componentDataDb"/>.
        /// </summary>
        /// <param name="customerDB">A <see cref="DbConnection"/> to the
        /// customer-specific OrderMappingDB.</param>
        /// <param name="componentDataDB">>A <see cref="DbConnection"/> to the URS OrderMappingDB.
        /// </param>
        /// <param name="componentAKA">A value that is to be treated as a potential AKA in the URS
        /// database.</param>
        /// <param name="orderCode">If not empty, the returned names will be restricted to
        /// components mapped to the specified order.</param>
        /// <returns>An array of any result component official names <see paramref="componentAKA"/>
        /// may refer to.</returns>
        [Obsolete("GetComponentCodesFromESAKA is deprecated. Use GetComponentNamesFromAKA instead.")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ESAKA")]
        public static string[] GetComponentCodesFromESAKA(DbConnection customerDB,
            DbConnection componentDataDB, string componentAKA, string orderCode)
        {
            try
            {
                var truncatedComponentAKA = componentAKA.Length > 255 ? componentAKA.Substring(0, 255) : componentAKA;
                var truncatedOrderCode = orderCode.Length > 25 ? orderCode.Substring(0, 25) : orderCode;

                // Find all component codes componentAKA may refer to in the URS DB>
                var ESComponentCodes = DBMethods.GetQueryResultsAsStringArray(componentDataDB,
                        "SELECT [ESComponentAKA].[ESComponentCode] FROM [ESComponentAKA] " +
                        "WHERE [Name] = @0",
                        new Dictionary<string, string>() { { "@0", truncatedComponentAKA } }, "");

                // Translate this to the component codes in the customer DB and ignore any AKAs in
                // the DisabledESComponentAKA table.
                var customerComponentCodes = DBMethods.GetQueryResultsAsStringArray(
                    customerDB,
                    "SELECT [LabTest].[OfficialName] FROM [LabTest] " +
                    "INNER JOIN [LabOrderTest] ON [LabTest].[TestCode] = [LabOrderTest].[TestCode] " +
                    "INNER JOIN [ComponentToESComponentMap] ON [LabTest].[TestCode] = [ComponentToESComponentMap].[ComponentCode] " +
                    "LEFT JOIN [DisabledESComponentAKA] ON [ComponentToESComponentMap].[ESComponentCode] = [DisabledESComponentAKA].[ESComponentCode] " +
                    "   AND [DisabledESComponentAKA].[ESComponentAKA] = @0 " +
                    "WHERE (LEN(@1) = 0 OR [OrderCode] = @1) " +
                    "AND [DisabledESComponentAKA].[ESComponentCode] IS NULL " +
                    "AND [ComponentToESComponentMap].[ESComponentCode] IN ('" + string.Join("','", ESComponentCodes) + "')",
                    new Dictionary<string, string>() { { "@0", truncatedComponentAKA }, { "@1", truncatedOrderCode } }, "");

                return customerComponentCodes;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39114");
            }
        }

        /// <summary>
        /// Gets any result component codes from <see paramref="orderMappingDbConnection"/> that the
        /// specified <see paramref="componentAKA"/> may refer to using the AKAs defined in
        /// <see paramref="componentDataDb"/> or <see paramref="customerDB"/>.
        /// </summary>
        /// <param name="customerDB">A <see cref="DbConnection"/> to the
        /// customer-specific OrderMappingDB.</param>
        /// <param name="componentDataDB">>A <see cref="DbConnection"/> to the URS OrderMappingDB.
        /// </param>
        /// <param name="componentAKA">A value that is to be treated as a potential AKA in the 
        /// databases.</param>
        /// <param name="orderCode">If not empty, the returned result codes will be restricted to
        /// components mapped to the specified order.</param>
        /// <returns>An array of any result component codes <see paramref="componentAKA"/> may refer
        /// to.</returns>
        public static string[] GetComponentCodesFromAKA(DbConnection customerDB,
            DbConnection componentDataDB, string componentAKA, string orderCode)
        {
            try
            {
                var truncatedComponentAKA = componentAKA.Length > 255 ? componentAKA.Substring(0, 255) : componentAKA;
                var truncatedOrderCode = orderCode.Length > 25 ? orderCode.Substring(0, 25) : orderCode;

                // Find all component codes componentAKA may refer to in the URS DB>
                var ESComponentCodes = DBMethods.GetQueryResultsAsStringArray(componentDataDB,
                        "SELECT [ESComponentAKA].[ESComponentCode] FROM [ESComponentAKA] " +
                        "WHERE [Name] = @0",
                        new Dictionary<string, string>() { { "@0", truncatedComponentAKA } }, "");

                // Translate these to the component codes in the customer DB and ignore any AKAs in
                // the DisabledESComponentAKA table.
                var customerComponentCodes = DBMethods.GetQueryResultsAsStringArray(
                    customerDB,
                    "SELECT [LabTest].[TestCode] FROM [LabTest] " +
                    "INNER JOIN [LabOrderTest] ON [LabTest].[TestCode] = [LabOrderTest].[TestCode] " +
                    "INNER JOIN [ComponentToESComponentMap] ON [LabTest].[TestCode] = [ComponentToESComponentMap].[ComponentCode] " +
                    "LEFT JOIN [DisabledESComponentAKA] ON [ComponentToESComponentMap].[ESComponentCode] = [DisabledESComponentAKA].[ESComponentCode] " +
                    "   AND [DisabledESComponentAKA].[ESComponentAKA] = @0 " +
                    "WHERE (LEN(@1) = 0 OR [OrderCode] = @1) " +
                    "AND [DisabledESComponentAKA].[ESComponentCode] IS NULL " +
                    "AND [ComponentToESComponentMap].[ESComponentCode] IN ('" + string.Join("','", ESComponentCodes) + "') " +
                    "UNION SELECT [AlternateTestName].[TestCode] FROM [AlternateTestName] " +
                    "INNER JOIN [LabOrderTest] ON [AlternateTestName].[TestCode] = [LabOrderTest].[TestCode] " +
                    "WHERE [Name] = @0 AND [StatusCode] = 'A' " +
                    "AND (LEN(@1) = 0 OR [OrderCode] = @1) "
                    , new Dictionary<string, string>() { { "@0", truncatedComponentAKA }, { "@1", truncatedOrderCode } }, "");

                return customerComponentCodes;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39268");
            }
        }

        /// <summary>
        /// Gets any result component official names from <see paramref="orderMappingDbConnection"/>
        /// that the specified <see paramref="componentAKA"/> may refer to using the AKAs defined in
        /// <see paramref="componentDataDb"/> or <see paramref="customerDB"/>.
        /// </summary>
        /// <param name="customerDB">A <see cref="DbConnection"/> to the
        /// customer-specific OrderMappingDB.</param>
        /// <param name="componentDataDB">>A <see cref="DbConnection"/> to the URS OrderMappingDB.
        /// </param>
        /// <param name="componentAKA">A value that is to be treated as a potential AKA in the 
        /// databases.</param>
        /// <param name="orderCode">If not empty, the returned names will be restricted to
        /// components mapped to the specified order.</param>
        /// <returns>An array of any result component official names <see paramref="componentAKA"/>
        /// may refer to.</returns>
        public static string[] GetComponentNamesFromAKA(DbConnection customerDB,
            DbConnection componentDataDB, string componentAKA, string orderCode)
        {
            try
            {
                var componentCodes = GetComponentCodesFromAKA(customerDB, componentDataDB, componentAKA, orderCode)
                    .Select(testCode => "'" + testCode.Replace("'", "''") + "'").ToList();

                if (componentCodes.Count == 0)
                {
                    return new string[0];
                }

                // Translate these to the official name in the customer DB
                var componentNames = DBMethods.GetQueryResultsAsStringArray(
                    customerDB,
                    "SELECT [OfficialName] FROM [LabTest] " +
                    "WHERE [TestCode] IN (" + string.Join(",", componentCodes) + ")");

                return componentNames;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39269");
            }
        }

        /// <summary>
        /// Gets all official names and valid AKAs for the given order code
        /// </summary>
        /// <param name="customerDB">A <see cref="DbConnection"/> to the
        /// customer-specific OrderMappingDB.</param>
        /// <param name="componentDataDB">>A <see cref="DbConnection"/> to the URS OrderMappingDB.
        /// </param>
        /// <param name="orderCode">If not empty, the returned names will be restricted to
        /// components mapped to the specified order.</param>
        /// <returns>An array of CSV records where the first item is
        /// the official name and the second, optional, item is an alternate name</returns>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "AKAs")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "AKAs")]
        public static string[] GetComponentNamesAndAKAs(DbConnection customerDB,
            DbConnection componentDataDB, string orderCode)
        {
            try
            {
                var safeOrderCode = (orderCode.Length > 25 ? orderCode.Substring(0, 25) : orderCode)
                    .Replace("'", "''");

                // Populate mapping of names to es test codes using customer-specific DB
                var query = "SELECT [OfficialName], [AKA], [ESComponentCode] FROM [LabTest]"
                    + " JOIN [LabOrderTest] ON [LabOrderTest].[TestCode] = [LabTest].[TestCode]"
                    + " LEFT JOIN (SELECT [TestCode], [Name] AS [AKA]"
                    + "   FROM [AlternateTestName] WHERE [StatusCode] = 'A') [AKAs]"
                    + " ON [LabTest].[TestCode] = [AKAs].[TestCode]"
                    + " LEFT JOIN [ComponentToESComponentMap] ON [LabTest].[TestCode] = [ComponentCode]"
                    + (string.IsNullOrWhiteSpace(safeOrderCode)
                        ? ""
                        : " WHERE [LabOrderTest].[OrderCode] = '" + safeOrderCode + "'");

                var results = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                var esCodesToNames = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

                var dt = DBMethods.ExecuteDBQuery(customerDB, query);
                foreach (DataRow row in dt.Rows)
                {
                    string officialName = row.ItemArray[0] as string;
                    string aka = row.ItemArray[1] as string;
                    string esComponentCode = row.ItemArray[2] as string;

                    List<string> akas = results.GetOrAdd(officialName, _ => new List<string>());
                    if (aka != null)
                    {
                        akas.Add(aka);
                    }

                    if (esComponentCode != null)
                    {
                        List<string> names = esCodesToNames.GetOrAdd(esComponentCode, _ => new List<string>());
                        names.Add(officialName);
                    }
                }

                // Get set of disabled ESComponentAKAs
                var disabledESComponentAKAs = new HashSet<Tuple<string, string>>();
                query = "SELECT [ESComponentCode], [ESComponentAKA] FROM [DisabledESComponentAKA]";
                dt = DBMethods.ExecuteDBQuery(customerDB, query);
                foreach (DataRow row in dt.Rows)
                {
                    string code = ((string)row.ItemArray[0]).ToUpperInvariant();
                    string aka = ((string)row.ItemArray[1]).ToUpperInvariant();

                    disabledESComponentAKAs.Add(Tuple.Create(code, aka));
                }

                // Add additional names using the component data (URS) database
                if (esCodesToNames.Any())
                {
                    query = "SELECT [ESComponentCode], [Name] FROM [ESComponentAKA]"
                            + " WHERE [ESComponentCode] IN ('"
                            + string.Join("','", esCodesToNames.Keys.Select(code => code.Replace("'", "''")))
                            + "')";
                    dt = DBMethods.ExecuteDBQuery(componentDataDB, query);
                    foreach (DataRow row in dt.Rows)
                    {
                        string code = ((string)row.ItemArray[0]);
                        string aka = ((string)row.ItemArray[1]);

                        // Skip if this AKA has been disabled by the customer order mapping DB
                        if (disabledESComponentAKAs.Contains(Tuple.Create(code.ToUpperInvariant(), aka.ToUpperInvariant())))
                        {
                            continue;
                        }

                        foreach (var officialName in esCodesToNames[code])
                        {
                            List<string> akas = results.GetOrAdd(officialName, _ => new List<string>());
                            akas.Add(aka);
                        }
                    }
                }

                var resultsTable = new DataTable { Locale = CultureInfo.CurrentCulture };
                resultsTable.Columns.Add("Name", typeof(string));
                resultsTable.Columns.Add("AKA", typeof(string));
                var rows = resultsTable.Rows;
                foreach (var (officialName, akas) in results)
                {
                    if (!akas.Any())
                    {
                        rows.Add(officialName, null);
                    }
                    else
                    {
                        foreach (var aka in akas)
                        {
                            rows.Add(officialName, aka);
                        }
                    }
                }

                return resultsTable.ToStringArray(", ");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39269");
            }
        }
        /// <summary>
        /// Accepts a date in a variety of formats, and returns a normalized date.
        /// </summary>
        /// <param name="inputDate">The date.</param>
        /// <returns>Returns the date in a normalized format, or on error just returns the original date</returns>
        public static string FormatDate(string inputDate)
        {
            string tempDate;
            string month;
            string day;
            string year;

            string date = inputDate.Trim();

            try
            {
                string generalDatePattern = @"^\d{1,2}[-/\\\.]\d{1,2}[-/\\\.](\d{2}|\d{4})$";
                if (Regex.IsMatch(date, generalDatePattern))
                {
                    var match = Regex.Match(date, @"^\d{1,2}(?=[-/\\\.])");
                    month = match.Length == 2 ? match.Value : '0' + match.Value;

                    match = Regex.Match(date, @"(?<=^\d{1,2}[-/\\\.])\d{1,2}(?=[-/\\\.])");
                    day = match.Length == 2 ? match.Value : '0' + match.Value;

                    match = Regex.Match(date,
                                        @"(?<=\d{1,2}[-/\\\.])(\d{2}|\d{4})$");
                    year = match.Value;
                    if (match.Length < 4)
                    {
                        year = ConvertTwoDigitYearToFourDigits(match.Value);
                    }
                }
                else
                {
                    // 8 digit date match attempted here - no separators
                    var match = Regex.Match(date, @"^\d{8}$");
                    if (match.Success)
                    {
                        month = match.Value.Substring(startIndex: 0, length: 2);
                        day = match.Value.Substring(startIndex: 2, length: 2);
                        year = match.Value.Substring(startIndex: 4);
                    }
                    else
                    {
                        // 6 digit date match attempted here - no separators
                        var match_ = Regex.Match(date, @"^\d{6}$");
                        if (match_.Success)
                        {
                            month = match_.Value.Substring(startIndex: 0, length: 2);
                            day = match_.Value.Substring(startIndex: 2, length: 2);
                            year = ConvertTwoDigitYearToFourDigits(match_.Value.Substring(startIndex: 4, length: 2));
                        }
                        else
                        {
                            // https://extract.atlassian.net/browse/ISSUE-14370
                            // Fallback to built-in DateTime parsing.
                            DateTime parsedDateTime;

                            // Setup a format-info object to ensure years are always <= to current year
                            var dtfi = (DateTimeFormatInfo)CultureInfo.CurrentCulture.DateTimeFormat.Clone();
                            dtfi.Calendar = (Calendar)dtfi.Calendar.Clone();
                            dtfi.Calendar.TwoDigitYearMax = DateTime.Now.Year;

                            if (DateTime.TryParse(date, dtfi,
                                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault,
                                out parsedDateTime))
                            {
                                return parsedDateTime.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                return "";
                            }
                        }
                    }
                }

                // Now make sure that the date is valid.
                int iMonth = Convert.ToInt32(month, CultureInfo.InvariantCulture);
                int iDay = Convert.ToInt32(day, CultureInfo.InvariantCulture);
                int iYear = Convert.ToInt32(year, CultureInfo.InvariantCulture);
                try
                {
                    DateTime checkDate = new DateTime(iYear, iMonth, iDay);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return "";
                }

                tempDate = month + '/' + day + '/' + year;
                return tempDate;
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// Formats the date which may have an optional time portion.
        /// </summary>
        /// <param name="inputDate">The date.</param>
        /// <returns>returns the formatted date, or "" on error</returns>
        static public string FormatDateWithOptionalTime(string inputDate)
        {
            try
            {
                var date = inputDate.Trim();
                string[] parts = date.Split(' ');
                foreach (var part in parts)
                {
                    var processed = FormatDate(part);
                    if (!String.IsNullOrWhiteSpace(processed))
                    {
                        return processed;
                    }
                }

                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// Formats the time.
        /// </summary>
        /// <param name="time">string with an embedded time value somewhere in it - maybe.</param>
        /// <returns>returns any recognized time string</returns>
        public static string FormatTime(string time)
        {
            try
            {
                var trimmedTime = time.Trim();
                int hours;
                int minutes;
                string matchedTime = "";
                string anteOrPostMeridian = "";

                var matches = Regex.Matches(trimmedTime, @"(\d?\d:?[0-5]\d\s?(?:AM|PM)?)", RegexOptions.None);
                var count = matches.Count;
                if (count == 0)
                {
                    return matchedTime;
                }
                else if (count == 1)
                {
                    matchedTime = matches[0].Value;
                }
                else
                {
                    string bestMatch = matches[0].Value;
                    for (int i = 1; i < count; ++i)
                    {
                        bestMatch = PickBestMatchforTimeValue(bestMatch, matches[i].Value);
                    }

                    matchedTime = bestMatch;
                }

                var hoursMinutes = GetHoursAndMinutesAndIndicator(matchedTime);
                hours = hoursMinutes.Item1;
                minutes = hoursMinutes.Item2;
                anteOrPostMeridian = hoursMinutes.Item3;
                if (anteOrPostMeridian == "PM")
                {
                    hours += 12;
                }

                DateTime dt = new DateTime(year: 2000, month: 1, day: 1, hour: hours, minute: minutes, second: 0);
                if (String.IsNullOrWhiteSpace(anteOrPostMeridian) || anteOrPostMeridian == "PM")
                {
                    return String.Format(CultureInfo.InvariantCulture,
                                         "{0}:{1}", 
                                         dt.Hour.ToString("D2", CultureInfo.InvariantCulture), 
                                         dt.Minute.ToString("D2", CultureInfo.InvariantCulture));
                }
                else
                {
                    return String.Format(CultureInfo.InvariantCulture,
                                         "{0}:{1} {2}", 
                                         dt.Hour.ToString("D2", CultureInfo.InvariantCulture), 
                                         dt.Minute.ToString("D2", CultureInfo.InvariantCulture), 
                                         anteOrPostMeridian);
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        #endregion Public Methods

        #region Private Members

        /// <summary>
        /// Specifies when comparing a value domain boundary to a range domain boundary what kind of
        /// comparison should be used (&lt;, &lt;=, &gt;, &gt;=)
        /// </summary>
        enum ComparisonBias
        {
            /// <summary>
            /// First value must be strictly less than the second value.
            /// </summary>
            MustBeLessThan = 0,

            /// <summary>
            /// First value may be equal to the second value.
            /// </summary>
            MayBeEqualTo = 1,

            /// <summary>
            /// First value must be strictly greater than the second value.
            /// </summary>
            MustBeGreaterThan = 2,

            /// <summary>
            /// There was an error parsing the value.
            /// </summary>
            Error = 3
        }

        /// <summary>
        /// Helper method for <see cref="ValidateFlagAgainstValueAndRange"/>.
        /// Parses any supported numerical format from <see paramref="value"/>.
        /// </summary>
        /// <param name="value">The string value to be parsed.</param>
        /// <returns>A supported numerical format found in <see paramref="value"/>; otherwise,
        /// <see langword="null"/>.</returns>
        static string ParseNumericalValue(string value)
        {
            // Attempt to parse a numerical expression from the value field
            // Allow for value of #, #-#, >#, <#, >=#, <=#, >/=#, </=#, POS, NEG, POSITIVE, NEGATIVE
            if (Regex.IsMatch(value, @"\bPOS(ITIVE)?\b", RegexOptions.IgnoreCase))
            {
                value = ">0";
            }
            else if (Regex.IsMatch(value, @"\bNEG(ATIVE)?\b", RegexOptions.IgnoreCase))
            {
                value = "0";
            }
            else
            {
                var match = Regex.Match(value,
                    @"^([\s><]{0,3}(/|OR)?[\s=]{0,3})?((?<=\d),\d{3}(?!=\d)|\d|\.|\s)+$|^\s*(((?<=\d),\d{3}(?!=\d)|\d|\.|\s)+-?){2}\s*$",
                    RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    return null;
                }

                value = match.Value.Replace(",", "").Replace(" ", "");
            }

            return value;
        }

        /// <summary>
        /// Helper method for <see cref="ValidateFlagAgainstValueAndRange"/>.
        /// Parses the minimum numerical value of <see paramref="value"/>. <see langword="null"/> if
        /// there is not minimum value. (i.e., &lt;X).
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="valueMinBias">A <see cref="ComparisonBias"/> indicating how the parsed
        /// value should be compared.</param>
        /// <returns>The minimum numerical value or <see langword="null"/> if there is no minimum
        /// value.</returns>
        static double? ParseMinValue(string value, out ComparisonBias valueMinBias)
        {
            try
            {
                var valueMinMatch =
                        Regex.Match(value, @"(^([\d\.]+)$)|([\d\.]+(?=-\d+))|((?<=>(/|OR)?=)[\d\.]+)",
                            RegexOptions.IgnoreCase);

                // Set bias as to whether equivalence is allowed.
                valueMinBias = valueMinMatch.Success
                    ? ComparisonBias.MayBeEqualTo
                    : ComparisonBias.MustBeGreaterThan;

                string valueMinString = null;
                if (valueMinMatch.Success)
                {
                    valueMinString = valueMinMatch.Value;
                }
                else
                {
                    // If equivalence is not allowed, parse for minimum where equivalence is not
                    // permitted.
                    valueMinMatch = Regex.Match(value, @"(?<=>)[\d\.]+", RegexOptions.IgnoreCase);
                    if (valueMinMatch.Success)
                    {
                        valueMinString = valueMinMatch.Value;
                    }
                }

                // Convert valueMinString to a double
                double? valueMin = null;
                if (!string.IsNullOrEmpty(valueMinString))
                {
                    valueMin = double.Parse(valueMinString, CultureInfo.CurrentCulture);
                }

                return valueMin;
            }
            catch
            {
                // https://extract.atlassian.net/browse/ISSUE-13738
                // The regex to identify valid numbers is not foolproof (nor, do I think, should we
                // ever count on it being so. Rather than throw an exception just return null if we
                // failed to parse the value.
                valueMinBias = ComparisonBias.Error;
                return null;
            }
        }

        /// <summary>
        /// Helper method for <see cref="ValidateFlagAgainstValueAndRange"/>.
        /// Parses the maximum numerical value of <see paramref="value"/>. <see langword="null"/> if
        /// there is not maximum value. (i.e., &gt;X).
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <param name="valueMaxBias">A <see cref="ComparisonBias"/> indicating how the parsed
        /// value should be compared.</param>
        /// <returns>The maximum numerical value or <see langword="null"/> if there is no maximum
        /// value.</returns>
        static double? ParseMaxValue(string value, out ComparisonBias valueMaxBias)
        {
            try
            {
                // Parse the maximum possible value where it is considered in-range if equal.
                var valueMaxMatch =
                    Regex.Match(value, @"(^([\d\.]+)$)|((?<=\d+-)[\d\.]+)|((?<=<(/|OR)?=)[\d\.]+)",
                        RegexOptions.IgnoreCase);

                // Set bias as to whether equivalence is allowed.
                valueMaxBias = valueMaxMatch.Success
                    ? ComparisonBias.MayBeEqualTo
                    : ComparisonBias.MustBeLessThan;

                string valueMaxString = null;
                if (valueMaxMatch.Success)
                {
                    valueMaxString = valueMaxMatch.Value;
                }
                else
                {
                    // If equivalence is not allowed, parse for maximum where equivalence is not
                    // permitted.
                    valueMaxMatch = Regex.Match(value, @"(?<=<)[\d\.]+", RegexOptions.IgnoreCase);
                    if (valueMaxMatch.Success)
                    {
                        valueMaxString = valueMaxMatch.Value;
                    }
                }

                // Convert valueMaxString to a double
                double? valueMax = null;
                if (!string.IsNullOrEmpty(valueMaxString))
                {
                    valueMax = double.Parse(valueMaxString, CultureInfo.CurrentCulture);
                }

                return valueMax;
            }
            catch
            {
                // https://extract.atlassian.net/browse/ISSUE-13738
                // The regex to identify valid numbers is not foolproof (nor, do I think, should we
                // ever count on it being so. Rather than throw an exception just return null if we
                // failed to parse the value.
                valueMaxBias = ComparisonBias.Error;
                return null;
            }
        }

        /// <summary>
        /// Converts a two digit year to four digits.
        /// </summary>
        /// <param name="inYear">The year value to convert.</param>
        /// <returns>4 digit year</returns>
        static string ConvertTwoDigitYearToFourDigits(string inYear)
        {
            int onesPart = Convert.ToInt32(inYear, CultureInfo.InvariantCulture);
            int currentYear = DateTime.Now.Year;
            int hundredsPart = onesPart <= currentYear % 100
                ? currentYear / 100
                : currentYear / 100 - 1;
            return UtilityMethods.FormatInvariant($"{hundredsPart}{inYear}");
        }

        /// <summary>
        /// Picks the best match for time value, preferring AM|PM markers, using ':' as a second choice.
        /// </summary>
        /// <param name="lhs">The LHS candidate time.</param>
        /// <param name="rhs">The RHS candidate time.</param>
        /// <returns>the "better" match, or lhs if equivalent</returns>
        static string PickBestMatchforTimeValue(string lhs, string rhs)
        {
            bool lhsHasColon = lhs.Contains(":");
            bool rhsHasColon = rhs.Contains(":");

            bool lhsHasAMPM = Regex.Match(lhs, @"(AM|PM)").Success;
            bool rhsHasAMPM = Regex.Match(rhs, @"(AM|PM)").Success;

            if (lhsHasAMPM && !rhsHasAMPM)
            {
                return lhs;
            }
            else if (rhsHasAMPM && !lhsHasAMPM)
            {
                return rhs;
            }

            if (lhsHasColon && !rhsHasColon)
            {
                return lhs;
            }
            else if (rhsHasColon && !lhsHasColon)
            {
                return rhs;
            }

            // Here when both sides are equivalent, so just pick one as neither is a better match)
            return lhs;
        }

        /// <summary>
        /// Picks apart a string and returns the numeric values of hours and minutes, and the AM/PM
        /// indicator if it exists.
        /// </summary>
        /// <param name="time">input time string</param>
        /// <returns>Tuple of hours, minutes, and meridion indicator</returns>
        static Tuple<int, int, string> GetHoursAndMinutesAndIndicator(string time)
        {
            int hours = -1;
            int minutes = -1;

            // AM or PM
            string meridianIndicator = "";

            string[] parts = time.Split(new char[] { ':', ' ' });
            if (parts.Count() > 1)
            {
                hours = Convert.ToInt32(parts[0], CultureInfo.InvariantCulture);

                // Handle the case where the input value was e.g. 9:00AM, or 9:00 AM.
                var mins = parts[1];
                string sMinutes = mins;
                if (mins.Count() > 2)
                {
                    sMinutes = mins.Substring(startIndex: 0, length: 2);
                    meridianIndicator = mins.Substring(startIndex: 2);
                }

                minutes = Convert.ToInt32(sMinutes, CultureInfo.InvariantCulture);
                if (parts.Count() >= 3)
                {
                    meridianIndicator = parts[2];
                }
            }
            else if (time.Length >= 4)
            {
                var Hours = time.Substring(startIndex: 0, length: 2);
                var Minutes = time.Substring(startIndex: 2, length: 2);
                hours = Convert.ToInt32(Hours, CultureInfo.InvariantCulture);
                minutes = Convert.ToInt32(Minutes, CultureInfo.InvariantCulture);
            }

            return new Tuple<int, int, string>(hours, minutes, meridianIndicator);
        }






        #endregion Private Members
    }
}