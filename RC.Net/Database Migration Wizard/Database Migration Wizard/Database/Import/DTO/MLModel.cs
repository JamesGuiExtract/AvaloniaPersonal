namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class MLModel
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return $"('{Name}')";
        }
    }
}
