using Microsoft.Win32;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Manages Extract imaging specific registry settings
    /// </summary>
    internal static class RegistryManager
    {
        #region Constants

        #region SubKeys

        /// <summary>
        /// The sub key for Extract imaging keys.
        /// </summary>
        const string _EXTRACT_IMAGING_SUB_KEY = @"Software\Extract Systems\Imaging";

        #endregion SubKeys

        #region Keys

        /// <summary>
        /// The key for logging image-related file locking.
        /// </summary>
        const string _LOG_LOCKING_KEY = "Log image locking";

        /// <summary>
        /// The key for image-related file locking.
        /// </summary>
        const string _LOCK_IMAGE_FILES_KEY = "Lock image files";

        #endregion Keys

        #endregion Constants

        #region Fields

        /// <summary>
        /// The current user registry sub key for Extract imaging keys.
        /// </summary>     
        static readonly RegistryKey _userExtractImaging =
            Registry.CurrentUser.CreateSubKey(_EXTRACT_IMAGING_SUB_KEY);

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets whether to log image-related file locking.
        /// </summary>
        /// <value>Whether to log image-related file locking.</value>
        public static bool LogFileLocking
        {
            get
            {
                int? registryValue = _userExtractImaging.GetValue(_LOG_LOCKING_KEY) as int?;
                if (registryValue == null)
                {
                    _userExtractImaging.SetValue(_LOG_LOCKING_KEY, 0, RegistryValueKind.DWord);
                }

                return registryValue == 1;
            }
        }

        /// <summary>
        /// Gets whether to lock image files when reading.
        /// </summary>
        /// <value>Whether to lock image file when reading.</value>
        public static bool LockFiles
        {
            get
            {
                int? registryValue = _userExtractImaging.GetValue(_LOCK_IMAGE_FILES_KEY) as int?;
                if (registryValue == null)
                {
                    _userExtractImaging.SetValue(_LOCK_IMAGE_FILES_KEY, 1, RegistryValueKind.DWord);
                    registryValue = 1;
                }

                return registryValue == 1;
            }
        }

        #endregion Properties
    }
}
