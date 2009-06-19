using System;
using System.Collections.Generic;
using System.Text;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents File Action Manager document tags.
    /// </summary>
    internal static class DocumentTags
    {
        #region DocumentTags Methods

        /// <summary>
        /// Gets all the File Action Manager document tags.
        /// </summary>
        /// <returns>All the File Action Manager document tags.</returns>
        public static List<string> GetAll()
        {
            // There are multiple tag managers in different dlls for different products.
            // As a result, many places that use the PathTagsButton must duplicate code like the 
            // code in this method and explicitly execute it in the form's constructor.
            // TODO: Is there a way to choose the tag manager as a property on the PathTagsButton?

            // Get the tags as a variant vector
            IFAMTagManager manager = new FAMTagManagerClass();
            VariantVector tags = manager.GetAllTags();

            // Convert the variant vector to a list
            int tagCount = tags.Size;
            List<string> docTags = new List<string>(tagCount);
            for (int i = 0; i < tagCount; i++)
            {
                docTags.Add((string)tags[i]);
            }

            return docTags;
        }

        #endregion DocumentTags Methods
    }
}
