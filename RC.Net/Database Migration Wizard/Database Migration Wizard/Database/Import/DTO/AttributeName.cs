namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class AttributeName
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return $@"('{Name}')";
        }
    }
}
