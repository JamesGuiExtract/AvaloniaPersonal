using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System.Windows.Input;

namespace AvaloniaDashboard.Models.AllDataClasses
{
    /// <summary>
    /// public IDataTemplate used to create a column value in a GridTreeTable
    /// returns a Button
    /// </summary
    [System.Serializable]
    
    public class DetailsButton : IDataTemplate
    {
        readonly ICommand? _command;
        public DetailsButton(object command)
        {
            if(command != null)
            {
                _command = command as ICommand;
            }
            
        }
        public IControl Build(object param)
        {
            return new Button() { Content = "Details", Command = _command };
        }
        public bool Match(object data)
        {
            // Check if we can accept the provided data
            return true;
        }
    }
}
