using System;

namespace IndexConverterV2.Models
{
    public record FileListItem(
            string Path,
            char Delimiter,
            Guid ID)
    {
        public sealed override string ToString() => Path;
    }
}
