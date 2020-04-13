namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class DBInfo
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return $"('{Name}', '{Value}')";
        }
    }
}
