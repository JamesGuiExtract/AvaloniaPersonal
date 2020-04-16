using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", Justification = "Naming violations are a result of acronyms in the database.")]
    public class Login
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Login login &&
                   ((UserName == "admin" && login.UserName == "admin" &&
                   Password == login.Password) ||
                   (UserName == login.UserName &&
                   Password == login.Password &&
                   Guid.Equals(login.Guid)));
        }

        public override int GetHashCode()
        {
            var hashCode = -473259731;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UserName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Password);
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($"('{UserName}', '{Password}', '{Guid}')");
        }
    }
}
