using Extract.Licensing;
using System;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Application that allows searching and inspection of files in a File Action Manager database
    /// based on database conditions, OCR content and data content.
    /// </summary>
    static class FAMFileInspectorProgram
    {
        #region Constants

        /// <summary>
        /// The name of this object.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FAMFileInspectorProgram).ToString();
        
        #endregion Constants

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Load the license files from folder
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI35709", _OBJECT_NAME);

                var famFileInspectorForm = new FAMFileInspectorForm();

                if (famFileInspectorForm.FileProcessingDB.ShowSelectDB(
                        "Select database", false, false))
                {
                    Application.Run(famFileInspectorForm);    
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35710");
            }
        }
    }
}
