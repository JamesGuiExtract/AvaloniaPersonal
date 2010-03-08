using Extract.Imaging;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

using ComAttribute = UCLID_AFCORELib.Attribute;
using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;
using EOrientation = UCLID_RASTERANDOCRMGMTLib.EOrientation;
using ESpatialEntity = UCLID_RASTERANDOCRMGMTLib.ESpatialEntity;
using SpatialPageInfo = UCLID_RASTERANDOCRMGMTLib.SpatialPageInfo;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;
using SpatialStringSearcher = UCLID_RASTERANDOCRMGMTLib.SpatialStringSearcher;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents a file processing task that extends redactions to cover surrounding context.
    /// </summary>
    [ComVisible(true)]
    [Guid("53E116BE-BB3F-49A6-B24A-C50EE69F50BC")]
    [ProgId("Extract.Redaction.SurroundContextTask")]
    public class SurroundContextTask : ICategorizedComponent, IConfigurableObject, ICopyableObject,
                     IFileProcessingTask, ILicensedComponent, IPersistStream
    {
        #region Constants

        const string _COMPONENT_DESCRIPTION = "Redaction: Extend redactions to surround context";

        const int _CURRENT_VERSION = 2;
        
        #endregion Constants

        #region Fields

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="SurroundContextTask"/> 
        /// since it was created; <see langword="false"/> if no changes have been made since it
        /// was created.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Settings to extend redactions to cover surrounding context.
        /// </summary>
        SurroundContextSettings _settings;

        /// <summary>
        /// Loads redaction voa files.
        /// </summary>
        RedactionFileLoader _voaLoader;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurroundContextTask"/> class.
        /// </summary>
        public SurroundContextTask()
        {
            _settings = new SurroundContextSettings();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurroundContextTask"/> class.
        /// </summary>
        public SurroundContextTask(SurroundContextTask task)
        {
            CopyFrom(task);
        }
        
        #endregion Constructors

        #region Methods

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// <see cref="ExtractGuids.FileProcessors"/> COM category.
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
        /// <see cref="ExtractGuids.FileProcessors"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileProcessors);
        }

        /// <summary>
        /// Copies the specified <see cref="SurroundContextTask"/> instance into this one.
        /// </summary>
        /// <param name="task">The <see cref="SurroundContextTask"/> from which to copy.</param>
        public void CopyFrom(SurroundContextTask task)
        {
            _settings = task._settings;
        }

        /// <summary>
        /// Extends the specified redaction based on the settings.
        /// </summary>
        /// <param name="source">The source document of the redaction item.</param>
        /// <param name="searcher">Used to search for text to extend.</param>
        /// <param name="item">The redaction item to extend.</param>
        /// <returns>The extended redaction <paramref name="item"/>.</returns>
        RedactionItem GetExtendedRedaction(SpatialString source, SpatialStringSearcher searcher, 
            RedactionItem item)
        {
            // Get the spatial string
            SpatialString value = item.ComAttribute.Value;

            // Include the original zones in the result [FIDSC #4045]
            RasterZoneCollection imageZones = GetImageZonesFromSpatialString(value);
            RasterZoneCollection resultZones = new RasterZoneCollection(imageZones);

            int loadedPage = -1;
            foreach (RasterZone zone in GetOcrZonesFromSpatialString(value))
            {
                int currentPage = zone.PageNumber;
                if (currentPage != loadedPage)
                {
                    SpatialString page = source.GetSpecifiedPages(currentPage, currentPage);
                    searcher.InitSpatialStringSearcher(page);
                    loadedPage = currentPage;
                }

                // TODO: ExtendDataInRegion should accept an IUnknownVector of Rectangles.
                // Otherwise there is no guarantee that extending doesn't result in overlapping areas
                SpatialString extended = searcher.ExtendDataInRegion(
                    GetLongRectangleFromZone(zone), GetMaxWordsToExtend(), _settings.ExtendHeight);

                if (extended.HasSpatialInfo())
                {
                    resultZones.AddRange(GetImageZonesFromSpatialString(extended));
                }
            }

            // If no zones were found, return nothing
            if (resultZones.Count <= 0)
            {
                return null;
            }

            // Remove any duplicate zones
            RemoveDuplicates(resultZones);

            // Create the result for the spatial string
            SpatialString resultValue = new SpatialString();
            LongToObjectMap pageInfoMap = GetUnrotatedSpatialPageInfoMap(source);
            resultValue.CreateHybridString(resultZones.ToIUnknownVector(), value.String, 
                source.SourceDocName, pageInfoMap);

            // Don't modify the original attribute since the RedactionFileLoader is still using it
            ICopyableObject copy = (ICopyableObject)item.ComAttribute;
            ComAttribute attribute = (ComAttribute)copy.Clone();
            attribute.Value = resultValue;

            return new RedactionItem(attribute);
        }

        /// <summary>
        /// Returns the maximum number of words by which to extend a redaction.
        /// </summary>
        /// <returns>The maximum number of words by which to extend a redaction.</returns>
        int GetMaxWordsToExtend()
        {
            return _settings.RedactWords ? _settings.MaxWords : 0;
        }

        /// <summary>
        /// Get the OCR raster zones of the specified spatial string.
        /// </summary>
        /// <param name="value">The spatial string from which to retrieve raster zones.</param>
        /// <returns>The OCR raster zones of the specified spatial string.</returns>
        static RasterZoneCollection GetOcrZonesFromSpatialString(SpatialString value)
        {
            return new RasterZoneCollection(value.GetOCRImageRasterZones());
        }

        /// <summary>
        /// Get the raster zones of the specified spatial string in image coordinates.
        /// </summary>
        /// <param name="value">The spatial string from which to retrieve raster zones.</param>
        /// <returns>The raster zones of the specified spatial string in image coordinates.</returns>
        static RasterZoneCollection GetImageZonesFromSpatialString(SpatialString value)
        {
            return new RasterZoneCollection(value.GetOriginalImageRasterZones());
        }

        /// <summary>
        /// Creates the smallest rectangle that fully contains the specified raster zone.
        /// </summary>
        /// <param name="zone">The zone from which to create a rectangle.</param>
        /// <returns>The smallest rectangle that fully contains the specified raster 
        /// <paramref name="zone"/>.</returns>
        static LongRectangle GetLongRectangleFromZone(RasterZone zone)
        {
            Rectangle rectangle = zone.GetRectangularBounds();
            LongRectangle longRectangle = new LongRectangle();
            longRectangle.SetBounds(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

            return longRectangle;
        }

        /// <summary>
        /// Removes any zones in the specified collection that are completely contained by the 
        /// other zones.
        /// </summary>
        /// <param name="zones">The collection of zones from which to remove duplicates.</param>
        static void RemoveDuplicates(RasterZoneCollection zones)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                RasterZone zoneI = zones[i];

                double overlapArea = 0.0;
                for (int j = 0; j < zones.Count; j++)
                {
                    if (i != j)
                    {
                        overlapArea += zoneI.GetAreaOverlappingWith(zones[j]);
                    }
                }

                if (overlapArea >= zoneI.Area())
                {
                    zones.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Gets the spatial page info map of the spatial string without any deskew or rotation 
        /// applied.
        /// </summary>
        /// <param name="value">The spatial string from which to retrieve the unrotated spatial 
        /// info map.</param>
        /// <returns>The spatial page info map of <paramref name="value"/> without any deskew or 
        /// rotation applied.</returns>
        static LongToObjectMap GetUnrotatedSpatialPageInfoMap(SpatialString value)
        {
            LongToObjectMap pageInfoMap = value.SpatialPageInfos;
            LongToObjectMap result = new LongToObjectMap();

            int size = pageInfoMap.Size;
            for (int i = 0; i < size; i++)
            {
                int page;
                object pageInfoObject;
                pageInfoMap.GetKeyValue(i, out page, out pageInfoObject);

                SpatialPageInfo pageInfo = (SpatialPageInfo)pageInfoObject;

                SpatialPageInfo newPageInfo = GetUnrotatedPageInfo(pageInfo);

                result.Set(page, newPageInfo);
            }

            return result;
        }

        /// <summary>
        /// Gets the spatial page info without any deskew or rotation applied.
        /// </summary>
        /// <param name="pageInfo">The spatial string from which to retrieve the unrotated spatial 
        /// info map.</param>
        /// <returns>The <paramref name="pageInfo"/> without any deskew or rotation applied.
        /// </returns>
        static SpatialPageInfo GetUnrotatedPageInfo(SpatialPageInfo pageInfo)
        {
            int width, height;
            pageInfo.GetWidthAndHeight(out width, out height);

            SpatialPageInfo newPageInfo = new SpatialPageInfo();
            newPageInfo.SetPageInfo(width, height, EOrientation.kRotNone, 0);

            return newPageInfo;
        }

        /// <summary>
        /// Creates a collection of redaction items that should be extended based on the 
        /// <see cref="SurroundContextSettings"/>.
        /// </summary>
        /// <param name="items">The items from which redaction items should be selected.</param>
        /// <returns>A collection of redaction items that should be extended based on the 
        /// settings.</returns>
        List<RedactionItem> GetRedactionsToExtend(ICollection<SensitiveItem> items)
        {
            List<RedactionItem> redactions = new List<RedactionItem>(items.Count);
            foreach (SensitiveItem item in items)
            {
                RedactionItem redaction = item.Attribute;
                if (redaction.Redacted && _settings.IsTypeToExtend(redaction.RedactionType))
                {
                    redactions.Add(redaction);
                }
            }

            return redactions;
        }

        #endregion Methods

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
        /// Performs configuration needed to create a valid <see cref="SurroundContextTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI29500",
                    _COMPONENT_DESCRIPTION);

                // Allow the user to configure the settings
                using (SurroundContextSettingsDialog dialog = new SurroundContextSettingsDialog(_settings))
                {
                    bool result = dialog.ShowDialog() == DialogResult.OK;

                    // Store the result
                    if (result)
                    {
                        _settings = dialog.SurroundContextSettings;
                        _dirty = true;
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29501",
                    "Error running configuration.", ex);
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="SurroundContextTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="SurroundContextTask"/> instance.</returns>
        public object Clone()
        {
            return new SurroundContextTask(this);
        }

        /// <summary>
        /// Copies the specified <see cref="SurroundContextTask"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                CopyFrom((SurroundContextTask)pObject);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29525",
                    "Unable to copy task.", ex);
            }
        }

        #endregion ICopyableObject Members
        
        #region IFileProcessingTask Members

        /// <summary>
        /// Stops processing the current file.
        /// </summary>
        public void Cancel()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI29502",
                    _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29503", 
                    "Unable to cancel 'Extend redactions to surround context' task.", ex);
            }
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI29504",
                    _COMPONENT_DESCRIPTION);

                _voaLoader = null;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29505",
                    "Unable to close 'Extend redactions to surround context' task.", ex);
            }
        }

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI29506",
                    _COMPONENT_DESCRIPTION);

                // Create the voa file loader
                InitializationSettings settings = new InitializationSettings();
                _voaLoader = new RedactionFileLoader(settings.ConfidenceLevels);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29507",
                    "Unable to initialize 'Extend redactions to surround context' task.", ex);
            }
        }

        /// <summary>
        /// Processes the specified file.
        /// </summary>
        /// <param name="bstrFileFullName">The file to process.</param>
        /// <param name="nFileID">The ID of the file being processed.</param>
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
        public EFileProcessingResult ProcessFile(string bstrFileFullName, int nFileID, int nActionID,
            FAMTagManager pFAMTM, FileProcessingDB pDB, ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI29508",
                    _COMPONENT_DESCRIPTION);

                // Start the timer
                IntervalTimer timer = IntervalTimer.StartNew();

                // Load the uss file
                SpatialString source = new SpatialString();
                source.LoadFrom(bstrFileFullName + ".uss", false);

                // Load the redactions
                FileActionManagerPathTags pathTags = 
                    new FileActionManagerPathTags(bstrFileFullName, pFAMTM.FPSFileDir);
                string voaFile = pathTags.Expand(_settings.DataFile);
                _voaLoader.LoadFrom(voaFile, bstrFileFullName);

                // Create the spatial string searcher
                SpatialStringSearcher searcher = new SpatialStringSearcher();
                searcher.SetBoundaryResolution(ESpatialEntity.kCharacter);
                searcher.SetIncludeDataOnBoundary(true);

                // Extend each redaction as necessary
                List<RedactionItem> results = new List<RedactionItem>(_voaLoader.Items.Count);
                List<RedactionItem> redactions = GetRedactionsToExtend(_voaLoader.Items);
                foreach (RedactionItem redaction in redactions)
                {
                    RedactionItem result = GetExtendedRedaction(source, searcher, redaction);
                    if (result != null)
                    {
                        results.Add(result);
                    }
                }

                // If no redactions were expanded throw an exception
                if (redactions.Count > 0 && results.Count <= 0)
                {
                    throw new ExtractException("ELI29618", 
                        "No redactions could be expanded.");
                }

                // Save the results
                RedactionFileChanges fileChanges = new RedactionFileChanges(new RedactionItem[0],
                    new RedactionItem[0], results);
                TimeInterval interval = timer.Stop();
                _voaLoader.SaveSurroundContextSession(voaFile, fileChanges, interval, _settings);
                
                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29509",
                    "Unable to extend redactions to cover surrounding context.", ex);
            }
        }

        #endregion IFileProcessingTask Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            try
            {
                return LicenseUtilities.IsLicensed(LicenseIdName.IDShieldCoreObjects);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29524",
                    "Error checking licensing state.", ex);
            }
        }

        #endregion ILicensedComponent Members

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
                    // Read the settings
                    _settings = SurroundContextSettings.ReadFrom(reader);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29510",
                    "Unable to load 'Extend redactions to surround context' task.", ex);
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
                    _settings.WriteTo(writer);

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
                throw ExtractException.CreateComVisible("ELI29511",
                    "Unable to save 'Extend redactions to surround context' task.", ex);
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

        #endregion IPersistStream Members
    }
}
