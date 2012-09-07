using DexFlow.Client;
using DexFlow.Framework;
using Dx_DocContent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace NetDMSExporter
{
    /// <summary>
    /// Provides logic to export document from NetDMS work items for processing by Extract Systems
    /// software.
    /// </summary>
    public static class NetDMSDocumentExporter
    {
        /// <summary>
        /// Exports the documents from the specified <see paramref="workItem"/> to a folder
        /// hierarchy under the specified path. The path of each exported document will be:
        /// [exportPath]\[ProjectISN]\[ParcelISN]-[NodeISN]\[DocISN].tif
        /// Additionally, each [ParcelISN] folder will have an index.txt file listing all the
        /// exported filenames.
        /// </summary>
        /// <param name="taskClient">The <see cref="TaskClient"/> to use for the export.</param>
        /// <param name="workItem">The <see cref="IWorkItem"/> for which documents should be
        /// exported.</param>
        /// <param name="exportPath">The path under which to export files.</param>
        // Since this assembly needs to be compiled in .Net 3.0, I can't reference Extract.dll to
        // use Extract exceptions.
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public static void Export(TaskClient taskClient, IWorkItem workItem, string exportPath)
        {
            List<string> exportedDocuments = new List<string>();

            string parcelExportPath = Path.Combine(exportPath,
                workItem.Parcel.Project.ISN.ToString(CultureInfo.InvariantCulture));
            parcelExportPath = Path.Combine(parcelExportPath, 
                workItem.Parcel.ISN.ToString(CultureInfo.InvariantCulture) + "-" +
                workItem.Parcel.NodeISN.ToString(CultureInfo.InvariantCulture));

            foreach (IDocument document in workItem.Documents)
            {
                DocumentContent docContent =
                    new DocumentContent(taskClient, document.ContentID, true);
                try
                {
                    if (!Directory.Exists(parcelExportPath))
                    {
                        Directory.CreateDirectory(parcelExportPath);
                    }

                    string exportFileName =
                        document.ISN.ToString(CultureInfo.InvariantCulture) + ".tif";
                    string exportFullPath = Path.Combine(parcelExportPath, exportFileName);
                    int pageCount;
                    docContent.ExportToMultiTIFF(exportFullPath, out pageCount);
                    exportedDocuments.Add(exportFileName);
                }
                finally
                {
                    docContent.Close();
                }
            }

            string parcelIndexFile = Path.Combine(parcelExportPath, "Index.txt");
            File.WriteAllLines(parcelIndexFile, exportedDocuments.ToArray());
        }
    }
}
