using UCLID_RASTERANDOCRMGMTLib;

namespace IDShieldOffice
{
    interface IRuleFormHelper
    {
        /// <summary>
        /// Retrieves the optical character recognition (OCR) results for the rule form to use.
        /// </summary>
        /// <returns>The optical character recognition (OCR) results for the rule form to use.
        /// </returns>
        SpatialString GetOcrResults();

        /// <summary>
        /// Determines whether the specified match has already been found.
        /// </summary>
        /// <param name="match">The match to check for duplication.</param>
        /// <returns><see langword="true"/> if the specified <paramref name="match"/> has 
        /// already been found; <see langword="false"/> has not yet been found.</returns>
        bool IsDuplicate(MatchResult match);
    }
}
