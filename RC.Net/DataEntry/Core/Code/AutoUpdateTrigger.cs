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
        IAttribute _targetAttribute;

        /// <summary>
        /// The query divided into terms.
        /// </summary>
        List<DataEntryQuery> _queries = new List<DataEntryQuery>();

        /// <summary>
        /// An auto-update query to be applied if the target attribute otherwise does not have any
        /// value.
        /// </summary>
        DataEntryQuery _defaultQuery;

        /// <summary>
        /// A database for auto-update queries.
        /// </summary>
        DbConnection _dbConnection;

        /// <summary>
        /// Indicates whether the trigger is to be used to update a validation list instead of the
        /// value itself.
        /// </summary>
        bool _validationTrigger;

        /// <summary>
        /// Indicates whether the trigger is currently updating the target attribute
        /// </summary>
        bool _updatingValue;

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

                DataEntryQuery[] dataEntryQueries = DataEntryQuery.CreateList(
                    query, _targetAttribute, _dbConnection,
                    _validationTrigger
                        ? MultipleQueryResultSelectionMode.List
                        : MultipleQueryResultSelectionMode.None);

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

                    dataEntryQuery.QueryValueModified += HandleQueryValueModified;
                }

                // Attempt to update the value once the query has been loaded.
                if (!_validationTrigger || AttributeStatusInfo.ValidationTriggersEnabled)
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

        #region Methods

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
                if (_defaultQuery != null)
                {
                    UpdateValue(_defaultQuery);
                }

                // Attempt an update with all resolved queries.
                foreach (DataEntryQuery query in _queries)
                {
                    if (UpdateValue(query))
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
        /// Handles the case that data behind the query has been modified thereby likely changing
        /// the result. The query needs to be re-evaluated and applied to the target attribute as
        /// appropriate.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="QueryValueModifiedEventArgs"/> that contains the
        /// event data.</param>
        void HandleQueryValueModified(object sender, QueryValueModifiedEventArgs e)
        {
            try
            {
                // Don't do anything for an incremental update or if already updating a value for
                // this trigger (to prevent recursion).
                if (!e.IncrementalUpdate && !_updatingValue)
                {
                    DataEntryQuery dataEntryQuery = (DataEntryQuery)sender;

                    // Always ensure a default query applies updates as part of a full
                    // auto-update trigger to ensure normal auto-update triggers can apply
                    // their updates on top of any default value.
                    if (dataEntryQuery.DefaultQuery)
                    {
                        UpdateValue();
                    }
                    // If not a default auto-update trigger, update using only the modified query,
                    // not the entire trigger.
                    else
                    {
                        UpdateValue(dataEntryQuery);
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
                // Don't evaluate disable queries or validation triggers if validation triggers are
                // not enabled.
                if (dataEntryQuery.Disabled ||
                    (_validationTrigger && !AttributeStatusInfo.ValidationTriggersEnabled))
                {
                    return false;
                }

                // Prevent recursion via HandleQueryValueModified.
                _updatingValue = true;

                // Evaluate the query.
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
                    // A default trigger will never need to fire again-- disable and disarm the
                    // query.
                    dataEntryQuery.Disabled = true;
                    dataEntryQuery.QueryValueModified -= HandleQueryValueModified;

                    // If the target attribute is empty and would need a default value (or it was
                    // when the default trigger was created), apply the default query value.
                    if (string.IsNullOrEmpty(_targetAttribute.Value.String))
                    {
                        return ApplyQueryResult(queryResult);
                    }
                    else
                    {
                        // The target attribute can't use a default value, 
                        dataEntryQuery.Disabled = true;
                        return false;
                    }
                }
                // A normal auto-update query- apply the query results (if there were any).
                else
                {
                    return ApplyQueryResult(queryResult);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26735",
                    "Failed to apply updated value!", ex);
                throw ee;
            }
            finally
            {
                _updatingValue = false;
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