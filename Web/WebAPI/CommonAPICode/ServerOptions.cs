using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI
{
    /// <summary>
    /// Web API server options
    /// </summary>
    public class ServerOptions
    {
        /// <summary>
        /// The database server to connect too
        /// </summary>
        public string DatabaseServer { get; set; }

        /// <summary>
        /// The database to use
        /// </summary>
        public string DatabaseName { get; set; }
    }
}
