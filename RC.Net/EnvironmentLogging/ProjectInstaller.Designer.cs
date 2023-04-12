namespace ExtractEnvironmentService
{
    partial class ProjectInstaller
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
            this.ExtractEnvironmentServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ExtractEnvironmentServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // ExtractEnvironmentServiceProcessInstaller
            // 
            this.ExtractEnvironmentServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.ExtractEnvironmentServiceProcessInstaller.Password = null;
            this.ExtractEnvironmentServiceProcessInstaller.Username = null;
            // 
            // ExtractEnvironmentServiceInstaller
            // 
            this.ExtractEnvironmentServiceInstaller.Description = "Logs environment information related to the processing of Extract Systems softwar" +
    "e";
            this.ExtractEnvironmentServiceInstaller.DisplayName = "Extract Environment Service";
            this.ExtractEnvironmentServiceInstaller.ServiceName = "ExtractEnvironment";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ExtractEnvironmentServiceProcessInstaller,
            this.ExtractEnvironmentServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller ExtractEnvironmentServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller ExtractEnvironmentServiceInstaller;
    }
}