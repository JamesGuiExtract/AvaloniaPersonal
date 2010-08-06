using Microsoft.SharePoint;
using System;
using System.Runtime.InteropServices;

namespace Extract.SharePoint.Features.Extract.SharePoint.Redaction
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
            ExtractSharePointHelper.LoadFeatureSettings(properties.Feature);
            base.FeatureActivated(properties);
        }


        /// <summary>
        /// Raises the feature deactivating event.
        /// </summary>
        /// <param name="properties">The properties for the feature being deactivated.</param>
        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            ExtractSharePointHelper.SaveFeatureSettings(properties.Feature);
            base.FeatureDeactivating(properties);
        }


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}
    }
}
