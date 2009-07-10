using System.Diagnostics.CodeAnalysis;

namespace Extract.VisualStudio.AddIns
{
    /// <summary>
    /// Represents the functionality and user interface of an add-in command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gets the name of the command prefixed with one or more categories separated by periods.
        /// </summary>
        /// <returns>The name of the command prefixed with one or more categories separated by 
        /// periods.
        /// </returns>
        string Name
        {
            get;
        }

        /// <summary>
        /// Performs the action of the command.
        /// </summary>
        void Execute();

        /// <summary>
        /// Gets or sets whether the command is enabled.
        /// </summary>
        /// <value><see langword="true"/> if the command is able to be executed;
        /// <see langword="false"/> if the command is not able to be executed.</value>
        /// <returns><see langword="true"/> if the command is able to be executed;
        /// <see langword="false"/> if the command is not able to be executed.</returns>
        bool Enabled
        {
            get;
        }

        /// <summary>
        /// Retrieves the user interface settings for the command.
        /// </summary>
        /// <returns>The user interface settings for the command.</returns>
        // Constructing the user interface may be a complex operation.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        CommandUISettings GetUISettings();
    }
}
