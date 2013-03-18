using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;

namespace Extract.SharePoint.DataCapture.Administration
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    [Guid("978e6dad-3600-4efd-bd70-a7c92e32bf8a")]
    public class ExtractSharePointDataCaptureEventReceiver : SPFeatureReceiver
    {
        /// <summary>
        /// Occurs when a Feature is uninstalled.
        /// </summary>
        /// <param name="properties">An <see cref="T:Microsoft.SharePoint.SPFeatureReceiverProperties"/> object that represents the properties of the event.</param>
        public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        {
            try
            {
                var settings = DataCaptureSettings.GetDataCaptureSettings(false);
                if (settings != null)
                {
                    // Delete the folder settings for each folder within the site
                    foreach (var siteId in settings.DataCaptureSites)
                    {
                        using (var site = new SPSite(siteId))
                        {
                            var list = DataCaptureHelper.GetFolderSettingsList(site);
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
                DataCaptureHelper.LogException(ex, ErrorCategoryId.Feature, "ELI31488");
            }
        }
    }
}
