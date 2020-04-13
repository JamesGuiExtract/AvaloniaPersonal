namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class DatabaseService
    {
        public string Description { get; set; }

        public string Settings { get; set; }

        public bool Enabled { get; set; }

        public override string ToString()
        {
            return $"('{Description}', '{Settings.Replace("'", "''")}', {(Enabled == true ? "1" : "0")})";
        }
    }
}
