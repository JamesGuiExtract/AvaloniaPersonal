using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.Rules
{
    /// <summary>
    /// Defines a method for searching a SpatialString and returning a collection
    /// of <see cref="MatchResult"/>.
    /// </summary>
    [CLSCompliant(false)]
    public interface IRule : IUserConfigurableComponent
    {
        /// <summary>
        /// Returns a <see cref="List{T}"/> of <see cref="MatchResult"/> objects that
        /// were found when searching the provided SpatialString.
        /// </summary>
        /// <param name="ocrOutput">A SpatialString to be searched for matches.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="MatchResult"/> objects that
        /// were found when searching the <paramref name="ocrOutput"/>.</returns>
        MatchResultCollection GetMatches(SpatialString ocrOutput);

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
