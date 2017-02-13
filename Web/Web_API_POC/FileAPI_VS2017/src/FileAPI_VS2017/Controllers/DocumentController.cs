using System;
using System.IO;                        // FileStream
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;     // IHostingEnvironment

using FileAPI_VS2017.Models;
using UCLID_FILEPROCESSINGLib;
using static FileAPI_VS2017.Utils;

namespace FileAPI_VS2017.Controllers
{
    /// <summary>
    /// args DTO for SubmitText
    /// </summary>
    public class SubmitTextArgs
    {
        /// <summary>
        /// text argument, required
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// WorkflowName - optional, if not specified then default workflow for user is used
        /// </summary>
        public string WorkflowName { get; set; }
    }

    /// <summary>
    /// The Document API class
    /// </summary>
    [Route("api/[controller]")]
    public class DocumentController : Controller
    {
        private IFileItemRepository FileItems { get; set; }
        private IHostingEnvironment Environment { get; set; }

        private FileProcessingDB _fileProcessingDB = null;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="fileItems"></param>
        /// <param name="env"></param>
        public DocumentController(IFileItemRepository fileItems, IHostingEnvironment env)
        {
            FileItems = fileItems;
            Environment = env;

            // TODO - remove all of this from the CTOR
            try
            {
                //LicenseUtilities.LoadLicenseFilesFromFolder(licenseType: 0, mapLabel: new MapLabel());
                //LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI9999999999", "Web FILE API");

                FAMDBUtils dbUtils = new FAMDBUtils();
                Type mgrType = Type.GetTypeFromProgID(dbUtils.GetFAMDBProgId());
                _fileProcessingDB = (FileProcessingDB)Activator.CreateInstance(mgrType);

                _fileProcessingDB.DatabaseServer = "(local)";
                _fileProcessingDB.DatabaseName = "Demo_LabDE";
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Gets result set for a submitted file that has finished processing
        /// </summary>
        /// <returns></returns>
        [HttpGet("/GetResultSet/{id}")]
        public DocumentAttributeSet GetResultSet(string id)
        {
            if (!ModelState.IsValid || String.IsNullOrEmpty(id))
            {
                return new DocumentAttributeSet
                {
                    Error = new ErrorInfo
                    {
                        ErrorOccurred = true,
                        Message = "id argument cannot be empty",
                        Code = -1
                    },

                    Attributes = null
                };
            }

            // For now, return a fake attribute set (DocumentAttributeSet)
            DocumentAttribute dc = new DocumentAttribute
            {
                Name = "FlexData",
                Value = "",
                Type = "",
                AverageCharacterConfidence = 78,
                AttributeTypeOf = AttributeType.Data,
                RedactionConfidenceLevel = RedactionConfidence.NotApplicable,
                SpatialPosition = new Position
                {
                    PageNumber = 1,
                    LineInfo = new SpatialLine
                    {
                        Zone = new SpatialLineZone
                        {
                            Start = new System.Drawing.Point { X = 0, Y = 0 },
                            End = new System.Drawing.Point { X = 250, Y = 1 },
                            Height = 200
                        },

                        Bounds = new SpatialLineBounds
                        {
                            TopLeft = new System.Drawing.Point { X = 0, Y = 0 },
                            BottonRight = new System.Drawing.Point { X = 250, Y = 200 }
                        }
                    }
                }
            };

            DocumentAttributeSet das = new DocumentAttributeSet
            {
                Error = new ErrorInfo(),
                Attributes = new List<DocumentAttribute>() { dc }
            };

            return das;
        }

        private string GetSafeFilename(string path, string filename)
        {
            string fullname = Path.Combine(path, filename);
            if (!System.IO.File.Exists(fullname))
            {
                return fullname;
            }

            string nameOnly = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);
            string guid = Guid.NewGuid().ToString();
            string newName = $"{nameOnly}_{ guid}{ extension}";
            string fullfilename = Path.Combine(path, newName);

            return fullfilename;
        }

        /// <summary>
        /// Upload 1 to N files for document processing
        /// </summary>
        /// <returns></returns>
        [HttpPost("/SubmitFile")]
        public async Task<IActionResult> SubmitFile()
        {
            Log.WriteLine("HttpPost");

            try
            {
                Log.WriteLine("checking for uploads path");

                string path = String.IsNullOrEmpty(Environment.WebRootPath) ? "c:\\temp\\fileApi" : Environment.WebRootPath;
                Log.WriteLine($"Path: {path}");

                var uploads = Path.Combine(path, "uploads");
                if (!Directory.Exists(uploads))
                {
                    Log.WriteLine(Inv($"Creating path: {uploads}"));
                    Directory.CreateDirectory(uploads);
                }

                string fileName = Request.Headers["X-FileName"];
                var fullPath = GetSafeFilename(uploads, fileName);
                Log.WriteLine(Inv($"Writing file: {fullPath}"));

                using (var fs = new FileStream(fullPath, FileMode.Create))
                {
                    await Request.Body.CopyToAsync(fs);
                    Log.WriteLine("Done writing file");

                    // Now add the file to the FAM queue
                    try
                    {
                        if (_fileProcessingDB != null)
                        {
                            Log.WriteLine("Adding file to FAM queue");
                            bool bAlreadyExists;
                            UCLID_FILEPROCESSINGLib.EActionStatus previousActionStatus;

                            _fileProcessingDB.AddFile(fullPath,                                                 // full path to file
                                                      "A01_ExtractData",                                        // action name
                                                      EFilePriority.kPriorityNormal,                            // file priority
                                                      false,                                                    // force status change
                                                      false,                                                    // file modified
                                                      UCLID_FILEPROCESSINGLib.EActionStatus.kActionPending,     // action status
                                                      false,                                                    // skip page count
                                                      out bAlreadyExists,                                       // returns whether file already existed
                                                      out previousActionStatus);                                // returns the previous action status (if file already existed)

                            // TODO - need to get the FileID from FAM
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(Inv($"Error: {ex.Message}"));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Error: {ex.Message}"));
            }

            return new ObjectResult("Ok");
        }

        /// <summary>
        /// submit text for processing
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [HttpPost("/SubmitText")]
        public IActionResult SubmitText([FromBody]SubmitTextArgs args)
        {
            Log.WriteLine("HttpPost");

            if (!ModelState.IsValid || String.IsNullOrEmpty(args.Text))
            {
                return BadRequest("args.Text is empty");
            }

            try
            {
                Log.WriteLine("checking for uploads path");

                string path = String.IsNullOrEmpty(Environment.WebRootPath) ? "c:\\temp\\fileApi" : Environment.WebRootPath;
                Log.WriteLine($"Path: {path}");

                var uploads = Path.Combine(path, "uploads");
                if (!Directory.Exists(uploads))
                {
                    Log.WriteLine(Inv($"Creating path: {uploads}"));
                    Directory.CreateDirectory(uploads);
                }

                var fullPath = GetSafeFilename(uploads, "SubmittedText.txt");
                Log.WriteLine(Inv($"Writing file: {fullPath}"));

                using (var fs = new FileStream(fullPath, FileMode.Create))
                {
                    byte[] text = Encoding.ASCII.GetBytes(args.Text);
                    fs.Write(text, 0, args.Text.Length);
                    fs.Close();
                    Log.WriteLine("Done writing file");

                    // Now add the file to the FAM queue
                    try
                    {
                        if (_fileProcessingDB != null)
                        {
                            Log.WriteLine("Adding file to FAM queue");
                            bool bAlreadyExists;
                            UCLID_FILEPROCESSINGLib.EActionStatus previousActionStatus;

                            _fileProcessingDB.AddFile(fullPath,                                                 // full path to file
                                                      "A01_ExtractData",                                        // action name
                                                      EFilePriority.kPriorityNormal,                            // file priority
                                                      false,                                                    // force status change
                                                      false,                                                    // file modified
                                                      UCLID_FILEPROCESSINGLib.EActionStatus.kActionPending,     // action status
                                                      false,                                                    // skip page count
                                                      out bAlreadyExists,                                       // returns whether file already existed
                                                      out previousActionStatus);                                // returns the previous action status (if file already existed)

                            // TODO - need to get the FileID from FAM
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(Inv($"Error: {ex.Message}"));
                    }

                    Log.WriteLine("Done");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Error: {ex.Message}"));
            }

            return new ObjectResult("Ok");  // TODO - need to send back a fileID here
        }

        /// <summary>
        /// get a list of 1..N processing status instances that corespond to the stringId of the submitted document
        /// </summary>
        /// <param name="stringId"></param>
        /// <returns></returns>
        [HttpGet("/GetStatus")]
        public List<ProcessingStatus> GetStatus([FromQuery] string stringId)
        {
            if (String.IsNullOrEmpty(stringId))
            {
                return MakeListProcessingStatus(isError: true, 
                                                message: "stringId argument is empty", 
                                                status: DocumentProcessingStatus.Failed, 
                                                code: -1);
            }

            return MakeListProcessingStatus(isError: false,
                                            message: "",
                                            status: DocumentProcessingStatus.Processing);
        }

        // Add File GetFileResult(string fileId)
        /// <summary>
        /// Gets a result file for the specified input document
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpGet("/GetFileResult")]
        public byte[] GetFileResult([FromQuery] string fileId)
        {
            return new byte[10];
        }

        // Add GetTextResult
        // TODO - may unify the Get????Result() methods, why not?
        /// <summary>
        /// Gets a text result for a specified input document
        /// </summary>
        /// <param name="textId"></param>
        /// <returns></returns>
        [HttpGet("/GetTextResult")]
        public byte[] GetTextResult([FromQuery] string textId)
        {
            return new byte[10];
        }







    }
}
