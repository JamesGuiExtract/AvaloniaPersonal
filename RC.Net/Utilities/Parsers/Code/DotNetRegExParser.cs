using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using Parser = Extract.Utilities.Union<System.Text.RegularExpressions.Regex, Extract.Utilities.Parsers.FindIfXOf>;

namespace Extract.Utilities.Parsers
{
    /// <summary>
    /// The type of underlying parser, currently could be a plain old regex or a compound parser
    /// consisting of more than one <see cref="DotNetRegexParser"/>
    /// </summary>
    enum ParserType
    {
        Regex = 0,
        FindIfXOf = 1
    }

    /// <summary>
    /// Container to hold some reusable regex parsing regex pattern constants
    /// </summary>
    internal static class RegexParsingPatterns
    {
        /// <summary>
        /// A minimal sequence of characters and/or escape sequences that matches round and square
        /// bracket pairs, following the special character rules that the .NET regex parser uses.
        /// Capped at 1000 so that a malformed fuzzy regex syntax in a very large regex pattern will
        /// not cause parsing process to hang.
        /// </summary>
        public const string VALID_SEQUENCE = @"(" + _ATOM + @"){1,1000}?" + _NO_OPEN_BRACKETS;

        // Matches a single character or escape sequence and tracks open parentheses and square brackets.
        const string _ATOM =
            @"(?>
                  \\( \d{1,3} | c\S | x[0-9a-eA-E]{2} | u[0-9a-eA-E]{4} | [\s\S] )
                | \[\^] (?'Square') # in this position a closing bracket is effectively escaped
                | \[ (?'Square')
                | \] (?(Square)(?'-Square')) # Closing square is only special if there is an open square
                | \( (?'Round')
                | \) (?'-Round')
                | [^])]
              )";

        // Fails the match if there is a bracket unclosed
        const string _NO_OPEN_BRACKETS = @"(?(Square)(?!)|(?(Round)(?!)))";
    }

    /// <summary>
    /// Class that implements the COM interfaces to encapsulate the .NET regular expression engine
    /// for use with Flex/IDShield.
    /// </summary>
    // Needs ComVisible = <see langword="true"/> because this implements com interface
    [ComVisible(true)]
    [Guid("6136D2EC-E3DB-4a13-9CDD-89C1C06E9CD5"), ProgIdAttribute("ESRegExParser.DotNetRegExParser.1")]
    public class DotNetRegexParser : IRegularExprParser, ILicensedComponent
    {
        #region Fields

        /// <summary>
        /// The RegEx object to use.
        /// </summary>
        Parser _regexParser;

        /// <summary>
        /// Pattern to be searched for.
        /// </summary>
        string _pattern = "";

        /// <summary>
        /// The <see cref="RegexOptions"/> that should be used for the underlying <see cref="Regex"/>.
        /// </summary>
        RegexOptions _regexOptions = RegexOptions.Multiline | RegexOptions.IgnoreCase;

        /// <summary>
        /// An expression formatter that provides custom formatting of the regex expression.
        /// </summary>
        IExpressionFormatter _expressionFormatter = null;

        /// <summary>
        /// Determines, for method calls that return tokens based on named groups,
        /// whether all captures of named groups are returned (if <see langword="true"/>)
        /// or only the last capture of the group are returned (if <see langword="false"/>).
        /// </summary>
        bool _ReturnAllGroupCaptures = false;

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DotNetRegexParser).ToString();

        /// <summary>
        /// The timeout value to be used for this regex. This can be overridden by specifying a new
        /// value at the very beginning of a regex pattern enclosed in an otherwise illegal regex
        /// construct (??) using supported TimeSpan.Parse format: "[ws][-]{ d | [d.]hh:mm[:ss[.ff]] }[ws]".
        /// E.g., (?? timeout = 00:00:10 ) would make the timeout 10 seconds.
        /// </summary>
        TimeSpan _timeout = TimeSpan.FromMinutes(10);

        /// <summary>
        /// The parser type
        /// </summary>
        ParserType _parserType = ParserType.Regex;

        /// <summary>
        /// The find-if-x-of-requirement, used for <see cref="ParserType.FindIfXOf"/>
        /// </summary>
        uint _findIfXOfRequirement;

        /// <summary>
        /// Whether to output groups named with leading underscores
        /// </summary>
        bool _outputUnderscoreGroups;

        #endregion

        #region IRegularExprParser Properties

        /// <summary>
        /// Property to indicate if the Regex parser should ignore case when looking for matches.
        /// </summary>
        public bool IgnoreCase
        {
            get
            {
                return _regexOptions.HasFlag(RegexOptions.IgnoreCase);
            }
            set
            {
                try
                {
                    // Only change the value if it is different from the previous value.
                    if (IgnoreCase != value)
                    {
                        if (value)
                        {
                            _regexOptions |= RegexOptions.IgnoreCase;
                        }
                        else
                        {
                            _regexOptions &= ~RegexOptions.IgnoreCase;
                        }

                        // Reset the RegEx parser to null since options can only be set when
                        // creating a RegEx object.
                        _regexParser = null;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35837");
                }
            }
        }

        /// <summary>
        /// Property for setting the pattern to be matched.
        /// </summary>
        // The set method here just sets a string value and clears the parser,
        // this should never throw an exception.
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public string Pattern
        {
            get
            {
                return _pattern;
            }
            set
            {
                // Only change the value if it is different from the previous value.
                if (!_pattern.Equals(value, StringComparison.Ordinal))
                {
                    _pattern = value;

                    // Reset the RegEx parser to null since options can only be set when
                    // creating a RegEx object.
                    _regexParser = null;

                    // Reset the parser type since this is set when parsing the pattern
                    // and only non Regex types are specified
                    _parserType = ParserType.Regex;
                }
            }
        }

        /// <summary>
        /// Gets or sets an expression formatter that provides custom formatting of the regex.
        /// </summary>
        /// <value>
        /// The <see cref="IExpressionFormatter"/> to provide custom formatting.
        /// </value>
        public IExpressionFormatter ExpressionFormatter
        {
            get
            {
                return _expressionFormatter;
            }

            set
            {
                try
                {
                    if (value != _expressionFormatter)
                    {
                        _expressionFormatter = value;

                        // Report memory usage of the expression formatter to compel .Net garbage
                        // collection to release the expression formatter when no longer used. Memory
                        // can be an issue with the expression formatter when an AFExpressionFormatter
                        // has an AFDocument with a large attribute hierarchy attached.
                        if (_expressionFormatter != null)
                        {
                            MemoryManager.ReportComObjectMemoryUsage(_expressionFormatter);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI36265", "Unable to set ExpressionFormatter.");
                }
            }
        }

        /// <summary>
        /// Property that determines, for method calls that return tokens based on named groups,
        /// whether all captures of named groups are returned (if <see langword="true"/>)
        /// or only the last capture of the group are returned (if <see langword="false"/>).
        /// </summary>
        public bool ReturnAllGroupCaptures
        {
            get
            {
                return _ReturnAllGroupCaptures;
            }

            set
            {
                _ReturnAllGroupCaptures = value;
            }
        }

        #endregion IRegularExprParser Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the <see cref="RegexOptions"/> that should be used for the underlying
        /// <see cref="Regex"/>.
        /// </summary>
        /// <value>
        /// The <see cref="RegexOptions"/> that should be used for the underlying
        /// <see cref="Regex"/>.
        /// </value>
        public RegexOptions RegexOptions
        {
            get
            {
                return _regexOptions;
            }

            set
            {
                _regexOptions = value;
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="T:Regex"/> used to perform searches.
        /// </summary>
        public Regex Regex
        {
            get
            {
                Regex parser = GetRegexParser().Match(regex => regex, _ => null);
                ExtractException.Assert("ELI41747", "This instance cannot be represented by a Regex object",
                    parser != null);
                return parser;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Validates the currently assigned <see cref="Pattern"/>.
        /// </summary>
        /// <throws><see cref="ExtractException"/> if the specified regular expression pattern is
        /// not valid.</throws>
        public void ValidatePattern()
        {
            try
            {
                GetRegexParser();
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI32733", "Invalid regular expression pattern", ex);
            }
        }

        #endregion Public Methods

        #region IRegularExprParser Methods

        /// <summary>
        /// Finds the <see cref="Pattern"/> in the input string.
        /// </summary>
        /// <param name="strInput">The string to search.</param>
        /// <param name="bFindFirstMatchOnly">Determines if only the first match should be returned
        /// or all matches.</param>
        /// <param name="bReturnNamedMatches">If <see langword="true"/> returns named submatches 
        /// otherwise returns no sub matches</param>
        /// <returns>IUnknownVector that contains ObjectPair objects that have  
        ///     Object1 = a Token object representing a match
        ///     Object2 = null if no captured groups and a IUnknownVector of Token objects
        ///                 with the Name set to the group name. </returns>
        public IUnknownVector Find(string strInput, bool bFindFirstMatchOnly, bool bReturnNamedMatches, bool bDoNotStopAtEmptyMatch)
        {
            try
            {
                // Make sure the Parser is licensed
                ValidateLicense();

                // Get the parser
                return GetRegexParser().Match(
                    plainOldParser =>
                    {
                        // Return the results
                        return Find(plainOldParser, strInput, bFindFirstMatchOnly, bReturnNamedMatches, bDoNotStopAtEmptyMatch);
                    },
                    findIfXOf =>
                    {
                        // If the find-x-of requirement is met, match the main regex.
                        IUnknownVector namedGroups;
                        if (StringContainsXOfPatterns(_findIfXOfRequirement, findIfXOf.TestParsers,
                            strInput, bReturnNamedMatches, out namedGroups))
                        {
                            var result = findIfXOf.FindingParser.Find(strInput, bFindFirstMatchOnly, bReturnNamedMatches, bDoNotStopAtEmptyMatch);

                            // If returning named group matches, also collect the named groups of the test patterns
                            // and attach them to the primary results
                            if (namedGroups != null && namedGroups.Size() > 0)
                            {
                                foreach (var pair in result.ToIEnumerable<ObjectPair>())
                                {
                                    if (pair.Object2 == null)
                                    {
                                        pair.Object2 = namedGroups;
                                    }
                                    else
                                    {
                                        ((IUnknownVector)pair.Object2).InsertVector(0, namedGroups);
                                    }
                                }
                            }
                            return result;
                        }
                        else
                        {
                            return new IUnknownVector();
                        }
                    });
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI22259", "Could not process find.", ex);
            }
        }

        /// <summary>
        /// Search the input text and retrieves tokens representing the named groups from a
        /// successful search. The primary search result is ignored.
        /// </summary>
        /// <param name="strInput">The string to search.</param>
        /// <param name="bFindFirstMatchOnly"><see langword="true"/> to return the named groups for
        /// the first match only; <see langword="true"/> to return the named groups for all matches.
        /// </param>
        /// <returns>An <see cref="IUnknownVector"/> of <see cref="Token"/>s representing the named
        /// groups from a successful search. Null will be returned if the overall regex
        /// did not match. An empty vector will be returned if there was a match but there were no
        /// non-empty named groups.</returns>
        public IUnknownVector FindNamedGroups(string strInput, bool bFindFirstMatchOnly)
        {
            try
            {
                // Make sure the Parser is licensed
                ValidateLicense();

                // Get the parser
                return GetRegexParser().Match(
                    plainOldParser =>
                    {
                        return FindNamedGroups(plainOldParser, strInput, bFindFirstMatchOnly);
                    },
                    findIfXOf =>
                    {
                        if (StringContainsXOfPatterns(_findIfXOfRequirement, findIfXOf.TestParsers, strInput))
                        {
                            return findIfXOf.FindingParser.FindNamedGroups(strInput, bFindFirstMatchOnly);
                        }
                        else
                        {
                            return null;
                        }
                    });
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI33368", "Failed to find named groups.", ex);
            }
        }

        /// <summary>
        /// Replaces the <see cref="Pattern"/> found in the input string.
        /// </summary>
        /// <param name="strInputText">The string to search.</param>
        /// <param name="strReplaceWith">The string to replace the found Pattern.</param>
        /// <param name="bReplaceFirstMatchOnly">If <see langword="true"/> replace only the first match.</param>
        /// <returns>The string with the patterns replaced.</returns>
        public string ReplaceMatches(string strInputText, string strReplaceWith, bool bReplaceFirstMatchOnly)
        {
            try
            {
                // Make sure the Parser is licensed
                ValidateLicense();

                // Get the parser.
                // Since this method is only used by the EntityNameDataScorer don't bother supporting
                // complicated features like FindIfXOf
                Regex parser = GetRegexParser().Match(regex => regex, _ => null);
                ExtractException.Assert("ELI41749", "This method does not support compound expressions", parser != null);

                // Check if only want to replace the first pattern.
                if (bReplaceFirstMatchOnly)
                {
                    // Set return value to the input string with only the first pattern replaced.
                    return parser.Replace(strInputText, strReplaceWith, 1);
                }

                // Return the string will all matches replaced
                return parser.Replace(strInputText, strReplaceWith);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI22260", "Could not process Replace.", ex);
            }
        }

        /// <summary>
        /// Checks for the pattern in the Input string.
        /// </summary>
        /// <param name="strInput">The string to search.</param>
        /// <returns><see langword="true"/> if the pattern is found in the input string
        ///          <see langword="false"/>if the pattern is not found in the input string</returns>
        public bool StringContainsPattern(string strInput)
        {
            try
            {
                // Make sure the Parser is licensed
                ValidateLicense();

                // Get the parser
                return GetRegexParser().Match(
                    plainOldParser =>
                    {
                        // Return results of IsMatch
                        return plainOldParser.IsMatch(strInput);
                    },
                    findIfXOf =>
                    {
                        // Return whether the find-x-of requirement is met and the string contains the main pattern
                        return StringContainsXOfPatterns(_findIfXOfRequirement, findIfXOf.TestParsers, strInput)
                            && findIfXOf.FindingParser.StringContainsPattern(strInput);
                    });
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI22261", "Could not process find pattern.", ex);
            }
        }

        /// <summary>
        /// Checks the input string for a list of patterns.
        /// </summary>
        /// <param name="strInput">The string to search for the patterns.</param>
        /// <param name="pvecExpressions">List of patterns to check for</param>
        /// <param name="bAndRelation">If <see langword="true"/> the all patterns must be found in the string
        /// for true to be returned and if <see langword="false"/> only one of the patterns needs to be found</param>
        /// <returns>true if bAndRelation is <see langword="true"/> and all patterns are found
        ///          true if bAndRelation is <see langword="false"/> and at least one pattern is found 
        ///          otherwise will return false</returns>
        public bool StringContainsPatterns(string strInput, VariantVector pvecExpressions, bool bAndRelation)
        {
            try
            {
                // Make sure the Parser is licensed
                ValidateLicense();

                // Get the number of pattern expressions
                int numberOfExpressions = pvecExpressions.Size;

                // Check the input string for each pattern.
                for (int i = 0; i < numberOfExpressions; i++)
                {
                    // Get the pattern
                    Pattern = (string)pvecExpressions[i];

                    // Check the input string for the pattern
                    // Since this method is only used by the EntityNameDataScorer don't bother supporting
                    // complicated features like FindIfXOf
                    Regex parser = GetRegexParser().Match(regex => regex, _ => null);
                    ExtractException.Assert("ELI41748", "This method does not support compound expressions", parser != null);

                    bool found = parser.IsMatch(strInput);

                    if (!found && bAndRelation)
                    {
                        // Pattern was not found and check for all to be found so return false
                        return false;
                    }
                    else if (found && !bAndRelation)
                    {
                        // Pattern was found and we only need one to be found so return true
                        return true;
                    }
                }

                // All patterns were checked 
                return bAndRelation;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI22445", "Could not process StringContainsPatterns.", ex);
            }
        }

        /// <summary>
        /// Checks input string for exact match of the pattern.
        /// </summary>
        /// <param name="strInput">The string to check for pattern.</param>
        /// <returns><see langword="true"/> if the input exactly matches pattern.
        /// <see langword="false"/> otherwise</returns>
        public bool StringMatchesPattern(string strInput)
        {
            try
            {
                // Make sure the Parser is licensed
                ValidateLicense();

                // Get parser
                return GetRegexParser().Match(
                    plainOldParser =>
                    {
                        return StringMatchesPattern(plainOldParser, strInput);
                    },
                    findIfXOf =>
                    {
                        // Return whether the find-x-of requirement is met and the main pattern matches
                        return StringContainsXOfPatterns(_findIfXOfRequirement, findIfXOf.TestParsers, strInput)
                            && findIfXOf.FindingParser.StringMatchesPattern(strInput);
                    });
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI22262", "Could not determine whether pattern matches.", ex);
            }
        }

        /// <summary>
        /// Determines the locations and resultant replacement text if matches were replaced with 
        /// the specified text.
        /// </summary>
        /// <param name="bstrInput">The <see cref="String"/> to search for replacements.</param>
        /// <param name="bstrReplacement">The string to use to replace matches in 
        /// <paramref name="bstrInput"/>. May contain capture groups (e.g. "$1 $2").</param>
        /// <param name="bFindFirstMatchOnly"><see langword="true"/> if only the first match should 
        /// be found; <see langword="false"/> if more than one match should be found.</param>
        /// <returns>An <see cref="IUnknownVector"/> of <see cref="Token"/>s, where the 
        /// positions are expressed prior to performing the replacement and the value of the token 
        /// represents the text after performing the replacement.</returns>
        public IUnknownVector FindReplacements(string bstrInput, string bstrReplacement,
            bool bFindFirstMatchOnly)
        {
            try
            {
                // Make sure the parser is licensed
                ValidateLicense();

                // Get the parser
                return GetRegexParser().Match(
                    plainOldParser =>
                    {
                        return FindReplacements(plainOldParser, bstrInput, bstrReplacement, bFindFirstMatchOnly);
                    },
                    findIfXOf =>
                    {
                        if (StringContainsXOfPatterns(_findIfXOfRequirement, findIfXOf.TestParsers, bstrInput))
                        {
                            return findIfXOf.FindingParser.FindReplacements(bstrInput, bstrReplacement, bFindFirstMatchOnly);
                        }
                        else
                        {
                            return new IUnknownVector();
                        }
                    });
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI24230", "Could not find replacements.", ex);
            }
        }

        #endregion

        #region ILicensedComponent Members

        /// <summary>
        /// Check if component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if licensed, <see langword="false"/> if not licensed</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(LicenseIdName.ExtractCoreObjects);
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Throws exception if the object is not licensed.
        /// </summary>
        private static void ValidateLicense()
        {
            // Validate the license
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI22446", _OBJECT_NAME);
        }

        /// <summary>
        /// Returns the <see cref="Parser"/> with the set pattern and options
        /// </summary>
        /// <returns>Regex parser with the pattern and options set.</returns>
        private Parser GetRegexParser()
        {
            // if internal variable is null, create a new parser with the pattern and options.
            if (_regexParser == null)
            {
                // Look for any proprietary options. This could change the _parserType
                ParseRegexOptions();

                switch (_parserType)
                {
                    // If this is a plain old regex, create the underlying parser
                    case ParserType.Regex:
                        // If an ExpressionFormatter has been specified, use it to do any necessary
                        // custom formatting of the pattern.
                        string expandedPattern = (ExpressionFormatter == null)
                            ? _pattern
                            : ExpressionFormatter.FormatExpression(_pattern);

                        // Expand any fuzzy search terms into the equivalent regular expression.
                        expandedPattern = FuzzySearchRegexBuilder.ExpandFuzzySearchExpressions(expandedPattern);

                        try
                        {
                            _regexParser = new Parser(new Regex(expandedPattern, RegexOptions, _timeout));
                        }
                        catch (Exception ex)
                        {

                            ExtractException ee = new ExtractException("ELI28437",
                                "Unable to initialize regular expression parser!");
                            ee.AddDebugData("Error", ex.Message, true);
                            ee.AddDebugData("Pattern", _pattern, true);
                            ee.AddDebugData("Expanded pattern", expandedPattern, true);
                            throw ee;
                        }
                        break;

                    // If this is a compound parser there will be recursive calls to this method that will
                    // result in actual parsers getting created eventually
                    case ParserType.FindIfXOf:
                        _regexParser = new Parser(new FindIfXOf(_pattern, this));
                        break;

                    default:
                        throw new ExtractException("ELI41751", UtilityMethods.FormatInvariant($"Unknown parser type: {_parserType}"));
                }
            }

            return _regexParser;
        }

        /// <summary>
        /// Parses proprietary global regex options and removes this part of the pattern.
        /// </summary>
        void ParseRegexOptions()
        {
            try
            {
                // Options are specified by (??...) as the very first thing after any leading whitespace
                int firstNonWhitespaceIndex = _pattern.TakeWhile(c => char.IsWhiteSpace(c)).Count();
                int possiblePrefixEndIndex = firstNonWhitespaceIndex + 3;
                if (_pattern.Length > possiblePrefixEndIndex
                    && _pattern.Substring(firstNonWhitespaceIndex, 3).Equals("(??", StringComparison.Ordinal))
                {
                    var optionsEnd = _pattern.IndexOf(')');
                    var options = _pattern.Substring(possiblePrefixEndIndex, optionsEnd - possiblePrefixEndIndex);
                    _pattern = _pattern.Length == optionsEnd + 1
                        ? ""
                        : _pattern.Substring(optionsEnd + 1);

                    foreach (var pair in options.Split(new[] { ',' }))
                    {
                        var nameValue = pair.Split(new[] { '=' });
                        if (nameValue.Length != 2)
                        {
                            var uex = new ExtractException("ELI41702", "Could not parse regex option");
                            uex.AddDebugData("Option string", pair, true);
                            throw uex;
                        }

                        var name = nameValue[0].Trim();
                        var uppercaseName = name.ToUpperInvariant();
                        var val = nameValue[1].Trim();
                        switch (uppercaseName)
                        {
                            case "TIMEOUT":
                                if (!TimeSpan.TryParse(val, out _timeout))
                                {
                                    var parseException = new ExtractException("ELI41703", "Could not parse timeout value");
                                    parseException.AddDebugData("Timeout", val, true);
                                    parseException.AddDebugData("Required format", "[ws][-]{ d | [d.]hh:mm[:ss[.ff]] }[ws]", true);
                                    throw parseException;
                                }
                                break;
                            case "FINDIFXOF":
                                if (!uint.TryParse(val, out _findIfXOfRequirement))
                                {
                                    var parseException = new ExtractException("ELI41703",
                                        "Could not parse FindIfXOf value (expected a positive integer)");
                                    parseException.AddDebugData("Value", val, true);
                                    throw parseException;
                                }
                                _parserType = ParserType.FindIfXOf;
                                break;
                            case "OUTPUTUNDERSCOREGROUPS":
                                if (!bool.TryParse(val, out _outputUnderscoreGroups))
                                {
                                    var parseException = new ExtractException("ELI41759",
                                        "Could not parse OutputUnderscoreGroups value (expected 'true' or 'false')");
                                    parseException.AddDebugData("Value", val, true);
                                    throw parseException;
                                }
                                break;
                            case "RIGHTTOLEFT":
                                bool rightToLeft;
                                if (!bool.TryParse(val, out rightToLeft))
                                {
                                    var parseException = new ExtractException("ELI41760",
                                        "Could not parse RightToLeft value (expected 'true' or 'false')");
                                    parseException.AddDebugData("Value", val, true);
                                    throw parseException;
                                }
                                if (rightToLeft)
                                {
                                    RegexOptions |= RegexOptions.RightToLeft;
                                }
                                else
                                {
                                    RegexOptions &= ~RegexOptions.RightToLeft;
                                }
                                break;
                            default:
                                var uex = new ExtractException("ELI41704", "Unrecognized regex option");
                                uex.AddDebugData("Option name", name, true);
                                throw uex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41705");
            }
        }

        /// <summary>
        /// Creates a shallow copy of this instance
        /// </summary>
        /// <returns>A new <see cref="DotNetRegexParser"/>  that is a shallow copy of this instance</returns>
        public DotNetRegexParser ShallowClone()
        {
            return (DotNetRegexParser)MemberwiseClone();
        }

        /// <summary>
        /// Tests the input with a collection of parsers to determine if a minimum requirement
        /// of matches exists.
        /// </summary>
        /// <remarks>Only one match per parser is counted against the <see paramref="findIfXOfRequirement"/></remarks>
        /// <param name="findIfXOfRequirement">The minimum match requirement.</param>
        /// <param name="parsers">The parsers.</param>
        /// <param name="input">The input.</param>
        /// <returns><c>true</c> if the minimum requirement is met, else <c>false</c></returns>
        static bool StringContainsXOfPatterns(uint findIfXOfRequirement, DotNetRegexParser[] parsers, string input)
        {
            IUnknownVector _ = null;
            return StringContainsXOfPatterns(findIfXOfRequirement, parsers, input, false, out _);
        }

        /// <summary>
        /// Tests the input with a collection of parsers to determine if a minimum requirement
        /// of matches exists.
        /// </summary>
        /// <remarks>Only one match per parser is counted against the <see paramref="findIfXOfRequirement"/></remarks>
        /// <param name="findIfXOfRequirement">The minimum match requirement.</param>
        /// <param name="parsers">The parsers.</param>
        /// <param name="input">The input.</param>
        /// <param name="returnNamedMatches">Whether to return data for named group matches</param>
        /// <param name="namedMatches">Vector that will be filled with any named group matches,
        /// if <see paramref="returnNamedMatches"/> is <c>true</c></param>
        /// <returns><c>true</c> if the minimum requirement is met, else <c>false</c></returns>
        static bool StringContainsXOfPatterns(uint findIfXOfRequirement, DotNetRegexParser[] parsers,
            string input, bool returnNamedMatches, out IUnknownVector namedMatches)
        {
            int testPatternMatches = 0;
            namedMatches = returnNamedMatches ? new IUnknownVector() : null;
            for (int i = 0; i < parsers.Length; i++)
            {
                // Stop searching as soon as there is no possibility that the requirement
                // can be met
                var remainingPatterns = parsers.Length - i;
                if (testPatternMatches + remainingPatterns < findIfXOfRequirement)
                {
                    return false;
                }
                var parser = parsers[i];
                try
                {
                    // FindNamedGroups will return a null vector if there was no match so this can
                    // be used to test the pattern and collect named group data at the same time
                    if (returnNamedMatches)
                    {
                        var patternResult = parser.FindNamedGroups(input, bFindFirstMatchOnly: true);
                        if (patternResult != null)
                        {
                            testPatternMatches++;
                            namedMatches.Append(patternResult);
                        }
                    }
                    else
                    {
                        if (parser.StringContainsPattern(input))
                        {
                            testPatternMatches++;
                        }
                    }

                    // Return as soon as the requirement is met
                    if (testPatternMatches == findIfXOfRequirement)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    var ue = ex.AsExtract("ELI41758");
                    ue.AddDebugData("Pattern", parser.Pattern, true);
                    throw ue;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the <see cref="Pattern"/> in the input string.
        /// </summary>
        /// <param name="parser">The <see cref="Regex"/> to be used for the search</param>
        /// <param name="input">The string to search.</param>
        /// <param name="findFirstMatchOnly">Determines if only the first match should be returned
        /// or all matches.</param>
        /// <param name="returnNamedMatches">If <see langword="true"/> returns named submatches 
        /// otherwise returns no sub matches</param>
        /// <param name="doNotStopAtZeroLengthMatch">Override legacy behavior that maybe made sense at some point...</param>
        /// <returns>IUnknownVector that contains ObjectPair objects that have  
        ///     Object1 = a Token object representing a match
        ///     Object2 = null if no captured groups and a IUnknownVector of Token objects
        ///                 with the Name set to the group name. </returns>
        IUnknownVector Find(Regex parser, string input, bool findFirstMatchOnly, bool returnNamedMatches, bool doNotStopAtZeroLengthMatch)
        {
            try
            {
                // Get the first match if there is one
                Match foundMatch = parser.Match(input);

                // Create the return object
                IUnknownVector returnVector = new IUnknownVector();

                // Process the matches while there is a match and that match is not empty (unless doNotStopAtZeroLengthMatch)
                while (foundMatch.Success && (foundMatch.Length != 0 || doNotStopAtZeroLengthMatch) && foundMatch.Index < input.Length)
                {
                    // Create the token object for the match
                    Token t = new Token();
                    t.InitToken(foundMatch.Index, foundMatch.Index + foundMatch.Length - 1, "",
                        foundMatch.Value);

                    // Get the sub matches 
                    IUnknownVector namedMatches = null;

                    if (returnNamedMatches)
                    {
                        namedMatches = new IUnknownVector();

                        // Process the groups, starting with the 2nd item
                        // in the group list because the first item is just the
                        // entire match
                        for (int i = 1; i < foundMatch.Groups.Count; i++)
                        {
                            // Get the group
                            Group g = foundMatch.Groups[i];

                            // Get the name of the group from the parser
                            string groupName = parser.GroupNameFromNumber(i);

                            // Don't create a capture token if name begins with a number or
                            // an '_' since these are understood as representing non-named groups
                            // Group name can be empty string so don't just index into it
                            // https://extract.atlassian.net/browse/ISSUE-13902
                            char firstChar = groupName.FirstOrDefault();
                            if (!_outputUnderscoreGroups && firstChar == '_'
                                || firstChar == default(char) || char.IsNumber(firstChar))
                            {
                                continue;
                            }

                            // If returning all captures and the group actually captured something
                            // iterate through its capture collection
                            if (ReturnAllGroupCaptures && g.Success)
                            {
                                foreach (Capture c in g.Captures)
                                {
                                    // Create a new token object
                                    Token captureToken = new Token();

                                    // Initialize with the values from the capture
                                    captureToken.InitToken(c.Index, c.Index + c.Length - 1, groupName, c.Value);

                                    // Add to the named matches collection.
                                    namedMatches.PushBack(captureToken);
                                }
                            }
                            // Otherwise use the values of the group
                            else
                            {
                                // Create a new token object
                                Token captureToken = new Token();

                                // Initialize with the values from the group
                                captureToken.InitToken(g.Index, g.Index + g.Length - 1, groupName, g.Value);

                                // Add to the named matches collection.
                                namedMatches.PushBack(captureToken);
                            }
                        }
                    }

                    // Set up the Object pair with the token and the sub matches
                    ObjectPair op = new ObjectPair();
                    op.Object1 = t;
                    op.Object2 = namedMatches;

                    // Put match on return vector
                    returnVector.PushBack(op);

                    // If only want the first match return.
                    if (findFirstMatchOnly)
                    {
                        return returnVector;
                    }

                    // Get the next match
                    foundMatch = foundMatch.NextMatch();
                }

                // Return the results
                return returnVector;
            }
            catch (Exception ex)
            {
                var ue = ex.AsExtract("ELI41754");
                ue.AddDebugData("Pattern", _pattern, true);
                throw ue;
            }
        }

        /// <summary>
        /// Search the input text and retrieves tokens representing the named groups from a
        /// successful search. The primary search result is ignored.
        /// </summary>
        /// <param name="parser">The <see cref="Regex"/> to be used for the search</param>
        /// <param name="input">The string to search.</param>
        /// <param name="findFirstMatchOnly"><see langword="true"/> to return the named groups for
        /// the first match only; <see langword="true"/> to return the named groups for all matches.
        /// </param>
        /// <returns>An <see cref="IUnknownVector"/> of <see cref="Token"/>s representing the named
        /// groups from a successful search. Null will be returned if the overall regex
        /// did not match. An empty vector will be returned if there was a match but there were no
        /// non-empty named groups.</returns>
        IUnknownVector FindNamedGroups(Regex parser, string input, bool findFirstMatchOnly)
        {
            try
            {
                // Get the first match if there is one
                Match foundMatch = parser.Match(input);

                if (!foundMatch.Success)
                {
                    return null;
                }

                // Create the return object
                IUnknownVector returnVector = new IUnknownVector();

                // Iterate every match.
                while (foundMatch.Success)
                {
                    // Process the groups, starting with the 2nd item
                    // in the group list because the first item is just the
                    // entire match
                    for (int i = 1; i < foundMatch.Groups.Count; i++)
                    {
                        // Get the group
                        Group g = foundMatch.Groups[i];

                        // Get the name of the group from the parser
                        string groupName = parser.GroupNameFromNumber(i);

                        // Don't create a capture token if name begins with a number or
                        // an '_' since these are understood as representing non-named groups
                        // Group name can be empty string so don't just index into it
                        // https://extract.atlassian.net/browse/ISSUE-13902
                        char firstChar = groupName.FirstOrDefault();
                        if (!_outputUnderscoreGroups && firstChar == '_'
                            || firstChar == default(char) || char.IsNumber(firstChar))
                        {
                            continue;
                        }

                        // If returning all captures and the group actually captured something
                        // iterate through its capture collection
                        if (ReturnAllGroupCaptures && g.Success)
                        {
                            foreach (Capture c in g.Captures)
                            {
                                // Create a new token object
                                Token captureToken = new Token();

                                // Initialize with the values from the capture
                                captureToken.InitToken(c.Index, c.Index + c.Length - 1, groupName, c.Value);

                                // Add to the named matches collection.
                                returnVector.PushBack(captureToken);
                            }
                        }
                        // Otherwise use the values of the group
                        else
                        {
                            // Create a new token object
                            Token captureToken = new Token();

                            // Initialize with the values from the group
                            captureToken.InitToken(g.Index, g.Index + g.Length - 1, groupName, g.Value);

                            // Add to the named matches collection.
                            returnVector.PushBack(captureToken);
                        }
                    }

                    // If only want the first match return.
                    if (findFirstMatchOnly)
                    {
                        break;
                    }

                    // Get the next match
                    foundMatch = foundMatch.NextMatch();
                }

                return returnVector;
            }
            catch (Exception ex)
            {
                var ue = ex.AsExtract("ELI41755");
                ue.AddDebugData("Pattern", _pattern, true);
                throw ue;
            }
        }

        /// <summary>
        /// Checks input string for exact match of the pattern.
        /// </summary>
        /// <param name="parser">The <see cref="Regex"/> to be used for the search</param>
        /// <param name="input">The string to check for pattern.</param>
        /// <returns><see langword="true"/> if the input exactly matches pattern.
        /// <see langword="false"/> otherwise</returns>
        bool StringMatchesPattern(Regex parser, string input)
        {
            try
            {
                // Find the pattern in the input
                Match foundMatch = parser.Match(input);

                // If the beginning and end of the match is the beginning and end of the
                // input string return true
                return foundMatch.Success
                    && foundMatch.Index == 0
                    && foundMatch.Length == input.Length;
            }
            catch (Exception ex)
            {
                var ue = ex.AsExtract("ELI41756");
                ue.AddDebugData("Pattern", _pattern, true);
                throw ue;
            }
        }

        /// <summary>
        /// Determines the locations and resultant replacement text if matches were replaced with 
        /// the specified text.
        /// </summary>
        /// <param name="parser">The <see cref="Regex"/> to be used for the search</param>
        /// <param name="input">The <see cref="String"/> to search for replacements.</param>
        /// <param name="replacement">The string to use to replace matches in 
        /// <paramref name="input"/>. May contain capture groups (e.g. "$1 $2").</param>
        /// <param name="findFirstMatchOnly"><see langword="true"/> if only the first match should 
        /// be found; <see langword="false"/> if more than one match should be found.</param>
        /// <returns>An <see cref="IUnknownVector"/> of <see cref="Token"/>s, where the 
        /// positions are expressed prior to performing the replacement and the value of the token 
        /// represents the text after performing the replacement.</returns>
        IUnknownVector FindReplacements(Regex parser, string input, string replacement,
            bool findFirstMatchOnly)
        {
            try
            {
                // Get the first match if there is one
                Match foundMatch = parser.Match(input);

                // Create a vector to hold the found replacements
                IUnknownVector replacements = new IUnknownVector();

                // While there is a match and that match is not empty process the match
                while (foundMatch.Success && foundMatch.Length != 0 &&
                    foundMatch.Index < input.Length)
                {
                    // Create the token object for the match
                    Token token = new Token();
                    token.InitToken(foundMatch.Index, foundMatch.Index + foundMatch.Length - 1, "",
                        foundMatch.Result(replacement));

                    // Put match in return vector
                    replacements.PushBack(token);

                    // If we are only looking for the first match, we are done.
                    if (findFirstMatchOnly)
                    {
                        return replacements;
                    }

                    // Get the next match
                    foundMatch = foundMatch.NextMatch();
                }

                // Return the results
                return replacements;
            }
            catch (Exception ex)
            {
                var ue = ex.AsExtract("ELI41757");
                ue.AddDebugData("Pattern", _pattern, true);
                throw ue;
            }
        }

        #endregion
    }

    /// <summary>
    /// Class used to construct and hold data for a compound, find-if-x-of parser. Actual
    /// finding logic will be handled by the DotNetRegexParser class.
    /// </summary>
    class FindIfXOf
    {
        /// <summary>
        /// Gets the test parsers to be used to determine if the input meats the minimum requirements
        /// </summary>
        public DotNetRegexParser[] TestParsers { get; }

        /// <summary>
        /// Gets the finding parser. This is the main parser that will be used to find attributes
        /// or do replacements.
        /// </summary>
        public DotNetRegexParser FindingParser { get; }

        /// <summary>
        /// The definition of the syntax used to specify the test parser and the finding parser.
        /// ( (testpattern1) (testpattern2) ) (findingpattern)
        /// </summary>
        const string _FIND_IF_X_OF_PATTERN =
            @"\A\s*\((\s*\((?'TestPattern'" + _PATTERN + @")\))+\s*\)" +
            @"\s*\((?'FindPattern'" + _PATTERN + @")\)\s*\z";

        /// <summary>
        /// Allow empty patterns
        /// </summary>
        const string _PATTERN = "(" + RegexParsingPatterns.VALID_SEQUENCE + ")?";

        /// <summary>
        /// The options to be used internally for regex pattern matching
        /// </summary>
        const RegexOptions _REGEX_OPTIONS = RegexOptions.ExplicitCapture
            | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindIfXOf"/> class.
        /// </summary>
        /// <param name="pattern">The string containing the test patterns and finding pattern.</param>
        /// <param name="parent">The parent parser to be used as a basis of the other patterns</param>
        public FindIfXOf(string pattern, DotNetRegexParser parent)
        {
            try
            {
                var match = Regex.Match(pattern, _FIND_IF_X_OF_PATTERN, _REGEX_OPTIONS);

                if (!match.Success)
                {
                    var ue = new ExtractException("ELI41752", "Could not parse FindIfXOf pattern");
                    AddHint(pattern, ue);
                    throw ue;
                }

                // Clone the parent so that any options will be propogated (e.g., case sensitivity or timeout)
                FindingParser = parent.ShallowClone();

                // Setting the pattern will reset the regex type to be plain old Regex (it could change after parsing, though)
                FindingParser.Pattern = match.Groups["FindPattern"].Value;

                // Do the same for the test patterns
                TestParsers = match.Groups["TestPattern"].Captures.Cast<Capture>()
                    .Select(c =>
                    {
                        var parser = parent.ShallowClone();
                        parser.Pattern = c.Value;
                        return parser;
                    }).ToArray();

            }
            catch (Exception ex)
            {
                var ue = ex.AsExtract("ELI41753");
                ue.AddDebugData("Pattern", pattern, true);
                throw ue;
            }
        }

        /// <summary>
        /// Adds a hint as to why the pattern did not parse.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="exception">The exception to add a hint to.</param>
        static void AddHint(string pattern, ExtractException exception)
        {
            try
            {
                var balanceParensPattern = @"\A" + _PATTERN + @"\z";
                var tooManyClosingParensPattern = @"\A(?>(" + RegexParsingPatterns.VALID_SEQUENCE + @")+)?\)";
                var notEnoughClosingParensPattern = @"\A(?>(" + RegexParsingPatterns.VALID_SEQUENCE + @")+)?\(";
                if (!Regex.IsMatch(pattern, balanceParensPattern, _REGEX_OPTIONS))
                {
                    if (Regex.IsMatch(pattern, tooManyClosingParensPattern, _REGEX_OPTIONS))
                    {
                        exception.AddDebugData("Hint", "Too many closing parens", true);
                        return;
                    }

                    if (Regex.IsMatch(pattern, notEnoughClosingParensPattern, _REGEX_OPTIONS))
                    {
                        exception.AddDebugData("Hint", "Not enough closing parens", true);
                        return;
                    }

                    exception.AddDebugData("Hint", "Invalid regex syntax", true);
                    return;
                }

                var parensPattern = @"\((" + _PATTERN + @")\)";
                var topLevelGroups = Regex.Matches(pattern, parensPattern, _REGEX_OPTIONS);
                var parenCount = topLevelGroups.Count;
                if (parenCount != 2)
                {
                    exception.AddDebugData("Hint", UtilityMethods.FormatInvariant($"Expecting 2 top-level groups, found {parenCount}"), true);
                    return;
                }

                var topLevelPattern = @"\A\s*" + parensPattern + @"\s*" + parensPattern + @"\s*\z";
                if (!Regex.IsMatch(pattern, topLevelPattern, _REGEX_OPTIONS))
                {
                    exception.AddDebugData("Hint", "Expecting only whitespace between top-level groups", true);
                    return;
                }

                var testPatterns = topLevelGroups[0].Groups[1].Value;
                var testPatternGroups = Regex.Matches(testPatterns, parensPattern, _REGEX_OPTIONS);
                if (testPatternGroups.Count == 0)
                {
                    exception.AddDebugData("Hint", "At least one test pattern is required", true);
                    return;
                }

                var testPatternsPattern = @"\A(\s*" + parensPattern + @")+\s*\z";
                if (!Regex.IsMatch(pattern, testPatternsPattern, _REGEX_OPTIONS))
                {
                    exception.AddDebugData("Hint", "Expecting only whitespace between test pattern groups", true);
                    return;
                }
            }
            catch { }
        }
    }
}