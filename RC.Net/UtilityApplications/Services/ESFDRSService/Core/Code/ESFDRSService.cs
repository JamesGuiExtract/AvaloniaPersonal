using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace Extract.UtilityApplications.Services
{
    /// <summary>
    /// Implements a service that runs FDRS (DetectAndReportFailures.exe)
    /// <para><b>Note</b></para>
    /// To allow installation on any version of our software, this project should not have any
    /// dependencies on other pieces of our software. Therefore, the ExtractException class can
    /// not be used.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ESFDRS")]
    public partial class ESFDRSService : ServiceBase
    {
        #region Fields

        /// <summary>
        /// The DetectAndReportFailures.exe <see cref="Process"/>.
        /// </summary>"
        Process _fdrsProcess;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="ESFDRSService"/> instance.
        /// </summary>
        public ESFDRSService()
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
            string commonComponentsDirectory =
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Build process information structure to launch the FDRS application.
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName =
                Path.Combine(commonComponentsDirectory, "DetectAndReportFailure.exe");
            processInfo.WorkingDirectory = commonComponentsDirectory;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;

            _fdrsProcess = Process.Start(processInfo);
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service
        /// by the Service Control Manager (SCM). Specifies actions to take when a service stops
        /// running.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                if (!_fdrsProcess.HasExited)
                {
                    // Attempt to close FDRS nicely.
                    _fdrsProcess.CloseMainWindow();

                    // If it doesn't close nicely within 5 seconds.
                    if (!_fdrsProcess.WaitForExit(5000))
                    {
                        _fdrsProcess.Kill();
                    }
                }
            }
            finally
            {
                _fdrsProcess.Dispose();
                _fdrsProcess = null;
            }
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ESFDRSService"/>.
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

                if (_fdrsProcess != null)
                {
                    _fdrsProcess.Dispose();
                    _fdrsProcess = null;
                }
            }

            // Dispose of unmanaged resources

            base.Dispose(disposing);
        }

        #endregion Overrides
    }
}
