using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static System.FormattableString;

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

        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is LabDEProvider provider &&
                   ID == provider.ID &&
                   FirstName == provider.FirstName &&
                   MiddleName == provider.MiddleName &&
                   LastName == provider.LastName &&
                   ProviderType == provider.ProviderType &&
                   Title == provider.Title &&
                   Degree == provider.Degree &&
                   Departments == provider.Departments &&
                   Specialties == provider.Specialties &&
                   Phone == provider.Phone &&
                   Fax == provider.Fax &&
                   Address == provider.Address &&
                   OtherProviderID == provider.OtherProviderID &&
                   Inactive == provider.Inactive &&
                   MFNMessage == provider.MFNMessage &&
                   Guid.Equals(provider.Guid);
        }

        public override int GetHashCode()
        {
            var hashCode = -898974822;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ID);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FirstName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MiddleName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LastName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProviderType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Degree);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Departments);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Specialties);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Phone);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Fax);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Address);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(OtherProviderID);
            hashCode = hashCode * -1521134295 + Inactive.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MFNMessage);
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($@"(
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
                        , '{Guid}'
                    )");
        }
    }
}
