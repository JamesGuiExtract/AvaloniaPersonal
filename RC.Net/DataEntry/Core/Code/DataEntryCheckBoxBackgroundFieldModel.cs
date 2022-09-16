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

    public static class CheckBoxObjectMethods
    {
        /// <summary>
        /// Set the <see paramref="attribute"/> value according to the <see paramref="checkBox"/> configuration.
        /// </summary>
        /// This control will force any attribute value (or lack of value) to either <see cref="CheckedValue"/>
        /// or <see cref="UncheckedValue"/><remarks>
        /// </remarks>
        /// <returns><c>true</c> if the attribute value results in the control being in the checked
        /// state; <c>false</c> if it results in the control being in the unchecked state.</returns>
        public static bool NormalizeValue(this ICheckBoxObject checkBox, IAttribute attribute)
        {
            try
            {
                _ = attribute ?? throw new ArgumentNullException(nameof(attribute));

                return NormalizeValue(checkBox, attribute, attribute.Value.String);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53620");
            }
        }

        /// <summary>
        /// Set the <see paramref="attribute"/> value to the provided <see paramref="value"/> and 
        /// according to the <see paramref="checkBox"/> configuration.
        /// </summary>
        /// <returns><c>true</c> if the attribute value results in the control being in the checked
        /// state; <c>false</c> if it results in the control being in the unchecked state.</returns>
        public static bool NormalizeValue(this ICheckBoxObject checkBox, IAttribute attribute, string value)
        {
            try
            {
                _ = attribute ?? throw new ArgumentNullException(nameof(attribute));

                string normalizedValue = NormalizeValue(checkBox, value, out bool isChecked);

                AttributeStatusInfo.SetValue(attribute, normalizedValue, acceptSpatialInfo: false, endOfEdit: true);

                return isChecked;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53621");
            }
        }

        /// <summary>
        /// Format a string value to match ICheckBoxObject.CheckedValue or ICheckBoxObject.UncheckedValue
        /// </summary>
        public static string NormalizeValue(this ICheckBoxObject checkBox, string value)
        {
            try
            {
                return NormalizeValue(checkBox, value, out _);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53622");
            }
        }

        // Format a string value to match ICheckBoxObject.CheckedValue or ICheckBoxObject.UncheckedValue
        static string NormalizeValue(this ICheckBoxObject checkBox, string value, out bool isCheckedValue)
        {
            _ = checkBox ?? throw new ArgumentNullException(nameof(checkBox));

            value ??= "";

            if (value.Equals(checkBox.CheckedValue, StringComparison.OrdinalIgnoreCase))
            {
                isCheckedValue = true;
                return checkBox.CheckedValue;
            }
            else if (value.Equals(checkBox.UncheckedValue, StringComparison.OrdinalIgnoreCase))
            {
                isCheckedValue = false;
                return checkBox.UncheckedValue;
            }
            else
            {
                isCheckedValue = checkBox.DefaultCheckedState;
                return isCheckedValue ? checkBox.CheckedValue : checkBox.UncheckedValue;
            }
        }
    }

    /// <summary>
    /// <see cref="DataEntry.BackgroundFieldModel"/> subclass with FormatValue override that
    /// mimics application of CheckedValue/UncheckedValue of the foreground control.
    /// </summary>
    [DataContract]
    public class DataEntryCheckBoxBackgroundFieldModel : BackgroundFieldModel, ICheckBoxObject
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
                    this.NormalizeValue(attribute);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50237");
            }
        }
    }
}
