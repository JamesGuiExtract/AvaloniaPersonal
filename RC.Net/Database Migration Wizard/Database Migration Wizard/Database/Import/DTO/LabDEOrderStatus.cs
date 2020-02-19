namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class LabDEOrderStatus
    {
        public string Code { get; set; }

        public string Meaning { get; set; }

        public override string ToString()
        {
            return $"('{Code}', '{Meaning}')";
        }
    }
}
