using System;
using static Extract.FileActionManager.Utilities.FAMServiceManager.Program;

namespace Extract.FileActionManager.Utilities.FAMServiceManager.Views
{
    static class Program
    {
        [STAThread]
        public static void Main() =>
            main(new MainWindow());
    }
}
