using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.Code.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DatabaseServiceAttribute : Attribute { };

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ExtractCategoryAttribute : Attribute
    {
        public string Name { get; set; }

        public string TypeDescription { get; set; }

        public ExtractCategoryAttribute(string name, string typeDescription)
        {
            Name = name;
            TypeDescription = typeDescription;
        }

    }
}
