namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class UserCreatedCounter
    {
        public string CounterName { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return $"('{CounterName}', '{Value}')";
        }
    }
}
