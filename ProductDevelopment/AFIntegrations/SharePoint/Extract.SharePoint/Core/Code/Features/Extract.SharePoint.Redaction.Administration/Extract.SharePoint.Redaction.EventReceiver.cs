using Microsoft.SharePoint;
using System;
using System.Runtime.InteropServices;

namespace Extract.SharePoint.Redaction.Features.Administration
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("e3003d4b-8511-43cd-a9f6-0ba8506f8230")]
    public class ExtractSharePointRedactionEventReceiver : SPFeatureReceiver
    {
        /// <summary>
        /// Handles the feature uninstalling event. Uninstalls the current IDShield settings.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        {
            var settings = IdShieldProcessingFeatureSettings.GetIdShieldSettings(false);
            if (settings != null)
            {
                settings.Delete();
                settings.Unprovision();
            }
        }
    }
}
