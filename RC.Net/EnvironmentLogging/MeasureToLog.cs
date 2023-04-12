using Extract;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ExtractEnvironmentService
{
    internal sealed class MeasureToLog : IDisposable
    {
        const int _RETRY_DELAY = 1000;
        int _consecutiveRetries;
        bool _isDisposed;

        BlockingCollection<(IExtractMeasure measurer, Dictionary<string, string> data)> _measurementQueue =
            new BlockingCollection<(IExtractMeasure measurer, Dictionary<string, string>)>();

        public void RequestDataLogging(IExtractMeasure measurer, IEnumerable<Dictionary<string, string>> data)
        {
            try
            {
                foreach (var measurement in data)
                {
                    _measurementQueue.Add((measurer, measurement));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI54184");
            }
        }

        public async Task Run(CancellationToken cancelToken)
        {
            await Task.Run(() =>
            {
                do
                {
                    while (_measurementQueue.TryTake(out var measurement, Timeout.Infinite, cancelToken))
                    {
                        try
                        {
                            EnvironmentLog log = new EnvironmentLog(
                                measurement.measurer.Customer,
                                DateTime.Now,
                                measurement.measurer.Context,
                                measurement.measurer.Entity,
                                measurement.measurer.MeasurementType,
                                measurement.data);
                            log.Log();

                            if (_consecutiveRetries > 0)
                            {
                                new ExtractException("ELI54179",
                                    $"Environment logging succeeded after {_consecutiveRetries} attempts.").Log();
                                _consecutiveRetries = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (cancelToken.IsCancellationRequested)
                            {
                                // If service stop has been requested, don't retry failures.
                                new ExtractException("ELI54181", "Environment logging failure", ex).Log();
                            }
                            else
                            {
                                if (_consecutiveRetries == 0)
                                {
                                    new ExtractException("ELI54182",
                                        "Environment logging failure: starting retries", ex).Log();
                                }

                                // Re-queue measurement for another logging attempt.
                                Thread.Sleep(_RETRY_DELAY);
                                _measurementQueue.Add((measurement.measurer, measurement.data));
                                _consecutiveRetries++;
                            }
                        }
                    }
                }
                // Continue to try to send all measurements taken but not yet sent even if 
                // cancellation has been requested.
                while (_measurementQueue.Any() || !cancelToken.IsCancellationRequested);

                // Flush all pending logs before exiting.
                LogManager.Flush();

                // So TaskContinuationOptions.OnlyOnCanceled is observed
                cancelToken.ThrowIfCancellationRequested();

            }, cancelToken).ConfigureAwait(false);
        }
        
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                // Dispose managed state (managed objects)
                if (disposing)
                {
                    _measurementQueue.Dispose();
                }

                _isDisposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
