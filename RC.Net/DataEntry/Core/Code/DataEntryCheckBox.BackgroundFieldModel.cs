using System;
using System.Runtime.Serialization;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Shared properties of <see cref="DataEntryCheckBox"/> and <see cref="DataEntryCheckBoxBackgroundFieldModel"/>
    /// </summary>
    public interface ICheckBoxObject
    {
        /// <summary>
        /// The attribute value representing this control in the checked state.
        /// The comparison will be made case-insensitively when setting the checked state based on
        /// attribute value.
        /// </summary>
        string CheckedValue { get; }

        /// <summary>
        /// The attribute value representing this control in the un-checked state.
        /// The comparison will be made case-insensitively when setting the checked state based on
        /// attribute value.
        /// </summary>
        string UncheckedValue { get; }

        /// <summary>
        /// <c>true</c> if this check box should default to the checked state if the attribute value
        /// matches neither the <see cref="CheckedValue"/> nor <see cref="UncheckedValue"/>.
        /// </summary>
        bool DefaultCheckedState { get; }
    }

    public partial class DataEntryCheckBox
    {
        /// <summary>
        /// <see cref="DataEntry.BackgroundFieldModel"/> subclass with FormatValue override that
        /// mimics application of CheckedValue/UncheckedValue of the foreground control.
        /// </summary>
        [DataContract]
        class DataEntryCheckBoxBackgroundFieldModel : BackgroundFieldModel, ICheckBoxObject
        {
            /// <summary>
            /// The attribute value representing this control in the checked state.
            /// The comparison will be made case-insensitively when setting the checked state based on
            /// attribute value.
            /// </summary>
            [DataMember]
            public string CheckedValue { get; set; }

            /// <summary>
            /// The attribute value representing this control in the un-checked state.
            /// The comparison will be made case-insensitively when setting the checked state based on
            /// attribute value.
            /// </summary>
            [DataMember]
            public string UncheckedValue { get; set; }

            /// <summary>
            /// <c>true</c> if this check box should default to the checked state if the attribute value
            /// matches neither the <see cref="CheckedValue"/> nor <see cref="UncheckedValue"/>.
            /// </summary>
            [DataMember]
            public bool DefaultCheckedState { get; set; }

            /// <summary>
            /// Provides formatting that mimics application of CheckedValue/UncheckedValue the foreground control.
            /// </summary>
            public override void FormatValue(IAttribute attribute)
            {
                try
                {
                    if (attribute != null)
                    {
                        NormalizeValue(this, attribute);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI50237");
                }
            }
        }
    }
}
