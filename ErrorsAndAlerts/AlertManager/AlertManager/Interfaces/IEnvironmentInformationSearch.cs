using AlertManager.Models.AllDataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager.Interfaces
{
    /// <summary>
    /// Elastic search query class for environment information.
    /// </summary>
    public interface IEnvironmentInformationSearch
    {
        /// <summary>
        /// Queries for an environment document in elastic search that has a given entry in its data dictionary.
        /// </summary>
        /// <param name="searchBackwardsFrom">Date and time of the alert or error. Query will find most recent document that is still before this time.</param>
        /// <param name="dataKeyName">Name of the entry to look for in the documents data dictionary.</param>
        /// <returns>List containing single best match EnvironmentInformation from query or empty list.</returns>
        List<EnvironmentInformation> TryGetInfoWithDataEntry(DateTime searchBackwardsFrom, string dataKeyName);

        /// <summary>
        /// Queries for an environment document in elastic search that has a given context type.
        /// </summary>
        /// <param name="searchBackwardsFrom">Date and time of the alert or error. Query will find most recent document that is still before this time.</param>
        /// <param name="contextType">Value for the context field of the desired document.</param>
        /// <returns>List containing single best match EnvironmentInformation from query or empty list.</returns>
        List<EnvironmentInformation> TryGetInfoWithContextType(DateTime searchBackwardsFrom, string contextType);
    }
}
