using System.ServiceProcess;

namespace Extract.UtilityApplications.Services
{
    /// <summary>
    /// A <see cref="ServiceBase"/> derivative that monitors Extract Systems applications in order
    /// to improve reliability of the software.
    /// </summary>
    public partial class AppMonitorService : ServiceBase
    {
        #region Fields

        /// <summary>
        /// The <see cref="OcrProcessMonitor"/> that monitors SSOCR2 processes.
        /// </summary>
        OcrProcessMonitor _ocrProcessMonitor;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AppMonitorService"/> class.
        /// </summary>
        public AppMonitorService()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the
        /// service by the Service Control Manager (SCM) or when the operating system starts
        /// (for a service that starts automatically). Specifies actions to take when the service
        /// starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            _ocrProcessMonitor = new OcrProcessMonitor();
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service
        /// by the Service Control Manager (SCM). Specifies actions to take when a service stops
        /// running.
        /// </summary>
        protected override void OnStop()
        {
            _ocrProcessMonitor.Dispose();
            _ocrProcessMonitor = null;
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="AppMonitorService"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_ocrProcessMonitor != null)
                {
                    _ocrProcessMonitor.Dispose();
                    _ocrProcessMonitor = null;
                }
            }

            // Dispose of unmanaged resources

            base.Dispose(disposing);
        }

        #endregion Overrides
    }
}
