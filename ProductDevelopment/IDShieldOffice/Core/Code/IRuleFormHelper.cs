using System;
using System.Diagnostics.CodeAnalysis;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.Rules
{
    /// <summary>
    /// Defines methods that provide requisite information for a <see cref="RuleForm"/>.
    /// </summary>
    [CLSCompliant(false)]
    public interface IRuleFormHelper
    {
        /// <summary>
        /// Retrieves the optical character recognition (OCR) results for the rule form to use.
        /// </summary>
        /// <returns>The optical character recognition (OCR) results for the rule form to use.
        /// </returns>
        // This may be a complex operation, so is better suited as a method
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
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
