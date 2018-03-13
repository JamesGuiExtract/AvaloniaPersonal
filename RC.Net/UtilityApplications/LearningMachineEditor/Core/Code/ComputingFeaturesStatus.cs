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
    /// Progress status displayed while computing feature encodings
    /// </summary>
    public partial class ComputingFeaturesStatus : Form
    {
        #region Fields

        private LearningMachine _learningMachine;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        Task _mainTask;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Computes features and displays progress
        /// </summary>
        /// <param name="learningMachine">The <see cref="LearningMachine"/> to compute features encodings for</param>
        public ComputingFeaturesStatus(LearningMachine learningMachine)
        {
            try
            {
                _learningMachine = learningMachine;
                InitializeComponent();
            }
            catch (Exception e)
            {
               e.ExtractDisplay("ELI40022");
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
                ComputeFeatures();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40023");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closing" /> event.
        /// </summary>
        /// <remarks>Cancels computation before closing the dialog</remarks>
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
                ex.ExtractDisplay("ELI40025");
            }

            base.OnClosing(e);
        }

        #endregion Overrides


        #region Private Methods

        /// <summary>
        /// Starts feature computation process
        /// </summary>
        [SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
        private void ComputeFeatures()
        {
            var cancellationToken = _cancellationTokenSource.Token;
            var statusUpdates = new ConcurrentQueue<StatusArgs>();
            StatusArgs lastStatus = new StatusArgs { StatusMessage = "Computing encoder..." };

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

            // Compute encodings
            _mainTask = Task.Factory.StartNew(() =>
                _learningMachine.ComputeEncodings(args => statusUpdates.Enqueue(args), cancellationToken), cancellationToken)
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
                        statusLabel.Text = "Encoder successfully computed.";
                        progressBar.Style = ProgressBarStyle.Blocks;
                        progressBar.Value = progressBar.Maximum;
                        okButton.Enabled = true;
                        okButton.Focus();
                    }
                    else
                    {
                        statusLabel.Text = "Error occurred. Encoder not computed.";
                        progressBar.Value = progressBar.Minimum;

                        // I don't think there will be more than one inner exception but just in case...
                        foreach (var ex in task.Exception.InnerExceptions)
                        {
                            ex.ExtractDisplay("ELI39819");
                        }
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());

            // Since computation has started, allow canceling
            cancelButton.Enabled = true;
        }

        #endregion Private Methods

        #region Event Handlers

        /// <summary>
        /// Cancels computation and closes the dialog
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
                ex.ExtractDisplay("ELI40024");
            }
        }

        #endregion Event Handlers
    }
}