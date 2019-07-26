using Extract.Code.Attributes;
using Extract.Dashboard.Utilities;
using Extract.ETL;
using Extract.Utilities;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Extract.Dashboard.ETL
{
    [DataContract]
    [KnownType(typeof(ScheduledEvent))]
    [ExtractCategory("DatabaseService", "Update dashboard data cache")]
    public class DashboardExtractedDataService : DatabaseService, IConfigSettings
    {
        #region Constants

        /// <summary>
        /// Current version
        /// </summary>
        const int CURRENT_VERSION = 1;

        #endregion

        #region Fields

        /// <summary>
        /// Indicates whether the Process method is currently executing.
        /// </summary>
        bool _processing;

        /// <summary>
        /// <see cref="CancellationToken"/> that was passed into the <see cref="Process(CancellationToken)"/> method
        /// </summary>
        CancellationToken _cancelToken = CancellationToken.None;

        #endregion

        #region DatabaseService Properties

        /// <summary>
        /// Gets a value indicating whether this instance is processing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if processing; otherwise, <c>false</c>.
        /// </value>
        public override bool Processing
        {
            get
            {
                return _processing;
            }
        }

        [DataMember]
        public override int Version { get; protected set; } = CURRENT_VERSION;

        #endregion DatabaseService Properties

        #region IConfigSettings implementation

        /// <summary>
        /// Method returns the state of the configuration
        /// </summary>
        /// <returns>Returns <see langword="true"/> if configuration is valid, otherwise false</returns>
        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(Description);
        }

        /// <summary>
        /// Displays a form to Configures the ExpandAttributes service
        /// </summary>
        /// <returns><see langword="true"/> if configuration was ok'd. if configuration was canceled returns 
        /// <see langword="false"/></returns>
        public bool Configure()
        {
            try
            {
                var form = new DashboardExtractedDataServiceConfigurationForm(this);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46887");
                return false;
            }
        }

        #endregion

        #region DatabaseService Methods

        public override void Process(CancellationToken cancelToken)
        {
            try
            {
                _processing = true;

                _cancelToken = cancelToken;

                using (var connection = NewSqlDBConnection())
                using (var cmd = connection.CreateCommand())
                {
                    connection.Open();
                    cmd.CommandText = "SELECT ExtractedDataDefinition, DashboardName FROM dbo.Dashboard WHERE UseExtractedData = 1";
                    var task = cmd.ExecuteReaderAsync(_cancelToken);
                    var reader = task.Result;

                    while (reader.Read() && !_cancelToken.IsCancellationRequested)
                    {
                        var xdoc = XDocument.Load(reader.GetXmlReader(0), LoadOptions.None);
                        var dashboard = new DevExpress.DashboardCommon.Dashboard();
                        dashboard.LoadFromXDocument(xdoc);

                        string dashboardName = reader.GetString(1);

                        try
                        {
                            DashboardDataConverter.UpdateExtractedDataSources(dashboard, _cancelToken);
                        }
                        catch (ExtractException ee)
                        {
                            ee.AddDebugData("DashboardName", dashboardName);
                            ee.Log();
                        }
                    }
                }

                _processing = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46922");
            }
        }

        #endregion
    }
}
