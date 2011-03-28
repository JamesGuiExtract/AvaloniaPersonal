using Extract.Utilities;
using Extract.Utilities.Parsers;
using Spring.Core.TypeConversion;
using Spring.Core.TypeResolution;
using Spring.Expressions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    #region Enums

    /// <summary>
    /// Specifies the way in which spatial information will be persisted from
    /// <see cref="QueryNode"/>s.
    /// </summary>
    enum SpatialMode
    {
        /// <summary>
        /// If a node's result is spatial, the spatial info will be returned.  Spatial info
        /// will be persisted as the result is combined with other results (spatial or not) as
        /// part of standard ComplexQueries, but will not be persisted through parent SQL nodes.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// If a node's result is spatial, the spatial info will be returned.  Spatial info
        /// will be persisted as the result is combined with other results (spatial or not) as
        /// part of standard ComplexQueries, and also will be persisted through parent SQL nodes.
        /// </summary>
        Force = 1,

        /// <summary>
        /// Only the spatial information from the node should be persisted, not the node text.
        /// The spatial info will be persisted as is with "Normal" mode.
        /// </summary>
        Only = 2,

        /// <summary>
        /// No spatial information should be persisted from the node.
        /// </summary>
        None = 3
    }

    /// <summary>
    /// For <see cref="DataEntryQuery"/>'s representing a validation list, specifies for what
    /// purpose the query result should be used.
    /// </summary>
    public enum ValidationListType
    {
        /// <summary>
        /// The result should be used both for an auto-complete list and to validate values.
        /// </summary>
        Both = 0,

        /// <summary>
        /// The result should be used only to validate values.
        /// </summary>
        ValidationListOnly = 1,

        /// <summary>
        /// The result should be used only for an auto-complete list.
        /// </summary>
        AutoCompleteOnly = 2
    }

    #endregion Enums

    #region Extension Methods

    /// <summary>
    /// Extension methods used by the data entry framework
    /// </summary>
    static class ExtensionMethods
    {
        /// <summary>
        /// Converts a <see langword="string"/> to a <see langword="bool"/> where "0" and "1" are
        /// recognized as well as "true" and "false".
        /// </summary>
        /// <param name="value">The <see langword="string"/> to be converted.</param>
        /// <returns>The <see langword="bool"/> equivalent.</returns>
        public static bool ToBoolean(this string value)
        {
            if (value == "1")
            {
                return true;
            }
            else if (value == "0")
            {
                return false;
            }

            return bool.Parse(value);
        }
    }

    #endregion Extension Methods

    #region QueryNode

    /// <summary>
    /// Describes an element of an <see cref="DataEntryQuery"/>.
    /// </summary>
    internal abstract class QueryNode
    {
        /// <summary>
        /// The unparsed XML text that defines the query.
        /// </summary>
        protected string _query;

        /// <summary>
        /// If specified, indicates the name by which this result can be looked up a later time.
        /// </summary>
        string _name;

        /// <summary>
        /// If this node is named, this is the last calculated result or <see langword="null"/> if
        /// the node has not yet been evaluated.
        /// </summary>
        protected QueryResult _namedResult;

        /// <summary>
        /// The DataEntryQueries to which this query is a descendent.
        /// </summary>
        protected HashSet<DataEntryQuery> _rootQueries = new HashSet<DataEntryQuery>();

        /// <summary>
        /// Specifies whether the query node should be parameterized if it is used
        /// in an <see cref="SqlQueryNode"/> or <see cref="ExpressionQueryNode"/>.
        /// </summary>
        bool _parameterize = true;

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
        /// The <see cref="MultipleQueryResultSelectionMode"/> that should be used to determine how
        /// multiple resulting values should be handled.
        /// </summary>
        MultipleQueryResultSelectionMode _selectionMode;

        /// <summary>
        /// indicates whether the result of the query should not be included in a combined result.
        /// </summary>
        bool _excludeFromResult;

        /// <summary>
        /// For queries representing a validation list, specifies for what purpose the query result
        /// should be used.
        /// </summary>
        ValidationListType _validationListType = ValidationListType.Both;

        /// <summary>
        /// Indicates whether to use case-sensitive string comparisons when comparisons are needed
        /// in-order to calculate the query result.
        /// </summary>
        bool _caseSensitive = true;

        /// <summary>
        /// Specifies a delimiter that should be used to flatten any resulting list into a single
        /// string value where each value is separated by the specified delimiter.
        /// </summary>
        string _stringListDelimiter;

        /// <summary>
        /// Specifies whether a query should resolve itself if possible as it is loaded.
        /// </summary>
        bool _resolveOnLoad;

        /// <summary>
        /// The properties assigned to the query node via <see cref="XmlAttribute"/>s (name/value
        /// pairs).
        /// </summary>
        private Dictionary<string, string> _properties = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new <see cref="QueryNode"/> instance.
        /// </summary>
        /// <param name="selectionMode">The <see cref="MultipleQueryResultSelectionMode"/> that
        /// should be used to determine how multiple resulting values should be handled.</param>
        /// <param name="resolveOnLoad"><see langword="true"/> if the query will resolve itself if
        /// possible as it is loaded, <see langword="false"/> if
        /// <see cref="RegisterTriggerCandidate"/> must be called manually to resolve the query if
        /// attribute triggers are involved.</param>
        protected QueryNode(MultipleQueryResultSelectionMode selectionMode, bool resolveOnLoad)
        {
            _selectionMode = selectionMode;
            _resolveOnLoad = resolveOnLoad;
        }

        /// <summary>
        /// Gets properties associated with this query node that have been specified via XML
        /// attributes.
        /// </summary>
        /// <value>The properties.</value>
        public Dictionary<string, string> Properties
        {
            get
            {
                return _properties;
            }
        }

        /// <summary>
        /// Gets or sets whether the query node should be should be parameterized if it is used
        /// in an <see cref="SqlQueryNode"/> or <see cref="ExpressionQueryNode"/>.
        /// </summary>
        /// <value><see langword="true"/> if the query's result should be parameterized when
        /// used as part of an <see cref="SqlQueryNode"/> or <see cref="ExpressionQueryNode"/>,
        /// <see langword="false"/> if the query's result should be used as literal text.</value>
        public bool Parameterize
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
        /// Gets or set the name by which this result can be looked up a later time.
        /// Can be <see langword="null"/>, in which case the results will not be accessible
        /// independently from the overal query result.
        /// </summary>
        /// <value>The name by which this result can be looked up a later time.</value>
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// The <see cref="MultipleQueryResultSelectionMode"/> that should be used to determine how
        /// multiple resulting values should be handled.
        /// </summary>
        /// <value>The <see cref="MultipleQueryResultSelectionMode"/> that should be used to
        /// determine how multiple resulting values should be handled.</value>
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
        /// Gets or sets whether the query result should not be included in a combined result.
        /// </summary>
        /// <value><see landword="true"/> if the result should be excluded from the combined result,
        /// <see landword="false"/> otherwise</value>
        public bool ExcludeFromResult
        {
            get
            {
                return _excludeFromResult;
            }

            set
            {
                _excludeFromResult = value;
            }
        }

        /// <summary>
        /// Gets or sets whether for queries representing a validation list, for what purpose the
        /// query result should be used.
        /// </summary>
        /// <value>A <see cref="ValidationListType"/> value indicating for what purpose the query
        /// result should be used.</value>
        public ValidationListType ValidationListType
        {
            get
            {
                return _validationListType;
            }

            set
            {
                _validationListType = value;
            }
        }

        /// <summary>
        /// Gets whether to use case-sensitive string comparisons when comparisons are needed
        /// in-order to calculate the query result.
        /// </summary>
        /// <returns><see landword="true"/> if case-sensitive comparisons should be used to generate
        /// the query result; <see landword="false"/> otherwise.</returns>
        public bool CaseSensitive
        {
            get
            {
                return _caseSensitive;
            }
        }

        /// <summary>
        /// Gets a delimiter that should be used to flatten any resulting list into a single
        /// string value where each value is separated by the specified delimiter.
        /// </summary>
        /// <reuturns>The <see langword="string"/> which should delineate the values of a flattened
        /// list.</reuturns>
        public string StringListDelimiter
        {
            get
            {
                return _stringListDelimiter;
            }
        }

        /// <summary>
        /// Gets whether a query will resolve itself if possible as it is loaded.
        /// </summary>
        /// <returns><see langword="true"/> if the query will resolve itself if possible as it is
        /// loaded, <see langword="false"/> if <see cref="RegisterTriggerCandidate"/> must be
        /// called manually to resolve the query if attribute triggers are involved.</returns>
        public bool ResolveOnLoad
        {
            get
            {
                return _resolveOnLoad;
            }
        }

        /// <summary>
        /// Gets whether the query node is resolved enough to be evaluated.
        /// </summary>
        /// <returns><see langword="true"/> if all required triggers are registered and the
        /// query node can be evaluated, <see langword="false"/> if one or more triggers must
        /// be resolved before the query node can be evaluated.</returns>
        public abstract bool GetIsMinimallyResolved();

        /// <summary>
        /// Gets whether the query node is completely resolved and there no more triggers that
        /// need to be registered.
        /// </summary>
        /// <returns><see langword="true"/> if all triggers are registered,
        /// <see langword="false"/> if one or more triggers are yet be resolved.</returns>
        public abstract bool GetIsFullyResolved();

        /// <summary>
        /// Causes the values returned by <see cref="GetIsFullyResolved"/> and
        /// <see cref="GetIsMinimallyResolved"/> to be re-calculated.
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

        /// <summary>
        /// Loads the <see cref="QueryNode"/> using the specified XML query string.
        /// </summary>
        /// <param name="xmlNode">The XML query string defining the query.</param>
        /// <param name="rootQuery">The <see cref="DataEntryQuery"/> that is the root of this
        /// query.</param>
        /// <param name="namedQueries">A communal collection of named <see cref="QueryNode"/>s
        /// available to allow referencing of named nodes by subsequent nodes.</param>
        public virtual void LoadFromXml(XmlNode xmlNode, DataEntryQuery rootQuery,
            Dictionary<string, QueryNode> namedQueries)
        {
            try
            {
                // Populate the assigned properties.
                foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
                {
                    _properties[xmlAttribute.Name] = xmlAttribute.Value;
                }

                string xmlAttributeValue;

                if (_properties.TryGetValue("Name", out xmlAttributeValue))
                {
                    Name = xmlAttributeValue;
                    namedQueries[xmlAttributeValue] = this;
                }

                if (_properties.TryGetValue("SelectionMode", out xmlAttributeValue))
                {
                    if (xmlAttributeValue.Equals("First", StringComparison.OrdinalIgnoreCase))
                    {
                        SelectionMode = MultipleQueryResultSelectionMode.First;
                    }
                    else if (xmlAttributeValue.Equals("List", StringComparison.OrdinalIgnoreCase))
                    {
                        SelectionMode = MultipleQueryResultSelectionMode.List;
                    }
                    else if (xmlAttributeValue.Equals("Distinct", StringComparison.OrdinalIgnoreCase))
                    {
                        SelectionMode = MultipleQueryResultSelectionMode.Distinct;
                    }
                    else if (xmlAttributeValue.Equals("None", StringComparison.OrdinalIgnoreCase))
                    {
                        SelectionMode = MultipleQueryResultSelectionMode.None;
                    }
                }

                // Required unless specified otherwise.
                if (_properties.TryGetValue("Required", out xmlAttributeValue))
                {
                    Required = xmlAttributeValue.ToBoolean();
                }

                if (_properties.TryGetValue("SpatialMode", out xmlAttributeValue))
                {
                    if (xmlAttributeValue.Equals("Force", StringComparison.OrdinalIgnoreCase))
                    {
                        SpatialMode = SpatialMode.Force;
                    }
                    else if (xmlAttributeValue.Equals("Only", StringComparison.OrdinalIgnoreCase))
                    {
                        SpatialMode = SpatialMode.Only;
                    }
                    else if (xmlAttributeValue.Equals("None", StringComparison.OrdinalIgnoreCase))
                    {
                        SpatialMode = SpatialMode.None;
                    }
                }

                // Parameterize unless the parameterize attribute is present and specifies not to.
                if (_properties.TryGetValue("Parameterize", out xmlAttributeValue))
                {
                    Parameterize = xmlAttributeValue.ToBoolean();
                }

                // Include unless otherwise specified.
                if (_properties.TryGetValue("Exclude", out xmlAttributeValue))
                {
                    ExcludeFromResult = xmlAttributeValue.ToBoolean();
                }

                if (_properties.TryGetValue("ValidationListType", out xmlAttributeValue))
                {
                    if (xmlAttributeValue.Equals("Both", StringComparison.OrdinalIgnoreCase))
                    {
                        ValidationListType = ValidationListType.Both;
                    }
                    else if (xmlAttributeValue.Equals("ValidationListOnly",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        ValidationListType = ValidationListType.ValidationListOnly;
                    }
                    else if (xmlAttributeValue.Equals("AutoCompleteOnly",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        ValidationListType = ValidationListType.AutoCompleteOnly;
                    }
                }

                // Use case-sensitive processing unless otherwise specified.
                if (_properties.TryGetValue("CaseSensitive", out xmlAttributeValue))
                {
                    _caseSensitive = xmlAttributeValue.ToBoolean();
                }

                // If set, the value should be returned as a single string of delimited values.
                if (_properties.TryGetValue("StringList", out xmlAttributeValue))
                {
                    _stringListDelimiter = xmlAttributeValue;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28934", ex);
            }
        }

        /// <summary>
        /// Specfies that this <see cref="QueryNode"/> is a sub-component to the specified
        /// <see cref="DataEntryQuery"/>.
        /// </summary>
        /// <param name="rootQuery">The <see cref="DataEntryQuery"/> for which this
        /// <see cref="QueryNode"/> is a sub-component.</param>
        public virtual void AddRootQuery(DataEntryQuery rootQuery)
        {
            if (rootQuery != null && rootQuery != this)
            {
                if (!_rootQueries.Contains(rootQuery))
                {
                    _rootQueries.Add(rootQuery);
                }
            }
        }
    }

    #endregion QueryNode

    #region LiteralQueryNode

    /// <summary>
    /// A <see cref="QueryNode"/> consisting of literal text and, optionally, extract systems
    /// path tags.
    /// </summary>
    internal class LiteralQueryNode : QueryNode
    {
        /// <summary>
        /// Initializes a new <see cref="LiteralQueryNode"/> instance.
        /// </summary>
        /// <param name="query">The literal text to be returned during evaluation.</param>
        public LiteralQueryNode(string query)
            : base(MultipleQueryResultSelectionMode.List, false)
        {
            _query = query;

            // A literal query node is the only node type that should default to not being
            // parameterized in an SQL query or expression (since the text would almost certainly
            // be a core part the SQL query or expression rather than a variable).
            Parameterize = false;
        }

        /// <summary>
        /// Gets whether the query node is resolved enough to be evaluated.
        /// </summary>
        /// <returns><see langword="true"/> since a literal query is always resolved.</returns>
        public override bool GetIsMinimallyResolved()
        {
            return true;
        }

        /// <summary>
        /// Gets whether the query node is completely resolved (all required triggers have been
        /// registered) and can be evaluated.
        /// </summary>
        /// <returns><see langword="true"/> since a literal query is always resolved.</returns>
        public override bool GetIsFullyResolved()
        {
            return true;
        }

        /// <summary>
        /// Evaluates the query.
        /// </summary>
        /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
        public override QueryResult Evaluate()
        {
            ExtractException.Assert("ELI26753", "Cannot evaluate un-resolved query!",
                this.GetIsMinimallyResolved());

            // [DataEntry:858]
            // Do not unnecessarily call SourceDocumentPathTags.Expand()-- this method is called a
            // lot and SourceDocumentPathTags.Expand() uses an expensive COM call.
            bool containsPossiblePathTag = (_query.Contains("$") || 
                _query.IndexOf("<SourceDocName>", StringComparison.Ordinal) >= 0);
            string expandedQuery = containsPossiblePathTag ?
                AttributeStatusInfo.SourceDocumentPathTags.Expand(_query) : _query;

            // Treat separate lines as separate values.
            string[] parsedQuery =
                expandedQuery.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            return new QueryResult(SelectionMode, parsedQuery);
        }
    }

    #endregion LiteralQueryNode

    #region SourceDocNameQueryNode

    /// <summary>
    /// A <see cref="QueryNode"/> to be resolved using the current source doc name.
    /// </summary>
    internal class SourceDocNameQueryNode : QueryNode
    {
        /// <summary>
        /// <see langword="true"/> if the full path of the source doc name should be used or
        /// <see langword="false"/> to use just the filename.
        /// </summary>
        bool _useFullPath = true;

        /// <summary>
        /// Initializes a new <see cref="SourceDocNameQueryNode"/> instance.
        /// </summary>
        public SourceDocNameQueryNode()
            : base(MultipleQueryResultSelectionMode.None, false)
        {
        }

        /// <summary>
        /// Gets whether the query node is resolved enough to be evaluated.
        /// </summary>
        /// <returns><see langword="true"/> since a SourceDoc query is always resolved.</returns>
        public override bool GetIsMinimallyResolved()
        {
            return true;
        }

        /// <summary>
        /// Gets whether the query node is completely resolved (all required triggers have been
        /// registered) and can be evaluated.
        /// </summary>
        /// <returns><see langword="true"/> since a SourceDoc query is always resolved.</returns>
        public override bool GetIsFullyResolved()
        {
            return true;
        }

        /// <summary>
        /// Loads the <see cref="SourceDocNameQueryNode"/> using the specified XML query string.
        /// </summary>
        /// <param name="xmlNode">The XML query string defining the query.</param>
        /// <param name="rootQuery">The <see cref="DataEntryQuery"/> that is the root of this
        /// query.</param>
        /// <param name="namedQueries">A communal collection of named <see cref="QueryNode"/>s
        /// available to allow referencing of named nodes by subsequent nodes.</param>
        public override void LoadFromXml(XmlNode xmlNode, DataEntryQuery rootQuery,
            Dictionary<string, QueryNode> namedQueries)
        {
            try
            {
                base.LoadFromXml(xmlNode, rootQuery, namedQueries);

                // Use the full path of the document unless specified not to.
                XmlAttribute xmlAttribute = xmlNode.Attributes["UseFullPath"];
                if (xmlAttribute != null)
                {
                    _useFullPath = xmlAttribute.Value.ToBoolean();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28957", ex);
            }
        }

        /// <summary>
        /// Evaluates the query using the current source doc name.
        /// </summary>
        /// <returns>The current source doc name</returns>
        public override QueryResult Evaluate()
        {
            if (_useFullPath)
            {
                return new QueryResult(SelectionMode, AttributeStatusInfo.SourceDocName);
            }
            else
            {
                return new QueryResult(SelectionMode,
                    Path.GetFileName(AttributeStatusInfo.SourceDocName));
            }
        }
    }

    #endregion SourceDocNameQueryNode

    #region SolutionDirectoryQueryNode

    /// <summary>
    /// A <see cref="QueryNode"/> to insert the solution directory name.
    /// </summary>
    internal class SolutionDirectoryQueryNode : QueryNode
    {
        /// <summary>
        /// Cache the solution directory name after it is calculated the first time since
        /// ConvertToNetworkPath can be slow to run.
        /// </summary>
        static string _solutionDirectory;

        /// <summary>
        /// Used to ensure calculation of _solutionDirectory happens on one thread only.
        /// </summary>
        object _lock = new object();

        /// <summary>
        /// Initializes a new <see cref="SolutionDirectoryQueryNode"/> instance.
        /// </summary>
        public SolutionDirectoryQueryNode()
            : base(MultipleQueryResultSelectionMode.None, false)
        {
        }

        /// <summary>
        /// Gets whether the query node is resolved enough to be evaluated.
        /// </summary>
        /// <returns><see langword="true"/> since this query is always resolved.</returns>
        public override bool GetIsMinimallyResolved()
        {
            return true;
        }

        /// <summary>
        /// Gets whether the query node is completely resolved (all required triggers have been
        /// registered) and can be evaluated.
        /// </summary>
        /// <returns><see langword="true"/> since this query is always resolved.</returns>
        public override bool GetIsFullyResolved()
        {
            return true;
        }

        /// <summary>
        /// Evaluates the query using the solution directory (as a UNC path, if possible).
        /// </summary>
        /// <returns>The query using the solution directory (as a UNC path, if possible).
        /// </returns>
        public override QueryResult Evaluate()
        {
            try
            {
                if (_solutionDirectory == null)
                {
                    // Calculate the solution directory only once for all instances of
                    // SolutionDirectoryQueryNode to minimize the performance hit of
                    // ConvertToNetworkPath.
                    lock (_lock)
                    {
                        if (_solutionDirectory == null)
                        {
                            _solutionDirectory = DataEntryMethods.ResolvePath(".");
                            FileSystemMethods.ConvertToNetworkPath(ref _solutionDirectory, true);
                        }
                    }
                }

                return new QueryResult(SelectionMode, _solutionDirectory);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28931", ex);
            }
        }
    }

    #endregion SolutionDirectoryQueryNode

    #region ResultQueryNode

    /// <summary>
    /// A <see cref="QueryNode"/> to allow access and manipulation of named results.
    /// </summary>
    internal class ResultQueryNode : ComplexQueryNode
    {
        /// <summary>
        /// Combine two results (as lists) together.
        /// </summary>
        const string _COMBINE_LISTS_OPERATION = "CombineLists";

        /// <summary>
        /// Subtract one result (as a list) from another.
        /// <para><b></b></para>
        /// If duplicates exist the same number of duplicates that exist in list2 will be subtracted
        /// from list1. If list1 contains more instances of a value than list2, list1 will have left
        /// over instances.
        /// </summary>
        const string _SUBTRACT_LISTS_OPERATION = "SubtractList";

        /// <summary>
        /// Indicates whether two result lists contain the same values.
        /// <para><b></b></para>
        /// If either list contains duplicates, the result of this operation is undefined.
        /// </summary>
        const string _COMPARE_LISTS_OPERATION = "CompareLists";

        /// <summary>
        /// The name of the result to be used as the first argument.
        /// </summary>
        string _argument1Name;

        /// <summary>
        /// The name of the result to be used as the second argument.
        /// </summary>
        string _argument2Name;

        /// <summary>
        /// The type of operation to be performed on existing results.
        /// </summary>
        string _operation;

        /// <summary>
        /// Initializes a new <see cref="ResultQueryNode"/> instance.
        /// </summary>
        public ResultQueryNode()
            : base(null, null, MultipleQueryResultSelectionMode.None, false)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ResultQueryNode"/> instance.
        /// </summary>
        /// <param name="argument1Name">The name of the result to be used as the first argument.
        /// </param>
        /// <param name="rootQuery">The root <see cref="DataEntryQuery"/> this result node belongs
        /// to.</param>
        /// <param name="namedQueries">A communal collection of named <see cref="QueryNode"/>s
        /// available to allow referencing of named nodes by subsequent nodes.</param>
        public ResultQueryNode(string argument1Name, DataEntryQuery rootQuery,
            Dictionary<string, QueryNode> namedQueries)
            : base(null, null, MultipleQueryResultSelectionMode.None, false)
        {
            _argument1Name = argument1Name;
            AddNamedDependency(_argument1Name, rootQuery, namedQueries);
        }

        /// <summary>
        /// Gets whether the query node is resolved enough to be evaluated.
        /// </summary>
        /// <returns><see langword="true"/> since this query is always resolved.</returns>
        public override bool GetIsMinimallyResolved()
        {
            return base.GetIsMinimallyResolved();
        }

        /// <summary>
        /// Gets whether the query node is completely resolved (all required triggers have been
        /// registered) and can be evaluated.
        /// </summary>
        /// <returns><see langword="true"/> since this query is always resolved.</returns>
        public override bool GetIsFullyResolved()
        {
            return base.GetIsFullyResolved();
        }

        /// <summary>
        /// Loads the <see cref="ResultQueryNode"/> using the specified XML query string.
        /// </summary>
        /// <param name="xmlNode">The XML query string defining the query.</param>
        /// <param name="rootQuery">The <see cref="DataEntryQuery"/> that is the root of this
        /// query.</param>
        /// <param name="namedQueries">A communal collection of named <see cref="QueryNode"/>s
        /// available to allow referencing of named nodes by subsequent nodes.</param>
        public override void LoadFromXml(XmlNode xmlNode, DataEntryQuery rootQuery,
            Dictionary<string, QueryNode> namedQueries)
        {
            try
            {
                base.LoadFromXml(xmlNode, rootQuery, namedQueries);

                // An operation is not required, but if provieded, ensure a valid operation was
                // specified.
                XmlAttribute xmlAttribute = xmlNode.Attributes["Operation"];
                if (xmlAttribute != null)
                {
                    _operation = xmlAttribute.Value;
                    if (!string.IsNullOrEmpty(_operation) &&
                        !_operation.Equals(
                            _COMBINE_LISTS_OPERATION, StringComparison.OrdinalIgnoreCase) &&
                        !_operation.Equals(
                            _SUBTRACT_LISTS_OPERATION, StringComparison.OrdinalIgnoreCase) &&
                        !_operation.Equals(
                            _COMPARE_LISTS_OPERATION, StringComparison.OrdinalIgnoreCase))
                    {
                        ExtractException ee =
                            new ExtractException("ELI28936", "Invalid list operation!");
                        ee.AddDebugData("Operation", _operation, false);
                        throw ee;
                    }
                }

                // Arg1 is always required.
                if (Properties.TryGetValue("Arg1", out _argument1Name))
                {
                    AddNamedDependency(_argument1Name, rootQuery, namedQueries);
                }
                else
                {
                    ExtractException ee = new ExtractException("ELI31590",
                        "List query missing argument 1!");
                    ee.AddDebugData("Name", _argument1Name, false);
                    throw ee;
                }

                // Arg2 is optional.
                if (Properties.TryGetValue("Arg2", out _argument2Name))
                {
                    AddNamedDependency(_argument2Name, rootQuery, namedQueries);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28932", ex);
            }
        }

        /// <summary>
        /// Evaluates the query by retrieving and manipulating the previous results as configured.
        /// </summary>
        /// <returns>The QueryResult representing the results of the operation.</returns>
        public override QueryResult Evaluate()
        {
            try
            {
                // Retrieve both arguments.
                QueryResult argument1 = GetNamedResult(_argument1Name);

                // If no operation is specified, simply return argument 1.
                if (string.IsNullOrEmpty(_operation))
                {
                    if (!string.IsNullOrEmpty(Name))
                    {
                        _namedResult = new QueryResult(argument1);
                    }
                    return argument1;
                }
                
                QueryResult argument2 = GetNamedResult(_argument2Name);
                
                // Combine the results if specified
                if (_operation.Equals(_COMBINE_LISTS_OPERATION, StringComparison.OrdinalIgnoreCase))
                {
                    argument1.SelectionMode = MultipleQueryResultSelectionMode.List;
                    argument2.SelectionMode = MultipleQueryResultSelectionMode.List;
                    return QueryResult.Combine(argument1, argument2);
                }

                // Otherwise, retrieve a string array of argument2 values to use for list operations.
                string[] argument2Strings = argument2.ToStringArray();
                if (!CaseSensitive)
                {
                    for (int i = 0; i < argument2Strings.Length; i++)
                    {
                        argument2Strings[i] =
                            argument2Strings[i].ToUpper(CultureInfo.CurrentCulture);
                    }
                }

                QueryResult uniqueToArgument1Head = null;
                QueryResult uniqueToArgument1Tail = null;
                int matchingCount = 0;

                // Loop through each value in argument1 to compile a list of values unique to
                // argument1.
                foreach (QueryResult argument1Value in argument1)
                {
                    string value = argument1Value.FirstStringValue;
                    if (!CaseSensitive)
                    {
                        value = value.ToUpper(CultureInfo.CurrentCulture);
                    }

                    if (Array.IndexOf(argument2Strings, value, 0) >= 0)
                    {
                        matchingCount++;
                    }
                    else
                    {
                        // If there's any item unique to argument 1, the two lists are not equal.
                        if (_operation.Equals(
                                _COMPARE_LISTS_OPERATION, StringComparison.OrdinalIgnoreCase))
                        {
                            return new QueryResult(MultipleQueryResultSelectionMode.None, "0");
                        }
                        // If this is the first unique value, initialize the unique value list.
                        else if (uniqueToArgument1Head == null)
                        {
                            uniqueToArgument1Head = argument1Value.CreateFirstValueCopy();
                            uniqueToArgument1Tail = uniqueToArgument1Head;
                        }
                        // If this is not the first unique value, append to the unique value list.
                        else
                        {
                            uniqueToArgument1Tail.NextValue =
                                argument1Value.CreateFirstValueCopy();
                            uniqueToArgument1Tail = uniqueToArgument1Tail.NextValue;
                        }
                    }
                }

                // If comparing lists and we haven't yet returned false, list 2 has everything in
                // list 1. Consider them equal if the counts in the two lists are the same.
                if (_operation.Equals(_COMPARE_LISTS_OPERATION, StringComparison.OrdinalIgnoreCase))
                {
                    if (matchingCount == argument2Strings.Length)
                    {
                        return new QueryResult(MultipleQueryResultSelectionMode.None, "1");
                    }
                    else
                    {
                        return new QueryResult(MultipleQueryResultSelectionMode.None, "0");
                    }
                }
                // If subtracting arguement 2, return the list of values unique to argument 1 that
                // have been compiled.
                else if (_operation.Equals(
                            _SUBTRACT_LISTS_OPERATION, StringComparison.OrdinalIgnoreCase))
                {
                    if (uniqueToArgument1Head == null)
                    {
                        return new QueryResult();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Name))
                        {
                            _namedResult = new QueryResult(uniqueToArgument1Head);
                        }
                        return uniqueToArgument1Head;
                    }
                }

                throw new ExtractException("ELI28939", "Error evaluating query list!");
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28933", ex);
            }
        }
    }

    #endregion ResultQueryNode

    #region ComplexQueryNode

    /// <summary>
    /// A <see cref="QueryNode"/> that is comprised of one or more child
    /// <see cref="QueryNode"/>s.
    /// </summary>
    internal class ComplexQueryNode : QueryNode
    {
        /// <summary>
        /// The child QueryNodes of this query node.
        /// </summary>
        protected List<QueryNode> _childNodes = new List<QueryNode>();

        /// <summary>
        /// The path of the rootAttribute.
        /// </summary>
        protected string _rootPath;

        /// <summary>
        /// Caches the <see cref="GetIsFullyResolved"/> status of the query node.
        /// <see langword="null"/> if there is no cached value and GetIsFullyResolved needs to
        /// be re-calculated.
        /// </summary>
        bool? _isFullyResolved;

        /// <summary>
        /// Caches the <see cref="GetIsMinimallyResolved"/> status of the query node.
        /// <see langword="null"/> if there is no cached value and GetIsMinimallyResolved needs to
        /// be re-calculated.
        /// </summary>
        bool? _isMinimallyResolved;

        /// <summary>
        /// The <see cref="IAttribute"/> that should be considered the root of any attribute query.
        /// </summary>
        IAttribute _rootAttribute;

        /// <summary>
        /// The <see cref="DbConnection"/> that should be used to evaluate any SQL queries.
        /// </summary>
        DbConnection _dbConnection;

        /// <summary>
        /// The named <see cref="QueryNode"/>s which this <see cref="QueryNode"/> references.
        /// </summary>
        protected Dictionary<string, QueryNode> _namedDependencies =
            new Dictionary<string, QueryNode>();

        /// <overrides>
        /// Initializes a new <see cref="ComplexQueryNode"/> instance.
        /// </overrides>
        /// <summary>
        /// Initializes a new <see cref="ComplexQueryNode"/> instance.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that should be used to
        /// evaluate any SQL queries.</param>
        /// <param name="resolveOnLoad"><see langword="true"/> if the query will resolve itself if
        /// possible as it is loaded, <see langword="false"/> if
        /// <see cref="RegisterTriggerCandidate"/> must be  called manually to resolve the query if
        /// attribute triggers are involved.</param>
        public ComplexQueryNode(IAttribute rootAttribute, DbConnection dbConnection,
            bool resolveOnLoad)
            : this(rootAttribute, dbConnection, MultipleQueryResultSelectionMode.None, resolveOnLoad)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ComplexQueryNode"/> instance.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that should be used to
        /// evaluate any SQL queries.</param>
        /// <param name="selectionMode">The <see cref="MultipleQueryResultSelectionMode"/> that
        /// should be used to determine how multiple resulting values should be handled.</param>
        /// <param name="resolveOnLoad"><see langword="true"/> if the query will resolve itself if
        /// possible as it is loaded, <see langword="false"/> if
        /// <see cref="RegisterTriggerCandidate"/> must be  called manually to resolve the query if
        /// attribute triggers are involved.</param>
        public ComplexQueryNode(IAttribute rootAttribute, DbConnection dbConnection,
            MultipleQueryResultSelectionMode selectionMode, bool resolveOnLoad)
            : base(selectionMode, resolveOnLoad)
        {
            try
            {
                _rootAttribute = rootAttribute;
                _rootPath = AttributeStatusInfo.GetFullPath(_rootAttribute);
                _dbConnection = dbConnection;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28865", ex);
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
        /// Gets the path of the rootAttribute.
        /// </summary>
        /// <returns>The path of the rootAttribute.</returns>
        public virtual string RootPath
        {
            get
            {
                return _rootPath;
            }
        }

        /// <summary>
        /// Gets the <see cref="DbConnection"/> that should be used to evaluate any SQL queries.
        /// </summary>
        /// <returns>The <see cref="DbConnection"/> that should be used to evaluate any SQL queries.
        /// </returns>
        public virtual DbConnection DbConnection
        {
            get
            {
                return _dbConnection;
            }
        }

        /// <summary>
        /// Loads the <see cref="QueryNode"/> using the specified XML query string.
        /// </summary>
        /// <param name="xmlNode">The XML query string defining the query.</param>
        /// <param name="rootQuery">The <see cref="DataEntryQuery"/> that is the root of this
        /// query.</param>
        /// <param name="namedQueries">A communal collection of named <see cref="QueryNode"/>s
        /// available to allow referencing of named nodes by subsequent nodes.</param>
        public override void LoadFromXml(XmlNode xmlNode, DataEntryQuery rootQuery,
            Dictionary<string, QueryNode> namedQueries)
        {
            try
            {
                _query = xmlNode.InnerXml;
                AddRootQuery(rootQuery);

                base.LoadFromXml(xmlNode, rootQuery, namedQueries);

                // Iterate through each child of the current XML node and use each to initialize a
                // new child QueryNode.
                foreach (XmlNode childNode in xmlNode.ChildNodes)
                {
                    // If the node is text, initialize a LiteralQueryNode node.
                    if (childNode.NodeType == XmlNodeType.Text)
                    {
                        _childNodes.Add(new LiteralQueryNode(childNode.InnerText));
                    }
                    else if (childNode.NodeType == XmlNodeType.Element)
                    {
                        QueryNode childQueryNode = null;
                        XmlElement childElement = ((XmlElement)childNode);

                        // Check for SourceDocName (which is not a ComplexQueryNode). 
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
                        else if (childElement.Name.Equals("Result",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            childQueryNode = new ResultQueryNode();
                        }
                        else if (childElement.Name.Equals("SQL",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            childQueryNode =
                                new SqlQueryNode(RootAttribute, DbConnection, ResolveOnLoad);
                        }
                        else if (childElement.Name.Equals("Attribute",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            childQueryNode =
                                new AttributeQueryNode(RootAttribute, DbConnection, ResolveOnLoad);
                        }
                        else if (childElement.Name.Equals("Complex",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            childQueryNode =
                                new ComplexQueryNode(RootAttribute, DbConnection, ResolveOnLoad);
                        }
                        else if (childElement.Name.Equals("Regex",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            childQueryNode =
                                new RegexQueryNode(RootAttribute, DbConnection, ResolveOnLoad);
                        }
                        else if (childElement.Name.Equals("Expression",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            childQueryNode =
                                new ExpressionQueryNode(RootAttribute, DbConnection, ResolveOnLoad);
                        }
                        else if (namedQueries.ContainsKey(childElement.Name))
                        {
                            // If the node name matches the name of one of the name QueryNodes,
                            // create this node as a single-argument ResultQueryNode.
                            childQueryNode = new ResultQueryNode();
                            childQueryNode.Properties["Arg1"] = childElement.Name;
                        }
                        else
                        {
                            ExtractException ee = new ExtractException("ELI26726", "Unrecognized element in query.");
                            ee.AddDebugData("Element Name", childElement.Name, false);
                            throw ee;
                        }

                        // If there is only one child query node, assume the user is going to want
                        // it to have the same selection mode as the overall query by default.
                        if (xmlNode.ChildNodes.Count == 1)
                        {
                            childQueryNode.SelectionMode = SelectionMode;
                        }

                        childQueryNode.LoadFromXml(childNode, rootQuery, namedQueries);

                        if (!string.IsNullOrEmpty(childQueryNode.Name))
                        {
                            _namedDependencies[childQueryNode.Name] = childQueryNode;
                        }

                        _childNodes.Add(childQueryNode);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28958", ex);
            }
        }

        /// <summary>
        /// Specfies that this <see cref="QueryNode"/> is a sub-component to the specified
        /// <see cref="DataEntryQuery"/>.
        /// </summary>
        /// <param name="rootQuery">The <see cref="DataEntryQuery"/> for which this
        /// <see cref="QueryNode"/> is a sub-component.</param>
        public override void AddRootQuery(DataEntryQuery rootQuery)
        {
            base.AddRootQuery(rootQuery);

            foreach (QueryNode childNode in _childNodes)
            {
                childNode.AddRootQuery(rootQuery);
            }
        }

        /// <summary>
        /// Links the <see cref="QueryNode"/> referenced by <see paramref="resultName"/> to this
        /// node.
        /// </summary>
        /// <param name="resultName">The name of the <see cref="QueryNode"/> that is to be an
        /// argument.</param>
        /// <param name="rootQuery">The <see cref="DataEntryQuery"/> that is the root of this
        /// query.</param>
        /// <param name="namedQueries">A communal collection of named <see cref="QueryNode"/>s
        /// available to allow referencing of named nodes by subsequent nodes.</param>
        protected void AddNamedDependency(string resultName, DataEntryQuery rootQuery,
            Dictionary<string, QueryNode> namedQueries)
        {
            if (!_namedDependencies.ContainsKey(resultName))
            {
                QueryNode namedQuery;
                if (!namedQueries.TryGetValue(resultName, out namedQuery))
                {
                    ExtractException ee = new ExtractException("ELI31977", "Undefined query!");
                    ee.AddDebugData("Name", resultName, false);
                    throw ee;
                }
                namedQuery.AddRootQuery(rootQuery);
                _namedDependencies[resultName] = namedQuery;
                _childNodes.Add(namedQuery);
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
            return _namedDependencies[name].Evaluate();
        }

        /// <summary>
        /// Causes the values returned by <see cref="GetIsFullyResolved"/> and
        /// <see cref="GetIsMinimallyResolved"/> to be re-calculated.
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
        public override bool GetIsMinimallyResolved()
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
                if (!childNode.GetIsMinimallyResolved())
                {
                    _isMinimallyResolved = false;
                    break;
                }
            }

            return _isMinimallyResolved.Value;
        }

        /// <summary>
        /// Gets whether the query node is completely resolved (all required triggers have been
        /// registered) and can be evaluated.
        /// </summary>
        /// <returns><see langword="true"/> if all triggers are registered and the query node can
        /// be evaluated, <see langword="false"/> if one or more triggers must be resolved before
        /// the query node can be evaluated.</returns>
        public override bool GetIsFullyResolved()
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
                if (!childNode.GetIsFullyResolved())
                {
                    _isFullyResolved = false;
                    break;
                }
            }

            return _isFullyResolved.Value;
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
            if (!this.GetIsFullyResolved())
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
                QueryResult result = new QueryResult();

                // Combine the results of all child nodes.
                foreach (QueryNode childNode in _childNodes)
                {
                    QueryResult childQueryResult = childNode.Evaluate();

                    // Flatten the result into a delimited string if specified.
                    if (!string.IsNullOrEmpty(childNode.StringListDelimiter))
                    {
                        childQueryResult.ConvertToDelimitedStringList(childNode.StringListDelimiter);
                    }

                    // Combine the results of all child nodes except those flagged to be excluded.
                    if (!childNode.ExcludeFromResult)
                    {
                        result = QueryResult.Combine(result, childQueryResult);
                    }
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

    #endregion ComplexQueryNode

    #region SqlQueryNode

    /// <summary>
    /// A <see cref="QueryNode"/> that is to be resolved using an SQL query against the active
    /// database.
    /// </summary>
    internal class SqlQueryNode : ComplexQueryNode
    {
        /// <summary>
        /// Initializes a new <see cref="SqlQueryNode"/> instance.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that should be used to
        /// evaluate any SQL queries.</param>
        /// <param name="resolveOnLoad"><see langword="true"/> if the query will resolve itself if
        /// possible as it is loaded, <see langword="false"/> if RegisterTriggerCandidate must be
        /// called manually to resolve the query if attribute triggers are involved.</param>
        public SqlQueryNode(IAttribute rootAttribute, DbConnection dbConnection, bool resolveOnLoad)
            : base(rootAttribute, dbConnection, MultipleQueryResultSelectionMode.List, resolveOnLoad)
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
                    this.GetIsMinimallyResolved());

                SqlCeConnection sqlCeConnection = DbConnection as SqlCeConnection;

                ExtractException.Assert("ELI26733",
                    "Unable to evaluate query without SQL CE database connection!",
                    sqlCeConnection != null);

                QueryResult result = new QueryResult();

                StringBuilder sqlQuery = new StringBuilder();

                // Keep track of the combined spatial result for all child nodes which have a
                // spatial mode of "Force".
                SpatialString spatialResult = null;

                // Child query nodes whose results have been parameterized.
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                // Combine the result of all child queries parameterizing as necessary.
                foreach (QueryNode childNode in _childNodes)
                {
                    QueryResult childQueryResult = childNode.Evaluate();

                    // Evaluate, but don't include any excluded child results.
                    if (childNode.ExcludeFromResult)
                    {
                        continue;
                    }

                    // If the current child has a spatial mode of "Force", store its spatial
                    // info so it can be applied to the query result later.
                    if (childQueryResult.IsSpatial && childNode.SpatialMode == SpatialMode.Force)
                    {
                        if (spatialResult == null)
                        {
                            ICopyableObject copySource =
                                (ICopyableObject)childQueryResult.FirstSpatialStringValue;
                            spatialResult = (SpatialString)copySource.Clone();
                        }
                        else
                        {
                            spatialResult.Append(childQueryResult.FirstSpatialStringValue);
                        }
                    }

                    if (childNode.Parameterize)
                    {
                        // If parameterizing, don't add the query result directly, rather add a
                        // parameter name to the query and add the key/value pair to parameters.
                        string key = "@" + parameters.Count.ToString(CultureInfo.InvariantCulture);
                        string value = childQueryResult.ToString();

                        parameters[key] = value;
                        sqlQuery.Append(key);
                    }
                    else
                    {
                        sqlQuery.Append(childQueryResult.ToString());
                    }
                }

                // Create a database command using the query.
                using (DbCommand dbCommand = DataEntryMethods.CreateDBCommand(
                    sqlCeConnection, sqlQuery.ToString(), parameters))
                {
                    // Execute the query.
                    string[] queryResults = DataEntryMethods.ExecuteDBQuery(dbCommand, ", ");

                    if (queryResults.Length == 0)
                    {
                        // Create an empty result if nothing was found (but preserve spatial info).
                        if (spatialResult != null && spatialResult.HasSpatialInfo())
                        {
                            spatialResult.ReplaceAndDowngradeToHybrid("");
                            result = new QueryResult(SelectionMode, spatialResult);
                        }
                        else
                        {
                            result = new QueryResult();
                        }
                    }
                    // Apply the spatial infomation of child nodes with "Force" spatial mode if
                    // necessary.
                    else if (spatialResult != null && spatialResult.HasSpatialInfo())
                    {
                        SpatialString[] spatialResults = new SpatialString[queryResults.Length];
                        ICopyableObject copySource = (ICopyableObject)spatialResult;

                        if (queryResults.Length == 1)
                        {
                            spatialResults[0] = (SpatialString)copySource.Clone();
                            spatialResults[0].ReplaceAndDowngradeToHybrid(queryResults[0]);
                        }
                        else
                        {
                            for (int i = 0; i < queryResults.Length; i++)
                            {
                                SpatialString spatialString = (SpatialString)copySource.Clone();
                                spatialString.ReplaceAndDowngradeToHybrid(queryResults[i]);
                                spatialResults[i] = spatialString;
                            }
                        }

                        result = new QueryResult(SelectionMode, spatialResults);
                    }
                    // Otherwise, just return the text value.
                    else
                    {
                        result = new QueryResult(SelectionMode, queryResults);
                    }
                }

                return result;
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

    #endregion SqlQueryNode

    #region AttributeQueryNode

    /// <summary>
    /// A <see cref="QueryNode"/> that is to be resolved using the value of an
    /// <see cref="IAttribute"/>. Changes made to this <see cref="IAttribute"/> will cause the
    /// target attribute to be updated.
    /// </summary>
    internal class AttributeQueryNode : ComplexQueryNode
    {
        // The attribute to evaluate the query and trigger evaluation.
        List<IAttribute> _triggerAttributes = new List<IAttribute>();

        /// <summary>
        /// The full path of any attribute matching the value query.  Used for efficiency when
        /// evaluating candidate triggers.
        /// </summary>
        string _attributeValueFullPath;

        /// <summary>
        /// A <see cref="ResultQueryNode"/> which refers to the <see cref="IAttribute"/>(s) which
        /// should serve as the root of the path to the target <see cref="IAttribute"/>(s).
        /// </summary>
        ResultQueryNode _rootAttributeResultQuery;

        /// <summary>
        /// Specifies whether changes to the attribute should trigger the parent
        /// <see cref="DataEntryQuery"/> to update.
        /// </summary>
        bool _triggerUpdate = true;

        /// <summary>
        /// Initialized a new <see cref="AttributeQueryNode"/> instance.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that should be used to
        /// evaluate any SQL queries.</param>
        /// <param name="resolveOnLoad"><see langword="true"/> if the query will resolve itself if
        /// possible as it is loaded, <see langword="false"/> if RegisterTriggerCandidate must be
        /// called manually to resolve the query if attribute triggers are involved.</param>
        public AttributeQueryNode(IAttribute rootAttribute, DbConnection dbConnection,
            bool resolveOnLoad)
            : base(rootAttribute, dbConnection, MultipleQueryResultSelectionMode.None, resolveOnLoad)
        {
        }

        /// <summary>
        /// Loads the <see cref="AttributeQueryNode"/> using the specified XML query string.
        /// </summary>
        /// <param name="xmlNode">The XML query string defining the query.</param>
        /// <param name="rootQuery">The <see cref="DataEntryQuery"/> that is the root of this
        /// query.</param>
        /// <param name="namedQueries">A communal collection of named <see cref="QueryNode"/>s
        /// available to allow referencing of named nodes by subsequent nodes.</param>
        public override void LoadFromXml(XmlNode xmlNode, DataEntryQuery rootQuery,
            Dictionary<string, QueryNode> namedQueries)
        {
            try
            {
                base.LoadFromXml(xmlNode, rootQuery, namedQueries);

                // Changes to the attribute should trigger an update unless specified not to.
                XmlAttribute xmlAttribute = xmlNode.Attributes["TriggerUpdate"];
                if (xmlAttribute != null)
                {
                    _triggerUpdate = xmlAttribute.Value.ToBoolean();
                }

                // If the query begins with a slash, search from the root of the attribute
                // hierarchy, not from the location of the root attribute.
                if (_query.StartsWith("/", StringComparison.Ordinal))
                {
                    _rootPath = "";
                    RootAttribute = null;
                }
                else
                {
                    xmlAttribute = xmlNode.Attributes["Root"];
                    if (xmlAttribute != null && !string.IsNullOrEmpty(xmlAttribute.Value))
                    {
                        _rootAttributeResultQuery =
                            new ResultQueryNode(xmlAttribute.Value, rootQuery, namedQueries);
                        _rootAttributeResultQuery.ExcludeFromResult = true;
                        _childNodes.Add(_rootAttributeResultQuery);
                        RootAttribute = null;
                    }
                    else if (ResolveOnLoad)
                    {
                        // After loading query node, attempt to register a trigger for it.
                        RegisterTriggerCandidate(null);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26756", ex);
                ee.AddDebugData("XML", xmlNode.InnerXml, false);
                throw ee;
            }
        }

        /// <summary>
        /// Specfies that this <see cref="QueryNode"/> is a sub-component to the specified
        /// <see cref="DataEntryQuery"/>.
        /// </summary>
        /// <param name="rootQuery">The <see cref="DataEntryQuery"/> for which this
        /// <see cref="QueryNode"/> is a sub-component.</param>
        public override void AddRootQuery(DataEntryQuery rootQuery)
        {
            // Determine whether trigger(s) needs to be set on rootQuery so that it is re-evaluated
            // every time this node is evaluated.
            bool setTrigger = _triggerUpdate &&
                              _triggerAttributes.Count > 0 &&
                              rootQuery != null &&
                              !_rootQueries.Contains(rootQuery);

            base.AddRootQuery(rootQuery);

            // Set the triggers if necessary.
            if (setTrigger)
            {
                foreach (IAttribute triggerAttribute in _triggerAttributes)
                {
                    rootQuery.SetTrigger(triggerAttribute,
                        SelectionMode != MultipleQueryResultSelectionMode.None);
                }
            }
        }

        /// <summary>
        /// Gets whether the query node is resolved enough to be evaluated.
        /// </summary>
        /// <returns><see langword="true"/> if all required triggers are registered and the
        /// query node can be evaluated, <see langword="false"/> if one or more triggers must
        /// be resolved before the query node can be evaluated.</returns>
        public override bool GetIsMinimallyResolved()
        {
            // If a triggerAttribute was previously registered, but the query used to
            // register it is no longer resolved, unregister the trigger attribute.
            if (_triggerAttributes.Count > 0 && !base.GetIsMinimallyResolved())
            {
                UnregisterTriggerAttributes();
            }

            // The node is minimally resolved if its not required,
            // the root attribute is to be resolved when evaluated or at least one trigger
            // attribute has been resolved.
            return (base.GetIsMinimallyResolved() &&
                    (!base.Required || _triggerAttributes.Count > 0 ||
                     SelectionMode == MultipleQueryResultSelectionMode.List ||
                     SelectionMode == MultipleQueryResultSelectionMode.Distinct ||
                     _rootAttributeResultQuery != null ||
                     _query.StartsWith("/", StringComparison.Ordinal)));
        }

        /// <summary>
        /// Gets whether the query node is completely resolved (all required triggers have been
        /// registered) and can be evaluated.
        /// </summary>
        /// <returns><see langword="true"/> if all triggers are registered and the query node can
        /// be evaluated, <see langword="false"/> if one or more triggers must be resolved before
        /// the query node can be evaluated.</returns>
        public override bool GetIsFullyResolved()
        {
            // If a triggerAttribute was previously registered, but the query used to
            // register it is no longer resolved, unregister the trigger attribute.
            if (SelectionMode == MultipleQueryResultSelectionMode.None &&
                _triggerAttributes.Count == 1  && !base.GetIsFullyResolved())
            {
                UnregisterTriggerAttributes();
            }

            // The node can only be fully resolve if selection mode equals none, in which case
            // assuming the first resolve attempt finds one and only one matching attribute, no
            // further attempts will be made to look for new matching attributes.
            return (SelectionMode == MultipleQueryResultSelectionMode.None && 
                _triggerAttributes.Count == 1 && base.GetIsFullyResolved());
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
                    this.GetIsMinimallyResolved());

                // If the RootAttribute needs to be resolved at evaluation time.
                List<IAttribute> resultingAttributes = new List<IAttribute>();
                if (_rootAttributeResultQuery != null)
                {
                    QueryResult namedResult = _rootAttributeResultQuery.Evaluate();
                    if (namedResult.IsAttribute)
                    {
                        // There may be multiple root attributes. Add the results using each root
                        // to the result.
                        foreach (IAttribute attribute in namedResult.ToAttributeArray())
                        {
                            try
                            {
                                RootAttribute = attribute;
                                RegisterTriggerCandidate(null);
                                resultingAttributes.AddRange(_triggerAttributes);
                            }
                            finally
                            {
                                // If the root attribute is specified by name, the RootAttribute
                                // should exist in the current evaluation context only.
                                UnregisterTriggerAttributes();
                                RootAttribute = null;
                            }
                        }
                    }
                }
                else if (!GetIsFullyResolved())
                {
                    RegisterTriggerCandidate(null);
                    resultingAttributes.AddRange(_triggerAttributes);
                }
                else
                {
                    // If the root does not need to be resolved or already is resolved, use the
                    // already registered trigger attributes.
                    resultingAttributes = _triggerAttributes;
                }

                QueryResult results = null;

                if (resultingAttributes.Count > 0)
                {
                    results = new QueryResult(SelectionMode, resultingAttributes.ToArray());

                    // Modify results to reflect Only or None spatial mode settings.
                    if (SpatialMode == SpatialMode.Only)
                    {
                        foreach (QueryResult result in results)
                        {
                            result.FirstStringValue = "";
                        }
                    }
                    else if (SpatialMode == SpatialMode.None)
                    {
                        results.RemoveSpatialInfo();
                    }
                }
                
                return results ?? new QueryResult();
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
            if (!this.GetIsFullyResolved() && base.GetIsMinimallyResolved())
            {
                string childQueryString = base.Evaluate().ToString();

                if (string.IsNullOrEmpty(_attributeValueFullPath))
                {
                    _attributeValueFullPath = 
                        AttributeStatusInfo.GetFullPath(RootPath, childQueryString);
                }

                // Test to see that if an attribute was supplied, its path matches the path
                // we would expect for a trigger attribute.
                if (statusInfo == null || statusInfo.FullPath == _attributeValueFullPath)
                {
                    // Search for candidate triggers.
                    List <IAttribute> foundTriggers =
                        AttributeStatusInfo.ResolveAttributeQuery(RootAttribute, childQueryString);

                    // Pare down the list as appropriate per MultiSelectionMode
                    if (foundTriggers.Count > 1)
                    {
                        if (SelectionMode == MultipleQueryResultSelectionMode.First)
                        {
                            foundTriggers.RemoveRange(1, foundTriggers.Count - 1);
                        }
                        else if (SelectionMode == MultipleQueryResultSelectionMode.None)
                        {
                            foundTriggers.Clear();
                        }
                    }

                    // Unregister any existing triggers that are not in the newly found set.
                    foreach (IAttribute existingTriggerAttribute in _triggerAttributes)
                    {
                        if (!foundTriggers.Contains(existingTriggerAttribute))
                        {
                            UnregisterTriggerAttribute(existingTriggerAttribute);
                        }
                    }

                    // Register any found triggers that are not already registered.
                    foreach (IAttribute foundTrigger in foundTriggers)
                    {
                        if (!_triggerAttributes.Contains(foundTrigger))
                        {
                            RegisterTriggerAttribute(foundTrigger);

                            resolved = true;
                        }
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
                    _triggerAttributes.Contains(e.DeletedAttribute));

                // Unregister the attribute as a trigger for all terms it is currently used in.
                UnregisterTriggerAttribute(e.DeletedAttribute);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26718", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Registers the specified <see cref="IAttribute"/> as a trigger attribute.
        /// </summary>
        /// <param name="triggerAttribute">The <see cref="IAttribute"/> to be a trigger.</param>
        void RegisterTriggerAttribute(IAttribute triggerAttribute)
        {
            AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(triggerAttribute);
            statusInfo.FullPath = _attributeValueFullPath;

            // Handle deletion of the attribute so the query will become unresolved
            // if the trigger attribute is deleted.
            statusInfo.AttributeDeleted += HandleAttributeDeleted;

            // Set a trigger for the attribute on all root nodes.
            if (_triggerUpdate)
            {
                foreach (DataEntryQuery rootQuery in _rootQueries)
                {
                    rootQuery.SetTrigger(triggerAttribute,
                        SelectionMode != MultipleQueryResultSelectionMode.None);
                }
            }
            
            _triggerAttributes.Add(triggerAttribute);
        }

        /// <summary>
        /// Un-registers all active trigger attribute.
        /// </summary>
        void UnregisterTriggerAttributes()
        {
            foreach (IAttribute triggerAttribute in new List<IAttribute>(_triggerAttributes))
            {
                UnregisterTriggerAttribute(triggerAttribute);
            }
        }

        /// <summary>
        /// Un-registers the specified trigger attribute.
        /// </summary>
        /// <param name="triggerAttribute">The <see cref="IAttribute"/> to unregister as a trigger.
        /// </param>
        void UnregisterTriggerAttribute(IAttribute triggerAttribute)
        {
            ExtractException.Assert("ELI28924", "Failed to unregister missing query attribute!",
                _triggerAttributes.Contains(triggerAttribute));

            AttributeStatusInfo statusInfo =
                            AttributeStatusInfo.GetStatusInfo(triggerAttribute);
            statusInfo.AttributeDeleted -= HandleAttributeDeleted;

            foreach (DataEntryQuery rootQuery in _rootQueries)
            {
                rootQuery.ClearTrigger(triggerAttribute,
                    SelectionMode != MultipleQueryResultSelectionMode.None);
            }

            _triggerAttributes.Remove(triggerAttribute);
        }

        #endregion Private Members
    }

    #endregion SqlQueryNode

    #region RegexQueryNode

    /// <summary>
    /// A <see cref="QueryNode"/> that is resolved by extracting matches in the query's value for
    /// the provided regex pattern.
    /// </summary>
    internal class RegexQueryNode : ComplexQueryNode
    {
        /// <summary>
        /// The <see cref="DotNetRegexParser"/> used to search for regex matches.
        /// </summary>
        DotNetRegexParser _regexParser = new DotNetRegexParser();

        /// <summary>
        /// Indicates whether only the first match should be returned or all matches should be
        /// returned.
        /// </summary>
        bool _firstMatchOnly = true;

        /// <summary>
        /// Initializes a new <see cref="RegexQueryNode"/> instance.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that should be used to
        /// evaluate any SQL queries.</param>
        /// <param name="resolveOnLoad"><see langword="true"/> if the query will resolve itself if
        /// possible as it is loaded, <see langword="false"/> if RegisterTriggerCandidate must be
        /// called manually to resolve the query if attribute triggers are involved.</param>
        public RegexQueryNode(IAttribute rootAttribute, DbConnection dbConnection, bool resolveOnLoad)
            : base(rootAttribute, dbConnection, MultipleQueryResultSelectionMode.List, resolveOnLoad)
        {
        }

        /// <summary>
        /// Loads the <see cref="RegexQueryNode"/> using the specified XML query string.
        /// </summary>
        /// <param name="xmlNode">The XML query string defining the query.</param>
        /// <param name="rootQuery">The <see cref="DataEntryQuery"/> that is the root of this
        /// query.</param>
        /// <param name="namedQueries">A communal collection of named <see cref="QueryNode"/>s
        /// available to allow referencing of named nodes by subsequent nodes.</param>
        public override void LoadFromXml(XmlNode xmlNode, DataEntryQuery rootQuery,
            Dictionary<string, QueryNode> namedQueries)
        {
            try
            {
                base.LoadFromXml(xmlNode, rootQuery, namedQueries);

                // Changes to the attribute should trigger an update unless specified not to.
                XmlAttribute xmlAttribute = xmlNode.Attributes["Pattern"];
                ExtractException.Assert("ELI31978",
                    "Regex query node must contain a \"Pattern\" attribute.", xmlAttribute != null);

                _regexParser.Pattern = xmlAttribute.Value;

                xmlAttribute = xmlNode.Attributes["FirstMatchOnly"];
                if (xmlAttribute != null)
                {
                    _firstMatchOnly = xmlAttribute.Value.ToBoolean();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31981", ex);
            }
        }

        /// <summary>
        /// Evaluates the query by searching the results of <see cref="ComplexQueryNode.Evaluate"/>
        /// with the configured regex pattern.
        /// </summary>
        /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
        public override QueryResult Evaluate()
        {
            try
            {
                ExtractException.Assert("ELI31979", "Cannot evaluate un-resolved query!",
                    this.GetIsMinimallyResolved());

                // The string to search is the result of the base class's evaluation.
                string childQueryString = base.Evaluate().ToString();

                // Search for regex matches
                IUnknownVector regexResults =
                    _regexParser.Find(childQueryString, _firstMatchOnly, false);
                
                // Convert the matches to a string array.
                string[] matches = regexResults.ToIEnumerable<IObjectPair>()
                    .Select(resultPair => ((Token)(resultPair.Object1)).Value)
                    .ToArray();

                return new QueryResult(SelectionMode, matches);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31980", ex);
            }
        }
    }

    #endregion RegexQueryNode

    #region ExpressionQueryNode

    /// <summary>
    /// A <see cref="QueryNode"/> that is resolved by evaluating an expression using the Spring.Net
    /// expression evaluation engine.
    /// </summary>
    internal class ExpressionQueryNode : ComplexQueryNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionQueryNode"/> class.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that should be used to
        /// evaluate any SQL queries.</param>
        /// <param name="resolveOnLoad"><see langword="true"/> if the query will resolve itself if
        /// possible as it is loaded, <see langword="false"/> otherwise.</param>
        /// <overrides>
        /// Initializes a new <see cref="ComplexQueryNode"/> instance.
        ///   </overrides>
        public ExpressionQueryNode(IAttribute rootAttribute, DbConnection dbConnection, bool resolveOnLoad)
            : base(rootAttribute, dbConnection, resolveOnLoad)
        {
        }

        /// <summary>
        /// Evaluates the query by combining evaluating the expression.
        /// </summary>
        /// <returns>
        /// A <see langword="string"/> representing the result of the query.
        /// </returns>
        public override QueryResult Evaluate()
        {
            StringBuilder expressionBuilder = new StringBuilder();

            try
            {
                Dictionary<string, object> variables = new Dictionary<string, object>();

                int variableNum = 0;
                foreach (QueryNode childNode in _childNodes)
                {
                    // Add any unparameterized node as part of the expression string itself.
                    if (!childNode.Parameterize)
                    {
                        expressionBuilder.Append(childNode.Evaluate());
                    }
                    // Any parameterized nodes should be be treated as variables of a specific type
                    else
                    {
                        // Create a unique name for the variable.
                        variableNum++;
                        string variableName = string.Format(CultureInfo.InvariantCulture,
                            "Variable{0}", variableNum);

                        string stringValue = childNode.Evaluate().ToString();
                        object value = null;

                        // Determine the type to which the chileNode's result should be cast at
                        // evaluation time.
                        Type type = null;
                        string typeName;
                        if (childNode.Properties.TryGetValue("Type", out typeName))
                        {
                            type = TypeResolutionUtils.ResolveType(typeName);
                        }

                        // If not specified, treat as a string.
                        if (type == null)
                        {
                            value = stringValue;
                        }
                        // Otherwise, try to cast to the specified type.
                        else
                        {
                            try
                            {
                                value = TypeConversionUtils.ConvertValueIfNecessary(type, stringValue, variableName);
                            }
                            catch
                            {
                                // If the cast failed, use a default value (if one is provided)
                                string defaultValue;
                                if (childNode.Properties.TryGetValue("Default", out defaultValue))
                                {
                                    value = TypeConversionUtils.ConvertValueIfNecessary(type, defaultValue, variableName);
                                }
                                // Otherwise, use the default value of the type.
                                else if (type.IsValueType)
                                {
                                    value = Activator.CreateInstance(type);
                                }
                            }
                        }

                        variables[variableName] = value;

                        // Plug the variable name into the expression.
                        expressionBuilder.Append("#");
                        expressionBuilder.Append(variableName);
                    }
                }

                string expression = expressionBuilder.ToString();
                string result = ExpressionEvaluator.GetValue(null, expression, variables).ToString();

                return new QueryResult(SelectionMode, result);
            }
            catch (Exception ex)
            {
                ExtractException ee = ex.AsExtract("ELI31983");
                ee.AddDebugData("Expression", expressionBuilder.ToString(), false);
                throw ee;
            }
        }
    }

    #endregion ExpressionQueryNode

    #region DataEntryQuery

    /// <summary>
    /// The master <see cref="QueryNode"/> class used to construct queries from their XML
    /// specifications.
    /// </summary>
    internal class DataEntryQuery : ComplexQueryNode, IDisposable
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
        /// Indicates what <see langword="string"/> value the query should resolve to in order for
        /// the <see cref="IAttribute"/> to be considered valid.
        /// </summary>
        string _validValue;

        /// <summary>
        /// Indicates whether this query should only indicate a warning when used for validation as
        /// opposed to data that is completely invalid.
        /// </summary>
        bool _isValidationWarning;

        /// <summary>
        /// The named result containing the validation error message that should be associated with
        /// the <see cref="IAttribute"/> when validation using this <see cref="DataEntryQuery"/>
        /// fails.
        /// <para><b>Note</b></para>
        /// If specified, this overrides the validation error message associated with the control.
        /// </summary>
        string _validationMessageResultName;

        /// <summary>
        /// Initializes a new <see cref="DataEntryQuery"/> instance.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that should be used to
        /// evaluate any SQL queries.</param>
        /// <param name="selectionMode">The <see cref="MultipleQueryResultSelectionMode"/> that
        /// should be used to determine how multiple resulting values should be handled.</param>
        /// <param name="resolveOnLoad"><see langword="true"/> if the query will resolve itself if
        /// possible as it is loaded, <see langword="false"/> if RegisterTriggerCandidate must be
        /// called manually to resolve the query if attribute triggers are involved.</param>
        private DataEntryQuery(IAttribute rootAttribute, DbConnection dbConnection,
            MultipleQueryResultSelectionMode selectionMode, bool resolveOnLoad)
            : base(rootAttribute, dbConnection, selectionMode, resolveOnLoad)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DataEntryQuery"/>.
        /// </summary>
        /// <param name="xml">The xml that specifies the query.</param>
        /// <param name="rootAttribute">The <see cref="IAttribute"/></param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that should be used to
        /// evaluate any SQL queries.</param>
        /// <param name="selectionMode">The <see cref="MultipleQueryResultSelectionMode"/> that
        /// should be used to determine how multiple resulting values should be handled.</param>
        /// <param name="resolveOnLoad"><see langword="true"/> if the query will resolve itself if
        /// possible as it is loaded, <see langword="false"/> if RegisterTriggerCandidate must be
        /// called manually to resolve the query if attribute triggers are involved.</param>
        /// <returns>A <see cref="DataEntryQuery"/> defined by the specified
        /// <see paramref="xml"/>.</returns>
        static public DataEntryQuery Create(string xml, IAttribute rootAttribute,
            DbConnection dbConnection, MultipleQueryResultSelectionMode selectionMode,
            bool resolveOnLoad)
        {
            try
            {
                DataEntryQuery[] queryList =
                    CreateList(xml, rootAttribute, dbConnection, selectionMode, resolveOnLoad);

                ExtractException.Assert("ELI28861", "Specified XML defines multiple queries!",
                     queryList.Length == 1);

                return queryList[0];
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28862", ex);
                ee.AddDebugData("Query", xml, false);
                throw ee;
            }
        }

        /// <summary>
        /// Creates a list of new <see cref="DataEntryQuery"/>.
        /// </summary>
        /// <param name="xml">The xml that specifies the queries</param>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that should be used to
        /// evaluate any SQL queries.</param>
        /// <param name="selectionMode">The <see cref="MultipleQueryResultSelectionMode"/> that
        /// should be used to determine how multiple resulting values should be handled.</param>
        /// <param name="resolveOnLoad"><see langword="true"/> if the query will resolve itself if
        /// possible as it is loaded, <see langword="false"/> if RegisterTriggerCandidate must be
        /// called manually to resolve the query if attribute triggers are involved.</param>
        /// <returns>A list of <see cref="DataEntryQuery"/> instances defined by the specified
        /// <see paramref="xml"/>.</returns>
        static public DataEntryQuery[] CreateList(string xml, IAttribute rootAttribute,
            DbConnection dbConnection, MultipleQueryResultSelectionMode selectionMode,
            bool resolveOnLoad)
        {
            try
            {
                // The resulting list of queries.
                List<DataEntryQuery> queryList = new List<DataEntryQuery>();

                // A query containing named QueryNodes accessible by all defined DataEntryQueries.
                DataEntryQuery queryNodeDeclarations = null;

                // A collection named QueryNodes accessible to subquent DataEntryQueries that are
                // loaded (includes name nodes both from the declarations node and the queries
                // themselves).
                Dictionary<string, QueryNode> namedQueries = new Dictionary<string, QueryNode>();

                // In order to avoid requiring queries to be wrapped in a query element, enclose the
                // query in a Query element if it hasn't already been.
                xml = xml.Trim();
                if (xml.IndexOf("<Query", StringComparison.OrdinalIgnoreCase) != 0 &&
                    xml.IndexOf("<Declarations", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    xml = "<Query>" + xml + "</Query>";
                }
                // Enclose all queries in a root query to ensure properly formed XML.
                xml = "<Root>" + xml + "</Root>";

                // Read the XML.
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.InnerXml = xml;
                XmlNode rootNode = xmlDocument.FirstChild;

                // Load any query node declarations that will be referenced by primary queries.
                XmlNode declarationsNode = rootNode.ChildNodes
                    .Cast<XmlNode>()
                    .Where(n => n.Name.Equals("Declarations", StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
                if (declarationsNode != null)
                {
                    queryNodeDeclarations = new DataEntryQuery(rootAttribute, dbConnection,
                        selectionMode, resolveOnLoad);
                    queryNodeDeclarations.LoadFromXml(declarationsNode, null, namedQueries);

                    // ExcludeFromResult is implied by virtue of the fact that the QueryNode is
                    // defined in the declarations node. Set ExcludeFromResult to ensure the nodes
                    // themselves can't directly be included by any that referenced it.
                    foreach (QueryNode namedQuery in namedQueries.Values)
                    {
                        namedQuery.ExcludeFromResult = true;
                    }
                }

                // Use the XML to generate all queries to be used for this trigger.
                foreach (XmlNode node in rootNode.ChildNodes
                    .OfType<XmlNode>()
                    .Where(n => n.Name.Equals("Query", StringComparison.OrdinalIgnoreCase)))
                {
                    DataEntryQuery dataEntryQuery = new DataEntryQuery(rootAttribute, dbConnection,
                        selectionMode, resolveOnLoad);
                    dataEntryQuery.LoadFromXml(node, dataEntryQuery, namedQueries);
                    queryList.Add(dataEntryQuery);
                }

                return queryList.ToArray();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28863", ex);
                ee.AddDebugData("Query", xml, false);
                throw ee;
            }
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
        /// 
        /// </summary>
        public bool UpdatePending
        {
            get
            {
                return _updatePending;
            }

            set
            {
                _updatePending = value;
            }
        }

        /// <summary>
        /// Gets whether the query is disabled.
        /// </summary>
        /// <value><see langword="true"/> if the query is disabled; <see langword="false"/>
        /// otherwise.</value>
        public bool Disabled
        {
            get
            {
                return _disabled;
            }

            set
            {
                _disabled = value;
            }
        }

        /// <summary>
        /// Gets what <see langword="string"/> value the query should resolve to in order for
        /// the <see cref="IAttribute"/> to be considered valid.
        /// </summary>
        /// <value>The <see langword="string"/> value the query should resolve to in order for
        /// the <see cref="IAttribute"/> to be considered valid. If <see langword="null"/>, the
        /// <see cref="IAttribute"/>'s value must match a string value from the result.</value>
        public string ValidValue
        {
            get
            {
                return _validValue;
            }
        }

        /// <summary>
        /// Gets whether this query should only indicate a warning when used for validation as
        /// opposed to data that is completely invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the data assosiated with this query may be wrong but
        /// is not clearly invalid, <see langword="false"/> if the data is completely invalid.
        /// </returns>
        public bool IsValidationWarning
        {
            get
            {
                return _isValidationWarning;
            }
        }

        /// <summary>
        /// Registers a trigger so that updates to the specified trigger <see cref="IAttribute"/>
        /// cause the target attribute to be updated.
        /// </summary>
        /// <param name="triggerAttribute">The <see cref="IAttribute"/> for which modifications
        /// should cause the query to be evaluated.</param>
        /// <param name="triggerOnDelete"><see langword="true"/> if the deletion of the
        /// <see paramref="triggerAttribute"/> should trigger the
        /// <see cref="TriggerAttributeModified"/> event, <see langword="false"/> if it should not.
        /// </param>
        public void SetTrigger(IAttribute triggerAttribute, bool triggerOnDelete)
        {
            if (!_triggerAttributes.Contains(triggerAttribute))
            {
                _triggerAttributes.Add(triggerAttribute);

                AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(triggerAttribute);

                statusInfo.AttributeValueModified += HandleAttributeValueModified;

                if (triggerOnDelete)
                {
                    statusInfo.AttributeDeleted += HandleAttributeDeleted;
                }
            }
        }

        /// <summary>
        /// Unregisters a trigger so that updates to the specified <see cref="IAttribute"/>
        /// no longer cause the target attribute to be updated.
        /// </summary>
        /// <param name="triggerAttribute">The <see cref="IAttribute"/> for which modifications
        /// should no longer cause the query to be evaluated.</param>
        /// <param name="triggerOnDelete"><see langword="true"/> if the deletion of the
        /// <see paramref="triggerAttribute"/> is configured to trigger the
        /// <see cref="TriggerAttributeModified"/> event, <see langword="false"/> if is should not.
        /// </param>
        public void ClearTrigger(IAttribute triggerAttribute, bool triggerOnDelete)
        {
            try
            {
                if (_triggerAttributes.Contains(triggerAttribute))
                {
                    _triggerAttributes.Remove(triggerAttribute);

                    AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(triggerAttribute);

                    statusInfo.AttributeValueModified -= HandleAttributeValueModified;

                    if (triggerOnDelete)
                    {
                        statusInfo.AttributeDeleted -= HandleAttributeDeleted;
                    }

                    // Ensure all nodes in the query update cached resolved status values to reflect
                    // the change.
                    foreach (DataEntryQuery rootQuery in _rootQueries)
                    {
                        rootQuery.UpdateResolvedStatus();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28860", ex);
            }
        }

        /// <summary>
        /// Loads the <see cref="AttributeQueryNode"/> using the specified XML query string.
        /// </summary>
        /// <param name="xmlNode">The XML query string defining the query.</param>
        /// <param name="rootQuery">The <see cref="DataEntryQuery"/> that is the root of this
        /// query.</param>
        /// <param name="namedQueries">A communal collection of named <see cref="QueryNode"/>s
        /// available to allow referencing of named nodes by subsequent nodes.</param>
        public override void LoadFromXml(XmlNode xmlNode, DataEntryQuery rootQuery,
            Dictionary<string, QueryNode> namedQueries)
        {
            try
            {
                base.LoadFromXml(xmlNode, rootQuery, namedQueries);

                // Check to see if this query has been specified as the default.
                XmlAttribute xmlAttribute = xmlNode.Attributes["Default"];
                if (xmlAttribute != null)
                {
                    DefaultQuery = xmlAttribute.Value.ToBoolean();
                }

                xmlAttribute = xmlNode.Attributes["ValidValue"];
                if (xmlAttribute != null)
                {
                    _validValue = xmlAttribute.Value;
                }

                xmlAttribute = xmlNode.Attributes["ValidationMessage"];
                if (xmlAttribute != null)
                {
                    _validationMessageResultName = xmlAttribute.Value;
                    AddNamedDependency(_validationMessageResultName, rootQuery, namedQueries);
                }

                // Not a validation warning by default
                xmlAttribute = xmlNode.Attributes["ValidationWarning"];
                if (xmlAttribute != null)
                {
                    _isValidationWarning = xmlAttribute.Value.ToBoolean();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28859", ex);
            }
        }

        /// <summary>
        /// Gets the validation error message that should be associated with the control
        /// when validation using this <see cref="DataEntryQuery"/> fails.
        /// <para><b>Note</b></para>
        /// If specified, this overrides the validation error message associated with the control.
        /// </summary>
        /// <returns>If not <see langword="null"/> the result is a validation message that should
        /// be used in place of the control's pre-defined error message.</returns>
        public string GetValidationMessage()
        {
            try
            {
                if (string.IsNullOrEmpty(_validationMessageResultName))
                {
                    return null;
                }
                else
                {
                    return GetNamedResult(_validationMessageResultName).ToString();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28965", ex);
            }
        }

        #region Events

        /// <summary>
        /// Raised when an <see cref="IAttribute"/> that is part of the query is registered or
        /// modified thereby resolving the query or changing the query result.
        /// </summary>
        public event EventHandler<AttributeValueModifiedEventArgs> TriggerAttributeModified;

        /// <summary>
        /// Raised when a registered trigger <see cref="IAttribute"/> is deleted thereby changing
        /// the query result.
        /// </summary>
        public event EventHandler<AttributeDeletedEventArgs> TriggerAttributeDeleted;

        #endregion Events

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
                OnTriggerAttributeModified(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26115", ex);
                ee.AddDebugData("Event Data", e, false);
                throw ee;
            }
        }

        /// <summary>
        /// Handles the case that a registered trigger <see cref="IAttribute"/> is deleted thereby
        /// changing the query result.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="AttributeDeletedEventArgs"/> that contains the event data.
        /// </param>
        void HandleAttributeDeleted(object sender, AttributeDeletedEventArgs e)
        {
            try
            {
                OnTriggerAttributeDeleted(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28925", ex);
                ee.AddDebugData("Event Data", e, false);
                throw ee;
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
                statusInfo.AttributeDeleted -= HandleAttributeDeleted;
            }

            _triggerAttributes.Clear();
        }

        /// <summary>
        /// Raises the <see cref="TriggerAttributeModified"/> event.
        /// </summary>
        /// <param name="e">An <see cref="AttributeValueModifiedEventArgs"/> that contains the event
        /// data.</param>
        void OnTriggerAttributeModified(AttributeValueModifiedEventArgs e)
        {
            if (this.TriggerAttributeModified != null)
            {
                TriggerAttributeModified(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="TriggerAttributeDeleted"/> event.
        /// </summary>
        /// <param name="e">An <see cref="AttributeDeletedEventArgs"/> that contains the event
        /// data.</param>
        void OnTriggerAttributeDeleted(AttributeDeletedEventArgs e)
        {
            if (this.TriggerAttributeDeleted != null)
            {
                TriggerAttributeDeleted(this, e);
            }
        }

        #endregion Private Members
    }

    #endregion DataEntryQuery
}
