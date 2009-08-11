using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Globalization;
using System.Text;
using System.Xml;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    partial class AutoUpdateTrigger
    {
        #region Enums

        /// <summary>
        /// Specifies the way in which spatial information will be persisted from
        /// <see cref="QueryNode"/>s.
        /// </summary>
        private enum SpatialMode
        {
            /// <summary>
            /// If a node's result is spatial, the spatial info will be returned.  Spatial info
            /// will be persisted as the result is combined with other results (spatial or not) as
            /// part of standard ComplexQueries, but will not be persisted through parent SQL nodes.
            /// </summary>
            Normal,

            /// <summary>
            /// If a node's result is spatial, the spatial info will be returned.  Spatial info
            /// will be persisted as the result is combined with other results (spatial or not) as
            /// part of standard ComplexQueries, and also will not be persisted through parent SQL
            /// nodes.
            /// </summary>
            Force,

            /// <summary>
            /// Only the spatial information from the node should be persisted, not the node text.
            /// The spatial info will be persisted as is with "Normal" mode.
            /// </summary>
            Only,

            /// <summary>
            /// No spatial information should be persisted from the node.
            /// </summary>
            None
        }

        #endregion Enums

        /// <summary>
        /// Represents the result of evaluating a <see cref="QueryNode"/>.
        /// </summary>
        private class QueryResult
        {
            /// <summary>
            /// A query result as a <see langword="string"/> (as opposed to spatial).
            /// </summary>
            string _stringResult;

            /// <summary>
            /// A query result with spatial information.
            /// </summary>
            SpatialString _spatialResult;

            /// <summary>
            /// Initializes a new <see cref="QueryResult"/> instance.
            /// </summary>
            /// <param name="stringResult">A <see langword="string"/> representing the result of a
            /// query.</param>
            public QueryResult(string stringResult)
            {
                _stringResult = stringResult ?? "";
            }

            /// <summary>
            /// Initializes a new <see cref="QueryResult"/> instance.
            /// </summary>
            /// <param name="spatialResult">A <see langword="SpatialString"/> representing the result
            /// of a query.</param>
            public QueryResult(SpatialString spatialResult)
            {
                try
                {
                    if (spatialResult != null && spatialResult.HasSpatialInfo())
                    {
                        _spatialResult = spatialResult;
                    }
                    else
                    {
                        _stringResult = (spatialResult != null) ? spatialResult.String : "";
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27060", ex);
                }
            }

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
                        return (_spatialResult != null &&
                            _spatialResult.HasSpatialInfo());
                    }
                    catch (Exception ex)
                    {
                        throw ExtractException.AsExtractException("ELI27059", ex);
                    }
                }
            }

            /// <summary>
            /// Gets the result of the query as a <see langword="string"/>.
            /// </summary>
            /// <returns>The result of the query as a <see langword="string"/>.</returns>
            public string String
            {
                get
                {
                    if (_spatialResult != null)
                    {
                        return _spatialResult.String;
                    }
                    else
                    {
                        return _stringResult;
                    }
                }
            }

            /// <summary>
            /// Gets the result of the query as a <see langword="SpatialString"/>.
            /// <para><b>Require:</b></para>
            /// <see cref="IsSpatial"/> must be <see langword="true"/>.
            /// </summary>
            /// <returns>The result of the query as a <see langword="SpatialString"/>.</returns>
            public SpatialString SpatialString
            {
                get
                {
                    ExtractException.Assert("ELI27058",
                        "Query result does not have any spatial info!", this.IsSpatial);

                    return _spatialResult;
                }
            }

            /// <summary>
            /// Appends the specified <see cref="QueryResult"/> to the end of this query result
            /// preserving any spatial info contained in accordance with the specified
            /// <see cref="SpatialMode"/>.
            /// </summary>
            /// <param name="otherResult">The <see cref="QueryResult"/> to append to this result.
            /// </param>
            /// <param name="spatialMode">A <see cref="SpatialMode"/> indicating the fashion in
            /// which spatial info should be persisted.</param>
            public void Append(QueryResult otherResult, SpatialMode spatialMode)
            {
                // If this string is spatial, preserve
                if (this.IsSpatial)
                {
                    // If the other result has spatial info that should be persisted.
                    if (otherResult.IsSpatial && spatialMode != SpatialMode.None)
                    {
                        // If persisting only the spatial info of the other result, append the
                        // strings, but then replace the text of the result with the original text
                        // of this result.
                        if (spatialMode == SpatialMode.Only)
                        {
                            _stringResult = this.String;
                            _spatialResult.Append(otherResult.SpatialString);
                            _spatialResult.ReplaceAndDowngradeToHybrid(_stringResult);
                        }
                        // Otherwise, append the other result normally.
                        else
                        {
                            _spatialResult.Append(otherResult.SpatialString);
                        }
                    }
                    // If the other result doesn't have any spatial info to persist, append its
                    // string value.
                    else
                    {
                        _spatialResult.AppendString(otherResult.String);
                    }
                }
                // If this result doesn't have spatial info, but the other result does have spatial
                // info to persist.
                else if (otherResult.IsSpatial && spatialMode != SpatialMode.None)
                {
                    // Make sure _stringResult contains the current string value in case
                    // _spatialString exists but is non-spatial.
                    _stringResult = this.String;

                    // Initialize this result's _spatialResult as a clone of the other result.
                    ICopyableObject copySource = (ICopyableObject)otherResult.SpatialString;
                    _spatialResult = (SpatialString)copySource.Clone();

                    // If persisting only the spatial info of the other result, clear the text.
                    if (_spatialResult.HasSpatialInfo() && spatialMode == SpatialMode.Only)
                    {
                        _spatialResult.ReplaceAndDowngradeToHybrid("");
                    }
                    
                    // Insert the text of this result before any text from the other result.
                    _spatialResult.InsertString(0, _stringResult);
                }
                // There is no spatial info to persist; simply append the other result's text.
                else
                {
                    _stringResult = this.String + otherResult.String;
                    
                    // In case a spatial result existed but did not have spatial info to persist,
                    // null it out now-- it is no longer needed.
                    _spatialResult = null;
                }
            }
        }

        /// <summary>
        /// Describes a node in an <see cref="AutoUpdateTrigger"/> query.
        /// </summary>
        abstract class QueryNode
        {
            /// <summary>
            /// The unparsed XML text that defines the query.
            /// </summary>
            protected string _query;

            /// <summary>
            /// Specifies whether the query node should be parameterized if it is used
            /// in an <see cref="SqlQueryNode"/>.
            /// </summary>
            bool _parameterize;

            /// <summary>
            /// Specifies the way in which spatial information will be persisted from this
            /// <see cref="QueryNode"/>.
            /// </summary>
            SpatialMode _spatialMode = SpatialMode.Normal;

            /// <summary>
            /// Specifies whether this <see cref="QueryNode"/> is required to execute the query.
            /// </summary>
            bool _required = true;

            /// <summary>
            /// Gets or sets whether the query node should be should be parameterized if it is used
            /// in an <see cref="SqlQueryNode"/>.
            /// </summary>
            /// <value><see langword="true"/> if the query's result should be parameterized when
            /// used as part of an <see cref="SqlQueryNode"/>, <see langword="false"/> if the
            /// query's result should be used as literal text.</value>
            /// <returns><see langword="true"/> if the query's result will be parameterized when
            /// used as part of an <see cref="SqlQueryNode"/>, <see langword="false"/> if the
            /// query's result will be used as literal text.</returns>
            public virtual bool Parameterize
            {
                get
                {
                    return _parameterize;
                }

                set
                {
                    _parameterize = value;
                }
            }

            /// <summary>
            /// Gets or sets the way in which spatial information will be persisted from this
            /// <see cref="QueryNode"/>.
            /// </summary>
            /// <value>A <see cref="SpatialMode"/> specifying the way in which spatial information
            /// will be persisted.</value>
            /// <returns>A <see cref="SpatialMode"/> specifying the way in which spatial information
            /// is persisted </returns>
            public SpatialMode SpatialMode
            {
                get
                {
                    return _spatialMode;
                }

                set
                {
                    _spatialMode = value;
                }
            }

            /// <summary>
            /// Gets or sets whether this <see cref="QueryNode"/> is required to execute the query.
            /// </summary>
            /// <value><see langword="true"/> if this node must be resolved to execute the query or
            /// <see langword="false"/> if this node can be evaluated when unresolved (and return an
            /// empty result.</value>
            /// <returns><see langword="true"/> if this node must be resolved to execute the query
            /// or <see langword="false"/> if this node can be evaluated when unresolved (and return an
            /// empty result.</returns>
            public virtual bool Required
            {
                get
                {
                    return _required;
                }

                set
                {
                    _required = value;
                }
            }

            /// <summary>
            /// Gets whether the query node is resolved enough to be evaluated.
            /// </summary>
            /// <returns><see langword="true"/> if all required triggers are registered and the
            /// query node can be evaluated, <see langword="false"/> if one or more triggers must
            /// be resolved before the query node can be evaluated.</returns>
            public abstract bool IsMinimallyResolved
            {
                get;
            }

            /// <summary>
            /// Gets whether the query node is completely resolved and there no more triggers that
            /// need to be registered.
            /// </summary>
            /// <returns><see langword="true"/> if all triggers are registered,
            /// <see langword="false"/> if one or more triggers are yet be resolved.</returns>
            public abstract bool IsFullyResolved
            {
                get;
            }

            /// <summary>
            /// Causes the values returned by <see cref="IsFullyResolved"/> and
            /// <see cref="IsMinimallyResolved"/> to be re-calculated.
            /// </summary>
            public virtual void UpdateResolvedStatus()
            {
            }

            /// <summary>
            /// Attempts to register candidate <see cref="IAttribute"/> trigger(s).
            /// </summary>
            /// <param name="statusInfo">If <see langword="null"/> the <see cref="QueryNode"/> will
            /// attempt to resolve all unresolved triggers. If specified, the corresponding
            /// query node will attempt to register the corresponding <see cref="IAttribute"/> with
            /// all unresolved nodes.</param>
            /// <returns><see langword="false"/> unless overriden.</returns>
            public virtual bool RegisterTriggerCandidate(AttributeStatusInfo statusInfo)
            {
                return false;
            }

            /// <summary>
            /// Evaluates the query.
            /// </summary>
            /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
            public abstract QueryResult Evaluate();
        }

        /// <summary>
        /// A <see cref="QueryNode"/> consisting of un-interpreted, literal text.
        /// </summary>
        private class LiteralQueryNode : QueryNode
        {
            /// <summary>
            /// Initializes a new <see cref="LiteralQueryNode"/> instance.
            /// </summary>
            /// <param name="query">The literal text to be returned during evaluation.</param>
            public LiteralQueryNode(string query)
            {
                _query = query;
            }

            /// <summary>
            /// Gets whether the query node is resolved enough to be evaluated.
            /// </summary>
            /// <returns><see langword="true"/> since a literal query is always resolved.</returns>
            public override bool IsMinimallyResolved
            {
                get
                {
                    return true;
                }
            }

            /// <summary>
            /// Gets whether the query node is completely resolved (all required triggers have been
            /// registered) and can be evaluated.
            /// </summary>
            /// <returns><see langword="true"/> since a literal query is always resolved.</returns>
            public override bool IsFullyResolved
            {
                get
                {
                    return true;
                }
            }

            /// <summary>
            /// Evaluates the query.
            /// </summary>
            /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
            public override QueryResult Evaluate()
            {
                ExtractException.Assert("ELI26753", "Cannot evaluate un-resolved query!",
                    this.IsMinimallyResolved);

                return new QueryResult(_query);
            }
        }

        /// <summary>
        /// A <see cref="QueryNode"/> to be resolved using the current source doc name.
        /// </summary>
        private class SourceDocNameQueryNode : QueryNode
        {
            /// <summary>
            /// Initializes a new <see cref="SourceDocNameQueryNode"/> instance.
            /// </summary>
            public SourceDocNameQueryNode()
            {
            }

            /// <summary>
            /// Gets whether the query node is resolved enough to be evaluated.
            /// </summary>
            /// <returns><see langword="true"/> since a SourceDoc query is always resolved.</returns>
            public override bool IsMinimallyResolved
            {
                get
                {
                    return true;
                }
            }

            /// <summary>
            /// Gets whether the query node is completely resolved (all required triggers have been
            /// registered) and can be evaluated.
            /// </summary>
            /// <returns><see langword="true"/> since a SourceDoc query is always resolved.</returns>
            public override bool IsFullyResolved
            {
                get
                {
                    return true;
                }
            }

            /// <summary>
            /// Evaluates the query using the current source doc name.
            /// </summary>
            /// <returns>The current source doc name</returns>
            public override QueryResult Evaluate()
            {
                return new QueryResult(AttributeStatusInfo.SourceDocName);
            }
        }

        /// <summary>
        /// A <see cref="QueryNode"/> that is comprised of one or more child
        /// <see cref="QueryNode"/>s.
        /// </summary>
        private class ComplexQueryNode : QueryNode
        {
            /// <summary>
            /// The AutoUpdateTrigger to which this query belongs.
            /// </summary>
            protected AutoUpdateTrigger _trigger;

            /// <summary>
            /// The RootQueryNode to which this query is a descendent.
            /// </summary>
            protected RootQueryNode _rootQuery;

            /// <summary>
            /// The child QueryNodes of this query node.
            /// </summary>
            protected List<QueryNode> _childNodes = new List<QueryNode>();

            /// <summary>
            /// Caches the <see cref="IsFullyResolved"/> status of the query node.
            /// <see langword="null"/> if there is no cached value and IsFullyResolved needs to be
            /// re-calculated.
            /// </summary>
            bool? _isFullyResolved;

            /// <summary>
            /// Caches the <see cref="IsMinimallyResolved"/> status of the query node.
            /// <see langword="null"/> if there is no cached value and IsMinimallyResolved needs to be
            /// re-calculated.
            /// </summary>
            bool? _isMinimallyResolved;

            /// <summary>
            /// Initializes a new <see cref="ComplexQueryNode"/> instance.
            /// </summary>
            public ComplexQueryNode()
            {
                base.Parameterize = true;
            }

            /// <summary>
            /// Loads the <see cref="QueryNode"/> using the specified XML query string.
            /// </summary>
            /// <param name="xmlNode">The XML query string defining the query.</param>
            /// <param name="trigger">The <see cref="AutoUpdateTrigger"/> for which this query is used.
            /// </param>
            /// <param name="rootQuery">The <see cref="RootQueryNode"/> that is the root of this
            /// query.</param>
            public virtual void LoadFromXml(XmlNode xmlNode, AutoUpdateTrigger trigger,
                RootQueryNode rootQuery)
            {
                _query = xmlNode.InnerXml;
                _trigger = trigger;
                _rootQuery = rootQuery;

                // Parameterize unless the parameterize attribute is present and specifies not to.
                XmlAttribute xmlAttribute = xmlNode.Attributes["Parameterize"];
                if (xmlAttribute != null && xmlAttribute.Value == "0")
                {
                    base.Parameterize = false;
                }

                xmlAttribute = xmlNode.Attributes["SpatialMode"];
                if (xmlAttribute != null)
                {
                    if (xmlAttribute.Value.Equals("Force", StringComparison.OrdinalIgnoreCase))
                    {
                        base.SpatialMode = SpatialMode.Force;
                    }
                    else if (xmlAttribute.Value.Equals("Only", StringComparison.OrdinalIgnoreCase))
                    {
                        base.SpatialMode = SpatialMode.Only;
                    }
                    else if (xmlAttribute.Value.Equals("None", StringComparison.OrdinalIgnoreCase))
                    {
                        base.SpatialMode = SpatialMode.None;
                    }
                }

                xmlAttribute = xmlNode.Attributes["Required"];
                if (xmlAttribute != null && xmlAttribute.Value == "0")
                {
                    base.Required = false;
                }

                // Iterate through each child of the current XML node and use each to initialize a
                // new child QueryNode.
                foreach (XmlNode childNode in xmlNode.ChildNodes)
                {
                    // If the node is text, initialize a LiteralQueryNode node.
                    if (childNode.NodeType == XmlNodeType.Text)
                    {
                        _childNodes.Add(new LiteralQueryNode(childNode.InnerText));
                    }
                    // If the node is an element, initialize a new ComplexQueryNode.
                    else if (childNode.NodeType == XmlNodeType.Element)
                    {
                        XmlElement childElement = ((XmlElement)childNode);

                        // Check for SourceDocName (which is not a ComplexQueryNode). 
                        if (childElement.Name.Equals("SourceDocName", 
                                StringComparison.OrdinalIgnoreCase))
                        {
                            _childNodes.Add(new SourceDocNameQueryNode());
                        }
                        else
                        {
                            ComplexQueryNode childQueryNode = null;

                            // Create the element as an SQL node?
                            if (childElement.Name.Equals("SQL",
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                childQueryNode = new SqlQueryNode();
                            }
                            // Create the element as an Attribute node?
                            else if (childElement.Name.Equals("Attribute",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                childQueryNode = new AttributeQueryNode();
                            }

                            ExtractException.Assert("ELI26726", "Failed parsing auto-update query!",
                                childQueryNode != null);

                            // Load the node.
                            childQueryNode.LoadFromXml(childNode, _trigger, rootQuery);

                            _childNodes.Add(childQueryNode);
                        }
                    }
                }
            }

            /// <summary>
            /// Causes the values returned by <see cref="IsFullyResolved"/> and
            /// <see cref="IsMinimallyResolved"/> to be re-calculated.
            /// </summary>
            public override void UpdateResolvedStatus()
            {
                // Clear the local cached values.
                _isFullyResolved = null;
                _isMinimallyResolved = null;

                // Call UpdateResolvedStatus on any child complex nodes.
                foreach (QueryNode childNode in _childNodes)
                {
                    childNode.UpdateResolvedStatus();
                }
            }

            /// <summary>
            /// Gets whether the query node is resolved enough to be evaluated.
            /// </summary>
            /// <returns><see langword="true"/> if all required triggers are registered and the
            /// query node can be evaluated, <see langword="false"/> if one or more triggers must
            /// be resolved before the query node can be evaluated.</returns>
            public override bool IsMinimallyResolved
            {
                get
                {
                    // If there is a cached minimally resolved status, return it.
                    if (_isMinimallyResolved != null)
                    {
                        return _isMinimallyResolved.Value;
                    }

                    // ...otherwise, re-calculate it. Default to true.
                    _isMinimallyResolved = true;

                    // If any child not is not minimally resolved, neither is this node.
                    foreach (QueryNode childNode in _childNodes)
                    {
                        if (!childNode.IsMinimallyResolved)
                        {
                            _isMinimallyResolved = false;
                            break;
                        }
                    }

                    return _isMinimallyResolved.Value;
                }
            }

            /// <summary>
            /// Gets whether the query node is completely resolved (all required triggers have been
            /// registered) and can be evaluated.
            /// </summary>
            /// <returns><see langword="true"/> if all triggers are registered and the query node can
            /// be evaluated, <see langword="false"/> if one or more triggers must be resolved before
            /// the query node can be evaluated.</returns>
            public override bool IsFullyResolved
            {
                get
                {
                    // If there is a cached fully resolved status, return it.
                    if (_isFullyResolved != null)
                    {
                        return _isFullyResolved.Value;
                    }

                    // ...otherwise, re-calculate it. Default to true.
                    _isFullyResolved = true;

                    // If any child not is not fully resolved, neither is this node.
                    foreach (QueryNode childNode in _childNodes)
                    {
                        if (!childNode.IsFullyResolved)
                        {
                            _isFullyResolved = false;
                            break;
                        }
                    }

                    return _isFullyResolved.Value;
                }
            }

            /// <summary>
            /// Attempts to register candidate <see cref="IAttribute"/> trigger(s).
            /// </summary>
            /// <param name="statusInfo">If <see langword="null"/> the <see cref="QueryNode"/> will
            /// attempt to resolve all unresolved triggers. If specified, the corresponding
            /// query node will attempt to register the corresponding <see cref="IAttribute"/> with
            /// all unresolved nodes.</param>
            /// <returns><see langword="true"/> if one or more <see cref="QueryNode"/>s were
            /// resolved; <see langword="false"/> otherwise.</returns>
            public override bool RegisterTriggerCandidate(AttributeStatusInfo statusInfo)
            {
                bool resolvedAttribute = false;

                // If this query isn't fully resolved, attempt to register all child query nodes.
                if (!this.IsFullyResolved)
                {
                    foreach (QueryNode childNode in _childNodes)
                    {
                        if (childNode.RegisterTriggerCandidate(statusInfo))
                        {
                            resolvedAttribute = true;
                        }
                    }
                }

                // If child node was resolved, the cached resolved statuses need to be recalculated.
                if (resolvedAttribute)
                {
                    _isFullyResolved = null;
                    _isMinimallyResolved = null;
                }

                return resolvedAttribute;
            }

            /// <summary>
            /// Evaluates the query by combining all child <see cref="QueryNode"/>s.
            /// </summary>
            /// <returns>A <see langword="string"/> representing the result of the query.</returns>
            public override QueryResult Evaluate()
            {
                try
                {
                    QueryResult result = new QueryResult("");

                    // Combine the results of all child nodes.
                    foreach (QueryNode childNode in _childNodes)
                    {
                        result.Append(childNode.Evaluate(), childNode.SpatialMode);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI26729", ex);
                    ee.AddDebugData("Query node type", "Complex", false);
                    ee.AddDebugData("Query", _query ?? "null", false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// A <see cref="QueryNode"/> that is to be resolved using an SQL query against the active
        /// database.
        /// </summary>
        private class SqlQueryNode : ComplexQueryNode
        {
            /// <summary>
            /// Initializes a new <see cref="SqlQueryNode"/> instance.
            /// </summary>
            public SqlQueryNode()
                : base()
            {
            }

            /// <summary>
            /// Evaluates the query by using the combined result of all child
            /// <see cref="QueryNode"/>s as an SQL query against the active database.
            /// </summary>
            /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
            public override QueryResult Evaluate()
            {
                try
                {
                    ExtractException.Assert("ELI26754", "Cannot evaluate un-resolved query!",
                        this.IsMinimallyResolved);

                    StringBuilder sqlQuery = new StringBuilder();

                    SqlCeConnection sqlCeConnection = _trigger._dbConnection as SqlCeConnection;

                    ExtractException.Assert("ELI26733",
                        "Auto-update queries currently support only SQL compact databases",
                        sqlCeConnection != null);

                    // Keep track of the combined spatial result for all child nodes which have a
                    // spatial mode of "Force".
                    SpatialString spatialResult = null;

                    // Child query nodes whose results have been parameterized.
                    Dictionary<string, string> parameters = new Dictionary<string, string>();

                    // Combine the result of all child queries parameterizing as necessary.
                    foreach (QueryNode childNode in _childNodes)
                    {
                        QueryResult childQueryResult = childNode.Evaluate();

                        // If the current child has a spatial mode of "Force", store its spatial
                        // info so it can be applied to the query result later.
                        if (childQueryResult.IsSpatial && childNode.SpatialMode == SpatialMode.Force)
                        {
                            if (spatialResult == null)
                            {
                                ICopyableObject copySource =
                                    (ICopyableObject)childQueryResult.SpatialString;
                                spatialResult = (SpatialString)copySource.Clone();
                            }
                            else
                            {
                                spatialResult.Append(childQueryResult.SpatialString);
                            }
                        }

                        if (childNode.Parameterize)
                        {
                            // If parameterizing, don't add the query result directly, rather add a
                            // parameter name to the query and add the key/value pair to parameters.
                            string key = "@" + parameters.Count.ToString(CultureInfo.InvariantCulture);
                            string value = childQueryResult.String;

                            parameters[key] = value;
                            sqlQuery.Append(key);
                        }
                        else
                        {
                            sqlQuery.Append(childQueryResult.String);
                        }
                    }

                    // Create a database command using the query.
                    using (DbCommand dbCommand = DataEntryMethods.CreateDBCommand(
                        sqlCeConnection, sqlQuery.ToString(), parameters))
                    {
                        // Execute the query.
                        string queryResult = DataEntryMethods.ExecuteDBQuery(dbCommand,
                            (_trigger._validationTrigger ? "\r\n" : null), ", ");

                        // Apply the spatial infomation of child nodes with "Force" spatial mode if
                        // necessary.
                        if (spatialResult != null && spatialResult.HasSpatialInfo())
                        {
                            spatialResult.ReplaceAndDowngradeToHybrid(queryResult);
                            return new QueryResult(spatialResult);
                        }
                        // Otherwise, just return the text value.
                        else
                        {
                            return new QueryResult(queryResult);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI26755", ex);
                    ee.AddDebugData("Query node type", "SQL", false);
                    ee.AddDebugData("Query", _query ?? "null", false);
                    throw ee;
                }
            }
        }

        /// <summary>
        /// A <see cref="QueryNode"/> that is to be resolved using the value of an
        /// <see cref="IAttribute"/>. Changes made to this <see cref="IAttribute"/> will cause the
        /// target attribute to be updated.
        /// </summary>
        private class AttributeQueryNode : ComplexQueryNode
        {
            // The attribute to evaluate the query and trigger evaluation.
            IAttribute _triggerAttribute;

            /// <summary>
            /// The full path of any attribute matching the value query.  Used for efficiency when
            /// evaluating candidate triggers.
            /// </summary>
            string _attributeValueFullPath;

            /// <summary>
            /// Initialized a new <see cref="AttributeQueryNode"/> instance.
            /// </summary>
            public AttributeQueryNode()
                : base()
            {
            }

            /// <summary>
            /// Loads the <see cref="AttributeQueryNode"/> using the specified XML query string.
            /// </summary>
            /// <param name="xmlNode">The XML query string defining the query.</param>
            /// <param name="trigger">The <see cref="AutoUpdateTrigger"/> for which this query is used.
            /// </param>
            /// <param name="rootQuery">The <see cref="RootQueryNode"/> that is the root of this
            /// query.</param>
            public override void LoadFromXml(XmlNode xmlNode, AutoUpdateTrigger trigger,
                RootQueryNode rootQuery)
            {
                try
                {
                    base.LoadFromXml(xmlNode, trigger, rootQuery);

                    // After loading query node, attempt to register a trigger for it.
                    RegisterTriggerCandidate(null);
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI26756", ex);
                    ee.AddDebugData("XML", xmlNode.InnerXml, false);
                }
            }

            /// <summary>
            /// Gets whether the query node is resolved enough to be evaluated.
            /// </summary>
            /// <returns><see langword="true"/> if all required triggers are registered and the
            /// query node can be evaluated, <see langword="false"/> if one or more triggers must
            /// be resolved before the query node can be evaluated.</returns>
            public override bool IsMinimallyResolved
            {
                get
                {
                    // If a triggerAttribute was previously registered, but the query used to
                    // register it is no longer resolved, unregister the trigger attribute.
                    if (!base.IsMinimallyResolved && _triggerAttribute != null)
                    {
                        UnregisterTriggerAttribute();
                    }

                    // The node is minimally resolved if the base is as well and this node either
                    // has a registered attribute or is marked as not required.
                    return base.IsMinimallyResolved && (!base.Required || _triggerAttribute != null);
                }
            }

            /// <summary>
            /// Gets whether the query node is completely resolved (all required triggers have been
            /// registered) and can be evaluated.
            /// </summary>
            /// <returns><see langword="true"/> if all triggers are registered and the query node can
            /// be evaluated, <see langword="false"/> if one or more triggers must be resolved before
            /// the query node can be evaluated.</returns>
            public override bool IsFullyResolved
            {
                get
                {
                    // If a triggerAttribute was previously registered, but the query used to
                    // register it is no longer resolved, unregister the trigger attribute.
                    if (!base.IsFullyResolved && _triggerAttribute != null)
                    {
                        UnregisterTriggerAttribute();
                    }

                    return base.IsFullyResolved && _triggerAttribute != null;
                }
            }

            /// <summary>
            /// Evaluates the query by using the value of the specified <see cref="IAttribute"/>.
            /// </summary>
            /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
            public override QueryResult Evaluate()
            {
                try
                {
                    ExtractException.Assert("ELI26757", "Cannot evaluate un-resolved query!",
                        this.IsMinimallyResolved);

                    if (_triggerAttribute != null)
                    {
                        return new QueryResult(_triggerAttribute.Value);
                    }
                    else
                    {
                        // In case the node is not required and not resolved.
                        return new QueryResult("");
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI26728", ex);
                    ee.AddDebugData("Query node type", "Attribute", false);
                    ee.AddDebugData("Query", _query ?? "null", false);
                    throw ee;
                }
            }

            /// <summary>
            /// Attempts to register candidate <see cref="IAttribute"/> trigger(s).
            /// </summary>
            /// <param name="statusInfo">If <see langword="null"/> the <see cref="QueryNode"/> will
            /// attempt to resolve all unresolved triggers. If specified, the corresponding
            /// query node will attempt to register the corresponding <see cref="IAttribute"/> with
            /// all unresolved nodes.</param>
            /// <returns><see langword="true"/> if one or more <see cref="QueryNode"/>s were
            /// resolved; <see langword="false"/> otherwise.</returns>
            public override bool RegisterTriggerCandidate(AttributeStatusInfo statusInfo)
            {
                // First attempt to register all child nodes.
                bool resolved = base.RegisterTriggerCandidate(statusInfo);

                // If all child nodes are resolved, but this node is not, attempt to resolve it.
                if (!this.IsFullyResolved && base.IsMinimallyResolved)
                {
                    if (string.IsNullOrEmpty(_attributeValueFullPath))
                    {
                        _attributeValueFullPath = AttributeStatusInfo.GetFullPath(
                            _trigger._rootPath, base.Evaluate().String);
                    }

                    // Test to see that if an attribute was supplied, its path matches the path
                    // we would expect for a trigger attribute.
                    if (statusInfo == null || statusInfo.FullPath == _attributeValueFullPath)
                    {
                        // Search for candidate triggers.
                        IUnknownVector candidateTriggers = AttributeStatusInfo.ResolveAttributeQuery(
                                            _trigger._targetAttribute, base.Evaluate().String);

                        int candidateCount = candidateTriggers.Size();

                        ExtractException.Assert("ELI26117",
                            "Multiple attribute triggers not supported for the auto-update value",
                            candidateCount <= 1);

                        // If a single candidate was found, register it as the trigger for this term
                        // (even if it wasn't the suggestd candidate).
                        if (candidateCount == 1)
                        {
                            _triggerAttribute = (IAttribute)candidateTriggers.At(0);

                            if (statusInfo == null)
                            {
                                statusInfo = AttributeStatusInfo.GetStatusInfo(_triggerAttribute);
                                statusInfo.FullPath = _attributeValueFullPath;
                            }

                            // Handle deletion of the attribute so the query will become unresolved
                            // if the trigger attribute is deleted.
                            statusInfo.AttributeDeleted += HandleAttributeDeleted;

                            // Set a trigger for the attribute on the root node.
                            _rootQuery.SetTrigger(_triggerAttribute);

                            resolved = true;
                        }
                    }
                }

                return resolved;
            }

            #region Private Members

            /// <summary>
            /// Handles the case that a trigger <see cref="IAttribute"/> was deleted so that it can be
            /// un-registered as a trigger.
            /// </summary>
            /// <param name="sender">The object that sent the event.</param>
            /// <param name="e">An <see cref="AttributeDeletedEventArgs"/> that contains the event data.
            /// </param>
            void HandleAttributeDeleted(object sender, AttributeDeletedEventArgs e)
            {
                try
                {
                    ExtractException.Assert("ELI26717", "Mis-matched trigger attribute detected!",
                        e.DeletedAttribute == _triggerAttribute);

                    // Unregister the attribute as a trigger for all terms it is currently used in.
                    UnregisterTriggerAttribute();
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI26718", ex);
                    ee.AddDebugData("Event Data", e, false);
                    ee.Display();
                }
            }


            /// <summary>
            /// Un-registers the active trigger attribute.
            /// </summary>
            void UnregisterTriggerAttribute()
            {
                if (_triggerAttribute != null)
                {
                    AttributeStatusInfo statusInfo =
                                AttributeStatusInfo.GetStatusInfo(_triggerAttribute);
                    statusInfo.AttributeDeleted -= HandleAttributeDeleted;

                    _rootQuery.ClearTrigger(_triggerAttribute);

                    _triggerAttribute = null;
                }
            }

            #endregion Private Members
        }

        /// <summary>
        /// A <see cref="QueryNode"/> that is to represent the root level of a query for a
        /// <see cref="AutoUpdateTrigger"/>.
        /// </summary>
        private class RootQueryNode : ComplexQueryNode, IDisposable
        {
            /// <summary>
            /// The set of <see cref="IAttribute"/>s that will trigger this query to be evaluated.
            /// </summary>
            List<IAttribute> _triggerAttributes = new List<IAttribute>();

            /// <summary>
            /// Indicates whether this query is a default query.
            /// </summary>
            bool _defaultQuery;

            /// <summary>
            /// Indicates whether an update using this query is needed once unresolved portions of
            /// the query are resolved.
            /// </summary>
            bool _updatePending;

            /// <summary>
            /// Indicates whether this query is disabled and should not be evaluated.
            /// </summary>
            bool _disabled;

            /// <summary>
            /// Initializes a new <see cref="RootQueryNode"/> instance.
            /// </summary>
            public RootQueryNode()
                : base()
            {
            }

            /// <summary>
            /// Gets or set whether this query is a default query.
            /// </summary>
            /// <value><see langword="true"/> if this query is a default auto-update query which is
            /// only to execute in order to provide a default value when there isn't an initial
            /// value; <see langword="false"/> otherwise.</value>
            /// <returns><see langword="true"/> if this query is a default auto-update query which
            /// is only to execute in order to provide a default value when there isn't an initial
            /// value; <see langword="false"/> otherwise.</returns>
            public bool DefaultQuery
            {
                get
                {
                    return _defaultQuery;
                }

                set
                {
                    _defaultQuery = value;
                }
            }

            /// <summary>
            /// Gets whether the query is disabled.
            /// </summary>
            /// <returns><see langword="true"/> if the query is disabled; <see langword="false"/>
            /// otherwise.</returns>
            public bool Disabled
            {
                get
                {
                    return _disabled;
                }
            }

            /// <summary>
            /// Attempts to update the target <see cref="IAttribute"/> using the result of the
            /// evaluated query.
            /// </summary>
            /// <returns><see langword="true"/> if the <see cref="IAttribute"/> was updated;
            /// <see langword="false"/> otherwise.</returns>
            public bool UpdateValue()
            {
                try
                {
                    // Ensure the query is resolved.
                    if (base.IsMinimallyResolved)
                    {
                        // If so, evaluate it.
                        QueryResult queryResult = base.Evaluate();

                        // Use the results to update the target attribute's validation list if the
                        // AutoUpdateTrigger is a validation trigger.
                        if (_trigger._validationTrigger)
                        {
                            // Update the validation list associated with the attribute.
                            AttributeStatusInfo statusInfo =
                                AttributeStatusInfo.GetStatusInfo(_trigger._targetAttribute);

                            // Validation queries can only be specified for attributes with a
                            // DataEntryValidator as its validator.
                            DataEntryValidator validator = statusInfo.Validator as DataEntryValidator;
                            ExtractException.Assert("ELI26154", "Uninitialized or invalid validator!",
                                validator != null);

                            // Parse the file contents into individual list items.
                            string[] listItems = queryResult.String.Split(
                                new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                            validator.SetValidationListValues(listItems);
                            statusInfo.OwningControl.RefreshAttribute(_trigger._targetAttribute);

                            return true;
                        }
                        // If this auto-update query should only provide a default value
                        else if (this.DefaultQuery)
                        {
                            // If this is a default trigger that is fully resolved, it will
                            // never need to fire again-- clear all triggers.
                            if (this.IsFullyResolved)
                            {
                                _disabled = true;
                                ClearAllTriggers();
                            }

                            // If the target attribute is empty and would need a default value
                            // (or it was when the default trigger was created) update the
                            // attribute using the default value.
                            if (_updatePending ||
                                string.IsNullOrEmpty(_trigger._targetAttribute.Value.String))
                            {
                                // If the default query is not fully resolved, flag _updatePending
                                // to allow it an opportunity to apply its update as query
                                // components become resolved.
                                _updatePending = !this.IsFullyResolved;

                                // Apply the default query value.
                                return ApplyQueryResult(queryResult);
                            }
                            else
                            {
                                return false;
                            }
                        }
                        // A normal auto-update query- apply the query results (if there were any).
                        else
                        {
                            return ApplyQueryResult(queryResult);
                        }
                    }
                    else if (this.DefaultQuery &&
                        string.IsNullOrEmpty(_trigger._targetAttribute.Value.String))
                    {
                        // If the target attribute could use a default value, but the default query
                        // is not yet resolved, set _updatePending so that it will fire
                        // once it is resolved.
                        _updatePending = true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26735", ex);
                }
            }

            /// <summary>
            /// Registers a trigger so that updates to the specified trigger <see cref="IAttribute"/>
            /// cause the target attribute to be updated.
            /// </summary>
            /// <param name="triggerAttribute">The <see cref="IAttribute"/> for which modifications
            /// should cause the query to be evaluated.</param>
            public void SetTrigger(IAttribute triggerAttribute)
            {
                if (!_triggerAttributes.Contains(triggerAttribute))
                {
                    _triggerAttributes.Add(triggerAttribute);

                    AttributeStatusInfo.GetStatusInfo(triggerAttribute).AttributeValueModified +=
                        HandleAttributeValueModified;
                }
            }

            /// <summary>
            /// Unregisters a trigger so that updates to the specified <see cref="IAttribute"/>
            /// no longer cause the target attribute to be updated.
            /// </summary>
            /// <param name="triggerAttribute">The <see cref="IAttribute"/> for which modifications
            /// should no longer cause the query to be evaluated.</param>
            public void ClearTrigger(IAttribute triggerAttribute)
            {
                if (_triggerAttributes.Contains(triggerAttribute))
                {
                    _triggerAttributes.Remove(triggerAttribute);

                    AttributeStatusInfo.GetStatusInfo(triggerAttribute).AttributeValueModified -=
                        HandleAttributeValueModified;

                    // Ensure all nodes in the query update cached resolved status values to reflect
                    // the change.
                    _rootQuery.UpdateResolvedStatus();
                }
            }

            #region IDisposable Members

            /// <summary>
            /// Releases all resources used by the <see cref="RootQueryNode"/>.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases all resources used by the <see cref="RootQueryNode"/>.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
            /// resources; <see langword="false"/> to release only unmanaged resources.</param> 
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // Ensure no more AttributeValueModified events are handled.
                    ClearAllTriggers();
                }
            }

            #endregion  IDisposable Members

            #region Event Handlers

            /// <summary>
            /// Handles the case that data was modified in a trigger <see cref="IAttribute"/> in order to
            /// trigger the target <see cref="IAttribute"/> to update.
            /// </summary>
            /// <param name="sender">The object that sent the event.</param>
            /// <param name="e">An <see cref="AttributeValueModifiedEventArgs"/> that contains the event data.
            /// </param>
            void HandleAttributeValueModified(object sender, AttributeValueModifiedEventArgs e)
            {
                try
                {
                    // If the modification is not incremental, update the attribute value.
                    if (!e.IncrementalUpdate)
                    {
                        if (this.DefaultQuery)
                        {
                            // Always ensure a default query applies updates as part of a full
                            // auto-update trigger to ensure normal auto-update triggers can apply
                            // their updates on top of any default value.
                            _trigger.UpdateValue();
                        }
                        else
                        {
                            // If no a default auto-update trigger, update using only this query.
                            UpdateValue();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExtractException ee = ExtractException.AsExtractException("ELI26115", ex);
                    ee.AddDebugData("Event Data", e, false);
                    ee.Display();
                }
            }

            #endregion Event Handlers

            #region Private Members

            /// <summary>
            /// Unregisters all triggers so that this query will no longer execute in response to
            /// updated attribute.
            /// </summary>
            public void ClearAllTriggers()
            {
                foreach (IAttribute attribute in _triggerAttributes)
                {
                    AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(attribute);
                    statusInfo.AttributeValueModified -= HandleAttributeValueModified;
                }

                _triggerAttributes.Clear();
            }

            /// <summary>
            /// Attempts to apply the specified <see cref="QueryResult"/> to the target attribute.
            /// </summary>
            /// <param name="queryResult">The <see cref="QueryResult"/> to be applied.</param>
            /// <returns><see langword="true"/> if the <see cref="IAttribute"/> was updated;
            /// <see langword="false"/> otherwise.</returns>
            bool ApplyQueryResult(QueryResult queryResult)
            {
                if (!string.IsNullOrEmpty(queryResult.String) ||
                    queryResult.IsSpatial)
                {
                    // Update the attribute's value.
                    if (queryResult.IsSpatial)
                    {
                        AttributeStatusInfo.SetValue(_trigger._targetAttribute,
                            queryResult.SpatialString, false, true);
                    }
                    else
                    {
                        AttributeStatusInfo.SetValue(_trigger._targetAttribute,
                            queryResult.String, false, true);
                    }

                    // After applying the value, direct the control that contains it to
                    // refresh the value.
                    AttributeStatusInfo.GetOwningControl(_trigger._targetAttribute).
                        RefreshAttribute(_trigger._targetAttribute);

                    return true;
                }
                else
                {
                    return false;
                }
            }

            #endregion Private Members
        }
    }
}