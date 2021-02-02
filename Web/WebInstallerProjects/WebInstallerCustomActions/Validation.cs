using Extract;
using Microsoft.Deployment.WindowsInstaller;
using System;

namespace WebInstallerCustomActions
{
    public static class Validation
    {
        [CustomAction]
        public static ActionResult ValidateSiteConfiguration(Session session)
        {
            ExtractException.Assert("ELI51534", "Session cannot be null", session != null);

            try
            {
                var websiteVerificationModel = new WebsiteVerificationModel(session);

                websiteVerificationModel.ShowValidationMessages();

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51513");

                // This seems counter-intuitive, but returning failure will abort the install.
                // The user may want to proceed despite not being able to complete validation, or the
                // exception may actually be indicative of a configuration issue that can be corrected.
                return ActionResult.Success;
            }
        }
    }
}
