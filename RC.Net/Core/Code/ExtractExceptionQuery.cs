using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Extract
{
    /// <summary>
    /// Provides ability to find data in an <see cref="ExtractException"/>.
    /// </summary>
    public class ExtractExceptionQuery
    {
        /// <summary>
        /// Indicates whether the query could not be defined based on the provided specification.
        /// </summary>
        bool _isNull;

        /// <summary>
        /// Indicates whether this instance includes or excludes matching exceptions.
        /// </summary>
        bool _inclusionQuery;

        /// <summary>
        /// If > 0, the lowest matching exception generation where 1 is top-level
        /// </summary>
        int _minGeneration;

        /// <summary>
        /// If > 0, the highest matching exception generation where 1 is top-level
        /// </summary>
        int _maxGeneration;

        /// <summary>
        /// The regex ELI codes will need to match to be a match.
        /// </summary>
        string _ELIfilter;

        /// <summary>
        /// The regex the exception message will need to match to be a match.
        /// </summary>
        string _messageFilter;

        /// <summary>
        /// The regex a debug data item name and associated value will need to match to be a match.
        /// </summary>
        Dictionary<string, string> _debugDataFilter = new Dictionary<string, string>();

        /// <summary>
        /// The debug data items from matching exceptions that are to be output.
        /// </summary>
        List<string> _outputDebugData = new List<string>();

        /// <summary>
        /// A sub-query whose result should be and'd with the result of this query.
        /// </summary>
        ExtractExceptionQuery _andQuery;

        /// <summary>
        /// A sub-query whose result should be or'd with the result of this query.
        /// </summary>
        ExtractExceptionQuery _orQuery;

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ExtractExceptionQuery"/> instance.
        /// </summary>
        /// <param name="specifications">The specifications for the query.</param>
        public ExtractExceptionQuery(IEnumerable<string> specifications)
            : this(new List<string>(specifications))
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ExtractExceptionQuery"/> instance.
        /// </summary>
        /// <param name="specifications">The specifications for the query.</param>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Text.RegularExpressions.Regex")]
        private ExtractExceptionQuery(List<string> specifications)
        {
            try
            {
                ExtractException.Assert("ELI30597", "Missing query specifications.",
                    specifications != null && specifications.Count > 0);

                string[] fields = null;

                for (int i = 0; i < specifications.Count; i++)
                {
                    string specificationLine = specifications[i];

                    // Ignore comment lines or blank lines
                    if (specificationLine.StartsWith("//", StringComparison.OrdinalIgnoreCase) ||
                        specificationLine.Trim().Length == 0)
                    {
                        specifications.RemoveAt(i);
                        i--;
                        continue;
                    }

                    // All exclusion queries (which are to be and'd) are to be processed first so that
                    // the spec file is order-independent.
                    if (specificationLine.StartsWith("E", StringComparison.OrdinalIgnoreCase))
                    {
                        fields = specificationLine.Split(',');
                        specifications.RemoveAt(i);
                        break;
                    }
                }

                if (fields == null)
                {
                    // If there are no valid specification lines, define this query as null.
                    if (specifications.Count == 0)
                    {
                        _isNull = true;
                        return;
                    }

                    fields = specifications[0].Split(',');
                    specifications.RemoveAt(0);
                }

                // Parameter 0: "I" for include, or "E" for exclude.
                // Parameter 1: "T" for top level exception, "I" for inner exception, blank for
                //              either type of exception.
                // Parameter 2: Regex that should match the ELI code, or blank for any ELI code.
                // Parameter 3: Regex that should match the exception's text, or blank to match any
                //              exception's text.
                // Parameter 4: Name of debug data item whose value should be extracted, or blank
                //              to match any debug data item for an inclusion, or nothing for an
                //              exclusion.
                // Parameter 5: (optional) Regex the debug data value should match to be considered a match.
                if (fields.Length < 5 || fields.Length > 6)
                {
                    ExtractException ee = new ExtractException("ELI30596",
                        "Invalid number of fields in query specification line.");
                    ee.AddDebugData("Specification", specifications[0], false);
                    throw ee;
                }

                // Parse the specification parameters.
                if (fields[0].Equals("I", StringComparison.OrdinalIgnoreCase))
                {
                    _inclusionQuery = true;
                }
                else if (fields[0].Equals("E", StringComparison.OrdinalIgnoreCase))
                {
                    _inclusionQuery = false;
                }
                else
                {
                    throw new ExtractException("ELI30576",
                        "First field on a line must be E for exclude or I for include.");
                }

                if (fields[1].Equals("T", StringComparison.OrdinalIgnoreCase))
                {
                    _maxGeneration = 1;
                }
                else if (fields[1].Equals("I", StringComparison.OrdinalIgnoreCase))
                {
                    _minGeneration = 2;
                }
                else if (!string.IsNullOrEmpty(fields[1]))
                {
                    throw new ExtractException("ELI30577",
                        "Second field on a line must be T for top-level or I for inner exception.");
                }
  
                _ELIfilter = fields[2];
                if (!string.IsNullOrEmpty(_ELIfilter))
                {
                    // Validate the regex
                    new Regex(_ELIfilter);
                }

                _messageFilter = fields[3];
                if (!string.IsNullOrEmpty(_messageFilter))
                {
                    // Validate the regex
                    new Regex(_messageFilter);
                }

                string debugDataFilter = fields[4];
                if (_inclusionQuery && string.IsNullOrEmpty(debugDataFilter))
                {
                    // An inclusion filter with no debug data filter specified should be
                    // interpreted as a filter that will match any debug data item.
                    debugDataFilter = "\\S";
                }
                if (!string.IsNullOrEmpty(debugDataFilter))
                {
                    _outputDebugData.Add(debugDataFilter);

                    _debugDataFilter[debugDataFilter] = fields.Length > 5 ? fields[5] : null;
                }

                // If there are sub-queries specified, create them.
                if (specifications.Count > 0)
                {
                    ExtractExceptionQuery nextQuery = new ExtractExceptionQuery(specifications);
                    if (!nextQuery._isNull)
                    {
                        // In this format, inclusion queries are to be or'd while exclusion queries are
                        // to be and'd.
                        if (_inclusionQuery)
                        {
                            _orQuery = nextQuery;
                        }
                        else
                        {
                            _andQuery = nextQuery;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30578", ex);
            }
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Retrieves the query results as a collection of <see langword="string"/> values.
        /// </summary>
        /// <param name="ee">The <see cref="ExtractException"/> on which the query should be
        /// performed.</param>
        /// <returns>The matching data elements from the specified
        /// <see cref="ExtractException"/>.</returns>
        public ReadOnlyCollection<string> GetDebugData(ExtractException ee)
        {
            try
            {
                List<string> results = new List<string>();

                GetResults(ee, 1, true, results);

                return results.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30587", ex);
            }
        }

        /// <summary>
        /// Gets whether the specified and all inner exceptions (if they exist) are specifically
        /// excluded by the query (as opposed to simply not being included).
        /// </summary>
        /// <param name="ee">The <see cref="ExtractException"/> to test.</param>
        /// <returns><see langword="true"/> if this exception and all inner exceptions are
        /// specifically excluded, <see langword="false"/> if either this exception or one of its
        /// inner exceptions are not specifically excluced.</returns>
        public bool GetIsEntirelyExcluded(ExtractException ee)
        {
            try
            {
                return GetIsEntirelyExcluded(ee, 1);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30602", ex);
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Helper method to recursively retrieve <see langword="string"/> and
        /// <see cref="ExtractException"/> values from the specified <see cref="ExtractException"/>.
        /// </summary>
        /// <param name="ee">The <see cref="ExtractException"/> on which the query should be
        /// performed.</param>
        /// <param name="generation">The generation of the current exception where 1 is a top level
        /// exception.</param>
        /// <param name="includeDescendents">Whether results should be gathered from the descendents
        /// of this exception as well.</param>
        /// <param name="strings">Any <see langword="string"/> results will be added to this list if
        /// non-<see langword="null"/>.
        /// </param>
        bool GetResults(ExtractException ee, int generation, bool includeDescendents,
            List<string> strings)
        {
            if (ee == null)
            {
                return false;
            }

            // The exception is included if the query matches for an inclusion query or the query
            // fails to match an exclusion query.
            bool include = GetIsMatch(ee, generation) == _inclusionQuery;

            if (include)
            {
                // Add debug data if requested.
                if (strings != null)
                {
                    foreach (KeyValuePair<string, string> debugDataItem in _debugDataFilter)
                    {
                        // Attempt to find a matching debug data item.
                        foreach (DictionaryEntry entry in ee.Data)
                        {
                            if (Regex.IsMatch(entry.Key.ToString(), debugDataItem.Key,
                                    RegexOptions.IgnoreCase))
                            {
                                // If a value filter has been specifed, does the value match?
                                if (string.IsNullOrEmpty(debugDataItem.Value) ||
                                    Regex.IsMatch(entry.Value.ToString(), debugDataItem.Value,
                                        RegexOptions.IgnoreCase))
                                {
                                    string value =
                                        (entry.Value == null ? "null" : entry.Value.ToString());
                                    // TODO: Decrypt value if necessary?

                                    strings.Add(value);
                                }
                            }
                        }
                    }
                }
            }

            // Retrieve the results of the subqueries against this exception.
            if (include && _andQuery != null)
            {
                _andQuery.GetResults(ee, generation, false, strings);
            }
            
            if (_orQuery != null)
            {
                include |= _orQuery.GetResults(ee, generation, false, strings);
            }

            if (includeDescendents)
            {
                // Retrieve the results of this query on the inner exceptions.
                include |= GetResults(ee.InnerException as ExtractException, generation + 1, true,
                    strings);
            }

            return include;
        }

        /// <summary>
        /// Gets whether the specified and all inner exceptions (if they exist) are specifically
        /// excluded by the query (as opposed to simply not being included).
        /// </summary>
        /// <param name="ee">The <see cref="ExtractException"/> to test.</param>
        /// <param name="generation">The generation of <paramref name="ee"/> where 1 is top-level.
        /// </param>
        /// <returns><see langword="true"/> if this exception and all inner exceptions are
        /// specifically excluded, <see langword="false"/> if either this exception or one of its
        /// inner exceptions are not specifically excluced.</returns>
        bool GetIsEntirelyExcluded(ExtractException ee, int generation)
        {
            if (ee == null)
            {
                return true;
            }

            // Test to see if this exception is exluded by either this query element or one of the
            // and query elements. Or elements need not be considered since once an or element is
            // present, we cannot say that the exception is entirely excluded.
            bool excluded = !_inclusionQuery && GetIsMatch(ee, generation);
            
            for (ExtractExceptionQuery andQuery = _andQuery;
                 !excluded && andQuery != null;
                 andQuery = andQuery._andQuery)
            {
                excluded = !andQuery._inclusionQuery && andQuery.GetIsMatch(ee, generation);
            }

            // Return true only if excluded == true and GetIsEntirelyExcluded returns true for all
            // inner exceptions as well.
            excluded &= GetIsEntirelyExcluded(ee.InnerException as ExtractException, generation + 1);

            return excluded;
        }

        /// <summary>
        /// Determines whether this <see cref="ExtractExceptionQuery"/> matches the specified
        /// exception.
        /// </summary>
        /// <param name="ee">The <see cref="ExtractException"/> to test for a match.</param>
        /// <param name="generation">The generation of the specified <see cref="ExtractException"/>.
        /// </param>
        /// <returns><see langword="true"/> if the query produces a match; <see langword="false"/>
        /// otherwise.</returns>
        bool GetIsMatch(ExtractException ee, int generation)
        {
            if (_minGeneration > 0 && generation < _minGeneration)
            {
                return false;
            }

            if (_maxGeneration > 0 && generation > _maxGeneration)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(_ELIfilter) &&
                !Regex.IsMatch(ee.EliCode, _ELIfilter, RegexOptions.IgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(_messageFilter) &&
                !Regex.IsMatch(ee.Message, _messageFilter, RegexOptions.IgnoreCase))
            {
                return false;
            }

            if (_debugDataFilter.Count > 0)
            {
                foreach (KeyValuePair<string, string> debugDataItem in _debugDataFilter)
                {
                    bool foundMatch = false;

                    // Attempt to find a matching debug data item.
                    foreach (DictionaryEntry entry in ee.Data)
                    {
                        if (Regex.IsMatch(entry.Key.ToString(), debugDataItem.Key,
                                RegexOptions.IgnoreCase))
                        {
                            // If a value filter has been specifed, does the value match?
                            if (string.IsNullOrEmpty(debugDataItem.Value) ||
                                Regex.IsMatch(entry.Value.ToString(), debugDataItem.Value,
                                    RegexOptions.IgnoreCase))

                            {
                                foundMatch = true;
                                break;
                            }
                        }
                    }

                    if (!foundMatch)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion Private Methods
    }
}
