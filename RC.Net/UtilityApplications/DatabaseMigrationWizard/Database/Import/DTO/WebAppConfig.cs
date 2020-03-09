using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class WebAppConfig
    {
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Naming violations are a result of acronyms in the database.")]
        public string Type { get; set; }

        public string Settings { get; set; }
       
        public Guid WorkflowGuid { get; set; }

        public Guid WebAppConfigGuid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is WebAppConfig config &&
                   Type == config.Type &&
                   Settings == config.Settings &&
                   WorkflowGuid.Equals(config.WorkflowGuid) &&
                   WebAppConfigGuid.Equals(config.WebAppConfigGuid);
        }

        public override int GetHashCode()
        {
            var hashCode = 1472487649;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Settings);
            hashCode = hashCode * -1521134295 + WorkflowGuid.GetHashCode();
            hashCode = hashCode * -1521134295 + WebAppConfigGuid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($@"(
                '{Type}'
                , {(Settings == null ? "NULL" : "'" + Settings.Replace("'", "''") + "'")}
                , '{WorkflowGuid}'
                , '{WebAppConfigGuid}'
                )");
        }
    }
}
