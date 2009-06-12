using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a splash screen.
    /// </summary>
    public partial class SplashScreen : Form
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(SplashScreen).ToString();

        #endregion Constants

        #region SplashScreen Constructors

        /// <summary>
        /// Initializes a new <see cref="SplashScreen"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public SplashScreen(Type type, string resource)
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel()); 
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23157",
                    _OBJECT_NAME);

                InitializeComponent();

                // Set the image for the splash screen
                _pictureBox.Image = new Bitmap(type, resource);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23158", ex);
            }
        }

        #endregion SplashScreen Constructors

        #region SplashScreen Methods

        /// <summary>
        /// Displays a splash screen and creates a form of the specified type.
        /// </summary>
        /// <typeparam name="TForm">The type of the form to initialize.</typeparam>
        /// <param name="form">The form to initialize.</param>
        /// <param name="resource">The name of the image resource to display as the splash screen. 
        /// Must be defined in the same assembly as <typeparamref name="TForm"/>.</param>
        // Fixing this FxCop message produces another one (CA1004), which should not be suppressed.
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId="1#")]
        public static void ShowDuringFormCreation<TForm>(string resource, out TForm form) 
            where TForm : Form, new()
        {
            // Create the splash screen
            using (SplashScreen splashScreen = new SplashScreen(typeof(TForm), resource))
            {
                // Display the splash screen
                splashScreen.Show();

                // Process the splash screen's show event
                Application.DoEvents();

                // Initialize the form
                form = new TForm();

                // Wait for the splash screen to finish
                while (splashScreen.Visible)
                {
                    Application.DoEvents();
                }
            }
        }

        #endregion SplashScreen Methods

        #region SplashScreen Event Handlers

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> event.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _timer.Start();
        }

        #endregion SplashScreen Event Handlers

        /// <summary>
        /// Handles the <see cref="Timer.Tick"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="Timer.Tick"/> event.</param>
        /// <param name="e">The event data associated with the <see cref="Timer.Tick"/> event.
        /// </param>
        private void HandleTimerTick(object sender, EventArgs e)
        {
            _timer.Stop();
            Close();
        }
    }
}