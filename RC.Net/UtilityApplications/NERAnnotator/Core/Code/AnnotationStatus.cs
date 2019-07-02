using Extract.AttributeFinder;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Extract.UtilityApplications.NERAnnotation
{
    /// <summary>
    /// Progress status displayed while labeling attributes
    /// </summary>
    public partial class AnnotationStatus : Form
    {
        #region Fields

        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        Task _mainTask;
        NERAnnotatorSettings _settings;
        bool _processPagesInParallel;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Annotates the input files and displays progress
        /// </summary>
        public AnnotationStatus(NERAnnotatorSettings settings, bool processPagesInParallel)
        {
            try
            {
                _settings = settings;
                _processPagesInParallel = processPagesInParallel;

                InitializeComponent();
            }
            catch (Exception e)
            {
               e.ExtractDisplay("ELI44863");
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
                Annotate(_processPagesInParallel);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44864");
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
                ex.ExtractDisplay("ELI44865");
            }

            base.OnClosing(e);
        }

        #endregion Overrides


        #region Private Methods

        /// <summary>
        /// Starts the annotation process
        /// </summary>
        [SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
        private void Annotate(bool processPagesInParallel)
        {
            var cancellationToken = _cancellationTokenSource.Token;
            var statusUpdates = new ConcurrentQueue<StatusArgs>();
            StatusArgs lastStatus = new StatusArgs { StatusMessage = "Annotating..." };

            // Update UI periodically
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var timer = new System.Windows.Forms.Timer
            {
                Interval = 200
            };
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

            // Annotate
            _mainTask = Task.Factory.StartNew(() =>
                NERAnnotator.Process(_settings, args => statusUpdates.Enqueue(args), cancellationToken, processPagesInParallel), cancellationToken)

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
                            ex.ExtractDisplay("ELI44866");
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
                ex.ExtractDisplay("ELI44867");
            }
        }

        #endregion Event Handlers
    }
}