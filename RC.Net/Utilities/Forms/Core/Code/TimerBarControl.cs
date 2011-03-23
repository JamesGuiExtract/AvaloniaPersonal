using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// A control that displays a solid color progress bar that "completes" in the specified amount
    /// of time.
    /// </summary>
    public partial class TimerBarControl : System.Windows.Forms.UserControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(TimerBarControl).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The bar that represents the current progress.
        /// </summary>
        Rectangle _progressRect = new Rectangle();

        /// <summary>
        /// Animates the width of the progress bar over time.
        /// </summary>
        DoubleAnimation _progressAnimation = new DoubleAnimation();

        /// <summary>
        /// <see cref="Storyboard"/> to control the animation.
        /// </summary>
        Storyboard _storyboard = new Storyboard();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerBarControl"/> class.
        /// </summary>
        public TimerBarControl()
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI32183",
                    _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32182");
            }
        }

        #endregion Constructors
        
        #region Methods

        /// <summary>
        /// Starts the progress bar animation such that it will complete in the specified number of
        /// <see paramref="milliseconds"/>.
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds it should take the progress bar
        /// to complete.</param>
        public void StartTimer(int milliseconds)
        {
            try
            {
                _progressAnimation.From = 0.0;
                _progressAnimation.To = Width;
                _progressAnimation.Duration = TimeSpan.FromMilliseconds(milliseconds);

                _storyboard.Begin(_progressRect, true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32184");
            }
        }

        /// <summary>
        /// Stops the progress bar animation.
        /// </summary>
        /// <param name="indicateComplete"><see langword="true"/> if the progress bar should be
        /// diplayed in complete (filled) status when stopped. If <see langword="false"/>, the
        /// progress bar will remain in its previous position.</param>
        public void StopTimer(bool indicateComplete)
        {
            try
            {
                _storyboard.Stop();

                if (indicateComplete)
                {
                    _progressRect.Width = Width;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32185");
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // In order to name _progressRect for use by the storyboard, we need a NameScope.
                NameScope.SetNameScope(_progressRect, new NameScope());

                // Initialize the progress bar rect.
                _progressRect.Width = 0;
                _progressRect.Height = Height;
                _progressRect.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                _progressRect.Fill = new SolidColorBrush(
                    Color.FromArgb(
                        ForeColor.A, ForeColor.R, ForeColor.G, ForeColor.B));
                _progressRect.Name = "_progressRect";
                _progressRect.RegisterName(_progressRect.Name, _progressRect);

                // Initialize the storyboard.
                _storyboard.Children.Add(_progressAnimation);
                Storyboard.SetTargetName(_progressAnimation, _progressRect.Name);
                Storyboard.SetTargetProperty(_progressAnimation,
                    new PropertyPath(Rectangle.WidthProperty));

                // Create the ElementHost control for hosting the WPF controls.
                ElementHost host = new ElementHost();
                host.Dock = DockStyle.Fill;

                // Use a StackPanel to hold the progress bar.
                StackPanel panel = new StackPanel();
                panel.Background = new SolidColorBrush(
                    Color.FromArgb(
                        BackColor.A, BackColor.R, BackColor.G, BackColor.B));
                panel.Margin = new Thickness(0);
                panel.Children.Add(_progressRect);

                // Add the panel to the ElementHost.
                host.Child = panel;
                host.BackColor = BackColor;

                // Add the ElementHost to the control.
                Controls.Add(host);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32186");
            }
        }

        #endregion Overrides
    }
}
