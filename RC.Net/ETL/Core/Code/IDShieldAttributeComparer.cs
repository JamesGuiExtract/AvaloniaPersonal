using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Extract.AttributeFinder;
using Extract.DataCaptureStats;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;


namespace Extract.ETL
{
    public static class IDShieldAttributeComparer
    {
        #region Public properties

        /// <summary>
        /// Property for the redaction area percentage that indicates an over redaction
        /// </summary>
        public static double OverRedactionERAP { get; } = 30;

        /// <summary>
        /// Property for indicating an under redaction
        /// </summary>
        public static double OverlapLeniencyPercent { get; } = 80;

        /// <summary>
        /// Minimum percent to be considered overlapping
        /// </summary>
        public static double OverlapMinimumPercent { get; } = 10.0;

        #endregion

        #region Constants

        /// <summary>
        /// Constant to indicate double zero
        /// </summary>
        const double ZERO = 1E-8;

        #endregion

        #region Internal MatchInfo Class

        /// <summary>
        /// Internal class to keep track of match information
        /// This class was patterned after the class in 
        /// C:\Engineering\ProductDevelopment\AttributeFinder\IndustrySpecific\Redaction\RedactionTester\Code\IDShieldTester.h
        /// </summary>
        internal class MatchInfo
        {
            // variables to hold the area information
            public double AreaOfExpectedRedaction;
            public double AreaOfFoundRedaction;
            public double AreaOfOverlap;

            // attributes being compared
            public IAttribute ExpectedAttribute;
            public IAttribute FoundAttribute;

            /// <summary>
            /// Default constructor
            /// </summary>
            public MatchInfo()
            {

            }

            // compute the percent of expected area that was redacted by this found attribute
            public double getPercentOfExpectedAreaRedacted()
            {
                double percent = (100.0 * (AreaOfOverlap / AreaOfExpectedRedaction));

                return percent;
            }
        };

        #endregion

        #region Public methods

        /// <summary>
        /// Compare method to compare expected and found attributes
        /// Note: This method was mostly created from analyzeExpectedAndFoundAttributes method in
        /// c:\Engineering\ProductDevelopment\AttributeFinder\IndustrySpecific\Redaction\RedactionTester\Code\IDShieldTester.cpp
        /// </summary>
        /// <param name="expected">Expected attributes</param>
        /// <param name="found">Found attributes</param>
        /// <param name="xPathOfSensitiveAttributes">XPath to select the attributes to compare</param>
        /// <returns>Dictionary that contains a List of AccuracyDetail for each page</returns>
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        public static Dictionary<Int32, List<AccuracyDetail>> CompareAttributes(IUnknownVector expected, IUnknownVector found,
            string xPathOfSensitiveAttributes, CancellationToken cancelToken)
        {
            try
            {
                // if XPathOfSensitiveAttributes is empty throw an exception
                if (string.IsNullOrWhiteSpace(xPathOfSensitiveAttributes))
                {
                    ExtractException ee = new ExtractException("ELI45396", "XPathOfSensitiveAttributes must be set.");
                    throw ee;
                }

                var foundContext = new XPathContext(found);
                var expectedContext = new XPathContext(expected);

                // Get enumerator for expected and found
                var expectedEnumerator = expectedContext
                    .FindAllOfType<IAttribute>(xPathOfSensitiveAttributes)
                    .ToList();
                var foundEnumerator = foundContext
                    .FindAllOfType<IAttribute>(xPathOfSensitiveAttributes)
                    .ToList();

                // Get the count of expected and found
                int expectedCount = expectedEnumerator.Count();
                int foundCount = foundEnumerator.Count();

                // Create list that will contain the output
                Dictionary<Int32, List<AccuracyDetail>> pageAccuracy = new Dictionary<int, List<AccuracyDetail>>();

                // if there are both expected and found attributes compare them
                if (expectedCount != 0 && foundCount != 0)
                {
                    HashSet<Int32> pagesWithExpectedOrFound = new HashSet<Int32>();

                    // initialize the matchInfo array
                    MatchInfo[,] matchInfos = new MatchInfo[expectedCount, foundCount];
                    int expectedIndex = 0;
                    foreach (var expectedAttribute in expectedEnumerator)
                    {
                        int foundIndex = 0;
                        pagesWithExpectedOrFound.Add(GetAttributePage(expectedAttribute));
                            
                        foreach (var foundAttribute in foundEnumerator)
                        {
                            cancelToken.ThrowIfCancellationRequested();

                            // Include page in hash set only need to do this the first time through
                            if (expectedIndex == 0)
                            {
                                pagesWithExpectedOrFound.Add(GetAttributePage(foundAttribute));
                            }
                            MatchInfo newMatchInfo = new MatchInfo();
                            matchInfos[expectedIndex, foundIndex] = newMatchInfo;
                            getMatchInfo(matchInfos[expectedIndex, foundIndex],
                                expectedAttribute,
                                foundAttribute);
                            foundIndex++;
                        }
                        expectedIndex++;
                    }

                    // initialize dictionary
                    foreach (Int32 page in pagesWithExpectedOrFound)
                    {
                        pageAccuracy[page] = new List<AccuracyDetail>();
                    }

                    HashSet<int> OverlappingFounds = new HashSet<int>();
                    HashSet<int> CheckedForOverRedactions = new HashSet<int>();

                    for (expectedIndex = 0; expectedIndex < expectedCount; expectedIndex++)
                    {
                        cancelToken.ThrowIfCancellationRequested();

                        bool foundCorrectRedaction = false;

                        // Add the expected count to the output
                        pageAccuracy[GetAttributePage(matchInfos[expectedIndex, 0].ExpectedAttribute)]
                            .Add(new AccuracyDetail(AccuracyDetailLabel.Expected,
                                matchInfos[expectedIndex, 0].ExpectedAttribute.Type, 1));

                        for (int foundIndex = 0; foundIndex < foundCount; foundIndex++)
                        {
                            cancelToken.ThrowIfCancellationRequested();

                            // if this the first time through the loop add the found count to the output
                            if (expectedIndex == 0)
                            {
                                pageAccuracy[GetAttributePage(matchInfos[0, foundIndex].FoundAttribute)]
                                    .Add(new AccuracyDetail(AccuracyDetailLabel.Found,
                                        matchInfos[0, foundIndex].FoundAttribute.Type, 1));
                            }

                            MatchInfo tempMatch = matchInfos[expectedIndex, foundIndex];

                            if (!(Math.Abs(tempMatch.AreaOfOverlap) < ZERO))
                            {
                                OverlappingFounds.Add(foundIndex);

                                if (tempMatch.getPercentOfExpectedAreaRedacted() < OverlapLeniencyPercent)
                                {
                                    pageAccuracy[GetAttributePage(matchInfos[0, foundIndex].FoundAttribute)]
                                        .Add(new AccuracyDetail(AccuracyDetailLabel.UnderRedacted,
                                            matchInfos[0, foundIndex].FoundAttribute.Type, 1));
                                }
                                else
                                {
                                    // The expected redaction is completely covered. Record a "correct" redaction
                                    // unless one has already been recorded for this expected redaction.
                                    if (!foundCorrectRedaction)
                                    {
                                        foundCorrectRedaction = true;
                                        pageAccuracy[GetAttributePage(matchInfos[expectedIndex, 0].ExpectedAttribute)]
                                            .Add(new AccuracyDetail(AccuracyDetailLabel.Correct,
                                                matchInfos[expectedIndex, 0].ExpectedAttribute.Type, 1));
                                    }

                                    // Look to see if we have already determined the found attribute to be an
                                    // over-redaction or not.
                                    bool checkedForOverRedaction = CheckedForOverRedactions.Contains(foundIndex);

                                    // If not, calculate whether it is an over-redaction.
                                    if (!checkedForOverRedaction)
                                    {
                                        if (IsOverredaction(matchInfos, foundIndex, expectedCount))
                                        {
                                            // Record an over-redaction.
                                            pageAccuracy[GetAttributePage(matchInfos[0, foundIndex].FoundAttribute)]
                                                .Add(new AccuracyDetail(AccuracyDetailLabel.OverRedacted,
                                                    matchInfos[0, foundIndex].FoundAttribute.Type, 1));
                                        }

                                        // Update the set of checked attributes
                                        CheckedForOverRedactions.Add(foundIndex);
                                    }
                                }
                            }
                        }

                        // if expected wasn't found count as missed
                        if (!foundCorrectRedaction)
                        {
                            pageAccuracy[GetAttributePage(matchInfos[expectedIndex, 0].ExpectedAttribute)]
                                .Add(new AccuracyDetail(AccuracyDetailLabel.Missed,
                                    matchInfos[expectedIndex, 0].ExpectedAttribute.Type, 1));
                        }
                    }

                    //--------------------------------------------------------
                    // Record all false positives
                    //--------------------------------------------------------
                    for (int foundIndex = 0; foundIndex < foundCount; foundIndex++)
                    {
                        cancelToken.ThrowIfCancellationRequested();

                        // A found redaction is a false positive if it was not already found to overlap an
                        // expected redaction.
                        if (!OverlappingFounds.Contains(foundIndex))
                        {
                            pageAccuracy[GetAttributePage(matchInfos[0, foundIndex].FoundAttribute)]
                                .Add(new AccuracyDetail(AccuracyDetailLabel.FalsePositives,
                                    matchInfos[0, foundIndex].FoundAttribute.Type, 1));
                        }
                    }
                }
                else if (expectedCount == 0 && foundCount == 0)
                {
                    // Ensure at least one row is created for every comparison of two files. Otherwise these files
                    // will not be included in dashboards.
                    pageAccuracy[1] = new[] { new AccuracyDetail(AccuracyDetailLabel.Expected, "", 0) }.ToList();
                }
                else
                {
                    // Add any expected attribute counts as missed
                    foreach (var expectedAttribute in expectedEnumerator)
                    {
                        int page = GetAttributePage(expectedAttribute);
                        if (!pageAccuracy.ContainsKey(page))
                        {
                            pageAccuracy[page] = new List<AccuracyDetail>();
                        }
                        pageAccuracy[page].Add(new AccuracyDetail(AccuracyDetailLabel.Expected,
                                expectedAttribute.Type, 1));
                        pageAccuracy[page].Add(new AccuracyDetail(AccuracyDetailLabel.Missed,
                                expectedAttribute.Type, 1));
                    }

                    // Add any found attributes as false positives
                    foreach (var foundAttribute in foundEnumerator)
                    {
                        int page = GetAttributePage(foundAttribute);
                        if (!pageAccuracy.ContainsKey(page))
                        {
                            pageAccuracy[page] = new List<AccuracyDetail>();
                        }
                        pageAccuracy[page].Add(new AccuracyDetail(AccuracyDetailLabel.Found,
                            foundAttribute.Type, 1));

                        pageAccuracy[page].Add(new AccuracyDetail(AccuracyDetailLabel.FalsePositives,
                            foundAttribute.Type, 1));
                    }
                }

                return pageAccuracy;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45398");
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Calculates the total area for the attribute
        /// Based on getTotalArea method in
        /// c:\Engineering\ProductDevelopment\AttributeFinder\IndustrySpecific\Redaction\RedactionTester\Code\IDShieldTester.cpp
        /// </summary>
        /// <param name="attribute">Attribute to calculate total area of</param>
        /// <returns>total area</returns>
        static double getTotalArea(IAttribute attribute)
        {
            // default total area to 0
            double area = 0.0;

            // get the spatial string for this attribute
            ISpatialString value = attribute.Value;

            // check for spatial info
            if (value.HasSpatialInfo())
            {
                // get the vector of raster zones
                IIUnknownVector vectorOfRasterZones = value.GetOriginalImageRasterZones();

                // for each zone, get its area and add that to the total area
                int lSize = vectorOfRasterZones.Size();
                for (int i = 0; i < lSize; i++)
                {
                    IRasterZone rasterZone = vectorOfRasterZones.At(i) as IRasterZone;

                    area += (double)rasterZone.Area;
                }
            }

            // return the computed area (0 if no spatial info)
            return area;
        }

        /// <summary>
        /// Initializes the values for the given matchInfo for the expected and found attributes
        /// Based on getMatchInfo method in
        /// c:\Engineering\ProductDevelopment\AttributeFinder\IndustrySpecific\Redaction\RedactionTester\Code\IDShieldTester.cpp
        /// </summary>
        /// <param name="matchInfo">MatchInfo that is being initialized</param>
        /// <param name="expected">expected attribute</param>
        /// <param name="found">found attribute</param>
        static void getMatchInfo(MatchInfo matchInfo, IAttribute expected,
                                        IAttribute found)
        {
            // store the expected and found attribute pointers
            matchInfo.ExpectedAttribute = expected;
            matchInfo.FoundAttribute = found;

            // get the total area of each attribute
            matchInfo.AreaOfExpectedRedaction = getTotalArea(expected);
            matchInfo.AreaOfFoundRedaction = getTotalArea(found);

            // get the area of overlap
            matchInfo.AreaOfOverlap = computeTotalAreaOfOverlap(expected, found);
        }

        /// <summary>
        /// Computes the total area of overlap between expected and found
        /// Based on computeTotalAreaOfOverlap method in
        /// c:\Engineering\ProductDevelopment\AttributeFinder\IndustrySpecific\Redaction\RedactionTester\Code\IDShieldTester.cpp
        /// </summary>
        /// <param name="expected">expected attribute</param>
        /// <param name="found">found attribute</param>
        /// <returns>Total area of overlap between expected and found</returns>
        static double computeTotalAreaOfOverlap(IAttribute expected, IAttribute found)
        {

            // get the spatial strings for each attribute
            ISpatialString expectedValue = expected.Value;

            ISpatialString foundValue = found.Value;

            // default the overlap area to 0
            double areaOfOverlap = 0.0;

            // Keeps track of whether any raster zone overlaps by m_dOverlapMinimumPercent.
            bool attributesOverlap = false;

            // make sure both strings are spatial
            if (expectedValue.HasSpatialInfo() && foundValue.HasSpatialInfo())
            {
                // get the vector of raster zones for both spatial strings
                IUnknownVector expectedRasterZones = expectedValue.GetOriginalImageRasterZones();

                IUnknownVector foundRasterZones = foundValue.GetOriginalImageRasterZones();

                // loop through each of the raster zones and add the area of overlap
                int ExpectedSize = expectedRasterZones.Size();
                int FoundSize = foundRasterZones.Size();
                for (int i = 0; i < ExpectedSize; i++)
                {
                    RasterZone expectedRasterZone = expectedRasterZones.At(i) as RasterZone;

                    for (int j = 0; j < FoundSize; j++)
                    {
                        RasterZone foundRasterZone = foundRasterZones.At(j) as RasterZone;

                        double overlap = expectedRasterZone.GetAreaOverlappingWith(foundRasterZone);

                        if (!(Math.Abs(overlap) < ZERO))
                        {
                            // add the area of overlap (GetArea checks page number of zone)
                            areaOfOverlap += overlap;

                            if (!attributesOverlap)
                            {
                                double overlapPercent = (overlap / Math.Min(expectedRasterZone.Area, foundRasterZone.Area)) * 100;

                                // [FlexIDSCore:4104] Ensure at least one raster zone overlaps by
                                // m_dOverlapMinimumPercent before allowing any overlap to be reported.
                                if (overlapPercent >= OverlapMinimumPercent)
                                {
                                    attributesOverlap = true;
                                }
                            }
                        }
                    }
                }
            }

            return (attributesOverlap ? areaOfOverlap : 0);
        }

        /// <summary>
        /// Checks if the found attribute is an over redaction
        /// Based on getIsOverredaction method in
        /// c:\Engineering\ProductDevelopment\AttributeFinder\IndustrySpecific\Redaction\RedactionTester\Code\IDShieldTester.cpp
        /// </summary>
        /// <param name="matchInfos">Array of MatchInfos</param>
        /// <param name="foundIndex">Index of found match</param>
        /// <param name="totalExpected">The total number of expected found</param>
        /// <returns>true if over redaction, false otherwise</returns>
        static bool IsOverredaction(MatchInfo[,] matchInfos, int foundIndex, int totalExpected)
        {
            // Iterate through all expected redactions to determine the total area of overlap with the
            // specified found redaction.
            double totalOverlapArea = 0.0;
            for (int expectedIndex = 0; expectedIndex < totalExpected; expectedIndex++)
            {
                // get the match info for these two attributes and add the area of
                // overlap
                MatchInfo overlapMatchInfo = matchInfos[expectedIndex, foundIndex];
                totalOverlapArea += overlapMatchInfo.AreaOfOverlap;
            }

            // get the area of the found redaction
            double areaOfFoundRedaction =
                matchInfos[0, foundIndex].AreaOfFoundRedaction;

            double dERAP = 0.0;

            // protect against divide by zero
            if (!(Math.Abs(areaOfFoundRedaction) < ZERO))
            {
                // compute the excess redaction area percentage
                dERAP = (areaOfFoundRedaction - totalOverlapArea) /
                    areaOfFoundRedaction;
                dERAP *= 100.0;

                // Don't get the absolute value
                // https://extract.atlassian.net/browse/ISSUE-12617
                //    // get absolute value
                //    dERAP = Math.Abs(dERAP);

                if (dERAP >= OverRedactionERAP)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Gets the first page number for the given attribute
        /// </summary>
        /// <param name="attribute">Attribute to get the first page number for</param>
        /// <returns>First page number of the attribute or if the string is non spatial returns 0</returns>
        static Int32 GetAttributePage(IAttribute attribute)
        {
            return (attribute.Value.GetMode() == ESpatialStringMode.kNonSpatialMode) ?
                                0 : attribute.Value.GetFirstPageNumber();
        }


        #endregion
    }
}
