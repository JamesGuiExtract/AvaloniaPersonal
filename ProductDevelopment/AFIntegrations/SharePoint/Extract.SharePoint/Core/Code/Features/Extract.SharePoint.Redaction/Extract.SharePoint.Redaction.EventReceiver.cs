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
            try
            {
                IdShieldSettings.LoadIdShieldSettings(properties.Feature);
                base.FeatureActivated(properties);
            }
            catch (Exception ex)
            {
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.Feature, ex);
            }
        }


        /// <summary>
        /// Raises the feature deactivating event.
        /// </summary>
        /// <param name="properties">The properties for the feature being deactivated.</param>
        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            try
            {
                IdShieldSettings.StoreIdShieldSettings(properties.Feature);
                base.FeatureDeactivating(properties);
            }
            catch (Exception ex)
            {
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.Feature, ex);
            }
        }

        /// <summary>
        /// Raises the feature uninstalling event.
        /// </summary>
        /// <param name="properties">The properties for the feature being uninstalled.</param>
        public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        {
            try
            {
#if !DEBUG
                IdShieldSettings.RemoveIdShieldSettings();
#endif
                base.FeatureUninstalling(properties);
            }
            catch (Exception ex)
            {
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.Feature, ex);
            }
        }
    }
}
