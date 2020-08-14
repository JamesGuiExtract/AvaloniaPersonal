using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Data representing a DEP field that can be used for loading data in a background thread
    /// without using the actual DEP controls.
    /// </summary>
    [DataContract]
    public partial class BackgroundFieldModel
    {
        /// <summary>
        /// The name of the corresponding attribute.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// A query which will cause the attribute's value to automatically be updated using values
        /// from other attribute's and/or a database query.
        /// </summary>
        [DataMember]
        public string AutoUpdateQuery { get; set; }

        /// <summary>
        /// A query which will cause the attribute's validation list to be automatically updated
        /// using values from other attributes and/or a database query.
        /// </summary>
        [DataMember]
        public string ValidationQuery { get; set; }

        /// <summary>
        /// An enumerable of <c>int</c> that allows the attribute to be sorted by comparing to the
        /// display order of other attributes.
        /// </summary>
        [DataMember]
        public IEnumerable<int> DisplayOrder { get; set; }

        /// <summary>
        /// Indicates whether the corresponding attribute would be viewable in the 
        /// <see cref="DataEntryControlHost"/>.
        /// </summary>
        [DataMember]
        public bool IsViewable { get; set; } = true;

        /// <summary>
        /// Specifies whether the attribute should be persisted in output.
        /// </summary>
        [DataMember]
        public bool PersistAttribute { get; set; } = true;

        /// <summary>
        /// The error message that should be displayed upon validation failure.
        /// </summary>
        [DataMember]
        public string ValidationErrorMessage { get; set; }

        /// <summary>
        /// A regular expression the data entered in a control must match prior to being saved.
        /// </summary>
        [DataMember]
        public string ValidationPattern { get; set; }

        /// <summary>
        /// A value that matches a validation list item case-insensitively but
        /// not case-sensitively will be changed to match the validation list value.
        /// </summary>
        [DataMember]
        public bool ValidationCorrectsCase { get; set; } = false;

        /// <summary>
        /// Whether at attribute should be automatically created for this field.
        /// </summary>
        [DataMember]
        public bool AutoCreate { get; set; } = true;

        [DataMember(Name = "OwningControl")]
        public BackgroundControlModel OwningControlModel { get; set; }

        // The models that should be mapped to subattributes of the current field's attributes.
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<BackgroundFieldModel> Children { get; set; } = new List<BackgroundFieldModel>();

        public IDataEntryControl OwningControl { get; set; }

        /// <summary>
        /// For controls that may format values beyond any defined AutoUpdateQueries, this method
        /// should be implemented to mimic the formatting the Windows control would perform in the
        /// foreground.
        /// </summary>
        public virtual void FormatValue(IAttribute attribute) { }
    }
}
