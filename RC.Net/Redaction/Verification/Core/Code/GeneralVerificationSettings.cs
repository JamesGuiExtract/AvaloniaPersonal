using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents general settings for the <see cref="VerificationTask"/>
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
        /// <see langword="true"/> if all redactions require a redaction type; 
        /// <see langword="false"/> if redactions can be saved without a redaction type.
        /// </summary>
        readonly bool _requireTypes;

        /// <summary>
        /// <see langword="true"/> if all redactions require an exemption code;
        /// <see langword="false"/> if redactions can be saved without an exemption code.
        /// </summary>
        readonly bool _requireExemptions;

        #endregion GeneralVerificationSettings Fields

        #region GeneralVerificationSettings Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralVerificationSettings"/> class 
        /// with default settings.
        /// </summary>
        public GeneralVerificationSettings() : this(true, true, false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralVerificationSettings"/> class.
        /// </summary>
        public GeneralVerificationSettings(bool verifyAllPages, bool requireTypes, 
            bool requireExemptions)
        {
            _verifyAllPages = verifyAllPages;
            _requireTypes = requireTypes;
            _requireExemptions = requireExemptions;
        }

        #endregion GeneralVerificationSettings Constructors

        #region GeneralVerificationSettings Properties

        /// <summary>
        /// Gets whether what all pages should be verified.
        /// </summary>
        /// <returns><see langword="true"/> if all pages should be verified;
        /// <see langword="false"/> if only pages that contain redactions shoudl be verfied.
        /// </returns>
        public bool VerifyAllPages
        {
            get
            {
                return _verifyAllPages;
            }
        }

        /// <summary>
        /// Gets whether whether the user is required to specify a type for all redactions.
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

        #endregion GeneralVerificationSettings Properties
    }
}
