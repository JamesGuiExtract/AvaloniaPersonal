using ControlzEx.Theming;
using System.Windows;

namespace ExtractDataExplorer.Services
{
    public enum Theme
    {
        Light,
        Dark
    }

    public interface IThemingService
    {
        public void SetTheme(Theme theme);
    }

    /// <summary>
    /// Handles changing the theme so that the main window view model doesn't need to know about <see cref="Window"/>s
    /// </summary>
    public class ThemingService : IThemingService
    {
        readonly Window _window;

        public ThemingService(Window window)
        {
            _window = window;
        }

        public void SetTheme(Theme theme)
        {
            if (theme == Theme.Light)
            {
                ThemeManager.Current.ChangeTheme(_window, "Light.Blue");
            }
            else
            {
                ThemeManager.Current.ChangeTheme(_window, "Dark.Blue");
            }
        }
    }
}
