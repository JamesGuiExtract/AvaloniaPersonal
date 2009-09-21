using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Globalization;
using System.Text;
using System.Xml;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Registers an <see cref="IAttribute"/> for auto-updates using trigger attributes specified
    /// in the provided auto update query.
    /// </summary>
    internal partial class AutoUpdateTrigger : IDisposable
    {
        #region Fields

        /// <summary>
        /// The <see cref="IAttribute"/> to be updated using the auto-update query.
        /// </summary>
        private IAttribute _targetAttribute;

        /// <summary>
        /// The query divided into terms.
        /// </summary>
        private List<RootQueryNode> _queries = new List<RootQueryNode>();

        /// <summary>
        /// An auto-update query to be applied if the target attribute otherwise does not have any
        /// value.
        /// </summary>
        private RootQueryNode _defaultQuery;

        /// <summary>
        /// A database for auto-update queries.
        /// </summary>
        private DbConnection _dbConnection;

        /// <summary>
        /// The path of the target attribute.
        /// </summary>
        private string _rootPath;

        /// <summary>
        /// Indicates whether the trigger is to be used to update a validation list instead of the
        /// value itself.
        /// </summary>
        private bool _validationTrigger;

        #endregion Fields

        #region Contructors

        /// <summary>
        /// Initializes a new <see cref="AutoUpdateTrigger"/> instance.
        /// </summary>
        /// <param name="targetAttribute">The <see cref="IAttribute"/> to be updated using the query.
        /// </param>
        /// <param name="query">The query to use to update the query. Values
        /// to be used from other <see cref="IAttribute"/>'s values should be inserted into the
        /// query using curly braces. For example, to have the value reflect the value of a
        /// sibling attribute named "Source", the query would be specified as "{../Source}".
        /// If the query matches SQL syntax it will be executed against the
        /// <see cref="DataEntryControlHost"/>'s database. Every time an attribute specified in the
        /// query is modified, this query will be re-evaluated and used to update the value.</param>
        /// <param name="dbConnection">The compact SQL database to use for auto-update queries that
        /// use a database query (can be <see langword="null"/> if not required).</param>
        /// <param name="validationTrigger"><see langword="true"/> if the trigger should update the
        /// validation list associated with the <see paramref="targetAttribute"/> instead of the
        /// <see cref="IAttribute"/> value itself; <see langword="false"/> otherwise.</param>
        public AutoUpdateTrigger(IAttribute targetAttribute, string query,
            DbConnection dbConnection, bool validationTrigger)
        {
            try
            {
                ExtractException.Assert("ELI26111", "Null argument exception!", 
                    targetAttribute != null);
                ExtractException.Assert("ELI26112", "Null argument exception!", query != null);

                // Initialize the fields.
                _targetAttribute = targetAttribute;
                _dbConnection = dbConnection;
                _validationTrigger = validationTrigger;
                _rootPath = AttributeStatusInfo.GetFullPath(targetAttribute);

                // In order to prevent requiring queries from having to be wrapped in a query
                // element, enclose the query in a Query element if it hasn't already been.
                query = query.Trim();
                if (query.IndexOf("<Query", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    query = "<Query>" + query + "</Query>";
                }
                // Enclose all queries in a root query to ensure properly formed XML.
                query = "<Root>" + query + "</Root>";

                // Read the XML.
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.InnerXml = query;
                XmlNode rootNode = xmlDocument.FirstChild;

                // Use the XML to generate all queries to be used for this trigger.
                foreach (XmlNode node in rootNode.ChildNodes)
                {
                    RootQueryNode rootQuery = new RootQueryNode();

                    // Check to see if this query has been specified as the default.
                    XmlAttribute defaultAttribute = node.Attributes["Default"];
                    if (defaultAttribute != null && defaultAttribute.Value == "1")
                    {
                        ExtractException.Assert("ELI26734",
                            "Validation queries cannot have a default query!", !_validationTrigger);
                        ExtractException.Assert("ELI26763",
                            "Only one default query can be specified!", _defaultQuery == null);

                        rootQuery.DefaultQuery = true;
                        _defaultQuery = rootQuery;
                    }
                    // Otherwise add it into the general _queries list.
                    else
                    {
                        ExtractException.Assert("ELI26752",
                            "Validation queries can have only one root-level query!",
                            (!_validationTrigger || _queries.Count == 0));

                        _queries.Add(rootQuery);
                    }

                    rootQuery.LoadFromXml(node, this, rootQuery);
                }

                // Attempt to update the value once the query has been loaded.
                UpdateValue();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26108", ex);
                ee.AddDebugData("Query", query, false);
                throw ee;
            }
        }

        #endregion Contructors

        #region Properties

        /// <summary>
        /// Inidicates whether the query is completely resolved.
        /// </summary>
        /// <returns><see langword="true"/> if the query is completely resolved, <see langword="false"/>
        /// there are still trigger attributes that need to be registered..</returns>
        public bool GetIsFullyResolved()
        {
            // Check to see if the default query has been resolved (if one was specifed).
            if (_defaultQuery != null && !_defaultQuery.GetIsFullyResolved())
            {
                return false;
            }

            // Check to see if any query in the queries list has not yet been resolved.
            foreach (RootQueryNode query in _queries)
            {
                if (!query.GetIsFullyResolved())
                {
                    return false;
                }
            }

            return true;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Tests to see if the provided <see cref="IAttribute"/> matches the value query for any
        /// unresolved term, and, if so, resolves the term using the attribute as the trigger.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to potentially resolve any
        /// unresolved terms.</param>
        public void RegisterTriggerCandidate(IAttribute attribute)
        {
            try
            {
                ExtractException.Assert("ELI26114", "Null argument exception!", attribute != null);

                // If the path for the provided attribute has not been resolved, resolve it now.
                // (This will allow for more efficient processing of candidate triggers).
                AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(attribute);
                if (string.IsNullOrEmpty(statusInfo.FullPath))
                {
                    statusInfo.FullPath = AttributeStatusInfo.GetFullPath(attribute);
                }

                bool registeredTrigger = false;

                // Attempt to register the attribute as a trigger for the default query (if one was
                // specified).
                if (_defaultQuery != null && !_defaultQuery.GetIsFullyResolved())
                {
                    if (_defaultQuery.RegisterTriggerCandidate(statusInfo))
                    {
                        registeredTrigger = true;
                    }
                }

                // Attempt to register the attribute as a trigger with each query.
                foreach (RootQueryNode query in _queries)
                {
                    if (!query.GetIsFullyResolved())
                    {
                        if (query.RegisterTriggerCandidate(statusInfo))
                        {
                            registeredTrigger = true;
                        }
                    }
                }

                // If any attribute was registered as a trigger, attempt to update the target
                // attribute.
                if (registeredTrigger)
                {
                    UpdateValue();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26113", ex);
            }
        }

        /// <summary>
        /// Updates the target attribute's value by executing the query.
        /// </summary>
        /// <returns></returns>
        public bool UpdateValue()
        {
            try
            {
                bool valueUpdated = false;

                // If the attribute's value is empty and a default query has been specified,
                // update the attribute using the default query.
                if (_defaultQuery != null && !_defaultQuery.Disabled &&
                    _defaultQuery.GetIsMinimallyResolved())
                {
                    _defaultQuery.UpdateValue();
                }

                // Attempt an update with all resolved queries.
                foreach (RootQueryNode query in _queries)
                {
                    if (query.GetIsMinimallyResolved() && query.UpdateValue())
                    {
                        // Once the target attribute has been updated, don't attempt an update with
                        // any remaining queries.
                        valueUpdated = true;
                        break;
                    }
                }

                return valueUpdated;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26116", ex);
            }
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="AutoUpdateTrigger"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the <see cref="AutoUpdateTrigger"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param> 
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_defaultQuery != null)
                {
                    _defaultQuery.Dispose();
                    _defaultQuery = null;
                }

                CollectionMethods.ClearAndDispose(_queries);
            }
        }

        #endregion  IDisposable Members
    }
}
