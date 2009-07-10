

namespace Extract.VisualStudio.AddIns
{
    /// <summary>
    /// Represents the functionality of an add-in command.
    /// </summary>
    public interface ICommandAction
    {
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
    }
}
