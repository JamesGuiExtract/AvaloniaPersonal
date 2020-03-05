using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class DataEntryCounterType
    {
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Naming violations are a result of acronyms in the database.")]
        public string Type { get; set; }

        public string Description { get; set; }

        public override string ToString()
        {
            return $@"(
                        '{Type}'
                        , '{Description}'
                    )";
        }
    }
}
