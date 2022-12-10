using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexConverterV2.Models.AllDataClasses
{
    public class FileListItem
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
    }
}
