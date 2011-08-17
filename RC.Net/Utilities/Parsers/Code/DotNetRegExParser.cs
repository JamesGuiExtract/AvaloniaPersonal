using Extract.Licensing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.Parsers
{
    /// <summary>
    /// Class that implements the COM interfaces to encapsulate the .NET regular expression engine
    /// for use with Flex/IDShield.
    /// </summary>
    // Needs ComVisible = <see langword="true"/> because this implements com interface
    [ComVisible (true)]
    [Guid("6136D2EC-E3DB-4a13-9CDD-89C1C06E9CD5"), ProgIdAttribute("ESRegExParser.DotNetRegExParser.1")]
    public class DotNetRegexParser : IRegularExprParser, ILicensedComponent
    {
        #region Fields

        /// <summary>
        /// The RegEx object to use.
        /// </summary>
        private Regex _regexParser;

        /// <summary>
        /// Specifies if a search should ignore case.
        /// </summary>
        private bool _ignoreCase = true;

        /// <summary>
        /// Pattern to be searched for.
        /// </summary>
        private string _pattern = "";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(DotNetRegexParser).ToString();

        #endregion

        #region IRegularExprParser Properties

        /// <summary>
        /// Property to indicate if the Regex parser should ignore case when looking for matches.
        /// </summary>
        public bool IgnoreCase
        {
            get
            {
                return _ignoreCase;
            }
            set
            {
                // Only change the value if it is different from the previous value.
                if (_ignoreCase != value)
                {
                    _ignoreCase = value;

                    // Reset the RegEx parser to null since options can only be set when
                    // creating a RegEx object.
                    _regexParser = null;
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
                }
            }
        }

        #endregion

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
        public IUnknownVector Find(string strInput, bool bFindFirstMatchOnly, bool bReturnNamedMatches)
        {
            try
            {
                // Make sure the Parser is licensed
                ValidateLicense();

                // Get the parser
                Regex parser = GetRegexParser();

                // Get the first match if there is one
                Match foundMatch = parser.Match(strInput);

                // Create the return object
                IUnknownVector returnVector = new IUnknownVector();

                // While there is a match and that match is not empty process the matches
                while (foundMatch.Success && foundMatch.Length != 0 && foundMatch.Index < strInput.Length)
                {
                    // Create the token object for the match
                    Token t = new Token();
                    t.InitToken(foundMatch.Index, foundMatch.Index + foundMatch.Length - 1, "",
                        foundMatch.Value);

                    // Get the sub matches 
                    IUnknownVector namedMatches = null;

                    if (bReturnNamedMatches)
                    {
                        namedMatches = new IUnknownVector();

                        // Process the groups, starting with the 2nd item
                        // in the group list because the first item is just the
                        // entire match
                        for ( int i = 1; i < foundMatch.Groups.Count; i++)
                        {
                            // Get the group
                            Group g = foundMatch.Groups[i];

                            // Get the name of the group from the parser
                            string groupName = parser.GroupNameFromNumber(i);

                            // Create a new token object
                            Token captureToken = new Token();

                            // Initialize with the values from the group
                            captureToken.InitToken(g.Index, g.Index + g.Length - 1, groupName, g.Value);

                            // Add to the named matches collection.
                            namedMatches.PushBack(captureToken);
                        }
                    }

                    // Set up the Object pair with the token and the sub matches
                    ObjectPair op = new ObjectPair();
                    op.Object1 = t;
                    op.Object2 = namedMatches;

                    // Put match on return vector
                    returnVector.PushBack(op);

                    // If only want the first match return.
                    if (bFindFirstMatchOnly)
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
        /// groups from a successful search. An empty vector will be returned if the overall regex
        /// did not match or there were no non-empty named groups.</returns>
        public IUnknownVector FindNamedGroups(string strInput, bool bFindFirstMatchOnly)
        {
            try
            {
                // Make sure the Parser is licensed
                ValidateLicense();

                // Get the parser
                Regex parser = GetRegexParser();

                // Get the first match if there is one
                Match foundMatch = parser.Match(strInput);

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

                        // Create a new token object
                        Token captureToken = new Token();

                        // Initialize with the values from the group
                        captureToken.InitToken(g.Index, g.Index + g.Length - 1, groupName, g.Value);

                        // Add to the named matches collection.
                        returnVector.PushBack(captureToken);
                    }

                    // If only want the first match return.
                    if (bFindFirstMatchOnly)
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

                // Get the parser
                Regex parser = GetRegexParser();

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
                Regex parser = GetRegexParser();

                // Return results of IsMatch
                return parser.IsMatch(strInput);
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
                    bool found = GetRegexParser().IsMatch(strInput);

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
                Regex parser = GetRegexParser();

                // Find the pattern in the input
                Match foundMatch = parser.Match(strInput);

                // If the beginning and end of the match is the beginning and end of the
                // input string return true
                return foundMatch.Success
                    && foundMatch.Index == 0
                    && foundMatch.Length == strInput.Length;
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
                Regex parser = GetRegexParser();

                // Get the first match if there is one
                Match foundMatch = parser.Match(bstrInput);

                // Create a vector to hold the found replacements
                IUnknownVector replacements = new IUnknownVector();

                // While there is a match and that match is not empty process the match
                while (foundMatch.Success && foundMatch.Length != 0 && 
                    foundMatch.Index < bstrInput.Length)
                {
                    // Create the token object for the match
                    Token token = new Token();
                    token.InitToken(foundMatch.Index, foundMatch.Index + foundMatch.Length - 1, "",
                        foundMatch.Result(bstrReplacement));

                    // Put match in return vector
                    replacements.PushBack(token);

                    // If we are only looking for the first match, we are done.
                    if (bFindFirstMatchOnly)
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
        /// Returns the RegexParser with the set pattern and options
        /// </summary>
        /// <returns>Regex parser with the pattern and options set.</returns>
        private Regex GetRegexParser()
        {
            string expandedPattern = "";

            try
            {
                // if internal variable is null, create a new parser with the pattern and options.
                if (_regexParser == null)
                {
                    // Expand any fuzzy search terms into the equivalent regular expression.
                    expandedPattern = FuzzySearchRegexBuilder.ExpandFuzzySearchExpressions(_pattern);

                    _regexParser = new Regex(expandedPattern, GetOptions());
                }

                return _regexParser;
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
        }

        /// <summary>
        /// Gets the options to use when initializing a Regex object
        /// </summary>
        /// <returns>The option indicated with the <see cref="IgnoreCase"/> property or'd with
        /// the Multiline option</returns>
        private RegexOptions GetOptions()
        {
            if (_ignoreCase)
            {
                return RegexOptions.IgnoreCase | RegexOptions.Multiline;
            }
            return RegexOptions.Multiline;
        }

        #endregion
    }
}
