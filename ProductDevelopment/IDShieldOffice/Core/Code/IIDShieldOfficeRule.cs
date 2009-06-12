using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Text;
using UCLID_RASTERANDOCRMGMTLib;

namespace IDShieldOffice
{
    /// <summary>
    /// Defines a method for searching a SpatialString and returning a collection
    /// of <see cref="MatchResult"/>.
    /// </summary>
    internal interface IIDShieldOfficeRule : IUserConfigurableComponent
    {
        /// <summary>
        /// Returns a <see cref="List{T}"/> of <see cref="MatchResult"/> objects that
        /// were found when searching the provided SpatialString.
        /// </summary>
        /// <param name="ocrOutput">A SpatialString to be searched for matches.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="MatchResult"/> objects that
        /// were found when searching the <paramref name="ocrOutput"/>.</returns>
        List<MatchResult> GetMatches(SpatialString ocrOutput);

        /// <summary>
        /// Indicates whether the rule uses clues or not.
        /// </summary>
        /// <returns><see langword="true"/> if the rule uses clues. <see langword="false"/>
        /// if the rule does not use clues.</returns>
        bool UsesClues
        {
            get;
        }
    }
}
