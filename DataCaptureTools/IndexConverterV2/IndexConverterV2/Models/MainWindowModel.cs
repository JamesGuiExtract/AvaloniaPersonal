using System.Collections.Generic;

namespace IndexConverterV2.Models
{
    public record MainWindowModel(
        IList<FileListItem> InputFiles,
        IList<AttributeListItem> Attributes,
        string OutputFolder);
}
