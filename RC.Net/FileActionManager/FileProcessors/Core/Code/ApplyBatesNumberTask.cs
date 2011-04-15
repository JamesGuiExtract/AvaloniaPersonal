using Extract.Drawing;
using Extract.Imaging;
using Extract.Imaging.Utilities;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Leadtools;
using Leadtools.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Represents a file processing task that applies Bates numbers to documents.
    /// </summary>
    [ComVisible(true)]
    [Guid("8DD63918-D299-48DB-BA54-FD1CFAAAF0E2")]
    [ProgId("Extract.FileActionManager.FileProcessors.ApplyBatesNumberTask")]
    public class ApplyBatesNumberTask : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFileProcessingTask, ILicensedComponent,
        IPersistStream, IDisposable
    {
        #region Constants

        /// <summary>
        /// The current version of the <see cref="ApplyBatesNumberTask"/>.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The description to be displayed in the categorized component selection list.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Core: Apply Bates number";

        /// <summary>
        /// The total number of times the apply bates number task will retry applying the
        /// Bates number.
        /// </summary>
        const int _MAX_RETRIES = 3;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="BatesNumberFormat"/> to use to apply Bates numbers.
        /// </summary>
        BatesNumberFormat _format = new BatesNumberFormat(true);

        /// <summary>
        /// The file to operate on.
        /// </summary>
        string _fileName;

        /// <summary>
        /// Indicates whether this task object is dirty or not
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Image codecs for encoding and decoding images.
        /// </summary>
        volatile ImageCodecs _codecs;

        /// <summary>
        /// Protects <see cref="_codecs"/>.
        /// </summary>
        readonly object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyBatesNumberTask"/> class.
        /// </summary>
        public ApplyBatesNumberTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyBatesNumberTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="ApplyBatesNumberTask"/> to initialize from.</param>
        public ApplyBatesNumberTask(ApplyBatesNumberTask task)
        {
            CopyFrom(task);
        }

        #endregion Constructors

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        /// <returns>The name of the COM object.</returns>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="ApplyBatesNumberTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Get a db manager and connect to the database
                FileProcessingDBClass databaseManager = new FileProcessingDBClass();
                databaseManager.ConnectLastUsedDBThisProcess();

                // Create a new BatesNumberGenerator
                using (BatesNumberGeneratorWithDatabase generator =
                    new BatesNumberGeneratorWithDatabase(_format, databaseManager))
                {

                    // Create the settings dialog
                    using (ApplyBatesNumberSettingsDialog dialog =
                        new ApplyBatesNumberSettingsDialog(generator, _fileName))
                    {

                        // Show the dialog and return whether the settings where modified or not
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            // Get the new format and file name from the dialog
                            Format = dialog.BatesNumberGenerator.Format;
                            FileName = dialog.FileName;

                            _dirty = true;

                            return true;
                        }

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI27888", "Error running configuration.", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks whether this object has been configured properly.
        /// </summary>
        /// <returns><see langword="true"/> if the object has been configured properly
        /// and <see langword="false"/> if it has not.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                // Get a db manager and connect to the database
                FileProcessingDBClass databaseManager = new FileProcessingDBClass();
                databaseManager.ConnectLastUsedDBThisProcess();

                // Create a FAM tag manager to check the file name tags
                FAMTagManagerClass manager = new FAMTagManagerClass();

                // This object is configured properly iff:
                // 1. There is a file name specified
                // 2. The file name contains only valid tags
                // 3. A database counter has been specified.
                // 4. The specified database counter is a valid counter in the current database.
                return !string.IsNullOrEmpty(_fileName)
                    && !manager.StringContainsInvalidTags(_fileName)
                    && !string.IsNullOrEmpty(_format.DatabaseCounter)
                    && databaseManager.IsUserCounterValid(_format.DatabaseCounter);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI27938",
                    "Failed while checking configuration.", ex);
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="ApplyBatesNumberTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="ApplyBatesNumberTask"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new ApplyBatesNumberTask(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI27889", "Unable to clone task.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="ApplyBatesNumberTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                CopyFrom(pObject as ApplyBatesNumberTask);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI27890", "Unable to copy the task.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="ApplyBatesNumberTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="ApplyBatesNumberTask"/> from which to copy.</param>
        public void CopyFrom(ApplyBatesNumberTask task)
        {
            try
            {
                ExtractException.Assert("ELI27891", "Task cannot be NULL.", task != null);

                // Use the property to ensure dispose is handled correctly for the format
                Format = task._format.Clone();

                _fileName = task._fileName;

                _dirty = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI27892", "Unable to copy the task.", ex);
            }
        }

        #endregion ICopyableObject Members 

        #region IFileProcessingTask Members

        /// <summary>
        /// Stops processing the current file.
        /// </summary>
        public void Cancel()
        {
            // Nothing to do (this task is not cancellable)
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            try
            {
                if (_codecs != null)
                {
                    lock (_lock)
                    {
                        if (_codecs != null)
                        {
                            _codecs.Dispose();
                            _codecs = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI28055", "Error closing task.", ex);
            }
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="pDB">The <see cref="FileProcessingDB"/> in use.</param>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB)
        {
            try
            {
                // Unlock the document support toolkit
                ExtractException ee = UnlockLeadtools.UnlockDocumentSupport(true);
                if (ee != null)
                {
                    throw ee;
                }

                // Ensure the database counter exists
                if (!pDB.IsUserCounterValid(_format.DatabaseCounter))
                {
                    var ee2 = new ExtractException("ELI27986",
                        "The user counter specified no longer exists in the database.");
                    ee2.AddDebugData("Counter Name", _format.DatabaseCounter, false);
                    throw ee2;
                }

                if (_codecs == null)
                {
                    lock (_lock)
                    {
                        if (_codecs == null)
                        {
                            _codecs = new ImageCodecs();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI28056", "Failed to initialize task.", ex);
            }
        }

        /// <summary>
        /// Processes the specified file.
        /// </summary>
        /// <param name="pFileRecord">The file record that contains the info of the file being 
        /// processed.</param>
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">A File Action Manager Tag Manager for expanding tags.</param>
        /// <param name="pDB">The File Action Manager database.</param>
        /// <param name="pProgressStatus">Object to provide progress status updates to caller.
        /// </param>
        /// <param name="bCancelRequested"><see langword="true"/> if cancel was requested; 
        /// <see langword="false"/> otherwise.</param>
        /// <returns><see langword="true"/> if processing should continue; <see langword="false"/> 
        /// if all file processing should be cancelled.</returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord,
            int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB,
            ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            string fileName = null;
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI27893",
                    _COMPONENT_DESCRIPTION);

                // Create a tag manager and expand the tags in the file name
                FileActionManagerPathTags tags = new FileActionManagerPathTags(
                    Path.GetFullPath(pFileRecord.Name), pFAMTM.FPSFileDir);
                fileName = Path.GetFullPath(tags.Expand(_fileName));

                // Apply the bates number based on the format settings
                using (BatesNumberGeneratorWithDatabase generator =
                    new BatesNumberGeneratorWithDatabase(_format, pDB))
                {
                    ApplyBatesNumbers(fileName, pProgressStatus, generator);
                }

                // If we reached this point then processing was successful
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                // Wrap the exception as an extract exception and add debug data
                ExtractException ee = ExtractException.AsExtractException("ELI27987", ex);
                if (fileName != null)
                {
                    ee.AddDebugData("File Being Processed", fileName, false);
                }
                ee.AddDebugData("File ID", pFileRecord.FileID, false);
                ee.AddDebugData("Action ID", nActionID, false);
                ee.AddDebugData("User Counter", string.IsNullOrEmpty(_format.DatabaseCounter)
                    ? "<Empty String>" : _format.DatabaseCounter, false);

                // Throw the extract exception as a COM visible exception
                throw ExtractException.CreateComVisible("ELI27894",
                    "Unable to process the file.", ee);
            }
        }
        
        #endregion IFileProcessingTask Members

        #region IAccessRequired Members

        /// <summary>
        /// Returns bool value indicating if the task requires admin access
        /// </summary>
        /// <returns><see langword="true"/> if the task requires admin access
        /// <see langword="false"/> if task does not require admin access</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(LicenseIdName.ExtractCoreObjects);
        }

        #endregion

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        /// <summary>
        /// Checks the object for changes since it was last saved.
        /// </summary>
        /// <returns><see cref="HResult.Ok"/> if changes have been made; 
        /// <see cref="HResult.False"/> if changes have not been made.</returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the <see cref="IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    // Read the file name from the stream
                    _fileName = reader.ReadString();

                    // Ensure the format is disposed if it exists
                    if (_format != null)
                    {
                        _format.Dispose();
                        _format = null;
                    }

                    // Read the new format from the stream
                    _format = reader.ReadObject<BatesNumberFormat>();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI27895",
                    "Unable to load object from stream", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> into which the object should be saved.
        /// </param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If 
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    // Serialize the settings
                    writer.Write(_fileName);
                    writer.WriteObject(_format);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI27896",
                    "Unable to save object to stream", ex);
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in 
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            size = HResult.NotImplemented;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID File Processors" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.FileProcessors);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID File Processors" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileProcessors);
        }

        /// <summary>
        /// Applies the Bates numbers to the specified image file.
        /// </summary>
        /// <param name="fileName">The file to apply the Bates numbers to.</param>
        /// <param name="progressStatus">The progress status to update.</param>
        /// <param name="generator">The <see cref="BatesNumberGeneratorWithDatabase"/>
        /// object to use to generate the Bates numbers.</param>
        void ApplyBatesNumbers(string fileName, ProgressStatus progressStatus,
            BatesNumberGeneratorWithDatabase generator)
        {
            // Ensure the file exists
            ExtractException.Assert("ELI27988", "File no longer exists.", File.Exists(fileName),
                "Image File Name", fileName);

            // Ensure IDisposable objects are disposed
            ImageReader reader = null;
            ImageWriter writer = null;
            int pageCount = 0;
            List<string> batesNumbers = null;
            TemporaryFile outputFile = null;
            try
            {
                // Get the file info along with the page information
                reader = _codecs.CreateReader(fileName);

                // Get the image information
                pageCount = reader.PageCount;
                RasterImageFormat format = reader.Format;

                if (progressStatus != null)
                {
                    progressStatus.InitProgressStatus("Applying Bates numbers...", 0,
                        (pageCount / 4) + 1, true);
                }

                // Create a temporary file to apply the Bates number to, copy back after
                // number is applied
                outputFile = new TemporaryFile(Path.GetExtension(fileName));

                // Generate bates numbers
                batesNumbers = new List<string>(generator.GetNextNumberStrings(pageCount));

                // Apply the bates number to each page
                writer = _codecs.CreateWriter(outputFile.FileName, format);
                for (int i = 1; i <= pageCount; i++)
                {
                    // Start a new progress status group every 4 pages
                    if (progressStatus != null && i % 4 == 1)
                    {
                        progressStatus.StartNextItemGroup("", 1);
                    }
                    
                    ApplyBatesNumberToPage(batesNumbers[i - 1], i, fileName, reader, writer);
                }
                reader.Dispose();
                reader = null;
                writer.Commit(true);

                // Copy the output file to the destination
                File.Copy(outputFile.FileName, fileName, true);

                // Ensure if progress status is being updated
                // that the last item is completed
                if (progressStatus != null && progressStatus.NumItemsInCurrentGroup > 0)
                {
                    progressStatus.CompleteCurrentItemGroup();
                }

                // Update the progress status
                if (progressStatus != null)
                {
                    progressStatus.CompleteCurrentItemGroup();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27990", ex);
                ee.AddDebugData("Total Pages", pageCount, false);
                if (batesNumbers != null)
                {
                    ee.AddDebugData("Last Bates Number", generator.LastBatesNumber, false);
                }
                throw ee;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
                if (outputFile != null)
                {
                    outputFile.Dispose();
                }
                if (writer != null)
                {
                    writer.Dispose();
                }
            }
        }

        /// <summary>
        /// Applies the specified Bates number string to the specified page of the output file.
        /// </summary>
        /// <param name="batesNumber">The Bates number string to apply.</param>
        /// <param name="pageNumber">The page to apply the string on.</param>
        /// <param name="inputFile">The input file to load.</param>
        /// <param name="reader">The reader from which to read the page.</param>
        /// <param name="writer">The writer to which to write the bates number.</param>
        void ApplyBatesNumberToPage(string batesNumber, int pageNumber, 
            string inputFile, ImageReader reader, ImageWriter writer)
        {
            int retryCount = 0;
            while (retryCount < _MAX_RETRIES)
            {
                retryCount++;

                bool wontFitOnPage = false;
                RasterImage image = null;
                Font pixelFont = null;
                RasterGraphics rg = null;
                try
                {
                    // Load the image page
                    image = reader.ReadPage(pageNumber);

                    // Compute the anchor point for the text
                    Point anchorPoint = GetAnchorPoint(_format.PageAnchorAlignment,
                        _format.HorizontalInches, _format.VerticalInches, image);

                    // Load the existing annotation objects
                    RasterTagMetadata tag = reader.ReadTagOnPage(pageNumber);

                    // Compute the appropriate font size
                    pixelFont = FontMethods.ConvertFontToUnits(_format.Font,
                    image.YResolution, GraphicsUnit.Pixel);

                    rg = RasterImagePainter.CreateGraphics(image);

                    // Compute the bounds for the string
                    Rectangle bounds = DrawingMethods.ComputeStringBounds(batesNumber,
                        rg.Graphics, pixelFont, 0, 0F, anchorPoint,
                        _format.AnchorAlignment);

                    // Ensure the Bates number fits on the image page
                    Rectangle pageBounds = new Rectangle(new Point(0, 0),
                        image.ImageSize.AsSize());

                    if (!pageBounds.Contains(bounds))
                    {
                        wontFitOnPage = true;

                        // Throw exception
                        ExtractException ee = new ExtractException("ELI27991",
                            "Bates number will appear off of the page with current settings.");
                        ee.AddDebugData("Bates Number String", batesNumber, false);
                        ee.AddDebugData("Bounds For Bates Number", bounds, false);
                        ee.AddDebugData("Bates Number Anchor Point", anchorPoint, false);
                        ee.AddDebugData("Page Number", pageNumber, false);
                        ee.AddDebugData("Image Bounds", pageBounds, false);
                        ee.AddDebugData("Image File Name", inputFile, false);
                        throw ee;
                    }

                    // Draw the Bates number on the image
                    DrawingMethods.DrawString(batesNumber, rg.Graphics,
                        rg.Graphics.Transform, pixelFont, 0, 0F, bounds, null, null);

                    // Save the image page (use append to add it to the end of the file)
                    writer.AppendImage(image);

                    // If there were annotation tags, save those as well
                    if (tag != null)
                    {
                        writer.WriteTagOnPage(tag, pageNumber);
                    }

                    // Successfully completed, break from loop
                    break;
                }
                catch (Exception ex)
                {
                    if (wontFitOnPage)
                    {
                        throw ExtractException.AsExtractException("ELI27992", ex);
                    }
                    else if (retryCount < _MAX_RETRIES)
                    {
                        ExtractException ee = new ExtractException("ELI27993",
                            "Application Trace: Could not apply Bates number on page, retrying.", ex);
                        ee.AddDebugData("Retry Count", retryCount, false);
                        ee.AddDebugData("Page Number", pageNumber, false);
                        ee.AddDebugData("Image File Name", inputFile, false);
                        ee.Log();
                        System.Threading.Thread.Sleep(100);
                    }
                    else
                    {
                        ExtractException ee = new ExtractException("ELI27994",
                            "Could not apply Bates number on page.", ex);
                        ee.AddDebugData("Number Of Retries", retryCount, false);
                        ee.AddDebugData("Page Number", pageNumber, false);
                        ee.AddDebugData("Bates Number String", batesNumber, false);
                        ee.AddDebugData("Image File Name", inputFile, false);
                        throw ee;
                    }
                }
                finally
                {
                    if (pixelFont != null)
                    {
                        pixelFont.Dispose();
                    }
                    if (rg != null)
                    {
                        rg.Dispose();
                    }
                    if (image != null)
                    {
                        image.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Computes the anchor point for the Bates number string.
        /// </summary>
        /// <param name="pageAnchorAlignment">The <see cref="AnchorAlignment"/> value
        /// with respect to the page.</param>
        /// <param name="horizontalInches">The horizontal offset from the specified
        /// <see cref="AnchorAlignment"/>.</param>
        /// <param name="verticalInches">The vertical offset from the specified
        /// <see cref="AnchorAlignment"/>.</param>
        /// <param name="image">The image page that the number will be applied to.</param>
        /// <returns>The anchor point for the Bates number string.</returns>
        static Point GetAnchorPoint(AnchorAlignment pageAnchorAlignment,
            float horizontalInches, float verticalInches, RasterImage image)
        {
            // Calculate the positive offset in logical (image) coordinates
            Size offset = GetAnchorPointOffset(horizontalInches, verticalInches, image);

            // Calculate the top left coordinate based on the anchor alignment
            Point anchorPoint;
            switch (pageAnchorAlignment)
            {
                case AnchorAlignment.LeftBottom:
                    anchorPoint = new Point(offset.Width, image.Height - offset.Height);
                    break;

                case AnchorAlignment.RightBottom:
                    anchorPoint = new Point(image.Width - offset.Width,
                        image.Height - offset.Height);
                    break;

                case AnchorAlignment.LeftTop:
                    anchorPoint = new Point(offset);
                    break;

                case AnchorAlignment.RightTop:
                    anchorPoint = new Point(image.Width - offset.Width, offset.Height);
                    break;

                default:
                    ExtractException ee = new ExtractException("ELI27995",
                        "Unexpected anchor alignment.");
                    ee.AddDebugData("Anchor alignment", pageAnchorAlignment, false);
                    throw ee;
            }

            return anchorPoint;
        }

        /// <summary>
        /// Computes the offset value for the anchor point based on the resolution
        /// of the image page.
        /// </summary>
        /// <param name="horizontalInches">The horizontal offset value.</param>
        /// <param name="verticalInches">The vertical offset value.</param>
        /// <param name="image">The image page the Bates number will be applied to.</param>
        /// <returns>The offset value for the Bates number anchor position.</returns>
        static Size GetAnchorPointOffset(float horizontalInches, float verticalInches,
            RasterImage image)
        {
            return new Size((int)(horizontalInches * image.XResolution + 0.5),
                (int)(verticalInches * image.YResolution + 0.5));
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets/sets the <see cref="BatesNumberFormat"/> for this task.
        /// <para><b>Note:</b></para>
        /// This task only supports <see cref="BatesNumberFormat"/> objects
        /// where <see cref="BatesNumberFormat.UseDatabaseCounter"/> is
        /// <see langword="true"/>.
        /// </summary>
        /// <value>The <see cref="BatesNumberFormat"/> to use for this task.</value>
        /// <returns>The <see cref="BatesNumberFormat"/> to use for this task.</returns>
        public BatesNumberFormat Format
        {
            get
            {
                return _format;
            }
            set
            {
                try
                {
                    if (value != null)
                    {
                        // Ensure the new format uses database counters
                        if (!value.UseDatabaseCounter)
                        {
                            throw new ExtractException("ELI27996",
                                "This task only supports using database counters.");
                        }
                    }

                    // If the new format is not the same as the old format
                    // store the new format and set the dirty flag
                    if (value != _format)
                    {
                        // Dispose of old format if it exists
                        if (_format != null)
                        {
                            _format.Dispose();
                        }
                        _format = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28001", ex);
                }
            }
        }

        /// <summary>
        /// Gets/sets the file name for this task.
        /// </summary>
        /// <value>The file name that this task will operate on.</value>
        /// <returns>The file name that this task will operate on.</returns>
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                try
                {
                    // If there is a string specified, check for invalid file tags
                    if (!string.IsNullOrEmpty(value))
                    {
                        // Create a tag manager to validate the file tags
                        FAMTagManagerClass manager = new FAMTagManagerClass();
                        ExtractException.Assert("ELI27997", "File name contains invalid file tags.",
                            !manager.StringContainsInvalidTags(value), "File Name With Invalid Tags",
                            value);
                    }

                    // If file names are different, store the value and set the dirty flag
                    if (!string.Equals(value, _fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        _fileName = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI28000", ex);
                }
            }
        }

        #endregion Properties

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the <see cref="ApplyBatesNumberTask"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ApplyBatesNumberTask"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ApplyBatesNumberTask"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_codecs != null)
                {
                    _codecs.Dispose();
                    _codecs = null;
                }
                if (_format != null)
                {
                    _format.Dispose();
                    _format = null;
                }
            }

            // No unmanaged resources to release
        }

        #endregion IDisposable
    }
}
