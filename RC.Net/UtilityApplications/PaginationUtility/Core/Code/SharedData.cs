using Extract.DataEntry;
using Extract.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UCLID_COMUTILSLib;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Represents zero one or more values to be shared with other documents under a specified
    /// field name.
    /// </summary>
    public class SharedDataField
    {
        // The special query result that should cause the text of a field to be blanked out. (As
        // opposed to a query result of "" which will not update the field.)
        static readonly string _BLANK_VALUE = "[BLANK]";

        // Maps the ID of the attribute supplying the field value to the current and previous
        // value of the field. The Guid will be Guid.Empty in the case the value is not associated
        // with a specific attribute.
        Dictionary<Guid, (string current, string previous)> _values = new();

        public SharedDataField(string name)
        {
            Name = name;
        }

        public SharedDataField(SharedDataField sharedDataField)
        {
            Name = sharedDataField.Name;
            _values = new Dictionary<Guid, (string current, string previous)>(sharedDataField._values);
        }

        /// <summary>
        /// The name of field as presented to other documents in a SharedData intance.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Indicates that at least one value for this field has changed.
        /// </summary>
        public bool IsUpdated => _values.Any(instance => 
            !string.Equals(instance.Value.current, instance.Value.previous));

        /// <summary>
        /// Gets all current values for this field
        /// </summary>
        public ReadOnlyCollection<string> Values
            => _values.Values.Select(value => value.current)
                .Where(value => value != null)
                .ToList().AsReadOnly();

        /// <summary>
        /// Gets all values for this field as of the last time ResetModifiedStatus was called.
        /// This allows for checks that a shared field that had affected another document no longer does.
        /// NOTE: While this property is not called within this project, it should be available for
        /// DEP implementations to leverage in IsSharedDataUpdateRequired or CanDocumentBeSaved overrides.
        /// </summary>
        public ReadOnlyCollection<string> PreviousValues
            => _values.Values.Select(value => value.previous)
                .Where(previousValue => previousValue != null)
                .ToList().AsReadOnly();

        /// <summary>
        /// Applies the specified queryResults as new values for this field.
        /// </summary>
        public bool SaveValues(IEnumerable<QueryResult> queryResults)
        {
            try
            {
                bool valueModified = false;

                HashSet<Guid> idsToDelete = new(_values.Keys);

                foreach (var result in queryResults)
                {
                    Guid attributeId = result.IsAttribute
                        ? ((IIdentifiableObject)result.FirstAttribute).InstanceGUID
                        : Guid.Empty;
                    string value = result.FirstString;

                    idsToDelete.Remove(attributeId);

                    if (value == _BLANK_VALUE)
                    {
                        value = "";
                    }

                    if (!_values.TryGetValue(attributeId, out var existingValue))
                    {
                        existingValue = (null, null);
                    }

                    if (value != existingValue.current)
                    {
                        // ResetModificationStatus updates previous to = current. In case of multiple
                        // updates before the next call to reset, previous should remain the same as
                        // it was last call to reset (rather to the value of the last immediate update)
                        _values[attributeId] = (value, existingValue.previous);
                        valueModified = true;
                    }
                }

                foreach (var id in idsToDelete.Where(id => _values[id].current != null))
                {
                    _values[id] = (null, _values[id].previous);
                    valueModified = true;
                }

                return valueModified;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53466");
            }
        }

        /// <summary>
        /// Once any changed values are accounted for, resets the previous values to match the current fields
        /// IsUpdated will be false after making this call.
        /// </summary>
        public void ResetModifiedStatus()
        {
            try
            {
                _values = _values.ToDictionary(
                    item => item.Key
                    , item => (item.Value.current, item.Value.current));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53465");
            }
        }
    }

    /// <summary>
    /// A collection of SharedDataFields representing all the data for a specific document
    /// to be shared with other documents loaded into the pagination UI.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class SharedData : IEnumerable<SharedDataField>
    {
        public const string AttributeName = "_SharedData";

        // Maps each field name to the associated SharedDataField
        Dictionary<string, SharedDataField> _dictionary = new();

        bool _isDeleted;
        bool _isSelected;
        bool _isDocumentStateChanged;
        bool _isFieldChanged;
        int _lastFieldRevisionNumber;

        public SharedData(Guid documentId)
        {
            DocumentId = documentId;
        }

        public SharedData(SharedData sharedData)
        {
            try
            {
                DocumentId = sharedData.DocumentId;
                _isSelected = sharedData.Selected;
                _isDeleted = sharedData.IsDeleted;
                _isDocumentStateChanged = sharedData._isDocumentStateChanged;

                CopyFieldValues(sharedData, copyFieldChangedStatus: true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53404");
            }
        }

        /// <summary>
        /// True if the document associated with this instance has been deleted
        /// </summary>
        public bool IsDeleted
        {
            get => _isDeleted;
            set
            {
                try
                {
                    if (value != _isDeleted)
                    {
                        _isDeleted = value;
                        if (_dictionary.Count > 0)
                        {
                            _isDocumentStateChanged = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI53480");
                }
            }
        }

        /// <summary>
        /// True any any updates have been made to this instance since the last call to
        /// ResetModifiedStatus. This includes changes to field values as well as deletion and
        /// selection status.
        /// </summary>
        public bool IsUpdated => _isDocumentStateChanged || _isFieldChanged;

        /// <summary>
        /// Indicates the number of cycles in which field values have been updated followed
        /// by a subsequence call to ResetModifiedStatus
        /// </summary>
        public int FieldRevisionNumber => _lastFieldRevisionNumber + (_isFieldChanged ? 1 : 0);

        /// <summary>
        /// Gets whether the associated document is selected to be committed.
        /// </summary>
        public bool Selected
        {
            get => _isSelected;
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    _isDocumentStateChanged = true;
                }
            }
        }

        /// <summary>
        /// A unique ID for the document this instance represents; Can be used to associate this
        /// instance with a <see cref="PaginationDocumentData"/> instance.
        /// </summary>
        public Guid DocumentId { get; }

        /// <summary>
        /// Provides access to a SharedDataField by name.
        /// NOTE: While this property is not called within this project, it should be available for
        /// DEP implementations to leverage in IsSharedDataUpdateRequired or CanDocumentBeSaved overrides.
        /// </summary>
        public SharedDataField this[string fieldName] =>
            _dictionary.TryGetValue(fieldName, out var values)
                ? values
                : new SharedDataField(fieldName);

        /// <summary>
        /// Creates or updates the fields in this based on execution of the provided DataEntryQueries.
        /// </summary>
        public void Update(IEnumerable<DataEntryQuery> queries)
        {
            try
            {
                HashSet<string> fieldsToDelete = new(_dictionary.Keys);

                foreach (var query in queries
                    .GroupBy(
                            query => query.Name,
                            query => query.Evaluate().SelectMany(results => results)))
                {
                    fieldsToDelete.Remove(query.Key);

                    var sharedDataField =
                         _dictionary.GetOrAdd(query.Key, (name) => new SharedDataField(name));

                    if (sharedDataField.SaveValues(query.SelectMany(q => q.ToList())))
                    {
                        _isFieldChanged = true;
                    }
                }

                foreach (var fieldName in fieldsToDelete)
                {
                    _dictionary.Remove(fieldName);
                    _isFieldChanged = true;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53431");
            }
        }

        /// <summary>
        /// Copies SharedDataField values from the specified source (without affecting document
        /// state values of the current instance)
        /// </summary>
        /// <param name="copyFieldChangedStatus">true to copy the field modification status
        /// which will ultimately impact whether <see cref="FieldRevisionNumber"/> is incremented.</param>
        public void CopyFieldValues(SharedData source, bool copyFieldChangedStatus)
        {
            try
            {
                _dictionary = source._dictionary.ToDictionary(
                    field => field.Key,
                    field => new SharedDataField(field.Value));

                _isFieldChanged = source._isFieldChanged && copyFieldChangedStatus;
                _lastFieldRevisionNumber = source._lastFieldRevisionNumber; 
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53489");
            }
        }

        /// <summary>
        /// Once any updates in this instance are accounted for, reset the status of changes to any fields
        /// as well as deletion and selection. IsUpdated will be false after making this call.
        /// </summary>
        public void ResetModifiedStatus()
        {
            try
            {
                if (_isFieldChanged)
                {
                    foreach (var field in _dictionary.Values)
                    {
                        field.ResetModifiedStatus();
                    }
                    
                    _lastFieldRevisionNumber++;
                    _isFieldChanged = false;
                }

                _isDocumentStateChanged = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53467");
            }
        }

        public IEnumerator<SharedDataField> GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
        }
    }
}
