using AlertManager.Models.AllDataClasses;

namespace AlertManager.Interfaces
{
    //Interface for logging alert actions
    public interface IAlertActionLogger
    {

        /// <summary>
        /// Logs a action for a given alert
        /// </summary>
        /// <param name="alert">The alerts object for which to post an action</param>
        void LogAction(AlertsObject alert);
    }
}
