using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using UCLID_LASERFICHECCLib;

namespace Extract.Laserfiche
{
    /// <summary>
    /// A Windows service that performs redaction on documents submitted for background redaction in 
    /// a Laserfiche repository.
    /// </summary>
    public partial class LaserficheServiceApp : ServiceBase
    {
        #region LaserficheServiceApp Private Members

        /// <summary>
        /// The IDShieldLF object which implements background processing.
        /// </summary>
        private IDShieldLF _IDShieldLF;

        /// <summary>
        /// A flag to indicate whether background processing is running.
        /// </summary>
        private bool _running = false;

        #endregion LaserficheServiceApp Private Members

        #region LaserficheServiceApp Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LaserficheServiceApp"/> class.
        /// </summary>
        public LaserficheServiceApp()
        {
            try
            {
                InitializeComponent();
                base.ExitCode = 0;
            }
            catch (Exception ex)
            {
                UCLID_EXCEPTIONMGMTLib.COMUCLIDException uex = new UCLID_EXCEPTIONMGMTLib.COMUCLIDException();
                uex.CreateFromString("ELI21054", ex.Message);
                uex.Log();
            }
        }

        #endregion LaserficheServiceApp Constructors

        #region LaserficheServiceApp ServiceBase Overrides

        /// <summary>
        /// ServiceBase::OnStart override.  Initializes an IDShieldLF instance (if necessary)
        /// and kicks off background redaction.  
        /// </summary>
        /// <param name="args">OnStart arguments-- not used by this service</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                // Once we are able to use the ExtractException class, comment this assert back in.
                //ExtractException.Assert("ELI21644", "Service is already running.", _running == false);
                if (_running)
                {
                    throw new Exception("Service is already running.");
                }
                       
                if (_IDShieldLF == null)
                {
                    _IDShieldLF = new IDShieldLF();
                }

                _IDShieldLF.StartBackgroundProcessing();
                _running = true;
            }
            catch (Exception ex)
            {
                // Set the Exit code to ERROR_EXCEPTION_IN_SERVICE to indicate the problem
                base.ExitCode = 1064;

                UCLID_EXCEPTIONMGMTLib.COMUCLIDException uexInner = new UCLID_EXCEPTIONMGMTLib.COMUCLIDException();
                uexInner.CreateFromString("ELI21052", ex.Message);

                UCLID_EXCEPTIONMGMTLib.COMUCLIDException uexOuter = new UCLID_EXCEPTIONMGMTLib.COMUCLIDException();
                uexOuter.CreateWithInnerException("ELI21719", "Failed to start " + this.ServiceName, uexInner);

                uexOuter.Log();

                // [FlexIDSIntegrations:60] Rethrow the exception to cause the service to fail.
                Exception exToThrow = new Exception(uexOuter.GetTopText());
                throw exToThrow;
            }
        }

        /// <summary>
        /// ServiceBase::OnStop override.  Stops the background redaction process.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                if (_running)
                {
                    _IDShieldLF.StopBackgroundProcessing();
                }
                _running = true;
            }
            catch (Exception ex)
            {
                // Set the Exit code to ERROR_EXCEPTION_IN_SERVICE to indicate the problem
                base.ExitCode = 1064;

                UCLID_EXCEPTIONMGMTLib.COMUCLIDException uexInner = new UCLID_EXCEPTIONMGMTLib.COMUCLIDException();
                uexInner.CreateFromString("ELI21720", ex.Message);

                UCLID_EXCEPTIONMGMTLib.COMUCLIDException uexOuter = new UCLID_EXCEPTIONMGMTLib.COMUCLIDException();
                uexOuter.CreateWithInnerException("ELI21721", "Failed to stop " + this.ServiceName, uexInner);

                uexOuter.Log();

                // [FlexIDSIntegrations:60] Rethrow the exception to cause the service to fail.
                Exception exToThrow = new Exception(uexOuter.GetTopText());
                throw exToThrow;
            }
        }

        #endregion LaserficheServiceApp ServiceBase Overrides
    }
}
