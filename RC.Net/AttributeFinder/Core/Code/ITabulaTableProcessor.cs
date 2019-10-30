using System;
using System.Collections.Generic;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Interface to inject behavior into 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    [CLSCompliant(false)]
    public interface ITabulaTableProcessor<TResult>
    {
        IEnumerable<TResult> ProcessTables(TabulaTablesForPage page, LongToObjectMap spatialPageInfos);
    }
}
