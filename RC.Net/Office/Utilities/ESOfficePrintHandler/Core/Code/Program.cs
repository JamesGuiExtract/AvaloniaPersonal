using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Extract.Office.ESOfficePrintHandler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 2)
                {
                    ExtractException ee = new ExtractException("ELI30258",
                        "Invalid number of arguments.");
                    ee.AddDebugData("Command Line", Environment.CommandLine, false);
                    throw ee;
                }

                string imageFile = args[0];
                string originalFile = args[1];
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30259", ex);
            }
        }
    }
}