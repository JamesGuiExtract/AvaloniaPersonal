using Microsoft.Office.Interop.Word;
using System;
using System.IO;

namespace Extract.FileConverter.Converters
{
    public class WordToPdfConverter : IConverter
    {
        /// <summary>
        /// Converts the input file to a PDF, and sets the name to the output file name.
        /// </summary>
        /// <param name="inputFile"></param>
        public void Convert(string inputFile)
        {
            try
            {
                // Create a new Microsoft Word application object
                Application word = new Application();

                // C# doesn't have optional arguments so we'll need a dummy value
                object oMissing = System.Reflection.Missing.Value;

                word.Visible = false;
                word.ScreenUpdating = false;

                FileInfo wordFile = new FileInfo(inputFile);

                // Cast as Object for word Open method
                Object filename = (Object)wordFile.FullName;

                // Use the dummy value as a placeholder for optional arguments
                Document doc = word.Documents.Open(ref filename, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                doc.Activate();

                object fileFormat = WdSaveFormat.wdFormatPDF;

                object outputFileNames = wordFile.FullName.Replace(".doc", ".pdf");
                // Save document into PDF Format
                doc.SaveAs(ref outputFileNames,
                    ref fileFormat, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing);

                // Close the Word document, but leave the Word application open.
                // doc has to be cast to type _Document so that it will find the
                // correct Close method.                
                object saveChanges = WdSaveOptions.wdDoNotSaveChanges;
                ((_Document)doc).Close(ref saveChanges, ref oMissing, ref oMissing);
                doc = null;
            }
            catch(Exception ex)
            {
                ex.AsExtract("ELI51655");
            }
        }
    }
}
