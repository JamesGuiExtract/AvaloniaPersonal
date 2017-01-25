using System;
using System.IO;                        // FileStream
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;     // IHostingEnvironment

using UCLID_FILEPROCESSINGLib;
using static FileAPI_VS2017.Utils;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FileAPI_VS2017.Controllers
{
    /// <summary>
    /// FileItemcontroller class
    /// </summary>
    [Route("api/[controller]")]
    public class FileItemController : Controller
    {
        private IFileItemRepository FileItems { get; set; }
        private IHostingEnvironment Environment { get; set; }

        private FileProcessingDB _fileProcessingDB = null;

        /// <summary>
        /// FileItemController
        /// </summary>
        /// <param name="fileItems">The interface of the FileItemRepository is dependency injected into this CTOR</param>
        /// <param name="env">The hosting environment is dependency injected into this CTOR</param>
        public FileItemController(IFileItemRepository fileItems, IHostingEnvironment env)
        {
            FileItems = fileItems;
            Environment = env;


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
        /// Gets a list of all currently known file items.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<FileItem> GetAll()
        {
            Log.WriteLine("HttpGet (all) request...");
            var items = FileItems.GetAll();
            foreach (var item in items)
            {
                Log.WriteLine(Inv($"returning: {item.ToString()}"));
            }

            return items;
        }

        /// <summary>
        /// Gets the specified FileItem instance by Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetFileItem")]
        public IActionResult GetById(int id)
        {
            Log.WriteLine(Inv($"HttpGet (index: {id}) request..."));

            var item = FileItems.Find(id);
            if (item == null)
            {
                Log.WriteLine(Inv($"Nothing found for index: {id}"));
                return NotFound();
            }

            Log.WriteLine(Inv($"Index: {id}, file item: {item.ToString()}"));
            return new ObjectResult(item);
        }

        /// <summary>
        /// Upload 1 to N files.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            Log.WriteLine("HttpPost");
            FileItem item = null;

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

                var fileName = Request.Headers["X-FileName"];
                var fileType = Request.Headers["X-ContentType"];
                var fullPath = $"{uploads}\\{fileName}";
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
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(Inv($"Error: {ex.Message}"));
                    }

                    Log.WriteLine("Updating file item list");
                    // Get a new ID; Count() will return the size which can be used as the next index.
                    int id = FileItems.Count();
                    long size = Request.ContentLength ?? 0;
                    item = new FileItem { Id = id, Name = fileName, Size = size };
                    FileItems.Update(item);

                    Log.WriteLine("Done");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(Inv($"Error: {ex.Message}"));
            }

            if (item == null)
            {
                Log.WriteLine("File item list is empty, returning Not Found");
                return NotFound();
            }

            // Note: The created-at-route name matches the name of the HttpGet method, in this case "GetFileItem".
            // This allows the returned header to display a Location element containing the URL that can be used to
            // invoke the HttpGet method to retrieve the (last) newly created object, in this case something like:
            // http://localhost:33677/api/FileItem/2
            return CreatedAtRoute("GetFileItem", new { id = item.Id }, item);
        }


        /// <summary>
        /// updates an existing file item
        /// </summary>
        /// <param name="id">numeric identifier of the file item (must match an existing fileitem)</param>
        /// <param name="item">A FileItem by value</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] FileItem item)
        {
            Log.WriteLine("HttpPut");

            if (item == null || item.Id != id)
            {
                Log.WriteLine("Bad request");
                return BadRequest();
            }

            var fileItem = FileItems.Find(id);
            if (fileItem == null)
            {
                Log.WriteLine("Not found");
                return NotFound();
            }

            FileItems.Update(item);
            Log.WriteLine(Inv($"item updated: {item.ToString()}"));

            return new NoContentResult();
        }

        /// <summary>
        /// Deletes a FileItem from the list.
        /// </summary>
        /// <param name="id">Numeric identifier (must be present)</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Log.WriteLine("HttpDelete");

            var fileItem = FileItems.Find(id);
            if (fileItem == null)
            {
                Log.WriteLine("Not found");
                return NotFound();
            }

            FileItems.Remove(id);
            Log.WriteLine(Inv($"Removed item: {id}"));

            return new NoContentResult();
        }
    }
}
