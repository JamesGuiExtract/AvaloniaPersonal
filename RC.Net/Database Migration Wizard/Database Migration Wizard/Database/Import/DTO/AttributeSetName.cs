namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class AttributeSetName
    {
        public string Description { get; set; }

        public override string ToString()
        {
            return $"('{Description}')";
        }
    }
}
