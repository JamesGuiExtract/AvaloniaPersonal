using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Extract
{
    /// <summary>
    /// Provides ability to find data in an <see cref="ExtractException"/>.
    /// </summary>
    public class ExtractExceptionQuery
    {
        /// <summary>
        /// Indicates whether this instance includes or excludes matching exceptions.
        /// </summary>
        bool _inclusionQuery;

        /// <summary>
        /// Indicates whether the exception itself should be returned as output.
        /// </summary>
        bool _isOutput;

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
        private ExtractExceptionQuery(List<string> specifications)
        {
            try
            {
                ExtractException.Assert("ELI30597", "Missing query specifications.",
                    specifications != null && specifications.Count > 0);

                // For now, it is not possible for ExceptionQueries to return exceptions. However,
                // this class is designed. with this possibility in mind.
                _isOutput = false;

                // Parameter 0: "I" for include, or "E" for exclude.
                // Parameter 1: "T" for top level exception, "I" for inner exception, blank for
                //              either type of exception.
                // Parameter 2: Regex that should match the ELI code, or blank for any ELI code.
                // Parameter 3: Regex that should match the exception's text, or blank to match any
                //              exception's text.
                // Parameter 4: Name of debug data item whose value should be extracted, or blank
                //              to match any debug data item for an inclusion, or nothing for an
                //              exclusion.
                string[] fields = specifications[0].Split(',');
                if (fields.Length != 5)
                {
                    ExtractException ee = new ExtractException("ELI30596",
                        "Invalid number of fields in query specification line.");
                    ee.AddDebugData("Specification", specifications[0], false);
                    throw ee;
                }

                specifications.RemoveAt(0);

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

                if (!string.IsNullOrEmpty(fields[4]))
                {
                    _outputDebugData.Add(fields[4]);
                }


                // If there are sub-queries specified, create them.
                if (specifications.Count > 0)
                {
                    // In this format, if a debug type was not specified, allow that to mean
                    // subsequent queries are to be and'd rather than or'd with this one.
                    if (_outputDebugData.Count == 0)
                    {
                        _andQuery = new ExtractExceptionQuery(specifications);
                    }
                    else
                    {
                        _orQuery = new ExtractExceptionQuery(specifications);
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
        public ReadOnlyCollection<string> GetResults(ExtractException ee)
        {
            try
            {
                List<string> results = new List<string>();

                GetResults(ee, 1, true, null, results);

                return results.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30587", ex);
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
        /// <param name="includeDecendents">Whether results should be gathered from the decendents
        /// of this exception as well.</param>
        /// <param name="exceptions">Any <see cref="ExtractException"/> results will be added to
        /// this list if non-<see langword="null"/>.</param>
        /// <param name="strings">Any <see langword="string"/> results will be added to this list if
        /// non-<see langword="null"/>.
        /// </param>
        void GetResults(ExtractException ee, int generation, bool includeDecendents,
            List<ExtractException> exceptions, List<string> strings)
        {
            if (ee == null)
            {
                return;
            }

            // The exception is included if the query matches for an inclusion query or the query
            // fails to match an exclusion query.
            bool include = GetIsMatch(ee, generation) == _inclusionQuery;

            if (include)
            {
                // Add exception output if requested.
                if (_isOutput && exceptions != null)
                {
                    exceptions.Add(ee);
                }

                // Add debug data if requested.
                if (strings != null)
                {
                    foreach (string debugDataItem in _outputDebugData)
                    {
                        foreach (DictionaryEntry entry in ee.Data)
                        {
                            if (entry.Key != null &&
                                Regex.IsMatch(entry.Key.ToString(), debugDataItem,
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

            // Retrieve the results of the subqueries against this exception.
            if (include && _andQuery != null)
            {
                _andQuery.GetResults(ee, generation, false, exceptions, strings);
            }
            
            if (_orQuery != null)
            {
                _orQuery.GetResults(ee, generation, false, exceptions, strings);
            }

            if (includeDecendents)
            {
                // Retrieve the results of this query on the inner exceptions.
                GetResults(ee.InnerException as ExtractException, generation + 1, true,
                    exceptions, strings);
            }
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

            return true;
        }

        #endregion Private Methods
    }
}
