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
    /// Progress status displayed while writing features/answers to a csv file
    /// </summary>
    public partial class WritingDataToCsvStatus : Form
    {
        #region Fields

        private LearningMachine _learningMachine;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        Task _mainTask;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Writes data to a CSV and displays progress
        /// </summary>
        /// <param name="learningMachine">The <see cref="LearningMachine"/> to use to compute features</param>
        public WritingDataToCsvStatus(LearningMachine learningMachine)
        {
            try
            {
                _learningMachine = learningMachine;
                InitializeComponent();
            }
            catch (Exception e)
            {
               e.ExtractDisplay("ELI44894");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event. Writes data to CSV.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                WriteData();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44895");
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
                ex.ExtractDisplay("ELI44896");
            }

            base.OnClosing(e);
        }

        #endregion Overrides


        #region Private Methods

        /// <summary>
        /// Starts data writing process
        /// </summary>
        [SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
        private void WriteData()
        {
            var cancellationToken = _cancellationTokenSource.Token;
            var statusUpdates = new ConcurrentQueue<StatusArgs>();
            StatusArgs lastStatus = new StatusArgs { StatusMessage = "Writing data..." };

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
                while (statusUpdates.TryDequeue(out StatusArgs status) && sw.ElapsedMilliseconds - start < 200)
                {
                    if (!string.IsNullOrWhiteSpace(status.StatusMessage))
                    {
                        if (string.Equals(status.StatusMessage, lastStatus.StatusMessage,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            status.Combine(lastStatus);
                        }
                        statusLabel.Text = status.GetFormattedValue(indent: false);
                        lastStatus = status;
                    }
                }
            };
            timer.Start();

            // Write data
            _mainTask = Task.Factory.StartNew(() =>
                _learningMachine.WriteDataToCsv(args => statusUpdates.Enqueue(args), cancellationToken), cancellationToken)

            // Clean-up
            .ContinueWith(task =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        progressBar.Value = progressBar.Minimum;

                        // Stop updating timer status
                        sw.Stop();
                        timer.Dispose();
                    }
                    else if (task.Exception == null)
                    {
                        timer.Tick += delegate
                        {
                            statusLabel.Text += " -- Complete.";
                            progressBar.Style = ProgressBarStyle.Blocks;
                            progressBar.Value = progressBar.Maximum;
                            okButton.Enabled = true;
                            okButton.Focus();

                            // Stop updating timer status
                            sw.Stop();
                            timer.Dispose();
                        };
                    }
                    else
                    {
                        // Stop updating timer status
                        sw.Stop();
                        timer.Dispose();

                        statusLabel.Text = "Error occurred.";
                        progressBar.Style = ProgressBarStyle.Blocks;
                        progressBar.Value = progressBar.Minimum;
                        okButton.Enabled = true;
                        okButton.Focus();

                        // I don't think there will be more than one inner exception but just in case...
                        foreach (var ex in task.Exception.InnerExceptions)
                        {
                            ex.ExtractDisplay("ELI44897");
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
                ex.ExtractDisplay("ELI44898");
            }
        }

        #endregion Event Handlers
    }
}