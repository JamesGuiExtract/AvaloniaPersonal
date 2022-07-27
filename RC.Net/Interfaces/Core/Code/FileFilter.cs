using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

// This assembly is reserved for the definition of interfaces and helper classes for those
// interfaces. To ensure these interfaces are accessible from all projects without circular
// dependency issues and to allow the assemblies definitions to be used in both 32 and 64 bit code,
// This assembly should have no dependencies on any other Extract projects.
namespace Extract.Interfaces
{
    /// <summary>
    /// A helper class for <see cref="FileReceiver"/> which enables lists of file paths to be
    /// filtered to meet the specifications of the <see cref="FileReceiver"/>'s creator.
    /// This class is constructed by an out-of-process channel creator and stored in the service's
    /// <see cref="IWcfFileReceiverManager"/> channel, but is important that
    /// <see cref="FileMatchesFilter"/> be used within an endpoint's process space as the service
    /// that is hosting it may not have necessary network access to properly evaluate the filter.
    /// </summary>
    [DataContract]
    public class FileFilter
    {
        /// <summary>
        /// If not <see langword="null"/> or empty, qualifying file paths must start with this value.
        /// </summary>
        [DataMember]
        string _pathRoot;

        /// <summary>
        /// The Window's style file filter qualifying files must match. (Multiple filters may be
        /// delemited with semi-colons).
        /// </summary>
        [DataMember]
        string _filterPattern;

        /// <summary>
        /// <see langword="true"/> to include the matchine files within folders that are supplied;
        /// <see langword="false"/> to ignore supplied folders.
        /// </summary>
        [DataMember]
        bool _allowFolders;

        /// <summary>
        /// A regular expression equivalent of _filterPattern.
        /// </summary>
        Regex _filterRegex;

        /// <summary>
        /// Initializes a new <see cref="FileFilter"/> instance.
        /// <para><b>Note</b></para>
        /// This constructor uses code from this posting on stackoverflow:
        /// http://stackoverflow.com/questions/652037/how-do-i-check-if-a-filename-matches-a-wildcard-pattern
        /// </summary>
        /// <param name="pathRoot">If not <see langword="null"/> or empty, qualifying file paths must
        /// start with this value.</param>
        /// <param name="filterPattern">The Window's style file filter qualifying files must match.
        /// (Multiple filters may be delemited with semi-colons).</param>
        /// <param name="allowFolders"><see langword="true"/> to include the matchine files within
        /// folders that are supplied; <see langword="false"/> to ignore supplied folders.</param>
        public FileFilter(string pathRoot, string filterPattern, bool allowFolders)
        {
            _ = filterPattern ?? throw new ArgumentNullException(nameof(filterPattern));

            // Ensure the _pathRoot doesn't include trailing slash or whitespace to simplify path
            // checks.
            if (pathRoot != null)
            {
                _pathRoot = pathRoot.TrimEnd('\\', '/', ' ', '\t');
            }
            _allowFolders = allowFolders;

            // Convert filterPattern into a RegEx pattern that can be used to evaluate fileNames.
            Regex HasQuestionMarkRegEx = new Regex(@"\?");
            Regex IlegalCharactersRegex = new Regex("[" + @"\/:<>|" + "\"]");
            Regex CatchExtentionRegex = new Regex(@"^\s*.+\.([^\.]+)\s*$");
            string NonDotCharacters = @"[^.]*";

            StringBuilder regexPatternBuilder = new StringBuilder();

            // Allow semi-colon to sperate multiple patterns. Loop to build a Regex for each pattern
            // separately (and "or" them together into one unified _filterRegex)
            char[] splitterChar = new char[] { ';' };
            string[] filePatterns =
                filterPattern.Split(splitterChar, StringSplitOptions.RemoveEmptyEntries);
            foreach (string pattern in filePatterns)
            {
                if (string.IsNullOrWhiteSpace(pattern))
                {
                    throw new ArgumentNullException(nameof(filterPattern));
                }
                else if (IlegalCharactersRegex.IsMatch(pattern))
                {
                    throw new ArgumentException("The specified file filter contains ilegal characters.");
                }

                // Create a new parenthesized term for this pattern.
                if (regexPatternBuilder.Length > 0)
                {
                    regexPatternBuilder.Append('|');
                }
                regexPatternBuilder.Append('(');

                string regexPattern = pattern.Trim();

                // Build the regex term for the current pattern.
                bool hasExtension = CatchExtentionRegex.IsMatch(regexPattern);
                bool matchExact = false;
                if (HasQuestionMarkRegEx.IsMatch(regexPattern))
                {
                    matchExact = true;
                }
                else if (hasExtension)
                {
                    matchExact = CatchExtentionRegex.Match(regexPattern).Groups[1].Length != 3;
                }
                regexPattern = Regex.Escape(regexPattern);
                regexPattern = "^" + Regex.Replace(regexPattern, @"\\\*", ".*");
                regexPattern = Regex.Replace(regexPattern, @"\\\?", ".");

                regexPatternBuilder.Append(regexPattern);
                if (!matchExact && hasExtension)
                {
                    regexPatternBuilder.Append(NonDotCharacters);
                }
                regexPatternBuilder.Append("$)");
            }


            _filterPattern = regexPatternBuilder.ToString();
        }

        /// <summary>
        /// Creates the <see cref="_filterRegex"/> instance using <see cref="_filterRegex"/>.
        /// This gets called immediately after deserialization due to the OnDeserializedAttribute.
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserializedAttribute]
        void InitRegex(StreamingContext context)
        {
            InitRegex();
        }

        /// <summary>
        /// Creates the <see cref="_filterRegex"/> instance using <see cref="_filterRegex"/>.
        /// </summary>
        void InitRegex()
        {
            _filterRegex = new Regex(_filterPattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Tests whether the specified <see paramref="pathName"/> meets the filter specifications
        /// of this instance.
        /// <para><b>Note</b></para>
        /// It is important that this method be called within an endpoint's process space as the
        /// service that is hosting it may not have necessary network access to properly evaluate
        /// the filter.
        /// </summary>
        /// <param name="pathName">The path name to be tested.</param>
        /// <returns><see langword="true"/> if the name meets the filter requirements; otherwise
        /// <see langword="false"/>.</returns>
        // This assembly and class do not have access to the ExtractException class and therefore
        // we can't really do anything about the exception if we caught it.
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public bool FileMatchesFilter(string pathName)
        {
            _ = pathName ?? throw new ArgumentNullException(nameof(pathName));

            if (_filterRegex == null)
            {
                InitRegex();
            }

            // [DotNetRCAndUtils:710]
            // If specified, _pathRoot must exactly match or must match with a trailing backslash so
            // as not to allow files from a folder that only starts with _rootPath.
            if (!string.IsNullOrWhiteSpace(_pathRoot) &&
                !pathName.Equals(_pathRoot, StringComparison.OrdinalIgnoreCase) &&
                !pathName.StartsWith(_pathRoot + "\\", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (_allowFolders && Directory.Exists(pathName))
            {
                return true;
            }

            return _filterRegex.IsMatch(pathName);
        }
    }
}
