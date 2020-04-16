using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using static System.FormattableString;

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

        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is LabDEEncounter encounter &&
                   CSN == encounter.CSN &&
                   PatientMRN == encounter.PatientMRN &&
                   DateTime.Parse(EncounterDateTime, CultureInfo.InvariantCulture) == DateTime.Parse(encounter.EncounterDateTime, CultureInfo.InvariantCulture) &&
                   Department == encounter.Department &&
                   EncounterType == encounter.EncounterType &&
                   EncounterProvider == encounter.EncounterProvider &&
                   DateTime.Parse(DischargeDate, CultureInfo.InvariantCulture) == DateTime.Parse(encounter.DischargeDate, CultureInfo.InvariantCulture) &&
                   DateTime.Parse(AdmissionDate, CultureInfo.InvariantCulture) == DateTime.Parse(encounter.AdmissionDate, CultureInfo.InvariantCulture) &&
                   ADTMessage == encounter.ADTMessage &&
                   Guid.Equals(encounter.Guid);
        }

        public override int GetHashCode()
        {
            var hashCode = 1787092135;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CSN);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PatientMRN);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(EncounterDateTime);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Department);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(EncounterType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(EncounterProvider);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DischargeDate);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AdmissionDate);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ADTMessage);
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($@"(
                '{CSN}'
                , {(PatientMRN == null ? "NULL" : "'" + PatientMRN + "'")}
                , '{EncounterDateTime}'
                , '{Department.Replace("'", "''")}'
                , '{EncounterType.Replace("'", "''")}'
                , '{EncounterProvider.Replace("'", "''")}'
                , {(DischargeDate == null ? "NULL" : "'" + DischargeDate + "'")}
                , {(AdmissionDate == null ? "NULL" : "'" + AdmissionDate + "'")}
                , {(ADTMessage == null ? "NULL" : "CONVERT(XML, N'" + ADTMessage.Replace("'", "''") + "')")}
                , '{Guid}'
                )");
        }
    }
}
