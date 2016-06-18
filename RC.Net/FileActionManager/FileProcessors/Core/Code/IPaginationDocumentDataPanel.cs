using Extract.UtilityApplications.PaginationUtility;
using System;
using UCLID_COMUTILSLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// A extension of <see cref="IDocumentDataPanel"/> for use in data entry verification that will
    /// be based on the VOA data for the associated file.
    /// </summary>
    [CLSCompliant(false)]
    public interface IPaginationDocumentDataPanel : IDocumentDataPanel
    {
        /// <summary>
        /// Gets a <see cref="PaginationDocumentData"/> instance based on the provided
        /// <see paramref="attributes"/>.
        /// </summary>
        /// <param name="attributes">The VOA data for while a <see cref="PaginationDocumentData"/>
        /// instance is needed.</param>
        /// <returns>The <see cref="PaginationDocumentData"/> instance.</returns>
        PaginationDocumentData GetDocumentData(IUnknownVector attributes);

        /// <summary>
        /// Provides a message to be displayed.
        /// </summary>
        /// <param name="message">The message to display.</param>
        void ShowMessage(string message);
    }
}
