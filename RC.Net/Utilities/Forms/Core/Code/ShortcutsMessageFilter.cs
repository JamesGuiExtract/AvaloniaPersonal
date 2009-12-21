using System;
using System.Security.Permissions;
using System.Windows.Forms;
using Extract.Licensing;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a delegate that determines whether shortcuts are enabled.
    /// </summary>
    public delegate bool ShortcutsEnabled();

    /// <summary>
    /// Represents a filter that routes shortcut key messages to a specific control.
    /// </summary>
    public sealed class ShortcutsMessageFilter : IMessageFilter, IDisposable
    {
        #region Constants

        /// <summary>
        /// Key down windows message.
        /// </summary>
        const int _WM_KEYDOWN = 0x100;

        /// <summary>
        /// System key down windows message.
        /// </summary>
        const int _WM_SYSKEYDOWN = 0x104;

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

        /// <summary>
        /// <see langword="true"/> if the <see cref="ShortcutsMessageFilter"/> has been disposed;
        /// <see langword="false"/> otherwise. Defaults to <see langword="true"/> so that if 
        /// the <see cref="ShortcutsMessageFilter"/> isn't fully constructed and hasn't been added 
        /// to the application's message filter, no attempt is made to remove it in the finalizer.
        /// </summary>
        bool _disposed = true;

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
            Control target)
        {
            ExtractException.Assert("ELI28895", "Missing shortcuts manager.", manager != null);
            ExtractException.Assert("ELI28896", "Missing target control.", target != null);

            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28897",
                _OBJECT_NAME);

            _enabled = enabled;
            _manager = manager;
            _target = target;

            Application.AddMessageFilter(this);

            _disposed = false;
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

        #region IMessageFilter Members

        /// <summary>
        /// Filters out a message before it is dispatched.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> to filter the message and stop it from being dispatched; 
        /// <see langword="false"/> to allow the message to continue to the next filter or control.
        /// </returns>
        /// <param name="m">The message to be dispatched. You cannot modify this message.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public bool PreFilterMessage(ref Message m)
        {
            try
            {
                if (m.Msg == _WM_KEYDOWN || m.Msg == _WM_SYSKEYDOWN)
                {
                    if (_target.Handle != m.HWnd && _target.CanFocus && ShortcutsEnabled)
                    {
                        Keys key = ((Keys)((int)((long)m.WParam))) | Control.ModifierKeys;
                        if (_manager[key] != null)
                        {
                            NativeMethods.BeginSendMessageToHandle(m, _target.Handle);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI28894", ex);
            }

            return false;
        }

        #endregion IMessageFilter Members

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="ShortcutsMessageFilter"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ShortcutsMessageFilter"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ShortcutsMessageFilter"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>		
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (!_disposed)
                {
                    Application.RemoveMessageFilter(this);
                    _disposed = true;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}