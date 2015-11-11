using Extract.Database;
using Extract.Licensing;
using Spring.Core.TypeResolution;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
                if (valueMin == null && valueMax == null)
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
                if (rangeMin == null && rangeMax == null)
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
                throw ex.AsExtract("ELI38425");
            }
        }

        /// <summary>
        /// Gets any result component codes from <see paramref="orderMappingDbConnection"/> that the
        /// specified <see paramref="componentAKA"/> may refer to using the AKAs defined in
        /// <see paramref="componentDataDb"/>.
        /// </summary>
        /// <param name="customerDB">A <see cref="DbConnection"/> to the
        /// customer-specific OrderMappingDB.</param>
        /// <param name="componentDataDB">>A <see cref="DbConnection"/> to the URS OrderMappingDB.
        /// </param>
        /// <param name="componentAKA">A value that is to be treated as a potential AKA in the URS
        /// database.</param>
        /// <param name="orderCode">If not empty, the returned result codes will be restricted to
        /// components mapped to the specified order.</param>
        /// <returns>An array of any result component codes <see paramref="componentAKA"/> may refer
        /// to.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ESAKA")]
        public static string[] GetComponentCodesFromESAKA(DbConnection customerDB,
            DbConnection componentDataDB, string componentAKA, string orderCode)
        {
            try
            {
                // Find all component codes componentAKA may refer to in the URS DB>
                var ESComponentCodes = DBMethods.GetQueryResultsAsStringArray(componentDataDB,
                        "SELECT [ESComponentAKA].[ESComponentCode] FROM [ESComponentAKA] " +
                        "WHERE [Name] LIKE @0",
                        new Dictionary<string, string>() { { "@0", componentAKA } }, "");

                // Translate this to the component codes in the customer DB and ignore any AKAs in
                // the DisabledESComponentAKA table.
                var customerComponentCodes = DBMethods.GetQueryResultsAsStringArray(
                    customerDB,
                    "SELECT [LabTest].[OfficialName] FROM [LabTest] " +
                    "INNER JOIN [LabOrderTest] ON [LabTest].[TestCode] = [LabOrderTest].[TestCode] " +
                    "INNER JOIN [ComponentToESComponentMap] ON [LabTest].[TestCode] = [ComponentToESComponentMap].[ComponentCode] " +
                    "LEFT JOIN [DisabledESComponentAKA] ON [ComponentToESComponentMap].[ESComponentCode] = [DisabledESComponentAKA].[ESComponentCode] " +
                    "   AND [DisabledESComponentAKA].[ESComponentAKA] LIKE @0 " +
                    "WHERE (LEN(@1) = 0 OR [OrderCode] LIKE @1) " +
                    "AND [DisabledESComponentAKA].[ESComponentCode] IS NULL " +
                    "AND [ComponentToESComponentMap].[ESComponentCode] IN ('" + string.Join("','", ESComponentCodes) + "')",
                    new Dictionary<string, string>() { { "@0", componentAKA }, { "@1", orderCode } }, "");

                return customerComponentCodes;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39114");
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
            MustBeGreaterThan = 2
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

        #endregion Private Members
    }
}
