using System.Collections.Generic;
using technology.tabula;
using UCLID_COMUTILSLib;
using static Extract.AttributeFinder.Tabula.TabulaUtils;

namespace Extract.AttributeFinder.Tabula
{
    /// <summary>
    /// Convert tables to matrices of lists of letters
    /// </summary>
    public sealed class TablesToLetters : ITabulaTableProcessor<List<LetterStruct>[][]>
    {
        #region ITabulaTableProcessor

        /// <summary>
        /// Transform a collection of tables on a page
        /// </summary>
        /// <param name="page">The collection of tables to process</param>
        /// <param name="spatialPageInfos">Dimension and rotation info about the image,
        /// or at least the page represented by <see paramref="page"/></param>
        /// <returns>A collection of <see cref="List{LetterStruct}"/> matrices</returns>
        public IEnumerable<List<LetterStruct>[][]> ProcessTables(TabulaTablesForPage page, LongToObjectMap spatialPageInfos)
        {
            var result = new List<List<LetterStruct>[][]>();
            foreach (var table in page.Tables)
            {
                var rowCount = table.getRowCount();
                if (rowCount == 0)
                {
                    continue;
                }
                var tableMatrix = new List<LetterStruct>[rowCount][];
                var colCount = table.getColCount();

                int rowIdx = 0;
                foreach (var row in table.getRows().ToEnumerable<java.util.List>())
                {
                    tableMatrix[rowIdx] = new List<LetterStruct>[colCount];
                    int colIdx = 0;
                    foreach (var cell in row.ToEnumerable<RectangularTextContainer>())
                    {
                        java.util.List textElementsOrChunks = cell.getTextElements();
                        List<TextElement> text = GetTextElements(textElementsOrChunks);
                        tableMatrix[rowIdx][colIdx] = GetLettersFromTextElements(text, (ushort)page.PageNumber, page.PdfToImageWidth, page.PdfToImageHeight);
                        colIdx++;
                    }
                    rowIdx++;
                }
                result.Add(tableMatrix);
            }
            return result;
        }

        #endregion ITabulaTableProcessor
    }
}
