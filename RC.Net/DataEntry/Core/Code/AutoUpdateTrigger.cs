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
        private List<DataEntryQuery> _queries = new List<DataEntryQuery>();

        /// <summary>
        /// An auto-update query to be applied if the target attribute otherwise does not have any
        /// value.
        /// </summary>
        private DataEntryQuery _defaultQuery;

        /// <summary>
        /// A database for auto-update queries.
        /// </summary>
        private DbConnection _dbConnection;

        /// <summary>
        /// Indicates whether the trigger is to be used to update a validation list instead of the
        /// value itself.
        /// </summary>
        private bool _validationTrigger;

        /// <summary>
        /// Indicates whether the trigger as updated the target attribute since its creation.
        /// </summary>
        bool _hasUpdatedValue;

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
        /// <param name="resolveOnLoad"><see langword="true"/> if the query will fire if possible
        /// as it is loaded, <see langword="false"/> if <see cref="RegisterTriggerCandidate"/> must
        /// be called manually to resolve the query if attribute triggers are involved.</param>
        public AutoUpdateTrigger(IAttribute targetAttribute, string query,
            DbConnection dbConnection, bool validationTrigger, bool resolveOnLoad)
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

                DataEntryQuery[] dataEntryQueries = DataEntryQuery.CreateList(
                    query, _targetAttribute, _dbConnection,
                    _validationTrigger
                        ? MultipleQueryResultSelectionMode.List
                        : MultipleQueryResultSelectionMode.None,
                    resolveOnLoad);

                foreach (DataEntryQuery dataEntryQuery in dataEntryQueries)
                {
                    // Check to see if this query has been specified as the default.
                    if (dataEntryQuery.DefaultQuery)
                    {
                        ExtractException.Assert("ELI26734",
                             "Validation queries cannot have a default query!", !_validationTrigger);
                        ExtractException.Assert("ELI26763",
                            "Only one default query can be specified!", _defaultQuery == null);

                        _defaultQuery = dataEntryQuery;
                    }
                    // Otherwise add it into the general _queries list.
                    else
                    {
                        _queries.Add(dataEntryQuery);
                    }

                    dataEntryQuery.TriggerAttributeModified += HandleQueryTriggerAttributeModified;
                    dataEntryQuery.TriggerAttributeDeleted += HandleQueryTriggerAttributeDeleted;
                }

                // Attempt to update the value once the query has been loaded.
                if (resolveOnLoad)
                {
                    UpdateValue();
                }
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
            foreach (DataEntryQuery query in _queries)
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
                AttributeStatusInfo statusInfo = null;

                if (attribute != null)
                {
                    // If the path for the provided attribute has not been resolved, resolve it now.
                    // (This will allow for more efficient processing of candidate triggers).
                    statusInfo = AttributeStatusInfo.GetStatusInfo(attribute);
                    if (string.IsNullOrEmpty(statusInfo.FullPath))
                    {
                        statusInfo.FullPath = AttributeStatusInfo.GetFullPath(attribute);
                    }
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
                foreach (DataEntryQuery query in _queries)
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
                // attribute. Also attempt an update if a general registration call is being made
                // (attribute == null) and UpdateValue has not yet been called.
                if (registeredTrigger || (attribute == null && !_hasUpdatedValue))
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
                    UpdateValue(_defaultQuery);
                }

                // Attempt an update with all resolved queries.
                foreach (DataEntryQuery query in _queries)
                {
                    if (query.GetIsMinimallyResolved() && UpdateValue(query))
                    {
                        valueUpdated = true;

                        // If this is not a validation trigger, once the attribute has been updated
                        // don't attempt an update with any remaining queries.
                        if (!_validationTrigger)
                        {
                            break;
                        }
                    }
                }

                _hasUpdatedValue = valueUpdated;

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

        #region EventHandlers

        /// <summary>
        /// Handles the case that the trigger <see cref="IAttribute"/> for a member query has been
        /// modified. The query needs to be re-evaluated and applied to the target attribute as
        /// appropriate.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="AttributeValueModifiedEventArgs"/> that contains the
        /// event data.</param>
        void HandleQueryTriggerAttributeModified(object sender, AttributeValueModifiedEventArgs e)
        {
            try
            {
                if (!e.IncrementalUpdate)
                {
                    DataEntryQuery dataEntryQuery = (DataEntryQuery)sender;

                    if (dataEntryQuery.DefaultQuery)
                    {
                        // Always ensure a default query applies updates as part of a full
                        // auto-update trigger to ensure normal auto-update triggers can apply
                        // their updates on top of any default value.
                        UpdateValue();
                    }
                    // If not a default auto-update trigger, update using only the sending and query
                    // not the entire trigger.
                    // Only update the value if a previous auto-update query hasn't already
                    // updated the value or this is a validation query.
                    // NOTE: This logic is dependent upon event delegates (or multicast
                    // delegates more generally) being called in order. There seems to be
                    // conflicting information on whether this is actually the case, but MSDN
                    // indicates it is the case and it is the behavior I am seeing in practice.
                    else if (_validationTrigger ||
                             !e.AutoUpdatedAttributes.Contains(_targetAttribute))
                    {
                        // Indicate that the target attribute value has been updated by this
                        // event if this is not a validation trigger.
                        if (UpdateValue(dataEntryQuery) && !_validationTrigger)
                        {
                            e.AutoUpdatedAttributes.Add(_targetAttribute);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28858", ex);
                ee.AddDebugData("Event Data", e, false);
                throw ee;
            }
        }

        
        /// <summary>
        /// Handles the case that the trigger <see cref="IAttribute"/> for a member query has been
        /// modified. The query needs to be re-evaluated and applied to the target attribute as
        /// appropriate.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="AttributeDeletedEventArgs"/> that contains the
        /// event data.</param>
        void HandleQueryTriggerAttributeDeleted(object sender, AttributeDeletedEventArgs e)
        {
            try
            {
                DataEntryQuery dataEntryQuery = (DataEntryQuery)sender;

                if (dataEntryQuery.DefaultQuery)
                {
                    // Always ensure a default query applies updates as part of a full
                    // auto-update trigger to ensure normal auto-update triggers can apply
                    // their updates on top of any default value.
                    UpdateValue();
                }
                // If not a default auto-update trigger, update using only the sending and query
                // not the entire trigger.
                else 
                {
                    UpdateValue(dataEntryQuery);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28926", ex);
                ee.AddDebugData("Event Data", e, false);
                throw ee;
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Attempts to update the target <see cref="IAttribute"/> using the result of the
        /// evaluated query.
        /// </summary>
        /// <param name="dataEntryQuery">The <see cref="DataEntryQuery"/> that should be used to
        /// update the target attribute.</param>
        /// <returns><see langword="true"/> if the target attribute was updated;
        /// <see langword="false"/> otherwise.</returns>
        bool UpdateValue(DataEntryQuery dataEntryQuery)
        {
            try
            {
                // Ensure the query is resolved.
                if (dataEntryQuery.GetIsMinimallyResolved())
                {
                    // If so, evaluate it.
                    QueryResult queryResult = dataEntryQuery.Evaluate();

                    // Use the results to update the target attribute's validation list if the
                    // AutoUpdateTrigger is a validation trigger.
                    if (_validationTrigger)
                    {
                        // Update the validation list associated with the attribute.
                        AttributeStatusInfo statusInfo =
                            AttributeStatusInfo.GetStatusInfo(_targetAttribute);

                        // Validation queries can only be specified for attributes with a
                        // DataEntryValidator as its validator.
                        DataEntryValidator validator = statusInfo.Validator as DataEntryValidator;
                        ExtractException.Assert("ELI26154", "Uninitialized or invalid validator!",
                            validator != null);

                        // Initialize the auto-complete list using the query results.
                        if (dataEntryQuery.ValidationListType != ValidationListType.ValidationListOnly)
                        {
                            string[] queryResultArray = queryResult.ToStringArray();
                            validator.SetAutoCompleteValues(queryResultArray); 
                        }

                        // Initialize the validation query which will determine if an attribute is
                        // valid.
                        if (dataEntryQuery.ValidationListType != ValidationListType.AutoCompleteOnly)
                        {
                            validator.SetValidationQuery(dataEntryQuery, queryResult);
                        }

                        statusInfo.OwningControl.RefreshAttributes(false, _targetAttribute);

                        return true;
                    }
                    // If this auto-update query should only provide a default value
                    else if (dataEntryQuery.DefaultQuery)
                    {
                        // If the target attribute is empty and would need a default value
                        // (or it was when the default trigger was created) update the
                        // attribute using the default value.
                        if (string.IsNullOrEmpty(_targetAttribute.Value.String) ||
                            dataEntryQuery.UpdatePending)
                        {
                            // If this is a default trigger that is fully resolved, it will
                            // never need to fire again-- clear all triggers.
                            if (dataEntryQuery.GetIsFullyResolved())
                            {
                                dataEntryQuery.Disabled = true;
                                dataEntryQuery.ClearAllTriggers();
                            }
                            else
                            {
                                // If the default query is not fully resolved, flag UpdatePending
                                // to allow it an opportunity to apply its update as query
                                // elements become resolved.
                                dataEntryQuery.UpdatePending = true;
                            }

                            // Apply the default query value.
                            return ApplyQueryResult(queryResult);
                        }
                        else
                        {
                            // If the target attribute can't use a default value, disable and
                            // disarm the query.
                            dataEntryQuery.Disabled = true;
                            dataEntryQuery.ClearAllTriggers();
                            return false;
                        }
                    }
                    // A normal auto-update query- apply the query results (if there were any).
                    else
                    {
                        return ApplyQueryResult(queryResult);
                    }
                }
                else if (dataEntryQuery.DefaultQuery &&
                    string.IsNullOrEmpty(_targetAttribute.Value.String))
                {
                    // If the target attribute could use a default value, but the default query
                    // is not yet resolved, set _updatePending so that it will fire
                    // once it is resolved.
                    dataEntryQuery.UpdatePending = true;
                }

                return false;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26735",
                    "Failed to apply updated value!", ex);
                throw ee;
            }
        }

        /// <summary>
        /// Attempts to apply the specified <see cref="QueryResult"/> to the target
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="queryResult">The <see cref="QueryResult"/> to be applied.</param>
        /// <returns><see langword="true"/> if the <see cref="IAttribute"/> was updated;
        /// <see langword="false"/> otherwise.</returns>
        bool ApplyQueryResult(QueryResult queryResult)
        {
            string stringResult = queryResult.ToString();

            if (!string.IsNullOrEmpty(stringResult) || queryResult.IsSpatial)
            {
                // Update the attribute's value.
                if (queryResult.IsSpatial)
                {
                    AttributeStatusInfo.SetValue(_targetAttribute,
                        queryResult.ToSpatialString(), false, true);
                }
                else
                {
                    AttributeStatusInfo.SetValue(_targetAttribute, stringResult, false, true);
                }

                // After applying the value, direct the control that contains it to
                // refresh the value.
                AttributeStatusInfo.GetOwningControl(_targetAttribute).
                    RefreshAttributes(queryResult.IsSpatial, _targetAttribute);

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
