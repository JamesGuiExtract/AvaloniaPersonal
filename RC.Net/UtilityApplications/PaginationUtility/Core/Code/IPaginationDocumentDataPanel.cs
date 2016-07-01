using Extract.Imaging.Forms;
using System.Windows.Forms;
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
        /// The <see cref="UserControl"/> to be displayed for viewing/editing of document data.
        /// </summary>
        UserControl Control
        {
            get;
        }

        /// <summary>
        /// Loads the specified <see paramref="data"/>.
        /// </summary>
        /// <param name="data">The data to load.</param>
        void LoadData(PaginationDocumentData data);

        /// <summary>
        /// Applies any data to the specified <see paramref="data"/>.
        /// <para><b>Note</b></para>
        /// In addition to returning <see langword="false"/>, it is the implementor's responsibility
        /// to notify the user of any problems with the data that needs to be corrected before it
        /// can be saved.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <returns><see langword="true"/> if the data was saved correctly or
        /// <see langword="false"/> if corrections are needed before it can be saved.</returns>
        bool SaveData(PaginationDocumentData data);

        /// <summary>
        /// Gets a <see cref="PaginationDocumentData"/> instance based on the provided
        /// <see paramref="attributes"/>.
        /// </summary>
        /// <param name="attributes">The VOA data for while a <see cref="PaginationDocumentData"/>
        /// instance is needed.</param>
        /// <param name="fileProcessingDB"></param>
        /// <param name="imageViewer"></param>
        /// <returns>The <see cref="PaginationDocumentData"/> instance.</returns>
        PaginationDocumentData GetDocumentData(IUnknownVector attributes,
            FileProcessingDB fileProcessingDB, ImageViewer imageViewer);

        /// <summary>
        /// Provides a message to be displayed.
        /// </summary>
        /// <param name="message">The message to display.</param>
        void ShowMessage(string message);
    }
}
