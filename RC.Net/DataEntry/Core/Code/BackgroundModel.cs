using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;

namespace Extract.DataEntry
{
    /// <summary>
    /// A representation of the controls and fields of a DEP needed to model the data formatting
    /// and validation of a DEP in the background without Windows controls.
    /// </summary>
    public class BackgroundModel
    {
        // The serialized JSON representation of this model.
        string _json;

        public BackgroundModel() { }

        public BackgroundModel(DataEntryControlHost controlHost)
        {
            try
            {
                ValidationEnabled = controlHost.ValidationEnabled;

                // The field models returned will have hierarchy only for fields within specific complex
                // controls (tables). The fields will need to be organized into the full hierarchy.
                var unorganizedFieldModels = GetFieldModels(controlHost).ToArray();

                Fields = new List<BackgroundFieldModel>();
                Controls = new List<BackgroundControlModel>();

                // Loop to build field hierarchy using 
                foreach (var fieldModel in unorganizedFieldModels)
                {
                    if (fieldModel.OwningControl.ParentDataEntryControl == null)
                    {
                        Fields.Add(fieldModel);
                    }
                    else
                    {
                        var parentModel = _fieldDictionary[fieldModel.OwningControl.ParentDataEntryControl];

                        // Confirm that the parent has a name since otherwise it won't be in the fieldModels collection
                        ExtractException.Assert("ELI45636", "Logic exception", !string.IsNullOrEmpty(parentModel?.Name));

                        parentModel.Children.Add(fieldModel);
                    }

                    Controls.Add(fieldModel.OwningControlModel);
                }

                // Sets OwningControl as OwningControlModel for all fields.
                ApplyOwningControlModels(Fields);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50372");
            }
        }

        /// <summary>
        /// Indicates whether validation is enabled for the all data in the panel as a whole.
        /// If <c>false</c>, validation queries will continue to provide auto-complete lists
        /// and alter case if ValidationCorrectsCase is set for any field, but it will not
        /// show any data errors or warnings or prevent saving of the document.
        /// </summary>
        public bool ValidationEnabled { get; set; }

        /// <summary>
        /// Represents the IDataEntryControls in the DEP
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<BackgroundControlModel> Controls { get; set; }

        /// <summary>
        /// Represents the fields (properties governing the attributes) in the DEP.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<BackgroundFieldModel> Fields { get; set; }

        // Tracks field/control relationships during model building in order to build the hierarchy
        // of fields.
        Dictionary<object, BackgroundFieldModel> _fieldDictionary = new Dictionary<object, BackgroundFieldModel>();

        /// <summary>
        /// Serializes this instance to a JSON representation of all <see cref="Controls"/> and <see cref="Fields"/>.
        /// </summary>
        public string ToJson()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_json))
                {
                    _json = JsonConvert.SerializeObject(this, Formatting.Indented, SerializationSettings);
                }

                return _json;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50241");
            }
        }

        /// <summary>
        /// Deserializes a <see cref="BackgroundModel"/> instance from the provided <see paramref="json"/>.
        /// </summary>
        /// <param name="masterCopy"><c>true</c> if this is to be a master copy from which per-OutputDocument
        /// instances will be cloned; for master copies which are likely to be cloned; _json will be cached
        /// for greater effiency.</param>
        public static BackgroundModel FromJson(string json, bool masterCopy)
        {
            try
            {
                var backgroundModel = JsonConvert.DeserializeObject<BackgroundModel>(
                        json, SerializationSettings);

                // Sets OwningControl as OwningControlModel for all fields.
                backgroundModel.ApplyOwningControlModels(backgroundModel.Fields);

                if (masterCopy)
                {
                    backgroundModel._json = json;
                }

                return backgroundModel;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50240");
            }
        }

        public BackgroundModel Clone()
        {
            try
            {
                return FromJson(ToJson(), masterCopy: false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50371");
            }
        }

        /// <summary>
        /// Retrieves a <see cref="BackgroundFieldModel"/> for each field to be represented by the specifed
        /// <see paramref="control"/>.
        /// </summary>
        IEnumerable<BackgroundFieldModel> GetFieldModels(Control control)
        {
            var dataEntryControl = control as IDataEntryControl;
            if (dataEntryControl != null)
            {
                var fieldModel = dataEntryControl.GetBackgroundFieldModel();

                // Don't create unnamed models
                // https://extract.atlassian.net/browse/ISSUE-15300
                if (!string.IsNullOrEmpty(fieldModel?.Name))
                {
                    _fieldDictionary[control] = fieldModel;

                    yield return fieldModel;
                }
            }

            foreach (var fieldModel in control.Controls.OfType<Control>()
                .SelectMany(child => GetFieldModels(child)))
            {
                yield return fieldModel;
            }
        }

        /// <summary>
        /// Sets <see cref="BackgroundFieldModel.OwningControl"/> as
        /// <see cref="BackgroundFieldModel.OwningControlModel"/> for all fields.
        /// </summary>
        void ApplyOwningControlModels(List<BackgroundFieldModel> fieldModels)
        {
            fieldModels.ForEach(fieldModel =>
            {
                fieldModel.OwningControlModel.BackgroundModel = this;
                fieldModel.OwningControl = fieldModel.OwningControlModel;
                ApplyOwningControlModels(fieldModel.Children);
            });
        }

        /// <summary>
        /// Gets the <see cref="JsonSerializerSettings"/> to use for <see cref="ToJson"/> and <see cref="FromJson"/>.
        /// </summary>
        static JsonSerializerSettings SerializationSettings { get; } =
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                ContractResolver = new ContractResolver(),
                ReferenceResolverProvider = () => new ControlReferenceResolver()
            };

        /// <summary>
        /// Ensures $type is serialized for subclasses of <see cref="BackgroundFieldModel"/>
        /// </summary>
        class ContractResolver : DefaultContractResolver
        {
            protected override JsonContract CreateContract(Type objectType)
            {
                var contract = base.CreateContract(objectType);

                // Use Auto for List<BackgroundFieldModel> or List<BackgroundControlModel> so that
                // subclasses get a $type field
                if ((objectType == typeof(List<BackgroundFieldModel>) || objectType == typeof(List<BackgroundControlModel>))
                    && contract is JsonContainerContract containerContract
                    && containerContract.ItemTypeNameHandling == null)
                {
                    containerContract.ItemTypeNameHandling = TypeNameHandling.Auto;
                }

                return contract;
            }
        }

        /// <summary>
        /// Creates and processes $refs to the owning <see cref="BackgroundControlModel"/>s of each
        /// <see cref="BackgroundFieldModel"/>.
        /// </summary>
        class ControlReferenceResolver : IReferenceResolver
        {
            // Maps ID (Windows control name) to each control model.
            Dictionary<string, BackgroundControlModel> _controlDictionary = new Dictionary<string, BackgroundControlModel>();

            public object ResolveReference(object context, string reference)
            {
                _controlDictionary.TryGetValue(reference, out var control);
                return control;
            }

            public string GetReference(object context, object value)
            {
                var control = (BackgroundControlModel)value;
                _controlDictionary[control.ID] = control;

                return control.ID;
            }

            public bool IsReferenced(object context, object value)
            {
                var control = (BackgroundControlModel)value;
                return _controlDictionary.ContainsKey(control.ID);
            }

            public void AddReference(object context, string reference, object value)
            {
                _controlDictionary[reference] = (BackgroundControlModel)value;
            }
        }
    }
}
