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
    partial class AutoUpdateTrigger
    {
        /// <summary>
        /// The name of a query element which should be resolved using an SQL query.
        /// </summary>
        private static string _SQL_NODE = "SQL";

        /// <summary>
        /// The name of a query element which should be resolved using an Attribute query.
        /// </summary>
        private static string _ATTRIBUTE_NODE = "Attribute";

        /// <summary>
        /// The name of a query element which should be resolved using the current source doc name.
        /// </summary>
        private static string _SOURCEDOCNAME_NODE = "SourceDocName";

        /// <summary>
        /// The name of an element attribute specifying whether the result should be parameterized
        /// if it is used in an SQL query.
        /// </summary>
        private static string _PARAMETERIZE_ATTRIBUTE = "Parameterize";

        /// <summary>
        /// Describes node in an <see cref="AutoUpdateTrigger"/> query.
        /// </summary>
        private interface IQueryNode
        {
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
            bool Parameterize
            {
                get;
                set;
            }

            /// <summary>
            /// Gets whether the query node is completely resolved (all required triggers have been
            /// registered) and can be evaluated.
            /// </summary>
            /// <returns><see langword="true"/> if all triggers are registered and the query node can
            /// be evaluated, <see langword="false"/> if one or more triggers must be resolved before
            /// the query node can be evaluated.</returns>
            bool IsResolved
            {
                get;
            }

            /// <summary>
            /// Attempts to register candidate <see cref="IAttribute"/> trigger(s).
            /// </summary>
            /// <param name="statusInfo">If <see langword="null"/> the <see cref="IQueryNode"/> will
            /// attempt to resolve all unresolved triggers. If specified, the corresponding
            /// query node will attempt to register the corresponding <see cref="IAttribute"/> with
            /// all unresolved nodes.</param>
            /// <returns><see langword="true"/> if one or more <see cref="IQueryNode"/>s were
            /// resolved; <see langword="false"/> otherwise.</returns>
            bool RegisterTriggerCandidate(AttributeStatusInfo statusInfo);

            /// <summary>
            /// Evaluates the query.
            /// </summary>
            /// <returns>A <see langword="string"/> representing the result of the query.</returns>
            string Evaluate();
        }

        /// <summary>
        /// An <see cref="IQueryNode"/> consisting of un-interpreted, literal text.
        /// </summary>
        private class LiteralQueryNode : IQueryNode
        {
            /// <summary>
            /// The literal text to be returned during evaluation.
            /// </summary>
            protected string _query;

            /// <summary>
            /// Specifies whether the query node should be parameterized if it is used
            /// in an <see cref="SqlQueryNode"/>.
            /// </summary>
            protected bool _parameterize;

            /// <summary>
            /// Initializes a new <see cref="LiteralQueryNode"/> instance.
            /// </summary>
            /// <param name="query">The literal text to be returned during evaluation.</param>
            public LiteralQueryNode(string query)
            {
                _query = query;
            }

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
            /// Gets whether the query node is completely resolved (all required triggers have been
            /// registered) and can be evaluated.
            /// </summary>
            /// <returns><see langword="true"/> if all triggers are registered and the query node can
            /// be evaluated, <see langword="false"/> if one or more triggers must be resolved before
            /// the query node can be evaluated.</returns>
            public bool IsResolved
            {
                get
                {
                    return !string.IsNullOrEmpty(_query);
                }
            }

            /// <summary>
            /// Attempts to register candidate <see cref="IAttribute"/> trigger(s).
            /// </summary>
            /// <param name="statusInfo">Not used by <see cref="LiteralQueryNode"/>.</param>
            /// <returns><see langword="false"/> since <see cref="LiteralQueryNode"/> instances do
            /// not support trigger attributes.</returns>
            public bool RegisterTriggerCandidate(AttributeStatusInfo statusInfo)
            {
                return false;
            }

            /// <summary>
            /// Evaluates the query.
            /// </summary>
            /// <returns>A <see langword="string"/> representing the result of the query.</returns>
            public string Evaluate()
            {
                ExtractException.Assert("ELI26753", "Cannot evaluate un-resolved query!",
                    this.IsResolved);

                return _query;
            }
        }

        /// <summary>
        /// An <see cref="IQueryNode"/> to be resolved using the current source doc name.
        /// </summary>
        private class SourceDocNameQueryNode : IQueryNode
        {
            /// <summary>
            /// Specifies whether the query node should be parameterized if it is used
            /// in an <see cref="SqlQueryNode"/>.
            /// </summary>
            protected bool _parameterize;

            /// <summary>
            /// Initializes a new <see cref="SourceDocNameQueryNode"/> instance.
            /// </summary>
            public SourceDocNameQueryNode()
            {
            }

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
            /// Gets whether the query node is completely resolved (all required triggers have been
            /// registered) and can be evaluated.
            /// </summary>
            /// <returns><see langword="true"/> if all triggers are registered and the query node can
            /// be evaluated, <see langword="false"/> if one or more triggers must be resolved before
            /// the query node can be evaluated.</returns>
            public bool IsResolved
            {
                get
                {
                    return true;
                }
            }

            /// <summary>
            /// Attempts to register candidate <see cref="IAttribute"/> trigger(s).
            /// </summary>
            /// <param name="statusInfo">Not used by <see cref="SourceDocNameQueryNode"/>.</param>
            /// <returns><see langword="false"/> since <see cref="SourceDocNameQueryNode"/>
            /// instances do not support trigger attributes.</returns>
            public bool RegisterTriggerCandidate(AttributeStatusInfo statusInfo)
            {
                return false;
            }

            /// <summary>
            /// Evaluates the query using the current source doc name.
            /// </summary>
            /// <returns>The current source doc name</returns>
            public string Evaluate()
            {
                return AttributeStatusInfo.SourceDocName;
            }
        }

        /// <summary>
        /// An <see cref="IQueryNode"/> that is comprised of one or more child
        /// <see cref="IQueryNode"/>s.
        /// </summary>
        private class ComplexQueryNode : IQueryNode
        {
            /// <summary>
            /// The unparsed XML from which the query node has been loaded.
            /// </summary>
            protected string _query;

            /// <summary>
            /// Specifies whether the query node should be should be parameterized if it is used
            /// in an <see cref="SqlQueryNode"/>.
            /// </summary>
            protected bool _parameterize = true;

            /// <summary>
            /// The AutoUpdateTrigger to which this query belongs.
            /// </summary>
            protected AutoUpdateTrigger _trigger;

            /// <summary>
            /// The RootQueryNode to which this query is a descendent.
            /// </summary>
            protected RootQueryNode _rootQuery;

            /// <summary>
            /// The child IQueryNodes of this query node.
            /// </summary>
            protected List<IQueryNode> _childNodes = new List<IQueryNode>();

            /// <summary>
            /// Initializes a new <see cref="ComplexQueryNode"/> instance.
            /// </summary>
            public ComplexQueryNode()
            {
            }

            /// <summary>
            /// Loads the <see cref="IQueryNode"/> using the specified XML query string.
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
                XmlAttribute xmlAttribute = xmlNode.Attributes[_PARAMETERIZE_ATTRIBUTE];
                if (xmlAttribute != null && xmlAttribute.Value == "0")
                {
                    _parameterize = false;
                }

                // Iterate through each child of the current XML node and use each to initialize a
                // new child IQueryNode.
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
                        if (childElement.Name.Equals(_SOURCEDOCNAME_NODE, 
                                StringComparison.OrdinalIgnoreCase))
                        {
                            _childNodes.Add(new SourceDocNameQueryNode());
                        }
                        else
                        {
                            ComplexQueryNode childQueryNode = null;

                            // Create the element as an SQL node?
                            if (childElement.Name.Equals(_SQL_NODE,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                childQueryNode = new SqlQueryNode();
                            }
                            // Create the element as an Attribute node?
                            else if (childElement.Name.Equals(_ATTRIBUTE_NODE,
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
            /// Gets whether the query node is completely resolved (all required triggers have been
            /// registered) and can be evaluated.
            /// </summary>
            /// <returns><see langword="true"/> if all triggers are registered and the query node can
            /// be evaluated, <see langword="false"/> if one or more triggers must be resolved before
            /// the query node can be evaluated.</returns>
            public virtual bool IsResolved
            {
                get
                {
                    foreach (IQueryNode childNode in _childNodes)
                    {
                        if (!childNode.IsResolved)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            /// <summary>
            /// Attempts to register candidate <see cref="IAttribute"/> trigger(s).
            /// </summary>
            /// <param name="statusInfo">If <see langword="null"/> the <see cref="IQueryNode"/> will
            /// attempt to resolve all unresolved triggers. If specified, the corresponding
            /// query node will attempt to register the corresponding <see cref="IAttribute"/> with
            /// all unresolved nodes.</param>
            /// <returns><see langword="true"/> if one or more <see cref="IQueryNode"/>s were
            /// resolved; <see langword="false"/> otherwise.</returns>
            public virtual bool RegisterTriggerCandidate(AttributeStatusInfo statusInfo)
            {
                bool resolved = false;

                // If this query isn't resolved, attempt to register all child query nodes.
                if (!this.IsResolved)
                {
                    foreach (IQueryNode childNode in _childNodes)
                    {
                        if (childNode.RegisterTriggerCandidate(statusInfo))
                        {
                            resolved = true;
                        }
                    }
                }

                return resolved;
            }

            /// <summary>
            /// Evaluates the query by combining all child <see cref="IQueryNode"/>s.
            /// </summary>
            /// <returns>A <see langword="string"/> representing the result of the query.</returns>
            public virtual string Evaluate()
            {
                try
                {
                    StringBuilder result = new StringBuilder();

                    // Combine the results of all child nodes.
                    foreach (IQueryNode childNode in _childNodes)
                    {
                        result.Append(childNode.Evaluate());
                    }

                    return result.ToString();
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
        /// An <see cref="IQueryNode"/> that is to be resolved using an SQL query against the active
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
            /// <see cref="IQueryNode"/>s as an SQL query against the active database.
            /// </summary>
            /// <returns>A <see langword="string"/> representing the result of the query.</returns>
            public override string Evaluate()
            {
                try
                {
                    ExtractException.Assert("ELI26754", "Cannot evaluate un-resolved query!",
                        this.IsResolved);

                    StringBuilder sqlQuery = new StringBuilder();

                    SqlCeConnection sqlCeConnection = _trigger._dbConnection as SqlCeConnection;

                    ExtractException.Assert("ELI26733",
                        "Auto-update queries currently support only SQL compact databases",
                        sqlCeConnection != null);

                    // Child query nodes whose results have been parameterized.
                    Dictionary<string, string> parameters = new Dictionary<string, string>();

                    // Combine the result of all child queries parameterizing as necessary.
                    foreach (IQueryNode childNode in _childNodes)
                    {
                        if (childNode.Parameterize)
                        {
                            // If parameterizing, don't add the query result directly, rather add a
                            // parameter name to the query and add the key/value pair to parameters.
                            string key = "@" + parameters.Count.ToString(CultureInfo.InvariantCulture);
                            string value = childNode.Evaluate();

                            parameters[key] = value;
                            sqlQuery.Append(key);
                        }
                        else
                        {
                            sqlQuery.Append(childNode.Evaluate());
                        }
                    }

                    // Create a database command using the query.
                    using (DbCommand dbCommand = DataEntryMethods.CreateDBCommand(
                        sqlCeConnection, sqlQuery.ToString(), parameters))
                    {
                        return DataEntryMethods.ExecuteDBQuery(dbCommand,
                            (_trigger._validationTrigger ? "\r\n" : null), ", ");
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
        /// An <see cref="IQueryNode"/> that is to be resolved using the value of an
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
            /// Gets whether the query node is completely resolved (all required triggers have been
            /// registered) and can be evaluated.
            /// </summary>
            /// <returns><see langword="true"/> if all triggers are registered and the query node can
            /// be evaluated, <see langword="false"/> if one or more triggers must be resolved before
            /// the query node can be evaluated.</returns>
            public override bool IsResolved
            {
                get
                {
                    // If a triggerAttribute was previously registered, but the query used to
                    // register it is no longer resolved, unregister the trigger attribute.
                    if (!base.IsResolved && _triggerAttribute != null)
                    {
                        UnregisterTriggerAttribute();
                    }

                    return base.IsResolved && _triggerAttribute != null;
                }
            }

            /// <summary>
            /// Evaluates the query by using the value of the specified <see cref="IAttribute"/>.
            /// </summary>
            /// <returns>A <see langword="string"/> representing the result of the query.</returns>
            public override string Evaluate()
            {
                try
                {
                    ExtractException.Assert("ELI26757", "Cannot evaluate un-resolved query!",
                        this.IsResolved);

                    return _triggerAttribute.Value.String;
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
            /// <param name="statusInfo">If <see langword="null"/> the <see cref="IQueryNode"/> will
            /// attempt to resolve all unresolved triggers. If specified, the corresponding
            /// query node will attempt to register the corresponding <see cref="IAttribute"/> with
            /// all unresolved nodes.</param>
            /// <returns><see langword="true"/> if one or more <see cref="IQueryNode"/>s were
            /// resolved; <see langword="false"/> otherwise.</returns>
            public override bool RegisterTriggerCandidate(AttributeStatusInfo statusInfo)
            {
                // First attempt to register all child nodes.
                bool resolved = base.RegisterTriggerCandidate(statusInfo);

                // If all child nodes are resolved, but this node is not, attempt to resolve it.
                if (!this.IsResolved && base.IsResolved)
                {
                    if (string.IsNullOrEmpty(_attributeValueFullPath))
                    {
                        _attributeValueFullPath = AttributeStatusInfo.GetFullPath(
                            _trigger._rootPath, base.Evaluate());
                    }

                    // Test to see that if an attribute was supplied, its path matches the path
                    // we would expect for a trigger attribute.
                    if (statusInfo == null || statusInfo.FullPath == _attributeValueFullPath)
                    {
                        // Search for candidate triggers.
                        IUnknownVector candidateTriggers = AttributeStatusInfo.ResolveAttributeQuery(
                                            _trigger._targetAttribute, base.Evaluate());

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
        /// An <see cref="IQueryNode"/> that is to represent the root level of a query for a
        /// <see cref="AutoUpdateTrigger"/>.
        /// </summary>
        private class RootQueryNode : ComplexQueryNode, IDisposable
        {
            /// <summary>
            /// The set of <see cref="IAttribute"/>s that will trigger this query to be evaluated.
            /// </summary>
            List<IAttribute> _triggerAttributes = new List<IAttribute>();

            /// <summary>
            /// Initializes a new <see cref="RootQueryNode"/> instance.
            /// </summary>
            public RootQueryNode()
                : base()
            {
            }

            /// <summary>
            /// Attempts to update the target <see cref="IAttribute"/> using the result of the
            /// evaluated query.
            /// </summary>
            public bool UpdateValue()
            {
                try
                {
                    // Ensure the query is resolved.
                    if (base.IsResolved)
                    {
                        // If so, evaluate it.
                        string queryResult = base.Evaluate();

                        // Use the results to update the target attribute's validation list if the
                        // AutoUpdateTrigger is a validation trigger.
                        if (_trigger._validationTrigger)
                        {
                            // Update the validation list associated with the attribute.
                            AttributeStatusInfo statusInfo =
                                AttributeStatusInfo.GetStatusInfo(_trigger._targetAttribute);

                            DataEntryValidator validator = statusInfo.Validator;
                            ExtractException.Assert("ELI26154", "Uninitialized validator!",
                                validator != null);

                            // Parse the file contents into individual list items.
                            string[] listItems = queryResult.ToString().Split(
                                new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                            validator.SetValidationListValues(listItems);
                            statusInfo.OwningControl.RefreshAttribute(_trigger._targetAttribute);

                            return true;
                        }
                        // Otherwise, update the value of the attribute itself.
                        else
                        {
                            if (!string.IsNullOrEmpty(queryResult))
                            {
                                // Update the attribute's value.
                                AttributeStatusInfo.SetValue(_trigger._targetAttribute, queryResult,
                                    false, true);

                                // After applying the value, direct the control that contains it to
                                // refresh the value.
                                AttributeStatusInfo.GetOwningControl(_trigger._targetAttribute).
                                    RefreshAttribute(_trigger._targetAttribute);

                                return true;
                            }
                        }
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
                    foreach (IAttribute attribute in _triggerAttributes)
                    {
                        AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(attribute);
                        statusInfo.AttributeValueModified -= HandleAttributeValueModified;
                    }

                    _triggerAttributes.Clear();
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
                        UpdateValue();
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
        }
    }
}