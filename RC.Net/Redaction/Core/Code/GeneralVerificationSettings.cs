using Extract.Interop;
using System;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents general settings for verification.
    /// </summary>
    public class GeneralVerificationSettings
    {
        #region GeneralVerificationSettings Fields

        /// <summary>
        /// <see langword="true"/> if all pages should be verified. <see langword="false"/> if 
        /// only pages that contain redactions should be verified.
        /// </summary>
        readonly bool _verifyAllPages;

        /// <summary>
        /// <see langword="true"/> if all suggested redactions and clues must be visited;
        /// <see langword="false"/> otherwise. 
        /// </summary>
        readonly bool _verifyAllItems;

        /// <summary>
        /// <see langword="true"/> if only visiting pages with full page clues;
        /// <see langword="false"/> otherwise. 
        /// </summary>
        readonly bool _verifyFullPageCluesOnly;

        /// <summary>
        /// <see langword="true"/> if all redactions require a redaction type; 
        /// <see langword="false"/> if redactions can be saved without a redaction type.
        /// </summary>
        readonly bool _requireTypes;

        /// <summary>
        /// <see langword="true"/> if all redactions require an exemption code;
        /// <see langword="false"/> if redactions can be saved without an exemption code.
        /// </summary>
        readonly bool _requireExemptions;

        /// <summary>
        /// <see langword="true"/> if all forward/backward navigation controls are permitted to move
        /// to the previous/next document; <see langword="false"/> if only document navigation
        /// controls are permitted to change documents.
        /// </summary>
        readonly bool _allowSeamlessNavigation;

        /// <summary>
        /// <see langword="true"/> if the user should be prompted to save documents that have not
        /// yet been committed when navigating away; <see langword="false"/> if the document should
        /// be saved (and committed if appropriate) without prompting.
        /// </summary>
        readonly bool _promptForSaveUntilCommit;

        #endregion GeneralVerificationSettings Fields

        #region GeneralVerificationSettings Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralVerificationSettings"/> class 
        /// with default settings.
        /// </summary>
        public GeneralVerificationSettings() : this(true, true, false, true, false, true, false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralVerificationSettings"/> class.
        /// </summary>
        public GeneralVerificationSettings(bool verifyAllPages, bool verifyAllItems,
            bool verifyFullPageCluesOnly, bool requireTypes, bool requireExemptions, 
            bool allowSeamlessNavigation, bool promptForSaveUntilCommit)
        {
            _verifyAllPages = verifyAllPages;
            _verifyAllItems = verifyAllItems;
            _verifyFullPageCluesOnly = verifyFullPageCluesOnly;
            _requireTypes = requireTypes;
            _requireExemptions = requireExemptions;
            _allowSeamlessNavigation = allowSeamlessNavigation;
            _promptForSaveUntilCommit = promptForSaveUntilCommit;
        }

        #endregion GeneralVerificationSettings Constructors

        #region GeneralVerificationSettings Properties

        /// <summary>
        /// Gets whether what all pages should be verified.
        /// </summary>
        /// <returns><see langword="true"/> if all pages should be verified;
        /// <see langword="false"/> if only pages that contain redactions should be verified.
        /// </returns>
        public bool VerifyAllPages
        {
            get
            {
                return _verifyAllPages;
            }
        }

        /// <summary>
        /// Gets whether what all redactions and clues must be visited.
        /// </summary>
        /// <returns><see langword="true"/> if all suggested redactions and clues must be visited;
        /// <see langword="false"/> otherwise. 
        /// </returns>
        public bool VerifyAllItems
        {
            get
            {
                return _verifyAllItems;
            }
        }

        /// <summary>
        /// Gets whether only full page clues should be visited
        /// </summary>
        /// <returns><see langword="true"/> if only visiting pages with full page clues;
        /// <see langword="false"/> otherwise. 
        /// </returns>
        public bool VerifyFullPageCluesOnly
        {
            get
            {
                return _verifyFullPageCluesOnly;
            }
        }

        /// <summary>
        /// Gets whether the user is required to specify a type for all redactions.
        /// </summary>
        /// <returns><see langword="true"/> if all redactions require a redaction type;
        /// <see langword="false"/> if redactions can be saved without a redaction type.</returns>
        public bool RequireTypes
        {
            get
            {
                return _requireTypes;
            }
        }

        /// <summary>
        /// Gets whether the user is required to specify an exemption code for all redactions.
        /// </summary>
        /// <returns><see langword="true"/> if all redactions require an exemption code;
        /// <see langword="false"/> if redactions can be saved without an exemption code.</returns>
        public bool RequireExemptions
        {
            get
            {
                return _requireExemptions;
            }
        }

        /// <summary>
        /// Gets a value indicating whether navigation controls can move seamlessly between
        /// documents.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if all forward/backward navigation controls are permitted to move
        /// to the previous/next document; <see langword="false"/> if only document navigation
        /// controls are permitted to change documents.
        /// </value>
        public bool AllowSeamlessNavigation
        {
            get
            {
                return _allowSeamlessNavigation;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to prompt to save documents that haven't been committed
        /// before navigating away.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the user should be prompted to save documents that have not
        /// yet been committed when navigating away; <see langword="false"/> if the document should
        /// be saved (and committed if appropriate) without prompting.
        /// </value>
        public bool PromptForSaveUntilCommit
        {
            get
            {
                return _promptForSaveUntilCommit;
            }
        }

        #endregion GeneralVerificationSettings Properties

        #region GeneralVerificationSettings Methods

        /// <summary>
        /// Creates a <see cref="GeneralVerificationSettings"/> from the specified 
        /// <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to create the 
        /// <see cref="GeneralVerificationSettings"/>.</param>
        /// <returns>A <see cref="GeneralVerificationSettings"/> created from the specified 
        /// <see cref="IStreamReader"/>.</returns>
        public static GeneralVerificationSettings ReadFrom(IStreamReader reader)
        {
            try
            {
                // The version number used here is the VerificationTask's version number.
                bool verifyAllPages = reader.ReadBoolean();
                bool verifyAllItems = (reader.Version < 8) ? true : reader.ReadBoolean();
                bool requireTypes = reader.ReadBoolean();
                bool requireExemptions = reader.ReadBoolean();
                bool allowSeamlessNavigation = (reader.Version < 8) ? true : reader.ReadBoolean();
                bool promptForSaveUntilCommit = (reader.Version < 8) ? false : reader.ReadBoolean();
                bool verifyFullPageCluesOnly = (reader.Version < 12) ? false : reader.ReadBoolean();

                return new GeneralVerificationSettings(verifyAllPages, verifyAllItems, verifyFullPageCluesOnly, requireTypes,
                    requireExemptions, allowSeamlessNavigation, promptForSaveUntilCommit);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26516",
                    "Unable to read general verification settings.", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="GeneralVerificationSettings"/> to the specified 
        /// <see cref="IStreamWriter"/>.
        /// </summary>
        /// <param name="writer">The writer into which the 
        /// <see cref="GeneralVerificationSettings"/> will be written.</param>
        public void WriteTo(IStreamWriter writer)
        {
            try
            {
                writer.Write(_verifyAllPages);
                writer.Write(_verifyAllItems);
                writer.Write(_requireTypes);
                writer.Write(_requireExemptions);
                writer.Write(_allowSeamlessNavigation);
                writer.Write(_promptForSaveUntilCommit);
                writer.Write(_verifyFullPageCluesOnly);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26517",
                    "Unable to write general verification settings.", ex);
            }
        }

        #endregion GeneralVerificationSettings Methods
    }
}
