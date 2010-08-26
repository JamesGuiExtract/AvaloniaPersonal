using System.ComponentModel;

namespace Extract.ExceptionService
{
    /// <summary>
    /// Service installer class for the Extract exception logging service.
    /// </summary>
    [RunInstaller(true)]
    public partial class ESExceptionServiceInstaller : System.Configuration.Install.Installer
    {
        /// <summary>
        /// Initializes a new instance of the<see cref="ESExceptionServiceInstaller"/> class.
        /// </summary>
        public ESExceptionServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
