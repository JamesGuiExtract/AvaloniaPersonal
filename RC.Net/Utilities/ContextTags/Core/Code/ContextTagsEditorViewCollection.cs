using Extract.Licensing;
using Extract.Utilities.ContextTags.SqliteModels.Version3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Utilities.ContextTags
{
    /// <summary>
    /// Defines an editable view of a a <see cref="CustomTagsDB"/> (CustomTags.sqlite) database
    /// where the defined contexts comprise the columns and the custom tags comprise the rows.
    /// </summary>
    public class ContextTagsEditorViewCollection : BindingSource, IList<ContextTagsEditorViewRow>
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ContextTagsEditorViewCollection).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Tracks the <see cref="CustomTagsDB"/> currently associated with this class type on
        /// the current thread.
        /// </summary>
        readonly CustomTagsDB _database;

        /// <summary>
        /// A default instance of <see cref="ContextTagsEditorViewRow"/> used to describe the
        /// available properties for the collection.
        /// </summary>
        ContextTagsEditorViewRow _defaultViewRow;

        /// <summary>
        /// The <see cref="ContextTagsEditorViewRow"/>s comprising the collection.
        /// </summary>
        readonly ObservableCollection<ContextTagsEditorViewRow> _contextRows = new();

        /// <summary>
        /// Flag to indicate that data is being loaded from the database
        /// </summary>
        bool _dataLoading;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Workflow that is currently active
        /// </summary>
        public string ActiveWorkflow { get; set; }

        #endregion Properties
        
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagsEditorViewCollection"/> class.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="workflow">The workflow to use</param>
        public ContextTagsEditorViewCollection(CustomTagsDB database, string workflow)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI38002",
                    _OBJECT_NAME);

                _database = database;
                ActiveWorkflow = workflow;

                _defaultViewRow = new ContextTagsEditorViewRow(database, null, ActiveWorkflow);

                // Handle any adds/deletes from _contextRows to be able to initialize or delete
                // corresponding data from the database.
                _contextRows.CollectionChanged += HandleContextRows_CollectionChanged;

                DataSource = _contextRows;

                LoadData();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38003");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when data has been changed.
        /// </summary>
        public event EventHandler<EventArgs> DataChanged;

        #endregion Events

        #region Methods

        /// <summary>
        /// Refreshes the properties and data from the database.
        /// </summary>
        public void Refresh()
        {
            try
            {
	            Clear();

                LoadData();

                // Refresh data from the database
                CurrencyManager.Refresh();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38004");
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Removes all rows from the collection.
        /// </summary>
        public override void Clear()
        {
            try
            {
                _contextRows.Clear();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38005");
            }
        }

        /// <summary>
        /// Retrieves an array of <see cref="PropertyDescriptor"/> objects representing the
        /// bindable properties of the data source list type.
        /// </summary>
        /// <param name="listAccessors">An array of <see cref="PropertyDescriptor"/> objects to
        /// find in the list as bindable.</param>
        /// <returns>
        /// An array of <see cref="System.ComponentModel.PropertyDescriptor"/> objects that
        /// represents the properties on this list type used to bind data.
        /// </returns>
        public override PropertyDescriptorCollection GetItemProperties(
            PropertyDescriptor[] listAccessors)
        {
            try
            {
                return new PropertyDescriptorCollection(_defaultViewRow.GetProperties()
                    .Cast<PropertyDescriptor>()
                    .Where(property => listAccessors == null ||
                        listAccessors.Any(accessor => accessor.PropertyType == property.PropertyType))
                    .ToArray());
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38006");
            }
        }

        #endregion Overrides

        #region IList<ContextTagsEditorView> Members

        /// <summary>
        /// Determines the index of a specific item in the <see cref="IList`1"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="IList`1"/>.</param>
        /// <returns>
        /// The index of item if found in the list; otherwise, -1.
        /// </returns>
        int IList<ContextTagsEditorViewRow>.IndexOf(ContextTagsEditorViewRow item)
        {
            try
            {
                return _contextRows.IndexOf(item);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38007");
            }
        }

        /// <summary>
        /// Inserts an item to the <see cref="IList`1"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="IList`1"/>.</param>
        void IList<ContextTagsEditorViewRow>.Insert(int index, ContextTagsEditorViewRow item)
        {
            try
            {
                _contextRows.Insert(index, item);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38008");
            }
        }

        /// <summary>
        /// Removes the item at the specified index in the list.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        void IList<ContextTagsEditorViewRow>.RemoveAt(int index)
        {
            try
            {
                _contextRows.RemoveAt(index);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38009");
            }
        }

        /// <summary>
        /// Gets or sets the list element at the specified index.
        /// </summary>
        /// <returns>The element at the specified index.</returns>
        ContextTagsEditorViewRow IList<ContextTagsEditorViewRow>.this[int index]
        {
            get
            {
                try
                {
                    return _contextRows[index];
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38010");
                }
            }
            set
            {
                try
                {
                    _contextRows[index] = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38011");
                }
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ICollection`1"/>.</param>
        void ICollection<ContextTagsEditorViewRow>.Add(ContextTagsEditorViewRow item)
        {
            try
            {
                ContextTagsEditorViewRow.ActiveWorkflow = ActiveWorkflow;
                _contextRows.Add(item);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38012");
            }
        }

        /// <summary>
        /// Removes all elements from the list.
        /// </summary>
        void ICollection<ContextTagsEditorViewRow>.Clear()
        {
            try
            {
                _contextRows.Clear();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38013");
            }
        }

        /// <summary>
        /// Determines whether the <see cref="ICollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ICollection`1"/>.</param>
        /// <returns>
        /// <see langword="true"/> if item is found in the <see cref="ICollection`1"/>; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        bool ICollection<ContextTagsEditorViewRow>.Contains(ContextTagsEditorViewRow item)
        {
            try
            {
                return _contextRows.Contains(item);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38014");
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection`1"/> to an
        /// <see cref="ContextTagsEditorViewRow"/> array, starting at a particular index.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        void ICollection<ContextTagsEditorViewRow>.CopyTo(ContextTagsEditorViewRow[] array,
            int arrayIndex)
        {
            try
            {
                _contextRows.CopyTo(array, arrayIndex);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38015");
            }
        }

        /// <summary>
        /// Gets the total number of items in the underlying list, taking the current
        /// <see cref="P:BindingSource.Filter"/> value into consideration.
        /// </summary>
        /// <returns>The total number of filtered items in the underlying list.</returns>
        int ICollection<ContextTagsEditorViewRow>.Count
        {
            get
            {
                try
                {
                    return _contextRows.Count;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38016");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the underlying list is read-only.
        /// </summary>
        /// <returns><see langword="true"/> if the list is read-only; otherwise,
        /// <see langword="false"/>.</returns>
        bool ICollection<ContextTagsEditorViewRow>.IsReadOnly
        {
            get
            {
                return IsReadOnly;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ICollection`1"/>.</param>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed from the
        /// <see cref="ICollection`1"/>; otherwise, <see langword="false"/>. This method also
        /// returns <see langword="false"/> if item is not found in the original
        /// <see cref="ICollection`1"/>.
        /// </returns>
        bool ICollection<ContextTagsEditorViewRow>.Remove(ContextTagsEditorViewRow item)
        {
            try
            {
                return _contextRows.Remove(item);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38017");
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<ContextTagsEditorViewRow> IEnumerable<ContextTagsEditorViewRow>.GetEnumerator()
        {
            try
            {
                return _contextRows.GetEnumerator();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38018");
            }
        }

        /// <summary>
        /// Retrieves an enumerator for the <see cref="P:BindingSource.List"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="Collections.IEnumerator"/> for the <see cref="P:BindingSource.List"/>.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            try
            {
                return _contextRows.GetEnumerator();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38019");
            }
        }

        #endregion IList<ContextTagsEditorView> Members

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="E:ObservableCollection.CollectionChanged"/> event of
        /// <see cref="_contextRows"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing
        /// the event data.</param>
        void HandleContextRows_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                bool dataChanged = false;

                // As rows are added to or deleted from _contextRows, reflect the change back to the
                // database.
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var row in e.NewItems.Cast<ContextTagsEditorViewRow>())
                    {
                        row.Initialize(_database, ActiveWorkflow);
                        dataChanged = true;
                        row.DataChanged += HandleRow_DataChanged;
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (var row in e.OldItems.Cast<ContextTagsEditorViewRow>())
                    {
                        if (row.HasBeenCommitted)
                        {
                            dataChanged = true;
                        }
                        row.Delete();
                    }
                }

                if (!_dataLoading && dataChanged)
                {
                    OnDataChanged();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38020");
            }
        }

        /// <summary>
        /// Handles the <see cref="ContextTagsEditorViewRow.DataChanged"/> event of a
        /// <see cref="ContextTagsEditorViewRow"/> in this collection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRow_DataChanged(object sender, EventArgs e)
        {
            try
            {
                OnDataChanged();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38229");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Loads the <see cref="PropertyDescriptor"/>s and data from the database.
        /// </summary>
        void LoadData()
        {
            try
            {
                _dataLoading = true;
                if (_database != null)
                {
                    // _defaultViewRow will be used to describe the collection's properties independent
                    // of specific rows in the collection.
                    _defaultViewRow = new ContextTagsEditorViewRow(_database, null, ActiveWorkflow);

                    foreach (var customTag in _database.CustomTags.OrderBy(ct => ct.Name).ToList())
                    {
                        // The row that is mapped to the new row in a DataGridView can end up being
                        // persisted. Ignore and delete any unnamed custom tags on load.
                        var row = new ContextTagsEditorViewRow(_database, customTag, ActiveWorkflow);
                        if (string.IsNullOrWhiteSpace(customTag.Name))
                        {
                            row.Delete();
                        }
                        else
                        {
                            _contextRows.Add(row);
                        }
                    }
                }
            }
            finally
            {
                _dataLoading = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="DataChanged"/> event.
        /// </summary>
        protected void OnDataChanged()
        {
            var eventHandler = DataChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
