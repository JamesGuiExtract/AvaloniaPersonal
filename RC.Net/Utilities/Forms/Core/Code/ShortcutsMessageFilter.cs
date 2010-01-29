using Extract.Licensing;
using System;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a delegate that determines whether shortcuts are enabled.
    /// </summary>
    public delegate bool ShortcutsEnabled();

    /// <summary>
    /// Represents a filter that routes shortcut key messages to a specific control.
    /// </summary>
    public sealed class ShortcutsMessageFilter : MessageFilterBase
    {
        #region Constants

        /// <summary>
        /// The name of the <see cref="ShortcutsMessageFilter"/> class. Used for licensing.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ShortcutsMessageFilter).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Determines if shortcuts are enabled.
        /// </summary>
        readonly ShortcutsEnabled _enabled;

        /// <summary>
        /// Manages shortcut keys.
        /// </summary>
        readonly ShortcutsManager _manager;

        /// <summary>
        /// The control to which shortcut keys should be redirected.
        /// </summary>
        readonly Control _target;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortcutsMessageFilter"/> class.
        /// </summary>
        /// <param name="enabled">A delegate that determines whether shortcuts are enabled. 
        /// <see langword="null"/> if shortcuts are always enabled.</param>
        /// <param name="manager">Shortcuts manager used to handle shortcut keys.</param>
        /// <param name="target">The control that will handle keyboard shortcuts.</param>
        public ShortcutsMessageFilter(ShortcutsEnabled enabled, ShortcutsManager manager,
            Control target) : base(target)
        {
            ExtractException.Assert("ELI28895", "Missing shortcuts manager.", manager != null);
            ExtractException.Assert("ELI28896", "Missing target control.", target != null);

            // Validate the license
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28897",
                _OBJECT_NAME);

            _enabled = enabled;
            _manager = manager;
            _target = Controls[0];
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets whether shortcuts are enabled.
        /// </summary>
        /// <value><see langword="true"/> if shortcuts are enabled;
        /// <see langword="false"/> if shortcuts are disabled.</value>
        bool ShortcutsEnabled
        {
            get
            {
                return _enabled == null ? true : _enabled();
            }
        }

        #endregion Properties

        /// <summary>
        /// Handles the message in the <see cref="MessageFilterBase.PreFilterMessage"/> function.
        /// </summary>
        /// <param name="m">The message to handle.</param>
        /// <returns><see langword="true"/> if the message has been handled
        /// and <see langword="false"/> if it has not been handled.</returns>
        protected override bool HandleMessage(Message m)
        {
            try
            {
                // Redirect the message if:
                // 1) It is a key down message
                // 2) The target is not already receiving the message
                // 3) The target can receive input focus
                // 4) The target does not already contain input focus
                // 5) Shortcuts are enabled
                // 6) This shortcut key has a handler
                if (m.Msg == WindowsMessage.KeyDown || m.Msg == WindowsMessage.SystemKeyDown)
                {
                    if (_target.Handle != m.HWnd && _target.CanFocus && !_target.ContainsFocus && 
                        ShortcutsEnabled)
                    {
                        Keys key = ((Keys)((int)((long)m.WParam))) | Control.ModifierKeys;
                        if (_manager[key] != null)
                        {
                            NativeMethods.BeginSendMessageToHandle(m, _target.Handle);
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28894", ex);
            }
        }
    }
}