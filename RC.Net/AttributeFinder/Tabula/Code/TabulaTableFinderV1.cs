using System;
using System.Collections.Generic;
using technology.tabula;
using technology.tabula.detectors;
using technology.tabula.extractors;

namespace Extract.AttributeFinder.Tabula
{
    /// <summary>
    /// Recreate algorithm of Tabula.IKVM.exe
    /// </summary>
    [CLSCompliant(false)]
    public sealed class TabulaTableFinderV1 : ITabulaTableFinder
    {
        readonly NurminenDetectionAlgorithm _detector = new NurminenDetectionAlgorithm();
        readonly SpreadsheetExtractionAlgorithm _spreadsheetExtractor = new SpreadsheetExtractionAlgorithm();
        readonly BasicExtractionAlgorithm _basicExtractor = new BasicExtractionAlgorithm();

        public IEnumerable<Table> GetTablesFromPageArea(Page pageArea)
        {
            // TODO: It seems like this test should be done once per guess but the cmdline app does it once/page...
            ExtractionAlgorithm extractionAlgorithm = _spreadsheetExtractor;
            try
            {
                if (!_spreadsheetExtractor.isTabular(pageArea))
                {
                    extractionAlgorithm = _basicExtractor;
                }
            }
            catch { } // TODO: Fix java code (It looks like there would be an exception when the spreadsheet alg finds tables and the basic alg finds none)

            var guesses = _detector.detect(pageArea).ToEnumerable<Rectangle>();
            foreach(var rectangle in guesses)
            {
                Page guess = pageArea.getArea(rectangle);
                foreach (var table in extractionAlgorithm.extract(guess).ToEnumerable<Table>())
                {
                    yield return table;
                }
            }
        }
    }
}
