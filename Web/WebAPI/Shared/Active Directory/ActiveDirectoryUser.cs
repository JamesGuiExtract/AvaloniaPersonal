using System;
using System.Collections.Generic;

namespace WebAPI
{
    public class ActiveDirectoryUser
    {
        /// <summary>
        /// Gets or sets the active directory user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the collection of active directory groups.
        /// </summary>
        public IList<string> ActiveDirectoryGroups { get; set; }

        /// <summary>
        /// Gets or sets the last time this information was updated.
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}
