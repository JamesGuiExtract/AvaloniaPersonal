using Extract;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ExtractEnvironmentService
{
    public partial class EnvironmentLogger : ServiceBase
    {
        const int _STAGGERED_START_INTERVAL = 5_000;
        const int _SERVICE_STOP_MAX_WAIT = 120_000;
        static DataTransferObjectSerializer _serializer = new(typeof(IExtractMeasure).Assembly);
        CancellationTokenSource _measurerCancellationTokenSource = new CancellationTokenSource();
        CancellationTokenSource _loggerCancellationTokenSource = new CancellationTokenSource();
        Task _runningMeasurers;
        Task _runningLogger;
        MeasureToLog _logger;
        bool _disposed;

        public EnvironmentLogger()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _measurerCancellationTokenSource = new CancellationTokenSource();
                _loggerCancellationTokenSource = new CancellationTokenSource();
                
                var eeStart = new ExtractException("ELI54169", "Application trace: Extract Environment Service starting");

                //Get each implementation of IExtractLog stored in configuraton
                List<IExtractMeasure> activeMeasures = new();
                foreach (var configName in ConfigurationManager.AppSettings.AllKeys)
                {
                    try
                    {
                        var config = ConfigurationManager.AppSettings[configName];
                        var measure = _serializer.Deserialize(config.ToString()).CreateDomainObject();
                        if (measure is IExtractMeasure activeMeasure && activeMeasure.Enabled)
                        {
                            activeMeasures.Add(activeMeasure);
                            eeStart.AddDebugData("Measure", configName);
                        }
                    }
                    catch (Exception ex)
                    {
                        new ExtractException("ELI54174",
                            "Failed to load environment measure config: " + configName,
                            ex).Log();
                    }
                }

                _logger = new();
                _runningLogger = _logger.Run(_loggerCancellationTokenSource.Token);

                int backoffTime = 0;
                _runningMeasurers = Task.WhenAll(
                    activeMeasures
                        // Stagger measurement starts, less frequent measurements first
                        .OrderByDescending(measurer => measurer.MeasurementInterval)
                        .Select(async measurer =>
                        {
                            // Stagger start of measurements / use available thread from thread pool.
                            await Task.Delay(backoffTime).ConfigureAwait(false);
                            backoffTime += _STAGGERED_START_INTERVAL;

                            await RunMeasurer(measurer, _logger, _measurerCancellationTokenSource.Token).ConfigureAwait(false);
                        }));

                eeStart.Log();
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI54168");
                ee.Log();
                // So service fails to start
                throw ee;
            }
        }

        private static async Task RunMeasurer(IExtractMeasure measurer, MeasureToLog logger, CancellationToken cancelToken)
        {
            // If PinThread, generate a new thread to be used exclusively for the provided measurer.
            // Otherwise, continue on current thread for now and let thread pool determine thread to
            // use for each loop (upon Task.Delay return).
            await (measurer.PinThread ? Task.Run(mainLoop, cancelToken) : mainLoop())
                .ConfigureAwait(false);

            async Task mainLoop()
            {
                while (true)
                {
                    // As opposed to checking IsCancellationRequested so TaskContinuationOptions.OnlyOnCanceled is observed
                    cancelToken.ThrowIfCancellationRequested();

                    // Start delay immediately so each measurements starts on MeasurementInterval
                    // regardless of how long measurement takes.
                    Task delay = Task.Delay(measurer.MeasurementInterval, cancelToken);

                    try
                    {
                        var data = measurer.Execute();
                        logger.RequestDataLogging(measurer, data);
                    }
                    catch (Exception ex) when (!cancelToken.IsCancellationRequested)
                    {
                        ex.AsExtract("ELI54040").Log();
                    }

                    if (delay.IsCompleted)
                    {
                        new ExtractException("ELI54186",
                            $"Measurement {measurer.MeasurementType} took longer to execute than its " +
                            $"configured interval of {TimeSpan.FromMilliseconds(measurer.MeasurementInterval)}")
                            .Log();
                    }

                    if (measurer.PinThread)
                    {
                        delay.ConfigureAwait(true).GetAwaiter().GetResult();
                    }
                    else
                    {
                        await delay.ConfigureAwait(false);
                    }
                }
            }
        }

        protected override void OnStop()
        {
            new ExtractException("ELI54170", "Application trace: Extract Environment Service stopping").Log();

            try
            {
                _measurerCancellationTokenSource.Cancel();
                _runningMeasurers
                    .ContinueWith(t => { },
                        default,
                        TaskContinuationOptions.OnlyOnCanceled,
                        TaskScheduler.Default)
                    .Wait(_SERVICE_STOP_MAX_WAIT);
            }
            catch (Exception ex)
            {
                new ExtractException("ELI54175", "Error stopping Extract Environment Service", ex).Log();
            }

            try
            {
                _loggerCancellationTokenSource.Cancel();
                _runningLogger
                    .ContinueWith(t => { },
                        default,
                        TaskContinuationOptions.OnlyOnCanceled,
                        TaskScheduler.Default)
                    .Wait(_SERVICE_STOP_MAX_WAIT);
                }
            catch (Exception ex)
            {
                new ExtractException("ELI54190", "Error stopping Extract Environment Service", ex).Log();
            }

            new ExtractException("ELI54191", "Application trace: Extract Environment Service stopped").Log();
        }
    }
}
