using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    /// <summary>
    /// A <see cref="QueryNode"/> that is to be resolved using the value of an
    /// <see cref="IAttribute"/>. Changes made to this <see cref="IAttribute"/> will cause the
    /// target attribute to be updated.
    /// </summary>
    internal class AttributeQueryNode : CompositeQueryNode
    {
        /// <summary>
        /// Handles the resolution of attribute queries (both on-demand and as new attributes are
        /// initialized.
        /// </summary>
        class AttributeReferenceManager
        {
            #region Statics

            /// <summary>
            /// For all attributes, maps the <see cref="AttributeQueryNode"/>s that are referencing
            /// <see cref="IAttribute"/>(s) beneath them. Used to quickly assign newly initialized
            /// attributes to the query node(s) that reference them.
            /// </summary>
            [ThreadStatic]
            static Dictionary<IAttribute, AttributeReferenceManager> _descendantQueryReferences
                = new Dictionary<IAttribute, AttributeReferenceManager>();

            /// <summary>
            /// Used as a placeholder for queries that reference attribute(s) with an absolute path
            /// (from the root). Needed because null can't be used as the key in
            /// _descendantQueryReferences.
            /// </summary>
            [ThreadStatic]
            static IAttribute _ROOT_ATTRIBUTE = new UCLID_AFCORELib.Attribute();

            #endregion Statics

            #region Fields

            /// <summary>
            /// Keeps track of all <see cref="AttributeQueryNode"/>s registered beneath a particular
            /// attribute.
            /// </summary>
            Dictionary<string, HashSet<AttributeQueryNode>> DescendantQueries =
                new Dictionary<string, HashSet<AttributeQueryNode>>();

            /// <summary>
            /// The sets from _descendantQueryReferences where this instance is registered to
            /// </summary>
            HashSet<HashSet<AttributeQueryNode>> _activeRegistrations =
                new HashSet<HashSet<AttributeQueryNode>>();

            /// <summary>
            /// The <see cref="AttributeQueryNode"/> this instance is managing attribute query
            /// registrations for.
            /// </summary>
            AttributeQueryNode _queryNode;

            #endregion Fields

            #region Construstors

            /// <summary>
            /// Initializes the <see cref="AttributeReferenceManager"/> class.
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
            static AttributeReferenceManager()
            {
                AttributeStatusInfo.AttributeInitialized += HandleAttributeInitialized;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AttributeReferenceManager"/> class.
            /// </summary>
            /// <param name="queryNode">The query node.</param>
            public AttributeReferenceManager(AttributeQueryNode queryNode)
            {
                _queryNode = queryNode;
            }

            #endregion Construstors

            #region Public Static Methods

            /// <summary>
            /// Unregisters all references.
            /// </summary>
            public static void UnregisterAll()
            {
                try
                {
                    InitializeStatics();

                    _descendantQueryReferences.Clear();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34513");
                }
            }

            #endregion Public Static Methods

            #region Public Members

            /// <summary>
            /// Resolves all existing attributes matching <see cref="_queryNode"/>'s reference path
            /// and registers a reference to be assigned all added attribute's matching the
            /// reference path.
            /// </summary>
            /// <returns>The existing <see cref="IAttribute"/>s matching the reference path.
            /// </returns>
            public HashSet<IAttribute> Register()
            {
                try
                {
                    var triggerAttributes = new HashSet<IAttribute>();

                    string queryString = _queryNode._attributeQueryString;
                    foreach (string queryPart in queryString.Split('|'))
                    {
                        triggerAttributes.UnionWith(Register(queryPart.Trim()));
                    }

                    return triggerAttributes;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35253");
                }
            }

            /// <summary>
            /// Resolves all existing attributes matching the specified <see paramref="queryString"/>
            /// and registers a reference to be assigned all added attribute's matching
            /// <see paramref="queryString"/>.
            /// </summary>
            /// <param name="queryString">The query string.</param>
            /// <returns>
            /// The existing <see cref="IAttribute"/>s matching the <see paramref="queryString"/>.
            /// </returns>
            public HashSet<IAttribute> Register(string queryString)
            {
                try
                {
                    InitializeStatics();

                    HashSet<IAttribute> attributeDomain = new HashSet<IAttribute>();
                    HashSet<IAttribute> triggerAttributes = new HashSet<IAttribute>();

                    // Divide the reference path into two parts:
                    // part1 = The "up" part; references the lowest attribute in the hierarchy that
                    // contains both the root attribute and the referenced attribute(s); AKA, the
                    // "domain" attribute(s)
                    // part2 = The "down" part; the path from the attribute reference in part 1 to
                    // the referenced attribute.
                    // Example: "../../Name/First" => part1 = "../.." part2 = "Name/First"
                    string pathPart1 = "";
                    string pathPart2 = "";
                    bool selfReferencing = (queryString == ".");
                    if (!selfReferencing)
                    {
                        pathPart2 = queryString.TrimStart(new char[] { '/', '.' });
                        ExtractException.Assert("ELI34514", "Invalid path!", !pathPart2.Contains(".."));
                        pathPart1 = queryString.Substring(0, queryString.Length - pathPart2.Length);
                    }

                    // Iterate each root attribute and find/register for all matching attributes
                    // relative to it.
                    foreach (IAttribute rootAttribute in GetRootAttributes())
                    {
                        // No need to register root attributes that are self-referencing; we know
                        // this attribute is the one and only that will ever match this path.
                        if (!selfReferencing)
                        {
                            foreach (IAttribute topMostAttribute in
                                AttributeStatusInfo.ResolveAttributeQuery(rootAttribute, pathPart1))
                            {
                                attributeDomain.Add(topMostAttribute ?? _ROOT_ATTRIBUTE);
                            }
                        }

                        // Find all existing attributes matching the refernce path relative to this
                        // root attribute.
                        foreach (IAttribute triggerAttribute in
                            AttributeStatusInfo.ResolveAttributeQuery((rootAttribute == _ROOT_ATTRIBUTE)
                                ? null
                                : rootAttribute, queryString))
                        {
                            triggerAttributes.Add(triggerAttribute);
                        }
                    }

                    // Keep track of all added registrations.
                    var newRegistrations = new HashSet<HashSet<AttributeQueryNode>>();

                    if (!selfReferencing)
                    {
                        // For each attribute in attributeDomain, register to receive any attributes
                        // beneath it matching pathPart2.
                        foreach (IAttribute domainAttribute in attributeDomain)
                        {
                            AttributeReferenceManager queryReferences;
                            if (!_descendantQueryReferences.TryGetValue(domainAttribute, out queryReferences))
                            {
                                queryReferences = new AttributeReferenceManager(_queryNode);
                                _descendantQueryReferences[domainAttribute] = queryReferences;
                            }

                            HashSet<AttributeQueryNode> referencingNodes;
                            if (!queryReferences.DescendantQueries.TryGetValue(pathPart2, out referencingNodes))
                            {
                                referencingNodes = new HashSet<AttributeQueryNode>();
                                queryReferences.DescendantQueries[pathPart2] = referencingNodes;
                            }

                            referencingNodes.Add(_queryNode);
                            newRegistrations.Add(referencingNodes);
                        }
                    }

                    // Remove any existing registrations not in the newRegistrations set.
                    _activeRegistrations.RemoveWhere(registration => !newRegistrations.Contains(registration));
                    foreach (HashSet<AttributeQueryNode> registration in newRegistrations)
                    {
                        _activeRegistrations.Add(registration);
                    }

                    return triggerAttributes;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34515");
                }
            }

            /// <summary>
            /// Unregisters this instance.
            /// </summary>
            public void Unregister()
            {
                try
                {
                    foreach (HashSet<AttributeQueryNode> registration in _activeRegistrations)
                    {
                        registration.Remove(_queryNode);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34516");
                }
            }

            #endregion Public Members

            #region Event Handlers

            /// <summary>
            /// Handles the <see cref="AttributeStatusInfo.AttributeInitialized"/> event by
            /// assigning the new attribute to all <see cref="AttributeQueryNode"/>s with matching
            /// references.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="AttributeInitializedEventArgs"/> instance containing
            /// the event data.</param>
            static void HandleAttributeInitialized(object sender, AttributeInitializedEventArgs e)
            {
                try
                {
                    InitializeStatics();

                    // Register to be notified with this attribute is deleted so that any
                    // registrations for the attribute or its descendants can be removed.
                    AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(e.Attribute);
                    statusInfo.AttributeDeleted += HandleAttributeDeletedStatic;

                    // For this attribute and each ancestor, check all references beneath each to
                    // see if the registered path matches the path to the new attribute.
                    string path = String.Empty;
                    for (IAttribute attribute = e.Attribute;
                         attribute != null;
                         attribute = (attribute == _ROOT_ATTRIBUTE)
                            ? null
                            : AttributeStatusInfo.GetParentAttribute(attribute) ?? _ROOT_ATTRIBUTE)
                    {
                        // Get the references relative to the current attribute.
                        AttributeReferenceManager descendantReferences;
                        if (_descendantQueryReferences.TryGetValue(attribute,
                            out descendantReferences))
                        {
                            // Get the references matching the path to the new attribute.
                            HashSet<AttributeQueryNode> descendantQueries;
                            if (descendantReferences.DescendantQueries.TryGetValue(path,
                                out descendantQueries))
                            {
                                // For each reference, register the new attribute.
                                foreach (AttributeQueryNode query in descendantQueries)
                                {
                                    query.RegisterTriggerAttribute(e.Attribute);
                                }
                            }
                        }

                        // Add the name of this attribute to the path before moving on to the parent
                        // attribute.
                        path = string.IsNullOrEmpty(path)
                            ? attribute.Name
                            : path = attribute.Name + "/" + path;
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI34507");
                }
            }

            /// <summary>
            /// Handles the <see cref="AttributeStatusInfo.AttributeDeleted"/> event.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="Extract.DataEntry.AttributeDeletedEventArgs"/>
            /// instance containing the event data.</param>
            static void HandleAttributeDeletedStatic(object sender, AttributeDeletedEventArgs e)
            {
                try
                {
                    // Remove the deleted attribute from _descendantQueryReferences.
                    AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(e.DeletedAttribute);
                    statusInfo.AttributeDeleted -= HandleAttributeDeletedStatic;

                    _descendantQueryReferences.Remove(e.DeletedAttribute);
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI34512");
                }
            }

            #endregion Event Handlers

            #region Private Members

            /// <summary>
            /// Ensures all ThreadStatic variables are initialized.
            /// </summary>
            static void InitializeStatics()
            {
                if (_descendantQueryReferences == null)
                {
                    _descendantQueryReferences =
                        new Dictionary<IAttribute, AttributeReferenceManager>();
                    _ROOT_ATTRIBUTE = new UCLID_AFCORELib.Attribute();
                }
            }

            /// <summary>
            /// Gets all existing root attributes for the <see cref="_queryNode"/>.
            /// </summary>
            /// <returns>All existing root attributes for the <see cref="_queryNode"/>.</returns>
            IEnumerable<IAttribute> GetRootAttributes()
            {
                InitializeStatics();

                // If there is no defined root attribute result, use the root of the hierarchy for
                // absolute paths, or the specified root attribute for relative paths.
                if (_queryNode._rootAttributeResultQuery == null)
                {
                    if (_queryNode._attributeQueryString.StartsWith("/"))
                    {
                        yield return _ROOT_ATTRIBUTE;
                    }
                    else
                    {
                        yield return _queryNode.RootAttribute;
                    }
                }
                // If there is an an attribute result that specifies the root attribute(s), evaluate
                // it to obtain the root attribute(s).
                else
                {
                    QueryResult namedResult = _queryNode._rootAttributeResultQuery.Evaluate();
                    if (namedResult.IsAttribute)
                    {
                        // There may be multiple root attributes. Add the results using each root
                        // to the result.
                        foreach (IAttribute attribute in namedResult.ToAttributeArray())
                        {
                            yield return attribute;
                        }
                    }
                }
            }

            #endregion Private Members
        }

        #region Fields

        /// <summary>
        /// The <see cref="AttributeReferenceManager"/> that manages resolving the reference paths
        /// for this node.
        /// </summary>
        AttributeReferenceManager _attributeReferenceManager;

        /// <summary>
        /// The attributes defining the result of this query node and which triggers
        /// <see cref="CompositeQueryNode.QueryValueModified"/> events.
        /// </summary>
        HashSet<IAttribute> _triggerAttributes = new HashSet<IAttribute>();

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
        /// The attribute query defining the path to attributes that are to be evaluated.
        /// </summary>
        string _attributeQueryString;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initialized a new <see cref="AttributeQueryNode"/> instance.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that should be used to
        /// evaluate any SQL queries.</param>
        public AttributeQueryNode(IAttribute rootAttribute, DbConnection dbConnection)
            : base(rootAttribute, dbConnection)
        {
            try
            {
                _attributeReferenceManager = new AttributeReferenceManager(this);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34509");
            }
        }

        #endregion Constructors

        #region Public Static Members

        /// <summary>
        /// Unregisters <see cref="AttributeQueryNode"/> instances so they no longer react to newly
        /// added attributes that match their reference paths.
        /// </summary>
        public static void UnregisterAll()
        {
            try
            {
                AttributeReferenceManager.UnregisterAll();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34510");
            }
        }

        #endregion Public Static Members

        #region Overrides

        /// <summary>
        /// Loads the <see cref="AttributeQueryNode"/> using the specified XML query string.
        /// </summary>
        /// <param name="xmlNode">The XML query string defining the query.</param>
        /// <param name="namedReferences">A communal collection of named
        /// <see cref="NamedQueryReferences"/>s available to allow referencing of named nodes.</param>
        internal override void LoadFromXml(XmlNode xmlNode,
            Dictionary<string, NamedQueryReferences> namedReferences)
        {
            try
            {
                base.LoadFromXml(xmlNode, namedReferences);

                // Changes to the attribute should trigger an update unless specified not to.
                XmlAttribute xmlAttribute = xmlNode.Attributes["TriggerUpdate"];
                if (xmlAttribute != null)
                {
                    _triggerUpdate = xmlAttribute.Value.ToBoolean();
                }

                // If the query begins with a slash, search from the root of the attribute
                // hierarchy, not from the location of the root attribute.
                if (QueryText.StartsWith("/", StringComparison.Ordinal))
                {
                    RootAttribute = null;
                }
                else
                {
                    // If the root attribute is the result(s) of another node, create a new
                    // ResultQueryNode instance to reference those results.
                    xmlAttribute = xmlNode.Attributes["Root"];
                    if (xmlAttribute != null && !string.IsNullOrEmpty(xmlAttribute.Value))
                    {
                        _rootAttributeResultQuery =
                            new ResultQueryNode(xmlAttribute.Value, namedReferences);

                        var compositeRootAttributeQuery = _rootAttributeResultQuery as CompositeQueryNode;
                        if (compositeRootAttributeQuery != null)
                        {
                            compositeRootAttributeQuery.QueryValueModified +=
                                HandleQueryValueModified;
                        }

                        _rootAttributeResultQuery.ExcludeFromResult = true;
                        ChildNodes.Add(_rootAttributeResultQuery);
                        RootAttribute = null;
                    }
                }

                // [DataEntry:1146]
                // Call Evaluate to resolve all existing attributes referenced by this node and
                // register it to be assigned any added attributes matching the reference path.
                Evaluate();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26756", ex);
                ee.AddDebugData("XML", xmlNode.InnerXml, false);
                throw ee;
            }
        }

        /// <summary>
        /// Evaluates the query by using the value of the specified <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="childQueryResults"><see cref="QueryResult"/>s representing the results of
        /// each child <see cref="QueryNode"/>.</param>
        /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
        protected override QueryResult Evaluate(IEnumerable<QueryResult> childQueryResults)
        {
            try
            {
                if (_attributeQueryString == null)
                {
                    _attributeQueryString = QueryResult.Combine(childQueryResults).ToString();

                    RegisterTriggerAttributes();
                }
                
                QueryResult results = (_triggerAttributes.Count == 0)
                    ? new QueryResult(this)
                    : new QueryResult(this, _triggerAttributes.ToArray());

                return results;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI26728");
            }
        }

        #endregion Overrides

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="DataEntryQuery"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param> 
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    UnregisterTriggerAttributes();

                    if (_rootAttributeResultQuery != null)
                    {
                        _rootAttributeResultQuery.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI34500");
                }
            }

            base.Dispose(disposing);
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
                CachedResult = null;

                if (_triggerUpdate)
                {
                    OnQueryValueModified(new QueryValueModifiedEventArgs(e.IncrementalUpdate));
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26115", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

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
                CachedResult = null;

                ExtractException.Assert("ELI26717", "Mis-matched trigger attribute detected!",
                    _triggerAttributes.Contains(e.DeletedAttribute));

                // Unregister the attribute as a trigger for all terms it is currently used in.
                UnregisterTriggerAttribute(e.DeletedAttribute);

                if (_triggerUpdate)
                {
                    OnQueryValueModified(new QueryValueModifiedEventArgs(false));
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26718", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the trigger attribute deleted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="QueryValueModifiedEventArgs"/> instance containing the
        /// event data.</param>
        protected override void HandleQueryValueModified(object sender, QueryValueModifiedEventArgs e)
        {
            try
            {
                // If it is the root attribute query that has been modified, force re-registration
                // of the trigger attributes next time Evaluate is called.
                if (sender == _rootAttributeResultQuery)
                {
                    _attributeQueryString = null;
                }

                base.HandleQueryValueModified(sender, e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34503");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Resolves all existing attributes reference by this node, and register to be assigned any
        /// added attributes matching the reference path.
        /// </summary>
        void RegisterTriggerAttributes()
        {
            try
            {
                HashSet<IAttribute> newTriggerAttributes = _attributeReferenceManager.Register();

                HashSet<IAttribute> oldTriggerAttributes = new HashSet<IAttribute>(_triggerAttributes
                    .Where(attribute => !newTriggerAttributes.Contains(attribute)));

                // Unregister any trigger attributes no longer referenced by this instance.
                foreach (IAttribute oldAttribute in oldTriggerAttributes)
                {
                    UnregisterTriggerAttribute(oldAttribute);
                }

                // Register the new trigger attributes.
                foreach (IAttribute newAttribute in newTriggerAttributes
                    .Where(attribute => !oldTriggerAttributes.Contains(attribute)))
                {
                    RegisterTriggerAttribute(newAttribute);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34502");
            }
        }

        /// <summary>
        /// Registers the specified <see cref="IAttribute"/> as a trigger attribute.
        /// </summary>
        /// <param name="triggerAttribute">The <see cref="IAttribute"/> to be a trigger.</param>
        void RegisterTriggerAttribute(IAttribute triggerAttribute)
        {
            if (!_triggerAttributes.Contains(triggerAttribute))
            {
                AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(triggerAttribute);

                // Handle deletion of the attribute so the query will become unresolved
                // if the trigger attribute is deleted.
                statusInfo.AttributeDeleted += HandleAttributeDeleted;

                _triggerAttributes.Add(triggerAttribute);

                CachedResult = null;

                if (_triggerUpdate)
                {
                    statusInfo.AttributeValueModified += HandleAttributeValueModified;

                    OnQueryValueModified(new QueryValueModifiedEventArgs(false));
                }
            }
        }

        /// <summary>
        /// Unregisters all existing trigger attribute(s).
        /// </summary>
        void UnregisterTriggerAttributes()
        {
            _attributeReferenceManager.Unregister();

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

            if (_triggerUpdate)
            {
                statusInfo.AttributeValueModified -= HandleAttributeValueModified;
            }

            _triggerAttributes.Remove(triggerAttribute);
        }

        #endregion Private Members
    }
}
