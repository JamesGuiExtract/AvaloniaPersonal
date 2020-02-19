namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class FAMUser
    {
        public string UserName { get; set; }

        public string FullUserName { get; set; }

        public override string ToString()
        {
            return $"('{UserName}', '{FullUserName}')";
        }
    }
}
