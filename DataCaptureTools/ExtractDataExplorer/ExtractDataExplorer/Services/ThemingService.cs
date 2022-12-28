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

    public class ThemingService : IThemingService
    {
        public Window _window;

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
