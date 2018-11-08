using Extract.AttributeFinder;
using Extract.Utilities.Forms;
using LearningMachineTrainer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AccuracyData = Extract.Utilities.Union<Accord.Statistics.Analysis.GeneralConfusionMatrix, Accord.Statistics.Analysis.ConfusionMatrix>;

namespace Extract.UtilityApplications.LearningMachineEditor
{
    /// <summary>
    /// Training/testing form
    /// </summary>
    public partial class TrainingTesting : Form
    {
        #region Constants

        private static readonly int _HISTORY_CUTOFF = 1000;

        #endregion Constants

        #region Fields

        private LearningMachineConfiguration _editor;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _mainTask;
        private ConcurrentQueue<StatusArgs> _statusUpdates = new ConcurrentQueue<StatusArgs>();
        private bool _processing;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a new instance of the form
        /// </summary>
        /// <param name="editor">The main editor form. Used to update current machine after training.</param>
        public TrainingTesting(LearningMachineConfiguration editor)
        {
            try
            {
                InitializeComponent();
                _editor = editor;
            }
            catch (Exception e)
            {
                e.ExtractDisplay("ELI40034");
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
            try
            {
                base.OnLoad(e);
                UpdateControlsAndFlags();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40035");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Runs training
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleTrainAndTestButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_processing)
                {
                    _processing = true;

                    LearningMachine tempMachine = null;
                    using (new TemporaryWaitCursor())
                        tempMachine = _editor.CurrentLearningMachine.DeepClone();
                    if (recomputeFeaturesRadioButton.Checked)
                    {
                        tempMachine.Encoder.Clear();
                    }
                    TrainTestMachine(tempMachine);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40036");
            }
        }

        /// <summary>
        /// Runs test
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleTestButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_processing)
                {
                    _processing = true;
                    ExtractException.Assert("ELI39860", "Logic error", _editor.CurrentLearningMachine.IsTrained);

                    LearningMachine tempMachine = null;
                    using (new TemporaryWaitCursor())
                        tempMachine = _editor.CurrentLearningMachine.DeepClone();
                    TrainTestMachine(tempMachine, testOnly: true);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39971");
            }
        }

        /// <summary>
        /// Cancels training
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleCancelButton_Click(object sender, EventArgs e)
        {
            try
            {
                cancelButton.Enabled = false;
                closeButton.Enabled = false;
                _cancellationTokenSource.Cancel();
                _statusUpdates.Enqueue(new StatusArgs { StatusMessage = "Canceling..." });

                // Wait for task to complete
                while (!(_mainTask == null || _mainTask.IsCompleted || _mainTask.IsCanceled || _mainTask.IsFaulted))
                {
                    Application.DoEvents();
                    Thread.Sleep(100);
                }

                // Allow log writing to finish before disposing of text box
                StatusArgs _;
                while (_statusUpdates.TryPeek(out _))
                {
                    Application.DoEvents();
                    Thread.Sleep(100);
                }

                // Free-up memory in case there is a lot used.
                System.GC.Collect();
                closeButton.Enabled = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40037");
            }
        }

        /// <summary>
        /// Cancels any operation before closing the dialog
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleTrainingTesting_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (cancelButton.Enabled)
                {
                    HandleCancelButton_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40038");
            }
        }

        /// <summary>
        /// Scrolls training log to end
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleTrainingTesting_Shown(object sender, EventArgs e)
        {
            try
            {
                textBox1.Text = _editor.CurrentLearningMachine.TrainingLog ?? "";
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.ScrollToCaret();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40039");
            }
        }

        /// <summary>
        /// Clears the training log
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleClearLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            _editor.ClearTrainingLog();
        }

        /// <summary>
        /// Copies the selected training log text to the clipboard
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Copy();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40078");
            }
        }

        /// <summary>
        /// Selects all training log text
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleSelectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.SelectAll();
                textBox1.Focus();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40079");
            }
        }

        /// <summary>
        /// Handles the Opening event of the HandleTrainingLogContextMenuStrip control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void HandleTrainingLogContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                trainingLogContextMenuStrip.Items["copyToolStripMenuItem"].Enabled = textBox1.SelectionLength > 0;
                trainingLogContextMenuStrip.Items["selectAllToolStripMenuItem"].Enabled =
                    trainingLogContextMenuStrip.Items["clearLogToolStripMenuItem"].Enabled =
                    textBox1.TextLength > 0;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40080");
            }
        }

        private void HandleDetailsButton_Click(object sender, EventArgs e)
        {
            var accuracyData = _editor.CurrentLearningMachine.AccuracyData;
            if (!accuracyData.HasValue)
            {
                return;
            }

            var val = accuracyData.Value;
            var copy = (val.train?.DeepClone(), val.test?.DeepClone());
            using (var form = new TestDetails(copy))
            {
                form.ShowDialog();
            }
        }

        private void HandleLoadDataFromCsvRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControlsAndFlags();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45545");
            }
        }

        private void HandleSaveFeatureVectorsToCsvsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControlsAndFlags();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46328");
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Run training process
        /// </summary>
        [SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges")]
        private void TrainTestMachine(LearningMachine learningMachine, bool testOnly = false)
        {
            testButton.Enabled = false;
            trainTestButton.Enabled = false;
            computeFeaturesGroupBox.Enabled = false;
            clearLogToolStripMenuItem.Enabled = false;

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            var writeToLog = GetStatusWriter(learningMachine);

            // Update UI periodically
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 200;
            timer.Tick += delegate
            {
                var start = sw.ElapsedMilliseconds;
                var updates = new List<StatusArgs>();
                StatusArgs status;
                while (_statusUpdates.TryDequeue(out status) && sw.ElapsedMilliseconds - start < 200)
                {
                    updates.Add(status);
                }
                writeToLog(updates);
            };
            timer.Start();

            // Write machine info to log
            _statusUpdates.Enqueue(new StatusArgs
            {
                StatusMessage = new StringBuilder(4)
                      .Append("Time: ")
                      .AppendLine(DateTime.Now.ToString("s", CultureInfo.CurrentCulture))
                      .AppendLine(testOnly ? "Operation: Testing Machine" : "Operation: Training Machine")
                      .AppendLine(learningMachine.ToString())
                      .ToString()
            });

            Func<Action<StatusArgs>, CancellationToken, ValueTuple<AccuracyData, AccuracyData>> operation = null;
            bool updateCsvs = updateCsvsWithPredictionsCheckBox.Checked;
            if (testOnly)
            {
                if (loadDataFromCsvRadioButton.Checked)
                {
                    operation = (u, c) => LearningMachineMethods.TrainAndTestWithCsvData(learningMachine, true, learningMachine.CsvOutputFile, updateCsvs, u, c);
                }
                else if (saveFeatureVectorsToCsvsCheckBox.Checked)
                {
                    operation = (u, c) =>
                    {
                        ExtractException.Assert("ELI46198", "You must uncheck 'standardize features...' to use CSV data for training/testing",
                            !learningMachine.StandardizeFeaturesForCsvOutput);

                        learningMachine.WriteDataToCsv(u, c);
                        return LearningMachineMethods.TrainAndTestWithCsvData(learningMachine, true, learningMachine.CsvOutputFile, updateCsvs, u, c);
                    };
                }
                else
                {
                    operation = learningMachine.TestMachine;
                }
            }
            else
            {
                if (loadDataFromCsvRadioButton.Checked)
                {
                    operation = (u, c) => LearningMachineMethods.TrainAndTestWithCsvData(learningMachine, false, learningMachine.CsvOutputFile, updateCsvs, u, c);
                }
                else if (saveFeatureVectorsToCsvsCheckBox.Checked)
                {
                    operation = (u, c) =>
                    {
                        ExtractException.Assert("ELI46199", "You must uncheck 'standardize features...' to use CSV data for training/testing",
                            !learningMachine.StandardizeFeaturesForCsvOutput);

                        if (!learningMachine.Encoder.AreEncodingsComputed)
                        {
                            learningMachine.ComputeEncodings(u, c);

                            // Preserve the computed encodings in case training fails
                            _editor.CurrentLearningMachine = learningMachine;
                        }
                        learningMachine.WriteDataToCsv(u, c);

                        return LearningMachineMethods.TrainAndTestWithCsvData(learningMachine, false, learningMachine.CsvOutputFile, updateCsvs, u, c);
                    };
                }
                else
                {
                    operation = (u, c) =>
                    {
                        // Compute encodings as a separate step so that if something fails during
                        // training it won't need to be repeated
                        if (!learningMachine.Encoder.AreEncodingsComputed)
                        {
                            learningMachine.ComputeEncodings(u, c);
                            _editor.CurrentLearningMachine = learningMachine;
                        }

                        return learningMachine.TrainMachine(u, c);
                    };
                }
            }

            // Train/test machine
            _mainTask = Task.Factory.StartNew(() =>
                operation(args =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    _statusUpdates.Enqueue(args);
                }, cancellationToken)
                , cancellationToken)
            .ContinueWith(task =>
            {
                Exception failed = SharedTrainingMethods.RunCleanup(task, learningMachine, testOnly, sw, _statusUpdates, cancellationToken);

                // Flush log
                StatusArgs _;
                while (_statusUpdates.TryPeek(out _))
                {
                    Application.DoEvents();
                    Thread.Sleep(100);
                }
                timer.Dispose();
                cancelButton.Enabled = false;

                if (failed is ExtractException ue)
                {
                    ue.Display();
                }
                else if (!cancellationToken.IsCancellationRequested)
                {
                    _editor.CurrentLearningMachine = learningMachine;

                    // The editor preserves the previous training log
                    // so overwrite it with the text box value
                    // (previously this wasn't necessary because the CurrentLearningMachine update took place
                    // before the accuracy status updates were written but now that happens above in
                    // SharedTrainingMethods.RunCleanup())
                    // https://extract.atlassian.net/projects/ISSUE/issues/ISSUE-15523
                    learningMachine.TrainingLog = textBox1.Text;
                }

                // Re-enable buttons
                UpdateControlsAndFlags();

            }, TaskScheduler.FromCurrentSynchronizationContext()); // End of ContinueWith

            // Since computation has started, allow canceling
            cancelButton.Enabled = true;
        }


        /// <summary>
        /// Set boolean flags and control states based on learning machines' states
        /// </summary>
        private void UpdateControlsAndFlags()
        {
            computeFeaturesGroupBox.Enabled = true;

            // Disable controls based on machine state
            testButton.Enabled = _editor.CurrentLearningMachine.Classifier.IsTrained;
            useCurrentFeaturesRadioButton.Enabled = _editor.CurrentLearningMachine.Encoder.AreEncodingsComputed;
            saveFeatureVectorsToCsvsCheckBox.Enabled =
                !loadDataFromCsvRadioButton.Checked
                && !string.IsNullOrWhiteSpace(_editor.CurrentLearningMachine.CsvOutputFile);

            loadDataFromCsvRadioButton.Enabled =
                !string.IsNullOrWhiteSpace(_editor.CurrentLearningMachine.CsvOutputFile);

            trainTestButton.Enabled = _editor.CurrentLearningMachine.InputConfig.TrainingSetPercentage > 0
                || loadDataFromCsvRadioButton.Checked;

            clearLogToolStripMenuItem.Enabled = !string.IsNullOrWhiteSpace(_editor.CurrentLearningMachine.TrainingLog);
            detailsButton.Enabled = _editor.CurrentLearningMachine.AccuracyData != null;

            // Set control values based on enabled states
            if (!loadDataFromCsvRadioButton.Enabled && loadDataFromCsvRadioButton.Checked)
            {
                useCurrentFeaturesRadioButton.Checked = true;
            }
            if (!useCurrentFeaturesRadioButton.Enabled && useCurrentFeaturesRadioButton.Checked)
            {
                recomputeFeaturesRadioButton.Checked = true;
            }

            updateCsvsWithPredictionsCheckBox.Enabled =
                saveFeatureVectorsToCsvsCheckBox.Enabled && saveFeatureVectorsToCsvsCheckBox.Checked
                || loadDataFromCsvRadioButton.Enabled && loadDataFromCsvRadioButton.Checked;

            _processing = false;
        }

        /// <summary>
        /// Gets a function that writes status updates to a text box and training log
        /// </summary>
        /// <param name="learningMachine">The <see cref="LearningMachine"/> to update the training log for</param>
        /// <returns>The function that writes status updates</returns>
        private Action<IEnumerable<StatusArgs>> GetStatusWriter(LearningMachine learningMachine)
        {
            if (textBox1.TextLength > 0)
            {
                // Truncate history so that it doesn't go over 1000 sessions
                var sessions = Regex.Split(textBox1.Text, @"(?mx)^(?=---\s(?:$|\s{2}))");
                if (sessions.Length >= _HISTORY_CUTOFF)
                {
                    textBox1.Text = string.Concat(sessions.Skip(1 + sessions.Length - _HISTORY_CUTOFF));

                    // Scroll to end
                    textBox1.SelectionStart = textBox1.TextLength;
                    textBox1.ScrollToCaret();
                }
                textBox1.AppendText("\r\n");
            }

            // Mark start of session
            StatusArgs lastStatus = new StatusArgs
            {
                TaskName = "__START_OF_SESSION__", // Name not important, keep from matching next status
                StatusMessage = "---" // In case these logs turn into YAML files, use YAML BOF
            };

            int overwriteLength = 0;

            return statuses =>
                {
                    if (!statuses.Any())
                    {
                        return;
                    }
                    var pendingWrite = new StringBuilder();
                    foreach (var status in statuses)
                    {
                        // If task name and message are the same as last, combine
                        if (string.Equals(lastStatus.TaskName, status.TaskName, StringComparison.OrdinalIgnoreCase)
                            && (string.Equals(lastStatus.StatusMessage, status.StatusMessage,
                                StringComparison.OrdinalIgnoreCase)))
                        {
                            lastStatus.Combine(status);
                        }
                        // If the task name is the same and replace is specified then replace old with new
                        else if (string.Equals(lastStatus.TaskName, status.TaskName, StringComparison.OrdinalIgnoreCase)
                            && status.ReplaceLastStatus)
                        {
                            lastStatus = status;
                        }
                        // Else add last status to string pending write
                        else
                        {
                            pendingWrite.AppendLine(lastStatus.GetFormattedValue());
                            lastStatus = status;
                        }
                    }

                    // update text box with history + latest status
                    var lastStatusValue = lastStatus.GetFormattedValue();
                    textBox1.Select(textBox1.TextLength - overwriteLength, overwriteLength);
                    textBox1.SelectedText = pendingWrite.ToString() + lastStatusValue;
                    if (learningMachine != null)
                    {
                        learningMachine.TrainingLog = textBox1.Text;
                    }

                    // Set overwriteLength value for next time
                    overwriteLength = lastStatusValue.Length;
                };
        }

        #endregion Private Methods
    }
}
