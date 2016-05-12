﻿using Extract.UtilityApplications.PaginationUtility;
using UCLID_COMUTILSLib;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// A extension of <see cref="IDocumentDataPanel"/> for use in data entry verification that will
    /// be based on the VOA data for the associated file.
    /// </summary>
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
    }
}