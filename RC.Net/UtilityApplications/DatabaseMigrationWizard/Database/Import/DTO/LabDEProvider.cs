using System.Diagnostics.CodeAnalysis;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class LabDEProvider
    {
        public string ID { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string ProviderType { get; set; }

        public string Title { get; set; }

        public string Degree { get; set; }

        public string Departments { get; set; }

        public string Specialties { get; set; }

        public string Phone { get; set; }

        public string Fax { get; set; }

        public string Address { get; set; }

        public string OtherProviderID { get; set; }

        public bool? Inactive { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Naming violations are a result of acronyms in the database.")]
        public string MFNMessage { get; set; }

        public override string ToString()
        {
            return $@"(
                        '{ID}'
                        , '{FirstName.Replace("'", "''")}'
                        , {(MiddleName == null ? "NULL" : "'" + MiddleName.Replace("'","''") + "'")}
                        , '{LastName.Replace("'", "''")}'
                        , {(ProviderType == null ? "NULL" : "'" + ProviderType.Replace("'", "''") + "'")}
                        , {(Title == null ? "NULL" : "'" + Title.Replace("'", "''") + "'")}
                        , {(Degree == null ? "NULL" : "'" + Degree.Replace("'", "''") + "'")}
                        , '{Departments.Replace("'", "''")}'
                        , {(Specialties == null ? "NULL" : "'" + Specialties.Replace("'", "''") + "'")}
                        , {(Phone == null ? "NULL" : "'" + Phone.Replace("'", "''") + "'")}
                        , {(Fax == null ? "NULL" : "'" + Fax.Replace("'", "''") + "'")}
                        , {(Address == null ? "NULL" : "'" + Address.Replace("'", "''") + "'")}
                        , {(OtherProviderID == null ? "NULL" : "'" + OtherProviderID.Replace("'", "''") + "'")}
                        , {(Inactive != null && Inactive == true ? "1" : "0")}
                        , {(MFNMessage == null ? "NULL" : "CONVERT(XML, N'" + MFNMessage.Replace("'", "''") + "')")}
                    )";
        }
    }
}
