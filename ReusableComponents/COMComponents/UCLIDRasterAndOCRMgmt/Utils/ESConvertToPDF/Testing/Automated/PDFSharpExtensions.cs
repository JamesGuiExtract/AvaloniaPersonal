using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ESConvertToPDF.Test
{
    /// <summary>
    /// Adapted from: https://stackoverflow.com/questions/10141143/c-sharp-extract-text-from-pdf-using-pdfsharp
    /// </summary>
    static class PDFSharpExtensions
    {
        public static string GetPageText(this PdfDocument pdfDocument, int pageNumber)
        {
            return pdfDocument.Pages[pageNumber - 1].GetText();
        }

        public static string GetText(this PdfPage page)
        {
            var content = ContentReader.ReadContent(page);
            var text = string.Concat(content.GetWords());
            return text;
        }

        public static IEnumerable<string> GetWords(this CObject cObject)
        {
            if (cObject is COperator)
            {
                var cOperator = cObject as COperator;
                if (cOperator.OpCode.Name == OpCodeName.Tj.ToString() ||
                    cOperator.OpCode.Name == OpCodeName.TJ.ToString())
                {
                    foreach (var cOperand in cOperator.Operands)
                        foreach (var txt in GetWords(cOperand))
                            yield return txt;
                }
            }
            else if (cObject is CSequence)
            {
                var cSequence = cObject as CSequence;
                foreach (var element in cSequence)
                    foreach (var txt in GetWords(element))
                        yield return txt;
            }
            else if (cObject is CString)
            {
                var cString = cObject as CString;
                yield return cString.Value;
            }
        }
    }
}
