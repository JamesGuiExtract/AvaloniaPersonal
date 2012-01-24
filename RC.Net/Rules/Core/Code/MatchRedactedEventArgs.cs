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
        /// Indicates whether this was the first match as part of a single redact operation.
        /// </summary>
        readonly bool _firstMatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchRedactedEventArgs"/> class.
        /// </summary>
        /// <param name="match">The match that was selected for redaction.</param>
        /// <param name="firstMatch"><see langword="true"/> for the first (or only) match as part of
        /// a single redact operation, otherwise, <see langword="false"/>.</param>
        public MatchRedactedEventArgs(MatchResult match, bool firstMatch)
        {
            _match = match;
            _firstMatch = firstMatch;
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

        /// <summary>
        /// Gets a value indicating whether this was the first match as part of a single redact
        /// operation.
        /// </summary>
        /// <value><see langword="true"/> for the first (or only) match; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool FirstMatch
        {
            get
            {
                return _firstMatch;
            }
        }
    }
}