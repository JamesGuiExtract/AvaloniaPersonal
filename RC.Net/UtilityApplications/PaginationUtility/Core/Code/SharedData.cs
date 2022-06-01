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
        public void SaveValues(IEnumerable<QueryResult> queryResults)
        {
            try
            {
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

                    string previousValue = null;
                    if (_values.TryGetValue(attributeId, out var existingValue))
                    {
                        previousValue = existingValue.previous;
                    }

                    _values[attributeId] = (value, previousValue);
                }

                foreach (var id in idsToDelete)
                {
                    _values.Remove(id);
                }
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

        public override bool Equals(object obj)
        {
            return obj is SharedDataField field &&
                   Name == field.Name &&
                   IsUpdated == field.IsUpdated &&
                   _values.OrderBy(kv => kv.Key).SequenceEqual(field._values.OrderBy(kv => kv.Key));
        }

        public override int GetHashCode()
        {
            return HashCode.Start
                .Hash(Name)
                .Hash(IsUpdated);
        }

        public static bool operator ==(SharedDataField left, SharedDataField right)
        {
            return EqualityComparer<SharedDataField>.Default.Equals(left, right);
        }

        public static bool operator !=(SharedDataField left, SharedDataField right)
        {
            return !(left == right);
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
        bool _isUpdated;
        bool _selected;

        public SharedData(Guid documentId)
        {
            DocumentId = documentId;
        }

        public SharedData(SharedData sharedData)
        {
            try
            {
                DocumentId = sharedData.DocumentId;
                Selected = sharedData.Selected;
                _isDeleted = sharedData.IsDeleted;
                _isUpdated = sharedData.IsUpdated;

                _dictionary = sharedData._dictionary.ToDictionary(
                    field => field.Key,
                    field => new SharedDataField(field.Value));
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
                            _isUpdated = true;
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
        public bool IsUpdated => _isUpdated || _dictionary.Values.Any(value => value.IsUpdated);

        /// <summary>
        /// Gets whether the associated document is selected to be committed.
        /// </summary>
        public bool Selected
        {
            get => _selected;
            set
            {
                if (value != _selected)
                {
                    _isUpdated = true;
                    _selected = value;
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

                    sharedDataField.SaveValues(query.SelectMany(q => q.ToList()));
                }

                foreach (var fieldName in fieldsToDelete)
                {
                    _dictionary.Remove(fieldName);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53431");
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
                _isUpdated = false;

                foreach (var field in _dictionary.Values)
                {
                    field.ResetModifiedStatus();
                }
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

        public override bool Equals(object obj)
        {
            return obj is SharedData data &&
                   _isDeleted == data._isDeleted &&
                   _isUpdated == data._isUpdated &&
                   _selected == data._selected &&
                   DocumentId == data.DocumentId &&
                   _dictionary.OrderBy(kv => kv.Key).SequenceEqual(data._dictionary.OrderBy(kv => kv.Key));
        }

        public override int GetHashCode()
        {
            return HashCode.Start
                .Hash(_isDeleted)
                .Hash(_isUpdated)
                .Hash(_selected)
                .Hash(DocumentId);
        }

        public static bool operator ==(SharedData left, SharedData right)
        {
            return EqualityComparer<SharedData>.Default.Equals(left, right);
        }

        public static bool operator !=(SharedData left, SharedData right)
        {
            return !(left == right);
        }
    }
}
