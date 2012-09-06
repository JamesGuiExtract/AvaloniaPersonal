using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using ScriptingSupport.Common;
using DexFlow.Framework;
using Dx_DocContent;
using WorkflowContext;
using System.CodeDom.Compiler;

namespace ScriptingSupport
{
    /// <summary>
    /// This class is not compiled/used. The export script is used as a MethodBody type meaning the
    /// code following the comment below can be dropped in used as the export script in NetDMS.
    /// This is merely an example of a script that could be used to export the document; the script
    /// can obviously be modified as necessary to suit the customer needs.
    /// </summary>
    public class AcpScriptClass : ScriptingSupport.Common.ScriptBase
    {
        public override void ProcessWorkItem(IWorkItem workItem, ScriptParametersBase parameters)
        {
            // <<< The following block can be used a script in NetDMS >>>
            foreach (IDocument document in workItem.Documents)
            {
                Dx_DocContent.DocumentContent docContent = new Dx_DocContent.DocumentContent(taskClient, document.ContentID, true);
                try
                {
                    string exportDirectory = "C:\\ID Shield Input\\" + document.Project.ISN.ToString() + "\\" + document.ParcelISN.ToString();
                    if (!Directory.Exists(exportDirectory))
                    {
                        Directory.CreateDirectory(exportDirectory);
                    }
                    string exportedFileName = exportDirectory + "\\" + document.ISN.ToString() + ".tif";
                    int pageCount;
                    docContent.ExportToMultiTIFF(exportedFileName, out pageCount);
                }
                finally
                {
                    docContent.Close();
                }
            }
            // <<< End MethodBody script >>>
        }
    }
}
