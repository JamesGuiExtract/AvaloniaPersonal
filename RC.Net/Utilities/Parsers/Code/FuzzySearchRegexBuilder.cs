using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Extract.Utilities.Parsers
{
    /// <summary>
    /// A utility class to used to expand Extract Systems fuzzy regex search syntax into an
    /// equivalent regular expression.
    /// </summary>
    static class FuzzySearchRegexBuilder
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
            /// names from stack names used for other fuzzy searches in the same regex.</param>
            public BalancingRegexStackNames(int id)
            {
                // Initialize the unique string to be added to each stack name.
                string termIdentifier = id.ToString(CultureInfo.InvariantCulture);

                // Initialze the stack names using termIdentifier.
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
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
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
                    return _useInitialStackSet ? _initialErrorStack : _finalErrorStack;
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
                    return _useInitialStackSet ? _initialExtraSpaceStack : _finalExtraSpaceStack;
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
                    return _useInitialStackSet ? _initialMissedStack : _finalMissedStack;
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

        #endregion Private Classes

        #region Fields

        /// <summary>
        /// A regular expression used to identify regex search expression(s) within a given regex
        /// string.
        /// </summary>
        static Regex _fuzzySearchExpressionParserRegex = new Regex(
            @"[(][?][~][<]" +
            @"(?'options'[^>]*?)" +
            @"[>]" +
            @"(?'search_string'" +
                @"(" +
                    // Treat all chars between unescaped open and closed square brackets
                    // as part of a character class (disregard an enclosed closing paren)
                    @"(?<=(^|[^\\])([\\]{2})*)\[" +
                        @".+?" +
                        @"(?<=[^\\]([\\]{2})*)\]" +
                    // Treat any escaped closing paren as part of the search string.
                    @"|(?<=[^\\]([\\]{2})*[\\])[)]" +
                    // Treat any other non-paren as part of the search string.
                    @"|[^)]" +
                @")*?" +
            @")" +
            @"(?<=[^\\]([\\]{2})*)[)]");

        /// <summary>
        /// A regular expression used to parse search tokens with a fuzzy search string.
        /// </summary>
        static Regex _searchStringTokenizerRegex = new Regex(
            // Search for token used to match next char
            @"(?'search_token'" +
            // Search for a character class enclosed in square brackets.
                @"(?<=(^|[^\\])([\\]{2})*)\[" +
                @"((?<![^\\]([\\]{2})*)\]|[^\]])*" +
                @"(?<=[^\\]([\\]{2})*)\]" +
            // Search for .NET regex defined escape chars
                @"|\\\d{1,3}" +
                @"|\\c\S" +
                @"|\\x[0-9a-eA-E]{2}" +
                @"|\\u[0-9a-eA-E]{4}" +
                @"|\\[\s\S]" +
                @"|[\s\S]" +
            @")" +
            // Look for specification of many times to repeat token.
            @"(" +
                @"(?<=[^\\]([\\]{2})*)" +
                @"\{(?'repetitions'\d+?)\}" +
            @")?");

        #endregion Fields

        #region Methods

        /// <summary>
        /// Expands all fuzzy search strings withing the specified regex to produce a resulting
        /// regular expression that will perform the search specified.
        /// <para><b>Syntax:</b></para>
        /// SYNTAX: (given as a .NET regex):
        /// [(] [?] [~] [&lt;]
        /// (?'options'[^>]+?)
        /// [>]
        /// (?'search_string'
		/// (
        /// 	(?&lt;=(^|[^\\])([\\]{2})*)\[.+?
		/// 		(?&lt;=[^\\]([\\]{2})*)\]
        /// 	|(?&lt;=[^\\]([\\]{2})*[\\])[)]
		/// 	|[^)]
		/// )*?
	    /// )
        /// (?&lt;=[^\\]([\\]{2})*)[)]
        /// where 'options' is a comma separated list of the following possible options:
        /// method=fast|better_fit (default=fast): 
        ///     fast will execute faster but match characters outside of the specified search string
        ///     depending upon how many of the errors allowed were found in a match.
        ///     better_fit will match only the specified search string but will take longer to
        ///     execute.
        /// error=[number] (default = 1):
        ///     The number of errors (missing or wrong characters) that are allowed in a potential
        ///     match before the match is discarded.
        /// xtra_ws=[number] (default = 0):
        ///     The number of extra whitespace chars that are allowed outside of the number of
        ///     errors that are allowed before disqualifying a potential match.
        /// 'search_string' is the string to search for. The string will be treated a literal text
        /// except in that:
        /// 1) Regex escape sequences (ie. \d, \x20, \040, \cC, \u0020), and character
        /// classes whether specified with a backslash or that are enclosed in unescaped square 
        /// brackets (ie. [\da-eA-E]) will be taken together as a token representing a single char
        /// in the text to be searched.
        /// 2) A number enclosed in curly braces except at the beginning of the search string will
        /// repeat the previous token the specified number of times. (ie: \d{9} or [\da-eA-E]{3}).
        /// </summary>
        /// <param name="sourceRegex">The regular expression in which fuzzy search syntax should be
        /// expanded out into the equivalent regex.</param>
        /// <returns>A regular expression in which fuzzy search syntax is in regex form ready to be
        /// parsed by the .NET regex engine.</returns>
        public static string ExpandFuzzySearchExpressions(string sourceRegex)
        {
            try
            {
                StringBuilder expandedSearchString = new StringBuilder();
                int expansionPos = 0;

                // Process each string within sourceRegex that matches the fuzzy search pattern.
                MatchCollection matches = _fuzzySearchExpressionParserRegex.Matches(sourceRegex);
                foreach (Match match in matches)
                {
                    // Create a new set of stack names that will be unique to this particular
                    // fuzzy search (so that the stack names are not repeated within the same
                    // sourceRegex
                    BalancingRegexStackNames stackNames =
                        new BalancingRegexStackNames(match.Index);

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
                    List<string> searchTokens = GetSearchTokens(match);

                    ExtractException.Assert("ELI28335", "Number of fuzzy search errors allowed " +
                        "cannot be more than the length of the string to search for.",
                        options.ErrorsAllowed <= searchTokens.Count);

                    // Create expression prefix to initialize error and extra space stack counts.
                    expandedSearchString.Append(@"(?nx:");
                    expandedSearchString.Append(
                        IncrementStack(stackNames.ErrorStack, (int)options.ErrorsAllowed));
                    expandedSearchString.Append(IncrementStack(stackNames.ExtraSpaceStack,
                            (int)options.ExtraWhitespaceAllowed));
                    expandedSearchString.AppendLine();

                    // For better fit method, create named group to store the matched string.
                    if (options.SearchMethod == FuzzySearchOptions.Method.BetterFit)
                    {
                        expandedSearchString.Append(@"(?'");
                        expandedSearchString.Append(stackNames.ActualMatchedString);
                        expandedSearchString.Append(@"'");
                        expandedSearchString.AppendLine();
                    }

                    // Leading whitespace will be allowed for the first token of the fast method.
                    bool allowLeadingSpace =
                        options.SearchMethod != FuzzySearchOptions.Method.BetterFit;

                    // Create a term that searches for each token in the search string and adjusts
                    // stack counts accordingly for each miss.
                    for (int i = 0; i < searchTokens.Count; i++)
                    {
                        string searchToken = searchTokens[i];
                        bool zeroWidthAssertion = isZeroWidthAssertion(searchToken);

                        AddTokenSearchTerm(expandedSearchString, searchToken, stackNames,
                            options.SearchMethod == FuzzySearchOptions.Method.BetterFit,
                            allowLeadingSpace);

                        // Allow leading whitespace starting with the first token following a
                        // non zero width assertion token.
                        allowLeadingSpace |= !zeroWidthAssertion;
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
                        expandedSearchString.Append(
                            IncrementStack(stackNames.FinalErrorStack, -1));
                        expandedSearchString.Append(@"(");
                        expandedSearchString.Append(
                            IncrementStack(stackNames.AllowableLookAheadStack, -1));
                        expandedSearchString.AppendLine(@".\s*)+?");

                        // Switch to use the final stack name set for the following group of token
                        // search terms.
                        stackNames.UseInitialStackSet = false;

                        // Leading whitespace not allowed for the first token of the better fit method.
                        allowLeadingSpace = false;

                        // Generate the second set of search token terms.
                        for (int i = 0; i < searchTokens.Count; i++)
                        {
                            string searchToken = searchTokens[i];
                            bool zeroWidthAssertion = isZeroWidthAssertion(searchToken);

                            AddTokenSearchTerm(expandedSearchString, searchToken, stackNames,
                                false, allowLeadingSpace);

                            // Allow leading whitespace starting with the first token following a
                            // non zero width assertion token.
                            allowLeadingSpace |= !zeroWidthAssertion;
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

                // If there is any remaining part of sourceRegex that has not been parsed as part of
                // the fuzzy search expansion, append it to the end of the expanded term.
                if (expansionPos < sourceRegex.Length)
                {
                    expandedSearchString.Append(
                        sourceRegex.Substring(expansionPos, sourceRegex.Length - expansionPos));
                }

                return expandedSearchString.ToString();
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
        static void AddTokenSearchTerm(StringBuilder regEx, string searchToken,
            BalancingRegexStackNames stackNames, bool initialBetterFitTerm, bool allowLeadingSpace)
        {
            // Open a group by attempting to match the search token.
            regEx.Append(@"  (");

            // Add a term to reflect a char that does not correspond to the search token.
            AddIgnoreTokenTerm(regEx, stackNames, initialBetterFitTerm);

            // Allow for space chars before matching the search token if specified.
            if (allowLeadingSpace)
            {
                AddExtraSpaceTerm(regEx, stackNames, initialBetterFitTerm);
            }

            AddMatchTokenTerm(regEx, stackNames, searchToken);

            regEx.Append(@"|");

            // Add a token for allow for a missing token.
            AddMissingTokenTerm(regEx, stackNames, initialBetterFitTerm);

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
        static void AddIgnoreTokenTerm(StringBuilder term, BalancingRegexStackNames stackNames,
            bool initialBetterFitTerm)
        {
            // Decrement the error stack as well the last miss that had been added to the missed
            // chars stack.
            term.Append(@"(?>.(");
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
        static void AddExtraSpaceTerm(StringBuilder term, BalancingRegexStackNames stackNames,
            bool initialBetterFitTerm)
        {
            // Decrement the extra space stack
            term.Append(@"(");
            term.Append(IncrementStack(stackNames.ExtraSpaceStack, -1));

            // Increment the number of extra spaces the better fit mode will try to remove from the
            // initial match
            if (initialBetterFitTerm)
            {
                term.Append(IncrementStack(stackNames.FinalExtraSpaceStack, 1));
            }

            term.Append(@"\s)*");
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
        static List<string> GetSearchTokens(Match match)
        {
            string searchString = "[not found]";

            try
            {
                // Retrieve the named group that contains the search string to be parsed.
                Group searchStringGroup = match.Groups["search_string"];
                searchString = searchStringGroup.Value;

                // Use SearchStringTokenizerRegex to split the search string into tokens.
                MatchCollection matches = _searchStringTokenizerRegex.Matches(searchString);
                List<string> searchTokens = new List<string>(matches.Count);

                // Process each token.
                for (int i = 0; i < matches.Count; i++)
                {
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

                    // Add the token to the search token list the specified number of times.
                    for (uint j = 0; j < repetitions; j++)
                    {
                        string token = matches[i].Groups["search_token"].Value;

                        // Generate a list of tokens that need to be escaped to avoid corrupting 
                        // the resulting regex.
                        char[] tokensToEscape =
                            new char[] { '[', ']', '(', ')', '?', '*', '+', '|' };

                        // Escape the token if necessary.
                        if (token.Length == 1 && token.IndexOfAny(tokensToEscape) == 0)
                        {
                            searchTokens.Add("\\" + token);
                        }
                        else
                        {
                            searchTokens.Add(token);
                        }
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

                // Retrieve the named group that contains the specified options.
                Group optionsGroup = match.Groups["options"];

                string optionsString = optionsGroup.Value;
                if (!string.IsNullOrEmpty(optionsString))
                {
                    // Iterate through each specified option.
                    string[] optionsArray = optionsString.Split(',');
                    foreach (string optionString in optionsArray)
                    {
                        int equalPos = optionString.IndexOf('=');
                        ExtractException.Assert("ELI28338", "Malformed fuzzy search string (option '" +
                            optionsString + "'.", equalPos >= 0 &&
                            equalPos < optionsString.Length - 1);

                        // Obtain both the name of the option and the corresponding value.
                        string optionName = optionString.Substring(0, equalPos).Trim();
                        string optionValue = optionString.Substring(equalPos + 1).Trim();

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
                        // An invalid option name was specified.
                        else
                        {
                            ExtractException ee = new ExtractException("ELI28340",
                                    "Invalid fuzzy search option.");
                            ee.AddDebugData("Option Name", optionName, true);
                            ee.AddDebugData("Option Value", optionValue, true);
                            throw ee;
                        }
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

        #endregion Methods
    }
}