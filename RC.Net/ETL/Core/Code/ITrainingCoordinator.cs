namespace Extract.ETL
{
    /// <summary>
    /// Interface to define the values used by <see cref="MachineLearningService"/>s contained by
    /// an <see cref="ITrainingCoordinator"/>
    /// </summary>
    public interface ITrainingCoordinator
    {
        /// <summary>
        /// This value is used to prefix MLModel names of contained services, like a namespace
        /// </summary>
        string ProjectName { get; }

        /// <summary>
        /// The directory to use to resolve any paths, e.g., to LM files
        /// </summary>
        string RootDir { get; }

        /// <summary>
        /// The number of backups to keep for each model
        /// </summary>
        /// <remarks>
        /// When this limit has been reached, the oldest backup will be deleted each time
        /// a new version is saved
        /// </remarks>
        int NumberOfBackupModelsToKeep { get; set; }
    }
}
