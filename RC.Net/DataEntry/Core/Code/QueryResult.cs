using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Extract.Utilities;
using UCLID_AFCORELib;
using UCLID_RASTERANDOCRMGMTLib;
using TextFieldParser = Microsoft.VisualBasic.FileIO.TextFieldParser;

namespace Extract.DataEntry
{
    /// <summary>
    /// Specifies the way in which multiple query results will be handled. This includes the initial
    /// selection of results from an individual query node as well as governing the way query
    /// results are combined with one another. The following grid demonstrates the results from a
    /// single query node base on the selection mode:
    /// <code>
    ///             [A]     [A, B]      [A, B, C]
    /// None        [A]
    /// First       [A]     [A]         [A]
    /// List        [A]     [A, B]      [A, B, C]
    /// Distinct    [A]     [A, B]      [A, B, C]
    /// </code>
    /// Since a query result can either be a single value or a list of values, combining query nodes
    /// may result in one of the following actions:
    /// 1) Combining value lists from 2 query results into a single, larger list
    /// 2) Generating new query results to allow for different permutations of string values to be
    /// created.
    /// Primarily, these actions are governed by the presense or lack of the Distinct selection mode.
    /// While both "List" and "Distinct" produce the same result when applied to a single query node,
    /// when combining with other nodes, the "Distinct" setting will attempt to combine the string
    /// values from one query result in all possible ways with the values of another query result.
    /// The following grid outlines what happens when two query results, each with 2 values ([A,B]
    /// and [1,2]) are combined under the various selection mode settings. (Note that if either had
    /// the "None" setting applied, both query results would already be empty. However, if either
    /// result had produced only one value, it would be combined in the same fashion as with "First").
    /// <code>
    ///                             [1,2]
    ///                 First       List        Distinct
    ///       First     [A,1]       [A,1,2]     [A1,A2]
    /// [A,B] List      [A,B,1]     [A,B,1,2]   [AB1,AB2]
    ///       Distinct  [A1,B1]     [A12,B12]   [A1,A2,B1,B2]
    /// </code>
    /// </summary>
    public enum MultipleQueryResultSelectionMode
    {
        #region Enums

        /// <summary>
        /// If a query returns multiple values, only the first will be reported.
        /// </summary>
        First = 0,

        /// <summary>
        /// All query values will be reported; will seek to combine with other results by combining
        /// values from both into a larger list.
        /// </summary>
        List = 1,

        /// <summary>
        /// All query values will be reported; will seek to combine with other results by combining
        /// each string value in all possible ways with the values from the other query.
        /// </summary>
        Distinct = 2,

        /// <summary>
        /// If a query returns a single result it will be used. If the query results in multiple
        /// values, the query result will be empty.
        /// </summary>
        None = 3
    }

    #endregion Enums

    /// <summary>
    /// Represents the result of evaluating a <see cref="DataEntryQuery"/>. The result may be a single
    /// value or a set of values. The values may be of type <see cref="IAttribute"/>,
    /// <see cref="SpatialString"/> or <see langword="string"/> and may be accessed in a variety of
    /// ways.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class QueryResult : IEnumerable<QueryResult>
    {
        #region Fields

        /// <summary>
        /// The <see cref="QueryNode"/> this result is associated with.
        /// </summary>
        QueryNode _queryNode;

        /// <summary>
        /// A list of <see langword="string"/> values that define this result.
        /// </summary>
        List<string> _stringResults;

        /// <summary>
        /// A list of <see cref="SpatialString"/> values that define this result.
        /// </summary>
        List<SpatialString> _spatialResults;

        /// <summary>
        /// A list of <see cref="IAttribute"/> values that define this result.
        /// </summary>
        List<IAttribute> _attributeResults;

        /// <summary>
        /// A <see langword="object"/> that represents this result.
        /// </summary>
        object _objectResult;

        /// <summary>
        /// <see langword="true"/> if multiple values are present; <see langword="false"/>
        /// otherwise.
        /// </summary>
        bool _hasMultipleValues;

        /// <summary>
        /// A value may need to be processed separately from the other values in this result in
        /// which case the appropriate list will contain only one value and all remaining values
        /// will need to be accessed via this separate <see cref="QueryResult"/> instance.
        /// </summary>
        QueryResult _nextValue;

        /// <summary>
        /// The <see cref="MultipleQueryResultSelectionMode"/> mode associated with this result.
        /// </summary>
        MultipleQueryResultSelectionMode _selectionMode = MultipleQueryResultSelectionMode.None;

        /// <summary>
        /// A memoized function to split csv lines returned from data queries
        /// in order to speed up the creation of auto suggest lists that use lots of data
        /// https://extract.atlassian.net/browse/ISSUE-15673
        /// </summary>
        static Func<string, string[]> SplitCSVLine;

        #endregion Fields
       
        #region Constructors

        /// <summary>
        /// Initialize the static variables
        /// </summary>
        static QueryResult()
        {
            Func<string, string[]> splitCSVLine = line =>
            {
                using (var sr = new StringReader(line))
                using (var csvReader = new TextFieldParser(sr) {Delimiters = new[] {", "}})
                {
                    return csvReader.ReadFields();
                }
            };

            SplitCSVLine = splitCSVLine.Memoize(threadSafe: true);
        }

        /// <summary>
        /// Initializes a new, empty <see cref="QueryResult"/> instance.
        /// </summary>
        /// <param name="queryNode">The <see cref="QueryNode"/> this result is to be associated
        /// with.</param>
        /// <param name="result">A <see langword="object"/> that represents this result.</param>
        public QueryResult(QueryNode queryNode, object result)
        {
            _queryNode = queryNode;
            _objectResult = result;
        }

        /// <summary>
        /// Initializes a new, empty <see cref="QueryResult"/> instance.
        /// </summary>
        /// <param name="queryNode">The <see cref="QueryNode"/> this result is to be associated
        /// with.</param>
        public QueryResult(QueryNode queryNode)
            : this(queryNode, "")
        {
        }

        /// <summary>
        /// Initializes a new <see cref="QueryResult"/> instance based on the provided instance.
        /// </summary>
        /// <param name="queryNode">The <see cref="QueryNode"/> this result is to be associated
        /// with.</param>
        /// <param name="queryResult">The <see cref="QueryResult"/> the new instance should be
        /// modeled after. The new instance will be an exact copy except in the fact that it will
        /// not have access to the named results <see paramref="queryResult"/> did.</param>
        public QueryResult(QueryNode queryNode, QueryResult queryResult)
            : this(queryResult._queryNode)
        {
            try
            {
                // When creating new copy of existing results, we don't want the selection mode to
                // limit the number of values that already exist in this result. (multiple values
                // may have been achieved by combining results).
                if (queryNode.SelectionMode == MultipleQueryResultSelectionMode.None &&
                    queryResult.HasMultipleValues)
                {
                    _selectionMode = MultipleQueryResultSelectionMode.List;
                }

                if (queryResult.IsAttribute)
                {
                    _attributeResults = new List<IAttribute>(queryResult._attributeResults);
                    Initialize<IAttribute>(queryNode, _attributeResults);
                }
                else if (queryResult.IsSpatial)
                {
                    _spatialResults = new List<SpatialString>(queryResult._spatialResults);
                    Initialize<SpatialString>(queryNode, _spatialResults);
                }
                else if (queryResult.IsObject)
                {
                    _queryNode = queryNode;
                    _objectResult = queryResult._objectResult;
                }
                else
                {
                    _stringResults = new List<string>(queryResult._stringResults);
                    Initialize<string>(queryNode, _stringResults);
                }

                if (queryResult._nextValue != null)
                {
                    _hasMultipleValues = true;
                    _nextValue = new QueryResult(queryNode, queryResult._nextValue);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28981", ex);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="QueryResult"/> instance.
        /// </summary>
        /// <param name="queryNode">The <see cref="QueryNode"/> this result is to be associated
        /// with.</param>
        /// <param name="results">One or more <see langword="string"/> values representing the
        /// result of a <see cref="DataEntryQuery"/>.</param>
        public QueryResult(QueryNode queryNode, params string[] results)
        {
            try
            {
                _stringResults = new List<string>(results);
                Initialize<string>(queryNode, _stringResults);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28884", ex);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="QueryResult"/> instance.
        /// </summary>
        /// <param name="queryNode">The <see cref="QueryNode"/> this result is to be associated
        /// with.</param>
        /// <param name="results">One or more <see cref="SpatialString"/> values representing
        /// the result of a <see cref="DataEntryQuery"/>.</param>
        public QueryResult(QueryNode queryNode, params SpatialString[] results)
        {
            try
            {
                _spatialResults = new List<SpatialString>(results);
                Initialize<SpatialString>(queryNode, _spatialResults);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27060", ex);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="QueryResult"/> instance.
        /// </summary>
        /// <param name="queryNode">The <see cref="QueryNode"/> this result is to be associated
        /// with.</param>
        /// <param name="attributeResults">One or more <see cref="SpatialString"/> values representing
        /// the result of a <see cref="DataEntryQuery"/>.</param>
        public QueryResult(QueryNode queryNode, params IAttribute[] attributeResults)
        {
            try
            {
                _attributeResults = new List<IAttribute>(attributeResults);
                Initialize<IAttribute>(queryNode, _attributeResults);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28909", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets whether the result contains spatial information.
        /// </summary>
        /// <returns><see langword="true"/> if the result contains spatial information;
        /// <see langword="false"/> if it does not.</returns>
        public bool IsSpatial
        {
            get
            {
                try
                {
                    if (_spatialResults != null && _spatialResults.Count > 0)
                    {
                        return _spatialResults[0].HasSpatialInfo();
                    }
                    else if (IsAttribute)
                    {
                        return _attributeResults[0].Value.HasSpatialInfo();
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27059", ex);
                }
            }
        }

        /// <summary>
        /// Gets whether the result contains attribute information.
        /// </summary>
        /// <returns><see langword="true"/> if the result contains attribute information;
        /// <see langword="false"/> if it does not.</returns>
        public bool IsAttribute
        {
            get
            {
                try
                {
                    return (_attributeResults != null && _attributeResults.Count > 0);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28907", ex);
                }
            }
        }

        /// <summary>
        /// Gets the first <see cref="IAttribute"/> value.
        /// <para><b>Require:</b></para>
        /// <see cref="IsAttribute"/> must be <see langword="true"/>.
        /// </summary>
        /// <returns>The first <see cref="IAttribute"/> value.</returns>
        public IAttribute FirstAttribute
        {
            get
            {
                try
                {
                    ExtractException.Assert("ELI28908",
                        "Query result does not have any attribute info!", IsAttribute);

                    return _attributeResults[0];
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28910", ex);
                }
            }
        }

        /// <summary>
        /// Gets the first <see langword="SpatialString"/> value.
        /// <para><b>Require:</b></para>
        /// <see cref="IsSpatial"/> must be <see langword="true"/>.
        /// </summary>
        /// <returns>The first <see langword="SpatialString"/> value.</returns>
        public SpatialString FirstSpatialString
        {
            get
            {
                try
                {
                    ExtractException.Assert("ELI27058",
                        "Query result does not have any spatial info!", IsSpatial);

                    // Make a separate copy of the attribute's spatial string so that modifications
                    // to the query result to not affect the original attribute.
                    if (IsAttribute && _spatialResults == null)
                    {
                        _spatialResults = new List<SpatialString>();
                        _spatialResults.Add(_attributeResults[0].Value.Clone());
                    }

                    return _spatialResults[0];
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28911", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the result of the query as a <see langword="string"/>.
        /// </summary>
        /// <value>The result of the query as a <see langword="string"/>.</value>
        public string FirstString
        {
            get
            {
                try
                {
                    if (IsAttribute)
                    {
                        return _attributeResults[0].Value.String;
                    }
                    else if (_spatialResults != null && _spatialResults.Count > 0)
                    {
                        return _spatialResults[0].String;
                    }
                    else if (_stringResults != null && _stringResults.Count > 0)
                    {
                        return _stringResults[0];
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28912", ex);
                }
            }

            set
            {
                try
                {
                    if (FirstString != value)
                    {
                        if (IsSpatial)
                        {
                            // [DataEntry:1137]
                            // Because the SpatialString class will remove spatial info for blank
                            // strings, allow for the case that the spatial mode of
                            // FirstSpatialStringValue does not match IsSpatial.
                            if (FirstSpatialString.HasSpatialInfo())
                            {
                                FirstSpatialString.ReplaceAndDowngradeToHybrid(value);
                            }
                            else
                            {
                                FirstSpatialString.ReplaceAndDowngradeToNonSpatial(value);
                            }
                        }
                        else if (_stringResults != null && _stringResults.Count > 0)
                        {
                            _stringResults[0] = value;
                        }
                        else
                        {
                            if (_stringResults == null)
                            {
                                _stringResults = new List<string>();
                            }

                            _stringResults.Add(value);
                        }

                        // Once the value is modified, an attribute in the attribute list no longer
                        // defines the results value. Break out this result from any additional
                        // values and clear the attribute information.
                        BreakOutCurrentValue();
                        _attributeResults = null;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28913", ex);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this result is represented by <see cref="Object"/>.
        /// </summary>
        /// <value><see langword="true"/> if this instance is represented by
        /// <see cref="Object"/>; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsObject
        {
            get
            {
                return _objectResult != null;
            }
        }

        /// <summary>
        /// Gets the <see paramref="object"/> that represents this result.
        /// </summary>
        public object Object
        {
            get
            {
                return _objectResult;
            }
        }

        /// <summary>
        /// Gets a <see cref="QueryResult"/> representing the next value from this
        /// <see cref="QueryResult"/>.
        /// </summary>
        /// <returns>A <see cref="QueryResult"/> representing the next value from this
        /// <see cref="QueryResult"/>.</returns>
        public QueryResult NextValue
        {
            get
            {
                try
                {
                    // If the next result has already been broken out (or there are not multiple values)
                    // we can return right away.
                    if (_nextValue != null || !HasMultipleValues)
                    {
                        return _nextValue;
                    }

                    // Otherwise, break out any remaining values into a linked result.
                    BreakOutCurrentValue();

                    return _nextValue;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28978", ex);
                }
            }

            set
            {
                try
                {
                    // Ensure only one value is represented by the current result instance before
                    // assigning the value that should follow.
                    BreakOutCurrentValue();

                    _nextValue = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28979", ex);
                }
            }
        }

        /// <summary>
        /// Gets whether this <see cref="QueryResult"/> has no value.
        /// </summary>
        /// <returns><see langword="true"/> if this result has no value (either text or spatial),
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool IsEmpty
        {
            get
            {
                try
                {
                    if (IsAttribute || IsSpatial || IsObject || !string.IsNullOrEmpty(FirstString))
                    {
                        return false;
                    }
                    else if (HasMultipleValues)
                    {
                        return NextValue.IsEmpty;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28940", ex);
                }
            }
        }

        /// <summary>
        /// Gets whether multiple values are present.
        /// </summary>
        /// <returns><see langword="true"/> if multiple values are present; <see langword="false"/>
        /// otherwise.</returns>
        public bool HasMultipleValues
        {
            get
            {
                return _hasMultipleValues;
            }
        }

        /// <summary>
        /// Gets the <see cref="QueryNode"/> this result is associated with.
        /// </summary>
        public QueryNode QueryNode
        {
            get
            {
                return _queryNode;
            }
        }

        /// <summary>
        /// Gets the <see cref="MultipleQueryResultSelectionMode"/> mode associated with this result.
        /// </summary>
        /// <reutrns>The <see cref="MultipleQueryResultSelectionMode"/> mode associated with this
        /// result.</reutrns>
        public MultipleQueryResultSelectionMode SelectionMode
        {
            get
            {
                return _selectionMode;
            }

            set
            {
                _selectionMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SpatialMode"/> used by this result
        /// </summary>
        /// <value>
        /// The <see cref="SpatialMode"/> used by this result.
        /// </value>
        public SpatialMode SpatialMode
        {
            get
            {
                return (_queryNode == null) ? SpatialMode.Normal : _queryNode.SpatialMode;
            }
        }

        /// <summary>
        /// Gets the properties associated with this query node that have been specified via XML
        /// attributes.
        /// </summary>
        /// <value>The properties associated with this query node that have been specified via XML
        /// attributes.</value>
        public Dictionary<string, string> QueryNodeProperties
        {
            get
            {
                return (_queryNode == null) ? new Dictionary<string, string>() : _queryNode.Properties;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Removes all spatial info associated with all values of this <see cref="QueryResult"/>.
        /// </summary>
        public void RemoveSpatialInfo()
        {
            try
            {
                if (!IsEmpty)
                {
                    // We need _stringResults if it is not already created.
                    if (_stringResults == null)
                    {
                        _stringResults = new List<string>();
                    }

                    // Ensure FirstStringValue is in _stringResults.
                    if (_stringResults.Count == 0)
                    {
                        _stringResults.Add(FirstString);
                    }
                    else
                    {
                        _stringResults[0] = FirstString;
                    }

                    // Do the same for all remaining values.
                    if (NextValue != null)
                    {
                        NextValue.RemoveSpatialInfo();
                    }

                    // There is no longer any attribute or spatial info in this result.
                    _attributeResults = null;
                    _spatialResults = null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28914", ex);
            }
        }

        /// <summary>
        /// Removes all string info associated with all values of this <see cref="QueryResult"/>.
        /// Spatial info is maintained.
        /// </summary>
        public void RemoveStringInfo()
        {
            try
            {
                if (!IsEmpty)
                {
                    FirstString = "";

                    // Do the same for all remaining values.
                    if (NextValue != null)
                    {
                        NextValue.RemoveStringInfo();
                    }

                    // There is no longer any attribute or string info in this result.
                    _attributeResults = null;
                    _stringResults = new List<string>();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI34531", ex);
            }
        }

        /// <summary>
        /// Converts a result currently represented by a list of multiple values into a single
        /// string value where each item in the list is separated by the specified delimiter.
        /// </summary>
        /// <param name="delimiter">The <see langword="string"/> that should separate all values
        /// currently in the result. Can be <see langword="null"/> or empty if the values should
        /// all be run together.</param>
        public void ConvertToDelimitedStringList(string delimiter)
        {
            try
            {
                FirstString = string.Join(delimiter, ToStringArray());

                // We no longer have multiple results, so break off any linked values it
                // previously had.
                BreakOutCurrentValue();
                _nextValue = null;
                _hasMultipleValues = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28960", ex);
            }
        }

        /// <summary>
        /// Returns a <see langword="string"/> that represents the current
        /// <see cref="QueryResult"/>.
        /// </summary>
        /// <returns>A <see langword="string"/> that represents the current
        /// <see cref="QueryResult"/>.</returns>
        public override string ToString()
        {
            try
            {
                StringBuilder stringResult = new StringBuilder();
                string[] stringList = ToStringArray();
                foreach (string stringItem in stringList)
                {
                    stringResult.Append(stringItem);
                }

                return stringResult.ToString();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28915", ex);
            }
        }

        /// <summary>
        /// Returns a <see langword="SpatialString"/> that represents the current
        /// <see cref="QueryResult"/>.
        /// </summary>
        /// <returns>A <see langword="SpatialString"/> that represents the current
        /// <see cref="QueryResult"/>.</returns>
        public SpatialString ToSpatialString()
        {
            try
            {
                QueryResult combinedResult = new QueryResult(_queryNode);
                foreach (QueryResult result in this)
                {
                    QueryResult resultCopy = result.CreateFirstValueCopy();
                    combinedResult.AppendStringValue(resultCopy);
                }

                return combinedResult.FirstSpatialString;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28928", ex);
            }
        }

        /// <summary>
        /// Returns a list of <see langword="string"/>s that represents the current
        /// <see cref="QueryResult"/>.
        /// </summary>
        /// <returns>A list <see langword="string"/>s that represents the current
        /// <see cref="QueryResult"/>.</returns>
        public string[] ToStringArray()
        {
            try
            {
                List<string> stringList = new List<string>();

                if (!IsEmpty && _nextValue == null)
                {
                    if (_attributeResults != null)
                    {
                        foreach (IAttribute attribute in _attributeResults)
                        {
                            stringList.Add(attribute.Value.String);
                        }
                    }
                    else if (_spatialResults != null)
                    {
                        foreach (SpatialString spatialString in _spatialResults)
                        {
                            stringList.Add(spatialString.String);
                        }
                    }
                    else if (_queryNode.SplitCsv)
                    {
                        foreach(string line in _stringResults)
                        {
                            stringList.Add(SplitCSVLine(line)[0]);
                        }
                    }
                    else
                    {
                        stringList = _stringResults;
                    }
                }
                else if (!IsEmpty)
                {
                    stringList.Add(FirstString);

                    if (NextValue != null)
                    {
                        stringList.AddRange(NextValue.ToStringArray());
                    }
                }

                return stringList.ToArray();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28916", ex);
            }
        }

        /// <summary>
        /// Returns a list of <see langword="string"/>s that represents the current
        /// <see cref="QueryResult"/>.
        /// </summary>
        /// <returns>A list <see langword="string"/>s that represents the current
        /// <see cref="QueryResult"/>.</returns>
        public string[][] ToArrayOfStringArrays()
        {
            try
            {
                List<string[]> stringList = new List<string[]>();

                if (!IsEmpty && _nextValue == null)
                {
                    if (_attributeResults != null)
                    {
                        foreach (IAttribute attribute in _attributeResults)
                        {
                            stringList.Add(new[] { attribute.Value.String });
                        }
                    }
                    else if (_spatialResults != null)
                    {
                        foreach (SpatialString spatialString in _spatialResults)
                        {
                            stringList.Add(new[] { spatialString.String });
                        }
                    }
                    else if (_queryNode.SplitCsv)
                    {
                        stringList.AddRange(_stringResults.Select(SplitCSVLine));
                    }
                    else
                    {
                        stringList = _stringResults.Select(s => new[] { s }).ToList();
                    }
                }
                else if (!IsEmpty)
                {
                    stringList.Add(new[] { FirstString });

                    if (NextValue != null)
                    {
                        stringList.AddRange(NextValue.ToArrayOfStringArrays());
                    }
                }

                return stringList.ToArray();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45369");
            }
        }

        /// <summary>
        /// Returns a list of <see langword="IAttribute"/>s that represents the current
        /// <see cref="QueryResult"/>.
        /// </summary>
        /// <returns>A list <see langword="IAttribute"/>s that represents the current
        /// <see cref="QueryResult"/>.</returns>
        public IAttribute[] ToAttributeArray()
        {
            try
            {
                if (_nextValue == null)
                {
                    return _attributeResults.ToArray();
                }
                else
                {
                    ExtractException.Assert("ELI28917",
                        "Query result does not contain any attribute info!", IsAttribute);

                    List<IAttribute> attributeList = new List<IAttribute>();
                    attributeList.Add(_attributeResults[0]);
                    attributeList.AddRange(NextValue.ToAttributeArray());

                    return attributeList.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28919", ex);
            }
        }

        /// <overrides>
        /// Combines the given results into a single result.
        /// </overrides>
        /// <summary>
        /// Combines the given results into a single result.
        /// </summary>
        /// <param name="queryNode">The query node.</param>
        /// <param name="results">The results.</param>
        /// <returns>The combined <see cref="QueryResult"/>.</returns>
        public static QueryResult Combine(QueryNode queryNode, IEnumerable<QueryResult> results)
        {
            try
            {
                QueryResult combinedResult = Combine(results);

                combinedResult._queryNode = queryNode;
                combinedResult._selectionMode = (queryNode == null)
                    ? MultipleQueryResultSelectionMode.None
                    : queryNode.SelectionMode;

                return combinedResult;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34681");
            }
        }

        /// <summary>
        /// Combines the given results into a single result.
        /// <para><b>Note</b></para>
        /// The original results may be altered and should no longer be used.
        /// </summary>
        /// <param name="results">The query <see cref="QueryResult"/>s to combine.</param>
        /// <returns>The combined <see cref="QueryResult"/>.</returns>
        public static QueryResult Combine(IEnumerable<QueryResult> results)
        {
            try
            {
                return results.Aggregate(new QueryResult(results.First()._queryNode),
                        (result, next) => QueryResult.Combine(result, next));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34532");
            }
        }
        

        /// <summary>
        /// Combines the given results into a single result.
        /// <para><b>Note</b></para>
        /// The original results may be altered and should no longer be used.
        /// </summary>
        /// <param name="resultA">The first <see cref="QueryResult"/> to be combined.</param>
        /// <param name="resultB">The second <see cref="QueryResult"/> to be combined.</param>
        /// <returns>The <see cref="QueryResult"/> representing the combination of
        /// <see paramref="resultA"/> and <see paramref="resultB"/>.</returns>
        public static QueryResult Combine(QueryResult resultA, QueryResult resultB)
        {
            try
            {
                // If resultA is empty, simply use resultB (and vice-versa)
                if (resultA.IsEmpty)
                {
                    resultB._queryNode = resultA._queryNode;

                    return resultB;
                }
                else if (resultB.IsEmpty)
                {
                    return resultA;
                }

                // If neither result has been broken out into a linked list and the result types
                // are homogeneous, combine the corresponding lists.
                if (resultA._nextValue == null && resultB._nextValue == null)
                {
                    if (resultA.IsAttribute && resultB.IsAttribute)
                    {
                        resultA._attributeResults.AddRange(resultB._attributeResults);
                    }
                    else if (resultA.IsSpatial && resultB.IsSpatial)
                    {
                        resultA._spatialResults.AddRange(resultB._spatialResults);
                    }
                    else if (!resultA.IsAttribute && !resultA.IsSpatial &&
                             !resultB.IsAttribute && !resultB.IsSpatial &&
                             resultA._stringResults != null && resultB._stringResults != null)
                    {
                        resultA._stringResults.AddRange(resultB._stringResults);
                    }
                    else
                    {
                        QueryResult lastResultA = resultA
                            .Cast<QueryResult>()
                            .Last();
                        lastResultA._nextValue = resultB;
                    }

                    resultA._hasMultipleValues = true;
                }
                // Otherwise link the two lists together.
                else
                {
                    QueryResult lastResultA = resultA
                        .Cast<QueryResult>()
                        .Last();

                    lastResultA._nextValue = resultB;

                    resultA._hasMultipleValues = true;
                }

                if (resultB.SelectionMode == MultipleQueryResultSelectionMode.Distinct)
                {
                    resultA.SelectionMode = MultipleQueryResultSelectionMode.Distinct;
                }

                return resultA;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28920", ex);
            }
        }

        /// <summary>
        /// Appends the string value of the specified <see cref="QueryResult"/> to the end of the
        /// string result of this value.
        /// </summary>
        /// <param name="otherResult">The <see cref="QueryResult"/> whose string value should be
        /// appended.</param>
        public void AppendStringValue(QueryResult otherResult)
        {
            try
            {
                // If this string is spatial, preserve spatialness.
                if (IsSpatial)
                {
                    // If the other result has spatial info that should be persisted as well.
                    if (otherResult.IsSpatial)
                    {
                        // If both results have spatial info, but neither has a string value, 
                        // temporarily assign a string value, otherwise the SpatialString.Append
                        // call will downgrade the value to non-spatial.
                        if (string.IsNullOrEmpty(FirstString) && 
                            string.IsNullOrEmpty(otherResult.FirstString))
                        {
                            FirstSpatialString.ReplaceAndDowngradeToHybrid("Temporary");
                            FirstSpatialString.Append(otherResult.FirstSpatialString);
                            FirstSpatialString.ReplaceAndDowngradeToHybrid("");
                        }
                        // Otherwise, append the other result normally.
                        else
                        {
                            FirstSpatialString.Append(otherResult.FirstSpatialString);
                        }
                    }
                    // If the other result doesn't have any spatial info to persist, append its
                    // string value.
                    else
                    {
                        FirstSpatialString.AppendString(otherResult.FirstString);
                    }
                }
                // If the other result has spatial info to persist.
                else if (otherResult.IsSpatial)
                {
                    // Make sure _stringResult contains the current string value in case
                    // _spatialString exists but is non-spatial.
                    string stringValue = FirstString;

                    // To ensure other result's spatial info is preserved, assign a temporary string
                    // value if it does not have one... otherwise, the "Clone" call will remove the
                    // spatial info.
                    bool assignedTemporaryString = false;
                    if (string.IsNullOrEmpty(otherResult.FirstString))
                    {
                        assignedTemporaryString = true;
                        otherResult.FirstString = "Temporary";
                    }

                    // Initialize this result's _spatialResult as a clone of the other result.
                    _spatialResults = new List<SpatialString>();
                    _spatialResults.Add(otherResult.FirstSpatialString.Clone());

                    // Remove any temporary string from both the original and clone.
                    if (assignedTemporaryString)
                    {
                        otherResult.FirstString = "";
                        _spatialResults[0].ReplaceAndDowngradeToHybrid("");
                    }

                    // Insert the text of this result before any text from the other result.
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        FirstSpatialString.InsertString(0, stringValue);
                    }
                }
                // There is no spatial info to persist; simply append the other result's text.
                else
                {
                    FirstString += otherResult.FirstString;

                    // In case a spatial result existed but did not have spatial info to persist,
                    // null, it out now-- it is no longer needed.
                    _spatialResults = null;
                }

                // Once the spatial or string value has been modified an attributes in the attribute
                // list no longer defines the result's value. Break out this result from any
                // additional values and clear the attribute information.
                _attributeResults = null;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28922", ex);
            }
        }

        #endregion Methods

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumeration of the <see cref="QueryResult"/> values.
        /// </summary>
        /// <returns>An enumeration of the <see cref="QueryResult"/> values.</returns>
        public IEnumerator<QueryResult> GetEnumerator()
        {
            BreakOutCurrentValue();

            for (QueryResult result = this;
                 result != null && !result.IsEmpty;
                 result = result.NextValue)
            {
                yield return result;
            }
        }

        /// <summary>
        /// Returns an enumeration of the <see cref="QueryResult"/> values.
        /// </summary>
        /// <returns>An enumeration of the <see cref="QueryResult"/> values.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            BreakOutCurrentValue();

            for (QueryResult result = this;
                 result != null && !result.IsEmpty;
                 result = result.NextValue)
            {
                yield return result;
            }
        }

        #endregion IEnumerable Members

        #region Private Members

        /// <summary>
        /// Initializes the <see cref="QueryResult"/> given the specified list of values.
        /// </summary>
        /// <typeparam name="T">The list <see langword="Type"/></typeparam>
        /// <param name="queryNode">The <see cref="QueryNode"/> this result is to be associated
        /// with.</param>
        /// <param name="valuesList">The values that are to comprise this result.</param>
        void Initialize<T>(QueryNode queryNode, List<T> valuesList)
        {
            _queryNode = queryNode;
            _selectionMode = queryNode == null
                ? MultipleQueryResultSelectionMode.None
                : queryNode.SelectionMode;

            if (valuesList.Count > 1)
            {
                if (_selectionMode == MultipleQueryResultSelectionMode.First)
                {
                    valuesList.RemoveRange(1, valuesList.Count - 1);
                }
                else if (_selectionMode == MultipleQueryResultSelectionMode.None)
                {
                    valuesList.Clear();
                }
            }

            _hasMultipleValues = (valuesList.Count > 1);
        }

        /// <summary>
        /// Creates a <see cref="QueryNode"/> that represents the first value of this
        /// <see cref="QueryNode"/> instance.
        /// </summary>
        /// <returns>A <see cref="QueryNode"/> that represents the first value of this
        /// <see cref="QueryNode"/> instance.</returns>
        public QueryResult CreateFirstValueCopy()
        {
            try
            {
                QueryResult resultCopy;

                if (IsAttribute)
                {
                    resultCopy = new QueryResult(_queryNode, FirstAttribute);
                }
                else if (IsSpatial)
                {
                    resultCopy = new QueryResult(_queryNode, FirstSpatialString);
                }
                else if (IsObject)
                {
                    resultCopy = new QueryResult(_queryNode, _objectResult);
                }
                else
                {
                    resultCopy = new QueryResult(_queryNode, FirstString);
                }

                return resultCopy;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28927", ex);
            }
        }

        /// <summary>
        /// Separates the first value so that any additional values are accessed via a linked list
        /// of <see cref="QueryResult"/> instances.
        /// </summary>
        void BreakOutCurrentValue()
        {
            if (HasMultipleValues)
            {
                QueryResult newResultInstance = null;

                if (IsAttribute && _attributeResults.Count > 1)
                {
                    IAttribute[] remainingResults = new IAttribute[_attributeResults.Count - 1];
                    _attributeResults.CopyTo(1, remainingResults, 0, remainingResults.Length);
                    _attributeResults.RemoveRange(1, _attributeResults.Count - 1);

                    newResultInstance = new QueryResult(_queryNode, remainingResults);
                }
                else if (_spatialResults != null && _spatialResults.Count > 1)
                {
                    SpatialString[] remainingResults = new SpatialString[_spatialResults.Count - 1];
                    _spatialResults.CopyTo(1, remainingResults, 0, remainingResults.Length);
                    _spatialResults.RemoveRange(1, _spatialResults.Count - 1);

                    newResultInstance = new QueryResult(_queryNode, remainingResults);
                }
                else if (_stringResults != null && _stringResults.Count > 1)
                {
                    string[] remainingResults = new string[_stringResults.Count - 1];
                    _stringResults.CopyTo(1, remainingResults, 0, remainingResults.Length);
                    _stringResults.RemoveRange(1, _stringResults.Count - 1);

                    newResultInstance = new QueryResult(_queryNode, remainingResults);
                }

                if (newResultInstance != null)
                {
                    // Assign the next result reference.
                    _nextValue = newResultInstance;
                }
            }
        }

        #endregion Private Members
    }
}