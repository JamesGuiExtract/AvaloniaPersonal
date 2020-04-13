using System;

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

        public override string ToString()
        {
            return $@"(
                        '{MRN}'
                        , '{FirstName.Replace("'", "''")}'
                        , {(MiddleName == null ? "NULL" : "'" + MiddleName.Replace("'", "''") + "'")}
                        , '{LastName.Replace("'", "''")}'
                        , {(Suffix == null ? "NULL" : "'" + Suffix.Replace("'", "''") + "'")}
                        , {(DOB == null ? "NULL" : "'" + DOB + "'")}
                        , {(Gender == null ? "NULL" : "'" + Gender + "'")}
                        , {(MergedInto == null ? "NULL" : "'" + MergedInto + "'")}
                        , '{CurrentMRN}'
                    )";
        }
    }
}
