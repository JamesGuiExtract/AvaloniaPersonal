using Extract.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Specifies the way in which multiple query results will be handled. This includes the initial
    /// selection of results from an individual query node as well as governing the way query
    /// results are combined with one another. Since a query result can either be a single value or
    /// a list of values, combining query nodes may result in one or more of the following actions:
    /// 1) Appending the string value from one query result to the string result of another.
    /// 2) Combining value lists from 2 query results into a single, larger list
    /// 3) Generating new query results to allow for different permutations of string values to be
    /// created.
    /// Primarily, these actions are governed by the use of 2 settings (List and Distinct).
    /// These two settings cause the exact same result when dealing with the result of a single
    /// query node, namely returning all values in a list. However, when combining with other query
    /// results while the "List" settings will seek to simply combine value lists, the "Distinct"
    /// setting will attempt to combine the string values from one query result in all possible ways
    /// with the values of another query result. The following grid outlines what happens when two
    /// query results, each with 2 values ([A,B] and [1,2]) are combined under the various 
    /// <see cref="MultipleQueryResultSelectionMode"/> settings. (Note that if either had the 
    /// "None" setting applied, both query results would already be empty).
    /// <code>
    ///                             [1,2]
    ///                 First       List        Distinct
    ///       First     [A1]        [A,1,2]     [A1,A2]
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
    internal class QueryResult : IEnumerable
    {
        #region Fields

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
        MultipleQueryResultSelectionMode _selectionMode;

        /// <summary>
        /// Contains named results that are stored for use by downstream queries. These are the
        /// original, unmodified results of the query node in which thery were named and are
        /// not a reference to any results to be output that may have since been modified.
        /// </summary>
        Dictionary<string, QueryResult> _namedResults = new Dictionary<string, QueryResult>();

        #endregion Fields
       
        #region Constructors

        /// <overloads>
        /// Initializes a new <see cref="QueryResult"/> instance.
        /// </overloads>
        /// <summary>
        /// Initializes a new, empty <see cref="QueryResult"/> instance.
        /// </summary>
        public QueryResult()
            : this(null, MultipleQueryResultSelectionMode.None, "")
        {
        }

        /// <summary>
        /// Initializes a new, empty <see cref="QueryResult"/> instance based on the provided
        /// instance.
        /// </summary>
        /// <param name="queryResult">The <see cref="QueryResult"/> the new instance should be
        /// modeled after. The new instance will be an exact copy except in the fact that it will
        /// not have access to the named results <see paramref="queryResult"/> did.</param>
        public QueryResult(QueryResult queryResult)
            : this()
        {
            try
            {
                if (queryResult.IsAttribute)
                {
                    _attributeResults = new List<IAttribute>(queryResult._attributeResults);
                    Initialize<IAttribute>(null, queryResult.SelectionMode, _attributeResults);
                }
                else if (queryResult.IsSpatial)
                {
                    _spatialResults = new List<SpatialString>(queryResult._spatialResults);
                    Initialize<SpatialString>(null, queryResult.SelectionMode, _spatialResults);
                }
                else
                {
                    _stringResults = new List<string>(queryResult._stringResults);
                    Initialize<string>(null, queryResult.SelectionMode, _stringResults);
                }

                if (queryResult._nextValue != null)
                {
                    _hasMultipleValues = true;
                    _nextValue = new QueryResult(queryResult._nextValue);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28981", ex);
            }
        }

        /// <summary>
        /// Initializes a new, empty, named <see cref="QueryResult"/> instance.
        /// </summary>
        /// <param name="name">If not <see langword="null"/>, downstream
        /// <see cref="DataEntryQuery"/>s will be able to access this result by name. Can be
        /// <see langword="null"/> if the results do not need to be accessed separately from the
        /// overal <see cref="QueryResult"/>.</param>
        public QueryResult(string name)
            : this(name, MultipleQueryResultSelectionMode.None, "")
        {
        }

        /// <summary>
        /// Initializes a new <see cref="QueryResult"/> instance.
        /// </summary>
        /// <param name="name">If not <see langword="null"/>, downstream
        /// <see cref="DataEntryQuery"/>s will be able to access this result by name. Can be
        /// <see langword="null"/> if the results do not need to be accessed separately from the
        /// overal <see cref="QueryResult"/>.</param>
        /// <param name="selectionMode">The <see cref="MultipleQueryResultSelectionMode"/> that will
        /// govern how this <see cref="QueryResult"/> is combined with others.</param>
        /// <param name="stringResults">One or more <see langword="string"/> values representing the
        /// result of a <see cref="DataEntryQuery"/>.</param>
        public QueryResult(string name, MultipleQueryResultSelectionMode selectionMode,
            params string[] stringResults)
        {
            try
            {
                _stringResults = new List<string>(stringResults);
                Initialize<string>(name, selectionMode, _stringResults);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28884", ex);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="QueryResult"/> instance.
        /// </summary>
        /// <param name="name">If not <see langword="null"/>, downstream
        /// <see cref="DataEntryQuery"/>s will be able to access this result by name. Can be
        /// <see langword="null"/> if the results do not need to be accessed separately from the
        /// overal <see cref="QueryResult"/>.</param>
        /// <param name="selectionMode">The <see cref="MultipleQueryResultSelectionMode"/> that will
        /// govern how this <see cref="QueryResult"/> is combined with others.</param>
        /// <param name="spatialResults">One or more <see cref="SpatialString"/> values representing
        /// the result of a <see cref="DataEntryQuery"/>.</param>
        public QueryResult(string name, MultipleQueryResultSelectionMode selectionMode,
            params SpatialString[] spatialResults)
        {
            try
            {
                _spatialResults = new List<SpatialString>(spatialResults);
                Initialize<SpatialString>(name, selectionMode, _spatialResults);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27060", ex);
            }
        }

        /// <summary>
        /// Initializes a new <see cref="QueryResult"/> instance.
        /// </summary>
        /// <param name="name">If not <see langword="null"/>, downstream
        /// <see cref="DataEntryQuery"/>s will be able to access this result by name. Can be
        /// <see langword="null"/> if the results do not need to be accessed separately from the
        /// overal <see cref="QueryResult"/>.</param>
        /// <param name="selectionMode">The <see cref="MultipleQueryResultSelectionMode"/> that will
        /// govern how this <see cref="QueryResult"/> is combined with others.</param>
        /// <param name="attributeResults">One or more <see cref="SpatialString"/> values representing
        /// the result of a <see cref="DataEntryQuery"/>.</param>
        public QueryResult(string name, MultipleQueryResultSelectionMode selectionMode,
            params IAttribute[] attributeResults)
        {
            try
            {
                _attributeResults = new List<IAttribute>(attributeResults);
                Initialize<IAttribute>(name, selectionMode, _attributeResults);
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
                    if (IsAttribute && _attributeResults[0].Value.HasSpatialInfo())
                    {
                        return true;
                    }
                    else if (_spatialResults != null && _spatialResults.Count > 0 &&
                        _spatialResults[0].HasSpatialInfo())
                    {
                        return true;
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
        public IAttribute FirstAttributeValue
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
        public SpatialString FirstSpatialStringValue
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
                        ICopyableObject copySource = (ICopyableObject)_attributeResults[0].Value;
                        _spatialResults.Add((SpatialString)copySource.Clone());
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
        public string FirstStringValue
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
                    if (FirstStringValue != value)
                    {
                        if (IsSpatial)
                        {
                            FirstSpatialStringValue.ReplaceAndDowngradeToHybrid(value);
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
                    if (IsSpatial || !string.IsNullOrEmpty(FirstStringValue))
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
                        _stringResults.Add(FirstStringValue);
                    }
                    else
                    {
                        _stringResults[0] = FirstStringValue;
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
                FirstStringValue = string.Join(delimiter, ToStringArray());

                // We no longer have multiple results, so break off any linked values it
                // previously had.
                BreakOutCurrentValue();
                _nextValue = null;
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
                QueryResult combinedResult = new QueryResult();
                foreach (QueryResult result in this)
                {
                    QueryResult resultCopy = result.CreateFirstValueCopy();
                    combinedResult.AppendStringValue(resultCopy);
                }

                return combinedResult.FirstSpatialStringValue;
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
                        foreach(IAttribute attribute in _attributeResults)
                        {
                            stringList.Add(attribute.Value.String);
                        }
                    }
                    else if (_spatialResults != null)
                    {
                        foreach(SpatialString spatialString in _spatialResults)
                        {
                            stringList.Add(spatialString.String);
                        }
                    }
                    else
                    {
                        stringList = _stringResults;
                    }
                }
                else if (!IsEmpty)
                {
                    stringList.Add(FirstStringValue);

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

        /// <summary>
        /// Retrieves a stored <see cref="QueryResult"/> by name.
        /// </summary>
        /// <param name="name">The name of the result to retrieve.</param>
        /// <returns>The <see cref="QueryResult"/> by the given name.</returns>
        public QueryResult GetNamedResult(string name)
        {
            try
            {
                // Throw an exception if its not there.
                return _namedResults[name];
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28918", ex);
                ee.AddDebugData("Result name", name, false);
                throw ee;
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
                // Ensure all named results are carried over into the return value.
                resultA.IncludeNamedResults(resultB);
                resultB._namedResults = resultA._namedResults;

                // If resultA is empty, simply use resultB (and vice-versa)
                if (resultA.IsEmpty)
                {
                    return resultB;
                }
                else if (resultB.IsEmpty)
                {
                    return resultA;
                }

                // Check for the case that multiple lists should be combined rather than combining
                // any string values.
                if ((resultA.SelectionMode != MultipleQueryResultSelectionMode.Distinct &&
                     (!resultB.HasMultipleValues ||
                      resultB.SelectionMode != MultipleQueryResultSelectionMode.Distinct)) ||
                    (resultB.SelectionMode != MultipleQueryResultSelectionMode.Distinct &&
                     (!resultA.HasMultipleValues ||
                      resultA.SelectionMode != MultipleQueryResultSelectionMode.Distinct)))
                {
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
                                 !resultA.IsAttribute && !resultA.IsSpatial &&
                                 resultA._stringResults != null && resultB._stringResults != null)
                        {
                            resultA._stringResults.AddRange(resultB._stringResults);
                        }
                        else
                        {
                            resultA._nextValue = resultB;
                        }
                    }
                    // Otherwise link the two lists together.
                    else
                    {
                        QueryResult lastResultA = resultA;
                        foreach (QueryResult nextResultA in resultA)
                        {
                            lastResultA = nextResultA;
                        }

                        lastResultA._nextValue = resultB;
                    }

                    resultA._hasMultipleValues = true;
                    return resultA;
                }

                // Combining the results may require creating new values. Keep track of them so
                // they can be added to the result.
                List<QueryResult> addedValues = new List<QueryResult>();

                // This will keep track of the current value from resultA being acted upon.
                QueryResult currentAValue = null;

                // Loop to combine value strings as necessary.
                foreach (QueryResult valueA in resultA)
                {
                    QueryResult valueACopy = null;

                    if (resultA.HasMultipleValues &&
                        resultB.HasMultipleValues &&
                        resultB.SelectionMode == MultipleQueryResultSelectionMode.Distinct)
                    {
                        if (valueA.SelectionMode == MultipleQueryResultSelectionMode.Distinct)
                        {
                            // Create a copy of valueA so that it can be combined separately with
                            // multiple values from B.
                            valueACopy = valueA.CreateFirstValueCopy();
                        }
                        else
                        {
                            // If value A is not distinct while B is, flatten valueA to a string
                            // so that its results can be combined with each.
                            valueA.FirstStringValue = valueA.ToString();

                            // We no longer have multiple results from A to iterate through, so break
                            // off any linked values it previously had.
                            valueA.BreakOutCurrentValue();
                            valueA._nextValue = null;
                        }
                    }
                    else if (resultA.HasMultipleValues &&
                             resultB.HasMultipleValues &&
                             resultA.SelectionMode == MultipleQueryResultSelectionMode.Distinct)
                    {
                        // If value A is distinct while B is not , flatten valueB to a string
                        // so that its results can be combined with each.
                        resultB.FirstStringValue = resultB.ToString();

                        // We no longer have multiple results from A to iterate through, so break
                        // off any linked values it previously had.
                        resultB.BreakOutCurrentValue();
                        resultB._nextValue = null;
                    }

                    // For each remaining value in resultB, perform a string append with the current
                    // value from A.
                    foreach (QueryResult valueB in resultB)
                    {
                        if (currentAValue == null)
                        {
                            // For the first value of resultB, use the original valueA.
                            currentAValue = valueA;
                        }
                        else 
                        {
                            // For any subsequent iterations, use a copy of the original valueA.
                            ExtractException.Assert("ELI28921", "Internal error!",
                                valueACopy != null);
                            currentAValue = valueACopy.CreateFirstValueCopy();
                            addedValues.Add(currentAValue);
                        }

                        currentAValue.AppendStringValue(valueB);
                    }
                }

                // Attach any new values created to resultA.                
                foreach (QueryResult addedResult in addedValues)
                {
                    currentAValue._nextValue = addedResult;
                    currentAValue = addedResult;
                    resultA._hasMultipleValues = true;
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
                        if (string.IsNullOrEmpty(FirstStringValue) && 
                            string.IsNullOrEmpty(otherResult.FirstStringValue))
                        {
                            FirstSpatialStringValue.ReplaceAndDowngradeToHybrid("Temporary");
                            FirstSpatialStringValue.Append(otherResult.FirstSpatialStringValue);
                            FirstSpatialStringValue.ReplaceAndDowngradeToHybrid("");
                        }
                        // Otherwise, append the other result normally.
                        else
                        {
                            FirstSpatialStringValue.Append(otherResult.FirstSpatialStringValue);
                        }
                    }
                    // If the other result doesn't have any spatial info to persist, append its
                    // string value.
                    else
                    {
                        FirstSpatialStringValue.AppendString(otherResult.FirstStringValue);
                    }
                }
                // If the other result has spatial info to persist.
                else if (otherResult.IsSpatial)
                {
                    // Make sure _stringResult contains the current string value in case
                    // _spatialString exists but is non-spatial.
                    string stringValue = FirstStringValue;

                    // To ensure other result's spatial info is preserved, assign a temporary string
                    // value if it does not have one... otherwise, the "Clone" call will remove the
                    // spatial info.
                    bool assignedTemporaryString = false;
                    if (string.IsNullOrEmpty(otherResult.FirstStringValue))
                    {
                        assignedTemporaryString = true;
                        otherResult.FirstStringValue = "Temporary";
                    }

                    // Initialize this result's _spatialResult as a clone of the other result.
                    ICopyableObject copySource = (ICopyableObject)otherResult.FirstSpatialStringValue;
                    _spatialResults = new List<SpatialString>();
                    _spatialResults.Add((SpatialString)copySource.Clone());

                    // Remove any temporary string from both the original and clone.
                    if (assignedTemporaryString)
                    {
                        otherResult.FirstStringValue = "";
                        _spatialResults[0].ReplaceAndDowngradeToHybrid("");
                    }

                    // Insert the text of this result before any text from the other result.
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        FirstSpatialStringValue.InsertString(0, stringValue);
                    }
                }
                // There is no spatial info to persist; simply append the other result's text.
                else
                {
                    FirstStringValue += otherResult.FirstStringValue;

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

        /// <summary>
        /// Applies all named results from the specified <see cref="QueryResult"/> to this instance.
        /// </summary>
        /// <param name="otherResults">The <see cref="QueryResult"/> containing named results that
        /// should be shared with this instance.</param>
        public void IncludeNamedResults(params QueryResult[] otherResults)
        {
            try
            {
                foreach (QueryResult otherResult in otherResults)
                {
                    if (otherResult == null)
                    {
                        continue;
                    }

                    foreach (KeyValuePair<string, QueryResult> otherNamedResult
                                in otherResult._namedResults)
                    { 
                        if (!_namedResults.ContainsKey(otherNamedResult.Key))
                        {
                            _namedResults[otherNamedResult.Key] = otherNamedResult.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28980", ex);
            }
        }

        /// <summary>
        /// Creates a named result from the current instance.
        /// </summary>
        /// <param name="name">The name the result should be accessed with.</param>
        public void CreateNamedResult(string name)
        {
            try
            {
                ExtractException.Assert("ELI28977", "Query result name not specified!",
                    !string.IsNullOrEmpty(name));

                _namedResults[name] = new QueryResult(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29182", ex);
            }
        }

        #endregion Methods

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the values of the <see cref="QueryResult"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="QueryResult"/>.</returns>
        public IEnumerator GetEnumerator()
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
        /// <param name="name">If not <see langword="null"/>, downstream
        /// <see cref="DataEntryQuery"/>s will be able to access this result by name. Can be
        /// <see langword="null"/> if the results do not need to be accessed separately from the
        /// overal <see cref="QueryResult"/>.</param>
        /// <param name="selectionMode">The <see cref="MultipleQueryResultSelectionMode"/> to be
        /// associated with this result.</param>
        /// <param name="valuesList">The values that are to comprise this result.</param>
        void Initialize<T>(string name, MultipleQueryResultSelectionMode selectionMode, List<T> valuesList)
        {
            _selectionMode = selectionMode;

            if (valuesList.Count > 1)
            {
                if (selectionMode == MultipleQueryResultSelectionMode.First)
                {
                    valuesList.RemoveRange(1, valuesList.Count - 1);
                }
                else if (selectionMode == MultipleQueryResultSelectionMode.None)
                {
                    valuesList.Clear();
                }
            }

            _hasMultipleValues = (valuesList.Count > 1);

            if (!string.IsNullOrEmpty(name))
            {
                CreateNamedResult(name);
            }
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
                    resultCopy = new QueryResult(null, SelectionMode, FirstAttributeValue);
                }
                else if (IsSpatial)
                {
                    resultCopy = new QueryResult(null, SelectionMode, FirstSpatialStringValue);
                }
                else
                {
                    resultCopy = new QueryResult(null, SelectionMode, FirstStringValue);
                }

                // Don't copy _namedResults... each named result should be available within the
                // the same "Distinct" result.

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

                // When creating the "next" result, limit the selection mode to either "List" or
                // "Distinct" since these are the only two settings relevant after the result was
                // initially created and we don't want the selection mode to limit the number of
                // values that already exist in this result.
                MultipleQueryResultSelectionMode selectionMode =
                    (SelectionMode == MultipleQueryResultSelectionMode.Distinct)
                        ? MultipleQueryResultSelectionMode.Distinct
                        : MultipleQueryResultSelectionMode.List;

                if (IsAttribute)
                {
                    IAttribute[] remainingResults = new IAttribute[_attributeResults.Count - 1];
                    _attributeResults.CopyTo(1, remainingResults, 0, remainingResults.Length);
                    _attributeResults.RemoveRange(1, _attributeResults.Count - 1);

                    newResultInstance = new QueryResult(null, selectionMode, remainingResults);
                }
                else if (_spatialResults != null && _spatialResults.Count > 1)
                {
                    SpatialString[] remainingResults = new SpatialString[_spatialResults.Count - 1];
                    _spatialResults.CopyTo(1, remainingResults, 0, remainingResults.Length);
                    _spatialResults.RemoveRange(1, _spatialResults.Count - 1);

                    newResultInstance = new QueryResult(null, selectionMode, remainingResults);
                }
                else if (_stringResults != null && _stringResults.Count > 1)
                {
                    string[] remainingResults = new string[_stringResults.Count - 1];
                    _stringResults.CopyTo(1, remainingResults, 0, remainingResults.Length);
                    _stringResults.RemoveRange(1, _stringResults.Count - 1);

                    newResultInstance = new QueryResult(null, selectionMode, remainingResults);
                }

                if (newResultInstance != null)
                {
                    // Pass on any named results.
                    newResultInstance.IncludeNamedResults(this);

                    // Assign the next result reference.
                    _nextValue = newResultInstance;
                }
            }
        }

        #endregion Private Members
    }
}
