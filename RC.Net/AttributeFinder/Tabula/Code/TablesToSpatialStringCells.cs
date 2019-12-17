using System.Collections.Generic;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.AttributeFinder.Tabula
{
    /// <summary>
    /// Convert tables to matrices of spatial strings
    /// </summary>
    public sealed class TablesToSpatialStringCells : ITabulaTableProcessor<SpatialString[][]>
    {
        #region ITabulaTableProcessor

        /// <summary>
        /// Transform a collection of tables on a page
        /// </summary>
        /// <param name="page">The collection of tables to process</param>
        /// <param name="spatialPageInfos">Dimension and rotation info about the image,
        /// or at least the page represented by <see paramref="page"/></param>
        /// <returns>A collection of <see cref="SpatialString"/> matrices</returns>
        public IEnumerable<SpatialString[][]> ProcessTables(TabulaTablesForPage page, LongToObjectMap spatialPageInfos)
        {
            var tables = new TablesToLetters().ProcessTables(page, spatialPageInfos)
                .Select(table =>
                    table.Select(row =>
                        row.Select(column =>
                            SpatialStringMethods.CreateFromLetters(column, page.SourceDocName, spatialPageInfos))
                            .ToArray()
                        ).ToArray()
                    ).ToList();
            return tables;
        }

        #endregion ITabulaTableProcessor
    }
}
