using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_AFCORELib;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// A helper class for LabOrder that can be used to ensure all mandatory tests for an order
    /// exist or to best map the available unmapped tests into the order.
    /// </summary>
    internal class TestMapper
    {
        #region Fields

        /// <summary>
        /// The set of mandatory test codes for the target <see cref="LabOrder"/>.
        /// </summary>
        HashSet<string> _mandatoryTestCodes;

        /// <summary>
        /// The set of all other test codes for the target <see cref="LabOrder"/>
        /// </summary>
        HashSet<string> _otherTestCodes;

        /// <summary>
        /// Keeps track of the test codes that have already been mapped and, thus, should not be
        /// considered for any additional tests.
        /// </summary>
        HashSet<string> _mappedTestCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Keeps track of attributes that have been iterated by PopNextNodeTests
        /// </summary>
        HashSet<IAttribute> _visitedTestAttributes = new HashSet<IAttribute>();

        /// <summary>
        /// Keeps track of the test attributes that have already been mapped and, thus, should not be
        /// considered for any additional tests.
        /// </summary>
        HashSet<IAttribute> _mappedTestAttributes = new HashSet<IAttribute>();

        /// <summary>
        /// A mapping of test attributes to possible tests
        /// </summary>
        Dictionary<IAttribute, List<LabTest>> _attributeToCandidateTests =
            new Dictionary<IAttribute, List<LabTest>>();

        /// <summary>
        /// Keeps track of the total number of open <see cref="TestMapper"/> instances as part of
        /// the overall mapping attempt. If this number exceeds _COMBINATION_ALGORITHM_SAFETY_CUTOFF
        /// it will stop searching any new permutations.
        /// HACK: This is an array with one and only value so the value can be accessed by reference.
        /// </summary>
        int[] _totalOpenInstances = new int[] { 0 };

        /// <summary>
        /// The sample type (Blood/Urine) of this collection of mappings
        /// </summary>
        string _sampleType;

        #endregion Fields

        #region Static Methods

        /// <summary>
        /// Searches <see paramref="possibleMappings"/> for the best mapping possible.
        /// </summary>
        /// <param name="possibleMappings">A <see cref="TestMapper"/> instance describing the set
        /// of possible mappings of test names into test codes.</param>
        /// <param name="requireMandatory"><see langword="true"/> if mandatory tests are required.</param>
        /// <param name="finalPass">Whether this is the final mapping pass</param>
        /// <Returns>A list of <see cref="LabTest"/>s that have been mapped into the test codes</Returns>
        public static List<LabTest> FindMapping(TestMapper possibleMappings, bool requireMandatory,
            bool finalPass)
        {
            IEnumerable<string> possibleSampleTypes = null;

            // If this is the final mapping pass or there are not multiple sample types in this order
            // (e.g., this is not a single-bucket customer), then sample type is not important
            if (finalPass || (possibleSampleTypes = possibleMappings.GetPossibleSampleTypes()).Count() < 2)
            {
                return FindMapping(possibleMappings, requireMandatory);
            }
            
            // Else try limiting choices to each possible sample type and keep the best result
            List<LabTest> bestResult = null;
            foreach (string sampleType in possibleSampleTypes
                .OrderBy(sampleType => sampleType == OrderMappingDBCache._BLOOD_SAMPLE_TYPE ? 0
                             : sampleType == OrderMappingDBCache._URINE_SAMPLE_TYPE ? 1 : 2 ))
            {
                var possibleMappingsForSampleType = new TestMapper(possibleMappings);
                possibleMappingsForSampleType._sampleType = sampleType;
                List<LabTest> result = FindMapping(possibleMappingsForSampleType, requireMandatory);

                // Update the best result if this result has more mappings than the last.
                if (result != null && (bestResult == null || result.Count > bestResult.Count))
                {
                    bestResult = result; 
                }
            }

            return bestResult;
        }

        /// <summary>
        /// Searches <see paramref="possibleMappings"/> for a mappings.
        /// </summary>
        /// <param name="possibleMappings">A <see cref="TestMapper"/> instance describing the set
        /// of possible mappings of test names into test codes.</param>
        /// <param name="requireMandatory"><see langword="true"/> if mandatory tests are required.</param>
        /// <Returns>A list of <see cref="LabTest"/>s that have been mapped into the test codes</Returns>
        static List<LabTest> FindMapping(TestMapper possibleMappings, bool requireMandatory)
        {
            try
            {
                List<LabTest> result = new List<LabTest>();
                List<LabTest> bestResult = null;

                // Loop through each node in possibleMappings until one has at least two possible
                // mappings.
                List<LabTest> candidateTests;
                for (candidateTests = possibleMappings.PopNextNodeTests();
                     candidateTests != null && candidateTests.Count < 2;
                     candidateTests = possibleMappings.PopNextNodeTests())
                {
                    if (candidateTests.Count == 1
                        && !possibleMappings.HasBetterMappingForAttribute(candidateTests[0]))
                    {
                        // For any node where there is only one possibility, add that into the
                        // result.
                        LabTest candidateTest = candidateTests[0];
                        result.Add(candidateTest);

                        // Don't allow this test attribute to be mapped for any subsequent node.
                        possibleMappings._mappedTestAttributes.Add(candidateTest.Attribute);

                        // Don't allow this test code to be mapped for any subsequent attribute.
                        possibleMappings._mappedTestCodes.Add(candidateTest.TestCode);
                    }
                }

                // Iterate each possible mapping for the current node/attribute and find the one that allows
                // the most remaining nodes to be mapped.
                if (candidateTests != null)
                {
                    foreach (var candidateTest in candidateTests)
                    {
                        if (possibleMappings.HasBetterMappingForAttribute(candidateTest))
                        {
                            continue;
                        }

                        // Create a new copy of possibleMappings to pass into a recursive call on the
                        // remaining nodes.
                        var remainingMappings = new TestMapper(possibleMappings);

                        // Don't allow this test attribute to be mapped for any subsequent node.
                        remainingMappings._mappedTestAttributes.Add(candidateTest.Attribute);

                        // Don't allow this test code to be mapped for any subsequent attribute.
                        remainingMappings._mappedTestCodes.Add(candidateTest.TestCode);

                        List<LabTest> subResult = FindMapping(remainingMappings, requireMandatory);

                        // If a mapping was found, combine it with candidateTest to form the complete
                        // mapping.
                        if (subResult != null)
                        {
                            // Combine the sub result with the result already compiled up to this point.
                            subResult.Add(candidateTest);
                            subResult.AddRange(result);

                            // Update the best result if this result has more mappings than the last.
                            if (bestResult == null || subResult.Count > bestResult.Count)
                            {
                                bestResult = subResult;
                            }
                        }

                        // After trying at least one combination, ensure the permutations being searched
                        // don't get out of control.
                        if (possibleMappings.TotalOpenInstances >=
                            LabDEOrderMapper._COMBINATION_ALGORITHM_SAFETY_CUTOFF)
                        {
                            break;
                        }
                    }
                }

                bestResult = bestResult ?? result;

                // Mark all test codes selected as mapped
                foreach (var test in bestResult)
                {
                    possibleMappings._mappedTestCodes.Add(test.TestCode);
                }

                // Ensure all mandatory tests have been mapped if required
                if (requireMandatory && !possibleMappings._mandatoryTestCodes
                    .IsSubsetOf(possibleMappings._mappedTestCodes))
                {
                    return null;
                }
                else
                {
                    return bestResult;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35150");
            }
        }

        #endregion Static Methods

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestMapper"/> class.
        /// </summary>
        /// <param name="tests">The <see cref="LabTest"/>s that should be mapped.</param>
        /// <param name="mandatoryTestCodes">The mandatory test codes for the <see cref="LabOrder"/>
        /// into which the <see paramref="tests"/> are being mapped.</param>
        /// <param name="otherTestCodes">Other possible test codes for the <see cref="LabOrder"/>
        /// into which the <see paramref="tests"/> are being mapped.</param>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use for mapping.</param>
        /// <param name="finalPass">Whether this is the final pass of the order mapping algorithm</param>
        public TestMapper(IEnumerable<LabTest> tests,
            HashSet<string> mandatoryTestCodes, HashSet<string> otherTestCodes,
            OrderMappingDBCache dbCache, bool finalPass)
        {
            try
            {
                TotalOpenInstances = 1;
                _mandatoryTestCodes = mandatoryTestCodes;
                _otherTestCodes = otherTestCodes;
               
                InitializeMappingCandidates(tests, dbCache, finalPass);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35151");
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="TestMapper"/> class from being created.
        /// </summary>
        /// <param name="parentPermutations">The <see cref="TestMapper"/> from which this instance
        /// derives.</param>
        TestMapper(TestMapper parentPermutations)
        {
            try
            {
                // The child instance will share the following values with its ancestors.
                _mandatoryTestCodes = parentPermutations._mandatoryTestCodes;
                _otherTestCodes = parentPermutations._otherTestCodes;
                _attributeToCandidateTests = parentPermutations._attributeToCandidateTests;
                _totalOpenInstances = parentPermutations._totalOpenInstances;
                _sampleType = parentPermutations._sampleType;

                // The child instance will have separate sets of mapped test codes and names,
                // however, so more can be mapped without affecting the parent instance.
                _mappedTestCodes = new HashSet<string>(parentPermutations._mappedTestCodes, StringComparer.OrdinalIgnoreCase);
                _visitedTestAttributes = new HashSet<IAttribute>(parentPermutations._visitedTestAttributes);
                _mappedTestAttributes = new HashSet<IAttribute>(parentPermutations._mappedTestAttributes);

                TotalOpenInstances++;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35152");
            }
        }

        #endregion Constructors

        #region Private Members

        /// <summary>
        /// Keeps track of the total number of open <see cref="TestMapper"/> instances as part of
        /// the overall mapping attempt. If this number exceeds _COMBINATION_ALGORITHM_SAFETY_CUTOFF
        /// it will stop searching any new permutations.
        /// </summary>
        /// <value>
        /// The total total number of open <see cref="TestMapper"/> instances as part of the overall
        /// mapping attempt.
        /// </value>
        int TotalOpenInstances
        {
            get
            {
                return _totalOpenInstances[0];
            }

            set
            {
                _totalOpenInstances[0] = value;
            }
        }

        /// <summary>
        /// Initializes <see cref="_attributeToCandidateTests"/> such that each result component attribute is
        /// mapped to a list of possible tests.
        /// </summary>
        /// <param name="tests">The <see cref="LabTest"/>s that should be mapped.</param>
        /// <param name="dbCache">The <see cref="OrderMappingDBCache"/> to use to get potential
        /// test codes.</param>
        /// <param name="finalPass">Whether this is the final pass of the order mapping algorithm</param>
        void InitializeMappingCandidates(IEnumerable<LabTest> tests, OrderMappingDBCache dbCache,
            bool finalPass)
        {
            try
            {
                var candidateTestCodes = new HashSet<string>
                    (_mandatoryTestCodes.Concat(_otherTestCodes), StringComparer.OrdinalIgnoreCase);

                // Loop through all available tests and assign a copy of each to every test code
                // key to which it could be mapped and add each test code to a map of attributes
                // to possible mappings
                foreach (LabTest test in tests)
                {
                    bool fuzzyMatch;
                    foreach (var testCode in dbCache.GetPotentialTestCodes(test.Name, out fuzzyMatch)
                        .Where(s => candidateTestCodes.Contains(s)))
                    {
                        // Create a copy of the test and assign the test code.
                        LabTest mappedTest = new LabTest(test.Attribute);
                        mappedTest.TestCode = testCode;
                        mappedTest.FuzzyMatch = fuzzyMatch;
                        if (finalPass && !String.IsNullOrEmpty(mappedTest.TestCode))
                        {
                            mappedTest.FirstPassMapping = testCode == test.TestCode;
                        }
                        mappedTest.SampleType = dbCache.GetSampleType(testCode);

                        // Add test to mapping of attributes to tests
                        _attributeToCandidateTests.GetOrAdd(mappedTest.Attribute, a =>
                            new List<LabTest>()).Add(mappedTest);
                    }
                }

                // Set the MatchScore for any test that could be mapped to more than one test code.
                foreach (var mappedTests in _attributeToCandidateTests.Values
                    .Where(l => l.Count > 1))
                {
                    foreach (var mappedTest in mappedTests)
                    {
                        mappedTest.MatchScore = dbCache.GetMappingScore(mappedTest.TestCode, mappedTest.Attribute);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35153");
            }
        }

        /// <summary>
        /// Gets the candidate <see cref="LabTest"/>s to map to the next test code (or node) in
        /// the target order.
        /// </summary>
        /// <returns>The candidate <see cref="LabTest"/>s.</returns>
        List<LabTest> PopNextNodeTests()
        {
            try
            {
                // Get candidates for the next test attribute
                var nextCandidates = _attributeToCandidateTests.Keys
                    // Where the attribute hasn't already been iterated or mapped
                    .Where(attribute => !_visitedTestAttributes.Contains(attribute)
                    && !_mappedTestAttributes.Contains(attribute))
                    // Select all possible test codes for this attribute...
                    .Select(attribute => _attributeToCandidateTests[attribute]
                        // That haven't been mapped for this order already
                        .Where(relatedTest => !_mappedTestCodes.Contains(relatedTest.TestCode)
                        // Where the sample type is compatible with this order
                        && IsSampleTypeCompatible(relatedTest))
                        .Select(test => new
                            {
                                Test = test,
                                Mandatory = _mandatoryTestCodes.Contains(test.TestCode),
                                MatchScore = test.MatchScore,
                                FuzzyMatch = test.FuzzyMatch
                            })
                        // Order the results to give preference to mandatory tests...
                        .OrderBy(candidate => candidate.Mandatory ? 0 : 1)
                        // and then to higher match scores
                        .ThenByDescending(candidate => candidate.MatchScore)
                        .ToList())
                    .Where(candidates => candidates.Count > 0)
                    // First try attributes that have only one possible match
                    .OrderBy(candidates => candidates.Count)
                    // ...giving preference to attributes that can be mapped to a mandatory test
                    .ThenBy(candidates => candidates[0].Mandatory ? 0 : 1)
                    // ...and those that did not require using a fuzzy pattern to match
                    .ThenBy(candidates => candidates[0].FuzzyMatch ? 1 : 0)
                    // and those with a higher match score
                    .ThenByDescending(candidates => candidates[0].MatchScore);

                // If there are no more nodes, return null;
                if (!nextCandidates.Any())
                {
                    return null;
                }

                // The candidate tests for the next attribute
                var nextCandidatesForAttribute = nextCandidates.First()
                    .Select(candidate => candidate.Test)
                    .ToList();

                // Mark the attribute as visited
                _visitedTestAttributes.Add(nextCandidatesForAttribute[0].Attribute);

                return nextCandidatesForAttribute;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35154");
            }
        }

        /// <summary>
        /// Determines whether the sample type of a <see cref="LabTest"/> is compatible with this
        /// set of mappings.
        /// </summary>
        /// <param name="test">The <see cref="LabTest"/> to check</param>
        /// <returns>True if the sample type is compatible</returns>
        bool IsSampleTypeCompatible(LabTest test)
        {
            return String.IsNullOrEmpty(_sampleType)
                || String.IsNullOrEmpty(test.SampleType)
                || _sampleType == test.SampleType;
        }

        /// <summary>
        /// Determines whether there is a better choice of attribute-to-test-code mapping for
        /// the <see cref="IAttribute"/> of this <see cref="LabTest"/>
        /// </summary>
        /// <param name="currentChoice">The mapping being considered</param>
        /// <returns>True if there is a better choice of mapping</returns>
        bool HasBetterMappingForAttribute(LabTest currentChoice)
        {
            return _attributeToCandidateTests[currentChoice.Attribute]
                .Where(test => test != currentChoice)
                .Where(possibleBetterMapping =>
                    possibleBetterMapping != currentChoice &&
                    (   // Prefer previously mapped test if sample type is different from current candidate
                       possibleBetterMapping.FirstPassMapping && !currentChoice.FirstPassMapping
                       && possibleBetterMapping.SampleType != currentChoice.SampleType
                       // Prefer other test if match score is greater and test code
                       // is not already mapped to other attribute
                    || possibleBetterMapping.MatchScore > currentChoice.MatchScore
                       && (possibleBetterMapping.FirstPassMapping || !currentChoice.FirstPassMapping)
                       && !_mappedTestCodes.Contains(possibleBetterMapping.TestCode
                    ))).Any();
        }

        /// <summary>
        /// Gets the possible sample types for this order
        /// </summary>
        /// <returns>A collection of possible sample types for this order</returns>
        IEnumerable<string> GetPossibleSampleTypes()
        {
            var possibleSampleTypes = new HashSet<string>();
            foreach (var test in _attributeToCandidateTests.Values
                .SelectMany(tests => tests).Where(test => !String.IsNullOrEmpty(test.SampleType)))
            {
                possibleSampleTypes.Add(test.SampleType);
            }

            return possibleSampleTypes;
        }

        #endregion Private Members
    }
}
