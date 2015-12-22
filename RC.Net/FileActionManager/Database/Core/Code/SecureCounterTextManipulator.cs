using Extract.Interfaces;
using Extract.Utilities;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// Allows for extraction of unlock/update codes from license email text as well as generation
    /// of request or confirmation text.
    /// </summary>
    internal class SecureCounterTextManipulator
    {
        #region Statics

        /// <summary>
        /// Expect a code to consist of at least 8 blocks of 8 chars that are either digits or A-F
        /// broken only by whitespace. Each instance of whitespace must contain at most one newline
        /// and at most two consecutive non-newline whitespace chars. The code shall also be bounded
        /// on both sides by a newline. (No other text expected to share the first or last line of a
        /// code.)
        /// </summary>
        static Regex _codeParserRegex = new Regex(
            @"(?<=\A|[\x0A\x0D]\s*)" +
            @"(([\x20\t]{0,2}((\x0D?\x0A|\x0A?\x0D)[\x20\t]{0,2})?[0-9,A-F]){8}){8,}" +
            @"(?=\z|\s*[\x0A\x0D])",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #endregion Statics

        #region Fields

        /// <summary>
        /// The database server/name for associated with text to be generated. The will utilize the
        /// same server name used by the encrypted database ID, not the user-entered server
        /// (i.e., will never be "(local)").
        /// </summary>
        string _databaseName;

        /// <summary>
        /// The string representation of the GUID ID for the FAM DB.
        /// </summary>
        string _databaseID;

        /// <summary>
        /// A textual status of the database's secure counters as they currently stand.
        /// </summary>
        string _countersDescription;

        /// <summary>
        /// The encrypted request or confirmation code to be used in the generated text.
        /// </summary>
        string _licenseString;

        #endregion Fields

        #region Static Methods

        /// <summary>
        /// Parses and returns license codes from <see paramref="text"/>.
        /// </summary>
        /// <param name="text">The text to be parsed (a licensing message received from Extract).
        /// </param>
        /// <returns></returns>
        public static string ParseLicenseCode(string text)
        {
            try
            {
                var matches = _codeParserRegex.Matches(text);
                if (matches.Count == 1)
                {
                    // While _codeParserRegex will find codes with embedded whitespace, remove this
                    // whitespace from the returned code.
                    return Regex.Replace(matches[0].Value, @"\s", "");
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39072");
            }
        }

        #endregion Static Methods

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureCounterTextManipulator"/> class.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> for which text is to
        /// be generated.</param>
        public SecureCounterTextManipulator(FileProcessingDB fileProcessingDB)
        {
            try
            {
                // Get the code before the DatabaseID because it is possible for the
                // call to GetCounterUpdateRequestCode to create a new databaseID
                // if it is missing
                _licenseString = fileProcessingDB.GetCounterUpdateRequestCode();

                _databaseName = fileProcessingDB.ConnectedDatabaseServer + "/" +
                        fileProcessingDB.ConnectedDatabaseName;
                _databaseID = fileProcessingDB.DatabaseID;

                var countersDescription = new StringBuilder();
                var secureCounters = fileProcessingDB.GetSecureCounters(false)
                    .ToIEnumerable<ISecureCounter>();
                if (secureCounters.Any())
                {
                    foreach (var secureCounter in secureCounters)
                    {
                        countersDescription.Append(
                            secureCounter.ID.ToString("D3", CultureInfo.CurrentCulture));
                        countersDescription.Append(" ");
                        countersDescription.Append(secureCounter.Name);
                        countersDescription.Append(": ");
                        if (secureCounter.IsValid)
                        {
                            countersDescription.AppendLine(string.Format(CultureInfo.CurrentCulture,
                                "{0:n0}", secureCounter.Value));
                        }
                        else
                        {
                            countersDescription.AppendLine("Corrupted");
                        }
                    }
                }
                else
                {
                    countersDescription.AppendLine("No counters currently exist.");
                }

                _countersDescription = countersDescription.ToString();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39073");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the organization for which text is being generated.
        /// </summary>
        /// <value>
        /// The organization for which text is being generated.
        /// </value>
        public string Organization
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the email address to which Extract should respond.
        /// </summary>
        /// <value>
        /// The email address to which Extract should respond.
        /// </value>
        public string EmailAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the phone number which Extract may contact regarding the generated
        /// message.
        /// </summary>
        /// <value>
        /// The phone number which Extract may contact regarding the generated message.
        /// </value>
        public string Phone
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the reason this message is being generated.
        /// </summary>
        /// <value>
        /// The reason this message is being generated.
        /// </value>
        public string Reason
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets a message requesting a counter update or unlock from Extract.
        /// </summary>
        /// <returns></returns>
        public string GetRequestText()
        {
            try
            {
                var requestText = new StringBuilder();

                requestText.Append(GetFormattedLine("Database", _databaseName));
                requestText.Append(GetFormattedLine("ID", _databaseID));
                requestText.Append(GetFormattedLine("Organization", Organization));
                requestText.Append(GetFormattedLine("Email", EmailAddress));
                requestText.Append(GetFormattedLine("Phone", Phone));
                if (!string.IsNullOrWhiteSpace(Reason))
                {
                    requestText.AppendLine();
                    requestText.AppendLine(Reason);
                }
                requestText.AppendLine();

                requestText.AppendLine("Current counter values:");
                requestText.AppendLine(_countersDescription);

                // _countersDescription ends with newline, AppendLine adds another.
                requestText.Append(_licenseString);

                return requestText.ToString();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39074");
            }
        }

        /// <summary>
        /// Gets a message confirming a counter update or unlock for Extract.
        /// </summary>
        /// <returns></returns>
        public string GetConfirmationText()
        {
            try
            {
                var confirmationText = new StringBuilder();

                confirmationText.Append(GetFormattedLine("Database", _databaseName));
                confirmationText.Append(GetFormattedLine("ID", _databaseID));
                confirmationText.Append(GetFormattedLine("Organization", Organization));
                confirmationText.Append(GetFormattedLine("Email", EmailAddress));
                confirmationText.Append(GetFormattedLine("Phone", Phone));
                confirmationText.AppendLine();

                confirmationText.AppendLine("The following changes were successfully applied:");
                confirmationText.AppendLine(Reason);
                confirmationText.AppendLine();

                confirmationText.AppendLine("New counter values:");
                confirmationText.AppendLine(_countersDescription);

                // _countersDescription ends with newline, AppendLine adds another.
                confirmationText.Append(_licenseString);

                return confirmationText.ToString();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39075");
            }
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Gets a single line of text listing the <see paramref="parameterValue"/> for
        /// <see paramref="parameterName"/>, or an empty string if the parameter doesn't have a
        /// value.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="parameterValue">The value of the parameter.</param>
        /// <returns>A single line of text listing the <see paramref="parameterValue"/> for
        /// <see paramref="parameterName"/>, or an empty string if the parameter doesn't have a
        /// value.</returns>
        static string GetFormattedLine(string parameterName, string parameterValue)
        {
            return string.IsNullOrWhiteSpace(parameterValue)
                ? ""
                : string.Format(CultureInfo.CurrentCulture, "{0}: {1}\r\n",
                    parameterName, parameterValue);
        }

        #endregion Private Methods
    }
}
