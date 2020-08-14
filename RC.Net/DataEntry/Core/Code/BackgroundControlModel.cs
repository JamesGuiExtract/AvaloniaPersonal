using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Represents properties of the control an attribute would be mapped if the load were occurring
    /// in the foreground. This allows AutoUpdateQuerys that would affect control properties such as
    /// Disabled or Visible to properly impact attribute data and validation.
    /// </summary>
    [JsonObject(IsReference = true, Id = "Name")]
    public class BackgroundControlModel : IDataEntryControl
    {
        public BackgroundControlModel() { }

        public BackgroundControlModel(IDataEntryControl dataEntryControl)
        {
            try
            {
                var control = (Control)dataEntryControl;

                ID = control.Name;
                Visible = control.Visible;
                Disabled = dataEntryControl.Disabled;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50321");
            }
        }

        /// <summary>
        /// The Windows control name will be assigned as the ID; Don't persist this ID separately;
        /// instead, allow it to be persisted via <see cref="ControlReferenceResolver"/>.
        /// </summary>
        [JsonIgnore]
        public string ID { get; set; }

        /// <summary>
        /// Whether the control is <see cref="Disabled"/>.
        /// <para><b>Note</b></para>
        /// Disabled is a DataEntry framework property that should be used in place of Control.Enabled
        /// The Enabled status of a field will be dynamically changed when data is loaded/unloaded or
        /// when data is propagated from parent controls; thus Enabled should be left to the DE
        /// framework to control.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Whether the control is <see cref="Visible"/>.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets the UI element associated with the specified <see paramref="attribute"/>. This may
        /// be a type of <see cref="T:Control"/> or it may also be
        /// <see cref="T:DataGridViewElement"/> such as a <see cref="T:DataGridViewCell"/> if the
        /// <see paramref="attribute"/>'s owning control is a table control.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the UI element is needed.
        /// </param>
        /// <param name="elementName">If a related element is required, the property name of an
        /// object relative to the object mapped directly to the attribute. E.g., While a specific
        /// attribute may be mapped to a DataGridViewRow, an elementName of "DataGridView" could be
        /// used to refer to the grid as a whole.</param>
        public virtual object GetAttributeUIElement(IAttribute attribute, string elementName)
        {
            try
            {
                // For most background control models, no elements other than the controls itself
                // are represented.
                return string.IsNullOrEmpty(elementName)
                    ? this
                    : null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50314");
            }
        }

        #region Unused

        [JsonIgnore]
        public bool SupportsSwiping { get; set; }
        [JsonIgnore]
        public bool ClearClipboardOnPaste { get; set; }
        [JsonIgnore]
        public bool HighlightSelectionInChildControls { get; set; }
        [JsonIgnore]
        public IDataEntryControl ParentDataEntryControl { get; set; }
        [JsonIgnore]
        public DataEntryControlHost DataEntryControlHost { get; set; }
        // When loading data in background, Attributes property should not be accessed.
        [JsonIgnore]
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public IEnumerable<IAttribute> Attributes => throw new NotImplementedException();

        public event EventHandler<AttributesSelectedEventArgs> AttributesSelected { add { } remove { } }
        public event EventHandler<AttributesEventArgs> PropagateAttributes { add { } remove { } }
        public event EventHandler<SwipingStateChangedEventArgs> SwipingStateChanged { add { } remove { } }
        public event EventHandler<QueryDraggedDataSupportedEventArgs> QueryDraggedDataSupported { add { } remove { } }
        public event EventHandler<EventArgs> UpdateStarted { add { } remove { } }
        public event EventHandler<EventArgs> UpdateEnded { add { } remove { } }

        public void ApplySelection(SelectionState selectionState) { }
        public void ClearCachedData() { }
        public BackgroundFieldModel GetBackgroundFieldModel() => throw new NotImplementedException();
        public void HandlePropagateAttributes(object sender, AttributesEventArgs e) { }
        public void IndicateActive(bool setActive, Color color) { }
        public bool ProcessSwipedText(SpatialString swipedText) => throw new NotImplementedException();
        public void PropagateAttribute(IAttribute attribute, bool selectAttribute, bool selectTabGroup) { }
        public void RefreshAttributes() { }
        public void RefreshAttributes(bool spatialInfoUpdated, params IAttribute[] attributes) { }
        public void SetAttributes(IUnknownVector sourceAttributes) { }

        #endregion Unused
    }
}
