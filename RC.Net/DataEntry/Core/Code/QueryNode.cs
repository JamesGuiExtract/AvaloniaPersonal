using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Extract.DataEntry
{
    #region Enums

    /// <summary>
    /// Specifies the way in which spatial information will be persisted from
    /// <see cref="QueryNode"/>s.
    /// </summary>
    [Flags]
    // For backward compatibility, I'm not changing the name of SpatialMode after making it flags
    [SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
    // Since "None" has a special meaning it should not be used as the name of the zero value.
    // "None" has an active effect (to remove spatial info). "Normal" as the zero value indicates
    // no special action should be taken on the result's spatial info.
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum SpatialMode
    {
        /// <summary>
        /// If a node's result is spatial, the spatial info will be returned.  Spatial info
        /// will be persisted as the result is combined with other sibling results (spatial or not),
        /// but will not be persisted through parent nodes with text return types (SQL, Regex,
        /// Expression for instance).
        /// </summary>
        Normal = 0,

        /// <summary>
        /// If a node's result is spatial, the spatial info will be returned. The spatial info
        /// will override the spatial results of the parent node or be added to any non-spatial
        /// result of the parent node.
        /// </summary>
        Force = 1,

        /// <summary>
        /// Only the spatial information from the node should be persisted, not the node text.
        /// </summary>
        Only = 2,

        /// <summary>
        /// Spatial info should not be included if the nodes text result is blank.
        /// </summary>
        IfNotBlank = 4,

        /// <summary>
        /// No spatial information should be persisted from the node.
        /// </summary>
        None = 8
    }

    /// <summary>
    /// Specifies fields of the bounds of a spatial result that can be returned as text.
    /// </summary>
    public enum SpatialField
    {
        /// <summary>
        /// The page number (first page, if it spans multiple).
        /// </summary>
        Page,

        /// <summary>
        /// The X coordinate of the left side.
        /// </summary>
        Left,

        /// <summary>
        /// The Y coordinate of the top side.
        /// </summary>
        Top,

        /// <summary>
        /// The X coordinate of the right side.
        /// </summary>
        Right,

        /// <summary>
        /// The Y coordinate of the bottom side.
        /// </summary>
        Bottom,

        /// <summary>
        /// The X coordinate of the start point.
        /// </summary>
        StartX,

        /// <summary>
        /// The Y coordinate of the start point.
        /// </summary>
        StartY,

        /// <summary>
        /// The X coordinate of the end point.
        /// </summary>
        EndX,

        /// <summary>
        /// The Y coordinate of the end point.
        /// </summary>
        EndY,

        /// <summary>
        /// The height of the zone.
        /// </summary>
        Height
    }

    /// <summary>
    /// Specifies non-value fields of an attribute to can be returned as text.
    /// </summary>
    public enum AttributeField
    {
        /// <summary>
        /// The attribute name
        /// </summary>
        Name,

        /// <summary>
        /// The attribute type. If the attribute has multiple types (delimited by a +), the
        /// each type will be returned as a separate element in a list.
        /// </summary>
        Type
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

    #region NamedQueryReferences

    /// <summary>
    /// Used to keep track of which <see cref="QueryNode"/>s are referencing a specific query node.
    /// </summary>
    internal class NamedQueryReferences
    {
        /// <summary>
        /// The <see cref="QueryNode"/> being (or to be) referenced.
        /// </summary>
        public QueryNode ReferencedQuery;

        /// <summary>
        /// The set of <see cref="QueryNode"/> currently referencing <see cref="ReferencedQuery"/>.
        /// </summary>
        public HashSet<CompositeQueryNode> ReferencingQueries = new HashSet<CompositeQueryNode>();
    }

    #endregion NamedQueryReferences

    /// <summary>
    /// Describes an element of an <see cref="DataEntryQuery"/>.
    /// </summary>
    public abstract class QueryNode
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(QueryNode).ToString();

        #endregion Constants

        /// <summary>
        /// The unparsed XML text that defines the query.
        /// </summary>
        string _queryText;

        /// <summary>
        /// If specified, indicates the name by which this result can be looked up a later time.
        /// </summary>
        string _name;

        /// <summary>
        /// Specifies whether the query node should be parameterized if it is used
        /// in an <see cref="SqlQueryNode"/> or <see cref="ExpressionQueryNode"/>.
        /// </summary>
        bool _parameterize = true;

        /// <summary>
        /// Indicates whether the result from evaluating query node may be cached for performance
        /// reasons if it is a node type that supports caching.
        /// </summary>
        bool _allowCaching = true;

        /// <summary>
        /// Indicates whether to flush the current node type's cache when this node is evaluated.
        /// </summary>
        bool _flushCache = false;

        /// <summary>
        /// Specifies the way in which spatial information will be persisted from this
        /// <see cref="QueryNode"/>.
        /// </summary>
        SpatialMode _spatialMode = SpatialMode.Normal;

        /// <summary>
        /// Specifies a field of the bounds of a spatial result that can be returned as text.
        /// </summary>
        SpatialField? _spatialField;

        /// <summary>
        /// Specifies a non-value field of an attribute to that can be returned as text.
        /// </summary>
        AttributeField? _attributeField;

        /// <summary>
        /// The <see cref="MultipleQueryResultSelectionMode"/> that should be used to determine how
        /// multiple resulting values should be handled.
        /// </summary>
        MultipleQueryResultSelectionMode _selectionMode = MultipleQueryResultSelectionMode.List;

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
        /// Specifies whether the parent node should abort processing if the result of this node is
        /// empty.
        /// </summary>
        bool _abortIfEmpty;

        /// <summary>
        /// The properties assigned to the query node via <see cref="XmlAttribute"/>s (name/value
        /// pairs).
        /// </summary>
        Dictionary<string, string> _properties = new Dictionary<string, string>();

        /// <summary>
        /// The distinct <see cref="QueryResult"/> for a node with the selection mode of "Distinct".
        /// The value on the top of the stack is active for the current evaluation scope. The other
        /// values are for higher scopes.
        /// </summary>
        Stack<QueryResult> _distinctResults = new Stack<QueryResult>();

        /// <summary>
        /// Initializes a new <see cref="QueryNode"/> instance.
        /// </summary>
        protected QueryNode()
        {
            try
            {
                // Validate the license
                // Since DataEntryQueries are not being used outside the DE framework (in rule
                // objects) and really should be abstracted out of Extract.DataEntry at some point,
                // don't license with a data entry license.
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.FlexIndexIDShieldCoreObjects, "ELI34721", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34722");
            }
        }

        /// <summary>
        /// Gets the unparsed XML text that defines the query.
        /// </summary>
        public string QueryText
        {
            get
            {
                return _queryText;
            }

            protected set
            {
                _queryText = value;
            }
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
        /// Gets or sets whether the query node should be parameterized if it is used
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
        /// Gets or sets whether the result from evaluating query node may be cached for performance
        /// reasons if it is a node type that supports caching. In some situations it may be
        /// desirable to force re-evaluation of query, such as for an SQL query node that is
        /// updating a database.
        /// <b><para>Note</para></b>
        /// A value of <see langwor="true"/> does not guarantee that a result will be cached, only
        /// that it is eligible to be cached.
        /// </summary>
        /// <value><see langword="true"/> if the query node's result is eligible to be cached for
        /// performance; <see langword="false"/> to force re-evaluation every time.</value>
        public bool AllowCaching
        {
            get
            {
                return _allowCaching;
            }

            set
            {
                _allowCaching = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to flush the current node type's cache when this
        /// node is evaluated.
        /// </summary>
        /// <value><see langword="true"/> if to flush the current node type's cache when this;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool FlushCache
        {
            get
            {
                return _flushCache;
            }

            set
            {
                _flushCache = value;
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
        /// Gets or sets a <see cref="SpatialField"/> of the bounds of a spatial result to return
        /// as text.
        /// </summary>
        /// <value>
        /// The <see cref="SpatialField"/> of the bounds of a spatial result to return as text.
        /// </value>
        public SpatialField? SpatialField
        {
            get
            {
                return _spatialField;
            }

            set
            {
                _spatialField = value;
            }
        }

        /// <summary>
        /// Gets or sets a non-value fields of an attribute to return as text.
        /// </summary>
        /// <value>
        /// The non-value fields of an attribute to return as text.
        /// </value>
        public AttributeField? AttributeField
        {
            get
            {
                return _attributeField;
            }

            set
            {
                _attributeField = value;
            }
        }

        /// <summary>
        /// Gets or set the name by which this result can be looked up a later time.
        /// Can be <see langword="null"/>, in which case the results will not be accessible
        /// independently from the overall query result.
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
        /// string value where each value is separated by the specified delimiter. The delimiter
        /// can be blank to force a query result to be flattened into a string value.
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
        /// Gets or sets a value indicating whether the parent node should abort processing if the
        /// result of this node is empty.
        /// </summary>
        /// <value>If <see langword="true"/> and the result of this node is empty, the parent node
        /// should return an empty result as well rather than attempting to evaluate; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool AbortIfEmpty
        {
            get
            {
                return _abortIfEmpty;
            }

            set
            {
                _abortIfEmpty = value;
            }
        }

        /// <summary>
        /// Indicates whether new line characters in literal text should be treated as delimiters
        /// between returned results or as literal whitespace.
        /// </summary>
        /// <value><see langword="true"/> to treat new line characters as literal whitespace;
        /// <see langword="false"/> to treat new line characters are delimiters between returned
        /// results.</value>
        public bool TreatNewLinesAsWhiteSpace
        {
            get;
            set;
        }

        /// <summary>
        /// Evaluates the query.
        /// </summary>
        /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
        public abstract QueryResult Evaluate();

        /// <summary>
        /// Informs the node that it's parent has completed evaluation. This may be needed to keep
        /// track of all the different results of <see cref="Evaluate"/> that have occurred in the
        /// course of evaluation the parent. These evaluations may have produced different results
        /// based on a sibling node with a Distinct selection mode.
        /// </summary>
        internal virtual void NotifyParentEvaluationComplete()
        {
        }

        /// <summary>
        /// The active distinct <see cref="QueryResult"/> for a node with the selection mode of 
        /// "Distinct" that is currently being evaluated.
        /// </summary>
        internal virtual QueryResult DistinctResult
        {
            get
            {
                try
                {
                    if (_distinctResults.Count == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return _distinctResults.Peek();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34677");
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of results any given SQL node type should be able to
        /// cache.
        /// </summary>
        public static int QueryCacheLimit
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to split each resulting row as a CSV
        /// </summary>
        public bool SplitCsv
        {
            get;
            set;
        }

        /// <summary>
        /// Pushes the <see paramref="distinctResult"/> to use for the current evaluation scope.
        /// </summary>
        /// <param name="distinctResult">The <see cref="QueryResult"/> to use as the distinct value
        /// for the current execution scope.</param>
        /// <returns><see langword="true"/> if the query result was modified as a result; otherwise,
        /// <see langword="false"/>.</returns>
        internal virtual bool PushDistinctResult(QueryResult distinctResult)
        {
            try
            {
                bool valueChanged = (_distinctResults.Count == 0)
                    ? (distinctResult != null)
                    : (distinctResult != _distinctResults.Peek());

                _distinctResults.Push(distinctResult);

                return valueChanged;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34678");
            }
        }

        /// <summary>
        /// Pops the distinct <see cref="QueryResult"/> that had been active for the evaluation
        /// scope that is ending.
        /// </summary>
        /// <returns><see langword="true"/> if the query result was modified as a result; otherwise,
        /// <see langword="false"/>.</returns>
        internal virtual bool PopDistinctResult()
        {
            try
            {
                QueryResult distinctResult = _distinctResults.Pop();

                bool valueChanged = (_distinctResults.Count == 0)
                    ? (distinctResult != null)
                    : (distinctResult != _distinctResults.Peek());

                return valueChanged;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34679");
            }
        }

        /// <summary>
        /// Loads the <see cref="QueryNode"/> using the specified XML query string.
        /// </summary>
        /// <param name="xmlNode">The XML query string defining the query.</param>
        /// <param name="namedReferences">A communal collection of named
        /// <see cref="NamedQueryReferences"/>s available to allow referencing of named nodes.</param>
        internal virtual void LoadFromXml(XmlNode xmlNode,
            Dictionary<string, NamedQueryReferences> namedReferences)
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

                    NamedQueryReferences namedReference = GetNamedReferences(Name, namedReferences);
                    ExtractException.Assert("ELI34494",
                        "Multiple nodes exist with the name \"" + Name + "\"",
                        namedReference.ReferencedQuery == null);
                    namedReference.ReferencedQuery = this;
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

                if (_properties.TryGetValue("SpatialMode", out xmlAttributeValue))
                {
                    var specifiedFlags = new HashSet<string>(
                        xmlAttributeValue.Split(new [] {'|', ','}), StringComparer.OrdinalIgnoreCase);

                    if (specifiedFlags.Contains("Force"))
                    {
                        SpatialMode |= SpatialMode.Force;
                    }
                    if (specifiedFlags.Contains("Only"))
                    {
                        SpatialMode |= SpatialMode.Only;
                    }
                    if (specifiedFlags.Contains("IfNotBlank"))
                    {
                        SpatialMode |= SpatialMode.IfNotBlank;
                    }
                    if (specifiedFlags.Contains("None"))
                    {
                        ExtractException.Assert("ELI35362",
                            "'Force' spatial mode may not be combined with any other spatial modes.",
                            SpatialMode == SpatialMode.Normal);

                        SpatialMode = SpatialMode.Force;
                    }
                }

                // Convert a spatial parameter to text if specified.
                if (_properties.TryGetValue("SpatialField", out xmlAttributeValue))
                {
                    _spatialField = (SpatialField)TypeDescriptor.GetConverter(typeof(SpatialField))
                        .ConvertFromString(xmlAttributeValue);
                }

                // Convert an attribute parameter to text if specified.
                if (_properties.TryGetValue("AttributeField", out xmlAttributeValue))
                {
                    ExtractException.Assert("ELI35251", "SpatialField and AttributeField attributes " +
                        "cannot be used together in the same node.", !_spatialField.HasValue);

                    _attributeField = (AttributeField)TypeDescriptor.GetConverter(typeof(AttributeField))
                        .ConvertFromString(xmlAttributeValue);
                }

                // Parameterize unless the parameterize attribute is present and specifies not to.
                if (_properties.TryGetValue("Parameterize", out xmlAttributeValue))
                {
                    Parameterize = xmlAttributeValue.ToBoolean();
                }
                else
                {
                    // If SpatialMode only is being used, the node should default to not being
                    // parameterized since more than likely the text value is intended not to factor
                    // into a query or expression at all.
                    Parameterize = !SpatialMode.HasFlag(SpatialMode.Only);
                }

                // Allow caching unless the AllowCaching attribute is present and specifies not to.
                if (_properties.TryGetValue("AllowCaching", out xmlAttributeValue))
                {
                    AllowCaching = xmlAttributeValue.ToBoolean();
                }

                // Flush the cache when evaluating if the FlushCache attribute is set.
                if (_properties.TryGetValue("FlushCache", out xmlAttributeValue))
                {
                    FlushCache = xmlAttributeValue.ToBoolean();
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

                // If set and the result of this node is empty, the parent should return empty
                // without attempting evaluation.
                if (_properties.TryGetValue("AbortIfEmpty", out xmlAttributeValue))
                {
                    AbortIfEmpty = xmlAttributeValue.ToBoolean();
                }

                if (_properties.TryGetValue("SplitCsv", out xmlAttributeValue))
                {
                    SplitCsv = xmlAttributeValue.ToBoolean();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28934", ex);
            }
        }

        /// <summary>
        /// Retrieves a <see cref="NamedQueryReferences"/> instance from
        /// <see paramref="namedReferences"/> for the specified <see paramref="resultName"/>.
        /// If one does not already exist it will be created.
        /// </summary>
        /// <param name="resultName">The name of the result in question.</param>
        /// <param name="namedReferences">A dictionary containing the set of
        /// <see cref="NamedQueryReferences"/> to work from.</param>
        /// <returns>The <see cref="NamedQueryReferences"/> instance.</returns>
        internal static NamedQueryReferences GetNamedReferences(string resultName,
            Dictionary<string, NamedQueryReferences> namedReferences)
        {
            try
            {
                NamedQueryReferences namedReference;
                if (!namedReferences.TryGetValue(resultName, out namedReference))
                {
                    namedReference = new NamedQueryReferences();
                    namedReferences[resultName] = namedReference;
                }

                return namedReference;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34496");
            }
        }
    }
}
