using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Extract.UtilityApplications.Services
{
    /// <summary>
    /// Collects performance data related to a specific process.
    /// </summary>
    class ProcessPerformanceData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessPerformanceData"/> class.
        /// </summary>
        /// <param name="processMilliseconds">The <see cref="Process.TotalProcessorTime"/> in
        /// milliseconds for the process to be tracked.</param>
        /// <param name="maxSnapshotCount">The maximum number of snapshots to be stored in
        /// <see cref="HistoricalCpuActivity"/>.</param>
        public ProcessPerformanceData(double processMilliseconds, int maxSnapshotCount)
        {
            LastProcessMilliseconds = processMilliseconds;
            // An extra snapshot will be temporarily stored before the old one is removed when
            // the historical activity snapshot collection is "full".
            HistoricalCpuActivity = new Queue<double>(maxSnapshotCount + 1);
        }

        /// <summary>
        /// The <see cref="Process.TotalProcessorTime"/> in milliseconds the last time the process
        /// was checked.
        /// </summary>
        public double LastProcessMilliseconds;

        /// <summary>
        /// Tracks for each polling of a process, the CPU usage percentage.
        /// </summary>
        public Queue<double> HistoricalCpuActivity;
    }


    /// <summary>
    /// Monitors SSOCR2 processes and force-kills any that have been idle for too long.
    /// </summary>
    class OcrProcessMonitor : IDisposable
    {
        #region Constants

        /// <summary>
        /// The name of the process to monitor.
        /// </summary>
        const string _OCR_PROCESS_NAME = "SSOCR2";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The last <see cref="DateTime"/> a polling occured.
        /// </summary>
        DateTime _lastCheck;

        /// <summary>
        /// A <see cref="Timer"/> used for polling at a set frequency.
        /// </summary>
        Timer _timer;

        /// <summary>
        /// A map of the PID of each process being tracked to the
        /// <see cref="ProcessPerformanceData"/> containing the data from each polling.
        /// </summary>
        Dictionary<int, ProcessPerformanceData> _processPerformanceData =
            new Dictionary<int, ProcessPerformanceData>();

        /// <summary>
        /// The number of snapshots needed to cover the
        /// <see cref="RegistryManager.OcrProcessTimeout"/> period.
        /// </summary>
        int _snapshotCount;

        /// <summary>
        /// Ensures each polling is serialized.
        /// </summary>
        static object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OcrProcessMonitor"/> class.
        /// </summary>
        public OcrProcessMonitor()
        {
            // Calculate the number of polling snapshots that are needed to cover the
            // OcrProcessTimeout period.
            _snapshotCount = (int)Math.Ceiling(
                RegistryManager.OcrProcessTimeout / RegistryManager.PollingFrequency);

            // Start polling at the specified interval.
            _lastCheck = DateTime.Now;
            int frequency = (int)(RegistryManager.PollingFrequency * 1000.0);
            _timer = new Timer(CheckOcrCpuUsage, null, frequency, frequency);
        }

        #endregion Constructors

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the <see cref="OcrProcessMonitor"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <overloads>Releases resources used by the <see cref="OcrProcessMonitor"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="OcrProcessMonitor"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources

                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }

            // Dispose of ummanaged resources
        }

        #endregion IDisposable

        #region Private Members

        /// <summary>
        /// Checks the CPU usage of each OCR process instance and kills any that have been idle for
        /// too long.
        /// </summary>
        /// <param name="state">The state. (not used)</param>
        void CheckOcrCpuUsage(Object state)
        {
            // If a previous polling is still in process for some reason, just skip this polling.
            if (Monitor.TryEnter(_lock))
            {
                try
                {
                    // Calculate the time elapsed since the processes we last polled.
                    TimeSpan elapsed = DateTime.Now - _lastCheck;
                    _lastCheck = DateTime.Now;

                    // Get all active OCR processes
                    var activeProcesses = Process.GetProcessesByName(_OCR_PROCESS_NAME);

                    // Remove any processes that were being tracked but that no longer are running
                    // from _processPerformanceData.
                    var exitedProcessIds = _processPerformanceData.Keys
                        .Where(id => !activeProcesses.Select(process => process.Id).Contains(id));
                    foreach (var processId in exitedProcessIds)
                    {
                        _processPerformanceData.Remove(processId);
                    }

                    // Check each active process and keep track of any that should be killed due to
                    // inactivity.
                    var processesToKill = new List<Process>();
                    foreach (var process in activeProcesses)
                    {
                        if (!CheckProcess(elapsed, process))
                        {
                            processesToKill.Add(process);
                        }
                    }

                    // Attempt to kill all processes (in parallel when possible) before throwing any
                    // exceptions.
                    Parallel.ForEach(processesToKill, process =>
                    {
                        process.Kill();
                    });
                }
                // In order to avoid dependencies, for-going exception handling. Just ignore exceptions.
                catch
                { }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
        }

        /// <summary>
        /// Checks the CPU usage for the specified <see paramref="process"/> and determines whether
        /// the process should be force-killed due to inactivity.
        /// </summary>
        /// <param name="elapsed">The elapsed.</param>
        /// <param name="process">The <see cref="Process"/> to be checked for inactivity.</param>
        /// <returns><see langword="true"/> if the process has exhibited some CPU activity over the
        /// course of the current set of snapshots; <see langword="false"/> if the process has
        /// essentially no CPU activity in each of the snapshots for the process.</returns>
        bool CheckProcess(TimeSpan elapsed, Process process)
        {
            try
            {
                // If we have not previously recorded any data for the process, initialize an
                // entry in _processPerformanceData for it.
                ProcessPerformanceData processData;
                if (!_processPerformanceData.TryGetValue(process.Id, out processData))
                {
                    processData = new ProcessPerformanceData(
                        process.TotalProcessorTime.TotalMilliseconds, _snapshotCount);
                    _processPerformanceData[process.Id] = processData;

                    // The process has not existed for a complete polling period; don't start
                    // recording CPU usage until the next check.
                }
                // If we have previously tracked this process, record data for CPU usage since the
                // the last polling and test whether the process has been inactive for the entire
                // timeout period.
                else
                {
                    // Calculate CPU usage since the last polling.
                    // NOTE: This percentage is in terms of wall time, not a ratio of the available
                    // CPU resources. A single-threaded process on a quad core will record 100%
                    // usage if it was continuously processing while a multi-threaded process may
                    // record up to 400%. For the purposes of determining whether a process is idle,
                    // it is only important how long during the polling period the process was
                    // active, not how much of the CPU's resources were being utilized.
                    double totalMilliseconds = process.TotalProcessorTime.TotalMilliseconds;
                    double cpuUsage = (totalMilliseconds - processData.LastProcessMilliseconds)
                        / elapsed.TotalMilliseconds;
                    
                    processData.LastProcessMilliseconds = totalMilliseconds;
                    processData.HistoricalCpuActivity.Enqueue(cpuUsage);

                    // If the historical activity snapshot collection is "full", deque the oldest
                    // snapshot, then test whether there was appreciable CPU activity in any
                    // snapshot.
                    if (processData.HistoricalCpuActivity.Count >= _snapshotCount)
                    {
                        processData.HistoricalCpuActivity.Dequeue();

                        // Consider that there was appreciable activity if the process was active
                        // for more that 1% of the wall time since the last polling.
                        if (processData.HistoricalCpuActivity.All(cpu => cpu < .01))
                        {
                            _processPerformanceData.Remove(process.Id);
                            return false;
                        }
                    }
                }
            }
            catch
            {
                // If there were any errors checking the CPU usage of the snapshot, throw out any
                // existing data for the process and start monitoring the process form scratch.
                try
                {
                    _processPerformanceData.Remove(process.Id);
                }
                catch { }
            }

            return true;
        }

        #endregion Private Members
    }
}
