using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class LabDEOrder
    {
        public string OrderNumber { get; set; }

        public string OrderCode { get; set; }

        public string PatientMRN { get; set; }

        public string ReceivedDateTime { get; set; }

        public string OrderStatus { get; set; }

        public string ReferenceDateTime { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Naming violations are a result of acronyms in the database.")]
        public string ORMMessage { get; set; }

        public string EncounterID { get; set; }

        public string AccessionNumber { get; set; }

        public override string ToString()
        {
            return $@"(
                        '{OrderNumber}'
                        , '{OrderCode}'
                        , {(PatientMRN == null ? "NULL" : "'" + PatientMRN.Replace("'", "''") + "'")}
                        , '{ReceivedDateTime}'
                        , '{OrderStatus}'
                        , {(ReferenceDateTime == null ? "NULL" : "'" + ReferenceDateTime + "'")}
                        , {(ORMMessage == null ? "NULL" : "CONVERT(XML, N'" + ORMMessage.Replace("'", "''") + "')")}
                        , {(EncounterID == null ? "NULL" : "'" + EncounterID + "'")}
                        , {(AccessionNumber == null ? "NULL" : "'" + AccessionNumber + "'")}
                    )";
        }
    }
}
