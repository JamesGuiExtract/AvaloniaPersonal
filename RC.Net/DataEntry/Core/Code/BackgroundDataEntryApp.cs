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
    /// the application when performing background data loads.
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
        public FileProcessingDB FileProcessingDB { get; private set; }
        public string DatabaseActionName => "";
        public string DatabaseComment { get; set; }
        public IFileRequestHandler FileRequestHandler => null;
        public bool Dirty => false;
        public ReadOnlyCollection<int> FileIds => new List<int>().AsReadOnly();
        public bool RunningInBackground => true;
        public bool SaveData(bool validateData) { return false; }
        public void DelayFile(int fileId = -1) { }
        public void SkipFile() { }
        public bool RequestFile(int fileID) { return false; }
        public void ReleaseFile(int fileID) { }
        public event EventHandler<EventArgs> ShowAllHighlightsChanged { add { } remove { } }
    }
}
