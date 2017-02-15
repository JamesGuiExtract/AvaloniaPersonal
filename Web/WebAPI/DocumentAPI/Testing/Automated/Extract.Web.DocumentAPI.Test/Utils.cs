using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Extract.Web.DocumentAPI.Test
{
    public static class Utils
    {
        // TODO - this should be an extension method somewhere in the Extract framework, 
        // as I've now copied this method...
        //
        /// <summary>
        /// string extension method to simplify determining if two strings are equivalent
        /// </summary>
        /// <param name="s1">this</param>
        /// <param name="s2">string to compare this against</param>
        /// <param name="ignoreCase">defaults to true</param>
        /// <returns>true or false</returns>
        public static bool IsEquivalent(this string s1,
                        string s2,
                        bool ignoreCase = true)
        {
            if (String.Compare(s1, s2, ignoreCase, CultureInfo.InvariantCulture) == 0)
                return true;

            return false;
        }

        /// <summary>
        /// returns the method name of the caller - do NOT set the default argument!
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static string GetMethodName([CallerMemberName] string caller = null)
        {
            return caller;
        }
    }
}
