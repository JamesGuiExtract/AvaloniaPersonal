using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexConverterV2.Models.AllDataClasses
{
    public class AttributeListItem : IEquatable<AttributeListItem>
    {
        public string Name { get; set; } 
        public string Value { get; set; }
        public string Type { get; set; }
        public FileListItem File { get; set; }
        public bool IsConditional { get; set; }

        //true means ==, false means !=
        public bool? ConditionType { get; set; }
        public string? LeftCondition { get; set; }
        public string? RightCondition { get; set; }

        public AttributeListItem(string name, string value, string type, FileListItem file, bool conditional, bool? conditionType, string? leftCondition, string? rightCondition) 
        {
            this.Name = name;
            this.Value = value;
            this.Type = type;
            this.File = file;
            this.IsConditional = conditional;
            if (IsConditional)
            {
                this.ConditionType = conditionType;
                this.LeftCondition = leftCondition;
                this.RightCondition = rightCondition;
            }
        }

        public bool Equals(AttributeListItem? other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Name == other.Name
                && Value == other.Value
                && Type == other.Type
                && File == other.File
                && IsConditional == IsConditional
                && ConditionType == other.ConditionType
                && LeftCondition == other.LeftCondition
                && RightCondition == other.RightCondition;
        }

        public override string ToString()
        {
            return $"\"{Name}\",\"{Value}\",\"{Type}\",{File},{IsConditional},{ConditionType},\"{LeftCondition}\",\"{RightCondition}\"";
        }
    }
}
