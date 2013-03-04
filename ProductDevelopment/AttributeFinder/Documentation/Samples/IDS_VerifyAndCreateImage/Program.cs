using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Extract;
using Extract.Redaction;
using Extract.Redaction.Verification;
using Extract.Utilities;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_REDACTIONCUSTOMCOMPONENTSLib;

namespace VerifyAndCreateImage
{
    class Program
    {
        // Just paste the password string from the file into this array
        static readonly int[] licKeys = new int[] { 0, 0, 0, 0 };
        const string licFile = @"C:\test\test.lic";

        static void Main(string[] args)
        {
            VerificationTask verify = null;
            try
            {
                // Initialize the license
                var license = new UCLID_COMLMLib.UCLIDComponentLM();
                license.InitializeFromFile(licFile,
                    licKeys[0], licKeys[1], licKeys[2], licKeys[3]);

                // Files to verify
                var imageFiles = new string[]
                {
                    @"C:\Demo_IDShield\Input\TestImage001.tif",
                    @"C:\Demo_IDShield\Input\TestImage002.tif",
                    @"C:\Demo_IDShield\Input\TestImage003.tif"
                };

                string voaFile = "<SourceDocName>.voa";
                string xmlFile = "<SourceDocName>.xml";

                // General settings - require all pages, do not require
                // types, do not require exemption codes
                var generalVerifySettings = new GeneralVerificationSettings(true,
                    true, true, false, true, true);

                // Verification settings -
                // GeneralVerificationSettings,
                // FeedbackSettings,
                // InputFile - voa file to process, this can contain document tags
                // useBackDropImage - If true, then another image name (can contain document tags
                //     must be specified to use as the displayed image in verification instead
                //     of the current processing file (<SourceDocName>)
                // SetFileActionStatusSettings - this must be null if not using the Extract FAM database
                // enableInputEventTracking - this must be false if not using the Extract FAM database
                var verifySettings = new VerificationSettings(generalVerifySettings, null, voaFile,
                    false, "", null, false, false, new SlideshowSettings());

                // A tag manager must be configured to resolve any document tags in the file names
                // In order to resolve the tag <FPSFileDir>, the FPSFileDir must be set, if
                // the <FPSFileDir> tag is not being used, this value can be set to empty string
                var tagManager = new FAMTagManager();
                tagManager.FPSFileDir = "";

                // Create the verification task, copy in the settings and initialize it
                // NOTE: The init call is what displays the verification UI. In order to
                //       close the UI, Close must be called on the task. It is recommended to
                //       use a try/finally block to ensure the Close is called. If this is not
                //       called it can lead to memory leaks (the Dispose() call for the form and
                //       event handles/threads/etc will not be called until Close is called).
                verify = new VerificationTask();
                verify.Settings = verifySettings;
                verify.Init(0, tagManager, null);

                foreach (var imageFile in imageFiles)
                {
                    FileRecord fileRecord = new FileRecord();
                    fileRecord.ActionID = 0;
                    fileRecord.FileID = 0;
                    fileRecord.Name = imageFile;

                    // Present the image in the verification UI
                    var result = verify.ProcessFile(fileRecord, 0, tagManager, null, null, false);

                    // The result is an EFileProcessingResult that is one of the following values:
                    // kProcessingSuccessful - User clicked the save button in verification
                    // kProcessingCancelled - This would only come from the UI if running from the
                    //     File Action Manager (processfiles.exe), you should not see this result
                    //     when displaying the UI from code like this
                    // kProcessingSkipped - The user chose the skip option in the file menu in
                    //     the verification UI
                    if (result == EFileProcessingResult.kProcessingSuccessful)
                    {
                        // In order to produce a redacted image, the types of data to redact
                        // must be specified. Typically a VOA file output from verification
                        // will contain HCData, MCData, LCData, Manual - these correspond
                        // to high confidence, medium confidence, low confidence, and manually
                        // added data items. Typical setup following a verification task is
                        // to redact all HC, MC, LC, and Manual data. If the user does not wish
                        // to redact the LCData items they can turn those off in the verification
                        // window
                        // Redact HC, MC, LC, and Manual data
                        var attributesToRedact = new VariantVector();
                        attributesToRedact.PushBack("HCData");
                        attributesToRedact.PushBack("MCData");
                        attributesToRedact.PushBack("LCData");
                        attributesToRedact.PushBack("Manual");

                        IFileProcessingTask redactTask = null;
                        MetadataTask metadataTask = null;
                        try
                        {
                            // The redaction task is the task used to create a redacted image
                            var createImage = new RedactionTask();

                            // OutputFileName is the file that will be created, this can contain
                            // document tags. $InsertBeforeExt(<SourceDocName>,.redacted) will
                            // create an output image that is the original file name with the
                            // word .redacted place between the file name and extension:
                            // Example: <SourceDocName> = C:\test\images\123.tif
                            //          OutputImage = C:\test\images\123.redacted.tif
                            createImage.OutputFileName = "$InsertBeforeExt(<SourceDocName>,.redacted)";

                            // The VOA file name should match the file name set in the verification task
                            createImage.VOAFileName = voaFile;

                            // Fille color is the color in the redaction, border color is the color
                            // that is around the border of the redaction
                            createImage.FillColor = Color.Black.ToArgb();
                            createImage.BorderColor = Color.Black.ToArgb();

                            // Set the attributes to redact
                            createImage.AttributeNames = attributesToRedact;

                            // The redaction task needs to be cast to an IFileProcessingTask
                            // as it is an explicit COM implementation of the interface
                            redactTask = createImage as IFileProcessingTask;

                            // Init the task and create the redacted image
                            redactTask.Init(0, tagManager, null);
                            redactTask.ProcessFile(fileRecord, 0, tagManager, null, null, false);

                            metadataTask = new MetadataTask();

                            metadataTask.DataFile = voaFile;
                            metadataTask.MetadataFile = xmlFile;

                            metadataTask.Init(0, tagManager, null);
                            metadataTask.ProcessFile(fileRecord, 0, tagManager, null, null, false);
                        }
                        finally
                        {
                            if (redactTask != null)
                            {
                                redactTask.Close();
                            }

                            if (metadataTask != null)
                            {
                                metadataTask.Close();
                            }
                        }
                    }
                    else if (result == EFileProcessingResult.kProcessingSkipped)
                    {
                        Console.WriteLine("Skipped: " + imageFile);
                    }
                    else if (result == EFileProcessingResult.kProcessingCancelled)
                    {
                        Console.WriteLine("Closed without saving: " + imageFile);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // With the extract exception class, you can display exceptions in a UI,
                // you can log the exception to a specified location, or you can log
                // the exceptions to the default extract log location
                ExtractException.Display("Error: 002", ex);
                //ExtractException.Log("Error: 002", ex);
                //ExtractException.Log(@"C:\test\CustomLogFile.uex", "Error: 002", ex);
            }
            finally
            {
                if (verify != null)
                {
                    verify.Close();
                    verify = null;
                }
            }

            Console.ReadLine();
        }
    }
}
