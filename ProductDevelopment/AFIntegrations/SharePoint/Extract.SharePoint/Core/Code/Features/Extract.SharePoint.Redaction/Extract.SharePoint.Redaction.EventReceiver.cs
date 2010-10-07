using Microsoft.SharePoint;
using System;
using System.Runtime.InteropServices;

namespace Extract.SharePoint.Redaction.Features
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    [Guid("6036eb8a-5586-4b15-9e3e-66bf544b005f")]
    public class ExtractEventReceiver : SPFeatureReceiver
    {
        /// <summary>
        /// Raises the feature activated event.
        /// </summary>
        /// <param name="properties">The properties for the feature being activated.</param>
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPSite site = null;
            try
            {
                site = properties.Feature.Parent as SPSite;
                if (site != null)
                {
                    IdShieldSettings.AddActiveFeatureSiteId(site.ID);

                    using (SPSite tempSite = new SPSite(site.ID))
                    using (SPWeb web = tempSite.RootWeb)
                    {
                        // Add hidden list to site (if it does not exist)
                        SPList list = web.Lists.TryGetList(IdShieldHelper._HIDDEN_LIST_NAME);
                        if (list == null)
                        {
                            web.AllowUnsafeUpdates = true;
                            Guid listId = web.Lists.Add(IdShieldHelper._HIDDEN_LIST_NAME, "",
                                SPListTemplateType.GenericList);
                            web.Update();
                            list = web.Lists[listId];
                            if (list != null)
                            {
                                list.Hidden = true;
                                list.Update();
                            }
                            web.AllowUnsafeUpdates = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.Feature, "ELI30591");
            }
            finally
            {
                if (site != null)
                {
                    site.Dispose();
                }
            }

            base.FeatureActivated(properties);
        }

        /// <summary>
        /// Raises the feature deactivating event.
        /// </summary>
        /// <param name="properties">The properties for the feature being deactivated.</param>
        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            SPSite site = null;
            try
            {
                site = properties.Feature.Parent as SPSite;
                if (site != null)
                {
                    IdShieldSettings.RemoveActiveFeatureSiteId(site.ID);

                    // Remove the hidden list if it exists
                    using (SPSite tempSite = new SPSite(site.ID))
                    using (SPWeb web = tempSite.RootWeb)
                    {
                        SPList list = web.Lists.TryGetList(IdShieldHelper._HIDDEN_LIST_NAME);
                        if (list != null)
                        {
                            web.AllowUnsafeUpdates = true;
                            list.Delete();
                            web.Update();
                            web.AllowUnsafeUpdates = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.Feature, "ELI30592");
            }
            finally
            {
                if (site != null)
                {
                    site.Dispose();
                }
            }

            base.FeatureDeactivating(properties);
        }

        /// <summary>
        /// Raises the feature uninstalling event.
        /// </summary>
        /// <param name="properties">The properties for the feature being uninstalled.</param>
        public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        {
            try
            {
                IdShieldSettings.RemoveIdShieldSettings();
                base.FeatureUninstalling(properties);
            }
            catch (Exception ex)
            {
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.Feature, ex, "ELI30551");
            }
        }
    }
}
