using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;

namespace Extract.UtilityApplications.Services
{
    /// <summary>
    /// 
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectInstaller"/> class.
        /// </summary>
        public ProjectInstaller()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33455");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Configuration.Install.Installer.AfterInstall"/> event.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary"/> that contains
        /// the state of the computer after all the installers contained in the
        /// <see cref="P:System.Configuration.Install.Installer.Installers"/> property have
        /// completed their installations.</param>
        protected override void OnAfterInstall(IDictionary savedState)
        {
            try
            {
                base.OnAfterInstall(savedState);

                // Grants all users access to start and stop the ESIPCService.
                NativeMethods.SetServicePermissions();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33456");
            }
        }
    }
}
