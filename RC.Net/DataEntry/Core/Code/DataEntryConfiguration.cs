using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.DataEntry
{
    /// <summary>
    /// Represents a data entry configuration used to display document data.
    /// </summary>
    public class DataEntryConfiguration : IDisposable
    {
        /// <summary>
        /// The configuration settings specified via config file.
        /// </summary>
        ConfigSettings<Extract.DataEntry.Properties.Settings> _config;

        /// <summary>
        /// The <see cref="DataEntryControlHost"/> instance associated with the configuration.
        /// </summary>
        DataEntryControlHost _dataEntryControlHost;

        /// <summary>
        /// Initializes a new <see cref="DataEntryConfiguration"/> instance.
        /// </summary>
        /// <param name="config">The configuration settings specified via config file.</param>
        /// <param name="dataEntryControlHost">The <see cref="DataEntryControlHost"/> instance
        /// associated with the configuration.</param>
        public DataEntryConfiguration(
            ConfigSettings<Extract.DataEntry.Properties.Settings> config,
            DataEntryControlHost dataEntryControlHost)
        {
            _config = config;
            _dataEntryControlHost = dataEntryControlHost;
        }

        /// <summary>
        /// The configuration settings specified via config file.
        /// </summary>
        public ConfigSettings<Extract.DataEntry.Properties.Settings> Config
        {
            get
            {
                return _config;
            }
        }

        /// <summary>
        /// The <see cref="DataEntryControlHost"/> instance associated with the configuration.
        /// </summary>
        public DataEntryControlHost DataEntryControlHost
        {
            get
            {
                return _dataEntryControlHost;
            }
        }

        /// <overloads>Releases resources used by the <see cref="DataEntryConfiguration"/>.
        /// </overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="DataEntryConfiguration"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="DataEntryConfiguration"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                if (_dataEntryControlHost != null)
                {
                    _dataEntryControlHost.Dispose();
                    _dataEntryControlHost = null;
                }
            }

            // Dispose of unmanaged resources
        }
    }
}
