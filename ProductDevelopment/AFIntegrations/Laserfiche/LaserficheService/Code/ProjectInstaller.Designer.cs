namespace Extract.Laserfiche
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
            this.IDShieldFLServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.IDShieldLFServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // IDShieldFLServiceProcessInstaller
            // 
            this.IDShieldFLServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.IDShieldFLServiceProcessInstaller.Password = null;
            this.IDShieldFLServiceProcessInstaller.Username = null;
            // 
            // IDShieldLFServiceInstaller
            // 
            this.IDShieldLFServiceInstaller.Description = "Extract Systems ID Shield for Laserfiche";
            this.IDShieldLFServiceInstaller.DisplayName = "Extract Systems ID Shield for Laserfiche";
            this.IDShieldLFServiceInstaller.ServiceName = "Extract Systems ID Shield for Laserfiche";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.IDShieldFLServiceProcessInstaller,
            this.IDShieldLFServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller IDShieldFLServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller IDShieldLFServiceInstaller;
    }
}