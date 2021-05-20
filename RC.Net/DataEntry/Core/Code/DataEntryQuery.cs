using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Xml;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Represents the context in which a <see cref="DataEntryQuery"/> is being executed.
    /// Note: It is valid to combine "When" contexts with "On" contexts to defined a more narrow
    /// context, however it is not valid to combine the two "When" contexts or multiple "On" contexts.
    /// https://extract.atlassian.net/browse/ISSUE-15342
    /// </summary>
    [Flags]
    [SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
    public enum ExecutionContext
    {
        /// <summary>
        /// No context specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// When the target attribute has data
        /// </summary>
        WhenPopulated = 1,

        /// <summary>
        ///  When the target attribute is empty
        /// </summary>
        WhenEmpty = 2,

        /// <summary>
        /// As part of the initial data load.
        /// </summary>
        OnLoad = 4,

        /// <summary>
        /// Via use of a data refresh button.
        /// </summary>
        OnRefresh = 8,

        /// <summary>
        /// Via the manual creation of a new attribute (added data table row, pasted data table row or swipe)
        /// </summary>
        OnCreate = 16,

        /// <summary>
        /// In response to data manually modified in the DEP.
        /// </summary>
        OnUpdate = 32
    }

    /// <summary>
    /// The master <see cref="QueryNode"/> class used to construct queries from their XML
    /// specifications.
    /// </summary>
    public class DataEntryQuery : CompositeQueryNode
    {
        #region Fields

        /// <summary>
        /// When <see cref="AttributeStatusInfo.PerformanceTesting"/> is <see langword="true"/> keeps track
        /// of the top 25 most expensive queries will be output as debug values where the
        /// expensiveness is the initial query execution time multiplied by the number of executions.
        /// This resulting "score" will be shown just before the query itself in the exception debug.
        /// </summary>
        static DataCache<string, CachedQueryData<string[]>> _performanceCache =
            new DataCache<string, CachedQueryData<string[]>>(
                26, CachedQueryData<string[]>.GetScore);

        /// <summary>
        /// If declaration nodes aren't tracked, unreferenced declaration nodes (and the handle used
        /// for QueryCacheCleared) will leak. This allows the declarations to be deterministically
        /// disposed.
        /// </summary>
        static ThreadLocal<List<DataEntryQuery>> _declarationNodes =
            new ThreadLocal<List<DataEntryQuery>>(() => new List<DataEntryQuery>());

        /// <summary>
        /// Indicates whether this query is a default query.
        /// </summary>
        bool _defaultQuery;

        /// <summary>
        /// Defined contexts in which execution of this query should be exempted.
        /// https://extract.atlassian.net/browse/ISSUE-15342
        /// </summary>
        List<ExecutionContext> _executionExemptions = new List<ExecutionContext>();

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
        /// Specifies the property that will be updated by this query when used as a AutoUpdateQuery.
        /// By default the target property is the property that displays the attribute's value such
        /// as "Text" or "Value". Can be a nested property such as "OwningColumn.Width".
        /// <para><b>Note</b></para>
        /// It is not supported to set the <see cref="DefaultQuery"/> attribute to true on a query
        /// for which <see cref="TargetProperty"/> has been specified.
        /// </summary>
        string _targetProperty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataEntryQuery"/> instance.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnections">The <see cref="DbConnection"/>(s) that should be used to
        /// evaluate any SQL queries; The key is the connection name (blank for default
        /// connection).</param>
        private DataEntryQuery(IAttribute rootAttribute,
            Dictionary<string, DbConnection> dbConnections)
            : base(rootAttribute, dbConnections)
        {

        }

        #endregion Constructors

        #region Static Members

        /// <overloads>
        /// Creates a new <see cref="DataEntryQuery"/>.
        /// </overloads>
        /// <summary>
        /// Creates a new <see cref="DataEntryQuery"/>.
        /// </summary>
        /// <param name="xml">The xml that specifies the query.</param>
        /// <returns>A <see cref="DataEntryQuery"/> defined by the specified
        /// <see paramref="xml"/>.</returns>
        static public DataEntryQuery Create(string xml)
        {
            return Create(xml, null, (DbConnection)null);
        }

        /// <summary>
        /// Creates a new <see cref="DataEntryQuery"/>.
        /// </summary>
        /// <param name="xml">The xml that specifies the query.</param>
        /// <param name="rootAttribute">The <see cref="IAttribute"/></param>
        /// <returns>A <see cref="DataEntryQuery"/> defined by the specified
        /// <see paramref="xml"/>.</returns>
        static public DataEntryQuery Create(string xml, IAttribute rootAttribute)
        {
            return Create(xml, rootAttribute, (DbConnection)null);
        }

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
                var dbConnections = new Dictionary<string, DbConnection>();
                dbConnections[""] = dbConnection;

                return Create(xml, rootAttribute, dbConnections);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37784");
            }
        }

        /// <summary>
        /// Creates a new <see cref="DataEntryQuery"/>.
        /// </summary>
        /// <param name="xml">The xml that specifies the query.</param>
        /// <param name="rootAttribute">The <see cref="IAttribute"/></param>
        /// <param name="dbConnections">The <see cref="DbConnection"/>(s) that should be used to
        /// evaluate any SQL queries; The key is the connection name (blank for default connection).
        /// </param>
        /// <returns>A <see cref="DataEntryQuery"/> defined by the specified
        /// <see paramref="xml"/>.</returns>
        static public DataEntryQuery Create(string xml, IAttribute rootAttribute,
            Dictionary<string, DbConnection> dbConnections)
        {
            try
            {
                DataEntryQuery[] queryList =
                    CreateList(xml, rootAttribute, dbConnections,
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
        /// <param name="dbConnections">The <see cref="DbConnection"/>(s) that should be used to
        /// evaluate any SQL queries; The key is the connection name (blank for default connection).
        /// </param>
        /// <param name="selectionMode">The <see cref="MultipleQueryResultSelectionMode"/> that
        /// should be used to determine how multiple resulting values should be handled.</param>
        /// <returns>A list of <see cref="DataEntryQuery"/> instances defined by the specified
        /// <see paramref="xml"/>.</returns>
        static public DataEntryQuery[] CreateList(string xml, IAttribute rootAttribute,
            Dictionary<string, DbConnection> dbConnections,
            MultipleQueryResultSelectionMode selectionMode)
        {
            try
            {
                // The resulting list of queries.
                List<DataEntryQuery> queryList = new List<DataEntryQuery>();

                // A query containing named QueryNodes accessible by all defined DataEntryQueries.
                DataEntryQuery queryNodeDeclarations = null;

                // A collection named QueryNodes accessible to subsequent DataEntryQueries that are
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
                    queryNodeDeclarations = new DataEntryQuery(rootAttribute, dbConnections);
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

                    _declarationNodes.Value.Add(queryNodeDeclarations);
                }

                // Use the XML to generate all queries to be used for this trigger.
                foreach (XmlNode node in rootNode.ChildNodes
                    .OfType<XmlNode>()
                    .Where(n => n.Name.Equals("Query", StringComparison.OrdinalIgnoreCase)))
                {
                    DataEntryQuery dataEntryQuery = new DataEntryQuery(rootAttribute, dbConnections);
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
        /// Gets defined contexts in which execution of this query should be exempted.
        /// https://extract.atlassian.net/browse/ISSUE-15342
        /// </summary>
        public IEnumerable<ExecutionContext> ExecutionExemptions
        {
            get
            {
                return _executionExemptions;
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
        /// <returns><see langword="true"/> if the data associated with this query may be wrong but
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
        /// Gets the property that will be updated by this query when used as a AutoUpdateQuery. By
        /// default the target property is the property that displays the attribute's value such as
        /// "Text" or "Value". Can be a nested property such as "OwningColumn.Width".
        /// <para><b>Note</b></para>
        /// It is not supported to set the <see cref="DefaultQuery"/> attribute to true on a query
        /// for which <see cref="TargetProperty"/> has been specified.
        /// </summary>
        /// <returns>The property that will be updated by this query when used as a AutoUpdateQuery.
        /// </returns>
        public string TargetProperty
        {
            get
            {
                return _targetProperty;
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

        /// <summary>
        /// Populates as debug info into <see paramref="ee"/> the most expensive queries that have
        /// been executed.
        /// </summary>
        /// <param name="ee">The <see cref="ExtractException"/> in which the query performance data
        /// should be reported.</param>
        static public void ReportPerformanceData(ExtractException ee)
        {
            try
            {
                foreach (string cachedQuery in _performanceCache.ReportCachedData())
                {
                    ee.AddDebugData("Query", cachedQuery, false);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39203");
            }
        }

        /// <summary>
        /// Disposes of all declarations being used in this thread.
        /// </summary>
        static public void DisposeDeclarations()
        {
            try
            {
                CollectionMethods.ClearAndDispose(_declarationNodes.Value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40275");
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

                xmlAttribute = xmlNode.Attributes["ExecutionExemptions"];
                if (xmlAttribute != null)
                {
                    
                    var exemptions = xmlAttribute.Value.Split(',', '|');
                    foreach (var exemption in exemptions)
                    {
                        // Convert operators used for logical reading into bitwise operations.
                        var parsedExemptions = exemption
                            .Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => e.Trim());

                        ExecutionContext exemptionFlags = ExecutionContext.None;
                        foreach (var parsedExemption in parsedExemptions)
                        {
                            exemptionFlags |= (ExecutionContext)Enum.Parse(typeof(ExecutionContext), parsedExemption);
                        }

                        _executionExemptions.Add(exemptionFlags);
                    }
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

                // By default auto-update queries update the Text or value property of a control
                // (depending on control type). Make sure that if the default property (Text/Value)
                // is manually targeted, it behaves the same as if it were not specified.
                xmlAttribute = xmlNode.Attributes["TargetProperty"];
                if (xmlAttribute != null &&
                    !string.Equals(xmlAttribute.Value, "Text", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(xmlAttribute.Value, "Value", StringComparison.OrdinalIgnoreCase))
                {
                    ExtractException.Assert("ELI37287", "The Default and TargetPorperty attributes " +
                        "may not be used simultaneously.", !_defaultQuery);
                    _targetProperty = xmlAttribute.Value;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28859", ex);
            }
        }

        /// <summary>
        /// Evaluates the query by combining all child <see cref="QueryNode"/>s.
        /// </summary>
        /// <returns>
        /// A <see cref="QueryResult"/> representing the result of the query.
        /// </returns>
        public override QueryResult Evaluate()
        {
            try
            {
                DateTime startTime = DateTime.Now;

                QueryResult result = base.Evaluate();

                if (AttributeStatusInfo.PerformanceTesting)
                {
                    double executionTime = (DateTime.Now - startTime).TotalMilliseconds;
                    CachedQueryData<string[]> cachedResults;
                    if (!_performanceCache.TryGetData(QueryText, out cachedResults))
                    {
                        cachedResults = new CachedQueryData<string[]>(new string[0], executionTime);
                        _performanceCache.CacheData(QueryText, cachedResults);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39201");
            }
        }

        #endregion Overrides
    }
}