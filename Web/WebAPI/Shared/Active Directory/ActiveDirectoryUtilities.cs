using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Principal;
using WebAPI.Security;
using System.Text.Json;

namespace WebAPI
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

        /// <summary>
        /// Gets an encrypted json object containing an encrypted <see cref="ActiveDirectoryUser"/>.
        /// </summary>
        /// <param name="claimsUser">The user to get the groups for</param>
        /// <returns>An encrypted json string.</returns>
        public static string GetEncryptedJsonUserAndGroups(WindowsIdentity claimsUser)
        {
            var user = new ActiveDirectoryUser()
            {
                UserName = claimsUser.Name,
                LastUpdated = DateTime.Now,
                ActiveDirectoryGroups = claimsUser.GetGroups()
            };
            // The password needs to be atleast 12 characters.
            string password = Environment.UserDomainName + DateTime.Today.ToString();
            return AESThenHMAC.SimpleEncryptWithPassword(JsonSerializer.Serialize(user), password);
        }

        /// <summary>
        /// Takes an encrypted JSON string containing a <see cref="ActiveDirectoryUser"/>, and parses it.
        /// </summary>
        /// <param name="encryptedJsonUserAndGroups">The encrypted string</param>
        /// <returns>An active directory user.</returns>
        public static ActiveDirectoryUser DecryptUser(string encryptedJsonUserAndGroups)
        {
            string password = Environment.UserDomainName + DateTime.Today.ToString();
            return JsonSerializer.Deserialize<ActiveDirectoryUser>(AESThenHMAC.SimpleDecryptWithPassword(encryptedJsonUserAndGroups, password));
        }
    }
}
