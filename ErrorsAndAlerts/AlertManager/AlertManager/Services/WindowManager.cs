using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;


namespace AlertManager.Services
{
    public static class WindowManager
    {

        private static Window? _activeWindow; 
        public static Window? ActiveWindow { get { return _activeWindow; } }

        public static void AddWindow(Window? window)
        {
            if (window != null)
            {
                _activeWindow = window;
            }
        }

        public static void ClearWindow()
        {
            _activeWindow = null;
        }
    }
}
