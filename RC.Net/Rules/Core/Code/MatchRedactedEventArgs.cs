using System;

namespace Extract.Rules
{
    /// <summary>
    /// Provides data for the <see cref="RuleForm.MatchRedacted"/> event.
    /// </summary>
    public class MatchRedactedEventArgs : EventArgs
    {
        /// <summary>
        /// The match that was selected for redaction.
        /// </summary>
        readonly MatchResult _match;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchRedactedEventArgs"/> class.
        /// </summary>
        /// <param name="match">The match that was selected for redaction.</param>
        public MatchRedactedEventArgs(MatchResult match)
        {
            _match = match;
        }

        /// <summary>
        /// Gets the match that was selected for redaction.
        /// </summary>
        /// <returns>The match that was selected for redaction.</returns>
        public MatchResult Match
        {
            get
            {
                return _match;
            }
        }
    }
}