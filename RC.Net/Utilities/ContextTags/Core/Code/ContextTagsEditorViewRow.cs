using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Extract.Utilities.ContextTags
{
    /// <summary>
    /// Defines an editable row of a view for a <see cref="ContextTagDatabase"/> (CustomTag.sdf) 
    /// database where the defined contexts comprise the columns and the custom tags comprise the
    /// rows.
    /// </summary>
    public class ContextTagsEditorViewRow : CustomTypeDescriptor
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ContextTagsEditorViewRow).ToString();

        /// <summary>
        /// Keeps track of whether a warning prompt has been displayed for editing a context other
        /// than the current context for the given database. (No need to prompt for each individual
        /// value that is edited.)
        /// </summary>
        static HashSet<ContextTagDatabase> _promptedForCrossContextEdit =
            new HashSet<ContextTagDatabase>();

        /// <summary>
        /// Constant string for the database server tag.
        /// </summary>
        const string DatabaseServerTag = "<DatabaseServer>";

        /// <summary>
        /// Constant string for the database name tag.
        /// </summary>
        const string DatabaseNameTag = "<DatabaseName>";

        /// <summary>
        /// Constant string for the default values tag
        /// </summary>
        const string DefaultValuesTag = "<DefaultValues>";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Tracks the <see cref="ContextTagDatabase"/> currently associated with this class type on
        /// the current thread.
        /// </summary>
        ContextTagDatabase _database;

        /// <summary>
        /// Tracks the <see cref="PropertyDescriptorCollection"/> currently associated with this
        /// class type on the current thread. 
        /// </summary>
        PropertyDescriptorCollection _properties;

        /// <summary>
        /// The name of the context from <see cref="_database"/> that is currently active.
        /// </summary>
        string _activeContextName;

        /// <summary>
        /// The <see cref="CustomTagTableV1"/> row represented by this instance.
        /// </summary>
        CustomTagTableV1 _customTag;

        /// <summary>
        /// The current workflow that is being edited.
		/// default is "" use ActiveWorkflow to access this
        /// </summary>
        static string _workflow = "";

		/// <summary>
		/// Object to lock when using _workflow string
		/// </summary>
        static readonly object _workflowLock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagsEditorViewRow"/> class.
        /// </summary>
        public ContextTagsEditorViewRow()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI37989",
                    _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                // The exception needs to be displayed here rather than throw. This class will be
                // instantiated by the DataGridView class, and there will be no higher-level place
                // in extract code where the exception can be handled.
                ex.ExtractDisplay("ELI37990");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagsEditorViewRow"/> class to
        /// represent the specified <see paramref="customTag"/>
        /// </summary>
        /// <param name="database"></param>
        /// <param name="customTag">The <see cref="CustomTagTableV1"/> row represented by this
        /// instance.</param>
        /// <param name="workflow">Workflow that is currently active</param>
        public ContextTagsEditorViewRow(ContextTagDatabase database, CustomTagTableV1 customTag, string workflow)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI37991",
                    _OBJECT_NAME);

                ExtractException.Assert("ELI37992", "Null argument exception", database != null);

                ActiveWorkflow = workflow;

                SetDatabase(database);

                _customTag = customTag;

                // If we loaded this row from the database, it is committed to the database.
                HasBeenCommitted = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37994");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when the row's data has been changed.
        /// </summary>
        public event EventHandler<EventArgs> DataChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance's data has been successfully committed to
        /// the database.
        /// </summary>
        /// <value><see langword="true"/> if this instance's data has been successfully committed to
        /// the database; otherwise, <see langword="false"/>.
        /// </value>
        public bool HasBeenCommitted
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to access the current _workflow field with locking
        /// </summary>
        static public string ActiveWorkflow
        {
            get
            {
                lock (_workflowLock)
                {
                    return _workflow;
                }
            }
            set
            {
                try
                {
                    lock (_workflowLock)
                    {
                        if (value == DefaultValuesTag)
                        {
                            _workflow = "";
                        }
                        else
                        {
                            _workflow = value;
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw ex.AsExtract("ELI43270");
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initializes this instance using the specified <see paramref="database"/>.
        /// </summary>
        /// <param name="database">The <see cref="ContextTagDatabase"/> instance associated with
        /// this row.</param>
        /// <param name="workflow">The currently active workflow</param>
        public void Initialize(ContextTagDatabase database, string workflow)
        {
            try
            {
                ActiveWorkflow = workflow;

                SetDatabase(database);

                // Do not attempt to commit any data to the database for this row as part of
                // initialization. At this point the row should not be represented in the database.
                // Also, at this point the tag name will be null and invalid which would cause an
                // error glyph for the new row of the table just by clicking on it.
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37995");
            }
        }

        /// <summary>
        /// Deletes the data associated with this instance from the database.
        /// </summary>
        public void Delete()
        {
            try
            {
                // If _customTag is not set, this row has not been initialized and there is nothing
                // to delete.
                if (_customTag != null)
                {
                    // Deletes will cascade.
                    _database.CustomTag.DeleteOnSubmit(CustomTag);
                    _database.SubmitChanges();

                    // Only consider this a data change if the row had not yet been committed to the DB.
                    if (HasBeenCommitted)
                    {
                        OnDataChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37996");
            }
        }

        /// <summary>
        /// Deletes the tag workflow value data associated with this instance from the database.
        /// </summary>
        /// <param name="context">Context that has the value to be deleted</param>
        public void DeleteWorkflowValue(string context)
        {
            try
            {
                // If _customTag is not set, this row has not been initialized and there is nothing
                // to delete.
                if (_customTag != null)
                {
                    var Context = _database.Context.Single(c => c.Name == context);

                    var TagValue = _database.TagValue
                        .Single(t => t.ContextID == Context.ID && t.TagID == _customTag.ID && t.Workflow == _workflow);

                    // Deletes will cascade.
                    _database.TagValue.DeleteOnSubmit(TagValue);
                    _database.SubmitChanges();

                    if (HasBeenCommitted)
                    {
                        OnDataChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43353");
            }
        }


        #endregion Methods

        #region Overrides

        /// <summary>
        /// Returns the properties for this instance of a component using the attribute array as a
        /// filter.
        /// </summary>
        /// <param name="attributes">An array of type <see cref="T:System.Attribute"/> that is used
        /// as a filter.</param>
        /// <returns>
        /// A <see cref="T:System.ComponentModel.PropertyDescriptorCollection"/> that represents the
        /// filtered properties for this component instance.
        /// </returns>
        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            try
            {
                List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
                foreach (PropertyDescriptor property in GetProperties())
                {
                    if (attributes.Any(attribute => property.Attributes.Contains(attribute)))
                    {
                        properties.Add(property);
                    }
                }
                return new PropertyDescriptorCollection(properties.ToArray());
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37997");
            }
        }

        /// <summary>
        /// Returns the properties for this instance of a component.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.ComponentModel.PropertyDescriptorCollection"/> that represents the
        /// properties for this component instance.
        /// </returns>
        public override PropertyDescriptorCollection GetProperties()
        {
            return _properties;
        }

        /// <summary>
        /// Returns the property descriptor for the default property of the object represented by
        /// this type descriptor.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.ComponentModel.PropertyDescriptor"/> for the default property on
        /// the object represented by this type descriptor. The default is null.
        /// </returns>
        public override PropertyDescriptor GetDefaultProperty()
        {
            return base.GetDefaultProperty();
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// The <see cref="CustomTagTableV1"/> row represented by this instance.
        /// </summary>
        CustomTagTableV1 CustomTag
        {
            get
            {
                if (_customTag == null)
                {
                    // Create the CustomTagTableV1 row to be represented by this instance.
                    _customTag = new CustomTagTableV1();
                    // Set the name to non-null so that it can be submitted (the name column has a
                    // non-null constraint.
                    _customTag.Name = "";
                    _database.CustomTag.InsertOnSubmit(_customTag);

                    // Submit the new row to get the ID which can then be used to initialize values
                    // for all the contexts.
                    _database.SubmitChanges();

                    // Use a local version of ActiveWorkflow to eliminate the locking in the loop
                    string workflowForValue = ActiveWorkflow;

                    foreach (var context in _database.Context)
                    {
                        var tagValue = new TagValueTableV2();
                        tagValue.ContextID = context.ID;
                        tagValue.TagID = _customTag.ID;
                        tagValue.Workflow = workflowForValue;
                        tagValue.Value = "";
                        _database.TagValue.InsertOnSubmit(tagValue);
                        
                        if (!string.IsNullOrEmpty(workflowForValue))
                        {
                            tagValue = new TagValueTableV2();
                            tagValue.ContextID = context.ID;
                            tagValue.TagID = _customTag.ID;
                            tagValue.Workflow = "";
                            tagValue.Value = "";
                            _database.TagValue.InsertOnSubmit(tagValue);
                        }
                    }
                    _database.SubmitChanges();

                    // After the custom tag has been added, set the name back to null to ensure it
                    // gets set before the row as a whole can be committed. (row will remain invalid
                    // until the user enters a proper name).
                    _customTag.Name = null;
                }

                return _customTag;
            }
        }

        /// <summary>
        /// Initializes the available properties based on the <see paramref="database"/>.
        /// </summary>
        /// <param name="database">The <see cref="ContextTagDatabase"/> associated with this
        /// instance.</param>
        void SetDatabase(ContextTagDatabase database)
        {
            _database = database;

            _properties = new PropertyDescriptorCollection(
                GetTagNamePropertyDescriptor()
                .Union(GetContextPropertyDescriptors())
                .ToArray());

            _activeContextName = _database.GetContextNameForDirectory(_database.DatabaseDirectory);
        }

        /// <summary>
        /// Gets a <see cref="PropertyDescriptor"/> representing the tag name field.
        /// </summary>
        /// <returns>A <see cref="PropertyDescriptor"/> representing the tag name field.</returns>
        IEnumerable<PropertyDescriptor> GetTagNamePropertyDescriptor()
        {
            yield return new ContextTagsEditorViewPropertyDescriptor(
                "Tag name",
                row => row.CustomTag.Name,
                (row, value) => SetTagName(row, value),
                row => false);
        }

        /// <summary>
        /// Gets a set of <see cref="PropertyDescriptor"/>s representing tag values for each
        /// context.
        /// </summary>
        /// <returns>A <see cref="PropertyDescriptor"/> representing the tag name field.</returns>
        IEnumerable<PropertyDescriptor> GetContextPropertyDescriptors()
        {
            // The AsEnumerable method here is critical; it flattens the results (contexts) to be
            // evaluated separately. Otherwise, this statement fails to get converted in to SQL.
            // See: http://stackoverflow.com/questions/5179341/a-lambda-expression-with-a-statement-body-cannot-be-converted-to-an-expression
            return _database.Context.AsEnumerable().Select(context =>
            {
                var propertyDescriptor = new ContextTagsEditorViewPropertyDescriptor(
                    context.Name,
                    (row) => GetTagValue(row, context),
                    (row, value) => SetTagValue(row, context, value),
                    (row) => IsWorkflowValue(row, context));

                return propertyDescriptor;
            });
        }

        /// <summary>
        /// Sets the name of the tag for the specified <see paramref="row"/>.
        /// </summary>
        /// <param name="row">The <see cref="ContextTagsEditorViewRow"/> for which the name should
        /// be set.</param>
        /// <param name="value">The new name.</param>
        void SetTagName(ContextTagsEditorViewRow row, object value)
        {
            try
            {
                string newName = value as string;
                if (string.IsNullOrWhiteSpace(newName))
                {
                    // If the tag name is blank, for it to null to deliberately set it to null to
                    // force a constraint violation so the row's data will appear as invalid.
                    newName = null;
                }
                else
                {
                    newName = newName.Trim();
                    if (!newName.StartsWith("<", StringComparison.OrdinalIgnoreCase))
                    {
                        newName = "<" + newName;
                    }
                    if (!newName.EndsWith(">", StringComparison.OrdinalIgnoreCase))
                    {
                        newName += ">";
                    }
                }

                row.CustomTag.Name = newName;

                _database.SubmitChanges();
                row.HasBeenCommitted = true;
                row.OnDataChanged();
            }
            catch (Exception ex)
            {
                throw AsDataErrorException("ELI37998", ex);
            }
        }

        /// <summary>
        /// Gets the value of the tag for the specified <see paramref="row"/> and
        /// <see paramref="context"/>.
        /// </summary>
        /// <param name="context">The <see cref="ContextTableV1"/> for which the tag value should be
        /// retrieved.</param>
        /// <param name="row">The <see cref="ContextTagsEditorViewRow"/> for which the tag value
        /// should be retrieved.</param>
        /// <returns>The value of the tag</returns>
        string GetTagValue(ContextTagsEditorViewRow row, ContextTableV1 context)
        {
            try
            {
                if (_database.IsDisposed)
                {
                    return null;
                }

                var contextValues = _database.TagValue
                    .Where(tagValue => tagValue.ContextID == context.ID);

                var workflowContextValues = contextValues
                    .Where(tagValue => tagValue.TagID == row.CustomTag.ID && tagValue.Workflow == ActiveWorkflow)
                    .Select(tagValue => tagValue.Value)
                    .SingleOrDefault();

                return workflowContextValues ??  contextValues
                     .Where(tagValue => tagValue.TagID == row.CustomTag.ID && tagValue.Workflow.Length == 0)
                     .Select(tagValue => tagValue.Value)
                     .SingleOrDefault();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37999");
            }
        }

        /// <summary>
        /// Sets the value of the tag for the specified <see paramref="row"/> and
        /// <see paramref="context"/>.
        /// </summary>
        /// <param name="row">The <see cref="ContextTagsEditorViewRow"/> for which the tag value
        /// should be set.</param>
        /// <param name="context">The <see cref="ContextTableV1"/> for which the tag value should be
        /// set.</param>
        /// <param name="value">The value to set.</param>
        void SetTagValue(ContextTagsEditorViewRow row, ContextTableV1 context, object value)
        {
            try
            {
                if (_database.IsDisposed)
                {
                    return;
                }

                bool IsDatabaseTag = row.CustomTag.Name == DatabaseServerTag || row.CustomTag.Name == DatabaseNameTag;

                string workflowForValue = "";
                if (!IsDatabaseTag && !string.IsNullOrEmpty(ActiveWorkflow))
                {
                    workflowForValue = ActiveWorkflow;
                }
                var targetValue = _database.TagValue
                    .Where(tagValue =>
                        tagValue.TagID == row.CustomTag.ID &&
                        tagValue.ContextID == context.ID &&
                        tagValue.Workflow == workflowForValue)
                    .SingleOrDefault();

                if (targetValue == null)
                {
                    targetValue = new TagValueTableV2();
                    targetValue.ContextID = context.ID;
                    targetValue.TagID = row.CustomTag.ID;
                    targetValue.Workflow = workflowForValue;
                    targetValue.Value = "";
                    _database.TagValue.InsertOnSubmit(targetValue);
                }

                string newValue = ((string)value) ?? "";

                // If changing a tag value for another context, prompt to ensure user understands
                // when this value will be used (but don't prompt again once the prompt has been
                // displayed for this database).
                if (newValue != targetValue.Value &&
                    _activeContextName != null && _activeContextName != context.Name &&
                    !_promptedForCrossContextEdit.Contains(_database))
                {
                    UtilityMethods.ShowMessageBox(
                        "You are editing a context other than the active context. The tag value " +
                        "applied here will only be used once this CustomTags file is copied to " +
                        "the directory: \"" + context.FPSFileDir + "\".", "Warning", false);
                    _promptedForCrossContextEdit.Add(_database);
                }

                targetValue.Value = newValue;
                _database.SubmitChanges();
                row.HasBeenCommitted = true;
                row.OnDataChanged();
            }
            catch (Exception ex)
            {
                throw AsDataErrorException("ELI38000", ex);
            }
        }

        /// <summary>
        /// Returns whether the current row is a workflow specific value for the given context
        /// </summary>
        /// <param name="row">The <see cref="ContextTagsEditorViewRow"/> for which the tag value
        /// should be set.</param>
        /// <param name="context">The <see cref="ContextTableV1"/> for which the tag value should be
        /// set.</param>
        /// <returns><see langword="true"/> if current value is workflow specific <see langword="false"/>
        /// if it is not</returns>
        bool IsWorkflowValue(ContextTagsEditorViewRow row, ContextTableV1 context)
        {
            try
            {
                if (_database.IsDisposed)
                {
                    return false;
                }

                var contextValues = _database.TagValue
                    .Where(tagValue => tagValue.ContextID == context.ID);

                var workflowContextValue = contextValues
                    .Where(tagValue => tagValue.TagID == row.CustomTag.ID && tagValue.Workflow == ActiveWorkflow)
                    .Select(tagValue => tagValue.Value)
                    .SingleOrDefault();

                return !String.IsNullOrEmpty(ActiveWorkflow) && workflowContextValue != null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43349");
            }
        }

        /// <summary>
        /// Gets the specified exception relating to a data error as a more user-friendly exception.
        /// </summary>
        /// <param name="eliCode">The ELI code to use.</param>
        /// <param name="ex">The <see cref="Exception"/> pertaining to a data error.</param>
        /// <returns>A user-friendly data error exception.</returns>
        static ExtractException AsDataErrorException(string eliCode, Exception ex)
        {
            ExtractException ee = null;
            if (ex.Message.Contains("Column name = Name") && ex.Message.Contains("null"))
            {
                ee = new ExtractException(eliCode, "Tag name has not been specified", ex);
            }
            else if (ex.Message.Contains("UC_CustomTagName"))
            {
                ee = new ExtractException(eliCode, "Tag name has already been used", ex);
            }

            if (ee == null)
            {
                ee = ex.AsExtract(eliCode);
            }

            return ee;
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

        #endregion Private Methods
    }
}
