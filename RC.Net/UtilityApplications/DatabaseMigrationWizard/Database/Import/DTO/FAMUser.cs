﻿using System;
using System.Collections.Generic;
using static System.FormattableString;

namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class FAMUser
    {
        public string UserName { get; set; }

        public string FullUserName { get; set; }

        public Guid Guid { get; set; }

        public override bool Equals(object obj)
        {
            return obj is FAMUser user &&
                   UserName == user.UserName &&
                   FullUserName == user.FullUserName &&
                   Guid.Equals(user.Guid);
        }

        public override int GetHashCode()
        {
            var hashCode = -435663247;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UserName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullUserName);
            hashCode = hashCode * -1521134295 + Guid.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Invariant($"('{UserName}', '{FullUserName}', '{Guid}')");
        }
    }
}
