using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.SourceControl
{
    /// <summary>
    /// Represents the settings used to login to a source control database.
    /// </summary>
    public class LogOnSettings
    {
        #region LogOnSettings Fields

        readonly string _server;
        readonly string _userName;
        readonly string _password;

        #endregion LogOnSettings Fields

        #region LogOnSettings Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LogOnSettings"/> class.
        /// </summary>
        public LogOnSettings() : this(null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogOnSettings"/> class.
        /// </summary>
        public LogOnSettings(string server) : this (server, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogOnSettings"/> class.
        /// </summary>
        public LogOnSettings(string server, string userName) : this(server, userName, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogOnSettings"/> class.
        /// </summary>
        public LogOnSettings(string server, string userName, string password)
        {
            _server = server;
            _userName = userName;
            _password = password;
        }

        #endregion LogOnSettings Constructors

        #region LogOnSettings Properties

        /// <summary>
        /// Gets the url to the source control server.
        /// </summary>
        /// <returns>The url to the source control server.</returns>
        public string Server
        {
            get
            {
                return _server;
            }
        }

        /// <summary>
        /// Gets the name of the login user.
        /// </summary>
        /// <returns>The name of the login user.</returns>
        public string UserName
        {
            get
            {
                return _userName;
            }
        }

        /// <summary>
        /// Gets the password to use to login.
        /// </summary>
        /// <returns>The password to use to login.</returns>
        public string Password
        {
            get
            {
                return _password;
            }
        }

        #endregion LogOnSettings Properties
    }
}
