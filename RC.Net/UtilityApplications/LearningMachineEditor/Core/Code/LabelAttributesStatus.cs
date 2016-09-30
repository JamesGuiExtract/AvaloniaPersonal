using Extract.AttributeFinder;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Extract.UtilityApplications.LearningMachineEditor
{
    /// <summary>
    /// Progress status displayed while labeling attributes
    /// </summary>
    public partial class LabelAttributesStatus : Form
    {
        #region Fields

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        Task _mainTask;
        private LabelAttributes _labelAttributesSettings;
        private InputConfiguration _inputConfig;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Labels attributes and displays progress
        /// </summary>
        /// <param name="labelAttributesSettings">The label attributes settings.</param>
        /// <param name="inputConfig">The input configuration (used to drive labeling/resolve SourceDocName tag).</param>
        public LabelAttributesStatus(LabelAttributes labelAttributesSettings, InputConfiguration inputConfig)
        {
            try
            {
                _labelAttributesSettings = labelAttributesSettings;
                _inputConfig = inputConfig;
                InitializeComponent();
            }
            catch (Exception e)
            {
               e.ExtractDisplay("ELI41455");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                LabelAttributes();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41456");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closing" /> event.
        /// </summary>
        /// <remarks>Cancels processing before closing the dialog</remarks>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
        
            try
            {
                if (cancelButton.Enabled)
                {
                    HandleCancelButton_Click(this, e);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41457");
            }

            base.OnClosing(e);
        }

        #endregion Overrides


        #region Private Methods

        /// <summary>
        /// Starts labeling process
        /// </summary>
        [SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
        private void LabelAttributes()
        {
            var cancellationToken = _cancellationTokenSource.Token;
            var statusUpdates = new ConcurrentQueue<StatusArgs>();
            StatusArgs lastStatus = new StatusArgs { StatusMessage = "Labeling attributes..." };

            // Update UI periodically
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 200;
            timer.Tick += delegate
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                timeElapsedLabel.Text = "Time elapsed: " +
                    sw.Elapsed.ToString(@"hh\:mm\:ss", CultureInfo.CurrentCulture);

                var start = sw.ElapsedMilliseconds;
                StatusArgs status;
                while (statusUpdates.TryDequeue(out status) && sw.ElapsedMilliseconds - start < 200)
                {
                    if (!string.IsNullOrWhiteSpace(status.StatusMessage))
                    {
                        if (string.Equals(status.StatusMessage, lastStatus.StatusMessage,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            status.Combine(lastStatus);
                        }
                        statusLabel.Text = status.GetFormattedValue(indent:false);
                        lastStatus = status;
                    }
                }
            };
            timer.Start();

            // label attributes
            _mainTask = Task.Factory.StartNew(() =>
                _labelAttributesSettings.Process(_inputConfig, args => statusUpdates.Enqueue(args), cancellationToken), cancellationToken)
            // Clean-up
            .ContinueWith(task =>
                {
                    // Stop updating timer status
                    sw.Stop();
                    timer.Dispose();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        progressBar.Value = progressBar.Minimum;
                    }
                    else if (task.Exception == null)
                    {
                        statusLabel.Text = "Attributes successfully labeled.";
                        progressBar.Style = ProgressBarStyle.Blocks;
                        progressBar.Value = progressBar.Maximum;
                        okButton.Enabled = true;
                        okButton.Focus();
                    }
                    else
                    {
                        statusLabel.Text = "Error occurred.";
                        progressBar.Value = progressBar.Minimum;

                        // I don't think there will be more than one inner exception but just in case...
                        foreach (var ex in task.Exception.InnerExceptions)
                        {
                            ex.ExtractDisplay("ELI41458");
                        }
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());

            // Since processing has started, allow canceling
            cancelButton.Enabled = true;
        }

        #endregion Private Methods

        #region Event Handlers

        /// <summary>
        /// Cancels processing and closes the dialog
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleCancelButton_Click(object sender, EventArgs e)
        {
            try
            {
                _cancellationTokenSource.Cancel();
                cancelButton.Enabled = false;
                while (!(_mainTask.IsCompleted || _mainTask.IsCanceled || _mainTask.IsFaulted))
                {
                    statusLabel.Text = "Canceling...";
                    Application.DoEvents();
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41459");
            }
        }

        #endregion Event Handlers
    }
}