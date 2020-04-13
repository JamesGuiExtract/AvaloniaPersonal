using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class LabDEEncounter
    {
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Naming violations are a result of acronyms in the database.")]
        public string CSN { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Naming violations are a result of acronyms in the database.")]
        public string PatientMRN { get; set; }

        public string EncounterDateTime { get; set; }

        public string Department { get; set; }

        public string EncounterType { get; set; }

        public string EncounterProvider { get; set; }

        public string DischargeDate { get; set; }

        public string AdmissionDate { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Naming violations are a result of acronyms in the database.")]
        public string ADTMessage { get; set; }

        public override string ToString()
        {
            return $@"(
                '{CSN}'
                , {(PatientMRN == null ? "NULL" : "'" + PatientMRN + "'")}
                , '{EncounterDateTime}'
                , '{Department.Replace("'", "''")}'
                , '{EncounterType.Replace("'", "''")}'
                , '{EncounterProvider.Replace("'", "''")}'
                , {(DischargeDate == null ? "NULL" : "'" + DischargeDate + "'")}
                , {(AdmissionDate == null ? "NULL" : "'" + AdmissionDate + "'")}
                , {(ADTMessage == null ? "NULL" : "CONVERT(XML, N'" + ADTMessage.Replace("'", "''") + "')")}
                )";
        }
    }
}
