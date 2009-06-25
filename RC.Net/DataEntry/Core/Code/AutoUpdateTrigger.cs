using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Registers an <see cref="IAttribute"/> for auto-updates using trigger attributes specified
    /// in the provided auto update query.
    /// </summary>
    internal class AutoUpdateTrigger : IDisposable
    {
        /// <summary>
        /// A helper class to track each term of the auto-update query.
        /// </summary>
        private class QueryTerm
        {
            /// <summary>
            /// A literal expression to define this term (no attributes need to be resolved during
            /// evaluation).
            /// </summary>
            public string _literalValue;

            /// <summary>
            /// An attribute query that defines the position of the trigger attribute in relation to
            /// the target attribute.
            /// </summary>
            public string _attributeValueQuery;

            /// <summary>
            /// Indicates whether single quotes should be escaped with another single quote for use
            /// in and SQL string value.
            /// </summary>
            public bool _escapeSingleQuote;

            /// <summary>
            /// The full path of any attribute matching the value query.  Used for efficiency when
            /// evaluating candidate triggers.
            /// </summary>
            public string _attributeValueFullPath;

            /// <summary>
            /// An attribute wich fulfills the query to be the trigger for this token. 
            /// </summary>
            public IAttribute _triggerAttribute; 
        }

        #region Fields

        /// <summary>
        /// The <see cref="IAttribute"/> to be updated using the auto-update query.
        /// </summary>
        private IAttribute _targetAttribute;

        /// <summary>
        /// The query divided into terms.
        /// </summary>
        private List<QueryTerm> _queryTerms;

        /// <summary>
        /// Specifies whether all trigger attributes have been resolved.
        /// </summary>
        private bool _resolved;

        /// <summary>
        /// A database for auto-update queries.
        /// </summary>
        private DbConnection _dbConnection;

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

                _targetAttribute = targetAttribute;
                _resolved = true;
                _dbConnection = dbConnection;
                _validationTrigger = validationTrigger;
                _queryTerms = new List<QueryTerm>();

                string rootPath = AttributeStatusInfo.GetFullPath(targetAttribute);

                // Parse the query into terms.
                int attributeQueryStart = query.IndexOf('{');
                while (attributeQueryStart != -1)
                {
                    int attributeQueryEnd = query.IndexOf('}', attributeQueryStart + 1);
                    ExtractException.Assert("ELI26106", "Invalid query!", attributeQueryEnd >= 0);

                    // Assign everything up to the opening attribute value query as a literal term.
                    if (attributeQueryStart > 0)
                    {
                        QueryTerm literalTerm = new QueryTerm();
                        literalTerm._literalValue = query.Substring(0, attributeQueryStart);
                        _queryTerms.Add(literalTerm);
                    }

                    int attributeQueryLength = (attributeQueryEnd - attributeQueryStart) - 1;

                    // Define an attribute value term.
                    QueryTerm attributeValueTerm = new QueryTerm();
                    attributeValueTerm._attributeValueQuery =
                        query.Substring(attributeQueryStart + 1, attributeQueryLength);
                    
                    // If the first character of the query is a single quote, it indicates that
                    // single quotes should be escaped in the result.
                    if (attributeValueTerm._attributeValueQuery.IndexOf('\'') == 0)
                    {
                        attributeValueTerm._escapeSingleQuote = true;
                        attributeValueTerm._attributeValueQuery =
                            attributeValueTerm._attributeValueQuery.Remove(0, 1);
                    }

                    attributeValueTerm._attributeValueFullPath = AttributeStatusInfo.GetFullPath(
                        rootPath, attributeValueTerm._attributeValueQuery);

                    // Attempt to register a trigger for the term.
                    if (!RegisterTriggerCandidate(attributeValueTerm, null))
                    {
                        _resolved = false;
                    }

                    _queryTerms.Add(attributeValueTerm);

                    // Remove the part of the query processed thus far
                    query = query.Remove(0, attributeQueryEnd + 1);
                
                    // ... then search for new attribute value terms.
                    attributeQueryStart = query.IndexOf('{');
                }

                // Anything remaining at the end becomes a literal term.
                if (!string.IsNullOrEmpty(query))
                {
                    QueryTerm literalTerm = new QueryTerm();
                    literalTerm._literalValue = query;
                    _queryTerms.Add(literalTerm);
                }

                // If all triggers were resolved, go ahead and update the value.
                if (_resolved)
                {
                    UpdateValue();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26108", ex);
            }
        }

        #endregion Contructors

        #region Properties

        /// <summary>
        /// Inidicates whether the query is completely resolved (can be executed)
        /// </summary>
        /// <returns><see langword="true"/> if the query is resolved, <see langword="false"/>
        /// if it is not.</returns>
        public bool IsResolved
        {
            get
            {
                return _resolved;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Tests to see if the provided <see cref="IAttribute"/> matches the value query for any
        /// unresolved term, and, if so, resolves the term using the attribute as the trigger.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to potentially resolve any
        /// unresolved terms.</param>
        /// <returns><see langword="true"/> if the <see cref="IAttribute"/> was successfully
        /// registered as the trigger for one or more unresolved terms; <see langword="false"/>
        /// otherwise.</returns>
        public bool RegisterTriggerCandidate(IAttribute attribute)
        {
            try
            {
                ExtractException.Assert("ELI26114", "Null argument exception!", attribute != null);

                // If the query is already resolved, don't bother processing the attribute.
                if (_resolved)
                {
                    return false;
                }

                bool registeredTrigger = false;
                bool unresolvedTermsRemain = false;

                // If the path for the provided attribute has not been resolved, resolve it now.
                // (This will allow for more efficient processing of candidate triggers).
                AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(attribute);
                if (string.IsNullOrEmpty(statusInfo.FullPath))
                {
                    statusInfo.FullPath = AttributeStatusInfo.GetFullPath(attribute);
                }

                // Attempt to register the attribute as a trigger with each term.
                foreach (QueryTerm term in _queryTerms)
                {
                    if (IsTermResolved(term))
                    {
                        // Nothing to do
                    }
                    else if (RegisterTriggerCandidate(term, statusInfo))
                    {
                        registeredTrigger = true;
                    }
                    else
                    {
                        unresolvedTermsRemain = true;
                    }
                }

                // If all terms are now resolved, update the value now.
                if (!unresolvedTermsRemain)
                {
                    _resolved = true;

                    UpdateValue();
                }

                return registeredTrigger;
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
                // The value can only be updated if the query has been resolved.
                if (_resolved)
                {
                    StringBuilder query = new StringBuilder();

                    // Piece together the complete query by evaluating each term.
                    foreach (QueryTerm term in _queryTerms)
                    {
                        // If the term is a literal value, simply append it.
                        if (term._literalValue != null)
                        {
                            query.Append(term._literalValue);
                        }
                        // If the term is defined by an attribute query, use the attribute's value
                        // to fill in the term.
                        else
                        {
                            ExtractException.Assert("ELI26119", "Failed to update data!",
                                term._triggerAttribute != null);

                            // Escape any contained single quotes if necessary.
                            if (term._escapeSingleQuote)
                            {
                                query.Append(
                                    term._triggerAttribute.Value.String.Replace("\'", "\'\'"));
                            }
                            else
                            {
                                query.Append(term._triggerAttribute.Value.String);
                            }
                        }
                    }

                    string queryString = query.ToString();

                    // Parse and resolve all SQL queries from the overall query.
                    int sqlQueryStart = queryString.IndexOf("<SQL>", StringComparison.Ordinal);
                    while (sqlQueryStart >= 0)
                    {
                        int sqlQueryEnd = queryString.IndexOf("</SQL>", sqlQueryStart + 1,
                            StringComparison.Ordinal);
                        ExtractException.Assert("ELI26149", "Invalid query!", sqlQueryEnd >= 0);

                        int sqlQueryLength = (sqlQueryEnd - sqlQueryStart) - 5;
                        string sqlQueryString =
                            queryString.Substring(sqlQueryStart + 5, sqlQueryLength);
                        queryString = queryString.Remove(sqlQueryStart, sqlQueryLength + 11);

                        string queryResult = 
                            DataEntryMethods.ExecuteSqlQuery(_dbConnection, sqlQueryString, 
                                (_validationTrigger ? "\r\n" : null), ", ");

                        if (!string.IsNullOrEmpty(queryResult))
                        {
                            // Insert the query results in place of the query.
                            queryString = queryString.Insert(sqlQueryStart, queryResult);
                        }
                        else if (!_validationTrigger)
                        {
                            // If the query produced no results, we do not have the data necessary to
                            // perform an update.  Return false.
                            return false;
                        }

                        // Look for any additional SQL queries
                        sqlQueryStart = queryString.IndexOf("<SQL>", StringComparison.Ordinal);
                    }

                    if (_validationTrigger)
                    {
                        // Update the validation list associated with the attribute.
                        AttributeStatusInfo statusInfo =
                            AttributeStatusInfo.GetStatusInfo(_targetAttribute);

                        DataEntryValidator validator = statusInfo.Validator;
                        ExtractException.Assert("ELI26154", "Uninitialized validator!",
                            validator != null);

                        // Parse the file contents into individual list items.
                        string[] listItems = queryString.Split(new string[] { Environment.NewLine },
                            StringSplitOptions.RemoveEmptyEntries);

                        validator.SetValidationListValues(listItems);
                        statusInfo.OwningControl.RefreshAttribute(_targetAttribute);
                    }
                    else
                    {
                        // Update the attribute's value.
                        AttributeStatusInfo.SetValue(_targetAttribute, queryString, false, true);

                        // After applying the value, direct the control that contains it to refresh the
                        // value.
                        AttributeStatusInfo.GetOwningControl(_targetAttribute).RefreshAttribute(
                            _targetAttribute);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26116", ex);
            }
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles the case that data was modified in a trigger <see cref="IAttribute"/> in order
        /// trigger the target attribute to update.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="AttributeValueModifiedEventArgs"/> that contains the event data.
        /// </param>
        private void HandleAttributeValueModified(object sender, AttributeValueModifiedEventArgs e)
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

        /// <summary>
        /// Handles the case that a trigger <see cref="IAttribute"/> was deleted so that it can be
        /// un-registered as a trigger.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="AttributeDeletedEventArgs"/> that contains the event data.
        /// </param>
        private void HandleAttributeDeleted(object sender, AttributeDeletedEventArgs e)
        {
            try
            {
                // Unregister the attribute as a trigger for all terms it is currently used in.
                foreach (QueryTerm term in _queryTerms)
                {
                    if (e.DeletedAttribute == term._triggerAttribute)
                    {
                        term._triggerAttribute = null;
                        _resolved = false;

                        AttributeStatusInfo statusInfo =
                            AttributeStatusInfo.GetStatusInfo(e.DeletedAttribute);

                        statusInfo.AttributeValueModified -= HandleAttributeValueModified;
                        statusInfo.AttributeDeleted -= HandleAttributeDeleted;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26218", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

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
                foreach (QueryTerm term in _queryTerms)
                {
                    if (term._triggerAttribute != null)
                    {
                        AttributeStatusInfo statusInfo =
                            AttributeStatusInfo.GetStatusInfo(term._triggerAttribute);
                        statusInfo.AttributeValueModified -= HandleAttributeValueModified;
                        statusInfo.AttributeDeleted -= HandleAttributeDeleted;
                    }
                }
            }
        }

        #endregion  IDisposable Members

        #region Private Members

        /// <summary>
        /// Tests to see if the provided <see cref="IAttribute"/> matches the value query for the
        /// specified term, and, if so, resolves the term using the attribute as the trigger.
        /// </summary>
        /// <param name="term">The term to test for a match to the specified attribute.</param>
        /// <param name="statusInfo">The <see cref="AttributeStatusInfo"/> associated with the
        /// suggested candidate attribute (if there was one). <see langword="null"/> otherwise.
        /// </param>
        /// <returns><see langword="true"/> if an <see cref="IAttribute"/> was successfully
        /// registered as the trigger for the term; <see langword="false"/> otherwise.</returns>
        private bool RegisterTriggerCandidate(QueryTerm term, AttributeStatusInfo statusInfo)
        {
            if (!IsTermResolved(term))
            {
                // Test to see that if an attribute was supplied, its path matches the path
                // we would expect for a trigger attribute.
                if (statusInfo == null || statusInfo.FullPath == term._attributeValueFullPath)
                {
                    // Search for candidate triggers.
                    IUnknownVector candidateTriggers = AttributeStatusInfo.ResolveAttributeQuery(
                                        _targetAttribute, term._attributeValueQuery);

                    int candidateCount = candidateTriggers.Size();

                    ExtractException.Assert("ELI26117",
                        "Multiple attribute triggers not supported for the auto-update value",
                        candidateCount <= 1);

                    // If a single candidate was found, register it as the trigger for this term
                    // (even if it wasn't the suggestd candidate).
                    if (candidateCount == 1)
                    {
                        term._triggerAttribute = (IAttribute)candidateTriggers.At(0);

                        if (statusInfo == null)
                        {
                            statusInfo = AttributeStatusInfo.GetStatusInfo(term._triggerAttribute);
                            statusInfo.FullPath = term._attributeValueFullPath;
                        }

                        statusInfo.AttributeValueModified += HandleAttributeValueModified;
                        statusInfo.AttributeDeleted += HandleAttributeDeleted;

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Indicates whether the specified term is resolved.
        /// </summary>
        /// <param name="term">The <see cref="QueryTerm"/> to test for whether its resolved.</param>
        /// <returns><see langword="true"/> if the term is resolved, <see langword="false"/>
        /// otherwise.</returns>
        private static bool IsTermResolved(QueryTerm term)
        {
            return (!string.IsNullOrEmpty(term._literalValue) || term._triggerAttribute != null);
        }

        #endregion Private Members
    }
}
