namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class FieldSearch
    {
        public bool Enabled { get; set; }

        public string FieldName { get; set; }
        
        public string AttributeQuery { get; set; }

        public override string ToString()
        {
            return $@"(
                '{(Enabled == true ? "1" : "0")}'
                , '{FieldName}'
                , '{AttributeQuery}'
                )";
        }
    }
}
