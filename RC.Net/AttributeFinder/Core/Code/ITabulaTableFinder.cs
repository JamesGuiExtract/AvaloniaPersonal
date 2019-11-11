using System;
using System.Collections.Generic;
using technology.tabula;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Interface to inject behavior into various <see cref="TabulaUtils"/> methods
    /// </summary>
    [CLSCompliant(false)]
    public interface ITabulaTableFinder
    {
        /// <summary>
        /// Find tables on a page area
        /// </summary>
        /// <param name="pageArea">The region to search</param>
        /// <returns>A collection of <see cref="Table"/>s</returns>
        IEnumerable<Table> GetTablesFromPageArea(Page pageArea);
    }
}
