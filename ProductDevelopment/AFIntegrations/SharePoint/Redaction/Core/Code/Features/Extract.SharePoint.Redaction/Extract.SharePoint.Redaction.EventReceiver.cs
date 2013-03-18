using Microsoft.SharePoint;
using System;
using System.Diagnostics.CodeAnalysis;
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
        // Suppressing the warning about the unused local group. This assignment is made to check
        // whether the group exists. It appears the only way to check a groups existence is to
        // attempt to get it, if we get it then it is there and we don't need to do anything to
        // it, if it is not there then we need to create the group.
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId="group")]
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPSite site = null;
            try
            {
                site = properties.Feature.Parent as SPSite;
                if (site != null)
                {
                    var web = site.RootWeb;
                    try
                    {
                        // Attempt to get the ID Shield group. If it does not exist
                        // then add it.
                        var group = web.SiteGroups[IdShieldHelper.IdShieldGroupName];
                    }
                    catch
                    {
                        try
                        {
                            web.SiteGroups.Add(IdShieldHelper.IdShieldGroupName,
                                web.CurrentUser, web.CurrentUser, "ID Shield administration group.");
                            web.AssociatedGroups.Add(web.SiteGroups[IdShieldHelper.IdShieldGroupName]);
                            web.Update();

                            var assignment = new SPRoleAssignment(web.SiteGroups[IdShieldHelper.IdShieldGroupName]);
                            var role = web.RoleDefinitions["Read"];
                            assignment.RoleDefinitionBindings.Add(role);
                            web.RoleAssignments.Add(assignment);
                            web.Update();
                        }
                        catch (Exception ex)
                        {
                            var ee = new SPException("Unable to add ID Shield administrator group.", ex);
                            IdShieldHelper.LogException(ee, ErrorCategoryId.Feature, "ELI31378");
                        }
                    }

                    var siteId = site.ID;
                    IdShieldSettings.AddActiveFeatureSiteId(siteId);

                    ExtractSharePointHelper.CreateSpecifiedList(siteId, IdShieldHelper._HIDDEN_IGNORE_FILE_LIST, true);
                    IdShieldHelper.CreateFolderSettingsList(siteId);
                }
            }
            catch (Exception ex)
            {
                IdShieldHelper.LogException(ex, ErrorCategoryId.Feature, "ELI30591");
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
                    var web = site.RootWeb;

                    IdShieldSettings.RemoveActiveFeatureSiteId(site.ID);

                    // Remove the hidden ignore file list if it exists
                    using (SPSite tempSite = new SPSite(site.ID))
                    {
                        var list = ExtractSharePointHelper.GetSpecifiedList(tempSite,
                            IdShieldHelper._HIDDEN_IGNORE_FILE_LIST);
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

            base.FeatureDeactivating(properties);
        }
    }
}
