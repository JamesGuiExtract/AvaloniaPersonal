using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Claims;
using System.Security.Principal;
using Extract.Web.Shared.Security;
using System.Text.Json;

namespace Extract.Web.Shared
{
    public static class ActiveDirectoryUtilities
    {
        /// <summary>
        /// A function to get the groups for a specific user.
        /// </summary>
        /// <param name="User">The user to get the groups for.</param>
        /// <returns>Returns a collection of groups assigned to the user.</returns>
        public static IList<string> GetGroups(this WindowsIdentity windowsIdentity)
        {
            var groups = new Collection<string>();

            if (windowsIdentity.Groups != null)
            {
                foreach (var group in windowsIdentity.Groups)
                {
                    try
                    {
                        groups.Add(group.Translate(typeof(NTAccount)).ToString());
                    }
                    catch (Exception)
                    {
                        // This has to be ignored because .net 6 does not have an implementation of extract exception.
                    }
                }
            }
            return groups;
        }

        public static string GetEncryptedJsonUserAndGroups(WindowsIdentity claimsUser)
        {
            var user = new ActiveDirectoryUser()
            {
                UserName = claimsUser.Name,
                LastUpdated = DateTime.Now,
                ActiveDirectoryGroups = claimsUser.GetGroups()
            };

            return AESThenHMAC.SimpleEncryptWithPassword(JsonSerializer.Serialize(user));
        }
    }
}
