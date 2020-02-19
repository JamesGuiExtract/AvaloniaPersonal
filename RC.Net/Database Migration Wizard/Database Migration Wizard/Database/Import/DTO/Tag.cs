namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class Tag
    {
        public string TagName { get; set; }

        public string TagDescription { get; set; }

        public override string ToString()
        {
            return $"('{TagName}', '{TagDescription}')";
        }
    }
}
