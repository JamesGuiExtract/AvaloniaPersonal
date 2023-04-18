using AlertManager.Models.AllDataClasses;

namespace AlertManager.Interfaces
{
    //Interface for logging alert resolutions
    public interface IAlertActionLogger
    {

        /// <summary>
        /// Logs a resolution for a given alert
        /// </summary>
        /// <param name="alert">The alerts object for which to post a resolution</param>
        void LogResolution(AlertsObject alert);
    }
}
