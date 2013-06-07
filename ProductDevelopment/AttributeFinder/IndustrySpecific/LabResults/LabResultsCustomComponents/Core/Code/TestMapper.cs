using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// The complete set of possible test codes for the target <see cref="LabOrder"/>.
        /// </summary>
        List<string> _allTestCodes;

        /// <summary>
        /// Keeps track of the test codes that have already been mapped and, thus, should not be
        /// considered for any additonal tests.
        /// </summary>
        HashSet<string> _mappedTestCodes = new HashSet<string>();

        /// <summary>
        /// Keeps track of the test names that have already been mapped and, thus, should not be
        /// considered for any additonal tests.
        /// </summary>
        HashSet<string> _mappedTestNames = new HashSet<string>();

        /// <summary>
        /// Maps each <see cref="_allTestCodes"/> to the available LabTests that could be associated
        /// with that test code based on the test name.
        /// </summary>
        Dictionary<string, List<LabTest>> _mappingCandidates = new Dictionary<string, List<LabTest>>();

        /// <summary>
        /// Keeps track of the total number of open <see cref="TestMapper"/> instances as part of
        /// the overall mapping attempt. If this number exceeds _COMBINATION_ALGORITHM_SAFETY_CUTOFF
        /// it will stop searching any new permutations.
        /// HACK: This is an array with one and only value so the value can be accessed by reference.
        /// </summary>
        int[] _totalOpenInstances = new int[] { 0 };

        #endregion Fields

        #region Static Methods

        /// <summary>
        /// Finds the best mapping in terms of the number of <see paramref="tests"/> that can be
        /// mapped into the the tests defined by <see paramref="allTestCodes"/> based on the
        /// available mappings in <see paramref="nameToTestMapping"/>. The best mapping is defined
        /// as the mapping that maps the most tests; in the event of a tiebreaker, the tiebreaker is
        /// the one that maps the most mandatory tests.
        /// </summary>
        /// <param name="tests">The <see cref="LabTest"/>s that should be mapped</param>
        /// <param name="mandatoryTestCodes">The mandatory test codes for the <see cref="LabOrder"/>
        /// into which the <see paramref="tests"/> are being mapped.</param>
        /// <param name="allTestCodes">All possible test codes for the <see cref="LabOrder"/>
        /// into which the <see paramref="tests"/> are being mapped.</param>
        /// <param name="nameToTestMapping">A map of each possible name on the document to the
        /// test code(s) for which that name might be associated.</param>
        /// <returns>
        /// A list of <see cref="LabTest"/>s that have been mapped into the test codes.
        /// <para><b>Note:</b></para>
        /// The LabTest instances returned will not be the same instances from
        /// <see paramref="tests"/>. Rather, they will be new instances where the the appropriate
        /// <see cref="LabTest.TestCode"/> value has been assigned.
        /// </returns>
        public static List<LabTest> FindBestMapping(IEnumerable<LabTest> tests,
            IEnumerable<string> mandatoryTestCodes, IEnumerable<string> allTestCodes,
            IEnumerable<KeyValuePair<string, List<string>>> nameToTestMapping)
        {
            try
            {
                TestMapper possibleMappings =
                        new TestMapper(tests, mandatoryTestCodes, allTestCodes, nameToTestMapping);

                var matchingTests = FindMapping(possibleMappings, false);

                return matchingTests;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35148");
            }
        }

        /// <summary>
        /// Checks whether all test codes in <see paramref="mandatoryTestCodeDomain"/> can be mapped
        /// from <see paramref="tests"/>.
        /// </summary>
        /// <param name="tests">The <see cref="LabTest"/>s that should be mapped</param>
        /// <param name="mandatoryTestCodes">The mandatory test codes for the <see cref="LabOrder"/>
        /// into which the <see paramref="tests"/> are being mapped.</param>
        /// <param name="nameToTestMapping">A map of each possible name on the document to the 
        /// test code(s) for which that name might be associated.</param>
        /// <returns><see langword="true"/> if <see paramref="tests"/> can be used to satisfy all
        /// codes from <see paramref="mandatoryTestCodeDomain"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool AllMandatoryTestsExist(IEnumerable<LabTest> tests,
            IEnumerable<string> mandatoryTestCodes,
            IEnumerable<KeyValuePair<string, List<string>>> nameToTestMapping)
        {
            try
            {
                TestMapper possibleMappings =
                        new TestMapper(tests, mandatoryTestCodes, new string[] { }, nameToTestMapping);

                var matchingTests = FindMapping(possibleMappings, true);

                return (matchingTests != null);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35149");
            }
        }

        /// <summary>
        /// Searches <see paramref="possibleMappings"/> for a mappings.
        /// </summary>
        /// <param name="possibleMappings">A <see cref="TestMapper"/> instance describing the set
        /// of possible mappings of test names into test codes.</param>
        /// <param name="checkingMandatory"><see langword="true"/> if mappings should be found only
        /// to determine whether all mandatory tests can be mapped; <see langword="false"/> to find
        /// the best match.</param>
        /// <Returns>A list of <see cref="LabTest"/>s that have been mapped into the test codes.
        /// </Returns>
        public static List<LabTest> FindMapping(TestMapper possibleMappings, bool checkingMandatory)
        {
            try
            {
                List<LabTest> result = new List<LabTest>();
                List<LabTest> bestResult = null;

                // Loop through each node in possibleMappings until one has at least two possible
                // mappings.
                bool isMandatory;
                List<LabTest> candidateTests;
                for (candidateTests = possibleMappings.PopNextNodeTests(out isMandatory);
                     candidateTests != null && candidateTests.Count < 2;
                     candidateTests = possibleMappings.PopNextNodeTests(out isMandatory))
                {
                    if (candidateTests.Count == 1)
                    {
                        // For any node where there is only one possibility, add that into the
                        // result.
                        LabTest candidateTest = candidateTests.First();
                        result.Add(candidateTest);

                        // Don't allow this test name to be mapped for any subsequent node.
                        // Mark this test code as mapped so that it is not returned in any future call.
                        possibleMappings.MarkTestNameAsMapped(candidateTest.Name);
                    }
                    else if (checkingMandatory && isMandatory)
                    {
                        // If there are no possibilities for a mandatory test, return null to
                        // indicate the mandatory check has failed.
                        return null;
                    }
                }

                // If there were no more candidate test nodes or we are past the mandatory nodes
                // when checkingMandatory, return what result we have.
                if (candidateTests == null ||
                    (!isMandatory && checkingMandatory))
                {
                    return result;
                }

                // Iterate each possible mapping for the current node and find the one that allows
                // the most remaining nodes to be mapped.
                foreach (LabTest candidateTest in candidateTests)
                {
                    // Create a new copy of possibleMappings to pass into a recusive call on the
                    // remaining nodes.
                    var remainingMappings = new TestMapper(possibleMappings);

                    // Don't allow this test name to be mapped for any subsequent node.
                    remainingMappings.MarkTestNameAsMapped(candidateTest.Name);

                    List<LabTest> subResult = FindMapping(remainingMappings, checkingMandatory);

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

                            // If checking for mandatory, we don't need the best mapping, we just
                            // need one; we can return right away.
                            if (checkingMandatory)
                            {
                                return bestResult;
                            }
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

                // If we have gotten to this point when checkingMandatory, we have failed to map a
                // mandatory test; return null to indicate the mapping failed.
                if (checkingMandatory)
                {
                    return null;
                }

                // Otherwise we will have found the best mapping; return it.
                return bestResult ?? result;
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
        /// <param name="allTestCodes">All possible test codes for the <see cref="LabOrder"/>
        /// into which the <see paramref="tests"/> are being mapped.</param>
        /// <param name="nameToTestMapping">A map of each possible name on the document to the
        /// test code(s) for which that name might be associated.</param>
        TestMapper(IEnumerable<LabTest> tests,
            IEnumerable<string> mandatoryTestCodes, IEnumerable<string> allTestCodes,
            IEnumerable<KeyValuePair<string, List<string>>> nameToTestMapping)
        {
            try
            {
                TotalOpenInstances = 1;
                _mandatoryTestCodes = new HashSet<string>(mandatoryTestCodes);
                _allTestCodes = new List<string>(mandatoryTestCodes.Union(allTestCodes));

                var nameToTestMappingDictionary = new Dictionary<string, List<string>>();
                foreach (var mapping in nameToTestMapping)
                {
                    nameToTestMappingDictionary[mapping.Key] = mapping.Value;
                }

                InitializeMappingCandidates(tests, nameToTestMappingDictionary);
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
                _allTestCodes = parentPermutations._allTestCodes;
                _mappingCandidates = parentPermutations._mappingCandidates;
                _totalOpenInstances = parentPermutations._totalOpenInstances;

                // The child instance will have separate sets of mapped test codes and names,
                // however, so more can be mapped without affecting the parent instance.
                _mappedTestCodes = new HashSet<string>(parentPermutations._mappedTestCodes);
                _mappedTestNames = new HashSet<string>(parentPermutations._mappedTestNames);

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
        /// Initializes <see cref="_mappingCandidates"/> such that each test code key (or node) is
        /// mapped to a list of possible names for the test.
        /// </summary>
        /// <param name="tests">The <see cref="LabTest"/>s that should be mapped.</param>
        /// <param name="nameToTestMapping">A map of each possible name on the document to the
        /// test code(s) for which that name might be associated.</param>
        void InitializeMappingCandidates(IEnumerable<LabTest> tests,
            Dictionary<string, List<string>> nameToTestMapping)
        {
            try
            {
                // Initialize _mappingCandidates with a key for every test code that could appear in
                // the target order.
                _mappingCandidates.Clear();
                foreach (string testCode in _allTestCodes.Distinct())
                {
                    _mappingCandidates[testCode] = new List<LabTest>();
                }

                // Loop through all available tests and assign a copy of each to every test code
                // key to which it could be mapped.
                foreach (LabTest test in tests)
                {
                    // Iterate each code for each test this name could be mapped under.
                    foreach (string testCode in nameToTestMapping
                        .Where(mapping =>
                            mapping.Key.Equals(test.Name, StringComparison.OrdinalIgnoreCase))
                        .SelectMany(mandatoryTest => mandatoryTest.Value)
                        .Distinct())
                    {
                        // Create a copy of the test and assign the test code.
                        LabTest mappedTest = new LabTest(test.Attribute);
                        mappedTest.TestCode = testCode;

                        // Add the test copy as a possible mapping for testCode.
                        _mappingCandidates[testCode].Add(mappedTest);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35153");
            }
        }

        /// <summary>
        /// Gets the candidate <see cref="LabTest"/>s for to map to the next test code (or node) in
        /// the target order.
        /// </summary>
        /// <returns>The candidate <see cref="LabTest"/>s.</returns>
        List<LabTest> PopNextNodeTests(out bool isMandatory)
        {
            try
            {
                // Select the next test code key for which candidate tests should be grabbed.
                // The mandatory test codes will come first so that they are mapped before
                // non-mandatory tests.
                string nextKey = _allTestCodes
                    .Where(code => !_mappedTestCodes.Contains(code))
                    .FirstOrDefault();

                // If there are no more nodes, return null;
                if (nextKey == default(string))
                {
                    isMandatory = false;
                    return null;
                }

                isMandatory = _mandatoryTestCodes.Contains(nextKey);

                // Mark this test code as mapped so that it is not returned in any future call.
                _mappedTestCodes.Add(nextKey);

                // Return a the list of all candiate LabTests for this test code that haven't
                // already been mapped.
                return new List<LabTest>(
                    _mappingCandidates[nextKey]
                        .Where(test => !_mappedTestNames.Contains(test.Name)));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35154");
            }
        }

        /// <summary>
        /// Specifies that <see paramref="testName"/> has been mapped for this order so that it
        /// isn't considered as a candidate for any additional test codes.
        /// </summary>
        /// <param name="testName">The test name to mark as mapped.</param>
        void MarkTestNameAsMapped(string testName)
        {
            try
            {
                _mappedTestNames.Add(testName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35155");
            }
        }

        #endregion Private Members
    }
}
