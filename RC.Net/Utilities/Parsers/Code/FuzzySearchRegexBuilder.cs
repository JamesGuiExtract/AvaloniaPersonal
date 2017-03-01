using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Extract.Utilities.Parsers
{
    /// <summary>
    /// A utility class to used to expand Extract Systems fuzzy regex search syntax into an
    /// equivalent regular expression.
    /// </summary>
    public static class FuzzySearchRegexBuilder
    {
        #region Private Classes

        /// <summary>
        /// Represents the options that can be applied to a fuzzy text search.
        /// </summary>
        class FuzzySearchOptions
        {
            /// <summary>
            /// Specifies whether the search should favor speed or a match that includes as few
            /// unmatched chars as possible.
            /// </summary>
            public enum Method
            {
                /// <summary>
                /// Specifies that the resulting expression should favor speed over finding the best
                /// fit match.
                /// </summary>
                Fast,

                /// <summary>
                /// Specifies that the resulting expression should favor finding a good fit over
                /// speed.
                /// </summary>
                BetterFit
            };

            /// <summary>
            /// The search method to use. 
            /// </summary>
            public Method SearchMethod = Method.Fast;

            /// <summary>
            /// The number of errors allowed before a potential match is discarded.
            /// </summary>
            public uint ErrorsAllowed = 1;

            /// <summary>
            /// The number of extra whitespace chars allowed in addition to the number of errors
            /// allowed before a potential match is discarded.
            /// </summary>
            public uint ExtraWhitespaceAllowed;

            /// <summary>
            /// The pattern to use for substitute characters.
            /// </summary>
            public string SubstitutePattern = _DEFAULT_SUBSTITUTE_PATTERN;

            /// <summary>
            /// The pattern to use for extra whitespace.
            /// </summary>
            public string WhitespacePattern = _DEFAULT_WHITESPACE_PATTERN;

            /// <summary>
            /// Whether to omit numbers from balancing stack names.
            /// </summary>
            public bool UseGlobalNames;

            /// <summary>
            /// Whether to escape literal space characters in patterns.
            /// </summary>
            public bool EscapeSpaceChars;

            /// <summary>
            /// List of character replacements to run against the search string before expansion
            /// </summary>
            public List<KeyValuePair<string,string>> CharacterReplacements = new List<KeyValuePair<string,string>>();
        }

        /// <summary>
        /// Represents the stack names to be used for a fuzzy search regular expression.
        /// </summary>
        class BalancingRegexStackNames
        {
            /// <summary>
            /// Specifies whether the initial or final value will be used for
            /// <see cref="ErrorStack"/>, <see cref="ExtraSpaceStack"/> and <see cref="MissedStack"/>.
            /// </summary>
            bool _useInitialStackSet = true;

            /// <summary>
            /// The name to use for the initial error stack.
            /// </summary>
            string _initialErrorStack;

            /// <summary>
            /// The name to use for the initial extra space stack.
            /// </summary>
            string _initialExtraSpaceStack;

            /// <summary>
            /// The name to use for the initial missed stack.
            /// </summary>
            string _initialMissedStack;

            /// <summary>
            /// The name to use for the final error stack.
            /// </summary>
            string _finalErrorStack;

            /// <summary>
            /// The name to use for the final extra space stack.
            /// </summary>
            string _finalExtraSpaceStack;

            /// <summary>
            /// The name to use for the final missed stack.
            /// </summary>
            string _finalMissedStack;

            /// <summary>
            /// The name to use for the allowable lookahead stack.
            /// </summary>
            string _allowableLookAheadStack;

            /// <summary>
            /// The name to use for the actual matched string stack.
            /// </summary>
            string _actualMatchedString;

            /// <summary>
            /// Initializes a new <see cref="BalancingRegexStackNames"/> instance with stack names
            /// that correspond to the specified id.
            /// </summary>
            /// <param name="id">A <see langword="int"/> used to differentiate this set of stack
            /// names from stack names used for other fuzzy searches in the same regex. If -1 then
            /// stack names will be 'global' (have no number suffix).</param>
            public BalancingRegexStackNames(int id)
            {
                // Initialize the unique string to be added to each stack name.
                string termIdentifier = id == -1 ? "" : id.ToString(CultureInfo.InvariantCulture);

                // Initialize the stack names using termIdentifier.
                _initialErrorStack = "_ie" + termIdentifier;
                _initialExtraSpaceStack = "_ies" + termIdentifier;
                _initialMissedStack = "_im" + termIdentifier;
                _finalErrorStack = "_fe" + termIdentifier;
                _finalExtraSpaceStack = "_fes" + termIdentifier;
                _finalMissedStack = "_fm" + termIdentifier;
                _allowableLookAheadStack = "_al" + termIdentifier;
                _actualMatchedString = "_ams" + termIdentifier;
            }

            /// <summary>
            /// Gets or sets whether the initial or final value will be used for
            /// <see cref="ErrorStack"/>, <see cref="ExtraSpaceStack"/> and <see cref="MissedStack"/>.
            /// </summary>
            /// <value><see langword="true"/> if the initial values should be used,
            /// <see langword="false"/> if the final values should be used.</value>
            /// <returns><see langword="true"/> if the initial values are being used,
            /// <see langword="false"/> if the final values are being used.</returns>
            public bool UseInitialStackSet
            {
                get
                {
                    return _useInitialStackSet;
                }

                set
                {
                    _useInitialStackSet = value;
                }
            }

            /// <summary>
            /// Gets either the initial or final error stack name depending upon the value of
            /// <see cref="UseInitialStackSet"/>.
            /// </summary>
            /// <returns>The error stack name.</returns>
            public string ErrorStack
            {
                get
                {
                    return UseInitialStackSet ? _initialErrorStack : _finalErrorStack;
                }
            }

            /// <summary>
            /// Gets either the initial or final extra space stack name depending upon the value of
            /// <see cref="UseInitialStackSet"/>.
            /// </summary>
            /// <returns>The extra space stack name.</returns>
            public string ExtraSpaceStack
            {
                get
                {
                    return UseInitialStackSet ? _initialExtraSpaceStack : _finalExtraSpaceStack;
                }
            }

            /// <summary>
            /// Gets either the initial or final missed stack name depending upon the value of
            /// <see cref="UseInitialStackSet"/>.
            /// </summary>
            /// <returns>The missed stack name.</returns>
            public string MissedStack
            {
                get
                {
                    return UseInitialStackSet ? _initialMissedStack : _finalMissedStack;
                }
            }

            /// <summary>
            /// Gets the final error stack name.
            /// </summary>
            /// <returns>The final error stack name.</returns>
            public string FinalErrorStack
            {
                get
                {
                    return _finalErrorStack;
                }
            }

            /// <summary>
            /// Gets the final extra space stack name.
            /// </summary>
            /// <returns>The final extra space stack name.</returns>
            public string FinalExtraSpaceStack
            {
                get
                {
                    return _finalExtraSpaceStack;
                }
            }

            /// <summary>
            /// Gets the allowable look-ahead stack name.
            /// </summary>
            /// <returns>The allowable look-ahead stack name.</returns>
            public string AllowableLookAheadStack
            {
                get
                {
                    return _allowableLookAheadStack;
                }
            }

            /// <summary>
            /// Gets the actual matched string stack name.
            /// </summary>
            /// <returns>The actual matched string stack name.</returns>
            public string ActualMatchedString
            {
                get
                {
                    return _actualMatchedString;
                }
            }
        }

        /// <summary>
        /// Represents a single search token with no qualifier, an optional qualifier,
        /// or a required qualifier
        /// </summary>
        struct SearchToken
        {
            /// <summary>
            /// The token's value
            /// </summary>
            public string value;

            /// <summary>
            /// Whether this token can be missing without affecting the score of the match
            /// </summary>
            public bool optional;

            /// <summary>
            /// Whether this token is not allowed to be missing or substituted
            /// </summary>
            public bool required;
        }

        #endregion Private Classes

        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FuzzySearchRegexBuilder).ToString();

        /// <summary>
        /// The options to be used internally for regex pattern matching
        /// </summary>
        const RegexOptions _REGEX_OPTIONS = RegexOptions.ExplicitCapture
            | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;

        // The default pattern for what will be considered as whitespace for the fuzzy regex
        // xtra_ws option.
        const string _DEFAULT_WHITESPACE_PATTERN = @"\s";

        // The default pattern allowed for token substitutions for the fuzzy regex
        const string _DEFAULT_SUBSTITUTE_PATTERN = ".";

        // Regex pattern that matches a fuzzy regex pattern
        const string _FUZZY_SEARCH_EXPRESSION = _PREFIX + _OPTIONS + "(?'search_string'" + RegexParsingPatterns.VALID_SEQUENCE + @")\)";

        // The prefix of a fuzzy pattern (non-escaped open parenthesis followed by '?~')
        const string _PREFIX = @"(?<=(^|[^\\])(\\\\)*)\(\?~";

        // The options for the fuzzy pattern
        const string _OPTIONS = @"<(?>(?>((?<=<)|,)\s*" + _OPTION + @"\s*(?=,|>))*)>";

        // A single option (name=value)
        const string _OPTION = @"(?'option_name'\w+)\s*=\s*(?'option_value'" + RegexParsingPatterns.VALID_SEQUENCE + ")";

        // A valid fuzzy search token, consists of a search token, an optional, fixed-length
        // quantifier and an optional qualifier (+ meaning the term is required and ? meaning
        // that the term can be missing without affecting the error count)
        const string _TOKEN =
            @"\G(?'search_token'" + RegexParsingPatterns.VALID_SEQUENCE + @")({(?'repetitions'\d+)})?(?'qualifier'[+?])?";

        // A sub-token of a fuzzy search token. This is used to break apart a token that is a
        // parenthesized group token before performing any specified replacements on its pieces.
        const string _SUB_TOKEN =
            @"\G(?'search_token'" + RegexParsingPatterns.VALID_SEQUENCE + @")(?'quantifier'" + _QUANTIFIER + @")?";

        // A .NET regex quantifier (can be variable length and non-greedy)
        const string _QUANTIFIER = @"({\d+(,(\d+)?)?} | [?*+])\??";

        const string _REPLACEMENT_PAIR_EXPRESSION = @"\((?'replace'" + RegexParsingPatterns.VALID_SEQUENCE
            + @")=>(?'replacement'" + RegexParsingPatterns.VALID_SEQUENCE + @")\)";
        const string _REPLACEMENTS_EXPRESSION = _REPLACEMENT_PAIR_EXPRESSION + @"(\s*" + _REPLACEMENT_PAIR_EXPRESSION + @")*";

        #endregion Constants

        static ThreadLocal<Regex> _fuzzySearchRegex = new ThreadLocal<Regex>(() =>
            new Regex(_FUZZY_SEARCH_EXPRESSION, _REGEX_OPTIONS));
        static ThreadLocal<Regex> _prefixRegex = new ThreadLocal<Regex>(() =>
            new Regex(_PREFIX, _REGEX_OPTIONS));
        static ThreadLocal<Regex> _tokenRegex = new ThreadLocal<Regex>(() =>
            new Regex(_TOKEN, _REGEX_OPTIONS));
        static ThreadLocal<Regex> _subTokenRegex = new ThreadLocal<Regex>(() =>
            new Regex(_SUB_TOKEN, _REGEX_OPTIONS));
        static ThreadLocal<Regex> _replacementsRegex = new ThreadLocal<Regex>(() =>
            new Regex(_REPLACEMENTS_EXPRESSION, _REGEX_OPTIONS));

        #region Public Methods

        /// <summary>
        /// Expands all fuzzy search strings withing the specified regex to produce a resulting
        /// regular expression that will perform the search specified.
        /// <para><b>Syntax:</b></para>
        /// <para>(?~&lt;options&gt;search_string)</para>
        /// <para>Where 'options' is a comma separated list of the following possible options:</para>
        /// <list type="bullet">
        /// <item><term>method=fast|better_fit (default=fast)</term><description>
        ///     fast will execute faster but match characters outside of the specified search string
        ///     depending upon how many of the errors allowed were found in a match.
        ///     better_fit will match only the specified search string but will take longer to
        ///     execute.</description></item>
        /// <item><term>error=number (default = 1)</term><description>
        ///     The number of errors (missing or wrong characters) that are allowed in a potential
        ///     match before the match is discarded.</description></item>
        /// <item><term>escape_space_chars|esc_s=true|false (default = false)</term><description>
        ///     Whether literal space characters (' ') will be escaped ('\ '). This only works in
        ///     some contexts because often whitespace is trimmed from the source expression prior to
        ///     fuzzy expansion.</description></item>
        /// <item><term>extra_ws|xtra_ws=number (default = 0)</term><description>
        ///     The number of extra whitespace chars that are allowed outside of the number of
        ///     errors that are allowed before disqualifying a potential match.</description></item>
        /// <item><term>substitute_pattern|sub=pattern (default = .)</term><description>
        ///     The pattern that defines what a substitute (error) token can be. Any valid, non-empty
        ///     regular expression will be accepted but if the pattern contains an unescaped '&gt;' or ','
        ///     character then it should be enclosed in parentheses to remove possible ambiguity.</description></item>
        /// <item><term>use_global_names|global|g=true|false (default = false)</term><description>
        ///     If use_global_names=true then the group names used in the expanded pattern will not
        ///     be unique to each pattern in the source regex. Only use this option if you understand
        ///     the implications of this.</description></item>
        /// <item><term>ws_pattern|ws=pattern (default = \s)</term><description>
        ///     The pattern that defines what is considered to be a whitespace character. Does not
        ///     need to match only a single character; any valid, non-empty regular expression will
        ///     be accepted but if the pattern contains an unescaped '&gt;' or ',' character then it
        ///     should be enclosed in parentheses to remove possible ambiguity.</description></item>
        /// <item><term>replacements=(replace=&gt;replacement)... (default = no replacements defined)</term><description>
        ///     Token replacements that will be made to the search string after it has been broken
        ///     into tokens.
        ///     <para>The replace part is a .NET regular expression that must match an entire token or an
        ///     entire sub-token for the replacement to occur. The replacement must be a valid .NET
        ///     regular expression to avoid ambiguities in parsing the list of pairs
        ///     (e.g., no unclosed parentheses). The replacements will not be made against tokens
        ///     that have sub-tokens, against sub-sub-tokens nor against any quantifier portions of
        ///     the regex, e.g., {1,2}.</para></description></item>
        /// <item><term>'search_string'</term><description>is the string to search for. The string will be treated a literal text
        /// except in that:
        /// <list type="number"><item>Regex escape sequences (ie. \d, \x20, \040, \cC, \u0020), and character
        /// classes whether specified with a backslash or that are enclosed in unescaped square 
        /// brackets (ie. [\da-eA-E]) will be taken together as a token representing a single char
        /// in the text to be searched.</item>
        /// <item>A number enclosed in curly braces except at the beginning of the search string will
        /// repeat the previous token the specified number of times. (ie: \d{9} or [\da-eA-E]{3}).</item>
        /// <item>Top-level parenthesized groups will be treated as single tokens as far as errors and
        /// extra whitespace are concerned but replacements will only occur on sub-tokens.</item>
        /// <item>A + or ? after a token will determine whether the token is required (substitution
        /// or omission not allowed) or optional (in which case omission will not count as an error).</item>
        /// </list></description></item></list>
        /// </summary>
        /// <param name="sourceRegex">The regular expression in which fuzzy search syntax should be
        /// expanded out into the equivalent regex.</param>
        /// <returns>A regular expression in which fuzzy search syntax is in regex form ready to be
        /// parsed by the .NET regex engine.</returns>
        public static string ExpandFuzzySearchExpressions(string sourceRegex)
        {
            try
            {
                ValidateLicense();

                StringBuilder expandedSearchString = new StringBuilder();
                int expansionPos = 0;

                // Process each string within sourceRegex that matches the fuzzy search pattern.
                MatchCollection matches = _fuzzySearchRegex.Value.Matches(sourceRegex);
                foreach (Match match in matches)
                {
                    // Add to the result the part of sourceRegex prior to the start of the fuzzy
                    // search pattern.
                    if (match.Index > expansionPos)
                    {
                        expandedSearchString.Append(
                            sourceRegex.Substring(expansionPos, match.Index - expansionPos));
                    }

                    // Update the position in the sourceRegex to be the first char after the fuzzy
                    // search pattern.
                    expansionPos = match.Index + match.Length;

                    // Obtain the options to use when performing this fuzzy search
                    FuzzySearchOptions options = GetFuzzySearchOptions(match);

                    // Parse the search string into a list of tokens.
                    List<SearchToken> searchTokens = GetSearchTokens(match, options);

                    if (searchTokens.Count == 1)
                    {
                        expandedSearchString.Append(searchTokens[0]);
                    }
                    else
                    {

                        ExtractException.Assert("ELI28335", "Number of fuzzy search errors allowed " +
                            "cannot be more than the length of the string to search for.",
                            options.ErrorsAllowed <= searchTokens.Count);

                        // Create a new set of stack names that will be unique (unless UseGlobalNames=true)
                        // to this particular fuzzy search (so that the stack names are not repeated
                        // within the same sourceRegex)
                        BalancingRegexStackNames stackNames =
                            new BalancingRegexStackNames(options.UseGlobalNames ? -1 : match.Index);

                        // Create expression prefix to initialize error and extra space stack counts.
                        expandedSearchString.Append(@"(?nx:");
                        expandedSearchString.Append(
                            IncrementStack(stackNames.ErrorStack, (int)options.ErrorsAllowed));
                        expandedSearchString.Append(
                            IncrementStack(stackNames.MissedStack, 0));
                        if (options.ExtraWhitespaceAllowed != 0)
                        {
                            expandedSearchString.Append(IncrementStack(stackNames.ExtraSpaceStack,
                                (int)options.ExtraWhitespaceAllowed));
                        }
                        else if (options.UseGlobalNames)
                        {
                            expandedSearchString.Append(IncrementStack(stackNames.ExtraSpaceStack, 0));
                        }
                        expandedSearchString.AppendLine();

                        // For better fit method, create named group to store the matched string.
                        if (options.SearchMethod == FuzzySearchOptions.Method.BetterFit)
                        {
                            expandedSearchString.Append(@"(?'");
                            expandedSearchString.Append(stackNames.ActualMatchedString);
                            expandedSearchString.Append(@"'");
                            expandedSearchString.AppendLine();
                        }

                        // Leading whitespace not allowed for the first token of the better_fit method.
                        // Don't allow leading space if ExtraWhitespaceAllowed == 0, unless using global
                        // names (because if using global names then extra whitespace might be specified
                        // outside the scope of this expression).
                        bool allowLeadingSpace =
                            options.SearchMethod != FuzzySearchOptions.Method.BetterFit
                            && (options.UseGlobalNames
                                 || options.ExtraWhitespaceAllowed != 0);

                        // Create a term that searches for each token in the search string and adjusts
                        // stack counts accordingly for each miss.
                        for (int i = 0; i < searchTokens.Count; i++)
                        {
                            SearchToken searchToken = searchTokens[i];

                            AddTokenSearchTerm(expandedSearchString, searchToken, stackNames,
                                options.SearchMethod == FuzzySearchOptions.Method.BetterFit,
                                allowLeadingSpace, options.SubstitutePattern, options.WhitespacePattern);

                            // Allow leading whitespace starting with the first token following a
                            // non zero width assertion token.
                            // Don't allow leading space if ExtraWhitespaceAllowed == 0 unless using global names
                            if (options.UseGlobalNames || options.ExtraWhitespaceAllowed != 0)
                            {
                                allowLeadingSpace |= !isZeroWidthAssertion(searchToken.value);
                            }
                        }

                        // If using better fit method, create another set of token search terms that
                        // lookahead to try to find a better match.
                        if (options.SearchMethod == FuzzySearchOptions.Method.BetterFit)
                        {
                            // Close the ActualMatchedString search token group
                            expandedSearchString.AppendLine(@")");

                            // Open a new search token group that attempts to lookahead to avoid
                            // matching any leading chars that are not part of the actual matched string.
                            expandedSearchString.AppendLine(@"(?<=");

                            expandedSearchString.Append(@"(?!");

                            // Don't allow leading space if ExtraWhitespaceAllowed == 0, unless using global names
                            allowLeadingSpace = options.UseGlobalNames || options.ExtraWhitespaceAllowed != 0;

                            // Decrement the error or extra space stack
                            if (allowLeadingSpace)
                            {
                                expandedSearchString.Append(@"(");
                                expandedSearchString.Append(
                                    IncrementStack(stackNames.FinalExtraSpaceStack, -1));
                                expandedSearchString.Append(@"|");
                            }
                            expandedSearchString.Append(
                                IncrementStack(stackNames.FinalErrorStack, -1));
                            if (allowLeadingSpace)
                            {
                                expandedSearchString.Append(@")");
                            }
                            expandedSearchString.Append(@"(");
                            expandedSearchString.Append(
                                IncrementStack(stackNames.AllowableLookAheadStack, -1));

                            // If using default whitespace and substitute patterns then it use a
                            // simple character class instead of the union to make the expression faster.
                            if (options.WhitespacePattern == _DEFAULT_WHITESPACE_PATTERN
                                && options.SubstitutePattern == _DEFAULT_SUBSTITUTE_PATTERN)
                            {
                                expandedSearchString.AppendLine(@"[\S\s])+?");
                            }
                            else
                            {
                                expandedSearchString.AppendLine("("+options.WhitespacePattern + "|" + options.SubstitutePattern + "))+?");
                            }

                            // Switch to use the final stack name set for the following group of token
                            // search terms.
                            stackNames.UseInitialStackSet = false;

                            // Leading whitespace not allowed for the first token of the better fit method.
                            allowLeadingSpace = false;

                            // Generate the second set of search token terms.
                            for (int i = 0; i < searchTokens.Count; i++)
                            {
                                SearchToken searchToken = searchTokens[i];
                                bool zeroWidthAssertion = isZeroWidthAssertion(searchToken.value);

                                AddTokenSearchTerm(expandedSearchString, searchToken, stackNames, false,
                                    allowLeadingSpace, options.SubstitutePattern, options.WhitespacePattern);

                                // Allow leading whitespace starting with the first token following a
                                // non zero width assertion token.
                                if (options.ExtraWhitespaceAllowed != 0)
                                {
                                    allowLeadingSpace |= !zeroWidthAssertion;
                                }
                            }

                            // End the lookahead term.
                            expandedSearchString.AppendLine(@")");
                            expandedSearchString.Append(@"\k'");
                            expandedSearchString.Append(stackNames.ActualMatchedString);
                            expandedSearchString.Append(@"')");
                        }

                        // End the fuzzy search regex expansion group
                        expandedSearchString.Append(@")");
                    }
                }

                // If there is any remaining part of sourceRegex that has not been parsed as part of
                // the fuzzy search expansion, append it to the end of the expanded term.
                if (expansionPos < sourceRegex.Length)
                {
                    expandedSearchString.Append(
                        sourceRegex.Substring(expansionPos, sourceRegex.Length - expansionPos));
                }

                string result = expandedSearchString.ToString();

                ExtractException.Assert("ELI38826", "Unable to expand one or more fuzzy search patterns.",
                    !_prefixRegex.Value.IsMatch(result));

                return result;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28336", ex);
            }
        }

        /// <summary>
        /// Determines if the specified search token is a zero-width assertion.
        /// </summary>
        /// <param name="token">The token to test.</param>
        /// <returns><see langword="true"/> if the token is a zero-witch assertion,
        /// <see langword="false"/> otherwise</returns>
        static bool isZeroWidthAssertion(string token)
        {
            if (token.Equals(@"^", StringComparison.Ordinal) ||
                token.Equals(@"$", StringComparison.Ordinal) ||
                token.Equals(@"\b", StringComparison.OrdinalIgnoreCase) ||
                token.Equals(@"\z", StringComparison.OrdinalIgnoreCase) ||
                token.Equals(@"\G", StringComparison.Ordinal) ||
                token.Equals(@"\A", StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Appends a regular expression to the specified string builder which searches for a
        /// character based on the specified token and increments/decrements balancing group stacks
        /// appropriately for missed.
        /// </summary>
        /// <param name="regEx">The <see cref="StringBuilder"/> to which the resulting regex term
        /// should be added.</param>
        /// <param name="searchToken">A regex token that specifies what constitutes a matching char
        /// for this term.</param>
        /// <param name="stackNames">The names of the stacks to be modified in the event of a miss.
        /// </param>
        /// <param name="initialBetterFitTerm"><see langword="true"/> to build a term for the first
        /// part of a better fit expression.</param>
        /// <param name="allowLeadingSpace"><see langword="true"/> to allow for a space prior to
        /// matching the search token, <see langword="false"/> to require the next char to match
        /// the search token.</param>
        /// <param name="substitutePattern">The pattern to use for substituted characters.</param>
        /// <param name="whitespacePattern">The pattern to use for whitespace.</param>
        static void AddTokenSearchTerm(StringBuilder regEx, SearchToken searchToken,
            BalancingRegexStackNames stackNames, bool initialBetterFitTerm, bool allowLeadingSpace,
            string substitutePattern, string whitespacePattern)
        {
            // Open a group by attempting to match the search token.
            regEx.Append(@"  (");

            // Add a term to reflect a char that does not correspond to the search token.
            AddIgnoreTokenTerm(regEx, stackNames, initialBetterFitTerm, allowLeadingSpace,
                substitutePattern, whitespacePattern);

            AddMatchTokenTerm(regEx, stackNames, searchToken.value);

            if (!searchToken.required)
            {
                regEx.Append(@"|");

                if (!searchToken.optional)
                {
                    // Add a token to allow for a missing token.
                    AddMissingTokenTerm(regEx, stackNames, initialBetterFitTerm);
                }
            }

            regEx.AppendLine(@")");
        }

        /// <summary>
        /// Adds a regular expression that will update the stacks to correspond with search token
        /// that was not found (missing).
        /// </summary>
        /// <param name="term">A <see cref="StringBuilder"/> to which the regular expression term
        /// should be appended.</param>
        /// <param name="stackNames">The set of stack names to be modified.</param>
        /// <param name="initialBetterFitTerm"><see langword="true"/> to build a term for the first
        /// part of a better fit expression.</param>
        static void AddMissingTokenTerm(StringBuilder term, BalancingRegexStackNames stackNames,
            bool initialBetterFitTerm)
        {
            // Decrement the error stack
            term.Append(IncrementStack(stackNames.ErrorStack, -1));

            term.Append(IncrementStack(stackNames.MissedStack, 1));

            // Increment the number of errors the better fit mode will try to remove from the
            // initial match.
            if (initialBetterFitTerm)
            {
                term.Append(IncrementStack(stackNames.FinalErrorStack, 1));
            }
        }

        /// <summary>
        /// Adds a regular expression that will update the stacks to correspond with search token
        /// that was paired with a character that does not match the current token.
        /// </summary>
        /// <param name="term">A <see cref="StringBuilder"/> to which the regular expression term
        /// should be appended.</param>
        /// <param name="stackNames">The set of stack names to be modified.</param>
        /// <param name="initialBetterFitTerm"><see langword="true"/> to build a term for the
        /// first part of a better fit expression.</param>
        /// <param name="allowLeadingSpace"><see langword="true"/> to allow for a space prior to
        /// matching the search token, <see langword="false"/> to require the next char to match
        /// the search token.</param>
        /// <param name="substitutePattern">The pattern to use for substituted characters.</param>
        /// <param name="whitespacePattern">The pattern to use for whitespace.</param>
        static void AddIgnoreTokenTerm(StringBuilder term, BalancingRegexStackNames stackNames,
            bool initialBetterFitTerm, bool allowLeadingSpace, string substitutePattern,
            string whitespacePattern)
        {
            // Decrement the error stack or the last miss that had been added to the missed
            // chars stack.
            term.Append(@"(");
            term.Append(substitutePattern);
            term.Append(@"(?>");
            term.Append(IncrementStack(stackNames.MissedStack, -1));
            term.Append(@"|");
            term.Append(IncrementStack(stackNames.ErrorStack, -1));

            // Increment the number of errors the better fit mode will try to remove from the
            // initial match and distance the better fit mode can look ahead.
            if (initialBetterFitTerm)
            {
                term.Append(IncrementStack(stackNames.FinalErrorStack, 1));
                term.Append(@")");
                term.Append(IncrementStack(stackNames.AllowableLookAheadStack, 1));
            }
            else
            {
                term.Append(@")");
            }

            // Allow for space chars before matching the search token if specified.
            if (allowLeadingSpace)
            {
                term.Append(@"|");
                AddExtraSpaceTerm(term, stackNames, initialBetterFitTerm, whitespacePattern);
            }


            term.Append(@")*?");
        }

        /// <summary>
        /// Generates a regular expression token that checks for a character that matches the
        /// specified search token.
        /// </summary>
        /// <param name="term">A <see cref="StringBuilder"/> to which the regular expression term
        /// should be appended.</param>
        /// <param name="stackNames">The set of stack names to be modified.</param>
        /// <param name="searchToken">A character or character class representing a match for the
        /// current term.</param>
        static void AddMatchTokenTerm(StringBuilder term, BalancingRegexStackNames stackNames,
             string searchToken)
        {
            term.Append(searchToken);
            term.Append(@"(?(");
            term.Append(stackNames.MissedStack);
            term.Append(@")");

            term.Append(IncrementStack(stackNames.MissedStack, -1));
            term.Append(@")");
        }

        /// <summary>
        /// Adds a regular expression that will update the stacks to correspond with an extra
        /// whitespace character found before or after the char matched to the current search
        /// token.
        /// </summary>
        /// <param name="term">A <see cref="StringBuilder"/> to which the regular expression term
        /// should be appended.</param>
        /// <param name="stackNames">The set of stack names to be modified.</param>
        /// <param name="initialBetterFitTerm"><see langword="true"/> to build a term for the first
        /// part of a better fit expression.</param>
        /// <param name="whitespacePattern">The pattern to use for whitespace.</param>
        static void AddExtraSpaceTerm(StringBuilder term, BalancingRegexStackNames stackNames,
            bool initialBetterFitTerm, string whitespacePattern)
        {
            // Decrement the extra space stack
            term.Append(IncrementStack(stackNames.ExtraSpaceStack, -1));

            // Increment the number of extra spaces the better fit mode will try to remove from the
            // initial match
            if (initialBetterFitTerm)
            {
                term.Append(IncrementStack(stackNames.FinalExtraSpaceStack, 1));
            }

            term.Append(whitespacePattern);
        }

        /// <summary>
        /// Creates a regular expression term that increments the number of items on the specified
        /// stack by the specified amount.
        /// </summary>
        /// <param name="stackName">The stack whose value is to be modified.</param>
        /// <param name="amount">The number that should be added (or removed) from the specified
        /// stack.</param>
        /// <returns>A regular expression term that increments the number of items on the specified
        /// stack by the specified amount.</returns>
        static string IncrementStack(string stackName, int amount)
        {
            string stackTerm = string.Format(CultureInfo.InvariantCulture,
                    amount >= 0 ? "(?'{0}')" : "(?'-{0}')", stackName);

            int magnitude = Math.Abs(amount);
            if (magnitude != 1)
            {
                stackTerm += string.Format(CultureInfo.InvariantCulture, "{{{0}}}", magnitude);
            }

            return stackTerm;
        }

        /// <summary>
        /// Retrieves a list of search tokens from the specified fuzzy search syntax match.
        /// </summary>
        /// <param name="match">A match containing fuzzy search syntax (obtained from
        /// _fuzzySearchExpressionParserRegex).</param>
        /// <returns>A list of tokens, each of which is to be used to match a single char in the
        /// searched text.</returns>
        /// <param name="options">The <see cref="FuzzySearchOptions"/> for this match.</param>
        static List<SearchToken> GetSearchTokens(Match match, FuzzySearchOptions options)
        {
            string searchString = "[not found]";

            try
            {
                // Retrieve the named group that contains the search string to be parsed.
                Group searchStringGroup = match.Groups["search_string"];
                searchString = searchStringGroup.Value;

                // Use the _TOKEN regex to split the search string into tokens.
                MatchCollection matches = _tokenRegex.Value.Matches(searchString);
                List<SearchToken> searchTokens = new List<SearchToken>(matches.Count);

                // Generate a list of tokens that need to be escaped to avoid corrupting 
                // the resulting regex.
                char[] tokensToEscape = options.EscapeSpaceChars
                    ? new char[] { '[', ']', '(', ')', '?', '*', '+', '|', ' ' }
                    : new char[] { '[', ']', '(', ')', '?', '*', '+', '|' };

                char[] tokensToIgnore = options.EscapeSpaceChars
                    ? new char[] { '\t', '\r', '\n' }
                    : new char[] { '\t', '\r', '\n', ' ' };

                // Process each token.
                for (int i = 0; i < matches.Count; i++)
                {
                    SearchToken token = new SearchToken();

                    token.value = matches[i].Groups["search_token"].Value;

                    // Check to see if token should be ignored
                    if (token.value.Length == 1 && token.value.IndexOfAny(tokensToIgnore) == 0)
                    {
                        continue;
                    }

                    // Check to see if the token should be repeated more than once.
                    Group repetitionGroup = matches[i].Groups["repetitions"];
                    uint repetitions = 1;
                    if (!string.IsNullOrEmpty(repetitionGroup.Value) &&
                        !uint.TryParse(repetitionGroup.Value, NumberStyles.Integer,
                            CultureInfo.InvariantCulture, out repetitions))
                    {
                        ExtractException ee = new ExtractException("ELI28346",
                            "Unable to parse number of token repetitions!");
                        ee.AddDebugData("Specified repetitions", repetitionGroup.Value, false);
                    }

                    // Check to see if a qualifier was specified
                    Group qualifierGroup = matches[i].Groups["qualifier"];
                    if (qualifierGroup.Success)
                    {
                        if (qualifierGroup.Value == "?")
                        {
                            token.optional = true;
                        }
                        else
                        {
                            token.required = true;
                        }
                    }

                    // Perform any character replacements
                    if (options.CharacterReplacements.Count > 0)
                    {
                        token.value = PerformCharacterReplacements(token.value, options.CharacterReplacements);
                    }

                    // Escape the token if necessary.
                    if (token.value.Length == 1 && token.value.IndexOfAny(tokensToEscape) == 0)
                    {
                        token.value = "\\" + token.value;
                    }

                    // Add the token to the search token list the specified number of times.
                    for (uint j = 0; j < repetitions; j++)
                    {
                        searchTokens.Add(token);
                    }
                }

                ExtractException.Assert("ELI28348",
                    "Fuzzy search string valid only for two or more characters!",
                    searchTokens.Count >= 2);

                return searchTokens;
            }
            catch (Exception ex)
            {
                ExtractException ee = 
                    new ExtractException("ELI28337", "Error parsing fuzzy text search string!", ex);
                ee.AddDebugData("Search string", searchString, true);
                throw ee;
            }
        }

        /// <summary>
        /// Performs any replacements specified, either against the search string component or, if
        /// the token is a parenthesized group, against its sub-tokens.
        /// </summary>
        /// <param name="searchStringToken">The string to perform the replacements against.</param>
        /// <param name="replacements">The list of replace regex and replacement pairs.</param>
        /// <returns>The search string token after replacements have been performed.</returns>
        static string PerformCharacterReplacements(string searchStringToken, List<KeyValuePair<string, string>> replacements)
        {
            StringBuilder tokenBuilder = new StringBuilder();

            // If the search_string part of the token is a parenthesized group, perform
            // replacements against its sub-tokens
            if (searchStringToken.StartsWith("(", StringComparison.Ordinal))
            {
                // Skip if anything more than a simple parenthesized group
                if (searchStringToken.StartsWith("(?", StringComparison.OrdinalIgnoreCase))
                {
                    return searchStringToken;
                }

                tokenBuilder.Append("(");
                searchStringToken = searchStringToken.Substring(1, searchStringToken.Length - 2);

                foreach (Match subTokenMatch in _subTokenRegex.Value.Matches(searchStringToken))
                {
                    string subToken = subTokenMatch.Groups["search_token"].Value;
                    string quantifier = subTokenMatch.Groups["quantifier"].Value;
                    foreach (var replacementPair in replacements)
                    {
                        subToken = Regex.Replace
                            (subToken, replacementPair.Key, replacementPair.Value, _REGEX_OPTIONS);
                    }

                    tokenBuilder.Append(subToken);
                    tokenBuilder.Append(quantifier);
                }
                tokenBuilder.Append(")");

                searchStringToken = tokenBuilder.ToString();
            }
            // Else perform the replace against the whole token
            else
            {
                foreach (var replacementPair in replacements)
                {
                    searchStringToken = Regex.Replace
                        (searchStringToken, replacementPair.Key, replacementPair.Value, _REGEX_OPTIONS);
                }
            }

            return searchStringToken;
        }

        /// <summary>
        /// Retrieves the fuzzy search options from the specified fuzzy search syntax match.
        /// </summary>
        /// <param name="match">A match containing fuzzy search syntax (obtained from
        /// _fuzzySearchExpressionParserRegex).</param>
        /// <returns>The <see cref="FuzzySearchOptions"/> to use when performing the fuzzy text
        /// search.</returns>
        static FuzzySearchOptions GetFuzzySearchOptions(Match match)
        {
            try
            {
                // Create a new options instance.
                FuzzySearchOptions options = new FuzzySearchOptions();

                // Retrieve the named group that contains the specified option names.
                CaptureCollection optionNames = match.Groups["option_name"].Captures;
                CaptureCollection optionValues = match.Groups["option_value"].Captures;

                for (int i = 0; i < optionNames.Count; i++)
                {
                    // Obtain both the name of the option and the corresponding value.
                    string optionName = optionNames[i].Value.Trim();
                    string optionValue = optionValues[i].Value.Trim();

                    // The search method is being specified.
                    if (optionName.Equals("method", StringComparison.OrdinalIgnoreCase))
                    {
                        if (optionValue.Equals("fast", StringComparison.OrdinalIgnoreCase))
                        {
                            options.SearchMethod = FuzzySearchOptions.Method.Fast;
                        }
                        else if (optionValue.Equals("better_fit",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            options.SearchMethod = FuzzySearchOptions.Method.BetterFit;
                        }
                        else
                        {
                            ExtractException ee = new ExtractException("ELI28339",
                                "Invalid fuzzy search method");
                            ee.AddDebugData("Method", optionValue, true);
                            throw ee;
                        }
                    }
                    // The number of errors allowed is being specified.
                    else if (optionName.Equals("error", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!UInt32.TryParse(optionValue, NumberStyles.Integer,
                                CultureInfo.InvariantCulture, out options.ErrorsAllowed))
                        {
                            ExtractException ee = new ExtractException("ELI28344",
                                "Unable to parse number of fuzzy search errors allowed!");
                            ee.AddDebugData("error value", optionValue, false);
                        }
                    }
                    // The number of extra whitespace chars allowed is being specified.
                    else if (optionName.Equals("xtra_ws", StringComparison.OrdinalIgnoreCase) ||
                             optionName.Equals("extra_ws", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!UInt32.TryParse(optionValue, NumberStyles.Integer,
                                CultureInfo.InvariantCulture, out options.ExtraWhitespaceAllowed))
                        {
                            ExtractException ee = new ExtractException("ELI28345",
                                "Unable to parse number of extra whitespace chars allowed!");
                            ee.AddDebugData("xtra_ws value", optionValue, false);
                        }
                    }
                    // The substitute pattern is being specified.
                    else if (optionName.Equals("sub", StringComparison.OrdinalIgnoreCase) ||
                        optionName.Equals("substitute_pattern", StringComparison.OrdinalIgnoreCase))
                    {
                        if (optionValue == "." || optionValue.Equals(@"[\s\s]", StringComparison.OrdinalIgnoreCase))
                        {
                            options.SubstitutePattern = optionValue;
                        }
                        // Surround with parentheses to guard against patterns with '|' in them
                        else
                        {
                            options.SubstitutePattern = "(" + optionValue + ")";
                        }
                    }
                    // The whitespace pattern is being specified.
                    else if (optionName.Equals("ws", StringComparison.OrdinalIgnoreCase) ||
                        optionName.Equals("ws_pattern", StringComparison.OrdinalIgnoreCase))
                    {
                        options.WhitespacePattern = "("+optionValue+")";
                    }
                    // Whether to omit numbers from group names is being specified.
                    else if (optionName.Equals("g", StringComparison.OrdinalIgnoreCase) ||
                        optionName.Equals("global", StringComparison.OrdinalIgnoreCase) ||
                        optionName.Equals("use_global_names", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!Boolean.TryParse(optionValue, out options.UseGlobalNames))
                        {
                            ExtractException ee = new ExtractException("ELI38849",
                                "Unable to parse use simple names option!");
                            ee.AddDebugData("use_global_names value", optionValue, false);
                        }
                    }
                    // Whether to escape space characters is being specified.
                    else if (optionName.Equals("esc_s", StringComparison.OrdinalIgnoreCase) ||
                        optionName.Equals("escape_space_chars", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!Boolean.TryParse(optionValue, out options.EscapeSpaceChars))
                        {
                            ExtractException ee = new ExtractException("ELI38850",
                                "Unable to parse escape space chars option!");
                            ee.AddDebugData("escape_space_chars value", optionValue, false);
                        }
                    }
                    // Replacement patterns are being specified.
                    else if (optionName.Equals("replacements", StringComparison.OrdinalIgnoreCase))
                    {
                        Match replacements = _replacementsRegex.Value.Match(optionValue);
                        if (!replacements.Success)
                        {
                            ExtractException ee = new ExtractException("ELI38853",
                                "Unable to parse replacements option!");
                            ee.AddDebugData("replacements value", optionValue, false);
                        }

                        CaptureCollection replaceValues = replacements.Groups["replace"].Captures;
                        CaptureCollection replacementValues = replacements.Groups["replacement"].Captures;
                        for (int j = 0; j < replaceValues.Count; j++)
                        {
                            options.CharacterReplacements.Add(new KeyValuePair<string,string>
                                ("^"+replaceValues[j].Value+"$", replacementValues[j].Value));
                        }
                    }
                    // An invalid option name was specified.
                    else
                    {
                        ExtractException ee = new ExtractException("ELI28340",
                                "Invalid fuzzy search option.");
                        ee.AddDebugData("Option Name", optionName, false);
                        ee.AddDebugData("Option Value", optionValue, false);
                        throw ee;
                    }
                }

                return options;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28341", ex);
                ee.AddDebugData("Fuzzy Search", match.Value, true);
                throw ee;
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Throws exception if the object is not licensed.
        /// </summary>
        private static void ValidateLicense()
        {
            // Validate the license
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI38854", _OBJECT_NAME);
        }

        #endregion Private Methods

    }
}
