using Extract;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UCLID_COMUTILSLib;

namespace IDShieldOffice
{
    /// <summary>
    /// A class that implements <see cref="IIDShieldOfficeRule"/> that will search a
    /// SpatialString for specified words or patterns.
    /// </summary>
    internal class WordOrPatternsListRule : IIDShieldOfficeRule, IDisposable
    {
        #region Constants

        /// <summary>
        /// The name for this rule (for use with specifying the rule that produced a particular
        /// match result).
        /// </summary>
        private static readonly string _RULE_NAME = "Word or pattern list rule";

        /// <summary>
        /// The name of the group that will be captured by the specified word list.
        /// </summary>
        static readonly string _GROUP_NAME = @"(?<WordList>";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(WordOrPatternsListRule).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Flag indicating if the search is case sensitive or not.
        /// </summary>
        private bool _matchCase;

        /// <summary>
        /// Flag indicating if the word list should be treated as a regular expression.
        /// </summary>
        private bool _treatAsRegularExpression;

        /// <summary>
        /// The word/pattern list to search for.
        /// </summary>
        private string _text;

        /// <summary>
        /// The regular expression pattern to search.
        /// </summary>
        /// <remarks>
        /// <para>All <see cref="WordOrPatternsListRule"/>s can be expressed as a regular 
        /// expression regardless of whether <see cref="_treatAsRegularExpression"/> is true.
        /// </para>
        /// <para>Value is <see langword="null"/> if the regular expression has not yet been 
        /// compiled.</para>
        /// </remarks>
        private Regex _regex;

        /// <summary>
        /// The property page associated with this rule.
        /// </summary>
        private WordOrPatternListRulePropertyPage _propertyPage;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new <see cref="WordOrPatternsListRule"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="WordOrPatternsListRule"/> class. 
        /// </summary>
        public WordOrPatternsListRule() : this(false, false, "")
        {
        }

        /// <summary>
        /// Initializes a new <see cref="WordOrPatternsListRule"/> class with
        /// the specified settings.</summary>
        /// <param name="matchCase">If <see langword="true"/> will search the text
        /// in a case-sensitive fashion.</param>
        /// <param name="treatAsRegularExpression">If <see langword="true"/> will
        /// treat the search text as a regular expression pattern.</param>
        /// <param name="text">The search text.</param>
        public WordOrPatternsListRule(bool matchCase, bool treatAsRegularExpression, string text)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IdShieldOfficeObject, "ELI23208",
                    _OBJECT_NAME);

                _matchCase = matchCase;
                _treatAsRegularExpression = treatAsRegularExpression;
                _text = text;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI22057",
                    "Failed to initialize WordOrPatternListRule!", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets whether the search should be case insensitive.
        /// </summary>
        /// <value>If <see langword="true"/> then the search will be performed in
        /// a case sensitive fashion otherwise it will be case-insensitive.</value>
        /// <returns>If <see langword="true"/> then the search will be performed in
        /// a case sensitive fashion otherwise it will be case-insensitive.</returns>
        public bool MatchCase
        {
            get
            {
                return _matchCase;
            }
            set
            {
                if (_matchCase != value)
                {
                    _matchCase = value;
                    _regex = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the search text should be treated as a regular expression.
        /// </summary>
        /// <value>If <see langword="true"/> then the search text will be treated as a
        /// regular expression</value>
        /// <returns>If <see langword="true"/> then the search text will be treated as a
        /// regular expression</returns>
        public bool TreatAsRegularExpression
        {
            get
            {
                return _treatAsRegularExpression;
            }
            set
            {
                if (_treatAsRegularExpression != value)
                {
                    _treatAsRegularExpression = value;
                    _regex = null;
                }
            }
        }

        /// <summary>
        /// The text that will be searched for (may be a list of strings to search for separated by
        /// <see cref="Environment.NewLine"/>).
        /// </summary>
        /// <value>The text to search for.</value>
        /// <returns>The text to search for.</returns>
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    _regex = null;
                }
            }
        }

        #endregion Properties

        #region WordOrPatternsListRule Methods

        void UpdateRegex()
        {
            // Ensure pattern is valid.
	        ExtractException.Assert("ELI22063", "Regular expression must be specified.", 
                !String.IsNullOrEmpty(_text));

            // Update the regular expression if it is not already updated
            if (_regex == null)
            {
                // Get the lines of words/patterns
                string[] lines = _text.Split(new string[] { Environment.NewLine },
                    StringSplitOptions.RemoveEmptyEntries);

                // Check if the words need to be escaped
                if (!_treatAsRegularExpression)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        lines[i] = Regex.Escape(lines[i]);
                    }
                }

                // Build the regular expression
                string regularExpression = _GROUP_NAME + string.Join("|", lines) + ")";

                // Check if the casing should be matched
                RegexOptions options = RegexOptions.Compiled | RegexOptions.ExplicitCapture;
                if (!_matchCase)
                {
                    options |= RegexOptions.IgnoreCase;
                }

                // Store the regular expression
                _regex = new Regex(regularExpression, options);
            }
        }

        #endregion WordOrPatternsListRule Methods

        #region IIDShieldOfficeRule Members

        /// <summary>
        /// Searches the specified SpatialString for the specified search text
        /// and returns a <see cref="List{T}"/> of <see cref="MatchResult"/> objects.
        /// </summary>
        /// <param name="ocrOutput">The SpatialString to be searched for matches.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="MatchResult"/> objects containing
        /// the found items in the SpatialString.</returns>
        public List<MatchResult> GetMatches(UCLID_RASTERANDOCRMGMTLib.SpatialString ocrOutput)
        {
            // Ensure the regular expression is valid
            UpdateRegex();

            // Ensure that the regular expression is not null
            ExtractException.Assert("ELI22222", "Regular expression must not be null!",
                _regex != null);

            // Compute the matches
            List<MatchResult> result = MatchResult.ComputeMatches(_RULE_NAME, _regex,
                ocrOutput, MatchType.Match, false);

            return result;
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
                // The word pattern rule never uses clues.
                return false;
            }
        }

        #endregion

        #region IUserConfigurableComponent Members

        /// <summary>
        /// Gets or sets the property page of the <see cref="WordOrPatternsListRule"/>.
        /// </summary>
        /// <return>The property page of the <see cref="WordOrPatternsListRule"/>.</return>
        public System.Windows.Forms.UserControl PropertyPage
        {
            get
            {
                // Create the property page if not already created
                if (_propertyPage == null)
                {
                    _propertyPage = new WordOrPatternListRulePropertyPage(this);
                }

                return _propertyPage;
            }
        }

        #endregion IUserConfigurableComponent Members

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="WordOrPatternsListRule"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="WordOrPatternsListRule"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="WordOrPatternsListRule"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_propertyPage != null)
                {
                    _propertyPage.Dispose();
                    _propertyPage = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}
