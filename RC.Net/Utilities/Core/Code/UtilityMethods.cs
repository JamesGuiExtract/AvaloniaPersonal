using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Extract.Utilities
{
    /// <summary>
    /// A class containing utility helper methods
    /// </summary>
    public static class UtilityMethods
    {
        #region Constants

        /// <summary>
        /// Object name used for license validation calls.
        /// </summary>
        readonly static string _OBJECT_NAME = typeof(UtilityMethods).ToString();
        
        /// <summary>
        /// The uppercase letters for use by GetRandomString.
        /// </summary>
        const string _UPPERCASE_LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// The lowercase letters for use by GetRandomString.
        /// </summary>
        const string _LOWERCASE_LETTERS = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// The digits for use by GetRandomString
        /// </summary>
        const string _DIGITS = "0123456789";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Used to validate names for XML elements.
        /// </summary>
        static ThreadLocal<RegexStringValidator> _xmlNameValidator = new ThreadLocal<RegexStringValidator>(() =>
            {
                string nameStartChar = @":A-Z_a-z\xC0-\xD6\xD8-\xF6\xF8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD";
                string nameChar = @"-.0-9\xB7\u0300-\u036F\u203F-\u2040" + nameStartChar;

                return new RegexStringValidator("^[" + nameStartChar + "][" + nameChar + "]+$");
            });

        /// <summary>
        /// Used to validate email addresses.
        /// </summary>
        static ThreadLocal<Regex> _emailValidator = new ThreadLocal<Regex>(() =>
                new Regex(@"^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$",
                    RegexOptions.IgnoreCase) );

        /// <summary>
        /// Used to validate identifiers (identifiers must start with either an underscore
        /// or a letter and can be followed by 0 or more underscores, letters or numbers).
        /// </summary>
        static ThreadLocal<Regex> _identifierValidator = new ThreadLocal<Regex>(() => new Regex(@"^[_a-zA-Z]\w*$"));

        /// <summary>
        /// Random number generator used by GetRandomString
        /// </summary>
        static readonly ThreadLocal<Random> _randomNumberGenerator = new ThreadLocal<Random>(() => new Random());

        #endregion Fields

        /// <summary>
        /// Swaps two value types in place.
        /// </summary>
        /// <typeparam name="T">The type of objects being swapped.</typeparam>
        /// <param name="valueOne">The first value to swap.</param>
        /// <param name="valueTwo">The second value to swap.</param>
        // These values are pass by reference because we are 'swapping' them in place. The
        // result of the swap method is that the two values are swapped. In order for this
        // to be reflected after the call to this method the objects must be passed as a
        // reference.
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#")]
        public static void Swap<T>(ref T valueOne, ref T valueTwo)
        {
            try
            {
                T c = valueOne;
                valueOne = valueTwo;
                valueTwo = c;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30146", ex);
            }
        }

        /// <summary>
        /// Alls the types that implement interface.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>An array of <see cref="Type"/> objects that implement the
        /// specified interface <paramref name="interfaceType"/>.</returns>
        public static Type[] AllTypesThatImplementInterface(Type interfaceType,
            params Assembly[] assemblies)
        {
            try
            {
                if (!interfaceType.IsInterface)
                {
                    throw new ArgumentException("Type to find must be an interface.",
                        "interfaceType");
                }
                if (assemblies == null)
                {
                    throw new ArgumentNullException("assemblies");
                }

                return assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(p => p.IsClass && interfaceType.IsAssignableFrom(p))
                    .ToArray();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31131", ex);
            }
        }

        /// <summary>
        /// Creates the type from type name.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns>An instance of the specified type.</returns>
        public static object CreateTypeFromTypeName(string typeName)
        {
            try
            {
                // Build name of assembly from typename (i.e. this assumes that type name follows
                // Extract standards - Extract.Test.FakeAssembly.FakeType
                // is in Extract.Test.FakeAssembly.dll)

                // Build the name to the assembly containing the type
                var sb = new StringBuilder();
                var names = typeName.Split(new char[] { '.' },
                    StringSplitOptions.RemoveEmptyEntries);
                if (names.Length > 0)
                {
                    sb.Append(names[0]);
                }
                for (int i = 1; i < names.Length - 1; i++)
                {
                    sb.Append(".");
                    sb.Append(names[i]);
                }
                var assemblyName = new AssemblyName();
                assemblyName.Name = sb.ToString();

                // Load the assembly if needed
                var assembly = LoadAssemblyIfNotLoaded(assemblyName);

                // Create the type and return it
                return assembly.CreateInstance(typeName, true);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31149", ex);
            }
        }

        /// <summary>
        /// Loads the assembly if not loaded.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly to load.</param>
        /// <returns>The loaded assembly.</returns>
        public static Assembly LoadAssemblyIfNotLoaded(AssemblyName assemblyName)
        {
            try
            {
                string shortName = assemblyName.Name;
                Assembly assembly = null;
                foreach (var loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (shortName.Equals(loadedAssembly.GetName().Name,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        assembly = loadedAssembly;
                        break;
                    }
                }

                // If the assembly is not loaded, load it
                if (assembly == null)
                {
                    assembly = Assembly.Load(assemblyName);
                }

                return assembly;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31153", ex);
            }
        }

        /// <summary>
        /// Creates the type from assembly.
        /// </summary>
        /// <typeparam name="T">The type to load from the assembly.</typeparam>
        /// <param name="assemblyFileName">Name of the assembly file.</param>
        /// <returns>A new instance of <typeparamref name="T"/>.</returns>
        public static T CreateTypeFromAssembly<T>(string assemblyFileName) where T : class
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyFileName);
                if (!LicenseUtilities.VerifyAssemblyData(assembly))
                {
                    var ee = new ExtractException("ELI31150",
                        "Unable to load assembly, verification failed.");
                    ee.AddDebugData("Assembly File", assemblyFileName, true);
                    throw ee;
                }

                T value = null;
                // Using reflection, iterate the classes in the assembly looking for one that 
                // implements T
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(T).IsAssignableFrom(type))
                    {
                        if (value != null)
                        {
                            var ee = new ExtractException("ELI31151",
                                "Assembly contains multiple implementations of specified type.");
                            ee.AddDebugData("Type", typeof(T).ToString(), false);
                            throw ee;
                        }

                        // Create and instance of the DEP class.
                        value = (T)assembly.CreateInstance(type.ToString());

                        // Keep searching to ensure there are not multiple implementations
                    }
                }

                return value;
            }
            catch (Exception ex)
            {
                var ee = ExtractException.AsExtractException("ELI31152", ex);

                try
                {
                    var typeLoadException = ex as ReflectionTypeLoadException;
                    if (typeLoadException != null)
                    {
                        var loaderExceptions = typeLoadException.LoaderExceptions;
                        foreach (var item in loaderExceptions)
                        {
                            ee.AddDebugData("LoaderException", item.Message, encrypt: false);
                        }
                    }
                }
                catch (Exception exc)
                {
                    exc.ExtractLog("ELI41364");
                }

                throw ee;
            }
        }

        /// <summary>
        /// Validates an XML name element name per the specifications here:
        /// http://www.w3.org/TR/REC-xml/#NT-S
        /// </summary>
        /// <param name="name">The name to be validated.</param>
        public static void ValidateXmlElementName(string name)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI31717", _OBJECT_NAME);

                _xmlNameValidator.Value.Validate(name);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31702");
            }
        }

        /// <summary>
        /// Determines whether <paramref name="emailAddress"/> is a valid email address.
        /// <para>
        /// The email validation regex is a modified form of the RFC 2822 standard for internet
        /// email addresses from http://www.regular-expressions.info/email.html
        /// </para>
        /// </summary>
        /// <param name="emailAddress">The email address to validate.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="emailAddress"/> is a valid email address;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool IsValidEmailAddress(string emailAddress)
        {
            try
            {
                return _emailValidator.Value.IsMatch(emailAddress);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32367");
            }
        }

        /// <summary>
        /// Determines whether all of the specified identifiers are valid.
        /// <para>Note:</para>
        /// A valid identifier must be of the form '[_a-zA-Z]\w*'
        /// </summary>
        /// <param name="identifiers">The identifiers.</param>
        /// <returns>
        /// <see langword="true"/> if all of the identifiers are valid;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsValidIdentifier(params string[] identifiers)
        {
            try
            {
                return identifiers.All(s => _identifierValidator.Value.IsMatch(s));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32441");
            }
        }

        /// <summary>
        /// Displays a message box with the specified message and caption.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="caption">The caption for the message box.</param>
        /// <param name="error">If <see langword="true"/> displays the error icon, otherwise
        /// displays the information icon.</param>
        public static void ShowMessageBox(string message, string caption, bool error)
        {
            try
            {
                MessageBox.Show(message, caption, MessageBoxButtons.OK,
                    error ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1, 0);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31890");
            }
        }

        /// <summary>
        /// Generates a random string.
        /// </summary>
        /// <param name="size">The size of the string to produce.</param>
        /// <param name="uppercase"><see langword="true"/> to include uppercase letters.</param>
        /// <param name="lowercase"><see langword="true"/> to include lowercase letters.</param>
        /// <param name="digits"><see langword="true"/> to include digits.</param>
        /// <returns>A random string.</returns>
        public static string GetRandomString(int size, bool uppercase, bool lowercase, bool digits)
        {
            try 
            {            
                ExtractException.Assert("ELI33379", "GetRandomString: empty character domain.",
                    uppercase || lowercase || digits);

                string charDomain = (uppercase ? _UPPERCASE_LETTERS : "") +
                                    (lowercase ? _LOWERCASE_LETTERS : "") +
                                    (digits ? _DIGITS : "");
                int domainSize = charDomain.Length;

                StringBuilder sb = new StringBuilder(size);
                for (int i = 0; i < size; i++)
                {
                    sb.Append(charDomain[_randomNumberGenerator.Value.Next(domainSize)]);
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33378");
            }
        }

        /// <summary>
        /// Returns an unique string to identify the current process
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static string GetCurrentProcessUpi()
        {
            try
            {
                string upi = "\\";
                upi += Environment.MachineName + "\\";
                upi += SystemMethods.GetProcessName(SystemMethods.GetCurrentProcessId());
                upi += SystemMethods.GetCurrentProcessId().AsString() + "\\";
                upi += DateTime.UtcNow.ToString("MdY\\H:m:s", CultureInfo.InvariantCulture);
                return upi;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34517");
            }
        }

        /// <summary>
        /// Executes the given action with try catch block that logs any exceptions
        /// </summary>
        /// <param name="eliCode">ELI code for the logged exception</param>
        /// <param name="action">The action that should be ran</param>
        public static void PerformWithExceptionLog(string eliCode, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                ex.ExtractLog(eliCode);
            }
        }

        /// <summary>
        /// Computes the Damerau-Levenshtein Distance between two strings, represented as arrays of
        /// integers, where each integer represents the code point of a character in the source string.
        /// Includes an optional threshold which can be used to indicate the maximum allowable distance.
        /// From: http://stackoverflow.com/questions/9453731/how-to-calculate-distance-similarity-measure-of-given-2-strings
        /// (http://stackoverflow.com/users/842685/joshua-honig)
        /// Modified to prevent user from having to pass in threshold value.
        /// </summary>
        /// <param name="source">The first string to compare.</param>
        /// <param name="target">The second string to compare.</param>
        /// <returns><see cref="int.MaxValue"/> if threshold exceeded; otherwise the
        /// Damerau-Levenshtein distance between the strings</returns>
        public static int LevenshteinDistance(string source, string target)
        {
            return LevenshteinDistance(source, target, int.MaxValue);
        }

        /// <summary>
        /// Computes the Damerau-Levenshtein Distance between two strings, represented as arrays of
        /// integers, where each integer represents the code point of a character in the source string.
        /// Includes an optional threshold which can be used to indicate the maximum allowable distance.
        /// From: http://stackoverflow.com/questions/9453731/how-to-calculate-distance-similarity-measure-of-given-2-strings
        /// (http://stackoverflow.com/users/842685/joshua-honig)
        /// </summary>
        /// <param name="source">The first string to compare.</param>
        /// <param name="target">The second string to compare.</param>
        /// <param name="threshold">Maximum allowable distance</param>
        /// <returns><see cref="int.MaxValue"/> if threshold exceeded; otherwise the
        /// Damerau-Levenshtein distance between the strings</returns>
        public static int LevenshteinDistance(string source, string target, int threshold)
        {
            try
            {
                int length1 = source.Length;
                int length2 = target.Length;

                // Return trivial case - difference in string lengths exceeds threshold
                if (Math.Abs(length1 - length2) > threshold) { return int.MaxValue; }

                // Ensure arrays [i] / length1 use shorter length 
                if (length1 > length2)
                {
                    Swap(ref target, ref source);
                    Swap(ref length1, ref length2);
                }

                int maxi = length1;
                int maxj = length2;

                int[] dCurrent = new int[maxi + 1];
                int[] dMinus1 = new int[maxi + 1];
                int[] dMinus2 = new int[maxi + 1];
                int[] dSwap;

                for (int i = 0; i <= maxi; i++) { dCurrent[i] = i; }

                int jm1 = 0, im1 = 0, im2 = -1;

                for (int j = 1; j <= maxj; j++)
                {

                    // Rotate
                    dSwap = dMinus2;
                    dMinus2 = dMinus1;
                    dMinus1 = dCurrent;
                    dCurrent = dSwap;

                    // Initialize
                    int minDistance = int.MaxValue;
                    dCurrent[0] = j;
                    im1 = 0;
                    im2 = -1;

                    for (int i = 1; i <= maxi; i++)
                    {

                        int cost = source[im1] == target[jm1] ? 0 : 1;

                        int del = dCurrent[im1] + 1;
                        int ins = dMinus1[i] + 1;
                        int sub = dMinus1[im1] + cost;

                        //Fastest execution for min value of 3 integers
                        int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

                        if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
                            min = Math.Min(min, dMinus2[im2] + cost);

                        dCurrent[i] = min;
                        if (min < minDistance) { minDistance = min; }
                        im1++;
                        im2++;
                    }
                    jm1++;
                    if (minDistance > threshold) { return int.MaxValue; }
                }

                int result = dCurrent[maxi];
                return (result > threshold) ? int.MaxValue : result;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39425");
            }
        }

        /// <summary>
        /// Parse pageRange, returns start and end page.
        /// </summary>
        /// <param name="pageRange">The string representation of the page range. Must have exactly one dash (-) and either start page, end page or both.</param>
        /// <param name="startPageNumber">Out parameter that will be set to the start page number. Could be 0, which means it's empty (no start page was present in the range).</param>
        /// <param name="endPageNumber">Out parameter that will be set to the end page number. Must be greater than 0 if start page is empty.</param>
        private static void GetStartAndEndPage(string pageRange, out int startPageNumber, out int endPageNumber)
        {
            // assume this is a range of page numbers, or last X number of pages
            // Further parse the string with delimiter as '-'
            string[] tokens = pageRange.Split('-');
            if (tokens.Length != 2)
            {
                var ue = new ExtractException("ELI39577", "Invalid format for page range or last X number of pages.");
                ue.AddDebugData("Page range", pageRange, false);
                throw ue;
            }
          
            string startPage = tokens[0].Trim();
            // start page could be empty
            startPageNumber = 0;
            if (!string.IsNullOrWhiteSpace(startPage))
            {
                if(startPage == "0")
                {
                    var ue = new ExtractException("ELI39578", "Starting page cannot be zero.");
                    ue.AddDebugData("Page range", pageRange, false);
                    throw ue;
                }
                // make sure the start page is a number
                if (!Int32.TryParse(startPage, out startPageNumber))
                {
                    var ue = new ExtractException("ELI39579", "Could not parse starting page of range.");
                    ue.AddDebugData("Starting page", startPage, false);
                    throw ue;
                }
            }
          
            string endPage = tokens[1].Trim();
            // end page must not be empty if start page is empty
            if (string.IsNullOrWhiteSpace(startPage) && string.IsNullOrWhiteSpace(endPage))
            {
                var ue = new ExtractException("ELI39580", "Starting and ending page can't be both empty.");
                ue.AddDebugData("Page range", pageRange, false);
                throw ue;
            }
            else if (string.IsNullOrWhiteSpace(endPage))
            {
                // if start page is not empty, but end page is empty, for instance, 2-,
                // then the user wants to get all pages from the starting page until the end
                endPageNumber = 0;
                return;
            }

            if(endPage == "0")
            {
                var ue = new ExtractException("ELI40054", "Ending page cannot be zero.");
                ue.AddDebugData("Page range", pageRange, false);
                throw ue;
            }
          
            if (!Int32.TryParse(endPage, out endPageNumber))
            {
                var ue = new ExtractException("ELI39581", "Could not parse ending page of range.");
                ue.AddDebugData("Ending page", endPage, false);
                throw ue;
            }
          
            // make sure the start page number is less than or equal to the end page number
            if (startPageNumber > endPageNumber)
            {
                var ue = new ExtractException("ELI39582", "Start page number must be less than or equal to the end page number.");
                ue.AddDebugData("Page range", pageRange, false);
                throw ue;
            }

            return;
        }

        /// <summary>
        /// Updates <see paramref="pageNumbers"/> with new page numbers.
        /// </summary>
        /// <param name="pageNumbers">The set of page numbers to update.</param>
        /// <param name="totalNumberOfPages">The total number of pages in the image</param>
        /// <param name="startPage">First page number of a range</param>
        /// <param name="endPage">Last page number of a range</param>
        /// <param name="throwExceptionOnPageOutOfRange">Whether to throw an exception if <see paramref="endPage"/>
        /// or <see paramref="startpage"/> are greater than <see paramref="totalNumberOfPages"/></param>
        private static void updatePageNumbers(HashSet<int> pageNumbers, 
                               int totalNumberOfPages, 
                               int startPage, 
                               int endPage,
                               bool throwExceptionOnPageOutOfRange)
        {
            bool endOutOfRange = endPage > totalNumberOfPages;
            if (throwExceptionOnPageOutOfRange)
            {
                if (endOutOfRange)
                {
                    var ue = new ExtractException("ELI39602", "Specified end page number is out of range.");
                    ue.AddDebugData("End Page Number", endPage, false);
                    ue.AddDebugData("Total Number Of Pages", totalNumberOfPages, false);
                    throw ue;
                }
                else if (startPage > totalNumberOfPages)
                {
                    var ue = new ExtractException("ELI39603", "Specified start page number is out of range.");
                    ue.AddDebugData("Start Page Number", startPage, false);
                    ue.AddDebugData("Total Number Of Pages", totalNumberOfPages, false);
                    throw ue;
                }
            }

            int lastPageNumber = !endOutOfRange && endPage > 0 ? endPage : totalNumberOfPages;
            for (int n = startPage; n <= lastPageNumber; n++)
            {
                pageNumbers.Add(n);
            }
        }

        /// <summary>
        /// Updates <see paramref="pageNumbers"/> with new page number(s).
        /// </summary>
        /// <param name="pageNumbers">The set of page numbers to update.</param>
        /// <param name="totalNumberOfPages">The total number of pages in the image</param>
        /// <param name="pageNumber">Page number or last X page numbers to add to the set.</param>
        /// <param name="throwExceptionOnPageOutOfRange">Whether to throw an exception if <see paramref="pageNumber"/>
        /// is greater than <see paramref="totalNumberOfPages"/></param>
        /// <param name="lastPagesDefined">If <see langword="true"/> then <see paramref="pageNumber"/> represents the last X number of pages.
        /// If <see langword="false"/> then <see paramref="pageNumber"/> is a single page number</param>
        private static void updatePageNumbers(HashSet<int> pageNumbers, 
                               int totalNumberOfPages, 
                               int pageNumber,
                               bool throwExceptionOnPageOutOfRange,
                               bool lastPagesDefined = false)
        {
            // Check if the page number is valid
            bool pageOutOfRange = pageNumber > totalNumberOfPages;
            if (pageOutOfRange)
            {
                // Throw exception if specified
                if (throwExceptionOnPageOutOfRange)
                {
                    var ue = new ExtractException("ELI39606", "Specified page number is out of range.");
                    ue.AddDebugData("Page Number", pageNumber, false);
                    ue.AddDebugData("Total Number Of Pages", totalNumberOfPages, false);
                    throw ue;
                }
                // Check if not last page defined just return
                else if (!lastPagesDefined)
                {
                    return;
                }
            }
            if (lastPagesDefined)
            {
                int n = !pageOutOfRange ? (totalNumberOfPages - pageNumber) + 1 : 1;
                for (; n <= totalNumberOfPages; n++)
                {
                    pageNumbers.Add(n);
                }
            }
            else
            {
                pageNumbers.Add(pageNumber);
            }
        }

        /// <summary>
        /// Based on the total number of pages and specified page numbers string, return a set of page numbers.
        /// Whoever calls this function, must have already called validatePageNumbers() to make sure the validity of 
        /// strSpecifiedPageNumbers.
        /// </summary>
        /// <param name="specifiedPageNumbers">String containing specified page numbers in various formats.</param>
        /// <param name="totalPages">The total number of pages in the image</param>
        /// <param name="throwExceptionOnPageOutOfRange">Whether to throw an exception if a page number
        /// is greater than <see paramref="totalPages"/></param>
        /// <returns>A set of page numbers in ascending order.</returns>
        private static HashSet<int> FillPageNumbersSet(string specifiedPageNumbers, int totalPages, bool throwExceptionOnPageOutOfRange)
        {
            // Assume before this methods is called, the caller has already called ValidatePageNumbers()
            string[] tokens = specifiedPageNumbers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var pageNumbers = new HashSet<int>();

            for (int n = 0; n < tokens.Length; n++)
            {
                // trim any leading/trailing white spaces
                string token = tokens[n].Trim();

                // if the token contains a dash
                if (token.IndexOf('-') != -1)
                {
                    // start page could be empty
                    int startPage, endPage;
                    GetStartAndEndPage(token, out startPage, out endPage);

                    if (startPage > 0 &&
                        (endPage >= startPage || endPage <= 0))
                    {
                        // range of pages
                        updatePageNumbers(pageNumbers, totalPages, startPage, endPage,
                            throwExceptionOnPageOutOfRange);
                    }
                    else
                    {
                        // last X number of pages
                        updatePageNumbers(pageNumbers, totalPages, endPage,
                            throwExceptionOnPageOutOfRange, true);
                    }
                }
                else
                {
                    // assume this is a page number
                    int pageNumber;
                    if (!Int32.TryParse(token, out pageNumber))
                    {
                        var ue = new ExtractException("ELI39604", "Could not parse page number.");
                        ue.AddDebugData("Page Number", token, false);
                        throw ue;
                    }
                    if (pageNumber <= 0)
                    {
                        var ue = new ExtractException("ELI39605", "Invalid page number.");
                        ue.AddDebugData("Page Number", pageNumber, false);
                        throw ue;
                    }

                    // single page number
                    updatePageNumbers(pageNumbers, totalPages, pageNumber,
                        throwExceptionOnPageOutOfRange);
                }
            }

            return pageNumbers;
        }

        /// <summary>
        /// Validate specifiedPageNumbers. Throws exception if the string is invalid.
        /// Valid page number format: single pages (eg. 2, 5), a range of pages (eg. 4-8),
        /// or last X number of pages (eg. -3). They must be separated by comma (,). When
        /// a range of pages is specified, starting page number must be less than ending page number.
        /// </summary>
        /// <param name="specifiedPageNumbers">String containing specified page numbers in various formats.</param>
        public static void ValidatePageNumbers(string specifiedPageNumbers)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(specifiedPageNumbers))
                {
                    throw new ExtractException("ELI39607", "Specified page number string is empty.");
                }

                // parse string into tokens
                string[] tokens = specifiedPageNumbers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int n = 0; n < tokens.Length; n++)
                {
                    // trim any leading/trailing white spaces
                    string token = tokens[n].Trim();

                    // if the token contains a dash
                    if (token.IndexOf('-') != -1)
                    {
                        // start page could be empty
                        int startPage, endPage;
                        GetStartAndEndPage(token, out startPage, out endPage);
                    }
                    else
                    {
                        // assume this is a page number
                        int pageNumber;
                        if (!Int32.TryParse(token, out pageNumber))
                        {
                            var ue = new ExtractException("ELI39609", "Could not parse page number.");
                            ue.AddDebugData("Page Number", token, false);
                            throw ue;
                        }
                        if (pageNumber <= 0)
                        {
                            var ue = new ExtractException("ELI39610", "Invalid page number.");
                            ue.AddDebugData("Page Number", pageNumber, false);
                            throw ue;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39612");
            }
        }

        /// <summary>
        /// Based on the total number of pages and specified page numbers string, return an enumeration
        /// of page numbers in an ascending order.
        /// Whoever calls this function, must have already called validatePageNumbers() to make sure the validity of 
        /// strSpecifiedPageNumbers.
        /// </summary>
        /// <param name="specifiedPageNumbers">String containing specified page numbers in various formats.</param>
        /// <param name="totalPages">The total number of pages in the image</param>
        /// <param name="throwExceptionOnPageOutOfRange">Whether to throw an exception if a page number
        /// is greater than <see paramref="totalPages"/></param>
        /// <returns>An enumeration of page numbers in ascending order.</returns>
        public static IEnumerable<int> GetPageNumbersFromString(string specifiedPageNumbers, int totalPages, bool throwExceptionOnPageOutOfRange)
        {
            try
            {
                var pageNumbers = FillPageNumbersSet(specifiedPageNumbers, totalPages, throwExceptionOnPageOutOfRange);
                return pageNumbers.OrderBy(p => p);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39611");
            }
        }

        /// <summary>
        /// Determines whether the specified text is a valid xpath expression.
        /// </summary>
        /// <param name="text">The text to validate.</param>
        /// <param name="throwException">if set to <c>true</c> throws the exception thrown by
        /// <see cref="System.Xml.XPath.XPathExpression.Compile"/>.</param>
        /// <returns>
        /// true if valid
        /// </returns>
        public static bool IsValidXPathExpression(string text, bool throwException = false)
        {
            try
            {
                System.Xml.XPath.XPathExpression.Compile(text);
                return true;
            }
            catch (Exception ex)
            {
                if (throwException)
                {
                    var ue = new ExtractException("ELI41632", "Invalid XPath Expression", ex);
                    ue.AddDebugData("XPath", text, encrypt: false);
                    throw ue;
                }
                return false;
            }
        }
    }

    /// <summary>
    /// Class to subscribe to events for debugging purposes
    /// http://stackoverflow.com/a/701831
    /// </summary>
    public class EventSubscriber
    {
        private static readonly MethodInfo HandleMethod = 
            typeof(EventSubscriber)
                .GetMethod("HandleEvent", 
                           BindingFlags.Instance | 
                           BindingFlags.NonPublic);

        private readonly EventInfo evt;

        private EventSubscriber(EventInfo evt)
        {
            this.evt = evt;
        }

        private void HandleEvent(object sender, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.CurrentCulture,"Event {0} fired", evt.Name));
        }

        private void Subscribe(object target)
        {
            Delegate handler = Delegate.CreateDelegate(
                evt.EventHandlerType, this, HandleMethod);
            evt.AddEventHandler(target, handler);
        }

        /// <summary>
        /// Subscribes to all events
        /// </summary>
        /// <param name="target">The target.</param>
        public static void SubscribeAll(object target)
        {
            try
            {
                foreach (EventInfo evt in target.GetType().GetEvents())
                {
                    EventSubscriber subscriber = new EventSubscriber(evt);
                    subscriber.Subscribe(target);
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40198");
            }
        }
    }
}
