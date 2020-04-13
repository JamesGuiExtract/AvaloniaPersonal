namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class DataEntryCounterDefinition
    {
        public string Name { get; set; }

        public string AttributeQuery { get; set; }

        public bool RecordOnLoad { get; set; }

        public bool RecordOnSave { get; set; }

        public override string ToString()
        {
            return $@"(
                        '{Name}'
                        , '{AttributeQuery}'
                        , {(RecordOnLoad == true ? "1" : "0")}
                        , {(RecordOnSave == true ? "1" : "0")}
                    )";
        }
    }
}
