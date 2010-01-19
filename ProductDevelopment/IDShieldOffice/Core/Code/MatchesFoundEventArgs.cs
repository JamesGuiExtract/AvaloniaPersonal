using System;
using System.Collections.ObjectModel;

namespace IDShieldOffice
{	
    /// <summary>
    /// Provides data for the <see cref="IDShieldOfficeRuleForm.MatchesFound"/> event.
    /// </summary>
    public class MatchesFoundEventArgs : EventArgs
    {
        /// <summary>
        /// The matches that were found.
        /// </summary>
        readonly ReadOnlyCollection<MatchResult> _matches;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchesFoundEventArgs"/> class.
        /// </summary>
        /// <param name="matches">The matches that were found.</param>
        public MatchesFoundEventArgs(ReadOnlyCollection<MatchResult> matches)
        {
            _matches = matches;
        }

        /// <summary>
        /// Gets the matches that were found.
        /// </summary>
        /// <value>The matches that were found.</value>
        public ReadOnlyCollection<MatchResult> Matches
        {
            get
            {
                return _matches;
            }
        }
    }
}