using System.Runtime.Serialization;

namespace Extract.DataEntry
{
    public partial class DataEntryCheckBox
    {
        /// <summary>
        /// <see cref="DataEntry.BackgroundFieldModel"/> subclass with FormatValue override that
        /// mimics application of CheckedValue/UncheckedValue of the foreground control.
        /// </summary>
        /// <remarks> This implementation has been moved to a top-level, public class so that it can be shared
        /// with the <see cref="DataEntryCheckBoxColumn"/> class</remarks>
        [DataContract]
        class DataEntryCheckBoxBackgroundFieldModel : DataEntry.DataEntryCheckBoxBackgroundFieldModel { }
    }
}
