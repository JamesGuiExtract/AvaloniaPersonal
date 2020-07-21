using Extract.DataEntry;
using Extract.Imaging.Forms;
using System;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Base class for a control to allow display and editing of data relating to the currently
    /// selected document in a <see cref="PaginationPanel"/>.
    /// <para><b>Note</b></para>
    /// Conceptually this would work best as an abstract base class, but then extensions of this
    /// class cannot be edited in the designer.
    /// </summary>
    public interface IPaginationDocumentDataPanel
    {
        /// <summary>
        /// Raised to indicate the panel is requesting a specific image page to be loaded.
        /// </summary>
        event EventHandler<PageLoadRequestEventArgs> PageLoadRequest;

        /// <summary>
        /// Indicates that the availability of an operation for the <see cref="Undo"/> method to
        /// revert has changed.
        /// </summary>
        event EventHandler<EventArgs> UndoAvailabilityChanged;

        /// <summary>
        /// Indicates that the availability of an operation for the <see cref="Redo"/> method to
        /// redo has changed.
        /// </summary>
        event EventHandler<EventArgs> RedoAvailabilityChanged;

        /// <summary>
        /// Occurs when a document has finished loading.
        /// </summary>
        event EventHandler<EventArgs> DocumentLoaded;

        /// <summary>
        /// Indicates that the displayed panel has been changed such as for document type specific
        /// panels when the document type field changes.
        /// </summary>
        event EventHandler<DataPanelChangedEventArgs> DataPanelChanged;

        /// <summary>
        /// Raised when tab navigation is activated; if handled, the control hosting this panel
        /// should implement the UI response to the navigation. If not handled, this class (or the
        /// underlying DataEntryControlHost) will.
        /// </summary>
        event EventHandler<TabNavigationEventArgs> TabNavigation;

        /// <summary>
        /// The <see cref="UserControl"/> to be displayed for viewing/editing of document data.
        /// </summary>
        UserControl PanelControl
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether advanced data entry operations (such as undo/redo) are
        /// supported.
        /// </summary>
        /// <value><see langword="true"/> if advanced data entry operations (such as undo/redo) are
        /// supported; otherwise,<see langword="false"/>.
        /// </value>
        bool AdvancedDataEntryOperationsSupported
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether an undo operation is available.
        /// </summary>
        /// <value>
        /// <c>true</c> if an undo operation is available; otherwise, <c>false</c>.
        /// </value>
        bool UndoOperationAvailable
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether an redo operation is available.
        /// </summary>
        /// <value>
        /// <c>true</c> if an redo operation is available; otherwise, <c>false</c>.
        /// </value>
        bool RedoOperationAvailable
        {
            get;
        }

        /// <summary>
        /// Gets the active <see cref="DataEntryDocumentDataPanel"/> or <c>null//c> if there
        /// is no DEP currently displayed.
        /// </summary>
        DataEntryDocumentDataPanel ActiveDataEntryPanel
        {
            get;
        }

        /// <summary>
        /// Gets the current "active" data entry. This is the last data entry control to have
        /// received input focus (but doesn't necessarily mean the control currently has input
        /// focus).
        /// </summary>
        Control ActiveDataControl
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the PageLayoutControl's PrimarySelection
        /// corresponds with the output document for which this DEP is editing data.
        /// </summary>
        /// <value>
        /// <c>true</c> if he PageLayoutControl's PrimarySelection corresponds with the output
        /// document for which this DEP is editing data; otherwise, <c>false</c>.
        /// </value>
        bool PrimaryPageIsForActiveDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether this panel should be editable (!read-only).
        /// </summary>
        bool Editable
        {
            get;
            set;
        }

        /// <summary>
        /// <see cref="TabNavigationEventArgs"/> indicating a deferred <see cref="TabNavigation"/>
        /// event that should be executed on the next tab navigation operation.
        /// </summary>
        TabNavigationEventArgs PendingTabNavigationEventArgs
        {
            get;
            set;
        }

        /// <summary>
        /// Loads the specified <see paramref="data"/>.
        /// </summary>
        /// <param name="data">The data to load.</param>
        /// <param name="forEditing"><c>true</c> if the loaded data is to be displayed for editing;
        /// <c>false</c> if the data is to be displayed read-only, or if it is being used for
        /// background formatting.</param>
        void LoadData(PaginationDocumentData data, bool forEditing);

        /// <summary>
        /// Applies any data to the specified <see paramref="data"/>.
        /// <para><b>Note</b></para>
        /// In addition to returning <see langword="false"/>, it is the implementor's responsibility
        /// to notify the user of any problems with the data that needs to be corrected before it
        /// can be saved.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <param name="validateData"><see langword="true"/> if the <see paramref="data"/> should
        /// be validated for errors when saving; otherwise, <see langwor="false"/>.</param>
        /// <returns><see langword="true"/> if the data was saved correctly or
        /// <see langword="false"/> if corrections are needed before it can be saved.</returns>
        bool SaveData(PaginationDocumentData data, bool validateData);

        /// <summary>
        /// Clears the state of all data associated with the previously loaded document.
        /// </summary>
        void ClearData();

        /// <summary>
        /// Gets a <see cref="PaginationDocumentData"/> instance based on the provided
        /// <see paramref="attributes"/>.
        /// </summary>
        /// <param name="attributes">The VOA data for while a <see cref="PaginationDocumentData"/>
        /// instance is needed.</param>
        /// <param name="sourceDocName">The name of the source document for which data is being
        /// loaded.</param>
        /// <param name="fileProcessingDB"></param>
        /// <param name="imageViewer"></param>
        /// <returns>The <see cref="PaginationDocumentData"/> instance.</returns>
        PaginationDocumentData GetDocumentData(IUnknownVector attributes, string sourceDocName,
            FileProcessingDB fileProcessingDB, ImageViewer imageViewer);

        /// <summary>
        /// Gets a <see cref="PaginationDocumentData" /> instance based on the provided
        /// <see paramref="attributes" />.
        /// </summary>
        /// <param name="documentDataAttribute">The VOA data for which a <see cref="PaginationDocumentData" />
        /// instance is needed including this top-level attribute which contains document data status info.
        /// </param>
        /// <param name="sourceDocName">The name of the source document for which data is being
        /// loaded.</param>
        /// <param name="fileProcessingDB"></param>
        /// <param name="imageViewer"></param>
        /// <returns>
        /// The <see cref="PaginationDocumentData" /> instance.
        /// </returns>
        PaginationDocumentData GetDocumentData(IAttribute documentDataAttribute, string sourceDocName,
            FileProcessingDB fileProcessingDB, ImageViewer imageViewer);

        /// <summary>
        /// Updates the document data.
        /// </summary>
        /// <param name="data">The <see cref="PaginationDocumentData"/> to be updated.
        /// </param>
        /// <param name="statusOnly"><c>true</c> to retrieve into <paramref name="data"/> only the high-level
        /// status such as the the summary string and other status flags; <c>false</c> to udpate
        /// <paramref name="data"/> with the complete voa data.</param>
        /// <param name="displayValidationErrors"><c>true</c> if you want to display validation errors;
        /// <c>false</c> if you do not want to display validation errors.</param>
        void UpdateDocumentData(PaginationDocumentData data, bool statusOnly, bool displayValidationErrors);

        /// <summary>
        /// Triggers an update to the <see cref="Summary"/>, <see cref="DataModified"/> and
        /// <see cref="DataError"/> properties of <see paramref="documentData"/> by processing the
        /// data in a background thread. If the results of status updates are needed
        /// programmatically rather than to update the UI, <see cref="WaitForDocumentStatusUpdates"/>
        /// should be used before checking the updated status.
        /// </summary>
        /// <param name="documentData">The <see cref="DataEntryPaginationDocumentData"/> for which
        /// document status should be updated.</param>
        /// <param name="statusOnly"><c>true</c> to retrieve into documentData only the high-level
        /// status such as the the summary string and other status flags; <c>false</c> to udpate
        /// documentData with the complete voa data.</param>
        /// <param name = "displayValidationErrors" >< c > true </ c > if you want to display validation errors;
        /// <c>false</c> if you do not want to display validation errors.</param>
        void StartUpdateDocumentStatus(DataEntryPaginationDocumentData documentData,
            bool statusOnly, bool applyUpdateToUI, bool displayValidationErrors);

        /// <summary>
        /// Waits for all documents status updates (started via <see cref="UpdateDocumentData"/>)
        /// to complete.
        /// </summary>
        bool WaitForDocumentStatusUpdates();

        /// <summary>
        /// Provides a message to be displayed.
        /// </summary>
        /// <param name="message">The message to display.</param>
        void ShowMessage(string message);

        /// <summary>
        /// Ensures a field is selected by selecting the first field if necessary.
        /// </summary>
        /// <param name="resetToFirstField"><c>true</c> to select the first field regardless of any
        /// active selection.</param>
        /// <param name="resetToLastField"><c>true</c> to select the last field regardless of any
        /// active selection.</param>
        void EnsureFieldSelection(bool resetToFirstField, bool resetToLastField);

        /// <summary>
        /// Performs an undo operation.
        /// </summary>
        void Undo();

        /// <summary>
        /// Performs a redo operation.
        /// </summary>
        void Redo();

        /// <summary>
        /// Toggles whether or not tooltip(s) for the active fields are currently visible.
        /// </summary>
        void ToggleHideTooltips();

        /// <summary>
        /// Refreshes the state of the control.
        /// </summary>
        void RefreshControlState();

        /// <summary>
        /// <c>true</c> if a document type field is available to select from document-type specific
        /// configurations.
        /// </summary>
        bool DocumentTypeAvailable { get; }

        /// <summary>
        /// <c>true</c> if a document type field current has focus; otherwise, <c>false</c>.
        /// </summary>
        bool DocumentTypeFocused { get; }

        /// <summary>
        /// Sets focus to the document type field.
        /// </summary>
        bool FocusDocumentType();

        /// <summary>
        /// Attempts to apply the current text of _documentTypeComboBox as the active document type.
        /// </summary>
        /// <returns><c>true</c> if the document type was successfully applied; <c>false</c> if the 
        /// document type was not successfully applied.</returns>
        bool ApplyDocumentType();
    }
}
