using Extract;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management;
using System.Reflection;
using System.Text;

namespace Extract.Utilities
{
    /// <summary>
    /// Enumeration for printer status information.
    /// See http://msdn.microsoft.com/en-us/library/aa394363%28VS.85%29.aspx
    /// in the section about PrinterStatus for the source of the values
    /// in this enum.
    /// </summary>
    public enum PrinterStatus
    {
        /// <summary>
        /// There is no status information available.
        /// </summary>
        None = 0,

        /// <summary>
        /// Some other status.
        /// </summary>
        Other = 1,

        /// <summary>
        /// The current status is unknown.
        /// </summary>
        Unknown = 2,

        /// <summary>
        /// The printer is idle.
        /// </summary>
        Idle = 3,

        /// <summary>
        /// The printer is currently printing.
        /// </summary>
        Printing = 4,

        /// <summary>
        /// The printer is warming up.
        /// </summary>
        WarmingUp = 5,

        /// <summary>
        /// The printer is stopped.
        /// </summary>
        StoppedPrinting = 6,

        /// <summary>
        /// The printer is offline.
        /// </summary>
        Offline = 7
    }

    /// <summary>
    /// Class containing information about a printer installed on the current system.
    /// </summary>
    public sealed class WindowsPrinterInformation
    {
        #region Fields

        /// <summary>
        /// The name of the object used in license validation calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(WindowsPrinterInformation).ToString();

        // For additional valid fields of printer information see
        // http://msdn.microsoft.com/en-us/library/aa394363%28VS.85%29.aspx

        /// <summary>
        /// The name of the windows printer driver.
        /// </summary>
        string _driverName;

        /// <summary>
        /// The physical location of the printer.
        /// </summary>
        string _location;

        /// <summary>
        /// The name of the printer.
        /// </summary>
        string _name;

        /// <summary>
        /// Whether the printer is a network printer or not.
        /// </summary>
        bool _network;

        /// <summary>
        /// The port that is used to transmit data to a printer.
        /// </summary>
        string _portName;

        /// <summary>
        /// The name of the server that controls the printer. If this string
        /// is <see langword="null"/> or <see cref="String.Empty"/> then
        /// the printer is controlled locally.
        /// </summary>
        string _serverName;

        /// <summary>
        /// Whether or not the printer is avaiable as a network resource.
        /// </summary>
        bool _shared;

        /// <summary>
        /// The current status information for the printer.
        /// </summary>
        PrinterStatus _status;

        /// <summary>
        /// Whether jobs can be queued to the printer when it is offline.
        /// </summary>
        bool _workOffline;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsPrinterInformation"/> class.
        /// </summary>
        WindowsPrinterInformation()
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the windows driver name for the printer.
        /// </summary>
        /// <value>Sets the name of the windows driver for the printer.</value>
        public string DriverName
        {
            get
            {
                return _driverName;
            }
            private set
            {
                _driverName = value;
            }
        }

        /// <summary>
        /// Gets the physical location of the printer.
        /// </summary>
        /// <value>Sets the physical location of the printer.</value>
        public string Location
        {
            get
            {
                return _location;
            }
            private set
            {
                _location = value;
            }
        }

        /// <summary>
        /// Gets the name of the printer.
        /// </summary>
        /// <value>Sets the name of the printer.</value>
        public string Name
        {
            get
            {
                return _name;
            }
            private set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Gets whether this printer is a network printer.
        /// </summary>
        /// <value>Sets whether this printer is a network printer or not.</value>
        public bool Network
        {
            get
            {
                return _network;
            }
            private set
            {
                _network = value;
            }
        }

        /// <summary>
        /// Gets the name of the port that the printer is attached to.
        /// </summary>
        /// <value>Sets the name of the port that the printers is attached to.</value>
        public string PortName
        {
            get
            {
                return _portName;
            }
            private set
            {
                _portName = value;
            }
        }

        /// <summary>
        /// Gets the name of the server that controls this printer. If
        /// this is <see langword="null"/> or <see cref="String.Empty"/> then
        /// the printer is a local printer.
        /// </summary>
        /// <value>Sets the name of the server that controls the printer.</value>
        public string ServerName
        {
            get
            {
                return _serverName;
            }
            private set
            {
                _serverName = value;
            }
        }

        /// <summary>
        /// Gets whether this printer is shared over a network.
        /// </summary>
        /// <value>Sets whether this printer is shared over a network.</value>
        public bool Shared
        {
            get
            {
                return _shared;
            }
            private set
            {
                _shared = value;
            }
        }

        /// <summary>
        /// Gets the status of the printer.
        /// </summary>
        /// <value>Sets the status of the printer.</value>
        public PrinterStatus Status
        {
            get
            {
                return _status;
            }
            private set
            {
                _status = value;
            }
        }

        /// <summary>
        /// Gets whether jobs can be queued to the printer when it is offline.
        /// </summary>
        /// <value>Sets whether jobs can be queued to the printer when it is offline.</value>
        public bool WorkOffline
        {
            get
            {
                return _workOffline;
            }
            private set
            {
                _workOffline = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Returns a string containing the printers name formatted with
        /// the current port information.
        /// <example>For a local CutePDF printer the string may be returned
        /// as 'CutePDF on CWP2:'</example>
        /// </summary>
        /// <returns>The printer name formatted with port information.</returns>
        public string PrinterNameWithPortInformation()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                if (Network)
                {
                    sb.Append("\\\\");
                    sb.Append(ServerName);
                    sb.Append("\\");
                }
                sb.Append(Name);
                sb.Append(" on ");
                sb.Append(PortName);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30270", ex);
            }
        }

        #endregion Methods

        #region Static Methods

        /// <summary>
        /// Gets the <see cref="WindowsPrinterInformation"/> for the specified printer name
        /// or <see langword="null"/> if the printer is not found.
        /// </summary>
        /// <param name="printerName">The name of the printer to get the information for.</param>
        /// <returns>The information for the specified printer or <see langword="null"/>
        /// if the printer is not found.</returns>
        public static WindowsPrinterInformation GetPrinterInformation(string printerName)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30267", _OBJECT_NAME);

                List<WindowsPrinterInformation> printers = new List<WindowsPrinterInformation>(
                    GetList());

                WindowsPrinterInformation result = null;
                foreach (WindowsPrinterInformation printer in printers)
                {
                    if (printer.Name.Equals(printerName, StringComparison.OrdinalIgnoreCase))
                    {
                        result = printer;
                        break;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30268", ex);
            }
        }

        /// <summary>
        /// Gets a list containing all of the currently installed printers
        /// on the system.
        /// </summary>
        /// <returns>A collection containing information for all of the
        /// currently installed printers on the system.</returns>
        static ReadOnlyCollection<WindowsPrinterInformation> GetList()
        {
            try
            {
                string query = "Select DriverName, Location, Name, Network, PortName, "
                    + "ServerName, Shared, Status, WorkOffline From Win32_Printer";

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                using (ManagementObjectCollection results = searcher.Get())
                {
                    List<WindowsPrinterInformation> list =
                        new List<WindowsPrinterInformation>(results.Count);

                    foreach (ManagementObject obj in results)
                    {
                        try
                        {
                            // Get the windows printer information for the object
                            // NOTE: It is safe to call Convert.ToBoolean for a null
                            // reference, the conversion will return false in this case.
                            // It is also safe to call Convert.ToUInt16 for a null
                            // reference, the conversion will return 0 in this case.
                            WindowsPrinterInformation entry = new WindowsPrinterInformation();
                            entry.DriverName = StringMethods.ConvertObjectToString(obj["DriverName"]);
                            entry.Location = StringMethods.ConvertObjectToString(obj["Location"]);
                            entry.Name = StringMethods.ConvertObjectToString(obj["Name"]);
                            entry.Network = Convert.ToBoolean(obj["Network"],
                                CultureInfo.InvariantCulture);
                            entry.PortName = StringMethods.ConvertObjectToString(obj["PortName"]);
                            entry.ServerName = StringMethods.ConvertObjectToString(obj["ServerName"]);
                            entry.Shared = Convert.ToBoolean(obj["Shared"],
                                CultureInfo.InvariantCulture);
                            entry.Status = (PrinterStatus)Enum.ToObject(typeof(PrinterStatus),
                                Convert.ToUInt16(obj["PrinterStatus"],
                                CultureInfo.InvariantCulture));
                            entry.WorkOffline = Convert.ToBoolean(obj["WorkOffline"],
                                CultureInfo.InvariantCulture);

                            list.Add(entry);
                        }
                        finally
                        {
                            obj.Dispose();
                        }
                    }

                    return list.AsReadOnly();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30269", ex);
            }
        }

        #endregion Static Methods
    }
}
