using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class Action
    {
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Naming violations are a result of acronyms in the database.")]
        public string ASCName { get; set; }

        public string Description { get; set; }
        
        public int? WorkflowID { get; set; }

        public bool? MainSequence { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return $@"(
                '{ASCName}'
                , {(Description == null ? "NULL" : "'" + Description + "'")}
                , {(WorkflowID == null ? "NULL" : WorkflowID.ToString())}
                , {(MainSequence == null ? "NULL" : (MainSequence == true ? "1" : "0" ))}
                , {(Name == null ? "NULL" : "'" + Name + "'")}
                )";
        }
    }
}
