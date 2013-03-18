using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using System.Diagnostics.CodeAnalysis;

namespace Extract.SharePoint.DataCapture.Features
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("88350409-db9b-432c-9758-474434c7de26")]
    public class ExtractSharePointEventReceiver : SPFeatureReceiver
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
                        // Attempt to get the data capture group. If it does not exist
                        // then add it.
                        var group = web.SiteGroups[DataCaptureHelper.ExtractDataCaptureGroupName];
                    }
                    catch
                    {
                        try
                        {
                            web.SiteGroups.Add(DataCaptureHelper.ExtractDataCaptureGroupName,
                                web.CurrentUser, web.CurrentUser, "Extract Data Capture administration group.");
                            web.AssociatedGroups.Add(web.SiteGroups[DataCaptureHelper.ExtractDataCaptureGroupName]);
                            web.Update();

                            var assignment = new SPRoleAssignment(web.SiteGroups[DataCaptureHelper.ExtractDataCaptureGroupName]);
                            var role = web.RoleDefinitions["Read"];
                            assignment.RoleDefinitionBindings.Add(role);
                            web.RoleAssignments.Add(assignment);
                            web.Update();
                        }
                        catch (Exception ex)
                        {
                            var ee = new SPException("Unable to add Data Capture administrator group.", ex);
                            DataCaptureHelper.LogException(ee, ErrorCategoryId.Feature, "ELI31485");
                        }
                    }

                    var siteId = site.ID;
                    DataCaptureSettings.AddActiveFeatureSiteId(siteId);

                    DataCaptureHelper.CreateFolderSettingsList(siteId);
                }
            }
            catch (Exception ex)
            {
                DataCaptureHelper.LogException(ex, ErrorCategoryId.Feature, "ELI31486");
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
                    DataCaptureSettings.RemoveActiveFeatureSiteId(site.ID);
                }
            }
            catch (Exception ex)
            {
                DataCaptureHelper.LogException(ex, ErrorCategoryId.Feature, "ELI31487");
            }

            base.FeatureDeactivating(properties);
        }
    }
}
