using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    /// <summary>
    /// The master <see cref="QueryNode"/> class used to construct queries from their XML
    /// specifications.
    /// </summary>
    public class DataEntryQuery : CompositeQueryNode
    {
        #region Constants


        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates whether this query is a default query.
        /// </summary>
        bool _defaultQuery;

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

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataEntryQuery"/> instance.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that should be used to
        /// evaluate any SQL queries.</param>
        private DataEntryQuery(IAttribute rootAttribute, DbConnection dbConnection)
            : base(rootAttribute, dbConnection)
        {

        }

        #endregion Constructors

        #region Static Members

        /// <summary>
        /// Creates a new <see cref="DataEntryQuery"/>.
        /// </summary>
        /// <param name="xml">The xml that specifies the query.</param>
        /// <param name="rootAttribute">The <see cref="IAttribute"/></param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that should be used to
        /// evaluate any SQL queries.</param>
        /// <returns>A <see cref="DataEntryQuery"/> defined by the specified
        /// <see paramref="xml"/>.</returns>
        static public DataEntryQuery Create(string xml, IAttribute rootAttribute,
            DbConnection dbConnection)
        {
            try
            {
                DataEntryQuery[] queryList =
                    CreateList(xml, rootAttribute, dbConnection,
                    MultipleQueryResultSelectionMode.List);

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
        /// <returns>A list of <see cref="DataEntryQuery"/> instances defined by the specified
        /// <see paramref="xml"/>.</returns>
        static public DataEntryQuery[] CreateList(string xml, IAttribute rootAttribute,
            DbConnection dbConnection, MultipleQueryResultSelectionMode selectionMode)
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
                Dictionary<string, NamedQueryReferences> namedReferences =
                    new Dictionary<string, NamedQueryReferences>();

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
                    queryNodeDeclarations = new DataEntryQuery(rootAttribute, dbConnection);
                    queryNodeDeclarations.LoadFromXml(declarationsNode, namedReferences);
                    RegisterNamedDependencies(namedReferences);

                    // ExcludeFromResult is implied by virtue of the fact that the QueryNode is
                    // defined in the declarations node. Set ExcludeFromResult to ensure the nodes
                    // themselves can't directly be included by any that referenced it.
                    foreach (QueryNode namedQuery in
                        namedReferences.Values.Select(reference => reference.ReferencedQuery))
                    {
                        namedQuery.ExcludeFromResult = true;
                    }
                }

                // Use the XML to generate all queries to be used for this trigger.
                foreach (XmlNode node in rootNode.ChildNodes
                    .OfType<XmlNode>()
                    .Where(n => n.Name.Equals("Query", StringComparison.OrdinalIgnoreCase)))
                {
                    DataEntryQuery dataEntryQuery = new DataEntryQuery(rootAttribute, dbConnection);
                    dataEntryQuery.SelectionMode = selectionMode;
                    dataEntryQuery.LoadFromXml(node, namedReferences);
                    queryList.Add(dataEntryQuery);
                }

                RegisterNamedDependencies(namedReferences);

                return queryList.ToArray();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28863", ex);
                ee.AddDebugData("Query", xml, false);
                throw ee;
            }
        }

        #endregion Static Members

        #region Properties

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

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the validation error message that should be associated with the control
        /// when validation using this <see cref="DataEntryQuery"/> fails.
        /// <para><b>Note</b></para>
        /// If specified, this overrides the validation error message associated with the control.
        /// </summary>
        /// <returns>If not <see langword="null"/> the result is a validation message that should
        /// be used in place of the control's pre-defined error message.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
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

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Loads the <see cref="AttributeQueryNode"/> using the specified XML query string.
        /// </summary>
        /// <param name="xmlNode">The XML query string defining the query.</param>
        /// <param name="namedReferences">A communal collection of named <see cref="QueryNode"/>s
        /// available to allow referencing of named nodes.</param>
        internal override void LoadFromXml(XmlNode xmlNode,
            Dictionary<string, NamedQueryReferences> namedReferences)
        {
            try
            {
                base.LoadFromXml(xmlNode, namedReferences);

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

                    NamedQueryReferences namedReference =
                        GetNamedReferences(_validationMessageResultName, namedReferences);
                    namedReference.ReferencingQueries.Add(this);
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

        #endregion Overrides
    }
}