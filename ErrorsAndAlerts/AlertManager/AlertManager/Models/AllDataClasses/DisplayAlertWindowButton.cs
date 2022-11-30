using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System.Windows.Input;

namespace AlertManager.Models.AllDataClasses
{
    /// <summary>
    /// public IDataTemplate used to create a executable value for a templatecolumn in a GridTreeTable
    /// returns a Button
    /// </summary>
    [System.Serializable]
    public class DisplayAlertsWindowButton : IDataTemplate
    {
        readonly ICommand? _command;
        readonly string alertType = "";
        public DisplayAlertsWindowButton(object command, string alertType)
        {
            _command = command as ICommand;
            this.alertType = alertType;
        }
        public IControl Build(object param)
        {
            return new Button() { Content = alertType, Command = _command };
        }
        public bool Match(object data)
        {
            return true;
        }
    }
}
