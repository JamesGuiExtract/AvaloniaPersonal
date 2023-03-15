using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "API")]
    public class WebAPIConfiguration
    {
        public Guid Guid { get; set; }

        public string Name { get; set; }

        public string Settings { get; set; }

        public override bool Equals(object obj)
        {
            return obj is WebAPIConfiguration webAPIConfiguration &&
                   Guid == webAPIConfiguration.Guid &&
                   Name == webAPIConfiguration.Name &&
                   Settings == webAPIConfiguration.Settings;
        }

        public override int GetHashCode()
        {
            return HashCode.Start
                .Hash(Name)
                .Hash(Settings)
                .Hash(Guid);
        }

        public override string ToString()
        {
            return Invariant($@"(
                '{Name}'
                , {(Settings == null ? "NULL" : "'" + Settings + "'")}
                , '{Guid}'
                )");
        }
    }
}
