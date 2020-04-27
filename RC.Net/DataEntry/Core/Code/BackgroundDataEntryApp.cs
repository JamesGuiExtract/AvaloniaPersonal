using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// A no-implementation version of <see cref="IDataEntryApplication"/> used as a stand-in for
    /// the application when performing background data loads or unit testing.
    /// </summary>
    /// <seealso cref="Extract.DataEntry.IDataEntryApplication" />
    public class BackgroundDataEntryApp : IDataEntryApplication
    {
        /// <summary>
        /// Initializes a new <see cref="BackgroundDataEntryApp"/> instance.
        /// </summary>
        /// <param name="fileProcessingDB">There are cases (such as order/encounter linking) where
        /// access to the FAM DB is needed via the IDataEntryApplication.</param>
        public BackgroundDataEntryApp(FileProcessingDB fileProcessingDB = null)
        {
            FileProcessingDB = fileProcessingDB;
        }

        public string ApplicationTitle => "";
        public AutoZoomMode AutoZoomMode => AutoZoomMode.NoZoom;
        public double AutoZoomContext => 0;
        public bool AllowTabbingByGroup => false;
        public bool ShowAllHighlights => false;
        public FileProcessingDB FileProcessingDB { get; set; }
        public string DatabaseActionName { get; set; } = "";
        public string DatabaseComment { get; set; }
        public IFileRequestHandler FileRequestHandler { get; set; }
        public bool Dirty => false;

        /// <summary>
        /// Gets the IDs of the files currently loaded in the application.
        /// Needs to return the ID for the active AttributeStatusInfo.SourceDocName in the case of unit testing
        /// (specifically TestLabDEDuplicateDocumentsButton).
        /// </summary>
        public ReadOnlyCollection<int> FileIds
        {
            get
            {
                if (FileProcessingDB != null && !string.IsNullOrWhiteSpace(AttributeStatusInfo.SourceDocName))
                {
                    return new[] { FileProcessingDB.GetFileID(AttributeStatusInfo.SourceDocName) }
                        .ToList()
                        .AsReadOnly();
                }
                else
                {
                    return new List<int>().AsReadOnly();
                }
            }
        }

        public bool RunningInBackground => true;
        public bool SaveData(bool validateData) { return false; }
        public void DelayFile(int fileId = -1) { }
        public void SkipFile() { }

        /// <summary>
        /// For unit testing, if a FileRequestHandler is specified, it should be used to request the file.
        /// </summary>
        public bool RequestFile(int fileID)
        {
            return FileRequestHandler?.ReleaseFile(fileID) == true;
        }

        /// <summary>
        /// For unit testing, if a FileRequestHandler is specified, it should be used to release the file.
        /// </summary>
        public void ReleaseFile(int fileID)
        {
            FileRequestHandler?.ReleaseFile(fileID);
        }

        public event EventHandler<EventArgs> ShowAllHighlightsChanged { add { } remove { } }
    }
}
