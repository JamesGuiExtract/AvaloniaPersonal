using System;
using System.Collections.Generic;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class Tag
    {
        public string TagName { get; set; }

        public string TagDescription { get; set; }

        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Tag tag &&
                   TagName == tag.TagName &&
                   TagDescription == tag.TagDescription &&
                   Guid.Equals(tag.Guid);
        }

        public override int GetHashCode()
        {
            var hashCode = 1344047235;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TagName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TagDescription);
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($"('{TagName}', '{TagDescription}', '{Guid}')");
        }
    }
}
