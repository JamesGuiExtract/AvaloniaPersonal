using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", Justification = "Naming violations are a result of acronyms in the database.")]
    public class Login
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public override string ToString()
        {
            return $"('{UserName}', '{Password}')";
        }
    }
}
