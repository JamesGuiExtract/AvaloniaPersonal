using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace SourceControl
{
    /// <summary>
    /// Manages registry settings for the Source Control Add-In.
    /// </summary>
    public static class RegistryManager
    {
        #region RegistryManager Constants

        /// <summary>
        /// The sub key for the Source Control Add-In.
        /// </summary>
        static readonly string _SOURCE_CONTROL_SUB_KEY =
            @"Software\Extract Systems\Visual Studio Add Ins\Source Control";

        /// <summary>
        /// The key for the date and time of the last get message.
        /// </summary>
        static readonly string _LAST_GET_KEY = "Last get";

        /// <summary>
        /// The key for the list of the recipients of get messages.
        /// </summary>
        static readonly string _GET_RECIPIENTS_KEY = "Get recipients";

        /// <summary>
        /// The key for the engineering root
        /// </summary>
        static readonly string _ENGINEERING_ROOT_KEY = "Engineering root";

        /// <summary>
        /// The repository root of the engineering tree.
        /// </summary>
        static readonly string _ENGINEERING_ROOT_DEFAULT = "$/Engineering";

        /// <summary>
        /// The default for the list of the recipients of get messages.
        /// </summary>
        static readonly string[] _GET_RECIPIENTS_DEFAULT = 
        {
            "Nathan Figueroa <nathan_figueroa@extractsystems.com>",
            "Arvind Ganesan <ag@extractsystems.com>", 
            "Steve Kurth <steve_kurth@extractsystems.com>", 
            "Wayne Lenius <wayne_lenius@extractsystems.com>", 
            "William Parr <william@extractsystems.com>", 
            "Jeff Shergalis <jeff_shergalis@extractsystems.com>"
        };

        #endregion RegistryManager Constants

        #region RegistryManager Fields

        /// <summary>
        /// The current user registry sub key for Source Control Add In keys.
        /// </summary>     
        static RegistryKey _subKey;

        /// <summary>
        /// The default for the date and time of the last get message.
        /// </summary>
        static DateTime _lastGetDefault = GetLastGetMessageTime() ?? DateTime.Now;

        #endregion RegistryManager Fields

        #region RegistryManager Properties

        /// <summary>
        /// Gets the current user registry sub key for Source Control Add In keys.
        /// </summary>
        /// <returns>The current user registry sub key for Source Control Add In keys.</returns>
        static RegistryKey SubKey
        {
            get
            {
                if (_subKey == null)
                {
                    _subKey = Registry.CurrentUser.CreateSubKey(_SOURCE_CONTROL_SUB_KEY);
                }

                return _subKey;
            }
        }

        /// <summary>
        /// Gets or sets the date and time of the last get message.
        /// </summary>
        /// <value>The date and time of the last get message.</value>
        /// <returns>The date and time of the last get message.</returns>
        public static DateTime LastGetMessageTime
        {
            get
            {
                return GetLastGetMessageTime() ?? _lastGetDefault;
            }
            set
            {
                SubKey.SetValue(_LAST_GET_KEY, value.ToString(), RegistryValueKind.String);
            }
        }

        /// <summary>
        /// Gets the recipients for get messages.
        /// </summary>
        /// <returns>The recipients for get messages.</returns>
        public static string[] GetMessageRecipients
        {
            get
            {
                string value = SubKey.GetValue(_GET_RECIPIENTS_KEY, null) as string;

                string[] recipients;
                if (value == null)
                {
                    // The registry key is not specified or is invalid. Use and set the default.
                    recipients = _GET_RECIPIENTS_DEFAULT;
                    SubKey.SetValue(_GET_RECIPIENTS_KEY, string.Join(",", recipients), 
                        RegistryValueKind.String);
                }
                else
                {
                    recipients = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                }

                return recipients;
            }
        }

        /// <summary>
        /// Gets the root of the engineering tree (used for creating get messages)
        /// </summary>
        /// <returns>The root of the engineering tree.</returns>
        public static string EngineeringRoot
        {
            get
            {
                // Get the root from the registry
                string value = SubKey.GetValue(_ENGINEERING_ROOT_KEY, null) as string;
                if (value == null)
                {
                    // If the value didn't exist, create it with default value
                    SubKey.SetValue(_ENGINEERING_ROOT_KEY, _ENGINEERING_ROOT_DEFAULT,
                        RegistryValueKind.String);
                }

                return value ?? _ENGINEERING_ROOT_DEFAULT;
            }
        }

        #endregion RegistryManager Properties

        #region RegistryManager Methods

        /// <summary>
        /// Gets the last time the get message was sent.
        /// </summary>
        /// <returns>The last time the get message was sent, or <see langword="null"/> if 
        /// the registry key does not exist.</returns>
        static DateTime? GetLastGetMessageTime()
        {
            // Get the value from the registry
            string value = SubKey.GetValue(_LAST_GET_KEY, null) as string;

            // Attempt to convert the value to a DateTime object
            DateTime lastGet;
            bool isValid = DateTime.TryParse(value, out lastGet);

            // Return the valid DateTime object or else null
            if (isValid)
            {
                _lastGetDefault = lastGet;
                return lastGet;
            }

            return null;
        }

        #endregion RegistryManager Methods
    }
}
