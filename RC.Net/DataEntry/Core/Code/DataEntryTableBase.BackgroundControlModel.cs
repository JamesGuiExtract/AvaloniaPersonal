using System;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    public partial class DataEntryTableBase
    {
        /// <summary>
        /// A <see cref="BackgroundControlModel"/> subclass that represents either a
        /// <see cref="DataEntryTable"/> or a <see cref="DataEntryTwoColumnTable"/> for the purpose
        /// of interpreting an elementName of "DataGridView" in <see cref="GetAttributeUIElement"/>.
        /// </summary>
        protected class DataEntryTableBaseBackgroundControlModel : BackgroundControlModel
        {
            public DataEntryTableBaseBackgroundControlModel() : base() { }
            public DataEntryTableBaseBackgroundControlModel(IDataEntryControl dataEntryControl) : base(dataEntryControl) { }

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
            public override object GetAttributeUIElement(IAttribute attribute, string elementName)
            {
                try
                {
                    // For table control models, only the "DataGridView" is represented (not rows,
                    // columns or cells).
                    if (elementName == "DataGridView")
                    {
                        return this;
                    }
                    else if (elementName == "DataGridView.DataEntryControlHost")
                    {
                        // The BackgroundModel represents DataEntryControlHost properties.
                        return BackgroundModel;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI50307");
                }
            }
        }

        /// <summary>
        /// Provides lazy instantiation of and <see cref="DataEntryTableBaseBackgroundControlModel"/>
        /// instance representing this control in background loads without Windows controls.
        /// </summary>
        protected DataEntryTableBaseBackgroundControlModel BackgroundOwningControlModel
        {
            get
            {
                if (_backgroundOwningControlModel == null)
                {
                    _backgroundOwningControlModel = new DataEntryTableBaseBackgroundControlModel(this);
                }

                return _backgroundOwningControlModel;
            }
        }
        DataEntryTableBaseBackgroundControlModel _backgroundOwningControlModel;
    }
}
