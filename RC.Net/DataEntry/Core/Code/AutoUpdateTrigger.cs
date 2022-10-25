using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Registers an <see cref="IAttribute"/> for auto-updates using trigger attributes specified
    /// in the provided auto update query.
    /// </summary>
    internal partial class AutoUpdateTrigger : IDisposable
    {
        #region Constants

        /// <summary>
        /// The special query result that should cause the text of a field to be blanked out. (As
        /// opposed to a query result of "" which will not update the field.)
        /// </summary>
        static readonly string _BLANK_VALUE = "[BLANK]";

        #endregion Constants

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
        /// Database(s) for auto-update queries; The key is the connection name (blank for default
        /// connection).
        /// </summary>
        Dictionary<string, DbConnection> _dbConnections;

        /// <summary>
        /// Indicates whether the trigger is to be used to update a validation list instead of the
        /// value itself.
        /// </summary>
        bool _validationTrigger;

        /// <summary>
        /// Indicates whether the trigger is currently updating the target attribute
        /// </summary>
        bool _updatingValue;

        /// <summary>
        /// Indicates whether <see cref="IDisposable.Dispose"/> has been called on this instance.
        /// </summary>
        bool _isDisposed;

        /// <summary>
        /// Keeps track of all queries for which updates were triggered while an
        /// <see cref="AttributeStatusInfo.EndEdit"/> or <see cref="UndoManager"/> undo/redo call is
        /// in progress. These triggers are batched until the end of the operation to prevent
        /// excessive recalculations as various parts of complex queries are updated.
        /// </summary>
        [ThreadStatic]
        static List<Tuple<AutoUpdateTrigger, DataEntryQuery>> _queriesPendingUpdate;

        /// <summary>
        /// Keeps track of the number of time all queries that have been triggered within an active
        /// call to <see cref="ExecuteAllPendingTriggers"/> to prevent them from being endlessly
        /// re-queued for execution within the same call thereby causing an infinite loop.
        /// </summary>
        [ThreadStatic]
        static Dictionary<DataEntryQuery, int> _pendingQueryExecutionCount;

        /// <summary>
        /// Keeps track of whether the <see cref="_queriesPendingUpdate"/> are in the process of
        /// being evaluated. Do not allow for re-entry to <see cref="ExecuteAllPendingTriggers"/>.
        /// </summary>
        [ThreadStatic]
        static bool _executingPendingTriggers;

        /// <summary>
        /// Indicates whether ThreadStatic fields and event handlers have been initialized on
        /// the current thread.
        /// </summary>
        [ThreadStatic]
        static bool _staticsInitialized;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="AutoUpdateTrigger"/> instance.
        /// </summary>
        /// <param name="targetAttribute">The <see cref="IAttribute"/> to be updated using the query.
        /// </param>
        /// <param name="query">. Every time an attribute specified in the query is modified, this
        /// query will be re-evaluated and used to update the value.</param>
        /// <param name="dbConnections">The database(s) to use for SQL query nodes; The key is the
        /// connection name (blank for default connection). (can be <see langword="null"/> if not
        /// required).</param>
        /// <param name="validationTrigger"><see langword="true"/> if the trigger should update the
        /// validation list associated with the <see paramref="targetAttribute"/> instead of the
        /// <see cref="IAttribute"/> value itself; <see langword="false"/> otherwise.</param>
        public AutoUpdateTrigger(IAttribute targetAttribute, string query,
            Dictionary<string, DbConnection> dbConnections, bool validationTrigger)
        {
            try
            {
                ExtractException.Assert("ELI26111", "Null argument exception!",
                    targetAttribute != null);
                ExtractException.Assert("ELI26112", "Null argument exception!", query != null);

                // [DataEntry:1292]
                // https://extract.atlassian.net/browse/ISSUE-13149
                // ThreadStatic fields need to be constructed in every thread rather than with a
                // default constructor (otherwise they will be null in subsequent verification
                // sessions).
                InitializeStatics();

                // Initialize the fields.
                _targetAttribute = targetAttribute;
                _dbConnections = dbConnections;
                _validationTrigger = validationTrigger;

                DataEntryQuery[] dataEntryQueries = DataEntryQuery.CreateList(
                    query, _targetAttribute, _dbConnections,
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

                    // Don't bother registering for query value modifications if this query is
                    // exempted from executing for all updates.
                    if (!dataEntryQuery.ExecutionExemptions
                            .Any(exemption => exemption == ExecutionContext.OnUpdate))
                    {
                        dataEntryQuery.QueryValueModified += HandleQueryValueModified;
                    }
                }

                // Attempt to update the value once the query has been loaded.
                UpdateValue();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26108", ex);
                ee.AddDebugData("Query", query, true);
                throw ee;
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Updates the target attribute's value by executing the query.
        /// </summary>
        /// <returns><see langword="true"/> if the call resulted in the target getting updated;
        /// otherwise, <see langword="false"/>.</returns>
        public bool UpdateValue()
        {
            try
            {
                bool valueUpdated = false;

                // If the attribute's value is empty and a default query has been specified,
                // update the attribute using the default query.
                if (_defaultQuery != null)
                {
                    UpdateValue(_defaultQuery, viaDataUpdate: false);
                }

                // Keep track of which properties have already been updated so that we can prevent
                // multiple auto-update queries from running against the same property.
                var setUpdatedProperties = new HashSet<string>();

                // Attempt an update with all resolved queries.
                foreach (DataEntryQuery query in _queries)
                {
                    // If the query's TargetProperty has already been updated, don't run any other
                    // queries against that property.
                    if (!_validationTrigger &&
                        setUpdatedProperties.Contains(query.TargetProperty))
                    {
                        continue;
                    }

                    if (UpdateValue(query, viaDataUpdate: false))
                    {
                        valueUpdated = true;

                        // If this is not a validation trigger, once the attribute has been updated
                        // a particular property, don't attempt to update the property with any
                        // remaining queries.
                        if (!_validationTrigger)
                        {
                            setUpdatedProperties.Add(query.TargetProperty);
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
            _isDisposed = true;

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
        /// Handles the <see cref="AttributeStatusInfo.EditEnded"/> event of the
        /// <see cref="AttributeStatusInfo"/> class.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        static void HandleAttributeStatusInfo_EditEnded(object sender, EventArgs e)
        {
            try
            {
                // Execute all pending AutoUpdateTriggers that fired during an EndEdit call (unless
                // queries are explicitly paused).
                if (!AttributeStatusInfo.PauseQueries)
                {
                    ExecuteAllPendingTriggers();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36174");
            }
        }

        /// <summary>
        /// Handles the <see cref="UndoManager.OperationEnded"/> event of
        /// <see cref="AttributeStatusInfo"/>'s <see cref="UndoManager"/> instance.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        static void HandleUndoManager_OperationEnded(object sender, EventArgs e)
        {
            try
            {
                // Execute all pending AutoUpdateTriggers that fired during an undo/redo operation.
                ExecuteAllPendingTriggers();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36175");
            }
        }

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.QueryDelayEnded"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        static void HandleQueryDelayEnded(object sender, EventArgs e)
        {
            try
            {
                // Execute all pending AutoUpdateTriggers that fired while a delay was in effect.
                ExecuteAllPendingTriggers();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37379");
            }
        }

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

                    // https://extract.atlassian.net/browse/ISSUE-13506
                    // https://extract.atlassian.net/browse/ISSUE-13538
                    // If we are currently in an ExecuteAllPendingTriggers call, to prevent an
                    // infinite do not re-queue a query that has already been triggered more than
                    // once within the active ExecuteAllPendingTriggers call.
                    // https://extract.atlassian.net/browse/ISSUE-13667
                    // Modified the initial recursion prevention to allow each query to execute
                    // twice. This mirrors similar recursion management code in AttributeStatusInfo
                    // where _attributesBeingModified prevents allows an edited attribute to be
                    // subsequently updated once in addition as a result of a query execution
                    // chain, but not again. Therefor, in the latest implementation here, given:
                    // - Auto-update query 1 on attribute A that formats attribute A
                    // - Auto-update query 2 on attribute B that updates attribute B based on A
                    // - Auto-update query 3 on attribute A that updates attribute A based on A & B
                    // Then query 1 should be allowed to execute once as part of the initial edit of
                    // attribute A, again in reaction to a execution of query 3, then any further
                    // executions of query 1 should be prevented. 
                    int executionCount = 0;
                    if (_pendingQueryExecutionCount != null &&
                        _pendingQueryExecutionCount.TryGetValue(dataEntryQuery, out executionCount) &&
                        executionCount > 1)
                    {
                        // No nothing to prevent the possibility of an infinite loop.
                    }
                    // [DataEntry:1283, 1284, 1289]
                    // If the query was modified during an EndEdit call, during an undo/redo
                    // operation or while processing the backlog of triggers that fired during
                    // either of those calls, postpone the update until all changes are registered.
                    // This prevents excessive recalculations as various parts of complex queries
                    // are updated.
                    else if (AttributeStatusInfo.EndEditInProgress ||
                        AttributeStatusInfo.UndoManager.InUndoOperation ||
                        AttributeStatusInfo.UndoManager.InRedoOperation ||
                        AttributeStatusInfo.PauseQueries ||
                        _queriesPendingUpdate.Count > 0)
                    {
                        var alreadyQueuedQuery = _queriesPendingUpdate.SingleOrDefault(
                            pendingTrigger => pendingTrigger.Item2 == dataEntryQuery);

                        // To ensure computed values are applied before validation is attempted:
                        // - If the same validation trigger is queued up multiple times, only the
                        // last instance should be evaluated so as to be sure all data is final
                        // before determining if valid.
                        // - If the same auto-update trigger is queued up multiple times, only the
                        // first instance should be evaluated.
                        if (_validationTrigger && alreadyQueuedQuery != null)
                        {
                            _queriesPendingUpdate.Remove(alreadyQueuedQuery);
                            alreadyQueuedQuery = null;
                        }

                        if (alreadyQueuedQuery == null)
                        {
                            _queriesPendingUpdate.Add(
                                new Tuple<AutoUpdateTrigger, DataEntryQuery>(this, dataEntryQuery));
                        }
                    }
                    else
                    {
                        // Always ensure a default query applies updates as part of a full
                        // auto-update trigger to ensure normal auto-update triggers can apply
                        // their updates on top of any default value.
                        if (dataEntryQuery.DefaultQuery)
                        {
                            UpdateValue();
                        }
                        // If not a default auto-update trigger, update using only the modified
                        // query, not the entire trigger.
                        else
                        {
                            UpdateValue(dataEntryQuery, viaDataUpdate: true);
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

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Ensures all ThreadStatic fields and event handlers are initialized.
        /// </summary>
        static void InitializeStatics()
        {
            if (!_staticsInitialized)
            {
                AttributeStatusInfo.EditEnded += HandleAttributeStatusInfo_EditEnded;
                AttributeStatusInfo.UndoManager.OperationEnded += HandleUndoManager_OperationEnded;
                AttributeStatusInfo.QueryDelayEnded += HandleQueryDelayEnded;

                _queriesPendingUpdate = new List<Tuple<AutoUpdateTrigger, DataEntryQuery>>();

                _staticsInitialized = true;
            }
        }

        /// <summary>
        /// Attempts to update the target <see cref="IAttribute"/> using the result of the
        /// evaluated query.
        /// </summary>
        /// <param name="dataEntryQuery">The <see cref="DataEntryQuery"/> that should be used to
        /// update the target attribute.</param>
        /// <param name="viaDataUpdate"><c>true</c> if the value is being updated in response to a
        /// referenced data value that has updated; <c>false</c> for an otherwise commanded execution
        /// of an attribute's queries.</param>
        /// <returns><see langword="true"/> if the target attribute was updated;
        /// <see langword="false"/> otherwise.</returns>
        bool UpdateValue(DataEntryQuery dataEntryQuery, bool viaDataUpdate)
        {
            QueryResult queryResult = null;

            try
            {
                // Don't evaluate disabled queries or validation triggers if validation triggers are
                // not enabled.
                // [DataEntry:1186]
                // ... or auto-update queries that are targeting the control's text (and not another property)
                // if auto-update queries are blocked.
                // [DataEntry:1271]
                // ... or attributes that are not initialized (in the process of being deleted).
                if (AttributeStatusInfo.ThreadEnding || dataEntryQuery.Disabled ||
                    !AttributeStatusInfo.GetStatusInfo(_targetAttribute).IsInitialized ||
                    (_validationTrigger && !AttributeStatusInfo.ValidationTriggersEnabled) ||
                    (!_validationTrigger && dataEntryQuery.TargetProperty == null &&
                        AttributeStatusInfo.BlockAutoUpdateQueries))
                {
                    return false;
                }

                // https://extract.atlassian.net/browse/ISSUE-15342
                if (IsExecutionExempted(dataEntryQuery, viaDataUpdate))
                {
                    return false;
                }

                // Prevent recursion via HandleQueryValueModified.
                _updatingValue = true;

                if (AttributeStatusInfo.IsLoggingEnabled(
                    _validationTrigger ? LogCategories.ValidationQuery : LogCategories.AutoUpdateQuery))
                {
                    AttributeStatusInfo.Logger.LogEvent(
                        _validationTrigger ? LogCategories.ValidationQuery : LogCategories.AutoUpdateQuery,
                        _targetAttribute, dataEntryQuery.QueryText);
                }

                // Evaluate the query.
                queryResult = dataEntryQuery.Evaluate();

                var loggingCategories = _validationTrigger
                    ? new[]
                        {
                            LogCategories.ValidationResult,
                            LogCategories.ValidationResultAbridged
                        }
                    : new[] { LogCategories.AutoUpdateResult };

                var loggingCategory = loggingCategories
                    .FirstOrDefault(cat => AttributeStatusInfo.IsLoggingEnabled(cat));

                if (loggingCategory != LogCategories.None)
                {
                    string[] queryResultArray = queryResult.ToStringArray();
                    if (loggingCategory == LogCategories.ValidationResultAbridged
                        && queryResultArray.Length != 1)
                    {
                        queryResultArray = new[]
                        {
                            "[RESULT_COUNT] = " + queryResultArray.Length.ToString(CultureInfo.InvariantCulture)
                        };
                    }

                    AttributeStatusInfo.Logger.LogEvent(loggingCategory, _targetAttribute, queryResultArray);
                }

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
                    if (dataEntryQuery.ValidationListType != ValidationListType.ValidationListOnly &&
                        dataEntryQuery.ValidValue == null)
                    {
                        string[][] queryResultArray = queryResult.ToArrayOfStringArrays();
                        validator.SetAutoCompleteValues(queryResultArray);
                    }

                    // Initialize the validation query which will determine if an attribute is
                    // valid.
                    if (dataEntryQuery.ValidationListType != ValidationListType.AutoCompleteOnly)
                    {
                        validator.SetValidationQuery(dataEntryQuery, queryResult);
                    }

                    // Because some fields may not be able to accept the value before being entirely
                    // initialized (i.e., combo box fields), if a backup value was stored for this
                    // attribute and the current value of the attribute does not match the backup
                    // value, restore the backup value. 
                    if (statusInfo.LastAppliedStringValue != null &&
                        _targetAttribute.Value.String != statusInfo.LastAppliedStringValue)
                    {
                        string lastAppliedStringValue = statusInfo.LastAppliedStringValue;

                        // https://extract.atlassian.net/browse/ISSUE-13506
                        // To prevent the possibility of an infinite loop in the query system, set
                        // LastAppliedStringValue to null before enforcing it within any postponed
                        // query.
                        if (_executingPendingTriggers)
                        {
                            statusInfo.LastAppliedStringValue = null;
                        }

                        AttributeStatusInfo.SetValue(_targetAttribute, lastAppliedStringValue, false, true);
                    }

                    // https://extract.atlassian.net/browse/ISSUE-12813
                    // Calling RefreshAttributes on the control will not necessarily trigger
                    // validation to occur in the case that the attribute is not currently loaded in
                    // the control (i.e., the ParentDataEntryControl does not currently have this
                    // attribute's parent selected).
                    // I investigated the potential performance hit of this call since in most
                    // cases validation will also be triggered elsewhere (including by the
                    // subsequent RefreshAttributes call). However, needless query re-evaluation
                    // is prevented via CompositeQueryNode.CachedResult and
                    // AttributeStatusInfo.ValidationStateChanged is raised only when the validation
                    // state actually changes, so extra calls to validate have very little added
                    // cost.
                    AttributeStatusInfo.Validate(_targetAttribute, false);

                    statusInfo.OwningControl?.RefreshAttributes(false, _targetAttribute);

                    return true;
                }
                // If this auto-update query should only provide a default value
                else if (dataEntryQuery.DefaultQuery)
                {
                    // A default trigger will never need to fire again-- disable and disarm the
                    // query.
                    dataEntryQuery.Disabled = true;
                    if (!dataEntryQuery.ExecutionExemptions
                        .Any(exemption => exemption == ExecutionContext.OnUpdate))
                    {
                        dataEntryQuery.QueryValueModified -= HandleQueryValueModified;
                    }

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
                // If the query is targeting a different property of the control (rather than that
                // which represents the attribute's value).
                else if (!string.IsNullOrWhiteSpace(dataEntryQuery.TargetProperty))
                {
                    return ApplyQueryResult(queryResult, dataEntryQuery.TargetProperty);
                }
                // Otherwise a normal auto-update query; apply the query result to the attribute
                // value.
                else
                {
                    return ApplyQueryResult(queryResult);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26735",
                    "Failed to apply updated value!", ex);
                try
                {
                    ee.AddDebugData("Query", dataEntryQuery.QueryText, true);
                    ee.AddDebugData("Validation", _validationTrigger ? "True" : "False", false);
                    ee.AddDebugData("Target", _targetAttribute.Name, false);
                    ee.AddDebugData("Result", (queryResult == null) 
                        ? "<Not Computed>"
                        : queryResult.ToString(), false);
                }
                catch {}

                throw ee;
            }
            finally
            {
                _updatingValue = false;
            }
        }

        /// <summary>
        /// Determines whether execution of the specified <see cref="dataEntryQuery"/> should be
        /// exempted under the current circumstances.
        /// </summary>
        /// <param name="dataEntryQuery">The <see cref="DataEntryQuery"/> to consider for exemption.
        /// </param>
        /// <param name="viaDataUpdate"><c>true</c> if the current check is being run in response to a
        /// referenced data value that has updated; <c>false</c> for an otherwise commanded execution
        /// of an attribute's queries.</param>
        /// <returns><c>true</c> if execution of the query is exempted; <c>false</c> if the query
        /// should be executed.</returns>
        bool IsExecutionExempted(DataEntryQuery dataEntryQuery, bool viaDataUpdate)
        {
            var executionContext = AttributeStatusInfo.QueryExecutionContext;
            if (AttributeStatusInfo.QueryExecutionContext == ExecutionContext.OnUpdate)
            {
                executionContext = viaDataUpdate
                    ? ExecutionContext.OnUpdate
                    : ExecutionContext.OnCreate;
            }

            executionContext |= string.IsNullOrWhiteSpace(_targetAttribute?.Value.String)
                ? ExecutionContext.WhenEmpty
                : ExecutionContext.WhenPopulated;

            foreach (var exemption in dataEntryQuery.ExecutionExemptions)
            {
                if ((exemption & executionContext) == exemption)
                {
                    return true;
                }
            }

            return false;
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
                bool isBlank = string.Equals(stringResult, _BLANK_VALUE,
                    StringComparison.CurrentCultureIgnoreCase);

                if (isBlank)
                {
                    stringResult = "";
                }

                // Update the attribute's value.
                if (queryResult.IsSpatial)
                {
                    SpatialString spatialString = queryResult.ToSpatialString();
                    if (isBlank)
                    {
                        spatialString.ReplaceAndDowngradeToHybrid("");
                    }

                    AttributeStatusInfo.SetValue(_targetAttribute, spatialString, false, true);
                }
                else
                {
                    AttributeStatusInfo.SetValue(_targetAttribute, stringResult, false, true);
                }

                // After applying the value, direct the control that contains it to
                // refresh the value.
                AttributeStatusInfo.GetOwningControl(_targetAttribute)?.
                    RefreshAttributes(queryResult.IsSpatial, _targetAttribute);

                // https://extract.atlassian.net/browse/ISSUE-13506
                // Only set LastAppliedStringValue in the case where the owning control did not
                // properly accept the new query result.
                var statusInfo = AttributeStatusInfo.GetStatusInfo(_targetAttribute);
                if (_targetAttribute.Value.String != stringResult && statusInfo.OwningControl != null)
                {
                    // Keep track of programmatically applied values, in case the field control isn't
                    // yet prepared to accept the value. (i.e. combo box whose item list has not yet
                    // been updated/initialized)
                    statusInfo.LastAppliedStringValue = stringResult;
                }
                else
                {
                    // https://extract.atlassian.net/browse/ISSUE-13975
                    // If the owning control did accept the value, we should not be keeping around
                    // any previous LastAppliedStringValue as this new value may be intentionally
                    // updated via an auto-update query.
                    statusInfo.LastAppliedStringValue = null;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to apply the specified <see cref="QueryResult"/> to the
        /// <see paramref="propertyName"/> of the UI element representing the target
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="queryResult">The <see cref="QueryResult"/> to be applied.</param>
        /// <param name="propertyName">Then name of the property to apply the value to. Can be a
        /// nested property such as "OwningColumn.Width".</param>
        /// <returns><see langword="true"/> if the control's property was updated;
        /// <see langword="false"/> otherwise.</returns>
        bool ApplyQueryResult(QueryResult queryResult, string propertyName)
        {
            string stringResult = queryResult.ToString();

            if (!string.IsNullOrEmpty(stringResult))
            {
                // Check if the query result is the special value to indicate the value be cleared.
                if (string.Equals(stringResult, _BLANK_VALUE,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    stringResult = "";
                }

                AttributeStatusInfo.SetPropertyValue(_targetAttribute, propertyName, stringResult);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Executes all <see cref="AutoUpdateTrigger"/>s that were triggered and postponed while an
        /// <see cref="AttributeStatusInfo.EndEdit"/> or <see cref="UndoManager"/> undo/redo call
        /// was in progress.
        /// </summary>
        static void ExecuteAllPendingTriggers()
        {
            if (_executingPendingTriggers)
            {
                // https://extract.atlassian.net/browse/ISSUE-12549
                // With the change to EndEdit mad for this issue, recursion into this method should
                // no longer occur. However, it doesn't hurt to keep this check in place.
                return;
            }

            try
            {
                _executingPendingTriggers = true;
                _pendingQueryExecutionCount = new Dictionary<DataEntryQuery, int>();

                // [DataEntry:1292]
                // https://extract.atlassian.net/browse/ISSUE-13149
                // ThreadStatic fields need to be constructed in every thread rather than with a
                // default constructor (otherwise they will be null in subsequent verification
                // sessions).
                InitializeStatics();

                while (_queriesPendingUpdate.Count > 0)
                {
                    var pendingTrigger = _queriesPendingUpdate.First();
                    _queriesPendingUpdate.RemoveAt(0);                    
                    
                    // Ensure that a field and/or it's autoupdate trigger(s) haven't been deleted or
                    // disposed of since the query modification occurred.
                    AutoUpdateTrigger autoUpdateQuery = pendingTrigger.Item1;
                    if (autoUpdateQuery._isDisposed ||
                        !AttributeStatusInfo.GetStatusInfo(autoUpdateQuery._targetAttribute).IsInitialized)
                    {
                        continue;
                    }

                    DataEntryQuery dataEntryQuery = pendingTrigger.Item2;
                    if (_pendingQueryExecutionCount.ContainsKey(dataEntryQuery))
                    {
                        _pendingQueryExecutionCount[dataEntryQuery] =
                            _pendingQueryExecutionCount[dataEntryQuery] + 1;
                    }
                    else
                    {
                        _pendingQueryExecutionCount[dataEntryQuery] = 1;
                    }

                    // Always ensure a default query applies updates as part of a full
                    // auto-update trigger to ensure normal auto-update triggers can apply
                    // their updates on top of any default value.
                    if (dataEntryQuery.DefaultQuery)
                    {
                        autoUpdateQuery.UpdateValue();
                    }
                    else
                    {
                        autoUpdateQuery.UpdateValue(dataEntryQuery, viaDataUpdate: true);
                    }
                }
            }
            finally
            {
                // Forget all LastAppliedStringValues that are currently being remembered to ensure
                // that they don't get used later on after the value has been changed to something
                // else.
                AttributeStatusInfo.ForgetLastAppliedStringValues();

                _executingPendingTriggers = false;
                _pendingQueryExecutionCount = null;
            }
        }

        #endregion Private Members
    }
}