using Extract;
using Extract.Encryption;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace EncryptTextFile2
{
    /// <summary>
    /// A utility program for encrypting files via the Extract encryption algorithm.
    /// </summary>
    class Program
    {
        #region Constants

        /// <summary>
        /// The default extension added to the output file, unless an output file
        /// has been specified.
        /// </summary>
        private static readonly string _EXTENSION = ".ese";

        #endregion

        /// <summary>
        /// The main function for the application.
        /// </summary>
        /// <param name="args">An array of command line parameters.</param>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Ensure there are the correct number of command line arguments
                if (args.Length < 1 || args.Length > 2)
                {
                    // Display usage with an error flag
                    DisplayUsage(true);
                    return;
                }

                // Check for usage flag
                if (args[0] == "/?")
                {
                    // Display usage, no error
                    DisplayUsage(false);
                    return;
                }

                // Get the fully qualified path to the file
                string fileToEncrypt = Path.GetFullPath(args[0]);

                // Ensure file exists
                ExtractException.Assert("ELI22678", "File does not exist!",
                    File.Exists(fileToEncrypt), "File to encrypt", fileToEncrypt);

                // Get the encrypted file name (either from the command line, or build it
                // from the fileToEncrypt file name)
                string encryptedFileName = args.Length == 2 ? args[1] : fileToEncrypt + _EXTENSION;

                // Encrypt the file
                ExtractEncryption.EncryptFile(fileToEncrypt, encryptedFileName,
                    true, new MapLabel());
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI22679", ex).Display();
            }
        }

        /// <summary>
        /// Displays a usage message to the user.
        /// </summary>
        /// <param name="error">If <see langword="true"/> then displays
        /// the usage with an error string and icon; if <see langword="false"/>
        /// will display the usage along with an information icon.</param>
        private static void DisplayUsage(bool error)
        {
            StringBuilder sb = new StringBuilder();

            // If error, append error string
            if (error)
            {
                sb.Append("Incorrect number of arguments!\n\n");
            }

            // Build usage message
            sb.AppendLine("Usage:");
            sb.AppendLine("----------");
            sb.Append(Path.GetFileNameWithoutExtension(Application.ExecutablePath));
            sb.AppendLine(" <File name>|/? [Output file name]");
            sb.AppendLine("File name - The file to encrypt");
            sb.AppendLine("/? - Display this usage message");
            sb.AppendLine("Output file name - The encrypted file that will be created");
            sb.AppendLine();
            sb.AppendLine("Output:");
            sb.AppendLine("----------");
            sb.AppendLine("If Output file name is not specified: <File name>" + _EXTENSION); 
            sb.AppendLine("If Output file name is specified: [Output file name]");

            // Display the message
            MessageBox.Show(sb.ToString(), "Usage", MessageBoxButtons.OK,
                error ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, 0);
        }
    }
}
