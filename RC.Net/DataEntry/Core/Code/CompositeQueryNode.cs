using Extract.Imaging;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Xml;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;
using System.Runtime.Serialization;

namespace Extract.DataEntry
{
    /// <summary>
    /// An exception indicating that the current evaluation should be aborted and an empty
    /// result should be returned.
    /// </summary>
    // This exception is to be created only by data queries. A standard set of constructors is not
    // necessary.
    [Serializable] 
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class QueryAbortEvaluationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAbortEvaluationException"/> class.
        /// </summary>
        // This exception is to be created only by data queries. External access to constructor is
        // not necessary.
        [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
        internal QueryAbortEvaluationException()
            : base()
        {
        }
    }

    /// <summary>
    /// A <see cref="QueryNode"/> that is comprised of one or more child
    /// <see cref="QueryNode"/>s.
    /// </summary>
    public class CompositeQueryNode : QueryNode, IDisposable
    {
        /// <summary>
        /// Represents a distinct combination of results from sub-queries. When one or more
        /// sub-queries use the distinct selection mode and return multiple results,
        /// there will be multiple instances returned from <see cref="GetChildNodeResults"/>
        /// for a given evaluation.
        /// </summary>
        class DistinctResultSet
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DistinctResultSet"/> class.
            /// </summary>
            public DistinctResultSet()
            {
                ResultList = new List<QueryResult>();
            }

            /// <summary>
            /// Gets or sets the results of the sub-queries for this set.
            /// </summary>
            public List<QueryResult> ResultList
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the forced spatial result for this set.
            /// </summary>
            public SpatialString ForcedSpatialResult
            {
                get;
                set;
            }
        }

        #region Fields

        /// <summary>
        /// The child QueryNodes of this query node.
        /// </summary>
        Collection<QueryNode> _childNodes = new Collection<QueryNode>();

        /// <summary>
        /// The <see cref="IAttribute"/> that should be considered the root of any attribute query.
        /// </summary>
        IAttribute _rootAttribute;

        /// <summary>
        /// The <see cref="DbConnection"/>(s) that should be used to evaluate any SQL queries;
        /// The key is the connection name (blank for default connection).
        /// </summary>
        Dictionary<string, DbConnection> _dbConnections;

        /// <summary>
        /// The named <see cref="QueryNode"/>s which this <see cref="QueryNode"/> references.
        /// </summary>
        Dictionary<string, QueryNode> _namedDependencies = new Dictionary<string, QueryNode>();

        /// <summary>
        /// Protects against recursion in <see cref="HandleQueryValueModified"/>
        /// </summary>
        bool _handlingQueryValueChange;

        /// <summary>
        /// Indicates whether the query node is currently being evaluated in which case the
        /// <see cref="QueryValueModified"/> event can be suppressed.
        /// </summary>
        bool _evaluating;

        #endregion Fields

        #region Delegates

        /// <summary>
        /// The method to use to evaluate the query using the provided
        /// <see paramref="childQueryResults"/>.
        /// </summary>
        /// <param name="childQueryResults"></param>
        /// <returns></returns>
        delegate QueryResult EvaluateChildNodes(IEnumerable<QueryResult> childQueryResults);

        #endregion Delegates

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="CompositeQueryNode"/> instance.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnections">The <see cref="DbConnection"/>(s) that should be used
        /// to evaluate any SQL queries; The key is the connection name (blank for default
        /// connection).</param>
        public CompositeQueryNode(IAttribute rootAttribute,
            Dictionary<string, DbConnection> dbConnections)
            : base()
        {
            try
            {
                _rootAttribute = rootAttribute;
                _dbConnections = dbConnections;

                AttributeStatusInfo.QueryCacheCleared += HandleQueryCacheCleared;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28865", ex);
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when the data behind the query has changed thereby likely changing the query result.
        /// </summary>
        public event EventHandler<QueryValueModifiedEventArgs> QueryValueModified;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets the child <see cref="QueryNode"/>s of this query node.
        /// </summary>
        protected Collection<QueryNode> ChildNodes
        {
            get
            {
                return _childNodes;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IAttribute"/> that should be considered the root of any
        /// attribute query.
        /// </summary>
        /// <value>The <see cref="IAttribute"/> that should be considered the root of any
        /// attribute query.</value>
        public virtual IAttribute RootAttribute
        {
            get
            {
                return _rootAttribute;
            }

            set
            {
                _rootAttribute = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="DbConnection"/>(s) that should be used to evaluate any SQL queries.
        /// </summary>
        /// <returns>The <see cref="DbConnection"/>(s) that should be used to evaluate any SQL
        /// queries; The key is the connection name (blank for default connection).
        /// </returns>
        public virtual Dictionary<string, DbConnection> DatabaseConnections
        {
            get
            {
                return _dbConnections;
            }
        }

        /// <summary>
        /// A previously calculated result that is still accurate.
        /// </summary>
        protected QueryResult CachedResult
        {
            get;
            set;
        }

        /// <summary>
        /// A previously calculated result that is still accurate.
        /// </summary>
        protected QueryResult CachedBaseResult
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Links the all <see cref="QueryNode"/> referenced by <see paramref="resultName"/> to
        /// the <see cref="QueryNode"/>s that reference them.
        /// </summary>
        /// <param name="namedReferences">A communal collection of named <see cref="QueryNode"/>s
        /// available to allow referencing of named nodes by subsequent nodes.</param>
        internal static void RegisterNamedDependencies(
            Dictionary<string, NamedQueryReferences> namedReferences)
        {
            try
            {
                foreach (KeyValuePair<string, NamedQueryReferences> namedReference in namedReferences)
                {
                    string resultName = namedReference.Key;
                    QueryNode referencedQuery = namedReference.Value.ReferencedQuery;

                    // If there is no ReferencedQuery by the time this is called, there is a syntax
                    // error.
                    if (referencedQuery == null)
                    {
                        ExtractException ee = new ExtractException("ELI26726",
                            "Unrecognized element in query.");
                        ee.AddDebugData("Element Name", resultName, true);
                        throw ee;
                    }

                    // Register each of the referencing queries to receive notifications of changes
                    // to the referenced query.
                    var compositeNamedQuery = referencedQuery as CompositeQueryNode;
                    foreach (CompositeQueryNode referencingQuery in
                        namedReference.Value.ReferencingQueries)
                    {
                        if (compositeNamedQuery != null)
                        {
                            // [DataEntry:1238]
                            // The referencing query may have already been evaluated without the
                            // reference having yet been hooked up. Ensure the referencingQuery is
                            // initialized using the current value of referencedQuery.
                            referencingQuery.HandleQueryValueModified(
                                referencedQuery, new QueryValueModifiedEventArgs(false));

                            compositeNamedQuery.QueryValueModified +=
                                referencingQuery.HandleQueryValueModified;
                        }

                        referencingQuery._namedDependencies[resultName] = referencedQuery;
                        referencingQuery.ChildNodes.Add(referencedQuery);
                    }

                    namedReference.Value.ReferencingQueries.Clear();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34499");
            }
        }

        /// <summary>
        /// Retrieves the <see cref="QueryResult"/> for the <see cref="QueryNode"/> of the specified
        /// <see paramref="name"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="QueryNode"/> for which a result is needed.
        /// </param>
        /// <returns>The <see cref="QueryResult"/> for the <see cref="QueryNode"/>.</returns>
        protected QueryResult GetNamedResult(string name)
        {
            try
            {
                QueryNode referencedQueryNode = _namedDependencies[name];

                if (referencedQueryNode.SelectionMode == MultipleQueryResultSelectionMode.Distinct)
                {
                    return new QueryResult(this, referencedQueryNode.DistinctResult);
                }
                else
                {
                    return new QueryResult(this, _namedDependencies[name].Evaluate());
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34487");
            }
        }

        /// <summary>
        /// Pushes the <see paramref="distinctResult"/> to use for the current evaluation scope.
        /// </summary>
        /// <param name="distinctResult">The <see cref="QueryResult"/> to use as the distinct value
        /// for the current execution scope.</param>
        /// <returns><see langword="true"/> if the query result was modified as a result; otherwise,
        /// <see langword="false"/>.</returns>
        internal override bool PushDistinctResult(QueryResult distinctResult)
        {
            try
            {
                if (base.PushDistinctResult(distinctResult))
                {
                    OnQueryValueModified(new QueryValueModifiedEventArgs(false));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34665");
            }
        }

        /// <summary>
        /// Pops the distinct <see cref="QueryResult"/> that had been active for the evaluation
        /// scope that is ending.
        /// </summary>
        /// <returns><see langword="true"/> if the query result was modified as a result; otherwise,
        /// <see langword="false"/>.</returns>
        internal override bool PopDistinctResult()
        {
            try
            {
                if (base.PopDistinctResult())
                {
                    OnQueryValueModified(new QueryValueModifiedEventArgs(false));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34666");
            }
        }

        #endregion Methods

        #region Virtuals

        /// <summary>
        /// Evaluates the query by combining all child <see cref="QueryNode"/>s.
        /// </summary>
        /// <param name="childQueryResults"><see cref="QueryResult"/>s representing the results of
        /// each child <see cref="QueryNode"/>.</param>
        /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
        protected virtual QueryResult Evaluate(IEnumerable<QueryResult> childQueryResults)
        {
            try
            {
                return QueryResult.Combine(this, childQueryResults);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34483");
            }
        }

        /// <summary>
        /// Raises the <see cref="QueryValueModified"/> event.
        /// </summary>
        /// <param name="e">An <see cref="QueryValueModifiedEventArgs"/> that contains the event
        /// data.</param>
        protected virtual void OnQueryValueModified(QueryValueModifiedEventArgs e)
        {
            if (this.QueryValueModified != null)
            {
                QueryValueModified(this, e);
            }
        }

        /// <summary>
        /// Handles the trigger attribute deleted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="QueryValueModifiedEventArgs"/> instance containing the
        /// event data.</param>
        protected virtual void HandleQueryValueModified(object sender, QueryValueModifiedEventArgs e)
        {
            // To cut down on some of the thrashing of QueryValueModified events (especially when
            // dealing with nodes with distinct selection mode which will re-evaluate multiple times
            // during the same evaluation of the parent node, ignore any QueryValueModified events
            // that occur during the evaluation of this node (because it is already evaluating,
            // there is not point to handling or forwarding this event).
            if (_evaluating)
            {
                return;
            }

            // [DataEntry:1133]
            // Even when _handlingQueryValueChange is true, clear the CachedResult if the attribute
            // has been modified to ensure any currently executing query returns an accurate result.
            // Do no re-raise OnQueryValueModified, however, which can lead to infinite recursion.
            CachedResult = null;

            // Prevent recursion that can occur if there are references to a distinct node.
            if (_handlingQueryValueChange)
            {
                return;
            }

            try
            {
                _handlingQueryValueChange = true;

                OnQueryValueModified(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34504");
            }
            finally
            {
                _handlingQueryValueChange = false;
            }
        }

        /// <summary>
        /// Handles the query cache cleared event
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void HandleQueryCacheCleared(object sender, EventArgs e)
        {
            CachedResult = null;
        }

        #endregion Virtuals

        #region Overrides

        /// <summary>
        /// Loads the <see cref="QueryNode"/> using the specified XML query string.
        /// </summary>
        /// <param name="xmlNode">The XML query string defining the query.</param>
        /// <param name="namedReferences">A communal collection of named
        /// <see cref="NamedQueryReferences"/>s available to allow referencing of named nodes.</param>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        internal override void LoadFromXml(XmlNode xmlNode,
            Dictionary<string, NamedQueryReferences> namedReferences)
        {
            try
            {
                QueryText = xmlNode.InnerXml;

                base.LoadFromXml(xmlNode, namedReferences);

                // Iterate through each child of the current XML node and use each to initialize a
                // new child QueryNode.
                foreach (XmlNode childNode in xmlNode.ChildNodes)
                {
                    // If the node is text, initialize a LiteralQueryNode node.
                    if (childNode.NodeType == XmlNodeType.Text)
                    {
                        // Any literal text nodes should respect this node's
                        // TreatNewLinesAsWhiteSpace setting.
                        QueryNode childQueryNode = new LiteralQueryNode(childNode.InnerText);
                        childQueryNode.TreatNewLinesAsWhiteSpace = TreatNewLinesAsWhiteSpace;
                        ChildNodes.Add(childQueryNode);
                    }
                    else if (childNode.NodeType == XmlNodeType.Element)
                    {
                        QueryNode childQueryNode = null;
                        CompositeQueryNode compositeChildNode = null;
                        XmlElement childElement = ((XmlElement)childNode);

                        // Check for SourceDocName (which is not a CompositeQueryNode). 
                        if (childElement.Name.Equals("SourceDocName",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            childQueryNode = new SourceDocNameQueryNode();
                        }
                        else if (childElement.Name.Equals("SolutionDirectory",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            childQueryNode = new SolutionDirectoryQueryNode();
                        }
                        else if (childElement.Name.Equals("SQL",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            childQueryNode =
                                new SqlQueryNode(RootAttribute, DatabaseConnections);
                        }
                        else if (childElement.Name.Equals("Attribute",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            childQueryNode =
                                new AttributeQueryNode(RootAttribute, DatabaseConnections);
                        }
                        // Maintain "Complex" keyword for compatibility with versions <= 9.0
                        else if (childElement.Name.Equals("Complex",
                            StringComparison.OrdinalIgnoreCase) ||
                        childElement.Name.Equals("Composite",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            childQueryNode =
                                new CompositeQueryNode(RootAttribute, DatabaseConnections);
                        }
                        else if (childElement.Name.Equals("Regex",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            childQueryNode =
                                new RegexQueryNode(RootAttribute, DatabaseConnections);
                        }
                        else if (childElement.Name.Equals("Expression",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            childQueryNode =
                                new ExpressionQueryNode(RootAttribute, DatabaseConnections);
                        }
                        else if (childElement.Name.Equals("DbConnection",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            childQueryNode = new DbConnectionQueryNode(RootAttribute, DatabaseConnections);
                        }
                        else
                        {
                            // If the node name matches the name of one of the name QueryNodes,
                            // create this node as a single-argument ResultQueryNode.
                            childQueryNode = new ResultQueryNode(childElement.Name, namedReferences);
                        }

                        childQueryNode.LoadFromXml(childNode, namedReferences);

                        if (!string.IsNullOrEmpty(childQueryNode.Name))
                        {
                            _namedDependencies[childQueryNode.Name] = childQueryNode;
                        }

                        if (compositeChildNode == null)
                        {
                            compositeChildNode = childQueryNode as CompositeQueryNode;
                        }

                        if (compositeChildNode != null)
                        {
                            compositeChildNode.QueryValueModified += HandleQueryValueModified;
                        }

                        ChildNodes.Add(childQueryNode);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28958", ex);
            }
        }

        /// <summary>
        /// Evaluates the query by combining all child <see cref="QueryNode"/>s.
        /// </summary>
        /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
        public override QueryResult Evaluate()
        {
            try
            {
                _evaluating = true;

                // If the selection mode is "Distinct" and a distinct result has been applied, the
                // distinct section defined by this node is currently being evaluated, meaning this
                // node has been referenced by another node in this distinct section. Return the
                // active distinct result in this case, rather than the complete list of results.
                if (SelectionMode == MultipleQueryResultSelectionMode.Distinct &&
                    DistinctResult != null)
                {
                    return new QueryResult(this, DistinctResult);
                }

                // If this query's data has not been modified since the last call to evaluate,
                // just return a copy of the original result.
                if (CachedResult != null)
                {
                    return new QueryResult(this, CachedResult);
                }

                Queue<QueryNode> childNodeQueue = new Queue<QueryNode>(ChildNodes);
                IEnumerable<DistinctResultSet> childQueryResults = GetChildNodeResults(childNodeQueue);

                List<QueryResult> resultList = new List<QueryResult>();
                SpatialString forcedSpatialResult = null;
                foreach (DistinctResultSet resultSet in childQueryResults)
                {
                    resultList.Add(Evaluate(resultSet.ResultList));

                    if (resultSet.ForcedSpatialResult != null)
                    {
                        if (forcedSpatialResult == null)
                        {
                            forcedSpatialResult = resultSet.ForcedSpatialResult;
                        }
                        else
                        {
                            forcedSpatialResult.DataEntryMergeAsHybridString(resultSet.ForcedSpatialResult);
                        }
                    }
                }

                // https://extract.atlassian.net/browse/ISSUE-12439
                // There are cases where child query nodes need to be aware of when the overall
                // evaluation of their parent is complete, such as when an attribute query node
                // with a selection mode of distinct is evaluated so that it can know all attributes
                // that were referenced, not just the most recently evaluated one.
                foreach (QueryNode childNode in ChildNodes)
                {
                    childNode.NotifyParentEvaluationComplete();
                }

                // [DataEntry:1225]
                // If there is a single result that is the combination of multiple child nodes, and
                // at least one of the child nodes is using the distinct selection mode, combine the
                // results from the result set as a single result. This prevents a query that
                // returns distinct results from behaving differently when there is only one
                // result set versus multiple.
                bool flatternDistinctResult = resultList.Count == 1 && ChildNodes.Count > 1 &&
                    ChildNodes.Any(node => node.SelectionMode == MultipleQueryResultSelectionMode.Distinct);
                    
                // The resultList may contain > 1 results if the selection mode of any QueryNode was
                // distinct. (Note each result may contain multiple values).
                // If there is more than 1 result or the result is to be handled as a single distinct
                // result, discard any spatial or attribute values and instead convert each result
                // into a simple string.
                QueryResult result = (resultList.Count == 1 && !flatternDistinctResult)
                    ? resultList.First()
                    : new QueryResult(this, resultList
                        .Select(distinctResult => distinctResult.ToString()).ToArray());

                // Flatten the result into a delimited string if specified.
                if (StringListDelimiter != null)
                {
                    result.ConvertToDelimitedStringList(StringListDelimiter);
                }

                if (result.HasMultipleValues && !string.IsNullOrEmpty(result.ToString()))
                {
                    if (SelectionMode == MultipleQueryResultSelectionMode.None)
                    {
                        result = new QueryResult(this);
                    }
                    else if (SelectionMode == MultipleQueryResultSelectionMode.First)
                    {
                        result = result.CreateFirstValueCopy();
                    }
                }

                // Apply the spatial information of child nodes with "Force" spatial mode if necessary.
                if (forcedSpatialResult != null && forcedSpatialResult.HasSpatialInfo())
                {
                    List<SpatialString> spatialResults = new List<SpatialString>();

                    if (result.HasMultipleValues)
                    {
                        foreach (QueryResult subResult in result)
                        {
                            SpatialString spatialString = forcedSpatialResult.Clone();
                            spatialString.ReplaceAndDowngradeToHybrid(subResult.FirstString);
                            spatialResults.Add(spatialString);
                        }
                    }
                    else
                    {
                        forcedSpatialResult.ReplaceAndDowngradeToHybrid(result.FirstString);
                        spatialResults.Add(forcedSpatialResult);
                    }

                    result = new QueryResult(this, spatialResults.ToArray());
                }

                if (result.IsSpatial)
                {
                    if (result.SpatialMode.HasFlag(SpatialMode.None))
                    {
                        result.RemoveSpatialInfo();
                    }
                    else if (result.SpatialMode.HasFlag(SpatialMode.IfNotBlank) &&
                            string.IsNullOrEmpty(result.ToString()))
                    {
                        result.RemoveSpatialInfo();
                    }
                }

                if (result.SpatialMode.HasFlag(SpatialMode.Only) &&
                    !string.IsNullOrEmpty(result.ToString()))
                {
                    result.RemoveStringInfo();
                }

                // If specified, turn spatial or attribute field into a string result.
                if (SpatialField != null)
                {
                    result = GetSpatialFieldValue(result);
                }
                else if (AttributeField != null)
                {
                    result = GetAttributeFieldValue(result);
                }

                // Cache the result for efficiency in the case of repeated calls.
                CachedResult = new QueryResult(this, result);

                return result;
            }
            catch (QueryAbortEvaluationException)
            {
                // [DataEntry:1126]
                // If the evaluation of this node has been aborted, return an empty result.
                return new QueryResult(this);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26729", ex);
                ee.AddDebugData("Query node type", GetType().Name, true);
                ee.AddDebugData("Query", QueryText ?? "null", true);
                throw ee;
            }
            finally
            {
                _evaluating = false;
            }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Retrieves the results from evaluating the specified <see paramref="queryNodes"/>s. If
        /// one or more of these nodes (or their child nodes) have a selection mode of distinct, a
        /// different <see cref="DistinctResultSet"/> will be returned for each distinct value.
        /// </summary>
        /// <param name="queryNodes">The <see cref="QueryNode"/>s which need to be evaluated.</param>
        /// <returns>Retrieves the results from evaluating the specified <see paramref="queryNodes"/>s.
        /// </returns>
        IEnumerable<DistinctResultSet> GetChildNodeResults(Queue<QueryNode> queryNodes)
        {
            // If there are no specified nodes, there is nothing to evaluate; break from the
            // enumeration.
            if (queryNodes.Count == 0)
            {
                yield break;
            }

            // Get the first query node and evaluate it.
            QueryNode queryNode = queryNodes.Dequeue();
            QueryResult queryResult = queryNode.Evaluate();

            // [DataEntry:1126]
            // If the AbortIfEmpty flag is set and the result is empty, abort the evaluation (ignore
            // the result of all other nodes).
            if (queryResult.IsEmpty && queryNode.AbortIfEmpty)
            {
                throw new QueryAbortEvaluationException();
            }

            // Flatten the result into a delimited string if specified.
            if (queryNode.StringListDelimiter != null)
            {
                queryResult.ConvertToDelimitedStringList(queryNode.StringListDelimiter);
            }

            // Iterate through each result that is to be evaluated as a distinct result.
            foreach (QueryResult distinctResult in GetDistinctResults(queryResult))
            {
                // If in distinct mode, push the current DistinctResult so that if this node is
                // referenced by another query, it will return the result currently being evaluated.
                if (queryResult.SelectionMode == MultipleQueryResultSelectionMode.Distinct)
                {
                    queryNode.PushDistinctResult(distinctResult);
                }
                else
                {
                    queryNode.PushDistinctResult(null);
                }
                
                // If the current child has a spatial mode of "Force", store its spatial
                // info so it can be applied to the query result later.
                SpatialString forcedSpatialResult = null;
                if (distinctResult.IsSpatial)
                {
                    if (distinctResult.SpatialMode.HasFlag(SpatialMode.None))
                    {
                        distinctResult.RemoveSpatialInfo();
                    }
                    else if (distinctResult.SpatialMode.HasFlag(SpatialMode.IfNotBlank) &&
                        string.IsNullOrEmpty(distinctResult.ToString()))
                    {
                        distinctResult.RemoveSpatialInfo();
                    }
                    else
                    {
                        if (distinctResult.SpatialMode.HasFlag(SpatialMode.Force))
                        {
                            SpatialString spatialString = distinctResult.ToSpatialString();
                            IUnknownVector vecRasterZones = spatialString.GetOCRImageRasterZones();

                            forcedSpatialResult = spatialString.Clone();

                            // Because the SpatialString class will remove spatial info for
                            // blank strings yet it is important for the attribute value spatial
                            // mode to match the spatial mode of the SpatialString value, add
                            // back any spatial info that was removed as part of the Clone or
                            // Append methods.
                            if (!forcedSpatialResult.HasSpatialInfo())
                            {
                                forcedSpatialResult.AddRasterZones(
                                    vecRasterZones,
                                    spatialString.SpatialPageInfos);
                            }
                        }
                    }
                }

                if (distinctResult.SpatialMode.HasFlag(SpatialMode.Only) &&
                    !string.IsNullOrEmpty(distinctResult.ToString()))
                {
                    distinctResult.RemoveStringInfo();
                }

                // If we are evaluating the last query node, create a new empty DistinctResultSet
                // for each result in this enumeration.
                IEnumerable<DistinctResultSet> resultSets;
                if (queryNodes.Count == 0)
                {
                    resultSets = new DistinctResultSet[] { new DistinctResultSet() };
                }
                // Otherwise, retrieve the DistinctResultSets created by the subsequent query nodes.
                else
                {
                    resultSets = GetChildNodeResults(new Queue<QueryNode>(queryNodes));
                }

                // Return a copy of each existing the result set for each distinct result of the
                // current queryNode.
                foreach (DistinctResultSet resultSet in resultSets)
                {
                    // Each DistinctResultSet should be returned whether or not this result is to
                    // be included in the list, but only add this result to the set if it is not
                    // excluded.
                    if (!queryNode.ExcludeFromResult)
                    {
                        resultSet.ResultList.Insert(0, new QueryResult(queryNode, distinctResult));

                        // If there is a forcedSpatialResult, add it to the resultSet.
                        if (forcedSpatialResult != null)
                        {
                            if (resultSet.ForcedSpatialResult == null)
                            {
                                resultSet.ForcedSpatialResult = forcedSpatialResult;
                            }
                            else
                            {
                                SpatialString spatialResultCopy = forcedSpatialResult.Clone();
                                spatialResultCopy.DataEntryMergeAsHybridString(resultSet.ForcedSpatialResult);
                                resultSet.ForcedSpatialResult = spatialResultCopy;
                            }
                        }
                    }

                    yield return resultSet;
                }

                // After iterating all distinct results, pop the DistinctResult for this evaluation
                // context.
                queryNode.PopDistinctResult();
            }
        }

        /// <summary>
        /// Gets each <see cref="QueryResult"/> that should be evaluated distinctly.
        /// </summary>
        /// <param name="queryResult">The <see cref="QueryResult"/> being processed.</param>
        /// <returns>Each <see cref="QueryResult"/> that should be evaluated distinctly.</returns>
        IEnumerable<QueryResult> GetDistinctResults(QueryResult queryResult)
        {
            if (queryResult.SelectionMode == MultipleQueryResultSelectionMode.Distinct)
            {
                // In distinct mode, break out each value separately.
                foreach (QueryResult result in queryResult)
                {
                    yield return result.CreateFirstValueCopy();
                }
            }
            else
            {
                // Otherwise, process all results together (as per the SelectionMode).
                if (queryResult.HasMultipleValues && !string.IsNullOrEmpty(queryResult.ToString()))
                {
                    if (SelectionMode == MultipleQueryResultSelectionMode.None)
                    {
                        queryResult = new QueryResult(this);
                    }
                    else if (SelectionMode == MultipleQueryResultSelectionMode.First)
                    {
                        queryResult = queryResult.CreateFirstValueCopy();
                    }
                }

                yield return queryResult;
            }
        }

        /// <summary>
        /// Gets the <see cref="QueryResult"/> as a string that describes the specified spatial
        /// field of <see paramref="result"/>.
        /// </summary>
        /// <returns>A <see cref="QueryResult"/> describing the spatial field, or an empty result if
        /// <see paramref="result"/> is not spatial.</returns>
        QueryResult GetSpatialFieldValue(QueryResult result)
        {
            try
            {
                // Compile an enumeration of all spatial results.
                IEnumerable<SpatialString> spatialResults = result.Select(value =>
                    value.IsSpatial
                        ? (value.IsAttribute
                            ? value.FirstAttribute.Value
                            : value.FirstSpatialString)
                        : null);

                // Convert each spatial result into a string value corresponding to the SpatialField
                // property.
                QueryResult results;
                results = new QueryResult(this,
                    spatialResults.Select(spatialString =>
                    {
                        // If non-spatial, return blank.
                        if (spatialString == null || !spatialString.HasSpatialInfo())
                        {
                            return "";
                        }

                        if (SpatialField == Extract.DataEntry.SpatialField.Page)
                        {
                            return spatialString.GetFirstPageNumber()
                                .ToString(CultureInfo.InvariantCulture);
                        }
                        else if (SpatialField == Extract.DataEntry.SpatialField.Left ||
                                 SpatialField == Extract.DataEntry.SpatialField.Top ||
                                 SpatialField == Extract.DataEntry.SpatialField.Right ||
                                 SpatialField == Extract.DataEntry.SpatialField.Bottom)
                        {
                            ILongRectangle bounds = spatialString.GetOriginalImageBounds();

                            switch (SpatialField)
                            {
                                case Extract.DataEntry.SpatialField.Left:
                                    return bounds.Left.ToString(CultureInfo.InvariantCulture);

                                case Extract.DataEntry.SpatialField.Top:
                                    return bounds.Top.ToString(CultureInfo.InvariantCulture);

                                case Extract.DataEntry.SpatialField.Right:
                                    return bounds.Right.ToString(CultureInfo.InvariantCulture);

                                case Extract.DataEntry.SpatialField.Bottom:
                                    return bounds.Bottom.ToString(CultureInfo.InvariantCulture);

                                default:
                                    return "";
                            }
                        }
                        else
                        {
                            RasterZone rasterZone = RasterZone.GetBoundingRasterZone(
                                spatialString.GetOriginalImageRasterZones()
                                    .ToIEnumerable<ComRasterZone>()
                                    .Select(comRasterZone => new RasterZone(comRasterZone)),
                                spatialString.GetFirstPageNumber());

                            if (rasterZone == null)
                            {
                                return "";
                            }
                            else
                            {
                                switch (SpatialField)
                                {
                                    case Extract.DataEntry.SpatialField.Page:
                                        return rasterZone.PageNumber
                                            .ToString(CultureInfo.InvariantCulture);

                                    case Extract.DataEntry.SpatialField.StartX:
                                        return rasterZone.StartX
                                            .ToString(CultureInfo.InvariantCulture);

                                    case Extract.DataEntry.SpatialField.StartY:
                                        return rasterZone.StartY
                                            .ToString(CultureInfo.InvariantCulture);

                                    case Extract.DataEntry.SpatialField.EndX:
                                        return rasterZone.EndX
                                            .ToString(CultureInfo.InvariantCulture);

                                    case Extract.DataEntry.SpatialField.EndY:
                                        return rasterZone.EndY
                                            .ToString(CultureInfo.InvariantCulture);

                                    case Extract.DataEntry.SpatialField.Height:
                                        return rasterZone.Height
                                            .ToString(CultureInfo.InvariantCulture);

                                    default:
                                        return "";
                                }
                            }
                        }
                    })
                .ToArray());
                return results;
            }
            catch (Exception ex)
            {
                // Treat an error getting the spatial field the same as if the value didn't have any
                // spatial info and return blank after logging the exception.
                var ee = new ExtractException("ELI34852", "Error calculating spatial field", ex);
                ee.AddDebugData("Field", SpatialField.ToString(), false);
                ee.Log();

                return new QueryResult(this);
            }
        }

        /// <summary>
        /// Gets the <see cref="QueryResult"/> as a string that describes the specified attribute
        /// field of <see paramref="result"/>.
        /// </summary>
        /// <returns>A <see cref="QueryResult"/> describing the attribute field, or an empty result
        /// if <see paramref="result"/> is not and attribute.</returns>
        QueryResult GetAttributeFieldValue(QueryResult result)
        {
            try
            {
                // Compile an enumeration of all attribute results.
                IEnumerable<IAttribute> attributeResults = result
                    .Where(value => value.IsAttribute)
                    .Select(value => value.FirstAttribute);

                // Convert each attribute result into a string value corresponding to the
                // AttributeField property.
                QueryResult results;
                results = new QueryResult(this,
                    attributeResults.SelectMany(attribute =>
                    {
                        if (AttributeField == Extract.DataEntry.AttributeField.Name)
                        {
                            return new[] { attribute.Name };
                        }
                        else if (AttributeField == Extract.DataEntry.AttributeField.Type)
                        {
                            IEnumerable<string> types = attribute.Type.Split('+').Distinct();
                            return types;
                        }
                        else
                        {
                            return new[] {""};
                        }
                    })
                .Where (value => !string.IsNullOrEmpty(value))
                .ToArray());

                return results;
            }
            catch (Exception ex)
            {
                // Treat an error getting the attribute field the same as if the value didn't have any
                // spatial info and return blank after logging the exception.
                var ee = new ExtractException("ELI35252", "Error retrieving attribute field", ex);
                ee.AddDebugData("Field", AttributeField.ToString(), false);
                ee.Log();

                return new QueryResult(this);
            }
        }

        #endregion Private Members

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="DataEntryQuery"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the <see cref="DataEntryQuery"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param> 
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    // Ensure no more AttributeValueModified events are handled.
                    foreach (CompositeQueryNode queryNode in ChildNodes.OfType<CompositeQueryNode>())
                    {
                        queryNode.QueryValueModified -= HandleQueryValueModified;
                    }

                    foreach (IDisposable childNode in ChildNodes.OfType<IDisposable>())
                    {
                        childNode.Dispose();
                    }

                    // Unregister for query cached cleared event to prevent memory leak
                    // https://extract.atlassian.net/browse/ISSUE-13855
                    AttributeStatusInfo.QueryCacheCleared -= HandleQueryCacheCleared;
                }
                catch { }
            }
        }

        #endregion  IDisposable Members
    }
}
