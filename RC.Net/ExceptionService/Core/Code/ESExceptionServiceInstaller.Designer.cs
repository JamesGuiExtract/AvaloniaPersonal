namespace Extract.ExceptionService
{
    /// <summary>
    /// Class to handle the installation of the Extract exception logging service.
    /// </summary>
    partial class ESExceptionServiceInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.esServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.esServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // esServiceProcessInstaller
            // 
            this.esServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.esServiceProcessInstaller.Password = null;
            this.esServiceProcessInstaller.Username = null;
            // 
            // esServiceInstaller
            // 
            this.esServiceInstaller.Description = "Extract Systems exception logging service";
            this.esServiceInstaller.DisplayName = "Extract Systems Exception Service";
            this.esServiceInstaller.ServiceName = "ESExceptionService";
            this.esServiceInstaller.ServicesDependedOn = new string[] {
        "NetTcpPortSharing"};
            this.esServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ESExceptionServiceInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.esServiceProcessInstaller,
            this.esServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller esServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller esServiceInstaller;
    }
}