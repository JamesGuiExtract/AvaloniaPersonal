using System;
using System.Collections.ObjectModel;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry
{
    #region Enums

    /// <summary>
    /// Specifies how the image viewer zoom/view is adjusted when new fields are selected.
    /// </summary>
    public enum AutoZoomMode
    {
        /// <summary>
        /// If the selected object is not completely visible, the image will be scrolled to place
        /// the center of the object in the center of the screen, but the zoom level will not be
        /// changed (either in or out).
        /// </summary>
        NoZoom = 0,

        /// <summary>
        /// If the selected object is not completely visible, the image will be scrolled. If the
        /// selected object cannot be fit at the current zoom level, the zoom level will be
        /// expanded. For items that can be fit, zoom will be returned as close as possible to the
        /// last manually set zoom level while still displaying the entire selected object.
        /// </summary>
        ZoomOutIfNecessary = 1,

        /// <summary>
        /// Zoom and position will be automatically centered around the current selection with a
        /// user-specified amount of page context displayed around the object.
        /// </summary>
        AutoZoom = 2
    }

    #endregion Enums

    #region IDataEntryApplication

    /// <summary>
    /// Represents application-wide properties and events required by the data entry framework
    /// </summary>
    public interface IDataEntryApplication
    {
        #region Properties

        /// <summary>
        /// The title of the current DataEntry application.
        /// </summary>
        string ApplicationTitle
        {
            get;
        }

        /// <summary>
        /// Specifies how the image viewer zoom/view is adjusted when new fields are selected.
        /// </summary>
        AutoZoomMode AutoZoomMode
        {
            get;
        }

        /// <summary>
        /// The page space (context) that should be shown around an object selected when AutoZoom
        /// mode is active. 0 indicates no context space should be shown around the current
        /// selection where 1 indicates the maximum context space should be shown.
        /// </summary>
        double AutoZoomContext
        {
            get;
        }

        /// <summary>
        /// Indicates whether tabbing should allow groups (rows) of attributes to be selected at a
        /// time for controls in which group tabbing is enabled.
        /// </summary>
        bool AllowTabbingByGroup
        {
            get;
        }

        /// <summary>
        /// Get whether highlights for all data mapped to an <see cref="IDataEntryControl"/> should
        /// be displayed in the <see cref="ImageViewer"/> or whether only highlights relating to the
        /// currently selected fields should be displayed.
        /// </summary>
        bool ShowAllHighlights
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="FileProcessingDB"/> the <see cref="IDataEntryApplication"/> is
        /// currently being run against.
        /// </summary>
        FileProcessingDB FileProcessingDB
        {
            get;
        }

        /// <summary>
        /// Gets the name of the action in <see cref="FileProcessingDB"/> that the
        /// <see cref="IDataEntryApplication"/> is currently being run against.
        /// </summary>
        string DatabaseActionName
        {
            get;
        }

        /// <summary>
        /// Gets or sets the comment for the current file that is stored in the file processing
        /// database.
        /// </summary>
        string DatabaseComment
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="IFileRequestHandler"/> that can be used to carry out requests for
        /// files to be checked out, released or re-ordered in the queue.
        /// </summary>
        IFileRequestHandler FileRequestHandler
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the currently loaded document is dirty.
        /// </summary>
        /// <value><see langword="true"/> if dirty; otherwise, <see langword="false"/>.
        /// </value>
        bool Dirty
        {
            get;
        }

        /// <summary>
        /// Gets the IDs of the files currently loaded in the application.
        /// </summary>
        /// <value>
        /// The IDs of the files currently loaded in the application.
        /// </value>
        ReadOnlyCollection<int> FileIds
        {
            get;
        }

        /// <summary>
        /// <c>true</c> if this application is running the in the background-- either for background
        /// loading of all documents in the pagination verification UI or as part of the pagination
        /// verification task; <c>false</c> if the application is displaying the data entry
        /// controls to a user.
        /// </summary>
        bool RunningInBackground
        {
            get;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Saves the data currently displayed to disk.
        /// </summary>
        /// <param name="validateData"><see langword="true"/> to ensure the data is conforms to the
        /// DataEntryControlHost InvalidDataSaveMode before saving, <see langword="false"/> to save
        /// data without validating.</param>
        /// <returns><see langword="true"/> if the data was saved, <see langword="false"/> if it was
        /// not.</returns>
        bool SaveData(bool validateData);

        /// <summary>
        /// Delays processing of the current file allowing the next file in the queue to be brought
        /// up in its place (though if there are no more files in the queue this will cause the same
        /// file to be re-displayed.
        /// <para><b>Note</b></para>
        /// If there are changes in the currently loaded document, they will be disregarded. To
        /// check for changes and save, use the <see cref="Dirty"/> and <see cref="SaveData"/>
        /// members first.
        /// </summary>
        /// <param name="fileId">The ID of the file to delay (or -1 when there is only a single
        /// file to which this call could apply).</param>
        void DelayFile(int fileId = -1);

        /// <summary>
        /// Skips processing for the current file. This is the same as pressing the skip button in
        /// the UI.
        /// <para><b>Note</b></para>
        /// If there are changes in the currently loaded document, they will be disregarded. To
        /// check for changes and save, use the <see cref="Dirty"/> and <see cref="SaveData"/>
        /// members first.
        /// </summary>
        void SkipFile();

        /// <summary>
        /// Requests the specified <see paramref="fileID"/> to be the next file displayed. The file
        /// should be allowed to jump ahead of any other files currently "processing" in the
        /// verification task on other threads (prefetch).
        /// <para><b>Note</b></para>
        /// The requested file will not be shown until the currently displayed file is closed. If
        /// the requested file needs to replace the currently displayed file immediately,
        /// <see cref="DelayFile"/> should be called after RequestFile.
        /// </summary>
        /// <param name="fileID">The file ID.</param>
        /// <returns><see langword="true"/> if the file is currently processing in the verification
        /// task and confirmed to be available,<see langword="false"/> if the task is not currently
        /// holding the file; the requested file will be expected to the next file in the queue or
        /// an exception will result.</returns>
        bool RequestFile(int fileID);

        /// <summary>
        /// Releases the specified file from the current process's internal queue of files checked
        /// out for processing. The file will be treated as if processing has been canceled/stopped
        /// and returned to the current fallback status (status before lock by default).
        /// </summary>
        /// <param name="fileID">The ID of the file to release.</param>
        void ReleaseFile(int fileID);

        #endregion Methods

        #region Events

        /// <summary>
        /// This event indicates the value of <see cref="ShowAllHighlights"/> has
        /// changed.
        /// </summary>
        event EventHandler<EventArgs> ShowAllHighlightsChanged;

        #endregion Events
    }

    #endregion IDataEntryApplication
}
