using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Extract.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    [ComVisible(true)]
    [Guid("96A5FF2E-0EAC-4792-BCE5-C184F8F43871")]
    public interface IAuthenticationProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="caption"></param>
        /// <param name="message"></param>
        /// <param name="defaultToCurrentUser"></param>
        void PromptForAndValidateWindowsCredentials(string databaseName, string databaseServer);
    }
}
