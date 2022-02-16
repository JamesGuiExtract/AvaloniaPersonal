using System;
using System.Net;
using System.Security;

namespace Extract.Utilities
{
    public static class SecureStringExtension
    {
        public static string Unsecure(this SecureString secureString)
        {
            try
            {
                return new NetworkCredential("", secureString).Password;
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI53224");
            }
        }
    }
}
