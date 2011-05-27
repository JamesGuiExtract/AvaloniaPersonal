using Extract;
using Extract.Licensing;
using Extract.Redaction;
using Extract.Redaction.Verification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace IDShieldOnDemand
{
    static class Program
    {
        #region Fields

        /// <summary>
        /// The <see cref="VerificationTaskForm"/> that will serve as the UI for this application.
        /// </summary>
        static VerificationTaskForm _verifyForm;

        /// <summary>
        /// Indicates a document that was specified via command line parameter that should be opened
        /// as soon as the _verifyForm is loaded.
        /// </summary>
        static string _fileToOpen;

        #endregion Fields

        #region Main

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string exceptionFile = null;

            try
            {
                // Load licenses and validate
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.RedactionCoreObjects, "ELI32606",
                    Application.ProductName);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Launch the verification UI with default settings except:
                // VerifyAllPages is false
                // VerifyAllItems is false
                // Require types is false
                // Seamless mode is false
                // Slideshow feature is disabled
                VerificationSettings settings = new VerificationSettings(
                    new GeneralVerificationSettings(false, false, false, false, false, false),
                    null,
                    @"<SourceDocName>.voa",
                    false,
                    "",
                    null,
                    false,
                    false,
                    new SlideshowSettings(false, false, "", false, "", EActionStatus.kActionUnattempted,
                        false, new ObjectWithDescription(), false, false, 1, false));

                _verifyForm = new VerificationTaskForm(settings, new FAMTagManager());

                Stack<string> arguments = new Stack<string>(args.Reverse());

                // The one and only command line argument is an optional argument specifying a file
                // to open right away.
                if (arguments.Count > 0)
                {
                    string argument = arguments.Pop();

                    if (argument == "/?")
                    {
                        ShowUsage(string.Empty);
                        return;
                    }
                   
                    if (!argument.Equals("/ef", StringComparison.OrdinalIgnoreCase))
                    {
                        _fileToOpen = argument;

                        // Don't attempt to open the document until the _verifyForm has finished loading.
                        _verifyForm.Load += new EventHandler(HandleVerifyFormLoad);

                        if (arguments.Count > 0)
                        {
                            argument = arguments.Pop();
                        }
                    }

                    if (argument.Equals("/ef", StringComparison.OrdinalIgnoreCase))
                    {
                        if (arguments.Count == 0)
                        {
                            ShowUsage("/ef requires a file be specified.");
                            return;
                        }
                        else
                        {
                            exceptionFile = Path.GetFullPath(arguments.Pop());
                        }
                    }

                    if (arguments.Count > 0)
                    {
                        ShowUsage("Unexpected argument: \"" + arguments.Pop() + "\"");
                        return;
                    }
                }

                Application.Run(_verifyForm);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI32607", ex);
                if (!string.IsNullOrEmpty(exceptionFile))
                {
                    ee.Log(exceptionFile);
                }
                else
                {
                    ee.Display();
                }
            }
        }

        #endregion Main

        #region Event Handlers

        /// <summary>
        /// Handles _verifyForm's <see cref="Form.Load"/> event in order to open any file specified
        /// on the command line.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        static void HandleVerifyFormLoad(object sender, EventArgs e)
        {
            try 
	        {
                // Invoke the file open so that it doesn't occur as part of the Form.Load event
                // handler.
                _verifyForm.BeginInvoke((MethodInvoker)(() =>
                    _verifyForm.Open(_fileToOpen, 0, 0, null, null)));
	        }
	        catch (Exception ex)
	        {
		        ex.ExtractDisplay("ELI32608");
	        }
        }
        
        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays the usage message prepended with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message to display before the usage message.</param>
        static void ShowUsage(string errorMessage)
        {
            // Check if there is an error message
            bool isError = !string.IsNullOrEmpty(errorMessage);

            // Initialize the string builder with the error message if specified
            StringBuilder usage = new StringBuilder(isError ? errorMessage : "");
            if (isError)
            {
                usage.AppendLine();
                usage.AppendLine();
            }

            // Add the command line syntax
            usage.Append(Environment.GetCommandLineArgs()[0]);
            usage.AppendLine(" </?>| [<filename>] [/ef <exceptionfile>]");
            usage.AppendLine();
            usage.AppendLine("Options:");
            usage.AppendLine();
            usage.AppendLine("    /? - Display help");
            usage.AppendLine("    filename - The name of the file to open.");
            usage.AppendLine("    /ef <exceptionfile> - Log any exceptions to the specified");
            usage.AppendLine("          exception file rather than display them.");

            // Display the usage as an error or as an information box
            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, 0);
        }

        #endregion Private Members
    }
}
