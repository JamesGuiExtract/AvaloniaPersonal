using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using static Extract.AttributeFinder.Tabula.TabulaUtils;

namespace Extract.AttributeFinder.Tabula
{
    /// <summary>
    /// Convert tables to formatted spatial strings
    /// </summary>
    public sealed class TablesToSpatialString : ITabulaTableProcessor<SpatialString>
    {
        readonly bool _byRow;
        readonly List<LetterStruct> _newLine = GetNonSpatialLetters("\r\n");
        readonly List<LetterStruct> _tablePrefix = GetNonSpatialLetters("START-TABLE");
        readonly List<LetterStruct> _tableSuffix = GetNonSpatialLetters("END-TABLE");
        readonly List<LetterStruct> _colSeparator;
#pragma warning disable IDE0044 // Add readonly modifier
        List<LetterStruct> _rowPrefix = GetNonSpatialLetters("START-ROW");
        List<LetterStruct> _rowSuffix = GetNonSpatialLetters("END-ROW");
        List<LetterStruct> _colPrefix = GetNonSpatialLetters("START-COLUMN");
        List<LetterStruct> _colSuffix = GetNonSpatialLetters("END-COLUMN");
#pragma warning restore IDE0044 // Add readonly modifier

        #region Constructors

        /// <summary>
        /// Create instance
        /// </summary>
        /// <param name="byRow">Whether row is the first dimension of the table</param>
        /// <param name="secondarySeparator">The string to use for separating the second dimension,
        /// e.g., the columns if <see paramref="byRow"/> is <c>true</c></param>
        public TablesToSpatialString(bool byRow, string secondarySeparator = "\t")
        {
            _byRow = byRow;
            if (!_byRow)
            {
                UtilityMethods.Swap(ref _rowPrefix, ref _colPrefix);
                UtilityMethods.Swap(ref _rowSuffix, ref _colSuffix);
            }
            _colSeparator = GetNonSpatialLetters(secondarySeparator);
        }

        #endregion Constructors

        #region ITabulaTableProcessor

        /// <summary>
        /// Transform a collection of tables on a page
        /// </summary>
        /// <param name="page">The collection of tables to process</param>
        /// <param name="spatialPageInfos">Dimension and rotation info about the image,
        /// or at least the page represented by <see paramref="page"/></param>
        /// <returns>A collection of formatted <see cref="SpatialString"/>s</returns>
        public IEnumerable<SpatialString> ProcessTables(TabulaTablesForPage page, LongToObjectMap spatialPageInfos)
        {
            try
            {
                return new TablesToLetters().ProcessTables(page, spatialPageInfos)
                    .Select((List<LetterStruct>[][] table) => ProcessTable(table, spatialPageInfos, page.SourceDocName))
                    .Where((SpatialString table) => table.HasSpatialInfo());
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49505");
            }
        }

        #endregion ITabulaTableProcessor

        #region Private Methods

        private SpatialString ProcessTable(List<LetterStruct>[][] table, LongToObjectMap spatialPageInfos, string sourceDocName)
        {
            List<LetterStruct> letters = new List<LetterStruct>();
            var rowCount = table.Length;
            var colCount = table[0].Length;
            if (!_byRow)
            {
                UtilityMethods.Swap(ref rowCount, ref colCount);
            }

            if (letters.Any())
            {
                letters.AddRange(_newLine);
            }
            letters.AddRange(_tablePrefix);
            letters.AddRange(_newLine);

            for (int rowIdx = 0; rowIdx < rowCount; rowIdx++)
            {
                letters.AddRange(_rowPrefix);
                letters.AddRange(_newLine);
                for (int colIdx = 0; colIdx < colCount; colIdx++)
                {
                    if (colIdx > 0)
                    {
                        letters.AddRange(_colSeparator);
                    }
                    if (_byRow)
                    {
                        letters.AddRange(table[rowIdx][colIdx]);
                    }
                    else
                    {
                        letters.AddRange(table[colIdx][rowIdx]);
                    }
                }
                letters.AddRange(_newLine);
                letters.AddRange(_rowSuffix);
                letters.AddRange(_newLine);
            }
            letters.AddRange(_tableSuffix);
            return SpatialStringMethods.CreateFromLetters(letters, sourceDocName, spatialPageInfos);
        }

        #endregion Private Methods
    }
}
