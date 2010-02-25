using Extract.Imaging;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.Rules
{
    /// <summary>
    /// An enum defining types of string matches.
    /// </summary>
    public enum MatchType
    {
        /// <summary>
        /// Indicates that the match is an actual data match.
        /// </summary>
        Match,

        /// <summary>
        /// Indicates that the match is a clue that may indicate sensitive data nearby.
        /// </summary>
        Clue
    }

    /// <summary>
    /// A class that represents a particular match from a SpatialString.  Contains both the
    /// location (as a <see cref="Extract.Imaging.RasterZone"/>) and the <see cref="MatchType"/> that was found.
    /// </summary>
    public class MatchResult : IComparable<MatchResult>
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(MatchResult).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="List{T}"/> of <see cref="Extract.Imaging.RasterZone"/> objects for this match.
        /// </summary>
        readonly ReadOnlyCollection<RasterZone> _rasterZones;

        /// <summary>
        /// The <see cref="MatchType"/> for this match.
        /// </summary>
        readonly MatchType _matchType;

        /// <summary>
        /// Text associated with the match (will be empty if <see cref="MatchType"/> is
        /// Match and will contain OCR text if <see cref="MatchType"/>
        /// is Clue.
        /// </summary>
        readonly string _text;

        /// <summary>
        /// The rule that created this <see cref="MatchResult"/>.
        /// </summary>
        readonly string _findingRule;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inititializes a new <see cref="MatchResult"/> class with the specified
        /// <see cref="RasterZone"/> and <see cref="MatchType"/>.
        /// </summary>
        /// <param name="rasterZone">The <see cref="RasterZone"/> for the match.</param>
        /// <param name="matchType">The <see cref="MatchType"/> for the match.</param>
        /// <param name="text">The text associated with this <see cref="MatchResult"/>.</param>
        /// <param name="findingRule">The rule which produced this <see cref="MatchResult"/>.</param>
        // This constructor is not currently called by any methods, but it may
        // be useful so we should leave it here.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public MatchResult(RasterZone rasterZone, MatchType matchType, string text,
            string findingRule)
            : this(new RasterZone[] { rasterZone }, matchType, text, findingRule)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MatchResult"/> class with the specified
        /// collection of <see cref="RasterZone"/> objects and <see cref="MatchType"/>
        /// </summary>
        /// <param name="rasterZones">An <see cref="IEnumerable{T}"/> collection
        /// of <see cref="RasterZone"/> objects for the match.</param>
        /// <param name="matchType">The <see cref="MatchType"/> for the match.</param>
        /// <param name="text">The text associated with this <see cref="MatchResult"/>.</param>
        /// <param name="findingRule">The rule which produced this <see cref="MatchResult"/>.</param>
        public MatchResult(IEnumerable<RasterZone> rasterZones, MatchType matchType, string text,
            string findingRule)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RedactionCoreObjects, "ELI23199",
                    _OBJECT_NAME);

                List<RasterZone> zones = new List<RasterZone>(rasterZones);
                _rasterZones = zones.AsReadOnly();
                _matchType = matchType;
                _text = text;
                _findingRule = findingRule;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22867", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the <see cref="RasterZone"/> for this <see cref="MatchResult"/>.
        /// </summary>
        /// <returns>The <see cref="RasterZone"/> for this <see cref="MatchResult"/>.</returns>
        public ReadOnlyCollection<RasterZone> RasterZones
        {
            get
            {
                return _rasterZones;
            }
        }

        /// <summary>
        /// Gets the <see cref="List{T}"/> of <see cref="MatchType"/> objects 
        /// for this <see cref="MatchResult"/>.
        /// </summary>
        /// <returns>The <see cref="MatchType"/> for this <see cref="MatchResult"/>.</returns>
        public MatchType MatchType
        {
            get
            {
                return _matchType;
            }
        }

        /// <summary>
        /// Gets the text associated with this <see cref="MatchResult"/>.  If
        /// <see cref="MatchType"/> is Clue then will contain the
        /// OCR text; if <see cref="MatchType"/> is Match will be
        /// empty.
        /// </summary>
        /// <returns>The text associated with this <see cref="MatchResult"/>.</returns>
        public string Text
        {
            get
            {
                return _text;
            }
        }

        /// <summary>
        /// Gets the name of the rule which produced this <see cref="MatchResult"/>.
        /// </summary>
        /// <returns>The name of the rule which produced this <see cref="MatchResult"/>.</returns>
        public string FindingRule
        {
            get
            {
                return _findingRule;
            }
        }

        #endregion Properties

        #region Static Methods

        /// <summary>
        /// Computes a <see cref="List{T}"/> of <see cref="MatchResult"/> objects
        /// for the specified <see cref="Regex"/> in the specified SpatialString
        /// and stores them with the specified <see cref="MatchType"/>.
        /// </summary>
        /// <param name="baseRule">The base name of the rule that called ComputeMatches.</param>
        /// <param name="regex">The <see cref="Regex"/> pattern to find. Must not
        /// be <see langword="null"/>.</param>
        /// <param name="ocrOutput">The SpatialString to search.</param>
        /// <param name="matchType">The <see cref="MatchType"/> to assign to the matches.</param>
        /// <param name="performIncrementalSearch">Whether to perform a full search or an
        /// incremental search of the ocr text.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="MatchResult"/> objects
        /// found in the specified SpatialString.</returns>
        /// <exception cref="ExtractException">If <paramref name="regex"/> is
        /// <see langword="null"/>.</exception>
        internal static MatchResultCollection ComputeMatches(string baseRule, Regex regex,
            SpatialString ocrOutput, MatchType matchType,
            bool performIncrementalSearch)
        {
            try
            {
                // Check that the regular expression is not null
                ExtractException.Assert("ELI22212", "Regular expression cannot be null!",
                    regex != null);

                // Create the list to hold the match results
                MatchResultCollection matches = new MatchResultCollection();

                if (performIncrementalSearch)
                {
                    // Compute the first match
                    SpatialString subString = ocrOutput;
                    Match match = regex.Match(subString.String);

                    // While there are matches, add the result and keep searching
                    while (match.Success)
                    {
                        // Get the next match result
                        MatchResult matchResult = 
                            ProcessMatch(baseRule, regex, match, subString, matchType);
                        if (matchResult == null)
                        {
                            continue;
                        }

                        // Add the match to the list
                        matches.Add(matchResult);

                        // Check if there is text left to search
                        if ((match.Index + 1) > subString.Size)
                        {
                            // Not enough text left to search, stop searching
                            break;
                        }

                        // Get the rest of the string starting at 1 character past the match
                        subString = subString.GetSubString(match.Index + 1, -1);

                        // Look for the next match
                        match = regex.Match(subString.String);
                    }
                }
                else
                {
                    MatchCollection matchResults = regex.Matches(ocrOutput.String);
                    foreach(Match match in matchResults)
                    {
                        // Get the next match result
                        MatchResult matchResult =
                            ProcessMatch(baseRule, regex, match, ocrOutput, matchType);
                        if (matchResult == null)
                        {
                            continue;
                        }

                        matches.Add(matchResult);
                    }
                }

                // Return the list of match results
                return matches;
            }
            catch(Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22213", ex);
            }
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> collection of <see cref="RasterZone"/> objects
        /// from the specified SpatialString for the specified <see cref="Group"/>.
        /// </summary>
        /// <param name="capture">The group to get the <see cref="RasterZone"/> objects
        /// for.</param>
        /// <param name="ocrOutput">The SpatialString where the <see cref="Group"/>
        /// was found.</param>
        /// <returns>A collection of <see cref="RasterZone"/> objects from 
        /// <paramref name="ocrOutput"/> for the specified <see cref="Group"/>; 
        /// <see langword="null"/> if no raster zones are available for this capture.</returns>
        static List<RasterZone> GetRasterZonesForCapture(Capture capture, SpatialString ocrOutput)
        {
            try
            {
                // If the capture has no length, we are done.
                if (capture.Length <= 0)
                {
                    return null;
                }

                // Get the capture as a spatial string
                SpatialString groupString = 
                    ocrOutput.GetSubString(capture.Index, capture.Index + capture.Length - 1);

                // If the spatial string is non-spatial, we are done.
                if (!groupString.HasSpatialInfo())
                {
                    return null;
                }

                // Get the raster zones for the match from the SpatialString
                UCLID_COMUTILSLib.IUnknownVector vectorOfZones =
                    groupString.GetOriginalImageRasterZones();

                // If there are no raster zones we are done
                int zones = vectorOfZones.Size();
                if (zones == 0)
                {
                    return null;
                }

                // Build a list of .Net raster zones from the IUnknownVector of
                // COM raster zones
                List<RasterZone> rasterZoneCollection = new List<RasterZone>(zones);
                for (int i = 0; i < zones; i++)
                {
                    rasterZoneCollection.Add(new RasterZone((ComRasterZone)vectorOfZones.At(i)));
                }

                return rasterZoneCollection;
            }
            catch(Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22214", ex);
            }
        }

        /// <summary>
        /// Gets the name of the matching group for the specified <see cref="Match"/>.
        /// </summary>
        /// <param name="match">The <see cref="Match"/> to get the group name for.
        /// Must be a successful <see cref="Match"/>.</param>
        /// <param name="regex">The <see cref="Regex"/> object which contains the group
        /// names.</param>
        /// <returns>The name of the matching group for the specified <see cref="Match"/>.</returns>
        /// <exception cref="ExtractException">If <see cref="Match"/> was not successful.
        /// </exception>
        static KeyValuePair<int, string> GetNameAndNumberOfMatchingGroup(Match match, Regex regex)
        {
            // Ensure the match was a successful match
            ExtractException.Assert("ELI22871", "Match must be successful to get group name!",
                match.Success);

            // Find the name of the matching group
            for (int i = 1; i < match.Groups.Count; i++)
            {
                if (match.Groups[i].Success)
                {
                    // Return the name of the successful group
                    return new KeyValuePair<int,string>(i, regex.GroupNameFromNumber(i));
                }
            }

            // Since the match was successful, there should be a group that matched
            ExtractException.ThrowLogicException("ELI22872");

            // Unreachable code to make the compiler happy
            return new KeyValuePair<int, string>();
        }

        /// <summary>
        /// Builds a new <see cref="MatchResult"/> for the specified <see cref="Match"/>.
        /// </summary>
        /// <param name="baseRule">The name of the base <see cref="IRule"/>
        /// which found this result.</param>
        /// <param name="regex">The <see cref="Regex"/> object that found this result.</param>
        /// <param name="match">The specific <see cref="Match"/> to build the
        /// <see cref="MatchResult"/> for.</param>
        /// <param name="ocrText">The OCR text that contains this match.</param>
        /// <param name="matchType">The type of <see cref="MatchResult"/> to build.</param>
        /// <returns>A new <see cref="MatchResult"/> for the specified <see cref="Match"/>; or 
        /// <see langword="null"/> if no match result was found for this match.</returns>
        static MatchResult ProcessMatch(string baseRule, Regex regex, Match match,
            SpatialString ocrText, MatchType matchType)
        {
            // Get the name and number of the matching named group
            KeyValuePair<int, string> groupNameAndNumber =
                GetNameAndNumberOfMatchingGroup(match, regex);

            string ruleText = baseRule + ": " + groupNameAndNumber.Value;
            string matchText =
                matchType == MatchType.Clue ? match.Groups[groupNameAndNumber.Key].Value : match.Value;
            List<RasterZone> rasterZones = GetRasterZonesForCapture(match.Groups[groupNameAndNumber.Key], ocrText);

            // If no raster zones were found, we are done.
            if (rasterZones == null)
            {
                return null;
            }

            return new MatchResult(rasterZones, matchType, matchText, ruleText);
        }

        #endregion Static Methods

        #region IComparable<MatchResult> Members

        /// <summary>
        /// Compares this <see cref="MatchResult"/> with another <see cref="MatchResult"/>.
        /// </summary>
        /// <param name="other">A <see cref="MatchResult"/> to compare with this
        /// <see cref="MatchResult"/>.</param>
        /// <returns>An <see cref="int"/> that indicates the relative order of the
        /// <see cref="MatchResult"/> objects that are being compared.</returns>
        public int CompareTo(MatchResult other)
        {
            // Compare the first raster zone of each match result first
            int returnVal = RasterZones[0].CompareTo(other.RasterZones[0]);

            // If the first raster zones are equal then compare the match types
            if (returnVal == 0)
            {
                returnVal = MatchType.CompareTo(other.MatchType);
            }

            // Return the compared value
            return returnVal;
        }

        /// <summary>
        /// Checks whether the specified <see cref="object"/> is equal to
        /// this <see cref="MatchResult"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with.</param>
        /// <returns><see langword="true"/> if the objects are equal and
        /// <see langword="false"/> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // Check if it is a match result object
            MatchResult result = obj as MatchResult;
            if (result == null)
            {
                return false;
            }

            // Check if they are equal
            return this == result;
        }

        /// <summary>
        /// Checks whether the specified <see cref="MatchResult"/> is equal to
        /// this <see cref="MatchResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="MatchResult"/> to compare with.</param>
        /// <returns><see langword="true"/> if the results are equal and
        /// <see langword="false"/> otherwise.</returns>
        public bool Equals(MatchResult result)
        {
            return this == result;
        }

        /// <summary>
        /// Returns a hashcode for this <see cref="MatchResult"/>.
        /// </summary>
        /// <returns>The hashcode for this <see cref="MatchResult"/>.</returns>
        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (RasterZone zone in _rasterZones)
            {
                hashCode ^= zone.GetHashCode();
            }

            return (hashCode ^ _matchType.GetHashCode());
        }

        /// <summary>
        /// Checks whether the two specified <see cref="MatchResult"/> objects
        /// are equal.
        /// </summary>
        /// <param name="result1">A <see cref="MatchResult"/> to compare.</param>
        /// <param name="result2">A <see cref="MatchResult"/> to compare.</param>
        /// <returns><see langword="true"/> if the results are equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator ==(MatchResult result1, MatchResult result2)
        {
            // If they are the same object return true
            if (ReferenceEquals(result1, result2))
            {
                return true;
            }

            // If at least one of the objects is null then they are not equal
            if (((object)result1 == null) || ((object)result2 == null))
            {
                return false;
            }

            // If they are not the same type then they are not equal
            if (result1.MatchType != result2.MatchType)
            {
                return false;
            }

            // If they do not have the same number of RasterZones they are not
            // equal
            if (result1.RasterZones.Count != result2.RasterZones.Count)
            {
                return false;
            }

            // Need to compare each RasterZone
            foreach (RasterZone zone1 in result1.RasterZones)
            {
                // See if this RasterZone matches one of the RasterZones
                // contained in the other result
                bool match = false;
                foreach (RasterZone zone2 in result2.RasterZones)
                {
                    // Found a match, set match to true and exit inner loop
                    if (zone1 == zone2)
                    {
                        match = true;
                        break;
                    }
                }

                // If one zone did not match then the results are not equal
                if (!match)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks whether the two specified <see cref="MatchResult"/> objects
        /// are not equal.
        /// </summary>
        /// <param name="result1">A <see cref="MatchResult"/> to compare.</param>
        /// <param name="result2">A <see cref="MatchResult"/> to compare.</param>
        /// <returns><see langword="true"/> if the results are not equal and
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator !=(MatchResult result1, MatchResult result2)
        {
            return !(result1 == result2);
        }

        /// <summary>
        /// Checks whether the first specified <see cref="MatchResult"/>
        /// is less than the second specified <see cref="MatchResult"/>.
        /// </summary>
        /// <param name="result1">A <see cref="MatchResult"/> to compare.</param>
        /// <param name="result2">A <see cref="MatchResult"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="result1"/> is less
        /// than <paramref name="result2"/> and <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator <(MatchResult result1, MatchResult result2)
        {
            return result1.CompareTo(result2) < 0;
        }

        /// <summary>
        /// Checks whether the first specified <see cref="MatchResult"/>
        /// is greater than the second specified <see cref="MatchResult"/>.
        /// </summary>
        /// <param name="result1">A <see cref="MatchResult"/> to compare.</param>
        /// <param name="result2">A <see cref="MatchResult"/> to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="result1"/> is greater
        /// than <paramref name="result2"/> and <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator >(MatchResult result1, MatchResult result2)
        {
            return result1.CompareTo(result2) > 0;
        }

        #endregion IComparable<MatchResult> Members
    }
}
