using System;
using static FAMServiceManager.Program;

namespace FAMServiceManager.Views
{
    static class Program
    {
        [STAThread]
        public static void Main() =>
            main(new MainWindow());
    }
}
