using System;
using System.Collections.Generic;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Tabula
{
    /// <summary>
    /// Interface to inject behavior into <see cref="TabulaUtility"/>
    /// </summary>
    /// <typeparam name="TResult">The item type of the collection returned by <see cref="ProcessTables"/></typeparam>
    [CLSCompliant(false)]
    public interface ITabulaTableProcessor<TResult>
    {
        /// <summary>
        /// Transform a collection of tables on a page
        /// </summary>
        /// <param name="page">The collection of tables to process</param>
        /// <param name="spatialPageInfos">Dimension and rotation info about at least the page represented by <see paramref="page"/></param>
        /// <returns>A collection of <typeparamref name="TResult"/>s</returns>
        IEnumerable<TResult> ProcessTables(TabulaTablesForPage page, LongToObjectMap spatialPageInfos);
    }
}
