using System;
using System.Collections.Generic;
using technology.tabula;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Container for page info and tables
    /// </summary>
    [CLSCompliant(false)]
    public class TabulaTablesForPage
    {
        /// <summary>
        /// Create instance
        /// </summary>
        /// <param name="sourceDocName">The original input image or PDF</param>
        /// <param name="pageNumber">The page number represented by this instance</param>
        /// <param name="pdfToImageWidth">The factor to scale PDF X dimension to OCR X dimension</param>
        /// <param name="pdfToImageHeight">The factor to scale PDF Y dimension to OCR Y dimension</param>
        /// <param name="tables">The collection of tables found on this page</param>
        public TabulaTablesForPage(string sourceDocName, int pageNumber, float pdfToImageWidth, float pdfToImageHeight, IEnumerable<Table> tables)
        {
            SourceDocName = sourceDocName;
            PageNumber = pageNumber;
            PdfToImageWidth = pdfToImageWidth;
            PdfToImageHeight = pdfToImageHeight;
            Tables = tables;
        }

        /// <summary>
        /// The original input image or PDF</param>
        /// </summary>
        public string SourceDocName { get; }
        /// <summary>
        /// The page number represented by this instance</param>
        /// </summary>
        public int PageNumber { get; }
        /// <summary>
        /// The factor to scale PDF X dimension to OCR X dimension</param>
        /// </summary>
        public float PdfToImageWidth { get; }
        /// <summary>
        /// The factor to scale PDF Y dimension to OCR Y dimension</param>
        /// </summary>
        public float PdfToImageHeight { get; }
        /// <summary>
        /// The collection of tables found on this page</param>
        /// </summary>
        public IEnumerable<Table> Tables { get; }
    }
}
