using System;
using System.Collections.Generic;
using System.Globalization;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class LabDEPatient
    {
        public string MRN { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Suffix { get; set; }

        public string DOB { get; set; }

        public string Gender { get; set; }

        public string MergedInto { get; set; }

        public string CurrentMRN { get; set; }

        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is LabDEPatient patient &&
                   MRN == patient.MRN &&
                   FirstName == patient.FirstName &&
                   MiddleName == patient.MiddleName &&
                   LastName == patient.LastName &&
                   Suffix == patient.Suffix &&
                   DateTime.Parse(DOB, CultureInfo.InvariantCulture) == DateTime.Parse(patient.DOB, CultureInfo.InvariantCulture) &&
                   Gender == patient.Gender &&
                   MergedInto == patient.MergedInto &&
                   CurrentMRN == patient.CurrentMRN &&
                   Guid.Equals(patient.Guid);
        }

        public override int GetHashCode()
        {
            var hashCode = 218944449;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MRN);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FirstName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MiddleName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LastName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Suffix);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DOB);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Gender);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MergedInto);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CurrentMRN);
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($@"(
                        '{MRN}'
                        , '{FirstName.Replace("'", "''")}'
                        , {(MiddleName == null ? "NULL" : "'" + MiddleName.Replace("'", "''") + "'")}
                        , '{LastName.Replace("'", "''")}'
                        , {(Suffix == null ? "NULL" : "'" + Suffix.Replace("'", "''") + "'")}
                        , {(DOB == null ? "NULL" : "'" + DOB + "'")}
                        , {(Gender == null ? "NULL" : "'" + Gender + "'")}
                        , {(MergedInto == null ? "NULL" : "'" + MergedInto + "'")}
                        , '{CurrentMRN}'
                        , '{Guid}'
                    )");
        }
    }
}
