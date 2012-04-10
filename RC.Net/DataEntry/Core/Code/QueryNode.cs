using System;
using System.Collections.Generic;
using System.Xml;

namespace Extract.DataEntry
{
    #region Enums

    /// <summary>
    /// Specifies the way in which spatial information will be persisted from
    /// <see cref="QueryNode"/>s.
    /// </summary>
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
        /// If a node's result is spatial, the spatial info will be returned.  The spatial info
        /// will override the spatial results of the parent node or be added to any non-spatial
        /// result of the parent node.
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
    /// Extension methods used by the query framework
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
        /// Specifies the way in which spatial information will be persisted from this
        /// <see cref="QueryNode"/>.
        /// </summary>
        SpatialMode _spatialMode = SpatialMode.Normal;

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
        /// The properties assigned to the query node via <see cref="XmlAttribute"/>s (name/value
        /// pairs).
        /// </summary>
        private Dictionary<string, string> _properties = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new <see cref="QueryNode"/> instance.
        /// </summary>
        protected QueryNode()
        {
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
        /// The active distinct <see cref="QueryResult"/> for a node with the selection mode of 
        /// "Distinct" that is currently being evaluated.
        /// </summary>
        internal virtual QueryResult DistinctResult
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
        /// Retrieves a <see cref="NamedQueryReferences"/> instance from
        /// <see paramref="namedReferences"/> for the specified <see paramref="resultName"/>.
        /// If one does not already exisit it will be created.
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
