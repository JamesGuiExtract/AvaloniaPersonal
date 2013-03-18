using Microsoft.SharePoint;
using System;
using System.Runtime.InteropServices;

namespace Extract.SharePoint.Redaction.Administration
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    [Guid("8f9b6767-53b1-4002-b366-4fb6db9c7d64")]
    public class ExtractSharePointRedactionEventReceiver : SPFeatureReceiver
    {
        /// <summary>
        /// Occurs when a Feature is uninstalled.
        /// </summary>
        /// <param name="properties">An <see cref="T:Microsoft.SharePoint.SPFeatureReceiverProperties"/> object that represents the properties of the event.</param>
        public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        {
            try
            {
                var settings = IdShieldSettings.GetIdShieldSettings(false);
                if (settings != null)
                {
                    // Delete the folder settings for each folder within the site
                    foreach (var siteId in settings.IdShieldSites)
                    {
                        using (var site = new SPSite(siteId))
                        {
                            var list = IdShieldHelper.GetFolderSettingsList(site);
                            if (list != null)
                            {
                                list.Delete();
                            }
                        }
                    }

                    settings.Delete();
                    settings.Unprovision();
                }

                base.FeatureUninstalling(properties);
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.Feature, "ELI31248");
            }
        }
    }
}
