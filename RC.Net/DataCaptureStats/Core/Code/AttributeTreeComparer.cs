using Extract.AttributeFinder;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.DataCaptureStats
{
    /// <summary>
    /// Class to handle comparing attribute trees (VOAs)
    /// </summary>
    public static class AttributeTreeComparer
    {
        #region Constants

        // By default ignore any empty attributes with only empty children (or no children)
        public const string DefaultIgnoreXPath = "/*//*[not(.//text())]";
        public const string DefaultContainerXPath = "/*//*[not(text()) or text()='N/A']";

        private static readonly IEqualityComparer<string> StringComparer = System.StringComparer.OrdinalIgnoreCase;
        private static readonly StringComparison StringComparison = System.StringComparison.OrdinalIgnoreCase;

        #endregion Constants

        static ThreadLocal<MiscUtils> _miscUtils = new ThreadLocal<MiscUtils>(() => new MiscUtilsClass());


        /// <summary>
        /// Compares one set of attributes with another. The per-file, map function of the design
        /// </summary>
        /// <param name="ignoreXPath">The XPath to select attributes to ignore.</param>
        /// <param name="containerXPath">The XPath to select attributes that will be considered as containers only</param>
        /// <remarks><see paramref="found"/> and <see paramref="expected"/> hierarchies may be modified
        /// by this method.</remarks>
        /// <param name="collectMatchData">Whether to collect each correct/incorrect/missed attribute in addition to counts</param>
        /// <param name="cancelToken">CancellationToken to allow cancellation of comparison</param>
        public static IEnumerable<AccuracyDetail> CompareAttributes(IUnknownVector expected, IUnknownVector found,
            string ignoreXPath = DefaultIgnoreXPath,
            string containerXPath = DefaultContainerXPath,
            bool collectMatchData = true,
            CancellationToken cancelToken = default(CancellationToken))
        {
            try
            {
                var containerAttributes = new HashSet<IAttribute>();

                if (!string.IsNullOrWhiteSpace(ignoreXPath)
                    || !string.IsNullOrWhiteSpace(containerXPath))
                {
                    var foundContext = new XPathContext(found);
                    var expectedContext = new XPathContext(expected);

                    if (!string.IsNullOrWhiteSpace(ignoreXPath))
                    {
                        foundContext.RemoveMatchingAttributes(ignoreXPath);
                        expectedContext.RemoveMatchingAttributes(ignoreXPath);
                    }

                    if (!string.IsNullOrWhiteSpace(containerXPath))
                    {
                        var containers = foundContext.FindAllOfType<IAttribute>(containerXPath)
                            .Concat(expectedContext.FindAllOfType<IAttribute>(containerXPath));
                        foreach (var attr in containers)
                        {
                            containerAttributes.Add(attr);
                        }
                    }
                }

                return CompareAttributesAfterXPathModifications(expected, found, containerAttributes, collectMatchData, cancelToken);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41504");
            }
        }


        //-------------------------------------------------------------------------------------------------
        // Internal classes
        //-------------------------------------------------------------------------------------------------

        /// <summary>
        /// To store data from computing the "matching" score between an expected and a found
        /// attribute.  This data is important in allowing for attributes to be found in any order
        /// and computing which attribute is a best match to which other attribute without depending
        /// on the order they appear in the file (NOTE - To break a tie between matching scores, the
        /// order an item appears in a file will be used i.e. if found attribute 2 and 3 have the
        /// same "best match score" when compared against expected attribute 3, found attribute 3 will
        /// be the preferred attribute because it lines up horizontally with the expected attribute).        
        /// </summary>
        private class AttributeScoreData : IComparable<AttributeScoreData>
        {
            // Attribute match score
            public int Score;

            // Whether this pair is still valid
            // (can be marked invalid due to not a match or
            // if either the expected or found attribute has
            // been used)
            public bool Valid;

            // Expected attribute index
            public int ExpectedIndex;

            // Found attribute index
            public int FoundIndex;

            /// <summary>
            /// Compares the current instance with another object of the same type and returns an integer
            /// that indicates whether the current instance precedes, follows, or occurs in the same
            /// position in the sort order as the other object.
            /// </summary>
            /// <remarks>
            /// The primary factor in comparison is whether the match is valid or not and how
            /// much the found matches the expected. As a tie-breaker, the correspondence between expected
            /// and found indexes and if neither correspond then the expected indexes are compared, earlier
            /// considered greater than later.
            /// </remarks>
            /// <param name="other">An object to compare with this instance.</param>
            /// <returns>
            /// A value that indicates the relative order of the objects being compared. The return value
            /// has these meanings: Value Meaning Less than zero This instance precedes <paramref name="other" />
            /// in the sort order.  Zero This instance occurs in the same position in the sort order as
            /// <paramref name="other" />. Greater than zero This instance follows <paramref name="other" /> in the sort order.
            /// </returns>
            public int CompareTo(AttributeScoreData other)
            {
                // If both are valid then check whether they have the same score or not
                if (Valid && other.Valid)
                {
                    // Different scores, return this score compared to other score
                    if (Score != other.Score)
                    {
                        return Score.CompareTo(other.Score);
                    }
                    else
                    {
                        // Same score check whether the index for other is the same
                        if (ExpectedIndex == FoundIndex)
                        {
                            // Now check if other has the same index
                            if (other.ExpectedIndex == other.FoundIndex)
                            {
                                // return opposite of this expected index compared to other expected index
                                // (consider earlier match to be greater)
                                return -1 * ExpectedIndex.CompareTo(other.ExpectedIndex);
                            }
                            else
                            {
                                // This matched its index but other didn't, prefer matched index
                                return 1;
                            }
                        }
                        // Now check if other has the same index
                        else if (other.ExpectedIndex == other.FoundIndex)
                        {
                            // Prefer matching index of other
                            return -1;
                        }
                        // Neither this nor other found indexes match expected
                        else
                        {
                            // return opposite of this expected index compared to other expected index
                            // (consider earlier match to be greater)
                            return -1 * ExpectedIndex.CompareTo(other.ExpectedIndex);
                        }
                    }
                }
                // Return 1 if this is valid, else -1 if other is valid else 0
                else
                {
                    return Valid ? 1 : other.Valid ? -1 : 0;
                }
            }
        }

        /// <summary>
        /// Pair of bools to mark whether an index is a match and if so the best match
        /// </summary>
        private struct MatchedBestMatch
        {
            /// <summary>
            /// Gets or sets a value indicating whether this is matched.
            /// </summary>
            public bool Matched;

            /// <summary>
            /// Gets or sets a value indicating whether this is the best match
            /// </summary>
            public bool BestMatch;
        }

        /// <summary>
        /// Pair of int to bool to hold the score and whether a (partial) match or not
        /// </summary>
        private struct ScoreAndMatch
        {
            /// <summary>
            /// Gets or sets a value indicating whether this is matched.
            /// </summary>
            public int Score;

            /// <summary>
            /// Gets or sets a value indicating whether this is the best match
            /// </summary>
            public bool Matched;
        }

        /// <summary>
        /// Builds a new fully qualified path for an attribute by adding to the path of its parent
        /// </summary>
        /// <param name="attribute">The attribute to build a name for.</param>
        /// <param name="parentPath">Path of the parent attribute.</param>
        /// <returns>The full path to this attribute based on the given parent path</returns>
        private static string GetQualifiedName(IAttribute attribute, string parentPath)
        {
            string nameAndType = attribute.Name;
            string type = attribute.Type;
            if (type.Length != 0)
            {
                nameAndType += "@" + type;
            }
            string separator = string.IsNullOrEmpty(parentPath) ? "" : "/";
            return parentPath + separator + nameAndType;
        }

        /// <summary>
        /// For an attribute pair, computes the score and whether it is a (partial) match or not.
        /// </summary>
        /// <param name="expected">The expected attribute.</param>
        /// <param name="found">The found attribute.</param>
        /// <param name="containerAttributes">The attributes that matched the container-only XPath pattern</param>
        /// <returns>A score and whether the top-level is a match or not</returns>
        private static ScoreAndMatch ComputeScore(IAttribute expected, IAttribute found,
            HashSet<IAttribute> containerAttributes)
        {
            try
            {
                var scoreAndMatch = new ScoreAndMatch();

                // Check if the attributes match (compare Name, Value, Type)
                // Disregard Values if both attributes were selected as container-only
                if (expected.Name.Equals(found.Name, StringComparison)
                    && expected.Type.Equals(found.Type, StringComparison)
                    && (expected.Value.String.Equals(found.Value.String, StringComparison)
                        || containerAttributes.Contains(expected) && containerAttributes.Contains(found)
                        ))
                {
                    // Attributes match, increment score and set matched to true
                    scoreAndMatch.Score++;
                    scoreAndMatch.Matched = true;

                    // ---------------------------------------
                    // Compute the score of the sub-attributes
                    // ---------------------------------------

                    // Get the sub attributes
                    IIUnknownVector expectedSubs = expected.SubAttributes;
                    IIUnknownVector foundSubs = found.SubAttributes;

                    // Create array to store whether an Attribute was a match
                    // and/or was a best match:
                    // Also create a set to store the index of a found item that was considered
                    // a best match so that we can short circuit comparison on that attribute later
                    var foundMatchBestMatchData = new MatchedBestMatch[foundSubs.Size()];
                    HashSet<int> bestMatchesSet = new HashSet<int>();

                    // Iterate through the sub attributes and compute score
                    // Holds best scores for a particular match
                    var bestScoresIndexToScore = new Dictionary<int, int>();
                    int foundSize = foundSubs.Size();
                    foreach (var expectedSub in expectedSubs.ToIEnumerable<IAttribute>())
                    {
                        // Compute the score and match value for each found attribute
                        var scores = new ScoreAndMatch[foundSize];
                        for (int foundIndex = 0; foundIndex < foundSize; foundIndex++)
                        {
                            // Skip any item that was already marked as a best match
                            if (bestMatchesSet.Contains(foundIndex))
                            {
                                continue;
                            }

                            // Get the found attribute
                            IAttribute foundSub = (IAttribute)foundSubs.At(foundIndex);

                            // Compute the score and store the computed score in the
                            // score vector
                            var subScoreAndMatch = ComputeScore(expectedSub, foundSub, containerAttributes);
                            scores[foundIndex] = subScoreAndMatch;

                            // If this was a match, mark it as such 
                            if (subScoreAndMatch.Matched)
                            {
                                foundMatchBestMatchData[foundIndex].Matched = true;
                            }
                        }

                        // Look through all the computed scores and find the best score
                        // NOTE: The best score may be a negative number
                        int bestScoreIndex = -1;
                        int bestScore = 0;
                        for (int foundIndex = 0; foundIndex < scores.Length; foundIndex++)
                        {
                            // Check if the score is from a "successful" match
                            if (scores[foundIndex].Matched)
                            {
                                // If the best score index has not been set yet, or the new score
                                // is better than the previous best score then set the new best
                                // score and index
                                if (bestScoreIndex == -1 || scores[foundIndex].Score > bestScore)
                                {
                                    bestScoreIndex = foundIndex;
                                    bestScore = scores[foundIndex].Score;
                                }
                            }
                        }

                        // Check for a best score found
                        if (bestScoreIndex != -1)
                        {
                            // Mark this attribute as a best match
                            foundMatchBestMatchData[bestScoreIndex].BestMatch = true;

                            // Store the score
                            bestScoresIndexToScore[bestScoreIndex] = bestScore;

                            // Add this index to the set of best matches (this will
                            // allow short circuit of comparison for this attribute)
                            bestMatchesSet.Add(bestScoreIndex);
                        }
                    }

                    // Compute the score
                    scoreAndMatch.Score += foundMatchBestMatchData.Select((matchData, foundIndex) =>
                        matchData.BestMatch
                        ? bestScoresIndexToScore[foundIndex]
                        : // False positive if not a best match. Score is -1 * AttributeSize
                          -1 * ((IAttribute)foundSubs.At(foundIndex)).EnumerateDepthFirst().Count()
                    ).Sum();
                }

                // Return the pair containing the computed score and whether this was a match or not
                return scoreAndMatch;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41501");
            }
        }

        /// <summary>
        /// Adds to the count of the path in the <see paramref="pathsToCounts"/> dictionary
        /// that corresponds to the path of each of <see paramref="attributes"/>.
        /// Optionally collects the attributes with their paths.
        /// </summary>
        /// <param name="attributes">The attributes to count.</param>
        /// <param name="pathsToCounts">If non-null, will be updated with the counts of attribute paths</param>
        /// <param name="pathAndAttribute">If non-null, will be filled with correct/incorrect/missed attributes with their paths</param>
        /// <param name="qualifiedAncestorName">The fully qualified name of the ancestor
        /// of these attributes.</param>
        /// <param name="containerOnlyPaths">The set of container only attribute paths to be updated
        /// based on these attributes.</param>
        /// <param name="containerAttributes">The attributes that matched the container-only XPath pattern</param>
        /// <param name="recursivelyCount">if set to <c>true</c> recursively count subattributes.</param>
        /// <param name="recursivelyCollect">if set to <c>true</c> recursively collect attributes.</param>
        private static void CountAttributes(
            IEnumerable<IAttribute> attributes,
            Dictionary<string, int> pathsToCounts,
            List<(string, Func<string>)> pathAndAttribute,
            string qualifiedAncestorName,
            Dictionary<string, bool> containerOnlyPaths,
            HashSet<IAttribute> containerAttributes,
            bool recursivelyCount,
            bool recursivelyCollect)
        {
            ExtractException.Assert("ELI46828", UtilityMethods.FormatInvariant(
                $"It is invalid to specify {nameof(recursivelyCollect)}=true when {nameof(recursivelyCount)}=false"),
                !recursivelyCollect || recursivelyCount);

            foreach (var attribute in attributes)
            {
                string qualifiedName = GetQualifiedName(attribute, qualifiedAncestorName);

                if (pathsToCounts != null)
                {
                    int count = pathsToCounts.GetOrAdd(qualifiedName, _ => 0);
                    pathsToCounts[qualifiedName] = count + 1;
                }

                if (pathAndAttribute != null)
                {
                    pathAndAttribute.Add((qualifiedName, () => _miscUtils.Value.GetObjectAsStringizedByteStream(attribute)));
                }
                
                // Update the container-only status. Don't overwrite a false value with a true value
                if (!containerAttributes.Contains(attribute))
                {
                    containerOnlyPaths[qualifiedName] = false;
                }
                else if (!containerOnlyPaths.ContainsKey(qualifiedName))
                {
                    containerOnlyPaths[qualifiedName] = true;
                }

                // Count all the subattributes recursively if specified
                if (recursivelyCount)
                {
                    CountAttributes(attribute.SubAttributes.ToIEnumerable<IAttribute>(), pathsToCounts,
                        recursivelyCollect ? pathAndAttribute : null,
                        qualifiedName, containerOnlyPaths, containerAttributes, true, recursivelyCollect);
                }
            }
        }

        /// <summary>
        /// Compares the found against the expected attributes.
        /// </summary>
        /// <remarks>Only <see cref="AccuracyDetailLabel.ContainerOnly"/> items will ever be returned with zero values.</remarks>
        /// <param name="topLevelExpected">The expected attributes.</param>
        /// <param name="topLevelFound">The found attributes.</param>
        /// <param name="containerAttributes">The attributes that matched the container-only XPath pattern</param>
        /// <param name="collectMatchData">Whether to collect each correct/incorrect/missed attribute in addition to counts</param>
        /// <param name="cancelToken">CancellationToken to allow cancellation of comparison</param>
        /// <returns>An enumeration of <see cref="AccuracyDetail"/> items representing the result of the comparison</returns>
        private static IEnumerable<AccuracyDetail> CompareAttributesAfterXPathModifications(
            IUnknownVector topLevelExpected, IUnknownVector topLevelFound, HashSet<IAttribute> containerAttributes,
            bool collectMatchData,
            CancellationToken cancelToken)
        {
            ExtractException.Assert("ELI41502", "Unable to compare Attributes.",
                topLevelFound != null && topLevelExpected != null);

            // Dictionaries to hold counts for each path encountered
            var totalExpected = new Dictionary<string, int>(StringComparer);
            var totalCorrectFound = new Dictionary<string, int>(StringComparer);
            var totalIncorrectFound = new Dictionary<string, int>(StringComparer);
            var containerOnly = new Dictionary<string, bool>(StringComparer);

            // Lists to hold attributes to save for investigational purposes
            List<(string, Func<string>)> listOfMissed = null;
            List<(string, Func<string>)> listOfCorrectFound = null;
            List<(string, Func<string>)> listOfIncorrectFound = null;
            if (collectMatchData)
            {
                listOfMissed = new List<(string, Func<string>)>();
                listOfCorrectFound = new List<(string, Func<string>)>();
                listOfIncorrectFound = new List<(string, Func<string>)>();
            }

            #region Function Definitions

            // These functions increment counts in the paths-to-counts dictionaries,
            // update the dictionary of paths-to-container-only? status, and
            // collect attributes for investigational purposes

            // Recursively count expected paths
            void countExpected(IUnknownVector expected, string qualifiedAncestorName) =>
                CountAttributes(expected.ToIEnumerable<IAttribute>(), totalExpected, null,
                                qualifiedAncestorName, containerOnly, containerAttributes, true, false);

            // Recursively count found paths of attribute enumeration as incorrect
            void countAttributesAsIncorrectlyFound(IEnumerable<IAttribute> found, string qualifiedAncestorName) =>
                CountAttributes(found, totalIncorrectFound, listOfIncorrectFound,
                qualifiedAncestorName, containerOnly, containerAttributes, true, false);

            // Recursively count found paths of VOA as incorrect
            void countVoaAsIncorrectlyFound(IUnknownVector found, string qualifiedAncestorName) =>
                CountAttributes(found.ToIEnumerable<IAttribute>(), totalIncorrectFound, listOfIncorrectFound,
                                qualifiedAncestorName, containerOnly, containerAttributes, true, false);

            // Count an attribute as correctly found
            void countAttributeAsCorrectlyFound(IAttribute found, string qualifiedAncestorName) =>
                CountAttributes(Enumerable.Repeat(found, 1), totalCorrectFound, listOfCorrectFound,
                qualifiedAncestorName, containerOnly, containerAttributes, false, false);

            // Collect an attribute as missed
            void collectAttributesAsMissed(IEnumerable<IAttribute> expected, string qualifiedAncestorName) =>
                CountAttributes(expected, null, listOfMissed,
                qualifiedAncestorName, containerOnly, containerAttributes, false, false);

            // Compares the found against the expected attributes
            void internalCompareAttributes(IUnknownVector expected, IUnknownVector found, string qualifiedAncestorName)
            {
                try
                {
                    // if nothing found then anything expected is missed (already counted but not collected)
                    if (found.Size() == 0)
                    {
                        collectAttributesAsMissed(expected.ToIEnumerable<IAttribute>(), qualifiedAncestorName);
                        return;
                    }

                    // if nothing expected then anything found is incorrect
                    if (expected.Size() == 0)
                    {
                        countVoaAsIncorrectlyFound(found, qualifiedAncestorName);
                        return;
                    }

                    // Compute match score for all expected vs. found attributes
                    // and store the data in a list (the attribute score data structure
                    // holds the score, the expected attribute index and the found attribute index.
                    List<AttributeScoreData> attributeScores =
                        expected.ToIEnumerable<IAttribute>().SelectMany((expectedAttribute, expectedIndex) =>
                            found.ToIEnumerable<IAttribute>().Select((foundAttribute, foundIndex) =>
                            {
                                cancelToken.ThrowIfCancellationRequested();

                                // Compute the score for this attribute pair
                                var score = ComputeScore(expectedAttribute, foundAttribute, containerAttributes);

                                // Build an attribute data structure
                                return new AttributeScoreData
                                {
                                    Score = score.Score,
                                    Valid = score.Matched,
                                    ExpectedIndex = expectedIndex,
                                    FoundIndex = foundIndex
                                };
                            }))
                        .Where(a => a.Valid)
                        .OrderBy(a => a)
                        .ToList();

                    // Track whether a found attribute has been used or not.
                    // Unused found attributes will be counted as incorrect
                    var foundAttributeUsedState = new bool[found.Size()];

                    var expectedAttributeUsedState = new bool[expected.Size()];

                    // Loop while there is an item in the list.
                    // Because the list is sorted, the last item in the list
                    // should be the best match out of all expected vs. found
                    // match comparisons. We use this item to perform the comparison,
                    // then remove all items that refer to the same found or expected
                    // attributes and repeat.
                    AttributeScoreData bestMatch;
                    while ((bestMatch = attributeScores.LastOrDefault()) != null)
                    {
                        cancelToken.ThrowIfCancellationRequested();

                        // Mark the found and expected attributes as used
                        foundAttributeUsedState[bestMatch.FoundIndex] = true;
                        expectedAttributeUsedState[bestMatch.ExpectedIndex] = true;

                        // Get the attributes from the expected and found lists
                        var e = (IAttribute)expected.At(bestMatch.ExpectedIndex);
                        var f = (IAttribute)found.At(bestMatch.FoundIndex);

                        // Since there was a best match, mark this attribute as correctly found
                        countAttributeAsCorrectlyFound(f, qualifiedAncestorName);

                        // Recurse: compare all the sub attributes
                        internalCompareAttributes(e.SubAttributes, f.SubAttributes,
                            GetQualifiedName(f, qualifiedAncestorName));

                        // Filter-out all attribute score entries that have either the same expected or found index
                        attributeScores = attributeScores.Where(scoreData =>
                            scoreData.ExpectedIndex != bestMatch.ExpectedIndex
                            && scoreData.FoundIndex != bestMatch.FoundIndex).ToList();
                    }

                    // Count each unused found attribute as incorrect
                    var unused = Enumerable.Range(0, foundAttributeUsedState.Length)
                        .Where(i => !foundAttributeUsedState[i])
                        .Select(i => (IAttribute)found.At(i));

                    countAttributesAsIncorrectlyFound(unused, qualifiedAncestorName);

                    // Count each unused expected attribute as missed
                    var unusedExpected = Enumerable.Range(0, expectedAttributeUsedState.Length)
                        .Where(i => !expectedAttributeUsedState[i])
                        .Select(i => (IAttribute)expected.At(i));

                    collectAttributesAsMissed(unusedExpected, qualifiedAncestorName);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41503");
                }
            }

            #endregion Function Definitions

            // Count expected
            countExpected(topLevelExpected, "");

            // Compare found against expected
            internalCompareAttributes(topLevelExpected, topLevelFound, "");

            var containerOnlySet = new HashSet<string>(containerOnly
                .Where(pathStatusPair => pathStatusPair.Value)
                .Select(pathStatusPair => pathStatusPair.Key), StringComparer);

            // Make accuracy detail items out of each collection. Don't include container-only items for any other label
            var result = containerOnlySet.Select(path => new AccuracyDetail(AccuracyDetailLabel.ContainerOnly, path, 0))
            .Concat(totalExpected
                .Where(pathCountPair => !containerOnlySet.Contains(pathCountPair.Key))
                .Select(pathCountPair => new AccuracyDetail(AccuracyDetailLabel.Expected, pathCountPair.Key, pathCountPair.Value)))
            .Concat(totalCorrectFound
                .Where(pathCountPair => !containerOnlySet.Contains(pathCountPair.Key))
                .Select(pathCountPair => new AccuracyDetail(AccuracyDetailLabel.Correct, pathCountPair.Key, pathCountPair.Value)))
            .Concat(totalIncorrectFound
                .Where(pathCountPair => !containerOnlySet.Contains(pathCountPair.Key))
                .Select(pathCountPair => new AccuracyDetail(AccuracyDetailLabel.Incorrect, pathCountPair.Key, pathCountPair.Value)));

            if (collectMatchData)
            {
                result = result
                .Concat(listOfCorrectFound
                    .Where(pathCollectionPair => !containerOnlySet.Contains(pathCollectionPair.Item1))
                    .Select(x => new AccuracyDetail(AccuracyDetailLabel.Correct, x.Item1, x.Item2())))
                .Concat(listOfIncorrectFound
                    .Where(pathCollectionPair => !containerOnlySet.Contains(pathCollectionPair.Item1))
                    .Select(x => new AccuracyDetail(AccuracyDetailLabel.Incorrect, x.Item1, x.Item2())))
                .Concat(listOfMissed
                    .Where(pathCollectionPair => !containerOnlySet.Contains(pathCollectionPair.Item1))
                    .Select(x => new AccuracyDetail(AccuracyDetailLabel.Missed, x.Item1, x.Item2())));
            }

            return result;
        }
    }
}