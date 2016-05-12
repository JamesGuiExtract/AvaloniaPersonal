using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Base class for a control to allow display and editing of data relating to the currently
    /// selected document in a <see cref="PaginationPanel"/>.
    /// <para><b>Note</b></para>
    /// Conceptually this would work best as an abstract base class, but then extensions of this
    /// class cannot be edited in the designer.
    /// </summary>
    public interface IDocumentDataPanel 
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
        void LoadData(IDocumentData data);

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
        bool SaveData(IDocumentData data);
    }
}
