

namespace Extract.Interfaces
{
    /// <summary>
    /// Defines the interface for processes that will be performed by a service
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// Description of the database service item
        /// </summary>
        string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the database. This value is not included in the settings
        /// </summary>
        string DatabaseName
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the Server. This value is not include in the settings
        /// </summary>
        string DatabaseServer
        {
            get;
            set;
        }

        /// <summary>
        /// This is the id from the DatabaseService table.  This value is not included in the settings
        /// </summary>
        int DatabaseServiceID
        {
            get;
            set;
        }

        /// <summary>
        /// Performs the processing defined the database service record
        /// </summary>
        void Process();

        /// <summary>
        /// Loads the settings for this database service process
        /// </summary>
        /// <param name="ID">ID of the record in the DatabaseService table</param>
        /// <param name="settings">The settings in json format that provide the settings for the particular service class</param>
        void Load(int ID, string settings);

        /// <summary>
        /// Returns the settings in a json string
        /// </summary>
        string GetSettings();

    }

}
