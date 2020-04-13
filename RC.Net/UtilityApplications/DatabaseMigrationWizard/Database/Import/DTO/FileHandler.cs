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

        public override string ToString()
        {
            return $@"(
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
                    )";
        }
    }
}
