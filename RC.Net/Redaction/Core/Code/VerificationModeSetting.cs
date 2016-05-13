using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extract.Redaction
{
    /// <summary>
    /// The set of possible valid modes
    /// </summary>
    public enum VerificationMode
    {
        /// <summary>
        /// Verification mode is enabled. This is the default.
        /// </summary>
        Verify,

        /// <summary>
        /// QA mode, preserve view status. Allows QA to review visited pages and sensitive items.
        /// </summary>
        QAModePreserveViewStatus,

        /// <summary>
        /// QA mode, reset view status. Resets visited items to a fresh and unvisited state.
        /// </summary>
        QAModeResetViewStatus
    }


    /// <summary>
    /// The verification mode setting - one of three possible values (see Mode enumeration).
    /// </summary>
    public class VerificationModeSetting
    {

        #region Fields

        VerificationMode _mode;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the verification mode
        /// </summary>
        /// <value>
        /// The verification mode.
        /// </value>
        public VerificationMode VerificationMode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Default CTOR - Initializes a new instance of the <see cref="VerificationModeSetting"/> class.
        /// </summary>
        public VerificationModeSetting()
        {
            _mode = VerificationMode.Verify;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationModeSetting"/> class.
        /// </summary>
        /// <param name="mode">The mode.</param>
        public VerificationModeSetting(VerificationMode mode)
        {
            _mode = mode;
        }

        #endregion Constructors
    }
}
