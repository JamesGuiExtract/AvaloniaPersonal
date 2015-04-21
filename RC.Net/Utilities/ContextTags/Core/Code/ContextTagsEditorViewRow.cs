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
        public ContextTagsEditorViewRow(ContextTagDatabase database, CustomTagTableV1 customTag)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI37991",
                    _OBJECT_NAME);

                ExtractException.Assert("ELI37992", "Null argument exception", database != null);

                SetDatabase(database);

                CustomTag = customTag;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37994");
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Initializes this instance using the specified <see paramref="database"/>.
        /// </summary>
        /// <param name="database">The <see cref="ContextTagDatabase"/> instance associated with
        /// this row.</param>
        public void Initialize(ContextTagDatabase database)
        {
            try
            {
                SetDatabase(database);

                if (CustomTag == null)
                {
                    // Create the CustomTagTableV1 row to be represented by this instance.
                    CustomTag = new CustomTagTableV1();
                    CustomTag.Name = "";
                    _database.CustomTag.InsertOnSubmit(CustomTag);
                    
                    // Submit the new row to get the ID which can then be used to initialize values
                    // for all the contexts.
                    _database.SubmitChanges();

                    foreach (var context in _database.Context)
                    {
                        var tagValue = new TagValueTableV1();
                        tagValue.ContextID = context.ID;
                        tagValue.TagID = CustomTag.ID;
                        tagValue.Value = "";
                        _database.TagValue.InsertOnSubmit(tagValue);
                    }
                    _database.SubmitChanges();
                }
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
                foreach (var tagValue in _database.TagValue
                        .Where(tagValue => tagValue.TagID == CustomTag.ID))
                {
                    _database.TagValue.DeleteOnSubmit(tagValue);
                }
                _database.SubmitChanges();

                _database.CustomTag.DeleteOnSubmit(CustomTag);
                _database.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37996");
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
            get;
            set;
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
                (row, value) => SetTagName(row, value));
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
                    (row, value) => SetTagValue(row, context, value));

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
                string originalName = row.CustomTag.Name;
                string newName = (string)value;
                newName = newName.Trim();
                if (!newName.StartsWith("<", StringComparison.OrdinalIgnoreCase))
                {
                    newName = "<" + newName;
                }
                if (!newName.EndsWith(">", StringComparison.OrdinalIgnoreCase))
                {
                    newName += ">";
                }

                row.CustomTag.Name = newName;

                try
                {
                    _database.SubmitChanges();
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI38043");

                    // If we failed to apply the new value, it may be because of a constraint
                    // violation. Short of a local lifetime of _database (which might actually be
                    // most appropriate), reverting to the original name stands the best chance of
                    // preventing _database from getting stuck in a bad state.
                    row.CustomTag.Name = originalName;
                    _database.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37998");
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
                var contextValues = _database.TagValue
                    .Where(tagValue => tagValue.ContextID == context.ID);

                return contextValues
                    .Where(tagValue => tagValue.TagID == row.CustomTag.ID)
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
        /// <param name="context">The <see cref="ContextTableV1"/> for which the tag value should be
        /// set.</param>
        /// <param name="row">The <see cref="ContextTagsEditorViewRow"/> for which the tag value
        /// should be set.</param>
        /// <param name="value">The value to set.</param>
        void SetTagValue(ContextTagsEditorViewRow row, ContextTableV1 context, object value)
        {
            try
            {
                var targetValue = _database.TagValue
                    .Where(tagValue =>
                        tagValue.TagID == row.CustomTag.ID &&
                        tagValue.ContextID == context.ID)
                    .SingleOrDefault();

                if (targetValue == null)
                {
                    targetValue = new TagValueTableV1();
                    targetValue.ContextID = context.ID;
                    targetValue.TagID = row.CustomTag.ID;
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
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38000");
            }
        }

        #endregion Private Methods
    }
}
