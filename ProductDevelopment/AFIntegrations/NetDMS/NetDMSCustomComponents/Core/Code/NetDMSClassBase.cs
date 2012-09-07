using DexFlow.Client;
using DexFlow.Framework;
using Extract;
using Extract.Interop;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NetDMSCustomComponents
{
    /// <summary>
    /// The interface definition for the settings used by <see cref="NetDMSClassBase"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("D70A9865-1621-4494-84B2-21EB76B53854")]
    public interface INetDMSConnectionSettings
    {
        /// <summary>
        /// The NetDMS dispatcher server.
        /// </summary>
        string Server
        {
            get;
            set;
        }

        /// <summary>
        /// The NetDMS dispatcher port.
        /// </summary>
        int Port
        {
            get;
            set;
        }

        /// <summary>
        /// The NetDMS user name under which connections are to be established.
        /// </summary>
        string User
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the password for the <see cref="User"/>.
        /// </summary>
        /// <param name="password">The password.</param>
        void SetPassword(string password);

        /// <summary>
        /// Gets a value indicating whether the password for <see cref="User"/> has been specified.
        /// </summary>
        /// <value><see langword="true"/> if the password for <see cref="User"/> has been specified;
        /// otherwise, <see langword="false"/>.
        /// </value>
        bool HasPassword
        {
            get;
        }
    }

    /// <summary>
    /// A base class to use for implementing NetDMS related classes. This class primarily
    /// encapsulates NetDMS connection related code.
    /// </summary>
    [ComVisible(true)]
    [Guid("9AB57253-5DB8-4B74-B870-D50A02488045")]
    public class NetDMSClassBase : IDisposable
    {
        #region Constants

        /// <summary>
        /// Current object version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The encryption password used to encrypt the password before persisting it.
        /// </summary>
        static readonly string _PASSWORD_ENCRYPTION_PASSWORD = "3A4E6502-19AB-4D1B-A0C3-115534046FC9";

        /// <summary>
        /// A constant used to indicate that the last password so
        /// </summary>
        internal const string _USE_LAST_PASSWORD = "<<PASS>>";

        /// <summary>
        /// The sub-directories of the 32-bit Program Files directory to which NetDMS projects which
        /// contain assemblies this project references may be installed.
        /// </summary>
        static readonly string[] _NETDMS_INSTALL_DIRECTORIES = {
            @"VistaSG\WorkFlow Console",
            @"VistaSG\WorkFlow Server",
            @"VistaSG\WorkFlow Configuration Manager" };

        #endregion Constants

        #region Fields

        /// <summary>
        /// The NetDMS dispatcher server. 
        /// </summary>
        string _server;

        /// <summary>
        /// The NetDMS dispatcher port.
        /// </summary>
        int _port;

        /// <summary>
        /// The NetDMS user name under which connections are to be established.
        /// </summary>
        string _user;

        /// <summary>
        /// An encrypted version of the password for <see cref="_user"/>.
        /// </summary>
        string _encryptedPassword;

        /// <summary>
        /// The <see cref="ICustomer"/> associated with the current connection.
        /// </summary>
        ICustomer _customer;

        /// <summary>
        /// Registry settings used to persist the last used NetDMS connection info.
        /// </summary>
        RegistrySettings<Properties.Settings> _registry =
            new RegistrySettings<Properties.Settings>(@"Software\Extract Systems\NetDMS");

        /// <summary>
        /// A map of all project IDs to <see cref="IProject"/> instances for currently connected
        /// NetDMS dispatcher.
        /// </summary>
        Dictionary<long, IProject> _projects = new Dictionary<long, IProject>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// A static constructor for <see cref="NetDMSClassBase"/>. Used to enable resolving of
        /// NetDMS assemblies from one of several possible NetDMS install directories.
        /// </summary>
        // The FX Cop rule against static constructors assumes the static constructor is being used
        // to initialize static fields.
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static NetDMSClassBase()
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += HandleCurrentDomain_AssemblyResolve;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34891");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the <see cref="TaskClient"/> instance being used to connect to NetDMS.
        /// </summary>
        protected TaskClient TaskClient
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="SessionInfo"/> for the current NetDMS connection.
        /// </summary>
        protected SessionInfo Session
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether settings have been changed, but not saved.
        /// </summary>
        /// <value><see langword="true"/> if dirty; otherwise, <see langword="false"/>.</value>
        protected bool Dirty
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the NetDMS connection settings are configured.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the NetDMS connection settings are configured; otherwise,
        /// <see langword="false"/>.
        /// </value>
        protected bool IsConnectionConfigured
        {
            get
            {
                try
                {
                    return !string.IsNullOrWhiteSpace(Server) &&
                                   Port > 0 &&
                                   !string.IsNullOrWhiteSpace(User);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34903");
                }
            }
        }

        /// <summary>
        /// Gets or sets the NetDMS dispatcher server.
        /// </summary>
        /// <value>
        /// The NetDMS dispatcher server.
        /// </value>
        public string Server
        {
            get
            {
                return _server;
            }

            set
            {
                try
                {
                    if (value != _server)
                    {
                        _server = value;
                        Dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34904");
                }
            }
        }

        /// <summary>
        /// Gets or sets the NetDMS dispatcher port.
        /// </summary>
        /// <value>
        /// The NetDMS dispatcher port.
        /// </value>
        public int Port
        {
            get
            {
                return _port;
            }

            set
            {
                try
                {
                    if (value != _port)
                    {
                        _port = value;
                        Dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34905");
                }
            }
        }

        /// <summary>
        /// Gets or sets the NetDMS user name under which connections are to be established.
        /// </summary>
        /// <value>
        /// The the NetDMS user name under which connections are to be established.
        /// </value>
        public string User
        {
            get
            {
                return _user;
            }

            set
            {
                try
                {
                    if (value != _user)
                    {
                        _user = value;
                        Dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34906");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the password for <see cref="User"/> has been specified.
        /// </summary>
        /// <value><see langword="true"/> if the password for <see cref="User"/> has been specified;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool HasPassword
        {
            get
            {
                return !string.IsNullOrEmpty(_encryptedPassword);
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Sets the password for the <see cref="User"/>.
        /// </summary>
        /// <param name="password">The password.</param>
        public void SetPassword(string password)
        {
            try
            {
                SetPasswordPrivate(password);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34878");
            }
        }

        /// <summary>
        /// Connects to the appropriate project assuming that the grand-parent directory name is the
        /// project ISN.
        /// </summary>
        /// <param name="fileName">The filename for which the corresponding NetDMS project is
        /// needed.</param>
        protected void ConnectToCorrespondingProject(string fileName)
        {
            try
            {
                // The path will be formatted [RootPath]\[ProjectISN]\[ParcelISN]-[NodeISN]\[DocISN].tif
                long projectISN = Int64.Parse(
                    Path.GetFileName(
                        Path.GetDirectoryName(   // Project ISN
                            Path.GetDirectoryName(fileName))),  // Parcel ISN
                    CultureInfo.InvariantCulture);  

                if (Session.Project == null || Session.Project.ISN == projectISN)
                {
                    Session.Project = _projects[projectISN];
                }
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI34900",
                    "Unable to connect to corresponding NetDMS project", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="IDocument"/> from NetDMS that corresponds to
        /// <see paramref="fileName"/>.
        /// </summary>
        /// <param name="fileName">Name of the file for which the corresponding NetDMS IDocument is
        /// needed.</param>
        /// <returns>The <see cref="IDocument"/> from NetDMS.</returns>
        protected IDocument GetCorrespondingDocument(string fileName)
        {
            try
            {
                ConnectToCorrespondingProject(fileName);

                long documentISN = Int64.Parse(
                    Path.GetFileNameWithoutExtension(fileName), CultureInfo.InvariantCulture);
                return GetNetDMSObject<IDocument>(SystemTables.Document, documentISN);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI34901",
                    "Unable to obtain to corresponding NetDMS document", ex);
            }
        }

        /// <summary>
        /// Gets the NetDMS object associated with the specified NetDMS <see paramref="table"/> and
        /// <see paramref="ISN"/>
        /// </summary>
        /// <typeparam name="T">The object's <see cref="Type"/>.</typeparam>
        /// <param name="table">The NetDMS table to which the object belongs.</param>
        /// <param name="ISN">The object's ISN.</param>
        /// <returns>The requested object.</returns>
        // Not concerned with VB6 interoperability.
        [SuppressMessage("Microsoft.Interoperability", "CA1406:AvoidInt64ArgumentsForVB6Clients")]
        // Despite adding "ISN" to our dictionary, FXCop still doesn't like it.
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ISN")]
        protected T GetNetDMSObject<T>(SystemTables table, long ISN)
        {
            try
            {
                return (T)API.GetObject(TaskClient, Session.User, Session.Project,
                    Session.Project.GetTableMetaData(table), ISN);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34902");
            }
        }

        /// <summary>
        /// Loads the connection settings from the <see paramref="reader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="IStreamReader"/> from which the connection info is
        /// to be loaded.</param>
        protected void LoadConnectionSettings(IStreamReader reader)
        {
            try
            {
                int version = reader.ReadInt32();
                if (version > _CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI34894",
                        "Cannot load newer version of NetDMS object");
                    ee.AddDebugData("Current Version", _CURRENT_VERSION, false);
                    ee.AddDebugData("Version To Load", version, false);
                    throw ee;
                }

                Server = reader.ReadString();
                Port = reader.ReadInt32();
                User = reader.ReadString();
                _encryptedPassword = reader.ReadString();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34895");
            }
        }

        /// <summary>
        /// Saves the connection settings to the <see paramref="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="IStreamWriter"/> tp which the connection info is
        /// to be saved.</param>
        protected void SaveConnectionSettings(IStreamWriter writer)
        {
            try
            {
                writer.Write(_CURRENT_VERSION);
                writer.Write(Server);
                writer.Write(Port);
                writer.Write(User);
                writer.Write(_encryptedPassword);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34896");
            }
        }

        /// <summary>
        /// Copies the connection settings from <see paramref="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="NetDMSClassBase"/> from which settings are to be
        /// copied.</param>
        protected void CopyConnectionSettings(NetDMSClassBase source)
        {
            try
            {
                Server = source.Server;
                Port = source.Port;
                User = source.User;
                _encryptedPassword = source._encryptedPassword;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34897");
            }
        }

        /// <summary>
        /// Copies the connection password from the specified <see paramref="passwordSource"/>.
        /// </summary>
        /// <param name="passwordSource">The <see cref="NetDMSClassBase"/> from which the password
        /// is to be copied.</param>
        internal void CopyPassword(NetDMSClassBase passwordSource)
        {
            try
            {
                _encryptedPassword = passwordSource._encryptedPassword;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34893");
            }
        }

        /// <summary>
        /// Connects to the NetDMS dispatcher.
        /// </summary>
        internal void Connect()
        {
            try
            {
                // Connect to the server
                TaskClient = new TaskClient(_registry.Settings.ConnectionTimeout, false);
                TaskClient.Connect(Server, Port);

                // Login under the specified user
                Session = new SessionInfo();
                Session.User = API.LoginUser(TaskClient, User, GetPassword(), out _customer);

                // Generate a map of all the available projects.
                _projects.Clear();
                IObjectCollection projects = API.GetProjects(TaskClient, Session.User, _customer);
                foreach (IProject project in projects)
                {
                    _projects[project.ISN] = project;
                }

                // We have successfully connected; store these connection settings for ease of
                // configuring another NetDMSClassBase derived instance.
                _registry.Settings.LastServer = Server;
                _registry.Settings.LastPort = Port.ToString(CultureInfo.InvariantCulture);
                _registry.Settings.LastUser = User;
                _registry.Settings.LastPassword = _encryptedPassword;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34898");
            }
        }

        /// <summary>
        /// Disconnects from the NetDMS dispatcher.
        /// </summary>
        internal void Disconnect()
        {
            try
            {
                TaskClient.Disconnect();
                TaskClient.Dispose();
                TaskClient = null;

                Session = null;

                _projects.Clear();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34899");
            }
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="AppDomain.AssemblyResolve"/> event for the current
        /// <see cref="AppDomain"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="System.ResolveEventArgs"/> instance containing the
        /// event data.</param>
        /// <returns>The resolved <see cref="Assembly"/>.</returns>
        static Assembly HandleCurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                // Extract the name the fileName of the assembly.
                string fileName = args.Name.Split(',').FirstOrDefault() + ".dll";

                // Attempt to find an alternate path for the file in one of the directories NetDMS
                // will install into.
                foreach (string netDMSDirectory in _NETDMS_INSTALL_DIRECTORIES)
                {
                    string alternateFileName = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                        netDMSDirectory, fileName);
                    if (File.Exists(alternateFileName))
                    {
                        return Assembly.LoadFrom(alternateFileName);
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34892");
                return null;
            }
        }

        #endregion Event Handlers

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="NetDMSClassBase"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="NetDMSClassBase"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="NetDMSClassBase"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                if (TaskClient != null)
                {
                    TaskClient.Dispose();
                    TaskClient = null;
                }
            }

            // Dispose of ummanaged resources
        }

        #endregion IDisposable Members

        #region Private Members

        /// <summary>
        /// Sets the password. Refactored from <see cref="SetPassword"/> to produce better
        /// obfuscation.
        /// </summary>
        /// <param name="password">The password to encrypt.</param>
        void SetPasswordPrivate(string password)
        {
            string newEncryptedPassword = (password == _USE_LAST_PASSWORD)
                 ? _registry.Settings.LastPassword
                 : Crypto.EncryptStringAES(password, _PASSWORD_ENCRYPTION_PASSWORD);

            if (newEncryptedPassword != _encryptedPassword)
            {
                _encryptedPassword = newEncryptedPassword;
                Dirty = true;
            }
        }

        /// <summary>
        /// Gets the unencrypted password to use when logging into NetDMS.
        /// </summary>
        string GetPassword()
        {
            if (string.IsNullOrEmpty(_encryptedPassword))
            {
                return null;
            }
            else
            {
                return Crypto.DecryptStringAES(_encryptedPassword, _PASSWORD_ENCRYPTION_PASSWORD);
            }
        }

        #endregion Private Members
    }
}
