using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcUploadFile
{
    public class Options
    {
        /// <summary>
        /// ctor
        /// </summary>
        public Options()
        {
        }

        /// <summary>
        /// The "site specific" portion of the URL
        /// e.g. http://david2016svrvm, or http://localHost:54321
        /// </summary>
        public string SiteSpecificUrl { get; set; }

        /// <summary>
        /// The "web api portion" of the URL
        /// E.g. "FileApi_VS2017/api/FileItem" or "api/FileItem" (for local IISExpress-hosted debug system)
        /// </summary>
        public string WebApiPortionOfUrl { get; set; }

        public string JWT { get; set; }
    }
}
