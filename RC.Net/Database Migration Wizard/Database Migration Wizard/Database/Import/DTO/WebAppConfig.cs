using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class WebAppConfig
    {
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Naming violations are a result of acronyms in the database.")]
        public string Type { get; set; }

        public string Settings { get; set; }
        
        public string Name { get; set; }

        public override string ToString()
        {
            return $@"(
                '{Type}'
                , {(Settings == null ? "NULL" : "'" + Settings.Replace("'", "''") + "'")}
                , {(Name == null ? "NULL" : "'" + Name.Replace("'", "''") + "'")}
                )";
        }
    }
}
