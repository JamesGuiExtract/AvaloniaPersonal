using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexConverterV2.Models.AllDataClasses
{
    public class FileListItem : IEquatable<FileListItem>
    {
        public string Path { get; set; }
        public char Delimiter { get; set; }
        public char Qualifier { get; set; }

        public FileListItem(string path, char delimiter, char qualifier) 
        {
            this.Path = path;
            this.Delimiter = delimiter;
            this.Qualifier = qualifier;
        }

        public bool Equals(FileListItem? other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Path == other.Path
                && Delimiter == other.Delimiter
                && Qualifier == other.Qualifier;
        }

        public override string ToString()
        {
            return $"\"{Path}\",\'{Delimiter}\',\'{Qualifier}\'";
        }
    }
}
