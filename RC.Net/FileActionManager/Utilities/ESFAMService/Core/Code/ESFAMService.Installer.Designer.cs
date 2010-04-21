namespace Extract.FileActionManager.Utilities
{
    partial class ProjectInstaller
    {
        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this._serviceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // _serviceProcessInstaller
            // 
            this._serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this._serviceProcessInstaller.Password = null;
            this._serviceProcessInstaller.Username = null;
            // 
            // _serviceInstaller
            // 
            this._serviceInstaller.Description = "Manages multiple instances of the Extract Systems FAM for file processing.";
            this._serviceInstaller.DisplayName = "Extract Systems FAM Service";
            this._serviceInstaller.ServiceName = "ESFAMService";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this._serviceProcessInstaller,
            this._serviceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller _serviceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller _serviceInstaller;
    }
}