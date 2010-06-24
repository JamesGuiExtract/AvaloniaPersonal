using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.Rules
{
    /// <summary>
    /// A class that implements <see cref="IRule"/> that will search a
    /// a SpatialString for text contained between pairs of specified brackets.
    /// </summary>
    public class BracketedTextRule : IRule, IDisposable
    {
        #region Constants

        /// <summary>
        /// The name for this rule (for use with specifying the rule that produced a particular
        /// match result).
        /// </summary>
        const string _RULE_NAME = "Bracketed text rule";

        /// <summary>
        /// The Regex for matching nested balanced square brackets
        /// </summary>
        static readonly string _MATCH_SQUARE_BRACKETS =
              @"(?<Square_brackets>                             " + "\n"
            + @"\[(?!\s*\])                    # match opening [         " + "\n"
            + @"  (?>                                           " + "\n"
            + @"      [^\[\]]         # match all except [ or ] " + "\n"
            + @"      | \[(?<DEPTH>)  # if [ increase depth     " + "\n"
            + @"      | \](?<-DEPTH>) # if ] decrease depth     " + "\n"
            + @"  )*                                            " + "\n"
            + @"  (?(DEPTH)(?!))      # if depth is not zero, not balanced, no match" + "\n"
            + @"\]                    # match closing ]         " + "\n"
            + @")                                               " + "\n";

        /// <summary>
        /// The Regex for matching nested balanced curved brackets
        /// </summary>
        static readonly string _MATCH_CURVED_BRACKETS = 
              @"(?<Curved_brackets>                             " + "\n"
            + @"\((?!\s*\))                    # match opening (         " + "\n"
            + @"  (?>                                           " + "\n"
            + @"      [^()]           # match all except ( or ) " + "\n"
            + @"      | \((?<DEPTH>)  # if ( increase depth     " + "\n"
            + @"      | \)(?<-DEPTH>) # if ) decrease depth     " + "\n"
            + @"  )*                                            " + "\n"
            + @"  (?(DEPTH)(?!))      # if depth is not zero, not balanced, no match" + "\n"
            + @"\)                    # match closing )         " + "\n"
            + @")                                               " + "\n";

        /// <summary>
        /// The Regex for matching nested balanced curly brackets
        /// </summary>
        static readonly string _MATCH_CURLY_BRACKETS =
              @"(?<Curly_brackets>                              " + "\n"
            + @"\{(?!\s*\})                    # match opening {         " + "\n"
            + @"  (?>                                           " + "\n"
            + @"      [^{}]           # match all except { or } " + "\n"
            + @"      | \{(?<DEPTH>)  # if { increase depth     " + "\n"
            + @"      | \}(?<-DEPTH>) # if } decrease depth     " + "\n"
            + @"  )*                                            " + "\n"
            + @"  (?(DEPTH)(?!))      # if depth is not zero, not balanced, no match" + "\n"
            + @"\}                    # match closing )         " + "\n"
            + @")                                               " + "\n";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(BracketedTextRule).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Flag to specify whether to search for matching square brackets ([...]).
        /// </summary>
        bool _matchSquareBrackets;

        /// <summary>
        /// Flag to specify whether to search for matching curved brackets ((...)).
        /// </summary>
        bool _matchCurvedBrackets;

        /// <summary>
        /// Flag to specify whether to search for matching curly brackets ({...}).
        /// </summary>
        bool _matchCurlyBrackets;

        /// <summary>
        /// The property page for this rule object.
        /// </summary>
        BracketedTextRulePropertyPage _propertyPage;

        /// <summary>
        /// The regular expression pattern to search.
        /// </summary>
        Regex _regex;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new <see cref="BracketedTextRule"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="BracketedTextRule"/> class. 
        /// </summary>
        public BracketedTextRule() : this(false, false, false)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BracketedTextRule"/> class with the specified
        /// bracket matching flags.
        /// </summary>
        /// <param name="matchSquareBrackets">If <see langword="true"/> then will match
        /// text contained within square brackets ([...]).</param>
        /// <param name="matchCurvedBrackets">If <see langword="true"/> then will match
        /// text contained within curved brackets ((...)).</param>
        /// <param name="matchCurlyBrackets">If <see langword="true"/> then will match
        /// text contained within curly brackets ({...}).</param>
        public BracketedTextRule(bool matchSquareBrackets, bool matchCurvedBrackets,
            bool matchCurlyBrackets)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RedactionCoreObjects, "ELI23189",
                    _OBJECT_NAME);

                _matchSquareBrackets = matchSquareBrackets;
                _matchCurvedBrackets = matchCurvedBrackets;
                _matchCurlyBrackets = matchCurlyBrackets;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI22058",
                    "Failed to initialize BracketedTextRule!", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets whether this rule will match square brackets ([...]).
        /// </summary>
        /// <value>Whether to match square brackets ([...]).</value>
        /// <returns>Whether to match square brackets ([...]).</returns>
        public bool MatchSquareBrackets
        {
            get
            {
                return _matchSquareBrackets;
            }
            set
            {
                _matchSquareBrackets = value;
            }
        }

        /// <summary>
        /// Gets or sets whether this rule will match curved brackets ((...)).
        /// </summary>
        /// <value>Whether to match square curved brackets ((...)).</value>
        /// <returns>Whether to match curved brackets ((...)).</returns>
        public bool MatchCurvedBrackets
        {
            get
            {
                return _matchCurvedBrackets;
            }
            set
            {
                _matchCurvedBrackets = value;
            }
        }

        /// <summary>
        /// Gets or sets whether this rule will match curly brackets ({...}).
        /// </summary>
        /// <value>Whether to match curly brackets ({...}).</value>
        /// <returns>Whether to match curly brackets ({...}).</returns>
        public bool MatchCurlyBrackets
        {
            get
            {
                return _matchCurlyBrackets;
            }
            set
            {
                _matchCurlyBrackets = value;
            }
        }
        #endregion Properties

        #region Methods

        void UpdateRegex()
        {
            // StringBuilder to build up regular expression
            StringBuilder builder = new StringBuilder();

            // If matching square brackets, add the square brackets Regex
            if (_matchSquareBrackets)
            {
                builder.Append(_MATCH_SQUARE_BRACKETS);
            }

            // If matching curly brackets, add the curly brackets Regex
            if (_matchCurlyBrackets)
            {
                // Check if there is already a regex, if so add the pipe character
                if (builder.Length != 0)
                {
                    builder.Append("|");
                }
                builder.Append(_MATCH_CURLY_BRACKETS);
            }

            // If matching curved brackets, add the curved brackets Regex
            if (_matchCurvedBrackets)
            {
                // Check if there is already a regex, if so add the pipe character
                if (builder.Length != 0)
                {
                    builder.Append("|");
                }
                builder.Append(_MATCH_CURVED_BRACKETS);
            }

            // If there is at least 1 regular expression, create a new regex object
            if (builder.Length > 0)
            {
                _regex = new Regex(builder.ToString(),
                    RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
            }
            else
            {
                _regex = null;
            }
        }

        #endregion Methods

        #region IRule Members

        /// <summary>
        /// Searches the specified SpatialString for the different specified bracket matches
        /// and returns a <see cref="List{T}"/> of <see cref="MatchResult"/> objects.
        /// </summary>
        /// <param name="ocrOutput">The SpatialString to be searched for matches.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="MatchResult"/> objects containing
        /// the found items in the SpatialString.</returns>
        [CLSCompliant(false)]
        public MatchResultCollection GetMatches(SpatialString ocrOutput)
        {
            try
            {
                // Update the regular expression
                UpdateRegex();

                // Ensure that the regular expression is not null
                ExtractException.Assert("ELI22120", "Regular expression must not be null!",
                    _regex != null);

                // Return the collection of matches
                return MatchResult.ComputeMatches(_RULE_NAME, _regex, ocrOutput,
                    MatchType.Match, true);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI22119",
                    "Failed finding bracketed text matches!", ex);
            }
        }

        /// <summary>
        /// Indicates whether the rule uses clues or not.
        /// </summary>
        /// <returns><see langword="true"/> if the rule uses clues. <see langword="false"/>
        /// if the rule does not use clues.</returns>
        public bool UsesClues
        {
            get
            {
                // The bracketed text rule never uses clues.
                return false;
            }
        }

        #endregion

        #region IUserConfigurableComponent Members

        /// <summary>
        /// Gets the property page for this rule object.
        /// </summary>
        public System.Windows.Forms.UserControl PropertyPage
        {
            get
            {
                // If the property page has not been created yet, create it.
                if (_propertyPage == null)
                {
                    _propertyPage = new BracketedTextRulePropertyPage(this);
                }

                return _propertyPage;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="BracketedTextRule"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="BracketedTextRule"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="BracketedTextRule"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            // Dispose of managed resources
            if (disposing)
            {
                if (_propertyPage != null)
                {
                    _propertyPage.Dispose();
                }
            }

            // No unmanaged resources to release
        }

        #endregion
    }
}
