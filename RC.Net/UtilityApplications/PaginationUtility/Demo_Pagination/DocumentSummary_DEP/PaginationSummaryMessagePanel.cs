using Extract.Imaging.Forms;
using Extract.UtilityApplications.PaginationUtility;
using System;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DocumentSummary
{
    /// <summary>
    /// An implementation of a <see cref="IPaginationDocumentDataPanel"/> used only to set summary
    /// text on the document separators.
    /// </summary>
    internal partial class PaginationSummaryMessagePanel : UserControl, IPaginationDocumentDataPanel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationSummaryMessagePanel"/> class.
        /// </summary>
        public PaginationSummaryMessagePanel()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49990");
            }
        }

        #endregion Constructors

        #region IPaginationDocumentDataPanel

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
        public PaginationDocumentData GetDocumentData(IUnknownVector attributes,
            string sourceDocName, FileProcessingDB fileProcessingDB, ImageViewer imageViewer)
        {
            return new PaginationSummaryDocumentData(attributes, sourceDocName);
        }

        #endregion IPaginationDocumentDataPanel

        #region Unused IPaginationDocumentDataPanel

        public event EventHandler<PageLoadRequestEventArgs> PageLoadRequest
        {
            add { }
            remove { }
        }

        public event EventHandler<EventArgs> UndoAvailabilityChanged
        {
            add { }
            remove { }
        }

        public event EventHandler<EventArgs> RedoAvailabilityChanged
        {
            add { }
            remove { }
        }

        public UserControl PanelControl
        {
            get
            {
                return this;
            }
        }

        public bool AdvancedDataEntryOperationsSupported
        {
            get
            {
                return false;
            }
        }

        public bool UndoOperationAvailable
        {
            get
            {
                return false;
            }
        }

        public bool RedoOperationAvailable
        {
            get
            {
                return false;
            }
        }

        public Control ActiveDataControl
        {
            get
            {
                return null;
            }
        }

        public bool PrimaryPageIsForActiveDocument
        {
            get
            {
                return false;
            }

            set
            {
            }
        }

        public void LoadData(PaginationDocumentData data, bool forDisplay)
        {
        }

        public bool SaveData(PaginationDocumentData data, bool validateData)
        {
            return true;
        }

        public void ClearData()
        {
        }

        public void ShowMessage(string message)
        {
        }

        public void Undo()
        {
            throw new NotSupportedException();
        }

        public void Redo()
        {
            throw new NotSupportedException();
        }

        public void UpdateDocumentDataStatus(PaginationDocumentData data)
        {
        }

        public void ToggleHideTooltips()
        {
        }

        public void RefreshControlState()
        {
        }

        #endregion Unused IPaginationDocumentDataPanel
    }
}
