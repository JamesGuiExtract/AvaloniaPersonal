namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class MetadataField
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return $"('{Name}')";
        }
    }
}
