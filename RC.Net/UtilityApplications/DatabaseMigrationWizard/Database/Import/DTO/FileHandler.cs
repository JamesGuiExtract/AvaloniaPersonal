using System;
using System.Collections.Generic;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class FileHandler
    {
        public bool Enabled { get; set; }

        public string AppName { get; set; }
        
        public string IconPath { get; set; }

        public string ApplicationPath { get; set; }

        public string Arguments { get; set; }

        public bool AdminOnly { get; set; }

        public bool AllowMultipleFiles { get; set; }

        public bool SupportsErrorHandling { get; set; }

        public bool Blocking { get; set; }

        public string WorkflowName { get; set; }

        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is FileHandler handler &&
                   Enabled == handler.Enabled &&
                   AppName == handler.AppName &&
                   IconPath == handler.IconPath &&
                   ApplicationPath == handler.ApplicationPath &&
                   Arguments == handler.Arguments &&
                   AdminOnly == handler.AdminOnly &&
                   AllowMultipleFiles == handler.AllowMultipleFiles &&
                   SupportsErrorHandling == handler.SupportsErrorHandling &&
                   Blocking == handler.Blocking &&
                   WorkflowName == handler.WorkflowName &&
                   Guid.Equals(handler.Guid);
        }

        public override int GetHashCode()
        {
            var hashCode = -784904647;
            hashCode = hashCode * -1521134295 + Enabled.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AppName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(IconPath);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ApplicationPath);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Arguments);
            hashCode = hashCode * -1521134295 + AdminOnly.GetHashCode();
            hashCode = hashCode * -1521134295 + AllowMultipleFiles.GetHashCode();
            hashCode = hashCode * -1521134295 + SupportsErrorHandling.GetHashCode();
            hashCode = hashCode * -1521134295 + Blocking.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(WorkflowName);
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($@"(
                    {(Enabled == true ? "1" : "0")}
                    , '{AppName}'
                    , {(IconPath == null ? "NULL" : "'" + IconPath + "'")}
                    , '{ApplicationPath}'
                    , {(Arguments == null ? "NULL" : "'" + Arguments + "'")}
                    , {(AdminOnly == true ? "1" : "0")}
                    , {(AllowMultipleFiles == true ? "1" : "0")}
                    , {(SupportsErrorHandling == true ? "1" : "0")}
                    , {(Blocking == true ? "1" : "0")}
                    , {(WorkflowName == null ? "NULL" : "'" + WorkflowName + "'")}
                    , '{Guid}'
                    )");
        }
    }
}
