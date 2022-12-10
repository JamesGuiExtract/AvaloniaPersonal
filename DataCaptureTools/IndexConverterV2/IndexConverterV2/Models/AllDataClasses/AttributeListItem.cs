using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexConverterV2.Models.AllDataClasses
{
    public class AttributeListItem
    {
        public string Name { get; set; } 
        public string Value { get; set; }
        public string Type { get; set; }
        public int FileIndex { get; set; }

        public AttributeListItem(string name, string value, string type, int fileIndex) 
        {
            this.Name = name;
            this.Value = value;
            this.Type = type;
            this.FileIndex = fileIndex;
        }
    }
}
